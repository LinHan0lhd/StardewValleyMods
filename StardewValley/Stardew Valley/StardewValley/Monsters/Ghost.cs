using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters
{
	// Token: 0x02000219 RID: 537
	public class Ghost : Monster
	{
		// Token: 0x06002384 RID: 9092 RVA: 0x001815A8 File Offset: 0x0017F7A8
		public Ghost()
		{
			this.lightSourceId = this.GenerateLightSourceId(this.identifier);
		}

		// Token: 0x06002385 RID: 9093 RVA: 0x0018160C File Offset: 0x0017F80C
		public Ghost(Vector2 position) : base("Ghost", position)
		{
			this.lightSourceId = this.GenerateLightSourceId(this.identifier);
			base.Slipperiness = 8;
			this.isGlider.Value = true;
			base.HideShadow = true;
		}

		// Token: 0x06002386 RID: 9094 RVA: 0x00181690 File Offset: 0x0017F890
		public Ghost(Vector2 position, string name) : base(name, position)
		{
			this.lightSourceId = this.GenerateLightSourceId(this.identifier);
			base.Slipperiness = 8;
			this.isGlider.Value = true;
			base.HideShadow = true;
			if (name == "Putrid Ghost")
			{
				this.variant.Value = Ghost.GhostVariant.Putrid;
			}
		}

		// Token: 0x06002387 RID: 9095 RVA: 0x00181728 File Offset: 0x0017F928
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.variant, "variant").AddField(this.currentState, "currentState");
			this.currentState.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				this.stateTimer = -1f;
			};
		}

		// Token: 0x06002388 RID: 9096 RVA: 0x00181779 File Offset: 0x0017F979
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + this.name.Value);
		}

		// Token: 0x06002389 RID: 9097 RVA: 0x0018179B File Offset: 0x0017F99B
		public override int GetBaseDifficultyLevel()
		{
			if (this.variant.Value == Ghost.GhostVariant.Putrid)
			{
				return 1;
			}
			return base.GetBaseDifficultyLevel();
		}

		// Token: 0x0600238A RID: 9098 RVA: 0x001817B4 File Offset: 0x0017F9B4
		public override List<Item> getExtraDropItems()
		{
			if (Game1.random.NextDouble() < 0.095 && Game1.player.team.SpecialOrderActive("Wizard") && !Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop"))
			{
				Object o = ItemRegistry.Create<Object>("(O)875", 1, 0, false);
				o.specialItem = true;
				o.questItem.Value = true;
				return new List<Item>
				{
					o
				};
			}
			return base.getExtraDropItems();
		}

		// Token: 0x0600238B RID: 9099 RVA: 0x00181834 File Offset: 0x0017FA34
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(21 + this.yOffset)), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
		}

		// Token: 0x0600238C RID: 9100 RVA: 0x0018198C File Offset: 0x0017FB8C
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (this.variant.Value == Ghost.GhostVariant.Putrid && this.currentState.Value <= 2)
			{
				this.currentState.Value = 0;
			}
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			base.Slipperiness = 8;
			Utility.addSprinklesToLocation(base.currentLocation, base.TilePoint.X, base.TilePoint.Y, 2, 2, 101, 50, Color.LightBlue, null, false);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
				base.setTrajectory(xTrajectory, yTrajectory);
			}
			this.addedSpeed = -1f;
			Utility.removeLightSource(this.lightSourceId);
			return actualDamage;
		}

		// Token: 0x0600238D RID: 9101 RVA: 0x00181A74 File Offset: 0x0017FC74
		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("ghost", null, null, SoundContext.Default);
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, false, false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, 0.049087387f, false));
		}

		// Token: 0x0600238E RID: 9102 RVA: 0x00181B08 File Offset: 0x0017FD08
		protected override void sharedDeathAnimation()
		{
		}

		// Token: 0x0600238F RID: 9103 RVA: 0x00181B0C File Offset: 0x0017FD0C
		protected override void updateAnimation(GameTime time)
		{
			this.nextParticle -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.nextParticle <= 0f)
			{
				this.nextParticle = 1f;
				if (this.variant.Value == Ghost.GhostVariant.Putrid)
				{
					if (this.currentLocationRef.Value != null)
					{
						int standingY = base.StandingPixel.Y;
						TemporaryAnimatedSprite drip = new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 16, 168, 16, 24), 100f, 1, 10, base.Position + new Vector2(Utility.RandomFloat(-16f, 16f, null), Utility.RandomFloat(-16f, 0f, null) - (float)this.yOffset), false, false, (float)standingY / 10000f, 0.01f, Color.White, 4f, -0.01f, 0f, 0f, false);
						drip.acceleration = new Vector2(0f, 0.025f);
						base.currentLocation.temporarySprites.Add(drip);
					}
					this.nextParticle = Utility.RandomFloat(0.3f, 0.5f, null);
				}
			}
			this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * 6.283185307179586) * 20.0) - this.yOffsetExtra;
			if (base.currentLocation == Game1.currentLocation)
			{
				LightSource light;
				if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out light))
				{
					light.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)this.yOffset);
				}
				else if (this.name.Value == "Carbon Ghost")
				{
					Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, new Color(80, 30, 0), LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
				}
				else
				{
					Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
				}
			}
			if (this.variant.Value != Ghost.GhostVariant.Putrid || !this.UpdateVariantAnimation(time))
			{
				Point monsterPixel = base.StandingPixel;
				Point standingPixel = base.Player.StandingPixel;
				float xSlope = (float)(-(float)(standingPixel.X - monsterPixel.X));
				float ySlope = (float)(standingPixel.Y - monsterPixel.Y);
				float t = 400f;
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
					this.wasHitCounter = 0;
				}
				float maxAccel = Math.Min(4f, Math.Max(1f, 5f - t / 64f / 2f));
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * 5f))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * 5f))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
				base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false, false);
				base.resetAnimationSpeed();
			}
		}

		// Token: 0x06002390 RID: 9104 RVA: 0x0018205C File Offset: 0x0018025C
		public virtual bool UpdateVariantAnimation(GameTime time)
		{
			if (this.variant.Value != Ghost.GhostVariant.Putrid)
			{
				return false;
			}
			if (this.currentState.Value == 0)
			{
				if (this.Sprite.CurrentFrame >= 20)
				{
					this.Sprite.CurrentFrame = 0;
				}
				return false;
			}
			if (this.currentState.Value >= 1 && this.currentState.Value <= 3)
			{
				this.shakeTimer = 250;
				if (base.Player != null)
				{
					base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false, false);
				}
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.CurrentFrame = 22;
					break;
				case 1:
					this.Sprite.CurrentFrame = 21;
					break;
				case 2:
					this.Sprite.CurrentFrame = 20;
					break;
				case 3:
					this.Sprite.CurrentFrame = 23;
					break;
				}
			}
			else if (this.currentState.Value >= 4)
			{
				this.shakeTimer = 250;
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.CurrentFrame = 26;
					break;
				case 1:
					this.Sprite.CurrentFrame = 25;
					break;
				case 2:
					this.Sprite.CurrentFrame = 24;
					break;
				case 3:
					this.Sprite.CurrentFrame = 27;
					break;
				}
			}
			return true;
		}

		// Token: 0x06002391 RID: 9105 RVA: 0x001821C9 File Offset: 0x001803C9
		public override void noMovementProgressNearPlayerBehavior()
		{
		}

		// Token: 0x06002392 RID: 9106 RVA: 0x001821CC File Offset: 0x001803CC
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.stateTimer > 0f)
			{
				this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.stateTimer <= 0f)
				{
					this.stateTimer = 0f;
				}
			}
			if (this.variant.Value == Ghost.GhostVariant.Putrid)
			{
				Farmer player = base.Player;
				switch (this.currentState.Value)
				{
				case 0:
					if (this.stateTimer == -1f)
					{
						this.stateTimer = Utility.RandomFloat(1f, 2f, null);
					}
					if (player != null && this.stateTimer == 0f && Math.Abs(player.Position.X - base.Position.X) < 448f && Math.Abs(player.Position.Y - base.Position.Y) < 448f)
					{
						this.currentState.Value = 1;
						base.currentLocation.playSound("croak", null, null, SoundContext.Default);
						this.stateTimer = 0.5f;
					}
					break;
				case 1:
					this.xVelocity = 0f;
					this.yVelocity = 0f;
					if (this.stateTimer <= 0f)
					{
						this.currentState.Value = 2;
					}
					break;
				case 2:
					if (player == null)
					{
						this.currentState.Value = 0;
					}
					else if (Math.Abs(player.Position.X - base.Position.X) < 80f && Math.Abs(player.Position.Y - base.Position.Y) < 80f)
					{
						this.currentState.Value = 3;
						this.stateTimer = 0.05f;
						this.xVelocity = 0f;
						this.yVelocity = 0f;
					}
					else
					{
						Vector2 offset = player.getStandingPosition() - base.getStandingPosition();
						if (offset.LengthSquared() == 0f)
						{
							this.currentState.Value = 3;
							this.stateTimer = 0.15f;
						}
						else
						{
							offset.Normalize();
							offset *= 10f;
							this.xVelocity = offset.X;
							this.yVelocity = -offset.Y;
						}
					}
					break;
				case 3:
					this.xVelocity = 0f;
					this.yVelocity = 0f;
					if (this.stateTimer <= 0f)
					{
						this.currentState.Value = 4;
						this.stateTimer = 1f;
						Vector2 shot_velocity;
						switch (this.FacingDirection)
						{
						case 0:
							shot_velocity = new Vector2(0f, -1f);
							break;
						case 1:
							shot_velocity = new Vector2(1f, 0f);
							break;
						case 2:
							shot_velocity = new Vector2(0f, 1f);
							break;
						case 3:
							shot_velocity = new Vector2(-1f, 0f);
							break;
						default:
							shot_velocity = Vector2.Zero;
							break;
						}
						shot_velocity *= 6f;
						base.currentLocation.playSound("fishSlap", null, null, SoundContext.Default);
						BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer, 7, 0, 1, 0.09817477f, shot_velocity.X, shot_velocity.Y, base.Position, null, null, null, false, false, base.currentLocation, this, null, null);
						projectile.debuff.Value = "25";
						projectile.scaleGrow.Value = 0.05f;
						projectile.ignoreTravelGracePeriod.Value = true;
						projectile.IgnoreLocationCollision = true;
						projectile.maxTravelDistance.Value = 192;
						base.currentLocation.projectiles.Add(projectile);
					}
					break;
				case 4:
					if (this.stateTimer <= 0f)
					{
						this.xVelocity = 0f;
						this.yVelocity = 0f;
						this.currentState.Value = 0;
						this.stateTimer = Utility.RandomFloat(3f, 4f, null);
					}
					break;
				}
			}
			base.behaviorAtGameTick(time);
			Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
			if (this.GetBoundingBox().Intersects(playerBounds) && base.Player.temporarilyInvincible && this.currentState.Value == 0)
			{
				Layer backLayer = base.currentLocation.map.RequireLayer("Back");
				Point playerCenter = playerBounds.Center;
				int attempts = 0;
				Vector2 attemptedPosition = new Vector2((float)(playerCenter.X / 64 + Game1.random.Next(-12, 12)), (float)(playerCenter.Y / 64 + Game1.random.Next(-12, 12)));
				while (attempts < 3 && (attemptedPosition.X >= (float)backLayer.LayerWidth || attemptedPosition.Y >= (float)backLayer.LayerHeight || attemptedPosition.X < 0f || attemptedPosition.Y < 0f || backLayer.Tiles[(int)attemptedPosition.X, (int)attemptedPosition.Y] == null || !base.currentLocation.isTilePassable(new Location((int)attemptedPosition.X, (int)attemptedPosition.Y), Game1.viewport) || attemptedPosition.Equals(new Vector2((float)(playerCenter.X / 64), (float)(playerCenter.Y / 64)))))
				{
					attemptedPosition = new Vector2((float)(playerCenter.X / 64 + Game1.random.Next(-12, 12)), (float)(playerCenter.Y / 64 + Game1.random.Next(-12, 12)));
					attempts++;
				}
				if (attempts < 3)
				{
					base.Position = new Vector2(attemptedPosition.X * 64f, attemptedPosition.Y * 64f - 32f);
					this.Halt();
				}
			}
		}

		// Token: 0x04001506 RID: 5382
		public const float rotationIncrement = 0.049087387f;

		// Token: 0x04001507 RID: 5383
		[XmlIgnore]
		public int wasHitCounter;

		// Token: 0x04001508 RID: 5384
		[XmlIgnore]
		public float targetRotation;

		// Token: 0x04001509 RID: 5385
		[XmlIgnore]
		public bool turningRight;

		// Token: 0x0400150A RID: 5386
		[XmlIgnore]
		public int identifier = Game1.random.Next(-99999, 99999);

		// Token: 0x0400150B RID: 5387
		[XmlIgnore]
		public new int yOffset;

		// Token: 0x0400150C RID: 5388
		[XmlIgnore]
		public int yOffsetExtra;

		// Token: 0x0400150D RID: 5389
		[XmlIgnore]
		public string lightSourceId;

		// Token: 0x0400150E RID: 5390
		public NetInt currentState = new NetInt(0);

		// Token: 0x0400150F RID: 5391
		public float stateTimer = -1f;

		// Token: 0x04001510 RID: 5392
		public float nextParticle;

		// Token: 0x04001511 RID: 5393
		public NetEnum<Ghost.GhostVariant> variant = new NetEnum<Ghost.GhostVariant>(Ghost.GhostVariant.Normal);

		// Token: 0x02000588 RID: 1416
		public enum GhostVariant
		{
			// Token: 0x04002BD3 RID: 11219
			Normal,
			// Token: 0x04002BD4 RID: 11220
			Putrid
		}
	}
}
