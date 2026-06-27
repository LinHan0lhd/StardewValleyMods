using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000178 RID: 376
	internal sealed class GalaxyLobbyMemberStateListener : ILobbyMemberStateListener
	{
		// Token: 0x06001C43 RID: 7235 RVA: 0x00140DF8 File Offset: 0x0013EFF8
		public GalaxyLobbyMemberStateListener(Action<GalaxyID, GalaxyID, LobbyMemberStateChange> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyMemberState.GetListenerType(), this);
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x00140E17 File Offset: 0x0013F017
		public override void OnLobbyMemberStateChanged(GalaxyID lobbyID, GalaxyID memberID, LobbyMemberStateChange memberStateChange)
		{
			Action<GalaxyID, GalaxyID, LobbyMemberStateChange> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, memberID, memberStateChange);
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x00140E2C File Offset: 0x0013F02C
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyMemberState.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001112 RID: 4370
		private readonly Action<GalaxyID, GalaxyID, LobbyMemberStateChange> Callback;
	}
}
