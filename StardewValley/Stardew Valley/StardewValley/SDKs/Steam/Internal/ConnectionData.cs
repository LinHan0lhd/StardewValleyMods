using System;
using Steamworks;

namespace StardewValley.SDKs.Steam.Internal
{
	// Token: 0x02000168 RID: 360
	internal sealed class ConnectionData
	{
		// Token: 0x06001BA6 RID: 7078 RVA: 0x0013E6B5 File Offset: 0x0013C8B5
		public ConnectionData(HSteamNetConnection connection, CSteamID steamId, string displayName)
		{
			this.Connection = connection;
			this.SteamId = steamId;
			this.DisplayName = displayName;
		}

		// Token: 0x040010BE RID: 4286
		public long FarmerId = long.MinValue;

		// Token: 0x040010BF RID: 4287
		public CSteamID SteamId;

		// Token: 0x040010C0 RID: 4288
		public HSteamNetConnection Connection;

		// Token: 0x040010C1 RID: 4289
		public bool Online;

		// Token: 0x040010C2 RID: 4290
		public string DisplayName;
	}
}
