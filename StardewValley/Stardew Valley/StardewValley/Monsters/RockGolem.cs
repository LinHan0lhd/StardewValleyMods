using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Pathfinding;

namespace StardewValley.Monsters
{
	// Token: 0x02000224 RID: 548
	public class RockGolem : Monster
	{
		// Token: 0x06002459 RID: 9305 RVA: 0x0018C474 File Offset: 0x0018A674
		public RockGolem()
		{
		}

		// Token: 0x0600245A RID: 9306 RVA: 0x0018C488 File Offset: 0x0018A688
		public RockGolem(Vector2 position) : base("Stone Golem", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.Slipperiness = 2;
			this.jitteriness.Value = 0.0;
			base.HideShadow = true;
		}

		// Token: 0x0600245B RID: 9307 RVA: 0x0018C4D8 File Offset: 0x0018A6D8
		public RockGolem(Vector2 position, MineShaft mineArea) : this(position)
		{
			int mineLevel = mineArea.mineLevel;
			if (mineLevel > 80)
			{
				base.DamageToFarmer *= 2;
				base.Health = (int)((float)base.Health * 2.5f);
				return;
			}
			if (mineLevel > 40)
			{
				base.DamageToFarmer = (int)((float)base.DamageToFarmer * 1.5f);
				base.Health = (int)((float)base.Health * 1.75f);
			}
		}

		// Token: 0x0600245C RID: 9308 RVA: 0x0018C548 File Offset: 0x0018A748
		public RockGolem(Vector2 position, int difficultyMod) : base((difficultyMod >= 9 && Game1.random.NextDouble() < 0.5 && Game1.whichFarm == 4) ? "Iridium Golem" : "Wilderness Golem", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.Slipperiness = 3;
			base.HideShadow = true;
			this.jitteriness.Value = 0.0;
			base.DamageToFarmer += difficultyMod;
			base.Health += (int)((float)(difficultyMod * difficultyMod) * 2f);
			base.ExperienceGained += difficultyMod;
			if (difficultyMod >= 5 && Game1.random.NextDouble() < 0.05)
			{
				this.objectsToDrop.Add("749");
			}
			if (difficultyMod >= 5 && Game1.random.NextDouble() < 0.2)
			{
				this.objectsToDrop.Add("770");
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.01)
			{
				this.objectsToDrop.Add("386");
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.01)
			{
				this.objectsToDrop.Add("386");
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.001)
			{
				this.objectsToDrop.Add("74");
			}
			if (this.name.Value == "Iridium Golem")
			{
				base.Speed *= 2;
				base.Health += 400;
				base.DamageToFarmer += 10;
				base.ExperienceGained += 10;
				if (Game1.random.NextDouble() < 0.03)
				{
					this.objectsToDrop.Add("337");
				}
				if (Game1.random.NextDouble() < 0.03)
				{
					this.objectsToDrop.Add("337");
				}
			}
			this.Sprite.currentFrame = 16;
			this.Sprite.loop = false;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x0018C788 File Offset: 0x0018A988
		public RockGolem(Vector2 position, bool alreadySpawned) : base("Stone Golem", position)
		{
			if (alreadySpawned)
			{
				base.IsWalkingTowardPlayer = true;
				this.seenPlayer.Value = true;
				this.moveTowardPlayerThreshold.Value = 16;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
			}
			this.Sprite.loop = false;
			base.Slipperiness = 2;
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x0018C7EB File Offset: 0x0018A9EB
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.seenPlayer, "seenPlayer");
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x0018C81C File Offset: 0x0018AA1C
		public override List<Item> getExtraDropItems()
		{
			if (this.name.Equals("Wilderness Golem"))
			{
				if (Game1.random.NextDouble() <= 0.0001)
				{
					return new List<Item>
					{
						ItemRegistry.Create("(H)40", 1, 0, false)
					};
				}
				if (Game1.IsSpring && Game1.random.NextDouble() < 0.0825)
				{
					List<Item> shoots = new List<Item>();
					int num = Game1.random.Next(2, 6);
					for (int i = 0; i < num; i++)
					{
						shoots.Add(ItemRegistry.Create("(O)273", 1, 0, false));
					}
					return shoots;
				}
			}
			else if (this.name.Equals("Iridium Golem"))
			{
				List<Item> extra = new List<Item>();
				while (Game1.random.NextDouble() < 0.5)
				{
					extra.Add(Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, Game1.random, 1));
				}
				while (Game1.random.NextDouble() < 0.2)
				{
					extra.Add(ItemRegistry.Create("(O)386", 1, 0, false));
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					extra.Add(ItemRegistry.Create("(O)SkillBook_" + Game1.random.Next(5).ToString(), 1, 0, false));
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					extra.Add(ItemRegistry.Create<Ring>("(O)527", 1, 0, false));
				}
				if (Game1.random.NextDouble() <= 0.0002)
				{
					extra.Add(ItemRegistry.Create("(H)40", 1, 0, false));
				}
				return extra;
			}
			return base.getExtraDropItems();
		}

		// Token: 0x06002460 RID: 9312 RVA: 0x0018C9D0 File Offset: 0x0018ABD0
		public override void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			base.BuffForAdditionalDifficulty(additional_difficulty);
			this.resilience.Value *= 2;
			int speed = base.Speed;
			base.Speed = speed + 1;
		}

		// Token: 0x06002461 RID: 9313 RVA: 0x0018CA08 File Offset: 0x0018AC08
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			base.focusedOnFarmers = true;
			base.IsWalkingTowardPlayer = true;
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					base.deathAnimation();
				}
				else
				{
					base.currentLocation.playSound("rockGolemHit", null, null, SoundContext.Default);
				}
				base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				if (this.name.Value == "Iridium Golem")
				{
					base.Speed = Game1.random.Next(2, 7);
				}
			}
			return actualDamage;
		}

		// Token: 0x06002462 RID: 9314 RVA: 0x0018CAFC File Offset: 0x0018ACFC
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position, Color.DarkGray, 10, false, 100f, 0, -1, -1f, -1, 0));
			base.currentLocation.localSound("rockGolemDie", null, null, SoundContext.Default);
		}

		// Token: 0x06002463 RID: 9315 RVA: 0x0018CB60 File Offset: 0x0018AD60
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 576, 64, 64), 32, standingPixel.X, standingPixel.Y, Game1.random.Next(4, 9), base.TilePoint.Y);
		}

		// Token: 0x06002464 RID: 9316 RVA: 0x0018CBC4 File Offset: 0x0018ADC4
		public override void noMovementProgressNearPlayerBehavior()
		{
			if (base.IsWalkingTowardPlayer)
			{
				this.Halt();
				base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false);
			}
		}

		// Token: 0x06002465 RID: 9317 RVA: 0x0018CBE8 File Offset: 0x0018ADE8
		public override void behaviorAtGameTick(GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				base.behaviorAtGameTick(time);
			}
			if (this.seenPlayer.Value)
			{
				if (this.Sprite.currentFrame >= 16)
				{
					this.Sprite.Animate(time, 16, 8, 75f);
					if (this.Sprite.currentFrame >= 24)
					{
						this.Sprite.loop = true;
						this.Sprite.currentFrame = 0;
						this.moveTowardPlayerThreshold.Value = 16;
						base.IsWalkingTowardPlayer = true;
						this.jitteriness.Value = 0.01;
						if (this.name.Value == "Iridium Golem")
						{
							this.jitteriness.Value += 0.01;
						}
						base.HideShadow = false;
						return;
					}
				}
				else if (base.IsWalkingTowardPlayer && Game1.random.NextDouble() < 0.001 && Utility.isOnScreen(base.getStandingPosition(), 0))
				{
					this.controller = new PathFindController(this, base.currentLocation, new Point(base.Player.TilePoint.X, base.Player.TilePoint.Y), -1, null, 200);
				}
				return;
			}
			if (this.withinPlayerThreshold())
			{
				base.currentLocation.playSound("rockGolemSpawn", null, null, SoundContext.Default);
				this.seenPlayer.Value = true;
				return;
			}
			this.Sprite.currentFrame = 16;
			this.Sprite.loop = false;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x06002466 RID: 9318 RVA: 0x0018CD8C File Offset: 0x0018AF8C
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
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
			if (!this.seenPlayer.Value)
			{
				this.Sprite.currentFrame = 16;
				this.Sprite.loop = false;
				this.Sprite.UpdateSourceRect();
				return;
			}
			if (this.Sprite.currentFrame >= 16)
			{
				this.Sprite.Animate(time, 16, 8, 75f);
				if (this.Sprite.currentFrame >= 24)
				{
					this.Sprite.loop = true;
					this.Sprite.currentFrame = 0;
					this.Sprite.UpdateSourceRect();
				}
			}
		}

		// Token: 0x04001581 RID: 5505
		[XmlIgnore]
		public readonly NetBool seenPlayer = new NetBool();
	}
}
