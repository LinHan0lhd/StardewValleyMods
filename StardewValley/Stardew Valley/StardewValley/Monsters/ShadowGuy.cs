using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	// Token: 0x02000228 RID: 552
	public class ShadowGuy : Monster
	{
		// Token: 0x06002488 RID: 9352 RVA: 0x0018F08F File Offset: 0x0018D28F
		public ShadowGuy()
		{
		}

		// Token: 0x06002489 RID: 9353 RVA: 0x0018F0A4 File Offset: 0x0018D2A4
		public ShadowGuy(Vector2 position) : base("Shadow Guy", position)
		{
			Friendship friendship;
			if (Game1.MasterPlayer.friendshipData.TryGetValue("???", out friendship) && friendship.Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
			this.Halt();
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x0018F0FA File Offset: 0x0018D2FA
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow " + ((base.Position.X % 4f == 0f) ? "Girl" : "Guy"));
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x0018F138 File Offset: 0x0018D338
		public override void draw(SpriteBatch b)
		{
			if (!this.casting)
			{
				base.draw(b);
				return;
			}
			Vector2 standingPosition = base.getStandingPosition();
			int standingY = (int)standingPosition.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-8, 9)), (float)(64 + Game1.random.Next(-8, 9))), new Rectangle?(this.Sprite.SourceRect), Color.White * 0.5f, this.rotation, new Vector2(32f, 64f), Math.Max(0.2f, this.scale.Value), this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-8, 9)), (float)(64 + Game1.random.Next(-8, 9))), new Rectangle?(this.Sprite.SourceRect), Color.White * 0.5f, this.rotation, new Vector2(32f, 64f), Math.Max(0.2f, this.scale.Value), this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(standingY + 1) / 10000f)));
			Vector2 projectilePosition = Game1.GlobalToLocal(Game1.viewport, standingPosition);
			Rectangle projectileSourceRect = new Rectangle(212, 20, 24, 24);
			Color projectileColor = Color.White * 0.7f;
			Vector2 projectileOrigin = new Vector2(32f, 256f);
			for (int i = 0; i < 8; i++)
			{
				b.Draw(Projectile.projectileSheet, projectilePosition, new Rectangle?(projectileSourceRect), projectileColor, this.rotationTimer + (float)i * 3.1415927f / 4f, projectileOrigin, 1.5f, SpriteEffects.None, 0.95f);
			}
		}

		// Token: 0x0600248C RID: 9356 RVA: 0x0018F370 File Offset: 0x0018D570
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
				if (this.casting && Game1.random.NextBool())
				{
					this.coolDown += 200;
				}
				else if (Game1.random.NextDouble() < 0.4 + 1.0 / (double)base.Health && !base.currentLocation.IsFarm)
				{
					this.castTeleport();
					if (base.Health <= 10)
					{
						base.speed = Math.Min(3, base.speed + 1);
					}
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

		// Token: 0x0600248D RID: 9357 RVA: 0x0018F4B0 File Offset: 0x0018D6B0
		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10, false, 100f, 0, -1, -1f, -1, 0));
		}

		// Token: 0x0600248E RID: 9358 RVA: 0x0018F4F0 File Offset: 0x0018D6F0
		protected override void sharedDeathAnimation()
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 64, 21), 64, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 10, this.Sprite.SourceRect.Y + 21, 64, 21), 42, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x0018F5D4 File Offset: 0x0018D7D4
		public void castTeleport()
		{
			int tries = 0;
			Vector2 curTile = base.Tile;
			Vector2 possiblePoint = new Vector2(curTile.X + (float)(Game1.random.NextBool() ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)), curTile.Y + (float)(Game1.random.NextBool() ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)));
			while (tries < 6 && (!base.currentLocation.isTileOnMap(possiblePoint) || !base.currentLocation.isTileLocationOpen(possiblePoint) || !base.currentLocation.CanSpawnCharacterHere(possiblePoint)))
			{
				possiblePoint = new Vector2(curTile.X + (float)(Game1.random.NextBool() ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)), curTile.Y + (float)(Game1.random.NextBool() ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)));
				tries++;
			}
			if (tries < 6)
			{
				this.teleporting = true;
				this.teleportationPath = Utility.GetPointsOnLine((int)curTile.X, (int)curTile.Y, (int)possiblePoint.X, (int)possiblePoint.Y, true).GetEnumerator();
				this.coolDown = 20;
			}
		}

		// Token: 0x06002490 RID: 9360 RVA: 0x0018F730 File Offset: 0x0018D930
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.timeBeforeAIMovementAgain <= 0f)
			{
				base.IsInvisible = false;
			}
			if (this.teleporting)
			{
				this.coolDown -= time.ElapsedGameTime.Milliseconds;
				if (this.coolDown <= 0)
				{
					if (this.teleportationPath.MoveNext())
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(this.Sprite.textureName.Value, this.Sprite.SourceRect, base.Position, false, 0.04f, Color.White)
						});
						base.Position = new Vector2((float)(this.teleportationPath.Current.X * 64 + 4), (float)(this.teleportationPath.Current.Y * 64 - 32 - 4));
						this.coolDown = 20;
						return;
					}
					this.teleporting = false;
					this.coolDown = 500;
					return;
				}
			}
			else if (!this.spottedPlayer && Utility.couldSeePlayerInPeripheralVision(base.Player, this) && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
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
			else if (this.casting)
			{
				this.Halt();
				base.IsWalkingTowardPlayer = false;
				this.rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * 0.024543693f / 24f) % 3216.990877275948);
				this.coolDown -= time.ElapsedGameTime.Milliseconds;
				if (this.coolDown <= 0)
				{
					Rectangle monsterBounds = this.GetBoundingBox();
					base.Scale = 1f;
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(monsterBounds.Center, 15f, base.Player);
					if (base.Player.Attack >= 0 && Game1.random.NextDouble() < 0.6)
					{
						base.currentLocation.projectiles.Add(new DebuffingProjectile("18", 2, 4, 4, 0.19634955f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)monsterBounds.X, (float)monsterBounds.Y), null, null, false, true));
					}
					else
					{
						base.currentLocation.playSound("fireball", null, null, SoundContext.Default);
						base.currentLocation.projectiles.Add(new BasicProjectile(10, 3, 0, 3, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)monsterBounds.X, (float)monsterBounds.Y)));
					}
					this.casting = false;
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
						this.casting = true;
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

		// Token: 0x0400158C RID: 5516
		public const int visionDistance = 8;

		// Token: 0x0400158D RID: 5517
		public const int spellCooldown = 1500;

		// Token: 0x0400158E RID: 5518
		[XmlIgnore]
		public bool spottedPlayer;

		// Token: 0x0400158F RID: 5519
		[XmlIgnore]
		public bool casting;

		// Token: 0x04001590 RID: 5520
		[XmlIgnore]
		public bool teleporting;

		// Token: 0x04001591 RID: 5521
		[XmlIgnore]
		public int coolDown = 1500;

		// Token: 0x04001592 RID: 5522
		[XmlIgnore]
		public IEnumerator<Point> teleportationPath;

		// Token: 0x04001593 RID: 5523
		[XmlIgnore]
		public float rotationTimer;
	}
}
