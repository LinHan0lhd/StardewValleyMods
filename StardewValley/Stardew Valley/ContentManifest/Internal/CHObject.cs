using System;
using System.Collections.Generic;

namespace ContentManifest.Internal
{
	// Token: 0x02000079 RID: 121
	internal class CHObject : CHParsable
	{
		// Token: 0x06000482 RID: 1154 RVA: 0x00015520 File Offset: 0x00013720
		public void Parse(CHJsonParserContext context)
		{
			if (context.JsonText[context.ReadHead] != '{')
			{
				throw new InvalidOperationException();
			}
			context.ReadHead++;
			bool needsMember = false;
			char c;
			for (;;)
			{
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				c = context.JsonText[context.ReadHead];
				if (c != '"')
				{
					break;
				}
				CHString memberKey = new CHString();
				memberKey.Parse(context);
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] != ':')
				{
					goto Block_5;
				}
				context.ReadHead++;
				CHElement element = new CHElement();
				element.Parse(context);
				this.Members[memberKey.RawString] = element.Value.GetManagedObject();
				needsMember = false;
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] == ',')
				{
					context.ReadHead++;
					needsMember = true;
				}
			}
			if (c != '}')
			{
				throw new InvalidOperationException();
			}
			if (needsMember)
			{
				throw new InvalidOperationException();
			}
			context.ReadHead++;
			return;
			Block_5:
			throw new InvalidOperationException();
		}

		// Token: 0x040001AE RID: 430
		public readonly Dictionary<string, object> Members = new Dictionary<string, object>();
	}
}
