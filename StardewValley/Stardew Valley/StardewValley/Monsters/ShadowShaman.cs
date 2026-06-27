using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x02000229 RID: 553
	public class ShadowShaman : Monster
	{
		// Token: 0x06002491 RID: 9361 RVA: 0x0018FC0C File Offset: 0x0018DE0C
		public ShadowShaman()
		{
		}

		// Token: 0x06002492 RID: 9362 RVA: 0x0018FC2C File Offset: 0x0018DE2C
		public ShadowShaman(Vector2 position) : base("Shadow Shaman", position)
		{
			Friendship friendship;
			if (Game1.MasterPlayer.friendshipData.TryGetValue("???", out friendship) && friendship.Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x0018FC87 File Offset: 0x0018DE87
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.casting, "casting");
		}

		// Token: 0x06002494 RID: 9364 RVA: 0x0018FCA6 File Offset: 0x0018DEA6
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Shaman");
		}

		// Token: 0x06002495 RID: 9365 RVA: 0x0018FCB8 File Offset: 0x0018DEB8
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.casting.Value)
			{
				for (int i = 0; i < 8; i++)
				{
					b.Draw(Projectile.projectileSheet, Game1.GlobalToLocal(Game1.viewport, base.getStandingPosition()), new Rectangle?(new Rectangle(119, 6, 3, 3)), Color.White * 0.7f, this.rotationTimer + (float)i * 3.1415927f / 4f, new Vector2(8f, 48f), 6f, SpriteEffects.None, 0.95f);
				}
			}
		}

		// Token: 0x06002496 RID: 9366 RVA: 0x0018FD50 File Offset: 0x0018DF50
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
				if (this.casting.Value && Game1.random.NextBool())
				{
					this.coolDown += 200;
				}
				else
				{
					base.setTrajectory(xTrajectory, yTrajectory);
					base.currentLocation.playSound("shadowHit", null, null, SoundContext.Default);
				}
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("shadowDie", null, null, SoundContext.Default);
					base.deathAnimation();
				}
			}
			return actualDamage;
		}

		// Token: 0x06002497 RID: 9367 RVA: 0x0018FE38 File Offset: 0x0018E038
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 2, this.Sprite.SourceRect.Y + 5, 16, 5), 10, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 10, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		}

		// Token: 0x06002498 RID: 9368 RVA: 0x0018FF5C File Offset: 0x0018E15C
		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0), base.currentLocation, 4, 64, 64);
			for (int i = 1; i < 3; i++)
			{
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, -1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, -1f) * 64f * (float)i, Color.Gray * 0.75f, 10, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = i * 159
				});
			}
		}

		// Token: 0x06002499 RID: 9369 RVA: 0x00190168 File Offset: 0x0018E368
		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.casting.Value)
			{
				this.Sprite.Animate(time, 16, 4, 200f);
				this.rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * 0.024543693f / 24f) % 3216.990877275948);
			}
			if (!this.isMoving())
			{
				this.Sprite.StopAnimation();
				return;
			}
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

		// Token: 0x0600249A RID: 9370 RVA: 0x00190244 File Offset: 0x0018E444
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.timeBeforeAIMovementAgain <= 0f)
			{
				base.IsInvisible = false;
			}
			if (!this.spottedPlayer && Utility.couldSeePlayerInPeripheralVision(base.Player, this) && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
			{
				this.controller = null;
				this.spottedPlayer = true;
				this.Halt();
				base.facePlayer(base.Player);
				if (Game1.random.NextDouble() < 0.3)
				{
					base.currentLocation.playSound("shadowpeep", null, null, SoundContext.Default);
					return;
				}
			}
			else if (this.casting.Value)
			{
				base.IsWalkingTowardPlayer = false;
				this.Sprite.Animate(time, 16, 4, 200f);
				this.rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * 0.024543693f / 24f) % 3216.990877275948);
				this.coolDown -= time.ElapsedGameTime.Milliseconds;
				if (this.coolDown <= 0)
				{
					base.Scale = 1f;
					Rectangle monsterBounds = this.GetBoundingBox();
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(monsterBounds.Center, 15f, base.Player);
					if (base.Player.Attack >= 0 && Game1.random.NextDouble() < 0.6)
					{
						base.currentLocation.projectiles.Add(new DebuffingProjectile("14", 7, 4, 4, 0.19634955f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)monsterBounds.X, (float)monsterBounds.Y), base.currentLocation, this, false, true));
					}
					else
					{
						List<Monster> monstersNearPlayer = new List<Monster>();
						foreach (NPC npc in base.currentLocation.characters)
						{
							Monster monster = npc as Monster;
							if (monster != null && monster.withinPlayerThreshold(6))
							{
								monstersNearPlayer.Add(monster);
							}
						}
						Monster lowestHealthMonster = null;
						double lowestHealth = 1.0;
						foreach (Monster i in monstersNearPlayer)
						{
							if ((double)i.Health / (double)i.MaxHealth <= lowestHealth)
							{
								lowestHealthMonster = i;
								lowestHealth = (double)i.Health / (double)i.MaxHealth;
							}
						}
						if (lowestHealthMonster != null)
						{
							int amountToHeal = this.isHardModeMonster.Value ? 250 : 60;
							lowestHealthMonster.Health = Math.Min(lowestHealthMonster.MaxHealth, lowestHealthMonster.Health + amountToHeal);
							base.currentLocation.playSound("healSound", null, null, SoundContext.Default);
							Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 256, 64, 64), 40f, 8, 0, lowestHealthMonster.Position + new Vector2(32f, 64f), false, false)
							});
							base.currentLocation.debris.Add(new Debris(amountToHeal, new Vector2((float)lowestHealthMonster.GetBoundingBox().Center.X, (float)lowestHealthMonster.GetBoundingBox().Center.Y), Color.Green, 1f, lowestHealthMonster));
						}
					}
					this.casting.Value = false;
					this.coolDown = 1500;
					base.IsWalkingTowardPlayer = true;
					return;
				}
			}
			else if (this.spottedPlayer)
			{
				if (this.withinPlayerThreshold(8))
				{
					if (base.Health < 30)
					{
						base.IsWalkingTowardPlayer = false;
						Point monsterPixel = base.StandingPixel;
						Point playerPixel = base.Player.StandingPixel;
						if (Math.Abs(playerPixel.Y - monsterPixel.Y) > 192)
						{
							if (playerPixel.X - monsterPixel.X > 0)
							{
								this.SetMovingLeft(true);
							}
							else
							{
								this.SetMovingRight(true);
							}
						}
						else if (playerPixel.Y - monsterPixel.Y > 0)
						{
							this.SetMovingUp(true);
						}
						else
						{
							this.SetMovingDown(true);
						}
					}
					else if (this.controller == null && !Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
					{
						this.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 300);
						PathFindController controller = this.controller;
						if (((controller != null) ? controller.pathToEndPoint : null) == null || this.controller.pathToEndPoint.Count == 0)
						{
							this.spottedPlayer = false;
							this.Halt();
							this.controller = null;
							this.addedSpeed = 0f;
						}
					}
					else if (this.coolDown <= 0 && Game1.random.NextDouble() < 0.02)
					{
						this.casting.Value = true;
						this.controller = null;
						base.IsWalkingTowardPlayer = false;
						this.Halt();
						this.coolDown = 500;
					}
					this.coolDown -= time.ElapsedGameTime.Milliseconds;
					return;
				}
				base.IsWalkingTowardPlayer = false;
				this.spottedPlayer = false;
				this.controller = null;
				this.addedSpeed = 0f;
				return;
			}
			else
			{
				this.defaultMovementBehavior(time);
			}
		}

		// Token: 0x04001594 RID: 5524
		public const int visionDistance = 8;

		// Token: 0x04001595 RID: 5525
		public const int spellCooldown = 1500;

		// Token: 0x04001596 RID: 5526
		[XmlIgnore]
		public bool spottedPlayer;

		// Token: 0x04001597 RID: 5527
		[XmlIgnore]
		public readonly NetBool casting = new NetBool();

		// Token: 0x04001598 RID: 5528
		[XmlIgnore]
		public int coolDown = 1500;

		// Token: 0x04001599 RID: 5529
		[XmlIgnore]
		public float rotationTimer;
	}
}
