using System;
using StardewValley.Mods;

namespace StardewValley
{
	// Token: 0x020000B5 RID: 181
	public interface IHaveModData
	{
		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000C9F RID: 3231
		ModDataDictionary modData { get; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000CA0 RID: 3232
		// (set) Token: 0x06000CA1 RID: 3233
		ModDataDictionary modDataForSerialization { get; set; }
	}
}
