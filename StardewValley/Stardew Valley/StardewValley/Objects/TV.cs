using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001B9 RID: 441
	public class TV : Furniture
	{
		// Token: 0x06001F80 RID: 8064 RVA: 0x00168B9F File Offset: 0x00166D9F
		public TV()
		{
		}

		// Token: 0x06001F81 RID: 8065 RVA: 0x00168BA7 File Offset: 0x00166DA7
		public TV(string itemId, Vector2 tile) : base(itemId, tile)
		{
		}

		// Token: 0x06001F82 RID: 8066 RVA: 0x00168BB4 File Offset: 0x00166DB4
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Response> channels = new List<Response>();
			channels.Add(new Response("Weather", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105")));
			channels.Add(new Response("Fortune", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107")));
			string a = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (!(a == "Mon") && !(a == "Thu"))
			{
				if (!(a == "Sun"))
				{
					if (a == "Wed")
					{
						if (Game1.stats.DaysPlayed > 7U)
						{
							channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117")));
						}
					}
				}
				else
				{
					channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114")));
				}
			}
			else
			{
				channels.Add(new Response("Livin'", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111")));
			}
			if (Game1.Date.Season == Season.Fall && Game1.Date.DayOfMonth == 26 && Game1.stats.Get("childrenTurnedToDoves") > 0U && !who.mailReceived.Contains("cursed_doll"))
			{
				channels.Add(new Response("???", "???"));
			}
			if (Game1.player.mailReceived.Contains("pamNewChannel"))
			{
				channels.Add(new Response("Fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel")));
			}
			channels.Add(new Response("(Leave)", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"), channels.ToArray(), new GameLocation.afterQuestionBehavior(this.selectChannel), null);
			Game1.player.Halt();
			return true;
		}

		// Token: 0x06001F83 RID: 8067 RVA: 0x00168D99 File Offset: 0x00166F99
		protected override Item GetOneNew()
		{
			return new TV(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001F84 RID: 8068 RVA: 0x00168DB4 File Offset: 0x00166FB4
		public virtual void selectChannel(Farmer who, string answer)
		{
			if (Game1.IsGreenRainingHere(null))
			{
				this.currentChannel = 9999;
				this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(386, 334, 42, 28), 40f, 3, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				Game1.drawObjectDialogue("...................");
				Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
			}
			else
			{
				string a = ArgUtility.SplitBySpaceAndGet(answer, 0, null);
				if (!(a == "Weather"))
				{
					if (!(a == "Fortune"))
					{
						if (!(a == "Livin'"))
						{
							if (!(a == "The"))
							{
								if (!(a == "???"))
								{
									if (a == "Fishing")
									{
										this.currentChannel = 6;
										this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(172, 33, 42, 28), 150f, 2, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Fishing_Channel_Intro")));
										Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
									}
								}
								else
								{
									Game1.changeMusicTrack("none", false, MusicContext.Default);
									this.currentChannel = 666;
									this.screen = new TemporaryAnimatedSprite("Maps\\springobjects", new Rectangle(112, 64, 16, 16), 150f, 1, 999999, this.getScreenPosition() + ((base.QualifiedItemId == "(F)1468") ? new Vector2(56f, 32f) : new Vector2(8f, 8f)), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, 3f, 0f, 0f, 0f, false);
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Cursed_Doll")));
									Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
								}
							}
							else
							{
								this.currentChannel = 5;
								this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(602, 361, 42, 28), 150f, 2, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13127")));
								Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
							}
						}
						else
						{
							this.currentChannel = 4;
							this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(517, 361, 42, 28), 150f, 2, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13124")));
							Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
						}
					}
					else
					{
						this.currentChannel = 3;
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 305, 42, 28), 150f, 2, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
						Game1.drawObjectDialogue(Game1.parseText(this.getFortuneTellerOpening()));
						Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					}
				}
				else
				{
					this.currentChannel = 2;
					this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 305, 42, 28), 150f, 2, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
					Game1.drawObjectDialogue(Game1.parseText(this.getWeatherChannelOpening()));
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
				}
			}
			if (this.currentChannel > 0)
			{
				IDictionary<string, LightSource> currentLightSources = Game1.currentLightSources;
				string id = this.GenerateLightSourceId(this.TileLocation) + "_Screen";
				int textureIndex = 2;
				Vector2 position = this.getScreenPosition() + ((base.QualifiedItemId == "(F)1468") ? new Vector2(88f, 80f) : new Vector2(38f, 48f));
				float radius = (base.QualifiedItemId == "(F)1468") ? 1f : 0.55f;
				Color black = Color.Black;
				LightSource.LightContext lightContext = LightSource.LightContext.None;
				long playerID = 0L;
				GameLocation location = this.Location;
				currentLightSources.Add(new LightSource(id, textureIndex, position, radius, black, lightContext, playerID, (location != null) ? location.NameOrUniqueName : null));
			}
		}

		// Token: 0x06001F85 RID: 8069 RVA: 0x0016938C File Offset: 0x0016758C
		protected virtual string getFortuneTellerOpening()
		{
			switch (Game1.random.Next(5))
			{
			case 0:
				if (!Game1.player.IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13130");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13128");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13132");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13133");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13134");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13135");
			default:
				return "";
			}
		}

		// Token: 0x06001F86 RID: 8070 RVA: 0x00169432 File Offset: 0x00167632
		protected virtual string getWeatherChannelOpening()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13136");
		}

		// Token: 0x06001F87 RID: 8071 RVA: 0x00169443 File Offset: 0x00167643
		public virtual float getScreenSizeModifier()
		{
			if (!(base.QualifiedItemId == "(F)1468") && !(base.QualifiedItemId == "(F)2326"))
			{
				return 2f;
			}
			return 4f;
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x00169474 File Offset: 0x00167674
		public virtual Vector2 getScreenPosition()
		{
			string qualifiedItemId = base.QualifiedItemId;
			if (qualifiedItemId == "(F)1466")
			{
				return new Vector2((float)(this.boundingBox.X + 24), (float)this.boundingBox.Y);
			}
			if (qualifiedItemId == "(F)1468")
			{
				return new Vector2((float)(this.boundingBox.X + 12), (float)(this.boundingBox.Y - 128 + 32));
			}
			if (qualifiedItemId == "(F)2326")
			{
				return new Vector2((float)(this.boundingBox.X + 12), (float)(this.boundingBox.Y - 128 + 40));
			}
			if (qualifiedItemId == "(F)1680")
			{
				return new Vector2((float)(this.boundingBox.X + 24), (float)(this.boundingBox.Y - 12));
			}
			if (!(qualifiedItemId == "(F)RetroTV"))
			{
				return Vector2.Zero;
			}
			return new Vector2((float)(this.boundingBox.X + 24), (float)(this.boundingBox.Y - 64));
		}

		// Token: 0x06001F89 RID: 8073 RVA: 0x00169598 File Offset: 0x00167798
		public virtual void proceedToNextScene()
		{
			int num = this.currentChannel;
			switch (num)
			{
			case 2:
				if (this.screenOverlay == null)
				{
					if (Utility.isGreenRainDay(Game1.dayOfMonth + 1, Game1.season))
					{
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(213, 335, 43, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false)
						{
							id = 776
						};
					}
					else
					{
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false)
						{
							id = 777
						};
					}
					Game1.drawObjectDialogue(Game1.parseText(this.getWeatherForecast()));
					this.setWeatherOverlay(false);
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					return;
				}
				if (Game1.player.hasOrWillReceiveMail("Visited_Island") && this.screen.id == 777)
				{
					this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(148, 62, 42, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
					Game1.drawObjectDialogue(Game1.parseText(this.getIslandWeatherForecast()));
					this.setWeatherOverlay(true);
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					return;
				}
				this.turnOffTV();
				return;
			case 3:
				if (this.screenOverlay == null)
				{
					if (Game1.player.team.sharedDailyLuck.Value >= 0.1)
					{
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(424, 447, 42, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
					}
					else if (Game1.player.team.sharedDailyLuck.Value <= -0.1)
					{
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(424, 476, 42, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
					}
					else
					{
						this.screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(624, 305, 42, 28), 9999f, 1, 999999, this.getScreenPosition(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
					}
					Game1.drawObjectDialogue(Game1.parseText(this.getFortuneForecast(Game1.player)));
					this.setFortuneOverlay(Game1.player);
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					return;
				}
				this.turnOffTV();
				return;
			case 4:
				if (this.screenOverlay == null)
				{
					Game1.drawObjectDialogue(Game1.parseText(this.getTodaysTip()));
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					this.screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
					return;
				}
				this.turnOffTV();
				return;
			case 5:
				if (this.screenOverlay == null)
				{
					Game1.multipleDialogues(this.getWeeklyRecipe());
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					this.screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
					return;
				}
				this.turnOffTV();
				return;
			case 6:
				if (this.screenOverlay == null)
				{
					Game1.multipleDialogues(this.getFishingInfo());
					Game1.afterDialogues = new Game1.afterFadeFunction(this.proceedToNextScene);
					this.screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
					return;
				}
				this.turnOffTV();
				break;
			default:
				if (num == 666)
				{
					Game1.flashAlpha = 1f;
					Game1.playSound("batScreech", null);
					Game1.createItemDebris(ItemRegistry.Create("(O)103", 1, 0, false), Game1.player.getStandingPosition(), 1, Game1.currentLocation, -1, false);
					Game1.player.mailReceived.Add("cursed_doll");
					this.turnOffTV();
					return;
				}
				if (num == 9999)
				{
					this.turnOffTV();
					return;
				}
				break;
			}
		}

		// Token: 0x06001F8A RID: 8074 RVA: 0x00169AF2 File Offset: 0x00167CF2
		public virtual void turnOffTV()
		{
			this.currentChannel = 0;
			this.screen = null;
			this.screenOverlay = null;
			Utility.removeLightSource(this.GenerateLightSourceId(this.TileLocation) + "_Screen");
		}

		// Token: 0x06001F8B RID: 8075 RVA: 0x00169B28 File Offset: 0x00167D28
		protected virtual void setWeatherOverlay(bool island = false)
		{
			WorldDate tomorrow = new WorldDate(Game1.Date);
			WorldDate worldDate = tomorrow;
			int totalDays = worldDate.TotalDays + 1;
			worldDate.TotalDays = totalDays;
			string forecast;
			if (island)
			{
				forecast = Game1.netWorldState.Value.GetWeatherForLocation("Island").WeatherForTomorrow;
			}
			else if (Game1.IsMasterGame)
			{
				forecast = Game1.getWeatherModificationsForDate(tomorrow, Game1.weatherForTomorrow);
			}
			else
			{
				forecast = Game1.getWeatherModificationsForDate(tomorrow, Game1.netWorldState.Value.WeatherForTomorrow);
			}
			this.setWeatherOverlay(forecast);
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x00169BA4 File Offset: 0x00167DA4
		protected virtual void setWeatherOverlay(string weatherId)
		{
			if (weatherId == "Snow")
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 346, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (weatherId == "Rain")
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 333, 13, 13), 70f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (weatherId == "GreenRain")
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(178, 363, 13, 13), 80f, 6, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (weatherId == "Wind")
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.IsSpring ? new Rectangle(465, 359, 13, 13) : (Game1.IsFall ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13)), 70f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (weatherId == "Storm")
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 346, 13, 13), 120f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (!(weatherId == "Festival"))
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 333, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 372, 13, 13), 120f, 4, 999999, this.getScreenPosition() + new Vector2(3f, 3f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x0016A03C File Offset: 0x0016823C
		private string[] getFishingInfo()
		{
			List<string> allDialogues = new List<string>();
			StringBuilder sb = new StringBuilder();
			StringBuilder singleLineSB = new StringBuilder();
			int currentSeasonNumber = Game1.seasonIndex;
			sb.AppendLine("---" + Utility.getSeasonNameFromNumber(currentSeasonNumber) + "---^^");
			Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);
			IDictionary<string, LocationData> locationsData = Game1.locationData;
			List<string> locationsFound = new List<string>();
			int count = 0;
			foreach (KeyValuePair<string, string> v in dictionary)
			{
				if (!v.Value.Contains("spring summer fall winter"))
				{
					locationsFound.Clear();
					foreach (KeyValuePair<string, LocationData> pair in locationsData)
					{
						string locationName = pair.Key;
						GameLocation location = null;
						bool found = false;
						if (pair.Value.Fish != null)
						{
							foreach (SpawnFishData spawn in pair.Value.Fish)
							{
								if (!spawn.IsBossFish)
								{
									if (spawn.Season != null)
									{
										Season? season = spawn.Season;
										Season season2 = Game1.season;
										if (!(season.GetValueOrDefault() == season2 & season != null))
										{
											continue;
										}
									}
									if (spawn.ItemId == v.Key || spawn.ItemId == "(O)" + v.Key)
									{
										if (spawn.Condition != null)
										{
											location = (location ?? Game1.getLocationFromName(locationName));
											if (!GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, null))
											{
												continue;
											}
										}
										found = true;
										break;
									}
								}
							}
						}
						if (found)
						{
							string sanitizedLocation = this.getSanitizedFishingLocation(locationName);
							if (sanitizedLocation != "" && !locationsFound.Contains(sanitizedLocation))
							{
								locationsFound.Add(sanitizedLocation);
							}
						}
					}
					if (locationsFound.Count > 0)
					{
						string[] split = v.Value.Split('/', StringSplitOptions.None);
						string[] array = ArgUtility.SplitBySpace(split[5]);
						ParsedItemData data = ItemRegistry.GetData("(O)" + v.Key);
						string name = ((data != null) ? data.DisplayName : null) ?? split[0];
						string weather = split[7];
						string lowerTime = array[0];
						string upperTime = array[1];
						singleLineSB.Append(name);
						singleLineSB.Append("...... ");
						singleLineSB.Append(Game1.getTimeOfDayString(Convert.ToInt32(lowerTime)).Replace(" ", ""));
						singleLineSB.Append("-");
						singleLineSB.Append(Game1.getTimeOfDayString(Convert.ToInt32(upperTime)).Replace(" ", ""));
						if (weather != "both")
						{
							singleLineSB.Append(", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_" + weather));
						}
						bool anySanitized = false;
						foreach (string s in locationsFound)
						{
							if (s != "")
							{
								anySanitized = true;
								singleLineSB.Append(", ");
								singleLineSB.Append(s);
							}
						}
						if (anySanitized)
						{
							singleLineSB.Append("^^");
							sb.Append(singleLineSB.ToString());
							count++;
						}
						singleLineSB.Clear();
						if (count > 3)
						{
							allDialogues.Add(sb.ToString());
							sb.Clear();
							count = 0;
						}
					}
				}
			}
			return allDialogues.ToArray();
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x0016A450 File Offset: 0x00168650
		private string getSanitizedFishingLocation(string rawLocationName)
		{
			if (rawLocationName == "Town" || rawLocationName == "Forest")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_River");
			}
			if (rawLocationName == "Beach")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Ocean");
			}
			if (!(rawLocationName == "Mountain"))
			{
				return "";
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Lake");
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x0016A4C8 File Offset: 0x001686C8
		protected virtual string getTodaysTip()
		{
			string tip;
			if (!DataLoader.Tv_TipChannel(Game1.temporaryContent).TryGetValue((Game1.stats.DaysPlayed % 224U).ToString() ?? "", out tip))
			{
				tip = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13148");
			}
			return tip;
		}

		// Token: 0x06001F90 RID: 8080 RVA: 0x0016A51C File Offset: 0x0016871C
		protected int getRerunWeek()
		{
			int totalRerunWeeksAvailable = Math.Min((int)((Game1.stats.DaysPlayed - 3U) / 7U), 32);
			if (TV.weekToRecipeMap == null)
			{
				TV.weekToRecipeMap = new Dictionary<int, string>();
				Dictionary<string, string> cookingRecipeChannel = DataLoader.Tv_CookingChannel(Game1.temporaryContent);
				foreach (string key in cookingRecipeChannel.Keys)
				{
					TV.weekToRecipeMap[Convert.ToInt32(key)] = cookingRecipeChannel[key].Split('/', StringSplitOptions.None)[0];
				}
			}
			List<Farmer> players = new List<Farmer>();
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (player.isCustomized.Value && !player.IsDedicatedPlayer)
				{
					players.Add(player);
				}
			}
			List<int> recipeWeeksNotKnownByAllFarmers = new List<int>();
			for (int i = 1; i <= totalRerunWeeksAvailable; i++)
			{
				using (List<Farmer>.Enumerator enumerator3 = players.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						if (!enumerator3.Current.cookingRecipes.ContainsKey(TV.weekToRecipeMap[i]))
						{
							recipeWeeksNotKnownByAllFarmers.Add(i);
							break;
						}
					}
				}
			}
			Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			int whichWeek;
			if (recipeWeeksNotKnownByAllFarmers.Count == 0)
			{
				whichWeek = Math.Max(1, 1 + r.Next(totalRerunWeeksAvailable));
			}
			else
			{
				whichWeek = recipeWeeksNotKnownByAllFarmers[r.Next(recipeWeeksNotKnownByAllFarmers.Count)];
			}
			return whichWeek;
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x0016A6E4 File Offset: 0x001688E4
		protected virtual string[] getWeeklyRecipe()
		{
			int whichWeek = (int)(Game1.stats.DaysPlayed % 224U / 7U);
			if (Game1.stats.DaysPlayed % 224U == 0U)
			{
				whichWeek = 32;
			}
			Dictionary<string, string> cookingRecipeChannel = DataLoader.Tv_CookingChannel(Game1.temporaryContent);
			FarmerTeam team = Game1.player.team;
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed"))
			{
				if (team.lastDayQueenOfSauceRerunUpdated.Value != Game1.Date.TotalDays)
				{
					team.lastDayQueenOfSauceRerunUpdated.Set(Game1.Date.TotalDays);
					team.queenOfSauceRerunWeek.Set(this.getRerunWeek());
				}
				whichWeek = team.queenOfSauceRerunWeek.Value;
			}
			string[] weeklyRecipe;
			try
			{
				weeklyRecipe = this.getWeeklyRecipe(cookingRecipeChannel, whichWeek.ToString());
			}
			catch
			{
				weeklyRecipe = this.getWeeklyRecipe(cookingRecipeChannel, "1");
			}
			return weeklyRecipe;
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x0016A7C4 File Offset: 0x001689C4
		private string[] getWeeklyRecipe(Dictionary<string, string> channelData, string id)
		{
			string recipeName = channelData[id].Split('/', StringSplitOptions.None)[0];
			bool knowsRecipe = Game1.player.cookingRecipes.ContainsKey(recipeName);
			string recipeDisplayName = new CraftingRecipe(recipeName, true).DisplayName;
			string[] result = new string[]
			{
				channelData[id].Split('/', StringSplitOptions.None)[1],
				knowsRecipe ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", recipeDisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", recipeDisplayName)
			};
			if (!knowsRecipe)
			{
				Game1.player.cookingRecipes.Add(recipeName, 0);
			}
			return result;
		}

		// Token: 0x06001F93 RID: 8083 RVA: 0x0016A858 File Offset: 0x00168A58
		private string getIslandWeatherForecast()
		{
			WorldDate worldDate = new WorldDate(Game1.Date);
			int totalDays = worldDate.TotalDays + 1;
			worldDate.TotalDays = totalDays;
			string forecast = Game1.netWorldState.Value.GetWeatherForLocation("Island").WeatherForTomorrow;
			string response = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_IslandWeatherIntro");
			if (!(forecast == "Sun"))
			{
				if (!(forecast == "Rain"))
				{
					if (!(forecast == "Storm"))
					{
						response += "???";
					}
					else
					{
						response += Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
					}
				}
				else
				{
					response += Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
				}
			}
			else
			{
				response += Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs." + Game1.random.Choose("13182", "13183"));
			}
			return response;
		}

		// Token: 0x06001F94 RID: 8084 RVA: 0x0016A940 File Offset: 0x00168B40
		protected virtual string getWeatherForecast()
		{
			WorldDate tomorrow = new WorldDate(Game1.Date);
			WorldDate worldDate = tomorrow;
			int totalDays = worldDate.TotalDays + 1;
			worldDate.TotalDays = totalDays;
			string forecast;
			if (Game1.IsMasterGame)
			{
				forecast = Game1.getWeatherModificationsForDate(tomorrow, Game1.weatherForTomorrow);
			}
			else
			{
				forecast = Game1.getWeatherModificationsForDate(tomorrow, Game1.netWorldState.Value.WeatherForTomorrow);
			}
			return this.getWeatherForecast(forecast);
		}

		// Token: 0x06001F95 RID: 8085 RVA: 0x0016A99C File Offset: 0x00168B9C
		protected virtual string getWeatherForecast(string weatherId)
		{
			if (weatherId == "Festival")
			{
				Dictionary<string, string> festivalData;
				try
				{
					festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (Game1.dayOfMonth + 1).ToString());
				}
				catch (Exception)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13164");
				}
				string[] array = festivalData["conditions"].Split('/', StringSplitOptions.None);
				string[] timeParts = ArgUtility.SplitBySpace(array[1]);
				string festName = festivalData["name"];
				string locationName = array[0];
				int startTime = Convert.ToInt32(timeParts[0]);
				int endTime = Convert.ToInt32(timeParts[1]);
				string locationFullName = "";
				if (!(locationName == "Town"))
				{
					if (!(locationName == "Beach"))
					{
						if (locationName == "Forest")
						{
							locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13174");
						}
					}
					else
					{
						locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13172");
					}
				}
				else
				{
					locationFullName = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13170");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13175", new object[]
				{
					festName,
					locationFullName,
					Game1.getTimeOfDayString(startTime),
					Game1.getTimeOfDayString(endTime)
				});
			}
			if (weatherId == "Snow")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs." + Game1.random.Choose("13180", "13181"));
			}
			if (weatherId == "Rain")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
			}
			if (weatherId == "GreenRain")
			{
				return Game1.content.LoadString("Strings\\1_6_Strings:GreenRainForecast");
			}
			if (weatherId == "Storm")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
			}
			if (!(weatherId == "Wind"))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs." + Game1.random.Choose("13182", "13183"));
			}
			Season season = Game1.season;
			if (season == Season.Spring)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13187");
			}
			if (season != Season.Fall)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13190");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13189");
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x0016AC00 File Offset: 0x00168E00
		public virtual void setFortuneOverlay(Farmer who)
		{
			if (who.DailyLuck < -0.07)
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 346, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(15f, 1f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (who.DailyLuck < -0.02)
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 346, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(15f, 1f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (who.DailyLuck > 0.07)
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(644, 333, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(15f, 1f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			if (who.DailyLuck > 0.02)
			{
				this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 333, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(15f, 1f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
				return;
			}
			this.screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 333, 13, 13), 100f, 4, 999999, this.getScreenPosition() + new Vector2(15f, 1f) * this.getScreenSizeModifier(), false, false, (float)(this.boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, this.getScreenSizeModifier(), 0f, 0f, 0f, false);
		}

		// Token: 0x06001F97 RID: 8087 RVA: 0x0016AF2C File Offset: 0x0016912C
		public virtual string getFortuneForecast(Farmer who)
		{
			string fortune;
			if (who.team.sharedDailyLuck.Value == -0.12)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13191");
			}
			else if (who.DailyLuck < -0.07)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13192");
			}
			else if (who.DailyLuck < -0.02)
			{
				Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs." + Game1.random.Choose("13193", "13195"));
			}
			else if (who.team.sharedDailyLuck.Value == 0.12)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13197");
			}
			else if (who.DailyLuck > 0.07)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13198");
			}
			else if (who.DailyLuck > 0.02)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13199");
			}
			else
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13200");
			}
			if (who.DailyLuck == 0.0)
			{
				fortune = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13201");
			}
			return fortune;
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x0016B094 File Offset: 0x00169294
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (this.screen != null)
			{
				this.screen.update(Game1.currentGameTime);
				this.screen.draw(spriteBatch, false, 0, 0, 1f);
				if (this.screenOverlay != null)
				{
					this.screenOverlay.update(Game1.currentGameTime);
					this.screenOverlay.draw(spriteBatch, false, 0, 0, 1f);
				}
			}
		}

		// Token: 0x0400135A RID: 4954
		public const int customChannel = 1;

		// Token: 0x0400135B RID: 4955
		public const int weatherChannel = 2;

		// Token: 0x0400135C RID: 4956
		public const int fortuneTellerChannel = 3;

		// Token: 0x0400135D RID: 4957
		public const int tipsChannel = 4;

		// Token: 0x0400135E RID: 4958
		public const int cookingChannel = 5;

		// Token: 0x0400135F RID: 4959
		public const int fishingChannel = 6;

		// Token: 0x04001360 RID: 4960
		private int currentChannel;

		// Token: 0x04001361 RID: 4961
		private TemporaryAnimatedSprite screen;

		// Token: 0x04001362 RID: 4962
		private TemporaryAnimatedSprite screenOverlay;

		// Token: 0x04001363 RID: 4963
		private static Dictionary<int, string> weekToRecipeMap;
	}
}
