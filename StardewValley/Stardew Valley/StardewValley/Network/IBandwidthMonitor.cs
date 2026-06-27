using System;

namespace StardewValley.Network
{
	// Token: 0x020001D0 RID: 464
	public interface IBandwidthMonitor
	{
		// Token: 0x17000354 RID: 852
		// (get) Token: 0x060020AE RID: 8366
		BandwidthLogger BandwidthLogger { get; }

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x060020AF RID: 8367
		// (set) Token: 0x060020B0 RID: 8368
		bool LogBandwidth { get; set; }
	}
}
