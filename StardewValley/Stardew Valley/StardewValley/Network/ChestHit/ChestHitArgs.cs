using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Network.ChestHit
{
	// Token: 0x02000206 RID: 518
	public sealed class ChestHitArgs
	{
		// Token: 0x040014AC RID: 5292
		public GameLocation Location;

		// Token: 0x040014AD RID: 5293
		public Point ChestTile;

		// Token: 0x040014AE RID: 5294
		public Vector2 ToolPosition;

		// Token: 0x040014AF RID: 5295
		public Point StandingPixel;

		// Token: 0x040014B0 RID: 5296
		public int Direction;

		// Token: 0x040014B1 RID: 5297
		public bool HoldDownClick;

		// Token: 0x040014B2 RID: 5298
		public bool ToolCanHit;

		// Token: 0x040014B3 RID: 5299
		public bool RecentlyHit;
	}
}
