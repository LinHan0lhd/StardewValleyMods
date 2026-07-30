using StardewModdingAPI;

namespace CPXnbExporter;
public class ModConfig
{
    public int Concurrency { get; set; } = 2;
    public int Queue { get; set; } = 100;
    public int PerFrame { get; set; } = 3;
    public bool AutoExport { get; set; } = false;
    public string AutoPlatform { get; set; } = "a";
    public bool AutoUnpacked { get; set; } = false;
    public void Validate(IMonitor m)
    {
        if (Concurrency < 1) { Concurrency = 1; m?.Log("Concurrency=1", LogLevel.Warn); }
        if (Queue < 10) { Queue = 10; m?.Log("Queue=10", LogLevel.Warn); }
        if (PerFrame < 1) { PerFrame = 1; m?.Log("PerFrame=1", LogLevel.Warn); }
    }
}
