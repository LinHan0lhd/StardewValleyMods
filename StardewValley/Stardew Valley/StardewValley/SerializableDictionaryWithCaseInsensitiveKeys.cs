using System;
using System.Collections.Generic;

namespace StardewValley
{
	// Token: 0x020000FE RID: 254
	public class SerializableDictionaryWithCaseInsensitiveKeys<TValue> : SerializableDictionary<string, TValue>
	{
		// Token: 0x06001461 RID: 5217 RVA: 0x000F6A46 File Offset: 0x000F4C46
		public SerializableDictionaryWithCaseInsensitiveKeys() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		// Token: 0x06001462 RID: 5218 RVA: 0x000F6A53 File Offset: 0x000F4C53
		public SerializableDictionaryWithCaseInsensitiveKeys(IDictionary<string, TValue> data) : base(data, StringComparer.OrdinalIgnoreCase)
		{
		}
	}
}
