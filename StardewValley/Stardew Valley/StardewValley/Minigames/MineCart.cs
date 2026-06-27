using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.SpecialOrders;

namespace StardewValley.Minigames
{
	// Token: 0x0200023C RID: 572
	public class MineCart : IMinigame
	{
		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x060025F9 RID: 9721 RVA: 0x001AA42B File Offset: 0x001A862B
		public double totalTime
		{
			get
			{
				return this._totalTime;
			}
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x060025FA RID: 9722 RVA: 0x001AA433 File Offset: 0x001A8633
		public double totalTimeMS
		{
			get
			{
				return this._totalTime * 1000.0;
			}
		}

		// Token: 0x060025FB RID: 9723 RVA: 0x001AA448 File Offset: 0x001A8648
		public MineCart(int whichTheme, int mode)
		{
			this._entities = new List<MineCart.Entity>();
			this._collectedFruit = new HashSet<MineCart.CollectableFruits>();
			this._generatorRolls = new List<MineCart.GeneratorRoll>();
			this._validObstacles = new Dictionary<MineCart.ObstacleTypes, List<Type>>();
			this.initLevelTransitions();
			if (Game1.player.team.junimoKartScores.GetScores().Count == 0)
			{
				Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Lewis", true).displayName, 50000);
				Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Shane", true).displayName, 25000);
				Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Sam", true).displayName, 10000);
				Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Abigail", true).displayName, 5000);
				Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Vincent", true).displayName, 250);
			}
			this.changeScreenSize();
			this.texture = Game1.content.Load<Texture2D>("Minigames\\MineCart");
			Game1.playSound("minecartLoop", out this.minecartLoop);
			this.minecartLoop.Pause();
			this.backBGYOffset = this.tileSize * 2;
			this.ytileOffset = this.screenHeight / 2 / this.tileSize;
			this.gameMode = mode;
			this.bottomTile = this.screenHeight / this.tileSize - 1;
			this.topTile = 4;
			this.currentTheme = whichTheme;
			this.ShowTitle();
		}

		// Token: 0x060025FC RID: 9724 RVA: 0x001AA6DC File Offset: 0x001A88DC
		public void initLevelTransitions()
		{
			this.LEVEL_TRANSITIONS = new MineCart.LevelTransition[]
			{
				new MineCart.LevelTransition(-1, 0, 2, 5, "rrr", null),
				new MineCart.LevelTransition(0, 8, 5, 5, "rddrrd", () => this.lastLevelWasPerfect),
				new MineCart.LevelTransition(0, 1, 5, 5, "rddlddrdd", null),
				new MineCart.LevelTransition(1, 3, 6, 11, "drdrrrrrrrrruuuuu", () => this.secondsOnThisLevel <= 60f),
				new MineCart.LevelTransition(1, 5, 6, 11, "rrurruuu", new Func<bool>(Game1.random.NextBool)),
				new MineCart.LevelTransition(1, 2, 6, 11, "rrurrrrddr", null),
				new MineCart.LevelTransition(8, 5, 8, 8, "ddrruuu", new Func<bool>(Game1.random.NextBool)),
				new MineCart.LevelTransition(8, 2, 8, 8, "ddrrrrddr", null),
				new MineCart.LevelTransition(5, 3, 10, 7, "urruulluurrrrrddddddr", null),
				new MineCart.LevelTransition(2, 3, 13, 12, "rurruuu", null),
				new MineCart.LevelTransition(3, 9, 16, 8, "rruuluu", new Func<bool>(Game1.random.NextBool)),
				new MineCart.LevelTransition(3, 4, 16, 8, "rrddrddr", null),
				new MineCart.LevelTransition(4, 6, 20, 12, "ruuruuuuuu", null),
				new MineCart.LevelTransition(9, 6, 17, 4, "rrdrrru", null),
				new MineCart.LevelTransition(6, 7, 22, 4, "rr", null)
			};
		}

		// Token: 0x060025FD RID: 9725 RVA: 0x001AA85C File Offset: 0x001A8A5C
		public void ShowTitle()
		{
			this.musicSW = new Stopwatch();
			Game1.changeMusicTrack("junimoKart", false, MusicContext.MiniGame);
			this.titleJunimoStartedBobbing = false;
			this.completelyPerfect = true;
			this.screenDarkness = 1f;
			this.fadeDelta = -1f;
			this.ResetState();
			this.player.enabled = false;
			this.setUpTheme(0);
			this.levelThemesFinishedThisRun.Clear();
			this.gameState = MineCart.GameStates.Title;
			this.CreateLakeDecor();
			this.RefreshHighScore();
			this.titleScreenJunimo = this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(259, 492, 14, 20), new Vector2((float)(this.screenWidth / 2 - 128 + 137), (float)(this.screenHeight / 2 - 35 + 46)), 100f, 0f, 0f, 0f, 99999f, 1f, 1, 1f, 0.24f, false, 0f));
			if (this.gameMode == 3)
			{
				this.setUpTheme(-1);
				return;
			}
			this.setUpTheme(0);
		}

		// Token: 0x060025FE RID: 9726 RVA: 0x001AA970 File Offset: 0x001A8B70
		public void RefreshHighScore()
		{
			this._currentHighScores = Game1.player.team.junimoKartScores.GetScores();
			this.currentHighScore = 0;
			if (this._currentHighScores.Count > 0)
			{
				this.currentHighScore = this._currentHighScores[0].Value;
			}
		}

		// Token: 0x060025FF RID: 9727 RVA: 0x001AA9C8 File Offset: 0x001A8BC8
		public MineCart.Obstacle AddObstacle(MineCart.Track track, MineCart.ObstacleTypes obstacle_type)
		{
			List<Type> obstacleTypes;
			if (track == null || !this._validObstacles.TryGetValue(obstacle_type, out obstacleTypes))
			{
				return null;
			}
			Type type = Game1.random.ChooseFrom(obstacleTypes);
			MineCart.Obstacle obstacle = this.AddEntity<MineCart.Obstacle>(Activator.CreateInstance(type) as MineCart.Obstacle);
			if (!obstacle.CanSpawnHere(track))
			{
				obstacle.Destroy();
				return null;
			}
			obstacle.position.X = track.position.X + (float)(this.tileSize / 2);
			obstacle.position.Y = (float)track.GetYAtPoint(obstacle.position.X);
			track.obstacle = obstacle;
			obstacle.InitializeObstacle(track);
			return obstacle;
		}

		// Token: 0x06002600 RID: 9728 RVA: 0x001AAA65 File Offset: 0x001A8C65
		public virtual T AddEntity<T>(T new_entity) where T : MineCart.Entity
		{
			this._entities.Add(new_entity);
			new_entity.Initialize(this);
			return new_entity;
		}

		// Token: 0x06002601 RID: 9729 RVA: 0x001AAA88 File Offset: 0x001A8C88
		public MineCart.Track GetTrackForXPosition(float x)
		{
			int tile_position = (int)(x / (float)this.tileSize);
			List<MineCart.Track> tracks;
			if (!this._tracks.TryGetValue(tile_position, out tracks))
			{
				return null;
			}
			return tracks[0];
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x001AAABC File Offset: 0x001A8CBC
		public void AddCheckpoint(int tile_x)
		{
			if (this.gameMode == 2)
			{
				return;
			}
			tile_x = this.GetValidCheckpointPosition(tile_x);
			if (tile_x != this.furthestGeneratedCheckpoint && tile_x > this.furthestGeneratedCheckpoint + 8 && this.IsTileInBounds((int)(this.GetTrackForXPosition((float)(tile_x * this.tileSize)).position.Y / (float)this.tileSize)))
			{
				this.furthestGeneratedCheckpoint = tile_x;
				MineCart.CheckpointIndicator checkpoint_indicator = this.AddEntity<MineCart.CheckpointIndicator>(new MineCart.CheckpointIndicator());
				checkpoint_indicator.position.X = ((float)tile_x + 0.5f) * (float)this.tileSize;
				checkpoint_indicator.position.Y = (float)this.GetTrackForXPosition((float)(tile_x * this.tileSize)).GetYAtPoint(checkpoint_indicator.position.X + 5f);
				this.checkpointPositions.Add(tile_x);
			}
		}

		// Token: 0x06002603 RID: 9731 RVA: 0x001AAB8C File Offset: 0x001A8D8C
		public List<MineCart.Track> GetTracksForXPosition(float x)
		{
			int tilePosition = (int)(x / (float)this.tileSize);
			return this._tracks.GetValueOrDefault(tilePosition);
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x001AABB0 File Offset: 0x001A8DB0
		protected bool _IsGeneratingOnUpperHalf()
		{
			int mid_point = (this.topTile + this.bottomTile) / 2;
			return this.generatorPosition.Y <= mid_point;
		}

		// Token: 0x06002605 RID: 9733 RVA: 0x001AABE0 File Offset: 0x001A8DE0
		protected bool _IsGeneratingOnLowerHalf()
		{
			int mid_point = (this.topTile + this.bottomTile) / 2;
			return this.generatorPosition.Y >= mid_point;
		}

		// Token: 0x06002606 RID: 9734 RVA: 0x001AAC10 File Offset: 0x001A8E10
		protected void _GenerateMoreTrack()
		{
			while ((float)(this.generatorPosition.X * this.tileSize) <= this.screenLeftBound + (float)this.screenWidth + (float)(16 * this.tileSize))
			{
				if (this._trackGenerator == null)
				{
					if (this.generatorPosition.X < this.distanceToTravel)
					{
						for (int tries = 0; tries < 2; tries++)
						{
							for (int i = 0; i < this._generatorRolls.Count; i++)
							{
								if (this._forcedNextGenerator != null)
								{
									this._trackGenerator = this._forcedNextGenerator;
									this._forcedNextGenerator = null;
									break;
								}
								if (this._generatorRolls[i].generator != this._lastGenerator && Game1.random.NextDouble() < (double)this._generatorRolls[i].chance && (this._generatorRolls[i].additionalGenerationCondition == null || this._generatorRolls[i].additionalGenerationCondition()))
								{
									this._trackGenerator = this._generatorRolls[i].generator;
									this._forcedNextGenerator = this._generatorRolls[i].forcedNextGenerator;
									break;
								}
							}
							if (this._trackGenerator != null)
							{
								IL_15C:
								this._trackGenerator.Initialize();
								this._lastGenerator = this._trackGenerator;
								goto IL_173;
							}
							if (this._trackGenerator == null)
							{
								if (this._lastGenerator != null)
								{
									this._lastGenerator = null;
								}
								else
								{
									this._trackGenerator = new MineCart.StraightAwayGenerator(this).SetLength(2, 2).SetStaggerChance(0f).SetCheckpoint(false);
									this._forcedNextGenerator = null;
								}
							}
						}
						goto IL_15C;
					}
					this._trackGenerator = null;
					break;
				}
				IL_173:
				MineCart.BaseTrackGenerator trackGenerator = this._trackGenerator;
				if (trackGenerator != null)
				{
					trackGenerator.GenerateTrack();
				}
				if (this.generatorPosition.X >= this.distanceToTravel)
				{
					break;
				}
				this._trackGenerator = null;
			}
			if (this.generatorPosition.X >= this.distanceToTravel)
			{
				MineCart.Track track = this.AddTrack(this.generatorPosition.X, this.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				if (this._goalIndicator == null)
				{
					this._goalIndicator = this.AddEntity<MineCart.GoalIndicator>(new MineCart.GoalIndicator());
					this._goalIndicator.position.X = ((float)this.generatorPosition.X + 0.5f) * (float)this.tileSize;
					this._goalIndicator.position.Y = (float)track.GetYAtPoint(this._goalIndicator.position.X);
				}
				else
				{
					this.CreatePickup(new Vector2((float)this.generatorPosition.X + 0.5f, (float)(this.generatorPosition.Y - 1)) * (float)this.tileSize, true);
				}
				this.generatorPosition.X = this.generatorPosition.X + 1;
			}
		}

		// Token: 0x06002607 RID: 9735 RVA: 0x001AAED8 File Offset: 0x001A90D8
		public MineCart.Track AddTrack(int x, int y, MineCart.Track.TrackType type = MineCart.Track.TrackType.Straight)
		{
			if (type == MineCart.Track.TrackType.UpSlope || type == MineCart.Track.TrackType.SlimeUpSlope)
			{
				y++;
			}
			this._trackAddedFlip = !this._trackAddedFlip;
			MineCart.Track track_object = new MineCart.Track(type, this._trackAddedFlip);
			track_object.position.X = (float)(x * this.tileSize);
			track_object.position.Y = (float)(y * this.tileSize);
			return this.AddTrack(track_object);
		}

		// Token: 0x06002608 RID: 9736 RVA: 0x001AAF40 File Offset: 0x001A9140
		public MineCart.Track AddTrack(MineCart.Track track_object)
		{
			MineCart.Track track = this.AddEntity<MineCart.Track>(track_object);
			int x = (int)(track.position.X / (float)this.tileSize);
			List<MineCart.Track> tracks;
			if (!this._tracks.TryGetValue(x, out tracks))
			{
				tracks = (this._tracks[x] = new List<MineCart.Track>());
			}
			tracks.Add(track_object);
			from o in tracks
			orderby o.position.Y
			select o;
			return track;
		}

		// Token: 0x06002609 RID: 9737 RVA: 0x001AAFB9 File Offset: 0x001A91B9
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x001AAFC8 File Offset: 0x001A91C8
		public void UpdateMapTick(float time)
		{
			this.mapTimer += time;
			MineCart.MapJunimo map_junimo = null;
			foreach (MineCart.Entity entity in this._entities)
			{
				MineCart.MapJunimo junimo = entity as MineCart.MapJunimo;
				if (junimo != null)
				{
					map_junimo = junimo;
					break;
				}
			}
			if (this.mapTimer >= 2f && map_junimo.moveState == MineCart.MapJunimo.MoveState.Idle)
			{
				map_junimo.StartMoving();
			}
			MineCart.MapJunimo.MoveState moveState = map_junimo.moveState;
			if (moveState != MineCart.MapJunimo.MoveState.Moving)
			{
				if (moveState == MineCart.MapJunimo.MoveState.Finished)
				{
					if (this.mapTimer >= 1.5f)
					{
						this.fadeDelta = 1f;
					}
				}
			}
			else
			{
				this.mapTimer = 0f;
			}
			if (this.screenDarkness >= 1f && this.fadeDelta > 0f)
			{
				this.ShowCutscene();
			}
		}

		// Token: 0x0600260B RID: 9739 RVA: 0x001AB0A4 File Offset: 0x001A92A4
		public void UpdateCutsceneTick()
		{
			int fade_out_time = 400;
			if (this.gamePaused)
			{
				return;
			}
			int num = this.cutsceneTick;
			if (num != 0)
			{
				if (num == 100)
				{
					this.player.enabled = true;
				}
			}
			else
			{
				if (!this.minecartLoop.IsPaused)
				{
					this.minecartLoop.Pause();
				}
				this.cutsceneText = Game1.content.LoadString("Strings\\UI:Junimo_Kart_Level_" + this.currentTheme.ToString());
				if (this.currentTheme == 7)
				{
					this.cutsceneText = "";
				}
				this.player.enabled = false;
				this.screenDarkness = 1f;
				this.fadeDelta = -1f;
			}
			switch (this.currentTheme)
			{
			case 0:
				this.UpdateCutSceneForBrownArea();
				break;
			case 1:
				this.UpdateCutsceneForFrostArea();
				break;
			case 2:
				this.UpdateCutsceneForWaterArea();
				break;
			case 3:
				this.UpdateCutsceneForDarkArea();
				break;
			case 4:
				this.UpdateCutsceneForLavaArea();
				break;
			case 5:
				this.UpdateCutsceneForHeavenlyArea();
				break;
			case 6:
				this.UpdateCutsceneForSunsetArea();
				break;
			case 7:
				this.UpdateCutsceneForEnding(ref fade_out_time);
				break;
			case 9:
				this.UpdateCutsceneForMushroomArea();
				break;
			}
			if (this.cutsceneTick == fade_out_time)
			{
				this.screenDarkness = 0f;
				this.fadeDelta = 2f;
			}
			if (this.cutsceneTick == fade_out_time + 100)
			{
				this.EndCutscene();
				return;
			}
			if (this.player.velocity.X > 0f && this.player.position.X > (float)(this.screenWidth + this.tileSize))
			{
				if (!this.minecartLoop.IsPaused)
				{
					this.minecartLoop.Pause();
				}
				this.player.enabled = false;
			}
			if (this.player.velocity.X < 0f && this.player.position.X < (float)(-(float)this.tileSize))
			{
				if (!this.minecartLoop.IsPaused)
				{
					this.minecartLoop.Pause();
				}
				this.player.enabled = false;
			}
		}

		// Token: 0x0600260C RID: 9740 RVA: 0x001AB2B8 File Offset: 0x001A94B8
		public void UpdateCutSceneForBrownArea()
		{
			int num = this.cutsceneTick;
			if (num <= 150)
			{
				if (num <= 130)
				{
					if (num == 0)
					{
						MineCart.Roadblock roadblock = this.AddEntity<MineCart.Roadblock>(new MineCart.Roadblock());
						roadblock.position.X = (float)(6 * this.tileSize);
						roadblock.position.Y = (float)(10 * this.tileSize);
						MineCart.Roadblock roadblock2 = this.AddEntity<MineCart.Roadblock>(new MineCart.Roadblock());
						roadblock2.position.X = (float)(19 * this.tileSize);
						roadblock2.position.Y = (float)(10 * this.tileSize);
						return;
					}
					if (num != 130)
					{
						return;
					}
				}
				else
				{
					if (num == 140)
					{
						this.player.Jump();
						return;
					}
					if (num != 150)
					{
						return;
					}
					this.player.ReleaseJump();
					return;
				}
			}
			else if (num <= 190)
			{
				if (num != 160 && num != 190)
				{
					return;
				}
			}
			else
			{
				if (num == 270)
				{
					this.player.Jump();
					return;
				}
				if (num != 275)
				{
					return;
				}
				this.player.ReleaseJump();
				return;
			}
			this.AddEntity<MineCart.FallingBoulder>(new MineCart.FallingBoulder()).position = new Vector2(this.player.position.X + 100f, -16f);
		}

		// Token: 0x0600260D RID: 9741 RVA: 0x001AB400 File Offset: 0x001A9600
		public void UpdateCutsceneForFrostArea()
		{
			int num = this.cutsceneTick;
			if (num <= 200)
			{
				if (num <= 100)
				{
					if (num == 0)
					{
						this.AddTrack(2, 9, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(3, 8, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(4, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(5, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(6, 7, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(7, 8, MineCart.Track.TrackType.IceDownSlope);
						this.AddTrack(8, 9, MineCart.Track.TrackType.IceDownSlope);
						this.AddTrack(9, 10, MineCart.Track.TrackType.IceDownSlope);
						this.AddTrack(13, 9, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(17, 8, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(19, 10, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(21, 6, MineCart.Track.TrackType.UpSlope);
						this.AddTrack(24, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(25, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(26, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(27, 8, MineCart.Track.TrackType.Straight);
						this.AddTrack(28, 8, MineCart.Track.TrackType.Straight);
						return;
					}
					if (num != 100)
					{
						return;
					}
					this.player.Jump();
					return;
				}
				else
				{
					if (num == 130)
					{
						this.player.ReleaseJump();
						return;
					}
					if (num != 200)
					{
						return;
					}
					this.player.Jump();
					return;
				}
			}
			else if (num <= 260)
			{
				if (num == 215)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 260)
				{
					return;
				}
				this.player.Jump();
				return;
			}
			else
			{
				if (num == 270)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 304)
				{
					return;
				}
				this.player.Jump();
				return;
			}
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x001AB588 File Offset: 0x001A9788
		public void UpdateCutsceneForLavaArea()
		{
			int num = this.cutsceneTick;
			if (num <= 100)
			{
				if (num == 0)
				{
					this.AddTrack(1, 12, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(2, 11, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(3, 10, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(4, 9, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(5, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(6, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(7, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(8, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(9, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(10, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(11, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(12, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(13, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(14, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(15, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(16, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(17, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(18, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(19, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(20, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(21, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(22, 7, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(23, 6, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(24, 5, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(25, 4, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(26, 3, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(27, 2, MineCart.Track.TrackType.UpSlope);
					return;
				}
				if (num != 100)
				{
					return;
				}
				this.player.Jump();
				return;
			}
			else
			{
				if (num == 115)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 265)
				{
					return;
				}
				this.player.Jump();
				return;
			}
		}

		// Token: 0x0600260F RID: 9743 RVA: 0x001AB714 File Offset: 0x001A9914
		public void UpdateCutsceneForWaterArea()
		{
			int num = this.cutsceneTick;
			if (num <= 250)
			{
				if (num == 0)
				{
					this.AddEntity<MineCart.Whale>(new MineCart.Whale());
					this.AddEntity<MineCart.PlayerBubbleSpawner>(new MineCart.PlayerBubbleSpawner());
					return;
				}
				if (num != 250)
				{
					return;
				}
				this.player.velocity.X = 0f;
				using (List<MineCart.Entity>.Enumerator enumerator = this._entities.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						MineCart.Entity entity = enumerator.Current;
						MineCart.Whale whale = entity as MineCart.Whale;
						if (whale != null)
						{
							Game1.playSound("croak", null);
							whale.SetState(MineCart.Whale.CurrentState.OpenMouth, 1f);
							break;
						}
					}
					return;
				}
			}
			else if (num != 260)
			{
				if (num == 265)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 310)
				{
					return;
				}
				this.player.velocity.X = -100f;
				return;
			}
			this.player.Jump();
		}

		// Token: 0x06002610 RID: 9744 RVA: 0x001AB824 File Offset: 0x001A9A24
		public void UpdateCutsceneForDarkArea()
		{
			int num = this.cutsceneTick;
			if (num <= 150)
			{
				if (num == 0)
				{
					this.AddTrack(-1, 3, MineCart.Track.TrackType.Straight);
					this.AddTrack(0, 3, MineCart.Track.TrackType.Straight);
					this.AddTrack(1, 4, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(2, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(3, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(4, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(5, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(6, -2, MineCart.Track.TrackType.Straight);
					this.AddTrack(7, -2, MineCart.Track.TrackType.Straight);
					this.AddTrack(8, -2, MineCart.Track.TrackType.Straight);
					this.AddTrack(9, -2, MineCart.Track.TrackType.Straight);
					this.AddTrack(19, 9, MineCart.Track.TrackType.Straight);
					this.AddTrack(20, 9, MineCart.Track.TrackType.Straight);
					this.AddTrack(21, 8, MineCart.Track.TrackType.UpSlope);
					this.AddTrack(22, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(23, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(24, 9, MineCart.Track.TrackType.DownSlope);
					this.AddTrack(25, 9, MineCart.Track.TrackType.Straight);
					this.AddTrack(26, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(27, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(28, 8, MineCart.Track.TrackType.Straight);
					this.player.position.Y = (float)(3 * this.tileSize);
					MineCart.WillOWisp willOWisp = this.AddEntity<MineCart.WillOWisp>(new MineCart.WillOWisp());
					willOWisp.position.X = (float)(10 * this.tileSize);
					willOWisp.position.Y = (float)(5 * this.tileSize);
					willOWisp.visible = false;
					return;
				}
				if (num == 130)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num == 150)
				{
					this.player.Jump();
					return;
				}
			}
			else
			{
				if (num == 200)
				{
					this.player.Jump();
					return;
				}
				if (num == 215)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num == 300)
				{
					Game1.playSound("ghost", null);
					return;
				}
			}
			if (this.cutsceneTick >= 300 && this.cutsceneTick % 3 == 0 && this.cutsceneTick < 350)
			{
				foreach (MineCart.Entity entity in this._entities)
				{
					if (entity is MineCart.WillOWisp)
					{
						entity.visible = !entity.visible;
					}
				}
			}
			if (this.cutsceneTick == 350)
			{
				foreach (MineCart.Entity entity2 in this._entities)
				{
					if (entity2 is MineCart.WillOWisp)
					{
						entity2.visible = true;
					}
				}
			}
		}

		// Token: 0x06002611 RID: 9745 RVA: 0x001ABAD8 File Offset: 0x001A9CD8
		public void UpdateCutsceneForMushroomArea()
		{
			int num = this.cutsceneTick;
			if (num <= 120)
			{
				if (num == 0)
				{
					this.AddTrack(0, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(1, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(2, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(3, 6, MineCart.Track.TrackType.Straight);
					MineCart.Track spring_track = this.AddTrack(4, 6, MineCart.Track.TrackType.Straight);
					MineCart.MushroomSpring mushroomSpring = this.AddEntity<MineCart.MushroomSpring>(new MineCart.MushroomSpring());
					mushroomSpring.InitializeObstacle(spring_track);
					mushroomSpring.position = new Vector2(4.5f, 6f) * (float)this.tileSize;
					this.AddTrack(8, 6, MineCart.Track.TrackType.MushroomLeft);
					this.AddTrack(9, 6, MineCart.Track.TrackType.MushroomMiddle);
					this.AddTrack(10, 6, MineCart.Track.TrackType.MushroomRight);
					this.AddTrack(12, 10, MineCart.Track.TrackType.Straight);
					List<MineCart.BalanceTrack> track_parts = new List<MineCart.BalanceTrack>();
					MineCart.NoxiousMushroom noxiousMushroom = this.AddEntity<MineCart.NoxiousMushroom>(new MineCart.NoxiousMushroom());
					noxiousMushroom.position = new Vector2(12.5f, 10f) * (float)this.tileSize;
					noxiousMushroom.nextFire = 3f;
					MineCart.BalanceTrack track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomLeft, false);
					track_piece.position.X = (float)(15 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomMiddle, false);
					track_piece.position.X = (float)(16 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomRight, false);
					track_piece.position.X = (float)(17 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					List<MineCart.BalanceTrack> other_track_parts = new List<MineCart.BalanceTrack>();
					track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomLeft, false);
					track_piece.position.X = (float)(22 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					other_track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomMiddle, false);
					track_piece.position.X = (float)(23 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					other_track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					track_piece = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomRight, false);
					track_piece.position.X = (float)(24 * this.tileSize);
					track_piece.position.Y = (float)(9 * this.tileSize);
					other_track_parts.Add(track_piece);
					this.AddTrack(track_piece);
					foreach (MineCart.BalanceTrack balanceTrack in track_parts)
					{
						balanceTrack.connectedTracks = new List<MineCart.BalanceTrack>(track_parts);
						balanceTrack.counterBalancedTracks = new List<MineCart.BalanceTrack>(other_track_parts);
					}
					foreach (MineCart.BalanceTrack balanceTrack2 in other_track_parts)
					{
						balanceTrack2.connectedTracks = new List<MineCart.BalanceTrack>(other_track_parts);
						balanceTrack2.counterBalancedTracks = new List<MineCart.BalanceTrack>(track_parts);
					}
					this.player.position.Y = (float)(6 * this.tileSize);
					return;
				}
				if (num == 115)
				{
					this.player.Jump();
					return;
				}
				if (num != 120)
				{
					return;
				}
				this.player.ReleaseJump();
				return;
			}
			else
			{
				if (num == 230)
				{
					this.player.Jump();
					return;
				}
				if (num == 250)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 298)
				{
					return;
				}
				this.player.Jump();
				return;
			}
		}

		// Token: 0x06002612 RID: 9746 RVA: 0x001ABE7C File Offset: 0x001AA07C
		public void UpdateCutsceneForSunsetArea()
		{
			int num = this.cutsceneTick;
			if (num <= 129)
			{
				if (num == 0)
				{
					this.AddTrack(0, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(1, 3, MineCart.Track.TrackType.Straight);
					this.AddTrack(2, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(4, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(5, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(6, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(8, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(9, 1, MineCart.Track.TrackType.Straight);
					this.AddTrack(10, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(12, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(13, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(14, 6, MineCart.Track.TrackType.Straight);
					this.AddTrack(15, 8, MineCart.Track.TrackType.Straight);
					this.AddTrack(17, 4, MineCart.Track.TrackType.Straight);
					this.AddTrack(18, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(19, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(20, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(21, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(22, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(23, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(24, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(25, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(26, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(27, 2, MineCart.Track.TrackType.Straight);
					this.AddTrack(28, 2, MineCart.Track.TrackType.Straight);
					this.player.position.Y = (float)(6 * this.tileSize);
					return;
				}
				if (num != 129)
				{
					return;
				}
				this.player.Jump();
				return;
			}
			else
			{
				if (num == 170)
				{
					this.player.ReleaseJump();
					return;
				}
				if (num != 214)
				{
					return;
				}
				this.player.Jump();
				return;
			}
		}

		// Token: 0x06002613 RID: 9747 RVA: 0x001AC007 File Offset: 0x001AA207
		public void UpdateCutsceneForHeavenlyArea()
		{
			if (this.cutsceneTick == 100)
			{
				this.AddEntity<MineCart.HugeSlime>(new MineCart.HugeSlime());
				this.slimeBossPosition = -100f;
			}
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x001AC02C File Offset: 0x001AA22C
		public void UpdateCutsceneForEnding(ref int fadeOutTimer)
		{
			fadeOutTimer = 800;
			int num = this.cutsceneTick;
			if (num <= 200)
			{
				if (num != 0)
				{
					if (num == 200)
					{
						this.player.velocity.X = 40f;
						return;
					}
				}
				else
				{
					if (this.completelyPerfect)
					{
						this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 182, 48, 45), new Vector2((float)(20 * this.tileSize) + 12f, (float)(10 * this.tileSize) - 21.5f), 0f, 0f, 0f, 0f, 1000f, 1f, 1, 0f, 0.23f, true, 0f));
						return;
					}
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 112, 25, 32), new Vector2((float)(20 * this.tileSize) + 12f, (float)(10 * this.tileSize) - 16f), 0f, 0f, 0f, 0f, 1000f, 1f, 1, 0f, 0.23f, true, 0f));
					return;
				}
			}
			else
			{
				if (num == 250)
				{
					this.player.velocity.X = 20f;
					return;
				}
				if (num == 300)
				{
					this.player.velocity.X = 0f;
					return;
				}
			}
			if (this.cutsceneTick >= 350 && this.cutsceneTick % 10 == 0 && this.cutsceneTick < 600)
			{
				Game1.playSound("junimoMeep1", null);
				this.AddEntity<MineCart.EndingJunimo>(new MineCart.EndingJunimo(this.completelyPerfect)).position = new Vector2((float)(20 * this.tileSize), (float)(10 * this.tileSize));
			}
		}

		// Token: 0x06002615 RID: 9749 RVA: 0x001AC214 File Offset: 0x001AA414
		public void UpdateFruitsSummary(float time)
		{
			if (this.currentTheme == 7)
			{
				this.currentFruitCheckIndex = -1;
				this.ShowCutscene();
			}
			if (this.gamePaused)
			{
				return;
			}
			if (this.stateTimer >= 0f)
			{
				this.stateTimer -= time;
				if (this.stateTimer < 0f)
				{
					this.stateTimer = 0f;
				}
			}
			if (this.stateTimer == 0f)
			{
				if (this.livesLeft < 3 && this.gameMode == 3)
				{
					this.livesLeft++;
					this.stateTimer = 0.25f;
					Game1.playSound("coin", null);
					return;
				}
				if (this.lastLevelWasPerfect && this.perfectText == null && this.gameMode == 3)
				{
					this.perfectText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Lime, Color.White, true, 0.1, 2500, -1, 500, 0f);
					Game1.playSound("yoba", null);
				}
				if (this.currentFruitCheckIndex == -1)
				{
					this.fruitEatCount = 0;
					this.currentFruitCheckIndex = 0;
					this.stateTimer = 0.5f;
					return;
				}
				if (this.currentFruitCheckIndex >= 3)
				{
					this.perfectText = null;
					this.currentFruitCheckIndex = -1;
					this.ShowMap();
					return;
				}
				if (this._collectedFruit.Contains((MineCart.CollectableFruits)this.currentFruitCheckIndex))
				{
					this._collectedFruit.Remove((MineCart.CollectableFruits)this.currentFruitCheckIndex);
					Game1.playSound("newArtifact", new int?(this.currentFruitCheckIndex * 100));
					this.fruitEatCount++;
					if (this.fruitEatCount >= 3)
					{
						Game1.playSound("yoba", null);
						if (this.gameMode == 3)
						{
							this.livesLeft++;
						}
						else
						{
							this.score += 5000;
							this.UpdateScoreState();
						}
					}
				}
				else
				{
					Game1.playSound("sell", new int?(this.currentFruitCheckIndex * 100));
				}
				this.stateTimer = 0.5f;
				this.currentFruitCheckMagnitude = 3f;
				this.currentFruitCheckIndex++;
			}
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x001AC450 File Offset: 0x001AA650
		public void UpdateInput()
		{
			if (Game1.IsChatting || Game1.textEntry != null)
			{
				this._wasJustChatting = true;
				return;
			}
			if (this.gamePaused)
			{
				return;
			}
			bool buttonPressed = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton) || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Space) || Game1.input.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.input.GetGamePadState().IsButtonDown(Buttons.A) || Game1.input.GetGamePadState().IsButtonDown(Buttons.B);
			if (buttonPressed != this._buttonState)
			{
				this._buttonState = buttonPressed;
				if (this._buttonState)
				{
					switch (this.gameState)
					{
					case MineCart.GameStates.Title:
						if (this.pauseBeforeTitleFadeOutTimer == 0f && this.screenDarkness == 0f && this.fadeDelta <= 0f)
						{
							this.pauseBeforeTitleFadeOutTimer = 0.5f;
							Game1.playSound("junimoMeep1", null);
							if (this.titleScreenJunimo != null)
							{
								this.titleScreenJunimo.Destroy();
								this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(259, 492, 14, 20), new Vector2(this.screenLeftBound + (float)(this.screenWidth / 2) - 128f + 137f, (float)(this.screenHeight / 2 - 35 + 46)), 110f, -200f, 0f, 3f, 99999f, 1f, 1, 1f, 0.24f, false, 0f));
							}
							Stopwatch stopwatch = this.musicSW;
							if (stopwatch != null)
							{
								stopwatch.Stop();
							}
							this.musicSW = null;
						}
						return;
					case MineCart.GameStates.Map:
						this.fadeDelta = 1f;
						return;
					case MineCart.GameStates.Cutscene:
						this.EndCutscene();
						return;
					}
					MineCart.MineCartCharacter mineCartCharacter = this.player;
					if (mineCartCharacter != null)
					{
						mineCartCharacter.QueueJump();
					}
					this.isJumpPressed = true;
				}
				else if (!this.gamePaused)
				{
					MineCart.MineCartCharacter mineCartCharacter2 = this.player;
					if (mineCartCharacter2 != null)
					{
						mineCartCharacter2.ReleaseJump();
					}
					this.isJumpPressed = false;
				}
			}
			this._wasJustChatting = false;
		}

		// Token: 0x06002617 RID: 9751 RVA: 0x001AC6C4 File Offset: 0x001AA8C4
		public virtual bool CanPause()
		{
			MineCart.GameStates gameStates = this.gameState;
			return gameStates - MineCart.GameStates.Ingame <= 3;
		}

		// Token: 0x06002618 RID: 9752 RVA: 0x001AC6E4 File Offset: 0x001AA8E4
		public bool tick(GameTime time)
		{
			this.UpdateInput();
			float delta_time = (float)time.ElapsedGameTime.TotalSeconds;
			if (this.gamePaused)
			{
				delta_time = 0f;
			}
			if (!this.CanPause())
			{
				this.gamePaused = false;
			}
			this.shakeMagnitude = Utility.MoveTowards(this.shakeMagnitude, 0f, delta_time * 3f);
			this.currentFruitCheckMagnitude = Utility.MoveTowards(this.currentFruitCheckMagnitude, 0f, delta_time * 6f);
			this._totalTime += (double)delta_time;
			this.screenDarkness += this.fadeDelta * delta_time;
			if (this.screenDarkness < 0f)
			{
				this.screenDarkness = 0f;
			}
			if (this.screenDarkness > 1f)
			{
				this.screenDarkness = 1f;
			}
			switch (this.gameState)
			{
			case MineCart.GameStates.Title:
				if (this.pauseBeforeTitleFadeOutTimer > 0f)
				{
					this.pauseBeforeTitleFadeOutTimer -= 0.0166666f;
					if (this.pauseBeforeTitleFadeOutTimer <= 0f)
					{
						this.fadeDelta = 1f;
					}
				}
				if (this.fadeDelta >= 0f && this.screenDarkness >= 1f)
				{
					this.restartLevel(true);
					return false;
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 250, 5, 5), Utility.getRandomPositionInThisRectangle(new Rectangle((int)this.screenLeftBound + this.screenWidth / 2 - 128, this.screenHeight / 2 - 35, 256, 71), Game1.random), 100f, 0f, 0f, 0f, 0.6f, 1f, 6, 0.1f, 0.23f, false, 0f));
				}
				if (this.musicSW != null)
				{
					ICue currentSong = Game1.currentSong;
					if (((currentSong != null) ? currentSong.Name : null) == "junimoKart" && Game1.currentSong.IsPlaying && !this.musicSW.IsRunning)
					{
						this.musicSW.Start();
					}
				}
				if (this.titleScreenJunimo != null && !this.titleJunimoStartedBobbing)
				{
					Stopwatch stopwatch = this.musicSW;
					if (stopwatch != null && stopwatch.ElapsedMilliseconds >= 48000L)
					{
						this.titleScreenJunimo.reset(new Rectangle(417, 347, 14, 20), this.titleScreenJunimo.position, 100f, 0f, 0f, 0f, 9999f, 1f, 2, 0.25f, this.titleScreenJunimo.depth, false, 0f);
						this.titleJunimoStartedBobbing = true;
						break;
					}
				}
				if (this.titleScreenJunimo != null && this.titleJunimoStartedBobbing)
				{
					Stopwatch stopwatch2 = this.musicSW;
					if (stopwatch2 != null && stopwatch2.ElapsedMilliseconds >= 80000L)
					{
						this.titleScreenJunimo.reset(new Rectangle(259, 492, 14, 20), this.titleScreenJunimo.position, 100f, 0f, 0f, 0f, 99999f, 1f, 1, 1f, 0.24f, false, 0f);
						this.musicSW.Stop();
						this.musicSW = null;
					}
				}
				break;
			case MineCart.GameStates.FruitsSummary:
				this.UpdateFruitsSummary(delta_time);
				break;
			case MineCart.GameStates.Map:
				this.UpdateMapTick(delta_time);
				break;
			case MineCart.GameStates.Cutscene:
				if (!this.gamePaused)
				{
					delta_time = 0.0166666f;
				}
				this.UpdateCutsceneTick();
				if (!this.gamePaused)
				{
					this.cutsceneTick++;
				}
				break;
			}
			int delta_ms = (int)(delta_time * 1000f);
			for (int i = 0; i < this._entities.Count; i++)
			{
				if (this._entities[i] != null && this._entities[i].IsActive())
				{
					this._entities[i].Update(delta_time);
				}
			}
			if (this.deathTimer <= 0f && this.respawnCounter > 0)
			{
				for (int j = 0; j < this._entities.Count; j++)
				{
					this._entities[j].OnPlayerReset();
				}
			}
			this._entities.RemoveAll((MineCart.Entity entity) => entity != null && entity.ShouldReap());
			float old_screen_left_bound = this.screenLeftBound;
			if (this.gameState == MineCart.GameStates.Ingame)
			{
				this.secondsOnThisLevel += delta_time;
				if (this.screenDarkness >= 1f && this.gameOver)
				{
					if (this.gameMode == 3)
					{
						this.ShowTitle();
					}
					else
					{
						this.levelsBeat = 0;
						this.coinCount = 0;
						this.setUpTheme(0);
						this.restartLevel(true);
					}
					return false;
				}
				if (this.checkpointPositions.Count > 0)
				{
					int k = 0;
					while (k < this.checkpointPositions.Count && this.player.position.X >= (float)(this.checkpointPositions[k] * this.tileSize))
					{
						foreach (MineCart.Entity entity2 in this._entities)
						{
							MineCart.CheckpointIndicator indicator = entity2 as MineCart.CheckpointIndicator;
							if (indicator != null && (int)(indicator.position.X / (float)this.tileSize) == this.checkpointPositions[k])
							{
								indicator.Activate();
								break;
							}
						}
						this.checkpointPosition = ((float)this.checkpointPositions[k] + 0.5f) * (float)this.tileSize;
						this.ReapEntities();
						this.checkpointPositions.RemoveAt(k);
						k--;
						k++;
					}
				}
				float minimum_left_bound = 0f;
				if (this.gameState == MineCart.GameStates.Cutscene)
				{
					this.screenLeftBound = 0f;
				}
				else
				{
					if (this.deathTimer <= 0f && this.respawnCounter > 0)
					{
						if (this.screenLeftBound - Math.Max(this.player.position.X - 96f, minimum_left_bound) > 400f)
						{
							this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, 0f), 1200f * delta_time);
						}
						else if (this.screenLeftBound - Math.Max(this.player.position.X - 96f, minimum_left_bound) > 200f)
						{
							this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, minimum_left_bound), 600f * delta_time);
						}
						else
						{
							this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, minimum_left_bound), 300f * delta_time);
						}
						if (this.screenLeftBound < minimum_left_bound)
						{
							this.screenLeftBound = minimum_left_bound;
						}
					}
					else if (this.deathTimer <= 0f && (float)this.respawnCounter <= 0f && !this.reachedFinish)
					{
						this.screenLeftBound = this.player.position.X - 96f;
					}
					if (this.screenLeftBound < minimum_left_bound)
					{
						this.screenLeftBound = minimum_left_bound;
					}
				}
				if ((float)(this.generatorPosition.X * this.tileSize) <= this.screenLeftBound + (float)this.screenWidth + (float)(16 * this.tileSize))
				{
					this._GenerateMoreTrack();
				}
				int player_tile_position = (int)this.player.position.X / this.tileSize;
				if (this.respawnCounter <= 0)
				{
					if (player_tile_position > this._lastTilePosition)
					{
						int number_of_motions = player_tile_position - this._lastTilePosition;
						this._lastTilePosition = player_tile_position;
						for (int l = 0; l < number_of_motions; l++)
						{
							this.score += 10;
						}
					}
				}
				else if (this.respawnCounter > 0)
				{
					if (this.deathTimer > 0f)
					{
						this.deathTimer -= delta_time;
					}
					else if (this.screenLeftBound <= Math.Max(minimum_left_bound, this.player.position.X - 96f))
					{
						if (!this.player.enabled)
						{
							Utility.CollectGarbage("", 0);
						}
						this.player.enabled = true;
						this.respawnCounter -= delta_ms;
					}
				}
				if (this._goalIndicator != null && this.distanceToTravel != -1 && this.player.position.X >= this._goalIndicator.position.X && this.distanceToTravel != -1 && this.player.position.Y <= this._goalIndicator.position.Y * (float)this.tileSize + 4f && !this.reachedFinish && this.fadeDelta < 0f)
				{
					Game1.playSound("reward", null);
					this.levelThemesFinishedThisRun.Add(this.currentTheme);
					if (this.gameMode == 2)
					{
						this.score += 5000;
						this.UpdateScoreState();
					}
					foreach (MineCart.Entity entity3 in this._entities)
					{
						MineCart.GoalIndicator indicator2 = entity3 as MineCart.GoalIndicator;
						if (indicator2 != null)
						{
							indicator2.Activate();
						}
						else if (entity3 is MineCart.Coin || entity3 is MineCart.Fruit)
						{
							this.lastLevelWasPerfect = false;
						}
					}
					this.reachedFinish = true;
					this.fadeDelta = 1f;
				}
				if (this.score > this.currentHighScore)
				{
					this.currentHighScore = this.score;
				}
				if (this.scoreUpdateTimer <= 0f)
				{
					this.UpdateScoreState();
				}
				else
				{
					this.scoreUpdateTimer -= delta_time;
				}
				if (this.reachedFinish && Game1.random.NextDouble() < 0.25 && !this.gamePaused)
				{
					this.createSparkShower();
				}
				if (this.reachedFinish && this.screenDarkness >= 1f)
				{
					this.reachedFinish = false;
					if (this.gameMode != 3)
					{
						this.currentTheme = this.infiniteModeLevels[(this.levelsBeat + 1) % 8];
					}
					this.levelsBeat++;
					this.setUpTheme(this.currentTheme);
					this.restartLevel(false);
				}
				float death_buffer = 3f;
				if (this.currentTheme == 9)
				{
					death_buffer = 32f;
				}
				if (this.player.position.Y > (float)this.screenHeight + death_buffer)
				{
					this.Die();
				}
			}
			else if (this.gameState == MineCart.GameStates.FruitsSummary)
			{
				this.screenLeftBound = 0f;
				if (this.perfectText != null && this.perfectText.update(time))
				{
					this.perfectText = null;
				}
			}
			if (this.gameState == MineCart.GameStates.Title)
			{
				this.screenLeftBound += delta_time * 100f;
			}
			float parallax_scroll_speed = (this.screenLeftBound - old_screen_left_bound) / (float)this.tileSize;
			this.lakeSpeedAccumulator += (float)delta_ms * (parallax_scroll_speed / 4f) % 96f;
			this.backBGPosition += (float)delta_ms * (parallax_scroll_speed / 5f);
			this.backBGPosition = (this.backBGPosition + 9600f) % 96f;
			this.midBGPosition += (float)delta_ms * (parallax_scroll_speed / 4f);
			this.midBGPosition = (this.midBGPosition + 9600f) % 96f;
			this.waterFallPosition += (float)delta_ms * (parallax_scroll_speed * 6f / 5f);
			if (this.waterFallPosition > (float)(this.screenWidth * 3 / 2))
			{
				this.waterFallPosition %= (float)(this.screenWidth * 3 / 2);
				this.waterfallWidth = Game1.random.Next(6);
			}
			for (int m = this.sparkShower.Count - 1; m >= 0; m--)
			{
				this.sparkShower[m].dy += 0.105f * (delta_time / 0.0166666f);
				this.sparkShower[m].x += this.sparkShower[m].dx * (delta_time / 0.0166666f);
				this.sparkShower[m].y += this.sparkShower[m].dy * (delta_time / 0.0166666f);
				this.sparkShower[m].c.B = (byte)(0.0 + Math.Max(0.0, Math.Sin(this.totalTimeMS / (62.83185307179586 / (double)this.sparkShower[m].dx)) * 255.0));
				if (this.reachedFinish)
				{
					this.sparkShower[m].c.R = (byte)(0.0 + Math.Max(0.0, Math.Sin((this.totalTimeMS + 50.0) / (62.83185307179586 / (double)this.sparkShower[m].dx)) * 255.0));
					this.sparkShower[m].c.G = (byte)(0.0 + Math.Max(0.0, Math.Sin((this.totalTimeMS + 100.0) / (62.83185307179586 / (double)this.sparkShower[m].dx)) * 255.0));
					if (this.sparkShower[m].c.R == 0)
					{
						this.sparkShower[m].c.R = byte.MaxValue;
					}
					if (this.sparkShower[m].c.G == 0)
					{
						this.sparkShower[m].c.G = byte.MaxValue;
					}
				}
				if (this.sparkShower[m].y > (float)this.screenHeight)
				{
					this.sparkShower.RemoveAt(m);
				}
			}
			return false;
		}

		// Token: 0x06002619 RID: 9753 RVA: 0x001AD598 File Offset: 0x001AB798
		public void UpdateScoreState()
		{
			Game1.player.team.junimoKartStatus.UpdateState(this.score.ToString());
			this.scoreUpdateTimer = 1f;
		}

		// Token: 0x0600261A RID: 9754 RVA: 0x001AD5C4 File Offset: 0x001AB7C4
		public int GetValidCheckpointPosition(int x_pos)
		{
			int i = 0;
			while (i < 16)
			{
				if (this.GetTrackForXPosition((float)(x_pos * this.tileSize)) == null)
				{
					x_pos--;
					i++;
				}
				else
				{
					IL_46:
					while (i < 16)
					{
						if (this.GetTrackForXPosition((float)(x_pos * this.tileSize)) == null)
						{
							x_pos++;
							break;
						}
						x_pos--;
						i++;
					}
					if (this.GetTrackForXPosition((float)(x_pos * this.tileSize)) == null)
					{
						return this.furthestGeneratedCheckpoint;
					}
					int valid_x_pos = x_pos;
					int tile_y = (int)(this.GetTrackForXPosition((float)(x_pos * this.tileSize)).position.Y / (float)this.tileSize);
					x_pos++;
					int consecutive_valid_tracks = 0;
					for (i = 0; i < 16; i++)
					{
						MineCart.Track current_track = this.GetTrackForXPosition((float)(x_pos * this.tileSize));
						if (current_track == null)
						{
							return this.furthestGeneratedCheckpoint;
						}
						if (Math.Abs((int)(current_track.position.Y / (float)this.tileSize) - tile_y) <= 1)
						{
							consecutive_valid_tracks++;
							if (consecutive_valid_tracks >= 3)
							{
								return valid_x_pos;
							}
						}
						else
						{
							consecutive_valid_tracks = 0;
							valid_x_pos = x_pos;
							tile_y = (int)(this.GetTrackForXPosition((float)(x_pos * this.tileSize)).position.Y / (float)this.tileSize);
						}
						x_pos++;
					}
					return this.furthestGeneratedCheckpoint;
				}
			}
			goto IL_46;
		}

		// Token: 0x0600261B RID: 9755 RVA: 0x001AD6E4 File Offset: 0x001AB8E4
		public virtual void CollectFruit(MineCart.CollectableFruits fruit_type)
		{
			this._collectedFruit.Add(fruit_type);
			if (this.gameMode == 3)
			{
				this.CollectCoin(10);
				return;
			}
			this.score += 1000;
			this.UpdateScoreState();
		}

		// Token: 0x0600261C RID: 9756 RVA: 0x001AD720 File Offset: 0x001AB920
		public virtual void CollectCoin(int amount)
		{
			if (this.gameMode == 3)
			{
				this.coinCount += amount;
				if (this.coinCount >= 100)
				{
					Game1.playSound("yoba", null);
					int added_lives = this.coinCount / 100;
					this.coinCount %= 100;
					this.livesLeft += added_lives;
					return;
				}
			}
			else
			{
				this.score += 30;
				this.UpdateScoreState();
			}
		}

		// Token: 0x0600261D RID: 9757 RVA: 0x001AD7A0 File Offset: 0x001AB9A0
		public void submitHighScore()
		{
			if (Game1.player.team.junimoKartScores.GetScores()[0].Value < this.score)
			{
				Game1.multiplayer.globalChatInfoMessage("JunimoKartHighScore", new string[]
				{
					Game1.player.Name
				});
			}
			Game1.player.team.junimoKartScores.AddScore(Game1.player.name.Value, this.score);
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					Action<Farmer, int> onJKScoreAchieved = specialOrder.onJKScoreAchieved;
					if (onJKScoreAchieved != null)
					{
						onJKScoreAchieved(Game1.player, this.score);
					}
				}
			}
			this.RefreshHighScore();
		}

		// Token: 0x0600261E RID: 9758 RVA: 0x001AD89C File Offset: 0x001ABA9C
		public void Die()
		{
			if (this.respawnCounter > 0 || this.deathTimer > 0f)
			{
				return;
			}
			if (this.reachedFinish)
			{
				return;
			}
			if (this.player.enabled)
			{
				this.player.OnDie();
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(16, 96, 16, 16), this.player.position, (float)Game1.random.Next(-80, 81), (float)Game1.random.Next(-100, -49), 0f, 1f, 1f, 1f, 1, 0.1f, 0.45f, false, 0f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(32, 96, 16, 16), this.player.position + new Vector2(0f, -this.player.characterExtraHeight), (float)Game1.random.Next(-80, 81), (float)Game1.random.Next(-150, -99), 0.1f, 1f, 1f, 0.6666667f, 1, 0.1f, 0.45f, false, 0f)).SetColor(Color.Lime);
				this.player.position.Y = -1000f;
				Game1.playSound("fishEscape", null);
				this.player.enabled = false;
				this.lastLevelWasPerfect = false;
				this.completelyPerfect = false;
				if (this.gameState != MineCart.GameStates.Cutscene)
				{
					this.livesLeft--;
					if (this.gameMode != 3 || this.livesLeft < 0)
					{
						this.gameOver = true;
						this.fadeDelta = 1f;
						if (this.gameMode == 2)
						{
							this.submitHighScore();
							return;
						}
					}
					else
					{
						this.player.position.X = this.checkpointPosition;
						for (int i = 0; i < 6; i++)
						{
							MineCart.Track runway_track = this.GetTrackForXPosition((this.checkpointPosition / (float)this.tileSize + (float)i) * (float)this.tileSize);
							if (((runway_track != null) ? runway_track.obstacle : null) != null)
							{
								runway_track.obstacle.Destroy();
								runway_track.obstacle = null;
							}
						}
						this.player.SnapToFloor();
						this.deathTimer = 0.25f;
						this.respawnCounter = 1400;
					}
				}
			}
		}

		// Token: 0x0600261F RID: 9759 RVA: 0x001ADAF0 File Offset: 0x001ABCF0
		public void ReapEntities()
		{
			float reap_position = this.checkpointPosition - 96f - (float)(4 * this.tileSize);
			foreach (int grid_position in new List<int>(this._tracks.Keys))
			{
				if ((float)grid_position < reap_position / (float)this.tileSize)
				{
					for (int i = 0; i < this._tracks[grid_position].Count; i++)
					{
						MineCart.Track track = this._tracks[grid_position][i];
						this._entities.Remove(track);
					}
					this._tracks.Remove(grid_position);
				}
			}
		}

		// Token: 0x06002620 RID: 9760 RVA: 0x001ADBB8 File Offset: 0x001ABDB8
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002621 RID: 9761 RVA: 0x001ADBBA File Offset: 0x001ABDBA
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x06002622 RID: 9762 RVA: 0x001ADBBC File Offset: 0x001ABDBC
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x06002623 RID: 9763 RVA: 0x001ADBBE File Offset: 0x001ABDBE
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x001ADBC0 File Offset: 0x001ABDC0
		public void receiveKeyPress(Keys k)
		{
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals(Keys.Escape))
			{
				this.QuitGame();
				return;
			}
			if ((this.CanPause() && !Game1.options.gamepadControls && (k.Equals(Keys.P) || k.Equals(Keys.Enter))) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.Start)))
			{
				this.gamePaused = !this.gamePaused;
				if (this.gamePaused)
				{
					Game1.playSound("bigSelect", null);
					return;
				}
				Game1.playSound("bigDeSelect", null);
			}
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x001ADCA2 File Offset: 0x001ABEA2
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x001ADCA4 File Offset: 0x001ABEA4
		public void ResetState()
		{
			this.gameOver = false;
			this.screenLeftBound = 0f;
			this.respawnCounter = 0;
			this.deathTimer = 0f;
			this._spawnedFruit = new HashSet<MineCart.CollectableFruits>();
			this.sparkShower.Clear();
			this._goalIndicator = null;
			this.checkpointPositions = new List<int>();
			this._tracks = new Dictionary<int, List<MineCart.Track>>();
			this._entities = new List<MineCart.Entity>();
			this.player = this.AddEntity<MineCart.PlayerMineCartCharacter>(new MineCart.PlayerMineCartCharacter());
			this.player.position.X = 0f;
			this.player.position.Y = (float)(this.ytileOffset * this.tileSize);
			this.generatorPosition.X = 0;
			this.generatorPosition.Y = this.ytileOffset + 1;
			this._lastGenerator = null;
			this._trackGenerator = null;
			this._forcedNextGenerator = null;
			this.trackBuilderCharacter = this.AddEntity<MineCart.MineCartCharacter>(new MineCart.MineCartCharacter());
			this.trackBuilderCharacter.visible = false;
			this.trackBuilderCharacter.enabled = false;
			this._lastTilePosition = 0;
			this.pauseBeforeTitleFadeOutTimer = 0f;
			this.lakeDecor.Clear();
			this.obstacles.Clear();
			this.reachedFinish = false;
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x001ADDE4 File Offset: 0x001ABFE4
		public void QuitGame()
		{
			this.unload();
			Game1.playSound("bigDeSelect", null);
			Game1.currentMinigame = null;
		}

		// Token: 0x06002628 RID: 9768 RVA: 0x001ADE14 File Offset: 0x001AC014
		private void restartLevel(bool new_game = false)
		{
			if (new_game)
			{
				this.livesLeft = 3;
				this._collectedFruit.Clear();
				this.coinCount = 0;
				this.score = 0;
				this.levelsBeat = 0;
			}
			this.ResetState();
			if ((this.levelsBeat > 0 && this._collectedFruit.Count > 0) || (this.livesLeft < 3 && !new_game))
			{
				this.ShowFruitsSummary();
				return;
			}
			this.ShowMap();
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x001ADE84 File Offset: 0x001AC084
		public void ShowFruitsSummary()
		{
			Game1.changeMusicTrack("none", false, MusicContext.MiniGame);
			if (!this.minecartLoop.IsPaused)
			{
				this.minecartLoop.Pause();
			}
			this.gameState = MineCart.GameStates.FruitsSummary;
			this.player.enabled = false;
			this.stateTimer = 0.75f;
		}

		// Token: 0x0600262A RID: 9770 RVA: 0x001ADED4 File Offset: 0x001AC0D4
		public void ShowMap()
		{
			if (this.gameMode == 2)
			{
				this.ShowCutscene();
				return;
			}
			this.gameState = MineCart.GameStates.Map;
			this.mapTimer = 0f;
			this.screenDarkness = 1f;
			this.ResetState();
			this.player.enabled = false;
			Game1.changeMusicTrack("none", false, MusicContext.MiniGame);
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(261f, 106f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.15f, 0.2f, false, 0f)
			{
				ySinWaveMagnitude = (float)Game1.random.Next(1, 6)
			});
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(276f, 117f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.17f, 0.2f, false, 0f)
			{
				ySinWaveMagnitude = (float)Game1.random.Next(1, 6)
			});
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(234f, 136f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f, false, 0f)
			{
				ySinWaveMagnitude = (float)Game1.random.Next(1, 6)
			});
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(264f, 131f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f, false, 0f)
			{
				ySinWaveMagnitude = (float)Game1.random.Next(1, 6)
			});
			if (Game1.random.NextDouble() < 0.4)
			{
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(247f, 119f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f, false, 0f)
				{
					ySinWaveMagnitude = (float)Game1.random.Next(1, 6)
				});
			}
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(327f, 186f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.17f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(362f, 190f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(299f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.21f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(375f, 212f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.16f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(402, 660, 100, 72), new Vector2(205f, 184f), 0f, 0f, 0f, 0f, 99f, 1f, 2, 0.765f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 736, 48, 50), new Vector2(280f, 66f), 0f, 0f, 0f, 0f, 99f, 1f, 2, 0.765f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(402, 638, 3, 21), new Vector2(234.66f, 66.66f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.55f, false, 0f));
			switch (this.currentTheme)
			{
			case 0:
				this.AddEntity<MineCart.CosmeticFallingBoulder>(new MineCart.CosmeticFallingBoulder(72f, new Color(130, 96, 79), 96f, 0.45f)).position = new Vector2((float)(40 + Game1.random.Next(40)), -16f);
				if (Game1.random.NextBool())
				{
					this.AddEntity<MineCart.CosmeticFallingBoulder>(new MineCart.CosmeticFallingBoulder(72f, new Color(130, 96, 79), 80f, 0.5f)).position = new Vector2((float)(80 + Game1.random.Next(40)), -16f);
				}
				if (Game1.random.NextBool())
				{
					this.AddEntity<MineCart.CosmeticFallingBoulder>(new MineCart.CosmeticFallingBoulder(72f, new Color(130, 96, 79), 88f, 0.55f)).position = new Vector2((float)(120 + Game1.random.Next(40)), -16f);
				}
				break;
			case 1:
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(119f, 162f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.55f, false, 0f)).SetDestroySound("boulderBreak");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(49f, 166f), 0f, 0f, 0f, 0f, 1.2f, 1f, 1, 0.1f, 0.55f, false, 0f)).SetDestroySound("boulderBreak");
				for (int i = 0; i < 4; i++)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(421, 607, 5, 5), new Vector2(119f, 162f), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-50, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, false, 0.8f));
				}
				for (int j = 0; j < 4; j++)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(421, 607, 5, 5), new Vector2(49f, 166f), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-50, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, false, 1.2f));
				}
				break;
			case 2:
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(416, 368, 24, 16), new Vector2(217f, 177f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.54f, true, 0.8f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(416, 368, 1, 1), new Vector2(217f, 177f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.55f, false, 0f)).SetDestroySound("pullItemFromWater");
				break;
			case 3:
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(455, 512, 58, 64), new Vector2(250f, 136f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.21f, false, 0f)).SetDestroySound("barrelBreak");
				for (int k = 0; k < 32; k++)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(51, 53, 9, 9), new Vector2(250f, 136f) + new Vector2((float)Game1.random.Next(-20, 31), (float)Game1.random.Next(-20, 21)), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-70, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, false, 0.8f + 0.01f * (float)k));
				}
				break;
			case 4:
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(328f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 2.5f)).SetStartSound("fireball");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(336f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, false, 2.625f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 2.75f)).SetStartSound("fireball");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 189f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, false, 2.825f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 181f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 3f)).SetStartSound("fireball");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 173f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, false, 3.125f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 3.25f)).SetStartSound("fireball");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(352f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, false, 3.325f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 3.5f)).SetStartSound("fireball");
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 157f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, false, 3.625f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 149f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, false, 3.75f)).SetStartSound("fireball");
				break;
			case 5:
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(416, 384, 16, 16), new Vector2(213f, 34f), 0f, 0f, 0f, 0f, 5f, 1f, 6, 0.1f, 0.55f, false, 0f)).SetDestroySound("slimedead");
				for (int l = 0; l < 8; l++)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(427, 607, 6, 6), new Vector2((float)(205 + Game1.random.Next(3, 14)), (float)(26 + Game1.random.Next(6, 14))), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-60, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, false, 5f + (float)l * 0.005f));
				}
				break;
			case 6:
				for (int m = 0; m < 52; m++)
				{
					this.AddEntity<MineCart.CosmeticFallingBoulder>(new MineCart.CosmeticFallingBoulder((float)Game1.random.Next(72, 195), new Color(100, 66, 49), (float)(96 + Game1.random.Next(-10, 11)), 0.65f + (float)m * 0.05f)).position = new Vector2((float)(5 + Game1.random.Next(360)), -16f);
				}
				break;
			case 9:
				for (int n = 0; n < 8; n++)
				{
					this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(368, 784, 16, 16), new Vector2((float)(274 + Game1.random.Next(-19, 20)), (float)(46 + Game1.random.Next(6, 14))), (float)Game1.random.Next(-4, 5), -16f, 0f, 0.05f, 2f, 1f, 3, 0.33f, 0.35f, true, 1f + (float)n * 0.1f)).SetStartSound("dirtyHit");
				}
				break;
			}
			if (!this.levelThemesFinishedThisRun.Contains(1))
			{
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(119f, 162f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.1f, 0.55f, false, 0f));
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(49f, 166f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.1f, 0.55f, false, 0f));
			}
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(0) ? 630 : 650, 10, 9), new Vector2(88f, 87.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(1) ? 630 : 650, 10, 9), new Vector2(105f, 183.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(5) ? 630 : 640, 10, 9), new Vector2(169f, 119.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(4) ? 630 : 650, 10, 9), new Vector2(328f, 199.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f, false, 0f));
			this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(6) ? 630 : 650, 10, 9), new Vector2(361f, 72.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f, false, 0f));
			if (this.levelThemesFinishedThisRun.Contains(2))
			{
				this.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(466, 642, 17, 17), new Vector2(216.66f, 200.66f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.17f, 0.52f, false, 0f));
			}
			this.fadeDelta = -1f;
			MineCart.MapJunimo map_junimo = this.AddEntity<MineCart.MapJunimo>(new MineCart.MapJunimo());
			foreach (MineCart.LevelTransition transition in this.LEVEL_TRANSITIONS)
			{
				if (transition.startLevel == this.currentTheme && (transition.shouldTakePath == null || transition.shouldTakePath()))
				{
					map_junimo.position = new Vector2(((float)transition.startGridCoordinates.X + 0.5f) * (float)this.tileSize, ((float)transition.startGridCoordinates.Y + 0.5f) * (float)this.tileSize);
					map_junimo.moveString = transition.pathString;
					this.currentTheme = transition.destinationLevel;
					return;
				}
			}
		}

		// Token: 0x0600262B RID: 9771 RVA: 0x001AF36C File Offset: 0x001AD56C
		public void ShowCutscene()
		{
			this.gameState = MineCart.GameStates.Cutscene;
			this.screenDarkness = 1f;
			this.ResetState();
			this.player.enabled = false;
			this.setGameModeParameters();
			this.setUpTheme(this.currentTheme);
			this.cutsceneTick = 0;
			Game1.changeMusicTrack("none", false, MusicContext.MiniGame);
			for (int i = 0; i < this.screenWidth / this.tileSize + 4; i++)
			{
				this.AddTrack(i, 10, MineCart.Track.TrackType.Straight).visible = false;
			}
			this.player.SnapToFloor();
			if (this.gameMode == 2)
			{
				this.EndCutscene();
			}
		}

		// Token: 0x0600262C RID: 9772 RVA: 0x001AF408 File Offset: 0x001AD608
		public void PlayLevelMusic()
		{
			switch (this.currentTheme)
			{
			case 0:
				Game1.changeMusicTrack("EarthMine", false, MusicContext.MiniGame);
				return;
			case 1:
				Game1.changeMusicTrack("FrostMine", false, MusicContext.MiniGame);
				return;
			case 2:
				Game1.changeMusicTrack("junimoKart_whaleMusic", false, MusicContext.MiniGame);
				return;
			case 3:
				Game1.changeMusicTrack("junimoKart_ghostMusic", false, MusicContext.MiniGame);
				return;
			case 4:
				Game1.changeMusicTrack("tribal", false, MusicContext.MiniGame);
				return;
			case 5:
				Game1.changeMusicTrack("junimoKart_slimeMusic", false, MusicContext.MiniGame);
				return;
			case 6:
				Game1.changeMusicTrack("nightTime", false, MusicContext.MiniGame);
				return;
			case 7:
				break;
			case 8:
				Game1.changeMusicTrack("Upper_Ambient", false, MusicContext.MiniGame);
				break;
			case 9:
				Game1.changeMusicTrack("junimoKart_mushroomMusic", false, MusicContext.MiniGame);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600262D RID: 9773 RVA: 0x001AF4C0 File Offset: 0x001AD6C0
		public void EndCutscene()
		{
			if (!this.minecartLoop.IsPaused)
			{
				this.minecartLoop.Pause();
			}
			this.gameState = MineCart.GameStates.Ingame;
			Utility.CollectGarbage("", 0);
			this.ResetState();
			this.setUpTheme(this.currentTheme);
			this.PlayLevelMusic();
			this.player.enabled = true;
			this.createBeginningOfLevel();
			this.player.position.X = (float)this.tileSize * 0.5f;
			this.player.SnapToFloor();
			this.checkpointPosition = this.player.position.X;
			this.furthestGeneratedCheckpoint = 0;
			this.lastLevelWasPerfect = true;
			this.secondsOnThisLevel = 0f;
			if (this.currentTheme == 2)
			{
				this.AddEntity<MineCart.Whale>(new MineCart.Whale());
				this.AddEntity<MineCart.PlayerBubbleSpawner>(new MineCart.PlayerBubbleSpawner());
			}
			if (this.currentTheme == 5)
			{
				this.AddEntity<MineCart.HugeSlime>(new MineCart.HugeSlime()).position = new Vector2(0f, 0f);
			}
			this.screenDarkness = 1f;
			this.fadeDelta = -1f;
			if (this.gameMode == 3 && this.currentTheme == 7)
			{
				Game1.player.stats.Increment("completedJunimoKart", 1U);
				Game1.player.AddMissedMailAndRecipes();
				Game1.multiplayer.globalChatInfoMessage("JunimoKart", new string[]
				{
					Game1.player.Name
				});
				this.unload();
				Game1.globalFadeToClear(delegate
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineCart.cs.12106"));
				}, 0.015f);
				Game1.currentMinigame = null;
				DelayedAction.playSoundAfterDelay("discoverMineral", 1000, null, null, -1, false);
			}
		}

		// Token: 0x0600262E RID: 9774 RVA: 0x001AF684 File Offset: 0x001AD884
		public void createSparkShower(Vector2 position)
		{
			int number = Game1.random.Next(3, 7);
			for (int i = 0; i < number; i++)
			{
				this.sparkShower.Add(new MineCart.Spark(position.X - 3f, position.Y, (float)Game1.random.Next(-200, 5) / 100f, (float)(-(float)Game1.random.Next(5, 150)) / 100f));
			}
		}

		// Token: 0x0600262F RID: 9775 RVA: 0x001AF6FC File Offset: 0x001AD8FC
		public void createSparkShower()
		{
			int number = Game1.random.Next(3, 7);
			for (int i = 0; i < number; i++)
			{
				this.sparkShower.Add(new MineCart.Spark(this.player.drawnPosition.X - 3f, this.player.drawnPosition.Y, (float)Game1.random.Next(-200, 5) / 100f, (float)(-(float)Game1.random.Next(5, 150)) / 100f));
			}
		}

		// Token: 0x06002630 RID: 9776 RVA: 0x001AF788 File Offset: 0x001AD988
		public void CreateLakeDecor()
		{
			for (int i = 0; i < 16; i++)
			{
				this.lakeDecor.Add(new MineCart.LakeDecor(this, this.currentTheme, false, -1));
			}
		}

		// Token: 0x06002631 RID: 9777 RVA: 0x001AF7BC File Offset: 0x001AD9BC
		public void CreateBGDecor()
		{
			for (int i = 0; i < 16; i++)
			{
				this.lakeDecor.Add(new MineCart.LakeDecor(this, this.currentTheme, true, i));
			}
		}

		// Token: 0x06002632 RID: 9778 RVA: 0x001AF7F0 File Offset: 0x001AD9F0
		public void createBeginningOfLevel()
		{
			this.CreateLakeDecor();
			for (int i = 0; i < 15; i++)
			{
				this.AddTrack(this.generatorPosition.X, this.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				this.generatorPosition.X = this.generatorPosition.X + 1;
			}
		}

		// Token: 0x06002633 RID: 9779 RVA: 0x001AF840 File Offset: 0x001ADA40
		public void setGameModeParameters()
		{
			int num = this.gameMode;
			if (num != 2)
			{
				if (num == 3)
				{
					this.distanceToTravel = 350;
					return;
				}
			}
			else
			{
				this.distanceToTravel = 150;
			}
		}

		// Token: 0x06002634 RID: 9780 RVA: 0x001AF874 File Offset: 0x001ADA74
		public void AddValidObstacle(MineCart.ObstacleTypes obstacle_type, Type type)
		{
			if (this._validObstacles == null)
			{
				return;
			}
			List<Type> obstacleTypes;
			if (!this._validObstacles.TryGetValue(obstacle_type, out obstacleTypes))
			{
				obstacleTypes = (this._validObstacles[obstacle_type] = new List<Type>());
			}
			obstacleTypes.Add(type);
		}

		// Token: 0x06002635 RID: 9781 RVA: 0x001AF8B4 File Offset: 0x001ADAB4
		public void setUpTheme(int whichTheme)
		{
			this._generatorRolls = new List<MineCart.GeneratorRoll>();
			this._validObstacles = new Dictionary<MineCart.ObstacleTypes, List<Type>>();
			float additional_trap_spawn_rate = 0f;
			float movement_speed_multiplier = 1f;
			if (this.gameState == MineCart.GameStates.Cutscene)
			{
				additional_trap_spawn_rate = 0f;
				movement_speed_multiplier = 1f;
			}
			else if (this.gameMode == 2)
			{
				int cycle_completions = this.levelsBeat / this.infiniteModeLevels.Length;
				additional_trap_spawn_rate = (float)cycle_completions * 0.25f;
				movement_speed_multiplier = 1f + (float)cycle_completions * 0.25f;
			}
			this.midBGSource = new Rectangle(64, 0, 96, 162);
			this.backBGSource = new Rectangle(64, 162, 96, 111);
			this.lakeBGSource = new Rectangle(0, 80, 16, 97);
			this.backBGYOffset = this.tileSize * 2;
			this.midBGYOffset = 0;
			switch (whichTheme)
			{
			case 0:
				this.backBGTint = Color.DarkKhaki;
				this.midBGTint = Color.SandyBrown;
				this.caveTint = Color.SandyBrown;
				this.lakeTint = Color.MediumAquamarine;
				this.trackTint = Color.Beige;
				this.waterfallTint = Color.MediumAquamarine * 0.9f;
				this.trackShadowTint = new Color(60, 60, 60);
				this.player.velocity.X = 95f;
				NoiseGenerator.Amplitude = 2.0;
				NoiseGenerator.Frequency = 0.12;
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.Roadblock));
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.FallingBoulderSpawner));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 3).SetDepth(2, 2), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					-2,
					-1,
					1,
					2
				}).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.3f, new MineCart.SmallGapGenerator(this).SetLength(1, 1).SetDepth(-4, -2).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 4).SetDepth(-3, -3).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.5f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-3,
					-2,
					-1,
					2
				}).SetLength(2, 4).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -11, 0.3f + additional_trap_spawn_rate), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.015f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					-3,
					-4,
					4,
					3
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0.1f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 1).SetLength(3, 5).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -10, 0.3f + additional_trap_spawn_rate), null, null));
				this.generatorPosition.Y = this.screenHeight / this.tileSize - 3;
				break;
			case 1:
			{
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.Roadblock));
				this.AddValidObstacle(MineCart.ObstacleTypes.Difficult, typeof(MineCart.Roadblock));
				MineCart.BaseTrackGenerator wavy_generator = new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValueRange(-1, 1).SetLength(4, 4).SetCheckpoint(true);
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.3f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 4).SetReleaseJumpChance(0.1f).SetStaggerValues(new int[]
				{
					-2,
					-1
				}).SetTrackType(MineCart.Track.TrackType.UpSlope), new Func<bool>(this._IsGeneratingOnLowerHalf), wavy_generator));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.15f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 4).SetReleaseJumpChance(0.1f).SetStaggerValues(new int[]
				{
					3,
					2,
					1
				}).SetTrackType(MineCart.Track.TrackType.UpSlope), new Func<bool>(this._IsGeneratingOnUpperHalf), wavy_generator));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.5f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					1
				}).SetLength(3, 5).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.IceDownSlopesOnly)).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -12, 1f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.3f, wavy_generator, null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValueRange(-1, 1).SetLength(3, 6).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Difficult, -13, 0.5f + additional_trap_spawn_rate), null, null));
				this.backBGTint = new Color(93, 242, 255);
				this.midBGTint = Color.White;
				this.caveTint = new Color(230, 244, 254);
				this.lakeBGSource = new Rectangle(304, 0, 16, 0);
				this.lakeTint = new Color(147, 217, 255);
				this.midBGSource = new Rectangle(320, 135, 96, 149);
				this.midBGYOffset = -13;
				this.waterfallTint = Color.LightCyan * 0.5f;
				this.trackTint = new Color(186, 240, 255);
				this.player.velocity.X = 85f;
				NoiseGenerator.Amplitude = 2.8;
				NoiseGenerator.Frequency = 0.18;
				this.trackShadowTint = new Color(50, 145, 250);
				break;
			}
			case 2:
				this.backBGTint = Color.White;
				this.midBGTint = Color.White;
				this.caveTint = Color.SlateGray;
				this.lakeTint = new Color(75, 104, 88);
				this.waterfallTint = Color.White * 0f;
				this.trackTint = new Color(100, 220, 255);
				this.player.velocity.X = 85f;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.15;
				this.trackShadowTint = new Color(32, 45, 180);
				this.midBGSource = new Rectangle(416, 0, 96, 69);
				this.backBGSource = new Rectangle(320, 0, 96, 135);
				this.backBGYOffset = 0;
				this.lakeBGSource = new Rectangle(304, 0, 16, 0);
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(2, 5).SetDepth(-7, -3).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 3).SetDepth(100, 100), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					2,
					-1,
					0,
					1,
					2
				}).SetLength(3, 5).SetCheckpoint(true), null, null));
				this.CreateBGDecor();
				if (this.gameMode != 2)
				{
					this.distanceToTravel = 300;
				}
				break;
			case 3:
				this.backBGTint = new Color(60, 60, 60);
				this.midBGTint = new Color(60, 60, 60);
				this.caveTint = new Color(70, 70, 70);
				this.lakeTint = new Color(60, 70, 80);
				this.trackTint = Color.DimGray;
				this.waterfallTint = Color.Black * 0f;
				this.trackShadowTint = Color.Black;
				this.player.velocity.X = 120f;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.2;
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.Roadblock));
				this.AddValidObstacle(MineCart.ObstacleTypes.Difficult, typeof(MineCart.WillOWisp));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.SmallGapGenerator(this).SetLength(3, 5).SetDepth(-10, -6), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 3).SetDepth(3, 3), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					4,
					3
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-1,
					0,
					0,
					-1
				}).SetLength(7, 9).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Difficult, -10, 1f).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.EveryOtherTile)).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -13, 0.75f + additional_trap_spawn_rate), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					4,
					-1,
					0,
					1,
					-4
				}).SetLength(2, 6).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.EveryOtherTile)), null, null));
				if (this.gameMode != 2)
				{
					this.distanceToTravel = 450;
				}
				else
				{
					this.distanceToTravel = (int)((float)this.distanceToTravel * 1.5f);
				}
				this.CreateBGDecor();
				break;
			case 4:
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.FallingBoulderSpawner));
				this.backBGTint = new Color(255, 137, 82);
				this.midBGTint = new Color(255, 82, 40);
				this.caveTint = Color.DarkRed;
				this.lakeTint = Color.Red;
				this.lakeBGSource = new Rectangle(304, 97, 16, 97);
				this.trackTint = new Color(255, 160, 160);
				this.waterfallTint = Color.Red * 0.9f;
				this.trackShadowTint = Color.Orange;
				this.player.velocity.X = 120f;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.18;
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(3, 5).SetStaggerValues(new int[]
				{
					-3,
					-1,
					1,
					3
				}).SetReleaseJumpChance(0.33f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-1,
					1
				}).SetLength(5, 8).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)).SetCheckpoint(true).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -13, 0.5f + additional_trap_spawn_rate), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-1,
					1
				}).SetLength(5, 8).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)).SetCheckpoint(true).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -13, 0.5f + additional_trap_spawn_rate), null, null));
				break;
			case 5:
				this.AddValidObstacle(MineCart.ObstacleTypes.Air, typeof(MineCart.FallingBoulderSpawner));
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.Roadblock));
				this.backBGTint = new Color(180, 250, 180);
				this.midBGSource = new Rectangle(416, 69, 96, 162);
				this.midBGTint = Color.White;
				this.caveTint = new Color(255, 200, 60);
				this.lakeTint = new Color(24, 151, 62);
				this.trackTint = Color.LightSlateGray;
				this.waterfallTint = new Color(0, 255, 180) * 0.5f;
				this.trackShadowTint = new Color(0, 180, 50);
				this.player.velocity.X = 100f;
				this.slimeBossSpeed = this.player.velocity.X;
				NoiseGenerator.Amplitude = 3.1;
				NoiseGenerator.Frequency = 0.24;
				this.lakeBGSource = new Rectangle(304, 0, 16, 0);
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					10,
					10
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0.1f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(2, 5).SetDepth(-7, -3).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValueRange(-1, -1).SetLength(3, 5).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Air, -11, 0.75f + additional_trap_spawn_rate).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetStaggerValues(new int[]
				{
					1,
					-2
				}).SetNumberOfHops(2, 2).SetReleaseJumpChance(0.25f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)).SetTrackType(MineCart.Track.TrackType.SlimeUpSlope), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-1,
					-1,
					0,
					2,
					2
				}).SetLength(3, 5).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -10, 0.3f + additional_trap_spawn_rate), null, null));
				break;
			case 6:
				this.backBGTint = Color.White;
				this.midBGTint = Color.White;
				this.caveTint = Color.Black;
				this.lakeTint = Color.Black;
				this.waterfallTint = Color.BlueViolet * 0.25f;
				this.trackTint = new Color(150, 70, 120);
				this.player.velocity.X = 110f;
				NoiseGenerator.Amplitude = 3.5;
				NoiseGenerator.Frequency = 0.35;
				this.trackShadowTint = Color.Black;
				this.midBGSource = new Rectangle(416, 231, 96, 53);
				this.backBGSource = new Rectangle(320, 284, 96, 116);
				this.backBGYOffset = 20;
				this.AddValidObstacle(MineCart.ObstacleTypes.Normal, typeof(MineCart.Roadblock));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.RapidHopsGenerator(this).SetLength(3, 5).SetYStep(-1).AddPickupFunction<MineCart.RapidHopsGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.RapidHopsGenerator(this).SetLength(3, 5).SetYStep(2).SetChaotic(true).AddPickupFunction<MineCart.RapidHopsGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.RapidHopsGenerator(this).SetLength(3, 5).SetYStep(-2), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.05f, new MineCart.RapidHopsGenerator(this).SetLength(3, 5).SetYStep(3), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					4,
					3
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(3, 5).SetStaggerValues(new int[]
				{
					-3,
					-1,
					1,
					3
				}).SetReleaseJumpChance(0.33f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 2).SetLength(3, 8).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.EveryOtherTile)).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Normal, -10, 0.75f + additional_trap_spawn_rate), null, null));
				this.generatorPosition.Y = this.screenHeight / this.tileSize - 2;
				this.CreateBGDecor();
				if (this.gameMode != 2)
				{
					this.distanceToTravel = 500;
				}
				break;
			case 7:
				this.backBGTint = Color.DarkKhaki;
				this.midBGTint = Color.SandyBrown;
				this.caveTint = Color.SandyBrown;
				this.lakeTint = Color.MediumAquamarine;
				this.trackTint = Color.Beige;
				this.waterfallTint = Color.MediumAquamarine * 0.9f;
				this.trackShadowTint = new Color(60, 60, 60);
				this.player.velocity.X = 95f;
				break;
			case 8:
				this.backBGTint = new Color(10, 30, 50);
				this.midBGTint = Color.Black;
				this.caveTint = Color.Black;
				this.lakeTint = new Color(0, 60, 150);
				this.trackTint = new Color(0, 90, 180);
				this.waterfallTint = Color.MediumAquamarine * 0f;
				this.trackShadowTint = new Color(0, 0, 60);
				this.player.velocity.X = 100f;
				this.generatorPosition.Y = this.screenHeight / this.tileSize - 4;
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 3).SetDepth(2, 2).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					-2,
					-1,
					1,
					2
				}).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.3f, new MineCart.SmallGapGenerator(this).SetLength(1, 1).SetDepth(-4, -2).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.SmallGapGenerator(this).SetLength(1, 4).SetDepth(-3, -3).AddPickupFunction<MineCart.SmallGapGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.5f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-3,
					-2,
					-1,
					2
				}).SetLength(2, 4).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.015f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					-3,
					-4,
					4,
					3
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0.1f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 1).SetLength(3, 5).AddPickupFunction<MineCart.StraightAwayGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				if (this.gameMode != 2)
				{
					this.distanceToTravel = 200;
				}
				break;
			case 9:
				this.AddValidObstacle(MineCart.ObstacleTypes.Difficult, typeof(MineCart.NoxiousMushroom));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.1f, new MineCart.MushroomBalanceTrackGenerator(this).SetHopSize(2, 2).SetReleaseJumpChance(1f).SetStaggerValues(new int[]
				{
					0,
					-1,
					3
				}).SetTrackType(MineCart.Track.TrackType.Straight), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.15f, new MineCart.MushroomBalanceTrackGenerator(this).SetHopSize(1, 1).SetReleaseJumpChance(1f).SetStaggerValues(new int[]
				{
					-2,
					4
				}).SetTrackType(MineCart.Track.TrackType.Straight), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.2f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					-1,
					0,
					1
				}).SetLength(4, 4).SetCheckpoint(true), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(new int[]
				{
					4,
					3
				}).SetNumberOfHops(1, 1).SetReleaseJumpChance(0f), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.25f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(0f).SetLength(7, 7).AddObstacle<MineCart.StraightAwayGenerator>(MineCart.ObstacleTypes.Difficult, 3, 1f).SetCheckpoint(false), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.2f, new MineCart.MushroomBunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 3).SetStaggerValues(new int[]
				{
					-3,
					-1,
					2,
					3
				}).SetReleaseJumpChance(0.25f).AddPickupFunction<MineCart.MushroomBunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.05f, new MineCart.BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 3).SetStaggerValues(new int[]
				{
					-3,
					-1,
					2,
					3
				}).SetReleaseJumpChance(0.33f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.35f, new MineCart.BunnyHopGenerator(this).SetTrackType(MineCart.Track.TrackType.MushroomMiddle).SetHopSize(1, 1).SetNumberOfHops(2, 3).SetStaggerValues(new int[]
				{
					-3,
					-4,
					4
				}).SetReleaseJumpChance(0.33f).AddPickupFunction<MineCart.BunnyHopGenerator>(new Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>(MineCart.BaseTrackGenerator.Always)), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(0.5f, new MineCart.MushroomBalanceTrackGenerator(this).SetHopSize(1, 1).SetReleaseJumpChance(1f).SetStaggerValues(new int[]
				{
					-2,
					4
				}).SetTrackType(MineCart.Track.TrackType.Straight), null, null));
				this._generatorRolls.Add(new MineCart.GeneratorRoll(1f, new MineCart.StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(new int[]
				{
					2,
					-1,
					0,
					1,
					2
				}).SetLength(3, 5).SetCheckpoint(true), null, null));
				this.CreateBGDecor();
				this.backBGTint = Color.White;
				this.backBGSource = new Rectangle(0, 789, 96, 111);
				this.midBGTint = Color.White;
				this.caveTint = Color.Purple;
				this.lakeBGSource = new Rectangle(304, 0, 16, 0);
				this.lakeTint = new Color(0, 8, 46);
				this.midBGSource = new Rectangle(416, 736, 96, 149);
				this.midBGYOffset = -13;
				this.waterfallTint = new Color(100, 0, 140) * 0.5f;
				this.trackTint = new Color(130, 50, 230);
				this.player.velocity.X = 120f;
				this.trackShadowTint = new Color(0, 225, 225);
				break;
			}
			MineCart.MineCartCharacter mineCartCharacter = this.player;
			mineCartCharacter.velocity.X = mineCartCharacter.velocity.X * movement_speed_multiplier;
			this.trackBuilderCharacter.velocity = this.player.velocity;
			this.currentTheme = whichTheme;
		}

		// Token: 0x06002636 RID: 9782 RVA: 0x001B132A File Offset: 0x001AF52A
		public int KeepTileInBounds(int y)
		{
			if (y < this.topTile)
			{
				return 4;
			}
			if (y > this.bottomTile)
			{
				return this.bottomTile;
			}
			return y;
		}

		// Token: 0x06002637 RID: 9783 RVA: 0x001B1348 File Offset: 0x001AF548
		public bool IsTileInBounds(int y)
		{
			return y >= this.topTile && y <= this.bottomTile;
		}

		// Token: 0x06002638 RID: 9784 RVA: 0x001B1364 File Offset: 0x001AF564
		public T GetOverlap<T>(MineCart.ICollideable source) where T : MineCart.Entity
		{
			Rectangle source_rect = source.GetBounds();
			foreach (MineCart.Entity entity in this._entities)
			{
				if (entity.IsActive())
				{
					MineCart.ICollideable collideable_entity = entity as MineCart.ICollideable;
					if (collideable_entity != null)
					{
						T match = entity as T;
						if (match != null)
						{
							Rectangle other_rect = collideable_entity.GetBounds();
							if (source_rect.Intersects(other_rect))
							{
								return match;
							}
						}
					}
				}
			}
			return default(T);
		}

		// Token: 0x06002639 RID: 9785 RVA: 0x001B1408 File Offset: 0x001AF608
		public List<T> GetOverlaps<T>(MineCart.ICollideable source) where T : MineCart.Entity
		{
			List<T> overlaps = new List<T>();
			Rectangle source_rect = source.GetBounds();
			foreach (MineCart.Entity entity in this._entities)
			{
				if (entity.IsActive())
				{
					MineCart.ICollideable collideable_entity = entity as MineCart.ICollideable;
					if (collideable_entity != null)
					{
						T match = entity as T;
						if (match != null)
						{
							Rectangle other_rect = collideable_entity.GetBounds();
							if (source_rect.Intersects(other_rect))
							{
								overlaps.Add(match);
							}
						}
					}
				}
			}
			return overlaps;
		}

		// Token: 0x0600263A RID: 9786 RVA: 0x001B14A8 File Offset: 0x001AF6A8
		public MineCart.Pickup CreatePickup(Vector2 position, bool fruit_only = false)
		{
			if (position.Y < (float)this.tileSize && !fruit_only)
			{
				return null;
			}
			MineCart.Pickup pickup = null;
			int spawned_fruit = 0;
			int i = 0;
			while (i < 3 && this._spawnedFruit.Contains((MineCart.CollectableFruits)i))
			{
				spawned_fruit++;
				i++;
			}
			if (spawned_fruit <= 2)
			{
				float boundary_position = 0f;
				switch (spawned_fruit)
				{
				case 0:
					boundary_position = 0.15f * (float)this.distanceToTravel * (float)this.tileSize;
					break;
				case 1:
					boundary_position = 0.48f * (float)this.distanceToTravel * (float)this.tileSize;
					break;
				case 2:
					boundary_position = 0.81f * (float)this.distanceToTravel * (float)this.tileSize;
					break;
				}
				if (position.X >= boundary_position)
				{
					this._spawnedFruit.Add((MineCart.CollectableFruits)spawned_fruit);
					pickup = this.AddEntity<MineCart.Pickup>(new MineCart.Fruit((MineCart.CollectableFruits)spawned_fruit));
				}
			}
			if (pickup == null && !fruit_only)
			{
				pickup = this.AddEntity<MineCart.Pickup>(new MineCart.Coin());
			}
			if (pickup != null)
			{
				pickup.position = position;
			}
			return pickup;
		}

		// Token: 0x0600263B RID: 9787 RVA: 0x001B1594 File Offset: 0x001AF794
		public void draw(SpriteBatch b)
		{
			this._shakeOffset = new Vector2(Utility.Lerp(-this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()), Utility.Lerp(-this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()));
			if (this.gamePaused)
			{
				this._shakeOffset = Vector2.Zero;
			}
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			Game1.isUsingBackToFrontSorting = true;
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			Rectangle scissor_rect = new Rectangle((int)this.upperLeft.X, (int)this.upperLeft.Y, (int)((float)this.screenWidth * this.pixelScale), (int)((float)this.screenHeight * this.pixelScale));
			scissor_rect = Utility.ConstrainScissorRectToScreen(scissor_rect);
			b.GraphicsDevice.ScissorRectangle = scissor_rect;
			MineCart.GameStates gameStates = this.gameState;
			if (gameStates != MineCart.GameStates.FruitsSummary)
			{
				if (gameStates - MineCart.GameStates.Map > 1)
				{
					for (int i = 0; i <= this.screenWidth / this.tileSize + 1; i++)
					{
						b.Draw(this.texture, this.TransformDraw(new Rectangle(i * this.tileSize - (int)this.lakeSpeedAccumulator % this.tileSize, this.tileSize * 9, this.tileSize, this.screenHeight - 96)), new Rectangle?(this.lakeBGSource), this.lakeTint, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
					}
					for (int j = 0; j < this.lakeDecor.Count; j++)
					{
						this.lakeDecor[j].Draw(b);
					}
					for (int k = 0; k <= this.screenWidth / this.backBGSource.Width + 2; k++)
					{
						b.Draw(this.texture, this.TransformDraw(new Vector2(-this.backBGPosition + (float)(k * this.backBGSource.Width), (float)this.backBGYOffset)), new Rectangle?(this.backBGSource), this.backBGTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.7f);
					}
					for (int l = 0; l < this.screenWidth / this.midBGSource.Width + 2; l++)
					{
						b.Draw(this.texture, this.TransformDraw(new Vector2(-this.midBGPosition + (float)(l * this.midBGSource.Width), (float)(162 - this.midBGSource.Height + this.midBGYOffset))), new Rectangle?(this.midBGSource), this.midBGTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.6f);
					}
				}
			}
			else
			{
				SparklingText sparklingText = this.perfectText;
				if (sparklingText != null)
				{
					sparklingText.draw(b, this.TransformDraw(new Vector2(80f, 40f)));
				}
			}
			foreach (MineCart.Entity entity in this._entities)
			{
				if (entity.IsOnScreen())
				{
					entity.Draw(b);
				}
			}
			foreach (MineCart.Spark s in this.sparkShower)
			{
				b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle((int)s.x, (int)s.y, 1, 1)), null, s.c, 0f, Vector2.Zero, SpriteEffects.None, 0.3f);
			}
			switch (this.gameState)
			{
			case MineCart.GameStates.Title:
				b.Draw(this.texture, this.TransformDraw(new Vector2((float)(this.screenWidth / 2 - 128), (float)(this.screenHeight / 2 - 35))), new Rectangle?(new Rectangle(256, 409, 256, 71)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.25f);
				if (this.gameMode == 2)
				{
					Vector2 score_offset = new Vector2(125f, 0f);
					Vector2 draw_position = new Vector2((float)(this.screenWidth / 2) - score_offset.X / 2f, 155f);
					for (int m = 0; m < 5; m++)
					{
						if (m >= this._currentHighScores.Count)
						{
							break;
						}
						Color color = Color.White;
						if (m == 0)
						{
							color = Utility.GetPrismaticColor(0, 1f);
						}
						KeyValuePair<string, int> score = this._currentHighScores[m];
						int score_text_width = (int)Game1.dialogueFont.MeasureString(score.Value.ToString() ?? "").X / 4;
						b.DrawString(Game1.dialogueFont, "#" + (m + 1).ToString(), this.TransformDraw(draw_position), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
						b.DrawString(Game1.dialogueFont, score.Key, this.TransformDraw(draw_position + new Vector2(16f, 0f)), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
						b.DrawString(Game1.dialogueFont, score.Value.ToString() ?? "", this.TransformDraw(draw_position + score_offset - new Vector2((float)score_text_width, 0f)), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
						Vector2 shadow_offset = new Vector2(1f, 1f);
						b.DrawString(Game1.dialogueFont, "#" + (m + 1).ToString(), this.TransformDraw(draw_position + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
						b.DrawString(Game1.dialogueFont, score.Key, this.TransformDraw(draw_position + new Vector2(16f, 0f) + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
						b.DrawString(Game1.dialogueFont, score.Value.ToString() ?? "", this.TransformDraw(draw_position + score_offset - new Vector2((float)score_text_width, 0f) + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
						draw_position.Y += 10f;
					}
					goto IL_A1C;
				}
				goto IL_A1C;
			case MineCart.GameStates.FruitsSummary:
				goto IL_A1C;
			case MineCart.GameStates.Map:
				b.Draw(this.texture, this.TransformDraw(new Vector2(0f, 0f)), new Rectangle?(new Rectangle(0, 512, 400, 224)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.6f);
				if (!this.levelThemesFinishedThisRun.Contains(3))
				{
					b.Draw(this.texture, this.TransformDraw(new Vector2(221f, 104f)), new Rectangle?(new Rectangle(455, 512, 57, 64)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
				}
				b.Draw(this.texture, this.TransformDraw(new Vector2(369f, 51f)), new Rectangle?(new Rectangle(480, 579, 31, 32)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
				b.Draw(this.texture, this.TransformDraw(new Vector2(109f, 198f)), new Rectangle?(new Rectangle(420, 512, 25, 26)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
				b.Draw(this.texture, this.TransformDraw(new Vector2(229f, 213f)), new Rectangle?(new Rectangle(425, 541, 9, 11)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
				goto IL_A1C;
			case MineCart.GameStates.Cutscene:
			{
				float scale_adjustment = this.GetPixelScale() / 4f;
				b.DrawString(Game1.dialogueFont, this.cutsceneText, this.TransformDraw(new Vector2((float)(this.screenWidth / 2 - (int)(Game1.dialogueFont.MeasureString(this.cutsceneText).X / 2f / 4f)), 32f)), Color.White, 0f, Vector2.Zero, scale_adjustment, SpriteEffects.None, 0.199f);
				goto IL_A1C;
			}
			}
			for (int n = 0; n < this.waterfallWidth; n += 2)
			{
				for (int i2 = -2; i2 <= this.screenHeight / this.tileSize + 1; i2++)
				{
					b.Draw(this.texture, this.TransformDraw(new Vector2((float)(this.screenWidth + this.tileSize * n) - this.waterFallPosition, (float)(i2 * this.tileSize + (int)(this._totalTime * 48.0 + (double)(this.tileSize * 100)) % this.tileSize))), new Rectangle?(new Rectangle(48, 32, 16, 16)), this.waterfallTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.2f);
				}
			}
			IL_A1C:
			if (!this.gamePaused && (this.gameState == MineCart.GameStates.Ingame || this.gameState == MineCart.GameStates.Cutscene || this.gameState == MineCart.GameStates.FruitsSummary || this.gameState == MineCart.GameStates.Map))
			{
				this._shakeOffset = Vector2.Zero;
				Vector2 draw_position2 = new Vector2(4f, 4f);
				if (this.gameMode == 2)
				{
					string txtbestScore = Game1.content.LoadString("Strings\\StringsFromCSFiles:MineCart.cs.12115");
					b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
					b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.11f);
					draw_position2.Y += 10f;
					b.DrawString(Game1.dialogueFont, txtbestScore + this.currentHighScore.ToString(), this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
					b.DrawString(Game1.dialogueFont, txtbestScore + this.currentHighScore.ToString(), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.11f);
				}
				else
				{
					draw_position2.X = 4f;
					for (int i3 = 0; i3 < this.livesLeft; i3++)
					{
						b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(160, 32, 16, 16)), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(160, 32, 16, 16)), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.071f);
						draw_position2.X += 18f;
						if (draw_position2.X > 90f && i3 < this.livesLeft - 1)
						{
							draw_position2.X = 4f;
							draw_position2.Y += 18f;
						}
					}
					draw_position2.X = 4f;
					draw_position2.X += 36f;
					for (int i4 = this.livesLeft; i4 < 3; i4++)
					{
						b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(160, 48, 16, 16)), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(160, 48, 16, 16)), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.071f);
						draw_position2.X -= 18f;
					}
				}
				draw_position2.X = 4f;
				draw_position2.Y += 18f;
				for (int i5 = 0; i5 < 3; i5++)
				{
					Vector2 shake_magnitude = Vector2.Zero;
					if (this.currentFruitCheckMagnitude > 0f && i5 == this.currentFruitCheckIndex - 1)
					{
						shake_magnitude.X = Utility.Lerp(-this.currentFruitCheckMagnitude, this.currentFruitCheckMagnitude, (float)Game1.random.NextDouble());
						shake_magnitude.Y = Utility.Lerp(-this.currentFruitCheckMagnitude, this.currentFruitCheckMagnitude, (float)Game1.random.NextDouble());
					}
					if (this._collectedFruit.Contains((MineCart.CollectableFruits)i5))
					{
						b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude), new Rectangle?(new Rectangle(160 + i5 * 16, 0, 16, 16)), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f) + shake_magnitude), new Rectangle?(new Rectangle(160 + i5 * 16, 0, 16, 16)), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.075f);
					}
					else
					{
						b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude), new Rectangle?(new Rectangle(160 + i5 * 16, 16, 16, 16)), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(160 + i5 * 16, 16, 16, 16)), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.075f);
					}
					draw_position2.X += 18f;
				}
				if (this.gameMode == 3)
				{
					draw_position2.X = 4f;
					draw_position2.Y += 18f;
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(0, 272, 9, 11)), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(0, 272, 9, 11)), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.08f);
					draw_position2.X += 12f;
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.01f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				}
				if (Game1.IsMultiplayer)
				{
					string time_of_day_string = Game1.getTimeOfDayString(Game1.timeOfDay);
					draw_position2 = new Vector2((float)this.screenWidth - Game1.dialogueFont.MeasureString(time_of_day_string).X / 4f - 4f, 4f);
					Color timeColor = Color.White;
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2), timeColor, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.01f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
					b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				}
				if (this.gameState == MineCart.GameStates.Ingame)
				{
					float left_edge = (float)(this.screenWidth - 192) / 2f;
					float right_edge = left_edge + 192f;
					draw_position2 = new Vector2(left_edge, 4f);
					for (int i6 = 0; i6 < 12; i6++)
					{
						Rectangle source_rect = new Rectangle(192, 48, 16, 16);
						if (i6 == 0)
						{
							source_rect = new Rectangle(176, 48, 16, 16);
						}
						else if (i6 >= 11)
						{
							source_rect = new Rectangle(207, 48, 16, 16);
						}
						b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(source_rect), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.15f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(source_rect), Color.Black, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.17f);
						draw_position2.X += 16f;
					}
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(176, 64, 16, 16)), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.15f);
					draw_position2.X += 8f;
					string level_text = (this.levelsBeat + 1).ToString() ?? "";
					draw_position2.Y += 3f;
					b.DrawString(Game1.dialogueFont, level_text, this.TransformDraw(draw_position2 - new Vector2(Game1.dialogueFont.MeasureString(level_text).X / 2f / 4f, 0f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
					draw_position2.X += 1f;
					draw_position2.Y += 1f;
					draw_position2 = new Vector2(left_edge, 4f);
					if (this.player != null && this.player.visible)
					{
						draw_position2.X = Utility.Lerp(left_edge, right_edge, Math.Min(this.player.position.X / (float)(this.distanceToTravel * this.tileSize), 1f));
					}
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(240, 48, 16, 16)), Color.White, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.12f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(240, 48, 16, 16)), Color.Black, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.13f);
					if (this.checkpointPosition > (float)this.tileSize * 0.5f)
					{
						draw_position2.X = Utility.Lerp(left_edge, right_edge, this.checkpointPosition / (float)(this.distanceToTravel * this.tileSize));
						b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle?(new Rectangle(224, 48, 16, 16)), Color.White, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.125f);
						b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle?(new Rectangle(224, 48, 16, 16)), Color.Black, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.135f);
					}
				}
			}
			if (this.gameMode == 2 && Game1.IsMultiplayer && this.gameState != MineCart.GameStates.Title)
			{
				Game1.player.team.junimoKartStatus.Draw(b, this.TransformDraw(new Vector2(4f, (float)(this.screenHeight - 4))), this.GetPixelScale(), 0.01f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
			}
			if (this.screenDarkness > 0f)
			{
				b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight + this.tileSize)), null, Color.Black * this.screenDarkness, 0f, Vector2.Zero, SpriteEffects.None, 0.145f);
			}
			if (this.gamePaused)
			{
				b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight + this.tileSize)), null, Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0.145f);
				string current_text = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
				Vector2 draw_position3 = default(Vector2);
				draw_position3.X = (float)(this.screenWidth / 2);
				draw_position3.Y = (float)(this.screenHeight / 4);
				b.DrawString(Game1.dialogueFont, current_text, this.TransformDraw(draw_position3 - new Vector2(Game1.dialogueFont.MeasureString(current_text).X / 2f / 4f, 0f)), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
			}
			if (!Game1.options.hardwareCursor && !Game1.options.gamepadControls)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0.0001f);
			}
			b.End();
			Game1.isUsingBackToFrontSorting = false;
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		}

		// Token: 0x0600263C RID: 9788 RVA: 0x001B3314 File Offset: 0x001B1514
		public float GetPixelScale()
		{
			return this.pixelScale;
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x001B331C File Offset: 0x001B151C
		public Rectangle TransformDraw(Rectangle dest)
		{
			dest.X = (int)Math.Round((double)(((float)dest.X + this._shakeOffset.X) * this.pixelScale)) + (int)this.upperLeft.X;
			dest.Y = (int)Math.Round((double)(((float)dest.Y + this._shakeOffset.Y) * this.pixelScale)) + (int)this.upperLeft.Y;
			dest.Width = (int)((float)dest.Width * this.pixelScale);
			dest.Height = (int)((float)dest.Height * this.pixelScale);
			return dest;
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x001B33C0 File Offset: 0x001B15C0
		public static int Mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		// Token: 0x0600263F RID: 9791 RVA: 0x001B33CC File Offset: 0x001B15CC
		public Vector2 TransformDraw(Vector2 dest)
		{
			dest.X = (float)((int)Math.Round((double)((dest.X + this._shakeOffset.X) * this.pixelScale)) + (int)this.upperLeft.X);
			dest.Y = (float)((int)Math.Round((double)((dest.Y + this._shakeOffset.Y) * this.pixelScale)) + (int)this.upperLeft.Y);
			return dest;
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x001B3444 File Offset: 0x001B1644
		public void changeScreenSize()
		{
			this.screenWidth = 400;
			this.screenHeight = 220;
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			int viewport_width = Game1.game1.localMultiplayerWindow.Width;
			int viewport_height = Game1.game1.localMultiplayerWindow.Height;
			this.pixelScale = (float)Math.Min(5, (int)Math.Floor((double)Math.Min((float)(viewport_width / this.screenWidth) * pixel_zoom_adjustment, (float)(viewport_height / this.screenHeight) * pixel_zoom_adjustment)));
			this.upperLeft = new Vector2((float)(viewport_width / 2) * pixel_zoom_adjustment, (float)(viewport_height / 2) * pixel_zoom_adjustment);
			this.upperLeft.X = this.upperLeft.X - (float)(this.screenWidth / 2) * this.pixelScale;
			this.upperLeft.Y = this.upperLeft.Y - (float)(this.screenHeight / 2) * this.pixelScale;
			this.tileSize = 16;
			this.ytileOffset = this.screenHeight / 2 / this.tileSize;
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x001B353C File Offset: 0x001B173C
		public void unload()
		{
			Game1.stopMusicTrack(MusicContext.MiniGame);
			Game1.player.team.junimoKartStatus.WithdrawState();
			Game1.player.faceDirection(0);
			if (this.minecartLoop != null && this.minecartLoop.IsPlaying)
			{
				this.minecartLoop.Stop(AudioStopOptions.Immediate);
			}
		}

		// Token: 0x06002642 RID: 9794 RVA: 0x001B358F File Offset: 0x001B178F
		public bool forceQuit()
		{
			if (this.gameState != MineCart.GameStates.Cutscene && this.gameState != MineCart.GameStates.Title && this.gameMode == 2)
			{
				this.submitHighScore();
			}
			this.unload();
			return true;
		}

		// Token: 0x06002643 RID: 9795 RVA: 0x001B35B8 File Offset: 0x001B17B8
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x001B35BA File Offset: 0x001B17BA
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x001B35C1 File Offset: 0x001B17C1
		public string minigameId()
		{
			return "MineCart";
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x001B35C8 File Offset: 0x001B17C8
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x04001763 RID: 5987
		public MineCart.GameStates gameState;

		// Token: 0x04001764 RID: 5988
		public const int followDistance = 96;

		// Token: 0x04001765 RID: 5989
		public float pixelScale = 4f;

		// Token: 0x04001766 RID: 5990
		public const int tilesBeyondViewportToSimulate = 4;

		// Token: 0x04001767 RID: 5991
		public const int bgLoopWidth = 96;

		// Token: 0x04001768 RID: 5992
		public const float gravity = 0.21f;

		// Token: 0x04001769 RID: 5993
		public const int brownArea = 0;

		// Token: 0x0400176A RID: 5994
		public const int frostArea = 1;

		// Token: 0x0400176B RID: 5995
		public const int darkArea = 3;

		// Token: 0x0400176C RID: 5996
		public const int waterArea = 2;

		// Token: 0x0400176D RID: 5997
		public const int lavaArea = 4;

		// Token: 0x0400176E RID: 5998
		public const int heavenlyArea = 5;

		// Token: 0x0400176F RID: 5999
		public const int sunsetArea = 6;

		// Token: 0x04001770 RID: 6000
		public const int endingCutscene = 7;

		// Token: 0x04001771 RID: 6001
		public const int bonusLevel1 = 8;

		// Token: 0x04001772 RID: 6002
		public const int mushroomArea = 9;

		// Token: 0x04001773 RID: 6003
		public const int LAST_LEVEL = 6;

		// Token: 0x04001774 RID: 6004
		public readonly int[] infiniteModeLevels = new int[]
		{
			0,
			1,
			2,
			3,
			5,
			9,
			4,
			6
		};

		// Token: 0x04001775 RID: 6005
		public float shakeMagnitude;

		// Token: 0x04001776 RID: 6006
		protected Vector2 _shakeOffset = Vector2.Zero;

		// Token: 0x04001777 RID: 6007
		public const int infiniteMode = 2;

		// Token: 0x04001778 RID: 6008
		public const int progressMode = 3;

		// Token: 0x04001779 RID: 6009
		public const int respawnTime = 1400;

		// Token: 0x0400177A RID: 6010
		public static float maxJumpGraceTime = 0.1f;

		// Token: 0x0400177B RID: 6011
		public float slimeBossPosition = -100f;

		// Token: 0x0400177C RID: 6012
		public float slimeBossSpeed;

		// Token: 0x0400177D RID: 6013
		public float secondsOnThisLevel;

		// Token: 0x0400177E RID: 6014
		public int fruitEatCount;

		// Token: 0x0400177F RID: 6015
		public int currentFruitCheckIndex = -1;

		// Token: 0x04001780 RID: 6016
		public float currentFruitCheckMagnitude;

		// Token: 0x04001781 RID: 6017
		public const int checkpointScanDistance = 16;

		// Token: 0x04001782 RID: 6018
		public int coinCount;

		// Token: 0x04001783 RID: 6019
		public bool gamePaused;

		// Token: 0x04001784 RID: 6020
		private SparklingText perfectText;

		// Token: 0x04001785 RID: 6021
		private float lakeSpeedAccumulator;

		// Token: 0x04001786 RID: 6022
		private float backBGPosition;

		// Token: 0x04001787 RID: 6023
		private float midBGPosition;

		// Token: 0x04001788 RID: 6024
		private float waterFallPosition;

		// Token: 0x04001789 RID: 6025
		public Vector2 upperLeft;

		// Token: 0x0400178A RID: 6026
		private Stopwatch musicSW;

		// Token: 0x0400178B RID: 6027
		private bool titleJunimoStartedBobbing;

		// Token: 0x0400178C RID: 6028
		private bool lastLevelWasPerfect;

		// Token: 0x0400178D RID: 6029
		private bool completelyPerfect = true;

		// Token: 0x0400178E RID: 6030
		private int screenWidth;

		// Token: 0x0400178F RID: 6031
		private int screenHeight;

		// Token: 0x04001790 RID: 6032
		public int tileSize;

		// Token: 0x04001791 RID: 6033
		private int waterfallWidth = 1;

		// Token: 0x04001792 RID: 6034
		private int ytileOffset;

		// Token: 0x04001793 RID: 6035
		private int score;

		// Token: 0x04001794 RID: 6036
		private int levelsBeat;

		// Token: 0x04001795 RID: 6037
		private int gameMode;

		// Token: 0x04001796 RID: 6038
		private int livesLeft;

		// Token: 0x04001797 RID: 6039
		private int distanceToTravel = -1;

		// Token: 0x04001798 RID: 6040
		private int respawnCounter;

		// Token: 0x04001799 RID: 6041
		private int currentTheme;

		// Token: 0x0400179A RID: 6042
		private bool reachedFinish;

		// Token: 0x0400179B RID: 6043
		private bool gameOver;

		// Token: 0x0400179C RID: 6044
		private float screenDarkness;

		// Token: 0x0400179D RID: 6045
		protected string cutsceneText = "";

		// Token: 0x0400179E RID: 6046
		public float fadeDelta;

		// Token: 0x0400179F RID: 6047
		private ICue minecartLoop;

		// Token: 0x040017A0 RID: 6048
		private Texture2D texture;

		// Token: 0x040017A1 RID: 6049
		private Dictionary<int, List<MineCart.Track>> _tracks;

		// Token: 0x040017A2 RID: 6050
		private List<MineCart.LakeDecor> lakeDecor = new List<MineCart.LakeDecor>();

		// Token: 0x040017A3 RID: 6051
		private List<Point> obstacles = new List<Point>();

		// Token: 0x040017A4 RID: 6052
		private List<MineCart.Spark> sparkShower = new List<MineCart.Spark>();

		// Token: 0x040017A5 RID: 6053
		private List<int> levelThemesFinishedThisRun = new List<int>();

		// Token: 0x040017A6 RID: 6054
		private Color backBGTint;

		// Token: 0x040017A7 RID: 6055
		private Color midBGTint;

		// Token: 0x040017A8 RID: 6056
		private Color caveTint;

		// Token: 0x040017A9 RID: 6057
		private Color lakeTint;

		// Token: 0x040017AA RID: 6058
		private Color waterfallTint;

		// Token: 0x040017AB RID: 6059
		private Color trackShadowTint;

		// Token: 0x040017AC RID: 6060
		private Color trackTint;

		// Token: 0x040017AD RID: 6061
		private Rectangle midBGSource = new Rectangle(64, 0, 96, 162);

		// Token: 0x040017AE RID: 6062
		private Rectangle backBGSource = new Rectangle(64, 162, 96, 111);

		// Token: 0x040017AF RID: 6063
		private Rectangle lakeBGSource = new Rectangle(0, 80, 16, 97);

		// Token: 0x040017B0 RID: 6064
		private int backBGYOffset;

		// Token: 0x040017B1 RID: 6065
		private int midBGYOffset;

		// Token: 0x040017B2 RID: 6066
		protected double _totalTime;

		// Token: 0x040017B3 RID: 6067
		private MineCart.MineCartCharacter player;

		// Token: 0x040017B4 RID: 6068
		private MineCart.MineCartCharacter trackBuilderCharacter;

		// Token: 0x040017B5 RID: 6069
		private MineCart.MineDebris titleScreenJunimo;

		// Token: 0x040017B6 RID: 6070
		private List<MineCart.Entity> _entities;

		// Token: 0x040017B7 RID: 6071
		public MineCart.LevelTransition[] LEVEL_TRANSITIONS;

		// Token: 0x040017B8 RID: 6072
		protected MineCart.BaseTrackGenerator _lastGenerator;

		// Token: 0x040017B9 RID: 6073
		protected MineCart.BaseTrackGenerator _forcedNextGenerator;

		// Token: 0x040017BA RID: 6074
		public float screenLeftBound;

		// Token: 0x040017BB RID: 6075
		public Point generatorPosition;

		// Token: 0x040017BC RID: 6076
		private MineCart.BaseTrackGenerator _trackGenerator;

		// Token: 0x040017BD RID: 6077
		protected MineCart.GoalIndicator _goalIndicator;

		// Token: 0x040017BE RID: 6078
		public int bottomTile;

		// Token: 0x040017BF RID: 6079
		public int topTile;

		// Token: 0x040017C0 RID: 6080
		public float deathTimer;

		// Token: 0x040017C1 RID: 6081
		protected int _lastTilePosition = -1;

		// Token: 0x040017C2 RID: 6082
		public int slimeResetPosition = -80;

		// Token: 0x040017C3 RID: 6083
		public float checkpointPosition;

		// Token: 0x040017C4 RID: 6084
		public int furthestGeneratedCheckpoint;

		// Token: 0x040017C5 RID: 6085
		public bool isJumpPressed;

		// Token: 0x040017C6 RID: 6086
		public float stateTimer;

		// Token: 0x040017C7 RID: 6087
		public int cutsceneTick;

		// Token: 0x040017C8 RID: 6088
		public float pauseBeforeTitleFadeOutTimer;

		// Token: 0x040017C9 RID: 6089
		public float mapTimer;

		// Token: 0x040017CA RID: 6090
		private List<KeyValuePair<string, int>> _currentHighScores;

		// Token: 0x040017CB RID: 6091
		private int currentHighScore;

		// Token: 0x040017CC RID: 6092
		public float scoreUpdateTimer;

		// Token: 0x040017CD RID: 6093
		protected HashSet<MineCart.CollectableFruits> _spawnedFruit;

		// Token: 0x040017CE RID: 6094
		protected HashSet<MineCart.CollectableFruits> _collectedFruit;

		// Token: 0x040017CF RID: 6095
		public List<int> checkpointPositions;

		// Token: 0x040017D0 RID: 6096
		protected Dictionary<MineCart.ObstacleTypes, List<Type>> _validObstacles;

		// Token: 0x040017D1 RID: 6097
		protected List<MineCart.GeneratorRoll> _generatorRolls;

		// Token: 0x040017D2 RID: 6098
		private bool _trackAddedFlip;

		// Token: 0x040017D3 RID: 6099
		protected bool _buttonState;

		// Token: 0x040017D4 RID: 6100
		public bool _wasJustChatting;

		// Token: 0x020005AB RID: 1451
		[XmlType("MineCart.GameStates")]
		public enum GameStates
		{
			// Token: 0x04002CA7 RID: 11431
			Title,
			// Token: 0x04002CA8 RID: 11432
			Ingame,
			// Token: 0x04002CA9 RID: 11433
			FruitsSummary,
			// Token: 0x04002CAA RID: 11434
			Map,
			// Token: 0x04002CAB RID: 11435
			Cutscene
		}

		// Token: 0x020005AC RID: 1452
		public class LevelTransition
		{
			// Token: 0x06004232 RID: 16946 RVA: 0x0031090D File Offset: 0x0030EB0D
			public LevelTransition(int start_level, int destination_level, int start_grid_x, int start_grid_y, string path_string, Func<bool> should_take_path = null)
			{
				this.startLevel = start_level;
				this.destinationLevel = destination_level;
				this.startGridCoordinates = new Point(start_grid_x, start_grid_y);
				this.pathString = path_string;
				this.shouldTakePath = should_take_path;
			}

			// Token: 0x04002CAC RID: 11436
			public int startLevel;

			// Token: 0x04002CAD RID: 11437
			public int destinationLevel;

			// Token: 0x04002CAE RID: 11438
			public Point startGridCoordinates;

			// Token: 0x04002CAF RID: 11439
			public string pathString = "";

			// Token: 0x04002CB0 RID: 11440
			public Func<bool> shouldTakePath;
		}

		// Token: 0x020005AD RID: 1453
		public enum CollectableFruits
		{
			// Token: 0x04002CB2 RID: 11442
			Cherry,
			// Token: 0x04002CB3 RID: 11443
			Orange,
			// Token: 0x04002CB4 RID: 11444
			Grape,
			// Token: 0x04002CB5 RID: 11445
			MAX
		}

		// Token: 0x020005AE RID: 1454
		public enum ObstacleTypes
		{
			// Token: 0x04002CB7 RID: 11447
			Normal,
			// Token: 0x04002CB8 RID: 11448
			Air,
			// Token: 0x04002CB9 RID: 11449
			Difficult
		}

		// Token: 0x020005AF RID: 1455
		public class GeneratorRoll
		{
			// Token: 0x06004233 RID: 16947 RVA: 0x0031094C File Offset: 0x0030EB4C
			public GeneratorRoll(float generator_chance, MineCart.BaseTrackGenerator track_generator, Func<bool> additional_generation_condition = null, MineCart.BaseTrackGenerator forced_next_generator = null)
			{
				this.chance = generator_chance;
				this.generator = track_generator;
				this.forcedNextGenerator = forced_next_generator;
				this.additionalGenerationCondition = additional_generation_condition;
			}

			// Token: 0x04002CBA RID: 11450
			public float chance;

			// Token: 0x04002CBB RID: 11451
			public MineCart.BaseTrackGenerator generator;

			// Token: 0x04002CBC RID: 11452
			public Func<bool> additionalGenerationCondition;

			// Token: 0x04002CBD RID: 11453
			public MineCart.BaseTrackGenerator forcedNextGenerator;
		}

		// Token: 0x020005B0 RID: 1456
		public class MapJunimo : MineCart.Entity
		{
			// Token: 0x06004234 RID: 16948 RVA: 0x00310971 File Offset: 0x0030EB71
			public void StartMoving()
			{
				this.moveState = MineCart.MapJunimo.MoveState.Moving;
			}

			// Token: 0x06004235 RID: 16949 RVA: 0x0031097C File Offset: 0x0030EB7C
			protected override void _Update(float time)
			{
				int desired_direction = this.direction;
				this.isOnWater = false;
				if (this.position.X > 194f && this.position.X < 251f && this.position.Y > 165f)
				{
					this.isOnWater = true;
					this._game.minecartLoop.Pause();
				}
				if (this.moveString.Length > 0)
				{
					char c = this.moveString[0];
					if (c <= 'l')
					{
						if (c != 'd')
						{
							if (c == 'l')
							{
								desired_direction = 3;
							}
						}
						else
						{
							desired_direction = 2;
						}
					}
					else if (c != 'r')
					{
						if (c == 'u')
						{
							desired_direction = 0;
						}
					}
					else
					{
						desired_direction = 1;
					}
				}
				if (this.moveState == MineCart.MapJunimo.MoveState.Idle && !this._game.minecartLoop.IsPaused)
				{
					this._game.minecartLoop.Pause();
				}
				if (this.moveState == MineCart.MapJunimo.MoveState.Moving)
				{
					this.nextBump -= time;
					this.bumpHeight = Utility.MoveTowards(this.bumpHeight, 0f, time * 5f);
					if (this.nextBump <= 0f)
					{
						this.nextBump = Utility.RandomFloat(0.1f, 0.3f, null);
						this.bumpHeight = -2f;
					}
					if (!this.isOnWater && this._game.minecartLoop.IsPaused)
					{
						this._game.minecartLoop.Resume();
					}
					if (this.pixelsToMove <= 0f)
					{
						if (desired_direction != this.direction)
						{
							this.direction = desired_direction;
							if (!this.isOnWater)
							{
								Game1.playSound("parry", null);
								this._game.createSparkShower(this.position);
							}
							else
							{
								Game1.playSound("waterSlosh", null);
							}
						}
						if (this.moveString.Length > 0)
						{
							this.pixelsToMove = 16f;
							this.moveString = this.moveString.Substring(1);
						}
						else
						{
							this.moveState = MineCart.MapJunimo.MoveState.Finished;
							this.direction = 2;
							if (this.position.X < 368f)
							{
								if (!this.isOnWater)
								{
									Game1.playSound("parry", null);
									this._game.createSparkShower(this.position);
								}
								else
								{
									Game1.playSound("waterSlosh", null);
								}
							}
						}
					}
					if (this.pixelsToMove > 0f)
					{
						float pixels_to_move_now = Math.Min(this.pixelsToMove, this.moveSpeed * time);
						Vector2 direction_to_move = Vector2.Zero;
						switch (this.direction)
						{
						case 0:
							direction_to_move.Y = -1f;
							break;
						case 1:
							direction_to_move.X = 1f;
							break;
						case 2:
							direction_to_move.Y = 1f;
							break;
						case 3:
							direction_to_move.X = -1f;
							break;
						}
						this.position += direction_to_move * pixels_to_move_now;
						this.pixelsToMove -= pixels_to_move_now;
					}
				}
				else
				{
					this.bumpHeight = -2f;
				}
				if (this.moveState == MineCart.MapJunimo.MoveState.Finished && !this._game.minecartLoop.IsPaused)
				{
					this._game.minecartLoop.Pause();
				}
				base._Update(time);
			}

			// Token: 0x06004236 RID: 16950 RVA: 0x00310CBC File Offset: 0x0030EEBC
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				Rectangle source_rect = new Rectangle(400, 512, 16, 16);
				int num = this.direction;
				if (num != 0)
				{
					if (num != 2)
					{
						source_rect.Y = 528;
						if (this.direction == 3)
						{
							effect = SpriteEffects.FlipHorizontally;
						}
					}
					else
					{
						source_rect.Y = 512;
					}
				}
				else
				{
					source_rect.Y = 544;
				}
				if (this.isOnWater)
				{
					source_rect.Height -= 3;
					b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + new Vector2(0f, -1f) + new Vector2(0f, 1f) * this.bumpHeight), new Rectangle?(source_rect), Color.White, 0f, new Vector2(8f, 8f), this._game.GetPixelScale(), effect, 0.45f);
					b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + new Vector2(2f, 10f) + new Vector2(0f, 1f) * this.bumpHeight), new Rectangle?(new Rectangle(414, 624, 13, 5)), Color.White, 0f, new Vector2(8f, 8f), this._game.GetPixelScale(), effect, 0.44f);
					return;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + new Vector2(0f, -1f) + new Vector2(0f, 1f) * this.bumpHeight), new Rectangle?(source_rect), Color.White, 0f, new Vector2(8f, 8f), this._game.GetPixelScale(), effect, 0.45f);
			}

			// Token: 0x04002CBE RID: 11454
			public int direction = 2;

			// Token: 0x04002CBF RID: 11455
			public string moveString = "";

			// Token: 0x04002CC0 RID: 11456
			public float moveSpeed = 60f;

			// Token: 0x04002CC1 RID: 11457
			public float pixelsToMove;

			// Token: 0x04002CC2 RID: 11458
			public MineCart.MapJunimo.MoveState moveState;

			// Token: 0x04002CC3 RID: 11459
			public float nextBump;

			// Token: 0x04002CC4 RID: 11460
			public float bumpHeight;

			// Token: 0x04002CC5 RID: 11461
			private bool isOnWater;

			// Token: 0x02000752 RID: 1874
			public enum MoveState
			{
				// Token: 0x040031AA RID: 12714
				Idle,
				// Token: 0x040031AB RID: 12715
				Moving,
				// Token: 0x040031AC RID: 12716
				Finished
			}
		}

		// Token: 0x020005B1 RID: 1457
		public class LakeDecor
		{
			// Token: 0x06004238 RID: 16952 RVA: 0x00310F00 File Offset: 0x0030F100
			public LakeDecor(MineCart game, int theme = -1, bool bgDecor = false, int forceXPosition = -1)
			{
				this._game = game;
				this._position = new Point(Game1.random.Next(0, this._game.screenWidth), Game1.random.Next(160, this._game.screenHeight));
				if (forceXPosition != -1)
				{
					this._position.X = forceXPosition * (this._game.screenWidth / 16) + Game1.random.Next(0, this._game.screenWidth / 16);
				}
				this._bgDecor = bgDecor;
				this.spriteIndex = Game1.random.Next(2);
				switch (theme)
				{
				case 1:
					this.spriteIndex += 3;
					break;
				case 2:
					this.spriteIndex = 2;
					break;
				case 4:
					this.spriteIndex = 14;
					this._animationFrames = 6;
					break;
				case 5:
					this.spriteIndex += 5;
					break;
				case 6:
					this.spriteIndex = 1;
					break;
				case 9:
					this.spriteIndex += 7;
					break;
				}
				if (bgDecor)
				{
					this.spriteIndex += 7;
					this._position.Y = Game1.random.Next(0, this._game.screenHeight / 3);
					switch (theme)
					{
					case 2:
						if (forceXPosition % 5 == 0)
						{
							this.spriteIndex++;
							this._animationFrames = 4;
							return;
						}
						break;
					case 3:
						this.spriteIndex = 24;
						this._animationFrames = 4;
						return;
					case 4:
					case 5:
						break;
					case 6:
						this.spriteIndex = 20;
						this._position.Y = Game1.random.Next(0, this._game.screenHeight / 5);
						this._animationFrames = 4;
						return;
					default:
						if (theme != 9)
						{
							return;
						}
						this.spriteIndex = 28;
						this._animationFrames = 4;
						break;
					}
				}
			}

			// Token: 0x06004239 RID: 16953 RVA: 0x003110FC File Offset: 0x0030F2FC
			public void Draw(SpriteBatch b)
			{
				Vector2 draw_position = default(Vector2);
				float side_buffer_space = 32f;
				float y_position_in_lake = (float)(this._position.Y - 160) / (float)(this._game.screenHeight - 160);
				float scroll_speed = Utility.Lerp(-0.4f, -0.75f, y_position_in_lake);
				int current_cycle = (int)Math.Floor((double)(((float)this._position.X + this._game.screenLeftBound * scroll_speed) / ((float)this._game.screenWidth + side_buffer_space * 2f)));
				if (current_cycle != this._lastCycle)
				{
					this._lastCycle = current_cycle;
					if (this.spriteIndex < 2)
					{
						this.spriteIndex = Game1.random.Next(2);
						if (this._game.currentTheme == 6)
						{
							this.spriteIndex = 1;
						}
					}
				}
				float drawY = (float)this._position.Y;
				if (this._bgDecor)
				{
					scroll_speed = Utility.Lerp(-0.15f, -0.25f, (float)this._position.Y / (float)(this._game.screenHeight / 3));
					if (this._game.currentTheme == 3)
					{
						drawY += (float)((int)(Math.Sin((double)Utility.Lerp(0f, 6.2831855f, (float)((this._game.totalTimeMS + (double)(this._position.X * 7) + (double)(this._position.Y * 2)) / 2.0 % 1000.0) / 1000f)) * 3.0));
					}
				}
				draw_position.X = (float)MineCart.Mod((int)((float)this._position.X + this._game.screenLeftBound * scroll_speed), (int)((float)this._game.screenWidth + side_buffer_space * 2f)) - side_buffer_space;
				b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(draw_position.X, drawY)), new Rectangle?(new Rectangle(96 + this.spriteIndex % 14 * this._game.tileSize + (int)((this._game.totalTimeMS + (double)(this._position.X * 10)) % 1000.0 / (double)(1000 / this._animationFrames)) % 14 * this._game.tileSize, 848 + this.spriteIndex / 14 * this._game.tileSize, 16, 16)), (this.spriteIndex == 0) ? this._game.midBGTint : ((this.spriteIndex == 1) ? this._game.lakeTint : Color.White), 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, this._bgDecor ? 0.65f : (0.8f + y_position_in_lake * -0.001f));
			}

			// Token: 0x04002CC6 RID: 11462
			public Point _position;

			// Token: 0x04002CC7 RID: 11463
			public int spriteIndex;

			// Token: 0x04002CC8 RID: 11464
			protected MineCart _game;

			// Token: 0x04002CC9 RID: 11465
			public int _lastCycle = -1;

			// Token: 0x04002CCA RID: 11466
			public bool _bgDecor;

			// Token: 0x04002CCB RID: 11467
			private int _animationFrames = 1;
		}

		// Token: 0x020005B2 RID: 1458
		public class StraightAwayGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x0600423A RID: 16954 RVA: 0x003113D3 File Offset: 0x0030F5D3
			public MineCart.StraightAwayGenerator SetMinimumDistanceBetweenStaggers(int min)
			{
				this.minimuimDistanceBetweenStaggers = min;
				return this;
			}

			// Token: 0x0600423B RID: 16955 RVA: 0x003113DD File Offset: 0x0030F5DD
			public MineCart.StraightAwayGenerator SetLength(int min, int max)
			{
				this.minLength = min;
				this.maxLength = max;
				return this;
			}

			// Token: 0x0600423C RID: 16956 RVA: 0x003113EE File Offset: 0x0030F5EE
			public MineCart.StraightAwayGenerator SetCheckpoint(bool checkpoint)
			{
				this.generateCheckpoint = checkpoint;
				return this;
			}

			// Token: 0x0600423D RID: 16957 RVA: 0x003113F8 File Offset: 0x0030F5F8
			public MineCart.StraightAwayGenerator SetStaggerChance(float chance)
			{
				this.staggerChance = chance;
				return this;
			}

			// Token: 0x0600423E RID: 16958 RVA: 0x00311404 File Offset: 0x0030F604
			public MineCart.StraightAwayGenerator SetStaggerValues(params int[] args)
			{
				this.staggerPattern = new List<int>();
				for (int i = 0; i < args.Length; i++)
				{
					this.staggerPattern.Add(args[i]);
				}
				return this;
			}

			// Token: 0x0600423F RID: 16959 RVA: 0x0031143C File Offset: 0x0030F63C
			public MineCart.StraightAwayGenerator SetStaggerValueRange(int min, int max)
			{
				this.staggerPattern = new List<int>();
				for (int i = min; i <= max; i++)
				{
					this.staggerPattern.Add(i);
				}
				return this;
			}

			// Token: 0x06004240 RID: 16960 RVA: 0x0031146D File Offset: 0x0030F66D
			public StraightAwayGenerator(MineCart game) : base(game)
			{
			}

			// Token: 0x06004241 RID: 16961 RVA: 0x003114AC File Offset: 0x0030F6AC
			public override void Initialize()
			{
				this.straightAwayLength = Game1.random.Next(this.minLength, this.maxLength + 1);
				this._generatedCheckpoint = false;
				if (this.straightAwayLength <= 3)
				{
					this._generatedCheckpoint = true;
				}
				base.Initialize();
			}

			// Token: 0x06004242 RID: 16962 RVA: 0x003114EC File Offset: 0x0030F6EC
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				for (int i = 0; i < this.straightAwayLength; i++)
				{
					if (this._game.generatorPosition.X >= this._game.distanceToTravel)
					{
						return;
					}
					int last_y = this._game.generatorPosition.Y;
					if (this.currentStaggerDistance <= 0)
					{
						if (Game1.random.NextDouble() < (double)this.staggerChance)
						{
							MineCart game = this._game;
							game.generatorPosition.Y = game.generatorPosition.Y + Game1.random.ChooseFrom(this.staggerPattern);
						}
						this.currentStaggerDistance = this.minimuimDistanceBetweenStaggers;
					}
					else
					{
						this.currentStaggerDistance--;
					}
					if (!this._game.IsTileInBounds(this._game.generatorPosition.Y))
					{
						this._game.generatorPosition.Y = last_y;
						this.straightAwayLength = 0;
						break;
					}
					this._game.generatorPosition.Y = this._game.KeepTileInBounds(this._game.generatorPosition.Y);
					MineCart.Track.TrackType tile_type = MineCart.Track.TrackType.Straight;
					if (this._game.generatorPosition.Y < last_y)
					{
						tile_type = MineCart.Track.TrackType.UpSlope;
					}
					else if (this._game.generatorPosition.Y > last_y)
					{
						tile_type = MineCart.Track.TrackType.DownSlope;
					}
					if (tile_type == MineCart.Track.TrackType.DownSlope && this._game.currentTheme == 1)
					{
						tile_type = MineCart.Track.TrackType.IceDownSlope;
					}
					if (tile_type == MineCart.Track.TrackType.UpSlope && this._game.currentTheme == 5)
					{
						tile_type = MineCart.Track.TrackType.SlimeUpSlope;
					}
					base.AddPickupTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, tile_type);
					MineCart game2 = this._game;
					game2.generatorPosition.X = game2.generatorPosition.X + 1;
				}
				if (this._generatedTracks != null && this._generatedTracks.Count > 0 && this.generateCheckpoint && !this._generatedCheckpoint)
				{
					this._generatedCheckpoint = true;
					from o in this._generatedTracks
					orderby o.position.X
					select o;
					this._game.AddCheckpoint((int)(this._generatedTracks[0].position.X / (float)this._game.tileSize));
				}
			}

			// Token: 0x04002CCC RID: 11468
			public int straightAwayLength = 10;

			// Token: 0x04002CCD RID: 11469
			public List<int> staggerPattern;

			// Token: 0x04002CCE RID: 11470
			public int minLength = 3;

			// Token: 0x04002CCF RID: 11471
			public int maxLength = 5;

			// Token: 0x04002CD0 RID: 11472
			public float staggerChance = 0.25f;

			// Token: 0x04002CD1 RID: 11473
			public int minimuimDistanceBetweenStaggers = 1;

			// Token: 0x04002CD2 RID: 11474
			public int currentStaggerDistance;

			// Token: 0x04002CD3 RID: 11475
			public bool generateCheckpoint = true;

			// Token: 0x04002CD4 RID: 11476
			protected bool _generatedCheckpoint = true;
		}

		// Token: 0x020005B3 RID: 1459
		public class SmallGapGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x06004243 RID: 16963 RVA: 0x00311738 File Offset: 0x0030F938
			public MineCart.SmallGapGenerator SetLength(int min, int max)
			{
				this.minLength = min;
				this.maxLength = max;
				return this;
			}

			// Token: 0x06004244 RID: 16964 RVA: 0x00311749 File Offset: 0x0030F949
			public MineCart.SmallGapGenerator SetDepth(int min, int max)
			{
				this.minDepth = min;
				this.maxDepth = max;
				return this;
			}

			// Token: 0x06004245 RID: 16965 RVA: 0x0031175A File Offset: 0x0030F95A
			public SmallGapGenerator(MineCart game) : base(game)
			{
			}

			// Token: 0x06004246 RID: 16966 RVA: 0x00311780 File Offset: 0x0030F980
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				int depth = Game1.random.Next(this.minDepth, this.maxDepth + 1);
				int length = Game1.random.Next(this.minLength, this.maxLength + 1);
				base.AddTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				MineCart game = this._game;
				game.generatorPosition.X = game.generatorPosition.X + 1;
				MineCart game2 = this._game;
				game2.generatorPosition.Y = game2.generatorPosition.Y + depth;
				for (int i = 0; i < length; i++)
				{
					if (this._game.generatorPosition.X >= this._game.distanceToTravel)
					{
						MineCart game3 = this._game;
						game3.generatorPosition.Y = game3.generatorPosition.Y - depth;
						return;
					}
					base.AddPickupTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
					MineCart game4 = this._game;
					game4.generatorPosition.X = game4.generatorPosition.X + 1;
				}
				MineCart game5 = this._game;
				game5.generatorPosition.Y = game5.generatorPosition.Y - depth;
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				base.AddTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				MineCart game6 = this._game;
				game6.generatorPosition.X = game6.generatorPosition.X + 1;
			}

			// Token: 0x04002CD5 RID: 11477
			public int minLength = 3;

			// Token: 0x04002CD6 RID: 11478
			public int maxLength = 5;

			// Token: 0x04002CD7 RID: 11479
			public int minDepth = 5;

			// Token: 0x04002CD8 RID: 11480
			public int maxDepth = 5;
		}

		// Token: 0x020005B4 RID: 1460
		public class RapidHopsGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x06004247 RID: 16967 RVA: 0x0031191B File Offset: 0x0030FB1B
			public MineCart.RapidHopsGenerator SetLength(int min, int max)
			{
				this.minLength = min;
				this.maxLength = max;
				return this;
			}

			// Token: 0x06004248 RID: 16968 RVA: 0x0031192C File Offset: 0x0030FB2C
			public MineCart.RapidHopsGenerator SetYStep(int yStep)
			{
				this.yStep = yStep;
				return this;
			}

			// Token: 0x06004249 RID: 16969 RVA: 0x00311936 File Offset: 0x0030FB36
			public MineCart.RapidHopsGenerator SetChaotic(bool chaotic)
			{
				this.chaotic = chaotic;
				return this;
			}

			// Token: 0x0600424A RID: 16970 RVA: 0x00311940 File Offset: 0x0030FB40
			public RapidHopsGenerator(MineCart game) : base(game)
			{
			}

			// Token: 0x0600424B RID: 16971 RVA: 0x00311958 File Offset: 0x0030FB58
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				if (this.startY == 0)
				{
					this.startY = this._game.generatorPosition.Y;
				}
				int length = Game1.random.Next(this.minLength, this.maxLength + 1);
				base.AddTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				MineCart game = this._game;
				game.generatorPosition.X = game.generatorPosition.X + 1;
				MineCart game2 = this._game;
				game2.generatorPosition.Y = game2.generatorPosition.Y + this.yStep;
				for (int i = 0; i < length; i++)
				{
					if (this._game.generatorPosition.Y < 3 || this._game.generatorPosition.Y > this._game.screenHeight / this._game.tileSize - 2)
					{
						this._game.generatorPosition.Y = this._game.screenHeight / this._game.tileSize - 2;
						this.startY = this._game.generatorPosition.Y;
					}
					if (this._game.generatorPosition.X >= this._game.distanceToTravel)
					{
						MineCart game3 = this._game;
						game3.generatorPosition.Y = game3.generatorPosition.Y - this.yStep;
						return;
					}
					base.AddPickupTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
					MineCart game4 = this._game;
					game4.generatorPosition.X = game4.generatorPosition.X + Game1.random.Next(2, 4);
					if (Game1.random.NextDouble() < 0.33)
					{
						base.AddTrack(this._game.generatorPosition.X - 1, Math.Min(this._game.screenHeight / this._game.tileSize - 2, this._game.generatorPosition.Y + Game1.random.Next(5)), MineCart.Track.TrackType.Straight);
					}
					if (this.chaotic)
					{
						this._game.generatorPosition.Y = this.startY + Game1.random.Next(-Math.Abs(this.yStep), Math.Abs(this.yStep) + 1);
					}
					else
					{
						MineCart game5 = this._game;
						game5.generatorPosition.Y = game5.generatorPosition.Y + this.yStep;
					}
				}
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				MineCart game6 = this._game;
				game6.generatorPosition.Y = game6.generatorPosition.Y - this.yStep;
				base.AddTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, MineCart.Track.TrackType.Straight);
				MineCart game7 = this._game;
				game7.generatorPosition.X = game7.generatorPosition.X + 1;
			}

			// Token: 0x04002CD9 RID: 11481
			public int minLength = 3;

			// Token: 0x04002CDA RID: 11482
			public int maxLength = 5;

			// Token: 0x04002CDB RID: 11483
			private int startY;

			// Token: 0x04002CDC RID: 11484
			public int yStep;

			// Token: 0x04002CDD RID: 11485
			public bool chaotic;
		}

		// Token: 0x020005B5 RID: 1461
		public class NoxiousMushroom : MineCart.Obstacle
		{
			// Token: 0x0600424C RID: 16972 RVA: 0x00311C5C File Offset: 0x0030FE5C
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -12, 8, 12);
			}

			// Token: 0x0600424D RID: 16973 RVA: 0x00311C6A File Offset: 0x0030FE6A
			public override void InitializeObstacle(MineCart.Track track)
			{
				this.nextFire = Utility.RandomFloat(0f, this.firePeriod, null);
				this._track = track;
				base.InitializeObstacle(track);
			}

			// Token: 0x0600424E RID: 16974 RVA: 0x00311C94 File Offset: 0x0030FE94
			protected override void _Update(float time)
			{
				this.nextFire -= time;
				if (this.nextFire <= 0f)
				{
					if (base.IsOnScreen() && this._game.deathTimer <= 0f && (float)this._game.respawnCounter <= 0f)
					{
						MineCart.NoxiousGas noxiousGas = this._game.AddEntity<MineCart.NoxiousGas>(new MineCart.NoxiousGas());
						noxiousGas.position = this.position;
						noxiousGas.position.Y = (float)this.GetBounds().Top;
						noxiousGas.InitializeObstacle(this._track);
						Game1.playSound("sandyStep", null);
						this.currentFrame = 1;
						this.frameTimer = this.frameDuration;
					}
					this.nextFire = 1.5f;
				}
				if (this.currentFrame > 0)
				{
					this.frameTimer -= time;
					if (this.frameTimer <= 0f)
					{
						this.frameTimer = this.frameDuration;
						this.currentFrame++;
						if (this.currentFrame >= this.frames.Length)
						{
							this.currentFrame = 0;
							this.frameTimer = 0f;
						}
					}
				}
			}

			// Token: 0x0600424F RID: 16975 RVA: 0x00311DC4 File Offset: 0x0030FFC4
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(this.frames[this.currentFrame]), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x06004250 RID: 16976 RVA: 0x00311E33 File Offset: 0x00310033
			public override bool CanSpawnHere(MineCart.Track track)
			{
				return track != null && track.trackType == MineCart.Track.TrackType.Straight;
			}

			// Token: 0x04002CDE RID: 11486
			public float nextFire;

			// Token: 0x04002CDF RID: 11487
			public float firePeriod = 1.75f;

			// Token: 0x04002CE0 RID: 11488
			protected MineCart.Track _track;

			// Token: 0x04002CE1 RID: 11489
			public Rectangle[] frames = new Rectangle[]
			{
				new Rectangle(288, 736, 16, 16),
				new Rectangle(288, 752, 16, 16),
				new Rectangle(288, 768, 16, 16)
			};

			// Token: 0x04002CE2 RID: 11490
			public int currentFrame;

			// Token: 0x04002CE3 RID: 11491
			public float frameDuration = 0.05f;

			// Token: 0x04002CE4 RID: 11492
			public float frameTimer;
		}

		// Token: 0x020005B6 RID: 1462
		public class MushroomSpring : MineCart.Obstacle
		{
			// Token: 0x06004252 RID: 16978 RVA: 0x00311ECB File Offset: 0x003100CB
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -12, 8, 12);
			}

			// Token: 0x06004253 RID: 16979 RVA: 0x00311ED9 File Offset: 0x003100D9
			public override void InitializeObstacle(MineCart.Track track)
			{
				base.InitializeObstacle(track);
				this._bouncedPlayers = new HashSet<MineCart.MineCartCharacter>();
			}

			// Token: 0x06004254 RID: 16980 RVA: 0x00311EF0 File Offset: 0x003100F0
			protected override void _Update(float time)
			{
				if (this.currentFrame > 0)
				{
					this.frameTimer -= time;
					if (this.frameTimer <= 0f)
					{
						this.frameTimer = this.frameDuration;
						this.currentFrame++;
						if (this.currentFrame >= this.frames.Length)
						{
							this.currentFrame = 0;
							this.frameTimer = 0f;
						}
					}
				}
			}

			// Token: 0x06004255 RID: 16981 RVA: 0x00311F60 File Offset: 0x00310160
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(this.frames[this.currentFrame]), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x06004256 RID: 16982 RVA: 0x00311FCF File Offset: 0x003101CF
			public override bool CanSpawnHere(MineCart.Track track)
			{
				return track != null && track.trackType == MineCart.Track.TrackType.Straight;
			}

			// Token: 0x06004257 RID: 16983 RVA: 0x00311FE1 File Offset: 0x003101E1
			public override bool OnBounce(MineCart.MineCartCharacter player)
			{
				this.BouncePlayer(player);
				return true;
			}

			// Token: 0x06004258 RID: 16984 RVA: 0x00311FEB File Offset: 0x003101EB
			public override bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				this.BouncePlayer(player);
				return true;
			}

			// Token: 0x06004259 RID: 16985 RVA: 0x00311FF8 File Offset: 0x003101F8
			public void BouncePlayer(MineCart.MineCartCharacter player)
			{
				if (!this._bouncedPlayers.Contains(player))
				{
					this._bouncedPlayers.Add(player);
					if (player is MineCart.PlayerMineCartCharacter)
					{
						this.currentFrame = 1;
						this.frameTimer = this.frameDuration;
						this.ShootDebris(Game1.random.Next(-10, -4), Game1.random.Next(-60, -19));
						this.ShootDebris(Game1.random.Next(5, 11), Game1.random.Next(-60, -19));
						this.ShootDebris(Game1.random.Next(-20, -9), Game1.random.Next(-40, 0));
						this.ShootDebris(Game1.random.Next(10, 21), Game1.random.Next(-40, 0));
						Game1.playSound("hitEnemy", null);
					}
					player.Bounce(0.15f);
				}
			}

			// Token: 0x0600425A RID: 16986 RVA: 0x003120E8 File Offset: 0x003102E8
			public void ShootDebris(int x, int y)
			{
				this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(368, 784, 16, 16), Utility.PointToVector2(this.GetBounds().Center), (float)x, (float)y, 0.25f, 0f, 0.9f, 1f, 3, 0.3f, 0.45f, false, 0f));
			}

			// Token: 0x0600425B RID: 16987 RVA: 0x00312155 File Offset: 0x00310355
			public override void OnPlayerReset()
			{
				this._bouncedPlayers.Clear();
				base.OnPlayerReset();
			}

			// Token: 0x04002CE5 RID: 11493
			protected HashSet<MineCart.MineCartCharacter> _bouncedPlayers;

			// Token: 0x04002CE6 RID: 11494
			public Rectangle[] frames = new Rectangle[]
			{
				new Rectangle(400, 736, 16, 16),
				new Rectangle(400, 752, 16, 16),
				new Rectangle(400, 768, 16, 16)
			};

			// Token: 0x04002CE7 RID: 11495
			public int currentFrame;

			// Token: 0x04002CE8 RID: 11496
			public float frameDuration = 0.05f;

			// Token: 0x04002CE9 RID: 11497
			public float frameTimer;
		}

		// Token: 0x020005B7 RID: 1463
		public class MushroomBalanceTrackGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x0600425D RID: 16989 RVA: 0x003121E0 File Offset: 0x003103E0
			public MineCart.MushroomBalanceTrackGenerator SetTrackType(MineCart.Track.TrackType track_type)
			{
				this.trackType = track_type;
				return this;
			}

			// Token: 0x0600425E RID: 16990 RVA: 0x003121EC File Offset: 0x003103EC
			public MineCart.MushroomBalanceTrackGenerator SetStaggerValues(params int[] args)
			{
				this.staggerPattern = new List<int>();
				for (int i = 0; i < args.Length; i++)
				{
					this.staggerPattern.Add(args[i]);
				}
				return this;
			}

			// Token: 0x0600425F RID: 16991 RVA: 0x00312221 File Offset: 0x00310421
			public MineCart.MushroomBalanceTrackGenerator SetReleaseJumpChance(float chance)
			{
				this.releaseJumpChance = chance;
				return this;
			}

			// Token: 0x06004260 RID: 16992 RVA: 0x0031222B File Offset: 0x0031042B
			public MineCart.MushroomBalanceTrackGenerator SetHopSize(int min, int max)
			{
				this.minHopSize = min;
				this.maxHopSize = max;
				return this;
			}

			// Token: 0x06004261 RID: 16993 RVA: 0x0031223C File Offset: 0x0031043C
			public MushroomBalanceTrackGenerator(MineCart game) : base(game)
			{
				this.staggerPattern = new List<int>();
			}

			// Token: 0x06004262 RID: 16994 RVA: 0x00312260 File Offset: 0x00310460
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				this._game.trackBuilderCharacter.enabled = true;
				List<MineCart.BalanceTrack> balance_tracks = new List<MineCart.BalanceTrack>();
				for (int i = 0; i < 4; i++)
				{
					if (i != 1 || !Game1.random.NextBool())
					{
						this._game.trackBuilderCharacter.position.X = ((float)this._game.generatorPosition.X - 1f + 0.5f) * (float)this._game.tileSize;
						this._game.trackBuilderCharacter.position.Y = (float)(this._game.generatorPosition.Y * this._game.tileSize);
						this._game.trackBuilderCharacter.ForceGrounded();
						this._game.trackBuilderCharacter.Jump();
						this._game.trackBuilderCharacter.Update(0.03f);
						int target_y = this._game.generatorPosition.Y;
						if (i != 1)
						{
							if (i == 3 && Game1.random.NextBool())
							{
								target_y -= 4;
							}
							else if (this.staggerPattern != null && this.staggerPattern.Count > 0)
							{
								target_y += Game1.random.ChooseFrom(this.staggerPattern);
							}
						}
						target_y = this._game.KeepTileInBounds(target_y);
						bool has_landed = false;
						while (!has_landed)
						{
							if (this._game.trackBuilderCharacter.position.Y < (float)(target_y * this._game.tileSize) && Math.Abs(Math.Round((double)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize)) - (double)this._game.generatorPosition.X) > 0.0 && this._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
							{
								this._game.trackBuilderCharacter.ReleaseJump();
							}
							Vector2 old_position = this._game.trackBuilderCharacter.position;
							this._game.trackBuilderCharacter.Update(0.03f);
							if (old_position.Y < (float)(target_y * this._game.tileSize) && this._game.trackBuilderCharacter.position.Y >= (float)(target_y * this._game.tileSize))
							{
								has_landed = true;
							}
							if (this._game.trackBuilderCharacter.IsGrounded() || this._game.trackBuilderCharacter.position.Y / (float)this._game.tileSize > (float)this._game.bottomTile)
							{
								this._game.trackBuilderCharacter.position = old_position;
								if (!this._game.IsTileInBounds(target_y))
								{
									return;
								}
								target_y = this._game.KeepTileInBounds((int)(old_position.Y / (float)this._game.tileSize));
								break;
							}
						}
						this._game.generatorPosition.Y = target_y;
						if (i == 0 || i == 2)
						{
							List<MineCart.BalanceTrack> current_balance_tracks = new List<MineCart.BalanceTrack>();
							this._game.generatorPosition.X = (int)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize);
							float y_offset = 0f;
							if (i == 2 && balance_tracks.Count > 0)
							{
								y_offset = balance_tracks[0].position.Y - balance_tracks[0].startY;
							}
							MineCart.BalanceTrack track = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomLeft, false);
							track.position.X = (float)(this._game.generatorPosition.X * this._game.tileSize);
							track.position.Y = this._game.trackBuilderCharacter.position.Y + y_offset;
							track.startY = track.position.Y;
							base.AddTrack(track);
							current_balance_tracks.Add(track);
							MineCart game = this._game;
							game.generatorPosition.X = game.generatorPosition.X + 1;
							track = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomMiddle, false);
							track.position.X = (float)(this._game.generatorPosition.X * this._game.tileSize);
							track.position.Y = this._game.trackBuilderCharacter.position.Y + y_offset;
							track.startY = track.position.Y;
							base.AddTrack(track);
							current_balance_tracks.Add(track);
							MineCart game2 = this._game;
							game2.generatorPosition.X = game2.generatorPosition.X + 1;
							track = new MineCart.BalanceTrack(MineCart.Track.TrackType.MushroomRight, false);
							track.position.X = (float)(this._game.generatorPosition.X * this._game.tileSize);
							track.position.Y = this._game.trackBuilderCharacter.position.Y + y_offset;
							track.startY = track.position.Y;
							base.AddTrack(track);
							current_balance_tracks.Add(track);
							MineCart game3 = this._game;
							game3.generatorPosition.X = game3.generatorPosition.X + 1;
							foreach (MineCart.BalanceTrack balanceTrack in current_balance_tracks)
							{
								balanceTrack.connectedTracks = new List<MineCart.BalanceTrack>(current_balance_tracks);
							}
							if (i == 2)
							{
								foreach (MineCart.BalanceTrack balanceTrack2 in balance_tracks)
								{
									balanceTrack2.counterBalancedTracks = new List<MineCart.BalanceTrack>(current_balance_tracks);
								}
								foreach (MineCart.BalanceTrack balanceTrack3 in current_balance_tracks)
								{
									balanceTrack3.counterBalancedTracks = new List<MineCart.BalanceTrack>(balance_tracks);
								}
							}
							this._game.trackBuilderCharacter.SnapToFloor();
							while (this._game.trackBuilderCharacter.IsGrounded())
							{
								float old_x = this._game.trackBuilderCharacter.position.X;
								this._game.trackBuilderCharacter.Update(0.03f);
								if (!this._game.trackBuilderCharacter.IsGrounded())
								{
									this._game.trackBuilderCharacter.position.X = old_x;
								}
								if (Game1.random.NextDouble() < 0.33000001311302185)
								{
									break;
								}
							}
							balance_tracks.AddRange(current_balance_tracks);
						}
						else
						{
							int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
							for (int width = 0; width < hop_width; width++)
							{
								this._game.generatorPosition.X = (int)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize) + width;
								if (this._game.generatorPosition.X >= this._game.distanceToTravel)
								{
									return;
								}
								base.AddPickupTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, this.trackType);
							}
						}
					}
				}
				foreach (MineCart.BalanceTrack balance_track in balance_tracks)
				{
					balance_track.position.Y = balance_track.startY;
				}
				MineCart game4 = this._game;
				game4.generatorPosition.X = game4.generatorPosition.X + 1;
			}

			// Token: 0x04002CEA RID: 11498
			protected int minHopSize = 1;

			// Token: 0x04002CEB RID: 11499
			protected int maxHopSize = 1;

			// Token: 0x04002CEC RID: 11500
			protected float releaseJumpChance;

			// Token: 0x04002CED RID: 11501
			protected List<int> staggerPattern;

			// Token: 0x04002CEE RID: 11502
			protected MineCart.Track.TrackType trackType;
		}

		// Token: 0x020005B8 RID: 1464
		public class MushroomBunnyHopGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x06004263 RID: 16995 RVA: 0x00312A20 File Offset: 0x00310C20
			public MineCart.MushroomBunnyHopGenerator SetStaggerValues(params int[] args)
			{
				this.staggerPattern = new List<int>();
				for (int i = 0; i < args.Length; i++)
				{
					this.staggerPattern.Add(args[i]);
				}
				return this;
			}

			// Token: 0x06004264 RID: 16996 RVA: 0x00312A55 File Offset: 0x00310C55
			public MineCart.MushroomBunnyHopGenerator SetReleaseJumpChance(float chance)
			{
				this.releaseJumpChance = chance;
				return this;
			}

			// Token: 0x06004265 RID: 16997 RVA: 0x00312A5F File Offset: 0x00310C5F
			public MineCart.MushroomBunnyHopGenerator SetHopSize(int min, int max)
			{
				this.minHopSize = min;
				this.maxHopSize = max;
				return this;
			}

			// Token: 0x06004266 RID: 16998 RVA: 0x00312A70 File Offset: 0x00310C70
			public MineCart.MushroomBunnyHopGenerator SetNumberOfHops(int min, int max)
			{
				this.minHops = min;
				this.maxHops = max;
				return this;
			}

			// Token: 0x06004267 RID: 16999 RVA: 0x00312A81 File Offset: 0x00310C81
			public MushroomBunnyHopGenerator(MineCart game) : base(game)
			{
				this.minHopSize = 1;
				this.maxHopSize = 1;
				this.staggerPattern = new List<int>();
			}

			// Token: 0x06004268 RID: 17000 RVA: 0x00312ABF File Offset: 0x00310CBF
			public override void Initialize()
			{
				this.numberOfHops = Game1.random.Next(this.minHops, this.maxHops + 1);
				base.Initialize();
			}

			// Token: 0x06004269 RID: 17001 RVA: 0x00312AE8 File Offset: 0x00310CE8
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				this._game.trackBuilderCharacter.enabled = true;
				MineCart.MushroomSpring spring = null;
				for (int i = 0; i < this.numberOfHops; i++)
				{
					this._game.trackBuilderCharacter.position.X = ((float)this._game.generatorPosition.X - 1f + 0.5f) * (float)this._game.tileSize;
					this._game.trackBuilderCharacter.position.Y = (float)(this._game.generatorPosition.Y * this._game.tileSize);
					this._game.trackBuilderCharacter.ForceGrounded();
					this._game.trackBuilderCharacter.Jump();
					if (spring != null)
					{
						spring.BouncePlayer(this._game.trackBuilderCharacter);
					}
					this._game.trackBuilderCharacter.Update(0.03f);
					int target_y = this._game.generatorPosition.Y;
					if (this.staggerPattern != null && this.staggerPattern.Count > 0)
					{
						target_y += Game1.random.ChooseFrom(this.staggerPattern);
					}
					target_y = this._game.KeepTileInBounds(target_y);
					bool has_landed = false;
					while (!has_landed)
					{
						if (this._game.trackBuilderCharacter.position.Y < (float)(target_y * this._game.tileSize) && Math.Abs(Math.Round((double)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize)) - (double)this._game.generatorPosition.X) > 1.0 && this._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
						{
							this._game.trackBuilderCharacter.ReleaseJump();
						}
						Vector2 old_position = this._game.trackBuilderCharacter.position;
						float y = this._game.trackBuilderCharacter.velocity.Y;
						this._game.trackBuilderCharacter.Update(0.03f);
						if (y < 0f && this._game.trackBuilderCharacter.velocity.Y >= 0f)
						{
							this._game.CreatePickup(this._game.trackBuilderCharacter.position + new Vector2(0f, 8f), false);
						}
						if (old_position.Y < (float)(target_y * this._game.tileSize) && this._game.trackBuilderCharacter.position.Y >= (float)(target_y * this._game.tileSize))
						{
							has_landed = true;
						}
						if (this._game.trackBuilderCharacter.IsGrounded() || this._game.trackBuilderCharacter.position.Y / (float)this._game.tileSize > (float)this._game.bottomTile)
						{
							this._game.trackBuilderCharacter.position = old_position;
							if (!this._game.IsTileInBounds(target_y))
							{
								return;
							}
							target_y = this._game.KeepTileInBounds((int)(old_position.Y / (float)this._game.tileSize));
							break;
						}
					}
					this._game.generatorPosition.Y = target_y;
					int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
					MineCart.Track.TrackType track_type = this.trackType;
					if (i >= this.numberOfHops - 1)
					{
						track_type = MineCart.Track.TrackType.Straight;
					}
					spring = null;
					for (int width = 0; width < hop_width; width++)
					{
						this._game.generatorPosition.X = (int)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize) + width;
						if (this._game.generatorPosition.X >= this._game.distanceToTravel)
						{
							return;
						}
						if (track_type == MineCart.Track.TrackType.MushroomMiddle)
						{
							base.AddTrack(this._game.generatorPosition.X - 1, this._game.generatorPosition.Y, MineCart.Track.TrackType.MushroomLeft);
							base.AddTrack(this._game.generatorPosition.X + 1, this._game.generatorPosition.Y, MineCart.Track.TrackType.MushroomRight);
						}
						MineCart.Track track = base.AddTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, track_type);
						if (width == hop_width - 1 && i < this.numberOfHops - 1 && this._game.generatorPosition.Y > 4)
						{
							spring = this._game.AddEntity<MineCart.MushroomSpring>(new MineCart.MushroomSpring());
							spring.InitializeObstacle(track);
							spring.position.X = track.position.X + (float)(this._game.tileSize / 2);
							spring.position.Y = (float)track.GetYAtPoint(spring.position.X);
						}
					}
				}
				MineCart game = this._game;
				game.generatorPosition.X = game.generatorPosition.X + 1;
			}

			// Token: 0x04002CEF RID: 11503
			protected int numberOfHops;

			// Token: 0x04002CF0 RID: 11504
			protected int minHops = 1;

			// Token: 0x04002CF1 RID: 11505
			protected int maxHops = 5;

			// Token: 0x04002CF2 RID: 11506
			protected int minHopSize = 1;

			// Token: 0x04002CF3 RID: 11507
			protected int maxHopSize = 1;

			// Token: 0x04002CF4 RID: 11508
			protected float releaseJumpChance;

			// Token: 0x04002CF5 RID: 11509
			protected List<int> staggerPattern;

			// Token: 0x04002CF6 RID: 11510
			protected MineCart.Track.TrackType trackType;
		}

		// Token: 0x020005B9 RID: 1465
		public class BunnyHopGenerator : MineCart.BaseTrackGenerator
		{
			// Token: 0x0600426A RID: 17002 RVA: 0x0031300C File Offset: 0x0031120C
			public MineCart.BunnyHopGenerator SetTrackType(MineCart.Track.TrackType track_type)
			{
				this.trackType = track_type;
				return this;
			}

			// Token: 0x0600426B RID: 17003 RVA: 0x00313018 File Offset: 0x00311218
			public MineCart.BunnyHopGenerator SetStaggerValues(params int[] args)
			{
				this.staggerPattern = new List<int>();
				for (int i = 0; i < args.Length; i++)
				{
					this.staggerPattern.Add(args[i]);
				}
				return this;
			}

			// Token: 0x0600426C RID: 17004 RVA: 0x0031304D File Offset: 0x0031124D
			public MineCart.BunnyHopGenerator SetReleaseJumpChance(float chance)
			{
				this.releaseJumpChance = chance;
				return this;
			}

			// Token: 0x0600426D RID: 17005 RVA: 0x00313057 File Offset: 0x00311257
			public MineCart.BunnyHopGenerator SetHopSize(int min, int max)
			{
				this.minHopSize = min;
				this.maxHopSize = max;
				return this;
			}

			// Token: 0x0600426E RID: 17006 RVA: 0x00313068 File Offset: 0x00311268
			public MineCart.BunnyHopGenerator SetNumberOfHops(int min, int max)
			{
				this.minHops = min;
				this.maxHops = max;
				return this;
			}

			// Token: 0x0600426F RID: 17007 RVA: 0x00313079 File Offset: 0x00311279
			public BunnyHopGenerator(MineCart game) : base(game)
			{
				this.minHopSize = 1;
				this.maxHopSize = 1;
				this.staggerPattern = new List<int>();
			}

			// Token: 0x06004270 RID: 17008 RVA: 0x003130B7 File Offset: 0x003112B7
			public override void Initialize()
			{
				this.numberOfHops = Game1.random.Next(this.minHops, this.maxHops + 1);
				base.Initialize();
			}

			// Token: 0x06004271 RID: 17009 RVA: 0x003130E0 File Offset: 0x003112E0
			protected override void _GenerateTrack()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				this._game.trackBuilderCharacter.enabled = true;
				for (int i = 0; i < this.numberOfHops; i++)
				{
					this._game.trackBuilderCharacter.position.X = ((float)this._game.generatorPosition.X - 1f + 0.5f) * (float)this._game.tileSize;
					this._game.trackBuilderCharacter.position.Y = (float)(this._game.generatorPosition.Y * this._game.tileSize);
					this._game.trackBuilderCharacter.ForceGrounded();
					this._game.trackBuilderCharacter.Jump();
					this._game.trackBuilderCharacter.Update(0.03f);
					int target_y = this._game.generatorPosition.Y;
					if (this.staggerPattern != null && this.staggerPattern.Count > 0)
					{
						target_y += Game1.random.ChooseFrom(this.staggerPattern);
					}
					target_y = this._game.KeepTileInBounds(target_y);
					bool has_landed = false;
					while (!has_landed)
					{
						if (this._game.trackBuilderCharacter.position.Y < (float)(target_y * this._game.tileSize) && Math.Abs(Math.Round((double)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize)) - (double)this._game.generatorPosition.X) > 1.0 && this._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
						{
							this._game.trackBuilderCharacter.ReleaseJump();
						}
						Vector2 old_position = this._game.trackBuilderCharacter.position;
						float y = this._game.trackBuilderCharacter.velocity.Y;
						this._game.trackBuilderCharacter.Update(0.03f);
						if (y < 0f && this._game.trackBuilderCharacter.velocity.Y >= 0f)
						{
							this._game.CreatePickup(this._game.trackBuilderCharacter.position + new Vector2(0f, 8f), false);
						}
						if (old_position.Y < (float)(target_y * this._game.tileSize) && this._game.trackBuilderCharacter.position.Y >= (float)(target_y * this._game.tileSize))
						{
							has_landed = true;
						}
						if (this._game.trackBuilderCharacter.IsGrounded() || this._game.trackBuilderCharacter.position.Y / (float)this._game.tileSize > (float)this._game.bottomTile)
						{
							this._game.trackBuilderCharacter.position = old_position;
							if (!this._game.IsTileInBounds(target_y))
							{
								return;
							}
							target_y = this._game.KeepTileInBounds((int)(old_position.Y / (float)this._game.tileSize));
							break;
						}
					}
					this._game.generatorPosition.Y = target_y;
					int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
					MineCart.Track.TrackType track_type = this.trackType;
					if (i >= this.numberOfHops - 1)
					{
						track_type = MineCart.Track.TrackType.Straight;
					}
					for (int width = 0; width < hop_width; width++)
					{
						this._game.generatorPosition.X = (int)(this._game.trackBuilderCharacter.position.X / (float)this._game.tileSize) + width;
						if (this._game.generatorPosition.X >= this._game.distanceToTravel)
						{
							return;
						}
						if (track_type == MineCart.Track.TrackType.MushroomMiddle)
						{
							base.AddTrack(this._game.generatorPosition.X - 1, this._game.generatorPosition.Y, MineCart.Track.TrackType.MushroomLeft);
							base.AddTrack(this._game.generatorPosition.X + 1, this._game.generatorPosition.Y, MineCart.Track.TrackType.MushroomRight);
						}
						base.AddPickupTrack(this._game.generatorPosition.X, this._game.generatorPosition.Y, track_type);
					}
				}
				MineCart game = this._game;
				game.generatorPosition.X = game.generatorPosition.X + 1;
			}

			// Token: 0x04002CF7 RID: 11511
			protected int numberOfHops;

			// Token: 0x04002CF8 RID: 11512
			protected int minHops = 1;

			// Token: 0x04002CF9 RID: 11513
			protected int maxHops = 5;

			// Token: 0x04002CFA RID: 11514
			protected int minHopSize = 1;

			// Token: 0x04002CFB RID: 11515
			protected int maxHopSize = 1;

			// Token: 0x04002CFC RID: 11516
			protected float releaseJumpChance;

			// Token: 0x04002CFD RID: 11517
			protected List<int> staggerPattern;

			// Token: 0x04002CFE RID: 11518
			protected MineCart.Track.TrackType trackType;
		}

		// Token: 0x020005BA RID: 1466
		public class BaseTrackGenerator
		{
			// Token: 0x06004272 RID: 17010 RVA: 0x00313566 File Offset: 0x00311766
			public static bool FlatsOnly(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return track.trackType == MineCart.Track.TrackType.None;
			}

			// Token: 0x06004273 RID: 17011 RVA: 0x00313571 File Offset: 0x00311771
			public static bool UpSlopesOnly(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return track.trackType == MineCart.Track.TrackType.UpSlope;
			}

			// Token: 0x06004274 RID: 17012 RVA: 0x0031357C File Offset: 0x0031177C
			public static bool DownSlopesOnly(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return track.trackType == MineCart.Track.TrackType.DownSlope;
			}

			// Token: 0x06004275 RID: 17013 RVA: 0x00313587 File Offset: 0x00311787
			public static bool IceDownSlopesOnly(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return track.trackType == MineCart.Track.TrackType.IceDownSlope;
			}

			// Token: 0x06004276 RID: 17014 RVA: 0x00313592 File Offset: 0x00311792
			public static bool Always(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return true;
			}

			// Token: 0x06004277 RID: 17015 RVA: 0x00313595 File Offset: 0x00311795
			public static bool EveryOtherTile(MineCart.Track track, MineCart.BaseTrackGenerator generator)
			{
				return (int)(track.position.X / 16f) % 2 == 0;
			}

			// Token: 0x06004278 RID: 17016 RVA: 0x003135B0 File Offset: 0x003117B0
			public T AddObstacle<T>(MineCart.ObstacleTypes obstacle_type, int position, float obstacle_chance = 1f) where T : MineCart.BaseTrackGenerator
			{
				this._obstacleIndices.Add(position, new KeyValuePair<MineCart.ObstacleTypes, float>(obstacle_type, obstacle_chance));
				return this as T;
			}

			// Token: 0x06004279 RID: 17017 RVA: 0x003135D0 File Offset: 0x003117D0
			public T AddPickupFunction<T>(Func<MineCart.Track, MineCart.BaseTrackGenerator, bool> pickup_spawn_function) where T : MineCart.BaseTrackGenerator
			{
				this._pickupFunction = (Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>)Delegate.Combine(this._pickupFunction, pickup_spawn_function);
				return this as T;
			}

			// Token: 0x0600427A RID: 17018 RVA: 0x003135F4 File Offset: 0x003117F4
			public BaseTrackGenerator(MineCart game)
			{
				this._game = game;
			}

			// Token: 0x0600427B RID: 17019 RVA: 0x00313610 File Offset: 0x00311810
			public MineCart.Track AddTrack(int x, int y, MineCart.Track.TrackType track_type = MineCart.Track.TrackType.Straight)
			{
				MineCart.Track track = this._game.AddTrack(x, y, track_type);
				this._generatedTracks.Add(track);
				return track;
			}

			// Token: 0x0600427C RID: 17020 RVA: 0x00313639 File Offset: 0x00311839
			public MineCart.Track AddTrack(MineCart.Track track)
			{
				this._game.AddTrack(track);
				this._generatedTracks.Add(track);
				return track;
			}

			// Token: 0x0600427D RID: 17021 RVA: 0x00313658 File Offset: 0x00311858
			public MineCart.Track AddPickupTrack(int x, int y, MineCart.Track.TrackType track_type = MineCart.Track.TrackType.Straight)
			{
				MineCart.Track track = this.AddTrack(x, y, track_type);
				if (this._pickupFunction == null)
				{
					return track;
				}
				Delegate[] invocationList = this._pickupFunction.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (!((Func<MineCart.Track, MineCart.BaseTrackGenerator, bool>)invocationList[i])(track, this))
					{
						return track;
					}
				}
				MineCart.Pickup pickup = this._game.CreatePickup(track.position + new Vector2(8f, (float)(-(float)this._game.tileSize)), false);
				if (pickup != null && (track.trackType == MineCart.Track.TrackType.DownSlope || track.trackType == MineCart.Track.TrackType.UpSlope || track.trackType == MineCart.Track.TrackType.IceDownSlope || track.trackType == MineCart.Track.TrackType.SlimeUpSlope))
				{
					pickup.position += new Vector2(0f, (float)(-(float)this._game.tileSize) * 0.75f);
				}
				return track;
			}

			// Token: 0x0600427E RID: 17022 RVA: 0x0031372B File Offset: 0x0031192B
			public virtual void Initialize()
			{
				this._generatedTracks = new List<MineCart.Track>();
			}

			// Token: 0x0600427F RID: 17023 RVA: 0x00313738 File Offset: 0x00311938
			public void GenerateTrack()
			{
				this._GenerateTrack();
				this.PopulateObstacles();
			}

			// Token: 0x06004280 RID: 17024 RVA: 0x00313748 File Offset: 0x00311948
			public void PopulateObstacles()
			{
				if (this._game.generatorPosition.X >= this._game.distanceToTravel)
				{
					return;
				}
				if (this._generatedTracks.Count == 0)
				{
					return;
				}
				from o in this._generatedTracks
				orderby o.position.X
				select o;
				if (this._obstacleIndices == null || this._obstacleIndices.Count == 0)
				{
					return;
				}
				foreach (int index in this._obstacleIndices.Keys)
				{
					if (Game1.random.NextBool(this._obstacleIndices[index].Value))
					{
						int track_index;
						switch (index)
						{
						case -13:
							track_index = Game1.random.Next(this._generatedTracks.Count);
							break;
						case -12:
							track_index = this._generatedTracks.Count - 1;
							break;
						case -11:
							track_index = 0;
							break;
						case -10:
							track_index = (this._generatedTracks.Count - 1) / 2;
							break;
						default:
							track_index = index;
							break;
						}
						MineCart.Track track = this._generatedTracks[track_index];
						if (track != null && (int)(track.position.X / (float)this._game.tileSize) < this._game.distanceToTravel)
						{
							this._game.AddObstacle(track, this._obstacleIndices[index].Key);
						}
					}
				}
			}

			// Token: 0x06004281 RID: 17025 RVA: 0x003138E4 File Offset: 0x00311AE4
			protected virtual void _GenerateTrack()
			{
				MineCart game = this._game;
				game.generatorPosition.X = game.generatorPosition.X + 1;
			}

			// Token: 0x04002CFF RID: 11519
			public const int OBSTACLE_NONE = -10;

			// Token: 0x04002D00 RID: 11520
			public const int OBSTACLE_MIDDLE = -10;

			// Token: 0x04002D01 RID: 11521
			public const int OBSTACLE_FRONT = -11;

			// Token: 0x04002D02 RID: 11522
			public const int OBSTACLE_BACK = -12;

			// Token: 0x04002D03 RID: 11523
			public const int OBSTACLE_RANDOM = -13;

			// Token: 0x04002D04 RID: 11524
			protected List<MineCart.Track> _generatedTracks;

			// Token: 0x04002D05 RID: 11525
			protected MineCart _game;

			// Token: 0x04002D06 RID: 11526
			protected Dictionary<int, KeyValuePair<MineCart.ObstacleTypes, float>> _obstacleIndices = new Dictionary<int, KeyValuePair<MineCart.ObstacleTypes, float>>();

			// Token: 0x04002D07 RID: 11527
			protected Func<MineCart.Track, MineCart.BaseTrackGenerator, bool> _pickupFunction;
		}

		// Token: 0x020005BB RID: 1467
		public class Spark
		{
			// Token: 0x06004282 RID: 17026 RVA: 0x003138FB File Offset: 0x00311AFB
			public Spark(float x, float y, float dx, float dy)
			{
				this.x = x;
				this.y = y;
				this.dx = dx;
				this.dy = dy;
				this.c = Color.Yellow;
			}

			// Token: 0x04002D08 RID: 11528
			public float x;

			// Token: 0x04002D09 RID: 11529
			public float y;

			// Token: 0x04002D0A RID: 11530
			public Color c;

			// Token: 0x04002D0B RID: 11531
			public float dx;

			// Token: 0x04002D0C RID: 11532
			public float dy;
		}

		// Token: 0x020005BC RID: 1468
		public class Entity
		{
			// Token: 0x170004E9 RID: 1257
			// (get) Token: 0x06004283 RID: 17027 RVA: 0x0031392B File Offset: 0x00311B2B
			public Vector2 drawnPosition
			{
				get
				{
					return this.position - new Vector2(this._game.screenLeftBound, 0f);
				}
			}

			// Token: 0x06004284 RID: 17028 RVA: 0x0031394D File Offset: 0x00311B4D
			public virtual void OnPlayerReset()
			{
			}

			// Token: 0x06004285 RID: 17029 RVA: 0x00313950 File Offset: 0x00311B50
			public bool IsOnScreen()
			{
				return this.position.X >= this._game.screenLeftBound - (float)(this._game.tileSize * 4) && this.position.X <= this._game.screenLeftBound + (float)this._game.screenWidth + (float)(this._game.tileSize * 4);
			}

			// Token: 0x06004286 RID: 17030 RVA: 0x003139BD File Offset: 0x00311BBD
			public bool IsActive()
			{
				return !this._destroyed && this.enabled;
			}

			// Token: 0x06004287 RID: 17031 RVA: 0x003139D4 File Offset: 0x00311BD4
			public void Initialize(MineCart game)
			{
				this._game = game;
				this._Initialize();
			}

			// Token: 0x06004288 RID: 17032 RVA: 0x003139E3 File Offset: 0x00311BE3
			public void Destroy()
			{
				this._destroyed = true;
			}

			// Token: 0x06004289 RID: 17033 RVA: 0x003139EC File Offset: 0x00311BEC
			protected virtual void _Initialize()
			{
			}

			// Token: 0x0600428A RID: 17034 RVA: 0x003139EE File Offset: 0x00311BEE
			public virtual bool ShouldReap()
			{
				return this._destroyed;
			}

			// Token: 0x0600428B RID: 17035 RVA: 0x003139F6 File Offset: 0x00311BF6
			public void Draw(SpriteBatch b)
			{
				if (this._destroyed)
				{
					return;
				}
				if (this.visible && this.enabled)
				{
					this._Draw(b);
				}
			}

			// Token: 0x0600428C RID: 17036 RVA: 0x00313A18 File Offset: 0x00311C18
			public virtual void _Draw(SpriteBatch b)
			{
			}

			// Token: 0x0600428D RID: 17037 RVA: 0x00313A1A File Offset: 0x00311C1A
			public void Update(float time)
			{
				if (this._destroyed)
				{
					return;
				}
				if (this.enabled)
				{
					this._Update(time);
				}
			}

			// Token: 0x0600428E RID: 17038 RVA: 0x00313A34 File Offset: 0x00311C34
			protected virtual void _Update(float time)
			{
			}

			// Token: 0x04002D0D RID: 11533
			public Vector2 position;

			// Token: 0x04002D0E RID: 11534
			protected MineCart _game;

			// Token: 0x04002D0F RID: 11535
			public bool visible = true;

			// Token: 0x04002D10 RID: 11536
			public bool enabled = true;

			// Token: 0x04002D11 RID: 11537
			protected bool _destroyed;
		}

		// Token: 0x020005BD RID: 1469
		public class BaseCharacter : MineCart.Entity
		{
			// Token: 0x04002D12 RID: 11538
			public Vector2 velocity;
		}

		// Token: 0x020005BE RID: 1470
		public interface ICollideable
		{
			// Token: 0x06004291 RID: 17041
			Rectangle GetLocalBounds();

			// Token: 0x06004292 RID: 17042
			Rectangle GetBounds();
		}

		// Token: 0x020005BF RID: 1471
		public class Bubble : MineCart.Obstacle
		{
			// Token: 0x06004293 RID: 17043 RVA: 0x00313A54 File Offset: 0x00311C54
			public override void OnPlayerReset()
			{
				base.Destroy();
			}

			// Token: 0x06004294 RID: 17044 RVA: 0x00313A5C File Offset: 0x00311C5C
			public override Rectangle GetBounds()
			{
				Rectangle bounds = base.GetBounds();
				bounds.X += (int)this.bubbleOffset.X;
				bounds.Y += (int)this.bubbleOffset.Y;
				return base.GetBounds();
			}

			// Token: 0x06004295 RID: 17045 RVA: 0x00313AA4 File Offset: 0x00311CA4
			public Bubble(float angle, float speed)
			{
				this._normalizedVelocity.X = (float)Math.Cos((double)(angle * 3.1415927f / 180f));
				this._normalizedVelocity.Y = -(float)Math.Sin((double)(angle * 3.1415927f / 180f));
				this.moveSpeed = speed;
				this._age = 0f;
			}

			// Token: 0x06004296 RID: 17046 RVA: 0x00313B52 File Offset: 0x00311D52
			public override bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				this.Pop(true);
				return base.OnBump(player);
			}

			// Token: 0x06004297 RID: 17047 RVA: 0x00313B62 File Offset: 0x00311D62
			public override bool OnBounce(MineCart.MineCartCharacter player)
			{
				if (!(player is MineCart.PlayerMineCartCharacter))
				{
					return false;
				}
				player.Bounce(0f);
				this.Pop(true);
				return true;
			}

			// Token: 0x06004298 RID: 17048 RVA: 0x00313B84 File Offset: 0x00311D84
			public void Pop(bool play_sound = true)
			{
				if (play_sound)
				{
					Game1.playSound("dropItemInWater", null);
				}
				base.Destroy();
				this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(32, 240, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Center.Y), 0f, 0f, 0f, 0f, 0.4f, 1f, 2, 0.2f, 0.45f, false, 0f));
			}

			// Token: 0x06004299 RID: 17049 RVA: 0x00313C2C File Offset: 0x00311E2C
			protected override void _Update(float time)
			{
				this.position += this.moveSpeed * this._normalizedVelocity * time;
				this._age += time;
				this._currentFrame = (int)(this._age / this._timePerFrame);
				if (this._currentFrame >= this._frames.Length)
				{
					this._currentFrame -= this._frames.Length;
					this._currentFrame %= this._repeatedFrameCount;
					this._currentFrame += this._frames.Length - this._repeatedFrameCount;
				}
				this.bubbleOffset.X = (float)Math.Cos((double)(this._age * 10f)) * 4f;
				this.bubbleOffset.Y = (float)Math.Sin((double)(this._age * 10f)) * 4f;
				if (this._age >= this._lifeTime)
				{
					this.Pop(false);
				}
				base._Update(time);
			}

			// Token: 0x0600429A RID: 17050 RVA: 0x00313D3C File Offset: 0x00311F3C
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + this.bubbleOffset), new Rectangle?(new Rectangle(this._frames[this._currentFrame] * 16, 256, 16, 16)), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.27f);
			}

			// Token: 0x04002D13 RID: 11539
			public Vector2 _normalizedVelocity;

			// Token: 0x04002D14 RID: 11540
			public float moveSpeed = 8f;

			// Token: 0x04002D15 RID: 11541
			protected float _age;

			// Token: 0x04002D16 RID: 11542
			protected int _currentFrame;

			// Token: 0x04002D17 RID: 11543
			protected float _timePerFrame = 0.5f;

			// Token: 0x04002D18 RID: 11544
			protected int[] _frames = new int[]
			{
				0,
				1,
				2,
				3,
				3,
				2
			};

			// Token: 0x04002D19 RID: 11545
			protected int _repeatedFrameCount = 4;

			// Token: 0x04002D1A RID: 11546
			protected float _lifeTime = 3f;

			// Token: 0x04002D1B RID: 11547
			public Vector2 bubbleOffset = Vector2.Zero;
		}

		// Token: 0x020005C0 RID: 1472
		public class PlayerBubbleSpawner : MineCart.Entity
		{
			// Token: 0x0600429B RID: 17051 RVA: 0x00313DC4 File Offset: 0x00311FC4
			protected override void _Update(float time)
			{
				this.position = this._game.player.position;
				this.timer -= time;
				if (this._game.player.velocity.Y > 0f && this.bubbleCount == 0)
				{
					this.bubbleCount = 1;
					this.timer = Utility.Lerp(0.05f, 0.25f, (float)Game1.random.NextDouble());
				}
				if (this.timer <= 0f && this.bubbleCount <= 0)
				{
					this.bubbleCount = Game1.random.Next(1, 4);
					this.timer = Utility.Lerp(0.15f, 0.25f, (float)Game1.random.NextDouble());
					return;
				}
				if (this.timer <= 0f)
				{
					this.bubbleCount--;
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 256, 16, 16), this.position + new Vector2(-this._game.player.characterExtraHeight - 16f) / 2f, -10f, 10f, 0f, -1f, 1.5f, 0.5f, 4, 0.1f, 0.45f, true, 0f));
					if (this.bubbleCount == 0)
					{
						this.timer = Utility.Lerp(1f, 1.5f, (float)Game1.random.NextDouble());
						return;
					}
					this.timer = Utility.Lerp(0.15f, 0.25f, (float)Game1.random.NextDouble());
				}
			}

			// Token: 0x04002D1C RID: 11548
			public int bubbleCount;

			// Token: 0x04002D1D RID: 11549
			public float timer;
		}

		// Token: 0x020005C1 RID: 1473
		public class Whale : MineCart.Entity
		{
			// Token: 0x0600429D RID: 17053 RVA: 0x00313F78 File Offset: 0x00312178
			public void SetState(MineCart.Whale.CurrentState new_state, float state_timer = 1f)
			{
				this._currentState = new_state;
				this._stateTimer = state_timer;
			}

			// Token: 0x0600429E RID: 17054 RVA: 0x00313F88 File Offset: 0x00312188
			public override void OnPlayerReset()
			{
				this._currentState = MineCart.Whale.CurrentState.Idle;
				this._stateTimer = 2f;
			}

			// Token: 0x0600429F RID: 17055 RVA: 0x00313F9C File Offset: 0x0031219C
			protected override void _Update(float time)
			{
				base._Update(time);
				this._basePosition.Y = Utility.MoveTowards(this._basePosition.Y, this._game.player.position.Y + 32f, 48f * time);
				this.position.X = this._game.screenLeftBound - 128f + (float)this._game.screenWidth + (float)Math.Cos(this._game.totalTime * 3.141592653589793 / 2.299999952316284) * 24f;
				this.position.Y = this._basePosition.Y + (float)Math.Sin(this._game.totalTime * 3.141592653589793 / 3.0) * 32f;
				if (this.position.Y > (float)this._game.screenHeight)
				{
					this.position.Y = (float)this._game.screenHeight;
				}
				if (this.position.Y < 120f)
				{
					this.position.Y = 120f;
				}
				this._stateTimer -= time;
				switch (this._currentState)
				{
				case MineCart.Whale.CurrentState.Idle:
					this._currentFrame = 0;
					if (this._stateTimer < 0f && this._game.gameState != MineCart.GameStates.Cutscene)
					{
						this._currentState = MineCart.Whale.CurrentState.OpenMouth;
						this._stateTimer = this.mouthCloseTime;
						Game1.playSound("croak", null);
						return;
					}
					break;
				case MineCart.Whale.CurrentState.OpenMouth:
					this._currentFrame = (int)Utility.Lerp(3f, 0f, this._stateTimer / this.mouthCloseTime);
					if (this._stateTimer < 0f)
					{
						this._currentState = MineCart.Whale.CurrentState.FireBubbles;
						this._stateTimer = 4f;
					}
					this._nextFire = 0f;
					return;
				case MineCart.Whale.CurrentState.FireBubbles:
					this._currentFrame = 3;
					this._nextFire -= time;
					if (this._nextFire <= 0f)
					{
						Game1.playSound("dwop", null);
						this._nextFire = 1f;
						float shoot_speed = 32f;
						float shoot_spread = 45f;
						if ((float)this._game.generatorPosition.X >= (float)this._game.distanceToTravel / 2f)
						{
							shoot_speed = Utility.Lerp(32f, 64f, (float)Game1.random.NextDouble());
							shoot_spread = 60f;
						}
						this._game.AddEntity<MineCart.Bubble>(new MineCart.Bubble(180f + Utility.Lerp(-shoot_spread, shoot_spread, (float)Game1.random.NextDouble()), shoot_speed)).position = this.position + new Vector2(48f, -40f);
						this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 256, 16, 16), this.position + new Vector2(96f, -100f), -10f, 10f, 0f, -1f, 1f, 0.5f, 4, 0.25f, 0.45f, false, 0f));
					}
					if (this._stateTimer < 0f)
					{
						this._currentState = MineCart.Whale.CurrentState.CloseMouth;
						this._stateTimer = this.mouthCloseTime;
						return;
					}
					break;
				case MineCart.Whale.CurrentState.CloseMouth:
					this._currentFrame = (int)Utility.Lerp(0f, 3f, this._stateTimer / this.mouthCloseTime);
					if (this._stateTimer < 0f)
					{
						this._currentState = MineCart.Whale.CurrentState.Idle;
						this._stateTimer = 2f;
					}
					break;
				default:
					return;
				}
			}

			// Token: 0x060042A0 RID: 17056 RVA: 0x0031434C File Offset: 0x0031254C
			protected override void _Initialize()
			{
				this._currentState = MineCart.Whale.CurrentState.Idle;
				this._stateTimer = Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
				this._basePosition.Y = (float)(this._game.screenHeight / 2 + 56);
				base._Initialize();
			}

			// Token: 0x060042A1 RID: 17057 RVA: 0x003143A4 File Offset: 0x003125A4
			public override void _Draw(SpriteBatch b)
			{
				Point source_rect_offset = default(Point);
				Point draw_offset = default(Point);
				if (this._currentFrame > 0)
				{
					source_rect_offset.X = 85 * (this._currentFrame - 1) + 1;
					source_rect_offset.Y = 112;
					draw_offset.X = 3;
					draw_offset.Y = -3;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + new Vector2(85f, 0f)), new Rectangle?(new Rectangle(86, 288, 75, 112)), Color.White, 0f, new Vector2(0f, 112f), this._game.GetPixelScale(), SpriteEffects.None, 0.29f);
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + Utility.PointToVector2(draw_offset)), new Rectangle?(new Rectangle(source_rect_offset.X, 288 + source_rect_offset.Y, 85, 112)), Color.White, 0f, new Vector2(0f, 112f), this._game.GetPixelScale(), SpriteEffects.None, 0.28f);
			}

			// Token: 0x04002D1E RID: 11550
			protected MineCart.Whale.CurrentState _currentState;

			// Token: 0x04002D1F RID: 11551
			protected float _stateTimer;

			// Token: 0x04002D20 RID: 11552
			public float mouthCloseTime = 1f;

			// Token: 0x04002D21 RID: 11553
			protected float _nextFire;

			// Token: 0x04002D22 RID: 11554
			protected int _currentFrame;

			// Token: 0x04002D23 RID: 11555
			protected Vector2 _basePosition;

			// Token: 0x02000755 RID: 1877
			public enum CurrentState
			{
				// Token: 0x040031B2 RID: 12722
				Idle,
				// Token: 0x040031B3 RID: 12723
				OpenMouth,
				// Token: 0x040031B4 RID: 12724
				FireBubbles,
				// Token: 0x040031B5 RID: 12725
				CloseMouth
			}
		}

		// Token: 0x020005C2 RID: 1474
		public class EndingJunimo : MineCart.Entity
		{
			// Token: 0x060042A3 RID: 17059 RVA: 0x003144F7 File Offset: 0x003126F7
			public EndingJunimo(bool special = false)
			{
				this._special = special;
			}

			// Token: 0x060042A4 RID: 17060 RVA: 0x00314508 File Offset: 0x00312708
			protected override void _Initialize()
			{
				if (this._special || Game1.random.NextDouble() < 0.01)
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						this._color = Color.Red;
						break;
					case 1:
						this._color = Color.Goldenrod;
						break;
					case 2:
						this._color = Color.Yellow;
						break;
					case 3:
						this._color = Color.Lime;
						break;
					case 4:
						this._color = new Color(0, 255, 180);
						break;
					case 5:
						this._color = new Color(0, 100, 255);
						break;
					case 6:
						this._color = Color.MediumPurple;
						break;
					case 7:
						this._color = Color.Salmon;
						break;
					}
					if (Game1.random.NextDouble() < 0.01)
					{
						this._color = Color.White;
					}
				}
				else
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						this._color = Color.LimeGreen;
						break;
					case 1:
						this._color = Color.Orange;
						break;
					case 2:
						this._color = Color.LightGreen;
						break;
					case 3:
						this._color = Color.Tan;
						break;
					case 4:
						this._color = Color.GreenYellow;
						break;
					case 5:
						this._color = Color.LawnGreen;
						break;
					case 6:
						this._color = Color.PaleGreen;
						break;
					case 7:
						this._color = Color.Turquoise;
						break;
					}
				}
				this._velocity.X = Utility.RandomFloat(-10f, -40f, null);
				this._velocity.Y = Utility.RandomFloat(-20f, -60f, null);
			}

			// Token: 0x060042A5 RID: 17061 RVA: 0x003146DC File Offset: 0x003128DC
			protected override void _Update(float time)
			{
				this.position += time * this._velocity;
				this._velocity.Y = this._velocity.Y + 210f * time;
				float floor_y = this._game.GetTrackForXPosition(this.position.X).position.Y;
				if (this.position.Y >= floor_y)
				{
					if (Game1.random.NextDouble() < 0.10000000149011612)
					{
						Game1.playSound("junimoMeep1", null);
					}
					this.position.Y = floor_y;
					this._velocity.Y = Utility.RandomFloat(-50f, -90f, null);
					if (this.position.X < this._game.player.position.X)
					{
						this._velocity.X = Utility.RandomFloat(10f, 40f, null);
					}
					if (this.position.X > this._game.player.position.X)
					{
						this._velocity.X = Utility.RandomFloat(10f, 40f, null) * -1f;
					}
				}
			}

			// Token: 0x060042A6 RID: 17062 RVA: 0x00314820 File Offset: 0x00312A20
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(Game1.mouseCursors, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(294 + (int)(this._game.totalTimeMS % 400.0) / 100 * 16, 1432, 16, 16)), this._color, 0f, new Vector2(8f, 16f), this._game.GetPixelScale() * 2f / 3f, SpriteEffects.None, 0.25f);
			}

			// Token: 0x04002D24 RID: 11556
			protected Color _color;

			// Token: 0x04002D25 RID: 11557
			protected Vector2 _velocity;

			// Token: 0x04002D26 RID: 11558
			private bool _special;
		}

		// Token: 0x020005C3 RID: 1475
		public class FallingBoulderSpawner : MineCart.Obstacle
		{
			// Token: 0x060042A7 RID: 17063 RVA: 0x003148B5 File Offset: 0x00312AB5
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(0, 0, 0, 0);
			}

			// Token: 0x060042A8 RID: 17064 RVA: 0x003148C0 File Offset: 0x00312AC0
			public override Rectangle GetBounds()
			{
				return new Rectangle(0, 0, 0, 0);
			}

			// Token: 0x060042A9 RID: 17065 RVA: 0x003148CB File Offset: 0x00312ACB
			public override void InitializeObstacle(MineCart.Track track)
			{
				this._track = track;
				this.currentTime = (float)Game1.random.NextDouble() * this.period;
				this.position.Y = -32f;
			}

			// Token: 0x060042AA RID: 17066 RVA: 0x003148FC File Offset: 0x00312AFC
			protected override void _Update(float time)
			{
				base._Update(time);
				this.currentTime += time;
				if (this.currentTime >= this.period)
				{
					this.currentTime = 0f;
					MineCart.FallingBoulder fallingBoulder = this._game.AddEntity<MineCart.FallingBoulder>(new MineCart.FallingBoulder());
					fallingBoulder.position = this.position;
					fallingBoulder.InitializeObstacle(this._track);
				}
			}

			// Token: 0x04002D27 RID: 11559
			public float period = 2.33f;

			// Token: 0x04002D28 RID: 11560
			public float currentTime;

			// Token: 0x04002D29 RID: 11561
			protected MineCart.Track _track;
		}

		// Token: 0x020005C4 RID: 1476
		public class WillOWisp : MineCart.Obstacle
		{
			// Token: 0x060042AC RID: 17068 RVA: 0x00314974 File Offset: 0x00312B74
			public override Rectangle GetBounds()
			{
				Rectangle bounds = base.GetBounds();
				bounds.X += (int)this.offset.X;
				bounds.Y += (int)this.offset.Y;
				return bounds;
			}

			// Token: 0x060042AD RID: 17069 RVA: 0x003149B7 File Offset: 0x00312BB7
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-5, -5, 10, 10);
			}

			// Token: 0x060042AE RID: 17070 RVA: 0x003149C8 File Offset: 0x00312BC8
			protected override void _Update(float time)
			{
				this._age += time;
				Vector2 old_offset = this.offset;
				float interval = 15f;
				this.offset.Y = (float)(Math.Sin((double)(this._age * interval * 3.1415927f / 180f)) - 1.0) * 32f;
				this.offset.X = (float)Math.Cos((double)(this._age * interval * 3f * 3.1415927f / 180f)) * 64f;
				this.offset.Y = this.offset.Y + (float)Math.Sin((double)(this._age * interval * 6f * 3.1415927f / 180f)) * 16f;
				Vector2 delta = this.offset - old_offset;
				this.tailRotation = (float)Math.Atan2((double)delta.Y, (double)delta.X);
				this.tailLength = delta.Length();
				this.scale = Utility.Lerp(0.5f, 0.6f, (float)Math.Sin((double)(this._age * 200f * 3.1415927f / 180f)) + 0.5f);
				this.nextDebris -= time;
				if (this.nextDebris <= 0f)
				{
					this.nextDebris = 0.1f;
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(192, 96, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom) + new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-4, 5)), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-30, -19), 0.25f, -0.15f, 1f, 1f, 4, 0.25f, 0.46f, false, 0f)).visible = this.visible;
				}
			}

			// Token: 0x060042AF RID: 17071 RVA: 0x00314BE4 File Offset: 0x00312DE4
			public override bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				base.Destroy();
				Game1.playSound("ghost", null);
				for (int i = 0; i < 8; i++)
				{
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(192, 96, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom) + new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-4, 5)), (float)Game1.random.Next(-50, 51), (float)Game1.random.Next(-50, 51), 0.25f, -0.15f, 1f, 1f, 4, 0.25f, 0.28f, false, 0f));
				}
				return base.OnBump(player);
			}

			// Token: 0x060042B0 RID: 17072 RVA: 0x00314CD8 File Offset: 0x00312ED8
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle?(new Rectangle(192, 80, 16, 16)), Color.White, this._age * 200f * 0.017453292f, new Vector2(8f, 8f), this._game.GetPixelScale() * this.scale, SpriteEffects.None, 0.27f);
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle?(new Rectangle(160, 112, 32, 32)), Color.White, this._age * 60f * 0.017453292f, new Vector2(16f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.29f);
				if (this._age > 0.25f)
				{
					Vector2 tail_scale = new Vector2(this.tailLength, this.scale);
					if (this.tailLength > 0.5f)
					{
						b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle?(new Rectangle(208 + (int)(this._age / 0.1f) % 3 * 16, 80, 16, 16)), Color.White, this.tailRotation, new Vector2(16f, 8f), tail_scale * this._game.GetPixelScale(), SpriteEffects.None, 0.44f);
					}
				}
			}

			// Token: 0x04002D2A RID: 11562
			protected float _age;

			// Token: 0x04002D2B RID: 11563
			protected Vector2 offset;

			// Token: 0x04002D2C RID: 11564
			public float tailRotation;

			// Token: 0x04002D2D RID: 11565
			public float tailLength;

			// Token: 0x04002D2E RID: 11566
			public float scale = 1f;

			// Token: 0x04002D2F RID: 11567
			public float nextDebris = 0.1f;
		}

		// Token: 0x020005C5 RID: 1477
		public class CosmeticFallingBoulder : MineCart.FallingBoulder
		{
			// Token: 0x060042B2 RID: 17074 RVA: 0x00314EB5 File Offset: 0x003130B5
			public CosmeticFallingBoulder(float yBreakPosition, Color color, float fallSpeed = 96f, float delayBeforeAppear = 0f)
			{
				this.yBreakPosition = yBreakPosition;
				this.color = color;
				this._fallSpeed = fallSpeed;
				this.delayBeforeAppear = delayBeforeAppear;
				if (delayBeforeAppear > 0f)
				{
					this.visible = false;
				}
			}

			// Token: 0x060042B3 RID: 17075 RVA: 0x00314EEC File Offset: 0x003130EC
			protected override void _Update(float time)
			{
				if (this.delayBeforeAppear > 0f)
				{
					this.delayBeforeAppear -= time;
					if (this.delayBeforeAppear > 0f)
					{
						return;
					}
					this.visible = true;
				}
				this._age += time;
				if (this.position.Y >= this.yBreakPosition)
				{
					this._currentFallSpeed = -30f;
					if (base.IsOnScreen())
					{
						Game1.playSound("hammer", null);
					}
					for (int i = 0; i < 3; i++)
					{
						this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(16, 80, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-30, -19), 0.25f, 1f, 0.5f, 1f, 1, 0.1f, 0.45f, false, 0f)).SetColor(this._game.caveTint);
					}
					this._destroyed = true;
				}
				if (this._currentFallSpeed < this._fallSpeed)
				{
					this._currentFallSpeed += 210f * time;
					if (this._currentFallSpeed > this._fallSpeed)
					{
						this._currentFallSpeed = this._fallSpeed;
					}
				}
				this.position.Y = this.position.Y + time * this._currentFallSpeed;
			}

			// Token: 0x060042B4 RID: 17076 RVA: 0x0031507C File Offset: 0x0031327C
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				if (Math.Floor((double)(this._age / 0.5f)) % 2.0 == 0.0)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(0, 32, 16, 16)), this.color, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), effect, 0.15f);
			}

			// Token: 0x04002D30 RID: 11568
			private float yBreakPosition;

			// Token: 0x04002D31 RID: 11569
			private float delayBeforeAppear;

			// Token: 0x04002D32 RID: 11570
			private Color color;
		}

		// Token: 0x020005C6 RID: 1478
		public class NoxiousGas : MineCart.Obstacle
		{
			// Token: 0x060042B5 RID: 17077 RVA: 0x00315112 File Offset: 0x00313312
			public override void OnPlayerReset()
			{
				base.Destroy();
			}

			// Token: 0x060042B6 RID: 17078 RVA: 0x0031511C File Offset: 0x0031331C
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				if (Math.Floor((double)(this._age / 0.5f)) % 2.0 == 0.0)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(368, 784, 16, 16)), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale() * Utility.Clamp(this._age / 0.5f, 0f, 1f), effect, 0.44f);
			}

			// Token: 0x060042B7 RID: 17079 RVA: 0x003151D4 File Offset: 0x003133D4
			protected override void _Update(float time)
			{
				this._age += time;
				if (this._currentRiseSpeed > this._riseSpeed)
				{
					this._currentRiseSpeed -= 40f * time;
					if (this._currentRiseSpeed < this._riseSpeed)
					{
						this._currentRiseSpeed = this._riseSpeed;
					}
				}
				this.position.Y = this.position.Y + time * this._currentRiseSpeed;
			}

			// Token: 0x060042B8 RID: 17080 RVA: 0x00315242 File Offset: 0x00313442
			public override bool OnBounce(MineCart.MineCartCharacter player)
			{
				return false;
			}

			// Token: 0x060042B9 RID: 17081 RVA: 0x00315245 File Offset: 0x00313445
			public override bool ShouldReap()
			{
				return this.position.Y < -32f || base.ShouldReap();
			}

			// Token: 0x04002D33 RID: 11571
			protected float _age;

			// Token: 0x04002D34 RID: 11572
			protected float _currentRiseSpeed;

			// Token: 0x04002D35 RID: 11573
			protected float _riseSpeed = -90f;
		}

		// Token: 0x020005C7 RID: 1479
		public class FallingBoulder : MineCart.Obstacle
		{
			// Token: 0x060042BB RID: 17083 RVA: 0x00315274 File Offset: 0x00313474
			public override void OnPlayerReset()
			{
				base.Destroy();
			}

			// Token: 0x060042BC RID: 17084 RVA: 0x0031527C File Offset: 0x0031347C
			public override void InitializeObstacle(MineCart.Track track)
			{
				base.InitializeObstacle(track);
				List<MineCart.Track> tracks = this._game.GetTracksForXPosition(this.position.X);
				if (tracks != null)
				{
					this._tracks = new List<MineCart.Track>(tracks);
				}
			}

			// Token: 0x060042BD RID: 17085 RVA: 0x003152B8 File Offset: 0x003134B8
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				if (Math.Floor((double)(this._age / 0.5f)) % 2.0 == 0.0)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(0, 32, 16, 16)), this._game.caveTint, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), effect, 0.45f);
			}

			// Token: 0x060042BE RID: 17086 RVA: 0x00315354 File Offset: 0x00313554
			protected override void _Update(float time)
			{
				this._age += time;
				if (this._tracks != null && this._tracks.Count > 0)
				{
					if (this._tracks[0] == null)
					{
						this._tracks.RemoveAt(0);
					}
					else if (this.position.Y >= (float)this._tracks[0].GetYAtPoint(this.position.X))
					{
						this._currentFallSpeed = -30f;
						this._tracks.RemoveAt(0);
						if (base.IsOnScreen())
						{
							Game1.playSound("hammer", null);
						}
						for (int i = 0; i < 3; i++)
						{
							this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(16, 80, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-30, -19), 0.25f, 1f, 0.5f, 1f, 1, 0.1f, 0.45f, false, 0f)).SetColor(this._game.caveTint);
						}
					}
				}
				if (this._currentFallSpeed < this._fallSpeed)
				{
					this._currentFallSpeed += 210f * time;
					if (this._currentFallSpeed > this._fallSpeed)
					{
						this._currentFallSpeed = this._fallSpeed;
					}
				}
				this.position.Y = this.position.Y + time * this._currentFallSpeed;
			}

			// Token: 0x060042BF RID: 17087 RVA: 0x00315508 File Offset: 0x00313708
			public override bool OnBounce(MineCart.MineCartCharacter player)
			{
				if (!(player is MineCart.PlayerMineCartCharacter))
				{
					return false;
				}
				this._wasBouncedOn = true;
				player.Bounce(0f);
				Game1.playSound("hammer", null);
				for (int i = 0; i < 3; i++)
				{
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(16, 80, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Top), (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-30, -19), 0.25f, 1f, 0.5f, 1f, 1, 0.1f, 0.45f, false, 0f)).SetColor(this._game.caveTint);
				}
				return true;
			}

			// Token: 0x060042C0 RID: 17088 RVA: 0x003155F0 File Offset: 0x003137F0
			public override bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				return this._wasBouncedOn || base.OnBump(player);
			}

			// Token: 0x060042C1 RID: 17089 RVA: 0x00315603 File Offset: 0x00313803
			public override bool ShouldReap()
			{
				return this.position.Y > (float)(this._game.screenHeight + 32) || base.ShouldReap();
			}

			// Token: 0x04002D36 RID: 11574
			protected float _age;

			// Token: 0x04002D37 RID: 11575
			protected List<MineCart.Track> _tracks;

			// Token: 0x04002D38 RID: 11576
			protected float _currentFallSpeed;

			// Token: 0x04002D39 RID: 11577
			protected float _fallSpeed = 96f;

			// Token: 0x04002D3A RID: 11578
			protected bool _wasBouncedOn;
		}

		// Token: 0x020005C8 RID: 1480
		public class MineCartSlime : MineCart.Obstacle
		{
			// Token: 0x060042C3 RID: 17091 RVA: 0x0031563C File Offset: 0x0031383C
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(0, 32, 16, 16)), this._game.caveTint, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), effect, 0.45f);
			}

			// Token: 0x060042C4 RID: 17092 RVA: 0x003156AE File Offset: 0x003138AE
			public override bool ShouldReap()
			{
				return false;
			}
		}

		// Token: 0x020005C9 RID: 1481
		public class SlimeTrack : MineCart.Obstacle
		{
			// Token: 0x060042C6 RID: 17094 RVA: 0x003156BC File Offset: 0x003138BC
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(0, 192, 32, 16)), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), effect, 0.45f);
			}

			// Token: 0x060042C7 RID: 17095 RVA: 0x0031572B File Offset: 0x0031392B
			public override bool ShouldReap()
			{
				return false;
			}
		}

		// Token: 0x020005CA RID: 1482
		public class HugeSlime : MineCart.Obstacle
		{
			// Token: 0x060042C9 RID: 17097 RVA: 0x00315736 File Offset: 0x00313936
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-40, -60, 80, 60);
			}

			// Token: 0x060042CA RID: 17098 RVA: 0x00315745 File Offset: 0x00313945
			public override void OnPlayerReset()
			{
				this._game.slimeBossPosition = this._game.checkpointPosition + (float)this._game.slimeResetPosition;
			}

			// Token: 0x060042CB RID: 17099 RVA: 0x0031576A File Offset: 0x0031396A
			protected override void _Initialize()
			{
				base._Initialize();
				this._game.slimeBossPosition = (float)this._game.slimeResetPosition;
				this._grounded = false;
			}

			// Token: 0x060042CC RID: 17100 RVA: 0x00315790 File Offset: 0x00313990
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				Rectangle source_rect = new Rectangle(160, 176, 96, 80);
				switch (this._currentFrame)
				{
				case 0:
					source_rect = new Rectangle(160, 176, 96, 80);
					break;
				case 1:
					source_rect = new Rectangle(160, 256, 96, 80);
					break;
				case 2:
					source_rect = new Rectangle(160, 336, 96, 64);
					break;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(source_rect), Color.White, 0f, new Vector2((float)source_rect.Width * 0.5f, (float)source_rect.Height), this._game.GetPixelScale() * this.spriteScale, effect, 0.45f);
			}

			// Token: 0x060042CD RID: 17101 RVA: 0x0031587C File Offset: 0x00313A7C
			protected override void _Update(float time)
			{
				MineCart.Track track = this._game.GetTrackForXPosition(this.position.X);
				float track_height = (float)(this._game.screenHeight + 32);
				if (track != null)
				{
					this._lastTrackY = (float)track.GetYAtPoint(this.position.X);
					track_height = this._lastTrackY;
				}
				this._game.slimeBossPosition += this._game.slimeBossSpeed * time;
				if (this._grounded)
				{
					this._timeUntilHop -= time;
					if (this._timeUntilHop <= 0f)
					{
						this._grounded = false;
						this.spriteScale = new Vector2(1.1f, 0.75f);
						this._desiredScale = new Vector2(1f, 1f);
						this._scaleSpeed = 1f;
						this._yVelocity = this._jumpStrength;
						Game1.playSound("dwoop", null);
						for (int i = 0; i < 8; i++)
						{
							this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(192, 112, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom) + new Vector2((float)Game1.random.Next(-32, 33), (float)Game1.random.Next(-32, 0)), (float)Game1.random.Next(-10, 11), (float)Game1.random.Next(-50, -29), 0.25f, 0.25f, 1f, 1f, 4, 0.25f, 0.46f, false, 0f));
						}
					}
					else if (this._timeUntilHop <= 0.25f)
					{
						if (!this._hasPeparedToJump)
						{
							this.spriteScale = new Vector2(0.9f, 1.1f);
							this._desiredScale = new Vector2(1f, 1f);
							this._scaleSpeed = 1f;
							this._currentFrame = 2;
							this._hasPeparedToJump = true;
						}
					}
					else
					{
						this._desiredScale = new Vector2(1f, 1f);
						this._scaleSpeed = 4f;
					}
				}
				else
				{
					this._currentFrame = 1;
					if (this.position.X > this._game.slimeBossPosition)
					{
						this.position.X = Utility.MoveTowards(this.position.X, this._game.slimeBossPosition, this._game.slimeBossSpeed * time * 8f);
					}
					else
					{
						this.position.X = Utility.MoveTowards(this.position.X, this._game.slimeBossPosition, this._game.slimeBossSpeed * time * 2f);
					}
					this._yVelocity += 200f * time;
					this.position.Y = this.position.Y + this._yVelocity * time;
					if (this.position.Y > this._lastTrackY && this._yVelocity < 0f)
					{
						this._yVelocity = this._jumpStrength;
					}
					if (this._yVelocity < 0f)
					{
						this._desiredScale = new Vector2(0.9f, 1.1f);
						this._scaleSpeed = 5f;
					}
					else if (this._yVelocity > 0f)
					{
						this._desiredScale = new Vector2(1f, 1f);
						this._scaleSpeed = 0.25f;
					}
					if (this.position.Y > track_height && this._yVelocity > 0f)
					{
						Game1.playSound("slimedead", null);
						Game1.playSound("breakingGlass", null);
						for (int j = 0; j < 8; j++)
						{
							this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(192, 112, 16, 16), new Vector2((float)this.GetBounds().Center.X, (float)this.GetBounds().Bottom) + new Vector2((float)Game1.random.Next(-32, 33), (float)Game1.random.Next(-32, 0)), (float)Game1.random.Next(-80, 81), (float)Game1.random.Next(-10, 1), 0.25f, 0.25f, 1f, 1f, 4, 0.25f, 0.46f, false, 0f));
						}
						this._game.shakeMagnitude = 1.5f;
						this.position.Y = track_height;
						this._grounded = true;
						this._timeUntilHop = 0.5f;
						this._currentFrame = 2;
						this._hasPeparedToJump = false;
						this.spriteScale = new Vector2(1.1f, 0.75f);
					}
				}
				this.spriteScale.X = Utility.MoveTowards(this.spriteScale.X, this._desiredScale.X, this._scaleSpeed * time);
				this.spriteScale.Y = Utility.MoveTowards(this.spriteScale.Y, this._desiredScale.Y, this._scaleSpeed * time);
			}

			// Token: 0x060042CE RID: 17102 RVA: 0x00315DCD File Offset: 0x00313FCD
			public override bool ShouldReap()
			{
				return false;
			}

			// Token: 0x04002D3B RID: 11579
			protected float _timeUntilHop = 30f;

			// Token: 0x04002D3C RID: 11580
			protected float _yVelocity;

			// Token: 0x04002D3D RID: 11581
			protected bool _grounded;

			// Token: 0x04002D3E RID: 11582
			protected float _lastTrackY = 300f;

			// Token: 0x04002D3F RID: 11583
			public Vector2 spriteScale = new Vector2(1f, 1f);

			// Token: 0x04002D40 RID: 11584
			protected int _currentFrame;

			// Token: 0x04002D41 RID: 11585
			protected Vector2 _desiredScale = new Vector2(1f, 1f);

			// Token: 0x04002D42 RID: 11586
			protected float _scaleSpeed = 4f;

			// Token: 0x04002D43 RID: 11587
			protected float _jumpStrength = -200f;

			// Token: 0x04002D44 RID: 11588
			private bool _hasPeparedToJump;
		}

		// Token: 0x020005CB RID: 1483
		public class Roadblock : MineCart.Obstacle
		{
			// Token: 0x060042D0 RID: 17104 RVA: 0x00315E39 File Offset: 0x00314039
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -12, 8, 12);
			}

			// Token: 0x060042D1 RID: 17105 RVA: 0x00315E47 File Offset: 0x00314047
			protected override void _Update(float time)
			{
			}

			// Token: 0x060042D2 RID: 17106 RVA: 0x00315E4C File Offset: 0x0031404C
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(16, 0, 16, 16)), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x060042D3 RID: 17107 RVA: 0x00315EB6 File Offset: 0x003140B6
			public override bool CanSpawnHere(MineCart.Track track)
			{
				return track != null && track.trackType == MineCart.Track.TrackType.Straight;
			}

			// Token: 0x060042D4 RID: 17108 RVA: 0x00315EC8 File Offset: 0x003140C8
			public override bool OnBounce(MineCart.MineCartCharacter player)
			{
				if (!(player is MineCart.PlayerMineCartCharacter))
				{
					return false;
				}
				this.ShootDebris(Game1.random.Next(-10, -4), Game1.random.Next(-60, -19));
				this.ShootDebris(Game1.random.Next(5, 11), Game1.random.Next(-60, -19));
				this.ShootDebris(Game1.random.Next(-20, -9), Game1.random.Next(-40, 0));
				this.ShootDebris(Game1.random.Next(10, 21), Game1.random.Next(-40, 0));
				Game1.playSound("woodWhack", null);
				player.velocity.Y = 0f;
				player.velocity.Y = 0f;
				base.Destroy();
				return true;
			}

			// Token: 0x060042D5 RID: 17109 RVA: 0x00315FA0 File Offset: 0x003141A0
			public override bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				this.ShootDebris(Game1.random.Next(10, 41), Game1.random.Next(-40, 0));
				this.ShootDebris(Game1.random.Next(10, 41), Game1.random.Next(-40, 0));
				this.ShootDebris(Game1.random.Next(5, 31), Game1.random.Next(-60, -19));
				this.ShootDebris(Game1.random.Next(5, 31), Game1.random.Next(-60, -19));
				Game1.playSound("woodWhack", null);
				base.Destroy();
				return false;
			}

			// Token: 0x060042D6 RID: 17110 RVA: 0x0031604C File Offset: 0x0031424C
			public void ShootDebris(int x, int y)
			{
				this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(48, 48, 16, 16), Utility.PointToVector2(this.GetBounds().Center), (float)x, (float)y, 0.25f, 1f, 1f, 1f, 1, 0.1f, 0.45f, false, 0f));
			}
		}

		// Token: 0x020005CC RID: 1484
		public class MineDebris : MineCart.Entity
		{
			// Token: 0x060042D8 RID: 17112 RVA: 0x003160BC File Offset: 0x003142BC
			public MineDebris(Rectangle source_rect, Vector2 spawn_position, float dx, float dy, float flip_rate = 0f, float gravity_multiplier = 1f, float life_time = 0.5f, float scale = 1f, int num_animation_frames = 1, float animation_interval = 0.1f, float draw_depth = 0.45f, bool holdLastFrame = false, float timeBeforeDisplay = 0f)
			{
				this.reset(source_rect, spawn_position, dx, dy, flip_rate, gravity_multiplier, life_time, scale, num_animation_frames, animation_interval, draw_depth, holdLastFrame, timeBeforeDisplay);
			}

			// Token: 0x060042D9 RID: 17113 RVA: 0x00316110 File Offset: 0x00314310
			public void reset(Rectangle source_rect, Vector2 spawn_position, float dx, float dy, float flip_rate = 0f, float gravity_multiplier = 1f, float life_time = 0.5f, float scale = 1f, int num_animation_frames = 1, float animation_interval = 0.1f, float draw_depth = 0.45f, bool holdLastFrame = false, float timeBeforeDisplay = 0f)
			{
				this._sourceRect = source_rect;
				this._dX = dx;
				this._dY = dy;
				this._lifeTime = life_time;
				this.flipRate = flip_rate;
				this.position = spawn_position;
				this._gravityMultiplier = gravity_multiplier;
				this._scale = scale;
				this._numAnimationFrames = num_animation_frames;
				this._animationInterval = animation_interval;
				this.depth = draw_depth;
				this._holdLastFrame = holdLastFrame;
				this._currentAnimationFrame = 0;
				this.timeBeforeDisplay = timeBeforeDisplay;
				if (timeBeforeDisplay > 0f)
				{
					this.visible = false;
				}
			}

			// Token: 0x060042DA RID: 17114 RVA: 0x00316199 File Offset: 0x00314399
			public void SetColor(Color color)
			{
				this._color = color;
			}

			// Token: 0x060042DB RID: 17115 RVA: 0x003161A2 File Offset: 0x003143A2
			public void SetDestroySound(string sound)
			{
				this.destroySound = sound;
			}

			// Token: 0x060042DC RID: 17116 RVA: 0x003161AB File Offset: 0x003143AB
			public void SetStartSound(string sound)
			{
				this.startSound = sound;
			}

			// Token: 0x060042DD RID: 17117 RVA: 0x003161B4 File Offset: 0x003143B4
			protected override void _Update(float time)
			{
				if (this.timeBeforeDisplay > 0f)
				{
					this.timeBeforeDisplay -= time;
					if (this.timeBeforeDisplay > 0f)
					{
						return;
					}
					this.visible = true;
					if (this.startSound != null)
					{
						Game1.playSound(this.startSound, null);
					}
				}
				this.position.X = this.position.X + this._dX * time;
				this.position.Y = this.position.Y + this._dY * time;
				this._dY += 210f * time * this._gravityMultiplier;
				this._age += time;
				if (this._age >= this._lifeTime)
				{
					if (this.destroySound != null)
					{
						Game1.playSound(this.destroySound, null);
					}
					base.Destroy();
					return;
				}
				this._animationTimer += time;
				if (this._animationTimer >= this._animationInterval)
				{
					this._animationTimer = 0f;
					this._currentAnimationFrame++;
					if (this._holdLastFrame && this._currentAnimationFrame >= this._numAnimationFrames - 1)
					{
						this._currentAnimationFrame = this._numAnimationFrames - 1;
					}
					else
					{
						this._currentAnimationFrame %= this._numAnimationFrames;
					}
				}
				base._Update(time);
			}

			// Token: 0x060042DE RID: 17118 RVA: 0x00316310 File Offset: 0x00314510
			private Rectangle _GetSourceRect()
			{
				return new Rectangle(this._sourceRect.X + this._currentAnimationFrame * this._sourceRect.Width, this._sourceRect.Y, this._sourceRect.Width, this._sourceRect.Height);
			}

			// Token: 0x060042DF RID: 17119 RVA: 0x00316364 File Offset: 0x00314564
			public override void _Draw(SpriteBatch b)
			{
				SpriteEffects effect = SpriteEffects.None;
				if (this.flipRate > 0f && Math.Floor((double)(this._age / this.flipRate)) % 2.0 == 0.0)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + new Vector2(0f, (float)Math.Sin(this._game.totalTime + (double)this.position.X) * this.ySinWaveMagnitude)), new Rectangle?(this._GetSourceRect()), this._color, 0f, new Vector2((float)this._sourceRect.Width / 2f, (float)this._sourceRect.Height / 2f), this._game.GetPixelScale() * this._scale, effect, this.depth);
			}

			// Token: 0x04002D45 RID: 11589
			protected Rectangle _sourceRect;

			// Token: 0x04002D46 RID: 11590
			protected float _dX;

			// Token: 0x04002D47 RID: 11591
			protected float _dY;

			// Token: 0x04002D48 RID: 11592
			protected float _age;

			// Token: 0x04002D49 RID: 11593
			protected float _lifeTime;

			// Token: 0x04002D4A RID: 11594
			protected float _gravityMultiplier;

			// Token: 0x04002D4B RID: 11595
			protected float _scale = 1f;

			// Token: 0x04002D4C RID: 11596
			protected Color _color = Color.White;

			// Token: 0x04002D4D RID: 11597
			protected int _numAnimationFrames;

			// Token: 0x04002D4E RID: 11598
			protected bool _holdLastFrame;

			// Token: 0x04002D4F RID: 11599
			protected float _animationInterval;

			// Token: 0x04002D50 RID: 11600
			protected int _currentAnimationFrame;

			// Token: 0x04002D51 RID: 11601
			protected float _animationTimer;

			// Token: 0x04002D52 RID: 11602
			public float ySinWaveMagnitude;

			// Token: 0x04002D53 RID: 11603
			public float flipRate;

			// Token: 0x04002D54 RID: 11604
			public float depth = 0.45f;

			// Token: 0x04002D55 RID: 11605
			private float timeBeforeDisplay;

			// Token: 0x04002D56 RID: 11606
			private string destroySound;

			// Token: 0x04002D57 RID: 11607
			private string startSound;
		}

		// Token: 0x020005CD RID: 1485
		public class Obstacle : MineCart.Entity, MineCart.ICollideable
		{
			// Token: 0x060042E0 RID: 17120 RVA: 0x00316458 File Offset: 0x00314658
			public virtual void InitializeObstacle(MineCart.Track track)
			{
			}

			// Token: 0x060042E1 RID: 17121 RVA: 0x0031645A File Offset: 0x0031465A
			public virtual bool OnBounce(MineCart.MineCartCharacter player)
			{
				return false;
			}

			// Token: 0x060042E2 RID: 17122 RVA: 0x0031645D File Offset: 0x0031465D
			public virtual bool OnBump(MineCart.PlayerMineCartCharacter player)
			{
				return false;
			}

			// Token: 0x060042E3 RID: 17123 RVA: 0x00316460 File Offset: 0x00314660
			public virtual Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -12, 8, 12);
			}

			// Token: 0x060042E4 RID: 17124 RVA: 0x00316470 File Offset: 0x00314670
			public virtual Rectangle GetBounds()
			{
				Rectangle bounds = this.GetLocalBounds();
				bounds.X += (int)this.position.X;
				bounds.Y += (int)this.position.Y;
				return bounds;
			}

			// Token: 0x060042E5 RID: 17125 RVA: 0x003164B4 File Offset: 0x003146B4
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(16, 0, 16, 16)), Color.White, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x060042E6 RID: 17126 RVA: 0x0031651E File Offset: 0x0031471E
			public virtual bool CanSpawnHere(MineCart.Track track)
			{
				return true;
			}
		}

		// Token: 0x020005CE RID: 1486
		public class Fruit : MineCart.Pickup
		{
			// Token: 0x060042E8 RID: 17128 RVA: 0x00316529 File Offset: 0x00314729
			public override Rectangle GetLocalBounds()
			{
				return new Rectangle(-6, -6, 12, 12);
			}

			// Token: 0x060042E9 RID: 17129 RVA: 0x00316538 File Offset: 0x00314738
			public Fruit(MineCart.CollectableFruits fruit_type)
			{
				this._fruitType = fruit_type;
			}

			// Token: 0x060042EA RID: 17130 RVA: 0x00316548 File Offset: 0x00314748
			public override void Collect(MineCart.PlayerMineCartCharacter player)
			{
				this._game.CollectFruit(this._fruitType);
				this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 250, 5, 5), this.position, 0f, 0f, 0f, 0f, 0.6f, 1f, 6, 0.1f, 0.45f, false, 0f));
				for (int i = 0; i < 4; i++)
				{
					float interval = Utility.Lerp(0.1f, 0.2f, (float)Game1.random.NextDouble());
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 250, 5, 5), this.position + new Vector2((float)Game1.random.Next(-8, 9), (float)Game1.random.Next(-8, 9)), 0f, 0f, 0f, 0f, interval * 6f, 1f, 6, interval, 0.45f, false, 0f));
				}
				Game1.playSound("eat", null);
				base.Destroy();
			}

			// Token: 0x060042EB RID: 17131 RVA: 0x0031667C File Offset: 0x0031487C
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle((int)(160 + (MineCart.CollectableFruits)16 * this._fruitType), 0, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), this._game.GetPixelScale(), SpriteEffects.None, 0.43f);
			}

			// Token: 0x04002D58 RID: 11608
			protected MineCart.CollectableFruits _fruitType;
		}

		// Token: 0x020005CF RID: 1487
		public class Coin : MineCart.Pickup
		{
			// Token: 0x060042EC RID: 17132 RVA: 0x003166F4 File Offset: 0x003148F4
			protected override void _Update(float time)
			{
				this.age += time;
				if (this.age > this.flashDelay + this.flashSpeed * 3f)
				{
					this.age = 0f;
				}
				if (this.collected)
				{
					this.afterCollectionTimer += time;
					if (time > 0f)
					{
						this.position.Y = this.position.Y - (3f - this.afterCollectionTimer * 8f * time);
					}
					if (this.afterCollectionTimer > 0.4f)
					{
						base.Destroy();
					}
				}
				base._Update(time);
			}

			// Token: 0x060042ED RID: 17133 RVA: 0x00316790 File Offset: 0x00314990
			public override void _Draw(SpriteBatch b)
			{
				int time = this.collected ? 450 : 900;
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(9 * ((int)this._game.totalTimeMS % time / (time / 12)), 273, 9, 9)), Color.White * (1f - this.afterCollectionTimer / 0.4f), 0f, new Vector2(4f, 4f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x060042EE RID: 17134 RVA: 0x00316840 File Offset: 0x00314A40
			public override void Collect(MineCart.PlayerMineCartCharacter player)
			{
				if (!this.collected)
				{
					this._game.CollectCoin(1);
					Game1.playSound("junimoKart_coin", null);
					this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 250, 5, 5), this.position, 0f, 0f, 0f, 0f, 0.6f, 1f, 6, 0.1f, 0.45f, false, 0f));
					for (int i = 0; i < 4; i++)
					{
						float interval = Utility.Lerp(0.1f, 0.2f, (float)Game1.random.NextDouble());
						this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(0, 250, 5, 5), this.position + new Vector2((float)Game1.random.Next(-8, 9), (float)Game1.random.Next(-8, 9)), 0f, 0f, 0f, 0f, interval * 6f, 1f, 6, interval, 0.45f, false, 0f));
					}
					this.collectYDelta = -3f;
					this.collected = true;
				}
			}

			// Token: 0x04002D59 RID: 11609
			public float age;

			// Token: 0x04002D5A RID: 11610
			public float afterCollectionTimer;

			// Token: 0x04002D5B RID: 11611
			public bool collected;

			// Token: 0x04002D5C RID: 11612
			public float flashSpeed = 0.25f;

			// Token: 0x04002D5D RID: 11613
			public float flashDelay = 0.5f;

			// Token: 0x04002D5E RID: 11614
			public float collectYDelta;
		}

		// Token: 0x020005D0 RID: 1488
		public class Pickup : MineCart.Entity, MineCart.ICollideable
		{
			// Token: 0x060042F0 RID: 17136 RVA: 0x003169A1 File Offset: 0x00314BA1
			public virtual Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -4, 8, 8);
			}

			// Token: 0x060042F1 RID: 17137 RVA: 0x003169B0 File Offset: 0x00314BB0
			public virtual Rectangle GetBounds()
			{
				Rectangle bounds = this.GetLocalBounds();
				bounds.X += (int)this.position.X;
				bounds.Y += (int)this.position.Y;
				return bounds;
			}

			// Token: 0x060042F2 RID: 17138 RVA: 0x003169F4 File Offset: 0x00314BF4
			public override void _Draw(SpriteBatch b)
			{
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(16, 16, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
			}

			// Token: 0x060042F3 RID: 17139 RVA: 0x00316A60 File Offset: 0x00314C60
			public virtual void Collect(MineCart.PlayerMineCartCharacter player)
			{
				Game1.playSound("Pickup_Coin15", null);
				base.Destroy();
			}
		}

		// Token: 0x020005D1 RID: 1489
		public class BalanceTrack : MineCart.Track
		{
			// Token: 0x060042F5 RID: 17141 RVA: 0x00316A8F File Offset: 0x00314C8F
			public BalanceTrack(MineCart.Track.TrackType type, bool showSecondTile) : base(type, showSecondTile)
			{
				this.connectedTracks = new List<MineCart.BalanceTrack>();
				this.counterBalancedTracks = new List<MineCart.BalanceTrack>();
			}

			// Token: 0x060042F6 RID: 17142 RVA: 0x00316ABA File Offset: 0x00314CBA
			public override void OnPlayerReset()
			{
				this.position.Y = this.startY;
			}

			// Token: 0x060042F7 RID: 17143 RVA: 0x00316AD0 File Offset: 0x00314CD0
			public override void WhileCartGrounded(MineCart.MineCartCharacter character, float time)
			{
				foreach (MineCart.BalanceTrack balanceTrack in this.connectedTracks)
				{
					balanceTrack.position.Y = balanceTrack.position.Y + this.moveSpeed * time;
				}
				foreach (MineCart.BalanceTrack balanceTrack2 in this.counterBalancedTracks)
				{
					balanceTrack2.position.Y = balanceTrack2.position.Y - this.moveSpeed * time;
				}
			}

			// Token: 0x04002D5F RID: 11615
			public List<MineCart.BalanceTrack> connectedTracks;

			// Token: 0x04002D60 RID: 11616
			public List<MineCart.BalanceTrack> counterBalancedTracks;

			// Token: 0x04002D61 RID: 11617
			public float startY;

			// Token: 0x04002D62 RID: 11618
			public float moveSpeed = 128f;
		}

		// Token: 0x020005D2 RID: 1490
		public class Track : MineCart.Entity
		{
			// Token: 0x060042F8 RID: 17144 RVA: 0x00316B84 File Offset: 0x00314D84
			public Track(MineCart.Track.TrackType type, bool showSecondTile)
			{
				this.trackType = type;
				this._showSecondTile = showSecondTile;
			}

			// Token: 0x060042F9 RID: 17145 RVA: 0x00316B9A File Offset: 0x00314D9A
			public virtual void WhileCartGrounded(MineCart.MineCartCharacter character, float time)
			{
			}

			// Token: 0x060042FA RID: 17146 RVA: 0x00316B9C File Offset: 0x00314D9C
			public override void _Draw(SpriteBatch b)
			{
				if (this.trackType == MineCart.Track.TrackType.SlimeUpSlope)
				{
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle(192, 144, 16, 32)), this._game.trackTint, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle((int)(160 + this.trackType * (MineCart.Track.TrackType)16), 144, 16, 32)), Color.White, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f - 0.0001f);
				}
				else if (this.trackType >= MineCart.Track.TrackType.MushroomLeft && this.trackType <= MineCart.Track.TrackType.MushroomRight)
				{
					if (base.GetType() == typeof(MineCart.Track))
					{
						b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle(304 + (this.trackType - MineCart.Track.TrackType.MushroomLeft) * 16, 736, 16, 48)), Color.White, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
					}
					else
					{
						b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle(352 + (this.trackType - MineCart.Track.TrackType.MushroomLeft) * 16, 736, 16, 48)), Color.White, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
					}
				}
				else if (this._game.currentTheme == 4 && (this.trackType == MineCart.Track.TrackType.UpSlope || this.trackType == MineCart.Track.TrackType.DownSlope))
				{
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle(256 + (this.trackType - MineCart.Track.TrackType.UpSlope) * 16, 144, 16, 32)), this._game.trackTint, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
				}
				else
				{
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle?(new Rectangle((int)(160 + this.trackType * (MineCart.Track.TrackType)16), 144, 16, 32)), this._game.trackTint, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
				}
				if (this.trackType == MineCart.Track.TrackType.MushroomLeft || this.trackType == MineCart.Track.TrackType.MushroomRight)
				{
					return;
				}
				float darkness = 0f;
				if (this.trackType == MineCart.Track.TrackType.MushroomMiddle)
				{
					for (float y = base.drawnPosition.Y; y < (float)this._game.screenHeight; y += (float)(this._game.tileSize * 4))
					{
						b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, y + 16f)), new Rectangle?(new Rectangle(320, 784, 16, 64)), Color.White, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.01f);
						b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, y + 16f)), new Rectangle?(new Rectangle(368, 784, 16, 64)), this._game.trackShadowTint * darkness, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.005f);
						darkness += 0.1f;
					}
					return;
				}
				bool flipper = this._showSecondTile;
				for (float y2 = base.drawnPosition.Y; y2 < (float)this._game.screenHeight; y2 += (float)this._game.tileSize)
				{
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, y2)), new Rectangle?((this._game.currentTheme == 4) ? new Rectangle(16 + ((flipper > false) ? 1 : 0) * 16, 160, 16, 16) : new Rectangle(16 + ((flipper > false) ? 1 : 0) * 16, 32, 16, 16)), this._game.trackTint, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.01f);
					b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(base.drawnPosition.X, y2)), new Rectangle?((this._game.currentTheme == 4) ? new Rectangle(16 + ((flipper > false) ? 1 : 0) * 16, 160, 16, 16) : new Rectangle(16 + ((flipper > false) ? 1 : 0) * 16, 32, 16, 16)), this._game.trackShadowTint * darkness, 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.005f);
					darkness += 0.1f;
					flipper = !flipper;
				}
			}

			// Token: 0x060042FB RID: 17147 RVA: 0x003172B8 File Offset: 0x003154B8
			public bool CanLandHere(Vector2 test_position)
			{
				int track_y = this.GetYAtPoint(test_position.X);
				return test_position.Y >= (float)(track_y - 2) && test_position.Y <= (float)(track_y + 8);
			}

			// Token: 0x060042FC RID: 17148 RVA: 0x003172F0 File Offset: 0x003154F0
			public int GetYAtPoint(float x)
			{
				int local_x = (int)(x - this.position.X);
				switch (this.trackType)
				{
				case MineCart.Track.TrackType.UpSlope:
					return (int)(this.position.Y - 2f - (float)local_x);
				case MineCart.Track.TrackType.DownSlope:
					return (int)(this.position.Y - 2f - 16f + (float)local_x);
				case MineCart.Track.TrackType.IceDownSlope:
					return (int)(this.position.Y - 2f - 16f + (float)local_x);
				case MineCart.Track.TrackType.SlimeUpSlope:
					return (int)(this.position.Y - 2f - (float)local_x);
				default:
					return (int)(this.position.Y - 2f);
				}
			}

			// Token: 0x04002D63 RID: 11619
			public MineCart.Obstacle obstacle;

			// Token: 0x04002D64 RID: 11620
			private bool _showSecondTile;

			// Token: 0x04002D65 RID: 11621
			public MineCart.Track.TrackType trackType;

			// Token: 0x02000756 RID: 1878
			public enum TrackType
			{
				// Token: 0x040031B7 RID: 12727
				None = -1,
				// Token: 0x040031B8 RID: 12728
				Straight,
				// Token: 0x040031B9 RID: 12729
				UpSlope = 2,
				// Token: 0x040031BA RID: 12730
				DownSlope,
				// Token: 0x040031BB RID: 12731
				IceDownSlope,
				// Token: 0x040031BC RID: 12732
				SlimeUpSlope,
				// Token: 0x040031BD RID: 12733
				MushroomLeft,
				// Token: 0x040031BE RID: 12734
				MushroomMiddle,
				// Token: 0x040031BF RID: 12735
				MushroomRight
			}
		}

		// Token: 0x020005D3 RID: 1491
		public class PlayerMineCartCharacter : MineCart.MineCartCharacter, MineCart.ICollideable
		{
			// Token: 0x060042FD RID: 17149 RVA: 0x003173A3 File Offset: 0x003155A3
			public Rectangle GetLocalBounds()
			{
				return new Rectangle(-4, -12, 8, 12);
			}

			// Token: 0x060042FE RID: 17150 RVA: 0x003173B4 File Offset: 0x003155B4
			public virtual Rectangle GetBounds()
			{
				Rectangle bounds = this.GetLocalBounds();
				bounds.X += (int)this.position.X;
				bounds.Y += (int)this.position.Y;
				return bounds;
			}

			// Token: 0x060042FF RID: 17151 RVA: 0x003173F8 File Offset: 0x003155F8
			protected override void _Update(float time)
			{
				if (!base.IsActive())
				{
					return;
				}
				int old_x_pos = (int)(this.position.X / (float)this._game.tileSize);
				float old_y_velocity = this.velocity.Y;
				if (this._game.gameState != MineCart.GameStates.Cutscene && this._jumping && !this._game.isJumpPressed && !this._game.gamePaused)
				{
					base.ReleaseJump();
				}
				base._Update(time);
				if (this._grounded && this._game.respawnCounter <= 0)
				{
					if (this._game.minecartLoop.IsPaused && this._game.currentTheme != 7)
					{
						this._game.minecartLoop.Resume();
					}
					if (old_x_pos != (int)(this.position.X / (float)this._game.tileSize) && Game1.random.NextBool())
					{
						this.minecartBumpOffset = (float)(-(float)Game1.random.Next(1, 3));
					}
				}
				else if (!this._grounded)
				{
					if (!this._game.minecartLoop.IsPaused)
					{
						this._game.minecartLoop.Pause();
					}
					this.minecartBumpOffset = 0f;
				}
				this.minecartBumpOffset = Utility.MoveTowards(this.minecartBumpOffset, 0f, time * 20f);
				foreach (MineCart.Pickup pickup in this._game.GetOverlaps<MineCart.Pickup>(this))
				{
					pickup.Collect(this);
				}
				MineCart.Obstacle obstacle = this._game.GetOverlap<MineCart.Obstacle>(this);
				if (this._game.GetOverlap<MineCart.Obstacle>(this) != null)
				{
					if (((this.velocity.Y <= 0f && old_y_velocity <= 0f && this.position.Y >= obstacle.position.Y - 1f) || !obstacle.OnBounce(this)) && !obstacle.OnBump(this))
					{
						this._game.Die();
					}
					return;
				}
			}

			// Token: 0x06004300 RID: 17152 RVA: 0x00317604 File Offset: 0x00315804
			public override void OnJump()
			{
				Game1.playSound("pickUpItem", new int?(200));
			}

			// Token: 0x06004301 RID: 17153 RVA: 0x0031761C File Offset: 0x0031581C
			public override void OnFall()
			{
				Game1.playSound("parry", null);
				this._game.createSparkShower();
			}

			// Token: 0x06004302 RID: 17154 RVA: 0x00317648 File Offset: 0x00315848
			public override void OnLand()
			{
				if (this.currentTrackType == MineCart.Track.TrackType.SlimeUpSlope)
				{
					Game1.playSound("slimeHit", null);
				}
				else
				{
					if (this.currentTrackType >= MineCart.Track.TrackType.MushroomLeft && this.currentTrackType <= MineCart.Track.TrackType.MushroomRight)
					{
						Game1.playSound("slimeHit", null);
						bool purple = base.GetTrack(default(Vector2)).GetType() != typeof(MineCart.Track);
						for (int i = 0; i < 3; i++)
						{
							this._game.AddEntity<MineCart.MineDebris>(new MineCart.MineDebris(new Rectangle(362 + (purple ? 5 : 0), 802, 5, 4), this.position, (float)Game1.random.Next(-30, 31), (float)Game1.random.Next(-50, -39), 0f, 1f, 0.75f, 1f, 1, 1f, 0.15f, false, 0f));
						}
						return;
					}
					Game1.playSound("parry", null);
				}
				this._game.createSparkShower();
			}

			// Token: 0x06004303 RID: 17155 RVA: 0x00317768 File Offset: 0x00315968
			public override void OnTrackChange()
			{
				if (this._hasJustSnapped)
				{
					return;
				}
				if (this._grounded)
				{
					if (this.currentTrackType == MineCart.Track.TrackType.SlimeUpSlope)
					{
						Game1.playSound("slimeHit", null);
					}
					else
					{
						if (this.currentTrackType >= MineCart.Track.TrackType.MushroomLeft && this.currentTrackType <= MineCart.Track.TrackType.MushroomRight)
						{
							return;
						}
						Game1.playSound("parry", null);
					}
					this._game.createSparkShower();
				}
			}
		}

		// Token: 0x020005D4 RID: 1492
		public class CheckpointIndicator : MineCart.Entity
		{
			// Token: 0x06004305 RID: 17157 RVA: 0x003177E0 File Offset: 0x003159E0
			protected override void _Update(float time)
			{
				if (this._activated)
				{
					this.swayTimer += time * 6.2831855f;
					if ((double)this.swayTimer >= 6.283185307179586)
					{
						this.swayTimer = 0f;
						this.swayRotation -= 20f;
						if (this.swayRotation <= 30f)
						{
							this.swayRotation = 30f;
						}
					}
					this.rotation = (float)Math.Sin((double)this.swayTimer) * this.swayRotation;
				}
			}

			// Token: 0x06004306 RID: 17158 RVA: 0x0031786C File Offset: 0x00315A6C
			public void Activate()
			{
				if (!this._activated)
				{
					Game1.playSound("fireball", null);
					this._activated = true;
				}
			}

			// Token: 0x06004307 RID: 17159 RVA: 0x0031789C File Offset: 0x00315A9C
			public override void _Draw(SpriteBatch b)
			{
				float rad_rotation = this.rotation * 3.1415927f / 180f;
				Vector2 lantern_offset = new Vector2(0f, -12f);
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(16, 112, 16, 16)), this._game.trackTint, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.31f);
				if (this._activated)
				{
					b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + lantern_offset), new Rectangle?(new Rectangle(48, 112, 16, 16)), Color.White, rad_rotation, new Vector2(8f, 16f) + lantern_offset, this._game.GetPixelScale(), SpriteEffects.None, 0.3f);
					return;
				}
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + lantern_offset), new Rectangle?(new Rectangle(32, 112, 16, 16)), Color.White, rad_rotation, new Vector2(8f, 16f) + lantern_offset, this._game.GetPixelScale(), SpriteEffects.None, 0.3f);
			}

			// Token: 0x04002D66 RID: 11622
			public const int CENTER_TO_POST_BASE_OFFSET = 5;

			// Token: 0x04002D67 RID: 11623
			public float rotation;

			// Token: 0x04002D68 RID: 11624
			protected bool _activated;

			// Token: 0x04002D69 RID: 11625
			public float swayRotation = 120f;

			// Token: 0x04002D6A RID: 11626
			public float swayTimer;
		}

		// Token: 0x020005D5 RID: 1493
		public class GoalIndicator : MineCart.Entity
		{
			// Token: 0x06004309 RID: 17161 RVA: 0x00317A19 File Offset: 0x00315C19
			public void Activate()
			{
				if (!this._activated)
				{
					this._activated = true;
				}
			}

			// Token: 0x0600430A RID: 17162 RVA: 0x00317A2A File Offset: 0x00315C2A
			protected override void _Update(float time)
			{
				if (this._activated)
				{
					this.rotation += time * 360f / 0.25f;
				}
			}

			// Token: 0x0600430B RID: 17163 RVA: 0x00317A50 File Offset: 0x00315C50
			public override void _Draw(SpriteBatch b)
			{
				float rad_rotation = this.rotation * 3.1415927f / 180f;
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition), new Rectangle?(new Rectangle(16, 128, 16, 16)), this._game.trackTint, 0f, new Vector2(8f, 16f), this._game.GetPixelScale(), SpriteEffects.None, 0.31f);
				Vector2 sign_offset = new Vector2(0f, -8f);
				b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + sign_offset), new Rectangle?(new Rectangle(32, 128, 16, 16)), Color.White, rad_rotation, new Vector2(8f, 16f) + sign_offset, this._game.GetPixelScale(), SpriteEffects.None, 0.3f);
			}

			// Token: 0x04002D6B RID: 11627
			public float rotation;

			// Token: 0x04002D6C RID: 11628
			protected bool _activated;
		}

		// Token: 0x020005D6 RID: 1494
		public class MineCartCharacter : MineCart.BaseCharacter
		{
			// Token: 0x0600430D RID: 17165 RVA: 0x00317B59 File Offset: 0x00315D59
			public void QueueJump()
			{
				this._jumpBuffer = 0.25f;
			}

			// Token: 0x0600430E RID: 17166 RVA: 0x00317B66 File Offset: 0x00315D66
			public virtual void OnDie()
			{
				this.cartScale = Vector2.One;
				this._speedMultiplier = 1f;
			}

			// Token: 0x0600430F RID: 17167 RVA: 0x00317B80 File Offset: 0x00315D80
			public void SnapToFloor()
			{
				List<MineCart.Track> position_tracks = this._game.GetTracksForXPosition(this.position.X);
				if (position_tracks != null)
				{
					int i = 0;
					if (i < position_tracks.Count)
					{
						MineCart.Track track = position_tracks[i];
						this.position.Y = (float)track.GetYAtPoint(this.position.X);
						this._grounded = true;
						this.gravity = 0f;
						this.velocity.Y = 0f;
						this.characterExtraHeight = 0f;
						this.minecartBumpOffset = 0f;
						this._hasJustSnapped = true;
						return;
					}
				}
			}

			// Token: 0x06004310 RID: 17168 RVA: 0x00317C1C File Offset: 0x00315E1C
			public MineCart.Track GetTrack(Vector2 offset = default(Vector2))
			{
				foreach (int x_offset in new int[]
				{
					0,
					4,
					-4
				})
				{
					Vector2 test_position = this.position + offset + new Vector2((float)x_offset, 0f);
					List<MineCart.Track> tracks = this._game.GetTracksForXPosition(test_position.X);
					if (tracks != null)
					{
						for (int j = 0; j < tracks.Count; j++)
						{
							if (tracks[j].CanLandHere(test_position))
							{
								return tracks[j];
							}
						}
					}
				}
				return null;
			}

			// Token: 0x06004311 RID: 17169 RVA: 0x00317CB4 File Offset: 0x00315EB4
			protected override void _Update(float time)
			{
				if (this._game.respawnCounter > 0)
				{
					this.characterExtraHeight = 0f;
					this.rotation = 0f;
					this._jumpBuffer = 0f;
					this.jumpGracePeriod = 0f;
					this.gravity = 0f;
					this.velocity.Y = 0f;
					this.minecartBumpOffset = 0f;
					this.SnapToFloor();
					return;
				}
				base._Update(time);
				if (this.jumpGracePeriod > 0f)
				{
					this.jumpGracePeriod -= time;
				}
				if ((this._grounded || this.jumpGracePeriod > 0f) && this._jumpBuffer > 0f && this._game.isJumpPressed)
				{
					this._jumpBuffer = 0f;
					this.Jump();
				}
				else if (this._jumpBuffer > 0f)
				{
					this._jumpBuffer -= time;
				}
				bool found_valid_ground = false;
				MineCart.Track.TrackType old_track_type = this.currentTrackType;
				MineCart.Track track = this.GetTrack(default(Vector2));
				if (track != null && this._grounded)
				{
					track.WhileCartGrounded(this, time);
				}
				bool was_grounded = this._grounded;
				if (this.velocity.Y >= 0f && track != null)
				{
					this.position.Y = (float)track.GetYAtPoint(this.position.X);
					this.currentTrackType = track.trackType;
					if (!this._grounded)
					{
						this.cartScale = new Vector2(1.5f, 0.5f);
						this.rotation = 0f;
						this.OnLand();
					}
					found_valid_ground = true;
					this.velocity.Y = 0f;
					this._grounded = true;
				}
				else if (this._grounded && this.velocity.Y >= 0f)
				{
					track = this.GetTrack(new Vector2(0f, 2f));
					if (track != null)
					{
						this.position.Y = (float)track.GetYAtPoint(this.position.X);
						this.currentTrackType = track.trackType;
						found_valid_ground = true;
						this.velocity.Y = 0f;
						this._grounded = true;
					}
				}
				if (!found_valid_ground)
				{
					if (this._grounded)
					{
						this.gravity = 0f;
						this.velocity.Y = this.GetMaxFallSpeed();
						if (!this.IsJumping())
						{
							this.OnFall();
							this.jumpGracePeriod = MineCart.maxJumpGraceTime;
						}
					}
					this.currentTrackType = MineCart.Track.TrackType.None;
					this._grounded = false;
				}
				float ground_rotation = 0f;
				switch (this.currentTrackType)
				{
				case MineCart.Track.TrackType.Straight:
					ground_rotation = 0f;
					break;
				case MineCart.Track.TrackType.UpSlope:
					ground_rotation = -45f;
					break;
				case MineCart.Track.TrackType.DownSlope:
					ground_rotation = 30f;
					break;
				}
				if (this.IsJumping())
				{
					this.rotation = Utility.MoveTowards(this.rotation, -45f, 300f * time);
					this.characterExtraHeight = 0f;
				}
				else if (!this._grounded)
				{
					this.rotation = Utility.MoveTowards(this.rotation, 0f, 100f * time);
					this.characterExtraHeight = Utility.MoveTowards(this.characterExtraHeight, 16f, 24f * time);
				}
				else
				{
					this.rotation = Utility.MoveTowards(this.rotation, ground_rotation, 360f * time);
					this.characterExtraHeight = Utility.MoveTowards(this.characterExtraHeight, 0f, 128f * time);
				}
				this.cartScale.X = Utility.MoveTowards(this.cartScale.X, 1f, 4f * time);
				this.cartScale.Y = Utility.MoveTowards(this.cartScale.Y, 1f, 4f * time);
				if (was_grounded && old_track_type != this.currentTrackType)
				{
					if ((this.rotation < 0f && ground_rotation > 0f) || (this.rotation > 0f && ground_rotation < 0f))
					{
						this.rotation = 0f;
					}
					this.OnTrackChange();
				}
				if (this.forcedJumpTime > 0f)
				{
					this.forcedJumpTime -= time;
					if (this._grounded)
					{
						this.forcedJumpTime = 0f;
					}
				}
				if (!this._grounded)
				{
					if (this._jumping)
					{
						this._jumpFloatAge += time;
						if (this._jumpFloatAge < this.jumpFloatDuration)
						{
							this.gravity = 0f;
							this.velocity.Y = Utility.Lerp(0f, -this.jumpStrength, this._jumpFloatAge / this.jumpFloatDuration);
						}
						else if (this.velocity.Y <= this._jumpMomentumThreshhold * 2f)
						{
							this.gravity += time * this.jumpGravity;
						}
						else
						{
							this.velocity.Y = this._jumpMomentumThreshhold;
							this.ReleaseJump();
						}
					}
					else
					{
						this.gravity += time * this.fallGravity;
					}
					this.velocity.Y = this.velocity.Y + time * this.gravity;
				}
				else
				{
					this._jumping = false;
				}
				if (this._game.currentTheme == 5)
				{
					this._speedMultiplier = 1f;
				}
				MineCart.Track.TrackType trackType = this.currentTrackType;
				if (trackType != MineCart.Track.TrackType.IceDownSlope)
				{
					if (trackType == MineCart.Track.TrackType.SlimeUpSlope)
					{
						this._speedMultiplier = 0.5f;
					}
					else if (this._grounded)
					{
						this._speedMultiplier = Utility.MoveTowards(this._speedMultiplier, 1f, time * 6f);
					}
				}
				else
				{
					this._speedMultiplier = Utility.MoveTowards(this._speedMultiplier, 3f, time * 2f);
				}
				if (!(this is MineCart.PlayerMineCartCharacter))
				{
					this._speedMultiplier = 1f;
				}
				this.position.X = this.position.X + time * this.velocity.X * this._speedMultiplier;
				this.position.Y = this.position.Y + time * this.velocity.Y;
				if (this.velocity.Y > 0f)
				{
					this._jumping = false;
				}
				if (this.velocity.Y > this.GetMaxFallSpeed())
				{
					this.velocity.Y = this.GetMaxFallSpeed();
				}
				if (this._hasJustSnapped)
				{
					this._hasJustSnapped = false;
				}
			}

			// Token: 0x06004312 RID: 17170 RVA: 0x003182D8 File Offset: 0x003164D8
			public float GetMaxFallSpeed()
			{
				if (this._game.currentTheme == 2)
				{
					return 75f;
				}
				return this.maxFallSpeed;
			}

			// Token: 0x06004313 RID: 17171 RVA: 0x003182F4 File Offset: 0x003164F4
			public virtual void OnLand()
			{
			}

			// Token: 0x06004314 RID: 17172 RVA: 0x003182F6 File Offset: 0x003164F6
			public virtual void OnTrackChange()
			{
			}

			// Token: 0x06004315 RID: 17173 RVA: 0x003182F8 File Offset: 0x003164F8
			public virtual void OnFall()
			{
			}

			// Token: 0x06004316 RID: 17174 RVA: 0x003182FA File Offset: 0x003164FA
			public virtual void OnJump()
			{
			}

			// Token: 0x06004317 RID: 17175 RVA: 0x003182FC File Offset: 0x003164FC
			public void ReleaseJump()
			{
				if (this.forcedJumpTime > 0f)
				{
					return;
				}
				if (this._jumping && this.velocity.Y < 0f)
				{
					this._jumping = false;
					this.gravity = 0f;
					if (this.velocity.Y < this._jumpMomentumThreshhold)
					{
						this.velocity.Y = this._jumpMomentumThreshhold;
					}
				}
			}

			// Token: 0x06004318 RID: 17176 RVA: 0x00318367 File Offset: 0x00316567
			public bool IsJumping()
			{
				return this._jumping;
			}

			// Token: 0x06004319 RID: 17177 RVA: 0x0031836F File Offset: 0x0031656F
			public bool IsGrounded()
			{
				return this._grounded;
			}

			// Token: 0x0600431A RID: 17178 RVA: 0x00318378 File Offset: 0x00316578
			public void Bounce(float forced_bounce_time = 0f)
			{
				this.forcedJumpTime = forced_bounce_time;
				this._jumping = true;
				this.gravity = 0f;
				this.cartScale = new Vector2(0.5f, 1.5f);
				this.velocity.Y = -this.jumpStrength;
				this._grounded = false;
			}

			// Token: 0x0600431B RID: 17179 RVA: 0x003183CC File Offset: 0x003165CC
			public void Jump()
			{
				if (this._grounded || this.jumpGracePeriod > 0f)
				{
					this._jumping = true;
					this.gravity = 0f;
					this._jumpFloatAge = 0f;
					this.cartScale = new Vector2(0.5f, 1.5f);
					this.OnJump();
					this.velocity.Y = -this.jumpStrength;
					this._grounded = false;
				}
			}

			// Token: 0x0600431C RID: 17180 RVA: 0x0031843F File Offset: 0x0031663F
			public void ForceGrounded()
			{
				this._grounded = true;
				this.gravity = 0f;
				this.velocity.Y = 0f;
			}

			// Token: 0x0600431D RID: 17181 RVA: 0x00318464 File Offset: 0x00316664
			public override void _Draw(SpriteBatch b)
			{
				if (this._game.respawnCounter / 200 % 2 == 0)
				{
					float rad_rotation = this.rotation * 3.1415927f / 180f;
					Vector2 right = new Vector2((float)Math.Cos((double)rad_rotation), -(float)Math.Sin((double)rad_rotation));
					Vector2 up = new Vector2((float)Math.Sin((double)rad_rotation), -(float)Math.Cos((double)rad_rotation));
					b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + up * -this.minecartBumpOffset + up * 4f), new Rectangle?(new Rectangle(0, 0, 16, 16)), Color.White, rad_rotation, new Vector2(8f, 14f), this.cartScale * this._game.GetPixelScale(), SpriteEffects.None, 0.45f);
					b.Draw(this._game.texture, this._game.TransformDraw(base.drawnPosition + up * -this.minecartBumpOffset + up * 4f), new Rectangle?(new Rectangle(0, 16, 16, 16)), Color.White, rad_rotation, new Vector2(8f, 14f), this.cartScale * this._game.GetPixelScale(), SpriteEffects.None, 0.4f);
					b.Draw(Game1.mouseCursors, this._game.TransformDraw(base.drawnPosition + right * -2f + up * -this.minecartBumpOffset + up * 12f + new Vector2(0f, -this.characterExtraHeight)), new Rectangle?(new Rectangle(294 + (int)(this._game.totalTimeMS % 400.0) / 100 * 16, 1432, 16, 16)), Color.Lime, 0f, new Vector2(8f, 8f), this._game.GetPixelScale() * 2f / 3f, SpriteEffects.None, 0.425f);
				}
			}

			// Token: 0x04002D6D RID: 11629
			public float minecartBumpOffset;

			// Token: 0x04002D6E RID: 11630
			public float jumpStrength = 300f;

			// Token: 0x04002D6F RID: 11631
			public float maxFallSpeed = 150f;

			// Token: 0x04002D70 RID: 11632
			public float jumpGravity = 3400f;

			// Token: 0x04002D71 RID: 11633
			public float fallGravity = 3000f;

			// Token: 0x04002D72 RID: 11634
			public float jumpFloatDuration = 0.1f;

			// Token: 0x04002D73 RID: 11635
			public float gravity;

			// Token: 0x04002D74 RID: 11636
			protected float _jumpBuffer;

			// Token: 0x04002D75 RID: 11637
			protected float _jumpFloatAge;

			// Token: 0x04002D76 RID: 11638
			protected float _speedMultiplier = 1f;

			// Token: 0x04002D77 RID: 11639
			protected float _jumpMomentumThreshhold = -30f;

			// Token: 0x04002D78 RID: 11640
			public float jumpGracePeriod;

			// Token: 0x04002D79 RID: 11641
			protected bool _grounded = true;

			// Token: 0x04002D7A RID: 11642
			protected bool _jumping;

			// Token: 0x04002D7B RID: 11643
			public float rotation;

			// Token: 0x04002D7C RID: 11644
			public Vector2 cartScale = Vector2.One;

			// Token: 0x04002D7D RID: 11645
			public MineCart.Track.TrackType currentTrackType = MineCart.Track.TrackType.None;

			// Token: 0x04002D7E RID: 11646
			public float characterExtraHeight;

			// Token: 0x04002D7F RID: 11647
			protected bool _hasJustSnapped;

			// Token: 0x04002D80 RID: 11648
			public float forcedJumpTime;
		}
	}
}
