using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.GarbageCans;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Minecarts;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Pets;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x020000D1 RID: 209
	[XmlInclude(typeof(AbandonedJojaMart))]
	[XmlInclude(typeof(AdventureGuild))]
	[XmlInclude(typeof(AnimalHouse))]
	[XmlInclude(typeof(BathHousePool))]
	[XmlInclude(typeof(Beach))]
	[XmlInclude(typeof(BeachNightMarket))]
	[XmlInclude(typeof(BoatTunnel))]
	[XmlInclude(typeof(BugLand))]
	[XmlInclude(typeof(BusStop))]
	[XmlInclude(typeof(Cabin))]
	[XmlInclude(typeof(Caldera))]
	[XmlInclude(typeof(Cellar))]
	[XmlInclude(typeof(Club))]
	[XmlInclude(typeof(CommunityCenter))]
	[XmlInclude(typeof(DecoratableLocation))]
	[XmlInclude(typeof(Desert))]
	[XmlInclude(typeof(DesertFestival))]
	[XmlInclude(typeof(Farm))]
	[XmlInclude(typeof(FarmCave))]
	[XmlInclude(typeof(FarmHouse))]
	[XmlInclude(typeof(FishShop))]
	[XmlInclude(typeof(Forest))]
	[XmlInclude(typeof(IslandEast))]
	[XmlInclude(typeof(IslandFarmCave))]
	[XmlInclude(typeof(IslandFarmHouse))]
	[XmlInclude(typeof(IslandFieldOffice))]
	[XmlInclude(typeof(IslandForestLocation))]
	[XmlInclude(typeof(IslandHut))]
	[XmlInclude(typeof(IslandLocation))]
	[XmlInclude(typeof(IslandNorth))]
	[XmlInclude(typeof(IslandSecret))]
	[XmlInclude(typeof(IslandShrine))]
	[XmlInclude(typeof(IslandSouth))]
	[XmlInclude(typeof(IslandSouthEast))]
	[XmlInclude(typeof(IslandSouthEastCave))]
	[XmlInclude(typeof(IslandWest))]
	[XmlInclude(typeof(IslandWestCave1))]
	[XmlInclude(typeof(JojaMart))]
	[XmlInclude(typeof(LibraryMuseum))]
	[XmlInclude(typeof(ManorHouse))]
	[XmlInclude(typeof(MermaidHouse))]
	[XmlInclude(typeof(Mine))]
	[XmlInclude(typeof(MineShaft))]
	[XmlInclude(typeof(Mountain))]
	[XmlInclude(typeof(MovieTheater))]
	[XmlInclude(typeof(Railroad))]
	[XmlInclude(typeof(SeedShop))]
	[XmlInclude(typeof(Sewer))]
	[XmlInclude(typeof(Shed))]
	[XmlInclude(typeof(ShopLocation))]
	[XmlInclude(typeof(SlimeHutch))]
	[XmlInclude(typeof(Submarine))]
	[XmlInclude(typeof(Summit))]
	[XmlInclude(typeof(Town))]
	[XmlInclude(typeof(WizardHouse))]
	[XmlInclude(typeof(Woods))]
	[InstanceStatics]
	[NotImplicitNetField]
	public class GameLocation : INetObject<NetFields>, IEquatable<GameLocation>, IAnimalLocation, IHaveModData
	{
		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06000E52 RID: 3666 RVA: 0x000998CA File Offset: 0x00097ACA
		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals
		{
			get
			{
				return this.animals;
			}
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000E53 RID: 3667 RVA: 0x000998D2 File Offset: 0x00097AD2
		[XmlIgnore]
		public NetFields NetFields { get; }

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000E54 RID: 3668 RVA: 0x000998DA File Offset: 0x00097ADA
		[XmlIgnore]
		public NetRoot<GameLocation> Root
		{
			get
			{
				return this.NetFields.Root as NetRoot<GameLocation>;
			}
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000E55 RID: 3669 RVA: 0x000998EC File Offset: 0x00097AEC
		// (set) Token: 0x06000E56 RID: 3670 RVA: 0x000998F4 File Offset: 0x00097AF4
		[XmlIgnore]
		public int ExtraMillisecondsPerInGameMinute { get; set; }

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000E57 RID: 3671 RVA: 0x00099900 File Offset: 0x00097B00
		// (set) Token: 0x06000E58 RID: 3672 RVA: 0x0009994E File Offset: 0x00097B4E
		[XmlIgnore]
		public string DisplayName
		{
			get
			{
				if (this._displayName == null)
				{
					this._displayName = this.GetDisplayName();
				}
				if (this._displayName != null)
				{
					return this._displayName;
				}
				GameLocation parentLocation = this.GetParentLocation();
				string parentName = (parentLocation != null) ? parentLocation.DisplayName : null;
				if (parentName != null)
				{
					return parentName;
				}
				return this.Name;
			}
			set
			{
				this._displayName = value;
			}
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00099958 File Offset: 0x00097B58
		public virtual string GetDisplayName()
		{
			LocationData data = this.GetData();
			string displayName = (data != null) ? data.DisplayName : null;
			if (displayName == null)
			{
				return null;
			}
			return TokenParser.ParseText(displayName, null, null, null);
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000E5A RID: 3674 RVA: 0x00099986 File Offset: 0x00097B86
		[XmlIgnore]
		public string NameOrUniqueName
		{
			get
			{
				if (this.uniqueName.Value != null)
				{
					return this.uniqueName.Value;
				}
				return this.name.Value;
			}
		}

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000E5B RID: 3675 RVA: 0x000999AC File Offset: 0x00097BAC
		// (set) Token: 0x06000E5C RID: 3676 RVA: 0x000999B4 File Offset: 0x00097BB4
		[XmlIgnore]
		public bool IsTemporary { get; protected set; }

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000E5D RID: 3677 RVA: 0x000999BD File Offset: 0x00097BBD
		// (set) Token: 0x06000E5E RID: 3678 RVA: 0x000999CA File Offset: 0x00097BCA
		[XmlIgnore]
		public float LightLevel
		{
			get
			{
				return this.lightLevel.Value;
			}
			set
			{
				this.lightLevel.Value = value;
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000E5F RID: 3679 RVA: 0x000999D8 File Offset: 0x00097BD8
		// (set) Token: 0x06000E60 RID: 3680 RVA: 0x000999E6 File Offset: 0x00097BE6
		[XmlIgnore]
		public Map Map
		{
			get
			{
				this.updateMap();
				return this.map;
			}
			set
			{
				this.map = value;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000E61 RID: 3681 RVA: 0x000999EF File Offset: 0x00097BEF
		[XmlIgnore]
		public OverlaidDictionary Objects
		{
			get
			{
				return this.objects;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000E62 RID: 3682 RVA: 0x000999F7 File Offset: 0x00097BF7
		[XmlIgnore]
		public TemporaryAnimatedSpriteList TemporarySprites
		{
			get
			{
				return this.temporarySprites;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000E63 RID: 3683 RVA: 0x000999FF File Offset: 0x00097BFF
		public string Name
		{
			get
			{
				return this.name.Value;
			}
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x00099A0C File Offset: 0x00097C0C
		// (set) Token: 0x06000E65 RID: 3685 RVA: 0x00099A19 File Offset: 0x00097C19
		[XmlIgnore]
		public bool IsFarm
		{
			get
			{
				return this.isFarm.Value;
			}
			set
			{
				this.isFarm.Value = value;
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000E66 RID: 3686 RVA: 0x00099A27 File Offset: 0x00097C27
		// (set) Token: 0x06000E67 RID: 3687 RVA: 0x00099A34 File Offset: 0x00097C34
		[XmlIgnore]
		public bool IsOutdoors
		{
			get
			{
				return this.isOutdoors.Value;
			}
			set
			{
				this.isOutdoors.Value = value;
			}
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000E68 RID: 3688 RVA: 0x00099A42 File Offset: 0x00097C42
		// (set) Token: 0x06000E69 RID: 3689 RVA: 0x00099A4F File Offset: 0x00097C4F
		public bool IsGreenhouse
		{
			get
			{
				return this.isGreenhouse.Value;
			}
			set
			{
				this.isGreenhouse.Value = value;
			}
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x00099A5D File Offset: 0x00097C5D
		public virtual bool SeedsIgnoreSeasonsHere()
		{
			return this.IsGreenhouse;
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x00099A68 File Offset: 0x00097C68
		public virtual bool CanPlantSeedsHere(string itemId, int tileX, int tileY, bool isGardenPot, out string deniedMessage)
		{
			LocationData data = this.GetData();
			return this.CheckItemPlantRules(itemId, isGardenPot, ((data != null) ? data.CanPlantHere : null) ?? this.IsFarm, out deniedMessage);
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x00099AB4 File Offset: 0x00097CB4
		public virtual bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
		{
			bool isGardenPot = false;
			bool defaultAllowed;
			if (!this.IsGreenhouse && !this.IsFarm)
			{
				LocationData data = this.GetData();
				if (!(((data != null) ? data.CanPlantHere : null) ?? false) && (!Object.isWildTreeSeed(itemId) || !this.IsOutdoors || !(this.doesTileHavePropertyNoNull(tileX, tileY, "Type", "Back") == "Dirt")))
				{
					Map map = this.map;
					defaultAllowed = (((map != null) ? new bool?(map.Properties.ContainsKey("ForceAllowTreePlanting")) : null) ?? false);
					goto IL_B4;
				}
			}
			defaultAllowed = true;
			IL_B4:
			return this.CheckItemPlantRules(itemId, isGardenPot, defaultAllowed, out deniedMessage);
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00099B7C File Offset: 0x00097D7C
		public bool CheckItemPlantRules(string itemId, bool isGardenPot, bool defaultAllowed, out string deniedMessage)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			if (metadata != null && metadata.TypeIdentifier == "(O)")
			{
				itemId = metadata.LocalItemId;
				CropData cropData;
				if (Crop.TryGetData(itemId, out cropData))
				{
					return this.CheckItemPlantRules(cropData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
				}
				string wildTreeType = Tree.ResolveTreeTypeFromSeed(metadata.QualifiedItemId);
				WildTreeData wildTreeData;
				if (wildTreeType != null && Tree.TryGetData(wildTreeType, out wildTreeData))
				{
					return this.CheckItemPlantRules(wildTreeData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
				}
				FruitTreeData fruitTreeData;
				if (FruitTree.TryGetData(itemId, out fruitTreeData))
				{
					return this.CheckItemPlantRules(fruitTreeData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
				}
			}
			deniedMessage = null;
			return defaultAllowed;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x00099C14 File Offset: 0x00097E14
		private bool CheckItemPlantRules(List<PlantableRule> rules, bool isGardenPot, bool defaultAllowed, out string deniedMessage)
		{
			if (rules != null && rules.Count > 0)
			{
				foreach (PlantableRule rule in rules)
				{
					if (rule.ShouldApplyWhen(isGardenPot) && GameStateQuery.CheckConditions(rule.Condition, this, null, null, null, null, null))
					{
						PlantableResult result = rule.Result;
						if (result == PlantableResult.Allow)
						{
							deniedMessage = null;
							return true;
						}
						if (result != PlantableResult.Deny)
						{
							deniedMessage = ((!defaultAllowed) ? TokenParser.ParseText(rule.DeniedMessage, null, null, null) : null);
							return defaultAllowed;
						}
						deniedMessage = TokenParser.ParseText(rule.DeniedMessage, null, null, null);
						return false;
					}
				}
			}
			deniedMessage = null;
			return defaultAllowed;
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000E6F RID: 3695 RVA: 0x00099CDC File Offset: 0x00097EDC
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000E70 RID: 3696 RVA: 0x00099CE4 File Offset: 0x00097EE4
		// (set) Token: 0x06000E71 RID: 3697 RVA: 0x00099CF1 File Offset: 0x00097EF1
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

		// Token: 0x06000E72 RID: 3698 RVA: 0x00099D00 File Offset: 0x00097F00
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.mapPath, "mapPath").AddField(this.uniqueName, "uniqueName").AddField(this.name, "name").AddField(this.lightLevel, "lightLevel").AddField(this.sharedLights, "sharedLights").AddField(this.isFarm, "isFarm").AddField(this.isOutdoors, "isOutdoors").AddField(this.isStructure, "isStructure").AddField(this.ignoreDebrisWeather, "ignoreDebrisWeather").AddField(this.ignoreOutdoorLighting, "ignoreOutdoorLighting").AddField(this.ignoreLights, "ignoreLights").AddField(this.treatAsOutdoors, "treatAsOutdoors").AddField(this.warps, "warps").AddField(this.doors, "doors").AddField(this.interiorDoors, "interiorDoors").AddField(this.waterColor, "waterColor").AddField(this.netObjects, "netObjects").AddField(this.projectiles, "projectiles").AddField(this.largeTerrainFeatures, "largeTerrainFeatures").AddField(this.terrainFeatures, "terrainFeatures").AddField(this.characters, "characters").AddField(this.debris, "debris").AddField(this.netAudio.NetFields, "netAudio.NetFields").AddField(this.removeTemporarySpritesWithIDEvent, "removeTemporarySpritesWithIDEvent").AddField(this.rumbleAndFadeEvent, "rumbleAndFadeEvent").AddField(this.damagePlayersEvent, "damagePlayersEvent").AddField(this.lightGlows, "lightGlows").AddField(this.fishSplashPoint, "fishSplashPoint").AddField(this.fishFrenzyFish, "fishFrenzyFish").AddField(this.orePanPoint, "orePanPoint").AddField(this.isGreenhouse, "isGreenhouse").AddField(this.miniJukeboxCount, "miniJukeboxCount").AddField(this.miniJukeboxTrack, "miniJukeboxTrack").AddField(this.randomMiniJukeboxTrack, "randomMiniJukeboxTrack").AddField(this.resourceClumps, "resourceClumps").AddField(this.isAlwaysActive, "isAlwaysActive").AddField(this.furniture, "furniture").AddField(this.furnitureToRemove.NetFields, "furnitureToRemove.NetFields").AddField(this.parentLocationName, "parentLocationName").AddField(this.buildings, "buildings").AddField(this.animals, "animals").AddField(this.piecesOfHay, "piecesOfHay").AddField(this.mapSeats, "mapSeats").AddField(this.modData, "modData");
			this.mapPath.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._mapPathDirty = true;
			};
			this.name.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this.OnNameChanged();
			};
			this.uniqueName.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this.OnNameChanged();
			};
			this.parentLocationName.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this.OnParentLocationChanged();
			};
			this.buildings.OnValueAdded += delegate(Building b)
			{
				if (b != null)
				{
					b.parentLocationName.Value = this.NameOrUniqueName;
					b.updateInteriorWarps(null);
				}
				if (Game1.IsMasterGame)
				{
					Game1.netWorldState.Value.UpdateBuildingCache(this);
				}
			};
			this.buildings.OnValueRemoved += delegate(Building b)
			{
				if (b != null)
				{
					b.parentLocationName.Value = null;
				}
				if (Game1.IsMasterGame)
				{
					Game1.netWorldState.Value.UpdateBuildingCache(this);
				}
			};
			this.isStructure.fieldChangeVisibleEvent += delegate(NetBool <p0>, bool <p1>, bool <p2>)
			{
				if (this.mapPath.Value != null)
				{
					this.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
					this.reloadMap();
				}
			};
			this.sharedLights.OnValueAdded += delegate(string identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Add(light);
				}
			};
			this.sharedLights.OnValueRemoved += delegate(string identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Remove((light != null) ? light.Id : null);
				}
			};
			this.netObjects.OnConflictResolve += delegate(Vector2 pos, NetRef<Object> rejected, NetRef<Object> accepted)
			{
				if (Game1.IsMasterGame)
				{
					Object obj = rejected.Value;
					if (obj != null)
					{
						obj.onDetachedFromParent();
						obj.dropItem(this, pos * 64f, pos * 64f);
					}
				}
			};
			this.netObjects.OnValueAdded += this.OnObjectAdded;
			this.overlayObjects.onValueAdded += this.OnObjectAdded;
			this.removeTemporarySpritesWithIDEvent.onEvent += this.removeTemporarySpritesWithIDLocal;
			this.rumbleAndFadeEvent.onEvent += this.performRumbleAndFade;
			this.damagePlayersEvent.onEvent += this.performDamagePlayers;
			this.fishSplashPoint.fieldChangeVisibleEvent += delegate(NetPoint <p0>, Point <p1>, Point <p2>)
			{
				this.updateFishSplashAnimation();
			};
			this.orePanPoint.fieldChangeVisibleEvent += delegate(NetPoint <p0>, Point <p1>, Point <p2>)
			{
				this.updateOrePanAnimation();
			};
			this.characters.OnValueRemoved += delegate(NPC npc)
			{
				npc.Removed();
			};
			this.terrainFeatures.OnValueAdded += delegate(Vector2 tile, TerrainFeature feature)
			{
				this.OnTerrainFeatureAdded(feature, tile);
			};
			this.terrainFeatures.OnValueRemoved += delegate(Vector2 tile, TerrainFeature feature)
			{
				this.OnTerrainFeatureRemoved(feature);
			};
			this.largeTerrainFeatures.OnValueAdded += delegate(LargeTerrainFeature feature)
			{
				this.OnTerrainFeatureAdded(feature, feature.Tile);
			};
			this.largeTerrainFeatures.OnValueRemoved += new NetCollection<LargeTerrainFeature>.ContentsChangeEvent(this.OnTerrainFeatureRemoved);
			this.resourceClumps.OnValueAdded += this.OnResourceClumpAdded;
			this.resourceClumps.OnValueRemoved += this.OnResourceClumpRemoved;
			this.furniture.OnValueAdded += delegate(Furniture f)
			{
				f.Location = this;
				f.OnAdded(this, f.TileLocation);
			};
			this.furniture.OnValueRemoved += delegate(Furniture f)
			{
				f.OnRemoved(this, f.TileLocation);
			};
			this.furnitureToRemove.Processor = new Action<Guid>(this.removeQueuedFurniture);
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x0009A26A File Offset: 0x0009846A
		public virtual void InvalidateCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (Game1.IsMasterGame)
			{
				return;
			}
			cached_data.Remove(this.NameOrUniqueName);
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x0009A284 File Offset: 0x00098484
		public virtual void MakeMapModifications(bool force = false)
		{
			if (force)
			{
				this._appliedMapOverrides.Clear();
			}
			this.interiorDoors.MakeMapModifications();
			string value = this.name.Value;
			if (value != null)
			{
				switch (value.Length)
				{
				case 6:
					if (!(value == "Saloon"))
					{
						return;
					}
					if (NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
					{
						this.ApplyMapOverride("RefurbishedSaloonRoom", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(32, 1, 6, 8)));
						Game1.currentLightSources.Add(new LightSource("Saloon_1", 1, new Vector2(33f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
						Game1.currentLightSources.Add(new LightSource("Saloon_2", 1, new Vector2(36f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
						Game1.currentLightSources.Add(new LightSource("Saloon_3", 1, new Vector2(34f, 5f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
						return;
					}
					break;
				case 7:
				{
					if (!(value == "Sunroom"))
					{
						return;
					}
					TileSheet tileSheet = this.map.RequireTileSheet(1, "2");
					string imageDir = Path.GetDirectoryName(tileSheet.ImageSource);
					if (string.IsNullOrWhiteSpace(imageDir))
					{
						imageDir = "Maps";
					}
					tileSheet.ImageSource = Path.Combine(imageDir, "CarolineGreenhouseTiles" + ((this.IsRainingHere() || Game1.timeOfDay > Game1.getTrulyDarkTime(this)) ? "_rainy" : ""));
					this.map.DisposeTileSheets(Game1.mapDisplayDevice);
					this.map.LoadTileSheets(Game1.mapDisplayDevice);
					return;
				}
				case 8:
					if (!(value == "WitchHut"))
					{
						return;
					}
					if (Game1.player.mailReceived.Contains("hasPickedUpMagicInk"))
					{
						this.setMapTile(4, 11, 113, "Buildings", "untitled tile sheet", null, true).Properties.Remove("Action");
						return;
					}
					break;
				case 9:
				{
					char c = value[0];
					if (c != 'B')
					{
						if (c != 'S')
						{
							return;
						}
						if (!(value == "SkullCave"))
						{
							return;
						}
						bool showShrineActivated = Game1.player.team.skullShrineActivated.Value || Game1.player.team.SpecialOrderRuleActive("SC_HARD", null);
						if (Game1.player.team.toggleSkullShrineOvernight.Value)
						{
							showShrineActivated = !showShrineActivated;
						}
						if (showShrineActivated)
						{
							this._appliedMapOverrides.Remove("SkullCaveAltarDeactivated");
							this.ApplyMapOverride("SkullCaveAltar", new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 5, 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4)));
							Game1.currentLightSources.Add(new LightSource("SkullCaveAltar", 4, new Vector2(12f, 3f) * 64f, 1f, LightSource.LightContext.MapLight, 0L, this.NameOrUniqueName));
							AmbientLocationSounds.addSound(new Vector2(12f, 3f), 1);
							return;
						}
						this._appliedMapOverrides.Remove("SkullCaveAltar");
						this.ApplyMapOverride(Game1.temporaryContent.Load<Map>("Maps\\SkullCave"), "SkullCaveAltarDeactivated", new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4)), null);
						Game1.currentLightSources.Remove("SkullCaveAltar");
						AmbientLocationSounds.removeSound(new Vector2(12f, 3f));
					}
					else
					{
						if (!(value == "Backwoods"))
						{
							return;
						}
						if (Game1.netWorldState.Value.hasWorldStateID("golemGrave"))
						{
							this.ApplyMapOverride("Backwoods_GraveSite", null, null);
						}
						if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts") && !this._appliedMapOverrides.Contains("Backwoods_Staircase"))
						{
							this.ApplyMapOverride("Backwoods_Staircase", null, null);
							LargeTerrainFeature blockingBush = null;
							foreach (LargeTerrainFeature t in this.largeTerrainFeatures)
							{
								if (t.Tile == new Vector2(37f, 16f))
								{
									blockingBush = t;
									break;
								}
							}
							if (blockingBush != null)
							{
								this.largeTerrainFeatures.Remove(blockingBush);
							}
						}
						if (!Game1.player.mailReceived.Contains("asdlkjfg1") || Game1.random.NextDouble() < 0.01)
						{
							this.setTileProperty(13, 29, "Back", "TouchAction", "asdlfkjg");
							this.setTileProperty(14, 29, "Back", "TouchAction", "asdlfkjg");
							this.setTileProperty(15, 29, "Back", "TouchAction", "asdlfkjg");
						}
						else if (Utility.doesAnyFarmerHaveMail("asdlkjfg1") && Utility.CreateDaySaveRandom(1244.0, 0.0, 0.0).NextDouble() < 0.02)
						{
							if (!this.IsTileOccupiedBy(new Vector2(13f, 26f), CollisionMask.All, CollisionMask.None, false))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 26f) * 64f, false, 0.003f, Color.White)
								{
									scale = 4f,
									layerDepth = 0f
								});
							}
							if (!this.IsTileOccupiedBy(new Vector2(12f, 25f), CollisionMask.All, CollisionMask.None, false))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(12f, 25f) * 64f, true, 0.003f, Color.White)
								{
									scale = 4f,
									layerDepth = 0f
								});
							}
							if (!this.IsTileOccupiedBy(new Vector2(13f, 24f), CollisionMask.All, CollisionMask.None, false))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 24f) * 64f, false, 0.003f, Color.White)
								{
									scale = 4f,
									layerDepth = 0f
								});
							}
							if (!this.IsTileOccupiedBy(new Vector2(13f, 23f), CollisionMask.All, CollisionMask.None, false))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(12f, 23f) * 64f, true, 0.003f, Color.White * 0.66f)
								{
									scale = 4f,
									layerDepth = 0f
								});
							}
							if (!this.IsTileOccupiedBy(new Vector2(13f, 22f), CollisionMask.All, CollisionMask.None, false))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 22f) * 64f, false, 0.003f, Color.White * 0.33f)
								{
									scale = 4f,
									layerDepth = 0f
								});
							}
						}
						if (Game1.timeOfDay >= 2400)
						{
							Random asdfaTime = Utility.CreateDaySaveRandom(124.0, 0.0, 0.0);
							int time = Utility.ModifyTime(2400, asdfaTime.Next(12) * 10);
							if (Game1.timeOfDay == time && asdfaTime.NextDouble() < 0.33)
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 48, 32, 48), new Vector2(60f, -260f), true, 0f, Color.White)
								{
									animationLength = 8,
									totalNumberOfLoops = 99,
									interval = 120f,
									scale = 4f,
									motion = new Vector2(0.5f, 1f),
									yStopCoordinate = 256,
									xStopCoordinate = 256,
									delayBeforeAnimationStart = 1000
								});
								return;
							}
						}
					}
					break;
				}
				case 10:
				{
					char c = value[0];
					if (c != 'H')
					{
						if (c != 'W')
						{
							return;
						}
						if (!(value == "WitchSwamp"))
						{
							return;
						}
						if (Game1.MasterPlayer.mailReceived.Contains("henchmanGone"))
						{
							this.removeTile(20, 29, "Buildings");
							return;
						}
						this.setMapTile(20, 29, 10, "Buildings", "wt", null, true);
						return;
					}
					else
					{
						if (!(value == "HaleyHouse"))
						{
							return;
						}
						if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse != "Emily")
						{
							this.setMapTile(14, 4, 2173, "Buildings", "1", null, true);
							this.setMapTile(14, 3, 2141, "Buildings", "1", null, true);
							this.setMapTile(14, 3, 219, "Back", "1", null, true);
							return;
						}
					}
					break;
				}
				case 11:
				{
					if (!(value == "MasteryCave"))
					{
						return;
					}
					Game1.stats.Get("MasteryExp");
					int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
					GameLocation.<>c__DisplayClass162_0 CS$<>8__locals1;
					CS$<>8__locals1.levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(4, new Vector2(54f, 98f), ref CS$<>8__locals1);
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(2, new Vector2(84f, 82f), ref CS$<>8__locals1);
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(0, new Vector2(116f, 82f), ref CS$<>8__locals1);
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(0, new Vector2(116f, 82f), ref CS$<>8__locals1);
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(1, new Vector2(148f, 82f), ref CS$<>8__locals1);
					this.<MakeMapModifications>g__ShowSkillMastery|162_0(3, new Vector2(179f, 98f), ref CS$<>8__locals1);
					if (MasteryTrackerMenu.hasCompletedAllMasteryPlaques())
					{
						MasteryTrackerMenu.addSpiritCandles(true);
						Game1.changeMusicTrack("grandpas_theme", false, MusicContext.Default);
						return;
					}
					break;
				}
				case 12:
				case 13:
				case 14:
				case 15:
				case 18:
					break;
				case 16:
					if (!(value == "IslandNorthCave1"))
					{
						return;
					}
					if (Game1.player.mailReceived.Contains("FizzIntro"))
					{
						if (this.getCharacterFromName("Fizz") == null)
						{
							this.characters.Add(new NPC(new AnimatedSprite("Characters\\Fizz", 0, 16, 32), new Vector2(6f, 3f) * 64f, 2, "Fizz", null)
							{
								SimpleNonVillagerNPC = true,
								Portrait = Game1.content.Load<Texture2D>("Portraits\\Fizz"),
								displayName = Game1.content.LoadString("Strings\\NPCNames:Fizz")
							});
							this.removeObjectsAndSpawned(6, 3, 1, 1);
						}
						else
						{
							this.getCharacterFromName("Fizz").SimpleNonVillagerNPC = true;
							this.getCharacterFromName("Fizz").Sprite.SpriteHeight = 32;
							this.getCharacterFromName("Fizz").Sprite.UpdateSourceRect();
						}
						Game1.currentLightSources.Add(new LightSource("IslandNorthCave1", 1, new Vector2(6f, 3f) * 64f + new Vector2(32f), 2f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
						return;
					}
					break;
				case 17:
					if (!(value == "AbandonedJojaMart"))
					{
						return;
					}
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						StaticTile[] tileFrames = CommunityCenter.getJunimoNoteTileFrames(0, this.map);
						string layer = "Buildings";
						Point position = new Point(8, 8);
						this.map.RequireLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(this.map.RequireLayer(layer), tileFrames, 70L);
						return;
					}
					this.removeTile(8, 8, "Buildings");
					return;
				case 19:
					if (!(value == "WizardHouseBasement"))
					{
						return;
					}
					if (Game1.player.mailReceived.Contains("hasActivatedForestPylon"))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 106, 14, 22), new Vector2(16.6f, 2.5f) * 64f, false, 0f, Color.White)
						{
							animationLength = 8,
							interval = 100f,
							totalNumberOfLoops = 9999,
							scale = 4f
						});
						return;
					}
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000E75 RID: 3701 RVA: 0x0009B008 File Offset: 0x00099208
		public virtual bool ApplyCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data, string requested_map_path)
		{
			if (Game1.IsMasterGame)
			{
				return false;
			}
			CachedMultiplayerMap data;
			if (!cached_data.TryGetValue(this.NameOrUniqueName, out data))
			{
				return false;
			}
			if (data.mapPath == requested_map_path)
			{
				this._appliedMapOverrides = data.appliedMapOverrides;
				this.map = data.map;
				this.loadedMapPath = data.loadedMapPath;
				return true;
			}
			cached_data.Remove(this.NameOrUniqueName);
			return false;
		}

		// Token: 0x06000E76 RID: 3702 RVA: 0x0009B074 File Offset: 0x00099274
		public virtual void StoreCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (Game1.IsMasterGame)
			{
				return;
			}
			if (this is VolcanoDungeon || this is MineShaft)
			{
				return;
			}
			CachedMultiplayerMap data = new CachedMultiplayerMap();
			data.map = this.map;
			data.appliedMapOverrides = this._appliedMapOverrides;
			data.mapPath = this.mapPath.Value;
			data.loadedMapPath = this.loadedMapPath;
			cached_data[this.NameOrUniqueName] = data;
		}

		// Token: 0x06000E77 RID: 3703 RVA: 0x0009B0E4 File Offset: 0x000992E4
		public virtual void TransferDataFromSavedLocation(GameLocation l)
		{
			this.modData.Clear();
			if (l.modData != null)
			{
				foreach (string key in l.modData.Keys)
				{
					this.modData[key] = l.modData[key];
				}
			}
			this.miniJukeboxCount.Value = l.miniJukeboxCount.Value;
			this.miniJukeboxTrack.Value = l.miniJukeboxTrack.Value;
			this.SelectRandomMiniJukeboxTrack();
			this.UpdateMapSeats();
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x0009B19C File Offset: 0x0009939C
		private void OnNameChanged()
		{
			this.IsTemporary = GameLocation.IsTemporaryName(this.Name);
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x0009B1AF File Offset: 0x000993AF
		private void OnParentLocationChanged()
		{
			this.locationContextId = null;
			if (this.seasonOverride == null || this.seasonOverride.IsValueCreated)
			{
				this.seasonOverride = new Lazy<Season?>(new Func<Season?>(this.LoadSeasonOverride));
			}
		}

		// Token: 0x06000E7A RID: 3706 RVA: 0x0009B1E4 File Offset: 0x000993E4
		public virtual void OnParentBuildingUpgraded(Building building)
		{
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x0009B1E8 File Offset: 0x000993E8
		public virtual void OnRemoved()
		{
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				this.characters[i].OnLocationRemoved();
			}
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x0009B21E File Offset: 0x0009941E
		protected virtual void OnObjectAdded(Vector2 tile, Object obj)
		{
			obj.Location = this;
			obj.TileLocation = tile;
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x0009B22E File Offset: 0x0009942E
		public virtual void OnResourceClumpAdded(ResourceClump resourceClump)
		{
			resourceClump.Location = this;
			resourceClump.OnAddedToLocation(this, resourceClump.Tile);
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x0009B244 File Offset: 0x00099444
		public virtual void OnResourceClumpRemoved(ResourceClump resourceClump)
		{
			resourceClump.Location = null;
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x0009B250 File Offset: 0x00099450
		public virtual void OnTerrainFeatureAdded(TerrainFeature feature, Vector2 location)
		{
			if (feature != null)
			{
				Flooring flooring = feature as Flooring;
				if (flooring == null)
				{
					HoeDirt dirt = feature as HoeDirt;
					if (dirt != null)
					{
						dirt.OnAdded(this, location);
					}
				}
				else
				{
					flooring.OnAdded(this, location);
				}
				feature.Location = this;
				feature.Tile = location;
				feature.OnAddedToLocation(this, location);
				this.UpdateTerrainFeatureUpdateSubscription(feature);
			}
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x0009B2A8 File Offset: 0x000994A8
		public virtual void OnTerrainFeatureRemoved(TerrainFeature feature)
		{
			if (feature != null)
			{
				Flooring flooring = feature as Flooring;
				if (flooring == null)
				{
					HoeDirt dirt = feature as HoeDirt;
					if (dirt == null)
					{
						LargeTerrainFeature largeFeature = feature as LargeTerrainFeature;
						if (largeFeature != null)
						{
							largeFeature.onDestroy();
						}
					}
					else
					{
						dirt.OnRemoved();
					}
				}
				else
				{
					flooring.OnRemoved();
				}
				if (feature.NeedsUpdate)
				{
					this._activeTerrainFeatures.Remove(feature);
				}
				feature.Location = null;
			}
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x0009B30A File Offset: 0x0009950A
		public virtual void UpdateTerrainFeatureUpdateSubscription(TerrainFeature feature)
		{
			if (feature.NeedsUpdate)
			{
				this._activeTerrainFeatures.Add(feature);
				return;
			}
			this._activeTerrainFeatures.Remove(feature);
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x0009B32E File Offset: 0x0009952E
		public int GetSeasonIndex()
		{
			return (int)this.GetSeason();
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x0009B338 File Offset: 0x00099538
		private Season? LoadSeasonOverride()
		{
			if (this.map == null && this.mapPath.Value != null)
			{
				this.reloadMap();
			}
			string propertyValue;
			if (this.map != null && this.map.Properties.TryGetValue("SeasonOverride", out propertyValue) && !string.IsNullOrWhiteSpace(propertyValue))
			{
				Season season;
				if (Utility.TryParseEnum<Season>(propertyValue, out season))
				{
					return new Season?(season);
				}
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Unable to read SeasonOverride map property value '");
				defaultInterpolatedStringHandler.AppendFormatted(propertyValue);
				defaultInterpolatedStringHandler.AppendLiteral("' for location '");
				defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral("', not a valid season name.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			LocationContextData locationContext = this.GetLocationContext();
			if (locationContext == null)
			{
				return null;
			}
			return locationContext.SeasonOverride;
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x0009B40C File Offset: 0x0009960C
		public Season GetSeason()
		{
			Season? value = this.seasonOverride.Value;
			if (value != null)
			{
				return value.GetValueOrDefault();
			}
			GameLocation parentLocation = this.GetParentLocation();
			if (parentLocation == null)
			{
				return Game1.season;
			}
			return parentLocation.GetSeason();
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x0009B44B File Offset: 0x0009964B
		public string GetSeasonKey()
		{
			return Utility.getSeasonKey(this.GetSeason());
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x0009B458 File Offset: 0x00099658
		public bool IsSpringHere()
		{
			return this.GetSeason() == Season.Spring;
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x0009B463 File Offset: 0x00099663
		public bool IsSummerHere()
		{
			return this.GetSeason() == Season.Summer;
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x0009B46E File Offset: 0x0009966E
		public bool IsFallHere()
		{
			return this.GetSeason() == Season.Fall;
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x0009B479 File Offset: 0x00099679
		public bool IsWinterHere()
		{
			return this.GetSeason() == Season.Winter;
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x0009B484 File Offset: 0x00099684
		public LocationWeather GetWeather()
		{
			return Game1.netWorldState.Value.GetWeatherForLocation(this.GetLocationContextId());
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x0009B49B File Offset: 0x0009969B
		public bool IsRainingHere()
		{
			return this.GetWeather().IsRaining;
		}

		// Token: 0x06000E8C RID: 3724 RVA: 0x0009B4A8 File Offset: 0x000996A8
		public bool IsGreenRainingHere()
		{
			return this.IsRainingHere() && this.GetWeather().IsGreenRain;
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x0009B4BF File Offset: 0x000996BF
		public bool IsLightningHere()
		{
			return this.GetWeather().IsLightning;
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x0009B4CC File Offset: 0x000996CC
		public bool IsSnowingHere()
		{
			return this.GetWeather().IsSnowing;
		}

		// Token: 0x06000E8F RID: 3727 RVA: 0x0009B4D9 File Offset: 0x000996D9
		public bool IsDebrisWeatherHere()
		{
			return this.GetWeather().IsDebrisWeather;
		}

		// Token: 0x06000E90 RID: 3728 RVA: 0x0009B4E6 File Offset: 0x000996E6
		public static bool IsTemporaryName(string name)
		{
			return !string.IsNullOrEmpty(name) && (name.StartsWith("Temp", StringComparison.Ordinal) || name == "fishingGame" || name == "tent");
		}

		// Token: 0x06000E91 RID: 3729 RVA: 0x0009B51C File Offset: 0x0009971C
		private void updateFishSplashAnimation()
		{
			if (this.fishSplashPoint.Value == Point.Zero)
			{
				this.fishSplashAnimation = null;
				return;
			}
			this.fishSplashAnimation = new TemporaryAnimatedSprite(51, new Vector2((float)(this.fishSplashPoint.X * 64), (float)(this.fishSplashPoint.Y * 64)), Color.White, 10, false, 80f, 999999, -1, -1f, -1, 0)
			{
				layerDepth = (float)(this.fishSplashPoint.Y * 64 - 64 - 1) / 10000f
			};
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x0009B5B4 File Offset: 0x000997B4
		private void updateOrePanAnimation()
		{
			if (this.orePanPoint.Value == Point.Zero)
			{
				this.orePanAnimation = null;
				return;
			}
			this.orePanAnimation = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), new Vector2((float)(this.orePanPoint.X * 64 + 32), (float)(this.orePanPoint.Y * 64 + 32)), false, 0f, Color.White)
			{
				totalNumberOfLoops = 9999999,
				interval = 100f,
				scale = 3f,
				animationLength = 6
			};
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x0009B660 File Offset: 0x00099860
		public GameLocation()
		{
			this.NetFields = new NetFields(NetFields.GetNameForInstance<GameLocation>(this));
			this.farmers = new FarmerCollection(this);
			this.interiorDoors = new InteriorDoorDictionary(this);
			this.netAudio = new NetAudio(this);
			this.objects = new OverlaidDictionary(this.netObjects, this.overlayObjects);
			this._appliedMapOverrides = new HashSet<string>();
			this.terrainFeatures.SetEqualityComparer(GameLocation.tilePositionComparer);
			this.netObjects.SetEqualityComparer(GameLocation.tilePositionComparer);
			this.objects.SetEqualityComparer(GameLocation.tilePositionComparer, ref this.netObjects, ref this.overlayObjects);
			this.seasonOverride = new Lazy<Season?>(new Func<Season?>(this.LoadSeasonOverride));
			this.initNetFields();
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x0009B9F8 File Offset: 0x00099BF8
		public GameLocation(string mapPath, string name) : this()
		{
			this.mapPath.Set(mapPath);
			this.name.Value = name;
			if (name.Contains("Farm") || name.Contains("Coop") || name.Contains("Barn") || name.Equals("SlimeHutch"))
			{
				this.isFarm.Value = true;
			}
			if (name == "Greenhouse")
			{
				this.IsGreenhouse = true;
			}
			this.reloadMap();
			this.loadObjects();
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x0009BA83 File Offset: 0x00099C83
		public virtual void AddDefaultBuildings(bool load = true)
		{
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x0009BA88 File Offset: 0x00099C88
		public virtual void AddDefaultBuilding(string id, Vector2 tile, bool load = true)
		{
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.buildingType.Value == id)
					{
						return;
					}
				}
			}
			Building building = Building.CreateInstanceFromId(id, tile);
			if (load)
			{
				building.load();
			}
			this.buildings.Add(building);
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x0009BB04 File Offset: 0x00099D04
		public void playSound(string audioName, Vector2? position = null, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			Game1.sounds.PlayAll(audioName, this, position, pitch, context);
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x0009BB18 File Offset: 0x00099D18
		public void localSound(string audioName, Vector2? position = null, int? pitch = null, SoundContext context = SoundContext.Default)
		{
			ICue cue;
			Game1.sounds.PlayLocal(audioName, this, position, pitch, context, out cue);
		}

		// Token: 0x06000E99 RID: 3737 RVA: 0x0009BB38 File Offset: 0x00099D38
		protected virtual LocalizedContentManager getMapLoader()
		{
			if (this.isStructure.Value)
			{
				if (this._structureMapLoader == null)
				{
					this._structureMapLoader = Game1.game1.xTileContent.CreateTemporary();
				}
				return this._structureMapLoader;
			}
			return Game1.game1.xTileContent;
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x0009BB75 File Offset: 0x00099D75
		public void cleanUpTileForMapOverride(Point tile)
		{
			this.cleanUpTileForMapOverride(tile, null);
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x0009BB80 File Offset: 0x00099D80
		public void cleanUpTileForMapOverride(Point tile, string exceptItemId)
		{
			Vector2 tileVector = Utility.PointToVector2(tile);
			Point tileCenterPoint = Utility.Vector2ToPoint(tileVector * new Vector2(64f) + new Vector2(32f, 32f));
			NetCollection<Item> lostAndFound = Game1.player.team.returnedDonations;
			Object o;
			if (this.Objects.TryGetValue(tileVector, out o) && (exceptItemId == null || !ItemRegistry.HasItemId(o, exceptItemId)))
			{
				if (o != null && (o.HasBeenInInventory || (!o.isDebrisOrForage() && o.QualifiedItemId != "(O)590" && o.QualifiedItemId != "(O)SeedSpot")))
				{
					Chest chest = o as Chest;
					if (chest != null)
					{
						foreach (Item i in chest.Items)
						{
							lostAndFound.Add(i);
						}
						chest.Items.Clear();
					}
					else if (o.readyForHarvest.Value && o.heldObject != null)
					{
						lostAndFound.Add(o.heldObject.Value);
						o.heldObject.Value = null;
					}
					lostAndFound.Add(o);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
				this.objects.Remove(tileVector);
			}
			this.furniture.RemoveWhere(delegate(Furniture item)
			{
				if (!item.GetBoundingBox().Contains(tileCenterPoint) || (exceptItemId != null && ItemRegistry.HasItemId(item, exceptItemId)))
				{
					return false;
				}
				if (item.heldObject.Value != null)
				{
					lostAndFound.Add(item.heldObject.Value);
					item.heldObject.Value = null;
				}
				lostAndFound.Add(item);
				return true;
			});
			this.terrainFeatures.Remove(tileVector);
			this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.getBoundingBox().Contains(tileCenterPoint));
			this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.getBoundingBox().Contains(tileCenterPoint));
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x0009BD78 File Offset: 0x00099F78
		public void ApplyMapOverride(Map override_map, string override_key, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? dest_rect = null, Action<Point> perTileCustomAction = null)
		{
			if (this._appliedMapOverrides.Contains(override_key))
			{
				return;
			}
			this._appliedMapOverrides.Add(override_key);
			this.updateSeasonalTileSheets(override_map);
			Dictionary<TileSheet, TileSheet> tilesheet_lookup = new Dictionary<TileSheet, TileSheet>();
			foreach (TileSheet override_tile_sheet in override_map.TileSheets)
			{
				TileSheet map_tilesheet = this.map.GetTileSheet(override_tile_sheet.Id);
				string source_image_source = "";
				string dest_image_source = "";
				if (map_tilesheet != null)
				{
					source_image_source = map_tilesheet.ImageSource;
				}
				if (dest_image_source != null)
				{
					dest_image_source = override_tile_sheet.ImageSource;
				}
				if (map_tilesheet == null || dest_image_source != source_image_source)
				{
					map_tilesheet = new TileSheet(GameLocation.GetAddedMapOverrideTilesheetId(override_key, override_tile_sheet.Id), this.map, override_tile_sheet.ImageSource, override_tile_sheet.SheetSize, override_tile_sheet.TileSize);
					for (int i = 0; i < override_tile_sheet.TileCount; i++)
					{
						map_tilesheet.TileIndexProperties[i].CopyFrom(override_tile_sheet.TileIndexProperties[i]);
					}
					this.map.AddTileSheet(map_tilesheet);
				}
				else if (map_tilesheet.TileCount < override_tile_sheet.TileCount)
				{
					int tileCount = map_tilesheet.TileCount;
					map_tilesheet.SheetWidth = override_tile_sheet.SheetWidth;
					map_tilesheet.SheetHeight = override_tile_sheet.SheetHeight;
					for (int j = tileCount; j < override_tile_sheet.TileCount; j++)
					{
						map_tilesheet.TileIndexProperties[j].CopyFrom(override_tile_sheet.TileIndexProperties[j]);
					}
				}
				tilesheet_lookup[override_tile_sheet] = map_tilesheet;
			}
			Dictionary<Layer, Layer> layer_lookup = new Dictionary<Layer, Layer>();
			int map_width = 0;
			int map_height = 0;
			for (int layer_index = 0; layer_index < override_map.Layers.Count; layer_index++)
			{
				map_width = Math.Max(map_width, override_map.Layers[layer_index].LayerWidth);
				map_height = Math.Max(map_height, override_map.Layers[layer_index].LayerHeight);
			}
			if (source_rect == null)
			{
				source_rect = new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, map_width, map_height));
			}
			map_width = 0;
			map_height = 0;
			for (int layer_index2 = 0; layer_index2 < this.map.Layers.Count; layer_index2++)
			{
				map_width = Math.Max(map_width, this.map.Layers[layer_index2].LayerWidth);
				map_height = Math.Max(map_height, this.map.Layers[layer_index2].LayerHeight);
			}
			bool layersDirty = false;
			for (int layer_index3 = 0; layer_index3 < override_map.Layers.Count; layer_index3++)
			{
				Layer original_layer = this.map.GetLayer(override_map.Layers[layer_index3].Id);
				if (original_layer == null)
				{
					original_layer = new Layer(override_map.Layers[layer_index3].Id, this.map, new Size(map_width, map_height), override_map.Layers[layer_index3].TileSize);
					this.map.AddLayer(original_layer);
					layersDirty = true;
				}
				layer_lookup[override_map.Layers[layer_index3]] = original_layer;
			}
			if (layersDirty)
			{
				this.SortLayers();
			}
			if (dest_rect == null)
			{
				dest_rect = new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, map_width, map_height));
			}
			int source_rect_x = source_rect.Value.X;
			int source_rect_y = source_rect.Value.Y;
			int dest_rect_x = dest_rect.Value.X;
			int dest_rect_y = dest_rect.Value.Y;
			for (int x = 0; x < source_rect.Value.Width; x++)
			{
				for (int y = 0; y < source_rect.Value.Height; y++)
				{
					Point source_tile_pos = new Point(source_rect_x + x, source_rect_y + y);
					Point dest_tile_pos = new Point(dest_rect_x + x, dest_rect_y + y);
					if (perTileCustomAction != null)
					{
						perTileCustomAction(dest_tile_pos);
					}
					bool lower_layer_overridden = false;
					for (int layer_index4 = 0; layer_index4 < override_map.Layers.Count; layer_index4++)
					{
						Layer override_layer = override_map.Layers[layer_index4];
						Layer target_layer = layer_lookup[override_layer];
						if (target_layer != null && dest_tile_pos.X < target_layer.LayerWidth && dest_tile_pos.Y < target_layer.LayerHeight && (lower_layer_overridden || override_map.Layers[layer_index4].Tiles[source_tile_pos.X, source_tile_pos.Y] != null))
						{
							lower_layer_overridden = true;
							if (source_tile_pos.X < override_layer.LayerWidth && source_tile_pos.Y < override_layer.LayerHeight)
							{
								if (override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y] == null)
								{
									target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = null;
								}
								else
								{
									Tile override_tile = override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y];
									Tile new_tile = null;
									if (!(override_tile is StaticTile))
									{
										AnimatedTile override_animated_tile = override_tile as AnimatedTile;
										if (override_animated_tile != null)
										{
											StaticTile[] tiles = new StaticTile[override_animated_tile.TileFrames.Length];
											for (int k = 0; k < override_animated_tile.TileFrames.Length; k++)
											{
												StaticTile frame_tile = override_animated_tile.TileFrames[k];
												tiles[k] = new StaticTile(target_layer, tilesheet_lookup[frame_tile.TileSheet], frame_tile.BlendMode, frame_tile.TileIndex);
											}
											new_tile = new AnimatedTile(target_layer, tiles, override_animated_tile.FrameInterval);
										}
									}
									else
									{
										new_tile = new StaticTile(target_layer, tilesheet_lookup[override_tile.TileSheet], override_tile.BlendMode, override_tile.TileIndex);
									}
									if (new_tile != null)
									{
										new_tile.Properties.CopyFrom(override_tile.Properties);
									}
									target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = new_tile;
								}
							}
						}
					}
				}
			}
			this.map.LoadTileSheets(Game1.mapDisplayDevice);
			if (Game1.IsMasterGame || this.IsTemporary)
			{
				this._mapSeatsDirty = true;
			}
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x0009C394 File Offset: 0x0009A594
		public static string GetAddedMapOverrideTilesheetId(string overrideKey, string tilesheetId)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted("zzzzz");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(overrideKey);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(tilesheetId);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x0009C3E5 File Offset: 0x0009A5E5
		public virtual bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			return true;
		}

		// Token: 0x06000E9F RID: 3743 RVA: 0x0009C3E8 File Offset: 0x0009A5E8
		public bool hasActiveFireplace()
		{
			for (int i = 0; i < this.furniture.Count; i++)
			{
				if (this.furniture[i].furniture_type.Value == 14 && this.furniture[i].isOn.Value)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x0009C440 File Offset: 0x0009A640
		public void ApplyMapOverride(string map_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (this._appliedMapOverrides.Contains(map_name))
			{
				return;
			}
			Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
			this.ApplyMapOverride(override_map, map_name, source_rect, destination_rect, null);
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x0009C484 File Offset: 0x0009A684
		public void ApplyMapOverride(string map_name, string override_key_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (this._appliedMapOverrides.Contains(override_key_name))
			{
				return;
			}
			Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
			this.ApplyMapOverride(override_map, override_key_name, source_rect, destination_rect, null);
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x0009C4C8 File Offset: 0x0009A6C8
		public virtual void UpdateMapSeats()
		{
			this._mapSeatsDirty = false;
			if (Game1.IsMasterGame || this.IsTemporary)
			{
				Dictionary<string, string> base_tilesheet_paths = new Dictionary<string, string>();
				Dictionary<string, string> chair_tile_data = DataLoader.ChairTiles(Game1.content);
				this.mapSeats.Clear();
				Layer buildings_layer = this.map.GetLayer("Buildings");
				if (buildings_layer != null)
				{
					for (int x = 0; x < buildings_layer.LayerWidth; x++)
					{
						for (int y = 0; y < buildings_layer.LayerHeight; y++)
						{
							Tile tile = buildings_layer.Tiles[x, y];
							if (tile != null)
							{
								string path = Path.GetFileNameWithoutExtension(tile.TileSheet.ImageSource);
								string overridePath;
								if (base_tilesheet_paths.TryGetValue(path, out overridePath))
								{
									path = overridePath;
								}
								else
								{
									if (path.StartsWith("summer_") || path.StartsWith("winter_") || path.StartsWith("fall_"))
									{
										path = "spring_" + path.Substring(path.IndexOf('_') + 1);
									}
									base_tilesheet_paths[path] = path;
								}
								int tiles_per_row = tile.TileSheet.SheetWidth;
								int tile_x = tile.TileIndex % tiles_per_row;
								int tile_y = tile.TileIndex / tiles_per_row;
								string key = string.Concat(new string[]
								{
									path,
									"/",
									tile_x.ToString(),
									"/",
									tile_y.ToString()
								});
								string data;
								if (chair_tile_data.TryGetValue(key, out data))
								{
									MapSeat seat = MapSeat.FromData(data, x, y);
									if (seat != null)
									{
										this.mapSeats.Add(seat);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x0009C668 File Offset: 0x0009A868
		public virtual void SortLayers()
		{
			this.backgroundLayers.Clear();
			this.buildingLayers.Clear();
			this.frontLayers.Clear();
			this.alwaysFrontLayers.Clear();
			Dictionary<string, List<KeyValuePair<Layer, int>>> layerNameLookup = new Dictionary<string, List<KeyValuePair<Layer, int>>>();
			layerNameLookup["Back"] = this.backgroundLayers;
			layerNameLookup["Buildings"] = this.buildingLayers;
			layerNameLookup["Front"] = this.frontLayers;
			layerNameLookup["AlwaysFront"] = this.alwaysFrontLayers;
			foreach (Layer layer in this.map.Layers)
			{
				foreach (string key in layerNameLookup.Keys)
				{
					if (layer.Id.StartsWith(key))
					{
						int sortIndex = 0;
						string sortString = layer.Id.Substring(key.Length);
						if (sortString.Length <= 0 || int.TryParse(sortString, out sortIndex))
						{
							layerNameLookup[key].Add(new KeyValuePair<Layer, int>(layer, sortIndex));
							break;
						}
					}
				}
			}
			using (Dictionary<string, List<KeyValuePair<Layer, int>>>.ValueCollection.Enumerator enumerator3 = layerNameLookup.Values.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					enumerator3.Current.Sort((KeyValuePair<Layer, int> a, KeyValuePair<Layer, int> b) => a.Value.CompareTo(b.Value));
				}
			}
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x0009C81C File Offset: 0x0009AA1C
		public virtual void OnMapLoad(Map map)
		{
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x0009C820 File Offset: 0x0009AA20
		public void loadMap(string mapPath, bool force_reload = false)
		{
			if (force_reload)
			{
				LocalizedContentManager loader = Program.gamePtr.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
				this.map = loader.Load<Map>(mapPath);
				loader.Unload();
				this.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
			}
			else if (!this.ApplyCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps, mapPath))
			{
				this.map = this.getMapLoader().Load<Map>(mapPath);
			}
			this.loadedMapPath = mapPath;
			this.OnMapLoad(this.map);
			this.SortLayers();
			if (this.map.Properties.ContainsKey("Outdoors"))
			{
				this.isOutdoors.Value = true;
			}
			if (this.map.Properties.ContainsKey("IsFarm"))
			{
				this.isFarm.Value = true;
			}
			if (this.map.Properties.ContainsKey("IsGreenhouse"))
			{
				this.isGreenhouse.Value = true;
			}
			if (this.HasMapPropertyWithValue("forceLoadPathLayerLights"))
			{
				this.forceLoadPathLayerLights = true;
			}
			if (this.HasMapPropertyWithValue("TreatAsOutdoors"))
			{
				this.treatAsOutdoors.Value = true;
			}
			this.updateSeasonalTileSheets(this.map);
			this.map.LoadTileSheets(Game1.mapDisplayDevice);
			if (Game1.IsMasterGame || this.IsTemporary)
			{
				this._mapSeatsDirty = true;
			}
			if ((this.isOutdoors.Value || this.HasMapPropertyWithValue("indoorWater") || this is Sewer || this is Submarine) && !(this is Desert))
			{
				this.waterTiles = new WaterTiles(this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
				bool foundAnyWater = false;
				for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
					{
						string water_property = this.doesTileHaveProperty(x, y, "Water", "Back", false);
						if (water_property != null)
						{
							foundAnyWater = true;
							if (water_property == "I")
							{
								this.waterTiles.waterTiles[x, y] = new WaterTiles.WaterTileData(true, false);
							}
							else
							{
								this.waterTiles[x, y] = true;
							}
						}
					}
				}
				if (!foundAnyWater)
				{
					this.waterTiles = null;
				}
			}
			if (this.isOutdoors.Value)
			{
				this.critters = new List<Critter>();
			}
			this.loadLights();
		}

		// Token: 0x06000EA6 RID: 3750 RVA: 0x0009CAAC File Offset: 0x0009ACAC
		public virtual void HandleGrassGrowth(int dayOfMonth)
		{
			if (dayOfMonth == 1)
			{
				if (this is Farm || this.HasMapPropertyWithValue("ClearEmptyDirtOnNewMonth"))
				{
					this.terrainFeatures.RemoveWhere(delegate(KeyValuePair<Vector2, TerrainFeature> pair)
					{
						HoeDirt dirt = pair.Value as HoeDirt;
						return dirt != null && dirt.crop == null && Game1.random.NextDouble() < 0.8;
					});
				}
				if (this is Farm || this.HasMapPropertyWithValue("SpawnDebrisOnNewMonth"))
				{
					this.spawnWeedsAndStones(20, false, false);
				}
				if (Game1.IsSpring && Game1.stats.DaysPlayed > 1U)
				{
					if (this is Farm || this.HasMapPropertyWithValue("SpawnDebrisOnNewYear"))
					{
						this.spawnWeedsAndStones(40, false, false);
						this.spawnWeedsAndStones(40, true, false);
					}
					if (this is Farm || this.HasMapPropertyWithValue("SpawnRandomGrassOnNewYear"))
					{
						for (int i = 0; i < 15; i++)
						{
							int xCoord = Game1.random.Next(this.map.DisplayWidth / 64);
							int yCoord = Game1.random.Next(this.map.DisplayHeight / 64);
							Vector2 location = new Vector2((float)xCoord, (float)yCoord);
							Object o;
							this.objects.TryGetValue(location, out o);
							if (o == null && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back", false) != null && !this.IsNoSpawnTile(location, "All", false) && this.isTileLocationOpen(new Location(xCoord, yCoord)) && !this.IsTileOccupiedBy(location, CollisionMask.All, CollisionMask.None, false) && !this.isWaterTile(xCoord, yCoord))
							{
								int grassType = 1;
								if (Game1.GetFarmTypeID() == "MeadowlandsFarm" && Game1.random.NextDouble() < 0.2)
								{
									grassType = 7;
								}
								this.terrainFeatures.Add(location, new Grass(grassType, 4));
							}
						}
						this.growWeedGrass(40);
					}
					if (this.HasMapPropertyWithValue("SpawnGrassFromPathsOnNewYear"))
					{
						Layer paths = this.map.GetLayer("Paths");
						if (paths != null)
						{
							for (int x = 0; x < paths.LayerWidth; x++)
							{
								for (int y = 0; y < paths.LayerHeight; y++)
								{
									Vector2 location2 = new Vector2((float)x, (float)y);
									Object o2;
									this.objects.TryGetValue(location2, out o2);
									if (o2 == null && this.getTileIndexAt(x, y, "Paths", null) == 22 && this.isTileLocationOpen(location2) && !this.IsTileOccupiedBy(location2, CollisionMask.All, CollisionMask.None, false))
									{
										this.terrainFeatures.Add(location2, new Grass(1, 4));
									}
								}
							}
						}
					}
				}
			}
			if ((this is Farm || this.HasMapPropertyWithValue("EnableGrassSpread")) && (!this.IsWinterHere() || this.HasMapPropertyWithValue("AllowGrassGrowInWinter")))
			{
				this.growWeedGrass(1);
			}
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x0009CD65 File Offset: 0x0009AF65
		public void reloadMap()
		{
			if (this.mapPath.Value != null)
			{
				this.loadMap(this.mapPath.Value, false);
			}
			else
			{
				this.map = null;
			}
			this.loadedMapPath = this.mapPath.Value;
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x0009CDA0 File Offset: 0x0009AFA0
		public virtual bool canSlimeMateHere()
		{
			return true;
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x0009CDA3 File Offset: 0x0009AFA3
		public virtual bool canSlimeHatchHere()
		{
			return true;
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x0009CDA6 File Offset: 0x0009AFA6
		public void addCharacter(NPC character)
		{
			this.characters.Add(character);
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x0009CDB4 File Offset: 0x0009AFB4
		public static Microsoft.Xna.Framework.Rectangle getSourceRectForObject(int tileIndex)
		{
			return new Microsoft.Xna.Framework.Rectangle(tileIndex * 16 % Game1.objectSpriteSheet.Width, tileIndex * 16 / Game1.objectSpriteSheet.Width * 16, 16, 16);
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x0009CDE0 File Offset: 0x0009AFE0
		public Warp isCollidingWithWarp(Microsoft.Xna.Framework.Rectangle position, Character character)
		{
			if (this.ignoreWarps)
			{
				return null;
			}
			foreach (Warp w in this.warps)
			{
				if ((character is NPC || !w.npcOnly.Value) && (w.X == (int)Math.Floor((double)position.Left / 64.0) || w.X == (int)Math.Floor((double)position.Right / 64.0)) && (w.Y == (int)Math.Floor((double)position.Top / 64.0) || w.Y == (int)Math.Floor((double)position.Bottom / 64.0)))
				{
					string targetName = w.TargetName;
					if (!(targetName == "BoatTunnel"))
					{
						if (targetName == "VolcanoEntrance")
						{
							return new Warp(w.X, w.Y, VolcanoDungeon.GetLevelName(0), w.TargetX, w.TargetY, false, false);
						}
					}
					else if (character is NPC)
					{
						return new Warp(w.X, w.Y, "IslandSouth", 17, 43, false, false);
					}
					return w;
				}
			}
			return null;
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x0009CF5C File Offset: 0x0009B15C
		public Warp isCollidingWithWarpOrDoor(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			Warp w = this.isCollidingWithWarp(position, character);
			if (w == null)
			{
				w = this.isCollidingWithDoors(position, character);
			}
			return w;
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x0009CF80 File Offset: 0x0009B180
		public virtual Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 v = Utility.getCornersOfThisRectangle(ref position, i);
				Point rectangleCorner = new Point((int)v.X / 64, (int)v.Y / 64);
				foreach (KeyValuePair<Point, string> pair in this.doors.Pairs)
				{
					Point door = pair.Key;
					if (rectangleCorner == door)
					{
						Warp warp = this.getWarpFromDoor(door, character);
						if (warp != null)
						{
							return warp;
						}
					}
				}
				foreach (Building building in this.buildings)
				{
					if (building.HasIndoors())
					{
						Point point = building.getPointForHumanDoor();
						if (rectangleCorner == point)
						{
							Warp warp2 = this.getWarpFromDoor(point, character);
							if (warp2 != null)
							{
								return warp2;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x0009D0AC File Offset: 0x0009B2AC
		public virtual Warp getWarpFromDoor(Point door, Character character = null)
		{
			foreach (Building building in this.buildings)
			{
				if (door == building.getPointForHumanDoor())
				{
					GameLocation interior = building.GetIndoors();
					if (interior != null)
					{
						return new Warp(door.X, door.Y, interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, false, false);
					}
				}
			}
			string[] split = this.GetTilePropertySplitBySpaces("Action", "Buildings", door.X, door.Y);
			string propertyName = ArgUtility.Get(split, 0, "", true);
			if (propertyName == null)
			{
				goto IL_2BF;
			}
			int length = propertyName.Length;
			if (length != 4)
			{
				switch (length)
				{
				case 14:
				{
					char c = propertyName[4];
					if (c != 'B')
					{
						if (c != 'M')
						{
							if (c != 'e')
							{
								goto IL_2BF;
							}
							if (!(propertyName == "LockedDoorWarp"))
							{
								goto IL_2BF;
							}
						}
						else if (!(propertyName == "WarpMensLocker"))
						{
							goto IL_2BF;
						}
					}
					else
					{
						if (!(propertyName == "WarpBoatTunnel"))
						{
							goto IL_2BF;
						}
						if (!(character is NPC))
						{
							return new Warp(door.X, door.Y, "BoatTunnel", 6, 11, false, false);
						}
						return new Warp(door.X, door.Y, "IslandSouth", 17, 43, false, false);
					}
					break;
				}
				case 15:
				case 18:
					goto IL_2BF;
				case 16:
					if (!(propertyName == "WarpWomensLocker"))
					{
						goto IL_2BF;
					}
					break;
				case 17:
					if (!(propertyName == "Warp_Sunroom_Door"))
					{
						goto IL_2BF;
					}
					return new Warp(door.X, door.Y, "Sunroom", 5, 13, false, false);
				case 19:
					if (!(propertyName == "WarpCommunityCenter"))
					{
						goto IL_2BF;
					}
					return new Warp(door.X, door.Y, "CommunityCenter", 32, 23, false, false);
				default:
					goto IL_2BF;
				}
			}
			else if (!(propertyName == "Warp"))
			{
				goto IL_2BF;
			}
			IL_229:
			Point tile;
			string error;
			string locationName;
			if (!ArgUtility.TryGetPoint(split, 1, out tile, out error, "Point tile") || !ArgUtility.TryGet(split, 3, out locationName, out error, true, "string locationName"))
			{
				this.LogTileActionError(split, door.X, door.Y, error);
				return null;
			}
			if (!(locationName == "BoatTunnel") || !(character is NPC))
			{
				return new Warp(door.X, door.Y, locationName, tile.X, tile.Y, false, false);
			}
			return new Warp(door.X, door.Y, "IslandSouth", 17, 43, false, false);
			IL_2BF:
			if (propertyName.Contains("Warp"))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Door in ");
				defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Point>(door);
				defaultInterpolatedStringHandler.AppendLiteral(") has unknown warp property '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", split));
				defaultInterpolatedStringHandler.AppendLiteral("', parsing with legacy logic.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				goto IL_229;
			}
			return null;
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x0009D414 File Offset: 0x0009B614
		public Warp GetFirstPlayerWarp()
		{
			Warp warpIgnoringGender = null;
			foreach (Warp warp in this.warps)
			{
				if (!warp.npcOnly.Value)
				{
					Gender gender;
					if (!WarpPathfindingCache.GenderRestrictions.TryGetValue(warp.TargetName, out gender) || gender == Game1.player.Gender)
					{
						return warp;
					}
					if (warpIgnoringGender == null)
					{
						warpIgnoringGender = warp;
					}
				}
			}
			return warpIgnoringGender ?? this.warps.FirstOrDefault<Warp>();
		}

		// Token: 0x06000EB1 RID: 3761 RVA: 0x0009D4B0 File Offset: 0x0009B6B0
		public void addResourceClumpAndRemoveUnderlyingTerrain(int resourceClumpIndex, int width, int height, Vector2 tile)
		{
			this.removeObjectsAndSpawned((int)tile.X, (int)tile.Y, width, height);
			this.resourceClumps.Add(new ResourceClump(resourceClumpIndex, width, height, tile, null, null));
		}

		// Token: 0x06000EB2 RID: 3762 RVA: 0x0009D4F4 File Offset: 0x0009B6F4
		public virtual bool canFishHere()
		{
			return true;
		}

		// Token: 0x06000EB3 RID: 3763 RVA: 0x0009D4F8 File Offset: 0x0009B6F8
		public virtual bool CanWakeUpHere(Farmer who, Point? tile = null)
		{
			Point wakeUpTile = tile ?? who.lastSleepPoint.Value;
			bool allowWakeUpWithoutBed;
			return BedFurniture.IsBedHere(this, wakeUpTile.X, wakeUpTile.Y) || who.sleptInTemporaryBed.Value || this is IslandFarmHouse || (this.TryGetMapPropertyAs("AllowWakeUpWithoutBed", out allowWakeUpWithoutBed, false) && allowWakeUpWithoutBed);
		}

		// Token: 0x06000EB4 RID: 3764 RVA: 0x0009D560 File Offset: 0x0009B760
		public virtual bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			Vector2 tile = new Vector2((float)tileX, (float)tileY);
			Building buildingAt = this.getBuildingAt(tile);
			return (buildingAt != null && buildingAt.CanRefillWateringCan()) || this.isWaterTile(tileX, tileY) || this.doesTileHaveProperty(tileX, tileY, "WaterSource", "Back", false) != null || (!this.isOutdoors.Value && this.doesTileHaveProperty(tileX, tileY, "Action", "Buildings", false) == "kitchen" && (this.getTileIndexAt(tileX, tileY, "Buildings", "untitled tile sheet") == 172 || this.getTileIndexAt(tileX, tileY, "Buildings", "untitled tile sheet") == 257));
		}

		// Token: 0x06000EB5 RID: 3765 RVA: 0x0009D614 File Offset: 0x0009B814
		public virtual bool isTileBuildingFishable(int tileX, int tileY)
		{
			Vector2 tile = new Vector2((float)tileX, (float)tileY);
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.isTileFishable(tile))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000EB6 RID: 3766 RVA: 0x0009D67C File Offset: 0x0009B87C
		public virtual bool isTileFishable(int tileX, int tileY)
		{
			return this.isTileBuildingFishable(tileX, tileY) || (this.isWaterTile(tileX, tileY) && this.doesTileHaveProperty(tileX, tileY, "NoFishing", "Back", false) == null && !this.hasTileAt(tileX, tileY, "Buildings", null)) || this.doesTileHaveProperty(tileX, tileY, "Water", "Buildings", false) != null;
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x0009D6DC File Offset: 0x0009B8DC
		public bool isFarmerCollidingWithAnyCharacter()
		{
			if (this.characters.Count > 0)
			{
				Microsoft.Xna.Framework.Rectangle playerBounds = Game1.player.GetBoundingBox();
				foreach (NPC i in this.characters)
				{
					if (i != null && playerBounds.Intersects(i.GetBoundingBox()))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x0009D75C File Offset: 0x0009B95C
		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, Character character)
		{
			return this.isCollidingPosition(position, viewport, character is Farmer, 0, false, character, false, false, false, false);
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x0009D784 File Offset: 0x0009B984
		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return this.isCollidingPosition(position, viewport, character is Farmer, damagesFarmer, glider, character, false, false, false, false);
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x0009D7AD File Offset: 0x0009B9AD
		protected bool _TestCornersWorld(int top, int bottom, int left, int right, Func<int, int, bool> action)
		{
			return action(right, top) || action(right, bottom) || action(left, top) || action(left, bottom);
		}

		// Token: 0x06000EBB RID: 3771 RVA: 0x0009D7E8 File Offset: 0x0009B9E8
		protected bool _TestCornersTiles(Vector2 top_right, Vector2 top_left, Vector2 bottom_right, Vector2 bottom_left, Vector2 top_mid, Vector2 bottom_mid, Vector2? player_top_right, Vector2? player_top_left, Vector2? player_bottom_right, Vector2? player_bottom_left, Vector2? player_top_mid, Vector2? player_bottom_mid, bool bigger_than_tile, Func<Vector2, bool> action)
		{
			this._visitedCollisionTiles.Clear();
			if (player_top_right != top_right && this._visitedCollisionTiles.Add(top_right) && action(top_right))
			{
				return true;
			}
			if (player_top_left != top_left && this._visitedCollisionTiles.Add(top_left) && action(top_left))
			{
				return true;
			}
			if (bottom_left != player_bottom_left && this._visitedCollisionTiles.Add(bottom_left) && action(bottom_left))
			{
				return true;
			}
			if (bottom_right != player_bottom_right && this._visitedCollisionTiles.Add(bottom_right) && action(bottom_right))
			{
				return true;
			}
			if (bigger_than_tile)
			{
				if (player_top_mid != top_mid && this._visitedCollisionTiles.Add(top_mid) && action(top_mid))
				{
					return true;
				}
				if (player_bottom_mid != bottom_mid && this._visitedCollisionTiles.Add(bottom_mid) && action(bottom_mid))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000EBC RID: 3772 RVA: 0x0009D96C File Offset: 0x0009BB6C
		public Furniture GetFurnitureAt(Vector2 tile_position)
		{
			Point position = default(Point);
			position.X = (int)((float)((int)tile_position.X) + 0.5f) * 64;
			position.Y = (int)((float)((int)tile_position.Y) + 0.5f) * 64;
			foreach (Furniture f in this.furniture)
			{
				if (!f.isPassable() && f.GetBoundingBox().Contains(position))
				{
					return f;
				}
			}
			foreach (Furniture f2 in this.furniture)
			{
				if (f2.isPassable() && f2.GetBoundingBox().Contains(position))
				{
					return f2;
				}
			}
			return null;
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x0009DA74 File Offset: 0x0009BC74
		public virtual Microsoft.Xna.Framework.Rectangle GetBuildableRectangle()
		{
			if (this._buildableTileRect == null)
			{
				Microsoft.Xna.Framework.Rectangle area;
				this._buildableTileRect = new Microsoft.Xna.Framework.Rectangle?(this.TryGetMapPropertyAs("ValidBuildRect", out area, false) ? area : Microsoft.Xna.Framework.Rectangle.Empty);
				this._looserBuildRestrictions = this.HasMapPropertyWithValue("LooserBuildRestrictions");
			}
			return this._buildableTileRect.Value;
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x0009DAD0 File Offset: 0x0009BCD0
		public virtual bool IsBuildableLocation()
		{
			if (this.HasMapPropertyWithValue("CanBuildHere"))
			{
				if (!Game1.multiplayer.isAlwaysActiveLocation(this))
				{
					if (!this.showedBuildableButNotAlwaysActiveWarning)
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(107, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Location ");
						defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral(" has the CanBuildHere map property set, but its ");
						defaultInterpolatedStringHandler.AppendFormatted("AlwaysActive");
						defaultInterpolatedStringHandler.AppendLiteral(" option is disabled, so building is disabled here.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						this.showedBuildableButNotAlwaysActiveWarning = true;
					}
					return false;
				}
				string conditions = this.getMapProperty("BuildConditions");
				if (string.IsNullOrEmpty(conditions) || GameStateQuery.CheckConditions(conditions, this, null, null, null, null, null))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x0009DB88 File Offset: 0x0009BD88
		public virtual bool IsOutOfBounds(Microsoft.Xna.Framework.Rectangle pixelPosition)
		{
			if (pixelPosition.Right < 0 || pixelPosition.Bottom < 0)
			{
				return true;
			}
			Layer layer = this.map.Layers[0];
			return pixelPosition.X > layer.DisplayWidth || pixelPosition.Top > layer.DisplayHeight;
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x0009DBDC File Offset: 0x0009BDDC
		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			bool is_event_up = Game1.eventUp;
			if (is_event_up && Game1.CurrentEvent != null && !Game1.CurrentEvent.ignoreObjectCollisions)
			{
				is_event_up = false;
			}
			this.updateMap();
			if (this.IsOutOfBounds(position))
			{
				if (isFarmer && Game1.eventUp)
				{
					Event @event = this.currentEvent;
					bool? flag = (@event != null) ? new bool?(@event.isFestival) : null;
					if (flag != null && flag.GetValueOrDefault() && this.currentEvent.checkForCollision(position, (character as Farmer) ?? Game1.player))
					{
						return true;
					}
				}
				return false;
			}
			if (character == null && !ignoreCharacterRequirement)
			{
				return true;
			}
			Vector2 nextTopRight = new Vector2((float)(position.Right / 64), (float)(position.Top / 64));
			Vector2 nextTopLeft = new Vector2((float)(position.Left / 64), (float)(position.Top / 64));
			Vector2 nextBottomRight = new Vector2((float)(position.Right / 64), (float)(position.Bottom / 64));
			Vector2 nextBottomLeft = new Vector2((float)(position.Left / 64), (float)(position.Bottom / 64));
			bool nextLargerThanTile = position.Width > 64;
			Vector2 nextBottomMid = new Vector2((float)(position.Center.X / 64), (float)(position.Bottom / 64));
			Vector2 nextTopMid = new Vector2((float)(position.Center.X / 64), (float)(position.Top / 64));
			BoundingBoxGroup passableTiles = null;
			Farmer farmer = character as Farmer;
			Microsoft.Xna.Framework.Rectangle? currentBounds;
			if (farmer != null)
			{
				isFarmer = true;
				currentBounds = new Microsoft.Xna.Framework.Rectangle?(farmer.GetBoundingBox());
				passableTiles = farmer.TemporaryPassableTiles;
			}
			else
			{
				farmer = null;
				isFarmer = false;
				currentBounds = null;
			}
			Vector2? currentTopRight = null;
			Vector2? currentTopLeft = null;
			Vector2? currentBottomRight = null;
			Vector2? currentBottomLeft = null;
			Vector2? currentBottomMid = null;
			Vector2? currentTopMid = null;
			if (currentBounds != null)
			{
				currentTopRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)(currentBounds.Value.Top / 64)));
				currentTopLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)(currentBounds.Value.Top / 64)));
				currentBottomRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
				currentBottomLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
				currentBottomMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
				currentTopMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)(currentBounds.Value.Top / 64)));
			}
			Farmer farmer2 = farmer;
			if (((farmer2 != null) ? farmer2.bridge : null) != null && farmer.onBridge.Value && position.Right >= farmer.bridge.bridgeBounds.X && position.Left <= farmer.bridge.bridgeBounds.Right)
			{
				return this._TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, (int x, int y) => y > farmer.bridge.bridgeBounds.Bottom || y < farmer.bridge.bridgeBounds.Top);
			}
			if (!glider)
			{
				if (character != null && this.animals.FieldDict.Count > 0 && !(character is FarmAnimal))
				{
					foreach (FarmAnimal animal in this.animals.Values)
					{
						Microsoft.Xna.Framework.Rectangle animalBounds = animal.GetBoundingBox();
						if (position.Intersects(animalBounds) && (currentBounds == null || !currentBounds.Value.Intersects(animalBounds)) && (passableTiles == null || !passableTiles.Intersects(position)))
						{
							if (!skipCollisionEffects)
							{
								animal.farmerPushing();
							}
							return true;
						}
					}
				}
				if (this.buildings.Count > 0)
				{
					foreach (Building b in this.buildings)
					{
						if (b.intersects(position) && (currentBounds == null || !b.intersects(currentBounds.Value)))
						{
							if (!(character is FarmAnimal) && !(character is JunimoHarvester))
							{
								if (!(character is NPC))
								{
									return true;
								}
								Microsoft.Xna.Framework.Rectangle door = b.getRectForHumanDoor();
								door.Height += 64;
								if (!door.Contains(position))
								{
									return true;
								}
							}
							else
							{
								Microsoft.Xna.Framework.Rectangle door2 = b.getRectForAnimalDoor();
								door2.Height += 64;
								if (!door2.Contains(position))
								{
									return true;
								}
								FarmAnimal animal2 = character as FarmAnimal;
								if (animal2 != null && !animal2.CanLiveIn(b))
								{
									return true;
								}
							}
						}
					}
				}
				if (this.resourceClumps.Count > 0)
				{
					foreach (ResourceClump resourceClump in this.resourceClumps)
					{
						Microsoft.Xna.Framework.Rectangle bounds = resourceClump.getBoundingBox();
						if (bounds.Intersects(position) && (currentBounds == null || !bounds.Intersects(currentBounds.Value)))
						{
							return true;
						}
					}
				}
				if (!is_event_up && this.furniture.Count > 0)
				{
					foreach (Furniture f in this.furniture)
					{
						if (f.furniture_type.Value != 12 && f.IntersectsForCollision(position) && (currentBounds == null || !f.IntersectsForCollision(currentBounds.Value)))
						{
							return true;
						}
					}
				}
				NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
				if (netCollection != null && netCollection.Count > 0)
				{
					foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
					{
						Microsoft.Xna.Framework.Rectangle bounds2 = largeTerrainFeature.getBoundingBox();
						if (bounds2.Intersects(position) && (currentBounds == null || !bounds2.Intersects(currentBounds.Value)))
						{
							return true;
						}
					}
				}
			}
			if (!glider)
			{
				if ((!is_event_up || (character != null && !isFarmer && (!pathfinding || !character.willDestroyObjectsUnderfoot))) && this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 corner)
				{
					Object o;
					if (this.objects.TryGetValue(corner, out o) && o != null)
					{
						if (o.isPassable())
						{
							return false;
						}
						Microsoft.Xna.Framework.Rectangle bounds3 = o.GetBoundingBox();
						if (bounds3.Intersects(position) && (character == null || character.collideWith(o)))
						{
							return (!(character is FarmAnimal) || !o.isAnimalProduct()) && (passableTiles == null || !passableTiles.Intersects(bounds3));
						}
					}
					return false;
				}))
				{
					return true;
				}
				this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, null, null, null, null, null, null, nextLargerThanTile, delegate(Vector2 corner)
				{
					TerrainFeature feature;
					if (this.terrainFeatures.TryGetValue(corner, out feature) && feature != null && feature.getBoundingBox().Intersects(position) && !pathfinding && character != null && !skipCollisionEffects)
					{
						feature.doCollisionAction(position, (int)((float)character.speed + character.addedSpeed), corner, character);
					}
					return false;
				});
				if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 corner)
				{
					TerrainFeature feature;
					return this.terrainFeatures.TryGetValue(corner, out feature) && feature != null && feature.getBoundingBox().Intersects(position) && !feature.isPassable(character);
				}))
				{
					return true;
				}
			}
			if (character != null && character.hasSpecialCollisionRules() && (character.isColliding(this, nextTopRight) || character.isColliding(this, nextTopLeft) || character.isColliding(this, nextBottomRight) || character.isColliding(this, nextBottomLeft)))
			{
				return true;
			}
			if (((isFarmer && (this.currentEvent == null || this.currentEvent.playerControlSequence)) || (character != null && character.collidesWithOtherCharacters.Value)) && !pathfinding)
			{
				for (int i = this.characters.Count - 1; i >= 0; i--)
				{
					NPC other = this.characters[i];
					if (other != null && (character == null || !character.Equals(other)))
					{
						Microsoft.Xna.Framework.Rectangle bounding_box = other.GetBoundingBox();
						if (other.layingDown)
						{
							bounding_box.Y -= 64;
							bounding_box.Height += 64;
						}
						if (bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible && !skipCollisionEffects)
						{
							other.behaviorOnFarmerPushing();
						}
						if (isFarmer)
						{
							if (!is_event_up && !other.farmerPassesThrough && bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible && Game1.player.TemporaryPassableTiles.IsEmpty() && (!other.IsMonster || (!((Monster)other).isGlider.Value && !Game1.player.GetBoundingBox().Intersects(other.GetBoundingBox()))) && !other.IsInvisible && !Game1.player.GetBoundingBox().Intersects(bounding_box))
							{
								return true;
							}
						}
						else if (bounding_box.Intersects(position))
						{
							return true;
						}
					}
				}
			}
			Layer back_layer = this.map.RequireLayer("Back");
			Layer buildings_layer = this.map.RequireLayer("Buildings");
			if (isFarmer)
			{
				Event event2 = this.currentEvent;
				if (event2 != null && event2.checkForCollision(position, (character as Farmer) ?? Game1.player))
				{
					return true;
				}
			}
			else
			{
				if (!pathfinding && !(character is Monster) && damagesFarmer == 0 && !glider)
				{
					foreach (Farmer otherFarmer in this.farmers)
					{
						if (position.Intersects(otherFarmer.GetBoundingBox()))
						{
							return true;
						}
					}
				}
				if ((this.isFarm.Value || MineShaft.IsGeneratedLevel(this) || this is IslandLocation) && character != null && !character.Name.Contains("NPC") && !character.EventActor && !glider)
				{
					Tile t;
					if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 tile)
					{
						t = back_layer.Tiles[(int)tile.X, (int)tile.Y];
						return t != null && t.Properties.ContainsKey("NPCBarrier");
					}))
					{
						return true;
					}
				}
				if (glider && !projectile)
				{
					return false;
				}
			}
			if (!isFarmer || !Game1.player.isRafting)
			{
				Tile t;
				if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 tile)
				{
					t = back_layer.Tiles[(int)tile.X, (int)tile.Y];
					return t != null && t.Properties.ContainsKey("TemporaryBarrier");
				}))
				{
					return true;
				}
			}
			if (isFarmer && Game1.player.isRafting)
			{
				Tile t;
				return this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 tile)
				{
					Tile t = back_layer.Tiles[(int)tile.X, (int)tile.Y];
					t = t;
					if (!(((t != null) ? new bool?(t.TileIndexProperties.ContainsKey("Water")) : null) ?? false))
					{
						int tileX = (int)tile.X;
						int tileY = (int)tile.Y;
						if (this.IsTileBlockedBy(new Vector2((float)tileX, (float)tileY), CollisionMask.All, CollisionMask.None, false))
						{
							Game1.player.isRafting = false;
							Game1.player.Position = new Vector2((float)(tileX * 64), (float)(tileY * 64 - 32));
							Game1.player.setTrajectory(0, 0);
						}
						return true;
					}
					return false;
				});
			}
			FarmAnimal animal3 = character as FarmAnimal;
			if ((animal3 == null || !animal3.IsActuallySwimming()) && this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 tile)
			{
				Tile tmpTile = back_layer.Tiles[(int)tile.X, (int)tile.Y];
				if (tmpTile != null)
				{
					bool blocked = tmpTile.TileIndexProperties.ContainsKey("Passable");
					if (!blocked)
					{
						blocked = tmpTile.Properties.ContainsKey("Passable");
					}
					if (blocked)
					{
						return passableTiles == null || !passableTiles.Contains((int)tile.X, (int)tile.Y);
					}
				}
				return false;
			}))
			{
				return true;
			}
			if (character == null || character.shouldCollideWithBuildingLayer(this))
			{
				Tile tmp;
				if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 tile)
				{
					tmp = buildings_layer.Tiles[(int)tile.X, (int)tile.Y];
					if (tmp != null)
					{
						if (projectile && this is VolcanoDungeon)
						{
							Tile back_tile = back_layer.Tiles[(int)tile.X, (int)tile.Y];
							if (back_tile != null)
							{
								if (back_tile.TileIndexProperties.ContainsKey("Water"))
								{
									return false;
								}
								if (back_tile.Properties.ContainsKey("Water"))
								{
									return false;
								}
							}
						}
						bool flag3;
						if (!tmp.TileIndexProperties.ContainsKey("Shadow") && !tmp.TileIndexProperties.ContainsKey("Passable") && !tmp.Properties.ContainsKey("Passable") && (!projectile || (!tmp.TileIndexProperties.ContainsKey("ProjectilePassable") && !tmp.Properties.ContainsKey("ProjectilePassable"))))
						{
							if (!isFarmer)
							{
								if (!tmp.TileIndexProperties.ContainsKey("NPCPassable") && !tmp.Properties.ContainsKey("NPCPassable"))
								{
									Character character3 = character;
									bool? flag2 = (character3 != null) ? new bool?(character3.canPassThroughActionTiles()) : null;
									flag3 = (flag2 != null && flag2.GetValueOrDefault() && tmp.Properties.ContainsKey("Action"));
								}
								else
								{
									flag3 = true;
								}
							}
							else
							{
								flag3 = false;
							}
						}
						else
						{
							flag3 = true;
						}
						if (!flag3)
						{
							return passableTiles == null || !passableTiles.Contains((int)tile.X, (int)tile.Y);
						}
					}
					return false;
				}))
				{
					return true;
				}
			}
			if (!isFarmer)
			{
				Character character2 = character;
				if (((character2 != null) ? character2.controller : null) != null && !skipCollisionEffects)
				{
					Point tileLocation = new Point(position.Center.X / 64, position.Bottom / 64);
					Tile tile2 = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
					if (tile2 != null && tile2.Properties.ContainsKey("Action"))
					{
						this.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(this));
					}
					else
					{
						tileLocation = new Point(position.Center.X / 64, position.Top / 64);
						tile2 = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
						if (tile2 != null && tile2.Properties.ContainsKey("Action"))
						{
							this.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(this));
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x0009EAF8 File Offset: 0x0009CCF8
		public bool isTilePassable(Vector2 tileLocation)
		{
			Tile backTile = this.map.RequireLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y];
			if (backTile != null && backTile.TileIndexProperties.ContainsKey("Passable"))
			{
				return false;
			}
			Tile buildingsTile = this.map.RequireLayer("Buildings").Tiles[(int)tileLocation.X, (int)tileLocation.Y];
			return buildingsTile == null || buildingsTile.TileIndexProperties.ContainsKey("Shadow") || buildingsTile.TileIndexProperties.ContainsKey("Passable");
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x0009EB98 File Offset: 0x0009CD98
		public bool isTilePassable(Location tileLocation, xTile.Dimensions.Rectangle viewport)
		{
			return this.isTilePassable(new Vector2((float)tileLocation.X, (float)tileLocation.Y));
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x0009EBB3 File Offset: 0x0009CDB3
		public bool isPointPassable(Location location, xTile.Dimensions.Rectangle viewport)
		{
			return this.isTilePassable(new Location(location.X / 64, location.Y / 64), viewport);
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x0009EBD4 File Offset: 0x0009CDD4
		public bool isTilePassable(Microsoft.Xna.Framework.Rectangle nextPosition, xTile.Dimensions.Rectangle viewport)
		{
			return this.isPointPassable(new Location(nextPosition.Left, nextPosition.Top), viewport) && this.isPointPassable(new Location(nextPosition.Right, nextPosition.Bottom), viewport) && this.isPointPassable(new Location(nextPosition.Left, nextPosition.Bottom), viewport) && this.isPointPassable(new Location(nextPosition.Right, nextPosition.Top), viewport);
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0009EC54 File Offset: 0x0009CE54
		public bool isTileOnMap(Vector2 position)
		{
			return position.X >= 0f && position.X < (float)this.map.Layers[0].LayerWidth && position.Y >= 0f && position.Y < (float)this.map.Layers[0].LayerHeight;
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0009ECBB File Offset: 0x0009CEBB
		public bool isTileOnMap(Point tile)
		{
			return this.isTileOnMap(tile.X, tile.Y);
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0009ECCF File Offset: 0x0009CECF
		public bool isTileOnMap(int x, int y)
		{
			return x >= 0 && x < this.map.Layers[0].LayerWidth && y >= 0 && y < this.map.Layers[0].LayerHeight;
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x0009ED10 File Offset: 0x0009CF10
		public int numberOfObjectsWithName(string name)
		{
			int number = 0;
			using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator = this.objects.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Name.Equals(name))
					{
						number++;
					}
				}
			}
			return number;
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x0009ED74 File Offset: 0x0009CF74
		public virtual Point getWarpPointTo(string location, Character character = null)
		{
			foreach (Building building in this.buildings)
			{
				if (building.HasIndoorsName(location))
				{
					return building.getPointForHumanDoor();
				}
			}
			foreach (Warp w in this.warps)
			{
				if (w.TargetName.Equals(location))
				{
					return new Point(w.X, w.Y);
				}
				if (w.TargetName.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return new Point(w.X, w.Y);
				}
			}
			foreach (KeyValuePair<Point, string> v in this.doors.Pairs)
			{
				if (v.Value.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return v.Key;
				}
				if (v.Value.Equals(location))
				{
					return v.Key;
				}
			}
			return Point.Zero;
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x0009EF04 File Offset: 0x0009D104
		public Point getWarpPointTarget(Point warpPointLocation, Character character = null)
		{
			foreach (Warp w in this.warps)
			{
				if (w.X == warpPointLocation.X && w.Y == warpPointLocation.Y)
				{
					return new Point(w.TargetX, w.TargetY);
				}
			}
			foreach (KeyValuePair<Point, string> v in this.doors.Pairs)
			{
				if (v.Key.Equals(warpPointLocation))
				{
					string[] action = this.GetTilePropertySplitBySpaces("Action", "Buildings", warpPointLocation.X, warpPointLocation.Y);
					string propertyName = ArgUtility.Get(action, 0, "", true);
					if (propertyName == null)
					{
						goto IL_287;
					}
					int length = propertyName.Length;
					if (length != 4)
					{
						switch (length)
						{
						case 14:
						{
							char c = propertyName[4];
							if (c != 'B')
							{
								if (c != 'M')
								{
									if (c != 'e')
									{
										goto IL_287;
									}
									if (!(propertyName == "LockedDoorWarp"))
									{
										goto IL_287;
									}
								}
								else if (!(propertyName == "WarpMensLocker"))
								{
									goto IL_287;
								}
							}
							else
							{
								if (!(propertyName == "WarpBoatTunnel"))
								{
									goto IL_287;
								}
								return new Point(17, 43);
							}
							break;
						}
						case 15:
						case 18:
							goto IL_287;
						case 16:
							if (!(propertyName == "WarpWomensLocker"))
							{
								goto IL_287;
							}
							break;
						case 17:
							if (!(propertyName == "Warp_Sunroom_Door"))
							{
								goto IL_287;
							}
							return new Point(5, 13);
						case 19:
							if (!(propertyName == "WarpCommunityCenter"))
							{
								goto IL_287;
							}
							return new Point(32, 23);
						default:
							goto IL_287;
						}
					}
					else if (!(propertyName == "Warp"))
					{
						goto IL_287;
					}
					IL_1DA:
					Point tile;
					string error;
					string locationName;
					if (!ArgUtility.TryGetPoint(action, 1, out tile, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out locationName, out error, true, "string locationName"))
					{
						this.LogTileActionError(action, warpPointLocation.X, warpPointLocation.Y, error);
						continue;
					}
					if (!(locationName == "BoatTunnel"))
					{
						if (locationName == "Trailer")
						{
							if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
							{
								return new Point(13, 24);
							}
						}
						return new Point(tile.X, tile.Y);
					}
					return new Point(17, 43);
					IL_287:
					if (propertyName.Contains("Warp"))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Door in ");
						defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral(" (");
						defaultInterpolatedStringHandler.AppendFormatted<Point>(v.Key);
						defaultInterpolatedStringHandler.AppendLiteral(") has unknown warp property '");
						defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", action));
						defaultInterpolatedStringHandler.AppendLiteral("', parsing with legacy logic.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						goto IL_1DA;
					}
				}
			}
			return Point.Zero;
		}

		// Token: 0x06000ECB RID: 3787 RVA: 0x0009F27C File Offset: 0x0009D47C
		public virtual bool HasLocationOverrideDialogue(NPC character)
		{
			return false;
		}

		// Token: 0x06000ECC RID: 3788 RVA: 0x0009F27F File Offset: 0x0009D47F
		public virtual string GetLocationOverrideDialogue(NPC character)
		{
			if (!this.HasLocationOverrideDialogue(character))
			{
				return null;
			}
			return "";
		}

		// Token: 0x06000ECD RID: 3789 RVA: 0x0009F294 File Offset: 0x0009D494
		public NPC doesPositionCollideWithCharacter(Microsoft.Xna.Framework.Rectangle r, bool ignoreMonsters = false)
		{
			foreach (NPC i in this.characters)
			{
				if (i.GetBoundingBox().Intersects(r) && (!i.IsMonster || !ignoreMonsters))
				{
					return i;
				}
			}
			return null;
		}

		// Token: 0x06000ECE RID: 3790 RVA: 0x0009F304 File Offset: 0x0009D504
		public void switchOutNightTiles()
		{
			string[] split = this.GetMapPropertySplitBySpaces("NightTiles");
			for (int i = 0; i < split.Length; i += 4)
			{
				string layerId;
				string error;
				Point position;
				int tileIndex;
				if (!ArgUtility.TryGet(split, i, out layerId, out error, true, "string layerId") || !ArgUtility.TryGetPoint(split, i + 1, out position, out error, "Point position") || !ArgUtility.TryGetInt(split, i + 3, out tileIndex, out error, "int tileIndex"))
				{
					this.LogMapPropertyError("NightTiles", split, error, ' ');
				}
				else if ((tileIndex != 726 && tileIndex != 720) || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Tile tile = this.map.RequireLayer(layerId).Tiles[position.X, position.Y];
					if (tile == null)
					{
						string text = "NightTiles";
						string[] value = split;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
						defaultInterpolatedStringHandler.AppendLiteral("there's no tile at position (");
						defaultInterpolatedStringHandler.AppendFormatted<Point>(position);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						this.LogMapPropertyError(text, value, defaultInterpolatedStringHandler.ToStringAndClear(), ' ');
					}
					else
					{
						tile.TileIndex = tileIndex;
					}
				}
			}
			if (!(this is MineShaft) && !(this is Woods))
			{
				this.lightGlows.Clear();
			}
		}

		// Token: 0x06000ECF RID: 3791 RVA: 0x0009F438 File Offset: 0x0009D638
		public string GetMorningSong()
		{
			LocationWeather locationWeather = this.GetWeather();
			if (locationWeather.IsRaining)
			{
				return "rain";
			}
			List<string> songList = new List<string>();
			List<LocationMusicData> entries = this.GetLocationContext().Music;
			if (entries == null || entries.Count <= 0)
			{
				entries = (LocationContexts.Default.Music ?? new List<LocationMusicData>());
			}
			foreach (LocationMusicData entry in entries)
			{
				if (GameStateQuery.CheckConditions(entry.Condition, this, null, null, null, null, null))
				{
					songList.Add(entry.Track);
				}
			}
			if (songList.Count == 0)
			{
				return "none";
			}
			int songIndex = locationWeather.monthlyNonRainyDayCount.Value - 1;
			if (songIndex < 0)
			{
				songIndex = 0;
			}
			return songList[songIndex % songList.Count];
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x0009F51C File Offset: 0x0009D71C
		public static void HandleMusicChange(GameLocation oldLocation, GameLocation newLocation)
		{
			string currentTrack = Game1.getMusicTrackName(MusicContext.Default);
			if (!newLocation.IsOutdoors && Game1.IsPlayingOutdoorsAmbience)
			{
				Game1.changeMusicTrack("none", true, MusicContext.Default);
			}
			if (currentTrack == "rain")
			{
				if (!Game1.IsRainingHere(newLocation))
				{
					Game1.stopMusicTrack(MusicContext.Default);
				}
				else if (newLocation is MineShaft && !(oldLocation is MineShaft))
				{
					Game1.stopMusicTrack(MusicContext.Default);
				}
			}
			if (Game1.getMusicTrackName(MusicContext.Default) == "sam_acoustic1")
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
			if (newLocation is MineShaft)
			{
				return;
			}
			string oldLocationContextId = (oldLocation != null) ? oldLocation.GetLocationContextId() : null;
			LocationContextData oldLocationContext = (oldLocation != null) ? oldLocation.GetLocationContext() : null;
			LocationData newLocationData = (newLocation != null) ? newLocation.GetData() : null;
			string newLocationContextId = (newLocation != null) ? newLocation.GetLocationContextId() : null;
			LocationContextData newLocationContext = (newLocation != null) ? newLocation.GetLocationContext() : null;
			string newLocationMusic = (newLocation != null) ? newLocation.GetLocationSpecificMusic() : null;
			MusicContext newMusicContext = (newLocationData != null) ? newLocationData.MusicContext : MusicContext.Default;
			bool newLocationIsTownTheme = false;
			if (newLocation != null)
			{
				if (newLocationMusic != null)
				{
					newLocationIsTownTheme = (newLocationData != null && newLocationData.MusicIsTownTheme);
					newLocation.isMusicTownMusic = new bool?(newLocationIsTownTheme);
				}
				else
				{
					newLocation.isMusicTownMusic = new bool?(false);
				}
			}
			if (newLocationMusic == null || newMusicContext == MusicContext.Default)
			{
				Game1.stopMusicTrack(MusicContext.SubLocation);
			}
			if (newLocationMusic == null && Game1.IsRainingHere(newLocation))
			{
				newLocationMusic = "rain";
			}
			else if (Game1.IsPlayingMorningSong && oldLocation != null && oldLocation.GetMorningSong() != newLocation.GetMorningSong() && Game1.shouldPlayMorningSong(true))
			{
				Game1.playMorningSong(true);
				return;
			}
			if (newLocationMusic == null && !Game1.IsPlayingBackgroundMusic && newLocation.isOutdoors.Value && Game1.shouldPlayMorningSong(false))
			{
				Game1.playMorningSong(false);
				return;
			}
			if (oldLocationContextId != newLocationContextId)
			{
				GameLocation.PlayedNewLocationContextMusic = false;
			}
			if (!newLocationContext.DefaultMusicDelayOneScreen)
			{
				GameLocation.PlayedNewLocationContextMusic = false;
			}
			if (Game1.IsPlayingTownMusic && newLocation.IsOutdoors && (!newLocationIsTownTheme || newLocationMusic != currentTrack))
			{
				Game1.IsPlayingTownMusic = false;
				Game1.changeMusicTrack("none", true, MusicContext.Default);
			}
			if (newLocationIsTownTheme)
			{
				if (newLocationMusic == currentTrack)
				{
					return;
				}
				newLocationMusic = null;
			}
			if (newLocationMusic == null)
			{
				if (oldLocationContext != null && newLocationContext.DefaultMusic != oldLocationContext.DefaultMusic)
				{
					Game1.stopMusicTrack(MusicContext.Default);
				}
				if (!GameLocation.PlayedNewLocationContextMusic)
				{
					if (newLocationContext.DefaultMusic != null)
					{
						if (Game1.isDarkOut(newLocation) || Game1.isStartingToGetDarkOut(newLocation) || Game1.IsRainingHere(newLocation))
						{
							GameLocation.PlayedNewLocationContextMusic = true;
						}
						else if (newLocationContext.DefaultMusicCondition == null || GameStateQuery.CheckConditions(newLocationContext.DefaultMusicCondition, newLocation, null, null, null, null, null))
						{
							Game1.changeMusicTrack(newLocationContext.DefaultMusic, true, MusicContext.Default);
							Game1.IsPlayingBackgroundMusic = true;
							GameLocation.PlayedNewLocationContextMusic = true;
						}
					}
					else
					{
						GameLocation.PlayedNewLocationContextMusic = true;
						if (!newLocationIsTownTheme && Game1.shouldPlayMorningSong(true))
						{
							Game1.playMorningSong(false);
							return;
						}
					}
				}
			}
			if (currentTrack != newLocationMusic)
			{
				if (newLocationMusic == null)
				{
					if (!Game1.IsPlayingBackgroundMusic && !Game1.IsPlayingOutdoorsAmbience)
					{
						Game1.stopMusicTrack(MusicContext.Default);
						return;
					}
				}
				else
				{
					Game1.changeMusicTrack(newLocationMusic, true, newMusicContext);
				}
			}
		}

		// Token: 0x06000ED1 RID: 3793 RVA: 0x0009F7DC File Offset: 0x0009D9DC
		public virtual void checkForMusic(GameTime time)
		{
			if (Game1.getMusicTrackName(MusicContext.Default) == "sam_acoustic1" && Game1.isMusicContextActiveButNotPlaying(MusicContext.Default))
			{
				Game1.changeMusicTrack("none", true, MusicContext.Default);
			}
			if (this.isMusicTownMusic != null && this.isMusicTownMusic.Value && !Game1.eventUp && Game1.timeOfDay < 1800 && (Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) || Game1.IsPlayingOutdoorsAmbience))
			{
				string townMusicTrack = this.GetLocationSpecificMusic();
				if (townMusicTrack != null)
				{
					LocationData data = this.GetData();
					MusicContext context = (data != null) ? data.MusicContext : MusicContext.Default;
					Game1.changeMusicTrack(townMusicTrack, false, context);
					Game1.IsPlayingBackgroundMusic = true;
					Game1.IsPlayingTownMusic = true;
				}
			}
			if (this.IsOutdoors && !this.IsRainingHere() && !Game1.eventUp)
			{
				bool isNight = Game1.isDarkOut(this);
				if (isNight && Game1.IsPlayingOutdoorsAmbience && !Game1.IsPlayingNightAmbience)
				{
					Game1.changeMusicTrack("none", true, MusicContext.Default);
				}
				if (Game1.isMusicContextActiveButNotPlaying(MusicContext.Default))
				{
					if (!isNight)
					{
						LocationContextData context2 = this.GetLocationContext();
						if (context2.DayAmbience != null)
						{
							Game1.changeMusicTrack(context2.DayAmbience, true, MusicContext.Default);
						}
						else
						{
							switch (this.GetSeason())
							{
							case Season.Spring:
								Game1.changeMusicTrack("spring_day_ambient", true, MusicContext.Default);
								break;
							case Season.Summer:
								Game1.changeMusicTrack("summer_day_ambient", true, MusicContext.Default);
								break;
							case Season.Fall:
								Game1.changeMusicTrack("fall_day_ambient", true, MusicContext.Default);
								break;
							case Season.Winter:
								Game1.changeMusicTrack("winter_day_ambient", true, MusicContext.Default);
								break;
							}
						}
						Game1.IsPlayingOutdoorsAmbience = true;
						return;
					}
					if (Game1.timeOfDay < 2500)
					{
						LocationContextData context3 = this.GetLocationContext();
						if (context3.NightAmbience != null)
						{
							Game1.changeMusicTrack(context3.NightAmbience, true, MusicContext.Default);
						}
						else
						{
							switch (this.GetSeason())
							{
							case Season.Spring:
								Game1.changeMusicTrack("spring_night_ambient", true, MusicContext.Default);
								break;
							case Season.Summer:
								Game1.changeMusicTrack("spring_night_ambient", true, MusicContext.Default);
								break;
							case Season.Fall:
								Game1.changeMusicTrack("spring_night_ambient", true, MusicContext.Default);
								break;
							case Season.Winter:
								Game1.changeMusicTrack("none", true, MusicContext.Default);
								break;
							}
						}
						Game1.IsPlayingNightAmbience = true;
						Game1.IsPlayingOutdoorsAmbience = true;
						return;
					}
				}
			}
			else if (this.IsRainingHere() && !Game1.showingEndOfNightStuff && Game1.isMusicContextActiveButNotPlaying(MusicContext.Default))
			{
				Game1.changeMusicTrack("rain", true, MusicContext.Default);
			}
		}

		// Token: 0x06000ED2 RID: 3794 RVA: 0x0009FA04 File Offset: 0x0009DC04
		public virtual string GetLocationSpecificMusic()
		{
			LocationData data = this.GetData();
			if (data != null)
			{
				if (data.MusicIgnoredInRain && this.IsRainingHere())
				{
					return null;
				}
				Season season = this.GetSeason();
				bool ignoreInSeason = false;
				switch (season)
				{
				case Season.Spring:
					ignoreInSeason = data.MusicIgnoredInSpring;
					break;
				case Season.Summer:
					ignoreInSeason = data.MusicIgnoredInSummer;
					break;
				case Season.Fall:
					ignoreInSeason = data.MusicIgnoredInFall;
					break;
				case Season.Winter:
					ignoreInSeason = data.MusicIgnoredInWinter;
					break;
				}
				if (ignoreInSeason)
				{
					return null;
				}
				if (season == Season.Fall && this.IsDebrisWeatherHere() && data.MusicIgnoredInFallDebris)
				{
					return null;
				}
				List<LocationMusicData> music2 = data.Music;
				if (music2 != null && music2.Count > 0)
				{
					foreach (LocationMusicData music in data.Music)
					{
						if (GameStateQuery.CheckConditions(music.Condition, this, null, null, null, null, null))
						{
							return music.Track;
						}
					}
				}
				if (data.MusicDefault != null)
				{
					return data.MusicDefault;
				}
			}
			string[] musicFields = this.GetMapPropertySplitBySpaces("Music");
			if (musicFields.Length == 0)
			{
				return null;
			}
			if (musicFields.Length <= 1)
			{
				return musicFields[0];
			}
			int startTime;
			string error;
			int endTime;
			string musicId;
			if (!ArgUtility.TryGetInt(musicFields, 0, out startTime, out error, "int startTime") || !ArgUtility.TryGetInt(musicFields, 1, out endTime, out error, "int endTime") || !ArgUtility.TryGet(musicFields, 2, out musicId, out error, true, "string musicId"))
			{
				this.LogMapPropertyError("Music", musicFields, error, ' ');
				return null;
			}
			if (Game1.timeOfDay < startTime || (endTime != 0 && Game1.timeOfDay >= endTime))
			{
				return null;
			}
			return musicId;
		}

		// Token: 0x06000ED3 RID: 3795 RVA: 0x0009FB9C File Offset: 0x0009DD9C
		public NPC isCollidingWithCharacter(Microsoft.Xna.Framework.Rectangle box)
		{
			if (Game1.isFestival() && this.currentEvent != null)
			{
				foreach (NPC i in this.currentEvent.actors)
				{
					if (i.GetBoundingBox().Intersects(box))
					{
						return i;
					}
				}
			}
			foreach (NPC j in this.characters)
			{
				if (j.GetBoundingBox().Intersects(box))
				{
					return j;
				}
			}
			return null;
		}

		// Token: 0x06000ED4 RID: 3796 RVA: 0x0009FC68 File Offset: 0x0009DE68
		public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (this.critters != null && Game1.farmEvent == null)
			{
				for (int i = 0; i < this.critters.Count; i++)
				{
					this.critters[i].drawAboveFrontLayer(b);
				}
			}
			foreach (NPC npc in this.characters)
			{
				npc.drawAboveAlwaysFrontLayer(b);
			}
			if (!(this is MineShaft))
			{
				foreach (NPC npc2 in this.characters)
				{
					Monster monster = npc2 as Monster;
					if (monster != null)
					{
						monster.drawAboveAllLayers(b);
					}
				}
			}
			if (this.TemporarySprites.Count > 0)
			{
				foreach (TemporaryAnimatedSprite s in this.TemporarySprites)
				{
					if (s.drawAboveAlwaysFront)
					{
						s.draw(b, false, 0, 0, 1f);
					}
				}
			}
			if (this.projectiles.Count > 0)
			{
				foreach (Projectile projectile in this.projectiles)
				{
					projectile.draw(b);
				}
			}
		}

		// Token: 0x06000ED5 RID: 3797 RVA: 0x0009FDEC File Offset: 0x0009DFEC
		public bool moveContents(int oldX, int oldY, int newX, int newY, string unlessItemId)
		{
			Vector2 oldTile = new Vector2((float)oldX, (float)oldY);
			Vector2 newTile = new Vector2((float)newX, (float)newY);
			Microsoft.Xna.Framework.Rectangle oldPixelArea = new Microsoft.Xna.Framework.Rectangle(oldX * 64, oldY * 64, 64, 64);
			Microsoft.Xna.Framework.Rectangle newPixelArea = new Microsoft.Xna.Framework.Rectangle(newX * 64, newY * 64, 64, 64);
			bool movedAny = false;
			Object o;
			if (this.objects.TryGetValue(oldTile, out o) && !this.objects.ContainsKey(newTile) && (unlessItemId == null || !ItemRegistry.HasItemId(o, unlessItemId)))
			{
				this.objects.Remove(oldTile);
				this.objects.Add(newTile, o);
				movedAny = true;
			}
			Func<Furniture, bool> <>9__0;
			for (int i = this.furniture.Count - 1; i >= 0; i--)
			{
				Furniture f = this.furniture[i];
				if (f.boundingBox.Value.Intersects(oldPixelArea) && (unlessItemId == null || !ItemRegistry.HasItemId(f, unlessItemId)))
				{
					IEnumerable<Furniture> source = this.furniture;
					Func<Furniture, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((Furniture p) => p.boundingBox.Value.Intersects(newPixelArea)));
					}
					if (!source.Any(predicate))
					{
						Vector2 offset = f.TileLocation - oldTile;
						this.furniture.RemoveAt(i);
						f.TileLocation = newTile + offset;
						this.furniture.Add(f);
						movedAny = true;
					}
				}
			}
			return movedAny;
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x0009FF58 File Offset: 0x0009E158
		private void getGalaxySword()
		{
			Item galaxySword = ItemRegistry.Create("(W)4", 1, 0, false);
			Game1.flashAlpha = 1f;
			Game1.player.holdUpItemThenMessage(galaxySword, true);
			Game1.player.reduceActiveItemByOne();
			if (!Game1.player.addItemToInventoryBool(galaxySword, false))
			{
				Game1.createItemDebris(galaxySword, Game1.player.getStandingPosition(), 1, null, -1, false);
			}
			Game1.player.mailReceived.Add("galaxySword");
			Game1.player.jitterStrength = 0f;
			Game1.screenGlowHold = false;
			Game1.multiplayer.globalChatInfoMessage("GalaxySword", new string[]
			{
				Game1.player.Name
			});
		}

		// Token: 0x06000ED7 RID: 3799 RVA: 0x000A0002 File Offset: 0x0009E202
		public static void RegisterTouchAction(string key, Action<GameLocation, string[], Farmer, Vector2> action)
		{
			if (action == null)
			{
				GameLocation.registeredTouchActions.Remove(key);
				return;
			}
			GameLocation.registeredTouchActions[key] = action;
		}

		// Token: 0x06000ED8 RID: 3800 RVA: 0x000A0020 File Offset: 0x0009E220
		public static void RegisterTileAction(string key, Func<GameLocation, string[], Farmer, Point, bool> action)
		{
			if (action == null)
			{
				GameLocation.registeredTileActions.Remove(key);
				return;
			}
			GameLocation.registeredTileActions[key] = action;
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x000A003E File Offset: 0x0009E23E
		public virtual bool IgnoreTouchActions()
		{
			return Game1.eventUp;
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x000A0048 File Offset: 0x0009E248
		public virtual void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			string[] split = ArgUtility.SplitBySpace(fullActionString);
			this.performTouchAction(split, playerStandingPosition);
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x000A0064 File Offset: 0x0009E264
		public virtual void performTouchAction(string[] action, Vector2 playerStandingPosition)
		{
			GameLocation.<>c__DisplayClass272_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.action = action;
			CS$<>8__locals1.playerStandingPosition = playerStandingPosition;
			if (this.IgnoreTouchActions())
			{
				return;
			}
			try
			{
				string actionType;
				string error;
				Action<GameLocation, string[], Farmer, Vector2> actionHandler;
				if (!ArgUtility.TryGet(CS$<>8__locals1.action, 0, out actionType, out error, true, "string actionType"))
				{
					this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
				}
				else if (GameLocation.registeredTouchActions.TryGetValue(actionType, out actionHandler))
				{
					actionHandler(this, CS$<>8__locals1.action, Game1.player, CS$<>8__locals1.playerStandingPosition);
				}
				else if (actionType != null)
				{
					switch (actionType.Length)
					{
					case 4:
					{
						char c = actionType[0];
						if (c != 'D')
						{
							if (c == 'W')
							{
								if (actionType == "Warp")
								{
									string locationToWarp2;
									Point tile2;
									string mailRequired;
									if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out locationToWarp2, out error, true, "string locationToWarp") || !ArgUtility.TryGetPoint(CS$<>8__locals1.action, 2, out tile2, out error, "Point tile") || !ArgUtility.TryGetOptional(CS$<>8__locals1.action, 4, out mailRequired, out error, null, true, "string mailRequired"))
									{
										this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
									}
									else if (mailRequired == null || Game1.player.mailReceived.Contains(mailRequired))
									{
										Game1.warpFarmer(locationToWarp2, tile2.X, tile2.Y, false);
									}
								}
							}
						}
						else if (actionType == "Door")
						{
							int i = 1;
							while (i < CS$<>8__locals1.action.Length)
							{
								if (CS$<>8__locals1.action[i] == "Sebastian" && this.IsGreenRainingHere() && Game1.year == 1)
								{
									break;
								}
								if (Game1.player.getFriendshipHeartLevelForNPC(CS$<>8__locals1.action[i]) < 2 && i == CS$<>8__locals1.action.Length - 1)
								{
									Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
									Game1.player.yVelocity = 0f;
									Game1.player.Halt();
									Game1.player.TemporaryPassableTiles.Clear();
									if (Game1.player.Tile == this.lastTouchActionLocation)
									{
										if (Game1.player.Position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
										{
											Game1.player.position.Y += 4f;
										}
										else
										{
											Game1.player.position.Y -= 4f;
										}
										this.lastTouchActionLocation = Vector2.Zero;
									}
									if ((Game1.player.mailReceived.Contains("doorUnlock" + CS$<>8__locals1.action[1]) && (CS$<>8__locals1.action.Length == 2 || Game1.player.mailReceived.Contains("doorUnlock" + CS$<>8__locals1.action[2]))) || (CS$<>8__locals1.action.Length == 3 && Game1.player.mailReceived.Contains("doorUnlock" + CS$<>8__locals1.action[2])))
									{
										break;
									}
									this.ShowLockedDoorMessage(CS$<>8__locals1.action);
									break;
								}
								else
								{
									if (i != CS$<>8__locals1.action.Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(CS$<>8__locals1.action[i]) >= 2)
									{
										Game1.player.mailReceived.Add("doorUnlock" + CS$<>8__locals1.action[i]);
										break;
									}
									if (i == CS$<>8__locals1.action.Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(CS$<>8__locals1.action[i]) >= 2)
									{
										Game1.player.mailReceived.Add("doorUnlock" + CS$<>8__locals1.action[i]);
										break;
									}
									i++;
								}
							}
						}
						break;
					}
					case 5:
					{
						char c = actionType[0];
						if (c != 'E')
						{
							if (c == 'S')
							{
								if (actionType == "Sleep")
								{
									if (!Game1.newDay && Game1.shouldTimePass(false) && Game1.player.hasMoved && !Game1.player.passedOut)
									{
										this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), this.createYesNoResponses(), "Sleep", null);
									}
								}
							}
						}
						else if (actionType == "Emote")
						{
							string npcName;
							int emote;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out npcName, out error, true, "string npcName") || !ArgUtility.TryGetInt(CS$<>8__locals1.action, 2, out emote, out error, "int emote"))
							{
								this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
							}
							else
							{
								NPC characterFromName = this.getCharacterFromName(npcName);
								if (characterFromName != null)
								{
									characterFromName.doEmote(emote, true);
								}
							}
						}
						break;
					}
					case 8:
						if (actionType == "asdlfkjg")
						{
							this.removeTileProperty(13, 29, "Back", "TouchAction");
							this.removeTileProperty(14, 29, "Back", "TouchAction");
							this.removeTileProperty(15, 29, "Back", "TouchAction");
							if (Game1.timeOfDay >= 1920 && Game1.timeOfDay < 2020 && this.farmers.Count == 1 && Game1.stats.DaysPlayed > 3U && !Game1.isRaining && Game1.random.NextDouble() < 0.025)
							{
								Game1.player.mailReceived.Add("asdlkjfg1");
								this.playSound("shadowDie", null, null, SoundContext.Default);
								DelayedAction.playSoundAfterDelay("grassyStep", 500, this, null, -1, false);
								DelayedAction.playSoundAfterDelay("grassyStep", 1000, this, null, -1, false);
								DelayedAction.playSoundAfterDelay("grassyStep", 1500, this, null, -1, false);
								this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(390f, 1980f), true, 0f, Color.White)
								{
									animationLength = 8,
									totalNumberOfLoops = 99,
									interval = 100f,
									motion = new Vector2(-5f, -1f),
									scale = 5.5f
								});
							}
						}
						break;
					case 9:
					{
						char c = actionType[0];
						if (c != 'M')
						{
							if (c == 'P')
							{
								if (actionType == "PlayEvent")
								{
									string eventId;
									bool checkPreconditions;
									bool checkSeen;
									string fallbackAction;
									if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out eventId, out error, true, "string eventId") || !ArgUtility.TryGetOptionalBool(CS$<>8__locals1.action, 2, out checkPreconditions, out error, true, "bool checkPreconditions") || !ArgUtility.TryGetOptionalBool(CS$<>8__locals1.action, 3, out checkSeen, out error, true, "bool checkSeen") || !ArgUtility.TryGetOptionalRemainder(CS$<>8__locals1.action, 4, out fallbackAction, null, ' '))
									{
										this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
									}
									else if (!Game1.PlayEvent(eventId, checkPreconditions, checkSeen) && fallbackAction != null)
									{
										this.performAction(fallbackAction, Game1.player, new Location((int)CS$<>8__locals1.playerStandingPosition.X, (int)CS$<>8__locals1.playerStandingPosition.Y));
									}
								}
							}
						}
						else if (actionType == "MagicWarp")
						{
							string mailRequired2;
							string locationToWarp;
							Point tile;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out locationToWarp, out error, true, "string locationToWarp") || !ArgUtility.TryGetPoint(CS$<>8__locals1.action, 2, out tile, out error, "Point tile") || !ArgUtility.TryGetOptional(CS$<>8__locals1.action, 4, out mailRequired2, out error, null, true, "string mailRequired"))
							{
								this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
							}
							else if (mailRequired2 == null || Game1.player.mailReceived.Contains(mailRequired2))
							{
								for (int j = 0; j < 12; j++)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)Game1.player.position.X - 256, (int)Game1.player.position.X + 192), (float)Game1.random.Next((int)Game1.player.position.Y - 256, (int)Game1.player.position.Y + 192)), false, Game1.random.NextBool())
									});
								}
								this.playSound("wand", null, null, SoundContext.Default);
								Game1.freezeControls = true;
								Game1.displayFarmer = false;
								Game1.player.CanMove = false;
								Game1.flashAlpha = 1f;
								DelayedAction.fadeAfterDelay(delegate
								{
									Game1.warpFarmer(locationToWarp, tile.X, tile.Y, false);
									Game1.fadeToBlackAlpha = 0.99f;
									Game1.screenGlow = false;
									Game1.displayFarmer = true;
									Game1.player.CanMove = true;
									Game1.freezeControls = false;
								}, 1000);
								Microsoft.Xna.Framework.Rectangle playerBounds = Game1.player.GetBoundingBox();
								Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
								r.Inflate(192, 192);
								int k = 0;
								Point playerTile = Game1.player.TilePoint;
								for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(6, new Vector2((float)x, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
										{
											layerDepth = 1f,
											delayBeforeAnimationStart = k * 25,
											motion = new Vector2(-0.25f, 0f)
										}
									});
									k++;
								}
							}
						}
						break;
					}
					case 10:
						if (actionType == "MensLocker")
						{
							if (!Game1.player.IsMale)
							{
								Game1.player.position.Y += ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
								Game1.player.Halt();
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
							}
						}
						break;
					case 11:
						if (actionType == "MagicalSeal")
						{
							if (!Game1.player.mailReceived.Contains("krobusUnseal"))
							{
								Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
								Game1.player.yVelocity = 0f;
								Game1.player.Halt();
								Game1.player.TemporaryPassableTiles.Clear();
								if (Game1.player.Tile == this.lastTouchActionLocation)
								{
									if (Game1.player.position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
									{
										Game1.player.position.Y += 4f;
									}
									else
									{
										Game1.player.position.Y -= 4f;
									}
									this.lastTouchActionLocation = Vector2.Zero;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_MagicSeal"));
								for (int l = 0; l < 40; l++)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2((float)(-8 + l % 4 * 16), (float)(-(float)(l / 4) * 64 / 4)), false, false)
										{
											layerDepth = 0.1152f + (float)l / 10000f,
											color = new Color(100 + l * 4, l * 5, 120 + l * 4),
											pingPong = true,
											delayBeforeAnimationStart = l * 10,
											scale = 4f,
											alphaFade = 0.01f
										}
									});
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 17f) * 64f + new Vector2((float)(-8 + l % 4 * 16), (float)(l / 4 * 64 / 4)), false, false)
										{
											layerDepth = 0.1152f + (float)l / 10000f,
											color = new Color(232 - l * 4, 192 - l * 6, 255 - l * 4),
											pingPong = true,
											delayBeforeAnimationStart = 320 + l * 10,
											scale = 4f,
											alphaFade = 0.01f
										}
									});
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2((float)(-8 + l % 4 * 16), (float)(-(float)(l / 4) * 64 / 4)), false, false)
										{
											layerDepth = 0.1152f + (float)l / 10000f,
											color = new Color(100 + l * 4, l * 6, 120 + l * 4),
											pingPong = true,
											delayBeforeAnimationStart = 640 + l * 10,
											scale = 4f,
											alphaFade = 0.01f
										}
									});
								}
								Game1.player.jitterStrength = 2f;
								Game1.player.freezePause = 500;
								this.playSound("debuffHit", null, null, SoundContext.Default);
							}
						}
						break;
					case 12:
					{
						char c = actionType[0];
						if (c != 'P')
						{
							if (c == 'W')
							{
								if (actionType == "WomensLocker")
								{
									if (Game1.player.IsMale)
									{
										Game1.player.position.Y += ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
										Game1.player.Halt();
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
									}
								}
							}
						}
						else if (actionType == "PoolEntrance")
						{
							if (!Game1.player.swimming.Value)
							{
								Game1.player.swimTimer = 800;
								Game1.player.swimming.Value = true;
								Game1.player.position.Y += 16f;
								Game1.player.yVelocity = -8f;
								this.playSound("pullItemFromWater", null, null, SoundContext.Default);
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(27, 100f, 4, 0, new Vector2(Game1.player.Position.X, (float)(Game1.player.StandingPixel.Y - 40)), false, false)
									{
										layerDepth = 1f,
										motion = new Vector2(0f, 2f)
									}
								});
							}
							else
							{
								Game1.player.jump();
								Game1.player.swimTimer = 800;
								Game1.player.position.X = CS$<>8__locals1.playerStandingPosition.X * 64f;
								this.playSound("pullItemFromWater", null, null, SoundContext.Default);
								Game1.player.yVelocity = 8f;
								Game1.player.swimming.Value = false;
							}
							Game1.player.noMovementPause = 500;
						}
						break;
					}
					case 13:
						if (actionType == "FaceDirection")
						{
							string npcName2;
							int direction;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out npcName2, out error, true, "string npcName") || !ArgUtility.TryGetInt(CS$<>8__locals1.action, 2, out direction, out error, "int direction"))
							{
								this.<performTouchAction>g__LogError|272_0(error, ref CS$<>8__locals1);
							}
							else
							{
								NPC characterFromName2 = this.getCharacterFromName(npcName2);
								if (characterFromName2 != null)
								{
									characterFromName2.faceDirection(direction);
								}
							}
						}
						break;
					case 14:
						if (actionType == "legendarySword")
						{
							Object activeObject = Game1.player.ActiveObject;
							if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)74" && !Game1.player.mailReceived.Contains("galaxySword"))
							{
								Game1.player.Halt();
								Game1.player.faceDirection(2);
								Game1.player.showCarrying();
								Game1.player.jitterStrength = 1f;
								Game1.pauseThenDoFunction(7000, new Game1.afterFadeFunction(this.getGalaxySword));
								Game1.changeMusicTrack("none", false, MusicContext.Event);
								this.playSound("crit", null, null, SoundContext.Default);
								Game1.screenGlowOnce(new Color(30, 0, 150), true, 0.01f, 0.999f);
								DelayedAction.playSoundAfterDelay("stardrop", 1500, null, null, -1, false);
								Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000, ""));
								Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
								{
									Game1.stopMusicTrack(MusicContext.Event);
								}));
							}
							else if (!Game1.player.mailReceived.Contains("galaxySword"))
							{
								this.localSound("SpringBirds", null, null, SoundContext.Default);
							}
						}
						break;
					case 15:
						if (actionType == "ConditionalDoor")
						{
							if (CS$<>8__locals1.action.Length > 1 && !Game1.eventUp)
							{
								if (!GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(CS$<>8__locals1.action, ' ', 1, 2147483647), null, null, null, null, null, null))
								{
									Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
									Game1.player.yVelocity = 0f;
									Game1.player.Halt();
									Game1.player.TemporaryPassableTiles.Clear();
									if (Game1.player.Tile == this.lastTouchActionLocation)
									{
										if (Game1.player.Position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
										{
											Game1.player.position.Y += 4f;
										}
										else
										{
											Game1.player.position.Y -= 4f;
										}
										this.lastTouchActionLocation = Vector2.Zero;
									}
									string message = this.doesTileHaveProperty((int)CS$<>8__locals1.playerStandingPosition.X / 64, (int)CS$<>8__locals1.playerStandingPosition.Y / 64, "LockedDoorMessage", "Back", false);
									if (message != null)
									{
										Game1.drawObjectDialogue(Game1.content.LoadString(TokenParser.ParseText(message, null, null, null)));
									}
									else
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
									}
								}
							}
						}
						break;
					case 18:
						if (actionType == "ChangeIntoSwimsuit")
						{
							Game1.player.changeIntoSwimsuit();
						}
						break;
					case 19:
						if (actionType == "ChangeOutOfSwimsuit")
						{
							Game1.player.changeOutOfSwimSuit();
						}
						break;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000EDC RID: 3804 RVA: 0x000A1554 File Offset: 0x0009F754
		public virtual void updateMap()
		{
			if (this._mapPathDirty)
			{
				this._mapPathDirty = false;
				if (!string.Equals(this.mapPath.Value, this.loadedMapPath, StringComparison.Ordinal))
				{
					this.reloadMap();
					this.updateLayout();
				}
			}
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x000A158A File Offset: 0x0009F78A
		public virtual void updateLayout()
		{
			if (Game1.IsMasterGame)
			{
				this.updateDoors();
				this.updateWarps();
			}
		}

		// Token: 0x06000EDE RID: 3806 RVA: 0x000A15A0 File Offset: 0x0009F7A0
		public LargeTerrainFeature getLargeTerrainFeatureAt(int tileX, int tileY)
		{
			foreach (LargeTerrainFeature ltf in this.largeTerrainFeatures)
			{
				if (ltf.getBoundingBox().Contains(tileX * 64 + 32, tileY * 64 + 32))
				{
					return ltf;
				}
			}
			return null;
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x000A1614 File Offset: 0x0009F814
		public virtual void UpdateWhenCurrentLocation(GameTime time)
		{
			GameLocation.<>c__DisplayClass276_0 CS$<>8__locals1 = new GameLocation.<>c__DisplayClass276_0();
			CS$<>8__locals1.time = time;
			CS$<>8__locals1.<>4__this = this;
			this.updateMap();
			if (this.wasUpdated)
			{
				return;
			}
			this.wasUpdated = true;
			if (this._mapSeatsDirty)
			{
				this.UpdateMapSeats();
			}
			this.furnitureToRemove.Update(this);
			if (Game1.player.currentLocation.Equals(this))
			{
				this._updateAmbientLighting();
			}
			for (int i = 0; i < this.furniture.Count; i++)
			{
				this.furniture[i].updateWhenCurrentLocation(CS$<>8__locals1.time);
			}
			AmbientLocationSounds.update(CS$<>8__locals1.time);
			List<Critter> list = this.critters;
			if (list != null)
			{
				list.RemoveAll((Critter critter) => critter.update(CS$<>8__locals1.time, CS$<>8__locals1.<>4__this));
			}
			if (this.fishSplashAnimation != null)
			{
				this.fishSplashAnimation.update(CS$<>8__locals1.time);
				bool frenzy = this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("");
				double rate = frenzy ? 0.1 : 0.02;
				if (Game1.random.NextDouble() < rate)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(0, this.fishSplashAnimation.position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), Color.White * 0.3f, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = (this.fishSplashAnimation.position.Y - 64f) / 10000f
					});
					if (frenzy)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite(0, this.fishSplashAnimation.position + new Vector2((float)Game1.random.Next(-64, 64), (float)Game1.random.Next(-64, 64)), Color.White * 0.3f, 8, false, 100f, 0, -1, -1f, -1, 0)
						{
							layerDepth = (this.fishSplashAnimation.position.Y - 64f) / 10000f
						});
						if (Game1.random.NextDouble() < 0.1)
						{
							ICue cue;
							Game1.sounds.PlayLocal("slosh", this, new Vector2?(this.fishSplashAnimation.Position / 64f), null, SoundContext.Default, out cue);
						}
					}
				}
				if (frenzy && Game1.random.NextDouble() < 0.005)
				{
					Vector2 position = this.fishSplashAnimation.position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32));
					Action<Vector2> splashAnimation = delegate(Vector2 pos)
					{
						CS$<>8__locals1.<>4__this.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, pos, false, false)
						{
							delayBeforeAnimationStart = 0,
							layerDepth = (pos.Y + 1f) / 10000f
						});
					};
					ICue cue;
					Game1.sounds.PlayLocal("slosh", this, new Vector2?(this.fishSplashAnimation.Position / 64f), null, SoundContext.Default, out cue);
					splashAnimation(position);
					ParsedItemData fishData = ItemRegistry.GetData(this.fishFrenzyFish.Value);
					int spriteID = 982648 + Game1.random.Next(99999);
					bool flip = Game1.random.NextDouble() < 0.5;
					float intensity = (float)Game1.random.Next(10, 20) / 10f;
					if (Game1.random.NextDouble() < 0.9)
					{
						intensity *= 0.75f;
					}
					this.TemporarySprites.Add(new TemporaryAnimatedSprite(fishData.GetTextureName(), fishData.GetSourceRect(0, null), position, flip, 0f, Color.White)
					{
						scale = 4f,
						motion = new Vector2((float)(flip ? -1 : 1) * ((float)Game1.random.Next(11) * intensity + intensity * 5f) / 20f, -((float)Game1.random.Next(30, 41) * intensity) / 10f),
						acceleration = new Vector2(0f, 0.1f),
						rotationChange = (float)(flip ? -1 : 1) * ((float)Game1.random.Next(5, 10) * intensity) / 800f,
						yStopCoordinate = (int)position.Y + 1,
						id = spriteID,
						layerDepth = position.Y / 10000f,
						reachedStopCoordinateSprite = delegate(TemporaryAnimatedSprite x)
						{
							CS$<>8__locals1.<>4__this.removeTemporarySpritesWithID(spriteID);
							ICue cue2;
							Game1.sounds.PlayLocal("dropItemInWater", CS$<>8__locals1.<>4__this, new Vector2?(position / 64f), null, SoundContext.Default, out cue2);
							splashAnimation(x.Position);
						}
					});
				}
			}
			if (this.orePanAnimation != null)
			{
				this.orePanAnimation.update(CS$<>8__locals1.time);
				if (Game1.random.NextDouble() < 0.05)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), this.orePanAnimation.position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), false, 0.02f, Color.White * 0.8f)
					{
						scale = 2f,
						animationLength = 6,
						interval = 100f
					});
				}
			}
			this.interiorDoors.Update(CS$<>8__locals1.time);
			this.updateWater(CS$<>8__locals1.time);
			this.Map.Update((long)CS$<>8__locals1.time.ElapsedGameTime.Milliseconds);
			this.debris.RemoveWhere((Debris d) => d.updateChunks(CS$<>8__locals1.time, CS$<>8__locals1.<>4__this));
			if (Game1.shouldTimePass(false) || Game1.isFestival())
			{
				this.projectiles.RemoveWhere((Projectile projectile) => projectile.update(CS$<>8__locals1.time, CS$<>8__locals1.<>4__this));
			}
			for (int j = this._activeTerrainFeatures.Count - 1; j >= 0; j--)
			{
				TerrainFeature feature2 = this._activeTerrainFeatures[j];
				if (feature2.tickUpdate(CS$<>8__locals1.time))
				{
					this.terrainFeatures.Remove(feature2.Tile);
				}
			}
			NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
			if (netCollection != null)
			{
				netCollection.RemoveWhere((LargeTerrainFeature feature) => feature.tickUpdate(CS$<>8__locals1.time));
			}
			foreach (ResourceClump resourceClump in this.resourceClumps)
			{
				resourceClump.tickUpdate(CS$<>8__locals1.time);
			}
			if (this.currentEvent != null)
			{
				bool continue_execution;
				do
				{
					int last_command_index = this.currentEvent.CurrentCommand;
					this.currentEvent.Update(this, CS$<>8__locals1.time);
					if (this.currentEvent != null)
					{
						continue_execution = this.currentEvent.simultaneousCommand;
						if (last_command_index == this.currentEvent.CurrentCommand)
						{
							continue_execution = false;
						}
					}
					else
					{
						continue_execution = false;
					}
				}
				while (continue_execution);
			}
			this.objects.Lock();
			foreach (Object @object in this.objects.Values)
			{
				@object.updateWhenCurrentLocation(CS$<>8__locals1.time);
			}
			this.objects.Unlock();
			if (Game1.gameMode == 3 && this == Game1.currentLocation)
			{
				if (Game1.currentLocation.GetLocationContext().PlayRandomAmbientSounds && this.isOutdoors.Value)
				{
					if (!this.IsRainingHere())
					{
						if (Game1.timeOfDay < 2000)
						{
							if (Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) && !this.IsWinterHere() && Game1.random.NextDouble() < 0.002)
							{
								this.localSound("SpringBirds", null, null, SoundContext.Default);
							}
						}
						else if (Game1.timeOfDay > 2100 && !(this is Beach) && this.IsSummerHere() && !this.IsTemporary && Game1.random.NextDouble() < 0.0005)
						{
							this.localSound("crickets", null, null, SoundContext.Default);
						}
					}
					else if (!Game1.eventUp && Game1.options.musicVolumeLevel > 0f && Game1.random.NextDouble() < 0.00015 && !this.name.Equals("Town"))
					{
						this.localSound("rainsound", null, null, SoundContext.Default);
					}
				}
				Vector2 playerTile = Game1.player.Tile;
				if (this.lastTouchActionLocation.Equals(Vector2.Zero))
				{
					string touchActionProperty = this.doesTileHaveProperty((int)playerTile.X, (int)playerTile.Y, "TouchAction", "Back", false);
					this.lastTouchActionLocation = playerTile;
					if (touchActionProperty != null)
					{
						this.performTouchAction(touchActionProperty, playerTile);
					}
				}
				else if (!this.lastTouchActionLocation.Equals(playerTile))
				{
					this.lastTouchActionLocation = Vector2.Zero;
				}
				foreach (Farmer farmer in this.farmers)
				{
					Vector2 playerPos = farmer.Tile;
					foreach (Vector2 offset in Utility.DirectionsTileVectorsWithDiagonals)
					{
						Vector2 v = playerPos + offset;
						Object obj;
						if (this.objects.TryGetValue(v, out obj))
						{
							obj.farmerAdjacentAction(farmer, offset.X != 0f && offset.Y != 0f);
						}
					}
				}
				if (Game1.player != null)
				{
					int direction = Game1.player.facingDirection.Value;
					GameLocation.<>c__DisplayClass276_2 CS$<>8__locals3;
					CS$<>8__locals3.player_position = Game1.player.Tile;
					Object sign = null;
					if (direction >= 0 && direction < 4)
					{
						Vector2 offset2 = Utility.DirectionsTileVectors[direction];
						sign = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6((int)offset2.X, (int)offset2.Y, ref CS$<>8__locals3);
					}
					if (sign == null)
					{
						Object object2;
						if ((object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(0, -1, ref CS$<>8__locals3)) == null && (object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(0, 1, ref CS$<>8__locals3)) == null && (object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(-1, 0, ref CS$<>8__locals3)) == null && (object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(1, 0, ref CS$<>8__locals3)) == null && (object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(-1, -1, ref CS$<>8__locals3)) == null && (object2 = CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(1, -1, ref CS$<>8__locals3)) == null)
						{
							object2 = (CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(-1, 1, ref CS$<>8__locals3) ?? CS$<>8__locals1.<UpdateWhenCurrentLocation>g__CheckForSign|6(1, 1, ref CS$<>8__locals3));
						}
						sign = object2;
					}
					if (sign != null)
					{
						sign.shouldShowSign = true;
					}
				}
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp in this.animals.Pairs)
			{
				this.tempAnimals.Add(kvp);
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp2 in this.tempAnimals)
			{
				if (kvp2.Value.updateWhenCurrentLocation(CS$<>8__locals1.time, this))
				{
					this.animals.Remove(kvp2.Key);
				}
			}
			this.tempAnimals.Clear();
			foreach (Building building in this.buildings)
			{
				building.Update(CS$<>8__locals1.time);
			}
		}

		// Token: 0x06000EE0 RID: 3808 RVA: 0x000A222C File Offset: 0x000A042C
		public void updateWater(GameTime time)
		{
			this.waterAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.waterAnimationTimer <= 0)
			{
				this.waterAnimationIndex = (this.waterAnimationIndex + 1) % 10;
				this.waterAnimationTimer = 200;
			}
			this.waterPosition += ((!this.isFarm.Value) ? ((float)((Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f)) + 1.0) * 0.15000000596046448)) : 0.1f);
			if (this.waterPosition >= 64f)
			{
				this.waterPosition -= 64f;
				this.waterTileFlip = !this.waterTileFlip;
			}
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x000A22F8 File Offset: 0x000A04F8
		public NPC getCharacterFromName(string name)
		{
			NPC character = null;
			foreach (NPC i in this.characters)
			{
				if (i.Name.Equals(name))
				{
					return i;
				}
			}
			return character;
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x000A235C File Offset: 0x000A055C
		protected virtual void updateCharacters(GameTime time)
		{
			bool shouldTimePass = Game1.shouldTimePass(false);
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				NPC character = this.characters[i];
				if (character != null && (shouldTimePass || character is Horse || character.forceUpdateTimer > 0))
				{
					character.currentLocation = this;
					character.update(time, this);
					if (i < this.characters.Count)
					{
						Monster monster = character as Monster;
						if (monster != null && monster.ShouldMonsterBeRemoved())
						{
							this.characters.RemoveAt(i);
						}
					}
				}
				else if (character != null)
				{
					if (character.hasJustStartedFacingPlayer)
					{
						character.updateFaceTowardsFarmer(time, this);
					}
					character.updateEmote(time);
				}
			}
		}

		// Token: 0x06000EE3 RID: 3811 RVA: 0x000A2408 File Offset: 0x000A0608
		public Projectile getProjectileFromID(int uniqueID)
		{
			foreach (Projectile p in this.projectiles)
			{
				if (p.uniqueID.Value == uniqueID)
				{
					return p;
				}
			}
			return null;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x000A246C File Offset: 0x000A066C
		public virtual void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			this.netAudio.Update();
			this.removeTemporarySpritesWithIDEvent.Poll();
			this.rumbleAndFadeEvent.Poll();
			this.damagePlayersEvent.Poll();
			if (!ignoreWasUpdatedFlush)
			{
				this.wasUpdated = false;
			}
			this.updateCharacters(time);
			for (int i = this.temporarySprites.Count - 1; i >= 0; i--)
			{
				TemporaryAnimatedSprite sprite = (i < this.temporarySprites.Count) ? this.temporarySprites[i] : null;
				if (i < this.temporarySprites.Count && sprite != null && sprite.update(time) && i < this.temporarySprites.Count)
				{
					this.temporarySprites.RemoveAt(i);
				}
			}
			foreach (Building building in this.buildings)
			{
				building.updateWhenFarmNotCurrentLocation(time);
			}
			if (!Game1.currentLocation.Equals(this) && this.animals.Length > 0)
			{
				Building containingBuilding = this.ParentBuilding;
				FarmAnimal[] array = this.animals.Values.ToArray<FarmAnimal>();
				for (int j = 0; j < array.Length; j++)
				{
					array[j].updateWhenNotCurrentLocation(containingBuilding, time, this);
				}
			}
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x000A25C0 File Offset: 0x000A07C0
		public GameLocation GetParentLocation()
		{
			if (this.parentLocationName.Value == null)
			{
				return null;
			}
			return Game1.getLocationFromName(this.parentLocationName.Value);
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x000A25E1 File Offset: 0x000A07E1
		public GameLocation GetRootLocation()
		{
			return this.GetParentLocation() ?? this;
		}

		// Token: 0x06000EE7 RID: 3815 RVA: 0x000A25F0 File Offset: 0x000A07F0
		public Response[] createYesNoResponses()
		{
			return new Response[]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
				new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
			};
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x000A2649 File Offset: 0x000A0849
		public virtual void customQuestCompleteBehavior(string questId)
		{
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x000A264B File Offset: 0x000A084B
		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
		{
			this.lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices);
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x000A265C File Offset: 0x000A085C
		public void createQuestionDialogueWithCustomWidth(string question, Response[] answerChoices, string dialogKey)
		{
			int width = SpriteText.getWidthOfString(question, 999999) + 64;
			this.lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices, width);
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x000A2687 File Offset: 0x000A0887
		public void createQuestionDialogue(string question, Response[] answerChoices, GameLocation.afterQuestionBehavior afterDialogueBehavior, NPC speaker = null)
		{
			this.lastQuestionKey = null;
			this.afterQuestion = afterDialogueBehavior;
			Game1.drawObjectQuestionDialogue(question, answerChoices);
			if (speaker != null)
			{
				Game1.objectDialoguePortraitPerson = speaker;
			}
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x000A26A9 File Offset: 0x000A08A9
		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey, Object actionObject)
		{
			this.lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices);
			this.actionObjectForQuestionDialogue = actionObject;
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x000A26C4 File Offset: 0x000A08C4
		public virtual void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			IList<string> objects = monster.objectsToDrop;
			Vector2 playerPosition = Utility.PointToVector2(who.StandingPixel);
			List<Item> extraDrops = monster.getExtraDropItems();
			string result;
			if (who.isWearingRing("526") && DataLoader.Monsters(Game1.content).TryGetValue(monster.Name, out result))
			{
				string[] objectsSplit = ArgUtility.SplitBySpace(result.Split('/', StringSplitOptions.None)[6]);
				for (int i = 0; i < objectsSplit.Length; i += 2)
				{
					if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
					{
						objects.Add(objectsSplit[i]);
					}
				}
			}
			List<Debris> debrisToAdd = new List<Debris>();
			for (int j = 0; j < objects.Count; j++)
			{
				string objectToAdd = objects[j];
				int parsedIndex;
				if (objectToAdd != null && objectToAdd.StartsWith('-') && int.TryParse(objectToAdd, out parsedIndex))
				{
					debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(Math.Abs(parsedIndex), Game1.random.Next(1, 4), new Vector2((float)x, (float)y), playerPosition, 1f)));
				}
				else
				{
					debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(objectToAdd, new Vector2((float)x, (float)y), playerPosition)));
				}
			}
			for (int k = 0; k < extraDrops.Count; k++)
			{
				debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(extraDrops[k], new Vector2((float)x, (float)y), playerPosition)));
			}
			Trinket.TrySpawnTrinket(this, monster, monster.getStandingPosition(), 1.0);
			if (who.isWearingRing("526"))
			{
				extraDrops = monster.getExtraDropItems();
				for (int l = 0; l < extraDrops.Count; l++)
				{
					Item tmp = extraDrops[l].getOne();
					tmp.Stack = extraDrops[l].Stack;
					tmp.HasBeenInInventory = false;
					debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(tmp, new Vector2((float)x, (float)y), playerPosition)));
				}
			}
			foreach (Debris d in debrisToAdd)
			{
				this.debris.Add(d);
			}
			if (who.stats.Get("Book_Void") > 0U && Game1.random.NextDouble() < 0.03 && debrisToAdd != null && monster != null)
			{
				foreach (Debris d2 in debrisToAdd)
				{
					if (d2.item != null)
					{
						Item tmp2 = d2.item.getOne();
						if (tmp2 != null)
						{
							tmp2.Stack = d2.item.Stack;
							tmp2.HasBeenInInventory = false;
							this.debris.Add(monster.ModifyMonsterLoot(new Debris(tmp2, new Vector2((float)x, (float)y), playerPosition)));
						}
					}
					else if (d2.itemId.Value != null && d2.itemId.Value.Length > 0)
					{
						Item tmp3 = ItemRegistry.Create(d2.itemId.Value, 1, 0, false);
						tmp3.HasBeenInInventory = false;
						this.debris.Add(monster.ModifyMonsterLoot(new Debris(tmp3, new Vector2((float)x, (float)y), playerPosition)));
					}
				}
			}
			if (this.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.033)
			{
				Object o = this.tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					monster.ModifyMonsterLoot(Game1.createItemDebris(o, new Vector2((float)x, (float)y), -1, this, -1, false));
				}
			}
			Utility.trySpawnRareObject(who, new Vector2((float)x, (float)y), this, 1.5, 1.0, -1, null);
			if (Utility.tryRollMysteryBox(0.01 + who.team.AverageDailyLuck(null) / 10.0 + (double)who.LuckLevel * 0.008, null))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create((who.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 1, 0, false), new Vector2((float)x, (float)y), -1, this, -1, false));
			}
			if (who.stats.MonstersKilled > 10U && Game1.random.NextDouble() < 0.0001 + ((!who.mailReceived.Contains("voidBookDropped")) ? (who.stats.MonstersKilled * 1.5E-05) : 0.0004))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)Book_Void", 1, 0, false), new Vector2((float)x, (float)y), -1, this, -1, false));
				who.mailReceived.Add("voidBookDropped");
			}
			if (this is Woods && Game1.random.NextDouble() < 0.1)
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)292", 1, 0, false), new Vector2((float)x, (float)y), -1, this, -1, false));
			}
			if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100)
			{
				if (monster.isHardModeMonster.Value && Game1.stats.Get("hardModeMonstersKilled") > 50U && Game1.random.NextDouble() < 0.001 + (double)((float)who.LuckLevel * 0.0002f))
				{
					monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)896", 1, 0, false), new Vector2((float)x, (float)y), -1, this, -1, false));
					return;
				}
				if (monster.isHardModeMonster.Value && Game1.random.NextDouble() < 0.008 + (double)((float)who.LuckLevel * 0.002f))
				{
					monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)858", 1, 0, false), new Vector2((float)x, (float)y), -1, this, -1, false));
				}
			}
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x000A2CD4 File Offset: 0x000A0ED4
		public virtual bool HasUnlockedAreaSecretNotes(Farmer who)
		{
			return this.InIslandContext() || who.hasMagnifyingGlass;
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x000A2CE8 File Offset: 0x000A0EE8
		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, Farmer who, bool isProjectile = false)
		{
			return this.damageMonster(areaOfEffect, minDamage, maxDamage, isBomb, 1f, 0, 0f, 1f, false, who, isProjectile);
		}

		// Token: 0x06000EF0 RID: 3824 RVA: 0x000A2D18 File Offset: 0x000A0F18
		private bool isMonsterDamageApplicable(Farmer who, Monster monster, bool horizontalBias = true)
		{
			if (!monster.isGlider.Value && !(who.CurrentTool is Slingshot) && !monster.ignoreDamageLOS.Value)
			{
				Point farmerStandingPoint = who.TilePoint;
				Point monsterStandingPoint = monster.TilePoint;
				if (Math.Abs(farmerStandingPoint.X - monsterStandingPoint.X) + Math.Abs(farmerStandingPoint.Y - monsterStandingPoint.Y) > 1)
				{
					int xDif = monsterStandingPoint.X - farmerStandingPoint.X;
					int yDif = monsterStandingPoint.Y - farmerStandingPoint.Y;
					Vector2 pointInQuestion = new Vector2((float)farmerStandingPoint.X, (float)farmerStandingPoint.Y);
					while (xDif != 0 || yDif != 0)
					{
						if (horizontalBias)
						{
							if (Math.Abs(xDif) >= Math.Abs(yDif))
							{
								pointInQuestion.X += (float)Math.Sign(xDif);
								xDif -= Math.Sign(xDif);
							}
							else
							{
								pointInQuestion.Y += (float)Math.Sign(yDif);
								yDif -= Math.Sign(yDif);
							}
						}
						else if (Math.Abs(yDif) >= Math.Abs(xDif))
						{
							pointInQuestion.Y += (float)Math.Sign(yDif);
							yDif -= Math.Sign(yDif);
						}
						else
						{
							pointInQuestion.X += (float)Math.Sign(xDif);
							xDif -= Math.Sign(xDif);
						}
						Object obj;
						if ((this.objects.TryGetValue(pointInQuestion, out obj) && !obj.isPassable()) || this.BlocksDamageLOS((int)pointInQuestion.X, (int)pointInQuestion.Y))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x000A2E96 File Offset: 0x000A1096
		public virtual bool BlocksDamageLOS(int x, int y)
		{
			return this.hasTileAt(x, y, "Buildings", null) && this.doesTileHaveProperty(x, y, "Passable", "Buildings", false) == null;
		}

		// Token: 0x06000EF2 RID: 3826 RVA: 0x000A2EC0 File Offset: 0x000A10C0
		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, float knockBackModifier, int addedPrecision, float critChance, float critMultiplier, bool triggerMonsterInvincibleTimer, Farmer who, bool isProjectile = false)
		{
			bool didAnyDamage = false;
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				if (i < this.characters.Count)
				{
					Monster monster = this.characters[i] as Monster;
					if (monster != null && monster.IsMonster && monster.Health > 0 && monster.TakesDamageFromHitbox(areaOfEffect))
					{
						if (monster.currentLocation == null)
						{
							monster.currentLocation = this;
						}
						if (!monster.IsInvisible && !monster.isInvincible() && (isBomb || isProjectile || this.isMonsterDamageApplicable(who, monster, true) || this.isMonsterDamageApplicable(who, monster, false)))
						{
							if (isBomb)
							{
								goto IL_D7;
							}
							MeleeWeapon weapon = ((who != null) ? who.CurrentTool : null) as MeleeWeapon;
							if (weapon == null)
							{
								goto IL_D7;
							}
							bool flag = weapon.type.Value == 1;
							IL_D8:
							bool isDagger = flag;
							bool isDaggerSpecial = false;
							if (isDagger && MeleeWeapon.daggerHitsLeft > 1)
							{
								isDaggerSpecial = true;
							}
							if (isDaggerSpecial)
							{
								triggerMonsterInvincibleTimer = false;
							}
							didAnyDamage = true;
							if (Game1.currentLocation == this)
							{
								Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), (float)(200 + Game1.random.Next(-50, 50)));
							}
							Microsoft.Xna.Framework.Rectangle monsterBox = monster.GetBoundingBox();
							Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, who);
							if (knockBackModifier > 0f)
							{
								trajectory *= knockBackModifier;
							}
							else
							{
								trajectory = new Vector2(monster.xVelocity, monster.yVelocity);
							}
							if (monster.Slipperiness == -1)
							{
								trajectory = Vector2.Zero;
							}
							bool crit = false;
							if (((who != null) ? who.CurrentTool : null) != null && monster.hitWithTool(who.CurrentTool))
							{
								return false;
							}
							if (who.hasBuff("statue_of_blessings_5"))
							{
								critChance += 0.1f;
							}
							if (who.professions.Contains(25))
							{
								critChance += critChance * 0.5f;
							}
							if (maxDamage < 0)
							{
								goto IL_562;
							}
							int damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
							if (who != null && Game1.random.NextDouble() < (double)(critChance + (float)who.LuckLevel * (critChance / 40f)))
							{
								crit = true;
								this.playSound("crit", null, null, SoundContext.Default);
								if (who.hasTrinketWithID("IridiumSpur"))
								{
									BuffEffects b = new BuffEffects();
									b.Speed.Value = 1f;
									who.applyBuff(new Buff("iridiumspur", null, Game1.content.LoadString("Strings\\1_6_Strings:IridiumSpur_Name"), who.getFirstTrinketWithID("IridiumSpur").GetEffect().GeneralStat * 1000, Game1.objectSpriteSheet_2, 76, b, new bool?(false), null, null));
								}
							}
							damageAmount = (crit ? ((int)((float)damageAmount * critMultiplier)) : damageAmount);
							damageAmount = Math.Max(1, damageAmount + ((who != null) ? (who.Attack * 3) : 0));
							if (who != null && who.professions.Contains(24))
							{
								damageAmount = (int)Math.Ceiling((double)((float)damageAmount * 1.1f));
							}
							if (who != null && who.professions.Contains(26))
							{
								damageAmount = (int)Math.Ceiling((double)((float)damageAmount * 1.15f));
							}
							if (who != null && crit && who.professions.Contains(29))
							{
								damageAmount = (int)((float)damageAmount * 2f);
							}
							if (who != null)
							{
								foreach (BaseEnchantment baseEnchantment in who.enchantments)
								{
									baseEnchantment.OnCalculateDamage(monster, this, who, isBomb, ref damageAmount);
								}
							}
							damageAmount = monster.takeDamage(damageAmount, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
							if (isDaggerSpecial)
							{
								if (monster.stunTime.Value < 50)
								{
									monster.stunTime.Value = 50;
								}
							}
							else if (monster.stunTime.Value < 50)
							{
								monster.stunTime.Value = 0;
							}
							if (damageAmount == -1)
							{
								string missText = Game1.content.LoadString("Strings\\StringsFromCSFiles:Attack_Miss");
								this.debris.Add(new Debris(missText, 1, new Vector2((float)monsterBox.Center.X, (float)monsterBox.Center.Y), Color.LightGray, 1f, 0f));
							}
							else
							{
								this.removeDamageDebris(monster);
								this.debris.Add(new Debris(damageAmount, new Vector2((float)(monsterBox.Center.X + 16), (float)monsterBox.Center.Y), crit ? Color.Yellow : new Color(255, 130, 0), crit ? (1f + (float)damageAmount / 300f) : 1f, monster));
								if (who != null)
								{
									foreach (BaseEnchantment baseEnchantment2 in who.enchantments)
									{
										baseEnchantment2.OnDealtDamage(monster, this, who, isBomb, damageAmount);
									}
								}
							}
							if (triggerMonsterInvincibleTimer)
							{
								monster.setInvincibleCountdown(450 / (isDagger ? 3 : 2));
							}
							if (who != null)
							{
								using (NetList<Trinket, NetRef<Trinket>>.Enumerator enumerator2 = who.trinketItems.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										Trinket trinket = enumerator2.Current;
										if (trinket != null)
										{
											trinket.OnDamageMonster(who, monster, damageAmount, isBomb, crit);
										}
									}
									goto IL_59C;
								}
								goto IL_562;
							}
							IL_59C:
							string a;
							if (who == null)
							{
								a = null;
							}
							else
							{
								Tool currentTool = who.CurrentTool;
								a = ((currentTool != null) ? currentTool.QualifiedItemId : null);
							}
							if (a == "(W)4")
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(50, 120), 6, 1, new Vector2((float)(monsterBox.Center.X - 32), (float)(monsterBox.Center.Y - 32)), false, false)
								});
							}
							if (monster.Health <= 0)
							{
								this.onMonsterKilled(who, monster, monsterBox, isBomb);
								goto IL_92B;
							}
							if (damageAmount <= 0)
							{
								goto IL_92B;
							}
							monster.shedChunks(Game1.random.Next(1, 3));
							if (crit)
							{
								Vector2 standPos = monster.getStandingPosition();
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32f, 32f), false, Game1.random.NextBool())
									{
										scale = 0.75f,
										alpha = (crit ? 0.75f : 0.5f)
									}
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(15, 50), 6, 1, standPos - new Vector2((float)(32 + Game1.random.Next(-21, 21) + 32), (float)(32 + Game1.random.Next(-21, 21))), false, Game1.random.NextBool())
									{
										scale = 0.5f,
										delayBeforeAnimationStart = 50,
										alpha = (crit ? 0.75f : 0.5f)
									}
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(15, 50), 6, 1, standPos - new Vector2((float)(32 + Game1.random.Next(-21, 21) - 32), (float)(32 + Game1.random.Next(-21, 21))), false, Game1.random.NextBool())
									{
										scale = 0.5f,
										delayBeforeAnimationStart = 100,
										alpha = (crit ? 0.75f : 0.5f)
									}
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(15, 50), 6, 1, standPos - new Vector2((float)(32 + Game1.random.Next(-21, 21) + 32), (float)(32 + Game1.random.Next(-21, 21))), false, Game1.random.NextBool())
									{
										scale = 0.5f,
										delayBeforeAnimationStart = 150,
										alpha = (crit ? 0.75f : 0.5f)
									}
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite(362, (float)Game1.random.Next(15, 50), 6, 1, standPos - new Vector2((float)(32 + Game1.random.Next(-21, 21) - 32), (float)(32 + Game1.random.Next(-21, 21))), false, Game1.random.NextBool())
									{
										scale = 0.5f,
										delayBeforeAnimationStart = 200,
										alpha = (crit ? 0.75f : 0.5f)
									}
								});
								goto IL_92B;
							}
							goto IL_92B;
							IL_562:
							damageAmount = -2;
							monster.setTrajectory(trajectory);
							if (monster.Slipperiness > 10)
							{
								monster.xVelocity /= 2f;
								monster.yVelocity /= 2f;
								goto IL_59C;
							}
							goto IL_59C;
							IL_D7:
							flag = false;
							goto IL_D8;
						}
					}
				}
				IL_92B:;
			}
			return didAnyDamage;
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x000A382C File Offset: 0x000A1A2C
		private void onMonsterKilled(Farmer who, Monster monster, Microsoft.Xna.Framework.Rectangle monsterBox, bool killedByBomb)
		{
			bool isHutchSlime = false;
			bool isBabySlime = false;
			GreenSlime slime = monster as GreenSlime;
			if (slime != null)
			{
				isHutchSlime = (this is SlimeHutch);
				isBabySlime = !slime.firstGeneration.Value;
			}
			who.NotifyQuests((Quest quest) => quest.OnMonsterSlain(this, monster, killedByBomb, isHutchSlime, false), false);
			if (!isHutchSlime && Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					Action<Farmer, Monster> onMonsterSlain = specialOrder.onMonsterSlain;
					if (onMonsterSlain != null)
					{
						onMonsterSlain(Game1.player, monster);
					}
				}
			}
			if (who != null)
			{
				foreach (BaseEnchantment baseEnchantment in who.enchantments)
				{
					baseEnchantment.OnMonsterSlay(monster, this, who, killedByBomb);
				}
			}
			if (who != null)
			{
				Ring value = who.leftRing.Value;
				if (value != null)
				{
					value.onMonsterSlay(monster, this, who);
				}
			}
			if (who != null)
			{
				Ring value2 = who.rightRing.Value;
				if (value2 != null)
				{
					value2.onMonsterSlay(monster, this, who);
				}
			}
			if (who != null && !isHutchSlime && !isBabySlime)
			{
				if (who.IsLocalPlayer)
				{
					Game1.stats.monsterKilled(monster.Name);
				}
				else if (Game1.IsMasterGame)
				{
					who.queueMessage(25, Game1.player, new object[]
					{
						monster.Name
					});
				}
			}
			if (monster.isHardModeMonster.Value)
			{
				Game1.stats.Increment("hardModeMonstersKilled", 1U);
			}
			Stats stats = Game1.stats;
			uint monstersKilled = stats.MonstersKilled;
			stats.MonstersKilled = monstersKilled + 1U;
			this.monsterDrop(monster, monsterBox.Center.X, monsterBox.Center.Y, who);
			if (!isHutchSlime && who != null)
			{
				who.gainExperience(4, this.isFarm.Value ? Math.Max(1, monster.ExperienceGained / 3) : monster.ExperienceGained);
			}
			if (monster.ShouldMonsterBeRemoved())
			{
				this.characters.Remove(monster);
			}
			this.removeTemporarySpritesWithID((int)(monster.position.X * 777f + monster.position.Y * 77777f));
			MeleeWeapon weapon = ((who != null) ? who.CurrentTool : null) as MeleeWeapon;
			if (weapon != null && (weapon.QualifiedItemId == "(W)65" || (weapon.appearance.Value != null && weapon.appearance.Value.Equals("(W)65"))))
			{
				Utility.addRainbowStarExplosion(this, new Vector2((float)(monsterBox.Center.X - 32), (float)(monsterBox.Center.Y - 32)), Game1.random.Next(6, 9));
			}
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x000A3B78 File Offset: 0x000A1D78
		public void growWeedGrass(int iterations)
		{
			for (int i = 0; i < iterations; i++)
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> pair in this.terrainFeatures.Pairs.ToArray<KeyValuePair<Vector2, TerrainFeature>>())
				{
					Grass grass = pair.Value as Grass;
					if (grass != null && Game1.random.NextDouble() < 0.65)
					{
						if (grass.numberOfWeeds.Value < 4)
						{
							grass.numberOfWeeds.Value = Math.Max(0, Math.Min(4, grass.numberOfWeeds.Value + Game1.random.Next(3)));
						}
						else if (grass.numberOfWeeds.Value >= 4)
						{
							int xCoord = (int)pair.Key.X;
							int yCoord = (int)pair.Key.Y;
							foreach (Vector2 tile in Utility.getAdjacentTileLocationsArray(pair.Key))
							{
								if (this.isTileOnMap(xCoord, yCoord) && !this.IsTileBlockedBy(tile, CollisionMask.All, CollisionMask.None, false) && this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back", false) != null && !this.IsNoSpawnTile(tile, "All", false) && Game1.random.NextDouble() < 0.25)
								{
									this.terrainFeatures.Add(tile, new Grass((int)grass.grassType.Value, Game1.random.Next(1, 3)));
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x000A3D2E File Offset: 0x000A1F2E
		public bool tryPlaceObject(Vector2 tile, Object o)
		{
			if (this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
			{
				o.initializeLightSource(tile, false);
				this.objects.Add(tile, o);
				return true;
			}
			return false;
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x000A3D60 File Offset: 0x000A1F60
		public void removeDamageDebris(Monster monster)
		{
			this.debris.RemoveWhere((Debris d) => d.toHover != null && d.toHover.Equals(monster) && !d.nonSpriteChunkColor.Equals(Color.Yellow) && d.timeSinceDoneBouncing > 900f);
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x000A3D94 File Offset: 0x000A1F94
		public void spawnWeeds(bool weedsOnly)
		{
			LocationData data = this.GetData();
			int numberOfNewWeeds = Game1.random.Next((data != null) ? data.MinDailyWeeds : 1, ((data != null) ? data.MaxDailyWeeds : 5) + 1);
			if (Game1.dayOfMonth == 1 && Game1.IsSpring)
			{
				numberOfNewWeeds *= ((data != null) ? data.FirstDayWeedMultiplier : 15);
			}
			for (int i = 0; i < numberOfNewWeeds; i++)
			{
				for (int numberOfTries = 0; numberOfTries < 3; numberOfTries++)
				{
					int xCoord = Game1.random.Next(this.map.DisplayWidth / 64);
					int yCoord = Game1.random.Next(this.map.DisplayHeight / 64);
					Vector2 location = new Vector2((float)xCoord, (float)yCoord);
					Object o;
					this.objects.TryGetValue(location, out o);
					int grass = -1;
					int tree = -1;
					if (Game1.random.NextDouble() < 0.15 + (weedsOnly ? 0.05 : 0.0))
					{
						grass = 1;
					}
					else if (!weedsOnly)
					{
						if (Game1.random.NextDouble() < 0.35)
						{
							tree = 1;
						}
						else if (!this.isFarm.Value && Game1.random.NextDouble() < 0.35)
						{
							tree = 2;
						}
					}
					if (tree != -1)
					{
						if (this is Farm && Game1.random.NextDouble() < 0.25)
						{
							return;
						}
					}
					else if (o == null && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back", false) != null && this.isTileLocationOpen(new Location(xCoord, yCoord)) && !this.IsTileOccupiedBy(location, CollisionMask.All, CollisionMask.None, false) && !this.isWaterTile(xCoord, yCoord))
					{
						if (this.IsNoSpawnTile(location, "Grass", false))
						{
							continue;
						}
						if (grass != -1 && this.GetSeason() != Season.Winter && this.name.Value == "Farm")
						{
							if (Game1.GetFarmTypeID() == "MeadowlandsFarm" && Game1.random.NextDouble() < 0.1)
							{
								grass = 7;
							}
							int numberOfWeeds = Game1.random.Next(1, 3);
							this.terrainFeatures.Add(location, new Grass(grass, numberOfWeeds));
						}
					}
				}
			}
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x000A3FDE File Offset: 0x000A21DE
		public virtual void OnMiniJukeboxAdded()
		{
			this.miniJukeboxCount.Value = this.miniJukeboxCount.Value + 1;
			this.UpdateMiniJukebox();
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x000A3FFE File Offset: 0x000A21FE
		public virtual void OnMiniJukeboxRemoved()
		{
			this.miniJukeboxCount.Value = this.miniJukeboxCount.Value - 1;
			this.UpdateMiniJukebox();
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x000A401E File Offset: 0x000A221E
		public virtual void UpdateMiniJukebox()
		{
			if (this.miniJukeboxCount.Value <= 0)
			{
				this.miniJukeboxCount.Set(0);
				this.miniJukeboxTrack.Set("");
			}
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x000A404C File Offset: 0x000A224C
		public virtual bool IsMiniJukeboxPlaying()
		{
			return this.miniJukeboxCount.Value > 0 && this.miniJukeboxTrack.Value != "" && (!this.IsOutdoors || !this.IsRainingHere()) && !Game1.isGreenRain;
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x000A4098 File Offset: 0x000A2298
		public virtual void DayUpdate(int dayOfMonth)
		{
			this.isMusicTownMusic = null;
			this.netAudio.StopPlaying("fuse");
			this.SelectRandomMiniJukeboxTrack();
			List<Critter> list = this.critters;
			if (list != null)
			{
				list.Clear();
			}
			this.characters.RemoveWhere(delegate(NPC npc)
			{
				if (!(npc is JunimoHarvester))
				{
					Monster monster = npc as Monster;
					return monster != null && monster.wildernessFarmMonster;
				}
				return true;
			});
			FarmAnimal[] array = this.animals.Values.ToArray<FarmAnimal>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].dayUpdate(this);
			}
			for (int i = this.debris.Count - 1; i >= 0; i--)
			{
				Debris d2 = this.debris[i];
				if (d2.isEssentialItem() && Game1.IsMasterGame)
				{
					Item item2 = d2.item;
					if (((item2 != null) ? item2.QualifiedItemId : null) == "(O)73")
					{
						d2.collect(Game1.player, null);
					}
					else
					{
						Item item = d2.item;
						d2.item = null;
						Game1.player.team.returnedDonations.Add(item);
						Game1.player.team.newLostAndFoundItems.Value = true;
					}
					this.debris.RemoveAt(i);
				}
			}
			this.updateMap();
			this.temporarySprites.Clear();
			KeyValuePair<Vector2, TerrainFeature>[] map_features = this.terrainFeatures.Pairs.ToArray<KeyValuePair<Vector2, TerrainFeature>>();
			foreach (KeyValuePair<Vector2, TerrainFeature> pair5 in map_features)
			{
				if (!this.isTileOnMap(pair5.Key))
				{
					this.terrainFeatures.Remove(pair5.Key);
				}
				else
				{
					pair5.Value.dayUpdate();
				}
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in map_features)
			{
				HoeDirt hoe_dirt = pair2.Value as HoeDirt;
				if (hoe_dirt != null)
				{
					hoe_dirt.updateNeighbors();
				}
			}
			if (this.largeTerrainFeatures != null)
			{
				LargeTerrainFeature[] array3 = this.largeTerrainFeatures.ToArray<LargeTerrainFeature>();
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j].dayUpdate();
				}
			}
			this.objects.Lock();
			foreach (KeyValuePair<Vector2, Object> pair3 in this.objects.Pairs)
			{
				pair3.Value.DayUpdate();
				if (pair3.Value.destroyOvernight)
				{
					pair3.Value.performRemoveAction();
					this.objects.Remove(pair3.Key);
				}
			}
			this.objects.Unlock();
			this.RespawnStumpsFromMapProperty();
			if (!(this is FarmHouse))
			{
				this.debris.RemoveWhere((Debris d) => d.item == null && d.itemId.Value == null);
			}
			if (this.map != null && (this.isOutdoors.Value || this.map.Properties.ContainsKey("ForceSpawnForageables")) && !this.map.Properties.ContainsKey("skipWeedGrowth"))
			{
				if (Game1.dayOfMonth % 7 == 0 && !(this is Farm))
				{
					Microsoft.Xna.Framework.Rectangle ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0);
					if (this is IslandWest)
					{
						ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(31, 3, 77, 70);
					}
					foreach (KeyValuePair<Vector2, Object> pair4 in this.objects.Pairs.ToArray<KeyValuePair<Vector2, Object>>())
					{
						if (pair4.Value.isSpawnedObject.Value && pair4.Value.SpecialVariable != 724519 && !ignoreRectangle.Contains(Utility.Vector2ToPoint(pair4.Key)))
						{
							this.objects.Remove(pair4.Key);
						}
					}
					this.numberOfSpawnedObjectsOnMap = 0;
					this.spawnObjects();
					this.spawnObjects();
				}
				this.spawnObjects();
				if (Game1.dayOfMonth == 1)
				{
					this.spawnObjects();
				}
				if (Game1.stats.DaysPlayed < 4U)
				{
					this.spawnObjects();
				}
				Layer pathsLayer = this.map.GetLayer("Paths");
				if (pathsLayer != null && !(this is Farm))
				{
					for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
					{
						for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
						{
							string treeId;
							int? growthStageOnLoad;
							int? growthStageOnRegrow;
							bool isFruitTree;
							if (this.TryGetTreeIdForTile(pathsLayer.Tiles[x, y], out treeId, out growthStageOnLoad, out growthStageOnRegrow, out isFruitTree) && Game1.random.NextBool())
							{
								Vector2 tile = new Vector2((float)x, (float)y);
								if (this.GetFurnitureAt(tile) == null && !this.terrainFeatures.ContainsKey(tile) && !this.objects.ContainsKey(tile) && this.getBuildingAt(tile) == null)
								{
									if (isFruitTree)
									{
										this.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnRegrow.GetValueOrDefault(2)));
									}
									else
									{
										this.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnRegrow.GetValueOrDefault(2), false));
									}
								}
							}
						}
					}
				}
			}
			this.terrainFeatures.RemoveWhere(delegate(KeyValuePair<Vector2, TerrainFeature> pair)
			{
				HoeDirt dirt = pair.Value as HoeDirt;
				Object tileObj;
				return dirt != null && (dirt.crop == null || dirt.crop.forageCrop.Value) && (!this.objects.TryGetValue(pair.Key, out tileObj) || tileObj == null || !tileObj.IsSpawnedObject || !tileObj.isForage()) && Game1.random.NextBool(this.GetDirtDecayChance(pair.Key));
			});
			this.lightLevel.Value = 0f;
			foreach (Furniture furniture in this.furniture)
			{
				furniture.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay));
				furniture.DayUpdate();
			}
			this.addLightGlows();
			if (!(this is Farm))
			{
				this.HandleGrassGrowth(dayOfMonth);
			}
			foreach (Building building2 in this.buildings)
			{
				building2.dayUpdate(dayOfMonth);
			}
			foreach (string builder in new List<string>(Game1.netWorldState.Value.Builders.Keys))
			{
				BuilderData builderData = Game1.netWorldState.Value.Builders[builder];
				if (builderData.buildingLocation.Value == this.NameOrUniqueName)
				{
					Building building = this.getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
					if (building == null || (building.daysUntilUpgrade.Value == 0 && building.daysOfConstructionLeft.Value == 0))
					{
						Game1.netWorldState.Value.Builders.Remove(builder);
					}
					else
					{
						Game1.netWorldState.Value.MarkUnderConstruction(builder, building);
					}
				}
			}
			if (dayOfMonth == 9 && this.Name.Equals("Backwoods"))
			{
				if (this.terrainFeatures.GetValueOrDefault(new Vector2(18f, 18f), null) is HoeDirt)
				{
					this.terrainFeatures.Remove(new Vector2(18f, 18f));
				}
				this.tryPlaceObject(new Vector2(18f, 18f), ItemRegistry.Create<Object>("(O)SeedSpot", 1, 0, false));
			}
			this.fishSplashPointTime = 0;
			this.fishFrenzyFish.Value = "";
			this.fishSplashPoint.Value = Point.Zero;
			this.orePanPoint.Value = Point.Zero;
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x000A4850 File Offset: 0x000A2A50
		public virtual double GetDirtDecayChance(Vector2 tile)
		{
			double chance;
			if (this.TryGetMapPropertyAs("DirtDecayChance", out chance, false))
			{
				return chance;
			}
			if (this.IsGreenhouse)
			{
				return 0.0;
			}
			if (this is Farm || this is IslandWest || this.isFarm.Value)
			{
				return 0.1;
			}
			return 1.0;
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x000A48B4 File Offset: 0x000A2AB4
		public void RespawnStumpsFromMapProperty()
		{
			string[] stumpData = this.GetMapPropertySplitBySpaces("Stumps");
			for (int i = 0; i < stumpData.Length; i += 3)
			{
				Vector2 tile;
				string error;
				if (!ArgUtility.TryGetVector2(stumpData, i, out tile, out error, false, "Vector2 tile"))
				{
					this.LogMapPropertyError("Stumps", stumpData, error, ' ');
				}
				else
				{
					bool foundStump = false;
					using (List<ResourceClump>.Enumerator enumerator = this.resourceClumps.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.Tile == tile)
							{
								foundStump = true;
								break;
							}
						}
					}
					if (!foundStump)
					{
						this.resourceClumps.Add(new ResourceClump(600, 2, 2, tile, null, null));
						this.removeObject(tile, false);
						this.removeObject(tile + new Vector2(1f, 0f), false);
						this.removeObject(tile + new Vector2(1f, 1f), false);
						this.removeObject(tile + new Vector2(0f, 1f), false);
					}
				}
			}
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x000A49E8 File Offset: 0x000A2BE8
		public void addLightGlows()
		{
			int night_tiles_time = Game1.getTrulyDarkTime(this) - 100;
			if (!this.isOutdoors.Value && (Game1.timeOfDay < night_tiles_time || Game1.newDay))
			{
				this.lightGlows.Clear();
				string[] split = this.GetMapPropertySplitBySpaces("DayTiles");
				for (int i = 0; i < split.Length; i += 4)
				{
					string layerId;
					string error;
					Vector2 position;
					int tileIndex;
					if (!ArgUtility.TryGet(split, i, out layerId, out error, true, "string layerId") || !ArgUtility.TryGetVector2(split, i + 1, out position, out error, false, "Vector2 position") || !ArgUtility.TryGetInt(split, i + 3, out tileIndex, out error, "int tileIndex"))
					{
						this.LogMapPropertyError("DayTiles", split, error, ' ');
					}
					else
					{
						Tile tile = this.map.RequireLayer(layerId).Tiles[(int)position.X, (int)position.Y];
						if (tile != null)
						{
							tile.TileIndex = tileIndex;
							if (tileIndex <= 257)
							{
								if (tileIndex != 256)
								{
									if (tileIndex == 257)
									{
										this.lightGlows.Add(position * 64f + new Vector2(32f, -4f));
									}
								}
								else
								{
									this.lightGlows.Add(position * 64f + new Vector2(32f, 64f));
								}
							}
							else if (tileIndex != 405)
							{
								if (tileIndex != 469)
								{
									if (tileIndex == 1224)
									{
										this.lightGlows.Add(position * 64f + new Vector2(32f, 32f));
									}
								}
								else
								{
									this.lightGlows.Add(position * 64f + new Vector2(32f, 36f));
								}
							}
							else
							{
								this.lightGlows.Add(position * 64f + new Vector2(32f, 32f));
								this.lightGlows.Add(position * 64f + new Vector2(96f, 32f));
							}
						}
					}
				}
			}
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x000A4C34 File Offset: 0x000A2E34
		public NPC isCharacterAtTile(Vector2 tileLocation)
		{
			NPC c = null;
			tileLocation.X = (float)((int)tileLocation.X);
			tileLocation.Y = (float)((int)tileLocation.Y);
			Microsoft.Xna.Framework.Rectangle tileBoundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			if (this.currentEvent == null)
			{
				using (List<NPC>.Enumerator enumerator = this.characters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NPC i = enumerator.Current;
						if (i.GetBoundingBox().Intersects(tileBoundingBox))
						{
							return i;
						}
					}
					return c;
				}
			}
			foreach (NPC j in this.currentEvent.actors)
			{
				if (j.GetBoundingBox().Intersects(tileBoundingBox))
				{
					return j;
				}
			}
			return c;
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x000A4D3C File Offset: 0x000A2F3C
		public void ResetCharacterDialogues()
		{
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				this.characters[i].resetCurrentDialogue();
			}
		}

		// Token: 0x06000F02 RID: 3842 RVA: 0x000A4D74 File Offset: 0x000A2F74
		public string getMapProperty(string propertyName)
		{
			string value;
			if (!this.TryGetMapProperty(propertyName, out value))
			{
				return null;
			}
			return value;
		}

		// Token: 0x06000F03 RID: 3843 RVA: 0x000A4D90 File Offset: 0x000A2F90
		public bool TryGetMapProperty(string propertyName, out string propertyValue)
		{
			Map map = this.Map;
			if (map == null)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't read map property '");
				defaultInterpolatedStringHandler.AppendFormatted(propertyName);
				defaultInterpolatedStringHandler.AppendLiteral("' for location '");
				defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral("' because the map is null.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				propertyValue = null;
				return false;
			}
			return map.Properties.TryGetValue(propertyName, out propertyValue) && propertyValue != null;
		}

		// Token: 0x06000F04 RID: 3844 RVA: 0x000A4E18 File Offset: 0x000A3018
		public string[] GetMapPropertySplitBySpaces(string propertyName)
		{
			string value;
			if (!this.TryGetMapProperty(propertyName, out value) || value == null)
			{
				return LegacyShims.EmptyArray<string>();
			}
			return ArgUtility.SplitBySpace(value);
		}

		// Token: 0x06000F05 RID: 3845 RVA: 0x000A4E40 File Offset: 0x000A3040
		public bool TryGetMapPropertyAs(string key, out bool parsed, bool required = false)
		{
			string raw;
			if (!this.TryGetMapProperty(key, out raw))
			{
				if (required)
				{
					this.LogMapPropertyError(key, "", "required map property isn't defined");
				}
				parsed = false;
				return false;
			}
			if (raw == "T" || raw == "t")
			{
				parsed = true;
				return true;
			}
			if (raw == "F" || raw == "f")
			{
				parsed = false;
				return true;
			}
			if (bool.TryParse(raw, out parsed))
			{
				return true;
			}
			this.LogMapPropertyError(key, raw, "not a valid boolean value");
			return false;
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x000A4ECC File Offset: 0x000A30CC
		public bool TryGetMapPropertyAs(string key, out double parsed, bool required = false)
		{
			string raw;
			if (!this.TryGetMapProperty(key, out raw))
			{
				if (required)
				{
					this.LogMapPropertyError(key, "", "required map property isn't defined");
				}
				parsed = 0.0;
				return false;
			}
			if (!double.TryParse(raw, out parsed))
			{
				this.LogMapPropertyError(key, raw, "value '" + raw + "' can't be parsed as a decimal value");
				return false;
			}
			return true;
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x000A4F2C File Offset: 0x000A312C
		public bool TryGetMapPropertyAs(string key, out Point parsed, bool required = false)
		{
			string[] fields = this.GetMapPropertySplitBySpaces(key);
			if (fields.Length == 0)
			{
				if (required)
				{
					this.LogMapPropertyError(key, "", "required map property isn't defined");
				}
				parsed = Point.Zero;
				return false;
			}
			string error;
			if (!ArgUtility.TryGetPoint(fields, 0, out parsed, out error, "parsed"))
			{
				this.LogMapPropertyError(key, fields, error, ' ');
				parsed = Point.Zero;
				return false;
			}
			return true;
		}

		// Token: 0x06000F08 RID: 3848 RVA: 0x000A4F90 File Offset: 0x000A3190
		public bool TryGetMapPropertyAs(string key, out Vector2 parsed, bool required = false)
		{
			string[] fields = this.GetMapPropertySplitBySpaces(key);
			if (fields.Length == 0)
			{
				if (required)
				{
					this.LogMapPropertyError(key, "", "required map property isn't defined");
				}
				parsed = Vector2.Zero;
				return false;
			}
			string error;
			if (!ArgUtility.TryGetVector2(fields, 0, out parsed, out error, false, "parsed"))
			{
				this.LogMapPropertyError(key, fields, error, ' ');
				parsed = Vector2.Zero;
				return false;
			}
			return true;
		}

		// Token: 0x06000F09 RID: 3849 RVA: 0x000A4FF8 File Offset: 0x000A31F8
		public bool TryGetMapPropertyAs(string key, out Microsoft.Xna.Framework.Rectangle parsed, bool required = false)
		{
			string[] fields = this.GetMapPropertySplitBySpaces(key);
			if (fields.Length == 0)
			{
				if (required)
				{
					this.LogMapPropertyError(key, "", "required map property isn't defined");
				}
				parsed = Microsoft.Xna.Framework.Rectangle.Empty;
				return false;
			}
			string error;
			if (!ArgUtility.TryGetRectangle(fields, 0, out parsed, out error, "parsed"))
			{
				this.LogMapPropertyError(key, fields, error, ' ');
				parsed = Microsoft.Xna.Framework.Rectangle.Empty;
				return false;
			}
			return true;
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x000A505C File Offset: 0x000A325C
		public bool HasMapPropertyWithValue(string propertyName)
		{
			string rawValue;
			return this.map != null && this.Map.Properties.TryGetValue(propertyName, out rawValue) && rawValue != null && rawValue.Length > 0;
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x000A5098 File Offset: 0x000A3298
		public virtual void tryToAddCritters(bool onlyIfOnScreen = false)
		{
			if (Game1.CurrentEvent != null)
			{
				return;
			}
			double mapArea = (double)(this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight);
			double baseChance = Math.Max(0.15, Math.Min(0.5, mapArea / 15000.0));
			double birdieChance = baseChance;
			double butterflyChance = baseChance;
			double bunnyChance = baseChance / 2.0;
			double squirrelChance = baseChance / 2.0;
			double woodPeckerChance = baseChance / 8.0;
			double cloudChange = baseChance * 2.0;
			if (!this.IsRainingHere())
			{
				this.addClouds(cloudChange / (double)(onlyIfOnScreen ? 2f : 1f), onlyIfOnScreen);
				if (this is Beach || this.critters == null)
				{
					return;
				}
				if (this.critters.Count > (this.IsSummerHere() ? 20 : 10))
				{
					return;
				}
				this.addBirdies(birdieChance, onlyIfOnScreen);
				this.addButterflies(butterflyChance, onlyIfOnScreen);
				this.addBunnies(bunnyChance, onlyIfOnScreen);
				this.addSquirrels(squirrelChance, onlyIfOnScreen);
				this.addWoodpecker(woodPeckerChance, onlyIfOnScreen);
				if (Game1.isDarkOut(this) && Game1.random.NextDouble() < 0.01)
				{
					this.addOwl();
				}
				if (Game1.isDarkOut(this))
				{
					this.addOpossums(baseChance / 10.0, onlyIfOnScreen);
				}
			}
		}

		// Token: 0x06000F0C RID: 3852 RVA: 0x000A51F8 File Offset: 0x000A33F8
		public void addClouds(double chance, bool onlyIfOnScreen = false)
		{
			if (this.IsSummerHere() && !this.IsRainingHere() && Game1.weatherIcon != 4 && Game1.timeOfDay < Game1.getStartingToGetDarkTime(this) - 100)
			{
				while (Game1.random.NextDouble() < Math.Min(0.9, chance))
				{
					Vector2 v = this.getRandomTile(null);
					if (onlyIfOnScreen)
					{
						v = (Game1.random.NextBool() ? new Vector2((float)this.map.Layers[0].LayerWidth, (float)Game1.random.Next(this.map.Layers[0].LayerHeight)) : new Vector2((float)Game1.random.Next(this.map.Layers[0].LayerWidth), (float)this.map.Layers[0].LayerHeight));
					}
					if (onlyIfOnScreen || !Utility.isOnScreen(v * 64f, 1280))
					{
						Cloud cloud = new Cloud(v);
						bool freeToAdd = true;
						if (this.critters != null)
						{
							foreach (Critter c in this.critters)
							{
								if (c is Cloud && c.getBoundingBox(0, 0).Intersects(cloud.getBoundingBox(0, 0)))
								{
									freeToAdd = false;
									break;
								}
							}
						}
						if (freeToAdd)
						{
							this.addCritter(cloud);
						}
					}
				}
			}
		}

		// Token: 0x06000F0D RID: 3853 RVA: 0x000A5394 File Offset: 0x000A3594
		public void addOwl()
		{
			this.critters.Add(new Owl(new Vector2((float)Game1.random.Next(64, this.map.Layers[0].LayerWidth * 64 - 64), -128f)));
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x000A53E4 File Offset: 0x000A35E4
		public void setFireplace(bool on, int tileLocationX, int tileLocationY, bool playSound = true, int xOffset = 0, int yOffset = 0)
		{
			int fireid = 944468 + tileLocationX * 1000 + tileLocationY;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 3);
			defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("_Fireplace_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(tileLocationX);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(tileLocationY);
			string lightSourceId = defaultInterpolatedStringHandler.ToStringAndClear();
			if (on)
			{
				if (this.getTemporarySpriteByID(fireid) == null)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2((float)tileLocationX, (float)tileLocationY) * 64f + new Vector2(32f, -32f) + new Vector2((float)xOffset, (float)yOffset), false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						lightId = lightSourceId + "_1",
						id = fireid,
						lightRadius = 2f,
						scale = 4f,
						layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2((float)(tileLocationX + 1), (float)tileLocationY) * 64f + new Vector2(-16f, -32f) + new Vector2((float)xOffset, (float)yOffset), false, 0f, Color.White)
					{
						delayBeforeAnimationStart = 10,
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						lightId = lightSourceId + "_2",
						id = fireid,
						lightRadius = 2f,
						scale = 4f,
						layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
					});
					if (playSound && Game1.gameMode != 6)
					{
						this.localSound("fireball", null, null, SoundContext.Default);
					}
					AmbientLocationSounds.addSound(new Vector2((float)tileLocationX, (float)tileLocationY), 1);
					return;
				}
			}
			else
			{
				this.removeTemporarySpritesWithID(fireid);
				Game1.currentLightSources.Remove(lightSourceId + "_1");
				Game1.currentLightSources.Remove(lightSourceId + "_2");
				if (playSound)
				{
					this.localSound("fireball", null, null, SoundContext.Default);
				}
				AmbientLocationSounds.removeSound(new Vector2((float)tileLocationX, (float)tileLocationY));
			}
		}

		// Token: 0x06000F0F RID: 3855 RVA: 0x000A56A4 File Offset: 0x000A38A4
		public void addWoodpecker(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut(this))
			{
				return;
			}
			if (onlyIfOnScreen || this is Town || this is Desert)
			{
				return;
			}
			if (Game1.random.NextDouble() < chance && this.terrainFeatures.Length > 0)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 tile;
					TerrainFeature feature;
					if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(this.terrainFeatures, out tile, out feature, null))
					{
						Tree tree = feature as Tree;
						if (tree != null)
						{
							WildTreeData data = tree.GetData();
							if (data != null && data.AllowWoodpeckers && tree.growthStage.Value >= 5)
							{
								this.critters.Add(new Woodpecker(tree, tile));
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000F10 RID: 3856 RVA: 0x000A5748 File Offset: 0x000A3948
		public void addSquirrels(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut(this))
			{
				return;
			}
			if (onlyIfOnScreen || this is Farm || this is Town || this is Desert)
			{
				return;
			}
			if (Game1.random.NextDouble() < chance && this.terrainFeatures.Length > 0)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 pos;
					TerrainFeature feature;
					if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(this.terrainFeatures, out pos, out feature, null))
					{
						Tree tree = feature as Tree;
						if (tree != null && tree.growthStage.Value >= 5 && !tree.stump.Value)
						{
							int distance = Game1.random.Next(4, 7);
							bool flip = Game1.random.NextBool();
							bool success = true;
							for (int j = 0; j < distance; j++)
							{
								pos.X += (float)(flip ? 1 : -1);
								if (!this.CanSpawnCharacterHere(pos))
								{
									success = false;
									break;
								}
							}
							if (success)
							{
								this.critters.Add(new Squirrel(pos, flip));
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000F11 RID: 3857 RVA: 0x000A5854 File Offset: 0x000A3A54
		public void addBunnies(double chance, bool onlyIfOnScreen = false)
		{
			if (onlyIfOnScreen || this is Farm || this is Desert)
			{
				return;
			}
			if (Game1.random.NextDouble() < chance && this.largeTerrainFeatures != null)
			{
				for (int i = 0; i < 3; i++)
				{
					int index = Game1.random.Next(this.largeTerrainFeatures.Count);
					if (this.largeTerrainFeatures.Count > 0 && this.largeTerrainFeatures[index] is Bush)
					{
						Vector2 pos = this.largeTerrainFeatures[index].Tile;
						int distance = Game1.random.Next(5, 12);
						bool flip = Game1.random.NextBool();
						bool success = true;
						for (int j = 0; j < distance; j++)
						{
							pos.X += (float)(flip ? 1 : -1);
							if (!this.largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)pos.X * 64, (int)pos.Y * 64, 64, 64)) && !this.CanSpawnCharacterHere(pos))
							{
								success = false;
								break;
							}
						}
						if (success)
						{
							this.critters.Add(new Rabbit(this, pos, flip));
							return;
						}
					}
				}
			}
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x000A5994 File Offset: 0x000A3B94
		public void addOpossums(double chance, bool onlyIfOnScreen = false)
		{
			if (onlyIfOnScreen || this is Farm || this is Desert)
			{
				return;
			}
			if (Game1.random.NextDouble() < chance && this.largeTerrainFeatures != null)
			{
				for (int i = 0; i < 3; i++)
				{
					int index = Game1.random.Next(this.largeTerrainFeatures.Count);
					if (this.largeTerrainFeatures.Count > 0 && this.largeTerrainFeatures[index] is Bush)
					{
						Vector2 pos = this.largeTerrainFeatures[index].Tile;
						int distance = Game1.random.Next(5, 12);
						bool flip = Game1.player.Position.X > (float)((this is BusStop) ? 704 : 64);
						bool success = true;
						for (int j = 0; j < distance; j++)
						{
							pos.X += (float)(flip ? 1 : -1);
							if (!this.largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)pos.X * 64, (int)pos.Y * 64, 64, 64)) && !this.CanSpawnCharacterHere(pos))
							{
								success = false;
								break;
							}
						}
						if (success)
						{
							if (this is BusStop && Game1.random.NextDouble() < 0.5)
							{
								pos = new Vector2((float)((Game1.player.Tile.X < 26f) ? 36 : 16), (float)(23 + Game1.random.Next(2)));
							}
							this.critters.Add(new Opossum(this, pos, flip));
							return;
						}
					}
				}
			}
		}

		// Token: 0x06000F13 RID: 3859 RVA: 0x000A5B3B File Offset: 0x000A3D3B
		public void instantiateCrittersList()
		{
			if (this.critters == null)
			{
				this.critters = new List<Critter>();
			}
		}

		// Token: 0x06000F14 RID: 3860 RVA: 0x000A5B50 File Offset: 0x000A3D50
		public void addCritter(Critter c)
		{
			List<Critter> list = this.critters;
			if (list == null)
			{
				return;
			}
			list.Add(c);
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x000A5B64 File Offset: 0x000A3D64
		public void addButterflies(double chance, bool onlyIfOnScreen = false)
		{
			Season season = this.GetSeason();
			bool island_location = this.InIslandContext();
			bool firefly = season == Season.Summer && Game1.isDarkOut(this);
			if (Game1.timeOfDay >= 1500 && !firefly && season != Season.Winter)
			{
				return;
			}
			if (season == Season.Spring || season == Season.Summer || (season == Season.Winter && Game1.dayOfMonth % 7 == 0 && Game1.isDarkOut(this)))
			{
				chance = Math.Min(0.8, chance * 1.5);
				while (Game1.random.NextDouble() < chance)
				{
					Vector2 v = this.getRandomTile(null);
					if (!onlyIfOnScreen || !Utility.isOnScreen(v * 64f, 64))
					{
						if (firefly)
						{
							this.critters.Add(new Firefly(v));
						}
						else
						{
							this.critters.Add(new Butterfly(this, v, island_location, false, -1, false));
						}
						while (Game1.random.NextDouble() < 0.4)
						{
							if (firefly)
							{
								this.critters.Add(new Firefly(v + new Vector2((float)Game1.random.Next(-2, 3), (float)Game1.random.Next(-2, 3))));
							}
							else
							{
								this.critters.Add(new Butterfly(this, v + new Vector2((float)Game1.random.Next(-2, 3), (float)Game1.random.Next(-2, 3)), island_location, false, -1, false));
							}
						}
					}
				}
			}
			if (Game1.timeOfDay < 1700)
			{
				this.tryAddPrismaticButterfly();
			}
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x000A5CEC File Offset: 0x000A3EEC
		public void tryAddPrismaticButterfly()
		{
			if (Game1.player.hasBuff("statue_of_blessings_6"))
			{
				foreach (Critter critter in this.critters)
				{
					Butterfly b = critter as Butterfly;
					if (b != null && b.isPrismatic)
					{
						return;
					}
				}
				Random r = Utility.CreateDaySaveRandom((double)(Game1.player.UniqueMultiplayerID % 10000L), 0.0, 0.0);
				string[] possibleLocations = new string[]
				{
					"Forest",
					"Town",
					"Beach",
					"Mountain",
					"Woods",
					"BusStop",
					"Backwoods"
				};
				string locationChoice = possibleLocations[r.Next(possibleLocations.Length)];
				if (locationChoice.Equals("Beach") && this.Name.Equals("BeachNightMarket"))
				{
					locationChoice = "BeachNightMarket";
				}
				if (this.Name.Equals(locationChoice))
				{
					Vector2 prism_v = this.getRandomTile(r);
					int i = 0;
					while (i < 32 && !this.isTileLocationOpen(prism_v))
					{
						prism_v = this.getRandomTile(r);
						i++;
					}
					this.critters.Add(new Butterfly(this, prism_v, false, false, 394, true)
					{
						stayInbounds = true
					});
				}
			}
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x000A5E5C File Offset: 0x000A405C
		public void addBirdies(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.timeOfDay >= 1500 || this is Desert || this is Railroad || this is Farm)
			{
				return;
			}
			Season season = this.GetSeason();
			if (season != Season.Summer)
			{
				while (Game1.random.NextDouble() < chance)
				{
					int birdiesToAdd = Game1.random.Next(1, 4);
					bool success = false;
					int tries = 0;
					while (!success && tries < 5)
					{
						Vector2 randomTile = this.getRandomTile(null);
						if (!onlyIfOnScreen || !Utility.isOnScreen(randomTile * 64f, 64))
						{
							Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 2, (int)randomTile.Y - 2, 5, 5);
							if (this.isAreaClear(area))
							{
								List<Critter> crittersToAdd = new List<Critter>();
								int whichBird = (season == Season.Fall) ? 45 : 25;
								if (Game1.random.NextBool() && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
								{
									whichBird = ((season == Season.Fall) ? 135 : 125);
								}
								if (whichBird == 25 && Game1.random.NextDouble() < 0.05)
								{
									whichBird = 165;
								}
								for (int i = 0; i < birdiesToAdd; i++)
								{
									crittersToAdd.Add(new Birdie(-100, -100, whichBird));
								}
								this.addCrittersStartingAtTile(randomTile, crittersToAdd);
								success = true;
							}
						}
						tries++;
					}
				}
			}
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x000A5FB7 File Offset: 0x000A41B7
		public void addJumperFrog(Vector2 tileLocation)
		{
			List<Critter> list = this.critters;
			if (list == null)
			{
				return;
			}
			list.Add(new Frog(tileLocation, false, false));
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x000A5FD4 File Offset: 0x000A41D4
		public void addFrog()
		{
			if (this.IsRainingHere() && !this.IsWinterHere())
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 v = this.getRandomTile(null);
					if (this.isWaterTile((int)v.X, (int)v.Y) && this.isWaterTile((int)v.X, (int)v.Y - 1) && this.doesTileHaveProperty((int)v.X, (int)v.Y, "Passable", "Buildings", false) == null)
					{
						int distanceToCheck = 10;
						bool flip = Game1.random.NextBool();
						for (int j = 0; j < distanceToCheck; j++)
						{
							v.X += (float)(flip ? 1 : -1);
							if (this.isTileOnMap((int)v.X, (int)v.Y) && !this.isWaterTile((int)v.X, (int)v.Y))
							{
								this.critters.Add(new Frog(v, true, flip));
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x000A60D7 File Offset: 0x000A42D7
		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			Event @event = this.currentEvent;
			if (@event == null)
			{
				return;
			}
			@event.checkForSpecialCharacterIconAtThisTile(tileLocation);
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x000A60EC File Offset: 0x000A42EC
		private void addCrittersStartingAtTile(Vector2 tile, List<Critter> crittersToAdd)
		{
			if (crittersToAdd == null)
			{
				return;
			}
			int tries = 0;
			HashSet<Vector2> tried_tiles = new HashSet<Vector2>();
			while (crittersToAdd.Count > 0 && tries < 20)
			{
				if (tried_tiles.Contains(tile))
				{
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
				}
				else
				{
					if (this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						Critter critter = crittersToAdd.Last<Critter>();
						critter.position = tile * 64f;
						critter.startingPosition = tile * 64f;
						this.critters.Add(critter);
						crittersToAdd.RemoveAt(crittersToAdd.Count - 1);
					}
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
					tried_tiles.Add(tile);
				}
				tries++;
			}
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x000A61BC File Offset: 0x000A43BC
		public bool isAreaClear(Microsoft.Xna.Framework.Rectangle area)
		{
			foreach (Vector2 tile in area.GetVectors())
			{
				if (!this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x000A6220 File Offset: 0x000A4420
		public void performGreenRainUpdate()
		{
			if (this.IsGreenRainingHere() && this.IsOutdoors)
			{
				LocationData data = this.GetData();
				if (((data != null) ? new bool?(data.CanHaveGreenRainSpawns) : null) ?? true)
				{
					Layer pathsLayer = this.map.GetLayer("Paths");
					if (pathsLayer != null)
					{
						for (int x = 0; x < pathsLayer.LayerWidth; x++)
						{
							for (int y = 0; y < pathsLayer.LayerHeight; y++)
							{
								Tile tile = pathsLayer.Tiles[x, y];
								if (tile != null && tile.TileIndexProperties.ContainsKey("GreenRain"))
								{
									Vector2 tilePos = new Vector2((float)x, (float)y);
									if (!this.IsTileOccupiedBy(tilePos, CollisionMask.All, CollisionMask.None, false))
									{
										this.terrainFeatures.Add(tilePos, (this is Forest) ? new Tree("12", 5, true) : new Tree((10 + (Game1.random.NextBool(0.1) ? 2 : Game1.random.Choose(1, 0))).ToString(), 5, true));
									}
								}
							}
						}
					}
					if (!(this is Town))
					{
						string[] trees = this.GetMapPropertySplitBySpaces("Trees");
						for (int i = 0; i < trees.Length; i += 3)
						{
							Vector2 position;
							string error;
							int treeType;
							if (!ArgUtility.TryGetVector2(trees, i, out position, out error, false, "Vector2 position") || !ArgUtility.TryGetInt(trees, i + 2, out treeType, out error, "int treeType"))
							{
								this.LogMapPropertyError("Trees", trees, error, ' ');
							}
							else
							{
								float chance = this.IsFarm ? 0.5f : 1f;
								if (Game1.random.NextBool(chance) && !this.IsTileOccupiedBy(position, CollisionMask.All, CollisionMask.None, false))
								{
									this.terrainFeatures.Add(position, new Tree((treeType + 1).ToString(), 5, false));
								}
							}
						}
						TerrainFeature[] array = this.terrainFeatures.Values.ToArray<TerrainFeature>();
						for (int k = 0; k < array.Length; k++)
						{
							Tree tree = array[k] as Tree;
							if (tree != null)
							{
								tree.onGreenRainDay(false);
							}
						}
						int mapArea = this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight;
						this.spawnWeedsAndStones(mapArea / 16, true, false);
						this.spawnWeedsAndStones(mapArea / 8, true, true);
						for (int j = 0; j < mapArea / 4; j++)
						{
							Vector2 v = this.getRandomTile(null);
							Object topLeft;
							Object topRight;
							Object bottomRight;
							Object bottomLeft;
							if (this.objects.TryGetValue(v, out topLeft) && topLeft.IsWeeds() && this.objects.TryGetValue(v + new Vector2(1f, 0f), out topRight) && topRight.IsWeeds() && this.objects.TryGetValue(v + new Vector2(1f, 1f), out bottomRight) && bottomRight.IsWeeds() && this.objects.TryGetValue(v + new Vector2(0f, 1f), out bottomLeft) && bottomLeft.IsWeeds())
							{
								this.objects.Remove(v);
								this.objects.Remove(v + new Vector2(1f, 0f));
								this.objects.Remove(v + new Vector2(1f, 1f));
								this.objects.Remove(v + new Vector2(0f, 1f));
								this.resourceClumps.Add(new ResourceClump(44 + Game1.random.Choose(2, 0), 2, 2, v, new int?(4), "TileSheets\\Objects_2"));
							}
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x000A662C File Offset: 0x000A482C
		public void performDayAfterGreenRainUpdate()
		{
			foreach (KeyValuePair<Vector2, Object> pair in this.objects.Pairs.ToArray<KeyValuePair<Vector2, Object>>())
			{
				if (pair.Value.Name.Contains("GreenRainWeeds"))
				{
					this.objects.Remove(pair.Key);
				}
			}
			this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.IsGreenRainBush());
			foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in this.terrainFeatures.Pairs.ToArray<KeyValuePair<Vector2, TerrainFeature>>())
			{
				Tree tree = pair2.Value as Tree;
				if (tree != null)
				{
					if (this is Town)
					{
						if (tree.isTemporaryGreenRainTree.Value)
						{
							this.terrainFeatures.Remove(pair2.Key);
						}
					}
					else
					{
						tree.onGreenRainDay(true);
					}
				}
			}
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x000A6728 File Offset: 0x000A4928
		public Vector2 getRandomTile(Random r = null)
		{
			if (r == null)
			{
				r = Game1.random;
			}
			return new Vector2((float)r.Next(this.Map.Layers[0].LayerWidth), (float)r.Next(this.Map.Layers[0].LayerHeight));
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x000A6780 File Offset: 0x000A4980
		public void setUpLocationSpecificFlair()
		{
			this.indoorLightingColor = new Color(100, 120, 30);
			this.indoorLightingNightColor = new Color(150, 150, 30);
			Color c;
			if (this.TryGetAmbientLightFromMap(out c, "AmbientLight"))
			{
				if (c == Color.White)
				{
					c = Color.Black;
				}
				this.indoorLightingColor = c;
				Color night;
				if (this.TryGetAmbientLightFromMap(out night, "AmbientNightLight"))
				{
					this.indoorLightingNightColor = night;
				}
				else
				{
					this.indoorLightingNightColor = this.indoorLightingColor;
				}
			}
			if (!this.isOutdoors.Value && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				Game1.ambientLight = this.indoorLightingColor;
			}
			Game1.screenGlow = false;
			if (!this.IsOutdoors && this.IsGreenRainingHere() && !this.InIslandContext() && this.IsRainingHere())
			{
				this.indoorLightingColor = new Color(123, 0, 96);
				this.indoorLightingNightColor = new Color(185, 40, 119);
				Game1.screenGlowOnce(new Color(0, 255, 50) * 0.5f, true, 1f, 0.3f);
			}
			string value = this.name.Value;
			if (value != null)
			{
				switch (value.Length)
				{
				case 6:
				{
					char c2 = value[1];
					if (c2 != 'a')
					{
						if (c2 != 'u')
						{
							return;
						}
						if (!(value == "Summit"))
						{
							return;
						}
						Game1.ambientLight = Color.Black;
						return;
					}
					else
					{
						if (!(value == "Saloon"))
						{
							return;
						}
						if (Game1.timeOfDay >= 1700 || this.IsGreenRainingHere())
						{
							this.setFireplace(true, 22, 17, false, 0, 0);
						}
						if (Game1.random.NextDouble() < 0.25)
						{
							NPC p = Game1.getCharacterFromName("Gus", true, false);
							if (p != null && p.TilePoint.Y == 18 && p.currentLocation == this)
							{
								string toSay;
								switch (Game1.random.Next(5))
								{
								case 0:
									toSay = "Greeting";
									break;
								case 1:
									toSay = (this.IsSummerHere() ? "Summer" : "NotSummer");
									break;
								case 2:
									toSay = (this.IsSnowingHere() ? "Snowing1" : "NotSnowing1");
									break;
								case 3:
									toSay = (this.IsRainingHere() ? "Raining" : "NotRaining");
									break;
								default:
									toSay = (this.IsSnowingHere() ? "Snowing2" : "NotSnowing2");
									break;
								}
								if (Game1.random.NextDouble() < 0.001)
								{
									toSay = "RareGreeting";
								}
								p.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:Saloon_Gus_" + toSay), null, 2, 3000, 0);
							}
						}
						if (this.getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.mouseCursors2,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(129f, 210f),
								interval = 50000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(11f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
								scale = 4f,
								layerDepth = 0.1281f,
								id = 777
							});
						}
						if (Game1.dayOfMonth % 7 == 0 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom") && Game1.timeOfDay < 1500)
						{
							Texture2D tempTxture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							this.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = tempTxture,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
								animationLength = 7,
								sourceRectStartingPos = new Vector2(368f, 336f),
								interval = 5000f,
								totalNumberOfLoops = 99999,
								position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
								scale = 4f,
								layerDepth = 0.0401f,
								id = 2400
							});
							return;
						}
						return;
					}
					break;
				}
				case 7:
				{
					char c2 = value[0];
					if (c2 != 'B')
					{
						if (c2 != 'S')
						{
							return;
						}
						if (!(value == "Sunroom"))
						{
							return;
						}
						this.indoorLightingColor = new Color(0, 0, 0);
						AmbientLocationSounds.addSound(new Vector2(3f, 4f), 0);
						if (this.largeTerrainFeatures.Count == 0)
						{
							Bush b = new Bush(new Vector2(6f, 7f), 3, this, -999);
							b.loadSprite();
							b.health = 99f;
							this.largeTerrainFeatures.Add(b);
						}
						if (!this.IsRainingHere())
						{
							this.critters = new List<Critter>();
							this.critters.Add(new Butterfly(this, this.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
							while (Game1.random.NextBool())
							{
								this.critters.Add(new Butterfly(this, this.getRandomTile(null), false, false, -1, false).setStayInbounds(true));
							}
							return;
						}
						return;
					}
					else
					{
						if (!(value == "BugLand"))
						{
							return;
						}
						if (!Game1.player.hasDarkTalisman && this.CanItemBePlacedHere(new Vector2(31f, 5f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							this.overlayObjects.Add(new Vector2(31f, 5f), new Chest(new List<Item>
							{
								new SpecialItem(6, "")
							}, new Vector2(31f, 5f), false, 0, false)
							{
								Tint = Color.Gray
							});
						}
						using (List<NPC>.Enumerator enumerator = this.characters.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NPC i = enumerator.Current;
								Grub grub = i as Grub;
								if (grub == null)
								{
									Fly fly = i as Fly;
									if (fly != null)
									{
										fly.setHard();
									}
								}
								else
								{
									grub.setHard();
								}
							}
							return;
						}
					}
					break;
				}
				case 8:
				{
					char c2 = value[0];
					if (c2 <= 'J')
					{
						if (c2 != 'H')
						{
							if (c2 != 'J')
							{
								return;
							}
							if (!(value == "JojaMart"))
							{
								return;
							}
							this.indoorLightingColor = new Color(0, 0, 0);
							if (!Game1.random.NextBool())
							{
								return;
							}
							NPC p2 = Game1.getCharacterFromName("Morris", true, false);
							if (p2 != null && p2.currentLocation == this)
							{
								string toSay2 = "Strings\\SpeechBubbles:JojaMart_Morris_Greeting";
								p2.showTextAboveHead(Game1.content.LoadString(toSay2), null, 2, 3000, 0);
								return;
							}
							return;
						}
						else
						{
							if (!(value == "Hospital"))
							{
								return;
							}
							this.indoorLightingColor = new Color(100, 100, 60);
							if (!Game1.random.NextBool())
							{
								return;
							}
							NPC p3 = Game1.getCharacterFromName("Maru", true, false);
							if (p3 == null || p3.currentLocation != this || p3.isDivorcedFrom(Game1.player))
							{
								return;
							}
							string toSay3;
							switch (Game1.random.Next(5))
							{
							case 0:
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting1";
								break;
							case 1:
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting2";
								break;
							case 2:
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting3";
								break;
							case 3:
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting4";
								break;
							default:
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting5";
								break;
							}
							if (Game1.player.spouse == "Maru")
							{
								toSay3 = "Strings\\SpeechBubbles:Hospital_Maru_Spouse";
								p3.showTextAboveHead(Game1.content.LoadString(toSay3), new Color?(SpriteText.color_Red), 2, 3000, 0);
								return;
							}
							p3.showTextAboveHead(Game1.content.LoadString(toSay3), null, 2, 3000, 0);
							return;
						}
					}
					else if (c2 != 'S')
					{
						if (c2 != 'W')
						{
							return;
						}
						if (!(value == "WitchHut"))
						{
							return;
						}
						if (Game1.player.mailReceived.Contains("cursed_doll") && !this.farmers.Any())
						{
							this.characters.Clear();
							uint childrenTurnedToDoves = Game1.stats.Get("childrenTurnedToDoves");
							this.addCharacter(new Bat(new Vector2(7f, 6f) * 64f, -666));
							if (childrenTurnedToDoves > 1U)
							{
								this.addCharacter(new Bat(new Vector2(4f, 7f) * 64f, -666));
							}
							if (childrenTurnedToDoves > 2U)
							{
								this.addCharacter(new Bat(new Vector2(10f, 7f) * 64f, -666));
							}
							int j = 4;
							while ((long)j <= (long)((ulong)childrenTurnedToDoves))
							{
								this.addCharacter(new Bat(Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(1, 4, 13, 4), Game1.random) * 64f + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), -666));
								j++;
							}
							return;
						}
						return;
					}
					else
					{
						if (!(value == "SeedShop"))
						{
							return;
						}
						this.setFireplace(true, 25, 13, false, 0, 0);
						if (Game1.random.NextBool() && Game1.player.TilePoint.Y > 10)
						{
							NPC p4 = Game1.getCharacterFromName("Pierre", true, false);
							if (p4 != null && p4.TilePoint.Y == 17 && p4.currentLocation == this)
							{
								string toSay4;
								switch (Game1.random.Next(5))
								{
								case 0:
									toSay4 = (this.IsWinterHere() ? "Winter" : "NotWinter");
									break;
								case 1:
									toSay4 = (this.IsSummerHere() ? "Summer" : "NotSummer");
									break;
								case 2:
									toSay4 = "Greeting1";
									break;
								case 3:
									toSay4 = "Greeting2";
									break;
								default:
									toSay4 = (this.IsRainingHere() ? "Raining" : "NotRaining");
									break;
								}
								if (Game1.random.NextDouble() < 0.001)
								{
									toSay4 = "RareGreeting";
								}
								string dialogue = Game1.content.LoadString("Strings\\SpeechBubbles:SeedShop_Pierre_" + toSay4);
								p4.showTextAboveHead(string.Format(dialogue, Game1.player.Name), null, 2, 3000, 0);
							}
						}
						if (this.getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.mouseCursors2,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(129f, 210f),
								interval = 50000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(5f, 17f) * 64f + new Vector2(3f, 0f) * 4f,
								scale = 4f,
								layerDepth = 0.1217f,
								id = 777
							});
						}
						if (this.getCharacterFromName("Abigail") != null && this.getCharacterFromName("Abigail").TilePoint.Equals(new Point(3, 6)))
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, false, false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
							{
								id = 688
							});
							return;
						}
						return;
					}
					break;
				}
				case 9:
				{
					char c2 = value[0];
					if (c2 != 'J')
					{
						if (c2 != 'L')
						{
							if (c2 != 'Q')
							{
								return;
							}
							if (!(value == "QiNutRoom"))
							{
								return;
							}
							Game1.ambientLight = this.indoorLightingColor;
							return;
						}
						else
						{
							if (!(value == "LeahHouse"))
							{
								return;
							}
							NPC k = Game1.getCharacterFromName("Leah", true, false);
							if (this.IsFallHere() || this.IsWinterHere() || this.IsRainingHere())
							{
								this.setFireplace(true, 11, 4, false, 0, 0);
							}
							if (k != null && k.currentLocation == this && !k.isDivorcedFrom(Game1.player))
							{
								int num = Game1.random.Next(3);
								string toSay5;
								if (num != 0)
								{
									if (num != 1)
									{
										toSay5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting3";
									}
									else
									{
										toSay5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting2";
									}
								}
								else
								{
									toSay5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting1";
								}
								k.faceTowardFarmerForPeriod(3000, 15, false, Game1.player);
								k.showTextAboveHead(Game1.content.LoadString(toSay5, Game1.player.Name), null, 2, 3000, 0);
								return;
							}
							return;
						}
					}
					else
					{
						if (!(value == "JoshHouse"))
						{
							return;
						}
						if (Game1.isGreenRain)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(386, 334, 36, 28), 40f, 3, 999999, new Vector2(246.5f, 317f) * 4f, false, false, 0.136001f, 0f, Color.White, 2f, 0f, 0f, 0f, false));
							return;
						}
						return;
					}
					break;
				}
				case 10:
				{
					char c2 = value[0];
					if (c2 <= 'G')
					{
						if (c2 != 'A')
						{
							if (c2 != 'B')
							{
								if (c2 != 'G')
								{
									return;
								}
								if (!(value == "Greenhouse"))
								{
									return;
								}
								if (Game1.isDarkOut(this))
								{
									Game1.ambientLight = Game1.outdoorLight;
									return;
								}
								return;
							}
							else
							{
								if (!(value == "Blacksmith"))
								{
									return;
								}
								AmbientLocationSounds.addSound(new Vector2(9f, 10f), 2);
								AmbientLocationSounds.changeSpecificVariable("Frequency", 2f, 2);
								return;
							}
						}
						else
						{
							if (!(value == "AnimalShop"))
							{
								return;
							}
							this.setFireplace(true, 3, 14, false, 0, 0);
							if (Game1.random.NextBool())
							{
								NPC p5 = Game1.getCharacterFromName("Marnie", true, false);
								if (p5 != null && p5.TilePoint.Y == 14)
								{
									string toSay6;
									switch (Game1.random.Next(5))
									{
									case 0:
										toSay6 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting1";
										break;
									case 1:
										toSay6 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting2";
										break;
									case 2:
										toSay6 = ((Game1.player.getFriendshipHeartLevelForNPC("Marnie") > 4) ? "Strings\\SpeechBubbles:AnimalShop_Marnie_CloseFriends" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotCloseFriends");
										break;
									case 3:
										toSay6 = (this.IsRainingHere() ? "Strings\\SpeechBubbles:AnimalShop_Marnie_Raining" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotRaining");
										break;
									default:
										toSay6 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting3";
										break;
									}
									if (Game1.random.NextDouble() < 0.001)
									{
										toSay6 = "Strings\\SpeechBubbles:AnimalShop_Marnie_RareGreeting";
									}
									p5.showTextAboveHead(Game1.content.LoadString(toSay6, Game1.player.Name, Game1.player.farmName), null, 2, 3000, 0);
								}
							}
							if (this.getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors2,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
									animationLength = 1,
									sourceRectStartingPos = new Vector2(129f, 210f),
									interval = 50000f,
									totalNumberOfLoops = 9999,
									position = new Vector2(13f, 14f) * 64f + new Vector2(3f, 0f) * 4f,
									scale = 4f,
									layerDepth = 0.1025f,
									id = 777
								});
							}
							if (Game1.netWorldState.Value.hasWorldStateID("m_painting0"))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(25, 1925, 25, 23),
									animationLength = 1,
									sourceRectStartingPos = new Vector2(25f, 1925f),
									interval = 5000f,
									totalNumberOfLoops = 9999,
									position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
									scale = 4f,
									layerDepth = 0.1f,
									id = 777
								});
								return;
							}
							if (Game1.netWorldState.Value.hasWorldStateID("m_painting1"))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1925, 25, 23),
									animationLength = 1,
									sourceRectStartingPos = new Vector2(0f, 1925f),
									interval = 5000f,
									totalNumberOfLoops = 9999,
									position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
									scale = 4f,
									layerDepth = 0.1f,
									id = 777
								});
								return;
							}
							if (Game1.netWorldState.Value.hasWorldStateID("m_painting2"))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1948, 25, 24),
									animationLength = 1,
									sourceRectStartingPos = new Vector2(0f, 1948f),
									interval = 5000f,
									totalNumberOfLoops = 9999,
									position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
									scale = 4f,
									layerDepth = 0.1f,
									id = 777
								});
								return;
							}
							return;
						}
					}
					else if (c2 != 'H')
					{
						if (c2 != 'M')
						{
							if (c2 != 'S')
							{
								return;
							}
							if (!(value == "SandyHouse"))
							{
								return;
							}
							this.indoorLightingColor = new Color(0, 0, 0);
							if (!Game1.random.NextBool())
							{
								return;
							}
							NPC p6 = Game1.getCharacterFromName("Sandy", true, false);
							if (p6 != null && p6.currentLocation == this)
							{
								string toSay7;
								switch (Game1.random.Next(5))
								{
								case 0:
									toSay7 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting1";
									break;
								case 1:
									toSay7 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting2";
									break;
								case 2:
									toSay7 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting3";
									break;
								case 3:
									toSay7 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting4";
									break;
								default:
									toSay7 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting5";
									break;
								}
								p6.showTextAboveHead(Game1.content.LoadString(toSay7), null, 2, 3000, 0);
								return;
							}
							return;
						}
						else
						{
							if (!(value == "ManorHouse"))
							{
								return;
							}
							this.indoorLightingColor = new Color(150, 120, 50);
							NPC le = Game1.getCharacterFromName("Lewis", true, false);
							if (le != null && le.currentLocation == this)
							{
								string toSay8 = (Game1.timeOfDay < 1200) ? "Morning" : ((Game1.timeOfDay < 1700) ? "Afternoon" : "Evening");
								le.faceTowardFarmerForPeriod(3000, 15, false, Game1.player);
								le.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ManorHouse_Lewis_" + toSay8), null, 2, 3000, 0);
								return;
							}
							return;
						}
					}
					else if (!(value == "HaleyHouse"))
					{
						return;
					}
					break;
				}
				case 11:
					return;
				case 12:
				{
					char c2 = value[0];
					if (c2 != 'E')
					{
						if (c2 != 'L')
						{
							if (c2 != 'S')
							{
								return;
							}
							if (!(value == "ScienceHouse"))
							{
								return;
							}
							if (Game1.random.NextBool() && Game1.player.currentLocation != null && Game1.player.currentLocation.isOutdoors.Value)
							{
								NPC p7 = Game1.getCharacterFromName("Robin", true, false);
								if (p7 != null && p7.TilePoint.Y == 18)
								{
									string toSay9;
									switch (Game1.random.Next(5))
									{
									case 0:
										toSay9 = (this.IsRainingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining1" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining1");
										break;
									case 1:
										toSay9 = (this.IsSnowingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Snowing" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotSnowing");
										break;
									case 2:
										toSay9 = ((Game1.player.getFriendshipHeartLevelForNPC("Robin") > 4) ? "Strings\\SpeechBubbles:ScienceHouse_Robin_CloseFriends" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotCloseFriends");
										break;
									case 3:
										toSay9 = (this.IsRainingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining2" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining2");
										break;
									default:
										toSay9 = "Strings\\SpeechBubbles:ScienceHouse_Robin_Greeting";
										break;
									}
									if (Game1.random.NextDouble() < 0.001)
									{
										toSay9 = "Strings\\SpeechBubbles:ScienceHouse_Robin_RareGreeting";
									}
									p7.showTextAboveHead(Game1.content.LoadString(toSay9, Game1.player.Name), null, 2, 3000, 0);
								}
							}
							if (this.getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.mouseCursors2,
									sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
									animationLength = 1,
									sourceRectStartingPos = new Vector2(129f, 210f),
									interval = 50000f,
									totalNumberOfLoops = 9999,
									position = new Vector2(7f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
									scale = 4f,
									layerDepth = 0.1281f,
									id = 777
								});
								return;
							}
							return;
						}
						else
						{
							if (!(value == "LeoTreeHouse"))
							{
								return;
							}
							this.temporarySprites.Add(new EmilysParrot(new Vector2(88f, 224f))
							{
								layerDepth = 1f,
								id = 5858585
							});
							this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(304f, 32f), false, 0f, Color.White)
							{
								layerDepth = 0.001f,
								interval = 700f,
								animationLength = 3,
								totalNumberOfLoops = 999999,
								scale = 4f
							});
							this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(47, 334, 12, 11), new Vector2(112f, -25.6f), true, 0f, Color.White)
							{
								layerDepth = 0.001f,
								interval = 300f,
								animationLength = 3,
								totalNumberOfLoops = 999999,
								scale = 4f
							});
							this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(224f, -25.6f), false, 0f, Color.White)
							{
								layerDepth = 0.001f,
								interval = 800f,
								animationLength = 3,
								totalNumberOfLoops = 999999,
								scale = 4f
							});
							return;
						}
					}
					else
					{
						if (!(value == "ElliottHouse"))
						{
							return;
						}
						NPC e = Game1.getCharacterFromName("Elliott", true, false);
						if (e != null && e.currentLocation == this && !e.isDivorcedFrom(Game1.player))
						{
							int num = Game1.random.Next(3);
							string toSay10;
							if (num != 0)
							{
								if (num != 1)
								{
									toSay10 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting3";
								}
								else
								{
									toSay10 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting2";
								}
							}
							else
							{
								toSay10 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting1";
							}
							e.faceTowardFarmerForPeriod(3000, 15, false, Game1.player);
							e.showTextAboveHead(Game1.content.LoadString(toSay10, Game1.player.Name), null, 2, 3000, 0);
							return;
						}
						return;
					}
					break;
				}
				case 13:
				{
					if (!(value == "LewisBasement"))
					{
						return;
					}
					if (this.farmers.Count == 0)
					{
						this.characters.Clear();
					}
					Vector2 shortsTile = new Vector2(17f, 15f);
					this.overlayObjects.Remove(shortsTile);
					Object o = ItemRegistry.Create<Object>("(O)789", 1, 0, false);
					o.questItem.Value = true;
					o.TileLocation = shortsTile;
					o.IsSpawnedObject = true;
					this.overlayObjects.Add(shortsTile, o);
					return;
				}
				case 14:
				{
					if (!(value == "AdventureGuild"))
					{
						return;
					}
					this.setFireplace(true, 9, 11, false, 0, 0);
					if (!Game1.random.NextBool())
					{
						return;
					}
					NPC p8 = Game1.getCharacterFromName("Marlon", true, false);
					if (p8 != null)
					{
						string toSay11;
						switch (Game1.random.Next(5))
						{
						case 0:
							toSay11 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting_" + (Game1.player.IsMale ? "Male" : "Female");
							break;
						case 1:
							toSay11 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting1";
							break;
						case 2:
							toSay11 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting2";
							break;
						case 3:
							toSay11 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting3";
							break;
						default:
							toSay11 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting4";
							break;
						}
						p8.showTextAboveHead(Game1.content.LoadString(toSay11), null, 2, 3000, 0);
						return;
					}
					return;
				}
				case 15:
					if (!(value == "CommunityCenter"))
					{
						return;
					}
					if (this is CommunityCenter)
					{
						if (!Game1.isLocationAccessible("CommunityCenter"))
						{
							Event @event = this.currentEvent;
							if (!(((@event != null) ? @event.id : null) == "191393"))
							{
								return;
							}
						}
						this.setFireplace(true, 31, 8, false, 0, 0);
						this.setFireplace(true, 32, 8, false, 0, 0);
						this.setFireplace(true, 33, 8, false, 0, 0);
						return;
					}
					return;
				case 16:
				{
					if (!(value == "ArchaeologyHouse"))
					{
						return;
					}
					this.setFireplace(true, 43, 4, false, 0, 0);
					if (!Game1.random.NextBool() || !Game1.player.hasOrWillReceiveMail("artifactFound"))
					{
						return;
					}
					NPC g = Game1.getCharacterFromName("Gunther", true, false);
					if (g != null && g.currentLocation == this)
					{
						string toSay12;
						switch (Game1.random.Next(5))
						{
						case 0:
							toSay12 = "Greeting1";
							break;
						case 1:
							toSay12 = "Greeting2";
							break;
						case 2:
							toSay12 = "Greeting3";
							break;
						case 3:
							toSay12 = "Greeting4";
							break;
						default:
							toSay12 = "Greeting5";
							break;
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay12 = "RareGreeting";
						}
						g.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ArchaeologyHouse_Gunther_" + toSay12), null, 2, 3000, 0);
						return;
					}
					return;
				}
				case 17:
				{
					if (!(value == "AbandonedJojaMart"))
					{
						return;
					}
					if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						return;
					}
					Point position = new Point(8, 8);
					Game1.currentLightSources.Add(new LightSource("AbandonedJojaMart", 4, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), 1f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
					this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f)
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64 - 12), (float)(position.Y * 64 - 12)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						scale = 0.75f,
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 50
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64 - 12), (float)(position.Y * 64 + 12)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 100
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)(position.X * 64), (float)(position.Y * 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						scale = 0.75f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 150
					});
					if (this.characters.Count == 0)
					{
						this.characters.Add(new Junimo(new Vector2(8f, 7f) * 64f, 6, false));
						return;
					}
					return;
				}
				default:
					return;
				}
				if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse != "Emily")
				{
					this.temporarySprites.Add(new EmilysParrot(new Vector2(912f, 160f)));
					return;
				}
			}
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x000A87C0 File Offset: 0x000A69C0
		public virtual void hostSetup()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (!this.farmers.Any() && !this.HasFarmerWatchingBroadcastEventReturningHere())
			{
				this.interiorDoors.ResetSharedState();
			}
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x000A87EA File Offset: 0x000A69EA
		public virtual void ResetForEvent(Event ev)
		{
			ev.eventPositionTileOffset = Vector2.Zero;
			if (this.IsOutdoors)
			{
				Game1.ambientLight = (this.IsRainingHere() ? new Color(255, 200, 80) : Color.White);
			}
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x000A8824 File Offset: 0x000A6A24
		public virtual bool HasFarmerWatchingBroadcastEventReturningHere()
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.locationBeforeForcedEvent.Value != null && farmer.locationBeforeForcedEvent.Value == this.NameOrUniqueName)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x000A8898 File Offset: 0x000A6A98
		public void resetForPlayerEntry()
		{
			Game1.updateWeatherIcon();
			Game1.hooks.OnGameLocation_ResetForPlayerEntry(this, delegate
			{
				this._madeMapModifications = false;
				if ((!this.farmers.Any() && !this.HasFarmerWatchingBroadcastEventReturningHere()) || Game1.player.sleptInTemporaryBed.Value)
				{
					this.resetSharedState();
				}
				this.resetLocalState();
				if (!this._madeMapModifications)
				{
					this._madeMapModifications = true;
					this.MakeMapModifications(false);
				}
			});
			Microsoft.Xna.Framework.Rectangle player_bounds = Game1.player.GetBoundingBox();
			foreach (Furniture f in this.furniture)
			{
				Microsoft.Xna.Framework.Rectangle furnitureBounds = f.GetBoundingBox();
				if (furnitureBounds.Intersects(player_bounds) && f.IntersectsForCollision(player_bounds) && !f.isPassable())
				{
					Game1.player.TemporaryPassableTiles.Add(furnitureBounds);
				}
			}
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x000A8940 File Offset: 0x000A6B40
		protected virtual void resetLocalState()
		{
			bool isUpdatingForNewDay = Game1.newDaySync.hasInstance();
			string clamp;
			if (this.TryGetMapProperty("ViewportClamp", out clamp))
			{
				try
				{
					int[] bounds = Utility.parseStringToIntArray(clamp, ' ');
					Game1.viewportClampArea = new Microsoft.Xna.Framework.Rectangle(bounds[0] * 64, bounds[1] * 64, bounds[2] * 64, bounds[3] * 64);
					goto IL_5E;
				}
				catch (Exception)
				{
					Game1.viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
					goto IL_5E;
				}
			}
			Game1.viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
			IL_5E:
			Game1.elliottPiano = 0;
			Game1.crabPotOverlayTiles.Clear();
			Utility.killAllStaticLoopingSoundCues();
			Game1.player.bridge = null;
			Game1.player.SetOnBridge(false);
			if (Game1.CurrentEvent == null && !this.Name.ContainsIgnoreCase("bath"))
			{
				Game1.player.canOnlyWalk = false;
			}
			if (!(this is Farm))
			{
				this.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.clearOnAreaEntry());
			}
			if (Game1.options != null)
			{
				if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton))
				{
					Game1.player.setRunning(!Game1.options.autoRun, true);
				}
				else
				{
					Game1.player.setRunning(Game1.options.autoRun, true);
				}
			}
			Horse mount = Game1.player.mount;
			if (mount != null)
			{
				mount.SyncPositionToRider();
			}
			Game1.UpdateViewPort(false, Game1.player.StandingPixel);
			Game1.previousViewportPosition = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
			Game1.PushUIMode();
			foreach (IClickableMenu clickableMenu in Game1.onScreenMenus)
			{
				clickableMenu.gameWindowSizeChanged(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
			}
			Game1.PopUIMode();
			this.ignoreWarps = false;
			if (!isUpdatingForNewDay || Game1.newDaySync.hasFinished())
			{
				if (Game1.player.rightRing.Value != null)
				{
					Game1.player.rightRing.Value.onNewLocation(Game1.player, this);
				}
				if (Game1.player.leftRing.Value != null)
				{
					Game1.player.leftRing.Value.onNewLocation(Game1.player, this);
				}
			}
			this.forceViewportPlayerFollow = this.Map.Properties.ContainsKey("ViewportFollowPlayer");
			this.lastTouchActionLocation = Game1.player.Tile;
			Game1.player.NotifyQuests((Quest quest) => quest.OnWarped(this, false), false);
			if (!this.isOutdoors.Value)
			{
				Game1.player.FarmerSprite.currentStep = "thudStep";
			}
			this.setUpLocationSpecificFlair();
			this._updateAmbientLighting();
			if (!this.ignoreLights.Value)
			{
				string lightIdPrefix = this.NameOrUniqueName + "_MapLight_";
				Game1.currentLightSources.RemoveWhere((KeyValuePair<string, LightSource> p) => p.Key.StartsWith(lightIdPrefix));
				string[] lights = this.GetMapPropertySplitBySpaces("Light");
				for (int i = 0; i < lights.Length; i += 3)
				{
					Point tile;
					string error;
					int textureIndex;
					if (!ArgUtility.TryGetPoint(lights, i, out tile, out error, "Point tile") || !ArgUtility.TryGetInt(lights, i + 2, out textureIndex, out error, "int textureIndex"))
					{
						this.LogMapPropertyError("Light", lights, error, ' ');
					}
					else
					{
						IDictionary<string, LightSource> currentLightSources = Game1.currentLightSources;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
						defaultInterpolatedStringHandler.AppendFormatted(lightIdPrefix);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tile.X);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(tile.Y);
						currentLightSources.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), textureIndex, new Vector2((float)(tile.X * 64 + 32), (float)(tile.Y * 64 + 32)), 1f, LightSource.LightContext.MapLight, 0L, this.NameOrUniqueName));
					}
				}
				if (!Game1.isTimeToTurnOffLighting(this) && !Game1.isRaining)
				{
					string[] windowLights = this.GetMapPropertySplitBySpaces("WindowLight");
					for (int j = 0; j < windowLights.Length; j += 3)
					{
						Point tile2;
						string error2;
						int textureIndex2;
						if (!ArgUtility.TryGetPoint(windowLights, j, out tile2, out error2, "Point tile") || !ArgUtility.TryGetInt(windowLights, j + 2, out textureIndex2, out error2, "int textureIndex"))
						{
							this.LogMapPropertyError("WindowLight", windowLights, error2, ' ');
						}
						else
						{
							IDictionary<string, LightSource> currentLightSources2 = Game1.currentLightSources;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 3);
							defaultInterpolatedStringHandler.AppendFormatted(lightIdPrefix);
							defaultInterpolatedStringHandler.AppendLiteral("_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(tile2.X);
							defaultInterpolatedStringHandler.AppendLiteral("_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(tile2.Y);
							defaultInterpolatedStringHandler.AppendLiteral("_Window");
							currentLightSources2.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), textureIndex2, new Vector2((float)(tile2.X * 64 + 32), (float)(tile2.Y * 64 + 32)), 1f, LightSource.LightContext.WindowLight, 0L, this.NameOrUniqueName));
						}
					}
					foreach (Vector2 v in this.lightGlows)
					{
						IDictionary<string, LightSource> currentLightSources3 = Game1.currentLightSources;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 3);
						defaultInterpolatedStringHandler.AppendFormatted(lightIdPrefix);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<float>(v.X);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<float>(v.Y);
						defaultInterpolatedStringHandler.AppendLiteral("_Glow");
						currentLightSources3.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 6, v, 1f, LightSource.LightContext.WindowLight, 0L, this.NameOrUniqueName));
					}
				}
			}
			if (this.isOutdoors.Value || this.treatAsOutdoors.Value)
			{
				string[] sounds = this.GetMapPropertySplitBySpaces("BrookSounds");
				for (int k = 0; k < sounds.Length; k += 3)
				{
					Vector2 tile3;
					string error3;
					int soundId;
					if (!ArgUtility.TryGetVector2(sounds, k, out tile3, out error3, false, "Vector2 tile") || !ArgUtility.TryGetInt(sounds, k + 2, out soundId, out error3, "int soundId"))
					{
						this.LogMapPropertyError("BrookSounds", sounds, error3, ' ');
					}
					else
					{
						AmbientLocationSounds.addSound(tile3, soundId);
					}
				}
				Game1.randomizeRainPositions();
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> kvp in this.terrainFeatures.Pairs)
			{
				kvp.Value.performPlayerEntryAction();
			}
			if (this.largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
				{
					largeTerrainFeature.performPlayerEntryAction();
				}
			}
			foreach (KeyValuePair<Vector2, Object> kvp2 in this.objects.Pairs)
			{
				kvp2.Value.actionOnPlayerEntry();
			}
			if (this.isOutdoors.Value)
			{
				((FarmerSprite)Game1.player.Sprite).currentStep = "sandyStep";
				this.tryToAddCritters(false);
			}
			this.interiorDoors.ResetLocalState();
			int night_tiles_time = Game1.getTrulyDarkTime(this) - 100;
			if (Game1.timeOfDay < night_tiles_time && (!this.IsRainingHere() || this.name.Equals("SandyHouse")))
			{
				string[] dayTiles = this.GetMapPropertySplitBySpaces("DayTiles");
				for (int l = 0; l < dayTiles.Length; l += 4)
				{
					string layerId;
					string error4;
					Point position;
					int tileIndex;
					if (!ArgUtility.TryGet(dayTiles, l, out layerId, out error4, true, "string layerId") || !ArgUtility.TryGetPoint(dayTiles, l + 1, out position, out error4, "Point position") || !ArgUtility.TryGetInt(dayTiles, l + 3, out tileIndex, out error4, "int tileIndex"))
					{
						this.LogMapPropertyError("DayTiles", dayTiles, error4, ' ');
					}
					else if (tileIndex != 720 || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						Tile tile4 = this.map.RequireLayer(layerId).Tiles[position.X, position.Y];
						if (tile4 != null)
						{
							tile4.TileIndex = tileIndex;
						}
					}
				}
			}
			else if (Game1.timeOfDay >= night_tiles_time || (this.IsRainingHere() && !this.name.Equals("SandyHouse")))
			{
				this.switchOutNightTiles();
			}
			if (Game1.killScreen && Game1.activeClickableMenu != null && !Game1.dialogueUp)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			if (Game1.activeClickableMenu == null && !Game1.warpingForForcedRemoteEvent && !isUpdatingForNewDay)
			{
				this.checkForEvents();
			}
			foreach (KeyValuePair<string, LightSource> pair in this.sharedLights.Pairs)
			{
				Game1.currentLightSources[pair.Key] = pair.Value;
			}
			foreach (NPC npc in this.characters)
			{
				npc.behaviorOnLocalFarmerLocationEntry(this);
			}
			foreach (Furniture furniture in this.furniture)
			{
				furniture.actionOnPlayerEntry();
			}
			this.updateFishSplashAnimation();
			this.updateOrePanAnimation();
			this.showDropboxIndicator = false;
			foreach (SpecialOrder s in Game1.player.team.specialOrders)
			{
				if (!s.ShouldDisplayAsComplete())
				{
					foreach (OrderObjective orderObjective in s.objectives)
					{
						DonateObjective donateObjective = orderObjective as DonateObjective;
						if (donateObjective != null && !string.IsNullOrEmpty(donateObjective.dropBoxGameLocation.Value) && donateObjective.GetDropboxLocationName() == this.Name)
						{
							this.showDropboxIndicator = true;
							this.dropBoxIndicatorLocation = donateObjective.dropBoxTileLocation.Value * 64f + new Vector2(7f, 0f) * 4f;
						}
					}
				}
			}
			if (Game1.timeOfDay >= 1830)
			{
				FarmAnimal[] array = this.animals.Values.ToArray<FarmAnimal>();
				for (int m = 0; m < array.Length; m++)
				{
					array[m].warpHome();
				}
			}
			foreach (Building building in this.buildings)
			{
				building.resetLocalState();
			}
			if (this.isThereABuildingUnderConstruction())
			{
				using (NetDictionary<string, BuilderData, NetRef<BuilderData>, SerializableDictionary<string, BuilderData>, NetStringDictionary<BuilderData, NetRef<BuilderData>>>.KeysCollection.Enumerator enumerator12 = Game1.netWorldState.Value.Builders.Keys.GetEnumerator())
				{
					while (enumerator12.MoveNext())
					{
						string builder = enumerator12.Current;
						BuilderData builderData = Game1.netWorldState.Value.Builders[builder];
						if (builderData.buildingLocation.Value == this.NameOrUniqueName && builderData.daysUntilBuilt.Value > 0)
						{
							NPC buildCharacter = Game1.getCharacterFromName(builder, true, false);
							if (buildCharacter != null && buildCharacter.currentLocation.Equals(this))
							{
								Building b = this.getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
								if (b != null)
								{
									this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(399, 262, (b.daysOfConstructionLeft.Value == 1) ? 29 : 9, 43), new Vector2((float)(b.tileX.Value + b.tilesWide.Value / 2), (float)(b.tileY.Value + b.tilesHigh.Value / 2)) * 64f + new Vector2(-16f, -144f), false, 0f, Color.White)
									{
										id = 16846,
										scale = 4f,
										interval = 999999f,
										animationLength = 1,
										totalNumberOfLoops = 99999,
										layerDepth = (float)((b.tileY.Value + b.tilesHigh.Value / 2) * 64 + 32) / 10000f
									});
								}
							}
						}
					}
					return;
				}
			}
			this.removeTemporarySpritesWithIDLocal(16846);
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x000A9784 File Offset: 0x000A7984
		protected virtual void _updateAmbientLighting()
		{
			if (Game1.eventUp || (Game1.player.viewingLocation.Value != null && !Game1.player.viewingLocation.Value.Equals(this.Name)))
			{
				return;
			}
			if (this.isOutdoors.Value && !this.ignoreOutdoorLighting.Value)
			{
				Game1.ambientLight = (this.IsRainingHere() ? new Color(255, 200, 80) : Color.White);
				return;
			}
			if (Game1.isStartingToGetDarkOut(this) || this.lightLevel.Value > 0f)
			{
				int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + this.ExtraMillisecondsPerInGameMinute);
				float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
				Game1.ambientLight = new Color((int)((byte)Utility.Lerp((float)this.indoorLightingColor.R, (float)this.indoorLightingNightColor.R, lerp)), (int)((byte)Utility.Lerp((float)this.indoorLightingColor.G, (float)this.indoorLightingNightColor.G, lerp)), (int)((byte)Utility.Lerp((float)this.indoorLightingColor.B, (float)this.indoorLightingNightColor.B, lerp)));
				return;
			}
			Game1.ambientLight = this.indoorLightingColor;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x000A98DC File Offset: 0x000A7ADC
		private bool TryGetAmbientLightFromMap(out Color color, string propertyName = "AmbientLight")
		{
			string[] fields = this.GetMapPropertySplitBySpaces(propertyName);
			if (fields.Length != 0)
			{
				int r;
				string error;
				int g;
				int b;
				if (ArgUtility.TryGetInt(fields, 0, out r, out error, "int r") && ArgUtility.TryGetInt(fields, 1, out g, out error, "int g") && ArgUtility.TryGetInt(fields, 2, out b, out error, "int b"))
				{
					color = new Color(r, g, b);
					return true;
				}
				this.LogMapPropertyError(propertyName, fields, error, ' ');
			}
			color = Color.White;
			return false;
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x000A9958 File Offset: 0x000A7B58
		public void SelectRandomMiniJukeboxTrack()
		{
			if (this.miniJukeboxTrack.Value != "random")
			{
				return;
			}
			Farmer farmer = Game1.player;
			FarmHouse farmhouse = this as FarmHouse;
			if (farmhouse != null && farmhouse.HasOwner)
			{
				farmer = farmhouse.owner;
			}
			List<string> song_options = Utility.GetJukeboxTracks(farmer, this);
			string song = Game1.random.ChooseFrom(song_options);
			this.randomMiniJukeboxTrack.Value = song;
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x000A99BC File Offset: 0x000A7BBC
		protected virtual void resetSharedState()
		{
			this.SelectRandomMiniJukeboxTrack();
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				this.characters[i].behaviorOnFarmerLocationEntry(this, Game1.player);
			}
			if (!(this is MineShaft))
			{
				switch (this.GetSeason())
				{
				case Season.Spring:
					this.waterColor.Value = new Color(120, 200, 255) * 0.5f;
					return;
				case Season.Summer:
					this.waterColor.Value = new Color(60, 240, 255) * 0.5f;
					return;
				case Season.Fall:
					this.waterColor.Value = new Color(255, 130, 200) * 0.5f;
					return;
				case Season.Winter:
					this.waterColor.Value = new Color(130, 80, 255) * 0.5f;
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x000A9AC8 File Offset: 0x000A7CC8
		public LightSource getLightSource([NotNullWhen(true)] string identifier)
		{
			LightSource light;
			if (identifier == null || !this.sharedLights.TryGetValue(identifier, out light))
			{
				return null;
			}
			return light;
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x000A9AEB File Offset: 0x000A7CEB
		public bool hasLightSource([NotNullWhen(true)] string identifier)
		{
			return identifier != null && this.sharedLights.ContainsKey(identifier);
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x000A9AFE File Offset: 0x000A7CFE
		public void removeLightSource([NotNullWhen(true)] string identifier)
		{
			if (identifier != null)
			{
				this.sharedLights.Remove(identifier);
			}
		}

		// Token: 0x06000F2D RID: 3885 RVA: 0x000A9B10 File Offset: 0x000A7D10
		public void repositionLightSource([NotNullWhen(true)] string identifier, Vector2 position)
		{
			LightSource light;
			if (identifier != null && this.sharedLights.TryGetValue(identifier, out light))
			{
				light.position.Value = position;
			}
		}

		// Token: 0x06000F2E RID: 3886 RVA: 0x000A9B3C File Offset: 0x000A7D3C
		public virtual bool CanSpawnCharacterHere(Vector2 tileLocation)
		{
			return this.isTileOnMap(tileLocation) && this.isTilePlaceable(tileLocation, false) && !this.IsTileBlockedBy(tileLocation, CollisionMask.All, CollisionMask.None, false);
		}

		// Token: 0x06000F2F RID: 3887 RVA: 0x000A9B64 File Offset: 0x000A7D64
		public virtual bool CanItemBePlacedHere(Vector2 tile, bool itemIsPassable = false, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = ~CollisionMask.Objects, bool useFarmerTile = false, bool ignorePassablesExactly = false)
		{
			if (!ignorePassablesExactly)
			{
				ignorePassables &= ~CollisionMask.Objects;
				if (!itemIsPassable)
				{
					ignorePassables &= ~(CollisionMask.Characters | CollisionMask.Farmers);
				}
			}
			if (!this.isTileOnMap(tile))
			{
				return false;
			}
			if (!this.isTilePlaceable(tile, itemIsPassable))
			{
				return false;
			}
			HoeDirt hoeDirtAtTile = this.GetHoeDirtAtTile(tile);
			return ((hoeDirtAtTile != null) ? hoeDirtAtTile.crop : null) == null && !this.IsTileBlockedBy(tile, collisionMask, ignorePassables, useFarmerTile) && (!itemIsPassable || this.getBuildingAt(tile) == null || this.getBuildingAt(tile).GetData() == null || this.getBuildingAt(tile).GetData().AllowsFlooringUnderneath);
		}

		// Token: 0x06000F30 RID: 3888 RVA: 0x000A9BFA File Offset: 0x000A7DFA
		public virtual bool IsTileBlockedBy(Vector2 tile, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = CollisionMask.None, bool useFarmerTile = false)
		{
			return this.IsTileOccupiedBy(tile, collisionMask, ignorePassables, useFarmerTile) || !this.isTilePassable(tile);
		}

		// Token: 0x06000F31 RID: 3889 RVA: 0x000A9C18 File Offset: 0x000A7E18
		public virtual bool IsTileOccupiedBy(Vector2 tile, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = CollisionMask.None, bool useFarmerTile = false)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
			if (collisionMask.HasFlag(CollisionMask.Farmers) && !ignorePassables.HasFlag(CollisionMask.Farmers))
			{
				foreach (Farmer f in this.farmers)
				{
					if (useFarmerTile ? (f.Tile == tile) : f.GetBoundingBox().Intersects(tileRect))
					{
						return true;
					}
				}
			}
			Object o;
			if (collisionMask.HasFlag(CollisionMask.Objects) && this.objects.TryGetValue(tile, out o) && (!ignorePassables.HasFlag(CollisionMask.Objects) || !o.isPassable()))
			{
				return true;
			}
			if (collisionMask.HasFlag(CollisionMask.Furniture))
			{
				Furniture f2 = this.GetFurnitureAt(tile);
				if (f2 != null && (!ignorePassables.HasFlag(CollisionMask.Furniture) || !f2.isPassable()))
				{
					return true;
				}
			}
			if (collisionMask.HasFlag(CollisionMask.Characters))
			{
				foreach (NPC character in this.characters)
				{
					if (character != null && character.GetBoundingBox().Intersects(tileRect) && !character.IsInvisible && (!ignorePassables.HasFlag(CollisionMask.Characters) || !character.farmerPassesThrough))
					{
						return true;
					}
				}
				if (this.animals.Length > 0)
				{
					foreach (FarmAnimal animal in this.animals.Values)
					{
						if (animal.Tile == tile && (!ignorePassables.HasFlag(CollisionMask.Characters) || !animal.farmerPassesThrough))
						{
							return true;
						}
					}
				}
			}
			if (collisionMask.HasFlag(CollisionMask.TerrainFeatures))
			{
				foreach (ResourceClump resourceClump in this.resourceClumps)
				{
					if (resourceClump.occupiesTile((int)tile.X, (int)tile.Y) && (!ignorePassables.HasFlag(CollisionMask.TerrainFeatures) || !resourceClump.isPassable(null)))
					{
						return true;
					}
				}
				if (this.largeTerrainFeatures != null)
				{
					foreach (LargeTerrainFeature t in this.largeTerrainFeatures)
					{
						if (t.getBoundingBox().Intersects(tileRect) && (!ignorePassables.HasFlag(CollisionMask.TerrainFeatures) || !t.isPassable(null)))
						{
							return true;
						}
					}
				}
			}
			TerrainFeature feature;
			if ((collisionMask.HasFlag(CollisionMask.TerrainFeatures) || collisionMask.HasFlag(CollisionMask.Flooring)) && this.terrainFeatures.TryGetValue(tile, out feature) && feature.getBoundingBox().Intersects(tileRect))
			{
				CollisionMask relevantMask = (feature is Flooring) ? CollisionMask.Flooring : CollisionMask.TerrainFeatures;
				if (collisionMask.HasFlag(relevantMask) && (!ignorePassables.HasFlag(relevantMask) || !feature.isPassable(null)))
				{
					return true;
				}
			}
			if (collisionMask.HasFlag(CollisionMask.LocationSpecific) && this.IsLocationSpecificOccupantOnTile(tile))
			{
				return true;
			}
			if (collisionMask.HasFlag(CollisionMask.Buildings))
			{
				foreach (Building b in this.buildings)
				{
					if (!b.isMoving && (ignorePassables.HasFlag(CollisionMask.Buildings) ? (!b.isTilePassable(tile)) : b.occupiesTile(tile, false)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x000AA0C0 File Offset: 0x000A82C0
		public virtual bool IsLocationSpecificOccupantOnTile(Vector2 tileLocation)
		{
			return false;
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x000AA0C3 File Offset: 0x000A82C3
		public virtual bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
		{
			return false;
		}

		// Token: 0x06000F34 RID: 3892 RVA: 0x000AA0C8 File Offset: 0x000A82C8
		public Farmer isTileOccupiedByFarmer(Vector2 tileLocation)
		{
			foreach (Farmer f in this.farmers)
			{
				if (f.Tile == tileLocation)
				{
					return f;
				}
			}
			return null;
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x000AA12C File Offset: 0x000A832C
		public HoeDirt GetHoeDirtAtTile(Vector2 tile)
		{
			Object obj;
			if (this.objects.TryGetValue(tile, out obj))
			{
				IndoorPot pot = obj as IndoorPot;
				if (pot != null)
				{
					return pot.hoeDirt.Value;
				}
			}
			TerrainFeature feature;
			if (this.terrainFeatures.TryGetValue(tile, out feature))
			{
				HoeDirt dirt = feature as HoeDirt;
				if (dirt != null)
				{
					return dirt;
				}
			}
			return null;
		}

		// Token: 0x06000F36 RID: 3894 RVA: 0x000AA17C File Offset: 0x000A837C
		public bool isTileHoeDirt(Vector2 tile)
		{
			return this.GetHoeDirtAtTile(tile) != null;
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x000AA188 File Offset: 0x000A8388
		public bool isTileLocationOpen(Location location)
		{
			return this.isTileLocationOpen(new Vector2((float)location.X, (float)location.Y));
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x000AA1A4 File Offset: 0x000A83A4
		public bool isTileLocationOpen(Vector2 location)
		{
			if (this.map.RequireLayer("Buildings").Tiles[(int)location.X, (int)location.Y] == null && !this.isWaterTile((int)location.X, (int)location.Y) && this.map.RequireLayer("Front").Tiles[(int)location.X, (int)location.Y] == null)
			{
				Layer layer = this.map.GetLayer("AlwaysFront");
				return ((layer != null) ? layer.Tiles[(int)location.X, (int)location.Y] : null) == null;
			}
			return false;
		}

		// Token: 0x06000F39 RID: 3897 RVA: 0x000AA250 File Offset: 0x000A8450
		public virtual bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			if (furniture == null)
			{
				return false;
			}
			bool isIndoor = this is DecoratableLocation || !this.IsOutdoors;
			if (furniture.furniture_type.Value == 15)
			{
				bool allowBedsHere;
				if (!this.TryGetMapPropertyAs("AllowBeds", out allowBedsHere, false))
				{
					allowBedsHere = (this is FarmHouse || this is IslandFarmHouse || (isIndoor && this.ParentBuilding != null));
				}
				if (!allowBedsHere)
				{
					return false;
				}
			}
			switch (furniture.placementRestriction)
			{
			case 0:
				return isIndoor;
			case 1:
				return !isIndoor;
			case 2:
				return isIndoor || !isIndoor;
			default:
				return false;
			}
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x000AA2EC File Offset: 0x000A84EC
		public virtual bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
		{
			if (this.IsLocationSpecificPlacementRestriction(v))
			{
				return false;
			}
			if (!this.hasTileAt((int)v.X, (int)v.Y, "Back", null))
			{
				return false;
			}
			if (this.isWaterTile((int)v.X, (int)v.Y))
			{
				return false;
			}
			string noFurniture = this.doesTileHaveProperty((int)v.X, (int)v.Y, "NoFurniture", "Back", false);
			if (noFurniture != null)
			{
				if (noFurniture == "total")
				{
					return false;
				}
				if (!itemIsPassable || !Game1.currentLocation.IsOutdoors)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x000AA380 File Offset: 0x000A8580
		public void playTerrainSound(Vector2 tileLocation, Character who = null, bool showTerrainDisturbAnimation = true)
		{
			string currentStep = "thudStep";
			if (this.IsOutdoors || this.treatAsOutdoors.Value || this.Name.ContainsIgnoreCase("mine"))
			{
				string stepType = this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back", false);
				if (stepType != null)
				{
					if (!(stepType == "Dirt"))
					{
						if (!(stepType == "Stone"))
						{
							if (!(stepType == "Grass"))
							{
								if (stepType == "Wood")
								{
									currentStep = "woodyStep";
								}
							}
							else
							{
								currentStep = ((this.GetSeason() == Season.Winter) ? "snowyStep" : "grassyStep");
							}
						}
						else
						{
							currentStep = "stoneStep";
						}
					}
					else
					{
						currentStep = "sandyStep";
					}
				}
				else if (this.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					currentStep = "waterSlosh";
				}
			}
			TerrainFeature terrainFeature;
			if (this.terrainFeatures.TryGetValue(tileLocation, out terrainFeature) && terrainFeature is Flooring)
			{
				currentStep = ((Flooring)this.terrainFeatures[tileLocation]).getFootstepSound();
			}
			if (who != null && showTerrainDisturbAnimation && currentStep == "sandyStep")
			{
				Vector2 offset = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset = who.drawOffset;
				}
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64), 50f, 4, 1, new Vector2(who.Position.X + (float)Game1.random.Next(-8, 8), who.Position.Y + (float)Game1.random.Next(-16, 0)) + offset, false, Game1.random.NextBool(), 0.0001f, 0f, Color.White, 1f, 0.01f, 0f, (float)Game1.random.Next(-5, 6) * 3.1415927f / 128f, false));
			}
			else if (who != null && showTerrainDisturbAnimation && this.GetSeason() == Season.Winter && currentStep == "grassyStep")
			{
				Vector2 offset2 = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset2 = who.drawOffset;
				}
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(who.Position.X, who.Position.Y) + offset2, false, false, 0.0001f, 0.001f, Color.White, 1f, 0.01f, 0f, 0f, false));
			}
			Farmer farmer = who as Farmer;
			string a;
			if (farmer == null)
			{
				a = null;
			}
			else
			{
				Boots value = farmer.boots.Value;
				a = ((value != null) ? value.ItemId : null);
			}
			if (a == "853")
			{
				this.localSound("jingleBell", null, null, SoundContext.Default);
			}
			if (currentStep.Length > 0)
			{
				this.localSound(currentStep, null, null, SoundContext.Default);
			}
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x000AA69C File Offset: 0x000A889C
		public bool checkTileIndexAction(int tileIndex)
		{
			if ((tileIndex == 1799 || tileIndex - 1824 <= 9) && this.Name.Equals("AbandonedJojaMart"))
			{
				Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart", false).checkBundle();
				return true;
			}
			return false;
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x000AA6D8 File Offset: 0x000A88D8
		public bool checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(int x, int y)
		{
			Vector2 v = new Vector2((float)x, (float)y);
			Object tileObj;
			if (this.objects.TryGetValue(v, out tileObj))
			{
				if (!tileObj.IsSpawnedObject || tileObj is Chest || tileObj.Type == "Crafting")
				{
					return false;
				}
				this.objects.Remove(v);
			}
			this.terrainFeatures.Remove(v);
			return true;
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x000AA740 File Offset: 0x000A8940
		public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			who.ignoreItemConsumptionThisFrame = false;
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (!this.objects.ContainsKey(new Vector2((float)tileLocation.X, (float)tileLocation.Y)) && this.CheckPetAnimal(tileRect, who))
			{
				return true;
			}
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.doAction(new Vector2((float)tileLocation.X, (float)tileLocation.Y), who))
					{
						return true;
					}
				}
			}
			if (who.IsSitting())
			{
				who.StopSitting(true);
				return true;
			}
			foreach (Farmer farmer in this.farmers)
			{
				if (farmer != Game1.player && farmer.GetBoundingBox().Intersects(tileRect) && farmer.checkAction(who, this))
				{
					return true;
				}
			}
			if (this.currentEvent != null && this.currentEvent.isFestival)
			{
				return this.currentEvent.checkAction(tileLocation, viewport, who);
			}
			foreach (NPC i in this.characters)
			{
				if (i != null && !i.IsMonster && (!who.isRidingHorse() || !(i is Horse)) && i.GetBoundingBox().Intersects(tileRect) && i.checkAction(who, this))
				{
					if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, false) || who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, true))
					{
						who.faceGeneralDirection(i.getStandingPosition(), 0, false, false);
					}
					return true;
				}
			}
			int tileIndex = this.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
			if (this.NameOrUniqueName == "SkullCave" && (tileIndex == 344 || tileIndex == 349))
			{
				if (Game1.player.team.SpecialOrderActive("QiChallenge10"))
				{
					who.doEmote(40);
					return false;
				}
				if (!Game1.player.team.completedSpecialOrders.Contains("QiChallenge10"))
				{
					who.doEmote(8);
					return false;
				}
				if (!Game1.player.team.toggleSkullShrineOvernight.Value)
				{
					if (!Game1.player.team.skullShrineActivated.Value)
					{
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_NotYetHard"), this.createYesNoResponses(), "ShrineOfSkullChallenge");
					}
					else
					{
						Game1.player.team.toggleSkullShrineOvernight.Value = true;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
						Game1.multiplayer.globalChatInfoMessage(Game1.player.team.skullShrineActivated.Value ? "HardModeSkullCaveDeactivated" : "HardModeSkullCaveActivated", new string[]
						{
							who.name.Value
						});
						this.playSound(Game1.player.team.skullShrineActivated.Value ? "skeletonStep" : "serpentDie", null, null, SoundContext.Default);
					}
				}
				else if (Game1.player.team.toggleSkullShrineOvernight.Value && Game1.player.team.skullShrineActivated.Value)
				{
					Game1.player.team.toggleSkullShrineOvernight.Value = false;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling"));
					this.playSound("skeletonStep", null, null, SoundContext.Default);
				}
				return true;
			}
			else
			{
				foreach (ResourceClump stump in this.resourceClumps)
				{
					if (stump.getBoundingBox().Intersects(tileRect) && stump.performUseAction(new Vector2((float)tileLocation.X, (float)tileLocation.Y)))
					{
						return true;
					}
				}
				Vector2 tilePos = new Vector2((float)tileLocation.X, (float)tileLocation.Y);
				Object obj;
				if (this.objects.TryGetValue(tilePos, out obj))
				{
					bool isErrorItem = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId).IsErrorItem;
					if (obj.Type != null || isErrorItem)
					{
						if (who.isRidingHorse() && !(obj is Fence))
						{
							return false;
						}
						if (tilePos == who.Tile && !obj.isPassable())
						{
							Fence fence = obj as Fence;
							if (fence == null || !fence.isGate.Value)
							{
								Tool t = ItemRegistry.Create<Tool>("(T)Pickaxe", 1, 0, false);
								t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
								if (obj.performToolAction(t))
								{
									obj.performRemoveAction();
									obj.dropItem(this, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel));
									Game1.currentLocation.Objects.Remove(tilePos);
									return true;
								}
								t = ItemRegistry.Create<Tool>("(T)Axe", 1, 0, false);
								t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
								if (this.objects.TryGetValue(tilePos, out obj) && obj.performToolAction(t))
								{
									obj.performRemoveAction();
									obj.dropItem(this, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel));
									Game1.currentLocation.Objects.Remove(tilePos);
									return true;
								}
								if (!this.objects.TryGetValue(tilePos, out obj))
								{
									return true;
								}
							}
						}
						if (this.objects.TryGetValue(tilePos, out obj) && (obj.Type == "Crafting" || obj.Type == "interactive"))
						{
							if (who.ActiveObject == null && obj.checkForAction(who, false))
							{
								return true;
							}
							if (this.objects.TryGetValue(tilePos, out obj))
							{
								if (who.CurrentItem == null)
								{
									return obj.checkForAction(who, false);
								}
								Object old_held_object = obj.heldObject.Value;
								obj.heldObject.Value = null;
								bool probe_returned_true = obj.performObjectDropInAction(who.CurrentItem, true, who, false);
								obj.heldObject.Value = old_held_object;
								bool perform_returned_true = obj.performObjectDropInAction(who.CurrentItem, false, who, true);
								if ((probe_returned_true || perform_returned_true) && who.isMoving())
								{
									Game1.haltAfterCheck = false;
								}
								if (who.ignoreItemConsumptionThisFrame)
								{
									return true;
								}
								if (perform_returned_true)
								{
									who.reduceActiveItemByOne();
									return true;
								}
								return obj.checkForAction(who, false) || probe_returned_true;
							}
						}
						else if (this.objects.TryGetValue(tilePos, out obj) && (obj.isSpawnedObject.Value || isErrorItem))
						{
							int oldQuality = obj.quality.Value;
							Random r = Utility.CreateDaySaveRandom((double)tilePos.X, (double)(tilePos.Y * 777f), 0.0);
							if (obj.isForage())
							{
								obj.Quality = this.GetHarvestSpawnedObjectQuality(who, obj.isForage(), obj.TileLocation, r);
							}
							if (obj.questItem.Value && obj.questId.Value != null && obj.questId.Value != "0" && !who.hasQuest(obj.questId.Value))
							{
								return false;
							}
							if (who.couldInventoryAcceptThisItem(obj))
							{
								if (who.IsLocalPlayer)
								{
									this.localSound("pickUpItem", null, null, SoundContext.Default);
									DelayedAction.playSoundAfterDelay("coin", 300, null, null, -1, false);
								}
								who.animateOnce(279 + who.FacingDirection);
								if (!this.isFarmBuildingInterior())
								{
									if (obj.isForage())
									{
										this.OnHarvestedForage(who, obj);
									}
									if (obj.ItemId.Equals("789") && this.Name.Equals("LewisBasement"))
									{
										Bat b = new Bat(Vector2.Zero, -789);
										b.focusedOnFarmers = true;
										Game1.changeMusicTrack("none", false, MusicContext.Default);
										this.playSound("cursed_mannequin", null, null, SoundContext.Default);
										this.characters.Add(b);
									}
								}
								else
								{
									who.gainExperience(0, 5);
								}
								who.addItemToInventoryBool(obj.getOne(), false);
								Stats stats = Game1.stats;
								uint itemsForaged = stats.ItemsForaged;
								stats.ItemsForaged = itemsForaged + 1U;
								if (who.professions.Contains(13) && r.NextDouble() < 0.2 && !obj.questItem.Value && who.couldInventoryAcceptThisItem(obj) && !this.isFarmBuildingInterior())
								{
									who.addItemToInventoryBool(obj.getOne(), false);
									who.gainExperience(2, 7);
								}
								this.objects.Remove(tilePos);
								return true;
							}
							obj.Quality = oldQuality;
						}
					}
				}
				if (who.isRidingHorse())
				{
					who.mount.checkAction(who, this);
					return true;
				}
				foreach (KeyValuePair<Vector2, TerrainFeature> v in this.terrainFeatures.Pairs)
				{
					if (v.Value.getBoundingBox().Intersects(tileRect) && v.Value.performUseAction(v.Key))
					{
						Game1.haltAfterCheck = false;
						return true;
					}
				}
				if (this.largeTerrainFeatures != null)
				{
					foreach (LargeTerrainFeature f in this.largeTerrainFeatures)
					{
						if (f.getBoundingBox().Intersects(tileRect) && f.performUseAction(f.Tile))
						{
							Game1.haltAfterCheck = false;
							return true;
						}
					}
				}
				Tile tile = this.map.RequireLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
				string action;
				if (tile == null || !tile.Properties.TryGetValue("Action", out action))
				{
					action = this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings", false);
				}
				if (action != null)
				{
					NPC characterAtTile = this.isCharacterAtTile(tilePos + new Vector2(0f, 1f));
					if (this.currentEvent == null && characterAtTile != null && !characterAtTile.IsInvisible && !characterAtTile.IsMonster && (!who.isRidingHorse() || !(characterAtTile is Horse)))
					{
						Point characterPixel = characterAtTile.StandingPixel;
						if (Utility.withinRadiusOfPlayer(characterPixel.X, characterPixel.Y, 1, who) && characterAtTile.checkAction(who, this))
						{
							if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, who.IsCarrying()))
							{
								who.faceGeneralDirection(Utility.PointToVector2(characterPixel), 0, false, false);
							}
							return true;
						}
					}
					return this.performAction(action, who, tileLocation);
				}
				if (tile != null && this.checkTileIndexAction(tile.TileIndex))
				{
					return true;
				}
				foreach (MapSeat seat in this.mapSeats)
				{
					if (seat.OccupiesTile(tileLocation.X, tileLocation.Y) && !seat.IsBlocked(this))
					{
						who.BeginSitting(seat);
						return true;
					}
				}
				Point vectOnWall = new Point(tileLocation.X * 64, (tileLocation.Y - 1) * 64);
				bool didRightClick = Game1.didPlayerJustRightClick(false);
				Furniture paintingFound = null;
				foreach (Furniture f2 in this.furniture)
				{
					if (f2.boundingBox.Value.Contains((int)(tilePos.X * 64f), (int)(tilePos.Y * 64f)) && f2.furniture_type.Value != 12)
					{
						if (!didRightClick)
						{
							return f2.clicked(who);
						}
						if (who.ActiveObject != null && f2.performObjectDropInAction(who.ActiveObject, false, who, false))
						{
							return true;
						}
						return f2.checkForAction(who, false);
					}
					else if (f2.furniture_type.Value == 6 && f2.boundingBox.Value.Contains(vectOnWall))
					{
						paintingFound = f2;
					}
				}
				if (paintingFound == null)
				{
					return Game1.didPlayerJustRightClick(true) && this.animals.Length > 0 && this.CheckInspectAnimal(tileRect, who);
				}
				if (didRightClick)
				{
					return (who.ActiveObject != null && paintingFound.performObjectDropInAction(who.ActiveObject, false, who, false)) || paintingFound.checkForAction(who, false);
				}
				return paintingFound.clicked(who);
			}
			bool result;
			return result;
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x000AB498 File Offset: 0x000A9698
		public int GetHarvestSpawnedObjectQuality(Farmer who, bool isForage, Vector2 tile, Random random = null)
		{
			if (who.professions.Contains(16) && isForage)
			{
				return 4;
			}
			if (isForage)
			{
				if (random == null)
				{
					random = Utility.CreateDaySaveRandom((double)tile.X, (double)(tile.Y * 777f), 0.0);
				}
				if (random.NextBool((float)who.ForagingLevel / 30f))
				{
					return 2;
				}
				if (random.NextBool((float)who.ForagingLevel / 15f))
				{
					return 1;
				}
			}
			return 0;
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x000AB514 File Offset: 0x000A9714
		public void OnHarvestedForage(Farmer who, Object forage)
		{
			if (forage.SpecialVariable == 724519)
			{
				who.gainExperience(2, 2);
				who.gainExperience(0, 3);
				return;
			}
			who.gainExperience(2, 7);
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x000AB53C File Offset: 0x000A973C
		public virtual bool CanFreePlaceFurniture()
		{
			return false;
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x000AB540 File Offset: 0x000A9740
		public virtual bool LowPriorityLeftClick(int x, int y, Farmer who)
		{
			if (Game1.activeClickableMenu != null)
			{
				return false;
			}
			for (int i = this.furniture.Count - 1; i >= 0; i--)
			{
				Furniture furnitureItem = this.furniture[i];
				if (this.CanFreePlaceFurniture() || furnitureItem.IsCloseEnoughToFarmer(who, null, null))
				{
					if (!furnitureItem.isPassable() && furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.canBeRemoved(who))
					{
						furnitureItem.AttemptRemoval(delegate(Furniture f)
						{
							Guid guid = this.furniture.GuidOf(f);
							if (!this.furnitureToRemove.Contains(guid))
							{
								this.furnitureToRemove.Add(guid);
							}
						});
						return true;
					}
					if (furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.heldObject.Value != null)
					{
						furnitureItem.clicked(who);
						return true;
					}
					if (!furnitureItem.isGroundFurniture() && furnitureItem.canBeRemoved(who))
					{
						int wall_y = y;
						DecoratableLocation decoratableLocation = this as DecoratableLocation;
						if (decoratableLocation != null)
						{
							wall_y = decoratableLocation.GetWallTopY(x / 64, y / 64);
							if (wall_y == -1)
							{
								wall_y = y * 64;
							}
							else
							{
								wall_y *= 64;
							}
						}
						if (furnitureItem.boundingBox.Value.Contains(x, wall_y))
						{
							furnitureItem.AttemptRemoval(delegate(Furniture f)
							{
								Guid guid = this.furniture.GuidOf(f);
								if (!this.furnitureToRemove.Contains(guid))
								{
									this.furnitureToRemove.Add(guid);
								}
							});
							return true;
						}
					}
				}
			}
			for (int j = this.furniture.Count - 1; j >= 0; j--)
			{
				Furniture furnitureItem2 = this.furniture[j];
				if ((this.CanFreePlaceFurniture() || furnitureItem2.IsCloseEnoughToFarmer(who, null, null)) && furnitureItem2.isPassable() && furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.canBeRemoved(who))
				{
					furnitureItem2.AttemptRemoval(delegate(Furniture f)
					{
						Guid guid = this.furniture.GuidOf(f);
						if (!this.furnitureToRemove.Contains(guid))
						{
							this.furnitureToRemove.Add(guid);
						}
					});
					return true;
				}
			}
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
			return Game1.didPlayerJustRightClick(true) && this.CheckInspectAnimal(tileRect, who);
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x000AB73D File Offset: 0x000A993D
		[Obsolete("These values returned by this function are no longer used by the game (except for rare, backwards compatibility related cases.) Check DecoratableLocation's wallpaper/flooring related functionality instead.")]
		public virtual List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>();
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x000AB744 File Offset: 0x000A9944
		protected virtual void removeQueuedFurniture(Guid guid)
		{
			Farmer who = Game1.player;
			Furniture furnitureItem;
			if (!this.furniture.TryGetValue(guid, out furnitureItem) || !who.couldInventoryAcceptThisItem(furnitureItem))
			{
				return;
			}
			furnitureItem.performRemoveAction();
			this.furniture.Remove(guid);
			bool foundInToolbar = false;
			for (int i = 0; i < 12; i++)
			{
				if (who.Items[i] == null)
				{
					who.Items[i] = furnitureItem;
					who.CurrentToolIndex = i;
					foundInToolbar = true;
					break;
				}
			}
			if (!foundInToolbar)
			{
				Item item = who.addItemToInventory(furnitureItem, 11);
				who.addItemToInventory(item);
				who.CurrentToolIndex = 11;
			}
			this.localSound("coin", null, null, SoundContext.Default);
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x000AB7F8 File Offset: 0x000A99F8
		public virtual bool leftClick(int x, int y, Farmer who)
		{
			Vector2 clickTile = new Vector2((float)(x / 64), (float)(y / 64));
			foreach (Building building in this.buildings)
			{
				if (building.CanLeftClick(x, y) && building.leftClicked())
				{
					return true;
				}
			}
			Object clickedObj;
			if (this.objects.TryGetValue(clickTile, out clickedObj) && clickedObj.clicked(who))
			{
				this.objects.Remove(clickTile);
				return true;
			}
			return false;
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x000AB898 File Offset: 0x000A9A98
		public virtual bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (this.doesTileHaveProperty((int)p.X, (int)p.Y, "Passable", "Buildings", false) != null)
			{
				return true;
			}
			TerrainFeature feature;
			if (this.terrainFeatures.TryGetValue(p, out feature) && feature is HoeDirt)
			{
				return true;
			}
			if (this.isWaterTile((int)p.X, (int)p.Y))
			{
				int tileIndex = this.getTileIndexAt((int)p.X, (int)p.Y, "Buildings", "Town");
				if (tileIndex < 1004 || tileIndex > 1013)
				{
					return true;
				}
			}
			foreach (Building building in this.buildings)
			{
				if (building.occupiesTile(p, false) && building.isTilePassable(p))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x000AB984 File Offset: 0x000A9B84
		public virtual Chest GetFridge(bool onlyUnlocked = true)
		{
			FarmHouse home = this as FarmHouse;
			if (home == null)
			{
				IslandFarmHouse islandHome = this as IslandFarmHouse;
				if (islandHome != null)
				{
					if (!onlyUnlocked || islandHome.fridgePosition != Point.Zero)
					{
						return islandHome.fridge.Value;
					}
				}
			}
			else if (!onlyUnlocked || home.fridgePosition != Point.Zero)
			{
				return home.fridge.Value;
			}
			return null;
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x000AB9EC File Offset: 0x000A9BEC
		public virtual Point? GetFridgePosition()
		{
			FarmHouse home = this as FarmHouse;
			if (home == null)
			{
				IslandFarmHouse islandHome = this as IslandFarmHouse;
				if (islandHome != null)
				{
					if (islandHome.fridgePosition != Point.Zero)
					{
						return new Point?(islandHome.fridgePosition);
					}
				}
			}
			else if (home.fridgePosition != Point.Zero)
			{
				return new Point?(home.fridgePosition);
			}
			return null;
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x000ABA58 File Offset: 0x000A9C58
		public void ActivateKitchen()
		{
			List<NetMutex> muticies = new List<NetMutex>();
			List<Chest> mini_fridges = new List<Chest>();
			foreach (Object item in this.objects.Values)
			{
				if (item != null && item.bigCraftable.Value)
				{
					Chest chest = item as Chest;
					if (chest != null && chest.fridge.Value)
					{
						mini_fridges.Add(chest);
						muticies.Add(chest.mutex);
					}
				}
			}
			Chest fridge = this.GetFridge(true);
			if (fridge != null)
			{
				muticies.Add(fridge.mutex);
			}
			new MultipleMutexRequest(muticies, delegate(MultipleMutexRequest request)
			{
				List<IInventory> materialChests = new List<IInventory>();
				if (fridge != null)
				{
					materialChests.Add(fridge.Items);
				}
				foreach (Chest miniFridge in mini_fridges)
				{
					materialChests.Add(miniFridge.Items);
				}
				Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
				Game1.activeClickableMenu = new CraftingPage((int)center.X, (int)center.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true, materialChests);
				Game1.activeClickableMenu.exitFunction = new IClickableMenu.onExit(request.ReleaseLocks);
			}, delegate(MultipleMutexRequest request)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"), true);
			});
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x000ABB5C File Offset: 0x000A9D5C
		public void openDoor(Location tileLocation, bool playSound)
		{
			try
			{
				int tileIndex = this.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "1");
				Point point = new Point(tileLocation.X, tileLocation.Y);
				if (this.interiorDoors.ContainsKey(point))
				{
					this.interiorDoors[point] = true;
					if (playSound)
					{
						Vector2 pos = new Vector2((float)tileLocation.X, (float)tileLocation.Y);
						if (tileIndex == 120)
						{
							this.playSound("doorOpen", new Vector2?(pos), null, SoundContext.Default);
						}
						else
						{
							this.playSound("doorCreak", new Vector2?(pos), null, SoundContext.Default);
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x000ABC20 File Offset: 0x000A9E20
		public void doStarpoint(string which)
		{
			if (!(which == "3"))
			{
				if (!(which == "4"))
				{
					return;
				}
				if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.QualifiedItemId == "(O)203")
				{
					Object reward = ItemRegistry.Create<Object>("(BC)162", 1, 0, false);
					if (!Game1.player.couldInventoryAcceptThisItem(reward) && Game1.player.ActiveObject.stack.Value > 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
						return;
					}
					Game1.player.reduceActiveItemByOne();
					Game1.player.makeThisTheActiveObject(reward);
					this.localSound("croak", null, null, SoundContext.Default);
					Game1.flashAlpha = 1f;
				}
			}
			else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.QualifiedItemId == "(O)307")
			{
				Object reward2 = ItemRegistry.Create<Object>("(BC)161", 1, 0, false);
				if (!Game1.player.couldInventoryAcceptThisItem(reward2) && Game1.player.ActiveObject.stack.Value > 1)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					return;
				}
				Game1.player.reduceActiveItemByOne();
				Game1.player.makeThisTheActiveObject(reward2);
				this.localSound("discoverMineral", null, null, SoundContext.Default);
				Game1.flashAlpha = 1f;
				return;
			}
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x000ABDB0 File Offset: 0x000A9FB0
		public virtual string FormatCompletionLine(Func<Farmer, float> check)
		{
			KeyValuePair<Farmer, float> kvp = Utility.GetFarmCompletion(check);
			if (kvp.Key == Game1.player)
			{
				return kvp.Value.ToString();
			}
			return "(" + kvp.Key.Name + ") " + kvp.Value.ToString();
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x000ABE0C File Offset: 0x000AA00C
		public virtual string FormatCompletionLine(Func<Farmer, bool> check, string true_value, string false_value)
		{
			KeyValuePair<Farmer, bool> kvp = Utility.GetFarmCompletion(check);
			if (kvp.Key != Game1.player)
			{
				return "(" + kvp.Key.Name + ") " + (kvp.Value ? true_value : false_value);
			}
			if (!kvp.Value)
			{
				return false_value;
			}
			return true_value;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x000ABE64 File Offset: 0x000AA064
		public virtual void ShowQiCat()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.MasterPlayer.mailReceived.Contains("GotPerfectionStatue"))
			{
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "GotPerfectionStatue", MailType.Received, true, null);
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)280", 1, 0, false), null, false);
				return;
			}
			if (!Game1.player.hasOrWillReceiveMail("FizzIntro"))
			{
				Game1.addMailForTomorrow("FizzIntro", false, true);
			}
			Game1.playSound("qi_shop", null);
			int totalWaivers = Game1.netWorldState.Value.PerfectionWaivers;
			double totalPercent = Math.Floor((double)(Utility.percentGameComplete() * 100f));
			if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ja || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
			{
				totalPercent += (double)totalWaivers;
			}
			string totalStr;
			if (totalWaivers != 0)
			{
				if (totalWaivers != 1)
				{
					totalStr = Game1.content.LoadString("Strings\\UI:PT_Total_ValueWithWaivers", totalPercent, totalWaivers);
				}
				else
				{
					totalStr = Game1.content.LoadString("Strings\\UI:PT_Total_ValueWithWaiver", totalPercent);
				}
			}
			else
			{
				totalStr = Game1.content.LoadString("Strings\\UI:PT_Total_Value", totalPercent);
			}
			string[] array = new string[15];
			array[0] = Utility.loadStringShort("UI", "PT_Title") + "^";
			array[1] = "----------------^";
			array[2] = Utility.loadStringShort("UI", "PT_Shipped") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor((double)(Utility.getFarmerItemsShippedPercent(farmer) * 100f))) + "%^";
			array[3] = Utility.loadStringShort("UI", "PT_Obelisks") + ": " + Math.Min(Utility.GetObeliskTypesBuilt(), 4).ToString() + "/4^";
			array[4] = Utility.loadStringShort("UI", "PT_GoldClock") + ": " + (Game1.IsBuildingConstructed("Gold Clock") ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^";
			array[5] = Utility.loadStringShort("UI", "PT_MonsterSlayer") + ": " + this.FormatCompletionLine((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value, Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^";
			array[6] = Utility.loadStringShort("UI", "PT_GreatFriends") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor((double)(Utility.getMaxedFriendshipPercent(farmer) * 100f))) + "%^";
			array[7] = Utility.loadStringShort("UI", "PT_FarmerLevel") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Min(farmer.Level, 25)) + "/25^";
			array[8] = Utility.loadStringShort("UI", "PT_Stardrops") + ": " + this.FormatCompletionLine((Farmer farmer) => Utility.foundAllStardrops(farmer), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^";
			array[9] = Utility.loadStringShort("UI", "PT_Cooking") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor((double)(Utility.getCookedRecipesPercent(farmer) * 100f))) + "%^";
			array[10] = Utility.loadStringShort("UI", "PT_Crafting") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor((double)(Utility.getCraftedRecipesPercent(farmer) * 100f))) + "%^";
			array[11] = Utility.loadStringShort("UI", "PT_Fish") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor((double)(Utility.getFishCaughtPercent(farmer) * 100f))) + "%^";
			array[12] = string.Concat(new string[]
			{
				Utility.loadStringShort("UI", "PT_GoldenWalnut"),
				": ",
				Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, 130).ToString(),
				"/",
				130.ToString(),
				"^"
			});
			array[13] = "----------------^";
			array[14] = Utility.loadStringShort("UI", "PT_Total") + ": " + totalStr;
			List<string> brokenUp = SpriteText.getStringBrokenIntoSectionsOfHeight(string.Concat(array), 9999, Game1.uiViewport.Height - 100);
			for (int i = 0; i < brokenUp.Count - 1; i++)
			{
				List<string> list = brokenUp;
				int index = i;
				list[index] += "...\n";
			}
			Game1.drawDialogueNoTyping(brokenUp);
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x000AC3C0 File Offset: 0x000AA5C0
		public virtual bool CheckGarbage(string id, Vector2 tile, Farmer who, bool playAnimations = true, bool reactNpcs = true, Action<string> logError = null)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				if (logError != null)
				{
					logError("must specify a garbage can ID");
				}
				return false;
			}
			if (id != null)
			{
				int num = id.Length;
				if (num == 1)
				{
					switch (id[0])
					{
					case '0':
						id = "JodiAndKent";
						break;
					case '1':
						id = "EmilyAndHaley";
						break;
					case '2':
						id = "Mayor";
						break;
					case '3':
						id = "Museum";
						break;
					case '4':
						id = "Blacksmith";
						break;
					case '5':
						id = "Saloon";
						break;
					case '6':
						id = "Evelyn";
						break;
					case '7':
						id = "JojaMart";
						break;
					}
				}
			}
			if (!Game1.netWorldState.Value.CheckedGarbage.Add(id))
			{
				Game1.haltAfterCheck = false;
				return true;
			}
			Random garbageRandom;
			Item item;
			GarbageCanItemData selected;
			this.TryGetGarbageItem(id, who.DailyLuck, out item, out selected, out garbageRandom, logError);
			if (playAnimations)
			{
				bool doubleMega = selected != null && selected.IsDoubleMegaSuccess;
				bool mega = !doubleMega && selected != null && selected.IsMegaSuccess;
				if (doubleMega)
				{
					this.playSound("explosion", null, null, SoundContext.Default);
				}
				else if (mega)
				{
					this.playSound("crit", null, null, SoundContext.Default);
				}
				this.playSound("trashcan", null, null, SoundContext.Default);
				int tileY = (int)tile.Y;
				int xSourceOffset = this.GetSeasonIndex() * 17;
				TemporaryAnimatedSprite lidSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 0, 16, 10), tile * 64f + new Vector2(0f, -6f) * 4f, false, 0f, Color.White)
				{
					interval = (float)(doubleMega ? 4000 : 1000),
					motion = (doubleMega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? -7f : ((float)(garbageRandom.Next(-1, 3) + ((garbageRandom.NextDouble() < 0.1) ? -2 : 0)))))),
					rotationChange = (doubleMega ? 0.4f : 0f),
					acceleration = new Vector2(0f, 0.7f),
					yStopCoordinate = tileY * 64 + -24,
					layerDepth = (doubleMega ? 1f : ((float)((tileY + 1) * 64 + 2) / 10000f)),
					scale = 4f,
					Parent = this,
					shakeIntensity = (doubleMega ? 0f : 1f),
					reachedStopCoordinate = delegate(int x)
					{
						this.removeTemporarySpritesWithID(97654);
						this.playSound("thudStep", null, null, SoundContext.Default);
						for (int j = 0; j < 3; j++)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), tile * 64f + new Vector2((float)(j * 6), (float)(-3 + garbageRandom.Next(3))) * 4f, false, 0.02f, Color.DimGray)
							{
								alpha = 0.85f,
								motion = new Vector2(-0.6f + (float)j * 0.3f, -1f),
								acceleration = new Vector2(0.002f, 0f),
								interval = 99999f,
								layerDepth = (float)((tileY + 1) * 64 + 3) / 10000f,
								scale = 3f,
								scaleChange = 0.02f,
								rotationChange = (float)garbageRandom.Next(-5, 6) * 3.1415927f / 256f,
								delayBeforeAnimationStart = 50
							});
						}
					},
					id = 97654
				};
				TemporaryAnimatedSprite bodySprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 11, 16, 16), tile * 64f + new Vector2(0f, -5f) * 4f, false, 0f, Color.White)
				{
					interval = (float)(doubleMega ? 999999 : 1000),
					layerDepth = (float)((tileY + 1) * 64 + 1) / 10000f,
					scale = 4f,
					id = 97654
				};
				if (doubleMega)
				{
					lidSprite.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(lidSprite.bounce);
				}
				TemporaryAnimatedSpriteList trashCanSprites = new TemporaryAnimatedSpriteList
				{
					lidSprite,
					bodySprite
				};
				for (int i = 0; i < 5; i++)
				{
					TemporaryAnimatedSprite particleSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + garbageRandom.Next(4) * 4, 32, 4, 4), tile * 64f + new Vector2((float)Game1.random.Next(13), (float)(-3 + Game1.random.Next(3))) * 4f, false, 0f, Color.White)
					{
						interval = 500f,
						motion = new Vector2((float)garbageRandom.Next(-2, 3), -5f),
						acceleration = new Vector2(0f, 0.4f),
						layerDepth = (float)((tileY + 1) * 64 + 3) / 10000f,
						scale = 4f,
						color = Utility.getRandomRainbowColor(garbageRandom),
						delayBeforeAnimationStart = garbageRandom.Next(100)
					};
					trashCanSprites.Add(particleSprite);
				}
				Game1.multiplayer.broadcastSprites(this, trashCanSprites);
			}
			if (reactNpcs)
			{
				foreach (NPC npc in Utility.GetNpcsWithinDistance(tile, 7, this))
				{
					if (!(npc is Horse))
					{
						Game1.multiplayer.globalChatInfoMessage("TrashCan", new string[]
						{
							who.Name,
							npc.GetTokenizedDisplayName()
						});
						if (npc.Name == "Linus")
						{
							Game1.multiplayer.globalChatInfoMessage("LinusTrashCan", Array.Empty<string>());
						}
						CharacterData data = npc.GetData();
						int friendshipChange = (data != null) ? data.DumpsterDiveFriendshipEffect : -25;
						int? emote = (data != null) ? data.DumpsterDiveEmote : null;
						Dialogue dialogue = npc.TryGetDialogue("DumpsterDiveComment");
						int num = npc.Age;
						if (num != 1)
						{
							if (num == 2)
							{
								emote = new int?(emote.GetValueOrDefault(28));
								dialogue = (dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Child", false));
							}
							else
							{
								emote = new int?(emote.GetValueOrDefault(12));
								dialogue = (dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult", false));
							}
						}
						else
						{
							emote = new int?(emote.GetValueOrDefault(8));
							dialogue = (dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen", false));
						}
						npc.doEmote(emote.Value, true);
						who.changeFriendship(friendshipChange, npc);
						npc.setNewDialogue(dialogue, true, true);
						Game1.drawDialogue(npc);
						break;
					}
				}
			}
			Game1.stats.Increment("trashCansChecked", 1U);
			if (selected != null)
			{
				if (selected.AddToInventoryDirectly)
				{
					who.addItemByMenuIfNecessary(item, null, false);
				}
				else
				{
					Vector2 origin = new Vector2(tile.X + 0.5f, tile.Y - 1f) * 64f;
					if (selected.CreateMultipleDebris)
					{
						Game1.createMultipleItemDebris(item, origin, 2, this, (int)origin.Y + 64, false);
					}
					else
					{
						Game1.createItemDebris(item, origin, 2, this, (int)origin.Y + 64, false);
					}
				}
			}
			return true;
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x000ACB0C File Offset: 0x000AAD0C
		public virtual bool TryGetGarbageItem(string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, Action<string> logError = null)
		{
			GarbageCanData allData = DataLoader.GarbageCans(Game1.content);
			GarbageCanEntryData data = allData.GarbageCans.GetValueOrDefault(id);
			float baseChance = (data != null && data.BaseChance > 0f) ? data.BaseChance : allData.DefaultBaseChance;
			baseChance += (float)dailyLuck;
			if (Game1.player.stats.Get("Book_Trash") > 0U)
			{
				baseChance += 0.2f;
			}
			garbageRandom = Utility.CreateDaySaveRandom((double)(777 + Game1.hash.GetDeterministicHashCode(id)), 0.0, 0.0);
			int prewarm = garbageRandom.Next(0, 100);
			for (int i = 0; i < prewarm; i++)
			{
				garbageRandom.NextDouble();
			}
			prewarm = garbageRandom.Next(0, 100);
			for (int j = 0; j < prewarm; j++)
			{
				garbageRandom.NextDouble();
			}
			selected = null;
			item = null;
			bool baseChancePassed = garbageRandom.NextDouble() < (double)baseChance;
			ItemQueryContext itemQueryContext = new ItemQueryContext(this, Game1.player, garbageRandom, "garbage data '" + id + "'");
			foreach (List<GarbageCanItemData> itemList in new List<GarbageCanItemData>[]
			{
				allData.BeforeAll,
				(data != null) ? data.Items : null,
				allData.AfterAll
			})
			{
				if (itemList != null)
				{
					foreach (GarbageCanItemData entry in itemList)
					{
						if (string.IsNullOrWhiteSpace(entry.Id))
						{
							logError("ignored item entry with no Id field.");
						}
						else if ((baseChancePassed || entry.IgnoreBaseChance) && GameStateQuery.CheckConditions(entry.Condition, this, null, null, null, garbageRandom, null))
						{
							bool error = false;
							Item result = ItemQueryResolver.TryResolveRandomItem(entry, itemQueryContext, false, null, null, null, delegate(string query, string message)
							{
								error = true;
								logError("failed parsing item query '" + query + "': " + message);
							});
							if (!error)
							{
								selected = entry;
								item = result;
								break;
							}
						}
					}
					if (selected != null)
					{
						break;
					}
				}
			}
			return item != null;
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x000ACD60 File Offset: 0x000AAF60
		public virtual bool performAction(string fullActionString, Farmer who, Location tileLocation)
		{
			if (fullActionString == null)
			{
				return false;
			}
			string[] action = ArgUtility.SplitBySpace(fullActionString);
			return this.performAction(action, who, tileLocation);
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x000ACD84 File Offset: 0x000AAF84
		public virtual bool ShouldIgnoreAction(string[] action, Farmer who, Location tileLocation)
		{
			string actionType = ArgUtility.Get(action, 0, null, true);
			if (string.IsNullOrWhiteSpace(actionType))
			{
				return true;
			}
			if (!(actionType == "DropBox"))
			{
				return actionType == "MonsterGrave" && !who.eventsSeen.Contains("6963327");
			}
			if (Game1.player.team.specialOrders != null)
			{
				string boxId = ArgUtility.Get(action, 1, null, true);
				if (boxId != null)
				{
					using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator = Game1.player.team.specialOrders.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.UsesDropBox(boxId))
							{
								return false;
							}
						}
					}
					return true;
				}
			}
			return true;
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x000ACE4C File Offset: 0x000AB04C
		public virtual void ShowLockedDoorMessage(string[] action)
		{
			Gender ownerGender = Gender.Female;
			string ownerName = null;
			string[] ownerDisplayNames = new string[(action.Length == 2) ? 1 : 2];
			for (int i = 0; i < ownerDisplayNames.Length; i++)
			{
				string ownerKey = action[i + 1];
				NPC owner = Game1.getCharacterFromName(ownerKey, true, false);
				if (owner != null)
				{
					ownerName = owner.Name;
					ownerGender = owner.Gender;
					ownerDisplayNames[i] = owner.displayName;
				}
				else
				{
					CharacterData data;
					if (!NPC.TryGetData(ownerKey, out data))
					{
						return;
					}
					ownerName = ownerKey;
					ownerGender = data.Gender;
					ownerDisplayNames[i] = TokenParser.ParseText(data.DisplayName, null, null, null);
				}
			}
			string lockedDoorMessage;
			if (ownerDisplayNames.Length > 1)
			{
				lockedDoorMessage = Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", ownerDisplayNames[0], ownerDisplayNames[1]);
			}
			else
			{
				string ownerDisplayName = ownerDisplayNames[0];
				string text;
				if ((text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:DoorUnlock_NotFriend_" + ownerName, true)) == null)
				{
					LocalizedContentManager content = Game1.content;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Strings\\Locations:DoorUnlock_NotFriend_");
					defaultInterpolatedStringHandler.AppendFormatted<Gender>(ownerGender);
					text = (content.LoadStringReturnNullIfNotFound(defaultInterpolatedStringHandler.ToStringAndClear(), ownerDisplayName, true) ?? Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Female", ownerDisplayName));
				}
				lockedDoorMessage = text;
			}
			Game1.drawObjectDialogue(lockedDoorMessage);
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x000ACF6C File Offset: 0x000AB16C
		public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			GameLocation.<>c__DisplayClass396_0 CS$<>8__locals1 = new GameLocation.<>c__DisplayClass396_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.action = action;
			CS$<>8__locals1.tileLocation = tileLocation;
			CS$<>8__locals1.who = who;
			if (this.ShouldIgnoreAction(CS$<>8__locals1.action, CS$<>8__locals1.who, CS$<>8__locals1.tileLocation))
			{
				return false;
			}
			string actionType;
			string error;
			if (!ArgUtility.TryGet(CS$<>8__locals1.action, 0, out actionType, out error, true, "string actionType"))
			{
				return CS$<>8__locals1.<performAction>g__LogError|0(error);
			}
			if (!CS$<>8__locals1.who.IsLocalPlayer)
			{
				if (actionType == "Door")
				{
					this.openDoor(CS$<>8__locals1.tileLocation, true);
				}
				return false;
			}
			Func<GameLocation, string[], Farmer, Point, bool> actionHandler;
			if (GameLocation.registeredTileActions.TryGetValue(actionType, out actionHandler))
			{
				return actionHandler(this, CS$<>8__locals1.action, CS$<>8__locals1.who, new Point(CS$<>8__locals1.tileLocation.X, CS$<>8__locals1.tileLocation.Y));
			}
			if (actionType != null)
			{
				switch (actionType.Length)
				{
				case 3:
				{
					if (!(actionType == "Buy"))
					{
						return false;
					}
					string which;
					if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out which, out error, true, "string which"))
					{
						return CS$<>8__locals1.<performAction>g__LogError|0(error);
					}
					return CS$<>8__locals1.who.TilePoint.Y >= CS$<>8__locals1.tileLocation.Y && this.HandleBuyAction(which);
				}
				case 4:
				{
					char c = actionType[0];
					if (c <= 'D')
					{
						if (c != 'C')
						{
							if (c != 'D')
							{
								return false;
							}
							if (!(actionType == "Door"))
							{
								return false;
							}
							if (CS$<>8__locals1.action.Length > 1 && !Game1.eventUp)
							{
								for (int i = 1; i < CS$<>8__locals1.action.Length; i++)
								{
									string name = CS$<>8__locals1.action[i];
									string mailKey = "doorUnlock" + name;
									if (CS$<>8__locals1.who.getFriendshipHeartLevelForNPC(name) >= 2 || Game1.player.mailReceived.Contains(mailKey))
									{
										Rumble.rumble(0.1f, 100f);
										Game1.player.mailReceived.Add(mailKey);
										this.openDoor(CS$<>8__locals1.tileLocation, true);
										return true;
									}
									if (name == "Sebastian" && this.IsGreenRainingHere() && Game1.year == 1)
									{
										Rumble.rumble(0.1f, 100f);
										this.openDoor(CS$<>8__locals1.tileLocation, true);
										return true;
									}
								}
								this.ShowLockedDoorMessage(CS$<>8__locals1.action);
								return true;
							}
							this.openDoor(CS$<>8__locals1.tileLocation, true);
							return true;
						}
						else
						{
							if (!(actionType == "Crib"))
							{
								return false;
							}
							foreach (NPC j in this.characters)
							{
								Child child = j as Child;
								if (child != null)
								{
									switch (child.Age)
									{
									case 0:
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", j.displayName)));
										return true;
									case 1:
										child.toss(CS$<>8__locals1.who);
										return true;
									case 2:
										if (child.isInCrib())
										{
											return j.checkAction(CS$<>8__locals1.who, this);
										}
										break;
									}
								}
							}
							return false;
						}
					}
					else
					{
						switch (c)
						{
						case 'L':
							if (!(actionType == "Lamp"))
							{
								return false;
							}
							if (this.lightLevel.Value == 0f)
							{
								this.lightLevel.Value = 0.6f;
							}
							else
							{
								this.lightLevel.Value = 0f;
							}
							this.playSound("openBox", null, null, SoundContext.Default);
							return true;
						case 'M':
							if (!(actionType == "Mine"))
							{
								return false;
							}
							goto IL_38B3;
						case 'N':
							if (!(actionType == "None"))
							{
								return false;
							}
							return true;
						default:
							if (c != 'W')
							{
								if (c != 'Y')
								{
									return false;
								}
								if (!(actionType == "Yoba"))
								{
									return false;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_Yoba"));
								return true;
							}
							else
							{
								if (!(actionType == "Warp"))
								{
									return false;
								}
								Point tile;
								string locationName;
								if (!ArgUtility.TryGetPoint(CS$<>8__locals1.action, 1, out tile, out error, "Point tile") || !ArgUtility.TryGet(CS$<>8__locals1.action, 3, out locationName, out error, true, "string locationName"))
								{
									return CS$<>8__locals1.<performAction>g__LogError|0(error);
								}
								bool flag = CS$<>8__locals1.action.Length < 5;
								CS$<>8__locals1.who.faceGeneralDirection(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y) * 64f, 0, false);
								Rumble.rumble(0.15f, 200f);
								if (flag)
								{
									this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
								}
								Game1.warpFarmer(locationName, tile.X, tile.Y, false);
								return true;
							}
							break;
						}
					}
					break;
				}
				case 5:
				{
					char c = actionType[0];
					if (c <= 'F')
					{
						if (c != 'C')
						{
							if (c != 'F')
							{
								return false;
							}
							if (!(actionType == "Forge"))
							{
								return false;
							}
							Game1.activeClickableMenu = new ForgeMenu();
							return true;
						}
						else
						{
							if (!(actionType == "Craft"))
							{
								return false;
							}
							GameLocation.openCraftingMenu();
							return true;
						}
					}
					else if (c != 'H')
					{
						if (c != 'N')
						{
							if (c != 'Q')
							{
								return false;
							}
							if (!(actionType == "QiCat"))
							{
								return false;
							}
							this.ShowQiCat();
							return true;
						}
						else
						{
							if (!(actionType == "Notes"))
							{
								return false;
							}
							int noteId;
							if (!ArgUtility.TryGetInt(CS$<>8__locals1.action, 1, out noteId, out error, "int noteId"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							this.readNote(noteId);
							return true;
						}
					}
					else
					{
						if (!(actionType == "HMTGF"))
						{
							return false;
						}
						if (CS$<>8__locals1.who.ActiveObject == null || !(CS$<>8__locals1.who.ActiveObject.QualifiedItemId == "(O)155"))
						{
							return true;
						}
						Object reward = ItemRegistry.Create<Object>("(BC)155", 1, 0, false);
						if (!Game1.player.couldInventoryAcceptThisItem(reward) && Game1.player.ActiveObject.stack.Value > 1)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
							return true;
						}
						Game1.player.reduceActiveItemByOne();
						Game1.player.makeThisTheActiveObject(reward);
						this.localSound("discoverMineral", null, null, SoundContext.Default);
						Game1.flashAlpha = 1f;
						return true;
					}
					break;
				}
				case 6:
				{
					char c = actionType[0];
					if (c != 'D')
					{
						if (c != 'L')
						{
							if (c != 'S')
							{
								return false;
							}
							if (!(actionType == "Saloon"))
							{
								return false;
							}
							return CS$<>8__locals1.who.TilePoint.Y > CS$<>8__locals1.tileLocation.Y && this.saloon(CS$<>8__locals1.tileLocation);
						}
						else
						{
							if (!(actionType == "Letter"))
							{
								return false;
							}
							string translationKey;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out translationKey, out error, true, "string translationKey"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							Game1.drawLetterMessage(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey.Replace("\"", "")));
							return true;
						}
					}
					else
					{
						if (!(actionType == "DyePot"))
						{
							return false;
						}
						if (!CS$<>8__locals1.who.eventsSeen.Contains("992559"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_DyePot"));
							return true;
						}
						if (!DyeMenu.IsWearingDyeable())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable"));
							return true;
						}
						Game1.activeClickableMenu = new DyeMenu();
						return true;
					}
					break;
				}
				case 7:
				{
					char c = actionType[0];
					if (c > 'D')
					{
						switch (c)
						{
						case 'G':
						{
							if (!(actionType == "Garbage"))
							{
								return false;
							}
							string id;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out id, out error, true, "string id"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							this.CheckGarbage(id, new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y), CS$<>8__locals1.who, true, true, delegate(string garbageError)
							{
								IGameLogger log = Game1.log;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(45, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("Ignored invalid 'Action Garbage ");
								defaultInterpolatedStringHandler2.AppendFormatted(id);
								defaultInterpolatedStringHandler2.AppendLiteral("' property: ");
								defaultInterpolatedStringHandler2.AppendFormatted(garbageError);
								defaultInterpolatedStringHandler2.AppendLiteral(".");
								log.Warn(defaultInterpolatedStringHandler2.ToStringAndClear());
							});
							Game1.haltAfterCheck = false;
							return true;
						}
						case 'H':
						case 'I':
						case 'L':
							return false;
						case 'J':
							if (!(actionType == "Jukebox"))
							{
								return false;
							}
							Game1.activeClickableMenu = new ChooseFromListMenu(Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation), new ChooseFromListMenu.actionOnChoosingListOption(ChooseFromListMenu.playSongAction), true, null);
							return true;
						case 'K':
							if (!(actionType == "Kitchen"))
							{
								return false;
							}
							break;
						case 'M':
							if (actionType == "Message")
							{
								goto IL_327C;
							}
							if (!(actionType == "Mailbox"))
							{
								return false;
							}
							if (this is Farm)
							{
								Building buildingAt = this.getBuildingAt(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y));
								FarmHouse farmhouse = ((buildingAt != null) ? buildingAt.GetIndoors() : null) as FarmHouse;
								if (farmhouse != null && !farmhouse.IsOwnedByCurrentPlayer)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
									return true;
								}
							}
							this.mailbox();
							return true;
						default:
							if (c != 'Q')
							{
								if (c != 'k')
								{
									return false;
								}
								if (!(actionType == "kitchen"))
								{
									return false;
								}
							}
							else
							{
								if (!(actionType == "QiCoins"))
								{
									return false;
								}
								if (CS$<>8__locals1.who.clubCoins > 0)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins", CS$<>8__locals1.who.clubCoins));
									return true;
								}
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins_BuyStarter"), this.createYesNoResponses(), "BuyClubCoins");
								return true;
							}
							break;
						}
						this.ActivateKitchen();
						return true;
					}
					if (c != 'B')
					{
						if (c != 'D')
						{
							return false;
						}
						if (!(actionType == "DropBox"))
						{
							return false;
						}
						string box_id;
						if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out box_id, out error, true, "string box_id"))
						{
							return CS$<>8__locals1.<performAction>g__LogError|0(error);
						}
						int minimum_capacity = 0;
						foreach (SpecialOrder order2 in Game1.player.team.specialOrders)
						{
							if (order2.UsesDropBox(box_id))
							{
								minimum_capacity = Math.Max(minimum_capacity, order2.GetMinimumDropBoxCapacity(box_id));
							}
						}
						using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator2 = Game1.player.team.specialOrders.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								SpecialOrder order = enumerator2.Current;
								if (order.UsesDropBox(box_id))
								{
									order.donateMutex.RequestLock(delegate
									{
										while (order.donatedItems.Count < minimum_capacity)
										{
											order.donatedItems.Add(null);
										}
										Game1.activeClickableMenu = new QuestContainerMenu(order.donatedItems, 3, new InventoryMenu.highlightThisItem(order.HighlightAcceptableItems), new Func<Item, int>(order.GetAcceptCount), new Action(order.UpdateDonationCounts), new Action(order.ConfirmCompleteDonations));
									}, null);
									return true;
								}
							}
						}
						return false;
					}
					else
					{
						if (!(actionType == "Bobbers"))
						{
							return false;
						}
						Game1.activeClickableMenu = new ChooseFromIconsMenu("bobbers");
						return true;
					}
					break;
				}
				case 8:
				{
					char c = actionType[2];
					if (c <= 'j')
					{
						if (c != 'a')
						{
							switch (c)
							{
							case 'e':
							{
								if (!(actionType == "OpenShop"))
								{
									return false;
								}
								string shopId;
								string direction;
								int openTime;
								int closeTime;
								int shopAreaX;
								int shopAreaY;
								int shopAreaWidth;
								int shopAreaHeight;
								if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out shopId, out error, true, "string shopId") || !ArgUtility.TryGetOptional(CS$<>8__locals1.action, 2, out direction, out error, null, true, "string direction") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 3, out openTime, out error, -1, "int openTime") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 4, out closeTime, out error, -1, "int closeTime") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 5, out shopAreaX, out error, -1, "int shopAreaX") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 6, out shopAreaY, out error, -1, "int shopAreaY") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 7, out shopAreaWidth, out error, -1, "int shopAreaWidth") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 8, out shopAreaHeight, out error, -1, "int shopAreaHeight"))
								{
									return CS$<>8__locals1.<performAction>g__LogError|0(error);
								}
								Microsoft.Xna.Framework.Rectangle? ownerSearchArea = null;
								if (shopAreaX != -1 || shopAreaY != -1 || shopAreaWidth != -1 || shopAreaHeight != -1)
								{
									if (shopAreaX == -1 || shopAreaY == -1 || shopAreaWidth == -1 || shopAreaHeight == -1)
									{
										return CS$<>8__locals1.<performAction>g__LogError|0("when specifying any of the shop area 'x y width height' arguments (indexes 5-8), all four must be specified");
									}
									ownerSearchArea = new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(shopAreaX, shopAreaY, shopAreaWidth, shopAreaHeight));
								}
								if (!(direction == "down"))
								{
									if (!(direction == "up"))
									{
										if (!(direction == "left"))
										{
											if (direction == "right")
											{
												if (CS$<>8__locals1.who.TilePoint.X < CS$<>8__locals1.tileLocation.X)
												{
													return false;
												}
											}
										}
										else if (CS$<>8__locals1.who.TilePoint.X > CS$<>8__locals1.tileLocation.X)
										{
											return false;
										}
									}
									else if (CS$<>8__locals1.who.TilePoint.Y > CS$<>8__locals1.tileLocation.Y)
									{
										return false;
									}
								}
								else if (CS$<>8__locals1.who.TilePoint.Y < CS$<>8__locals1.tileLocation.Y)
								{
									return false;
								}
								if ((openTime >= 0 && Game1.timeOfDay < openTime) || (closeTime >= 0 && Game1.timeOfDay >= closeTime))
								{
									return false;
								}
								string shopId2 = shopId;
								Microsoft.Xna.Framework.Rectangle? ownerArea = ownerSearchArea;
								bool forceOpen = ownerSearchArea == null;
								return Utility.TryOpenShopMenu(shopId2, this, ownerArea, null, forceOpen, true, null);
							}
							case 'f':
							case 'h':
								return false;
							case 'g':
								if (!(actionType == "MagicInk"))
								{
									return false;
								}
								if (CS$<>8__locals1.who.mailReceived.Add("hasPickedUpMagicInk"))
								{
									CS$<>8__locals1.who.hasMagicInk = true;
									this.setMapTile(4, 11, 113, "Buildings", "untitled tile sheet", null, true);
									CS$<>8__locals1.who.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(7, ""), null, false);
									return true;
								}
								return true;
							case 'i':
							{
								if (!(actionType == "ExitMine"))
								{
									return false;
								}
								Response[] responses = new Response[]
								{
									new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")),
									new Response("Go", Game1.content.LoadString("Strings\\Locations:Mines_GoUp")),
									new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
								};
								this.createQuestionDialogue(" ", responses, "ExitMine");
								return true;
							}
							case 'j':
								if (!(actionType == "JojaShop"))
								{
									return false;
								}
								Utility.TryOpenShopMenu("Joja", null, true);
								return true;
							default:
								return false;
							}
						}
						else
						{
							if (!(actionType == "Dialogue"))
							{
								return false;
							}
							string dialogue;
							if (!ArgUtility.TryGetRemainder(CS$<>8__locals1.action, 1, out dialogue, out error, ' ', "string dialogue"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							dialogue = TokenParser.ParseText(dialogue, null, null, null);
							Game1.drawDialogueNoTyping(dialogue);
							return true;
						}
					}
					else if (c != 'n')
					{
						if (c != 't')
						{
							if (c != 'u')
							{
								return false;
							}
							if (!(actionType == "ClubShop"))
							{
								return false;
							}
							Utility.TryOpenShopMenu("Casino", null, true);
							return true;
						}
						else
						{
							if (!(actionType == "Tutorial"))
							{
								return false;
							}
							Game1.activeClickableMenu = new TutorialMenu();
							return true;
						}
					}
					else
					{
						if (!(actionType == "MineSign"))
						{
							return false;
						}
						string dialogue2;
						if (!ArgUtility.TryGetRemainder(CS$<>8__locals1.action, 1, out dialogue2, out error, ' ', "string dialogue"))
						{
							return CS$<>8__locals1.<performAction>g__LogError|0(error);
						}
						Game1.drawObjectDialogue(Game1.parseText(dialogue2));
						return true;
					}
					break;
				}
				case 9:
				{
					char c = actionType[4];
					if (c <= 'E')
					{
						if (c != 'C')
						{
							if (c != 'E')
							{
								return false;
							}
							if (!(actionType == "PlayEvent"))
							{
								return false;
							}
							string eventId;
							bool checkPreconditions;
							bool checkSeen;
							string fallbackAction;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out eventId, out error, true, "string eventId") || !ArgUtility.TryGetOptionalBool(CS$<>8__locals1.action, 2, out checkPreconditions, out error, true, "bool checkPreconditions") || !ArgUtility.TryGetOptionalBool(CS$<>8__locals1.action, 3, out checkSeen, out error, true, "bool checkSeen") || !ArgUtility.TryGetOptionalRemainder(CS$<>8__locals1.action, 4, out fallbackAction, null, ' '))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							return Game1.PlayEvent(eventId, checkPreconditions, checkSeen) || (fallbackAction != null && this.performAction(fallbackAction, CS$<>8__locals1.who, CS$<>8__locals1.tileLocation));
						}
						else if (!(actionType == "ClubCards"))
						{
							return false;
						}
					}
					else if (c != 'S')
					{
						switch (c)
						{
						case 'a':
						{
							if (!(actionType == "LeoParrot"))
							{
								return false;
							}
							EmilysParrot emilysParrot = this.getTemporarySpriteByID(5858585) as EmilysParrot;
							if (emilysParrot == null)
							{
								return true;
							}
							emilysParrot.doAction();
							return true;
						}
						case 'b':
							if (!(actionType == "Billboard"))
							{
								return false;
							}
							Game1.activeClickableMenu = new Billboard(ArgUtility.Get(CS$<>8__locals1.action, 1, null, true) == "3");
							return true;
						case 'c':
						case 'd':
							return false;
						case 'e':
							if (!(actionType == "Carpenter"))
							{
								return false;
							}
							return CS$<>8__locals1.who.TilePoint.Y > CS$<>8__locals1.tileLocation.Y && this.carpenters(CS$<>8__locals1.tileLocation);
						default:
							switch (c)
							{
							case 'k':
								if (!(actionType == "BlackJack"))
								{
									return false;
								}
								break;
							case 'l':
								if (!(actionType == "SkullDoor"))
								{
									return false;
								}
								if (!CS$<>8__locals1.who.hasSkullKey && !Utility.IsPassiveFestivalDay("DesertFestival"))
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Locked"));
									return true;
								}
								if (!CS$<>8__locals1.who.hasUnlockedSkullDoor && !Utility.IsPassiveFestivalDay("DesertFestival"))
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Unlock")));
									DelayedAction.playSoundAfterDelay("openBox", 500, null, null, -1, false);
									DelayedAction.playSoundAfterDelay("openBox", 700, null, null, -1, false);
									Game1.addMailForTomorrow("skullCave", false, false);
									CS$<>8__locals1.who.hasUnlockedSkullDoor = true;
									CS$<>8__locals1.who.completeQuest("19");
									return true;
								}
								CS$<>8__locals1.who.completelyStopAnimatingOrDoingAction();
								this.playSound("doorClose", null, null, SoundContext.Default);
								DelayedAction.playSoundAfterDelay("stairsdown", 500, this, null, -1, false);
								Game1.enterMine(121, null);
								MineShaft.numberOfCraftedStairsUsedThisRun = 0;
								return true;
							case 'm':
								if (!(actionType == "QiGemShop"))
								{
									return false;
								}
								return Utility.TryOpenShopMenu("QiGemShop", null, true);
							case 'n':
							case 'q':
							case 'r':
							case 's':
								return false;
							case 'o':
								if (!(actionType == "Tailoring"))
								{
									return false;
								}
								if (CS$<>8__locals1.who.eventsSeen.Contains("992559"))
								{
									Game1.activeClickableMenu = new TailoringMenu();
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_SewingMachine"));
								return true;
							case 'p':
							{
								if (!(actionType == "Starpoint"))
								{
									return false;
								}
								string which2;
								if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out which2, out error, true, "string which"))
								{
									return CS$<>8__locals1.<performAction>g__LogError|0(error);
								}
								this.doStarpoint(which2);
								return true;
							}
							case 't':
							{
								if (!(actionType == "DogStatue"))
								{
									return false;
								}
								if (GameLocation.canRespec(0) || GameLocation.canRespec(3) || GameLocation.canRespec(2) || GameLocation.canRespec(4) || GameLocation.canRespec(1))
								{
									this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), this.createYesNoResponses(), "dogStatue");
									return true;
								}
								string displayed_text = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
								displayed_text = displayed_text.Substring(0, displayed_text.LastIndexOf('^'));
								Game1.drawObjectDialogue(displayed_text);
								return true;
							}
							default:
								return false;
							}
							break;
						}
					}
					else if (!(actionType == "playSound"))
					{
						if (!(actionType == "ClubSlots"))
						{
							return false;
						}
						Game1.currentMinigame = new Slots(-1, false);
						return true;
					}
					else
					{
						string audioName;
						if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out audioName, out error, true, "string audioName"))
						{
							return CS$<>8__locals1.<performAction>g__LogError|0(error);
						}
						this.localSound(audioName, null, null, SoundContext.Default);
						return true;
					}
					if (ArgUtility.Get(CS$<>8__locals1.action, 1, null, true) == "1000")
					{
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_HS"), new Response[]
						{
							new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave"))
						}, "CalicoJackHS");
						return true;
					}
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack"), new Response[]
					{
						new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave")),
						new Response("Rules", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules"))
					}, "CalicoJack");
					return true;
				}
				case 10:
				{
					char c = actionType[0];
					switch (c)
					{
					case 'A':
						if (!(actionType == "AnimalShop"))
						{
							return false;
						}
						return CS$<>8__locals1.who.TilePoint.Y > CS$<>8__locals1.tileLocation.Y && this.animalShop(CS$<>8__locals1.tileLocation);
					case 'B':
						if (!(actionType == "Bookseller"))
						{
							if (actionType == "BuyQiCoins")
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_Buy100Coins"), this.createYesNoResponses(), "BuyQiCoins");
								return true;
							}
							if (!(actionType == "Blacksmith"))
							{
								return false;
							}
							return CS$<>8__locals1.who.TilePoint.Y > CS$<>8__locals1.tileLocation.Y && this.blacksmith(CS$<>8__locals1.tileLocation);
						}
						else
						{
							if (!Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
							{
								return true;
							}
							if (Game1.player.mailReceived.Contains("read_a_book"))
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:books_welcome"), new Response[]
								{
									new Response("Buy", Game1.content.LoadString("Strings\\1_6_Strings:buy_books")),
									new Response("Trade", Game1.content.LoadString("Strings\\1_6_Strings:trade_books")),
									new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
								}, "Bookseller");
								return true;
							}
							Utility.TryOpenShopMenu("Bookseller", null, true);
							return true;
						}
						break;
					case 'C':
						if (!(actionType == "ClubSeller"))
						{
							return false;
						}
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller"), new Response[]
						{
							new Response("I'll", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_Yes")),
							new Response("No", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_No"))
						}, "ClubSeller");
						return true;
					case 'D':
						if (!(actionType == "DwarfGrave"))
						{
							return false;
						}
						if (CS$<>8__locals1.who.canUnderstandDwarves)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8214"));
						return true;
					case 'E':
						if (!(actionType == "EnterSewer"))
						{
							return false;
						}
						if (CS$<>8__locals1.who.mailReceived.Contains("OpenedSewer"))
						{
							this.playSound("stairsdown", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
							Game1.warpFarmer("Sewer", 16, 11, 2);
							return true;
						}
						if (CS$<>8__locals1.who.hasRustyKey)
						{
							this.playSound("openBox", null, null, SoundContext.Default);
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
							CS$<>8__locals1.who.mailReceived.Add("OpenedSewer");
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
						return true;
					case 'F':
						if (!(actionType == "FarmerFile"))
						{
							return false;
						}
						goto IL_3DA2;
					case 'G':
					case 'H':
					case 'I':
					case 'J':
					case 'K':
					case 'M':
						return false;
					case 'L':
						if (!(actionType == "LumberPile"))
						{
							return false;
						}
						if (!CS$<>8__locals1.who.hasOrWillReceiveMail("TH_LumberPile") && CS$<>8__locals1.who.hasOrWillReceiveMail("TH_SandDragon"))
						{
							Game1.player.hasClubCard = true;
							Game1.player.CanMove = false;
							Game1.player.mailReceived.Add("TH_LumberPile");
							Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(2, ""), null, false);
							Game1.player.removeQuest("5");
							return true;
						}
						return true;
					case 'N':
						if (!(actionType == "NPCMessage"))
						{
							return false;
						}
						goto IL_3410;
					default:
						switch (c)
						{
						case 'S':
						{
							if (!(actionType == "SandDragon"))
							{
								return false;
							}
							Object activeObject = CS$<>8__locals1.who.ActiveObject;
							if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)768" && !CS$<>8__locals1.who.hasOrWillReceiveMail("TH_SandDragon") && CS$<>8__locals1.who.hasOrWillReceiveMail("TH_MayorFridge"))
							{
								CS$<>8__locals1.who.reduceActiveItemByOne();
								Game1.player.CanMove = false;
								this.localSound("eat", null, null, SoundContext.Default);
								Game1.player.mailReceived.Add("TH_SandDragon");
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_ConsumeEssence"),
									Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote")
								});
								Game1.player.removeQuest("4");
								Game1.player.addQuest("5");
								return true;
							}
							if (CS$<>8__locals1.who.hasOrWillReceiveMail("TH_SandDragon"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote"));
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_Initial"));
							return true;
						}
						case 'T':
						{
							if (!(actionType == "TunnelSafe"))
							{
								return false;
							}
							Object activeObject2 = CS$<>8__locals1.who.ActiveObject;
							if (((activeObject2 != null) ? activeObject2.QualifiedItemId : null) == "(O)787" && !CS$<>8__locals1.who.hasOrWillReceiveMail("TH_Tunnel"))
							{
								CS$<>8__locals1.who.reduceActiveItemByOne();
								Game1.player.CanMove = false;
								this.playSound("openBox", null, null, SoundContext.Default);
								DelayedAction.playSoundAfterDelay("doorCreakReverse", 500, null, null, -1, false);
								Game1.player.mailReceived.Add("TH_Tunnel");
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_ConsumeBattery"),
									Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote")
								});
								Game1.player.addQuest("2");
								return true;
							}
							if (CS$<>8__locals1.who.hasOrWillReceiveMail("TH_Tunnel"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote"));
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_Initial"));
							return true;
						}
						case 'U':
						case 'V':
							return false;
						case 'W':
							if (!(actionType == "WizardBook"))
							{
								return false;
							}
							if (CS$<>8__locals1.who.mailReceived.Contains("hasPickedUpMagicInk") || CS$<>8__locals1.who.hasMagicInk)
							{
								this.ShowConstructOptions("Wizard", -1);
								return true;
							}
							return true;
						default:
							return false;
						}
						break;
					}
					break;
				}
				case 11:
				{
					char c = actionType[7];
					if (c <= 'a')
					{
						if (c <= 'O')
						{
							if (c != 'B')
							{
								if (c != 'O')
								{
									return false;
								}
								if (!(actionType == "MessageOnce"))
								{
									return false;
								}
								string eventFlag;
								string dialogue3;
								if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out eventFlag, out error, true, "string eventFlag") || !ArgUtility.TryGetRemainder(CS$<>8__locals1.action, 2, out dialogue3, out error, ' ', "string dialogue"))
								{
									return CS$<>8__locals1.<performAction>g__LogError|0(error);
								}
								if (CS$<>8__locals1.who.eventsSeen.Add(eventFlag))
								{
									Game1.drawObjectDialogue(Game1.parseText(dialogue3));
									return true;
								}
								return true;
							}
							else
							{
								if (!(actionType == "ElliottBook"))
								{
									return false;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Blank"));
								return true;
							}
						}
						else if (c != 'R')
						{
							if (c != 'W')
							{
								if (c != 'a')
								{
									return false;
								}
								if (!(actionType == "WizardHatch"))
								{
									return false;
								}
								Friendship friendship;
								if (CS$<>8__locals1.who.friendshipData.TryGetValue("Wizard", out friendship) && friendship.Points >= 1000)
								{
									this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
									Game1.warpFarmer("WizardHouseBasement", 4, 4, true);
									return true;
								}
								NPC wizard = this.characters[0];
								wizard.CurrentDialogue.Push(new Dialogue(wizard, "Data\\ExtraDialogue:Wizard_Hatch", false));
								Game1.drawDialogue(wizard);
								return true;
							}
							else
							{
								if (!(actionType == "ObeliskWarp"))
								{
									return false;
								}
								string targetLocation;
								Point targetTile;
								bool forceDismount;
								if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out targetLocation, out error, true, "string targetLocation") || !ArgUtility.TryGetPoint(CS$<>8__locals1.action, 2, out targetTile, out error, "Point targetTile") || !ArgUtility.TryGetOptionalBool(CS$<>8__locals1.action, 4, out forceDismount, out error, false, "bool forceDismount"))
								{
									return CS$<>8__locals1.<performAction>g__LogError|0(error);
								}
								Building.PerformObeliskWarp(targetLocation, targetTile.X, targetTile.Y, forceDismount, CS$<>8__locals1.who);
								return true;
							}
						}
						else
						{
							if (!(actionType == "MasteryRoom"))
							{
								return false;
							}
							int totalSkills = Game1.player.farmingLevel.Value / 10 + Game1.player.fishingLevel.Value / 10 + Game1.player.foragingLevel.Value / 10 + Game1.player.miningLevel.Value / 10 + Game1.player.combatLevel.Value / 10;
							if (totalSkills >= 5)
							{
								Game1.playSound("doorClose", null);
								Game1.warpFarmer("MasteryCave", 7, 11, 0);
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", totalSkills));
							return true;
						}
					}
					else if (c <= 'h')
					{
						if (c != 'd')
						{
							if (c != 'h')
							{
								return false;
							}
							if (!(actionType == "ColaMachine"))
							{
								return false;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_ColaMachine_Question"), this.createYesNoResponses(), "buyJojaCola");
							return true;
						}
						else
						{
							if (!(actionType == "RailroadBox"))
							{
								return false;
							}
							Object activeObject3 = CS$<>8__locals1.who.ActiveObject;
							if (((activeObject3 != null) ? activeObject3.QualifiedItemId : null) == "(O)394" && !CS$<>8__locals1.who.hasOrWillReceiveMail("TH_Railroad") && CS$<>8__locals1.who.hasOrWillReceiveMail("TH_Tunnel"))
							{
								CS$<>8__locals1.who.reduceActiveItemByOne();
								Game1.player.CanMove = false;
								this.localSound("Ship", null, null, SoundContext.Default);
								Game1.player.mailReceived.Add("TH_Railroad");
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Railroad_Box_ConsumeShell"),
									Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote")
								});
								Game1.player.removeQuest("2");
								Game1.player.addQuest("3");
								return true;
							}
							if (CS$<>8__locals1.who.hasOrWillReceiveMail("TH_Railroad"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote"));
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_Initial"));
							return true;
						}
					}
					else if (c != 'l')
					{
						if (c != 'p')
						{
							if (c != 'y')
							{
								return false;
							}
							if (!(actionType == "ForestPylon"))
							{
								return false;
							}
							Farmer who2 = CS$<>8__locals1.who;
							string a;
							if (who2 == null)
							{
								a = null;
							}
							else
							{
								Object activeObject4 = who2.ActiveObject;
								a = ((activeObject4 != null) ? activeObject4.QualifiedItemId : null);
							}
							if (a == "(O)FarAwayStone")
							{
								CS$<>8__locals1.who.reduceActiveItemByOne();
								Game1.playSound("openBox", null);
								Game1.player.mailReceived.Add("hasActivatedForestPylon");
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 106, 14, 22), new Vector2(16.6f, 2.5f) * 64f, false, 0f, Color.White)
								{
									animationLength = 8,
									interval = 100f,
									totalNumberOfLoops = 9999,
									scale = 4f
								});
								Game1.player.freezePause = 3000;
								DelayedAction.functionAfterDelay(delegate
								{
									Game1.afterFadeFunction afterFade;
									if ((afterFade = CS$<>8__locals1.<>9__8) == null)
									{
										afterFade = (CS$<>8__locals1.<>9__8 = delegate()
										{
											CS$<>8__locals1.<>4__this.startEvent(new Event(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylonEvent"), null));
										});
									}
									Game1.globalFadeToBlack(afterFade, 0.02f);
								}, 1000);
								return true;
							}
							if (CS$<>8__locals1.who.mailReceived.Contains("hasActivatedForestPylon"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylonActivated"));
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylon"));
							return true;
						}
						else
						{
							if (!(actionType == "BuyBackpack"))
							{
								return false;
							}
							Response purchase2000 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
							Response purchase2001 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
							Response notNow = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
							if (Game1.player.maxItems.Value == 12)
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"), new Response[]
								{
									purchase2000,
									notNow
								}, "Backpack");
								return true;
							}
							if (Game1.player.maxItems.Value < 36)
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"), new Response[]
								{
									purchase2001,
									notNow
								}, "Backpack");
								return true;
							}
							return true;
						}
					}
					else
					{
						if (!(actionType == "SpiritAltar"))
						{
							return false;
						}
						if (CS$<>8__locals1.who.ActiveObject != null && Game1.player.team.sharedDailyLuck.Value != -0.12 && Game1.player.team.sharedDailyLuck.Value != 0.12)
						{
							if (CS$<>8__locals1.who.ActiveObject.Price >= 60)
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite(352, 70f, 2, 2, new Vector2((float)(CS$<>8__locals1.tileLocation.X * 64), (float)(CS$<>8__locals1.tileLocation.Y * 64)), false, false));
								Game1.player.team.sharedDailyLuck.Value = 0.12;
								this.playSound("money", null, null, SoundContext.Default);
							}
							else
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite(362, 50f, 6, 1, new Vector2((float)(CS$<>8__locals1.tileLocation.X * 64), (float)(CS$<>8__locals1.tileLocation.Y * 64)), false, false));
								Game1.player.team.sharedDailyLuck.Value = -0.12;
								this.playSound("thunder", null, null, SoundContext.Default);
							}
							CS$<>8__locals1.who.ActiveObject = null;
							CS$<>8__locals1.who.showNotCarrying();
							return true;
						}
						return true;
					}
					break;
				}
				case 12:
				{
					char c = actionType[3];
					if (c <= 'l')
					{
						switch (c)
						{
						case 'a':
							if (!(actionType == "WizardShrine"))
							{
								return false;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WizardTower_WizardShrine").Replace('\n', '^'), this.createYesNoResponses(), "WizardShrine");
							return true;
						case 'b':
							if (!(actionType == "ClubComputer"))
							{
								return false;
							}
							goto IL_3DA2;
						case 'c':
						case 'f':
						case 'g':
						case 'h':
							return false;
						case 'd':
							if (!(actionType == "GoldenScythe"))
							{
								return false;
							}
							if (Game1.player.mailReceived.Contains("gotGoldenScythe"))
							{
								Game1.changeMusicTrack("silence", false, MusicContext.Default);
								this.performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
								return true;
							}
							if (!Game1.player.isInventoryFull())
							{
								Game1.playSound("parry", null);
								Game1.player.mailReceived.Add("gotGoldenScythe");
								this.setMapTile(29, 4, 245, "Front", "mine", null, true);
								this.setMapTile(30, 4, 246, "Front", "mine", null, true);
								this.setMapTile(29, 5, 261, "Front", "mine", null, true);
								this.setMapTile(30, 5, 262, "Front", "mine", null, true);
								this.setMapTile(29, 6, 277, "Buildings", "mine", null, true);
								this.setMapTile(30, 56, 278, "Buildings", "mine", null, true);
								Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(W)53", 1, 0, false), null, false);
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							return true;
						case 'e':
							if (!(actionType == "MineElevator"))
							{
								return false;
							}
							if (MineShaft.lowestLevelReached < 5)
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
								return true;
							}
							Game1.activeClickableMenu = new MineElevatorMenu();
							return true;
						case 'i':
							if (!(actionType == "ElliottPiano"))
							{
								return false;
							}
							goto IL_3593;
						default:
						{
							if (c != 'l')
							{
								return false;
							}
							if (!(actionType == "BuildingSilo"))
							{
								return false;
							}
							if (!CS$<>8__locals1.who.IsLocalPlayer)
							{
								return true;
							}
							Object activeObj = CS$<>8__locals1.who.ActiveObject;
							if (!(((activeObj != null) ? activeObj.QualifiedItemId : null) == "(O)178"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay", this.piecesOfHay, this.GetHayCapacity()));
								return true;
							}
							activeObj.FixStackSize();
							int stored = activeObj.Stack - this.tryToAddHay(activeObj.Stack);
							if (stored > 0)
							{
								if (activeObj.ConsumeStack(stored) == null)
								{
									CS$<>8__locals1.who.ActiveObject = null;
								}
								Game1.playSound("Ship", null);
								DelayedAction.playSoundAfterDelay("grassyStep", 100, null, null, -1, false);
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", stored));
								return true;
							}
							return true;
						}
						}
					}
					else if (c != 'p')
					{
						if (c != 's')
						{
							if (c != 'z')
							{
								return false;
							}
							if (!(actionType == "PrizeMachine"))
							{
								return false;
							}
							Game1.activeClickableMenu = new PrizeTicketMenu();
							return true;
						}
						else
						{
							if (!(actionType == "MonsterGrave"))
							{
								return false;
							}
							Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Backwoods_MonsterGrave").Split('#', StringSplitOptions.None));
							return true;
						}
					}
					else
					{
						if (!(actionType == "HospitalShop"))
						{
							return false;
						}
						Point playerTile = CS$<>8__locals1.who.TilePoint;
						Microsoft.Xna.Framework.Rectangle ownerSearchArea2 = new Microsoft.Xna.Framework.Rectangle(playerTile.X - 1, playerTile.Y - 2, 2, 1);
						Utility.TryOpenShopMenu("Hospital", this, new Microsoft.Xna.Framework.Rectangle?(ownerSearchArea2), null, false, true, null);
						return true;
					}
					break;
				}
				case 13:
				{
					char c = actionType[3];
					if (c <= 'e')
					{
						if (c != 'C')
						{
							if (c != 'c')
							{
								if (c != 'e')
								{
									return false;
								}
								if (!(actionType == "AdventureShop"))
								{
									return false;
								}
								this.adventureShop();
								return true;
							}
							else
							{
								if (!(actionType == "SpecialOrders"))
								{
									return false;
								}
								Game1.player.team.ordersBoardMutex.RequestLock(delegate
								{
									SpecialOrdersBoard specialOrdersBoard = new SpecialOrdersBoard("");
									specialOrdersBoard.behaviorBeforeCleanup = delegate(IClickableMenu menu)
									{
										Game1.player.team.ordersBoardMutex.ReleaseLock();
									};
									Game1.activeClickableMenu = specialOrdersBoard;
								}, null);
								return true;
							}
						}
						else
						{
							if (!(actionType == "IceCreamStand"))
							{
								return false;
							}
							Microsoft.Xna.Framework.Rectangle npcArea = new Microsoft.Xna.Framework.Rectangle(CS$<>8__locals1.tileLocation.X, CS$<>8__locals1.tileLocation.Y - 3, 1, 3);
							Utility.TryOpenShopMenu("IceCreamStand", this, new Microsoft.Xna.Framework.Rectangle?(npcArea), null, false, true, null);
							return true;
						}
					}
					else if (c <= 'm')
					{
						if (c != 'l')
						{
							if (c != 'm')
							{
								return false;
							}
							if (!(actionType == "SummitBoulder"))
							{
								return false;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SummitBoulder"));
							return true;
						}
						else
						{
							if (!(actionType == "BuildingChest"))
							{
								return false;
							}
							string buildingAction;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out buildingAction, out error, true, "string buildingAction"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							Building buildingAt2 = this.getBuildingAt(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y));
							if (buildingAt2 != null)
							{
								buildingAt2.PerformBuildingChestAction(buildingAction, CS$<>8__locals1.who);
							}
							return true;
						}
					}
					else if (c != 's')
					{
						if (c != 't')
						{
							return false;
						}
						if (!(actionType == "NextMineLevel"))
						{
							return false;
						}
						goto IL_38B3;
					}
					else if (!(actionType == "MessageSpeech"))
					{
						return false;
					}
					break;
				}
				case 14:
				{
					char c = actionType[11];
					if (c <= 'e')
					{
						if (c != 'a')
						{
							if (c != 'e')
							{
								return false;
							}
							if (!(actionType == "EvilShrineLeft"))
							{
								return false;
							}
							if (CS$<>8__locals1.who.getChildrenCount() == 0)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeftInactive"));
								return true;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeft"), this.createYesNoResponses(), "evilShrineLeft");
							return true;
						}
						else
						{
							if (!(actionType == "LockedDoorWarp"))
							{
								return false;
							}
							Point tile2;
							string locationName2;
							int openTime2;
							int closeTime2;
							string npcName;
							int minFriendship;
							if (!ArgUtility.TryGetPoint(CS$<>8__locals1.action, 1, out tile2, out error, "Point tile") || !ArgUtility.TryGet(CS$<>8__locals1.action, 3, out locationName2, out error, true, "string locationName") || !ArgUtility.TryGetInt(CS$<>8__locals1.action, 4, out openTime2, out error, "int openTime") || !ArgUtility.TryGetInt(CS$<>8__locals1.action, 5, out closeTime2, out error, "int closeTime") || !ArgUtility.TryGetOptional(CS$<>8__locals1.action, 6, out npcName, out error, null, true, "string npcName") || !ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 7, out minFriendship, out error, 0, "int minFriendship"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							CS$<>8__locals1.who.faceGeneralDirection(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y) * 64f, 0, false);
							this.lockedDoorWarp(tile2, locationName2, openTime2, closeTime2, npcName, minFriendship);
							return true;
						}
					}
					else if (c != 'k')
					{
						switch (c)
						{
						case 'o':
							if (!(actionType == "SquidFestBooth"))
							{
								return false;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFestBooth_Intro"), new Response[]
							{
								new Response("Rewards", Game1.content.LoadString("Strings\\1_6_Strings:GetRewards")),
								new Response("Explanation", Game1.content.LoadString("Strings\\1_6_Strings:Explanation")),
								new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
							}, "SquidFestBooth");
							return true;
						case 'p':
						case 'q':
						case 's':
							return false;
						case 'r':
							if (!(actionType == "Arcade_Prairie"))
							{
								return false;
							}
							this.showPrairieKingMenu();
							return true;
						case 't':
						{
							if (!(actionType == "Theater_Poster"))
							{
								return false;
							}
							if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
							{
								return true;
							}
							MovieData currentMovie = MovieTheater.GetMovieToday();
							if (currentMovie != null)
							{
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Theater_Poster_0", TokenParser.ParseText(currentMovie.Title, null, null, null)),
									Game1.content.LoadString("Strings\\Locations:Theater_Poster_1", TokenParser.ParseText(currentMovie.Description, null, null, null))
								});
								return true;
							}
							return true;
						}
						case 'u':
							if (!(actionType == "WarpGreenhouse"))
							{
								return false;
							}
							if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
							{
								CS$<>8__locals1.who.faceGeneralDirection(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y) * 64f, 0, false);
								this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
								GameLocation greenhouse = Game1.getLocationFromName("Greenhouse");
								int destination_x = 10;
								int destination_y = 23;
								if (greenhouse != null)
								{
									foreach (Warp warp in greenhouse.warps)
									{
										if (warp.TargetName == "Farm")
										{
											destination_x = warp.X;
											destination_y = warp.Y - 1;
											break;
										}
									}
								}
								Game1.warpFarmer("Greenhouse", destination_x, destination_y, false);
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GreenhouseRuins"));
							return true;
						default:
							return false;
						}
					}
					else
					{
						if (!(actionType == "WarpMensLocker"))
						{
							return false;
						}
						Point tile3;
						string locationName3;
						if (!ArgUtility.TryGetPoint(CS$<>8__locals1.action, 1, out tile3, out error, "Point tile") || !ArgUtility.TryGet(CS$<>8__locals1.action, 3, out locationName3, out error, true, "string locationName"))
						{
							return CS$<>8__locals1.<performAction>g__LogError|0(error);
						}
						bool playDoorSound = CS$<>8__locals1.action.Length < 5;
						if (!CS$<>8__locals1.who.IsMale)
						{
							if (CS$<>8__locals1.who.IsLocalPlayer)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
							}
							return true;
						}
						CS$<>8__locals1.who.faceGeneralDirection(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y) * 64f, 0, false);
						if (playDoorSound)
						{
							this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
						}
						Game1.warpFarmer(locationName3, tile3.X, tile3.Y, false);
						return true;
					}
					break;
				}
				case 15:
				{
					char c = actionType[4];
					if (c <= 'd')
					{
						if (c != 'S')
						{
							if (c != 'd')
							{
								return false;
							}
							if (!(actionType == "Arcade_Minecart"))
							{
								return false;
							}
							if (CS$<>8__locals1.who.hasSkullKey)
							{
								Response[] junimoKartOptions = new Response[]
								{
									new Response("Progress", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode")),
									new Response("Endless", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode")),
									new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
								};
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), junimoKartOptions, "MinecartGame");
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Inactive"));
							return true;
						}
						else
						{
							if (!(actionType == "EvilShrineRight"))
							{
								return false;
							}
							if (Game1.spawnMonstersAtNight)
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightDeActivate"), this.createYesNoResponses(), "evilShrineRightDeActivate");
								return true;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightActivate"), this.createYesNoResponses(), "evilShrineRightActivate");
							return true;
						}
					}
					else if (c != 'i')
					{
						if (c != 't')
						{
							if (c != 'y')
							{
								return false;
							}
							if (!(actionType == "EmilyRoomObject"))
							{
								return false;
							}
							if (!Game1.player.eventsSeen.Contains("463391") || !(Game1.player.spouse != "Emily"))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_EmilyRoomObject"));
								return true;
							}
							EmilysParrot emilysParrot2 = this.getTemporarySpriteByID(5858585) as EmilysParrot;
							if (emilysParrot2 == null)
							{
								return true;
							}
							emilysParrot2.doAction();
							return true;
						}
						else
						{
							if (!(actionType == "TroutDerbyBooth"))
							{
								return false;
							}
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_Intro"), new Response[]
							{
								new Response("Rewards", Game1.content.LoadString("Strings\\1_6_Strings:GetRewards")),
								new Response("Explanation", Game1.content.LoadString("Strings\\1_6_Strings:Explanation")),
								new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
							}, "TroutDerbyBooth");
							return true;
						}
					}
					else
					{
						if (!(actionType == "ConditionalDoor"))
						{
							return false;
						}
						if (CS$<>8__locals1.action.Length <= 1 || Game1.eventUp)
						{
							return true;
						}
						if (GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(CS$<>8__locals1.action, ' ', 1, 2147483647), null, null, null, null, null, null))
						{
							this.openDoor(CS$<>8__locals1.tileLocation, true);
							return true;
						}
						string message = this.doesTileHaveProperty(CS$<>8__locals1.tileLocation.X, CS$<>8__locals1.tileLocation.Y, "LockedDoorMessage", "Buildings", false);
						if (message != null)
						{
							Game1.drawObjectDialogue(TokenParser.ParseText(Game1.content.LoadString(message), null, null, null));
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
						return true;
					}
					break;
				}
				case 16:
				{
					char c = actionType[0];
					if (c <= 'F')
					{
						if (c != 'E')
						{
							if (c != 'F')
							{
								return false;
							}
							if (!(actionType == "FishingDerbySign"))
							{
								return false;
							}
							Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString(Game1.IsSummer ? "Strings\\1_6_Strings:FishingDerbySign" : "Strings\\1_6_Strings:SquidFestSign"));
							return true;
						}
						else
						{
							if (!(actionType == "EvilShrineCenter"))
							{
								return false;
							}
							if (CS$<>8__locals1.who.isDivorced())
							{
								this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenter"), this.createYesNoResponses(), "evilShrineCenter");
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenterInactive"));
							return true;
						}
					}
					else if (c != 'Q')
					{
						if (c != 'T')
						{
							if (c != 'W')
							{
								return false;
							}
							if (!(actionType == "WarpWomensLocker"))
							{
								return false;
							}
							Point tile4;
							string locationName4;
							if (!ArgUtility.TryGetPoint(CS$<>8__locals1.action, 1, out tile4, out error, "Point tile") || !ArgUtility.TryGet(CS$<>8__locals1.action, 3, out locationName4, out error, true, "string locationName"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							bool playDoorSound2 = CS$<>8__locals1.action.Length < 5;
							if (CS$<>8__locals1.who.IsMale)
							{
								if (CS$<>8__locals1.who.IsLocalPlayer)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
								}
								return true;
							}
							CS$<>8__locals1.who.faceGeneralDirection(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y) * 64f, 0, false);
							if (playDoorSound2)
							{
								this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
							}
							Game1.warpFarmer(locationName4, tile4.X, tile4.Y, false);
							return true;
						}
						else
						{
							if (!(actionType == "Theater_Entrance"))
							{
								return false;
							}
							if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
							{
								return true;
							}
							if (Game1.player.team.movieMutex.IsLocked())
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_CurrentlyShowing")));
								return true;
							}
							if (Game1.isFestival())
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
								return true;
							}
							if (Game1.timeOfDay > 2100 || Game1.timeOfDay < 900)
							{
								string openTime3 = Game1.getTimeOfDayString(900).Replace(" ", "");
								string closeTime3 = Game1.getTimeOfDayString(2100).Replace(" ", "");
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTime3, closeTime3));
								return true;
							}
							if (Game1.player.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AlreadySeen"));
								return true;
							}
							NPC invited_npc = null;
							foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
							{
								if (invitation.farmer == Game1.player && !invitation.fulfilled && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
								{
									invited_npc = invitation.invitedNPC;
									break;
								}
							}
							if (Game1.player.Items.ContainsId("(O)809"))
							{
								string question = (invited_npc != null) ? Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchWithFriendPrompt", invited_npc.displayName) : Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchAlonePrompt");
								Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
								return true;
							}
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_NoTicket")));
							return true;
						}
					}
					else
					{
						if (!(actionType == "QiChallengeBoard"))
						{
							return false;
						}
						Game1.player.team.qiChallengeBoardMutex.RequestLock(delegate
						{
							SpecialOrdersBoard specialOrdersBoard = new SpecialOrdersBoard("Qi");
							specialOrdersBoard.behaviorBeforeCleanup = delegate(IClickableMenu menu)
							{
								Game1.player.team.qiChallengeBoardMutex.ReleaseLock();
							};
							Game1.activeClickableMenu = specialOrdersBoard;
						}, null);
						return true;
					}
					break;
				}
				case 17:
				{
					char c = actionType[0];
					if (c <= 'M')
					{
						if (c != 'B')
						{
							if (c != 'M')
							{
								return false;
							}
							if (!(actionType == "MinecartTransport"))
							{
								return false;
							}
							string networkId = ArgUtility.Get(CS$<>8__locals1.action, 1, null, true) ?? "Default";
							string excludeDestinationId = ArgUtility.Get(CS$<>8__locals1.action, 2, null, true);
							this.ShowMineCartMenu(networkId, excludeDestinationId);
							return true;
						}
						else
						{
							if (!(actionType == "BuildingGoldClock"))
							{
								return false;
							}
							bool clockOn = !Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
							CS$<>8__locals1.who.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:GoldClock_" + (clockOn ? "Off" : "On")), CS$<>8__locals1.who.currentLocation.createYesNoResponses(), "GoldClock");
							return true;
						}
					}
					else if (c != 'T')
					{
						if (c != 'W')
						{
							return false;
						}
						if (!(actionType == "Warp_Sunroom_Door"))
						{
							return false;
						}
						if (CS$<>8__locals1.who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
						{
							this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
							Game1.warpFarmer("Sunroom", 5, 13, false);
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Caroline_Sunroom_Door"));
						return true;
					}
					else
					{
						if (!(actionType == "Theater_BoxOffice"))
						{
							return false;
						}
						if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
						{
							return true;
						}
						if (Game1.isFestival())
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
							return true;
						}
						if (Game1.timeOfDay > 2100)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_BoxOfficeClosed"));
							return true;
						}
						if (MovieTheater.GetMovieToday() != null)
						{
							Utility.TryOpenShopMenu("BoxOffice", null, true);
							return true;
						}
						return true;
					}
					break;
				}
				case 18:
				{
					char c = actionType[12];
					if (c != 'C')
					{
						if (c != 'M')
						{
							if (c != 'r')
							{
								return false;
							}
							if (!(actionType == "GrandpaMasteryNote"))
							{
								return false;
							}
							Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\1_6_Strings:GrandpaMasteryNote", Game1.player.Name, Game1.player.farmName));
							return true;
						}
						else
						{
							if (!(actionType == "MasteryCave_Mining"))
							{
								return false;
							}
							if (Game1.player.stats.Get(StatKeys.Mastery(3)) >= 0U)
							{
								Game1.activeClickableMenu = new MasteryTrackerMenu(3);
								return true;
							}
							return true;
						}
					}
					else
					{
						if (!(actionType == "MasteryCave_Combat"))
						{
							return false;
						}
						if (Game1.player.stats.Get(StatKeys.Mastery(4)) >= 0U)
						{
							Game1.activeClickableMenu = new MasteryTrackerMenu(4);
							return true;
						}
						return true;
					}
					break;
				}
				case 19:
				{
					char c = actionType[13];
					if (c != 'C')
					{
						if (c != 'a')
						{
							if (c != 'i')
							{
								return false;
							}
							if (!(actionType == "MasteryCave_Fishing"))
							{
								return false;
							}
							if (Game1.player.stats.Get(StatKeys.Mastery(1)) >= 0U)
							{
								Game1.activeClickableMenu = new MasteryTrackerMenu(1);
								return true;
							}
							return true;
						}
						else
						{
							if (!(actionType == "MasteryCave_Farming"))
							{
								return false;
							}
							if (Game1.player.stats.Get(StatKeys.Mastery(0)) >= 0U)
							{
								Game1.activeClickableMenu = new MasteryTrackerMenu(0);
								return true;
							}
							return true;
						}
					}
					else
					{
						if (!(actionType == "WarpCommunityCenter"))
						{
							return false;
						}
						if (Game1.MasterPlayer.mailReceived.Contains("ccDoorUnlock") || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
						{
							this.playSound("doorClose", new Vector2?(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y)), null, SoundContext.Default);
							Game1.warpFarmer("CommunityCenter", 32, 23, false);
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8175"));
						return true;
					}
					break;
				}
				case 20:
				{
					char c = actionType[12];
					if (c != 'F')
					{
						if (c != 'P')
						{
							return false;
						}
						if (!(actionType == "MasteryCave_Pedestal"))
						{
							return false;
						}
						Game1.activeClickableMenu = new MasteryTrackerMenu(-1);
						return true;
					}
					else
					{
						if (!(actionType == "MasteryCave_Foraging"))
						{
							return false;
						}
						if (Game1.player.stats.Get(StatKeys.Mastery(2)) >= 0U)
						{
							Game1.activeClickableMenu = new MasteryTrackerMenu(2);
							return true;
						}
						return true;
					}
					break;
				}
				case 21:
					if (!(actionType == "SpecialWaterDroppable"))
					{
						return false;
					}
					if (!(this is MineShaft) || (this as MineShaft).mineLevel == 100)
					{
						Farmer who3 = CS$<>8__locals1.who;
						string a2;
						if (who3 == null)
						{
							a2 = null;
						}
						else
						{
							Object activeObject5 = who3.ActiveObject;
							a2 = ((activeObject5 != null) ? activeObject5.QualifiedItemId : null);
						}
						if (a2 == "(O)103")
						{
							this.localSound("throwDownITem", null, null, SoundContext.Default);
							CS$<>8__locals1.who.reduceActiveItemByOne();
							TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(103, 9999f, 1, 1, CS$<>8__locals1.who.position.Value + new Vector2(0f, -128f), false, false, false)
							{
								motion = new Vector2(4f, -4f),
								acceleration = new Vector2(0f, 0.3f),
								yStopCoordinate = (int)CS$<>8__locals1.who.position.Y,
								id = 777
							};
							CS$<>8__locals1.who.freezePause = 4000;
							Action <>9__5;
							tempSprite.reachedStopCoordinate = delegate(int x)
							{
								CS$<>8__locals1.<>4__this.removeTemporarySpritesWithID(777);
								CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, tempSprite.position, false, false)
								{
									color = Color.OrangeRed
								});
								CS$<>8__locals1.<>4__this.localSound("dropItemInWater", null, null, SoundContext.Default);
								Action func;
								if ((func = <>9__5) == null)
								{
									func = (<>9__5 = delegate()
									{
										CS$<>8__locals1.<>4__this.localSound("terraria_boneSerpent", null, null, SoundContext.Default);
										CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(128, 96, 32, 32), 70f, 4, 5, tempSprite.position + new Vector2(-5f, -3f) * 4f, false, true, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
										CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(128, 96, 32, 32), 60f, 4, 5, tempSprite.position + new Vector2(-5f, 7f) * 4f, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
										CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(134, 2, 21, 38), 9999f, 1, 1, tempSprite.position, false, false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											xPeriodic = true,
											xPeriodicLoopTime = 500f,
											xPeriodicRange = 2f,
											motion = new Vector2(0f, -8f)
										});
										for (int k = 0; k < 13; k++)
										{
											CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(134, (k == 12) ? 54 : 41, 21, 12), 9999f, 1, 1, tempSprite.position, false, false, 0.97f - (float)k * 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												xPeriodic = true,
												xPeriodicLoopTime = (float)(500 + Game1.random.Next(-50, 50)),
												xPeriodicRange = 2f,
												motion = new Vector2(0f, -8f),
												delayBeforeAnimationStart = 220 + 80 * k
											});
										}
										TemporaryAnimatedSprite stoneSprite = new TemporaryAnimatedSprite(935, 9999f, 1, 1, tempSprite.position + new Vector2(0f, -128f), false, false, false)
										{
											motion = new Vector2(-4f, -4f),
											acceleration = new Vector2(0f, 0.3f),
											yStopCoordinate = (int)(CS$<>8__locals1.who.position.Y - 128f + 12f),
											id = 888
										};
										TemporaryAnimatedSprite temporaryAnimatedSprite = stoneSprite;
										TemporaryAnimatedSprite.endBehavior reachedStopCoordinate;
										if ((reachedStopCoordinate = CS$<>8__locals1.<>9__6) == null)
										{
											reachedStopCoordinate = (CS$<>8__locals1.<>9__6 = delegate(int y)
											{
												CS$<>8__locals1.who.addItemByMenuIfNecessary(new Object("FarAwayStone", 1, false, -1, 0), null, false);
												CS$<>8__locals1.who.currentLocation.removeTemporarySpritesWithID(888);
												CS$<>8__locals1.<>4__this.localSound("coin", null, null, SoundContext.Default);
											});
										}
										temporaryAnimatedSprite.reachedStopCoordinate = reachedStopCoordinate;
										CS$<>8__locals1.who.currentLocation.temporarySprites.Add(stoneSprite);
									});
								}
								DelayedAction.functionAfterDelay(func, 1000);
							};
							this.temporarySprites.Add(tempSprite);
							return true;
						}
						Farmer who4 = CS$<>8__locals1.who;
						if (((who4 != null) ? who4.ActiveObject : null) != null && !CS$<>8__locals1.who.ActiveObject.questItem.Value && CS$<>8__locals1.who.ActiveObject.QualifiedItemId != "(O)FarAwayStone" && CS$<>8__locals1.who.ActiveObject.Edibility <= 0 && !CS$<>8__locals1.who.ActiveObject.Name.Contains("Totem"))
						{
							Farmer who5 = CS$<>8__locals1.who;
							ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem((who5 != null) ? who5.ActiveObject.QualifiedItemId : null);
							if (itemData != null)
							{
								this.localSound("throwDownITem", null, null, SoundContext.Default);
								int _id = Game1.random.Next();
								TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(itemData.GetTextureName(), itemData.GetSourceRect(0, null), 9999f, 1, 1, CS$<>8__locals1.who.position.Value + new Vector2(0f, -128f), false, false)
								{
									motion = new Vector2(4f, -4f),
									acceleration = new Vector2(0f, 0.3f),
									yStopCoordinate = (int)CS$<>8__locals1.who.position.Y,
									id = _id,
									scale = 4f * ((itemData.GetSourceRect(0, null).Height > 32) ? 0.5f : 1f)
								};
								CS$<>8__locals1.who.reduceActiveItemByOne();
								tempSprite.reachedStopCoordinate = delegate(int x)
								{
									CS$<>8__locals1.<>4__this.removeTemporarySpritesWithID(_id);
									CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, tempSprite.position, false, false)
									{
										color = Color.OrangeRed
									});
									CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), tempSprite.position + new Vector2(2f, 0f) * 4f, false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 4,
										scale = 4f,
										layerDepth = 0.99f,
										alphaFade = 0.02f
									});
									for (int k = 0; k < 4; k++)
									{
										CS$<>8__locals1.<>4__this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), tempSprite.position + new Vector2(2f, 0f) * 4f, false, 0f, Color.White)
										{
											motion = new Vector2((float)Game1.random.Next(-15, 26) / 10f, -4f),
											acceleration = new Vector2(0f, (float)Game1.random.Next(3, 7) / 30f),
											interval = 50f,
											totalNumberOfLoops = 99999,
											animationLength = 7,
											scale = 4f,
											layerDepth = 0.99f,
											alphaFade = 0.02f,
											delayBeforeAnimationStart = k * 30
										});
									}
									CS$<>8__locals1.<>4__this.localSound("dropItemInWater", null, null, SoundContext.Default);
									CS$<>8__locals1.<>4__this.localSound("fireball", null, null, SoundContext.Default);
								};
								this.temporarySprites.Add(tempSprite);
							}
							return true;
						}
					}
					return false;
				case 22:
				case 23:
					return false;
				case 24:
				{
					char c = actionType[0];
					if (c != 'B')
					{
						if (c != 'N')
						{
							if (c != 'T')
							{
								return false;
							}
							if (!(actionType == "Theater_PosterComingSoon"))
							{
								return false;
							}
							if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
							{
								return true;
							}
							MovieData upcomingMovie = MovieTheater.GetUpcomingMovie();
							if (upcomingMovie != null)
							{
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Theater_Poster_Coming_Soon", TokenParser.ParseText(upcomingMovie.Title, null, null, null))
								});
								return true;
							}
							return true;
						}
						else
						{
							if (!(actionType == "NPCSpeechMessageNoRadius"))
							{
								return false;
							}
							string npcName2;
							string translationKey2;
							if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out npcName2, out error, true, "string npcName") || !ArgUtility.TryGet(CS$<>8__locals1.action, 2, out translationKey2, out error, true, "string translationKey"))
							{
								return CS$<>8__locals1.<performAction>g__LogError|0(error);
							}
							NPC npc = Game1.getCharacterFromName(npcName2, true, false);
							if (npc == null)
							{
								try
								{
									npc = new NPC(null, Vector2.Zero, "", 0, npcName2, false, Game1.temporaryContent.Load<Texture2D>("Portraits\\" + npcName2));
								}
								catch (Exception)
								{
									return CS$<>8__locals1.<performAction>g__LogError|0("couldn't find or create a matching NPC");
								}
							}
							try
							{
								npc.setNewDialogue("Strings\\StringsFromMaps:" + translationKey2, true, false);
								Game1.drawDialogue(npc);
								return true;
							}
							catch (Exception e)
							{
								GameLocation.<>c__DisplayClass396_0 CS$<>8__locals7 = CS$<>8__locals1;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
								defaultInterpolatedStringHandler.AppendLiteral("unhandled exception drawing dialogue: ");
								defaultInterpolatedStringHandler.AppendFormatted<Exception>(e);
								return CS$<>8__locals7.<performAction>g__LogError|0(defaultInterpolatedStringHandler.ToStringAndClear());
							}
							goto IL_3410;
						}
					}
					else
					{
						if (!(actionType == "BuildingToggleAnimalDoor"))
						{
							return false;
						}
						Building building = this.getBuildingAt(new Vector2((float)CS$<>8__locals1.tileLocation.X, (float)CS$<>8__locals1.tileLocation.Y));
						if (building != null)
						{
							if (Game1.didPlayerJustRightClick(true))
							{
								building.ToggleAnimalDoor(CS$<>8__locals1.who);
							}
							return true;
						}
						return true;
					}
					break;
				}
				case 25:
					if (!(actionType == "SpecialOrdersPrizeTickets"))
					{
						return false;
					}
					if (Game1.player.stats.Get("specialOrderPrizeTickets") <= 0U)
					{
						return true;
					}
					if (Game1.player.couldInventoryAcceptThisItem(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false)))
					{
						Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)PrizeTicket", 1, 0, false), false);
						Game1.player.stats.Decrement("specialOrderPrizeTickets", 1U);
						Game1.playSound("coin", null);
						return true;
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					return true;
				default:
					return false;
				}
				IL_327C:
				string translationKey3;
				if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out translationKey3, out error, true, "string translationKey"))
				{
					return CS$<>8__locals1.<performAction>g__LogError|0(error);
				}
				string s = null;
				try
				{
					s = Game1.content.LoadStringReturnNullIfNotFound(translationKey3, true);
				}
				catch (Exception)
				{
					s = null;
				}
				if (s != null)
				{
					Game1.drawDialogueNoTyping(s);
					return true;
				}
				Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey3.Replace("\"", "")));
				return true;
				IL_3410:
				string npcName3;
				string rawMessage;
				if (!ArgUtility.TryGet(CS$<>8__locals1.action, 1, out npcName3, out error, true, "string npcName") || !ArgUtility.TryGetRemainder(CS$<>8__locals1.action, 2, out rawMessage, out error, ' ', "string rawMessage"))
				{
					return CS$<>8__locals1.<performAction>g__LogError|0(error);
				}
				string message2 = rawMessage.Replace("\"", "");
				NPC npc2 = Game1.getCharacterFromName(npcName3, true, false);
				if (npc2 != null && npc2.currentLocation == CS$<>8__locals1.who.currentLocation && Utility.tileWithinRadiusOfPlayer(npc2.TilePoint.X, npc2.TilePoint.Y, 14, CS$<>8__locals1.who))
				{
					try
					{
						string str_name = message2.Split('/', StringSplitOptions.None)[0];
						string str_name_no_filePath = str_name.Substring(str_name.IndexOf(':') + 1);
						npc2.setNewDialogue(str_name, true, false);
						Game1.drawDialogue(npc2);
						if ((str_name_no_filePath == "AnimalShop.20" || str_name_no_filePath == "JoshHouse_Alex_Trash" || str_name_no_filePath == "SamHouse_Sam_Trash" || str_name_no_filePath == "SeedShop_Abigail_Drawers") && CS$<>8__locals1.who != null)
						{
							Game1.multiplayer.globalChatInfoMessage("Caught_Snooping", new string[]
							{
								CS$<>8__locals1.who.name.Value,
								npc2.GetTokenizedDisplayName()
							});
						}
						return true;
					}
					catch (Exception)
					{
						return false;
					}
				}
				try
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString(message2.Split('/', StringSplitOptions.None)[1]));
					return false;
				}
				catch (Exception)
				{
					return false;
				}
				IL_3593:
				int key;
				if (!ArgUtility.TryGetInt(CS$<>8__locals1.action, 1, out key, out error, "int key"))
				{
					return CS$<>8__locals1.<performAction>g__LogError|0(error);
				}
				this.playElliottPiano(key);
				return true;
				IL_38B3:
				int mineLevel;
				if (!ArgUtility.TryGetOptionalInt(CS$<>8__locals1.action, 1, out mineLevel, out error, 1, "int mineLevel"))
				{
					return CS$<>8__locals1.<performAction>g__LogError|0(error);
				}
				this.playSound("stairsdown", null, null, SoundContext.Default);
				Game1.enterMine(mineLevel, null);
				return true;
				IL_3DA2:
				this.farmerFile();
				return true;
			}
			return false;
		}

		// Token: 0x06000F55 RID: 3925 RVA: 0x000B1704 File Offset: 0x000AF904
		public void showPrairieKingMenu()
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.currentMinigame = new AbigailGame(null);
				return;
			}
			Response[] junimoKartOptions = new Response[]
			{
				new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")),
				new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")),
				new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
			};
			this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), junimoKartOptions, "CowboyGame");
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x000B17A4 File Offset: 0x000AF9A4
		public void ShowMineCartMenu(string networkId, string excludeDestinationId)
		{
			if (Game1.player.mount != null)
			{
				return;
			}
			Dictionary<string, MinecartNetworkData> networks = DataLoader.Minecarts(Game1.content);
			MinecartNetworkData network;
			if (networkId == null || !networks.TryGetValue(networkId, out network))
			{
				Game1.log.Warn("Can't show minecart menu for unknown network ID '" + networkId + "'.");
				return;
			}
			if (!GameStateQuery.CheckConditions(network.UnlockCondition, this, null, null, null, null, null))
			{
				Game1.drawObjectDialogue(TokenParser.ParseText(network.LockedMessage, null, null, null) ?? Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
				return;
			}
			MinecartNetworkData network2 = network;
			bool flag;
			if (network2 == null)
			{
				flag = false;
			}
			else
			{
				List<MinecartDestinationData> destinations2 = network2.Destinations;
				int? num = (destinations2 != null) ? new int?(destinations2.Count) : null;
				int num2 = 0;
				flag = (num.GetValueOrDefault() > num2 & num != null);
			}
			if (!flag)
			{
				Game1.log.Warn("Can't show minecart menu for network ID '" + networkId + "' with missing destination data.");
				return;
			}
			List<KeyValuePair<string, string>> destinations = new List<KeyValuePair<string, string>>();
			Dictionary<string, MinecartDestinationData> destinationLookup = new Dictionary<string, MinecartDestinationData>();
			MinecartDestinationData destination;
			using (List<MinecartDestinationData>.Enumerator enumerator = network.Destinations.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					destination = enumerator.Current;
					if (string.IsNullOrWhiteSpace((destination != null) ? destination.Id : null) || string.IsNullOrWhiteSpace((destination != null) ? destination.TargetLocation : null))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(97, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Ignored invalid minecart destination '");
						defaultInterpolatedStringHandler.AppendFormatted((destination != null) ? destination.Id : null);
						defaultInterpolatedStringHandler.AppendLiteral("' in network '");
						defaultInterpolatedStringHandler.AppendFormatted(networkId);
						defaultInterpolatedStringHandler.AppendLiteral("' because its ID or location isn't specified.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else if (!destination.Id.EqualsIgnoreCase(excludeDestinationId) && GameStateQuery.CheckConditions(destination.Condition, this, null, null, null, null, null))
					{
						if (destinationLookup.TryAdd(destination.Id, destination))
						{
							string label = TokenParser.ParseText(destination.DisplayName, null, null, null) ?? destination.TargetLocation;
							if (destination.Price > 0)
							{
								label = Game1.content.LoadString("Strings\\Locations:MineCart_DestinationWithPrice", label, destination.Price);
							}
							destinations.Add(new KeyValuePair<string, string>(destination.Id, label));
						}
						else
						{
							IGameLogger log2 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Ignored minecart destination with duplicate ID '");
							defaultInterpolatedStringHandler.AppendFormatted(destination.Id);
							defaultInterpolatedStringHandler.AppendLiteral("' in network '");
							defaultInterpolatedStringHandler.AppendFormatted(networkId);
							defaultInterpolatedStringHandler.AppendLiteral("'.");
							log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
			}
			this.ShowPagedResponses(TokenParser.ParseText(network.ChooseDestinationMessage, null, null, null) ?? Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), destinations, delegate(string destinationId)
			{
				MinecartDestinationData destination;
				if (!destinationLookup.TryGetValue(destinationId, out destination))
				{
					return;
				}
				int price = destination.Price;
				if (price < 1)
				{
					this.MinecartWarp(destination);
					return;
				}
				string displayPrice = Utility.getNumberWithCommas(price);
				string buyTicketMessage = ((destination.BuyTicketMessage ?? network.BuyTicketMessage) != null) ? string.Format(TokenParser.ParseText(network.BuyTicketMessage, null, null, null), displayPrice) : Game1.content.LoadString("Strings\\Locations:BuyTicket", displayPrice);
				this.createQuestionDialogue(buyTicketMessage, this.createYesNoResponses(), delegate(Farmer who, string whichAnswer)
				{
					if (whichAnswer == "Yes")
					{
						if (who.Money >= price)
						{
							who.Money -= price;
							this.MinecartWarp(destination);
							return;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
				}, null);
			}, false, true, 5);
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x000B1AC8 File Offset: 0x000AFCC8
		public void MinecartWarp(MinecartDestinationData destination)
		{
			GameLocation targetLocation = Game1.RequireLocation(destination.TargetLocation, false);
			Point targetTile = destination.TargetTile;
			int direction;
			if (!Utility.TryParseDirection(destination.TargetDirection, out direction))
			{
				direction = 2;
			}
			Game1.player.Halt();
			Game1.player.freezePause = 700;
			Game1.warpFarmer(targetLocation.NameOrUniqueName, targetTile.X, targetTile.Y, direction);
			if (Game1.IsPlayingTownMusic && !targetLocation.IsOutdoors)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x000B1B48 File Offset: 0x000AFD48
		public void lockedDoorWarp(Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
		{
			bool town_key_applies = Game1.player.HasTownKey;
			if (GameLocation.AreStoresClosedForFestival() && this.InValleyContext())
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FestivalDay_DoorLocked")));
				return;
			}
			if (locationName == "SeedShop" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393") && !town_key_applies)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed")));
				return;
			}
			if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
			{
				openTime = 800;
			}
			if (town_key_applies)
			{
				if (town_key_applies && !this.InValleyContext())
				{
					town_key_applies = false;
				}
				if (town_key_applies && this is BeachNightMarket && locationName != "FishShop")
				{
					town_key_applies = false;
				}
			}
			Friendship friendship;
			bool canOpenDoor = (town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime)) && (minFriendship <= 0 || this.IsWinterHere() || (Game1.player.friendshipData.TryGetValue(npcName, out friendship) && friendship.Points >= minFriendship));
			if (this.IsGreenRainingHere() && Game1.year == 1 && !(this is Beach) && !(this is Forest) && !locationName.Equals("AdventureGuild"))
			{
				canOpenDoor = true;
			}
			if (canOpenDoor)
			{
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				this.playSound("doorClose", new Vector2?(Game1.player.Tile), null, SoundContext.Default);
				Game1.warpFarmer(locationName, tile.X, tile.Y, false);
				return;
			}
			if (minFriendship <= 0)
			{
				string openTimeString = Game1.getTimeOfDayString(openTime).Replace(" ", "");
				if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
				{
					openTimeString = Game1.getTimeOfDayString(800).Replace(" ", "");
				}
				string closeTimeString = Game1.getTimeOfDayString(closeTime).Replace(" ", "");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTimeString, closeTimeString));
				return;
			}
			if (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				return;
			}
			NPC character = Game1.getCharacterFromName(npcName, true, false);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", character.displayName));
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x000B1DD4 File Offset: 0x000AFFD4
		public void playElliottPiano(int key)
		{
			if (Game1.IsMultiplayer && Game1.player.UniqueMultiplayerID % 111L == 0L)
			{
				switch (key)
				{
				case 1:
				{
					string audioName = "toyPiano";
					int? pitch = new int?(500);
					this.playSound(audioName, null, pitch, SoundContext.Default);
					return;
				}
				case 2:
				{
					string audioName2 = "toyPiano";
					int? pitch = new int?(1200);
					this.playSound(audioName2, null, pitch, SoundContext.Default);
					return;
				}
				case 3:
				{
					string audioName3 = "toyPiano";
					int? pitch = new int?(1400);
					this.playSound(audioName3, null, pitch, SoundContext.Default);
					return;
				}
				case 4:
				{
					string audioName4 = "toyPiano";
					int? pitch = new int?(2000);
					this.playSound(audioName4, null, pitch, SoundContext.Default);
					return;
				}
				default:
					return;
				}
			}
			else
			{
				switch (key)
				{
				case 1:
				{
					string audioName5 = "toyPiano";
					int? pitch = new int?(1100);
					this.playSound(audioName5, null, pitch, SoundContext.Default);
					break;
				}
				case 2:
				{
					string audioName6 = "toyPiano";
					int? pitch = new int?(1500);
					this.playSound(audioName6, null, pitch, SoundContext.Default);
					break;
				}
				case 3:
				{
					string audioName7 = "toyPiano";
					int? pitch = new int?(1600);
					this.playSound(audioName7, null, pitch, SoundContext.Default);
					break;
				}
				case 4:
				{
					string audioName8 = "toyPiano";
					int? pitch = new int?(1800);
					this.playSound(audioName8, null, pitch, SoundContext.Default);
					break;
				}
				}
				switch (Game1.elliottPiano)
				{
				case 0:
					if (key == 2)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 1:
					if (key == 4)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 2:
					if (key == 3)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 3:
					if (key == 2)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 4:
					if (key == 3)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 5:
					if (key == 4)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 6:
					if (key == 2)
					{
						Game1.elliottPiano++;
						return;
					}
					Game1.elliottPiano = 0;
					return;
				case 7:
					if (key == 1)
					{
						Game1.elliottPiano = 0;
						NPC elliott = this.getCharacterFromName("Elliott");
						if (!Game1.eventUp && elliott != null && !elliott.isMoving())
						{
							elliott.faceTowardFarmerForPeriod(1000, 100, false, Game1.player);
							elliott.doEmote(20, true);
							return;
						}
					}
					else
					{
						Game1.elliottPiano = 0;
					}
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x000B2070 File Offset: 0x000B0270
		public void readNote(int which)
		{
			if (Game1.netWorldState.Value.LostBooksFound >= which)
			{
				string message = Game1.content.LoadString("Strings\\Notes:" + which.ToString()).Replace('\n', '^');
				Game1.player.mailReceived.Add("lb_" + which.ToString());
				this.removeTemporarySpritesWithIDLocal(which);
				Game1.drawLetterMessage(message);
				return;
			}
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Notes:Missing")));
		}

		// Token: 0x06000F5B RID: 3931 RVA: 0x000B20FC File Offset: 0x000B02FC
		public void mailbox()
		{
			if (Game1.mailbox.Count > 0)
			{
				string mailTitle = Game1.mailbox[0];
				if (!mailTitle.Contains("passedOut") && !mailTitle.Contains("Cooking"))
				{
					Game1.player.mailReceived.Add(mailTitle);
				}
				Game1.mailbox.RemoveAt(0);
				Dictionary<string, string> mails = DataLoader.Mail(Game1.content);
				string mail = mails.GetValueOrDefault(mailTitle, "");
				if (mailTitle.StartsWith("passedOut"))
				{
					if (mailTitle.StartsWith("passedOut "))
					{
						string[] split = ArgUtility.SplitBySpace(mailTitle);
						int moneyTaken = (split.Length > 1) ? Convert.ToInt32(split[1]) : 0;
						int num = Utility.CreateDaySaveRandom((double)moneyTaken, 0.0, 0.0).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3);
						string translationKey;
						if (num != 0)
						{
							if (num != 1)
							{
								translationKey = "passedOut3_" + ((moneyTaken > 0) ? "Billed" : "NotBilled");
							}
							else
							{
								translationKey = "passedOut2";
							}
						}
						else
						{
							translationKey = ((Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember")) ? "passedOut4" : ("passedOut1_" + ((moneyTaken > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")));
						}
						mail = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, mails[translationKey]);
						mail = string.Format(mail, moneyTaken);
					}
					else
					{
						string[] split2 = ArgUtility.SplitBySpace(mailTitle);
						if (split2.Length > 1)
						{
							int moneyTaken2 = Convert.ToInt32(split2[1]);
							mail = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, mails[split2[0]]);
							mail = string.Format(mail, moneyTaken2);
						}
					}
				}
				if (mail.Length > 0)
				{
					Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle, false);
					return;
				}
			}
			else if (Game1.mailbox.Count == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));
			}
		}

		// Token: 0x06000F5C RID: 3932 RVA: 0x000B2330 File Offset: 0x000B0530
		public void farmerFile()
		{
			Game1.multipleDialogues(new string[]
			{
				Game1.content.LoadString("Strings\\UI:FarmerFile_1", new object[]
				{
					Game1.player.Name,
					Game1.stats.StepsTaken,
					Game1.stats.GiftsGiven,
					Game1.stats.DaysPlayed,
					Game1.stats.DirtHoed,
					Game1.stats.ItemsCrafted,
					Game1.stats.ItemsCooked,
					Game1.stats.PiecesOfTrashRecycled
				}).Replace('\n', '^'),
				Game1.content.LoadString("Strings\\UI:FarmerFile_2", new object[]
				{
					Game1.stats.MonstersKilled,
					Game1.stats.FishCaught,
					Game1.stats.TimesFished,
					Game1.stats.SeedsSown,
					Game1.stats.ItemsShipped
				}).Replace('\n', '^')
			});
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x000B2470 File Offset: 0x000B0670
		public int getTotalCrops()
		{
			int amount = 0;
			foreach (TerrainFeature terrainFeature in this.terrainFeatures.Values)
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null && dirt.crop != null && !dirt.crop.dead.Value)
				{
					amount++;
				}
			}
			return amount;
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x000B24F0 File Offset: 0x000B06F0
		public int getTotalCropsReadyForHarvest()
		{
			int amount = 0;
			foreach (TerrainFeature terrainFeature in this.terrainFeatures.Values)
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null && dirt.readyForHarvest())
				{
					amount++;
				}
			}
			return amount;
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x000B255C File Offset: 0x000B075C
		public int getTotalUnwateredCrops()
		{
			int amount = 0;
			foreach (TerrainFeature terrainFeature in this.terrainFeatures.Values)
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null && dirt.crop != null && dirt.needsWatering() && !dirt.isWatered())
				{
					amount++;
				}
			}
			return amount;
		}

		// Token: 0x06000F60 RID: 3936 RVA: 0x000B25D8 File Offset: 0x000B07D8
		public int? getTotalGreenhouseCropsReadyForHarvest()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
			{
				int amount = 0;
				foreach (TerrainFeature terrainFeature in Game1.RequireLocation("Greenhouse", false).terrainFeatures.Values)
				{
					HoeDirt dirt = terrainFeature as HoeDirt;
					if (dirt != null && dirt.readyForHarvest())
					{
						amount++;
					}
				}
				return new int?(amount);
			}
			return null;
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x000B2674 File Offset: 0x000B0874
		public int getTotalOpenHoeDirt()
		{
			int amount = 0;
			foreach (TerrainFeature t in this.terrainFeatures.Values)
			{
				HoeDirt dirt = t as HoeDirt;
				if (dirt != null && dirt.crop == null && !this.objects.ContainsKey(t.Tile))
				{
					amount++;
				}
			}
			return amount;
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x000B26F8 File Offset: 0x000B08F8
		public int getTotalForageItems()
		{
			int amount = 0;
			using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator = this.objects.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.isSpawnedObject.Value)
					{
						amount++;
					}
				}
			}
			return amount;
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x000B275C File Offset: 0x000B095C
		public int getNumberOfMachinesReadyForHarvest()
		{
			int num = 0;
			using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator = this.objects.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsConsideredReadyMachineForComputer())
					{
						num++;
					}
				}
			}
			string houseName = null;
			if (!(this is Farm))
			{
				IslandWest islandWest = this as IslandWest;
				if (islandWest != null)
				{
					if (islandWest.farmhouseRestored.Value)
					{
						houseName = "IslandFarmHouse";
					}
				}
			}
			else
			{
				houseName = "FarmHouse";
			}
			if (houseName != null)
			{
				using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator = Game1.RequireLocation(houseName, false).objects.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.IsConsideredReadyMachineForComputer())
						{
							num++;
						}
					}
				}
			}
			foreach (Building building in this.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null)
				{
					using (Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator = indoors.objects.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.IsConsideredReadyMachineForComputer())
							{
								num++;
							}
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x000B28D4 File Offset: 0x000B0AD4
		public static void openCraftingMenu()
		{
			Game1.activeClickableMenu = new GameMenu(GameMenu.craftingTab, -1, true);
		}

		// Token: 0x06000F65 RID: 3941 RVA: 0x000B28E8 File Offset: 0x000B0AE8
		public virtual bool HandleBuyAction(string which)
		{
			if (which.Equals("Fish"))
			{
				string shopId = "FishShop";
				int? maxOwnerY = new int?(Game1.player.TilePoint.Y - 1);
				return Utility.TryOpenShopMenu(shopId, this, null, maxOwnerY, false, true, null);
			}
			if (this is SeedShop)
			{
				if (this.getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_MoneyBox"));
					Game1.afterDialogues = delegate()
					{
						Utility.TryOpenShopMenu("SeedShop", null, true);
					};
				}
				else
				{
					Utility.TryOpenShopMenu("SeedShop", this, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(4, 17, 1, 1)), new int?(Game1.player.TilePoint.Y - 1), false, true, null);
				}
				return true;
			}
			if (this.name.Equals("SandyHouse"))
			{
				string shopId2 = "Sandy";
				Microsoft.Xna.Framework.Rectangle? ownerArea = null;
				int? maxOwnerY = null;
				Utility.TryOpenShopMenu(shopId2, this, ownerArea, maxOwnerY, false, true, null);
				return true;
			}
			return false;
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x000B2A00 File Offset: 0x000B0C00
		public virtual bool isObjectAt(int x, int y)
		{
			Vector2 v = new Vector2((float)(x / 64), (float)(y / 64));
			using (List<Furniture>.Enumerator enumerator = this.furniture.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.boundingBox.Value.Contains(x, y))
					{
						return true;
					}
				}
			}
			return this.objects.ContainsKey(v);
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x000B2A84 File Offset: 0x000B0C84
		public virtual bool isObjectAtTile(int tileX, int tileY)
		{
			Vector2 v = new Vector2((float)tileX, (float)tileY);
			using (List<Furniture>.Enumerator enumerator = this.furniture.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.boundingBox.Value.Contains(tileX * 64, tileY * 64))
					{
						return true;
					}
				}
			}
			return this.objects.ContainsKey(v);
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x000B2B08 File Offset: 0x000B0D08
		public virtual Object getObjectAt(int x, int y, bool ignorePassables = false)
		{
			Vector2 v = new Vector2((float)(x / 64), (float)(y / 64));
			foreach (Furniture f in this.furniture)
			{
				if (f.boundingBox.Value.Contains(x, y) && (!ignorePassables || !f.isPassable()))
				{
					return f;
				}
			}
			Object obj = null;
			this.objects.TryGetValue(v, out obj);
			if (ignorePassables && obj != null && obj.isPassable())
			{
				obj = null;
			}
			return obj;
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x000B2BB4 File Offset: 0x000B0DB4
		public Object getObjectAtTile(int x, int y, bool ignorePassables = false)
		{
			return this.getObjectAt(x * 64, y * 64, ignorePassables);
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x000B2BC8 File Offset: 0x000B0DC8
		public virtual bool saloon(Location tileLocation)
		{
			NPC gus = this.getCharacterFromName("Gus");
			Microsoft.Xna.Framework.Rectangle shopOwnerArea = new Microsoft.Xna.Framework.Rectangle(9, 17, 10, 2);
			if (Utility.TryOpenShopMenu("Saloon", this, new Microsoft.Xna.Framework.Rectangle?(shopOwnerArea), null, false, true, null))
			{
				if (gus != null)
				{
					gus.facePlayer(Game1.player);
				}
				return true;
			}
			if (gus == null && Game1.IsVisitingIslandToday("Gus"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_MoneyBox"));
				Game1.afterDialogues = delegate()
				{
					Utility.TryOpenShopMenu("Saloon", null, true);
				};
				return true;
			}
			return false;
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x000B2C68 File Offset: 0x000B0E68
		private void adventureShop()
		{
			if (Game1.player.itemsLostLastDeath.Count > 0)
			{
				List<Response> options = new List<Response>
				{
					new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
					new Response("Recovery", Game1.content.LoadString("Strings\\Locations:AdventureGuild_ItemRecovery")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
				};
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AdventureGuild_Greeting"), options.ToArray(), "adventureGuild");
				return;
			}
			Utility.TryOpenShopMenu("AdventureShop", "Marlon", true);
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x000B2D20 File Offset: 0x000B0F20
		public virtual bool carpenters(Location tileLocation)
		{
			foreach (NPC i in this.characters)
			{
				if (i.Name.Equals("Robin"))
				{
					if (Vector2.Distance(i.Tile, new Vector2((float)tileLocation.X, (float)tileLocation.Y)) > 3f)
					{
						return false;
					}
					i.faceDirection(2);
					if (Game1.player.daysUntilHouseUpgrade.Value < 0 && !Game1.IsThereABuildingUnderConstruction("Robin"))
					{
						List<Response> options = new List<Response>();
						options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
						if (Game1.IsMasterGame)
						{
							if (Game1.player.houseUpgradeLevel.Value < 3)
							{
								options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
							}
							else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value <= 0)
							{
								if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
								{
									options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
								}
								else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
								{
									options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
								}
							}
						}
						else if (Game1.player.houseUpgradeLevel.Value < 3)
						{
							options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeCabin")));
						}
						if (Game1.player.houseUpgradeLevel.Value >= 2)
						{
							if (Game1.IsMasterGame)
							{
								options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateHouse")));
							}
							else
							{
								options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateCabin")));
							}
						}
						options.Add(new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")));
						options.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), options.ToArray(), "carpenter");
					}
					else
					{
						Utility.TryOpenShopMenu("Carpenter", "Robin", true);
					}
					return true;
				}
			}
			if (this.getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_MoneyBox"));
				Game1.afterDialogues = delegate()
				{
					Utility.TryOpenShopMenu("Carpenter", null, true);
				};
				return true;
			}
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_RobinAbsent").Replace('\n', '^'));
				return true;
			}
			return false;
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x000B30A0 File Offset: 0x000B12A0
		public virtual bool blacksmith(Location tileLocation)
		{
			foreach (NPC i in this.characters)
			{
				if (i.Name.Equals("Clint"))
				{
					if (i.Tile != new Vector2((float)tileLocation.X, (float)(tileLocation.Y - 1)))
					{
						i.Tile != new Vector2((float)(tileLocation.X - 1), (float)(tileLocation.Y - 1));
					}
					i.faceDirection(2);
					if (Game1.player.toolBeingUpgraded.Value != null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
					{
						if (Game1.player.freeSpotsInInventory() > 0 || Game1.player.toolBeingUpgraded.Value is GenericTool)
						{
							Tool tool = Game1.player.toolBeingUpgraded.Value;
							Game1.player.toolBeingUpgraded.Value = null;
							Game1.player.hasReceivedToolUpgradeMessageYet = false;
							Game1.player.holdUpItemThenMessage(tool, true);
							if (tool is GenericTool)
							{
								tool.actionWhenClaimed();
							}
							else
							{
								Game1.player.addItemToInventoryBool(tool, false);
							}
							if (Game1.player.team.useSeparateWallets.Value && tool.UpgradeLevel == 4)
							{
								Game1.multiplayer.globalChatInfoMessage("IridiumToolUpgrade", new string[]
								{
									Game1.player.Name,
									TokenStringBuilder.ToolName(tool.QualifiedItemId, tool.UpgradeLevel)
								});
							}
						}
						else
						{
							Game1.DrawDialogue(i, "Data\\ExtraDialogue:Clint_NoInventorySpace");
						}
					}
					else
					{
						bool hasGeode = false;
						using (IEnumerator<Item> enumerator2 = Game1.player.Items.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (Utility.IsGeode(enumerator2.Current, false))
								{
									hasGeode = true;
									break;
								}
							}
						}
						Response[] responses;
						if (hasGeode)
						{
							responses = new Response[]
							{
								new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
								new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
								new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
								new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
							};
						}
						else
						{
							responses = new Response[]
							{
								new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
								new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
								new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
							};
						}
						this.createQuestionDialogue("", responses, "Blacksmith");
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x000B33C0 File Offset: 0x000B15C0
		public virtual bool animalShop(Location tileLocation)
		{
			foreach (NPC i in this.characters)
			{
				if (i.Name.Equals("Marnie"))
				{
					if (!(i.Tile != new Vector2((float)tileLocation.X, (float)(tileLocation.Y - 1))) || !(i.Tile != new Vector2((float)(tileLocation.X - 1), (float)(tileLocation.Y - 1))))
					{
						i.faceDirection(2);
						List<Response> options = new List<Response>
						{
							new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
							new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
						};
						if ((Utility.getAllPets().Count == 0 && Game1.year >= 2) || Game1.player.mailReceived.Contains("MarniePetAdoption") || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption"))
						{
							options.Insert(2, new Response("Adopt", Game1.content.LoadString("Strings\\1_6_Strings:AdoptPets")));
						}
						this.createQuestionDialogue("", options.ToArray(), "Marnie");
						return true;
					}
					if (Game1.player.stats.Get("Book_AnimalCatalogue") > 0U)
					{
						break;
					}
					return false;
				}
			}
			if (this.getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_MoneyBox"));
				Game1.afterDialogues = delegate()
				{
					Utility.TryOpenShopMenu("AnimalShop", null, true);
				};
				return true;
			}
			if (Game1.player.stats.Get("Book_AnimalCatalogue") > 0U)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Marnie_Counter"));
				Game1.afterDialogues = delegate()
				{
					List<Response> options2 = new List<Response>
					{
						new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
						new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
					};
					if ((Utility.getAllPets().Count == 0 && Game1.year >= 2) || Game1.player.mailReceived.Contains("MarniePetAdoption") || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption"))
					{
						options2.Insert(2, new Response("Adopt", Game1.content.LoadString("Strings\\1_6_Strings:AdoptPets")));
					}
					this.createQuestionDialogue("", options2.ToArray(), "Marnie");
				};
				return true;
			}
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Absent").Replace('\n', '^'));
				return true;
			}
			return false;
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x000B3654 File Offset: 0x000B1854
		public void removeTile(Location tileLocation, string layer)
		{
			this.Map.RequireLayer(layer).Tiles[tileLocation.X, tileLocation.Y] = null;
		}

		// Token: 0x06000F70 RID: 3952 RVA: 0x000B3679 File Offset: 0x000B1879
		public void removeTile(int x, int y, string layer)
		{
			this.Map.RequireLayer(layer).Tiles[x, y] = null;
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x000B3694 File Offset: 0x000B1894
		public void characterTrampleTile(Vector2 tile)
		{
			if (this is FarmHouse || this is IslandFarmHouse || this is Farm)
			{
				return;
			}
			TerrainFeature tf;
			this.terrainFeatures.TryGetValue(tile, out tf);
			Tree tree = tf as Tree;
			if (tree != null && tree.growthStage.Value < 1 && tree.instantDestroy(tile))
			{
				this.terrainFeatures.Remove(tile);
			}
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x000B36FC File Offset: 0x000B18FC
		public bool characterDestroyObjectWithinRectangle(Microsoft.Xna.Framework.Rectangle rect, bool showDestroyedObject)
		{
			if (this is FarmHouse || this is IslandFarmHouse)
			{
				return false;
			}
			foreach (Farmer farmer in this.farmers)
			{
				if (rect.Intersects(farmer.GetBoundingBox()))
				{
					return false;
				}
			}
			Vector2 tilePositionToTry = new Vector2((float)(rect.X / 64), (float)(rect.Y / 64));
			Object o;
			this.objects.TryGetValue(tilePositionToTry, out o);
			if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			TerrainFeature tf;
			this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = (float)(rect.Right / 64);
			this.objects.TryGetValue(tilePositionToTry, out o);
			if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = (float)(rect.X / 64);
			tilePositionToTry.Y = (float)(rect.Bottom / 64);
			this.objects.TryGetValue(tilePositionToTry, out o);
			if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = (float)(rect.Right / 64);
			this.objects.TryGetValue(tilePositionToTry, out o);
			if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			for (int i = this.largeTerrainFeatures.Count - 1; i >= 0; i--)
			{
				LargeTerrainFeature feature = this.largeTerrainFeatures[i];
				if (feature.isDestroyedByNPCTrample && feature.getBoundingBox().Intersects(rect))
				{
					feature.onDestroy();
					this.largeTerrainFeatures.RemoveAt(i);
					return true;
				}
			}
			for (int j = this.resourceClumps.Count - 1; j >= 0; j--)
			{
				ResourceClump clump = this.resourceClumps[j];
				if (clump.IsGreenRainBush() && clump.getBoundingBox().Intersects(rect) && clump.destroy(null, this, clump.Tile))
				{
					this.resourceClumps.RemoveAt(j);
				}
			}
			return false;
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x000B3978 File Offset: 0x000B1B78
		private bool checkDestroyTerrainFeature(TerrainFeature tf, Vector2 tilePositionToTry)
		{
			Tree tree = tf as Tree;
			if (tree != null && tree.instantDestroy(tilePositionToTry))
			{
				this.terrainFeatures.Remove(tilePositionToTry);
			}
			return false;
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x000B39A8 File Offset: 0x000B1BA8
		private bool checkDestroyItem(Object o, Vector2 tilePositionToTry, bool showDestroyedObject)
		{
			if (o != null && !o.isPassable() && !this.map.RequireLayer("Back").Tiles[(int)tilePositionToTry.X, (int)tilePositionToTry.Y].Properties.ContainsKey("NPCBarrier"))
			{
				if (o.IsSpawnedObject)
				{
					this.numberOfSpawnedObjectsOnMap--;
				}
				if (showDestroyedObject && !o.bigCraftable.Value)
				{
					TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(tilePositionToTry.X * 64f, tilePositionToTry.Y * 64f), false, o.flipped.Value)
					{
						alphaFade = 0.01f
					};
					sprite.CopyAppearanceFromItemId(o.QualifiedItemId, 0);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						sprite
					});
				}
				o.performToolAction(null);
				if (this.objects.ContainsKey(tilePositionToTry))
				{
					Chest chest = o as Chest;
					if (chest != null)
					{
						if (chest.TryMoveToSafePosition(null))
						{
							return true;
						}
						chest.destroyAndDropContents(tilePositionToTry * 64f);
					}
					this.objects.Remove(tilePositionToTry);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x000B3AE0 File Offset: 0x000B1CE0
		public Object removeObject(Vector2 location, bool showDestroyedObject)
		{
			Object o;
			this.objects.TryGetValue(location, out o);
			if (o != null && (o.CanBeGrabbed || showDestroyedObject))
			{
				if (o.IsSpawnedObject)
				{
					this.numberOfSpawnedObjectsOnMap--;
				}
				Object tmp = this.objects[location];
				this.objects.Remove(location);
				if (showDestroyedObject)
				{
					TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(location.X * 64f, location.Y * 64f), true, tmp.bigCraftable.Value, tmp.flipped.Value);
					sprite.CopyAppearanceFromItemId(tmp.QualifiedItemId, (!(tmp.Type == "Crafting")) ? 1 : 0);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						sprite
					});
				}
				if (o.IsWeeds())
				{
					Stats stats = Game1.stats;
					uint weedsEliminated = stats.WeedsEliminated;
					stats.WeedsEliminated = weedsEliminated + 1U;
				}
				return tmp;
			}
			return null;
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x000B3BD8 File Offset: 0x000B1DD8
		public void removeTileProperty(int tileX, int tileY, string layer, string key)
		{
			try
			{
				Map map = this.map;
				Tile tile2;
				if (map == null)
				{
					tile2 = null;
				}
				else
				{
					Layer layer2 = map.GetLayer(layer);
					tile2 = ((layer2 != null) ? layer2.Tiles[tileX, tileY] : null);
				}
				Tile tile = tile2;
				if (tile != null)
				{
					tile.Properties.Remove(key);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x000B3C34 File Offset: 0x000B1E34
		public void setTileProperty(int tileX, int tileY, string layer, string key, string value)
		{
			try
			{
				Map map = this.map;
				Tile tile2;
				if (map == null)
				{
					tile2 = null;
				}
				else
				{
					Layer layer2 = map.GetLayer(layer);
					tile2 = ((layer2 != null) ? layer2.Tiles[tileX, tileY] : null);
				}
				Tile tile = tile2;
				if (tile != null)
				{
					tile.Properties[key] = value;
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x000B3C98 File Offset: 0x000B1E98
		public void setObjectAt(float x, float y, Object o)
		{
			Vector2 v = new Vector2(x, y);
			this.objects[v] = o;
		}

		// Token: 0x06000F79 RID: 3961 RVA: 0x000B3CBC File Offset: 0x000B1EBC
		public virtual void cleanupBeforeSave()
		{
			this.characters.RemoveWhere((NPC npc) => npc is Junimo);
			if (this.name.Equals("WitchHut"))
			{
				this.characters.Clear();
			}
			this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature is Tent);
			foreach (Building building in this.buildings)
			{
				GameLocation value = building.indoors.Value;
				if (value != null)
				{
					value.cleanupBeforeSave();
				}
			}
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x000B3D90 File Offset: 0x000B1F90
		public virtual void cleanupForVacancy()
		{
			if (Game1.IsMasterGame)
			{
				this.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.collect(Game1.player, null));
			}
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x000B3DC4 File Offset: 0x000B1FC4
		public virtual void cleanupBeforePlayerExit()
		{
			this.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.player.Value != null && d.player.Value == Game1.player && d.collect(d.player.Value, null));
			Game1.currentLightSources.Clear();
			List<Critter> list = this.critters;
			if (list != null)
			{
				list.Clear();
			}
			Game1.onScreenMenus.RemoveWhere(delegate(IClickableMenu menu)
			{
				if (menu.destroy)
				{
					IDisposable disposable = menu as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
					return true;
				}
				return false;
			});
			AmbientLocationSounds.onLocationLeave();
			Ring value = Game1.player.rightRing.Value;
			if (value != null)
			{
				value.onLeaveLocation(Game1.player, this);
			}
			Ring value2 = Game1.player.leftRing.Value;
			if (value2 != null)
			{
				value2.onLeaveLocation(Game1.player, this);
			}
			if (this.name.Equals("AbandonedJojaMart") && this.farmers.Count <= 1)
			{
				this.characters.RemoveWhere((NPC npc) => npc is Junimo);
			}
			this.furnitureToRemove.Clear();
			this.interiorDoors.CleanUpLocalState();
			Game1.temporaryContent.Unload();
			Utility.CollectGarbage("", 0);
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x000B3EFC File Offset: 0x000B20FC
		public static string getWeedForSeason(Random r, Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return r.Choose("(O)784", "(O)674", "(O)675");
			case Season.Summer:
				return r.Choose("(O)785", "(O)676", "(O)677");
			case Season.Fall:
				return r.Choose("(O)786", "(O)678", "(O)679");
			default:
				return "(O)674";
			}
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x000B3F64 File Offset: 0x000B2164
		private void startSleep()
		{
			Game1.player.timeWentToBed.Value = Game1.timeOfDay;
			if (Game1.IsMultiplayer)
			{
				Game1.netReady.SetLocalReady("sleep", true);
				Game1.dialogueUp = false;
				Game1.activeClickableMenu = new ReadyCheckDialog("sleep", true, delegate(Farmer who)
				{
					this.doSleep();
				}, delegate(Farmer who)
				{
					ReadyCheckDialog readyCheckDialog = Game1.activeClickableMenu as ReadyCheckDialog;
					if (readyCheckDialog != null)
					{
						readyCheckDialog.closeDialog(who);
					}
					who.timeWentToBed.Value = 0;
				});
			}
			else
			{
				this.doSleep();
			}
			if (!Game1.IsDedicatedHost && !Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
			{
				Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
				if (Game1.IsMultiplayer && (Game1.player.team.sleepAnnounceMode.Value == FarmerTeam.SleepAnnounceModes.All || (Game1.player.team.sleepAnnounceMode.Value == FarmerTeam.SleepAnnounceModes.First && Game1.player.team.announcedSleepingFarmers.Count == 1)))
				{
					string key = "GoneToBed";
					if (Game1.random.NextDouble() < 0.75)
					{
						if (Game1.timeOfDay < 1800)
						{
							key += "Early";
						}
						else if (Game1.timeOfDay > 2530)
						{
							key += "Late";
						}
					}
					int key_index = 0;
					for (int i = 0; i < 2; i++)
					{
						if (Game1.random.NextDouble() < 0.25)
						{
							key_index++;
						}
					}
					Game1.multiplayer.globalChatInfoMessage(key + key_index.ToString(), new string[]
					{
						Game1.player.displayName
					});
				}
			}
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x000B4118 File Offset: 0x000B2318
		protected virtual void _CleanupPagedResponses()
		{
			GameLocation._PagedResponses.Clear();
			GameLocation._OnPagedResponse = null;
			GameLocation._PagedResponsePrompt = null;
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x000B4130 File Offset: 0x000B2330
		public virtual void ShowPagedResponses(string prompt, List<KeyValuePair<string, string>> responses, Action<string> on_response, bool auto_select_single_choice = false, bool addCancel = true, int itemsPerPage = 5)
		{
			GameLocation._PagedResponses.Clear();
			GameLocation._PagedResponses.AddRange(responses);
			GameLocation._PagedResponsePage = 0;
			GameLocation._PagedResponseAddCancel = addCancel;
			GameLocation._PagedResponseItemsPerPage = itemsPerPage;
			GameLocation._PagedResponsePrompt = prompt;
			GameLocation._OnPagedResponse = on_response;
			if (GameLocation._PagedResponses.Count == 1 && auto_select_single_choice)
			{
				on_response(GameLocation._PagedResponses[0].Key);
				return;
			}
			if (GameLocation._PagedResponses.Count > 0)
			{
				this._ShowPagedResponses(GameLocation._PagedResponsePage);
			}
		}

		// Token: 0x06000F80 RID: 3968 RVA: 0x000B41B8 File Offset: 0x000B23B8
		protected virtual void _ShowPagedResponses(int page = -1)
		{
			GameLocation._PagedResponsePage = page;
			int itemsPerPage = GameLocation._PagedResponseItemsPerPage;
			int pages = (GameLocation._PagedResponses.Count - 1) / itemsPerPage;
			int itemsOnCurPage = itemsPerPage;
			if (GameLocation._PagedResponsePage == pages - 1 && GameLocation._PagedResponses.Count % itemsPerPage == 1)
			{
				itemsOnCurPage++;
				pages--;
			}
			List<Response> locationResponses = new List<Response>();
			for (int i = 0; i < itemsOnCurPage; i++)
			{
				int index = i + GameLocation._PagedResponsePage * itemsPerPage;
				if (index < GameLocation._PagedResponses.Count)
				{
					KeyValuePair<string, string> response = GameLocation._PagedResponses[index];
					locationResponses.Add(new Response(response.Key, response.Value));
				}
			}
			if (GameLocation._PagedResponsePage < pages)
			{
				locationResponses.Add(new Response("nextPage", Game1.content.LoadString("Strings\\UI:NextPage")));
			}
			if (GameLocation._PagedResponsePage > 0)
			{
				locationResponses.Add(new Response("previousPage", Game1.content.LoadString("Strings\\UI:PreviousPage")));
			}
			if (GameLocation._PagedResponseAddCancel)
			{
				locationResponses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			}
			this.createQuestionDialogue(GameLocation._PagedResponsePrompt, locationResponses.ToArray(), "pagedResponse");
		}

		// Token: 0x06000F81 RID: 3969 RVA: 0x000B42E4 File Offset: 0x000B24E4
		public virtual void ShowConstructOptions(string builder, int page = -1)
		{
			if (builder != null)
			{
				this._constructLocationBuilderName = builder;
			}
			List<KeyValuePair<string, string>> buildableLocations = new List<KeyValuePair<string, string>>();
			foreach (GameLocation location in Game1.locations)
			{
				if (location.IsBuildableLocation())
				{
					buildableLocations.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
				}
			}
			if (!buildableLocations.Any<KeyValuePair<string, string>>())
			{
				Farm farm = Game1.getFarm();
				buildableLocations.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
			}
			this.ShowPagedResponses(Game1.content.LoadString("Strings\\Buildings:Construction_ChooseLocation"), buildableLocations, delegate(string value)
			{
				GameLocation location2 = Game1.getLocationFromName(value);
				if (location2 != null)
				{
					Game1.activeClickableMenu = new CarpenterMenu(this._constructLocationBuilderName, location2);
					return;
				}
				Game1.log.Error("Can't find location '" + value + "' for construct menu.", null);
			}, true, true, 5);
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x000B43A4 File Offset: 0x000B25A4
		public void ShowAnimalShopMenu(Action<PurchaseAnimalsMenu> onMenuOpened = null)
		{
			List<KeyValuePair<string, string>> validLocations = new List<KeyValuePair<string, string>>();
			foreach (GameLocation location in Game1.locations)
			{
				if (location.buildings.Any((Building p) => p.GetIndoors() is AnimalHouse) && (!Game1.IsClient || location.CanBeRemotedlyViewed()))
				{
					validLocations.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
				}
			}
			if (!validLocations.Any<KeyValuePair<string, string>>())
			{
				Farm farm = Game1.getFarm();
				validLocations.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
			}
			Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.ChooseLocation"), validLocations, delegate(string value)
			{
				GameLocation location2 = Game1.getLocationFromName(value);
				if (location2 != null)
				{
					PurchaseAnimalsMenu menu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock(location2), location2);
					Action<PurchaseAnimalsMenu> onMenuOpened2 = onMenuOpened;
					if (onMenuOpened2 != null)
					{
						onMenuOpened2(menu);
					}
					Game1.activeClickableMenu = menu;
					return;
				}
				Game1.log.Error("Can't find location '" + value + "' for animal purchase menu.", null);
			}, true, true, 5);
		}

		// Token: 0x06000F83 RID: 3971 RVA: 0x000B44A0 File Offset: 0x000B26A0
		private void doSleep()
		{
			if (this.lightLevel.Value == 0f && Game1.timeOfDay < 2000)
			{
				if (!this.isOutdoors.Value)
				{
					this.lightLevel.Value = 0.6f;
					this.localSound("openBox", null, null, SoundContext.Default);
				}
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if (this.lightLevel.Value > 0f && Game1.timeOfDay >= 2000)
			{
				if (!this.isOutdoors.Value)
				{
					this.lightLevel.Value = 0f;
					this.localSound("openBox", null, null, SoundContext.Default);
				}
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if (Game1.IsMasterGame)
			{
				Game1.NewDay(0f);
			}
			Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
			Game1.player.lastSleepPoint.Value = Game1.player.TilePoint;
			Game1.player.mostRecentBed = Game1.player.Position;
			Game1.player.doEmote(24);
			Game1.player.freezePause = 2000;
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x000B45FC File Offset: 0x000B27FC
		public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer != null)
			{
				if (questionAndAnswer != null)
				{
					switch (questionAndAnswer.Length)
					{
					case 6:
						if (!(questionAndAnswer == "Eat_No"))
						{
							goto IL_393D;
						}
						Game1.player.isEating = false;
						Game1.player.completelyStopAnimatingOrDoingAction();
						return true;
					case 7:
					{
						char c = questionAndAnswer[0];
						if (c != 'E')
						{
							if (c != 'M')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "Mine_No"))
							{
								goto IL_393D;
							}
							Response[] noYesResponses = new Response[]
							{
								new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")),
								new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"))
							};
							this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_ResetMine")), noYesResponses, "ResetMine");
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "Eat_Yes"))
							{
								goto IL_393D;
							}
							Game1.player.isEating = false;
							Game1.player.eatHeldObject();
							return true;
						}
						break;
					}
					case 8:
					{
						char c = questionAndAnswer[0];
						if (c != 'F')
						{
							if (c != 'M')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "Mine_Yes"))
							{
								goto IL_393D;
							}
							if (Game1.CurrentMineLevel > 120)
							{
								Game1.warpFarmer("SkullCave", 3, 4, 2);
								return true;
							}
							Game1.warpFarmer("UndergroundMine", 16, 16, false);
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "Fizz_Yes"))
							{
								goto IL_393D;
							}
							if (Game1.player.Money < 500000)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
								return true;
							}
							Game1.player.Money -= 500000;
							NetWorldState value = Game1.netWorldState.Value;
							int num = value.PerfectionWaivers;
							value.PerfectionWaivers = num + 1;
							DelayedAction.playSoundAfterDelay("qi_shop_purchase", 500, null, null, -1, false);
							NPC characterFromName = this.getCharacterFromName("Fizz");
							if (characterFromName != null)
							{
								characterFromName.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Fizz_Sweet"), null, 2, 3000, 0);
							}
							NPC characterFromName2 = this.getCharacterFromName("Fizz");
							if (characterFromName2 != null)
							{
								characterFromName2.shake(500);
							}
							if (Game1.IsMultiplayer)
							{
								Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Waiver_Note_Multiplayer", false, null, new string[]
								{
									Game1.player.Name
								});
								return true;
							}
							Game1.showGlobalMessage(string.Format(Game1.content.LoadString("Strings\\1_6_Strings:Waiver_Note", Game1.netWorldState.Value.PerfectionWaivers.ToString() ?? ""), Array.Empty<object>()));
							return true;
						}
						break;
					}
					case 9:
						if (!(questionAndAnswer == "Sleep_Yes"))
						{
							goto IL_393D;
						}
						this.startSleep();
						return true;
					case 10:
					{
						char c = questionAndAnswer[0];
						if (c != 'D')
						{
							if (c != 'M')
							{
								if (c != 'S')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "Shaft_Jump"))
								{
									goto IL_393D;
								}
								MineShaft mineShaft = this as MineShaft;
								if (mineShaft != null)
								{
									mineShaft.enterMineShaft();
									return true;
								}
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "Mine_Enter"))
								{
									goto IL_393D;
								}
								Game1.enterMine(1, null);
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "Dungeon_Go"))
							{
								goto IL_393D;
							}
							Game1.enterMine(Game1.CurrentMineLevel + 1, null);
							return true;
						}
						break;
					}
					case 11:
					{
						char c = questionAndAnswer[0];
						if (c <= 'E')
						{
							if (c != 'B')
							{
								if (c != 'E')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "ExitMine_Go"))
								{
									goto IL_393D;
								}
								Game1.enterMine(Game1.CurrentMineLevel - 1, null);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "Bouquet_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Money < 500)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
									return true;
								}
								if (Game1.player.ActiveObject == null)
								{
									Game1.player.Money -= 500;
									Object bouquet = ItemRegistry.Create<Object>("(O)458", 1, 0, false);
									bouquet.CanBeSetDown = false;
									Game1.player.grabObject(bouquet);
									return true;
								}
								return true;
							}
						}
						else if (c != 'M')
						{
							if (c != 'm')
							{
								if (c != 'u')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "upgrade_Yes"))
								{
									goto IL_393D;
								}
								this.houseUpgradeAccept();
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "mariner_Buy"))
								{
									goto IL_393D;
								}
								if (Game1.player.Money < 5000)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
									return true;
								}
								Game1.player.Money -= 5000;
								Item mermaidPendant = ItemRegistry.Create("(O)460", 1, 0, false);
								mermaidPendant.specialItem = true;
								Game1.player.addItemByMenuIfNecessary(mermaidPendant, null, false);
								if (Game1.activeClickableMenu == null)
								{
									Game1.player.holdUpItemThenMessage(ItemRegistry.Create("(O)460", 1, 0, false), true);
									return true;
								}
								return true;
							}
						}
						else
						{
							if (questionAndAnswer == "Mine_Return")
							{
								Game1.enterMine(Game1.player.deepestMineLevel, null);
								return true;
							}
							if (!(questionAndAnswer == "Mariner_Buy"))
							{
								goto IL_393D;
							}
							if (Game1.player.Money >= 5000)
							{
								Game1.player.Money -= 5000;
								Object mermaidPendant2 = ItemRegistry.Create<Object>("(O)460", 1, 0, false);
								mermaidPendant2.CanBeSetDown = false;
								Game1.player.grabObject(mermaidPendant2);
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
							return true;
						}
						break;
					}
					case 12:
					{
						char c = questionAndAnswer[0];
						if (c != 'E')
						{
							if (c != 'M')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "Marnie_Adopt"))
							{
								goto IL_393D;
							}
							Utility.TryOpenShopMenu("PetAdoption", "Marnie", true);
							return true;
						}
						else if (!(questionAndAnswer == "ExitMine_Yes"))
						{
							goto IL_393D;
						}
						break;
					}
					case 13:
					{
						char c = questionAndAnswer[0];
						if (c <= 'G')
						{
							if (c != 'C')
							{
								if (c != 'G')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "GoldClock_Yes"))
								{
									goto IL_393D;
								}
								Game1.netWorldState.Value.goldenClocksTurnedOff.Value = !Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
								Game1.playSound("yoba", null);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "ClubCard_Yes."))
								{
									goto IL_393D;
								}
								goto IL_3292;
							}
						}
						else if (c != 'S')
						{
							if (c != 'd')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "dogStatue_Yes"))
							{
								goto IL_393D;
							}
							if (Game1.player.Money < 10000)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
								return true;
							}
							List<Response> skill_responses = new List<Response>();
							if (GameLocation.canRespec(0))
							{
								skill_responses.Add(new Response("farming", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604")));
							}
							if (GameLocation.canRespec(3))
							{
								skill_responses.Add(new Response("mining", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605")));
							}
							if (GameLocation.canRespec(2))
							{
								skill_responses.Add(new Response("foraging", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606")));
							}
							if (GameLocation.canRespec(1))
							{
								skill_responses.Add(new Response("fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607")));
							}
							if (GameLocation.canRespec(4))
							{
								skill_responses.Add(new Response("combat", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608")));
							}
							skill_responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueQuestion"), skill_responses.ToArray(), "professionForget");
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "SleepTent_Yes"))
							{
								goto IL_393D;
							}
							Game1.player.isInBed.Value = true;
							Game1.player.sleptInTemporaryBed.Value = true;
							Game1.displayFarmer = false;
							Game1.playSound("sandyStep", null);
							DelayedAction.playSoundAfterDelay("sandyStep", 500, null, null, -1, false);
							this.startSleep();
							return true;
						}
						break;
					}
					case 14:
					{
						char c = questionAndAnswer[1];
						if (c <= 'l')
						{
							if (c != 'a')
							{
								if (c != 'l')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "ClearHouse_Yes"))
								{
									goto IL_393D;
								}
								Vector2 playerPos = Game1.player.Tile;
								foreach (Vector2 offset in Character.AdjacentTilesOffsets)
								{
									Vector2 v = playerPos + offset;
									this.objects.Remove(v);
								}
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "carpenter_Shop"))
								{
									goto IL_393D;
								}
								Game1.player.forceCanMove();
								Utility.TryOpenShopMenu("Carpenter", "Robin", true);
								return true;
							}
						}
						else if (c != 'o')
						{
							if (c != 'u')
							{
								if (c != 'x')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "ExitMine_Leave"))
								{
									goto IL_393D;
								}
							}
							else
							{
								if (!(questionAndAnswer == "BuyQiCoins_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Money >= 1000)
								{
									Game1.player.Money -= 1000;
									this.localSound("Pickup_Coin15", null, null, SoundContext.Default);
									Game1.player.clubCoins += 100;
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8715"));
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "Bookseller_Buy"))
							{
								goto IL_393D;
							}
							Utility.TryOpenShopMenu("Bookseller", null, true);
							return true;
						}
						break;
					}
					case 15:
					{
						char c = questionAndAnswer[4];
						if (c <= 'T')
						{
							if (c != 'C')
							{
								if (c != 'S')
								{
									if (c != 'T')
									{
										goto IL_393D;
									}
									if (!(questionAndAnswer == "ExitToTitle_Yes"))
									{
										goto IL_393D;
									}
									Game1.fadeScreenToBlack();
									Game1.exitToTitle = true;
									return true;
								}
								else
								{
									if (!(questionAndAnswer == "ClubSeller_I'll"))
									{
										goto IL_393D;
									}
									if (Game1.player.Money >= 1000000)
									{
										Game1.player.Money -= 1000000;
										Game1.exitActiveMenu();
										Game1.player.forceCanMove();
										Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)127", 1, 0, false), null, false);
										return true;
									}
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_NotEnoughMoney"));
									return true;
								}
							}
							else
							{
								if (!(questionAndAnswer == "ClubCard_That's"))
								{
									goto IL_393D;
								}
								goto IL_3292;
							}
						}
						else if (c <= 'i')
						{
							if (c != 'c')
							{
								if (c != 'i')
								{
									goto IL_393D;
								}
								if (questionAndAnswer == "Marnie_Supplies")
								{
									Utility.TryOpenShopMenu("AnimalShop", "Marnie", true);
									return true;
								}
								if (!(questionAndAnswer == "Marnie_Purchase"))
								{
									goto IL_393D;
								}
								Game1.player.forceCanMove();
								Game1.currentLocation.ShowAnimalShopMenu(null);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "CalicoJack_Play"))
								{
									goto IL_393D;
								}
								if (Game1.player.clubCoins >= 100)
								{
									Game1.currentMinigame = new CalicoJack(-1, false);
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_NotEnoughCoins"));
								return true;
							}
						}
						else if (c != 'k')
						{
							if (c != 'o')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "buyJojaCola_Yes"))
							{
								goto IL_393D;
							}
							if (Game1.player.Money >= 75)
							{
								Game1.player.Money -= 75;
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)167", 1, 0, false), null, false);
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "Blacksmith_Shop"))
							{
								goto IL_393D;
							}
							Utility.TryOpenShopMenu("Blacksmith", "Clint", true);
							return true;
						}
						break;
					}
					case 16:
					{
						char c = questionAndAnswer[1];
						if (c <= 'i')
						{
							if (c != 'a')
							{
								if (c != 'i')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "WizardShrine_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Money >= 500)
								{
									Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard, false);
									Game1.player.Money -= 500;
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "CalicoJack_Rules"))
								{
									goto IL_393D;
								}
								Game1.multipleDialogues(new string[]
								{
									Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules1"),
									Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules2")
								});
								return true;
							}
						}
						else if (c != 'n')
						{
							if (c != 'o')
							{
								if (c != 'u')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "BuyClubCoins_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Money >= 1000)
								{
									Game1.player.Money -= 1000;
									Game1.player.clubCoins += 10;
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "Bookseller_Trade"))
								{
									goto IL_393D;
								}
								Utility.TryOpenShopMenu("BooksellerTrade", null, true);
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "EnterTheater_Yes"))
							{
								goto IL_393D;
							}
							Rumble.rumble(0.15f, 200f);
							Game1.player.completelyStopAnimatingOrDoingAction();
							this.playSound("doorClose", new Vector2?(Game1.player.Tile), null, SoundContext.Default);
							Game1.warpFarmer("MovieTheater", 13, 15, 0);
							return true;
						}
						break;
					}
					case 17:
					{
						char c = questionAndAnswer[0];
						if (c != 'B')
						{
							if (c != 'C')
							{
								if (c != 'c')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "carpenter_Upgrade"))
								{
									goto IL_393D;
								}
								this.houseUpgradeOffer();
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "CalicoJackHS_Play"))
								{
									goto IL_393D;
								}
								if (Game1.player.clubCoins >= 1000)
								{
									Game1.currentMinigame = new CalicoJack(-1, true);
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJackHS_NotEnoughCoins"));
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "Backpack_Purchase"))
							{
								goto IL_393D;
							}
							if (Game1.player.maxItems.Value == 12 && Game1.player.Money >= 2000)
							{
								Game1.player.Money -= 2000;
								Game1.player.increaseBackpackSize(12);
								Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708")), true);
								Game1.multiplayer.globalChatInfoMessage("BackpackLarge", new string[]
								{
									Game1.player.Name
								});
								return true;
							}
							if (Game1.player.maxItems.Value < 36 && Game1.player.Money >= 10000)
							{
								Game1.player.Money -= 10000;
								Game1.player.maxItems.Value += 12;
								Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709")), true);
								for (int i = 0; i < Game1.player.maxItems.Value; i++)
								{
									if (Game1.player.Items.Count <= i)
									{
										Game1.player.Items.Add(null);
									}
								}
								Game1.multiplayer.globalChatInfoMessage("BackpackDeluxe", new string[]
								{
									Game1.player.Name
								});
								return true;
							}
							if (Game1.player.maxItems.Value != 36)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
								return true;
							}
							return true;
						}
						break;
					}
					case 18:
					{
						char c = questionAndAnswer[12];
						if (c != 'e')
						{
							if (c != 'f')
							{
								switch (c)
								{
								case 'n':
									if (!(questionAndAnswer == "carpenter_Renovate"))
									{
										goto IL_393D;
									}
									Game1.player.forceCanMove();
									HouseRenovation.ShowRenovationMenu();
									return true;
								case 'o':
								case 'q':
									goto IL_393D;
								case 'p':
								{
									if (!(questionAndAnswer == "Blacksmith_Upgrade"))
									{
										goto IL_393D;
									}
									if (Game1.player.daysLeftForToolUpgrade.Value <= 0)
									{
										Utility.TryOpenShopMenu("ClintUpgrade", "Clint", true);
										return true;
									}
									NPC j = this.getCharacterFromName("Clint");
									if (j != null)
									{
										Game1.DrawDialogue(j, "Data\\ExtraDialogue:Clint_StillWorking", new object[]
										{
											Game1.player.toolBeingUpgraded.Value.DisplayName
										});
										return true;
									}
									return true;
								}
								case 'r':
									if (!(questionAndAnswer == "Blacksmith_Process"))
									{
										goto IL_393D;
									}
									Game1.activeClickableMenu = new GeodeMenu();
									return true;
								default:
									goto IL_393D;
								}
							}
							else
							{
								if (!(questionAndAnswer == "evilShrineLeft_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Items.ReduceId("(O)74", 1) > 0)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), false, 0f, Color.White)
										{
											interval = 50f,
											totalNumberOfLoops = 99999,
											animationLength = 7,
											layerDepth = 0.038500004f,
											scale = 4f
										}
									});
									for (int k = 0; k < 20; k++)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)), false, 0.002f, Color.LightGray)
											{
												alpha = 0.75f,
												motion = new Vector2(1f, -0.5f),
												acceleration = new Vector2(-0.002f, 0f),
												interval = 99999f,
												layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
												scale = 3f,
												scaleChange = 0.01f,
												rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
												delayBeforeAnimationStart = k * 25
											}
										});
									}
									this.playSound("fireball", null, null, SoundContext.Default);
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, false, true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(4f, -2f)
										}
									});
									if (Game1.player.getChildrenCount() > 1)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, false, true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												motion = new Vector2(4f, -1.5f),
												delayBeforeAnimationStart = 50
											}
										});
									}
									string message = "";
									foreach (NPC l in Game1.player.getChildren())
									{
										message += Game1.content.LoadString("Strings\\Locations:WitchHut_Goodbye", l.getName());
									}
									Game1.showGlobalMessage(message);
									Game1.player.getRidOfChildren();
									Game1.multiplayer.globalChatInfoMessage("EvilShrine", new string[]
									{
										Game1.player.name.Value
									});
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "CowboyGame_NewGame"))
							{
								goto IL_393D;
							}
							Game1.player.jotpkProgress.Value = null;
							Game1.currentMinigame = new AbigailGame(null);
							return true;
						}
						break;
					}
					case 19:
					{
						char c = questionAndAnswer[0];
						if (c != 'C')
						{
							if (c != 'a')
							{
								if (c != 'c')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "carpenter_Construct"))
								{
									goto IL_393D;
								}
								this.ShowConstructOptions("Robin", -1);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "adventureGuild_Shop"))
								{
									goto IL_393D;
								}
								Game1.player.forceCanMove();
								Utility.TryOpenShopMenu("AdventureShop", "Marlon", true);
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "CowboyGame_Continue"))
							{
								goto IL_393D;
							}
							Game1.currentMinigame = new AbigailGame(null);
							return true;
						}
						break;
					}
					case 20:
					{
						char c = questionAndAnswer[0];
						if (c <= 'c')
						{
							if (c != 'M')
							{
								if (c != 'c')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "communityUpgrade_Yes"))
								{
									goto IL_393D;
								}
								this.communityUpgradeAccept();
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "MinecartGame_Endless"))
								{
									goto IL_393D;
								}
								Game1.currentMinigame = new MineCart(0, 2);
								return true;
							}
						}
						else if (c != 'e')
						{
							if (c != 'p')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "pagedResponse_cancel"))
							{
								goto IL_393D;
							}
							this._CleanupPagedResponses();
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "evilShrineCenter_Yes"))
							{
								goto IL_393D;
							}
							if (Game1.player.Money >= 30000)
							{
								Game1.player.Money -= 30000;
								Game1.player.wipeExMemories();
								Game1.multiplayer.globalChatInfoMessage("EvilShrine", new string[]
								{
									Game1.player.name.Value
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(468f, 328f), false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 7,
										layerDepth = 0.038500004f,
										scale = 4f
									}
								});
								this.playSound("fireball", null, null, SoundContext.Default);
								DelayedAction.playSoundAfterDelay("debuffHit", 500, this, null, -1, false);
								int count = 0;
								Game1.player.faceDirection(2);
								Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
								{
									new FarmerSprite.AnimationFrame(94, 1500),
									new FarmerSprite.AnimationFrame(0, 1)
								}, null);
								Game1.player.freezePause = 1500;
								Game1.player.jitterStrength = 1f;
								for (int m = 0; m < 20; m++)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(7f, 5f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)), false, 0.002f, Color.SlateGray)
										{
											alpha = 0.75f,
											motion = new Vector2(0f, -0.5f),
											acceleration = new Vector2(-0.002f, 0f),
											interval = 99999f,
											layerDepth = 0.032f + (float)Game1.random.Next(100) / 10000f,
											scale = 3f,
											scaleChange = 0.01f,
											rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
											delayBeforeAnimationStart = m * 25
										}
									});
								}
								for (int n = 0; n < 16; n++)
								{
									foreach (Vector2 v2 in Utility.getBorderOfThisRectangle(Utility.getRectangleCenteredAt(new Vector2(7f, 5f), 2 + n * 2)))
									{
										if (count % 2 == 0)
										{
											Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
											{
												new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(692, 1853, 4, 4), 25f, 1, 16, v2 * 64f + new Vector2(32f, 32f), false, false)
												{
													layerDepth = 1f,
													delayBeforeAnimationStart = n * 50,
													scale = 4f,
													scaleChange = 1f,
													color = new Color((int)(byte.MaxValue - Utility.getRedToGreenLerpColor(1f / (float)(n + 1)).R), (int)(byte.MaxValue - Utility.getRedToGreenLerpColor(1f / (float)(n + 1)).G), (int)(byte.MaxValue - Utility.getRedToGreenLerpColor(1f / (float)(n + 1)).B)),
													acceleration = new Vector2(-0.1f, 0f)
												}
											});
										}
										count++;
									}
								}
								return true;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
							return true;
						}
						break;
					}
					case 21:
					{
						char c = questionAndAnswer[0];
						if (c != 'M')
						{
							if (c != 'S')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "ShrineOfChallenge_Yes"))
							{
								goto IL_393D;
							}
							Game1.player.team.toggleMineShrineOvernight.Value = true;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
							Game1.multiplayer.globalChatInfoMessage((!Game1.player.team.mineShrineActivated.Value) ? "HardModeMinesActivated" : "HardModeMinesDeactivated", new string[]
							{
								Game1.player.Name
							});
							DelayedAction.functionAfterDelay(delegate
							{
								if (!Game1.player.team.mineShrineActivated.Value)
								{
									Game1.playSound("fireball", null);
									this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 4,
										lightId = "ShrineOfChallenge_Activation_1",
										id = 888,
										lightRadius = 2f,
										scale = 4f,
										yPeriodic = true,
										lightcolor = new Color(100, 0, 0),
										yPeriodicLoopTime = 1000f,
										yPeriodicRange = 4f,
										layerDepth = 0.04544f
									});
									this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 4,
										lightId = "ShrineOfChallenge_Activation_2",
										id = 889,
										lightRadius = 2f,
										scale = 4f,
										lightcolor = new Color(100, 0, 0),
										yPeriodic = true,
										yPeriodicLoopTime = 1100f,
										yPeriodicRange = 4f,
										layerDepth = 0.04544f
									});
									return;
								}
								this.removeTemporarySpritesWithID(888);
								this.removeTemporarySpritesWithID(889);
								Game1.playSound("fireball", null);
							}, 500);
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "MinecartGame_Progress"))
							{
								goto IL_393D;
							}
							Game1.currentMinigame = new MineCart(0, 3);
							return true;
						}
						break;
					}
					case 22:
					{
						char c = questionAndAnswer[0];
						if (c != 'S')
						{
							if (c != 'p')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "pagedResponse_nextPage"))
							{
								goto IL_393D;
							}
							this._ShowPagedResponses(GameLocation._PagedResponsePage + 1);
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "SquidFestBooth_Rewards"))
							{
								goto IL_393D;
							}
							if (Game1.player.mailReceived.Contains(string.Concat(new string[]
							{
								"GotSquidFestReward_",
								Game1.year.ToString(),
								"_",
								Game1.dayOfMonth.ToString(),
								"_3"
							})) || Game1.player.mailReceived.Contains(string.Concat(new string[]
							{
								"GotSquidFestReward_",
								Game1.year.ToString(),
								"_",
								Game1.dayOfMonth.ToString(),
								"_3"
							})))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFest_GotAllRewardsToday"));
								return true;
							}
							List<string> availableRewards = new List<string>();
							int[] array;
							if (Game1.dayOfMonth != 12)
							{
								RuntimeHelpers.InitializeArray(array = new int[4], fieldof(<PrivateImplementationDetails>.470ECF406D2EE2DD961A4E100C987577296AEC0682B55A955BF1C894FA7EA362).FieldHandle);
							}
							else
							{
								RuntimeHelpers.InitializeArray(array = new int[4], fieldof(<PrivateImplementationDetails>.149A886C82F71D1EB8C19D213C0088AC0B43028C9071B748925AF596A057C4F8).FieldHandle);
							}
							int[] squidTargets = array;
							int currentSquid = (int)Game1.stats.Get(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year));
							bool alreadyReceivedAllRewards = false;
							bool alreadyGotCrabbingBook = Game1.player.mailReceived.Contains("GotCrabbingBook");
							for (int i2 = 0; i2 < squidTargets.Length; i2++)
							{
								if (currentSquid >= squidTargets[i2])
								{
									if (!Game1.player.mailReceived.Contains(string.Concat(new string[]
									{
										"GotSquidFestReward_",
										Game1.year.ToString(),
										"_",
										Game1.dayOfMonth.ToString(),
										"_",
										i2.ToString()
									})))
									{
										availableRewards.Add(Game1.dayOfMonth.ToString() + "_" + i2.ToString());
										Game1.player.mailReceived.Add(string.Concat(new string[]
										{
											"GotSquidFestReward_",
											Game1.year.ToString(),
											"_",
											Game1.dayOfMonth.ToString(),
											"_",
											i2.ToString()
										}));
										alreadyReceivedAllRewards = false;
										if (!alreadyGotCrabbingBook && i2 >= 3)
										{
											Game1.player.mailReceived.Add("GotCrabbingBook");
										}
									}
									else
									{
										alreadyReceivedAllRewards = true;
									}
								}
							}
							if (availableRewards.Count <= 0)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString(alreadyReceivedAllRewards ? "Strings\\1_6_Strings:SquidFest_AlreadyGotAvailableRewards" : "Strings\\1_6_Strings:SquidFestBooth_NoRewards"));
								return true;
							}
							List<Item> rewards = new List<Item>();
							Random r = Utility.CreateDaySaveRandom((double)(Game1.year * 2000), (double)(Game1.dayOfMonth * 10), 0.0);
							foreach (string s in availableRewards)
							{
								if (s != null)
								{
									int num = s.Length;
									if (num == 4)
									{
										switch (s[3])
										{
										case '0':
											if (!(s == "12_0"))
											{
												if (s == "13_0")
												{
													rewards.Add(ItemRegistry.Create("(O)694", 1, 0, false));
												}
											}
											else
											{
												rewards.Add(ItemRegistry.Create("(O)DeluxeBait", 20, 0, false));
											}
											break;
										case '1':
											if (!(s == "12_1"))
											{
												if (s == "13_1")
												{
													rewards.Add((r.NextDouble() < 0.5) ? ItemRegistry.Create("(O)498", 15, 0, false) : ItemRegistry.Create("(O)MysteryBox", 3, 0, false));
													rewards.Add(ItemRegistry.Create("(O)242", 1, 0, false));
												}
											}
											else
											{
												rewards.Add((r.NextDouble() < 0.5) ? ItemRegistry.Create("(O)498", 10, 0, false) : ItemRegistry.Create("(O)MysteryBox", 2, 0, false));
												rewards.Add(ItemRegistry.Create("(O)242", 1, 0, false));
											}
											break;
										case '2':
											if (!(s == "12_2"))
											{
												if (s == "13_2")
												{
													rewards.Add(ItemRegistry.Create("(O)166", 1, 0, false));
													rewards.Add(ItemRegistry.Create("(O)253", 3, 0, false));
												}
											}
											else
											{
												rewards.Add(ItemRegistry.Create("(O)797", 1, 0, false));
												rewards.Add(ItemRegistry.Create("(O)395", 3, 0, false));
											}
											break;
										case '3':
											if (!(s == "12_3"))
											{
												if (s == "13_3")
												{
													rewards.Add(new Hat("SquidHat"));
													if (!alreadyGotCrabbingBook)
													{
														rewards.Add(ItemRegistry.Create("(O)Book_Crabbing", 1, 0, false));
													}
													else
													{
														rewards.Add(ItemRegistry.Create("(O)MysteryBox", 3, 0, false));
														rewards.Add(ItemRegistry.Create("(O)265", 1, 0, false));
													}
												}
											}
											else
											{
												rewards.Add(new Furniture("SquidKid_Painting", Vector2.Zero));
												if (!alreadyGotCrabbingBook)
												{
													rewards.Add(ItemRegistry.Create("(O)Book_Crabbing", 1, 0, false));
												}
												else
												{
													rewards.Add(ItemRegistry.Create("(O)MysteryBox", 3, 0, false));
													rewards.Add(ItemRegistry.Create("(O)265", 1, 0, false));
												}
											}
											break;
										}
									}
								}
							}
							if (rewards.Count > 0)
							{
								ItemGrabMenu itemGrabMenu = new ItemGrabMenu(rewards, null).setEssential(true, true);
								itemGrabMenu.inventory.showGrayedOutSlots = true;
								itemGrabMenu.source = 2;
								Game1.activeClickableMenu = itemGrabMenu;
								return true;
							}
							return true;
						}
						break;
					}
					case 23:
					{
						char c = questionAndAnswer[19];
						if (c <= 'm')
						{
							if (c != 'a')
							{
								if (c != 'm')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "professionForget_combat"))
								{
									goto IL_393D;
								}
								if (Game1.player.newLevels.Contains(new Point(4, 5)) || Game1.player.newLevels.Contains(new Point(4, 10)))
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
									return true;
								}
								Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
								GameLocation.RemoveProfession(26);
								GameLocation.RemoveProfession(27);
								GameLocation.RemoveProfession(29);
								GameLocation.RemoveProfession(25);
								GameLocation.RemoveProfession(28);
								GameLocation.RemoveProfession(24);
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
								int num2 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
								if (num2 >= 5)
								{
									Game1.player.newLevels.Add(new Point(4, 5));
								}
								if (num2 >= 10)
								{
									Game1.player.newLevels.Add(new Point(4, 10));
								}
								DelayedAction.playSoundAfterDelay("dog_bark", 300, null, null, -1, false);
								DelayedAction.playSoundAfterDelay("dog_bark", 900, null, null, -1, false);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "TroutDerbyBooth_Rewards"))
								{
									goto IL_393D;
								}
								if (Game1.player.Items.CountId("TroutDerbyTag") <= 0)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_NoTags"));
									return true;
								}
								Item reward = null;
								int rewardIndex = (int)((long)Utility.CreateRandom(Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0, 0.0).Next(10) + (long)((ulong)Game1.stats.Get("GoldenTagsTurnedIn"))) % 10;
								if (Game1.stats.Get("GoldenTagsTurnedIn") == 0U)
								{
									reward = ItemRegistry.Create("(O)TentKit", 1, 0, false);
								}
								else
								{
									switch (rewardIndex)
									{
									case 0:
										reward = ItemRegistry.Create("(H)BucketHat", 1, 0, false);
										break;
									case 1:
										reward = ItemRegistry.Create("(O)710", 1, 0, false);
										break;
									case 2:
										reward = ItemRegistry.Create("(O)MysteryBox", 3, 0, false);
										break;
									case 3:
										reward = ItemRegistry.Create("(O)72", 1, 0, false);
										break;
									case 4:
										reward = ItemRegistry.Create("(F)MountedTrout_Painting", 1, 0, false);
										break;
									case 5:
										reward = ItemRegistry.Create("(O)DeluxeBait", 20, 0, false);
										break;
									case 6:
										reward = ItemRegistry.Create("(O)253", 2, 0, false);
										break;
									case 7:
										reward = ItemRegistry.Create("(O)621", 1, 0, false);
										break;
									case 8:
										reward = ItemRegistry.Create("(O)688", 3, 0, false);
										break;
									case 9:
										reward = ItemRegistry.Create("(O)749", 3, 0, false);
										break;
									}
								}
								if (reward != null && (Game1.player.couldInventoryAcceptThisItem(reward) || Game1.player.Items.CountId("TroutDerbyTag") == 1))
								{
									Game1.stats.Increment("GoldenTagsTurnedIn", 1U);
									Game1.player.Items.ReduceId("TroutDerbyTag", 1);
									Game1.player.holdUpItemThenMessage(reward, true);
									Game1.player.addItemToInventoryBool(reward, false);
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_BagFull"));
								return true;
							}
						}
						else if (c != 'n')
						{
							if (c != 'v')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "adventureGuild_Recovery"))
							{
								goto IL_393D;
							}
							Game1.player.forceCanMove();
							Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon", true);
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "professionForget_mining"))
							{
								goto IL_393D;
							}
							if (Game1.player.newLevels.Contains(new Point(3, 5)) || Game1.player.newLevels.Contains(new Point(3, 10)))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
								return true;
							}
							Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
							GameLocation.RemoveProfession(23);
							GameLocation.RemoveProfession(21);
							GameLocation.RemoveProfession(18);
							GameLocation.RemoveProfession(19);
							GameLocation.RemoveProfession(22);
							GameLocation.RemoveProfession(20);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
							int num3 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
							if (num3 >= 5)
							{
								Game1.player.newLevels.Add(new Point(3, 5));
							}
							if (num3 >= 10)
							{
								Game1.player.newLevels.Add(new Point(3, 10));
							}
							DelayedAction.playSoundAfterDelay("dog_bark", 300, null, null, -1, false);
							DelayedAction.playSoundAfterDelay("dog_bark", 900, null, null, -1, false);
							return true;
						}
						break;
					}
					case 24:
					{
						char c = questionAndAnswer[18];
						if (c != 'a')
						{
							if (c != 'i')
							{
								if (c != 'o')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "specialCharmQuestion_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Items.ContainsId("(O)446"))
								{
									Game1.player.holdUpItemThenMessage(new SpecialItem(3, ""), true);
									Game1.player.removeFirstOfThisItemFromInventory("446", 1);
									Game1.player.hasSpecialCharm = true;
									Game1.player.mailReceived.Add("SecretNote20_done");
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_specialCharmNoFoot"));
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "professionForget_fishing"))
								{
									goto IL_393D;
								}
								if (Game1.player.newLevels.Contains(new Point(1, 5)) || Game1.player.newLevels.Contains(new Point(1, 10)))
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
									return true;
								}
								Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
								GameLocation.RemoveProfession(8);
								GameLocation.RemoveProfession(11);
								GameLocation.RemoveProfession(10);
								GameLocation.RemoveProfession(6);
								GameLocation.RemoveProfession(9);
								GameLocation.RemoveProfession(7);
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
								int num4 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
								if (num4 >= 5)
								{
									Game1.player.newLevels.Add(new Point(1, 5));
								}
								if (num4 >= 10)
								{
									Game1.player.newLevels.Add(new Point(1, 10));
								}
								DelayedAction.playSoundAfterDelay("dog_bark", 300, null, null, -1, false);
								DelayedAction.playSoundAfterDelay("dog_bark", 900, null, null, -1, false);
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "professionForget_farming"))
							{
								goto IL_393D;
							}
							if (Game1.player.newLevels.Contains(new Point(0, 5)) || Game1.player.newLevels.Contains(new Point(0, 10)))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
								return true;
							}
							Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
							GameLocation.RemoveProfession(0);
							GameLocation.RemoveProfession(1);
							GameLocation.RemoveProfession(3);
							GameLocation.RemoveProfession(5);
							GameLocation.RemoveProfession(2);
							GameLocation.RemoveProfession(4);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
							int num5 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
							if (num5 >= 5)
							{
								Game1.player.newLevels.Add(new Point(0, 5));
							}
							if (num5 >= 10)
							{
								Game1.player.newLevels.Add(new Point(0, 10));
							}
							DelayedAction.playSoundAfterDelay("dog_bark", 300, null, null, -1, false);
							DelayedAction.playSoundAfterDelay("dog_bark", 900, null, null, -1, false);
							return true;
						}
						break;
					}
					case 25:
					{
						if (!(questionAndAnswer == "professionForget_foraging"))
						{
							goto IL_393D;
						}
						if (Game1.player.newLevels.Contains(new Point(2, 5)) || Game1.player.newLevels.Contains(new Point(2, 10)))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
							return true;
						}
						Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
						GameLocation.RemoveProfession(16);
						GameLocation.RemoveProfession(14);
						GameLocation.RemoveProfession(17);
						GameLocation.RemoveProfession(12);
						GameLocation.RemoveProfession(13);
						GameLocation.RemoveProfession(15);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
						int num6 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
						if (num6 >= 5)
						{
							Game1.player.newLevels.Add(new Point(2, 5));
						}
						if (num6 >= 10)
						{
							Game1.player.newLevels.Add(new Point(2, 10));
						}
						DelayedAction.playSoundAfterDelay("dog_bark", 300, null, null, -1, false);
						DelayedAction.playSoundAfterDelay("dog_bark", 900, null, null, -1, false);
						return true;
					}
					case 26:
					{
						char c = questionAndAnswer[5];
						if (c <= 'R')
						{
							if (c != 'F')
							{
								if (c != 'R')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "pagedResponse_previousPage"))
								{
									goto IL_393D;
								}
								this._ShowPagedResponses(GameLocation._PagedResponsePage - 1);
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "SquidFestBooth_Explanation"))
								{
									goto IL_393D;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFestBooth_Explanation"));
								return true;
							}
						}
						else if (c != 'e')
						{
							if (c != 'n')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "carpenter_CommunityUpgrade"))
							{
								goto IL_393D;
							}
							this.communityUpgradeOffer();
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "ShrineOfSkullChallenge_Yes"))
							{
								goto IL_393D;
							}
							Game1.player.team.toggleSkullShrineOvernight.Value = true;
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
							Game1.multiplayer.globalChatInfoMessage(Game1.player.team.skullShrineActivated.Value ? "HardModeSkullCaveDeactivated" : "HardModeSkullCaveActivated", new string[]
							{
								Game1.player.Name
							});
							this.playSound(Game1.player.team.skullShrineActivated.Value ? "skeletonStep" : "serpentDie", null, null, SoundContext.Default);
							return true;
						}
						break;
					}
					case 27:
					{
						char c = questionAndAnswer[0];
						if (c != 'E')
						{
							if (c != 'T')
							{
								if (c != 'e')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "evilShrineRightActivate_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Items.ReduceId("(O)203", 1) > 0)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), false, 0f, Color.White)
										{
											interval = 50f,
											totalNumberOfLoops = 99999,
											animationLength = 7,
											layerDepth = 0.038500004f,
											scale = 4f
										}
									});
									this.playSound("fireball", null, null, SoundContext.Default);
									DelayedAction.playSoundAfterDelay("batScreech", 500, this, null, -1, false);
									for (int i3 = 0; i3 < 20; i3++)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)), false, 0.002f, Color.DarkSlateBlue)
											{
												alpha = 0.75f,
												motion = new Vector2(-0.1f, -0.5f),
												acceleration = new Vector2(-0.002f, 0f),
												interval = 99999f,
												layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
												scale = 3f,
												scaleChange = 0.01f,
												rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
												delayBeforeAnimationStart = i3 * 60
											}
										});
									}
									Game1.player.freezePause = 1501;
									for (int i4 = 0; i4 < 28; i4++)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 347, 13, 13), 50f, 4, 9999, new Vector2(12f, 5f) * 64f, false, true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
											{
												delayBeforeAnimationStart = 500 + i4 * 25,
												motion = new Vector2((float)(Game1.random.Next(1, 5) * Game1.random.Choose(-1, 1)), (float)(Game1.random.Next(1, 5) * Game1.random.Choose(-1, 1)))
											}
										});
									}
									Game1.spawnMonstersAtNight = true;
									Game1.multiplayer.globalChatInfoMessage("MonstersActivated", new string[]
									{
										Game1.player.name.Value
									});
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "TroutDerbyBooth_Explanation"))
								{
									goto IL_393D;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_Explanation"));
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "EnterTheaterSpendTicket_Yes"))
							{
								goto IL_393D;
							}
							Game1.player.Items.ReduceId("(O)809", 1);
							Rumble.rumble(0.15f, 200f);
							Game1.player.completelyStopAnimatingOrDoingAction();
							this.playSound("doorClose", new Vector2?(Game1.player.Tile), null, SoundContext.Default);
							Game1.warpFarmer("MovieTheater", 13, 15, 0);
							return true;
						}
						break;
					}
					case 28:
					case 30:
					case 31:
					case 34:
					case 35:
					case 36:
					case 37:
						goto IL_393D;
					case 29:
					{
						char c = questionAndAnswer[20];
						if (c != 'H')
						{
							if (c != 'S')
							{
								if (c != 'i')
								{
									goto IL_393D;
								}
								if (!(questionAndAnswer == "evilShrineRightDeActivate_Yes"))
								{
									goto IL_393D;
								}
								if (Game1.player.Items.ReduceId("(O)203", 1) > 0)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), false, 0f, Color.White)
										{
											interval = 50f,
											totalNumberOfLoops = 99999,
											animationLength = 7,
											layerDepth = 0.038500004f,
											scale = 4f
										}
									});
									this.playSound("fireball", null, null, SoundContext.Default);
									for (int i5 = 0; i5 < 20; i5++)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
										{
											new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)), false, 0.002f, Color.DarkSlateBlue)
											{
												alpha = 0.75f,
												motion = new Vector2(0f, -0.5f),
												acceleration = new Vector2(-0.002f, 0f),
												interval = 99999f,
												layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
												scale = 3f,
												scaleChange = 0.01f,
												rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
												delayBeforeAnimationStart = i5 * 25
											}
										});
									}
									Game1.spawnMonstersAtNight = false;
									Game1.multiplayer.globalChatInfoMessage("MonstersDeActivated", new string[]
									{
										Game1.player.name.Value
									});
									return true;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
								return true;
							}
							else
							{
								if (!(questionAndAnswer == "telephone_Carpenter_ShopStock"))
								{
									goto IL_393D;
								}
								Utility.TryOpenShopMenu("Carpenter", null, true);
								ShopMenu menu5 = Game1.activeClickableMenu as ShopMenu;
								if (menu5 != null)
								{
									menu5.readOnly = true;
									ShopMenu shopMenu = menu5;
									shopMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu closed_menu)
									{
										this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
									}));
									return true;
								}
								return true;
							}
						}
						else
						{
							if (!(questionAndAnswer == "telephone_Carpenter_HouseCost"))
							{
								goto IL_393D;
							}
							NPC characterFromName3 = Game1.getCharacterFromName("Robin", true, false);
							string upgradeTextKey = "Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse" + (Game1.player.houseUpgradeLevel.Value + 1).ToString();
							string upgrade_text = Game1.content.LoadString(upgradeTextKey, "65,000", "100");
							if (upgrade_text.Contains('.'))
							{
								upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('.') + 1);
							}
							else if (upgrade_text.Contains('。'))
							{
								upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('。') + 1);
							}
							Game1.DrawDialogue(new Dialogue(characterFromName3, upgradeTextKey, upgrade_text)
							{
								overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine")
							});
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
							{
								this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
							}));
							return true;
						}
						break;
					}
					case 32:
					{
						char c = questionAndAnswer[10];
						if (c != 'B')
						{
							if (c != 'C')
							{
								goto IL_393D;
							}
							if (!(questionAndAnswer == "telephone_Carpenter_BuildingCost"))
							{
								goto IL_393D;
							}
							GameLocation targetLocation = Game1.getFarm();
							if (Game1.currentLocation.IsBuildableLocation())
							{
								targetLocation = Game1.currentLocation;
							}
							Game1.activeClickableMenu = new CarpenterMenu("Robin", targetLocation);
							CarpenterMenu menu2 = Game1.activeClickableMenu as CarpenterMenu;
							if (menu2 != null)
							{
								menu2.readOnly = true;
								CarpenterMenu carpenterMenu = menu2;
								carpenterMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(carpenterMenu.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu closed_menu)
								{
									this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
								}));
								return true;
							}
							return true;
						}
						else
						{
							if (!(questionAndAnswer == "telephone_Blacksmith_UpgradeCost"))
							{
								goto IL_393D;
							}
							this.answerDialogueAction("Blacksmith_Upgrade", LegacyShims.EmptyArray<string>());
							ShopMenu menu3 = Game1.activeClickableMenu as ShopMenu;
							if (menu3 != null)
							{
								menu3.readOnly = true;
								ShopMenu shopMenu2 = menu3;
								shopMenu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu2.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu closed_menu)
								{
									this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
								}));
								return true;
							}
							return true;
						}
						break;
					}
					case 33:
					{
						if (!(questionAndAnswer == "telephone_SeedShop_CheckSeedStock"))
						{
							goto IL_393D;
						}
						if (!(Game1.getLocationFromName("SeedShop") is SeedShop))
						{
							this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
							return true;
						}
						if (!Utility.TryOpenShopMenu("SeedShop", null, true))
						{
							return true;
						}
						ShopMenu menu4 = Game1.activeClickableMenu as ShopMenu;
						if (menu4 != null)
						{
							menu4.readOnly = true;
							ShopMenu shopMenu3 = menu4;
							shopMenu3.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu3.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu closed_menu)
							{
								this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
							}));
							return true;
						}
						return true;
					}
					case 38:
						if (!(questionAndAnswer == "telephone_AnimalShop_CheckAnimalPrices"))
						{
							goto IL_393D;
						}
						Game1.currentLocation.ShowAnimalShopMenu(delegate(PurchaseAnimalsMenu menu)
						{
							menu.readOnly = true;
							menu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu closed_menu)
							{
								this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
							}));
						});
						return true;
					default:
						goto IL_393D;
					}
					if (Game1.CurrentMineLevel == 77377)
					{
						Game1.warpFarmer("Mine", 67, 10, true);
						return true;
					}
					if (Game1.CurrentMineLevel > 120)
					{
						Game1.warpFarmer("SkullCave", 3, 4, 2);
						return true;
					}
					Game1.warpFarmer("Mine", 23, 8, false);
					return true;
					IL_3292:
					Game1.addMail("bouncerGone", true, true);
					this.playSound("explosion", null, null, SoundContext.Default);
					Game1.flashAlpha = 5f;
					this.characters.Remove(this.getCharacterFromName("Bouncer"));
					NPC sandy = this.getCharacterFromName("Sandy");
					if (sandy != null)
					{
						sandy.faceDirection(1);
						sandy.setNewDialogue("Data\\ExtraDialogue:Sandy_PlayerClubMember", false, false);
						sandy.doEmote(16, true);
					}
					Game1.pauseThenMessage(500, Game1.content.LoadString("Strings\\Locations:Club_Bouncer_PlayerClubMember"));
					Game1.player.Halt();
					NPC characterFromName4 = Game1.getCharacterFromName("Mister Qi", true, false);
					if (characterFromName4 == null)
					{
						return true;
					}
					characterFromName4.setNewDialogue("Data\\ExtraDialogue:MisterQi_PlayerClubMember", false, false);
					return true;
				}
				IL_393D:
				if (questionAndAnswer.StartsWith("pagedResponse"))
				{
					string response = questionAndAnswer.Substring("pagedResponse".Length + 1);
					Action<string> onPagedResponse = GameLocation._OnPagedResponse;
					this._CleanupPagedResponses();
					if (onPagedResponse != null)
					{
						onPagedResponse(response);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x000B7FCC File Offset: 0x000B61CC
		public void playShopPhoneNumberSounds(string whichShop)
		{
			Random r = Utility.CreateRandom((double)whichShop.GetHashCode(), 0.0, 0.0, 0.0, 0.0);
			DelayedAction.playSoundAfterDelay("telephone_dialtone", 495, null, null, 1200, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1200, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1370, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1600, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1850, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2030, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2250, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2410, null, null, 1200 + r.Next(-4, 5) * 100, false);
			DelayedAction.playSoundAfterDelay("telephone_ringingInEar", 3150, null, null, -1, false);
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x000B8178 File Offset: 0x000B6378
		public virtual bool answerDialogue(Response answer)
		{
			string[] questionParams = (this.lastQuestionKey != null) ? ArgUtility.SplitBySpace(this.lastQuestionKey) : null;
			string questionAndAnswer = (questionParams != null) ? (questionParams[0] + "_" + answer.responseKey) : null;
			if (answer.responseKey.Equals("Move"))
			{
				Game1.player.grabObject(this.actionObjectForQuestionDialogue);
				this.removeObject(this.actionObjectForQuestionDialogue.TileLocation, false);
				this.actionObjectForQuestionDialogue = null;
				return true;
			}
			if (this.afterQuestion != null)
			{
				this.afterQuestion(Game1.player, answer.responseKey);
				this.afterQuestion = null;
				Game1.objectDialoguePortraitPerson = null;
				return true;
			}
			return questionAndAnswer != null && this.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06000F87 RID: 3975 RVA: 0x000B822E File Offset: 0x000B642E
		public static bool AreStoresClosedForFestival()
		{
			return Utility.isFestivalDay() && Utility.getStartTimeOfFestival() < 1900;
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x000B8245 File Offset: 0x000B6445
		public static void RemoveProfession(int profession)
		{
			if (Game1.player.professions.Remove(profession))
			{
				LevelUpMenu.removeImmediateProfessionPerk(profession);
			}
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x000B8260 File Offset: 0x000B6460
		public static bool canRespec(int skill_index)
		{
			return Game1.player.GetUnmodifiedSkillLevel(skill_index) >= 5 && !Game1.player.newLevels.Contains(new Point(skill_index, 5)) && !Game1.player.newLevels.Contains(new Point(skill_index, 10));
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x000B82B1 File Offset: 0x000B64B1
		public void setObject(Vector2 v, Object o)
		{
			this.objects[v] = o;
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x000B82C0 File Offset: 0x000B64C0
		private void houseUpgradeOffer()
		{
			switch (Game1.player.houseUpgradeLevel.Value)
			{
			case 0:
				this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1")), this.createYesNoResponses(), "upgrade");
				return;
			case 1:
				this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2", "65,000", "100")), this.createYesNoResponses(), "upgrade");
				return;
			case 2:
				this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3")), this.createYesNoResponses(), "upgrade");
				return;
			default:
				return;
			}
		}

		// Token: 0x06000F8C RID: 3980 RVA: 0x000B836C File Offset: 0x000B656C
		private void communityUpgradeOffer()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade1")), this.createYesNoResponses(), "communityUpgrade");
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "pamHouseUpgradeAsked", MailType.Received, true, null);
				return;
			}
			if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade2")), this.createYesNoResponses(), "communityUpgrade");
			}
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x000B8410 File Offset: 0x000B6610
		public virtual bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return false;
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x000B8414 File Offset: 0x000B6614
		private void communityUpgradeAccept()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
				{
					if (Game1.player.Money >= 300000)
					{
						Game1.player.Money -= 300000;
						Game1.RequireCharacter("Robin", true).setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", true, false);
						Game1.drawDialogue(Game1.getCharacterFromName("Robin", true, false));
						Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value = 3;
						Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", new string[]
						{
							Game1.player.Name
						});
						return;
					}
					if (Game1.player.Money < 300000)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					}
				}
				return;
			}
			if (Game1.player.Money >= 500000 && Game1.player.Items.ContainsId("(O)388", 950))
			{
				Game1.player.Money -= 500000;
				Game1.player.Items.ReduceId("(O)388", 950);
				Game1.RequireCharacter("Robin", true).setNewDialogue("Data\\ExtraDialogue:Robin_PamUpgrade_Accepted", false, false);
				Game1.drawDialogue(Game1.getCharacterFromName("Robin", true, false));
				Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value = 3;
				Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", new string[]
				{
					Game1.player.Name
				});
				return;
			}
			if (Game1.player.Money < 500000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				return;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", 950));
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x000B8608 File Offset: 0x000B6808
		private void houseUpgradeAccept()
		{
			switch (Game1.player.houseUpgradeLevel.Value)
			{
			case 0:
				if (Game1.player.Money >= 10000 && Game1.player.Items.ContainsId("(O)388", 450))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 10000;
					Game1.player.Items.ReduceId("(O)388", 450);
					Game1.RequireCharacter("Robin", true).setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", true, false);
					Game1.drawDialogue(Game1.getCharacterFromName("Robin", true, false));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", new string[]
					{
						Game1.player.Name,
						Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale)
					});
					return;
				}
				if (Game1.player.Money < 10000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					return;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", 450));
				return;
			case 1:
				if (Game1.player.Money >= 65000 && Game1.player.Items.ContainsId("(O)709", 100))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 65000;
					Game1.player.Items.ReduceId("(O)709", 100);
					Game1.RequireCharacter("Robin", true).setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", true, false);
					Game1.drawDialogue(Game1.getCharacterFromName("Robin", true, false));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", new string[]
					{
						Game1.player.Name,
						Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale)
					});
					return;
				}
				if (Game1.player.Money < 65000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					return;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughHardwood", 100));
				return;
			case 2:
				if (Game1.player.Money >= 100000)
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 100000;
					Game1.RequireCharacter("Robin", true).setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", true, false);
					Game1.drawDialogue(Game1.getCharacterFromName("Robin", true, false));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", new string[]
					{
						Game1.player.Name,
						Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale)
					});
					return;
				}
				if (Game1.player.Money < 100000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				return;
			default:
				return;
			}
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x000B8913 File Offset: 0x000B6B13
		public void destroyObject(Vector2 tileLocation, Farmer who)
		{
			this.destroyObject(tileLocation, false, who);
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x000B8920 File Offset: 0x000B6B20
		public void destroyObject(Vector2 tileLocation, bool hardDestroy, Farmer who)
		{
			Object obj;
			if (this.objects.TryGetValue(tileLocation, out obj) && obj.fragility.Value != 2 && !(obj is Chest) && obj.QualifiedItemId != "(BC)165")
			{
				bool remove = false;
				if (obj.Type == "Fish" || obj.Type == "Cooking" || obj.Type == "Crafting")
				{
					if (!(obj is BreakableContainer))
					{
						TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), true, obj.bigCraftable.Value, obj.flipped.Value);
						sprite.CopyAppearanceFromItemId(obj.QualifiedItemId, (obj.showNextIndex.Value > false) ? 1 : 0);
						sprite.scale = 4f;
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
						{
							sprite
						});
					}
					remove = true;
				}
				else if (obj.CanBeGrabbed || hardDestroy)
				{
					remove = true;
				}
				if (obj.IsBreakableStone())
				{
					remove = true;
					this.OnStoneDestroyed(obj.ItemId, (int)tileLocation.X, (int)tileLocation.Y, who);
				}
				if (remove)
				{
					this.objects.Remove(tileLocation);
				}
			}
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x000B8A78 File Offset: 0x000B6C78
		public void addOneTimeGiftBox(Item i, int x, int y, int whichGiftBox = 2)
		{
			string id = string.Concat(new string[]
			{
				this.Name,
				"_giftbox_",
				x.ToString(),
				"_",
				y.ToString()
			});
			if (!Game1.player.mailReceived.Contains(id))
			{
				Vector2 v = new Vector2((float)x, (float)y);
				Chest oldChest = this.overlayObjects.GetValueOrDefault(v, null) as Chest;
				if (oldChest == null || !(oldChest.mailToAddOnItemDump == id))
				{
					this.cleanUpTileForMapOverride(new Point(x, y));
				}
				if (!this.overlayObjects.ContainsKey(v))
				{
					Chest c = new Chest(new List<Item>
					{
						i
					}, v, true, whichGiftBox, false)
					{
						mailToAddOnItemDump = id
					};
					this.overlayObjects.Add(v, c);
				}
			}
		}

		// Token: 0x06000F93 RID: 3987 RVA: 0x000B8B48 File Offset: 0x000B6D48
		public virtual string GetLocationContextId()
		{
			if (this.locationContextId == null)
			{
				if (this.map == null)
				{
					this.reloadMap();
				}
				string contextId;
				if (this.map != null && this.map.Properties.TryGetValue("LocationContext", out contextId))
				{
					if (Game1.locationContextData.ContainsKey(contextId))
					{
						this.locationContextId = contextId;
					}
					else
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Location ");
						defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral(" has invalid LocationContext map property '");
						defaultInterpolatedStringHandler.AppendFormatted(contextId);
						defaultInterpolatedStringHandler.AppendLiteral("', ignoring value.");
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
				}
				if (this.locationContextId == null)
				{
					GameLocation parentLocation = this.GetParentLocation();
					this.locationContextId = (((parentLocation != null) ? parentLocation.GetLocationContextId() : null) ?? "Default");
				}
			}
			return this.locationContextId;
		}

		// Token: 0x06000F94 RID: 3988 RVA: 0x000B8C2C File Offset: 0x000B6E2C
		public virtual LocationContextData GetLocationContext()
		{
			return LocationContexts.Require(this.GetLocationContextId());
		}

		// Token: 0x06000F95 RID: 3989 RVA: 0x000B8C39 File Offset: 0x000B6E39
		public bool InDesertContext()
		{
			return this.GetLocationContextId() == "Desert";
		}

		// Token: 0x06000F96 RID: 3990 RVA: 0x000B8C4B File Offset: 0x000B6E4B
		public bool InIslandContext()
		{
			return this.GetLocationContextId() == "Island";
		}

		// Token: 0x06000F97 RID: 3991 RVA: 0x000B8C5D File Offset: 0x000B6E5D
		public bool InValleyContext()
		{
			return this.GetLocationContextId() == "Default";
		}

		// Token: 0x06000F98 RID: 3992 RVA: 0x000B8C70 File Offset: 0x000B6E70
		public virtual bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
		{
			if (debris.isEssentialItem())
			{
				return false;
			}
			if (debris.item != null && debris.item.HasContextTag("book_item"))
			{
				return false;
			}
			if (debris.debrisType.Value == Debris.DebrisType.OBJECT && debris.chunkType.Value == 74)
			{
				return false;
			}
			if (debris.floppingFish.Value)
			{
				using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.isTileFishable(chunkTile))
						{
							return false;
						}
					}
				}
			}
			if (!debris.isSinking.Value)
			{
				debris.isSinking.Value = true;
				Debris.DebrisType value = debris.debrisType.Value;
				if (value == Debris.DebrisType.OBJECT || value == Debris.DebrisType.RESOURCE)
				{
					if (Game1.random.NextBool())
					{
						this.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 8, 0, chunkPosition + new Vector2(-8f), false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 1f, 0.003f, 0f, 0f, false)
						{
							delayBeforeAnimationStart = Game1.random.Next(300),
							startSound = "quickSlosh"
						});
					}
					return false;
				}
			}
			else
			{
				bool anySunk = false;
				using (NetObjectShrinkList<Chunk>.Enumerator enumerator2 = debris.Chunks.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.sinkTimer.Value <= 0)
						{
							anySunk = true;
							break;
						}
					}
				}
				if (!anySunk)
				{
					return false;
				}
			}
			if (debris.debrisType.Value == Debris.DebrisType.CHUNKS)
			{
				this.localSound("quickSlosh", null, null, SoundContext.Default);
				this.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 8, 0, chunkPosition + new Vector2(-8f), false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 1f, 0.003f, 0f, 0f, false));
				return true;
			}
			this.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, chunkPosition + new Vector2(-8f), false, false));
			this.localSound("dropItemInWater", null, null, SoundContext.Default);
			return true;
		}

		// Token: 0x06000F99 RID: 3993 RVA: 0x000B8F20 File Offset: 0x000B7120
		public virtual bool doesTileSinkDebris(int xTile, int yTile, Debris.DebrisType type)
		{
			if (this.isTileBuildingFishable(xTile, yTile))
			{
				return true;
			}
			if (type == Debris.DebrisType.CHUNKS)
			{
				return this.isWaterTile(xTile, yTile) && !this.hasTileAt(xTile, yTile, "Buildings", null);
			}
			return this.isWaterTile(xTile, yTile) && !this.isTileUpperWaterBorder(this.getTileIndexAt(xTile, yTile, "Buildings", "untitled tile sheet")) && this.doesTileHaveProperty(xTile, yTile, "Passable", "Buildings", false) == null;
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x000B8F96 File Offset: 0x000B7196
		private bool isTileUpperWaterBorder(int index)
		{
			if (index <= 211)
			{
				if (index - 183 > 2 && index != 211)
				{
					return false;
				}
			}
			else if (index - 1182 > 2 && index != 1210)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x000B8FCC File Offset: 0x000B71CC
		public virtual bool doesEitherTileOrTileIndexPropertyEqual(int xTile, int yTile, string propertyName, string layerName, string propertyValue)
		{
			Map map = this.map;
			Layer layer = (map != null) ? map.GetLayer(layerName) : null;
			if (layer != null)
			{
				Tile tmp = layer.PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				string property;
				if (tmp != null && tmp.TileIndexProperties.TryGetValue(propertyName, out property) && property == propertyValue)
				{
					return true;
				}
				string property2;
				if (tmp != null && layer.PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out property2) && property2 == propertyValue)
				{
					return true;
				}
			}
			return propertyValue == null;
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x000B9070 File Offset: 0x000B7270
		public virtual bool IsNoSpawnTile(Vector2 tile, string type = "All", bool ignoreTileSheetProperties = false)
		{
			int x = (int)tile.X;
			int y = (int)tile.Y;
			string noSpawn = this.doesTileHaveProperty(x, y, "NoSpawn", "Back", ignoreTileSheetProperties);
			if (noSpawn != null)
			{
				bool isBanned;
				if (noSpawn == "Grass" || noSpawn == "Tree")
				{
					if (type == noSpawn)
					{
						return true;
					}
				}
				else if (!bool.TryParse(noSpawn, out isBanned) || isBanned)
				{
					return true;
				}
			}
			return this.getBuildingAt(tile) != null;
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x000B90E4 File Offset: 0x000B72E4
		public virtual string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName, bool ignoreTileSheetProperties = false)
		{
			Vector2 tilePos = new Vector2((float)xTile, (float)yTile);
			bool buildingOnTile = false;
			foreach (Building building in this.buildings)
			{
				if (!building.isMoving && building.occupiesTile(tilePos, true))
				{
					string tileProperty = null;
					if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tileProperty))
					{
						return tileProperty;
					}
					buildingOnTile = (buildingOnTile || building.occupiesTile(tilePos, false));
				}
			}
			foreach (Furniture f in this.furniture)
			{
				if ((float)xTile >= f.tileLocation.X - (float)f.GetAdditionalTilePropertyRadius() && (float)xTile < f.tileLocation.X + (float)f.getTilesWide() + (float)f.GetAdditionalTilePropertyRadius() && (float)yTile >= f.tileLocation.Y - (float)f.GetAdditionalTilePropertyRadius() && (float)yTile < f.tileLocation.Y + (float)f.getTilesHigh() + (float)f.GetAdditionalTilePropertyRadius())
				{
					string tile_property = null;
					if (f.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
					{
						return tile_property;
					}
				}
			}
			if (!buildingOnTile && this.map != null)
			{
				Layer layer = this.map.GetLayer(layerName);
				Tile tile = (layer != null) ? layer.Tiles[xTile, yTile] : null;
				if (tile != null)
				{
					string propertyValue;
					if (tile.Properties.TryGetValue(propertyName, out propertyValue))
					{
						return propertyValue;
					}
					if (!ignoreTileSheetProperties && tile.TileIndexProperties.TryGetValue(propertyName, out propertyValue))
					{
						return propertyValue;
					}
				}
			}
			return null;
		}

		// Token: 0x06000F9E RID: 3998 RVA: 0x000B92AC File Offset: 0x000B74AC
		public virtual string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
		{
			return this.doesTileHaveProperty(xTile, yTile, propertyName, layerName, false) ?? "";
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x000B92C4 File Offset: 0x000B74C4
		public string[] GetTilePropertySplitBySpaces(string propertyName, string layerId, int tileX, int tileY)
		{
			string raw = this.doesTileHaveProperty(tileX, tileY, propertyName, layerId, false);
			if (raw == null)
			{
				return LegacyShims.EmptyArray<string>();
			}
			return ArgUtility.SplitBySpace(raw);
		}

		// Token: 0x06000FA0 RID: 4000 RVA: 0x000B92ED File Offset: 0x000B74ED
		public bool isWaterTile(int xTile, int yTile)
		{
			return this.doesTileHaveProperty(xTile, yTile, "Water", "Back", false) != null;
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x000B9308 File Offset: 0x000B7508
		public bool isOpenWater(int xTile, int yTile)
		{
			if (!this.isWaterTile(xTile, yTile))
			{
				return false;
			}
			int tileIndexAt = this.getTileIndexAt(xTile, yTile, "Buildings", "outdoors");
			return tileIndexAt - 628 > 1 && tileIndexAt != 734 && tileIndexAt != 759 && !this.objects.ContainsKey(new Vector2((float)xTile, (float)yTile));
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x000B9368 File Offset: 0x000B7568
		public bool isCropAtTile(int tileX, int tileY)
		{
			Vector2 v = new Vector2((float)tileX, (float)tileY);
			TerrainFeature terrainFeature;
			if (this.terrainFeatures.TryGetValue(v, out terrainFeature))
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null)
				{
					return dirt.crop != null;
				}
			}
			return false;
		}

		// Token: 0x06000FA3 RID: 4003 RVA: 0x000B93A8 File Offset: 0x000B75A8
		public virtual bool dropObject(Object obj, Vector2 dropLocation, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			Vector2 tileLocation = new Vector2((float)((int)dropLocation.X / 64), (float)((int)dropLocation.Y / 64));
			obj.Location = this;
			obj.TileLocation = tileLocation;
			obj.isSpawnedObject.Value = true;
			if (!this.isTileOnMap(tileLocation) || this.map.RequireLayer("Back").PickTile(new Location((int)dropLocation.X, (int)dropLocation.Y), Game1.viewport.Size) == null || this.map.RequireLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y].TileIndexProperties.ContainsKey("Unplaceable"))
			{
				return false;
			}
			if (obj.bigCraftable.Value)
			{
				if (!this.isFarm.Value)
				{
					return false;
				}
				if (!obj.setOutdoors.Value && this.isOutdoors.Value)
				{
					return false;
				}
				if (!obj.setIndoors.Value && !this.isOutdoors.Value)
				{
					return false;
				}
				if (obj.performDropDownAction(who))
				{
					return false;
				}
			}
			else if (obj.Type == "Crafting" && obj.performDropDownAction(who))
			{
				obj.CanBeSetDown = false;
			}
			bool tilePassable = this.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), viewport) && this.CanItemBePlacedHere(tileLocation, false, CollisionMask.All, ~CollisionMask.Objects, false, false);
			if ((obj.CanBeSetDown || initialPlacement) && tilePassable && !this.isTileHoeDirt(tileLocation))
			{
				if (!this.objects.TryAdd(tileLocation, obj))
				{
					return false;
				}
			}
			else if (this.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(28, 300f, 2, 1, dropLocation, false, obj.flipped.Value)
				});
				this.playSound("dropItemInWater", null, null, SoundContext.Default);
			}
			else
			{
				if (obj.CanBeSetDown && !tilePassable)
				{
					return false;
				}
				if (obj.ParentSheetIndex >= 0 && obj.Type != null)
				{
					if (obj.Type == "Fish" || obj.Type == "Cooking" || obj.Type == "Crafting")
					{
						TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, dropLocation, true, obj.flipped.Value);
						sprite.CopyAppearanceFromItemId(obj.QualifiedItemId, 0);
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
						{
							sprite
						});
					}
					else
					{
						TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite(0, 150f, 1, 3, dropLocation, true, obj.flipped.Value);
						sprite2.CopyAppearanceFromItemId(obj.QualifiedItemId, 1);
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
						{
							sprite2
						});
					}
				}
			}
			return true;
		}

		// Token: 0x06000FA4 RID: 4004 RVA: 0x000B968A File Offset: 0x000B788A
		private void rumbleAndFade(int milliseconds)
		{
			this.rumbleAndFadeEvent.Fire(milliseconds);
		}

		// Token: 0x06000FA5 RID: 4005 RVA: 0x000B9698 File Offset: 0x000B7898
		private void performRumbleAndFade(int milliseconds)
		{
			if (Game1.currentLocation == this)
			{
				Rumble.rumbleAndFade(1f, (float)milliseconds);
			}
		}

		// Token: 0x06000FA6 RID: 4006 RVA: 0x000B96B0 File Offset: 0x000B78B0
		private void damagePlayers(Microsoft.Xna.Framework.Rectangle area, int damage, bool isBomb = false)
		{
			this.damagePlayersEvent.Fire(new GameLocation.DamagePlayersEventArg
			{
				Area = area,
				Damage = damage,
				IsBomb = isBomb
			});
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x000B96EC File Offset: 0x000B78EC
		private void performDamagePlayers(GameLocation.DamagePlayersEventArg arg)
		{
			if (Game1.player.currentLocation == this)
			{
				if (arg.IsBomb && Game1.player.hasBuff("dwarfStatue_3"))
				{
					return;
				}
				int damage = arg.Damage;
				if (Game1.player.stats.Get("Book_Bombs") > 0U)
				{
					damage = (int)((float)damage * 0.75f);
				}
				if (Game1.player.GetBoundingBox().Intersects(arg.Area) && !Game1.player.onBridge.Value)
				{
					Game1.player.takeDamage(damage, true, null);
				}
			}
		}

		// Token: 0x06000FA8 RID: 4008 RVA: 0x000B9780 File Offset: 0x000B7980
		public void explode(Vector2 tileLocation, int radius, Farmer who, bool damageFarmers = true, int damage_amount = -1, bool destroyObjects = true)
		{
			int insideCircle = 0;
			this.updateMap();
			Vector2 currentTile = new Vector2(Math.Min((float)(this.map.Layers[0].LayerWidth - 1), Math.Max(0f, tileLocation.X - (float)radius)), Math.Min((float)(this.map.Layers[0].LayerHeight - 1), Math.Max(0f, tileLocation.Y - (float)radius)));
			bool[,] circleOutline = Game1.getCircleOutlineGrid(radius);
			Microsoft.Xna.Framework.Rectangle areaOfEffect = new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - (float)radius) * 64, (int)(tileLocation.Y - (float)radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
			if (damage_amount > 0)
			{
				this.damageMonster(areaOfEffect, damage_amount, damage_amount, true, who, false);
			}
			else
			{
				this.damageMonster(areaOfEffect, radius * 6, radius * 8, true, who, false);
			}
			TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = new TemporaryAnimatedSpriteList();
			TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(23, 9999f, 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), false, Game1.random.NextBool());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 5);
			defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted("explode");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<float>(tileLocation.X);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<float>(tileLocation.Y);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
			temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
			temporaryAnimatedSprite.lightRadius = (float)radius;
			temporaryAnimatedSprite.lightcolor = Color.Black;
			temporaryAnimatedSprite.alphaFade = 0.03f - (float)radius * 0.003f;
			temporaryAnimatedSprite.Parent = this;
			temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite);
			TemporaryAnimatedSpriteList sprites = temporaryAnimatedSpriteList;
			this.rumbleAndFade(300 + radius * 100);
			if (damageFarmers)
			{
				int actualDamage = (damage_amount > 0) ? damage_amount : (radius * 3);
				this.damagePlayers(areaOfEffect, actualDamage, true);
			}
			for (int i = 0; i < radius * 2 + 1; i++)
			{
				for (int j = 0; j < radius * 2 + 1; j++)
				{
					if (i == 0 || j == 0 || i == radius * 2 || j == radius * 2)
					{
						insideCircle = ((circleOutline[i, j] > false) ? 1 : 0);
					}
					else if (circleOutline[i, j])
					{
						insideCircle += ((j <= radius) ? 1 : -1);
						if (insideCircle <= 0)
						{
							if (destroyObjects)
							{
								Object obj;
								if (this.objects.TryGetValue(currentTile, out obj) && obj.onExplosion(who))
								{
									this.destroyObject(currentTile, who);
								}
								TerrainFeature terrainFeature;
								if (this.terrainFeatures.TryGetValue(currentTile, out terrainFeature) && terrainFeature.performToolAction(null, radius / 2, currentTile))
								{
									this.terrainFeatures.Remove(currentTile);
								}
							}
							if (Game1.random.NextDouble() < 0.45)
							{
								if (Game1.random.NextBool())
								{
									sprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), false, Game1.random.NextBool())
									{
										delayBeforeAnimationStart = Game1.random.Next(700)
									});
								}
								else
								{
									sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
									{
										delayBeforeAnimationStart = Game1.random.Next(200),
										scale = (float)Game1.random.Next(5, 15) / 10f
									});
								}
							}
						}
					}
					if (insideCircle >= 1)
					{
						this.explosionAt(currentTile.X, currentTile.Y);
						if (destroyObjects)
						{
							Object obj2;
							if (this.objects.TryGetValue(currentTile, out obj2) && obj2.onExplosion(who))
							{
								this.destroyObject(currentTile, who);
							}
							TerrainFeature terrainFeature2;
							if (this.terrainFeatures.TryGetValue(currentTile, out terrainFeature2) && terrainFeature2.performToolAction(null, radius / 2, currentTile))
							{
								this.terrainFeatures.Remove(currentTile);
							}
						}
						if (Game1.random.NextDouble() < 0.45)
						{
							if (Game1.random.NextBool())
							{
								sprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), false, Game1.random.NextBool())
								{
									delayBeforeAnimationStart = Game1.random.Next(700)
								});
							}
							else
							{
								sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
								{
									delayBeforeAnimationStart = Game1.random.Next(200),
									scale = (float)Game1.random.Next(5, 15) / 10f
								});
							}
						}
						sprites.Add(new TemporaryAnimatedSprite(6, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, Game1.random.NextBool(), Vector2.Distance(currentTile, tileLocation) * 20f, 0, -1, -1f, -1, 0));
					}
					currentTile.Y += 1f;
					currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerHeight - 1), Math.Max(0f, currentTile.Y));
				}
				currentTile.X += 1f;
				currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerWidth - 1), Math.Max(0f, currentTile.X));
				currentTile.Y = tileLocation.Y - (float)radius;
				currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerHeight - 1), Math.Max(0f, currentTile.Y));
			}
			Game1.multiplayer.broadcastSprites(this, sprites);
			radius /= 2;
			circleOutline = Game1.getCircleOutlineGrid(radius);
			currentTile = new Vector2((float)((int)(tileLocation.X - (float)radius)), (float)((int)(tileLocation.Y - (float)radius)));
			insideCircle = 0;
			for (int k = 0; k < radius * 2 + 1; k++)
			{
				for (int l = 0; l < radius * 2 + 1; l++)
				{
					if (k == 0 || l == 0 || k == radius * 2 || l == radius * 2)
					{
						insideCircle = ((circleOutline[k, l] > false) ? 1 : 0);
					}
					else if (circleOutline[k, l])
					{
						insideCircle += ((l <= radius) ? 1 : -1);
						if (insideCircle <= 0 && !this.objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && !this.isTileHoeDirt(currentTile) && this.makeHoeDirt(currentTile, false))
						{
							this.checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, true, false, who);
						}
					}
					if (insideCircle >= 1 && !this.objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && !this.isTileHoeDirt(currentTile) && this.makeHoeDirt(currentTile, false))
					{
						this.checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, true, false, who);
					}
					currentTile.Y += 1f;
					currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerHeight - 1), Math.Max(0f, currentTile.Y));
				}
				currentTile.X += 1f;
				currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerWidth - 1), Math.Max(0f, currentTile.X));
				currentTile.Y = tileLocation.Y - (float)radius;
				currentTile.Y = Math.Min((float)(this.map.Layers[0].LayerHeight - 1), Math.Max(0f, currentTile.Y));
			}
		}

		// Token: 0x06000FA9 RID: 4009 RVA: 0x000B9FE0 File Offset: 0x000B81E0
		public virtual void explosionAt(float x, float y)
		{
		}

		// Token: 0x06000FAA RID: 4010 RVA: 0x000B9FE2 File Offset: 0x000B81E2
		public void removeTemporarySpritesWithID(int id)
		{
			this.removeTemporarySpritesWithIDEvent.Fire(id);
		}

		// Token: 0x06000FAB RID: 4011 RVA: 0x000B9FF0 File Offset: 0x000B81F0
		public void removeTemporarySpritesWithIDLocal(int id)
		{
			this.temporarySprites.RemoveWhere(delegate(TemporaryAnimatedSprite sprite)
			{
				if (sprite.id == id)
				{
					if (sprite.hasLit)
					{
						Utility.removeLightSource(sprite.lightId);
					}
					return true;
				}
				return false;
			});
		}

		// Token: 0x06000FAC RID: 4012 RVA: 0x000BA024 File Offset: 0x000B8224
		public bool makeHoeDirt(Vector2 tileLocation, bool ignoreChecks = false)
		{
			bool flag;
			if (ignoreChecks || (this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back", false) != null && !this.IsTileBlockedBy(tileLocation, ~(CollisionMask.Characters | CollisionMask.Farmers), CollisionMask.None, false)))
			{
				MineShaft mineShaft = this as MineShaft;
				flag = (mineShaft == null || mineShaft.getMineArea(-1) != 77377);
			}
			else
			{
				flag = false;
			}
			return flag && this.terrainFeatures.TryAdd(tileLocation, new HoeDirt((this.IsRainingHere() && this.isOutdoors.Value) ? 1 : 0, this));
		}

		// Token: 0x06000FAD RID: 4013 RVA: 0x000BA0B4 File Offset: 0x000B82B4
		public int numberOfObjectsOfType(string itemId, bool bigCraftable)
		{
			int number = 0;
			string type = bigCraftable ? "(BC)" : "(O)";
			foreach (Object obj in this.Objects.Values)
			{
				if (obj.HasTypeId(type) && obj.ItemId == itemId)
				{
					number++;
				}
			}
			return number;
		}

		// Token: 0x06000FAE RID: 4014 RVA: 0x000BA134 File Offset: 0x000B8334
		public virtual void timeUpdate(int timeElapsed)
		{
			if (Game1.IsMasterGame)
			{
				foreach (FarmAnimal farmAnimal in this.animals.Values)
				{
					farmAnimal.updatePerTenMinutes(Game1.timeOfDay, this);
				}
			}
			foreach (Building b in this.buildings)
			{
				if (b.daysOfConstructionLeft.Value <= 0)
				{
					b.performTenMinuteAction(timeElapsed);
					if (b.GetIndoorsType() == IndoorsType.Instanced)
					{
						GameLocation indoors = b.GetIndoors();
						if (indoors != null)
						{
							foreach (FarmAnimal farmAnimal2 in indoors.animals.Values)
							{
								farmAnimal2.updatePerTenMinutes(Game1.timeOfDay, indoors);
							}
							if (timeElapsed >= 10)
							{
								indoors.performTenMinuteUpdate(Game1.timeOfDay);
								if (timeElapsed > 10)
								{
									indoors.passTimeForObjects(timeElapsed - 10);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000FAF RID: 4015 RVA: 0x000BA278 File Offset: 0x000B8478
		public void passTimeForObjects(int timeElapsed)
		{
			this.objects.Lock();
			foreach (KeyValuePair<Vector2, Object> pair in this.objects.Pairs)
			{
				if (pair.Value.minutesElapsed(timeElapsed))
				{
					Vector2 key = pair.Key;
					this.objects.Remove(key);
				}
			}
			this.objects.Unlock();
		}

		// Token: 0x06000FB0 RID: 4016 RVA: 0x000BA300 File Offset: 0x000B8500
		public virtual void performTenMinuteUpdate(int timeOfDay)
		{
			for (int i = 0; i < this.furniture.Count; i++)
			{
				this.furniture[i].minutesElapsed(10);
			}
			for (int j = 0; j < this.characters.Count; j++)
			{
				NPC character = this.characters[j];
				if (!character.IsInvisible)
				{
					character.checkSchedule(timeOfDay);
					character.performTenMinuteUpdate(timeOfDay, this);
				}
			}
			this.passTimeForObjects(10);
			if (this.isOutdoors.Value)
			{
				Random r = Utility.CreateDaySaveRandom((double)timeOfDay, (double)this.Map.Layers[0].LayerWidth, 0.0);
				if (this.Equals(Game1.currentLocation))
				{
					this.tryToAddCritters(true);
				}
				if (Game1.IsMasterGame)
				{
					int splashPointDurationSoFar = Utility.CalculateMinutesBetweenTimes(this.fishSplashPointTime, Game1.timeOfDay);
					bool frenzy = this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("");
					if (this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextBool() && (!(this is Farm) || Game1.whichFarm == 1))
					{
						for (int tries = 0; tries < 2; tries++)
						{
							Point p = new Point(r.Next(0, this.map.RequireLayer("Back").LayerWidth), r.Next(0, this.map.RequireLayer("Back").LayerHeight));
							if (this.isOpenWater(p.X, p.Y) && this.doesTileHaveProperty(p.X, p.Y, "NoFishing", "Back", false) == null)
							{
								int toLand = FishingRod.distanceToLand(p.X, p.Y, this, false);
								if (toLand > 1 && toLand < 5)
								{
									if (Game1.player.currentLocation.Equals(this))
									{
										this.playSound("waterSlosh", null, null, SoundContext.Default);
									}
									if (r.NextDouble() < ((this is Beach) ? 0.008 : 0.01) && Game1.Date.TotalDays > 3 && (this is Town || this is Mountain || this is Forest || this is Beach) && Game1.timeOfDay < 2300 && (Game1.player.fishCaught.Count() > 2 || Game1.Date.TotalDays > 14) && !Utility.isFestivalDay())
									{
										Item f = this.getFish((float)r.Next(500), "", toLand, Game1.player, 0.0, Utility.PointToVector2(p), null);
										if (f.Category == -4 && !f.HasContextTag("fish_legendary"))
										{
											this.fishFrenzyFish.Value = f.QualifiedItemId;
											string locationName;
											if (!(this is Mountain))
											{
												if (!(this is Forest))
												{
													if (!(this is Town))
													{
														locationName = "beach";
													}
													else
													{
														locationName = "town";
													}
												}
												else
												{
													locationName = "forest";
												}
											}
											else
											{
												locationName = "mountain";
											}
											string tokenizedName = TokenStringBuilder.ItemNameFor(f, null);
											string tokenizedArticle = TokenStringBuilder.CapitalizeFirstLetter(TokenStringBuilder.ArticleFor(tokenizedName));
											Game1.multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:FishFrenzy_" + locationName, false, null, new string[]
											{
												tokenizedName,
												tokenizedArticle
											});
										}
									}
									this.fishSplashPointTime = Game1.timeOfDay;
									this.fishSplashPoint.Value = p;
									break;
								}
							}
						}
					}
					else if (!this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1 + (double)((float)splashPointDurationSoFar / 1800f) && splashPointDurationSoFar > (frenzy ? 120 : 60))
					{
						this.fishSplashPointTime = 0;
						this.fishFrenzyFish.Value = "";
						this.fishSplashPoint.Value = Point.Zero;
					}
					this.performOrePanTenMinuteUpdate(r);
				}
			}
			if (Game1.dayOfMonth % 7 == 0 && Game1.timeOfDay >= 1200 && Game1.timeOfDay <= 1500 && this.name.Equals("Saloon") && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
			{
				if (Game1.timeOfDay == 1500)
				{
					this.removeTemporarySpritesWithID(2400);
				}
				else
				{
					bool goodEvent = Game1.random.NextDouble() < 0.25;
					bool badEvent = Game1.random.NextDouble() < 0.25;
					List<NPC> sportsBoys = new List<NPC>();
					foreach (NPC k in this.characters)
					{
						if (k.TilePoint.Y < 12 && k.TilePoint.X > 26 && Game1.random.NextDouble() < ((goodEvent || badEvent) ? 0.66 : 0.25))
						{
							sportsBoys.Add(k);
						}
					}
					foreach (NPC l in sportsBoys)
					{
						l.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Saloon_" + (goodEvent ? "goodEvent" : (badEvent ? "badEvent" : "neutralEvent")) + "_" + Game1.random.Next(5).ToString()), null, 2, 3000, 0);
						if (goodEvent && Game1.random.NextDouble() < 0.55)
						{
							l.jump();
						}
					}
				}
			}
			if (Game1.currentLocation.Equals(this) && this.name.Equals("BugLand") && Game1.random.NextDouble() <= 0.2)
			{
				this.characters.Add(new Fly(this.getRandomTile(null) * 64f, true));
			}
		}

		// Token: 0x06000FB1 RID: 4017 RVA: 0x000BA984 File Offset: 0x000B8B84
		public virtual bool performOrePanTenMinuteUpdate(Random r)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && !(this is Beach) && this.orePanPoint.Value.Equals(Point.Zero) && r.NextBool())
			{
				for (int tries = 0; tries < 8; tries++)
				{
					Point p = new Point(r.Next(0, this.Map.RequireLayer("Back").LayerWidth), r.Next(0, this.Map.RequireLayer("Back").LayerHeight));
					if (this.isOpenWater(p.X, p.Y) && FishingRod.distanceToLand(p.X, p.Y, this, true) <= 1 && !this.hasTileAt(p, "Buildings", null))
					{
						if (Game1.player.currentLocation.Equals(this))
						{
							this.playSound("slosh", null, null, SoundContext.Default);
						}
						this.orePanPoint.Value = p;
						return true;
					}
				}
			}
			else if (!this.orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
			{
				this.orePanPoint.Value = Point.Zero;
			}
			return false;
		}

		// Token: 0x06000FB2 RID: 4018 RVA: 0x000BAAE4 File Offset: 0x000B8CE4
		public virtual IList<string> GetCrabPotFishForTile(Vector2 tile)
		{
			if (this.catchOceanCrabPotFishFromThisSpot((int)tile.X, (int)tile.Y))
			{
				return GameLocation.OceanCrabPotFishTypes;
			}
			string text;
			FishAreaData data;
			if (this.TryGetFishAreaForTile(tile, out text, out data))
			{
				List<string> crabPotFishTypes = data.CrabPotFishTypes;
				if (crabPotFishTypes != null && crabPotFishTypes.Count > 0)
				{
					return data.CrabPotFishTypes;
				}
			}
			return GameLocation.DefaultCrabPotFishTypes;
		}

		// Token: 0x06000FB3 RID: 4019 RVA: 0x000BAB40 File Offset: 0x000B8D40
		public virtual bool TryGetFishAreaForTile(Vector2 tile, out string id, out FishAreaData data)
		{
			LocationData locationData = this.GetData();
			if (((locationData != null) ? locationData.FishAreas : null) != null)
			{
				string defaultId = null;
				FishAreaData defaultArea = null;
				foreach (KeyValuePair<string, FishAreaData> pair in locationData.FishAreas)
				{
					FishAreaData area = pair.Value;
					Microsoft.Xna.Framework.Rectangle? rectangle;
					bool? flag = (area.Position != null) ? new bool?(rectangle.GetValueOrDefault().Contains((int)tile.X, (int)tile.Y)) : null;
					if (flag != null)
					{
						if (flag.GetValueOrDefault())
						{
							id = pair.Key;
							data = area;
							return true;
						}
					}
					else if (defaultId == null)
					{
						defaultId = pair.Key;
						defaultArea = pair.Value;
					}
				}
				if (defaultId != null)
				{
					id = defaultId;
					data = defaultArea;
					return true;
				}
			}
			id = null;
			data = null;
			return false;
		}

		// Token: 0x06000FB4 RID: 4020 RVA: 0x000BAC48 File Offset: 0x000B8E48
		public virtual string GetFishingAreaDisplayName(string id)
		{
			LocationData data = this.GetData();
			FishAreaData fishArea;
			if (((data != null) ? data.FishAreas : null) == null || !data.FishAreas.TryGetValue(id, out fishArea) || fishArea.DisplayName == null)
			{
				return null;
			}
			return TokenParser.ParseText(fishArea.DisplayName, null, null, null);
		}

		// Token: 0x06000FB5 RID: 4021 RVA: 0x000BAC94 File Offset: 0x000B8E94
		public virtual Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (locationName != null && locationName != this.Name && (!(locationName == "UndergroundMine") || !(this is MineShaft)))
			{
				GameLocation location = Game1.getLocationFromName(locationName);
				if (location != null && location != this)
				{
					return location.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, null);
				}
			}
			if (bobberTile != Vector2.Zero)
			{
				GameLocation currentLocation = who.currentLocation;
				if (((currentLocation != null) ? currentLocation.NameOrUniqueName : null) == this.NameOrUniqueName)
				{
					foreach (Building building in this.buildings)
					{
						FishPond pond = building as FishPond;
						if (pond != null && pond.isTileFishable(bobberTile))
						{
							return pond.CatchFish();
						}
					}
				}
			}
			if (this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("") && Vector2.Distance(bobberTile, Utility.PointToVector2(this.fishSplashPoint.Value)) <= 2f)
			{
				return ItemRegistry.Create(this.fishFrenzyFish.Value, 1, 0, false);
			}
			bool isTutorialCatch = who.fishCaught.Length == 0;
			return GameLocation.GetFishFromLocationData(this.Name, bobberTile, waterDepth, who, isTutorialCatch, false, this) ?? ItemRegistry.Create("(O)168", 1, 0, false);
		}

		// Token: 0x06000FB6 RID: 4022 RVA: 0x000BAE04 File Offset: 0x000B9004
		public static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location = null)
		{
			return GameLocation.GetFishFromLocationData(locationName, bobberTile, waterDepth, player, isTutorialCatch, isInherited, location, null);
		}

		// Token: 0x06000FB7 RID: 4023 RVA: 0x000BAE18 File Offset: 0x000B9018
		internal static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location, ItemQueryContext itemQueryContext)
		{
			if (location == null)
			{
				location = Game1.getLocationFromName(locationName);
			}
			LocationData locationData = (location != null) ? location.GetData() : GameLocation.GetData(locationName);
			Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
			Season season = Game1.GetSeasonForLocation(location);
			string fishAreaId;
			FishAreaData fishAreaData;
			if (location == null || !location.TryGetFishAreaForTile(bobberTile, out fishAreaId, out fishAreaData))
			{
				fishAreaId = null;
			}
			bool usingMagicBait = false;
			bool hasCuriosityLure = false;
			string baitTargetFish = null;
			bool usingGoodBait = false;
			FishingRod rod = ((player != null) ? player.CurrentTool : null) as FishingRod;
			if (rod != null && rod.isFishing)
			{
				usingMagicBait = rod.HasMagicBait();
				hasCuriosityLure = rod.HasCuriosityLure();
				Object bait = rod.GetBait();
				if (bait != null)
				{
					if (bait.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
					{
						baitTargetFish = "(O)" + bait.preservedParentSheetIndex.Value;
					}
					if (bait.QualifiedItemId != "(O)685")
					{
						usingGoodBait = true;
					}
				}
			}
			Point playerTile = player.TilePoint;
			if (itemQueryContext == null)
			{
				itemQueryContext = new ItemQueryContext(location, null, Game1.random, "location '" + locationName + "' > fish data");
			}
			IEnumerable<SpawnFishData> possibleFish = Game1.locationData["Default"].Fish;
			if (locationData != null)
			{
				List<SpawnFishData> fish2 = locationData.Fish;
				int? num = (fish2 != null) ? new int?(fish2.Count) : null;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					possibleFish = possibleFish.Concat(locationData.Fish);
				}
			}
			possibleFish = from p in possibleFish
			orderby p.Precedence, Game1.random.Next()
			select p;
			int targetedBaitTries = 0;
			HashSet<string> ignoreQueryKeys = usingMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null;
			Item firstNonTargetFish = null;
			Func<float, IList<QuantityModifier>, QuantityModifier.QuantityModifierMode, float> <>9__2;
			Func<string, string> <>9__3;
			for (int i = 0; i < 2; i++)
			{
				using (IEnumerator<SpawnFishData> enumerator = possibleFish.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SpawnFishData spawn = enumerator.Current;
						if ((!isInherited || spawn.CanBeInherited) && (spawn.FishAreaId == null || !(fishAreaId != spawn.FishAreaId)))
						{
							if (spawn.Season != null && !usingMagicBait)
							{
								Season? season2 = spawn.Season;
								Season season3 = season;
								if (!(season2.GetValueOrDefault() == season3 & season2 != null))
								{
									continue;
								}
							}
							Microsoft.Xna.Framework.Rectangle? rectangle = spawn.PlayerPosition;
							if (rectangle == null || rectangle.GetValueOrDefault().Contains(playerTile.X, playerTile.Y))
							{
								rectangle = spawn.BobberPosition;
								if ((rectangle == null || rectangle.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) && player.FishingLevel >= spawn.MinFishingLevel && waterDepth >= spawn.MinDistanceFromShore && (spawn.MaxDistanceFromShore <= -1 || waterDepth <= spawn.MaxDistanceFromShore) && (!spawn.RequireMagicBait || usingMagicBait))
								{
									SpawnFishData spawn3 = spawn;
									bool hasCuriosityLure2 = hasCuriosityLure;
									double dailyLuck = player.DailyLuck;
									int luckLevel = player.LuckLevel;
									Func<float, IList<QuantityModifier>, QuantityModifier.QuantityModifierMode, float> applyModifiers;
									if ((applyModifiers = <>9__2) == null)
									{
										applyModifiers = (<>9__2 = ((float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location, null, null, null, null)));
									}
									float chance = spawn3.GetChance(hasCuriosityLure2, dailyLuck, luckLevel, applyModifiers, spawn.ItemId == baitTargetFish);
									if (spawn.UseFishCaughtSeededRandom)
									{
										if (!Utility.CreateRandom(Game1.uniqueIDForThisGame, player.stats.Get("PreciseFishCaught") * 859U, 0.0, 0.0, 0.0).NextBool(chance))
										{
											continue;
										}
									}
									else if (!Game1.random.NextBool(chance))
									{
										continue;
									}
									if (spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
									{
										ISpawnItemData spawn2 = spawn;
										ItemQueryContext context = itemQueryContext;
										bool avoidRepeat = false;
										HashSet<string> avoidItemIds = null;
										Func<string, string> formatItemId;
										if ((formatItemId = <>9__3) == null)
										{
											formatItemId = (<>9__3 = ((string query) => query.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString())));
										}
										Item item = ItemQueryResolver.TryResolveRandomItem(spawn2, context, avoidRepeat, avoidItemIds, formatItemId, null, delegate(string query, string error)
										{
											IGameLogger log = Game1.log;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 4);
											defaultInterpolatedStringHandler.AppendLiteral("Location '");
											defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
											defaultInterpolatedStringHandler.AppendLiteral("' failed parsing item query '");
											defaultInterpolatedStringHandler.AppendFormatted(query);
											defaultInterpolatedStringHandler.AppendLiteral("' for fish '");
											defaultInterpolatedStringHandler.AppendFormatted(spawn.Id);
											defaultInterpolatedStringHandler.AppendLiteral("': ");
											defaultInterpolatedStringHandler.AppendFormatted(error);
											log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
										});
										if (item != null)
										{
											if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
											{
												item.SetFlagOnPickup = spawn.SetFlagOnCatch;
											}
											if (spawn.IsBossFish)
											{
												item.SetTempData<bool>("IsBossFish", true);
											}
											Item fish = item;
											int[] values;
											if ((spawn.CatchLimit <= -1 || !player.fishCaught.TryGetValue(fish.QualifiedItemId, out values) || values[0] < spawn.CatchLimit) && GameLocation.CheckGenericFishRequirements(fish, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, spawn.ItemId == baitTargetFish, isTutorialCatch))
											{
												if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
												{
													return fish;
												}
												if (firstNonTargetFish == null)
												{
													firstNonTargetFish = fish;
												}
												targetedBaitTries++;
											}
										}
									}
								}
							}
						}
					}
				}
				if (!usingGoodBait)
				{
					i++;
				}
			}
			if (firstNonTargetFish != null)
			{
				return firstNonTargetFish;
			}
			if (!isTutorialCatch)
			{
				return null;
			}
			return ItemRegistry.Create("(O)145", 1, 0, false);
		}

		// Token: 0x06000FB8 RID: 4024 RVA: 0x000BB4AC File Offset: 0x000B96AC
		internal static bool CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation location, Farmer player, SpawnFishData spawn, int waterDepth, bool usingMagicBait, bool hasCuriosityLure, bool usingTargetBait, bool isTutorialCatch)
		{
			GameLocation.<>c__DisplayClass503_0 CS$<>8__locals1;
			CS$<>8__locals1.fish = fish;
			string rawSpecificFishData;
			if (!CS$<>8__locals1.fish.HasTypeObject() || !allFishData.TryGetValue(CS$<>8__locals1.fish.ItemId, out rawSpecificFishData))
			{
				return !isTutorialCatch;
			}
			string[] specificFishData = rawSpecificFishData.Split('/', StringSplitOptions.None);
			if (ArgUtility.Get(specificFishData, 1, null, true) == "trap")
			{
				return !isTutorialCatch;
			}
			string a;
			if (player == null)
			{
				a = null;
			}
			else
			{
				Tool currentTool = player.CurrentTool;
				a = ((currentTool != null) ? currentTool.QualifiedItemId : null);
			}
			bool isTrainingRod = a == "(T)TrainingRod";
			if (isTrainingRod)
			{
				bool? canUseTrainingRod = spawn.CanUseTrainingRod;
				if (canUseTrainingRod != null)
				{
					if (!canUseTrainingRod.GetValueOrDefault())
					{
						return false;
					}
				}
				else
				{
					int difficulty;
					string error;
					if (!ArgUtility.TryGetInt(specificFishData, 1, out difficulty, out error, "int difficulty"))
					{
						return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error, ref CS$<>8__locals1);
					}
					if (difficulty >= 50)
					{
						return false;
					}
				}
			}
			if (isTutorialCatch)
			{
				bool isTutorialFish;
				string error2;
				if (!ArgUtility.TryGetOptionalBool(specificFishData, 13, out isTutorialFish, out error2, false, "bool isTutorialFish"))
				{
					return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error2, ref CS$<>8__locals1);
				}
				if (!isTutorialFish)
				{
					return false;
				}
			}
			if (!spawn.IgnoreFishDataRequirements)
			{
				if (!usingMagicBait)
				{
					string rawTimeSpans;
					string error3;
					if (!ArgUtility.TryGet(specificFishData, 5, out rawTimeSpans, out error3, true, "string rawTimeSpans"))
					{
						return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error3, ref CS$<>8__locals1);
					}
					string[] timeSpans = ArgUtility.SplitBySpace(rawTimeSpans);
					bool found = false;
					for (int i = 0; i < timeSpans.Length; i += 2)
					{
						int startTime;
						int endTime;
						if (!ArgUtility.TryGetInt(timeSpans, i, out startTime, out error3, "int startTime") || !ArgUtility.TryGetInt(timeSpans, i + 1, out endTime, out error3, "int endTime"))
						{
							return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0("invalid time spans '" + rawTimeSpans + "': " + error3, ref CS$<>8__locals1);
						}
						if (Game1.timeOfDay >= startTime && Game1.timeOfDay < endTime)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						return false;
					}
				}
				if (!usingMagicBait)
				{
					string weather;
					string error4;
					if (!ArgUtility.TryGet(specificFishData, 7, out weather, out error4, true, "string weather"))
					{
						return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error4, ref CS$<>8__locals1);
					}
					if (!(weather == "rainy"))
					{
						if (weather == "sunny")
						{
							if (location.IsRainingHere())
							{
								return false;
							}
						}
					}
					else if (!location.IsRainingHere())
					{
						return false;
					}
				}
				int minFishingLevel;
				string error5;
				if (!ArgUtility.TryGetInt(specificFishData, 12, out minFishingLevel, out error5, "int minFishingLevel"))
				{
					return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error5, ref CS$<>8__locals1);
				}
				if (player.FishingLevel < minFishingLevel)
				{
					return false;
				}
				int maxDepth;
				string error6;
				float chance;
				float depthMultiplier;
				if (!ArgUtility.TryGetInt(specificFishData, 9, out maxDepth, out error6, "int maxDepth") || !ArgUtility.TryGetFloat(specificFishData, 10, out chance, out error6, "float chance") || !ArgUtility.TryGetFloat(specificFishData, 11, out depthMultiplier, out error6, "float depthMultiplier"))
				{
					return GameLocation.<CheckGenericFishRequirements>g__LogFormatError|503_0(error6, ref CS$<>8__locals1);
				}
				float dropOffAmount = depthMultiplier * chance;
				chance -= (float)Math.Max(0, maxDepth - waterDepth) * dropOffAmount;
				chance += (float)player.FishingLevel / 50f;
				if (isTrainingRod)
				{
					chance *= 1.1f;
				}
				chance = Math.Min(chance, 0.9f);
				if ((double)chance < 0.25 && hasCuriosityLure)
				{
					if (spawn.CuriosityLureBuff > -1f)
					{
						chance += spawn.CuriosityLureBuff;
					}
					else
					{
						float max = 0.25f;
						float min = 0.08f;
						chance = (max - min) / max * chance + (max - min) / 2f;
					}
				}
				if (usingTargetBait)
				{
					chance *= 1.66f;
				}
				if (spawn.ApplyDailyLuck)
				{
					chance += (float)player.DailyLuck;
				}
				List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
				if (chanceModifiers != null && chanceModifiers.Count > 0)
				{
					chance = Utility.ApplyQuantityModifiers(chance, spawn.ChanceModifiers, spawn.ChanceModifierMode, location, null, null, null, null);
				}
				if (!Game1.random.NextBool(chance))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000FB9 RID: 4025 RVA: 0x000BB814 File Offset: 0x000B9A14
		public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.isActionableTile(xTile, yTile, who))
					{
						return true;
					}
				}
			}
			bool isActionable = false;
			string[] action = ArgUtility.SplitBySpace(this.doesTileHaveProperty(xTile, yTile, "Action", "Buildings", false));
			if (!this.ShouldIgnoreAction(action, who, new Location(xTile, yTile)))
			{
				string a = action[0];
				if (!(a == "Dialogue") && !(a == "Message") && !(a == "MessageOnce") && !(a == "NPCMessage"))
				{
					if (!(a == "MessageSpeech"))
					{
						isActionable = true;
					}
					else
					{
						isActionable = true;
						Game1.isSpeechAtCurrentCursorTile = true;
					}
				}
				else
				{
					isActionable = true;
					Game1.isInspectionAtCurrentCursorTile = true;
				}
			}
			if (!isActionable)
			{
				Object obj;
				if (this.objects.TryGetValue(new Vector2((float)xTile, (float)yTile), out obj) && obj.isActionable(who))
				{
					isActionable = true;
				}
				TerrainFeature terrainFeature;
				if (!Game1.isFestival() && this.terrainFeatures.TryGetValue(new Vector2((float)xTile, (float)yTile), out terrainFeature) && terrainFeature.isActionable())
				{
					isActionable = true;
				}
			}
			if (isActionable && !Utility.tileWithinRadiusOfPlayer(xTile, yTile, 1, who))
			{
				Game1.mouseCursorTransparency = 0.5f;
			}
			return isActionable;
		}

		// Token: 0x06000FBA RID: 4026 RVA: 0x000BB96C File Offset: 0x000B9B6C
		public Item tryGetRandomArtifactFromThisLocation(Farmer who, Random r, double chanceMultipler = 1.0)
		{
			LocationData locationData = this.GetData();
			ItemQueryContext itemQueryContext = new ItemQueryContext(this, who, r, "location '" + this.NameOrUniqueName + "' > artifact spots");
			IEnumerable<ArtifactSpotDropData> possibleDrops = Game1.locationData["Default"].ArtifactSpots;
			if (locationData != null)
			{
				List<ArtifactSpotDropData> artifactSpots = locationData.ArtifactSpots;
				int? num = (artifactSpots != null) ? new int?(artifactSpots.Count) : null;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					possibleDrops = possibleDrops.Concat(locationData.ArtifactSpots);
				}
			}
			possibleDrops = from p in possibleDrops
			orderby p.Precedence
			select p;
			using (IEnumerator<ArtifactSpotDropData> enumerator = possibleDrops.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ArtifactSpotDropData drop = enumerator.Current;
					if (r.NextBool(drop.Chance * chanceMultipler) && (drop.Condition == null || GameStateQuery.CheckConditions(drop.Condition, this, who, null, null, r, null)))
					{
						Item item = ItemQueryResolver.TryResolveRandomItem(drop, itemQueryContext, false, null, null, null, delegate(string query, string error)
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Location '");
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral("' failed parsing item query '");
							defaultInterpolatedStringHandler.AppendFormatted(query);
							defaultInterpolatedStringHandler.AppendLiteral("' for artifact spot '");
							defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
							defaultInterpolatedStringHandler.AppendLiteral("': ");
							defaultInterpolatedStringHandler.AppendFormatted(error);
							log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						});
						if (item != null)
						{
							return item;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x000BBAE4 File Offset: 0x000B9CE4
		public virtual void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)yLocation, (double)(Game1.netWorldState.Value.TreasureTotemsUsed * 777));
			Vector2 tilePixelPos = new Vector2((float)(xLocation * 64), (float)(yLocation * 64));
			Hoe hoe = ((who != null) ? who.CurrentTool : null) as Hoe;
			bool hasGenerousEnchantment = hoe != null && hoe.hasEnchantmentOfType<GenerousEnchantment>();
			LocationData locationData = this.GetData();
			ItemQueryContext itemQueryContext = new ItemQueryContext(this, who, r, "location '" + this.NameOrUniqueName + "' > artifact spots");
			IEnumerable<ArtifactSpotDropData> possibleDrops = Game1.locationData["Default"].ArtifactSpots;
			if (locationData != null)
			{
				List<ArtifactSpotDropData> artifactSpots = locationData.ArtifactSpots;
				int? num = (artifactSpots != null) ? new int?(artifactSpots.Count) : null;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					possibleDrops = possibleDrops.Concat(locationData.ArtifactSpots);
				}
			}
			possibleDrops = from p in possibleDrops
			orderby p.Precedence
			select p;
			if (Game1.player.mailReceived.Contains("sawQiPlane") && r.NextDouble() < 0.05 + Game1.player.team.AverageDailyLuck(null) / 2.0)
			{
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MysteryBox", r.Next(1, 3), 0, false), tilePixelPos, -1, this, -1, false);
			}
			Utility.trySpawnRareObject(who, tilePixelPos, this, 9.0, 1.0, -1, r);
			using (IEnumerator<ArtifactSpotDropData> enumerator = possibleDrops.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ArtifactSpotDropData drop = enumerator.Current;
					if (r.NextBool(drop.Chance) && (drop.Condition == null || GameStateQuery.CheckConditions(drop.Condition, this, who, null, null, r, null)))
					{
						Item item = ItemQueryResolver.TryResolveRandomItem(drop, itemQueryContext, false, null, null, null, delegate(string query, string error)
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Location '");
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral("' failed parsing item query '");
							defaultInterpolatedStringHandler.AppendFormatted(query);
							defaultInterpolatedStringHandler.AppendLiteral("' for artifact spot '");
							defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
							defaultInterpolatedStringHandler.AppendLiteral("': ");
							defaultInterpolatedStringHandler.AppendFormatted(error);
							log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						});
						if (item != null)
						{
							if (drop.OneDebrisPerDrop && item.Stack > 1)
							{
								Game1.createMultipleItemDebris(item, tilePixelPos, -1, this, -1, false);
							}
							else
							{
								Game1.createItemDebris(item, tilePixelPos, Game1.random.Next(4), this, -1, false);
							}
							if (hasGenerousEnchantment && drop.ApplyGenerousEnchantment && r.NextBool())
							{
								item = item.getOne();
								item = (Item)ItemQueryResolver.ApplyItemFields(item, drop, itemQueryContext, null);
								if (drop.OneDebrisPerDrop && item.Stack > 1)
								{
									Game1.createMultipleItemDebris(item, tilePixelPos, -1, this, -1, false);
								}
								else
								{
									Game1.createItemDebris(item, tilePixelPos, -1, this, -1, false);
								}
							}
							if (!drop.ContinueOnDrop)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000FBC RID: 4028 RVA: 0x000BBDF8 File Offset: 0x000B9FF8
		public LocationData GetData()
		{
			string name = this.Name;
			if (!(this is MineShaft))
			{
				if (this is Cellar)
				{
					if (name.StartsWith("Cellar"))
					{
						name = "Cellar";
					}
				}
			}
			else
			{
				name = "UndergroundMine";
			}
			return GameLocation.GetData(name);
		}

		// Token: 0x06000FBD RID: 4029 RVA: 0x000BBE44 File Offset: 0x000BA044
		public static LocationData GetData(string name)
		{
			GameLocation.<>c__DisplayClass508_0 CS$<>8__locals1;
			CS$<>8__locals1.rawData = Game1.locationData;
			if (name == "Farm")
			{
				return GameLocation.<GetData>g__GetImpl|508_0("Farm_" + Game1.GetFarmTypeKey(), ref CS$<>8__locals1) ?? GameLocation.<GetData>g__GetImpl|508_0("Farm_Standard", ref CS$<>8__locals1);
			}
			return GameLocation.<GetData>g__GetImpl|508_0(name, ref CS$<>8__locals1);
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x000BBE99 File Offset: 0x000BA099
		public virtual bool ShouldExcludeFromNpcPathfinding()
		{
			LocationData data = this.GetData();
			return data != null && data.ExcludeFromNpcPathfinding;
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x000BBEAC File Offset: 0x000BA0AC
		public virtual string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			Random r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)(yLocation * 77), Game1.stats.DirtHoed);
			string treasureType = this.HandleTreasureTileProperty(xLocation, yLocation, detectOnly);
			if (treasureType != null)
			{
				return treasureType;
			}
			bool generousEnchant = ((who != null) ? who.CurrentTool : null) is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
			float generousChance = 0.5f;
			if (!this.isFarm.Value && this.isOutdoors.Value && this.GetSeason() == Season.Winter && r.NextDouble() < 0.08 && !explosion && !detectOnly && !(this is Desert))
			{
				Game1.createObjectDebris(r.Choose("(O)412", "(O)416"), xLocation, yLocation, -1, 0, 1f, null);
				if (generousEnchant && r.NextDouble() < (double)generousChance)
				{
					Game1.createObjectDebris(r.Choose("(O)412", "(O)416"), xLocation, yLocation, -1, 0, 1f, null);
				}
				return "";
			}
			LocationData data = this.GetData();
			if (!this.isOutdoors.Value || !r.NextBool((data != null) ? data.ChanceForClay : 0.03) || explosion)
			{
				return "";
			}
			if (detectOnly)
			{
				this.map.RequireLayer("Back").Tiles[xLocation, yLocation].Properties.Add("Treasure", "Item (O)330");
				return "Item";
			}
			Game1.createObjectDebris("(O)330", xLocation, yLocation, -1, 0, 1f, null);
			if (generousEnchant && r.NextDouble() < (double)generousChance)
			{
				Game1.createObjectDebris("(O)330", xLocation, yLocation, -1, 0, 1f, null);
			}
			return "";
		}

		// Token: 0x06000FC0 RID: 4032 RVA: 0x000BC064 File Offset: 0x000BA264
		private string HandleTreasureTileProperty(int xLocation, int yLocation, bool detectOnly)
		{
			GameLocation.<>c__DisplayClass511_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.xLocation = xLocation;
			CS$<>8__locals1.yLocation = yLocation;
			string property = this.doesTileHaveProperty(CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, "Treasure", "Back", false);
			if (property == null)
			{
				return null;
			}
			string[] fields = ArgUtility.SplitBySpace(property);
			string type;
			string error;
			if (!ArgUtility.TryGet(fields, 0, out type, out error, true, "string type"))
			{
				this.<HandleTreasureTileProperty>g__LogError|511_0(property, error, ref CS$<>8__locals1);
				return null;
			}
			if (detectOnly)
			{
				return type;
			}
			if (type != null)
			{
				switch (type.Length)
				{
				case 4:
				{
					char c = type[2];
					switch (c)
					{
					case 'a':
						if (!(type == "Coal"))
						{
							goto IL_3C5;
						}
						break;
					case 'b':
					case 'd':
						goto IL_3C5;
					case 'c':
					{
						if (!(type == "Arch"))
						{
							goto IL_3C5;
						}
						string itemId;
						if (ArgUtility.TryGet(fields, 1, out itemId, out error, true, "string itemId"))
						{
							Game1.createObjectDebris(itemId, CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, -1, 0, 1f, null);
							goto IL_3E0;
						}
						this.<HandleTreasureTileProperty>g__LogError|511_0(property, error, ref CS$<>8__locals1);
						goto IL_3E0;
					}
					case 'e':
					{
						if (!(type == "Item"))
						{
							goto IL_3C5;
						}
						string itemId2;
						if (!ArgUtility.TryGet(fields, 1, out itemId2, out error, true, "string itemId"))
						{
							this.<HandleTreasureTileProperty>g__LogError|511_0(property, error, ref CS$<>8__locals1);
							goto IL_3E0;
						}
						Item item = ItemRegistry.Create(itemId2, 1, 0, false);
						Game1.createItemDebris(item, new Vector2((float)CS$<>8__locals1.xLocation, (float)CS$<>8__locals1.yLocation), -1, this, -1, false);
						if (item.QualifiedItemId == "(O)78")
						{
							Stats stats = Game1.stats;
							uint caveCarrotsFound = stats.CaveCarrotsFound;
							stats.CaveCarrotsFound = caveCarrotsFound + 1U;
							goto IL_3E0;
						}
						goto IL_3E0;
					}
					default:
						if (c != 'l')
						{
							if (c != 'o')
							{
								goto IL_3C5;
							}
							if (!(type == "Iron"))
							{
								goto IL_3C5;
							}
						}
						else if (!(type == "Gold"))
						{
							goto IL_3C5;
						}
						break;
					}
					break;
				}
				case 5:
					if (!(type == "Coins"))
					{
						goto IL_3C5;
					}
					Game1.createObjectDebris("(O)330", CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, -1, 0, 1f, null);
					goto IL_3E0;
				case 6:
				{
					char c = type[0];
					if (c != 'C')
					{
						if (c != 'O')
						{
							goto IL_3C5;
						}
						if (!(type == "Object"))
						{
							goto IL_3C5;
						}
						string itemId3;
						if (!ArgUtility.TryGet(fields, 1, out itemId3, out error, true, "string itemId"))
						{
							this.<HandleTreasureTileProperty>g__LogError|511_0(property, error, ref CS$<>8__locals1);
							goto IL_3E0;
						}
						Game1.createObjectDebris(itemId3, CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, -1, 0, 1f, null);
						if (itemId3 == "78" || itemId3 == "(O)79")
						{
							Stats stats2 = Game1.stats;
							uint caveCarrotsFound = stats2.CaveCarrotsFound;
							stats2.CaveCarrotsFound = caveCarrotsFound + 1U;
							goto IL_3E0;
						}
						goto IL_3E0;
					}
					else if (!(type == "Copper"))
					{
						goto IL_3C5;
					}
					break;
				}
				case 7:
					if (!(type == "Iridium"))
					{
						goto IL_3C5;
					}
					break;
				case 8:
				case 9:
					goto IL_3C5;
				case 10:
					if (!(type == "CaveCarrot"))
					{
						goto IL_3C5;
					}
					Game1.createObjectDebris("(O)78", CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, -1, 0, 1f, null);
					goto IL_3E0;
				default:
					goto IL_3C5;
				}
				int debris;
				if (!(type == "Coal"))
				{
					if (!(type == "Copper"))
					{
						if (!(type == "Gold"))
						{
							if (!(type == "Iridium"))
							{
								debris = 2;
							}
							else
							{
								debris = 10;
							}
						}
						else
						{
							debris = 6;
						}
					}
					else
					{
						debris = 0;
					}
				}
				else
				{
					debris = 4;
				}
				int itemId4;
				if (ArgUtility.TryGetInt(fields, 1, out itemId4, out error, "int itemId"))
				{
					Game1.createDebris(debris, CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation, itemId4);
					goto IL_3E0;
				}
				this.<HandleTreasureTileProperty>g__LogError|511_0(property, error, ref CS$<>8__locals1);
				goto IL_3E0;
			}
			IL_3C5:
			type = null;
			this.<HandleTreasureTileProperty>g__LogError|511_0(property, "invalid treasure type '" + type + "'", ref CS$<>8__locals1);
			IL_3E0:
			this.map.RequireLayer("Back").Tiles[CS$<>8__locals1.xLocation, CS$<>8__locals1.yLocation].Properties["Treasure"] = null;
			return type;
		}

		// Token: 0x06000FC1 RID: 4033 RVA: 0x000BC488 File Offset: 0x000BA688
		public virtual bool AllowMapModificationsInResetState()
		{
			return false;
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x000BC48C File Offset: 0x000BA68C
		public void removeMapTile(int tileX, int tileY, string layer)
		{
			Map map = this.map;
			Layer mapLayer = (map != null) ? map.RequireLayer(layer) : null;
			if (((mapLayer != null) ? mapLayer.Tiles[tileX, tileY] : null) != null)
			{
				mapLayer.Tiles[tileX, tileY] = null;
			}
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x000BC4D0 File Offset: 0x000BA6D0
		public StaticTile setMapTile(int tileX, int tileY, int index, string layer, string tileSheetId, string action = null, bool copyProperties = true)
		{
			Layer mapLayer = this.map.RequireLayer(layer);
			Tile oldTile = mapLayer.Tiles[tileX, tileY];
			StaticTile tile = oldTile as StaticTile;
			if (tile != null && tile.TileSheet.Id == tileSheetId)
			{
				tile.TileIndex = index;
			}
			else
			{
				tile = (mapLayer.Tiles[tileX, tileY] = new StaticTile(mapLayer, this.map.RequireTileSheet(tileSheetId), BlendMode.Alpha, index));
				if (copyProperties && oldTile != null)
				{
					foreach (KeyValuePair<string, PropertyValue> property in oldTile.Properties)
					{
						tile.Properties[property.Key] = property.Value;
					}
				}
			}
			if (action != null && layer == "Buildings")
			{
				tile.Properties["Action"] = action;
			}
			return tile;
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x000BC5C8 File Offset: 0x000BA7C8
		public AnimatedTile setAnimatedMapTile(int tileX, int tileY, int[] animationTileIndexes, long interval, string layer, string tileSheetId, string action = null, bool copyProperties = true)
		{
			Layer mapLayer = this.map.RequireLayer(layer);
			TileSheet tileSheet = this.map.RequireTileSheet(tileSheetId);
			StaticTile[] tiles = new StaticTile[animationTileIndexes.Length];
			for (int i = 0; i < animationTileIndexes.Length; i++)
			{
				tiles[i] = new StaticTile(mapLayer, tileSheet, BlendMode.Alpha, animationTileIndexes[i]);
			}
			AnimatedTile tile = new AnimatedTile(mapLayer, tiles, interval);
			if (copyProperties)
			{
				Tile oldTile = mapLayer.Tiles[tileX, tileY];
				if (oldTile != null)
				{
					foreach (KeyValuePair<string, PropertyValue> property in oldTile.Properties)
					{
						tile.Properties[property.Key] = property.Value;
					}
				}
			}
			if (action != null && layer == "Buildings")
			{
				tile.Properties["Action"] = action;
			}
			mapLayer.Tiles[tileX, tileY] = tile;
			return tile;
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x000BC6D0 File Offset: 0x000BA8D0
		public virtual void shiftContents(int dx, int dy, Func<Vector2, object, bool> where = null)
		{
			Vector2 offset = new Vector2((float)dx, (float)dy);
			List<KeyValuePair<Vector2, Object>> list = new List<KeyValuePair<Vector2, Object>>(this.objects.Pairs);
			this.objects.Clear();
			foreach (KeyValuePair<Vector2, Object> v in list)
			{
				if (where == null || where(v.Key, v.Value))
				{
					LightSource lightSource = v.Value.lightSource;
					this.removeLightSource((lightSource != null) ? lightSource.Id : null);
					Vector2 tile = v.Key + offset;
					this.objects.Add(tile, v.Value);
					v.Value.initializeLightSource(tile, false);
				}
				else
				{
					this.objects.Add(v.Key, v.Value);
				}
			}
			List<KeyValuePair<Vector2, TerrainFeature>> list2 = new List<KeyValuePair<Vector2, TerrainFeature>>(this.terrainFeatures.Pairs);
			this.terrainFeatures.Clear();
			foreach (KeyValuePair<Vector2, TerrainFeature> v2 in list2)
			{
				Vector2 tile2 = (where == null || where(v2.Key, v2.Value)) ? (v2.Key + offset) : v2.Key;
				this.terrainFeatures.Add(tile2, v2.Value);
			}
			foreach (LargeTerrainFeature v3 in this.largeTerrainFeatures)
			{
				if (where == null || where(v3.Tile, v3))
				{
					v3.Tile += offset;
				}
			}
			foreach (Furniture v4 in this.furniture)
			{
				if (where == null || where(v4.TileLocation, v4))
				{
					v4.removeLights();
					v4.TileLocation = new Vector2(v4.TileLocation.X + (float)dx, v4.TileLocation.Y + (float)dy);
					v4.updateDrawPosition();
					if (Game1.isDarkOut(this))
					{
						v4.addLights();
					}
				}
			}
		}

		// Token: 0x06000FC6 RID: 4038 RVA: 0x000BC968 File Offset: 0x000BAB68
		public void moveFurniture(int oldX, int oldY, int newX, int newY)
		{
			Vector2 oldSpot = new Vector2((float)oldX, (float)oldY);
			foreach (Furniture f in this.furniture)
			{
				if (f.tileLocation.Equals(oldSpot))
				{
					f.removeLights();
					f.TileLocation = new Vector2((float)newX, (float)newY);
					if (Game1.isDarkOut(this))
					{
						f.addLights();
					}
					return;
				}
			}
			Object o;
			if (this.objects.TryGetValue(oldSpot, out o))
			{
				this.objects.Remove(oldSpot);
				this.objects.Add(new Vector2((float)newX, (float)newY), o);
			}
		}

		// Token: 0x06000FC7 RID: 4039 RVA: 0x000BCA2C File Offset: 0x000BAC2C
		public bool hasTileAt(int x, int y, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			return map != null && map.HasTileAt(x, y, layer, tilesheetId);
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x000BCA44 File Offset: 0x000BAC44
		public bool hasTileAt(Location tile, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			return map != null && map.HasTileAt(tile.X, tile.Y, layer, tilesheetId);
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x000BCA65 File Offset: 0x000BAC65
		public bool hasTileAt(Point tile, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			return map != null && map.HasTileAt(tile.X, tile.Y, layer, tilesheetId);
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x000BCA86 File Offset: 0x000BAC86
		public int getTileIndexAt(Location p, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			if (map == null)
			{
				return -1;
			}
			return map.GetTileIndexAt(p.X, p.Y, layer, tilesheetId);
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x000BCAA7 File Offset: 0x000BACA7
		public int getTileIndexAt(Point p, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			if (map == null)
			{
				return -1;
			}
			return map.GetTileIndexAt(p.X, p.Y, layer, tilesheetId);
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x000BCAC8 File Offset: 0x000BACC8
		public int getTileIndexAt(int x, int y, string layer, string tilesheetId = null)
		{
			Map map = this.map;
			if (map == null)
			{
				return -1;
			}
			return map.GetTileIndexAt(x, y, layer, tilesheetId);
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x000BCAE0 File Offset: 0x000BACE0
		public string getTileSheetIDAt(int x, int y, string layer)
		{
			Layer layer2 = this.map.GetLayer(layer);
			string text;
			if (layer2 == null)
			{
				text = null;
			}
			else
			{
				Tile tile = layer2.Tiles[x, y];
				text = ((tile != null) ? tile.TileSheet.Id : null);
			}
			return text ?? "";
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x000BCB1B File Offset: 0x000BAD1B
		public virtual void OnBuildingConstructed(Building building, Farmer who)
		{
			building.performActionOnConstruction(this, who);
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x000BCB25 File Offset: 0x000BAD25
		public virtual void OnBuildingMoved(Building building)
		{
			building.performActionOnBuildingPlacement();
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x000BCB2D File Offset: 0x000BAD2D
		public virtual void OnBuildingDemolished(string type, Guid id)
		{
			if (type == "Stable")
			{
				Horse mount = Game1.player.mount;
				if (mount != null && mount.HorseId == id)
				{
					Game1.player.mount.dismount(true);
				}
			}
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x000BCB6A File Offset: 0x000BAD6A
		public virtual void OnDayStarted()
		{
		}

		// Token: 0x06000FD2 RID: 4050 RVA: 0x000BCB6C File Offset: 0x000BAD6C
		public void OnStoneDestroyed(string stoneId, int x, int y, Farmer who)
		{
			long farmerId = (who != null) ? who.UniqueMultiplayerID : 0L;
			MineShaft mine = ((who != null) ? who.currentLocation : null) as MineShaft;
			if (mine != null && mine.mineLevel > 120 && !mine.isSideBranch(-1))
			{
				int floor = mine.mineLevel - 121;
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
				{
					float chance = 0.01f;
					chance += (float)floor * 0.0005f;
					if (chance > 0.5f)
					{
						chance = 0.5f;
					}
					if (Game1.random.NextBool(chance))
					{
						Game1.createMultipleObjectDebris("CalicoEgg", x, y, Game1.random.Next(1, 4), who.UniqueMultiplayerID, this);
					}
				}
			}
			if (who != null && Game1.random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
			{
				Game1.createMultipleObjectDebris("(O)890", x, y, 1, who.UniqueMultiplayerID, this);
			}
			if (!MineShaft.IsGeneratedLevel(this))
			{
				if (stoneId == "343" || stoneId == "450")
				{
					Random r = Utility.CreateDaySaveRandom((double)(x * 2000), (double)y, 0.0);
					double geodeChanceMultiplier = (who != null && who.hasBuff("dwarfStatue_4")) ? 1.25 : 1.0;
					if (r.NextDouble() < 0.035 * geodeChanceMultiplier && Game1.stats.DaysPlayed > 1U)
					{
						Game1.createObjectDebris("(O)" + (535 + ((Game1.stats.DaysPlayed > 60U && r.NextDouble() < 0.2) ? 1 : ((Game1.stats.DaysPlayed > 120U && r.NextDouble() < 0.2) ? 2 : 0))).ToString(), x, y, farmerId, this);
					}
					int burrowerMultiplier = (who != null && who.professions.Contains(21)) ? 2 : 1;
					double addedCoalChance = (who != null && who.hasBuff("dwarfStatue_2")) ? 0.03 : 0.0;
					if (r.NextDouble() < 0.035 * (double)burrowerMultiplier + addedCoalChance && Game1.stats.DaysPlayed > 1U)
					{
						Game1.createObjectDebris("(O)382", x, y, farmerId, this);
					}
					if (r.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1U)
					{
						Game1.createObjectDebris("(O)390", x, y, farmerId, this);
					}
				}
				this.breakStone(stoneId, x, y, who, Utility.CreateDaySaveRandom((double)(x * 4000), (double)y, 0.0));
				return;
			}
			(this as MineShaft).checkStoneForItems(stoneId, x, y, who);
		}

		// Token: 0x06000FD3 RID: 4051 RVA: 0x000BCE28 File Offset: 0x000BB028
		protected virtual bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
		{
			int experience = 0;
			int addedOres = (who != null && who.professions.Contains(18)) ? 1 : 0;
			if (who != null && who.hasBuff("dwarfStatue_0"))
			{
				addedOres++;
			}
			if (stoneId == 44.ToString())
			{
				stoneId = (r.Next(1, 8) * 2).ToString();
			}
			long farmerId = (who != null) ? who.UniqueMultiplayerID : 0L;
			int farmerLuckLevel = (who != null) ? who.LuckLevel : 0;
			double farmerDailyLuck = (who != null) ? who.DailyLuck : 0.0;
			int farmerMiningLevel = (who != null) ? who.MiningLevel : 0;
			if (stoneId != null)
			{
				int length = stoneId.Length;
				switch (length)
				{
				case 1:
					switch (stoneId[0])
					{
					case '2':
						Game1.createMultipleObjectDebris("(O)72", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 150;
						goto IL_BBF;
					case '3':
					case '5':
					case '7':
						goto IL_BBF;
					case '4':
						Game1.createMultipleObjectDebris("(O)64", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 80;
						goto IL_BBF;
					case '6':
						Game1.createMultipleObjectDebris("(O)70", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 40;
						goto IL_BBF;
					case '8':
						Game1.createMultipleObjectDebris("(O)66", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 16;
						goto IL_BBF;
					default:
						goto IL_BBF;
					}
					break;
				case 2:
					switch (stoneId[1])
					{
					case '0':
						if (!(stoneId == "10"))
						{
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("(O)68", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 16;
						goto IL_BBF;
					case '1':
					case '3':
						goto IL_BBF;
					case '2':
						if (!(stoneId == "12"))
						{
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("(O)60", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 80;
						goto IL_BBF;
					case '4':
						if (!(stoneId == "14"))
						{
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("(O)62", x, y, (who != null && who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1, farmerId, this);
						experience = 40;
						goto IL_BBF;
					case '5':
						if (stoneId == "95")
						{
							Game1.createMultipleObjectDebris("(O)909", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 200f)) ? 1 : 0), farmerId, this);
							experience = 18;
							goto IL_BBF;
						}
						if (!(stoneId == "25"))
						{
							if (!(stoneId == "75"))
							{
								goto IL_BBF;
							}
							Game1.createObjectDebris("(O)535", x, y, farmerId, this);
							experience = 8;
							goto IL_BBF;
						}
						else
						{
							Game1.createMultipleObjectDebris("(O)719", x, y, r.Next(2, 5), farmerId, this);
							experience = 5;
							if (this is IslandLocation && r.NextDouble() < 0.1)
							{
								Game1.player.team.RequestLimitedNutDrops("MusselStone", this, x * 64, y * 64, 5, 1);
								goto IL_BBF;
							}
							goto IL_BBF;
						}
						break;
					case '6':
						if (!(stoneId == "76"))
						{
							goto IL_BBF;
						}
						Game1.createObjectDebris("(O)536", x, y, farmerId, this);
						experience = 16;
						goto IL_BBF;
					case '7':
						if (!(stoneId == "77"))
						{
							goto IL_BBF;
						}
						Game1.createObjectDebris("(O)537", x, y, farmerId, this);
						experience = 32;
						goto IL_BBF;
					default:
						goto IL_BBF;
					}
					break;
				case 3:
					switch (stoneId[2])
					{
					case '0':
						if (stoneId == "670")
						{
							goto IL_805;
						}
						if (!(stoneId == "850") && !(stoneId == "290"))
						{
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("(O)380", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
						experience = 12;
						Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100, ""));
						goto IL_BBF;
					case '1':
						if (!(stoneId == "751"))
						{
							goto IL_BBF;
						}
						goto IL_874;
					case '2':
						goto IL_BBF;
					case '3':
						if (!(stoneId == "843"))
						{
							goto IL_BBF;
						}
						break;
					case '4':
						if (!(stoneId == "844"))
						{
							if (!(stoneId == "764"))
							{
								goto IL_BBF;
							}
							goto IL_A08;
						}
						break;
					case '5':
						if (stoneId == "845")
						{
							goto IL_805;
						}
						if (!(stoneId == "765"))
						{
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("(O)386", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
						Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100, ""));
						if (r.NextDouble() < 0.035)
						{
							Game1.createMultipleObjectDebris("(O)74", x, y, 1, farmerId, this);
						}
						experience = 50;
						goto IL_BBF;
					case '6':
						if (stoneId == "816")
						{
							goto IL_578;
						}
						if (!(stoneId == "846"))
						{
							goto IL_BBF;
						}
						goto IL_805;
					case '7':
						if (stoneId == "817")
						{
							goto IL_578;
						}
						if (!(stoneId == "847"))
						{
							goto IL_BBF;
						}
						goto IL_805;
					case '8':
						if (stoneId == "818")
						{
							Game1.createMultipleObjectDebris("(O)330", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
							experience = 6;
							goto IL_BBF;
						}
						if (!(stoneId == "668"))
						{
							goto IL_BBF;
						}
						goto IL_805;
					case '9':
						if (stoneId == "819")
						{
							Game1.createObjectDebris("(O)749", x, y, farmerId, this);
							experience = 64;
							goto IL_BBF;
						}
						if (!(stoneId == "849"))
						{
							goto IL_BBF;
						}
						goto IL_874;
					default:
						goto IL_BBF;
					}
					Game1.createMultipleObjectDebris("(O)848", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 200f)) ? 1 : 0), farmerId, this);
					experience = 12;
					goto IL_BBF;
					IL_578:
					if (r.NextDouble() < 0.1)
					{
						Game1.createObjectDebris("(O)823", x, y, farmerId, this);
					}
					else if (r.NextDouble() < 0.015)
					{
						Game1.createObjectDebris("(O)824", x, y, farmerId, this);
					}
					else if (r.NextDouble() < 0.1)
					{
						Game1.createObjectDebris("(O)" + (579 + r.Next(11)).ToString(), x, y, farmerId, this);
					}
					Game1.createMultipleObjectDebris("(O)881", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
					experience = 6;
					goto IL_BBF;
					IL_805:
					Game1.createMultipleObjectDebris("(O)390", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
					experience = 3;
					if (r.NextDouble() < 0.08)
					{
						Game1.createMultipleObjectDebris("(O)382", x, y, 1 + addedOres, farmerId, this);
						experience = 4;
						goto IL_BBF;
					}
					goto IL_BBF;
					IL_874:
					Game1.createMultipleObjectDebris("(O)378", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
					experience = 5;
					Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100, ""));
					goto IL_BBF;
				default:
					switch (length)
					{
					case 14:
					{
						char c = stoneId[13];
						if (c != '0')
						{
							if (c != '1')
							{
								goto IL_BBF;
							}
							if (!(stoneId == "BasicCoalNode1"))
							{
								goto IL_BBF;
							}
						}
						else if (!(stoneId == "BasicCoalNode0"))
						{
							goto IL_BBF;
						}
						break;
					}
					case 15:
						if (!(stoneId == "VolcanoGoldNode"))
						{
							goto IL_BBF;
						}
						goto IL_A08;
					case 16:
						switch (stoneId[15])
						{
						case '0':
							if (stoneId == "VolcanoCoalNode0")
							{
								goto IL_981;
							}
							if (!(stoneId == "CalicoEggStone_0"))
							{
								goto IL_BBF;
							}
							break;
						case '1':
							if (stoneId == "VolcanoCoalNode1")
							{
								goto IL_981;
							}
							if (!(stoneId == "CalicoEggStone_1"))
							{
								goto IL_BBF;
							}
							break;
						case '2':
							if (!(stoneId == "CalicoEggStone_2"))
							{
								goto IL_BBF;
							}
							break;
						default:
							goto IL_BBF;
						}
						Game1.createMultipleObjectDebris("CalicoEgg", x, y, r.Next(1, 4) + ((r.NextBool((float)farmerLuckLevel / 100f) > false) ? 1 : 0) + ((r.NextBool((float)farmerMiningLevel / 100f) > false) ? 1 : 0), farmerId, this);
						experience = 50;
						Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, new Color(255, 120, 0) * 0.5f, 175, 100, ""));
						goto IL_BBF;
					default:
						goto IL_BBF;
					}
					IL_981:
					Game1.createMultipleObjectDebris("(O)382", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
					experience = 10;
					Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Black * 0.5f, 175, 100, ""));
					goto IL_BBF;
				}
				IL_A08:
				Game1.createMultipleObjectDebris("(O)384", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
				experience = 18;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100, ""));
			}
			IL_BBF:
			if (who != null && who.professions.Contains(19) && r.NextBool())
			{
				int numToDrop = (who.stats.Get(StatKeys.Mastery(3)) > 0U) ? 2 : 1;
				if (stoneId != null)
				{
					int length = stoneId.Length;
					if (length != 1)
					{
						if (length == 2)
						{
							switch (stoneId[1])
							{
							case '0':
								if (stoneId == "10")
								{
									Game1.createMultipleObjectDebris("(O)68", x, y, numToDrop, who.UniqueMultiplayerID, this);
									experience = 8;
								}
								break;
							case '2':
								if (stoneId == "12")
								{
									Game1.createMultipleObjectDebris("(O)60", x, y, numToDrop, who.UniqueMultiplayerID, this);
									experience = 50;
								}
								break;
							case '4':
								if (stoneId == "14")
								{
									Game1.createMultipleObjectDebris("(O)62", x, y, numToDrop, who.UniqueMultiplayerID, this);
									experience = 20;
								}
								break;
							}
						}
					}
					else
					{
						switch (stoneId[0])
						{
						case '2':
							Game1.createMultipleObjectDebris("(O)72", x, y, numToDrop, who.UniqueMultiplayerID, this);
							experience = 100;
							break;
						case '4':
							Game1.createMultipleObjectDebris("(O)64", x, y, numToDrop, who.UniqueMultiplayerID, this);
							experience = 50;
							break;
						case '6':
							Game1.createMultipleObjectDebris("(O)70", x, y, numToDrop, who.UniqueMultiplayerID, this);
							experience = 20;
							break;
						case '8':
							Game1.createMultipleObjectDebris("(O)66", x, y, numToDrop, who.UniqueMultiplayerID, this);
							experience = 8;
							break;
						}
					}
				}
			}
			if (stoneId == 46.ToString())
			{
				Game1.createDebris(10, x, y, r.Next(1, 4), this);
				Game1.createDebris(6, x, y, r.Next(1, 5), this);
				if (r.NextDouble() < 0.25)
				{
					Game1.createMultipleObjectDebris("(O)74", x, y, 1, farmerId, this);
				}
				experience = 150;
				Stats stats = Game1.stats;
				uint mysticStonesCrushed = stats.MysticStonesCrushed;
				stats.MysticStonesCrushed = mysticStonesCrushed + 1U;
			}
			if ((this.isOutdoors.Value || this.treatAsOutdoors.Value) && experience == 0)
			{
				double chanceModifier = farmerDailyLuck / 2.0 + (double)farmerMiningLevel * 0.005 + (double)farmerLuckLevel * 0.001;
				Random ran = Utility.CreateDaySaveRandom((double)(x * 1000), (double)y, 0.0);
				Game1.createDebris(14, x, y, 1, this);
				if (who != null)
				{
					who.gainExperience(3, 1);
					double coalChance = 0.0;
					if (who.professions.Contains(21))
					{
						coalChance += 0.05 * (1.0 + chanceModifier);
					}
					if (who.hasBuff("dwarfStatue_2"))
					{
						coalChance += 0.025;
					}
					if (ran.NextDouble() < coalChance)
					{
						Game1.createObjectDebris("(O)382", x, y, who.UniqueMultiplayerID, this);
					}
				}
				if (ran.NextDouble() < 0.05 * (1.0 + chanceModifier))
				{
					Game1.createObjectDebris("(O)382", x, y, farmerId, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(25, new Vector2((float)(64 * x), (float)(64 * y)), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128, 0)
					});
					if (who != null)
					{
						who.gainExperience(3, 5);
					}
				}
			}
			if (who != null && this.HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
			{
				Object o = this.tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					Game1.createItemDebris(o, new Vector2((float)x + 0.5f, (float)y + 0.75f) * 64f, Game1.player.FacingDirection, this, -1, false);
				}
			}
			if (who != null)
			{
				who.gainExperience(3, experience);
			}
			return experience > 0;
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x000BDE04 File Offset: 0x000BC004
		public bool isBehindBush(Vector2 Tile)
		{
			if (this.largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)Tile.X * 64, (int)(Tile.Y + 1f) * 64, 64, 128);
				using (List<LargeTerrainFeature>.Enumerator enumerator = this.largeTerrainFeatures.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.getBoundingBox().Intersects(down))
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x000BDE98 File Offset: 0x000BC098
		public bool isBehindTree(Vector2 Tile)
		{
			if (this.terrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)(Tile.X - 1f) * 64, (int)Tile.Y * 64, 192, 256);
				foreach (KeyValuePair<Vector2, TerrainFeature> i in this.terrainFeatures.Pairs)
				{
					if (i.Value is Tree && i.Value.getBoundingBox().Intersects(down))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x000BDF54 File Offset: 0x000BC154
		public virtual void spawnObjects()
		{
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			LocationData data = this.GetData();
			if (data != null && this.numberOfSpawnedObjectsOnMap < data.MaxSpawnedForageAtOnce)
			{
				Season season = this.GetSeason();
				List<SpawnForageData> possibleForage = new List<SpawnForageData>();
				foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
				{
					if (spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, this, null, null, null, r, null))
					{
						if (spawn.Season != null)
						{
							Season? season2 = spawn.Season;
							Season season3 = season;
							if (!(season2.GetValueOrDefault() == season3 & season2 != null))
							{
								continue;
							}
						}
						possibleForage.Add(spawn);
					}
				}
				if (possibleForage.Any<SpawnForageData>())
				{
					int numberToSpawn = r.Next(data.MinDailyForageSpawn, data.MaxDailyForageSpawn + 1);
					numberToSpawn = Math.Min(numberToSpawn, data.MaxSpawnedForageAtOnce - this.numberOfSpawnedObjectsOnMap);
					ItemQueryContext itemQueryContext = new ItemQueryContext(this, null, r, "location '" + this.NameOrUniqueName + "' > forage");
					for (int i = 0; i < numberToSpawn; i++)
					{
						for (int attempt = 0; attempt < 11; attempt++)
						{
							int xCoord = r.Next(this.map.DisplayWidth / 64);
							int yCoord = r.Next(this.map.DisplayHeight / 64);
							Vector2 location = new Vector2((float)xCoord, (float)yCoord);
							if (!this.objects.ContainsKey(location) && !this.IsNoSpawnTile(location, "All", false) && this.doesTileHaveProperty(xCoord, yCoord, "Spawnable", "Back", false) != null && !this.doesEitherTileOrTileIndexPropertyEqual(xCoord, yCoord, "Spawnable", "Back", "F") && this.CanItemBePlacedHere(location, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && !this.hasTileAt(xCoord, yCoord, "AlwaysFront", null) && !this.hasTileAt(xCoord, yCoord, "AlwaysFront2", null) && !this.hasTileAt(xCoord, yCoord, "AlwaysFront3", null) && !this.hasTileAt(xCoord, yCoord, "Front", null) && !this.isBehindBush(location) && (r.NextBool(0.1) || !this.isBehindTree(location)))
							{
								SpawnForageData forage = r.ChooseFrom(possibleForage);
								if (r.NextBool(forage.Chance))
								{
									Item forageItem = ItemQueryResolver.TryResolveRandomItem(forage, itemQueryContext, false, null, null, null, delegate(string query, string error)
									{
										IGameLogger log2 = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 4);
										defaultInterpolatedStringHandler2.AppendLiteral("Location '");
										defaultInterpolatedStringHandler2.AppendFormatted(this.NameOrUniqueName);
										defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
										defaultInterpolatedStringHandler2.AppendFormatted(query);
										defaultInterpolatedStringHandler2.AppendLiteral("' for forage '");
										defaultInterpolatedStringHandler2.AppendFormatted(forage.Id);
										defaultInterpolatedStringHandler2.AppendLiteral("': ");
										defaultInterpolatedStringHandler2.AppendFormatted(error);
										log2.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
									});
									if (forageItem != null)
									{
										Object forageObj = forageItem as Object;
										if (forageObj == null)
										{
											IGameLogger log = Game1.log;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 4);
											defaultInterpolatedStringHandler.AppendLiteral("Location '");
											defaultInterpolatedStringHandler.AppendFormatted(this.Name);
											defaultInterpolatedStringHandler.AppendLiteral("' ignored invalid forage data '");
											defaultInterpolatedStringHandler.AppendFormatted(forage.Id);
											defaultInterpolatedStringHandler.AppendLiteral("': the resulting item '");
											defaultInterpolatedStringHandler.AppendFormatted(forageItem.QualifiedItemId);
											defaultInterpolatedStringHandler.AppendLiteral("' isn't an ");
											defaultInterpolatedStringHandler.AppendFormatted("Object");
											defaultInterpolatedStringHandler.AppendLiteral("-type item.");
											log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
										}
										else
										{
											forageObj.IsSpawnedObject = true;
											if (this.dropObject(forageObj, location * 64f, Game1.viewport, true, null))
											{
												this.numberOfSpawnedObjectsOnMap++;
												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			List<Vector2> positionOfArtifactSpots = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> v in this.objects.Pairs)
			{
				if (v.Value.QualifiedItemId == "(O)590" || v.Value.QualifiedItemId == "(O)SeedSpot")
				{
					positionOfArtifactSpots.Add(v.Key);
				}
			}
			if (!(this is Farm) && !(this is IslandWest))
			{
				this.spawnWeedsAndStones(-1, false, true);
			}
			for (int j = positionOfArtifactSpots.Count - 1; j >= 0; j--)
			{
				if ((!(this is IslandNorth) || positionOfArtifactSpots[j].X >= 26f) && r.NextBool(0.15))
				{
					this.objects.Remove(positionOfArtifactSpots[j]);
					positionOfArtifactSpots.RemoveAt(j);
				}
			}
			if (positionOfArtifactSpots.Count <= ((!(this is Farm)) ? 1 : 0) || (this.GetSeason() == Season.Winter && positionOfArtifactSpots.Count <= 4))
			{
				double chanceForNewArtifactAttempt = 1.0;
				while (r.NextDouble() < chanceForNewArtifactAttempt)
				{
					int xCoord2 = r.Next(this.map.DisplayWidth / 64);
					int yCoord2 = r.Next(this.map.DisplayHeight / 64);
					Vector2 location2 = new Vector2((float)xCoord2, (float)yCoord2);
					if (this.CanItemBePlacedHere(location2, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && !this.IsTileOccupiedBy(location2, CollisionMask.All, CollisionMask.None, false) && !this.hasTileAt(xCoord2, yCoord2, "AlwaysFront", null) && !this.hasTileAt(xCoord2, yCoord2, "Front", null) && !this.isBehindBush(location2) && (this.doesTileHaveProperty(xCoord2, yCoord2, "Diggable", "Back", false) != null || (this.GetSeason() == Season.Winter && this.doesTileHaveProperty(xCoord2, yCoord2, "Type", "Back", false) != null && this.doesTileHaveProperty(xCoord2, yCoord2, "Type", "Back", false).Equals("Grass"))))
					{
						if (this.name.Equals("Forest") && xCoord2 >= 93 && yCoord2 <= 22)
						{
							continue;
						}
						this.objects.Add(location2, ItemRegistry.Create<Object>(r.NextBool(0.166) ? "(O)SeedSpot" : "(O)590", 1, 0, false));
					}
					chanceForNewArtifactAttempt *= 0.75;
					if (this.GetSeason() == Season.Winter)
					{
						chanceForNewArtifactAttempt += 0.10000000149011612;
					}
				}
			}
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x000BE5EC File Offset: 0x000BC7EC
		public void spawnWeedsAndStones(int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
		{
			if ((this is Farm || this is IslandWest) && Game1.IsBuildingConstructed("Gold Clock") && !Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
			{
				return;
			}
			bool notified_destruction = false;
			if (!(this is Beach) && this.GetSeason() != Season.Winter && !(this is Desert))
			{
				int numWeedsAndStones = (numDebris != -1) ? numDebris : ((Game1.random.NextDouble() < 0.95) ? ((Game1.random.NextDouble() < 0.25) ? Game1.random.Next(10, 21) : Game1.random.Next(5, 11)) : 0);
				if (this.IsRainingHere())
				{
					numWeedsAndStones *= 2;
				}
				if (Game1.dayOfMonth == 1)
				{
					numWeedsAndStones *= 5;
				}
				if (this.objects.Length <= 0 && spawnFromOldWeeds)
				{
					return;
				}
				if (!(this is Farm))
				{
					numWeedsAndStones /= 2;
				}
				bool greenRain = this.IsGreenRainingHere();
				for (int i = 0; i < numWeedsAndStones; i++)
				{
					Vector2 v = spawnFromOldWeeds ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : new Vector2((float)Game1.random.Next(this.map.Layers[0].LayerWidth), (float)Game1.random.Next(this.map.Layers[0].LayerHeight));
					if (!spawnFromOldWeeds && this is IslandWest)
					{
						v = new Vector2((float)Game1.random.Next(57, 97), (float)Game1.random.Next(44, 68));
					}
					while (spawnFromOldWeeds && v.Equals(Vector2.Zero))
					{
						v = new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
					}
					Vector2 fromTile = Vector2.Zero;
					Object fromObj = null;
					if (spawnFromOldWeeds)
					{
						Utility.TryGetRandom(this.objects, out fromTile, out fromObj, null);
					}
					Vector2 baseVect = spawnFromOldWeeds ? fromTile : Vector2.Zero;
					if ((!(this is Mountain) || v.X + baseVect.X <= 100f) && !(this is IslandNorth))
					{
						bool flag = this is Farm || this is IslandWest;
						int checked_tile_x = (int)(v.X + baseVect.X);
						int checked_tile_y = (int)(v.Y + baseVect.Y);
						Vector2 checked_tile = v + baseVect;
						int health = 1;
						bool is_valid_tile = false;
						bool tile_is_diggable = this.doesTileHaveProperty(checked_tile_x, checked_tile_y, "Diggable", "Back", false) != null;
						if (flag == tile_is_diggable && !this.IsNoSpawnTile(checked_tile, "All", false) && this.doesTileHaveProperty(checked_tile_x, checked_tile_y, "Type", "Back", false) != "Wood")
						{
							bool is_tile_clear = false;
							if (this.CanItemBePlacedHere(checked_tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && !this.terrainFeatures.ContainsKey(checked_tile))
							{
								is_tile_clear = true;
							}
							else if (spawnFromOldWeeds)
							{
								Object tileObj;
								if (this.objects.TryGetValue(checked_tile, out tileObj))
								{
									if (greenRain)
									{
										is_tile_clear = false;
									}
									else if (!tileObj.IsTapper())
									{
										is_tile_clear = true;
									}
								}
								TerrainFeature terrainFeature;
								if (!is_tile_clear && this.terrainFeatures.TryGetValue(checked_tile, out terrainFeature) && (terrainFeature is HoeDirt || terrainFeature is Flooring))
								{
									is_tile_clear = (!greenRain && this.getLargeTerrainFeatureAt(checked_tile_x, checked_tile_y) == null);
								}
							}
							if (is_tile_clear)
							{
								if (spawnFromOldWeeds)
								{
									is_valid_tile = true;
								}
								else if (!this.objects.ContainsKey(checked_tile))
								{
									is_valid_tile = true;
								}
							}
						}
						if (is_valid_tile)
						{
							string whatToAdd = null;
							if (this is Desert)
							{
								whatToAdd = "(O)750";
							}
							else
							{
								if (Game1.random.NextBool() && !weedsOnly && (!spawnFromOldWeeds || fromObj.IsBreakableStone() || fromObj.IsTwig()))
								{
									whatToAdd = Game1.random.Choose("(O)294", "(O)295", "(O)343", "(O)450");
								}
								else if (!spawnFromOldWeeds || fromObj.IsWeeds())
								{
									whatToAdd = GameLocation.getWeedForSeason(Game1.random, this.GetSeason());
									if (this.IsGreenRainingHere())
									{
										if (this.doesTileHavePropertyNoNull((int)(v.X + baseVect.X), (int)(v.Y + baseVect.Y), "Type", "Back") == (this.IsFarm ? "Dirt" : "Grass"))
										{
											int which = Game1.random.Next(8);
											whatToAdd = "(O)GreenRainWeeds" + which.ToString();
											if (which == 2 || which == 3 || which == 7)
											{
												health = 2;
											}
										}
										else
										{
											whatToAdd = null;
										}
									}
								}
								if (this is Farm && !spawnFromOldWeeds && Game1.random.NextDouble() < 0.05 && !this.terrainFeatures.ContainsKey(checked_tile))
								{
									this.terrainFeatures.Add(checked_tile, new Tree((Game1.random.Next(3) + 1).ToString(), Game1.random.Next(3), false));
									goto IL_667;
								}
							}
							if (whatToAdd != null)
							{
								bool destroyed = false;
								Object removedObj;
								if (this.objects.TryGetValue(v + baseVect, out removedObj))
								{
									if (greenRain || removedObj is Fence || removedObj is Chest || removedObj.QualifiedItemId == "(O)590" || removedObj.QualifiedItemId == "(BC)MushroomLog")
									{
										goto IL_667;
									}
									if (removedObj.name.Length > 0 && removedObj.Category != -999)
									{
										destroyed = true;
										Game1.debugOutput = removedObj.Name + " was destroyed";
									}
									this.objects.Remove(v + baseVect);
								}
								TerrainFeature removedFeature;
								if (this.terrainFeatures.TryGetValue(v + baseVect, out removedFeature))
								{
									try
									{
										destroyed = (removedFeature is HoeDirt || removedFeature is Flooring);
									}
									catch (Exception)
									{
									}
									if (!destroyed || this.IsGreenRainingHere())
									{
										return;
									}
									this.terrainFeatures.Remove(v + baseVect);
								}
								if (destroyed && this is Farm && Game1.stats.DaysPlayed > 1U && !notified_destruction)
								{
									notified_destruction = true;
									Game1.multiplayer.broadcastGlobalMessage("Strings\\Locations:Farm_WeedsDestruction", false, null, Array.Empty<string>());
								}
								Object obj = ItemRegistry.Create<Object>(whatToAdd, 1, 0, false);
								obj.minutesUntilReady.Value = health;
								this.objects.TryAdd(v + baseVect, obj);
							}
						}
					}
					IL_667:;
				}
			}
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x000BEC7C File Offset: 0x000BCE7C
		[Obsolete("Use removeObjectsAndSpawned instead.")]
		public virtual void removeEverythingExceptCharactersFromThisTile(int x, int y)
		{
			this.removeObjectsAndSpawned(x, y, 1, 1);
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x000BEC88 File Offset: 0x000BCE88
		public virtual void removeObjectsAndSpawned(int x, int y, int width, int height)
		{
			Microsoft.Xna.Framework.Rectangle pixelArea = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, width * 64, height * 64);
			int maxX = x + width - 1;
			int maxY = y + height - 1;
			for (int curY = y; curY <= maxY; curY++)
			{
				for (int curX = x; curX <= maxX; curX++)
				{
					Vector2 tile = new Vector2((float)curX, (float)curY);
					this.terrainFeatures.Remove(tile);
					this.objects.Remove(tile);
				}
			}
			this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.getBoundingBox().Intersects(pixelArea));
			this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.getBoundingBox().Intersects(pixelArea));
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x000BED38 File Offset: 0x000BCF38
		public virtual string getFootstepSoundReplacement(string footstep)
		{
			return footstep;
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x000BED3C File Offset: 0x000BCF3C
		public virtual void removeEverythingFromThisTile(int x, int y)
		{
			Vector2 tile = new Vector2((float)x, (float)y);
			Point pixel = Utility.Vector2ToPoint(tile * 64f + new Vector2(32f));
			this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.Tile == tile);
			this.terrainFeatures.Remove(tile);
			this.objects.Remove(tile);
			this.furniture.RemoveWhere((Furniture f) => f.GetBoundingBox().Contains(pixel));
			this.characters.RemoveWhere((NPC npc) => npc.Tile == tile && npc is Monster);
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x000BEDF4 File Offset: 0x000BCFF4
		public virtual bool TryGetLocationEvents(out string assetName, out Dictionary<string, string> events)
		{
			events = null;
			assetName = ((this.NameOrUniqueName == Game1.player.homeLocation.Value) ? "Data\\Events\\FarmHouse" : ("Data\\Events\\" + this.name.Value));
			try
			{
				if (Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					events = Game1.content.Load<Dictionary<string, string>>(assetName);
				}
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed loading events for location '");
				defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral("' from asset '");
				defaultInterpolatedStringHandler.AppendFormatted(assetName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
			}
			if (events == null)
			{
				events = new Dictionary<string, string>();
			}
			if (assetName != "Data\\Events\\FarmHouse")
			{
				foreach (KeyValuePair<string, string> @event in Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse"))
				{
					if (@event.Key.StartsWith("558291/") || @event.Key.StartsWith("558292/"))
					{
						events.TryAdd(@event.Key, @event.Value);
					}
				}
			}
			if (this.Name == "Trailer_Big")
			{
				events = new Dictionary<string, string>(events);
				Dictionary<string, string> trailer_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Trailer");
				if (trailer_events != null)
				{
					foreach (string trailer_event_key in trailer_events.Keys)
					{
						string event_string = trailer_events[trailer_event_key];
						if (!(this.name.Value == "Trailer_Big") || !events.ContainsKey(trailer_event_key))
						{
							if (trailer_event_key.StartsWith("36/"))
							{
								event_string = event_string.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								event_string = event_string.Replace("/playSound doorClose/warp farmer 12 9", "/move farmer 0 -10 0");
							}
							else if (trailer_event_key.StartsWith("35/"))
							{
								event_string = event_string.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								event_string = event_string.Replace("/warp farmer 12 9/playSound doorClose", "/move farmer 0 -10 0");
								event_string = event_string.Replace("/warp farmer -40 -40/playSound doorClose", "/move farmer 0 10 0/warp farmer -40 -40");
							}
							events[trailer_event_key] = event_string;
						}
					}
				}
			}
			return events.Count > 0;
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x000BF09C File Offset: 0x000BD29C
		public static bool IsValidLocationEvent(string key, string eventScript)
		{
			int num;
			if (!key.Contains('/') && !int.TryParse(key, out num))
			{
				return false;
			}
			string[] commands = Event.ParseCommands(eventScript, null);
			if (commands.Length < 3)
			{
				return false;
			}
			string cameraPosition = commands[1];
			return cameraPosition.Length != 0 && (!(cameraPosition != "follow") || char.IsDigit(cameraPosition[0]) || cameraPosition[0] == '-');
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x000BF104 File Offset: 0x000BD304
		public virtual void checkForEvents()
		{
			if (Game1.killScreen && !Game1.eventUp)
			{
				if (Game1.player.bathingClothes.Value)
				{
					Game1.player.changeOutOfSwimSuit();
				}
				if (this.name.Equals("Mine"))
				{
					string rescuer;
					string uniquemessage;
					switch (Game1.random.Next(7))
					{
					case 0:
						rescuer = "Robin";
						uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Robin";
						break;
					case 1:
						rescuer = "Clint";
						uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Clint";
						break;
					case 2:
						rescuer = "Maru";
						uniquemessage = ((Game1.player.spouse == "Maru") ? "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_Spouse" : "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_NotSpouse");
						break;
					default:
						rescuer = "Linus";
						uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
						break;
					}
					if (Game1.random.NextDouble() < 0.1 && Game1.player.spouse != null && !Game1.player.isEngaged() && Game1.player.spouse.Length > 1)
					{
						rescuer = Game1.player.spouse;
						uniquemessage = (Game1.player.IsMale ? "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerMale" : "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerFemale");
					}
					this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Mine:PlayerKilled", rescuer, uniquemessage, ArgUtility.EscapeQuotes(Game1.player.Name)), null);
				}
				else if (this is IslandLocation)
				{
					string rescuer2 = "Willy";
					string uniquemessage2 = "Data\\ExtraDialogue:Island_willy_rescue";
					if (Game1.player.friendshipData.ContainsKey("Leo") && Game1.random.NextBool())
					{
						rescuer2 = "Leo";
						uniquemessage2 = "Data\\ExtraDialogue:Island_leo_rescue";
					}
					this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:PlayerKilled", rescuer2, uniquemessage2, ArgUtility.EscapeQuotes(Game1.player.Name)), null);
				}
				else if (this.name.Equals("Hospital"))
				{
					this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Hospital:PlayerKilled", ArgUtility.EscapeQuotes(Game1.player.Name)), null);
				}
				else
				{
					try
					{
						string assetName;
						Dictionary<string, string> events;
						string eventScript;
						if (this.TryGetLocationEvents(out assetName, out events) && events.TryGetValue("PlayerKilled", out eventScript))
						{
							this.currentEvent = new Event(eventScript, assetName, "PlayerKilled", null);
						}
					}
					catch (Exception)
					{
					}
				}
				if (this.currentEvent != null)
				{
					Game1.eventUp = true;
				}
				Game1.changeMusicTrack("none", true, MusicContext.Default);
				Game1.killScreen = false;
				Game1.player.health = 10;
				return;
			}
			if (!Game1.eventUp && Game1.weddingsToday.Count > 0 && (Game1.CurrentEvent == null || Game1.CurrentEvent.id != "-2") && Game1.currentLocation != null && !Game1.currentLocation.IsTemporary)
			{
				this.currentEvent = Game1.getAvailableWeddingEvent();
				if (this.currentEvent != null)
				{
					this.startEvent(this.currentEvent);
					return;
				}
			}
			else if (!Game1.eventUp && Game1.farmEvent == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
				defaultInterpolatedStringHandler.AppendFormatted(Game1.currentSeason);
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.dayOfMonth);
				string key = defaultInterpolatedStringHandler.ToStringAndClear();
				try
				{
					Event festival;
					if (Event.tryToLoadFestival(key, out festival))
					{
						this.currentEvent = festival;
					}
				}
				catch (Exception)
				{
				}
				if (!Game1.eventUp && this.currentEvent == null && Game1.farmEvent == null && !this.IsGreenRainingHere())
				{
					string eventAssetName;
					Dictionary<string, string> events2;
					try
					{
						if (!this.TryGetLocationEvents(out eventAssetName, out events2))
						{
							return;
						}
					}
					catch
					{
						return;
					}
					if (events2 != null)
					{
						foreach (string eventKey in events2.Keys)
						{
							string eventId = this.checkEventPrecondition(eventKey);
							if (!string.IsNullOrEmpty(eventId) && eventId != "-1" && GameLocation.IsValidLocationEvent(eventKey, events2[eventKey]))
							{
								this.currentEvent = new Event(events2[eventKey], eventAssetName, eventId, null);
								break;
							}
						}
						PetData data;
						if (this.currentEvent == null && Game1.IsMasterGame && Game1.stats.DaysPlayed >= 20U && !Game1.player.mailReceived.Contains("rejectedPet") && !Game1.player.hasPet() && Pet.TryGetData(Game1.player.whichPetType, out data) && this.Name == data.AdoptionEventLocation && !string.IsNullOrWhiteSpace(data.AdoptionEventId) && !Game1.player.eventsSeen.Contains(data.AdoptionEventId))
						{
							Game1.PlayEvent(data.AdoptionEventId, false, false);
						}
					}
				}
				if (this.currentEvent != null)
				{
					this.startEvent(this.currentEvent);
				}
			}
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x000BF5E4 File Offset: 0x000BD7E4
		public Event findEventById(string id, Farmer farmerActor = null)
		{
			if (id == "-2")
			{
				long? spouseFarmer = Game1.player.team.GetSpouse(farmerActor.UniqueMultiplayerID);
				if (farmerActor == null || spouseFarmer == null)
				{
					return Utility.getWeddingEvent(farmerActor);
				}
				if (Game1.otherFarmers.ContainsKey(spouseFarmer.Value))
				{
					return Utility.getWeddingEvent(farmerActor);
				}
			}
			string eventAssetName;
			Dictionary<string, string> events;
			try
			{
				if (!this.TryGetLocationEvents(out eventAssetName, out events))
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
			foreach (KeyValuePair<string, string> pair in events)
			{
				if (Event.SplitPreconditions(pair.Key)[0] == id)
				{
					return new Event(pair.Value, eventAssetName, id, farmerActor);
				}
			}
			return null;
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x000BF6CC File Offset: 0x000BD8CC
		public virtual void startEvent(Event evt)
		{
			if (Game1.eventUp || Game1.eventOver)
			{
				return;
			}
			this.currentEvent = evt;
			this.ResetForEvent(evt);
			if (evt.exitLocation == null)
			{
				evt.exitLocation = Game1.getLocationRequest(this.NameOrUniqueName, this.isStructure.Value);
			}
			if (Game1.player.mount != null)
			{
				Horse mount = Game1.player.mount;
				mount.currentLocation = this;
				mount.dismount(false);
				Microsoft.Xna.Framework.Rectangle bbox = mount.GetBoundingBox();
				Vector2 position = mount.Position;
				if (mount.currentLocation != null && mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, false, 0, false, mount, true, false, false, false))
				{
					bbox.X -= 64;
					if (!mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, false, 0, false, mount, true, false, false, false))
					{
						position.X -= 64f;
						mount.Position = position;
					}
					else
					{
						bbox.X += 128;
						if (!mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, false, 0, false, mount, true, false, false, false))
						{
							position.X += 64f;
							mount.Position = position;
						}
					}
				}
			}
			foreach (NPC npc in this.characters)
			{
				npc.clearTextAboveHead();
			}
			Game1.eventUp = true;
			Game1.displayHUD = false;
			Game1.player.CanMove = false;
			Game1.player.showNotCarrying();
			List<Critter> list = this.critters;
			if (list != null)
			{
				list.Clear();
			}
			if (this.currentEvent != null)
			{
				Game1.player.autoGenerateActiveDialogueEvent("eventSeen_" + this.currentEvent.id, 4);
			}
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x000BF89C File Offset: 0x000BDA9C
		public virtual void drawBackground(SpriteBatch b)
		{
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x000BF8A0 File Offset: 0x000BDAA0
		public virtual void drawWater(SpriteBatch b)
		{
			Event @event = this.currentEvent;
			if (@event != null)
			{
				@event.drawUnderWater(b);
			}
			if (this.waterTiles == null)
			{
				return;
			}
			for (int y = Math.Max(0, Game1.viewport.Y / 64 - 1); y < Math.Min(this.map.Layers[0].LayerHeight, (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2); y++)
			{
				for (int x = Math.Max(0, Game1.viewport.X / 64 - 1); x < Math.Min(this.map.Layers[0].LayerWidth, (Game1.viewport.X + Game1.viewport.Width) / 64 + 1); x++)
				{
					if (this.waterTiles.waterTiles[x, y].isWater && this.waterTiles.waterTiles[x, y].isVisible)
					{
						this.drawWaterTile(b, x, y);
					}
				}
			}
		}

		// Token: 0x06000FE3 RID: 4067 RVA: 0x000BF9B1 File Offset: 0x000BDBB1
		public virtual void drawWaterTile(SpriteBatch b, int x, int y)
		{
			this.drawWaterTile(b, x, y, this.waterColor.Value);
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x000BF9C8 File Offset: 0x000BDBC8
		public void drawWaterTile(SpriteBatch b, int x, int y, Color color)
		{
			bool flag = y == this.map.Layers[0].LayerHeight - 1 || !this.waterTiles[x, y + 1];
			bool topY = y == 0 || !this.waterTiles[x, y - 1];
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - (int)((!topY) ? this.waterPosition : 0f)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.waterAnimationIndex * 64, 2064 + (((x + y) % 2 == 0) ? (this.waterTileFlip ? 128 : 0) : (this.waterTileFlip ? 0 : 128)) + (topY ? ((int)this.waterPosition) : 0), 64, 64 + (topY ? ((int)(-(int)this.waterPosition)) : 0))), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			if (flag)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y + 1) * 64 - (int)this.waterPosition))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 == 0) ? (this.waterTileFlip ? 128 : 0) : (this.waterTileFlip ? 0 : 128)), 64, 64 - (int)(64f - this.waterPosition) - 1)), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			}
		}

		// Token: 0x06000FE5 RID: 4069 RVA: 0x000BFB70 File Offset: 0x000BDD70
		public virtual void drawFloorDecorations(SpriteBatch b)
		{
			int borderBuffer = 1;
			Microsoft.Xna.Framework.Rectangle viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - borderBuffer, Game1.viewport.Y / 64 - borderBuffer, (int)Math.Ceiling((double)((float)Game1.viewport.Width / 64f)) + 2 * borderBuffer, (int)Math.Ceiling((double)((float)Game1.viewport.Height / 64f)) + 3 + 2 * borderBuffer);
			Microsoft.Xna.Framework.Rectangle objectRectangle = default(Microsoft.Xna.Framework.Rectangle);
			if (this.buildings.Count > 0)
			{
				foreach (Building building in this.buildings)
				{
					int additionalRadius = building.GetAdditionalTilePropertyRadius();
					Microsoft.Xna.Framework.Rectangle sourceRect = building.getSourceRect();
					objectRectangle.X = building.tileX.Value - additionalRadius;
					objectRectangle.Width = building.tilesWide.Value + additionalRadius * 2;
					int bottomY = building.tileY.Value + building.tilesHigh.Value + additionalRadius;
					int topY = bottomY - (int)Math.Ceiling((double)((float)sourceRect.Height * 4f / 64f)) - additionalRadius;
					objectRectangle.Y = topY;
					objectRectangle.Height = bottomY - topY;
					if (objectRectangle.Intersects(viewportRect))
					{
						building.drawBackground(b);
					}
				}
			}
			if (!Game1.isFestival() && this.terrainFeatures.Length > 0)
			{
				Vector2 tile = default(Vector2);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
					{
						tile.X = (float)x;
						tile.Y = (float)y;
						TerrainFeature feat;
						if (this.terrainFeatures.TryGetValue(tile, out feat) && feat is Flooring)
						{
							feat.draw(b);
						}
					}
				}
			}
			if (!Game1.eventUp || this is Farm || this is FarmHouse)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture f in this.furniture)
				{
					if (f.furniture_type.Value == 12)
					{
						f.draw(b, -1, -1, 1f);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x000BFE30 File Offset: 0x000BE030
		public TemporaryAnimatedSprite getTemporarySpriteByID(int id)
		{
			for (int i = 0; i < this.temporarySprites.Count; i++)
			{
				if (this.temporarySprites[i].id == id)
				{
					return this.temporarySprites[i];
				}
			}
			return null;
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x000BFE78 File Offset: 0x000BE078
		protected void drawDebris(SpriteBatch b)
		{
			int counter = 0;
			foreach (Debris d in this.debris)
			{
				counter++;
				if (d.item != null)
				{
					Vector2 position = d.Chunks[0].GetVisualPosition();
					Object obj = d.item as Object;
					if (obj != null && obj.bigCraftable.Value)
					{
						obj.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f))), 1.6f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, true);
					}
					else
					{
						d.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f))), 0.8f + (float)d.itemQuality * 0.1f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, true);
					}
				}
				else
				{
					Debris.DebrisType value = d.debrisType.Value;
					if (value != Debris.DebrisType.LETTERS)
					{
						if (value != Debris.DebrisType.SPRITECHUNKS)
						{
							if (value != Debris.DebrisType.NUMBERS)
							{
								if (d.itemId.Value != null)
								{
									ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(d.itemId.Value);
									Texture2D texture = itemData.GetTexture();
									float scale = (d.debrisType.Value == Debris.DebrisType.RESOURCE || d.floppingFish.Value) ? 4f : (4f * (0.8f + (float)d.itemQuality * 0.1f));
									for (int i = 0; i < d.Chunks.Count; i++)
									{
										Chunk chunk = d.Chunks[i];
										Vector2 position2 = chunk.GetVisualPosition();
										Microsoft.Xna.Framework.Rectangle sourceRect = (d.debrisType.Value == Debris.DebrisType.RESOURCE) ? itemData.GetSourceRect(chunk.randomOffset, null) : itemData.GetSourceRect(0, null);
										SpriteEffects spriteEffect = (d.floppingFish.Value && chunk.bounces % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
										b.Draw(texture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position2)), new Microsoft.Xna.Framework.Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, scale, spriteEffect, ((float)(d.chunkFinalYLevel + 32) + position2.X / 10000f) / 10000f);
										b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(position2.X + 25.6f, (d.chunksMoveTowardPlayer ? (position2.Y + 8f) : ((float)d.chunkFinalYLevel)) + 32f + (float)(12 * d.itemQuality)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.75f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (d.chunksMoveTowardPlayer ? 0f : (((float)d.chunkFinalYLevel - position2.Y) / 96f))), SpriteEffects.None, (float)d.chunkFinalYLevel / 10000f);
									}
								}
								else
								{
									for (int j = 0; j < d.Chunks.Count; j++)
									{
										Vector2 position3 = Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[j].position.Value));
										Microsoft.Xna.Framework.Rectangle sourceRect2 = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, d.chunkType.Value + d.Chunks[j].randomOffset, 16, 16);
										float drawLayer = (d.Chunks[j].position.Y + 128f + d.Chunks[j].position.X / 10000f) / 10000f;
										b.Draw(Game1.debrisSpriteSheet, position3, new Microsoft.Xna.Framework.Rectangle?(sourceRect2), d.chunksColor.Value, 0f, Vector2.Zero, 4f * d.scale.Value, SpriteEffects.None, drawLayer);
									}
								}
							}
							else
							{
								Chunk chunk2 = d.Chunks[0];
								Vector2 position4 = chunk2.GetVisualPosition();
								NumberSprite.draw(d.chunkType.Value, b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(new Vector2(position4.X, (float)d.chunkFinalYLevel - ((float)d.chunkFinalYLevel - position4.Y)))), d.nonSpriteChunkColor.Value, chunk2.scale * 0.75f, 0.98f + 0.0001f * (float)counter, chunk2.alpha, -1 * (int)((float)d.chunkFinalYLevel - position4.Y) / 2, 0);
							}
						}
						else
						{
							for (int k = 0; k < d.Chunks.Count; k++)
							{
								Chunk chunk3 = d.Chunks[0];
								Vector2 position5 = chunk3.GetVisualPosition();
								b.Draw(d.spriteChunkSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position5)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(chunk3.xSpriteSheet.Value, chunk3.ySpriteSheet.Value, Math.Min(d.sizeOfSourceRectSquares.Value, d.spriteChunkSheet.Bounds.Width), Math.Min(d.sizeOfSourceRectSquares.Value, d.spriteChunkSheet.Bounds.Height))), d.nonSpriteChunkColor.Value * chunk3.alpha, chunk3.rotation, new Vector2((float)(d.sizeOfSourceRectSquares.Value / 2), (float)(d.sizeOfSourceRectSquares.Value / 2)), chunk3.scale, SpriteEffects.None, ((float)(d.chunkFinalYLevel + 16) + position5.X / 10000f) / 10000f);
							}
						}
					}
					else
					{
						Chunk chunk4 = d.Chunks[0];
						Vector2 position6 = chunk4.GetVisualPosition();
						Game1.drawWithBorder(d.debrisMessage.Value, Color.Black, d.nonSpriteChunkColor.Value, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position6)), chunk4.rotation, chunk4.scale, (position6.Y + 64f) / 10000f);
					}
				}
			}
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x000C057C File Offset: 0x000BE77C
		public virtual bool shouldHideCharacters()
		{
			return false;
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x000C0580 File Offset: 0x000BE780
		protected virtual void drawCharacters(SpriteBatch b)
		{
			if (this.shouldHideCharacters())
			{
				return;
			}
			if (!Game1.eventUp || (Game1.CurrentEvent != null && Game1.CurrentEvent.showWorldCharacters))
			{
				for (int i = 0; i < this.characters.Count; i++)
				{
					if (this.characters[i] != null)
					{
						this.characters[i].draw(b);
					}
				}
			}
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x000C05E8 File Offset: 0x000BE7E8
		protected virtual void drawFarmers(SpriteBatch b)
		{
			if (this.shouldHideCharacters())
			{
				return;
			}
			if (Game1.currentMinigame == null)
			{
				if (this.currentEvent == null || this.currentEvent.isFestival || this.currentEvent.farmerActors.Count == 0)
				{
					using (FarmerCollection.Enumerator enumerator = this.farmers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Farmer farmer = enumerator.Current;
							if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
							{
								farmer.draw(b);
							}
						}
						return;
					}
				}
				this.currentEvent.drawFarmers(b);
			}
		}

		// Token: 0x06000FEB RID: 4075 RVA: 0x000C0690 File Offset: 0x000BE890
		public virtual void DrawFarmerUsernames(SpriteBatch b)
		{
			if (this.shouldHideCharacters())
			{
				return;
			}
			if (Game1.currentMinigame == null && (this.currentEvent == null || this.currentEvent.isFestival || this.currentEvent.farmerActors.Count == 0))
			{
				foreach (Farmer farmer in this.farmers)
				{
					if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
					{
						farmer.DrawUsername(b);
					}
				}
			}
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x000C072C File Offset: 0x000BE92C
		public virtual void draw(SpriteBatch b)
		{
			if (this.animals.Length > 0)
			{
				foreach (FarmAnimal farmAnimal in this.animals.Values)
				{
					farmAnimal.draw(b);
				}
			}
			if (this.mapSeats.Count > 0)
			{
				foreach (MapSeat mapSeat in this.mapSeats)
				{
					mapSeat.Draw(b);
				}
			}
			Microsoft.Xna.Framework.Rectangle viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
			viewportRect.Inflate(128, 128);
			if (this is Woods && Game1.eventUp)
			{
				Event @event = this.currentEvent;
				if (@event == null || !@event.showGroundObjects)
				{
					goto IL_14D;
				}
			}
			if (this.resourceClumps.Count > 0)
			{
				foreach (ResourceClump r in this.resourceClumps)
				{
					if (r.getRenderBounds().Intersects(viewportRect))
					{
						r.draw(b);
					}
				}
			}
			IL_14D:
			this._currentLocationFarmersForDisambiguating.Clear();
			foreach (Farmer farmer in this.farmers)
			{
				farmer.drawLayerDisambiguator = 0f;
				this._currentLocationFarmersForDisambiguating.Add(farmer);
			}
			if (this._currentLocationFarmersForDisambiguating.Contains(Game1.player))
			{
				this._currentLocationFarmersForDisambiguating.Remove(Game1.player);
				this._currentLocationFarmersForDisambiguating.Insert(0, Game1.player);
			}
			float disambiguator_amount = 0.0001f;
			for (int i = 0; i < this._currentLocationFarmersForDisambiguating.Count; i++)
			{
				for (int j = i + 1; j < this._currentLocationFarmersForDisambiguating.Count; j++)
				{
					Farmer farmer2 = this._currentLocationFarmersForDisambiguating[i];
					Farmer other_farmer = this._currentLocationFarmersForDisambiguating[j];
					if (!other_farmer.IsSitting() && Math.Abs(farmer2.getDrawLayer() - other_farmer.getDrawLayer()) < disambiguator_amount && Math.Abs(farmer2.position.X - other_farmer.position.X) < 64f)
					{
						other_farmer.drawLayerDisambiguator += farmer2.getDrawLayer() - disambiguator_amount - other_farmer.getDrawLayer();
					}
				}
			}
			this.drawCharacters(b);
			this.drawFarmers(b);
			if (this.critters != null && Game1.farmEvent == null)
			{
				for (int k = 0; k < this.critters.Count; k++)
				{
					this.critters[k].draw(b);
				}
			}
			this.drawDebris(b);
			if ((!Game1.eventUp || (this.currentEvent != null && this.currentEvent.showGroundObjects)) && this.objects.Length > 0)
			{
				Vector2 tile = default(Vector2);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 3; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
					{
						tile.X = (float)x;
						tile.Y = (float)y;
						Object o;
						if (this.objects.TryGetValue(tile, out o))
						{
							o.draw(b, (int)tile.X, (int)tile.Y, 1f);
						}
					}
				}
			}
			if (this.TemporarySprites.Count > 0)
			{
				foreach (TemporaryAnimatedSprite s in this.TemporarySprites)
				{
					if (!s.drawAboveAlwaysFront)
					{
						s.draw(b, false, 0, 0, 1f);
					}
				}
			}
			this.interiorDoors.Draw(b);
			NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
			if (netCollection != null && netCollection.Count > 0)
			{
				foreach (LargeTerrainFeature f in this.largeTerrainFeatures)
				{
					if (f.getRenderBounds().Intersects(viewportRect))
					{
						f.draw(b);
					}
				}
			}
			if (this.buildings.Count > 0)
			{
				int borderBuffer = 1;
				viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - borderBuffer, Game1.viewport.Y / 64 - borderBuffer, (int)Math.Ceiling((double)((float)Game1.viewport.Width / 64f)) + 2 * borderBuffer, (int)Math.Ceiling((double)((float)Game1.viewport.Height / 64f)) + 3 + 2 * borderBuffer);
				Microsoft.Xna.Framework.Rectangle objectRectangle = default(Microsoft.Xna.Framework.Rectangle);
				foreach (Building building in this.buildings)
				{
					int additionalRadius = building.GetAdditionalTilePropertyRadius();
					Microsoft.Xna.Framework.Rectangle sourceRect = building.getSourceRect();
					objectRectangle.X = building.tileX.Value - additionalRadius;
					objectRectangle.Width = building.tilesWide.Value + additionalRadius * 2;
					int bottomY = building.tileY.Value + building.tilesHigh.Value + additionalRadius;
					int topY = bottomY - (int)Math.Ceiling((double)((float)sourceRect.Height * 4f / 64f)) - additionalRadius;
					objectRectangle.Y = topY;
					objectRectangle.Height = bottomY - topY;
					if (objectRectangle.Intersects(viewportRect))
					{
						building.draw(b);
					}
				}
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.fishSplashAnimation;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.orePanAnimation;
			if (temporaryAnimatedSprite2 != null)
			{
				temporaryAnimatedSprite2.draw(b, false, 0, 0, 1f);
			}
			if (!Game1.eventUp || this is Farm || this is FarmHouse)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture f2 in this.furniture)
				{
					if (f2.furniture_type.Value != 12)
					{
						f2.draw(b, -1, -1, 1f);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
			if (this.showDropboxIndicator && !Game1.eventUp)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.dropBoxIndicatorLocation.X, this.dropBoxIndicatorLocation.Y + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10)), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
			if (this.lightGlows.Count > 0)
			{
				this.drawLightGlows(b);
			}
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x000C0F0C File Offset: 0x000BF10C
		public virtual void drawOverlays(SpriteBatch b)
		{
		}

		// Token: 0x06000FEE RID: 4078 RVA: 0x000C0F10 File Offset: 0x000BF110
		public virtual void drawAboveFrontLayer(SpriteBatch b)
		{
			Vector2 tile = default(Vector2);
			for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
			{
				for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
				{
					tile.X = (float)x;
					tile.Y = (float)y;
					TerrainFeature feat;
					if (this.terrainFeatures.TryGetValue(tile, out feat) && !(feat is Flooring))
					{
						feat.draw(b);
					}
				}
			}
		}

		// Token: 0x06000FEF RID: 4079 RVA: 0x000C0FC0 File Offset: 0x000BF1C0
		public virtual void drawLightGlows(SpriteBatch b)
		{
			foreach (Vector2 v in this.lightGlows)
			{
				if (!this.lightGlowLayerCache.ContainsKey(v))
				{
					Furniture f = this.GetFurnitureAt(new Vector2((float)((int)(v.X / 64f)), (float)((int)(v.Y / 64f) + 2)));
					if (f != null && f.sourceRect.Height / 16 - f.getTilesHigh() > 1)
					{
						this.lightGlowLayerCache.Add(v, 2.5f);
					}
					else
					{
						FarmHouse farmhouse = this as FarmHouse;
						if (farmhouse != null && farmhouse.upgradeLevel > 0)
						{
							Vector2 tileV = new Vector2((float)((int)(v.X / 64f)), (float)((int)(v.Y / 64f)));
							Vector2 diff = Utility.PointToVector2(farmhouse.getKitchenStandingSpot()) - tileV;
							if (diff.Y == 3f && (diff.X == 2f || diff.X == 3f || diff.X == -1f || diff.X == -2f))
							{
								this.lightGlowLayerCache.Add(v, 1.5f);
							}
							else
							{
								this.lightGlowLayerCache.Add(v, 10f);
							}
						}
						else
						{
							this.lightGlowLayerCache.Add(v, 10f);
						}
					}
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(21, 1695, 41, 67)), Color.White, 0f, new Vector2(19f, 22f), 4f, SpriteEffects.None, (v.Y + 64f * this.lightGlowLayerCache[v]) / 10000f);
			}
		}

		// Token: 0x06000FF0 RID: 4080 RVA: 0x000C11BC File Offset: 0x000BF3BC
		public Object tryToCreateUnseenSecretNote(Farmer who)
		{
			if (this.currentEvent != null && this.currentEvent.isFestival)
			{
				return null;
			}
			bool journal = this.InIslandContext();
			if (!journal && (who == null || !who.hasMagnifyingGlass))
			{
				return null;
			}
			string noteItemId = journal ? "(O)842" : "(O)79";
			int totalNotes;
			int totalUnseen = Utility.GetUnseenSecretNotes(who, journal, out totalNotes).Length - who.Items.CountId(noteItemId);
			if (totalUnseen <= 0)
			{
				return null;
			}
			float fractionOfNotesRemaining = (float)(totalUnseen - 1) / (float)Math.Max(1, totalNotes - 1);
			float chanceForNewNote = GameLocation.LAST_SECRET_NOTE_CHANCE + (GameLocation.FIRST_SECRET_NOTE_CHANCE - GameLocation.LAST_SECRET_NOTE_CHANCE) * fractionOfNotesRemaining;
			if (!Game1.random.NextBool(chanceForNewNote))
			{
				return null;
			}
			return ItemRegistry.Create<Object>(noteItemId, 1, 0, false);
		}

		// Token: 0x06000FF1 RID: 4081 RVA: 0x000C1268 File Offset: 0x000BF468
		public virtual bool performToolAction(Tool t, int tileX, int tileY)
		{
			MeleeWeapon weapon = t as MeleeWeapon;
			if (weapon != null)
			{
				foreach (FarmAnimal animal in this.animals.Values)
				{
					if (animal.GetBoundingBox().Intersects(weapon.mostRecentArea))
					{
						animal.hitWithWeapon(weapon);
					}
				}
			}
			foreach (Building building in this.buildings)
			{
				if (building.occupiesTile(new Vector2((float)tileX, (float)tileY), false))
				{
					building.performToolAction(t, tileX, tileY);
				}
			}
			for (int i = this.resourceClumps.Count - 1; i >= 0; i--)
			{
				if (this.resourceClumps[i] != null && this.resourceClumps[i].getBoundingBox().Contains(tileX * 64, tileY * 64) && this.resourceClumps[i].performToolAction(t, 1, this.resourceClumps[i].Tile))
				{
					this.resourceClumps.RemoveAt(i);
					return true;
				}
			}
			Microsoft.Xna.Framework.Rectangle toolArea = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
			foreach (LargeTerrainFeature ltf in this.largeTerrainFeatures)
			{
				if (ltf.getBoundingBox().Intersects(toolArea))
				{
					ltf.performToolAction(t, 1, new Vector2((float)tileX, (float)tileY));
				}
			}
			return false;
		}

		// Token: 0x06000FF2 RID: 4082 RVA: 0x000C1440 File Offset: 0x000BF640
		public virtual void seasonUpdate(bool onLoad = false)
		{
			Season season = this.GetSeason();
			this.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value.seasonUpdate(onLoad));
			NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
			if (netCollection != null)
			{
				netCollection.RemoveWhere((LargeTerrainFeature feature) => feature.seasonUpdate(onLoad));
			}
			foreach (NPC i in this.characters)
			{
				if (!i.IsMonster)
				{
					i.resetSeasonalDialogue();
				}
			}
			if (this.IsOutdoors && !onLoad)
			{
				foreach (KeyValuePair<Vector2, Object> pair2 in this.objects.Pairs.ToArray<KeyValuePair<Vector2, Object>>())
				{
					Vector2 tile = pair2.Key;
					Object obj = pair2.Value;
					if (obj.IsSpawnedObject && !obj.IsBreakableStone())
					{
						this.objects.Remove(tile);
					}
					else if (obj.QualifiedItemId == "(O)590" && this.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "Diggable", "Back") == "")
					{
						this.objects.Remove(tile);
					}
				}
				this.numberOfSpawnedObjectsOnMap = 0;
			}
			switch (season)
			{
			case Season.Spring:
				this.waterColor.Value = new Color(120, 200, 255) * 0.5f;
				break;
			case Season.Summer:
				this.waterColor.Value = new Color(60, 240, 255) * 0.5f;
				break;
			case Season.Fall:
				this.waterColor.Value = new Color(255, 130, 200) * 0.5f;
				break;
			case Season.Winter:
				this.waterColor.Value = new Color(130, 80, 255) * 0.5f;
				break;
			}
			if (!onLoad && season == Season.Spring && Game1.stats.DaysPlayed > 1U && !(this is Farm))
			{
				this.loadWeeds();
			}
		}

		// Token: 0x06000FF3 RID: 4083 RVA: 0x000C169C File Offset: 0x000BF89C
		public List<FarmAnimal> getAllFarmAnimals()
		{
			List<FarmAnimal> farmAnimals = this.animals.Values.ToList<FarmAnimal>();
			foreach (Building building in this.buildings)
			{
				GameLocation interior = building.GetIndoors();
				if (interior != null)
				{
					farmAnimals.AddRange(interior.animals.Values);
				}
			}
			return farmAnimals;
		}

		// Token: 0x06000FF4 RID: 4084 RVA: 0x000C1720 File Offset: 0x000BF920
		public virtual int GetHayCapacity()
		{
			int totalCapacity = 0;
			foreach (Building building in this.buildings)
			{
				if (building.hayCapacity.Value > 0 && building.daysOfConstructionLeft.Value <= 0)
				{
					totalCapacity += building.hayCapacity.Value;
				}
			}
			return totalCapacity;
		}

		// Token: 0x06000FF5 RID: 4085 RVA: 0x000C179C File Offset: 0x000BF99C
		public bool CheckPetAnimal(Vector2 position, Farmer who)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (!animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					animal.pet(who, false);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF6 RID: 4086 RVA: 0x000C182C File Offset: 0x000BFA2C
		public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (!animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
				{
					animal.pet(who, false);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF7 RID: 4087 RVA: 0x000C18B0 File Offset: 0x000BFAB0
		public bool CheckInspectAnimal(Vector2 position, Farmer who)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					animal.pet(who, false);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF8 RID: 4088 RVA: 0x000C1940 File Offset: 0x000BFB40
		public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
				{
					animal.pet(who, false);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x000C19C4 File Offset: 0x000BFBC4
		public virtual void updateSeasonalTileSheets(Map map = null)
		{
			if (map == null)
			{
				map = this.Map;
			}
			if (!(this is Summit) && (!this.IsOutdoors || this.Name.Equals("Desert")))
			{
				return;
			}
			map.DisposeTileSheets(Game1.mapDisplayDevice);
			foreach (TileSheet tilesheet in map.TileSheets)
			{
				string prevImageSource = tilesheet.ImageSource;
				try
				{
					tilesheet.ImageSource = GameLocation.GetSeasonalTilesheetName(tilesheet.ImageSource, this.GetSeasonKey());
					Game1.mapDisplayDevice.LoadTileSheet(tilesheet);
				}
				catch (Exception ex)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Location '");
					defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
					defaultInterpolatedStringHandler.AppendLiteral("' failed to load seasonal asset name '");
					defaultInterpolatedStringHandler.AppendFormatted(tilesheet.ImageSource);
					defaultInterpolatedStringHandler.AppendLiteral("' for tilesheet ID '");
					defaultInterpolatedStringHandler.AppendFormatted(tilesheet.Id);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					tilesheet.ImageSource = prevImageSource;
				}
			}
			map.LoadTileSheets(Game1.mapDisplayDevice);
		}

		// Token: 0x06000FFA RID: 4090 RVA: 0x000C1B08 File Offset: 0x000BFD08
		public static string GetSeasonalTilesheetName(string sheet_path, string current_season)
		{
			string file_name = Path.GetFileName(sheet_path);
			if (file_name.StartsWith("spring_") || file_name.StartsWith("summer_") || file_name.StartsWith("fall_") || file_name.StartsWith("winter_"))
			{
				sheet_path = Path.Combine(Path.GetDirectoryName(sheet_path), current_season + file_name.Substring(file_name.IndexOf('_')));
			}
			return sheet_path;
		}

		// Token: 0x06000FFB RID: 4091 RVA: 0x000C1B72 File Offset: 0x000BFD72
		public virtual string checkEventPrecondition(string precondition)
		{
			return this.checkEventPrecondition(precondition, true);
		}

		// Token: 0x06000FFC RID: 4092 RVA: 0x000C1B7C File Offset: 0x000BFD7C
		public virtual string checkEventPrecondition(string precondition, bool check_seen)
		{
			string[] split = Event.SplitPreconditions(precondition);
			string eventId = split[0];
			if (string.IsNullOrEmpty(eventId) || eventId == "-1")
			{
				return "-1";
			}
			if (check_seen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
			{
				return "-1";
			}
			for (int i = 1; i < split.Length; i++)
			{
				if (!string.IsNullOrEmpty(split[i]) && !Event.CheckPrecondition(this, split[0], split[i]))
				{
					return "-1";
				}
			}
			return eventId;
		}

		// Token: 0x06000FFD RID: 4093 RVA: 0x000C1C04 File Offset: 0x000BFE04
		public static Object GetHayFromAnySilo(GameLocation currentLocation)
		{
			Object hay;
			if (GameLocation.<GetHayFromAnySilo>g__TryGetHayFrom|574_0(currentLocation, out hay))
			{
				return hay;
			}
			if (currentLocation.Name != "Farm" && GameLocation.<GetHayFromAnySilo>g__TryGetHayFrom|574_0(Game1.getFarm(), out hay))
			{
				return hay;
			}
			Utility.ForEachLocation((GameLocation location) => !GameLocation.<GetHayFromAnySilo>g__TryGetHayFrom|574_0(location, out hay), false, false);
			return hay;
		}

		// Token: 0x06000FFE RID: 4094 RVA: 0x000C1C70 File Offset: 0x000BFE70
		public static int StoreHayInAnySilo(int count, GameLocation currentLocation)
		{
			count = currentLocation.tryToAddHay(count);
			if (count > 0 && currentLocation.Name != "Farm")
			{
				count = Game1.getFarm().tryToAddHay(count);
				if (count <= 0)
				{
					return 0;
				}
			}
			if (count > 0)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location.buildings.Count > 0)
					{
						count = location.tryToAddHay(count);
						return count > 0;
					}
					return true;
				}, false, false);
			}
			if (count <= 0)
			{
				return 0;
			}
			return count;
		}

		// Token: 0x06000FFF RID: 4095 RVA: 0x000C1D08 File Offset: 0x000BFF08
		public int tryToAddHay(int num)
		{
			int piecesToAdd = Math.Min(this.GetHayCapacity() - this.piecesOfHay.Value, num);
			this.piecesOfHay.Value += piecesToAdd;
			return num - piecesToAdd;
		}

		// Token: 0x06001000 RID: 4096 RVA: 0x000C1D44 File Offset: 0x000BFF44
		public Building getBuildingAt(Vector2 tile)
		{
			foreach (Building building in this.buildings)
			{
				if (building.occupiesTile(tile, false) || !building.isTilePassable(tile))
				{
					return building;
				}
			}
			return null;
		}

		// Token: 0x06001001 RID: 4097 RVA: 0x000C1DAC File Offset: 0x000BFFAC
		public Building getBuildingByType(string type)
		{
			if (type != null)
			{
				foreach (Building building in this.buildings)
				{
					if (string.Equals(building.buildingType.Value, type, StringComparison.Ordinal))
					{
						return building;
					}
				}
			}
			return null;
		}

		// Token: 0x06001002 RID: 4098 RVA: 0x000C1E18 File Offset: 0x000C0018
		public Building getBuildingById(Guid id)
		{
			if (id != Guid.Empty)
			{
				foreach (Building building in this.buildings)
				{
					if (building.id.Value == id)
					{
						return building;
					}
				}
			}
			return null;
		}

		// Token: 0x06001003 RID: 4099 RVA: 0x000C1E8C File Offset: 0x000C008C
		public Building getBuildingByName(string name)
		{
			if (name != null)
			{
				foreach (Building building in this.buildings)
				{
					if (building.HasIndoorsName(name))
					{
						return building;
					}
				}
			}
			return null;
		}

		// Token: 0x06001004 RID: 4100 RVA: 0x000C1EEC File Offset: 0x000C00EC
		public bool destroyStructure(Vector2 tile)
		{
			Building building = this.getBuildingAt(tile);
			return building != null && this.destroyStructure(building);
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x000C1F0D File Offset: 0x000C010D
		public bool destroyStructure(Building building)
		{
			if (this.buildings.Remove(building))
			{
				building.performActionOnDemolition(this);
				Game1.player.team.SendBuildingDemolishedEvent(this, building);
				return true;
			}
			return false;
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x000C1F38 File Offset: 0x000C0138
		public bool buildStructure(Building building, Vector2 tileLocation, Farmer who, bool skipSafetyChecks = false)
		{
			if (!skipSafetyChecks)
			{
				for (int y = 0; y < building.tilesHigh.Value; y++)
				{
					for (int x = 0; x < building.tilesWide.Value; x++)
					{
						this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
					}
				}
				foreach (BuildingPlacementTile buildingPlacementTile in building.GetAdditionalPlacementTiles())
				{
					foreach (Point areaTile in buildingPlacementTile.TileArea.GetPoints())
					{
						this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)areaTile.X, tileLocation.Y + (float)areaTile.Y));
					}
				}
				for (int y2 = 0; y2 < building.tilesHigh.Value; y2++)
				{
					for (int x2 = 0; x2 < building.tilesWide.Value; x2++)
					{
						Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
						if (!this.buildings.Contains(building) || !building.occupiesTile(currentGlobalTilePosition, false))
						{
							if (!this.isBuildable(currentGlobalTilePosition, false))
							{
								return false;
							}
							using (FarmerCollection.Enumerator enumerator3 = this.farmers.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									if (enumerator3.Current.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
									{
										return false;
									}
								}
							}
						}
					}
				}
				foreach (BuildingPlacementTile buildingPlacementTile2 in building.GetAdditionalPlacementTiles())
				{
					bool onlyNeedsToBePassable = buildingPlacementTile2.OnlyNeedsToBePassable;
					foreach (Point point in buildingPlacementTile2.TileArea.GetPoints())
					{
						int x3 = point.X;
						int y3 = point.Y;
						Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3);
						if (!this.buildings.Contains(building) || !building.occupiesTile(currentGlobalTilePosition2, false))
						{
							if (!this.isBuildable(currentGlobalTilePosition2, onlyNeedsToBePassable))
							{
								return false;
							}
							if (!onlyNeedsToBePassable)
							{
								using (FarmerCollection.Enumerator enumerator3 = this.farmers.GetEnumerator())
								{
									while (enumerator3.MoveNext())
									{
										if (enumerator3.Current.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x3 * 64, y3 * 64, 64, 64)))
										{
											return false;
										}
									}
								}
							}
						}
					}
				}
				if (building.humanDoor.Value != new Point(-1, -1))
				{
					Vector2 doorPos = tileLocation + new Vector2((float)building.humanDoor.X, (float)(building.humanDoor.Y + 1));
					if ((!this.buildings.Contains(building) || !building.occupiesTile(doorPos, false)) && !this.isBuildable(doorPos, false) && !this.isPath(doorPos))
					{
						return false;
					}
				}
				string finalCheckResult = building.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (finalCheckResult != null)
				{
					Game1.addHUDMessage(new HUDMessage(finalCheckResult, 3));
					return false;
				}
			}
			building.tileX.Value = (int)tileLocation.X;
			building.tileY.Value = (int)tileLocation.Y;
			for (int y4 = 0; y4 < building.tilesHigh.Value; y4++)
			{
				int x4 = 0;
				while (x4 < building.tilesWide.Value)
				{
					Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y4);
					if (!(this.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition3, null) is Flooring))
					{
						goto IL_3FF;
					}
					BuildingData data = building.GetData();
					if (!(((data != null) ? new bool?(data.AllowsFlooringUnderneath) : null) ?? false))
					{
						goto IL_3FF;
					}
					IL_40D:
					x4++;
					continue;
					IL_3FF:
					this.terrainFeatures.Remove(currentGlobalTilePosition3);
					goto IL_40D;
				}
			}
			if (!this.buildings.Contains(building))
			{
				this.buildings.Add(building);
				who.team.SendBuildingConstructedEvent(this, building, who);
			}
			GameLocation interior = building.GetIndoors();
			AnimalHouse animalHouse = interior as AnimalHouse;
			if (animalHouse != null)
			{
				foreach (long animalId in animalHouse.animalsThatLiveHere)
				{
					FarmAnimal animal = Utility.getAnimal(animalId);
					if (animal != null)
					{
						animal.homeInterior = interior;
					}
					else if (animalHouse.animals.TryGetValue(animalId, out animal))
					{
						animal.homeInterior = interior;
					}
				}
			}
			if (interior != null)
			{
				foreach (Warp warp in interior.warps)
				{
					if (warp.TargetName == this.NameOrUniqueName)
					{
						warp.TargetX = building.humanDoor.X + building.tileX.Value;
						warp.TargetY = building.humanDoor.Y + building.tileY.Value + 1;
					}
				}
			}
			for (int y5 = 0; y5 < building.tilesHigh.Value; y5++)
			{
				for (int x5 = 0; x5 < building.tilesWide.Value; x5++)
				{
					this.<buildStructure>g__RemoveArtifactSpots|583_0(new Vector2(tileLocation.X + (float)x5, tileLocation.Y + (float)y5));
				}
			}
			foreach (BuildingPlacementTile area in building.GetAdditionalPlacementTiles())
			{
				if (!area.OnlyNeedsToBePassable)
				{
					foreach (Point areaTile2 in area.TileArea.GetPoints())
					{
						this.<buildStructure>g__RemoveArtifactSpots|583_0(new Vector2(tileLocation.X + (float)areaTile2.X, tileLocation.Y + (float)areaTile2.Y));
					}
				}
			}
			return true;
		}

		// Token: 0x06001007 RID: 4103 RVA: 0x000C2688 File Offset: 0x000C0888
		public bool buildStructure(string typeId, BuildingData data, Vector2 tileLocation, Farmer who, out Building constructed, bool magicalConstruction = false, bool skipSafetyChecks = false)
		{
			if (data == null || (!skipSafetyChecks && !this.IsBuildableLocation()))
			{
				constructed = null;
				return false;
			}
			int tilesWide = data.Size.X;
			int tilesHigh = data.Size.Y;
			List<BuildingPlacementTile> additionalPlacementTiles = data.AdditionalPlacementTiles ?? new List<BuildingPlacementTile>(0);
			if (!skipSafetyChecks)
			{
				for (int y = 0; y < tilesHigh; y++)
				{
					for (int x = 0; x < tilesWide; x++)
					{
						this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
					}
				}
				foreach (BuildingPlacementTile buildingPlacementTile in additionalPlacementTiles)
				{
					foreach (Point areaTile in buildingPlacementTile.TileArea.GetPoints())
					{
						this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)areaTile.X, tileLocation.Y + (float)areaTile.Y));
					}
				}
				for (int y2 = 0; y2 < tilesHigh; y2++)
				{
					for (int x2 = 0; x2 < tilesWide; x2++)
					{
						Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
						if (!this.isBuildable(currentGlobalTilePosition, false))
						{
							constructed = null;
							return false;
						}
						using (FarmerCollection.Enumerator enumerator3 = this.farmers.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								if (enumerator3.Current.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
								{
									constructed = null;
									return false;
								}
							}
						}
					}
				}
				foreach (BuildingPlacementTile buildingPlacementTile2 in additionalPlacementTiles)
				{
					bool onlyNeedsToBePassable = buildingPlacementTile2.OnlyNeedsToBePassable;
					foreach (Point point in buildingPlacementTile2.TileArea.GetPoints())
					{
						int x3 = point.X;
						int y3 = point.Y;
						Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3);
						if (!this.isBuildable(currentGlobalTilePosition2, onlyNeedsToBePassable))
						{
							constructed = null;
							return false;
						}
						if (!onlyNeedsToBePassable)
						{
							using (FarmerCollection.Enumerator enumerator3 = this.farmers.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									if (enumerator3.Current.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x3 * 64, y3 * 64, 64, 64)))
									{
										constructed = null;
										return false;
									}
								}
							}
						}
					}
				}
				if (data.HumanDoor != new Point(-1, -1))
				{
					Vector2 doorPos = tileLocation + new Vector2((float)data.HumanDoor.X, (float)(data.HumanDoor.Y + 1));
					if (!this.isBuildable(doorPos, true) && !this.isPath(doorPos))
					{
						constructed = null;
						return false;
					}
				}
			}
			Building building = Building.CreateInstanceFromId(typeId, tileLocation);
			if (magicalConstruction)
			{
				building.magical.Value = true;
				building.daysOfConstructionLeft.Value = 0;
			}
			building.owner.Value = who.UniqueMultiplayerID;
			if (!skipSafetyChecks)
			{
				string finalCheckResult = building.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (finalCheckResult != null)
				{
					Game1.addHUDMessage(new HUDMessage(finalCheckResult, 3));
					constructed = null;
					return false;
				}
			}
			for (int y4 = 0; y4 < building.tilesHigh.Value; y4++)
			{
				int x4 = 0;
				while (x4 < building.tilesWide.Value)
				{
					Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y4);
					if (!(this.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition3, null) is Flooring))
					{
						goto IL_3F5;
					}
					BuildingData data2 = building.GetData();
					if (!(((data2 != null) ? new bool?(data2.AllowsFlooringUnderneath) : null) ?? false))
					{
						goto IL_3F5;
					}
					IL_403:
					x4++;
					continue;
					IL_3F5:
					this.terrainFeatures.Remove(currentGlobalTilePosition3);
					goto IL_403;
				}
			}
			this.buildings.Add(building);
			who.team.SendBuildingConstructedEvent(this, building, who);
			string chatKey = magicalConstruction ? "BuildingMagicBuild" : "BuildingBuild";
			Game1.multiplayer.globalChatInfoMessage(chatKey, new string[]
			{
				Game1.player.Name,
				"aOrAn:" + data.Name,
				data.Name,
				Game1.player.farmName.Value
			});
			constructed = building;
			return true;
		}

		// Token: 0x06001008 RID: 4104 RVA: 0x000C2B98 File Offset: 0x000C0D98
		public bool buildStructure(string typeId, Vector2 tileLocation, Farmer who, out Building constructed, bool magicalConstruction = false, bool skipSafetyChecks = false)
		{
			BuildingData buildingData;
			if (typeId == null || !Game1.buildingData.TryGetValue(typeId, out buildingData))
			{
				Game1.log.Error("Can't construct building '" + typeId + "', no data found matching that ID.", null);
				constructed = null;
				return false;
			}
			return this.buildStructure(typeId, buildingData, tileLocation, who, out constructed, magicalConstruction, skipSafetyChecks);
		}

		// Token: 0x06001009 RID: 4105 RVA: 0x000C2BE8 File Offset: 0x000C0DE8
		public bool isBuildingConstructed(string name)
		{
			return this.getNumberBuildingsConstructed(name, false) > 0;
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x000C2BF5 File Offset: 0x000C0DF5
		public bool HasMinBuildings(string buildingType, int minCount)
		{
			return this.getNumberBuildingsConstructed(buildingType, false) >= minCount;
		}

		// Token: 0x0600100B RID: 4107 RVA: 0x000C2C08 File Offset: 0x000C0E08
		public bool HasMinBuildings(Func<Building, bool> match, int minCount)
		{
			if (minCount <= 0)
			{
				return true;
			}
			int count = 0;
			foreach (Building building in this.buildings)
			{
				if (match(building))
				{
					count++;
				}
				if (count >= minCount)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x000C2C74 File Offset: 0x000C0E74
		public int getNumberBuildingsConstructed(bool includeUnderConstruction = false)
		{
			if (includeUnderConstruction || this.buildings.Count == 0)
			{
				return this.buildings.Count;
			}
			int count = 0;
			using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.isUnderConstruction(true))
					{
						count++;
					}
				}
			}
			return count;
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x000C2CEC File Offset: 0x000C0EEC
		public int getNumberBuildingsConstructed(string name, bool includeUnderConstruction = false)
		{
			int count = 0;
			if (this.buildings.Count > 0)
			{
				foreach (Building building in this.buildings)
				{
					if (building.buildingType.Value == name && (includeUnderConstruction || !building.isUnderConstruction(true)))
					{
						count++;
					}
				}
			}
			return count;
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x000C2D6C File Offset: 0x000C0F6C
		public bool isThereABuildingUnderConstruction()
		{
			if (this.buildings.Count > 0)
			{
				using (List<Building>.Enumerator enumerator = this.buildings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.isUnderConstruction(true))
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x000C2DD4 File Offset: 0x000C0FD4
		public IEnumerable<GameLocation> GetInstancedBuildingInteriors()
		{
			List<GameLocation> interiors = null;
			this.ForEachInstancedInterior(delegate(GameLocation location)
			{
				if (interiors == null)
				{
					interiors = new List<GameLocation>();
				}
				interiors.Add(location);
				return true;
			});
			if (interiors == null)
			{
				return LegacyShims.EmptyArray<GameLocation>();
			}
			return interiors;
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x000C2E14 File Offset: 0x000C1014
		public void ForEachInstancedInterior(Func<GameLocation, bool> action)
		{
			foreach (Building building in this.buildings)
			{
				if (building.GetIndoorsType() == IndoorsType.Instanced)
				{
					GameLocation indoors = building.GetIndoors();
					if (indoors != null && !action(indoors))
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001011 RID: 4113 RVA: 0x000C2E80 File Offset: 0x000C1080
		public void ForEachDirt(Func<HoeDirt, bool> action, bool includeGardenPots = true)
		{
			foreach (TerrainFeature terrainFeature in this.terrainFeatures.Values)
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null && !action(dirt))
				{
					return;
				}
			}
			if (includeGardenPots)
			{
				foreach (Object @object in this.objects.Values)
				{
					IndoorPot pot = @object as IndoorPot;
					if (pot != null && pot.bush.Value == null && !action(pot.hoeDirt.Value))
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x000C2F58 File Offset: 0x000C1158
		public bool isPath(Vector2 tileLocation)
		{
			TerrainFeature terrainFeature;
			Object obj;
			return this.terrainFeatures.TryGetValue(tileLocation, out terrainFeature) && terrainFeature != null && terrainFeature.isPassable(null) && (!this.objects.TryGetValue(tileLocation, out obj) || obj == null || obj.isPassable());
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x000C2FA0 File Offset: 0x000C11A0
		public bool isBuildable(Vector2 tileLocation, bool onlyNeedsToBePassable = false)
		{
			Microsoft.Xna.Framework.Rectangle validRect = this.GetBuildableRectangle();
			if (validRect != Microsoft.Xna.Framework.Rectangle.Empty && !validRect.Contains((int)tileLocation.X, (int)tileLocation.Y))
			{
				return false;
			}
			if (onlyNeedsToBePassable)
			{
				return this.isTilePassable(tileLocation) && !this.IsTileOccupiedBy(tileLocation, CollisionMask.All, CollisionMask.All, false);
			}
			Building buildingAtTile = this.getBuildingAt(tileLocation);
			if (buildingAtTile != null && !buildingAtTile.isMoving)
			{
				return false;
			}
			if (!this.CanItemBePlacedHere(tileLocation, false, CollisionMask.All, ~CollisionMask.Objects, true, false))
			{
				Object objectAtTile = this.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y, false);
				if (!(((objectAtTile != null) ? objectAtTile.QualifiedItemId : null) == "(O)590"))
				{
					return false;
				}
			}
			if (this._looserBuildRestrictions)
			{
				return !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("f");
			}
			if (Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("t") || Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("true"))
			{
				return true;
			}
			if (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back", false) != null && !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("f"))
			{
				return true;
			}
			return false;
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x000C314C File Offset: 0x000C134C
		public virtual void pokeTileForConstruction(Vector2 tile)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (animal.Tile == tile)
				{
					animal.Poke();
				}
			}
		}

		// Token: 0x06001015 RID: 4117 RVA: 0x000C31B4 File Offset: 0x000C13B4
		public virtual void updateWarps()
		{
			if (Game1.IsClient)
			{
				return;
			}
			this.warps.Clear();
			foreach (string propertyName in new string[]
			{
				"NPCWarp",
				"Warp"
			})
			{
				string warpsUnparsed;
				if (this.map.Properties.TryGetValue(propertyName, out warpsUnparsed) && warpsUnparsed != null)
				{
					bool npcOnly = propertyName == "NPCWarp";
					string[] fields = ArgUtility.SplitBySpace(warpsUnparsed);
					for (int i = 0; i < fields.Length; i += 5)
					{
						bool hasFields = fields.Length >= i + 5;
						int fromX;
						int fromY;
						int toX;
						int toY;
						if (!hasFields || !int.TryParse(fields[i], out fromX) || !int.TryParse(fields[i + 1], out fromY) || !int.TryParse(fields[i + 3], out toX) || !int.TryParse(fields[i + 4], out toY))
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(118, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Failed parsing ");
							defaultInterpolatedStringHandler.AppendFormatted(npcOnly ? "NPC warp" : "warp");
							defaultInterpolatedStringHandler.AppendLiteral(" '");
							defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", fields.Skip(i)));
							defaultInterpolatedStringHandler.AppendLiteral("' for location '");
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral("'. Warps must have five fields in the form 'fromX fromY toLocationName toX toY', but ");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear() + ((!hasFields) ? "got insufficient fields." : "got a non-numeric value for one of the X/Y position fields."));
						}
						else
						{
							this.warps.Add(new Warp(fromX, fromY, fields[i + 2], toX, toY, false, npcOnly));
						}
					}
				}
			}
			if (this.warps.Count > 0)
			{
				Building parentBuilding = this.ParentBuilding;
				if (parentBuilding == null)
				{
					return;
				}
				parentBuilding.updateInteriorWarps(this);
			}
		}

		// Token: 0x06001016 RID: 4118 RVA: 0x000C3384 File Offset: 0x000C1584
		public void loadWeeds()
		{
			if (!this.isOutdoors.Value && !this.treatAsOutdoors.Value)
			{
				return;
			}
			Map map = this.map;
			Layer pathsLayer = (map != null) ? map.GetLayer("Paths") : null;
			if (pathsLayer == null)
			{
				return;
			}
			for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
				{
					int tileIndex = pathsLayer.GetTileIndexAt(x, y, null);
					if (tileIndex != -1)
					{
						Vector2 tile = new Vector2((float)x, (float)y);
						switch (tileIndex)
						{
						case 13:
						case 14:
						case 15:
							if (this.CanLoadPathObjectHere(tile))
							{
								this.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, this.GetSeason()), 1, 0, false));
							}
							break;
						case 16:
							if (this.CanLoadPathObjectHere(tile))
							{
								this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
							}
							break;
						case 17:
							if (this.CanLoadPathObjectHere(tile))
							{
								this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
							}
							break;
						case 18:
							if (this.CanLoadPathObjectHere(tile))
							{
								this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295"), 1, 0, false));
							}
							break;
						}
					}
				}
			}
		}

		// Token: 0x06001017 RID: 4119 RVA: 0x000C3530 File Offset: 0x000C1730
		public bool CanLoadPathObjectHere(Vector2 tile)
		{
			if (this.IsTileOccupiedBy(tile, CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.Objects | CollisionMask.TerrainFeatures, CollisionMask.None, false))
			{
				return false;
			}
			Vector2 tile_center = tile * 64f;
			tile_center.X += 32f;
			tile_center.Y += 32f;
			foreach (Furniture f in this.furniture)
			{
				if (f.furniture_type.Value != 12 && !f.isPassable() && f.GetBoundingBox().Contains((int)tile_center.X, (int)tile_center.Y) && !f.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001018 RID: 4120 RVA: 0x000C360C File Offset: 0x000C180C
		public void loadObjects()
		{
			this._startingCabinLocations.Clear();
			if (this.map != null)
			{
				this.updateWarps();
				Layer pathsLayer = this.map.GetLayer("Paths");
				string[] trees = this.GetMapPropertySplitBySpaces("Trees");
				for (int i = 0; i < trees.Length; i += 3)
				{
					Vector2 position;
					string error;
					int treeType;
					if (!ArgUtility.TryGetVector2(trees, i, out position, out error, false, "Vector2 position") || !ArgUtility.TryGetInt(trees, i + 2, out treeType, out error, "int treeType"))
					{
						this.LogMapPropertyError("Trees", trees, error, ' ');
					}
					else
					{
						this.terrainFeatures.Add(position, new Tree((treeType + 1).ToString(), 5, false));
					}
				}
				string parentTreeLocation;
				if (pathsLayer != null && this.TryGetMapProperty("LoadTreesFrom", out parentTreeLocation))
				{
					GameLocation parentTreeMap = Game1.getLocationFromName(parentTreeLocation);
					if (parentTreeMap != null)
					{
						foreach (KeyValuePair<Vector2, TerrainFeature> pair in parentTreeMap.terrainFeatures.Pairs)
						{
							Tree tree = pair.Value as Tree;
							if (tree != null)
							{
								Point p = new Point((int)pair.Key.X, (int)pair.Key.Y);
								string id;
								int? growthStageOnLoad;
								int? growthStageOnRegrow;
								bool isFruitTree;
								if (pathsLayer.HasTileAt(p.X, p.Y, null) && this.TryGetTreeIdForTile(pathsLayer.Tiles[p.X, p.Y], out id, out growthStageOnLoad, out growthStageOnRegrow, out isFruitTree))
								{
									this.terrainFeatures.Add(pair.Key, new Tree(tree.treeType.Value, tree.growthStage.Value, false));
								}
							}
						}
					}
				}
				if ((this.isOutdoors.Value || this.name.Equals("BathHouse_Entry") || this.treatAsOutdoors.Value || this.map.Properties.ContainsKey("forceLoadObjects")) && pathsLayer != null)
				{
					this.loadPathsLayerObjectsInArea(0, 0, this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
					if (!Game1.eventUp && this.HasMapPropertyWithValue(this.GetSeason().ToString() + "_Objects"))
					{
						this.spawnObjects();
					}
				}
				this.updateDoors();
			}
		}

		// Token: 0x06001019 RID: 4121 RVA: 0x000C388C File Offset: 0x000C1A8C
		public void loadPathsLayerObjectsInArea(int startingX, int startingY, int width, int height)
		{
			Layer pathsLayer = this.map.GetLayer("Paths");
			for (int x = startingX; x < startingX + width; x++)
			{
				for (int y = startingY; y < startingY + height; y++)
				{
					Tile t = pathsLayer.Tiles[x, y];
					if (t != null)
					{
						Vector2 tile = new Vector2((float)x, (float)y);
						string treeId;
						int? growthStageOnLoad;
						int? growthStageOnRegrow;
						bool isFruitTree;
						if (this.TryGetTreeIdForTile(t, out treeId, out growthStageOnLoad, out growthStageOnRegrow, out isFruitTree))
						{
							if (this.GetFurnitureAt(tile) == null && !this.terrainFeatures.ContainsKey(tile) && !this.objects.ContainsKey(tile))
							{
								if (isFruitTree)
								{
									this.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnLoad.GetValueOrDefault(4)));
								}
								else
								{
									this.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnLoad.GetValueOrDefault(5), false));
								}
							}
						}
						else
						{
							switch (t.TileIndex)
							{
							case 13:
							case 14:
							case 15:
								if (!this.objects.ContainsKey(tile) && (!this.IsOutdoors || !Game1.IsWinter))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, this.GetSeason()), 1, 0, false));
								}
								break;
							case 16:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
								}
								break;
							case 17:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
								}
								break;
							case 18:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295"), 1, 0, false));
								}
								break;
							case 19:
								this.addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, tile);
								break;
							case 20:
								this.addResourceClumpAndRemoveUnderlyingTerrain(672, 2, 2, tile);
								break;
							case 21:
								this.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, tile);
								break;
							case 22:
							case 36:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
									tileRect.Inflate(-1, -1);
									bool fail = false;
									using (List<ResourceClump>.Enumerator enumerator = this.resourceClumps.GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											if (enumerator.Current.getBoundingBox().Intersects(tileRect))
											{
												fail = true;
												break;
											}
										}
									}
									if (!fail)
									{
										this.terrainFeatures.Add(tile, new Grass((t.TileIndex == 36) ? 7 : 1, 3));
									}
								}
								break;
							case 23:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									this.terrainFeatures.Add(tile, new Tree(Game1.random.Next(1, 4).ToString(), Game1.random.Next(2, 4), false));
								}
								break;
							case 24:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									this.largeTerrainFeatures.Add(new Bush(tile, 2, this, -1));
								}
								break;
							case 25:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									this.largeTerrainFeatures.Add(new Bush(tile, 1, this, -1));
								}
								break;
							case 26:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									this.largeTerrainFeatures.Add(new Bush(tile, 0, this, -1));
								}
								break;
							case 27:
								this.changeMapProperties("BrookSounds", tile.X.ToString() + " " + tile.Y.ToString() + " 0");
								break;
							case 29:
							case 30:
							{
								string rawOrder;
								if (Game1.startingCabins > 0 && t.Properties.TryGetValue("Order", out rawOrder) && int.Parse(rawOrder) <= Game1.startingCabins && ((t.TileIndex == 29 && !Game1.cabinsSeparate) || (t.TileIndex == 30 && Game1.cabinsSeparate)))
								{
									this._startingCabinLocations.Add(tile);
								}
								break;
							}
							case 33:
								if (!this.terrainFeatures.ContainsKey(tile))
								{
									this.largeTerrainFeatures.Add(new Bush(tile, 4, this, -1));
								}
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600101A RID: 4122 RVA: 0x000C3D88 File Offset: 0x000C1F88
		public bool TryGetTreeIdForTile(Tile tile, out string treeId, out int? growthStageOnLoad, out int? growthStageOnRegrow, out bool isFruitTree)
		{
			isFruitTree = false;
			growthStageOnLoad = null;
			growthStageOnRegrow = null;
			if (tile == null)
			{
				treeId = null;
				return false;
			}
			int tileIndex = tile.TileIndex;
			switch (tileIndex)
			{
			case 9:
				treeId = (this.IsWinterHere() ? "4" : "1");
				return true;
			case 10:
				treeId = (this.IsWinterHere() ? "5" : "2");
				return true;
			case 11:
				treeId = "3";
				return true;
			case 12:
				treeId = "6";
				return true;
			default:
				switch (tileIndex)
				{
				case 31:
					treeId = "9";
					return true;
				case 32:
					treeId = "8";
					return true;
				case 34:
				{
					string property;
					if (!tile.Properties.TryGetValue("SpawnTree", out property))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(106, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Location '");
						defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral("' ignored path tile index 34 (spawn tree) at position ");
						defaultInterpolatedStringHandler.AppendFormatted<Tile>(tile);
						defaultInterpolatedStringHandler.AppendLiteral(" because the tile has no '");
						defaultInterpolatedStringHandler.AppendFormatted("SpawnTree");
						defaultInterpolatedStringHandler.AppendLiteral("' tile property.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						string[] args = ArgUtility.SplitBySpace(property);
						string rawType;
						string error;
						string rawId;
						int rawGrowthStageOnLoad;
						int rawGrowthStageOnRegrow;
						if (!ArgUtility.TryGet(args, 0, out rawType, out error, true, "string rawType") || !ArgUtility.TryGet(args, 1, out rawId, out error, true, "string rawId") || !ArgUtility.TryGetOptionalInt(args, 2, out rawGrowthStageOnLoad, out error, -1, "int rawGrowthStageOnLoad") || !ArgUtility.TryGetOptionalInt(args, 3, out rawGrowthStageOnRegrow, out error, -1, "int rawGrowthStageOnRegrow"))
						{
							IGameLogger log2 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(107, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Location '");
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral("' ignored path tile index 34 (spawn tree) at position ");
							defaultInterpolatedStringHandler.AppendFormatted<Tile>(tile);
							defaultInterpolatedStringHandler.AppendLiteral(" because the '");
							defaultInterpolatedStringHandler.AppendFormatted("SpawnTree");
							defaultInterpolatedStringHandler.AppendLiteral("' tile property is invalid: ");
							defaultInterpolatedStringHandler.AppendFormatted(error);
							defaultInterpolatedStringHandler.AppendLiteral(".");
							log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						else
						{
							if (rawGrowthStageOnLoad > -1)
							{
								growthStageOnLoad = new int?(rawGrowthStageOnLoad);
							}
							if (rawGrowthStageOnRegrow > -1)
							{
								growthStageOnRegrow = new int?(rawGrowthStageOnRegrow);
							}
							if (rawType.EqualsIgnoreCase("wild"))
							{
								treeId = rawId;
								return true;
							}
							if (rawType.EqualsIgnoreCase("fruit"))
							{
								treeId = rawId;
								isFruitTree = true;
								return true;
							}
							IGameLogger log3 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(143, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Location '");
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral("' ignored path tile index 34 (spawn tree) at position ");
							defaultInterpolatedStringHandler.AppendFormatted<Tile>(tile);
							defaultInterpolatedStringHandler.AppendLiteral(" because the '");
							defaultInterpolatedStringHandler.AppendFormatted("SpawnTree");
							defaultInterpolatedStringHandler.AppendLiteral("' tile property has invalid type '");
							defaultInterpolatedStringHandler.AppendFormatted(rawType);
							defaultInterpolatedStringHandler.AppendLiteral("' (expected 'fruit' or 'wild').");
							log3.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					break;
				}
				}
				growthStageOnLoad = null;
				growthStageOnRegrow = null;
				treeId = null;
				return false;
			}
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x000C4088 File Offset: 0x000C2288
		public void BuildStartingCabins()
		{
			if (this._startingCabinLocations.Count > 0)
			{
				List<string> cabinStyleOrder = new List<string>();
				int whichFarm = Game1.whichFarm;
				if (whichFarm != 1)
				{
					if (whichFarm - 3 <= 1)
					{
						cabinStyleOrder.Add("Stone Cabin");
						cabinStyleOrder.Add("Log Cabin");
						cabinStyleOrder.Add("Plank Cabin");
						cabinStyleOrder.Add("Rustic Cabin");
						cabinStyleOrder.Add("Trailer Cabin");
						cabinStyleOrder.Add("Neighbor Cabin");
						cabinStyleOrder.Add("Beach Cabin");
					}
					else
					{
						bool logFirst = Game1.random.NextBool();
						cabinStyleOrder.Add(logFirst ? "Log Cabin" : "Plank Cabin");
						cabinStyleOrder.Add("Stone Cabin");
						cabinStyleOrder.Add(logFirst ? "Plank Cabin" : "Log Cabin");
						cabinStyleOrder.Add("Trailer Cabin");
						cabinStyleOrder.Add("Neighbor Cabin");
						cabinStyleOrder.Add("Rustic Cabin");
						cabinStyleOrder.Add("Beach Cabin");
					}
				}
				else
				{
					cabinStyleOrder.Add("Beach Cabin");
					cabinStyleOrder.Add("Plank Cabin");
					cabinStyleOrder.Add("Log Cabin");
					cabinStyleOrder.Add("Neighbor Cabin");
					cabinStyleOrder.Add("Trailer Cabin");
					cabinStyleOrder.Add("Stone Cabin");
					cabinStyleOrder.Add("Rustic Cabin");
				}
				List<Vector2> startingCabinsInOrder = new List<Vector2>();
				for (int i = 0; i < this._startingCabinLocations.Count; i++)
				{
					for (int j = 0; j < this._startingCabinLocations.Count; j++)
					{
						if (this.doesTileHavePropertyNoNull((int)this._startingCabinLocations[j].X, (int)this._startingCabinLocations[j].Y, "Order", "Paths").Equals((i + 1).ToString() ?? ""))
						{
							startingCabinsInOrder.Add(this._startingCabinLocations[j]);
						}
					}
				}
				for (int k = 0; k < startingCabinsInOrder.Count; k++)
				{
					this.removeObjectsAndSpawned((int)startingCabinsInOrder[k].X, (int)startingCabinsInOrder[k].Y, 5, 3);
					this.removeObjectsAndSpawned((int)startingCabinsInOrder[k].X + 2, (int)startingCabinsInOrder[k].Y + 3, 1, 1);
					Building b = new Building("Cabin", startingCabinsInOrder[k]);
					b.magical.Value = true;
					b.skinId.Value = cabinStyleOrder[k % cabinStyleOrder.Count];
					b.daysOfConstructionLeft.Value = 0;
					b.load();
					this.buildStructure(b, startingCabinsInOrder[k], Game1.player, true);
					b.removeOverlappingBushes(this);
				}
			}
			this._startingCabinLocations.Clear();
		}

		// Token: 0x0600101C RID: 4124 RVA: 0x000C4354 File Offset: 0x000C2554
		public void updateDoors()
		{
			if (Game1.IsClient)
			{
				return;
			}
			this.doors.Clear();
			Layer buildingLayer = this.map.RequireLayer("Buildings");
			int y = 0;
			int layerHeight = buildingLayer.LayerHeight;
			while (y < layerHeight)
			{
				int x = 0;
				int layerWidth = buildingLayer.LayerWidth;
				while (x < layerWidth)
				{
					Tile tile = buildingLayer.Tiles[x, y];
					string door;
					if (tile != null && tile.Properties.TryGetValue("Action", out door) && door.Contains("Warp"))
					{
						string[] split = ArgUtility.SplitBySpace(door);
						string propertyName = ArgUtility.Get(split, 0, null, true);
						if (propertyName == null)
						{
							goto IL_237;
						}
						int length = propertyName.Length;
						if (length != 4)
						{
							switch (length)
							{
							case 14:
							{
								char c = propertyName[4];
								if (c != 'B')
								{
									if (c != 'M')
									{
										if (c != 'e')
										{
											goto IL_237;
										}
										if (!(propertyName == "LockedDoorWarp"))
										{
											goto IL_237;
										}
									}
									else if (!(propertyName == "WarpMensLocker"))
									{
										goto IL_237;
									}
								}
								else
								{
									if (!(propertyName == "WarpBoatTunnel"))
									{
										goto IL_237;
									}
									this.doors.Add(new Point(x, y), new NetString("BoatTunnel"));
									goto IL_2BB;
								}
								break;
							}
							case 15:
							case 18:
								goto IL_237;
							case 16:
								if (!(propertyName == "WarpWomensLocker"))
								{
									goto IL_237;
								}
								break;
							case 17:
								if (!(propertyName == "Warp_Sunroom_Door"))
								{
									goto IL_237;
								}
								this.doors.Add(new Point(x, y), new NetString("Sunroom"));
								goto IL_2BB;
							case 19:
								if (!(propertyName == "WarpCommunityCenter"))
								{
									goto IL_237;
								}
								this.doors.Add(new Point(x, y), new NetString("CommunityCenter"));
								goto IL_2BB;
							default:
								goto IL_237;
							}
						}
						else if (!(propertyName == "Warp"))
						{
							goto IL_237;
						}
						IL_1E3:
						if (this.name.Value == "Mountain" && x == 8 && y == 20)
						{
							goto IL_2BB;
						}
						string locationName = ArgUtility.Get(split, 3, null, true);
						if (locationName != null)
						{
							this.doors.Add(new Point(x, y), new NetString(locationName));
							goto IL_2BB;
						}
						goto IL_2BB;
						IL_237:
						if (propertyName.Contains("Warp"))
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 4);
							defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(x);
							defaultInterpolatedStringHandler.AppendLiteral(", ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(y);
							defaultInterpolatedStringHandler.AppendLiteral(") has unknown warp property '");
							defaultInterpolatedStringHandler.AppendFormatted(door);
							defaultInterpolatedStringHandler.AppendLiteral("', parsing with legacy logic.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
							goto IL_1E3;
						}
					}
					IL_2BB:
					x++;
				}
				y++;
			}
		}

		// Token: 0x0600101D RID: 4125 RVA: 0x000C4633 File Offset: 0x000C2833
		[Obsolete("Use removeObjectsAndSpawned instead.")]
		private void clearArea(int startingX, int startingY, int width, int height)
		{
			this.removeObjectsAndSpawned(startingX, startingY, width, height);
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x000C4640 File Offset: 0x000C2840
		public bool isTerrainFeatureAt(int x, int y)
		{
			Vector2 v = new Vector2((float)x, (float)y);
			TerrainFeature terrainFeature;
			if (this.terrainFeatures.TryGetValue(v, out terrainFeature) && !terrainFeature.isPassable(null))
			{
				return true;
			}
			if (this.largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
				using (List<LargeTerrainFeature>.Enumerator enumerator = this.largeTerrainFeatures.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.getBoundingBox().Intersects(tileRect))
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600101F RID: 4127 RVA: 0x000C46E8 File Offset: 0x000C28E8
		public void loadLights()
		{
			if ((!this.isOutdoors.Value || Game1.isFestival() || this.forceLoadPathLayerLights) && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				Layer pathsLayer = this.map.GetLayer("Paths");
				Layer frontLayer = this.map.RequireLayer("Front");
				Layer buildingsLayer = this.map.RequireLayer("Buildings");
				for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
					{
						int tileIndex;
						if (!this.isOutdoors.Value && !this.map.Properties.ContainsKey("IgnoreLightingTiles"))
						{
							tileIndex = frontLayer.GetTileIndexAt(x, y, null);
							if (tileIndex != -1)
							{
								this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Front");
							}
							tileIndex = buildingsLayer.GetTileIndexAt(x, y, null);
							if (tileIndex != -1)
							{
								this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Buildings");
							}
						}
						tileIndex = ((pathsLayer != null) ? pathsLayer.GetTileIndexAt(x, y, null) : -1);
						if (tileIndex != -1)
						{
							this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Paths");
						}
					}
				}
			}
		}

		// Token: 0x06001020 RID: 4128 RVA: 0x000C483C File Offset: 0x000C2A3C
		public bool isFarmBuildingInterior()
		{
			return this is AnimalHouse;
		}

		// Token: 0x06001021 RID: 4129 RVA: 0x000C4847 File Offset: 0x000C2A47
		public bool IsActiveLocation()
		{
			if (Game1.IsMasterGame)
			{
				return true;
			}
			NetRoot<GameLocation> root = this.Root;
			return ((root != null) ? root.Value : null) != null && Game1.multiplayer.isActiveLocation(this);
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x000C4873 File Offset: 0x000C2A73
		public virtual bool CanBeRemotedlyViewed()
		{
			return Game1.multiplayer.isAlwaysActiveLocation(this);
		}

		// Token: 0x06001023 RID: 4131 RVA: 0x000C4880 File Offset: 0x000C2A80
		protected void adjustMapLightPropertiesForLamp(int tile, int x, int y, string layer)
		{
			string tilesheet = this.getTileSheetIDAt(x, y, layer);
			if (this.isFarmBuildingInterior())
			{
				if (tilesheet == "Coop" || tilesheet == "barn")
				{
					if (tile == 24)
					{
						this.changeMapProperties("DayTiles", string.Concat(new string[]
						{
							layer,
							" ",
							x.ToString(),
							" ",
							y.ToString(),
							" ",
							tile.ToString()
						}));
						this.changeMapProperties("NightTiles", string.Concat(new string[]
						{
							layer,
							" ",
							x.ToString(),
							" ",
							y.ToString(),
							" ",
							26.ToString()
						}));
						this.changeMapProperties("WindowLight", x.ToString() + " " + (y + 1).ToString() + " 4");
						this.changeMapProperties("WindowLight", x.ToString() + " " + (y + 3).ToString() + " 4");
						return;
					}
					if (tile == 25)
					{
						this.changeMapProperties("DayTiles", string.Concat(new string[]
						{
							layer,
							" ",
							x.ToString(),
							" ",
							y.ToString(),
							" ",
							tile.ToString()
						}));
						this.changeMapProperties("NightTiles", string.Concat(new string[]
						{
							layer,
							" ",
							x.ToString(),
							" ",
							y.ToString(),
							" ",
							12.ToString()
						}));
						return;
					}
					if (tile != 46)
					{
						return;
					}
					this.changeMapProperties("DayTiles", string.Concat(new string[]
					{
						layer,
						" ",
						x.ToString(),
						" ",
						y.ToString(),
						" ",
						tile.ToString()
					}));
					this.changeMapProperties("NightTiles", string.Concat(new string[]
					{
						layer,
						" ",
						x.ToString(),
						" ",
						y.ToString(),
						" ",
						53.ToString()
					}));
					return;
				}
			}
			else
			{
				if (tile == 8 && layer == "Paths")
				{
					this.changeMapProperties("Light", x.ToString() + " " + y.ToString() + " 4");
					return;
				}
				if (tilesheet == "indoor")
				{
					if (tile <= 480)
					{
						if (tile != 225)
						{
							if (tile == 256)
							{
								this.changeMapProperties("DayTiles", string.Concat(new string[]
								{
									layer,
									" ",
									x.ToString(),
									" ",
									y.ToString(),
									" ",
									tile.ToString()
								}));
								this.changeMapProperties("NightTiles", string.Concat(new string[]
								{
									layer,
									" ",
									x.ToString(),
									" ",
									y.ToString(),
									" ",
									1253.ToString()
								}));
								this.changeMapProperties("DayTiles", string.Concat(new string[]
								{
									layer,
									" ",
									x.ToString(),
									" ",
									(y + 1).ToString(),
									" ",
									288.ToString()
								}));
								this.changeMapProperties("NightTiles", string.Concat(new string[]
								{
									layer,
									" ",
									x.ToString(),
									" ",
									(y + 1).ToString(),
									" ",
									1285.ToString()
								}));
								this.changeMapProperties("WindowLight", x.ToString() + " " + y.ToString() + " 4");
								this.changeMapProperties("WindowLight", x.ToString() + " " + (y + 1).ToString() + " 4");
								return;
							}
							if (tile != 480)
							{
								return;
							}
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								tile.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								809.ToString()
							}));
							this.changeMapProperties("Light", x.ToString() + " " + y.ToString() + " 4");
							return;
						}
						else if (!this.name.Value.Contains("BathHouse") && !this.name.Value.Contains("Club") && (!this.name.Equals("SeedShop") || (x != 36 && x != 37)))
						{
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								tile.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								1222.ToString()
							}));
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								(y + 1).ToString(),
								" ",
								257.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								(y + 1).ToString(),
								" ",
								1254.ToString()
							}));
							this.changeMapProperties("WindowLight", x.ToString() + " " + y.ToString() + " 4");
							this.changeMapProperties("WindowLight", x.ToString() + " " + (y + 1).ToString() + " 4");
						}
					}
					else
					{
						if (tile == 826)
						{
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								tile.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								827.ToString()
							}));
							this.changeMapProperties("Light", x.ToString() + " " + y.ToString() + " 4");
							return;
						}
						if (tile == 1344)
						{
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								tile.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								layer,
								" ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								1345.ToString()
							}));
							this.changeMapProperties("Light", x.ToString() + " " + y.ToString() + " 4");
							return;
						}
						if (tile == 1346)
						{
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								"Front ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								tile.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								"Front ",
								x.ToString(),
								" ",
								y.ToString(),
								" ",
								1347.ToString()
							}));
							this.changeMapProperties("DayTiles", string.Concat(new string[]
							{
								"Buildings ",
								x.ToString(),
								" ",
								(y + 1).ToString(),
								" ",
								452.ToString()
							}));
							this.changeMapProperties("NightTiles", string.Concat(new string[]
							{
								"Buildings ",
								x.ToString(),
								" ",
								(y + 1).ToString(),
								" ",
								453.ToString()
							}));
							this.changeMapProperties("Light", x.ToString() + " " + y.ToString() + " 4");
							return;
						}
					}
				}
			}
		}

		// Token: 0x06001024 RID: 4132 RVA: 0x000C5358 File Offset: 0x000C3558
		private void changeMapProperties(string propertyName, string toAdd)
		{
			try
			{
				string oldValue;
				if (!this.map.Properties.TryGetValue(propertyName, out oldValue))
				{
					this.map.Properties[propertyName] = new PropertyValue(toAdd);
				}
				else if (!oldValue.Contains(toAdd))
				{
					string newValue = oldValue + " " + toAdd;
					this.map.Properties[propertyName] = new PropertyValue(newValue);
				}
			}
			catch
			{
			}
		}

		// Token: 0x06001025 RID: 4133 RVA: 0x000C53D8 File Offset: 0x000C35D8
		public void LogMapPropertyError(string name, string value, string error)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Can't parse map property '");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("' with value '");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("' in location '");
			defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("': ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x06001026 RID: 4134 RVA: 0x000C5462 File Offset: 0x000C3662
		public void LogMapPropertyError(string name, string[] value, string error, char delimiter = ' ')
		{
			this.LogMapPropertyError(name, string.Join(delimiter, value), error);
		}

		// Token: 0x06001027 RID: 4135 RVA: 0x000C5474 File Offset: 0x000C3674
		public void LogTilePropertyError(string name, string layerId, int x, int y, string value, string error)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 7);
			defaultInterpolatedStringHandler.AppendLiteral("Can't parse tile property '");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("' at ");
			defaultInterpolatedStringHandler.AppendFormatted(layerId);
			defaultInterpolatedStringHandler.AppendLiteral(":");
			defaultInterpolatedStringHandler.AppendFormatted<int>(x);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<int>(y);
			defaultInterpolatedStringHandler.AppendLiteral(" with value '");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("' in location '");
			defaultInterpolatedStringHandler.AppendFormatted(this.NameOrUniqueName);
			defaultInterpolatedStringHandler.AppendLiteral("': ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x06001028 RID: 4136 RVA: 0x000C553D File Offset: 0x000C373D
		public void LogTilePropertyError(string name, string layerId, int x, int y, string[] value, string error, char delimiter = ' ')
		{
			this.LogTilePropertyError(name, layerId, x, y, string.Join(delimiter, value), error);
		}

		// Token: 0x06001029 RID: 4137 RVA: 0x000C5555 File Offset: 0x000C3755
		public void LogTileActionError(string[] action, int x, int y, string error)
		{
			this.LogTilePropertyError("Action", "Buildings", x, y, action, error, ' ');
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x000C556E File Offset: 0x000C376E
		public void LogTileTouchActionError(string[] action, Vector2 tile, string error)
		{
			this.LogTilePropertyError("TouchAction", "Back", (int)tile.X, (int)tile.Y, action, error, ' ');
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x000C5594 File Offset: 0x000C3794
		public override bool Equals(object obj)
		{
			GameLocation location = obj as GameLocation;
			return location != null && this.Equals(location);
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x000C55B4 File Offset: 0x000C37B4
		public bool Equals(GameLocation other)
		{
			return other != null && this.isStructure.Get() == other.isStructure.Get() && string.Equals(this.NameOrUniqueName, other.NameOrUniqueName, StringComparison.Ordinal);
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x000C57F8 File Offset: 0x000C39F8
		[CompilerGenerated]
		private void <MakeMapModifications>g__ShowSkillMastery|162_0(int skill, Vector2 spritePosition, ref GameLocation.<>c__DisplayClass162_0 A_3)
		{
			uint mastery = Game1.player.stats.Get(StatKeys.Mastery(skill));
			if (A_3.levelsNotSpent > 0 && mastery == 0U)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(16, 110, 7, 9), spritePosition * 4f, false, 0f, Color.White)
				{
					animationLength = 15,
					interval = 50f,
					totalNumberOfLoops = 999999,
					scale = 4f,
					id = 8765 + skill
				});
			}
			else if (mastery > 0U)
			{
				MasteryTrackerMenu.addSkillFlairPlaque(skill);
			}
			Game1.changeMusicTrack("Upper_Ambient", false, MusicContext.Default);
		}

		// Token: 0x06001040 RID: 4160 RVA: 0x000C58AC File Offset: 0x000C3AAC
		[CompilerGenerated]
		private void <performTouchAction>g__LogError|272_0(string errorPhrase, ref GameLocation.<>c__DisplayClass272_0 A_2)
		{
			this.LogTileTouchActionError(A_2.action, A_2.playerStandingPosition, errorPhrase);
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x000C5E41 File Offset: 0x000C4041
		[CompilerGenerated]
		internal static bool <CheckGenericFishRequirements>g__LogFormatError|503_0(string error, ref GameLocation.<>c__DisplayClass503_0 A_1)
		{
			Game1.log.Warn("Skipped fish '" + A_1.fish.ItemId + "' due to invalid requirements in Data/Fish: " + error);
			return false;
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x000C5E6C File Offset: 0x000C406C
		[CompilerGenerated]
		internal static LocationData <GetData>g__GetImpl|508_0(string entryName, ref GameLocation.<>c__DisplayClass508_0 A_1)
		{
			LocationData data;
			if (A_1.rawData.TryGetValue(entryName, out data))
			{
				return data;
			}
			using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PassiveFestivalData festivalData;
					if (Utility.TryGetPassiveFestivalData(enumerator.Current, out festivalData) && festivalData.MapReplacements != null)
					{
						foreach (KeyValuePair<string, string> replacement in festivalData.MapReplacements)
						{
							if (replacement.Value == entryName)
							{
								if (A_1.rawData.TryGetValue(replacement.Key, out data))
								{
									return data;
								}
								break;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x000C5F48 File Offset: 0x000C4148
		[CompilerGenerated]
		private void <HandleTreasureTileProperty>g__LogError|511_0(string value, string errorPhrase, ref GameLocation.<>c__DisplayClass511_0 A_3)
		{
			this.LogTilePropertyError("Treasure", "Back", A_3.xLocation, A_3.yLocation, value, errorPhrase);
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x000C5F68 File Offset: 0x000C4168
		[CompilerGenerated]
		internal static bool <GetHayFromAnySilo>g__TryGetHayFrom|574_0(GameLocation location, out Object foundHay)
		{
			if (location.piecesOfHay.Value < 1)
			{
				foundHay = null;
				return false;
			}
			foundHay = ItemRegistry.Create<Object>("(O)178", 1, 0, false);
			NetInt netInt = location.piecesOfHay;
			int value = netInt.Value;
			netInt.Value = value - 1;
			return true;
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x000C5FAD File Offset: 0x000C41AD
		[CompilerGenerated]
		private void <buildStructure>g__RemoveArtifactSpots|583_0(Vector2 tile_location)
		{
			Object objectAtTile = this.getObjectAtTile((int)tile_location.X, (int)tile_location.Y, false);
			if (((objectAtTile != null) ? objectAtTile.QualifiedItemId : null) == "(O)590")
			{
				this.removeObject(tile_location, false);
			}
		}

		// Token: 0x04000978 RID: 2424
		public const int maxTriesForDebrisPlacement = 3;

		// Token: 0x04000979 RID: 2425
		public const string DefaultTileSheetId = "untitled tile sheet";

		// Token: 0x0400097A RID: 2426
		public const string OVERRIDE_MAP_TILESHEET_PREFIX = "zzzzz";

		// Token: 0x0400097B RID: 2427
		public const string PHONE_DIAL_SOUND = "telephone_buttonPush";

		// Token: 0x0400097C RID: 2428
		public const int PHONE_RING_DURATION = 4950;

		// Token: 0x0400097D RID: 2429
		public const string PHONE_PICKUP_SOUND = "bigSelect";

		// Token: 0x0400097E RID: 2430
		public const string PHONE_HANGUP_SOUND = "openBox";

		// Token: 0x0400097F RID: 2431
		public static readonly IList<string> OceanCrabPotFishTypes = new string[]
		{
			"ocean"
		};

		// Token: 0x04000980 RID: 2432
		public static readonly IList<string> DefaultCrabPotFishTypes = new string[]
		{
			"freshwater"
		};

		// Token: 0x04000981 RID: 2433
		[XmlIgnore]
		private Lazy<Season?> seasonOverride;

		// Token: 0x04000982 RID: 2434
		[XmlIgnore]
		public bool? isMusicTownMusic;

		// Token: 0x04000983 RID: 2435
		[XmlIgnore]
		public string locationContextId;

		// Token: 0x04000984 RID: 2436
		public readonly NetCollection<Building> buildings = new NetCollection<Building>
		{
			InterpolationWait = false
		};

		// Token: 0x04000985 RID: 2437
		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		// Token: 0x04000986 RID: 2438
		[XmlElement("piecesOfHay")]
		public readonly NetInt piecesOfHay = new NetInt(0);

		// Token: 0x04000987 RID: 2439
		private readonly List<KeyValuePair<long, FarmAnimal>> tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		// Token: 0x04000988 RID: 2440
		[XmlIgnore]
		public readonly NetString parentLocationName = new NetString();

		// Token: 0x04000989 RID: 2441
		[XmlIgnore]
		public Building ParentBuilding;

		// Token: 0x0400098A RID: 2442
		[XmlIgnore]
		public List<KeyValuePair<Layer, int>> backgroundLayers = new List<KeyValuePair<Layer, int>>();

		// Token: 0x0400098B RID: 2443
		[XmlIgnore]
		public List<KeyValuePair<Layer, int>> buildingLayers = new List<KeyValuePair<Layer, int>>();

		// Token: 0x0400098C RID: 2444
		[XmlIgnore]
		public List<KeyValuePair<Layer, int>> frontLayers = new List<KeyValuePair<Layer, int>>();

		// Token: 0x0400098D RID: 2445
		[XmlIgnore]
		public List<KeyValuePair<Layer, int>> alwaysFrontLayers = new List<KeyValuePair<Layer, int>>();

		// Token: 0x0400098E RID: 2446
		[NonInstancedStatic]
		[XmlIgnore]
		protected static Dictionary<string, Action<GameLocation, string[], Farmer, Vector2>> registeredTouchActions = new Dictionary<string, Action<GameLocation, string[], Farmer, Vector2>>();

		// Token: 0x0400098F RID: 2447
		[NonInstancedStatic]
		[XmlIgnore]
		protected static Dictionary<string, Func<GameLocation, string[], Farmer, Point, bool>> registeredTileActions = new Dictionary<string, Func<GameLocation, string[], Farmer, Point, bool>>();

		// Token: 0x04000991 RID: 2449
		[XmlIgnore]
		public NetBool isAlwaysActive = new NetBool();

		// Token: 0x04000993 RID: 2451
		[XmlIgnore]
		public GameLocation.afterQuestionBehavior afterQuestion;

		// Token: 0x04000994 RID: 2452
		[XmlIgnore]
		public Map map;

		// Token: 0x04000995 RID: 2453
		[XmlIgnore]
		public readonly NetString mapPath = new NetString().Interpolated(false, false);

		// Token: 0x04000996 RID: 2454
		[XmlIgnore]
		protected string loadedMapPath;

		// Token: 0x04000997 RID: 2455
		public readonly NetCollection<NPC> characters = new NetCollection<NPC>();

		// Token: 0x04000998 RID: 2456
		[XmlIgnore]
		public readonly NetVector2Dictionary<Object, NetRef<Object>> netObjects = new NetVector2Dictionary<Object, NetRef<Object>>();

		// Token: 0x04000999 RID: 2457
		[XmlIgnore]
		public readonly OverlayDictionary<Vector2, Object> overlayObjects = new OverlayDictionary<Vector2, Object>(GameLocation.tilePositionComparer);

		// Token: 0x0400099A RID: 2458
		[XmlElement("objects")]
		public readonly OverlaidDictionary objects;

		// Token: 0x0400099B RID: 2459
		[XmlIgnore]
		public NetList<MapSeat, NetRef<MapSeat>> mapSeats = new NetList<MapSeat, NetRef<MapSeat>>();

		// Token: 0x0400099C RID: 2460
		protected bool _mapSeatsDirty;

		// Token: 0x0400099D RID: 2461
		[XmlIgnore]
		public TemporaryAnimatedSpriteList temporarySprites = new TemporaryAnimatedSpriteList();

		// Token: 0x0400099E RID: 2462
		[XmlIgnore]
		public List<Action> postFarmEventOvernightActions = new List<Action>();

		// Token: 0x0400099F RID: 2463
		[XmlIgnore]
		public readonly NetObjectList<Warp> warps = new NetObjectList<Warp>();

		// Token: 0x040009A0 RID: 2464
		[XmlIgnore]
		public readonly NetPointDictionary<string, NetString> doors = new NetPointDictionary<string, NetString>();

		// Token: 0x040009A1 RID: 2465
		[XmlIgnore]
		public readonly InteriorDoorDictionary interiorDoors;

		// Token: 0x040009A2 RID: 2466
		[XmlIgnore]
		public readonly FarmerCollection farmers;

		// Token: 0x040009A3 RID: 2467
		[XmlIgnore]
		public readonly NetCollection<Projectile> projectiles = new NetCollection<Projectile>();

		// Token: 0x040009A4 RID: 2468
		public readonly NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();

		// Token: 0x040009A5 RID: 2469
		public readonly NetCollection<LargeTerrainFeature> largeTerrainFeatures = new NetCollection<LargeTerrainFeature>();

		// Token: 0x040009A6 RID: 2470
		[XmlIgnore]
		public List<TerrainFeature> _activeTerrainFeatures = new List<TerrainFeature>();

		// Token: 0x040009A7 RID: 2471
		[XmlIgnore]
		public List<Critter> critters;

		// Token: 0x040009A8 RID: 2472
		[XmlElement("terrainFeatures")]
		public readonly NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = new NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>();

		// Token: 0x040009A9 RID: 2473
		[XmlIgnore]
		public readonly NetCollection<Debris> debris = new NetCollection<Debris>();

		// Token: 0x040009AA RID: 2474
		[XmlIgnore]
		public readonly NetPoint fishSplashPoint = new NetPoint(Point.Zero);

		// Token: 0x040009AB RID: 2475
		private int fishSplashPointTime;

		// Token: 0x040009AC RID: 2476
		[XmlIgnore]
		public readonly NetString fishFrenzyFish = new NetString();

		// Token: 0x040009AD RID: 2477
		[XmlIgnore]
		public readonly NetPoint orePanPoint = new NetPoint(Point.Zero);

		// Token: 0x040009AE RID: 2478
		[XmlIgnore]
		public TemporaryAnimatedSprite fishSplashAnimation;

		// Token: 0x040009AF RID: 2479
		[XmlIgnore]
		public TemporaryAnimatedSprite orePanAnimation;

		// Token: 0x040009B0 RID: 2480
		[XmlIgnore]
		public WaterTiles waterTiles;

		// Token: 0x040009B1 RID: 2481
		[XmlIgnore]
		protected HashSet<string> _appliedMapOverrides;

		// Token: 0x040009B2 RID: 2482
		[XmlElement("uniqueName")]
		public readonly NetString uniqueName = new NetString();

		// Token: 0x040009B3 RID: 2483
		[XmlIgnore]
		protected string _displayName;

		// Token: 0x040009B4 RID: 2484
		[XmlElement("name")]
		public readonly NetString name = new NetString();

		// Token: 0x040009B6 RID: 2486
		[XmlElement("waterColor")]
		public readonly NetColor waterColor = new NetColor(Color.White * 0.33f);

		// Token: 0x040009B7 RID: 2487
		[XmlIgnore]
		public string lastQuestionKey;

		// Token: 0x040009B8 RID: 2488
		[XmlIgnore]
		public Vector2 lastTouchActionLocation = Vector2.Zero;

		// Token: 0x040009B9 RID: 2489
		[XmlElement("lightLevel")]
		protected readonly NetFloat lightLevel = new NetFloat(0f);

		// Token: 0x040009BA RID: 2490
		[XmlElement("isFarm")]
		public readonly NetBool isFarm = new NetBool();

		// Token: 0x040009BB RID: 2491
		[XmlElement("isOutdoors")]
		public readonly NetBool isOutdoors = new NetBool();

		// Token: 0x040009BC RID: 2492
		[XmlIgnore]
		public readonly NetBool isGreenhouse = new NetBool();

		// Token: 0x040009BD RID: 2493
		[XmlElement("isStructure")]
		public readonly NetBool isStructure = new NetBool();

		// Token: 0x040009BE RID: 2494
		[XmlElement("ignoreDebrisWeather")]
		public readonly NetBool ignoreDebrisWeather = new NetBool();

		// Token: 0x040009BF RID: 2495
		[XmlElement("ignoreOutdoorLighting")]
		public readonly NetBool ignoreOutdoorLighting = new NetBool();

		// Token: 0x040009C0 RID: 2496
		[XmlElement("ignoreLights")]
		public readonly NetBool ignoreLights = new NetBool();

		// Token: 0x040009C1 RID: 2497
		[XmlElement("treatAsOutdoors")]
		public readonly NetBool treatAsOutdoors = new NetBool();

		// Token: 0x040009C2 RID: 2498
		[XmlIgnore]
		public bool wasUpdated;

		// Token: 0x040009C3 RID: 2499
		public int numberOfSpawnedObjectsOnMap;

		// Token: 0x040009C4 RID: 2500
		[XmlIgnore]
		public bool showDropboxIndicator;

		// Token: 0x040009C5 RID: 2501
		[XmlIgnore]
		public Vector2 dropBoxIndicatorLocation;

		// Token: 0x040009C6 RID: 2502
		[XmlElement("miniJukeboxCount")]
		public readonly NetInt miniJukeboxCount = new NetInt();

		// Token: 0x040009C7 RID: 2503
		[XmlElement("miniJukeboxTrack")]
		public readonly NetString miniJukeboxTrack = new NetString("");

		// Token: 0x040009C8 RID: 2504
		[XmlIgnore]
		public readonly NetString randomMiniJukeboxTrack = new NetString();

		// Token: 0x040009C9 RID: 2505
		[XmlIgnore]
		public Event currentEvent;

		// Token: 0x040009CA RID: 2506
		[XmlIgnore]
		public Object actionObjectForQuestionDialogue;

		// Token: 0x040009CB RID: 2507
		[XmlIgnore]
		public int waterAnimationIndex;

		// Token: 0x040009CC RID: 2508
		[XmlIgnore]
		public int waterAnimationTimer;

		// Token: 0x040009CD RID: 2509
		[XmlIgnore]
		public bool waterTileFlip;

		// Token: 0x040009CE RID: 2510
		[XmlIgnore]
		public bool forceViewportPlayerFollow;

		// Token: 0x040009CF RID: 2511
		[XmlIgnore]
		public bool forceLoadPathLayerLights;

		// Token: 0x040009D0 RID: 2512
		[XmlIgnore]
		public float waterPosition;

		// Token: 0x040009D1 RID: 2513
		[XmlIgnore]
		public readonly NetAudio netAudio;

		// Token: 0x040009D2 RID: 2514
		[XmlIgnore]
		public readonly NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights = new NetStringDictionary<LightSource, NetRef<LightSource>>();

		// Token: 0x040009D3 RID: 2515
		private readonly NetEvent1Field<int, NetInt> removeTemporarySpritesWithIDEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x040009D4 RID: 2516
		private readonly NetEvent1Field<int, NetInt> rumbleAndFadeEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x040009D5 RID: 2517
		private readonly NetEvent1<GameLocation.DamagePlayersEventArg> damagePlayersEvent = new NetEvent1<GameLocation.DamagePlayersEventArg>();

		// Token: 0x040009D6 RID: 2518
		[XmlIgnore]
		public NetVector2HashSet lightGlows = new NetVector2HashSet();

		// Token: 0x040009D7 RID: 2519
		public static readonly int JOURNAL_INDEX = 1000;

		// Token: 0x040009D8 RID: 2520
		public static readonly float FIRST_SECRET_NOTE_CHANCE = 0.8f;

		// Token: 0x040009D9 RID: 2521
		public static readonly float LAST_SECRET_NOTE_CHANCE = 0.12f;

		// Token: 0x040009DA RID: 2522
		public static readonly int NECKLACE_SECRET_NOTE_INDEX = 25;

		// Token: 0x040009DB RID: 2523
		public static readonly string CAROLINES_NECKLACE_ITEM_QID = "(O)191";

		// Token: 0x040009DC RID: 2524
		public static readonly string CAROLINES_NECKLACE_MAIL = "carolinesNecklace";

		// Token: 0x040009DD RID: 2525
		public static TilePositionComparer tilePositionComparer = new TilePositionComparer();

		// Token: 0x040009DE RID: 2526
		protected List<Vector2> _startingCabinLocations = new List<Vector2>();

		// Token: 0x040009DF RID: 2527
		[XmlIgnore]
		public bool wasInhabited;

		// Token: 0x040009E0 RID: 2528
		[XmlIgnore]
		protected bool _madeMapModifications;

		// Token: 0x040009E2 RID: 2530
		public readonly NetCollection<Furniture> furniture = new NetCollection<Furniture>
		{
			InterpolationWait = false
		};

		// Token: 0x040009E3 RID: 2531
		protected readonly NetMutexQueue<Guid> furnitureToRemove = new NetMutexQueue<Guid>();

		// Token: 0x040009E4 RID: 2532
		protected bool _mapPathDirty = true;

		// Token: 0x040009E5 RID: 2533
		protected LocalizedContentManager _structureMapLoader;

		// Token: 0x040009E6 RID: 2534
		protected bool ignoreWarps;

		// Token: 0x040009E7 RID: 2535
		protected HashSet<Vector2> _visitedCollisionTiles = new HashSet<Vector2>();

		// Token: 0x040009E8 RID: 2536
		protected bool _looserBuildRestrictions;

		// Token: 0x040009E9 RID: 2537
		protected Microsoft.Xna.Framework.Rectangle? _buildableTileRect;

		// Token: 0x040009EA RID: 2538
		private bool showedBuildableButNotAlwaysActiveWarning;

		// Token: 0x040009EB RID: 2539
		public static bool PlayedNewLocationContextMusic = false;

		// Token: 0x040009EC RID: 2540
		private const int fireIDBase = 944468;

		// Token: 0x040009ED RID: 2541
		protected Color indoorLightingColor = new Color(100, 120, 30);

		// Token: 0x040009EE RID: 2542
		protected Color indoorLightingNightColor = new Color(150, 150, 30);

		// Token: 0x040009EF RID: 2543
		protected static List<KeyValuePair<string, string>> _PagedResponses = new List<KeyValuePair<string, string>>();

		// Token: 0x040009F0 RID: 2544
		protected static int _PagedResponsePage = 0;

		// Token: 0x040009F1 RID: 2545
		protected static int _PagedResponseItemsPerPage;

		// Token: 0x040009F2 RID: 2546
		public static bool _PagedResponseAddCancel;

		// Token: 0x040009F3 RID: 2547
		protected static string _PagedResponsePrompt;

		// Token: 0x040009F4 RID: 2548
		protected static Action<string> _OnPagedResponse;

		// Token: 0x040009F5 RID: 2549
		protected string _constructLocationBuilderName;

		// Token: 0x040009F6 RID: 2550
		protected List<Farmer> _currentLocationFarmersForDisambiguating = new List<Farmer>();

		// Token: 0x040009F7 RID: 2551
		[XmlIgnore]
		public Dictionary<Vector2, float> lightGlowLayerCache = new Dictionary<Vector2, float>();

		// Token: 0x02000477 RID: 1143
		// (Invoke) Token: 0x06003E48 RID: 15944
		public delegate void afterQuestionBehavior(Farmer who, string whichAnswer);

		// Token: 0x02000478 RID: 1144
		private struct DamagePlayersEventArg : NetEventArg
		{
			// Token: 0x06003E4B RID: 15947 RVA: 0x002F9632 File Offset: 0x002F7832
			public void Read(BinaryReader reader)
			{
				this.Area = reader.ReadRectangle();
				this.Damage = reader.ReadInt32();
				this.IsBomb = reader.ReadBoolean();
			}

			// Token: 0x06003E4C RID: 15948 RVA: 0x002F9658 File Offset: 0x002F7858
			public void Write(BinaryWriter writer)
			{
				writer.WriteRectangle(this.Area);
				writer.Write(this.Damage);
				writer.Write(this.IsBomb);
			}

			// Token: 0x04002855 RID: 10325
			public Microsoft.Xna.Framework.Rectangle Area;

			// Token: 0x04002856 RID: 10326
			public int Damage;

			// Token: 0x04002857 RID: 10327
			public bool IsBomb;
		}
	}
}
