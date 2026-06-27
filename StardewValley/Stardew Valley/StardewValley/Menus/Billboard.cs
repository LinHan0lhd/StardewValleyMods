using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Quests;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus
{
	// Token: 0x0200024F RID: 591
	public class Billboard : IClickableMenu
	{
		// Token: 0x0600274C RID: 10060 RVA: 0x001BDF90 File Offset: 0x001BC190
		public Billboard(bool dailyQuest = false) : base(0, 0, 0, 0, true)
		{
			if (!Game1.player.hasOrWillReceiveMail("checkedBulletinOnce"))
			{
				Game1.player.mailReceived.Add("checkedBulletinOnce");
				Game1.RequireLocation<Town>("Town", false).checkedBoard();
			}
			this.dailyQuestBoard = dailyQuest;
			this.billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard");
			this.width = (dailyQuest ? 338 : 301) * 4;
			this.height = 792;
			Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
			this.xPositionOnScreen = (int)center.X;
			this.yPositionOnScreen = (int)center.Y;
			if (!dailyQuest)
			{
				this.booksellerdays = Utility.getDaysOfBooksellerThisSeason();
			}
			this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 128, this.yPositionOnScreen + this.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 0
			};
			this.UpdateDailyQuestButton();
			this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 20, this.yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
			Game1.playSound("bigSelect", null);
			if (!dailyQuest)
			{
				this.calendarDays = new List<ClickableTextureComponent>();
				Dictionary<int, List<NPC>> birthdays = this.GetBirthdays();
				for (int day = 1; day <= 28; day++)
				{
					List<Billboard.BillboardEvent> curEvents = this.GetEventsForDay(day, birthdays);
					if (curEvents.Count > 0)
					{
						this.calendarDayData[day] = new Billboard.BillboardDay(curEvents.ToArray());
					}
					int index = day - 1;
					this.calendarDays.Add(new ClickableTextureComponent(day.ToString(), new Rectangle(this.xPositionOnScreen + 152 + index % 7 * 32 * 4, this.yPositionOnScreen + 200 + index / 7 * 32 * 4, 124, 124), string.Empty, string.Empty, null, Rectangle.Empty, 1f, false)
					{
						myID = day,
						rightNeighborID = ((day % 7 != 0) ? (day + 1) : -1),
						leftNeighborID = ((day % 7 != 1) ? (day - 1) : -1),
						downNeighborID = day + 7,
						upNeighborID = ((day > 7) ? (day - 7) : -1)
					});
				}
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x0600274D RID: 10061 RVA: 0x001BE271 File Offset: 0x001BC471
		public virtual Dictionary<int, List<NPC>> GetBirthdays()
		{
			HashSet<string> addedBirthdays = new HashSet<string>();
			Dictionary<int, List<NPC>> birthdays = new Dictionary<int, List<NPC>>();
			Utility.ForEachVillager(delegate(NPC npc)
			{
				if (npc.Birthday_Season != Game1.currentSeason)
				{
					return true;
				}
				CharacterData data = npc.GetData();
				CalendarBehavior? calendarBehavior = (data != null) ? new CalendarBehavior?(data.Calendar) : null;
				if (calendarBehavior.GetValueOrDefault() == CalendarBehavior.HiddenAlways || (calendarBehavior.GetValueOrDefault() == CalendarBehavior.HiddenUntilMet && !Game1.player.friendshipData.ContainsKey(npc.Name)))
				{
					return true;
				}
				if (addedBirthdays.Contains(npc.Name))
				{
					return true;
				}
				List<NPC> list;
				if (!birthdays.TryGetValue(npc.Birthday_Day, out list))
				{
					list = (birthdays[npc.Birthday_Day] = new List<NPC>());
				}
				list.Add(npc);
				addedBirthdays.Add(npc.Name);
				return true;
			}, false);
			return birthdays;
		}

		// Token: 0x0600274E RID: 10062 RVA: 0x001BE2A8 File Offset: 0x001BC4A8
		public virtual List<Billboard.BillboardEvent> GetEventsForDay(int day, Dictionary<int, List<NPC>> birthdays)
		{
			List<Billboard.BillboardEvent> curEvents = new List<Billboard.BillboardEvent>();
			if (Utility.isFestivalDay(day, Game1.season))
			{
				string id = Game1.currentSeason + day.ToString();
				string festivalName = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + id)["name"];
				curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Festival, new string[]
				{
					id
				}, festivalName, null, default(Rectangle)));
			}
			string id2;
			PassiveFestivalData festivalData;
			if (Utility.TryGetPassiveFestivalDataForDay(day, Game1.season, null, out id2, out festivalData, true))
			{
				bool? flag = (festivalData != null) ? new bool?(festivalData.ShowOnCalendar) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					string festivalName2 = TokenParser.ParseText(festivalData.DisplayName, null, null, null);
					if (!GameStateQuery.CheckConditions(festivalData.Condition, null, null, null, null, null, null))
					{
						curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.PassiveFestival, new string[]
						{
							id2
						}, "???", null, default(Rectangle))
						{
							locked = true
						});
					}
					else
					{
						curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.PassiveFestival, new string[]
						{
							id2
						}, festivalName2, null, default(Rectangle)));
					}
				}
			}
			if (Game1.IsSummer && (day == 20 || day == 21))
			{
				string festivalName3 = Game1.content.LoadString("Strings\\1_6_Strings:TroutDerby");
				curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.FishingDerby, LegacyShims.EmptyArray<string>(), festivalName3, null, default(Rectangle)));
			}
			else if (Game1.IsWinter && (day == 12 || day == 13))
			{
				string festivalName4 = Game1.content.LoadString("Strings\\1_6_Strings:SquidFest");
				curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.FishingDerby, LegacyShims.EmptyArray<string>(), festivalName4, null, default(Rectangle)));
			}
			if (this.booksellerdays.Contains(day))
			{
				string name = Game1.content.LoadString("Strings\\1_6_Strings:Bookseller");
				curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Bookseller, LegacyShims.EmptyArray<string>(), name, null, default(Rectangle)));
			}
			List<NPC> birthdayVillagers;
			if (birthdays.TryGetValue(day, out birthdayVillagers))
			{
				foreach (NPC i in birthdayVillagers)
				{
					char lastChar = i.displayName.Last<char>();
					string displayText = (lastChar == 's' || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de && (lastChar == 'x' || lastChar == 'ß' || lastChar == 'z'))) ? Game1.content.LoadString("Strings\\UI:Billboard_SBirthday", i.displayName) : Game1.content.LoadString("Strings\\UI:Billboard_Birthday", i.displayName);
					Texture2D character_texture;
					try
					{
						character_texture = Game1.content.Load<Texture2D>("Characters\\" + i.getTextureName());
					}
					catch
					{
						character_texture = i.Sprite.Texture;
					}
					curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Birthday, new string[]
					{
						i.Name
					}, displayText, character_texture, i.getMugShotSourceRect()));
				}
			}
			HashSet<Farmer> traversed_farmers = new HashSet<Farmer>();
			FarmerCollection onlineFarmers = Game1.getOnlineFarmers();
			foreach (Farmer farmer in onlineFarmers)
			{
				if (!traversed_farmers.Contains(farmer) && farmer.isEngaged() && !farmer.hasCurrentOrPendingRoommate())
				{
					string spouse_name = null;
					WorldDate wedding_date = null;
					NPC spouse = Game1.getCharacterFromName(farmer.spouse, true, false);
					if (spouse != null)
					{
						wedding_date = farmer.friendshipData[farmer.spouse].WeddingDate;
						spouse_name = spouse.displayName;
					}
					else
					{
						long? spouseId = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
						if (spouseId != null)
						{
							Farmer spouse_farmer = Game1.GetPlayer(spouseId.Value, false);
							if (spouse_farmer != null && onlineFarmers.Contains(spouse_farmer))
							{
								wedding_date = farmer.team.GetFriendship(farmer.UniqueMultiplayerID, spouseId.Value).WeddingDate;
								traversed_farmers.Add(spouse_farmer);
								spouse_name = spouse_farmer.Name;
							}
						}
					}
					if (!(wedding_date == null))
					{
						if (wedding_date.TotalDays < Game1.Date.TotalDays)
						{
							wedding_date = new WorldDate(Game1.Date);
							wedding_date.TotalDays++;
						}
						int? num = (wedding_date != null) ? new int?(wedding_date.TotalDays) : null;
						int totalDays = Game1.Date.TotalDays;
						if ((num.GetValueOrDefault() >= totalDays & num != null) && Game1.season == wedding_date.Season && day == wedding_date.DayOfMonth)
						{
							curEvents.Add(new Billboard.BillboardEvent(Billboard.BillboardEventType.Wedding, new string[]
							{
								farmer.Name,
								spouse_name
							}, Game1.content.LoadString("Strings\\UI:Calendar_Wedding", farmer.Name, spouse_name), null, default(Rectangle)));
							traversed_farmers.Add(farmer);
						}
					}
				}
			}
			return curEvents;
		}

		// Token: 0x0600274F RID: 10063 RVA: 0x001BE7E0 File Offset: 0x001BC9E0
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID((!this.dailyQuestBoard) ? 1 : 0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002750 RID: 10064 RVA: 0x001BE7FD File Offset: 0x001BC9FD
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Game1.activeClickableMenu = new Billboard(this.dailyQuestBoard);
		}

		// Token: 0x06002751 RID: 10065 RVA: 0x001BE818 File Offset: 0x001BCA18
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.playSound("bigDeSelect", null);
			base.exitThisMenu(true);
		}

		// Token: 0x06002752 RID: 10066 RVA: 0x001BE840 File Offset: 0x001BCA40
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.acceptQuestButton.visible && this.acceptQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact", null);
				Game1.questOfTheDay.dailyQuest.Value = true;
				Game1.questOfTheDay.dayQuestAccepted.Value = Game1.Date.TotalDays;
				Game1.questOfTheDay.accepted.Value = true;
				Game1.questOfTheDay.canBeCancelled.Value = true;
				Game1.questOfTheDay.daysLeft.Value = 2;
				Game1.player.questLog.Add(Game1.questOfTheDay);
				Game1.player.acceptedDailyQuest.Set(true);
				this.UpdateDailyQuestButton();
			}
		}

		// Token: 0x06002753 RID: 10067 RVA: 0x001BE910 File Offset: 0x001BCB10
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.hoverText = "";
			if (this.dailyQuestBoard && Game1.questOfTheDay != null && !Game1.questOfTheDay.accepted.Value)
			{
				float oldScale = this.acceptQuestButton.scale;
				this.acceptQuestButton.scale = (this.acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (this.acceptQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
			}
			if (this.calendarDays != null)
			{
				foreach (ClickableTextureComponent c in this.calendarDays)
				{
					if (c.bounds.Contains(x, y))
					{
						Billboard.BillboardDay day;
						this.hoverText = (this.calendarDayData.TryGetValue(c.myID, out day) ? day.HoverText : string.Empty);
						break;
					}
				}
			}
		}

		// Token: 0x06002754 RID: 10068 RVA: 0x001BEA2C File Offset: 0x001BCC2C
		public override void draw(SpriteBatch b)
		{
			bool hide_mouse = false;
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			b.Draw(this.billboardTexture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(this.dailyQuestBoard ? new Rectangle(0, 0, 338, 198) : new Rectangle(0, 198, 301, 198)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (!this.dailyQuestBoard)
			{
				b.DrawString(Game1.dialogueFont, Utility.getSeasonNameFromNumber(Game1.seasonIndex), new Vector2((float)(this.xPositionOnScreen + 160), (float)(this.yPositionOnScreen + 80)), Game1.textColor);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_Year", Game1.year), new Vector2((float)(this.xPositionOnScreen + 448), (float)(this.yPositionOnScreen + 80)), Game1.textColor);
				for (int i = 0; i < this.calendarDays.Count; i++)
				{
					ClickableTextureComponent component = this.calendarDays[i];
					Billboard.BillboardDay day;
					if (this.calendarDayData.TryGetValue(component.myID, out day))
					{
						if (day.Texture != null)
						{
							b.Draw(day.Texture, new Vector2((float)(component.bounds.X + 48), (float)(component.bounds.Y + 28)), new Rectangle?(day.TextureSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						}
						if (day.Type.HasFlag(Billboard.BillboardEventType.PassiveFestival))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(component.bounds.X + 12), (float)(component.bounds.Y + 60) - Game1.dialogueButtonScale / 2f), new Rectangle(346, 392, 8, 8), day.GetEventOfType(Billboard.BillboardEventType.PassiveFestival).locked ? (Color.Black * 0.3f) : Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
						}
						if (day.Type.HasFlag(Billboard.BillboardEventType.Festival))
						{
							Utility.drawWithShadow(b, this.billboardTexture, new Vector2((float)(component.bounds.X + 40), (float)(component.bounds.Y + 56) - Game1.dialogueButtonScale / 2f), new Rectangle(1 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 14, 398, 14, 12), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
						}
						if (day.Type.HasFlag(Billboard.BillboardEventType.FishingDerby))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2((float)(this.calendarDays[i].bounds.X + 8), (float)(this.calendarDays[i].bounds.Y + 60) - Game1.dialogueButtonScale / 2f), new Rectangle(103, 2, 10, 11), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
						}
						if (day.Type.HasFlag(Billboard.BillboardEventType.Wedding))
						{
							b.Draw(Game1.mouseCursors2, new Vector2((float)(component.bounds.Right - 56), (float)(component.bounds.Top - 12)), new Rectangle?(new Rectangle(112, 32, 16, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						}
						if (day.Type.HasFlag(Billboard.BillboardEventType.Bookseller))
						{
							b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(component.bounds.Right - 72) - 2f * (float)Math.Sin((Game1.currentGameTime.TotalGameTime.TotalSeconds + (double)i * 0.3) * 3.0), (float)(component.bounds.Top + 52) - 2f * (float)Math.Cos((Game1.currentGameTime.TotalGameTime.TotalSeconds + (double)i * 0.3) * 2.0)), new Rectangle?(new Rectangle(71, 63, 8, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						}
					}
					if (Game1.dayOfMonth > i + 1)
					{
						b.Draw(Game1.staminaRect, component.bounds, Color.Gray * 0.25f);
					}
					else if (Game1.dayOfMonth == i + 1)
					{
						int offset = (int)(4f * Game1.dialogueButtonScale / 8f);
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), component.bounds.X - offset, component.bounds.Y - offset, component.bounds.Width + offset * 2, component.bounds.Height + offset * 2, Color.Blue, 4f, false, -1f);
					}
				}
			}
			else
			{
				if (Game1.options.SnappyMenus)
				{
					hide_mouse = true;
				}
				Quest questOfTheDay = Game1.questOfTheDay;
				if (string.IsNullOrEmpty((questOfTheDay != null) ? questOfTheDay.currentObjective : null))
				{
					b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2((float)(this.xPositionOnScreen + 384), (float)(this.yPositionOnScreen + 320)), Game1.textColor);
				}
				else
				{
					SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
					string description = Game1.parseText(Game1.questOfTheDay.questDescription, font, 640);
					Utility.drawTextWithShadow(b, description, font, new Vector2((float)(this.xPositionOnScreen + 320 + 32), (float)(this.yPositionOnScreen + 256)), Game1.textColor, 1f, -1f, -1, -1, 0.5f, 3);
					if (this.acceptQuestButton.visible)
					{
						hide_mouse = false;
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (this.acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale, true, -1f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptQuestButton.bounds.X + 12), (float)(this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					}
					if (Game1.stats.Get("BillboardQuestsDone") % 3U == 2U && (this.acceptQuestButton.visible || !Game1.questOfTheDay.completed.Value))
					{
						Utility.drawWithShadow(b, Game1.content.Load<Texture2D>("TileSheets\\Objects_2"), base.Position + new Vector2(215f, 144f) * 4f, new Rectangle(80, 128, 16, 16), Color.White, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, 0.35f);
						SpriteText.drawString(b, "x1", (int)base.Position.X + 936, (int)base.Position.Y + 596, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					}
				}
				bool drawAllStars = Game1.stats.Get("BillboardQuestsDone") % 3U == 0U && Game1.questOfTheDay != null && Game1.questOfTheDay.completed.Value;
				int j = 0;
				while ((long)j < (long)((ulong)(drawAllStars ? 3U : (Game1.stats.Get("BillboardQuestsDone") % 3U))))
				{
					b.Draw(this.billboardTexture, base.Position + new Vector2((float)(18 + 12 * j), 36f) * 4f, new Rectangle?(new Rectangle(140, 397, 10, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
					j++;
				}
				if (Game1.player.hasCompletedCommunityCenter())
				{
					b.Draw(this.billboardTexture, base.Position + new Vector2(290f, 59f) * 4f, new Rectangle?(new Rectangle(0, 427, 39, 54)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
				}
			}
			base.draw(b);
			if (!hide_mouse)
			{
				Game1.mouseCursorTransparency = 1f;
				base.drawMouse(b, false, -1);
				if (this.hoverText.Length > 0)
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
		}

		// Token: 0x06002755 RID: 10069 RVA: 0x001BF48E File Offset: 0x001BD68E
		public void UpdateDailyQuestButton()
		{
			if (this.acceptQuestButton == null)
			{
				return;
			}
			if (!this.dailyQuestBoard)
			{
				this.acceptQuestButton.visible = false;
				return;
			}
			this.acceptQuestButton.visible = Game1.CanAcceptDailyQuest();
		}

		// Token: 0x04001861 RID: 6241
		private Texture2D billboardTexture;

		// Token: 0x04001862 RID: 6242
		public const int basewidth = 338;

		// Token: 0x04001863 RID: 6243
		public const int baseWidth_calendar = 301;

		// Token: 0x04001864 RID: 6244
		public const int baseheight = 198;

		// Token: 0x04001865 RID: 6245
		private bool dailyQuestBoard;

		// Token: 0x04001866 RID: 6246
		public ClickableComponent acceptQuestButton;

		// Token: 0x04001867 RID: 6247
		public List<ClickableTextureComponent> calendarDays;

		// Token: 0x04001868 RID: 6248
		private string hoverText = "";

		// Token: 0x04001869 RID: 6249
		private List<int> booksellerdays;

		// Token: 0x0400186A RID: 6250
		public readonly Dictionary<int, Billboard.BillboardDay> calendarDayData = new Dictionary<int, Billboard.BillboardDay>();

		// Token: 0x020005E5 RID: 1509
		[Flags]
		public enum BillboardEventType
		{
			// Token: 0x04002DD5 RID: 11733
			None = 0,
			// Token: 0x04002DD6 RID: 11734
			Birthday = 1,
			// Token: 0x04002DD7 RID: 11735
			Festival = 2,
			// Token: 0x04002DD8 RID: 11736
			FishingDerby = 4,
			// Token: 0x04002DD9 RID: 11737
			PassiveFestival = 8,
			// Token: 0x04002DDA RID: 11738
			Wedding = 16,
			// Token: 0x04002DDB RID: 11739
			Bookseller = 32
		}

		// Token: 0x020005E6 RID: 1510
		public class BillboardDay
		{
			// Token: 0x170004EA RID: 1258
			// (get) Token: 0x06004357 RID: 17239 RVA: 0x00319299 File Offset: 0x00317499
			public Billboard.BillboardEventType Type { get; }

			// Token: 0x170004EB RID: 1259
			// (get) Token: 0x06004358 RID: 17240 RVA: 0x003192A1 File Offset: 0x003174A1
			public Billboard.BillboardEvent[] Events { get; }

			// Token: 0x170004EC RID: 1260
			// (get) Token: 0x06004359 RID: 17241 RVA: 0x003192A9 File Offset: 0x003174A9
			public string HoverText { get; }

			// Token: 0x170004ED RID: 1261
			// (get) Token: 0x0600435A RID: 17242 RVA: 0x003192B1 File Offset: 0x003174B1
			public Texture2D Texture { get; }

			// Token: 0x170004EE RID: 1262
			// (get) Token: 0x0600435B RID: 17243 RVA: 0x003192B9 File Offset: 0x003174B9
			public Rectangle TextureSourceRect { get; }

			// Token: 0x0600435C RID: 17244 RVA: 0x003192C4 File Offset: 0x003174C4
			public BillboardDay(Billboard.BillboardEvent[] events)
			{
				this.Events = events;
				this.HoverText = string.Empty;
				foreach (Billboard.BillboardEvent @event in events)
				{
					this.Type |= @event.Type;
					if (this.Texture == null && @event.Texture != null)
					{
						this.Texture = @event.Texture;
						this.TextureSourceRect = @event.TextureSourceRect;
					}
					this.HoverText = this.HoverText + @event.DisplayName + Environment.NewLine;
				}
				this.HoverText = this.HoverText.Trim();
			}

			// Token: 0x0600435D RID: 17245 RVA: 0x00319368 File Offset: 0x00317568
			public Billboard.BillboardEvent GetEventOfType(Billboard.BillboardEventType type)
			{
				foreach (Billboard.BillboardEvent b in this.Events)
				{
					if (b.Type == type)
					{
						return b;
					}
				}
				return null;
			}
		}

		// Token: 0x020005E7 RID: 1511
		public class BillboardEvent
		{
			// Token: 0x170004EF RID: 1263
			// (get) Token: 0x0600435E RID: 17246 RVA: 0x0031939A File Offset: 0x0031759A
			public Billboard.BillboardEventType Type { get; }

			// Token: 0x170004F0 RID: 1264
			// (get) Token: 0x0600435F RID: 17247 RVA: 0x003193A2 File Offset: 0x003175A2
			public string[] Arguments { get; }

			// Token: 0x170004F1 RID: 1265
			// (get) Token: 0x06004360 RID: 17248 RVA: 0x003193AA File Offset: 0x003175AA
			public string DisplayName { get; }

			// Token: 0x170004F2 RID: 1266
			// (get) Token: 0x06004361 RID: 17249 RVA: 0x003193B2 File Offset: 0x003175B2
			public Texture2D Texture { get; }

			// Token: 0x170004F3 RID: 1267
			// (get) Token: 0x06004362 RID: 17250 RVA: 0x003193BA File Offset: 0x003175BA
			public Rectangle TextureSourceRect { get; }

			// Token: 0x06004363 RID: 17251 RVA: 0x003193C2 File Offset: 0x003175C2
			public BillboardEvent(Billboard.BillboardEventType type, string[] arguments, string displayName, Texture2D texture = null, Rectangle sourceRect = default(Rectangle))
			{
				this.Type = type;
				this.Arguments = arguments;
				this.DisplayName = displayName;
				this.Texture = texture;
				this.TextureSourceRect = sourceRect;
			}

			// Token: 0x04002DE6 RID: 11750
			public bool locked;
		}
	}
}
