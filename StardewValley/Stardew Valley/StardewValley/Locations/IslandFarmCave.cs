using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002D4 RID: 724
	public class IslandFarmCave : IslandLocation
	{
		// Token: 0x06002F94 RID: 12180 RVA: 0x002592D6 File Offset: 0x002574D6
		public IslandFarmCave()
		{
		}

		// Token: 0x06002F95 RID: 12181 RVA: 0x0025930B File Offset: 0x0025750B
		public IslandFarmCave(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002F96 RID: 12182 RVA: 0x00259344 File Offset: 0x00257544
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.gourmandRequestsFulfilled, "gourmandRequestsFulfilled").AddField(this.requestGourmandCheckEvent, "requestGourmandCheckEvent").AddField(this.gourmandResponseEvent, "gourmandResponseEvent").AddField(this.gourmandMutex.NetFields, "gourmandMutex.NetFields");
			this.requestGourmandCheckEvent.onEvent += this.OnRequestGourmandCheck;
			this.gourmandResponseEvent.onEvent += this.OnGourmandResponse;
		}

		// Token: 0x06002F97 RID: 12183 RVA: 0x002593D4 File Offset: 0x002575D4
		public virtual void OnRequestGourmandCheck()
		{
			if (Game1.IsMasterGame)
			{
				string gourmand_response = "";
				IslandWest island_farm = Game1.RequireLocation<IslandWest>("IslandWest", false);
				foreach (Vector2 key in island_farm.terrainFeatures.Keys)
				{
					HoeDirt dirt = island_farm.terrainFeatures[key] as HoeDirt;
					if (dirt != null && dirt.crop != null)
					{
						bool harvestable = dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && (!dirt.crop.fullyGrown.Value || dirt.crop.dayOfCurrentPhase.Value <= 0);
						if (dirt.crop.indexOfHarvest.Value == this.IndexForRequest(this.gourmandRequestsFulfilled.Value))
						{
							if (harvestable)
							{
								Point target_tile = new Point((int)key.X, (int)key.Y);
								Point player_tile = this.FindNearbyUnoccupiedTileThatFitsCharacter(island_farm, target_tile.X, target_tile.Y, 1, null);
								Point gourmand_tile = this.FindNearbyUnoccupiedTileThatFitsCharacter(island_farm, target_tile.X, target_tile.Y, 2, new Point?(player_tile));
								int farmer_direction = this.GetRelativeDirection(player_tile, target_tile);
								this.gourmandResponseEvent.Fire(string.Concat(new string[]
								{
									key.X.ToString(),
									" ",
									key.Y.ToString(),
									" ",
									player_tile.X.ToString(),
									" ",
									player_tile.Y.ToString(),
									" ",
									farmer_direction.ToString(),
									" ",
									gourmand_tile.X.ToString(),
									" ",
									gourmand_tile.Y.ToString(),
									" 2"
								}));
								return;
							}
							gourmand_response = "inProgress";
						}
					}
				}
				this.gourmandResponseEvent.Fire(gourmand_response);
			}
		}

		// Token: 0x06002F98 RID: 12184 RVA: 0x0025963C File Offset: 0x0025783C
		public int GetRelativeDirection(Point source, Point destination)
		{
			Point offset = new Point(destination.X - source.X, destination.Y - source.Y);
			if (Math.Abs(offset.Y) > Math.Abs(offset.X))
			{
				if (offset.Y < 0)
				{
					return 0;
				}
				return 2;
			}
			else
			{
				if (offset.X < 0)
				{
					return 3;
				}
				return 1;
			}
		}

		// Token: 0x06002F99 RID: 12185 RVA: 0x0025969C File Offset: 0x0025789C
		public Point FindNearbyUnoccupiedTileThatFitsCharacter(GameLocation location, int target_x, int target_y, int width = 1, Point? invalid_tile = null)
		{
			HashSet<Point> visited_tiles = new HashSet<Point>();
			List<Point> open_tiles = new List<Point>();
			open_tiles.Add(new Point(target_x, target_y));
			visited_tiles.Add(new Point(target_x, target_y));
			Point[] offsets = new Point[]
			{
				new Point(-1, 0),
				new Point(1, 0),
				new Point(0, -1),
				new Point(0, 1)
			};
			int i = 0;
			while (i < 500 && open_tiles.Count != 0)
			{
				Point tile = open_tiles[0];
				open_tiles.RemoveAt(0);
				foreach (Point offset in offsets)
				{
					Point next_tile = new Point(tile.X + offset.X, tile.Y + offset.Y);
					if (!visited_tiles.Contains(next_tile))
					{
						open_tiles.Add(next_tile);
					}
				}
				if (!visited_tiles.Contains(tile) && (invalid_tile == null || tile.X != invalid_tile.Value.X || tile.Y != invalid_tile.Value.Y))
				{
					visited_tiles.Add(tile);
					bool fail = false;
					int height = 1;
					for (int w = 0; w < width; w++)
					{
						for (int h = 0; h < height; h++)
						{
							Point checked_tile = new Point(tile.X + w, tile.Y + h);
							Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(checked_tile.X * 64, checked_tile.Y * 64, 64, 64);
							rect.Inflate(-4, -4);
							if (checked_tile.X == target_x && checked_tile.Y == target_y + 1)
							{
								fail = true;
								break;
							}
							if (invalid_tile != null && invalid_tile.Value == checked_tile)
							{
								fail = true;
								break;
							}
							if (location.IsTileOccupiedBy(new Vector2((float)checked_tile.X, (float)checked_tile.Y), CollisionMask.All, CollisionMask.TerrainFeatures, false))
							{
								fail = true;
								break;
							}
						}
					}
					if (!fail)
					{
						return tile;
					}
				}
				i++;
			}
			return new Point(target_x, target_y);
		}

		// Token: 0x06002F9A RID: 12186 RVA: 0x002598D0 File Offset: 0x00257AD0
		public virtual void OnGourmandResponse(string response)
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			if (response == null || response.Length != 0)
			{
				if (!(response == "inProgress"))
				{
					string[] split = ArgUtility.SplitBySpace(response);
					StringBuilder sb = new StringBuilder();
					sb.Append("none/-1000 -1000/");
					sb.Append(string.Concat(new string[]
					{
						"farmer ",
						split[2],
						" ",
						split[3],
						" ",
						split[4],
						"/"
					}));
					sb.Append("changeLocation IslandWest/");
					sb.Append(string.Concat(new string[]
					{
						"viewport ",
						split[0],
						" ",
						split[1],
						"/"
					}));
					sb.Append(string.Concat(new string[]
					{
						"playMusic none/addTemporaryActor Gourmand 32 32 ",
						split[5],
						" ",
						split[6],
						" ",
						split[7],
						" true character/positionOffset Gourmand 0 1/positionOffset farmer 0 1/animate Gourmand false true 500 2 3/"
					}));
					sb.Append(string.Concat(new string[]
					{
						"viewport ",
						split[0],
						" ",
						split[1],
						" true/"
					}));
					sb.Append("pause 3000/playSound croak/");
					foreach (string text in Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + this.gourmandRequestsFulfilled.Value.ToString() + "_Success").Split('|', StringSplitOptions.None))
					{
						sb.Append("message \"" + text + "\"/pause 250/");
					}
					sb.Append("pause 1000/end");
					Event evt = new Event(sb.ToString(), null);
					if (this.triggeredGourmand)
					{
						Event evt2 = evt;
						evt2.onEventFinished = (Action)Delegate.Combine(evt2.onEventFinished, new Action(delegate()
						{
							if (Game1.locationRequest != null)
							{
								Game1.locationRequest.OnWarp += this.CompleteGourmandRequest;
								return;
							}
							this.CompleteGourmandRequest();
						}));
					}
					Game1.globalFadeToBlack(delegate
					{
						Game1.currentLocation.startEvent(evt);
					}, 0.02f);
					Game1.player.freezePause = 0;
				}
				else
				{
					Game1.player.freezePause = 0;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_InProgress"));
				}
			}
			else if (this.triggeredGourmand)
			{
				Game1.player.freezePause = 0;
				this.ShowGourmandUnhappy();
			}
			this.triggeredGourmand = false;
		}

		// Token: 0x06002F9B RID: 12187 RVA: 0x00259B48 File Offset: 0x00257D48
		public virtual void CompleteGourmandRequest()
		{
			if (!this.gourmandMutex.IsLockHeld())
			{
				return;
			}
			Game1.player.freezePause = 1250;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("croak", null);
				this.gourmand.shake(1000);
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(this.GiveReward));
				if (this.gourmandRequestsFulfilled.Value < IslandFarmCave.TOTAL_GOURMAND_REQUESTS - 1)
				{
					Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_Reward").Split('|', StringSplitOptions.None));
					return;
				}
				Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_LastReward").Split('|', StringSplitOptions.None));
			}, 1000);
		}

		// Token: 0x06002F9C RID: 12188 RVA: 0x00259B80 File Offset: 0x00257D80
		public virtual void GiveReward()
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(4.5f, 4f) * 64f, 3, this, -1, false);
			for (int i = 0; i < 4; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(4.5f, 4f) * 64f, 1, this, -1, false);
			}
			NetInt netInt = this.gourmandRequestsFulfilled;
			int value = netInt.Value;
			netInt.Value = value + 1;
			Game1.player.team.MarkCollectedNut("IslandGourmand" + this.gourmandRequestsFulfilled.Value.ToString());
			this.gourmandMutex.ReleaseLock();
		}

		// Token: 0x06002F9D RID: 12189 RVA: 0x00259C48 File Offset: 0x00257E48
		public void ShowGourmandUnhappy()
		{
			Game1.playSound("croak", null);
			this.gourmand.shake(1000);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestFailed"));
			if (this.gourmandMutex.IsLockHeld())
			{
				this.gourmandMutex.ReleaseLock();
			}
		}

		// Token: 0x06002F9E RID: 12190 RVA: 0x00259CA8 File Offset: 0x00257EA8
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.gourmand = new NPC(new AnimatedSprite("Characters\\Gourmand", 0, 32, 32), new Vector2(4f, 4f) * 64f, "IslandFarmCave", 2, "Gourmand", false, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"));
			this.gourmand.AllowDynamicAppearance = false;
			this.smokeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			this.waterColor.Value = new Color(10, 250, 120);
		}

		// Token: 0x06002F9F RID: 12191 RVA: 0x00259D44 File Offset: 0x00257F44
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.gourmand != null && !Game1.eventUp)
			{
				this.gourmand.draw(b);
			}
			if (this.gourmandRequestsFulfilled.Value < IslandFarmCave.TOTAL_GOURMAND_REQUESTS)
			{
				Point standingPixel = this.gourmand.StandingPixel;
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)standingPixel.X, (float)(standingPixel.Y - 128 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10)), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x06002FA0 RID: 12192 RVA: 0x00259E2A File Offset: 0x0025802A
		public override void DayUpdate(int dayOfMonth)
		{
			this.gourmandMutex.ReleaseLock();
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x06002FA1 RID: 12193 RVA: 0x00259E40 File Offset: 0x00258040
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (this.gourmand != null)
			{
				this.gourmand.update(time, this);
				if (time.TotalGameTime.TotalMilliseconds % 1000.0 < 500.0)
				{
					this.gourmand.Sprite.CurrentFrame = 1;
				}
				else
				{
					this.gourmand.Sprite.CurrentFrame = 0;
				}
			}
			this.requestGourmandCheckEvent.Poll();
			this.gourmandResponseEvent.Poll();
			this.smokeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0f && this.smokeTexture != null)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.smokeTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(2f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.smokeTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(7f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				this.smokeTimer = 1250f;
			}
		}

		// Token: 0x06002FA2 RID: 12194 RVA: 0x0025A12E File Offset: 0x0025832E
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.gourmandMutex.Update(Game1.getOnlineFarmers());
		}

		// Token: 0x06002FA3 RID: 12195 RVA: 0x0025A148 File Offset: 0x00258348
		public virtual void TalkToGourmand()
		{
			List<string> dialogue = new List<string>();
			if (this.gourmandRequestsFulfilled.Value >= IslandFarmCave.TOTAL_GOURMAND_REQUESTS)
			{
				dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Finished").Split('|', StringSplitOptions.None));
			}
			else
			{
				bool flag = !Game1.player.hasOrWillReceiveMail("talkedToGourmand");
				if (flag)
				{
					Game1.addMailForTomorrow("talkedToGourmand", true, false);
					dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Intro").Split("|", StringSplitOptions.None));
					dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestIntro").Split("|", StringSplitOptions.None));
				}
				Game1.playSound("croak", null);
				this.gourmand.shake(1000);
				dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + this.gourmandRequestsFulfilled.Value.ToString()).Split("|", StringSplitOptions.None));
				Response[] responses = base.createYesNoResponses();
				if (!flag)
				{
					Game1.afterDialogues = delegate()
					{
						Game1.afterDialogues = null;
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestQuestion"), responses, "Gourmand");
					};
				}
			}
			Game1.multipleDialogues(dialogue.ToArray());
		}

		// Token: 0x06002FA4 RID: 12196 RVA: 0x0025A284 File Offset: 0x00258484
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "Gourmand_Yes"))
			{
				return questionAndAnswer == "Gourmand_No" || base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			this.triggeredGourmand = true;
			Game1.player.freezePause = 3000;
			this.requestGourmandCheckEvent.Fire();
			return true;
		}

		// Token: 0x06002FA5 RID: 12197 RVA: 0x0025A2DF File Offset: 0x002584DF
		public string IndexForRequest(int request_number)
		{
			switch (request_number)
			{
			case 0:
				return "254";
			case 1:
				return "262";
			case 2:
				return "248";
			default:
				return null;
			}
		}

		// Token: 0x06002FA6 RID: 12198 RVA: 0x0025A308 File Offset: 0x00258508
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "Gourmand")
			{
				this.gourmandMutex.RequestLock(new Action(this.TalkToGourmand), null);
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06002FA7 RID: 12199 RVA: 0x0025A344 File Offset: 0x00258544
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandFarmCave cave = l as IslandFarmCave;
			if (cave != null)
			{
				this.gourmandRequestsFulfilled.Value = cave.gourmandRequestsFulfilled.Value;
			}
		}

		// Token: 0x04002040 RID: 8256
		[XmlIgnore]
		public NPC gourmand;

		// Token: 0x04002041 RID: 8257
		[XmlElement("gourmandRequestsFulfilled")]
		public NetInt gourmandRequestsFulfilled = new NetInt();

		// Token: 0x04002042 RID: 8258
		[XmlIgnore]
		public NetEvent0 requestGourmandCheckEvent = new NetEvent0(false);

		// Token: 0x04002043 RID: 8259
		[XmlIgnore]
		public NetEvent1Field<string, NetString> gourmandResponseEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x04002044 RID: 8260
		[XmlIgnore]
		public bool triggeredGourmand;

		// Token: 0x04002045 RID: 8261
		[XmlIgnore]
		public static int TOTAL_GOURMAND_REQUESTS = 3;

		// Token: 0x04002046 RID: 8262
		[XmlIgnore]
		private NetMutex gourmandMutex = new NetMutex();

		// Token: 0x04002047 RID: 8263
		private Texture2D smokeTexture;

		// Token: 0x04002048 RID: 8264
		private float smokeTimer;
	}
}
