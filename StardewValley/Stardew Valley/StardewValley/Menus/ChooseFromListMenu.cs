using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;

namespace StardewValley.Menus
{
	// Token: 0x0200025C RID: 604
	public class ChooseFromListMenu : IClickableMenu
	{
		// Token: 0x06002824 RID: 10276 RVA: 0x001D3780 File Offset: 0x001D1980
		public ChooseFromListMenu(List<string> options, ChooseFromListMenu.actionOnChoosingListOption chooseAction, bool isJukebox = false, string default_selection = null) : base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height - 64 - 192, 640, 192, false)
		{
			this.chooseAction = chooseAction;
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen - 128 - 4, this.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 640 + 16 + 64, this.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = 103
			};
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width + 128 + 8, this.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f, false)
			{
				myID = 103,
				leftNeighborID = 102,
				rightNeighborID = 104
			};
			this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width + 192 + 12, this.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 104,
				leftNeighborID = 103
			};
			Game1.playSound("bigSelect", null);
			this.isJukebox = isJukebox;
			this.options = options;
			if (default_selection != null)
			{
				int default_index = options.IndexOf(default_selection);
				if (default_index >= 0)
				{
					this.index = default_index;
				}
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002825 RID: 10277 RVA: 0x001D39C9 File Offset: 0x001D1BC9
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(103);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002826 RID: 10278 RVA: 0x001D39E0 File Offset: 0x001D1BE0
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - 320;
			this.yPositionOnScreen = Game1.uiViewport.Height - 64 - 192;
			this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen - 128 - 4, this.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false);
			this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 640 + 16 + 64, this.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false);
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width + 128 + 8, this.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f, false);
			this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width + 192 + 12, this.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false);
		}

		// Token: 0x06002827 RID: 10279 RVA: 0x001D3B78 File Offset: 0x001D1D78
		public static void playSongAction(string s)
		{
			Game1.changeMusicTrack(s, false, MusicContext.Default);
		}

		// Token: 0x06002828 RID: 10280 RVA: 0x001D3B84 File Offset: 0x001D1D84
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.okButton.tryHover(x, y, 0.1f);
			this.cancelButton.tryHover(x, y, 0.1f);
			this.backButton.tryHover(x, y, 0.1f);
			this.forwardButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002829 RID: 10281 RVA: 0x001D3BE4 File Offset: 0x001D1DE4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.okButton.containsPoint(x, y) && this.chooseAction != null)
			{
				this.chooseAction(this.options[this.index]);
				Game1.playSound("select", null);
			}
			if (this.cancelButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
			}
			if (this.backButton.containsPoint(x, y))
			{
				this.index--;
				if (this.index < 0)
				{
					this.index = this.options.Count - 1;
				}
				this.backButton.scale = this.backButton.baseScale - 1f;
				Game1.playSound("shwip", null);
			}
			if (this.forwardButton.containsPoint(x, y))
			{
				this.index++;
				this.index %= this.options.Count;
				Game1.playSound("shwip", null);
				this.forwardButton.scale = this.forwardButton.baseScale - 1f;
			}
		}

		// Token: 0x0600282A RID: 10282 RVA: 0x001D3D28 File Offset: 0x001D1F28
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			string maxWidthJukeboxString = "Summer (The Sun Can Bend An Orange Sky)";
			int stringWidth = (int)Game1.dialogueFont.MeasureString(this.isJukebox ? maxWidthJukeboxString : this.options[this.index]).X;
			IClickableMenu.drawTextureBox(b, this.xPositionOnScreen + this.width / 2 - stringWidth / 2 - 16, this.yPositionOnScreen + 64 - 4, stringWidth + 32, 80, Color.White);
			if (this.index < this.options.Count)
			{
				Utility.drawTextWithShadow(b, this.isJukebox ? Utility.getSongTitleFromCueName(this.options[this.index]) : this.options[this.index], Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.dialogueFont.MeasureString(this.isJukebox ? Utility.getSongTitleFromCueName(this.options[this.index]) : this.options[this.index]).X / 2f, (float)(this.yPositionOnScreen + this.height / 2 - 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			}
			this.okButton.draw(b);
			this.cancelButton.draw(b);
			this.forwardButton.draw(b);
			this.backButton.draw(b);
			if (this.isJukebox)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:JukeboxMenu_Title"), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen - 32, "", 1f, null, 0, 0.88f, false);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x040019CB RID: 6603
		public const int region_backButton = 101;

		// Token: 0x040019CC RID: 6604
		public const int region_forwardButton = 102;

		// Token: 0x040019CD RID: 6605
		public const int region_okButton = 103;

		// Token: 0x040019CE RID: 6606
		public const int region_cancelButton = 104;

		// Token: 0x040019CF RID: 6607
		public const int w = 640;

		// Token: 0x040019D0 RID: 6608
		public const int h = 192;

		// Token: 0x040019D1 RID: 6609
		public ClickableTextureComponent backButton;

		// Token: 0x040019D2 RID: 6610
		public ClickableTextureComponent forwardButton;

		// Token: 0x040019D3 RID: 6611
		public ClickableTextureComponent okButton;

		// Token: 0x040019D4 RID: 6612
		public ClickableTextureComponent cancelButton;

		// Token: 0x040019D5 RID: 6613
		private List<string> options = new List<string>();

		// Token: 0x040019D6 RID: 6614
		private int index;

		// Token: 0x040019D7 RID: 6615
		private ChooseFromListMenu.actionOnChoosingListOption chooseAction;

		// Token: 0x040019D8 RID: 6616
		private bool isJukebox;

		// Token: 0x020005F6 RID: 1526
		// (Invoke) Token: 0x060043C3 RID: 17347
		public delegate void actionOnChoosingListOption(string s);
	}
}
