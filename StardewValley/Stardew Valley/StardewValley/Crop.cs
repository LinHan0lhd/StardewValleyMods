using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	// Token: 0x0200008D RID: 141
	public class Crop : INetObject<NetFields>, IHaveModData
	{
		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060005A7 RID: 1447 RVA: 0x0001F211 File Offset: 0x0001D411
		// (set) Token: 0x060005A8 RID: 1448 RVA: 0x0001F219 File Offset: 0x0001D419
		[XmlIgnore]
		public GameLocation currentLocation
		{
			get
			{
				return this.currentLocationImpl;
			}
			set
			{
				if (value != this.currentLocationImpl)
				{
					this.currentLocationImpl = value;
					this.updateDrawMath(this.tilePosition);
				}
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060005A9 RID: 1449 RVA: 0x0001F237 File Offset: 0x0001D437
		// (set) Token: 0x060005AA RID: 1450 RVA: 0x0001F23F File Offset: 0x0001D43F
		[XmlIgnore]
		public HoeDirt Dirt { get; set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060005AB RID: 1451 RVA: 0x0001F248 File Offset: 0x0001D448
		[XmlIgnore]
		public Texture2D DrawnCropTexture
		{
			get
			{
				if (this.dead.Value)
				{
					return Game1.cropSpriteSheet;
				}
				if (this._drawnTexture == null)
				{
					if (this.overrideTexturePath.Value == null)
					{
						NetFieldBase<string, NetString> netFieldBase = this.overrideTexturePath;
						CropData data = this.GetData();
						netFieldBase.Value = ((data != null) ? data.GetCustomTextureName("TileSheets\\crops") : null);
					}
					this._drawnTexture = null;
					if (this.overrideTexturePath.Value != null)
					{
						try
						{
							this._drawnTexture = Game1.content.Load<Texture2D>(this.overrideTexturePath.Value);
						}
						catch (Exception)
						{
							this._drawnTexture = null;
						}
					}
					if (this._drawnTexture == null)
					{
						this._drawnTexture = Game1.cropSpriteSheet;
					}
				}
				return this._drawnTexture;
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x0001F304 File Offset: 0x0001D504
		[XmlIgnore]
		public ModDataDictionary modData { get; } = new ModDataDictionary();

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x0001F30C File Offset: 0x0001D50C
		// (set) Token: 0x060005AE RID: 1454 RVA: 0x0001F319 File Offset: 0x0001D519
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

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x0001F327 File Offset: 0x0001D527
		public NetFields NetFields { get; } = new NetFields("Crop");

		// Token: 0x060005B0 RID: 1456 RVA: 0x0001F330 File Offset: 0x0001D530
		public Crop()
		{
			this.NetFields.SetOwner(this).AddField(this.phaseDays, "phaseDays").AddField(this.rowInSpriteSheet, "rowInSpriteSheet").AddField(this.phaseToShow, "phaseToShow").AddField(this.currentPhase, "currentPhase").AddField(this.indexOfHarvest, "indexOfHarvest").AddField(this.dayOfCurrentPhase, "dayOfCurrentPhase").AddField(this.whichForageCrop, "whichForageCrop").AddField(this.replaceWithObjectOnFullGrown, "replaceWithObjectOnFullGrown").AddField(this.tintColor, "tintColor").AddField(this.flip, "flip").AddField(this.fullyGrown, "fullyGrown").AddField(this.raisedSeeds, "raisedSeeds").AddField(this.programColored, "programColored").AddField(this.dead, "dead").AddField(this.forageCrop, "forageCrop").AddField(this.netSeedIndex, "netSeedIndex").AddField(this.overrideTexturePath, "overrideTexturePath").AddField(this.modData, "modData");
			this.dayOfCurrentPhase.fieldChangeVisibleEvent += delegate(NetInt <p0>, int <p1>, int <p2>)
			{
				this.updateDrawMath(this.tilePosition);
			};
			this.fullyGrown.fieldChangeVisibleEvent += delegate(NetBool <p0>, bool <p1>, bool <p2>)
			{
				this.updateDrawMath(this.tilePosition);
			};
			this.currentLocation = Game1.currentLocation;
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x0001F580 File Offset: 0x0001D780
		public Crop(bool forageCrop, string which, int tileX, int tileY, GameLocation location) : this()
		{
			this.currentLocation = location;
			this.forageCrop.Value = forageCrop;
			this.whichForageCrop.Value = which;
			this.fullyGrown.Value = true;
			this.currentPhase.Value = 5;
			this.updateDrawMath(new Vector2((float)tileX, (float)tileY));
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x0001F5DC File Offset: 0x0001D7DC
		public Crop(string seedId, int tileX, int tileY, GameLocation location) : this()
		{
			this.currentLocation = location;
			seedId = Crop.ResolveSeedId(seedId, location);
			CropData data;
			if (Crop.TryGetData(seedId, out data))
			{
				ParsedItemData harvestItemData = ItemRegistry.GetDataOrErrorItem(data.HarvestItemId);
				if (!harvestItemData.HasTypeObject())
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Crop seed ");
					defaultInterpolatedStringHandler.AppendFormatted(seedId);
					defaultInterpolatedStringHandler.AppendLiteral(" produces non-object item ");
					defaultInterpolatedStringHandler.AppendFormatted(harvestItemData.QualifiedItemId);
					defaultInterpolatedStringHandler.AppendLiteral(", which isn't valid.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this.phaseDays.AddRange(data.DaysInPhase);
				this.phaseDays.Add(99999);
				this.rowInSpriteSheet.Value = data.SpriteIndex;
				this.indexOfHarvest.Value = harvestItemData.ItemId;
				this.overrideTexturePath.Value = data.GetCustomTextureName("TileSheets\\crops");
				if (this.isWildSeedCrop())
				{
					this.whichForageCrop.Value = seedId;
					this.replaceWithObjectOnFullGrown.Value = this.getRandomWildCropForSeason(true);
				}
				else
				{
					this.netSeedIndex.Value = seedId;
				}
				this.raisedSeeds.Value = data.IsRaised;
				List<string> tintColors = data.TintColors;
				if (tintColors != null && tintColors.Count > 0)
				{
					Color? color = Utility.StringToColor(Utility.CreateRandom((double)tileX * 1000.0, (double)tileY, (double)Game1.dayOfMonth, 0.0, 0.0).ChooseFrom(data.TintColors));
					if (color != null)
					{
						this.tintColor.Value = color.Value;
						this.programColored.Value = true;
					}
				}
			}
			else
			{
				this.netSeedIndex.Value = (seedId ?? "0");
				this.indexOfHarvest.Value = (seedId ?? "0");
			}
			this.flip.Value = Game1.random.NextBool();
			this.updateDrawMath(new Vector2((float)tileX, (float)tileY));
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x0001F7E4 File Offset: 0x0001D9E4
		public static string ResolveSeedId(string itemId, GameLocation location)
		{
			if (itemId == "MixedFlowerSeeds")
			{
				return Crop.getRandomFlowerSeedForThisSeason(location.GetSeason());
			}
			if (!(itemId == "770"))
			{
				return itemId;
			}
			string seedId = Crop.getRandomLowGradeCropForThisSeason(location.GetSeason());
			if (seedId == "473")
			{
				seedId = "472";
			}
			if (location is IslandLocation)
			{
				switch (Game1.random.Next(4))
				{
				case 0:
					seedId = "479";
					break;
				case 1:
					seedId = "833";
					break;
				case 2:
					seedId = "481";
					break;
				default:
					seedId = "478";
					break;
				}
			}
			return seedId;
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x0001F884 File Offset: 0x0001DA84
		public CropData GetData()
		{
			CropData data;
			if (!Crop.TryGetData(this.isWildSeedCrop() ? this.whichForageCrop.Value : this.netSeedIndex.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x0001F8BD File Offset: 0x0001DABD
		public static bool TryGetData(string seedId, out CropData data)
		{
			if (seedId == null)
			{
				data = null;
				return false;
			}
			return Game1.cropData.TryGetValue(seedId, out data);
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x0001F8D4 File Offset: 0x0001DAD4
		public bool IsInSeason(GameLocation location)
		{
			if (location.SeedsIgnoreSeasonsHere())
			{
				return true;
			}
			CropData data = this.GetData();
			bool? flag;
			if (data == null)
			{
				flag = null;
			}
			else
			{
				List<Season> seasons = data.Seasons;
				flag = ((seasons != null) ? new bool?(seasons.Contains(location.GetSeason())) : null);
			}
			return flag ?? false;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x0001F938 File Offset: 0x0001DB38
		public static bool IsInSeason(GameLocation location, string seedId)
		{
			if (location.SeedsIgnoreSeasonsHere())
			{
				return true;
			}
			CropData data;
			if (Crop.TryGetData(seedId, out data))
			{
				List<Season> seasons = data.Seasons;
				return ((seasons != null) ? new bool?(seasons.Contains(location.GetSeason())) : null) ?? false;
			}
			return false;
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x0001F993 File Offset: 0x0001DB93
		public HarvestMethod GetHarvestMethod()
		{
			CropData data = this.GetData();
			if (data == null)
			{
				return HarvestMethod.Grab;
			}
			return data.HarvestMethod;
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x0001F9A6 File Offset: 0x0001DBA6
		public bool RegrowsAfterHarvest()
		{
			CropData data = this.GetData();
			return data != null && data.RegrowDays > 0;
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0001F9BC File Offset: 0x0001DBBC
		public virtual bool IsErrorCrop()
		{
			if (this.forageCrop.Value)
			{
				return false;
			}
			if (this._isErrorCrop == null)
			{
				this._isErrorCrop = new bool?(this.GetData() == null);
			}
			return this._isErrorCrop.Value;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x0001F9FC File Offset: 0x0001DBFC
		public virtual void ResetPhaseDays()
		{
			CropData data = this.GetData();
			if (data != null)
			{
				this.phaseDays.Clear();
				this.phaseDays.AddRange(data.DaysInPhase);
				this.phaseDays.Add(99999);
			}
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x0001FA40 File Offset: 0x0001DC40
		public static string getRandomLowGradeCropForThisSeason(Season season)
		{
			if (season == Season.Winter)
			{
				season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
			}
			switch (season)
			{
			case Season.Spring:
				return Game1.random.Next(472, 476).ToString();
			case Season.Summer:
				switch (Game1.random.Next(4))
				{
				case 0:
					return "487";
				case 1:
					return "483";
				case 2:
					return "482";
				default:
					return "484";
				}
				break;
			case Season.Fall:
				return Game1.random.Next(487, 491).ToString();
			default:
				return null;
			}
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x0001FAE8 File Offset: 0x0001DCE8
		public static string getRandomFlowerSeedForThisSeason(Season season)
		{
			if (season == Season.Winter)
			{
				season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
			}
			switch (season)
			{
			case Season.Spring:
				return Game1.random.Choose("427", "429");
			case Season.Summer:
				return Game1.random.Choose("455", "453", "431");
			case Season.Fall:
				return Game1.random.Choose("431", "425");
			default:
				return "-1";
			}
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x0001FB68 File Offset: 0x0001DD68
		public virtual void growCompletely()
		{
			this.currentPhase.Value = this.phaseDays.Count - 1;
			this.dayOfCurrentPhase.Value = 0;
			if (this.RegrowsAfterHarvest())
			{
				this.fullyGrown.Value = true;
			}
			this.updateDrawMath(this.tilePosition);
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x0001FBBC File Offset: 0x0001DDBC
		public virtual bool hitWithHoe(int xTile, int yTile, GameLocation location, HoeDirt dirt)
		{
			if (this.forageCrop.Value && this.whichForageCrop.Value == "2")
			{
				dirt.state.Value = ((location.IsRainingHere() > false) ? 1 : 0);
				Object harvestedItem = ItemRegistry.Create<Object>("(O)829", 1, 0, false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(12, new Vector2((float)(xTile * 64), (float)(yTile * 64)), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
				});
				location.playSound("dirtyHit", null, null, SoundContext.Default);
				Game1.createItemDebris(harvestedItem.getOne(), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1, false);
				return true;
			}
			return false;
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x0001FCA4 File Offset: 0x0001DEA4
		public virtual bool harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null, bool isForcedScytheHarvest = false)
		{
			if (this.dead.Value)
			{
				return junimoHarvester != null;
			}
			bool success = false;
			if (this.forageCrop.Value)
			{
				Object o = null;
				int experience = 3;
				Random r = Utility.CreateDaySaveRandom((double)(xTile * 1000), (double)(yTile * 2000), 0.0);
				string value = this.whichForageCrop.Value;
				if (!(value == "1"))
				{
					if (value == "2")
					{
						soil.shake(0.06544985f, 0.07853982f, (float)(xTile * 64) < Game1.player.Position.X);
						return false;
					}
				}
				else
				{
					o = ItemRegistry.Create<Object>("(O)399", 1, 0, false);
				}
				if (Game1.player.professions.Contains(16))
				{
					o.Quality = 4;
				}
				else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
				{
					o.Quality = 2;
				}
				else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
				{
					o.Quality = 1;
				}
				Game1.stats.ItemsForaged += (uint)o.Stack;
				if (junimoHarvester != null)
				{
					junimoHarvester.tryToAddItemToHut(o);
					return true;
				}
				if (isForcedScytheHarvest)
				{
					Vector2 initialTile = new Vector2((float)xTile, (float)yTile);
					Game1.createItemDebris(o, new Vector2(initialTile.X * 64f + 32f, initialTile.Y * 64f + 32f), -1, null, -1, false);
					Game1.player.gainExperience(2, experience);
					Game1.player.currentLocation.playSound("moss_cut", null, null, SoundContext.Default);
					return true;
				}
				if (Game1.player.addItemToInventoryBool(o, false))
				{
					Vector2 initialTile2 = new Vector2((float)xTile, (float)yTile);
					Game1.player.animateOnce(279 + Game1.player.FacingDirection);
					Game1.player.canMove = false;
					Game1.player.currentLocation.playSound("harvest", null, null, SoundContext.Default);
					DelayedAction.playSoundAfterDelay("coin", 260, null, null, -1, false);
					if (!this.RegrowsAfterHarvest())
					{
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(17, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r.NextBool(), 125f, 0, -1, -1f, -1, 0)
						});
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(14, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r.NextBool(), 50f, 0, -1, -1f, -1, 0)
						});
					}
					Game1.player.gainExperience(2, experience);
					return true;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
			}
			else if (this.currentPhase.Value >= this.phaseDays.Count - 1 && (!this.fullyGrown.Value || this.dayOfCurrentPhase.Value <= 0))
			{
				if (string.IsNullOrWhiteSpace(this.indexOfHarvest.Value))
				{
					return true;
				}
				CropData data = this.GetData();
				Random r2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 0.0);
				int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();
				double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
				double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
				int cropQuality = 0;
				if (fertilizerQualityLevel >= 3 && r2.NextDouble() < chanceForGoldQuality / 2.0)
				{
					cropQuality = 4;
				}
				else if (r2.NextDouble() < chanceForGoldQuality)
				{
					cropQuality = 2;
				}
				else if (r2.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
				{
					cropQuality = 1;
				}
				cropQuality = MathHelper.Clamp(cropQuality, (data != null) ? data.HarvestMinQuality : 0, ((data != null) ? data.HarvestMaxQuality : null).GetValueOrDefault(cropQuality));
				int numToHarvest = 1;
				if (data != null)
				{
					int minStack = data.HarvestMinStack;
					int maxStack = Math.Max(minStack, data.HarvestMaxStack);
					if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
					{
						maxStack += (int)((float)Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
					}
					if (minStack > 1 || maxStack > 1)
					{
						numToHarvest = r2.Next(minStack, maxStack + 1);
					}
				}
				if (data != null && data.ExtraHarvestChance > 0.0)
				{
					while (r2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
					{
						numToHarvest++;
					}
				}
				Item item;
				if (!this.programColored.Value)
				{
					item = ItemRegistry.Create(this.indexOfHarvest.Value, 1, cropQuality, false);
				}
				else
				{
					(item = new ColoredObject(this.indexOfHarvest.Value, 1, this.tintColor.Value)).Quality = cropQuality;
				}
				Item harvestedItem = item;
				HarvestMethod harvestMethod = (data != null) ? data.HarvestMethod : HarvestMethod.Grab;
				if (harvestMethod == HarvestMethod.Scythe || isForcedScytheHarvest)
				{
					if (junimoHarvester != null)
					{
						DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation, null, -1, false);
						if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
						{
							junimoHarvester.currentLocation.playSound("harvest", null, null, SoundContext.Default);
							DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation, null, -1, false);
						}
						junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
					}
					else
					{
						Game1.createItemDebris(harvestedItem.getOne(), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1, false);
					}
					success = true;
				}
				else if (junimoHarvester != null || (harvestedItem != null && Game1.player.addItemToInventoryBool(harvestedItem.getOne(), false)))
				{
					Vector2 initialTile3 = new Vector2((float)xTile, (float)yTile);
					if (junimoHarvester == null)
					{
						Game1.player.animateOnce(279 + Game1.player.FacingDirection);
						Game1.player.canMove = false;
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
					}
					if (r2.NextDouble() < Game1.player.team.AverageLuckLevel(null) / 1500.0 + Game1.player.team.AverageDailyLuck(null) / 1200.0 + 9.999999747378752E-05)
					{
						numToHarvest *= 2;
						if (junimoHarvester == null)
						{
							Game1.player.currentLocation.playSound("dwoop", null, null, SoundContext.Default);
						}
						else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
						{
							junimoHarvester.currentLocation.playSound("dwoop", null, null, SoundContext.Default);
						}
					}
					else if (harvestMethod == HarvestMethod.Grab)
					{
						if (junimoHarvester == null)
						{
							Game1.player.currentLocation.playSound("harvest", null, null, SoundContext.Default);
						}
						else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
						{
							junimoHarvester.currentLocation.playSound("harvest", null, null, SoundContext.Default);
						}
						if (junimoHarvester == null)
						{
							DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation, null, -1, false);
						}
						else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
						{
							DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation, null, -1, false);
						}
						if (!this.RegrowsAfterHarvest() && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
						{
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(17, new Vector2(initialTile3.X * 64f, initialTile3.Y * 64f), Color.White, 7, Game1.random.NextBool(), 125f, 0, -1, -1f, -1, 0)
							});
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(14, new Vector2(initialTile3.X * 64f, initialTile3.Y * 64f), Color.White, 7, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							});
						}
					}
					success = true;
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
				}
				if (success)
				{
					if (this.indexOfHarvest.Value == "421")
					{
						this.indexOfHarvest.Value = "431";
						numToHarvest = r2.Next(1, 4);
					}
					harvestedItem = (this.programColored.Value ? new ColoredObject(this.indexOfHarvest.Value, 1, this.tintColor.Value) : ItemRegistry.Create(this.indexOfHarvest.Value, 1, 0, false));
					int price = 0;
					Object obj = harvestedItem as Object;
					if (obj != null)
					{
						price = obj.Price;
					}
					float experience2 = (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, 2.718281828459045));
					if (junimoHarvester == null)
					{
						Game1.player.gainExperience(0, (int)Math.Round((double)experience2));
					}
					for (int i = 0; i < numToHarvest - 1; i++)
					{
						if (junimoHarvester == null)
						{
							Game1.createItemDebris(harvestedItem.getOne(), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1, false);
						}
						else
						{
							junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
						}
					}
					string value = this.indexOfHarvest.Value;
					if (!(value == "262"))
					{
						if (value == "771")
						{
							if (soil != null)
							{
								GameLocation location = soil.Location;
								if (location != null)
								{
									location.playSound("cut", null, null, SoundContext.Default);
								}
							}
							if (r2.NextDouble() < 0.1)
							{
								Item mixedSeeds = ItemRegistry.Create("(O)770", 1, 0, false);
								if (junimoHarvester == null)
								{
									Game1.createItemDebris(mixedSeeds.getOne(), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1, false);
								}
								else
								{
									junimoHarvester.tryToAddItemToHut(mixedSeeds.getOne());
								}
							}
						}
					}
					else if (r2.NextDouble() < 0.4)
					{
						Item hay_item = ItemRegistry.Create("(O)178", 1, 0, false);
						if (junimoHarvester == null)
						{
							Game1.createItemDebris(hay_item.getOne(), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1, false);
						}
						else
						{
							junimoHarvester.tryToAddItemToHut(hay_item.getOne());
						}
					}
					int regrowDays = (data != null) ? data.RegrowDays : -1;
					if (regrowDays <= 0)
					{
						return true;
					}
					this.fullyGrown.Value = true;
					if (this.dayOfCurrentPhase.Value == regrowDays)
					{
						this.updateDrawMath(this.tilePosition);
					}
					this.dayOfCurrentPhase.Value = regrowDays;
				}
			}
			return false;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x000208A8 File Offset: 0x0001EAA8
		public string getRandomWildCropForSeason(bool onlyDeterministic = false)
		{
			string value = this.whichForageCrop.Value;
			if (value == "495")
			{
				return this.getRandomWildCropForSeason(Season.Spring);
			}
			if (value == "496")
			{
				return this.getRandomWildCropForSeason(Season.Summer);
			}
			if (value == "497")
			{
				return this.getRandomWildCropForSeason(Season.Fall);
			}
			if (value == "498")
			{
				return this.getRandomWildCropForSeason(Season.Winter);
			}
			if (onlyDeterministic && !this.currentLocation.SeedsIgnoreSeasonsHere())
			{
				return null;
			}
			return this.getRandomWildCropForSeason(this.currentLocation.GetSeason());
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x0002093C File Offset: 0x0001EB3C
		public string getRandomWildCropForSeason(Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return Game1.random.Choose("(O)16", "(O)18", "(O)20", "(O)22");
			case Season.Summer:
				return Game1.random.Choose("(O)396", "(O)398", "(O)402");
			case Season.Fall:
				return Game1.random.Choose("(O)404", "(O)406", "(O)408", "(O)410");
			case Season.Winter:
				return Game1.random.Choose("(O)412", "(O)414", "(O)416", "(O)418");
			default:
				return "(O)22";
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x000209E0 File Offset: 0x0001EBE0
		public virtual Rectangle getSourceRect(int number)
		{
			if (this.dead.Value)
			{
				return new Rectangle(192 + number % 4 * 16, 384, 16, 32);
			}
			int effectiveRow = this.rowInSpriteSheet.Value;
			Season localSeason = Game1.GetSeasonForLocation(this.currentLocation);
			if (this.indexOfHarvest.Value == "771")
			{
				if (localSeason != Season.Fall)
				{
					if (localSeason == Season.Winter)
					{
						effectiveRow = this.rowInSpriteSheet.Value + 2;
					}
				}
				else
				{
					effectiveRow = this.rowInSpriteSheet.Value + 1;
				}
			}
			return new Rectangle(Math.Min(240, (this.fullyGrown.Value ? ((this.dayOfCurrentPhase.Value <= 0) ? 6 : 7) : (((this.phaseToShow.Value != -1) ? this.phaseToShow.Value : this.currentPhase.Value) + ((((this.phaseToShow.Value != -1) ? this.phaseToShow.Value : this.currentPhase.Value) == 0 && number % 2 == 0) ? -1 : 0) + 1)) * 16 + ((effectiveRow % 2 != 0) ? 128 : 0)), effectiveRow / 2 * 16 * 2, 16, 32);
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00020B13 File Offset: 0x0001ED13
		public bool TryGetGiantCrops(out IReadOnlyList<KeyValuePair<string, GiantCropData>> giantCrops)
		{
			giantCrops = GiantCrop.GetGiantCropsFor("(O)" + this.indexOfHarvest.Value);
			return giantCrops.Count > 0;
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00020B3B File Offset: 0x0001ED3B
		public void Kill()
		{
			this.dead.Value = true;
			this.raisedSeeds.Value = false;
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00020B58 File Offset: 0x0001ED58
		public virtual void newDay(int state)
		{
			GameLocation environment = this.currentLocation;
			Vector2 tileVector = this.tilePosition;
			Utility.Vector2ToPoint(tileVector);
			if (environment.isOutdoors.Value && (this.dead.Value || !this.IsInSeason(environment)))
			{
				this.Kill();
				return;
			}
			if (state != 1)
			{
				CropData data = this.GetData();
				if (data == null || data.NeedsWatering)
				{
					goto IL_1F4;
				}
			}
			if (!this.fullyGrown.Value)
			{
				this.dayOfCurrentPhase.Value = Math.Min(this.dayOfCurrentPhase.Value + 1, (this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase.Value)] : 0);
			}
			else
			{
				NetInt netInt = this.dayOfCurrentPhase;
				int value = netInt.Value;
				netInt.Value = value - 1;
			}
			if (this.dayOfCurrentPhase.Value >= ((this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase.Value)] : 0) && this.currentPhase.Value < this.phaseDays.Count - 1)
			{
				NetInt netInt2 = this.currentPhase;
				int value = netInt2.Value;
				netInt2.Value = value + 1;
				this.dayOfCurrentPhase.Value = 0;
			}
			while (this.currentPhase.Value < this.phaseDays.Count - 1 && this.phaseDays.Count > 0 && this.phaseDays[this.currentPhase.Value] <= 0)
			{
				NetInt netInt3 = this.currentPhase;
				int value = netInt3.Value;
				netInt3.Value = value + 1;
			}
			if (this.isWildSeedCrop() && this.phaseToShow.Value == -1 && this.currentPhase.Value > 0)
			{
				this.phaseToShow.Value = Game1.random.Next(1, 7);
			}
			this.TryGrowGiantCrop(true, null);
			IL_1F4:
			if ((!this.fullyGrown.Value || this.dayOfCurrentPhase.Value <= 0) && this.currentPhase.Value >= this.phaseDays.Count - 1)
			{
				if (this.replaceWithObjectOnFullGrown.Value != null || this.isWildSeedCrop())
				{
					Object obj;
					if (environment.objects.TryGetValue(tileVector, out obj))
					{
						IndoorPot pot = obj as IndoorPot;
						if (pot != null)
						{
							pot.heldObject.Value = ItemRegistry.Create<Object>(this.replaceWithObjectOnFullGrown.Value ?? this.getRandomWildCropForSeason(false), 1, 0, false);
							pot.hoeDirt.Value.crop = null;
						}
						else
						{
							environment.objects.Remove(tileVector);
						}
					}
					if (!environment.objects.ContainsKey(tileVector))
					{
						Object spawned = ItemRegistry.Create<Object>(this.replaceWithObjectOnFullGrown.Value ?? this.getRandomWildCropForSeason(false), 1, 0, false);
						spawned.IsSpawnedObject = true;
						spawned.CanBeGrabbed = true;
						spawned.SpecialVariable = 724519;
						environment.objects.Add(tileVector, spawned);
					}
					TerrainFeature terrainFeature;
					if (environment.terrainFeatures.TryGetValue(tileVector, out terrainFeature))
					{
						HoeDirt dirt = terrainFeature as HoeDirt;
						if (dirt != null)
						{
							dirt.crop = null;
						}
					}
				}
				if (this.indexOfHarvest.Value != null && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value.Length > 0 && environment.IsFarm)
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						farmer.autoGenerateActiveDialogueEvent("cropMatured_" + this.indexOfHarvest.Value, 4);
					}
				}
			}
			if (this.fullyGrown.Value && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value == "595")
			{
				Game1.getFarm().hasMatureFairyRoseTonight = true;
			}
			this.updateDrawMath(tileVector);
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00020F70 File Offset: 0x0001F170
		public virtual bool TryGrowGiantCrop(bool checkPreconditions = true, Random random = null)
		{
			GameLocation environment = this.currentLocation;
			Vector2 tile = this.tilePosition;
			if (checkPreconditions)
			{
				if (!(environment is Farm) && !environment.HasMapPropertyWithValue("AllowGiantCrops"))
				{
					return false;
				}
				if (this.currentPhase.Value != this.phaseDays.Count - 1)
				{
					return false;
				}
			}
			IReadOnlyList<KeyValuePair<string, GiantCropData>> possibleGiantCrops;
			if (!this.TryGetGiantCrops(out possibleGiantCrops))
			{
				return false;
			}
			foreach (KeyValuePair<string, GiantCropData> pair in possibleGiantCrops)
			{
				string giantCropId = pair.Key;
				GiantCropData giantCrop = pair.Value;
				if ((giantCrop.Chance >= 1f || (random ?? Utility.CreateDaySaveRandom((double)tile.X, (double)tile.Y, (double)Game1.hash.GetDeterministicHashCode(giantCropId))).NextBool(giantCrop.Chance)) && GameStateQuery.CheckConditions(giantCrop.Condition, environment, null, null, null, null, null))
				{
					bool valid = true;
					int y = (int)tile.Y;
					while ((float)y < tile.Y + (float)giantCrop.TileSize.Y)
					{
						int x = (int)tile.X;
						while ((float)x < tile.X + (float)giantCrop.TileSize.X)
						{
							HoeDirt dirt = environment.terrainFeatures.GetValueOrDefault(new Vector2((float)x, (float)y), null) as HoeDirt;
							if (dirt != null)
							{
								Crop crop = dirt.crop;
								if (((crop != null) ? crop.indexOfHarvest.Value : null) == this.indexOfHarvest.Value)
								{
									x++;
									continue;
								}
							}
							valid = false;
							break;
						}
						if (!valid)
						{
							break;
						}
						y++;
					}
					if (valid)
					{
						int y2 = (int)tile.Y;
						while ((float)y2 < tile.Y + (float)giantCrop.TileSize.Y)
						{
							int x2 = (int)tile.X;
							while ((float)x2 < tile.X + (float)giantCrop.TileSize.X)
							{
								Vector2 v = new Vector2((float)x2, (float)y2);
								((HoeDirt)environment.terrainFeatures[v]).crop = null;
								x2++;
							}
							y2++;
						}
						environment.resourceClumps.Add(new GiantCrop(giantCropId, tile));
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x000211D0 File Offset: 0x0001F3D0
		public virtual bool isPaddyCrop()
		{
			CropData data = this.GetData();
			return ((data != null) ? new bool?(data.IsPaddyCrop) : null) ?? false;
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x0002120F File Offset: 0x0001F40F
		public virtual bool shouldDrawDarkWhenWatered()
		{
			return !this.isPaddyCrop() && !this.raisedSeeds.Value;
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00021229 File Offset: 0x0001F429
		public virtual bool isWildSeedCrop()
		{
			return (this.overrideTexturePath.Value == null || this.overrideTexturePath.Value == Game1.cropSpriteSheet.Name) && this.rowInSpriteSheet.Value == 23;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00021268 File Offset: 0x0001F468
		public virtual void updateDrawMath(Vector2 tileLocation)
		{
			if (tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			if (this.forageCrop.Value)
			{
				int which_forage_crop;
				if (!int.TryParse(this.whichForageCrop.Value, out which_forage_crop))
				{
					which_forage_crop = 1;
				}
				this.drawPosition = new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 32f);
				this.layerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f;
				this.sourceRect = new Rectangle((int)(tileLocation.X * 51f + tileLocation.Y * 77f) % 3 * 16, 128 + which_forage_crop * 16, 16, 16);
			}
			else
			{
				this.drawPosition = new Vector2(tileLocation.X * 64f + ((!this.shouldDrawDarkWhenWatered() || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f)) + 32f, tileLocation.Y * 64f + ((this.raisedSeeds.Value || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) + 32f);
				this.layerDepth = (tileLocation.Y * 64f + 32f + ((!this.shouldDrawDarkWhenWatered() || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / ((this.currentPhase.Value == 0 && this.shouldDrawDarkWhenWatered()) ? 2f : 1f);
				this.sourceRect = this.getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
				this.coloredSourceRect = new Rectangle((this.fullyGrown.Value ? ((this.dayOfCurrentPhase.Value <= 0) ? 6 : 7) : (this.currentPhase.Value + 1 + 1)) * 16 + ((this.rowInSpriteSheet.Value % 2 != 0) ? 128 : 0), this.rowInSpriteSheet.Value / 2 * 16 * 2, 16, 32);
				this.coloredLayerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f / (float)((this.currentPhase.Value == 0 && this.shouldDrawDarkWhenWatered()) ? 2 : 1);
			}
			this.tilePosition = tileLocation;
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x000215FC File Offset: 0x0001F7FC
		public virtual void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
		{
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, this.drawPosition);
			if (this.forageCrop.Value)
			{
				if (this.whichForageCrop.Value == "2")
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 64f)), new Rectangle?(new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(tileLocation.X * 111f + tileLocation.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16)), Color.White, rotation, new Vector2(8f, 16f), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f);
					return;
				}
				b.Draw(Game1.mouseCursors, position, new Rectangle?(this.sourceRect), Color.White, 0f, Crop.smallestTileSizeOrigin, 4f, SpriteEffects.None, this.layerDepth);
				return;
			}
			else
			{
				if (this.IsErrorCrop())
				{
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
					b.Draw(itemData.GetTexture(), position, new Rectangle?(itemData.GetSourceRect(0, null)), toTint, rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, this.layerDepth);
					return;
				}
				SpriteEffects effect = this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				b.Draw(this.DrawnCropTexture, position, new Rectangle?(this.sourceRect), toTint, rotation, Crop.origin, 4f, effect, this.layerDepth);
				Color tintColor = this.tintColor.Value;
				if (!tintColor.Equals(Color.White) && this.currentPhase.Value == this.phaseDays.Count - 1 && !this.dead.Value)
				{
					b.Draw(this.DrawnCropTexture, position, new Rectangle?(this.coloredSourceRect), tintColor, rotation, Crop.origin, 4f, effect, this.coloredLayerDepth);
				}
				return;
			}
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x000218D0 File Offset: 0x0001FAD0
		public virtual void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
		{
			if (this.IsErrorCrop())
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
				b.Draw(itemData.GetTexture(), screenPosition, new Rectangle?(itemData.GetSourceRect(0, null)), toTint, rotation, new Vector2(32f, 32f), scale, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
				return;
			}
			b.Draw(this.DrawnCropTexture, screenPosition, new Rectangle?(this.getSourceRect(0)), toTint, rotation, new Vector2(32f, 96f), scale, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x0002198C File Offset: 0x0001FB8C
		public virtual void drawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
		{
			if (this.IsErrorCrop())
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
				b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(itemData.GetSourceRect(0, null)), toTint, rotation, new Vector2(8f, 8f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
				return;
			}
			if (this.forageCrop.Value)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(this.sourceRect), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
				return;
			}
			b.Draw(this.DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(this.sourceRect), toTint, rotation, new Vector2(8f, 24f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			if (!this.tintColor.Equals(Color.White) && this.currentPhase.Value == this.phaseDays.Count - 1 && !this.dead.Value)
			{
				b.Draw(this.DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(this.coloredSourceRect), this.tintColor.Value, rotation, new Vector2(8f, 24f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.67f) * 64f / 10000f + tileLocation.X * 1E-05f);
			}
		}

		// Token: 0x040002C9 RID: 713
		public const string mixedSeedsId = "770";

		// Token: 0x040002CA RID: 714
		public const string mixedSeedsQId = "(O)770";

		// Token: 0x040002CB RID: 715
		public const int seedPhase = 0;

		// Token: 0x040002CC RID: 716
		public const int rowOfWildSeeds = 23;

		// Token: 0x040002CD RID: 717
		public const int finalPhaseLength = 99999;

		// Token: 0x040002CE RID: 718
		public const int forageCrop_springOnion = 1;

		// Token: 0x040002CF RID: 719
		public const string forageCrop_springOnionID = "1";

		// Token: 0x040002D0 RID: 720
		public const int forageCrop_ginger = 2;

		// Token: 0x040002D1 RID: 721
		public const string forageCrop_gingerID = "2";

		// Token: 0x040002D2 RID: 722
		public const int specialVariable_farmedForageCrop = 724519;

		// Token: 0x040002D3 RID: 723
		private GameLocation currentLocationImpl;

		// Token: 0x040002D4 RID: 724
		public readonly NetIntList phaseDays = new NetIntList();

		// Token: 0x040002D5 RID: 725
		[XmlElement("rowInSpriteSheet")]
		public readonly NetInt rowInSpriteSheet = new NetInt();

		// Token: 0x040002D6 RID: 726
		[XmlElement("phaseToShow")]
		public readonly NetInt phaseToShow = new NetInt(-1);

		// Token: 0x040002D7 RID: 727
		[XmlElement("currentPhase")]
		public readonly NetInt currentPhase = new NetInt();

		// Token: 0x040002D8 RID: 728
		[XmlElement("indexOfHarvest")]
		public readonly NetString indexOfHarvest = new NetString();

		// Token: 0x040002D9 RID: 729
		[XmlElement("dayOfCurrentPhase")]
		public readonly NetInt dayOfCurrentPhase = new NetInt();

		// Token: 0x040002DA RID: 730
		[XmlElement("whichForageCrop")]
		public readonly NetString whichForageCrop = new NetString();

		// Token: 0x040002DB RID: 731
		[XmlElement("overrideHarvestItemId")]
		public readonly NetString replaceWithObjectOnFullGrown = new NetString();

		// Token: 0x040002DC RID: 732
		[XmlElement("tintColor")]
		public readonly NetColor tintColor = new NetColor();

		// Token: 0x040002DD RID: 733
		[XmlElement("flip")]
		public readonly NetBool flip = new NetBool();

		// Token: 0x040002DE RID: 734
		[XmlElement("fullGrown")]
		public readonly NetBool fullyGrown = new NetBool();

		// Token: 0x040002DF RID: 735
		[XmlElement("raisedSeeds")]
		public readonly NetBool raisedSeeds = new NetBool();

		// Token: 0x040002E0 RID: 736
		[XmlElement("programColored")]
		public readonly NetBool programColored = new NetBool();

		// Token: 0x040002E1 RID: 737
		[XmlElement("dead")]
		public readonly NetBool dead = new NetBool();

		// Token: 0x040002E2 RID: 738
		[XmlElement("forageCrop")]
		public readonly NetBool forageCrop = new NetBool();

		// Token: 0x040002E3 RID: 739
		[XmlElement("seedIndex")]
		public readonly NetString netSeedIndex = new NetString();

		// Token: 0x040002E4 RID: 740
		[XmlElement("overrideTexturePath")]
		public readonly NetString overrideTexturePath = new NetString();

		// Token: 0x040002E5 RID: 741
		protected Texture2D _drawnTexture;

		// Token: 0x040002E6 RID: 742
		protected bool? _isErrorCrop;

		// Token: 0x040002EA RID: 746
		[XmlIgnore]
		public Vector2 drawPosition;

		// Token: 0x040002EB RID: 747
		[XmlIgnore]
		public Vector2 tilePosition;

		// Token: 0x040002EC RID: 748
		[XmlIgnore]
		public float layerDepth;

		// Token: 0x040002ED RID: 749
		[XmlIgnore]
		public float coloredLayerDepth;

		// Token: 0x040002EE RID: 750
		[XmlIgnore]
		public Rectangle sourceRect;

		// Token: 0x040002EF RID: 751
		[XmlIgnore]
		public Rectangle coloredSourceRect;

		// Token: 0x040002F0 RID: 752
		private static Vector2 origin = new Vector2(8f, 24f);

		// Token: 0x040002F1 RID: 753
		private static Vector2 smallestTileSizeOrigin = new Vector2(8f, 8f);
	}
}
