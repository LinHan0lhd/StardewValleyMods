using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // 纯主机方案：只在主机端运行，farmhand 无需安装此 mod。
            //
            // 原理限制（通过研究反编译源码确认）：
            // - 每个 farmer 权威管理自己的 friendshipData，主机改在线 farmhand 的数据不会同步
            // - 送礼检查和修改都在 giver 本地执行（NPC.receiveGift 用 giver.friendshipData）
            // - 过夜 dayupdate 每端各自执行，主机无法阻止 farmhand 本地清零
            //
            // 所以纯主机方案只能做到：
            // - 主机自己：完全无限送礼（patch updateFriendshipGifts 阻止清零）
            // - farmhand：每次下线再上线时重置（patch saveFarmhand 在写回 farmhandData 前修改）
            //   farmhand 在线过夜后会被本地清零，需要重新下线上线才能恢复

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);
            // 1. 阻止主机自己过夜清零（仅对主机自己的 farmer 生效）
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateFriendshipGifts)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_UpdateFriendshipGifts))
            );
            // 2. farmhand 下线时，在 saveFarmhand 写回 farmhandData 前重置 friendship
            //    Multiplayer.playerDisconnected (Multiplayer.cs:1009) 调用 saveFarmhand
            //    saveFarmhand 把 farmhand 数据写回 netWorldState.farmhandData
            //    farmhand 重连时从 farmhandData 全量同步（GameServer.cs:556）
            harmony.Patch(
                original: AccessTools.Method(typeof(Multiplayer), "saveFarmhand", new[] { typeof(NetFarmerRoot) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_SaveFarmhand))
            );
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            // 主机自己
            ReplaceAllFriendships(Game1.player);
            // 所有离线 farmhand（farmhand 重连时从 farmhandData 全量同步）
            foreach (var farmhand in Game1.netWorldState.Value.farmhandData.Values)
            {
                ReplaceAllFriendships(farmhand);
            }
            Monitor.Log($"[无限送礼] 存档加载完成，已重置主机 + {Game1.netWorldState.Value.farmhandData.Count} 个离线 farmhand", LogLevel.Info);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            // 主机自己（防止过夜清零，虽然 prefix 已拦截，双保险）
            ReplaceAllFriendships(Game1.player);
            // 所有离线 farmhand（在线的改了也没用，数据不在 farmhandData）
            foreach (var farmhand in Game1.netWorldState.Value.farmhandData.Values)
            {
                ReplaceAllFriendships(farmhand);
            }
            Monitor.Log($"[无限送礼] 新一天开始，已重置主机 + 离线 farmhand", LogLevel.Info);
        }

        /// <summary>
        /// 阻止主机自己过夜时 updateFriendshipGifts 清零。
        /// 仅对主机自己的 farmer 跳过原方法，其他 farmer（farmhand 副本）正常执行不影响。
        /// </summary>
        public static bool Prefix_UpdateFriendshipGifts(Farmer __instance)
        {
            if (__instance == Game1.player)
            {
                ReplaceAllFriendships(__instance);
                return false;   // 跳过主机自己的清零
            }
            return true;   // 其他 farmer 副本正常执行（不影响）
        }

        /// <summary>
        /// farmhand 下线时，在 saveFarmhand 写回 farmhandData 前重置 friendship。
        /// 这样 farmhandData 中保存的是 -999，farmhand 重连时全量同步得到 -999。
        /// 重连时 Client.setUpGame 调用 updateFriendshipGifts，由于 LastGiftDate=today，不会清零。
        /// </summary>
        public static void Prefix_SaveFarmhand(NetFarmerRoot farmhand)
        {
            if (farmhand?.Value != null)
            {
                ReplaceAllFriendships(farmhand.Value);
                Monitor.Log($"[无限送礼] farmhand {farmhand.Value.Name} 下线，已重置 friendship 到 -999", LogLevel.Info);
            }
        }

        /// <summary>
        /// 把所有可送礼 NPC 的 GiftsToday/GiftsThisWeek 设为 -999，实现无限送礼。
        /// 同时设置 LastGiftDate = 今天，避免 farmhand 重连时 updateFriendshipGifts 清零
        /// （Client.setUpGame 在重连时立即调用 updateFriendshipGifts，如果 LastGiftDate != today 会清零）。
        ///
        /// 必须遍历 Game1.NPCGiftTastes（全量可送礼 NPC）而非 friendshipData.Keys
        /// （后者是懒加载，只有互动过的 NPC 才有条目）。
        /// </summary>
        public static void ReplaceAllFriendships(Farmer farmer)
        {
            if (farmer?.friendshipData == null) return;
            if (Game1.NPCGiftTastes == null) return;

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
                    friendship = new Friendship(0);
                    farmer.friendshipData[npcName] = friendship;
                }

                friendship.GiftsToday = -999;
                friendship.GiftsThisWeek = -999;
                friendship.LastGiftDate = new WorldDate(Game1.Date);   // 关键：绕过重连清零
            }
        }
    }
}
