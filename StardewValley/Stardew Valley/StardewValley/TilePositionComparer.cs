using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x020000DA RID: 218
	public class TilePositionComparer : IEqualityComparer<Vector2>
	{
		// Token: 0x06001095 RID: 4245 RVA: 0x000C7342 File Offset: 0x000C5542
		public bool Equals(Vector2 a, Vector2 b)
		{
			return a.Equals(b);
		}

		// Token: 0x06001096 RID: 4246 RVA: 0x000C734C File Offset: 0x000C554C
		public int GetHashCode(Vector2 a)
		{
			return (int)((ushort)a.X) | (int)((ushort)a.Y) << 16;
		}
	}
}
