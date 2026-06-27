using System;

namespace StardewValley
{
	// Token: 0x020000CF RID: 207
	[Flags]
	public enum CollisionMask : byte
	{
		// Token: 0x04000949 RID: 2377
		None = 0,
		// Token: 0x0400094A RID: 2378
		Buildings = 1,
		// Token: 0x0400094B RID: 2379
		Characters = 2,
		// Token: 0x0400094C RID: 2380
		Farmers = 4,
		// Token: 0x0400094D RID: 2381
		Flooring = 8,
		// Token: 0x0400094E RID: 2382
		Furniture = 16,
		// Token: 0x0400094F RID: 2383
		Objects = 32,
		// Token: 0x04000950 RID: 2384
		TerrainFeatures = 64,
		// Token: 0x04000951 RID: 2385
		LocationSpecific = 128,
		// Token: 0x04000952 RID: 2386
		All = 255
	}
}
