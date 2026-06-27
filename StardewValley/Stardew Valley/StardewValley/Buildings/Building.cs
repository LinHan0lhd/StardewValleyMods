using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Util;
using xTile.Dimensions;

namespace StardewValley.Buildings
{
	// Token: 0x0200037F RID: 895
	[XmlInclude(typeof(Barn))]
	[XmlInclude(typeof(Coop))]
	[XmlInclude(typeof(FishPond))]
	[XmlInclude(typeof(GreenhouseBuilding))]
	[XmlInclude(typeof(JunimoHut))]
	[XmlInclude(typeof(Mill))]
	[XmlInclude(typeof(PetBowl))]
	[XmlInclude(typeof(ShippingBin))]
	[XmlInclude(typeof(Stable))]
	[NotImplicitNetField]
	public class Building : INetObject<NetFields>, IHaveModData
	{
		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x060036FE RID: 14078 RVA: 0x002B6ED9 File Offset: 0x002B50D9
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x060036FF RID: 14079 RVA: 0x002B6EE1 File Offset: 0x002B50E1
		// (set) Token: 0x06003700 RID: 14080 RVA: 0x002B6EEE File Offset: 0x002B50EE
		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return this.modData.GetForSerialization();
			}
			set
			{
				this.modData.SetFromSerialization(value);
			}
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06003701 RID: 14081 RVA: 0x002B6EFC File Offset: 0x002B50FC
		public bool isCabin
		{
			get
			{
				return this.buildingType.Value == "Cabin";
			}
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06003702 RID: 14082 RVA: 0x002B6F13 File Offset: 0x002B5113
		// (set) Token: 0x06003703 RID: 14083 RVA: 0x002B6F1B File Offset: 0x002B511B
		public bool isMoving
		{
			get
			{
				return this._isMoving;
			}
			set
			{
				if (this._isMoving != value)
				{
					this._isMoving = value;
					if (this._isMoving)
					{
						this.OnStartMove();
					}
					if (!this._isMoving)
					{
						this.OnEndMove();
					}
				}
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06003704 RID: 14084 RVA: 0x002B6F49 File Offset: 0x002B5149
		public NetFields NetFields { get; } = new NetFields("Building");

		// Token: 0x06003705 RID: 14085 RVA: 0x002B6F54 File Offset: 0x002B5154
		public Building()
		{
			this.id.Value = Guid.NewGuid();
			this.resetTexture();
			this.initNetFields();
		}

		// Token: 0x06003706 RID: 14086 RVA: 0x002B7104 File Offset: 0x002B5304
		public Building(string type, Vector2 tile) : this()
		{
			this.tileX.Value = (int)tile.X;
			this.tileY.Value = (int)tile.Y;
			this.buildingType.Value = type;
			BuildingData data = this.ReloadBuildingData(false, false);
			this.daysOfConstructionLeft.Value = ((data != null) ? data.BuildDays : 0);
		}

		// Token: 0x06003707 RID: 14087 RVA: 0x002B7168 File Offset: 0x002B5368
		public virtual bool CanBeReskinned(bool ignoreSeparateConstructionEntries = false)
		{
			BuildingData data = this.GetData();
			if (this.skinId.Value != null)
			{
				return true;
			}
			if (((data != null) ? data.Skins : null) != null)
			{
				foreach (BuildingSkin skin in data.Skins)
				{
					if (!(skin.Id == this.skinId.Value) && (!ignoreSeparateConstructionEntries || !skin.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin.Condition, this.GetParentLocation(), null, null, null, null, null))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06003708 RID: 14088 RVA: 0x002B721C File Offset: 0x002B541C
		public bool AllowsAnimalPregnancy()
		{
			BuildingData data = this.GetData();
			return data != null && data.AllowAnimalPregnancy;
		}

		// Token: 0x06003709 RID: 14089 RVA: 0x002B7230 File Offset: 0x002B5430
		public virtual bool CanBePainted()
		{
			if (this is GreenhouseBuilding && !Game1.getFarm().greenhouseUnlocked.Value)
			{
				return false;
			}
			if (this.isCabin || this.HasIndoorsName("Farmhouse"))
			{
				FarmHouse house = this.GetIndoors() as FarmHouse;
				if (house != null && house.upgradeLevel < 2)
				{
					return false;
				}
			}
			return this.GetPaintDataKey() != null;
		}

		// Token: 0x0600370A RID: 14090 RVA: 0x002B7290 File Offset: 0x002B5490
		public BuildingSkin GetSkin()
		{
			return Building.GetSkin(this.skinId.Value, this.GetData());
		}

		// Token: 0x0600370B RID: 14091 RVA: 0x002B72A8 File Offset: 0x002B54A8
		public static BuildingSkin GetSkin(string skinId, BuildingData data)
		{
			if (skinId != null && ((data != null) ? data.Skins : null) != null)
			{
				foreach (BuildingSkin skin in data.Skins)
				{
					if (skin.Id == skinId)
					{
						return skin;
					}
				}
			}
			return null;
		}

		// Token: 0x0600370C RID: 14092 RVA: 0x002B731C File Offset: 0x002B551C
		public virtual string GetPaintDataKey()
		{
			Dictionary<string, string> asset = DataLoader.PaintData(Game1.content);
			return this.GetPaintDataKey(asset);
		}

		// Token: 0x0600370D RID: 14093 RVA: 0x002B733C File Offset: 0x002B553C
		public virtual string GetPaintDataKey(Dictionary<string, string> paintData)
		{
			if (this.skinId.Value != null && paintData.ContainsKey(this.skinId.Value))
			{
				return this.skinId.Value;
			}
			string value = this.buildingType.Value;
			string lookupName;
			if (!(value == "Farmhouse"))
			{
				if (!(value == "Cabin"))
				{
					lookupName = this.buildingType.Value;
				}
				else
				{
					lookupName = "Stone Cabin";
				}
			}
			else
			{
				lookupName = "House";
			}
			if (!paintData.ContainsKey(lookupName))
			{
				return null;
			}
			return lookupName;
		}

		// Token: 0x0600370E RID: 14094 RVA: 0x002B73C8 File Offset: 0x002B55C8
		public string GetMetadata(string key)
		{
			if (this.buildingMetadata == null)
			{
				this.buildingMetadata = new Dictionary<string, string>();
				BuildingData data = this.GetData();
				if (data != null)
				{
					foreach (KeyValuePair<string, string> kvp in data.Metadata)
					{
						this.buildingMetadata[kvp.Key] = kvp.Value;
					}
					BuildingSkin skin = Building.GetSkin(this.skinId.Value, data);
					if (skin != null)
					{
						foreach (KeyValuePair<string, string> kvp2 in skin.Metadata)
						{
							this.buildingMetadata[kvp2.Key] = kvp2.Value;
						}
					}
				}
			}
			if (!this.buildingMetadata.TryGetValue(key, out key))
			{
				return null;
			}
			return key;
		}

		// Token: 0x0600370F RID: 14095 RVA: 0x002B74D0 File Offset: 0x002B56D0
		public GameLocation GetParentLocation()
		{
			return Game1.getLocationFromName(this.parentLocationName.Value);
		}

		// Token: 0x06003710 RID: 14096 RVA: 0x002B74E2 File Offset: 0x002B56E2
		public bool IsInCurrentLocation()
		{
			return Game1.currentLocation != null && Game1.currentLocation.NameOrUniqueName == this.parentLocationName.Value;
		}

		// Token: 0x06003711 RID: 14097 RVA: 0x002B7508 File Offset: 0x002B5708
		public virtual bool hasCarpenterPermissions()
		{
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (this.owner.Value == Game1.player.UniqueMultiplayerID)
			{
				return true;
			}
			FarmHouse farmHouse = this.GetIndoors() as FarmHouse;
			return farmHouse != null && farmHouse.IsOwnedByCurrentPlayer;
		}

		// Token: 0x06003712 RID: 14098 RVA: 0x002B7554 File Offset: 0x002B5754
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.id, "id").AddField(this.indoors, "indoors").AddField(this.nonInstancedIndoorsName, "nonInstancedIndoorsName").AddField(this.tileX, "tileX").AddField(this.tileY, "tileY").AddField(this.tilesWide, "tilesWide").AddField(this.tilesHigh, "tilesHigh").AddField(this.maxOccupants, "maxOccupants").AddField(this.currentOccupants, "currentOccupants").AddField(this.daysOfConstructionLeft, "daysOfConstructionLeft").AddField(this.daysUntilUpgrade, "daysUntilUpgrade").AddField(this.buildingType, "buildingType").AddField(this.humanDoor, "humanDoor").AddField(this.animalDoor, "animalDoor").AddField(this.magical, "magical").AddField(this.fadeWhenPlayerIsBehind, "fadeWhenPlayerIsBehind").AddField(this.animalDoorOpen, "animalDoorOpen").AddField(this.owner, "owner").AddField(this.newConstructionTimer, "newConstructionTimer").AddField(this.netBuildingPaintColor, "netBuildingPaintColor").AddField(this.buildingChests, "buildingChests").AddField(this.animalDoorOpenAmount, "animalDoorOpenAmount").AddField(this.hayCapacity, "hayCapacity").AddField(this.parentLocationName, "parentLocationName").AddField(this.upgradeName, "upgradeName").AddField(this.skinId, "skinId").AddField(this.modData, "modData");
			this.buildingType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.hasChimney = null;
				bool isUpgrade = b != null && b != c;
				this.ReloadBuildingData(isUpgrade, false);
			};
			this.skinId.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.hasChimney = null;
				this.buildingMetadata = null;
				this.resetTexture();
			};
			this.buildingType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.hasChimney = null;
				this.buildingMetadata = null;
				this.resetTexture();
			};
			this.indoors.fieldChangeVisibleEvent += delegate(NetRef<GameLocation> field, GameLocation oldValue, GameLocation newValue)
			{
				this.UpdateIndoorParent();
			};
			this.parentLocationName.fieldChangeVisibleEvent += delegate(NetString field, string oldValue, string newValue)
			{
				this.UpdateIndoorParent();
			};
			if (this.netBuildingPaintColor.Value == null)
			{
				this.netBuildingPaintColor.Value = new BuildingPaintColor();
			}
		}

		// Token: 0x06003713 RID: 14099 RVA: 0x002B77B0 File Offset: 0x002B59B0
		public virtual void UpdateIndoorParent()
		{
			GameLocation interior = this.GetIndoors();
			if (interior != null)
			{
				interior.ParentBuilding = this;
				interior.parentLocationName.Value = this.parentLocationName.Value;
			}
		}

		// Token: 0x06003714 RID: 14100 RVA: 0x002B77E4 File Offset: 0x002B59E4
		public virtual BuildingData GetData()
		{
			BuildingData data;
			if (!Building.TryGetData(this.buildingType.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06003715 RID: 14101 RVA: 0x002B7808 File Offset: 0x002B5A08
		public static bool TryGetData(string buildingType, out BuildingData data)
		{
			if (buildingType == null)
			{
				data = null;
				return false;
			}
			return Game1.buildingData.TryGetValue(buildingType, out data);
		}

		// Token: 0x06003716 RID: 14102 RVA: 0x002B7820 File Offset: 0x002B5A20
		public virtual BuildingData ReloadBuildingData(bool forUpgrade = false, bool forConstruction = false)
		{
			BuildingData data = this.GetData();
			if (data != null)
			{
				this.LoadFromBuildingData(data, forUpgrade, forConstruction);
			}
			return data;
		}

		// Token: 0x06003717 RID: 14103 RVA: 0x002B7844 File Offset: 0x002B5A44
		public virtual void LoadFromBuildingData(BuildingData data, bool forUpgrade = false, bool forConstruction = false)
		{
			if (data == null)
			{
				return;
			}
			this.tilesWide.Value = data.Size.X;
			this.tilesHigh.Value = data.Size.Y;
			this.humanDoor.X = data.HumanDoor.X;
			this.humanDoor.Y = data.HumanDoor.Y;
			this.animalDoor.Value = data.AnimalDoor.Location;
			if (data.MaxOccupants >= 0)
			{
				this.maxOccupants.Value = data.MaxOccupants;
			}
			this.hayCapacity.Value = data.HayCapacity;
			this.magical.Value = (data.Builder == "Wizard");
			this.fadeWhenPlayerIsBehind.Value = data.FadeWhenBehind;
			foreach (KeyValuePair<string, string> pair in data.ModData)
			{
				this.modData[pair.Key] = pair.Value;
			}
			GameLocation gameLocation = this.GetIndoors();
			if (gameLocation != null)
			{
				gameLocation.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
			}
			if (Game1.IsMasterGame)
			{
				if (this.hasLoaded || forConstruction)
				{
					if (this.nonInstancedIndoorsName.Value == null)
					{
						string mapPath = data.IndoorMap;
						string mapType = typeof(GameLocation).ToString();
						if (data.IndoorMapType != null)
						{
							mapType = data.IndoorMapType;
						}
						if (mapPath != null)
						{
							mapPath = "Maps\\" + mapPath;
							if (this.indoors.Value == null)
							{
								this.indoors.Value = this.createIndoors(data, data.IndoorMap);
								this.InitializeIndoor(data, forConstruction, forUpgrade);
							}
							else if (this.indoors.Value.mapPath.Value == mapPath)
							{
								if (forUpgrade)
								{
									this.InitializeIndoor(data, forConstruction, true);
								}
							}
							else
							{
								if (this.indoors.Value.GetType().ToString() != mapType)
								{
									this.load();
								}
								else
								{
									this.indoors.Value.mapPath.Value = mapPath;
									this.indoors.Value.updateMap();
								}
								this.updateInteriorWarps(this.indoors.Value);
								this.InitializeIndoor(data, forConstruction, forUpgrade);
							}
						}
					}
					else
					{
						this.updateInteriorWarps(null);
					}
				}
				if (this.hasLoaded || forConstruction)
				{
					HashSet<string> validChests = new HashSet<string>();
					if (data.Chests != null)
					{
						foreach (BuildingChest buildingChest in data.Chests)
						{
							validChests.Add(buildingChest.Id);
						}
					}
					this.buildingChests.RemoveWhere((Chest chest) => !validChests.Contains(chest.Name));
					if (data.Chests != null)
					{
						foreach (BuildingChest buildingChest2 in data.Chests)
						{
							if (this.GetBuildingChest(buildingChest2.Id) == null)
							{
								Chest newChest = new Chest(true, "130")
								{
									Name = buildingChest2.Id
								};
								this.buildingChests.Add(newChest);
							}
						}
					}
				}
			}
		}

		// Token: 0x06003718 RID: 14104 RVA: 0x002B7BC8 File Offset: 0x002B5DC8
		public static Building CreateInstanceFromId(string typeId, Vector2 tile)
		{
			BuildingData data;
			if (typeId != null && Game1.buildingData.TryGetValue(typeId, out data))
			{
				Type type = (data.BuildingType != null) ? Type.GetType(data.BuildingType) : null;
				if (type != null && type != typeof(Building))
				{
					try
					{
						return (Building)Activator.CreateInstance(type, new object[]
						{
							typeId,
							tile
						});
					}
					catch (MissingMethodException)
					{
						try
						{
							Building building = (Building)Activator.CreateInstance(type, new object[]
							{
								tile
							});
							building.buildingType.Value = typeId;
							return building;
						}
						catch (Exception e)
						{
							Game1.log.Error("Error trying to instantiate building for type '" + typeId + "'", e);
						}
					}
				}
			}
			return new Building(typeId, tile);
		}

		// Token: 0x06003719 RID: 14105 RVA: 0x002B7CB0 File Offset: 0x002B5EB0
		public virtual void InitializeIndoor(BuildingData data, bool forConstruction, bool forUpgrade)
		{
			if (data == null)
			{
				return;
			}
			GameLocation interior = this.GetIndoors();
			if (interior == null)
			{
				return;
			}
			AnimalHouse animalHouse = interior as AnimalHouse;
			if (animalHouse != null && data.MaxOccupants > 0)
			{
				animalHouse.animalLimit.Value = data.MaxOccupants;
			}
			if (forUpgrade && data.IndoorItemMoves != null)
			{
				foreach (IndoorItemMove move in data.IndoorItemMoves)
				{
					for (int x = 0; x < move.Size.X; x++)
					{
						for (int y = 0; y < move.Size.Y; y++)
						{
							interior.moveContents(move.Source.X + x, move.Source.Y + y, move.Destination.X + x, move.Destination.Y + y, move.UnlessItemId);
						}
					}
				}
			}
			if ((forConstruction || forUpgrade) && data.IndoorItems != null)
			{
				foreach (IndoorItemAdd item in data.IndoorItems)
				{
					Vector2 tileVector = Utility.PointToVector2(item.Tile);
					Object newObj = ItemRegistry.Create(item.ItemId, 1, 0, false) as Object;
					Furniture newFurniture = newObj as Furniture;
					if (newObj != null)
					{
						if (item.ClearTile)
						{
							if (newFurniture != null)
							{
								int y2 = 0;
								int height = newFurniture.getTilesHigh();
								while (y2 < height)
								{
									int x2 = 0;
									int width = newFurniture.getTilesWide();
									while (x2 < width)
									{
										interior.cleanUpTileForMapOverride(new Point((int)tileVector.X + x2, (int)tileVector.Y + y2), item.ItemId);
										x2++;
									}
									y2++;
								}
							}
							else
							{
								interior.cleanUpTileForMapOverride(Utility.Vector2ToPoint(tileVector), item.ItemId);
							}
						}
						if (!interior.IsTileBlockedBy(tileVector, CollisionMask.Furniture | CollisionMask.Objects, CollisionMask.None, false))
						{
							if (item.Indestructible)
							{
								newObj.fragility.Value = 2;
							}
							newObj.TileLocation = tileVector;
							if (newFurniture != null)
							{
								interior.furniture.Add(newFurniture);
							}
							else
							{
								interior.objects.Add(tileVector, newObj);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600371A RID: 14106 RVA: 0x002B7F30 File Offset: 0x002B6130
		public BuildingItemConversion GetItemConversionForItem(Item item, Chest chest)
		{
			if (item == null || chest == null)
			{
				return null;
			}
			BuildingData data = this.GetData();
			if (((data != null) ? data.ItemConversions : null) != null)
			{
				foreach (BuildingItemConversion conversion in data.ItemConversions)
				{
					if (conversion.SourceChest == chest.Name)
					{
						bool fail = false;
						foreach (string requiredTag in conversion.RequiredTags)
						{
							if (!item.HasContextTag(requiredTag))
							{
								fail = true;
								break;
							}
						}
						if (!fail)
						{
							return conversion;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x0600371B RID: 14107 RVA: 0x002B8008 File Offset: 0x002B6208
		public bool IsValidObjectForChest(Item item, Chest chest)
		{
			return this.GetItemConversionForItem(item, chest) != null;
		}

		// Token: 0x0600371C RID: 14108 RVA: 0x002B8018 File Offset: 0x002B6218
		public bool PerformBuildingChestAction(string name, Farmer who)
		{
			Chest chest = this.GetBuildingChest(name);
			if (chest == null)
			{
				return false;
			}
			BuildingChest chestData = this.GetBuildingChestData(name);
			if (chestData == null)
			{
				return false;
			}
			switch (chestData.Type)
			{
			case BuildingChestType.Chest:
				(Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, false, true, (Item item) => this.IsValidObjectForChest(item, chest), new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromChest), false, true, true, true, true, 1, null, -1, this, ItemExitBehavior.ReturnToPlayer, false)).inventory.moveItemSound = chestData.Sound;
				return true;
			case BuildingChestType.Collect:
				Utility.CollectSingleItemOrShowChestMenu(chest, null);
				return true;
			case BuildingChestType.Load:
				if (((who != null) ? who.ActiveObject : null) != null)
				{
					if (!this.IsValidObjectForChest(who.ActiveObject, chest))
					{
						if (chestData.InvalidItemMessage != null && (chestData.InvalidItemMessageCondition == null || GameStateQuery.CheckConditions(chestData.InvalidItemMessageCondition, this.GetParentLocation(), who, who.ActiveObject, who.ActiveObject, null, null)))
						{
							Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidItemMessage, null, null, null), true);
						}
						return false;
					}
					BuildingItemConversion conversion = this.GetItemConversionForItem(who.ActiveObject, chest);
					Utility.consolidateStacks(chest.Items);
					chest.clearNulls();
					int roomForItem = Utility.GetNumberOfItemThatCanBeAddedToThisInventoryList(who.ActiveObject, chest.Items, 36);
					if (who.ActiveObject.Stack > conversion.RequiredCount && roomForItem < conversion.RequiredCount)
					{
						Game1.showRedMessage(TokenParser.ParseText(chestData.ChestFullMessage, null, null, null), true);
						return false;
					}
					int acceptAmount = Math.Min(roomForItem, who.ActiveObject.Stack) / conversion.RequiredCount * conversion.RequiredCount;
					if (acceptAmount == 0)
					{
						if (chestData.InvalidCountMessage != null)
						{
							Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidCountMessage, null, null, null), true);
						}
						return false;
					}
					Item one = who.ActiveObject.getOne();
					if (who.ActiveObject.ConsumeStack(acceptAmount) == null)
					{
						who.ActiveObject = null;
					}
					one.Stack = acceptAmount;
					Utility.addItemToThisInventoryList(one, chest.Items, 36);
					if (chestData.Sound != null)
					{
						Game1.playSound(chestData.Sound, null);
					}
				}
				return true;
			default:
				return false;
			}
		}

		// Token: 0x0600371D RID: 14109 RVA: 0x002B8271 File Offset: 0x002B6471
		public BuildingChest GetBuildingChestData(string name)
		{
			return Building.GetBuildingChestData(this.GetData(), name);
		}

		// Token: 0x0600371E RID: 14110 RVA: 0x002B8280 File Offset: 0x002B6480
		public static BuildingChest GetBuildingChestData(BuildingData data, string name)
		{
			if (data == null)
			{
				return null;
			}
			foreach (BuildingChest buildingChestData in data.Chests)
			{
				if (buildingChestData.Id == name)
				{
					return buildingChestData;
				}
			}
			return null;
		}

		// Token: 0x0600371F RID: 14111 RVA: 0x002B82E8 File Offset: 0x002B64E8
		public Chest GetBuildingChest(string name)
		{
			foreach (Chest buildingChest in this.buildingChests)
			{
				if (buildingChest.Name == name)
				{
					return buildingChest;
				}
			}
			return null;
		}

		// Token: 0x06003720 RID: 14112 RVA: 0x002B834C File Offset: 0x002B654C
		public virtual string textureName()
		{
			BuildingData data = this.GetData();
			BuildingSkin skin = Building.GetSkin(this.skinId.Value, data);
			string result;
			if ((result = ((skin != null) ? skin.Texture : null)) == null)
			{
				result = (((data != null) ? data.Texture : null) ?? ("Buildings\\" + this.buildingType.Value));
			}
			return result;
		}

		// Token: 0x06003721 RID: 14113 RVA: 0x002B83A6 File Offset: 0x002B65A6
		public virtual void resetTexture()
		{
			this.texture = new Lazy<Texture2D>(delegate()
			{
				if (this.paintedTexture != null)
				{
					this.paintedTexture.Dispose();
					this.paintedTexture = null;
				}
				string name = this.textureName();
				Texture2D val;
				try
				{
					val = Game1.content.Load<Texture2D>(name);
				}
				catch
				{
					val = Game1.content.Load<Texture2D>("Buildings\\Error");
					return val;
				}
				this.paintedTexture = BuildingPainter.Apply(val, name + "_PaintMask", this.netBuildingPaintColor.Value);
				if (this.paintedTexture != null)
				{
					val = this.paintedTexture;
				}
				return val;
			});
		}

		// Token: 0x06003722 RID: 14114 RVA: 0x002B83BF File Offset: 0x002B65BF
		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == this.humanDoor.X && y == this.humanDoor.Y)
			{
				return 2;
			}
			if (x == this.animalDoor.X && y == this.animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		// Token: 0x06003723 RID: 14115 RVA: 0x002B83FE File Offset: 0x002B65FE
		public virtual void performTenMinuteAction(int timeElapsed)
		{
		}

		// Token: 0x06003724 RID: 14116 RVA: 0x002B8400 File Offset: 0x002B6600
		public virtual void resetLocalState()
		{
			this.alpha = 1f;
			this.color = Color.White;
			this.isMoving = false;
		}

		// Token: 0x06003725 RID: 14117 RVA: 0x002B8420 File Offset: 0x002B6620
		public virtual bool CanLeftClick(int x, int y)
		{
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1);
			return this.intersects(r);
		}

		// Token: 0x06003726 RID: 14118 RVA: 0x002B843F File Offset: 0x002B663F
		public virtual bool leftClicked()
		{
			return false;
		}

		// Token: 0x06003727 RID: 14119 RVA: 0x002B8444 File Offset: 0x002B6644
		public virtual void ToggleAnimalDoor(Farmer who)
		{
			BuildingData data = this.GetData();
			string sound = this.animalDoorOpen.Value ? ((data != null) ? data.AnimalDoorOpenSound : null) : ((data != null) ? data.AnimalDoorCloseSound : null);
			if (sound != null)
			{
				who.currentLocation.playSound(sound, null, null, SoundContext.Default);
			}
			this.animalDoorOpen.Value = !this.animalDoorOpen.Value;
		}

		// Token: 0x06003728 RID: 14120 RVA: 0x002B84BB File Offset: 0x002B66BB
		public virtual bool OnUseHumanDoor(Farmer who)
		{
			return true;
		}

		// Token: 0x06003729 RID: 14121 RVA: 0x002B84C0 File Offset: 0x002B66C0
		public virtual bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (who.isRidingHorse())
			{
				return false;
			}
			if (who.IsLocalPlayer && this.occupiesTile(tileLocation, false) && this.daysOfConstructionLeft.Value > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
			}
			else
			{
				if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
				{
					return false;
				}
				GameLocation interior = this.GetIndoors();
				if (who.IsLocalPlayer && tileLocation.X == (float)(this.humanDoor.X + this.tileX.Value) && tileLocation.Y == (float)(this.humanDoor.Y + this.tileY.Value) && interior != null)
				{
					if (who.mount != null)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"), true);
						return false;
					}
					if (who.team.demolishLock.IsLocked())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"), true);
						return false;
					}
					if (this.OnUseHumanDoor(who))
					{
						who.currentLocation.playSound("doorClose", new Vector2?(tileLocation), null, SoundContext.Default);
						bool isStructure = this.indoors.Value != null;
						Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
					}
					return true;
				}
				else
				{
					BuildingData data = this.GetData();
					if (data != null)
					{
						Microsoft.Xna.Framework.Rectangle door = this.getRectForAnimalDoor(data);
						door.Width /= 64;
						door.Height /= 64;
						door.X /= 64;
						door.Y /= 64;
						if (this.daysOfConstructionLeft.Value <= 0 && door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(true))
						{
							this.ToggleAnimalDoor(who);
							return true;
						}
						if (who.IsLocalPlayer && this.occupiesTile(tileLocation, true) && !this.isTilePassable(tileLocation))
						{
							string tileAction = data.GetActionAtTile((int)tileLocation.X - this.tileX.Value, (int)tileLocation.Y - this.tileY.Value);
							if (tileAction != null)
							{
								tileAction = TokenParser.ParseText(tileAction, null, null, null);
								if (who.currentLocation.performAction(tileAction, who, new Location((int)tileLocation.X, (int)tileLocation.Y)))
								{
									return true;
								}
							}
						}
					}
					else if (who.IsLocalPlayer)
					{
						if (!this.isTilePassable(tileLocation) && Building.TryPerformObeliskWarp(this.buildingType.Value, who))
						{
							return true;
						}
						if (who.ActiveObject != null && !this.isTilePassable(tileLocation))
						{
							return this.performActiveObjectDropInAction(who, false);
						}
					}
				}
			}
			return false;
		}

		// Token: 0x0600372A RID: 14122 RVA: 0x002B87AC File Offset: 0x002B69AC
		public static bool TryPerformObeliskWarp(string buildingType, Farmer who)
		{
			if (buildingType == "Desert Obelisk")
			{
				Building.PerformObeliskWarp("Desert", 35, 43, true, who);
				return true;
			}
			if (buildingType == "Water Obelisk")
			{
				Building.PerformObeliskWarp("Beach", 20, 4, false, who);
				return true;
			}
			if (buildingType == "Earth Obelisk")
			{
				Building.PerformObeliskWarp("Mountain", 31, 20, false, who);
				return true;
			}
			if (!(buildingType == "Island Obelisk"))
			{
				return false;
			}
			Building.PerformObeliskWarp("IslandSouth", 11, 11, false, who);
			return true;
		}

		// Token: 0x0600372B RID: 14123 RVA: 0x002B8838 File Offset: 0x002B6A38
		public static void PerformObeliskWarp(string destination, int warp_x, int warp_y, bool force_dismount, Farmer who)
		{
			if (force_dismount && who.isRidingHorse() && who.mount != null)
			{
				who.mount.checkAction(who, who.currentLocation);
				return;
			}
			for (int i = 0; i < 12; i++)
			{
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), (float)Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), false, Game1.random.NextBool()));
			}
			who.currentLocation.playSound("wand", null, null, SoundContext.Default);
			Game1.displayFarmer = false;
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = -2000;
			Game1.player.freezePause = 1000;
			Game1.flashAlpha = 1f;
			Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
			DelayedAction.fadeAfterDelay(delegate
			{
				Building.obeliskWarpForReal(destination, warp_x, warp_y, who);
			}, 1000);
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
			r.Inflate(192, 192);
			int j = 0;
			Point playerTile = who.TilePoint;
			for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
			{
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)x, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = j * 25,
					motion = new Vector2(-0.25f, 0f)
				});
				j++;
			}
		}

		// Token: 0x0600372C RID: 14124 RVA: 0x002B8AC8 File Offset: 0x002B6CC8
		private static void obeliskWarpForReal(string destination, int warp_x, int warp_y, Farmer who)
		{
			Game1.warpFarmer(destination, warp_x, warp_y, false);
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			Game1.player.temporarilyInvincible = false;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}

		// Token: 0x0600372D RID: 14125 RVA: 0x002B8B00 File Offset: 0x002B6D00
		public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			BuildingData data = this.GetData();
			if (data != null)
			{
				Vector2 tileLocation = new Vector2((float)xTile, (float)yTile);
				if (this.occupiesTile(tileLocation, true) && !this.isTilePassable(tileLocation) && data.GetActionAtTile(xTile - this.tileX.Value, yTile - this.tileY.Value) != null)
				{
					return true;
				}
			}
			if (this.humanDoor.X >= 0 && xTile == this.tileX.Value + this.humanDoor.X && yTile == this.tileY.Value + this.humanDoor.Y)
			{
				return true;
			}
			Microsoft.Xna.Framework.Rectangle door = this.getRectForAnimalDoor(data);
			door.Width /= 64;
			door.Height /= 64;
			door.X /= 64;
			door.Y /= 64;
			return door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(new Point(xTile, yTile));
		}

		// Token: 0x0600372E RID: 14126 RVA: 0x002B8BF8 File Offset: 0x002B6DF8
		public virtual void performActionOnBuildingPlacement()
		{
			GameLocation location = this.GetParentLocation();
			if (location != null)
			{
				for (int y = 0; y < this.tilesHigh.Value; y++)
				{
					int x = 0;
					while (x < this.tilesWide.Value)
					{
						Vector2 currentGlobalTilePosition = new Vector2((float)(this.tileX.Value + x), (float)(this.tileY.Value + y));
						if (!(location.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition, null) is Flooring))
						{
							goto IL_83;
						}
						BuildingData data = this.GetData();
						if (!(((data != null) ? new bool?(data.AllowsFlooringUnderneath) : null) ?? false))
						{
							goto IL_83;
						}
						IL_90:
						x++;
						continue;
						IL_83:
						location.terrainFeatures.Remove(currentGlobalTilePosition);
						goto IL_90;
					}
				}
				foreach (BuildingPlacementTile buildingPlacementTile in this.GetAdditionalPlacementTiles())
				{
					bool onlyNeedsToBePassable = buildingPlacementTile.OnlyNeedsToBePassable;
					foreach (Point areaTile in buildingPlacementTile.TileArea.GetPoints())
					{
						Vector2 currentGlobalTilePosition2 = new Vector2((float)(this.tileX.Value + areaTile.X), (float)(this.tileY.Value + areaTile.Y));
						TerrainFeature feature;
						if (!onlyNeedsToBePassable || (location.terrainFeatures.TryGetValue(currentGlobalTilePosition2, out feature) && !feature.isPassable(null)))
						{
							if (location.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition2, null) is Flooring)
							{
								BuildingData data2 = this.GetData();
								if (((data2 != null) ? new bool?(data2.AllowsFlooringUnderneath) : null) ?? false)
								{
									continue;
								}
							}
							location.terrainFeatures.Remove(currentGlobalTilePosition2);
						}
					}
				}
			}
		}

		// Token: 0x0600372F RID: 14127 RVA: 0x002B8E0C File Offset: 0x002B700C
		public virtual void performActionOnConstruction(GameLocation location, Farmer who)
		{
			BuildingData data = this.GetData();
			this.LoadFromBuildingData(data, false, true);
			Vector2 buildingCenter = new Vector2((float)this.tileX.Value + (float)this.tilesWide.Value * 0.5f, (float)this.tileY.Value + (float)this.tilesHigh.Value * 0.5f);
			location.localSound("axchop", new Vector2?(buildingCenter), null, SoundContext.Default);
			this.newConstructionTimer.Value = ((this.magical.Value || this.daysOfConstructionLeft.Value <= 0) ? 2000 : 1000);
			if (((data != null) ? data.AddMailOnBuild : null) != null)
			{
				foreach (string mailName in data.AddMailOnBuild)
				{
					Game1.addMail(mailName, false, true);
				}
			}
			if (!this.magical.Value)
			{
				location.localSound("axchop", new Vector2?(buildingCenter), null, SoundContext.Default);
				for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
				{
					for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
					{
						for (int i = 0; i < 5; i++)
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(46, 12), new Vector2((float)x, (float)y) * 64f + new Vector2((float)Game1.random.Next(-16, 32), (float)Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool(), 100f, 0, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
								motion = new Vector2(0f, -1f),
								interval = (float)Game1.random.Next(50, 80)
							});
						}
						location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2((float)x, (float)y) * 64f + new Vector2((float)Game1.random.Next(-16, 32), (float)Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool(), 100f, 0, -1, -1f, -1, 0));
					}
				}
				for (int j = 0; j < 8; j++)
				{
					DelayedAction.playSoundAfterDelay("dirtyHit", 250 + j * 150, location, new Vector2?(buildingCenter), -1, true);
				}
			}
			else
			{
				for (int k = 0; k < 8; k++)
				{
					DelayedAction.playSoundAfterDelay("dirtyHit", 100 + k * 210, location, new Vector2?(buildingCenter), -1, true);
				}
				if (Game1.player == who)
				{
					Game1.flashAlpha = 2f;
				}
				location.localSound("wand", new Vector2?(buildingCenter), null, SoundContext.Default);
				Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
				Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu().GetValueOrDefault(mainSourceRect);
				int y2 = 0;
				int bottomEdge = mainSourceRect.Height / 16 * 2;
				while (y2 <= bottomEdge)
				{
					int x2 = 0;
					int rightEdge = sourceRectForMenu.Width / 16 * 2;
					while (x2 < rightEdge)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((float)this.tileX.Value, (float)this.tileY.Value) * 64f + new Vector2((float)(x2 * 64 / 2), (float)(y2 * 64 / 2 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64)) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), false, false)
						{
							layerDepth = (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + (float)x2 / 10000f,
							pingPong = true,
							delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - y2) * 100,
							scale = 4f,
							alphaFade = 0.01f,
							color = Color.AliceBlue
						});
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((float)this.tileX.Value, (float)this.tileY.Value) * 64f + new Vector2((float)(x2 * 64 / 2), (float)(y2 * 64 / 2 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64)) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), false, false)
						{
							layerDepth = (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + (float)x2 / 10000f + 0.0001f,
							pingPong = true,
							delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - y2) * 100,
							scale = 4f,
							alphaFade = 0.01f,
							color = Color.AliceBlue
						});
						x2++;
					}
					y2++;
				}
			}
			Cabin cabin = this.GetIndoors() as Cabin;
			if (cabin != null && !cabin.HasOwner)
			{
				cabin.CreateFarmhand();
				if (Game1.IsMasterGame)
				{
					this.hasLoaded = true;
				}
			}
		}

		// Token: 0x06003730 RID: 14128 RVA: 0x002B945C File Offset: 0x002B765C
		public virtual void performActionOnDemolition(GameLocation location)
		{
			Cabin cabin = this.GetIndoors() as Cabin;
			if (cabin != null)
			{
				cabin.DeleteFarmhand();
			}
			if (this.indoors.Value != null)
			{
				Game1.multiplayer.broadcastRemoveLocationFromLookup(this.indoors.Value);
				this.indoors.Value.OnRemoved();
				this.indoors.Value = null;
			}
		}

		// Token: 0x06003731 RID: 14129 RVA: 0x002B94BC File Offset: 0x002B76BC
		public virtual bool ForEachItemExcludingInterior(Func<Item, bool> action)
		{
			Building.<>c__DisplayClass95_0 CS$<>8__locals1 = new Building.<>c__DisplayClass95_0();
			CS$<>8__locals1.action = action;
			CS$<>8__locals1.<>4__this = this;
			return this.ForEachItemContextExcludingInterior(new ForEachItemDelegate(CS$<>8__locals1.<ForEachItemExcludingInterior>g__Handle|0), new GetForEachItemPathDelegate(CS$<>8__locals1.<ForEachItemExcludingInterior>g__GetParentPath|1));
		}

		// Token: 0x06003732 RID: 14130 RVA: 0x002B94FC File Offset: 0x002B76FC
		public virtual bool ForEachItemContextExcludingInterior(ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath)
		{
			Building.<>c__DisplayClass96_0 CS$<>8__locals1 = new Building.<>c__DisplayClass96_0();
			CS$<>8__locals1.getParentPath = getParentPath;
			CS$<>8__locals1.<>4__this = this;
			using (NetList<Chest, NetRef<Chest>>.Enumerator enumerator = this.buildingChests.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Building.<>c__DisplayClass96_1 CS$<>8__locals2 = new Building.<>c__DisplayClass96_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.chest = enumerator.Current;
					if (!CS$<>8__locals2.chest.ForEachItem(handler, new GetForEachItemPathDelegate(CS$<>8__locals2.<ForEachItemContextExcludingInterior>g__GetPath|0)))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06003733 RID: 14131 RVA: 0x002B9590 File Offset: 0x002B7790
		public virtual void BeforeDemolish()
		{
			List<Item> quest_items = new List<Item>();
			this.ForEachItemExcludingInterior(delegate(Item item)
			{
				base.<BeforeDemolish>g__CollectQuestItem|0(item);
				return true;
			});
			if (this.indoors.Value != null)
			{
				Utility.ForEachItemIn(this.indoors.Value, delegate(Item item)
				{
					base.<BeforeDemolish>g__CollectQuestItem|0(item);
					return true;
				});
				Cabin cabin = this.indoors.Value as Cabin;
				if (cabin != null)
				{
					Cellar cellar = cabin.GetCellar();
					if (cellar != null)
					{
						Utility.ForEachItemIn(cellar, delegate(Item item)
						{
							base.<BeforeDemolish>g__CollectQuestItem|0(item);
							return true;
						});
					}
				}
			}
			if (quest_items.Count > 0)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
				for (int i = 0; i < quest_items.Count; i++)
				{
					Game1.player.team.returnedDonations.Add(quest_items[i]);
				}
			}
		}

		// Token: 0x06003734 RID: 14132 RVA: 0x002B9674 File Offset: 0x002B7874
		public virtual void performActionOnUpgrade(GameLocation location)
		{
			Farm farm = location as Farm;
			if (farm != null)
			{
				farm.UnsetFarmhouseValues();
			}
		}

		// Token: 0x06003735 RID: 14133 RVA: 0x002B9691 File Offset: 0x002B7891
		public virtual string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_location)
		{
			return null;
		}

		// Token: 0x06003736 RID: 14134 RVA: 0x002B9694 File Offset: 0x002B7894
		public virtual bool performActiveObjectDropInAction(Farmer who, bool probe)
		{
			return false;
		}

		// Token: 0x06003737 RID: 14135 RVA: 0x002B9697 File Offset: 0x002B7897
		public virtual void performToolAction(Tool t, int tileX, int tileY)
		{
		}

		// Token: 0x06003738 RID: 14136 RVA: 0x002B969C File Offset: 0x002B789C
		public virtual void updateWhenFarmNotCurrentLocation(GameTime time)
		{
			if (this.indoors.Value != null && Game1.currentLocation != this.indoors.Value)
			{
				this.indoors.Value.netAudio.Update();
			}
			BuildingPaintColor value = this.netBuildingPaintColor.Value;
			if (value != null)
			{
				value.Poll(new Action(this.resetTexture));
			}
			if (this.newConstructionTimer.Value > 0)
			{
				this.newConstructionTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if (this.newConstructionTimer.Value <= 0 && this.magical.Value)
				{
					this.daysOfConstructionLeft.Value = 0;
				}
			}
			if (Game1.IsMasterGame)
			{
				BuildingData data = this.GetData();
				if (data != null)
				{
					if (this.animalDoorOpen.Value)
					{
						if (this.animalDoorOpenAmount.Value < 1f)
						{
							this.animalDoorOpenAmount.Value = ((data.AnimalDoorOpenDuration > 0f) ? Utility.MoveTowards(this.animalDoorOpenAmount.Value, 1f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorOpenDuration) : 1f);
							return;
						}
					}
					else if (this.animalDoorOpenAmount.Value > 0f)
					{
						this.animalDoorOpenAmount.Value = ((data.AnimalDoorCloseDuration > 0f) ? Utility.MoveTowards(this.animalDoorOpenAmount.Value, 0f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorCloseDuration) : 0f);
					}
				}
			}
		}

		// Token: 0x06003739 RID: 14137 RVA: 0x002B9838 File Offset: 0x002B7A38
		public virtual void Update(GameTime time)
		{
			if (!this.hasLoaded && Game1.IsMasterGame && Game1.hasLoadedGame)
			{
				this.ReloadBuildingData(false, true);
				this.load();
			}
			this.UpdateTransparency();
			if (!this.isUnderConstruction(true))
			{
				if (this.hasChimney == null)
				{
					string chimneyString = this.GetMetadata("ChimneyPosition");
					if (chimneyString != null)
					{
						this.hasChimney = new bool?(true);
						string[] split = ArgUtility.SplitBySpace(chimneyString);
						this.chimneyPosition.X = (float)int.Parse(split[0]);
						this.chimneyPosition.Y = (float)int.Parse(split[1]);
					}
					else
					{
						this.hasChimney = new bool?(false);
					}
				}
				GameLocation interior = this.GetIndoors();
				FarmHouse farmhouse = interior as FarmHouse;
				if (farmhouse != null)
				{
					int upgradeLevel = farmhouse.upgradeLevel;
					if (this.lastHouseUpgradeLevel != upgradeLevel)
					{
						this.lastHouseUpgradeLevel = upgradeLevel;
						string chimneyString2 = null;
						for (int i = 1; i <= this.lastHouseUpgradeLevel; i++)
						{
							string currentChimneyString = this.GetMetadata("ChimneyPosition" + (i + 1).ToString());
							if (currentChimneyString != null)
							{
								chimneyString2 = currentChimneyString;
							}
						}
						if (chimneyString2 != null)
						{
							this.hasChimney = new bool?(true);
							string[] split2 = ArgUtility.SplitBySpace(chimneyString2);
							this.chimneyPosition.X = (float)int.Parse(split2[0]);
							this.chimneyPosition.Y = (float)int.Parse(split2[1]);
						}
					}
				}
				if (this.hasChimney.GetValueOrDefault() && interior != null)
				{
					this.chimneyTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.chimneyTimer <= 0)
					{
						if (interior.hasActiveFireplace())
						{
							GameLocation parentLocation = this.GetParentLocation();
							Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
							Vector2 cornerPosition = new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64 - mainSourceRect.Height * 4));
							BuildingData data = this.GetData();
							Vector2 cornerOffset = (data != null) ? (data.DrawOffset * 4f) : Vector2.Zero;
							TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(cornerPosition.X + cornerOffset.X, cornerPosition.Y + cornerOffset.Y) + this.chimneyPosition * 4f + new Vector2(-8f, -12f), false, 0.002f, Color.Gray);
							sprite.alpha = 0.75f;
							sprite.motion = new Vector2(0f, -0.5f);
							sprite.acceleration = new Vector2(0.002f, 0f);
							sprite.interval = 99999f;
							sprite.layerDepth = 1f;
							sprite.scale = 2f;
							sprite.scaleChange = 0.02f;
							sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
							parentLocation.temporarySprites.Add(sprite);
						}
						this.chimneyTimer = 500;
					}
				}
			}
		}

		// Token: 0x0600373A RID: 14138 RVA: 0x002B9B68 File Offset: 0x002B7D68
		public virtual void UpdateTransparency()
		{
			if (this.fadeWhenPlayerIsBehind.Value)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = this.getSourceRectForMenu() ?? this.getSourceRect();
				Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle(this.tileX.Value * 64, (this.tileY.Value + (-(sourceRect.Height / 16) + this.tilesHigh.Value)) * 64, this.tilesWide.Value * 64, (sourceRect.Height / 16 - this.tilesHigh.Value) * 64 + 32);
				if (Game1.player.GetBoundingBox().Intersects(boundingBox))
				{
					if (this.alpha > 0.4f)
					{
						this.alpha = Math.Max(0.4f, this.alpha - 0.04f);
					}
					return;
				}
			}
			if (this.alpha < 1f)
			{
				this.alpha = Math.Min(1f, this.alpha + 0.05f);
			}
		}

		// Token: 0x0600373B RID: 14139 RVA: 0x002B9C70 File Offset: 0x002B7E70
		public virtual void showUpgradeAnimation(GameLocation location)
		{
			this.color = Color.White;
			location.temporarySprites.Add(new TemporaryAnimatedSprite(46, this.getUpgradeSignLocation() + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f, 0, -1, -1f, -1, 0)
			{
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(-0.02f, 0.01f),
				delayBeforeAnimationStart = Game1.random.Next(100),
				layerDepth = 0.89f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite(46, this.getUpgradeSignLocation() + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f, 0, -1, -1f, -1, 0)
			{
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(-0.02f, 0.01f),
				delayBeforeAnimationStart = Game1.random.Next(40),
				layerDepth = 0.89f
			});
		}

		// Token: 0x0600373C RID: 14140 RVA: 0x002B9DD4 File Offset: 0x002B7FD4
		public virtual Vector2 getUpgradeSignLocation()
		{
			BuildingData data = this.GetData();
			Vector2 signOffset = (data != null) ? data.UpgradeSignTile : new Vector2(0.5f, 0f);
			float signHeight = (data != null) ? data.UpgradeSignHeight : 8f;
			return new Vector2(((float)this.tileX.Value + signOffset.X) * 64f, ((float)this.tileY.Value + signOffset.Y) * 64f - signHeight * 4f);
		}

		// Token: 0x0600373D RID: 14141 RVA: 0x002B9E54 File Offset: 0x002B8054
		public virtual void showDestroyedAnimation(GameLocation location)
		{
			for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
			{
				for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2((float)(x * 64), (float)(y * 64)) + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), false, Game1.random.NextBool())
					{
						delayBeforeAnimationStart = Game1.random.Next(300)
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2((float)(x * 64), (float)(y * 64)) + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), false, Game1.random.NextBool())
					{
						delayBeforeAnimationStart = 250 + Game1.random.Next(300)
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2((float)x, (float)y) * 64f + new Vector2(32f, -32f) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 16)), false, 0f, Color.White)
					{
						interval = 30f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						scale = 4f,
						alphaFade = 0.01f
					});
				}
			}
		}

		// Token: 0x0600373E RID: 14142 RVA: 0x002BA07C File Offset: 0x002B827C
		public void FinishConstruction(bool onGameStart = false)
		{
			bool changed = false;
			if (this.daysOfConstructionLeft.Value > 0)
			{
				Game1.player.team.constructedBuildings.Add(this.buildingType.Value);
				if (this.buildingType.Value == "Slime Hutch")
				{
					Game1.player.mailReceived.Add("slimeHutchBuilt");
				}
				this.daysOfConstructionLeft.Value = 0;
				changed = true;
			}
			if (this.daysUntilUpgrade.Value > 0)
			{
				string nextUpgrade = this.upgradeName.Value ?? "Well";
				Game1.player.team.constructedBuildings.Add(nextUpgrade);
				this.buildingType.Value = nextUpgrade;
				this.ReloadBuildingData(true, false);
				this.daysUntilUpgrade.Value = 0;
				this.OnUpgraded();
				changed = true;
			}
			if (changed)
			{
				Game1.netWorldState.Value.UpdateUnderConstruction();
				this.resetTexture();
			}
			if (!onGameStart)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					farmer.autoGenerateActiveDialogueEvent("structureBuilt_" + this.buildingType.Value, 4);
				}
			}
		}

		// Token: 0x0600373F RID: 14143 RVA: 0x002BA1C4 File Offset: 0x002B83C4
		public virtual void dayUpdate(int dayOfMonth)
		{
			int num;
			if (this.daysOfConstructionLeft.Value <= 0 || Utility.isFestivalDay(dayOfMonth, Game1.season) || (Game1.isGreenRain && Game1.year <= 1))
			{
				if (this.daysUntilUpgrade.Value > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.season) && (!Game1.isGreenRain || Game1.year > 1))
				{
					if (this.daysUntilUpgrade.Value == 1)
					{
						this.FinishConstruction(false);
					}
					else
					{
						NetInt netInt = this.daysUntilUpgrade;
						num = netInt.Value;
						netInt.Value = num - 1;
					}
				}
				GameLocation interior = this.GetIndoors();
				AnimalHouse animalHouse = interior as AnimalHouse;
				if (animalHouse != null)
				{
					this.currentOccupants.Value = animalHouse.animals.Length;
				}
				if (this.GetIndoorsType() == IndoorsType.Instanced && interior != null)
				{
					interior.DayUpdate(dayOfMonth);
				}
				BuildingData data = this.GetData();
				if (data != null)
				{
					List<BuildingItemConversion> itemConversions = data.ItemConversions;
					int? num2 = (itemConversions != null) ? new int?(itemConversions.Count) : null;
					num = 0;
					if (num2.GetValueOrDefault() > num & num2 != null)
					{
						ItemQueryContext itemQueryContext = new ItemQueryContext(this.GetParentLocation(), null, null, "building '" + this.buildingType.Value + "' > item conversion rules");
						foreach (BuildingItemConversion conversion in data.ItemConversions)
						{
							this.CheckItemConversionRule(conversion, itemQueryContext);
						}
					}
				}
				return;
			}
			if (this.daysOfConstructionLeft.Value == 1)
			{
				this.FinishConstruction(false);
				return;
			}
			NetInt netInt2 = this.daysOfConstructionLeft;
			num = netInt2.Value;
			netInt2.Value = num - 1;
		}

		// Token: 0x06003740 RID: 14144 RVA: 0x002BA370 File Offset: 0x002B8570
		public virtual void CheckItemConversionRule(BuildingItemConversion conversion, ItemQueryContext itemQueryContext)
		{
			int convertAmount = 0;
			int currentCount = 0;
			Chest sourceChest = this.GetBuildingChest(conversion.SourceChest);
			Chest destinationChest = this.GetBuildingChest(conversion.DestinationChest);
			if (sourceChest == null)
			{
				return;
			}
			foreach (Item item in sourceChest.Items)
			{
				if (item != null)
				{
					bool fail = false;
					foreach (string requiredTag in conversion.RequiredTags)
					{
						if (!item.HasContextTag(requiredTag))
						{
							fail = true;
							break;
						}
					}
					if (!fail)
					{
						currentCount += item.Stack;
						if (currentCount >= conversion.RequiredCount)
						{
							int conversions = currentCount / conversion.RequiredCount;
							if (conversion.MaxDailyConversions >= 0)
							{
								conversions = Math.Min(conversions, conversion.MaxDailyConversions - convertAmount);
							}
							convertAmount += conversions;
							currentCount -= conversions * conversion.RequiredCount;
						}
						if (conversion.MaxDailyConversions >= 0 && convertAmount >= conversion.MaxDailyConversions)
						{
							break;
						}
					}
				}
			}
			if (convertAmount == 0)
			{
				return;
			}
			int totalConversions = 0;
			for (int i = 0; i < convertAmount; i++)
			{
				bool conversionCreatedItem = false;
				for (int j = 0; j < conversion.ProducedItems.Count; j++)
				{
					GenericSpawnItemDataWithCondition producedItem = conversion.ProducedItems[j];
					if (GameStateQuery.CheckConditions(producedItem.Condition, this.GetParentLocation(), null, null, null, null, null))
					{
						Item item2 = ItemQueryResolver.TryResolveRandomItem(producedItem, itemQueryContext, false, null, null, null, null);
						int producedCount = item2.Stack;
						Item item4 = destinationChest.addItem(item2);
						if (item4 == null || item4.Stack != producedCount)
						{
							conversionCreatedItem = true;
						}
					}
				}
				if (conversionCreatedItem)
				{
					totalConversions++;
				}
			}
			if (totalConversions > 0)
			{
				int requiredAmount = totalConversions * conversion.RequiredCount;
				for (int k = 0; k < sourceChest.Items.Count; k++)
				{
					Item item3 = sourceChest.Items[k];
					if (item3 != null)
					{
						bool fail2 = false;
						foreach (string requiredTag2 in conversion.RequiredTags)
						{
							if (!item3.HasContextTag(requiredTag2))
							{
								fail2 = true;
								break;
							}
						}
						if (!fail2)
						{
							int consumedAmount = Math.Min(requiredAmount, item3.Stack);
							sourceChest.Items[k] = item3.ConsumeStack(consumedAmount);
							requiredAmount -= consumedAmount;
							if (requiredAmount <= 0)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06003741 RID: 14145 RVA: 0x002BA60C File Offset: 0x002B880C
		public virtual void OnUpgraded()
		{
			GameLocation gameLocation = this.GetIndoors();
			if (gameLocation != null)
			{
				gameLocation.OnParentBuildingUpgraded(this);
			}
			BuildingData data = this.GetData();
			if (((data != null) ? data.AddMailOnBuild : null) != null)
			{
				foreach (string mailName in data.AddMailOnBuild)
				{
					Game1.addMail(mailName, false, true);
				}
			}
		}

		// Token: 0x06003742 RID: 14146 RVA: 0x002BA688 File Offset: 0x002B8888
		public virtual Microsoft.Xna.Framework.Rectangle getSourceRect()
		{
			BuildingData data = this.GetData();
			if (data != null)
			{
				Microsoft.Xna.Framework.Rectangle rect = data.SourceRect;
				if (rect == Microsoft.Xna.Framework.Rectangle.Empty)
				{
					return this.texture.Value.Bounds;
				}
				GameLocation interior = this.GetIndoors();
				FarmHouse farmhouse = interior as FarmHouse;
				if (farmhouse != null)
				{
					if (interior is Cabin)
					{
						rect.X += rect.Width * Math.Min(farmhouse.upgradeLevel, 2);
					}
					else
					{
						rect.Y += rect.Height * Math.Min(farmhouse.upgradeLevel, 2);
					}
				}
				rect = this.ApplySourceRectOffsets(rect);
				if (this.buildingType.Value == "Greenhouse")
				{
					Farm farm = this.GetParentLocation() as Farm;
					if (farm != null && !farm.greenhouseUnlocked.Value)
					{
						rect.Y -= rect.Height;
					}
				}
				return rect;
			}
			else
			{
				if (this.isCabin)
				{
					Cabin cabin = this.GetIndoors() as Cabin;
					return new Microsoft.Xna.Framework.Rectangle(((cabin != null) ? Math.Min(cabin.upgradeLevel, 2) : 0) * 80, 0, 80, 112);
				}
				return this.texture.Value.Bounds;
			}
		}

		// Token: 0x06003743 RID: 14147 RVA: 0x002BA7B8 File Offset: 0x002B89B8
		public virtual Microsoft.Xna.Framework.Rectangle ApplySourceRectOffsets(Microsoft.Xna.Framework.Rectangle source)
		{
			BuildingData data = this.GetData();
			if (data != null && data.SeasonOffset != Point.Zero)
			{
				int seasonOffset = Game1.GetSeasonIndexForLocation(this.GetParentLocation());
				source.X += data.SeasonOffset.X * seasonOffset;
				source.Y += data.SeasonOffset.Y * seasonOffset;
			}
			return source;
		}

		// Token: 0x06003744 RID: 14148 RVA: 0x002BA820 File Offset: 0x002B8A20
		public virtual Microsoft.Xna.Framework.Rectangle? getSourceRectForMenu()
		{
			return null;
		}

		// Token: 0x06003745 RID: 14149 RVA: 0x002BA838 File Offset: 0x002B8A38
		public virtual void updateInteriorWarps(GameLocation interior = null)
		{
			interior = (interior ?? this.GetIndoors());
			if (interior == null)
			{
				return;
			}
			GameLocation parentLocation = this.GetParentLocation();
			foreach (Warp warp in interior.warps)
			{
				if (warp.TargetName == "Farm" || (parentLocation != null && warp.TargetName == parentLocation.NameOrUniqueName))
				{
					warp.TargetName = (((parentLocation != null) ? parentLocation.NameOrUniqueName : null) ?? warp.TargetName);
					warp.TargetX = this.humanDoor.X + this.tileX.Value;
					warp.TargetY = this.humanDoor.Y + this.tileY.Value + 1;
				}
			}
		}

		// Token: 0x06003746 RID: 14150 RVA: 0x002BA928 File Offset: 0x002B8B28
		public bool HasIndoors()
		{
			return this.indoors.Value != null || this.nonInstancedIndoorsName.Value != null;
		}

		// Token: 0x06003747 RID: 14151 RVA: 0x002BA947 File Offset: 0x002B8B47
		public bool HasIndoorsName(string name)
		{
			return name != null && this.GetIndoorsName().EqualsIgnoreCase(name);
		}

		// Token: 0x06003748 RID: 14152 RVA: 0x002BA95A File Offset: 0x002B8B5A
		public string GetIndoorsName()
		{
			GameLocation value = this.indoors.Value;
			return ((value != null) ? value.NameOrUniqueName : null) ?? this.nonInstancedIndoorsName.Value;
		}

		// Token: 0x06003749 RID: 14153 RVA: 0x002BA982 File Offset: 0x002B8B82
		public IndoorsType GetIndoorsType()
		{
			if (this.indoors.Value != null)
			{
				return IndoorsType.Instanced;
			}
			if (this.nonInstancedIndoorsName.Value != null)
			{
				return IndoorsType.Global;
			}
			return IndoorsType.None;
		}

		// Token: 0x0600374A RID: 14154 RVA: 0x002BA9A3 File Offset: 0x002B8BA3
		public GameLocation GetIndoors()
		{
			if (this.indoors.Value != null)
			{
				return this.indoors.Value;
			}
			if (this.nonInstancedIndoorsName.Value != null)
			{
				return Game1.getLocationFromName(this.nonInstancedIndoorsName.Value);
			}
			return null;
		}

		// Token: 0x0600374B RID: 14155 RVA: 0x002BA9E0 File Offset: 0x002B8BE0
		protected virtual GameLocation createIndoors(BuildingData data, string nameOfIndoorsWithoutUnique)
		{
			GameLocation localIndoors = null;
			if (data != null && !string.IsNullOrEmpty(data.IndoorMap))
			{
				Type locationType = typeof(GameLocation);
				if (data.IndoorMapType != null)
				{
					Exception exception = null;
					try
					{
						locationType = Type.GetType(data.IndoorMapType);
					}
					catch (Exception exception)
					{
					}
					if (locationType == null || exception != null)
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error constructing interior type '");
						defaultInterpolatedStringHandler.AppendFormatted(data.IndoorMapType);
						defaultInterpolatedStringHandler.AppendLiteral("' for building '");
						defaultInterpolatedStringHandler.AppendFormatted(this.buildingType.Value);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear() + ((exception != null) ? "." : ": that type doesn't exist."), null);
						locationType = typeof(GameLocation);
					}
				}
				string mapAssetName = "Maps\\" + data.IndoorMap;
				try
				{
					localIndoors = (GameLocation)Activator.CreateInstance(locationType, new object[]
					{
						mapAssetName,
						this.buildingType.Value
					});
				}
				catch (Exception)
				{
					try
					{
						localIndoors = (GameLocation)Activator.CreateInstance(locationType, new object[]
						{
							mapAssetName
						});
					}
					catch (Exception e)
					{
						IGameLogger log2 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error trying to instantiate indoors for '");
						defaultInterpolatedStringHandler.AppendFormatted<NetString>(this.buildingType);
						defaultInterpolatedStringHandler.AppendLiteral("'");
						log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), e);
						localIndoors = new GameLocation("Maps\\" + nameOfIndoorsWithoutUnique, this.buildingType.Value);
					}
				}
			}
			if (localIndoors != null)
			{
				localIndoors.uniqueName.Value = nameOfIndoorsWithoutUnique + GuidHelper.NewGuid().ToString();
				localIndoors.IsFarm = true;
				localIndoors.isStructure.Value = true;
				localIndoors.ParentBuilding = this;
				this.updateInteriorWarps(localIndoors);
			}
			return localIndoors;
		}

		// Token: 0x0600374C RID: 14156 RVA: 0x002BABD8 File Offset: 0x002B8DD8
		public virtual Point getPointForHumanDoor()
		{
			return new Point(this.tileX.Value + this.humanDoor.Value.X, this.tileY.Value + this.humanDoor.Value.Y);
		}

		// Token: 0x0600374D RID: 14157 RVA: 0x002BAC17 File Offset: 0x002B8E17
		public virtual Microsoft.Xna.Framework.Rectangle getRectForHumanDoor()
		{
			return new Microsoft.Xna.Framework.Rectangle(this.getPointForHumanDoor().X * 64, this.getPointForHumanDoor().Y * 64, 64, 64);
		}

		// Token: 0x0600374E RID: 14158 RVA: 0x002BAC3E File Offset: 0x002B8E3E
		public Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor()
		{
			return this.getRectForAnimalDoor(this.GetData());
		}

		// Token: 0x0600374F RID: 14159 RVA: 0x002BAC4C File Offset: 0x002B8E4C
		public virtual Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor(BuildingData data)
		{
			if (data != null)
			{
				Microsoft.Xna.Framework.Rectangle rect = data.AnimalDoor;
				return new Microsoft.Xna.Framework.Rectangle((rect.X + this.tileX.Value) * 64, (rect.Y + this.tileY.Value) * 64, rect.Width * 64, rect.Height * 64);
			}
			return new Microsoft.Xna.Framework.Rectangle((this.animalDoor.X + this.tileX.Value) * 64, (this.tileY.Value + this.animalDoor.Y) * 64, 64, 64);
		}

		// Token: 0x06003750 RID: 14160 RVA: 0x002BACE4 File Offset: 0x002B8EE4
		public virtual void load()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			BuildingData data = this.GetData();
			if (!this.hasLoaded)
			{
				this.hasLoaded = true;
				if (data != null)
				{
					if (data.NonInstancedIndoorLocation == null && this.nonInstancedIndoorsName.Value != null)
					{
						GameLocation interior = this.GetIndoors();
						if (interior != null)
						{
							interior.parentLocationName.Value = null;
						}
						this.nonInstancedIndoorsName.Value = null;
					}
					else if (data.NonInstancedIndoorLocation != null)
					{
						bool nonInstancedLocationAlreadyUsed = false;
						Utility.ForEachBuilding(delegate(Building building)
						{
							if (building.HasIndoorsName(data.NonInstancedIndoorLocation))
							{
								nonInstancedLocationAlreadyUsed = true;
								return false;
							}
							return true;
						}, true);
						if (!nonInstancedLocationAlreadyUsed)
						{
							this.nonInstancedIndoorsName.Value = Game1.RequireLocation(data.NonInstancedIndoorLocation, false).NameOrUniqueName;
						}
					}
				}
				this.LoadFromBuildingData(data, false, false);
			}
			if (this.nonInstancedIndoorsName.Value != null)
			{
				this.UpdateIndoorParent();
			}
			else
			{
				BuildingData data2 = data;
				string text;
				if ((text = ((data2 != null) ? data2.IndoorMap : null)) == null)
				{
					GameLocation value = this.indoors.Value;
					text = ((value != null) ? value.Name : null);
				}
				string nameOfIndoorsWithoutUnique = text;
				GameLocation indoorInstance = this.createIndoors(data, nameOfIndoorsWithoutUnique);
				if (indoorInstance != null && this.indoors.Value != null)
				{
					indoorInstance.characters.Set(this.indoors.Value.characters);
					indoorInstance.netObjects.MoveFrom(this.indoors.Value.netObjects);
					indoorInstance.terrainFeatures.MoveFrom(this.indoors.Value.terrainFeatures);
					indoorInstance.IsFarm = true;
					indoorInstance.IsOutdoors = false;
					indoorInstance.isStructure.Value = true;
					indoorInstance.miniJukeboxCount.Set(this.indoors.Value.miniJukeboxCount.Value);
					indoorInstance.miniJukeboxTrack.Set(this.indoors.Value.miniJukeboxTrack.Value);
					indoorInstance.uniqueName.Value = (this.indoors.Value.uniqueName.Value ?? (nameOfIndoorsWithoutUnique + (this.tileX.Value * 2000 + this.tileY.Value).ToString()));
					indoorInstance.numberOfSpawnedObjectsOnMap = this.indoors.Value.numberOfSpawnedObjectsOnMap;
					indoorInstance.animals.MoveFrom(this.indoors.Value.animals);
					AnimalHouse house = this.indoors.Value as AnimalHouse;
					if (house != null)
					{
						AnimalHouse houseInstance = indoorInstance as AnimalHouse;
						if (houseInstance != null)
						{
							houseInstance.animalsThatLiveHere.Set(house.animalsThatLiveHere);
						}
					}
					foreach (KeyValuePair<long, FarmAnimal> kvp in indoorInstance.animals.Pairs)
					{
						kvp.Value.reload(indoorInstance);
					}
					indoorInstance.furniture.Set(this.indoors.Value.furniture);
					foreach (Furniture furniture in indoorInstance.furniture)
					{
						furniture.updateDrawPosition();
					}
					Cabin cabin = this.indoors.Value as Cabin;
					if (cabin != null)
					{
						Cabin cabinInstance = indoorInstance as Cabin;
						if (cabinInstance != null)
						{
							cabinInstance.fridge.Value = cabin.fridge.Value;
							cabinInstance.farmhandReference.Value = cabin.farmhandReference.Value;
						}
					}
					indoorInstance.TransferDataFromSavedLocation(this.indoors.Value);
					this.indoors.Value = indoorInstance;
				}
				this.updateInteriorWarps(null);
				if (this.indoors.Value != null)
				{
					for (int i = this.indoors.Value.characters.Count - 1; i >= 0; i--)
					{
						SaveGame.initializeCharacter(this.indoors.Value.characters[i], this.indoors.Value);
					}
					foreach (TerrainFeature terrainFeature in this.indoors.Value.terrainFeatures.Values)
					{
						terrainFeature.loadSprite();
					}
					foreach (KeyValuePair<Vector2, Object> v in this.indoors.Value.objects.Pairs)
					{
						v.Value.initializeLightSource(v.Key, false);
						v.Value.reloadSprite();
					}
				}
			}
			if (data != null)
			{
				this.humanDoor.X = data.HumanDoor.X;
				this.humanDoor.Y = data.HumanDoor.Y;
			}
		}

		// Token: 0x06003751 RID: 14161 RVA: 0x002BB210 File Offset: 0x002B9410
		public IEnumerable<BuildingPlacementTile> GetAdditionalPlacementTiles()
		{
			BuildingData data = this.GetData();
			IEnumerable<BuildingPlacementTile> enumerable = (data != null) ? data.AdditionalPlacementTiles : null;
			return enumerable ?? LegacyShims.EmptyArray<BuildingPlacementTile>();
		}

		// Token: 0x06003752 RID: 14162 RVA: 0x002BB23A File Offset: 0x002B943A
		public bool isUnderConstruction(bool ignoreUpgrades = true)
		{
			return (!ignoreUpgrades && this.daysUntilUpgrade.Value > 0) || this.daysOfConstructionLeft.Value > 0;
		}

		// Token: 0x06003753 RID: 14163 RVA: 0x002BB25D File Offset: 0x002B945D
		public bool occupiesTile(Vector2 tile, bool applyTilePropertyRadius = false)
		{
			return this.occupiesTile((int)tile.X, (int)tile.Y, applyTilePropertyRadius);
		}

		// Token: 0x06003754 RID: 14164 RVA: 0x002BB274 File Offset: 0x002B9474
		public virtual bool occupiesTile(int x, int y, bool applyTilePropertyRadius = false)
		{
			int additionalRadius = applyTilePropertyRadius ? this.GetAdditionalTilePropertyRadius() : 0;
			int leftX = this.tileX.Value;
			int topY = this.tileY.Value;
			int width = this.tilesWide.Value;
			int height = this.tilesHigh.Value;
			return x >= leftX - additionalRadius && x < leftX + width + additionalRadius && y >= topY - additionalRadius && y < topY + height + additionalRadius;
		}

		// Token: 0x06003755 RID: 14165 RVA: 0x002BB2E0 File Offset: 0x002B94E0
		public virtual bool isTilePassable(Vector2 tile)
		{
			bool occupied = this.occupiesTile(tile, false);
			if (occupied && this.isUnderConstruction(true))
			{
				return false;
			}
			BuildingData data = this.GetData();
			if (data != null && this.occupiesTile(tile, true))
			{
				return data.IsTilePassable((int)tile.X - this.tileX.Value, (int)tile.Y - this.tileY.Value);
			}
			return !occupied;
		}

		// Token: 0x06003756 RID: 14166 RVA: 0x002BB349 File Offset: 0x002B9549
		public virtual bool isTileOccupiedForPlacement(Vector2 tile, Object to_place)
		{
			return !this.isTilePassable(tile);
		}

		// Token: 0x06003757 RID: 14167 RVA: 0x002BB358 File Offset: 0x002B9558
		public virtual Color? GetWaterColor(Vector2 tile)
		{
			return null;
		}

		// Token: 0x06003758 RID: 14168 RVA: 0x002BB36E File Offset: 0x002B956E
		public virtual bool isTileFishable(Vector2 tile)
		{
			return false;
		}

		// Token: 0x06003759 RID: 14169 RVA: 0x002BB371 File Offset: 0x002B9571
		public virtual bool CanRefillWateringCan()
		{
			return false;
		}

		// Token: 0x0600375A RID: 14170 RVA: 0x002BB374 File Offset: 0x002B9574
		public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			return new Microsoft.Xna.Framework.Rectangle(this.tileX.Value * 64, this.tileY.Value * 64, this.tilesWide.Value * 64, this.tilesHigh.Value * 64);
		}

		// Token: 0x0600375B RID: 14171 RVA: 0x002BB3B4 File Offset: 0x002B95B4
		public virtual bool intersects(Microsoft.Xna.Framework.Rectangle boundingBox)
		{
			Microsoft.Xna.Framework.Rectangle buildingRect = this.GetBoundingBox();
			int additionalRadius = this.GetAdditionalTilePropertyRadius();
			if (additionalRadius > 0)
			{
				buildingRect.Inflate(additionalRadius * 64, additionalRadius * 64);
			}
			if (buildingRect.Intersects(boundingBox))
			{
				int y = boundingBox.Top / 64;
				int maxY = boundingBox.Bottom / 64;
				while (y <= maxY)
				{
					int x = boundingBox.Left / 64;
					int maxX = boundingBox.Right / 64;
					while (x <= maxX)
					{
						if (!this.isTilePassable(new Vector2((float)x, (float)y)))
						{
							return true;
						}
						x++;
					}
					y++;
				}
			}
			return false;
		}

		// Token: 0x0600375C RID: 14172 RVA: 0x002BB448 File Offset: 0x002B9648
		public virtual void drawInMenu(SpriteBatch b, int x, int y)
		{
			BuildingData data = this.GetData();
			if (data != null)
			{
				x += (int)(data.DrawOffset.X * 4f);
				y += (int)(data.DrawOffset.Y * 4f);
			}
			float baseSortY = (float)(this.tilesHigh.Value * 64);
			float sortY = baseSortY;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY /= 10000f;
			if (this.ShouldDrawShadow(data))
			{
				this.drawShadow(b, x, y);
			}
			Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
			b.Draw(this.texture.Value, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(mainSourceRect), this.color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
			if (((data != null) ? data.DrawLayers : null) != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.OnlyDrawIfChestHasContents == null)
					{
						sortY = baseSortY - drawLayer.SortTileOffset * 64f;
						sortY += 1f;
						if (drawLayer.DrawInBackground)
						{
							sortY = 0f;
						}
						sortY /= 10000f;
						Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
						sourceRect = this.ApplySourceRectOffsets(sourceRect);
						Texture2D layerTexture = this.texture.Value;
						if (drawLayer.Texture != null)
						{
							layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
						}
						b.Draw(layerTexture, new Vector2((float)x, (float)y) + drawLayer.DrawPosition * 4f, new Microsoft.Xna.Framework.Rectangle?(sourceRect), Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
					}
				}
			}
		}

		// Token: 0x0600375D RID: 14173 RVA: 0x002BB644 File Offset: 0x002B9844
		public virtual void drawBackground(SpriteBatch b)
		{
			if (this.isMoving)
			{
				return;
			}
			if (this.daysOfConstructionLeft.Value <= 0 && this.newConstructionTimer.Value <= 0)
			{
				BuildingData data = this.GetData();
				if (((data != null) ? data.DrawLayers : null) != null)
				{
					Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
					Vector2 drawOrigin = new Vector2(0f, (float)mainSourceRect.Height);
					Vector2 drawPosition = new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64));
					foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
					{
						if (drawLayer.DrawInBackground)
						{
							if (drawLayer.OnlyDrawIfChestHasContents != null)
							{
								Chest chest = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
								if (chest == null || chest.isEmpty())
								{
									continue;
								}
							}
							Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
							sourceRect = this.ApplySourceRectOffsets(sourceRect);
							Vector2 drawOffset = Vector2.Zero;
							if (drawLayer.AnimalDoorOffset != Point.Zero)
							{
								drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
							}
							Texture2D layerTexture = this.texture.Value;
							if (drawLayer.Texture != null)
							{
								layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
							}
							b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), new Microsoft.Xna.Framework.Rectangle?(sourceRect), this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0f);
						}
					}
				}
			}
		}

		// Token: 0x0600375E RID: 14174 RVA: 0x002BB87C File Offset: 0x002B9A7C
		public virtual void draw(SpriteBatch b)
		{
			if (this.isMoving)
			{
				return;
			}
			if (this.daysOfConstructionLeft.Value > 0 || this.newConstructionTimer.Value > 0)
			{
				this.drawInConstruction(b);
				return;
			}
			BuildingData data = this.GetData();
			if (this.ShouldDrawShadow(data))
			{
				this.drawShadow(b, -1, -1);
			}
			float baseSortY = (float)((this.tileY.Value + this.tilesHigh.Value) * 64);
			float sortY = baseSortY;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64));
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
			Vector2 drawOrigin = new Vector2(0f, (float)mainSourceRect.Height);
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), new Microsoft.Xna.Framework.Rectangle?(mainSourceRect), this.color * this.alpha, 0f, drawOrigin, 4f, SpriteEffects.None, sortY);
			if (this.magical.Value && this.buildingType.Value.Equals("Gold Clock"))
			{
				if (Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
				{
					b.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 68), (float)(this.tileY.Value * 64 - 56))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(498, 368, 13, 9)), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 92), (float)(this.tileY.Value * 64 - 40))), new Microsoft.Xna.Framework.Rectangle?(Town.hourHandSource), Color.White * this.alpha, (float)(6.283185307179586 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes / 23f)), new Vector2(2.5f, 8f), 3f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 92), (float)(this.tileY.Value * 64 - 40))), new Microsoft.Xna.Framework.Rectangle?(Town.minuteHandSource), Color.White * this.alpha, (float)(6.283185307179586 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 1.02f)), new Vector2(2.5f, 12f), 3f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.00011f);
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + 92), (float)(this.tileY.Value * 64 - 40))), new Microsoft.Xna.Framework.Rectangle?(Town.clockNub), Color.White * this.alpha, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.00012f);
				}
			}
			if (data != null)
			{
				foreach (Chest chest in this.buildingChests)
				{
					BuildingChest chestData = Building.GetBuildingChestData(data, chest.Name);
					if (chestData.DisplayTile.X != -1f && chestData.DisplayTile.Y != -1f && chest.Items.Count > 0 && chest.Items[0] != null)
					{
						sortY = ((float)this.tileY.Value + chestData.DisplayTile.Y + 1f) * 64f;
						sortY += 1f;
						float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) - chestData.DisplayHeight * 64f;
						float drawX = ((float)this.tileX.Value + chestData.DisplayTile.X) * 64f;
						float drawY = ((float)this.tileY.Value + chestData.DisplayTile.Y - 1f) * 64f;
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY / 10000f);
						ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(chest.Items[0].QualifiedItemId);
						b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX + 32f + 4f, drawY + 32f + yOffset)), new Microsoft.Xna.Framework.Rectangle?(itemData.GetSourceRect(0, null)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (sortY + 1f) / 10000f);
					}
				}
				if (data.DrawLayers != null)
				{
					foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
					{
						if (!drawLayer.DrawInBackground)
						{
							if (drawLayer.OnlyDrawIfChestHasContents != null)
							{
								Chest chest2 = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
								if (chest2 == null || chest2.isEmpty())
								{
									continue;
								}
							}
							sortY = baseSortY - drawLayer.SortTileOffset * 64f;
							sortY += 1f;
							sortY /= 10000f;
							Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
							sourceRect = this.ApplySourceRectOffsets(sourceRect);
							drawOffset = Vector2.Zero;
							if (drawLayer.AnimalDoorOffset != Point.Zero)
							{
								drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
							}
							Texture2D layerTexture = this.texture.Value;
							if (drawLayer.Texture != null)
							{
								layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
							}
							b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), new Microsoft.Xna.Framework.Rectangle?(sourceRect), this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
						}
					}
				}
			}
			if (this.daysUntilUpgrade.Value > 0)
			{
				if (data != null)
				{
					if (data.UpgradeSignTile.X >= 0f)
					{
						sortY = ((float)this.tileY.Value + data.UpgradeSignTile.Y + 1f) * 64f;
						sortY += 2f;
						sortY /= 10000f;
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15)), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY);
						return;
					}
				}
				else if (this.GetIndoors() is Shed)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15)), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				}
			}
		}

		// Token: 0x0600375F RID: 14175 RVA: 0x002BC230 File Offset: 0x002BA430
		public bool ShouldDrawShadow(BuildingData data)
		{
			return data == null || data.DrawShadow;
		}

		// Token: 0x06003760 RID: 14176 RVA: 0x002BC240 File Offset: 0x002BA440
		public virtual void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu() ?? this.getSourceRect();
			Vector2 basePosition = (localX == -1) ? Game1.GlobalToLocal(new Vector2((float)(this.tileX.Value * 64), (float)((this.tileY.Value + this.tilesHigh.Value) * 64))) : new Vector2((float)localX, (float)(localY + sourceRectForMenu.Height * 4));
			b.Draw(Game1.mouseCursors, basePosition, new Microsoft.Xna.Framework.Rectangle?(Building.leftShadow), Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			for (int x = 1; x < this.tilesWide.Value - 1; x++)
			{
				b.Draw(Game1.mouseCursors, basePosition + new Vector2((float)(x * 64), 0f), new Microsoft.Xna.Framework.Rectangle?(Building.middleShadow), Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			b.Draw(Game1.mouseCursors, basePosition + new Vector2((float)((this.tilesWide.Value - 1) * 64), 0f), new Microsoft.Xna.Framework.Rectangle?(Building.rightShadow), Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		}

		// Token: 0x06003761 RID: 14177 RVA: 0x002BC3D8 File Offset: 0x002BA5D8
		public virtual void OnStartMove()
		{
		}

		// Token: 0x06003762 RID: 14178 RVA: 0x002BC3DA File Offset: 0x002BA5DA
		public virtual void OnEndMove()
		{
			Game1.player.team.SendBuildingMovedEvent(this.GetParentLocation(), this);
		}

		// Token: 0x06003763 RID: 14179 RVA: 0x002BC3F2 File Offset: 0x002BA5F2
		public Point getPorchStandingSpot()
		{
			if (this.isCabin)
			{
				return new Point(this.tileX.Value + 1, this.tileY.Value + this.tilesHigh.Value - 1);
			}
			return new Point(0, 0);
		}

		// Token: 0x06003764 RID: 14180 RVA: 0x002BC430 File Offset: 0x002BA630
		public virtual bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			BuildingData data = this.GetData();
			if (data != null && this.daysOfConstructionLeft.Value <= 0 && data.HasPropertyAtTile(tile_x - this.tileX.Value, tile_y - this.tileY.Value, property_name, layer_name, ref property_value))
			{
				return true;
			}
			if (property_name == "NoSpawn" && layer_name == "Back" && this.occupiesTile(tile_x, tile_y, false))
			{
				property_value = "All";
				return true;
			}
			return false;
		}

		// Token: 0x06003765 RID: 14181 RVA: 0x002BC4B0 File Offset: 0x002BA6B0
		public Point getMailboxPosition()
		{
			if (this.isCabin)
			{
				return new Point(this.tileX.Value + this.tilesWide.Value - 1, this.tileY.Value + this.tilesHigh.Value - 1);
			}
			return new Point(68, 16);
		}

		// Token: 0x06003766 RID: 14182 RVA: 0x002BC506 File Offset: 0x002BA706
		public virtual int GetAdditionalTilePropertyRadius()
		{
			BuildingData data = this.GetData();
			if (data == null)
			{
				return 0;
			}
			return data.AdditionalTilePropertyRadius;
		}

		// Token: 0x06003767 RID: 14183 RVA: 0x002BC51C File Offset: 0x002BA71C
		public void removeOverlappingBushes(GameLocation location)
		{
			for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
			{
				for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
				{
					if (location.isTerrainFeatureAt(x, y))
					{
						LargeTerrainFeature large_feature = location.getLargeTerrainFeatureAt(x, y);
						if (large_feature is Bush)
						{
							location.largeTerrainFeatures.Remove(large_feature);
						}
					}
				}
			}
		}

		// Token: 0x06003768 RID: 14184 RVA: 0x002BC5AC File Offset: 0x002BA7AC
		public virtual void drawInConstruction(SpriteBatch b)
		{
			int drawPercentage = Math.Min(16, Math.Max(0, (int)(16f - (float)this.newConstructionTimer.Value / 1000f * 16f)));
			float drawPercentageReal = (float)(2000 - this.newConstructionTimer.Value) / 2000f;
			if (!this.magical.Value && this.daysOfConstructionLeft.Value > 0)
			{
				bool drawFloor = this.daysOfConstructionLeft.Value == 1;
				for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
				{
					for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
					{
						if (x == this.tileX.Value + this.tilesWide.Value / 2 && y == this.tileY.Value + this.tilesHigh.Value - 1)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16 - 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 309, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
						}
						else if (x == this.tileX.Value && y == this.tileY.Value)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 293, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
						}
						else if (x == this.tileX.Value + this.tilesWide.Value - 1 && y == this.tileY.Value)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 293, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
						}
						else if (x == this.tileX.Value + this.tilesWide.Value - 1 && y == this.tileY.Value + this.tilesHigh.Value - 1)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 277, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 325, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
						}
						else if (x == this.tileX.Value && y == this.tileY.Value + this.tilesHigh.Value - 1)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 277, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 325, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
						}
						else if (x == this.tileX.Value + this.tilesWide.Value - 1)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(383, 309, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
						}
						else if (y == this.tileY.Value + this.tilesHigh.Value - 1)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 325, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
						}
						else if (x == this.tileX.Value)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(351, 309, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
						}
						else if (y == this.tileY.Value)
						{
							if (drawFloor)
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4)) + ((this.newConstructionTimer.Value > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 293, 16, drawPercentage)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
						}
						else if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f) + new Vector2(0f, (float)(64 - drawPercentage * 4 + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
					}
				}
				return;
			}
			BuildingData data = this.GetData();
			if (this.ShouldDrawShadow(data))
			{
				this.drawShadow(b, -1, -1);
			}
			Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu().GetValueOrDefault(mainSourceRect);
			int yPos = (int)((float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal));
			float baseSortY = (float)((this.tileY.Value + this.tilesHigh.Value) * 64);
			float sortY = baseSortY;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64));
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Vector2 offset = new Vector2(0f, (float)(yPos + 4 - yPos % 4));
			Vector2 drawOrigin = new Vector2(0f, (float)mainSourceRect.Height);
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + offset + drawOffset), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(mainSourceRect.Left, mainSourceRect.Bottom - (int)(drawPercentageReal * (float)mainSourceRect.Height), sourceRectForMenu.Width, (int)((float)mainSourceRect.Height * drawPercentageReal))), this.color * this.alpha, 0f, new Vector2(0f, (float)mainSourceRect.Height), 4f, SpriteEffects.None, sortY);
			if (((data != null) ? data.DrawLayers : null) != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.OnlyDrawIfChestHasContents == null)
					{
						sortY = baseSortY - drawLayer.SortTileOffset * 64f;
						sortY += 1f;
						sortY /= 10000f;
						Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
						sourceRect = this.ApplySourceRectOffsets(sourceRect);
						float cutoffPixels = (float)(yPos / 4) - drawLayer.DrawPosition.Y;
						drawOffset = Vector2.Zero;
						if (cutoffPixels <= (float)sourceRect.Height)
						{
							if (cutoffPixels > 0f)
							{
								drawOffset.Y += cutoffPixels;
								sourceRect.Y += (int)cutoffPixels;
								sourceRect.Height -= (int)cutoffPixels;
							}
							Texture2D layerTexture = this.texture.Value;
							if (drawLayer.Texture != null)
							{
								layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
							}
							b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), new Microsoft.Xna.Framework.Rectangle?(sourceRect), this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
						}
					}
				}
			}
			if (this.magical.Value)
			{
				for (int i = 0; i < this.tilesWide.Value * 4; i++)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + i * 16), (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2((float)Game1.random.Next(-1, 2), (float)(Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(536 + (this.newConstructionTimer.Value + i * 4) % 56 / 8 * 8, 1945, 8, 8)), (i % 2 == 1) ? (Color.Pink * this.alpha) : (Color.LightPink * this.alpha), 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
					if (i % 2 == 0)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 + i * 16), (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2((float)Game1.random.Next(-1, 2), (float)(Game1.random.Next(-1, 2) + ((i % 2 == 0) ? 32 : 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(536 + (this.newConstructionTimer.Value + i * 4) % 56 / 8 * 8, 1945, 8, 8)), Color.White * this.alpha, 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
					}
				}
				return;
			}
			for (int j = 0; j < this.tilesWide.Value * 4; j++)
			{
				b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 - 16 + j * 16), (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2((float)Game1.random.Next(-1, 2), (float)(Game1.random.Next(-1, 2) - ((j % 2 == 0) ? 32 : 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((this.newConstructionTimer.Value + j * 20) % 304 / 38 * 64, 768, 64, 64)), Color.White * this.alpha * ((float)this.newConstructionTimer.Value / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				if (j % 2 == 0)
				{
					b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value * 64 - 16 + j * 16), (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2((float)Game1.random.Next(-1, 2), (float)(Game1.random.Next(-1, 2) - ((j % 2 == 0) ? 32 : 8))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((this.newConstructionTimer.Value + j * 20) % 400 / 50 * 64, 2944, 64, 64)), Color.White * this.alpha * ((float)this.newConstructionTimer.Value / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				}
			}
		}

		// Token: 0x040023D1 RID: 9169
		[XmlElement("id")]
		public readonly NetGuid id = new NetGuid();

		// Token: 0x040023D2 RID: 9170
		[XmlIgnore]
		public Lazy<Texture2D> texture;

		// Token: 0x040023D3 RID: 9171
		[XmlIgnore]
		public Texture2D paintedTexture;

		// Token: 0x040023D4 RID: 9172
		public NetString skinId = new NetString();

		// Token: 0x040023D5 RID: 9173
		[XmlElement("indoors")]
		public readonly NetRef<GameLocation> indoors = new NetRef<GameLocation>();

		// Token: 0x040023D6 RID: 9174
		public readonly NetString nonInstancedIndoorsName = new NetString();

		// Token: 0x040023D7 RID: 9175
		[XmlElement("tileX")]
		public readonly NetInt tileX = new NetInt();

		// Token: 0x040023D8 RID: 9176
		[XmlElement("tileY")]
		public readonly NetInt tileY = new NetInt();

		// Token: 0x040023D9 RID: 9177
		[XmlElement("tilesWide")]
		public readonly NetInt tilesWide = new NetInt();

		// Token: 0x040023DA RID: 9178
		[XmlElement("tilesHigh")]
		public readonly NetInt tilesHigh = new NetInt();

		// Token: 0x040023DB RID: 9179
		[XmlElement("maxOccupants")]
		public readonly NetInt maxOccupants = new NetInt();

		// Token: 0x040023DC RID: 9180
		[XmlElement("currentOccupants")]
		public readonly NetInt currentOccupants = new NetInt();

		// Token: 0x040023DD RID: 9181
		[XmlElement("daysOfConstructionLeft")]
		public readonly NetInt daysOfConstructionLeft = new NetInt();

		// Token: 0x040023DE RID: 9182
		[XmlElement("daysUntilUpgrade")]
		public readonly NetInt daysUntilUpgrade = new NetInt();

		// Token: 0x040023DF RID: 9183
		[XmlElement("upgradeName")]
		public readonly NetString upgradeName = new NetString();

		// Token: 0x040023E0 RID: 9184
		[XmlElement("buildingType")]
		public readonly NetString buildingType = new NetString();

		// Token: 0x040023E1 RID: 9185
		[XmlElement("buildingPaintColor")]
		public NetRef<BuildingPaintColor> netBuildingPaintColor = new NetRef<BuildingPaintColor>();

		// Token: 0x040023E2 RID: 9186
		[XmlElement("hayCapacity")]
		public NetInt hayCapacity = new NetInt();

		// Token: 0x040023E3 RID: 9187
		public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();

		// Token: 0x040023E4 RID: 9188
		[XmlIgnore]
		public NetString parentLocationName = new NetString();

		// Token: 0x040023E5 RID: 9189
		[XmlIgnore]
		public bool hasLoaded;

		// Token: 0x040023E6 RID: 9190
		[XmlIgnore]
		protected Dictionary<string, string> buildingMetadata = new Dictionary<string, string>();

		// Token: 0x040023E7 RID: 9191
		protected int lastHouseUpgradeLevel = -1;

		// Token: 0x040023E8 RID: 9192
		protected bool? hasChimney;

		// Token: 0x040023E9 RID: 9193
		protected Vector2 chimneyPosition = Vector2.Zero;

		// Token: 0x040023EA RID: 9194
		protected int chimneyTimer = 500;

		// Token: 0x040023EC RID: 9196
		[XmlElement("humanDoor")]
		public readonly NetPoint humanDoor = new NetPoint();

		// Token: 0x040023ED RID: 9197
		[XmlElement("animalDoor")]
		public readonly NetPoint animalDoor = new NetPoint();

		// Token: 0x040023EE RID: 9198
		[XmlIgnore]
		public Color color = Color.White;

		// Token: 0x040023EF RID: 9199
		[XmlElement("animalDoorOpen")]
		public readonly NetBool animalDoorOpen = new NetBool();

		// Token: 0x040023F0 RID: 9200
		[XmlElement("animalDoorOpenAmount")]
		public readonly NetFloat animalDoorOpenAmount = new NetFloat
		{
			InterpolationWait = false
		};

		// Token: 0x040023F1 RID: 9201
		[XmlElement("magical")]
		public readonly NetBool magical = new NetBool();

		// Token: 0x040023F2 RID: 9202
		[XmlElement("fadeWhenPlayerIsBehind")]
		public readonly NetBool fadeWhenPlayerIsBehind = new NetBool(true);

		// Token: 0x040023F3 RID: 9203
		[XmlElement("owner")]
		public readonly NetLong owner = new NetLong();

		// Token: 0x040023F4 RID: 9204
		[XmlElement("newConstructionTimer")]
		protected readonly NetInt newConstructionTimer = new NetInt();

		// Token: 0x040023F5 RID: 9205
		[XmlIgnore]
		public float alpha = 1f;

		// Token: 0x040023F6 RID: 9206
		[XmlIgnore]
		protected bool _isMoving;

		// Token: 0x040023F8 RID: 9208
		public static Microsoft.Xna.Framework.Rectangle leftShadow = new Microsoft.Xna.Framework.Rectangle(656, 394, 16, 16);

		// Token: 0x040023F9 RID: 9209
		public static Microsoft.Xna.Framework.Rectangle middleShadow = new Microsoft.Xna.Framework.Rectangle(672, 394, 16, 16);

		// Token: 0x040023FA RID: 9210
		public static Microsoft.Xna.Framework.Rectangle rightShadow = new Microsoft.Xna.Framework.Rectangle(688, 394, 16, 16);
	}
}
