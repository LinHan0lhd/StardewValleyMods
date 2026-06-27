using System;

namespace ContentManifest.Internal
{
	// Token: 0x02000075 RID: 117
	internal class CHElement : CHParsable
	{
		// Token: 0x06000476 RID: 1142 RVA: 0x00015162 File Offset: 0x00013362
		public void Parse(CHJsonParserContext context)
		{
			context.SkipWhitespace();
			this.Value = new CHValue();
			this.Value.Parse(context);
			context.SkipWhitespace();
		}

		// Token: 0x040001A8 RID: 424
		public CHValue Value;
	}
}
