using HarmonyLib;
using Microsoft.Xna.Framework;
using System;
using StardewValley;

namespace GiftsReturn
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.receiveGift))]
    internal static class NpcGiftPatch
    {
        private const int TASTE_LOVE = 0;
        private const int TASTE_LIKE = 1;
        private const int TASTE_HATE = 3;

        static bool Prefix(NPC __instance, StardewValley.Object o, Farmer giver, ref bool __result)
        {
            if (!Game1.IsMasterGame || giver == null || o == null)
                return true;

            int taste = __instance.getGiftTasteForThisItem(o);

            if (taste == TASTE_HATE)
            {
                __result = false; // 拒绝接受

                // 概率扣好感
                if (Game1.random.NextDouble() < (ModEntry.Config?.HateDeductChance ?? 0))
                {
                    if (giver.friendshipData.TryGetValue(__instance.Name, out Friendship f))
                    {
                        f.Points = Math.Clamp(f.Points - 40, 0, 2500);
                    }
                }

                // 向所有人发送拒绝提示
                Game1.showGlobalMessage($"{__instance.Name} 拒绝了 {giver.Name} 送来的礼物……");
                return false; // 完全不执行原方法，礼物不会被消耗
            }

            // 记录送礼前的好感点数（用于回礼增量计算）
            if (taste == TASTE_LOVE || taste == TASTE_LIKE)
            {
                string key = $"{__instance.Name}_{giver.UniqueMultiplayerID}";
                if (giver.friendshipData.TryGetValue(__instance.Name, out Friendship f))
                    ModEntry.PreGiftPoints[key] = f.Points;
            }

            return true;
        }

        static void Postfix(NPC __instance, StardewValley.Object o, Farmer giver, bool __result)
        {
            if (ModEntry.Config == null) return;
            if (!Game1.IsMasterGame || !__result || giver == null) return;

            int taste = __instance.getGiftTasteForThisItem(o);
            if (taste != TASTE_LOVE && taste != TASTE_LIKE) return;

            if (ModEntry.NpcReturnedToday.ContainsKey(__instance.Name)) return;

            float chance = taste == TASTE_LOVE
                ? ModEntry.Config.LoveReturnChance
                : ModEntry.Config.LikeReturnChance;

            if (Game1.random.NextDouble() > chance) return;

            // 计算好感增量
            string key = $"{__instance.Name}_{giver.UniqueMultiplayerID}";
            int prePoints = ModEntry.PreGiftPoints.TryGetValue(key, out int pts) ? pts : 0;
            if (!giver.friendshipData.TryGetValue(__instance.Name, out Friendship f)) return;
            int delta = f.Points - prePoints;
            if (delta < 0) delta = 0;

            int basePrice = o.salePrice();
            float loveMultiplier = taste == TASTE_LOVE ? 1.5f : 1.0f;
            int hearts = f.Points / 250;
            float heartFactor = Math.Max(0.5f, hearts * 0.1f);
            float deltaFactor = 1f + delta * 0.01f;

            int targetPrice = (int)(basePrice * loveMultiplier * heartFactor * deltaFactor);

            Item? gift = ModEntry.GetReturnGiftByPrice(__instance, targetPrice);
            if (gift == null) return;

            if (giver.addItemToInventoryBool(gift))
            {
                Game1.showGlobalMessage($"{__instance.Name} 回赠了 {giver.Name} 一个 {gift.Name}！");
            }
            else
            {
                // 从 NetDirection 中取出枚举值并转换为整数方向
                int dir = (int)giver.facingDirection.Value;
                Game1.createItemDebris(gift, giver.Position, dir, giver.currentLocation);
                Game1.showGlobalMessage($"{__instance.Name} 的回礼掉在了地上……");
            }

            ModEntry.NpcReturnedToday[__instance.Name] = true;
        }
    }
}