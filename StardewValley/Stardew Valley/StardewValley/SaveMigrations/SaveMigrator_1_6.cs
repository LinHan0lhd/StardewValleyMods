using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000187 RID: 391
	public class SaveMigrator_1_6 : ISaveMigrator
	{
		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06001C7E RID: 7294 RVA: 0x00142430 File Offset: 0x00140630
		public Version GameVersion { get; } = new Version(1, 5);

		// Token: 0x06001C7F RID: 7295 RVA: 0x00142438 File Offset: 0x00140638
		public bool ApplySaveFix(SaveFixes saveFix)
		{
			switch (saveFix)
			{
			case SaveFixes.MigrateBuildingsToData:
				Utility.ForEachBuilding(delegate(Building building)
				{
					JunimoHut hut = building as JunimoHut;
					if (hut != null && hut.obsolete_output != null)
					{
						hut.GetOutputChest().Items.AddRange(hut.obsolete_output.Items);
						hut.obsolete_output = null;
					}
					if (building.isUnderConstruction(false))
					{
						Game1.netWorldState.Value.MarkUnderConstruction("Robin", building);
						if (building.daysUntilUpgrade.Value > 0 && string.IsNullOrWhiteSpace(building.upgradeName.Value))
						{
							building.upgradeName.Value = SaveMigrator_1_6.InferBuildingUpgradingTo(building.buildingType.Value);
						}
					}
					return true;
				}, true);
				return true;
			case SaveFixes.ModularizeFarmhouse:
				Game1.getFarm().AddDefaultBuildings(true);
				return true;
			case SaveFixes.ModularizePets:
			{
				foreach (Farmer farmer3 in Game1.getAllFarmers())
				{
					farmer3.whichPetType = ((farmer3.obsolete_catPerson ?? false) ? "Cat" : "Dog");
					farmer3.obsolete_catPerson = null;
				}
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					for (int i2 = location.characters.Count - 1; i2 >= 0; i2--)
					{
						Pet existingPet = location.characters[i2] as Pet;
						if (existingPet != null)
						{
							string newPetType = null;
							if (existingPet.GetType() == typeof(Cat))
							{
								newPetType = "Cat";
							}
							else if (existingPet.GetType() == typeof(Dog))
							{
								newPetType = "Dog";
							}
							if (newPetType != null)
							{
								Pet newPet = new Pet((int)(existingPet.Position.X / 64f), (int)(existingPet.Position.X / 64f), existingPet.whichBreed.Value, newPetType);
								newPet.Name = existingPet.Name;
								newPet.displayName = existingPet.displayName;
								if (existingPet.currentLocation != null)
								{
									newPet.currentLocation = existingPet.currentLocation;
								}
								newPet.friendshipTowardFarmer.Value = existingPet.friendshipTowardFarmer.Value;
								newPet.grantedFriendshipForPet.Value = existingPet.grantedFriendshipForPet.Value;
								newPet.lastPetDay.Clear();
								newPet.lastPetDay.CopyFrom(existingPet.lastPetDay.Pairs);
								newPet.isSleepingOnFarmerBed.Value = existingPet.isSleepingOnFarmerBed.Value;
								newPet.modData.CopyFrom(existingPet.modData);
								location.characters[i2] = newPet;
							}
						}
					}
					return true;
				}, true, false);
				Farm farm = Game1.getFarm();
				farm.AddDefaultBuilding("Pet Bowl", farm.GetStarterPetBowlLocation(), true);
				PetBowl bowl = farm.getBuildingByType("Pet Bowl") as PetBowl;
				Pet pet = Game1.player.getPet();
				if (bowl != null && pet != null)
				{
					bowl.AssignPet(pet);
					pet.setAtFarmPosition();
				}
				return true;
			}
			case SaveFixes.AddNpcRemovalFlags:
			{
				GameLocation location2 = Game1.getLocationFromName("WitchSwamp");
				if (location2 != null && location2.getCharacterFromName("Henchman") == null)
				{
					Game1.addMail("henchmanGone", true, true);
				}
				location2 = Game1.getLocationFromName("SandyHouse");
				if (location2 != null && location2.getCharacterFromName("Bouncer") == null)
				{
					Game1.addMail("bouncerGone", true, true);
				}
				return true;
			}
			case SaveFixes.MigrateFarmhands:
				return true;
			case SaveFixes.MigrateLitterItemData:
				Utility.ForEachItem(delegate(Item item)
				{
					string qualifiedItemId = item.QualifiedItemId;
					uint num2 = <PrivateImplementationDetails>.ComputeStringHash(qualifiedItemId);
					if (num2 <= 1630881340U)
					{
						if (num2 <= 1066059619U)
						{
							if (num2 <= 633929998U)
							{
								if (num2 <= 264946568U)
								{
									if (num2 <= 231391330U)
									{
										if (num2 != 139884361U)
										{
											if (num2 != 231391330U)
											{
												return true;
											}
											if (!(qualifiedItemId == "(O)816"))
											{
												return true;
											}
										}
										else if (!(qualifiedItemId == "(O)668"))
										{
											return true;
										}
									}
									else if (num2 != 248168949U)
									{
										if (num2 != 264946568U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)818"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)817"))
									{
										return true;
									}
								}
								else if (num2 <= 565833784U)
								{
									if (num2 != 281724187U)
									{
										if (num2 != 565833784U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)784"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)819"))
									{
										return true;
									}
								}
								else if (num2 != 582611403U)
								{
									if (num2 != 599389022U)
									{
										if (num2 != 633929998U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)764"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)786"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)785"))
								{
									return true;
								}
							}
							else if (num2 <= 700201831U)
							{
								if (num2 <= 666646593U)
								{
									if (num2 != 650707617U)
									{
										if (num2 != 666646593U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)794"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)765"))
									{
										return true;
									}
								}
								else if (num2 != 667485236U)
								{
									if (num2 != 683424212U)
									{
										if (num2 != 700201831U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)792"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)793"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)762"))
								{
									return true;
								}
							}
							else if (num2 <= 997816310U)
							{
								if (num2 != 701040474U)
								{
									if (num2 != 997816310U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)32"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)760"))
								{
									return true;
								}
							}
							else if (num2 != 1031371548U)
							{
								if (num2 != 1064926786U)
								{
									if (num2 != 1066059619U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)44"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)36"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)34"))
							{
								return true;
							}
						}
						else if (num2 <= 1267391047U)
						{
							if (num2 <= 1115406738U)
							{
								if (num2 <= 1098776214U)
								{
									if (num2 != 1098482024U)
									{
										if (num2 != 1098776214U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)14"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)38"))
									{
										return true;
									}
								}
								else if (num2 != 1099614857U)
								{
									if (num2 != 1115406738U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)25"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)46"))
								{
									return true;
								}
							}
							else if (num2 <= 1133170095U)
							{
								if (num2 != 1132331452U)
								{
									if (num2 != 1133170095U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)40"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)12"))
								{
									return true;
								}
							}
							else if (num2 != 1165886690U)
							{
								if (num2 != 1166725333U)
								{
									if (num2 != 1267391047U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)48"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)42"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)10"))
							{
								return true;
							}
						}
						else if (num2 <= 1416986528U)
						{
							if (num2 <= 1364262521U)
							{
								if (num2 != 1330707283U)
								{
									if (num2 != 1364262521U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)0"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)2"))
								{
									return true;
								}
							}
							else if (num2 != 1385275665U)
							{
								if (num2 != 1397817759U)
								{
									if (num2 != 1416986528U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)450"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)6"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)95"))
							{
								return true;
							}
						}
						else if (num2 <= 1450541766U)
						{
							if (num2 != 1431372997U)
							{
								if (num2 != 1450541766U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)452"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)4"))
							{
								return true;
							}
						}
						else if (num2 != 1498483473U)
						{
							if (num2 != 1563770864U)
							{
								if (num2 != 1630881340U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)317"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)313"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)8"))
						{
							return true;
						}
					}
					else if (num2 <= 2664881560U)
					{
						if (num2 <= 2186503616U)
						{
							if (num2 <= 1714916530U)
							{
								if (num2 <= 1664436578U)
								{
									if (num2 != 1647658959U)
									{
										if (num2 != 1664436578U)
										{
											return true;
										}
										if (!(qualifiedItemId == "(O)315"))
										{
											return true;
										}
									}
									else if (!(qualifiedItemId == "(O)316"))
									{
										return true;
									}
								}
								else if (num2 != 1681214197U)
								{
									if (num2 != 1714916530U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)320"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)314"))
								{
									return true;
								}
							}
							else if (num2 <= 1731547054U)
							{
								if (num2 != 1715755173U)
								{
									if (num2 != 1731547054U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)319"))
									{
										return true;
									}
								}
								else
								{
									if (!(qualifiedItemId == "(O)372"))
									{
										return true;
									}
									item.Category = -4;
									Object clamObj = item as Object;
									if (clamObj != null)
									{
										clamObj.Type = "Fish";
										return true;
									}
									return true;
								}
							}
							else if (num2 != 1731694149U)
							{
								if (num2 != 1748324673U)
								{
									if (num2 != 2186503616U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)678"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)318"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)321"))
							{
								return true;
							}
						}
						else if (num2 <= 2421390282U)
						{
							if (num2 <= 2320724568U)
							{
								if (num2 != 2203281235U)
								{
									if (num2 != 2320724568U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)670"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)679"))
								{
									return true;
								}
							}
							else if (num2 != 2387835044U)
							{
								if (num2 != 2404612663U)
								{
									if (num2 != 2421390282U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)676"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)675"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)674"))
							{
								return true;
							}
						}
						else if (num2 <= 2598462632U)
						{
							if (num2 != 2438167901U)
							{
								if (num2 != 2598462632U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)883"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)677"))
							{
								return true;
							}
						}
						else if (num2 != 2615240251U)
						{
							if (num2 != 2631326322U)
							{
								if (num2 != 2664881560U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)847"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)849"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)882"))
						{
							return true;
						}
					}
					else if (num2 <= 3179789350U)
					{
						if (num2 <= 2715905965U)
						{
							if (num2 <= 2681659179U)
							{
								if (num2 != 2681512084U)
								{
									if (num2 != 2681659179U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)846"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)850"))
								{
									return true;
								}
							}
							else if (num2 != 2698436798U)
							{
								if (num2 != 2715214417U)
								{
									if (num2 != 2715905965U)
									{
										return true;
									}
									if (!(qualifiedItemId == "(O)884"))
									{
										return true;
									}
								}
								else if (!(qualifiedItemId == "(O)844"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)845"))
							{
								return true;
							}
						}
						else if (num2 <= 2731992036U)
						{
							if (num2 != 2730882110U)
							{
								if (num2 != 2731992036U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)843"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)751"))
							{
								return true;
							}
						}
						else if (num2 != 2747659729U)
						{
							if (num2 != 3146234112U)
							{
								if (num2 != 3179789350U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)50"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)52"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)750"))
						{
							return true;
						}
					}
					else if (num2 <= 3481492302U)
					{
						if (num2 <= 3246899826U)
						{
							if (num2 != 3213344588U)
							{
								if (num2 != 3246899826U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)54"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)56"))
							{
								return true;
							}
						}
						else if (num2 != 3314010302U)
						{
							if (num2 != 3464714683U)
							{
								if (num2 != 3481492302U)
								{
									return true;
								}
								if (!(qualifiedItemId == "(O)76"))
								{
									return true;
								}
							}
							else if (!(qualifiedItemId == "(O)75"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)58"))
						{
							return true;
						}
					}
					else if (num2 <= 3738940766U)
					{
						if (num2 != 3498269921U)
						{
							if (num2 != 3738940766U)
							{
								return true;
							}
							if (!(qualifiedItemId == "(O)294"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)77"))
						{
							return true;
						}
					}
					else if (num2 != 3755718385U)
					{
						if (num2 != 3806051242U)
						{
							if (num2 != 4114410237U)
							{
								return true;
							}
							if (!(qualifiedItemId == "(O)343"))
							{
								return true;
							}
						}
						else if (!(qualifiedItemId == "(O)290"))
						{
							return true;
						}
					}
					else if (!(qualifiedItemId == "(O)295"))
					{
						return true;
					}
					item.Category = -999;
					Object obj = item as Object;
					if (obj != null)
					{
						obj.Type = "Litter";
					}
					return true;
				});
				return true;
			case SaveFixes.MigrateHoneyItems:
				Utility.ForEachItem(delegate(Item item)
				{
					Object obj = item as Object;
					if (obj == null || obj.QualifiedItemId != "(O)340")
					{
						return true;
					}
					obj.preserve.Value = new Object.PreserveType?(Object.PreserveType.Honey);
					if (obj.preservedParentSheetIndex.Value == null || obj.preservedParentSheetIndex.Value == "0")
					{
						string flowerName = obj.obsolete_honeyType;
						if (string.IsNullOrWhiteSpace(flowerName) && obj.name.EndsWith(" Honey"))
						{
							flowerName = obj.name.Substring(0, obj.name.Length - " Honey".Length).Replace(" ", "");
						}
						if (!(flowerName == "Poppy"))
						{
							if (!(flowerName == "Tulip"))
							{
								if (!(flowerName == "SummerSpangle"))
								{
									if (!(flowerName == "FairyRose"))
									{
										if (!(flowerName == "BlueJazz"))
										{
											obj.Name = "Wild Honey";
											obj.preservedParentSheetIndex.Value = null;
										}
										else
										{
											obj.preservedParentSheetIndex.Value = "597";
										}
									}
									else
									{
										obj.preservedParentSheetIndex.Value = "595";
									}
								}
								else
								{
									obj.preservedParentSheetIndex.Value = "593";
								}
							}
							else
							{
								obj.preservedParentSheetIndex.Value = "591";
							}
						}
						else
						{
							obj.preservedParentSheetIndex.Value = "376";
						}
					}
					if (obj.Name == "Honey" && obj.preservedParentSheetIndex.Value == "-1")
					{
						obj.Name = "Wild Honey";
					}
					obj.obsolete_honeyType = null;
					return true;
				});
				return true;
			case SaveFixes.MigrateMachineLastOutputRule:
				Utility.ForEachItem(delegate(Item item)
				{
					Object machine = item as Object;
					if (machine != null)
					{
						SaveMigrator_1_6.InferMachineInputOutputFields(machine);
					}
					return true;
				});
				return true;
			case SaveFixes.StandardizeBundleFields:
				return true;
			case SaveFixes.MigrateAdventurerGoalFlags:
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
				dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
				dictionary["Gil_Savage Ring"] = "Gil_Shadows";
				dictionary["Gil_Vampire Ring"] = "Gil_Bats";
				dictionary["Gil_Skeleton Mask"] = "Gil_Skeletons";
				dictionary["Gil_Insect Head"] = "Gil_Insects";
				dictionary["Gil_Hard Hat"] = "Gil_Duggy";
				dictionary["Gil_Burglar's Ring"] = "Gil_DustSpirits";
				dictionary["Gil_Crabshell Ring"] = "Gil_Crabs";
				dictionary["Gil_Arcane Hat"] = "Gil_Mummies";
				dictionary["Gil_Knight's Helmet"] = "Gil_Dinos";
				dictionary["Gil_Napalm Ring"] = "Gil_Serpents";
				dictionary["Gil_Telephone"] = "Gil_FlameSpirits";
				Dictionary<string, string> map = dictionary;
				foreach (Farmer player in Game1.getAllFarmers())
				{
					foreach (NetStringHashSet mail in new NetStringHashSet[]
					{
						player.mailReceived,
						player.mailForTomorrow
					})
					{
						foreach (KeyValuePair<string, string> pair in map)
						{
							if (mail.Remove(pair.Key))
							{
								mail.Add(pair.Value);
							}
						}
					}
					IList<string> mailbox = Game1.mailbox;
					for (int i = 0; i < mailbox.Count; i++)
					{
						string newFlag;
						if (map.TryGetValue(mailbox[i], out newFlag))
						{
							mailbox[i] = newFlag;
						}
					}
				}
				return true;
			}
			case SaveFixes.SetCropSeedId:
			{
				Dictionary<string, string> seedsByHarvestId = new Dictionary<string, string>();
				foreach (KeyValuePair<string, CropData> pair2 in Game1.cropData)
				{
					string seedId = pair2.Key;
					string harvestId = pair2.Value.HarvestItemId;
					if (harvestId != null)
					{
						seedsByHarvestId.TryAdd(harvestId, seedId);
					}
				}
				Utility.ForEachCrop(delegate(Crop crop)
				{
					if (crop.netSeedIndex.Value == "-1")
					{
						crop.netSeedIndex.Value = null;
					}
					if (!string.IsNullOrWhiteSpace(crop.netSeedIndex.Value))
					{
						return true;
					}
					if (crop.isWildSeedCrop() || crop.forageCrop.Value)
					{
						return true;
					}
					string newSeedId;
					if (crop.indexOfHarvest.Value != null && seedsByHarvestId.TryGetValue(crop.indexOfHarvest.Value, out newSeedId))
					{
						crop.netSeedIndex.Value = newSeedId;
					}
					return true;
				});
				return true;
			}
			case SaveFixes.FixMineBoulderCollisions:
			{
				Mine mine = Game1.RequireLocation<Mine>("Mine", false);
				Vector2 tile = mine.GetBoulderPosition();
				Object boulder;
				if (mine.objects.TryGetValue(tile, out boulder) && boulder.QualifiedItemId == "(BC)78" && boulder.TileLocation == Vector2.Zero)
				{
					boulder.TileLocation = tile;
				}
				return true;
			}
			case SaveFixes.MigratePetAndPetBowlIds:
			{
				Pet pet2 = Game1.player.getPet();
				if (pet2 != null)
				{
					pet2.petId.Value = Guid.NewGuid();
					PetBowl bowl2 = (PetBowl)Game1.getFarm().getBuildingByType("Pet Bowl");
					if (bowl2 != null)
					{
						bowl2.AssignPet(pet2);
						pet2.setAtFarmPosition();
					}
				}
				return true;
			}
			case SaveFixes.MigrateHousePaint:
			{
				Farm farm2 = Game1.getFarm();
				if (farm2.housePaintColor.Value != null)
				{
					farm2.GetMainFarmHouse().netBuildingPaintColor.Value.CopyFrom(farm2.housePaintColor.Value);
					farm2.housePaintColor.Value = null;
				}
				return true;
			}
			case SaveFixes.MigrateShedFloorWallIds:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					Shed shed = location as Shed;
					if (shed != null)
					{
						string floorId;
						if (shed.appliedFloor.TryGetValue("Floor_0", out floorId))
						{
							shed.appliedFloor.Remove("Floor_0");
							shed.appliedFloor["Floor"] = floorId;
						}
						string wallId;
						if (shed.appliedWallpaper.TryGetValue("Wall_0", out wallId))
						{
							shed.appliedWallpaper.Remove("Wall_0");
							shed.appliedWallpaper["Wall"] = wallId;
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.MigrateItemIds:
				Utility.ForEachItem(delegate(Item item)
				{
					Boots boots = item as Boots;
					if (boots == null)
					{
						MeleeWeapon weapon = item as MeleeWeapon;
						if (weapon == null)
						{
							Fence fence = item as Fence;
							if (fence == null)
							{
								Slingshot slingshot = item as Slingshot;
								if (slingshot == null)
								{
									if (item is Torch)
									{
										if (item.itemId.Value != item.ParentSheetIndex.ToString())
										{
											item.itemId.Value = null;
										}
									}
								}
								else
								{
									slingshot.ItemId = null;
								}
							}
							else if (fence.obsolete_whichType != null)
							{
								item.itemId.Value = null;
							}
						}
						else
						{
							weapon.appearance.Value = ((!string.IsNullOrWhiteSpace(weapon.appearance.Value) && weapon.appearance.Value != "-1") ? ItemRegistry.ManuallyQualifyItemId(weapon.appearance.Value, "(W)", false) : null);
						}
					}
					else if (boots.appliedBootSheetIndex.Value == "-1")
					{
						boots.appliedBootSheetIndex.Value = null;
					}
					string itemId = item.ItemId;
					return true;
				});
				foreach (Farmer player2 in Game1.getAllFarmers())
				{
					NetStringIntArrayDictionary fishCaught = player2.fishCaught;
					if (fishCaught != null)
					{
						foreach (KeyValuePair<string, int[]> pair3 in fishCaught.Pairs.ToArray<KeyValuePair<string, int[]>>())
						{
							fishCaught.Remove(pair3.Key);
							fishCaught[ItemRegistry.ManuallyQualifyItemId(pair3.Key, "(O)", false)] = pair3.Value;
						}
					}
					if (player2.toolBeingUpgraded.Value != null)
					{
						int n = player2.toolBeingUpgraded.Value.InitialParentTileIndex;
						switch (n)
						{
						case 13:
							player2.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)CopperTrashCan", 1, 0, false);
							break;
						case 14:
							player2.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)SteelTrashCan", 1, 0, false);
							break;
						case 15:
							player2.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)GoldTrashCan", 1, 0, false);
							break;
						case 16:
							player2.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)IridiumTrashCan", 1, 0, false);
							break;
						}
					}
					if (!(player2.obsolete_isMale ?? player2.IsMale))
					{
						foreach (NetRef<Clothing> field in new NetRef<Clothing>[]
						{
							player2.shirtItem,
							player2.pantsItem
						})
						{
							Clothing clothing = field.Value;
							if (clothing != null)
							{
								int? obsolete_indexInTileSheetFemale = clothing.obsolete_indexInTileSheetFemale;
								int num = -1;
								if (obsolete_indexInTileSheetFemale.GetValueOrDefault() > num & obsolete_indexInTileSheetFemale != null)
								{
									int variantId = clothing.obsolete_indexInTileSheetFemale.Value;
									if (clothing.HasTypeId("(S)"))
									{
										variantId += 1000;
									}
									ItemMetadata variantData = ItemRegistry.GetMetadata(clothing.TypeDefinitionId + variantId.ToString());
									if (variantData.Exists())
									{
										Clothing newClothing = (Clothing)variantData.CreateItemOrErrorItem(1, 0);
										newClothing.clothesColor.Value = clothing.clothesColor.Value;
										newClothing.modData.CopyFrom(clothing.modData);
										field.Value = newClothing;
									}
								}
								clothing.obsolete_indexInTileSheetFemale = null;
							}
						}
					}
					foreach (Quest rawQuest in player2.questLog)
					{
						CraftingQuest quest = rawQuest as CraftingQuest;
						if (quest == null)
						{
							FishingQuest quest2 = rawQuest as FishingQuest;
							if (quest2 == null)
							{
								ItemDeliveryQuest quest3 = rawQuest as ItemDeliveryQuest;
								if (quest3 == null)
								{
									ItemHarvestQuest quest4 = rawQuest as ItemHarvestQuest;
									if (quest4 == null)
									{
										LostItemQuest quest5 = rawQuest as LostItemQuest;
										if (quest5 == null)
										{
											ResourceCollectionQuest quest6 = rawQuest as ResourceCollectionQuest;
											if (quest6 == null)
											{
												SecretLostItemQuest quest7 = rawQuest as SecretLostItemQuest;
												if (quest7 != null)
												{
													quest7.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest7.ItemId.Value, "(O)", false);
												}
											}
											else
											{
												quest6.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest6.ItemId.Value, "(O)", false);
											}
										}
										else
										{
											quest5.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest5.ItemId.Value, "(O)", false);
										}
									}
									else
									{
										quest4.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest4.ItemId.Value, "(O)", false);
									}
								}
								else
								{
									quest3.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest3.ItemId.Value, "(O)", false);
									if (quest3.dailyQuest.Value)
									{
										quest3.moneyReward.Value = quest3.GetGoldRewardPerItem(ItemRegistry.Create(quest3.ItemId.Value, 1, 0, false));
									}
								}
							}
							else
							{
								quest2.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest2.ItemId.Value, "(O)", false);
							}
						}
						else
						{
							quest.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest.ItemId.Value, quest.obsolete_isBigCraftable.GetValueOrDefault() ? "(BC)" : "(O)", false);
							quest.obsolete_isBigCraftable = null;
						}
					}
				}
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.itemToRemoveOnEnd.Value == "-1")
					{
						order.itemToRemoveOnEnd.Value = null;
					}
				}
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					IslandShrine shrine = location as IslandShrine;
					if (shrine != null)
					{
						shrine.AddMissingPedestals();
					}
					foreach (KeyValuePair<Vector2, Object> pair8 in location.objects.Pairs)
					{
						Fence fence = pair8.Value as Fence;
						if (fence != null && fence.obsolete_whichType != null)
						{
							fence.ItemId = null;
						}
					}
					foreach (TerrainFeature terrainFeature in location.terrainFeatures.Values)
					{
						FruitTree tree = terrainFeature as FruitTree;
						if (tree != null)
						{
							if (tree.obsolete_treeType != null)
							{
								string obsolete_treeType = tree.obsolete_treeType;
								if (obsolete_treeType == null)
								{
									goto IL_19A;
								}
								int length = obsolete_treeType.Length;
								if (length != 1)
								{
									goto IL_19A;
								}
								switch (obsolete_treeType[0])
								{
								case '0':
									tree.treeId.Value = "628";
									break;
								case '1':
									tree.treeId.Value = "629";
									break;
								case '2':
									tree.treeId.Value = "630";
									break;
								case '3':
									tree.treeId.Value = "631";
									break;
								case '4':
									tree.treeId.Value = "632";
									break;
								case '5':
									tree.treeId.Value = "633";
									break;
								case '6':
									goto IL_19A;
								case '7':
									tree.treeId.Value = "69";
									break;
								case '8':
									tree.treeId.Value = "835";
									break;
								default:
									goto IL_19A;
								}
								IL_1AD:
								tree.obsolete_treeType = null;
								goto IL_1B5;
								IL_19A:
								tree.treeId.Value = tree.obsolete_treeType;
								goto IL_1AD;
							}
							IL_1B5:
							if (tree.obsolete_fruitsOnTree != null)
							{
								bool wasGreenhouse = tree.Location.IsGreenhouse;
								try
								{
									tree.Location.IsGreenhouse = true;
									int i2 = 0;
									for (;;)
									{
										int num2 = i2;
										int? obsolete_fruitsOnTree = tree.obsolete_fruitsOnTree;
										if (!(num2 < obsolete_fruitsOnTree.GetValueOrDefault() & obsolete_fruitsOnTree != null))
										{
											break;
										}
										tree.TryAddFruit();
										i2++;
									}
								}
								finally
								{
									tree.Location.IsGreenhouse = wasGreenhouse;
								}
								tree.obsolete_fruitsOnTree = null;
							}
						}
					}
					foreach (Building building in location.buildings)
					{
						FishPond fishPond = building as FishPond;
						if (fishPond != null && fishPond.fishType.Value == "-1")
						{
							fishPond.fishType.Value = null;
						}
					}
					foreach (FarmAnimal animal in location.animals.Values)
					{
						if (animal.currentProduce.Value == "-1")
						{
							animal.currentProduce.Value = null;
							animal.ReloadTextureIfNeeded(false);
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.RemoveMeatFromAnimalBundle:
			{
				string rawData;
				if (Game1.netWorldState.Value.BundleData.TryGetValue("Pantry/4", out rawData) && rawData.StartsWith("Animal/"))
				{
					string[] fields = rawData.Split('/', StringSplitOptions.None);
					List<string> ingredients = ArgUtility.SplitBySpace(ArgUtility.Get(rawData.Split('/', StringSplitOptions.None), 2, null, true)).ToList<string>();
					for (int j = 0; j < ingredients.Count; j += 3)
					{
						string id = ingredients[j];
						if ((id == "639" || id == "640" || id == "641" || id == "642" || id == "643") && ItemRegistry.ResolveMetadata("(O)" + id) == null)
						{
							ingredients.RemoveRange(j, Math.Min(3, ingredients.Count - 1));
							j -= 3;
						}
					}
					fields[2] = string.Join(" ", ingredients);
					Game1.netWorldState.Value.BundleData["Pantry/4"] = string.Join("/", fields);
					bool[] values;
					if (Game1.netWorldState.Value.Bundles.TryGetValue(4, out values) && values.Length > ingredients.Count)
					{
						Array.Resize<bool>(ref values, ingredients.Count);
						Game1.netWorldState.Value.Bundles.Remove(4);
						Game1.netWorldState.Value.Bundles.Add(4, values);
					}
				}
				return true;
			}
			case SaveFixes.RemoveMasteryRoomFoliage:
			{
				GameLocation forest = Game1.getLocationFromName("Forest");
				if (forest != null)
				{
					forest.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.Tile == new Vector2(100f, 74f) || feature.Tile == new Vector2(101f, 76f));
					Tree t = forest.terrainFeatures.GetValueOrDefault(new Vector2(98f, 75f), null) as Tree;
					Object o;
					if (t != null && t.tapped.Value && forest.objects.TryGetValue(new Vector2(98f, 75f), out o))
					{
						if (o.readyForHarvest.Value && o.heldObject != null)
						{
							Game1.player.team.returnedDonations.Add(o.heldObject.Value);
						}
						Game1.player.team.returnedDonations.Add(o);
						Game1.player.team.newLostAndFoundItems.Value = true;
					}
					forest.terrainFeatures.Remove(new Vector2(98f, 75f));
				}
				return true;
			}
			case SaveFixes.AddTownTrees:
			{
				GameLocation town = Game1.getLocationFromName("Town");
				Map map2 = town.map;
				Layer pathsLayer = (map2 != null) ? map2.GetLayer("Paths") : null;
				if (pathsLayer == null)
				{
					return false;
				}
				for (int x = 0; x < town.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < town.map.Layers[0].LayerHeight; y++)
					{
						Tile t2 = pathsLayer.Tiles[x, y];
						if (t2 != null)
						{
							Vector2 tile2 = new Vector2((float)x, (float)y);
							int? obsolete_indexInTileSheetFemale;
							string treeId;
							int? growthStageOnLoad;
							bool isFruitTree;
							if (town.TryGetTreeIdForTile(t2, out treeId, out growthStageOnLoad, out obsolete_indexInTileSheetFemale, out isFruitTree) && town.GetFurnitureAt(tile2) == null && !town.terrainFeatures.ContainsKey(tile2) && !town.objects.ContainsKey(tile2))
							{
								if (isFruitTree)
								{
									town.terrainFeatures.Add(tile2, new FruitTree(treeId, growthStageOnLoad.GetValueOrDefault(4)));
								}
								else
								{
									town.terrainFeatures.Add(tile2, new Tree(treeId, growthStageOnLoad.GetValueOrDefault(5), false));
								}
							}
						}
					}
				}
				return true;
			}
			case SaveFixes.MapAdjustments_1_6:
			{
				Game1.getLocationFromName("BusStop").shiftContents(10, 0, null);
				List<Point> list = new List<Point>();
				list.Add(new Point(78, 17));
				list.Add(new Point(79, 17));
				list.Add(new Point(79, 18));
				list.Add(new Point(80, 17));
				list.Add(new Point(80, 18));
				list.Add(new Point(80, 19));
				list.Add(new Point(81, 16));
				list.Add(new Point(81, 17));
				list.Add(new Point(81, 18));
				list.Add(new Point(81, 19));
				list.Add(new Point(82, 15));
				list.Add(new Point(82, 16));
				list.Add(new Point(82, 17));
				list.Add(new Point(82, 18));
				list.Add(new Point(83, 13));
				list.Add(new Point(83, 14));
				list.Add(new Point(83, 15));
				list.Add(new Point(83, 16));
				list.Add(new Point(83, 17));
				list.Add(new Point(84, 13));
				list.Add(new Point(84, 14));
				list.Add(new Point(84, 15));
				list.Add(new Point(84, 16));
				list.Add(new Point(84, 17));
				list.Add(new Point(84, 18));
				list.Add(new Point(85, 13));
				list.Add(new Point(85, 14));
				list.Add(new Point(85, 15));
				list.Add(new Point(85, 16));
				list.Add(new Point(85, 17));
				list.Add(new Point(85, 18));
				list.Add(new Point(86, 14));
				list.Add(new Point(86, 15));
				list.Add(new Point(86, 16));
				list.Add(new Point(86, 17));
				list.Add(new Point(86, 18));
				list.Add(new Point(87, 14));
				list.Add(new Point(87, 15));
				list.Add(new Point(87, 16));
				list.Add(new Point(87, 17));
				list.Add(new Point(87, 18));
				list.Add(new Point(87, 19));
				list.Add(new Point(88, 13));
				list.Add(new Point(88, 14));
				list.Add(new Point(88, 15));
				list.Add(new Point(88, 16));
				list.Add(new Point(88, 17));
				list.Add(new Point(88, 18));
				list.Add(new Point(88, 19));
				list.Add(new Point(89, 13));
				list.Add(new Point(89, 14));
				list.Add(new Point(89, 15));
				list.Add(new Point(89, 16));
				list.Add(new Point(89, 17));
				list.Add(new Point(79, 21));
				list.Add(new Point(79, 22));
				list.Add(new Point(79, 23));
				list.Add(new Point(79, 24));
				list.Add(new Point(79, 25));
				list.Add(new Point(76, 16));
				list.Add(new Point(75, 16));
				list.Add(new Point(74, 16));
				GameLocation mountain2 = Game1.getLocationFromName("Mountain");
				foreach (Point p in list)
				{
					mountain2.cleanUpTileForMapOverride(p);
				}
				mountain2.terrainFeatures.Remove(new Vector2(79f, 20f));
				mountain2.terrainFeatures.Remove(new Vector2(79f, 19f));
				mountain2.terrainFeatures.Remove(new Vector2(79f, 16f));
				mountain2.terrainFeatures.Remove(new Vector2(80f, 20f));
				mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(82, 11));
				mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(86, 13));
				mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(85, 16));
				mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(81f, 9f), 1, mountain2, -1));
				mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(84f, 18f), 2, mountain2, -1));
				mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(87f, 19f), 1, mountain2, -1));
				List<Point> list2 = new List<Point>();
				list2.Add(new Point(92, 10));
				list2.Add(new Point(93, 10));
				list2.Add(new Point(94, 10));
				list2.Add(new Point(93, 13));
				list2.Add(new Point(95, 13));
				list2.Add(new Point(92, 5));
				list2.Add(new Point(92, 6));
				list2.Add(new Point(97, 9));
				list2.Add(new Point(91, 10));
				list2.Add(new Point(91, 9));
				list2.Add(new Point(91, 8));
				list2.Add(new Point(93, 11));
				list2.Add(new Point(94, 11));
				list2.Add(new Point(95, 11));
				GameLocation town2 = Game1.getLocationFromName("Town");
				foreach (Point p2 in list2)
				{
					town2.cleanUpTileForMapOverride(p2);
				}
				town2.loadPathsLayerObjectsInArea(103, 16, 16, 27);
				town2.loadPathsLayerObjectsInArea(120, 57, 7, 12);
				town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(105, 42));
				town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(108, 42));
				List<Point> list3 = new List<Point>();
				list3.Add(new Point(63, 77));
				list3.Add(new Point(63, 78));
				list3.Add(new Point(63, 79));
				list3.Add(new Point(63, 80));
				list3.Add(new Point(46, 26));
				list3.Add(new Point(46, 27));
				list3.Add(new Point(46, 28));
				list3.Add(new Point(46, 29));
				GameLocation forest2 = Game1.getLocationFromName("Forest");
				foreach (Point p3 in list3)
				{
					forest2.cleanUpTileForMapOverride(p3);
				}
				forest2.largeTerrainFeatures.Add(new Bush(new Vector2(54f, 8f), 0, forest2, -1));
				forest2.largeTerrainFeatures.Add(new Bush(new Vector2(58f, 8f), 0, forest2, -1));
				return true;
			}
			case SaveFixes.MigrateWalletItems:
			{
				Farmer player3 = Game1.MasterPlayer;
				player3.hasRustyKey = (player3.hasRustyKey || (player3.obsolete_hasRustyKey ?? false));
				player3.hasSkullKey = (player3.hasSkullKey || (player3.obsolete_hasSkullKey ?? false));
				player3.canUnderstandDwarves = (player3.canUnderstandDwarves || (player3.obsolete_canUnderstandDwarves ?? false));
				player3.obsolete_hasRustyKey = null;
				player3.obsolete_hasSkullKey = null;
				player3.obsolete_canUnderstandDwarves = null;
				foreach (Farmer player4 in Game1.getAllFarmers())
				{
					player4.hasClubCard = (player4.hasClubCard || (player4.obsolete_hasClubCard ?? false));
					player4.hasDarkTalisman = (player4.hasDarkTalisman || (player4.obsolete_hasDarkTalisman ?? false));
					player4.hasMagicInk = (player4.hasMagicInk || (player4.obsolete_hasMagicInk ?? false));
					player4.hasMagnifyingGlass = (player4.hasMagnifyingGlass || (player4.obsolete_hasMagnifyingGlass ?? false));
					player4.hasSpecialCharm = (player4.hasSpecialCharm || (player4.obsolete_hasSpecialCharm ?? false));
					player4.HasTownKey = (player4.HasTownKey || (player4.obsolete_hasTownKey ?? false));
					player4.hasUnlockedSkullDoor = (player4.hasUnlockedSkullDoor || (player4.obsolete_hasUnlockedSkullDoor ?? false));
					player4.obsolete_hasClubCard = null;
					player4.obsolete_hasDarkTalisman = null;
					player4.obsolete_hasMagicInk = null;
					player4.obsolete_hasMagnifyingGlass = null;
					player4.obsolete_hasSpecialCharm = null;
					player4.obsolete_hasTownKey = null;
					player4.obsolete_hasUnlockedSkullDoor = null;
					player4.obsolete_daysMarried = null;
				}
				return true;
			}
			case SaveFixes.MigrateResourceClumps:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					Forest forest3 = location as Forest;
					if (forest3 == null)
					{
						Woods woods = location as Woods;
						if (woods != null)
						{
							woods.DayUpdate(Game1.dayOfMonth);
						}
					}
					else if (forest3.obsolete_log != null)
					{
						forest3.resourceClumps.Add(forest3.obsolete_log);
						forest3.obsolete_log = null;
					}
					return true;
				}, false, false);
				return true;
			case SaveFixes.MigrateFishingRodAttachmentSlots:
				Utility.ForEachItem(delegate(Item item)
				{
					FishingRod rod = item as FishingRod;
					if (rod != null)
					{
						ToolData data = rod.GetToolData();
						if (data == null || data.AttachmentSlots < 0 || rod.AttachmentSlotsCount <= data.AttachmentSlots)
						{
							return true;
						}
						INetSerializable parent = rod.attachments.Parent;
						rod.attachments.Parent = null;
						try
						{
							int slot = rod.AttachmentSlotsCount - 1;
							while (rod.AttachmentSlotsCount > data.AttachmentSlots && slot >= 0)
							{
								if (rod.attachments.Count <= slot)
								{
									FishingRod fishingRod = rod;
									int attachmentSlotsCount = fishingRod.AttachmentSlotsCount;
									fishingRod.AttachmentSlotsCount = attachmentSlotsCount - 1;
								}
								else if (rod.attachments[slot] == null)
								{
									FishingRod fishingRod2 = rod;
									int attachmentSlotsCount = fishingRod2.AttachmentSlotsCount;
									fishingRod2.AttachmentSlotsCount = attachmentSlotsCount - 1;
								}
								slot--;
							}
						}
						finally
						{
							rod.attachments.Parent = parent;
						}
					}
					return true;
				});
				return true;
			case SaveFixes.MoveSlimeHutches:
			{
				Farm farm3 = Game1.getFarm();
				for (int k = farm3.buildings.Count - 1; k >= 0; k--)
				{
					if (farm3.buildings[k].buildingType.Value == "Slime Hutch")
					{
						farm3.buildings[k].tileX.Value += 2;
						farm3.buildings[k].tileY.Value += 2;
						farm3.buildings[k].ReloadBuildingData(false, false);
						farm3.buildings[k].updateInteriorWarps(null);
					}
				}
				return true;
			}
			case SaveFixes.AddLocationsVisited:
				foreach (Farmer who in Game1.getAllFarmers())
				{
					NetStringHashSet visited = who.locationsVisited;
					Farmer mainPlayer = Game1.MasterPlayer;
					visited.AddRange(new string[]
					{
						"Farm",
						"FarmHouse",
						"FarmCave",
						"Cellar",
						"Town",
						"JoshHouse",
						"HaleyHouse",
						"SamHouse",
						"Blacksmith",
						"ManorHouse",
						"SeedShop",
						"Saloon",
						"Trailer",
						"Hospital",
						"HarveyRoom",
						"ArchaeologyHouse",
						"JojaMart",
						"Beach",
						"ElliottHouse",
						"FishShop",
						"Mountain",
						"ScienceHouse",
						"SebastianRoom",
						"Tent",
						"Forest",
						"AnimalShop",
						"LeahHouse",
						"Backwoods",
						"BusStop",
						"Tunnel"
					});
					if (mainPlayer.mailReceived.Contains("ccPantry"))
					{
						visited.Add("Greenhouse");
					}
					if (Game1.isLocationAccessible("CommunityCenter"))
					{
						visited.Add("CommunityCenter");
					}
					if (who.eventsSeen.Contains("100162"))
					{
						visited.Add("Mine");
					}
					if (mainPlayer.mailReceived.Contains("ccVault"))
					{
						visited.AddRange(new string[]
						{
							"Desert",
							"SkullCave"
						});
					}
					if (who.eventsSeen.Contains("67"))
					{
						visited.Add("SandyHouse");
					}
					if (mainPlayer.mailReceived.Contains("bouncerGone"))
					{
						visited.Add("Club");
					}
					if (Game1.isLocationAccessible("Railroad"))
					{
						visited.AddRange(new string[]
						{
							"Railroad",
							"BathHouse_Entry",
							who.IsMale ? "BathHouse_MensLocker" : "BathHouse_WomensLocker",
							"BathHouse_Pool"
						});
					}
					if (mainPlayer.mailReceived.Contains("Farm_Eternal"))
					{
						visited.Add("Summit");
					}
					if (mainPlayer.mailReceived.Contains("witchStatueGone"))
					{
						visited.AddRange(new string[]
						{
							"WitchSwamp",
							"WitchWarpCave"
						});
					}
					if (mainPlayer.mailReceived.Contains("henchmanGone"))
					{
						visited.Add("WitchHut");
					}
					if (who.mailReceived.Contains("beenToWoods"))
					{
						visited.Add("Woods");
					}
					if (Forest.isWizardHouseUnlocked())
					{
						visited.Add("WizardHouse");
						if (who.getFriendshipHeartLevelForNPC("Wizard") >= 4)
						{
							visited.Add("WizardHouseBasement");
						}
					}
					if (who.mailReceived.Add("guildMember"))
					{
						visited.Add("AdventureGuild");
					}
					if (who.mailReceived.Contains("OpenedSewer"))
					{
						visited.Add("Sewer");
					}
					if (who.mailReceived.Contains("krobusUnseal"))
					{
						visited.Add("BugLand");
					}
					if (mainPlayer.mailReceived.Contains("abandonedJojaMartAccessible"))
					{
						visited.Add("AbandonedJojaMart");
					}
					if (mainPlayer.mailReceived.Contains("ccMovieTheater"))
					{
						visited.Add("MovieTheater");
					}
					if (mainPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						visited.Add("Trailer_Big");
					}
					if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
					{
						visited.Add("Sunroom");
					}
					if (Game1.year > 1 || (Game1.season == Season.Winter && Game1.dayOfMonth >= 15))
					{
						visited.AddRange(new string[]
						{
							"BeachNightMarket",
							"MermaidHouse",
							"Submarine"
						});
					}
					if (who.mailReceived.Contains("willyBackRoomInvitation"))
					{
						visited.Add("BoatTunnel");
					}
					if (who.mailReceived.Contains("Visited_Island"))
					{
						visited.AddRange(new string[]
						{
							"IslandSouth",
							"IslandEast",
							"IslandHut",
							"IslandShrine"
						});
						if (mainPlayer.mailReceived.Contains("Island_FirstParrot"))
						{
							visited.AddRange(new string[]
							{
								"IslandNorth",
								"IslandFieldOffice"
							});
						}
						if (mainPlayer.mailReceived.Contains("islandNorthCaveOpened"))
						{
							visited.Add("IslandNorthCave1");
						}
						if (mainPlayer.mailReceived.Contains("reachedCaldera"))
						{
							visited.Add("Caldera");
						}
						if (mainPlayer.mailReceived.Contains("Island_Turtle"))
						{
							visited.AddRange(new string[]
							{
								"IslandWest",
								"IslandWestCave1"
							});
						}
						if (mainPlayer.mailReceived.Contains("Island_UpgradeHouse"))
						{
							visited.AddRange(new string[]
							{
								"IslandFarmHouse",
								"IslandFarmCave"
							});
						}
						if (mainPlayer.team.collectedNutTracker.Contains("Bush_CaptainRoom_2_4"))
						{
							visited.Add("CaptainRoom");
						}
						int n;
						if (IslandWest.IsQiWalnutRoomDoorUnlocked(out n))
						{
							visited.Add("QiNutRoom");
						}
						if (mainPlayer.mailReceived.Contains("Island_Resort"))
						{
							visited.AddRange(new string[]
							{
								"IslandSouthEast",
								"IslandSouthEastCave"
							});
						}
					}
					if (mainPlayer.mailReceived.Contains("leoMoved"))
					{
						visited.Add("LeoTreeHouse");
					}
				}
				return true;
			case SaveFixes.MarkStarterGiftBoxes:
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location is FarmHouse)
					{
						foreach (Object @object in location.objects.Values)
						{
							Chest chest = @object as Chest;
							if (chest != null && chest.giftbox.Value && !chest.playerChest.Value)
							{
								chest.giftboxIsStarterGift.Value = true;
							}
						}
					}
					return true;
				}, true, false);
				return true;
			case SaveFixes.MigrateMailEventsToTriggerActions:
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				dictionary2["2346097"] = "Mail_Abigail_8heart";
				dictionary2["2346096"] = "Mail_Penny_10heart";
				dictionary2["2346095"] = "Mail_Elliott_8heart";
				dictionary2["2346094"] = "Mail_Elliott_10heart";
				dictionary2["3333094"] = "Mail_Pierre_ExtendedHours";
				dictionary2["2346093"] = "Mail_Harvey_10heart";
				dictionary2["2346092"] = "Mail_Sam_10heart";
				dictionary2["2346091"] = "Mail_Alex_10heart";
				dictionary2["68"] = "Mail_Mom_5K";
				dictionary2["69"] = "Mail_Mom_15K";
				dictionary2["70"] = "Mail_Mom_32K";
				dictionary2["71"] = "Mail_Mom_120K";
				dictionary2["72"] = "Mail_Dad_5K";
				dictionary2["73"] = "Mail_Dad_15K";
				dictionary2["74"] = "Mail_Dad_32K";
				dictionary2["75"] = "Mail_Dad_120K";
				dictionary2["76"] = "Mail_Tribune_UpAndComing";
				dictionary2["706"] = "Mail_Pierre_Fertilizers";
				dictionary2["707"] = "Mail_Pierre_FertilizersHighQuality";
				dictionary2["909"] = "Mail_Robin_Woodchipper";
				dictionary2["3872126"] = "Mail_Willy_BackRoomUnlocked";
				Dictionary<string, string> migrateFromEvents = dictionary2;
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				dictionary3["2111194"] = "Mail_Emily_8heart";
				dictionary3["2111294"] = "Mail_Emily_10heart";
				dictionary3["3912126"] = "Mail_Elliott_Tour1";
				dictionary3["3912127"] = "Mail_Elliott_Tour2";
				dictionary3["3912128"] = "Mail_Elliott_Tour3";
				dictionary3["3912129"] = "Mail_Elliott_Tour4";
				dictionary3["3912130"] = "Mail_Elliott_Tour5";
				dictionary3["3912131"] = "Mail_Elliott_Tour6";
				Dictionary<string, string> duplicateFromEvents = dictionary3;
				foreach (Farmer farmer4 in Game1.getAllFarmers())
				{
					NetStringHashSet events = farmer4.eventsSeen;
					NetStringHashSet actions = farmer4.triggerActionsRun;
					foreach (KeyValuePair<string, string> pair4 in migrateFromEvents)
					{
						if (events.Remove(pair4.Key))
						{
							actions.Add(pair4.Value);
						}
					}
					foreach (KeyValuePair<string, string> pair5 in duplicateFromEvents)
					{
						if (events.Contains(pair5.Key))
						{
							actions.Add(pair5.Value);
						}
					}
				}
				return true;
			}
			case SaveFixes.ShiftFarmHouseFurnitureForExpansion:
			{
				Vector2 tile;
				FarmHouse house;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					FarmHouse house = location as FarmHouse;
					if (house != null && house.upgradeLevel >= 2)
					{
						house.shiftContents(15, 10, delegate(Vector2 tile, object entity)
						{
							if (entity is BedFurniture)
							{
								int x2 = (int)tile.X;
								int y2 = (int)tile.Y;
								return house.doesTileHaveProperty(x2, y2, "DefaultBedPosition", "Back", false) == null && house.doesTileHaveProperty(x2, y2, "DefaultChildBedPosition", "Back", false) == null;
							}
							Furniture furniture = entity as Furniture;
							if (furniture != null && furniture.QualifiedItemId == "(F)1792")
							{
								Vector2 diff = tile - Utility.PointToVector2(house.getFireplacePoint());
								return Math.Abs(diff.X) > 1E-05f || Math.Abs(diff.Y) > 1E-05f;
							}
							return true;
						});
						foreach (NPC c in house.characters)
						{
							if (!c.TilePoint.Equals(house.getKitchenStandingSpot()))
							{
								c.Position += new Vector2(15f, 10f) * 64f;
							}
							if (house.hasTileAt(c.TilePoint, "Buildings", null) || !house.hasTileAt(c.TilePoint, "Back", null))
							{
								Vector2 v = Utility.recursiveFindOpenTileForCharacter(c, house, Utility.PointToVector2(house.getKitchenStandingSpot()), 99, false);
								if (v != Vector2.Zero)
								{
									c.setTileLocation(v);
								}
								else
								{
									c.setTileLocation(Utility.PointToVector2(house.getKitchenStandingSpot()));
								}
							}
						}
					}
					return true;
				}, true, false);
				foreach (Farmer f in Game1.getAllFarmers())
				{
					house = (f.currentLocation as FarmHouse);
					if (house != null && house.upgradeLevel >= 2)
					{
						f.Position += new Vector2(15f, 10f) * 64f;
					}
				}
				return true;
			}
			case SaveFixes.MigratePreservesTo16:
			{
				SaveMigrator_1_6.<>c__DisplayClass3_2 CS$<>8__locals2 = new SaveMigrator_1_6.<>c__DisplayClass3_2();
				CS$<>8__locals2.objTypeDefinition = ItemRegistry.GetObjectTypeDefinition();
				Utility.ForEachItemContext(new ForEachItemDelegate(CS$<>8__locals2.<ApplySaveFix>g__HandleItem|24));
				return true;
			}
			case SaveFixes.MigrateQuestDataTo16:
			{
				Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(SaveMigrator_1_6.LegacyDescriptionElement), new Type[]
				{
					typeof(DescriptionElement),
					typeof(Character),
					typeof(Item)
				}));
				foreach (Farmer farmer5 in Game1.getAllFarmers())
				{
					foreach (Quest quest8 in farmer5.questLog)
					{
						FieldInfo[] fields2 = quest8.GetType().GetFields();
						int n = 0;
						while (n < fields2.Length)
						{
							FieldInfo field2 = fields2[n];
							if (!(field2.FieldType == typeof(NetDescriptionElementList)))
							{
								goto IL_24E7;
							}
							NetDescriptionElementList fieldValue = (NetDescriptionElementList)field2.GetValue(quest8);
							if (fieldValue != null)
							{
								using (NetList<DescriptionElement, NetDescriptionElementRef>.Enumerator enumerator7 = fieldValue.GetEnumerator())
								{
									while (enumerator7.MoveNext())
									{
										DescriptionElement entry = enumerator7.Current;
										SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, entry);
									}
									goto IL_2524;
								}
								goto IL_24E7;
							}
							IL_2524:
							n++;
							continue;
							IL_24E7:
							if (field2.FieldType == typeof(NetDescriptionElementRef))
							{
								NetDescriptionElementRef fieldValue2 = (NetDescriptionElementRef)field2.GetValue(quest8);
								SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, (fieldValue2 != null) ? fieldValue2.Value : null);
								goto IL_2524;
							}
							goto IL_2524;
						}
					}
				}
				return true;
			}
			case SaveFixes.SetBushesInPots:
				Utility.ForEachItem(delegate(Item item)
				{
					IndoorPot pot = item as IndoorPot;
					if (pot != null && pot.bush.Value != null)
					{
						pot.bush.Value.inPot.Value = true;
					}
					return true;
				});
				return true;
			case SaveFixes.FixItemsNotMarkedAsInInventory:
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					foreach (Item item3 in farmer.GetEquippedItems())
					{
						item3.HasBeenInInventory = true;
					}
					foreach (Item item4 in farmer.Items)
					{
						if (item4 != null)
						{
							item4.HasBeenInInventory = true;
						}
					}
				}
				return true;
			case SaveFixes.BetaFixesFor16:
				Utility.ForEachItem(delegate(Item item)
				{
					if (item is Boots || item is Clothing || item is Hat)
					{
						item.FixStackSize();
					}
					return true;
				});
				return true;
			case SaveFixes.FixBasicWines:
				Utility.ForEachItem(delegate(Item item)
				{
					if (item.ParentSheetIndex == 348 && item.QualifiedItemId.Equals("(O)348"))
					{
						item.ParentSheetIndex = 123;
					}
					return true;
				});
				return true;
			case SaveFixes.ResetForges_1_6:
				SaveMigrator_1_5.ResetForges();
				return true;
			case SaveFixes.RestoreAncientSeedRecipe_1_6:
				foreach (Farmer farmer2 in Game1.getAllFarmers())
				{
					if (farmer2.mailReceived.Contains("museumCollectedRewardO_499_1"))
					{
						farmer2.craftingRecipes.TryAdd("Ancient Seeds", 0);
					}
				}
				return true;
			case SaveFixes.FixInstancedInterior:
			{
				string id;
				Utility.ForEachBuilding(delegate(Building building)
				{
					if (building.GetIndoorsType() == IndoorsType.Instanced)
					{
						GameLocation indoors = building.GetIndoors();
						if (indoors.uniqueName.Value == null)
						{
							NetFieldBase<string, NetString> uniqueName = indoors.uniqueName;
							BuildingData data = building.GetData();
							uniqueName.Value = (((data != null) ? data.IndoorMap : null) ?? indoors.Name) + GuidHelper.NewGuid().ToString();
						}
						AnimalHouse animalHouse = indoors as AnimalHouse;
						if (animalHouse != null)
						{
							animalHouse.animalsThatLiveHere.RemoveWhere(delegate(long id)
							{
								FarmAnimal animal = Utility.getAnimal(id);
								return ((animal != null) ? animal.home : null) != building;
							});
						}
					}
					return true;
				}, true);
				return true;
			}
			case SaveFixes.FixNonInstancedInterior:
				Utility.ForEachBuilding(delegate(Building building)
				{
					if (building.GetIndoorsType() == IndoorsType.Global)
					{
						building.GetIndoors().uniqueName.Value = null;
					}
					return true;
				}, true);
				return true;
			case SaveFixes.PopulateConstructedBuildings:
				Utility.ForEachBuilding(delegate(Building building)
				{
					if (!string.IsNullOrWhiteSpace(building.buildingType.Value))
					{
						if (!building.isUnderConstruction(false))
						{
							Game1.player.team.constructedBuildings.Add(building.buildingType.Value);
						}
						BuildingData data = building.GetData();
						while (!string.IsNullOrWhiteSpace((data != null) ? data.BuildingToUpgrade : null))
						{
							Game1.player.team.constructedBuildings.Add(data.BuildingToUpgrade);
							Building.TryGetData(data.BuildingToUpgrade, out data);
						}
					}
					return true;
				}, false);
				return true;
			case SaveFixes.FixRacoonQuestCompletion:
				if (NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
				{
					Game1.player.removeQuest("134");
					foreach (Farmer farmer6 in Game1.getOfflineFarmhands())
					{
						farmer6.removeQuest("134");
					}
				}
				return true;
			case SaveFixes.RestoreDwarvish:
				if (Game1.player.hasOrWillReceiveMail("museumCollectedRewardO_326_1"))
				{
					Game1.player.canUnderstandDwarves = true;
				}
				return true;
			case SaveFixes.FixTubOFlowers:
				Utility.ForEachItem(delegate(Item item)
				{
					if (item.QualifiedItemId == "(BC)109")
					{
						item.ItemId = "108";
						item.ResetParentSheetIndex();
						Object obj = item as Object;
						if (obj != null)
						{
							GameLocation location3 = obj.Location;
							bool? flag = (location3 != null) ? new bool?(location3.IsOutdoors) : null;
							if (flag != null && flag.GetValueOrDefault())
							{
								Season season = obj.Location.GetSeason();
								if (season == Season.Winter || season == Season.Fall)
								{
									item.ParentSheetIndex = 109;
								}
							}
						}
					}
					return true;
				});
				return true;
			case SaveFixes.MigrateStatFields:
				foreach (Farmer player5 in Game1.getAllFarmers())
				{
					SaveMigrator_1_6.<>c__DisplayClass3_4 CS$<>8__locals3;
					CS$<>8__locals3.stats = player5.stats;
					SerializableDictionary<string, uint> obsolete_stat_dictionary = CS$<>8__locals3.stats.obsolete_stat_dictionary;
					if (obsolete_stat_dictionary != null && obsolete_stat_dictionary.Count > 0)
					{
						foreach (KeyValuePair<string, uint> pair6 in CS$<>8__locals3.stats.obsolete_stat_dictionary)
						{
							uint prevValue;
							CS$<>8__locals3.stats.Values[pair6.Key] = (CS$<>8__locals3.stats.Values.TryGetValue(pair6.Key, out prevValue) ? (prevValue + pair6.Value) : pair6.Value);
						}
						CS$<>8__locals3.stats.obsolete_stat_dictionary = null;
					}
					uint walnutsFound;
					if (CS$<>8__locals3.stats.Values.TryGetValue("walnutsFound", out walnutsFound))
					{
						Game1.netWorldState.Value.GoldenWalnutsFound += (int)walnutsFound;
						CS$<>8__locals3.stats.Values.Remove("walnutsFound");
					}
					foreach (KeyValuePair<string, uint> pair7 in CS$<>8__locals3.stats.Values.ToArray<KeyValuePair<string, uint>>())
					{
						if (pair7.Value == 0U)
						{
							CS$<>8__locals3.stats.Values.Remove(pair7.Key);
						}
					}
					if (CS$<>8__locals3.stats.AverageBedtime == 0U)
					{
						CS$<>8__locals3.stats.Set("averageBedtime", CS$<>8__locals3.stats.obsolete_averageBedtime.GetValueOrDefault());
					}
					CS$<>8__locals3.stats.obsolete_averageBedtime = null;
					CS$<>8__locals3.stats.obsolete_beveragesMade = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("beveragesMade", CS$<>8__locals3.stats.obsolete_beveragesMade, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_caveCarrotsFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("caveCarrotsFound", CS$<>8__locals3.stats.obsolete_caveCarrotsFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_cheeseMade = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("cheeseMade", CS$<>8__locals3.stats.obsolete_cheeseMade, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_chickenEggsLayed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("chickenEggsLayed", CS$<>8__locals3.stats.obsolete_chickenEggsLayed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_copperFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("copperFound", CS$<>8__locals3.stats.obsolete_copperFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_cowMilkProduced = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("cowMilkProduced", CS$<>8__locals3.stats.obsolete_cowMilkProduced, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_cropsShipped = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("cropsShipped", CS$<>8__locals3.stats.obsolete_cropsShipped, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_daysPlayed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("daysPlayed", CS$<>8__locals3.stats.obsolete_daysPlayed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_diamondsFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("diamondsFound", CS$<>8__locals3.stats.obsolete_diamondsFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_dirtHoed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("dirtHoed", CS$<>8__locals3.stats.obsolete_dirtHoed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_duckEggsLayed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("duckEggsLayed", CS$<>8__locals3.stats.obsolete_duckEggsLayed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_fishCaught = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("fishCaught", CS$<>8__locals3.stats.obsolete_fishCaught, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_geodesCracked = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("geodesCracked", CS$<>8__locals3.stats.obsolete_geodesCracked, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_giftsGiven = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("giftsGiven", CS$<>8__locals3.stats.obsolete_giftsGiven, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_goatCheeseMade = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("goatCheeseMade", CS$<>8__locals3.stats.obsolete_goatCheeseMade, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_goatMilkProduced = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("goatMilkProduced", CS$<>8__locals3.stats.obsolete_goatMilkProduced, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_goldFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("goldFound", CS$<>8__locals3.stats.obsolete_goldFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_goodFriends = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("goodFriends", CS$<>8__locals3.stats.obsolete_goodFriends, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_individualMoneyEarned = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("individualMoneyEarned", CS$<>8__locals3.stats.obsolete_individualMoneyEarned, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_iridiumFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("iridiumFound", CS$<>8__locals3.stats.obsolete_iridiumFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_ironFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("ironFound", CS$<>8__locals3.stats.obsolete_ironFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_itemsCooked = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("itemsCooked", CS$<>8__locals3.stats.obsolete_itemsCooked, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_itemsCrafted = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("itemsCrafted", CS$<>8__locals3.stats.obsolete_itemsCrafted, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_itemsForaged = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("itemsForaged", CS$<>8__locals3.stats.obsolete_itemsForaged, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_itemsShipped = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("itemsShipped", CS$<>8__locals3.stats.obsolete_itemsShipped, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_monstersKilled = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("monstersKilled", CS$<>8__locals3.stats.obsolete_monstersKilled, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_mysticStonesCrushed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("mysticStonesCrushed", CS$<>8__locals3.stats.obsolete_mysticStonesCrushed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_notesFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("notesFound", CS$<>8__locals3.stats.obsolete_notesFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_otherPreciousGemsFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("otherPreciousGemsFound", CS$<>8__locals3.stats.obsolete_otherPreciousGemsFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_piecesOfTrashRecycled = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("piecesOfTrashRecycled", CS$<>8__locals3.stats.obsolete_piecesOfTrashRecycled, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_preservesMade = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("preservesMade", CS$<>8__locals3.stats.obsolete_preservesMade, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_prismaticShardsFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("prismaticShardsFound", CS$<>8__locals3.stats.obsolete_prismaticShardsFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_questsCompleted = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("questsCompleted", CS$<>8__locals3.stats.obsolete_questsCompleted, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_rabbitWoolProduced = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("rabbitWoolProduced", CS$<>8__locals3.stats.obsolete_rabbitWoolProduced, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_rocksCrushed = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("rocksCrushed", CS$<>8__locals3.stats.obsolete_rocksCrushed, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_sheepWoolProduced = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("sheepWoolProduced", CS$<>8__locals3.stats.obsolete_sheepWoolProduced, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_slimesKilled = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("slimesKilled", CS$<>8__locals3.stats.obsolete_slimesKilled, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_stepsTaken = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("stepsTaken", CS$<>8__locals3.stats.obsolete_stepsTaken, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_stoneGathered = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("stoneGathered", CS$<>8__locals3.stats.obsolete_stoneGathered, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_stumpsChopped = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("stumpsChopped", CS$<>8__locals3.stats.obsolete_stumpsChopped, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_timesFished = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("timesFished", CS$<>8__locals3.stats.obsolete_timesFished, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_timesUnconscious = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("timesUnconscious", CS$<>8__locals3.stats.obsolete_timesUnconscious, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_totalMoneyGifted = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("totalMoneyGifted", CS$<>8__locals3.stats.obsolete_totalMoneyGifted, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_trufflesFound = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("trufflesFound", CS$<>8__locals3.stats.obsolete_trufflesFound, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_weedsEliminated = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("weedsEliminated", CS$<>8__locals3.stats.obsolete_weedsEliminated, ref CS$<>8__locals3);
					CS$<>8__locals3.stats.obsolete_seedsSown = SaveMigrator_1_6.<ApplySaveFix>g__MergeStats|3_27("seedsSown", CS$<>8__locals3.stats.obsolete_seedsSown, ref CS$<>8__locals3);
				}
				return true;
			case SaveFixes.MakeWildSeedsDeterministic:
				Utility.ForEachCrop(delegate(Crop crop)
				{
					if (crop.isWildSeedCrop())
					{
						crop.replaceWithObjectOnFullGrown.Value = crop.getRandomWildCropForSeason(true);
					}
					return true;
				});
				return true;
			case SaveFixes.FixTranslatedInternalNames:
				Utility.ForEachItem(delegate(Item item)
				{
					string qualifiedItemId = item.QualifiedItemId;
					if (qualifiedItemId != null)
					{
						switch (qualifiedItemId.Length)
						{
						case 5:
							switch (qualifiedItemId[3])
							{
							case '1':
								if (!(qualifiedItemId == "(H)15") && !(qualifiedItemId == "(H)17") && !(qualifiedItemId == "(H)18"))
								{
									return true;
								}
								break;
							case '2':
								if (!(qualifiedItemId == "(H)23") && !(qualifiedItemId == "(H)28"))
								{
									return true;
								}
								break;
							case '3':
								if (!(qualifiedItemId == "(H)35"))
								{
									return true;
								}
								break;
							case '4':
								if (!(qualifiedItemId == "(H)41"))
								{
									return true;
								}
								break;
							case '5':
								if (!(qualifiedItemId == "(H)50") && !(qualifiedItemId == "(H)51"))
								{
									return true;
								}
								break;
							case '6':
							case '7':
								return true;
							case '8':
								if (!(qualifiedItemId == "(H)82"))
								{
									return true;
								}
								break;
							case '9':
								if (!(qualifiedItemId == "(H)90"))
								{
									return true;
								}
								break;
							default:
								return true;
							}
							break;
						case 6:
							if (!(qualifiedItemId == "(O)804"))
							{
								return true;
							}
							break;
						case 7:
						case 8:
						case 9:
						case 11:
						case 12:
							return true;
						case 10:
							if (!(qualifiedItemId == "(H)GilsHat"))
							{
								return true;
							}
							break;
						case 13:
							if (!(qualifiedItemId == "(H)GoldPanHat"))
							{
								return true;
							}
							if (item.Name == "Steel Pan")
							{
								ParsedItemData data = ItemRegistry.GetData(item.QualifiedItemId);
								item.Name = (((data != null) ? data.InternalName : null) ?? item.Name);
								return true;
							}
							return true;
						case 14:
							if (!(qualifiedItemId == "(H)AbigailsBow"))
							{
								return true;
							}
							break;
						case 15:
							if (!(qualifiedItemId == "(H)GovernorsHat"))
							{
								return true;
							}
							break;
						default:
							return true;
						}
						if (item.Name.Contains('’'))
						{
							ParsedItemData data2 = ItemRegistry.GetData(item.QualifiedItemId);
							item.Name = (((data2 != null) ? data2.InternalName : null) ?? item.Name);
						}
					}
					return true;
				});
				return true;
			case SaveFixes.ConvertBuildingQuests:
				foreach (Farmer player6 in Game1.getAllFarmers())
				{
					for (int l = 0; l < player6.questLog.Count; l++)
					{
						Quest quest9 = player6.questLog[l];
						if (quest9.questType.Value == 8)
						{
							player6.questLog[l] = new HaveBuildingQuest(quest9.obsolete_completionString);
						}
					}
				}
				return true;
			case SaveFixes.AddJunimoKartAndPrairieKingStats:
				foreach (Farmer player7 in Game1.getAllFarmers())
				{
					if (player7.hasOrWillReceiveMail("JunimoKart"))
					{
						player7.stats.Increment("completedJunimoKart", 1);
					}
					if (player7.hasOrWillReceiveMail("Beat_PK"))
					{
						player7.stats.Increment("completedPrairieKing", 1);
					}
				}
				return true;
			case SaveFixes.FixEmptyLostAndFoundItemStacks:
				foreach (Item item2 in Game1.player.team.returnedDonations)
				{
					if (item2 != null && item2.Stack < 1)
					{
						item2.Stack = 1;
					}
				}
				return true;
			case SaveFixes.FixDuplicateMissedMail:
			{
				HashSet<string> mailboxSet = new HashSet<string>();
				List<int> indicesToRemove = new List<int>();
				foreach (Farmer player8 in Game1.getAllFarmers())
				{
					mailboxSet.Clear();
					indicesToRemove.Clear();
					for (int m = 0; m < player8.mailbox.Count; m++)
					{
						string mailKey = player8.mailbox[m];
						if (!mailboxSet.Add(mailKey) && (mailKey == "robinKitchenLetter" || mailKey == "marnieAutoGrabber" || mailKey == "JunimoKart" || mailKey == "Beat_PK"))
						{
							indicesToRemove.Add(m);
						}
					}
					indicesToRemove.Reverse();
					foreach (int indexToRemove in indicesToRemove)
					{
						player8.mailbox.RemoveAt(indexToRemove);
					}
				}
				return true;
			}
			}
			return false;
		}

		// Token: 0x06001C80 RID: 7296 RVA: 0x00145A08 File Offset: 0x00143C08
		public static void ConvertBuildingsToData(GameLocation location)
		{
			for (int i = location.buildings.Count - 1; i >= 0; i--)
			{
				Building building = location.buildings[i];
				GameLocation indoors = building.GetIndoors();
				if (indoors != null)
				{
					SaveMigrator_1_6.ConvertBuildingsToData(indoors);
				}
				string value = building.buildingType.Value;
				if (value == "Log Cabin" || value == "Plank Cabin" || value == "Stone Cabin")
				{
					building.skinId.Value = building.buildingType.Value;
					building.buildingType.Value = "Cabin";
					building.ReloadBuildingData(false, false);
					building.updateInteriorWarps(null);
				}
				BuildingData data = building.GetData();
				string expectedType = (data != null) ? data.BuildingType : null;
				if (expectedType != null && expectedType != building.GetType().FullName)
				{
					Building newBuilding = Building.CreateInstanceFromId(building.buildingType.Value, new Vector2((float)building.tileX.Value, (float)building.tileY.Value));
					if (newBuilding != null)
					{
						newBuilding.indoors.Value = building.indoors.Value;
						newBuilding.buildingType.Value = building.buildingType.Value;
						newBuilding.tileX.Value = building.tileX.Value;
						newBuilding.tileY.Value = building.tileY.Value;
						location.buildings.RemoveAt(i);
						location.buildings.Add(newBuilding);
						SaveMigrator_1_6.TransferValuesToDataBuilding(building, newBuilding);
					}
				}
			}
		}

		// Token: 0x06001C81 RID: 7297 RVA: 0x00145B9C File Offset: 0x00143D9C
		public static void TransferValuesToDataBuilding(Building oldBuilding, Building newBuilding)
		{
			newBuilding.animalDoorOpen.Value = oldBuilding.animalDoorOpen.Value;
			newBuilding.animalDoorOpenAmount.Value = oldBuilding.animalDoorOpenAmount.Value;
			newBuilding.netBuildingPaintColor.Value.CopyFrom(oldBuilding.netBuildingPaintColor.Value);
			newBuilding.modData.CopyFrom(oldBuilding.modData.Pairs);
			Mill oldMill = oldBuilding as Mill;
			if (oldMill != null)
			{
				oldMill.TransferValuesToNewBuilding(newBuilding);
			}
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x00145C1C File Offset: 0x00143E1C
		public static void MigrateFarmhands(List<GameLocation> locations)
		{
			foreach (GameLocation gameLocation in locations)
			{
				foreach (Building building in gameLocation.buildings)
				{
					Cabin cabin = building.GetIndoors() as Cabin;
					if (cabin != null)
					{
						Farmer farmhand = cabin.obsolete_farmhand;
						cabin.obsolete_farmhand = null;
						Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
						cabin.farmhandReference.Value = farmhand;
					}
				}
			}
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x00145CE0 File Offset: 0x00143EE0
		public static void StandardizeBundleFields(Dictionary<string, string> bundleData)
		{
			foreach (string key in bundleData.Keys.ToArray<string>())
			{
				string[] fields = bundleData[key].Split('/', StringSplitOptions.None);
				if (fields.Length < 7)
				{
					Array.Resize<string>(ref fields, 7);
					fields[6] = fields[0];
					bundleData[key] = string.Join("/", fields);
				}
			}
		}

		// Token: 0x06001C84 RID: 7300 RVA: 0x00145D44 File Offset: 0x00143F44
		public static string InferBuildingUpgradingTo(string fromBuildingType)
		{
			if (fromBuildingType == "Coop")
			{
				return "Big Coop";
			}
			if (fromBuildingType == "Big Coop")
			{
				return "Deluxe Coop";
			}
			if (fromBuildingType == "Barn")
			{
				return "Big Barn";
			}
			if (fromBuildingType == "Big Barn")
			{
				return "Deluxe Barn";
			}
			if (!(fromBuildingType == "Shed"))
			{
				foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
				{
					if (pair.Value.BuildingToUpgrade == fromBuildingType)
					{
						return pair.Key;
					}
				}
				return null;
			}
			return "Big Shed";
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x00145E0C File Offset: 0x0014400C
		public static void InferMachineInputOutputFields(Object machine)
		{
			Object output = machine.heldObject.Value;
			string outputItemId = (output != null) ? output.QualifiedItemId : null;
			if (outputItemId == null)
			{
				return;
			}
			NetRef<Item> inputItem = machine.lastInputItem;
			NetString outputRule = machine.lastOutputRuleId;
			string qualifiedItemId = machine.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				switch (qualifiedItemId.Length)
				{
				case 5:
					if (!(qualifiedItemId == "(BC)9"))
					{
						return;
					}
					break;
				case 6:
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(BC)90"))
						{
							if (!(qualifiedItemId == "(BC)20"))
							{
								if (!(qualifiedItemId == "(BC)10"))
								{
									return;
								}
							}
							else
							{
								if (outputItemId != null)
								{
									int num = outputItemId.Length;
									if (num != 5)
									{
										if (num != 6)
										{
											return;
										}
										char c = outputItemId[4];
										if (c > '3')
										{
											if (c != '8')
											{
												if (c != '9')
												{
													return;
												}
												if (!(outputItemId == "(O)390"))
												{
													return;
												}
											}
											else if (!(outputItemId == "(O)382") && !(outputItemId == "(O)380"))
											{
												if (!(outputItemId == "(O)388"))
												{
													return;
												}
												outputRule.Value = "Default_Driftwood";
												inputItem.Value = ItemRegistry.Create("(O)169", 1, 0, false);
												return;
											}
											outputRule.Value = "Default_Trash";
											inputItem.Value = ItemRegistry.Create("(O)168", 1, 0, false);
											return;
										}
										if (c != '2')
										{
											if (c != '3')
											{
												return;
											}
											outputItemId == "(O)338";
											return;
										}
										else if (!(outputItemId == "(O)428"))
										{
											return;
										}
									}
									else if (!(outputItemId == "(O)93"))
									{
										return;
									}
									outputRule.Value = "Default_SoggyNewspaper";
									inputItem.Value = ItemRegistry.Create("(O)172", 1, 0, false);
									return;
								}
								return;
							}
						}
						else
						{
							if (outputItemId == "(O)466" || outputItemId == "(O)465" || outputItemId == "(O)369" || outputItemId == "(O)805")
							{
								outputRule.Value = "Default";
								return;
							}
							return;
						}
						break;
					case '1':
						if (!(qualifiedItemId == "(BC)21"))
						{
							return;
						}
						outputRule.Value = "Default";
						inputItem.Value = output.getOne();
						return;
					case '2':
					{
						if (!(qualifiedItemId == "(BC)12"))
						{
							return;
						}
						if (outputItemId == "(O)346")
						{
							outputRule.Value = "Default_Wheat";
							inputItem.Value = ItemRegistry.Create("(O)262", 1, 0, false);
							return;
						}
						if (outputItemId == "(O)303")
						{
							outputRule.Value = "Default_Hops";
							inputItem.Value = ItemRegistry.Create("(O)304", 1, 0, false);
							return;
						}
						if (outputItemId == "(O)614")
						{
							outputRule.Value = "Default_TeaLeaves";
							inputItem.Value = ItemRegistry.Create("(O)815", 1, 0, false);
							return;
						}
						if (outputItemId == "(O)395")
						{
							outputRule.Value = "Default_CoffeeBeans";
							inputItem.Value = ItemRegistry.Create("(O)433", 5, 0, false);
							return;
						}
						if (outputItemId == "(O)340")
						{
							outputRule.Value = "Default_Honey";
							inputItem.Value = ItemRegistry.Create("(O)459", 5, 0, false);
							return;
						}
						Object.PreserveType? value = output.preserve.Value;
						if (value == null)
						{
							return;
						}
						Object.PreserveType valueOrDefault = value.GetValueOrDefault();
						if (valueOrDefault == Object.PreserveType.Wine)
						{
							outputRule.Value = "Default_Wine";
							inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, true);
							return;
						}
						if (valueOrDefault == Object.PreserveType.Juice)
						{
							outputRule.Value = "Default_Juice";
							inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, true);
							return;
						}
						return;
					}
					case '3':
					{
						if (!(qualifiedItemId == "(BC)13"))
						{
							return;
						}
						if (outputItemId == null)
						{
							return;
						}
						int num = outputItemId.Length;
						if (num != 6)
						{
							return;
						}
						switch (outputItemId[5])
						{
						case '0':
							if (!(outputItemId == "(O)910"))
							{
								return;
							}
							outputRule.Value = "Default_RadioactiveOre";
							inputItem.Value = ItemRegistry.Create("(O)909", 5, 0, false);
							return;
						case '1':
						case '2':
						case '3':
							return;
						case '4':
							if (!(outputItemId == "(O)334"))
							{
								return;
							}
							outputRule.Value = "Default_CopperOre";
							inputItem.Value = ItemRegistry.Create("(O)378", 5, 0, false);
							return;
						case '5':
							if (!(outputItemId == "(O)335"))
							{
								return;
							}
							outputRule.Value = "Default_IronOre";
							inputItem.Value = ItemRegistry.Create("(O)380", 5, 0, false);
							return;
						case '6':
							if (!(outputItemId == "(O)336"))
							{
								return;
							}
							outputRule.Value = "Default_GoldOre";
							inputItem.Value = ItemRegistry.Create("(O)384", 5, 0, false);
							return;
						case '7':
							if (outputItemId == "(O)337")
							{
								outputRule.Value = "Default_IridiumOre";
								inputItem.Value = ItemRegistry.Create("(O)386", 5, 0, false);
								return;
							}
							if (!(outputItemId == "(O)277"))
							{
								return;
							}
							outputRule.Value = "Default_Bouquet";
							inputItem.Value = ItemRegistry.Create("(O)458", 1, 0, false);
							return;
						case '8':
							if (!(outputItemId == "(O)338"))
							{
								return;
							}
							if (output.Stack > 1)
							{
								outputRule.Value = "Default_FireQuartz";
								inputItem.Value = ItemRegistry.Create("(O)82", 1, 0, false);
								return;
							}
							outputRule.Value = "Default_Quartz";
							inputItem.Value = ItemRegistry.Create("(O)80", 1, 0, false);
							return;
						default:
							return;
						}
						break;
					}
					case '4':
						if (!(qualifiedItemId == "(BC)24"))
						{
							return;
						}
						if (!(outputItemId == "(O)306"))
						{
							if (outputItemId == "(O)307")
							{
								outputRule.Value = "Default_DuckEgg";
								inputItem.Value = ItemRegistry.Create("(O)442", 1, 0, false);
								return;
							}
							if (outputItemId == "(O)308")
							{
								outputRule.Value = "Default_VoidEgg";
								inputItem.Value = ItemRegistry.Create("(O)305", 1, 0, false);
								return;
							}
							if (!(outputItemId == "(O)807"))
							{
								return;
							}
							outputRule.Value = "Default_DinosaurEgg";
							inputItem.Value = ItemRegistry.Create("(O)107", 1, 0, false);
							return;
						}
						else
						{
							int num = output.Stack;
							if (num == 3)
							{
								outputRule.Value = "Default_GoldenEgg";
								inputItem.Value = ItemRegistry.Create("(O)928", 1, 0, false);
								return;
							}
							if (num == 10)
							{
								outputRule.Value = "Default_OstrichEgg";
								inputItem.Value = ItemRegistry.Create("(O)289", 1, output.Quality, false);
								return;
							}
							if (output.Quality == 2)
							{
								outputRule.Value = "Default_LargeEgg";
								inputItem.Value = ItemRegistry.Create("(O)174", 1, 0, false);
								return;
							}
							outputRule.Value = "Default_Egg";
							inputItem.Value = ItemRegistry.Create("(O)176", 1, 0, false);
							return;
						}
						break;
					case '5':
						if (!(qualifiedItemId == "(BC)15"))
						{
							if (!(qualifiedItemId == "(BC)25"))
							{
								return;
							}
							outputRule.Value = "Default";
							CropData cropData;
							if (outputItemId != "(O)499" && output.HasTypeObject() && Game1.cropData.TryGetValue(output.ItemId, out cropData) && cropData.HarvestItemId != null)
							{
								inputItem.Value = ItemRegistry.Create(cropData.HarvestItemId, 1, 0, true);
								return;
							}
							return;
						}
						else
						{
							if (outputItemId == "(O)445")
							{
								outputRule.Value = "Default_SturgeonRoe";
								inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>("(O)698", 1, 0, false));
								return;
							}
							if (outputItemId == "(O)447")
							{
								outputRule.Value = "Default_Roe";
								inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>(output.preservedParentSheetIndex.Value, 1, 0, false));
								return;
							}
							if (outputItemId == "(O)342")
							{
								outputRule.Value = "Default_Pickled";
								inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, true);
								return;
							}
							if (!(outputItemId == "(O)344"))
							{
								return;
							}
							outputRule.Value = "Default_Jelly";
							inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, true);
							return;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(BC)16"))
						{
							return;
						}
						if (!(outputItemId == "(O)426"))
						{
							if (!(outputItemId == "(O)424"))
							{
								return;
							}
							if (output.Quality == 0)
							{
								outputRule.Value = "Default_Milk";
								inputItem.Value = ItemRegistry.Create("(O)184", 1, 0, false);
								return;
							}
							outputRule.Value = "Default_LargeMilk";
							inputItem.Value = ItemRegistry.Create("(O)186", 1, 0, false);
							return;
						}
						else
						{
							if (output.Quality == 0)
							{
								outputRule.Value = "Default_GoatMilk";
								inputItem.Value = ItemRegistry.Create("(O)436", 1, 0, false);
								return;
							}
							outputRule.Value = "Default_LargeGoatMilk";
							inputItem.Value = ItemRegistry.Create("(O)438", 1, 0, false);
							return;
						}
						break;
					case '7':
						if (!(qualifiedItemId == "(BC)17"))
						{
							return;
						}
						if (outputItemId == "(O)428")
						{
							outputRule.Value = "Default";
							inputItem.Value = ItemRegistry.Create("(O)440", 1, 0, false);
							return;
						}
						return;
					case '8':
						return;
					case '9':
						if (!(qualifiedItemId == "(BC)19"))
						{
							return;
						}
						if (!(outputItemId == "(O)247") && outputItemId == "(O)432")
						{
							outputRule.Value = "Default_Truffle";
							inputItem.Value = ItemRegistry.Create("(O)430", 1, 0, false);
							return;
						}
						return;
					default:
						return;
					}
					break;
				case 7:
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(BC)101"))
						{
							qualifiedItemId == "(BC)105";
							return;
						}
						break;
					case '1':
						if (!(qualifiedItemId == "(BC)114"))
						{
							if (qualifiedItemId == "(BC)211")
							{
								return;
							}
							if (!(qualifiedItemId == "(BC)117"))
							{
								return;
							}
							goto IL_C29;
						}
						else
						{
							if (outputItemId == "(O)382")
							{
								outputRule.Value = "Default";
								inputItem.Value = ItemRegistry.Create("(O)388", 10, 0, false);
								return;
							}
							return;
						}
						break;
					case '2':
						if (!(qualifiedItemId == "(BC)127") && !(qualifiedItemId == "(BC)128"))
						{
							return;
						}
						goto IL_C29;
					case '3':
						if (!(qualifiedItemId == "(BC)231"))
						{
							return;
						}
						goto IL_C29;
					case '4':
						if (!(qualifiedItemId == "(BC)246"))
						{
							return;
						}
						goto IL_C29;
					case '5':
						if (!(qualifiedItemId == "(BC)254") && !(qualifiedItemId == "(BC)156"))
						{
							if (qualifiedItemId == "(BC)158")
							{
								outputRule.Value = "Default";
								inputItem.Value = ItemRegistry.Create("(O)766", 100, 0, false);
								return;
							}
							if (!(qualifiedItemId == "(BC)154"))
							{
								return;
							}
							goto IL_C29;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(BC)163"))
						{
							if (qualifiedItemId == "(BC)265")
							{
								outputRule.Value = "Default";
								return;
							}
							if (!(qualifiedItemId == "(BC)160"))
							{
								qualifiedItemId == "(BC)264";
								return;
							}
							goto IL_C29;
						}
						else
						{
							if (!(outputItemId == "(O)424"))
							{
								if (!(outputItemId == "(O)426"))
								{
									if (!(outputItemId == "(O)348"))
									{
										if (!(outputItemId == "(O)459"))
										{
											if (!(outputItemId == "(O)303"))
											{
												if (outputItemId == "(O)346")
												{
													outputRule.Value = "Beer";
												}
											}
											else
											{
												outputRule.Value = "PaleAle";
											}
										}
										else
										{
											outputRule.Value = "Mead";
										}
									}
									else
									{
										outputRule.Value = "Wine";
									}
								}
								else
								{
									outputRule.Value = "GoatCheese";
								}
							}
							else
							{
								outputRule.Value = "Cheese";
							}
							if (outputRule.Value != null)
							{
								inputItem.Value = output.getOne();
								inputItem.Value.Quality = 0;
								return;
							}
							return;
						}
						break;
					case '7':
						return;
					case '8':
						if (qualifiedItemId == "(BC)182")
						{
							outputRule.Value = "Default";
							return;
						}
						if (!(qualifiedItemId == "(BC)280"))
						{
							return;
						}
						goto IL_C29;
					default:
						return;
					}
					outputRule.Value = "Default";
					inputItem.Value = output.getOne();
					return;
				default:
					return;
				}
				IL_C29:
				outputRule.Value = "Default";
			}
		}

		// Token: 0x06001C86 RID: 7302 RVA: 0x00146A50 File Offset: 0x00144C50
		public static void MigrateLegacyDescriptionElement(Lazy<XmlSerializer> serializer, DescriptionElement element)
		{
			if (element == null)
			{
				return;
			}
			List<object> substitutions = element.substitutions;
			if (substitutions != null && substitutions.Count == 1)
			{
				XmlNode[] nodes = element.substitutions[0] as XmlNode[];
				if (nodes != null)
				{
					StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?><LegacyDescriptionElement xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><param>");
					foreach (XmlNode node in nodes)
					{
						xml.Append(node.OuterXml);
					}
					xml.Append("</param></LegacyDescriptionElement>");
					SaveMigrator_1_6.LegacyDescriptionElement data;
					using (StringReader stringReader = new StringReader(xml.ToString()))
					{
						using (XmlReader xmlReader = new XmlTextReader(stringReader))
						{
							data = (SaveMigrator_1_6.LegacyDescriptionElement)serializer.Value.Deserialize(xmlReader);
						}
					}
					if (data != null)
					{
						element.substitutions = data.param;
					}
				}
			}
			string translationKey = element.translationKey;
			if (!(translationKey == "Strings\\StringsFromCSFiles:FishingQuest.cs.13251"))
			{
				if (!(translationKey == "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13563"))
				{
					if (translationKey == "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13574")
					{
						element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13571";
					}
				}
				else
				{
					element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13560";
				}
			}
			else
			{
				element.translationKey = "Strings\\StringsFromCSFiles:FishingQuest.cs.13248";
			}
			List<object> substitutions2 = element.substitutions;
			if (substitutions2 != null && substitutions2.Count > 0)
			{
				foreach (object obj in element.substitutions)
				{
					DescriptionElement childElement = obj as DescriptionElement;
					if (childElement != null)
					{
						SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, childElement);
					}
				}
			}
		}

		// Token: 0x06001C88 RID: 7304 RVA: 0x00146C18 File Offset: 0x00144E18
		[CompilerGenerated]
		internal static uint? <ApplySaveFix>g__MergeStats|3_27(string newKey, uint? oldValue, ref SaveMigrator_1_6.<>c__DisplayClass3_4 A_2)
		{
			A_2.stats.Increment(newKey, oldValue.GetValueOrDefault());
			return null;
		}

		// Token: 0x02000540 RID: 1344
		public class LegacyDescriptionElement
		{
			// Token: 0x04002B00 RID: 11008
			public string xmlKey;

			// Token: 0x04002B01 RID: 11009
			public List<object> param;
		}
	}
}
