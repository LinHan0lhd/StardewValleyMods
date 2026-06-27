using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Monsters
{
	// Token: 0x0200021B RID: 539
	public class Grub : Monster
	{
		// Token: 0x060023AD RID: 9133 RVA: 0x00185E8A File Offset: 0x0018408A
		public Grub()
		{
		}

		// Token: 0x060023AE RID: 9134 RVA: 0x00185EC9 File Offset: 0x001840C9
		public Grub(Vector2 position) : this(position, false)
		{
		}

		// Token: 0x060023AF RID: 9135 RVA: 0x00185ED4 File Offset: 0x001840D4
		public Grub(Vector2 position, bool hard) : base("Grub", position)
		{
			if (Game1.random.NextBool())
			{
				this.leftDrift.Value = true;
			}
			this.FacingDirection = Game1.random.Next(4);
			this.targetRotation.Value = (this.rotation = (float)Game1.random.Next(4) / 3.1415927f);
			this.hard.Value = hard;
			if (hard)
			{
				base.DamageToFarmer *= 3;
				base.Health *= 5;
				base.MaxHealth = base.Health;
				base.ExperienceGained *= 3;
				if (Game1.random.NextDouble() < 0.1)
				{
					this.objectsToDrop.Add("456");
				}
			}
		}

		// Token: 0x060023B0 RID: 9136 RVA: 0x00185FE0 File Offset: 0x001841E0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.leftDrift, "leftDrift").AddField(this.pupating, "pupating").AddField(this.hard, "hard").AddField(this.targetRotation, "targetRotation");
			this.position.Field.AxisAlignedMovement = true;
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x0018604B File Offset: 0x0018424B
		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			this.Sprite.SpriteHeight = 24;
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060023B2 RID: 9138 RVA: 0x0018606C File Offset: 0x0018426C
		public void setHard()
		{
			this.hard.Value = true;
			if (this.hard.Value)
			{
				base.DamageToFarmer = 12;
				base.Health = 100;
				base.MaxHealth = base.Health;
				base.ExperienceGained = 10;
				if (Game1.random.NextDouble() < 0.1)
				{
					this.objectsToDrop.Add("456");
				}
			}
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x001860DC File Offset: 0x001842DC
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.currentLocation.playSound("slimeHit", null, null, SoundContext.Default);
				if (this.pupating.Value)
				{
					base.currentLocation.playSound("crafting", null, null, SoundContext.Default);
					base.setTrajectory(xTrajectory / 2, yTrajectory / 2);
					return 0;
				}
				base.Slipperiness = 4;
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("slimedead", null, null, SoundContext.Default);
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, this.isHardModeMonster.Value ? Color.LimeGreen : Color.Orange, 10, false, 100f, 0, -1, -1f, -1, 0)
					{
						holdLastFrame = true,
						alphaFade = 0.01f,
						interval = 50f
					}, base.currentLocation, 4, 64, 64);
				}
			}
			return actualDamage;
		}

		// Token: 0x060023B4 RID: 9140 RVA: 0x0018623C File Offset: 0x0018443C
		public override void defaultMovementBehavior(GameTime time)
		{
			base.Scale = 1f + (float)(0.125 * Math.Sin(time.TotalGameTime.TotalMilliseconds / (double)(500f + base.Position.X / 100f)));
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x0018628C File Offset: 0x0018448C
		public override void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			base.BuffForAdditionalDifficulty(additional_difficulty);
			this.rotation = 0f;
			this.targetRotation.Value = 0f;
		}

		// Token: 0x060023B6 RID: 9142 RVA: 0x001862B0 File Offset: 0x001844B0
		public override void update(GameTime time, GameLocation location)
		{
			if ((base.Health > 8 || (this.hard.Value && base.Health >= base.MaxHealth)) && !this.pupating.Value)
			{
				base.update(time, location);
				return;
			}
			if (this.invincibleCountdown > 0)
			{
				this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (Game1.IsMasterGame)
			{
				this.behaviorAtGameTick(time);
			}
			this.updateAnimation(time);
		}

		// Token: 0x060023B7 RID: 9143 RVA: 0x00186340 File Offset: 0x00184540
		public override void draw(SpriteBatch b)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)(this.GetBoundingBox().Height / 2)) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(this.Sprite.SourceRect), this.hard.Value ? Color.Lime : Color.White, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth / 2), (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, this.scale.Value) * 4f, (this.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
		}

		// Token: 0x060023B8 RID: 9144 RVA: 0x001864A0 File Offset: 0x001846A0
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.pupating.Value)
			{
				base.Scale = 1f + (float)Math.Sin((double)((float)time.TotalGameTime.Milliseconds * 0.3926991f)) / 12f;
				this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				return;
			}
			if (base.Health <= 8 || (this.hard.Value && base.Health < base.MaxHealth))
			{
				this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (this.metamorphCounter <= 0)
				{
					this.Sprite.Animate(time, 16, 4, 125f);
					if (this.Sprite.currentFrame == 19)
					{
						this.metamorphCounter = 4500;
					}
					return;
				}
			}
			else
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
					this.rotation = 0f;
					base.Scale = 1f;
					return;
				}
				if (!this.withinPlayerThreshold())
				{
					this.Halt();
					this.rotation = this.targetRotation.Value;
				}
			}
		}

		// Token: 0x060023B9 RID: 9145 RVA: 0x00186630 File Offset: 0x00184830
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.pupating.Value)
			{
				base.Scale = 1f + (float)Math.Sin((double)((float)time.TotalGameTime.Milliseconds * 0.3926991f)) / 12f;
				this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (this.metamorphCounter <= 0)
				{
					Point standingPixel = base.StandingPixel;
					base.Health = -500;
					Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(208, 424, 32, 40), 4, standingPixel.X, standingPixel.Y, 25, base.TilePoint.Y);
					Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(208, 424, 32, 40), 8, standingPixel.X, standingPixel.Y, 15, base.TilePoint.Y);
					MineShaft mine = base.currentLocation as MineShaft;
					if (mine != null)
					{
						base.currentLocation.characters.Add(mine.BuffMonsterIfNecessary(new Fly(base.Position, this.hard.Value)
						{
							currentLocation = base.currentLocation
						}));
						return;
					}
					base.currentLocation.characters.Add(new Fly(base.Position, this.hard.Value)
					{
						currentLocation = base.currentLocation
					});
					return;
				}
			}
			else if (base.Health <= base.MaxHealth / 2 - 2 || (this.hard.Value && base.Health < base.MaxHealth))
			{
				this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (this.metamorphCounter <= 0)
				{
					this.Sprite.Animate(time, 16, 4, 125f);
					if (this.Sprite.currentFrame == 19)
					{
						this.pupating.Value = true;
						this.metamorphCounter = 4500;
					}
					return;
				}
				Point monsterPixel = base.StandingPixel;
				Point playerPixel = base.Player.StandingPixel;
				if (Math.Abs(playerPixel.Y - monsterPixel.Y) > 128)
				{
					if (playerPixel.X > monsterPixel.X)
					{
						this.SetMovingLeft(true);
					}
					else
					{
						this.SetMovingRight(true);
					}
				}
				else if (Math.Abs(playerPixel.X - monsterPixel.X) > 128)
				{
					if (playerPixel.Y > monsterPixel.Y)
					{
						this.SetMovingUp(true);
					}
					else
					{
						this.SetMovingDown(true);
					}
				}
				this.MovePosition(time, Game1.viewport, base.currentLocation);
				return;
			}
			else
			{
				if (this.withinPlayerThreshold())
				{
					base.Scale = 1f;
					this.rotation = 0f;
					return;
				}
				if (this.isMoving())
				{
					this.Halt();
					this.faceDirection(Game1.random.Next(4));
					this.targetRotation.Value = (this.rotation = (float)Game1.random.Next(4) / 3.1415927f);
				}
			}
		}

		// Token: 0x0400152F RID: 5423
		public const int healthToRunAway = 8;

		// Token: 0x04001530 RID: 5424
		[XmlIgnore]
		public readonly NetBool leftDrift = new NetBool();

		// Token: 0x04001531 RID: 5425
		[XmlIgnore]
		public readonly NetBool pupating = new NetBool();

		// Token: 0x04001532 RID: 5426
		[XmlElement("hard")]
		public readonly NetBool hard = new NetBool();

		// Token: 0x04001533 RID: 5427
		[XmlIgnore]
		public int metamorphCounter = 2000;

		// Token: 0x04001534 RID: 5428
		[XmlIgnore]
		public readonly NetFloat targetRotation = new NetFloat();
	}
}
