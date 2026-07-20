using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace InfiniteGifts
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.receiveGift)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_ReceiveGift))
            );
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
    }
}