using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewValley.Monsters
{
	// Token: 0x02000218 RID: 536
	public class Fly : Monster
	{
		// Token: 0x06002379 RID: 9081 RVA: 0x00180B76 File Offset: 0x0017ED76
		public Fly()
		{
		}

		// Token: 0x0600237A RID: 9082 RVA: 0x00180B89 File Offset: 0x0017ED89
		public Fly(Vector2 position) : this(position, false)
		{
		}

		// Token: 0x0600237B RID: 9083 RVA: 0x00180B94 File Offset: 0x0017ED94
		public Fly(Vector2 position, bool hard) : base("Fly", position)
		{
			base.Slipperiness = 24 + Game1.random.Next(-10, 10);
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			this.hard = hard;
			if (hard)
			{
				base.DamageToFarmer *= 2;
				base.MaxHealth *= 3;
				base.Health = base.MaxHealth;
			}
			base.HideShadow = true;
		}

		// Token: 0x0600237C RID: 9084 RVA: 0x00180C15 File Offset: 0x0017EE15
		public void setHard()
		{
			this.hard = true;
			if (this.hard)
			{
				base.DamageToFarmer = 12;
				base.MaxHealth = 66;
				base.Health = base.MaxHealth;
			}
		}

		// Token: 0x0600237D RID: 9085 RVA: 0x00180C42 File Offset: 0x0017EE42
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Fly");
			base.HideShadow = true;
			if (!onlyAppearance)
			{
				Fly.buzz = Game1.soundBank.GetCue("flybuzzing");
			}
		}

		// Token: 0x0600237E RID: 9086 RVA: 0x00180C74 File Offset: 0x0017EE74
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				this.wasHitCounter = 500;
				GameLocation currentLocation = base.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				}
				if (base.Health <= 0)
				{
					if (base.currentLocation != null)
					{
						base.currentLocation.playSound("monsterdead", null, null, SoundContext.Default);
						Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10, false, 100f, 0, -1, -1f, -1, 0)
						{
							interval = 70f
						}, base.currentLocation, 4, 64, 64);
					}
					ICue cue = Fly.buzz;
					if (cue != null)
					{
						cue.Stop(AudioStopOptions.AsAuthored);
					}
				}
			}
			this.addedSpeed = (float)Game1.random.Next(-1, 1);
			return actualDamage;
		}

		// Token: 0x0600237F RID: 9087 RVA: 0x00180DA8 File Offset: 0x0017EFA8
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				int boundsHeight = this.GetBoundingBox().Height;
				int standingY = base.StandingPixel.Y;
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 - 32)), new Rectangle?(this.Sprite.SourceRect), this.hard ? Color.Lime : Color.White, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f)));
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 - 32)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
				}
			}
		}

		// Token: 0x06002380 RID: 9088 RVA: 0x00180FDF File Offset: 0x0017F1DF
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (base.currentLocation != null && base.currentLocation.treatAsOutdoors.Value)
			{
				this.drawAboveAllLayers(b);
			}
		}

		// Token: 0x06002381 RID: 9089 RVA: 0x00181004 File Offset: 0x0017F204
		protected override void updateAnimation(GameTime time)
		{
			if ((Fly.buzz == null || !Fly.buzz.IsPlaying) && (base.currentLocation == null || base.currentLocation.Equals(Game1.currentLocation)))
			{
				Game1.playSound("flybuzzing", out Fly.buzz);
				Fly.buzz.SetVariable("Volume", 0f);
			}
			if ((double)Game1.fadeToBlackAlpha > 0.8 && Game1.fadeIn && Fly.buzz != null)
			{
				Fly.buzz.Stop(AudioStopOptions.AsAuthored);
			}
			else if (Fly.buzz != null)
			{
				Fly.buzz.SetVariable("Volume", Math.Max(0f, Fly.buzz.GetVariable("Volume") - 1f));
				float volume = Math.Max(0f, 100f - Vector2.Distance(base.Position, base.Player.Position) / 64f / 16f * 100f);
				if (volume > Fly.buzz.GetVariable("Volume"))
				{
					Fly.buzz.SetVariable("Volume", volume);
				}
			}
			if (this.wasHitCounter >= 0)
			{
				this.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			this.Sprite.Animate(time, (this.FacingDirection == 0) ? 8 : ((this.FacingDirection == 2) ? 0 : (this.FacingDirection * 4)), 4, 75f);
			if (this.spawningCounter >= 0)
			{
				this.spawningCounter -= time.ElapsedGameTime.Milliseconds;
				base.Scale = 1f - (float)this.spawningCounter / 1000f;
			}
			else if ((this.withinPlayerThreshold() || Utility.isOnScreen(base.Position, 256)) && this.invincibleCountdown <= 0)
			{
				this.faceDirection(0);
				Point monsterPixel = base.StandingPixel;
				Point standingPixel = base.Player.StandingPixel;
				float xSlope = (float)(-(float)(standingPixel.X - monsterPixel.X));
				float ySlope = (float)(standingPixel.Y - monsterPixel.Y);
				float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
				if (t < 64f)
				{
					this.xVelocity = Math.Max(-7f, Math.Min(7f, this.xVelocity * 1.1f));
					this.yVelocity = Math.Max(-7f, Math.Min(7f, this.yVelocity * 1.1f));
				}
				xSlope /= t;
				ySlope /= t;
				if (this.wasHitCounter <= 0)
				{
					this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.5707964f;
					if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextBool())
					{
						this.turningRight = true;
					}
					else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
					{
						this.turningRight = false;
					}
					if (this.turningRight)
					{
						this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					else
					{
						this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					this.rotation %= 6.2831855f;
					this.wasHitCounter = 5 + Game1.random.Next(-1, 2);
				}
				float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * 7f))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * 7f))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x06002382 RID: 9090 RVA: 0x001814C0 File Offset: 0x0017F6C0
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity))
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
		}

		// Token: 0x06002383 RID: 9091 RVA: 0x0018158E File Offset: 0x0017F78E
		public override void Removed()
		{
			base.Removed();
			ICue cue = Fly.buzz;
			if (cue == null)
			{
				return;
			}
			cue.Stop(AudioStopOptions.AsAuthored);
		}

		// Token: 0x040014FD RID: 5373
		public const float rotationIncrement = 0.049087387f;

		// Token: 0x040014FE RID: 5374
		public const int volumeTileRange = 16;

		// Token: 0x040014FF RID: 5375
		public const int spawnTime = 1000;

		// Token: 0x04001500 RID: 5376
		[XmlIgnore]
		public int spawningCounter = 1000;

		// Token: 0x04001501 RID: 5377
		[XmlIgnore]
		public int wasHitCounter;

		// Token: 0x04001502 RID: 5378
		[XmlIgnore]
		public float targetRotation;

		// Token: 0x04001503 RID: 5379
		public static ICue buzz;

		// Token: 0x04001504 RID: 5380
		[XmlIgnore]
		public bool turningRight;

		// Token: 0x04001505 RID: 5381
		public bool hard;
	}
}
