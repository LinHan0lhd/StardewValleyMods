using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x0200022D RID: 557
	public class SquidKid : Monster
	{
		// Token: 0x060024C0 RID: 9408 RVA: 0x00192140 File Offset: 0x00190340
		public SquidKid()
		{
		}

		// Token: 0x060024C1 RID: 9409 RVA: 0x00192160 File Offset: 0x00190360
		public SquidKid(Vector2 position) : base("Squid Kid", position)
		{
			this.Sprite.SpriteHeight = 16;
			base.IsWalkingTowardPlayer = false;
			this.Sprite.UpdateSourceRect();
			base.HideShadow = true;
		}

		// Token: 0x060024C2 RID: 9410 RVA: 0x001921B8 File Offset: 0x001903B8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.fireballEvent, "fireballEvent").AddField(this.hurtAnimationEvent, "hurtAnimationEvent");
			this.fireballEvent.onEvent += delegate()
			{
				if (!Game1.IsMasterGame)
				{
					this.fireballFired();
				}
			};
			this.hurtAnimationEvent.onEvent += delegate()
			{
				this.Sprite.currentFrame = this.Sprite.currentFrame - this.Sprite.currentFrame % 4 + 3;
			};
		}

		// Token: 0x060024C3 RID: 9411 RVA: 0x00192220 File Offset: 0x00190420
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Squid Kid");
		}

		// Token: 0x060024C4 RID: 9412 RVA: 0x00192234 File Offset: 0x00190434
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
				base.setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				this.hurtAnimationEvent.Fire();
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x060024C5 RID: 9413 RVA: 0x001922D3 File Offset: 0x001904D3
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x060024C6 RID: 9414 RVA: 0x001922D8 File Offset: 0x001904D8
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

		// Token: 0x060024C7 RID: 9415 RVA: 0x00192500 File Offset: 0x00190700
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(21 + this.yOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
		}

		// Token: 0x060024C8 RID: 9416 RVA: 0x00192658 File Offset: 0x00190858
		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * 6.283185307179586) * 15.0);
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

		// Token: 0x060024C9 RID: 9417 RVA: 0x00192714 File Offset: 0x00190914
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.isMoving())
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.AnimateUp(time, 0, "");
					break;
				case 1:
					this.Sprite.AnimateRight(time, 0, "");
					break;
				case 2:
					this.Sprite.AnimateDown(time, 0, "");
					break;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					break;
				}
			}
			base.faceGeneralDirection(base.Player.Position, 0, false);
		}

		// Token: 0x060024CA RID: 9418 RVA: 0x001927AC File Offset: 0x001909AC
		private Vector2 fireballFired()
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.currentFrame = 3;
				return Vector2.Zero;
			case 1:
				this.Sprite.currentFrame = 7;
				return new Vector2(64f, 0f);
			case 2:
				this.Sprite.currentFrame = 11;
				return new Vector2(0f, 32f);
			case 3:
				this.Sprite.currentFrame = 15;
				return new Vector2(-32f, 0f);
			default:
				return Vector2.Zero;
			}
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x00192845 File Offset: 0x00190A45
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			this.fireballEvent.Poll();
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x0019285C File Offset: 0x00190A5C
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			base.faceGeneralDirection(base.Player.Position, 0, false);
			this.lastFireball = Math.Max(0f, this.lastFireball - (float)time.ElapsedGameTime.Milliseconds);
			if (this.isHardModeMonster.Value)
			{
				if ((this.numFireballsLeft > 0 || this.withinPlayerThreshold()) && this.lastFireball <= 0f)
				{
					if (this.lastFireball <= 0f && this.numFireballsLeft <= 0)
					{
						this.numFireballsLeft = 4;
						this.firingTimer = 0f;
					}
					this.firingTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (this.firingTimer <= 0f && this.numFireballsLeft > 0)
					{
						Rectangle playerBounds = base.Player.GetBoundingBox();
						this.numFireballsLeft--;
						base.IsWalkingTowardPlayer = false;
						this.Halt();
						this.fireballEvent.Fire();
						this.fireballFired();
						this.Sprite.UpdateSourceRect();
						Vector2 standingPixel = base.getStandingPosition();
						Vector2 trajectory = Utility.getVelocityTowardPoint(standingPixel, new Vector2((float)playerBounds.X, (float)playerBounds.Y) + new Vector2((float)Game1.random.Next(-128, 128)), 8f);
						BasicProjectile projectile = new BasicProjectile(15, 10, 2, 4, 0f, trajectory.X, trajectory.Y, standingPixel - new Vector2(32f, 0f), null, null, null, true, false, base.currentLocation, this, null, null);
						projectile.height.Value = 48f;
						base.currentLocation.projectiles.Add(projectile);
						base.currentLocation.playSound("fireball", null, null, SoundContext.Default);
						this.firingTimer = 400f;
						if (this.numFireballsLeft <= 0)
						{
							this.lastFireball = (float)Game1.random.Next(3000, 6500);
							return;
						}
					}
				}
			}
			else
			{
				if (this.withinPlayerThreshold() && this.lastFireball == 0f && Game1.random.NextDouble() < 0.01)
				{
					base.IsWalkingTowardPlayer = false;
					this.Halt();
					this.fireballEvent.Fire();
					this.fireballFired();
					this.Sprite.UpdateSourceRect();
					Point standingPixel2 = base.StandingPixel;
					Vector2 trajectory2 = Utility.getVelocityTowardPlayer(standingPixel2, 8f, base.Player);
					BasicProjectile projectile2 = new BasicProjectile(15, 10, 3, 4, 0f, trajectory2.X, trajectory2.Y, new Vector2((float)(standingPixel2.X - 32), (float)standingPixel2.Y), null, null, null, true, false, base.currentLocation, this, null, null);
					projectile2.height.Value = 48f;
					base.currentLocation.projectiles.Add(projectile2);
					base.currentLocation.playSound("fireball", null, null, SoundContext.Default);
					this.lastFireball = (float)Game1.random.Next(1200, 3500);
					return;
				}
				if (this.lastFireball != 0f && Game1.random.NextDouble() < 0.02)
				{
					this.Halt();
					if (this.withinPlayerThreshold())
					{
						base.Slipperiness = 8;
						Point standingTile = base.StandingPixel;
						base.setTrajectory((int)Utility.getVelocityTowardPlayer(standingTile, 8f, base.Player).X, (int)(-(int)Utility.getVelocityTowardPlayer(standingTile, 8f, base.Player).Y));
					}
				}
			}
		}

		// Token: 0x040015B2 RID: 5554
		[XmlIgnore]
		public float lastFireball;

		// Token: 0x040015B3 RID: 5555
		[XmlIgnore]
		public new int yOffset;

		// Token: 0x040015B4 RID: 5556
		private readonly NetEvent0 fireballEvent = new NetEvent0(false);

		// Token: 0x040015B5 RID: 5557
		private readonly NetEvent0 hurtAnimationEvent = new NetEvent0(false);

		// Token: 0x040015B6 RID: 5558
		private int numFireballsLeft;

		// Token: 0x040015B7 RID: 5559
		private float firingTimer;
	}
}
