using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace GiftsReturn
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;
        private readonly Dictionary<string, int> _lastGiftsToday = new();
        private readonly Dictionary<string, bool> _npcReturnedToday = new();
        private bool _initialized = false;

        // 缓存标签对应的物品列表，避免重复遍历所有物品
        private readonly Dictionary<string, List<string>> _tagItemCache = new();

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            Monitor.Log("GiftsReturn 已加载（支持上下文标签）", LogLevel.Info);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            _npcReturnedToday.Clear();
            _lastGiftsToday.Clear();
            _initialized = false;
            // 标签缓存不清空，因为游戏物品不会变
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            if (!_initialized)
            {
                foreach (Farmer farmer in Game1.getOnlineFarmers())
                {
                    if (farmer.friendshipData?.Pairs == null) continue;
                    foreach (var pair in farmer.friendshipData.Pairs)
                    {
                        string key = $"{farmer.UniqueMultiplayerID}_{pair.Key}";
                        _lastGiftsToday[key] = pair.Value.GiftsToday;
                    }
                }
                _initialized = true;
                Monitor.Log("轮询初始化完成，开始监控…", LogLevel.Debug);
                return;
            }

            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                if (farmer.friendshipData?.Pairs == null) continue;
                foreach (var pair in farmer.friendshipData.Pairs)
                {
                    string npcName = pair.Key;
                    int current = pair.Value.GiftsToday;
                    string key = $"{farmer.UniqueMultiplayerID}_{npcName}";

                    int previous = _lastGiftsToday.TryGetValue(key, out int v) ? v : 0;
                    if (current > previous)
                    {
                        Monitor.Log($"检测到送礼：{farmer.Name} -> {npcName} ({previous} -> {current})", LogLevel.Info);
                        _lastGiftsToday[key] = current;
                        ProcessGift(farmer, npcName, pair.Value);
                    }
                    else
                    {
                        _lastGiftsToday[key] = current;
                    }
                }
            }
        }

        private void ProcessGift(Farmer giver, string npcName, Friendship friendship)
        {
            Monitor.Log($"[ProcessGift] 处理 {giver.Name} 送 {npcName}", LogLevel.Debug);

            if (_npcReturnedToday.ContainsKey(npcName))
            {
                Monitor.Log($"[ProcessGift] {npcName} 今日已回礼，跳过", LogLevel.Debug);
                return;
            }

            NPC? npc = Game1.getCharacterFromName(npcName) as NPC;
            if (npc == null)
            {
                Monitor.Log($"[ProcessGift] 找不到NPC对象: {npcName}", LogLevel.Warn);
                return;
            }

            float chance = Config.LoveReturnChance;
            if (Game1.random.NextDouble() > chance)
            {
                Monitor.Log("[ProcessGift] 随机数未命中", LogLevel.Debug);
                return;
            }

            // 物品选取：普通 → 喜欢 → 最爱（按等级回退）
            var rawItems = GetItemsByTaste(npc, 2);
            if (rawItems.Count == 0) rawItems = GetItemsByTaste(npc, 1);
            if (rawItems.Count == 0) rawItems = GetItemsByTaste(npc, 0);

            if (rawItems.Count == 0)
            {
                Monitor.Log($"[ProcessGift] {npcName} 没有任何可回礼的物品", LogLevel.Warn);
                return;
            }

            // 解析标签和数字ID，构建最终的有效物品ID列表
            var resolvedItems = new List<string>();
            foreach (string rawId in rawItems)
            {
                string? resolved = ResolveItemId(rawId);
                if (resolved != null)
                    resolvedItems.Add(resolved);
            }

            if (resolvedItems.Count == 0)
            {
                Monitor.Log($"[ProcessGift] 无法解析任何有效物品ID", LogLevel.Warn);
                return;
            }

            string finalId = resolvedItems[Game1.random.Next(resolvedItems.Count)];
            Item? gift = ItemRegistry.Create(finalId, 1, 0);
            if (gift == null)
            {
                Monitor.Log($"[ProcessGift] 物品创建失败: {finalId}", LogLevel.Error);
                return;
            }

            Monitor.Log($"[ProcessGift] 准备赠送 {gift.Name} (ID:{finalId})", LogLevel.Debug);

            if (giver.addItemToInventoryBool(gift))
            {
                Game1.showGlobalMessage($"{npcName} 回赠了 {giver.Name} 一个 {gift.Name}！");
                Monitor.Log($"回礼入包：{npcName} -> {giver.Name}：{gift.Name}", LogLevel.Info);
            }
            else
            {
                int dir = (int)giver.facingDirection.Value;
                Game1.createItemDebris(gift, new Vector2(giver.Position.X, giver.Position.Y + 64), dir, giver.currentLocation, (int)giver.UniqueMultiplayerID);
                Game1.showGlobalMessage($"{npcName} 的回礼掉在了 {giver.Name} 的脚下！");
                Monitor.Log($"私有掉落：{npcName} -> {giver.Name}：{gift.Name}", LogLevel.Info);
            }

            _npcReturnedToday[npcName] = true;
        }

        /// <summary>
        /// 如果传入的是数字ID直接返回；如果是标签（如 doll_item），则从拥有该标签的物品中随机返回一个数字ID。
        /// 找不到对应物品时返回 null。
        /// </summary>
        private string? ResolveItemId(string input)
        {
            // 是纯数字ID，直接返回
            if (int.TryParse(input, out _))
                return input;

            // 是标签，需要解析
            string tag = input.Trim();
            if (string.IsNullOrEmpty(tag)) return null;

            // 从缓存获取或构建物品列表
            if (!_tagItemCache.TryGetValue(tag, out var idList))
            {
                idList = new List<string>();
                foreach (var kvp in Game1.objectData)
                {
                    ObjectData data = kvp.Value;
                    if (data.ContextTags?.Contains(tag, StringComparer.OrdinalIgnoreCase) == true)
                    {
                        idList.Add(kvp.Key);
                    }
                }
                _tagItemCache[tag] = idList;
                Monitor.Log($"解析标签 '{tag}': 找到 {idList.Count} 个物品", LogLevel.Debug);
            }

            if (idList.Count == 0) return null;
            return idList[Game1.random.Next(idList.Count)];
        }

        // 从 NPC 礼物词条中提取指定等级的物品 ID（可能包含数字和标签）
        private List<string> GetItemsByTaste(NPC npc, int taste)
        {
            if (!Game1.NPCGiftTastes.TryGetValue(npc.Name, out string? raw))
                return new List<string>();
            var sections = raw.Split('/');
            int index = taste switch { 0 => 0, 1 => 1, 2 => 2, _ => -1 };
            if (index < 0 || index >= sections.Length) return new List<string>();
            return ParseItemIds(sections[index].Trim());
        }

        // 解析字符串，提取物品 ID（数字）和标签（英文标识符）
        private static List<string> ParseItemIds(string raw)
        {
            var ids = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return ids;
            foreach (string token in raw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = token.Trim();
                if (t.Length == 0 || t.Length > 50) continue;
                // 保留数字 ID 和可能的标签（字母开头、无特殊符号）
                if (int.TryParse(t, out _) || Regex.IsMatch(t, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    ids.Add(t);
            }
            return ids.Distinct().ToList();
        }
    }
}