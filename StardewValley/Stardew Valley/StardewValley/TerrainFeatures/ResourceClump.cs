using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000147 RID: 327
	[XmlInclude(typeof(GiantCrop))]
	public class ResourceClump : TerrainFeature
	{
		// Token: 0x170002CC RID: 716
		// (get) Token: 0x060019F2 RID: 6642 RVA: 0x00131896 File Offset: 0x0012FA96
		// (set) Token: 0x060019F3 RID: 6643 RVA: 0x001318A3 File Offset: 0x0012FAA3
		[XmlIgnore]
		public override Vector2 Tile
		{
			get
			{
				return this.netTile.Value;
			}
			set
			{
				this.netTile.Value = value;
			}
		}

		// Token: 0x060019F4 RID: 6644 RVA: 0x001318B4 File Offset: 0x0012FAB4
		public ResourceClump() : base(true)
		{
		}

		// Token: 0x060019F5 RID: 6645 RVA: 0x0013190C File Offset: 0x0012FB0C
		public ResourceClump(int parentSheetIndex, int width, int height, Vector2 tile, int? health = null, string textureName = null) : this()
		{
			this.width.Value = width;
			this.height.Value = height;
			this.parentSheetIndex.Value = parentSheetIndex;
			this.Tile = tile;
			this.textureName.Value = textureName;
			this.health.Value = (float)(health ?? this.GetDefaultHealth(parentSheetIndex));
			this.loadSprite();
		}

		// Token: 0x060019F6 RID: 6646 RVA: 0x00131988 File Offset: 0x0012FB88
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.width, "width").AddField(this.height, "height").AddField(this.parentSheetIndex, "parentSheetIndex").AddField(this.health, "health").AddField(this.netTile, "netTile").AddField(this.textureName, "textureName");
		}

		// Token: 0x060019F7 RID: 6647 RVA: 0x00131A04 File Offset: 0x0012FC04
		protected virtual int GetDefaultHealth(int parentSheetIndex)
		{
			if (parentSheetIndex <= 602)
			{
				if (parentSheetIndex == 148)
				{
					return 20;
				}
				if (parentSheetIndex != 600)
				{
					if (parentSheetIndex != 602)
					{
						return 1;
					}
					return 20;
				}
			}
			else
			{
				if (parentSheetIndex == 622)
				{
					return 20;
				}
				if (parentSheetIndex != 672)
				{
					switch (parentSheetIndex)
					{
					case 752:
					case 754:
					case 756:
					case 758:
						return 8;
					case 753:
					case 755:
					case 757:
						return 1;
					default:
						return 1;
					}
				}
			}
			return 10;
		}

		// Token: 0x060019F8 RID: 6648 RVA: 0x00131A76 File Offset: 0x0012FC76
		public override bool isPassable(Character c = null)
		{
			return false;
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x00131A79 File Offset: 0x0012FC79
		public bool IsGreenRainBush()
		{
			return this.parentSheetIndex.Value == 44 || this.parentSheetIndex.Value == 46;
		}

		// Token: 0x060019FA RID: 6650 RVA: 0x00131A9C File Offset: 0x0012FC9C
		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			if (t == null || this.lastToolHitTicker == t.swingTicker)
			{
				return false;
			}
			this.lastToolHitTicker = t.swingTicker;
			float power = Math.Max(1f, (float)(t.upgradeLevel.Value + 1) * 0.75f);
			GameLocation location = this.Location;
			int radialDebris = 12;
			int value = this.parentSheetIndex.Value;
			if (value <= 602)
			{
				if (value != 148)
				{
					if (value != 600)
					{
						if (value != 602)
						{
							goto IL_345;
						}
						if (t is Axe && t.upgradeLevel.Value < 2)
						{
							location.playSound("axe", new Vector2?(tileLocation), null, SoundContext.Default);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
							Game1.player.jitterStrength = 1f;
							return false;
						}
						if (!(t is Axe))
						{
							return false;
						}
						location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
						goto IL_392;
					}
					else
					{
						if (t is Axe && t.upgradeLevel.Value < 1)
						{
							location.playSound("axe", new Vector2?(tileLocation), null, SoundContext.Default);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13945"));
							Game1.player.jitterStrength = 1f;
							return false;
						}
						if (!(t is Axe))
						{
							return false;
						}
						location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
						goto IL_392;
					}
				}
			}
			else if (value != 622)
			{
				if (value != 672)
				{
					switch (value)
					{
					case 752:
					case 754:
					case 756:
					case 758:
						if (!(t is Pickaxe))
						{
							return false;
						}
						location.playSound("hammer", new Vector2?(tileLocation), null, SoundContext.Default);
						radialDebris = 14;
						this.shakeTimer = 500f;
						base.NeedsUpdate = true;
						goto IL_392;
					case 753:
					case 755:
					case 757:
						goto IL_345;
					default:
						goto IL_345;
					}
				}
				else
				{
					if (t is Pickaxe && t.upgradeLevel.Value < 2)
					{
						location.playSound("clubhit", new Vector2?(tileLocation), null, SoundContext.Default);
						location.playSound("clank", new Vector2?(tileLocation), null, SoundContext.Default);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13956"));
						Game1.player.jitterStrength = 1f;
						return false;
					}
					if (!(t is Pickaxe))
					{
						return false;
					}
					location.playSound("hammer", new Vector2?(tileLocation), null, SoundContext.Default);
					radialDebris = 14;
					goto IL_392;
				}
			}
			if (t is Pickaxe && t.upgradeLevel.Value < 3)
			{
				location.playSound("clubhit", new Vector2?(tileLocation), null, SoundContext.Default);
				location.playSound("clank", new Vector2?(tileLocation), null, SoundContext.Default);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13952"));
				Game1.player.jitterStrength = 1f;
				return false;
			}
			if (!(t is Pickaxe))
			{
				return false;
			}
			location.playSound("hammer", new Vector2?(tileLocation), null, SoundContext.Default);
			radialDebris = 14;
			goto IL_392;
			IL_345:
			if (this.IsGreenRainBush())
			{
				location.playSound((this.health.Value - power <= 0f) ? "cut" : "weed_cut", new Vector2?(tileLocation), null, SoundContext.Default);
				this.shakeTimer = 500f;
				radialDebris = 36;
			}
			IL_392:
			this.health.Value -= power;
			if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(power / 12f) && (this.parentSheetIndex.Value == 602 || this.parentSheetIndex.Value == 600))
			{
				Debris d = new Debris(709, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
				d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
				d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
				location.debris.Add(d);
			}
			Game1.createRadialDebris(Game1.currentLocation, radialDebris, (int)tileLocation.X + Game1.random.Next(this.width.Value / 2 + 1), (int)tileLocation.Y + Game1.random.Next(this.height.Value / 2 + 1), Game1.random.Next(4, 9), false, -1, false, null);
			if (this.health.Value <= 0f)
			{
				return this.destroy(t, location, tileLocation);
			}
			this.shakeTimer = 100f;
			base.NeedsUpdate = true;
			return false;
		}

		// Token: 0x060019FB RID: 6651 RVA: 0x00131FD8 File Offset: 0x001301D8
		public bool destroy(Tool t, GameLocation location, Vector2 tileLocation)
		{
			if (t != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
			{
				Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
				if (o != null)
				{
					Game1.createItemDebris(o, tileLocation * 64f + new Vector2((float)(this.width.Value / 2), (float)(this.height.Value / 2)) * 64f, -1, location, -1, false);
				}
			}
			int value = this.parentSheetIndex.Value;
			if (value <= 602)
			{
				if (value != 148)
				{
					if (value != 600 && value != 602)
					{
						goto IL_AC3;
					}
					if (t == null)
					{
						return false;
					}
					if (t.getLastFarmerToUse() == Game1.player)
					{
						Stats stats = Game1.stats;
						uint stumpsChopped = stats.StumpsChopped;
						stats.StumpsChopped = stumpsChopped + 1U;
					}
					t.getLastFarmerToUse().gainExperience(2, 25);
					int numChunks = (this.parentSheetIndex.Value == 602) ? 8 : 2;
					Random r;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
						r = Game1.recentMultiplayerRandom;
					}
					else
					{
						r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
					}
					if (t.getLastFarmerToUse().professions.Contains(12))
					{
						if (numChunks == 8)
						{
							numChunks = 10;
						}
						else if (r.NextBool())
						{
							numChunks++;
						}
					}
					Item hardwood = ItemRegistry.Create("(O)709", numChunks, 0, false);
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleItemDebris(hardwood, tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					}
					else
					{
						Game1.createMultipleItemDebris(hardwood, tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					}
					location.playSound("stumpCrack", new Vector2?(tileLocation), null, SoundContext.Default);
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(23, tileLocation * 64f, Color.White, 4, false, 140f, 0, 128, -1f, 128, 0)
					});
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(385, 1522, 127, 79), 2000f, 1, 1, tileLocation * 64f + new Vector2(0f, 49f), false, false, 1E-05f, 0.016f, Color.White, 1f, 0f, 0f, 0f, false)
					});
					Game1.createRadialDebris(Game1.currentLocation, 34, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), false, -1, false, null);
					if (r.NextDouble() < 0.1)
					{
						Game1.createMultipleObjectDebris("(O)292", (int)tileLocation.X, (int)tileLocation.Y, 1);
					}
					if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
					{
						Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y, (int)tileLocation.Y, 0, 1f, location);
					}
					return true;
				}
			}
			else if (value != 622)
			{
				if (value != 672)
				{
					switch (value)
					{
					case 752:
					case 754:
					case 756:
					case 758:
						break;
					case 753:
					case 755:
					case 757:
						goto IL_AC3;
					default:
						goto IL_AC3;
					}
				}
			}
			else
			{
				if (t == null)
				{
					return false;
				}
				if (Game1.IsMultiplayer)
				{
					Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
					Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X, (double)(tileLocation.Y * 983728f), 0.0, 0.0);
					Game1.createMultipleObjectDebris("(O)386", (int)tileLocation.X, (int)tileLocation.Y, 10, t.getLastFarmerToUse().UniqueMultiplayerID);
					Game1.createMultipleObjectDebris("(O)390", (int)tileLocation.X, (int)tileLocation.Y, 8, t.getLastFarmerToUse().UniqueMultiplayerID);
					Game1.createMultipleObjectDebris("(O)749", (int)tileLocation.X, (int)tileLocation.Y, 2, t.getLastFarmerToUse().UniqueMultiplayerID);
					if (random.NextDouble() < 0.25)
					{
						Game1.createMultipleItemDebris(ItemRegistry.Create("(O)74", 1, 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					}
				}
				else
				{
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)386", 10, 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)390", 8, 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					Game1.createMultipleObjectDebris("(O)749", (int)tileLocation.X, (int)tileLocation.Y, 2);
					if (Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X, (double)(tileLocation.Y * 983728f), 0.0, 0.0).NextDouble() < 0.25)
					{
						Game1.createMultipleItemDebris(ItemRegistry.Create("(O)74", 1, 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
					}
				}
				location.playSound("boulderBreak", new Vector2?(tileLocation), null, SoundContext.Default);
				Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), false, -1, false, null);
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				});
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 0f)) * 64f, Color.White, 8, false, 110f, 0, -1, -1f, -1, 0)
				});
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 1f)) * 64f, Color.White, 8, true, 80f, 0, -1, -1f, -1, 0)
				});
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 1f)) * 64f, Color.White, 8, false, 90f, 0, -1, -1f, -1, 0)
				});
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
				{
					new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(32f, 32f), Color.White, 8, false, 70f, 0, -1, -1f, -1, 0)
				});
				return true;
			}
			if (t == null)
			{
				return false;
			}
			int numChunks2 = (this.parentSheetIndex.Value == 672) ? 15 : 10;
			if (Game1.IsMultiplayer)
			{
				Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0);
				Game1.createMultipleObjectDebris("(O)390", (int)tileLocation.X, (int)tileLocation.Y, numChunks2, t.getLastFarmerToUse().UniqueMultiplayerID);
			}
			else
			{
				Game1.createRadialDebris(Game1.currentLocation, 390, (int)tileLocation.X, (int)tileLocation.Y, numChunks2, false, -1, true, null);
			}
			location.playSound("boulderBreak", new Vector2?(tileLocation), null, SoundContext.Default);
			Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), false, -1, false, null);
			Color c = Color.White;
			switch (this.parentSheetIndex.Value)
			{
			case 752:
				c = new Color(188, 119, 98);
				break;
			case 754:
				c = new Color(168, 120, 95);
				break;
			case 756:
			case 758:
				c = new Color(67, 189, 238);
				break;
			}
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(48, tileLocation * 64f, c, 5, false, 180f, 0, 128, -1f, 128, 0)
				{
					alphaFade = 0.01f
				}
			});
			return true;
			IL_AC3:
			if (this.IsGreenRainBush())
			{
				Color col = Color.Green;
				for (int x = 0; x < 2; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						Vector2 tile = tileLocation + new Vector2((float)x, (float)y);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(50, tile * 64f, col, 8, false, 100f, 0, -1, -1f, -1, 0)
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(50, tile * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), col * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
							{
								scale = 0.75f,
								flipped = true
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(50, tile * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), col * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
							{
								scale = 0.75f,
								delayBeforeAnimationStart = 50
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(50, tile * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-48, 48)), col * 0.75f, 8, false, 100f, 0, -1, -1f, -1, 0)
							{
								scale = 0.75f,
								flipped = true,
								delayBeforeAnimationStart = 100
							}
						});
					}
				}
				if (t != null)
				{
					t.getLastFarmerToUse().gainExperience(2, 15);
				}
				Random ran = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)Moss", ran.Next(2, 4), 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)771", ran.Next(2, 4), 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 3f, (float)this.height.Value / 3f) * 64f, -1, Game1.currentLocation, -1, false);
				if (ran.NextDouble() < 0.05)
				{
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MossySeed", 1, 0, false), tileLocation * 64f + new Vector2((float)this.width.Value / 4f, (float)this.height.Value / 4f) * 64f, -1, Game1.currentLocation, -1, false);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060019FC RID: 6652 RVA: 0x00132E44 File Offset: 0x00131044
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, this.width.Value * 64, this.height.Value * 64);
		}

		// Token: 0x060019FD RID: 6653 RVA: 0x00132E90 File Offset: 0x00131090
		public bool occupiesTile(int x, int y)
		{
			Vector2 tile = this.Tile;
			return (float)x >= tile.X && (float)x - tile.X < (float)this.width.Value && (float)y >= tile.Y && (float)y - tile.Y < (float)this.height.Value;
		}

		// Token: 0x060019FE RID: 6654 RVA: 0x00132EE8 File Offset: 0x001310E8
		public override void draw(SpriteBatch spriteBatch)
		{
			if (this.texture == null)
			{
				this.loadSprite();
			}
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(this.texture, this.parentSheetIndex.Value, 16, 16);
			sourceRect.Width = this.width.Value * 16;
			sourceRect.Height = this.height.Value * 16;
			Vector2 tile = this.Tile;
			Vector2 position = tile * 64f;
			if (this.shakeTimer > 0f)
			{
				position.X += (float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 4f;
			}
			spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, position), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tile.Y + 1f) * 64f / 10000f + tile.X / 100000f);
		}

		// Token: 0x060019FF RID: 6655 RVA: 0x00132FE7 File Offset: 0x001311E7
		public override void loadSprite()
		{
			this.texture = ((this.textureName.Value != null) ? Game1.content.Load<Texture2D>(this.textureName.Value) : Game1.objectSpriteSheet);
		}

		// Token: 0x06001A00 RID: 6656 RVA: 0x00133018 File Offset: 0x00131218
		public override bool performUseAction(Vector2 tileLocation)
		{
			if (!Game1.didPlayerJustRightClick(true))
			{
				Game1.haltAfterCheck = false;
				return false;
			}
			int value = this.parentSheetIndex.Value;
			if (value == 602)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13962")));
				return true;
			}
			if (value == 622)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13964")));
				return true;
			}
			if (value != 672)
			{
				return false;
			}
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13963")));
			return true;
		}

		// Token: 0x06001A01 RID: 6657 RVA: 0x001330B0 File Offset: 0x001312B0
		public override bool tickUpdate(GameTime time)
		{
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				base.NeedsUpdate = false;
			}
			return false;
		}

		// Token: 0x04000FDD RID: 4061
		public const int greenRainBush1Index = 44;

		// Token: 0x04000FDE RID: 4062
		public const int greenRainBush2Index = 46;

		// Token: 0x04000FDF RID: 4063
		public const int stumpIndex = 600;

		// Token: 0x04000FE0 RID: 4064
		public const int hollowLogIndex = 602;

		// Token: 0x04000FE1 RID: 4065
		public const int meteoriteIndex = 622;

		// Token: 0x04000FE2 RID: 4066
		public const int boulderIndex = 672;

		// Token: 0x04000FE3 RID: 4067
		public const int mineRock1Index = 752;

		// Token: 0x04000FE4 RID: 4068
		public const int mineRock2Index = 754;

		// Token: 0x04000FE5 RID: 4069
		public const int mineRock3Index = 756;

		// Token: 0x04000FE6 RID: 4070
		public const int mineRock4Index = 758;

		// Token: 0x04000FE7 RID: 4071
		public const int quarryBoulderIndex = 148;

		// Token: 0x04000FE8 RID: 4072
		[XmlElement("width")]
		public readonly NetInt width = new NetInt();

		// Token: 0x04000FE9 RID: 4073
		[XmlElement("height")]
		public readonly NetInt height = new NetInt();

		// Token: 0x04000FEA RID: 4074
		[XmlElement("parentSheetIndex")]
		public readonly NetInt parentSheetIndex = new NetInt();

		// Token: 0x04000FEB RID: 4075
		[XmlElement("textureName")]
		public readonly NetString textureName = new NetString();

		// Token: 0x04000FEC RID: 4076
		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		// Token: 0x04000FED RID: 4077
		[XmlElement("tile")]
		public readonly NetVector2 netTile = new NetVector2();

		// Token: 0x04000FEE RID: 4078
		[XmlIgnore]
		public float shakeTimer;

		// Token: 0x04000FEF RID: 4079
		private Texture2D texture;

		// Token: 0x04000FF0 RID: 4080
		private int lastToolHitTicker;
	}
}
