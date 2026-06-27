using System;

namespace StardewValley.Menus
{
	// Token: 0x02000257 RID: 599
	public class ChatSnippet
	{
		// Token: 0x060027E3 RID: 10211 RVA: 0x001D0323 File Offset: 0x001CE523
		public ChatSnippet(string message, LocalizedContentManager.LanguageCode language)
		{
			this.message = message;
			this.myLength = ChatBox.messageFont(language).MeasureString(message).X;
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x001D0350 File Offset: 0x001CE550
		public ChatSnippet(int emojiIndex)
		{
			this.emojiIndex = emojiIndex;
			this.myLength = 40f;
		}

		// Token: 0x04001996 RID: 6550
		public string message;

		// Token: 0x04001997 RID: 6551
		public int emojiIndex = -1;

		// Token: 0x04001998 RID: 6552
		public float myLength;
	}
}
