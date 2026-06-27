using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000292 RID: 658
	public class OptionsCheckbox : OptionsElement
	{
		// Token: 0x06002B4B RID: 11083 RVA: 0x0020BFD0 File Offset: 0x0020A1D0
		public OptionsCheckbox(string label, int whichOption, int x = -1, int y = -1) : base(label, x, y, 36, 36, whichOption)
		{
			Game1.options.setCheckBoxToProperValue(this);
		}

		// Token: 0x06002B4C RID: 11084 RVA: 0x0020BFEC File Offset: 0x0020A1EC
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut)
			{
				Game1.playSound("drumkit6", null);
				OptionsCheckbox.selected = this;
				base.receiveLeftClick(x, y);
				this.isChecked = !this.isChecked;
				Game1.options.changeCheckBoxOption(this.whichOption, this.isChecked);
				OptionsCheckbox.selected = null;
			}
		}

		// Token: 0x06002B4D RID: 11085 RVA: 0x0020C050 File Offset: 0x0020A250
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X), (float)(slotY + this.bounds.Y)), new Rectangle?(this.isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked), Color.White * (this.greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			base.draw(b, slotX, slotY, context);
		}

		// Token: 0x04001CF3 RID: 7411
		public const int pixelsWide = 9;

		// Token: 0x04001CF4 RID: 7412
		public static OptionsCheckbox selected;

		// Token: 0x04001CF5 RID: 7413
		public bool isChecked;

		// Token: 0x04001CF6 RID: 7414
		public static Rectangle sourceRectUnchecked = new Rectangle(227, 425, 9, 9);

		// Token: 0x04001CF7 RID: 7415
		public static Rectangle sourceRectChecked = new Rectangle(236, 425, 9, 9);
	}
}
