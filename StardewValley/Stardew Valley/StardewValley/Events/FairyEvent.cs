using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events
{
	// Token: 0x02000325 RID: 805
	public class FairyEvent : BaseFarmEvent
	{
		// Token: 0x0600348E RID: 13454 RVA: 0x0029D6F4 File Offset: 0x0029B8F4
		public override bool setUp()
		{
			this.lightSourceId = this.GenerateLightSourceId();
			this.f = Game1.getFarm();
			if (this.f.IsRainingHere())
			{
				return true;
			}
			this.targetCrop = this.ChooseCrop();
			if (this.targetCrop == Vector2.Zero)
			{
				return true;
			}
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.fairyPosition, 1f, Color.Black, LightSource.LightContext.None, 0L, null));
			Game1.currentLocation = this.f;
			this.f.resetForPlayerEntry();
			Game1.fadeClear();
			Game1.nonWarpFade = true;
			Game1.timeOfDay = 2400;
			Game1.displayHUD = false;
			Game1.freezeControls = true;
			Game1.viewportFreeze = true;
			Game1.displayFarmer = false;
			Game1.viewport.X = Math.Max(0, Math.Min(this.f.map.DisplayWidth - Game1.viewport.Width, (int)this.targetCrop.X * 64 - Game1.viewport.Width / 2));
			Game1.viewport.Y = Math.Max(0, Math.Min(this.f.map.DisplayHeight - Game1.viewport.Height, (int)this.targetCrop.Y * 64 - Game1.viewport.Height / 2));
			this.fairyPosition = new Vector2((float)(Game1.viewport.X + Game1.viewport.Width + 128), this.targetCrop.Y * 64f - 64f);
			Game1.changeMusicTrack("nightTime", false, MusicContext.Default);
			return false;
		}

		// Token: 0x0600348F RID: 13455 RVA: 0x0029D8A4 File Offset: 0x0029BAA4
		public override bool tickUpdate(GameTime time)
		{
			if (this.terminate)
			{
				return true;
			}
			Game1.UpdateGameClock(time);
			this.f.UpdateWhenCurrentLocation(time);
			this.f.updateEvenIfFarmerIsntHere(time, false);
			Game1.UpdateOther(time);
			Utility.repositionLightSource(this.lightSourceId, this.fairyPosition + new Vector2(32f, 32f));
			if (this.animationLoopsDone < 1)
			{
				this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
			if (this.fairyPosition.X > this.targetCrop.X * 64f + 32f)
			{
				if (this.timerSinceFade < 2000)
				{
					return false;
				}
				this.fairyPosition.X = this.fairyPosition.X - (float)time.ElapsedGameTime.Milliseconds * 0.1f;
				this.fairyPosition.Y = this.fairyPosition.Y + (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) * 1f;
				int num = this.fairyFrame;
				if (time.TotalGameTime.Milliseconds % 500 > 250)
				{
					this.fairyFrame = 1;
				}
				else
				{
					this.fairyFrame = 0;
				}
				if (num != this.fairyFrame && this.fairyFrame == 1)
				{
					Game1.playSound("batFlap", null);
					this.f.temporarySprites.Add(new TemporaryAnimatedSprite(11, this.fairyPosition + new Vector2(32f, 0f), Color.Purple, 8, false, 100f, 0, -1, -1f, -1, 0));
				}
				if (this.fairyPosition.X <= this.targetCrop.X * 64f + 32f)
				{
					this.fairyFrame = 1;
				}
			}
			else if (this.animationLoopsDone < 4)
			{
				this.fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
				if (this.fairyAnimationTimer > 250)
				{
					this.fairyAnimationTimer = 0;
					if (!this.animateLeft)
					{
						this.fairyFrame++;
						if (this.fairyFrame == 3)
						{
							this.animateLeft = true;
							this.f.temporarySprites.Add(new TemporaryAnimatedSprite(10, this.fairyPosition + new Vector2(-16f, 64f), Color.LightPink, 8, false, 100f, 0, -1, -1f, -1, 0));
							Game1.playSound("yoba", null);
							TerrainFeature terrainFeature;
							if (this.f.terrainFeatures.TryGetValue(this.targetCrop, out terrainFeature))
							{
								HoeDirt dirt = terrainFeature as HoeDirt;
								if (dirt != null)
								{
									dirt.crop.currentPhase.Value = Math.Min(dirt.crop.currentPhase.Value + 1, dirt.crop.phaseDays.Count - 1);
								}
							}
						}
					}
					else
					{
						this.fairyFrame--;
						if (this.fairyFrame == 1)
						{
							this.animateLeft = false;
							this.animationLoopsDone++;
							if (this.animationLoopsDone >= 4)
							{
								for (int i = 0; i < 10; i++)
								{
									DelayedAction.playSoundAfterDelay("batFlap", 4000 + 500 * i, null, null, -1, false);
								}
							}
						}
					}
				}
			}
			else
			{
				this.fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
				if (time.TotalGameTime.Milliseconds % 500 > 250)
				{
					this.fairyFrame = 1;
				}
				else
				{
					this.fairyFrame = 0;
				}
				if (this.fairyAnimationTimer > 2000 && this.fairyPosition.Y > -999999f)
				{
					this.fairyPosition.X = this.fairyPosition.X + (float)Math.Cos((double)time.TotalGameTime.Milliseconds * 3.141592653589793 / 256.0) * 2f;
					this.fairyPosition.Y = this.fairyPosition.Y - (float)time.ElapsedGameTime.Milliseconds * 0.2f;
				}
				if (this.fairyPosition.Y < (float)(Game1.viewport.Y - 128) || float.IsNaN(this.fairyPosition.Y))
				{
					if (!Game1.fadeToBlack && this.fairyPosition.Y != -999999f)
					{
						Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterLastFade), 0.02f);
						Game1.changeMusicTrack("none", false, MusicContext.Default);
						this.timerSinceFade = 0;
						this.fairyPosition.Y = -999999f;
					}
					this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
				}
			}
			return false;
		}

		// Token: 0x06003490 RID: 13456 RVA: 0x0029DD97 File Offset: 0x0029BF97
		public void afterLastFade()
		{
			this.terminate = true;
			Game1.globalFadeToClear(null, 0.02f);
		}

		// Token: 0x06003491 RID: 13457 RVA: 0x0029DDAC File Offset: 0x0029BFAC
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.fairyPosition), new Rectangle?(new Rectangle(16 + this.fairyFrame * 16, 592, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}

		// Token: 0x06003492 RID: 13458 RVA: 0x0029DE10 File Offset: 0x0029C010
		public override void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			int x = (int)this.targetCrop.X - 2;
			while ((float)x <= this.targetCrop.X + 2f)
			{
				int y = (int)this.targetCrop.Y - 2;
				while ((float)y <= this.targetCrop.Y + 2f)
				{
					Vector2 v = new Vector2((float)x, (float)y);
					TerrainFeature terrainFeature;
					if (this.f.terrainFeatures.TryGetValue(v, out terrainFeature))
					{
						HoeDirt dirt = terrainFeature as HoeDirt;
						if (dirt != null && dirt.crop != null)
						{
							dirt.crop.growCompletely();
						}
					}
					y++;
				}
				x++;
			}
		}

		// Token: 0x06003493 RID: 13459 RVA: 0x0029DEC0 File Offset: 0x0029C0C0
		protected Vector2 ChooseCrop()
		{
			Vector2[] validCropPositions = (from p in this.f.terrainFeatures.Pairs.Where(delegate(KeyValuePair<Vector2, TerrainFeature> p)
			{
				HoeDirt dirt = p.Value as HoeDirt;
				return dirt != null && dirt.crop != null && !dirt.crop.dead.Value && !dirt.crop.isWildSeedCrop() && dirt.crop.currentPhase.Value < dirt.crop.phaseDays.Count - 1;
			})
			orderby p.Key.X, p.Key.Y
			select p.Key).ToArray<Vector2>();
			return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0).ChooseFrom(validCropPositions);
		}

		// Token: 0x04002246 RID: 8774
		public string lightSourceId;

		// Token: 0x04002247 RID: 8775
		private Vector2 fairyPosition;

		// Token: 0x04002248 RID: 8776
		private Vector2 targetCrop;

		// Token: 0x04002249 RID: 8777
		private Farm f;

		// Token: 0x0400224A RID: 8778
		private int fairyFrame;

		// Token: 0x0400224B RID: 8779
		private int fairyAnimationTimer;

		// Token: 0x0400224C RID: 8780
		private int animationLoopsDone;

		// Token: 0x0400224D RID: 8781
		private int timerSinceFade;

		// Token: 0x0400224E RID: 8782
		private bool animateLeft;

		// Token: 0x0400224F RID: 8783
		private bool terminate;
	}
}
