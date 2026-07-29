using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace CPXnbExporter
{
    /// <summary>
    /// 导出管道：管理有界任务队列和后台写入线程池。
    /// 主线程作为生产者加载资产并入队，后台线程作为消费者写入文件。
    /// </summary>
    public class ExportPipeline : IDisposable
    {
        private readonly BlockingCollection<ExportWorkItem> _queue;
        private readonly Task[] _workers;
        private readonly CancellationTokenSource _cts;
        private readonly IMonitor _monitor;
        private readonly int _workerCount;

        // 线程安全统计
        private long _texSuccess;
        private long _texFail;
        private long _mapSuccess;
        private long _mapFail;
        private long _dataSuccess;
        private long _dataFail;

        public long TexSuccess => Interlocked.Read(ref _texSuccess);
        public long TexFail => Interlocked.Read(ref _texFail);
        public long MapSuccess => Interlocked.Read(ref _mapSuccess);
        public long MapFail => Interlocked.Read(ref _mapFail);
        public long DataSuccess => Interlocked.Read(ref _dataSuccess);
        public long DataFail => Interlocked.Read(ref _dataFail);

        public bool IsAddingCompleted => _queue.IsAddingCompleted;
        public bool IsCompleted => _queue.IsCompleted;

        public ExportPipeline(int workerCount, int maxQueueSize, IMonitor monitor)
        {
            _workerCount = workerCount;
            _monitor = monitor;
            _queue = new BlockingCollection<ExportWorkItem>(maxQueueSize);
            _cts = new CancellationTokenSource();
            _workers = new Task[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                int id = i;
                _workers[i] = Task.Run(() => WorkerLoop(id), _cts.Token);
            }

            monitor?.Log($"导出管道已启动: {workerCount} 个后台写入线程，队列上限 {maxQueueSize}", LogLevel.Info);
        }

        /// <summary>尝试将工作项加入队列（非阻塞）</summary>
        public bool TryAdd(ExportWorkItem item)
        {
            if (_queue.IsAddingCompleted)
                return false;

            // 使用 TryAdd 避免阻塞主线程；队列满时返回 false，由调用方下一帧重试
            return _queue.TryAdd(item, TimeSpan.FromMilliseconds(1));
        }

        /// <summary>标记不再添加新任务</summary>
        public void CompleteAdding()
        {
            if (!_queue.IsAddingCompleted)
            {
                _queue.CompleteAdding();
                _monitor?.Log("任务队列已关闭，等待后台线程完成写入...", LogLevel.Info);
            }
        }

        /// <summary>检查所有后台线程是否已完成</summary>
        public bool CheckAllWorkersCompleted()
        {
            return _queue.IsCompleted && _workers.All(w => w.IsCompleted);
        }

        /// <summary>等待所有后台线程完成（阻塞，仅用于清理）</summary>
        public void WaitForCompletion(TimeSpan timeout)
        {
            try { Task.WaitAll(_workers, timeout); }
            catch { /* ignore */ }
        }

        private void WorkerLoop(int workerId)
        {
            _monitor?.Log($"后台写入线程 #{workerId} 已启动", LogLevel.Trace);
            try
            {
                // GetConsumingEnumerable 会在 CompleteAdding 且队列空后自动退出
                foreach (var item in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    ProcessItem(item);
                }
            }
            catch (OperationCanceledException)
            {
                _monitor?.Log($"后台写入线程 #{workerId} 已取消", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"后台写入线程 #{workerId} 异常: {ex.Message}", LogLevel.Error);
            }
            _monitor?.Log($"后台写入线程 #{workerId} 已结束", LogLevel.Trace);
        }

        private void ProcessItem(ExportWorkItem item)
        {
            try
            {
                switch (item.Type)
                {
                    case WorkItemType.Texture:
                        ProcessTexture(item);
                        break;
                    case WorkItemType.Map:
                        ProcessMap(item);
                        break;
                    case WorkItemType.Data:
                        ProcessData(item);
                        break;
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"写入失败 [{item.FileName}]: {ex.Message}", LogLevel.Trace);
                switch (item.Type)
                {
                    case WorkItemType.Texture:
                        Interlocked.Increment(ref _texFail);
                        break;
                    case WorkItemType.Map:
                        Interlocked.Increment(ref _mapFail);
                        break;
                    case WorkItemType.Data:
                        Interlocked.Increment(ref _dataFail);
                        break;
                }
            }
        }

        private void ProcessTexture(ExportWorkItem item)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(item.PackedBasePath));

            // 1. XNB 写入（纯 CPU，无需 GPU）
            using (var fs = new FileStream(item.PackedBasePath + ".xnb", FileMode.Create, FileAccess.Write))
            {
                XnbWriter.WriteXnbBinaryFromPixels(
                    fs, item.PixelData, item.Width, item.Height, item.Platform);
            }

            // 2. Unpacked 输出
            if (item.UnpackedBasePath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item.UnpackedBasePath));

                // PNG（主线程已预生成字节数组）
                if (item.PngData != null)
                    File.WriteAllBytes(item.UnpackedBasePath + ".png", item.PngData);

                // .config (JSON format compatible with XnbConverter)
                WriteConfig(item.UnpackedBasePath + ".config", item.Platform, ".png", "Microsoft.Xna.Framework.Content.Texture2DReader");
            }

            Interlocked.Increment(ref _texSuccess);
        }

        private void ProcessMap(ExportWorkItem item)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(item.PackedBasePath));

            // 1. XNB 写入
            using (var fs = new FileStream(item.PackedBasePath + ".xnb", FileMode.Create, FileAccess.Write))
            {
                XnbMapWriter.WriteMapXnbFromTbin(fs, item.TbinData, item.Platform);
            }

            // 2. TBIN + Config 写入
            if (item.UnpackedBasePath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item.UnpackedBasePath));
                File.WriteAllBytes(item.UnpackedBasePath + ".tbin", item.TbinData);

                // .config (JSON format compatible with XnbConverter)
                WriteConfig(item.UnpackedBasePath + ".config", item.Platform, ".tbin", "xTile.Pipeline.TideReader, xTile");
            }

            Interlocked.Increment(ref _mapSuccess);
        }

        private void ProcessData(ExportWorkItem item)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(item.PackedBasePath));
            // DataExporter.ExportData 会自动追加 .json
            if (DataExporter.ExportData(item.PackedBasePath, item.DataObject, item.FileName))
                Interlocked.Increment(ref _dataSuccess);
            else
                Interlocked.Increment(ref _dataFail);
        }

                private static void WriteConfig(string path, char platform, string extension, string readerType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"Content\": {");
            sb.AppendLine("    \"Extension\": \"" + extension + "\",");
            sb.AppendLine("    \"Format\": 0");
            sb.AppendLine("  },");
            sb.AppendLine("  \"Header\": {");
            sb.AppendLine("    \"Target\": \"" + GetPlatformName(platform) + "\",");
            sb.AppendLine("    \"FormatVersion\": 5,");
            sb.AppendLine("    \"CompressedFlag\": \"" + (platform == 'a' ? "Lz4" : "None") + "\"");
            sb.AppendLine("  },");
            sb.AppendLine("  \"Readers\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"Type\": \"" + readerType + "\",");
            sb.AppendLine("      \"Version\": 0");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

private static string GetPlatformName(char platform)
        {
            return platform switch
            {
                'a' => "Android",
                'i' => "iOS",
                'w' => "Windows",
                'm' => "WindowsPhone7",
                'x' => "Xbox360",
                'l' => "Linux",
                'X' => "MacOSX",
                _ => "Windows"
            };
        }

        public void Dispose()
        {
            _cts.Cancel();
            WaitForCompletion(TimeSpan.FromSeconds(5));
            _queue.Dispose();
            _cts.Dispose();
        }
    }
}
