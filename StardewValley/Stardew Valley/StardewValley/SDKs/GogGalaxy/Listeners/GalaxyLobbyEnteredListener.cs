using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000176 RID: 374
	internal sealed class GalaxyLobbyEnteredListener : ILobbyEnteredListener
	{
		// Token: 0x06001C3D RID: 7229 RVA: 0x00140D62 File Offset: 0x0013EF62
		public GalaxyLobbyEnteredListener(Action<GalaxyID, LobbyEnterResult> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x00140D81 File Offset: 0x0013EF81
		public override void OnLobbyEntered(GalaxyID lobbyID, LobbyEnterResult result)
		{
			Action<GalaxyID, LobbyEnterResult> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, result);
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x00140D95 File Offset: 0x0013EF95
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001110 RID: 4368
		private readonly Action<GalaxyID, LobbyEnterResult> Callback;
	}
}
