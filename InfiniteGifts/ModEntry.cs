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
            // 1. 存档加载后重置（farmhand 连接前，主机的 friendship 会被全量同步过去）
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            // 2. patch updateFriendshipGifts：阻止过夜时把 -999 清零
            //    过夜时每个客户端在本地调用 dayupdate → updateFriendshipGifts，
            //    friendshipData 是 NetField 会自动同步，所以必须从源头阻止清零。
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateFriendshipGifts)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_UpdateFriendshipGifts))
            );
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;
            ReplaceAllFriendships(Game1.player);
            Monitor.Log("[无限送礼] 存档加载完成，已重置所有 NPC friendship", LogLevel.Info);
        }

        /// <summary>
        /// 阻止原版 updateFriendshipGifts 把 GiftsToday/GiftsThisWeek 清零。
        /// 过夜时每个客户端都会在本地调用此方法，friendshipData 是 NetField 会自动同步，
        /// 不阻止的话 -999 会被清成 0 并同步给所有人。
        /// 我们改成：重新把所有 friendship 设回 -999，并跳过原方法。
        /// </summary>
        public static bool Prefix_UpdateFriendshipGifts(Farmer __instance)
        {
            ReplaceAllFriendships(__instance);
            return false;   // 跳过原方法
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
