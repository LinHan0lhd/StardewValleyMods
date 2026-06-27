using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E0 RID: 736
	public class IslandSouthEastCave : IslandLocation
	{
		// Token: 0x0600309B RID: 12443 RVA: 0x00266891 File Offset: 0x00264A91
		public IslandSouthEastCave()
		{
		}

		// Token: 0x0600309C RID: 12444 RVA: 0x002668A4 File Offset: 0x00264AA4
		public IslandSouthEastCave(string map, string name) : base(map, name)
		{
		}

		// Token: 0x0600309D RID: 12445 RVA: 0x002668B9 File Offset: 0x00264AB9
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.drinksClaimed, "drinksClaimed");
		}

		// Token: 0x0600309E RID: 12446 RVA: 0x002668D8 File Offset: 0x00264AD8
		public override void updateMap()
		{
			if (IslandSouthEastCave.isPirateNight())
			{
				this.mapPath.Value = "Maps\\IslandSouthEastCave_pirates";
			}
			else
			{
				this.mapPath.Value = "Maps\\IslandSouthEastCave";
			}
			base.updateMap();
		}

		// Token: 0x0600309F RID: 12447 RVA: 0x0026690C File Offset: 0x00264B0C
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (IslandSouthEastCave.isPirateNight())
			{
				base.setTileProperty(19, 9, "Buildings", "Action", "MessageSpeech Pirates1");
				base.setTileProperty(20, 9, "Buildings", "Action", "MessageSpeech Pirates2");
				base.setTileProperty(26, 17, "Buildings", "Action", "MessageSpeech Pirates3");
				base.setTileProperty(23, 8, "Buildings", "Action", "MessageSpeech Pirates4");
				base.setTileProperty(27, 5, "Buildings", "Action", "MessageSpeech Pirates5");
				base.setTileProperty(32, 6, "Buildings", "Action", "MessageSpeech Pirates6");
				base.setTileProperty(30, 8, "Buildings", "Action", "DartsGame");
				base.setTileProperty(33, 8, "Buildings", "Action", "Bartender");
			}
		}

		// Token: 0x060030A0 RID: 12448 RVA: 0x002669F0 File Offset: 0x00264BF0
		protected override void resetLocalState()
		{
			this.wasPirateCaveOnLoad = IslandSouthEastCave.isPirateNight();
			base.resetLocalState();
			if (IslandSouthEastCave.isPirateNight())
			{
				this.addFlame(new Vector2(25.6f, 5.7f), 0f);
				this.addFlame(new Vector2(18f, 11f) + new Vector2(0.2f, -0.05f), 2.25f);
				this.addFlame(new Vector2(22f, 11f) + new Vector2(0.2f, -0.05f), 2.25f);
				this.addFlame(new Vector2(23f, 16f) + new Vector2(0.2f, -0.05f), 2.25f);
				this.addFlame(new Vector2(19f, 27f) + new Vector2(0.2f, -0.05f), 2.25f);
				this.addFlame(new Vector2(33f, 10f) + new Vector2(0.2f, -0.05f), 2.25f);
				this.addFlame(new Vector2(21f, 22f) + new Vector2(0.2f, -0.05f), 2.25f);
				this._parrotTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
				this._parrots = new PerchingBirds(this._parrotTextures, 3, 24, 24, new Vector2(12f, 19f), new Point[]
				{
					new Point(12, 2),
					new Point(35, 6),
					new Point(25, 14),
					new Point(28, 1),
					new Point(27, 12)
				}, new Point[0]);
				this._parrots.peckDuration = 0;
				for (int i = 0; i < 3; i++)
				{
					this._parrots.AddBird(Game1.random.Next(0, 4));
				}
				Game1.changeMusicTrack("PIRATE_THEME", true, MusicContext.Default);
			}
			if (base.AreMoonlightJelliesOut())
			{
				base.addMoonlightJellies(40, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0, 0.0, 0.0), new Microsoft.Xna.Framework.Rectangle(0, 0, 30, 15));
			}
		}

		// Token: 0x060030A1 RID: 12449 RVA: 0x00266C64 File Offset: 0x00264E64
		public static bool isWearingPirateClothes(Farmer who)
		{
			if (who.hat.Value != null)
			{
				string hatId = who.hat.Value.ItemId;
				if (hatId == "62" || hatId == "76" || hatId == "24")
				{
					return true;
				}
			}
			return who.hasTrinketWithID("ParrotEgg");
		}

		// Token: 0x060030A2 RID: 12450 RVA: 0x00266CC8 File Offset: 0x00264EC8
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (who.IsLocalPlayer)
			{
				string a = ArgUtility.Get(action, 0, null, true);
				if (!(a == "Bartender"))
				{
					if (a == "DartsGame")
					{
						string question_prompt;
						switch (Game1.player.team.GetDroppedLimitedNutCount("Darts"))
						{
						case 0:
							question_prompt = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_0");
							break;
						case 1:
							question_prompt = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_1");
							break;
						case 2:
							question_prompt = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_2");
							break;
						default:
							question_prompt = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_3");
							break;
						}
						base.createQuestionDialogue(question_prompt, base.createYesNoResponses(), "DartsGame");
					}
				}
				else if (IslandSouthEastCave.isWearingPirateClothes(who))
				{
					if (this.drinksClaimed.Contains(Game1.player.UniqueMultiplayerID))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:PirateBartender_PirateClothes_NoMore"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:PirateBartender_PirateClothes"));
						ItemGrabMenu.behaviorOnItemSelect <>9__1;
						Game1.afterDialogues = delegate()
						{
							Farmer who2 = who;
							Item item = ItemRegistry.Create("(O)459", 1, 0, false);
							ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback;
							if ((itemSelectedCallback = <>9__1) == null)
							{
								itemSelectedCallback = (<>9__1 = delegate(Item x, Farmer y)
								{
									this.drinksClaimed.Add(Game1.player.UniqueMultiplayerID);
								});
							}
							who2.addItemByMenuIfNecessary(item, itemSelectedCallback, false);
						};
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:Pirates8"));
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x060030A3 RID: 12451 RVA: 0x00266E34 File Offset: 0x00265034
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer == "DartsGame_Yes")
			{
				int won_dart_nuts = Game1.player.team.GetDroppedLimitedNutCount("Darts");
				int darts;
				if (won_dart_nuts != 1)
				{
					if (won_dart_nuts != 2)
					{
						darts = 20;
					}
					else
					{
						darts = 10;
					}
				}
				else
				{
					darts = 15;
				}
				Game1.currentMinigame = new Darts(darts);
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x060030A4 RID: 12452 RVA: 0x00266E94 File Offset: 0x00265094
		public override void cleanupBeforePlayerExit()
		{
			this._parrots = null;
			this._parrotTextures = null;
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x060030A5 RID: 12453 RVA: 0x00266EAC File Offset: 0x002650AC
		private void addFlame(Vector2 tileLocation, float sort_offset_tiles = 2.25f)
		{
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), tileLocation * 64f, false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "IslandSouthEastCave_Flame",
				lightRadius = 2f,
				scale = 4f,
				layerDepth = (tileLocation.Y + sort_offset_tiles) * 64f / 10000f
			});
		}

		// Token: 0x060030A6 RID: 12454 RVA: 0x00266F4F File Offset: 0x0026514F
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			PerchingBirds parrots = this._parrots;
			if (parrots != null)
			{
				parrots.Draw(b);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		// Token: 0x060030A7 RID: 12455 RVA: 0x00266F6A File Offset: 0x0026516A
		public override void DayUpdate(int dayOfMonth)
		{
			this.drinksClaimed.Clear();
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x060030A8 RID: 12456 RVA: 0x00266F7E File Offset: 0x0026517E
		public override void SetBuriedNutLocations()
		{
			base.SetBuriedNutLocations();
			this.buriedNutPoints.Add(new Point(36, 26));
		}

		// Token: 0x060030A9 RID: 12457 RVA: 0x00266F9C File Offset: 0x0026519C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (IslandSouthEastCave.isPirateNight())
			{
				if (Game1.currentLocation == this && !this.wasPirateCaveOnLoad && Game1.locationRequest == null && Game1.activeClickableMenu == null && Game1.currentMinigame == null && Game1.CurrentEvent == null)
				{
					if (Game1.player.CurrentTool != null)
					{
						FishingRod rod = Game1.player.CurrentTool as FishingRod;
						if (rod != null && (rod.pullingOutOfWater || rod.fishCaught || rod.showingTreasure))
						{
							return;
						}
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.warpFarmer("IslandSouthEast", 29, 19, 1);
				}
				PerchingBirds parrots = this._parrots;
				if (parrots != null)
				{
					parrots.Update(time);
				}
				this.smokeTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.smokeTimer <= 0f)
				{
					Utility.addSmokePuff(this, new Vector2(25.6f, 5.7f) * 64f, 0, 2f, 0.02f, 0.75f, 0.002f);
					Utility.addSmokePuff(this, new Vector2(34f, 7.2f) * 64f, 0, 2f, 0.02f, 0.75f, 0.002f);
					this.smokeTimer = 1000f;
				}
			}
		}

		// Token: 0x060030AA RID: 12458 RVA: 0x002670E5 File Offset: 0x002652E5
		public static bool isPirateNight()
		{
			return !Game1.IsRainingHere(null) && Game1.timeOfDay >= 2000 && Game1.dayOfMonth % 2 == 0;
		}

		// Token: 0x060030AB RID: 12459 RVA: 0x00267108 File Offset: 0x00265308
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandSouthEastCave cave = l as IslandSouthEastCave;
			if (cave != null)
			{
				this.drinksClaimed.Clear();
				foreach (long id in cave.drinksClaimed)
				{
					this.drinksClaimed.Add(id);
				}
			}
		}

		// Token: 0x040020C3 RID: 8387
		protected PerchingBirds _parrots;

		// Token: 0x040020C4 RID: 8388
		protected Texture2D _parrotTextures;

		// Token: 0x040020C5 RID: 8389
		public NetLongList drinksClaimed = new NetLongList();

		// Token: 0x040020C6 RID: 8390
		[XmlIgnore]
		public bool wasPirateCaveOnLoad;

		// Token: 0x040020C7 RID: 8391
		private float smokeTimer;
	}
}
