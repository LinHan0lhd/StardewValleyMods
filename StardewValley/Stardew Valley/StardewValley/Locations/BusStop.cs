using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C4 RID: 708
	public class BusStop : GameLocation
	{
		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06002DED RID: 11757 RVA: 0x0023EDE0 File Offset: 0x0023CFE0
		// (set) Token: 0x06002DEE RID: 11758 RVA: 0x0023EDE8 File Offset: 0x0023CFE8
		[XmlIgnore]
		public int TicketPrice { get; set; } = 500;

		// Token: 0x06002DEF RID: 11759 RVA: 0x0023EDF4 File Offset: 0x0023CFF4
		public BusStop()
		{
		}

		// Token: 0x06002DF0 RID: 11760 RVA: 0x0023EE5C File Offset: 0x0023D05C
		public BusStop(string mapPath, string name) : base(mapPath, name)
		{
			this.busPosition = new Vector2(21f, 6f) * 64f;
		}

		// Token: 0x06002DF1 RID: 11761 RVA: 0x0023EEE5 File Offset: 0x0023D0E5
		public override bool IgnoreTouchActions()
		{
			return base.IgnoreTouchActions() || this.drivingBack || this.drivingOff;
		}

		// Token: 0x06002DF2 RID: 11762 RVA: 0x0023EF00 File Offset: 0x0023D100
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "outdoors");
			if (tileIndexAt != 958)
			{
				if (tileIndexAt == 1057)
				{
					if (Game1.MasterPlayer.mailReceived.Contains("ccVault"))
					{
						if (Game1.player.isRidingHorse() && Game1.player.mount != null)
						{
							Game1.player.mount.checkAction(Game1.player, this);
						}
						else
						{
							string displayPrice = Utility.getNumberWithCommas(this.TicketPrice);
							if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
							{
								base.createQuestionDialogueWithCustomWidth(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert", displayPrice), base.createYesNoResponses(), "Bus");
								goto IL_FF;
							}
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert", displayPrice), base.createYesNoResponses(), "Bus");
							goto IL_FF;
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_DesertOutOfService"));
					}
					return true;
				}
				if (tileIndexAt - 1080 <= 1)
				{
					goto IL_2F;
				}
				IL_FF:
				return base.checkAction(tileLocation, viewport, who);
			}
			IL_2F:
			base.ShowMineCartMenu("Default", "Bus");
			return true;
		}

		// Token: 0x06002DF3 RID: 11763 RVA: 0x0023F018 File Offset: 0x0023D218
		private void playerReachedBusDoor(Character c, GameLocation l)
		{
			this.forceWarpTimer = 0;
			Game1.player.position.X = -10000f;
			Game1.changeMusicTrack("silence", false, MusicContext.Default);
			this.busDriveOff();
			base.playSound("stoneStep", null, null, SoundContext.Default);
			if (Game1.player.mount != null)
			{
				Game1.player.mount.farmerPassesThrough = false;
			}
		}

		// Token: 0x06002DF4 RID: 11764 RVA: 0x0023F08C File Offset: 0x0023D28C
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (Game1.netWorldState.Value.canDriveYourselfToday.Value && Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.canDriveYourselfToday.Value = false;
			}
			Object possibleSign = base.getObjectAtTile(25, 10, false);
			if (possibleSign != null && possibleSign.SpecialVariable == 987659)
			{
				this.objects.Remove(new Vector2(25f, 10f));
			}
		}

		// Token: 0x06002DF5 RID: 11765 RVA: 0x0023F10C File Offset: 0x0023D30C
		public override bool answerDialogue(Response answer)
		{
			if (this.lastQuestionKey != null && this.afterQuestion == null && ArgUtility.SplitBySpaceAndGet(this.lastQuestionKey, 0, null) + "_" + answer.responseKey == "Bus_Yes")
			{
				NPC pam = Game1.getCharacterFromName("Pam", true, false);
				if (!Game1.netWorldState.Value.canDriveYourselfToday.Value && (!this.characters.Contains(pam) || pam.TilePoint.X != 21 || pam.TilePoint.Y != 10))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NoDriver"));
				}
				else if (Game1.player.Money < this.TicketPrice)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
				}
				else
				{
					Game1.player.Money -= this.TicketPrice;
					Game1.freezeControls = true;
					Game1.viewportFreeze = true;
					this.forceWarpTimer = 8000;
					Game1.player.controller = new PathFindController(Game1.player, this, new Point(22, 9), 0, new PathFindController.endBehavior(this.playerReachedBusDoor));
					Game1.player.setRunning(true, false);
					if (Game1.player.mount != null)
					{
						Game1.player.mount.farmerPassesThrough = true;
					}
					Desert.warpedToDesert = false;
				}
				return true;
			}
			return base.answerDialogue(answer);
		}

		// Token: 0x06002DF6 RID: 11766 RVA: 0x0023F280 File Offset: 0x0023D480
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.leaving = false;
			if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
			{
				this.minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(1032f, 144f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					totalNumberOfLoops = 999999,
					interval = 60f,
					flipped = true
				};
			}
			if (Game1.getFarm().grandpaScore.Value == 0 && Game1.year >= 3)
			{
				Game1.player.eventsSeen.Remove("558292");
			}
			bool arrived_from_other_location_context = false;
			GameLocation previous_location = Game1.getLocationFromName(Game1.player.previousLocationName);
			if (previous_location != null && previous_location.GetLocationContext() != this.GetLocationContext())
			{
				arrived_from_other_location_context = true;
			}
			if (Game1.player.TilePoint.Y > 16 || Game1.eventUp || Game1.player.TilePoint.X <= 10 || Game1.player.isRidingHorse() || !arrived_from_other_location_context)
			{
				this.drivingOff = false;
				this.drivingBack = false;
				this.busMotion = Vector2.Zero;
				this.busPosition = new Vector2(21f, 6f) * 64f;
				this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
				{
					interval = 999999f,
					animationLength = 6,
					holdLastFrame = true,
					layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
					scale = 4f
				};
			}
			else
			{
				Game1.changeMusicTrack("silence", false, MusicContext.Default);
				this.busPosition = new Vector2(21f, 6f) * 64f;
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
			if (Game1.player.TilePoint.Y > 16 && Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken") && Game1.isDarkOut(this) && Game1.random.NextDouble() < 0.01)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(12f, 6.5f) * 64f, true, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(-3f, 0f),
					animationLength = 4,
					interval = 80f,
					totalNumberOfLoops = 200,
					layerDepth = 0.0448f,
					delayBeforeAnimationStart = Game1.random.Next(1500)
				});
			}
		}

		// Token: 0x06002DF7 RID: 11767 RVA: 0x0023F631 File Offset: 0x0023D831
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (this.farmers.Count <= 1)
			{
				this.minecartSteam = null;
				this.busDoor = null;
			}
		}

		// Token: 0x06002DF8 RID: 11768 RVA: 0x0023F658 File Offset: 0x0023D858
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

		// Token: 0x06002DF9 RID: 11769 RVA: 0x0023F768 File Offset: 0x0023D968
		public void busDriveBack()
		{
			this.busPosition.X = (float)this.map.RequireLayer("Back").DisplayWidth;
			this.busDoor.Position = this.busPosition + new Vector2(16f, 26f) * 4f;
			this.drivingBack = true;
			this.drivingOff = false;
			base.localSound("busDriveOff", null, null, SoundContext.Default);
			this.busMotion = new Vector2(-12f, 0f);
			Game1.freezeControls = true;
		}

		// Token: 0x06002DFA RID: 11770 RVA: 0x0023F80C File Offset: 0x0023DA0C
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

		// Token: 0x06002DFB RID: 11771 RVA: 0x0023F824 File Offset: 0x0023DA24
		private void doorOpenAfterReturn(int extraInfo)
		{
			this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			Game1.player.Position = new Vector2(22f, 10f) * 64f;
			this.lastTouchActionLocation = Game1.player.Tile;
			Game1.displayFarmer = true;
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
			Game1.changeMusicTrack("none", true, MusicContext.Default);
			GameLocation.HandleMusicChange(null, this);
		}

		// Token: 0x06002DFC RID: 11772 RVA: 0x0023F92B File Offset: 0x0023DB2B
		private void busLeftToDesert()
		{
			Game1.viewportFreeze = true;
			Game1.warpFarmer("Desert", 16, 24, true);
			Game1.globalFade = false;
		}

		// Token: 0x06002DFD RID: 11773 RVA: 0x0023F948 File Offset: 0x0023DB48
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
			if (this.forceWarpTimer > 0)
			{
				this.forceWarpTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.forceWarpTimer <= 0)
				{
					this.playerReachedBusDoor(Game1.player, this);
				}
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.minecartSteam;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.update(time);
			}
			if (this.drivingOff && !this.leaving)
			{
				this.busMotion.X = this.busMotion.X - 0.075f;
				if (this.busPosition.X + 512f < 10f)
				{
					this.leaving = true;
					this.busLeftToDesert();
				}
			}
			if (this.drivingBack && this.busMotion != Vector2.Zero)
			{
				Game1.player.Position = this.busPosition;
				if (this.busPosition.X - 1344f < 512f)
				{
					this.busMotion.X = Math.Min(-1f, this.busMotion.X * 0.98f);
				}
				if (Math.Abs(this.busPosition.X - 1344f) <= Math.Abs(this.busMotion.X * 1.5f))
				{
					this.busPosition.X = 1344f;
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
						if (!string.IsNullOrEmpty(Game1.player.horseName.Value))
						{
							int i = 0;
							while (i < this.characters.Count)
							{
								Horse horse = this.characters[i] as Horse;
								if (horse != null && horse.getOwner() == Game1.player)
								{
									if (string.IsNullOrEmpty(this.characters[i].Name))
									{
										Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse2", this.characters[i].displayName));
										break;
									}
									Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse" + (Game1.random.Next(2) + 1).ToString(), this.characters[i].displayName));
									break;
								}
								else
								{
									i++;
								}
							}
						}
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
			TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.busDoor;
			if (temporaryAnimatedSprite2 == null)
			{
				return;
			}
			temporaryAnimatedSprite2.update(time);
		}

		// Token: 0x06002DFE RID: 11774 RVA: 0x0023FB55 File Offset: 0x0023DD55
		public override bool shouldHideCharacters()
		{
			return this.drivingOff || this.drivingBack;
		}

		// Token: 0x06002DFF RID: 11775 RVA: 0x0023FB68 File Offset: 0x0023DD68
		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.minecartSteam;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(spriteBatch, false, 0, 0, 1f);
			}
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y))), new Microsoft.Xna.Framework.Rectangle?(this.busSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f) / 10000f);
			TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.busDoor;
			if (temporaryAnimatedSprite2 != null)
			{
				temporaryAnimatedSprite2.draw(spriteBatch, false, 0, 0, 1f);
			}
			if ((Game1.netWorldState.Value.canDriveYourselfToday.Value && (this.drivingOff || this.drivingBack)) || (this.drivingBack && Desert.warpedToDesert))
			{
				Game1.player.faceDirection(3);
				Game1.player.blinkTimer = -1000;
				Game1.player.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(117, 99999, 0, false, true, null, false, 0), 117, new Microsoft.Xna.Framework.Rectangle(48, 608, 16, 32), Game1.GlobalToLocal(new Vector2((float)((int)(this.busPosition.X + 4f)), (float)((int)(this.busPosition.Y - 8f))) + this.pamOffset * 4f), Vector2.Zero, (this.busPosition.Y + 192f + 4f) / 10000f, Color.White, 0f, 1f, Game1.player);
				spriteBatch.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y - 40)) + this.pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 21, 41)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 8f) / 10000f);
				return;
			}
			if (this.drivingOff || this.drivingBack)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((int)this.busPosition.X), (float)((int)this.busPosition.Y)) + this.pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle?(this.pamSource), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 4f) / 10000f);
			}
		}

		// Token: 0x04001F77 RID: 8055
		public const int busDefaultXTile = 21;

		// Token: 0x04001F78 RID: 8056
		public const int busDefaultYTile = 6;

		// Token: 0x04001F7A RID: 8058
		private TemporaryAnimatedSprite minecartSteam;

		// Token: 0x04001F7B RID: 8059
		private TemporaryAnimatedSprite busDoor;

		// Token: 0x04001F7C RID: 8060
		[XmlIgnore]
		public Vector2 busPosition;

		// Token: 0x04001F7D RID: 8061
		[XmlIgnore]
		public Vector2 busMotion;

		// Token: 0x04001F7E RID: 8062
		[XmlIgnore]
		public bool drivingOff;

		// Token: 0x04001F7F RID: 8063
		[XmlIgnore]
		public bool drivingBack;

		// Token: 0x04001F80 RID: 8064
		[XmlIgnore]
		public bool leaving;

		// Token: 0x04001F81 RID: 8065
		private int forceWarpTimer;

		// Token: 0x04001F82 RID: 8066
		private Microsoft.Xna.Framework.Rectangle busSource = new Microsoft.Xna.Framework.Rectangle(288, 1247, 128, 64);

		// Token: 0x04001F83 RID: 8067
		private Microsoft.Xna.Framework.Rectangle pamSource = new Microsoft.Xna.Framework.Rectangle(384, 1311, 15, 19);

		// Token: 0x04001F84 RID: 8068
		private Vector2 pamOffset = new Vector2(0f, 29f);
	}
}
