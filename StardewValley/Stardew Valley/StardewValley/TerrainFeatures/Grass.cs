using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000143 RID: 323
	[XmlInclude(typeof(CosmeticPlant))]
	[NotImplicitNetField]
	public class Grass : TerrainFeature
	{
		// Token: 0x0600199C RID: 6556 RVA: 0x0012DED0 File Offset: 0x0012C0D0
		public Grass() : base(true)
		{
			this.texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>(this.textureName()));
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x0012DF77 File Offset: 0x0012C177
		public Grass(int which, int numberOfWeeds) : this()
		{
			this.grassType.Value = (byte)which;
			this.loadSprite();
			this.numberOfWeeds.Value = numberOfWeeds;
		}

		// Token: 0x0600199E RID: 6558 RVA: 0x0012DF9E File Offset: 0x0012C19E
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.grassType, "grassType").AddField(this.numberOfWeeds, "numberOfWeeds").AddField(this.grassSourceOffset, "grassSourceOffset");
		}

		// Token: 0x0600199F RID: 6559 RVA: 0x0012DFDD File Offset: 0x0012C1DD
		public static void PlayGrassSound()
		{
			ICue cue = Grass.grassSound;
			if (cue == null || !cue.IsPlaying)
			{
				Game1.playSound("grassyStep", out Grass.grassSound);
			}
		}

		// Token: 0x060019A0 RID: 6560 RVA: 0x0012E005 File Offset: 0x0012C205
		public virtual string textureName()
		{
			return "TerrainFeatures\\grass";
		}

		// Token: 0x060019A1 RID: 6561 RVA: 0x0012E00C File Offset: 0x0012C20C
		public override bool isPassable(Character c = null)
		{
			return true;
		}

		// Token: 0x060019A2 RID: 6562 RVA: 0x0012E010 File Offset: 0x0012C210
		public override void loadSprite()
		{
			try
			{
				switch (this.grassType.Value)
				{
				case 1:
					switch (Game1.GetSeasonForLocation(this.Location))
					{
					case Season.Spring:
						this.grassSourceOffset.Value = 0;
						goto IL_194;
					case Season.Summer:
						this.grassSourceOffset.Value = 20;
						goto IL_194;
					case Season.Fall:
						this.grassSourceOffset.Value = 40;
						goto IL_194;
					case Season.Winter:
						this.grassSourceOffset.Value = ((this.Location != null && this.Location.IsOutdoors) ? 80 : 0);
						goto IL_194;
					default:
						goto IL_194;
					}
					break;
				case 2:
					this.grassSourceOffset.Value = 60;
					goto IL_194;
				case 3:
					this.grassSourceOffset.Value = 80;
					goto IL_194;
				case 4:
					this.grassSourceOffset.Value = 100;
					goto IL_194;
				case 7:
					switch (Game1.GetSeasonForLocation(this.Location))
					{
					case Season.Spring:
						this.grassSourceOffset.Value = 160;
						goto IL_194;
					case Season.Summer:
						this.grassSourceOffset.Value = 180;
						goto IL_194;
					case Season.Fall:
						this.grassSourceOffset.Value = 200;
						goto IL_194;
					case Season.Winter:
						this.grassSourceOffset.Value = ((this.Location != null && this.Location.IsOutdoors) ? 220 : 160);
						goto IL_194;
					default:
						goto IL_194;
					}
					break;
				}
				this.grassSourceOffset.Value = (int)((this.grassType.Value + 1) * 20);
				IL_194:;
			}
			catch
			{
			}
		}

		// Token: 0x060019A3 RID: 6563 RVA: 0x0012E1D4 File Offset: 0x0012C3D4
		public override void OnAddedToLocation(GameLocation location, Vector2 tile)
		{
			base.OnAddedToLocation(location, tile);
			this.loadSprite();
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x0012E1E4 File Offset: 0x0012C3E4
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		// Token: 0x060019A5 RID: 6565 RVA: 0x0012E21C File Offset: 0x0012C41C
		public override Rectangle getRenderBounds()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X * 64f) - 32, (int)(tileLocation.Y * 64f) - 32, 128, 112);
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x0012E25C File Offset: 0x0012C45C
		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
		{
			GameLocation location = this.Location;
			if (location != Game1.currentLocation)
			{
				return;
			}
			if (speedOfCollision > 0 && this.maxShake == 0f && positionOfCollider.Intersects(this.getBoundingBox()))
			{
				if (!(who is FarmAnimal) && Utility.isOnScreen(new Point((int)tileLocation.X, (int)tileLocation.Y), 2, location))
				{
					Grass.PlayGrassSound();
				}
				this.shake(0.3926991f / Math.Min(1f, 5f / (float)speedOfCollision), 0.03926991f / Math.Min(1f, 5f / (float)speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
			}
			if (who is Farmer)
			{
				MeleeWeapon weapon = Game1.player.CurrentTool as MeleeWeapon;
				if (weapon != null && weapon.isOnSpecial && weapon.type.Value == 0 && Math.Abs(this.shakeRotation) < 0.001f && this.performToolAction(Game1.player.CurrentTool, -1, tileLocation))
				{
					Game1.currentLocation.terrainFeatures.Remove(tileLocation);
				}
			}
			Farmer player = who as Farmer;
			if (player != null)
			{
				if (player.stats.Get("Book_Grass") > 0U)
				{
					player.temporarySpeedBuff = -0.33f;
				}
				else
				{
					player.temporarySpeedBuff = -1f;
				}
				if (this.grassType.Value == 6)
				{
					player.temporarySpeedBuff = -3f;
				}
			}
		}

		// Token: 0x060019A7 RID: 6567 RVA: 0x0012E3DC File Offset: 0x0012C5DC
		public bool reduceBy(int number, bool showDebris)
		{
			this.grassBladeHealth -= number;
			if (this.grassBladeHealth > 0)
			{
				return true;
			}
			int grassToDeplete;
			if (this.grassType.Value == 7)
			{
				grassToDeplete = 1 + this.grassBladeHealth / -2;
				this.grassBladeHealth = 2 - this.grassBladeHealth % 2;
			}
			else
			{
				this.grassBladeHealth = 1;
				grassToDeplete = number;
			}
			this.numberOfWeeds.Value -= grassToDeplete;
			if (showDebris)
			{
				Vector2 tileLocation = this.Tile;
				Game1.createRadialDebris(Game1.currentLocation, this.textureName(), new Rectangle(2, 8 + this.grassSourceOffset.Value, 8, 8), 1, (int)((tileLocation.X + 1f) * 64f), ((int)tileLocation.Y + 1) * 64, Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
				Game1.createRadialDebris(Game1.currentLocation, this.textureName(), new Rectangle(2, 8 + this.grassSourceOffset.Value, 8, 8), 1, (int)((tileLocation.X + 1.1f) * 64f), (int)((tileLocation.Y + 1.1f) * 64f), Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
				Game1.createRadialDebris(Game1.currentLocation, this.textureName(), new Rectangle(2, 8 + this.grassSourceOffset.Value, 8, 8), 1, (int)((tileLocation.X + 0.9f) * 64f), (int)((tileLocation.Y + 1.1f) * 64f), Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
				this.createDestroySprites(Game1.currentLocation, tileLocation);
			}
			return this.numberOfWeeds.Value <= 0;
		}

		// Token: 0x060019A8 RID: 6568 RVA: 0x0012E5B0 File Offset: 0x0012C7B0
		protected void shake(float shake, float rate, bool left)
		{
			this.maxShake = shake;
			this.shakeRate = rate;
			this.shakeRotation = 0f;
			this.shakeLeft = left;
			base.NeedsUpdate = true;
		}

		// Token: 0x060019A9 RID: 6569 RVA: 0x0012E5D9 File Offset: 0x0012C7D9
		public override void performPlayerEntryAction()
		{
			base.performPlayerEntryAction();
			if (this.shakeRandom[0] == 0.0)
			{
				this.setUpRandom();
			}
		}

		// Token: 0x060019AA RID: 6570 RVA: 0x0012E5FC File Offset: 0x0012C7FC
		public override bool tickUpdate(GameTime time)
		{
			if (this.shakeRandom[0] == 0.0)
			{
				this.setUpRandom();
			}
			if (this.maxShake > 0f)
			{
				if (this.shakeLeft)
				{
					this.shakeRotation -= this.shakeRate;
					if (Math.Abs(this.shakeRotation) >= this.maxShake)
					{
						this.shakeLeft = false;
					}
				}
				else
				{
					this.shakeRotation += this.shakeRate;
					if (this.shakeRotation >= this.maxShake)
					{
						this.shakeLeft = true;
						this.shakeRotation -= this.shakeRate;
					}
				}
				this.maxShake = Math.Max(0f, this.maxShake - 0.008975979f);
			}
			else
			{
				this.shakeRotation /= 2f;
				if (this.shakeRotation <= 0.01f)
				{
					base.NeedsUpdate = false;
					this.shakeRotation = 0f;
				}
			}
			return false;
		}

		// Token: 0x060019AB RID: 6571 RVA: 0x0012E6F4 File Offset: 0x0012C8F4
		public override void dayUpdate()
		{
			GameLocation environment = this.Location;
			if ((this.grassType.Value == 1 || this.grassType.Value == 7) && (environment.GetSeason() != Season.Winter || environment.HasMapPropertyWithValue("AllowGrassGrowInWinter")) && this.numberOfWeeds.Value < 4)
			{
				this.numberOfWeeds.Value = Utility.Clamp(this.numberOfWeeds.Value + Game1.random.Next(1, 4), 0, 4);
			}
			this.setUpRandom();
			if (this.grassType.Value == 7)
			{
				this.grassBladeHealth = 2;
				return;
			}
			this.grassBladeHealth = 1;
		}

		// Token: 0x060019AC RID: 6572 RVA: 0x0012E798 File Offset: 0x0012C998
		public void setUpRandom()
		{
			Vector2 tileLocation = this.Tile;
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed / 28.0, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
			GameLocation location = this.Location;
			bool front = ((location != null) ? new bool?(location.hasTileAt((int)tileLocation.X, (int)tileLocation.Y, "Front", null)) : null) ?? false;
			for (int i = 0; i < 4; i++)
			{
				this.whichWeed[i] = r.Next(3);
				this.offset1[i] = r.Next(-2, 3);
				this.offset2[i] = r.Next(-2, 3) + (front ? -7 : 0);
				this.offset3[i] = r.Next(-2, 3);
				this.offset4[i] = r.Next(-2, 3) + (front ? -7 : 0);
				this.flip[i] = r.NextBool();
				this.shakeRandom[i] = r.NextDouble();
			}
		}

		// Token: 0x060019AD RID: 6573 RVA: 0x0012E8E4 File Offset: 0x0012CAE4
		public override bool seasonUpdate(bool onLoad)
		{
			if (this.grassType.Value == 1 || this.grassType.Value == 7)
			{
				if (this.Location.IsOutdoors && this.Location.IsWinterHere() && this.Location.HasMapPropertyWithValue("AllowGrassSurviveInWinter") && this.Location.getMapProperty("AllowGrassSurviveInWinter").StartsWithIgnoreCase("f") && !onLoad)
				{
					return true;
				}
				this.loadSprite();
			}
			return false;
		}

		// Token: 0x060019AE RID: 6574 RVA: 0x0012E964 File Offset: 0x0012CB64
		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
		{
			GameLocation location = this.Location ?? Game1.currentLocation;
			MeleeWeapon weapon = t as MeleeWeapon;
			if ((weapon != null && weapon.type.Value != 2) || explosion > 0)
			{
				if (weapon != null && weapon.type.Value != 1)
				{
					DelayedAction.playSoundAfterDelay("daggerswipe", 50, location, new Vector2?(tileLocation), -1, false);
				}
				else
				{
					location.playSound("swordswipe", new Vector2?(tileLocation), null, SoundContext.Default);
				}
				this.shake(0.2945243f, 0.07853982f, Game1.random.NextBool());
				int numberOfWeedsToDestroy = (explosion > 0) ? Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)) : 1;
				if (weapon != null && t.ItemId == "53")
				{
					numberOfWeedsToDestroy = 2;
				}
				else if (weapon != null && t.ItemId == "66")
				{
					numberOfWeedsToDestroy = 4;
				}
				if (this.grassType.Value == 6 && Game1.random.NextBool())
				{
					numberOfWeedsToDestroy = 0;
				}
				this.numberOfWeeds.Value = this.numberOfWeeds.Value - numberOfWeedsToDestroy;
				this.createDestroySprites(location, tileLocation);
				return this.TryDropItemsOnCut(t, true);
			}
			return false;
		}

		// Token: 0x060019AF RID: 6575 RVA: 0x0012EA90 File Offset: 0x0012CC90
		private void createDestroySprites(GameLocation location, Vector2 tileLocation)
		{
			Color c;
			switch (this.grassType.Value)
			{
			case 1:
				switch (location.GetSeason())
				{
				case Season.Spring:
					c = new Color(60, 180, 58);
					goto IL_175;
				case Season.Summer:
					c = new Color(110, 190, 24);
					goto IL_175;
				case Season.Fall:
					c = new Color(219, 102, 58);
					goto IL_175;
				case Season.Winter:
					c = new Color(63, 167, 156);
					goto IL_175;
				default:
					c = Color.Green;
					goto IL_175;
				}
				break;
			case 2:
				c = new Color(148, 146, 71);
				goto IL_175;
			case 3:
				c = new Color(216, 240, 255);
				goto IL_175;
			case 4:
				c = new Color(165, 93, 58);
				goto IL_175;
			case 6:
				c = Color.White * 0.6f;
				goto IL_175;
			case 7:
				switch (location.GetSeason())
				{
				case Season.Spring:
				case Season.Summer:
					c = new Color(0, 178, 174);
					goto IL_175;
				case Season.Fall:
					c = new Color(129, 80, 148);
					goto IL_175;
				case Season.Winter:
					c = new Color(40, 125, 178);
					goto IL_175;
				default:
					c = Color.Green;
					goto IL_175;
				}
				break;
			}
			c = Color.Green;
			IL_175:
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(28, tileLocation * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), c, 8, Game1.random.NextBool(), (float)Game1.random.Next(60, 100), 0, -1, -1f, -1, 0)
			});
		}

		// Token: 0x060019B0 RID: 6576 RVA: 0x0012EC84 File Offset: 0x0012CE84
		public bool TryDropItemsOnCut(Tool tool, bool addAnimation = true)
		{
			Vector2 tileLocation = this.Tile;
			GameLocation location = this.Location;
			if (this.numberOfWeeds.Value <= 0)
			{
				if (this.grassType.Value != 1 && this.grassType.Value != 7)
				{
					Random grassRandom = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 1000.0, (double)tileLocation.Y * 11.0, (double)Game1.CurrentMineLevel, (double)Game1.player.timesReachedMineBottom);
					if (grassRandom.NextDouble() < 0.005)
					{
						Game1.createObjectDebris("(O)114", (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
					}
					else if (grassRandom.NextDouble() < 0.01)
					{
						Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
					}
					else if (grassRandom.NextDouble() < 0.02)
					{
						Game1.createObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, (long)grassRandom.Next(2, 4), location);
					}
				}
				else if (tool != null && tool.isScythe())
				{
					Farmer player = tool.getLastFarmerToUse() ?? Game1.player;
					Random random = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 1000.0, (double)tileLocation.Y * 11.0, 0.0, 0.0);
					double chance = (tool.ItemId == "66") ? 1.0 : ((tool.ItemId == "53") ? 0.75 : 0.5);
					if (player.currentLocation.IsWinterHere())
					{
						chance *= 0.33;
					}
					if (random.NextDouble() < chance)
					{
						int num = (this.grassType.Value == 7) ? 2 : 1;
						if (GameLocation.StoreHayInAnySilo(num, this.Location) == 0)
						{
							if (addAnimation)
							{
								TemporaryAnimatedSprite tmpSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, player.Position - new Vector2(0f, 128f), false, false, player.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f, false);
								tmpSprite.motion.Y = -3f + (float)Game1.random.Next(-10, 11) / 100f;
								tmpSprite.acceleration.Y = 0.07f + (float)Game1.random.Next(-10, 11) / 1000f;
								tmpSprite.motion.X = (float)Game1.random.Next(-20, 21) / 10f;
								tmpSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
								tmpSprite.delayBeforeAnimationStart = Game1.random.Next(150);
								Game1.multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite[]
								{
									tmpSprite
								});
							}
							Game1.addHUDMessage(HUDMessage.ForItemGained(ItemRegistry.Create("(O)178", 1, 0, false), num, null));
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060019B1 RID: 6577 RVA: 0x0012F01C File Offset: 0x0012D21C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed / 28.0, (double)positionOnScreen.X * 7.0, (double)positionOnScreen.Y * 11.0, 0.0);
			for (int i = 0; i < this.numberOfWeeds.Value; i++)
			{
				int whichWeed = r.Next(3);
				Vector2 pos;
				if (i == 4)
				{
					pos = tileLocation * 64f + new Vector2((float)(16 + r.Next(-2, 2) * 4 - 4) + 30f, (float)(16 + r.Next(-2, 2) * 4 + 40));
				}
				else
				{
					pos = tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + r.Next(-2, 2) * 4 - 4) + 30f, (float)(i / 2 * 64 / 2 + r.Next(-2, 2) * 4 + 40));
				}
				spriteBatch.Draw(this.texture.Value, pos, new Rectangle?(new Rectangle(whichWeed * 15, this.grassSourceOffset.Value, 15, 20)), Color.White, this.shakeRotation / (float)(r.NextDouble() + 1.0), Vector2.Zero, scale, SpriteEffects.None, layerDepth + (32f * scale + 300f) / 20000f);
			}
		}

		// Token: 0x060019B2 RID: 6578 RVA: 0x0012F198 File Offset: 0x0012D398
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			for (int i = 0; i < this.numberOfWeeds.Value; i++)
			{
				Vector2 pos;
				if (i == 4)
				{
					pos = tileLocation * 64f + new Vector2((float)(16 + this.offset1[i] * 4 - 4) + 30f, (float)(16 + this.offset2[i] * 4 + 40));
				}
				else
				{
					pos = tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + this.offset3[i] * 4 - 4) + 30f, (float)(i / 2 * 64 / 2 + this.offset4[i] * 4 + 40));
				}
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, pos), new Rectangle?(new Rectangle(this.whichWeed[i] * 15, this.grassSourceOffset.Value, 15, 20)), Color.White, this.shakeRotation / (float)(this.shakeRandom[i] + 1.0), new Vector2(7.5f, 17.5f), 4f, this.flip[i] ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (pos.Y + 16f - 20f) / 10000f + pos.X / 10000000f);
			}
		}

		// Token: 0x04000F82 RID: 3970
		public const float defaultShakeRate = 0.03926991f;

		// Token: 0x04000F83 RID: 3971
		public const float maximumShake = 0.3926991f;

		// Token: 0x04000F84 RID: 3972
		public const float shakeDecayRate = 0.008975979f;

		// Token: 0x04000F85 RID: 3973
		public const byte springGrass = 1;

		// Token: 0x04000F86 RID: 3974
		public const byte caveGrass = 2;

		// Token: 0x04000F87 RID: 3975
		public const byte frostGrass = 3;

		// Token: 0x04000F88 RID: 3976
		public const byte lavaGrass = 4;

		// Token: 0x04000F89 RID: 3977
		public const byte caveGrass2 = 5;

		// Token: 0x04000F8A RID: 3978
		public const byte cobweb = 6;

		// Token: 0x04000F8B RID: 3979
		public const byte blueGrass = 7;

		// Token: 0x04000F8C RID: 3980
		public static ICue grassSound;

		// Token: 0x04000F8D RID: 3981
		[XmlElement("grassType")]
		public readonly NetByte grassType = new NetByte();

		// Token: 0x04000F8E RID: 3982
		private bool shakeLeft;

		// Token: 0x04000F8F RID: 3983
		protected float shakeRotation;

		// Token: 0x04000F90 RID: 3984
		protected float maxShake;

		// Token: 0x04000F91 RID: 3985
		protected float shakeRate;

		// Token: 0x04000F92 RID: 3986
		[XmlElement("numberOfWeeds")]
		public readonly NetInt numberOfWeeds = new NetInt();

		// Token: 0x04000F93 RID: 3987
		[XmlElement("grassSourceOffset")]
		public readonly NetInt grassSourceOffset = new NetInt();

		// Token: 0x04000F94 RID: 3988
		private int grassBladeHealth = 1;

		// Token: 0x04000F95 RID: 3989
		[XmlIgnore]
		public Lazy<Texture2D> texture;

		// Token: 0x04000F96 RID: 3990
		private int[] whichWeed = new int[4];

		// Token: 0x04000F97 RID: 3991
		private int[] offset1 = new int[4];

		// Token: 0x04000F98 RID: 3992
		private int[] offset2 = new int[4];

		// Token: 0x04000F99 RID: 3993
		private int[] offset3 = new int[4];

		// Token: 0x04000F9A RID: 3994
		private int[] offset4 = new int[4];

		// Token: 0x04000F9B RID: 3995
		private bool[] flip = new bool[4];

		// Token: 0x04000F9C RID: 3996
		private double[] shakeRandom = new double[4];
	}
}
