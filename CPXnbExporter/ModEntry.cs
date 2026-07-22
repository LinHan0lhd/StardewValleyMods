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

namespace CPXnbExporter
{
    /// <summary>
    /// CPXnbExporter 主入口。v2.1.1：多线程导出，仅导出当前语言。
    /// 架构：主线程加载资产 → 入队 → 后台线程池写入文件。
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

        // 导出阶段
        private enum ExportPhase { Idle, Loading, WaitingForWorkers, Finishing }
        private ExportPhase _phase = ExportPhase.Idle;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _config.Validate(Monitor);

            CpAssetLoader.Init(helper, Monitor);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
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

            Monitor.Log($"CPXnbExporter 精简版已加载。后台线程: {_config.WorkerThreadCount}, 队列上限: {_config.MaxQueueSize}", LogLevel.Info);
        }

        #region Event Handlers

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Monitor.Log("CPXnbExporter: 游戏已启动，准备就绪", LogLevel.Info);
        }

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

            // 设置路径规范化：SMAPI 虚拟路径 → Mods/模组ID/... 路径
            TBinWriter.PathNormalizer = NormalizeAssetPath;

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
            Monitor.Log($"找到 {_assetList.Count} 个资产，开始加载（当前语言: {LocalizedContentManager.CurrentLanguageCode}）...", LogLevel.Info);
        }

        /// <summary>
        /// 加载单个资产并尝试入队。返回 false 表示队列满，需下一帧重试。
        /// </summary>
        private bool LoadAndEnqueue(CpAssetLoader.CpAssetInfo asset)
        {
            try
            {
                string assetName = asset.AssetName;
                string safeName = GetExportAssetName(assetName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                string packedBase = Path.Combine(_currentOptions.PackedDir, safeName);
                string unpackedBase = _currentOptions.OutputUnpacked ? Path.Combine(_currentOptions.UnpackedDir, safeName) : null;

                if (asset.AssetType == CpAssetLoader.CpAssetType.Texture)
                {
                    if (!EnqueueTexture(assetName, packedBase, unpackedBase))
                        return false; // 队列满，回退索引重试
                    _exportedAssetNames.Add(assetName);
                }
                else // Map
                {
                    // 总是通过 Map 对象中转，确保生成正确的 tbin 格式
                    // CP 包的 .tmx 是 XML 文本格式，不能直接当 tbin 二进制写入 XNB
                    Map map;
                    string actualAssetName = assetName;
                    try { map = Helper.GameContent.Load<Map>(assetName); }
                    catch
                    {
                        if (!assetName.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase))
                        {
                            actualAssetName = "Maps/" + assetName;
                            map = Helper.GameContent.Load<Map>(actualAssetName);
                        }
                        else
                            throw;
                    }
                    // 设置当前地图 assetName，供 TBinWriter 计算 tilesheet 相对路径
                    TBinWriter.MapAssetName = actualAssetName;
                    byte[] tbinData = TBinWriter.SerializeTbin(map);

                    var item = new ExportWorkItem
                    {
                        Type = WorkItemType.Map,
                        FileName = assetName,
                        PackedBasePath = packedBase,
                        UnpackedBasePath = unpackedBase,
                        Platform = _currentOptions.Platform,
                        TbinData = tbinData
                    };

                    if (!_pipeline.TryAdd(item))
                        return false;
                    _exportedAssetNames.Add(assetName);

                    // 自动补全：扫描地图引用的所有 tilesheet，导出未在列表中的贴图
                    // 这解决了 CP 包只声明 EditMap 而未声明 Load tilesheet 导致的闪退问题
                    foreach (var tileSheet in map.TileSheets)
                    {
                        string rawImageSource = tileSheet.ImageSource;
                        if (string.IsNullOrEmpty(rawImageSource)) continue;

                        // 移除扩展名
                        string ext = System.IO.Path.GetExtension(rawImageSource);
                        if (!string.IsNullOrEmpty(ext) &&
                            (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                             ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                             ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                             ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                             ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)))
                        {
                            rawImageSource = rawImageSource.Substring(0, rawImageSource.Length - ext.Length);
                        }

                        // 映射路径用于导出和去重（与 TBinWriter 写入 TBIN 的路径一致）
                        string normalizedPath = NormalizeAssetPath(rawImageSource);

                        if (_exportedAssetNames.Contains(normalizedPath)) continue;

                        string tsSafeName = normalizedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                        string tsPackedBase = Path.Combine(_currentOptions.PackedDir, tsSafeName);
                        string tsUnpackedBase = _currentOptions.OutputUnpacked ? Path.Combine(_currentOptions.UnpackedDir, tsSafeName) : null;

                        try
                        {
                            // 加载用原始路径（SMAPI 注册的虚拟资产路径）
                            // 导出路径用映射后的路径（Mods/模组ID/...）
                            if (EnqueueTexture(rawImageSource, tsPackedBase, tsUnpackedBase))
                            {
                                _exportedAssetNames.Add(normalizedPath);
                                Monitor.Log($"  ↳ 自动补全 tilesheet 贴图: {normalizedPath} (来自地图 {assetName})", LogLevel.Trace);
                            }
                        }
                        catch (Exception tex)
                        {
                            Monitor.Log($"  ↳ 自动补全 tilesheet 失败 [{normalizedPath}]: {tex.Message}", LogLevel.Trace);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
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
            var pixels = new Color[original.Width * original.Height];
            original.GetData(pixels);

            // 重要：原版 XNB 贴图是预乘 Alpha 格式（XNA/MonoGame Content Pipeline 默认 PremultiplyAlpha=true）。
            // SMAPI 通过 Texture2D.FromStream 加载 PNG 时也会做预乘 Alpha 转换。
            // 所以无论资产来源（原版 XNB 还是 SMAPI 虚拟路径 PNG），GetData 拿到的都是预乘 Alpha 像素。
            // 直接存入 XNB 即可，游戏加载时会正确渲染。
            //
            // 之前的错误做法：对 SMAPI 资产做 UnpremultiplyAlpha 还原，导致 XNB 存的是 Straight Alpha，
            // 游戏用预乘 Alpha 的 blend mode 渲染时会过亮（半透明背景变白）。
            //
            // PNG 预览需要还原为 Straight Alpha（PNG 格式标准），XNB 保持预乘 Alpha（游戏期望的格式）。

            // 预生成 PNG 字节（还原为 Straight Alpha，确保 unpacked 预览正确）
            byte[] pngData = null;
            if (unpackedBase != null)
            {
                Color[] pngPixels = (Color[])pixels.Clone();
                UnpremultiplyAlpha(pngPixels);
                using var pngTex = new Texture2D(original.GraphicsDevice, original.Width, original.Height);
                pngTex.SetData(pngPixels);
                using var pngMs = new MemoryStream();
                pngTex.SaveAsPng(pngMs, original.Width, original.Height);
                pngData = pngMs.ToArray();
            }

            var item = new ExportWorkItem
            {
                Type = WorkItemType.Texture,
                FileName = assetName,
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
        /// 路径规范化：把 SMAPI 虚拟资产路径映射为游戏 Content 可识别的路径。
        ///
        /// 重要：原版游戏（无 SMAPI）不支持 ../ 跨目录路径，tilesheet 必须在 Content/Maps/ 内。
        /// 所以虚拟资产映射到 Maps/文件夹/模组ID_资源 路径（模组ID编码到文件名避免冲突）。
        /// 这样游戏 eager prefixing 会正确解析：
        ///   ImageSource "glasses/xxx_z_glass" → 游戏 prefixing → "Maps/glasses/xxx_z_glass" → Content/Maps/glasses/xxx_z_glass.xnb
        ///
        /// SMAPI/模组ID/assets/glasses/z_glass.png → Maps/glasses/模组ID_z_glass.png（模组ID编码到文件名，避免冲突）
        /// SMAPI/模组ID/文件夹/资源 → Maps/文件夹/模组ID_资源
        /// 非 SMAPI 路径原样返回。
        /// </summary>
        private static string NormalizeAssetPath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return assetName;

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
                    // 扁平化唯一文件名：模组ID_原路径，用_替换所有分隔符
                    // SMAPI/nekotekina.../assets/glasses/z_glass.png → Mods/nekotekina..._glasses_z_glass.png
                    string safeName = (modId + "_" + afterModId)
                        .Replace('/', '_').Replace('\\', '_').Replace('.', '_');
                    return "Mods/" + safeName;
                }
                return "Mods/" + rest;
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
            TBinWriter.PathNormalizer = null;
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
                string safeName = GetExportAssetName(assetName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                string packedBase = Path.Combine(options.PackedDir, safeName);
                string unpackedBase = options.OutputUnpacked ? Path.Combine(options.UnpackedDir, safeName) : null;
                Directory.CreateDirectory(Path.GetDirectoryName(packedBase));
                if (unpackedBase != null) Directory.CreateDirectory(Path.GetDirectoryName(unpackedBase));

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
                string safeName = GetExportAssetName(assetName).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                string packedPath = Path.Combine(options.PackedDir, safeName + ".xnb");
                Directory.CreateDirectory(Path.GetDirectoryName(packedPath));

                Map map = Helper.GameContent.Load<Map>(assetName);
                TBinWriter.MapAssetName = assetName;
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

        /// <summary>创建 Texture2D 的独立副本（不共享 GPU 资源引用）</summary>
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
}
