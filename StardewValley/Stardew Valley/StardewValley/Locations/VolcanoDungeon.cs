using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002F3 RID: 755
	public class VolcanoDungeon : IslandLocation
	{
		// Token: 0x0600327A RID: 12922 RVA: 0x0028E33C File Offset: 0x0028C53C
		public VolcanoDungeon()
		{
			this.mapContent = Game1.game1.xTileContent.CreateTemporary();
			this.mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
		}

		// Token: 0x0600327B RID: 12923 RVA: 0x0028E413 File Offset: 0x0028C613
		public VolcanoDungeon(int level) : this()
		{
			this.level.Value = level;
			this.name.Value = VolcanoDungeon.GetLevelName(level);
		}

		// Token: 0x0600327C RID: 12924 RVA: 0x0028E438 File Offset: 0x0028C638
		public override bool BlocksDamageLOS(int x, int y)
		{
			return !this.cooledLavaTiles.ContainsKey(new Vector2((float)x, (float)y)) && base.BlocksDamageLOS(x, y);
		}

		// Token: 0x0600327D RID: 12925 RVA: 0x0028E45C File Offset: 0x0028C65C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.level, "level").AddField(this.coolLavaEvent, "coolLavaEvent").AddField(this.cooledLavaTiles.NetFields, "cooledLavaTiles.NetFields").AddField(this.generationSeed, "generationSeed").AddField(this.layoutIndex, "layoutIndex").AddField(this.dwarfGates, "dwarfGates").AddField(this.shortcutOutUnlocked, "shortcutOutUnlocked").AddField(this.bridgeUnlocked, "bridgeUnlocked");
			this.coolLavaEvent.onEvent += this.OnCoolLavaEvent;
			this.bridgeUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.UpdateBridge();
				}
			};
			this.shortcutOutUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.UpdateShortcutOut();
				}
			};
		}

		// Token: 0x0600327E RID: 12926 RVA: 0x0028E541 File Offset: 0x0028C741
		protected override LocalizedContentManager getMapLoader()
		{
			return this.mapContent;
		}

		// Token: 0x0600327F RID: 12927 RVA: 0x0028E549 File Offset: 0x0028C749
		public override bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			return false;
		}

		// Token: 0x06003280 RID: 12928 RVA: 0x0028E54C File Offset: 0x0028C74C
		public virtual void OnCoolLavaEvent(Point point)
		{
			this.CoolLava(point.X, point.Y, true);
			this.UpdateLavaNeighbor(point.X, point.Y);
			this.UpdateLavaNeighbor(point.X - 1, point.Y);
			this.UpdateLavaNeighbor(point.X + 1, point.Y);
			this.UpdateLavaNeighbor(point.X, point.Y - 1);
			this.UpdateLavaNeighbor(point.X, point.Y + 1);
		}

		// Token: 0x06003281 RID: 12929 RVA: 0x0028E5D0 File Offset: 0x0028C7D0
		public virtual void CoolLava(int x, int y, bool playSound = true)
		{
			if (Game1.currentLocation == this)
			{
				for (int i = 0; i < 5; i++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2((float)x, (float)y - 0.5f) * 64f + new Vector2((float)Game1.random.Next(64), (float)Game1.random.Next(64)), false, 0.007f, Color.White)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -1f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 4f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
						delayBeforeAnimationStart = i * 35
					});
				}
				if (playSound && this.lavaSoundsPlayedThisTick < 3)
				{
					DelayedAction.playSoundAfterDelay("steam", this.lavaSoundsPlayedThisTick * 300, null, null, -1, false);
					this.lavaSoundsPlayedThisTick++;
				}
			}
			this.cooledLavaTiles.TryAdd(new Vector2((float)x, (float)y), true);
		}

		// Token: 0x06003282 RID: 12930 RVA: 0x0028E748 File Offset: 0x0028C948
		public virtual void UpdateLavaNeighbor(int x, int y)
		{
			if (this.IsCooledLava(x, y))
			{
				base.setTileProperty(x, y, "Buildings", "Passable", "T");
				int neighbors = 0;
				if (this.IsCooledLava(x, y - 1))
				{
					neighbors++;
				}
				if (this.IsCooledLava(x, y + 1))
				{
					neighbors += 2;
				}
				if (this.IsCooledLava(x - 1, y))
				{
					neighbors += 8;
				}
				if (this.IsCooledLava(x + 1, y))
				{
					neighbors += 4;
				}
				Point offset;
				if (this.GetBlobLookup().TryGetValue(neighbors, out offset))
				{
					this.localCooledLavaTiles[new Vector2((float)x, (float)y)] = offset;
				}
			}
		}

		// Token: 0x06003283 RID: 12931 RVA: 0x0028E7DD File Offset: 0x0028C9DD
		public virtual bool IsCooledLava(int x, int y)
		{
			return x >= 0 && x < this.mapWidth && y >= 0 && y < this.mapHeight && this.cooledLavaTiles.ContainsKey(new Vector2((float)x, (float)y));
		}

		// Token: 0x06003284 RID: 12932 RVA: 0x0028E811 File Offset: 0x0028CA11
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer == "LeaveVolcano_Yes")
			{
				this.UseVolcanoShortcut();
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x06003285 RID: 12933 RVA: 0x0028E838 File Offset: 0x0028CA38
		public void UseVolcanoShortcut()
		{
			DelayedAction.playSoundAfterDelay("fallDown", 200, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("clubSmash", 900, null, null, -1, false);
			Game1.player.CanMove = false;
			Game1.player.jump();
			Game1.warpFarmer("IslandNorth", 56, 17, 1);
		}

		// Token: 0x06003286 RID: 12934 RVA: 0x0028E8A0 File Offset: 0x0028CAA0
		public virtual void GenerateContents(bool use_level_level_as_layout = false)
		{
			this.generated = true;
			if (Game1.IsMasterGame)
			{
				this.generationSeed.Value = Utility.CreateRandomSeed((double)((ulong)Game1.stats.DaysPlayed * (ulong)((long)(this.level.Value + 1))), (double)(this.level.Value * 5152), Game1.uniqueIDForThisGame / 2UL, 0.0, 0.0);
				int value = this.level.Value;
				if (value != 0)
				{
					if (value != 5)
					{
						if (value != 9)
						{
							List<int> valid_layouts = new List<int>();
							for (int i = 1; i < this.GetMaxRoomLayouts(); i++)
							{
								valid_layouts.Add(i);
							}
							Random layout_random = Utility.CreateRandom((double)this.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
							float luckMultiplier = 1f + (float)Game1.player.team.AverageLuckLevel(null) * 0.035f + (float)Game1.player.team.AverageDailyLuck(null) / 2f;
							if (this.level.Value > 1 && layout_random.NextDouble() < 0.5 * (double)luckMultiplier)
							{
								bool foundSpecialLevel = false;
								for (int j = 0; j < VolcanoDungeon.activeLevels.Count; j++)
								{
									if (VolcanoDungeon.activeLevels[j].layoutIndex.Value >= 32)
									{
										foundSpecialLevel = true;
										break;
									}
								}
								if (!foundSpecialLevel)
								{
									for (int k = 32; k < 38; k++)
									{
										valid_layouts.Add(k);
									}
								}
							}
							if (this.level.Value > 0 && Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked") && layout_random.NextDouble() < 0.75)
							{
								for (int l = 38; l < 58; l++)
								{
									valid_layouts.Add(l);
								}
							}
							for (int m = 0; m < VolcanoDungeon.activeLevels.Count; m++)
							{
								if (VolcanoDungeon.activeLevels[m].level.Value == this.level.Value - 1)
								{
									valid_layouts.Remove(VolcanoDungeon.activeLevels[m].layoutIndex.Value);
									break;
								}
							}
							this.layoutIndex.Value = layout_random.ChooseFrom(valid_layouts);
						}
						else
						{
							this.layoutIndex.Value = 30;
						}
					}
					else
					{
						this.layoutIndex.Value = 31;
						this.waterColor.Value = Color.DeepSkyBlue * 0.6f;
						this.shortcutOutUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut");
						this.parrotUpgradePerches.Clear();
						this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y), new Microsoft.Xna.Framework.Rectangle(VolcanoDungeon.shortcutOutPosition.X - 1, VolcanoDungeon.shortcutOutPosition.Y - 1, 3, 3), 5, delegate()
						{
							Game1.addMailForTomorrow("Island_VolcanoShortcutOut", true, true);
							this.shortcutOutUnlocked.Value = true;
						}, () => this.shortcutOutUnlocked.Value, "VolcanoShortcutOut", "Island_Turtle"));
					}
				}
				else
				{
					this.layoutIndex.Value = 0;
					this.bridgeUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge");
					this.parrotUpgradePerches.Clear();
					this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(27, 39), new Microsoft.Xna.Framework.Rectangle(28, 34, 5, 4), 5, delegate()
					{
						Game1.addMailForTomorrow("Island_VolcanoBridge", true, true);
						this.bridgeUnlocked.Value = true;
					}, () => this.bridgeUnlocked.Value, "VolcanoBridge", "reachedCaldera, Island_Turtle"));
				}
			}
			this.GenerateLevel(use_level_level_as_layout);
			if (this.level.Value == 5)
			{
				base.ApplyMapOverride("Mines\\Volcano_Well", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(25, 29, 6, 4)));
				for (int x = 27; x < 31; x++)
				{
					for (int y = 29; y < 33; y++)
					{
						this.waterTiles[x, y] = true;
					}
				}
				base.ApplyMapOverride("Mines\\Volcano_DwarfShop", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(34, 29, 5, 4)));
				base.setMapTile(36, 30, 77, "Buildings", "dungeon", "asedf", true);
				base.setMapTile(36, 29, 61, "Front", "dungeon", null, true);
				base.setMapTile(35, 31, 78, "Back", "dungeon", null, true);
				base.setMapTile(36, 31, 79, "Back", "dungeon", null, true);
				base.setMapTile(37, 31, 62, "Back", "dungeon", null, true);
				if (Game1.IsMasterGame)
				{
					this.objects.Add(new Vector2(34f, 29f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(34f, 29f)));
					this.objects.Add(new Vector2(26f, 32f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(26f, 32f)));
					this.objects.Add(new Vector2(38f, 33f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(38f, 33f)));
				}
			}
		}

		// Token: 0x06003287 RID: 12935 RVA: 0x0028EDF5 File Offset: 0x0028CFF5
		public bool isMushroomLevel()
		{
			return this.layoutIndex.Value >= 32 && this.layoutIndex.Value <= 34;
		}

		// Token: 0x06003288 RID: 12936 RVA: 0x0028EE1A File Offset: 0x0028D01A
		public bool isMonsterLevel()
		{
			return this.layoutIndex.Value >= 35 && this.layoutIndex.Value <= 37;
		}

		// Token: 0x06003289 RID: 12937 RVA: 0x0028EE3F File Offset: 0x0028D03F
		public override void checkForMusic(GameTime time)
		{
			if (Game1.getMusicTrackName(MusicContext.Default) == "none" || Game1.isMusicContextActiveButNotPlaying(MusicContext.Default))
			{
				Game1.changeMusicTrack("Volcano_Ambient", false, MusicContext.Default);
			}
			base.checkForMusic(time);
		}

		// Token: 0x0600328A RID: 12938 RVA: 0x0028EE70 File Offset: 0x0028D070
		public virtual void UpdateShortcutOut()
		{
			if (this != Game1.currentLocation)
			{
				return;
			}
			if (this.shortcutOutUnlocked.Value)
			{
				base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y, 367, "Buildings", "dungeon", null, true);
				base.removeTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y - 1, "Front");
				return;
			}
			base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y, 399, "Buildings", "dungeon", null, true);
			base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y - 1, 383, "Front", "dungeon", null, true);
		}

		// Token: 0x0600328B RID: 12939 RVA: 0x0028EF3C File Offset: 0x0028D13C
		public virtual void UpdateBridge()
		{
			if (this != Game1.currentLocation)
			{
				return;
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
			{
				base.setMapTile(27, 39, 399, "Buildings", "dungeon", null, true);
				base.setMapTile(27, 38, 383, "Front", "dungeon", null, true);
			}
			if (this.bridgeUnlocked.Value)
			{
				for (int x = 28; x <= 32; x++)
				{
					for (int y = 34; y <= 37; y++)
					{
						int tile_index;
						if (x == 28)
						{
							if (y == 34)
							{
								tile_index = 189;
							}
							else if (y == 37)
							{
								tile_index = 221;
							}
							else
							{
								tile_index = 205;
							}
						}
						else if (x == 32)
						{
							if (y == 34)
							{
								tile_index = 191;
							}
							else if (y == 37)
							{
								tile_index = 223;
							}
							else
							{
								tile_index = 207;
							}
						}
						else if (y == 34)
						{
							tile_index = 190;
						}
						else if (y == 37)
						{
							tile_index = 222;
						}
						else
						{
							tile_index = 206;
						}
						base.setMapTile(x, y, tile_index, "Buildings", "dungeon", null, true).Properties["Passable"] = "T";
						base.removeTileProperty(x, y, "Back", "Water");
						NPC i = base.isCharacterAtTile(new Vector2((float)x, (float)y));
						if (i is Monster)
						{
							this.characters.Remove(i);
						}
						if (this.waterTiles != null && x != 28 && x != 32)
						{
							this.waterTiles[x, y] = false;
						}
						this.cooledLavaTiles.Remove(new Vector2((float)x, (float)y));
					}
				}
			}
		}

		// Token: 0x0600328C RID: 12940 RVA: 0x0028F0DC File Offset: 0x0028D2DC
		protected override void resetLocalState()
		{
			if (!this.generated)
			{
				this.GenerateContents(false);
				this.generated = true;
			}
			foreach (Vector2 position in this.cooledLavaTiles.Keys)
			{
				this.UpdateLavaNeighbor((int)position.X, (int)position.Y);
			}
			if (this.level.Value == 0)
			{
				this.UpdateBridge();
			}
			if (this.level.Value == 5)
			{
				this.UpdateShortcutOut();
			}
			base.resetLocalState();
			Game1.ambientLight = Color.White;
			int player_tile_y = (int)(Game1.player.Position.Y / 64f);
			if (this.level.Value == 0 && Game1.player.previousLocationName == "Caldera")
			{
				Game1.player.Position = new Vector2(44f, 50f) * 64f;
			}
			else if (player_tile_y == 0 && this.endPosition != null)
			{
				if (this.endPosition != null)
				{
					Game1.player.Position = new Vector2((float)this.endPosition.Value.X, (float)this.endPosition.Value.Y) * 64f;
				}
			}
			else if (player_tile_y == 1 && this.startPosition != null)
			{
				Game1.player.Position = new Vector2((float)this.startPosition.Value.X, (float)this.startPosition.Value.Y) * 64f;
			}
			TileSheet mainTileSheet = this.map.RequireTileSheet(0, "dungeon");
			this.mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(mainTileSheet.ImageSource);
			foreach (DwarfGate dwarfGate in this.dwarfGates)
			{
				dwarfGate.ResetLocalState();
			}
			if (this.level.Value == 5)
			{
				AmbientLocationSounds.addSound(new Vector2(29f, 31f), 0);
			}
			if (this.level.Value == 0)
			{
				if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_Volcano"))
				{
					this._sawFlameSprite = true;
				}
				if (!this._sawFlameSprite)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(30f, 38f) * 64f, false, 0f, Color.White)
					{
						id = 999,
						scale = 4f,
						totalNumberOfLoops = 99999,
						interval = 70f,
						lightId = "VolcanoDungeon_FlameSpirit",
						lightRadius = 1f,
						animationLength = 7,
						layerDepth = 1f,
						yPeriodic = true,
						yPeriodicRange = 12f,
						yPeriodicLoopTime = 1000f,
						xPeriodic = true,
						xPeriodicRange = 16f,
						xPeriodicLoopTime = 1800f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(30.2f, 39.4f) * 64f, false, 0f, Color.White)
					{
						id = 998,
						scale = 4f,
						totalNumberOfLoops = 99999,
						interval = 1000f,
						animationLength = 1,
						layerDepth = 0.001f,
						yPeriodic = true,
						yPeriodicRange = 1f,
						yPeriodicLoopTime = 1000f,
						xPeriodic = true,
						xPeriodicRange = 16f,
						xPeriodicLoopTime = 1800f
					});
				}
				base.ApplyMapOverride("Mines\\Volcano_Well", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(22, 43, 6, 4)));
				for (int x = 24; x < 28; x++)
				{
					for (int y = 43; y < 47; y++)
					{
						this.waterTiles[x, y] = true;
					}
				}
			}
		}

		// Token: 0x0600328D RID: 12941 RVA: 0x0028F544 File Offset: 0x0028D744
		public override string GetLocationSpecificMusic()
		{
			if (this.level.Value == 5)
			{
				return "Volcano_Ambient";
			}
			if (Game1.getMusicTrackName(MusicContext.Default) == "VolcanoMines")
			{
				return "VolcanoMines";
			}
			if (this.level.Value == 1 || ((Game1.random.NextDouble() < 0.25 || this.level.Value == 6) && (Game1.getMusicTrackName(MusicContext.Default) == "none" || Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) || Game1.getMusicTrackName(MusicContext.Default).EndsWith("_Ambient"))))
			{
				return "VolcanoMines";
			}
			return "Volcano_Ambient";
		}

		// Token: 0x0600328E RID: 12942 RVA: 0x0028F5E5 File Offset: 0x0028D7E5
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (this.level.Value != 5)
			{
				this.waterColor.Value = Color.White;
			}
		}

		// Token: 0x0600328F RID: 12943 RVA: 0x0028F60C File Offset: 0x0028D80C
		public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			return (this.level.Value == 5 && new Microsoft.Xna.Framework.Rectangle(27, 29, 4, 4).Contains(tileX, tileY)) || (this.level.Value == 0 && tileX > 23 && tileX < 28 && tileY > 42 && tileY < 47);
		}

		// Token: 0x06003290 RID: 12944 RVA: 0x0028F664 File Offset: 0x0028D864
		public virtual void GenerateLevel(bool use_level_level_as_layout = false)
		{
			this.generationRandom = Utility.CreateRandom((double)this.generationSeed.Value, 0.0, 0.0, 0.0, 0.0);
			this.generationRandom.Next();
			this.mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
			this.updateMap();
			this.loadedMapPath = this.mapPath.Value;
			Texture2D layout_texture = Game1.temporaryContent.Load<Texture2D>("VolcanoLayouts\\Layouts");
			this.mapWidth = 64;
			this.mapHeight = 64;
			this.waterTiles = new WaterTiles(this.mapWidth, this.mapHeight);
			for (int i = 0; i < this.map.Layers.Count; i++)
			{
				Layer template_layer = this.map.Layers[i];
				this.map.RemoveLayer(template_layer);
				this.map.InsertLayer(new Layer(template_layer.Id, this.map, new Size(this.mapWidth, this.mapHeight), template_layer.TileSize), i);
			}
			this.backLayer = this.map.RequireLayer("Back");
			this.buildingsLayer = this.map.RequireLayer("Buildings");
			this.frontLayer = this.map.RequireLayer("Front");
			this.alwaysFrontLayer = this.map.RequireLayer("AlwaysFront");
			TileSheet tileSheet = this.map.RequireTileSheet(0, "dungeon");
			tileSheet.TileIndexProperties[1].Add("Type", "Stone");
			tileSheet.TileIndexProperties[2].Add("Type", "Stone");
			tileSheet.TileIndexProperties[3].Add("Type", "Stone");
			tileSheet.TileIndexProperties[17].Add("Type", "Stone");
			tileSheet.TileIndexProperties[18].Add("Type", "Stone");
			tileSheet.TileIndexProperties[19].Add("Type", "Stone");
			tileSheet.TileIndexProperties[528].Add("Type", "Stone");
			tileSheet.TileIndexProperties[544].Add("Type", "Stone");
			tileSheet.TileIndexProperties[560].Add("Type", "Stone");
			tileSheet.TileIndexProperties[545].Add("Type", "Stone");
			tileSheet.TileIndexProperties[561].Add("Type", "Stone");
			tileSheet.TileIndexProperties[564].Add("Type", "Stone");
			tileSheet.TileIndexProperties[565].Add("Type", "Stone");
			tileSheet.TileIndexProperties[555].Add("Type", "Stone");
			tileSheet.TileIndexProperties[571].Add("Type", "Stone");
			this.pixelMap = new Color[this.mapWidth * this.mapHeight];
			this.heightMap = new int[this.mapWidth * this.mapHeight];
			int columns = layout_texture.Width / 64;
			int value = this.layoutIndex.Value;
			int layout_offset_x = value % columns * 64;
			int layout_offset_y = value / columns * 64;
			bool flip_x = this.generationRandom.Next(2) == 1;
			if (this.layoutIndex.Value == 0 || this.layoutIndex.Value == 31)
			{
				flip_x = false;
			}
			this.ApplyPixels("VolcanoLayouts\\Layouts", layout_offset_x, layout_offset_y, this.mapWidth, this.mapHeight, 0, 0, flip_x);
			for (int x2 = 0; x2 < this.mapWidth; x2++)
			{
				for (int y2 = 0; y2 < this.mapHeight; y2++)
				{
					this.PlaceGroundTile(x2, y2);
				}
			}
			this.ApplyToColor(new Color(0, 255, 0), delegate(int x, int y)
			{
				if (this.startPosition == null)
				{
					this.startPosition = new Point?(new Point(x, y));
				}
				if (this.level.Value == 0)
				{
					this.warps.Add(new Warp(x, y + 2, "IslandNorth", 40, 24, false, false));
					return;
				}
				this.warps.Add(new Warp(x, y + 2, VolcanoDungeon.GetLevelName(this.level.Value - 1), x - this.startPosition.Value.X, 0, false, false));
			});
			this.ApplyToColor(new Color(255, 0, 0), delegate(int x, int y)
			{
				if (this.endPosition == null)
				{
					this.endPosition = new Point?(new Point(x, y));
				}
				if (this.level.Value == 9)
				{
					this.warps.Add(new Warp(x, y - 2, "Caldera", 21, 39, false, false));
					return;
				}
				this.warps.Add(new Warp(x, y - 2, VolcanoDungeon.GetLevelName(this.level.Value + 1), x - this.endPosition.Value.X, 1, false, false));
			});
			VolcanoDungeon.setPieceAreas.Clear();
			Color set_piece_color = new Color(255, 255, 0);
			this.ApplyToColor(set_piece_color, delegate(int x, int y)
			{
				int size = 0;
				while (size < 32 && !(this.GetPixel(x + size, y, Color.Black) != set_piece_color) && !(this.GetPixel(x, y + size, Color.Black) != set_piece_color))
				{
					size++;
				}
				VolcanoDungeon.setPieceAreas.Add(new Microsoft.Xna.Framework.Rectangle(x, y, size, size));
				for (int off_x = 0; off_x < size; off_x++)
				{
					for (int off_y = 0; off_y < size; off_y++)
					{
						this.SetPixelMap(x + off_x, y + off_y, Color.White);
					}
				}
			});
			this.possibleSwitchPositions = new Dictionary<int, List<Point>>();
			this.possibleGatePositions = new Dictionary<int, List<Point>>();
			this.ApplyToColor(new Color(128, 128, 128), delegate(int x, int y)
			{
				this.AddPossibleSwitchLocation(0, x, y);
			});
			this.ApplySetPieces();
			this.GenerateWalls(Color.Black, 0, 4, 4, 4, true, delegate(int x, int y)
			{
				this.SetPixelMap(x, y, Color.Chartreuse);
			}, true);
			this.GenerateWalls(Color.Chartreuse, 0, 13, 1, 1, false, null, false);
			this.ApplyToColor(Color.Blue, delegate(int x, int y)
			{
				this.waterTiles[x, y] = true;
				this.SetTile(this.backLayer, x, y, 4);
				this.setTileProperty(x, y, "Back", "Water", "T");
				if (this.generationRandom.NextDouble() < 0.1)
				{
					NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights = this.sharedLights;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
					defaultInterpolatedStringHandler.AppendLiteral("VolcanoDungeon_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.level.Value);
					defaultInterpolatedStringHandler.AppendLiteral("_Lava_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(x);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(y);
					sharedLights.AddLight(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)x, (float)y) * 64f, 2f, new Color(0, 50, 50), LightSource.LightContext.None, 0L, this.NameOrUniqueName));
				}
			});
			this.GenerateBlobs(Color.Blue, 0, 16, true, true);
			if (this.startPosition != null)
			{
				this.CreateEntrance(new Point?(this.startPosition.Value));
			}
			if (this.endPosition != null)
			{
				this.CreateExit(this.endPosition, true);
			}
			if (this.level.Value != 0)
			{
				this.GenerateDirtTiles();
			}
			List<Point> endSwitchPositions;
			if ((this.level.Value == 9 || this.generationRandom.NextDouble() < (this.isMonsterLevel() ? 1.0 : 0.2)) && this.possibleSwitchPositions.TryGetValue(0, out endSwitchPositions) && endSwitchPositions.Count > 0)
			{
				this.AddPossibleGateLocation(0, this.endPosition.Value.X, this.endPosition.Value.Y);
			}
			foreach (int index in this.possibleGatePositions.Keys)
			{
				List<Point> dwarfSwitchPositions;
				if (this.possibleGatePositions[index].Count > 0 && this.possibleSwitchPositions.TryGetValue(index, out dwarfSwitchPositions) && dwarfSwitchPositions.Count > 0)
				{
					Point gate_point = this.generationRandom.ChooseFrom(this.possibleGatePositions[index]);
					this.CreateDwarfGate(index, gate_point);
				}
			}
			if (this.level.Value == 0)
			{
				this.CreateExit(new Point?(new Point(40, 48)), false);
				base.removeTile(40, 46, "Buildings");
				base.removeTile(40, 45, "Buildings");
				base.removeTile(40, 44, "Buildings");
				base.setMapTile(40, 45, 266, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(40, 44, 76, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(39, 44, 76, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(41, 44, 76, "AlwaysFront", "dungeon", null, true);
				base.removeTile(40, 43, "Front");
				base.setMapTile(40, 43, 70, "AlwaysFront", "dungeon", null, true);
				base.removeTile(39, 43, "Front");
				base.setMapTile(39, 43, 69, "AlwaysFront", "dungeon", null, true);
				base.removeTile(41, 43, "Front");
				base.setMapTile(41, 43, 69, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(39, 45, 265, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(41, 45, 267, "AlwaysFront", "dungeon", null, true);
				base.setMapTile(40, 45, 60, "Back", "dungeon", null, true);
				base.setMapTile(40, 46, 60, "Back", "dungeon", null, true);
				base.setMapTile(40, 47, 60, "Back", "dungeon", null, true);
				base.setMapTile(40, 48, 555, "Back", "dungeon", null, true);
				this.AddPossibleSwitchLocation(-1, 40, 51);
				this.CreateDwarfGate(-1, new Point(40, 48));
				base.setMapTile(34, 30, 90, "Buildings", "dungeon", null, true);
				base.setMapTile(34, 29, 148, "Buildings", "dungeon", null, true);
				base.setMapTile(34, 31, 180, "Buildings", "dungeon", null, true);
				base.setMapTile(34, 32, 196, "Buildings", "dungeon", null, true);
				this.CoolLava(34, 34, false);
				if (Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
				{
					foreach (DwarfGate gate in this.dwarfGates)
					{
						if (gate.gateIndex.Value == -1)
						{
							gate.opened.Value = true;
							gate.triggeredOpen = true;
							foreach (Point point in gate.switches.Keys)
							{
								gate.switches[point] = true;
							}
						}
					}
				}
				this.CreateExit(new Point?(new Point(44, 50)), true);
				this.warps.Add(new Warp(44, 48, "Caldera", 11, 36, false, false));
				this.CreateEntrance(new Point?(new Point(6, 48)));
				this.warps.Add(new Warp(6, 50, "IslandNorth", 12, 31, false, false));
			}
			if (Game1.IsMasterGame)
			{
				this.GenerateEntities();
			}
			this.pixelMap = null;
			this.SortLayers();
		}

		// Token: 0x06003291 RID: 12945 RVA: 0x002900FC File Offset: 0x0028E2FC
		public virtual void GenerateDirtTiles()
		{
			if (this.level.Value == 5)
			{
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				int center_x = this.generationRandom.Next(0, 64);
				int center_y = this.generationRandom.Next(0, 64);
				int travel_distance = this.generationRandom.Next(2, 8);
				int radius = this.generationRandom.Next(1, 3);
				int direction_x = (this.generationRandom.Next(2) == 0) ? -1 : 1;
				int direction_y = (this.generationRandom.Next(2) == 0) ? -1 : 1;
				bool x_oriented = this.generationRandom.Next(2) == 0;
				for (int j = 0; j < travel_distance; j++)
				{
					for (int x = center_x - radius; x <= center_x + radius; x++)
					{
						for (int y = center_y - radius; y <= center_y + radius; y++)
						{
							if (!(this.GetPixel(x, y, Color.Black) != Color.White))
							{
								this.dirtTiles.Add(new Point(x, y));
							}
						}
					}
					if (x_oriented)
					{
						direction_y += ((this.generationRandom.Next(2) == 0) ? -1 : 1);
					}
					else
					{
						direction_x += ((this.generationRandom.Next(2) == 0) ? -1 : 1);
					}
					center_x += direction_x;
					center_y += direction_y;
					radius += ((this.generationRandom.Next(2) == 0) ? -1 : 1);
					if (radius < 1)
					{
						radius = 1;
					}
					if (radius > 4)
					{
						radius = 4;
					}
				}
			}
			for (int k = 0; k < 2; k++)
			{
				this.ErodeInvalidDirtTiles();
			}
			HashSet<Point> visited_neighbors = new HashSet<Point>();
			Point[] neighboring_tiles = new Point[]
			{
				new Point(-1, -1),
				new Point(0, -1),
				new Point(1, -1),
				new Point(-1, 0),
				new Point(1, 0),
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 1)
			};
			foreach (Point point in this.dirtTiles)
			{
				this.SetTile(this.backLayer, point.X, point.Y, VolcanoDungeon.GetTileIndex(9, 1));
				if (this.generationRandom.NextDouble() < 0.015)
				{
					this.characters.Add(new Duggy(Utility.PointToVector2(point) * 64f, true));
				}
				foreach (Point offset in neighboring_tiles)
				{
					Point neighbor = new Point(point.X + offset.X, point.Y + offset.Y);
					if (!this.dirtTiles.Contains(neighbor) && !visited_neighbors.Contains(neighbor))
					{
						visited_neighbors.Add(neighbor);
						Point? neighbor_tile_offset = this.GetDirtNeighborTile(neighbor.X, neighbor.Y);
						if (neighbor_tile_offset != null)
						{
							this.SetTile(this.backLayer, neighbor.X, neighbor.Y, VolcanoDungeon.GetTileIndex(8 + neighbor_tile_offset.Value.X, neighbor_tile_offset.Value.Y));
						}
					}
				}
			}
		}

		// Token: 0x06003292 RID: 12946 RVA: 0x00290484 File Offset: 0x0028E684
		public virtual void CreateEntrance(Point? position)
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = 0; y <= 3; y++)
				{
					if (base.isTileOnMap(new Vector2((float)(position.Value.X + x), (float)(position.Value.Y + y))))
					{
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Back");
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Buildings");
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Front");
					}
				}
			}
			if (base.hasTileAt(position.Value.X - 1, position.Value.Y - 1, "Front", null))
			{
				this.SetTile(this.frontLayer, position.Value.X - 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(13, 16));
			}
			base.removeTile(position.Value.X, position.Value.Y - 1, "Front");
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y, VolcanoDungeon.GetTileIndex(13, 17));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(13, 18));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(13, 19));
			if (base.hasTileAt(position.Value.X + 1, position.Value.Y - 1, "Front", null))
			{
				this.SetTile(this.frontLayer, position.Value.X + 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(15, 16));
			}
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y, VolcanoDungeon.GetTileIndex(15, 17));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(15, 18));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(15, 19));
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y, VolcanoDungeon.GetTileIndex(14, 17));
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(14, 18));
			this.SetTile(this.frontLayer, position.Value.X, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(14, 19));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
		}

		// Token: 0x06003293 RID: 12947 RVA: 0x0029086C File Offset: 0x0028EA6C
		private void CreateExit(Point? position, bool draw_stairs = true)
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -4; y <= 0; y++)
				{
					if (base.isTileOnMap(new Vector2((float)(position.Value.X + x), (float)(position.Value.Y + y))))
					{
						if (draw_stairs)
						{
							base.removeTile(position.Value.X + x, position.Value.Y + y, "Back");
						}
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Buildings");
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Front");
					}
				}
			}
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y, VolcanoDungeon.GetTileIndex(9, 19));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(9, 18));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(9, 17));
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(9, 16));
			this.SetTile(this.alwaysFrontLayer, position.Value.X - 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.alwaysFrontLayer, position.Value.X, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.alwaysFrontLayer, position.Value.X + 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(10, 16));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y, VolcanoDungeon.GetTileIndex(11, 19));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(11, 18));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(11, 17));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(11, 16));
			if (draw_stairs)
			{
				this.SetTile(this.backLayer, position.Value.X, position.Value.Y, VolcanoDungeon.GetTileIndex(12, 19));
				this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(12, 18));
				this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(12, 17));
				this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(12, 16));
			}
			this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
			this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		}

		// Token: 0x06003294 RID: 12948 RVA: 0x00290CD0 File Offset: 0x0028EED0
		public virtual void ErodeInvalidDirtTiles()
		{
			Point[] neighboring_tiles = new Point[]
			{
				new Point(-1, -1),
				new Point(0, -1),
				new Point(1, -1),
				new Point(-1, 0),
				new Point(1, 0),
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 1)
			};
			Dictionary<Point, bool> visited_tiles = new Dictionary<Point, bool>();
			List<Point> dirt_to_remove = new List<Point>();
			foreach (Point dirt_tile in this.dirtTiles)
			{
				bool fail = false;
				foreach (Microsoft.Xna.Framework.Rectangle rect in VolcanoDungeon.setPieceAreas)
				{
					if (rect.Contains(dirt_tile))
					{
						fail = true;
						break;
					}
				}
				if (!fail && base.hasTileAt(dirt_tile, "Buildings", null))
				{
					fail = true;
				}
				if (!fail)
				{
					foreach (Point offset in neighboring_tiles)
					{
						Point neighbor = new Point(dirt_tile.X + offset.X, dirt_tile.Y + offset.Y);
						bool prevSucceeded;
						if (visited_tiles.TryGetValue(neighbor, out prevSucceeded))
						{
							if (!prevSucceeded)
							{
								fail = true;
								break;
							}
						}
						else if (!this.dirtTiles.Contains(neighbor))
						{
							if (this.GetDirtNeighborTile(neighbor.X, neighbor.Y) == null)
							{
								fail = true;
							}
							visited_tiles[neighbor] = !fail;
							if (fail)
							{
								break;
							}
						}
					}
				}
				if (fail)
				{
					dirt_to_remove.Add(dirt_tile);
				}
			}
			foreach (Point remove in dirt_to_remove)
			{
				this.dirtTiles.Remove(remove);
			}
		}

		// Token: 0x06003295 RID: 12949 RVA: 0x00290F2C File Offset: 0x0028F12C
		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			base.monsterDrop(monster, x, y, who);
			if (Game1.random.NextDouble() < 0.05)
			{
				Game1.player.team.RequestLimitedNutDrops("VolcanoMonsterDrop", this, x, y, 5, 1);
			}
		}

		// Token: 0x06003296 RID: 12950 RVA: 0x00290F68 File Offset: 0x0028F168
		public Point? GetDirtNeighborTile(int tile_x, int tile_y)
		{
			if (this.GetPixel(tile_x, tile_y, Color.Black) != Color.White)
			{
				return null;
			}
			if (base.hasTileAt(new Point(tile_x, tile_y), "Buildings", null))
			{
				return null;
			}
			if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
			{
				return null;
			}
			if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
			{
				return null;
			}
			if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && !this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
			{
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
				{
					return new Point?(new Point(3, 3));
				}
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
				{
					return new Point?(new Point(3, 1));
				}
				return new Point?(new Point(2, 1));
			}
			else if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)) && !this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)))
			{
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
				{
					return new Point?(new Point(3, 2));
				}
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
				{
					return new Point?(new Point(3, 0));
				}
				return new Point?(new Point(0, 1));
			}
			else
			{
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && !this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
				{
					return new Point?(new Point(1, 2));
				}
				if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)) && !this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
				{
					return new Point?(new Point(1, 0));
				}
				if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y - 1)))
				{
					return new Point?(new Point(2, 2));
				}
				if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y - 1)))
				{
					return new Point?(new Point(0, 2));
				}
				if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y + 1)))
				{
					return new Point?(new Point(0, 2));
				}
				if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y + 1)))
				{
					return new Point?(new Point(2, 2));
				}
				return null;
			}
		}

		// Token: 0x06003297 RID: 12951 RVA: 0x00291230 File Offset: 0x0028F430
		public virtual void CreateDwarfGate(int gate_index, Point tile_position)
		{
			this.SetTile(this.backLayer, tile_position.X, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(3, 34));
			this.SetTile(this.buildingsLayer, tile_position.X - 1, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(2, 34));
			this.SetTile(this.buildingsLayer, tile_position.X + 1, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(4, 34));
			this.SetTile(this.buildingsLayer, tile_position.X - 1, tile_position.Y, VolcanoDungeon.GetTileIndex(2, 33));
			this.SetTile(this.buildingsLayer, tile_position.X + 1, tile_position.Y, VolcanoDungeon.GetTileIndex(4, 33));
			this.SetTile(this.frontLayer, tile_position.X - 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(2, 32));
			this.SetTile(this.frontLayer, tile_position.X + 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(4, 32));
			this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(2, 32));
			this.SetTile(this.alwaysFrontLayer, tile_position.X, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(3, 32));
			this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(4, 32));
			if (gate_index == 0)
			{
				this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(0, 32));
				this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(0, 32));
			}
			else
			{
				this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(9, 25));
				this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(10, 25));
			}
			int seed = this.generationRandom.Next();
			if (Game1.IsMasterGame)
			{
				DwarfGate gate = new DwarfGate(this, gate_index, tile_position.X, tile_position.Y, seed);
				this.dwarfGates.Add(gate);
			}
		}

		// Token: 0x06003298 RID: 12952 RVA: 0x00291468 File Offset: 0x0028F668
		public virtual void AddPossibleSwitchLocation(int switch_index, int x, int y)
		{
			List<Point> positions;
			if (!this.possibleSwitchPositions.TryGetValue(switch_index, out positions))
			{
				positions = (this.possibleSwitchPositions[switch_index] = new List<Point>());
			}
			positions.Add(new Point(x, y));
		}

		// Token: 0x06003299 RID: 12953 RVA: 0x002914A8 File Offset: 0x0028F6A8
		public virtual void AddPossibleGateLocation(int gate_index, int x, int y)
		{
			List<Point> positions;
			if (!this.possibleGatePositions.TryGetValue(gate_index, out positions))
			{
				positions = (this.possibleGatePositions[gate_index] = new List<Point>());
			}
			positions.Add(new Point(x, y));
		}

		// Token: 0x0600329A RID: 12954 RVA: 0x002914E8 File Offset: 0x0028F6E8
		private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
		{
			if (this.level.Value == 0 || this.level.Value == 5)
			{
				monsterChance = 0.0;
				itemChance = 0.0;
				gemStoneChance = 0.0;
				stoneChance = 0.0;
			}
			if (this.isMushroomLevel())
			{
				monsterChance = 0.025;
				itemChance *= 35.0;
				stoneChance = 0.0;
			}
			else if (this.isMonsterLevel())
			{
				stoneChance = 0.0;
				itemChance = 0.0;
				monsterChance *= 2.0;
			}
			bool has_avoid_monsters_buff = false;
			bool has_spawn_monsters_buff = false;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.hasBuff("23"))
				{
					has_avoid_monsters_buff = true;
				}
				if (farmer.hasBuff("24"))
				{
					has_spawn_monsters_buff = true;
				}
				if (has_spawn_monsters_buff && has_avoid_monsters_buff)
				{
					break;
				}
			}
			if (has_spawn_monsters_buff)
			{
				monsterChance *= 2.0;
			}
			gemStoneChance /= 2.0;
		}

		// Token: 0x0600329B RID: 12955 RVA: 0x00291620 File Offset: 0x0028F820
		public bool isTileClearForMineObjects(Vector2 v, bool ignoreRuins = false)
		{
			if ((Math.Abs((float)this.startPosition.Value.X - v.X) <= 2f && Math.Abs((float)this.startPosition.Value.Y - v.Y) <= 2f) || (Math.Abs((float)this.endPosition.Value.X - v.X) <= 2f && Math.Abs((float)this.endPosition.Value.Y - v.Y) <= 2f))
			{
				return false;
			}
			if (this.GetPixel((int)v.X, (int)v.Y, Color.Black) == new Color(128, 128, 128))
			{
				return false;
			}
			if (!this.CanItemBePlacedHere(v, false, CollisionMask.All, CollisionMask.None, false, false))
			{
				return false;
			}
			string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back", false);
			if (s == null || !s.Equals("Stone"))
			{
				return false;
			}
			if (!this.isTileOnClearAndSolidGround(v))
			{
				return false;
			}
			if (this.objects.ContainsKey(v))
			{
				return false;
			}
			if (ignoreRuins)
			{
				int tileIndex = base.getTileIndexAt((int)v.X, (int)v.Y, "Back", "dungeon");
				if (tileIndex == -1 || tileIndex >= 384)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600329C RID: 12956 RVA: 0x00291788 File Offset: 0x0028F988
		public bool isTileOnClearAndSolidGround(Vector2 v)
		{
			return this.map.RequireLayer("Back").Tiles[(int)v.X, (int)v.Y] != null && this.map.RequireLayer("Front").Tiles[(int)v.X, (int)v.Y] == null && this.map.RequireLayer("Buildings").Tiles[(int)v.X, (int)v.Y] == null;
		}

		// Token: 0x0600329D RID: 12957 RVA: 0x00291818 File Offset: 0x0028FA18
		public virtual void GenerateEntities()
		{
			List<Point> spawn_points = new List<Point>();
			this.ApplyToColor(new Color(0, 255, 255), delegate(int x, int y)
			{
				spawn_points.Add(new Point(x, y));
			});
			List<Point> spiker_spawn_points = new List<Point>();
			this.ApplyToColor(new Color(0, 128, 255), delegate(int x, int y)
			{
				spiker_spawn_points.Add(new Point(x, y));
			});
			double stoneChance = (double)this.generationRandom.Next(11, 18) / 150.0;
			double monsterChance = 0.0008 + (double)this.generationRandom.Next(70) / 10000.0;
			double itemChance = 0.001;
			double gemStoneChance = 0.003;
			this.adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
			if (this.level.Value > 0 && this.level.Value != 5 && (this.generationRandom.NextBool() || this.isMushroomLevel()))
			{
				int numBarrels = this.generationRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(Game1.currentLocation) * 20.0);
				if (this.isMushroomLevel())
				{
					numBarrels += 50;
				}
				for (int i = 0; i < numBarrels; i++)
				{
					Point p;
					Point motion;
					if (this.generationRandom.NextDouble() < 0.33)
					{
						p = new Point(this.generationRandom.Next(this.map.RequireLayer("Back").LayerWidth), 0);
						motion = new Point(0, 1);
					}
					else if (this.generationRandom.NextBool())
					{
						p = new Point(0, this.generationRandom.Next(this.map.RequireLayer("Back").LayerHeight));
						motion = new Point(1, 0);
					}
					else
					{
						p = new Point(this.map.RequireLayer("Back").LayerWidth - 1, this.generationRandom.Next(this.map.RequireLayer("Back").LayerHeight));
						motion = new Point(-1, 0);
					}
					while (base.isTileOnMap(p.X, p.Y))
					{
						p.X += motion.X;
						p.Y += motion.Y;
						if (this.isTileClearForMineObjects(new Vector2((float)p.X, (float)p.Y), false))
						{
							Vector2 objectPos = new Vector2((float)p.X, (float)p.Y);
							if (this.isMushroomLevel())
							{
								this.terrainFeatures.Add(objectPos, new CosmeticPlant(6 + this.generationRandom.Next(3)));
								break;
							}
							this.objects.Add(objectPos, BreakableContainer.GetBarrelForVolcanoDungeon(objectPos));
							break;
						}
					}
				}
			}
			if (this.level.Value != 5)
			{
				for (int x2 = 0; x2 < this.map.Layers[0].LayerWidth; x2++)
				{
					for (int y2 = 0; y2 < this.map.Layers[0].LayerHeight; y2++)
					{
						Vector2 objectPos2 = new Vector2((float)x2, (float)y2);
						if ((Math.Abs((float)this.startPosition.Value.X - objectPos2.X) > 5f || Math.Abs((float)this.startPosition.Value.Y - objectPos2.Y) > 5f) && (Math.Abs((float)this.endPosition.Value.X - objectPos2.X) > 5f || Math.Abs((float)this.endPosition.Value.Y - objectPos2.Y) > 5f))
						{
							if (this.CanItemBePlacedHere(objectPos2, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && this.generationRandom.NextDouble() < monsterChance)
							{
								if (base.getTileIndexAt((int)objectPos2.X, (int)objectPos2.Y, "Back", "dungeon") == 25)
								{
									if (!this.isMushroomLevel())
									{
										this.characters.Add(new Duggy(objectPos2 * 64f, true));
									}
								}
								else if (this.isMushroomLevel())
								{
									this.characters.Add(new RockCrab(objectPos2 * 64f, "False Magma Cap"));
								}
								else
								{
									this.characters.Add(new Bat(objectPos2 * 64f, (this.level.Value > 5 && this.generationRandom.NextBool()) ? -556 : -555));
								}
							}
							else if (this.isTileClearForMineObjects(objectPos2, true))
							{
								double chance = stoneChance;
								if (chance > 0.0)
								{
									foreach (Vector2 v in Utility.getAdjacentTileLocations(objectPos2))
									{
										if (this.objects.ContainsKey(v))
										{
											chance += 0.1;
										}
									}
								}
								int stoneIndex = this.chooseStoneTypeIndexOnly(objectPos2);
								bool basicStone = stoneIndex >= 845 && stoneIndex <= 847;
								if (chance > 0.0 && (!basicStone || this.generationRandom.NextDouble() < chance))
								{
									Object stone = this.createStone(stoneIndex, objectPos2);
									if (stone != null)
									{
										base.Objects.Add(objectPos2, stone);
									}
								}
								else if (this.generationRandom.NextDouble() < itemChance)
								{
									base.Objects.Add(objectPos2, new Object("851", 1, false, -1, 0)
									{
										IsSpawnedObject = true,
										CanBeGrabbed = true
									});
								}
							}
						}
					}
				}
				while (stoneChance != 0.0 && this.generationRandom.NextDouble() < 0.2)
				{
					this.tryToAddOreClumps();
				}
			}
			int j = 0;
			while (j < 7 && spawn_points.Count != 0)
			{
				int index = this.generationRandom.Next(0, spawn_points.Count);
				Point spawn_point = spawn_points[index];
				if (this.CanItemBePlacedHere(new Vector2((float)spawn_point.X, (float)spawn_point.Y), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					Monster monster = null;
					if (this.generationRandom.NextDouble() <= 0.25)
					{
						for (int k = 0; k < 20; k++)
						{
							Point point = spawn_point;
							point.X += this.generationRandom.Next(-10, 11);
							point.Y += this.generationRandom.Next(-10, 11);
							bool fail = false;
							for (int check_x = -1; check_x <= 1; check_x++)
							{
								for (int check_y = -1; check_y <= 1; check_y++)
								{
									if (!LavaLurk.IsLavaTile(this, point.X + check_x, point.Y + check_y))
									{
										fail = true;
										break;
									}
								}
							}
							if (!fail)
							{
								monster = new LavaLurk(Utility.PointToVector2(point) * 64f);
								break;
							}
						}
					}
					if (monster == null && this.generationRandom.NextDouble() <= 0.20000000298023224)
					{
						monster = new HotHead(Utility.PointToVector2(spawn_point) * 64f);
					}
					if (monster == null)
					{
						GreenSlime greenSlime = new GreenSlime(Utility.PointToVector2(spawn_point) * 64f, 0);
						greenSlime.makeTigerSlime(false);
						monster = greenSlime;
					}
					if (monster != null)
					{
						this.characters.Add(monster);
					}
				}
				spawn_points.RemoveAt(index);
				j++;
			}
			foreach (Point p2 in spiker_spawn_points)
			{
				if (this.CanSpawnCharacterHere(new Vector2((float)p2.X, (float)p2.Y)))
				{
					int direction = 1;
					int tileIndexAt = base.getTileIndexAt(p2, "Back", "dungeon");
					if (tileIndexAt <= 552)
					{
						if (tileIndexAt - 537 > 1)
						{
							if (tileIndexAt == 552)
							{
								goto IL_85E;
							}
						}
						else
						{
							direction = 2;
						}
					}
					else
					{
						if (tileIndexAt != 553)
						{
							if (tileIndexAt == 569)
							{
								goto IL_85E;
							}
							if (tileIndexAt != 570)
							{
								goto IL_866;
							}
						}
						direction = 0;
					}
					IL_866:
					this.characters.Add(new Spiker(new Vector2((float)p2.X, (float)p2.Y) * 64f, direction));
					continue;
					IL_85E:
					direction = 3;
					goto IL_866;
				}
			}
		}

		// Token: 0x0600329E RID: 12958 RVA: 0x002920F4 File Offset: 0x002902F4
		private Object createStone(int stone, Vector2 tile)
		{
			string whichStone = this.chooseStoneTypeIndexOnly(tile).ToString() ?? "";
			int stoneHealth = 1;
			if (whichStone != null)
			{
				int length = whichStone.Length;
				if (length != 3)
				{
					if (length == 7)
					{
						if (whichStone == "1095382")
						{
							whichStone = (Game1.random.NextBool() ? "VolcanoCoalNode0" : "VolcanoCoalNode1");
							stoneHealth = 10;
						}
					}
				}
				else
				{
					switch (whichStone[2])
					{
					case '0':
						if (!(whichStone == "290"))
						{
							goto IL_16F;
						}
						stoneHealth = 8;
						goto IL_16F;
					case '1':
						if (!(whichStone == "751"))
						{
							goto IL_16F;
						}
						stoneHealth = 8;
						goto IL_16F;
					case '2':
					case '8':
						goto IL_16F;
					case '3':
						if (!(whichStone == "843"))
						{
							goto IL_16F;
						}
						goto IL_151;
					case '4':
						if (whichStone == "844")
						{
							goto IL_151;
						}
						if (!(whichStone == "764"))
						{
							goto IL_16F;
						}
						whichStone = "VolcanoGoldNode";
						stoneHealth = 8;
						goto IL_16F;
					case '5':
						if (!(whichStone == "845"))
						{
							if (!(whichStone == "765"))
							{
								goto IL_16F;
							}
							stoneHealth = 16;
							goto IL_16F;
						}
						break;
					case '6':
						if (!(whichStone == "846"))
						{
							goto IL_16F;
						}
						break;
					case '7':
						if (!(whichStone == "847"))
						{
							goto IL_16F;
						}
						break;
					case '9':
						if (!(whichStone == "819"))
						{
							goto IL_16F;
						}
						stoneHealth = 8;
						goto IL_16F;
					default:
						goto IL_16F;
					}
					stoneHealth = 6;
					goto IL_16F;
					IL_151:
					stoneHealth = 12;
				}
			}
			IL_16F:
			return new Object(whichStone, 1, false, -1, 0)
			{
				MinutesUntilReady = stoneHealth
			};
		}

		// Token: 0x0600329F RID: 12959 RVA: 0x00292284 File Offset: 0x00290484
		private int chooseStoneTypeIndexOnly(Vector2 tile)
		{
			int whichStone = this.generationRandom.Next(845, 848);
			float levelMod = 1f + (float)this.level.Value / 7f;
			float masterMultiplier = 0.8f;
			float luckMultiplier = 1f + (float)Game1.player.team.AverageLuckLevel(null) * 0.035f + (float)Game1.player.team.AverageDailyLuck(null) / 2f;
			double chance = 0.008 * (double)levelMod * (double)masterMultiplier * (double)luckMultiplier;
			foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
			{
				Object obj;
				if (this.objects.TryGetValue(v, out obj) && (obj.QualifiedItemId == "(O)843" || obj.QualifiedItemId == "(O)844"))
				{
					chance += 0.15;
				}
			}
			if (this.generationRandom.NextDouble() < chance)
			{
				whichStone = this.generationRandom.Next(843, 845);
			}
			else
			{
				chance = 0.0025 * (double)levelMod * (double)masterMultiplier * (double)luckMultiplier;
				foreach (Vector2 v2 in Utility.getAdjacentTileLocations(tile))
				{
					Object obj2;
					if (this.objects.TryGetValue(v2, out obj2) && obj2.QualifiedItemId == "(O)765")
					{
						chance += 0.1;
					}
				}
				if (this.generationRandom.NextDouble() < chance)
				{
					whichStone = 765;
				}
				else
				{
					chance = 0.01 * (double)levelMod * (double)masterMultiplier;
					foreach (Vector2 v3 in Utility.getAdjacentTileLocations(tile))
					{
						Object obj3;
						if (this.objects.TryGetValue(v3, out obj3) && obj3.QualifiedItemId == "(O)VolcanoGoldNode")
						{
							chance += 0.2;
						}
					}
					if (this.generationRandom.NextDouble() < chance)
					{
						whichStone = 764;
					}
					else
					{
						chance = 0.012 * (double)levelMod * (double)masterMultiplier;
						foreach (Vector2 v4 in Utility.getAdjacentTileLocations(tile))
						{
							Object obj4;
							if (this.objects.TryGetValue(v4, out obj4) && obj4.QualifiedItemId.StartsWith("(O)VolcanoCoalNode"))
							{
								chance += 0.2;
							}
						}
						if (this.generationRandom.NextDouble() < chance)
						{
							whichStone = 1095382;
						}
						else
						{
							chance = 0.015 * (double)levelMod * (double)masterMultiplier;
							foreach (Vector2 v5 in Utility.getAdjacentTileLocations(tile))
							{
								Object obj5;
								if (this.objects.TryGetValue(v5, out obj5) && obj5.QualifiedItemId == "(O)850")
								{
									chance += 0.25;
								}
							}
							if (this.generationRandom.NextDouble() < chance)
							{
								whichStone = 850;
							}
							else
							{
								chance = 0.018 * (double)levelMod * (double)masterMultiplier;
								foreach (Vector2 v6 in Utility.getAdjacentTileLocations(tile))
								{
									Object obj6;
									if (this.objects.TryGetValue(v6, out obj6) && obj6.QualifiedItemId == "(O)849")
									{
										chance += 0.25;
									}
								}
								if (this.generationRandom.NextDouble() < chance)
								{
									whichStone = 849;
								}
							}
						}
					}
				}
			}
			if (this.generationRandom.NextDouble() < 0.0005)
			{
				whichStone = 819;
			}
			if (this.generationRandom.NextDouble() < 0.0007)
			{
				whichStone = 44;
			}
			if (this.level.Value > 2 && this.generationRandom.NextDouble() < 0.0002)
			{
				whichStone = 46;
			}
			return whichStone;
		}

		// Token: 0x060032A0 RID: 12960 RVA: 0x00292720 File Offset: 0x00290920
		public void tryToAddOreClumps()
		{
			if (this.generationRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(Game1.currentLocation))
			{
				Vector2 endPoint = base.getRandomTile(null);
				int tries = 0;
				while (tries < 1 || this.generationRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(Game1.currentLocation))
				{
					if (this.CanItemBePlacedHere(endPoint, false, CollisionMask.All, CollisionMask.None, false, false) && this.isTileOnClearAndSolidGround(endPoint) && this.doesTileHaveProperty((int)endPoint.X, (int)endPoint.Y, "Diggable", "Back", false) == null)
					{
						Utility.recursiveObjectPlacement(new Object(this.generationRandom.Next(843, 845).ToString(), 1, false, -1, 0)
						{
							MinutesUntilReady = 12
						}, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", 0, 0.05000000074505806, 1, null);
					}
					endPoint = base.getRandomTile(null);
					tries++;
				}
			}
		}

		// Token: 0x060032A1 RID: 12961 RVA: 0x00292854 File Offset: 0x00290A54
		public virtual void ApplySetPieces()
		{
			for (int i = 0; i < VolcanoDungeon.setPieceAreas.Count; i++)
			{
				Microsoft.Xna.Framework.Rectangle rectangle = VolcanoDungeon.setPieceAreas[i];
				int size = 3;
				if (rectangle.Width >= 32)
				{
					size = 32;
				}
				else if (rectangle.Width >= 16)
				{
					size = 16;
				}
				else if (rectangle.Width >= 8)
				{
					size = 8;
				}
				else if (rectangle.Width >= 4)
				{
					size = 4;
				}
				Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\Mines\\Volcano_SetPieces_" + size.ToString());
				int cols = override_map.Layers[0].LayerWidth / size;
				int rows = override_map.Layers[0].LayerHeight / size;
				int selected_col = this.generationRandom.Next(0, cols);
				int selected_row = this.generationRandom.Next(0, rows);
				base.ApplyMapOverride(override_map, "area_" + i.ToString(), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(selected_col * size, selected_row * size, size, size)), new Microsoft.Xna.Framework.Rectangle?(rectangle), null);
				Layer paths_layer = override_map.GetLayer("Paths");
				if (paths_layer != null)
				{
					for (int x = 0; x < size; x++)
					{
						for (int y = 0; y <= size; y++)
						{
							int source_x = selected_col * size + x;
							int source_y = selected_row * size + y;
							int dest_x = rectangle.Left + x;
							int dest_y = rectangle.Top + y;
							if (paths_layer.IsValidTileLocation(source_x, source_y))
							{
								Tile tile = paths_layer.Tiles[source_x, source_y];
								int path_index = (tile != null) ? tile.TileIndex : -1;
								if (path_index >= VolcanoDungeon.GetTileIndex(10, 14) && path_index <= VolcanoDungeon.GetTileIndex(15, 14))
								{
									int index = path_index - VolcanoDungeon.GetTileIndex(10, 14);
									if (index > 0)
									{
										index += i * 10;
									}
									double chance = 1.0;
									string property;
									if (tile.Properties.TryGetValue("Chance", out property) && !double.TryParse(property, out chance))
									{
										chance = 1.0;
									}
									if (this.generationRandom.NextDouble() < chance)
									{
										this.AddPossibleGateLocation(index, dest_x, dest_y);
									}
								}
								else if (path_index >= VolcanoDungeon.GetTileIndex(10, 15) && path_index <= VolcanoDungeon.GetTileIndex(15, 15))
								{
									int index2 = path_index - VolcanoDungeon.GetTileIndex(10, 15);
									if (index2 > 0)
									{
										index2 += i * 10;
									}
									this.AddPossibleSwitchLocation(index2, dest_x, dest_y);
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(10, 20))
								{
									this.SetPixelMap(dest_x, dest_y, new Color(0, 255, 255));
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(11, 20))
								{
									this.SetPixelMap(dest_x, dest_y, new Color(0, 0, 255));
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(12, 20))
								{
									this.SpawnChest(dest_x, dest_y);
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(13, 20))
								{
									this.SetPixelMap(dest_x, dest_y, new Color(0, 0, 0));
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(14, 20) && this.generationRandom.NextBool())
								{
									if (Game1.IsMasterGame)
									{
										this.objects.Add(new Vector2((float)dest_x, (float)dest_y), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2((float)dest_x, (float)dest_y)));
									}
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(15, 20) && this.generationRandom.NextBool())
								{
									if (Game1.IsMasterGame)
									{
										Vector2 objTile = new Vector2((float)dest_x, (float)dest_y);
										this.objects.Add(objTile, new Object("852", 1, false, -1, 0)
										{
											IsSpawnedObject = true,
											CanBeGrabbed = true
										});
									}
								}
								else if (path_index == VolcanoDungeon.GetTileIndex(10, 21))
								{
									this.SetPixelMap(dest_x, dest_y, new Color(0, 128, 255));
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060032A2 RID: 12962 RVA: 0x00292C38 File Offset: 0x00290E38
		public virtual void SpawnChest(int tile_x, int tile_y)
		{
			Random chest_random = Utility.CreateRandom((double)this.generationRandom.Next(), 0.0, 0.0, 0.0, 0.0);
			float extraRare_luckboost = (float)Game1.player.team.AverageLuckLevel(null) * 0.035f + (float)Game1.player.team.AverageDailyLuck(null) / 2f;
			if (Game1.IsMasterGame)
			{
				Vector2 position = new Vector2((float)tile_x, (float)tile_y);
				Chest chest = new Chest(false, position, "130");
				chest.dropContents.Value = true;
				chest.synchronized.Value = true;
				chest.type.Value = "interactive";
				if (chest_random.NextDouble() < (double)((this.level.Value == 9) ? (0.5f + extraRare_luckboost) : (0.1f + extraRare_luckboost)))
				{
					chest.SetBigCraftableSpriteIndex(227, -1, 3);
					this.PopulateChest(chest.Items, chest_random, 1);
				}
				else
				{
					chest.SetBigCraftableSpriteIndex(223, -1, 3);
					this.PopulateChest(chest.Items, chest_random, 0);
				}
				base.setObject(position, chest);
			}
		}

		// Token: 0x060032A3 RID: 12963 RVA: 0x00292D60 File Offset: 0x00290F60
		protected override bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
		{
			if (who != null && (stoneId == "845" || stoneId == "846" || stoneId == "847") && Game1.random.NextDouble() < 0.005)
			{
				Game1.createObjectDebris("(O)827", x, y, who.UniqueMultiplayerID, this);
			}
			if (who != null && r.NextDouble() < 0.03)
			{
				Game1.player.team.RequestLimitedNutDrops("VolcanoMining", this, x * 64, y * 64, 5, 1);
			}
			return base.breakStone(stoneId, x, y, who, r);
		}

		// Token: 0x060032A4 RID: 12964 RVA: 0x00292E04 File Offset: 0x00291004
		public virtual void PopulateChest(IList<Item> items, Random chest_random, int chest_type)
		{
			if (chest_type != 0)
			{
				if (chest_type != 1)
				{
					return;
				}
				int random_count = 9;
				int random = chest_random.Next(random_count);
				if (!Game1.netWorldState.Value.GoldenCoconutCracked)
				{
					while (random == 3)
					{
						random = chest_random.Next(random_count);
					}
				}
				if (Game1.random.NextDouble() <= 1.0 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
				{
					int num = chest_random.Next(4, 6);
					for (int i = 0; i < num; i++)
					{
						items.Add(ItemRegistry.Create("(O)890", 1, 0, false));
					}
				}
				switch (random)
				{
				case 0:
					for (int j = 0; j < 10; j++)
					{
						items.Add(ItemRegistry.Create("(O)848", 1, 0, false));
					}
					return;
				case 1:
					items.Add(ItemRegistry.Create("(B)854", 1, 0, false));
					return;
				case 2:
					items.Add(ItemRegistry.Create("(B)855", 1, 0, false));
					return;
				case 3:
					for (int k = 0; k < 3; k++)
					{
						items.Add(ItemRegistry.Create<Object>("(O)791", 1, 0, false));
					}
					return;
				case 4:
					items.Add(new Ring("863"));
					return;
				case 5:
					items.Add(new Ring("860"));
					return;
				case 6:
					items.Add(MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)" + chest_random.Next(57, 60).ToString(), 1, 0, false), chest_random, false, null));
					return;
				case 7:
					items.Add(ItemRegistry.Create("(H)76", 1, 0, false));
					return;
				default:
					items.Add(ItemRegistry.Create("(O)289", 1, 0, false));
					return;
				}
			}
			else
			{
				int random_count2 = 7;
				int random2 = chest_random.Next(random_count2);
				if (!Game1.netWorldState.Value.GoldenCoconutCracked)
				{
					while (random2 == 1)
					{
						random2 = chest_random.Next(random_count2);
					}
				}
				if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
				{
					int num2 = chest_random.Next(2, 6);
					for (int l = 0; l < num2; l++)
					{
						items.Add(ItemRegistry.Create("(O)890", 1, 0, false));
					}
				}
				switch (random2)
				{
				case 0:
					for (int m = 0; m < 3; m++)
					{
						items.Add(ItemRegistry.Create("(O)848", 1, 0, false));
					}
					return;
				case 1:
					items.Add(ItemRegistry.Create("(O)791", 1, 0, false));
					return;
				case 2:
					for (int n = 0; n < 8; n++)
					{
						items.Add(ItemRegistry.Create("(O)831", 1, 0, false));
					}
					return;
				case 3:
					for (int i2 = 0; i2 < 5; i2++)
					{
						items.Add(ItemRegistry.Create("(O)833", 1, 0, false));
					}
					return;
				case 4:
					items.Add(new Ring("861"));
					return;
				case 5:
					items.Add(new Ring("862"));
					return;
				default:
					items.Add(MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)" + chest_random.Next(54, 57).ToString(), 1, 0, false), chest_random, false, null));
					return;
				}
			}
		}

		// Token: 0x060032A5 RID: 12965 RVA: 0x00293130 File Offset: 0x00291330
		public virtual void ApplyToColor(Color match, Action<int, int> action)
		{
			for (int x = 0; x < this.mapWidth; x++)
			{
				for (int y = 0; y < this.mapHeight; y++)
				{
					if (this.GetPixel(x, y, match) == match && action != null)
					{
						action(x, y);
					}
				}
			}
		}

		// Token: 0x060032A6 RID: 12966 RVA: 0x0029317B File Offset: 0x0029137B
		public override bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
		{
			return !this.cooledLavaTiles.ContainsKey(chunkTile) && base.sinkDebris(debris, chunkTile, chunkPosition);
		}

		// Token: 0x060032A7 RID: 12967 RVA: 0x00293198 File Offset: 0x00291398
		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (this.level.Value != 5 && t is WateringCan && base.isTileOnMap(new Vector2((float)tileX, (float)tileY)) && this.waterTiles[tileX, tileY] && !this.cooledLavaTiles.ContainsKey(new Vector2((float)tileX, (float)tileY)))
			{
				this.coolLavaEvent.Fire(new Point(tileX, tileY));
			}
			return base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x060032A8 RID: 12968 RVA: 0x0029320C File Offset: 0x0029140C
		public virtual void GenerateBlobs(Color match, int tile_x, int tile_y, bool fill_center = true, bool is_lava_pool = false)
		{
			for (int x = 0; x < this.mapWidth; x++)
			{
				for (int y = 0; y < this.mapHeight; y++)
				{
					if (this.GetPixel(x, y, match) == match)
					{
						int value = this.GetNeighborValue(x, y, match, is_lava_pool);
						if (fill_center || value != 15)
						{
							Dictionary<int, Point> blob_lookup = this.GetBlobLookup();
							if (is_lava_pool)
							{
								blob_lookup = this.GetLavaBlobLookup();
							}
							Point offset;
							if (blob_lookup.TryGetValue(value, out offset))
							{
								this.SetTile(this.buildingsLayer, x, y, VolcanoDungeon.GetTileIndex(tile_x + offset.X, tile_y + offset.Y));
							}
						}
					}
				}
			}
		}

		// Token: 0x060032A9 RID: 12969 RVA: 0x002932A8 File Offset: 0x002914A8
		public Dictionary<int, Point> GetBlobLookup()
		{
			if (VolcanoDungeon._blobIndexLookup == null)
			{
				VolcanoDungeon._blobIndexLookup = new Dictionary<int, Point>();
				VolcanoDungeon._blobIndexLookup[0] = new Point(0, 0);
				VolcanoDungeon._blobIndexLookup[6] = new Point(1, 0);
				VolcanoDungeon._blobIndexLookup[14] = new Point(2, 0);
				VolcanoDungeon._blobIndexLookup[10] = new Point(3, 0);
				VolcanoDungeon._blobIndexLookup[7] = new Point(1, 1);
				VolcanoDungeon._blobIndexLookup[11] = new Point(3, 1);
				VolcanoDungeon._blobIndexLookup[5] = new Point(1, 2);
				VolcanoDungeon._blobIndexLookup[13] = new Point(2, 2);
				VolcanoDungeon._blobIndexLookup[9] = new Point(3, 2);
				VolcanoDungeon._blobIndexLookup[2] = new Point(0, 1);
				VolcanoDungeon._blobIndexLookup[3] = new Point(0, 2);
				VolcanoDungeon._blobIndexLookup[1] = new Point(0, 3);
				VolcanoDungeon._blobIndexLookup[4] = new Point(1, 3);
				VolcanoDungeon._blobIndexLookup[12] = new Point(2, 3);
				VolcanoDungeon._blobIndexLookup[8] = new Point(3, 3);
				VolcanoDungeon._blobIndexLookup[15] = new Point(2, 1);
			}
			return VolcanoDungeon._blobIndexLookup;
		}

		// Token: 0x060032AA RID: 12970 RVA: 0x002933F8 File Offset: 0x002915F8
		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
		{
			return (isFarmer && !glider && (position.Left < 0 || position.Right > this.map.DisplayWidth || position.Top < 0 || position.Bottom > this.map.DisplayHeight)) || base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement, false);
		}

		// Token: 0x060032AB RID: 12971 RVA: 0x00293460 File Offset: 0x00291660
		public Dictionary<int, Point> GetLavaBlobLookup()
		{
			if (VolcanoDungeon._lavaBlobIndexLookup == null)
			{
				VolcanoDungeon._lavaBlobIndexLookup = new Dictionary<int, Point>(this.GetBlobLookup());
				VolcanoDungeon._lavaBlobIndexLookup[63] = new Point(2, 1);
				VolcanoDungeon._lavaBlobIndexLookup[47] = new Point(4, 3);
				VolcanoDungeon._lavaBlobIndexLookup[31] = new Point(4, 2);
				VolcanoDungeon._lavaBlobIndexLookup[15] = new Point(4, 1);
			}
			return VolcanoDungeon._lavaBlobIndexLookup;
		}

		// Token: 0x060032AC RID: 12972 RVA: 0x002934D8 File Offset: 0x002916D8
		public virtual void GenerateWalls(Color match, int source_x, int source_y, int wall_height = 4, int random_wall_variants = 1, bool start_in_wall = false, Action<int, int> on_insufficient_wall_height = null, bool use_corner_hack = false)
		{
			this.heightMap = new int[this.mapWidth * this.mapHeight];
			for (int i = 0; i < this.heightMap.Length; i++)
			{
				this.heightMap[i] = -1;
			}
			for (int pass = 0; pass < 2; pass++)
			{
				for (int x = 0; x < this.mapWidth; x++)
				{
					int last_y = -1;
					int clearance = 0;
					if (start_in_wall)
					{
						clearance = wall_height;
					}
					for (int current_y = 0; current_y <= this.mapHeight; current_y++)
					{
						if (this.GetPixel(x, current_y, match) != match || current_y >= this.mapHeight)
						{
							int current_height = 0;
							int wall_variant_index = 0;
							if (random_wall_variants > 1 && this.generationRandom.NextBool())
							{
								wall_variant_index = this.generationRandom.Next(1, random_wall_variants);
							}
							if (current_y >= this.mapHeight)
							{
								current_height = wall_height;
								clearance = wall_height;
							}
							for (int curr_y = current_y - 1; curr_y > last_y; curr_y--)
							{
								if (clearance < wall_height)
								{
									if (on_insufficient_wall_height != null)
									{
										on_insufficient_wall_height(x, curr_y);
									}
									else
									{
										this.SetPixelMap(x, curr_y, Color.White);
										this.PlaceSingleWall(x, curr_y);
									}
									current_height--;
								}
								else if (pass != 0)
								{
									if (pass == 1)
									{
										this.heightMap[x + curr_y * this.mapWidth] = current_height + 1;
										if (current_height < wall_height || wall_height == 0)
										{
											if (wall_height > 0)
											{
												this.SetTile(this.buildingsLayer, x, curr_y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + wall_variant_index, source_y + 1 + random_wall_variants + wall_height - current_height - 1));
											}
										}
										else
										{
											this.SetTile(this.buildingsLayer, x, curr_y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
										}
									}
								}
								else if (this.GetPixelClearance(x - 1, curr_y, wall_height, match) < wall_height && this.GetPixelClearance(x + 1, curr_y, wall_height, match) < wall_height)
								{
									if (on_insufficient_wall_height != null)
									{
										on_insufficient_wall_height(x, curr_y);
									}
									else
									{
										this.SetPixelMap(x, curr_y, Color.White);
										this.PlaceSingleWall(x, curr_y);
									}
									current_height--;
								}
								if (current_height < wall_height)
								{
									current_height++;
								}
							}
							last_y = current_y;
							clearance = 0;
						}
						else
						{
							clearance++;
						}
					}
				}
			}
			List<Point> corner_tiles = new List<Point>();
			for (int y = 0; y < this.mapHeight; y++)
			{
				for (int x2 = 0; x2 < this.mapWidth; x2++)
				{
					int height = this.GetHeight(x2, y, wall_height);
					int left_height = this.GetHeight(x2 - 1, y, wall_height);
					int right_height = this.GetHeight(x2 + 1, y, wall_height);
					int top_height = this.GetHeight(x2, y - 1, wall_height);
					int index = this.generationRandom.Next(0, random_wall_variants);
					if (right_height < height)
					{
						if (right_height == wall_height)
						{
							if (use_corner_hack)
							{
								corner_tiles.Add(new Point(x2, y));
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
							}
						}
						else
						{
							Layer target_layer = this.buildingsLayer;
							if (right_height >= 0)
							{
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - right_height));
								target_layer = this.frontLayer;
							}
							if (height > wall_height)
							{
								this.SetTile(target_layer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 - 1, source_y + 1 + index));
							}
							else
							{
								this.SetTile(target_layer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
							}
							if (wall_height > 0 && y + 1 < this.mapHeight && right_height == -1 && this.GetHeight(x2 + 1, y + 1, wall_height) >= 0 && this.GetHeight(x2, y + 1, wall_height) >= 0)
							{
								if (use_corner_hack)
								{
									corner_tiles.Add(new Point(x2, y));
									this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
								}
								else
								{
									this.SetTile(this.frontLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
								}
							}
						}
					}
					else if (left_height < height)
					{
						if (left_height == wall_height)
						{
							if (use_corner_hack)
							{
								corner_tiles.Add(new Point(x2, y));
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
							}
						}
						else
						{
							Layer target_layer2 = this.buildingsLayer;
							if (left_height >= 0)
							{
								this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - left_height));
								target_layer2 = this.frontLayer;
							}
							if (height > wall_height)
							{
								this.SetTile(target_layer2, x2, y, VolcanoDungeon.GetTileIndex(source_x, source_y + 1 + index));
							}
							else
							{
								this.SetTile(target_layer2, x2, y, VolcanoDungeon.GetTileIndex(source_x + index, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
							}
							if (wall_height > 0 && y + 1 < this.mapHeight && left_height == -1 && this.GetHeight(x2 - 1, y + 1, wall_height) >= 0 && this.GetHeight(x2, y + 1, wall_height) >= 0)
							{
								if (use_corner_hack)
								{
									corner_tiles.Add(new Point(x2, y));
									this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
								}
								else
								{
									this.SetTile(this.frontLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
								}
							}
						}
					}
					if (height >= 0 && top_height == -1)
					{
						if (wall_height > 0)
						{
							if (right_height == -1)
							{
								this.SetTile(this.frontLayer, x2, y - 1, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y));
							}
							else if (left_height == -1)
							{
								this.SetTile(this.frontLayer, x2, y - 1, VolcanoDungeon.GetTileIndex(source_x + index, source_y));
							}
							else
							{
								this.SetTile(this.frontLayer, x2, y - 1, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + index, source_y));
							}
						}
						else if (right_height == -1)
						{
							this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y));
						}
						else if (left_height == -1)
						{
							this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + index, source_y));
						}
						else
						{
							this.SetTile(this.buildingsLayer, x2, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + index, source_y));
						}
					}
				}
			}
			if (use_corner_hack)
			{
				foreach (Point corner_tile in corner_tiles)
				{
					if (this.GetHeight(corner_tile.X - 1, corner_tile.Y, wall_height) == -1)
					{
						this.SetTile(this.frontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
					}
					else if (this.GetHeight(corner_tile.X + 1, corner_tile.Y, wall_height) == -1)
					{
						this.SetTile(this.frontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
					}
					if (this.GetHeight(corner_tile.X - 1, corner_tile.Y, wall_height) == wall_height)
					{
						this.SetTile(this.alwaysFrontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
					}
					else if (this.GetHeight(corner_tile.X + 1, corner_tile.Y, wall_height) == wall_height)
					{
						this.SetTile(this.alwaysFrontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
					}
				}
			}
			this.heightMap = null;
		}

		// Token: 0x060032AD RID: 12973 RVA: 0x00293CEC File Offset: 0x00291EEC
		public int GetPixelClearance(int x, int y, int wall_height, Color match)
		{
			int current_height = 0;
			if (this.GetPixel(x, y, Color.White) == match)
			{
				current_height++;
				int i = 1;
				while (i < wall_height && current_height < wall_height)
				{
					if (y + i >= this.mapHeight)
					{
						return wall_height;
					}
					if (!(this.GetPixel(x, y + i, Color.White) == match))
					{
						break;
					}
					current_height++;
					i++;
				}
				int j = 1;
				while (j < wall_height && current_height < wall_height)
				{
					if (y - j < 0)
					{
						return wall_height;
					}
					if (!(this.GetPixel(x, y - j, Color.White) == match))
					{
						break;
					}
					current_height++;
					j++;
				}
				return current_height;
			}
			return 0;
		}

		// Token: 0x060032AE RID: 12974 RVA: 0x00293D88 File Offset: 0x00291F88
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.coolLavaEvent.Poll();
			this.lavaSoundsPlayedThisTick = 0;
			if (this.level.Value == 0 && Game1.currentLocation == this)
			{
				this.steamTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.steamTimer < 0f)
				{
					this.steamTimer = 5000f;
					Game1.playSound("cavedrip", null);
					this.temporarySprites.Add(new TemporaryAnimatedSprite(null, new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(34.5f, 30.75f) * 64f, false, 0f, Color.White)
					{
						texture = Game1.staminaRect,
						color = new Color(100, 150, 255),
						alpha = 0.75f,
						motion = new Vector2(0f, 1f),
						acceleration = new Vector2(0f, 0.1f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 8f,
						id = 89898,
						yStopCoordinate = 2208,
						reachedStopCoordinate = delegate(int x)
						{
							base.removeTemporarySpritesWithID(89898);
							Game1.playSound("steam", null);
							for (int i = 0; i < 4; i++)
							{
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(33.75f, 33.5f) * 64f + new Vector2((float)Game1.random.Next(64), (float)Game1.random.Next(64)), false, 0.007f, Color.White)
								{
									alpha = 0.75f,
									motion = new Vector2(0f, -1f),
									acceleration = new Vector2(0.002f, 0f),
									interval = 99999f,
									layerDepth = 1f,
									scale = 4f,
									scaleChange = 0.02f,
									rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
								});
							}
						}
					});
				}
			}
			foreach (DwarfGate dwarfGate in this.dwarfGates)
			{
				dwarfGate.UpdateWhenCurrentLocation(time, this);
			}
			if (!this._sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(30f, 38f), 3, this) != null)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_Volcano", true, false);
				TemporaryAnimatedSprite v = base.getTemporarySpriteByID(999);
				if (v != null)
				{
					v.yPeriodic = false;
					v.xPeriodic = false;
					v.sourceRect.Y = 0;
					v.sourceRectStartingPos.Y = 0f;
					v.motion = new Vector2(0f, -4f);
					v.acceleration = new Vector2(0f, -0.04f);
				}
				base.localSound("magma_sprite_spot", null, null, SoundContext.Default);
				v = base.getTemporarySpriteByID(998);
				if (v != null)
				{
					v.yPeriodic = false;
					v.xPeriodic = false;
					v.motion = new Vector2(0f, -4f);
					v.acceleration = new Vector2(0f, -0.04f);
				}
				this._sawFlameSprite = true;
			}
		}

		// Token: 0x060032AF RID: 12975 RVA: 0x0029403C File Offset: 0x0029223C
		public virtual void PlaceGroundTile(int x, int y)
		{
			if (this.generationRandom.NextDouble() < 0.30000001192092896)
			{
				this.SetTile(this.backLayer, x, y, VolcanoDungeon.GetTileIndex(1 + this.generationRandom.Next(0, 3), this.generationRandom.Next(0, 2)));
				return;
			}
			this.SetTile(this.backLayer, x, y, VolcanoDungeon.GetTileIndex(1, 0));
		}

		// Token: 0x060032B0 RID: 12976 RVA: 0x002940A4 File Offset: 0x002922A4
		public override void drawFloorDecorations(SpriteBatch b)
		{
			base.drawFloorDecorations(b);
			for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 1; y++)
			{
				for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
				{
					Vector2 tile = new Vector2((float)x, (float)y);
					Point point;
					if (this.localCooledLavaTiles.TryGetValue(tile, out point))
					{
						point.X += 5;
						point.Y += 16;
						b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(point.X * 16, point.Y * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.55f);
					}
				}
			}
		}

		// Token: 0x060032B1 RID: 12977 RVA: 0x002941C0 File Offset: 0x002923C0
		public override void drawWaterTile(SpriteBatch b, int x, int y)
		{
			if (this.level.Value == 5)
			{
				base.drawWaterTile(b, x, y);
				return;
			}
			if (this.level.Value == 0 && x > 23 && x < 28 && y > 42 && y < 47)
			{
				base.drawWaterTile(b, x, y, Color.DeepSkyBlue * 0.8f);
				return;
			}
			bool flag = y == this.map.Layers[0].LayerHeight - 1 || !this.waterTiles[x, y + 1];
			bool topY = y == 0 || !this.waterTiles[x, y - 1];
			int water_tile_upper_left_x = 0;
			int water_tile_upper_left_y = 320;
			b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - (int)((!topY) ? this.waterPosition : 0f)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + this.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + y) % 2 == 0) ? (this.waterTileFlip ? 32 : 0) : (this.waterTileFlip ? 0 : 32)) + (topY ? ((int)this.waterPosition / 4) : 0), 16, 16 + (topY ? ((int)(-(int)this.waterPosition) / 4) : 0))), this.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			if (flag)
			{
				b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y + 1) * 64 - (int)this.waterPosition))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + this.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + (y + 1)) % 2 == 0) ? (this.waterTileFlip ? 32 : 0) : (this.waterTileFlip ? 0 : 32)), 16, 16 - (int)(16f - this.waterPosition / 4f) - 1)), this.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			}
		}

		// Token: 0x060032B2 RID: 12978 RVA: 0x002943D0 File Offset: 0x002925D0
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (DwarfGate dwarfGate in this.dwarfGates)
			{
				dwarfGate.Draw(b);
			}
		}

		// Token: 0x060032B3 RID: 12979 RVA: 0x00294428 File Offset: 0x00292628
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (!Game1.game1.takingMapScreenshot && this.level.Value > 0)
			{
				Color col = SpriteText.color_Red;
				string txt = this.level.Value.ToString() ?? "";
				Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
				SpriteText.drawString(b, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, 999999, 1f, 1f, false, 2, "", new Color?(col), SpriteText.ScrollTextAlignment.Left);
			}
		}

		// Token: 0x060032B4 RID: 12980 RVA: 0x002944D0 File Offset: 0x002926D0
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (Game1.random.NextDouble() < 0.1 && this.level.Value > 0 && this.level.Value != 5)
			{
				int numsprites = 0;
				using (List<NPC>.Enumerator enumerator = this.characters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is Bat)
						{
							numsprites++;
						}
					}
				}
				if (numsprites < this.farmers.Count * 4)
				{
					this.spawnFlyingMonsterOffScreen();
				}
			}
		}

		// Token: 0x060032B5 RID: 12981 RVA: 0x00294578 File Offset: 0x00292778
		public void spawnFlyingMonsterOffScreen()
		{
			Vector2 spawnLocation = Vector2.Zero;
			switch (Game1.random.Next(4))
			{
			case 0:
				spawnLocation.X = (float)Game1.random.Next(this.map.Layers[0].LayerWidth);
				break;
			case 1:
				spawnLocation.X = (float)(this.map.Layers[0].LayerWidth - 1);
				spawnLocation.Y = (float)Game1.random.Next(this.map.Layers[0].LayerHeight);
				break;
			case 2:
				spawnLocation.Y = (float)(this.map.Layers[0].LayerHeight - 1);
				spawnLocation.X = (float)Game1.random.Next(this.map.Layers[0].LayerWidth);
				break;
			case 3:
				spawnLocation.Y = (float)Game1.random.Next(this.map.Layers[0].LayerHeight);
				break;
			}
			base.playSound("magma_sprite_spot", null, null, SoundContext.Default);
			this.characters.Add(new Bat(spawnLocation, (this.level.Value > 5 && Game1.random.NextBool()) ? -556 : -555)
			{
				focusedOnFarmers = true
			});
		}

		// Token: 0x060032B6 RID: 12982 RVA: 0x002946FC File Offset: 0x002928FC
		public virtual void PlaceSingleWall(int x, int y)
		{
			int index = this.generationRandom.Next(0, 4);
			this.SetTile(this.frontLayer, x, y - 1, VolcanoDungeon.GetTileIndex(index, 2));
			this.SetTile(this.buildingsLayer, x, y, VolcanoDungeon.GetTileIndex(index, 3));
		}

		// Token: 0x060032B7 RID: 12983 RVA: 0x00294744 File Offset: 0x00292944
		public virtual void ApplyPixels(string layout_texture_name, int source_x = 0, int source_y = 0, int width = 64, int height = 64, int x_offset = 0, int y_offset = 0, bool flip_x = false)
		{
			Texture2D texture2D = Game1.temporaryContent.Load<Texture2D>(layout_texture_name);
			Color[] pixels = new Color[width * height];
			texture2D.GetData<Color>(0, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(source_x, source_y, width, height)), pixels, 0, width * height);
			for (int base_x = 0; base_x < width; base_x++)
			{
				int x = base_x + x_offset;
				if (flip_x)
				{
					x = x_offset + width - 1 - base_x;
				}
				if (x >= 0 && x < this.mapWidth)
				{
					for (int base_y = 0; base_y < height; base_y++)
					{
						int y = base_y + y_offset;
						if (y >= 0 && y < this.mapHeight)
						{
							Color pixel_color = this.GetPixelColor(width, height, pixels, base_x, base_y);
							this.SetPixelMap(x, y, pixel_color);
						}
					}
				}
			}
		}

		// Token: 0x060032B8 RID: 12984 RVA: 0x002947ED File Offset: 0x002929ED
		public int GetHeight(int x, int y, int max_height)
		{
			if (x < 0 || x >= this.mapWidth || y < 0 || y >= this.mapHeight)
			{
				return max_height + 1;
			}
			return this.heightMap[x + y * this.mapWidth];
		}

		// Token: 0x060032B9 RID: 12985 RVA: 0x0029481E File Offset: 0x00292A1E
		public Color GetPixel(int x, int y, Color out_of_bounds_color)
		{
			if (x < 0 || x >= this.mapWidth || y < 0 || y >= this.mapHeight)
			{
				return out_of_bounds_color;
			}
			return this.pixelMap[x + y * this.mapWidth];
		}

		// Token: 0x060032BA RID: 12986 RVA: 0x00294851 File Offset: 0x00292A51
		public void SetPixelMap(int x, int y, Color color)
		{
			if (x < 0 || x >= this.mapWidth)
			{
				return;
			}
			if (y < 0 || y >= this.mapHeight)
			{
				return;
			}
			this.pixelMap[x + y * this.mapWidth] = color;
		}

		// Token: 0x060032BB RID: 12987 RVA: 0x00294888 File Offset: 0x00292A88
		public int GetNeighborValue(int x, int y, Color matched_color, bool is_lava_pool = false)
		{
			int neighbor_value = 0;
			if (this.GetPixel(x, y - 1, matched_color) == matched_color)
			{
				neighbor_value++;
			}
			if (this.GetPixel(x, y + 1, matched_color) == matched_color)
			{
				neighbor_value += 2;
			}
			if (this.GetPixel(x + 1, y, matched_color) == matched_color)
			{
				neighbor_value += 4;
			}
			if (this.GetPixel(x - 1, y, matched_color) == matched_color)
			{
				neighbor_value += 8;
			}
			if (is_lava_pool && neighbor_value == 15)
			{
				if (this.GetPixel(x - 1, y - 1, matched_color) == matched_color)
				{
					neighbor_value += 16;
				}
				if (this.GetPixel(x + 1, y - 1, matched_color) == matched_color)
				{
					neighbor_value += 32;
				}
			}
			return neighbor_value;
		}

		// Token: 0x060032BC RID: 12988 RVA: 0x00294934 File Offset: 0x00292B34
		public Color GetPixelColor(int width, int height, Color[] pixels, int x, int y)
		{
			if (x < 0 || x >= width)
			{
				return Color.Black;
			}
			if (y < 0 || y >= height)
			{
				return Color.Black;
			}
			int index = x + y * width;
			return pixels[index];
		}

		// Token: 0x060032BD RID: 12989 RVA: 0x00294970 File Offset: 0x00292B70
		public static int GetTileIndex(int x, int y)
		{
			return x + y * 16;
		}

		// Token: 0x060032BE RID: 12990 RVA: 0x00294978 File Offset: 0x00292B78
		public void SetTile(Layer layer, int x, int y, int index)
		{
			if (x < 0 || x >= layer.LayerWidth)
			{
				return;
			}
			if (y < 0 || y >= layer.LayerHeight)
			{
				return;
			}
			Location location = new Location(x, y);
			TileSheet mainTileSheet = this.map.RequireTileSheet(0, "dungeon");
			layer.Tiles[location] = new StaticTile(layer, mainTileSheet, BlendMode.Alpha, index);
		}

		// Token: 0x060032BF RID: 12991 RVA: 0x002949D2 File Offset: 0x00292BD2
		public int GetMaxRoomLayouts()
		{
			return 30;
		}

		// Token: 0x060032C0 RID: 12992 RVA: 0x002949D8 File Offset: 0x00292BD8
		public static VolcanoDungeon GetLevel(string name, bool use_level_level_as_layout = false)
		{
			foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
			{
				if (level.Name.Equals(name))
				{
					return level;
				}
			}
			int newLevelNumber;
			if (!VolcanoDungeon.IsGeneratedLevel(name, out newLevelNumber))
			{
				Game1.log.Warn("Failed parsing Volcano Dungeon level from location name '" + name + "', defaulting to level 0.");
				newLevelNumber = 0;
			}
			VolcanoDungeon new_level = new VolcanoDungeon(newLevelNumber);
			VolcanoDungeon.activeLevels.Add(new_level);
			if (Game1.IsMasterGame)
			{
				new_level.GenerateContents(use_level_level_as_layout);
			}
			else
			{
				new_level.reloadMap();
			}
			return new_level;
		}

		// Token: 0x060032C1 RID: 12993 RVA: 0x00294A88 File Offset: 0x00292C88
		public static string GetLevelName(int level)
		{
			return "VolcanoDungeon" + level.ToString();
		}

		// Token: 0x060032C2 RID: 12994 RVA: 0x00294A9C File Offset: 0x00292C9C
		public static bool IsGeneratedLevel(string locationName)
		{
			int num;
			return VolcanoDungeon.IsGeneratedLevel(locationName, out num);
		}

		// Token: 0x060032C3 RID: 12995 RVA: 0x00294AB1 File Offset: 0x00292CB1
		public static bool IsGeneratedLevel(string locationName, out int level)
		{
			if (locationName == null || !locationName.StartsWithIgnoreCase("VolcanoDungeon"))
			{
				level = 0;
				return false;
			}
			return int.TryParse(locationName.Substring("VolcanoDungeon".Length), out level);
		}

		// Token: 0x060032C4 RID: 12996 RVA: 0x00294AE0 File Offset: 0x00292CE0
		public static void UpdateLevels(GameTime time)
		{
			foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
			{
				if (level.farmers.Count > 0)
				{
					level.UpdateWhenCurrentLocation(time);
				}
				level.updateEvenIfFarmerIsntHere(time, false);
			}
		}

		// Token: 0x060032C5 RID: 12997 RVA: 0x00294B48 File Offset: 0x00292D48
		public static void UpdateLevels10Minutes(int timeOfDay)
		{
			if (Game1.IsClient)
			{
				return;
			}
			foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
			{
				if (level.farmers.Count > 0)
				{
					level.performTenMinuteUpdate(timeOfDay);
				}
			}
		}

		// Token: 0x060032C6 RID: 12998 RVA: 0x00294BB0 File Offset: 0x00292DB0
		public static void ClearAllLevels()
		{
			VolcanoDungeon.activeLevels.RemoveAll(delegate(VolcanoDungeon level)
			{
				level.OnRemoved();
				return true;
			});
		}

		// Token: 0x060032C7 RID: 12999 RVA: 0x00294BDC File Offset: 0x00292DDC
		public override void OnRemoved()
		{
			base.OnRemoved();
			if (Game1.IsMasterGame)
			{
				this.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.collect(Game1.player, null));
			}
			this.mapContent.Dispose();
		}

		// Token: 0x060032C8 RID: 13000 RVA: 0x00294C2C File Offset: 0x00292E2C
		public static void ForEach(Action<VolcanoDungeon> action)
		{
			foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
			{
				action(level);
			}
		}

		// Token: 0x060032C9 RID: 13001 RVA: 0x00294C80 File Offset: 0x00292E80
		public override bool ShouldExcludeFromNpcPathfinding()
		{
			return true;
		}

		// Token: 0x060032CA RID: 13002 RVA: 0x00294C84 File Offset: 0x00292E84
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "dungeon");
			if (tileIndexAt == 77)
			{
				if (Game1.player.canUnderstandDwarves)
				{
					Utility.TryOpenShopMenu("VolcanoShop", null, true);
				}
				else
				{
					Game1.player.doEmote(8);
				}
				return true;
			}
			if (tileIndexAt == 367)
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Volcano_ShortcutOut"), base.createYesNoResponses(), "LeaveVolcano");
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060032CB RID: 13003 RVA: 0x00294D10 File Offset: 0x00292F10
		public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
		{
			if (this.IgnoreTouchActions())
			{
				return;
			}
			if (ArgUtility.Get(action, 0, null, true) == "DwarfSwitch")
			{
				Point tile_point = new Point((int)playerStandingPosition.X, (int)playerStandingPosition.Y);
				using (NetList<DwarfGate, NetRef<DwarfGate>>.Enumerator enumerator = this.dwarfGates.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DwarfGate gate = enumerator.Current;
						bool wasPressed;
						if (gate.switches.TryGetValue(tile_point, out wasPressed) && !wasPressed)
						{
							gate.pressEvent.Fire(tile_point);
						}
					}
					return;
				}
			}
			base.performTouchAction(action, playerStandingPosition);
		}

		// Token: 0x040021BB RID: 8635
		private const int coalIndexPlaceholder = 1095382;

		// Token: 0x040021BC RID: 8636
		private const string coalIndexPlaceholderString = "1095382";

		// Token: 0x040021BD RID: 8637
		public const string MainTileSheetId = "dungeon";

		// Token: 0x040021BE RID: 8638
		public NetInt level = new NetInt();

		// Token: 0x040021BF RID: 8639
		public NetEvent1Field<Point, NetPoint> coolLavaEvent = new NetEvent1Field<Point, NetPoint>();

		// Token: 0x040021C0 RID: 8640
		public static List<VolcanoDungeon> activeLevels = new List<VolcanoDungeon>();

		// Token: 0x040021C1 RID: 8641
		public NetVector2Dictionary<bool, NetBool> cooledLavaTiles = new NetVector2Dictionary<bool, NetBool>();

		// Token: 0x040021C2 RID: 8642
		public Dictionary<Vector2, Point> localCooledLavaTiles = new Dictionary<Vector2, Point>();

		// Token: 0x040021C3 RID: 8643
		public HashSet<Point> dirtTiles = new HashSet<Point>();

		// Token: 0x040021C4 RID: 8644
		public NetInt generationSeed = new NetInt();

		// Token: 0x040021C5 RID: 8645
		public NetInt layoutIndex = new NetInt();

		// Token: 0x040021C6 RID: 8646
		public Random generationRandom;

		// Token: 0x040021C7 RID: 8647
		private LocalizedContentManager mapContent;

		// Token: 0x040021C8 RID: 8648
		[XmlIgnore]
		public int mapWidth;

		// Token: 0x040021C9 RID: 8649
		[XmlIgnore]
		public int mapHeight;

		// Token: 0x040021CA RID: 8650
		public const int WALL_HEIGHT = 4;

		// Token: 0x040021CB RID: 8651
		public Layer backLayer;

		// Token: 0x040021CC RID: 8652
		public Layer buildingsLayer;

		// Token: 0x040021CD RID: 8653
		public Layer frontLayer;

		// Token: 0x040021CE RID: 8654
		public Layer alwaysFrontLayer;

		// Token: 0x040021CF RID: 8655
		[XmlIgnore]
		public Point? startPosition;

		// Token: 0x040021D0 RID: 8656
		[XmlIgnore]
		public Point? endPosition;

		// Token: 0x040021D1 RID: 8657
		public const int LAYOUT_WIDTH = 64;

		// Token: 0x040021D2 RID: 8658
		public const int LAYOUT_HEIGHT = 64;

		// Token: 0x040021D3 RID: 8659
		[XmlIgnore]
		public Texture2D mapBaseTilesheet;

		// Token: 0x040021D4 RID: 8660
		public static List<Microsoft.Xna.Framework.Rectangle> setPieceAreas = new List<Microsoft.Xna.Framework.Rectangle>();

		// Token: 0x040021D5 RID: 8661
		protected static Dictionary<int, Point> _blobIndexLookup = null;

		// Token: 0x040021D6 RID: 8662
		protected static Dictionary<int, Point> _lavaBlobIndexLookup = null;

		// Token: 0x040021D7 RID: 8663
		protected bool generated;

		// Token: 0x040021D8 RID: 8664
		protected static Point shortcutOutPosition = new Point(29, 34);

		// Token: 0x040021D9 RID: 8665
		[XmlIgnore]
		protected NetBool shortcutOutUnlocked = new NetBool(false)
		{
			InterpolationWait = false
		};

		// Token: 0x040021DA RID: 8666
		[XmlIgnore]
		protected NetBool bridgeUnlocked = new NetBool(false)
		{
			InterpolationWait = false
		};

		// Token: 0x040021DB RID: 8667
		public Color[] pixelMap;

		// Token: 0x040021DC RID: 8668
		public int[] heightMap;

		// Token: 0x040021DD RID: 8669
		public Dictionary<int, List<Point>> possibleSwitchPositions = new Dictionary<int, List<Point>>();

		// Token: 0x040021DE RID: 8670
		public Dictionary<int, List<Point>> possibleGatePositions = new Dictionary<int, List<Point>>();

		// Token: 0x040021DF RID: 8671
		public NetList<DwarfGate, NetRef<DwarfGate>> dwarfGates = new NetList<DwarfGate, NetRef<DwarfGate>>();

		// Token: 0x040021E0 RID: 8672
		[XmlIgnore]
		protected bool _sawFlameSprite;

		// Token: 0x040021E1 RID: 8673
		private int lavaSoundsPlayedThisTick;

		// Token: 0x040021E2 RID: 8674
		private float steamTimer = 6000f;

		// Token: 0x0200066F RID: 1647
		public enum TileNeighbors
		{
			// Token: 0x04002FA6 RID: 12198
			N = 1,
			// Token: 0x04002FA7 RID: 12199
			S,
			// Token: 0x04002FA8 RID: 12200
			E = 4,
			// Token: 0x04002FA9 RID: 12201
			W = 8,
			// Token: 0x04002FAA RID: 12202
			NW = 16,
			// Token: 0x04002FAB RID: 12203
			NE = 32
		}
	}
}
