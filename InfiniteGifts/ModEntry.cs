using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(ModManifest.UniqueID);

            // 1. 玩家加载存档后立即对所有 NPC 启用无限送礼
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            // 2. 每天日终结算后再次重置（防止 updateFriendshipGifts 把计数清零）
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            // 3. patch receiveGift 的 prefix 保留作为兜底
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.receiveGift)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_ReceiveGift))
            );

            // 4. patch updateFriendshipGifts：让被标记为无限的 friendship 跳过日终重置
            //    这样 -999 不会被清回 0，效果持久
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateFriendshipGifts)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_UpdateFriendshipGifts))
            );
        }

        // ===== 玩家上线/新一天：把所有可社交 NPC 的 friendship 重置为 -999 =====

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // farmhand 的 friendshipData 在主机端有效，需等到联机就绪
            if (Context.IsMainPlayer)
                ReplaceAllFriendships(Game1.player);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Context.IsMainPlayer)
                ReplaceAllFriendships(Game1.player);
        }

        public static void Prefix_ReceiveGift(NPC __instance, Farmer giver)
        {
            if (!Context.IsMainPlayer || giver == null) return;
            ReplaceAllFriendships(giver);
        }

        public static void ReplaceAllFriendships(Farmer farmer)
        {
            if (farmer?.friendshipData == null) return;

            foreach (var key in farmer.friendshipData.Keys.ToArray())
            {
                var old = farmer.friendshipData[key];
                if (old == null) continue;
                if (old.GiftsToday <= -999 && old.GiftsThisWeek <= -999) continue;

                var newFriendship = new Friendship(old.Points)
                {
                    Status = old.Status,
                    Proposer = old.Proposer,
                    RoommateMarriage = old.RoommateMarriage,
                    TalkedToToday = old.TalkedToToday,
                    ProposalRejected = old.ProposalRejected,
                    GiftsToday = -999,
                    GiftsThisWeek = -999
                };

                if (old.WeddingDate != null)
                    newFriendship.WeddingDate = new WorldDate(old.WeddingDate);
                if (old.LastGiftDate != null)
                    newFriendship.LastGiftDate = new WorldDate(old.LastGiftDate);
                if (old.NextBirthingDate != null)
                    newFriendship.NextBirthingDate = new WorldDate(old.NextBirthingDate);

                farmer.friendshipData[key] = newFriendship;
            }
        }

        // ===== 阻止日终重置把 -999 清回 0 =====
        //
        // 原版 Farmer.updateFriendshipGifts 会在每天/跨周把 GiftsToday/GiftsThisWeek 清 0。
        // 我们直接跳过整个方法，让 -999 永久保留。
        // 副作用：本周满 2 礼的 +10 友谊奖励也不会触发，但无限送礼本身就让这点收益可忽略。
        public static bool Prefix_UpdateFriendshipGifts(Farmer __instance)
        {
            // 只对主机玩家生效（farmhand 的 friendship 由主机管理）
            if (!Context.IsMainPlayer) return true;

            ReplaceAllFriendships(__instance);
            return false;   // 跳过原方法
        }
    }
}
