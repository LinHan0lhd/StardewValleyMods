using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += (_, e) =>
            {
                if (!Context.IsMainPlayer || !Context.IsWorldReady) return;
                if (e.Ticks % 60 != 0) return;
                foreach (var farmer in Game1.getAllFarmers())
                    FixFriendshipData(farmer);
            };
        }

        private static void FixFriendshipData(Farmer? farmer)
        {
            if (farmer?.friendshipData == null) return;
            foreach (var pair in farmer.friendshipData.Pairs)
            {
                if (pair.Value != null)
                {
                    pair.Value.GiftsToday = -999;
                    pair.Value.GiftsThisWeek = -999;
                }
            }
        }
    }
}