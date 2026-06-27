using System;

namespace ContentManifest.Internal
{
	// Token: 0x02000076 RID: 118
	internal class CHJson : CHParsable
	{
		// Token: 0x06000478 RID: 1144 RVA: 0x0001518F File Offset: 0x0001338F
		public void Parse(CHJsonParserContext context)
		{
			this.Element = new CHElement();
			this.Element.Parse(context);
		}

		// Token: 0x040001A9 RID: 425
		public CHElement Element;
	}
}
