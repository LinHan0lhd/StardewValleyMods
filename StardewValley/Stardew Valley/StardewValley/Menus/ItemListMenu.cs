using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x0200027D RID: 637
	public class ItemListMenu : IClickableMenu
	{
		// Token: 0x06002A20 RID: 10784 RVA: 0x001F7418 File Offset: 0x001F5618
		public ItemListMenu(string menuTitle, List<Item> itemList)
		{
			this.title = menuTitle;
			this.itemsToList = itemList;
			foreach (Item i in itemList)
			{
				this.totalValueOfItems += Utility.getSellToStorePriceOfItem(i, true);
			}
			this.itemsToList.Add(null);
			int centerX = Game1.uiViewport.Width / 2;
			int centerY = Game1.uiViewport.Height / 2;
			this.width = Math.Min(800, Game1.uiViewport.Width - 128);
			this.height = Math.Min(720, Game1.uiViewport.Height - 128);
			if (this.height <= 720)
			{
				this.itemsPerCategoryPage = 7;
			}
			this.xPositionOnScreen = centerX - this.width / 2;
			this.yPositionOnScreen = centerY - this.height / 2;
			Rectangle okRect = new Rectangle(centerX + this.width / 2 + 4, centerY + this.height / 2 - 96, 64, 64);
			this.okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), okRect, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f, false)
			{
				myID = 101,
				leftNeighborID = -7777
			};
			if (Game1.options.gamepadControls)
			{
				Game1.setMousePositionRaw(okRect.Center.X, okRect.Center.Y);
			}
			this.backButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + this.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 103,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width - 32 - 48, this.yPositionOnScreen + this.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 103,
				rightNeighborID = 101
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A21 RID: 10785 RVA: 0x001F76CC File Offset: 0x001F58CC
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A22 RID: 10786 RVA: 0x001F76E4 File Offset: 0x001F58E4
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID != 103 || direction != 1)
			{
				if (oldID == 101 && direction == 3)
				{
					if (this.showForwardButton())
					{
						this.currentlySnappedComponent = base.getComponentWithID(102);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
					if (this.showBackButton())
					{
						this.currentlySnappedComponent = base.getComponentWithID(103);
						this.snapCursorToCurrentSnappedComponent();
					}
				}
				return;
			}
			if (this.showForwardButton())
			{
				this.currentlySnappedComponent = base.getComponentWithID(102);
				this.snapCursorToCurrentSnappedComponent();
				return;
			}
			this.snapToDefaultClickableComponent();
		}

		// Token: 0x06002A23 RID: 10787 RVA: 0x001F7760 File Offset: 0x001F5960
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button != Buttons.B)
			{
				if (button != Buttons.RightTrigger)
				{
					if (button == Buttons.LeftTrigger && this.showBackButton())
					{
						this.currentTab--;
						Game1.playSound("shwip", null);
						return;
					}
				}
				else if (this.showForwardButton())
				{
					this.currentTab++;
					Game1.playSound("shwip", null);
					return;
				}
			}
			else
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x06002A24 RID: 10788 RVA: 0x001F77E9 File Offset: 0x001F59E9
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.okButton.tryHover(x, y, 0.1f);
			this.backButton.tryHover(x, y, 0.1f);
			this.forwardButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002A25 RID: 10789 RVA: 0x001F782C File Offset: 0x001F5A2C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.okButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
			}
			if (this.backButton.containsPoint(x, y))
			{
				if (this.currentTab != 0)
				{
					this.currentTab--;
				}
				Game1.playSound("shwip", null);
				return;
			}
			if (this.showForwardButton() && this.forwardButton.containsPoint(x, y))
			{
				this.currentTab++;
				Game1.playSound("shwip", null);
			}
		}

		// Token: 0x06002A26 RID: 10790 RVA: 0x001F78CC File Offset: 0x001F5ACC
		protected override void cleanupBeforeExit()
		{
			if (Game1.CurrentEvent != null)
			{
				Event currentEvent = Game1.CurrentEvent;
				int currentCommand = currentEvent.CurrentCommand;
				currentEvent.CurrentCommand = currentCommand + 1;
			}
		}

		// Token: 0x06002A27 RID: 10791 RVA: 0x001F78F4 File Offset: 0x001F5AF4
		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);
			SpriteText.drawStringHorizontallyCenteredAt(b, this.title, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + 32 + 12, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);
			Vector2 position = new Vector2((float)(this.xPositionOnScreen + 32), (float)(this.yPositionOnScreen + 96 + 4));
			for (int i = this.currentTab * this.itemsPerCategoryPage; i < this.currentTab * this.itemsPerCategoryPage + this.itemsPerCategoryPage; i++)
			{
				if (this.itemsToList.Count > i)
				{
					if (this.itemsToList[i] == null)
					{
						if (this.totalValueOfItems > 0)
						{
							SpriteText.drawString(b, Game1.content.LoadString("Strings\\UI:ItemList_ItemsLostValue", this.totalValueOfItems), (int)position.X + 64 + 12, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						}
					}
					else
					{
						this.itemsToList[i].drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw_OneInclusive);
						SpriteText.drawString(b, this.itemsToList[i].DisplayName, (int)position.X + 64 + 12, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
						position.Y += 68f;
					}
				}
			}
			if (this.showBackButton())
			{
				this.backButton.draw(b);
			}
			if (this.showForwardButton())
			{
				this.forwardButton.draw(b);
			}
			this.okButton.draw(b);
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b, false, -1);
		}

		// Token: 0x06002A28 RID: 10792 RVA: 0x001F7B18 File Offset: 0x001F5D18
		public bool showBackButton()
		{
			return this.currentTab > 0;
		}

		// Token: 0x06002A29 RID: 10793 RVA: 0x001F7B23 File Offset: 0x001F5D23
		public bool showForwardButton()
		{
			return this.itemsToList.Count > this.itemsPerCategoryPage * (this.currentTab + 1);
		}

		// Token: 0x04001BC0 RID: 7104
		public const int region_okbutton = 101;

		// Token: 0x04001BC1 RID: 7105
		public const int region_forwardButton = 102;

		// Token: 0x04001BC2 RID: 7106
		public const int region_backButton = 103;

		// Token: 0x04001BC3 RID: 7107
		public int itemsPerCategoryPage = 8;

		// Token: 0x04001BC4 RID: 7108
		public ClickableTextureComponent okButton;

		// Token: 0x04001BC5 RID: 7109
		public ClickableTextureComponent forwardButton;

		// Token: 0x04001BC6 RID: 7110
		public ClickableTextureComponent backButton;

		// Token: 0x04001BC7 RID: 7111
		private List<Item> itemsToList;

		// Token: 0x04001BC8 RID: 7112
		private string title;

		// Token: 0x04001BC9 RID: 7113
		private int currentTab;

		// Token: 0x04001BCA RID: 7114
		private int totalValueOfItems;
	}
}
