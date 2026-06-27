using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x0200028C RID: 652
	public class NamingMenu : IClickableMenu
	{
		// Token: 0x06002B20 RID: 11040 RVA: 0x0020A288 File Offset: 0x00208488
		public NamingMenu(NamingMenu.doneNamingBehavior b, string title, string defaultName = null)
		{
			this.doneNaming = b;
			this.xPositionOnScreen = 0;
			this.yPositionOnScreen = 0;
			this.width = Game1.uiViewport.Width;
			this.height = Game1.uiViewport.Height;
			this.title = title;
			this.randomButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f, false);
			this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			this.textBox.X = Game1.uiViewport.Width / 2 - 192;
			this.textBox.Y = Game1.uiViewport.Height / 2;
			this.textBox.Width = 256;
			this.textBox.Height = 192;
			this.textBox.OnEnterPressed += this.textBoxEnter;
			Game1.keyboardDispatcher.Subscriber = this.textBox;
			this.textBox.Text = ((defaultName != null) ? defaultName : Dialogue.randomName());
			this.textBox.Selected = true;
			this.randomButton = new ClickableTextureComponent(new Rectangle(this.textBox.X + this.textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f, false)
			{
				myID = 103,
				leftNeighborID = 102
			};
			this.doneNamingButton = new ClickableTextureComponent(new Rectangle(this.textBox.X + this.textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 102,
				rightNeighborID = 103,
				leftNeighborID = 104
			};
			this.textBoxCC = new ClickableComponent(new Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = 102
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002B21 RID: 11041 RVA: 0x0020A522 File Offset: 0x00208722
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(104);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B22 RID: 11042 RVA: 0x0020A538 File Offset: 0x00208738
		public void textBoxEnter(TextBox sender)
		{
			if (sender.Text.Length >= this.minLength)
			{
				if (this.doneNaming != null)
				{
					string text = this.FilterInput ? Utility.FilterDirtyWords(sender.Text) : sender.Text;
					this.doneNaming(text);
					this.textBox.Selected = false;
					return;
				}
				Game1.exitActiveMenu();
			}
		}

		// Token: 0x06002B23 RID: 11043 RVA: 0x0020A59C File Offset: 0x0020879C
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (this.textBox.Selected)
			{
				if (button <= Buttons.DPadRight)
				{
					if (button - Buttons.DPadUp > 1 && button != Buttons.DPadLeft && button != Buttons.DPadRight)
					{
						return;
					}
				}
				else if (button <= Buttons.LeftThumbstickUp)
				{
					if (button != Buttons.LeftThumbstickLeft && button != Buttons.LeftThumbstickUp)
					{
						return;
					}
				}
				else if (button != Buttons.LeftThumbstickDown && button != Buttons.LeftThumbstickRight)
				{
					return;
				}
				this.textBox.Selected = false;
			}
		}

		// Token: 0x06002B24 RID: 11044 RVA: 0x0020A605 File Offset: 0x00208805
		public override void receiveKeyPress(Keys key)
		{
			if (!this.textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002B25 RID: 11045 RVA: 0x0020A634 File Offset: 0x00208834
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (this.doneNamingButton != null)
			{
				if (this.doneNamingButton.containsPoint(x, y))
				{
					this.doneNamingButton.scale = Math.Min(1.1f, this.doneNamingButton.scale + 0.05f);
				}
				else
				{
					this.doneNamingButton.scale = Math.Max(1f, this.doneNamingButton.scale - 0.05f);
				}
			}
			this.randomButton.tryHover(x, y, 0.5f);
		}

		// Token: 0x06002B26 RID: 11046 RVA: 0x0020A6C0 File Offset: 0x002088C0
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			this.textBox.Update();
			if (this.doneNamingButton.containsPoint(x, y))
			{
				this.textBoxEnter(this.textBox);
				Game1.playSound("smallSelect", null);
				return;
			}
			if (this.randomButton.containsPoint(x, y))
			{
				this.textBox.Text = Dialogue.randomName();
				this.randomButton.scale = this.randomButton.baseScale;
				Game1.playSound("drumkit6", null);
			}
		}

		// Token: 0x06002B27 RID: 11047 RVA: 0x0020A75C File Offset: 0x0020895C
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			SpriteText.drawStringWithScrollCenteredAt(b, this.title, Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 - 128, this.title, 1f, null, 0, 0.88f, false);
			this.textBox.Draw(b, true);
			this.doneNamingButton.draw(b);
			this.randomButton.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001CBA RID: 7354
		public const int region_okButton = 101;

		// Token: 0x04001CBB RID: 7355
		public const int region_doneNamingButton = 102;

		// Token: 0x04001CBC RID: 7356
		public const int region_randomButton = 103;

		// Token: 0x04001CBD RID: 7357
		public const int region_namingBox = 104;

		// Token: 0x04001CBE RID: 7358
		public ClickableTextureComponent doneNamingButton;

		// Token: 0x04001CBF RID: 7359
		public ClickableTextureComponent randomButton;

		// Token: 0x04001CC0 RID: 7360
		public TextBox textBox;

		// Token: 0x04001CC1 RID: 7361
		public ClickableComponent textBoxCC;

		// Token: 0x04001CC2 RID: 7362
		public NamingMenu.doneNamingBehavior doneNaming;

		// Token: 0x04001CC3 RID: 7363
		public string title;

		// Token: 0x04001CC4 RID: 7364
		public int minLength = 1;

		// Token: 0x04001CC5 RID: 7365
		public bool FilterInput = true;

		// Token: 0x02000625 RID: 1573
		// (Invoke) Token: 0x0600445E RID: 17502
		public delegate void doneNamingBehavior(string s);
	}
}
