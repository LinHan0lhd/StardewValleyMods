using System;

namespace StardewValley
{
	// Token: 0x0200009B RID: 155
	public class DialogueLine
	{
		// Token: 0x170000DB RID: 219
		// (get) Token: 0x060006EC RID: 1772 RVA: 0x0002A327 File Offset: 0x00028527
		public bool HasText
		{
			get
			{
				return !string.IsNullOrEmpty(this.Text) && this.Text != "{";
			}
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x0002A348 File Offset: 0x00028548
		public DialogueLine(string text, Action sideEffects = null)
		{
			this.Text = (text ?? "");
			this.SideEffects = sideEffects;
		}

		// Token: 0x0400039E RID: 926
		public string Text;

		// Token: 0x0400039F RID: 927
		public Action SideEffects;
	}
}
