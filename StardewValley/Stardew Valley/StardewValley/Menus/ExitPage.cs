using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200026D RID: 621
	public class ExitPage : IClickableMenu
	{
		// Token: 0x06002920 RID: 10528 RVA: 0x001E3744 File Offset: 0x001E1944
		public ExitPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			string exit_to_title_string = Game1.content.LoadString("Strings\\UI:ExitToTitle");
			if (!Game1.game1.IsMainInstance)
			{
				exit_to_title_string = Game1.content.LoadString("Strings\\UI:DropOutLocalMulti");
			}
			Vector2 exitPos = new Vector2((float)(this.xPositionOnScreen + width / 2 - (int)((Game1.dialogueFont.MeasureString(exit_to_title_string).X + 64f) / 2f)), (float)(this.yPositionOnScreen + 256 - 32));
			this.exitToTitle = new ClickableComponent(new Rectangle((int)exitPos.X, (int)exitPos.Y, (int)Game1.dialogueFont.MeasureString(exit_to_title_string).X + 64, 96), "", exit_to_title_string)
			{
				myID = 535,
				upNeighborID = 12349,
				downNeighborID = 536
			};
			exitPos = new Vector2((float)(this.xPositionOnScreen + width / 2 - (int)((Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:ExitToDesktop")).X + 64f) / 2f)), (float)(this.yPositionOnScreen + 384 + 8 - 32));
			this.exitToDesktop = new ClickableComponent(new Rectangle((int)exitPos.X, (int)exitPos.Y, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:ExitToDesktop")).X + 64, 96), "", Game1.content.LoadString("Strings\\UI:ExitToDesktop"))
			{
				myID = 536,
				upNeighborID = 535
			};
			if (!Game1.game1.IsMainInstance)
			{
				this.exitToDesktop.visible = false;
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002921 RID: 10529 RVA: 0x001E3910 File Offset: 0x001E1B10
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(12349);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002922 RID: 10530 RVA: 0x001E392C File Offset: 0x001E1B2C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.conventionMode)
			{
				return;
			}
			if (this.exitToTitle.containsPoint(x, y) && this.exitToTitle.visible)
			{
				if (Game1.options.optionsDirty)
				{
					Game1.options.SaveDefaultOptions();
				}
				Game1.playSound("bigDeSelect", null);
				Game1.ExitToTitle(null);
			}
			if (this.exitToDesktop.containsPoint(x, y) && this.exitToDesktop.visible)
			{
				if (Game1.options.optionsDirty)
				{
					Game1.options.SaveDefaultOptions();
				}
				Game1.playSound("bigDeSelect", null);
				Game1.quit = true;
			}
		}

		// Token: 0x06002923 RID: 10531 RVA: 0x001E39DC File Offset: 0x001E1BDC
		public override void performHoverAction(int x, int y)
		{
			if (this.exitToTitle.containsPoint(x, y) && this.exitToTitle.visible)
			{
				if (this.exitToTitle.scale == 0f)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.exitToTitle.scale = 1f;
			}
			else
			{
				this.exitToTitle.scale = 0f;
			}
			if (this.exitToDesktop.containsPoint(x, y) && this.exitToDesktop.visible)
			{
				if (this.exitToDesktop.scale == 0f)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.exitToDesktop.scale = 1f;
				return;
			}
			this.exitToDesktop.scale = 0f;
		}

		// Token: 0x06002924 RID: 10532 RVA: 0x001E3AB0 File Offset: 0x001E1CB0
		public override void draw(SpriteBatch b)
		{
			if (this.exitToTitle.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), this.exitToTitle.bounds.X, this.exitToTitle.bounds.Y, this.exitToTitle.bounds.Width, this.exitToTitle.bounds.Height, (this.exitToTitle.scale > 0f) ? Color.Wheat : Color.White, 4f, true, -1f);
				Utility.drawTextWithShadow(b, this.exitToTitle.label, Game1.dialogueFont, new Vector2((float)this.exitToTitle.bounds.Center.X, (float)(this.exitToTitle.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.exitToTitle.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f, 3);
			}
			if (this.exitToDesktop.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), this.exitToDesktop.bounds.X, this.exitToDesktop.bounds.Y, this.exitToDesktop.bounds.Width, this.exitToDesktop.bounds.Height, (this.exitToDesktop.scale > 0f) ? Color.Wheat : Color.White, 4f, true, -1f);
				Utility.drawTextWithShadow(b, this.exitToDesktop.label, Game1.dialogueFont, new Vector2((float)this.exitToDesktop.bounds.Center.X, (float)(this.exitToDesktop.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.exitToDesktop.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f, 3);
			}
		}

		// Token: 0x04001AE5 RID: 6885
		public ClickableComponent exitToTitle;

		// Token: 0x04001AE6 RID: 6886
		public ClickableComponent exitToDesktop;
	}
}
