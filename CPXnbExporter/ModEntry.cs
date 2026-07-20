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
                    // 加载贴图（主线程，GPU 相关）
                    var original = Helper.GameContent.Load<Texture2D>(assetName);
                    var pixels = new Color[original.Width * original.Height];
                    original.GetData(pixels);

                    // 预生成 PNG 字节（SaveAsPng 需要 GPU，必须在主线程）
                    byte[] pngData = null;
                    if (unpackedBase != null)
                    {
                        using var pngMs = new MemoryStream();
                        original.SaveAsPng(pngMs, original.Width, original.Height);
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

                    if (!_pipeline.TryAdd(item))
                        return false; // 队列满，回退索引重试
                }
                else // Map
                {
                    // 总是通过 Map 对象中转，确保生成正确的 tbin 格式
                    // CP 包的 .tmx 是 XML 文本格式，不能直接当 tbin 二进制写入 XNB
                    Map map;
                    try { map = Helper.GameContent.Load<Map>(assetName); }
                    catch
                    {
                        if (!assetName.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase))
                            map = Helper.GameContent.Load<Map>("Maps/" + assetName);
                        else
                            throw;
                    }
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
                }

                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"加载失败 [{asset.AssetName}]: {ex.Message}", LogLevel.Trace);
                return true; // 标记为已处理（失败），继续下一个
            }
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
