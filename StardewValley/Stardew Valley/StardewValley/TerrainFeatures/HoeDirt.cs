using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000144 RID: 324
	public class HoeDirt : TerrainFeature
	{
		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x060019B4 RID: 6580 RVA: 0x0012F30C File Offset: 0x0012D50C
		// (set) Token: 0x060019B5 RID: 6581 RVA: 0x0012F314 File Offset: 0x0012D514
		[XmlIgnore]
		public override GameLocation Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
				if (this.netCrop.Value != null)
				{
					this.netCrop.Value.currentLocation = value;
				}
			}
		}

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x060019B6 RID: 6582 RVA: 0x0012F33B File Offset: 0x0012D53B
		// (set) Token: 0x060019B7 RID: 6583 RVA: 0x0012F343 File Offset: 0x0012D543
		public override Vector2 Tile
		{
			get
			{
				return base.Tile;
			}
			set
			{
				base.Tile = value;
				if (this.netCrop.Value != null)
				{
					this.netCrop.Value.tilePosition = value;
				}
			}
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x060019B8 RID: 6584 RVA: 0x0012F36A File Offset: 0x0012D56A
		// (set) Token: 0x060019B9 RID: 6585 RVA: 0x0012F377 File Offset: 0x0012D577
		public Crop crop
		{
			get
			{
				return this.netCrop.Value;
			}
			set
			{
				this.netCrop.Value = value;
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x060019BA RID: 6586 RVA: 0x0012F385 File Offset: 0x0012D585
		// (set) Token: 0x060019BB RID: 6587 RVA: 0x0012F38D File Offset: 0x0012D58D
		[XmlIgnore]
		public IndoorPot Pot { get; set; }

		// Token: 0x060019BC RID: 6588 RVA: 0x0012F398 File Offset: 0x0012D598
		public HoeDirt() : base(true)
		{
			this.loadSprite();
			if (HoeDirt.drawGuide == null)
			{
				HoeDirt.populateDrawGuide();
			}
			this.initialize(Game1.currentLocation);
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x0012F41C File Offset: 0x0012D61C
		public HoeDirt(int startingState, GameLocation location = null) : this()
		{
			this.state.Value = startingState;
			this.Location = (location ?? Game1.currentLocation);
			if (location != null)
			{
				this.initialize(location);
			}
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x0012F44A File Offset: 0x0012D64A
		public HoeDirt(int startingState, Crop crop) : this()
		{
			this.state.Value = startingState;
			this.crop = crop;
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x0012F468 File Offset: 0x0012D668
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.netCrop, "netCrop").AddField(this.state, "state").AddField(this.fertilizer, "fertilizer").AddField(this.c, "c").AddField(this.nearWaterForPaddy, "nearWaterForPaddy");
			this.state.fieldChangeVisibleEvent += delegate(NetInt x, int y, int z)
			{
				this.OnAdded(this.Location, this.Tile);
			};
			this.netCrop.fieldChangeVisibleEvent += delegate(NetRef<Crop> x, Crop y, Crop z)
			{
				this.nearWaterForPaddy.Value = -1;
				this.updateNeighbors();
				if (this.netCrop.Value != null)
				{
					this.netCrop.Value.Dirt = this;
					this.netCrop.Value.currentLocation = this.Location;
					this.netCrop.Value.updateDrawMath(this.Tile);
				}
			};
			this.nearWaterForPaddy.Interpolated(false, false);
			this.netCrop.Interpolated(false, false);
			this.netCrop.OnConflictResolve += delegate(Crop rejected, Crop accepted)
			{
				if (!Game1.IsMasterGame)
				{
					return;
				}
				if (rejected != null && rejected.netSeedIndex.Value != null)
				{
					this.queuedActions.Add(delegate(GameLocation gLocation, Vector2 tileLocation)
					{
						Vector2 pos = tileLocation * 64f;
						gLocation.debris.Add(new Debris(rejected.netSeedIndex.Value, pos, pos));
					});
					base.NeedsUpdate = true;
				}
			};
		}

		// Token: 0x060019C0 RID: 6592 RVA: 0x0012F534 File Offset: 0x0012D734
		private void initialize(GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if (location != null)
			{
				MineShaft mine = location as MineShaft;
				if (mine != null)
				{
					int mineArea = mine.getMineArea(-1);
					if (mine.GetAdditionalDifficulty() > 0)
					{
						if (mineArea == 0 || mineArea == 10)
						{
							this.c.Value = new Color(80, 100, 140) * 0.5f;
							return;
						}
					}
					else if (mineArea == 80)
					{
						this.c.Value = Color.MediumPurple * 0.4f;
						return;
					}
				}
				else
				{
					if (location.GetSeason() == Season.Fall && location.IsOutdoors && !(location is Beach))
					{
						this.c.Value = new Color(250, 210, 240);
						return;
					}
					if (location is VolcanoDungeon)
					{
						this.c.Value = Color.MediumPurple * 0.7f;
					}
				}
			}
		}

		// Token: 0x060019C1 RID: 6593 RVA: 0x0012F616 File Offset: 0x0012D816
		public float getShakeRotation()
		{
			return this.shakeRotation;
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x0012F61E File Offset: 0x0012D81E
		public float getMaxShake()
		{
			return this.maxShake;
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x0012F628 File Offset: 0x0012D828
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		// Token: 0x060019C4 RID: 6596 RVA: 0x0012F660 File Offset: 0x0012D860
		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
		{
			if (this.crop != null && this.crop.currentPhase.Value != 0 && speedOfCollision > 0 && this.maxShake == 0f && positionOfCollider.Intersects(this.getBoundingBox()) && Utility.isOnScreen(Utility.Vector2ToPoint(tileLocation), 64, this.Location))
			{
				if (!(who is FarmAnimal))
				{
					Grass.PlayGrassSound();
				}
				this.shake(0.3926991f / Math.Min(1f, 5f / (float)speedOfCollision) - ((speedOfCollision > 2) ? ((float)this.crop.currentPhase.Value * 3.1415927f / 64f) : 0f), 0.03926991f / Math.Min(1f, 5f / (float)speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
			}
			if (this.crop != null && this.crop.currentPhase.Value != 0)
			{
				Farmer player = who as Farmer;
				if (player != null && player.running)
				{
					if (player.stats.Get("Book_Grass") > 0U)
					{
						player.temporarySpeedBuff = -0.33f;
						return;
					}
					player.temporarySpeedBuff = -1f;
				}
			}
		}

		// Token: 0x060019C5 RID: 6597 RVA: 0x0012F7B4 File Offset: 0x0012D9B4
		public void shake(float shake, float rate, bool left)
		{
			if (this.crop != null)
			{
				this.maxShake = shake * (this.crop.raisedSeeds.Value ? 0.6f : 1.5f);
				this.shakeRate = rate * 0.5f;
				this.shakeRotation = 0f;
				this.shakeLeft = left;
			}
			base.NeedsUpdate = true;
		}

		// Token: 0x060019C6 RID: 6598 RVA: 0x0012F815 File Offset: 0x0012DA15
		public bool needsWatering()
		{
			if (this.crop != null && (!this.readyForHarvest() || this.crop.RegrowsAfterHarvest()))
			{
				CropData data = this.crop.GetData();
				return data == null || data.NeedsWatering;
			}
			return false;
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x0012F84C File Offset: 0x0012DA4C
		public bool isWatered()
		{
			return this.state.Value == 1;
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x0012F85C File Offset: 0x0012DA5C
		public static void populateDrawGuide()
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary[0] = 0;
			dictionary[8] = 15;
			dictionary[2] = 13;
			dictionary[1] = 12;
			dictionary[4] = 4;
			dictionary[9] = 11;
			dictionary[3] = 9;
			dictionary[5] = 8;
			dictionary[6] = 1;
			dictionary[12] = 3;
			dictionary[10] = 14;
			dictionary[7] = 5;
			dictionary[15] = 6;
			dictionary[13] = 7;
			dictionary[11] = 10;
			dictionary[14] = 2;
			HoeDirt.drawGuide = dictionary;
		}

		// Token: 0x060019C9 RID: 6601 RVA: 0x0012F904 File Offset: 0x0012DB04
		public override void loadSprite()
		{
			if (HoeDirt.lightTexture == null)
			{
				try
				{
					HoeDirt.lightTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirt");
				}
				catch (Exception)
				{
				}
			}
			if (HoeDirt.darkTexture == null)
			{
				try
				{
					HoeDirt.darkTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtDark");
				}
				catch (Exception)
				{
				}
			}
			if (HoeDirt.snowTexture == null)
			{
				try
				{
					HoeDirt.snowTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtSnow");
				}
				catch (Exception)
				{
				}
			}
			this.nearWaterForPaddy.Value = -1;
			Crop crop = this.crop;
			if (crop == null)
			{
				return;
			}
			crop.updateDrawMath(this.Tile);
		}

		// Token: 0x060019CA RID: 6602 RVA: 0x0012F9BC File Offset: 0x0012DBBC
		public override bool isPassable(Character c = null)
		{
			return this.crop == null || !this.crop.raisedSeeds.Value || c is JunimoHarvester;
		}

		// Token: 0x060019CB RID: 6603 RVA: 0x0012F9E4 File Offset: 0x0012DBE4
		public bool readyForHarvest()
		{
			return this.crop != null && (!this.crop.fullyGrown.Value || this.crop.dayOfCurrentPhase.Value <= 0) && this.crop.currentPhase.Value >= this.crop.phaseDays.Count - 1 && !this.crop.dead.Value && (!this.crop.forageCrop.Value || this.crop.whichForageCrop.Value != "2");
		}

		// Token: 0x060019CC RID: 6604 RVA: 0x0012FA88 File Offset: 0x0012DC88
		public override bool performUseAction(Vector2 tileLocation)
		{
			if (this.crop != null)
			{
				bool harvestable = this.crop.currentPhase.Value >= this.crop.phaseDays.Count - 1 && (!this.crop.fullyGrown.Value || this.crop.dayOfCurrentPhase.Value <= 0);
				HarvestMethod harvestMethod = this.crop.GetHarvestMethod();
				if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isScythe() && Game1.player.CurrentTool.ItemId == "66")
				{
					harvestMethod = HarvestMethod.Scythe;
				}
				if (harvestMethod != HarvestMethod.Grab)
				{
					if (harvestMethod == HarvestMethod.Scythe)
					{
						if (this.readyForHarvest())
						{
							Tool currentTool = Game1.player.CurrentTool;
							if (currentTool != null && currentTool.isScythe())
							{
								Game1.player.CanMove = false;
								Game1.player.UsingTool = true;
								Game1.player.canReleaseTool = true;
								Game1.player.Halt();
								try
								{
									Game1.player.CurrentTool.beginUsing(Game1.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
								}
								catch (Exception)
								{
								}
								((MeleeWeapon)Game1.player.CurrentTool).setFarmerAnimating(Game1.player);
							}
							else if (Game1.didPlayerJustClickAtAll(true))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13915"), true);
							}
						}
					}
				}
				else if (this.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this, null, false))
				{
					GameLocation location = this.Location;
					if (location is IslandLocation && Game1.random.NextDouble() < 0.05)
					{
						Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5, 1);
					}
					this.destroyCrop(false);
					return true;
				}
				return harvestable;
			}
			return false;
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x0012FC9C File Offset: 0x0012DE9C
		public bool plant(string itemId, Farmer who, bool isFertilizer)
		{
			GameLocation location = this.Location;
			if (isFertilizer)
			{
				if (!this.CanApplyFertilizer(itemId))
				{
					return false;
				}
				this.fertilizer.Value = (ItemRegistry.QualifyItemId(itemId) ?? itemId);
				this.applySpeedIncreases(who);
				location.playSound("dirtyHit", null, null, SoundContext.Default);
				return true;
			}
			else
			{
				Season season = location.GetSeason();
				Point tilePos = Utility.Vector2ToPoint(this.Tile);
				itemId = Crop.ResolveSeedId(itemId, location);
				CropData cropData;
				if (!Crop.TryGetData(itemId, out cropData) || cropData.Seasons.Count == 0)
				{
					return false;
				}
				Object obj;
				bool isGardenPot = location.objects.TryGetValue(this.Tile, out obj) && obj is IndoorPot;
				bool isIndoorPot = isGardenPot && !location.IsOutdoors;
				GameLocation currentLocation = who.currentLocation;
				string itemId2 = itemId;
				bool isGardenPot2 = isGardenPot;
				bool defaultAllowed;
				if (!isIndoorPot)
				{
					LocationData data = location.GetData();
					defaultAllowed = (((data != null) ? data.CanPlantHere : null) ?? location.IsFarm);
				}
				else
				{
					defaultAllowed = true;
				}
				string deniedMessage;
				if (!currentLocation.CheckItemPlantRules(itemId2, isGardenPot2, defaultAllowed, out deniedMessage))
				{
					if (Game1.didPlayerJustClickAtAll(true))
					{
						if (deniedMessage == null && location.NameOrUniqueName != "Farm")
						{
							Farm farm = Game1.getFarm();
							GameLocation gameLocation = farm;
							string itemId3 = itemId;
							bool isGardenPot3 = isGardenPot;
							LocationData data2 = farm.GetData();
							string text;
							if (gameLocation.CheckItemPlantRules(itemId3, isGardenPot3, ((data2 != null) ? data2.CanPlantHere : null).GetValueOrDefault(true), out text))
							{
								deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919");
							}
						}
						if (deniedMessage == null)
						{
							deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925");
						}
						Game1.showRedMessage(deniedMessage, true);
					}
					return false;
				}
				if (!isIndoorPot && !who.currentLocation.CanPlantSeedsHere(itemId, tilePos.X, tilePos.Y, isGardenPot, out deniedMessage))
				{
					if (Game1.didPlayerJustClickAtAll(true))
					{
						if (deniedMessage == null)
						{
							deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925");
						}
						Game1.showRedMessage(deniedMessage, true);
					}
					return false;
				}
				if (!isIndoorPot && !location.SeedsIgnoreSeasonsHere())
				{
					List<Season> seasons = cropData.Seasons;
					bool? flag = (seasons != null) ? new bool?(seasons.Contains(season)) : null;
					if (flag == null || !flag.GetValueOrDefault())
					{
						if (Game1.didPlayerJustClickAtAll(true))
						{
							List<Season> seasons2 = cropData.Seasons;
							flag = ((seasons2 != null) ? new bool?(seasons2.Contains(season)) : null);
							string errorKey = (flag != null && !flag.GetValueOrDefault()) ? "Strings\\StringsFromCSFiles:HoeDirt.cs.13924" : "Strings\\StringsFromCSFiles:HoeDirt.cs.13925";
							Game1.showRedMessage(Game1.content.LoadString(errorKey), true);
						}
						return false;
					}
				}
				this.crop = new Crop(itemId, tilePos.X, tilePos.Y, this.Location);
				if (this.crop.raisedSeeds.Value)
				{
					location.playSound("stoneStep", null, null, SoundContext.Default);
				}
				location.playSound("dirtyHit", null, null, SoundContext.Default);
				Stats stats = Game1.stats;
				uint seedsSown = stats.SeedsSown;
				stats.SeedsSown = seedsSown + 1U;
				this.applySpeedIncreases(who);
				this.nearWaterForPaddy.Value = -1;
				if (this.hasPaddyCrop() && this.paddyWaterCheck(false))
				{
					this.state.Value = 1;
					this.updateNeighbors();
				}
				return true;
			}
		}

		// Token: 0x060019CE RID: 6606 RVA: 0x0012FFF0 File Offset: 0x0012E1F0
		public void applySpeedIncreases(Farmer who)
		{
			if (this.crop == null)
			{
				return;
			}
			bool paddy_bonus = this.Location != null && this.paddyWaterCheck(false);
			float fertilizerSpeedBoost = this.GetFertilizerSpeedBoost();
			if (fertilizerSpeedBoost != 0f || who.professions.Contains(5) || paddy_bonus)
			{
				this.crop.ResetPhaseDays();
				int totalDaysOfCropGrowth = 0;
				for (int i = 0; i < this.crop.phaseDays.Count - 1; i++)
				{
					totalDaysOfCropGrowth += this.crop.phaseDays[i];
				}
				float speedIncrease = fertilizerSpeedBoost;
				if (paddy_bonus)
				{
					speedIncrease += 0.25f;
				}
				if (who.professions.Contains(5))
				{
					speedIncrease += 0.1f;
				}
				int daysToRemove = (int)Math.Ceiling((double)((float)totalDaysOfCropGrowth * speedIncrease));
				int tries = 0;
				while (daysToRemove > 0 && tries < 3)
				{
					for (int j = 0; j < this.crop.phaseDays.Count; j++)
					{
						if ((j > 0 || this.crop.phaseDays[j] > 1) && this.crop.phaseDays[j] != 99999 && this.crop.phaseDays[j] > 0)
						{
							NetIntList phaseDays = this.crop.phaseDays;
							int index = j;
							int num = phaseDays[index];
							phaseDays[index] = num - 1;
							daysToRemove--;
						}
						if (daysToRemove <= 0)
						{
							break;
						}
					}
					tries++;
				}
			}
		}

		// Token: 0x060019CF RID: 6607 RVA: 0x00130168 File Offset: 0x0012E368
		public void destroyCrop(bool showAnimation)
		{
			GameLocation location = this.Location;
			if (this.crop != null && showAnimation && location != null)
			{
				Vector2 tileLocation = this.Tile;
				if (this.crop.currentPhase.Value < 1 && !this.crop.dead.Value)
				{
					Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(12, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
					});
					location.playSound("dirtyHit", new Vector2?(tileLocation), null, SoundContext.Default);
				}
				else
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(50, tileLocation * 64f, this.crop.dead.Value ? new Color(207, 193, 43) : Color.ForestGreen, 8, false, 100f, 0, -1, -1f, -1, 0)
					});
				}
			}
			this.crop = null;
			this.nearWaterForPaddy.Value = -1;
			if (location != null)
			{
				this.updateNeighbors();
			}
		}

		// Token: 0x060019D0 RID: 6608 RVA: 0x0013029C File Offset: 0x0012E49C
		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			if (t != null)
			{
				if (!(t is Hoe))
				{
					if (!(t is Pickaxe))
					{
						if (t is WateringCan)
						{
							if (this.crop == null || !this.crop.forageCrop.Value || this.crop.whichForageCrop.Value != "2")
							{
								this.state.Value = 1;
								goto IL_379;
							}
							goto IL_379;
						}
					}
					else if (this.crop == null)
					{
						return true;
					}
					if (t.isScythe())
					{
						Crop crop = this.crop;
						if ((crop != null && crop.GetHarvestMethod() == HarvestMethod.Scythe) || (this.crop != null && t.ItemId == "66"))
						{
							if (this.crop.indexOfHarvest.Value == "771" && t.hasEnchantmentOfType<HaymakerEnchantment>())
							{
								for (int i = 0; i < 2; i++)
								{
									Game1.createItemDebris(ItemRegistry.Create("(O)771", 1, 0, false), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1, null, -1, false);
								}
							}
							if (this.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this, null, true))
							{
								if (location is IslandLocation && Game1.random.NextDouble() < 0.05)
								{
									Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5, 1);
								}
								this.destroyCrop(true);
							}
						}
						if (this.crop != null && this.crop.dead.Value)
						{
							this.destroyCrop(true);
						}
						Object tileObj;
						if (this.crop == null && t.ItemId == "66" && location.objects.TryGetValue(tileLocation, out tileObj) && tileObj.isForage())
						{
							Farmer player = t.getLastFarmerToUse() ?? Game1.player;
							tileObj.Quality = location.GetHarvestSpawnedObjectQuality(player, tileObj.isForage(), tileObj.TileLocation, null);
							Vector2 spawnPosition = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f);
							Game1.createItemDebris(tileObj, spawnPosition, -1, null, -1, false);
							location.OnHarvestedForage(player, tileObj);
							location.objects.Remove(tileLocation);
							if (player.professions.Contains(13) && Game1.random.NextDouble() < 0.2)
							{
								Object extraDrop = (Object)tileObj.getOne();
								extraDrop.Quality = location.GetHarvestSpawnedObjectQuality(player, extraDrop.isForage(), extraDrop.TileLocation, null);
								Game1.createItemDebris(extraDrop, spawnPosition, -1, null, -1, false);
								location.OnHarvestedForage(player, extraDrop);
							}
						}
					}
					else if (t.isHeavyHitter() && !(t is MeleeWeapon) && this.crop != null)
					{
						this.destroyCrop(true);
					}
				}
				else if (this.crop != null && this.crop.hitWithHoe((int)tileLocation.X, (int)tileLocation.Y, location, this))
				{
					if (this.crop.forageCrop.Value && this.crop.whichForageCrop.Value == "2" && t.getLastFarmerToUse() != null)
					{
						t.getLastFarmerToUse().gainExperience(2, 7);
					}
					this.destroyCrop(true);
				}
				IL_379:
				this.shake(0.09817477f, 0.07853982f, tileLocation.X * 64f < Game1.player.Position.X);
			}
			else if (damage > 0 && this.crop != null)
			{
				if (damage == 50)
				{
					this.crop.Kill();
				}
				else
				{
					this.destroyCrop(true);
				}
			}
			return false;
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x00130678 File Offset: 0x0012E878
		public bool canPlantThisSeedHere(string itemId, bool isFertilizer = false)
		{
			if (isFertilizer)
			{
				return this.CanApplyFertilizer(itemId);
			}
			if (this.crop == null)
			{
				Season season = this.Location.GetSeason();
				itemId = Crop.ResolveSeedId(itemId, this.Location);
				CropData cropData;
				if (Crop.TryGetData(itemId, out cropData))
				{
					if (cropData.Seasons.Count == 0)
					{
						return false;
					}
					if (!Game1.currentLocation.IsOutdoors || Game1.currentLocation.SeedsIgnoreSeasonsHere() || cropData.Seasons.Contains(season))
					{
						return !cropData.IsRaised || !Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), (int)this.Tile.X, (int)this.Tile.Y);
					}
					if (itemId == "309" || itemId == "310" || itemId == "311")
					{
						return true;
					}
					if (Game1.didPlayerJustClickAtAll(false) && !Game1.doesHUDMessageExist(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924")))
					{
						Game1.playSound("cancel", null);
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"), true);
					}
				}
			}
			return false;
		}

		// Token: 0x060019D2 RID: 6610 RVA: 0x0013079D File Offset: 0x0012E99D
		public override void performPlayerEntryAction()
		{
			base.performPlayerEntryAction();
			Crop crop = this.crop;
			if (crop == null)
			{
				return;
			}
			crop.updateDrawMath(this.Tile);
		}

		// Token: 0x060019D3 RID: 6611 RVA: 0x001307BC File Offset: 0x0012E9BC
		public override bool tickUpdate(GameTime time)
		{
			foreach (Action<GameLocation, Vector2> action in this.queuedActions)
			{
				action(this.Location, this.Tile);
			}
			this.queuedActions.Clear();
			if (this.maxShake > 0f)
			{
				if (this.shakeLeft)
				{
					this.shakeRotation -= this.shakeRate;
					if (Math.Abs(this.shakeRotation) >= this.maxShake)
					{
						this.shakeLeft = false;
					}
				}
				else
				{
					this.shakeRotation += this.shakeRate;
					if (this.shakeRotation >= this.maxShake)
					{
						this.shakeLeft = true;
						this.shakeRotation -= this.shakeRate;
					}
				}
				this.maxShake = Math.Max(0f, this.maxShake - 0.010471975f);
			}
			else
			{
				this.shakeRotation /= 2f;
				if (this.shakeRotation <= 0.01f)
				{
					base.NeedsUpdate = false;
					this.shakeRotation = 0f;
				}
			}
			return this.state.Value == 2 && this.crop == null;
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x00130910 File Offset: 0x0012EB10
		public bool hasPaddyCrop()
		{
			return this.crop != null && this.crop.isPaddyCrop();
		}

		// Token: 0x060019D5 RID: 6613 RVA: 0x00130928 File Offset: 0x0012EB28
		public bool paddyWaterCheck(bool forceUpdate = false)
		{
			if (!forceUpdate && this.nearWaterForPaddy.Value >= 0)
			{
				return this.nearWaterForPaddy.Value == 1;
			}
			if (!this.hasPaddyCrop())
			{
				this.nearWaterForPaddy.Value = 0;
				return false;
			}
			Vector2 tile_location = this.Tile;
			if (this.Location.getObjectAtTile((int)tile_location.X, (int)tile_location.Y, false) is IndoorPot)
			{
				this.nearWaterForPaddy.Value = 0;
				return false;
			}
			int range = 3;
			for (int x_offset = -range; x_offset <= range; x_offset++)
			{
				for (int y_offset = -range; y_offset <= range; y_offset++)
				{
					if (this.Location.isWaterTile((int)(tile_location.X + (float)x_offset), (int)(tile_location.Y + (float)y_offset)))
					{
						this.nearWaterForPaddy.Value = 1;
						return true;
					}
				}
			}
			this.nearWaterForPaddy.Value = 0;
			return false;
		}

		// Token: 0x060019D6 RID: 6614 RVA: 0x001309FC File Offset: 0x0012EBFC
		public override void dayUpdate()
		{
			GameLocation environment = this.Location;
			bool flag = this.hasPaddyCrop() && this.paddyWaterCheck(true);
			if (flag && this.state.Value == 0)
			{
				this.state.Value = 1;
			}
			if (this.crop != null)
			{
				this.crop.newDay(this.state.Value);
				if (environment.isOutdoors.Value && environment.GetSeason() == Season.Winter && this.crop != null && !this.crop.isWildSeedCrop() && !this.crop.IsInSeason(environment))
				{
					this.destroyCrop(false);
				}
			}
			if (!flag && !Game1.random.NextBool(this.GetFertilizerWaterRetentionChance()))
			{
				this.state.Value = 0;
			}
			if (environment.IsGreenhouse)
			{
				this.c.Value = Color.White;
			}
		}

		// Token: 0x060019D7 RID: 6615 RVA: 0x00130AD4 File Offset: 0x0012ECD4
		public override bool seasonUpdate(bool onLoad)
		{
			GameLocation location = this.Location;
			if (!onLoad && !location.SeedsIgnoreSeasonsHere() && (this.crop == null || this.crop.dead.Value || !this.crop.IsInSeason(location)))
			{
				this.fertilizer.Value = null;
			}
			if (location.GetSeason() == Season.Fall && !location.IsGreenhouse)
			{
				this.c.Value = new Color(250, 210, 240);
			}
			else
			{
				this.c.Value = Color.White;
			}
			this.texture = null;
			return false;
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x00130B74 File Offset: 0x0012ED74
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			byte drawSum = 0;
			Vector2 surroundingLocations = tileLocation;
			surroundingLocations.X += 1f;
			Farm farm = Game1.getFarm();
			TerrainFeature rightFeature;
			if (farm.terrainFeatures.TryGetValue(surroundingLocations, out rightFeature) && rightFeature is HoeDirt)
			{
				drawSum += 2;
			}
			surroundingLocations.X -= 2f;
			TerrainFeature leftFeature;
			if (farm.terrainFeatures.TryGetValue(surroundingLocations, out leftFeature) && leftFeature is HoeDirt)
			{
				drawSum += 8;
			}
			surroundingLocations.X += 1f;
			surroundingLocations.Y += 1f;
			TerrainFeature downFeature;
			if (Game1.currentLocation.terrainFeatures.TryGetValue(surroundingLocations, out downFeature) && downFeature is HoeDirt)
			{
				drawSum += 4;
			}
			surroundingLocations.Y -= 2f;
			TerrainFeature upFeature;
			if (farm.terrainFeatures.TryGetValue(surroundingLocations, out upFeature) && upFeature is HoeDirt)
			{
				drawSum += 1;
			}
			int sourceRectPosition = HoeDirt.drawGuide[drawSum];
			spriteBatch.Draw(HoeDirt.lightTexture, positionOnScreen, new Rectangle?(new Rectangle(sourceRectPosition % 4 * 64, sourceRectPosition / 4 * 64, 64, 64)), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + positionOnScreen.Y / 20000f);
			Crop crop = this.crop;
			if (crop == null)
			{
				return;
			}
			crop.drawInMenu(spriteBatch, positionOnScreen + new Vector2(64f * scale, 64f * scale), Color.White, 0f, scale, layerDepth + (positionOnScreen.Y + 64f * scale) / 20000f);
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x00130CF9 File Offset: 0x0012EEF9
		public override void draw(SpriteBatch spriteBatch)
		{
			this.DrawOptimized(spriteBatch, spriteBatch, spriteBatch);
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x00130D04 File Offset: 0x0012EF04
		public void DrawOptimized(SpriteBatch dirt_batch, SpriteBatch fert_batch, SpriteBatch crop_batch)
		{
			int state = this.state.Value;
			Vector2 tileLocation = this.Tile;
			if (state != 2 && (dirt_batch != null || fert_batch != null))
			{
				if (dirt_batch != null && this.texture == null)
				{
					Texture2D texture2D;
					if (!Game1.currentLocation.Name.Equals("Mountain") && !Game1.currentLocation.Name.Equals("Mine"))
					{
						MineShaft mine = Game1.currentLocation as MineShaft;
						if ((mine == null || !mine.shouldShowDarkHoeDirt()) && !(Game1.currentLocation is VolcanoDungeon))
						{
							texture2D = HoeDirt.lightTexture;
							goto IL_8F;
						}
					}
					texture2D = HoeDirt.darkTexture;
					IL_8F:
					this.texture = texture2D;
					if (Game1.currentLocation.GetSeason() != Season.Winter || Game1.currentLocation.SeedsIgnoreSeasonsHere() || Game1.currentLocation is MineShaft)
					{
						MineShaft shaft = Game1.currentLocation as MineShaft;
						if (shaft == null || !shaft.shouldUseSnowTextureHoeDirt())
						{
							goto IL_DD;
						}
					}
					this.texture = HoeDirt.snowTexture;
				}
				IL_DD:
				Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f);
				if (dirt_batch != null)
				{
					dirt_batch.Draw(this.texture, drawPos, new Rectangle?(new Rectangle(this.sourceRectPosition % 4 * 16, this.sourceRectPosition / 4 * 16, 16, 16)), this.c.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
					if (state == 1)
					{
						dirt_batch.Draw(this.texture, drawPos, new Rectangle?(new Rectangle(this.wateredRectPosition % 4 * 16 + (this.paddyWaterCheck(false) ? 128 : 64), this.wateredRectPosition / 4 * 16, 16, 16)), this.c.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.2E-08f);
					}
				}
				if (fert_batch != null && this.HasFertilizer())
				{
					fert_batch.Draw(Game1.mouseCursors, drawPos, new Rectangle?(this.GetFertilizerSourceRect()), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
				}
			}
			if (this.crop != null && crop_batch != null)
			{
				this.crop.draw(crop_batch, tileLocation, (state == 1 && this.crop.currentPhase.Value == 0 && this.crop.shouldDrawDarkWhenWatered()) ? (new Color(180, 100, 200) * 1f) : Color.White, this.shakeRotation);
			}
		}

		// Token: 0x060019DB RID: 6619 RVA: 0x00130F62 File Offset: 0x0012F162
		public virtual bool HasFertilizer()
		{
			return this.fertilizer.Value != null && this.fertilizer.Value != "0";
		}

		// Token: 0x060019DC RID: 6620 RVA: 0x00130F88 File Offset: 0x0012F188
		public virtual bool CanApplyFertilizer(string fertilizerId)
		{
			return this.CheckApplyFertilizerRules(fertilizerId) == HoeDirtFertilizerApplyStatus.Okay;
		}

		// Token: 0x060019DD RID: 6621 RVA: 0x00130F94 File Offset: 0x0012F194
		public virtual HoeDirtFertilizerApplyStatus CheckApplyFertilizerRules(string fertilizerId)
		{
			if (this.HasFertilizer())
			{
				fertilizerId = ItemRegistry.QualifyItemId(fertilizerId);
				if (!(fertilizerId == ItemRegistry.QualifyItemId(this.fertilizer.Value)))
				{
					return HoeDirtFertilizerApplyStatus.HasAnotherFertilizer;
				}
				return HoeDirtFertilizerApplyStatus.HasThisFertilizer;
			}
			else
			{
				if (this.crop != null && this.crop.currentPhase.Value != 0 && (fertilizerId == "(O)368" || fertilizerId == "(O)369"))
				{
					return HoeDirtFertilizerApplyStatus.CropAlreadySprouted;
				}
				return HoeDirtFertilizerApplyStatus.Okay;
			}
		}

		// Token: 0x060019DE RID: 6622 RVA: 0x00131004 File Offset: 0x0012F204
		public virtual float GetFertilizerSpeedBoost()
		{
			string value = this.fertilizer.Value;
			if (value == "465" || value == "(O)465")
			{
				return 0.1f;
			}
			if (value == "466" || value == "(O)466")
			{
				return 0.25f;
			}
			if (!(value == "918") && !(value == "(O)918"))
			{
				return 0f;
			}
			return 0.33f;
		}

		// Token: 0x060019DF RID: 6623 RVA: 0x00131084 File Offset: 0x0012F284
		public virtual float GetFertilizerWaterRetentionChance()
		{
			string value = this.fertilizer.Value;
			if (value == "370" || value == "(O)370")
			{
				return 0.33f;
			}
			if (value == "371" || value == "(O)371")
			{
				return 0.66f;
			}
			if (!(value == "920") && !(value == "(O)920"))
			{
				return 0f;
			}
			return 1f;
		}

		// Token: 0x060019E0 RID: 6624 RVA: 0x00131104 File Offset: 0x0012F304
		public virtual int GetFertilizerQualityBoostLevel()
		{
			string value = this.fertilizer.Value;
			if (value == "368" || value == "(O)368")
			{
				return 1;
			}
			if (value == "369" || value == "(O)369")
			{
				return 2;
			}
			if (!(value == "919") && !(value == "(O)919"))
			{
				return 0;
			}
			return 3;
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x00131174 File Offset: 0x0012F374
		public virtual Rectangle GetFertilizerSourceRect()
		{
			string value = this.fertilizer.Value;
			int fertilizerIndex;
			if (value != null)
			{
				int length = value.Length;
				if (length != 3)
				{
					if (length != 6)
					{
						goto IL_1D4;
					}
					switch (value[5])
					{
					case '0':
						if (value == "(O)370")
						{
							goto IL_1B8;
						}
						if (!(value == "(O)920"))
						{
							goto IL_1D4;
						}
						goto IL_1C0;
					case '1':
						if (!(value == "(O)371"))
						{
							goto IL_1D4;
						}
						goto IL_1BC;
					case '2':
					case '3':
					case '4':
					case '7':
						goto IL_1D4;
					case '5':
						if (!(value == "(O)465"))
						{
							goto IL_1D4;
						}
						goto IL_1C4;
					case '6':
						if (!(value == "(O)466"))
						{
							goto IL_1D4;
						}
						goto IL_1C8;
					case '8':
						if (!(value == "(O)918"))
						{
							goto IL_1D4;
						}
						goto IL_1CC;
					case '9':
						if (!(value == "(O)369"))
						{
							if (!(value == "(O)919"))
							{
								goto IL_1D4;
							}
							goto IL_1D0;
						}
						break;
					default:
						goto IL_1D4;
					}
				}
				else
				{
					switch (value[2])
					{
					case '0':
						if (value == "370")
						{
							goto IL_1B8;
						}
						if (!(value == "920"))
						{
							goto IL_1D4;
						}
						goto IL_1C0;
					case '1':
						if (!(value == "371"))
						{
							goto IL_1D4;
						}
						goto IL_1BC;
					case '2':
					case '3':
					case '4':
					case '7':
						goto IL_1D4;
					case '5':
						if (!(value == "465"))
						{
							goto IL_1D4;
						}
						goto IL_1C4;
					case '6':
						if (!(value == "466"))
						{
							goto IL_1D4;
						}
						goto IL_1C8;
					case '8':
						if (!(value == "918"))
						{
							goto IL_1D4;
						}
						goto IL_1CC;
					case '9':
						if (!(value == "369"))
						{
							if (!(value == "919"))
							{
								goto IL_1D4;
							}
							goto IL_1D0;
						}
						break;
					default:
						goto IL_1D4;
					}
				}
				fertilizerIndex = 1;
				goto IL_1D6;
				IL_1B8:
				fertilizerIndex = 3;
				goto IL_1D6;
				IL_1BC:
				fertilizerIndex = 4;
				goto IL_1D6;
				IL_1C0:
				fertilizerIndex = 5;
				goto IL_1D6;
				IL_1C4:
				fertilizerIndex = 6;
				goto IL_1D6;
				IL_1C8:
				fertilizerIndex = 7;
				goto IL_1D6;
				IL_1CC:
				fertilizerIndex = 8;
				goto IL_1D6;
				IL_1D0:
				fertilizerIndex = 2;
				goto IL_1D6;
			}
			IL_1D4:
			fertilizerIndex = 0;
			IL_1D6:
			return new Rectangle(173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16);
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x00131378 File Offset: 0x0012F578
		private List<HoeDirt.Neighbor> gatherNeighbors()
		{
			List<HoeDirt.Neighbor> results = this._neighbors;
			results.Clear();
			if (this.Pot == null)
			{
				GameLocation location = this.Location;
				Vector2 tilePos = this.Tile;
				NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = location.terrainFeatures;
				foreach (HoeDirt.NeighborLoc item in HoeDirt._offsets)
				{
					Vector2 tile = tilePos + item.Offset;
					TerrainFeature feature;
					if (terrainFeatures.TryGetValue(tile, out feature))
					{
						HoeDirt dirt = feature as HoeDirt;
						if (dirt != null && dirt.state.Value != 2)
						{
							HoeDirt.Neighbor i = new HoeDirt.Neighbor(dirt, item.Direction, item.InvDirection);
							results.Add(i);
						}
					}
				}
			}
			return results;
		}

		// Token: 0x060019E3 RID: 6627 RVA: 0x0013142C File Offset: 0x0012F62C
		public void updateNeighbors()
		{
			if (this.Location == null)
			{
				return;
			}
			List<HoeDirt.Neighbor> list = this.gatherNeighbors();
			this.neighborMask = 0;
			this.wateredNeighborMask = 0;
			foreach (HoeDirt.Neighbor i in list)
			{
				this.neighborMask |= i.direction;
				if (this.state.Value != 2)
				{
					i.feature.OnNeighborAdded(i.invDirection, this.state.Value);
				}
				if (this.isWatered() && i.feature.isWatered())
				{
					if (i.feature.paddyWaterCheck(false) == this.paddyWaterCheck(false))
					{
						this.wateredNeighborMask |= i.direction;
						HoeDirt feature = i.feature;
						feature.wateredNeighborMask |= i.invDirection;
					}
					else
					{
						i.feature.wateredNeighborMask = (i.feature.wateredNeighborMask & ~i.invDirection);
					}
				}
				i.feature.UpdateDrawSums();
			}
			this.UpdateDrawSums();
		}

		// Token: 0x060019E4 RID: 6628 RVA: 0x0013155C File Offset: 0x0012F75C
		public void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			this.Location = loc;
			this.Tile = tilePos;
			this.updateNeighbors();
		}

		// Token: 0x060019E5 RID: 6629 RVA: 0x00131574 File Offset: 0x0012F774
		public void OnRemoved()
		{
			if (this.Location == null)
			{
				return;
			}
			List<HoeDirt.Neighbor> list = this.gatherNeighbors();
			this.neighborMask = 0;
			this.wateredNeighborMask = 0;
			foreach (HoeDirt.Neighbor i in list)
			{
				i.feature.OnNeighborRemoved(i.invDirection);
				if (this.isWatered())
				{
					i.feature.wateredNeighborMask = (i.feature.wateredNeighborMask & ~i.invDirection);
				}
				i.feature.UpdateDrawSums();
			}
			this.UpdateDrawSums();
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x00131620 File Offset: 0x0012F820
		public virtual void UpdateDrawSums()
		{
			this.drawSum = (this.neighborMask & 15);
			this.sourceRectPosition = HoeDirt.drawGuide[this.drawSum];
			this.wateredRectPosition = HoeDirt.drawGuide[this.wateredNeighborMask];
		}

		// Token: 0x060019E7 RID: 6631 RVA: 0x0013165E File Offset: 0x0012F85E
		public void OnNeighborAdded(byte direction, int neighborState)
		{
			this.neighborMask |= direction;
			if (neighborState == 1)
			{
				this.wateredNeighborMask |= direction;
				return;
			}
			this.wateredNeighborMask &= ~direction;
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x00131693 File Offset: 0x0012F893
		public void OnNeighborRemoved(byte direction)
		{
			this.neighborMask &= ~direction;
			this.wateredNeighborMask &= ~direction;
		}

		// Token: 0x04000F9D RID: 3997
		public const float defaultShakeRate = 0.03926991f;

		// Token: 0x04000F9E RID: 3998
		public const float maximumShake = 0.3926991f;

		// Token: 0x04000F9F RID: 3999
		public const float shakeDecayRate = 0.010471975f;

		// Token: 0x04000FA0 RID: 4000
		public const byte N = 1;

		// Token: 0x04000FA1 RID: 4001
		public const byte E = 2;

		// Token: 0x04000FA2 RID: 4002
		public const byte S = 4;

		// Token: 0x04000FA3 RID: 4003
		public const byte W = 8;

		// Token: 0x04000FA4 RID: 4004
		public const byte Cardinals = 15;

		// Token: 0x04000FA5 RID: 4005
		public static readonly Vector2 N_Offset = new Vector2(0f, -1f);

		// Token: 0x04000FA6 RID: 4006
		public static readonly Vector2 E_Offset = new Vector2(1f, 0f);

		// Token: 0x04000FA7 RID: 4007
		public static readonly Vector2 S_Offset = new Vector2(0f, 1f);

		// Token: 0x04000FA8 RID: 4008
		public static readonly Vector2 W_Offset = new Vector2(-1f, 0f);

		// Token: 0x04000FA9 RID: 4009
		public const float paddyGrowBonus = 0.25f;

		// Token: 0x04000FAA RID: 4010
		public const int dry = 0;

		// Token: 0x04000FAB RID: 4011
		public const int watered = 1;

		// Token: 0x04000FAC RID: 4012
		public const int invisible = 2;

		// Token: 0x04000FAD RID: 4013
		public const string fertilizerLowQualityID = "368";

		// Token: 0x04000FAE RID: 4014
		public const string fertilizerHighQualityID = "369";

		// Token: 0x04000FAF RID: 4015
		public const string waterRetentionSoilID = "370";

		// Token: 0x04000FB0 RID: 4016
		public const string waterRetentionSoilQualityID = "371";

		// Token: 0x04000FB1 RID: 4017
		public const string speedGroID = "465";

		// Token: 0x04000FB2 RID: 4018
		public const string superSpeedGroID = "466";

		// Token: 0x04000FB3 RID: 4019
		public const string hyperSpeedGroID = "918";

		// Token: 0x04000FB4 RID: 4020
		public const string fertilizerDeluxeQualityID = "919";

		// Token: 0x04000FB5 RID: 4021
		public const string waterRetentionSoilDeluxeID = "920";

		// Token: 0x04000FB6 RID: 4022
		public const string fertilizerLowQualityQID = "(O)368";

		// Token: 0x04000FB7 RID: 4023
		public const string fertilizerHighQualityQID = "(O)369";

		// Token: 0x04000FB8 RID: 4024
		public const string waterRetentionSoilQID = "(O)370";

		// Token: 0x04000FB9 RID: 4025
		public const string waterRetentionSoilQualityQID = "(O)371";

		// Token: 0x04000FBA RID: 4026
		public const string speedGroQID = "(O)465";

		// Token: 0x04000FBB RID: 4027
		public const string superSpeedGroQID = "(O)466";

		// Token: 0x04000FBC RID: 4028
		public const string hyperSpeedGroQID = "(O)918";

		// Token: 0x04000FBD RID: 4029
		public const string fertilizerDeluxeQualityQID = "(O)919";

		// Token: 0x04000FBE RID: 4030
		public const string waterRetentionSoilDeluxeQID = "(O)920";

		// Token: 0x04000FBF RID: 4031
		public static Texture2D lightTexture;

		// Token: 0x04000FC0 RID: 4032
		public static Texture2D darkTexture;

		// Token: 0x04000FC1 RID: 4033
		public static Texture2D snowTexture;

		// Token: 0x04000FC2 RID: 4034
		private readonly NetRef<Crop> netCrop = new NetRef<Crop>();

		// Token: 0x04000FC3 RID: 4035
		public static Dictionary<byte, int> drawGuide;

		// Token: 0x04000FC4 RID: 4036
		[XmlElement("state")]
		public readonly NetInt state = new NetInt();

		// Token: 0x04000FC5 RID: 4037
		[XmlElement("fertilizer")]
		public readonly NetString fertilizer = new NetString();

		// Token: 0x04000FC6 RID: 4038
		private bool shakeLeft;

		// Token: 0x04000FC7 RID: 4039
		private float shakeRotation;

		// Token: 0x04000FC8 RID: 4040
		private float maxShake;

		// Token: 0x04000FC9 RID: 4041
		private float shakeRate;

		// Token: 0x04000FCA RID: 4042
		[XmlElement("c")]
		private readonly NetColor c = new NetColor(Color.White);

		// Token: 0x04000FCB RID: 4043
		private List<Action<GameLocation, Vector2>> queuedActions = new List<Action<GameLocation, Vector2>>();

		// Token: 0x04000FCC RID: 4044
		private byte neighborMask;

		// Token: 0x04000FCD RID: 4045
		private byte wateredNeighborMask;

		// Token: 0x04000FCE RID: 4046
		[XmlIgnore]
		public NetInt nearWaterForPaddy = new NetInt(-1);

		// Token: 0x04000FCF RID: 4047
		private byte drawSum;

		// Token: 0x04000FD0 RID: 4048
		private int sourceRectPosition;

		// Token: 0x04000FD1 RID: 4049
		private int wateredRectPosition;

		// Token: 0x04000FD3 RID: 4051
		private Texture2D texture;

		// Token: 0x04000FD4 RID: 4052
		private static readonly HoeDirt.NeighborLoc[] _offsets = new HoeDirt.NeighborLoc[]
		{
			new HoeDirt.NeighborLoc(HoeDirt.N_Offset, 1, 4),
			new HoeDirt.NeighborLoc(HoeDirt.S_Offset, 4, 1),
			new HoeDirt.NeighborLoc(HoeDirt.E_Offset, 2, 8),
			new HoeDirt.NeighborLoc(HoeDirt.W_Offset, 8, 2)
		};

		// Token: 0x04000FD5 RID: 4053
		private List<HoeDirt.Neighbor> _neighbors = new List<HoeDirt.Neighbor>();

		// Token: 0x02000524 RID: 1316
		private struct NeighborLoc
		{
			// Token: 0x060040B2 RID: 16562 RVA: 0x00303B59 File Offset: 0x00301D59
			public NeighborLoc(Vector2 a, byte b, byte c)
			{
				this.Offset = a;
				this.Direction = b;
				this.InvDirection = c;
			}

			// Token: 0x04002AA7 RID: 10919
			public readonly Vector2 Offset;

			// Token: 0x04002AA8 RID: 10920
			public readonly byte Direction;

			// Token: 0x04002AA9 RID: 10921
			public readonly byte InvDirection;
		}

		// Token: 0x02000525 RID: 1317
		private struct Neighbor
		{
			// Token: 0x060040B3 RID: 16563 RVA: 0x00303B70 File Offset: 0x00301D70
			public Neighbor(HoeDirt a, byte b, byte c)
			{
				this.feature = a;
				this.direction = b;
				this.invDirection = c;
			}

			// Token: 0x04002AAA RID: 10922
			public readonly HoeDirt feature;

			// Token: 0x04002AAB RID: 10923
			public readonly byte direction;

			// Token: 0x04002AAC RID: 10924
			public readonly byte invDirection;
		}
	}
}
