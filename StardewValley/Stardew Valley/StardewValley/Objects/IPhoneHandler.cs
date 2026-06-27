using System;
using System.Collections.Generic;

namespace StardewValley.Objects
{
	// Token: 0x020001BE RID: 446
	public interface IPhoneHandler
	{
		// Token: 0x06001FCA RID: 8138
		string CheckForIncomingCall(Random random);

		// Token: 0x06001FCB RID: 8139
		bool TryHandleIncomingCall(string callId, out Action showDialogue);

		// Token: 0x06001FCC RID: 8140
		IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers();

		// Token: 0x06001FCD RID: 8141
		bool TryHandleOutgoingCall(string callId);
	}
}
