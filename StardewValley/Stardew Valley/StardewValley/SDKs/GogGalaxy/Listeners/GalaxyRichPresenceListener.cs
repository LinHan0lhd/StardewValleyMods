using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x0200017A RID: 378
	internal sealed class GalaxyRichPresenceListener : IRichPresenceListener
	{
		// Token: 0x06001C49 RID: 7241 RVA: 0x00140E8E File Offset: 0x0013F08E
		public GalaxyRichPresenceListener(Action<GalaxyID> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerRichPresence.GetListenerType(), this);
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x00140EAD File Offset: 0x0013F0AD
		public override void OnRichPresenceUpdated(GalaxyID userID)
		{
			Action<GalaxyID> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(userID);
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x00140EC0 File Offset: 0x0013F0C0
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerRichPresence.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001114 RID: 4372
		private readonly Action<GalaxyID> Callback;
	}
}
