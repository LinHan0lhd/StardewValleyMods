using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x0200022B RID: 555
	public class Skeleton : Monster
	{
		// Token: 0x060024AB RID: 9387 RVA: 0x00191369 File Offset: 0x0018F569
		public Skeleton()
		{
		}

		// Token: 0x060024AC RID: 9388 RVA: 0x00191388 File Offset: 0x0018F588
		public Skeleton(Vector2 position, bool isMage = false) : base("Skeleton", position, Game1.random.Next(4))
		{
			this.isMage.Value = isMage;
			this.reloadSprite(false);
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
			base.IsWalkingTowardPlayer = false;
			this.jitteriness.Value = 0.0;
		}

		// Token: 0x060024AD RID: 9389 RVA: 0x00191408 File Offset: 0x0018F608
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.throwing, "throwing").AddField(this.isMage, "isMage");
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x00191448 File Offset: 0x0018F648
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Skeleton" + (this.isMage.Value ? " Mage" : ""));
			this.Sprite.SpriteHeight = 32;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x0019149C File Offset: 0x0018F69C
		public override List<Item> getExtraDropItems()
		{
			List<Item> extra = new List<Item>();
			if (Game1.random.NextDouble() < 0.04)
			{
				extra.Add(ItemRegistry.Create("(W)5", 1, 0, false));
			}
			return extra;
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x001914D8 File Offset: 0x0018F6D8
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			base.currentLocation.playSound("skeletonHit", null, null, SoundContext.Default);
			base.Slipperiness = 3;
			if (this.throwing.Value)
			{
				this.throwing.Value = false;
				this.Halt();
			}
			if (base.Health - damage <= 0)
			{
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(46, base.Position, Color.White, 10, false, 70f, 0, -1, -1f, -1, 0)
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(46, base.Position + new Vector2(-16f, 0f), Color.White, 10, false, 70f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 100
					}
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(46, base.Position + new Vector2(16f, 0f), Color.White, 10, false, 70f, 0, -1, -1f, -1, 0)
					{
						delayBeforeAnimationStart = 200
					}
				});
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		// Token: 0x060024B1 RID: 9393 RVA: 0x0019163C File Offset: 0x0018F83C
		public override void shedChunks(int number)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 128, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x0019169D File Offset: 0x0018F89D
		public override void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			base.BuffForAdditionalDifficulty(additional_difficulty);
			if (!this.isMage.Value)
			{
				base.MaxHealth += 300;
				base.Health += 300;
			}
		}

		// Token: 0x060024B3 RID: 9395 RVA: 0x001916D8 File Offset: 0x0018F8D8
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			base.currentLocation.playSound("skeletonDie", null, null, SoundContext.Default);
			this.shedChunks(20);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(3, Game1.random.Choose(3, 35), 10, 10), 11, standingPixel.X, standingPixel.Y, 1, base.TilePoint.Y, Color.White, 4f);
		}

		// Token: 0x060024B4 RID: 9396 RVA: 0x0019176D File Offset: 0x0018F96D
		public override void update(GameTime time, GameLocation location)
		{
			if (!this.throwing.Value)
			{
				base.update(time, location);
				return;
			}
			if (Game1.IsMasterGame)
			{
				this.behaviorAtGameTick(time);
			}
			this.updateAnimation(time);
		}

		// Token: 0x060024B5 RID: 9397 RVA: 0x0019179C File Offset: 0x0018F99C
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.throwing.Value)
			{
				if (this.invincibleCountdown > 0)
				{
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				if (this.Sprite.Animate(time, 20, 4, 150f))
				{
					this.Sprite.currentFrame = 23;
					return;
				}
			}
			else if (this.isMoving())
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
					return;
				case 3:
					this.Sprite.AnimateLeft(time, 0, "");
					return;
				default:
					return;
				}
			}
			else
			{
				this.Sprite.StopAnimation();
			}
		}

		// Token: 0x060024B6 RID: 9398 RVA: 0x00191890 File Offset: 0x0018FA90
		public override void behaviorAtGameTick(GameTime time)
		{
			if (!this.throwing.Value)
			{
				base.behaviorAtGameTick(time);
			}
			if (!this.spottedPlayer && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
			{
				this.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
				this.spottedPlayer = true;
				PathFindController controller = this.controller;
				if (((controller != null) ? controller.pathToEndPoint : null) == null || this.controller.pathToEndPoint.Count == 0)
				{
					this.Halt();
					base.facePlayer(base.Player);
				}
				base.currentLocation.playSound("skeletonStep", null, null, SoundContext.Default);
				base.IsWalkingTowardPlayer = true;
			}
			else if (this.throwing.Value)
			{
				if (this.invincibleCountdown > 0)
				{
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				if (this.Sprite.Animate(time, 20, 4, 150f))
				{
					this.throwing.Value = false;
					this.Sprite.currentFrame = 0;
					this.faceDirection(2);
					Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), 8f, base.Player);
					if (this.isMage.Value)
					{
						if (Game1.random.NextBool())
						{
							base.currentLocation.projectiles.Add(new DebuffingProjectile("19", 14, 4, 4, 0.19634955f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), base.currentLocation, this, false, true));
						}
						else
						{
							base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer * 2, 9, 0, 4, 0f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "flameSpellHit", "flameSpell", null, false, false, base.currentLocation, this, null, null));
						}
					}
					else
					{
						base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer, 4, 0, 0, 0.19634955f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "skeletonHit", "skeletonStep", null, false, false, base.currentLocation, this, null, null));
					}
				}
			}
			else if (this.spottedPlayer && this.controller == null && Game1.random.NextDouble() < (this.isMage.Value ? 0.009 : 0.003) && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
			{
				this.throwing.Value = true;
				this.Halt();
				this.Sprite.currentFrame = 20;
				base.shake(750);
			}
			else if (this.withinPlayerThreshold(2))
			{
				this.controller = null;
			}
			else if (this.spottedPlayer && this.controller == null && this.controllerAttemptTimer <= 0)
			{
				this.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
				this.controllerAttemptTimer = (base.wildernessFarmMonster ? 2000 : 1000);
				PathFindController controller2 = this.controller;
				if (((controller2 != null) ? controller2.pathToEndPoint : null) == null || this.controller.pathToEndPoint.Count == 0)
				{
					this.Halt();
				}
			}
			else if (base.wildernessFarmMonster)
			{
				this.spottedPlayer = true;
				base.IsWalkingTowardPlayer = true;
			}
			this.controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
		}

		// Token: 0x040015AA RID: 5546
		[XmlIgnore]
		public bool spottedPlayer;

		// Token: 0x040015AB RID: 5547
		[XmlIgnore]
		public readonly NetBool throwing = new NetBool();

		// Token: 0x040015AC RID: 5548
		public readonly NetBool isMage = new NetBool();

		// Token: 0x040015AD RID: 5549
		private int controllerAttemptTimer;
	}
}
