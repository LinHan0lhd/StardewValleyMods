using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x0200028E RID: 654
	public class OptionsElement : IScreenReadable
	{
		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06002B32 RID: 11058 RVA: 0x0020B385 File Offset: 0x00209585
		// (set) Token: 0x06002B33 RID: 11059 RVA: 0x0020B38D File Offset: 0x0020958D
		public string ScreenReaderText { get; set; }

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06002B34 RID: 11060 RVA: 0x0020B396 File Offset: 0x00209596
		// (set) Token: 0x06002B35 RID: 11061 RVA: 0x0020B39E File Offset: 0x0020959E
		public string ScreenReaderDescription { get; set; }

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06002B36 RID: 11062 RVA: 0x0020B3A7 File Offset: 0x002095A7
		// (set) Token: 0x06002B37 RID: 11063 RVA: 0x0020B3AF File Offset: 0x002095AF
		public bool ScreenReaderIgnore { get; set; }

		// Token: 0x06002B38 RID: 11064 RVA: 0x0020B3B8 File Offset: 0x002095B8
		public OptionsElement(string label)
		{
			this.label = label;
			this.bounds = new Rectangle(32, 16, 36, 36);
			this.whichOption = -1;
		}

		// Token: 0x06002B39 RID: 11065 RVA: 0x0020B3EC File Offset: 0x002095EC
		public OptionsElement(string label, int x, int y, int width, int height, int whichOption = -1)
		{
			if (x == -1)
			{
				x = 32;
			}
			if (y == -1)
			{
				y = 16;
			}
			this.bounds = new Rectangle(x, y, width, height);
			this.label = label;
			this.whichOption = whichOption;
		}

		// Token: 0x06002B3A RID: 11066 RVA: 0x0020B43A File Offset: 0x0020963A
		public OptionsElement(string label, Rectangle bounds, int whichOption)
		{
			this.whichOption = whichOption;
			this.label = label;
			this.bounds = bounds;
		}

		// Token: 0x06002B3B RID: 11067 RVA: 0x0020B462 File Offset: 0x00209662
		public virtual void receiveLeftClick(int x, int y)
		{
		}

		// Token: 0x06002B3C RID: 11068 RVA: 0x0020B464 File Offset: 0x00209664
		public virtual void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002B3D RID: 11069 RVA: 0x0020B466 File Offset: 0x00209666
		public virtual void leftClickReleased(int x, int y)
		{
		}

		// Token: 0x06002B3E RID: 11070 RVA: 0x0020B468 File Offset: 0x00209668
		public virtual void receiveKeyPress(Keys key)
		{
		}

		// Token: 0x06002B3F RID: 11071 RVA: 0x0020B46C File Offset: 0x0020966C
		public virtual void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			if (this.style == OptionsElement.Style.OptionLabel)
			{
				Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.X + (int)this.labelOffset.X), (float)(slotY + this.bounds.Y + (int)this.labelOffset.Y + 12)), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f, -1, -1, 1f, 3);
				return;
			}
			if (this.whichOption == -1)
			{
				SpriteText.drawString(b, this.label, slotX + this.bounds.X + (int)this.labelOffset.X, slotY + this.bounds.Y + (int)this.labelOffset.Y + 56 - SpriteText.getHeightOfString(this.label, 999999), 999, -1, 999, 1f, 0.1f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				return;
			}
			int label_start_x = slotX + this.bounds.X + this.bounds.Width + 8 + (int)this.labelOffset.X;
			int label_start_y = slotY + this.bounds.Y + (int)this.labelOffset.Y;
			string displayed_text = this.label;
			SpriteFont font = Game1.dialogueFont;
			if (context != null)
			{
				int max_width = context.width - 64;
				int menu_start_x = context.xPositionOnScreen;
				if (font.MeasureString(this.label).X + (float)label_start_x > (float)(max_width + menu_start_x))
				{
					int allowed_space = max_width + menu_start_x - label_start_x;
					font = Game1.smallFont;
					displayed_text = Game1.parseText(this.label, font, allowed_space);
					label_start_y -= (int)((font.MeasureString(displayed_text).Y - font.MeasureString("T").Y) / 2f);
				}
			}
			Utility.drawTextWithShadow(b, displayed_text, font, new Vector2((float)label_start_x, (float)label_start_y), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f, -1, -1, 1f, 3);
		}

		// Token: 0x04001CD7 RID: 7383
		public const int defaultX = 8;

		// Token: 0x04001CD8 RID: 7384
		public const int defaultY = 4;

		// Token: 0x04001CD9 RID: 7385
		public const int defaultPixelWidth = 9;

		// Token: 0x04001CDA RID: 7386
		public Rectangle bounds;

		// Token: 0x04001CDB RID: 7387
		public string label;

		// Token: 0x04001CDC RID: 7388
		public int whichOption;

		// Token: 0x04001CDD RID: 7389
		public bool greyedOut;

		// Token: 0x04001CDE RID: 7390
		public Vector2 labelOffset = Vector2.Zero;

		// Token: 0x04001CDF RID: 7391
		public OptionsElement.Style style;

		// Token: 0x02000627 RID: 1575
		public enum Style
		{
			// Token: 0x04002E9D RID: 11933
			Default,
			// Token: 0x04002E9E RID: 11934
			OptionLabel
		}
	}
}
