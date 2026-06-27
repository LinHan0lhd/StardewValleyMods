using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Monsters;
using StardewValley.Quests;

namespace StardewValley.Menus
{
	// Token: 0x02000265 RID: 613
	public class DayTimeMoneyBox : IClickableMenu
	{
		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x060028AF RID: 10415 RVA: 0x001DB41F File Offset: 0x001D961F
		public static int Width
		{
			get
			{
				return 300;
			}
		}

		// Token: 0x060028B0 RID: 10416 RVA: 0x001DB428 File Offset: 0x001D9628
		public DayTimeMoneyBox() : base(Game1.uiViewport.Width - 300 + 32, 8, 300, 284, false)
		{
			this.position = new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen);
			this.sourceRect = new Rectangle(333, 431, 71, 43);
			this.questButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 220, this.yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f, false);
			this.zoomOutButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 92, this.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f, false);
			this.zoomInButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 124, this.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f, false);
			this.questButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 220, this.yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f, false);
			this.zoomOutButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 92, this.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f, false);
			this.zoomInButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 124, this.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f, false);
		}

		// Token: 0x060028B1 RID: 10417 RVA: 0x001DB694 File Offset: 0x001D9894
		public override bool isWithinBounds(int x, int y)
		{
			return (Game1.options.zoomButtons && (this.zoomInButton.containsPoint(x, y) || this.zoomOutButton.containsPoint(x, y))) || (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y));
		}

		// Token: 0x060028B2 RID: 10418 RVA: 0x001DB6EC File Offset: 0x001D98EC
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y) && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp && Game1.farmEvent == null)
			{
				Game1.activeClickableMenu = new QuestLog();
			}
			if (Game1.options.zoomButtons)
			{
				if (this.zoomInButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel < 2f)
				{
					int zoom = (int)Math.Round((double)(Game1.options.desiredBaseZoomLevel * 100f));
					zoom -= zoom % 5;
					zoom += 5;
					Game1.options.desiredBaseZoomLevel = Math.Min(2f, (float)zoom / 100f);
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.playSound("drumkit6", null);
					return;
				}
				if (this.zoomOutButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel > 0.75f)
				{
					int zoom2 = (int)Math.Round((double)(Game1.options.desiredBaseZoomLevel * 100f));
					zoom2 -= zoom2 % 5;
					zoom2 -= 5;
					Game1.options.desiredBaseZoomLevel = Math.Max(0.75f, (float)zoom2 / 100f);
					Game1.forceSnapOnNextViewportUpdate = true;
					Program.gamePtr.refreshWindowSettings();
					Game1.playSound("drumkit6", null);
				}
			}
		}

		// Token: 0x060028B3 RID: 10419 RVA: 0x001DB84C File Offset: 0x001D9A4C
		public void gotGoldCoin(int amount)
		{
			this.goldCoinCounter += amount;
			this.goldCoinTimer = 4000;
			this.goldCoinString = "+" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.goldCoinCounter);
		}

		// Token: 0x060028B4 RID: 10420 RVA: 0x001DB89C File Offset: 0x001D9A9C
		public void pingQuest(Quest quest)
		{
			if (!quest.dailyQuest.Value)
			{
				return;
			}
			this.questNotificationTimer = 3000;
			this.questPingString = null;
			SlayMonsterQuest monsterQuest = quest as SlayMonsterQuest;
			if (monsterQuest == null)
			{
				ResourceCollectionQuest resourceQuest = quest as ResourceCollectionQuest;
				if (resourceQuest == null)
				{
					FishingQuest fishingQuest = quest as FishingQuest;
					if (fishingQuest == null)
					{
						SocializeQuest socializeQuest = quest as SocializeQuest;
						if (socializeQuest == null)
						{
							this.questNotificationTimer = 0;
						}
						else
						{
							this.questPingTexture = Game1.mouseCursors_1_6;
							this.questPingSourceRect = new Rectangle(298, 237, 12, 12);
							if (socializeQuest.whoToGreet.Count != 0)
							{
								this.questPingString = (socializeQuest.total.Value - socializeQuest.whoToGreet.Count).ToString() + "/" + socializeQuest.total.Value.ToString();
								return;
							}
						}
					}
					else
					{
						ParsedItemData data = ItemRegistry.GetData(fishingQuest.ItemId.Value);
						this.questPingTexture = data.GetTexture();
						this.questPingSourceRect = data.GetSourceRect(0, null);
						if (fishingQuest.numberFished.Value != fishingQuest.numberToFish.Value)
						{
							this.questPingString = fishingQuest.numberFished.Value.ToString() + "/" + fishingQuest.numberToFish.Value.ToString();
							return;
						}
					}
				}
				else
				{
					ParsedItemData data2 = ItemRegistry.GetData(resourceQuest.ItemId.Value);
					this.questPingTexture = data2.GetTexture();
					this.questPingSourceRect = data2.GetSourceRect(0, null);
					if (resourceQuest.numberCollected.Value != resourceQuest.number.Value)
					{
						this.questPingString = resourceQuest.numberCollected.Value.ToString() + "/" + resourceQuest.number.Value.ToString();
						return;
					}
				}
			}
			else
			{
				Monster value = monsterQuest.monster.Value;
				bool flag;
				if (value == null)
				{
					flag = (null != null);
				}
				else
				{
					AnimatedSprite sprite = value.Sprite;
					flag = (((sprite != null) ? sprite.Texture : null) != null);
				}
				if (!flag || !(monsterQuest.monsterName != null))
				{
					this.questNotificationTimer = 0;
					return;
				}
				this.questPingTexture = monsterQuest.monster.Value.Sprite.Texture;
				this.questPingSourceRect = new Rectangle(0, 5, 16, 16);
				if (monsterQuest.monsterName.Equals("Green Slime"))
				{
					this.questPingSourceRect = new Rectangle(0, 264, 16, 16);
				}
				else if (monsterQuest.monsterName.Value.Contains("Frost"))
				{
					this.questPingSourceRect = new Rectangle(16, 264, 16, 16);
				}
				else if (monsterQuest.monsterName.Value.Contains("Sludge"))
				{
					this.questPingSourceRect = new Rectangle(32, 264, 16, 16);
				}
				else if (monsterQuest.monsterName.Value.Equals("Dust Spirit"))
				{
					this.questPingSourceRect.Y = 9;
				}
				else if (monsterQuest.monsterName.Value.Contains("Crab"))
				{
					this.questPingSourceRect = new Rectangle(48, 106, 16, 16);
				}
				else if (monsterQuest.monsterName.Value.Contains("Duggy"))
				{
					this.questPingSourceRect = new Rectangle(0, 32, 16, 16);
				}
				else if (monsterQuest.monsterName.Equals("Squid Kid"))
				{
					this.questPingSourceRect = new Rectangle(0, 0, 16, 16);
				}
				if (monsterQuest.numberToKill.Value != monsterQuest.numberKilled.Value)
				{
					this.questPingString = monsterQuest.numberKilled.Value.ToString() + "/" + monsterQuest.numberToKill.Value.ToString();
					return;
				}
			}
		}

		// Token: 0x060028B5 RID: 10421 RVA: 0x001DBC83 File Offset: 0x001D9E83
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.updatePosition();
		}

		// Token: 0x060028B6 RID: 10422 RVA: 0x001DBC8C File Offset: 0x001D9E8C
		public override void performHoverAction(int x, int y)
		{
			this.updatePosition();
			if (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y))
			{
				this._hoverText.Clear();
				if (Game1.options.gamepadControls)
				{
					this._hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover_Console"));
				}
				else
				{
					this._hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover", Game1.options.journalButton[0].ToString()));
				}
			}
			if (Game1.options.zoomButtons)
			{
				if (this.zoomInButton.containsPoint(x, y))
				{
					this._hoverText.Clear();
					this._hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomInButton_Hover"));
					return;
				}
				if (this.zoomOutButton.containsPoint(x, y))
				{
					this._hoverText.Clear();
					this._hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomOutButton_Hover"));
				}
			}
		}

		// Token: 0x060028B7 RID: 10423 RVA: 0x001DBDA0 File Offset: 0x001D9FA0
		public void drawMoneyBox(SpriteBatch b, int overrideX = -1, int overrideY = -1)
		{
			this.updatePosition();
			b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? this.position.X : ((float)overrideX), (float)(overrideY - 172)) : this.position) + new Vector2((float)(28 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(172 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 65, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			this.moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? this.position.X : ((float)overrideX), (float)(overrideY - 172)) : this.position) + new Vector2((float)(68 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(196 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), Game1.player.Money);
			if (this.moneyShakeTimer > 0)
			{
				this.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
		}

		// Token: 0x060028B8 RID: 10424 RVA: 0x001DBF14 File Offset: 0x001DA114
		public override void update(GameTime time)
		{
			base.update(time);
			if (this._languageCode != LocalizedContentManager.CurrentLanguageCode)
			{
				this._languageCode = LocalizedContentManager.CurrentLanguageCode;
				this._amString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370");
				this._pmString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
			}
			if (this.questPingTimer > 0)
			{
				this.questPingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.questPingTimer < 0)
			{
				this.questPingTimer = 0;
			}
			if (this.questNotificationTimer > 0)
			{
				this.questNotificationTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.goldCoinTimer > 0)
			{
				this.goldCoinTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this.goldCoinTimer <= 0)
				{
					this.goldCoinCounter = 0;
				}
			}
			if (this.questsDirty)
			{
				if (Game1.player.hasPendingCompletedQuests)
				{
					this.PingQuestLog();
				}
				this.questsDirty = false;
			}
		}

		// Token: 0x060028B9 RID: 10425 RVA: 0x001DC01A File Offset: 0x001DA21A
		public virtual void PingQuestLog()
		{
			this.questPingTimer = 6000;
		}

		// Token: 0x060028BA RID: 10426 RVA: 0x001DC027 File Offset: 0x001DA227
		public virtual void DismissQuestPing()
		{
			this.questPingTimer = 0;
		}

		// Token: 0x060028BB RID: 10427 RVA: 0x001DC030 File Offset: 0x001DA230
		public override void draw(SpriteBatch b)
		{
			SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
			this.updatePosition();
			if (this.timeShakeTimer > 0)
			{
				this.timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (this.questPulseTimer > 0)
			{
				this.questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (this.whenToPulseTimer >= 0)
			{
				this.whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (this.whenToPulseTimer <= 0)
				{
					this.whenToPulseTimer = 3000;
					if (Game1.player.hasNewQuestActivity())
					{
						this.questPulseTimer = 1000;
					}
				}
			}
			b.Draw(Game1.mouseCursors, this.position, new Rectangle?(this.sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			if (Game1.dayOfMonth != this._lastDayOfMonth)
			{
				this._lastDayOfMonth = Game1.dayOfMonth;
				this._lastDayOfMonthString = Game1.shortDayDisplayNameFromDayOfSeason(this._lastDayOfMonth);
			}
			this._dateText.Clear();
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (currentLanguageCode != LocalizedContentManager.LanguageCode.ja)
			{
				if (currentLanguageCode != LocalizedContentManager.LanguageCode.zh)
				{
					if (currentLanguageCode != LocalizedContentManager.LanguageCode.mod)
					{
						this._dateText.Append(this._lastDayOfMonthString);
						this._dateText.Append(". ");
						this._dateText.AppendEx(Game1.dayOfMonth);
					}
					else
					{
						this._dateText.Append(LocalizedContentManager.CurrentModLanguage.ClockDateFormat.Replace("[DAY_OF_WEEK]", this._lastDayOfMonthString).Replace("[DAY_OF_MONTH]", Game1.dayOfMonth.ToString()));
					}
				}
				else
				{
					this._dateText.AppendEx(Game1.dayOfMonth);
					this._dateText.Append("日 ");
					this._dateText.Append(this._lastDayOfMonthString);
				}
			}
			else
			{
				this._dateText.AppendEx(Game1.dayOfMonth);
				this._dateText.Append("日 (");
				this._dateText.Append(this._lastDayOfMonthString);
				this._dateText.Append(")");
			}
			Vector2 daySize = font.MeasureString(this._dateText);
			Vector2 dayPosition = new Vector2((float)this.sourceRect.X * 0.5625f - daySize.X / 2f, (float)this.sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
			Utility.drawTextWithShadow(b, this._dateText, font, this.position + dayPosition, Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			b.Draw(Game1.mouseCursors, this.position + new Vector2(212f, 68f), new Rectangle?(new Rectangle(406, 441 + Game1.seasonIndex * 8, 12, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			if (Game1.weatherIcon == 999)
			{
				b.Draw(Game1.mouseCursors_1_6, this.position + new Vector2(116f, 68f), new Rectangle?(new Rectangle(243, 293, 12, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, this.position + new Vector2(116f, 68f), new Rectangle?(new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			this._padZeros.Clear();
			if (Game1.timeOfDay % 100 == 0)
			{
				this._padZeros.Append("0");
			}
			this._hours.Clear();
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.zh:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				this._temp.Clear();
				this._temp.AppendEx(Game1.timeOfDay / 100 % 24);
				if (Game1.timeOfDay / 100 % 24 <= 9)
				{
					this._hours.Append("0");
				}
				this._hours.AppendEx(this._temp);
				goto IL_50D;
			}
			if (Game1.timeOfDay / 100 % 12 == 0)
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
				{
					this._hours.Append("0");
				}
				else
				{
					this._hours.Append("12");
				}
			}
			else
			{
				this._hours.AppendEx(Game1.timeOfDay / 100 % 12);
			}
			IL_50D:
			this._timeText.Clear();
			this._timeText.AppendEx(this._hours);
			this._timeText.Append(":");
			this._timeText.AppendEx(Game1.timeOfDay % 100);
			this._timeText.AppendEx(this._padZeros);
			currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (currentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				if (currentLanguageCode != LocalizedContentManager.LanguageCode.ja)
				{
					switch (currentLanguageCode)
					{
					case LocalizedContentManager.LanguageCode.ko:
						if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
						{
							this._timeText.Append(this._amString);
							goto IL_711;
						}
						this._timeText.Append(this._pmString);
						goto IL_711;
					case LocalizedContentManager.LanguageCode.it:
						break;
					case LocalizedContentManager.LanguageCode.tr:
					case LocalizedContentManager.LanguageCode.hu:
						goto IL_711;
					case LocalizedContentManager.LanguageCode.mod:
						this._timeText.Clear();
						this._timeText.Append(LocalizedContentManager.FormatTimeString(Game1.timeOfDay, LocalizedContentManager.CurrentModLanguage.ClockTimeFormat));
						goto IL_711;
					default:
						goto IL_711;
					}
				}
				else
				{
					this._temp.Clear();
					this._temp.AppendEx(this._timeText);
					this._timeText.Clear();
					if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
					{
						this._timeText.Append(this._amString);
						this._timeText.Append(" ");
						this._timeText.AppendEx(this._temp);
						goto IL_711;
					}
					this._timeText.Append(this._pmString);
					this._timeText.Append(" ");
					this._timeText.AppendEx(this._temp);
					goto IL_711;
				}
			}
			this._timeText.Append(" ");
			if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
			{
				this._timeText.Append(this._amString);
			}
			else
			{
				this._timeText.Append(this._pmString);
			}
			IL_711:
			Vector2 txtSize = font.MeasureString(this._timeText);
			Vector2 timePosition = new Vector2((float)this.sourceRect.X * 0.55f - txtSize.X / 2f + (float)((this.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)this.sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((this.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
			bool nofade = Game1.shouldTimePass(false) || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
			Utility.drawTextWithShadow(b, this._timeText, font, this.position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)), 1f, -1f, -1, -1, 1f, 3);
			int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			if (Game1.player.hasVisibleQuests)
			{
				this.questButton.draw(b);
				if (this.questPulseTimer > 0)
				{
					float scaleMult = 1f / (Math.Max(300f, (float)Math.Abs(this.questPulseTimer % 1000 - 500)) / 500f);
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.questButton.bounds.X + 24), (float)(this.questButton.bounds.Y + 32)) + ((scaleMult > 1f) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
				}
				if (this.questPingTimer > 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16), (float)(Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8)), new Rectangle?(new Rectangle(128 + ((this.questPingTimer / 200 % 2 == 0) ? 0 : 16), 208, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
			}
			if (Game1.options.zoomButtons)
			{
				this.zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f, 0, 0, 0);
				this.zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f, 0, 0, 0);
			}
			this.drawMoneyBox(b, -1, -1);
			if (this._hoverText.Length > 0 && this.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				IClickableMenu.drawHoverText(b, this._hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			b.Draw(Game1.mouseCursors, this.position + new Vector2(88f, 88f), new Rectangle?(new Rectangle(324, 477, 7, 19)), Color.White, (float)(3.141592653589793 + Math.Min(3.141592653589793, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 600f) / 2000f) * 3.141592653589793)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
			if (this.questNotificationTimer > 0)
			{
				Vector2 basePosition = this.position + new Vector2(27f, 76f) * 4f;
				b.Draw(Game1.mouseCursors_1_6, basePosition, new Rectangle?(new Rectangle(257, 228, 39, 18)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				b.Draw(this.questPingTexture, basePosition + new Vector2(1f, 1f) * 4f, new Rectangle?(this.questPingSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
				if (this.questPingString != null)
				{
					Utility.drawTextWithShadow(b, this.questPingString, Game1.smallFont, basePosition + new Vector2(27f, 9.5f) * 4f - Game1.smallFont.MeasureString(this.questPingString) * 0.5f, Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
				else
				{
					b.Draw(Game1.mouseCursors_1_6, basePosition + new Vector2(22f, 5f) * 4f, new Rectangle?(new Rectangle(297, 229, 9, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
				}
			}
			if (this.goldCoinTimer > 0)
			{
				SpriteText.drawSmallTextBubble(b, this.goldCoinString, this.position + new Vector2(5f, 73f) * 4f, -1, 0.99f, true);
			}
		}

		// Token: 0x060028BC RID: 10428 RVA: 0x001DCD98 File Offset: 0x001DAF98
		private void updatePosition()
		{
			this.position = new Vector2((float)(Game1.uiViewport.Width - 300), 8f);
			if (Game1.isOutdoorMapSmallerThanViewport())
			{
				this.position = new Vector2(Math.Min(this.position.X, (float)(-(float)Game1.uiViewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300)), 8f);
			}
			Utility.makeSafe(ref this.position, 300, 284);
			this.xPositionOnScreen = (int)this.position.X;
			this.yPositionOnScreen = (int)this.position.Y;
			this.questButton.bounds = new Rectangle(this.xPositionOnScreen + 212, this.yPositionOnScreen + 240, 44, 46);
			this.zoomOutButton.bounds = new Rectangle(this.xPositionOnScreen + 92, this.yPositionOnScreen + 244, 28, 32);
			this.zoomInButton.bounds = new Rectangle(this.xPositionOnScreen + 124, this.yPositionOnScreen + 244, 28, 32);
		}

		// Token: 0x04001A62 RID: 6754
		public new const int width = 300;

		// Token: 0x04001A63 RID: 6755
		public new const int height = 284;

		// Token: 0x04001A64 RID: 6756
		public Vector2 position;

		// Token: 0x04001A65 RID: 6757
		private Rectangle sourceRect;

		// Token: 0x04001A66 RID: 6758
		public MoneyDial moneyDial = new MoneyDial(8, true);

		// Token: 0x04001A67 RID: 6759
		public int timeShakeTimer;

		// Token: 0x04001A68 RID: 6760
		public int moneyShakeTimer;

		// Token: 0x04001A69 RID: 6761
		public int questPulseTimer;

		// Token: 0x04001A6A RID: 6762
		public int whenToPulseTimer;

		// Token: 0x04001A6B RID: 6763
		public ClickableTextureComponent questButton;

		// Token: 0x04001A6C RID: 6764
		public ClickableTextureComponent zoomOutButton;

		// Token: 0x04001A6D RID: 6765
		public ClickableTextureComponent zoomInButton;

		// Token: 0x04001A6E RID: 6766
		private StringBuilder _hoverText = new StringBuilder();

		// Token: 0x04001A6F RID: 6767
		private StringBuilder _timeText = new StringBuilder();

		// Token: 0x04001A70 RID: 6768
		private StringBuilder _dateText = new StringBuilder();

		// Token: 0x04001A71 RID: 6769
		private StringBuilder _hours = new StringBuilder();

		// Token: 0x04001A72 RID: 6770
		private StringBuilder _padZeros = new StringBuilder();

		// Token: 0x04001A73 RID: 6771
		private StringBuilder _temp = new StringBuilder();

		// Token: 0x04001A74 RID: 6772
		private int _lastDayOfMonth = -1;

		// Token: 0x04001A75 RID: 6773
		private string _lastDayOfMonthString;

		// Token: 0x04001A76 RID: 6774
		private string _amString;

		// Token: 0x04001A77 RID: 6775
		private string _pmString;

		// Token: 0x04001A78 RID: 6776
		private int questNotificationTimer;

		// Token: 0x04001A79 RID: 6777
		private Texture2D questPingTexture;

		// Token: 0x04001A7A RID: 6778
		private Rectangle questPingSourceRect;

		// Token: 0x04001A7B RID: 6779
		private string questPingString;

		// Token: 0x04001A7C RID: 6780
		private int goldCoinCounter;

		// Token: 0x04001A7D RID: 6781
		private int goldCoinTimer;

		// Token: 0x04001A7E RID: 6782
		private string goldCoinString;

		// Token: 0x04001A7F RID: 6783
		private LocalizedContentManager.LanguageCode _languageCode = (LocalizedContentManager.LanguageCode)(-1);

		// Token: 0x04001A80 RID: 6784
		public bool questsDirty;

		// Token: 0x04001A81 RID: 6785
		public int questPingTimer;
	}
}
