using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CPXnbExporter
{
    public static class CpAssetLoader
    {
        static IModHelper _h; static IMonitor _m; static List<CpAssetInfo> _cache;
        public static void Init(IModHelper h, IMonitor m) { _h = h; _m = m; }
        public enum CpAssetType { Unknown, Texture, Map, Data }
        public class CpAssetInfo { public string AssetName, ModName, SourceFilePath; public CpAssetType AssetType; }

        public static List<CpAssetInfo> LoadAllCpAssets()
        {
            if (_cache != null) return _cache;
            var r = new List<CpAssetInfo>(); var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string md = Path.GetDirectoryName(_h.DirectoryPath);
                foreach (var d in FindPacks(md)) Scan(d, r, seen);
            }
            catch (Exception ex) { _m?.Log($"扫描出错: {ex}", LogLevel.Error); }
            _cache = r; return r;
        }

        static IEnumerable<string> FindPacks(string root)
        {
            if (!Directory.Exists(root)) yield break;
            var s = new Stack<string>(); s.Push(root);
            while (s.Count > 0)
            {
                string d = s.Pop();
                if (File.Exists(Path.Combine(d, "content.json"))) { yield return d; continue; }
                foreach (var sub in Directory.GetDirectories(d)) s.Push(sub);
            }
        }

        static void Scan(string dir, List<CpAssetInfo> r, HashSet<string> seen)
        {
            ScanFile(dir, Path.Combine(dir, "content.json"), r, seen, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        static void ScanFile(string dir, string path, List<CpAssetInfo> r, HashSet<string> seen, HashSet<string> vis)
        {
            if (!File.Exists(path) || !vis.Add(path)) return;
            JObject j; try { j = JObject.Parse(File.ReadAllText(path)); } catch { return; }
            var changes = j["Changes"] as JArray; if (changes == null) return;
            string mod = Path.GetFileName(dir);
            foreach (var c in changes)
            {
                string t = c["Target"]?.Value<string>(), a = c["Action"]?.Value<string>() ?? "Load";
                if (string.IsNullOrEmpty(t)) continue;
                if (a.Equals("Include", StringComparison.OrdinalIgnoreCase))
                {
                    string f = c["FromFile"]?.Value<string>();
                    if (!string.IsNullOrEmpty(f)) ScanFile(dir, Path.Combine(dir, f.Replace('/', Path.DirectorySeparatorChar)), r, seen, vis);
                    continue;
                }
                if (!a.Equals("Load", StringComparison.OrdinalIgnoreCase) && !a.Equals("EditData", StringComparison.OrdinalIgnoreCase)) continue;
                if (!seen.Add(t)) continue;
                CpAssetType ty = CpAssetType.Unknown;
                if (t.StartsWith("Maps/", StringComparison.OrdinalIgnoreCase)) ty = CpAssetType.Map;
                else if (t.StartsWith("Data/", StringComparison.OrdinalIgnoreCase)) ty = CpAssetType.Data;
                else ty = CpAssetType.Texture;
                r.Add(new CpAssetInfo { AssetName = t, AssetType = ty, ModName = mod, SourceFilePath = path });
            }
        }
    }
}
