using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200028F RID: 655
	public class OptionsButton : OptionsElement
	{
		// Token: 0x06002B40 RID: 11072 RVA: 0x0020B69C File Offset: 0x0020989C
		public OptionsButton(string label, Action action) : base(label)
		{
			this.action = action;
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			int height = 68;
			this.bounds = new Rectangle(32, 0, width, height);
		}

		// Token: 0x06002B41 RID: 11073 RVA: 0x0020B6DF File Offset: 0x002098DF
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut && this.bounds.Contains(x, y) && this.action != null)
			{
				this.action();
			}
			base.receiveLeftClick(x, y);
		}

		// Token: 0x06002B42 RID: 11074 RVA: 0x0020B714 File Offset: 0x00209914
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			float draw_layer = 0.8f - (float)(slotY + this.bounds.Y) * 1E-06f;
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White * (this.greyedOut ? 0.33f : 1f), 4f, true, draw_layer);
			Vector2 string_center = Game1.dialogueFont.MeasureString(this.label) / 2f;
			string_center.X = (float)((int)(string_center.X / 4f) * 4);
			string_center.Y = (float)((int)(string_center.Y / 4f) * 4);
			Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.Center.X), (float)(slotY + this.bounds.Center.Y)) - string_center, Game1.textColor * (this.greyedOut ? 0.33f : 1f), 1f, draw_layer + 1E-06f, -1, -1, 0f, 3);
		}

		// Token: 0x04001CE3 RID: 7395
		private Action action;
	}
}
