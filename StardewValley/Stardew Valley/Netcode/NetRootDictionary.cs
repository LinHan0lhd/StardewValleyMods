using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Netcode
{
	// Token: 0x0200005E RID: 94
	public class NetRootDictionary<TKey, TValue> : IDictionary<!0, !1>, ICollection<KeyValuePair<!0, !1>>, IEnumerable<KeyValuePair<!0, !1>>, IEnumerable where TValue : class, INetObject<INetSerializable>
	{
		// Token: 0x060003E1 RID: 993 RVA: 0x0001272F File Offset: 0x0001092F
		public NetRootDictionary()
		{
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00012744 File Offset: 0x00010944
		public NetRootDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			foreach (KeyValuePair<TKey, TValue> pair in values)
			{
				this.Add(pair.Key, pair.Value);
			}
		}

		// Token: 0x1700007D RID: 125
		public TValue this[TKey key]
		{
			get
			{
				return this.Roots[key].Get();
			}
			set
			{
				if (!this.ContainsKey(key))
				{
					this.Add(key, value);
					return;
				}
				this.Roots[key].Set(value);
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060003E5 RID: 997 RVA: 0x000127E5 File Offset: 0x000109E5
		public int Count
		{
			get
			{
				return this.Roots.Count;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x000127F2 File Offset: 0x000109F2
		public bool IsReadOnly
		{
			get
			{
				return ((IDictionary)this.Roots).IsReadOnly;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060003E7 RID: 999 RVA: 0x000127FF File Offset: 0x000109FF
		public ICollection<TKey> Keys
		{
			get
			{
				return this.Roots.Keys;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060003E8 RID: 1000 RVA: 0x0001280C File Offset: 0x00010A0C
		public ICollection<TValue> Values
		{
			get
			{
				return (from root in this.Roots.Values
				select root.Get()).ToList<TValue>();
			}
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00012842 File Offset: 0x00010A42
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00012858 File Offset: 0x00010A58
		public void Add(TKey key, TValue value)
		{
			NetRoot<TValue> root = new NetRoot<TValue>(value);
			root.Serializer = this.Serializer;
			this.Roots.Add(key, root);
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x00012885 File Offset: 0x00010A85
		public void Clear()
		{
			this.Roots.Clear();
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00012894 File Offset: 0x00010A94
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			NetRoot<TValue> root;
			return this.Roots.TryGetValue(item.Key, out root) && root == item.Value;
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x000128C8 File Offset: 0x00010AC8
		public bool ContainsKey(TKey key)
		{
			return this.Roots.ContainsKey(key);
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x000128D8 File Offset: 0x00010AD8
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (array.Length < this.Count - arrayIndex)
			{
				throw new ArgumentException();
			}
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				array[arrayIndex++] = pair;
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x00012954 File Offset: 0x00010B54
		public NetRootDictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return new NetRootDictionary<TKey, TValue>.Enumerator(this.Roots);
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x00012961 File Offset: 0x00010B61
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<!0, !1>>.GetEnumerator()
		{
			return new NetRootDictionary<TKey, TValue>.Enumerator(this.Roots);
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x00012973 File Offset: 0x00010B73
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NetRootDictionary<TKey, TValue>.Enumerator(this.Roots);
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00012985 File Offset: 0x00010B85
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return this.Contains(item) && this.Remove(item.Key);
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0001299F File Offset: 0x00010B9F
		public bool Remove(TKey key)
		{
			return this.Roots.Remove(key);
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x000129B0 File Offset: 0x00010BB0
		public bool TryGetValue(TKey key, out TValue value)
		{
			NetRoot<TValue> root;
			if (this.Roots.TryGetValue(key, out root))
			{
				value = root.Get();
				return true;
			}
			value = default(TValue);
			return false;
		}

		// Token: 0x04000192 RID: 402
		public XmlSerializer Serializer;

		// Token: 0x04000193 RID: 403
		public Dictionary<TKey, NetRoot<TValue>> Roots = new Dictionary<TKey, NetRoot<TValue>>();

		// Token: 0x020003F1 RID: 1009
		public struct Enumerator : IEnumerator<KeyValuePair<!0, !1>>, IEnumerator, IDisposable
		{
			// Token: 0x06003A0B RID: 14859 RVA: 0x002D80E0 File Offset: 0x002D62E0
			public Enumerator(Dictionary<TKey, NetRoot<TValue>> roots)
			{
				this._roots = roots;
				this._enumerator = this._roots.GetEnumerator();
				this._current = default(KeyValuePair<TKey, TValue>);
				this._done = false;
			}

			// Token: 0x06003A0C RID: 14860 RVA: 0x002D8110 File Offset: 0x002D6310
			public bool MoveNext()
			{
				if (!this._enumerator.MoveNext())
				{
					this._done = true;
					this._current = default(KeyValuePair<TKey, TValue>);
					return false;
				}
				KeyValuePair<TKey, NetRoot<TValue>> pair = this._enumerator.Current;
				this._current = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Get());
				return true;
			}

			// Token: 0x170004B7 RID: 1207
			// (get) Token: 0x06003A0D RID: 14861 RVA: 0x002D816C File Offset: 0x002D636C
			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					return this._current;
				}
			}

			// Token: 0x06003A0E RID: 14862 RVA: 0x002D8174 File Offset: 0x002D6374
			public void Dispose()
			{
			}

			// Token: 0x170004B8 RID: 1208
			// (get) Token: 0x06003A0F RID: 14863 RVA: 0x002D8176 File Offset: 0x002D6376
			object IEnumerator.Current
			{
				get
				{
					if (this._done)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			// Token: 0x06003A10 RID: 14864 RVA: 0x002D8191 File Offset: 0x002D6391
			void IEnumerator.Reset()
			{
				this._enumerator = this._roots.GetEnumerator();
				this._current = default(KeyValuePair<TKey, TValue>);
				this._done = false;
			}

			// Token: 0x040026D0 RID: 9936
			private Dictionary<TKey, NetRoot<TValue>> _roots;

			// Token: 0x040026D1 RID: 9937
			private Dictionary<TKey, NetRoot<TValue>>.Enumerator _enumerator;

			// Token: 0x040026D2 RID: 9938
			private KeyValuePair<TKey, TValue> _current;

			// Token: 0x040026D3 RID: 9939
			private bool _done;
		}
	}
}
