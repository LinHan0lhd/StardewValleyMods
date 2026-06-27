using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Monsters
{
	// Token: 0x02000210 RID: 528
	public class Bat : Monster
	{
		// Token: 0x0600231B RID: 8987 RVA: 0x00179864 File Offset: 0x00177A64
		public Bat()
		{
		}

		// Token: 0x0600231C RID: 8988 RVA: 0x00179914 File Offset: 0x00177B14
		public Bat(Vector2 position) : base("Bat", position)
		{
			base.Slipperiness = 24 + Game1.random.Next(-10, 11);
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
		}

		// Token: 0x0600231D RID: 8989 RVA: 0x001799F4 File Offset: 0x00177BF4
		public Bat(Vector2 position, int mineLevel) : base("Bat", position)
		{
			base.Slipperiness = 20 + Game1.random.Next(-5, 6);
			if (mineLevel <= -666)
			{
				if (mineLevel == -789)
				{
					base.parseMonsterInfo("Iridium Bat");
					this.reloadSprite(false);
					this.extraVelocity = 1f;
					this.extraVelocity = 3f;
					this.maxSpeed = 4f;
					base.Health *= 2;
					this.shakeTimer = 100;
					this.cursedDoll.Value = true;
					this.objectsToDrop.Clear();
					base.Age = 789;
					goto IL_3B0;
				}
				if (mineLevel == -666)
				{
					base.parseMonsterInfo("Iridium Bat");
					this.reloadSprite(false);
					this.extraVelocity = 1f;
					this.extraVelocity = 3f;
					this.maxSpeed = 8f;
					base.Health *= 2;
					this.shakeTimer = 100;
					this.cursedDoll.Value = true;
					this.objectsToDrop.Clear();
					goto IL_3B0;
				}
			}
			else
			{
				if (mineLevel == -556)
				{
					base.parseMonsterInfo("Magma Sparker");
					base.Name = "Magma Sparker";
					this.reloadSprite(false);
					this.extraVelocity = 2f;
					base.Slipperiness += 3;
					this.maxSpeed = (float)Game1.random.Next(6, 8);
					this.shakeTimer = 100;
					this.cursedDoll.Value = true;
					this.magmaSprite.Value = true;
					this.canLunge.Value = true;
					goto IL_3B0;
				}
				if (mineLevel == -555)
				{
					base.parseMonsterInfo("Magma Sprite");
					base.Name = "Magma Sprite";
					this.reloadSprite(false);
					base.Slipperiness *= 2;
					this.extraVelocity = 2f;
					this.maxSpeed = (float)Game1.random.Next(6, 9);
					this.shakeTimer = 100;
					this.cursedDoll.Value = true;
					this.magmaSprite.Value = true;
					goto IL_3B0;
				}
				if (mineLevel == 77377)
				{
					base.parseMonsterInfo("Lava Bat");
					base.Name = "Haunted Skull";
					this.reloadSprite(false);
					this.extraVelocity = 1f;
					this.extraVelocity = 3f;
					this.maxSpeed = 8f;
					this.shakeTimer = 100;
					this.cursedDoll.Value = true;
					this.hauntedSkull.Value = true;
					this.objectsToDrop.Clear();
					goto IL_3B0;
				}
			}
			if (mineLevel >= 40 && mineLevel < 80)
			{
				base.Name = "Frost Bat";
				base.parseMonsterInfo("Frost Bat");
				this.reloadSprite(false);
			}
			else if (mineLevel >= 80 && mineLevel < 171)
			{
				base.Name = "Lava Bat";
				base.parseMonsterInfo("Lava Bat");
				this.reloadSprite(false);
			}
			else if (mineLevel >= 171)
			{
				base.Name = "Iridium Bat";
				base.parseMonsterInfo("Iridium Bat");
				this.reloadSprite(false);
				this.extraVelocity = 1f;
			}
			IL_3B0:
			if (mineLevel > 999)
			{
				this.extraVelocity = 3f;
				this.maxSpeed = 8f;
				base.Health *= 2;
				this.shakeTimer = 999999;
			}
			if (this.canLunge.Value)
			{
				this.nextLunge = this.lungeFrequency;
			}
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
		}

		// Token: 0x0600231E RID: 8990 RVA: 0x00179E18 File Offset: 0x00178018
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.wasHitCounter, "wasHitCounter").AddField(this.turningRight, "turningRight").AddField(this.seenPlayer, "seenPlayer").AddField(this.cursedDoll, "cursedDoll").AddField(this.hauntedSkull, "hauntedSkull").AddField(this.magmaSprite, "magmaSprite").AddField(this.canLunge, "canLunge");
		}

		// Token: 0x0600231F RID: 8991 RVA: 0x00179EA4 File Offset: 0x001780A4
		public override void reloadSprite(bool onlyAppearance = false)
		{
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
			}
			else
			{
				this.Sprite.textureName.Value = "Characters\\Monsters\\" + base.Name;
			}
			base.HideShadow = true;
		}

		// Token: 0x06002320 RID: 8992 RVA: 0x00179EFD File Offset: 0x001780FD
		public override Debris ModifyMonsterLoot(Debris debris)
		{
			if (debris != null && this.magmaSprite.Value)
			{
				debris.chunksMoveTowardPlayer = true;
			}
			return debris;
		}

		// Token: 0x06002321 RID: 8993 RVA: 0x00179F18 File Offset: 0x00178118
		public override List<Item> getExtraDropItems()
		{
			List<Item> extraDrops = new List<Item>();
			if (this.cursedDoll.Value && Game1.random.NextDouble() < 0.1429 && this.hauntedSkull.Value)
			{
				switch (Game1.random.Next(11))
				{
				case 0:
					switch (Game1.random.Next(6))
					{
					case 0:
					{
						Clothing v = ItemRegistry.Create<Clothing>("(P)10", 1, 0, false);
						v.clothesColor.Value = Color.DimGray;
						extraDrops.Add(v);
						break;
					}
					case 1:
						extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1004", 1, 0, false));
						break;
					case 2:
						extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1014", 1, 0, false));
						break;
					case 3:
						extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1263", 1, 0, false));
						break;
					case 4:
						extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1262", 1, 0, false));
						break;
					case 5:
					{
						Clothing v = ItemRegistry.Create<Clothing>("(P)12", 1, 0, false);
						v.clothesColor.Value = Color.DimGray;
						extraDrops.Add(v);
						break;
					}
					}
					break;
				case 1:
				{
					MeleeWeapon weapon = ItemRegistry.Create<MeleeWeapon>("(W)2", 1, 0, false);
					weapon.AddEnchantment(new VampiricEnchantment());
					extraDrops.Add(weapon);
					break;
				}
				case 2:
					extraDrops.Add(ItemRegistry.Create("(O)288", 1, 0, false));
					break;
				case 3:
					extraDrops.Add(new Ring("534"));
					break;
				case 4:
					extraDrops.Add(new Ring("531"));
					break;
				case 5:
					do
					{
						extraDrops.Add(ItemRegistry.Create("(O)768", 1, 0, false));
						extraDrops.Add(ItemRegistry.Create("(O)769", 1, 0, false));
					}
					while (Game1.random.NextDouble() < 0.33);
					break;
				case 6:
					extraDrops.Add(ItemRegistry.Create("(O)581", 1, 0, false));
					break;
				case 7:
					extraDrops.Add(ItemRegistry.Create("(O)582", 1, 0, false));
					break;
				case 8:
					extraDrops.Add(ItemRegistry.Create("(O)725", 1, 0, false));
					break;
				case 9:
					extraDrops.Add(ItemRegistry.Create("(O)86", 1, 0, false));
					break;
				case 10:
					if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccVault"))
					{
						extraDrops.Add(ItemRegistry.Create("(O)275", 1, 0, false));
					}
					else
					{
						extraDrops.Add(ItemRegistry.Create("(O)749", 1, 0, false));
					}
					break;
				}
				return extraDrops;
			}
			if (this.hauntedSkull.Value && Game1.random.NextDouble() < 0.25 && Game1.IsWinter)
			{
				do
				{
					extraDrops.Add(ItemRegistry.Create("(O)273", 1, 0, false));
				}
				while (Game1.random.NextDouble() < 0.4);
			}
			if (this.hauntedSkull.Value && Game1.random.NextDouble() < 0.01)
			{
				extraDrops.Add(ItemRegistry.Create("(M)CursedMannequin" + ((Game1.random.NextDouble() < 0.5) ? "Male" : "Female"), 1, 0, false));
			}
			if (this.hauntedSkull.Value && Game1.random.NextDouble() < 0.001502)
			{
				extraDrops.Add(ItemRegistry.Create("(O)279", 1, 0, false));
			}
			if (extraDrops.Count > 0)
			{
				return extraDrops;
			}
			return base.getExtraDropItems();
		}

		// Token: 0x06002322 RID: 8994 RVA: 0x0017A2B0 File Offset: 0x001784B0
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (base.Age == 789)
			{
				return -1;
			}
			this.lungeVelocity = Vector2.Zero;
			if (this.lungeTimer > 0)
			{
				this.nextLunge = this.lungeFrequency;
				this.lungeTimer = 0;
			}
			else if (this.nextLunge < 1000)
			{
				this.nextLunge = 1000;
			}
			int actualDamage = Math.Max(1, damage - this.resilience.Value);
			this.seenPlayer.Value = true;
			if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				this.wasHitCounter.Value = 500;
				if (this.magmaSprite.Value)
				{
					base.currentLocation.playSound("magma_sprite_hit", null, null, SoundContext.Default);
				}
				else
				{
					base.currentLocation.playSound("hitEnemy", null, null, SoundContext.Default);
				}
				if (base.Health <= 0)
				{
					base.deathAnimation();
					if (!this.magmaSprite.Value)
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(44, base.Position, Color.DarkMagenta, 10, false, 100f, 0, -1, -1f, -1, 0)
						});
					}
					if (this.cursedDoll.Value)
					{
						Vector2 pixelPosition = base.Position;
						if (this.magmaSprite.Value)
						{
							base.currentLocation.playSound("magma_sprite_die", null, null, SoundContext.Default);
							for (int i = 0; i < 20; i++)
							{
								base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Rectangle(0, 64, 8, 8), pixelPosition + new Vector2((float)Game1.random.Next(64), (float)Game1.random.Next(64)), false, 0f, Color.White)
								{
									scale = 4f,
									scaleChange = 0f,
									motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, -6f),
									acceleration = new Vector2(0f, 0.25f),
									layerDepth = 0.9f,
									animationLength = 6,
									totalNumberOfLoops = 2,
									interval = 60f,
									delayBeforeAnimationStart = i * 10
								});
							}
							Utility.addSmokePuff(base.currentLocation, pixelPosition, 0, 4f, 0.01f, 1f, 0.01f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(32f, 16f), 400, 4f, 0.01f, 1f, 0.02f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(-32f, -16f), 200, 4f, 0.01f, 1f, 0.02f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(0f, 32f), 200, 4f, 0.01f, 1f, 0.01f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition, 0, 3f, 0.01f, 1f, 0.02f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(21f, 16f), 500, 3f, 0.01f, 1f, 0.01f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(-32f, -21f), 100, 3f, 0.01f, 1f, 0.02f);
							Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(0f, 32f), 250, 3f, 0.01f, 1f, 0.01f);
						}
						else
						{
							base.currentLocation.playSound("rockGolemHit", null, null, SoundContext.Default);
						}
						if (this.hauntedSkull.Value)
						{
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 32, 16, 16), 2000f, 1, 9999, pixelPosition, false, false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2((float)xTrajectory / 4f, (float)Game1.random.Next(-12, -7)),
									acceleration = new Vector2(0f, 0.4f),
									rotationChange = (float)Game1.random.Next(-200, 200) / 1000f
								}
							});
						}
						else if (who != null && !this.magmaSprite.Value)
						{
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 22), 40f, 6, 9999, pixelPosition, false, true, 1f, 0f, Color.Black * 0.67f, 4f, 0f, 0f, 0f, false)
								{
									motion = new Vector2(8f, -4f)
								}
							});
						}
					}
					else
					{
						base.currentLocation.playSound("batScreech", null, null, SoundContext.Default);
					}
				}
			}
			this.addedSpeed = (float)Game1.random.Next(-1, 1);
			return actualDamage;
		}

		// Token: 0x06002323 RID: 8995 RVA: 0x0017A8D4 File Offset: 0x00178AD4
		public override void shedChunks(int number, float scale)
		{
			Point standingPixel = base.StandingPixel;
			if (this.cursedDoll.Value && this.hauntedSkull.Value)
			{
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 64, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
				return;
			}
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 384, 64, 64), 32, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, scale);
		}

		// Token: 0x06002324 RID: 8996 RVA: 0x0017A998 File Offset: 0x00178B98
		public override void onDealContactDamage(Farmer who)
		{
			base.onDealContactDamage(who);
			if (this.magmaSprite.Value && Game1.random.NextDouble() < 0.5 && this.name.Equals("Magma Sparker") && Game1.random.Next(11) >= who.Immunity && !who.hasBuff("28") && !who.hasTrinketWithID("BasiliskPaw"))
			{
				who.applyBuff("12");
			}
		}

		// Token: 0x06002325 RID: 8997 RVA: 0x0017AA1C File Offset: 0x00178C1C
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				if (this.cursedDoll.Value)
				{
					if (this.hauntedSkull.Value)
					{
						Vector2 pos_offset = Vector2.Zero;
						if (this.previousPositions.Count > 2)
						{
							pos_offset = base.Position - this.previousPositions[1];
						}
						int direction = (Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y)) ? ((pos_offset.X > 0f) ? 1 : 3) : ((pos_offset.Y < 0f) ? 0 : 2);
						if (direction == -1)
						{
							direction = 2;
						}
						Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));
						b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - 1f) / 10000f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f) / 10000f);
						if (this.seenPlayer.Value)
						{
							for (int i = this.previousPositions.Count - 1; i >= 0; i -= 2)
							{
								b.Draw(this.Sprite.Texture, new Vector2(this.previousPositions[i].X - (float)Game1.viewport.X, this.previousPositions[i].Y - (float)Game1.viewport.Y + (float)this.yJumpOffset) + this.drawOffset + new Vector2(32f, 32f) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.White * (0f + 0.125f * (float)i), 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - (float)i) / 10000f);
							}
						}
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f + 1f) / 10000f);
						return;
					}
					if (this.magmaSprite.Value)
					{
						Vector2 pos_offset2 = Vector2.Zero;
						if (this.previousPositions.Count > 2)
						{
							pos_offset2 = base.Position - this.previousPositions[1];
						}
						int direction2 = (Math.Abs(pos_offset2.X) > Math.Abs(pos_offset2.Y)) ? ((pos_offset2.X > 0f) ? 1 : 3) : ((pos_offset2.Y < 0f) ? 0 : 2);
						if (direction2 == -1)
						{
							direction2 = 2;
						}
						Vector2 offset2 = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));
						b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset2.Y / 20f, SpriteEffects.None, 0.0001f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction2 * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16)), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9955f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction2 * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16)), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9975f);
						for (int j = this.previousPositions.Count - 1; j >= 0; j -= 2)
						{
							b.Draw(this.Sprite.Texture, new Vector2(this.previousPositions[j].X - (float)Game1.viewport.X, this.previousPositions[j].Y - (float)Game1.viewport.Y + (float)this.yJumpOffset) + this.drawOffset + new Vector2(32f, 32f) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction2 * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16)), Color.White * (0f + 0.125f * (float)j), 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9985f);
						}
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction2 * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16)), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
						return;
					}
					int index = 103;
					if (base.Age == 789)
					{
						index = 789;
					}
					Vector2 offset3 = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset3.Y / 20f, SpriteEffects.None, 0.0001f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset3, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16)), Color.Violet * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - 1f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset3, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16)), Color.Lime * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset3, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16)), (index == 789) ? Color.White : new Color(255, 50, 50), 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f + 1f) / 10000f);
					return;
				}
				else
				{
					int standingY = base.StandingPixel.Y;
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(this.Sprite.SourceRect), (this.shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, base.wildernessFarmMonster ? 0.0001f : ((float)(standingY - 1) / 10000f));
					if (this.isGlowing)
					{
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
					}
				}
			}
		}

		// Token: 0x06002326 RID: 8998 RVA: 0x0017BA34 File Offset: 0x00179C34
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (this.wasHitCounter.Value >= 0)
			{
				this.wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}
			if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity) || base.Position.X < -2000f || base.Position.Y < -2000f)
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
			if (this.canLunge.Value)
			{
				if (this.nextLunge == -1)
				{
					this.nextLunge = this.lungeFrequency;
				}
				if (this.lungeVelocity.LengthSquared() > 0f)
				{
					float decel = (float)this.lungeSpeed / (float)this.lungeDecelerationTicks;
					this.lungeVelocity = new Vector2(Utility.MoveTowards(this.lungeVelocity.X, 0f, decel), Utility.MoveTowards(this.lungeVelocity.Y, 0f, decel));
					this.xVelocity = this.lungeVelocity.X;
					this.yVelocity = -this.lungeVelocity.Y;
					if (this.lungeVelocity.LengthSquared() == 0f)
					{
						this.xVelocity = 0f;
						this.yVelocity = 0f;
					}
					return;
				}
				if (this.lungeTimer > 0)
				{
					this.lungeTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
					Vector2 offset = Utility.PointToVector2(base.Player.StandingPixel) - Utility.PointToVector2(base.StandingPixel);
					if (offset.LengthSquared() == 0f)
					{
						offset = new Vector2(1f, 0f);
					}
					offset.Normalize();
					if (this.lungeTimer < 0)
					{
						this.lungeVelocity = offset * 25f;
						this.lungeTimer = 0;
						this.nextLunge = this.lungeFrequency;
					}
					this.xVelocity = offset.X * 0.5f;
					this.yVelocity = -offset.Y * 0.5f;
				}
				else if (this.nextLunge > 0 && this.withinPlayerThreshold(6))
				{
					this.nextLunge -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.nextLunge < 0)
					{
						base.currentLocation.playSound("magma_sprite_spot", null, null, SoundContext.Default);
						this.nextLunge = 0;
						this.lungeTimer = this.lungeChargeTime;
						return;
					}
				}
			}
			if (base.focusedOnFarmers || this.withinPlayerThreshold(6) || this.seenPlayer.Value)
			{
				if (this.magmaSprite.Value && !this.seenPlayer.Value)
				{
					base.currentLocation.playSound("magma_sprite_spot", null, null, SoundContext.Default);
				}
				this.seenPlayer.Value = true;
				if (this.invincibleCountdown > 0)
				{
					if (base.Name.Equals("Lava Bat"))
					{
						this.glowingColor = Color.Cyan;
					}
					return;
				}
				Point monsterPixel = base.StandingPixel;
				Point standingPixel = base.Player.StandingPixel;
				float xSlope = (float)(-(float)(standingPixel.X - monsterPixel.X));
				float ySlope = (float)(standingPixel.Y - monsterPixel.Y);
				float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
				if (t < (float)((this.extraVelocity > 0f) ? 192 : 64))
				{
					this.xVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.xVelocity * 1.05f));
					this.yVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.yVelocity * 1.05f));
				}
				xSlope /= t;
				ySlope /= t;
				if (this.wasHitCounter.Value <= 0)
				{
					this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.5707964f;
					if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextBool())
					{
						this.turningRight.Value = true;
					}
					else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
					{
						this.turningRight.Value = false;
					}
					if (this.turningRight.Value)
					{
						this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					else
					{
						this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.049087387f;
					}
					this.rotation %= 6.2831855f;
					this.wasHitCounter.Value = 0;
				}
				float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + this.extraVelocity;
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * this.maxSpeed))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * this.maxSpeed))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
			}
		}

		// Token: 0x06002327 RID: 8999 RVA: 0x0017C0EC File Offset: 0x0017A2EC
		protected override void updateAnimation(GameTime time)
		{
			if (base.focusedOnFarmers || this.withinPlayerThreshold(6) || this.seenPlayer.Value || this.magmaSprite.Value)
			{
				this.Sprite.Animate(time, 0, 4, 80f);
				if (this.Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(base.Position, 512) && (this.batFlap == null || !this.batFlap.IsPlaying) && base.currentLocation == Game1.currentLocation && !this.cursedDoll.Value)
				{
					Game1.playSound("batFlap", out this.batFlap);
				}
				if (this.cursedDoll.Value)
				{
					this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shakeTimer < 0)
					{
						this.shakeTimer = 50;
						if (this.magmaSprite.Value)
						{
							this.shakeTimer = ((this.lungeTimer > 0) ? 50 : 100);
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Rectangle(0, 64, 8, 8), base.Position + new Vector2((float)Game1.random.Next(32), (float)(-16 - Game1.random.Next(32))), false, 0f, Color.White)
							{
								scale = 4f,
								scaleChange = -0.05f,
								motion = new Vector2((this.lungeTimer > 0) ? ((float)Game1.random.Next(-30, 31) / 10f) : 0f, -this.maxSpeed / ((this.lungeTimer > 0) ? 2f : 8f)),
								layerDepth = 0.9f,
								animationLength = 6,
								totalNumberOfLoops = 1,
								interval = 50f,
								xPeriodic = (this.lungeTimer <= 0),
								xPeriodicLoopTime = (float)Game1.random.Next(500, 800),
								xPeriodicRange = (float)(4 * ((this.lungeTimer > 0) ? 2 : 1))
							});
						}
						else if (!this.hauntedSkull.Value)
						{
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (base.Age == 789) ? 789 : 103, 16, 16), base.Position + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
							{
								scale = 4f
							});
						}
					}
					this.previousPositions.Add(base.Position);
					if (this.previousPositions.Count > 8)
					{
						this.previousPositions.RemoveAt(0);
					}
				}
			}
			else
			{
				this.Sprite.currentFrame = 4;
				this.Halt();
			}
			base.resetAnimationSpeed();
		}

		// Token: 0x040014CC RID: 5324
		public const float rotationIncrement = 0.049087387f;

		// Token: 0x040014CD RID: 5325
		[XmlIgnore]
		public readonly NetInt wasHitCounter = new NetInt(0);

		// Token: 0x040014CE RID: 5326
		[XmlIgnore]
		public float targetRotation;

		// Token: 0x040014CF RID: 5327
		[XmlIgnore]
		public readonly NetBool turningRight = new NetBool();

		// Token: 0x040014D0 RID: 5328
		[XmlIgnore]
		public readonly NetBool seenPlayer = new NetBool();

		// Token: 0x040014D1 RID: 5329
		public readonly NetBool cursedDoll = new NetBool();

		// Token: 0x040014D2 RID: 5330
		public readonly NetBool hauntedSkull = new NetBool();

		// Token: 0x040014D3 RID: 5331
		public readonly NetBool magmaSprite = new NetBool();

		// Token: 0x040014D4 RID: 5332
		public readonly NetBool canLunge = new NetBool();

		// Token: 0x040014D5 RID: 5333
		private ICue batFlap;

		// Token: 0x040014D6 RID: 5334
		private float extraVelocity;

		// Token: 0x040014D7 RID: 5335
		private float maxSpeed = 5f;

		// Token: 0x040014D8 RID: 5336
		public int lungeFrequency = 3000;

		// Token: 0x040014D9 RID: 5337
		public int lungeChargeTime = 500;

		// Token: 0x040014DA RID: 5338
		public int lungeSpeed = 30;

		// Token: 0x040014DB RID: 5339
		public int lungeDecelerationTicks = 60;

		// Token: 0x040014DC RID: 5340
		public int nextLunge = -1;

		// Token: 0x040014DD RID: 5341
		public int lungeTimer;

		// Token: 0x040014DE RID: 5342
		public Vector2 lungeVelocity = Vector2.Zero;

		// Token: 0x040014DF RID: 5343
		private List<Vector2> previousPositions = new List<Vector2>();
	}
}
