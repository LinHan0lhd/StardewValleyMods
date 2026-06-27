using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000174 RID: 372
	internal sealed class GalaxyLobbyDataListener : ILobbyDataListener
	{
		// Token: 0x06001C37 RID: 7223 RVA: 0x00140CDA File Offset: 0x0013EEDA
		public GalaxyLobbyDataListener(Action<GalaxyID, GalaxyID> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyData.GetListenerType(), this);
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x00140CF9 File Offset: 0x0013EEF9
		public override void OnLobbyDataUpdated(GalaxyID lobbyID, GalaxyID memberID)
		{
			Action<GalaxyID, GalaxyID> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, memberID);
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x00140D0D File Offset: 0x0013EF0D
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyData.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x0400110D RID: 4365
		private readonly Action<GalaxyID, GalaxyID> Callback;
	}
}
