using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x0200013E RID: 318
	public class Bush : LargeTerrainFeature
	{
		// Token: 0x06001929 RID: 6441 RVA: 0x00127520 File Offset: 0x00125720
		public Bush() : base(true)
		{
		}

		// Token: 0x0600192A RID: 6442 RVA: 0x00127598 File Offset: 0x00125798
		public Bush(Vector2 tileLocation, int size, GameLocation location, int datePlantedOverride = -1) : this()
		{
			this.Tile = tileLocation;
			this.size.Value = size;
			this.Location = location;
			this.townBush.Value = (location is Town && (size == 0 || size == 1 || size == 2) && tileLocation.X % 5f != 0f);
			if (location.map.RequireLayer("Front").Tiles[(int)tileLocation.X, (int)tileLocation.Y] != null)
			{
				this.drawShadow.Value = false;
			}
			this.datePlanted.Value = (int)((datePlantedOverride == -1) ? Game1.stats.DaysPlayed : ((uint)datePlantedOverride));
			if (size != 3)
			{
				if (size == 4)
				{
					this.tileSheetOffset.Value = 1;
				}
			}
			else
			{
				this.drawShadow.Value = false;
			}
			GameLocation old_location = this.Location;
			this.Location = location;
			this.loadSprite();
			this.Location = old_location;
			this.flipped.Value = Game1.random.NextBool();
		}

		// Token: 0x0600192B RID: 6443 RVA: 0x001276A4 File Offset: 0x001258A4
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.size, "size").AddField(this.tileSheetOffset, "tileSheetOffset").AddField(this.flipped, "flipped").AddField(this.townBush, "townBush").AddField(this.drawShadow, "drawShadow").AddField(this.sourceRect, "sourceRect").AddField(this.datePlanted, "datePlanted").AddField(this.inPot, "inPot").AddField(this.uniqueSpawnMutex.NetFields, "uniqueSpawnMutex.NetFields");
		}

		// Token: 0x0600192C RID: 6444 RVA: 0x00127753 File Offset: 0x00125953
		public int getAge()
		{
			return (int)(Game1.stats.DaysPlayed - (uint)this.datePlanted.Value);
		}

		// Token: 0x0600192D RID: 6445 RVA: 0x0012776C File Offset: 0x0012596C
		public void setUpSourceRect()
		{
			Season season = this.GetCosmeticSeason();
			int seasonNumber = (int)season;
			switch (this.size.Value)
			{
			case 0:
				this.sourceRect.Value = new Rectangle(seasonNumber * 16 * 2 + this.tileSheetOffset.Value * 16, 224, 16, 32);
				return;
			case 1:
			{
				if (this.townBush.Value)
				{
					this.sourceRect.Value = new Rectangle(seasonNumber * 16 * 2, 96, 32, 32);
					return;
				}
				int xOffset = seasonNumber * 16 * 4 + this.tileSheetOffset.Value * 16 * 2;
				this.sourceRect.Value = new Rectangle(xOffset % Bush.texture.Value.Bounds.Width, xOffset / Bush.texture.Value.Bounds.Width * 3 * 16, 32, 48);
				return;
			}
			case 2:
				if (this.townBush.Value && (season == Season.Spring || season == Season.Summer))
				{
					this.sourceRect.Value = new Rectangle(48, 176, 48, 48);
					return;
				}
				switch (season)
				{
				case Season.Spring:
				case Season.Summer:
					this.sourceRect.Value = new Rectangle(0, 128, 48, 48);
					return;
				case Season.Fall:
					this.sourceRect.Value = new Rectangle(48, 128, 48, 48);
					return;
				case Season.Winter:
					this.sourceRect.Value = new Rectangle(0, 176, 48, 48);
					return;
				default:
					return;
				}
				break;
			case 3:
			{
				int age = this.getAge();
				switch (season)
				{
				case Season.Spring:
					this.sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 256, 16, 32);
					return;
				case Season.Summer:
					this.sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 256, 16, 32);
					return;
				case Season.Fall:
					this.sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 288, 16, 32);
					return;
				case Season.Winter:
					this.sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 288, 16, 32);
					return;
				default:
					return;
				}
				break;
			}
			case 4:
				this.sourceRect.Value = new Rectangle(this.tileSheetOffset.Value * 32, 320, 32, 32);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600192E RID: 6446 RVA: 0x00127A1F File Offset: 0x00125C1F
		public bool readyForHarvest()
		{
			return this.tileSheetOffset.Value == 1;
		}

		// Token: 0x0600192F RID: 6447 RVA: 0x00127A2F File Offset: 0x00125C2F
		public virtual Season GetCosmeticSeason()
		{
			if (this.size.Value == 1)
			{
				return this.Location.GetSeason();
			}
			if (!this.IsSheltered())
			{
				return this.Location.GetSeason();
			}
			return Season.Spring;
		}

		// Token: 0x06001930 RID: 6448 RVA: 0x00127A60 File Offset: 0x00125C60
		public bool IsSheltered()
		{
			return (this.Location != null && this.Location.SeedsIgnoreSeasonsHere()) || (this.inPot.Value && !this.Location.IsOutdoors);
		}

		// Token: 0x06001931 RID: 6449 RVA: 0x00127A98 File Offset: 0x00125C98
		public bool inBloom()
		{
			if (this.size.Value == 4)
			{
				return this.readyForHarvest();
			}
			GameLocation location = this.Location;
			Season season = (location != null) ? location.GetSeason() : Game1.season;
			int dayOfMonth = Game1.dayOfMonth;
			if (this.size.Value == 3)
			{
				bool inBloom = this.getAge() >= 20 && dayOfMonth >= 22 && (season != Season.Winter || this.IsSheltered());
				if (inBloom && this.Location != null && this.Location.IsFarm)
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						farmer.autoGenerateActiveDialogueEvent("cropMatured_815", 4);
					}
				}
				return inBloom;
			}
			if (season != Season.Spring)
			{
				return season == Season.Fall && dayOfMonth > 7 && dayOfMonth < 12;
			}
			return dayOfMonth > 14 && dayOfMonth < 19;
		}

		// Token: 0x06001932 RID: 6450 RVA: 0x00127B88 File Offset: 0x00125D88
		public override bool isActionable()
		{
			return true;
		}

		// Token: 0x06001933 RID: 6451 RVA: 0x00127B8C File Offset: 0x00125D8C
		public override void loadSprite()
		{
			Vector2 tilePosition = this.Tile;
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, (double)tilePosition.X, (double)tilePosition.Y * 777.0, 0.0);
			double extra = (r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0);
			if (this.size.Value != 4)
			{
				if (this.size.Value == 1 && !this.readyForHarvest() && r.NextDouble() < 0.2 + extra && this.inBloom())
				{
					this.tileSheetOffset.Value = 1;
				}
				else if (Game1.GetSeasonForLocation(this.Location) != Season.Summer && !this.inBloom())
				{
					this.tileSheetOffset.Value = 0;
				}
			}
			if (this.size.Value == 3)
			{
				this.tileSheetOffset.Value = ((this.inBloom() > false) ? 1 : 0);
			}
			this.setUpSourceRect();
		}

		// Token: 0x06001934 RID: 6452 RVA: 0x00127CA0 File Offset: 0x00125EA0
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			switch (this.size.Value)
			{
			case 0:
			case 3:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			case 1:
			case 4:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 128, 64);
			case 2:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 192, 64);
			default:
				return Rectangle.Empty;
			}
		}

		// Token: 0x06001935 RID: 6453 RVA: 0x00127D44 File Offset: 0x00125F44
		public override Rectangle getRenderBounds()
		{
			Vector2 tileLocation = this.Tile;
			switch (this.size.Value)
			{
			case 0:
			case 3:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 1f) * 64, 64, 160);
			case 1:
			case 4:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 128, 256);
			case 2:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 192, 256);
			default:
				return Rectangle.Empty;
			}
		}

		// Token: 0x06001936 RID: 6454 RVA: 0x00127E00 File Offset: 0x00126000
		public override bool performUseAction(Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			base.NeedsUpdate = true;
			if (Game1.didPlayerJustRightClick(true))
			{
				this.shakeTimer = 0f;
			}
			if (this.shakeTimer <= 0f)
			{
				Season season = location.GetSeason();
				if (this.maxShake == 0f && (this.size.Value != 3 || season != Season.Winter || this.IsSheltered()))
				{
					location.localSound("leafrustle", null, null, SoundContext.Default);
				}
				GameLocation old_location = this.Location;
				this.Location = location;
				this.shake(tileLocation, false);
				this.Location = old_location;
				this.shakeTimer = 500f;
			}
			return true;
		}

		// Token: 0x06001937 RID: 6455 RVA: 0x00127EB4 File Offset: 0x001260B4
		public override bool tickUpdate(GameTime time)
		{
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (this.size.Value == 4)
			{
				this.uniqueSpawnMutex.Update(this.Location);
			}
			if (this.maxShake > 0f)
			{
				if (this.shakeLeft)
				{
					this.shakeRotation -= 0.015707964f;
					if (this.shakeRotation <= -this.maxShake)
					{
						this.shakeLeft = false;
					}
				}
				else
				{
					this.shakeRotation += 0.015707964f;
					if (this.shakeRotation >= this.maxShake)
					{
						this.shakeLeft = true;
					}
				}
				this.maxShake = Math.Max(0f, this.maxShake - 0.0030679617f);
			}
			if (this.shakeTimer <= 0f && this.size.Value != 4 && this.maxShake <= 0f)
			{
				base.NeedsUpdate = false;
			}
			return false;
		}

		// Token: 0x06001938 RID: 6456 RVA: 0x00127FBC File Offset: 0x001261BC
		public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
		{
			if (this.maxShake == 0f || doEvenIfStillShaking)
			{
				this.shakeLeft = (Game1.player.Tile.X > tileLocation.X || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool()));
				this.maxShake = 0.024543693f;
				base.NeedsUpdate = true;
				if (!this.townBush.Value && this.readyForHarvest() && this.inBloom())
				{
					string shakeOff = this.GetShakeOffItem();
					if (shakeOff != null)
					{
						this.tileSheetOffset.Value = 0;
						this.setUpSourceRect();
						int value = this.size.Value;
						if (value != 3)
						{
							if (value == 4)
							{
								this.uniqueSpawnMutex.RequestLock(delegate
								{
									Game1.player.team.MarkCollectedNut(string.Concat(new string[]
									{
										"Bush_",
										this.Location.Name,
										"_",
										tileLocation.X.ToString(),
										"_",
										tileLocation.Y.ToString()
									}));
									Game1.createItemDebris(ItemRegistry.Create(shakeOff, 1, 0, false), new Vector2((float)this.getBoundingBox().Center.X, (float)(this.getBoundingBox().Bottom - 2)), 0, this.Location, this.getBoundingBox().Bottom, false);
								}, null);
							}
							else
							{
								int number = Utility.CreateRandom((double)tileLocation.X, (double)tileLocation.Y * 5000.0, Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0).Next(1, 2) + Game1.player.ForagingLevel / 4;
								for (int i = 0; i < number; i++)
								{
									Item item = ItemRegistry.Create(shakeOff, 1, 0, false);
									if (Game1.player.professions.Contains(16))
									{
										item.Quality = 4;
									}
									Game1.createItemDebris(item, Utility.PointToVector2(this.getBoundingBox().Center), Game1.random.Next(1, 4), null, -1, false);
								}
								Game1.player.gainExperience(2, number);
							}
						}
						else
						{
							Game1.createObjectDebris(shakeOff, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, null);
						}
						if (this.size.Value != 3)
						{
							DelayedAction.playSoundAfterDelay("leafrustle", 100, null, null, -1, false);
							return;
						}
					}
				}
				else
				{
					if (tileLocation.X == 20f && tileLocation.Y == 8f && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
					{
						Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(F)1733", 1, 0, false), new ItemGrabMenu.behaviorOnItemSelect(this.junimoPlushCallback), false);
						return;
					}
					Town town = Game1.currentLocation as Town;
					if (town != null)
					{
						if (tileLocation.X == 28f && tileLocation.Y == 14f && Game1.player.eventsSeen.Contains("520702") && !Game1.player.hasMagnifyingGlass)
						{
							town.initiateMagnifyingGlassGet();
							return;
						}
						if (tileLocation.X == 47f && tileLocation.Y == 100f && Game1.player.secretNotesSeen.Contains(21) && Game1.timeOfDay == 2440 && Game1.player.mailReceived.Add("secretNote21_done"))
						{
							town.initiateMarnieLewisBush();
						}
					}
				}
			}
		}

		// Token: 0x06001939 RID: 6457 RVA: 0x00128328 File Offset: 0x00126528
		public string GetShakeOffItem()
		{
			int value = this.size.Value;
			if (value == 3)
			{
				return "(O)815";
			}
			if (value == 4)
			{
				return "(O)73";
			}
			Season season = this.Location.GetSeason();
			if (season == Season.Spring)
			{
				return "(O)296";
			}
			if (season != Season.Fall)
			{
				return null;
			}
			return "(O)410";
		}

		// Token: 0x0600193A RID: 6458 RVA: 0x00128379 File Offset: 0x00126579
		public void junimoPlushCallback(Item item, Farmer who)
		{
			if (((item != null) ? item.QualifiedItemId : null) == "(F)1733" && who != null)
			{
				who.mailReceived.Add("junimoPlush");
			}
		}

		// Token: 0x0600193B RID: 6459 RVA: 0x001283A7 File Offset: 0x001265A7
		public override bool isPassable(Character c = null)
		{
			return c is JunimoHarvester;
		}

		// Token: 0x0600193C RID: 6460 RVA: 0x001283B4 File Offset: 0x001265B4
		public override void dayUpdate()
		{
			GameLocation environment = this.Location;
			base.NeedsUpdate = true;
			Season season = environment.GetSeason();
			if (this.size.Value == 4)
			{
				return;
			}
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, (double)this.Tile.X, (double)this.Tile.Y * 777.0, 0.0);
			double extra = (r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0);
			if (this.size.Value == 1 && !this.readyForHarvest() && r.NextDouble() < 0.2 + extra && this.inBloom())
			{
				this.tileSheetOffset.Value = 1;
			}
			else if (season != Season.Summer && !this.inBloom())
			{
				this.tileSheetOffset.Value = 0;
			}
			if (this.size.Value == 3)
			{
				this.tileSheetOffset.Value = ((this.inBloom() > false) ? 1 : 0);
			}
			this.setUpSourceRect();
			Vector2 tileLocation = this.Tile;
			if (tileLocation.X != 6f || tileLocation.Y != 7f || !(environment.Name == "Sunroom"))
			{
				this.health = 0f;
			}
		}

		// Token: 0x0600193D RID: 6461 RVA: 0x00128518 File Offset: 0x00126718
		public override bool seasonUpdate(bool onLoad)
		{
			if (this.size.Value == 4)
			{
				return false;
			}
			if (!Game1.IsMultiplayer || Game1.IsServer)
			{
				Season season = this.Location.GetSeason();
				this.tileSheetOffset.Value = ((this.size.Value == 1 && season == Season.Summer && Game1.random.NextBool()) ? 1 : 0);
				this.loadSprite();
			}
			return false;
		}

		// Token: 0x0600193E RID: 6462 RVA: 0x00128584 File Offset: 0x00126784
		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
		{
			GameLocation location = this.Location;
			base.NeedsUpdate = true;
			if (this.size.Value == 4)
			{
				return false;
			}
			if (explosion > 0)
			{
				this.shake(tileLocation, true);
				return false;
			}
			if (this.size.Value == 3)
			{
				MeleeWeapon weapon = t as MeleeWeapon;
				if (weapon != null && weapon.ItemId == "66")
				{
					this.shake(tileLocation, true);
					return false;
				}
			}
			Axe axe = t as Axe;
			if (axe != null && this.isDestroyable())
			{
				location.playSound("leafrustle", new Vector2?(tileLocation), null, SoundContext.Default);
				this.shake(tileLocation, true);
				if (axe.upgradeLevel.Value >= 1 || this.size.Value == 3)
				{
					this.health -= ((this.size.Value == 3) ? 0.5f : ((float)axe.upgradeLevel.Value / 5f));
					if (this.health <= -1f)
					{
						location.playSound("treethud", new Vector2?(tileLocation), null, SoundContext.Default);
						DelayedAction.playSoundAfterDelay("leafrustle", 100, location, new Vector2?(tileLocation), -1, false);
						Color c = Color.Green;
						Season season = location.GetSeason();
						if (!this.IsSheltered())
						{
							switch (season)
							{
							case Season.Spring:
								c = Color.Green;
								break;
							case Season.Summer:
								c = Color.ForestGreen;
								break;
							case Season.Fall:
								c = Color.IndianRed;
								break;
							case Season.Winter:
								c = Color.Cyan;
								break;
							}
						}
						if (location.Name == "Sunroom")
						{
							foreach (NPC npc in location.characters)
							{
								npc.jump();
								npc.doEmote(12, true);
							}
						}
						for (int i = 0; i <= this.getEffectiveSize(); i++)
						{
							for (int j = 0; j < 12; j++)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
								{
									new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200 + (season.Equals("fall") ? 16 : (season.Equals("winter") ? -16 : 0)), 16, 16), Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(0f, (float)Game1.random.Next(64)), false, 0.01f, c)
									{
										motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)(-(float)Game1.random.Next(5, 7))),
										acceleration = new Vector2(0f, (float)Game1.random.Next(13, 17) / 100f),
										accelerationChange = new Vector2(0f, -0.001f),
										scale = 4f,
										layerDepth = (tileLocation.Y + 1f) * 64f / 10000f,
										animationLength = 11,
										totalNumberOfLoops = 99,
										interval = (float)Game1.random.Next(20, 90),
										delayBeforeAnimationStart = (i + 1) * j * 20
									}
								});
								if (j % 6 == 0)
								{
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(50, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, (float)Game1.random.Next(32, 64)), c, 8, false, 100f, 0, -1, -1f, -1, 0)
									});
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite(12, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, (float)Game1.random.Next(32, 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
									});
								}
							}
						}
						if (this.size.Value == 3)
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)251", 1, 0, false), tileLocation * 64f, 2, location, -1, false);
						}
						return true;
					}
					location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
				}
			}
			return false;
		}

		// Token: 0x0600193F RID: 6463 RVA: 0x00128A14 File Offset: 0x00126C14
		public bool isDestroyable()
		{
			if (this.size.Value == 3)
			{
				return true;
			}
			if (this.Location is Farm)
			{
				Vector2 tile = this.Tile;
				switch (Game1.whichFarm)
				{
				case 1:
					return new Rectangle(32, 11, 11, 25).Contains((int)tile.X, (int)tile.Y);
				case 2:
					return (tile.X == 13f && tile.Y == 35f) || (tile.X == 37f && tile.Y == 9f) || new Rectangle(43, 11, 34, 50).Contains((int)tile.X, (int)tile.Y);
				case 3:
					return new Rectangle(24, 56, 10, 8).Contains((int)tile.X, (int)tile.Y);
				case 6:
					return new Rectangle(20, 44, 36, 44).Contains((int)tile.X, (int)tile.Y);
				}
			}
			return false;
		}

		// Token: 0x06001940 RID: 6464 RVA: 0x00128B3C File Offset: 0x00126D3C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			spriteBatch.Draw(Bush.texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle?(new Rectangle(32, 96, 16, 32)), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
		}

		// Token: 0x06001941 RID: 6465 RVA: 0x00128BD4 File Offset: 0x00126DD4
		public override void performPlayerEntryAction()
		{
			base.performPlayerEntryAction();
			Season season = this.Location.GetSeason();
			if (season != Season.Winter && !this.Location.IsRainingHere() && Game1.isDarkOut(this.Location) && Game1.random.NextBool((season == Season.Summer) ? 0.08 : 0.04))
			{
				AmbientLocationSounds.addSound(this.Tile, 3);
			}
			NetRectangle netRectangle = this.sourceRect;
			if (netRectangle != null && netRectangle.X < 0)
			{
				this.setUpSourceRect();
			}
		}

		// Token: 0x06001942 RID: 6466 RVA: 0x00128C60 File Offset: 0x00126E60
		private int getEffectiveSize()
		{
			int value = this.size.Value;
			if (value == 3)
			{
				return 0;
			}
			if (value != 4)
			{
				return this.size.Value;
			}
			return 1;
		}

		// Token: 0x06001943 RID: 6467 RVA: 0x00128C92 File Offset: 0x00126E92
		public void draw(SpriteBatch spriteBatch, float yDrawOffset)
		{
			this.yDrawOffset = yDrawOffset;
			this.draw(spriteBatch);
		}

		// Token: 0x06001944 RID: 6468 RVA: 0x00128CA4 File Offset: 0x00126EA4
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			if (this.drawShadow.Value)
			{
				if (this.getEffectiveSize() > 0)
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X + ((this.getEffectiveSize() == 1) ? 0.5f : 1f)) * 64f - 51f, tileLocation.Y * 64f - 16f + this.yDrawOffset)), new Rectangle?(Bush.shadowSourceRect), Color.White, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				else
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f - 4f + this.yDrawOffset)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-06f);
				}
			}
			spriteBatch.Draw(Bush.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + (float)((this.getEffectiveSize() + 1) * 64 / 2), (tileLocation.Y + 1f) * 64f - (float)((this.getEffectiveSize() > 0 && (!this.townBush.Value || this.getEffectiveSize() != 1) && this.size.Value != 4) ? 64 : 0) + this.yDrawOffset)), new Rectangle?(this.sourceRect.Value), Color.White, this.shakeRotation, new Vector2((float)((this.getEffectiveSize() + 1) * 16 / 2), 32f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.getBoundingBox().Center.Y + 48) / 10000f - tileLocation.X / 1000000f);
		}

		// Token: 0x04000F16 RID: 3862
		public const float shakeRate = 0.015707964f;

		// Token: 0x04000F17 RID: 3863
		public const float shakeDecayRate = 0.0030679617f;

		// Token: 0x04000F18 RID: 3864
		public const int smallBush = 0;

		// Token: 0x04000F19 RID: 3865
		public const int mediumBush = 1;

		// Token: 0x04000F1A RID: 3866
		public const int largeBush = 2;

		// Token: 0x04000F1B RID: 3867
		public const int greenTeaBush = 3;

		// Token: 0x04000F1C RID: 3868
		public const int walnutBush = 4;

		// Token: 0x04000F1D RID: 3869
		public const int daysToMatureGreenTeaBush = 20;

		// Token: 0x04000F1E RID: 3870
		[XmlElement("size")]
		public readonly NetInt size = new NetInt();

		// Token: 0x04000F1F RID: 3871
		[XmlElement("datePlanted")]
		public readonly NetInt datePlanted = new NetInt();

		// Token: 0x04000F20 RID: 3872
		[XmlElement("tileSheetOffset")]
		public readonly NetInt tileSheetOffset = new NetInt();

		// Token: 0x04000F21 RID: 3873
		public float health;

		// Token: 0x04000F22 RID: 3874
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		// Token: 0x04000F23 RID: 3875
		[XmlElement("townBush")]
		public readonly NetBool townBush = new NetBool();

		// Token: 0x04000F24 RID: 3876
		public readonly NetBool inPot = new NetBool();

		// Token: 0x04000F25 RID: 3877
		[XmlElement("drawShadow")]
		public readonly NetBool drawShadow = new NetBool(true);

		// Token: 0x04000F26 RID: 3878
		private bool shakeLeft;

		// Token: 0x04000F27 RID: 3879
		private float shakeRotation;

		// Token: 0x04000F28 RID: 3880
		private float maxShake;

		// Token: 0x04000F29 RID: 3881
		[XmlIgnore]
		public float shakeTimer;

		// Token: 0x04000F2A RID: 3882
		[XmlIgnore]
		public readonly NetRectangle sourceRect = new NetRectangle();

		// Token: 0x04000F2B RID: 3883
		[XmlIgnore]
		public NetMutex uniqueSpawnMutex = new NetMutex();

		// Token: 0x04000F2C RID: 3884
		public static Lazy<Texture2D> texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>("TileSheets\\bushes"));

		// Token: 0x04000F2D RID: 3885
		public static Rectangle shadowSourceRect = new Rectangle(663, 1011, 41, 30);

		// Token: 0x04000F2E RID: 3886
		private float yDrawOffset;
	}
}
