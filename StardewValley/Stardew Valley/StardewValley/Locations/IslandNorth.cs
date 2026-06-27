using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002DB RID: 731
	public class IslandNorth : IslandLocation
	{
		// Token: 0x06003020 RID: 12320 RVA: 0x00260200 File Offset: 0x0025E400
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.bridgeFixed, "bridgeFixed").AddField(this.traderActivated, "traderActivated").AddField(this.caveOpened, "caveOpened").AddField(this.treeNutShot, "treeNutShot");
			this.bridgeFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyFixedBridge();
				}
			};
			this.traderActivated.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (!Utility.ShouldIgnoreValueChangeCallback())
				{
					this.ApplyIslandTraderHut();
				}
			};
			this.caveOpened.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (!Utility.ShouldIgnoreValueChangeCallback())
				{
					this.ApplyCaveOpened();
				}
			};
		}

		// Token: 0x06003021 RID: 12321 RVA: 0x002602A0 File Offset: 0x0025E4A0
		public override void SetBuriedNutLocations()
		{
			this.buriedNutPoints.Add(new Point(57, 79));
			this.buriedNutPoints.Add(new Point(19, 39));
			this.buriedNutPoints.Add(new Point(19, 13));
			this.buriedNutPoints.Add(new Point(54, 21));
			this.buriedNutPoints.Add(new Point(42, 77));
			this.buriedNutPoints.Add(new Point(62, 54));
			this.buriedNutPoints.Add(new Point(26, 81));
			base.SetBuriedNutLocations();
		}

		// Token: 0x06003022 RID: 12322 RVA: 0x00260340 File Offset: 0x0025E540
		public virtual void ApplyFixedBridge()
		{
			if (this.map != null)
			{
				base.ApplyMapOverride("Island_Bridge_Repaired", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 3)));
			}
		}

		// Token: 0x06003023 RID: 12323 RVA: 0x0026037C File Offset: 0x0025E57C
		public virtual void ApplyIslandTraderHut()
		{
			if (this.map != null)
			{
				base.ApplyMapOverride("Island_N_Trader", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(32, 64, 9, 10)));
				base.removeTemporarySpritesWithIDLocal(8989);
				base.removeTemporarySpritesWithIDLocal(8988);
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(33.45f, 70.33f) * 64f + new Vector2(-16f, -32f), false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandNorth_Trader_1",
					id = 8989,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(39.45f, 70.33f) * 64f + new Vector2(-16f, -32f), false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandNorth_Trader_2",
					id = 8988,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
			}
		}

		// Token: 0x06003024 RID: 12324 RVA: 0x00260548 File Offset: 0x0025E748
		public virtual void ApplyCaveOpened()
		{
			if (Game1.player.currentLocation != null && Game1.player.currentLocation.Equals(this))
			{
				for (int i = 0; i < 12; i++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(146, 229 + Game1.random.Next(3) * 9, 9, 9), Utility.getRandomPositionInThisRectangle(this.boulderPosition, Game1.random), Game1.random.NextBool(), 0f, Color.White)
					{
						scale = 4f,
						motion = new Vector2((float)Game1.random.Next(-3, 1), (float)Game1.random.Next(-15, -9)),
						acceleration = new Vector2(0f, 0.4f),
						rotationChange = (float)Game1.random.Next(-2, 3) * 0.01f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = this.boulderPosition.Bottom + 1 + Game1.random.Next(64),
						delayBeforeAnimationStart = i * 15
					});
					this.temporarySprites[this.temporarySprites.Count - 1].initialPosition.Y = (float)this.temporarySprites[this.temporarySprites.Count - 1].yStopCoordinate;
					this.temporarySprites[this.temporarySprites.Count - 1].reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.temporarySprites[this.temporarySprites.Count - 1].bounce);
				}
				for (int j = 0; j < 8; j++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), Utility.getRandomPositionInThisRectangle(this.boulderPosition, Game1.random) + new Vector2(-32f, -32f), false, 0.007f, Color.White)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -1f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 4f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
						delayBeforeAnimationStart = j * 40
					});
				}
				Game1.playSound("boulderBreak", null);
				Game1.player.freezePause = 3000;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.globalFadeToBlack(delegate
					{
						this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandNorth_Event_SafariManAppear"), null));
					}, 0.02f);
				}, 1000);
			}
		}

		// Token: 0x06003025 RID: 12325 RVA: 0x00260828 File Offset: 0x0025EA28
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 27 && yLocation == 28 && who.secretNotesSeen.Contains(1010))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_N_BuriedTreasureNut", this, xLocation * 64, yLocation * 64, 1, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_N_BuriedTreasure"))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)289", 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, 1, null, -1, false);
					Game1.addMailForTomorrow("Island_N_BuriedTreasure", true, false);
				}
			}
			if (xLocation == 26 && yLocation == 81 && !Game1.player.team.collectedNutTracker.Contains("Buried_IslandNorth_26_81"))
			{
				DelayedAction.functionAfterDelay(delegate
				{
					TemporaryAnimatedSprite t = base.getTemporarySpriteByID(79797);
					if (t != null)
					{
						TemporaryAnimatedSprite t2 = t;
						t2.sourceRectStartingPos.X = t2.sourceRectStartingPos.X + 40f;
						t.sourceRect.X = 181;
						t.interval = 100f;
						t.shakeIntensity = 1f;
						base.playSound("monkey1", null, null, SoundContext.Default);
						t.motion = new Vector2(-3f, -10f);
						t.acceleration = new Vector2(0f, 0.3f);
						t.yStopCoordinate = (int)t.position.Y + 1;
						t.reachedStopCoordinate = delegate(int x)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite(50, t.position, Color.Green, 8, false, 100f, 0, -1, -1f, -1, 0)
							{
								drawAboveAlwaysFront = true
							});
							this.removeTemporarySpritesWithID(79797);
							this.playSound("leafrustle", null, null, SoundContext.Default);
						};
					}
				}, 700);
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x06003026 RID: 12326 RVA: 0x00260904 File Offset: 0x0025EB04
		public IslandNorth()
		{
		}

		// Token: 0x06003027 RID: 12327 RVA: 0x00260988 File Offset: 0x0025EB88
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			if (!projectile || damagesFarmer != 0 || position.Bottom >= 832)
			{
				return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement, false);
			}
			if (position.Intersects(new Microsoft.Xna.Framework.Rectangle(3648, 576, 256, 64)))
			{
				if (Game1.IsMasterGame && !this.treeNutShot.Value)
				{
					Game1.player.team.MarkCollectedNut("TreeNutShot");
					this.treeNutShot.Value = true;
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(58.5f, 11f) * 64f, 0, this, 0, false);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003028 RID: 12328 RVA: 0x00260A54 File Offset: 0x0025EC54
		public IslandNorth(string map, string name) : base(map, name)
		{
			this.parrotUpgradePerches.Clear();
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(35, 52), new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 4), 10, delegate()
			{
				Game1.addMailForTomorrow("Island_UpgradeBridge", true, true);
				this.bridgeFixed.Value = true;
			}, () => this.bridgeFixed.Value, "Bridge", "Island_Turtle"));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(32, 72), new Microsoft.Xna.Framework.Rectangle(33, 68, 5, 5), 10, delegate()
			{
				Game1.addMailForTomorrow("Island_UpgradeTrader", true, true);
				this.traderActivated.Value = true;
			}, () => this.traderActivated.Value, "Trader", "Island_UpgradeHouse"));
			if (!Game1.netWorldState.Value.ActivatedGoldenParrot && Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(14, 14), new Microsoft.Xna.Framework.Rectangle(2, 2, base.Map.Layers[0].LayerWidth - 4, base.Map.Layers[0].LayerHeight - 4), -1, delegate()
				{
				}, () => false, "GoldenParrot", ""));
			}
			this.largeTerrainFeatures.Add(new Bush(new Vector2(45f, 38f), 4, this, -1));
			this.largeTerrainFeatures.Add(new Bush(new Vector2(47f, 40f), 4, this, -1));
			this.largeTerrainFeatures.Add(new Bush(new Vector2(13f, 33f), 4, this, -1));
			this.largeTerrainFeatures.Add(new Bush(new Vector2(5f, 30f), 4, this, -1));
		}

		// Token: 0x06003029 RID: 12329 RVA: 0x00260CC4 File Offset: 0x0025EEC4
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			IslandNorth location = l as IslandNorth;
			if (location != null)
			{
				this.bridgeFixed.Value = location.bridgeFixed.Value;
				this.treeNutShot.Value = location.treeNutShot.Value;
				this.caveOpened.Value = location.caveOpened.Value;
				this.traderActivated.Value = location.traderActivated.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x0600302A RID: 12330 RVA: 0x00260D3C File Offset: 0x0025EF3C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "untitled tile sheet");
			if (tileIndexAt - 2074 <= 4)
			{
				Utility.TryOpenShopMenu("IslandTrade", null, true);
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x0600302B RID: 12331 RVA: 0x00260D88 File Offset: 0x0025EF88
		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			return new List<Vector2>
			{
				new Vector2(56f, 27f)
			};
		}

		// Token: 0x0600302C RID: 12332 RVA: 0x00260DA4 File Offset: 0x0025EFA4
		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return false;
		}

		// Token: 0x0600302D RID: 12333 RVA: 0x00260DA8 File Offset: 0x0025EFA8
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
			{
				suspensionBridge.Update(time);
			}
			if (!this.caveOpened.Value && Utility.isOnScreen(Utility.PointToVector2(this.boulderPosition.Location), 1))
			{
				this.boulderKnockTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				this.boulderTextTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.doneHittingBoulderWithToolTimer > 0f)
				{
					this.doneHittingBoulderWithToolTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (this.doneHittingBoulderWithToolTimer <= 0f)
					{
						this.boulderTextTimer = 2000f;
						this.boulderTextString = Game1.content.LoadString("Strings\\Locations:IslandNorth_CaveTool_" + Game1.random.Next(4).ToString());
					}
				}
				if (this.boulderKnocksLeft > 0)
				{
					if (this.boulderKnockTimer < 0f)
					{
						Game1.playSound("hammer", null);
						this.boulderKnocksLeft--;
						this.boulderKnockTimer = 500f;
						if (this.boulderKnocksLeft == 0 && Game1.random.NextBool())
						{
							DelayedAction.functionAfterDelay(delegate
							{
								this.boulderTextTimer = 2000f;
								this.boulderTextString = Game1.content.LoadString("Strings\\Locations:IslandNorth_CaveHelp_" + Game1.random.Next(4).ToString());
							}, 1000);
						}
					}
				}
				else if (Game1.random.NextDouble() < 0.002 && this.boulderTextTimer < -500f)
				{
					this.boulderKnocksLeft = Game1.random.Next(3, 6);
				}
			}
			if (!this._sawFlameSpriteSouth && Utility.isThereAFarmerWithinDistance(new Vector2(36f, 79f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_North_South", true, false);
				TemporaryAnimatedSprite v = base.getTemporarySpriteByID(999);
				if (v != null)
				{
					v.yPeriodic = false;
					v.xPeriodic = false;
					v.sourceRect.Y = 0;
					v.sourceRectStartingPos.Y = 0f;
					v.motion = new Vector2(1f, -4f);
					v.acceleration = new Vector2(0f, -0.04f);
					v.drawAboveAlwaysFront = true;
				}
				base.localSound("magma_sprite_spot", null, null, SoundContext.Default);
				v = base.getTemporarySpriteByID(998);
				if (v != null)
				{
					v.yPeriodic = false;
					v.xPeriodic = false;
					v.motion = new Vector2(1f, -4f);
					v.acceleration = new Vector2(0f, -0.04f);
				}
				this._sawFlameSpriteSouth = true;
			}
			if (!this._sawFlameSpriteNorth && Utility.isThereAFarmerWithinDistance(new Vector2(41f, 30f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_North_North", true, false);
				TemporaryAnimatedSprite v2 = base.getTemporarySpriteByID(9999);
				if (v2 != null)
				{
					v2.yPeriodic = false;
					v2.xPeriodic = false;
					v2.sourceRect.Y = 0;
					v2.sourceRectStartingPos.Y = 0f;
					v2.motion = new Vector2(0f, -4f);
					v2.acceleration = new Vector2(0f, -0.04f);
					v2.yStopCoordinate = 1216;
					v2.reachedStopCoordinate = delegate(int x)
					{
						base.removeTemporarySpritesWithID(9999);
					};
				}
				base.localSound("magma_sprite_spot", null, null, SoundContext.Default);
				v2 = base.getTemporarySpriteByID(9998);
				if (v2 != null)
				{
					v2.yPeriodic = false;
					v2.xPeriodic = false;
					v2.motion = new Vector2(0f, -4f);
					v2.acceleration = new Vector2(0f, -0.04f);
					v2.yStopCoordinate = 1280;
					v2.reachedStopCoordinate = delegate(int x)
					{
						base.removeTemporarySpritesWithID(9998);
					};
				}
				this._sawFlameSpriteNorth = true;
			}
			if (!this.hasTriedFirstEntryDigSiteLoad)
			{
				if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail("ISLAND_NORTH_DIGSITE_LOAD"))
				{
					Game1.addMail("ISLAND_NORTH_DIGSITE_LOAD", true, false);
					for (int i = 0; i < 40; i++)
					{
						this.digSiteUpdate();
					}
				}
				this.hasTriedFirstEntryDigSiteLoad = true;
			}
		}

		// Token: 0x0600302E RID: 12334 RVA: 0x00261234 File Offset: 0x0025F434
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return (!this.caveOpened.Value && this.boulderPosition.Intersects(position)) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x0600302F RID: 12335 RVA: 0x00261264 File Offset: 0x0025F464
		public override bool isTilePlaceable(Vector2 tile_location, bool itemIsPassable = false)
		{
			Point non_tile_position = Utility.Vector2ToPoint((tile_location + new Vector2(0.5f, 0.5f)) * 64f);
			return (this.caveOpened.Value || !this.boulderPosition.Contains(non_tile_position)) && base.isTilePlaceable(tile_location, itemIsPassable);
		}

		// Token: 0x06003030 RID: 12336 RVA: 0x002612BC File Offset: 0x0025F4BC
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.digSiteUpdate();
			this.terrainFeatures.RemoveWhere(delegate(KeyValuePair<Vector2, TerrainFeature> pair)
			{
				HoeDirt dirt = pair.Value as HoeDirt;
				return dirt != null && dirt.crop != null && dirt.crop.forageCrop.Value;
			});
			Microsoft.Xna.Framework.Rectangle[] gingerLocations = new Microsoft.Xna.Framework.Rectangle[]
			{
				new Microsoft.Xna.Framework.Rectangle(10, 51, 1, 8),
				new Microsoft.Xna.Framework.Rectangle(15, 59, 1, 4),
				new Microsoft.Xna.Framework.Rectangle(18, 34, 1, 1),
				new Microsoft.Xna.Framework.Rectangle(40, 48, 6, 6)
			};
			for (int i = 0; i < 1; i++)
			{
				Microsoft.Xna.Framework.Rectangle r = gingerLocations[Game1.random.Next(gingerLocations.Length)];
				Vector2 origin = new Vector2((float)Game1.random.Next(r.X, r.Right), (float)Game1.random.Next(r.Y, r.Bottom));
				foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16, 50))
				{
					string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back", false);
					if (!this.terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.35f))
					{
						HoeDirt d = new HoeDirt(0, new Crop(true, "2", (int)v.X, (int)v.Y, this));
						d.state.Value = 2;
						this.terrainFeatures.Add(v, d);
					}
				}
			}
		}

		// Token: 0x06003031 RID: 12337 RVA: 0x00261490 File Offset: 0x0025F690
		private bool isTileOpenForDigSiteStone(int tileX, int tileY)
		{
			return this.doesTileHaveProperty(tileX, tileY, "Diggable", "Back", false) != null && this.doesTileHaveProperty(tileX, tileY, "Diggable", "Back", false) == "T" && this.CanItemBePlacedHere(new Vector2((float)tileX, (float)tileY), false, CollisionMask.All, CollisionMask.None, false, false);
		}

		// Token: 0x06003032 RID: 12338 RVA: 0x002614EC File Offset: 0x0025F6EC
		public void digSiteUpdate()
		{
			bool added_forced_bone_node = false;
			Random r = Utility.CreateDaySaveRandom(78.0, 0.0, 0.0);
			Microsoft.Xna.Framework.Rectangle digSiteBounds = new Microsoft.Xna.Framework.Rectangle(4, 47, 22, 20);
			int numberOfAdditionsToTry = 20;
			Vector2[] claySpots = new Vector2[]
			{
				new Vector2(18f, 49f),
				new Vector2(15f, 54f),
				new Vector2(21f, 52f),
				new Vector2(18f, 61f),
				new Vector2(23f, 57f),
				new Vector2(9f, 63f),
				new Vector2(7f, 51f),
				new Vector2(7f, 57f)
			};
			if (Utility.getNumObjectsOfIndexWithinRectangle(digSiteBounds, new string[]
			{
				"(O)816",
				"(O)817",
				"(O)818",
				"(O)819",
				"(O)32",
				"(O)38",
				"(O)40",
				"(O)42",
				"(O)590"
			}, this) < 60)
			{
				for (int i = 0; i < numberOfAdditionsToTry; i++)
				{
					Vector2 position = Utility.getRandomPositionInThisRectangle(digSiteBounds, Game1.random);
					Vector2 claySpot = r.Choose(claySpots);
					if (this.isTileOpenForDigSiteStone((int)position.X, (int)position.Y))
					{
						if (!added_forced_bone_node || Game1.random.NextDouble() < 0.3)
						{
							added_forced_bone_node = true;
							Object node = ItemRegistry.Create<Object>("(O)" + (816 + Game1.random.Next(2)).ToString(), 1, 0, false);
							node.MinutesUntilReady = 4;
							this.objects.Add(position, node);
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							int xCoord = (int)position.X;
							int yCoord = (int)position.Y;
							if (this.CanItemBePlacedHere(position, false, CollisionMask.All, CollisionMask.None, false, false) && !this.IsTileOccupiedBy(position, CollisionMask.All, CollisionMask.None, false) && !base.hasTileAt(xCoord, yCoord, "AlwaysFront", null) && !base.hasTileAt(xCoord, yCoord, "Front", null) && !base.isBehindBush(position) && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back", false) != null && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back", false) == "T")
							{
								this.objects.Add(position, ItemRegistry.Create<Object>("(O)590", 1, 0, false));
							}
						}
						else if (Game1.random.NextDouble() < 0.06)
						{
							this.terrainFeatures.Add(position, new Tree("8", 1, false));
						}
						else if (Game1.random.NextDouble() < 0.2)
						{
							if (this.isTileOpenForDigSiteStone((int)claySpot.X, (int)claySpot.Y))
							{
								int numToSpawn = Game1.random.Next(2, 5);
								for (int j = 0; j < numToSpawn; j++)
								{
									Object clay = ItemRegistry.Create<Object>("(O)818", 1, 0, false);
									clay.MinutesUntilReady = 4;
									Utility.spawnObjectAround(claySpot, clay, this, false, delegate(Object o)
									{
										o.CanBeGrabbed = false;
										o.IsSpawnedObject = false;
									});
								}
							}
						}
						else if (Game1.random.NextDouble() < 0.25)
						{
							this.objects.Add(position, new Object(r.Choose("785", "676", "677"), 1, false, -1, 0));
						}
						else
						{
							string id = r.Choose("32", "38", "40", "42");
							this.objects.Add(position, new Object(id, 1, false, -1, 0)
							{
								MinutesUntilReady = 2
							});
						}
					}
				}
				return;
			}
			if (Utility.getNumObjectsOfIndexWithinRectangle(digSiteBounds, new string[]
			{
				"(O)785",
				"(O)676",
				"(O)677"
			}, this) < 100)
			{
				int times = r.Next(4);
				for (int k = 0; k < times; k++)
				{
					Vector2 position2 = Utility.getRandomPositionInThisRectangle(digSiteBounds, Game1.random);
					if (this.isTileOpenForDigSiteStone((int)position2.X, (int)position2.Y))
					{
						this.objects.Add(position2, ItemRegistry.Create<Object>(r.Choose("(O)785", "(O)676", "(O)677"), 1, 0, false));
					}
				}
			}
		}

		// Token: 0x06003033 RID: 12339 RVA: 0x002619B0 File Offset: 0x0025FBB0
		public override bool performOrePanTenMinuteUpdate(Random r)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && this.orePanPoint.Value.Equals(Point.Zero) && r.NextBool())
			{
				for (int tries = 0; tries < 3; tries++)
				{
					Point p = new Point(r.Next(4, 15), r.Next(45, 70));
					if (base.isOpenWater(p.X, p.Y) && FishingRod.distanceToLand(p.X, p.Y, this, false) <= 1 && !base.hasTileAt(p, "Buildings", null))
					{
						if (Game1.player.currentLocation.Equals(this))
						{
							base.playSound("slosh", null, null, SoundContext.Default);
						}
						this.orePanPoint.Value = p;
						return true;
					}
				}
			}
			else if (!this.orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.2)
			{
				this.orePanPoint.Value = Point.Zero;
			}
			return false;
		}

		// Token: 0x06003034 RID: 12340 RVA: 0x00261AE0 File Offset: 0x0025FCE0
		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (!this.caveOpened.Value && tileY == 47 && (tileX == 21 || tileX == 22))
			{
				this.boulderKnockTimer = 500f;
				Game1.playSound("hammer", null);
				this.boulderKnocksLeft = 0;
				this.doneHittingBoulderWithToolTimer = 1200f;
			}
			return base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x06003035 RID: 12341 RVA: 0x00261B44 File Offset: 0x0025FD44
		public override void explosionAt(float x, float y)
		{
			base.explosionAt(x, y);
			if (!this.caveOpened.Value && y == 47f && (x == 21f || x == 22f))
			{
				this.caveOpened.Value = true;
				Game1.addMailForTomorrow("islandNorthCaveOpened", true, true);
			}
		}

		// Token: 0x06003036 RID: 12342 RVA: 0x00261B98 File Offset: 0x0025FD98
		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			this.DrawParallaxHorizon(b, true);
			if (!this.treeNutShot.Value)
			{
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(58.25f, 10f) * 64f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
		}

		// Token: 0x06003037 RID: 12343 RVA: 0x00261C20 File Offset: 0x0025FE20
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
			{
				suspensionBridge.Draw(b);
			}
			if (!this.caveOpened.Value)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Utility.PointToVector2(this.boulderPosition.Location) + new Vector2((float)((this.boulderKnockTimer > 250f) ? Game1.random.Next(-1, 2) : 0), (float)(-64 + ((this.boulderKnockTimer > 250f) ? Game1.random.Next(-1, 2) : 0)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(155, 224, 32, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.boulderPosition.Y / 10000f);
			}
		}

		// Token: 0x06003038 RID: 12344 RVA: 0x00261D30 File Offset: 0x0025FF30
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (!this.caveOpened.Value && this.boulderTextTimer > 0f)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, this.boulderTextString, (int)Game1.GlobalToLocal(Utility.PointToVector2(this.boulderPosition.Location)).X + 64, (int)Game1.GlobalToLocal(Utility.PointToVector2(this.boulderPosition.Location)).Y - 128 - 32, "", 1f, null, 1, 1f, false);
			}
		}

		// Token: 0x06003039 RID: 12345 RVA: 0x00261DC8 File Offset: 0x0025FFC8
		public override bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
		{
			using (List<SuspensionBridge>.Enumerator enumerator = this.suspensionBridges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CheckPlacementPrevention(tileLocation))
					{
						return true;
					}
				}
			}
			return base.IsLocationSpecificPlacementRestriction(tileLocation);
		}

		// Token: 0x0600303A RID: 12346 RVA: 0x00261E28 File Offset: 0x00260028
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (this.bridgeFixed.Value)
			{
				this.ApplyFixedBridge();
			}
			else
			{
				base.ApplyMapOverride("Island_Bridge_Broken", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 3)));
			}
			if (this.traderActivated.Value)
			{
				this.ApplyIslandTraderHut();
			}
		}

		// Token: 0x0600303B RID: 12347 RVA: 0x00261E88 File Offset: 0x00260088
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (this.traderActivated.Value)
			{
				base.removeTemporarySpritesWithIDLocal(8989);
				base.removeTemporarySpritesWithIDLocal(8988);
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(33.45f, 70.33f) * 64f + new Vector2(-16f, -32f), false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandNorth_Trader_1",
					id = 8989,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(39.45f, 70.33f) * 64f + new Vector2(-16f, -32f), false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandNorth_Trader_2",
					id = 8988,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
			}
			if (this.caveOpened.Value && !Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened"))
			{
				Game1.addMailForTomorrow("islandNorthCaveOpened", true, false);
			}
			this.suspensionBridges.Clear();
			SuspensionBridge bridge = new SuspensionBridge(38, 39);
			this.suspensionBridges.Add(bridge);
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_North_South"))
			{
				this._sawFlameSpriteSouth = true;
			}
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_North_North"))
			{
				this._sawFlameSpriteNorth = true;
			}
			if (!this._sawFlameSpriteSouth)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(36f, 79f) * 64f, false, 0f, Color.White)
				{
					id = 999,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					lightId = "IslandNorth_FlameSpirit_South",
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(36.2f, 80.4f) * 64f, false, 0f, Color.White)
				{
					id = 998,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			if (!this._sawFlameSpriteNorth)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(41f, 30f) * 64f, false, 0f, Color.White)
				{
					id = 9999,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					lightId = "IslandNorth_FlameSpirit_North",
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(41.2f, 31.4f) * 64f, false, 0f, Color.White)
				{
					id = 9998,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			Random marsupialRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 978.0, 0.0, 0.0);
			if (!Game1.player.team.collectedNutTracker.Contains("Buried_IslandNorth_26_81"))
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(141, 310, 20, 23), new Vector2(23.75f, 77.15f) * 64f, false, 0f, Color.White)
				{
					totalNumberOfLoops = 999999,
					animationLength = 2,
					interval = 200f,
					id = 79797,
					layerDepth = 1f,
					scale = 4f,
					drawAboveAlwaysFront = true
				});
				return;
			}
			if (!base.IsRainingHere() && marsupialRandom.NextDouble() < 0.1)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(141, 310, 20, 23), new Vector2(23.75f, 77.15f) * 64f, false, 0f, Color.White)
				{
					totalNumberOfLoops = 999999,
					animationLength = 2,
					interval = 200f,
					layerDepth = 1f,
					scale = 4f,
					drawAboveAlwaysFront = true
				});
			}
		}

		// Token: 0x04002089 RID: 8329
		[XmlElement("bridgeFixed")]
		public readonly NetBool bridgeFixed = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400208A RID: 8330
		[XmlElement("traderActivated")]
		public readonly NetBool traderActivated = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400208B RID: 8331
		[XmlElement("caveOpened")]
		public readonly NetBool caveOpened = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400208C RID: 8332
		[XmlElement("treeNutShot")]
		public readonly NetBool treeNutShot = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400208D RID: 8333
		[XmlIgnore]
		public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

		// Token: 0x0400208E RID: 8334
		[XmlIgnore]
		protected bool _sawFlameSpriteSouth;

		// Token: 0x0400208F RID: 8335
		[XmlIgnore]
		protected bool _sawFlameSpriteNorth;

		// Token: 0x04002090 RID: 8336
		[XmlIgnore]
		protected bool hasTriedFirstEntryDigSiteLoad;

		// Token: 0x04002091 RID: 8337
		private float boulderKnockTimer;

		// Token: 0x04002092 RID: 8338
		private float boulderTextTimer;

		// Token: 0x04002093 RID: 8339
		private string boulderTextString;

		// Token: 0x04002094 RID: 8340
		private int boulderKnocksLeft;

		// Token: 0x04002095 RID: 8341
		private Microsoft.Xna.Framework.Rectangle boulderPosition = new Microsoft.Xna.Framework.Rectangle(1344, 3008, 128, 64);

		// Token: 0x04002096 RID: 8342
		private float doneHittingBoulderWithToolTimer;
	}
}
