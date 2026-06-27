using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x0200018A RID: 394
	public class NetDescriptionElementList : NetList<DescriptionElement, NetDescriptionElementRef>
	{
		// Token: 0x06001C8F RID: 7311 RVA: 0x00146D02 File Offset: 0x00144F02
		public NetDescriptionElementList()
		{
		}

		// Token: 0x06001C90 RID: 7312 RVA: 0x00146D0A File Offset: 0x00144F0A
		public NetDescriptionElementList(IEnumerable<DescriptionElement> values) : base(values)
		{
		}

		// Token: 0x06001C91 RID: 7313 RVA: 0x00146D13 File Offset: 0x00144F13
		public NetDescriptionElementList(int capacity) : base(capacity)
		{
		}

		// Token: 0x06001C92 RID: 7314 RVA: 0x00146D1C File Offset: 0x00144F1C
		public void Add(string key)
		{
			this.Add(new DescriptionElement(key, Array.Empty<object>()));
		}
	}
}
