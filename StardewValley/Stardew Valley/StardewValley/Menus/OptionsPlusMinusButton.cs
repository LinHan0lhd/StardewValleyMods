using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000290 RID: 656
	public class OptionsPlusMinusButton : OptionsPlusMinus
	{
		// Token: 0x06002B43 RID: 11075 RVA: 0x0020B86C File Offset: 0x00209A6C
		public OptionsPlusMinusButton(string label, int whichOptions, List<string> options, List<string> displayOptions, Texture2D buttonTexture, Rectangle buttonRect, Action<string> buttonAction, int x = -1, int y = -1) : base(label, whichOptions, options, displayOptions, x, y)
		{
			this._buttonRect = buttonRect;
			this._buttonBounds = new Rectangle(this.bounds.Left, 4 - this._buttonRect.Height / 2 + 8, this._buttonRect.Width * 4, this._buttonRect.Height * 4);
			this._buttonTexture = buttonTexture;
			this._buttonAction = buttonAction;
			int offset = 8;
			this.plusButton.X = this.plusButton.X + (this._buttonBounds.Width + offset * 4);
			this.minusButton.X = this.minusButton.X + (this._buttonBounds.Width + offset * 4);
			this.bounds.Width = this.bounds.Width + (this._buttonBounds.Width + offset * 4);
			int height_adjustment = this._buttonBounds.Height - this.bounds.Height;
			if (height_adjustment > 0)
			{
				this.bounds.Y = this.bounds.Y - height_adjustment / 2;
				this.bounds.Height = this.bounds.Height + height_adjustment;
				this.labelOffset.Y = this.labelOffset.Y + (float)(height_adjustment / 2);
			}
		}

		// Token: 0x06002B44 RID: 11076 RVA: 0x0020B990 File Offset: 0x00209B90
		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			b.Draw(this._buttonTexture, new Vector2((float)(slotX + this._buttonBounds.X), (float)(slotY + this._buttonBounds.Y)), new Rectangle?(this._buttonRect), Color.White * (this.greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			base.draw(b, slotX, slotY, context);
		}

		// Token: 0x06002B45 RID: 11077 RVA: 0x0020BA14 File Offset: 0x00209C14
		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut && this._buttonBounds.Contains(x, y))
			{
				if (this._buttonAction != null)
				{
					string selection = "";
					if (this.selected >= 0 && this.selected < this.options.Count)
					{
						selection = this.options[this.selected];
					}
					this._buttonAction(selection);
				}
				return;
			}
			base.receiveLeftClick(x, y);
		}

		// Token: 0x04001CE4 RID: 7396
		protected Rectangle _buttonBounds;

		// Token: 0x04001CE5 RID: 7397
		protected Rectangle _buttonRect;

		// Token: 0x04001CE6 RID: 7398
		protected Texture2D _buttonTexture;

		// Token: 0x04001CE7 RID: 7399
		protected Action<string> _buttonAction;
	}
}
