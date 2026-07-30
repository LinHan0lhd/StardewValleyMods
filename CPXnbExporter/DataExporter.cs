using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CPXnbExporter;
public static class DataExporter
{
    static readonly System.Lazy<JsonSerializerSettings> _s = new(() => new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = { new StringEnumConverter() } });
    public static bool ExportData(string path, object data, string name)
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path + ".json", JsonConvert.SerializeObject(data, _s.Value), System.Text.Encoding.UTF8); return true; }
        catch { return false; }
    }
}
