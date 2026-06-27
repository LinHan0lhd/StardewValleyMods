using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000F8 RID: 248
	public class NPCDialogueResponse : Response
	{
		// Token: 0x0600142E RID: 5166 RVA: 0x000F4873 File Offset: 0x000F2A73
		public NPCDialogueResponse(string id, int friendshipChange, string keyToNPCresponse, string responseText, string extraArgument = null) : base(keyToNPCresponse, responseText)
		{
			this.friendshipChange = friendshipChange;
			this.id = id;
			this.extraArgument = extraArgument;
		}

		// Token: 0x0600142F RID: 5167 RVA: 0x000F4894 File Offset: 0x000F2A94
		public NPCDialogueResponse(NPCDialogueResponse other) : this(other.id, other.friendshipChange, other.responseKey, other.responseText, other.extraArgument)
		{
			if (other.hotkey != Keys.None)
			{
				base.SetHotKey(other.hotkey);
			}
		}

		// Token: 0x04000CA8 RID: 3240
		public int friendshipChange;

		// Token: 0x04000CA9 RID: 3241
		public string id;

		// Token: 0x04000CAA RID: 3242
		public string extraArgument;
	}
}
