using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace StardewValley.Projectiles
{
	// Token: 0x02000198 RID: 408
	public class BasicProjectile : Projectile
	{
		// Token: 0x06001D1A RID: 7450 RVA: 0x0014DB28 File Offset: 0x0014BD28
		public BasicProjectile()
		{
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x0014DB78 File Offset: 0x0014BD78
		public BasicProjectile(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound = null, string bounceSound = null, string firingSound = null, bool explode = false, bool damagesMonsters = false, GameLocation location = null, Character firer = null, BasicProjectile.onCollisionBehavior collisionBehavior = null, string shotItemId = null) : this()
		{
			this.damageToFarmer.Value = damageToFarmer;
			this.currentTileSheetIndex.Value = spriteIndex;
			this.bouncesLeft.Value = bouncesTillDestruct;
			this.tailLength.Value = tailLength;
			this.rotationVelocity.Value = rotationVelocity;
			this.xVelocity.Value = xVelocity;
			this.yVelocity.Value = yVelocity;
			this.position.Value = startingPosition;
			this.explode.Value = explode;
			this.collisionSound.Value = collisionSound;
			this.bounceSound.Value = bounceSound;
			this.damagesMonsters.Value = damagesMonsters;
			this.theOneWhoFiredMe.Set(location, firer);
			this.collisionBehavior = collisionBehavior;
			this.itemId.Value = (ItemRegistry.QualifyItemId(shotItemId) ?? shotItemId);
			if (!string.IsNullOrEmpty(firingSound) && location != null)
			{
				location.playSound(firingSound, null, null, SoundContext.Default);
			}
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x0014DC7C File Offset: 0x0014BE7C
		public BasicProjectile(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition) : this(damageToFarmer, spriteIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, "flameSpellHit", "flameSpell", null, true, false, null, null, null, null)
		{
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x0014DCB0 File Offset: 0x0014BEB0
		public override void updatePosition(GameTime time)
		{
			this.xVelocity.Value += this.acceleration.X;
			this.yVelocity.Value += this.acceleration.Y;
			if (this.maxVelocity.Value != -1f && Math.Sqrt((double)(this.xVelocity.Value * this.xVelocity.Value + this.yVelocity.Value * this.yVelocity.Value)) >= (double)this.maxVelocity.Value)
			{
				this.xVelocity.Value -= this.acceleration.X;
				this.yVelocity.Value -= this.acceleration.Y;
			}
			this.position.X += this.xVelocity.Value;
			this.position.Y += this.yVelocity.Value;
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x0014DDC0 File Offset: 0x0014BFC0
		protected override void InitNetFields()
		{
			base.InitNetFields();
			base.NetFields.AddField(this.damageToFarmer, "damageToFarmer").AddField(this.collisionSound, "collisionSound").AddField(this.explode, "explode").AddField(this.debuff, "debuff").AddField(this.debuffSound, "debuffSound");
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x0014DE2C File Offset: 0x0014C02C
		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			if (!this.damagesMonsters.Value)
			{
				if (this.debuff.Value != null && player.CanBeDamaged() && Game1.random.Next(11) >= player.Immunity && !player.hasBuff("28") && !player.hasTrinketWithID("BasiliskPaw"))
				{
					if (Game1.player == player)
					{
						player.applyBuff(this.debuff.Value);
					}
					location.playSound(this.debuffSound.Value, null, null, SoundContext.Default);
				}
				if (player.CanBeDamaged())
				{
					NetInt piercesLeft = this.piercesLeft;
					int value = piercesLeft.Value;
					piercesLeft.Value = value - 1;
				}
				player.takeDamage(this.damageToFarmer.Value, false, null);
				this.explosionAnimation(location);
			}
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x0014DF00 File Offset: 0x0014C100
		public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
		{
			t.performUseAction(tileLocation);
			this.explosionAnimation(location);
			NetInt piercesLeft = this.piercesLeft;
			int value = piercesLeft.Value;
			piercesLeft.Value = value - 1;
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x0014DF34 File Offset: 0x0014C134
		public override void behaviorOnCollisionWithOther(GameLocation location)
		{
			if (!this.ignoreObjectCollisions.Value)
			{
				this.explosionAnimation(location);
				NetInt piercesLeft = this.piercesLeft;
				int value = piercesLeft.Value;
				piercesLeft.Value = value - 1;
			}
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x0014DF6C File Offset: 0x0014C16C
		public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
		{
			if (this.damagesMonsters.Value)
			{
				Farmer player = this.GetPlayerWhoFiredMe(location);
				this.explosionAnimation(location);
				if (n is Monster)
				{
					location.damageMonster(n.GetBoundingBox(), this.damageToFarmer.Value, this.damageToFarmer.Value + 1, false, player, true);
					if (this.currentTileSheetIndex.Value == 15)
					{
						Utility.addRainbowStarExplosion(location, this.position.Value, 11);
					}
					if (!(n as Monster).IsInvisible)
					{
						NetInt piercesLeft = this.piercesLeft;
						int value = piercesLeft.Value;
						piercesLeft.Value = value - 1;
						return;
					}
				}
				else if (this.itemId.Value != null)
				{
					n.getHitByPlayer(player, location);
					string projectileTokenizedName = TokenStringBuilder.ItemName(this.itemId.Value, null);
					Game1.multiplayer.globalChatInfoMessage("Slingshot_Hit", new string[]
					{
						player.Name,
						n.GetTokenizedDisplayName(),
						Lexicon.prependTokenizedArticle(projectileTokenizedName)
					});
					NetInt piercesLeft2 = this.piercesLeft;
					int value = piercesLeft2.Value;
					piercesLeft2.Value = value - 1;
				}
			}
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x0014E07C File Offset: 0x0014C27C
		protected virtual void explosionAnimation(GameLocation location)
		{
			if (this.projectileID.Value == 14)
			{
				for (int i = 0; i < 12; i++)
				{
					Vector2 motion = new Vector2(0f, -1.5f + (float)Game1.random.Next(-10, 11) / 12f);
					motion = Vector2.Transform(motion, Matrix.CreateRotationZ((float)(0.5235987755982988 + (double)((float)Game1.random.Next(-10, 11) / 50f)) * (float)i));
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), 80f, 6, 1, this.position.Value + new Vector2(8f, 8f) * 4f, false, false, 1f, 0f, Utility.Get2PhaseColor(Color.White, Color.Cyan, 0, 1f, (float)Game1.random.Next(1000)), 4f, 0f, 0f, 0f, false)
					{
						drawAboveAlwaysFront = true,
						motion = motion
					});
				}
			}
			else
			{
				Rectangle sourceRect = base.GetSourceRect();
				sourceRect.X += 4;
				sourceRect.Y += 4;
				sourceRect.Width = 8;
				sourceRect.Height = 8;
				if (this.itemId.Value != null)
				{
					int whichDebris = 12;
					string value = this.itemId.Value;
					if (!(value == "(O)390"))
					{
						if (!(value == "(O)378"))
						{
							if (!(value == "(O)380"))
							{
								if (!(value == "(O)384"))
								{
									if (!(value == "(O)386"))
									{
										if (value == "(O)382")
										{
											whichDebris = 4;
										}
									}
									else
									{
										whichDebris = 10;
									}
								}
								else
								{
									whichDebris = 6;
								}
							}
							else
							{
								whichDebris = 2;
							}
						}
						else
						{
							whichDebris = 0;
						}
					}
					else
					{
						whichDebris = 14;
					}
					Game1.createRadialDebris(location, whichDebris, (int)(this.position.X + 32f) / 64, (int)(this.position.Y + 32f) / 64, 6, false, -1, false, null);
				}
				else
				{
					Game1.createRadialDebris_MoreNatural(location, "TileSheets\\Projectiles", sourceRect, 1, (int)this.position.X + 32, (int)this.position.Y + 32, 6, (int)(this.position.Y / 64f) + 1);
				}
			}
			if (!string.IsNullOrEmpty(this.collisionSound.Value))
			{
				location.playSound(this.collisionSound.Value, null, null, SoundContext.Default);
			}
			if (this.explode.Value)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, this.position.Value, false, Game1.random.NextBool())
				});
			}
			BasicProjectile.onCollisionBehavior onCollisionBehavior = this.collisionBehavior;
			if (onCollisionBehavior != null)
			{
				onCollisionBehavior(location, this.getBoundingBox().Center.X, this.getBoundingBox().Center.Y, this.GetPlayerWhoFiredMe(location));
			}
			this.destroyMe = true;
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x0014E3C4 File Offset: 0x0014C5C4
		public static void explodeOnImpact(GameLocation location, int x, int y, Character who)
		{
			location.explode(new Vector2((float)(x / 64), (float)(y / 64)), 2, who as Farmer, true, -1, true);
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x0014E3E5 File Offset: 0x0014C5E5
		public virtual Farmer GetPlayerWhoFiredMe(GameLocation location)
		{
			return (this.theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;
		}

		// Token: 0x040011CC RID: 4556
		public readonly NetInt damageToFarmer = new NetInt();

		// Token: 0x040011CD RID: 4557
		public readonly NetString collisionSound = new NetString();

		// Token: 0x040011CE RID: 4558
		public readonly NetBool explode = new NetBool();

		// Token: 0x040011CF RID: 4559
		public BasicProjectile.onCollisionBehavior collisionBehavior;

		// Token: 0x040011D0 RID: 4560
		public NetString debuff = new NetString(null);

		// Token: 0x040011D1 RID: 4561
		public NetString debuffSound = new NetString("debuffHit");

		// Token: 0x02000548 RID: 1352
		// (Invoke) Token: 0x0600412C RID: 16684
		public delegate void onCollisionBehavior(GameLocation location, int xPosition, int yPosition, Character who);
	}
}
