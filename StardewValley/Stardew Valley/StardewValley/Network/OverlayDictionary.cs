using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	// Token: 0x020001F1 RID: 497
	public class OverlayDictionary<TKey, TValue> : IDictionary<!0, !1>, ICollection<KeyValuePair<!0, !1>>, IEnumerable<KeyValuePair<!0, !1>>, IEnumerable
	{
		// Token: 0x1400001D RID: 29
		// (add) Token: 0x0600223B RID: 8763 RVA: 0x001760E4 File Offset: 0x001742E4
		// (remove) Token: 0x0600223C RID: 8764 RVA: 0x0017611C File Offset: 0x0017431C
		public event Action<TKey, TValue> onValueAdded;

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x0600223D RID: 8765 RVA: 0x00176154 File Offset: 0x00174354
		// (remove) Token: 0x0600223E RID: 8766 RVA: 0x0017618C File Offset: 0x0017438C
		public event Action<TKey, TValue> onValueRemoved;

		// Token: 0x0600223F RID: 8767 RVA: 0x001761C1 File Offset: 0x001743C1
		public OverlayDictionary()
		{
			this._dictionary = new Dictionary<TKey, TValue>();
		}

		// Token: 0x06002240 RID: 8768 RVA: 0x001761DF File Offset: 0x001743DF
		public OverlayDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			this._dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
		}

		// Token: 0x06002241 RID: 8769 RVA: 0x001761FF File Offset: 0x001743FF
		public OverlayDictionary(IEqualityComparer<TKey> comparer)
		{
			this._dictionary = new Dictionary<TKey, TValue>(comparer);
		}

		// Token: 0x170003BC RID: 956
		public TValue this[TKey key]
		{
			get
			{
				return this._dictionary[key];
			}
			set
			{
				this._dictionary[key] = value;
				Action<TKey, TValue> action = this.onValueAdded;
				if (action == null)
				{
					return;
				}
				action(key, value);
			}
		}

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06002244 RID: 8772 RVA: 0x0017624D File Offset: 0x0017444D
		public ICollection<TKey> Keys
		{
			get
			{
				return this._dictionary.Keys;
			}
		}

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06002245 RID: 8773 RVA: 0x0017625A File Offset: 0x0017445A
		public ICollection<TValue> Values
		{
			get
			{
				return this._dictionary.Values;
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06002246 RID: 8774 RVA: 0x00176267 File Offset: 0x00174467
		public int Count
		{
			get
			{
				return this._dictionary.Count;
			}
		}

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06002247 RID: 8775 RVA: 0x00176274 File Offset: 0x00174474
		public bool IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).IsReadOnly;
			}
		}

		// Token: 0x06002248 RID: 8776 RVA: 0x00176281 File Offset: 0x00174481
		public void Add(TKey key, TValue value)
		{
			this._dictionary.Add(key, value);
			Action<TKey, TValue> action = this.onValueAdded;
			if (action == null)
			{
				return;
			}
			action(key, value);
		}

		// Token: 0x06002249 RID: 8777 RVA: 0x001762A2 File Offset: 0x001744A2
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		// Token: 0x0600224A RID: 8778 RVA: 0x001762B8 File Offset: 0x001744B8
		public void Clear()
		{
			this._removedPairs.AddRange(this._dictionary);
			((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Clear();
			foreach (KeyValuePair<TKey, TValue> pair in this._removedPairs)
			{
				this.onValueRemoved(pair.Key, pair.Value);
			}
			this._removedPairs.Clear();
		}

		// Token: 0x0600224B RID: 8779 RVA: 0x00176344 File Offset: 0x00174544
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Contains(item);
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x00176352 File Offset: 0x00174552
		public bool ContainsKey(TKey key)
		{
			return this._dictionary.ContainsKey(key);
		}

		// Token: 0x0600224D RID: 8781 RVA: 0x00176360 File Offset: 0x00174560
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
		}

		// Token: 0x0600224E RID: 8782 RVA: 0x0017636F File Offset: 0x0017456F
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this._dictionary.GetEnumerator();
		}

		// Token: 0x0600224F RID: 8783 RVA: 0x00176384 File Offset: 0x00174584
		public bool Remove(TKey key)
		{
			TValue value;
			if (this._dictionary.TryGetValue(key, out value))
			{
				this._dictionary.Remove(key);
				Action<TKey, TValue> action = this.onValueRemoved;
				if (action != null)
				{
					action(key, value);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002250 RID: 8784 RVA: 0x001763C4 File Offset: 0x001745C4
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return this.Contains(item) && this.Remove(item.Key);
		}

		// Token: 0x06002251 RID: 8785 RVA: 0x001763DE File Offset: 0x001745DE
		public bool TryGetValue(TKey key, out TValue value)
		{
			return this._dictionary.TryGetValue(key, out value);
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x001763ED File Offset: 0x001745ED
		public TValue GetValueOrDefault(TKey key, TValue defaultValue = default(TValue))
		{
			return this._dictionary.GetValueOrDefault(key, defaultValue);
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x001763FC File Offset: 0x001745FC
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._dictionary.GetEnumerator();
		}

		// Token: 0x0400145F RID: 5215
		protected Dictionary<TKey, TValue> _dictionary;

		// Token: 0x04001460 RID: 5216
		protected List<KeyValuePair<TKey, TValue>> _removedPairs = new List<KeyValuePair<TKey, TValue>>();
	}
}
