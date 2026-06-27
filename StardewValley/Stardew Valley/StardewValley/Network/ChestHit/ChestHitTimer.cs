using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Network.ChestHit
{
	// Token: 0x02000208 RID: 520
	public sealed class ChestHitTimer
	{
		// Token: 0x06002301 RID: 8961 RVA: 0x00178D1C File Offset: 0x00176F1C
		public void Update(GameTime time)
		{
			if (this.Milliseconds > 0)
			{
				this.Milliseconds -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x040014B6 RID: 5302
		public int Milliseconds;

		// Token: 0x040014B7 RID: 5303
		public int SavedTime = -1;
	}
}
