using System;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x020000F6 RID: 246
	public struct RainDrop
	{
		// Token: 0x0600142B RID: 5163 RVA: 0x000F4833 File Offset: 0x000F2A33
		public RainDrop(int x, int y, int frame, int accumulator)
		{
			this.position = new Vector2((float)x, (float)y);
			this.frame = frame;
			this.accumulator = accumulator;
		}

		// Token: 0x04000CA2 RID: 3234
		public int frame;

		// Token: 0x04000CA3 RID: 3235
		public int accumulator;

		// Token: 0x04000CA4 RID: 3236
		public Vector2 position;
	}
}
