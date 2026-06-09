#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewValley;

namespace GiftReveal
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;
        private Dictionary<string, Dictionary<int, List<string>>> _npcPositiveGifts = new();
        private Dictionary<string, HashSet<string>> _npcNegativeGifts = new();
        private Dictionary<int, List<string>> _universalPositiveGifts = new();
        private bool _initialized = false;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.SaveLoaded += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;
                LoadGiftData();
            };

            helper.Events.GameLoop.DayStarted += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;
                if (!_initialized) LoadGiftData();
                if (_initialized) WriteGiftsToAllPlayers();
            };

            helper.Events.Multiplayer.PeerConnected += (_, _) =>
            {
                if (!Context.IsMainPlayer || !_initialized) return;
                WriteGiftsToAllPlayers();
            };
        }

        private void LoadGiftData()
        {
            try
            {
                if (Game1.NPCGiftTastes == null || Game1.NPCGiftTastes.Count == 0)
                {
                    Monitor.Log("NPCGiftTastes 为空 > 等待下次尝试", LogLevel.Warn);
                    return;
                }

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

                        // 顺序：最爱、喜欢、一般、讨厌、最讨厌
                        string[] sections = raw.Split('/');

                        for (int i = 0; i < sections.Length; i++)
                        {
                            string section = sections[i].Trim();
                            if (string.IsNullOrWhiteSpace(section)) continue;

                            // 跳过纯文本行
                            if (!ContainsAnyItemId(section)) continue;

                            if (i == 0) // 最爱
                                AddItemsToTier(posDict, 5, section);
                            else if (i == 1) // 喜欢
                                AddItemsToTier(posDict, 2, section);
                            else if (i == 3 || i == 4) // 讨厌/最讨厌
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

        private bool ContainsAnyItemId(string text)
        {
            foreach (string token in text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token, out _) || Regex.IsMatch(token, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    return true;
            }
            return false;
        }

        private void ExtractUniversalItems(string raw, int tier)
        {
            var ids = ParseItemIds(raw);
            if (ids.Count > 0)
            {
                if (!_universalPositiveGifts.ContainsKey(tier))
                    _universalPositiveGifts[tier] = new List<string>();
                _universalPositiveGifts[tier].AddRange(ids);
            }
        }

        private void AddItemsToTier(Dictionary<int, List<string>> dict, int tier, string content)
        {
            var ids = ParseItemIds(content);
            if (ids.Count > 0)
            {
                if (!dict.ContainsKey(tier))
                    dict[tier] = new List<string>();
                dict[tier].AddRange(ids);
            }
        }

        private void CollectItemsToSet(HashSet<string> set, string content)
        {
            var ids = ParseItemIds(content);
            foreach (var id in ids)
                set.Add(id);
        }

        private List<string> ParseItemIds(string raw)
        {
            var set = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

            string[] tokens = raw.Split(new[] { ' ', '\t', '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                string t = token.Trim();
                if (t.Length == 0 || t.Length > 50) continue;
                if (int.TryParse(t, out _) || Regex.IsMatch(t, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    set.Add(t);
            }
            return set.ToList();
        }

        private void WriteGiftsToAllPlayers()
        {
            if (!_initialized || !Config.Enabled) return;

            int totalAdded = 0;
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                long uid = farmer.UniqueMultiplayerID;
                if (Config.RevealedPlayerIDs.Contains(uid))
                    continue;

                totalAdded += WriteGiftsForPlayer(farmer);
                Config.RevealedPlayerIDs.Add(uid);
            }

            Helper.WriteConfig(Config);

            if (totalAdded > 0)
                Monitor.Log($"本轮新增 {totalAdded} 个礼物标记", LogLevel.Info);
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
                                innerDict[id] = tier; // 升级等级
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