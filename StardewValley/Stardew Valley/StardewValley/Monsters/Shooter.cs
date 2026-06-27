using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x0200022A RID: 554
	public class Shooter : Monster
	{
		// Token: 0x0600249B RID: 9371 RVA: 0x001907F4 File Offset: 0x0018E9F4
		public Shooter()
		{
		}

		// Token: 0x0600249C RID: 9372 RVA: 0x00190890 File Offset: 0x0018EA90
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.shooting, "shooting").AddField(this.fireEvent, "fireEvent");
			this.fireEvent.onEvent += this.OnFire;
		}

		// Token: 0x0600249D RID: 9373 RVA: 0x001908E2 File Offset: 0x0018EAE2
		public override int GetBaseDifficultyLevel()
		{
			return 1;
		}

		// Token: 0x0600249E RID: 9374 RVA: 0x001908E5 File Offset: 0x0018EAE5
		public virtual void OnFire()
		{
			this.shakeTimer = 250;
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x001908F4 File Offset: 0x0018EAF4
		public override bool ShouldActuallyMoveAwayFromPlayer()
		{
			if (base.Player != null)
			{
				Point playerTile = base.Player.TilePoint;
				Point curTile = base.TilePoint;
				if (Math.Abs(playerTile.X - curTile.X) < this.desiredDistance && Math.Abs(playerTile.Y - curTile.Y) < this.desiredDistance)
				{
					return true;
				}
			}
			return base.ShouldActuallyMoveAwayFromPlayer();
		}

		// Token: 0x060024A0 RID: 9376 RVA: 0x00190958 File Offset: 0x0018EB58
		public Shooter(Vector2 position) : base("Shadow Sniper", position)
		{
			this.Sprite.SpriteHeight = 32;
			this.Sprite.SpriteWidth = 32;
			this.forceOneTileWide.Value = true;
			this.Sprite.UpdateSourceRect();
			this.InitializeVariant();
		}

		// Token: 0x060024A1 RID: 9377 RVA: 0x00190A30 File Offset: 0x0018EC30
		public Shooter(Vector2 position, string monster_name) : base(monster_name, position)
		{
			this.Sprite.SpriteHeight = 32;
			this.Sprite.SpriteWidth = 32;
			this.forceOneTileWide.Value = true;
			this.Sprite.UpdateSourceRect();
			this.InitializeVariant();
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x00190B02 File Offset: 0x0018ED02
		public virtual void InitializeVariant()
		{
			this.nextShot = 1f;
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x00190B0F File Offset: 0x0018ED0F
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x00190B44 File Offset: 0x0018ED44
		protected override void updateAnimation(GameTime time)
		{
			if (this.shooting.Value)
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.CurrentFrame = 18;
					break;
				case 1:
					this.Sprite.CurrentFrame = 17;
					break;
				case 2:
					this.Sprite.CurrentFrame = 16;
					break;
				case 3:
					this.Sprite.CurrentFrame = 19;
					break;
				}
			}
			if (!Game1.IsMasterGame && this.isMoving())
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.AnimateUp(time, 0, "");
					return;
				case 1:
					this.Sprite.AnimateRight(time, 0, "");
					return;
				case 2:
					this.Sprite.AnimateDown(time, 0, "");
					break;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x00190C30 File Offset: 0x0018EE30
		public override void behaviorAtGameTick(GameTime time)
		{
			if (!this.shooting.Value)
			{
				if (this.nextShot > 0f)
				{
					this.nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else if (base.Player != null)
				{
					Point tilePoint = base.Player.TilePoint;
					Point curTile = base.TilePoint;
					int playerX = tilePoint.X;
					int playerY = tilePoint.Y;
					int x = curTile.X;
					int y = curTile.Y;
					if (Math.Abs(playerX - x) <= this.fireRange && Math.Abs(playerY - y) <= this.fireRange && (Math.Abs(playerX - x) < 2 || Math.Abs(playerY - y) < 2))
					{
						this.Halt();
						base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false);
						this.shooting.Value = true;
						this.nextShot = this.aimTime;
						this.shotsLeft = this.numberOfShotsPerFire;
					}
				}
			}
			else
			{
				this.xVelocity = 0f;
				this.yVelocity = 0f;
				if (this.shotsLeft > 0)
				{
					if (this.nextShot > 0f)
					{
						this.nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
						if (this.nextShot <= 0f)
						{
							Vector2 shot_velocity;
							float starting_rotation;
							switch (this.FacingDirection)
							{
							case 0:
								shot_velocity = new Vector2(0f, -1f);
								starting_rotation = 0f;
								break;
							case 1:
								shot_velocity = new Vector2(1f, 0f);
								starting_rotation = 1.5707964f;
								break;
							case 2:
								shot_velocity = new Vector2(0f, 1f);
								starting_rotation = 3.1415927f;
								break;
							case 3:
								shot_velocity = new Vector2(-1f, 0f);
								starting_rotation = -1.5707964f;
								break;
							default:
								shot_velocity = Vector2.Zero;
								starting_rotation = 0f;
								break;
							}
							shot_velocity *= (float)this.projectileSpeed;
							this.fireEvent.Fire();
							base.currentLocation.playSound(this.fireSound, null, null, SoundContext.Default);
							BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer, this.firedProjectile, 0, 0, 0f, shot_velocity.X, shot_velocity.Y, base.Position, null, null, null, false, false, base.currentLocation, this, null, null);
							projectile.startingRotation.Value = starting_rotation;
							projectile.height.Value = 24f;
							projectile.debuff.Value = this.projectileDebuff;
							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.IgnoreLocationCollision = true;
							projectile.maxTravelDistance.Value = 64 * this.projectileRange;
							base.currentLocation.projectiles.Add(projectile);
							this.shotsLeft--;
							if (this.shotsLeft == 0)
							{
								this.nextShot = this.aimEndTime;
							}
							else
							{
								this.nextShot = this.burstTime;
							}
						}
					}
				}
				else if (this.nextShot > 0f)
				{
					this.nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					this.shooting.Value = false;
					this.nextShot = 2f;
				}
			}
			base.behaviorAtGameTick(time);
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x00190F94 File Offset: 0x0018F194
		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (this.shooting.Value)
			{
				this.MovePosition(time, Game1.viewport, location);
				return;
			}
			base.updateMovement(location, time);
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x00190FBC File Offset: 0x0018F1BC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			this.shooting.Value = false;
			this.shotsLeft = 0;
			this.nextShot = Math.Max(0.5f, this.nextShot);
			base.currentLocation.playSound(this.damageSound, null, null, SoundContext.Default);
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x00191028 File Offset: 0x0018F228
		protected override void localDeathAnimation()
		{
			if (base.Name == "Shadow Sniper")
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0), base.currentLocation, 4, 64, 64);
				for (int i = 1; i < 3; i++)
				{
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, 1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, -1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 0f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 0f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = i * 159
					});
				}
				base.currentLocation.localSound("shadowDie", null, null, SoundContext.Default);
			}
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x0019126C File Offset: 0x0018F46C
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 2, this.Sprite.SourceRect.Y + 5, 16, 5), 10, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x00191354 File Offset: 0x0018F554
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			this.fireEvent.Poll();
		}

		// Token: 0x0400159A RID: 5530
		public NetBool shooting = new NetBool();

		// Token: 0x0400159B RID: 5531
		public int shotsLeft;

		// Token: 0x0400159C RID: 5532
		public float nextShot;

		// Token: 0x0400159D RID: 5533
		public int projectileSpeed = 12;

		// Token: 0x0400159E RID: 5534
		public string projectileDebuff = "26";

		// Token: 0x0400159F RID: 5535
		public int numberOfShotsPerFire = 1;

		// Token: 0x040015A0 RID: 5536
		public float aimTime = 0.25f;

		// Token: 0x040015A1 RID: 5537
		public float burstTime = 0.25f;

		// Token: 0x040015A2 RID: 5538
		public float aimEndTime = 1f;

		// Token: 0x040015A3 RID: 5539
		public int firedProjectile = 12;

		// Token: 0x040015A4 RID: 5540
		public string damageSound = "shadowHit";

		// Token: 0x040015A5 RID: 5541
		public string fireSound = "Cowboy_gunshot";

		// Token: 0x040015A6 RID: 5542
		public int projectileRange = 10;

		// Token: 0x040015A7 RID: 5543
		public int desiredDistance = 5;

		// Token: 0x040015A8 RID: 5544
		public int fireRange = 8;

		// Token: 0x040015A9 RID: 5545
		[XmlIgnore]
		public NetEvent0 fireEvent = new NetEvent0(false);
	}
}
