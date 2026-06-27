using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Util
{
	// Token: 0x02000118 RID: 280
	public class BoundingBoxGroup
	{
		// Token: 0x060017AD RID: 6061 RVA: 0x001118D8 File Offset: 0x0010FAD8
		public bool Intersects(Rectangle rect)
		{
			foreach (Rectangle rectangle in this.rectangles)
			{
				if (rectangle.Intersects(rect))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060017AE RID: 6062 RVA: 0x00111938 File Offset: 0x0010FB38
		public bool Contains(int x, int y)
		{
			foreach (Rectangle rectangle in this.rectangles)
			{
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060017AF RID: 6063 RVA: 0x00111998 File Offset: 0x0010FB98
		public void Add(Rectangle rect)
		{
			if (!this.rectangles.Contains(rect))
			{
				this.rectangles.Add(rect);
			}
		}

		// Token: 0x060017B0 RID: 6064 RVA: 0x001119B4 File Offset: 0x0010FBB4
		public void ClearNonIntersecting(Rectangle rect)
		{
			this.rectangles.RemoveAll((Rectangle r) => !r.Intersects(rect));
		}

		// Token: 0x060017B1 RID: 6065 RVA: 0x001119E6 File Offset: 0x0010FBE6
		public void Clear()
		{
			this.rectangles.Clear();
		}

		// Token: 0x060017B2 RID: 6066 RVA: 0x001119F4 File Offset: 0x0010FBF4
		public void Draw(SpriteBatch b)
		{
			foreach (Rectangle r in this.rectangles)
			{
				r.Offset(-Game1.viewport.X, -Game1.viewport.Y);
				b.Draw(Game1.fadeToBlackRect, r, Color.Green * 0.5f);
			}
		}

		// Token: 0x060017B3 RID: 6067 RVA: 0x00111A78 File Offset: 0x0010FC78
		public bool IsEmpty()
		{
			return this.rectangles.Count == 0;
		}

		// Token: 0x04000E46 RID: 3654
		private List<Rectangle> rectangles = new List<Rectangle>();
	}
}
