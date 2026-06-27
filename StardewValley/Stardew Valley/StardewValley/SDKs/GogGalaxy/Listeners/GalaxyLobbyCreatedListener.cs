using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000173 RID: 371
	internal sealed class GalaxyLobbyCreatedListener : ILobbyCreatedListener
	{
		// Token: 0x06001C34 RID: 7220 RVA: 0x00140C8F File Offset: 0x0013EE8F
		public GalaxyLobbyCreatedListener(Action<GalaxyID, LobbyCreateResult> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x00140CAE File Offset: 0x0013EEAE
		public override void OnLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
		{
			Action<GalaxyID, LobbyCreateResult> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, result);
		}

		// Token: 0x06001C36 RID: 7222 RVA: 0x00140CC2 File Offset: 0x0013EEC2
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x0400110C RID: 4364
		private readonly Action<GalaxyID, LobbyCreateResult> Callback;
	}
}
