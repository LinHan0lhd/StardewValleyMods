using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Locations
{
	// Token: 0x020002CC RID: 716
	public class Desert : GameLocation
	{
		// Token: 0x06002EA3 RID: 11939 RVA: 0x00246000 File Offset: 0x00244200
		public Desert()
		{
		}

		// Token: 0x06002EA4 RID: 11940 RVA: 0x00246098 File Offset: 0x00244298
		public Desert(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002EA5 RID: 11941 RVA: 0x00246134 File Offset: 0x00244334
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Layer layer = this.map.GetLayer("Buildings");
			if (((layer != null) ? layer.Tiles[tileLocation] : null) != null)
			{
				return base.checkAction(tileLocation, viewport, who);
			}
			if ((tileLocation.X == 41 || tileLocation.X == 42) && tileLocation.Y == 24)
			{
				this.OnDesertTrader();
				return true;
			}
			if (tileLocation.X >= 34 && tileLocation.X <= 38 && tileLocation.Y == 24)
			{
				this.OnCamel();
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002EA6 RID: 11942 RVA: 0x002461C4 File Offset: 0x002443C4
		public virtual void OnDesertTrader()
		{
			Utility.TryOpenShopMenu("DesertTrade", this, null, null, false, true, null);
		}

		// Token: 0x06002EA7 RID: 11943 RVA: 0x002461F4 File Offset: 0x002443F4
		public virtual void OnCamel()
		{
			Game1.playSound("camel", null);
			this.ShowCamelAnimation();
			Game1.player.faceDirection(0);
			Game1.haltAfterCheck = false;
		}

		// Token: 0x06002EA8 RID: 11944 RVA: 0x0024622C File Offset: 0x0024442C
		public virtual void ShowCamelAnimation()
		{
			if (base.getTemporarySpriteByID(999) == null)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(208, 591, 65, 49),
					sourceRectStartingPos = new Vector2(208f, 591f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 300f,
					scale = 4f,
					position = new Vector2(536f, 340f) * 4f,
					layerDepth = 0.1332f,
					id = 999
				});
			}
		}

		// Token: 0x06002EA9 RID: 11945 RVA: 0x002462F8 File Offset: 0x002444F8
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (who.secretNotesSeen.Contains(18) && xLocation == 40 && yLocation == 55 && who.mailReceived.Add("SecretNote18_done"))
			{
				Game1.createObjectDebris("(O)127", xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x06002EAA RID: 11946 RVA: 0x0024635C File Offset: 0x0024455C
		private void playerReachedBusDoor(Character c, GameLocation l)
		{
			Game1.viewportFreeze = true;
			Game1.player.position.X = -10000f;
			Game1.freezeControls = true;
			Game1.player.CanMove = false;
			this.busDriveOff();
			base.playSound("stoneStep", null, null, SoundContext.Default);
		}

		// Token: 0x06002EAB RID: 11947 RVA: 0x002463B8 File Offset: 0x002445B8
		public override bool answerDialogue(Response answer)
		{
			if (this.lastQuestionKey != null && this.afterQuestion == null && ArgUtility.SplitBySpaceAndGet(this.lastQuestionKey, 0, null) + "_" + answer.responseKey == "DesertBus_Yes")
			{
				this.playerReachedBusDoor(Game1.player, this);
				return true;
			}
			return base.answerDialogue(answer);
		}

		// Token: 0x06002EAC RID: 11948 RVA: 0x00246414 File Offset: 0x00244614
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.leaving = false;
			Game1.ambientLight = Color.White;
			GameLocation previousLocation = Game1.getLocationFromName(Game1.player.previousLocationName);
			bool showingBusArrival = false;
			if (previousLocation == null || previousLocation.GetLocationContextId() != this.GetLocationContextId())
			{
				Desert.warpedToDesert = true;
				if (Game1.player.previousLocationName == "BusStop" && Game1.player.TilePoint.X == 16 && Game1.player.TilePoint.Y == 24)
				{
					Desert.warpedToDesert = false;
					showingBusArrival = true;
					Game1.changeMusicTrack("silence", false, MusicContext.Default);
					this.busPosition = new Vector2(17f, 24f) * 64f;
					this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
					{
						interval = 999999f,
						animationLength = 1,
						holdLastFrame = true,
						layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
						scale = 4f
					};
					Game1.displayFarmer = false;
					this.busDriveBack();
				}
			}
			if (!showingBusArrival)
			{
				this.drivingOff = false;
				this.drivingBack = false;
				this.busMotion = Vector2.Zero;
				this.busPosition = new Vector2(17f, 24f) * 64f;
				this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
				{
					interval = 999999f,
					animationLength = 6,
					holdLastFrame = true,
					layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
					scale = 4f
				};
			}
			if (base.GetType() == typeof(DesertFestival))
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(208, 524, 65, 49),
					sourceRectStartingPos = new Vector2(208f, 524f),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(536f, 340f) * 4f,
					layerDepth = 0.1324f,
					id = 996
				});
			}
			else
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 513, 208, 101),
					sourceRectStartingPos = new Vector2(0f, 513f),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(528f, 298f) * 4f,
					layerDepth = 0.1324f,
					id = 996
				});
			}
			if (this.IsTravelingDesertMerchantHere())
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 614, 20, 26),
					sourceRectStartingPos = new Vector2(0f, 614f),
					animationLength = 1,
					totalNumberOfLoops = 999,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(663f, 354f) * 4f,
					layerDepth = 0.1328f,
					id = 995
				});
			}
			if (Game1.timeOfDay >= Game1.getModeratelyDarkTime(this))
			{
				this.lightMerchantLamps();
			}
		}

		// Token: 0x06002EAD RID: 11949 RVA: 0x002468A5 File Offset: 0x00244AA5
		private bool IsTravelingDesertMerchantHere()
		{
			return !Game1.IsWinter || Game1.dayOfMonth < 15 || Game1.dayOfMonth > 17;
		}

		// Token: 0x06002EAE RID: 11950 RVA: 0x002468C2 File Offset: 0x00244AC2
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return position.Intersects(this.desertMerchantBounds) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x06002EAF RID: 11951 RVA: 0x002468E4 File Offset: 0x00244AE4
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (Game1.currentLocation == this)
			{
				if (this.IsTravelingDesertMerchantHere())
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(40, 614, 20, 26),
							sourceRectStartingPos = new Vector2(40f, 614f),
							animationLength = 6,
							totalNumberOfLoops = 1,
							interval = 100f,
							scale = 4f,
							position = new Vector2(663f, 354f) * 4f,
							layerDepth = 0.1336f,
							id = 997,
							pingPong = true
						});
					}
					else
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(20, 614, 20, 26),
							sourceRectStartingPos = new Vector2(20f, 614f),
							animationLength = 1,
							totalNumberOfLoops = 1,
							interval = (float)Game1.random.Next(100, 800),
							scale = 4f,
							position = new Vector2(663f, 354f) * 4f,
							layerDepth = 0.1332f,
							id = 998
						});
					}
				}
				this.ShowCamelAnimation();
				if (timeOfDay == Game1.getModeratelyDarkTime(this) && Game1.currentLocation == this)
				{
					this.lightMerchantLamps();
				}
			}
		}

		// Token: 0x06002EB0 RID: 11952 RVA: 0x00246AB0 File Offset: 0x00244CB0
		public void lightMerchantLamps()
		{
			if (base.getTemporarySpriteByID(1000) != null)
			{
				return;
			}
			this.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
				sourceRectStartingPos = new Vector2(181f, 633f),
				animationLength = 1,
				totalNumberOfLoops = 9999,
				interval = 99999f,
				scale = 4f,
				position = new Vector2(545f, 309f) * 4f,
				layerDepth = 0.134f,
				id = 1000,
				lightId = "Desert_MerchantLamp_1",
				lightRadius = 1f,
				lightcolor = Color.Black
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
				sourceRectStartingPos = new Vector2(181f, 633f),
				animationLength = 1,
				totalNumberOfLoops = 9999,
				interval = 99999f,
				scale = 4f,
				position = new Vector2(644f, 360f) * 4f,
				layerDepth = 0.134f,
				id = 1000,
				lightId = "Desert_MerchantLamp_2",
				lightRadius = 1f,
				lightcolor = Color.Black
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
				sourceRect = new Microsoft.Xna.Framework.Rectangle(181, 633, 7, 6),
				sourceRectStartingPos = new Vector2(181f, 633f),
				animationLength = 1,
				totalNumberOfLoops = 9999,
				interval = 99999f,
				scale = 4f,
				position = new Vector2(717f, 309f) * 4f,
				layerDepth = 0.134f,
				id = 1000,
				lightId = "Desert_MerchantLamp_3",
				lightRadius = 1f,
				lightcolor = Color.Black
			});
		}

		// Token: 0x06002EB1 RID: 11953 RVA: 0x00246D38 File Offset: 0x00244F38
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (this.farmers.Count <= 1)
			{
				this.busDoor = null;
			}
		}

		// Token: 0x06002EB2 RID: 11954 RVA: 0x00246D58 File Offset: 0x00244F58
		public void busDriveOff()
		{
			this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			this.busDoor.timer = 0f;
			this.busDoor.interval = 70f;
			this.busDoor.endFunction = new TemporaryAnimatedSprite.endBehavior(this.busStartMovingOff);
			base.localSound("trashcanlid", null, null, SoundContext.Default);
			this.drivingBack = false;
			this.busDoor.paused = false;
		}

		// Token: 0x06002EB3 RID: 11955 RVA: 0x00246E68 File Offset: 0x00245068
		public void busDriveBack()
		{
			this.busPosition.X = (float)this.map.RequireLayer("Back").DisplayWidth;
			this.busDoor.Position = this.busPosition + new Vector2(16f, 26f) * 4f;
			this.drivingBack = true;
			this.drivingOff = false;
			base.localSound("busDriveOff", null, null, SoundContext.Default);
			this.busMotion = new Vector2(-6f, 0f);
		}

		// Token: 0x06002EB4 RID: 11956 RVA: 0x00246F06 File Offset: 0x00245106
		private void busStartMovingOff(int extraInfo)
		{
			Game1.globalFadeToBlack(delegate
			{
				Game1.globalFadeToClear(null, 0.02f);
				base.localSound("batFlap", null, null, SoundContext.Default);
				this.drivingOff = true;
				base.localSound("busDriveOff", null, null, SoundContext.Default);
				Game1.changeMusicTrack("silence", false, MusicContext.Default);
			}, 0.02f);
		}

		// Token: 0x06002EB5 RID: 11957 RVA: 0x00246F1E File Offset: 0x0024511E
		public override bool IgnoreTouchActions()
		{
			return base.IgnoreTouchActions() || this.drivingBack || this.drivingOff;
		}

		// Token: 0x06002EB6 RID: 11958 RVA: 0x00246F38 File Offset: 0x00245138
		public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
		{
			if (this.IgnoreTouchActions())
			{
				return;
			}
			if (ArgUtility.Get(action, 0, null, true) == "DesertBus")
			{
				Response[] returnOptions = new Response[]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), returnOptions, "DesertBus");
				return;
			}
			base.performTouchAction(action, playerStandingPosition);
		}

		// Token: 0x06002EB7 RID: 11959 RVA: 0x00246FC8 File Offset: 0x002451C8
		private void doorOpenAfterReturn(int extraInfo)
		{
			base.localSound("batFlap", null, null, SoundContext.Default);
			this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			Game1.player.Position = new Vector2(18f, 27f) * 64f;
			this.lastTouchActionLocation = Game1.player.Tile;
			Game1.displayFarmer = true;
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
			Game1.changeMusicTrack("none", true, MusicContext.Default);
			GameLocation.HandleMusicChange(null, this);
		}

		// Token: 0x06002EB8 RID: 11960 RVA: 0x002470ED File Offset: 0x002452ED
		private void busLeftToValley()
		{
			Game1.viewport.Y = -100000;
			Game1.viewportFreeze = true;
			Game1.warpFarmer("BusStop", 22, 10, true);
		}

		// Token: 0x06002EB9 RID: 11961 RVA: 0x00247114 File Offset: 0x00245314
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (this.drivingBack || this.drivingOff)
			{
				if (Game1.player.currentLocation == this)
				{
					Game1.player.CanMove = false;
				}
				else
				{
					this.drivingBack = false;
					this.drivingOff = false;
				}
			}
			if (this.drivingOff && !this.leaving)
			{
				this.busMotion.X = this.busMotion.X - 0.075f;
				if (this.busPosition.X + 512f < 0f)
				{
					this.leaving = true;
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.busLeftToValley), 0.01f);
				}
			}
			if (this.drivingBack && this.busMotion != Vector2.Zero)
			{
				Game1.player.Position = this.busDoor.position;
				Game1.player.freezePause = 100;
				if (this.busPosition.X - 1088f < 256f)
				{
					this.busMotion.X = Math.Min(-1f, this.busMotion.X * 0.98f);
				}
				if (Math.Abs(this.busPosition.X - 1088f) <= Math.Abs(this.busMotion.X * 1.5f))
				{
					this.busPosition.X = 1088f;
					this.busMotion = Vector2.Zero;
					Game1.globalFadeToBlack(delegate
					{
						this.drivingBack = false;
						this.busDoor.Position = this.busPosition + new Vector2(16f, 26f) * 4f;
						this.busDoor.pingPong = true;
						this.busDoor.interval = 70f;
						this.busDoor.currentParentTileIndex = 5;
						this.busDoor.endFunction = new TemporaryAnimatedSprite.endBehavior(this.doorOpenAfterReturn);
						base.localSound("trashcanlid", null, null, SoundContext.Default);
						Game1.globalFadeToClear(null, 0.02f);
					}, 0.02f);
				}
			}
			if (!this.busMotion.Equals(Vector2.Zero))
			{
				this.busPosition += this.busMotion;
				if (this.busDoor != null)
				{
					this.busDoor.Position += this.busMotion;
				}
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.busDoor;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.update(time);
			}
			if (this.IsTravelingDesertMerchantHere())
			{
				this.chimneyTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.chimneyTimer <= 0)
				{
					this.chimneyTimer = 500;
					Vector2 smokeSpot = new Vector2(670f, 308f) * 4f;
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), smokeSpot, false, 0.002f, new Color(255, 222, 198))
					{
						alpha = 0.05f,
						alphaFade = -0.01f,
						alphaFadeFade = -8E-05f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
						drawAboveAlwaysFront = (this is DesertFestival)
					});
				}
			}
		}

		// Token: 0x06002EBA RID: 11962 RVA: 0x00247437 File Offset: 0x00245637
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.removeObjectsAndSpawned(33, 20, 13, 6);
		}

		// Token: 0x06002EBB RID: 11963 RVA: 0x0024744D File Offset: 0x0024564D
		public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
		{
			return (v.X < 33f || v.X >= 46f || v.Y < 20f || v.Y >= 25f) && base.isTilePlaceable(v, itemIsPassable);
		}

		// Token: 0x06002EBC RID: 11964 RVA: 0x0024748D File Offset: 0x0024568D
		public override bool shouldHideCharacters()
		{
			return this.drivingOff || this.drivingBack;
		}

		// Token: 0x06002EBD RID: 11965 RVA: 0x002474A0 File Offset: 0x002456A0
		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y))), new Microsoft.Xna.Framework.Rectangle?(this.busSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f) / 10000f);
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.busDoor;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(spriteBatch, false, 0, 0, 1f);
			}
			if (this.drivingOff || this.drivingBack)
			{
				if (Game1.netWorldState.Value.canDriveYourselfToday.Value || (this.drivingOff && Desert.warpedToDesert))
				{
					Game1.player.faceDirection(3);
					Game1.player.blinkTimer = -1000;
					Game1.player.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(117, 99999, 0, false, true, null, false, 0), 117, new Microsoft.Xna.Framework.Rectangle(48, 608, 16, 32), Game1.GlobalToLocal(new Vector2((float)((int)(this.busPosition.X + 4f)), (float)((int)(this.busPosition.Y - 8f))) + this.pamOffset * 4f), Vector2.Zero, (this.busPosition.Y + 192f + 4f) / 10000f, Color.White, 0f, 1f, Game1.player);
					spriteBatch.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y - 40)) + this.pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle?(this.transparentWindowSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 8f) / 10000f);
					return;
				}
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y)) + this.pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle?(this.pamSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 4f) / 10000f);
			}
		}

		// Token: 0x04001FBF RID: 8127
		public const int busDefaultXTile = 17;

		// Token: 0x04001FC0 RID: 8128
		public const int busDefaultYTile = 24;

		// Token: 0x04001FC1 RID: 8129
		private TemporaryAnimatedSprite busDoor;

		// Token: 0x04001FC2 RID: 8130
		private Vector2 busPosition;

		// Token: 0x04001FC3 RID: 8131
		private Vector2 busMotion;

		// Token: 0x04001FC4 RID: 8132
		public bool drivingOff;

		// Token: 0x04001FC5 RID: 8133
		public bool drivingBack;

		// Token: 0x04001FC6 RID: 8134
		public bool leaving;

		// Token: 0x04001FC7 RID: 8135
		private int chimneyTimer = 500;

		// Token: 0x04001FC8 RID: 8136
		private Microsoft.Xna.Framework.Rectangle desertMerchantBounds = new Microsoft.Xna.Framework.Rectangle(2112, 1280, 836, 280);

		// Token: 0x04001FC9 RID: 8137
		public static bool warpedToDesert;

		// Token: 0x04001FCA RID: 8138
		private Microsoft.Xna.Framework.Rectangle busSource = new Microsoft.Xna.Framework.Rectangle(288, 1247, 128, 64);

		// Token: 0x04001FCB RID: 8139
		private Microsoft.Xna.Framework.Rectangle pamSource = new Microsoft.Xna.Framework.Rectangle(384, 1311, 15, 19);

		// Token: 0x04001FCC RID: 8140
		private Microsoft.Xna.Framework.Rectangle transparentWindowSource = new Microsoft.Xna.Framework.Rectangle(0, 0, 21, 41);

		// Token: 0x04001FCD RID: 8141
		private Vector2 pamOffset = new Vector2(0f, 29f);
	}
}
