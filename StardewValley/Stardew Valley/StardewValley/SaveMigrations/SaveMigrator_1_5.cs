using System;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Enchantments;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000186 RID: 390
	public class SaveMigrator_1_5 : ISaveMigrator
	{
		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06001C7A RID: 7290 RVA: 0x00141FC9 File Offset: 0x001401C9
		public Version GameVersion { get; } = new Version(1, 5);

		// Token: 0x06001C7B RID: 7291 RVA: 0x00141FD4 File Offset: 0x001401D4
		public bool ApplySaveFix(SaveFixes saveFix)
		{
			switch (saveFix)
			{
			case SaveFixes.BedsToFurniture:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					FarmHouse house = location as FarmHouse;
					if (house != null)
					{
						bool hasOwner = house.HasOwner;
						for (int x = 0; x < house.map.Layers[0].LayerWidth; x++)
						{
							for (int y = 0; y < house.map.Layers[0].LayerHeight; y++)
							{
								if (house.doesTileHaveProperty(x, y, "DefaultBedPosition", "Back", false) != null)
								{
									if (house.upgradeLevel == 0)
									{
										house.furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2((float)x, (float)y)));
									}
									else
									{
										string bedId = BedFurniture.DOUBLE_BED_INDEX;
										if (hasOwner && !house.owner.activeDialogueEvents.ContainsKey("pennyRedecorating"))
										{
											if (house.owner.mailReceived.Contains("pennyQuilt0"))
											{
												bedId = "2058";
											}
											if (house.owner.mailReceived.Contains("pennyQuilt1"))
											{
												bedId = "2064";
											}
											if (house.owner.mailReceived.Contains("pennyQuilt2"))
											{
												bedId = "2070";
											}
										}
										house.furniture.Add(new BedFurniture(bedId, new Vector2((float)x, (float)y)));
									}
								}
							}
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.ChildBedsToFurniture:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					FarmHouse house = location as FarmHouse;
					if (house != null)
					{
						for (int x = 0; x < house.map.Layers[0].LayerWidth; x++)
						{
							for (int y = 0; y < house.map.Layers[0].LayerHeight; y++)
							{
								if (house.doesTileHaveProperty(x, y, "DefaultChildBedPosition", "Back", false) != null)
								{
									house.furniture.Add(new BedFurniture(BedFurniture.CHILD_BED_INDEX, new Vector2((float)x, (float)y)));
								}
							}
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.ModularizeFarmStructures:
				Game1.getFarm().AddDefaultBuildings(true);
				return true;
			case SaveFixes.FixFlooringFlags:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					foreach (TerrainFeature terrainFeature in location.terrainFeatures.Values)
					{
						Flooring flooring = terrainFeature as Flooring;
						if (flooring != null)
						{
							flooring.ApplyFlooringFlags();
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.FixStableOwnership:
				Utility.ForEachBuilding<Stable>(delegate(Stable stable)
				{
					if (stable.owner.Value == -6666666L && Game1.GetPlayer(-6666666L, false) == null)
					{
						stable.owner.Value = Game1.player.UniqueMultiplayerID;
					}
					return true;
				}, true);
				return true;
			case SaveFixes.ResetForges:
				SaveMigrator_1_5.ResetForges();
				return true;
			case SaveFixes.MakeDarkSwordVampiric:
				Utility.ForEachItem(delegate(Item item)
				{
					MeleeWeapon weapon = item as MeleeWeapon;
					if (weapon != null && weapon.QualifiedItemId == "(W)2")
					{
						weapon.AddEnchantment(new VampiricEnchantment());
					}
					return true;
				});
				return true;
			case SaveFixes.FixBeachFarmBushes:
				if (Game1.whichFarm == 6)
				{
					Farm farm = Game1.getFarm();
					foreach (Vector2 bushLocation in new Vector2[]
					{
						new Vector2(77f, 4f),
						new Vector2(78f, 3f),
						new Vector2(83f, 4f),
						new Vector2(83f, 3f)
					})
					{
						foreach (LargeTerrainFeature feature in farm.largeTerrainFeatures)
						{
							if (feature.Tile == bushLocation)
							{
								Bush bush = feature as Bush;
								if (bush != null)
								{
									bush.Tile = new Vector2(bush.Tile.X, bush.Tile.Y + 1f);
									break;
								}
								break;
							}
						}
					}
				}
				return true;
			case SaveFixes.OstrichIncubatorFragility:
				Utility.ForEachItem(delegate(Item item)
				{
					Object obj = item as Object;
					if (obj != null && obj.Fragility == 2 && obj.Name == "Ostrich Incubator")
					{
						obj.Fragility = 0;
					}
					return true;
				});
				return true;
			case SaveFixes.LeoChildrenFix:
				Utility.FixChildNameCollisions();
				return true;
			case SaveFixes.Leo6HeartGermanFix:
				if (Utility.HasAnyPlayerSeenEvent("6497428") && !Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved"))
				{
					Game1.addMailForTomorrow("leoMoved", true, true);
					Game1.player.team.requestLeoMove.Fire();
				}
				return true;
			case SaveFixes.BirdieQuestRemovedFix:
				foreach (Farmer who in Game1.getAllFarmers())
				{
					if (who.hasQuest("130"))
					{
						foreach (Quest quest in who.questLog)
						{
							if (quest.id.Value == "130")
							{
								quest.canBeCancelled.Value = true;
							}
						}
					}
					if (who.hasOrWillReceiveMail("birdieQuestBegun") && !who.hasOrWillReceiveMail("birdieQuestFinished"))
					{
						who.addQuest("130");
					}
				}
				return true;
			case SaveFixes.SkippedSummit:
				if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
				{
					foreach (Farmer who2 in Game1.getAllFarmers())
					{
						if (!who2.songsHeard.Contains("end_credits"))
						{
							who2.mailReceived.Remove("Summit_event");
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x001423F4 File Offset: 0x001405F4
		public static void ResetForges()
		{
			Utility.ForEachItem(delegate(Item item)
			{
				MeleeWeapon weapon = item as MeleeWeapon;
				if (weapon != null)
				{
					weapon.RecalculateAppliedForges(false);
				}
				return true;
			});
		}
	}
}
