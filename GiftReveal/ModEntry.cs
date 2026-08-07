#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;

namespace GiftReveal
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;
        private Dictionary<string, Dictionary<int, List<string>>> _npcPositiveGifts = new();
        private Dictionary<string, HashSet<string>> _npcNegativeGifts = new();
        private Dictionary<int, List<string>> _universalPositiveGifts = new();
        private bool _initialized = false;

        // 等待第二天确认后再写入配置的揭示玩家 ID
        private HashSet<long> _pendingRevealedPlayerIDs = new();

        // 缓存：类别 -> 物品ID列表
        private Dictionary<int, List<string>> _categoryItems = new();
        // 缓存：标签 -> 物品ID列表
        private Dictionary<string, List<string>> _tagItems = new();

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.SaveLoaded += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;
                BuildItemCaches();
                LoadGiftData();
            };

            helper.Events.GameLoop.DayStarted += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;

                // 先确认前一天的揭示结果（确保已存档）
                CommitPendingReveals();

                if (!_initialized)
                {
                    BuildItemCaches();
                    LoadGiftData();
                }
                if (_initialized) WriteGiftsToAllPlayers();
            };

            helper.Events.Multiplayer.PeerConnected += (_, _) =>
            {
                if (!Context.IsMainPlayer || !_initialized) return;
                WriteGiftsToAllPlayers();
            };
        }

        private void BuildItemCaches()
        {
            _categoryItems.Clear();
            _tagItems.Clear();

            foreach (var kvp in Game1.objectData)
            {
                string itemId = kvp.Key;
                ObjectData data = kvp.Value;

                // 类别
                int category = data.Category;
                if (category != 0)
                {
                    if (!_categoryItems.ContainsKey(category))
                        _categoryItems[category] = new List<string>();
                    _categoryItems[category].Add(itemId);
                }

                // 标签
                if (data.ContextTags != null)
                {
                    foreach (string tag in data.ContextTags)
                    {
                        if (string.IsNullOrWhiteSpace(tag)) continue;
                        if (!_tagItems.ContainsKey(tag))
                            _tagItems[tag] = new List<string>();
                        _tagItems[tag].Add(itemId);
                    }
                }
            }
        }

        private List<string> ResolveItemOrCategoryOrTag(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return new List<string>();

            // 1. 负数：类别 ID
            if (int.TryParse(token, out int num) && num < 0)
            {
                if (_categoryItems.TryGetValue(num, out var items))
                    return items;
                return new List<string>();
            }

            // 2. 物品 ID（数字、字符串 ID 或限定 ID 如 (O)24）
            ParsedItemData data = ItemRegistry.GetData(token);
            if (data != null)
            {
                return new List<string> { data.ItemId };
            }

            // 3. 上下文标签
            if (_tagItems.TryGetValue(token, out var taggedItems))
                return taggedItems;

            // 4. 无法识别：忽略
            return new List<string>();
        }

        private void LoadGiftData()
        {
            try
            {
                if (Game1.NPCGiftTastes == null || Game1.NPCGiftTastes.Count == 0) return;

                _npcPositiveGifts.Clear();
                _npcNegativeGifts.Clear();
                _universalPositiveGifts.Clear();

                foreach (var kvp in Game1.NPCGiftTastes)
                {
                    string name = kvp.Key;
                    string raw = kvp.Value;
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    if (name.StartsWith("Universal_"))
                    {
                        if (name == "Universal_Love")
                            ExtractUniversalItems(raw, 5);
                        else if (name == "Universal_Like")
                            ExtractUniversalItems(raw, 2);
                    }
                    else
                    {
                        var posDict = new Dictionary<int, List<string>>();
                        var negSet = new HashSet<string>();

                        string[] sections = raw.Split('/');
                        // 只处理前5个段落：最爱、喜欢、普通、不喜欢、讨厌
                        int limit = Math.Min(sections.Length, 5);

                        for (int i = 0; i < limit; i++)
                        {
                            string section = sections[i].Trim();
                            if (string.IsNullOrWhiteSpace(section)) continue;
                            if (!ContainsAnyItemIdOrCategory(section)) continue;

                            if (i == 0) // 最爱
                                AddItemsToTier(posDict, 5, section);
                            else if (i == 1) // 喜欢
                                AddItemsToTier(posDict, 2, section);
                            else if (i == 3 || i == 4) // 讨厌
                                CollectItemsToSet(negSet, section);
                        }

                        if (posDict.Count > 0)
                            _npcPositiveGifts[name] = posDict;
                        if (negSet.Count > 0)
                            _npcNegativeGifts[name] = negSet;
                    }
                }

                _initialized = _npcPositiveGifts.Count > 0 || _universalPositiveGifts.Count > 0;
                if (_initialized)
                    Monitor.Log($"加载完成：{_npcPositiveGifts.Count} 个NPC，{_universalPositiveGifts.Sum(x => x.Value.Count)} 个通用物品", LogLevel.Info);
                else
                    Monitor.Log("加载不完整 > 将在明天重试", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Monitor.Log($"加载异常: {ex}", LogLevel.Error);
            }
        }

        private bool ContainsAnyItemIdOrCategory(string text)
        {
            foreach (string token in text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                // 类别
                if (int.TryParse(token, out int num) && num < 0)
                    return true;

                // 物品 ID（支持数字、字符串 ID、限定 ID 如 (O)24）
                if (ItemRegistry.Exists(token))
                    return true;

                // 上下文标签
                if (_tagItems.ContainsKey(token))
                    return true;
            }
            return false;
        }

        private void ExtractUniversalItems(string raw, int tier)
        {
            var ids = ExpandAllTokens(raw);
            if (ids.Count > 0)
            {
                if (!_universalPositiveGifts.ContainsKey(tier))
                    _universalPositiveGifts[tier] = new List<string>();
                _universalPositiveGifts[tier].AddRange(ids);
            }
        }

        private void AddItemsToTier(Dictionary<int, List<string>> dict, int tier, string content)
        {
            var ids = ExpandAllTokens(content);
            if (ids.Count > 0)
            {
                if (!dict.ContainsKey(tier))
                    dict[tier] = new List<string>();
                dict[tier].AddRange(ids);
            }
        }

        private void CollectItemsToSet(HashSet<string> set, string content)
        {
            var ids = ExpandAllTokens(content);
            foreach (var id in ids)
                set.Add(id);
        }

        private List<string> ExpandAllTokens(string raw)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

            string[] tokens = raw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                string t = token.Trim();
                if (t.Length == 0 || t.Length > 50) continue;

                var expanded = ResolveItemOrCategoryOrTag(t);
                foreach (var id in expanded)
                    result.Add(id);
            }
            return result.ToList();
        }

        private void WriteGiftsToAllPlayers()
        {
            if (!_initialized || !Config.Enabled) return;

            int totalAdded = 0;
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                long uid = farmer.UniqueMultiplayerID;
                if (Config.RevealedPlayerIDs.Contains(uid) || _pendingRevealedPlayerIDs.Contains(uid))
                    continue;

                totalAdded += WriteGiftsForPlayer(farmer);
                _pendingRevealedPlayerIDs.Add(uid);
            }

            if (totalAdded > 0)
                Monitor.Log($"本轮新增 {totalAdded} 个礼物标记", LogLevel.Info);
        }

        private void CommitPendingReveals()
        {
            if (_pendingRevealedPlayerIDs.Count == 0) return;

            bool changed = false;
            foreach (long id in _pendingRevealedPlayerIDs)
            {
                if (!Config.RevealedPlayerIDs.Contains(id))
                {
                    Config.RevealedPlayerIDs.Add(id);
                    changed = true;
                }
            }
            _pendingRevealedPlayerIDs.Clear();

            if (changed)
            {
                Helper.WriteConfig(Config);
                Monitor.Log($"已确认 {Config.RevealedPlayerIDs.Count} 名玩家的礼物揭示", LogLevel.Info);
            }
        }

        private int WriteGiftsForPlayer(Farmer player)
        {
            if (player.giftedItems == null)
                player.giftedItems = new SerializableDictionary<string, SerializableDictionary<string, int>>();

            int addedCount = 0;

            foreach (string npcName in _npcPositiveGifts.Keys)
            {
                if (!player.giftedItems.TryGetValue(npcName, out var innerDict))
                {
                    innerDict = new SerializableDictionary<string, int>();
                    player.giftedItems[npcName] = innerDict;
                }

                // NPC 专属爱/喜欢
                if (_npcPositiveGifts.TryGetValue(npcName, out var npcPositives))
                {
                    foreach (var tierKvp in npcPositives)
                    {
                        int tier = tierKvp.Key;
                        foreach (string id in tierKvp.Value)
                        {
                            if (!innerDict.ContainsKey(id))
                            {
                                innerDict[id] = tier;
                                addedCount++;
                            }
                            else if (innerDict[id] < tier)
                            {
                                innerDict[id] = tier;
                            }
                        }
                    }
                }

                // 通用爱/喜欢（排除该 NPC 讨厌物品）
                if (_universalPositiveGifts.Count > 0)
                {
                    _npcNegativeGifts.TryGetValue(npcName, out var negativeSet);

                    foreach (var universalKvp in _universalPositiveGifts)
                    {
                        int universalTier = universalKvp.Key;
                        foreach (string id in universalKvp.Value)
                        {
                            if (negativeSet != null && negativeSet.Contains(id))
                                continue;
                            if (innerDict.ContainsKey(id))
                                continue;
                            innerDict[id] = universalTier;
                            addedCount++;
                        }
                    }
                }
            }

            return addedCount;
        }
    }
}