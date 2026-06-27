using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x020002B5 RID: 693
	public class TitleTextInputMenu : NamingMenu
	{
		// Token: 0x06002D5A RID: 11610 RVA: 0x00237350 File Offset: 0x00235550
		public TitleTextInputMenu(string title, NamingMenu.doneNamingBehavior b, string default_text = "", string context = "", bool filterInput = true) : base(b, title, "")
		{
			this.FilterInput = filterInput;
			this.context = context;
			this.textBox.limitWidth = false;
			this.textBox.Width = 512;
			this.textBox.X -= 128;
			this.randomButton.visible = false;
			this.pasteButton = new ClickableTextureComponent(new Rectangle(this.textBox.X + this.textBox.Width + 32 + 4 + 64, Game1.viewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f, false)
			{
				myID = 105,
				leftNeighborID = 102
			};
			this.pasteButton.visible = true;
			this.doneNamingButton.rightNeighborID = 105;
			ClickableTextureComponent doneNamingButton = this.doneNamingButton;
			doneNamingButton.bounds.X = doneNamingButton.bounds.X + 128;
			this.minLength = 0;
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			this.textBox.Text = default_text;
		}

		// Token: 0x06002D5B RID: 11611 RVA: 0x00237490 File Offset: 0x00235690
		public override void performHoverAction(int x, int y)
		{
			ClickableTextureComponent clickableTextureComponent = this.pasteButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.tryHover(x, y, 0.1f);
			}
			base.performHoverAction(x, y);
		}

		// Token: 0x06002D5C RID: 11612 RVA: 0x002374B4 File Offset: 0x002356B4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.pasteButton != null && this.pasteButton.containsPoint(x, y))
			{
				string pasted_text = "";
				if (DesktopClipboard.GetText(ref pasted_text))
				{
					Game1.playSound("drumkit6", null);
					this.textBox.Text = pasted_text;
				}
				else
				{
					Game1.playSound("cancel", null);
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002D5D RID: 11613 RVA: 0x00237528 File Offset: 0x00235728
		public override void update(GameTime time)
		{
			GamePadState pad = Game1.input.GetGamePadState();
			KeyboardState keyboard = Game1.GetKeyboardState();
			if (Game1.IsPressEvent(ref pad, Buttons.B) || Game1.IsPressEvent(ref keyboard, Keys.Escape))
			{
				TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
				if (titleMenu != null)
				{
					titleMenu.backButtonPressed();
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
			base.update(time);
		}

		// Token: 0x06002D5E RID: 11614 RVA: 0x00237581 File Offset: 0x00235781
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			ClickableTextureComponent clickableTextureComponent = this.pasteButton;
			if (clickableTextureComponent == null)
			{
				return;
			}
			clickableTextureComponent.draw(b);
		}

		// Token: 0x04001F24 RID: 7972
		public ClickableTextureComponent pasteButton;

		// Token: 0x04001F25 RID: 7973
		public const int region_pasteButton = 105;

		// Token: 0x04001F26 RID: 7974
		public string context = "";
	}
}
