using System;

namespace StardewValley.Network
{
	// Token: 0x020001D2 RID: 466
	public interface IHookableServer
	{
		// Token: 0x17000358 RID: 856
		// (get) Token: 0x060020B5 RID: 8373
		// (set) Token: 0x060020B6 RID: 8374
		Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }
	}
}
