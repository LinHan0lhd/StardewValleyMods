using System;
using ContentManifest.Internal;

namespace ContentManifest
{
	// Token: 0x02000071 RID: 113
	public class CHJsonParser
	{
		// Token: 0x0600046D RID: 1133 RVA: 0x00014F20 File Offset: 0x00013120
		public static object ParseJson(string text)
		{
			CHJsonParserContext context = new CHJsonParserContext(text);
			CHJson chjson = new CHJson();
			chjson.Parse(context);
			return chjson.Element.Value.GetManagedObject();
		}
	}
}
