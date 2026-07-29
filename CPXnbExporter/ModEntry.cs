using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.ContentManagers;
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
            "导出单个资产: xnb_export <assetName> [mobile|pc] [unpacked] [type]",
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
            Monitor.Log("用法: xnb_export <assetName> [mobile|pc] [unpacked] [type]", LogLevel.Info);
            Monitor.Log("可选 type: image (Texture2D), map (xTile.Map), 或完整 C# 类型名", LogLevel.Info);
            return;
        }

        string assetName = args[0];

        // 判断最后一个参数是否是类型名（非选项关键词）
        string last = args.Length > 1 ? args[^1].ToLowerInvariant() : null;
        bool lastIsOption = last is "pc" or "w" or "windows" or "mobile" or "a" or "android" or "i" or "ios" or "unpacked" or "u";
        string typeName = (args.Length > 1 && !lastIsOption) ? args[^1] : null;
        string[] optionArgs = args.Skip(1).ToArray();
        if (typeName != null)
            optionArgs = optionArgs.Take(optionArgs.Length - 1).ToArray();

        var typeCandidates = TryGetTypes(typeName);
        if (typeCandidates.Length == 0)
        {
            Monitor.Log($"找不到类型 '{typeName}'。输入 xnb_export 查看用法。", LogLevel.Error);
            return;
        }

        var options = ExportOptions.Parse(optionArgs, Path.Combine(Helper.DirectoryPath, "exported"));

        // 确定可能的类型列表
        var possibleTypes = new List<Type>(typeCandidates);
        if (possibleTypes[0] == typeof(object))
        {
            var likelyTypes = GetLikelyTypes(assetName);
            if (likelyTypes != null)
                possibleTypes.InsertRange(0, likelyTypes);
        }

        // 尝试加载资产（参考 ContentPatcher ExportCommand 的多类型加载模式）
        object asset = null;
        Dictionary<Type, Exception> exceptions = null;
        foreach (Type type in possibleTypes)
        {
            try
            {
                asset = this.LoadAsset(assetName, type);
                if (asset != null)
                    break;
            }
            catch (Exception ex) when (ex.InnerException is ContentLoadException or InvalidCastException)
            {
                exceptions ??= new Dictionary<Type, Exception>();
                exceptions[type] = ex.InnerException;
            }
        }

        if (asset == null)
        {
            Monitor.Log($"无法加载资产 '{assetName}'（尝试了: {string.Join(", ", possibleTypes.Select(p => p.FullName))}）。尝试指定类型，或等游戏加载后再导出。", LogLevel.Error);
            if (exceptions != null)
            {
                foreach ((Type type, Exception ex) in exceptions)
                    Monitor.Log($"  - 使用 '{type.FullName}' 失败: {ex.Message}", LogLevel.Trace);
            }
            return;
        }

        // 统一导出分发（参考 ContentPatcher TryExportRaw 模式）
        switch (asset)
        {
            case Map map:
                ExportMapSingle(assetName, map, options);
                break;
            case Texture2D texture:
                ExportTextureSingle(assetName, texture, options);
                break;
            case IRawTextureData rawTexture:
                ExportRawTextureSingle(assetName, rawTexture, options);
                break;
            default:
                ExportDataSingle(assetName, asset, options);
                break;
        }
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
            Monitor.Log($"数据: 成功 {_pipeline.DataSuccess}, 失败 {_pipeline.DataFail}", LogLevel.Info);
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
        string safeName = SanitizeAssetPath(GetExportAssetName(normalizedAssetName));
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
            else if (asset.AssetType == CpAssetLoader.CpAssetType.Map)
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
                        string hostSafeName = SanitizeAssetPath(hostNormalizedPath);
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

                    string tsSafeName = SanitizeAssetPath(normalizedRaw);
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
            else if (asset.AssetType == CpAssetLoader.CpAssetType.Data)
            {
                if (!EnqueueData(rawAssetName, packedBase))
                    return false;
                _exportedAssetNames.Add(normalizedAssetName);
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
    ///
    /// Alpha 处理（参考 SMAPI 加载机制和原版内容管道）：
    /// - SMAPI 通过 SKPMColor.PreMultiply 对 PNG 纹理预乘 Alpha
    /// - GetData() 返回的像素已是预乘格式
    /// - XNB 直接使用预乘像素（与原版 XNB 一致）
    /// - PNG 反预乘为非预乘像素（供 XnbConverter/MGCB 重新打包时正确预乘）
    /// - A=0 像素规范化为 Color.Transparent（匹配 SMAPI LoadRawImageData）
    /// - 不做边缘 padding / extrusion / alpha bleeding（SMAPI 和原版均不做）
    /// </summary>
    private bool EnqueueTexture(Texture2D original, string fileName, string packedBase, string unpackedBase)
    {
        var pixels = new Color[original.Width * original.Height];
        original.GetData(pixels);

        // Alpha 规范化：A=0 → Color.Transparent（匹配 SMAPI LoadRawImageData）
        XnbWriter.NormalizeAlpha(pixels);

        // XNB：直接使用预乘像素（与原版 XNB 一致）
        // PNG：反预乘为非预乘像素（供 XnbConverter/MGCB 重新打包时正确预乘）
        byte[] pngData = null;
        if (unpackedBase != null)
        {
            var pngPixels = XnbWriter.UnpremultiplyAlpha(pixels);
            using var pngTex = new Texture2D(original.GraphicsDevice, original.Width, original.Height);
            pngTex.SetData(pngPixels);
            using var pngMs = new MemoryStream();
            pngTex.SaveAsPng(pngMs, original.Width, original.Height);
            pngData = pngMs.ToArray();
        }

        var item = new ExportWorkItem
        {
            Type = WorkItemType.Texture,
            FileName = fileName,
            PackedBasePath = packedBase,
            UnpackedBasePath = unpackedBase,
            Platform = _currentOptions.Platform,
            PixelData = pixels,
            PngData = pngData,
            Width = original.Width,
            Height = original.Height
        };

        return _pipeline.TryAdd(item);
    }

    /// <summary>
    /// 把 IRawTextureData 入队。返回 false 表示队列满，需下一帧重试。
    /// IRawTextureData 的像素来自 SMAPI，已是预乘格式。
    /// XNB 直接使用预乘像素，PNG 反预乘为非预乘像素。
    /// </summary>
    private bool EnqueueTexture(IRawTextureData raw, string fileName, string packedBase, string unpackedBase)
    {
        // 复制数组：避免后台线程访问 SMAPI 内部数据时产生竞态
        var pixels = (Color[])raw.Data.Clone();
        int width = raw.Width;
        int height = raw.Height;

        // Alpha 规范化：A=0 → Color.Transparent（匹配 SMAPI LoadRawImageData）
        XnbWriter.NormalizeAlpha(pixels);

        byte[] pngData = null;
        if (unpackedBase != null)
        {
            var pngPixels = XnbWriter.UnpremultiplyAlpha(pixels);
            using var pngTex = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
            pngTex.SetData(pngPixels);
            using var pngMs = new MemoryStream();
            pngTex.SaveAsPng(pngMs, width, height);
            pngData = pngMs.ToArray();
        }

        var item = new ExportWorkItem
        {
            Type = WorkItemType.Texture,
            FileName = fileName,
            PackedBasePath = packedBase,
            UnpackedBasePath = unpackedBase,
            Platform = _currentOptions.Platform,
            PixelData = pixels,
            PngData = pngData,
            Width = width,
            Height = height
        };

        return _pipeline.TryAdd(item);
    }

    /// <summary>
    /// 加载数据资产并入队。返回 false 表示队列满，需下一帧重试。
    /// </summary>
    private bool EnqueueData(string assetName, string packedBase)
    {
        // 使用 ContentPatcher 风格的类型探测
        var likelyTypes = GetLikelyTypes(assetName);
        var possibleTypes = new List<Type> { typeof(object) };
        if (likelyTypes != null)
            possibleTypes.InsertRange(0, likelyTypes);

        object data = null;
        Dictionary<Type, Exception> exceptions = null;
        foreach (Type type in possibleTypes)
        {
            try
            {
                data = this.LoadAsset(assetName, type);
                if (data != null)
                    break;
            }
            catch (Exception ex) when (ex.InnerException is ContentLoadException or InvalidCastException)
            {
                exceptions ??= new Dictionary<Type, Exception>();
                exceptions[type] = ex.InnerException;
            }
        }

        if (data == null)
        {
            Monitor.Log($"无法加载数据资产 '{assetName}'（尝试了: {string.Join(", ", possibleTypes.Select(p => p.FullName))}）", LogLevel.Trace);
            return true; // 标记为已处理（失败），继续下一个
        }

        var item = new ExportWorkItem
        {
            Type = WorkItemType.Data,
            FileName = assetName,
            PackedBasePath = packedBase,
            UnpackedBasePath = null,
            Platform = _currentOptions.Platform,
            DataObject = data,
            DataTypeName = data.GetType().FullName
        };

        return _pipeline.TryAdd(item);
    }

    /// <summary>
    /// 资产名规范化：统一 SMAPI 虚拟路径的命名格式，用于内部去重和匹配。
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

    #region ContentPatcher-style Asset Loading & Type Detection

    /// <summary>Convert a full asset name like <c>Data/Buildings</c> into a filename-safe value like <c>Data_Buildings</c>.</summary>
    private static string GetSanitizedFileName(string assetName)
    {
        return string.Join("_", assetName.Split(Path.GetInvalidFileNameChars()));
    }

    /// <summary>Sanitize each path segment while preserving directory structure.</summary>
    private static string SanitizeAssetPath(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return assetName;
        string[] segments = assetName.Replace('\\', '/').Split('/');
        for (int i = 0; i < segments.Length; i++)
            segments[i] = GetSanitizedFileName(segments[i]);
        return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
    }

    /// <summary>Load an asset from a content manager using the given type. Uses temporary content manager if not cached to avoid corrupting the cache.</summary>
    private object LoadAsset(string assetName, Type type)
    {
        return this
            .GetType()
            .GetMethod(nameof(this.LoadAssetImpl), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(type)
            .Invoke(this, new object[] { assetName });
    }

    /// <summary>Load an asset from a content manager using the given type.</summary>
    private TAsset LoadAssetImpl<TAsset>(string assetName)
    {
        // get from main content manager if it's already cached
        if (this.IsAssetLoaded(Game1.content, assetName))
            return Game1.content.Load<TAsset>(assetName);

        // If it's not already cached, use a temporary content manager
        // This avoids corrupting the cache with an invalid type if it doesn't match.
        using ContentManager contentManager = Game1.content.CreateTemporary();
        return contentManager.Load<TAsset>(assetName);
    }

    /// <summary>Get whether the given content manager has already loaded and cached the given asset.</summary>
    private bool IsAssetLoaded(ContentManager contentManager, string assetName)
    {
        // IContentManager 是 SMAPI 内部接口，无法直接访问。
        // 用反射获取 IsLoaded 方法（接收 IAssetName 参数）。
        Type cmType = contentManager.GetType();
        MethodInfo isLoadedMethod = cmType.GetMethod("IsLoaded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (isLoadedMethod == null)
            return false;

        IAssetName parsedName = Helper.GameContent.ParseAssetName(assetName);
        try
        {
            return (bool)isLoadedMethod.Invoke(contentManager, new object[] { parsedName });
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Get the types matching a name, if any.</summary>
    private Type[] TryGetTypes(string name)
    {
        // none specified, default to object
        if (string.IsNullOrWhiteSpace(name))
            return new[] { typeof(object) };

        // short alias
        if (string.Equals(name, "image", StringComparison.OrdinalIgnoreCase))
            return new[] { typeof(Texture2D) };
        if (string.Equals(name, "map", StringComparison.OrdinalIgnoreCase))
            return new[] { typeof(Map) };

        // by assembly-qualified name
        {
            Type type = Type.GetType(name);
            if (type != null)
                return new[] { type };
        }

        // by type name
        {
            HashSet<Type> typesByName = new HashSet<Type>();
            HashSet<Type> typesByFullName = new HashSet<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                foreach (Type type in assembly.GetExportedTypes())
                {
                    try
                    {
                        if (string.Equals(type.FullName, name, StringComparison.OrdinalIgnoreCase))
                            typesByFullName.Add(type);
                        if (string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase))
                            typesByName.Add(type);
                    }
                    catch
                    {
                        // ignore invalid types
                    }
                }
            }

            HashSet<Type> matches = typesByFullName.Any()
                ? typesByFullName
                : typesByName;
            return matches.OrderBy(p => p.FullName, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }

    /// <summary>Try to get likely export types for an asset name.</summary>
    private List<Type> GetLikelyTypes(string asset)
    {
        IAssetName assetName = Helper.GameContent.ParseAssetName(asset);

        // based on path
        if (assetName.IsDirectlyUnderPath("Maps"))
            return new List<Type> { typeof(Map), typeof(Texture2D) };

        if (
            assetName.IsDirectlyUnderPath("Animals")
            || assetName.IsDirectlyUnderPath("Buildings")
            || assetName.IsDirectlyUnderPath("Characters")
            || assetName.IsDirectlyUnderPath("Portraits")
            || assetName.IsDirectlyUnderPath("Minigames")
            || assetName.IsDirectlyUnderPath("TerrainFeatures")
            || assetName.IsDirectlyUnderPath("TileSheets")
        )
            return new List<Type> { typeof(Texture2D) };

        if (
            assetName.IsDirectlyUnderPath("Characters/Dialogue")
            || assetName.IsDirectlyUnderPath("Characters/schedules")
            || assetName.IsDirectlyUnderPath("Data/Events")
            || assetName.IsDirectlyUnderPath("Data/Festivals")
        )
            return new List<Type> { typeof(Dictionary<string, string>) };

        // based on DataLoader method
        if (assetName.IsDirectlyUnderPath("Data"))
        {
            string name = assetName.BaseName["Data/".Length..];
            if (name.Contains('_'))
                return null; // no vanilla data asset has `_` in its name, but DataLoader uses it for subfolders like 'Festivals_FestivalDates'

            MethodInfo method = typeof(DataLoader).GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (method != null)
                return new List<Type> { method.ReturnType };
        }

        return null;
    }

    #endregion

    private void FinishExport()
    {
        _phase = ExportPhase.Idle;

        // 读取统计
        long texS = _pipeline?.TexSuccess ?? 0;
        long texF = _pipeline?.TexFail ?? 0;
        long mapS = _pipeline?.MapSuccess ?? 0;
        long mapF = _pipeline?.MapFail ?? 0;
        long dataS = _pipeline?.DataSuccess ?? 0;
        long dataF = _pipeline?.DataFail ?? 0;

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
        Monitor.Log($"数据: 成功 {dataS}, 失败 {dataF}", LogLevel.Info);
        Monitor.Log($"输出目录: {_currentOptions.PackedDir}", LogLevel.Info);
        if (_currentOptions.OutputUnpacked)
            Monitor.Log($"Unpacked: {_currentOptions.UnpackedDir}", LogLevel.Info);
    }

    #endregion

    #region Single Asset Export (同步，不走多线程)

    private bool ExportTextureSingle(string assetName, Texture2D original, ExportOptions options)
    {
        Texture2D tempCopy = null;
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = SanitizeAssetPath(GetExportAssetName(normalizedName));
            string packedBase = Path.Combine(options.PackedDir, safeName);
            string unpackedBase = options.OutputUnpacked ? Path.Combine(options.UnpackedDir, safeName) : null;
            Directory.CreateDirectory(Path.GetDirectoryName(packedBase));
            if (unpackedBase != null) Directory.CreateDirectory(Path.GetDirectoryName(unpackedBase));

            tempCopy = CloneTexture(original);
            // ExportTextureSet 内部执行 Alpha 规范化和 PNG 反预乘
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

    private bool ExportRawTextureSingle(string assetName, IRawTextureData raw, ExportOptions options)
    {
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = SanitizeAssetPath(GetExportAssetName(normalizedName));
            string packedBase = Path.Combine(options.PackedDir, safeName);
            string unpackedBase = options.OutputUnpacked ? Path.Combine(options.UnpackedDir, safeName) : null;
            Directory.CreateDirectory(Path.GetDirectoryName(packedBase));
            if (unpackedBase != null) Directory.CreateDirectory(Path.GetDirectoryName(unpackedBase));

            using var texture = new Texture2D(Game1.graphics.GraphicsDevice, raw.Width, raw.Height);
            texture.SetData(raw.Data);
            // ExportTextureSet 内部执行 Alpha 规范化和 PNG 反预乘
            XnbWriter.ExportTextureSet(packedBase, unpackedBase, texture, options.Platform);

            Monitor.Log($"✓ [R] {assetName} ({raw.Width}x{raw.Height})", LogLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            Monitor.Log($"✗ [R] {assetName}: {ex.Message}", LogLevel.Warn);
            return false;
        }
    }

    private bool ExportMapSingle(string assetName, Map map, ExportOptions options)
    {
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = SanitizeAssetPath(GetExportAssetName(normalizedName));
            string packedPath = Path.Combine(options.PackedDir, safeName + ".xnb");
            Directory.CreateDirectory(Path.GetDirectoryName(packedPath));

            // 设置当前地图 assetName，供 TBinWriter 计算 tilesheet 相对路径
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

    private bool ExportDataSingle(string assetName, object data, ExportOptions options)
    {
        try
        {
            string normalizedName = NormalizeAssetPath(assetName);
            string safeName = SanitizeAssetPath(GetExportAssetName(normalizedName));
            string packedBase = Path.Combine(options.PackedDir, safeName);
            Directory.CreateDirectory(Path.GetDirectoryName(packedBase));

            // DataExporter.ExportData 会自动追加 .json
            if (DataExporter.ExportData(packedBase, data, assetName))
            {
                Monitor.Log($"✓ [D] {assetName} ({data.GetType().Name})", LogLevel.Info);
                return true;
            }
            Monitor.Log($"✗ [D] {assetName}: JSON 序列化失败", LogLevel.Warn);
            return false;
        }
        catch (Exception ex)
        {
            Monitor.Log($"✗ [D] {assetName}: {ex.Message}", LogLevel.Warn);
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