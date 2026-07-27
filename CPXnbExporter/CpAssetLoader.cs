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

                // 递归扫描所有子目录，查找含 content.json 的 CP 内容包。
                // 支持嵌套目录结构（如 Mods/我的/模组A），不只扫描一层。
                foreach (var modDir in FindContentPacks(modsDir))
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
            ScanContentFile(modDir, Path.Combine(modDir, "content.json"), result, seenAssets, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 递归查找所有含 content.json 的目录（CP 内容包）。
        /// 支持嵌套目录结构，如 Mods/我的/模组A。
        /// 一旦某目录含 content.json，不再深入其子目录（CP 包不会嵌套 CP 包）。
        /// </summary>
        private static IEnumerable<string> FindContentPacks(string rootDir)
        {
            if (!Directory.Exists(rootDir)) yield break;

            var stack = new Stack<string>();
            stack.Push(rootDir);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                string[] subDirs;
                try { subDirs = Directory.GetDirectories(currentDir); }
                catch { continue; }

                foreach (string subDir in subDirs)
                {
                    // 跳过导出器自身（避免扫描 exported 目录）
                    if (subDir.Equals(_helper?.DirectoryPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string contentJson = Path.Combine(subDir, "content.json");
                    if (File.Exists(contentJson))
                    {
                        // 找到 CP 内容包，不再深入其子目录
                        yield return subDir;
                    }
                    else
                    {
                        // 不是 CP 包，继续递归查找
                        stack.Push(subDir);
                    }
                }
            }
        }

        /// <summary>
        /// 扫描单个 content.json 文件（支持 Include 动作递归）。
        /// CP 的 Include 动作会引用另一个 content.json 文件，该文件有自己的 Changes 数组。
        /// 不处理 Include 会导致 TileSheets/weapons 等被拆分到子文件的贴图漏检。
        /// </summary>
        private static void ScanContentFile(string modDir, string contentJsonPath, List<CpAssetInfo> result, HashSet<string> seenAssets, HashSet<string> includedFiles)
        {
            // 防止 Include 循环引用
            string normalizedPath = Path.GetFullPath(contentJsonPath);
            if (includedFiles.Contains(normalizedPath))
            {
                _monitor?.Log($"    跳过已扫描的 Include 文件: {contentJsonPath}", LogLevel.Trace);
                return;
            }
            includedFiles.Add(normalizedPath);

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

                _monitor?.Log($"  扫描 CP 文件: {Path.GetFileName(contentJsonPath)} ({changes.Count} 个 patches)", LogLevel.Trace);

                foreach (var change in changes)
                {
                    try
                    {
                        string action = change["Action"]?.ToString() ?? "";
                        string targetRaw = change["Target"]?.ToString();
                        string fromFile = change["FromFile"]?.ToString();
                        string logName = change["LogName"]?.ToString();

                        // Include 动作：递归扫描引用的子 content.json
                        if (action.Equals("Include", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(fromFile)) continue;
                            // CP 的 Include 支持通配符（如 "assets/*.json"）和逗号分隔多文件
                            foreach (string includePath in ResolveIncludePaths(modDir, fromFile))
                            {
                                ScanContentFile(modDir, includePath, result, seenAssets, includedFiles);
                            }
                            continue;
                        }

                        if (string.IsNullOrEmpty(targetRaw)) continue;
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
                _monitor?.Log($"  读取 content 文件失败 [{contentJsonPath}]: {ex.Message}", LogLevel.Trace);
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

        /// <summary>
        /// 解析 Include 动作的 FromFile，支持通配符（如 "assets/*.json"）和逗号分隔多文件。
        /// CP 的 Include 常用于把大型 content.json 拆分成多个子文件。
        /// </summary>
        private static List<string> ResolveIncludePaths(string modDir, string fromFile)
        {
            var paths = new List<string>();
            foreach (string part in fromFile.Split(','))
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                string fullPath = Path.Combine(modDir, trimmed);

                // 处理通配符（* 或 ?）
                if (trimmed.Contains('*') || trimmed.Contains('?'))
                {
                    string dir = Path.GetDirectoryName(fullPath);
                    string pattern = Path.GetFileName(fullPath);
                    if (string.IsNullOrEmpty(dir)) dir = modDir;
                    if (Directory.Exists(dir))
                    {
                        try
                        {
                            foreach (string matched in Directory.GetFiles(dir, pattern))
                                paths.Add(matched);
                        }
                        catch { }
                    }
                }
                else
                {
                    paths.Add(fullPath);
                }
            }
            return paths;
        }

        private static CpAssetType DetectAssetType(string action, string fromFile, string target, string modDir)
        {
            string actionLower = action.ToLowerInvariant();
            // 规范化路径分隔符：统一为正斜杠，避免反斜杠导致 StartsWith 匹配失败
            string targetLower = target.ToLowerInvariant().Replace('\\', '/');
            string ext = Path.GetExtension(fromFile ?? "").ToLowerInvariant();

            // 1. FromFile 扩展名（最可靠）
            if (!string.IsNullOrEmpty(ext))
            {
                if (ext == ".png") return CpAssetType.Texture;
                if (ext == ".tmx" || ext == ".tbin") return CpAssetType.Map;
            }
            // 2. FromFile 没有扩展名，查 CP 包目录下实际文件（尝试常见扩展名）
            else if (!string.IsNullOrEmpty(fromFile) && !string.IsNullOrEmpty(modDir))
            {
                string fullPath = Path.Combine(modDir, fromFile);
                if (File.Exists(fullPath))
                {
                    string realExt = Path.GetExtension(fullPath).ToLowerInvariant();
                    if (realExt == ".png") return CpAssetType.Texture;
                    if (realExt == ".tmx" || realExt == ".tbin") return CpAssetType.Map;
                }
                // 尝试补常见扩展名（CP 的 FromFile 有时会省略扩展名）
                if (File.Exists(fullPath + ".png")) return CpAssetType.Texture;
                if (File.Exists(fullPath + ".tbin") || File.Exists(fullPath + ".tmx")) return CpAssetType.Map;
            }

            // 3. Action 判断
            if (actionLower == "editmap") return CpAssetType.Map;
            if (actionLower == "editdata") return CpAssetType.Data;
            if (actionLower == "editimage") return CpAssetType.Texture;

            // 3.5 Data 路径判断（Load/Include 动作可能也指向 Data）
            if (targetLower.StartsWith("data/"))
                return CpAssetType.Data;

            // 4. 已知贴图目录兜底（这些目录下的资产一定是贴图）
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

            // 4.5 Maps/ 目录下已知贴图资产（非地图）
            // 这些是 Maps/ 下的 .png 精灵图，不是 .tbin 地图。
            // 物品贴图（springobjects）是最常被漏检的，因为它在 Maps/ 下但不是地图。
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
            foreach (var tex in mapsTextures)
            {
                // 精确匹配或带语言后缀（如 maps/springobjects.zh-cn）
                if (targetLower == tex || targetLower.StartsWith(tex + "."))
                    return CpAssetType.Texture;
            }

            // 5. Load 动作兜底
            if (actionLower == "load") return CpAssetType.Texture;

            return CpAssetType.Unknown;
        }
    }
}
