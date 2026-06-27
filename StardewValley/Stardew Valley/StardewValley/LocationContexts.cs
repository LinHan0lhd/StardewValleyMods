using System;
using System.Collections.Generic;
using StardewValley.GameData.LocationContexts;

namespace StardewValley
{
	// Token: 0x020000D4 RID: 212
	public static class LocationContexts
	{
		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06001070 RID: 4208 RVA: 0x000C681C File Offset: 0x000C4A1C
		public static LocationContextData Island
		{
			get
			{
				return LocationContexts.Require("Island");
			}
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06001071 RID: 4209 RVA: 0x000C6828 File Offset: 0x000C4A28
		public static LocationContextData Default
		{
			get
			{
				return LocationContexts.Require("Default");
			}
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x000C6834 File Offset: 0x000C4A34
		public static LocationContextData Require(string id)
		{
			LocationContextData data;
			if (id == null || !Game1.locationContextData.TryGetValue(id, out data))
			{
				throw new KeyNotFoundException("There's no entry in Data/LocationContexts with the required ID '" + id + "'.");
			}
			return data;
		}

		// Token: 0x040009FD RID: 2557
		public const string DefaultId = "Default";

		// Token: 0x040009FE RID: 2558
		public const string DesertId = "Desert";

		// Token: 0x040009FF RID: 2559
		public const string IslandId = "Island";
	}
}
