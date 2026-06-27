using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000172 RID: 370
	internal sealed class GalaxyGameJoinRequestedListener : IGameJoinRequestedListener
	{
		// Token: 0x06001C31 RID: 7217 RVA: 0x00140C44 File Offset: 0x0013EE44
		public GalaxyGameJoinRequestedListener(Action<GalaxyID, string> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerGameJoinRequested.GetListenerType(), this);
		}

		// Token: 0x06001C32 RID: 7218 RVA: 0x00140C63 File Offset: 0x0013EE63
		public override void OnGameJoinRequested(GalaxyID lobbyID, string result)
		{
			Action<GalaxyID, string> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(lobbyID, result);
		}

		// Token: 0x06001C33 RID: 7219 RVA: 0x00140C77 File Offset: 0x0013EE77
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerGameJoinRequested.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x0400110B RID: 4363
		private readonly Action<GalaxyID, string> Callback;
	}
}
