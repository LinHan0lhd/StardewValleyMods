using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000185 RID: 389
	public class SaveMigrator_1_4 : ISaveMigrator
	{
		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06001C75 RID: 7285 RVA: 0x00141929 File Offset: 0x0013FB29
		public Version GameVersion { get; } = new Version(1, 4);

		// Token: 0x06001C76 RID: 7286 RVA: 0x00141934 File Offset: 0x0013FB34
		public bool ApplySaveFix(SaveFixes saveFix)
		{
			switch (saveFix)
			{
			case SaveFixes.StoredBigCraftablesStackFix:
				Utility.ForEachItem(delegate(Item item)
				{
					Object obj = item as Object;
					if (obj != null && obj.bigCraftable.Value && obj.Stack == 0)
					{
						obj.Stack = 1;
					}
					return true;
				});
				return true;
			case SaveFixes.PorchedCabinBushesFix:
				Utility.ForEachBuilding(delegate(Building building)
				{
					if (building.daysOfConstructionLeft.Value <= 0 && building.GetIndoors() is Cabin)
					{
						building.removeOverlappingBushes(Game1.getFarm());
					}
					return true;
				}, true);
				return true;
			case SaveFixes.ChangeObeliskFootprintHeight:
				Utility.ForEachBuilding(delegate(Building building)
				{
					if (building.buildingType.Value.Contains("Obelisk"))
					{
						building.tilesHigh.Value = 2;
						NetInt tileY = building.tileY;
						int value = tileY.Value;
						tileY.Value = value + 1;
					}
					return true;
				}, true);
				return true;
			case SaveFixes.CreateStorageDressers:
				Utility.ForEachItem(delegate(Item item)
				{
					if (item is Clothing)
					{
						item.Category = -100;
					}
					return true;
				});
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location is DecoratableLocation)
					{
						List<Furniture> furnitureToAdd = new List<Furniture>();
						for (int j = 0; j < location.furniture.Count; j++)
						{
							Furniture oldFurniture = location.furniture[j];
							if (oldFurniture.ItemId == "704" || oldFurniture.ItemId == "709" || oldFurniture.ItemId == "714" || oldFurniture.ItemId == "719")
							{
								StorageFurniture storageFurniture = new StorageFurniture(oldFurniture.ItemId, oldFurniture.TileLocation, oldFurniture.currentRotation.Value);
								furnitureToAdd.Add(storageFurniture);
								location.furniture.RemoveAt(j);
								j--;
							}
						}
						foreach (Furniture furniture in furnitureToAdd)
						{
							location.furniture.Add(furniture);
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.InferPreserves:
			{
				string[] preserveItemIndices = new string[]
				{
					"(O)350",
					"(O)348",
					"(O)344",
					"(O)342"
				};
				string[] suffixes = new string[]
				{
					" Juice",
					" Wine",
					" Jelly"
				};
				Object.PreserveType[] suffixPreserveTypes = new Object.PreserveType[]
				{
					Object.PreserveType.Juice,
					Object.PreserveType.Wine,
					Object.PreserveType.Jelly
				};
				string[] prefixes = new string[]
				{
					"Pickled "
				};
				Object.PreserveType[] prefixPreserveTypes = new Object.PreserveType[]
				{
					Object.PreserveType.Pickle
				};
				Utility.ForEachItem(delegate(Item item)
				{
					Object obj = item as Object;
					if (obj == null)
					{
						return true;
					}
					if (!Utility.IsNormalObjectAtParentSheetIndex(obj, obj.ItemId))
					{
						return true;
					}
					if (!preserveItemIndices.Contains(obj.QualifiedItemId))
					{
						return true;
					}
					if (obj.preserve.Value == null)
					{
						bool migrated = false;
						for (int j = 0; j < suffixes.Length; j++)
						{
							string suffix = suffixes[j];
							if (obj.Name.EndsWith(suffix))
							{
								string itemName = obj.Name.Substring(0, obj.Name.Length - suffix.Length);
								string preserveIndex = null;
								foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
								{
									if (data.InternalName == itemName)
									{
										preserveIndex = data.ItemId;
										break;
									}
								}
								if (preserveIndex != null)
								{
									obj.preservedParentSheetIndex.Value = preserveIndex;
									obj.preserve.Value = new Object.PreserveType?(suffixPreserveTypes[j]);
									migrated = true;
									break;
								}
							}
						}
						if (migrated)
						{
							return true;
						}
						for (int k = 0; k < prefixes.Length; k++)
						{
							string prefix = prefixes[k];
							if (obj.Name.StartsWith(prefix))
							{
								string itemName2 = obj.Name.Substring(prefix.Length);
								string preserveIndex2 = null;
								foreach (ParsedItemData data2 in ItemRegistry.GetObjectTypeDefinition().GetAllData())
								{
									if (data2.InternalName == itemName2)
									{
										preserveIndex2 = data2.ItemId;
										break;
									}
								}
								if (preserveIndex2 != null)
								{
									obj.preservedParentSheetIndex.Value = preserveIndex2;
									obj.preserve.Value = new Object.PreserveType?(prefixPreserveTypes[k]);
									break;
								}
							}
						}
					}
					return true;
				});
				return true;
			}
			case SaveFixes.TransferHatSkipHairFlag:
				Utility.ForEachItem(delegate(Item item)
				{
					Hat hat = item as Hat;
					if (hat != null && hat.skipHairDraw)
					{
						hat.hairDrawType.Set(0);
						hat.skipHairDraw = false;
					}
					return true;
				});
				return true;
			case SaveFixes.RevealSecretNoteItemTastes:
			{
				Dictionary<int, string> notesData = DataLoader.SecretNotes(Game1.content);
				for (int i = 0; i < 21; i++)
				{
					string note;
					if (notesData.TryGetValue(i, out note) && Game1.player.secretNotesSeen.Contains(i))
					{
						Utility.ParseGiftReveals(note);
					}
				}
				return true;
			}
			case SaveFixes.TransferHoneyTypeToPreserves:
				return true;
			case SaveFixes.TransferNoteBlockScale:
				Utility.ForEachItem(delegate(Item item)
				{
					Object obj = item as Object;
					if (obj != null && (obj.QualifiedItemId == "(O)363" || obj.QualifiedItemId == "(O)464"))
					{
						obj.preservedParentSheetIndex.Value = ((int)obj.scale.X).ToString();
					}
					return true;
				});
				return true;
			case SaveFixes.FixCropHarvestAmountsAndInferSeedIndex:
				return true;
			case SaveFixes.quarryMineBushes:
			{
				GameLocation mountain = Game1.RequireLocation("Mountain", false);
				mountain.largeTerrainFeatures.Add(new Bush(new Vector2(101f, 18f), 1, mountain, -1));
				mountain.largeTerrainFeatures.Add(new Bush(new Vector2(104f, 21f), 0, mountain, -1));
				mountain.largeTerrainFeatures.Add(new Bush(new Vector2(105f, 18f), 0, mountain, -1));
				return true;
			}
			case SaveFixes.MissingQisChallenge:
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer.mailReceived.Contains("skullCave") && !farmer.hasQuest("20") && !farmer.hasOrWillReceiveMail("QiChallengeComplete"))
					{
						farmer.addQuest("20");
					}
				}
				return true;
			case SaveFixes.AddTownBush:
			{
				Town town = Game1.getLocationFromName("Town") as Town;
				if (town != null)
				{
					Vector2 tile = new Vector2(61f, 93f);
					if (town.getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) == null)
					{
						town.largeTerrainFeatures.Add(new Bush(tile, 2, town, -1));
					}
				}
				return true;
			}
			}
			return false;
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x00141CE4 File Offset: 0x0013FEE4
		public static void ApplyLegacyChanges()
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				foreach (string npcName in farmer.friendshipData.Keys)
				{
					farmer.friendshipData[npcName].Points = Math.Min(farmer.friendshipData[npcName].Points, 3125);
				}
			}
			foreach (KeyValuePair<string, string> pair in Game1.netWorldState.Value.BundleData)
			{
				int key = Convert.ToInt32(pair.Key.Split('/', StringSplitOptions.None)[1]);
				if (!Game1.netWorldState.Value.Bundles.ContainsKey(key))
				{
					Game1.netWorldState.Value.Bundles.Add(key, new NetArray<bool, NetBool>(ArgUtility.SplitBySpace(pair.Value.Split('/', StringSplitOptions.None)[2]).Length));
				}
				if (!Game1.netWorldState.Value.BundleRewards.ContainsKey(key))
				{
					Game1.netWorldState.Value.BundleRewards.Add(key, new NetBool(false));
				}
			}
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				foreach (Item item2 in farmer2.Items)
				{
					if (item2 != null)
					{
						item2.HasBeenInInventory = true;
					}
				}
			}
			SaveMigrator_1_4.RecalculateLostBookCount();
			Utility.iterateChestsAndStorage(delegate(Item item)
			{
				item.HasBeenInInventory = true;
			});
			Game1.hasApplied1_4_UpdateChanges = true;
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x00141F28 File Offset: 0x00140128
		public static void RecalculateLostBookCount()
		{
			int highestLostBookCount = 0;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				int[] data;
				if (player.archaeologyFound.TryGetValue("102", out data) && data[0] > 0)
				{
					highestLostBookCount = Math.Max(highestLostBookCount, data[0]);
					player.mailForTomorrow.Add("lostBookFound%&NL&%");
				}
			}
			Game1.netWorldState.Value.LostBooksFound = highestLostBookCount;
		}
	}
}
