using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000184 RID: 388
	public class SaveMigrator_1_3 : ISaveMigrator
	{
		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06001C6E RID: 7278 RVA: 0x0014144D File Offset: 0x0013F64D
		public Version GameVersion { get; } = new Version(1, 3);

		// Token: 0x06001C6F RID: 7279 RVA: 0x00141455 File Offset: 0x0013F655
		public bool ApplySaveFix(SaveFixes saveFix)
		{
			return false;
		}

		// Token: 0x06001C70 RID: 7280 RVA: 0x00141458 File Offset: 0x0013F658
		public static void ApplyLegacyChanges()
		{
			if (Game1.IsMasterGame)
			{
				FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
				farmHouse.furniture.Add(new Furniture("1792", Utility.PointToVector2(farmHouse.getFireplacePoint())));
				GameLocation town = Game1.RequireLocation("Town", false);
				if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && town.CanItemBePlacedHere(new Vector2(57f, 16f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					town.objects.Add(new Vector2(57f, 16f), ItemRegistry.Create<Object>("(BC)55", 1, 0, false));
				}
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(10);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(20);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(40);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(50);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(60);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(70);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(80);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(90);
				SaveMigrator_1_3.MarkFloorChestAsCollectedIfNecessary(100);
				Utility.ForEachVillager(delegate(NPC villager)
				{
					if (villager.datingFarmer.GetValueOrDefault())
					{
						Friendship friendship;
						if (Game1.player.friendshipData.TryGetValue(villager.Name, out friendship) && !friendship.IsDating())
						{
							friendship.Status = FriendshipStatus.Dating;
						}
						villager.datingFarmer = null;
					}
					if (villager.divorcedFromFarmer.GetValueOrDefault())
					{
						Friendship friendship2;
						if (Game1.player.friendshipData.TryGetValue(villager.Name, out friendship2) && !friendship2.IsDating() && !friendship2.IsDivorced())
						{
							friendship2.Status = FriendshipStatus.Divorced;
						}
						villager.divorcedFromFarmer = null;
					}
					return true;
				}, false);
				SaveMigrator_1_3.MigrateHorseIds();
				Game1.hasApplied1_3_UpdateChanges = true;
			}
		}

		// Token: 0x06001C71 RID: 7281 RVA: 0x00141578 File Offset: 0x0013F778
		public static void MarkFloorChestAsCollectedIfNecessary(int floorNumber)
		{
			MineInfo changes;
			if (MineShaft.permanentMineChanges != null && MineShaft.permanentMineChanges.TryGetValue(floorNumber, out changes) && changes.chestsLeft <= 0)
			{
				Game1.player.chestConsumedMineLevels[floorNumber] = true;
			}
		}

		// Token: 0x06001C72 RID: 7282 RVA: 0x001415B8 File Offset: 0x0013F7B8
		public static void MigrateFriendshipData(Farmer player)
		{
			if (player.obsolete_friendships != null && player.friendshipData.Length == 0)
			{
				foreach (KeyValuePair<string, int[]> friend in player.obsolete_friendships)
				{
					player.friendshipData[friend.Key] = new Friendship(friend.Value[0])
					{
						GiftsThisWeek = friend.Value[1],
						TalkedToToday = (friend.Value[2] != 0),
						GiftsToday = friend.Value[3],
						ProposalRejected = (friend.Value[4] != 0)
					};
				}
				player.obsolete_friendships = null;
			}
			if (!string.IsNullOrEmpty(player.spouse))
			{
				bool engaged = player.spouse.Contains("engaged");
				string spouseName = player.spouse.Replace("engaged", "");
				Friendship friendship = player.friendshipData[spouseName];
				if (friendship.Status == FriendshipStatus.Friendly || friendship.Status == FriendshipStatus.Dating || engaged)
				{
					friendship.Status = (engaged ? FriendshipStatus.Engaged : FriendshipStatus.Married);
					player.spouse = spouseName;
					if (!engaged)
					{
						friendship.WeddingDate = WorldDate.Now();
						friendship.WeddingDate.TotalDays -= player.obsolete_daysMarried.GetValueOrDefault();
						player.obsolete_daysMarried = null;
					}
				}
			}
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x00141738 File Offset: 0x0013F938
		private static void MigrateHorseIds()
		{
			List<Stable> stablesMissingHorses = new List<Stable>();
			Utility.ForEachBuilding<Stable>(delegate(Stable stable)
			{
				if (stable.getStableHorse() == null && stable.GetParentLocation() != null)
				{
					stablesMissingHorses.Add(stable);
				}
				return true;
			}, true);
			for (int i = stablesMissingHorses.Count - 1; i >= 0; i--)
			{
				Stable stable4 = stablesMissingHorses[i];
				GameLocation parentLocation = stable4.GetParentLocation();
				Rectangle boundingBox = stable4.GetBoundingBox();
				foreach (NPC npc in parentLocation.characters)
				{
					Horse horse = npc as Horse;
					if (horse != null && horse.HorseId == Guid.Empty && boundingBox.Intersects(horse.GetBoundingBox()))
					{
						horse.HorseId = stable4.HorseId;
						stablesMissingHorses.RemoveAt(i);
						break;
					}
				}
			}
			for (int j = stablesMissingHorses.Count - 1; j >= 0; j--)
			{
				Stable stable2 = stablesMissingHorses[j];
				foreach (NPC npc2 in stable2.GetParentLocation().characters)
				{
					Horse horse2 = npc2 as Horse;
					if (horse2 != null && horse2.HorseId == Guid.Empty)
					{
						horse2.HorseId = stable2.HorseId;
						stablesMissingHorses.RemoveAt(j);
						break;
					}
				}
			}
			foreach (Stable stable3 in stablesMissingHorses)
			{
				stable3.grabHorse();
			}
		}
	}
}
