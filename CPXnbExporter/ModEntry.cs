using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace CPXnbExporter;
/// <summary>
/// CPXnbExporter 主入口
/// 多线程导出 - 仅导出当前语言
/// </summary>
public class ModEntry : Mod
{
    private ModConfig _config;
    private ExportOptions _currentOptions;
    private ExportPipeline _pipeline;

    // 资产加载状态
    private List<CpAssetLoader.CpAssetInfo> _assetList;
    private int _currentAssetIndex = -1;

    // 已导出资产名去重集合（用于地图 tilesheet 自动补全时避免重复导出）
    private HashSet<string> _exportedAssetNames;

    // CP 资产名称集合（规范化后）用于判断 tilesheet 是否被 CP 修改过
    private HashSet<string> _cpAssetNamesSet;

    // 导出阶段
    private enum ExportPhase { Idle, Loading, WaitingForWorkers, Finishing }
    private ExportPhase _phase = ExportPhase.Idle;

    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        _config.Validate(Monitor);

        CpAssetLoader.Init(helper, Monitor);

        helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

        helper.ConsoleCommands.Add("xnb_export",
            "导出单个资产: xnb_export <assetName> [mobile|pc] [unpacked]",
            OnExportSingleCommand);
        helper.ConsoleCommands.Add("xnb_export_all",
            "批量导出 CP 修改后的资产: xnb_export_all [mobile|pc] [unpacked]",
            OnExportBatchCommand);
        helper.ConsoleCommands.Add("xnb_status",
            "查看导出进度",
            OnStatusCommand);

        Monitor.Log($"后台线程: {_config.WorkerThreadCount}, 队列上限: {_config.MaxQueueSize}", LogLevel.Info);
    }

    #region Event Handlers

    private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
    {
        if (_config.AutoExportOnTitleScreen && _phase == ExportPhase.Idle)
        {
            Monitor.Log("进入标题画面，开始自动导出 CP 资产...", LogLevel.Info);
            var args = new List<string> { _config.AutoExportPlatform };
            if (_config.AutoExportUnpacked) args.Add("unpacked");
            StartExportCp(args.ToArray());
        }
    }

    /// <summary>
    /// 核心调度器：每帧加载 N 个资产并入队，后台线程池并行写入文件。
    /// </summary>
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (_phase == ExportPhase.Idle) return;

        // 1. 等待后台线程完成
        if (_phase == ExportPhase.WaitingForWorkers)
        {
            if (_pipeline.CheckAllWorkersCompleted())
            {
                _phase = ExportPhase.Finishing;
                FinishExport();
            }
            return;
        }

        // 2. 加载阶段：每帧处理 AssetsPerFrame 个资产
        if (_phase == ExportPhase.Loading)
        {
            int loadedThisFrame = 0;
            while (loadedThisFrame < _config.AssetsPerFrame
                && _currentAssetIndex + 1 < _assetList.Count)
            {
                _currentAssetIndex++;
                var asset = _assetList[_currentAssetIndex];

                // 加载并入队
                if (!LoadAndEnqueue(asset))
                {
                    // 队列满，回退索引，下一帧重试
                    _currentAssetIndex--;
                    return;
                }

                loadedThisFrame++;
            }

            // 检查是否全部加载完毕
            if (_currentAssetIndex + 1 >= _assetList.Count)
            {
                _pipeline.CompleteAdding();
                _phase = ExportPhase.WaitingForWorkers;
                Monitor.Log($"所有 {_assetList.Count} 个资产已加载入队，等待 {_config.WorkerThreadCount} 个后台线程完成写入...", LogLevel.Info);
            }
        }
    }

    #endregion

    #region Commands

    private void OnExportBatchCommand(string command, string[] args)
    {
        if (_phase != ExportPhase.Idle)
        {
            Monitor.Log("已有导出任务在进行中，输入 xnb_status 查看进度", LogLevel.Warn);
            return;
        }
        StartExportCp(args);
    }

    private void OnExportSingleCommand(string command, string[] args)
    {
        if (args.Length == 0)
        {
            Monitor.Log("用法: xnb_export <assetName> [mobile|pc] [unpacked]", LogLevel.Info);
            return;
        }

        string assetName = args[0];
        var options = ExportOptions.Parse(args.Skip(1).ToArray(),
            Path.Combine(Helper.DirectoryPath, "exported"));

        bool isMap = assetName.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase)
                  || assetName.StartsWith("maps/", StringComparison.OrdinalIgnoreCase);

        if (isMap)
            ExportMapSingle(assetName, options);
        else
            ExportTextureSingle(assetName, options);
    }

    private void OnStatusCommand(string command, string[] args)
    {
        if (_phase == ExportPhase.Idle)
        {
            Monitor.Log("当前无导出任务", LogLevel.Info);
            return;
        }

        int total = _assetList?.Count ?? 0;
        int current = _currentAssetIndex + 1;
        Monitor.Log($"导出状态: {_phase}", LogLevel.Info);
        Monitor.Log($"资产进度: {current}/{total} 已加载入队", LogLevel.Info);

        if (_pipeline != null)
        {
            Monitor.Log($"贴图: 成功 {_pipeline.TexSuccess}, 失败 {_pipeline.TexFail}", LogLevel.Info);
            Monitor.Log($"地图: 成功 {_pipeline.MapSuccess}, 失败 {_pipeline.MapFail}", LogLevel.Info);
        }
    }

    #endregion

    #region Export Orchestration

    private void StartExportCp(string[] args)
    {
        _phase = ExportPhase.Loading;
        _currentOptions = ExportOptions.Parse(args, Path.Combine(Helper.DirectoryPath, "exported"));
        _currentAssetIndex = -1;
        _exportedAssetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 创建后台写入管道
        _pipeline = new ExportPipeline(_config.WorkerThreadCount, _config.MaxQueueSize, Monitor);

        Monitor.Log($"[{_currentOptions.Platform}] {(_currentOptions.OutputUnpacked ? "packed + unpacked" : "仅 packed")}", LogLevel.Info);
        Monitor.Log("正在扫描 CP 资产...", LogLevel.Info);

        var allAssets = CpAssetLoader.LoadAllCpAssets();
        if (allAssets.Count == 0)
        {
            Monitor.Log("未找到任何可导出的 CP 资产", LogLevel.Info);
            _pipeline.CompleteAdding();
            _phase = ExportPhase.WaitingForWorkers;
            return;
        }

        _assetList = allAssets;

        // 缓存所有 CP 资产的规范化名称（用于自动补全判断）
        _cpAssetNamesSet = new HashSet<string>(
            allAssets.Select(a => NormalizeAssetPath(a.AssetName)),
            StringComparer.OrdinalIgnoreCase);

        Monitor.Log($"找到 {_assetList.Count} 个资产，开始加载（当前语言: {LocalizedContentManager.CurrentLanguageCode}）...", LogLevel.Info);
    }

    /// <summary>
    /// 加载单个资产并尝试入队。返回 false 表示队列满，需下一帧重试。
    /// </summary>
    private bool LoadAndEnqueue(CpAssetLoader.CpAssetInfo asset)
    {
        // rawAssetName: SMAPI 虚拟资产路径，用于 ContentManager 加载
        // normalizedAssetName: 规范化后的导出路径，用于文件写入和去重
        // 声明在 try 外，以便 catch 中的贴图回退能访问
        string rawAssetName = asset.AssetName;
        string normalizedAssetName = NormalizeAssetPath(rawAssetName);
        string safeName = GetExportAssetName(normalizedAssetName)
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        string packedBase = Path.Combine(_currentOptions.PackedDir, safeName);
        string unpackedBase = _currentOptions.OutputUnpacked ? Path.Combine(_currentOptions.UnpackedDir, safeName) : null;

        try
        {
            if (asset.AssetType == CpAssetLoader.CpAssetType.Texture)
            {
                // 用原始路径加载（SMAPI 虚拟资产路径），导出到规范化路径
                if (!EnqueueTexture(rawAssetName, packedBase, unpackedBase))
                    return false; // 队列满，回退索引重试
                _exportedAssetNames.Add(normalizedAssetName);
            }
            else // Map
            {
                Map map;
                string actualAssetName = normalizedAssetName;
                try { map = Helper.GameContent.Load<Map>(rawAssetName); }
                catch
                {
                    if (!rawAssetName.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase)
                        && !rawAssetName.StartsWith("maps/", StringComparison.OrdinalIgnoreCase))
                    {
                        actualAssetName = "Maps/" + rawAssetName;
                        map = Helper.GameContent.Load<Map>(actualAssetName);
                    }
                    else
                        throw;
                }

                // 自动整合寄生：把 SMAPI 虚拟 tilesheet 合并到宿主 tilesheet（默认 Maps/busPeople）。
                // 这绕过原版游戏的 ContentHashes.json 白名单限制，因为新增路径无法加载。
                string troubleshootDir = _currentOptions.OutputUnpacked ? _currentOptions.TroubleshootDir : null;
                if (troubleshootDir != null) System.IO.Directory.CreateDirectory(troubleshootDir);
                var mergedHostTexture = TileSheetMerger.MergeVirtualTileSheets(map, TileSheetMerger.DefaultHostAssetName, Helper, Monitor, troubleshootDir);
                if (mergedHostTexture != null)
                {
                    string hostNormalizedPath = TileSheetMerger.DefaultHostAssetName;
                    if (!_exportedAssetNames.Contains(hostNormalizedPath))
                    {
                        string hostSafeName = hostNormalizedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                        string hostPackedBase = Path.Combine(_currentOptions.PackedDir, hostSafeName);
                        string hostUnpackedBase = _currentOptions.OutputUnpacked ? Path.Combine(_currentOptions.UnpackedDir, hostSafeName) : null;

                        // 如果宿主贴图入队失败（队列满），回退当前地图资产，下一帧重试
                        if (!EnqueueTexture(mergedHostTexture, hostNormalizedPath, hostPackedBase, hostUnpackedBase))
                            return false;

                        _exportedAssetNames.Add(hostNormalizedPath);
                        Monitor.Log($"  ↳ 已合并虚拟 tilesheet 到 {hostNormalizedPath}", LogLevel.Trace);
                    }
                }

                // 设置当前地图 assetName，供 TBinWriter 计算 tilesheet 相对路径
                TBinWriter.MapAssetName = actualAssetName;
                byte[] tbinData = TBinWriter.SerializeTbin(map);

                var item = new ExportWorkItem
                {
                    Type = WorkItemType.Map,
                    FileName = rawAssetName,
                    PackedBasePath = packedBase,
                    UnpackedBasePath = unpackedBase,
                    Platform = _currentOptions.Platform,
                    TbinData = tbinData
                };

                if (!_pipeline.TryAdd(item))
                    return false;
                _exportedAssetNames.Add(normalizedAssetName);

                // 自动补全：仅导出被 CP 明确修改过的 tilesheet 贴图
                foreach (var tileSheet in map.TileSheets)
                {
                    string rawImageSource = tileSheet.ImageSource;
                    if (string.IsNullOrEmpty(rawImageSource)) continue;

                    // 跳过虚拟 tilesheet（已被合并）
                    if (TileSheetMerger.IsVirtualTileSheet(tileSheet)) continue;

                    string normalizedRaw = NormalizeAssetPath(rawImageSource);

                    // 如果该 tilesheet 不在 CP 资产列表中，说明是原版自带且未被修改，跳过
                    if (_cpAssetNamesSet == null || !_cpAssetNamesSet.Contains(normalizedRaw))
                        continue;

                    // 已经导出过也跳过
                    if (_exportedAssetNames.Contains(normalizedRaw)) continue;

                    string tsSafeName = normalizedRaw.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                    string tsPackedBase = Path.Combine(_currentOptions.PackedDir, tsSafeName);
                    string tsUnpackedBase = _currentOptions.OutputUnpacked ? Path.Combine(_currentOptions.UnpackedDir, tsSafeName) : null;

                    try
                    {
                        if (!EnqueueTexture(rawImageSource, tsPackedBase, tsUnpackedBase))
                        {
                            Monitor.Log($"  ↳ 自动补全 tilesheet 入队失败，本帧暂停 [{normalizedRaw}]", LogLevel.Trace);
                            return false;
                        }
                        _exportedAssetNames.Add(normalizedRaw);
                        Monitor.Log($"  ↳ 自动补全 tilesheet 贴图: {normalizedRaw} (来自地图 {rawAssetName})", LogLevel.Trace);
                    }
                    catch (Exception tex)
                    {
                        Monitor.Log($"  ↳ 自动补全 tilesheet 失败 [{normalizedRaw}]: {tex.Message}", LogLevel.Trace);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Map/Unknown 加载失败时，尝试作为贴图加载（兜底）
            // 这处理了贴图被误分类为 Unknown/Map 的情况——最常见的是 Maps/ 下的贴图
            // （如 Maps/springobjects）在检测阶段未被识别为贴图。
            if (asset.AssetType != CpAssetLoader.CpAssetType.Texture
                && asset.AssetType != CpAssetLoader.CpAssetType.Data)
            {
                try
                {
                    if (EnqueueTexture(rawAssetName, packedBase, unpackedBase))
                    {
                        _exportedAssetNames.Add(normalizedAssetName);
                        Monitor.Log($"  ↳ [{rawAssetName}] 作为贴图导出（地图加载失败的回退）", LogLevel.Trace);
                        return true;
                    }
                    return false; // 队列满，下一帧重试
                }
                catch (Exception texEx)
                {
                    Monitor.Log($"加载失败 [{asset.AssetName}]: 地图({ex.Message}) 贴图({texEx.Message})", LogLevel.Trace);
                    return true;
                }
            }
            Monitor.Log($"加载失败 [{asset.AssetName}]: {ex.Message}", LogLevel.Trace);
            return true; // 标记为已处理（失败），继续下一个
        }
    }

    /// <summary>
    /// 加载贴图并入队。返回 false 表示队列满，需下一帧重试。
    /// </summary>
    private bool EnqueueTexture(string assetName, string packedBase, string unpackedBase)
    {
        // 加载贴图（主线程，GPU 相关）
        var original = Helper.GameContent.Load<Texture2D>(assetName);
        return EnqueueTexture(original, assetName, packedBase, unpackedBase);
    }

    /// <summary>
    /// 把已加载的 Texture2D 入队。返回 false 表示队列满，需下一帧重试。
    /// </summary>
    private bool EnqueueTexture(Texture2D original, string fileName, string packedBase, string unpackedBase)
    {
        var pixels = new Color[original.Width * original.Height];
        original.GetData(pixels);

        // 检测原始像素是预乘还是 Straight Alpha（CP 的 Load 动作可能返回 Straight Alpha）
        bool isStraightAlpha = IsStraightAlpha(pixels);

        // 预生成 PNG 字节（PNG 格式要求 Straight Alpha）
        byte[] pngData = null;
        if (unpackedBase != null)
        {
            Color[] pngPixels;
            if (isStraightAlpha)
            {
                // 原始已经是 Straight Alpha，直接用
                pngPixels = (Color[])pixels.Clone();
            }
            else
            {
                // 原始是预乘 Alpha，需要反预乘还原为 Straight Alpha
                pngPixels = (Color[])pixels.Clone();
                UnpremultiplyAlpha(pngPixels);
            }
            using var pngTex = new Texture2D(original.GraphicsDevice, original.Width, original.Height);
            pngTex.SetData(pngPixels);
            using var pngMs = new MemoryStream();
            pngTex.SaveAsPng(pngMs, original.Width, original.Height);
            pngData = pngMs.ToArray();
        }

        // 如果原始是 Straight Alpha，预乘后存入 PixelData（XNB 要求预乘格式）
        // 无论原始格式如何，都做 Alpha Bleeding，防止移动端 block compression 污染边缘。
        Color[] xnbPixels;
        if (isStraightAlpha)
        {
            xnbPixels = (Color[])pixels.Clone();
            PremultiplyAlpha(xnbPixels); // 先预乘
            AlphaBleed(xnbPixels, original.Width, original.Height); // 再 bleeding
        }
        else
        {
            xnbPixels = (Color[])pixels.Clone();
            AlphaBleed(xnbPixels, original.Width, original.Height); // 填充 A=0 像素 RGB
        }

        var item = new ExportWorkItem
        {
            Type = WorkItemType.Texture,
            FileName = fileName,
            PackedBasePath = packedBase,
            UnpackedBasePath = unpackedBase,
            Platform = _currentOptions.Platform,
            PixelData = xnbPixels,
            PngData = pngData,
            Width = original.Width,
            Height = original.Height
        };

        return _pipeline.TryAdd(item);
    }

    /// <summary>
    /// 检测像素数组是否为 Straight Alpha（非预乘）格式。
    /// 预乘格式下 R ≤ A、G ≤ A、B ≤ A 恒成立；若任一像素违反则为 Straight Alpha。
    /// 注意：A=0 的像素 RGB 可能有残留（SMAPI 的 PremultiplyTransparency 跳过 A=0），
    /// 所以只检查半透明像素（0 &lt; A &lt; 255）来判断格式。
    /// </summary>
    private static bool IsStraightAlpha(Color[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            // 只检查半透明像素：A=0 的 RGB 残留不代表格式，A=255 时 R≤A 恒成立无法判断
            if (p.A > 0 && p.A < 255)
            {
                if (p.R > p.A || p.G > p.A || p.B > p.A)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 将 Straight Alpha 像素转换为预乘 Alpha（XNB SurfaceFormat.Color 要求）。
    /// 同时清零所有完全透明（A=0）像素的 RGB，防止线性过滤采样时边缘出现白线。
    /// （SMAPI 的 PremultiplyTransparency 跳过了 A=0 像素，导致 RGB 残留）
    /// </summary>
    private static void PremultiplyAlpha(Color[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int a = pixels[i].A;
            if (a == 0)
            {
                // 清零 RGB 残留：防止线性过滤采样透明边缘像素的 RGB
                pixels[i] = new Color(0, 0, 0, 0);
            }
            else if (a < 255)
            {
                pixels[i] = new Color(
                    (byte)(pixels[i].R * a / 255),
                    (byte)(pixels[i].G * a / 255),
                    (byte)(pixels[i].B * a / 255),
                    a);
            }
        }
    }

    /// <summary>
    /// 对完全透明（A=0）像素做 Alpha Bleeding：用最近的不透明像素颜色填充其 RGB。
    /// SMAPI 的 PremultiplyTransparency 跳过 A=0 像素，导致透明区域 RGB 残留（常为白色）。
    /// 在 iOS/Android 等移动端，GPU 会把纹理压缩为 PVRTC/ETC 等 block-based 格式，
    /// A=0 像素的 RGB 会污染相邻半透明边缘，形成白线/黑缝/透明缝隙。
    /// Alpha Bleeding 让透明像素的 RGB 与内容边缘一致，避免 block compression 污染。
    /// </summary>
    private static void AlphaBleed(Color[] pixels, int width, int height, int maxIterations = 32)
    {
        int count = width * height;
        var filled = new bool[count];
        for (int i = 0; i < count; i++)
            filled[i] = pixels[i].A > 0;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    if (filled[i]) continue;

                    int r = 0, g = 0, b = 0, n = 0;
                    if (x > 0 && filled[i - 1])
                    {
                        r += pixels[i - 1].R; g += pixels[i - 1].G; b += pixels[i - 1].B; n++;
                    }
                    if (x < width - 1 && filled[i + 1])
                    {
                        r += pixels[i + 1].R; g += pixels[i + 1].G; b += pixels[i + 1].B; n++;
                    }
                    if (y > 0 && filled[i - width])
                    {
                        r += pixels[i - width].R; g += pixels[i - width].G; b += pixels[i - width].B; n++;
                    }
                    if (y < height - 1 && filled[i + width])
                    {
                        r += pixels[i + width].R; g += pixels[i + width].G; b += pixels[i + width].B; n++;
                    }

                    if (n > 0)
                    {
                        pixels[i] = new Color((byte)(r / n), (byte)(g / n), (byte)(b / n), (byte)0);
                        filled[i] = true;
                        changed = true;
                    }
                }
            }
            if (!changed) break;
        }
    }

    /// <summary>
    /// 将预乘 Alpha 像素还原为 Straight Alpha。
    /// 完全透明(A=0)的像素 RGB 无法恢复，保持黑色（不影响渲染）。
    /// </summary>
    private static void UnpremultiplyAlpha(Color[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            if (p.A == 0)
            {
                pixels[i] = new Color(0, 0, 0, 0);
            }
            else if (p.A < 255)
            {
                float scale = 255f / p.A;
                pixels[i] = new Color(
                    (byte)Math.Min(255, p.R * scale),
                    (byte)Math.Min(255, p.G * scale),
                    (byte)Math.Min(255, p.B * scale),
                    p.A);
            }
        }
    }

    /// <summary>
    /// 资产名规范化：统一 SMAPI 虚拟路径的命名格式，用于内部去重和匹配。
    ///
    /// 仅用于 ModEntry 内部的 _exportedAssetNames 去重集合和 _cpAssetNamesSet 匹配，
    /// 不直接参与 tbin 写入（虚拟 tilesheet 已合并到宿主，tbin 中不再出现 SMAPI 路径）。
    ///
    /// SMAPI/模组ID/assets/文件夹/资源 → Maps/Mods/模组ID/文件夹/资源（去掉 assets 层）
    /// SMAPI/模组ID/文件夹/资源 → Maps/Mods/模组ID/文件夹/资源
    /// 非 SMAPI 路径原样返回。
    /// </summary>
    private static string NormalizeAssetPath(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return assetName;

        // 规范化路径分隔符：统一为正斜杠
        // 这对 _cpAssetNamesSet 匹配至关重要——CP 的 Target 可能用反斜杠（Maps\springobjects），
        // 而地图 tilesheet 的 ImageSource 用正斜杠（Maps/springobjects），
        // 不规范化会导致自动补全时匹配失败，tilesheet 贴图被跳过。
        assetName = assetName.Replace('\\', '/');

        if (assetName.StartsWith("SMAPI/", StringComparison.OrdinalIgnoreCase))
        {
            string rest = assetName.Substring("SMAPI/".Length);
            int firstSlash = rest.IndexOf('/');
            if (firstSlash >= 0)
            {
                string modId = rest.Substring(0, firstSlash);
                string afterModId = rest.Substring(firstSlash + 1);
                if (afterModId.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
                {
                    afterModId = afterModId.Substring("assets/".Length);
                }
                return "Maps/Mods/" + modId + "/" + afterModId;
            }
            return "Maps/Mods/" + rest;
        }
        return assetName;
    }

    private void FinishExport()
    {
        _phase = ExportPhase.Idle;

        // 读取统计
        long texS = _pipeline?.TexSuccess ?? 0;
        long texF = _pipeline?.TexFail ?? 0;
        long mapS = _pipeline?.MapSuccess ?? 0;
        long mapF = _pipeline?.MapFail ?? 0;

        // 清理管道
        _pipeline?.Dispose();
        _pipeline = null;
        _assetList = null;
        _currentAssetIndex = -1;
        _exportedAssetNames = null;
        _cpAssetNamesSet = null;
        TBinWriter.MapAssetName = null;

        Monitor.Log("\n==== 导出完成 ====", LogLevel.Info);
        Monitor.Log($"贴图: 成功 {texS}, 失败 {texF}", LogLevel.Info);
        Monitor.Log($"地图: 成功 {mapS}, 失败 {mapF}", LogLevel.Info);
        Monitor.Log($"输出目录: {_currentOptions.PackedDir}", LogLevel.Info);
        if (_currentOptions.OutputUnpacked)
            Monitor.Log($"Unpacked: {_currentOptions.UnpackedDir}", LogLevel.Info);
    }

    #endregion

    #region Single Asset Export (同步，不走多线程)

    private bool ExportTextureSingle(string assetName, ExportOptions options)
    {
        Texture2D tempCopy = null;
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = GetExportAssetName(normalizedName)
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            string packedBase = Path.Combine(options.PackedDir, safeName);
            string unpackedBase = options.OutputUnpacked ? Path.Combine(options.UnpackedDir, safeName) : null;
            Directory.CreateDirectory(Path.GetDirectoryName(packedBase));
            if (unpackedBase != null) Directory.CreateDirectory(Path.GetDirectoryName(unpackedBase));

            // 用原始路径加载（SMAPI 虚拟资产路径）
            Texture2D original = Helper.GameContent.Load<Texture2D>(assetName);
            tempCopy = CloneTexture(original);
            XnbWriter.ExportTextureSet(packedBase, unpackedBase, tempCopy, options.Platform);

            Monitor.Log($"✓ [T] {assetName} ({original.Width}x{original.Height})", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            Monitor.Log($"✗ [T] {assetName}: {ex.Message}", LogLevel.Warn);
            return false;
        }
        finally
        {
            tempCopy?.Dispose();
        }
    }

    private bool ExportMapSingle(string assetName, ExportOptions options)
    {
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = GetExportAssetName(normalizedName)
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            string packedPath = Path.Combine(options.PackedDir, safeName + ".xnb");
            Directory.CreateDirectory(Path.GetDirectoryName(packedPath));

            // 用原始路径加载（SMAPI 虚拟资产路径）
            Map map = Helper.GameContent.Load<Map>(assetName);
            TBinWriter.MapAssetName = normalizedName;
            using (var fs = new FileStream(packedPath, FileMode.Create, FileAccess.Write))
                TBinWriter.WriteMapXnb(fs, map, options.Platform);

            if (options.OutputUnpacked)
            {
                string unpackedPath = Path.Combine(options.UnpackedDir, safeName + ".tbin");
                Directory.CreateDirectory(Path.GetDirectoryName(unpackedPath));
                using (var fs = new FileStream(unpackedPath, FileMode.Create, FileAccess.Write))
                    TBinWriter.WriteMapTbin(fs, map);
            }

            Monitor.Log($"✓ [M] {assetName}", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            Monitor.Log($"✗ [M] {assetName}: {ex.Message}", LogLevel.Warn);
            return false;
        }
    }

    /// <summary>
    /// 获取导出用的资产名称。如果当前语言版本存在，则加上语言后缀。
    /// </summary>
    private string GetExportAssetName(string assetName)
    {
        string lang = LocalizedContentManager.CurrentLanguageString;
        if (string.IsNullOrEmpty(lang))
            return assetName;

        string localizedName = assetName + "." + lang;

        bool isMap = assetName.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase)
                  || assetName.StartsWith("maps/", StringComparison.OrdinalIgnoreCase);

        try
        {
            if (isMap)
                return Helper.GameContent.DoesAssetExist<Map>(Helper.GameContent.ParseAssetName(localizedName)) ? localizedName : assetName;
            else
                return Helper.GameContent.DoesAssetExist<Texture2D>(Helper.GameContent.ParseAssetName(localizedName)) ? localizedName : assetName;
        }
        catch
        {
            return assetName;
        }
    }

    private Texture2D CloneTexture(Texture2D source)
    {
        int width = source.Width;
        int height = source.Height;
        var data = new Color[width * height];
        source.GetData(data);

        var clone = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
        clone.SetData(data);
        return clone;
    }

    #endregion
}