using System;
using System.Collections.Generic;

namespace StardewValley.Util
{
	// Token: 0x0200011A RID: 282
	public static class SaveablePairExtensions
	{
		// Token: 0x060017B8 RID: 6072 RVA: 0x00111AE4 File Offset: 0x0010FCE4
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this SaveablePair<TKey, TValue>[] pairs)
		{
			Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
			if (pairs != null)
			{
				foreach (SaveablePair<TKey, TValue> pair in pairs)
				{
					result[pair.Key] = pair.Value;
				}
			}
			return result;
		}

		// Token: 0x060017B9 RID: 6073 RVA: 0x00111B27 File Offset: 0x0010FD27
		public static SaveablePair<TKey, TValue>[] ToSaveableArray<TKey, TValue>(this IDictionary<TKey, TValue> data)
		{
			return DictionarySaver<TKey, TValue>.ArrayFrom(data);
		}
	}
}
