using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200029D RID: 669
	public class ProfileItem
	{
		// Token: 0x06002BBE RID: 11198 RVA: 0x00214872 File Offset: 0x00212A72
		public ProfileItem(ProfileMenu context, string name)
		{
			this._context = context;
			this.itemName = name;
		}

		// Token: 0x06002BBF RID: 11199 RVA: 0x00214893 File Offset: 0x00212A93
		public virtual void Unload()
		{
		}

		// Token: 0x06002BC0 RID: 11200 RVA: 0x00214895 File Offset: 0x00212A95
		public virtual string GetName()
		{
			return this.itemName;
		}

		// Token: 0x06002BC1 RID: 11201 RVA: 0x0021489D File Offset: 0x00212A9D
		public virtual void performHover(int x, int y)
		{
		}

		// Token: 0x06002BC2 RID: 11202 RVA: 0x002148A0 File Offset: 0x00212AA0
		public virtual float HandleLayout(float draw_y, Rectangle content_rectangle, int index)
		{
			if (index > 0)
			{
				draw_y += Game1.smallFont.MeasureString(this.GetName()).Y;
			}
			this._nameDrawPosition = new Vector2((float)content_rectangle.Left, draw_y);
			draw_y += Game1.smallFont.MeasureString(this.GetName()).Y;
			return draw_y;
		}

		// Token: 0x06002BC3 RID: 11203 RVA: 0x002148F8 File Offset: 0x00212AF8
		public virtual void DrawItemName(SpriteBatch b)
		{
			b.DrawString(Game1.smallFont, this.GetName(), this._nameDrawPosition, Game1.textColor);
		}

		// Token: 0x06002BC4 RID: 11204 RVA: 0x00214916 File Offset: 0x00212B16
		public virtual void Draw(SpriteBatch b)
		{
			this.DrawItemName(b);
			this.DrawItem(b);
		}

		// Token: 0x06002BC5 RID: 11205 RVA: 0x00214926 File Offset: 0x00212B26
		public virtual void DrawItem(SpriteBatch b)
		{
		}

		// Token: 0x06002BC6 RID: 11206 RVA: 0x00214928 File Offset: 0x00212B28
		public virtual bool ShouldDraw()
		{
			return true;
		}

		// Token: 0x04001D7E RID: 7550
		protected ProfileMenu _context;

		// Token: 0x04001D7F RID: 7551
		public string itemName = "";

		// Token: 0x04001D80 RID: 7552
		protected Vector2 _nameDrawPosition;
	}
}
