using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.Monsters
{
	// Token: 0x02000223 RID: 547
	public class RockCrab : Monster
	{
		// Token: 0x0600244D RID: 9293 RVA: 0x0018BB84 File Offset: 0x00189D84
		public RockCrab()
		{
		}

		// Token: 0x0600244E RID: 9294 RVA: 0x0018BBB0 File Offset: 0x00189DB0
		public RockCrab(Vector2 position) : base("Rock Crab", position)
		{
			this.waiter = (Game1.random.NextDouble() < 0.4);
			this.moveTowardPlayerThreshold.Value = 3;
		}

		// Token: 0x0600244F RID: 9295 RVA: 0x0018BC12 File Offset: 0x00189E12
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x06002450 RID: 9296 RVA: 0x0018BC28 File Offset: 0x00189E28
		public RockCrab(Vector2 position, string name) : base(name, position)
		{
			this.waiter = (Game1.random.NextDouble() < 0.4);
			this.moveTowardPlayerThreshold.Value = 3;
			if (name == "Truffle Crab")
			{
				this.waiter = false;
				this.moveTowardPlayerThreshold.Value = 1;
				return;
			}
			if (name == "Iridium Crab")
			{
				this.waiter = true;
				this.moveTowardPlayerThreshold.Value = 1;
				return;
			}
			if (!(name == "False Magma Cap"))
			{
				return;
			}
			this.waiter = false;
		}

		// Token: 0x06002451 RID: 9297 RVA: 0x0018BCE0 File Offset: 0x00189EE0
		public void makeStickBug()
		{
			this.isStickBug.Value = true;
			this.waiter = false;
			base.Name = "Stick Bug";
			base.DamageToFarmer = 20;
			base.MaxHealth = 700;
			base.Health = 700;
			base.reloadSprite(false);
			base.HideShadow = true;
			this.Sprite.SpriteHeight = 24;
			this.Sprite.UpdateSourceRect();
			this.objectsToDrop.Clear();
			this.objectsToDrop.Add("858");
			while (Game1.random.NextBool())
			{
				this.objectsToDrop.Add("858");
			}
			this.objectsToDrop.Add("829");
		}

		// Token: 0x06002452 RID: 9298 RVA: 0x0018BD98 File Offset: 0x00189F98
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.shellGone, "shellGone").AddField(this.shellHealth, "shellHealth").AddField(this.isStickBug, "isStickBug");
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x06002453 RID: 9299 RVA: 0x0018BDF4 File Offset: 0x00189FF4
		public override bool hitWithTool(Tool t)
		{
			if (this.isStickBug.Value)
			{
				return false;
			}
			if (t is Pickaxe && t.getLastFarmerToUse() != null && this.shellHealth.Value > 0)
			{
				base.currentLocation.playSound("hammer", null, null, SoundContext.Default);
				NetInt netInt = this.shellHealth;
				int value = netInt.Value;
				netInt.Value = value - 1;
				base.shake(500);
				this.waiter = false;
				this.moveTowardPlayerThreshold.Value = 3;
				this.setTrajectory(Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), t.getLastFarmerToUse()));
				if (this.shellHealth.Value <= 0)
				{
					Point tile = base.TilePoint;
					this.shellGone.Value = true;
					base.moveTowardPlayer(-1);
					base.currentLocation.playSound("stoneCrack", null, null, SoundContext.Default);
					Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), false, -1, false, null);
					Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), false, -1, false, null);
				}
				return true;
			}
			return base.hitWithTool(t);
		}

		// Token: 0x06002454 RID: 9300 RVA: 0x0018BF60 File Offset: 0x0018A160
		public override void shedChunks(int number)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 120, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f * this.scale.Value);
		}

		// Token: 0x06002455 RID: 9301 RVA: 0x0018BFCC File Offset: 0x0018A1CC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (isBomb && !this.isStickBug.Value)
			{
				this.shellGone.Value = true;
				this.waiter = false;
				base.moveTowardPlayer(-1);
			}
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else if (this.Sprite.currentFrame % 4 == 0 && !this.shellGone.Value)
			{
				actualDamage = 0;
				base.currentLocation.playSound("crafting", null, null, SoundContext.Default);
			}
			else
			{
				base.Health -= actualDamage;
				base.Slipperiness = 3;
				base.setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				this.glowingColor = Color.Cyan;
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("monsterdead", null, null, SoundContext.Default);
					base.deathAnimation();
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.Red, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						holdLastFrame = true,
						alphaFade = 0.01f
					}, base.currentLocation, 4, 64, 64);
				}
			}
			return actualDamage;
		}

		// Token: 0x06002456 RID: 9302 RVA: 0x0018C150 File Offset: 0x0018A350
		public override void update(GameTime time, GameLocation location)
		{
			if (!location.farmers.Any())
			{
				return;
			}
			if (!this.shellGone.Value && !base.Player.isRafting)
			{
				base.update(time, location);
				return;
			}
			if (!base.Player.isRafting)
			{
				if (Game1.IsMasterGame)
				{
					this.behaviorAtGameTick(time);
				}
				this.updateAnimation(time);
			}
		}

		// Token: 0x06002457 RID: 9303 RVA: 0x0018C1B0 File Offset: 0x0018A3B0
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.waiter && this.shellHealth.Value > 4)
			{
				this.moveTowardPlayerThreshold.Value = 0;
				return;
			}
			base.behaviorAtGameTick(time);
			if (this.isMoving() && this.Sprite.currentFrame % 4 == 0)
			{
				this.Sprite.currentFrame++;
				this.Sprite.UpdateSourceRect();
			}
			if (!this.withinPlayerThreshold() && !this.shellGone.Value)
			{
				this.Halt();
				return;
			}
			if (this.withinPlayerThreshold() && !this.shellGone.Value && this.name.Equals("Truffle Crab"))
			{
				this.shellGone.Value = true;
				return;
			}
			if (this.shellGone.Value)
			{
				base.updateGlow();
				if (this.invincibleCountdown > 0)
				{
					this.glowingColor = Color.Cyan;
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				base.IsWalkingTowardPlayer = false;
				Point standingPixel = base.StandingPixel;
				Point standingPixel2 = base.Player.StandingPixel;
				this.FacingDirection = base.getGeneralDirectionTowards(base.Player.getStandingPosition(), 0, true, false);
				this.moveUp = false;
				this.moveDown = false;
				this.moveRight = false;
				this.moveLeft = false;
				base.setMovingInFacingDirection();
				this.MovePosition(time, Game1.viewport, base.currentLocation);
				this.Sprite.CurrentFrame = 16 + this.Sprite.currentFrame % 4;
			}
		}

		// Token: 0x06002458 RID: 9304 RVA: 0x0018C344 File Offset: 0x0018A544
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
			else
			{
				this.Sprite.StopAnimation();
			}
			if (this.isMoving() && this.Sprite.currentFrame % 4 == 0)
			{
				this.Sprite.currentFrame++;
				this.Sprite.UpdateSourceRect();
			}
			if (this.shellGone.Value)
			{
				base.updateGlow();
				if (this.invincibleCountdown > 0)
				{
					this.glowingColor = Color.Cyan;
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				this.Sprite.currentFrame = 16 + this.Sprite.currentFrame % 4;
			}
		}

		// Token: 0x0400157D RID: 5501
		[XmlIgnore]
		public bool waiter;

		// Token: 0x0400157E RID: 5502
		[XmlIgnore]
		public readonly NetBool shellGone = new NetBool();

		// Token: 0x0400157F RID: 5503
		[XmlIgnore]
		public readonly NetInt shellHealth = new NetInt(5);

		// Token: 0x04001580 RID: 5504
		[XmlIgnore]
		public readonly NetBool isStickBug = new NetBool();
	}
}
