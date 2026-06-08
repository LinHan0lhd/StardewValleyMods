#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GiftReveal
{
    public class ModEntry : Mod
    {
        private Dictionary<string, List<string>> _npcGifts = new();
        private Dictionary<string, List<string>> _universalGifts = new();
        private bool _initialized = false;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;
                LoadGiftData();
            };

            helper.Events.GameLoop.DayStarted += (_, _) =>
            {
                if (!Context.IsMainPlayer) return;
                if (!_initialized) LoadGiftData();
                if (_initialized) WriteFullGiftsToAllPlayers();
            };

            helper.Events.Multiplayer.PeerConnected += (_, _) =>
            {
                if (!Context.IsMainPlayer || !_initialized) return;
                WriteFullGiftsToAllPlayers();
            };

            Monitor.Log("GiftReveal 已加载", LogLevel.Info);
        }

        private void LoadGiftData()
        {
            try
            {
                if (Game1.NPCGiftTastes == null || Game1.NPCGiftTastes.Count == 0)
                {
                    Monitor.Log("NPCGiftTastes 为空，等待下次尝试", LogLevel.Warn);
                    return;
                }

                _npcGifts.Clear();
                _universalGifts.Clear();

                foreach (var kvp in Game1.NPCGiftTastes)
                {
                    string name = kvp.Key;
                    if (name.StartsWith("Universal_"))
                    {
                        string category = name.Substring("Universal_".Length);
                        var ids = ParseGiftItems(kvp.Value);
                        if (ids.Count > 0)
                            _universalGifts[category] = ids;
                    }
                    else
                    {
                        var ids = ParseGiftItems(kvp.Value);
                        if (ids.Count > 0)
                            _npcGifts[name] = ids;
                    }
                }

                _initialized = _npcGifts.Count > 0 && _universalGifts.Count > 0;
                if (_initialized)
                    Monitor.Log($"加载完成：{_npcGifts.Count} 个NPC，{_universalGifts.Count} 个通用类别", LogLevel.Info);
                else
                    Monitor.Log("加载不完整，将在明天重试", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Monitor.Log($"加载异常: {ex}", LogLevel.Error);
            }
        }

        private void WriteFullGiftsToAllPlayers()
        {
            if (!_initialized) return;
            int totalAdded = 0;
            foreach (Farmer farmer in Game1.getAllFarmers())
                totalAdded += WriteGiftsForPlayer(farmer);

            if (totalAdded > 0)
                Monitor.Log($"本轮新增 {totalAdded} 个礼物标记", LogLevel.Info);
        }

        private int WriteGiftsForPlayer(Farmer player)
        {
            if (player.giftedItems == null)
                player.giftedItems = new SerializableDictionary<string, SerializableDictionary<string, int>>();

            int addedCount = 0;

            foreach (var npcKvp in _npcGifts)
            {
                string npcName = npcKvp.Key;
                if (!player.giftedItems.TryGetValue(npcName, out var innerDict))
                {
                    innerDict = new SerializableDictionary<string, int>();
                    player.giftedItems[npcName] = innerDict;
                }

                // 专属喜好
                foreach (string id in npcKvp.Value)
                {
                    if (!innerDict.ContainsKey(id))
                    {
                        innerDict[id] = 1;
                        addedCount++;
                    }
                }

                // 通用喜好
                foreach (var universalKvp in _universalGifts)
                {
                    foreach (string id in universalKvp.Value)
                    {
                        if (!innerDict.ContainsKey(id))
                        {
                            innerDict[id] = 1;
                            addedCount++;
                        }
                    }
                }
            }

            return addedCount;
        }

        private List<string> ParseGiftItems(string raw)
        {
            var set = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

            foreach (string section in raw.Split('/'))
            {
                string trimmed = section.Trim();
                if (trimmed.Length == 0) continue;

                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex >= 0)
                    trimmed = trimmed.Substring(colonIndex + 1).Trim();

                string[] tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (token.Length > 50) continue;
                    if (int.TryParse(token, out _) || Regex.IsMatch(token, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                        set.Add(token);
                }
            }

            return set.ToList();
        }
    }
}