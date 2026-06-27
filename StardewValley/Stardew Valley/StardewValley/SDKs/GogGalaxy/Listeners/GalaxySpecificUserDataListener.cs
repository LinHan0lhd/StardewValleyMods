using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x0200017B RID: 379
	internal sealed class GalaxySpecificUserDataListener : ISpecificUserDataListener
	{
		// Token: 0x06001C4C RID: 7244 RVA: 0x00140ED8 File Offset: 0x0013F0D8
		public GalaxySpecificUserDataListener(Action<GalaxyID> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerSpecificUserData.GetListenerType(), this);
		}

		// Token: 0x06001C4D RID: 7245 RVA: 0x00140EF7 File Offset: 0x0013F0F7
		public override void OnSpecificUserDataUpdated(GalaxyID userID)
		{
			Action<GalaxyID> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(userID);
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x00140F0A File Offset: 0x0013F10A
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerSpecificUserData.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001115 RID: 4373
		private readonly Action<GalaxyID> Callback;
	}
}
