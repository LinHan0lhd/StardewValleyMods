using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace StardewValley.Menus
{
	// Token: 0x020002A1 RID: 673
	public class QuestLog : IClickableMenu
	{
		// Token: 0x06002BF0 RID: 11248 RVA: 0x0021755C File Offset: 0x0021575C
		public QuestLog() : base(0, 0, 0, 0, true)
		{
			Game1.dayTimeMoneyBox.DismissQuestPing();
			Game1.playSound("bigSelect", null);
			this.paginateQuests();
			this.width = 832;
			this.height = 576;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				this.height += 64;
			}
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
			this.xPositionOnScreen = (int)topLeft.X;
			this.yPositionOnScreen = (int)topLeft.Y + 32;
			this.questLogButtons = new List<ClickableComponent>();
			for (int i = 0; i < 6; i++)
			{
				this.questLogButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * ((this.height - 32) / 6), this.width - 32, (this.height - 32) / 6 + 4), i.ToString() ?? "")
				{
					myID = i,
					downNeighborID = -7777,
					upNeighborID = ((i > 0) ? (i - 1) : -1),
					rightNeighborID = -7777,
					leftNeighborID = -7777,
					fullyImmutable = true
				});
			}
			this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 20, this.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 102,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 64 - 48, this.yPositionOnScreen + this.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 101
			};
			this.rewardBox = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 80, this.yPositionOnScreen + this.height - 32 - 96, 96, 96), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f, true)
			{
				myID = 103
			};
			this.cancelQuestButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 4, this.yPositionOnScreen + this.height + 4, 48, 48), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 4f, true)
			{
				myID = 104
			};
			int scrollbar_x = this.xPositionOnScreen + this.width + 16;
			this.upArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, this.yPositionOnScreen + 96, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
			this.downArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
			this.scrollBarBounds = default(Rectangle);
			this.scrollBarBounds.X = this.upArrow.bounds.X + 12;
			this.scrollBarBounds.Width = 24;
			this.scrollBarBounds.Y = this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4;
			this.scrollBarBounds.Height = this.downArrow.bounds.Y - 4 - this.scrollBarBounds.Y;
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.scrollBarBounds.X, this.scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002BF1 RID: 11249 RVA: 0x002179F4 File Offset: 0x00215BF4
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID >= 0 && oldID < 6 && this.questPage == -1)
			{
				switch (direction)
				{
				case 1:
					if (this.currentPage < this.pages.Count - 1)
					{
						this.currentlySnappedComponent = base.getComponentWithID(101);
						this.currentlySnappedComponent.leftNeighborID = oldID;
					}
					break;
				case 2:
					if (oldID < 5 && this.pages[this.currentPage].Count - 1 > oldID)
					{
						this.currentlySnappedComponent = base.getComponentWithID(oldID + 1);
					}
					break;
				case 3:
					if (this.currentPage > 0)
					{
						this.currentlySnappedComponent = base.getComponentWithID(102);
						this.currentlySnappedComponent.rightNeighborID = oldID;
					}
					break;
				}
			}
			else if (oldID == 102)
			{
				if (this.questPage != -1)
				{
					return;
				}
				this.currentlySnappedComponent = base.getComponentWithID(0);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002BF2 RID: 11250 RVA: 0x00217AE4 File Offset: 0x00215CE4
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002BF3 RID: 11251 RVA: 0x00217AFC File Offset: 0x00215CFC
		public override void receiveGamePadButton(Buttons button)
		{
			if (button != Buttons.RightTrigger)
			{
				if (button != Buttons.LeftTrigger)
				{
					return;
				}
				if (this.questPage == -1 && this.currentPage > 0)
				{
					this.nonQuestPageBackButton();
				}
			}
			else if (this.questPage == -1 && this.currentPage < this.pages.Count - 1)
			{
				this.nonQuestPageForwardButton();
				return;
			}
		}

		// Token: 0x06002BF4 RID: 11252 RVA: 0x00217B58 File Offset: 0x00215D58
		protected virtual void paginateQuests()
		{
			this.pages = new List<List<IQuest>>();
			IList<IQuest> quests = this.GetAllQuests();
			int startIndex = 0;
			while (startIndex < quests.Count)
			{
				List<IQuest> page = new List<IQuest>();
				int i = 0;
				while (i < 6 && startIndex < quests.Count)
				{
					page.Add(quests[startIndex]);
					startIndex++;
					i++;
				}
				this.pages.Add(page);
			}
			if (this.pages.Count == 0)
			{
				this.pages.Add(new List<IQuest>());
			}
			this.currentPage = Utility.Clamp(this.currentPage, 0, this.pages.Count - 1);
			this.questPage = -1;
		}

		// Token: 0x06002BF5 RID: 11253 RVA: 0x00217C00 File Offset: 0x00215E00
		protected virtual IList<IQuest> GetAllQuests()
		{
			List<IQuest> quests = new List<IQuest>();
			for (int i = Game1.player.team.specialOrders.Count - 1; i >= 0; i--)
			{
				SpecialOrder order = Game1.player.team.specialOrders[i];
				if (!order.IsHidden())
				{
					quests.Add(order);
				}
			}
			for (int j = Game1.player.questLog.Count - 1; j >= 0; j--)
			{
				Quest quest = Game1.player.questLog[j];
				if (quest == null || quest.destroy.Value)
				{
					Game1.player.questLog.RemoveAt(j);
				}
				else if (!quest.IsHidden())
				{
					quests.Add(quest);
				}
			}
			return quests;
		}

		// Token: 0x06002BF6 RID: 11254 RVA: 0x00217CBD File Offset: 0x00215EBD
		public bool NeedsScroll()
		{
			return (this._shownQuest == null || !this._shownQuest.ShouldDisplayAsComplete()) && this.questPage != -1 && this._contentHeight > this._scissorRectHeight;
		}

		// Token: 0x06002BF7 RID: 11255 RVA: 0x00217CF0 File Offset: 0x00215EF0
		public override void receiveScrollWheelAction(int direction)
		{
			if (this.NeedsScroll())
			{
				float new_scroll = this.scrollAmount - (float)(Math.Sign(direction) * 64 / 2);
				if (new_scroll < 0f)
				{
					new_scroll = 0f;
				}
				if (new_scroll > this._contentHeight - this._scissorRectHeight)
				{
					new_scroll = this._contentHeight - this._scissorRectHeight;
				}
				if (this.scrollAmount != new_scroll)
				{
					this.scrollAmount = new_scroll;
					Game1.playSound("shiny4", null);
					this.SetScrollBarFromAmount();
				}
			}
			base.receiveScrollWheelAction(direction);
		}

		// Token: 0x06002BF8 RID: 11256 RVA: 0x00217D78 File Offset: 0x00215F78
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			base.performHoverAction(x, y);
			if (this.questPage == -1)
			{
				for (int i = 0; i < this.questLogButtons.Count; i++)
				{
					if (this.pages.Count > 0 && this.pages[0].Count > i && this.questLogButtons[i].containsPoint(x, y) && !this.questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
					{
						Game1.playSound("Cowboy_gunshot", null);
					}
				}
			}
			else if (this._shownQuest.CanBeCancelled() && this.cancelQuestButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11364");
			}
			this.forwardButton.tryHover(x, y, 0.2f);
			this.backButton.tryHover(x, y, 0.2f);
			this.cancelQuestButton.tryHover(x, y, 0.2f);
			if (this.NeedsScroll())
			{
				this.upArrow.tryHover(x, y, 0.1f);
				this.downArrow.tryHover(x, y, 0.1f);
				this.scrollBar.tryHover(x, y, 0.1f);
			}
		}

		// Token: 0x06002BF9 RID: 11257 RVA: 0x00217ECC File Offset: 0x002160CC
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.isAnyGamePadButtonBeingPressed() && this.questPage != -1 && Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				this.exitQuestPage();
			}
			else
			{
				base.receiveKeyPress(key);
			}
			if (Game1.options.doesInputListContain(Game1.options.journalButton, key) && this.readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect", null);
			}
		}

		// Token: 0x06002BFA RID: 11258 RVA: 0x00217F48 File Offset: 0x00216148
		private void nonQuestPageForwardButton()
		{
			this.currentPage++;
			Game1.playSound("shwip", null);
			if (Game1.options.SnappyMenus && this.currentPage == this.pages.Count - 1)
			{
				this.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002BFB RID: 11259 RVA: 0x00217FAC File Offset: 0x002161AC
		private void nonQuestPageBackButton()
		{
			this.currentPage--;
			Game1.playSound("shwip", null);
			if (Game1.options.SnappyMenus && this.currentPage == 0)
			{
				this.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x00218002 File Offset: 0x00216202
		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				this.SetScrollFromY(y);
			}
		}

		// Token: 0x06002BFD RID: 11261 RVA: 0x00218023 File Offset: 0x00216223
		public override void releaseLeftClick(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002BFE RID: 11262 RVA: 0x0021803C File Offset: 0x0021623C
		public virtual void SetScrollFromY(int y)
		{
			int y2 = this.scrollBar.bounds.Y;
			float percentage = (float)(y - this.scrollBarBounds.Y) / (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height);
			percentage = Utility.Clamp(percentage, 0f, 1f);
			this.scrollAmount = percentage * (this._contentHeight - this._scissorRectHeight);
			this.SetScrollBarFromAmount();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002BFF RID: 11263 RVA: 0x002180D8 File Offset: 0x002162D8
		public void UpArrowPressed()
		{
			this.upArrow.scale = this.upArrow.baseScale;
			this.scrollAmount -= 64f;
			if (this.scrollAmount < 0f)
			{
				this.scrollAmount = 0f;
			}
			this.SetScrollBarFromAmount();
		}

		// Token: 0x06002C00 RID: 11264 RVA: 0x0021812C File Offset: 0x0021632C
		public void DownArrowPressed()
		{
			this.downArrow.scale = this.downArrow.baseScale;
			this.scrollAmount += 64f;
			if (this.scrollAmount > this._contentHeight - this._scissorRectHeight)
			{
				this.scrollAmount = this._contentHeight - this._scissorRectHeight;
			}
			this.SetScrollBarFromAmount();
		}

		// Token: 0x06002C01 RID: 11265 RVA: 0x00218190 File Offset: 0x00216390
		private void SetScrollBarFromAmount()
		{
			if (!this.NeedsScroll())
			{
				this.scrollAmount = 0f;
				return;
			}
			if (this.scrollAmount < 8f)
			{
				this.scrollAmount = 0f;
			}
			if (this.scrollAmount > this._contentHeight - this._scissorRectHeight - 8f)
			{
				this.scrollAmount = this._contentHeight - this._scissorRectHeight;
			}
			this.scrollBar.bounds.Y = (int)((float)this.scrollBarBounds.Y + (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height) / Math.Max(1f, this._contentHeight - this._scissorRectHeight) * this.scrollAmount);
		}

		// Token: 0x06002C02 RID: 11266 RVA: 0x00218251 File Offset: 0x00216451
		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (this.NeedsScroll())
			{
				if (direction == 0)
				{
					this.UpArrowPressed();
					return;
				}
				if (direction != 2)
				{
					return;
				}
				this.DownArrowPressed();
			}
		}

		// Token: 0x06002C03 RID: 11267 RVA: 0x00218278 File Offset: 0x00216478
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			if (this.questPage != -1)
			{
				Quest quest = this._shownQuest as Quest;
				int yOffset = (this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0 && SpriteText.getWidthOfString(this._shownQuest.GetName(), 999999) > this.width / 2) ? -48 : 0;
				if (this.questPage != -1 && this._shownQuest.ShouldDisplayAsComplete() && this._shownQuest.HasMoneyReward() && this.rewardBox.containsPoint(x, y + yOffset))
				{
					Game1.player.Money += this._shownQuest.GetMoneyReward();
					Game1.playSound("purchaseRepeat", null);
					this._shownQuest.OnMoneyRewardClaimed();
				}
				else if (this.questPage != -1 && quest != null && !quest.completed.Value && quest.canBeCancelled.Value && this.cancelQuestButton.containsPoint(x, y))
				{
					quest.accepted.Value = false;
					if (quest.dailyQuest.Value && quest.dayQuestAccepted.Value == Game1.Date.TotalDays)
					{
						Game1.player.acceptedDailyQuest.Set(false);
					}
					Game1.player.questLog.Remove(quest);
					this.pages[this.currentPage].RemoveAt(this.questPage);
					this.questPage = -1;
					Game1.playSound("trashcan", null);
					if (Game1.options.SnappyMenus && this.currentPage == 0)
					{
						this.currentlySnappedComponent = base.getComponentWithID(0);
						this.snapCursorToCurrentSnappedComponent();
					}
				}
				else if (!this.NeedsScroll() || this.backButton.containsPoint(x, y))
				{
					this.exitQuestPage();
				}
				if (this.NeedsScroll())
				{
					if (this.downArrow.containsPoint(x, y) && this.scrollAmount < this._contentHeight - this._scissorRectHeight)
					{
						this.DownArrowPressed();
						Game1.playSound("shwip", null);
						return;
					}
					if (this.upArrow.containsPoint(x, y) && this.scrollAmount > 0f)
					{
						this.UpArrowPressed();
						Game1.playSound("shwip", null);
						return;
					}
					if (this.scrollBar.containsPoint(x, y))
					{
						this.scrolling = true;
						return;
					}
					if (this.scrollBarBounds.Contains(x, y))
					{
						this.scrolling = true;
						return;
					}
					if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
					{
						this.scrolling = true;
						this.leftClickHeld(x, y);
						this.releaseLeftClick(x, y);
					}
				}
				return;
			}
			for (int i = 0; i < this.questLogButtons.Count; i++)
			{
				if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i && this.questLogButtons[i].containsPoint(x, y))
				{
					Game1.playSound("smallSelect", null);
					this.questPage = i;
					this._shownQuest = this.pages[this.currentPage][i];
					this._objectiveText = this._shownQuest.GetObjectiveDescriptions();
					this._shownQuest.MarkAsViewed();
					this.scrollAmount = 0f;
					this.SetScrollBarFromAmount();
					if (Game1.options.SnappyMenus)
					{
						this.currentlySnappedComponent = base.getComponentWithID(102);
						this.currentlySnappedComponent.rightNeighborID = -7777;
						this.currentlySnappedComponent.downNeighborID = (this.HasMoneyReward() ? 103 : (this._shownQuest.CanBeCancelled() ? 104 : -1));
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
			}
			if (this.currentPage < this.pages.Count - 1 && this.forwardButton.containsPoint(x, y))
			{
				this.nonQuestPageForwardButton();
				return;
			}
			if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
			{
				this.nonQuestPageBackButton();
				return;
			}
			Game1.playSound("bigDeSelect", null);
			base.exitThisMenu(true);
		}

		// Token: 0x06002C04 RID: 11268 RVA: 0x0021870E File Offset: 0x0021690E
		public bool HasReward()
		{
			return this._shownQuest.HasReward();
		}

		// Token: 0x06002C05 RID: 11269 RVA: 0x0021871B File Offset: 0x0021691B
		public bool HasMoneyReward()
		{
			return this._shownQuest.HasMoneyReward();
		}

		// Token: 0x06002C06 RID: 11270 RVA: 0x00218728 File Offset: 0x00216928
		public void exitQuestPage()
		{
			if (this._shownQuest.OnLeaveQuestPage())
			{
				this.pages[this.currentPage].RemoveAt(this.questPage);
			}
			this.questPage = -1;
			this.paginateQuests();
			Game1.playSound("shwip", null);
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002C07 RID: 11271 RVA: 0x00218791 File Offset: 0x00216991
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.questPage != -1 && this.HasReward())
			{
				this.rewardBox.scale = this.rewardBox.baseScale + Game1.dialogueButtonScale / 20f;
			}
		}

		// Token: 0x06002C08 RID: 11272 RVA: 0x002187D0 File Offset: 0x002169D0
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen - 64, "", 1f, null, 0, 0.88f, false);
			if (this.questPage == -1)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White, 4f, true, -1f);
				for (int i = 0; i < this.questLogButtons.Count; i++)
				{
					if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.questLogButtons[i].bounds.X, this.questLogButtons[i].bounds.Y, this.questLogButtons[i].bounds.Width, this.questLogButtons[i].bounds.Height, this.questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, false, -1f);
						if (this.pages[this.currentPage][i].ShouldDisplayAsNew() || this.pages[this.currentPage][i].ShouldDisplayAsComplete())
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.questLogButtons[i].bounds.X + 64 + 4), (float)(this.questLogButtons[i].bounds.Y + 44)), new Rectangle(this.pages[this.currentPage][i].ShouldDisplayAsComplete() ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, false, 0.99f, -1, -1, 0.35f);
						}
						else
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.questLogButtons[i].bounds.X + 32), (float)(this.questLogButtons[i].bounds.Y + 28)), this.pages[this.currentPage][i].IsTimedQuest() ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (this.pages[this.currentPage][i].IsTimedQuest() ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, 0.35f);
						}
						this.pages[this.currentPage][i].IsTimedQuest();
						SpriteText.drawString(b, this.pages[this.currentPage][i].GetName(), this.questLogButtons[i].bounds.X + 128 + 4, this.questLogButtons[i].bounds.Y + 20, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					}
				}
			}
			else
			{
				int titleWidth = SpriteText.getWidthOfString(this._shownQuest.GetName(), 999999);
				if (titleWidth > this.width / 2)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height + (this._shownQuest.ShouldDisplayAsComplete() ? 48 : 0), Color.White, 4f, true, -1f);
					SpriteText.drawStringHorizontallyCenteredAt(b, this._shownQuest.GetName(), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + 32, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
				}
				else
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White, 4f, true, -1f);
					SpriteText.drawStringHorizontallyCenteredAt(b, this._shownQuest.GetName(), this.xPositionOnScreen + this.width / 2 + ((this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0) ? (Math.Max(32, SpriteText.getWidthOfString(this._shownQuest.GetName(), 999999) / 3) - 32) : 0), this.yPositionOnScreen + 32, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
				}
				float extraYOffset = 0f;
				if (this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0)
				{
					int xOffset = 0;
					if (titleWidth > this.width / 2)
					{
						xOffset = 28;
						extraYOffset = 48f;
					}
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + xOffset + 32), (float)(this.yPositionOnScreen + 48 - 8) + extraYOffset), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.parseText((this.pages[this.currentPage][this.questPage].GetDaysLeft() > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", this.pages[this.currentPage][this.questPage].GetDaysLeft()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest_FinalDay"), Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + xOffset + 80), (float)(this.yPositionOnScreen + 48 - 8) + extraYOffset), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
				string description = Game1.parseText(this._shownQuest.GetDescription(), Game1.dialogueFont, this.width - 128);
				Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
				Vector2 description_size = Game1.dialogueFont.MeasureString(description);
				Rectangle scissor_rect = default(Rectangle);
				scissor_rect.X = this.xPositionOnScreen + 32;
				scissor_rect.Y = this.yPositionOnScreen + 96 + (int)extraYOffset;
				scissor_rect.Height = this.yPositionOnScreen + this.height - 32 - scissor_rect.Y;
				scissor_rect.Width = this.width - 64;
				this._scissorRectHeight = (float)scissor_rect.Height;
				scissor_rect = Utility.ConstrainScissorRectToScreen(scissor_rect);
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState
				{
					ScissorTestEnable = true
				}, null, null);
				Game1.graphics.GraphicsDevice.ScissorRectangle = scissor_rect;
				Utility.drawTextWithShadow(b, description, Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + 64), (float)this.yPositionOnScreen - this.scrollAmount + 96f + extraYOffset), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				float yPos = (float)(this.yPositionOnScreen + 96) + description_size.Y + 32f - this.scrollAmount + extraYOffset;
				if (this._shownQuest.ShouldDisplayAsComplete())
				{
					b.End();
					b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376"), this.xPositionOnScreen + 32 + 4, this.rewardBox.bounds.Y + 21 + 4 + (int)extraYOffset, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					this.rewardBox.draw(b, Color.White, 0.9f, 0, 0, (int)extraYOffset);
					if (this.HasMoneyReward())
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(this.rewardBox.bounds.X + 16), (float)(this.rewardBox.bounds.Y + 16) - Game1.dialogueButtonScale / 2f + extraYOffset), new Rectangle?(new Rectangle(280, 410, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this._shownQuest.GetMoneyReward()), this.xPositionOnScreen + 448, this.rewardBox.bounds.Y + 21 + 4 + (int)extraYOffset, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					}
				}
				else
				{
					for (int j = 0; j < this._objectiveText.Count; j++)
					{
						string text = this._objectiveText[j];
						int text_width = this.width - 192;
						string parsed_text = Game1.parseText(text, Game1.dialogueFont, text_width);
						SpecialOrder o = this._shownQuest as SpecialOrder;
						bool flag = o != null && o.objectives[j].IsComplete();
						Color text_color = Game1.unselectedOptionColor;
						if (!flag)
						{
							text_color = Color.DarkBlue;
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 96) + 8f * Game1.dialogueButtonScale / 10f, yPos), new Rectangle(412, 495, 5, 4), Color.White, 1.5707964f, Vector2.Zero, -1f, false, -1f, -1, -1, 0.35f);
						}
						Utility.drawTextWithShadow(b, parsed_text, Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + 128), yPos - 8f), text_color, 1f, -1f, -1, -1, 1f, 3);
						yPos += Game1.dialogueFont.MeasureString(parsed_text).Y;
						SpecialOrder order = this._shownQuest as SpecialOrder;
						if (order != null)
						{
							OrderObjective order_objective = order.objectives[j];
							if (order_objective.GetMaxCount() > 1 && order_objective.ShouldShowProgress())
							{
								Color dark_bar_color = Color.DarkRed;
								Color bar_color = Color.Red;
								if (order_objective.GetCount() >= order_objective.GetMaxCount())
								{
									bar_color = Color.LimeGreen;
									dark_bar_color = Color.Green;
								}
								int inset = 64;
								int objective_count_draw_width = 160;
								int notches = 4;
								Rectangle bar_background_source = new Rectangle(0, 224, 47, 12);
								Rectangle bar_notch_source = new Rectangle(47, 224, 1, 12);
								int bar_horizontal_padding = 3;
								int bar_vertical_padding = 3;
								int slice_width = 5;
								string objective_count_text = order_objective.GetCount().ToString() + "/" + order_objective.GetMaxCount().ToString();
								int max_text_width = (int)Game1.dialogueFont.MeasureString(order_objective.GetMaxCount().ToString() + "/" + order_objective.GetMaxCount().ToString()).X;
								int count_text_width = (int)Game1.dialogueFont.MeasureString(objective_count_text).X;
								int text_draw_position = this.xPositionOnScreen + this.width - inset - count_text_width;
								int max_text_draw_position = this.xPositionOnScreen + this.width - inset - max_text_width;
								Utility.drawTextWithShadow(b, objective_count_text, Game1.dialogueFont, new Vector2((float)text_draw_position, yPos), Color.DarkBlue, 1f, -1f, -1, -1, 1f, 3);
								Rectangle bar_draw_position = new Rectangle(this.xPositionOnScreen + inset, (int)yPos, this.width - inset * 2 - objective_count_draw_width, bar_background_source.Height * 4);
								if (bar_draw_position.Right > max_text_draw_position - 16)
								{
									int adjustment = bar_draw_position.Right - (max_text_draw_position - 16);
									bar_draw_position.Width -= adjustment;
								}
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle?(new Rectangle(bar_background_source.X, bar_background_source.Y, slice_width, bar_background_source.Height)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X + slice_width * 4, bar_draw_position.Y, bar_draw_position.Width - 2 * slice_width * 4, bar_draw_position.Height), new Rectangle?(new Rectangle(bar_background_source.X + slice_width, bar_background_source.Y, bar_background_source.Width - 2 * slice_width, bar_background_source.Height)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.Right - slice_width * 4, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle?(new Rectangle(bar_background_source.Right - slice_width, bar_background_source.Y, slice_width, bar_background_source.Height)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								float quest_progress = (float)order_objective.GetCount() / (float)order_objective.GetMaxCount();
								if (order_objective.GetMaxCount() < notches)
								{
									notches = order_objective.GetMaxCount();
								}
								bar_draw_position.X += 4 * bar_horizontal_padding;
								bar_draw_position.Width -= 4 * bar_horizontal_padding * 2;
								for (int k = 1; k < notches; k++)
								{
									b.Draw(Game1.mouseCursors2, new Vector2((float)bar_draw_position.X + (float)bar_draw_position.Width * ((float)k / (float)notches), (float)bar_draw_position.Y), new Rectangle?(bar_notch_source), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
								}
								bar_draw_position.Y += 4 * bar_vertical_padding;
								bar_draw_position.Height -= 4 * bar_vertical_padding * 2;
								Rectangle rect = new Rectangle(bar_draw_position.X, bar_draw_position.Y, (int)((float)bar_draw_position.Width * quest_progress) - 4, bar_draw_position.Height);
								b.Draw(Game1.staminaRect, rect, null, bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
								rect.X = rect.Right;
								rect.Width = 4;
								b.Draw(Game1.staminaRect, rect, null, dark_bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
								yPos += (float)((bar_background_source.Height + 4) * 4);
							}
						}
						this._contentHeight = yPos + this.scrollAmount - (float)scissor_rect.Y;
					}
					b.End();
					b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					if (this._shownQuest.CanBeCancelled())
					{
						this.cancelQuestButton.draw(b);
					}
					if (this.NeedsScroll())
					{
						if (this.scrollAmount > 0f)
						{
							b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Top, scissor_rect.Width, 4), Color.Black * 0.15f);
						}
						if (this.scrollAmount < this._contentHeight - this._scissorRectHeight)
						{
							b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Bottom - 4, scissor_rect.Width, 4), Color.Black * 0.15f);
						}
					}
				}
			}
			if (this.NeedsScroll())
			{
				this.upArrow.draw(b);
				this.downArrow.draw(b);
				this.scrollBar.draw(b);
			}
			if (this.currentPage < this.pages.Count - 1 && this.questPage == -1)
			{
				this.forwardButton.draw(b);
			}
			if (this.currentPage > 0 || this.questPage != -1)
			{
				this.backButton.draw(b);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b, false, -1);
			if (this.hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001DA6 RID: 7590
		public const int questsPerPage = 6;

		// Token: 0x04001DA7 RID: 7591
		public const int region_forwardButton = 101;

		// Token: 0x04001DA8 RID: 7592
		public const int region_backButton = 102;

		// Token: 0x04001DA9 RID: 7593
		public const int region_rewardBox = 103;

		// Token: 0x04001DAA RID: 7594
		public const int region_cancelQuestButton = 104;

		// Token: 0x04001DAB RID: 7595
		protected List<List<IQuest>> pages;

		// Token: 0x04001DAC RID: 7596
		public List<ClickableComponent> questLogButtons;

		// Token: 0x04001DAD RID: 7597
		protected int currentPage;

		// Token: 0x04001DAE RID: 7598
		protected int questPage = -1;

		// Token: 0x04001DAF RID: 7599
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001DB0 RID: 7600
		public ClickableTextureComponent backButton;

		// Token: 0x04001DB1 RID: 7601
		public ClickableTextureComponent rewardBox;

		// Token: 0x04001DB2 RID: 7602
		public ClickableTextureComponent cancelQuestButton;

		// Token: 0x04001DB3 RID: 7603
		protected IQuest _shownQuest;

		// Token: 0x04001DB4 RID: 7604
		protected List<string> _objectiveText;

		// Token: 0x04001DB5 RID: 7605
		protected float _contentHeight;

		// Token: 0x04001DB6 RID: 7606
		protected float _scissorRectHeight;

		// Token: 0x04001DB7 RID: 7607
		public float scrollAmount;

		// Token: 0x04001DB8 RID: 7608
		public ClickableTextureComponent upArrow;

		// Token: 0x04001DB9 RID: 7609
		public ClickableTextureComponent downArrow;

		// Token: 0x04001DBA RID: 7610
		public ClickableTextureComponent scrollBar;

		// Token: 0x04001DBB RID: 7611
		protected bool scrolling;

		// Token: 0x04001DBC RID: 7612
		public Rectangle scrollBarBounds;

		// Token: 0x04001DBD RID: 7613
		private string hoverText = "";
	}
}
