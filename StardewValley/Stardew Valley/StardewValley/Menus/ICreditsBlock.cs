using System;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000247 RID: 583
	public abstract class ICreditsBlock
	{
		// Token: 0x060026E1 RID: 9953 RVA: 0x001B815F File Offset: 0x001B635F
		public virtual void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
		}

		// Token: 0x060026E2 RID: 9954 RVA: 0x001B8161 File Offset: 0x001B6361
		public virtual int getHeight(int maxWidth)
		{
			return 0;
		}

		// Token: 0x060026E3 RID: 9955 RVA: 0x001B8164 File Offset: 0x001B6364
		public virtual void hovered()
		{
		}

		// Token: 0x060026E4 RID: 9956 RVA: 0x001B8166 File Offset: 0x001B6366
		public virtual void clicked()
		{
		}
	}
}
