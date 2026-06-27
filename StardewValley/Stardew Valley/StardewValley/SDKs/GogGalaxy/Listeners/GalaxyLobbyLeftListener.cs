using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000177 RID: 375
	internal sealed class GalaxyLobbyLeftListener : ILobbyLeftListener
	{
		// Token: 0x06001C40 RID: 7232 RVA: 0x00140DAD File Offset: 0x0013EFAD
		public GalaxyLobbyLeftListener(Action<GalaxyID, ILobbyLeftListener.LobbyLeaveReason> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
		}

		// Token: 0x06001C41 RID: 7233 RVA: 0x00140DCC File Offset: 0x0013EFCC
		public override void OnLobbyLeft(GalaxyID lobbyID, ILobbyLeftListener.LobbyLeaveReason leaveReason)
		{
			Action<GalaxyID, ILobbyLeftListener.LobbyLeaveReason> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, leaveReason);
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x00140DE0 File Offset: 0x0013EFE0
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001111 RID: 4369
		private readonly Action<GalaxyID, ILobbyLeftListener.LobbyLeaveReason> Callback;
	}
}
