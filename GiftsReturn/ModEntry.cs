using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<string, int> _lastPoints = new();
        private readonly Dictionary<string, bool> _npcReturnedToday = new();
        private bool _initialized = false;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            _npcReturnedToday.Clear();
            _lastPoints.Clear();
            _initialized = false;
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
                        _lastPoints[key] = pair.Value.Points;
                    }
                }
                _initialized = true;
                return;
            }

            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                if (farmer.friendshipData?.Pairs == null) continue;
                foreach (var pair in farmer.friendshipData.Pairs)
                {
                    string npcName = pair.Key;
                    int currentPoints = pair.Value.Points;
                    string key = $"{farmer.UniqueMultiplayerID}_{npcName}";

                    if (_lastPoints.TryGetValue(key, out int previousPoints) && currentPoints > previousPoints)
                    {
                        int gained = currentPoints - previousPoints;
                        Monitor.Log($"检测到送礼：{farmer.Name} -> {npcName} (+{gained} 好感)", LogLevel.Debug);
                        ProcessGift(farmer, npcName, gained);
                    }
                    _lastPoints[key] = currentPoints;
                }
            }
        }

        private void ProcessGift(Farmer giver, string npcName, int pointsGained)
        {
            if (pointsGained <= 0) return;
            if (_npcReturnedToday.ContainsKey(npcName)) return;

            NPC? npc = Game1.getCharacterFromName(npcName) as NPC;
            if (npc == null) return;

            float chance = Config.LoveReturnChance;
            if (Game1.random.NextDouble() > chance) return;

            int giftValue = (int)(pointsGained * Config.GiftValueMultiplier);
            if (giftValue <= 0) return;

            Item? gift = CreateRandomGift(giftValue);
            if (gift == null) return;

            if (giver.addItemToInventoryBool(gift))
            {
                Game1.showGlobalMessage($"{npcName} 回赠了 {giver.Name} 一个 {gift.Name}！");
                Monitor.Log($"回礼入包：{npcName} -> {giver.Name}：{gift.Name} (价值 {gift.salePrice()})", LogLevel.Info);
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

        private Item? CreateRandomGift(int targetValue)
        {
            var candidates = new List<string>();
            foreach (var kvp in Game1.objectData)
            {
                ObjectData data = kvp.Value;
                if (data.Price <= 0 || data.Price > targetValue * 2) continue;
                if (Math.Abs(data.Price - targetValue) <= targetValue * 0.3f)
                {
                    candidates.Add(kvp.Key);
                }
            }
            if (candidates.Count == 0) return null;
            string id = candidates[Game1.random.Next(candidates.Count)];
            return ItemRegistry.Create(id, 1, 0);
        }
    }
}