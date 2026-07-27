using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // 仅在存档加载后和每天开始时重置 friendship。
            // 关键时机：必须在 farmhand 连接前修改主机的 friendshipData，
            // farmhand 上线时才会从主机全量同步过去；上线后再改无效。
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            ReplaceAllFriendships(Game1.player);
            Monitor.Log("[无限送礼] 存档加载完成，已重置所有 NPC friendship", LogLevel.Info);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            ReplaceAllFriendships(Game1.player);
            Monitor.Log("[无限送礼] 新一天开始，已重置所有 NPC friendship", LogLevel.Info);
        }

        /// <summary>
        /// 把所有可送礼 NPC 的 GiftsToday/GiftsThisWeek 设为 -999，实现无限送礼。
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

                // 已有 friendship：直接修改原对象的 NetField，保持同步链路
                if (farmer.friendshipData.TryGetValue(npcName, out var old) && old != null)
                {
                    old.GiftsToday = -999;
                    old.GiftsThisWeek = -999;
                    continue;
                }

                // 没有 friendship：主动创建（解决"送谁谁才无限"的懒加载问题）
                farmer.friendshipData[npcName] = new Friendship(0)
                {
                    GiftsToday = -999,
                    GiftsThisWeek = -999
                };
            }
        }
    }
}
