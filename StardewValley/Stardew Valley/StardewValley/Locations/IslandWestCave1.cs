using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E2 RID: 738
	public class IslandWestCave1 : IslandLocation
	{
		// Token: 0x060030D5 RID: 12501 RVA: 0x002691D4 File Offset: 0x002673D4
		public IslandWestCave1()
		{
		}

		// Token: 0x060030D6 RID: 12502 RVA: 0x0026925C File Offset: 0x0026745C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.netPhase, "netPhase").AddField(this.isActivated, "isActivated").AddField(this.currentDifficulty, "currentDifficulty").AddField(this.currentCrystalSequenceIndex, "currentCrystalSequenceIndex").AddField(this.currentCrystalSequence, "currentCrystalSequence").AddField(this.enterValueEvent.NetFields, "enterValueEvent.NetFields").AddField(this.netPhaseTimer, "netPhaseTimer").AddField(this.completed, "completed").AddField(this.timesFailed, "timesFailed");
			this.enterValueEvent.onEvent += this.enterValue;
			this.isActivated.fieldChangeVisibleEvent += this.onActivationChanged;
		}

		// Token: 0x060030D7 RID: 12503 RVA: 0x0026933C File Offset: 0x0026753C
		public IslandWestCave1(string map, string name) : base(map, name)
		{
		}

		// Token: 0x060030D8 RID: 12504 RVA: 0x002693C4 File Offset: 0x002675C4
		public void onActivationChanged(NetBool field, bool old_value, bool new_value)
		{
			this.updateActivationVisuals();
		}

		// Token: 0x060030D9 RID: 12505 RVA: 0x002693CC File Offset: 0x002675CC
		protected override void resetSharedState()
		{
			base.resetSharedState();
			this.resetPuzzle();
		}

		// Token: 0x060030DA RID: 12506 RVA: 0x002693DA File Offset: 0x002675DA
		public void resetPuzzle()
		{
			this.isActivated.Value = false;
			this.updateActivationVisuals();
			this.netPhase.Value = 3;
		}

		// Token: 0x060030DB RID: 12507 RVA: 0x002693FA File Offset: 0x002675FA
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			this.UpdateActivationTiles();
		}

		// Token: 0x060030DC RID: 12508 RVA: 0x0026940C File Offset: 0x0026760C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (this.crystals.Count == 0)
			{
				this.crystals.Add(new IslandWestCave1.CaveCrystal
				{
					tileLocation = new Vector2(3f, 4f),
					color = new Color(220, 0, 255),
					currentColor = new Color(220, 0, 255),
					id = 1,
					pitch = 0
				});
				this.crystals.Add(new IslandWestCave1.CaveCrystal
				{
					tileLocation = new Vector2(4f, 6f),
					color = Color.Lime,
					currentColor = Color.Lime,
					id = 2,
					pitch = 700
				});
				this.crystals.Add(new IslandWestCave1.CaveCrystal
				{
					tileLocation = new Vector2(6f, 7f),
					color = new Color(255, 50, 100),
					currentColor = new Color(255, 50, 100),
					id = 3,
					pitch = 1200
				});
				this.crystals.Add(new IslandWestCave1.CaveCrystal
				{
					tileLocation = new Vector2(8f, 6f),
					color = new Color(0, 200, 255),
					currentColor = new Color(0, 200, 255),
					id = 4,
					pitch = 1400
				});
				this.crystals.Add(new IslandWestCave1.CaveCrystal
				{
					tileLocation = new Vector2(9f, 4f),
					color = new Color(255, 180, 0),
					currentColor = new Color(255, 180, 0),
					id = 5,
					pitch = 1600
				});
			}
			this.updateActivationVisuals();
		}

		// Token: 0x060030DD RID: 12509 RVA: 0x00269608 File Offset: 0x00267808
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (who.IsLocalPlayer)
			{
				string a = ArgUtility.Get(action, 0, null, true);
				if (!(a == "Crystal"))
				{
					if (a == "CrystalCaveActivate")
					{
						if (!this.isActivated.Value && !this.completed.Value)
						{
							this.isActivated.Value = true;
							Game1.playSound("openBox", null);
							this.updateActivationVisuals();
							this.netPhaseTimer.Value = 1200f;
							this.netPhase.Value = 0;
							this.currentDifficulty.Value = 2;
							return true;
						}
					}
				}
				else
				{
					int crystalId;
					string error;
					if (!ArgUtility.TryGetInt(action, 1, out crystalId, out error, "int crystalId"))
					{
						base.LogTileActionError(action, tileLocation.X, tileLocation.Y, error);
						return false;
					}
					if (this.netPhase.Value == 5 || this.netPhase.Value == 3 || this.netPhase.Value == 2)
					{
						this.enterValueEvent.Fire(crystalId);
						return true;
					}
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x060030DE RID: 12510 RVA: 0x0026971C File Offset: 0x0026791C
		public virtual void updateActivationVisuals()
		{
			if (this.map == null || Game1.gameMode == 6 || Game1.currentLocation != this)
			{
				return;
			}
			if (this.isActivated.Value || this.completed.Value)
			{
				Game1.currentLightSources.Add(new LightSource("IslandWestCave1", 1, new Vector2(6.5f, 1f) * 64f, 2f, Color.Black, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
			else
			{
				Utility.removeLightSource("IslandWestCave1");
			}
			this.UpdateActivationTiles();
			if (this.completed.Value)
			{
				this.addCompletionTorches();
			}
		}

		// Token: 0x060030DF RID: 12511 RVA: 0x002697C4 File Offset: 0x002679C4
		public virtual void UpdateActivationTiles()
		{
			if (this.map == null || Game1.gameMode == 6 || Game1.currentLocation != this)
			{
				return;
			}
			int headIndex = (this.isActivated.Value || this.completed.Value) ? 33 : 31;
			base.setMapTile(6, 1, headIndex, "Buildings", "untitled tile sheet", null, true);
		}

		// Token: 0x060030E0 RID: 12512 RVA: 0x00269824 File Offset: 0x00267A24
		public virtual void enterValue(int which)
		{
			if (this.netPhase.Value == 2 && Game1.IsMasterGame && this.currentCrystalSequence.Count > this.currentCrystalSequenceIndex.Value)
			{
				int value;
				if (this.currentCrystalSequence[this.currentCrystalSequenceIndex.Value] != which - 1)
				{
					base.playSound("cancel", null, null, SoundContext.Default);
					this.resetPuzzle();
					NetInt netInt = this.timesFailed;
					value = netInt.Value;
					netInt.Value = value + 1;
					return;
				}
				NetInt netInt2 = this.currentCrystalSequenceIndex;
				value = netInt2.Value;
				netInt2.Value = value + 1;
				if (this.currentCrystalSequenceIndex.Value >= this.currentCrystalSequence.Count)
				{
					DelayedAction.playSoundAfterDelay((this.currentDifficulty.Value == 7) ? "discoverMineral" : "newArtifact", 500, this, null, -1, false);
					this.netPhaseTimer.Value = 2000f;
					this.netPhase.Value = 4;
				}
			}
			if (this.crystals.Count > which - 1)
			{
				this.crystals[which - 1].activate();
			}
		}

		// Token: 0x060030E1 RID: 12513 RVA: 0x0026995D File Offset: 0x00267B5D
		public override void cleanupBeforePlayerExit()
		{
			this.crystals.Clear();
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x060030E2 RID: 12514 RVA: 0x00269970 File Offset: 0x00267B70
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			this.enterValueEvent.Poll();
			if ((this.localPhase != 1 || this.currentPlaybackCrystalSequenceIndex < 0 || this.currentPlaybackCrystalSequenceIndex >= this.currentCrystalSequence.Count) && this.localPhase != this.netPhase.Value)
			{
				this.localPhaseTimer = this.netPhaseTimer.Value;
				this.localPhase = this.netPhase.Value;
				if (this.localPhase != 1)
				{
					this.currentPlaybackCrystalSequenceIndex = -1;
				}
				else
				{
					this.currentPlaybackCrystalSequenceIndex = 0;
				}
			}
			base.UpdateWhenCurrentLocation(time);
			foreach (IslandWestCave1.CaveCrystal caveCrystal in this.crystals)
			{
				caveCrystal.update();
			}
			if (this.localPhaseTimer > 0f)
			{
				this.localPhaseTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.localPhaseTimer <= 0f)
				{
					int num = this.localPhase;
					if (num != 0 && num != 4)
					{
						if (num == 5)
						{
							if (Game1.currentLocation == this)
							{
								Game1.playSound("fireball", null);
								Utility.addSmokePuff(this, new Vector2(5f, 1f) * 64f, 0, 2f, 0.02f, 0.75f, 0.002f);
								Utility.addSmokePuff(this, new Vector2(7f, 1f) * 64f, 0, 2f, 0.02f, 0.75f, 0.002f);
							}
							if (Game1.IsMasterGame)
							{
								Game1.player.team.MarkCollectedNut("IslandWestCavePuzzle");
								Game1.createObjectDebris("(O)73", 5, 1, this);
								Game1.createObjectDebris("(O)73", 7, 1, this);
								Game1.createObjectDebris("(O)73", 6, 1, this);
							}
							this.completed.Value = true;
							if (Game1.currentLocation == this)
							{
								this.addCompletionTorches();
							}
						}
					}
					else
					{
						this.currentPlaybackCrystalSequenceIndex = 0;
						if (Game1.IsMasterGame)
						{
							NetInt netInt = this.currentDifficulty;
							int value = netInt.Value;
							netInt.Value = value + 1;
							this.currentCrystalSequence.Clear();
							this.currentCrystalSequenceIndex.Value = 0;
							if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
							{
								this.netPhaseTimer.Value = 10f;
								this.netPhase.Value = 5;
								goto IL_2B8;
							}
							for (int i = 0; i < this.currentDifficulty.Value; i++)
							{
								this.currentCrystalSequence.Add(Game1.random.Next(5));
							}
							this.netPhase.Value = 1;
						}
						this.betweenNotesTimer = 600f;
					}
				}
			}
			IL_2B8:
			if (this.localPhase == 1)
			{
				this.betweenNotesTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.betweenNotesTimer <= 0f && this.currentCrystalSequence.Count > 0 && this.currentPlaybackCrystalSequenceIndex >= 0)
				{
					int which = this.currentCrystalSequence[this.currentPlaybackCrystalSequenceIndex];
					if (which < this.crystals.Count)
					{
						this.crystals[which].activate();
					}
					this.currentPlaybackCrystalSequenceIndex++;
					int betweenNotesDivisor = this.currentDifficulty.Value;
					if (this.currentDifficulty.Value > 5)
					{
						betweenNotesDivisor--;
						if (this.timesFailed.Value >= 4)
						{
							betweenNotesDivisor--;
						}
						if (this.timesFailed.Value >= 6)
						{
							betweenNotesDivisor--;
						}
						if (this.timesFailed.Value >= 8)
						{
							betweenNotesDivisor = 3;
						}
					}
					else if (this.timesFailed.Value >= 4 && this.currentDifficulty.Value > 4)
					{
						betweenNotesDivisor--;
					}
					this.betweenNotesTimer = 1500f / (float)betweenNotesDivisor;
					if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
					{
						this.betweenNotesTimer = 100f;
					}
					if (this.currentPlaybackCrystalSequenceIndex >= this.currentCrystalSequence.Count)
					{
						this.currentPlaybackCrystalSequenceIndex = -1;
						if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
						{
							if (Game1.IsMasterGame)
							{
								this.netPhaseTimer.Value = 1000f;
								this.netPhase.Value = 5;
								return;
							}
						}
						else if (Game1.IsMasterGame)
						{
							this.netPhase.Value = 2;
							this.currentCrystalSequenceIndex.Value = 0;
						}
					}
				}
			}
		}

		// Token: 0x060030E3 RID: 12515 RVA: 0x00269E10 File Offset: 0x00268010
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandWestCave1 cave = l as IslandWestCave1;
			if (cave != null)
			{
				this.completed.Value = cave.completed.Value;
			}
		}

		// Token: 0x060030E4 RID: 12516 RVA: 0x00269E44 File Offset: 0x00268044
		public void addCompletionTorches()
		{
			if (this.completed.Value)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(5f, 1f) * 64f + new Vector2(0f, -20f), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandWestCave1_Torch_1",
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.013439999f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(7f, 1f) * 64f + new Vector2(8f, -20f), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandWestCave1_Torch_2",
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.013439999f
				});
			}
		}

		// Token: 0x060030E5 RID: 12517 RVA: 0x00269FB4 File Offset: 0x002681B4
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (IslandWestCave1.CaveCrystal caveCrystal in this.crystals)
			{
				caveCrystal.draw(b);
			}
		}

		// Token: 0x040020D0 RID: 8400
		public const string lightSourceId = "IslandWestCave1";

		// Token: 0x040020D1 RID: 8401
		[XmlIgnore]
		protected List<IslandWestCave1.CaveCrystal> crystals = new List<IslandWestCave1.CaveCrystal>();

		// Token: 0x040020D2 RID: 8402
		public const int PHASE_INTRO = 0;

		// Token: 0x040020D3 RID: 8403
		public const int PHASE_PLAY_SEQUENCE = 1;

		// Token: 0x040020D4 RID: 8404
		public const int PHASE_WAIT_FOR_PLAYER_INPUT = 2;

		// Token: 0x040020D5 RID: 8405
		public const int PHASE_NOTHING = 3;

		// Token: 0x040020D6 RID: 8406
		public const int PHASE_SUCCESSFUL_SEQUENCE = 4;

		// Token: 0x040020D7 RID: 8407
		public const int PHASE_OUTRO = 5;

		// Token: 0x040020D8 RID: 8408
		[XmlElement("completed")]
		public NetBool completed = new NetBool();

		// Token: 0x040020D9 RID: 8409
		[XmlIgnore]
		public NetBool isActivated = new NetBool(false);

		// Token: 0x040020DA RID: 8410
		[XmlIgnore]
		public NetFloat netPhaseTimer = new NetFloat();

		// Token: 0x040020DB RID: 8411
		[XmlIgnore]
		public float localPhaseTimer;

		// Token: 0x040020DC RID: 8412
		[XmlIgnore]
		public float betweenNotesTimer;

		// Token: 0x040020DD RID: 8413
		[XmlIgnore]
		public int localPhase;

		// Token: 0x040020DE RID: 8414
		[XmlIgnore]
		public NetInt netPhase = new NetInt(3);

		// Token: 0x040020DF RID: 8415
		[XmlIgnore]
		public NetInt currentDifficulty = new NetInt(2);

		// Token: 0x040020E0 RID: 8416
		[XmlIgnore]
		public NetInt currentCrystalSequenceIndex = new NetInt(0);

		// Token: 0x040020E1 RID: 8417
		[XmlIgnore]
		public int currentPlaybackCrystalSequenceIndex;

		// Token: 0x040020E2 RID: 8418
		[XmlIgnore]
		public NetInt timesFailed = new NetInt(0);

		// Token: 0x040020E3 RID: 8419
		[XmlIgnore]
		public NetList<int, NetInt> currentCrystalSequence = new NetList<int, NetInt>();

		// Token: 0x040020E4 RID: 8420
		[XmlIgnore]
		public NetEvent1Field<int, NetInt> enterValueEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x02000664 RID: 1636
		public class CaveCrystal
		{
			// Token: 0x0600453A RID: 17722 RVA: 0x0031ED58 File Offset: 0x0031CF58
			public void update()
			{
				if (this.glowTimer > 0f)
				{
					this.glowTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					this.currentColor.R = (byte)Utility.Lerp((float)this.color.R, 255f, this.glowTimer / 1000f);
					this.currentColor.G = (byte)Utility.Lerp((float)this.color.G, 255f, this.glowTimer / 1000f);
					this.currentColor.B = (byte)Utility.Lerp((float)this.color.B, 255f, this.glowTimer / 1000f);
				}
				if (this.shakeTimer > 0f)
				{
					this.shakeTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
			}

			// Token: 0x0600453B RID: 17723 RVA: 0x0031EE4C File Offset: 0x0031D04C
			public void activate()
			{
				this.glowTimer = 1000f;
				this.shakeTimer = 100f;
				Game1.playSound("crystal", new int?(this.pitch));
				this.currentColor = this.color;
			}

			// Token: 0x0600453C RID: 17724 RVA: 0x0031EE88 File Offset: 0x0031D088
			public void draw(SpriteBatch b)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(this.tileLocation * 64f + new Vector2(8f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(188, 228, 52, 28)), this.currentColor, 0f, new Vector2(52f, 28f) / 2f, 4f, SpriteEffects.None, (this.tileLocation.Y * 64f + 64f - 8f) / 10000f);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(this.tileLocation * 64f + new Vector2(0f, -52f) + new Vector2((float)((this.shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0), (float)((this.shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(240, 227, 16, 29)), this.currentColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.tileLocation.Y * 64f + 64f - 4f) / 10000f);
			}

			// Token: 0x04002F80 RID: 12160
			public Vector2 tileLocation;

			// Token: 0x04002F81 RID: 12161
			public int id;

			// Token: 0x04002F82 RID: 12162
			public int pitch;

			// Token: 0x04002F83 RID: 12163
			public Color color;

			// Token: 0x04002F84 RID: 12164
			public Color currentColor;

			// Token: 0x04002F85 RID: 12165
			public float shakeTimer;

			// Token: 0x04002F86 RID: 12166
			public float glowTimer;
		}
	}
}
