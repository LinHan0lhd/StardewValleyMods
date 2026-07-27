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
            // 关键：mod 必须在每端各自运行，每端只改自己的 Game1.player.friendshipData。
            // 不能用 Context.IsMainPlayer 限制，否则 farmhand 端不运行。
            //
            // 原理：
            // - 每个 farmer 权威管理自己的 friendshipData（Multiplayer.broadcastFarmerDeltas
            //   仅当 farmerRoot.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID 才广播）
            // - 主机改 farmhand 的 friendshipData 不会同步给 farmhand
            // - 送礼检查和修改都在 giver 本地执行（NPC.receiveGift 用 giver.friendshipData）
            // - 过夜 dayupdate 每端各自执行，只对自己的 Game1.player 调用
            // - farmhand 重连时 Client.setUpGame 会立即调用 updateFriendshipGifts 清零
            //
            // 所以必须：每端运行 + 设 LastGiftDate=今天 绕过重连清零 + patch 过夜清零

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateFriendshipGifts)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_UpdateFriendshipGifts))
            );
        }

        /// <summary>
        /// 存档加载后：每端各自重置自己的 friendshipData。
        /// 主机端此时 farmhand 全部离线，主机改自己的 friendship 会被广播给后续连入的 farmhand。
        /// farmhand 端加载存档后立即重置自己的 friendship（不再依赖主机同步）。
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ReplaceAllFriendships(Game1.player);
            Monitor.Log($"[无限送礼] 存档加载完成，已重置 {Game1.player.Name} 的所有 NPC friendship", LogLevel.Info);
        }

        /// <summary>
        /// 每天开始：再次重置，防止过夜后状态丢失。
        /// 注意：此时 updateFriendshipGifts 已在 dayupdate 中执行过（会被 prefix 拦截），
        /// 这里再保险性地设一次。
        /// </summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            ReplaceAllFriendships(Game1.player);
            Monitor.Log($"[无限送礼] 新一天开始，已重置 {Game1.player.Name} 的所有 NPC friendship", LogLevel.Info);
        }

        /// <summary>
        /// 阻止原版 updateFriendshipGifts 把 GiftsToday/GiftsThisWeek 清零。
        /// 此方法在两个时机被调用：
        /// 1. 过夜时 Farmer.dayupdate → resetFriendshipsForNewDay → updateFriendshipGifts
        /// 2. farmhand 重连时 Client.setUpGame → updateFriendshipGifts
        /// 这两个时机都会把 -999 清零，必须拦截。
        /// </summary>
        public static bool Prefix_UpdateFriendshipGifts(Farmer __instance)
        {
            // 只处理本地玩家（避免对其他 farmer 副本造成意外影响）
            if (__instance.IsLocalPlayer)
            {
                ReplaceAllFriendships(__instance);
            }
            return false;   // 跳过原方法
        }

        /// <summary>
        /// 把所有可送礼 NPC 的 GiftsToday/GiftsThisWeek 设为 -999，实现无限送礼。
        /// 同时设置 LastGiftDate = 今天，避免后续 updateFriendshipGifts 检查时重置。
        ///
        /// 必须遍历 Game1.NPCGiftTastes（全量可送礼 NPC）而非 friendshipData.Keys
        /// （后者是懒加载，只有互动过的 NPC 才有条目）。
        /// </summary>
        public static void ReplaceAllFriendships(Farmer farmer)
        {
            if (farmer?.friendshipData == null) return;
            if (Game1.NPCGiftTastes == null) return;

            int count = 0;
            foreach (var npcName in Game1.NPCGiftTastes.Keys.ToArray())
            {
                // 跳过不可社交/不可送礼的 NPC
                var npc = Game1.getCharacterFromName(npcName, true, false);
                if (npc == null || !npc.CanReceiveGifts()) continue;

                Friendship friendship;
                if (farmer.friendshipData.TryGetValue(npcName, out var old) && old != null)
                {
                    friendship = old;
                }
                else
                {
                    // 没有 friendship：主动创建（解决"送谁谁才无限"的懒加载问题）
                    friendship = new Friendship(0);
                    farmer.friendshipData[npcName] = friendship;
                }

                friendship.GiftsToday = -999;
                friendship.GiftsThisWeek = -999;
                friendship.LastGiftDate = new WorldDate(Game1.Date);   // 关键：设为今天，绕过重置
                count++;
            }
        }
    }
}
