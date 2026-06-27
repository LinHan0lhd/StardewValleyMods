using System;

namespace StardewValley.Network.NetReady.Internal
{
	// Token: 0x020001F6 RID: 502
	internal enum ReadyCheckMessageType : byte
	{
		// Token: 0x0400146D RID: 5229
		Ready,
		// Token: 0x0400146E RID: 5230
		Cancel,
		// Token: 0x0400146F RID: 5231
		Lock,
		// Token: 0x04001470 RID: 5232
		Release,
		// Token: 0x04001471 RID: 5233
		AcceptLock,
		// Token: 0x04001472 RID: 5234
		RejectLock,
		// Token: 0x04001473 RID: 5235
		UpdateAmounts,
		// Token: 0x04001474 RID: 5236
		RequireFarmers,
		// Token: 0x04001475 RID: 5237
		Finish
	}
}
