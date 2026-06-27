using System;
using System.Collections.Generic;
using StardewValley.Internal;

namespace StardewValley.Delegates
{
	// Token: 0x02000366 RID: 870
	// (Invoke) Token: 0x060035C8 RID: 13768
	public delegate IEnumerable<ItemQueryResult> ResolveItemQueryDelegate(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError);
}
