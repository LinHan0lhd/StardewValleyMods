using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace CPXnbExporter
{
    /// <summary>CP 资产扫描器。v2.1.1：仅扫描当前语言资产，移除多语言支持。</summary>
    public static class CpAssetLoader
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;
        private static List<CpAssetInfo> _cachedAssets;

        public static void Init(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }


        public enum CpAssetType
        {
            Unknown,
            Texture,
            Map,
            Data
        }

        public class CpAssetInfo
        {
            public string AssetName { get; set; }
            public CpAssetType AssetType { get; set; }
            public string ModName { get; set; }
            public string SourceFilePath { get; set; }
        }

        /// <summary>
        /// 扫描所有 Content Patcher 包中的资产，仅返回当前游戏语言下的可导出资产。
        /// v2.1.1：移除多语言扫描，大幅简化。
        /// </summary>
        public static List<CpAssetInfo> LoadAllCpAssets()
        {
            if (_cachedAssets != null)
                return _cachedAssets;

            var result = new List<CpAssetInfo>();
            var seenAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string modsDir = Path.GetDirectoryName(_helper.DirectoryPath);
                _monitor?.Log($"扫描 Mods 目录: {modsDir}", LogLevel.Trace);

                foreach (var modDir in Directory.GetDirectories(modsDir))
                {
                    ScanContentPack(modDir, result, seenAssets);
                }

                _monitor?.Log($"扫描完成，找到 {result.Count} 个 CP 资产（当前语言）", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"扫描 CP 资产时出错: {ex}", LogLevel.Error);
            }

            _cachedAssets = result;
            return result;
        }

        private static void ScanContentPack(string modDir, List<CpAssetInfo> result, HashSet<string> seenAssets)
        {
            string contentJsonPath = Path.Combine(modDir, "content.json");
            if (!File.Exists(contentJsonPath))
                return;

            string modName = Path.GetFileName(modDir);
            string manifestPath = Path.Combine(modDir, "manifest.json");
            if (File.Exists(manifestPath))
            {
                try
                {
                    var manifest = JObject.Parse(File.ReadAllText(manifestPath));
                    modName = manifest["Name"]?.ToString() ?? modName;
                }
                catch { }
            }

            try
            {
                string jsonText = File.ReadAllText(contentJsonPath);
                var settings = new JsonLoadSettings { CommentHandling = CommentHandling.Ignore };
                var contentData = JObject.Parse(jsonText, settings);

                var changes = contentData["Changes"] as JArray;
                if (changes == null) return;

                _monitor?.Log($"  扫描 CP 包: {modName} ({changes.Count} 个 patches)", LogLevel.Trace);

                foreach (var change in changes)
                {
                    try
                    {
                        string action = change["Action"]?.ToString() ?? "";
                        string targetRaw = change["Target"]?.ToString();
                        string fromFile = change["FromFile"]?.ToString();
                        string logName = change["LogName"]?.ToString();

                        if (string.IsNullOrEmpty(targetRaw)) continue;
                        if (action.Equals("Include", StringComparison.OrdinalIgnoreCase)) continue;
                        if (action.Equals("EditData", StringComparison.OrdinalIgnoreCase)) continue;

                        var targets = ParseTargets(targetRaw);
                        foreach (var target in targets)
                        {
                            CpAssetType assetType = DetectAssetType(action, fromFile, target, modDir);
                            if (assetType == CpAssetType.Data) continue;

                            if (seenAssets.Contains(target))
                                continue;
                            seenAssets.Add(target);

                            result.Add(new CpAssetInfo
                            {
                                AssetName = target,
                                AssetType = assetType,
                                ModName = modName,
                                SourceFilePath = !string.IsNullOrEmpty(fromFile) ? Path.Combine(modDir, fromFile) : null
                            });

                            _monitor?.Log($"    [CP] {modName} - {logName ?? target}: {target} ({action})", LogLevel.Trace);
                        }
                    }
                    catch (Exception ex)
                    {
                        _monitor?.Log($"    解析 patch 时出错: {ex.Message}", LogLevel.Trace);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"  读取 content.json 失败 [{modDir}]: {ex.Message}", LogLevel.Trace);
            }
        }

        private static List<string> ParseTargets(string targetRaw)
        {
            var targets = new List<string>();
            if (string.IsNullOrWhiteSpace(targetRaw)) return targets;

            foreach (var part in targetRaw.Split(','))
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    targets.Add(trimmed);
            }
            return targets;
        }

        private static CpAssetType DetectAssetType(string action, string fromFile, string target, string modDir)
        {
            string actionLower = action.ToLowerInvariant();
            string targetLower = target.ToLowerInvariant();
            string ext = Path.GetExtension(fromFile ?? "").ToLowerInvariant();

            // 1. FromFile 扩展名（最可靠）
            if (!string.IsNullOrEmpty(ext))
            {
                if (ext == ".png") return CpAssetType.Texture;
                if (ext == ".tmx" || ext == ".tbin") return CpAssetType.Map;
            }
            // 2. FromFile 没有扩展名，查 CP 包目录下实际文件
            else if (!string.IsNullOrEmpty(fromFile) && !string.IsNullOrEmpty(modDir))
            {
                string fullPath = Path.Combine(modDir, fromFile);
                if (File.Exists(fullPath))
                {
                    string realExt = Path.GetExtension(fullPath).ToLowerInvariant();
                    if (realExt == ".png") return CpAssetType.Texture;
                    if (realExt == ".tmx" || realExt == ".tbin") return CpAssetType.Map;
                }
            }

            // 3. Action 判断
            if (actionLower == "editmap") return CpAssetType.Map;
            if (actionLower == "editdata") return CpAssetType.Data;
            if (actionLower == "editimage") return CpAssetType.Texture;

            // 4. 已知贴图目录兜底
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

            foreach (var dir in textureDirs)
            {
                if (targetLower.StartsWith(dir))
                    return CpAssetType.Texture;
            }

            // 5. Load 动作兜底
            if (actionLower == "load") return CpAssetType.Texture;

            return CpAssetType.Unknown;
        }
    }
}
