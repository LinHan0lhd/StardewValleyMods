using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewValley.Monsters
{
	// Token: 0x02000217 RID: 535
	public class DwarvishSentry : Monster
	{
		// Token: 0x06002371 RID: 9073 RVA: 0x001804C0 File Offset: 0x0017E6C0
		public DwarvishSentry()
		{
		}

		// Token: 0x06002372 RID: 9074 RVA: 0x001804C8 File Offset: 0x0017E6C8
		public DwarvishSentry(Vector2 position) : base("Dwarvish Sentry", position)
		{
			this.Sprite.SpriteHeight = 16;
			base.IsWalkingTowardPlayer = false;
			this.Sprite.UpdateSourceRect();
			base.HideShadow = true;
			this.isGlider.Value = true;
			base.Slipperiness = 1;
			this.pauseTimer = 10000f;
			DelayedAction.playSoundAfterDelay("DwarvishSentry", 500, null, null, -1, false);
		}

		// Token: 0x06002373 RID: 9075 RVA: 0x00180540 File Offset: 0x0017E740
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Dwarvish Sentry");
		}

		// Token: 0x06002374 RID: 9076 RVA: 0x00180554 File Offset: 0x0017E754
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
				GameLocation currentLocation = base.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.playSound("clank", null, null, SoundContext.Default);
				}
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002375 RID: 9077 RVA: 0x001805E8 File Offset: 0x0017E7E8
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 64, 16, 16), 70f, 7, 0, base.Position + new Vector2(0f, -32f), false, false)
			{
				scale = 4f
			});
			base.currentLocation.localSound("fireball", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2((float)(-16 + Game1.random.Next(64)), (float)(Game1.random.Next(64) - 32)), false, Game1.random.NextBool())
			{
				delayBeforeAnimationStart = 100
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2((float)(-16 + Game1.random.Next(64)), (float)(Game1.random.Next(64) - 32)), false, Game1.random.NextBool())
			{
				delayBeforeAnimationStart = 200
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2((float)(-16 + Game1.random.Next(64)), (float)(Game1.random.Next(64) - 32)), false, Game1.random.NextBool())
			{
				delayBeforeAnimationStart = 300
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2((float)(-16 + Game1.random.Next(64)), (float)(Game1.random.Next(64) - 32)), false, Game1.random.NextBool())
			{
				delayBeforeAnimationStart = 400
			});
		}

		// Token: 0x06002376 RID: 9078 RVA: 0x00180810 File Offset: 0x0017EA10
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, base.getLocalPosition(Game1.viewport) + new Vector2(50f, (float)(80 + this.yOffset)), new Rectangle?(new Rectangle(536 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 350.0 / 70.0) * 8, 1945, 8, 8)), Color.White * 0.75f, 0f, new Vector2(8f, 16f), 4f, SpriteEffects.FlipVertically, 0.99f - this.position.X / 10000f);
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(21 + this.yOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f - this.position.X / 10000f);
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(base.StandingPixel.Y - 1) / 10000f);
		}

		// Token: 0x06002377 RID: 9079 RVA: 0x00180A08 File Offset: 0x0017EC08
		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * 6.283185307179586) * 7.0);
			if (this.Sprite.currentFrame % 4 != 0 && Game1.random.NextDouble() < 0.1)
			{
				this.Sprite.currentFrame -= this.Sprite.currentFrame % 4;
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				this.Sprite.currentFrame++;
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x00180AC4 File Offset: 0x0017ECC4
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			base.faceGeneralDirection(base.Player.Position, 0, false);
			this.pauseTimer += (float)((int)time.ElapsedGameTime.TotalMilliseconds);
			if (this.pauseTimer < 10000f)
			{
				this.setTrajectory(Utility.getVelocityTowardPoint(base.Position, base.Player.Position, 1f) * new Vector2(1f, -1f));
				return;
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				this.pauseTimer = (float)Game1.random.Next(5000);
			}
		}

		// Token: 0x040014FB RID: 5371
		[XmlIgnore]
		public new int yOffset;

		// Token: 0x040014FC RID: 5372
		[XmlIgnore]
		public float pauseTimer;
	}
}
