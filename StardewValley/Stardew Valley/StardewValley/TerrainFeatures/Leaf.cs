using System;
using Microsoft.Xna.Framework;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x0200014A RID: 330
	public class Leaf
	{
		// Token: 0x06001A2A RID: 6698 RVA: 0x00133696 File Offset: 0x00131896
		public Leaf(Vector2 position, float rotationRate, int type, float yVelocity)
		{
			this.position = position;
			this.rotationRate = rotationRate;
			this.type = type;
			this.yVelocity = yVelocity;
		}

		// Token: 0x04000FFD RID: 4093
		public Vector2 position;

		// Token: 0x04000FFE RID: 4094
		public float rotation;

		// Token: 0x04000FFF RID: 4095
		public float rotationRate;

		// Token: 0x04001000 RID: 4096
		public float yVelocity;

		// Token: 0x04001001 RID: 4097
		public int type;
	}
}
