using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x0200027E RID: 638
	public class JojaCDMenu : IClickableMenu
	{
		// Token: 0x06002A2A RID: 10794 RVA: 0x001F7B44 File Offset: 0x001F5D44
		public JojaCDMenu(Texture2D noteTexture) : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 288, 1280, 576, true)
		{
			Game1.player.forceCanMove();
			this.noteTexture = noteTexture;
			int x = this.xPositionOnScreen + 4;
			int y = this.yPositionOnScreen + 208;
			for (int i = 0; i < 5; i++)
			{
				this.checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), i.ToString() ?? "")
				{
					myID = i,
					rightNeighborID = ((i % 2 != 0 || i == 4) ? -1 : (i + 1)),
					leftNeighborID = ((i % 2 == 0) ? -1 : (i - 1)),
					downNeighborID = i + 2,
					upNeighborID = i - 2
				});
				x += 592;
				if (x > this.xPositionOnScreen + 1184)
				{
					x = this.xPositionOnScreen + 4;
					y += 120;
				}
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccVault"))
			{
				this.checkboxes[0].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccBoilerRoom"))
			{
				this.checkboxes[1].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccCraftsRoom"))
			{
				this.checkboxes[2].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccPantry"))
			{
				this.checkboxes[3].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccFishTank"))
			{
				this.checkboxes[4].name = "complete";
			}
			this.exitFunction = new IClickableMenu.onExit(this.onExitFunction);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
				Game1.mouseCursorTransparency = 1f;
			}
		}

		// Token: 0x06002A2B RID: 10795 RVA: 0x001F7D41 File Offset: 0x001F5F41
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A2C RID: 10796 RVA: 0x001F7D56 File Offset: 0x001F5F56
		private void onExitFunction()
		{
			if (this.boughtSomething)
			{
				JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_JojaCDConfirm", false, false);
				Game1.drawDialogue(JojaMart.Morris);
			}
		}

		// Token: 0x06002A2D RID: 10797 RVA: 0x001F7D7C File Offset: 0x001F5F7C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.exitTimer >= 0)
			{
				return;
			}
			base.receiveLeftClick(x, y, true);
			foreach (ClickableComponent b in this.checkboxes)
			{
				if (b.containsPoint(x, y) && !b.name.Equals("complete"))
				{
					int buttonNumber = Convert.ToInt32(b.name);
					int price = this.getPriceFromButtonNumber(buttonNumber);
					if (Game1.player.Money >= price)
					{
						Game1.player.Money -= price;
						Game1.playSound("reward", null);
						b.name = "complete";
						this.boughtSomething = true;
						switch (buttonNumber)
						{
						case 0:
							Game1.addMailForTomorrow("jojaVault", true, true);
							Game1.addMailForTomorrow("ccVault", true, true);
							break;
						case 1:
							Game1.addMailForTomorrow("jojaBoilerRoom", true, true);
							Game1.addMailForTomorrow("ccBoilerRoom", true, true);
							break;
						case 2:
							Game1.addMailForTomorrow("jojaCraftsRoom", true, true);
							Game1.addMailForTomorrow("ccCraftsRoom", true, true);
							break;
						case 3:
							Game1.addMailForTomorrow("jojaPantry", true, true);
							Game1.addMailForTomorrow("ccPantry", true, true);
							break;
						case 4:
							Game1.addMailForTomorrow("jojaFishTank", true, true);
							Game1.addMailForTomorrow("ccFishTank", true, true);
							break;
						}
						this.exitTimer = 1000;
					}
					else
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					}
				}
			}
		}

		// Token: 0x06002A2E RID: 10798 RVA: 0x001F7F28 File Offset: 0x001F6128
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.exitTimer >= 0)
			{
				this.exitTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.exitTimer <= 0)
				{
					base.exitThisMenu(true);
				}
			}
			Game1.mouseCursorTransparency = 1f;
		}

		// Token: 0x06002A2F RID: 10799 RVA: 0x001F7F7A File Offset: 0x001F617A
		public int getPriceFromButtonNumber(int buttonNumber)
		{
			switch (buttonNumber)
			{
			case 0:
				return 40000;
			case 1:
				return 15000;
			case 2:
				return 25000;
			case 3:
				return 35000;
			case 4:
				return 20000;
			default:
				return -1;
			}
		}

		// Token: 0x06002A30 RID: 10800 RVA: 0x001F7FB7 File Offset: 0x001F61B7
		public string getDescriptionFromButtonNumber(int buttonNumber)
		{
			return Game1.content.LoadString("Strings\\UI:JojaCDMenu_Hover" + buttonNumber.ToString());
		}

		// Token: 0x06002A31 RID: 10801 RVA: 0x001F7FD4 File Offset: 0x001F61D4
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.hoverText = "";
			foreach (ClickableComponent b in this.checkboxes)
			{
				if (b.containsPoint(x, y))
				{
					this.hoverText = (b.name.Equals("complete") ? "" : Game1.parseText(this.getDescriptionFromButtonNumber(Convert.ToInt32(b.name)), Game1.dialogueFont, 384));
				}
			}
		}

		// Token: 0x06002A32 RID: 10802 RVA: 0x001F807C File Offset: 0x001F627C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - 288;
			int x = this.xPositionOnScreen + 4;
			int y = this.yPositionOnScreen + 208;
			this.checkboxes.Clear();
			for (int i = 0; i < 5; i++)
			{
				this.checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), i.ToString() ?? ""));
				x += 592;
				if (x > this.xPositionOnScreen + 1184)
				{
					x = this.xPositionOnScreen + 4;
					y += 120;
				}
			}
		}

		// Token: 0x06002A33 RID: 10803 RVA: 0x001F8144 File Offset: 0x001F6344
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			b.Draw(this.noteTexture, Utility.getTopLeftPositionForCenteringOnScreen(1280, 576, 0, 0), new Rectangle?(new Rectangle(0, 0, 320, 144)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.79f);
			base.draw(b);
			foreach (ClickableComponent c in this.checkboxes)
			{
				if (c.name.Equals("complete"))
				{
					b.Draw(this.noteTexture, new Vector2((float)(c.bounds.Left + 16), (float)(c.bounds.Y + 16)), new Rectangle?(new Rectangle(0, 144, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
				}
			}
			Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.uiViewport.Width - 300 - IClickableMenu.spaceToClearSideBorder * 2, 4);
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b, false, -1);
			if (!string.IsNullOrEmpty(this.hoverText))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001BCB RID: 7115
		public new const int width = 1280;

		// Token: 0x04001BCC RID: 7116
		public new const int height = 576;

		// Token: 0x04001BCD RID: 7117
		public const int buttonWidth = 147;

		// Token: 0x04001BCE RID: 7118
		public const int buttonHeight = 30;

		// Token: 0x04001BCF RID: 7119
		private Texture2D noteTexture;

		// Token: 0x04001BD0 RID: 7120
		public List<ClickableComponent> checkboxes = new List<ClickableComponent>();

		// Token: 0x04001BD1 RID: 7121
		private string hoverText;

		// Token: 0x04001BD2 RID: 7122
		private bool boughtSomething;

		// Token: 0x04001BD3 RID: 7123
		private int exitTimer = -1;
	}
}
