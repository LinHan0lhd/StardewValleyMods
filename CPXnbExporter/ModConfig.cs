using StardewModdingAPI;

namespace CPXnbExporter
{
    /// <summary>模组配置</summary>
    public class ModConfig
    {
        /// <summary>是否在返回标题画面时自动导出</summary>
        public bool AutoExportOnTitleScreen { get; set; } = false;

        /// <summary>自动导出平台: "mobile" (a) 或 "pc" (w)</summary>
        public string AutoExportPlatform { get; set; } = "mobile";

        /// <summary>自动导出时是否同时输出 unpacked</summary>
        public bool AutoExportUnpacked { get; set; } = false;

        /// <summary>后台写入线程数（建议等于CPU逻辑核心数）</summary>
        public int WorkerThreadCount { get; set; } = 4;

        /// <summary>任务队列最大长度（防止内存溢出）</summary>
        public int MaxQueueSize { get; set; } = 32;

        /// <summary>每帧加载的最大资产数（建议保持1避免卡顿）</summary>
        public int AssetsPerFrame { get; set; } = 1;

        /// <summary>验证配置有效性</summary>
        public void Validate(IMonitor monitor)
        {
            if (WorkerThreadCount < 1)
            {
                monitor.Log("配置 WorkerThreadCount 过小，已重置为 1", LogLevel.Warn);
                WorkerThreadCount = 1;
            }
            if (WorkerThreadCount > 16)
            {
                monitor.Log("配置 WorkerThreadCount 过大，已限制为 16", LogLevel.Warn);
                WorkerThreadCount = 16;
            }
            if (MaxQueueSize < 4)
            {
                monitor.Log("配置 MaxQueueSize 过小，已重置为 4", LogLevel.Warn);
                MaxQueueSize = 4;
            }
            if (AssetsPerFrame < 1)
            {
                AssetsPerFrame = 1;
            }
        }
    }
}
