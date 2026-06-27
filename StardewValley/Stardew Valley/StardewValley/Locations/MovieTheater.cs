using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Movies;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Pathfinding;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;

namespace StardewValley.Locations
{
	// Token: 0x020002EB RID: 747
	public class MovieTheater : GameLocation
	{
		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x060031CD RID: 12749 RVA: 0x0027BDFD File Offset: 0x00279FFD
		// (set) Token: 0x060031CE RID: 12750 RVA: 0x0027BE0A File Offset: 0x0027A00A
		protected int CurrentState
		{
			get
			{
				return this.currentState.Value;
			}
			set
			{
				if (Game1.IsMasterGame)
				{
					this.currentState.Value = value;
					return;
				}
				Game1.log.Warn("Tried to set MovieTheater::CurrentState as a farmhand.");
			}
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x060031CF RID: 12751 RVA: 0x0027BE2F File Offset: 0x0027A02F
		// (set) Token: 0x060031D0 RID: 12752 RVA: 0x0027BE3C File Offset: 0x0027A03C
		protected int ShowingId
		{
			get
			{
				return this.showingId.Value;
			}
			set
			{
				if (Game1.IsMasterGame)
				{
					this.showingId.Value = value;
					return;
				}
				Game1.log.Warn("Tried to set MovieTheater::ShowingId as a farmhand.");
			}
		}

		// Token: 0x060031D1 RID: 12753 RVA: 0x0027BE64 File Offset: 0x0027A064
		public MovieTheater()
		{
		}

		// Token: 0x060031D2 RID: 12754 RVA: 0x0027BF30 File Offset: 0x0027A130
		public static void AddMoviePoster(GameLocation location, float x, float y, bool isUpcoming = false)
		{
			MovieData data = isUpcoming ? MovieTheater.GetUpcomingMovie() : MovieTheater.GetMovieToday();
			if (data != null)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = MovieTheater.GetSourceRectForPoster(data.SheetIndex);
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>(data.Texture ?? "LooseSprites\\Movies"),
					sourceRect = sourceRect,
					sourceRectStartingPos = new Vector2((float)sourceRect.X, (float)sourceRect.Y),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 9999f,
					scale = 4f,
					position = new Vector2(x, y),
					layerDepth = 0.01f
				});
			}
		}

		// Token: 0x060031D3 RID: 12755 RVA: 0x0027BFF0 File Offset: 0x0027A1F0
		public MovieTheater(string map, string name) : base(map, name)
		{
			this.CurrentState = 0;
			MovieTheater.GetMovieData();
			this._InitializeMap();
			MovieTheater.GetMovieReactions();
		}

		// Token: 0x060031D4 RID: 12756 RVA: 0x0027C0D6 File Offset: 0x0027A2D6
		public static List<MovieCharacterReaction> GetMovieReactions()
		{
			if (MovieTheater._genericReactions == null)
			{
				MovieTheater._genericReactions = DataLoader.MoviesReactions(Game1.content);
			}
			return MovieTheater._genericReactions;
		}

		// Token: 0x060031D5 RID: 12757 RVA: 0x0027C0F4 File Offset: 0x0027A2F4
		public static string GetConcessionTasteForCharacter(Character character, MovieConcession concession)
		{
			if (MovieTheater._concessionTastes == null)
			{
				MovieTheater._concessionTastes = DataLoader.ConcessionTastes(Game1.content);
			}
			ConcessionTaste universal_taste = null;
			foreach (ConcessionTaste taste in MovieTheater._concessionTastes)
			{
				if (taste.Name == "*")
				{
					universal_taste = taste;
					break;
				}
			}
			foreach (ConcessionTaste taste2 in MovieTheater._concessionTastes)
			{
				if (taste2.Name == character.Name)
				{
					if (taste2.LovedTags.Contains(concession.Name))
					{
						return "love";
					}
					if (taste2.LikedTags.Contains(concession.Name))
					{
						return "like";
					}
					if (taste2.DislikedTags.Contains(concession.Name))
					{
						return "dislike";
					}
					if (universal_taste != null)
					{
						if (universal_taste.LovedTags.Contains(concession.Name))
						{
							return "love";
						}
						if (universal_taste.LikedTags.Contains(concession.Name))
						{
							return "like";
						}
						if (universal_taste.DislikedTags.Contains(concession.Name))
						{
							return "dislike";
						}
					}
					if (concession.Tags == null)
					{
						break;
					}
					using (List<string>.Enumerator enumerator2 = concession.Tags.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							string tag = enumerator2.Current;
							if (taste2.LovedTags.Contains(tag))
							{
								return "love";
							}
							if (taste2.LikedTags.Contains(tag))
							{
								return "like";
							}
							if (taste2.DislikedTags.Contains(tag))
							{
								return "dislike";
							}
							if (universal_taste != null)
							{
								if (universal_taste.LovedTags.Contains(tag))
								{
									return "love";
								}
								if (universal_taste.LikedTags.Contains(tag))
								{
									return "like";
								}
								if (universal_taste.DislikedTags.Contains(tag))
								{
									return "dislike";
								}
							}
						}
						break;
					}
				}
			}
			return "like";
		}

		// Token: 0x060031D6 RID: 12758 RVA: 0x0027C398 File Offset: 0x0027A598
		public static IEnumerable<string> GetPatronNames()
		{
			MovieTheater movieTheater = Game1.getLocationFromName("MovieTheater") as MovieTheater;
			NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>.KeysCollection? keysCollection;
			if (movieTheater == null)
			{
				keysCollection = null;
			}
			else
			{
				NetStringDictionary<int, NetInt> spawnedMoviePatrons = movieTheater._spawnedMoviePatrons;
				keysCollection = ((spawnedMoviePatrons != null) ? new NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>.KeysCollection?(spawnedMoviePatrons.Keys) : null);
			}
			return keysCollection;
		}

		// Token: 0x060031D7 RID: 12759 RVA: 0x0027C3E8 File Offset: 0x0027A5E8
		protected void _InitializeMap()
		{
			this._hangoutPoints = new Dictionary<int, List<Point>>();
			this._maxHangoutGroups = 0;
			Layer paths_layer = this.map.GetLayer("Paths");
			if (paths_layer != null)
			{
				for (int x = 0; x < paths_layer.LayerWidth; x++)
				{
					for (int y = 0; y < paths_layer.LayerHeight; y++)
					{
						string property;
						int hangout_group;
						if (paths_layer.Tiles[x, y] != null && paths_layer.GetTileIndexAt(x, y, null) == 7 && paths_layer.Tiles[x, y].Properties.TryGetValue("group", out property) && int.TryParse(property, out hangout_group))
						{
							List<Point> points;
							if (!this._hangoutPoints.TryGetValue(hangout_group, out points))
							{
								points = (this._hangoutPoints[hangout_group] = new List<Point>());
							}
							points.Add(new Point(x, y));
							this._maxHangoutGroups = Math.Max(this._maxHangoutGroups, hangout_group);
						}
					}
				}
			}
			this.ResetTheater();
		}

		// Token: 0x060031D8 RID: 12760 RVA: 0x0027C4E0 File Offset: 0x0027A6E0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this._spawnedMoviePatrons, "_spawnedMoviePatrons").AddField(this._purchasedConcessions, "_purchasedConcessions").AddField(this.currentState, "currentState").AddField(this.showingId, "showingId").AddField(this.movieViewerLockEvent, "movieViewerLockEvent").AddField(this.requestStartMovieEvent, "requestStartMovieEvent").AddField(this.startMovieEvent, "startMovieEvent").AddField(this.endMovieEvent, "endMovieEvent").AddField(this._playerInvitedPatrons, "_playerInvitedPatrons").AddField(this._characterGroupLookup, "_characterGroupLookup").AddField(this.dayFirstEntered, "dayFirstEntered");
			this.movieViewerLockEvent.onEvent += this.OnMovieViewerLockEvent;
			this.requestStartMovieEvent.onEvent += this.OnRequestStartMovieEvent;
			this.startMovieEvent.onEvent += this.OnStartMovieEvent;
		}

		// Token: 0x060031D9 RID: 12761 RVA: 0x0027C5F0 File Offset: 0x0027A7F0
		public void OnStartMovieEvent(StartMovieEvent e)
		{
			if (e.uid != Game1.player.UniqueMultiplayerID)
			{
				return;
			}
			ReadyCheckDialog readyCheckDialog = Game1.activeClickableMenu as ReadyCheckDialog;
			if (readyCheckDialog != null)
			{
				readyCheckDialog.closeDialog(Game1.player);
			}
			MovieTheaterScreeningEvent event_generator = new MovieTheaterScreeningEvent();
			Event viewing_event = event_generator.getMovieEvent(MovieTheater.GetMovieToday().Id, e.playerGroups, e.npcGroups, this.GetConcessionsDictionary());
			Rumble.rumble(0.15f, 200f);
			Game1.player.completelyStopAnimatingOrDoingAction();
			base.playSound("doorClose", new Vector2?(Game1.player.Tile), null, SoundContext.Default);
			Game1.globalFadeToBlack(delegate
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
				this.startEvent(viewing_event);
			}, 0.02f);
		}

		// Token: 0x060031DA RID: 12762 RVA: 0x0027C6B8 File Offset: 0x0027A8B8
		public void OnRequestStartMovieEvent(long uid)
		{
			if (Game1.IsMasterGame)
			{
				if (this.CurrentState == 0)
				{
					if (Game1.player.team.movieMutex.IsLocked())
					{
						Game1.player.team.movieMutex.ReleaseLock();
					}
					Game1.player.team.movieMutex.RequestLock(null, null);
					this._playerGroups = new List<List<Character>>();
					this._npcGroups = new List<List<Character>>();
					List<Character> patrons = new List<Character>();
					foreach (string name in MovieTheater.GetPatronNames())
					{
						Character character = Game1.getCharacterFromName(name, true, false);
						patrons.Add(character);
					}
					foreach (Farmer farmer in this._viewingFarmers)
					{
						List<Character> farmer_group = new List<Character>();
						farmer_group.Add(farmer);
						for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
						{
							MovieInvitation invite = Game1.player.team.movieInvitations[i];
							if (invite.farmer == farmer && MovieTheater.GetFirstInvitedPlayer(invite.invitedNPC) == farmer && patrons.Contains(invite.invitedNPC))
							{
								patrons.Remove(invite.invitedNPC);
								farmer_group.Add(invite.invitedNPC);
							}
						}
						this._playerGroups.Add(farmer_group);
					}
					foreach (List<Character> list in this._playerGroups)
					{
						foreach (Character character3 in list)
						{
							NPC npc = character3 as NPC;
							if (npc != null)
							{
								npc.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
							}
						}
					}
					this._npcGroups.Add(new List<Character>(patrons));
					this._PopulateNPCOnlyGroups(this._playerGroups, this._npcGroups);
					this._viewingGroups = new List<List<Character>>();
					List<Character> player_invited_npcs = new List<Character>();
					foreach (List<Character> list2 in this._playerGroups)
					{
						foreach (Character character2 in list2)
						{
							player_invited_npcs.Add(character2);
						}
					}
					this._viewingGroups.Add(player_invited_npcs);
					foreach (List<Character> characters in this._npcGroups)
					{
						this._viewingGroups.Add(new List<Character>(characters));
					}
					this.CurrentState = 1;
				}
				this.startMovieEvent.Fire(new StartMovieEvent(uid, this._playerGroups, this._npcGroups));
			}
		}

		// Token: 0x060031DB RID: 12763 RVA: 0x0027CA1C File Offset: 0x0027AC1C
		public void OnMovieViewerLockEvent(MovieViewerLockEvent e)
		{
			this._viewingFarmers = new List<Farmer>();
			this._movieStartTime = e.movieStartTime;
			foreach (long id in e.uids)
			{
				Farmer farmer = Game1.GetPlayer(id, true);
				if (farmer != null)
				{
					this._viewingFarmers.Add(farmer);
				}
			}
			if (this._viewingFarmers.Count > 0 && Game1.IsMultiplayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:MovieStartRequest"));
			}
			if (Game1.player.team.movieMutex.IsLockHeld())
			{
				this._ShowMovieStartReady();
			}
		}

		// Token: 0x060031DC RID: 12764 RVA: 0x0027CADC File Offset: 0x0027ACDC
		public void _ShowMovieStartReady()
		{
			if (!Game1.IsMultiplayer)
			{
				this.requestStartMovieEvent.Fire(Game1.player.UniqueMultiplayerID);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
			defaultInterpolatedStringHandler.AppendLiteral("start_movie_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.ShowingId);
			string readyCheckName = defaultInterpolatedStringHandler.ToStringAndClear();
			Game1.netReady.SetLocalRequiredFarmers(readyCheckName, this._viewingFarmers);
			Game1.netReady.SetLocalReady(readyCheckName, true);
			Game1.dialogueUp = false;
			MovieTheater._hasRequestedMovieStart = true;
			Game1.activeClickableMenu = new ReadyCheckDialog(readyCheckName, true, delegate(Farmer farmer)
			{
				if (MovieTheater._hasRequestedMovieStart)
				{
					MovieTheater._hasRequestedMovieStart = false;
					this.requestStartMovieEvent.Fire(farmer.UniqueMultiplayerID);
				}
			}, delegate(Farmer farmer)
			{
				if (Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(farmer);
				}
				if (Game1.player.team.movieMutex.IsLockHeld())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
			});
		}

		// Token: 0x060031DD RID: 12765 RVA: 0x0027CB94 File Offset: 0x0027AD94
		public static List<MovieData> GetMovieData()
		{
			if (MovieTheater._movieData == null)
			{
				MovieTheater._movieData = new List<MovieData>();
				MovieTheater._movieDataById = new Dictionary<string, MovieData>();
				foreach (MovieData movie in DataLoader.Movies(Game1.content))
				{
					if (string.IsNullOrWhiteSpace(movie.Id))
					{
						Game1.log.Warn("Ignored movie with no ID.");
					}
					else if (!MovieTheater._movieDataById.TryAdd(movie.Id, movie))
					{
						Game1.log.Warn("Ignored duplicate movie with ID '" + movie.Id + "'.");
					}
					else
					{
						MovieTheater._movieData.Add(movie);
					}
				}
			}
			return MovieTheater._movieData;
		}

		// Token: 0x060031DE RID: 12766 RVA: 0x0027CC64 File Offset: 0x0027AE64
		public static Dictionary<string, MovieData> GetMovieDataById()
		{
			if (MovieTheater._movieDataById == null)
			{
				MovieTheater.GetMovieData();
			}
			return MovieTheater._movieDataById;
		}

		// Token: 0x060031DF RID: 12767 RVA: 0x0027CC78 File Offset: 0x0027AE78
		public static bool TryGetMovieData(string id, out MovieData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return MovieTheater.GetMovieDataById().TryGetValue(id, out data);
		}

		// Token: 0x060031E0 RID: 12768 RVA: 0x0027CC90 File Offset: 0x0027AE90
		public static string GetMovieIdFromLegacyIndex(string id)
		{
			int index;
			if (int.TryParse(id, out index))
			{
				foreach (MovieData movie in MovieTheater.GetMovieData())
				{
					if (movie.SheetIndex == index && (string.IsNullOrWhiteSpace(movie.Texture) || movie.Texture == "LooseSprites\\Movies"))
					{
						return movie.Id;
					}
				}
				return id;
			}
			return id;
		}

		// Token: 0x060031E1 RID: 12769 RVA: 0x0027CD1C File Offset: 0x0027AF1C
		public static Microsoft.Xna.Framework.Rectangle GetSourceRectForScreen(int movieIndex, int frame)
		{
			int yOffset = movieIndex * 128 + frame / 5 * 64;
			int xOffset = frame % 5 * 96;
			return new Microsoft.Xna.Framework.Rectangle(16 + xOffset, yOffset, 90, 61);
		}

		// Token: 0x060031E2 RID: 12770 RVA: 0x0027CD4D File Offset: 0x0027AF4D
		public static Microsoft.Xna.Framework.Rectangle GetSourceRectForPoster(int movieIndex)
		{
			return new Microsoft.Xna.Framework.Rectangle(0, movieIndex * 128, 13, 19);
		}

		// Token: 0x060031E3 RID: 12771 RVA: 0x0027CD60 File Offset: 0x0027AF60
		public NPC GetMoviePatron(string name)
		{
			for (int i = 0; i < this.characters.Count; i++)
			{
				if (this.characters[i].name.Value == name)
				{
					return this.characters[i];
				}
			}
			return null;
		}

		// Token: 0x060031E4 RID: 12772 RVA: 0x0027CDB0 File Offset: 0x0027AFB0
		protected NPC AddMoviePatronNPC(string name, int x, int y, int facingDirection)
		{
			if (this._spawnedMoviePatrons.ContainsKey(name))
			{
				return this.GetMoviePatron(name);
			}
			string textureName = NPC.getTextureNameForCharacter(name);
			CharacterData data;
			NPC.TryGetData(name, out data);
			int width = (data != null) ? data.Size.X : 16;
			int height = (data != null) ? data.Size.Y : 32;
			NPC i = new NPC(new AnimatedSprite("Characters\\" + textureName, 0, width, height), new Vector2((float)(x * 64), (float)(y * 64)), base.Name, facingDirection, name, null, true);
			i.EventActor = true;
			i.collidesWithOtherCharacters.Set(false);
			base.addCharacter(i);
			this._spawnedMoviePatrons.Add(name, 1);
			this.GetDialogueForCharacter(i);
			return i;
		}

		// Token: 0x060031E5 RID: 12773 RVA: 0x0027CE72 File Offset: 0x0027B072
		public void RemoveAllPatrons()
		{
			if (this._spawnedMoviePatrons != null)
			{
				this.characters.RemoveWhere((NPC npc) => this._spawnedMoviePatrons.ContainsKey(npc.Name));
				this._spawnedMoviePatrons.Clear();
			}
		}

		// Token: 0x060031E6 RID: 12774 RVA: 0x0027CEA0 File Offset: 0x0027B0A0
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (this.CurrentState == 0)
			{
				MovieData movie = MovieTheater.GetMovieToday();
				Game1.multiplayer.globalChatInfoMessage("MovieStart", new string[]
				{
					TokenStringBuilder.MovieName(movie.Id)
				});
			}
		}

		// Token: 0x060031E7 RID: 12775 RVA: 0x0027CEE4 File Offset: 0x0027B0E4
		protected override void resetLocalState()
		{
			base.resetLocalState();
			Game1.getAchievement(36, true);
			this.birds = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, new Vector2(8f, 14f), new Point[]
			{
				new Point(19, 5),
				new Point(21, 4),
				new Point(16, 3),
				new Point(10, 13),
				new Point(2, 13),
				new Point(2, 6),
				new Point(9, 2),
				new Point(18, 12),
				new Point(21, 11),
				new Point(3, 11),
				new Point(4, 2),
				new Point(12, 12),
				new Point(11, 5),
				new Point(13, 13)
			}, new Point[]
			{
				new Point(19, 5),
				new Point(21, 4),
				new Point(16, 3),
				new Point(9, 2),
				new Point(21, 11),
				new Point(4, 2)
			});
			if (!MovieTheater._isJojaTheater && Game1.MasterPlayer.mailReceived.Contains("ccMovieTheaterJoja"))
			{
				MovieTheater._isJojaTheater = true;
			}
			if (this.dayFirstEntered.Value == -1)
			{
				this.dayFirstEntered.Value = Game1.Date.TotalDays;
			}
			if (!MovieTheater._isJojaTheater)
			{
				this.birds.roosting = (this.CurrentState == 2);
				for (int i = 0; i < Game1.random.Next(2, 5); i++)
				{
					int bird_type = Game1.random.Next(0, 4);
					if (base.IsFallHere())
					{
						bird_type = 10;
					}
					this.birds.AddBird(bird_type);
				}
				if (Game1.timeOfDay > 2100 && Game1.random.NextBool())
				{
					this.birds.AddBird(11);
				}
			}
			MovieTheater.AddMoviePoster(this, 1104f, 292f, false);
			base.loadMap(this.mapPath.Value, true);
			if (MovieTheater._isJojaTheater)
			{
				string addOn = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? "" : "_international";
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
				{
					addOn = ".ru-RU";
				}
				base.Map.RequireTileSheet(0, "movieTheater_tileSheet").ImageSource = "Maps\\MovieTheaterJoja_TileSheet" + addOn;
				base.Map.LoadTileSheets(Game1.mapDisplayDevice);
			}
			int num = this.CurrentState;
			if (num == 0)
			{
				this.addRandomNPCs();
				return;
			}
			if (num != 2)
			{
				return;
			}
			Game1.changeMusicTrack("movieTheaterAfter", false, MusicContext.Default);
			Game1.ambientLight = new Color(150, 170, 80);
			this.addSpecificRandomNPC(0);
		}

		// Token: 0x060031E8 RID: 12776 RVA: 0x0027D1E4 File Offset: 0x0027B3E4
		private void addRandomNPCs()
		{
			Season season = base.GetSeason();
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, 0.0, 0.0, 0.0);
			this.critters = new List<Critter>();
			if (this.dayFirstEntered.Value == Game1.Date.TotalDays || r.NextDouble() < 0.25)
			{
				this.addSpecificRandomNPC(0);
			}
			if (!MovieTheater._isJojaTheater && r.NextDouble() < 0.28)
			{
				this.addSpecificRandomNPC(4);
				this.addSpecificRandomNPC(11);
			}
			else if (MovieTheater._isJojaTheater && r.NextDouble() < 0.33)
			{
				this.addSpecificRandomNPC(13);
			}
			if (r.NextDouble() < 0.1)
			{
				this.addSpecificRandomNPC(9);
				this.addSpecificRandomNPC(7);
			}
			if (season != Season.Spring)
			{
				if (season == Season.Fall && r.NextBool())
				{
					this.addSpecificRandomNPC(1);
				}
			}
			else if (r.NextBool())
			{
				this.addSpecificRandomNPC(3);
			}
			if (r.NextDouble() < 0.25)
			{
				this.addSpecificRandomNPC(2);
			}
			if (r.NextDouble() < 0.25)
			{
				this.addSpecificRandomNPC(6);
			}
			if (r.NextDouble() < 0.25)
			{
				this.addSpecificRandomNPC(8);
			}
			if (r.NextDouble() < 0.2)
			{
				this.addSpecificRandomNPC(10);
			}
			if (r.NextDouble() < 0.2)
			{
				this.addSpecificRandomNPC(12);
			}
			if (r.NextDouble() < 0.2)
			{
				this.addSpecificRandomNPC(5);
			}
			if (!MovieTheater._isJojaTheater)
			{
				if (r.NextDouble() < 0.75)
				{
					base.addCritter(new Butterfly(this, new Vector2(13f, 7f), false, false, -1, false).setStayInbounds(true));
				}
				if (r.NextDouble() < 0.75)
				{
					base.addCritter(new Butterfly(this, new Vector2(4f, 8f), false, false, -1, false).setStayInbounds(true));
				}
				if (r.NextDouble() < 0.75)
				{
					base.addCritter(new Butterfly(this, new Vector2(17f, 10f), false, false, -1, false).setStayInbounds(true));
				}
			}
		}

		// Token: 0x060031E9 RID: 12777 RVA: 0x0027D434 File Offset: 0x0027B634
		private void addSpecificRandomNPC(int whichRandomNPC)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, (double)whichRandomNPC, 0.0, 0.0);
			switch (whichRandomNPC)
			{
			case 0:
				base.setMapTile(2, 9, 215, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_CraneMan" + r.Choose("2", ""), true);
				base.setMapTile(2, 8, 199, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 1:
				base.setMapTile(19, 7, 216, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Welwick" + r.Choose("2", ""), true);
				base.setMapTile(19, 6, 200, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 2:
				base.setAnimatedMapTile(21, 7, new int[]
				{
					217,
					217,
					217,
					218
				}, 700L, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_ShortsMan" + r.Choose("2", ""), true);
				base.setAnimatedMapTile(21, 6, new int[]
				{
					201,
					201,
					201,
					202
				}, 700L, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 3:
				base.setMapTile(5, 9, 219, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Mother" + r.Choose("2", ""), true);
				base.setMapTile(6, 9, 220, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Child" + r.Choose("2", ""), true);
				base.setAnimatedMapTile(5, 8, new int[]
				{
					203,
					203,
					203,
					204,
					204,
					204
				}, 1000L, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 4:
				base.setMapTile(20, 9, 222, "Front", "movieTheater_tileSheet", null, true);
				base.setMapTile(21, 9, 223, "Front", "movieTheater_tileSheet", null, true);
				base.setMapTile(20, 10, 238, "Buildings", "movieTheater_tileSheet", null, true);
				base.setMapTile(21, 10, 239, "Buildings", "movieTheater_tileSheet", null, true);
				base.setMapTile(20, 11, 254, "Buildings", "movieTheater_tileSheet", null, true);
				base.setMapTile(21, 11, 255, "Buildings", "movieTheater_tileSheet", null, true);
				return;
			case 5:
				base.setAnimatedMapTile(10, 7, new int[]
				{
					251,
					251,
					251,
					252
				}, 900L, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Lupini" + r.Choose("2", ""), true);
				base.setAnimatedMapTile(10, 6, new int[]
				{
					235,
					235,
					235,
					236
				}, 900L, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 6:
				base.setAnimatedMapTile(5, 7, new int[]
				{
					249,
					249,
					249,
					250
				}, 600L, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_ConcessionMan" + r.Choose("2", ""), true);
				base.setAnimatedMapTile(5, 6, new int[]
				{
					233,
					233,
					233,
					234
				}, 600L, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 7:
				base.setMapTile(1, 12, 248, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_PurpleHairLady", true);
				base.setMapTile(1, 11, 232, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 8:
				base.setMapTile(3, 8, 247, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_RedCapGuy" + r.Choose("2", ""), true);
				base.setMapTile(3, 7, 231, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 9:
				base.setMapTile(2, 11, 253, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Governor" + r.Choose("2", ""), true);
				base.setMapTile(2, 10, 237, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 10:
				base.setMapTile(9, 7, 221, "Buildings", "movieTheater_tileSheet", "NPCSpeechMessageNoRadius Gunther MovieTheater_Gunther" + r.Choose("2", ""), true);
				base.setMapTile(9, 6, 205, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 11:
				base.setMapTile(19, 10, 208, "Buildings", "movieTheater_tileSheet", "NPCSpeechMessageNoRadius Marlon MovieTheater_Marlon" + r.Choose("2", ""), true);
				base.setMapTile(19, 9, 192, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 12:
				base.setMapTile(12, 4, 209, "Buildings", "movieTheater_tileSheet", "MessageSpeech MovieTheater_Marcello" + r.Choose("2", ""), true);
				base.setMapTile(12, 3, 193, "Front", "movieTheater_tileSheet", null, true);
				return;
			case 13:
				base.setMapTile(17, 12, 241, "Buildings", "movieTheater_tileSheet", "NPCSpeechMessageNoRadius Morris MovieTheater_Morris" + r.Choose("2", ""), true);
				base.setMapTile(17, 11, 225, "Front", "movieTheater_tileSheet", null, true);
				return;
			default:
				return;
			}
		}

		// Token: 0x060031EA RID: 12778 RVA: 0x0027DA00 File Offset: 0x0027BC00
		public static MovieData GetMovieToday()
		{
			if (MovieTheater.forceMovieId != null)
			{
				MovieData data;
				if (MovieTheater.TryGetMovieData(MovieTheater.forceMovieId, out data))
				{
					return data;
				}
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Ignored invalid ");
				defaultInterpolatedStringHandler.AppendFormatted("MovieTheater");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("forceMovieId");
				defaultInterpolatedStringHandler.AppendLiteral(" override '");
				defaultInterpolatedStringHandler.AppendFormatted(MovieTheater.forceMovieId);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				MovieTheater.forceMovieId = null;
			}
			return MovieTheater.GetMovieForDate(Game1.Date);
		}

		// Token: 0x060031EB RID: 12779 RVA: 0x0027DAA8 File Offset: 0x0027BCA8
		public static List<MovieData> GetMoviesForSeason(WorldDate date)
		{
			WorldDate dayTheaterBuilt = WorldDate.ForDaysPlayed((int)Game1.player.team.theaterBuildDate.Value);
			int relativeBuildYear = date.Year - dayTheaterBuilt.Year;
			List<MovieData> allMovies = MovieTheater.GetMovieData();
			List<MovieData> movies = new List<MovieData>();
			foreach (MovieData movie in allMovies)
			{
				if (MovieTheater.MovieSeasonMatches(movie, date.Season) && MovieTheater.MovieYearMatches(movie, relativeBuildYear))
				{
					movies.Add(movie);
				}
			}
			if (movies.Count == 0)
			{
				foreach (MovieData movie2 in allMovies)
				{
					if (MovieTheater.MovieSeasonMatches(movie2, date.Season))
					{
						movies.Add(movie2);
					}
				}
			}
			if (movies.Count == 0)
			{
				movies.AddRange(allMovies);
			}
			if (movies.Count > 28)
			{
				Utility.Shuffle<MovieData>(Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.season, (double)Game1.year, 0.0, 0.0), movies);
				movies.RemoveRange(28, movies.Count - 28);
			}
			return movies;
		}

		// Token: 0x060031EC RID: 12780 RVA: 0x0027DBFC File Offset: 0x0027BDFC
		public static MovieData GetMovieForDate(WorldDate date)
		{
			List<MovieData> movies = MovieTheater.GetMoviesForSeason(date);
			if (movies.Count == 0)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
				defaultInterpolatedStringHandler.AppendLiteral("There are no available movies for ");
				defaultInterpolatedStringHandler.AppendFormatted<WorldDate>(date);
				defaultInterpolatedStringHandler.AppendLiteral(". Defaulting to all movies.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				movies = MovieTheater.GetMovieData();
			}
			float daysPerMovie = 28f / (float)movies.Count;
			int index = ((int)Math.Ceiling((double)((float)date.DayOfMonth / daysPerMovie)) - 1) % movies.Count;
			return movies[index];
		}

		// Token: 0x060031ED RID: 12781 RVA: 0x0027DC88 File Offset: 0x0027BE88
		public static MovieData GetUpcomingMovie()
		{
			return MovieTheater.GetUpcomingMovieForDate(Game1.Date);
		}

		// Token: 0x060031EE RID: 12782 RVA: 0x0027DC94 File Offset: 0x0027BE94
		public static MovieData GetUpcomingMovieForDate(WorldDate afterDate)
		{
			List<MovieData> movies = MovieTheater.GetMoviesForSeason(afterDate);
			MovieData currentMovie = MovieTheater.GetMovieForDate(afterDate);
			bool foundMovie = false;
			foreach (MovieData movie in movies)
			{
				if (movie.Id == currentMovie.Id)
				{
					foundMovie = true;
				}
				else if (foundMovie)
				{
					return movie;
				}
			}
			movies = MovieTheater.GetMoviesForSeason(WorldDate.ForDaysPlayed(afterDate.TotalDays + 28));
			foreach (MovieData movie2 in movies)
			{
				if (movie2.Id != currentMovie.Id)
				{
					return movie2;
				}
			}
			return movies[0];
		}

		// Token: 0x060031EF RID: 12783 RVA: 0x0027DD7C File Offset: 0x0027BF7C
		public static bool MovieYearMatches(MovieData movie, int year)
		{
			int? yearModulus = movie.YearModulus;
			if (yearModulus == null)
			{
				return true;
			}
			int modulus = movie.YearModulus.Value;
			int remainder = movie.YearRemainder.GetValueOrDefault();
			if (modulus < 1)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Movie '");
				defaultInterpolatedStringHandler.AppendFormatted(movie.Id);
				defaultInterpolatedStringHandler.AppendLiteral("' has invalid year modulus ");
				defaultInterpolatedStringHandler.AppendFormatted<int?>(movie.YearModulus);
				defaultInterpolatedStringHandler.AppendLiteral(", must be a number greater than zero.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			return year % modulus == remainder;
		}

		// Token: 0x060031F0 RID: 12784 RVA: 0x0027DE18 File Offset: 0x0027C018
		public static bool MovieSeasonMatches(MovieData movie, Season season)
		{
			List<Season> seasons = movie.Seasons;
			return seasons == null || seasons.Count <= 0 || movie.Seasons.Contains(season);
		}

		// Token: 0x060031F1 RID: 12785 RVA: 0x0027DE3F File Offset: 0x0027C03F
		public override void DayUpdate(int dayOfMonth)
		{
			this.ShowingId = 0;
			this.ResetTheater();
			this._ResetHangoutPoints();
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x060031F2 RID: 12786 RVA: 0x0027DE5C File Offset: 0x0027C05C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this._farmerCount != this.farmers.Count)
			{
				this._farmerCount = this.farmers.Count;
				ReadyCheckDialog readyCheckDialog = Game1.activeClickableMenu as ReadyCheckDialog;
				if (readyCheckDialog != null)
				{
					readyCheckDialog.closeDialog(Game1.player);
					if (Game1.player.team.movieMutex.IsLockHeld())
					{
						Game1.player.team.movieMutex.ReleaseLock();
					}
				}
			}
			PerchingBirds perchingBirds = this.birds;
			if (perchingBirds != null)
			{
				perchingBirds.Update(time);
			}
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x060031F3 RID: 12787 RVA: 0x0027DEE9 File Offset: 0x0027C0E9
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			PerchingBirds perchingBirds = this.birds;
			if (perchingBirds != null)
			{
				perchingBirds.Draw(b);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		// Token: 0x060031F4 RID: 12788 RVA: 0x0027DF04 File Offset: 0x0027C104
		public static bool Invite(Farmer farmer, NPC invited_npc)
		{
			if (farmer == null || invited_npc == null)
			{
				return false;
			}
			MovieInvitation invitation = new MovieInvitation();
			invitation.farmer = farmer;
			invitation.invitedNPC = invited_npc;
			farmer.team.movieInvitations.Add(invitation);
			return true;
		}

		// Token: 0x060031F5 RID: 12789 RVA: 0x0027DF40 File Offset: 0x0027C140
		public void ResetTheater()
		{
			MovieTheater._playerHangoutGroup = -1;
			this.RemoveAllPatrons();
			this._playerGroups.Clear();
			this._npcGroups.Clear();
			this._viewingGroups.Clear();
			this._viewingFarmers.Clear();
			this._purchasedConcessions.Clear();
			this._playerInvitedPatrons.Clear();
			this._characterGroupLookup.Clear();
			this._ResetHangoutPoints();
			Game1.player.team.movieMutex.ReleaseLock();
			this.CurrentState = 0;
		}

		// Token: 0x060031F6 RID: 12790 RVA: 0x0027DFC8 File Offset: 0x0027C1C8
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.movieViewerLockEvent.Poll();
			this.requestStartMovieEvent.Poll();
			this.startMovieEvent.Poll();
			this.endMovieEvent.Poll();
			if (!Game1.IsMasterGame)
			{
				return;
			}
			for (int i = 0; i < this._viewingFarmers.Count; i++)
			{
				Farmer viewing_farmer = this._viewingFarmers[i];
				if (!Game1.getOnlineFarmers().Contains(viewing_farmer))
				{
					this._viewingFarmers.RemoveAt(i);
					i--;
				}
				else if (this.CurrentState == 2 && !this.farmers.Contains(viewing_farmer) && !this.HasFarmerWatchingBroadcastEventReturningHere() && viewing_farmer.currentLocation != null && !viewing_farmer.currentLocation.IsTemporary)
				{
					this._viewingFarmers.RemoveAt(i);
					i--;
				}
			}
			if (this.CurrentState != 0 && this._viewingFarmers.Count == 0)
			{
				MovieData movie = MovieTheater.GetMovieToday();
				Game1.multiplayer.globalChatInfoMessage("MovieEnd", new string[]
				{
					TokenStringBuilder.MovieName(movie.Id)
				});
				this.ResetTheater();
				int num = this.ShowingId;
				this.ShowingId = num + 1;
			}
			if (Game1.player.team.movieInvitations != null && this._playerInvitedPatrons.Count() < 8)
			{
				foreach (Farmer farmer in this.farmers)
				{
					for (int j = 0; j < Game1.player.team.movieInvitations.Count; j++)
					{
						MovieInvitation invite = Game1.player.team.movieInvitations[j];
						if (!invite.fulfilled && !this._spawnedMoviePatrons.ContainsKey(invite.invitedNPC.displayName))
						{
							if (MovieTheater._playerHangoutGroup < 0)
							{
								MovieTheater._playerHangoutGroup = Game1.random.Next(this._maxHangoutGroups);
							}
							int group = MovieTheater._playerHangoutGroup;
							if (invite.farmer == farmer && MovieTheater.GetFirstInvitedPlayer(invite.invitedNPC) == farmer)
							{
								while (this._availableHangoutPoints[group].Count == 0)
								{
									group = Game1.random.Next(this._maxHangoutGroups);
								}
								Point point = Game1.random.ChooseFrom(this._availableHangoutPoints[group]);
								NPC character = this.AddMoviePatronNPC(invite.invitedNPC.name.Value, 14, 15, 0);
								this._playerInvitedPatrons.Add(character.name.Value, 1);
								this._availableHangoutPoints[group].Remove(point);
								int direction = 2;
								IPropertyCollection tileProperties = this.map.GetLayer("Paths").Tiles[point.X, point.Y].Properties;
								string rawDirection;
								if (tileProperties != null && tileProperties.TryGetValue("direction", out rawDirection))
								{
									int.TryParse(rawDirection, out direction);
								}
								this._destinationPositions[character.Name] = new KeyValuePair<Point, int>(point, direction);
								this.PathCharacterToLocation(character, point, direction);
								invite.fulfilled = true;
							}
						}
					}
				}
			}
		}

		// Token: 0x060031F7 RID: 12791 RVA: 0x0027E320 File Offset: 0x0027C520
		public static MovieCharacterReaction GetReactionsForCharacter(NPC character)
		{
			if (character == null)
			{
				return null;
			}
			foreach (MovieCharacterReaction reactions in MovieTheater.GetMovieReactions())
			{
				if (!(reactions.NPCName != character.Name))
				{
					return reactions;
				}
			}
			return null;
		}

		// Token: 0x060031F8 RID: 12792 RVA: 0x0027E38C File Offset: 0x0027C58C
		public override void checkForMusic(GameTime time)
		{
		}

		// Token: 0x060031F9 RID: 12793 RVA: 0x0027E390 File Offset: 0x0027C590
		public static string GetResponseForMovie(NPC character)
		{
			string response = "like";
			MovieData movie = MovieTheater.GetMovieToday();
			if (movie == null)
			{
				return null;
			}
			if (movie != null)
			{
				foreach (MovieCharacterReaction reactions in MovieTheater.GetMovieReactions())
				{
					if (!(reactions.NPCName != character.Name))
					{
						foreach (MovieReaction tagged_reactions in reactions.Reactions)
						{
							if (tagged_reactions.ShouldApplyToMovie(movie, MovieTheater.GetPatronNames(), Array.Empty<string>()))
							{
								string response2 = tagged_reactions.Response;
								if (response2 != null && response2.Length > 0)
								{
									response = tagged_reactions.Response;
									break;
								}
							}
						}
					}
				}
			}
			return response;
		}

		// Token: 0x060031FA RID: 12794 RVA: 0x0027E480 File Offset: 0x0027C680
		public Dialogue GetDialogueForCharacter(NPC character)
		{
			MovieData movie = MovieTheater.GetMovieToday();
			if (movie != null)
			{
				foreach (MovieCharacterReaction reactions in MovieTheater._genericReactions)
				{
					if (!(reactions.NPCName != character.Name))
					{
						using (List<MovieReaction>.Enumerator enumerator2 = reactions.Reactions.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								MovieReaction tagged_reactions = enumerator2.Current;
								if (tagged_reactions.ShouldApplyToMovie(movie, MovieTheater.GetPatronNames(), new string[]
								{
									MovieTheater.GetResponseForMovie(character)
								}))
								{
									string response = tagged_reactions.Response;
									if (response != null && response.Length > 0 && tagged_reactions.SpecialResponses != null)
									{
										switch (this.CurrentState)
										{
										case 0:
											if (tagged_reactions.SpecialResponses.BeforeMovie != null)
											{
												return new Dialogue(character, null, this.FormatString(tagged_reactions.SpecialResponses.BeforeMovie.Text, Array.Empty<string>()));
											}
											goto IL_16D;
										case 1:
											if (tagged_reactions.SpecialResponses.DuringMovie != null)
											{
												return new Dialogue(character, null, this.FormatString(tagged_reactions.SpecialResponses.DuringMovie.Text, Array.Empty<string>()));
											}
											goto IL_16D;
										case 2:
											if (tagged_reactions.SpecialResponses.AfterMovie != null)
											{
												return new Dialogue(character, null, this.FormatString(tagged_reactions.SpecialResponses.AfterMovie.Text, Array.Empty<string>()));
											}
											goto IL_16D;
										default:
											goto IL_199;
										}
									}
								}
							}
							IL_16D:
							break;
						}
					}
				}
			}
			IL_199:
			return null;
		}

		// Token: 0x060031FB RID: 12795 RVA: 0x0027E660 File Offset: 0x0027C860
		public string FormatString(string text, params string[] args)
		{
			text = TokenParser.ParseText(text, null, null, null);
			string title = TokenParser.ParseText(MovieTheater.GetMovieToday().Title, null, null, null);
			return string.Format(text, title, Game1.player.displayName, args);
		}

		// Token: 0x060031FC RID: 12796 RVA: 0x0027E6A0 File Offset: 0x0027C8A0
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			string[] action = base.GetTilePropertySplitBySpaces("Action", "Buildings", tileLocation.X, tileLocation.Y);
			if (action.Length != 0)
			{
				return this.performAction(action, who, tileLocation);
			}
			foreach (NPC npc in this.characters)
			{
				if (npc != null && !npc.IsMonster && (!who.isRidingHorse() || !(npc is Horse)) && npc.GetBoundingBox().Intersects(tileRect))
				{
					if (!npc.isMoving())
					{
						bool is_in_group;
						if (this._playerInvitedPatrons.ContainsKey(npc.Name))
						{
							npc.faceTowardFarmerForPeriod(5000, 4, false, who);
							Dialogue dialogue = this.GetDialogueForCharacter(npc);
							if (dialogue != null)
							{
								npc.CurrentDialogue.Push(dialogue);
								Game1.drawDialogue(npc);
								npc.grantConversationFriendship(Game1.player, 20);
							}
						}
						else if (this._characterGroupLookup.TryGetValue(npc.Name, out is_in_group))
						{
							if (!is_in_group)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovieAlone", npc.displayName));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovie", npc.displayName));
							}
						}
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060031FD RID: 12797 RVA: 0x0027E83C File Offset: 0x0027CA3C
		protected void _PopulateNPCOnlyGroups(List<List<Character>> player_groups, List<List<Character>> groups)
		{
			HashSet<string> used_characters = new HashSet<string>();
			foreach (List<Character> list in player_groups)
			{
				foreach (Character character in list)
				{
					if (character is NPC)
					{
						used_characters.Add(character.name.Value);
					}
				}
			}
			foreach (List<Character> list2 in groups)
			{
				foreach (Character character2 in list2)
				{
					if (character2 is NPC)
					{
						used_characters.Add(character2.name.Value);
					}
				}
			}
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, 0.0, 0.0, 0.0);
			int group_count = 0;
			for (int i = 0; i < 2; i++)
			{
				if (r.NextDouble() < 0.75)
				{
					group_count++;
				}
			}
			int time_of_day = 0;
			if (this._movieStartTime >= 1200)
			{
				time_of_day = 1;
			}
			if (this._movieStartTime >= 1800)
			{
				time_of_day = 2;
			}
			string[][] possible_npcs_for_this_day = MovieTheater.possibleNPCGroups[(int)Game1.Date.DayOfWeek][time_of_day];
			if (possible_npcs_for_this_day == null)
			{
				return;
			}
			if (groups.Count > 0 && groups[0].Count == 0)
			{
				groups.RemoveAt(0);
			}
			int j = 0;
			while (j < group_count && groups.Count < 2)
			{
				string[] characters = r.Choose(possible_npcs_for_this_day);
				bool valid = true;
				foreach (string character3 in characters)
				{
					bool found_friendship = false;
					using (IEnumerator<Farmer> enumerator3 = Game1.getAllFarmers().GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							if (enumerator3.Current.friendshipData.ContainsKey(character3))
							{
								found_friendship = true;
								break;
							}
						}
					}
					if (!found_friendship)
					{
						valid = false;
						break;
					}
					if (used_characters.Contains(character3))
					{
						valid = false;
						break;
					}
					if (MovieTheater.GetResponseForMovie(Game1.getCharacterFromName(character3, true, false)) == "dislike" || MovieTheater.GetResponseForMovie(Game1.getCharacterFromName(character3, true, false)) == "reject")
					{
						valid = false;
						break;
					}
				}
				if (valid)
				{
					List<Character> new_group = new List<Character>();
					foreach (string character4 in characters)
					{
						NPC patron = this.AddMoviePatronNPC(character4, 1000, 1000, 2);
						new_group.Add(patron);
						used_characters.Add(character4);
						this._characterGroupLookup[character4] = (characters.Length > 1);
					}
					groups.Add(new_group);
				}
				j++;
			}
		}

		// Token: 0x060031FE RID: 12798 RVA: 0x0027EB74 File Offset: 0x0027CD74
		public Dictionary<Character, MovieConcession> GetConcessionsDictionary()
		{
			Dictionary<Character, MovieConcession> dictionary = new Dictionary<Character, MovieConcession>();
			foreach (string npc_name in this._purchasedConcessions.Keys)
			{
				Character character = Game1.getCharacterFromName(npc_name, true, false);
				MovieConcession purchasedConcession;
				if (character != null && MovieTheater.GetConcessions().TryGetValue(this._purchasedConcessions[npc_name], out purchasedConcession))
				{
					dictionary[character] = purchasedConcession;
				}
			}
			return dictionary;
		}

		// Token: 0x060031FF RID: 12799 RVA: 0x0027EC04 File Offset: 0x0027CE04
		protected void _ResetHangoutPoints()
		{
			this._destinationPositions.Clear();
			this._availableHangoutPoints = new Dictionary<int, List<Point>>();
			foreach (int key in this._hangoutPoints.Keys)
			{
				this._availableHangoutPoints[key] = new List<Point>(this._hangoutPoints[key]);
			}
		}

		// Token: 0x06003200 RID: 12800 RVA: 0x0027EC88 File Offset: 0x0027CE88
		public override void cleanupBeforePlayerExit()
		{
			if (!Game1.eventUp)
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			this.birds = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x06003201 RID: 12801 RVA: 0x0027ECAC File Offset: 0x0027CEAC
		public void RequestEndMovie(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (this.CurrentState == 1)
			{
				this.CurrentState = 2;
				for (int i = 0; i < this._viewingGroups.Count; i++)
				{
					int index = Game1.random.Next(this._viewingGroups.Count);
					List<Character> characters = this._viewingGroups[i];
					this._viewingGroups[i] = this._viewingGroups[index];
					this._viewingGroups[index] = characters;
				}
				this._ResetHangoutPoints();
				int character_index = 0;
				for (int group = 0; group < this._viewingGroups.Count; group++)
				{
					for (int j = 0; j < this._viewingGroups[group].Count; j++)
					{
						if (this._viewingGroups[group][j] is NPC)
						{
							NPC patron_character = this.GetMoviePatron(this._viewingGroups[group][j].Name);
							if (patron_character != null)
							{
								patron_character.setTileLocation(new Vector2(14f, 4f + (float)character_index * 1f));
								Point point = Game1.random.ChooseFrom(this._availableHangoutPoints[group]);
								int direction;
								if (!int.TryParse(this.doesTileHaveProperty(point.X, point.Y, "direction", "Paths", false), out direction))
								{
									direction = 2;
								}
								this._destinationPositions[patron_character.Name] = new KeyValuePair<Point, int>(point, direction);
								this.PathCharacterToLocation(patron_character, point, direction);
								this._availableHangoutPoints[group].Remove(point);
								character_index++;
							}
						}
					}
				}
			}
			Farmer player = Game1.GetPlayer(uid, true);
			(((player != null) ? player.team : null) ?? Game1.MasterPlayer.team).endMovieEvent.Fire(uid);
		}

		// Token: 0x06003202 RID: 12802 RVA: 0x0027EE98 File Offset: 0x0027D098
		public void PathCharacterToLocation(NPC character, Point point, int direction)
		{
			if (character.currentLocation != this)
			{
				return;
			}
			character.temporaryController = new PathFindController(character, this, character.TilePoint, direction)
			{
				pathToEndPoint = PathFindController.findPathForNPCSchedules(character.TilePoint, point, this, 30000, character)
			};
			character.followSchedule = true;
			character.ignoreScheduleToday = true;
		}

		// Token: 0x06003203 RID: 12803 RVA: 0x0027EEEC File Offset: 0x0027D0EC
		public static Dictionary<string, MovieConcession> GetConcessions()
		{
			if (MovieTheater._concessions == null)
			{
				MovieTheater._concessions = new Dictionary<string, MovieConcession>();
				foreach (ConcessionItemData data in DataLoader.Concessions(Game1.content))
				{
					MovieTheater._concessions[data.Id] = new MovieConcession(data);
				}
			}
			return MovieTheater._concessions;
		}

		// Token: 0x06003204 RID: 12804 RVA: 0x0027EF68 File Offset: 0x0027D168
		public static MovieConcession GetConcessionItem(string id)
		{
			MovieConcession concession;
			if (id == null || !MovieTheater.GetConcessions().TryGetValue(id, out concession))
			{
				return null;
			}
			return concession;
		}

		// Token: 0x06003205 RID: 12805 RVA: 0x0027EF8C File Offset: 0x0027D18C
		public bool OnPurchaseConcession(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
		{
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && this._spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
				{
					MovieConcession concession = (MovieConcession)salable;
					this._purchasedConcessions[invitation.invitedNPC.Name] = concession.Id;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", concession.DisplayName, invitation.invitedNPC.displayName));
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003206 RID: 12806 RVA: 0x0027F068 File Offset: 0x0027D268
		public bool HasInvitedSomeone(Farmer who)
		{
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && this._spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003207 RID: 12807 RVA: 0x0027F0F4 File Offset: 0x0027D2F4
		public bool HasPurchasedConcession(Farmer who)
		{
			if (!this.HasInvitedSomeone(who))
			{
				return false;
			}
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
				{
					using (NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>.KeysCollection.Enumerator enumerator2 = this._purchasedConcessions.Keys.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current == invitation.invitedNPC.Name && this._spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06003208 RID: 12808 RVA: 0x0027F1E8 File Offset: 0x0027D3E8
		public static Farmer GetFirstInvitedPlayer(NPC npc)
		{
			foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
			{
				if (invitation.invitedNPC.Name == npc.Name)
				{
					return invitation.farmer;
				}
			}
			return null;
		}

		// Token: 0x06003209 RID: 12809 RVA: 0x0027F264 File Offset: 0x0027D464
		public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
		{
			if (this.IgnoreTouchActions())
			{
				return;
			}
			if (!(ArgUtility.Get(action, 0, null, true) == "Theater_Exit"))
			{
				base.performTouchAction(action, playerStandingPosition);
				return;
			}
			Point exitTile;
			string error;
			if (!ArgUtility.TryGetPoint(action, 1, out exitTile, out error, "Point exitTile"))
			{
				base.LogTileTouchActionError(action, playerStandingPosition, error);
				return;
			}
			Point offset = Town.GetTheaterTileOffset();
			this._exitX = exitTile.X + offset.X;
			this._exitY = exitTile.Y + offset.Y;
			if (Game1.player.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
			{
				this._Leave();
				return;
			}
			Game1.player.position.Y -= ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
			Game1.player.Halt();
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_LeavePrompt"), Game1.currentLocation.createYesNoResponses(), "LeaveMovie");
		}

		// Token: 0x0600320A RID: 12810 RVA: 0x0027F36C File Offset: 0x0027D56C
		public static List<MovieConcession> GetConcessionsForGuest()
		{
			string npcName = null;
			foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
			{
				if (invitation.farmer == Game1.player && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
				{
					npcName = invitation.invitedNPC.Name;
					break;
				}
			}
			return MovieTheater.GetConcessionsForGuest(npcName);
		}

		// Token: 0x0600320B RID: 12811 RVA: 0x0027F3F8 File Offset: 0x0027D5F8
		public static List<MovieConcession> GetConcessionsForGuest(string npc_name)
		{
			if (npc_name == null)
			{
				npc_name = "Abigail";
			}
			List<MovieConcession> concessions = new List<MovieConcession>();
			List<MovieConcession> all_concessions = MovieTheater.GetConcessions().Values.ToList<MovieConcession>();
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			Utility.Shuffle<MovieConcession>(r, all_concessions);
			NPC npc = Game1.getCharacterFromName(npc_name, true, false);
			if (npc == null)
			{
				return concessions;
			}
			int num_loved = 1;
			int num_liked = 2;
			int num_disliked = 1;
			int min_concessions = 5;
			for (int i = 0; i < num_loved; i++)
			{
				for (int j = 0; j < all_concessions.Count; j++)
				{
					MovieConcession concession = all_concessions[j];
					if (MovieTheater.GetConcessionTasteForCharacter(npc, concession) == "love" && (!concession.Name.Equals("Stardrop Sorbet") || r.NextDouble() < 0.33))
					{
						concessions.Add(concession);
						all_concessions.RemoveAt(j);
						j--;
						break;
					}
				}
			}
			for (int k = 0; k < num_liked; k++)
			{
				for (int l = 0; l < all_concessions.Count; l++)
				{
					MovieConcession concession2 = all_concessions[l];
					if (MovieTheater.GetConcessionTasteForCharacter(npc, concession2) == "like")
					{
						concessions.Add(concession2);
						all_concessions.RemoveAt(l);
						l--;
						break;
					}
				}
			}
			for (int m = 0; m < num_disliked; m++)
			{
				for (int n = 0; n < all_concessions.Count; n++)
				{
					MovieConcession concession3 = all_concessions[n];
					if (MovieTheater.GetConcessionTasteForCharacter(npc, concession3) == "dislike")
					{
						concessions.Add(concession3);
						all_concessions.RemoveAt(n);
						n--;
						break;
					}
				}
			}
			for (int j2 = concessions.Count; j2 < min_concessions; j2++)
			{
				int i2 = 0;
				if (i2 < all_concessions.Count)
				{
					MovieConcession concession4 = all_concessions[i2];
					concessions.Add(concession4);
					all_concessions.RemoveAt(i2);
					i2--;
				}
			}
			if (MovieTheater._isJojaTheater)
			{
				if (!concessions.Exists((MovieConcession x) => x.Name.Equals("JojaCorn")))
				{
					MovieConcession jojaCorn = all_concessions.Find((MovieConcession x) => x.Name.Equals("JojaCorn"));
					if (jojaCorn != null)
					{
						concessions.Add(jojaCorn);
					}
				}
			}
			Utility.Shuffle<MovieConcession>(r, concessions);
			return concessions;
		}

		// Token: 0x0600320C RID: 12812 RVA: 0x0027F650 File Offset: 0x0027D850
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer == "LeaveMovie_Yes")
			{
				this._Leave();
				return true;
			}
			if (!(questionAndAnswer == "Concession_Yes"))
			{
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			Utility.TryOpenShopMenu("Concessions", this, null, null, true, true, null);
			ShopMenu menu = Game1.activeClickableMenu as ShopMenu;
			if (menu != null)
			{
				menu.onPurchase = new ShopMenu.OnPurchaseDelegate(this.OnPurchaseConcession);
			}
			return true;
		}

		// Token: 0x0600320D RID: 12813 RVA: 0x0027F6D1 File Offset: 0x0027D8D1
		protected void _Leave()
		{
			MovieTheater.forceMovieId = null;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.warpFarmer("Town", this._exitX, this._exitY, 2);
		}

		// Token: 0x0600320E RID: 12814 RVA: 0x0027F6FC File Offset: 0x0027D8FC
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			string a = ArgUtility.Get(action, 0, null, true);
			if (!(a == "Concessions"))
			{
				if (!(a == "Theater_Doors"))
				{
					if (!(a == "CraneGame"))
					{
						return base.performAction(action, who, tileLocation);
					}
					if (!base.hasTileAt(2, 9, "Buildings", null))
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CranePlay", 500), base.createYesNoResponses(), new GameLocation.afterQuestionBehavior(this.tryToStartCraneGame), null);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CraneOccupied"));
					}
					return true;
				}
				else
				{
					if (this.CurrentState > 0)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Theater_MovieEndReEntry"));
						return true;
					}
					if (Game1.player.team.movieMutex.IsLocked())
					{
						this._ShowMovieStartReady();
						return true;
					}
					Game1.player.team.movieMutex.RequestLock(delegate
					{
						List<Farmer> farmers_here = new List<Farmer>();
						foreach (Farmer farmer in this.farmers)
						{
							if (farmer.isActive() && farmer.currentLocation == this)
							{
								farmers_here.Add(farmer);
							}
						}
						this.movieViewerLockEvent.Fire(new MovieViewerLockEvent(farmers_here, Game1.timeOfDay));
					}, null);
					return true;
				}
			}
			else
			{
				if (this.CurrentState > 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAfterMovie"));
					return true;
				}
				if (!this.HasInvitedSomeone(who))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAlone"));
					return true;
				}
				if (this.HasPurchasedConcession(who))
				{
					foreach (MovieInvitation invitation in who.team.movieInvitations)
					{
						if (invitation.farmer == who && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
						{
							foreach (string name in this._purchasedConcessions.Keys)
							{
								if (name == invitation.invitedNPC.Name)
								{
									MovieConcession concession = this.GetConcessionsDictionary()[Game1.getCharacterFromName(name, true, false)];
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", concession.DisplayName, Game1.RequireCharacter(name, true).displayName));
									return true;
								}
							}
						}
					}
					return true;
				}
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_Concession"), Game1.currentLocation.createYesNoResponses(), "Concession");
				return true;
			}
		}

		// Token: 0x0600320F RID: 12815 RVA: 0x0027F98C File Offset: 0x0027DB8C
		private void tryToStartCraneGame(Farmer who, string whichAnswer)
		{
			if (whichAnswer.EqualsIgnoreCase("yes"))
			{
				if (Game1.player.Money >= 500)
				{
					Game1.player.Money -= 500;
					Game1.changeMusicTrack("none", false, MusicContext.MiniGame);
					Game1.globalFadeToBlack(delegate
					{
						Game1.currentMinigame = new CraneGame();
					}, 0.008f);
					return;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
			}
		}

		// Token: 0x06003210 RID: 12816 RVA: 0x0027FA17 File Offset: 0x0027DC17
		public static void ClearCachedLocalizedData()
		{
			MovieTheater._concessions = null;
			MovieTheater._genericReactions = null;
			MovieTheater._movieData = null;
		}

		// Token: 0x06003211 RID: 12817 RVA: 0x0027FA2B File Offset: 0x0027DC2B
		public static void ClearCachedConcessionTastes()
		{
			MovieTheater._concessionTastes = null;
		}

		// Token: 0x0400215A RID: 8538
		protected bool _startedMovie;

		// Token: 0x0400215B RID: 8539
		protected static bool _isJojaTheater = false;

		// Token: 0x0400215C RID: 8540
		protected static List<MovieData> _movieData;

		// Token: 0x0400215D RID: 8541
		protected static Dictionary<string, MovieData> _movieDataById;

		// Token: 0x0400215E RID: 8542
		protected static List<MovieCharacterReaction> _genericReactions;

		// Token: 0x0400215F RID: 8543
		protected static List<ConcessionTaste> _concessionTastes;

		// Token: 0x04002160 RID: 8544
		protected readonly NetStringDictionary<int, NetInt> _spawnedMoviePatrons = new NetStringDictionary<int, NetInt>();

		// Token: 0x04002161 RID: 8545
		protected readonly NetStringDictionary<string, NetString> _purchasedConcessions = new NetStringDictionary<string, NetString>();

		// Token: 0x04002162 RID: 8546
		protected readonly NetStringDictionary<int, NetInt> _playerInvitedPatrons = new NetStringDictionary<int, NetInt>();

		// Token: 0x04002163 RID: 8547
		protected readonly NetStringDictionary<bool, NetBool> _characterGroupLookup = new NetStringDictionary<bool, NetBool>();

		// Token: 0x04002164 RID: 8548
		protected Dictionary<int, List<Point>> _hangoutPoints;

		// Token: 0x04002165 RID: 8549
		protected Dictionary<int, List<Point>> _availableHangoutPoints;

		// Token: 0x04002166 RID: 8550
		protected int _maxHangoutGroups;

		// Token: 0x04002167 RID: 8551
		protected int _movieStartTime = -1;

		// Token: 0x04002168 RID: 8552
		[XmlElement("dayFirstEntered")]
		public readonly NetInt dayFirstEntered = new NetInt(-1);

		// Token: 0x04002169 RID: 8553
		protected static Dictionary<string, MovieConcession> _concessions;

		// Token: 0x0400216A RID: 8554
		public const int LOVE_MOVIE_FRIENDSHIP = 200;

		// Token: 0x0400216B RID: 8555
		public const int LIKE_MOVIE_FRIENDSHIP = 100;

		// Token: 0x0400216C RID: 8556
		public const int DISLIKE_MOVIE_FRIENDSHIP = 0;

		// Token: 0x0400216D RID: 8557
		public const int LOVE_CONCESSION_FRIENDSHIP = 50;

		// Token: 0x0400216E RID: 8558
		public const int LIKE_CONCESSION_FRIENDSHIP = 25;

		// Token: 0x0400216F RID: 8559
		public const int DISLIKE_CONCESSION_FRIENDSHIP = 0;

		// Token: 0x04002170 RID: 8560
		public const int OPEN_TIME = 900;

		// Token: 0x04002171 RID: 8561
		public const int CLOSE_TIME = 2100;

		// Token: 0x04002172 RID: 8562
		public const string MainTileSheetId = "movieTheater_tileSheet";

		// Token: 0x04002173 RID: 8563
		[XmlIgnore]
		protected Dictionary<string, KeyValuePair<Point, int>> _destinationPositions = new Dictionary<string, KeyValuePair<Point, int>>();

		// Token: 0x04002174 RID: 8564
		[XmlIgnore]
		public PerchingBirds birds;

		// Token: 0x04002175 RID: 8565
		[XmlIgnore]
		public static string forceMovieId;

		// Token: 0x04002176 RID: 8566
		protected int _exitX;

		// Token: 0x04002177 RID: 8567
		protected int _exitY;

		// Token: 0x04002178 RID: 8568
		private NetEvent1<MovieViewerLockEvent> movieViewerLockEvent = new NetEvent1<MovieViewerLockEvent>();

		// Token: 0x04002179 RID: 8569
		private NetEvent1<StartMovieEvent> startMovieEvent = new NetEvent1<StartMovieEvent>();

		// Token: 0x0400217A RID: 8570
		private NetEvent1Field<long, NetLong> requestStartMovieEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x0400217B RID: 8571
		private NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x0400217C RID: 8572
		protected List<Farmer> _viewingFarmers = new List<Farmer>();

		// Token: 0x0400217D RID: 8573
		protected List<List<Character>> _viewingGroups = new List<List<Character>>();

		// Token: 0x0400217E RID: 8574
		protected List<List<Character>> _playerGroups = new List<List<Character>>();

		// Token: 0x0400217F RID: 8575
		protected List<List<Character>> _npcGroups = new List<List<Character>>();

		// Token: 0x04002180 RID: 8576
		protected static bool _hasRequestedMovieStart = false;

		// Token: 0x04002181 RID: 8577
		protected static int _playerHangoutGroup = -1;

		// Token: 0x04002182 RID: 8578
		protected int _farmerCount;

		// Token: 0x04002183 RID: 8579
		protected readonly NetInt currentState = new NetInt();

		// Token: 0x04002184 RID: 8580
		protected readonly NetInt showingId = new NetInt();

		// Token: 0x04002185 RID: 8581
		public static string[][][][] possibleNPCGroups = new string[][][][]
		{
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Jas",
						"Vincent",
						"Marnie"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[]
					{
						"Penny",
						"Maru"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Lewis",
						"Marnie"
					}
				}
			},
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Clint"
					},
					new string[]
					{
						"Demetrius",
						"Robin"
					},
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Caroline",
						"Jodi"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Lewis"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				}
			},
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Evelyn",
						"George"
					},
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Sandy",
						"Emily"
					},
					new string[]
					{
						"Elliot"
					}
				}
			},
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Alex",
						"Haley",
						"Emily"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Pierre",
						"Caroline"
					},
					new string[]
					{
						"Shane",
						"Jas",
						"Marnie"
					}
				}
			},
			new string[][][]
			{
				null,
				new string[][]
				{
					new string[]
					{
						"Haley",
						"Emily"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				}
			},
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Harvey",
						"Maru",
						"Penny"
					},
					new string[]
					{
						"Leah"
					}
				}
			},
			new string[][][]
			{
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"George",
						"Evelyn",
						"Alex"
					},
					new string[]
					{
						"Lewis"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Gus",
						"Willy"
					},
					new string[]
					{
						"Maru",
						"Sebastian"
					}
				},
				new string[][]
				{
					new string[]
					{
						"Penny",
						"Pam"
					},
					new string[]
					{
						"Sandy",
						"Emily"
					}
				}
			}
		};

		// Token: 0x0200066A RID: 1642
		public enum MovieStates
		{
			// Token: 0x04002F96 RID: 12182
			Preshow,
			// Token: 0x04002F97 RID: 12183
			Show,
			// Token: 0x04002F98 RID: 12184
			PostShow
		}
	}
}
