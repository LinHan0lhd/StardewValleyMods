using System;
using Galaxy.Api;
using Steamworks;

namespace StardewValley.SDKs.Steam.Internal
{
	// Token: 0x02000169 RID: 361
	internal struct HybridLobby
	{
		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06001BA7 RID: 7079 RVA: 0x0013E6E1 File Offset: 0x0013C8E1
		// (set) Token: 0x06001BA8 RID: 7080 RVA: 0x0013E6E9 File Offset: 0x0013C8E9
		public ulong SteamId { readonly get; private set; }

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06001BA9 RID: 7081 RVA: 0x0013E6F2 File Offset: 0x0013C8F2
		// (set) Token: 0x06001BAA RID: 7082 RVA: 0x0013E6FA File Offset: 0x0013C8FA
		public ulong GalaxyId { readonly get; private set; }

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06001BAB RID: 7083 RVA: 0x0013E704 File Offset: 0x0013C904
		public LobbyConnectionType LobbyType
		{
			get
			{
				CSteamID steamID = new CSteamID(this.SteamId);
				if (steamID.IsValid() && steamID.IsLobby())
				{
					return LobbyConnectionType.Steam;
				}
				if (!new GalaxyID(this.GalaxyId).IsValid())
				{
					return LobbyConnectionType.Invalid;
				}
				if (this.IsHybrid)
				{
					return LobbyConnectionType.Hybrid;
				}
				return LobbyConnectionType.Galaxy;
			}
		}

		// Token: 0x06001BAC RID: 7084 RVA: 0x0013E751 File Offset: 0x0013C951
		public HybridLobby(CSteamID steamID)
		{
			this.SteamId = steamID.m_SteamID;
			this.GalaxyId = 0UL;
			this.IsHybrid = false;
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x0013E76E File Offset: 0x0013C96E
		public HybridLobby(GalaxyID galaxyID, bool isHybrid = false)
		{
			this.SteamId = 0UL;
			this.GalaxyId = galaxyID.ToUint64();
			this.IsHybrid = isHybrid;
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x0013E78B File Offset: 0x0013C98B
		public void Clear()
		{
			this.SteamId = 0UL;
			this.GalaxyId = 0UL;
			this.IsHybrid = false;
		}

		// Token: 0x040010C5 RID: 4293
		private bool IsHybrid;
	}
}
