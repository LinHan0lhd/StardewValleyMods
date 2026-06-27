using System;

namespace StardewValley.Network
{
	// Token: 0x020001D1 RID: 465
	public interface IHookableClient
	{
		// Token: 0x17000356 RID: 854
		// (get) Token: 0x060020B1 RID: 8369
		// (set) Token: 0x060020B2 RID: 8370
		Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x060020B3 RID: 8371
		// (set) Token: 0x060020B4 RID: 8372
		Action<OutgoingMessage, Action<OutgoingMessage>, Action> OnSendingMessage { get; set; }
	}
}
