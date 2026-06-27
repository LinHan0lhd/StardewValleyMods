using System;
using System.Collections.Generic;

namespace StardewValley.Util
{
	// Token: 0x0200011B RID: 283
	public static class DictionarySaver<TKey, TValue>
	{
		// Token: 0x060017BA RID: 6074 RVA: 0x00111B30 File Offset: 0x0010FD30
		public static SaveablePair<TKey, TValue>[] ArrayFrom(IDictionary<TKey, TValue> data)
		{
			SaveablePair<TKey, TValue>[] result = new SaveablePair<TKey, TValue>[(data != null) ? data.Count : 0];
			int i = 0;
			if (data != null)
			{
				foreach (KeyValuePair<TKey, TValue> entry in data)
				{
					result[i++] = new SaveablePair<TKey, TValue>(entry.Key, entry.Value);
				}
			}
			return result;
		}

		// Token: 0x060017BB RID: 6075 RVA: 0x00111BA8 File Offset: 0x0010FDA8
		public static SaveablePair<TKey, TValue>[] ArrayFrom<TSourceValue>(IDictionary<TKey, TSourceValue> data, Func<TSourceValue, TValue> getValue)
		{
			SaveablePair<TKey, TValue>[] result = new SaveablePair<TKey, TValue>[(data != null) ? data.Count : 0];
			int i = 0;
			if (data != null)
			{
				foreach (KeyValuePair<TKey, TSourceValue> entry in data)
				{
					result[i++] = new SaveablePair<TKey, TValue>(entry.Key, getValue(entry.Value));
				}
			}
			return result;
		}

		// Token: 0x060017BC RID: 6076 RVA: 0x00111C28 File Offset: 0x0010FE28
		public static SaveablePair<TKey, TValue>[] ArrayFrom<TSourceKey, TSourceValue>(IDictionary<TSourceKey, TSourceValue> data, Func<TSourceKey, TKey> getKey, Func<TSourceValue, TValue> getValue)
		{
			SaveablePair<TKey, TValue>[] result = new SaveablePair<TKey, TValue>[(data != null) ? data.Count : 0];
			int i = 0;
			if (data != null)
			{
				foreach (KeyValuePair<TSourceKey, TSourceValue> entry in data)
				{
					result[i++] = new SaveablePair<TKey, TValue>(getKey(entry.Key), getValue(entry.Value));
				}
			}
			return result;
		}
	}
}
