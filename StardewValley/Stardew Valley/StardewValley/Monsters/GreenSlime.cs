using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using StardewValley.SpecialOrders;

namespace StardewValley.Monsters
{
	// Token: 0x0200021A RID: 538
	public class GreenSlime : Monster
	{
		// Token: 0x06002394 RID: 9108 RVA: 0x001827FC File Offset: 0x001809FC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.leftDrift, "leftDrift").AddField(this.cute, "cute").AddField(this.ageUntilFullGrown, "ageUntilFullGrown").AddField(this.specialNumber, "specialNumber").AddField(this.firstGeneration, "firstGeneration").AddField(this.color, "color").AddField(this.pursuingMate, "pursuingMate").AddField(this.avoidingMate, "avoidingMate").AddField(this.facePosition, "facePosition").AddField(this.jumpEvent, "jumpEvent").AddField(this.prismatic, "prismatic").AddField(this.stackedSlimes, "stackedSlimes").AddField(this.attackedEvent.NetFields, "attackedEvent.NetFields");
			this.attackedEvent.onEvent += this.OnAttacked;
			this.jumpEvent.onEvent += this.doJump;
		}

		// Token: 0x06002395 RID: 9109 RVA: 0x0018291C File Offset: 0x00180B1C
		public GreenSlime()
		{
		}

		// Token: 0x06002396 RID: 9110 RVA: 0x001829E8 File Offset: 0x00180BE8
		public GreenSlime(Vector2 position) : base("Green Slime", position)
		{
			if (Game1.random.NextBool())
			{
				this.leftDrift.Value = true;
			}
			base.Slipperiness = 4;
			this.readyToMate = Game1.random.Next(1000, 120000);
			int green = Game1.random.Next(200, 256);
			this.color.Value = new Color(green / Game1.random.Next(2, 10), Game1.random.Next(180, 256), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - green));
			this.firstGeneration.Value = true;
			this.flip = Game1.random.NextBool();
			this.cute.Value = (Game1.random.NextDouble() < 0.49);
			base.HideShadow = true;
		}

		// Token: 0x06002397 RID: 9111 RVA: 0x00182BA0 File Offset: 0x00180DA0
		public GreenSlime(Vector2 position, int mineLevel) : base("Green Slime", position)
		{
			this.randomStackOffset = Utility.RandomFloat(0f, 100f, null);
			this.cute.Value = (Game1.random.NextDouble() < 0.49);
			this.flip = Game1.random.NextBool();
			this.specialNumber.Value = Game1.random.Next(100);
			if (mineLevel < 40)
			{
				base.parseMonsterInfo("Green Slime");
				int green = Game1.random.Next(200, 256);
				this.color.Value = new Color(green / Game1.random.Next(2, 10), green, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green));
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					this.color.Value = new Color(205, 255, 0) * 0.7f;
					this.hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					this.objectsToDrop.Add("680");
				}
			}
			else if (mineLevel < 80)
			{
				base.Name = "Frost Jelly";
				base.parseMonsterInfo("Frost Jelly");
				int blue = Game1.random.Next(200, 256);
				this.color.Value = new Color((Game1.random.NextDouble() < 0.01) ? 180 : (blue / Game1.random.Next(2, 10)), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - blue / 3), blue);
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					this.color.Value = new Color(0, 0, 0) * 0.7f;
					this.hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					this.objectsToDrop.Add("413");
				}
			}
			else if (mineLevel >= 77377 && mineLevel < 77387)
			{
				base.Name = "Sludge";
				base.parseMonsterInfo("Sludge");
			}
			else if (mineLevel > 120)
			{
				base.Name = "Sludge";
				base.parseMonsterInfo("Sludge");
				this.color.Value = Color.BlueViolet;
				base.Health *= 2;
				int r = (int)this.color.R;
				int g = (int)this.color.G;
				int b = (int)this.color.B;
				r += Game1.random.Next(-20, 21);
				g += Game1.random.Next(-20, 21);
				b += Game1.random.Next(-20, 21);
				this.color.R = (byte)Math.Max(Math.Min(255, r), 0);
				this.color.G = (byte)Math.Max(Math.Min(255, g), 0);
				this.color.B = (byte)Math.Max(Math.Min(255, b), 0);
				while (Game1.random.NextDouble() < 0.08)
				{
					this.objectsToDrop.Add("386");
				}
				if (Game1.random.NextDouble() < 0.009)
				{
					this.objectsToDrop.Add("337");
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					this.objectsToDrop.Add("439");
				}
			}
			else
			{
				base.Name = "Sludge";
				base.parseMonsterInfo("Sludge");
				int green2 = Game1.random.Next(200, 256);
				this.color.Value = new Color(green2, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green2), green2 / Game1.random.Next(2, 10));
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					this.color.Value = new Color(50, 10, 50) * 0.7f;
					this.hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					this.objectsToDrop.Add("437");
				}
			}
			if (this.cute.Value)
			{
				base.Health += base.Health / 4;
				int damageToFarmer = base.DamageToFarmer;
				base.DamageToFarmer = damageToFarmer + 1;
			}
			if (Game1.random.NextBool())
			{
				this.leftDrift.Value = true;
			}
			base.Slipperiness = 3;
			this.readyToMate = Game1.random.Next(1000, 120000);
			if (Game1.random.NextDouble() < 0.001)
			{
				this.color.Value = new Color(255, 255, 50);
				this.objectsToDrop.Add("GoldCoin");
				double extraChance = (double)(Game1.stats.DaysPlayed / 28U) * 0.08;
				extraChance = Math.Min(extraChance, 0.55);
				while (Game1.random.NextDouble() < 0.1 + extraChance)
				{
					this.objectsToDrop.Add("GoldCoin");
				}
			}
			if (mineLevel == 9999899)
			{
				this.color.Value = new Color(0, 255, 200);
				base.Health *= 2;
				this.objectsToDrop.Clear();
				if (Game1.random.NextDouble() < 0.02)
				{
					this.objectsToDrop.Add("394");
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					this.objectsToDrop.Add("60");
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					this.objectsToDrop.Add("62");
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					this.objectsToDrop.Add("797");
				}
				if (Game1.random.NextDouble() < 0.03 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					this.objectsToDrop.Add("413");
				}
				while (Game1.random.NextBool())
				{
					this.objectsToDrop.Add("766");
				}
			}
			this.firstGeneration.Value = true;
			base.HideShadow = true;
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x00183430 File Offset: 0x00181630
		public GreenSlime(Vector2 position, Color color) : base("Green Slime", position)
		{
			this.color.Value = color;
			this.firstGeneration.Value = true;
			base.HideShadow = true;
		}

		// Token: 0x06002399 RID: 9113 RVA: 0x00183520 File Offset: 0x00181720
		public void makeTigerSlime(bool onlyAppearance = false)
		{
			string oldName = base.Name;
			try
			{
				base.Name = "Tiger Slime";
				base.reloadSprite(false);
			}
			finally
			{
				if (onlyAppearance)
				{
					base.Name = oldName;
				}
			}
			this.Sprite.SpriteHeight = 24;
			this.Sprite.UpdateSourceRect();
			this.color.Value = Color.White;
			if (!onlyAppearance)
			{
				base.parseMonsterInfo("Tiger Slime");
			}
		}

		// Token: 0x0600239A RID: 9114 RVA: 0x0018359C File Offset: 0x0018179C
		public void makePrismatic()
		{
			this.prismatic.Value = true;
			base.Name = "Prismatic Slime";
			base.Health = 1000;
			this.damageToFarmer.Value = 35;
			this.hasSpecialItem.Value = false;
		}

		// Token: 0x0600239B RID: 9115 RVA: 0x001835DC File Offset: 0x001817DC
		public override void reloadSprite(bool onlyAppearance = false)
		{
			if (base.Name == "Tiger Slime")
			{
				this.makeTigerSlime(onlyAppearance);
				return;
			}
			string oldName = this.name.Value;
			try
			{
				base.Name = "Green Slime";
				base.reloadSprite(onlyAppearance);
			}
			finally
			{
				base.Name = oldName;
			}
			this.Sprite.SpriteHeight = 24;
			this.Sprite.UpdateSourceRect();
			base.HideShadow = true;
		}

		// Token: 0x0600239C RID: 9116 RVA: 0x0018365C File Offset: 0x0018185C
		public virtual void OnAttacked(Vector2 trajectory)
		{
			if (Game1.IsMasterGame && this.stackedSlimes.Value > 0)
			{
				NetIntDelta netIntDelta = this.stackedSlimes;
				int value = netIntDelta.Value;
				netIntDelta.Value = value - 1;
				if (trajectory.LengthSquared() == 0f)
				{
					trajectory = new Vector2(0f, -1f);
				}
				else
				{
					trajectory.Normalize();
				}
				trajectory *= 16f;
				BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer / 3 * 2, 13, 3, 0, 0.19634955f, trajectory.X, trajectory.Y, base.Position, null, null, null, true, false, base.currentLocation, this, null, null);
				projectile.height.Value = 24f;
				projectile.color.Value = this.color.Value;
				projectile.ignoreMeleeAttacks.Value = true;
				projectile.hostTimeUntilAttackable = 0.1f;
				if (Game1.random.NextBool())
				{
					projectile.debuff.Value = "13";
				}
				base.currentLocation.projectiles.Add(projectile);
			}
		}

		// Token: 0x0600239D RID: 9117 RVA: 0x00183774 File Offset: 0x00181974
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (this.stackedSlimes.Value > 0)
			{
				this.attackedEvent.Fire(new Vector2((float)xTrajectory, (float)(-(float)yTrajectory)));
				xTrajectory = 0;
				yTrajectory = 0;
				damage = 1;
			}
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				if (Game1.random.NextDouble() < 0.025 && this.cute.Value)
				{
					if (!base.focusedOnFarmers)
					{
						base.DamageToFarmer += base.DamageToFarmer / 2;
						base.shake(1000);
					}
					base.focusedOnFarmers = true;
				}
				base.Slipperiness = 3;
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("slimeHit", null, null, SoundContext.Default);
				this.readyToJump = -1;
				base.IsWalkingTowardPlayer = true;
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("slimedead", null, null, SoundContext.Default);
					Stats stats = Game1.stats;
					uint slimesKilled = stats.SlimesKilled;
					stats.SlimesKilled = slimesKilled + 1U;
					if (this.mate != null)
					{
						this.mate.mate = null;
					}
					if (Game1.gameMode == 3 && this.scale.Value > 1.8f)
					{
						base.Health = 10;
						int toCreate = (this.scale.Value > 1.8f) ? Game1.random.Next(3, 5) : 1;
						base.Scale *= 0.6666667f;
						Rectangle bounds = this.GetBoundingBox();
						for (int i = 0; i < toCreate; i++)
						{
							GreenSlime slime = new GreenSlime(base.Position + new Vector2((float)(i * bounds.Width), 0f), Game1.CurrentMineLevel);
							slime.setTrajectory(xTrajectory + Game1.random.Next(-20, 20), yTrajectory + Game1.random.Next(-20, 20));
							slime.willDestroyObjectsUnderfoot = false;
							slime.moveTowardPlayer(4);
							slime.Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
							base.currentLocation.characters.Add(slime);
						}
					}
					else
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(44, base.Position, this.color.Value * 0.66f, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								interval = 70f,
								holdLastFrame = true,
								alphaFade = 0.01f
							}
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), this.color.Value * 0.66f, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								interval = 70f,
								delayBeforeAnimationStart = 0,
								holdLastFrame = true,
								alphaFade = 0.01f
							}
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, 16f), this.color.Value * 0.66f, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								interval = 70f,
								delayBeforeAnimationStart = 100,
								holdLastFrame = true,
								alphaFade = 0.01f
							}
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), this.color.Value * 0.66f, 10, false, 100f, 0, -1, -1f, -1, 0)
							{
								interval = 70f,
								delayBeforeAnimationStart = 200,
								holdLastFrame = true,
								alphaFade = 0.01f
							}
						});
					}
				}
			}
			return actualDamage;
		}

		// Token: 0x0600239E RID: 9118 RVA: 0x00183BF8 File Offset: 0x00181DF8
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 120, 16, 16), 8, standingPixel.X + 32, standingPixel.Y, number, base.TilePoint.Y, this.color.Value, 4f * scale);
		}

		// Token: 0x0600239F RID: 9119 RVA: 0x00183C61 File Offset: 0x00181E61
		public override void collisionWithFarmerBehavior()
		{
			this.farmerPassesThrough = base.Player.isWearingRing("520");
		}

		// Token: 0x060023A0 RID: 9120 RVA: 0x00183C7C File Offset: 0x00181E7C
		public override void onDealContactDamage(Farmer who)
		{
			if (Game1.random.NextDouble() < 0.3 && base.Player == Game1.player && !base.Player.temporarilyInvincible && !base.Player.isWearingRing("520") && Game1.random.Next(11) >= who.Immunity && !base.Player.hasBuff("28") && !base.Player.hasTrinketWithID("BasiliskPaw"))
			{
				base.Player.applyBuff("13");
				base.currentLocation.playSound("slime", null, null, SoundContext.Default);
			}
			base.onDealContactDamage(who);
		}

		// Token: 0x060023A1 RID: 9121 RVA: 0x00183D44 File Offset: 0x00181F44
		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				int boundsHeight = this.GetBoundingBox().Height;
				int standingY = base.StandingPixel.Y;
				for (int i = 0; i <= this.stackedSlimes.Value; i++)
				{
					bool top_slime = i == this.stackedSlimes.Value;
					Vector2 stack_adjustment = Vector2.Zero;
					if (this.stackedSlimes.Value > 0)
					{
						stack_adjustment = new Vector2((float)Math.Sin((double)this.randomStackOffset + Game1.currentGameTime.TotalGameTime.TotalSeconds * 3.141592653589793 * 2.0 + (double)(i * 30)) * 8f, (float)(-30 * i));
					}
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 + this.yOffset)) + stack_adjustment, new Rectangle?(this.Sprite.SourceRect), this.prismatic.Value ? Utility.GetPrismaticColor(348 + this.specialNumber.Value, 5f) : this.color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, this.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f)));
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 * 7) / 4f + (float)this.yOffset + 8f * this.scale.Value - (float)((this.ageUntilFullGrown.Value > 0) ? 8 : 0)) + stack_adjustment, new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + this.scale.Value - (float)this.ageUntilFullGrown.Value / 120000f - ((this.Sprite.currentFrame % 4 % 3 != 0 || i != 0) ? 1f : 0f) + (float)this.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1 + i * 2) / 10000f);
					if (this.ageUntilFullGrown.Value <= 0)
					{
						if (top_slime && (this.cute.Value || this.hasSpecialItem.Value))
						{
							int xDongleSource = (this.isMoving() || this.wagTimer > 0) ? (16 * Math.Min(7, Math.Abs(((this.wagTimer > 0) ? (992 - this.wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) % 64) : 48;
							int yDongleSource = (this.isMoving() || this.wagTimer > 0) ? (24 * Math.Min(1, Math.Max(1, Math.Abs(((this.wagTimer > 0) ? (992 - this.wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) / 4)) : 24;
							if (this.hasSpecialItem.Value)
							{
								yDongleSource += 48;
							}
							b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, (float)(boundsHeight - 16 + ((this.readyToJump <= 0) ? (4 * (-2 + Math.Abs(this.Sprite.currentFrame % 4 - 2))) : (4 + 4 * (this.Sprite.currentFrame % 4 % 3))) + this.yOffset)) * this.scale.Value, new Rectangle?(new Rectangle(xDongleSource, 168 + yDongleSource, 16, 24)), this.hasSpecialItem.Value ? Color.White : this.color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, this.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.0001f)));
						}
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + (new Vector2(32f, (float)(boundsHeight / 2 + ((this.readyToJump <= 0) ? (4 * (-2 + Math.Abs(this.Sprite.currentFrame % 4 - 2))) : (4 - 4 * (this.Sprite.currentFrame % 4 % 3))) + this.yOffset)) + this.facePosition.Value) * Math.Max(0.2f, this.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), new Rectangle?(new Rectangle(32 + ((this.readyToJump > 0 || base.focusedOnFarmers) ? 16 : 0), 120 + ((this.readyToJump < 0 && (base.focusedOnFarmers || this.invincibleCountdown > 0)) ? 24 : 0), 16, 24)), Color.White * ((this.FacingDirection == 0) ? 0.5f : 1f), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, this.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f + 0.0001f)));
					}
					if (this.isGlowing)
					{
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, (float)(boundsHeight / 2 + this.yOffset)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, this.scale.Value), SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
					}
				}
				if (this.pursuingMate.Value)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(-32 + this.yOffset)), new Rectangle?(new Rectangle(16, 120, 8, 8)), Color.White, 0f, new Vector2(3f, 3f), 4f, SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
					return;
				}
				if (this.avoidingMate.Value)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(-32 + this.yOffset)), new Rectangle?(new Rectangle(24, 120, 8, 8)), Color.White, 0f, new Vector2(4f, 4f), 4f, SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
				}
			}
		}

		// Token: 0x060023A2 RID: 9122 RVA: 0x001845F8 File Offset: 0x001827F8
		public void moveTowardOtherSlime(GreenSlime other, bool moveAway, GameTime time)
		{
			Point curPixel = base.StandingPixel;
			Point otherPixel = other.StandingPixel;
			int xToGo = Math.Abs(otherPixel.X - curPixel.X);
			int yToGo = Math.Abs(otherPixel.Y - curPixel.Y);
			if (xToGo > 4 || yToGo > 4)
			{
				int dx = (otherPixel.X > curPixel.X) ? 1 : -1;
				int dy = (otherPixel.Y > curPixel.Y) ? 1 : -1;
				if (moveAway)
				{
					dx = -dx;
					dy = -dy;
				}
				double chanceForX = (double)xToGo / (double)(xToGo + yToGo);
				if (Game1.random.NextDouble() < chanceForX)
				{
					base.tryToMoveInDirection((dx > 0) ? 1 : 3, false, base.DamageToFarmer, false);
				}
				else
				{
					base.tryToMoveInDirection((dy > 0) ? 2 : 0, false, base.DamageToFarmer, false);
				}
			}
			this.Sprite.AnimateDown(time, 0, "");
			if (this.invincibleCountdown > 0)
			{
				this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
		}

		// Token: 0x060023A3 RID: 9123 RVA: 0x00184703 File Offset: 0x00182903
		public void doneMating()
		{
			this.readyToMate = 120000;
			this.matingCountdown = 2000;
			this.mate = null;
			this.pursuingMate.Value = false;
			this.avoidingMate.Value = false;
		}

		// Token: 0x060023A4 RID: 9124 RVA: 0x0018473A File Offset: 0x0018293A
		public override void noMovementProgressNearPlayerBehavior()
		{
			base.faceGeneralDirection(base.Player.getStandingPosition(), 0, false);
		}

		// Token: 0x060023A5 RID: 9125 RVA: 0x00184750 File Offset: 0x00182950
		public void mateWith(GreenSlime mateToPursue, GameLocation location)
		{
			if (location.canSlimeMateHere())
			{
				GreenSlime baby = new GreenSlime(Vector2.Zero);
				Utility.recursiveFindPositionForCharacter(baby, location, base.Tile, 30);
				Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 10.0, (double)this.scale.Value * 100.0, (double)mateToPursue.scale.Value * 100.0, 0.0);
				switch (r.Next(4))
				{
				case 0:
					baby.color.Value = new Color(Math.Min(255, Math.Max(0, (int)this.color.R + r.Next((int)((float)(-(float)this.color.R) * 0.25f), (int)((float)this.color.R * 0.25f)))), Math.Min(255, Math.Max(0, (int)this.color.G + r.Next((int)((float)(-(float)this.color.G) * 0.25f), (int)((float)this.color.G * 0.25f)))), Math.Min(255, Math.Max(0, (int)this.color.B + r.Next((int)((float)(-(float)this.color.B) * 0.25f), (int)((float)this.color.B * 0.25f)))));
					break;
				case 1:
				case 2:
					baby.color.Value = Utility.getBlendedColor(this.color.Value, mateToPursue.color.Value);
					break;
				case 3:
					baby.color.Value = new Color(Math.Min(255, Math.Max(0, (int)mateToPursue.color.R + r.Next((int)((float)(-(float)mateToPursue.color.R) * 0.25f), (int)((float)mateToPursue.color.R * 0.25f)))), Math.Min(255, Math.Max(0, (int)mateToPursue.color.G + r.Next((int)((float)(-(float)mateToPursue.color.G) * 0.25f), (int)((float)mateToPursue.color.G * 0.25f)))), Math.Min(255, Math.Max(0, (int)mateToPursue.color.B + r.Next((int)((float)(-(float)mateToPursue.color.B) * 0.25f), (int)((float)mateToPursue.color.B * 0.25f)))));
					break;
				}
				int red = (int)baby.color.R;
				int green = (int)baby.color.G;
				int blue = (int)baby.color.B;
				baby.Name = this.name.Value;
				if (baby.Name == "Tiger Slime")
				{
					baby.makeTigerSlime(false);
				}
				else if (red > 100 && blue > 100 && green < 50)
				{
					baby.parseMonsterInfo("Sludge");
					while (r.NextDouble() < 0.1)
					{
						baby.objectsToDrop.Add("386");
					}
					if (r.NextDouble() < 0.01)
					{
						baby.objectsToDrop.Add("337");
					}
				}
				else if (red >= 200 && green < 75)
				{
					baby.parseMonsterInfo("Sludge");
				}
				else if (blue >= 200 && red < 100)
				{
					baby.parseMonsterInfo("Frost Jelly");
				}
				baby.Health = r.Choose(base.Health, mateToPursue.Health);
				baby.Health = Math.Max(1, base.Health + r.Next(-4, 5));
				baby.DamageToFarmer = r.Choose(base.DamageToFarmer, mateToPursue.DamageToFarmer);
				baby.DamageToFarmer = Math.Max(0, base.DamageToFarmer + r.Next(-1, 2));
				baby.resilience.Value = r.Choose(this.resilience.Value, mateToPursue.resilience.Value);
				baby.resilience.Value = Math.Max(0, this.resilience.Value + r.Next(-1, 2));
				baby.missChance.Value = r.Choose(this.missChance.Value, mateToPursue.missChance.Value);
				baby.missChance.Value = Math.Max(0.0, this.missChance.Value + (double)((float)r.Next(-1, 2) / 100f));
				baby.Scale = r.Choose(this.scale.Value, mateToPursue.scale.Value);
				baby.Scale = Math.Max(0.6f, Math.Min(1.5f, this.scale.Value + (float)r.Next(-2, 3) / 100f));
				baby.Slipperiness = 8;
				base.speed = r.Choose(base.speed, mateToPursue.speed);
				if (r.NextDouble() < 0.015)
				{
					base.speed = Math.Max(1, Math.Min(6, base.speed + r.Next(-1, 2)));
				}
				baby.setTrajectory(Utility.getAwayFromPositionTrajectory(baby.GetBoundingBox(), base.getStandingPosition()) / 2f);
				baby.ageUntilFullGrown.Value = 120000;
				baby.Halt();
				baby.firstGeneration.Value = false;
				if (Utility.isOnScreen(base.Position, 128))
				{
					base.currentLocation.playSound("slime", null, null, SoundContext.Default);
				}
			}
			mateToPursue.doneMating();
			this.doneMating();
		}

		// Token: 0x060023A6 RID: 9126 RVA: 0x00184D2C File Offset: 0x00182F2C
		public override List<Item> getExtraDropItems()
		{
			List<Item> extra = new List<Item>();
			if (this.name.Value != "Tiger Slime")
			{
				if (this.color.R >= 50 && this.color.R <= 100 && this.color.G >= 25 && this.color.G <= 50 && this.color.B <= 25)
				{
					extra.Add(ItemRegistry.Create("(O)388", Game1.random.Next(3, 7), 0, false));
					if (Game1.random.NextDouble() < 0.1)
					{
						extra.Add(ItemRegistry.Create("(O)709", 1, 0, false));
					}
				}
				else if (this.color.R < 80 && this.color.G < 80 && this.color.B < 80)
				{
					extra.Add(ItemRegistry.Create("(O)382", 1, 0, false));
					Random random = Utility.CreateRandom((double)base.Position.X * 777.0, (double)base.Position.Y * 77.0, Game1.stats.DaysPlayed, 0.0, 0.0);
					if (random.NextDouble() < 0.05)
					{
						extra.Add(ItemRegistry.Create("(O)553", 1, 0, false));
					}
					if (random.NextDouble() < 0.05)
					{
						extra.Add(ItemRegistry.Create("(O)539", 1, 0, false));
					}
				}
				else if (this.color.R > 200 && this.color.G > 180 && this.color.B < 50)
				{
					extra.Add(ItemRegistry.Create("(O)384", 2, 0, false));
				}
				else if (this.color.R > 220 && this.color.G > 90 && this.color.G < 150 && this.color.B < 50)
				{
					extra.Add(ItemRegistry.Create("(O)378", 2, 0, false));
				}
				else if (this.color.R > 230 && this.color.G > 230 && this.color.B > 230)
				{
					if (this.color.R % 2 == 1)
					{
						extra.Add(ItemRegistry.Create("(O)338", 1, 0, false));
						if (this.color.G % 2 == 1)
						{
							extra.Add(ItemRegistry.Create("(O)338", 1, 0, false));
						}
					}
					else
					{
						extra.Add(ItemRegistry.Create("(O)380", 1, 0, false));
					}
					if ((this.color.R % 2 == 0 && this.color.G % 2 == 0 && this.color.B % 2 == 0) || this.color.Equals(Color.White))
					{
						extra.Add(new Object("72", 1, false, -1, 0));
					}
				}
				else if (this.color.R > 150 && this.color.G > 150 && this.color.B > 150)
				{
					extra.Add(ItemRegistry.Create("(O)390", 2, 0, false));
				}
				else if (this.color.R > 150 && this.color.B > 180 && this.color.G < 50 && this.specialNumber.Value % (this.firstGeneration.Value ? 4 : 2) == 0)
				{
					extra.Add(ItemRegistry.Create("(O)386", 2, 0, false));
					if (this.firstGeneration.Value && Game1.random.NextDouble() < 0.005)
					{
						extra.Add(ItemRegistry.Create("(O)485", 1, 0, false));
					}
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt") && this.specialNumber.Value == 1)
			{
				string name = base.Name;
				if (!(name == "Green Slime"))
				{
					if (!(name == "Frost Jelly"))
					{
						if (name == "Tiger Slime")
						{
							extra.Add(ItemRegistry.Create("(O)857", 1, 0, false));
						}
					}
					else
					{
						extra.Add(ItemRegistry.Create("(O)413", 1, 0, false));
					}
				}
				else
				{
					extra.Add(ItemRegistry.Create("(O)680", 1, 0, false));
				}
			}
			if (base.Name == "Tiger Slime")
			{
				if (Game1.random.NextDouble() < 0.001)
				{
					extra.Add(ItemRegistry.Create("(H)91", 1, 0, false));
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					extra.Add(ItemRegistry.Create("(O)831", 1, 0, false));
					while (Game1.random.NextBool())
					{
						extra.Add(ItemRegistry.Create("(O)831", 1, 0, false));
					}
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					extra.Add(ItemRegistry.Create("(O)829", 1, 0, false));
				}
				else if (Game1.random.NextDouble() < 0.02)
				{
					extra.Add(ItemRegistry.Create("(O)833", 1, 0, false));
					while (Game1.random.NextBool())
					{
						extra.Add(ItemRegistry.Create("(O)833", 1, 0, false));
					}
				}
				else if (Game1.random.NextDouble() < 0.006)
				{
					extra.Add(ItemRegistry.Create("(O)835", 1, 0, false));
				}
			}
			if (this.prismatic.Value)
			{
				if ((from x in Game1.player.team.specialOrders
				where x.questKey.Value == "Wizard2"
				select x) != null)
				{
					Object o = ItemRegistry.Create<Object>("(O)876", 1, 0, false);
					o.specialItem = true;
					o.questItem.Value = true;
					return new List<Item>
					{
						o
					};
				}
			}
			return extra;
		}

		// Token: 0x060023A7 RID: 9127 RVA: 0x0018538C File Offset: 0x0018358C
		public override void dayUpdate(int dayOfMonth)
		{
			if (this.ageUntilFullGrown.Value > 0)
			{
				this.ageUntilFullGrown.Value /= 2;
			}
			if (this.readyToMate > 0)
			{
				this.readyToMate /= 2;
			}
			base.dayUpdate(dayOfMonth);
		}

		// Token: 0x060023A8 RID: 9128 RVA: 0x001853D8 File Offset: 0x001835D8
		protected override void updateAnimation(GameTime time)
		{
			if (this.wagTimer > 0)
			{
				this.wagTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.stunTime.Value > 0)
			{
				this.yOffset = 0;
			}
			else
			{
				this.yOffset = Math.Max(this.yOffset - (int)Math.Abs(this.xVelocity + this.yVelocity) / 2, -64);
				if (this.yOffset < 0)
				{
					this.yOffset = Math.Min(0, this.yOffset + 4 + (int)((this.yOffset <= -64) ? ((float)(-(float)this.yOffset) / 8f) : ((float)(-(float)this.yOffset) / 16f)));
				}
				this.timeSinceLastJump += time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.random.NextDouble() < 0.01 && this.wagTimer <= 0)
			{
				this.wagTimer = 992;
			}
			if (Math.Abs(this.xVelocity) >= 0.5f || Math.Abs(this.yVelocity) >= 0.5f)
			{
				this.Sprite.AnimateDown(time, 0, "");
			}
			else if (!base.Position.Equals(this.lastPosition))
			{
				this.animateTimer = 500;
			}
			if (this.animateTimer > 0 && this.readyToJump <= 0)
			{
				this.animateTimer -= time.ElapsedGameTime.Milliseconds;
				this.Sprite.AnimateDown(time, 0, "");
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x060023A9 RID: 9129 RVA: 0x00185576 File Offset: 0x00183776
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			this.jumpEvent.Poll();
			this.attackedEvent.Poll();
		}

		// Token: 0x060023AA RID: 9130 RVA: 0x00185598 File Offset: 0x00183798
		public override void behaviorAtGameTick(GameTime time)
		{
			if (this.mate == null)
			{
				this.pursuingMate.Value = false;
				this.avoidingMate.Value = false;
			}
			switch (this.FacingDirection)
			{
			case 0:
				if (this.facePosition.X > 0f)
				{
					this.facePosition.X -= 2f;
				}
				else if (this.facePosition.X < 0f)
				{
					this.facePosition.X += 2f;
				}
				if (this.facePosition.Y > -8f)
				{
					this.facePosition.Y -= 2f;
				}
				break;
			case 1:
				if (this.facePosition.X < 8f)
				{
					this.facePosition.X += 2f;
				}
				if (this.facePosition.Y < 0f)
				{
					this.facePosition.Y += 2f;
				}
				break;
			case 2:
				if (this.facePosition.X > 0f)
				{
					this.facePosition.X -= 2f;
				}
				else if (this.facePosition.X < 0f)
				{
					this.facePosition.X += 2f;
				}
				if (this.facePosition.Y < 0f)
				{
					this.facePosition.Y += 2f;
				}
				break;
			case 3:
				if (this.facePosition.X > -8f)
				{
					this.facePosition.X -= 2f;
				}
				if (this.facePosition.Y < 0f)
				{
					this.facePosition.Y += 2f;
				}
				break;
			}
			if (this.stackedSlimes.Value <= 0)
			{
				if (this.ageUntilFullGrown.Value <= 0)
				{
					this.readyToMate -= time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					this.ageUntilFullGrown.Value -= time.ElapsedGameTime.Milliseconds;
				}
			}
			if (this.pursuingMate.Value && this.mate != null)
			{
				if (this.readyToMate <= -35000)
				{
					this.mate.doneMating();
					this.doneMating();
					return;
				}
				this.moveTowardOtherSlime(this.mate, false, time);
				if (this.mate.mate != null && this.mate.pursuingMate.Value && !this.mate.mate.Equals(this))
				{
					this.doneMating();
					return;
				}
				Vector2 curStandingPosition = base.getStandingPosition();
				Vector2 mateStandingPosition = this.mate.getStandingPosition();
				if (Vector2.Distance(curStandingPosition, mateStandingPosition) < (float)(this.GetBoundingBox().Width + 4))
				{
					if (this.mate.mate != null && this.mate.avoidingMate.Value && this.mate.mate.Equals(this))
					{
						this.mate.avoidingMate.Value = false;
						this.mate.matingCountdown = 2000;
						this.mate.pursuingMate.Value = true;
					}
					this.matingCountdown -= time.ElapsedGameTime.Milliseconds;
					if (base.currentLocation != null && this.matingCountdown <= 0 && this.pursuingMate.Value && (!base.currentLocation.isOutdoors.Value || Utility.getNumberOfCharactersInRadius(base.currentLocation, Utility.Vector2ToPoint(base.Position), 1) <= 4))
					{
						this.mateWith(this.mate, base.currentLocation);
						return;
					}
				}
				else if (Vector2.Distance(curStandingPosition, mateStandingPosition) > (float)(GreenSlime.matingRange * 2))
				{
					this.mate.mate = null;
					this.mate.avoidingMate.Value = false;
					this.mate = null;
					return;
				}
			}
			else
			{
				if (this.avoidingMate.Value && this.mate != null)
				{
					this.moveTowardOtherSlime(this.mate, true, time);
					return;
				}
				if (this.readyToMate < 0 && this.cute.Value)
				{
					this.readyToMate = -1;
					if (Game1.random.NextDouble() < 0.001)
					{
						Point standingPixel = base.StandingPixel;
						GreenSlime newMate = (GreenSlime)Utility.checkForCharacterWithinArea(base.GetType(), base.Position, base.currentLocation, new Rectangle(standingPixel.X - GreenSlime.matingRange, standingPixel.Y - GreenSlime.matingRange, GreenSlime.matingRange * 2, GreenSlime.matingRange * 2));
						if (newMate != null && newMate.readyToMate <= 0 && !newMate.cute.Value && newMate.stackedSlimes.Value <= 0)
						{
							this.matingCountdown = 2000;
							this.mate = newMate;
							this.pursuingMate.Value = true;
							newMate.mate = this;
							newMate.avoidingMate.Value = true;
							this.addedSpeed = 1f;
							this.mate.addedSpeed = 1f;
							return;
						}
					}
				}
				else if (!this.isGlowing)
				{
					this.addedSpeed = 0f;
				}
				base.behaviorAtGameTick(time);
				if (this.readyToJump != -1)
				{
					this.Halt();
					base.IsWalkingTowardPlayer = false;
					this.readyToJump -= time.ElapsedGameTime.Milliseconds;
					this.Sprite.currentFrame = 16 + (800 - this.readyToJump) / 200;
					if (this.readyToJump <= 0)
					{
						this.timeSinceLastJump = this.timeSinceLastJump;
						base.Slipperiness = 10;
						base.IsWalkingTowardPlayer = true;
						this.readyToJump = -1;
						this.invincibleCountdown = 0;
						Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), base.Player);
						trajectory.X = -trajectory.X / 2f;
						trajectory.Y = -trajectory.Y / 2f;
						this.jumpEvent.Fire(trajectory);
						base.setTrajectory((int)trajectory.X, (int)trajectory.Y);
						return;
					}
				}
				else if (Game1.random.NextDouble() < 0.1 && !base.focusedOnFarmers)
				{
					if (this.FacingDirection == 0 || this.FacingDirection == 2)
					{
						if (this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(3), Game1.viewport, false, 1, false, this))
						{
							this.position.X -= (float)base.speed;
						}
						else if (!this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(1), Game1.viewport, false, 1, false, this))
						{
							this.position.X += (float)base.speed;
						}
					}
					else if (this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(0), Game1.viewport, false, 1, false, this))
					{
						this.position.Y -= (float)base.speed;
					}
					else if (!this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(2), Game1.viewport, false, 1, false, this))
					{
						this.position.Y += (float)base.speed;
					}
					if (Game1.random.NextDouble() < 0.08)
					{
						this.leftDrift.Value = !this.leftDrift.Value;
						return;
					}
				}
				else if (this.withinPlayerThreshold() && this.timeSinceLastJump > (base.focusedOnFarmers ? 1000 : 4000) && Game1.random.NextDouble() < 0.01 && this.stackedSlimes.Value <= 0)
				{
					if (base.Name.Equals("Frost Jelly") && Game1.random.NextDouble() < 0.25)
					{
						this.addedSpeed = 2f;
						base.startGlowing(Color.Cyan, false, 0.15f);
						return;
					}
					this.addedSpeed = 0f;
					base.stopGlowing();
					this.readyToJump = 800;
				}
			}
		}

		// Token: 0x060023AB RID: 9131 RVA: 0x00185E30 File Offset: 0x00184030
		private void doJump(Vector2 trajectory)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				base.currentLocation.localSound("slime", null, null, SoundContext.Default);
			}
			this.Sprite.currentFrame = 1;
		}

		// Token: 0x04001512 RID: 5394
		public const float mutationFactor = 0.25f;

		// Token: 0x04001513 RID: 5395
		public const int matingInterval = 120000;

		// Token: 0x04001514 RID: 5396
		public const int childhoodLength = 120000;

		// Token: 0x04001515 RID: 5397
		public const int durationOfMating = 2000;

		// Token: 0x04001516 RID: 5398
		public const double chanceToMate = 0.001;

		// Token: 0x04001517 RID: 5399
		public static int matingRange = 192;

		// Token: 0x04001518 RID: 5400
		public const int AQUA_SLIME = 9999899;

		// Token: 0x04001519 RID: 5401
		public NetIntDelta stackedSlimes = new NetIntDelta(0)
		{
			Minimum = new int?(0)
		};

		// Token: 0x0400151A RID: 5402
		public float randomStackOffset;

		// Token: 0x0400151B RID: 5403
		[XmlIgnore]
		public NetEvent1Field<Vector2, NetVector2> attackedEvent = new NetEvent1Field<Vector2, NetVector2>();

		// Token: 0x0400151C RID: 5404
		[XmlElement("leftDrift")]
		public readonly NetBool leftDrift = new NetBool();

		// Token: 0x0400151D RID: 5405
		[XmlElement("cute")]
		public readonly NetBool cute = new NetBool(true);

		// Token: 0x0400151E RID: 5406
		[XmlIgnore]
		public int readyToJump = -1;

		// Token: 0x0400151F RID: 5407
		[XmlIgnore]
		public int matingCountdown;

		// Token: 0x04001520 RID: 5408
		[XmlIgnore]
		public new int yOffset;

		// Token: 0x04001521 RID: 5409
		[XmlIgnore]
		public int wagTimer;

		// Token: 0x04001522 RID: 5410
		public int readyToMate = 120000;

		// Token: 0x04001523 RID: 5411
		[XmlElement("ageUntilFullGrown")]
		public readonly NetInt ageUntilFullGrown = new NetInt();

		// Token: 0x04001524 RID: 5412
		public int animateTimer;

		// Token: 0x04001525 RID: 5413
		public int timeSinceLastJump;

		// Token: 0x04001526 RID: 5414
		[XmlElement("specialNumber")]
		public readonly NetInt specialNumber = new NetInt();

		// Token: 0x04001527 RID: 5415
		[XmlElement("firstGeneration")]
		public readonly NetBool firstGeneration = new NetBool();

		// Token: 0x04001528 RID: 5416
		[XmlElement("color")]
		public readonly NetColor color = new NetColor();

		// Token: 0x04001529 RID: 5417
		private readonly NetBool pursuingMate = new NetBool();

		// Token: 0x0400152A RID: 5418
		private readonly NetBool avoidingMate = new NetBool();

		// Token: 0x0400152B RID: 5419
		private GreenSlime mate;

		// Token: 0x0400152C RID: 5420
		public readonly NetBool prismatic = new NetBool();

		// Token: 0x0400152D RID: 5421
		private readonly NetVector2 facePosition = new NetVector2();

		// Token: 0x0400152E RID: 5422
		private readonly NetEvent1Field<Vector2, NetVector2> jumpEvent = new NetEvent1Field<Vector2, NetVector2>
		{
			InterpolationWait = false
		};
	}
}
