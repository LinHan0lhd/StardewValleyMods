using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Extensions
{
	// Token: 0x0200031A RID: 794
	public static class CollectionExtensions
	{
		// Token: 0x0600344A RID: 13386 RVA: 0x0029C4D8 File Offset: 0x0029A6D8
		public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> match)
		{
			if (dictionary.Count == 0)
			{
				return 0;
			}
			int removed = 0;
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				if (match(pair))
				{
					dictionary.Remove(pair.Key);
					removed++;
				}
			}
			return removed;
		}

		// Token: 0x0600344B RID: 13387 RVA: 0x0029C544 File Offset: 0x0029A744
		public static int TryAddMany<TKey, TValue>(this IDictionary<TKey, TValue> dict, Dictionary<TKey, TValue> values)
		{
			if (values == null)
			{
				return 0;
			}
			int added = 0;
			foreach (KeyValuePair<TKey, TValue> pair in values)
			{
				if (dict.TryAdd(pair.Key, pair.Value))
				{
					added++;
				}
			}
			return added;
		}

		// Token: 0x0600344C RID: 13388 RVA: 0x0029C5B0 File Offset: 0x0029A7B0
		public static int RemoveWhere<T>(this IList<T> list, Predicate<T> match)
		{
			List<T> concreteList = list as List<T>;
			if (concreteList != null)
			{
				return concreteList.RemoveAll(match);
			}
			int count = 0;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (match(list[i]))
				{
					list.RemoveAt(i);
					count++;
				}
			}
			return count;
		}

		// Token: 0x0600344D RID: 13389 RVA: 0x0029C5FF File Offset: 0x0029A7FF
		public static void Toggle<T>(this ISet<T> set, T value, bool add)
		{
			if (add)
			{
				set.Add(value);
				return;
			}
			set.Remove(value);
		}

		// Token: 0x0600344E RID: 13390 RVA: 0x0029C618 File Offset: 0x0029A818
		public static int AddRange<T>(this ISet<T> set, IEnumerable<T> values)
		{
			if (values == null)
			{
				return 0;
			}
			int added = 0;
			foreach (T value in values)
			{
				if (set.Add(value))
				{
					added++;
				}
			}
			return added;
		}

		// Token: 0x0600344F RID: 13391 RVA: 0x0029C670 File Offset: 0x0029A870
		public static int RemoveWhere<T>(this ISet<T> set, Predicate<T> match)
		{
			HashSet<T> hashSet = set as HashSet<T>;
			if (hashSet != null)
			{
				return hashSet.RemoveWhere(match);
			}
			NetHashSet<T> netHashSet = set as NetHashSet<T>;
			if (netHashSet != null)
			{
				return netHashSet.RemoveWhere(match);
			}
			List<T> removed = null;
			foreach (T value in set)
			{
				if (match(value))
				{
					if (removed == null)
					{
						removed = new List<T>();
					}
					removed.Add(value);
				}
			}
			if (removed != null)
			{
				foreach (T value2 in removed)
				{
					set.Remove(value2);
				}
				return removed.Count;
			}
			return 0;
		}
	}
}
