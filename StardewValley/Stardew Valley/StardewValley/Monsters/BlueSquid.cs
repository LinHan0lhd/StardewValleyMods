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
	// Token: 0x02000212 RID: 530
	public class BlueSquid : Monster
	{
		// Token: 0x06002334 RID: 9012 RVA: 0x0017CF16 File Offset: 0x0017B116
		public BlueSquid()
		{
		}

		// Token: 0x06002335 RID: 9013 RVA: 0x0017CF4C File Offset: 0x0017B14C
		public BlueSquid(Vector2 position) : base("Blue Squid", position)
		{
			this.Sprite.SpriteHeight = 24;
			this.Sprite.SpriteWidth = 24;
			base.IsWalkingTowardPlayer = true;
			this.reloadSprite(false);
			this.Sprite.UpdateSourceRect();
			base.HideShadow = true;
			this.slipperiness.Value = Game1.random.Next(6, 9);
			this.canMoveTimer = (float)Game1.random.Next(500);
			this.isHardModeMonster.Value = true;
		}

		// Token: 0x06002336 RID: 9014 RVA: 0x0017D008 File Offset: 0x0017B208
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.projectileIntroTimer, "projectileIntroTimer").AddField(this.projectileOutroTimer, "projectileOutroTimer").AddField(this.lastRotation, "lastRotation").AddField(this.nearFarmer, "nearFarmer");
			this.lastRotation.Interpolated(false, false);
			this.projectileIntroTimer.Interpolated(false, false);
			this.projectileOutroTimer.Interpolated(false, false);
		}

		// Token: 0x06002337 RID: 9015 RVA: 0x0017D08C File Offset: 0x0017B28C
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Blue Squid", 0, 24, 24);
		}

		// Token: 0x06002338 RID: 9016 RVA: 0x0017D0A4 File Offset: 0x0017B2A4
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
				this.projectileOutroTimer.Value = 0f;
				this.projectileIntroTimer.Value = 0f;
				this.shakeTimer = 250;
				base.setTrajectory(xTrajectory, yTrajectory);
				this.lastRotation.Value = (float)Math.Atan2((double)(-(double)this.yVelocity), (double)this.xVelocity) + 1.5707964f;
				DelayedAction.playSoundAfterDelay("squid_hit", 80, base.currentLocation, null, -1, false);
				base.currentLocation.playSound("slimeHit", null, null, SoundContext.Default);
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002339 RID: 9017 RVA: 0x0017D1AC File Offset: 0x0017B3AC
		protected override void sharedDeathAnimation()
		{
			base.currentLocation.localSound("slimedead", null, null, SoundContext.Default);
			if (this.Sprite.Texture.Height > this.Sprite.getHeight() * 4)
			{
				Point standingPixel = base.StandingPixel;
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 48, 16, 16), 8, standingPixel.X, standingPixel.Y, 6, base.TilePoint.Y, Color.White, 4f * this.scale.Value);
			}
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x0017D258 File Offset: 0x0017B458
		protected override void localDeathAnimation()
		{
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, base.Position, Color.HotPink * 0.86f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), Color.HotPink * 0.86f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 0,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -16f), Color.HotPink * 0.86f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 100,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), Color.HotPink * 0.86f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					interval = 70f,
					delayBeforeAnimationStart = 200,
					holdLastFrame = true,
					alphaFade = 0.01f
				}
			});
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x0017D448 File Offset: 0x0017B648
		public override Rectangle GetBoundingBox()
		{
			if (this.Sprite == null)
			{
				return Rectangle.Empty;
			}
			Vector2 position = base.Position;
			int width = base.GetSpriteWidthForPositioning() * 4 * 3 / 4;
			return new Rectangle((int)position.X, (int)position.Y + 16, width, 64);
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x0017D490 File Offset: 0x0017B690
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 96f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), Math.Min(4f, 4f + (float)this.squidYOffset / 20f), SpriteEffects.None, (float)(standingY - 32) / 10000f);
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(21 + this.squidYOffset)) + new Vector2((float)((this.shakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)((this.shakeTimer > 0) ? Game1.random.Next(-2, 3) : 0)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.lastRotation.Value, new Vector2(12f, 12f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		}

		// Token: 0x0600233D RID: 9021 RVA: 0x0017D634 File Offset: 0x0017B834
		protected override void updateAnimation(GameTime time)
		{
			if (this.Sprite.CurrentFrame != 2)
			{
				this.justThrust = false;
			}
			if (this.projectileIntroTimer.Value > 0f)
			{
				this.shakeTimer = 10;
				this.Sprite.CurrentFrame = 6;
				this.squidYOffset--;
				if (this.squidYOffset < 0)
				{
					this.squidYOffset = 0;
				}
			}
			else if (this.projectileOutroTimer.Value > 0f)
			{
				this.Sprite.CurrentFrame = 5;
				this.squidYOffset += 2;
			}
			else
			{
				this.squidYOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.TotalMilliseconds / 2000f) * 3.141592653589793 * 2.0) * 30.0);
				this.Sprite.currentFrame = Math.Abs(this.squidYOffset - 24) / 12;
				if (this.squidYOffset < 0)
				{
					this.Sprite.CurrentFrame = 2;
				}
			}
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x0600233E RID: 9022 RVA: 0x0017D74C File Offset: 0x0017B94C
		public override void noMovementProgressNearPlayerBehavior()
		{
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x0017D750 File Offset: 0x0017B950
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			this.nearFarmer.Value = (this.withinPlayerThreshold(10) || base.focusedOnFarmers);
			if (this.projectileIntroTimer.Value <= 0f && this.projectileOutroTimer.Value <= 0f)
			{
				if (Math.Abs(this.xVelocity) <= 1f && Math.Abs(this.yVelocity) <= 1f && this.nearFarmer.Value)
				{
					Utility.getVelocityTowardPoint(this.findPlayer().position.Value, this.position.Value, (float)Game1.random.Next(25, 50)).X *= -1f;
					if (this.canMoveTimer > 0f)
					{
						this.canMoveTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					}
					if (!this.justThrust && this.Sprite.CurrentFrame == 2 && this.canMoveTimer <= 0f)
					{
						this.justThrust = true;
						Vector2 traj = Utility.getVelocityTowardPoint(this.findPlayer().position.Value, this.position.Value + new Vector2((float)Game1.random.Next(-64, 64)), (float)Game1.random.Next(25, 50));
						traj.X *= -1f;
						this.setTrajectory(traj);
						this.lastRotation.Value = (float)Math.Atan2((double)(-(double)this.yVelocity), (double)this.xVelocity) + 1.5707964f;
						base.currentLocation.playSound("squid_move", null, null, SoundContext.Default);
						this.canMoveTimer = 500f;
					}
				}
				else if (!this.nearFarmer.Value)
				{
					this.lastRotation.Value = 0f;
				}
			}
			if ((Math.Abs(this.xVelocity) >= 10f || Math.Abs(this.yVelocity) >= 10f) && Game1.random.NextDouble() < 0.25)
			{
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(Game1.random.Choose(135, 140), 234, 5, 5), base.Position + new Vector2(32f, (float)(32 + Game1.random.Next(-8, 8))), false, 0.01f, Color.White)
					{
						interval = 9999f,
						holdLastFrame = true,
						alphaFade = 0.01f,
						motion = new Vector2(0f, -1f),
						xPeriodic = true,
						xPeriodicLoopTime = (float)Game1.random.Next(800, 1200),
						xPeriodicRange = (float)Game1.random.Next(8, 20),
						scale = 4f,
						drawAboveAlwaysFront = true
					}
				});
			}
			if (this.projectileIntroTimer.Value > 0f)
			{
				this.projectileIntroTimer.Value -= (float)time.ElapsedGameTime.TotalMilliseconds;
				this.shakeTimer = 10;
				if (Game1.random.NextDouble() < 0.25)
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(Game1.random.Choose(135, 140), 234, 5, 5), base.Position + new Vector2((float)(21 + Game1.random.Next(-21, 21)), (float)(this.squidYOffset / 2 + 32 + Game1.random.Next(-32, 32))), false, 0.01f, Color.White)
						{
							interval = 9999f,
							holdLastFrame = true,
							alphaFade = 0.01f,
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = (float)Game1.random.Next(800, 1200),
							xPeriodicRange = (float)Game1.random.Next(8, 20),
							scale = 4f,
							drawAboveAlwaysFront = true
						}
					});
				}
				if (this.projectileIntroTimer.Value < 0f)
				{
					this.projectileOutroTimer.Value = 500f;
					base.IsWalkingTowardPlayer = false;
					this.Halt();
					Point standingPixel = base.StandingPixel;
					Vector2 trajectory = Utility.getVelocityTowardPlayer(standingPixel, 8f, base.Player);
					DebuffingProjectile projectile = new DebuffingProjectile("27", 8, 3, 4, 0f, trajectory.X, trajectory.Y, Utility.PointToVector2(standingPixel) - new Vector2(32f, (float)(-(float)this.squidYOffset)), base.currentLocation, this, false, true);
					projectile.height.Value = 48f;
					base.currentLocation.projectiles.Add(projectile);
					base.currentLocation.playSound("debuffSpell", null, null, SoundContext.Default);
					this.nextFire = (float)Game1.random.Next(1200, 3500);
				}
			}
			else if (this.projectileOutroTimer.Value > 0f)
			{
				this.projectileOutroTimer.Value -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			this.nextFire = Math.Max(0f, this.nextFire - (float)time.ElapsedGameTime.Milliseconds);
			if (this.withinPlayerThreshold(6) && this.nextFire == 0f && this.projectileIntroTimer.Value <= 0f && Math.Abs(this.xVelocity) < 1f && Math.Abs(this.yVelocity) < 1f && Game1.random.NextDouble() < 0.003 && this.canMoveTimer <= 0f && base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Back", null) && !base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Buildings", null) && !base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Front", null))
			{
				this.projectileIntroTimer.Value = 1000f;
				this.lastRotation.Value = 0f;
				base.currentLocation.playSound("squid_bubble", null, null, SoundContext.Default);
			}
		}

		// Token: 0x040014E3 RID: 5347
		public float nextFire;

		// Token: 0x040014E4 RID: 5348
		public int squidYOffset;

		// Token: 0x040014E5 RID: 5349
		public float canMoveTimer;

		// Token: 0x040014E6 RID: 5350
		public NetFloat projectileIntroTimer = new NetFloat();

		// Token: 0x040014E7 RID: 5351
		public NetFloat projectileOutroTimer = new NetFloat();

		// Token: 0x040014E8 RID: 5352
		public NetBool nearFarmer = new NetBool();

		// Token: 0x040014E9 RID: 5353
		public NetFloat lastRotation = new NetFloat();

		// Token: 0x040014EA RID: 5354
		[XmlIgnore]
		public bool justThrust;
	}
}
