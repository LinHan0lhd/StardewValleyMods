using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Minigames
{
	// Token: 0x02000237 RID: 567
	public class FishingGame : IMinigame
	{
		// Token: 0x06002596 RID: 9622 RVA: 0x001A3ABC File Offset: 0x001A1CBC
		public FishingGame()
		{
			Tool fishingRod = ItemRegistry.Create<Tool>("(T)BambooPole", 1, 0, false);
			fishingRod.AttachmentSlotsCount = 2;
			fishingRod.attachments[0] = ItemRegistry.Create<Object>("(O)690", 99, 0, false);
			fishingRod.attachments[1] = ItemRegistry.Create<Object>("(O)687", 1, 0, false);
			this.content = Game1.content.CreateTemporary();
			this.location = new GameLocation("Maps\\FishingGame", "fishingGame");
			this.location.isStructure.Value = true;
			this.location.uniqueName.Value = "fishingGame" + Game1.player.UniqueMultiplayerID.ToString();
			this.location.currentEvent = Game1.currentLocation.currentEvent;
			Game1.player.CurrentToolIndex = 0;
			Game1.player.TemporaryItem = fishingRod;
			Game1.player.UsingTool = false;
			Game1.player.CurrentToolIndex = 0;
			Game1.globalFadeToClear(null, 0.01f);
			this.location.Map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.player.Position = new Vector2(14f, 7f) * 64f;
			Game1.player.currentLocation = this.location;
			this.originalLocation = Game1.currentLocation;
			Game1.currentLocation = this.location;
			this.changeScreenSize();
			this.gameEndTimer = 100000;
			this.showResultsTimer = -1;
			Game1.player.faceDirection(3);
			Game1.player.Halt();
		}

		// Token: 0x06002597 RID: 9623 RVA: 0x001A3C5B File Offset: 0x001A1E5B
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002598 RID: 9624 RVA: 0x001A3C68 File Offset: 0x001A1E68
		public bool tick(GameTime time)
		{
			Rumble.update((float)time.ElapsedGameTime.Milliseconds);
			Game1.player.Stamina = (float)Game1.player.MaxStamina;
			if (Game1.activeClickableMenu != null)
			{
				Game1.updateActiveMenu(time);
			}
			if (this.timerToStart > 0)
			{
				Game1.player.faceDirection(3);
				this.timerToStart -= time.ElapsedGameTime.Milliseconds;
				if (this.timerToStart <= 0)
				{
					Game1.playSound("whistle", null);
				}
			}
			else if (this.showResultsTimer >= 0)
			{
				int num = this.showResultsTimer;
				this.showResultsTimer -= time.ElapsedGameTime.Milliseconds;
				if (num > 11000 && this.showResultsTimer <= 11000)
				{
					Game1.playSound("smallSelect", null);
				}
				if (num > 9000 && this.showResultsTimer <= 9000)
				{
					Game1.playSound("smallSelect", null);
				}
				if (num > 7000 && this.showResultsTimer <= 7000)
				{
					if (this.perfections > 0)
					{
						this.score += this.perfections * 10;
						this.perfectionBonus = this.perfections * 10;
						if (this.fishCaught >= 3 && this.perfections >= 3)
						{
							this.perfectionBonus += this.score;
							this.score *= 2;
						}
						Game1.playSound("newArtifact", null);
					}
					else
					{
						Game1.playSound("smallSelect", null);
					}
				}
				if (num > 5000 && this.showResultsTimer <= 5000)
				{
					if (this.score >= 10)
					{
						Game1.playSound("reward", null);
						this.starTokensWon = (this.score + 5) / 10 * 6;
						this.starTokensWon *= 2;
						Game1.player.festivalScore += this.starTokensWon;
					}
					else
					{
						Game1.playSound("fishEscape", null);
					}
				}
				if (this.showResultsTimer <= 0)
				{
					Game1.globalFadeToClear(null, 0.02f);
					return true;
				}
			}
			else if (!this.gameDone)
			{
				this.gameEndTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.gameEndTimer <= 0 && Game1.activeClickableMenu == null && (!Game1.player.UsingTool || (Game1.player.CurrentTool as FishingRod).isFishing))
				{
					(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player, false);
					(Game1.player.CurrentTool as FishingRod).tickUpdate(time, Game1.player);
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.playSound("whistle", null);
					this.gameEndTimer = 1000;
					this.gameDone = true;
				}
			}
			else if (this.gameDone && this.gameEndTimer > 0)
			{
				this.gameEndTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.gameEndTimer <= 0)
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.gameDoneAfterFade), 0.01f);
					Game1.exitActiveMenu();
					Game1.player.forceCanMove();
				}
			}
			return this.exit;
		}

		// Token: 0x06002599 RID: 9625 RVA: 0x001A3FE4 File Offset: 0x001A21E4
		public void gameDoneAfterFade()
		{
			this.showResultsTimer = 11100;
			Game1.player.canMove = false;
			Game1.player.Position = ((Game1.year % 2 == 0) ? (new Vector2(36f, 68f) * 64f) : (new Vector2(24f, 71f) * 64f));
			Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(Game1.player.TilePoint.X * 64, Game1.player.TilePoint.Y * 64, 64, 64));
			Game1.player.currentLocation = this.originalLocation;
			Game1.currentLocation = this.originalLocation;
			Game1.player.faceDirection(2);
			Utility.killAllStaticLoopingSoundCues();
			if (FishingRod.reelSound != null && FishingRod.reelSound.IsPlaying)
			{
				FishingRod.reelSound.Stop(AudioStopOptions.Immediate);
			}
		}

		// Token: 0x0600259A RID: 9626 RVA: 0x001A40D3 File Offset: 0x001A22D3
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				return;
			}
			this.handleCastInput();
		}

		// Token: 0x0600259B RID: 9627 RVA: 0x001A40E3 File Offset: 0x001A22E3
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x0600259C RID: 9628 RVA: 0x001A40E5 File Offset: 0x001A22E5
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600259D RID: 9629 RVA: 0x001A40E7 File Offset: 0x001A22E7
		public void releaseLeftClick(int x, int y)
		{
			this.handleCastInputReleased();
		}

		// Token: 0x0600259E RID: 9630 RVA: 0x001A40EF File Offset: 0x001A22EF
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600259F RID: 9631 RVA: 0x001A40F4 File Offset: 0x001A22F4
		public void receiveKeyPress(Keys k)
		{
			if (!this.gameDone)
			{
				if (Game1.player.movementDirections.Count < 2 && !Game1.player.UsingTool && this.timerToStart <= 0)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
					{
						Game1.player.setMoving(1);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
					{
						Game1.player.setMoving(2);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
					{
						Game1.player.setMoving(4);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
					{
						Game1.player.setMoving(8);
					}
				}
				if (Game1.options.doesInputListContain(Game1.options.useToolButton, k))
				{
					this.handleCastInput();
				}
				if (k == Keys.Escape)
				{
					if (this.gameEndTimer <= 0 && !this.gameDone)
					{
						this.EmergencyCancel();
					}
					else if (Game1.activeClickableMenu == null)
					{
						this.gameEndTimer = 1;
					}
					else
					{
						BobberBar bobberBar = Game1.activeClickableMenu as BobberBar;
						if (bobberBar != null)
						{
							bobberBar.receiveKeyPress(k);
						}
					}
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.runButton, k) || Game1.isGamePadThumbstickInMotion(0.2))
			{
				Game1.player.setRunning(true, false);
			}
		}

		// Token: 0x060025A0 RID: 9632 RVA: 0x001A4258 File Offset: 0x001A2458
		public void receiveKeyRelease(Keys k)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				Game1.player.setMoving(33);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
			{
				Game1.player.setMoving(34);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				Game1.player.setMoving(36);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
			{
				Game1.player.setMoving(40);
			}
			if (Game1.options.doesInputListContain(Game1.options.runButton, k))
			{
				Game1.player.setRunning(false, false);
			}
			if (Game1.player.movementDirections.Count == 0 && !Game1.player.UsingTool)
			{
				Game1.player.Halt();
			}
			if (Game1.options.doesInputListContain(Game1.options.useToolButton, k))
			{
				this.handleCastInputReleased();
			}
		}

		// Token: 0x060025A1 RID: 9633 RVA: 0x001A4358 File Offset: 0x001A2558
		public virtual void EmergencyCancel()
		{
			Game1.player.Halt();
			Game1.player.isEating = false;
			Game1.player.CanMove = true;
			Game1.player.UsingTool = false;
			Game1.player.usingSlingshot = false;
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			FishingRod rod = Game1.player.CurrentTool as FishingRod;
			if (rod != null)
			{
				rod.resetState();
			}
		}

		// Token: 0x060025A2 RID: 9634 RVA: 0x001A43C4 File Offset: 0x001A25C4
		private void handleCastInput()
		{
			if (this.timerToStart <= 0 && this.showResultsTimer < 0 && !this.gameDone && Game1.activeClickableMenu == null && !(Game1.player.CurrentTool as FishingRod).hit && !(Game1.player.CurrentTool as FishingRod).pullingOutOfWater && !(Game1.player.CurrentTool as FishingRod).isCasting && !(Game1.player.CurrentTool as FishingRod).fishCaught && !(Game1.player.CurrentTool as FishingRod).castedButBobberStillInAir)
			{
				Game1.player.lastClick = Vector2.Zero;
				Game1.player.Halt();
				Game1.pressUseToolButton();
				return;
			}
			if (this.showResultsTimer > 11000)
			{
				this.showResultsTimer = 11001;
				return;
			}
			if (this.showResultsTimer > 9000)
			{
				this.showResultsTimer = 9001;
				return;
			}
			if (this.showResultsTimer > 7000)
			{
				this.showResultsTimer = 7001;
				return;
			}
			if (this.showResultsTimer > 5000)
			{
				this.showResultsTimer = 5001;
				return;
			}
			if (this.showResultsTimer < 5000 && this.showResultsTimer > 1000)
			{
				this.showResultsTimer = 1500;
				Game1.playSound("smallSelect", null);
			}
		}

		// Token: 0x060025A3 RID: 9635 RVA: 0x001A452C File Offset: 0x001A272C
		private void handleCastInputReleased()
		{
			if (this.showResultsTimer < 0 && Game1.player.CurrentTool != null && !(Game1.player.CurrentTool as FishingRod).isCasting && Game1.activeClickableMenu == null && Game1.player.CurrentTool.onRelease(this.location, 0, 0, Game1.player))
			{
				Game1.player.Halt();
			}
		}

		// Token: 0x060025A4 RID: 9636 RVA: 0x001A4594 File Offset: 0x001A2794
		public void draw(SpriteBatch b)
		{
			if (this.showResultsTimer < 0)
			{
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				Game1.mapDisplayDevice.BeginScene(b);
				this.location.Map.RequireLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, 0f);
				this.location.drawWater(b);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(32f, 24f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f - ((Game1.player.running || Game1.player.UsingTool) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[Game1.player.FarmerSprite.CurrentFrame]) * 0.8f) : 0f), SpriteEffects.None, Math.Max(0f, (float)Game1.player.StandingPixel.Y / 10000f + 0.00011f) - 1E-07f);
				this.location.Map.RequireLayer("Buildings").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, 0f);
				this.location.draw(b);
				b.End();
				b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				Game1.player.draw(b);
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				this.location.Map.RequireLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, 0f);
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.draw(b);
				}
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", Utility.getMinutesSecondsStringFromMilliseconds(Math.Max(0, this.gameEndTimer))), new Vector2(16f, 64f), Color.White);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), new Vector2(16f, 32f), Color.White);
				b.End();
				return;
			}
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			Vector2 position = new Vector2((float)(Game1.viewport.Width / 2 - 128), (float)(Game1.viewport.Height / 2 - 64));
			if (this.showResultsTimer <= 11000)
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), Game1.textColor, (this.showResultsTimer <= 7000 && this.perfectionBonus > 0) ? Color.Lime : Color.White, position);
			}
			if (this.showResultsTimer <= 9000)
			{
				position.Y += 48f;
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12010", this.fishCaught), Game1.textColor, Color.White, position);
			}
			if (this.showResultsTimer <= 7000)
			{
				position.Y += 48f;
				if (this.perfectionBonus > 1)
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12011", this.perfectionBonus), Game1.textColor, Color.Yellow, position);
				}
				else
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12012"), Game1.textColor, Color.Red, position);
				}
			}
			if (this.showResultsTimer <= 5000)
			{
				position.Y += 64f;
				if (this.starTokensWon > 0)
				{
					float fade = Math.Min(1f, (float)(this.showResultsTimer - 2000) / 4000f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", this.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", this.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", this.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", this.starTokensWon), Game1.textColor, Color.SkyBlue, position, 0f, 1f, 1f);
				}
				else
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12021"), Game1.textColor, Color.Red, position);
				}
			}
			if (this.showResultsTimer <= 1000)
			{
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * (1f - (float)this.showResultsTimer / 1000f));
			}
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
			b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, 29f), 0f, 1f, 1f, false);
			b.End();
		}

		// Token: 0x060025A5 RID: 9637 RVA: 0x001A4D28 File Offset: 0x001A2F28
		public static void startMe()
		{
			Game1.currentMinigame = new FishingGame();
		}

		// Token: 0x060025A6 RID: 9638 RVA: 0x001A4D34 File Offset: 0x001A2F34
		public void changeScreenSize()
		{
			Game1.viewport.X = this.location.Map.Layers[0].LayerWidth * 64 / 2 - (int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel);
			Game1.viewport.Y = this.location.Map.Layers[0].LayerHeight * 64 / 2 - (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel);
		}

		// Token: 0x060025A7 RID: 9639 RVA: 0x001A4DD4 File Offset: 0x001A2FD4
		public void unload()
		{
			FishingRod fishingRod = (FishingRod)Game1.player.CurrentTool;
			fishingRod.castingEndFunction(Game1.player);
			fishingRod.doneFishing(Game1.player, false);
			Game1.player.TemporaryItem = null;
			Game1.player.currentLocation = Game1.currentLocation;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
			this.content.Unload();
			this.content.Dispose();
			this.content = null;
		}

		// Token: 0x060025A8 RID: 9640 RVA: 0x001A4E5C File Offset: 0x001A305C
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x060025A9 RID: 9641 RVA: 0x001A4E5E File Offset: 0x001A305E
		public string minigameId()
		{
			return "FishingGame";
		}

		// Token: 0x060025AA RID: 9642 RVA: 0x001A4E65 File Offset: 0x001A3065
		public bool doMainGameUpdates()
		{
			return true;
		}

		// Token: 0x060025AB RID: 9643 RVA: 0x001A4E68 File Offset: 0x001A3068
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x040016E4 RID: 5860
		private GameLocation location;

		// Token: 0x040016E5 RID: 5861
		private LocalizedContentManager content;

		// Token: 0x040016E6 RID: 5862
		private int timerToStart = 1000;

		// Token: 0x040016E7 RID: 5863
		private int gameEndTimer;

		// Token: 0x040016E8 RID: 5864
		private int showResultsTimer;

		// Token: 0x040016E9 RID: 5865
		public bool exit;

		// Token: 0x040016EA RID: 5866
		public bool gameDone;

		// Token: 0x040016EB RID: 5867
		public int score;

		// Token: 0x040016EC RID: 5868
		public int fishCaught;

		// Token: 0x040016ED RID: 5869
		public int starTokensWon;

		// Token: 0x040016EE RID: 5870
		public int perfections;

		// Token: 0x040016EF RID: 5871
		public int perfectionBonus;

		// Token: 0x040016F0 RID: 5872
		public GameLocation originalLocation;
	}
}
