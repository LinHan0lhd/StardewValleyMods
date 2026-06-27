using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x020002B7 RID: 695
	public class TooManyFarmsMenu : IClickableMenu
	{
		// Token: 0x06002D67 RID: 11623 RVA: 0x00237F38 File Offset: 0x00236138
		public TooManyFarmsMenu()
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(800, 180, 0, 0);
			base.initialize((int)topLeft.X, (int)topLeft.Y, 800, 180, false);
		}

		// Token: 0x06002D68 RID: 11624 RVA: 0x00237F7C File Offset: 0x0023617C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.exitThisMenu(true);
		}

		// Token: 0x06002D69 RID: 11625 RVA: 0x00237F88 File Offset: 0x00236188
		public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle?(new Rectangle(306, 320, 16, 16)), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos - 28)), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos - 28)), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos + boxHeight - 8)), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos + boxHeight - 4)), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		}

		// Token: 0x06002D6A RID: 11626 RVA: 0x002381DC File Offset: 0x002363DC
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			this.drawBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
			int pad = 35;
			string message = Game1.content.LoadString("Strings\\UI:TooManyFarmsMenu_TooManyFarms");
			SpriteText.drawString(b, message, this.xPositionOnScreen + pad, this.yPositionOnScreen + pad, 999999, this.width, this.height, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			int ypos = 260;
			Rectangle dstRect = new Rectangle(this.xPositionOnScreen + this.width - 14 - 52, this.yPositionOnScreen + this.height - 14 - 52, 52, 52);
			Rectangle srcRect = new Rectangle(542, ypos, 26, 26);
			if (Game1.options.gamepadControls)
			{
				b.Draw(Game1.controllerMaps, dstRect, new Rectangle?(srcRect), Color.White);
			}
		}

		// Token: 0x04001F2E RID: 7982
		public const int cWidth = 800;

		// Token: 0x04001F2F RID: 7983
		public const int cHeight = 180;
	}
}
