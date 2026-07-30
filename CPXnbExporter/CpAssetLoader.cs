using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CPXnbExporter
{
    /// <summary>CP 资产扫描器。仅扫描当前语言资产。</summary>
    public static class CpAssetLoader
    {
        private static IModHelper _h;
        private static IMonitor _m;
        private static List<CpAssetInfo> _cache;

        public static void Init(IModHelper h, IMonitor m) { _h = h; _m = m; }

        public enum CpAssetType { Unknown, Texture, Map, Data }

        public class CpAssetInfo
        {
            public string AssetName, ModName, SourceFilePath;
            public CpAssetType AssetType;
        }

        public static List<CpAssetInfo> LoadAllCpAssets()
        {
            if (_cache != null) return _cache;
            var r = new List<CpAssetInfo>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string md = Path.GetDirectoryName(_h.DirectoryPath);
                _m?.Log($"扫描 Mods 目录: {md}", LogLevel.Trace);
                foreach (var d in FindContentPacks(md))
                    ScanContentPack(d, r, seen);
                _m?.Log($"扫描完成，找到 {r.Count} 个 CP 资产（当前语言）", LogLevel.Info);
            }
            catch (Exception ex) { _m?.Log($"扫描 CP 资产时出错: {ex}", LogLevel.Error); }
            _cache = r; return r;
        }

        /// <summary>
        /// 递归查找所有含 content.json 的目录（CP 内容包）。
        /// 一旦某目录含 content.json，不再深入其子目录（CP 包不会嵌套 CP 包）。
        /// </summary>
        static IEnumerable<string> FindContentPacks(string root)
        {
            if (!Directory.Exists(root)) yield break;
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                string cur = stack.Pop();
                string[] subs;
                try { subs = Directory.GetDirectories(cur); }
                catch { continue; }
                foreach (var sub in subs)
                {
                    // 跳过导出器自身
                    if (sub.Equals(_h?.DirectoryPath, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (File.Exists(Path.Combine(sub, "content.json")))
                        yield return sub; // 找到 CP 包，不再深入
                    else
                        stack.Push(sub);
                }
            }
        }

        static void ScanContentPack(string dir, List<CpAssetInfo> r, HashSet<string> seen)
        {
            ScanFile(dir, Path.Combine(dir, "content.json"), r, seen,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        static void ScanFile(string dir, string path, List<CpAssetInfo> r,
            HashSet<string> seen, HashSet<string> vis)
        {
            string normPath = Path.GetFullPath(path);
            if (!File.Exists(path) || !vis.Add(normPath)) return;

            JObject j;
            try { j = JObject.Parse(File.ReadAllText(path)); }
            catch { return; }
            var changes = j["Changes"] as JArray;
            if (changes == null) return;

            string modName = Path.GetFileName(dir);
            string manifestPath = Path.Combine(dir, "manifest.json");
            if (File.Exists(manifestPath))
            {
                try { modName = JObject.Parse(File.ReadAllText(manifestPath))["Name"]?.ToString() ?? modName; }
                catch { }
            }

            foreach (var c in changes)
            {
                try
                {
                    string action = c["Action"]?.Value<string>() ?? "";
                    string target = c["Target"]?.Value<string>();
                    string fromFile = c["FromFile"]?.Value<string>();
                    string logName = c["LogName"]?.Value<string>();

                    // Include 动作：递归扫描引用的子 content.json
                    if (action.Equals("Include", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(fromFile)) continue;
                        foreach (var inc in ResolveIncludePaths(dir, fromFile))
                            ScanFile(dir, inc, r, seen, vis);
                        continue;
                    }

                    if (string.IsNullOrEmpty(target)) continue;
                    // 跳过 EditData — 数据资产不导出
                    if (action.Equals("EditData", StringComparison.OrdinalIgnoreCase)) continue;

                    var targets = ParseTargets(target);
                    foreach (var t in targets)
                    {
                        if (seen.Contains(t)) continue;
                        var ty = DetectType(action, fromFile, t, dir);
                        if (ty == CpAssetType.Data) continue; // 跳过数据类型

                        seen.Add(t);
                        r.Add(new CpAssetInfo
                        {
                            AssetName = t,
                            AssetType = ty,
                            ModName = modName,
                            SourceFilePath = !string.IsNullOrEmpty(fromFile) ? Path.Combine(dir, fromFile) : null
                        });
                        _m?.Log($"    [CP] {modName} - {logName ?? t}: {t} ({action})", LogLevel.Trace);
                    }
                }
                catch (Exception ex) { _m?.Log($"    解析 patch 时出错: {ex.Message}", LogLevel.Trace); }
            }
        }

        static List<string> ParseTargets(string raw)
        {
            var r = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return r;
            foreach (var part in raw.Split(','))
            {
                var t = part.Trim();
                if (!string.IsNullOrEmpty(t)) r.Add(t);
            }
            return r;
        }

        /// <summary>
        /// 解析 Include 动作的 FromFile，支持通配符（如 "assets/*.json"）和逗号分隔多文件。
        /// </summary>
        static List<string> ResolveIncludePaths(string dir, string fromFile)
        {
            var paths = new List<string>();
            foreach (var part in fromFile.Split(','))
            {
                var t = part.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                string full = Path.Combine(dir, t.Replace('/', Path.DirectorySeparatorChar));
                if (t.Contains('*') || t.Contains('?'))
                {
                    string d = Path.GetDirectoryName(full) ?? dir;
                    string pat = Path.GetFileName(full);
                    if (Directory.Exists(d))
                    {
                        try { foreach (var f in Directory.GetFiles(d, pat)) paths.Add(f); }
                        catch { }
                    }
                }
                else paths.Add(full);
            }
            return paths;
        }

        static CpAssetType DetectType(string action, string fromFile, string target, string dir)
        {
            string actionLower = action.ToLowerInvariant();
            string targetLower = target.ToLowerInvariant().Replace('\\', '/');
            string ext = Path.GetExtension(fromFile ?? "").ToLowerInvariant();

            // 1. FromFile 扩展名（最可靠）
            if (!string.IsNullOrEmpty(ext))
            {
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp") return CpAssetType.Texture;
                if (ext == ".tmx" || ext == ".tbin") return CpAssetType.Map;
                if (ext == ".json") return CpAssetType.Data;
            }
            // 2. FromFile 没有扩展名，查实际文件
            else if (!string.IsNullOrEmpty(fromFile) && !string.IsNullOrEmpty(dir))
            {
                string full = Path.Combine(dir, fromFile.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full))
                {
                    string re = Path.GetExtension(full).ToLowerInvariant();
                    if (re == ".png") return CpAssetType.Texture;
                    if (re == ".tmx" || re == ".tbin") return CpAssetType.Map;
                }
                if (File.Exists(full + ".png")) return CpAssetType.Texture;
                if (File.Exists(full + ".tbin") || File.Exists(full + ".tmx")) return CpAssetType.Map;
            }

            // 3. Action 判断
            if (actionLower == "editmap") return CpAssetType.Map;
            if (actionLower == "editdata") return CpAssetType.Data;
            if (actionLower == "editimage") return CpAssetType.Texture;

            // 4. Data 路径判断
            if (targetLower.StartsWith("data/")) return CpAssetType.Data;

            // 5. 已知贴图目录兜底
            var textureDirs = new[]
            {
                "characters/", "loosesprites/", "tilesheets/", "terrainfeatures/",
                "buildings/", "portraits/", "objects/", "furniture/", "hats/",
                "boots/", "weapons/", "tools/", "craftables/", "bigcraftables/",
                "animals/", "monsters/", "shadows/", "emotes/", "achievements/",
                "bundles/", "craftingmenu/", "cursors/", "font/", "fonts/",
                "lumberjack/", "mail/", "menutiles/", "nightmarket/", "quicksell/",
                "robin/", "temp/", "tv/", "wand/", "junimohut/", "cat/", "dog/",
                "horse/", "babies/", "secretnotes/", "springobjects/", "crop/",
                "fruittrees/", "wildtrees/"
            };
            foreach (var d in textureDirs)
                if (targetLower.StartsWith(d)) return CpAssetType.Texture;

            // 6. Maps/ 目录下已知贴图资产（非地图）
            var mapsTextures = new[]
            {
                "maps/springobjects", "maps/summerobjects",
                "maps/fallobjects", "maps/winterobjects",
                "maps/craftables", "maps/crops", "maps/bushes",
                "maps/concessions", "maps/parrotupgrade",
                "maps/festival", "maps/festivals",
                "maps/extras", "maps/walls",
                "maps/equippable", "maps/objectinfo",
                "maps/fruit_tree", "maps/wild_tree"
            };
            foreach (var t in mapsTextures)
                if (targetLower == t || targetLower.StartsWith(t + ".")) return CpAssetType.Texture;

            // 7. Load 动作兜底 → Texture
            if (actionLower == "load") return CpAssetType.Texture;

            return CpAssetType.Unknown;
        }
    }
}
