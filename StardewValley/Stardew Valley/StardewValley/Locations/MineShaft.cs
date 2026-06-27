using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Pathfinding;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002E8 RID: 744
	public class MineShaft : GameLocation
	{
		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06003132 RID: 12594 RVA: 0x0026F2B0 File Offset: 0x0026D4B0
		// (set) Token: 0x06003133 RID: 12595 RVA: 0x0026F328 File Offset: 0x0026D528
		public static int lowestLevelReached
		{
			get
			{
				if (Game1.netWorldState.Value.LowestMineLevelForOrder < 0)
				{
					return Game1.netWorldState.Value.LowestMineLevel;
				}
				if (Game1.netWorldState.Value.LowestMineLevelForOrder == 120)
				{
					return Math.Max(Game1.netWorldState.Value.LowestMineLevelForOrder, Game1.netWorldState.Value.LowestMineLevelForOrder);
				}
				return Game1.netWorldState.Value.LowestMineLevelForOrder;
			}
			set
			{
				if (Game1.netWorldState.Value.LowestMineLevelForOrder >= 0 && value <= 120)
				{
					Game1.netWorldState.Value.LowestMineLevelForOrder = value;
					return;
				}
				if (!Game1.player.hasSkullKey && value > 120)
				{
					return;
				}
				Game1.netWorldState.Value.LowestMineLevel = value;
			}
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06003134 RID: 12596 RVA: 0x0026F37F File Offset: 0x0026D57F
		// (set) Token: 0x06003135 RID: 12597 RVA: 0x0026F38C File Offset: 0x0026D58C
		public int mineLevel
		{
			get
			{
				return this.netMineLevel.Value;
			}
			set
			{
				this.netMineLevel.Value = value;
			}
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06003136 RID: 12598 RVA: 0x0026F39A File Offset: 0x0026D59A
		// (set) Token: 0x06003137 RID: 12599 RVA: 0x0026F3A7 File Offset: 0x0026D5A7
		public int stonesLeftOnThisLevel
		{
			get
			{
				return this.netStonesLeftOnThisLevel.Value;
			}
			set
			{
				this.netStonesLeftOnThisLevel.Value = value;
			}
		}

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06003138 RID: 12600 RVA: 0x0026F3B5 File Offset: 0x0026D5B5
		// (set) Token: 0x06003139 RID: 12601 RVA: 0x0026F3C2 File Offset: 0x0026D5C2
		public Vector2 tileBeneathLadder
		{
			get
			{
				return this.netTileBeneathLadder.Value;
			}
			set
			{
				this.netTileBeneathLadder.Value = value;
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x0600313A RID: 12602 RVA: 0x0026F3D0 File Offset: 0x0026D5D0
		// (set) Token: 0x0600313B RID: 12603 RVA: 0x0026F3DD File Offset: 0x0026D5DD
		public Vector2 tileBeneathElevator
		{
			get
			{
				return this.netTileBeneathElevator.Value;
			}
			set
			{
				this.netTileBeneathElevator.Value = value;
			}
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x0600313C RID: 12604 RVA: 0x0026F3EB File Offset: 0x0026D5EB
		// (set) Token: 0x0600313D RID: 12605 RVA: 0x0026F3F8 File Offset: 0x0026D5F8
		public Point ElevatorLightSpot
		{
			get
			{
				return this.netElevatorLightSpot.Value;
			}
			set
			{
				this.netElevatorLightSpot.Value = value;
			}
		}

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x0600313E RID: 12606 RVA: 0x0026F406 File Offset: 0x0026D606
		// (set) Token: 0x0600313F RID: 12607 RVA: 0x0026F413 File Offset: 0x0026D613
		public bool isSlimeArea
		{
			get
			{
				return this.netIsSlimeArea.Value;
			}
			set
			{
				this.netIsSlimeArea.Value = value;
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06003140 RID: 12608 RVA: 0x0026F421 File Offset: 0x0026D621
		// (set) Token: 0x06003141 RID: 12609 RVA: 0x0026F42E File Offset: 0x0026D62E
		public bool isDinoArea
		{
			get
			{
				return this.netIsDinoArea.Value;
			}
			set
			{
				this.netIsDinoArea.Value = value;
			}
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06003142 RID: 12610 RVA: 0x0026F43C File Offset: 0x0026D63C
		// (set) Token: 0x06003143 RID: 12611 RVA: 0x0026F449 File Offset: 0x0026D649
		public bool isMonsterArea
		{
			get
			{
				return this.netIsMonsterArea.Value;
			}
			set
			{
				this.netIsMonsterArea.Value = value;
			}
		}

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06003144 RID: 12612 RVA: 0x0026F457 File Offset: 0x0026D657
		// (set) Token: 0x06003145 RID: 12613 RVA: 0x0026F464 File Offset: 0x0026D664
		public bool isQuarryArea
		{
			get
			{
				return this.netIsQuarryArea.Value;
			}
			set
			{
				this.netIsQuarryArea.Value = value;
			}
		}

		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06003146 RID: 12614 RVA: 0x0026F472 File Offset: 0x0026D672
		// (set) Token: 0x06003147 RID: 12615 RVA: 0x0026F47F File Offset: 0x0026D67F
		public bool ambientFog
		{
			get
			{
				return this.netAmbientFog.Value;
			}
			set
			{
				this.netAmbientFog.Value = value;
			}
		}

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06003148 RID: 12616 RVA: 0x0026F48D File Offset: 0x0026D68D
		// (set) Token: 0x06003149 RID: 12617 RVA: 0x0026F49A File Offset: 0x0026D69A
		public Color lighting
		{
			get
			{
				return this.netLighting.Value;
			}
			set
			{
				this.netLighting.Value = value;
			}
		}

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x0600314A RID: 12618 RVA: 0x0026F4A8 File Offset: 0x0026D6A8
		// (set) Token: 0x0600314B RID: 12619 RVA: 0x0026F4B5 File Offset: 0x0026D6B5
		public Color fogColor
		{
			get
			{
				return this.netFogColor.Value;
			}
			set
			{
				this.netFogColor.Value = value;
			}
		}

		// Token: 0x0600314C RID: 12620 RVA: 0x0026F4C4 File Offset: 0x0026D6C4
		public MineShaft() : this(0, null)
		{
		}

		// Token: 0x0600314D RID: 12621 RVA: 0x0026F4E4 File Offset: 0x0026D6E4
		public MineShaft(int level, int? forceLayout = null)
		{
			this.mineLevel = level;
			this.name.Value = MineShaft.GetLevelName(level, null);
			this.mapContent = Game1.game1.xTileContent.CreateTemporary();
			this.forceLayout = forceLayout;
			if (!Game1.IsMultiplayer && this.getMineArea(-1) == 121)
			{
				base.ExtraMillisecondsPerInGameMinute = 200;
			}
		}

		// Token: 0x0600314E RID: 12622 RVA: 0x0026F685 File Offset: 0x0026D885
		public override string GetLocationContextId()
		{
			if (this.locationContextId == null)
			{
				this.locationContextId = ((this.mineLevel >= 121) ? "Desert" : "Default");
			}
			return base.GetLocationContextId();
		}

		// Token: 0x0600314F RID: 12623 RVA: 0x0026F6B1 File Offset: 0x0026D8B1
		public override bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			return false;
		}

		// Token: 0x06003150 RID: 12624 RVA: 0x0026F6B4 File Offset: 0x0026D8B4
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.netMineLevel, "netMineLevel").AddField(this.netStonesLeftOnThisLevel, "netStonesLeftOnThisLevel").AddField(this.netTileBeneathLadder, "netTileBeneathLadder").AddField(this.netTileBeneathElevator, "netTileBeneathElevator").AddField(this.netElevatorLightSpot, "netElevatorLightSpot").AddField(this.netIsSlimeArea, "netIsSlimeArea").AddField(this.netIsMonsterArea, "netIsMonsterArea").AddField(this.netIsTreasureRoom, "netIsTreasureRoom").AddField(this.netIsDinoArea, "netIsDinoArea").AddField(this.netIsQuarryArea, "netIsQuarryArea").AddField(this.netAmbientFog, "netAmbientFog").AddField(this.netLighting, "netLighting").AddField(this.netFogColor, "netFogColor").AddField(this.createLadderAtEvent, "createLadderAtEvent").AddField(this.createLadderDownEvent, "createLadderDownEvent").AddField(this.mapImageSource, "mapImageSource").AddField(this.rainbowLights, "rainbowLights").AddField(this.isLightingDark, "isLightingDark").AddField(this.elevatorShouldDing, "elevatorShouldDing").AddField(this.isFogUp, "isFogUp").AddField(this.calicoStatueSpot, "calicoStatueSpot").AddField(this.recentlyActivatedCalicoStatue, "recentlyActivatedCalicoStatue");
			this.isFogUp.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				if (!oldValue && newValue)
				{
					if (Game1.currentLocation == this)
					{
						Game1.changeMusicTrack("none", false, MusicContext.Default);
					}
					if (Game1.IsClient)
					{
						this.fogTime = 35000;
						return;
					}
				}
				else if (!newValue)
				{
					this.fogTime = 0;
				}
			};
			this.createLadderAtEvent.OnValueAdded += delegate(Vector2 v, bool b)
			{
				this.doCreateLadderAt(v);
			};
			this.createLadderDownEvent.OnValueAdded += this.doCreateLadderDown;
			this.mapImageSource.fieldChangeEvent += delegate(NetString field, string oldValue, string newValue)
			{
				if (newValue != null && newValue != oldValue)
				{
					base.Map.RequireTileSheet(0, "mine").ImageSource = newValue;
					base.Map.LoadTileSheets(Game1.mapDisplayDevice);
				}
			};
			this.recentlyActivatedCalicoStatue.fieldChangeEvent += this.calicoStatueActivated;
		}

		// Token: 0x06003151 RID: 12625 RVA: 0x0026F8A4 File Offset: 0x0026DAA4
		public void calicoStatueActivated(NetPoint field, Point oldVector, Point newVector)
		{
			if (newVector == Point.Zero)
			{
				return;
			}
			if (Game1.currentLocation != null && Game1.currentLocation.Equals(this))
			{
				Game1.playSound("openBox", null);
				this.temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle((newVector.X - 1) * 64, (newVector.Y - 3) * 64, 192, 192), 20, Color.White, 50, 500, ""));
				this.calicoEggIconTimerShake = 1500f;
				base.setMapTile(newVector.X, newVector.Y, 285, "Buildings", "mine", null, true);
				base.setMapTile(newVector.X, newVector.Y - 1, 269, "Front", "mine", null, true);
				base.setMapTile(newVector.X, newVector.Y - 2, 253, "Front", "mine", null, true);
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(20, 0, 19, 21), new Vector2((float)(newVector.X * 64 - 4), ((float)newVector.Y - 2.5f) * 64f), false, 0f, Color.White)
				{
					motion = new Vector2(0f, -1f),
					yStopCoordinate = (int)(((float)newVector.Y - 3.25f) * 64f),
					scale = 4f,
					animationLength = 1,
					delayBeforeAnimationStart = 1500,
					totalNumberOfLoops = 10,
					interval = 300f,
					drawAboveAlwaysFront = true
				});
			}
			if (Game1.IsMasterGame)
			{
				NetInt calicoEggSkullCavernRating = Game1.player.team.calicoEggSkullCavernRating;
				int value = calicoEggSkullCavernRating.Value;
				calicoEggSkullCavernRating.Value = value + 1;
				MineShaft.totalCalicoStatuesActivatedToday++;
				Random r = Utility.CreateDaySaveRandom((double)MineShaft.totalCalicoStatuesActivatedToday, 0.0, 0.0);
				if (r.NextBool(0.51 + Game1.player.team.AverageDailyLuck(this)))
				{
					if (this.tryToAddCalicoStatueEffect(r, 0.15, 10, false))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.01, 17, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.05, 12, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 15, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.2, 16, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 14, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.5, 11, true))
					{
						return;
					}
					Game1.player.team.AddCalicoStatueEffect(13);
					this.signalCalicoStatueActivation(13);
					return;
				}
				else
				{
					if (r.NextBool(0.2))
					{
						for (int tries = 0; tries < 30; tries++)
						{
							int which = r.Next(4);
							if (!Game1.player.team.calicoStatueEffects.ContainsKey(which))
							{
								Game1.player.team.AddCalicoStatueEffect(which);
								this.signalCalicoStatueActivation(which);
								return;
							}
						}
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 4, false))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 9, false))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 5, false))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.1, 6, false))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.2, 7, true))
					{
						return;
					}
					if (this.tryToAddCalicoStatueEffect(r, 0.2, 8, true))
					{
						return;
					}
					Game1.player.team.AddCalicoStatueEffect(13);
					this.signalCalicoStatueActivation(13);
				}
			}
		}

		// Token: 0x06003152 RID: 12626 RVA: 0x0026FC82 File Offset: 0x0026DE82
		private void signalCalicoStatueActivation(int whichEffect)
		{
			this.recentCalicoStatueEffect = whichEffect;
			if (Game1.IsMultiplayer)
			{
				Game1.multiplayer.globalChatInfoMessage("CalicoStatue_Activated", new string[]
				{
					TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Description_" + whichEffect.ToString())
				});
			}
		}

		// Token: 0x06003153 RID: 12627 RVA: 0x0026FCC0 File Offset: 0x0026DEC0
		private bool tryToAddCalicoStatueEffect(Random r, double chance, int which, bool effectCanStack = false)
		{
			if (r.NextBool(chance) && (effectCanStack || !Game1.player.team.calicoStatueEffects.ContainsKey(which)))
			{
				Game1.player.team.AddCalicoStatueEffect(which);
				this.signalCalicoStatueActivation(which);
				return true;
			}
			return false;
		}

		// Token: 0x06003154 RID: 12628 RVA: 0x0026FD00 File Offset: 0x0026DF00
		public override bool AllowMapModificationsInResetState()
		{
			return true;
		}

		// Token: 0x06003155 RID: 12629 RVA: 0x0026FD03 File Offset: 0x0026DF03
		protected override LocalizedContentManager getMapLoader()
		{
			return this.mapContent;
		}

		// Token: 0x06003156 RID: 12630 RVA: 0x0026FD0C File Offset: 0x0026DF0C
		private void setElevatorLit()
		{
			if (this.ElevatorLightSpot.X == -1 || this.ElevatorLightSpot.Y == -1)
			{
				return;
			}
			base.setMapTile(this.ElevatorLightSpot.X, this.ElevatorLightSpot.Y, 48, "Buildings", "mine", null, true);
			IDictionary<string, LightSource> currentLightSources = Game1.currentLightSources;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Mine_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
			defaultInterpolatedStringHandler.AppendLiteral("_Elevator");
			currentLightSources.Add(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)this.ElevatorLightSpot.X, (float)this.ElevatorLightSpot.Y) * 64f, 2f, Color.Black, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			this.elevatorShouldDing.Value = false;
		}

		// Token: 0x06003157 RID: 12631 RVA: 0x0026FDF0 File Offset: 0x0026DFF0
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			bool flag = Game1.currentLocation == this;
			if ((Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) || Game1.getMusicTrackName(MusicContext.Default).Contains("Ambient")) && Game1.random.NextDouble() < 0.00195)
			{
				string audioName = "cavedrip";
				Vector2? position = null;
				int? pitch = null;
				base.localSound(audioName, position, pitch, SoundContext.Default);
			}
			if (this.timeUntilElevatorLightUp > 0)
			{
				this.timeUntilElevatorLightUp -= time.ElapsedGameTime.Milliseconds;
				if (this.timeUntilElevatorLightUp <= 0)
				{
					string audioName2 = "crystal";
					int? pitch = new int?(0);
					base.localSound(audioName2, null, pitch, SoundContext.Default);
					this.setElevatorLit();
				}
			}
			if (this.calicoEggIconTimerShake > 0f)
			{
				this.calicoEggIconTimerShake -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (flag)
			{
				if (this.isFogUp.Value && Game1.shouldTimePass(false))
				{
					if (MineShaft.bugLevelLoop == null || MineShaft.bugLevelLoop.IsStopped)
					{
						Game1.playSound("bugLevelLoop", out MineShaft.bugLevelLoop);
					}
					if (this.fogAlpha < 1f)
					{
						if (Game1.shouldTimePass(false))
						{
							this.fogAlpha += 0.01f;
						}
						if (MineShaft.bugLevelLoop != null)
						{
							MineShaft.bugLevelLoop.SetVariable("Volume", this.fogAlpha * 100f);
							MineShaft.bugLevelLoop.SetVariable("Frequency", this.fogAlpha * 25f);
						}
					}
					else if (MineShaft.bugLevelLoop != null)
					{
						float f = (float)Math.Max(0.0, Math.Min(100.0, Math.Sin((double)((float)this.fogTime / 10000f) % 628.3185307179587)));
						MineShaft.bugLevelLoop.SetVariable("Frequency", Math.Max(0f, Math.Min(100f, this.fogAlpha * 25f + f * 10f)));
					}
				}
				else if (this.fogAlpha > 0f)
				{
					if (Game1.shouldTimePass(false))
					{
						this.fogAlpha -= 0.01f;
					}
					if (MineShaft.bugLevelLoop != null)
					{
						MineShaft.bugLevelLoop.SetVariable("Volume", this.fogAlpha * 100f);
						MineShaft.bugLevelLoop.SetVariable("Frequency", Math.Max(0f, MineShaft.bugLevelLoop.GetVariable("Frequency") - 0.01f));
						if (this.fogAlpha <= 0f)
						{
							MineShaft.bugLevelLoop.Stop(AudioStopOptions.Immediate);
							MineShaft.bugLevelLoop = null;
						}
					}
				}
				if (this.fogAlpha > 0f || this.ambientFog)
				{
					Vector2 currentViewport = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
					this.fogPos = Game1.updateFloatingObjectPositionForMovement(this.fogPos, currentViewport, Game1.previousViewportPosition, -1f);
					this.fogPos.X = (this.fogPos.X + 0.5f) % 256f;
					this.fogPos.Y = (this.fogPos.Y + 0.5f) % 256f;
				}
			}
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x06003158 RID: 12632 RVA: 0x00270130 File Offset: 0x0026E330
		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (MineShaft.bugLevelLoop != null)
			{
				MineShaft.bugLevelLoop.Stop(AudioStopOptions.Immediate);
				MineShaft.bugLevelLoop = null;
			}
			if (!Game1.IsMultiplayer && this.mineLevel == 20)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
		}

		// Token: 0x06003159 RID: 12633 RVA: 0x00270170 File Offset: 0x0026E370
		public Vector2 mineEntrancePosition(Farmer who)
		{
			if (!who.ridingMineElevator || this.tileBeneathElevator.Equals(Vector2.Zero))
			{
				return this.tileBeneathLadder;
			}
			return this.tileBeneathElevator;
		}

		// Token: 0x0600315A RID: 12634 RVA: 0x002701A7 File Offset: 0x0026E3A7
		private void generateContents()
		{
			this.ladderHasSpawned = false;
			this.loadLevel(this.mineLevel);
			this.chooseLevelType();
			this.findLadder();
			this.populateLevel();
		}

		// Token: 0x0600315B RID: 12635 RVA: 0x002701D0 File Offset: 0x0026E3D0
		public void chooseLevelType()
		{
			this.fogTime = 0;
			if (MineShaft.bugLevelLoop != null)
			{
				MineShaft.bugLevelLoop.Stop(AudioStopOptions.Immediate);
				MineShaft.bugLevelLoop = null;
			}
			this.ambientFog = false;
			this.rainbowLights.Value = false;
			this.isLightingDark.Value = false;
			Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed, (double)this.mineLevel, (double)(4 * this.mineLevel));
			this.lighting = new Color(80, 80, 40);
			if (this.getMineArea(-1) == 80)
			{
				this.lighting = new Color(100, 100, 50);
			}
			if (this.GetAdditionalDifficulty() > 0)
			{
				if (this.getMineArea(-1) == 40)
				{
					this.lighting = new Color(230, 200, 90);
					this.ambientFog = true;
					this.fogColor = new Color(0, 80, 255) * 0.55f;
					if (this.mineLevel < 50)
					{
						this.lighting = new Color(100, 80, 40);
						this.ambientFog = false;
					}
				}
			}
			else if (r.NextDouble() < 0.3 && this.mineLevel > 2)
			{
				this.isLightingDark.Value = true;
				this.lighting = new Color(120, 120, 40);
				if (r.NextDouble() < 0.3)
				{
					this.lighting = new Color(150, 150, 60);
				}
			}
			if (r.NextDouble() < 0.15 && this.mineLevel > 5 && this.mineLevel != 120)
			{
				this.isLightingDark.Value = true;
				int mineArea = this.getMineArea(-1);
				if (mineArea <= 10)
				{
					if (mineArea == 0 || mineArea == 10)
					{
						this.lighting = new Color(110, 110, 70);
					}
				}
				else if (mineArea != 40)
				{
					if (mineArea == 80)
					{
						this.lighting = new Color(90, 130, 70);
					}
				}
				else
				{
					this.lighting = Color.Black;
					if (this.GetAdditionalDifficulty() > 0)
					{
						this.lighting = new Color(237, 212, 185);
					}
				}
			}
			if (r.NextDouble() < 0.035 && this.getMineArea(-1) == 80 && this.mineLevel % 5 != 0 && !MineShaft.mushroomLevelsGeneratedToday.Contains(this.mineLevel))
			{
				this.rainbowLights.Value = true;
				MineShaft.mushroomLevelsGeneratedToday.Add(this.mineLevel);
			}
			if (this.isDarkArea() && this.mineLevel < 120)
			{
				this.isLightingDark.Value = true;
				this.lighting = ((this.getMineArea(-1) == 80) ? new Color(70, 100, 100) : new Color(150, 150, 120));
				if (this.getMineArea(-1) == 0)
				{
					this.ambientFog = true;
					this.fogColor = Color.Black;
				}
			}
			if (this.mineLevel == 100)
			{
				this.lighting = new Color(140, 140, 80);
			}
			if (this.getMineArea(-1) == 121)
			{
				this.lighting = new Color(110, 110, 40);
				if (r.NextDouble() < 0.05)
				{
					this.lighting = (r.NextBool() ? new Color(30, 30, 0) : new Color(150, 150, 50));
				}
			}
			if (this.getMineArea(-1) == 77377)
			{
				this.isLightingDark.Value = false;
				this.rainbowLights.Value = false;
				this.ambientFog = true;
				this.fogColor = Color.White * 0.4f;
				this.lighting = new Color(80, 80, 30);
			}
		}

		// Token: 0x0600315C RID: 12636 RVA: 0x0027057C File Offset: 0x0026E77C
		public static void yearUpdate()
		{
			MineShaft.permanentMineChanges.RemoveWhere((KeyValuePair<int, MineInfo> p) => p.Key > 120 || p.Key % 5 != 0);
			MineInfo change;
			if (MineShaft.permanentMineChanges.TryGetValue(5, out change))
			{
				change.platformContainersLeft = 6;
			}
			if (MineShaft.permanentMineChanges.TryGetValue(45, out change))
			{
				change.platformContainersLeft = 6;
			}
			if (MineShaft.permanentMineChanges.TryGetValue(85, out change))
			{
				change.platformContainersLeft = 6;
			}
		}

		// Token: 0x0600315D RID: 12637 RVA: 0x002705F8 File Offset: 0x0026E7F8
		private bool canAdd(int typeOfFeature, int numberSoFar)
		{
			MineInfo changes;
			if (MineShaft.permanentMineChanges.TryGetValue(this.mineLevel, out changes))
			{
				switch (typeOfFeature)
				{
				case 0:
					return changes.platformContainersLeft > numberSoFar;
				case 1:
					return changes.chestsLeft > numberSoFar;
				case 2:
					return changes.coalCartsLeft > numberSoFar;
				case 3:
					return changes.elevator == 0;
				}
			}
			return true;
		}

		// Token: 0x0600315E RID: 12638 RVA: 0x0027065C File Offset: 0x0026E85C
		public void updateMineLevelData(int feature, int amount = 1)
		{
			MineInfo changes;
			if (!MineShaft.permanentMineChanges.TryGetValue(this.mineLevel, out changes))
			{
				changes = (MineShaft.permanentMineChanges[this.mineLevel] = new MineInfo());
				if (this.mineLevel == 5 || this.mineLevel == 45 || this.mineLevel == 85)
				{
					this.forceFirstTime = true;
				}
			}
			switch (feature)
			{
			case 0:
				changes.platformContainersLeft += amount;
				return;
			case 1:
				changes.chestsLeft += amount;
				return;
			case 2:
				changes.coalCartsLeft += amount;
				return;
			case 3:
				changes.elevator += amount;
				return;
			default:
				return;
			}
		}

		// Token: 0x0600315F RID: 12639 RVA: 0x0027070A File Offset: 0x0026E90A
		public void chestConsumed()
		{
			Game1.player.chestConsumedMineLevels[this.mineLevel] = true;
		}

		// Token: 0x06003160 RID: 12640 RVA: 0x00270722 File Offset: 0x0026E922
		public bool isLevelSlimeArea()
		{
			return this.isSlimeArea;
		}

		// Token: 0x06003161 RID: 12641 RVA: 0x0027072C File Offset: 0x0026E92C
		public void checkForMapAlterations(int x, int y)
		{
			if (base.getTileIndexAt(x, y, "Buildings", "mine") == 194 && !this.canAdd(2, 0))
			{
				base.setMapTile(x, y, 195, "Buildings", "mine", null, true);
				base.setMapTile(x, y - 1, 179, "Front", "mine", null, true);
			}
		}

		// Token: 0x06003162 RID: 12642 RVA: 0x00270794 File Offset: 0x0026E994
		public void findLadder()
		{
			int found = 0;
			this.tileBeneathElevator = Vector2.Zero;
			bool lookForWater = this.mineLevel % 20 == 0;
			this.lightGlows.Clear();
			Layer buildingsLayer = this.map.RequireLayer("Buildings");
			for (int y = 0; y < buildingsLayer.LayerHeight; y++)
			{
				for (int x = 0; x < buildingsLayer.LayerWidth; x++)
				{
					int tileIndex = buildingsLayer.GetTileIndexAt(x, y, "mine");
					if (tileIndex != -1)
					{
						if (tileIndex != 112)
						{
							if (tileIndex == 115)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Mines_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
								defaultInterpolatedStringHandler.AppendLiteral("_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(x);
								defaultInterpolatedStringHandler.AppendLiteral("_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(y);
								string lightSourceId = defaultInterpolatedStringHandler.ToStringAndClear();
								this.tileBeneathLadder = new Vector2((float)x, (float)(y + 1));
								this.sharedLights.AddLight(new LightSource(lightSourceId + "_1", 4, new Vector2((float)x, (float)(y - 2)) * 64f + new Vector2(32f, 0f), 0.25f, new Color(0, 20, 50), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
								this.sharedLights.AddLight(new LightSource(lightSourceId + "_2", 4, new Vector2((float)x, (float)(y - 1)) * 64f + new Vector2(32f, 0f), 0.5f, new Color(0, 20, 50), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
								this.sharedLights.AddLight(new LightSource(lightSourceId + "_3", 4, new Vector2((float)x, (float)y) * 64f + new Vector2(32f, 0f), 0.75f, new Color(0, 20, 50), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
								this.sharedLights.AddLight(new LightSource(lightSourceId + "_4", 4, new Vector2((float)x, (float)(y + 1)) * 64f + new Vector2(32f, 0f), 1f, new Color(0, 20, 50), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
								found++;
							}
						}
						else
						{
							this.tileBeneathElevator = new Vector2((float)x, (float)(y + 1));
							found++;
						}
						if (this.lighting.Equals(Color.White) && found == 2 && !lookForWater)
						{
							return;
						}
						if (!this.lighting.Equals(Color.White))
						{
							if (tileIndex <= 66)
							{
								if (tileIndex != 48 && tileIndex - 65 > 1)
								{
									goto IL_3EA;
								}
							}
							else if (tileIndex - 81 > 1 && tileIndex != 97 && tileIndex != 113)
							{
								goto IL_3EA;
							}
							NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights = this.sharedLights;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Mines_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
							defaultInterpolatedStringHandler.AppendLiteral("_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(x);
							defaultInterpolatedStringHandler.AppendLiteral("_");
							defaultInterpolatedStringHandler.AppendFormatted<int>(y);
							defaultInterpolatedStringHandler.AppendLiteral("_5");
							sharedLights.AddLight(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)x, (float)y) * 64f, 2.5f, new Color(0, 50, 100), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
							if (tileIndex == 66)
							{
								this.lightGlows.Add(new Vector2((float)x, (float)y) * 64f + new Vector2(0f, 64f));
							}
							else if (tileIndex == 97 || tileIndex == 113)
							{
								this.lightGlows.Add(new Vector2((float)x, (float)y) * 64f + new Vector2(32f, 32f));
							}
						}
					}
					IL_3EA:
					if (Game1.IsMasterGame && base.isWaterTile(x, y) && this.getMineArea(-1) == 80 && Game1.random.NextDouble() < 0.1)
					{
						NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights2 = this.sharedLights;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Mines_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(x);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(y);
						defaultInterpolatedStringHandler.AppendLiteral("_Lava");
						sharedLights2.AddLight(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)x, (float)y) * 64f, 2f, new Color(0, 220, 220), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
					}
				}
			}
			if (this.isFallingDownShaft)
			{
				Vector2 p = default(Vector2);
				while (!this.isTileClearForMineObjects(p))
				{
					p.X = (float)Game1.random.Next(1, this.map.Layers[0].LayerWidth);
					p.Y = (float)Game1.random.Next(1, this.map.Layers[0].LayerHeight);
				}
				this.tileBeneathLadder = p;
				Game1.player.showFrame(5, false);
			}
			this.isFallingDownShaft = false;
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06003163 RID: 12643 RVA: 0x00270D17 File Offset: 0x0026EF17
		public int EnemyCount
		{
			get
			{
				return this.characters.Count((NPC p) => p is Monster);
			}
		}

		// Token: 0x06003164 RID: 12644 RVA: 0x00270D44 File Offset: 0x0026EF44
		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (this.mustKillAllMonstersToAdvance() && this.EnemyCount == 0)
			{
				Vector2 p = new Vector2((float)((int)this.tileBeneathLadder.X), (float)((int)this.tileBeneathLadder.Y));
				if (!base.hasTileAt((int)p.X, (int)p.Y, "Buildings", null))
				{
					this.createLadderAt(p, "newArtifact");
					if (this.mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
					}
				}
			}
			if (!this.isFogUp.Value && this.map != null && this.mineLevel % 5 != 0 && Game1.random.NextDouble() < 0.1 && !this.AnyOnlineFarmerHasBuff("23"))
			{
				if (this.mineLevel > 10 && !this.mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.11 && this.getMineArea(-1) != 77377)
				{
					this.isFogUp.Value = true;
					this.fogTime = 35000 + Game1.random.Next(-5, 6) * 1000;
					int mineArea = this.getMineArea(-1);
					if (mineArea <= 10)
					{
						if (mineArea != 0 && mineArea != 10)
						{
							return;
						}
						if (this.GetAdditionalDifficulty() > 0)
						{
							this.fogColor = (this.isDarkArea() ? new Color(255, 150, 0) : (Color.Cyan * 0.75f));
							return;
						}
						this.fogColor = (this.isDarkArea() ? Color.Khaki : (Color.Green * 0.75f));
						return;
					}
					else
					{
						if (mineArea == 40)
						{
							this.fogColor = Color.Blue * 0.75f;
							return;
						}
						if (mineArea == 80)
						{
							this.fogColor = Color.Red * 0.5f;
							return;
						}
						if (mineArea == 121)
						{
							this.fogColor = Color.BlueViolet * 1f;
							return;
						}
					}
				}
				else
				{
					this.spawnFlyingMonsterOffScreen();
				}
			}
		}

		// Token: 0x06003165 RID: 12645 RVA: 0x00270F6C File Offset: 0x0026F16C
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
			if (Utility.isOnScreen(spawnLocation * 64f, 64))
			{
				spawnLocation.X -= (float)(Game1.viewport.Width / 64);
			}
			int mineArea = this.getMineArea(-1);
			if (mineArea <= 40)
			{
				if (mineArea != 0)
				{
					if (mineArea != 10)
					{
						if (mineArea != 40)
						{
							return;
						}
						this.characters.Add(this.BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, this.mineLevel)
						{
							focusedOnFarmers = true
						}));
						base.playSound("batScreech", null, null, SoundContext.Default);
						return;
					}
					else
					{
						if (this.GetAdditionalDifficulty() > 0)
						{
							this.characters.Add(this.BuffMonsterIfNecessary(new BlueSquid(spawnLocation * 64f)
							{
								focusedOnFarmers = true
							}));
							return;
						}
						this.characters.Add(this.BuffMonsterIfNecessary(new Fly(spawnLocation * 64f)
						{
							focusedOnFarmers = true
						}));
						return;
					}
				}
				else if (this.mineLevel > 10 && this.isDarkArea())
				{
					this.characters.Add(this.BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, this.mineLevel)
					{
						focusedOnFarmers = true
					}));
					base.playSound("batScreech", null, null, SoundContext.Default);
					return;
				}
			}
			else
			{
				if (mineArea == 80)
				{
					this.characters.Add(this.BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, this.mineLevel)
					{
						focusedOnFarmers = true
					}));
					base.playSound("batScreech", null, null, SoundContext.Default);
					return;
				}
				if (mineArea != 121)
				{
					if (mineArea != 77377)
					{
						return;
					}
					this.characters.Add(new Bat(spawnLocation * 64f, 77377)
					{
						focusedOnFarmers = true
					});
					base.playSound("rockGolemHit", null, null, SoundContext.Default);
				}
				else
				{
					if (this.mineLevel < 171 || Game1.random.NextBool())
					{
						NetCollection<NPC> characters = this.characters;
						Serpent monster;
						if (this.GetAdditionalDifficulty() <= 0)
						{
							(monster = new Serpent(spawnLocation * 64f)).focusedOnFarmers = true;
						}
						else
						{
							(monster = new Serpent(spawnLocation * 64f, "Royal Serpent")).focusedOnFarmers = true;
						}
						characters.Add(this.BuffMonsterIfNecessary(monster));
						base.playSound("serpentDie", null, null, SoundContext.Default);
						return;
					}
					this.characters.Add(this.BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, this.mineLevel)
					{
						focusedOnFarmers = true
					}));
					base.playSound("batScreech", null, null, SoundContext.Default);
					return;
				}
			}
		}

		// Token: 0x06003166 RID: 12646 RVA: 0x00271370 File Offset: 0x0026F570
		public override void drawLightGlows(SpriteBatch b)
		{
			int mineArea = this.getMineArea(-1);
			Color c;
			if (mineArea <= 40)
			{
				if (mineArea == 0)
				{
					c = (this.isDarkArea() ? (Color.PaleGoldenrod * 0.5f) : (Color.PaleGoldenrod * 0.33f));
					goto IL_12A;
				}
				if (mineArea == 40)
				{
					c = Color.White * 0.65f;
					if (this.GetAdditionalDifficulty() <= 0)
					{
						goto IL_12A;
					}
					if (this.mineLevel % 40 < 30)
					{
						c = new Color(230, 225, 100) * 0.8f;
						goto IL_12A;
					}
					c = new Color(220, 240, 255) * 0.8f;
					goto IL_12A;
				}
			}
			else
			{
				if (mineArea == 80)
				{
					c = (this.isDarkArea() ? (Color.Pink * 0.4f) : (Color.Red * 0.33f));
					goto IL_12A;
				}
				if (mineArea == 121)
				{
					c = Color.White * 0.8f;
					if (this.isDinoArea)
					{
						c = Color.Orange * 0.5f;
						goto IL_12A;
					}
					goto IL_12A;
				}
			}
			c = Color.PaleGoldenrod * 0.33f;
			IL_12A:
			foreach (Vector2 v in this.lightGlows)
			{
				if (this.rainbowLights.Value)
				{
					switch ((int)(v.X / 64f + v.Y / 64f) % 4)
					{
					case 0:
						c = Color.Red * 0.5f;
						break;
					case 1:
						c = Color.Yellow * 0.5f;
						break;
					case 2:
						c = Color.Cyan * 0.33f;
						break;
					case 3:
						c = Color.Lime * 0.45f;
						break;
					}
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(88, 1779, 30, 30)), c, 0f, new Vector2(15f, 15f), 8f + (float)(96.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(v.X * 777f) + (double)(v.Y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06003167 RID: 12647 RVA: 0x00271624 File Offset: 0x0026F824
		public Monster BuffMonsterIfNecessary(Monster monster)
		{
			if (monster != null && monster.GetBaseDifficultyLevel() < this.GetAdditionalDifficulty())
			{
				monster.BuffForAdditionalDifficulty(this.GetAdditionalDifficulty() - monster.GetBaseDifficultyLevel());
				GreenSlime slime = monster as GreenSlime;
				if (slime != null)
				{
					if (this.mineLevel < 40)
					{
						slime.color.Value = new Color(Game1.random.Next(40, 70), Game1.random.Next(100, 190), 255);
					}
					else if (this.mineLevel < 80)
					{
						slime.color.Value = new Color(0, 180, 120);
					}
					else if (this.mineLevel < 120)
					{
						slime.color.Value = new Color(Game1.random.Next(180, 250), 20, 120);
					}
					else
					{
						slime.color.Value = new Color(Game1.random.Next(120, 180), 20, 255);
					}
				}
				this.setMonsterTextureToDangerousVersion(monster);
			}
			return monster;
		}

		// Token: 0x06003168 RID: 12648 RVA: 0x00271734 File Offset: 0x0026F934
		private void setMonsterTextureToDangerousVersion(Monster monster)
		{
			string newAssetName = monster.Sprite.textureName.Value + "_dangerous";
			if (!Game1.content.DoesAssetExist<Texture2D>(newAssetName))
			{
				return;
			}
			try
			{
				monster.Sprite.LoadTexture(newAssetName, true);
			}
			catch (Exception e)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed loading '");
				defaultInterpolatedStringHandler.AppendFormatted(newAssetName);
				defaultInterpolatedStringHandler.AppendLiteral("' texture for dangerous ");
				defaultInterpolatedStringHandler.AppendFormatted(monster.Name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), e);
			}
		}

		// Token: 0x06003169 RID: 12649 RVA: 0x002717E4 File Offset: 0x0026F9E4
		public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			FishingRod r = ((who != null) ? who.CurrentTool : null) as FishingRod;
			if (r != null && r.QualifiedItemId.Contains("TrainingRod"))
			{
				return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173).ToString(), 1, 0, false);
			}
			string fish = null;
			double chanceMultiplier = 1.0;
			chanceMultiplier += 0.4 * (double)who.FishingLevel;
			chanceMultiplier += (double)waterDepth * 0.1;
			string baitName = "";
			FishingRod rod = ((who != null) ? who.CurrentTool : null) as FishingRod;
			if (rod != null)
			{
				if (rod.HasCuriosityLure())
				{
					chanceMultiplier += 5.0;
				}
				Object bait2 = rod.GetBait();
				baitName = (((bait2 != null) ? bait2.Name : null) ?? "");
			}
			int mineArea = this.getMineArea(-1);
			if (mineArea <= 10)
			{
				if (mineArea == 0 || mineArea == 10)
				{
					chanceMultiplier += (double)(baitName.Contains("Stonefish") ? 10 : 0);
					if (Game1.random.NextDouble() < 0.02 + 0.01 * chanceMultiplier)
					{
						fish = "(O)158";
					}
				}
			}
			else if (mineArea != 40)
			{
				if (mineArea == 80)
				{
					chanceMultiplier += (double)(baitName.Contains("Lava Eel") ? 10 : 0);
					if (Game1.random.NextDouble() < 0.01 + 0.008 * chanceMultiplier)
					{
						fish = "(O)162";
					}
				}
			}
			else
			{
				chanceMultiplier += (double)(baitName.Contains("Ice Pip") ? 10 : 0);
				if (Game1.random.NextDouble() < 0.015 + 0.009 * chanceMultiplier)
				{
					fish = "(O)161";
				}
			}
			int quality = 0;
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 10f))
			{
				quality = 1;
			}
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 50f + (float)who.LuckLevel / 100f))
			{
				quality = 2;
			}
			if (fish != null)
			{
				return ItemRegistry.Create(fish, 1, quality, false);
			}
			if (this.getMineArea(-1) != 80)
			{
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "UndergroundMine");
			}
			if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.05)
			{
				return ItemRegistry.Create("(O)CaveJelly", 1, 0, false);
			}
			return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173).ToString(), 1, 0, false);
		}

		// Token: 0x0600316A RID: 12650 RVA: 0x00271AA4 File Offset: 0x0026FCA4
		private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
		{
			if (this.mineLevel == 1)
			{
				monsterChance = 0.0;
				itemChance = 0.0;
				gemStoneChance = 0.0;
			}
			else if (this.mineLevel % 5 == 0 && this.getMineArea(-1) != 121)
			{
				itemChance = 0.0;
				gemStoneChance = 0.0;
				if (this.mineLevel % 10 == 0)
				{
					monsterChance = 0.0;
				}
			}
			if (this.mustKillAllMonstersToAdvance())
			{
				monsterChance = 0.025;
				itemChance = 0.001;
				stoneChance = 0.0;
				gemStoneChance = 0.0;
				if (this.isDinoArea)
				{
					itemChance *= 4.0;
				}
			}
			monsterChance += 0.02 * (double)this.GetAdditionalDifficulty();
			bool flag = this.AnyOnlineFarmerHasBuff("23");
			bool has_spawn_monsters_buff = this.AnyOnlineFarmerHasBuff("24");
			if (flag && this.getMineArea(-1) != 121)
			{
				if (!has_spawn_monsters_buff)
				{
					monsterChance = 0.0;
				}
			}
			else if (has_spawn_monsters_buff)
			{
				monsterChance *= 2.0;
			}
			gemStoneChance /= 2.0;
			if (this.isQuarryArea || this.getMineArea(-1) == 77377)
			{
				gemStoneChance = 0.001;
				itemChance = 0.0001;
				stoneChance *= 2.0;
				monsterChance = 0.02;
			}
			if (this.GetAdditionalDifficulty() > 0 && this.getMineArea(-1) == 40)
			{
				monsterChance *= 0.6600000262260437;
			}
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121)
			{
				double finalModifier = 1.0;
				foreach (int invasionId in DesertFestival.CalicoStatueInvasionIds)
				{
					int invasionAmount;
					if (Game1.player.team.calicoStatueEffects.TryGetValue(invasionId, out invasionAmount))
					{
						monsterChance += (double)invasionAmount * 0.01;
					}
				}
				int monsterSurgeAmount;
				if (Game1.player.team.calicoStatueEffects.TryGetValue(7, out monsterSurgeAmount))
				{
					finalModifier += (double)monsterSurgeAmount * 0.2;
				}
				monsterChance *= finalModifier;
			}
		}

		// Token: 0x0600316B RID: 12651 RVA: 0x00271CE0 File Offset: 0x0026FEE0
		public bool AnyOnlineFarmerHasBuff(string which_buff)
		{
			if (which_buff == "23" && this.GetAdditionalDifficulty() > 0)
			{
				return false;
			}
			using (FarmerCollection.Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.hasBuff(which_buff))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600316C RID: 12652 RVA: 0x00271D54 File Offset: 0x0026FF54
		private void populateLevel()
		{
			this.objects.Clear();
			this.terrainFeatures.Clear();
			this.resourceClumps.Clear();
			this.debris.Clear();
			this.characters.Clear();
			this.ghostAdded = false;
			this.stonesLeftOnThisLevel = 0;
			if (this.mineLevel == 77377)
			{
				this.resourceClumps.Add(new ResourceClump(148, 2, 2, new Vector2(47f, 37f), null, "TileSheets\\Objects_2"));
				this.resourceClumps.Add(new ResourceClump(148, 2, 2, new Vector2(36f, 12f), null, "TileSheets\\Objects_2"));
			}
			double stoneChance = (double)this.mineRandom.Next(10, 30) / 100.0;
			double monsterChance = 0.002 + (double)this.mineRandom.Next(200) / 10000.0;
			double itemChance = 0.0025;
			double gemStoneChance = 0.003;
			this.adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
			int barrelsAdded = 0;
			bool firstTime = !MineShaft.permanentMineChanges.ContainsKey(this.mineLevel) || this.forceFirstTime;
			float df_barrelExtra = 0f;
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.mineLevel > 131)
			{
				df_barrelExtra += 1f - 130f / (float)this.mineLevel;
			}
			if (this.mineLevel > 1 && (this.mineLevel % 5 != 0 || this.mineLevel >= 121) && (this.mineRandom.NextBool() || this.isDinoArea))
			{
				Layer backLayer = this.map.RequireLayer("Back");
				int numBarrels = this.mineRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(this) * 20.0);
				if (this.isDinoArea)
				{
					numBarrels += this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight / 40;
				}
				for (int i = 0; i < numBarrels; i++)
				{
					Point p;
					Point motion;
					if (this.mineRandom.NextDouble() < 0.33 + (double)(df_barrelExtra / 2f))
					{
						p = new Point(this.mineRandom.Next(backLayer.LayerWidth), 0);
						motion = new Point(0, 1);
					}
					else if (this.mineRandom.NextBool())
					{
						p = new Point(0, this.mineRandom.Next(backLayer.LayerHeight));
						motion = new Point(1, 0);
					}
					else
					{
						p = new Point(backLayer.LayerWidth - 1, this.mineRandom.Next(backLayer.LayerHeight));
						motion = new Point(-1, 0);
					}
					while (base.isTileOnMap(p.X, p.Y))
					{
						p.X += motion.X;
						p.Y += motion.Y;
						if (this.isTileClearForMineObjects(p.X, p.Y))
						{
							Vector2 objectPos = new Vector2((float)p.X, (float)p.Y);
							if (this.isDinoArea)
							{
								this.terrainFeatures.Add(objectPos, new CosmeticPlant(this.mineRandom.Next(3)));
								break;
							}
							if (this.mustKillAllMonstersToAdvance())
							{
								break;
							}
							if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121 && !this.hasAddedDesertFestivalStatue && base.hasTileAt((int)objectPos.X, (int)objectPos.Y - 1, "Buildings", null))
							{
								this.calicoStatueSpot.Value = p;
								this.hasAddedDesertFestivalStatue = true;
								break;
							}
							this.objects.Add(objectPos, BreakableContainer.GetBarrelForMines(objectPos, this));
							break;
						}
					}
				}
			}
			bool spawned_prismatic_jelly = false;
			if (this.mineLevel % 10 != 0 || (this.getMineArea(-1) == 121 && !this.isForcedChestLevel(this.mineLevel) && !this.netIsTreasureRoom.Value))
			{
				Layer backLayer2 = this.map.RequireLayer("Back");
				for (int j = 0; j < backLayer2.LayerWidth; j++)
				{
					for (int k = 0; k < backLayer2.LayerHeight; k++)
					{
						this.checkForMapAlterations(j, k);
						if (this.isTileClearForMineObjects(j, k))
						{
							if (this.mineRandom.NextDouble() <= stoneChance)
							{
								Vector2 objectPos2 = new Vector2((float)j, (float)k);
								if (!base.Objects.ContainsKey(objectPos2))
								{
									if (this.getMineArea(-1) == 40 && this.mineRandom.NextDouble() < 0.15)
									{
										int which = this.mineRandom.Next(319, 322);
										if (this.GetAdditionalDifficulty() > 0 && this.mineLevel % 40 < 30)
										{
											which = this.mineRandom.Next(313, 316);
										}
										base.Objects.Add(objectPos2, new Object(which.ToString(), 1, false, -1, 0)
										{
											Fragility = 2,
											CanBeGrabbed = true
										});
									}
									else if (this.rainbowLights.Value && this.mineRandom.NextDouble() < 0.55)
									{
										if (this.mineRandom.NextDouble() < 0.25)
										{
											string which2;
											if (this.mineRandom.Next(5) == 0)
											{
												which2 = "(O)422";
											}
											else
											{
												which2 = "(O)420";
											}
											Object obj = ItemRegistry.Create<Object>(which2, 1, 0, false);
											obj.IsSpawnedObject = true;
											base.Objects.Add(objectPos2, obj);
										}
									}
									else
									{
										Object litter = this.createLitterObject(0.001, 5E-05, gemStoneChance, objectPos2);
										if (litter != null)
										{
											base.Objects.Add(objectPos2, litter);
											if (litter.IsBreakableStone())
											{
												int m = this.stonesLeftOnThisLevel;
												this.stonesLeftOnThisLevel = m + 1;
											}
										}
									}
								}
							}
							else if (this.mineRandom.NextDouble() <= monsterChance && this.getDistanceFromStart(j, k) > 5f)
							{
								Monster monsterToAdd = null;
								if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121)
								{
									foreach (int invasionId in DesertFestival.CalicoStatueInvasionIds)
									{
										int amount;
										if (Game1.player.team.calicoStatueEffects.TryGetValue(invasionId, out amount))
										{
											int invasion = 0;
											while (invasion < amount)
											{
												if (this.mineRandom.NextBool(0.15))
												{
													Vector2 position = new Vector2((float)j, (float)k) * 64f;
													switch (invasionId)
													{
													case 0:
														monsterToAdd = new Ghost(position, "Carbon Ghost");
														goto IL_761;
													case 1:
														monsterToAdd = new Serpent(position);
														goto IL_761;
													case 2:
														if (this.mineRandom.NextDouble() < 0.33)
														{
															monsterToAdd = new Bat(position, 77377);
														}
														else
														{
															monsterToAdd = new Skeleton(position, this.mineRandom.NextBool());
														}
														monsterToAdd.BuffForAdditionalDifficulty(1);
														goto IL_761;
													case 3:
														monsterToAdd = new Bat(position, this.mineLevel);
														goto IL_761;
													default:
														goto IL_761;
													}
												}
												else
												{
													invasion++;
												}
											}
										}
										IL_761:;
									}
								}
								if (monsterToAdd == null)
								{
									monsterToAdd = this.BuffMonsterIfNecessary(this.getMonsterForThisLevel(this.mineLevel, j, k));
								}
								GreenSlime slime = monsterToAdd as GreenSlime;
								if (slime == null)
								{
									if (!(monsterToAdd is Leaper))
									{
										if (!(monsterToAdd is Grub))
										{
											if (monsterToAdd is DustSpirit)
											{
												if (this.mineRandom.NextDouble() < 0.6)
												{
													this.tryToAddMonster(this.BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j - 1, k);
												}
												if (this.mineRandom.NextDouble() < 0.6)
												{
													this.tryToAddMonster(this.BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j + 1, k);
												}
												if (this.mineRandom.NextDouble() < 0.6)
												{
													this.tryToAddMonster(this.BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j, k - 1);
												}
												if (this.mineRandom.NextDouble() < 0.6)
												{
													this.tryToAddMonster(this.BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j, k + 1);
												}
											}
										}
										else
										{
											if (this.mineRandom.NextDouble() < 0.4)
											{
												this.tryToAddMonster(this.BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j - 1, k);
											}
											if (this.mineRandom.NextDouble() < 0.4)
											{
												this.tryToAddMonster(this.BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j + 1, k);
											}
											if (this.mineRandom.NextDouble() < 0.4)
											{
												this.tryToAddMonster(this.BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j, k - 1);
											}
											if (this.mineRandom.NextDouble() < 0.4)
											{
												this.tryToAddMonster(this.BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j, k + 1);
											}
										}
									}
									else
									{
										float partner_chance = (float)(this.GetAdditionalDifficulty() + 1) * 0.3f;
										if (this.mineRandom.NextDouble() < (double)partner_chance)
										{
											this.tryToAddMonster(this.BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j - 1, k);
										}
										if (this.mineRandom.NextDouble() < (double)partner_chance)
										{
											this.tryToAddMonster(this.BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j + 1, k);
										}
										if (this.mineRandom.NextDouble() < (double)partner_chance)
										{
											this.tryToAddMonster(this.BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j, k - 1);
										}
										if (this.mineRandom.NextDouble() < (double)partner_chance)
										{
											this.tryToAddMonster(this.BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j, k + 1);
										}
									}
								}
								else
								{
									if (!spawned_prismatic_jelly && Game1.random.NextDouble() <= Math.Max(0.01, 0.012 + Game1.player.team.AverageDailyLuck(this) / 10.0) && Game1.player.team.SpecialOrderActive("Wizard2"))
									{
										slime.makePrismatic();
										spawned_prismatic_jelly = true;
									}
									if (this.GetAdditionalDifficulty() > 0 && this.mineRandom.NextDouble() < (double)Math.Min((float)this.GetAdditionalDifficulty() * 0.1f, 0.5f))
									{
										if (this.mineRandom.NextDouble() < 0.009999999776482582)
										{
											slime.stackedSlimes.Value = 4;
										}
										else
										{
											slime.stackedSlimes.Value = 2;
										}
									}
								}
								if (this.mineRandom.NextDouble() < 0.00175)
								{
									monsterToAdd.hasSpecialItem.Value = true;
								}
								if (monsterToAdd.GetBoundingBox().Width <= 64 || this.isTileClearForMineObjects(j + 1, k))
								{
									this.characters.Add(monsterToAdd);
								}
							}
							else if (this.mineRandom.NextDouble() <= itemChance)
							{
								Vector2 objectPos3 = new Vector2((float)j, (float)k);
								base.Objects.Add(objectPos3, this.getRandomItemForThisLevel(this.mineLevel, objectPos3));
							}
							else if (this.mineRandom.NextDouble() <= 0.005 && !this.isDarkArea() && !this.mustKillAllMonstersToAdvance() && (this.GetAdditionalDifficulty() <= 0 || (this.getMineArea(-1) == 40 && this.mineLevel % 40 < 30)))
							{
								if (this.isTileClearForMineObjects(j + 1, k) && this.isTileClearForMineObjects(j, k + 1) && this.isTileClearForMineObjects(j + 1, k + 1))
								{
									Vector2 objectPos4 = new Vector2((float)j, (float)k);
									int whichClump = this.mineRandom.Choose(752, 754);
									if (this.getMineArea(-1) == 40)
									{
										if (this.GetAdditionalDifficulty() > 0)
										{
											whichClump = 600;
											if (this.mineRandom.NextDouble() < 0.1)
											{
												whichClump = 602;
											}
										}
										else
										{
											whichClump = this.mineRandom.Choose(756, 758);
										}
									}
									this.resourceClumps.Add(new ResourceClump(whichClump, 2, 2, objectPos4, null, null));
								}
							}
							else if (this.GetAdditionalDifficulty() > 0)
							{
								if (this.getMineArea(-1) == 40 && this.mineLevel % 40 < 30 && this.mineRandom.NextDouble() < 0.01 && base.hasTileAt(j, k - 1, "Buildings", null))
								{
									this.terrainFeatures.Add(new Vector2((float)j, (float)k), new Tree("8", 5, false));
								}
								else if (this.getMineArea(-1) == 40 && this.mineLevel % 40 < 30 && this.mineRandom.NextDouble() < 0.1 && (base.hasTileAt(j, k - 1, "Buildings", null) || base.hasTileAt(j - 1, k, "Buildings", null) || base.hasTileAt(j, k + 1, "Buildings", null) || base.hasTileAt(j + 1, k, "Buildings", null) || this.terrainFeatures.ContainsKey(new Vector2((float)(j - 1), (float)k)) || this.terrainFeatures.ContainsKey(new Vector2((float)(j + 1), (float)k)) || this.terrainFeatures.ContainsKey(new Vector2((float)j, (float)(k - 1))) || this.terrainFeatures.ContainsKey(new Vector2((float)j, (float)(k + 1)))))
								{
									this.terrainFeatures.Add(new Vector2((float)j, (float)k), new Grass((this.mineLevel >= 50) ? 6 : 5, (this.mineLevel >= 50) ? 1 : this.mineRandom.Next(1, 5)));
								}
								else if (this.getMineArea(-1) == 80 && !this.isDarkArea() && this.mineRandom.NextDouble() < 0.1 && (base.hasTileAt(j, k - 1, "Buildings", null) || base.hasTileAt(j - 1, k, "Buildings", null) || base.hasTileAt(j, k + 1, "Buildings", null) || base.hasTileAt(j + 1, k, "Buildings", null) || this.terrainFeatures.ContainsKey(new Vector2((float)(j - 1), (float)k)) || this.terrainFeatures.ContainsKey(new Vector2((float)(j + 1), (float)k)) || this.terrainFeatures.ContainsKey(new Vector2((float)j, (float)(k - 1))) || this.terrainFeatures.ContainsKey(new Vector2((float)j, (float)(k + 1)))))
								{
									this.terrainFeatures.Add(new Vector2((float)j, (float)k), new Grass(4, this.mineRandom.Next(1, 5)));
								}
							}
						}
						else if (this.isContainerPlatform(j, k) && this.CanItemBePlacedHere(new Vector2((float)j, (float)k), false, CollisionMask.All, ~CollisionMask.Objects, false, false) && this.mineRandom.NextDouble() < 0.4 && (firstTime || this.canAdd(0, barrelsAdded)))
						{
							Vector2 objectPos5 = new Vector2((float)j, (float)k);
							this.objects.Add(objectPos5, BreakableContainer.GetBarrelForMines(objectPos5, this));
							barrelsAdded++;
							if (firstTime)
							{
								this.updateMineLevelData(0, 1);
							}
						}
						else if (this.mineRandom.NextDouble() <= monsterChance && this.CanSpawnCharacterHere(new Vector2((float)j, (float)k)) && this.isTileOnClearAndSolidGround(j, k) && this.getDistanceFromStart(j, k) > 5f && (!this.AnyOnlineFarmerHasBuff("23") || this.getMineArea(-1) == 121))
						{
							Monster monsterToAdd2 = this.BuffMonsterIfNecessary(this.getMonsterForThisLevel(this.mineLevel, j, k));
							if (monsterToAdd2.GetBoundingBox().Width <= 64 || this.isTileClearForMineObjects(j + 1, k))
							{
								if (this.mineRandom.NextDouble() < 0.01)
								{
									monsterToAdd2.hasSpecialItem.Value = true;
								}
								this.characters.Add(monsterToAdd2);
							}
						}
					}
				}
				if (this.stonesLeftOnThisLevel > 35)
				{
					int tries = this.stonesLeftOnThisLevel / 35;
					for (int l = 0; l < tries; l++)
					{
						Vector2 stone;
						Object obj2;
						if (Utility.TryGetRandom(this.objects, out stone, out obj2, null) && obj2.IsBreakableStone())
						{
							int radius = this.mineRandom.Next(3, 8);
							bool monsterSpot = this.mineRandom.NextDouble() < 0.1;
							int x = (int)stone.X - radius / 2;
							while ((float)x < stone.X + (float)(radius / 2))
							{
								int y = (int)stone.Y - radius / 2;
								while ((float)y < stone.Y + (float)(radius / 2))
								{
									Vector2 tile = new Vector2((float)x, (float)y);
									Object tileObj;
									if (this.objects.TryGetValue(tile, out tileObj) && tileObj.IsBreakableStone())
									{
										this.objects.Remove(tile);
										int m = this.stonesLeftOnThisLevel;
										this.stonesLeftOnThisLevel = m - 1;
										if (this.getDistanceFromStart(x, y) > 5f && monsterSpot && this.mineRandom.NextDouble() < 0.12)
										{
											Monster monster = this.BuffMonsterIfNecessary(this.getMonsterForThisLevel(this.mineLevel, x, y));
											if (monster.GetBoundingBox().Width <= 64 || this.isTileClearForMineObjects(x + 1, y))
											{
												this.characters.Add(monster);
											}
										}
									}
									y++;
								}
								x++;
							}
						}
					}
				}
				this.tryToAddAreaUniques();
				if (this.mineRandom.NextDouble() < 0.95 && !this.mustKillAllMonstersToAdvance() && this.mineLevel > 1 && this.mineLevel % 5 != 0 && this.shouldCreateLadderOnThisLevel())
				{
					Vector2 possibleSpot = new Vector2((float)this.mineRandom.Next(backLayer2.LayerWidth), (float)this.mineRandom.Next(backLayer2.LayerHeight));
					if (this.isTileClearForMineObjects(possibleSpot))
					{
						this.createLadderDown((int)possibleSpot.X, (int)possibleSpot.Y, false);
					}
				}
				if (this.mustKillAllMonstersToAdvance() && this.EnemyCount <= 1)
				{
					this.characters.Add(new Bat(this.tileBeneathLadder * 64f + new Vector2(256f, 256f)));
				}
			}
			if ((!this.mustKillAllMonstersToAdvance() || this.isDinoArea) && this.mineLevel % 5 != 0 && this.mineLevel > 2 && !this.isForcedChestLevel(this.mineLevel) && !this.netIsTreasureRoom.Value)
			{
				this.tryToAddOreClumps();
				if (this.isLightingDark.Value)
				{
					this.tryToAddOldMinerPath();
				}
			}
		}

		// Token: 0x0600316D RID: 12653 RVA: 0x00273122 File Offset: 0x00271322
		public void placeAppropriateOreAt(Vector2 tile)
		{
			if (this.CanItemBePlacedHere(tile, false, CollisionMask.All, CollisionMask.None, false, false))
			{
				this.objects.Add(tile, this.getAppropriateOre(tile));
			}
		}

		// Token: 0x0600316E RID: 12654 RVA: 0x0027314C File Offset: 0x0027134C
		public Object getAppropriateOre(Vector2 tile)
		{
			Object ore = new Object("751", 1, false, -1, 0)
			{
				MinutesUntilReady = 3
			};
			int mineArea = this.getMineArea(-1);
			if (mineArea <= 10)
			{
				if (mineArea == 0 || mineArea == 10)
				{
					if (this.GetAdditionalDifficulty() > 0)
					{
						ore = new Object("849", 1, false, -1, 0)
						{
							MinutesUntilReady = 6
						};
					}
				}
			}
			else if (mineArea != 40)
			{
				if (mineArea != 80)
				{
					if (mineArea == 121)
					{
						if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.mineRandom.NextBool(0.25 + (double)((float)(Game1.player.team.calicoEggSkullCavernRating.Value * 5) / 100f)))
						{
							ore = new Object("CalicoEggStone_" + this.mineRandom.Next(3).ToString(), 1, false, -1, 0)
							{
								MinutesUntilReady = 8
							};
						}
						else
						{
							ore = new Object("764", 1, false, -1, 0)
							{
								MinutesUntilReady = 8
							};
							if (this.mineRandom.NextDouble() < 0.02)
							{
								ore = new Object("765", 1, false, -1, 0)
								{
									MinutesUntilReady = 16
								};
							}
						}
					}
				}
				else if (this.mineRandom.NextDouble() < 0.8)
				{
					ore = new Object("764", 1, false, -1, 0)
					{
						MinutesUntilReady = 8
					};
				}
			}
			else if (this.GetAdditionalDifficulty() > 0)
			{
				ore = new ColoredObject("290", 1, new Color(150, 225, 160))
				{
					MinutesUntilReady = 6,
					TileLocation = tile,
					Flipped = this.mineRandom.NextBool()
				};
			}
			else if (this.mineRandom.NextDouble() < 0.8)
			{
				ore = new Object("290", 1, false, -1, 0)
				{
					MinutesUntilReady = 4
				};
			}
			if (this.mineRandom.NextDouble() < 0.25 && this.getMineArea(-1) != 40 && this.GetAdditionalDifficulty() <= 0)
			{
				ore = new Object(this.mineRandom.Choose("668", "670"), 1, false, -1, 0)
				{
					MinutesUntilReady = 2
				};
			}
			return ore;
		}

		// Token: 0x0600316F RID: 12655 RVA: 0x00273388 File Offset: 0x00271588
		public void tryToAddOreClumps()
		{
			if (this.mineRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(this))
			{
				Vector2 endPoint = base.getRandomTile(null);
				int tries = 0;
				while (tries < 1 || this.mineRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(this))
				{
					if (this.CanItemBePlacedHere(endPoint, false, CollisionMask.All, CollisionMask.None, false, false) && this.isTileOnClearAndSolidGround(endPoint) && this.doesTileHaveProperty((int)endPoint.X, (int)endPoint.Y, "Diggable", "Back", false) == null)
					{
						Object ore = this.getAppropriateOre(endPoint);
						if (ore.QualifiedItemId == "(O)670")
						{
							ore = new Object("668", 1, false, -1, 0);
						}
						bool hasVariant = ore.QualifiedItemId == "(O)668";
						if (ore.QualifiedItemId.Contains("CalicoEgg"))
						{
							Utility.recursiveObjectPlacement(ore, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", 0, 0.05000000074505806, 1, new List<string>
							{
								"CalicoEggStone_0",
								"CalicoEggStone_1",
								"CalicoEggStone_2"
							});
						}
						else
						{
							Utility.recursiveObjectPlacement(ore, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", (hasVariant > false) ? 1 : 0, 0.05000000074505806, hasVariant ? 2 : 1, null);
						}
					}
					endPoint = base.getRandomTile(null);
					tries++;
				}
			}
		}

		// Token: 0x06003170 RID: 12656 RVA: 0x0027353C File Offset: 0x0027173C
		public void tryToAddOldMinerPath()
		{
			Vector2 endPoint = base.getRandomTile(null);
			int tries = 0;
			while (!this.isTileOnClearAndSolidGround(endPoint) && tries < 8)
			{
				endPoint = base.getRandomTile(null);
				tries++;
			}
			if (this.isTileOnClearAndSolidGround(endPoint))
			{
				Stack<Point> path = PathFindController.findPath(Utility.Vector2ToPoint(this.tileBeneathLadder), Utility.Vector2ToPoint(endPoint), new PathFindController.isAtEnd(PathFindController.isAtEndPoint), this, Game1.player, 500);
				if (path != null)
				{
					while (path.Count > 0)
					{
						Point p = path.Pop();
						this.removeObjectsAndSpawned(p.X, p.Y, 1, 1);
						if (path.Count > 0 && this.mineRandom.NextDouble() < 0.2)
						{
							Vector2 torchPosition = (path.Peek().X == p.X) ? new Vector2((float)(p.X + this.mineRandom.Choose(-1, 1)), (float)p.Y) : new Vector2((float)p.X, (float)(p.Y + this.mineRandom.Choose(-1, 1)));
							if (!torchPosition.Equals(Vector2.Zero) && this.CanItemBePlacedHere(torchPosition, false, CollisionMask.All, ~CollisionMask.Objects, false, false) && this.isTileOnClearAndSolidGround(torchPosition))
							{
								if (this.mineRandom.NextBool())
								{
									new Torch().placementAction(this, (int)torchPosition.X * 64, (int)torchPosition.Y * 64, null);
								}
								else
								{
									this.placeAppropriateOreAt(torchPosition);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06003171 RID: 12657 RVA: 0x002736C4 File Offset: 0x002718C4
		public void tryToAddAreaUniques()
		{
			if ((this.getMineArea(-1) == 10 || this.getMineArea(-1) == 80 || (this.getMineArea(-1) == 40 && this.mineRandom.NextDouble() < 0.1)) && !this.isDarkArea() && !this.mustKillAllMonstersToAdvance())
			{
				int tries = this.mineRandom.Next(7, 24);
				int baseWeedIndex = (this.getMineArea(-1) == 80) ? 316 : ((this.getMineArea(-1) == 40) ? 319 : 313);
				Color tintColor = Color.White;
				int indexRandomizeRange = 2;
				if (this.GetAdditionalDifficulty() > 0)
				{
					if (this.getMineArea(-1) == 10)
					{
						baseWeedIndex = 674;
						tintColor = new Color(30, 120, 255);
					}
					else if (this.getMineArea(-1) == 40)
					{
						if (this.mineLevel % 40 >= 30)
						{
							baseWeedIndex = 319;
						}
						else
						{
							baseWeedIndex = 882;
							tintColor = new Color(100, 180, 220);
						}
					}
					else if (this.getMineArea(-1) == 80)
					{
						return;
					}
				}
				Layer backLayer = this.map.RequireLayer("Back");
				for (int i = 0; i < tries; i++)
				{
					Vector2 tile = new Vector2((float)this.mineRandom.Next(backLayer.LayerWidth), (float)this.mineRandom.Next(backLayer.LayerHeight));
					if (tintColor.Equals(Color.White))
					{
						Utility.recursiveObjectPlacement(new Object(baseWeedIndex.ToString(), 1, false, -1, 0)
						{
							Fragility = 2,
							CanBeGrabbed = true
						}, (int)tile.X, (int)tile.Y, 1.0, (double)((float)this.mineRandom.Next(10, 40) / 100f), this, "Dirt", indexRandomizeRange, 0.29, 1, null);
					}
					else
					{
						Utility.recursiveObjectPlacement(new ColoredObject(baseWeedIndex.ToString(), 1, tintColor)
						{
							Fragility = 2,
							CanBeGrabbed = true,
							CanBeSetDown = true,
							TileLocation = tile
						}, (int)tile.X, (int)tile.Y, 1.0, (double)((float)this.mineRandom.Next(10, 40) / 100f), this, "Dirt", indexRandomizeRange, 0.29, 1, null);
					}
				}
			}
		}

		// Token: 0x06003172 RID: 12658 RVA: 0x00273914 File Offset: 0x00271B14
		public bool tryToAddMonster(Monster m, int tileX, int tileY)
		{
			if (this.isTileClearForMineObjects(tileX, tileY) && !this.IsTileOccupiedBy(new Vector2((float)tileX, (float)tileY), CollisionMask.All, CollisionMask.None, false))
			{
				m.setTilePosition(tileX, tileY);
				this.characters.Add(m);
				return true;
			}
			return false;
		}

		// Token: 0x06003173 RID: 12659 RVA: 0x0027394F File Offset: 0x00271B4F
		public bool isContainerPlatform(int x, int y)
		{
			return base.getTileIndexAt(x, y, "Back", "mine") == 257;
		}

		// Token: 0x06003174 RID: 12660 RVA: 0x0027396A File Offset: 0x00271B6A
		public bool mustKillAllMonstersToAdvance()
		{
			return this.isSlimeArea || this.isMonsterArea || this.isDinoArea;
		}

		// Token: 0x06003175 RID: 12661 RVA: 0x00273984 File Offset: 0x00271B84
		public void createLadderAt(Vector2 p, string sound = "hoeHit")
		{
			if (this.shouldCreateLadderOnThisLevel())
			{
				base.playSound(sound, null, null, SoundContext.Default);
				this.createLadderAtEvent[p] = true;
			}
		}

		// Token: 0x06003176 RID: 12662 RVA: 0x002739C0 File Offset: 0x00271BC0
		public bool shouldCreateLadderOnThisLevel()
		{
			return this.mineLevel != 77377 && this.mineLevel != 120;
		}

		// Token: 0x06003177 RID: 12663 RVA: 0x002739E0 File Offset: 0x00271BE0
		private void doCreateLadderAt(Vector2 p)
		{
			string startSound = (Game1.currentLocation == this) ? "sandyStep" : null;
			this.updateMap();
			base.setMapTile((int)p.X, (int)p.Y, 173, "Buildings", "mine", null, true);
			this.temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f, Color.White * 0.5f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				interval = 80f
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(16f, 16f), Color.White * 0.5f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 150,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f + new Vector2(32f, 16f), Color.White * 0.5f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 300,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(32f, -16f), Color.White * 0.5f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 450,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(-16f, 16f), Color.White * 0.5f, 8, false, 100f, 0, -1, -1f, -1, 0)
			{
				delayBeforeAnimationStart = 600,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)p.X * 64, (int)p.Y * 64, 64, 64));
			}
		}

		// Token: 0x06003178 RID: 12664 RVA: 0x00273C88 File Offset: 0x00271E88
		public bool recursiveTryToCreateLadderDown(Vector2 centerTile, string sound = "hoeHit", int maxIterations = 16)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(centerTile);
			List<Vector2> closedList = new List<Vector2>();
			while (iterations < maxIterations && positionsToCheck.Count > 0)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				if (!this.IsTileOccupiedBy(currentPoint, CollisionMask.All, CollisionMask.None, false) && this.isTileOnClearAndSolidGround(currentPoint) && this.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back", false) != null && this.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back", false).Equals("Stone"))
				{
					this.createLadderAt(currentPoint, "hoeHit");
					return true;
				}
				foreach (Vector2 v in Utility.DirectionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}
			return false;
		}

		// Token: 0x06003179 RID: 12665 RVA: 0x00273D88 File Offset: 0x00271F88
		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			if (monster.hasSpecialItem.Value)
			{
				Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(this.mineLevel, x / 64, y / 64), monster.Position, Game1.random.Next(4), monster.currentLocation, -1, false);
			}
			else if (this.mineLevel > 121 && who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && who.houseUpgradeLevel.Value >= 1 && !who.isMarriedOrRoommates() && !who.isEngaged() && Game1.random.NextDouble() < 0.001)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)808", 1, 0, false), monster.Position, Game1.random.Next(4), monster.currentLocation, -1, false);
			}
			else
			{
				base.monsterDrop(monster, x, y, who);
			}
			double extraLadderChance = (who != null && who.hasBuff("dwarfStatue_1")) ? 0.07 : 0.0;
			if ((!this.mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.15 + extraLadderChance) || (this.mustKillAllMonstersToAdvance() && this.EnemyCount <= 1))
			{
				Vector2 p = new Vector2((float)x, (float)y) / 64f;
				p.X = (float)((int)p.X);
				p.Y = (float)((int)p.Y);
				monster.IsInvisible = true;
				if (!this.IsTileOccupiedBy(p, CollisionMask.All, CollisionMask.None, false) && this.isTileOnClearAndSolidGround(p) && this.doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back", false) != null && this.doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back", false).Equals("Stone"))
				{
					this.createLadderAt(p, "hoeHit");
					return;
				}
				if (this.mustKillAllMonstersToAdvance() && this.EnemyCount <= 1)
				{
					p = new Vector2((float)((int)this.tileBeneathLadder.X), (float)((int)this.tileBeneathLadder.Y));
					this.createLadderAt(p, "newArtifact");
					if (this.mustKillAllMonstersToAdvance() && who.IsLocalPlayer && who.currentLocation == this)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
					}
				}
			}
		}

		// Token: 0x0600317A RID: 12666 RVA: 0x00273FDC File Offset: 0x002721DC
		public Item GetReplacementChestItem(int floor)
		{
			List<Item> valid_items = null;
			if (Game1.netWorldState.Value.ShuffleMineChests == Game1.MineChestType.Remixed)
			{
				valid_items = new List<Item>();
				if (floor <= 60)
				{
					if (floor <= 20)
					{
						if (floor != 10)
						{
							if (floor == 20)
							{
								valid_items.Add(ItemRegistry.Create("(W)11", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)24", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)20", 1, 0, false));
								valid_items.Add(new Ring("517"));
								valid_items.Add(new Ring("519"));
							}
						}
						else
						{
							valid_items.Add(ItemRegistry.Create("(B)506", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(B)507", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)12", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)17", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)22", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)31", 1, 0, false));
						}
					}
					else if (floor != 40)
					{
						if (floor != 50)
						{
							if (floor == 60)
							{
								valid_items.Add(ItemRegistry.Create("(W)21", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)44", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)6", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)18", 1, 0, false));
								valid_items.Add(ItemRegistry.Create("(W)27", 1, 0, false));
							}
						}
						else
						{
							valid_items.Add(ItemRegistry.Create("(B)509", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(B)510", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(B)508", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)1", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)43", 1, 0, false));
						}
					}
				}
				else if (floor <= 80)
				{
					if (floor != 70)
					{
						if (floor == 80)
						{
							valid_items.Add(ItemRegistry.Create("(B)512", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(B)511", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)10", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)7", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)46", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)19", 1, 0, false));
						}
					}
				}
				else if (floor != 90)
				{
					if (floor != 100)
					{
						if (floor == 110)
						{
							valid_items.Add(ItemRegistry.Create("(B)514", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(B)878", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)50", 1, 0, false));
							valid_items.Add(ItemRegistry.Create("(W)28", 1, 0, false));
						}
					}
				}
				else
				{
					valid_items.Add(ItemRegistry.Create("(W)8", 1, 0, false));
					valid_items.Add(ItemRegistry.Create("(W)52", 1, 0, false));
					valid_items.Add(ItemRegistry.Create("(W)45", 1, 0, false));
					valid_items.Add(ItemRegistry.Create("(W)5", 1, 0, false));
					valid_items.Add(ItemRegistry.Create("(W)60", 1, 0, false));
				}
			}
			if (valid_items != null && valid_items.Count > 0)
			{
				return Utility.CreateRandom(Game1.uniqueIDForThisGame * 512.0, (double)floor, 0.0, 0.0, 0.0).ChooseFrom(valid_items);
			}
			return null;
		}

		// Token: 0x0600317B RID: 12667 RVA: 0x00274380 File Offset: 0x00272580
		private void addLevelChests()
		{
			List<Item> chestItem = new List<Item>();
			Vector2 chestSpot = new Vector2(9f, 9f);
			Color tint = Color.White;
			if (this.mineLevel < 121 && this.mineLevel % 20 == 0 && this.mineLevel % 40 != 0)
			{
				chestSpot.Y += 4f;
			}
			Item replacement_item = this.GetReplacementChestItem(this.mineLevel);
			bool force_treasure_room = false;
			if (replacement_item != null)
			{
				chestItem.Add(replacement_item);
			}
			else
			{
				int mineLevel = this.mineLevel;
				if (mineLevel <= 70)
				{
					if (mineLevel <= 20)
					{
						if (mineLevel != 5)
						{
							if (mineLevel != 10)
							{
								if (mineLevel == 20)
								{
									chestItem.Add(ItemRegistry.Create("(W)11", 1, 0, false));
								}
							}
							else
							{
								chestItem.Add(ItemRegistry.Create("(B)506", 1, 0, false));
							}
						}
						else
						{
							Game1.player.completeQuest("14");
							if (!Game1.player.hasOrWillReceiveMail("guildQuest"))
							{
								Game1.addMailForTomorrow("guildQuest", false, false);
							}
						}
					}
					else if (mineLevel <= 50)
					{
						if (mineLevel != 40)
						{
							if (mineLevel == 50)
							{
								chestItem.Add(ItemRegistry.Create("(B)509", 1, 0, false));
							}
						}
						else
						{
							Game1.player.completeQuest("17");
							chestItem.Add(ItemRegistry.Create("(W)32", 1, 0, false));
						}
					}
					else if (mineLevel != 60)
					{
						if (mineLevel == 70)
						{
							chestItem.Add(ItemRegistry.Create("(W)33", 1, 0, false));
						}
					}
					else
					{
						chestItem.Add(ItemRegistry.Create("(W)21", 1, 0, false));
					}
				}
				else if (mineLevel <= 110)
				{
					if (mineLevel <= 90)
					{
						if (mineLevel != 80)
						{
							if (mineLevel == 90)
							{
								chestItem.Add(ItemRegistry.Create("(W)8", 1, 0, false));
							}
						}
						else
						{
							chestItem.Add(ItemRegistry.Create("(B)512", 1, 0, false));
						}
					}
					else if (mineLevel != 100)
					{
						if (mineLevel == 110)
						{
							chestItem.Add(ItemRegistry.Create("(B)514", 1, 0, false));
						}
					}
					else
					{
						chestItem.Add(new Object("434", 1, false, -1, 0));
					}
				}
				else if (mineLevel <= 220)
				{
					if (mineLevel != 120)
					{
						if (mineLevel == 220)
						{
							if (Game1.player.secretNotesSeen.Contains(10) && !Game1.player.mailReceived.Contains("qiCave"))
							{
								Game1.eventUp = true;
								Game1.displayHUD = false;
								Game1.player.CanMove = false;
								Game1.player.showNotCarrying();
								this.currentEvent = new Event(Game1.content.LoadString((MineShaft.numberOfCraftedStairsUsedThisRun <= 10) ? "Data\\ExtraDialogue:SkullCavern_100_event_honorable" : "Data\\ExtraDialogue:SkullCavern_100_event"), null);
								this.currentEvent.exitLocation = new LocationRequest(base.Name, false, this);
								Game1.player.chestConsumedMineLevels[this.mineLevel] = true;
							}
							else
							{
								force_treasure_room = true;
							}
						}
					}
					else
					{
						Game1.player.completeQuest("18");
						Game1.player.stats.checkForMineAchievement(true, true);
						if (!Game1.player.hasSkullKey)
						{
							Game1.player.chestConsumedMineLevels.Remove(120);
							chestItem.Add(new SpecialItem(4, ""));
							tint = Color.Pink;
						}
					}
				}
				else if (mineLevel == 320 || mineLevel == 420)
				{
					force_treasure_room = true;
				}
			}
			if (this.netIsTreasureRoom.Value || force_treasure_room)
			{
				chestItem.Add(MineShaft.getTreasureRoomItem());
			}
			if (this.mineLevel == 320)
			{
				chestSpot.X += 1f;
			}
			if (chestItem.Count > 0 && !Game1.player.chestConsumedMineLevels.ContainsKey(this.mineLevel))
			{
				this.overlayObjects[chestSpot] = new Chest(chestItem, chestSpot, false, 0, false)
				{
					Tint = tint
				};
				if (this.getMineArea(-1) == 121 && force_treasure_room)
				{
					(this.overlayObjects[chestSpot] as Chest).SetBigCraftableSpriteIndex(344, -1, 3);
				}
			}
			if (this.mineLevel == 320 || this.mineLevel == 420)
			{
				this.overlayObjects[chestSpot + new Vector2(-2f, 0f)] = new Chest(new List<Item>
				{
					MineShaft.getTreasureRoomItem()
				}, chestSpot + new Vector2(-2f, 0f), false, 0, false)
				{
					Tint = new Color(255, 210, 200)
				};
				(this.overlayObjects[chestSpot + new Vector2(-2f, 0f)] as Chest).SetBigCraftableSpriteIndex(344, -1, 3);
			}
			if (this.mineLevel == 420)
			{
				this.overlayObjects[chestSpot + new Vector2(2f, 0f)] = new Chest(new List<Item>
				{
					MineShaft.getTreasureRoomItem()
				}, chestSpot + new Vector2(2f, 0f), false, 0, false)
				{
					Tint = new Color(216, 255, 240)
				};
				(this.overlayObjects[chestSpot + new Vector2(2f, 0f)] as Chest).SetBigCraftableSpriteIndex(344, -1, 3);
			}
		}

		// Token: 0x0600317C RID: 12668 RVA: 0x0027491A File Offset: 0x00272B1A
		private bool isForcedChestLevel(int level)
		{
			return level == 220 || level == 320 || level == 420;
		}

		// Token: 0x0600317D RID: 12669 RVA: 0x00274938 File Offset: 0x00272B38
		public static Item getTreasureRoomItem()
		{
			if (Game1.player.stats.Get(StatKeys.Mastery(0)) > 0U && Game1.random.NextDouble() < 0.02)
			{
				return ItemRegistry.Create("(O)GoldenAnimalCracker", 1, 0, false);
			}
			if (Trinket.CanSpawnTrinket(Game1.player) && Game1.random.NextDouble() < 0.045)
			{
				return Trinket.GetRandomTrinket();
			}
			switch (Game1.random.Next(26))
			{
			case 0:
				return ItemRegistry.Create("(O)288", 5, 0, false);
			case 1:
				return ItemRegistry.Create("(O)287", 10, 0, false);
			case 2:
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked") || Game1.random.NextDouble() >= 0.66)
				{
					return ItemRegistry.Create("(O)275", 5, 0, false);
				}
				return ItemRegistry.Create("(O)848", 5 + Game1.random.Next(1, 4) * 5, 0, false);
			case 3:
				return ItemRegistry.Create("(O)773", Game1.random.Next(2, 5), 0, false);
			case 4:
				return ItemRegistry.Create("(O)749", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), 0, false);
			case 5:
				return ItemRegistry.Create("(O)688", 5, 0, false);
			case 6:
				return ItemRegistry.Create("(O)681", Game1.random.Next(1, 4), 0, false);
			case 7:
				return ItemRegistry.Create("(O)" + Game1.random.Next(628, 634).ToString(), 1, 0, false);
			case 8:
				return ItemRegistry.Create("(O)645", Game1.random.Next(1, 3), 0, false);
			case 9:
				return ItemRegistry.Create("(O)621", 4, 0, false);
			case 10:
				if (Game1.random.NextDouble() >= 0.33)
				{
					return ItemRegistry.Create("(O)" + Game1.random.Next(472, 499).ToString(), Game1.random.Next(1, 5) * 5, 0, false);
				}
				return ItemRegistry.Create("(O)802", 15, 0, false);
			case 11:
				return ItemRegistry.Create("(O)286", 15, 0, false);
			case 12:
				if (Game1.random.NextDouble() >= 0.5)
				{
					return ItemRegistry.Create("(O)437", 1, 0, false);
				}
				return ItemRegistry.Create("(O)265", 1, 0, false);
			case 13:
				return ItemRegistry.Create("(O)439", 1, 0, false);
			case 14:
				if (Game1.random.NextDouble() >= 0.33)
				{
					return ItemRegistry.Create("(O)349", Game1.random.Next(2, 5), 0, false);
				}
				return ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.5) ? 226 : 732).ToString(), 5, 0, false);
			case 15:
				return ItemRegistry.Create("(O)337", Game1.random.Next(2, 4), 0, false);
			case 16:
				if (Game1.random.NextDouble() >= 0.33)
				{
					return ItemRegistry.Create("(O)" + Game1.random.Next(235, 245).ToString(), 5, 0, false);
				}
				return ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.5) ? 226 : 732).ToString(), 5, 0, false);
			case 17:
				return ItemRegistry.Create("(O)74", 1, 0, false);
			case 18:
				return ItemRegistry.Create("(BC)21", 1, 0, false);
			case 19:
				return ItemRegistry.Create("(BC)25", 1, 0, false);
			case 20:
				return ItemRegistry.Create("(BC)165", 1, 0, false);
			case 21:
				return ItemRegistry.Create(Game1.random.NextBool() ? "(H)38" : "(H)37", 1, 0, false);
			case 22:
				if (Game1.player.mailReceived.Contains("sawQiPlane"))
				{
					return ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) > 0U) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 5, 0, false);
				}
				return ItemRegistry.Create("(O)749", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0), 0, false);
			case 23:
				return ItemRegistry.Create("(H)65", 1, 0, false);
			case 24:
				return ItemRegistry.Create("(BC)272", 1, 0, false);
			case 25:
				return ItemRegistry.Create("(H)83", 1, 0, false);
			default:
				return ItemRegistry.Create("(O)288", 5, 0, false);
			}
		}

		// Token: 0x0600317E RID: 12670 RVA: 0x00274E00 File Offset: 0x00273000
		public static Item getSpecialItemForThisMineLevel(int level, int x, int y)
		{
			Random r = Utility.CreateRandom((double)level, Game1.stats.DaysPlayed, (double)x, (double)y * 9999.0, 0.0);
			if (Game1.mine == null)
			{
				return ItemRegistry.Create("(O)388", 1, 0, false);
			}
			if (Game1.mine.GetAdditionalDifficulty() > 0)
			{
				if (r.NextDouble() < 0.02)
				{
					return ItemRegistry.Create("(BC)272", 1, 0, false);
				}
				switch (r.Next(7))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)61", 1, 0, false), r, false, null);
				case 1:
					return ItemRegistry.Create("(O)910", 1, 0, false);
				case 2:
					return ItemRegistry.Create("(O)913", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(O)915", 1, 0, false);
				case 4:
					return new Ring("527");
				case 5:
					return ItemRegistry.Create("(O)858", 1, 0, false);
				case 6:
				{
					Item treasureRoomItem = MineShaft.getTreasureRoomItem();
					treasureRoomItem.Stack = 1;
					return treasureRoomItem;
				}
				}
			}
			if (level < 20)
			{
				switch (r.Next(6))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)16", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)24", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)504", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)505", 1, 0, false);
				case 4:
					return new Ring("516");
				case 5:
					return new Ring("518");
				}
			}
			else if (level < 40)
			{
				switch (r.Next(7))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)22", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)24", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)504", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)505", 1, 0, false);
				case 4:
					return new Ring("516");
				case 5:
					return new Ring("518");
				case 6:
					return ItemRegistry.Create("(W)15", 1, 0, false);
				}
			}
			else if (level < 60)
			{
				switch (r.Next(7))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)6", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)26", 1, 0, false), r, false, null);
				case 2:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)15", 1, 0, false), r, false, null);
				case 3:
					return ItemRegistry.Create("(B)510", 1, 0, false);
				case 4:
					return new Ring("517");
				case 5:
					return new Ring("519");
				case 6:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)27", 1, 0, false), r, false, null);
				}
			}
			else if (level < 80)
			{
				switch (r.Next(7))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)26", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)27", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)508", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)510", 1, 0, false);
				case 4:
					return new Ring("517");
				case 5:
					return new Ring("519");
				case 6:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)19", 1, 0, false), r, false, null);
				}
			}
			else if (level < 100)
			{
				switch (r.Next(8))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)48", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)48", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)511", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)513", 1, 0, false);
				case 4:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18", 1, 0, false), r, false, null);
				case 5:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)28", 1, 0, false), r, false, null);
				case 6:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)52", 1, 0, false), r, false, null);
				case 7:
				{
					MeleeWeapon meleeWeapon = (MeleeWeapon)MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)3", 1, 0, false), r, false, null);
					meleeWeapon.AddEnchantment(new CrusaderEnchantment());
					return meleeWeapon;
				}
				}
			}
			else if (level < 120)
			{
				switch (r.Next(8))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)19", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)50", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)511", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)513", 1, 0, false);
				case 4:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18", 1, 0, false), r, false, null);
				case 5:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)46", 1, 0, false), r, false, null);
				case 6:
					return new Ring("887");
				case 7:
				{
					MeleeWeapon meleeWeapon2 = (MeleeWeapon)MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)3", 1, 0, false), r, false, null);
					meleeWeapon2.AddEnchantment(new CrusaderEnchantment());
					return meleeWeapon2;
				}
				}
			}
			else
			{
				switch (r.Next(12))
				{
				case 0:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)45", 1, 0, false), r, false, null);
				case 1:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)50", 1, 0, false), r, false, null);
				case 2:
					return ItemRegistry.Create("(B)511", 1, 0, false);
				case 3:
					return ItemRegistry.Create("(B)513", 1, 0, false);
				case 4:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18", 1, 0, false), r, false, null);
				case 5:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)28", 1, 0, false), r, false, null);
				case 6:
					return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)52", 1, 0, false), r, false, null);
				case 7:
					return ItemRegistry.Create("(O)787", 1, 0, false);
				case 8:
					return ItemRegistry.Create("(B)878", 1, 0, false);
				case 9:
					return ItemRegistry.Create("(O)856", 1, 0, false);
				case 10:
					return new Ring("859");
				case 11:
					return new Ring("887");
				}
			}
			return new Object("78", 1, false, -1, 0);
		}

		// Token: 0x0600317F RID: 12671 RVA: 0x0027548C File Offset: 0x0027368C
		public override bool IsLocationSpecificOccupantOnTile(Vector2 tileLocation)
		{
			return this.tileBeneathLadder.Equals(tileLocation) || (this.tileBeneathElevator != Vector2.Zero && this.tileBeneathElevator.Equals(tileLocation)) || base.IsLocationSpecificOccupantOnTile(tileLocation);
		}

		// Token: 0x06003180 RID: 12672 RVA: 0x002754D8 File Offset: 0x002736D8
		public bool isDarkArea()
		{
			return (this.loadedDarkArea || this.mineLevel % 40 > 30) && this.getMineArea(-1) != 40;
		}

		// Token: 0x06003181 RID: 12673 RVA: 0x00275500 File Offset: 0x00273700
		public bool isTileClearForMineObjects(Vector2 v)
		{
			if (this.tileBeneathLadder.Equals(v) || this.tileBeneathElevator.Equals(v))
			{
				return false;
			}
			if (!this.CanItemBePlacedHere(v, false, CollisionMask.All, CollisionMask.None, false, false))
			{
				return false;
			}
			if (this.IsTileOccupiedBy(v, CollisionMask.Characters, CollisionMask.None, false))
			{
				return false;
			}
			if (this.IsTileOccupiedBy(v, CollisionMask.Flooring | CollisionMask.TerrainFeatures, CollisionMask.None, false))
			{
				return false;
			}
			string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back", false);
			return s != null && s.Equals("Stone") && this.isTileOnClearAndSolidGround(v) && !this.objects.ContainsKey(v) && !Utility.PointToVector2(this.calicoStatueSpot.Value).Equals(v);
		}

		// Token: 0x06003182 RID: 12674 RVA: 0x002755CD File Offset: 0x002737CD
		public override string getFootstepSoundReplacement(string footstep)
		{
			if (this.GetAdditionalDifficulty() > 0 && this.getMineArea(-1) == 40 && this.mineLevel % 40 < 30 && footstep == "stoneStep")
			{
				return "grassyStep";
			}
			return base.getFootstepSoundReplacement(footstep);
		}

		// Token: 0x06003183 RID: 12675 RVA: 0x0027560C File Offset: 0x0027380C
		public bool isTileOnClearAndSolidGround(Vector2 v)
		{
			return base.hasTileAt((int)v.X, (int)v.Y, "Back", null) && !base.hasTileAt((int)v.X, (int)v.Y, "Front", null) && !base.hasTileAt((int)v.X, (int)v.Y, "Buildings", null) && base.getTileIndexAt((int)v.X, (int)v.Y, "Back", "mine") != 77;
		}

		// Token: 0x06003184 RID: 12676 RVA: 0x00275694 File Offset: 0x00273894
		public bool isTileOnClearAndSolidGround(int x, int y)
		{
			return base.hasTileAt(x, y, "Back", null) && !base.hasTileAt(x, y, "Front", null) && base.getTileIndexAt(x, y, "Back", "mine") != 77;
		}

		// Token: 0x06003185 RID: 12677 RVA: 0x002756D1 File Offset: 0x002738D1
		public bool isTileClearForMineObjects(int x, int y)
		{
			return this.isTileClearForMineObjects(new Vector2((float)x, (float)y));
		}

		// Token: 0x06003186 RID: 12678 RVA: 0x002756E4 File Offset: 0x002738E4
		public void loadLevel(int level)
		{
			this.forceFirstTime = false;
			this.hasAddedDesertFestivalStatue = false;
			this.isMonsterArea = false;
			this.isSlimeArea = false;
			this.loadedDarkArea = false;
			this.isQuarryArea = false;
			this.isDinoArea = false;
			this.mineLoader.Unload();
			this.mineLoader.Dispose();
			this.mineLoader = Game1.content.CreateTemporary();
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.IsMasterGame && level > MineShaft.deepestLevelOnCurrentDesertFestivalRun && this.getMineArea(-1) == 121)
			{
				if (level % 5 == 0)
				{
					NetInt calicoEggSkullCavernRating = Game1.player.team.calicoEggSkullCavernRating;
					int value = calicoEggSkullCavernRating.Value;
					calicoEggSkullCavernRating.Value = value + 1;
				}
				MineShaft.deepestLevelOnCurrentDesertFestivalRun = level;
			}
			bool preventMonsterLevel = false;
			int mapNumberToLoad = -1;
			if (this.forceLayout != null)
			{
				mapNumberToLoad = this.forceLayout.Value;
				string assetName = "Maps\\Mines\\" + mapNumberToLoad.ToString();
				if (!this.mapContent.DoesAssetExist<Map>(assetName))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Can't force mine layout to ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(mapNumberToLoad);
					defaultInterpolatedStringHandler.AppendLiteral(" because there's no '");
					defaultInterpolatedStringHandler.AppendFormatted(assetName);
					defaultInterpolatedStringHandler.AppendLiteral("' asset, falling back to default logic.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					mapNumberToLoad = -1;
				}
			}
			if (mapNumberToLoad < 0)
			{
				mapNumberToLoad = ((level % 40 % 20 == 0 && level % 40 != 0) ? 20 : ((level % 10 == 0) ? 10 : level));
				mapNumberToLoad %= 40;
				if (level == 120)
				{
					mapNumberToLoad = 120;
				}
				if (this.getMineArea(level) == 121)
				{
					MineShaft lastLevel = null;
					foreach (MineShaft mine in MineShaft.activeMines)
					{
						if (mine != null && mine.mineLevel > 120 && mine.mineLevel < level && (lastLevel == null || mine.mineLevel > lastLevel.mineLevel))
						{
							lastLevel = mine;
						}
					}
					mapNumberToLoad = this.mineRandom.Next(40);
					for (;;)
					{
						int num = mapNumberToLoad;
						int? num2 = (lastLevel != null) ? new int?(lastLevel.loadedMapNumber) : null;
						if (!(num == num2.GetValueOrDefault() & num2 != null))
						{
							break;
						}
						mapNumberToLoad = this.mineRandom.Next(40);
					}
					while (mapNumberToLoad % 5 == 0)
					{
						mapNumberToLoad = this.mineRandom.Next(40);
					}
					if (this.isForcedChestLevel(level))
					{
						mapNumberToLoad = 10;
					}
					else if (level >= 130)
					{
						double chance = 0.01;
						chance += Game1.player.team.AverageDailyLuck(this) / 10.0 + Game1.player.team.AverageLuckLevel(this) / 100.0;
						if (Game1.random.NextDouble() < chance)
						{
							this.netIsTreasureRoom.Value = true;
							mapNumberToLoad = 10;
						}
					}
				}
				else if (this.getMineArea(-1) == 77377 && this.mineLevel == 77377)
				{
					mapNumberToLoad = 77377;
				}
				if (MineShaft.lowestLevelReached >= 120 && mapNumberToLoad != 10 && mapNumberToLoad % 5 != 0 && this.mineLevel > 1 && this.mineLevel != 77377)
				{
					Random alt_random = Utility.CreateDaySaveRandom((double)(1293857 + this.mineLevel * 400), 0.0, 0.0);
					double chance2 = 0.06;
					if (this.mineLevel > 120)
					{
						chance2 += Math.Min(0.06, (double)this.mineLevel / 10000.0);
					}
					if (alt_random.NextDouble() < chance2)
					{
						int[] source = new int[]
						{
							40,
							47,
							50,
							51
						};
						mapNumberToLoad = alt_random.Next(40, 61);
						if (source.Contains(mapNumberToLoad) && alt_random.NextDouble() < 0.75)
						{
							mapNumberToLoad = alt_random.Next(40, 61);
						}
						if (mapNumberToLoad == 53 && this.getMineArea(-1) != 121)
						{
							mapNumberToLoad = alt_random.Next(52, 61);
						}
						if (mapNumberToLoad == 40 && this.getMineArea(-1) != 0 && this.getMineArea(-1) != 80)
						{
							mapNumberToLoad = alt_random.Next(52, 61);
						}
						if (source.Contains(mapNumberToLoad))
						{
							preventMonsterLevel = true;
						}
					}
				}
			}
			this.mapPath.Value = "Maps\\Mines\\" + mapNumberToLoad.ToString();
			this.loadedMapNumber = mapNumberToLoad;
			this.updateMap();
			Random r = Utility.CreateDaySaveRandom((double)(level * 100), 0.0, 0.0);
			if ((!this.AnyOnlineFarmerHasBuff("23") || this.getMineArea(-1) == 121) && r.NextDouble() < 0.044 && mapNumberToLoad % 5 != 0 && mapNumberToLoad % 40 > 5 && mapNumberToLoad % 40 < 30 && mapNumberToLoad % 40 != 19 && !preventMonsterLevel)
			{
				if (r.NextBool())
				{
					this.isMonsterArea = true;
				}
				else
				{
					this.isSlimeArea = true;
				}
				if (this.getMineArea(-1) == 121 && this.mineLevel > 126 && r.NextBool())
				{
					this.isDinoArea = true;
					this.isSlimeArea = false;
					this.isMonsterArea = false;
				}
			}
			else if (this.mineLevel < 121 && r.NextDouble() < 0.044 && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom") && Game1.MasterPlayer.hasOrWillReceiveMail("VisitedQuarryMine") && mapNumberToLoad % 40 > 1 && mapNumberToLoad % 5 != 0)
			{
				this.isQuarryArea = true;
				if (r.NextDouble() < 0.25 && !preventMonsterLevel)
				{
					this.isMonsterArea = true;
				}
			}
			if (this.isQuarryArea || this.getMineArea(level) == 77377)
			{
				this.mapImageSource.Value = "Maps\\Mines\\mine_quarryshaft";
				int numBrownSpots = this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight / 100;
				this.isQuarryArea = true;
				this.isSlimeArea = false;
				this.isMonsterArea = false;
				this.isDinoArea = false;
				for (int i = 0; i < numBrownSpots; i++)
				{
					this.brownSpots.Add(new Vector2((float)this.mineRandom.Next(0, this.map.Layers[0].LayerWidth), (float)this.mineRandom.Next(0, this.map.Layers[0].LayerHeight)));
				}
			}
			else if (this.isDinoArea)
			{
				this.mapImageSource.Value = "Maps\\Mines\\mine_dino";
			}
			else if (this.isSlimeArea)
			{
				this.mapImageSource.Value = "Maps\\Mines\\mine_slime";
			}
			else if (this.getMineArea(-1) == 0 || this.getMineArea(-1) == 10 || (this.getMineArea(level) != 0 && this.getMineArea(level) != 10))
			{
				if (this.getMineArea(level) == 40)
				{
					this.mapImageSource.Value = "Maps\\Mines\\mine_frost";
					if (level >= 70)
					{
						NetString netString = this.mapImageSource;
						netString.Value += "_dark";
						this.loadedDarkArea = true;
					}
				}
				else if (this.getMineArea(level) == 80)
				{
					this.mapImageSource.Value = "Maps\\Mines\\mine_lava";
					if (level >= 110 && level != 120)
					{
						NetString netString2 = this.mapImageSource;
						netString2.Value += "_dark";
						this.loadedDarkArea = true;
					}
				}
				else if (this.getMineArea(level) == 121)
				{
					this.mapImageSource.Value = "Maps\\Mines\\mine_desert";
					if (mapNumberToLoad % 40 >= 30)
					{
						NetString netString3 = this.mapImageSource;
						netString3.Value += "_dark";
						this.loadedDarkArea = true;
					}
				}
			}
			if (mapNumberToLoad == 45)
			{
				this.loadedDarkArea = true;
				if (this.mapImageSource.Value == null)
				{
					this.mapImageSource.Value = "Maps\\Mines\\mine_dark";
				}
				else if (!this.mapImageSource.Value.EndsWith("dark"))
				{
					NetString netString4 = this.mapImageSource;
					netString4.Value += "_dark";
				}
			}
			if (this.GetAdditionalDifficulty() > 0)
			{
				string map_image_source = "Maps\\Mines\\mine";
				if (this.mapImageSource.Value != null)
				{
					map_image_source = this.mapImageSource.Value;
				}
				if (map_image_source.EndsWith("_dark"))
				{
					map_image_source = map_image_source.Remove(map_image_source.Length - "_dark".Length);
				}
				string base_map_image_source = map_image_source;
				if (level % 40 >= 30)
				{
					this.loadedDarkArea = true;
				}
				if (this.loadedDarkArea)
				{
					map_image_source += "_dark";
				}
				map_image_source += "_dangerous";
				try
				{
					this.mapImageSource.Value = map_image_source;
					Game1.temporaryContent.Load<Texture2D>(this.mapImageSource.Value);
				}
				catch (ContentLoadException)
				{
					map_image_source = base_map_image_source + "_dangerous";
					try
					{
						this.mapImageSource.Value = map_image_source;
						Game1.temporaryContent.Load<Texture2D>(this.mapImageSource.Value);
					}
					catch (ContentLoadException)
					{
						map_image_source = base_map_image_source;
						if (this.loadedDarkArea)
						{
							map_image_source += "_dark";
						}
						try
						{
							this.mapImageSource.Value = map_image_source;
							Game1.temporaryContent.Load<Texture2D>(this.mapImageSource.Value);
						}
						catch (ContentLoadException)
						{
							this.mapImageSource.Value = base_map_image_source;
						}
					}
				}
			}
			this.ApplyDiggableTileFixes();
			if (!this.isSideBranch(-1))
			{
				MineShaft.lowestLevelReached = Math.Max(MineShaft.lowestLevelReached, level);
				if (this.mineLevel % 5 == 0 && this.getMineArea(-1) != 121)
				{
					this.prepareElevator();
				}
			}
		}

		// Token: 0x06003187 RID: 12679 RVA: 0x00276088 File Offset: 0x00274288
		private void addBlueFlamesToChallengeShrine()
		{
			TemporaryAnimatedSpriteList temporarySprites = this.temporarySprites;
			TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), false, 0f, Color.White);
			temporaryAnimatedSprite.interval = 50f;
			temporaryAnimatedSprite.totalNumberOfLoops = 99999;
			temporaryAnimatedSprite.animationLength = 4;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Mines_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
			defaultInterpolatedStringHandler.AppendLiteral("_ChallengeShrineFlames_1");
			temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
			temporaryAnimatedSprite.id = 888;
			temporaryAnimatedSprite.lightRadius = 2f;
			temporaryAnimatedSprite.scale = 4f;
			temporaryAnimatedSprite.yPeriodic = true;
			temporaryAnimatedSprite.lightcolor = new Color(100, 0, 0);
			temporaryAnimatedSprite.yPeriodicLoopTime = 1000f;
			temporaryAnimatedSprite.yPeriodicRange = 4f;
			temporaryAnimatedSprite.layerDepth = 0.04544f;
			temporarySprites.Add(temporaryAnimatedSprite);
			TemporaryAnimatedSpriteList temporarySprites2 = this.temporarySprites;
			TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), false, 0f, Color.White);
			temporaryAnimatedSprite2.interval = 50f;
			temporaryAnimatedSprite2.totalNumberOfLoops = 99999;
			temporaryAnimatedSprite2.animationLength = 4;
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Mines_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
			defaultInterpolatedStringHandler.AppendLiteral("_ChallengeShrineFlames_2");
			temporaryAnimatedSprite2.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
			temporaryAnimatedSprite2.id = 889;
			temporaryAnimatedSprite2.lightRadius = 2f;
			temporaryAnimatedSprite2.scale = 4f;
			temporaryAnimatedSprite2.lightcolor = new Color(100, 0, 0);
			temporaryAnimatedSprite2.yPeriodic = true;
			temporaryAnimatedSprite2.yPeriodicLoopTime = 1100f;
			temporaryAnimatedSprite2.yPeriodicRange = 4f;
			temporaryAnimatedSprite2.layerDepth = 0.04544f;
			temporarySprites2.Add(temporaryAnimatedSprite2);
			Game1.playSound("fireball", null);
		}

		// Token: 0x06003188 RID: 12680 RVA: 0x002762C8 File Offset: 0x002744C8
		public static void CheckForQiChallengeCompletion()
		{
			if (Game1.player.deepestMineLevel >= 145 && Game1.player.hasQuest("20") && !Game1.player.hasOrWillReceiveMail("QiChallengeComplete"))
			{
				Game1.player.completeQuest("20");
				Game1.addMailForTomorrow("QiChallengeComplete", false, false);
			}
		}

		// Token: 0x06003189 RID: 12681 RVA: 0x00276324 File Offset: 0x00274524
		private void prepareElevator()
		{
			Point elevatorSpot = Utility.findTile(this, 80, "Buildings", "mine");
			this.ElevatorLightSpot = elevatorSpot;
			if (elevatorSpot.X >= 0)
			{
				if (this.canAdd(3, 0))
				{
					this.elevatorShouldDing.Value = true;
					this.updateMineLevelData(3, 1);
					return;
				}
				base.setMapTile(elevatorSpot.X, elevatorSpot.Y, 48, "Buildings", "mine", null, true);
			}
		}

		// Token: 0x0600318A RID: 12682 RVA: 0x00276394 File Offset: 0x00274594
		public void enterMineShaft()
		{
			DelayedAction.playSoundAfterDelay("fallDown", 800, this, null, -1, false);
			DelayedAction.playSoundAfterDelay("clubSmash", 1800, null, null, -1, false);
			Random random = Utility.CreateRandom((double)this.mineLevel, Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, 0.0, 0.0);
			int levelsDown = random.Next(3, 9);
			if (random.NextDouble() < 0.1)
			{
				levelsDown = levelsDown * 2 - 1;
			}
			if (this.mineLevel < 220 && this.mineLevel + levelsDown > 220)
			{
				levelsDown = 220 - this.mineLevel;
			}
			this.lastLevelsDownFallen = levelsDown;
			Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * 3);
			this.isFallingDownShaft = true;
			Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterFall), 0.045f);
			Game1.player.CanMove = false;
			Game1.player.jump();
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.player.flashDuringThisTemporaryInvincibility = false;
			Game1.player.currentTemporaryInvincibilityDuration = 700;
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.IsMasterGame && this.lastLevelsDownFallen + this.mineLevel > MineShaft.deepestLevelOnCurrentDesertFestivalRun && this.isFallingDownShaft && (this.lastLevelsDownFallen + this.mineLevel) / 5 > this.mineLevel / 5)
			{
				Game1.player.team.calicoEggSkullCavernRating.Value += (this.lastLevelsDownFallen + this.mineLevel) / 5 - this.mineLevel / 5;
			}
		}

		// Token: 0x0600318B RID: 12683 RVA: 0x00276554 File Offset: 0x00274754
		private void afterFall()
		{
			Game1.drawObjectDialogue(Game1.content.LoadString((this.lastLevelsDownFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", this.lastLevelsDownFallen));
			Game1.messagePause = true;
			Game1.enterMine(this.mineLevel + this.lastLevelsDownFallen, null);
			Game1.fadeToBlackAlpha = 1f;
			Game1.player.faceDirection(2);
			Game1.player.showFrame(5, false);
		}

		// Token: 0x0600318C RID: 12684 RVA: 0x002765D2 File Offset: 0x002747D2
		public override bool ShouldExcludeFromNpcPathfinding()
		{
			return true;
		}

		// Token: 0x0600318D RID: 12685 RVA: 0x002765D8 File Offset: 0x002747D8
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (who.IsLocalPlayer)
			{
				int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "mine");
				if (tileIndexAt <= 173)
				{
					if (tileIndexAt != 112)
					{
						if (tileIndexAt == 115)
						{
							Response[] options = new Response[]
							{
								new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")).SetHotKey(Keys.Y),
								new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
							};
							base.createQuestionDialogue(" ", options, "ExitMine");
							return true;
						}
						if (tileIndexAt == 173)
						{
							Game1.enterMine(this.mineLevel + 1, null);
							base.playSound("stairsdown", null, null, SoundContext.Default);
							return true;
						}
					}
					else if (this.mineLevel <= 120)
					{
						Game1.activeClickableMenu = new MineElevatorMenu();
						return true;
					}
				}
				else if (tileIndexAt <= 194)
				{
					if (tileIndexAt == 174)
					{
						Response[] options2 = new Response[]
						{
							new Response("Jump", Game1.content.LoadString("Strings\\Locations:Mines_ShaftJumpIn")).SetHotKey(Keys.Y),
							new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
						};
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Mines_Shaft"), options2, "Shaft");
						return true;
					}
					if (tileIndexAt == 194)
					{
						base.playSound("openBox", null, null, SoundContext.Default);
						base.playSound("Ship", null, null, SoundContext.Default);
						Tile tile = this.map.RequireLayer("Buildings").Tiles[tileLocation];
						int tileIndex = tile.TileIndex;
						tile.TileIndex = tileIndex + 1;
						Tile tile2 = this.map.RequireLayer("Front").Tiles[tileLocation.X, tileLocation.Y - 1];
						tileIndex = tile2.TileIndex;
						tile2.TileIndex = tileIndex + 1;
						Game1.createRadialDebris(this, 382, tileLocation.X, tileLocation.Y, 6, false, -1, true, null);
						this.updateMineLevelData(2, -1);
						return true;
					}
				}
				else if (tileIndexAt != 284)
				{
					if (tileIndexAt - 315 <= 2)
					{
						if (Game1.player.team.SpecialOrderRuleActive("MINE_HARD", null) || Game1.player.team.specialRulesRemovedToday.Contains("MINE_HARD"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_OnQiChallenge"));
						}
						else if (Game1.player.team.toggleMineShrineOvernight.Value)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyActive"));
						}
						else
						{
							base.createQuestionDialogue(Game1.player.team.mineShrineActivated.Value ? Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyHard") : Game1.content.LoadString("Strings\\Locations:ChallengeShrine_NotYetHard"), base.createYesNoResponses(), "ShrineOfChallenge");
						}
					}
				}
				else if (this.mineLevel > 120 && this.mineLevel != 77377)
				{
					this.recentlyActivatedCalicoStatue.Value = new Point(tileLocation.X, tileLocation.Y);
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x0600318E RID: 12686 RVA: 0x00276954 File Offset: 0x00274B54
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (this.isQuarryArea)
			{
				return "";
			}
			if (Game1.random.NextDouble() < 0.15)
			{
				string objectId = "(O)330";
				if (Game1.random.NextDouble() < 0.07)
				{
					if (Game1.random.NextDouble() < 0.75)
					{
						switch (Game1.random.Next(5))
						{
						case 0:
							objectId = "(O)96";
							break;
						case 1:
							objectId = (who.hasOrWillReceiveMail("lostBookFound") ? ((Game1.netWorldState.Value.LostBooksFound < 21) ? "(O)102" : "(O)770") : "(O)770");
							break;
						case 2:
							objectId = "(O)110";
							break;
						case 3:
							objectId = "(O)112";
							break;
						case 4:
							objectId = "(O)585";
							break;
						}
					}
					else if (Game1.random.NextDouble() < 0.75)
					{
						int mineArea = this.getMineArea(-1);
						if (mineArea <= 10)
						{
							if (mineArea == 0 || mineArea == 10)
							{
								objectId = Game1.random.Choose("(O)121", "(O)97");
							}
						}
						else if (mineArea != 40)
						{
							if (mineArea == 80)
							{
								objectId = "(O)99";
							}
						}
						else
						{
							objectId = Game1.random.Choose("(O)122", "(O)336");
						}
					}
					else
					{
						objectId = Game1.random.Choose("(O)126", "(O)127");
					}
				}
				else if (Game1.random.NextDouble() < 0.19)
				{
					objectId = (Game1.random.NextBool() ? "(O)390" : this.getOreIdForLevel(this.mineLevel, Game1.random));
				}
				else if (Game1.random.NextDouble() < 0.45)
				{
					objectId = "(O)330";
				}
				else if (Game1.random.NextDouble() < 0.12)
				{
					if (Game1.random.NextDouble() < 0.25)
					{
						objectId = "(O)749";
					}
					else
					{
						int mineArea = this.getMineArea(-1);
						if (mineArea <= 10)
						{
							if (mineArea == 0 || mineArea == 10)
							{
								objectId = "(O)535";
							}
						}
						else if (mineArea != 40)
						{
							if (mineArea == 80)
							{
								objectId = "(O)537";
							}
						}
						else
						{
							objectId = "(O)536";
						}
					}
				}
				else
				{
					objectId = "(O)78";
				}
				Game1.createObjectDebris(objectId, xLocation, yLocation, who.UniqueMultiplayerID, this);
				bool flag = ((who != null) ? who.CurrentTool : null) is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
				float generousChance = 0.25f;
				if (flag && Game1.random.NextDouble() < (double)generousChance)
				{
					Game1.createObjectDebris(objectId, xLocation, yLocation, who.UniqueMultiplayerID, this);
				}
				return "";
			}
			return "";
		}

		// Token: 0x0600318F RID: 12687 RVA: 0x00276C1C File Offset: 0x00274E1C
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			foreach (NPC npc in this.characters)
			{
				Monster monster = npc as Monster;
				if (monster != null)
				{
					monster.drawAboveAllLayers(b);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (this.fogAlpha > 0f || this.ambientFog)
			{
				Vector2 v = default(Vector2);
				for (float x = (float)(-256 + (int)(this.fogPos.X % 256f)); x < (float)Game1.graphics.GraphicsDevice.Viewport.Width; x += 256f)
				{
					for (float y = (float)(-256 + (int)(this.fogPos.Y % 256f)); y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 256f)
					{
						v.X = (float)((int)x);
						v.Y = (float)((int)y);
						b.Draw(Game1.mouseCursors, v, new Microsoft.Xna.Framework.Rectangle?(this.fogSource), (this.fogAlpha > 0f) ? (this.fogColor * this.fogAlpha) : this.fogColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (!Game1.game1.takingMapScreenshot && !this.isSideBranch(-1))
			{
				Color col = (this.getMineArea(-1) == 0 || (this.isDarkArea() && this.getMineArea(-1) != 121)) ? SpriteText.color_White : ((this.getMineArea(-1) == 10) ? SpriteText.color_Green : ((this.getMineArea(-1) == 40) ? SpriteText.color_Cyan : ((this.getMineArea(-1) == 80) ? SpriteText.color_Red : SpriteText.color_Purple)));
				string txt = (this.mineLevel + ((this.getMineArea(-1) == 121) ? -120 : 0)).ToString() ?? "";
				Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
				int height = SpriteText.getHeightOfString(txt, 999999);
				SpriteText.drawString(b, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, height, 1f, 1f, false, 2, "", new Color?(col), SpriteText.ScrollTextAlignment.Left);
				int text_width = SpriteText.getWidthOfString(txt, 999999);
				if (this.mustKillAllMonstersToAdvance())
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(tsarea.Left + 16 + text_width + 16), (float)(tsarea.Top + 16)) + new Vector2(4f, 6f) * 4f, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(192, 324, 7, 10)), Color.White, 0f, new Vector2(3f, 5f), 4f + Game1.dialogueButtonScale / 25f, SpriteEffects.None, 1f);
				}
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
				{
					int buffs = 0;
					foreach (IClickableMenu clickableMenu in Game1.onScreenMenus)
					{
						BuffsDisplay _bd = clickableMenu as BuffsDisplay;
						if (_bd != null)
						{
							buffs = _bd.getNumBuffs();
						}
					}
					Vector2 eggPos = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width - 300f * ((float)Game1.graphics.GraphicsDevice.Viewport.Width / (float)Game1.uiViewport.Width) - 100f, (float)(tsarea.Top + 64 + 16 + (buffs - 1) / 5 * 16 * 4)) + new Vector2(4f, 6f) * 4f;
					if (this.calicoEggIconTimerShake > 0f)
					{
						eggPos += new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-4, 5));
						b.DrawString(Game1.dialogueFont, "+1", eggPos + new Vector2(eggPos.X - 32f, eggPos.Y + 32f), Color.White);
					}
					b.Draw(Game1.mouseCursors_1_6, eggPos, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 19, 21)), Color.White, 0f, new Vector2(3f, 5f), 4f, SpriteEffects.None, 1f);
					SpriteText.drawString(b, (Game1.player.team.calicoEggSkullCavernRating.Value + 1).ToString() ?? "", (int)eggPos.X + 28 - SpriteText.getWidthOfString((Game1.player.team.calicoEggSkullCavernRating.Value + 1).ToString() ?? "", 999999) / 2, (int)eggPos.Y + 4, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
			}
		}

		// Token: 0x06003190 RID: 12688 RVA: 0x002771CC File Offset: 0x002753CC
		public override void checkForMusic(GameTime time)
		{
			if (Game1.player.freezePause > 0 || this.isFogUp.Value)
			{
				return;
			}
			if (this.mineLevel == 120)
			{
				return;
			}
			string trackName = null;
			int mineArea = this.getMineArea(-1);
			if (mineArea <= 40)
			{
				if (mineArea != 0 && mineArea != 10)
				{
					if (mineArea != 40)
					{
						goto IL_6C;
					}
					trackName = "Frost_Ambient";
					goto IL_6C;
				}
			}
			else
			{
				if (mineArea == 80)
				{
					trackName = "Lava_Ambient";
					goto IL_6C;
				}
				if (mineArea != 121 && mineArea != 77377)
				{
					goto IL_6C;
				}
			}
			trackName = "Upper_Ambient";
			IL_6C:
			if (this.GetAdditionalDifficulty() > 0 && this.getMineArea(-1) == 40 && this.mineLevel < 70)
			{
				trackName = "jungle_ambience";
			}
			if (Game1.getMusicTrackName(MusicContext.Default) == "none" || Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) || (Game1.getMusicTrackName(MusicContext.Default).EndsWith("_Ambient") && Game1.getMusicTrackName(MusicContext.Default) != trackName))
			{
				Game1.changeMusicTrack(trackName, false, MusicContext.Default);
			}
			MineShaft.timeSinceLastMusic = Math.Min(335000, MineShaft.timeSinceLastMusic + time.ElapsedGameTime.Milliseconds);
		}

		// Token: 0x06003191 RID: 12689 RVA: 0x002772D0 File Offset: 0x002754D0
		public string getMineSong()
		{
			if (this.mineLevel < 40)
			{
				return "EarthMine";
			}
			if (this.mineLevel < 80)
			{
				return "FrostMine";
			}
			if (this.getMineArea(-1) != 121)
			{
				return "LavaMine";
			}
			if (Game1.random.NextDouble() < 0.75)
			{
				return "LavaMine";
			}
			return "EarthMine";
		}

		// Token: 0x06003192 RID: 12690 RVA: 0x0027732E File Offset: 0x0027552E
		public int GetAdditionalDifficulty()
		{
			if (this.mineLevel == 77377)
			{
				return 0;
			}
			if (this.mineLevel > 120)
			{
				return Game1.netWorldState.Value.SkullCavesDifficulty;
			}
			return Game1.netWorldState.Value.MinesDifficulty;
		}

		// Token: 0x06003193 RID: 12691 RVA: 0x00277368 File Offset: 0x00275568
		public bool isPlayingSongFromDifferentArea()
		{
			return Game1.getMusicTrackName(MusicContext.Default) != this.getMineSong() && Game1.getMusicTrackName(MusicContext.Default).EndsWith("Mine");
		}

		// Token: 0x06003194 RID: 12692 RVA: 0x00277390 File Offset: 0x00275590
		public void playMineSong()
		{
			string track_for_area = this.getMineSong();
			if ((Game1.getMusicTrackName(MusicContext.Default) == "none" || Game1.isMusicContextActiveButNotPlaying(MusicContext.Default) || Game1.getMusicTrackName(MusicContext.Default).Contains("Ambient")) && !this.isDarkArea() && this.mineLevel != 77377)
			{
				Game1.changeMusicTrack(track_for_area, false, MusicContext.Default);
				MineShaft.timeSinceLastMusic = 0;
			}
		}

		// Token: 0x06003195 RID: 12693 RVA: 0x002773F4 File Offset: 0x002755F4
		protected override void resetLocalState()
		{
			this.addLevelChests();
			base.resetLocalState();
			if (Game1.IsPlayingBackgroundMusic)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			if (this.elevatorShouldDing.Value)
			{
				this.timeUntilElevatorLightUp = 1500;
			}
			else if (this.mineLevel % 5 == 0 && this.getMineArea(-1) != 121)
			{
				this.setElevatorLit();
			}
			if (!this.isSideBranch(this.mineLevel))
			{
				Game1.player.deepestMineLevel = Math.Max(Game1.player.deepestMineLevel, this.mineLevel);
				if (Game1.player.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						Action<Farmer, int> onMineFloorReached = specialOrder.onMineFloorReached;
						if (onMineFloorReached != null)
						{
							onMineFloorReached(Game1.player, this.mineLevel);
						}
					}
				}
				Game1.player.autoGenerateActiveDialogueEvent("mineArea_" + this.getMineArea(-1).ToString(), 4);
			}
			if (this.mineLevel == 77377)
			{
				Game1.addMailForTomorrow("VisitedQuarryMine", true, true);
			}
			if (this.getMineArea(-1) == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(10) && !Game1.player.hasBuff("CalicoStatueSpeed"))
			{
				DesertFestival.addCalicoStatueSpeedBuff();
			}
			MineShaft.CheckForQiChallengeCompletion();
			int num;
			if (this.mineLevel == 120)
			{
				Farmer player = Game1.player;
				num = player.timesReachedMineBottom + 1;
				player.timesReachedMineBottom = num;
			}
			Vector2 vector = this.mineEntrancePosition(Game1.player);
			Game1.xLocationAfterWarp = (int)vector.X;
			Game1.yLocationAfterWarp = (int)vector.Y;
			if (Game1.IsClient)
			{
				Game1.player.Position = new Vector2((float)(Game1.xLocationAfterWarp * 64), (float)(Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16));
			}
			this.forceViewportPlayerFollow = true;
			num = this.mineLevel;
			if (num != 20)
			{
				if (num == 120)
				{
					if (this.GetAdditionalDifficulty() > 0 && !Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
					{
						Game1.addMailForTomorrow("reachedBottomOfHardMines", true, true);
					}
					if (this.GetAdditionalDifficulty() > 0)
					{
						Game1.getAchievement(41, true);
					}
					if (Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
					{
						base.setMapTile(9, 6, 315, "Buildings", "mine", "None", true);
						base.setMapTile(10, 6, 316, "Buildings", "mine", "None", true);
						base.setMapTile(11, 6, 317, "Buildings", "mine", "None", true);
						base.setMapTile(9, 5, 299, "Front", "mine", null, true);
						base.setMapTile(10, 5, 300, "Front", "mine", null, true);
						base.setMapTile(11, 5, 301, "Front", "mine", null, true);
						if ((Game1.player.team.mineShrineActivated.Value && !Game1.player.team.toggleMineShrineOvernight.Value) || (!Game1.player.team.mineShrineActivated.Value && Game1.player.team.toggleMineShrineOvernight.Value))
						{
							DelayedAction.functionAfterDelay(new Action(this.addBlueFlamesToChallengeShrine), 1000);
						}
					}
				}
			}
			else if (!Game1.IsMultiplayer && base.IsRainingHere() && Game1.player.eventsSeen.Contains("901756"))
			{
				this.characters.Clear();
				NPC a = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 32), new Vector2(896f, 644f), "SeedShop", 3, "AbigailMine", true, Game1.content.Load<Texture2D>("Portraits\\Abigail"))
				{
					displayName = NPC.GetDisplayName("Abigail")
				};
				Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, 0.0, 0.0, 0.0, 0.0);
				if (Game1.player.mailReceived.Add("AbigailInMineFirst"))
				{
					a.setNewDialogue("Strings\\Characters:AbigailInMineFirst", false, false);
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				else if (r.NextDouble() < 0.15)
				{
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(16, 500),
						new FarmerSprite.AnimationFrame(17, 500),
						new FarmerSprite.AnimationFrame(18, 500),
						new FarmerSprite.AnimationFrame(19, 500)
					});
					a.setNewDialogue("Strings\\Characters:AbigailInMineFlute", false, false);
					Game1.changeMusicTrack("AbigailFlute", false, MusicContext.Default);
				}
				else
				{
					a.setNewDialogue("Strings\\Characters:AbigailInMine" + r.Next(5).ToString(), false, false);
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				this.characters.Add(a);
			}
			this.ApplyDiggableTileFixes();
			if (this.isMonsterArea || this.isSlimeArea)
			{
				Random r2 = Utility.CreateRandom(Game1.stats.DaysPlayed, 0.0, 0.0, 0.0, 0.0);
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:Mines_" + r2.Choose("Infested", "Overrun")));
			}
			bool flag = this.mineLevel % 20 == 0;
			bool foundAnyWater = false;
			if (flag)
			{
				this.waterTiles = new WaterTiles(this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
				this.waterColor.Value = ((this.getMineArea(-1) == 80) ? (Color.Red * 0.8f) : (new Color(50, 100, 200) * 0.5f));
				for (int y = 0; y < this.map.RequireLayer("Buildings").LayerHeight; y++)
				{
					for (int x = 0; x < this.map.RequireLayer("Buildings").LayerWidth; x++)
					{
						string water_property = this.doesTileHaveProperty(x, y, "Water", "Back", false);
						if (water_property != null)
						{
							foundAnyWater = true;
							if (water_property == "I")
							{
								this.waterTiles.waterTiles[x, y] = new WaterTiles.WaterTileData(true, false);
							}
							else
							{
								this.waterTiles[x, y] = true;
							}
							if (this.getMineArea(-1) == 80 && Game1.random.NextDouble() < 0.1)
							{
								NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights = this.sharedLights;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Mines_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(this.mineLevel);
								defaultInterpolatedStringHandler.AppendLiteral("_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(x);
								defaultInterpolatedStringHandler.AppendLiteral("_");
								defaultInterpolatedStringHandler.AppendFormatted<int>(y);
								defaultInterpolatedStringHandler.AppendLiteral("_Lava");
								sharedLights.AddLight(new LightSource(defaultInterpolatedStringHandler.ToStringAndClear(), 4, new Vector2((float)x, (float)y) * 64f, 2f, new Color(0, 220, 220), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
							}
						}
					}
				}
			}
			if (!foundAnyWater)
			{
				this.waterTiles = null;
			}
			if (this.getMineArea(this.mineLevel) != this.getMineArea(this.mineLevel - 1) || this.mineLevel == 120 || this.isPlayingSongFromDifferentArea())
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			if (this.GetAdditionalDifficulty() > 0 && this.mineLevel == 70)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			if (this.mineLevel == 77377 && Game1.player.mailReceived.Contains("gotGoldenScythe"))
			{
				base.setMapTile(29, 4, 245, "Front", "mine", null, true);
				base.setMapTile(30, 4, 246, "Front", "mine", null, true);
				base.setMapTile(29, 5, 261, "Front", "mine", null, true);
				base.setMapTile(30, 5, 262, "Front", "mine", null, true);
				base.setMapTile(29, 6, 277, "Buildings", "mine", null, true);
				base.setMapTile(30, 56, 278, "Buildings", "mine", null, true);
			}
			if (this.calicoStatueSpot.Value != Point.Zero)
			{
				if (this.recentlyActivatedCalicoStatue.Value != Point.Zero)
				{
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y, 285, "Buildings", "mine", null, true);
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y - 1, 269, "Front", "mine", null, true);
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y - 2, 253, "Front", "mine", null, true);
				}
				else
				{
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y, 284, "Buildings", "mine", null, true);
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y - 1, 268, "Front", "mine", null, true);
					base.setMapTile(this.calicoStatueSpot.X, this.calicoStatueSpot.Y - 2, 252, "Front", "mine", null, true);
				}
			}
			if (this.mineLevel > 1 && (this.mineLevel == 2 || (this.mineLevel % 5 != 0 && MineShaft.timeSinceLastMusic > 150000 && Game1.random.NextBool())))
			{
				this.playMineSong();
			}
		}

		// Token: 0x06003196 RID: 12694 RVA: 0x00277EFC File Offset: 0x002760FC
		public virtual void ApplyDiggableTileFixes()
		{
			if (this.map == null)
			{
				return;
			}
			if (this.GetAdditionalDifficulty() <= 0 || this.getMineArea(-1) == 40 || !this.isDarkArea())
			{
				TileSheet tileSheet = this.map.RequireTileSheet(0, "mine");
				tileSheet.TileIndexProperties[165].TryAdd("Diggable", "true");
				tileSheet.TileIndexProperties[181].TryAdd("Diggable", "true");
				tileSheet.TileIndexProperties[183].TryAdd("Diggable", "true");
			}
		}

		// Token: 0x06003197 RID: 12695 RVA: 0x00277FA0 File Offset: 0x002761A0
		public void createLadderDown(int x, int y, bool forceShaft = false)
		{
			this.createLadderDownEvent[new Point(x, y)] = (forceShaft || (this.getMineArea(-1) == 121 && !this.mustKillAllMonstersToAdvance() && this.mineRandom.NextDouble() < 0.2));
		}

		// Token: 0x06003198 RID: 12696 RVA: 0x00277FF4 File Offset: 0x002761F4
		private void doCreateLadderDown(Point point, bool shaft)
		{
			this.updateMap();
			int x = point.X;
			int y = point.Y;
			Layer layer = this.map.RequireLayer("Buildings");
			TileSheet tileSheet = this.map.RequireTileSheet(0, "mine");
			if (shaft)
			{
				layer.Tiles[x, y] = new StaticTile(layer, tileSheet, BlendMode.Alpha, 174);
			}
			else
			{
				this.ladderHasSpawned = true;
				layer.Tiles[x, y] = new StaticTile(layer, tileSheet, BlendMode.Alpha, 173);
			}
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64));
			}
		}

		// Token: 0x06003199 RID: 12697 RVA: 0x002780A8 File Offset: 0x002762A8
		public void checkStoneForItems(string stoneId, int x, int y, Farmer who)
		{
			long farmerId = (who != null) ? who.UniqueMultiplayerID : 0L;
			int farmerLuckLevel = (who != null) ? who.LuckLevel : 0;
			double num = (who != null) ? who.DailyLuck : 0.0;
			int farmerMiningLevel = (who != null) ? who.MiningLevel : 0;
			double chanceModifier = num / 2.0 + (double)farmerMiningLevel * 0.005 + (double)farmerLuckLevel * 0.001;
			Random r = Utility.CreateDaySaveRandom((double)(x * 1000), (double)y, (double)this.mineLevel);
			r.NextDouble();
			double oreModifier = (stoneId == 40.ToString() || stoneId == 42.ToString()) ? 1.2 : 0.8;
			int stonesLeftOnThisLevel = this.stonesLeftOnThisLevel;
			this.stonesLeftOnThisLevel = stonesLeftOnThisLevel - 1;
			double chanceForLadderDown = 0.02 + 1.0 / (double)Math.Max(1, this.stonesLeftOnThisLevel) + (double)farmerLuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
			if (this.EnemyCount == 0)
			{
				chanceForLadderDown += 0.04;
			}
			if (who != null && who.hasBuff("dwarfStatue_1"))
			{
				chanceForLadderDown *= 1.25;
			}
			if (!this.ladderHasSpawned && !this.mustKillAllMonstersToAdvance() && (this.stonesLeftOnThisLevel == 0 || r.NextDouble() < chanceForLadderDown) && this.shouldCreateLadderOnThisLevel())
			{
				this.createLadderDown(x, y, false);
			}
			if (this.breakStone(stoneId, x, y, who, r))
			{
				return;
			}
			if (stoneId == 44.ToString())
			{
				int whichGem = r.Next(59, 70);
				whichGem += whichGem % 2;
				bool reachedBottom = false;
				using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.timesReachedMineBottom > 0)
						{
							reachedBottom = true;
							break;
						}
					}
				}
				if (!reachedBottom)
				{
					if (this.mineLevel < 40 && whichGem != 66 && whichGem != 68)
					{
						whichGem = r.Choose(66, 68);
					}
					else if (this.mineLevel < 80 && (whichGem == 64 || whichGem == 60))
					{
						whichGem = r.Choose(66, 70, 68, 62);
					}
				}
				Game1.createObjectDebris("(O)" + whichGem.ToString(), x, y, farmerId, this);
				Stats stats = Game1.stats;
				uint otherPreciousGemsFound = stats.OtherPreciousGemsFound;
				stats.OtherPreciousGemsFound = otherPreciousGemsFound + 1U;
				return;
			}
			int excavatorMultiplier = (who != null && who.professions.Contains(22)) ? 2 : 1;
			double dwarfStatueMultiplier = (who != null && who.hasBuff("dwarfStatue_4")) ? 1.25 : 1.0;
			if (r.NextDouble() < 0.022 * (1.0 + chanceModifier) * (double)excavatorMultiplier * dwarfStatueMultiplier)
			{
				string id = "(O)" + (535 + ((this.getMineArea(-1) == 40) ? 1 : ((this.getMineArea(-1) == 80) ? 2 : 0))).ToString();
				if (this.getMineArea(-1) == 121)
				{
					id = "(O)749";
				}
				if (who != null && who.professions.Contains(19) && r.NextBool())
				{
					Game1.createObjectDebris(id, x, y, farmerId, this);
				}
				Game1.createObjectDebris(id, x, y, farmerId, this);
				if (who != null)
				{
					who.gainExperience(5, 20 * this.getMineArea(-1));
				}
			}
			if (this.mineLevel > 20 && r.NextDouble() < 0.005 * (1.0 + chanceModifier) * (double)excavatorMultiplier * dwarfStatueMultiplier)
			{
				if (who != null && who.professions.Contains(19) && r.NextBool())
				{
					Game1.createObjectDebris("(O)749", x, y, farmerId, this);
				}
				Game1.createObjectDebris("(O)749", x, y, farmerId, this);
				if (who != null)
				{
					who.gainExperience(5, 40 * this.getMineArea(-1));
				}
			}
			if (r.NextDouble() < 0.05 * (1.0 + chanceModifier) * oreModifier)
			{
				int burrowerMultiplier = (who != null && who.professions.Contains(21)) ? 2 : 1;
				double addedCoalChance = (who != null && who.hasBuff("dwarfStatue_2")) ? 0.1 : 0.0;
				if (r.NextDouble() < 0.25 * (double)burrowerMultiplier + addedCoalChance)
				{
					Game1.createObjectDebris("(O)382", x, y, farmerId, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite(25, new Vector2((float)(64 * x), (float)(64 * y)), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128, 0)
					});
				}
				Game1.createObjectDebris(this.getOreIdForLevel(this.mineLevel, r), x, y, farmerId, this);
				if (who != null)
				{
					who.gainExperience(3, 5);
					return;
				}
			}
			else if (r.NextBool())
			{
				Game1.createDebris(14, x, y, 1, this);
			}
		}

		// Token: 0x0600319A RID: 12698 RVA: 0x002785D4 File Offset: 0x002767D4
		public string getOreIdForLevel(int mineLevel, Random r)
		{
			if (this.getMineArea(mineLevel) == 77377)
			{
				return "(O)380";
			}
			if (mineLevel < 40)
			{
				if (mineLevel >= 20 && r.NextDouble() < 0.1)
				{
					return "(O)380";
				}
				return "(O)378";
			}
			else if (mineLevel < 80)
			{
				if (mineLevel >= 60 && r.NextDouble() < 0.1)
				{
					return "(O)384";
				}
				if (r.NextDouble() >= 0.75)
				{
					return "(O)378";
				}
				return "(O)380";
			}
			else if (mineLevel < 120)
			{
				if (r.NextDouble() < 0.75)
				{
					return "(O)384";
				}
				if (r.NextDouble() >= 0.75)
				{
					return "(O)378";
				}
				return "(O)380";
			}
			else
			{
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && r.NextDouble() < 0.13 + (double)((float)(Game1.player.team.calicoEggSkullCavernRating.Value * 5) / 1000f))
				{
					return "CalicoEgg";
				}
				if (r.NextDouble() < 0.01 + (double)((float)(mineLevel - 120) / 2000f))
				{
					return "(O)386";
				}
				if (r.NextDouble() < 0.75)
				{
					return "(O)384";
				}
				if (r.NextDouble() >= 0.75)
				{
					return "(O)378";
				}
				return "(O)380";
			}
		}

		// Token: 0x0600319B RID: 12699 RVA: 0x00278730 File Offset: 0x00276930
		public bool shouldUseSnowTextureHoeDirt()
		{
			return !this.isSlimeArea && ((this.GetAdditionalDifficulty() > 0 && (this.mineLevel < 40 || (this.mineLevel >= 70 && this.mineLevel < 80))) || (this.GetAdditionalDifficulty() <= 0 && this.getMineArea(-1) == 40));
		}

		// Token: 0x0600319C RID: 12700 RVA: 0x00278788 File Offset: 0x00276988
		public int getMineArea(int level = -1)
		{
			if (level == -1)
			{
				level = this.mineLevel;
			}
			if (this.isQuarryArea || level == 77377)
			{
				return 77377;
			}
			if (level >= 80 && level <= 120)
			{
				return 80;
			}
			if (level > 120)
			{
				return 121;
			}
			if (level >= 40)
			{
				return 40;
			}
			if (level > 10 && this.mineLevel < 30)
			{
				return 10;
			}
			return 0;
		}

		// Token: 0x0600319D RID: 12701 RVA: 0x002787E7 File Offset: 0x002769E7
		public bool isSideBranch(int level = -1)
		{
			if (level == -1)
			{
				level = this.mineLevel;
			}
			return level == 77377;
		}

		// Token: 0x0600319E RID: 12702 RVA: 0x002787FD File Offset: 0x002769FD
		public byte getWallAt(int x, int y)
		{
			return byte.MaxValue;
		}

		// Token: 0x0600319F RID: 12703 RVA: 0x00278804 File Offset: 0x00276A04
		public Color getLightingColor(GameTime time)
		{
			return this.lighting;
		}

		// Token: 0x060031A0 RID: 12704 RVA: 0x0027880C File Offset: 0x00276A0C
		public Object getRandomItemForThisLevel(int level, Vector2 tile)
		{
			string id = "80";
			if (this.mineRandom.NextDouble() < 0.05 && level > 80)
			{
				id = "422";
			}
			else if (this.mineRandom.NextDouble() < 0.1 && level > 20 && this.getMineArea(-1) != 40)
			{
				id = "420";
			}
			else if (this.mineRandom.NextDouble() < 0.25 || this.GetAdditionalDifficulty() > 0)
			{
				int mineArea = this.getMineArea(-1);
				if (mineArea <= 10)
				{
					if (mineArea == 0 || mineArea == 10)
					{
						if (this.GetAdditionalDifficulty() > 0 && !this.isDarkArea())
						{
							switch (this.mineRandom.Next(6))
							{
							case 0:
							case 6:
								id = "152";
								break;
							case 1:
								id = "393";
								break;
							case 2:
								id = "397";
								break;
							case 3:
								id = "372";
								break;
							case 4:
								id = "392";
								break;
							}
							if (this.mineRandom.NextDouble() < 0.005)
							{
								id = "797";
							}
							else if (this.mineRandom.NextDouble() < 0.08)
							{
								id = "394";
							}
						}
						else
						{
							id = "86";
						}
					}
				}
				else if (mineArea != 40)
				{
					if (mineArea != 80)
					{
						if (mineArea == 121)
						{
							id = ((this.mineRandom.NextDouble() < 0.3) ? "86" : ((this.mineRandom.NextDouble() < 0.3) ? "84" : "82"));
						}
					}
					else
					{
						id = "82";
					}
				}
				else if (this.GetAdditionalDifficulty() > 0 && this.mineLevel % 40 < 30)
				{
					switch (this.mineRandom.Next(4))
					{
					case 0:
					case 3:
						id = "259";
						break;
					case 1:
						id = "404";
						break;
					case 2:
						id = "420";
						break;
					}
					if (this.mineRandom.NextDouble() < 0.08)
					{
						id = "422";
					}
				}
				else
				{
					id = "84";
				}
			}
			else
			{
				id = "80";
			}
			if (this.isDinoArea)
			{
				id = "259";
				if (this.mineRandom.NextDouble() < 0.06)
				{
					id = "107";
				}
			}
			return new Object(id, 1, false, -1, 0)
			{
				IsSpawnedObject = true
			};
		}

		// Token: 0x060031A1 RID: 12705 RVA: 0x00278A8F File Offset: 0x00276C8F
		public bool shouldShowDarkHoeDirt()
		{
			return this.getMineArea(-1) != 121 || this.isDinoArea;
		}

		// Token: 0x060031A2 RID: 12706 RVA: 0x00278AA8 File Offset: 0x00276CA8
		public string getRandomGemRichStoneForThisLevel(int level)
		{
			int whichGem = this.mineRandom.Next(59, 70);
			whichGem += whichGem % 2;
			if (Game1.player.timesReachedMineBottom == 0)
			{
				if (level < 40 && whichGem != 66 && whichGem != 68)
				{
					whichGem = this.mineRandom.Choose(66, 68);
				}
				else if (level < 80 && (whichGem == 64 || whichGem == 60))
				{
					whichGem = this.mineRandom.Choose(66, 70, 68, 62);
				}
			}
			switch (whichGem)
			{
			case 60:
				return "12";
			case 62:
				return "14";
			case 64:
				return "4";
			case 66:
				return "8";
			case 68:
				return "10";
			case 70:
				return "6";
			}
			return 40.ToString();
		}

		// Token: 0x060031A3 RID: 12707 RVA: 0x00278B80 File Offset: 0x00276D80
		public float getDistanceFromStart(int xTile, int yTile)
		{
			float distance = Utility.distance((float)xTile, this.tileBeneathLadder.X, (float)yTile, this.tileBeneathLadder.Y);
			if (this.tileBeneathElevator != Vector2.Zero)
			{
				distance = Math.Min(distance, Utility.distance((float)xTile, this.tileBeneathElevator.X, (float)yTile, this.tileBeneathElevator.Y));
			}
			return distance;
		}

		// Token: 0x060031A4 RID: 12708 RVA: 0x00278BE8 File Offset: 0x00276DE8
		public Monster getMonsterForThisLevel(int level, int xTile, int yTile)
		{
			Vector2 position = new Vector2((float)xTile, (float)yTile) * 64f;
			float distanceFromLadder = this.getDistanceFromStart(xTile, yTile);
			if (this.isSlimeArea)
			{
				if (this.GetAdditionalDifficulty() > 0)
				{
					if (this.mineLevel < 20)
					{
						return new GreenSlime(position, this.mineLevel);
					}
					if (this.mineLevel < 30)
					{
						return new BlueSquid(position);
					}
					if (this.mineLevel < 40)
					{
						return new RockGolem(position, this);
					}
					if (this.mineLevel < 50)
					{
						if (this.mineRandom.NextDouble() < 0.15 && distanceFromLadder >= 10f)
						{
							return new Fly(position);
						}
						return new Grub(position);
					}
					else if (this.mineLevel < 70)
					{
						return new Leaper(position);
					}
				}
				else
				{
					if (this.mineRandom.NextDouble() < 0.2)
					{
						return new BigSlime(position, this.getMineArea(-1));
					}
					return new GreenSlime(position, this.mineLevel);
				}
			}
			else if (this.isDinoArea)
			{
				if (this.mineRandom.NextDouble() < 0.1)
				{
					return new Bat(position, 999);
				}
				if (this.mineRandom.NextDouble() < 0.1)
				{
					return new Fly(position, true);
				}
				return new DinoMonster(position);
			}
			if (this.getMineArea(-1) == 0 || this.getMineArea(-1) == 10)
			{
				if (this.mineRandom.NextDouble() < 0.25 && !this.mustKillAllMonstersToAdvance())
				{
					return new Bug(position, this.mineRandom.Next(4), this);
				}
				if (level < 15)
				{
					if (this.doesTileHaveProperty(xTile, yTile, "Diggable", "Back", false) != null)
					{
						return new Duggy(position);
					}
					if (this.mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(position);
					}
					return new GreenSlime(position, level);
				}
				else if (level <= 30)
				{
					if (this.doesTileHaveProperty(xTile, yTile, "Diggable", "Back", false) != null)
					{
						return new Duggy(position);
					}
					if (this.mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(position);
					}
					if (this.mineRandom.NextDouble() < 0.05 && distanceFromLadder > 10f && this.GetAdditionalDifficulty() <= 0)
					{
						return new Fly(position);
					}
					if (this.mineRandom.NextDouble() < 0.45)
					{
						return new GreenSlime(position, level);
					}
					if (this.GetAdditionalDifficulty() <= 0)
					{
						return new Grub(position);
					}
					if (distanceFromLadder > 9f)
					{
						return new BlueSquid(position);
					}
					if (this.mineRandom.NextDouble() < 0.01)
					{
						return new RockGolem(position, this);
					}
					return new GreenSlime(position, level);
				}
				else if (level <= 40)
				{
					if (this.mineRandom.NextDouble() < 0.1 && distanceFromLadder > 10f)
					{
						return new Bat(position, level);
					}
					if (this.GetAdditionalDifficulty() > 0 && this.mineRandom.NextDouble() < 0.1)
					{
						return new Ghost(position, "Carbon Ghost");
					}
					return new RockGolem(position, this);
				}
			}
			else if (this.getMineArea(-1) == 40)
			{
				if (this.mineLevel >= 70 && (this.mineRandom.NextDouble() < 0.75 || this.GetAdditionalDifficulty() > 0))
				{
					if (this.mineRandom.NextDouble() < 0.75 || this.GetAdditionalDifficulty() <= 0)
					{
						return new Skeleton(position, this.GetAdditionalDifficulty() > 0 && this.mineRandom.NextBool());
					}
					return new Bat(position, 77377);
				}
				else
				{
					if (this.mineRandom.NextDouble() < 0.3)
					{
						return new DustSpirit(position, this.mineRandom.NextDouble() < 0.8);
					}
					if (this.mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
					{
						return new Bat(position, this.mineLevel);
					}
					if (!this.ghostAdded && this.mineLevel > 50 && this.mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
					{
						this.ghostAdded = true;
						if (this.GetAdditionalDifficulty() > 0)
						{
							return new Ghost(position, "Putrid Ghost");
						}
						return new Ghost(position);
					}
					else if (this.GetAdditionalDifficulty() > 0)
					{
						if (this.mineRandom.NextDouble() < 0.01)
						{
							RockCrab rockCrab = new RockCrab(position);
							rockCrab.makeStickBug();
							return rockCrab;
						}
						if (this.mineLevel >= 50)
						{
							return new Leaper(position);
						}
						if (this.mineRandom.NextDouble() < 0.7)
						{
							return new Grub(position);
						}
						return new GreenSlime(position, this.mineLevel);
					}
				}
			}
			else if (this.getMineArea(-1) == 80)
			{
				if (this.isDarkArea() && this.mineRandom.NextDouble() < 0.25)
				{
					return new Bat(position, this.mineLevel);
				}
				if (this.mineRandom.NextDouble() < ((this.GetAdditionalDifficulty() > 0) ? 0.05 : 0.15))
				{
					return new GreenSlime(position, this.getMineArea(-1));
				}
				if (this.mineRandom.NextDouble() < 0.15)
				{
					return new MetalHead(position, this.getMineArea(-1));
				}
				if (this.mineRandom.NextDouble() < 0.25)
				{
					return new ShadowBrute(position);
				}
				if (this.GetAdditionalDifficulty() > 0 && this.mineRandom.NextDouble() < 0.25)
				{
					return new Shooter(position, "Shadow Sniper");
				}
				if (this.mineRandom.NextDouble() < 0.25)
				{
					return new ShadowShaman(position);
				}
				if (this.mineRandom.NextDouble() < 0.25)
				{
					return new RockCrab(position, "Lava Crab");
				}
				if (this.mineRandom.NextDouble() < 0.2 && distanceFromLadder > 8f && this.mineLevel >= 90 && base.hasTileAt(xTile, yTile, "Back", null) && !base.hasTileAt(xTile, yTile, "Front", null))
				{
					return new SquidKid(position);
				}
			}
			else if (this.getMineArea(-1) == 121)
			{
				if (this.loadedDarkArea)
				{
					if (this.mineRandom.NextDouble() < 0.18 && distanceFromLadder > 8f)
					{
						return new Ghost(position, "Carbon Ghost");
					}
					Mummy mummy = new Mummy(position);
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(9))
					{
						mummy.BuffForAdditionalDifficulty(2);
						mummy.speed *= 2;
						this.setMonsterTextureToDangerousVersion(mummy);
					}
					return mummy;
				}
				else
				{
					if (this.mineLevel % 20 == 0 && distanceFromLadder > 10f)
					{
						return new Bat(position, this.mineLevel);
					}
					if (this.mineLevel % 16 == 0 && !this.mustKillAllMonstersToAdvance())
					{
						if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(4))
						{
							return new Bug(position, this.mineRandom.Next(4), "Assassin Bug");
						}
						return new Bug(position, this.mineRandom.Next(4), this);
					}
					else if (this.mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f)
					{
						if (this.GetAdditionalDifficulty() <= 0)
						{
							return new Serpent(position);
						}
						return new Serpent(position, "Royal Serpent");
					}
					else
					{
						if (this.mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f && this.mineLevel >= 171)
						{
							return new Bat(position, this.mineLevel);
						}
						if (this.mineLevel >= 126 && distanceFromLadder > 10f && this.mineRandom.NextDouble() < 0.04 && !this.mustKillAllMonstersToAdvance())
						{
							return new DinoMonster(position);
						}
						if (this.mineRandom.NextDouble() < 0.33 && !this.mustKillAllMonstersToAdvance())
						{
							if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.getMineArea(-1) == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(4))
							{
								return new Bug(position, this.mineRandom.Next(4), "Assassin Bug");
							}
							return new Bug(position, this.mineRandom.Next(4), this);
						}
						else
						{
							if (this.mineRandom.NextDouble() < 0.25)
							{
								return new GreenSlime(position, level);
							}
							if (this.mineLevel >= 146 && this.mineRandom.NextDouble() < 0.25)
							{
								return new RockCrab(position, "Iridium Crab");
							}
							if (this.GetAdditionalDifficulty() > 0 && this.mineRandom.NextDouble() < 0.2 && distanceFromLadder > 8f && base.hasTileAt(xTile, yTile, "Back", null) && !base.hasTileAt(xTile, yTile, "Front", null))
							{
								return new SquidKid(position);
							}
							return new BigSlime(position, this);
						}
					}
				}
			}
			else if (this.getMineArea(-1) == 77377)
			{
				if ((this.mineLevel == 77377 && yTile > 59) || (this.mineLevel != 77377 && this.mineLevel % 2 == 0))
				{
					GreenSlime slime = new GreenSlime(position, 77377);
					Vector2 tile = new Vector2((float)xTile, (float)yTile);
					bool brown = false;
					for (int i = 0; i < this.brownSpots.Count; i++)
					{
						if (Vector2.Distance(tile, this.brownSpots[i]) < 4f)
						{
							brown = true;
							break;
						}
					}
					if (brown)
					{
						int red = Game1.random.Next(120, 200);
						slime.color.Value = new Color(red, red / 2, red / 4);
						while (Game1.random.NextDouble() < 0.33)
						{
							slime.objectsToDrop.Add("378");
						}
						slime.Health = (int)((float)slime.Health * 0.5f);
						slime.Speed += 2;
					}
					else
					{
						int colorBase = Game1.random.Next(120, 200);
						slime.color.Value = new Color(colorBase, colorBase, colorBase);
						while (Game1.random.NextDouble() < 0.33)
						{
							slime.objectsToDrop.Add("380");
						}
						slime.Speed = 1;
					}
					return slime;
				}
				if (yTile >= 51 && this.mineLevel == 77377)
				{
					return new Bat(position, 77377)
					{
						focusedOnFarmers = true
					};
				}
				if (xTile >= 70)
				{
					Monster skel = new Skeleton(position, Game1.random.NextBool());
					skel.BuffForAdditionalDifficulty(this.mineRandom.Next(1, 3));
					this.setMonsterTextureToDangerousVersion(skel);
					return skel;
				}
				return new Bat(position, 77377);
			}
			return new GreenSlime(position, level);
		}

		// Token: 0x060031A5 RID: 12709 RVA: 0x002796C4 File Offset: 0x002778C4
		private Object createLitterObject(double chanceForPurpleStone, double chanceForMysticStone, double gemStoneChance, Vector2 tile)
		{
			Color stoneColor = Color.White;
			int stoneHealth = 1;
			if (this.GetAdditionalDifficulty() > 0 && this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < (double)this.GetAdditionalDifficulty() * 0.001 + (double)((float)this.mineLevel / 100000f) + Game1.player.team.AverageDailyLuck(this) / 13.0 + Game1.player.team.AverageLuckLevel(this) * 0.0001500000071246177)
			{
				return new Object("95", 1, false, -1, 0)
				{
					MinutesUntilReady = 25
				};
			}
			int whichStone;
			if (this.getMineArea(-1) == 0 || this.getMineArea(-1) == 10)
			{
				whichStone = this.mineRandom.Next(31, 42);
				if (this.mineLevel % 40 < 30 && whichStone >= 33 && whichStone < 38)
				{
					whichStone = this.mineRandom.Choose(32, 38);
				}
				else if (this.mineLevel % 40 >= 30)
				{
					whichStone = this.mineRandom.Choose(34, 36);
				}
				if (this.GetAdditionalDifficulty() > 0)
				{
					whichStone = this.mineRandom.Next(33, 37);
					stoneHealth = 5;
					if (Game1.random.NextDouble() < 0.33)
					{
						whichStone = 846;
					}
					else
					{
						stoneColor = new Color(Game1.random.Next(60, 90), Game1.random.Next(150, 200), Game1.random.Next(190, 240));
					}
					if (this.isDarkArea())
					{
						whichStone = this.mineRandom.Next(32, 39);
						int tone = Game1.random.Next(130, 160);
						stoneColor = new Color(tone, tone, tone);
					}
					if (this.mineLevel != 1 && this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
					{
						return new Object("849", 1, false, -1, 0)
						{
							MinutesUntilReady = 6
						};
					}
					if (stoneColor.Equals(Color.White))
					{
						return new Object(whichStone.ToString(), 1, false, -1, 0)
						{
							MinutesUntilReady = stoneHealth
						};
					}
				}
				else if (this.mineLevel != 1 && this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
				{
					return new Object("751", 1, false, -1, 0)
					{
						MinutesUntilReady = 3
					};
				}
			}
			else if (this.getMineArea(-1) == 40)
			{
				whichStone = this.mineRandom.Next(47, 54);
				stoneHealth = 3;
				if (this.GetAdditionalDifficulty() > 0 && this.mineLevel % 40 < 30)
				{
					whichStone = this.mineRandom.Next(39, 42);
					stoneHealth = 5;
					stoneColor = new Color(170, 255, 160);
					if (this.isDarkArea())
					{
						whichStone = this.mineRandom.Next(32, 39);
						int tone2 = Game1.random.Next(130, 160);
						stoneColor = new Color(tone2, tone2, tone2);
					}
					if (this.mineRandom.NextDouble() < 0.15)
					{
						return new ColoredObject((294 + this.mineRandom.Choose(1, 0)).ToString(), 1, new Color(170, 140, 155))
						{
							MinutesUntilReady = 6,
							CanBeSetDown = true,
							Flipped = this.mineRandom.NextBool()
						};
					}
					if (this.mineLevel != 1 && this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
					{
						return new ColoredObject("290", 1, new Color(150, 225, 160))
						{
							MinutesUntilReady = 6,
							CanBeSetDown = true,
							Flipped = this.mineRandom.NextBool()
						};
					}
					if (stoneColor.Equals(Color.White))
					{
						return new Object(whichStone.ToString(), 1, false, -1, 0)
						{
							MinutesUntilReady = stoneHealth
						};
					}
				}
				else if (this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
				{
					return new Object("290", 1, false, -1, 0)
					{
						MinutesUntilReady = 4
					};
				}
			}
			else if (this.getMineArea(-1) == 80)
			{
				stoneHealth = 4;
				if (this.mineRandom.NextDouble() < 0.3 && !this.isDarkArea())
				{
					if (this.mineRandom.NextBool())
					{
						whichStone = 38;
					}
					else
					{
						whichStone = 32;
					}
				}
				else if (this.mineRandom.NextDouble() < 0.3)
				{
					whichStone = this.mineRandom.Next(55, 58);
				}
				else if (this.mineRandom.NextBool())
				{
					whichStone = 760;
				}
				else
				{
					whichStone = 762;
				}
				if (this.GetAdditionalDifficulty() > 0)
				{
					if (this.mineRandom.NextBool())
					{
						whichStone = 38;
					}
					else
					{
						whichStone = 32;
					}
					stoneHealth = 5;
					stoneColor = new Color(Game1.random.Next(140, 190), Game1.random.Next(90, 120), Game1.random.Next(210, 255));
					if (this.isDarkArea())
					{
						whichStone = this.mineRandom.Next(32, 39);
						int tone3 = Game1.random.Next(130, 160);
						stoneColor = new Color(tone3, tone3, tone3);
					}
					if (this.mineLevel != 1 && this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
					{
						return new Object("764", 1, false, -1, 0)
						{
							MinutesUntilReady = 7
						};
					}
					if (stoneColor.Equals(Color.White))
					{
						return new Object(whichStone.ToString(), 1, false, -1, 0)
						{
							MinutesUntilReady = stoneHealth
						};
					}
				}
				else if (this.mineLevel % 5 != 0 && this.mineRandom.NextDouble() < 0.029)
				{
					return new Object("764", 1, false, -1, 0)
					{
						MinutesUntilReady = 8
					};
				}
			}
			else if (this.getMineArea(-1) == 77377)
			{
				stoneHealth = 5;
				bool foundSomething = false;
				foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
				{
					if (this.objects.ContainsKey(v))
					{
						foundSomething = true;
						break;
					}
				}
				if (!foundSomething && this.mineRandom.NextDouble() < 0.45)
				{
					return null;
				}
				bool brownSpot = false;
				for (int i = 0; i < this.brownSpots.Count; i++)
				{
					if (Vector2.Distance(tile, this.brownSpots[i]) < 4f)
					{
						brownSpot = true;
						break;
					}
					if (Vector2.Distance(tile, this.brownSpots[i]) < 6f)
					{
						return null;
					}
				}
				if (tile.X > 50f)
				{
					whichStone = Game1.random.Choose(668, 670);
					if (this.mineRandom.NextDouble() < 0.09 + Game1.player.team.AverageDailyLuck(this) / 2.0)
					{
						return new Object(Game1.random.Choose("BasicCoalNode0", "BasicCoalNode1"), 1, false, -1, 0)
						{
							MinutesUntilReady = 5
						};
					}
					if (this.mineRandom.NextDouble() < 0.25)
					{
						return null;
					}
				}
				else if (brownSpot)
				{
					whichStone = this.mineRandom.Choose(32, 38);
					if (this.mineRandom.NextDouble() < 0.01)
					{
						return new Object("751", 1, false, -1, 0)
						{
							MinutesUntilReady = 3
						};
					}
				}
				else
				{
					whichStone = this.mineRandom.Choose(34, 36);
					if (this.mineRandom.NextDouble() < 0.01)
					{
						return new Object("290", 1, false, -1, 0)
						{
							MinutesUntilReady = 3
						};
					}
				}
				return new Object(whichStone.ToString(), 1, false, -1, 0)
				{
					MinutesUntilReady = stoneHealth
				};
			}
			else
			{
				stoneHealth = 5;
				if (this.mineRandom.NextBool())
				{
					if (this.mineRandom.NextBool())
					{
						whichStone = 38;
					}
					else
					{
						whichStone = 32;
					}
				}
				else if (this.mineRandom.NextBool())
				{
					whichStone = 40;
				}
				else
				{
					whichStone = 42;
				}
				int skullCavernMineLevel = this.mineLevel - 120;
				double chanceForOre = 0.02 + (double)skullCavernMineLevel * 0.0005;
				if (this.mineLevel >= 130)
				{
					chanceForOre += 0.01 * (double)((float)(Math.Min(100, skullCavernMineLevel) - 10) / 10f);
				}
				double iridiumBoost = 0.0;
				if (this.mineLevel >= 130)
				{
					iridiumBoost += 0.001 * (double)((float)(skullCavernMineLevel - 10) / 10f);
				}
				iridiumBoost = Math.Min(iridiumBoost, 0.004);
				if (skullCavernMineLevel > 100)
				{
					iridiumBoost += (double)skullCavernMineLevel / 1000000.0;
				}
				if (!this.netIsTreasureRoom.Value && this.mineRandom.NextDouble() < chanceForOre)
				{
					double chanceForIridium = (double)Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost);
					double chanceForGold = 0.01 + (double)(this.mineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
					double chanceForIron = Math.Min(0.5, 0.1 + (double)(this.mineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && this.mineRandom.NextBool(0.13 + (double)((float)(Game1.player.team.calicoEggSkullCavernRating.Value * 5) / 1000f)))
					{
						return new Object("CalicoEggStone_" + this.mineRandom.Next(3).ToString(), 1, false, -1, 0)
						{
							MinutesUntilReady = 8
						};
					}
					if (this.mineRandom.NextDouble() < chanceForIridium)
					{
						return new Object("765", 1, false, -1, 0)
						{
							MinutesUntilReady = 16
						};
					}
					if (this.mineRandom.NextDouble() < chanceForGold)
					{
						return new Object("764", 1, false, -1, 0)
						{
							MinutesUntilReady = 8
						};
					}
					if (this.mineRandom.NextDouble() < chanceForIron)
					{
						return new Object("290", 1, false, -1, 0)
						{
							MinutesUntilReady = 4
						};
					}
					return new Object("751", 1, false, -1, 0)
					{
						MinutesUntilReady = 2
					};
				}
			}
			double averageDailyLuck = Game1.player.team.AverageDailyLuck(this);
			double averageMiningLevel = Game1.player.team.AverageSkillLevel(3, Game1.currentLocation);
			double chanceModifier = averageDailyLuck + averageMiningLevel * 0.005;
			if (this.mineLevel > 50 && this.mineRandom.NextDouble() < 0.00025 + (double)this.mineLevel / 120000.0 + 0.0005 * chanceModifier / 2.0)
			{
				whichStone = 2;
				stoneHealth = 10;
			}
			else if (gemStoneChance != 0.0 && this.mineRandom.NextDouble() < gemStoneChance + gemStoneChance * chanceModifier + (double)this.mineLevel / 24000.0)
			{
				return new Object(this.getRandomGemRichStoneForThisLevel(this.mineLevel), 1, false, -1, 0)
				{
					MinutesUntilReady = 5
				};
			}
			if (this.mineRandom.NextDouble() < chanceForPurpleStone / 2.0 + chanceForPurpleStone * averageMiningLevel * 0.008 + chanceForPurpleStone * (averageDailyLuck / 2.0))
			{
				whichStone = 44;
			}
			if (this.mineLevel > 100 && this.mineRandom.NextDouble() < chanceForMysticStone + chanceForMysticStone * averageMiningLevel * 0.008 + chanceForMysticStone * (averageDailyLuck / 2.0))
			{
				whichStone = 46;
			}
			whichStone += whichStone % 2;
			if (this.mineRandom.NextDouble() < 0.1 && this.getMineArea(-1) != 40)
			{
				if (!stoneColor.Equals(Color.White))
				{
					return new ColoredObject(this.mineRandom.Choose("668", "670"), 1, stoneColor)
					{
						MinutesUntilReady = 2,
						Flipped = this.mineRandom.NextBool()
					};
				}
				return new Object(this.mineRandom.Choose("668", "670"), 1, false, -1, 0)
				{
					MinutesUntilReady = 2,
					Flipped = this.mineRandom.NextBool()
				};
			}
			else
			{
				if (!stoneColor.Equals(Color.White))
				{
					return new ColoredObject(whichStone.ToString(), 1, stoneColor)
					{
						MinutesUntilReady = stoneHealth,
						Flipped = this.mineRandom.NextBool()
					};
				}
				return new Object(whichStone.ToString(), 1, false, -1, 0)
				{
					MinutesUntilReady = stoneHealth
				};
			}
		}

		// Token: 0x060031A6 RID: 12710 RVA: 0x0027A39C File Offset: 0x0027859C
		public static void OnLeftMines()
		{
			if (!Game1.IsClient && !Game1.IsMultiplayer)
			{
				MineShaft.clearInactiveMines(false);
			}
			Game1.player.buffs.Remove("CalicoStatueSpeed");
		}

		// Token: 0x060031A7 RID: 12711 RVA: 0x0027A3C6 File Offset: 0x002785C6
		public static void clearActiveMines()
		{
			MineShaft.activeMines.RemoveAll(delegate(MineShaft mine)
			{
				mine.OnRemoved();
				return true;
			});
		}

		// Token: 0x060031A8 RID: 12712 RVA: 0x0027A3F4 File Offset: 0x002785F4
		private static void clearInactiveMines(bool keepUntickedLevels = true)
		{
			int maxMineLevel = -1;
			int maxSkullLevel = -1;
			string[] disconnectLevels = Game1.getAllFarmhands().Select(delegate(Farmer fh)
			{
				if ((long)fh.disconnectDay.Value != (long)((ulong)Game1.MasterPlayer.stats.DaysPlayed))
				{
					return null;
				}
				return fh.disconnectLocation.Value;
			}).ToArray<string>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				int playerMineLevel;
				if (farmer.locationBeforeForcedEvent.Value != null && MineShaft.IsGeneratedLevel(farmer.locationBeforeForcedEvent.Value, out playerMineLevel))
				{
					if (playerMineLevel > 120)
					{
						if (playerMineLevel < 77377)
						{
							maxSkullLevel = Math.Max(maxSkullLevel, playerMineLevel);
						}
					}
					else
					{
						maxMineLevel = Math.Max(maxMineLevel, playerMineLevel);
					}
				}
			}
			foreach (MineShaft mine2 in MineShaft.activeMines)
			{
				if (mine2.farmers.Any() || disconnectLevels.Contains(mine2.NameOrUniqueName))
				{
					if (mine2.mineLevel > 120)
					{
						if (mine2.mineLevel < 77377)
						{
							maxSkullLevel = Math.Max(maxSkullLevel, mine2.mineLevel);
						}
					}
					else
					{
						maxMineLevel = Math.Max(maxMineLevel, mine2.mineLevel);
					}
				}
			}
			MineShaft.activeMines.RemoveAll(delegate(MineShaft mine)
			{
				if (mine.mineLevel == 77377)
				{
					return false;
				}
				if (disconnectLevels.Contains(mine.NameOrUniqueName))
				{
					return false;
				}
				if (mine.mineLevel > 120)
				{
					if (mine.mineLevel <= maxSkullLevel)
					{
						return false;
					}
				}
				else if (mine.mineLevel <= maxMineLevel)
				{
					return false;
				}
				if (mine.lifespan == 0 & keepUntickedLevels)
				{
					return false;
				}
				if (Game1.IsServer)
				{
					LocationRequest locationRequest = Game1.locationRequest;
					MineShaft requestedMine = ((locationRequest != null) ? locationRequest.Location : null) as MineShaft;
					if (requestedMine != null && mine.NameOrUniqueName == requestedMine.NameOrUniqueName)
					{
						return false;
					}
				}
				mine.OnRemoved();
				return true;
			});
			if (MineShaft.activeMines.Count == 0)
			{
				Game1.player.team.calicoEggSkullCavernRating.Value = 0;
				Game1.player.team.calicoStatueEffects.Clear();
				MineShaft.deepestLevelOnCurrentDesertFestivalRun = 0;
			}
		}

		// Token: 0x060031A9 RID: 12713 RVA: 0x0027A5D8 File Offset: 0x002787D8
		public static void UpdateMines10Minutes(int timeOfDay)
		{
			MineShaft.clearInactiveMines(true);
			if (Game1.IsClient)
			{
				return;
			}
			foreach (MineShaft mine in MineShaft.activeMines)
			{
				if (mine.farmers.Any())
				{
					mine.performTenMinuteUpdate(timeOfDay);
				}
				mine.lifespan++;
			}
		}

		// Token: 0x060031AA RID: 12714 RVA: 0x0027A654 File Offset: 0x00278854
		protected override void updateCharacters(GameTime time)
		{
			if (!this.farmers.Any())
			{
				return;
			}
			base.updateCharacters(time);
		}

		// Token: 0x060031AB RID: 12715 RVA: 0x0027A66C File Offset: 0x0027886C
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (Game1.shouldTimePass(false) && this.isFogUp.Value)
			{
				int oldTime = this.fogTime;
				this.fogTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (Game1.IsMasterGame)
				{
					if (this.fogTime > 5000 && oldTime % 4000 < this.fogTime % 4000)
					{
						this.spawnFlyingMonsterOffScreen();
					}
					if (this.fogTime <= 0)
					{
						this.isFogUp.Value = false;
						if (this.isDarkArea())
						{
							this.netFogColor.Value = Color.Black;
							return;
						}
						if (this.GetAdditionalDifficulty() > 0 && this.getMineArea(-1) == 40 && !this.isDarkArea())
						{
							this.netFogColor.Value = default(Color);
						}
					}
				}
			}
		}

		// Token: 0x060031AC RID: 12716 RVA: 0x0027A750 File Offset: 0x00278950
		public static void UpdateMines(GameTime time)
		{
			foreach (MineShaft mine in MineShaft.activeMines)
			{
				if (mine.farmers.Any())
				{
					mine.UpdateWhenCurrentLocation(time);
				}
				mine.updateEvenIfFarmerIsntHere(time, false);
			}
		}

		// Token: 0x060031AD RID: 12717 RVA: 0x0027A7B8 File Offset: 0x002789B8
		public override void OnRemoved()
		{
			base.OnRemoved();
			this.mapContent.Dispose();
		}

		// Token: 0x060031AE RID: 12718 RVA: 0x0027A7CC File Offset: 0x002789CC
		public static string GetLevelName(int level, int? forceLayout = null)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (forceLayout == null)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("UndergroundMine");
				defaultInterpolatedStringHandler.AppendFormatted<int>(level);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 2);
			defaultInterpolatedStringHandler.AppendLiteral("UndergroundMine");
			defaultInterpolatedStringHandler.AppendFormatted<int>(level);
			defaultInterpolatedStringHandler.AppendLiteral(":");
			defaultInterpolatedStringHandler.AppendFormatted<int?>(forceLayout);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060031AF RID: 12719 RVA: 0x0027A844 File Offset: 0x00278A44
		public static bool IsGeneratedLevel(GameLocation location)
		{
			int num;
			int? num2;
			return MineShaft.IsGeneratedLevel(location, out num, out num2);
		}

		// Token: 0x060031B0 RID: 12720 RVA: 0x0027A85C File Offset: 0x00278A5C
		public static bool IsGeneratedLevel(GameLocation location, out int level)
		{
			int? num;
			return MineShaft.IsGeneratedLevel(location, out level, out num);
		}

		// Token: 0x060031B1 RID: 12721 RVA: 0x0027A874 File Offset: 0x00278A74
		public static bool IsGeneratedLevel(GameLocation location, out int level, out int? forceLayout)
		{
			MineShaft mine = location as MineShaft;
			if (mine != null)
			{
				level = mine.mineLevel;
				forceLayout = mine.forceLayout;
				return true;
			}
			level = 0;
			forceLayout = null;
			return false;
		}

		// Token: 0x060031B2 RID: 12722 RVA: 0x0027A8AC File Offset: 0x00278AAC
		public static bool IsGeneratedLevel(string locationName)
		{
			int num;
			int? num2;
			return MineShaft.IsGeneratedLevel(locationName, out num, out num2);
		}

		// Token: 0x060031B3 RID: 12723 RVA: 0x0027A8C4 File Offset: 0x00278AC4
		public static bool IsGeneratedLevel(string locationName, out int level)
		{
			int? num;
			return MineShaft.IsGeneratedLevel(locationName, out level, out num);
		}

		// Token: 0x060031B4 RID: 12724 RVA: 0x0027A8DC File Offset: 0x00278ADC
		public static bool IsGeneratedLevel(string locationName, out int level, out int? forceLayout)
		{
			if (locationName == null || !locationName.StartsWithIgnoreCase("UndergroundMine"))
			{
				level = 0;
				forceLayout = null;
				return false;
			}
			string rawLevel = locationName.Substring("UndergroundMine".Length);
			int splitIndex = rawLevel.IndexOf(':');
			if (splitIndex <= 0)
			{
				forceLayout = null;
				return int.TryParse(rawLevel, out level);
			}
			int forceLayoutValue;
			if (int.TryParse(rawLevel.Substring(0, splitIndex), out level) && int.TryParse(rawLevel.Substring(splitIndex + 1), out forceLayoutValue))
			{
				forceLayout = new int?(forceLayoutValue);
				return true;
			}
			level = 0;
			forceLayout = null;
			return false;
		}

		// Token: 0x060031B5 RID: 12725 RVA: 0x0027A970 File Offset: 0x00278B70
		public static MineShaft GetMine(string name)
		{
			int mineLevel;
			int? forceLayout;
			if (!MineShaft.IsGeneratedLevel(name, out mineLevel, out forceLayout))
			{
				Game1.log.Warn("Failed parsing mine level from location name '" + name + "', defaulting to level 0.");
				mineLevel = 0;
			}
			if (forceLayout != null)
			{
				name = MineShaft.GetLevelName(mineLevel, null);
			}
			foreach (MineShaft mine in MineShaft.activeMines)
			{
				if (mine.Name == name)
				{
					if (forceLayout != null)
					{
						int num = mine.loadedMapNumber;
						int? num2 = forceLayout;
						if (!(num == num2.GetValueOrDefault() & num2 != null))
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Can't set mine level ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(mineLevel);
							defaultInterpolatedStringHandler.AppendLiteral(" to layout ");
							defaultInterpolatedStringHandler.AppendFormatted<int?>(forceLayout);
							defaultInterpolatedStringHandler.AppendLiteral(" because it's already active with layout ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(mine.loadedMapNumber);
							defaultInterpolatedStringHandler.AppendLiteral(".");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					return mine;
				}
			}
			MineShaft newMine = new MineShaft(mineLevel, forceLayout);
			MineShaft.activeMines.Add(newMine);
			newMine.generateContents();
			return newMine;
		}

		// Token: 0x060031B6 RID: 12726 RVA: 0x0027AACC File Offset: 0x00278CCC
		public static void ForEach(Action<MineShaft> action)
		{
			foreach (MineShaft mine in MineShaft.activeMines)
			{
				action(mine);
			}
		}

		// Token: 0x04002101 RID: 8449
		public const int mineFrostLevel = 40;

		// Token: 0x04002102 RID: 8450
		public const int mineLavaLevel = 80;

		// Token: 0x04002103 RID: 8451
		public const int upperArea = 0;

		// Token: 0x04002104 RID: 8452
		public const int jungleArea = 10;

		// Token: 0x04002105 RID: 8453
		public const int frostArea = 40;

		// Token: 0x04002106 RID: 8454
		public const int lavaArea = 80;

		// Token: 0x04002107 RID: 8455
		public const int desertArea = 121;

		// Token: 0x04002108 RID: 8456
		public const int bottomOfMineLevel = 120;

		// Token: 0x04002109 RID: 8457
		public const int quarryMineShaft = 77377;

		// Token: 0x0400210A RID: 8458
		public const int numberOfLevelsPerArea = 40;

		// Token: 0x0400210B RID: 8459
		public const int mineFeature_barrels = 0;

		// Token: 0x0400210C RID: 8460
		public const int mineFeature_chests = 1;

		// Token: 0x0400210D RID: 8461
		public const int mineFeature_coalCart = 2;

		// Token: 0x0400210E RID: 8462
		public const int mineFeature_elevator = 3;

		// Token: 0x0400210F RID: 8463
		public const double chanceForColoredGemstone = 0.008;

		// Token: 0x04002110 RID: 8464
		public const double chanceForDiamond = 0.0005;

		// Token: 0x04002111 RID: 8465
		public const double chanceForPrismaticShard = 0.0005;

		// Token: 0x04002112 RID: 8466
		public const int monsterLimit = 30;

		// Token: 0x04002113 RID: 8467
		public const string MineTileSheetId = "mine";

		// Token: 0x04002114 RID: 8468
		public static SerializableDictionary<int, MineInfo> permanentMineChanges = new SerializableDictionary<int, MineInfo>();

		// Token: 0x04002115 RID: 8469
		public static int numberOfCraftedStairsUsedThisRun;

		// Token: 0x04002116 RID: 8470
		public Random mineRandom = new Random();

		// Token: 0x04002117 RID: 8471
		private LocalizedContentManager mineLoader = Game1.content.CreateTemporary();

		// Token: 0x04002118 RID: 8472
		private int timeUntilElevatorLightUp;

		// Token: 0x04002119 RID: 8473
		[XmlIgnore]
		public int loadedMapNumber;

		// Token: 0x0400211A RID: 8474
		public int fogTime;

		// Token: 0x0400211B RID: 8475
		public NetBool isFogUp = new NetBool();

		// Token: 0x0400211C RID: 8476
		public static int timeSinceLastMusic = 200000;

		// Token: 0x0400211D RID: 8477
		public bool ladderHasSpawned;

		// Token: 0x0400211E RID: 8478
		public bool ghostAdded;

		// Token: 0x0400211F RID: 8479
		public bool loadedDarkArea;

		// Token: 0x04002120 RID: 8480
		public bool isFallingDownShaft;

		// Token: 0x04002121 RID: 8481
		public Vector2 fogPos;

		// Token: 0x04002122 RID: 8482
		private readonly NetBool elevatorShouldDing = new NetBool();

		// Token: 0x04002123 RID: 8483
		public readonly NetString mapImageSource = new NetString();

		// Token: 0x04002124 RID: 8484
		private readonly NetInt netMineLevel = new NetInt();

		// Token: 0x04002125 RID: 8485
		private readonly NetIntDelta netStonesLeftOnThisLevel = new NetIntDelta();

		// Token: 0x04002126 RID: 8486
		private readonly NetVector2 netTileBeneathLadder = new NetVector2();

		// Token: 0x04002127 RID: 8487
		private readonly NetVector2 netTileBeneathElevator = new NetVector2();

		// Token: 0x04002128 RID: 8488
		public readonly NetPoint calicoStatueSpot = new NetPoint();

		// Token: 0x04002129 RID: 8489
		public readonly NetPoint recentlyActivatedCalicoStatue = new NetPoint();

		// Token: 0x0400212A RID: 8490
		private readonly NetPoint netElevatorLightSpot = new NetPoint();

		// Token: 0x0400212B RID: 8491
		private readonly NetBool netIsSlimeArea = new NetBool();

		// Token: 0x0400212C RID: 8492
		private readonly NetBool netIsMonsterArea = new NetBool();

		// Token: 0x0400212D RID: 8493
		private readonly NetBool netIsTreasureRoom = new NetBool();

		// Token: 0x0400212E RID: 8494
		private readonly NetBool netIsDinoArea = new NetBool();

		// Token: 0x0400212F RID: 8495
		private readonly NetBool netIsQuarryArea = new NetBool();

		// Token: 0x04002130 RID: 8496
		private readonly NetBool netAmbientFog = new NetBool();

		// Token: 0x04002131 RID: 8497
		private readonly NetColor netLighting = new NetColor(Color.White);

		// Token: 0x04002132 RID: 8498
		private readonly NetColor netFogColor = new NetColor();

		// Token: 0x04002133 RID: 8499
		private readonly NetVector2Dictionary<bool, NetBool> createLadderAtEvent = new NetVector2Dictionary<bool, NetBool>();

		// Token: 0x04002134 RID: 8500
		private readonly NetPointDictionary<bool, NetBool> createLadderDownEvent = new NetPointDictionary<bool, NetBool>();

		// Token: 0x04002135 RID: 8501
		private float fogAlpha;

		// Token: 0x04002136 RID: 8502
		[XmlIgnore]
		public static ICue bugLevelLoop;

		// Token: 0x04002137 RID: 8503
		public readonly NetBool rainbowLights = new NetBool(false);

		// Token: 0x04002138 RID: 8504
		public readonly NetBool isLightingDark = new NetBool(false);

		// Token: 0x04002139 RID: 8505
		private readonly int? forceLayout;

		// Token: 0x0400213A RID: 8506
		private LocalizedContentManager mapContent;

		// Token: 0x0400213B RID: 8507
		public static List<MineShaft> activeMines = new List<MineShaft>();

		// Token: 0x0400213C RID: 8508
		public static HashSet<int> mushroomLevelsGeneratedToday = new HashSet<int>();

		// Token: 0x0400213D RID: 8509
		public static int totalCalicoStatuesActivatedToday;

		// Token: 0x0400213E RID: 8510
		private int recentCalicoStatueEffect;

		// Token: 0x0400213F RID: 8511
		private bool forceFirstTime;

		// Token: 0x04002140 RID: 8512
		private static int deepestLevelOnCurrentDesertFestivalRun;

		// Token: 0x04002141 RID: 8513
		private int lastLevelsDownFallen;

		// Token: 0x04002142 RID: 8514
		private Microsoft.Xna.Framework.Rectangle fogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);

		// Token: 0x04002143 RID: 8515
		private List<Vector2> brownSpots = new List<Vector2>();

		// Token: 0x04002144 RID: 8516
		private int lifespan;

		// Token: 0x04002145 RID: 8517
		private bool hasAddedDesertFestivalStatue;

		// Token: 0x04002146 RID: 8518
		public float calicoEggIconTimerShake;
	}
}
