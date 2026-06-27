using System;

namespace StardewValley.Hashing
{
	// Token: 0x02000319 RID: 793
	public interface IHashUtility
	{
		// Token: 0x06003448 RID: 13384
		int GetDeterministicHashCode(string value);

		// Token: 0x06003449 RID: 13385
		int GetDeterministicHashCode(params int[] values);
	}
}
