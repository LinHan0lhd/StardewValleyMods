using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Tools
{
	// Token: 0x02000130 RID: 304
	public class Pickaxe : Tool
	{
		// Token: 0x06001891 RID: 6289 RVA: 0x00121B28 File Offset: 0x0011FD28
		public Pickaxe() : base("Pickaxe", 0, 105, 131, false, 0)
		{
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x00121B4B File Offset: 0x0011FD4B
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.additionalPower, "additionalPower");
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x00121B6C File Offset: 0x0011FD6C
		protected override void MigrateLegacyItemId()
		{
			switch (base.UpgradeLevel)
			{
			case 0:
				base.ItemId = "Pickaxe";
				return;
			case 1:
				base.ItemId = "CopperPickaxe";
				return;
			case 2:
				base.ItemId = "SteelPickaxe";
				return;
			case 3:
				base.ItemId = "GoldPickaxe";
				return;
			case 4:
				base.ItemId = "IridiumPickaxe";
				return;
			default:
				base.ItemId = "Pickaxe";
				return;
			}
		}

		// Token: 0x06001894 RID: 6292 RVA: 0x00121BE3 File Offset: 0x0011FDE3
		protected override Item GetOneNew()
		{
			return new Pickaxe();
		}

		// Token: 0x06001895 RID: 6293 RVA: 0x00121BEC File Offset: 0x0011FDEC
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Pickaxe fromPickaxe = source as Pickaxe;
			if (fromPickaxe != null)
			{
				this.additionalPower.Value = fromPickaxe.additionalPower.Value;
			}
		}

		// Token: 0x06001896 RID: 6294 RVA: 0x00121C20 File Offset: 0x0011FE20
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			this.Update(who.FacingDirection, 0, who);
			who.EndUsingTool();
			return true;
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x00121C3C File Offset: 0x0011FE3C
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			power = who.toolPower.Value;
			if (!this.isEfficient.Value)
			{
				who.Stamina -= (float)(2 * (power + 1)) - (float)who.MiningLevel * 0.1f;
			}
			Utility.clampToTile(new Vector2((float)x, (float)y));
			int tileX = x / 64;
			int tileY = y / 64;
			Vector2 tile = new Vector2((float)tileX, (float)tileY);
			if (location.performToolAction(this, tileX, tileY))
			{
				return;
			}
			Object o;
			location.Objects.TryGetValue(tile, out o);
			if (o == null)
			{
				if (who.FacingDirection == 0 || who.FacingDirection == 2)
				{
					tileX = (x - 8) / 64;
					location.Objects.TryGetValue(new Vector2((float)tileX, (float)tileY), out o);
					if (o == null)
					{
						tileX = (x + 8) / 64;
						location.Objects.TryGetValue(new Vector2((float)tileX, (float)tileY), out o);
					}
				}
				else
				{
					tileY = (y + 8) / 64;
					location.Objects.TryGetValue(new Vector2((float)tileX, (float)tileY), out o);
					if (o == null)
					{
						tileY = (y - 8) / 64;
						location.Objects.TryGetValue(new Vector2((float)tileX, (float)tileY), out o);
					}
				}
				x = tileX * 64;
				y = tileY * 64;
				TerrainFeature terrainFeature;
				if (location.terrainFeatures.TryGetValue(tile, out terrainFeature) && terrainFeature.performToolAction(this, 0, tile))
				{
					location.terrainFeatures.Remove(tile);
				}
			}
			tile = new Vector2((float)tileX, (float)tileY);
			if (o != null)
			{
				if (o.IsBreakableStone())
				{
					if (this.PlayUseSounds)
					{
						location.playSound("hammer", new Vector2?(tile), null, SoundContext.Default);
					}
					if (o.MinutesUntilReady > 0)
					{
						int damage = Math.Max(1, this.upgradeLevel.Value + 1) + this.additionalPower.Value;
						o.minutesUntilReady.Value -= damage;
						o.shakeTimer = 200;
						if (o.MinutesUntilReady > 0)
						{
							Game1.createRadialDebris(Game1.currentLocation, 14, tileX, tileY, Game1.random.Next(2, 5), false, -1, false, null);
							return;
						}
					}
					TemporaryAnimatedSprite temporaryAnimatedSprite;
					if (!(ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).TextureName == "Maps\\springobjects") || o.ParentSheetIndex >= 200 || Game1.objectData.ContainsKey((o.ParentSheetIndex + 1).ToString()) || !(o.QualifiedItemId != "(O)25"))
					{
						temporaryAnimatedSprite = new TemporaryAnimatedSprite(47, new Vector2((float)(tileX * 64), (float)(tileY * 64)), Color.Gray, 10, false, 80f, 0, -1, -1f, -1, 0);
					}
					else
					{
						(temporaryAnimatedSprite = new TemporaryAnimatedSprite(o.ParentSheetIndex + 1, 300f, 1, 2, new Vector2((float)(x - x % 64), (float)(y - y % 64)), true, o.flipped.Value)).alphaFade = 0.01f;
					}
					TemporaryAnimatedSprite sprite = temporaryAnimatedSprite;
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						sprite
					});
					Game1.createRadialDebris(location, 14, tileX, tileY, Game1.random.Next(2, 5), false, -1, false, null);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(46, new Vector2((float)(tileX * 64), (float)(tileY * 64)), Color.White, 10, false, 80f, 0, -1, -1f, -1, 0)
						{
							motion = new Vector2(0f, -0.6f),
							acceleration = new Vector2(0f, 0.002f),
							alphaFade = 0.015f
						}
					});
					location.OnStoneDestroyed(o.ItemId, tileX, tileY, base.getLastFarmerToUse());
					if (who != null && who.stats.Get("Book_Diamonds") > 0U && Game1.random.NextDouble() < 0.0066)
					{
						Game1.createObjectDebris("(O)72", tileX, tileY, who.UniqueMultiplayerID, location);
						if (who.professions.Contains(19) && Game1.random.NextBool())
						{
							Game1.createObjectDebris("(O)72", tileX, tileY, who.UniqueMultiplayerID, location);
						}
					}
					if (o.MinutesUntilReady <= 0)
					{
						o.performRemoveAction();
						location.Objects.Remove(new Vector2((float)tileX, (float)tileY));
						if (this.PlayUseSounds)
						{
							location.playSound("stoneCrack", new Vector2?(tile), null, SoundContext.Default);
						}
						Stats stats = Game1.stats;
						uint rocksCrushed = stats.RocksCrushed;
						stats.RocksCrushed = rocksCrushed + 1U;
					}
					return;
				}
				if (o.Name.Contains("Boulder"))
				{
					if (this.PlayUseSounds)
					{
						location.playSound("hammer", new Vector2?(tile), null, SoundContext.Default);
					}
					if (base.UpgradeLevel < 2)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14194")));
						return;
					}
					if (tileX == this.boulderTileX && tileY == this.boulderTileY)
					{
						this.hitsToBoulder += power + 1;
						o.shakeTimer = 190;
					}
					else
					{
						this.hitsToBoulder = 0;
						this.boulderTileX = tileX;
						this.boulderTileY = tileY;
					}
					if (this.hitsToBoulder >= 4)
					{
						location.removeObject(tile, false);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X - 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 0
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X + 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 200
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * (tile.Y - 1f) - 32f), Color.Gray, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 400
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * tile.Y - 32f), Color.Gray, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, -1, 0)
							{
								delayBeforeAnimationStart = 600
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128, 0)
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X + 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128, 0)
							{
								delayBeforeAnimationStart = 250
							}
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X - 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128, 0)
							{
								delayBeforeAnimationStart = 500
							}
						});
						if (this.PlayUseSounds)
						{
							location.playSound("boulderBreak", new Vector2?(tile), null, SoundContext.Default);
							return;
						}
					}
				}
				else if (o.performToolAction(this))
				{
					o.performRemoveAction();
					if (o.Type == "Crafting" && o.fragility.Value != 2)
					{
						Game1.currentLocation.debris.Add(new Debris(o.QualifiedItemId, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel)));
					}
					Game1.currentLocation.Objects.Remove(tile);
					return;
				}
			}
			else
			{
				if (this.PlayUseSounds)
				{
					location.playSound("woodyHit", new Vector2?(tile), null, SoundContext.Default);
				}
				if (location.doesTileHaveProperty(tileX, tileY, "Diggable", "Back", false) != null)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(12, new Vector2((float)(tileX * 64), (float)(tileY * 64)), Color.White, 8, false, 80f, 0, -1, -1f, -1, 0)
						{
							alphaFade = 0.015f
						}
					});
				}
			}
		}

		// Token: 0x04000EE6 RID: 3814
		public const int hitMargin = 8;

		// Token: 0x04000EE7 RID: 3815
		public const int BoulderStrength = 4;

		// Token: 0x04000EE8 RID: 3816
		private int boulderTileX;

		// Token: 0x04000EE9 RID: 3817
		private int boulderTileY;

		// Token: 0x04000EEA RID: 3818
		private int hitsToBoulder;

		// Token: 0x04000EEB RID: 3819
		public NetInt additionalPower = new NetInt(0);
	}
}
