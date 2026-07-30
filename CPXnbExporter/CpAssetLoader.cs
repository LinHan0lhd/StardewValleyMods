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
            var r = new List<CpAssetInfo>();
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string md = Path.GetDirectoryName(_h.DirectoryPath);
                _m?.Log($"扫描根目录: {md}", LogLevel.Info);
                int count = 0;
                foreach (var d in FindPacks(md)) { Scan(d, r, seen); count++; }
                _m?.Log($"找到 {count} 个 CP 包, {r.Count} 个资源", LogLevel.Info);
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
                if (File.Exists(Path.Combine(d, "content.json"))) { yield return d; }
                // Keep scanning subdirectories even after finding a content.json,
                // since some mods nest CP packs deeper
                foreach (var sub in Directory.GetDirectories(d)) s.Push(sub);
            }
        }

        static void Scan(string dir, List<CpAssetInfo> r, Dictionary<string, int> seen)
        {
            ScanFile(dir, Path.Combine(dir, "content.json"), r, seen, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        static void ScanFile(string dir, string path, List<CpAssetInfo> r, Dictionary<string, int> seen, HashSet<string> vis)
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
                if (!IsSupportedAction(a)) continue;
                string fromFile = c["FromFile"]?.Value<string>();
                CpAssetType ty = DetectType(a, t, fromFile, dir);
                if (seen.TryGetValue(t, out int idx))
                {
                    // Load action with a detectable file type takes precedence over Edit* actions
                    if (a.Equals("Load", StringComparison.OrdinalIgnoreCase) && ty != CpAssetType.Unknown)
                        r[idx].AssetType = ty;
                    continue;
                }
                seen[t] = r.Count;
                r.Add(new CpAssetInfo { AssetName = t, AssetType = ty, ModName = mod, SourceFilePath = path });
            }
        }

        static bool IsSupportedAction(string a) =>
            a.Equals("Load", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("EditData", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("EditImage", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("EditMap", StringComparison.OrdinalIgnoreCase);

        static CpAssetType DetectType(string action, string target, string fromFile, string dir)
        {
            // Priority 1: FromFile extension
            if (!string.IsNullOrEmpty(fromFile))
            {
                var ty = TypeFromExtension(Path.GetExtension(fromFile));
                if (ty != CpAssetType.Unknown) return ty;
                // Priority 2: FromFile has no recognizable extension — check disk
                string fullPath = Path.Combine(dir, fromFile.Replace('/', Path.DirectorySeparatorChar));
                foreach (var e in new[] { ".png", ".tbin", ".tmx", ".json" })
                    if (File.Exists(fullPath + e)) return TypeFromExtension(e);
            }
            // Priority 3: Action-based (EditImage/EditMap/EditData)
            if (!action.Equals("Load", StringComparison.OrdinalIgnoreCase))
            {
                var ty = TypeFromAction(action);
                if (ty != CpAssetType.Unknown) return ty;
            }
            // Priority 4: Path prefix — only Data/ is deterministic
            if (target.StartsWith("Data/", StringComparison.OrdinalIgnoreCase)) return CpAssetType.Data;
            // Priority 5: Load action fallback → Texture
            if (action.Equals("Load", StringComparison.OrdinalIgnoreCase)) return CpAssetType.Texture;
            // Edit actions with no match → Texture
            return CpAssetType.Texture;
        }

        static CpAssetType TypeFromExtension(string ext) => ext.ToLowerInvariant() switch
        {
            ".png" or ".jpg" or ".jpeg" or ".bmp" => CpAssetType.Texture,
            ".tmx" or ".tbin" => CpAssetType.Map,
            ".json" => CpAssetType.Data,
            _ => CpAssetType.Unknown
        };

        static CpAssetType TypeFromAction(string action) => action.ToLowerInvariant() switch
        {
            "editimage" => CpAssetType.Texture,
            "editmap" => CpAssetType.Map,
            "editdata" => CpAssetType.Data,
            _ => CpAssetType.Unknown
        };
    }
}
