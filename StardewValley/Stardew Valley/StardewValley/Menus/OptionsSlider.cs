using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x02000293 RID: 659
	public class OptionsSlider : OptionsElement
	{
		// Token: 0x06002B4F RID: 11087 RVA: 0x0020C112 File Offset: 0x0020A312
		public OptionsSlider(string label, int whichOption, int x = -1, int y = -1) : base(label, x, y, 192, 24, whichOption)
		{
			Game1.options.setSliderToProperValue(this);
		}

		// Token: 0x06002B50 RID: 11088 RVA: 0x0020C134 File Offset: 0x0020A334
		public override void leftClickHeld(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.leftClickHeld(x, y);
				if (x < this.bounds.X)
				{
					this.value = 0;
				}
				else if (x > this.bounds.Right - 40)
				{
					this.value = 100;
				}
				else
				{
					this.value = (int)((float)(x - this.bounds.X) / (float)(this.bounds.Width - 40) * 100f);
				}
				Game1.options.changeSliderOption(this.whichOption, this.value);
			}
		}

		// Token: 0x06002B51 RID: 11089 RVA: 0x0020C1C5 File Offset: 0x0020A3C5
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.receiveLeftClick(x, y);
				this.leftClickHeld(x, y);
			}
		}

		// Token: 0x06002B52 RID: 11090 RVA: 0x0020C1E0 File Offset: 0x0020A3E0
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && !this.greyedOut)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
				{
					this.value = Math.Min(this.value + 10, 100);
					Game1.options.changeSliderOption(this.whichOption, this.value);
					return;
				}
				if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
				{
					this.value = Math.Max(this.value - 10, 0);
					Game1.options.changeSliderOption(this.whichOption, this.value);
				}
			}
		}

		// Token: 0x06002B53 RID: 11091 RVA: 0x0020C2A4 File Offset: 0x0020A4A4
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			base.draw(b, slotX, slotY, context);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsSlider.sliderBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White, 4f, false, -1f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X) + (float)(this.bounds.Width - 40) * ((float)this.value / 100f), (float)(slotY + this.bounds.Y)), new Rectangle?(OptionsSlider.sliderButtonRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		}

		// Token: 0x04001CF8 RID: 7416
		public const int pixelsWide = 48;

		// Token: 0x04001CF9 RID: 7417
		public const int pixelsHigh = 6;

		// Token: 0x04001CFA RID: 7418
		public const int sliderButtonWidth = 10;

		// Token: 0x04001CFB RID: 7419
		public const int sliderMaxValue = 100;

		// Token: 0x04001CFC RID: 7420
		public int value;

		// Token: 0x04001CFD RID: 7421
		public static Rectangle sliderBGSource = new Rectangle(403, 383, 6, 6);

		// Token: 0x04001CFE RID: 7422
		public static Rectangle sliderButtonRect = new Rectangle(420, 441, 10, 6);
	}
}
