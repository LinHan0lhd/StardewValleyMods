using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x020000D0 RID: 208
	public class Farm : GameLocation
	{
		// Token: 0x06000E10 RID: 3600 RVA: 0x000958F4 File Offset: 0x00093AF4
		public Farm()
		{
		}

		// Token: 0x06000E11 RID: 3601 RVA: 0x00095990 File Offset: 0x00093B90
		public Farm(string mapPath, string name) : base(mapPath, name)
		{
			this.isAlwaysActive.Value = true;
		}

		// Token: 0x06000E12 RID: 3602 RVA: 0x00095A3A File Offset: 0x00093C3A
		public override bool IsBuildableLocation()
		{
			return true;
		}

		// Token: 0x06000E13 RID: 3603 RVA: 0x00095A40 File Offset: 0x00093C40
		public override void AddDefaultBuildings(bool load = true)
		{
			this.AddDefaultBuilding("Farmhouse", this.GetStarterFarmhouseLocation(), load);
			this.AddDefaultBuilding("Greenhouse", this.GetGreenhouseStartLocation(), load);
			this.AddDefaultBuilding("Shipping Bin", this.GetStarterShippingBinLocation(), load);
			this.AddDefaultBuilding("Pet Bowl", this.GetStarterPetBowlLocation(), load);
			base.BuildStartingCabins();
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x00095A9B File Offset: 0x00093C9B
		public override string GetDisplayName()
		{
			return base.GetDisplayName() ?? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.player.farmName.Value);
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x00095AC8 File Offset: 0x00093CC8
		public virtual Vector2 GetStarterShippingBinLocation()
		{
			if (this.mapShippingBinPosition == null)
			{
				Vector2 position;
				if (!base.TryGetMapPropertyAs("ShippingBinLocation", out position, false))
				{
					position = new Vector2(71f, 14f);
				}
				this.mapShippingBinPosition = new Vector2?(position);
			}
			return this.mapShippingBinPosition.Value;
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x00095B1C File Offset: 0x00093D1C
		public virtual Vector2 GetStarterPetBowlLocation()
		{
			Vector2 tile;
			if (!base.TryGetMapPropertyAs("PetBowlLocation", out tile, false))
			{
				return new Vector2(53f, 7f);
			}
			return tile;
		}

		// Token: 0x06000E17 RID: 3607 RVA: 0x00095B4C File Offset: 0x00093D4C
		public virtual Vector2 GetStarterFarmhouseLocation()
		{
			Point entry = this.GetMainFarmHouseEntry();
			return new Vector2((float)(entry.X - 5), (float)(entry.Y - 3));
		}

		// Token: 0x06000E18 RID: 3608 RVA: 0x00095B78 File Offset: 0x00093D78
		public virtual Vector2 GetGreenhouseStartLocation()
		{
			Vector2 position;
			if (base.TryGetMapPropertyAs("GreenhouseLocation", out position, false))
			{
				return position;
			}
			int whichFarm = Game1.whichFarm;
			if (whichFarm == 5)
			{
				return new Vector2(36f, 29f);
			}
			if (whichFarm != 6)
			{
				return new Vector2(25f, 10f);
			}
			return new Vector2(14f, 14f);
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x00095BD8 File Offset: 0x00093DD8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.sharedShippingBin, "sharedShippingBin").AddField(this.spawnCrowEvent, "spawnCrowEvent").AddField(this.lightningStrikeEvent, "lightningStrikeEvent").AddField(this.grandpaScore, "grandpaScore").AddField(this.greenhouseUnlocked, "greenhouseUnlocked").AddField(this.greenhouseMoved, "greenhouseMoved").AddField(this.farmCaveReady, "farmCaveReady");
			this.spawnCrowEvent.onEvent += this.doSpawnCrow;
			this.lightningStrikeEvent.onEvent += this.doLightningStrike;
			this.greenhouseMoved.fieldChangeVisibleEvent += delegate(NetBool field, bool old_value, bool new_value)
			{
				this.ClearGreenhouseGrassTiles();
			};
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x00095CA8 File Offset: 0x00093EA8
		public virtual void ClearGreenhouseGrassTiles()
		{
			if (this.map == null)
			{
				return;
			}
			if (Game1.gameMode == 6)
			{
				return;
			}
			if (this.greenhouseMoved.Value)
			{
				switch (Game1.whichFarm)
				{
				case 0:
				case 3:
				case 4:
					base.ApplyMapOverride("Farm_Greenhouse_Dirt", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)this.GetGreenhouseStartLocation().X, (int)this.GetGreenhouseStartLocation().Y, 9, 6)));
					return;
				case 1:
				case 2:
					break;
				case 5:
					base.ApplyMapOverride("Farm_Greenhouse_Dirt_FourCorners", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((int)this.GetGreenhouseStartLocation().X, (int)this.GetGreenhouseStartLocation().Y, 9, 6)));
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000E1B RID: 3611 RVA: 0x00095D70 File Offset: 0x00093F70
		public static string getMapNameFromTypeInt(int type)
		{
			switch (type)
			{
			case 0:
				return "Farm";
			case 1:
				return "Farm_Fishing";
			case 2:
				return "Farm_Foraging";
			case 3:
				return "Farm_Mining";
			case 4:
				return "Farm_Combat";
			case 5:
				return "Farm_FourCorners";
			case 6:
				return "Farm_Island";
			case 7:
				if (Game1.whichModFarm != null)
				{
					return Game1.whichModFarm.MapName;
				}
				break;
			}
			return "Farm";
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00095DE8 File Offset: 0x00093FE8
		public void onNewGame()
		{
			if (Game1.whichFarm == 3 || this.ShouldSpawnMountainOres())
			{
				for (int i = 0; i < 28; i++)
				{
					this.doDailyMountainFarmUpdate();
				}
				return;
			}
			if (Game1.whichFarm == 5)
			{
				for (int j = 0; j < 10; j++)
				{
					this.doDailyMountainFarmUpdate();
				}
				return;
			}
			if (Game1.GetFarmTypeID() == "MeadowlandsFarm")
			{
				for (int x = 47; x < 63; x++)
				{
					this.objects.Add(new Vector2((float)x, 20f), new Fence(new Vector2((float)x, 20f), "322", false));
				}
				for (int y = 16; y < 20; y++)
				{
					this.objects.Add(new Vector2(47f, (float)y), new Fence(new Vector2(47f, (float)y), "322", false));
				}
				for (int y2 = 7; y2 < 20; y2++)
				{
					this.objects.Add(new Vector2(62f, (float)y2), new Fence(new Vector2(62f, (float)y2), "322", y2 == 13));
				}
				Building b = new Building("Coop", new Vector2(54f, 9f));
				b.FinishConstruction(true);
				b.LoadFromBuildingData(b.GetData(), false, true);
				b.load();
				FarmAnimal starterChicken = new FarmAnimal("White Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
				FarmAnimal starterChicken2 = new FarmAnimal("Brown Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
				string[] chickenSplit = Game1.content.LoadString("Strings\\1_6_Strings:StarterChicken_Names").Split('|', StringSplitOptions.None);
				string chickenNames = chickenSplit[Game1.random.Next(chickenSplit.Length)];
				starterChicken.Name = chickenNames.Split(',', StringSplitOptions.None)[0].Trim();
				starterChicken2.Name = chickenNames.Split(',', StringSplitOptions.None)[1].Trim();
				(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken);
				(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken2);
				this.buildings.Add(b);
			}
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x00096014 File Offset: 0x00094214
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.UpdatePatio();
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				Pet pet = this.characters[i] as Pet;
				if (pet != null && (base.hasTileAt(pet.TilePoint, "Buildings", null) || base.hasTileAt(pet.TilePoint.X + 1, pet.TilePoint.Y, "Buildings", null) || !this.CanSpawnCharacterHere(pet.Tile) || !this.CanSpawnCharacterHere(new Vector2((float)(pet.TilePoint.X + 1), (float)pet.TilePoint.Y))))
				{
					pet.WarpToPetBowl();
				}
			}
			this.lastItemShipped = null;
			if (this.characters.Count > 5)
			{
				int slimesEscaped = this.characters.RemoveWhere((NPC npc) => npc is GreenSlime && Game1.random.NextDouble() < 0.035);
				if (slimesEscaped > 0)
				{
					Game1.multiplayer.broadcastGlobalMessage((slimesEscaped == 1) ? "Strings\\Locations:Farm_1SlimeEscaped" : "Strings\\Locations:Farm_NSlimesEscaped", false, null, new string[]
					{
						slimesEscaped.ToString() ?? ""
					});
				}
			}
			if (Game1.whichFarm == 5)
			{
				if (this.CanItemBePlacedHere(new Vector2(5f, 32f), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2(6f, 32f), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2(6f, 33f), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2(5f, 33f), false, CollisionMask.All, CollisionMask.None, false, false))
				{
					this.resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(5f, 32f), null, null));
				}
				if (this.objects.Length > 0)
				{
					for (int j = 0; j < 6; j++)
					{
						Vector2 vector;
						Object o;
						if (Utility.TryGetRandom(this.objects, out vector, out o, null) && o.IsWeeds() && o.tileLocation.X < 36f && o.tileLocation.Y < 34f)
						{
							o.SetIdAndSprite(792 + Game1.seasonIndex);
						}
					}
				}
			}
			if (this.ShouldSpawnBeachFarmForage())
			{
				while (Game1.random.NextDouble() < 0.9)
				{
					Vector2 v = base.getRandomTile(null);
					if (this.CanItemBePlacedHere(v, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && !base.hasTileAt((int)v.X, (int)v.Y, "AlwaysFront", null))
					{
						string whichItem = null;
						if (this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "BeachSpawn", "Back") != "")
						{
							whichItem = "372";
							Game1.stats.Increment("beachFarmSpawns", 1U);
							switch (Game1.random.Next(6))
							{
							case 0:
								whichItem = "393";
								break;
							case 1:
								whichItem = "719";
								break;
							case 2:
								whichItem = "718";
								break;
							case 3:
								whichItem = "723";
								break;
							case 4:
							case 5:
								whichItem = "152";
								break;
							}
							if (Game1.stats.DaysPlayed > 1U)
							{
								if (Game1.random.NextDouble() < 0.15 || Game1.stats.Get("beachFarmSpawns") % 4U == 0U)
								{
									whichItem = Game1.random.Next(922, 925).ToString();
									this.objects.Add(v, new Object(whichItem, 1, false, -1, 0)
									{
										Fragility = 2,
										MinutesUntilReady = 3
									});
									whichItem = null;
								}
								else if (Game1.random.NextDouble() < 0.1)
								{
									whichItem = "397";
								}
								else if (Game1.random.NextDouble() < 0.05)
								{
									whichItem = "392";
								}
								else if (Game1.random.NextDouble() < 0.02)
								{
									whichItem = "394";
								}
							}
						}
						else if (Game1.season != Season.Winter && new Microsoft.Xna.Framework.Rectangle(20, 66, 33, 18).Contains((int)v.X, (int)v.Y) && this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back") == "Grass")
						{
							whichItem = Utility.getRandomBasicSeasonalForageItem(Game1.season, (int)Game1.stats.DaysPlayed);
						}
						if (whichItem != null)
						{
							Object obj = ItemRegistry.Create<Object>("(O)" + whichItem, 1, 0, false);
							obj.CanBeSetDown = false;
							obj.IsSpawnedObject = true;
							this.dropObject(obj, v * 64f, Game1.viewport, true, null);
						}
					}
				}
			}
			if (Game1.whichFarm == 2)
			{
				for (int x = 0; x < 20; x++)
				{
					for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
					{
						if (base.getTileIndexAt(x, y, "Paths", null) == 21 && this.CanItemBePlacedHere(new Vector2((float)x, (float)y), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2((float)(x + 1), (float)y), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2((float)(x + 1), (float)(y + 1)), false, CollisionMask.All, CollisionMask.None, false, false) && this.CanItemBePlacedHere(new Vector2((float)x, (float)(y + 1)), false, CollisionMask.All, CollisionMask.None, false, false))
						{
							this.resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2((float)x, (float)y), null, null));
						}
					}
				}
			}
			if (this.ShouldSpawnForestFarmForage() && !Game1.IsWinter)
			{
				while (Game1.random.NextDouble() < 0.75)
				{
					Vector2 v2 = new Vector2((float)Game1.random.Next(18), (float)Game1.random.Next(this.map.Layers[0].LayerHeight));
					if (Game1.random.NextBool() || Game1.whichFarm != 2)
					{
						v2 = base.getRandomTile(null);
					}
					if (this.CanItemBePlacedHere(v2, false, CollisionMask.All, CollisionMask.None, false, false) && !base.hasTileAt((int)v2.X, (int)v2.Y, "AlwaysFront", null) && ((Game1.whichFarm == 2 && v2.X < 18f) || this.doesTileHavePropertyNoNull((int)v2.X, (int)v2.Y, "Type", "Back").Equals("Grass")))
					{
						string whichItem2;
						switch (Game1.season)
						{
						case Season.Spring:
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem2 = "(O)" + 16.ToString();
								break;
							case 1:
								whichItem2 = "(O)" + 22.ToString();
								break;
							case 2:
								whichItem2 = "(O)" + 20.ToString();
								break;
							default:
								whichItem2 = "(O)257";
								break;
							}
							break;
						case Season.Summer:
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem2 = "(O)402";
								break;
							case 1:
								whichItem2 = "(O)396";
								break;
							case 2:
								whichItem2 = "(O)398";
								break;
							default:
								whichItem2 = "(O)404";
								break;
							}
							break;
						case Season.Fall:
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem2 = "(O)281";
								break;
							case 1:
								whichItem2 = "(O)420";
								break;
							case 2:
								whichItem2 = "(O)422";
								break;
							default:
								whichItem2 = "(O)404";
								break;
							}
							break;
						default:
							whichItem2 = "(O)792";
							break;
						}
						Object obj2 = ItemRegistry.Create<Object>(whichItem2, 1, 0, false);
						obj2.CanBeSetDown = false;
						obj2.IsSpawnedObject = true;
						this.dropObject(obj2, v2 * 64f, Game1.viewport, true, null);
					}
				}
				if (this.objects.Length > 0)
				{
					for (int k = 0; k < 6; k++)
					{
						Vector2 vector;
						Object o2;
						if (Utility.TryGetRandom(this.objects, out vector, out o2, null) && o2.IsWeeds())
						{
							o2.SetIdAndSprite(792 + Game1.seasonIndex);
						}
					}
				}
			}
			if (Game1.whichFarm == 3 || Game1.whichFarm == 5 || this.ShouldSpawnMountainOres())
			{
				this.doDailyMountainFarmUpdate();
			}
			if (this.terrainFeatures.Length > 0 && Game1.season == Season.Fall && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
			{
				for (int tries = 0; tries < 10; tries++)
				{
					Vector2 tile;
					TerrainFeature feature;
					if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(this.terrainFeatures, out tile, out feature, null))
					{
						Tree tree = feature as Tree;
						if (tree != null && tree.growthStage.Value >= 5 && !tree.tapped.Value && !tree.isTemporaryGreenRainTree.Value)
						{
							tree.treeType.Value = "7";
							tree.loadSprite();
							break;
						}
					}
				}
			}
			this.addCrows();
			if (Game1.season != Season.Winter)
			{
				base.spawnWeedsAndStones((Game1.season == Season.Summer) ? 30 : 20, false, true);
			}
			base.spawnWeeds(false);
			this.HandleGrassGrowth(dayOfMonth);
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x000969F0 File Offset: 0x00094BF0
		public void doDailyMountainFarmUpdate()
		{
			double chance = 1.0;
			while (Game1.random.NextDouble() < chance)
			{
				Vector2 v = this.ShouldSpawnMountainOres() ? Utility.getRandomPositionInThisRectangle(this._mountainForageRectangle.Value, Game1.random) : ((Game1.whichFarm == 5) ? Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(51, 67, 11, 3), Game1.random) : Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(5, 37, 22, 8), Game1.random));
				if (this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Dirt") && this.CanItemBePlacedHere(v, false, CollisionMask.All, CollisionMask.None, false, false))
				{
					string stone_id = "668";
					int health = 2;
					if (Game1.random.NextDouble() < 0.15)
					{
						this.objects.Add(v, ItemRegistry.Create<Object>("(O)590", 1, 0, false));
						continue;
					}
					if (Game1.random.NextBool())
					{
						stone_id = "670";
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						if (Game1.player.MiningLevel >= 8 && Game1.random.NextDouble() < 0.33)
						{
							stone_id = "77";
							health = 7;
						}
						else if (Game1.player.MiningLevel >= 5 && Game1.random.NextBool())
						{
							stone_id = "76";
							health = 5;
						}
						else
						{
							stone_id = "75";
							health = 3;
						}
					}
					if (Game1.random.NextDouble() < 0.21)
					{
						stone_id = "751";
						health = 3;
					}
					if (Game1.player.MiningLevel >= 4 && Game1.random.NextDouble() < 0.15)
					{
						stone_id = "290";
						health = 4;
					}
					if (Game1.player.MiningLevel >= 7 && Game1.random.NextDouble() < 0.1)
					{
						stone_id = "764";
						health = 8;
					}
					if (Game1.player.MiningLevel >= 10 && Game1.random.NextDouble() < 0.01)
					{
						stone_id = "765";
						health = 16;
					}
					this.objects.Add(v, new Object(stone_id, 10, false, -1, 0)
					{
						MinutesUntilReady = health
					});
				}
				chance *= 0.75;
			}
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x00096C34 File Offset: 0x00094E34
		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			if (this.map != null)
			{
				if (this._oceanCrabPotOverride == null)
				{
					this._oceanCrabPotOverride = new bool?(this.map.Properties.ContainsKey("FarmOceanCrabPotOverride"));
				}
				if (this._oceanCrabPotOverride.Value)
				{
					return true;
				}
			}
			return base.catchOceanCrabPotFishFromThisSpot(x, y);
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x00096C90 File Offset: 0x00094E90
		public void addCrows()
		{
			int numCrops = 0;
			foreach (KeyValuePair<Vector2, TerrainFeature> v in this.terrainFeatures.Pairs)
			{
				HoeDirt dirt = v.Value as HoeDirt;
				if (dirt != null && dirt.crop != null)
				{
					numCrops++;
				}
			}
			List<Vector2> scarecrowPositions = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> v2 in this.objects.Pairs)
			{
				if (v2.Value.IsScarecrow())
				{
					scarecrowPositions.Add(v2.Key);
				}
			}
			int potentialCrows = Math.Min(4, numCrops / 16);
			for (int i = 0; i < potentialCrows; i++)
			{
				if (Game1.random.NextDouble() < 0.3)
				{
					for (int attempts = 0; attempts < 10; attempts++)
					{
						Vector2 tile;
						TerrainFeature feature;
						if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(this.terrainFeatures, out tile, out feature, null))
						{
							HoeDirt dirt2 = feature as HoeDirt;
							if (dirt2 != null)
							{
								Crop crop = dirt2.crop;
								if (crop != null && crop.currentPhase.Value > 1)
								{
									bool scarecrow = false;
									foreach (Vector2 s in scarecrowPositions)
									{
										int radius = this.objects[s].GetRadiusForScarecrow();
										if (Vector2.Distance(s, tile) < (float)radius)
										{
											scarecrow = true;
											Object @object = this.objects[s];
											int specialVariable = @object.SpecialVariable;
											@object.SpecialVariable = specialVariable + 1;
											break;
										}
									}
									if (!scarecrow)
									{
										dirt2.destroyCrop(false);
										this.spawnCrowEvent.Fire(tile);
										break;
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x00096E98 File Offset: 0x00095098
		private void doSpawnCrow(Vector2 v)
		{
			if (this.critters == null && this.isOutdoors.Value)
			{
				this.critters = new List<Critter>();
			}
			this.critters.Add(new Crow((int)v.X, (int)v.Y));
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x00096ED8 File Offset: 0x000950D8
		public static Point getFrontDoorPositionForFarmer(Farmer who)
		{
			Point entry_point = Game1.getFarm().GetMainFarmHouseEntry();
			entry_point.Y--;
			return entry_point;
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00096F00 File Offset: 0x00095100
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (timeOfDay >= 1300 && Game1.IsMasterGame)
			{
				foreach (Character character in new List<Character>(this.characters))
				{
					NPC i = (NPC)character;
					if (i.isMarried())
					{
						i.returnHomeFromFarmPosition(this);
					}
				}
			}
			foreach (NPC c in this.characters)
			{
				if (c.getSpouse() == Game1.player)
				{
					c.checkForMarriageDialogue(timeOfDay, this);
				}
				Child child = c as Child;
				if (child != null)
				{
					child.tenMinuteUpdate();
				}
			}
			if (Game1.spawnMonstersAtNight && Game1.farmEvent == null && Game1.timeOfDay >= 1900 && Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck(null) / 2.0)
			{
				if (Game1.random.NextDouble() < 0.25)
				{
					if (base.Equals(Game1.currentLocation))
					{
						this.spawnFlyingMonstersOffScreen();
						return;
					}
				}
				else
				{
					this.spawnGroundMonsterOffScreen();
				}
			}
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00097058 File Offset: 0x00095258
		public void spawnGroundMonsterOffScreen()
		{
			for (int i = 0; i < 15; i++)
			{
				Vector2 spawnLocation = base.getRandomTile(null);
				if (Utility.isOnScreen(Utility.Vector2ToPoint(spawnLocation), 64, this))
				{
					spawnLocation.X -= (float)(Game1.viewport.Width / 64);
				}
				if (this.CanItemBePlacedHere(spawnLocation, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					int combatLevel = Game1.player.CombatLevel;
					bool success;
					if (combatLevel >= 8 && Game1.random.NextDouble() < 0.15)
					{
						this.characters.Add(new ShadowBrute(spawnLocation * 64f)
						{
							focusedOnFarmers = true,
							wildernessFarmMonster = true
						});
						success = true;
					}
					else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33))
					{
						this.characters.Add(new RockGolem(spawnLocation * 64f, combatLevel)
						{
							wildernessFarmMonster = true
						});
						success = true;
					}
					else
					{
						int virtualMineLevel = 1;
						if (combatLevel >= 10)
						{
							virtualMineLevel = 140;
						}
						else if (combatLevel >= 8)
						{
							virtualMineLevel = 100;
						}
						else if (combatLevel >= 4)
						{
							virtualMineLevel = 41;
						}
						this.characters.Add(new GreenSlime(spawnLocation * 64f, virtualMineLevel)
						{
							wildernessFarmMonster = true
						});
						success = true;
					}
					if (success && Game1.currentLocation.Equals(this))
					{
						foreach (KeyValuePair<Vector2, Object> v in this.objects.Pairs)
						{
							Object value = v.Value;
							if (((value != null) ? value.QualifiedItemId : null) == "(BC)83")
							{
								v.Value.shakeTimer = 1000;
								v.Value.showNextIndex.Value = true;
								Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
							}
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x000972C0 File Offset: 0x000954C0
		public void spawnFlyingMonstersOffScreen()
		{
			Vector2 spawnLocation = Vector2.Zero;
			switch (Game1.random.Next(4))
			{
			case 0:
				spawnLocation.X = (float)Game1.random.Next(this.map.Layers[0].LayerWidth);
				break;
			case 1:
				spawnLocation.X = (float)(this.map.Layers[0].LayerWidth - 1);
				spawnLocation.Y = (float)Game1.random.Next(this.map.Layers[0].LayerHeight);
				break;
			case 2:
				spawnLocation.Y = (float)(this.map.Layers[0].LayerHeight - 1);
				spawnLocation.X = (float)Game1.random.Next(this.map.Layers[0].LayerWidth);
				break;
			case 3:
				spawnLocation.Y = (float)Game1.random.Next(this.map.Layers[0].LayerHeight);
				break;
			}
			if (Utility.isOnScreen(spawnLocation * 64f, 64))
			{
				spawnLocation.X -= (float)Game1.viewport.Width;
			}
			int combatLevel = Game1.player.CombatLevel;
			bool success;
			if (combatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.Items.ContainsId("(W)4"))
			{
				this.characters.Add(new Bat(spawnLocation * 64f, 9999)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				this.characters.Add(new Bat(spawnLocation * 64f, 172)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				this.characters.Add(new Serpent(spawnLocation * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (combatLevel >= 8 && Game1.random.NextBool())
			{
				this.characters.Add(new Bat(spawnLocation * 64f, 81)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (combatLevel >= 5 && Game1.random.NextBool())
			{
				this.characters.Add(new Bat(spawnLocation * 64f, 41)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else
			{
				this.characters.Add(new Bat(spawnLocation * 64f, 1)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			if (success && Game1.currentLocation.Equals(this))
			{
				foreach (KeyValuePair<Vector2, Object> v in this.objects.Pairs)
				{
					Object value = v.Value;
					if (((value != null) ? value.QualifiedItemId : null) == "(BC)83")
					{
						v.Value.shakeTimer = 1000;
						v.Value.showNextIndex.Value = true;
						Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
					}
				}
			}
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x000976D4 File Offset: 0x000958D4
		public virtual void requestGrandpaReevaluation()
		{
			this.grandpaScore.Value = 0;
			if (Game1.IsMasterGame)
			{
				Game1.player.eventsSeen.Remove("558292");
				Game1.player.eventsSeen.Add("321777");
			}
			base.removeTemporarySpritesWithID(6666);
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x00097729 File Offset: 0x00095929
		public override void OnMapLoad(Map map)
		{
			this.CacheOffBasePatioArea();
			base.OnMapLoad(map);
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x00097738 File Offset: 0x00095938
		public override void OnBuildingMoved(Building building)
		{
			base.OnBuildingMoved(building);
			if (building.HasIndoorsName("FarmHouse"))
			{
				this.UnsetFarmhouseValues();
			}
			if (building is GreenhouseBuilding)
			{
				this.greenhouseMoved.Value = true;
			}
			FarmHouse house = building.GetIndoors() as FarmHouse;
			if (house != null && house.HasNpcSpouseOrRoommate())
			{
				NPC npc = base.getCharacterFromName(house.owner.spouse);
				if (npc != null && !npc.shouldPlaySpousePatioAnimation.Value)
				{
					Game1.player.team.requestNPCGoHome.Fire(npc.Name);
				}
			}
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x000977C6 File Offset: 0x000959C6
		public override bool ShouldExcludeFromNpcPathfinding()
		{
			return true;
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x000977CC File Offset: 0x000959CC
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Point shrine_position = this.GetGrandpaShrinePosition();
			if (tileLocation.X >= shrine_position.X - 1 && tileLocation.X <= shrine_position.X + 1 && tileLocation.Y == shrine_position.Y)
			{
				if (!this.hasSeenGrandpaNote)
				{
					Game1.addMail("hasSeenGrandpaNote", true, false);
					this.hasSeenGrandpaNote = true;
					Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", Game1.player.Name).Replace('\n', '^'));
					return true;
				}
				if (Game1.year >= 3 && this.grandpaScore.Value > 0 && this.grandpaScore.Value < 4)
				{
					Object activeObject = who.ActiveObject;
					if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)72" && this.grandpaScore.Value < 4)
					{
						who.reduceActiveItemByOne();
						base.playSound("stoneStep", null, null, SoundContext.Default);
						base.playSound("fireball", null, null, SoundContext.Default);
						DelayedAction.playSoundAfterDelay("yoba", 800, this, null, -1, false);
						DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
						Game1.multiplayer.broadcastGrandpaReevaluation();
						Game1.player.freezePause = 1200;
						return true;
					}
					Object activeObject2 = who.ActiveObject;
					if (((activeObject2 != null) ? activeObject2.QualifiedItemId : null) != "(O)72")
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
						return true;
					}
				}
				else
				{
					if (this.grandpaScore.Value >= 4 && !Utility.doesItemExistAnywhere("(BC)160"))
					{
						who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)160", 1, 0, false), new ItemGrabMenu.behaviorOnItemSelect(this.grandpaStatueCallback), false);
						return true;
					}
					if (this.grandpaScore.Value == 0 && Game1.year >= 3)
					{
						Game1.player.eventsSeen.Remove("558292");
						Game1.player.eventsSeen.Add("321777");
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00097A10 File Offset: 0x00095C10
		public void grandpaStatueCallback(Item item, Farmer who)
		{
			Object obj = item as Object;
			if (obj != null && obj.QualifiedItemId == "(BC)160" && who != null)
			{
				who.mailReceived.Add("grandpaPerfect");
			}
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x00097A50 File Offset: 0x00095C50
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			Farm fromFarm = (Farm)l;
			base.TransferDataFromSavedLocation(l);
			this.housePaintColor.Value = fromFarm.housePaintColor.Value;
			this.farmCaveReady.Value = fromFarm.farmCaveReady.Value;
			if (fromFarm.hasSeenGrandpaNote)
			{
				Game1.addMail("hasSeenGrandpaNote", true, false);
			}
			this.UnsetFarmhouseValues();
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x00097AB1 File Offset: 0x00095CB1
		public IInventory getShippingBin(Farmer who)
		{
			if (Game1.player.team.useSeparateWallets.Value)
			{
				return who.personalShippingBin.Value;
			}
			return this.sharedShippingBin.Value;
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x00097AE0 File Offset: 0x00095CE0
		public void shipItem(Item i, Farmer who)
		{
			if (i != null)
			{
				who.removeItemFromInventory(i);
				this.getShippingBin(who).Add(i);
				this.showShipment(i, false);
				this.lastItemShipped = i;
				if (Game1.player.ActiveItem == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x00097B33 File Offset: 0x00095D33
		public void UnsetFarmhouseValues()
		{
			this.mainFarmhouseEntry = null;
			this.mapMainMailboxPosition = null;
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x00097B50 File Offset: 0x00095D50
		public void showShipment(Item item, bool playThrowSound = true)
		{
			if (playThrowSound)
			{
				base.localSound("backpackIN", null, null, SoundContext.Default);
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0, null, null, -1, false);
			int id = Game1.random.Next();
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(71f, 13f) * 64f + new Vector2(0f, 5f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.09601f,
				id = id,
				extraInfoForEndBehavior = id,
				endFunction = new TemporaryAnimatedSprite.endBehavior(base.removeTemporarySpritesWithID)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(71f, 13f) * 64f + new Vector2(0f, 17f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.0963f,
				id = id,
				extraInfoForEndBehavior = id
			});
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
			ColoredObject coloredObj = item as ColoredObject;
			Vector2 initialPosition = new Vector2(71f, 13f) * 64f + new Vector2((float)(8 + Game1.random.Next(6)), 2f) * 4f;
			foreach (bool isColorOverlay in new bool[]
			{
				default(bool),
				true
			})
			{
				if (!isColorOverlay || (coloredObj != null && !coloredObj.ColorSameIndexAsParentSheetIndex))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect((isColorOverlay > false) ? 1 : 0, null), initialPosition, false, 0f, Color.White)
					{
						interval = 9999f,
						scale = 4f,
						alphaFade = 0.045f,
						layerDepth = 0.096225f,
						motion = new Vector2(0f, 0.3f),
						acceleration = new Vector2(0f, 0.2f),
						scaleChange = -0.05f,
						color = ((coloredObj != null) ? coloredObj.color.Value : Color.White)
					});
				}
			}
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x00097E64 File Offset: 0x00096064
		public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location = null)
		{
			if (this._fishLocationOverride == null)
			{
				this._fishLocationOverride = "";
				string[] fields = base.GetMapPropertySplitBySpaces("FarmFishLocationOverride");
				if (fields.Length != 0)
				{
					string targetLocation;
					string error;
					float chance;
					if (!ArgUtility.TryGet(fields, 0, out targetLocation, out error, true, "string targetLocation") || !ArgUtility.TryGetFloat(fields, 1, out chance, out error, "float chance"))
					{
						base.LogMapPropertyError("FarmFishLocationOverride", fields, error, ' ');
					}
					else
					{
						this._fishLocationOverride = targetLocation;
						this._fishChanceOverride = chance;
					}
				}
			}
			if (this._fishChanceOverride > 0f && Game1.random.NextDouble() < (double)this._fishChanceOverride)
			{
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, this._fishLocationOverride);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, null);
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x00097F20 File Offset: 0x00096120
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (!this.greenhouseUnlocked.Value && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry"))
			{
				this.greenhouseUnlocked.Value = true;
			}
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				if (Game1.timeOfDay >= 1300 && this.characters[i].isMarried() && this.characters[i].controller == null)
				{
					this.characters[i].Halt();
					this.characters[i].drawOffset = Vector2.Zero;
					this.characters[i].Sprite.StopAnimation();
					FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(this.characters[i].getSpouse().homeLocation.Value, false);
					Game1.warpCharacter(this.characters[i], this.characters[i].getSpouse().homeLocation.Value, farmHouse.getKitchenStandingSpot());
					return;
				}
			}
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x00098043 File Offset: 0x00096243
		public virtual void UpdatePatio()
		{
			if (Game1.MasterPlayer.isMarriedOrRoommates() && Game1.MasterPlayer.spouse != null)
			{
				this.addSpouseOutdoorArea(Game1.MasterPlayer.spouse);
				return;
			}
			this.addSpouseOutdoorArea("");
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x00098079 File Offset: 0x00096279
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			this.ClearGreenhouseGrassTiles();
			this.UpdatePatio();
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x00098090 File Offset: 0x00096290
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.hasSeenGrandpaNote = Game1.player.hasOrWillReceiveMail("hasSeenGrandpaNote");
			if (Game1.player.mailReceived.Add("button_tut_2"))
			{
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(1));
			}
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				Child child = this.characters[i] as Child;
				if (child != null)
				{
					child.resetForPlayerEntry(this);
				}
			}
			this.addGrandpaCandles();
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("Farm_Eternal_Parrots") && !base.IsRainingHere())
			{
				for (int j = 0; j < 20; j++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(49, 24 * Game1.random.Next(4), 24, 24), new Vector2((float)Game1.viewport.MaxCorner.X, (float)(Game1.viewport.Location.Y + Game1.random.Next(64, Game1.viewport.Height / 2))), false, 0f, Color.White)
					{
						scale = 4f,
						motion = new Vector2(-5f + (float)Game1.random.Next(-10, 11) / 10f, 4f + (float)Game1.random.Next(-10, 11) / 10f),
						acceleration = new Vector2(0f, -0.02f),
						animationLength = 3,
						interval = 100f,
						pingPong = true,
						totalNumberOfLoops = 999,
						delayBeforeAnimationStart = j * 250,
						drawAboveAlwaysFront = true,
						startSound = "batFlap"
					});
				}
				DelayedAction.playSoundAfterDelay("parrot_squawk", 1000, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 4000, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("parrot", 3000, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("parrot", 5500, null, null, -1, false);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 7000, null, null, -1, false);
				for (int k = 0; k < 20; k++)
				{
					DelayedAction.playSoundAfterDelay("batFlap", 5000 + k * 250, null, null, -1, false);
				}
				Game1.player.mailReceived.Add("Farm_Eternal_Parrots");
			}
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x00098354 File Offset: 0x00096554
		public virtual Vector2 GetSpouseOutdoorAreaCorner()
		{
			if (this.mapSpouseAreaCorner == null)
			{
				Vector2 position;
				if (!base.TryGetMapPropertyAs("SpouseAreaLocation", out position, false))
				{
					position = new Vector2(69f, 6f);
				}
				this.mapSpouseAreaCorner = new Vector2?(position);
			}
			return this.mapSpouseAreaCorner.Value;
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x000983A8 File Offset: 0x000965A8
		public virtual void CacheOffBasePatioArea()
		{
			this._baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();
			List<string> layers_to_cache = new List<string>();
			foreach (Layer layer in this.map.Layers)
			{
				layers_to_cache.Add(layer.Id);
			}
			foreach (string layer_name in layers_to_cache)
			{
				Layer original_layer = this.map.GetLayer(layer_name);
				Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
				this._baseSpouseAreaTiles[layer_name] = tiles;
				Vector2 spouse_area_corner = this.GetSpouseOutdoorAreaCorner();
				for (int x = (int)spouse_area_corner.X; x < (int)spouse_area_corner.X + 4; x++)
				{
					for (int y = (int)spouse_area_corner.Y; y < (int)spouse_area_corner.Y + 4; y++)
					{
						if (original_layer == null)
						{
							tiles[new Point(x, y)] = null;
						}
						else
						{
							tiles[new Point(x, y)] = original_layer.Tiles[x, y];
						}
					}
				}
			}
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x000984F4 File Offset: 0x000966F4
		public virtual void ReapplyBasePatioArea()
		{
			foreach (string layer in this._baseSpouseAreaTiles.Keys)
			{
				Layer map_layer = this.map.GetLayer(layer);
				foreach (Point location in this._baseSpouseAreaTiles[layer].Keys)
				{
					Tile base_tile = this._baseSpouseAreaTiles[layer][location];
					if (map_layer != null)
					{
						map_layer.Tiles[location.X, location.Y] = base_tile;
					}
				}
			}
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x000985D4 File Offset: 0x000967D4
		public void addSpouseOutdoorArea(string spouseName)
		{
			this.ReapplyBasePatioArea();
			Point patio_corner = Utility.Vector2ToPoint(this.GetSpouseOutdoorAreaCorner());
			this.spousePatioSpot = new Point(patio_corner.X + 2, patio_corner.Y + 3);
			CharacterData spouseData;
			CharacterSpousePatioData patioData = NPC.TryGetData(spouseName, out spouseData) ? spouseData.SpousePatio : null;
			if (patioData != null)
			{
				string assetName = patioData.MapAsset ?? "spousePatios";
				Microsoft.Xna.Framework.Rectangle sourceArea = patioData.MapSourceRect;
				int width = Math.Min(sourceArea.Width, 4);
				int height = Math.Min(sourceArea.Height, 4);
				Point corner = patio_corner;
				Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, width, height);
				Point fromOrigin = sourceArea.Location;
				if (this._appliedMapOverrides.Contains("spouse_patio"))
				{
					this._appliedMapOverrides.Remove("spouse_patio");
				}
				base.ApplyMapOverride(assetName, "spouse_patio", new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height)), new Microsoft.Xna.Framework.Rectangle?(areaToRefurbish));
				foreach (Point tile in areaToRefurbish.GetPoints())
				{
					if (base.getTileIndexAt(tile, "Paths", null) == 7)
					{
						this.spousePatioSpot = tile;
						break;
					}
				}
			}
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x0009873C File Offset: 0x0009693C
		public void addGrandpaCandles()
		{
			Point grandpa_shrine_location = this.GetGrandpaShrinePosition();
			if (this.grandpaScore.Value > 0)
			{
				Microsoft.Xna.Framework.Rectangle candleSource = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
				base.removeTemporarySpritesWithIDLocal(6666);
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((float)((grandpa_shrine_location.X - 1) * 64 + 20), (float)((grandpa_shrine_location.Y - 1) * 64 + 20)), false, false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((float)((grandpa_shrine_location.X - 1) * 64 + 12), (float)((grandpa_shrine_location.Y - 1) * 64 - 4)), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					lightId = "Farm_GrandpaCandles_1",
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 0
				});
				if (this.grandpaScore.Value > 1)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((float)((grandpa_shrine_location.X - 1) * 64 + 40), (float)((grandpa_shrine_location.Y - 2) * 64 + 24)), false, false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((float)((grandpa_shrine_location.X - 1) * 64 + 36), (float)((grandpa_shrine_location.Y - 2) * 64)), false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						lightId = "Farm_GrandpaCandles_2",
						id = 6666,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 50
					});
				}
				if (this.grandpaScore.Value > 2)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((float)((grandpa_shrine_location.X + 1) * 64 + 20), (float)((grandpa_shrine_location.Y - 2) * 64 + 24)), false, false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((float)((grandpa_shrine_location.X + 1) * 64 + 16), (float)((grandpa_shrine_location.Y - 2) * 64)), false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						lightId = "Farm_GrandpaCandles_3",
						id = 6666,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 100
					});
				}
				if (this.grandpaScore.Value > 3)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((float)((grandpa_shrine_location.X + 1) * 64 + 40), (float)((grandpa_shrine_location.Y - 1) * 64 + 20)), false, false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((float)((grandpa_shrine_location.X + 1) * 64 + 36), (float)((grandpa_shrine_location.Y - 1) * 64 - 4)), false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						lightId = "Farm_GrandpaCandles_4",
						id = 6666,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 150
					});
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(176, 157, 15, 16), 99999f, 1, 9999, new Vector2((float)(grandpa_shrine_location.X * 64 + 4), (float)((grandpa_shrine_location.Y - 2) * 64 - 24)), false, false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
			}
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x00098CE8 File Offset: 0x00096EE8
		private void openShippingBinLid()
		{
			if (this.shippingBinLid != null)
			{
				if (this.shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
				{
					base.localSound("doorCreak", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = 1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x00098D4C File Offset: 0x00096F4C
		private void closeShippingBinLid()
		{
			if (this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex > 0)
			{
				if (this.shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
				{
					base.localSound("doorCreakReverse", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = -1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x00098DBC File Offset: 0x00096FBC
		private void updateShippingBinLid(GameTime time)
		{
			if (this.isShippingBinLidOpen(true) && this.shippingBinLid.pingPongMotion == 1)
			{
				this.shippingBinLid.paused = true;
			}
			else if (this.shippingBinLid.currentParentTileIndex == 0 && this.shippingBinLid.pingPongMotion == -1)
			{
				if (!this.shippingBinLid.paused && Game1.currentLocation == this)
				{
					base.localSound("woodyStep", null, null, SoundContext.Default);
				}
				this.shippingBinLid.paused = true;
			}
			this.shippingBinLid.update(time);
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x00098E55 File Offset: 0x00097055
		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			return this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex >= (requiredToBeFullyOpen ? (this.shippingBinLid.animationLength - 1) : 1);
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x00098E84 File Offset: 0x00097084
		public override void pokeTileForConstruction(Vector2 tile)
		{
			base.pokeTileForConstruction(tile);
			foreach (NPC npc in this.characters)
			{
				Pet pet = npc as Pet;
				if (pet != null && pet.Tile == tile)
				{
					pet.FacingDirection = Game1.random.Next(0, 4);
					pet.faceDirection(pet.FacingDirection);
					pet.CurrentBehavior = "Walk";
					pet.forceUpdateTimer = 2000;
					pet.setMovingInFacingDirection();
				}
			}
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x00098F28 File Offset: 0x00097128
		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			return (this.doesTileHaveProperty((int)p.X, (int)p.Y, "NoSpawn", "Back", false) == "All" && this.doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back", false) == "Wood") || base.shouldShadowBeDrawnAboveBuildingsLayer(p);
		}

		// Token: 0x06000E41 RID: 3649 RVA: 0x00098F94 File Offset: 0x00097194
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.mailbox.Count > 0)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point mailbox_position = Game1.player.getMailboxPosition();
				float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64), (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64 + 32 + 4), (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13)), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.shippingBinLid;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			if (!this.hasSeenGrandpaNote)
			{
				Point grandpa_shrine = this.GetGrandpaShrinePosition();
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((grandpa_shrine.X + 1) * 64), (float)(grandpa_shrine.Y * 64))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(grandpa_shrine.Y * 64) / 10000f + 1E-06f);
			}
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x000991A4 File Offset: 0x000973A4
		public virtual Point GetMainMailboxPosition()
		{
			if (this.mapMainMailboxPosition == null)
			{
				Point position;
				if (!base.TryGetMapPropertyAs("MailboxLocation", out position, false))
				{
					position = new Point(68, 16);
				}
				this.mapMainMailboxPosition = new Point?(position);
				Building farmhouse = this.GetMainFarmHouse();
				BuildingData buildingData = (farmhouse != null) ? farmhouse.GetData() : null;
				if (((buildingData != null) ? buildingData.ActionTiles : null) != null)
				{
					foreach (BuildingActionTile action in buildingData.ActionTiles)
					{
						if (action.Action == "Mailbox")
						{
							this.mapMainMailboxPosition = new Point?(new Point(farmhouse.tileX.Value + action.Tile.X, farmhouse.tileY.Value + action.Tile.Y));
							break;
						}
					}
				}
			}
			return this.mapMainMailboxPosition.Value;
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x000992B0 File Offset: 0x000974B0
		public virtual Point GetGrandpaShrinePosition()
		{
			if (this.mapGrandpaShrinePosition == null)
			{
				Point position;
				if (!base.TryGetMapPropertyAs("GrandpaShrineLocation", out position, false))
				{
					position = new Point(8, 7);
				}
				this.mapGrandpaShrinePosition = new Point?(position);
			}
			return this.mapGrandpaShrinePosition.Value;
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x000992FC File Offset: 0x000974FC
		public virtual Point GetMainFarmHouseEntry()
		{
			if (this.mainFarmhouseEntry == null)
			{
				Point position;
				if (!base.TryGetMapPropertyAs("FarmHouseEntry", out position, false))
				{
					position = new Point(64, 15);
				}
				this.mainFarmhouseEntry = new Point?(position);
				Building farmhouse = this.GetMainFarmHouse();
				if (farmhouse != null)
				{
					this.mainFarmhouseEntry = new Point?(new Point(farmhouse.tileX.Value + farmhouse.humanDoor.X, farmhouse.tileY.Value + farmhouse.humanDoor.Y + 1));
				}
			}
			return this.mainFarmhouseEntry.Value;
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x00099392 File Offset: 0x00097592
		public virtual Building GetMainFarmHouse()
		{
			return base.getBuildingByType("Farmhouse");
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x000993A0 File Offset: 0x000975A0
		public override void ResetForEvent(Event ev)
		{
			base.ResetForEvent(ev);
			if (ev.id != "-2")
			{
				Point main_farmhouse_entry = Farm.getFrontDoorPositionForFarmer(ev.farmer);
				main_farmhouse_entry.Y++;
				int offset_x = main_farmhouse_entry.X - 64;
				int offset_y = main_farmhouse_entry.Y - 15;
				ev.eventPositionTileOffset = new Vector2((float)offset_x, (float)offset_y);
			}
		}

		// Token: 0x06000E47 RID: 3655 RVA: 0x00099401 File Offset: 0x00097601
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			this.spawnCrowEvent.Poll();
			this.lightningStrikeEvent.Poll();
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x00099424 File Offset: 0x00097624
		public bool isTileOpenBesidesTerrainFeatures(Vector2 tile)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.intersects(boundingBox))
					{
						return false;
					}
				}
			}
			using (List<ResourceClump>.Enumerator enumerator2 = this.resourceClumps.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.getBoundingBox().Intersects(boundingBox))
					{
						return false;
					}
				}
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp in this.animals.Pairs)
			{
				if (kvp.Value.Tile == tile)
				{
					return true;
				}
			}
			return !this.objects.ContainsKey(tile) && base.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x00099580 File Offset: 0x00097780
		private void doLightningStrike(Farm.LightningStrikeEvent lightning)
		{
			if (lightning.smallFlash)
			{
				if (Game1.currentLocation.IsOutdoors && !Game1.newDay && Game1.currentLocation.IsLightningHere())
				{
					Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
					if (Game1.random.NextBool())
					{
						DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000), null);
					}
					DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500), null, null, -1, false);
				}
			}
			else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsLightningHere() && !Game1.newDay)
			{
				Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
				Game1.playSound("thunder", null);
			}
			if (lightning.createBolt && Game1.currentLocation.name.Equals("Farm"))
			{
				if (lightning.destroyedTerrainFeature)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, false, false));
				}
				Utility.drawLightningBolt(lightning.boltPosition, this);
			}
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x000996EC File Offset: 0x000978EC
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this.wasUpdated && Game1.gameMode != 0)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			if (this.shippingBinLid != null)
			{
				bool opening = false;
				using (FarmerCollection.Enumerator enumerator = this.farmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.GetBoundingBox().Intersects(this.shippingBinLidOpenArea))
						{
							this.openShippingBinLid();
							opening = true;
						}
					}
				}
				if (!opening)
				{
					this.closeShippingBinLid();
				}
				this.updateShippingBinLid(time);
			}
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x00099788 File Offset: 0x00097988
		public bool ShouldSpawnMountainOres()
		{
			if (this._mountainForageRectangle == null)
			{
				Microsoft.Xna.Framework.Rectangle area;
				this._mountainForageRectangle = new Microsoft.Xna.Framework.Rectangle?(base.TryGetMapPropertyAs("SpawnMountainFarmOreRect", out area, false) ? area : Microsoft.Xna.Framework.Rectangle.Empty);
			}
			return this._mountainForageRectangle.Value.Width > 0;
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x000997D8 File Offset: 0x000979D8
		public bool ShouldSpawnForestFarmForage()
		{
			if (this.map != null)
			{
				if (this._shouldSpawnForestFarmForage == null)
				{
					this._shouldSpawnForestFarmForage = new bool?(this.map.Properties.ContainsKey("SpawnForestFarmForage"));
				}
				if (this._shouldSpawnForestFarmForage.Value)
				{
					return true;
				}
			}
			return Game1.whichFarm == 2;
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00099834 File Offset: 0x00097A34
		public bool ShouldSpawnBeachFarmForage()
		{
			if (this.map != null)
			{
				if (this._shouldSpawnBeachFarmForage == null)
				{
					this._shouldSpawnBeachFarmForage = new bool?(this.map.Properties.ContainsKey("SpawnBeachFarmForage"));
				}
				if (this._shouldSpawnBeachFarmForage.Value)
				{
					return true;
				}
			}
			return Game1.whichFarm == 6;
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x0009988D File Offset: 0x00097A8D
		public bool SpawnsForage()
		{
			return this.ShouldSpawnForestFarmForage() || this.ShouldSpawnBeachFarmForage();
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0009989F File Offset: 0x00097A9F
		public bool doesFarmCaveNeedHarvesting()
		{
			return this.farmCaveReady.Value;
		}

		// Token: 0x04000953 RID: 2387
		[XmlIgnore]
		[NonInstancedStatic]
		public static Texture2D houseTextures = Game1.content.Load<Texture2D>("Buildings\\houses");

		// Token: 0x04000954 RID: 2388
		[NotNetField]
		public NetRef<BuildingPaintColor> housePaintColor = new NetRef<BuildingPaintColor>();

		// Token: 0x04000955 RID: 2389
		public const int default_layout = 0;

		// Token: 0x04000956 RID: 2390
		public const int riverlands_layout = 1;

		// Token: 0x04000957 RID: 2391
		public const int forest_layout = 2;

		// Token: 0x04000958 RID: 2392
		public const int mountains_layout = 3;

		// Token: 0x04000959 RID: 2393
		public const int combat_layout = 4;

		// Token: 0x0400095A RID: 2394
		public const int fourCorners_layout = 5;

		// Token: 0x0400095B RID: 2395
		public const int beach_layout = 6;

		// Token: 0x0400095C RID: 2396
		public const int mod_layout = 7;

		// Token: 0x0400095D RID: 2397
		public const int layout_max = 7;

		// Token: 0x0400095E RID: 2398
		[XmlElement("grandpaScore")]
		public readonly NetInt grandpaScore = new NetInt(0);

		// Token: 0x0400095F RID: 2399
		[XmlElement("farmCaveReady")]
		public NetBool farmCaveReady = new NetBool(false);

		// Token: 0x04000960 RID: 2400
		private TemporaryAnimatedSprite shippingBinLid;

		// Token: 0x04000961 RID: 2401
		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

		// Token: 0x04000962 RID: 2402
		[XmlIgnore]
		private readonly NetRef<Inventory> sharedShippingBin = new NetRef<Inventory>(new Inventory());

		// Token: 0x04000963 RID: 2403
		[XmlIgnore]
		public Item lastItemShipped;

		// Token: 0x04000964 RID: 2404
		public bool hasSeenGrandpaNote;

		// Token: 0x04000965 RID: 2405
		protected Dictionary<string, Dictionary<Point, Tile>> _baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();

		// Token: 0x04000966 RID: 2406
		[XmlIgnore]
		public bool hasMatureFairyRoseTonight;

		// Token: 0x04000967 RID: 2407
		[XmlElement("greenhouseUnlocked")]
		public readonly NetBool greenhouseUnlocked = new NetBool();

		// Token: 0x04000968 RID: 2408
		[XmlElement("greenhouseMoved")]
		public readonly NetBool greenhouseMoved = new NetBool();

		// Token: 0x04000969 RID: 2409
		private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

		// Token: 0x0400096A RID: 2410
		public readonly NetEvent1<Farm.LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<Farm.LightningStrikeEvent>();

		// Token: 0x0400096B RID: 2411
		[XmlIgnore]
		public Point? mapGrandpaShrinePosition;

		// Token: 0x0400096C RID: 2412
		[XmlIgnore]
		public Point? mapMainMailboxPosition;

		// Token: 0x0400096D RID: 2413
		[XmlIgnore]
		public Point? mainFarmhouseEntry;

		// Token: 0x0400096E RID: 2414
		[XmlIgnore]
		public Vector2? mapSpouseAreaCorner;

		// Token: 0x0400096F RID: 2415
		[XmlIgnore]
		public Vector2? mapShippingBinPosition;

		// Token: 0x04000970 RID: 2416
		protected Microsoft.Xna.Framework.Rectangle? _mountainForageRectangle;

		// Token: 0x04000971 RID: 2417
		protected bool? _shouldSpawnForestFarmForage;

		// Token: 0x04000972 RID: 2418
		protected bool? _shouldSpawnBeachFarmForage;

		// Token: 0x04000973 RID: 2419
		protected bool? _oceanCrabPotOverride;

		// Token: 0x04000974 RID: 2420
		protected string _fishLocationOverride;

		// Token: 0x04000975 RID: 2421
		protected float _fishChanceOverride;

		// Token: 0x04000976 RID: 2422
		public Point spousePatioSpot;

		// Token: 0x04000977 RID: 2423
		public const int numCropsForCrow = 16;

		// Token: 0x02000475 RID: 1141
		public class LightningStrikeEvent : NetEventArg
		{
			// Token: 0x06003E41 RID: 15937 RVA: 0x002F9530 File Offset: 0x002F7730
			public void Read(BinaryReader reader)
			{
				this.createBolt = reader.ReadBoolean();
				this.bigFlash = reader.ReadBoolean();
				this.smallFlash = reader.ReadBoolean();
				this.destroyedTerrainFeature = reader.ReadBoolean();
				this.boltPosition.X = (float)reader.ReadInt32();
				this.boltPosition.Y = (float)reader.ReadInt32();
			}

			// Token: 0x06003E42 RID: 15938 RVA: 0x002F9594 File Offset: 0x002F7794
			public void Write(BinaryWriter writer)
			{
				writer.Write(this.createBolt);
				writer.Write(this.bigFlash);
				writer.Write(this.smallFlash);
				writer.Write(this.destroyedTerrainFeature);
				writer.Write((int)this.boltPosition.X);
				writer.Write((int)this.boltPosition.Y);
			}

			// Token: 0x0400284E RID: 10318
			public Vector2 boltPosition;

			// Token: 0x0400284F RID: 10319
			public bool createBolt;

			// Token: 0x04002850 RID: 10320
			public bool bigFlash;

			// Token: 0x04002851 RID: 10321
			public bool smallFlash;

			// Token: 0x04002852 RID: 10322
			public bool destroyedTerrainFeature;
		}
	}
}
