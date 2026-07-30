using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace CPXnbExporter;
public class ExportPipeline : IDisposable
{
    readonly BlockingCollection<ExportWorkItem> _q;
    readonly Task[] _w;
    readonly CancellationTokenSource _cts;
    readonly IMonitor _m;
    long _ts, _tf, _ms, _mf, _ds, _df;
    public long TexSuccess => Interlocked.Read(ref _ts); public long TexFail => Interlocked.Read(ref _tf);
    public long MapSuccess => Interlocked.Read(ref _ms); public long MapFail => Interlocked.Read(ref _mf);
    public long DataSuccess => Interlocked.Read(ref _ds); public long DataFail => Interlocked.Read(ref _df);
    public bool IsAddingCompleted => _q.IsAddingCompleted; public bool IsCompleted => _q.IsCompleted;

    public ExportPipeline(int n, int cap, IMonitor m)
    {
        _m = m; _q = new BlockingCollection<ExportWorkItem>(cap); _cts = new CancellationTokenSource(); _w = new Task[n];
        for (int i = 0; i < n; i++) { int id = i; _w[i] = Task.Run(() => Loop(id), _cts.Token); }
    }
    public bool TryAdd(ExportWorkItem item) => !_q.IsAddingCompleted && _q.TryAdd(item, TimeSpan.FromMilliseconds(1));
    public void CompleteAdding() { if (!_q.IsAddingCompleted) { _q.CompleteAdding(); _m?.Log("队列关闭", LogLevel.Info); } }
    public bool CheckAllWorkersCompleted() => _q.IsCompleted && _w.All(x => x.IsCompleted);
    public void Dispose() { if (!_cts.IsCancellationRequested) _cts.Cancel(); try { Task.WaitAll(_w, TimeSpan.FromSeconds(30)); } catch { } _cts.Dispose(); _q.Dispose(); }

    void Loop(int id)
    {
        foreach (var item in _q.GetConsumingEnumerable(_cts.Token))
        {
            try { switch (item.Type) { case WorkItemType.Texture: DoTex(item); break; case WorkItemType.Map: DoMap(item); break; case WorkItemType.Data: DoData(item); break; } }
            catch (Exception ex) { _m?.Log($"W{id} err: {ex.Message}", LogLevel.Error); }
        }
    }

    void DoTex(ExportWorkItem i)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(i.PackedBasePath));
            if (i.UnpackedBasePath != null) Directory.CreateDirectory(Path.GetDirectoryName(i.UnpackedBasePath));
            XnbWriter.WriteTextureXnb(i.PackedBasePath + ".xnb", i.PixelData, i.Width, i.Height, i.Platform);
            if (i.PngData != null)
            {
                string pngPath = i.UnpackedBasePath + ".png";
                File.WriteAllBytes(pngPath, i.PngData);
                long fsz = new FileInfo(pngPath).Length;
                var meta = new XnbMetadata { Width = i.Width, Height = i.Height, Format = 0, FormatName = "Color", MipCount = 1, Platform = i.Platform.ToString(), Version = 5, Compressed = i.Platform == 'a', FileSize = fsz };
                File.WriteAllText(i.UnpackedBasePath + ".config", meta.ToConfig(), Encoding.UTF8);
            }
            Interlocked.Increment(ref _ts);
        }
        catch (Exception ex) { _m?.Log($"✗ 写入纹理失败 {i.FileName}: {ex.Message}", LogLevel.Error); Interlocked.Increment(ref _tf); }
    }

    void DoMap(ExportWorkItem i)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(i.PackedBasePath));
            using (var fs = new FileStream(i.PackedBasePath + ".xnb", FileMode.Create, FileAccess.Write)) XnbMapWriter.WriteMapXnbFromTbin(fs, i.TbinData, i.Platform);
            if (i.UnpackedBasePath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(i.UnpackedBasePath));
                string tbinPath = i.UnpackedBasePath + ".tbin";
                File.WriteAllBytes(tbinPath, i.TbinData);
                long fsz = new FileInfo(tbinPath).Length;
                File.WriteAllText(i.UnpackedBasePath + ".config", XnbMetadata.MapConfig(i.Platform, fsz), Encoding.UTF8);
            }
            Interlocked.Increment(ref _ms);
        }
        catch (Exception ex) { _m?.Log($"✗ 写入地图失败 {i.FileName}: {ex.Message}", LogLevel.Error); Interlocked.Increment(ref _mf); }
    }

    void DoData(ExportWorkItem i)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(i.PackedBasePath));
            DataExporter.ExportData(i.PackedBasePath, i.DataObject, i.FileName);
            if (i.UnpackedBasePath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(i.UnpackedBasePath));
                string src = i.PackedBasePath + ".json";
                string dst = i.UnpackedBasePath + ".json";
                File.Copy(src, dst, true);
                long fsz = new FileInfo(dst).Length;
                File.WriteAllText(i.UnpackedBasePath + ".config", XnbMetadata.DataConfig(i.DataTypeName, i.FileName, fsz), Encoding.UTF8);
            }
            Interlocked.Increment(ref _ds);
        }
        catch (Exception ex) { _m?.Log($"✗ 写入数据失败 {i.FileName}: {ex.Message}", LogLevel.Error); Interlocked.Increment(ref _df); }
    }
}
