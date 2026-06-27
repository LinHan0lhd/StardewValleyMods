using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000179 RID: 377
	internal sealed class GalaxyOperationalStateChangeListener : IOperationalStateChangeListener
	{
		// Token: 0x06001C46 RID: 7238 RVA: 0x00140E44 File Offset: 0x0013F044
		public GalaxyOperationalStateChangeListener(Action<uint> callback)
		{
			this.Callback = callback;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerOperationalStateChange.GetListenerType(), this);
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x00140E63 File Offset: 0x0013F063
		public override void OnOperationalStateChanged(uint operationalState)
		{
			Action<uint> callback = this.Callback;
			if (callback == null)
			{
				return;
			}
			callback(operationalState);
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x00140E76 File Offset: 0x0013F076
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerOperationalStateChange.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001113 RID: 4371
		private readonly Action<uint> Callback;
	}
}
