using System;
using System.Collections.Generic;

namespace ContentManifest.Internal
{
	// Token: 0x02000073 RID: 115
	internal class CHArray : CHParsable
	{
		// Token: 0x06000471 RID: 1137 RVA: 0x00014F74 File Offset: 0x00013174
		public void Parse(CHJsonParserContext context)
		{
			if (context.JsonText[context.ReadHead] != '[')
			{
				throw new InvalidOperationException();
			}
			context.ReadHead++;
			bool needsElement = false;
			CHArray.ElementList.Clear();
			for (;;)
			{
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] == ']')
				{
					break;
				}
				CHElement element = new CHElement();
				element.Parse(context);
				CHArray.ElementList.Add(element.Value.GetManagedObject());
				needsElement = false;
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] == ',')
				{
					context.ReadHead++;
					needsElement = true;
				}
			}
			if (needsElement)
			{
				throw new InvalidOperationException();
			}
			this.Elements = CHArray.ElementList.ToArray();
			context.ReadHead++;
		}

		// Token: 0x040001A5 RID: 421
		private static readonly List<object> ElementList = new List<object>();

		// Token: 0x040001A6 RID: 422
		public object[] Elements;
	}
}
