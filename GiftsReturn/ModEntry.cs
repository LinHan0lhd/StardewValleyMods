using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace GiftsReturn
{
    public class ModEntry : Mod
    {
        internal static ModConfig? Config;
        internal static Dictionary<string, bool> NpcReturnedToday = new();
        internal static Dictionary<string, int> PreGiftPoints = new();

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            Harmony harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            Monitor.Log("GiftsReturn 已加载", LogLevel.Info);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            NpcReturnedToday.Clear();
            PreGiftPoints.Clear();
        }

        internal static List<string> GetLovedLikedItems(NPC npc)
        {
            if (Config == null) return new List<string>();
            var ids = new List<string>();
            if (!Game1.NPCGiftTastes.TryGetValue(npc.Name, out string tasteStr))
                return ids;

            foreach (string section in tasteStr.Split('/'))
            {
                string trimmed = section.Trim();
                bool isLove = trimmed.StartsWith("Love");
                bool isLike = trimmed.StartsWith("Like");
                if (!isLove && !isLike) continue;

                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex == -1) continue;
                string idPart = trimmed.Substring(colonIndex + 1).Trim();
                if (string.IsNullOrEmpty(idPart)) continue;

                foreach (string token in idPart.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (token.Length > 30 || token.Contains('*') || token.Contains('@') || token.Contains('$'))
                        continue;
                    ids.Add(token);
                }
            }
            return ids.Distinct().ToList();
        }

        internal static Item? GetReturnGiftByPrice(NPC npc, int targetPrice)
        {
            if (Config == null) return null;
            if (targetPrice < Config.MinReturnPrice) return null;

            var lovedLiked = GetLovedLikedItems(npc);

            if (Config.EnableGlobalSearch && lovedLiked.Count > 0)
            {
                int maxFavoritePrice = 0;
                foreach (string id in lovedLiked)
                {
                    int price = GetItemPrice(id);
                    if (price > maxFavoritePrice) maxFavoritePrice = price;
                }

                if (maxFavoritePrice < targetPrice * Config.GlobalSearchThreshold &&
                    targetPrice > Config.MinPriceForGlobalSearch)
                {
                    return GetClosestItemByPrice(targetPrice, null);
                }
            }

            var bestInFavorite = GetClosestItemByPrice(targetPrice, lovedLiked);
            if (bestInFavorite != null) return bestInFavorite;

            if (lovedLiked.Count == 0 && Config.EnableGlobalSearch)
                return GetClosestItemByPrice(targetPrice, null);

            return null;
        }

        private static int GetItemPrice(string itemId)
        {
            if (int.TryParse(itemId, out int intId))
                itemId = intId.ToString();

            if (Game1.objectData.TryGetValue(itemId, out ObjectData? data))
                return data.Price;
            return 0;
        }

        private static Item? GetClosestItemByPrice(int targetPrice, List<string>? idPool)
        {
            IEnumerable<string> query;
            if (idPool != null && idPool.Count > 0)
            {
                query = idPool;
            }
            else
            {
                query = Game1.objectData.Keys
                    .Where(key =>
                    {
                        if (!Game1.objectData.TryGetValue(key, out ObjectData? data)) return false;
                        if (data.Type is "Quest" or "Arch" or "asdf" or "Litter") return false;
                        if (data.Price <= 0) return false;
                        return true;
                    });
            }

            string bestId = "";
            int bestDiff = int.MaxValue;

            foreach (string id in query)
            {
                int price = GetItemPrice(id);
                if (price <= 0) continue;

                int diff = Math.Abs(price - targetPrice);
                if (diff < bestDiff || (diff == bestDiff && price > GetItemPrice(bestId)))
                {
                    bestDiff = diff;
                    bestId = id;
                }
            }

            if (string.IsNullOrEmpty(bestId)) return null;
            return ItemRegistry.Create(bestId, 1, 0);
        }
    }

    public class ModConfig
    {
        public float LoveReturnChance { get; set; } = 0.3f;
        public float LikeReturnChance { get; set; } = 0.3f;
        public float HateDeductChance { get; set; } = 0f;
        public int MinReturnPrice { get; set; } = 50;
        public bool EnableGlobalSearch { get; set; } = true;
        public float GlobalSearchThreshold { get; set; } = 0.5f;
        public int MinPriceForGlobalSearch { get; set; } = 500;
    }
}