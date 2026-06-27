using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002EA RID: 746
	public class Mountain : GameLocation
	{
		// Token: 0x060031BC RID: 12732 RVA: 0x0027ABDC File Offset: 0x00278DDC
		public Mountain()
		{
		}

		// Token: 0x060031BD RID: 12733 RVA: 0x0027ACF0 File Offset: 0x00278EF0
		public Mountain(string map, string name) : base(map, name)
		{
			for (int i = 0; i < 10; i++)
			{
				this.quarryDayUpdate();
			}
		}

		// Token: 0x060031BE RID: 12734 RVA: 0x0027AE16 File Offset: 0x00279016
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.oreBoulderPresent, "oreBoulderPresent").AddField(this.railroadAreaBlocked, "railroadAreaBlocked").AddField(this.landslide, "landslide");
		}

		// Token: 0x060031BF RID: 12735 RVA: 0x0027AE58 File Offset: 0x00279058
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "outdoors");
			if (tileIndexAt == 958 || tileIndexAt - 1080 <= 1)
			{
				base.ShowMineCartMenu("Default", "Quarry");
				return true;
			}
			if (tileIndexAt == 1136 && !who.mailReceived.Contains("guildMember") && !who.hasQuest("16"))
			{
				Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Mountain_AdventurersGuildNote").Replace('\n', '^'));
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060031C0 RID: 12736 RVA: 0x0027AEEC File Offset: 0x002790EC
		public void ApplyTreehouseIfNecessary()
		{
			WorldChangeEvent worldChangeEvent = Game1.farmEvent as WorldChangeEvent;
			if ((worldChangeEvent == null || worldChangeEvent.whichEvent.Value != 14) && !Game1.MasterPlayer.mailReceived.Contains("leoMoved") && !Game1.MasterPlayer.mailReceived.Contains("leoMoved%&NL&%"))
			{
				return;
			}
			if (this.treehouseBuilt)
			{
				return;
			}
			TileSheet tilesheet = this.map.RequireTileSheet("untitled tile sheet2");
			Layer buildingsLayer = this.map.RequireLayer("Buildings");
			Layer backLayer = this.map.RequireLayer("Back");
			buildingsLayer.Tiles[16, 6] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, 197);
			buildingsLayer.Tiles[16, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, 213);
			backLayer.Tiles[16, 8] = new StaticTile(backLayer, tilesheet, BlendMode.Alpha, 229);
			buildingsLayer.Tiles[16, 7].Properties["Action"] = "LockedDoorWarp 3 8 LeoTreeHouse 600 2300";
			this.treehouseBuilt = true;
			if (Game1.IsMasterGame)
			{
				base.updateDoors();
				this.treehouseDoorDirty = true;
			}
		}

		// Token: 0x060031C1 RID: 12737 RVA: 0x0027B018 File Offset: 0x00279218
		private void restoreBridge()
		{
			LocalizedContentManager temp = Game1.content.CreateTemporary();
			Map map = temp.Load<Map>("Maps\\Mountain-BridgeFixed");
			int xOffset = 92;
			int yOffset = 24;
			Layer curBackLayer = this.map.RequireLayer("Back");
			Layer curBuildingsLayer = this.map.RequireLayer("Buildings");
			Layer curFrontLayer = this.map.RequireLayer("Front");
			Layer fixedBackLayer = map.RequireLayer("Back");
			Layer fixedBuildingsLayer = map.RequireLayer("Buildings");
			Layer fixedFrontLayer = map.RequireLayer("Front");
			TileSheet tileSheet = this.map.RequireTileSheet(0, "outdoors");
			for (int x = 0; x < fixedBackLayer.LayerWidth; x++)
			{
				for (int y = 0; y < fixedBackLayer.LayerHeight; y++)
				{
					curBackLayer.Tiles[x + xOffset, y + yOffset] = ((fixedBackLayer.Tiles[x, y] == null) ? null : new StaticTile(curBackLayer, tileSheet, BlendMode.Alpha, fixedBackLayer.Tiles[x, y].TileIndex));
					curBuildingsLayer.Tiles[x + xOffset, y + yOffset] = ((fixedBuildingsLayer.Tiles[x, y] == null) ? null : new StaticTile(curBuildingsLayer, tileSheet, BlendMode.Alpha, fixedBuildingsLayer.Tiles[x, y].TileIndex));
					curFrontLayer.Tiles[x + xOffset, y + yOffset] = ((fixedFrontLayer.Tiles[x, y] == null) ? null : new StaticTile(curFrontLayer, tileSheet, BlendMode.Alpha, fixedFrontLayer.Tiles[x, y].TileIndex));
				}
			}
			this.bridgeRestored = true;
			temp.Unload();
		}

		// Token: 0x060031C2 RID: 12738 RVA: 0x0027B1CC File Offset: 0x002793CC
		protected override void resetSharedState()
		{
			base.resetSharedState();
			this.oreBoulderPresent.Value = (!Game1.MasterPlayer.mailReceived.Contains("ccFishTank") || Game1.farmEvent != null);
			Vector2 fireTile = new Vector2(29f, 9f);
			if (!this.objects.ContainsKey(fireTile))
			{
				this.objects.Add(fireTile, new Torch("146", true)
				{
					IsOn = false,
					Fragility = 2
				});
				this.objects[fireTile].checkForAction(null, false);
			}
			if (Game1.stats.DaysPlayed >= 5U)
			{
				this.landslide.Value = false;
			}
			if (Game1.stats.DaysPlayed >= 31U)
			{
				this.railroadAreaBlocked.Value = false;
			}
		}

		// Token: 0x060031C3 RID: 12739 RVA: 0x0027B298 File Offset: 0x00279498
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				this.treehouseBuilt = false;
				this.bridgeRestored = false;
			}
			if (!this.bridgeRestored && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom"))
			{
				this.restoreBridge();
			}
			WorldChangeEvent worldChangeEvent = Game1.farmEvent as WorldChangeEvent;
			if (worldChangeEvent == null || worldChangeEvent.whichEvent.Value != 14)
			{
				this.ApplyTreehouseIfNecessary();
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				base.ApplyMapOverride("Mountain_Shortcuts", null, null);
				this.waterTiles[81, 37] = false;
				this.waterTiles[82, 37] = false;
				this.waterTiles[83, 37] = false;
				this.waterTiles[84, 37] = false;
				this.waterTiles[85, 37] = false;
				this.waterTiles[85, 38] = false;
				this.waterTiles[85, 39] = false;
				this.waterTiles[85, 40] = false;
			}
		}

		// Token: 0x060031C4 RID: 12740 RVA: 0x0027B3B4 File Offset: 0x002795B4
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
			{
				this.minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(8072f, 656f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					totalNumberOfLoops = 999999,
					interval = 60f,
					flipped = true
				};
			}
			Season season = base.GetSeason();
			this.boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439 + ((season == Season.Winter) ? 39 : 0), 1385, 39, 48);
			this.raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, (season == Season.Spring) ? 2176 : 1453, 64, 80);
			base.addFrog();
			if (Game1.IsWinter)
			{
				Game1.currentLightSources.Add(new LightSource("Mountain_1", 4, new Vector2(800f, 1366f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Mountain_2", 4, new Vector2(544f, 1155f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Mountain_3", 4, new Vector2(924f, 1563f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Mountain_4", 4, new Vector2(673f, 1567f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
		}

		// Token: 0x060031C5 RID: 12741 RVA: 0x0027B550 File Offset: 0x00279750
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.quarryDayUpdate();
			if (Game1.stats.DaysPlayed >= 31U)
			{
				this.railroadAreaBlocked.Value = false;
			}
			if (Game1.stats.DaysPlayed >= 5U)
			{
				this.landslide.Value = false;
				if (!Game1.player.hasOrWillReceiveMail("landslideDone"))
				{
					Game1.addMail("landslideDone", false, true);
				}
			}
			if (Game1.IsFall && Game1.dayOfMonth == 17)
			{
				base.tryPlaceObject(new Vector2(11f, 26f), ItemRegistry.Create<Object>("(O)746", 1, 0, false));
			}
		}

		// Token: 0x060031C6 RID: 12742 RVA: 0x0027B5F0 File Offset: 0x002797F0
		private void quarryDayUpdate()
		{
			Microsoft.Xna.Framework.Rectangle quarryBounds = new Microsoft.Xna.Framework.Rectangle(106, 13, 22, 22);
			int numberOfAdditionsToTry = Math.Min(16, 5 + Game1.year * 2);
			for (int i = 0; i < numberOfAdditionsToTry; i++)
			{
				Vector2 position = Utility.getRandomPositionInThisRectangle(quarryBounds, Game1.random);
				if (this.isTileOpenForQuarryStone((int)position.X, (int)position.Y))
				{
					if (Game1.random.NextDouble() < 0.06)
					{
						this.terrainFeatures.Add(position, new Tree((1 + Game1.random.Next(2)).ToString(), 1, false));
					}
					else if (Game1.random.NextDouble() < 0.02)
					{
						if (Game1.random.NextDouble() < 0.1)
						{
							this.objects.Add(position, new Object(46.ToString(), 1, false, -1, 0)
							{
								MinutesUntilReady = 12
							});
						}
						else
						{
							this.objects.Add(position, new Object(((Game1.random.Next(7) + 1) * 2).ToString(), 1, false, -1, 0)
							{
								MinutesUntilReady = 5
							});
						}
					}
					else if (Game1.random.NextDouble() < 0.04)
					{
						this.objects.Add(position, ItemRegistry.Create<Object>(Game1.random.NextBool(0.15) ? "(O)SeedSpot" : "(O)590", 1, 0, false));
					}
					else if (Game1.random.NextDouble() < 0.15)
					{
						if (Game1.random.NextDouble() < 0.001)
						{
							this.objects.Add(position, new Object("765", 1, false, -1, 0)
							{
								MinutesUntilReady = 16
							});
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							this.objects.Add(position, new Object("764", 1, false, -1, 0)
							{
								MinutesUntilReady = 8
							});
						}
						else if (Game1.random.NextDouble() < 0.33)
						{
							this.objects.Add(position, new Object("290", 1, false, -1, 0)
							{
								MinutesUntilReady = 5
							});
						}
						else
						{
							this.objects.Add(position, new Object("751", 1, false, -1, 0)
							{
								MinutesUntilReady = 3
							});
						}
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						this.objects.Add(position, new Object(Game1.random.Choose("BasicCoalNode0", "BasicCoalNode1"), 1, false, -1, 0)
						{
							MinutesUntilReady = 5
						});
					}
					else
					{
						string id = Game1.random.Choose(new string[]
						{
							"32",
							"38",
							"40",
							"42",
							"668",
							"670"
						});
						this.objects.Add(position, new Object(id, 1, false, -1, 0)
						{
							MinutesUntilReady = 2
						});
					}
				}
			}
		}

		// Token: 0x060031C7 RID: 12743 RVA: 0x0027B90D File Offset: 0x00279B0D
		private bool isTileOpenForQuarryStone(int tileX, int tileY)
		{
			return this.doesTileHaveProperty(tileX, tileY, "Diggable", "Back", false) != null && this.CanItemBePlacedHere(new Vector2((float)tileX, (float)tileY), false, CollisionMask.All, CollisionMask.None, false, false);
		}

		// Token: 0x060031C8 RID: 12744 RVA: 0x0027B93E File Offset: 0x00279B3E
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			this.minecartSteam = null;
		}

		// Token: 0x060031C9 RID: 12745 RVA: 0x0027B950 File Offset: 0x00279B50
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.minecartSteam;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.update(time);
			}
			if (this.landslide.Value && (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds - 400.0) / 1600.0) % 2 != 0 && Utility.isOnScreen(new Point(this.landSlideRect.X / 64, this.landSlideRect.Y / 64), 128, null))
			{
				if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 < (double)(this.oldTime % 400))
				{
					base.localSound("hammer", null, null, SoundContext.Default);
				}
				this.oldTime = (int)time.TotalGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x060031CA RID: 12746 RVA: 0x0027BA44 File Offset: 0x00279C44
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return (this.landslide.Value && position.Intersects(this.landSlideRect)) || (this.railroadAreaBlocked.Value && position.Intersects(this.railroadBlockRect)) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		// Token: 0x060031CB RID: 12747 RVA: 0x0027BA9C File Offset: 0x00279C9C
		public override bool isTilePlaceable(Vector2 tileLocation, bool itemIsPassable = false)
		{
			Point non_tile_position = Utility.Vector2ToPoint((tileLocation + new Vector2(0.5f, 0.5f)) * 64f);
			return (!this.landslide.Value || !this.landSlideRect.Contains(non_tile_position)) && (!this.railroadAreaBlocked.Value || !this.railroadBlockRect.Contains(non_tile_position)) && base.isTilePlaceable(tileLocation, itemIsPassable);
		}

		// Token: 0x060031CC RID: 12748 RVA: 0x0027BB10 File Offset: 0x00279D10
		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.minecartSteam;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(spriteBatch, false, 0, 0, 1f);
			}
			if (this.oreBoulderPresent.Value)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.boulderPosition), new Microsoft.Xna.Framework.Rectangle?(this.boulderSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
			}
			if (this.railroadAreaBlocked.Value)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.railroadBlockRect), new Microsoft.Xna.Framework.Rectangle?(this.raildroadBlocksourceRect), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0193f);
			}
			if (this.landslide.Value)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.landSlideRect), new Microsoft.Xna.Framework.Rectangle?(this.landSlideSourceRect), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0192f);
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2((float)(this.landSlideRect.X + 192 - 20), (float)(this.landSlideRect.Y + 192 + 20)) + new Vector2(32f, 24f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0224f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)(this.landSlideRect.X + 192 - 20), (float)(this.landSlideRect.Y + 128))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(288 + (((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1600.0 % 2.0) == 0) ? 0 : ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 19)), 1349, 19, 28)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)(this.landSlideRect.X + 256 - 20), (float)(this.landSlideRect.Y + 128))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(335, 1410, 21, 21)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0128f);
			}
		}

		// Token: 0x0400214B RID: 8523
		public const int daysBeforeLandslide = 31;

		// Token: 0x0400214C RID: 8524
		private TemporaryAnimatedSprite minecartSteam;

		// Token: 0x0400214D RID: 8525
		private bool bridgeRestored;

		// Token: 0x0400214E RID: 8526
		[XmlIgnore]
		public bool treehouseBuilt;

		// Token: 0x0400214F RID: 8527
		[XmlIgnore]
		public bool treehouseDoorDirty;

		// Token: 0x04002150 RID: 8528
		private readonly NetBool oreBoulderPresent = new NetBool();

		// Token: 0x04002151 RID: 8529
		private readonly NetBool railroadAreaBlocked = new NetBool(Game1.stats.DaysPlayed < 31U);

		// Token: 0x04002152 RID: 8530
		private readonly NetBool landslide = new NetBool(Game1.stats.DaysPlayed < 5U);

		// Token: 0x04002153 RID: 8531
		private Microsoft.Xna.Framework.Rectangle landSlideRect = new Microsoft.Xna.Framework.Rectangle(3200, 256, 192, 320);

		// Token: 0x04002154 RID: 8532
		private Microsoft.Xna.Framework.Rectangle railroadBlockRect = new Microsoft.Xna.Framework.Rectangle(512, 0, 256, 320);

		// Token: 0x04002155 RID: 8533
		private int oldTime;

		// Token: 0x04002156 RID: 8534
		private Microsoft.Xna.Framework.Rectangle boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439, 1385, 39, 48);

		// Token: 0x04002157 RID: 8535
		private Microsoft.Xna.Framework.Rectangle raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, 2176, 64, 80);

		// Token: 0x04002158 RID: 8536
		private Microsoft.Xna.Framework.Rectangle landSlideSourceRect = new Microsoft.Xna.Framework.Rectangle(646, 1218, 48, 80);

		// Token: 0x04002159 RID: 8537
		private Vector2 boulderPosition = new Vector2(47f, 3f) * 64f - new Vector2(4f, 3f) * 4f;
	}
}
