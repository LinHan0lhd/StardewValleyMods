using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x02000296 RID: 662
	public class OptionsDropDown : OptionsElement
	{
		// Token: 0x06002B5D RID: 11101 RVA: 0x0020C8F0 File Offset: 0x0020AAF0
		public OptionsDropDown(string label, int whichOption, int x = -1, int y = -1) : base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
		{
			Game1.options.setDropDownToProperValue(this);
			this.RecalculateBounds();
		}

		// Token: 0x06002B5E RID: 11102 RVA: 0x0020C94C File Offset: 0x0020AB4C
		public virtual void RecalculateBounds()
		{
			foreach (string displayed_option in this.dropDownDisplayOptions)
			{
				float text_width = Game1.smallFont.MeasureString(displayed_option).X;
				if (text_width >= (float)(this.bounds.Width - 48))
				{
					this.bounds.Width = (int)(text_width + 64f);
				}
			}
			this.dropDownBounds = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width - 48, this.bounds.Height * this.dropDownOptions.Count);
		}

		// Token: 0x06002B5F RID: 11103 RVA: 0x0020CA14 File Offset: 0x0020AC14
		public override void leftClickHeld(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.leftClickHeld(x, y);
				this.clicked = true;
				this.dropDownBounds.Y = Math.Min(this.dropDownBounds.Y, Game1.uiViewport.Height - this.dropDownBounds.Height - this.recentSlotY);
				if (!Game1.options.SnappyMenus)
				{
					this.selectedOption = (int)Math.Max(Math.Min((float)(y - this.dropDownBounds.Y) / (float)this.bounds.Height, (float)(this.dropDownOptions.Count - 1)), 0f);
				}
			}
		}

		// Token: 0x06002B60 RID: 11104 RVA: 0x0020CAC0 File Offset: 0x0020ACC0
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.receiveLeftClick(x, y);
				this.startingSelected = this.selectedOption;
				if (!this.clicked)
				{
					Game1.playSound("shwip", null);
				}
				this.leftClickHeld(x, y);
				OptionsDropDown.selected = this;
			}
		}

		// Token: 0x06002B61 RID: 11105 RVA: 0x0020CB14 File Offset: 0x0020AD14
		public override void leftClickReleased(int x, int y)
		{
			if (!this.greyedOut && this.dropDownOptions.Count > 0)
			{
				base.leftClickReleased(x, y);
				if (this.clicked)
				{
					Game1.playSound("drumkit6", null);
				}
				this.clicked = false;
				OptionsDropDown.selected = this;
				if (this.dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
				{
					Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
				}
				else
				{
					this.selectedOption = this.startingSelected;
				}
				OptionsDropDown.selected = null;
			}
		}

		// Token: 0x06002B62 RID: 11106 RVA: 0x0020CBC8 File Offset: 0x0020ADC8
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.SnappyMenus && !this.greyedOut)
			{
				if (!this.clicked)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						this.selectedOption++;
						if (this.selectedOption >= this.dropDownOptions.Count)
						{
							this.selectedOption = 0;
						}
						OptionsDropDown.selected = this;
						Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
						OptionsDropDown.selected = null;
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						this.selectedOption--;
						if (this.selectedOption < 0)
						{
							this.selectedOption = this.dropDownOptions.Count - 1;
						}
						OptionsDropDown.selected = this;
						Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
						OptionsDropDown.selected = null;
						return;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
				{
					Game1.playSound("shiny4", null);
					this.selectedOption++;
					if (this.selectedOption >= this.dropDownOptions.Count)
					{
						this.selectedOption = 0;
						return;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
				{
					Game1.playSound("shiny4", null);
					this.selectedOption--;
					if (this.selectedOption < 0)
					{
						this.selectedOption = this.dropDownOptions.Count - 1;
					}
				}
			}
		}

		// Token: 0x06002B63 RID: 11107 RVA: 0x0020CD8C File Offset: 0x0020AF8C
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			this.recentSlotY = slotY;
			base.draw(b, slotX, slotY, context);
			float alpha = this.greyedOut ? 0.33f : 1f;
			if (this.clicked)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height, Color.White * alpha, 4f, false, 0.97f);
				for (int i = 0; i < this.dropDownDisplayOptions.Count; i++)
				{
					if (i == this.selectedOption)
					{
						b.Draw(Game1.staminaRect, new Rectangle(slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y + i * this.bounds.Height, this.dropDownBounds.Width, this.bounds.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
					}
					b.DrawString(Game1.smallFont, this.dropDownDisplayOptions[i], new Vector2((float)(slotX + this.dropDownBounds.X + 4), (float)(slotY + this.dropDownBounds.Y + 8 + this.bounds.Height * i)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
				}
				b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
				return;
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width - 48, this.bounds.Height, Color.White * alpha, 4f, false, -1f);
			b.DrawString(Game1.smallFont, (this.selectedOption < this.dropDownDisplayOptions.Count && this.selectedOption >= 0) ? this.dropDownDisplayOptions[this.selectedOption] : "", new Vector2((float)(slotX + this.bounds.X + 4), (float)(slotY + this.bounds.Y + 8)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}

		// Token: 0x04001D06 RID: 7430
		public const int pixelsHigh = 11;

		// Token: 0x04001D07 RID: 7431
		[InstancedStatic]
		public static OptionsDropDown selected;

		// Token: 0x04001D08 RID: 7432
		public List<string> dropDownOptions = new List<string>();

		// Token: 0x04001D09 RID: 7433
		public List<string> dropDownDisplayOptions = new List<string>();

		// Token: 0x04001D0A RID: 7434
		public int selectedOption;

		// Token: 0x04001D0B RID: 7435
		public int recentSlotY;

		// Token: 0x04001D0C RID: 7436
		public int startingSelected;

		// Token: 0x04001D0D RID: 7437
		private bool clicked;

		// Token: 0x04001D0E RID: 7438
		public Rectangle dropDownBounds;

		// Token: 0x04001D0F RID: 7439
		public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

		// Token: 0x04001D10 RID: 7440
		public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);
	}
}
