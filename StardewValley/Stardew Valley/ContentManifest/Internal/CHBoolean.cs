using System;

namespace ContentManifest.Internal
{
	// Token: 0x02000074 RID: 116
	internal class CHBoolean : CHParsable
	{
		// Token: 0x06000474 RID: 1140 RVA: 0x00015070 File Offset: 0x00013270
		public void Parse(CHJsonParserContext context)
		{
			string jsonText = context.JsonText;
			int readHead = context.ReadHead;
			char c = jsonText[readHead];
			if (c != 'f')
			{
				if (c != 't')
				{
					throw new NotImplementedException();
				}
				if (readHead + 3 >= jsonText.Length)
				{
					throw new InvalidOperationException();
				}
				if (jsonText[readHead + 1] != 'r' || jsonText[readHead + 2] != 'u' || jsonText[readHead + 3] != 'e')
				{
					throw new InvalidOperationException();
				}
				context.ReadHead += 4;
				this.RawBoolean = true;
				return;
			}
			else
			{
				if (readHead + 4 >= jsonText.Length)
				{
					throw new InvalidOperationException();
				}
				if (jsonText[readHead + 1] != 'a' || jsonText[readHead + 2] != 'l' || jsonText[readHead + 3] != 's' || jsonText[readHead + 4] != 'e')
				{
					throw new InvalidOperationException();
				}
				context.ReadHead += 5;
				this.RawBoolean = false;
				return;
			}
		}

		// Token: 0x040001A7 RID: 423
		public bool RawBoolean;
	}
}
