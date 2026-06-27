using System;

namespace ContentManifest.Internal
{
	// Token: 0x02000077 RID: 119
	internal class CHJsonParserContext
	{
		// Token: 0x0600047A RID: 1146 RVA: 0x000151B0 File Offset: 0x000133B0
		public CHJsonParserContext(string jsonText)
		{
			this.JsonText = jsonText;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x000151CC File Offset: 0x000133CC
		public void SkipWhitespace()
		{
			while (this.ReadHead < this.JsonText.Length)
			{
				char c = this.JsonText[this.ReadHead];
				switch (c)
				{
				case '\t':
				case '\n':
				case '\r':
					break;
				case '\v':
				case '\f':
					return;
				default:
					if (c != ' ')
					{
						return;
					}
					break;
				}
				this.ReadHead++;
			}
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00015233 File Offset: 0x00013433
		public void AssertReadHeadIsValid()
		{
			if (this.ReadHead < 0 || this.ReadHead >= this.JsonText.Length)
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x040001AA RID: 426
		public int ReadHead;

		// Token: 0x040001AB RID: 427
		public string JsonText = "";
	}
}
