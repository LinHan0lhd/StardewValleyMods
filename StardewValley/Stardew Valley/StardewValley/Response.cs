using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000F7 RID: 247
	public class Response
	{
		// Token: 0x0600142C RID: 5164 RVA: 0x000F4853 File Offset: 0x000F2A53
		public Response(string responseKey, string responseText)
		{
			this.responseKey = responseKey;
			this.responseText = responseText;
		}

		// Token: 0x0600142D RID: 5165 RVA: 0x000F4869 File Offset: 0x000F2A69
		public Response SetHotKey(Keys key)
		{
			this.hotkey = key;
			return this;
		}

		// Token: 0x04000CA5 RID: 3237
		public string responseKey;

		// Token: 0x04000CA6 RID: 3238
		public string responseText;

		// Token: 0x04000CA7 RID: 3239
		public Keys hotkey;
	}
}
