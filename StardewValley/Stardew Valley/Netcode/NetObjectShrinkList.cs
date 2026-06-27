using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x02000058 RID: 88
	public class NetObjectShrinkList<T> : AbstractNetSerializable, IList<!0>, ICollection<!0>, IEnumerable<!0>, IEnumerable, IEquatable<NetObjectShrinkList<!0>> where T : class, INetObject<INetSerializable>
	{
		// Token: 0x17000078 RID: 120
		public T this[int index]
		{
			get
			{
				int count = 0;
				for (int i = 0; i < this.array.Count; i++)
				{
					T v = this.array[i];
					if (v != null)
					{
						if (index == count)
						{
							return v;
						}
						count++;
					}
				}
				throw new ArgumentOutOfRangeException("index");
			}
			set
			{
				int count = 0;
				for (int i = 0; i < this.array.Count; i++)
				{
					if (this.array[i] != null)
					{
						if (index == count)
						{
							this.array[i] = value;
							return;
						}
						count++;
					}
				}
				throw new ArgumentOutOfRangeException("index");
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600038C RID: 908 RVA: 0x000115F8 File Offset: 0x0000F7F8
		public int Count
		{
			get
			{
				int count = 0;
				for (int i = 0; i < this.array.Count; i++)
				{
					if (this.array[i] != null)
					{
						count++;
					}
				}
				return count;
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600038D RID: 909 RVA: 0x00011635 File Offset: 0x0000F835
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00011638 File Offset: 0x0000F838
		public NetObjectShrinkList()
		{
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0001164C File Offset: 0x0000F84C
		public NetObjectShrinkList(IEnumerable<T> values) : this()
		{
			foreach (T value in values)
			{
				this.array.Add(value);
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x000116A0 File Offset: 0x0000F8A0
		public void Add(T item)
		{
			this.array.Add(item);
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000116B0 File Offset: 0x0000F8B0
		public void Clear()
		{
			for (int i = 0; i < this.array.Count; i++)
			{
				this.array[i] = default(T);
			}
		}

		// Token: 0x06000392 RID: 914 RVA: 0x000116E8 File Offset: 0x0000F8E8
		public void CopyFrom(IList<T> list)
		{
			if (list == this)
			{
				return;
			}
			if (list.Count > this.array.Count)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < this.array.Count; i++)
			{
				if (i < list.Count)
				{
					this.array[i] = list[i];
				}
				else
				{
					this.array[i] = default(T);
				}
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0001175C File Offset: 0x0000F95C
		public void Set(IList<T> list)
		{
			this.CopyFrom(list);
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00011768 File Offset: 0x0000F968
		public void MoveFrom(IList<T> list)
		{
			List<T> values = new List<T>(list);
			list.Clear();
			this.Set(values);
		}

		// Token: 0x06000395 RID: 917 RVA: 0x0001178C File Offset: 0x0000F98C
		public bool Contains(T item)
		{
			using (NetObjectShrinkList<T>.Enumerator enumerator = this.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000396 RID: 918 RVA: 0x000117E8 File Offset: 0x0000F9E8
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (this.Count - arrayIndex > array.Length)
			{
				throw new ArgumentException();
			}
			foreach (T value in this)
			{
				array[arrayIndex++] = value;
			}
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00011864 File Offset: 0x0000FA64
		public List<T> GetRange(int index, int count)
		{
			List<T> result = new List<T>();
			for (int i = index; i < index + count; i++)
			{
				result.Add(this[i]);
			}
			return result;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00011894 File Offset: 0x0000FA94
		public void AddRange(IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				this.Add(value);
			}
		}

		// Token: 0x06000399 RID: 921 RVA: 0x000118DC File Offset: 0x0000FADC
		public void RemoveRange(int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				this.RemoveAt(index);
			}
		}

		// Token: 0x0600039A RID: 922 RVA: 0x000118FC File Offset: 0x0000FAFC
		public bool Equals(NetObjectShrinkList<T> other)
		{
			if (this.Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00011947 File Offset: 0x0000FB47
		public NetObjectShrinkList<T>.Enumerator GetEnumerator()
		{
			return new NetObjectShrinkList<T>.Enumerator(this.array);
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00011954 File Offset: 0x0000FB54
		IEnumerator<T> IEnumerable<!0>.GetEnumerator()
		{
			return new NetObjectShrinkList<T>.Enumerator(this.array);
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00011966 File Offset: 0x0000FB66
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NetObjectShrinkList<T>.Enumerator(this.array);
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00011978 File Offset: 0x0000FB78
		public int IndexOf(T item)
		{
			int index = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				T v = this.array[i];
				if (v != null)
				{
					if (v == item)
					{
						return index;
					}
					index++;
				}
			}
			return -1;
		}

		// Token: 0x0600039F RID: 927 RVA: 0x000119C8 File Offset: 0x0000FBC8
		public void Insert(int index, T item)
		{
			int count = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				if (this.array[i] != null)
				{
					if (count == index)
					{
						this.array.Insert(i, item);
						return;
					}
					count++;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00011A20 File Offset: 0x0000FC20
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.array.Read(reader, version);
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x00011A2F File Offset: 0x0000FC2F
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.array.ReadFull(reader, version);
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x00011A40 File Offset: 0x0000FC40
		public bool Remove(T item)
		{
			for (int i = 0; i < this.array.Count; i++)
			{
				if (this.array[i] == item)
				{
					this.array[i] = default(T);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00011A94 File Offset: 0x0000FC94
		public void RemoveAt(int index)
		{
			int count = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				if (this.array[i] != null)
				{
					if (count == index)
					{
						this.array[i] = default(T);
						return;
					}
					count++;
				}
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00011AEA File Offset: 0x0000FCEA
		public override void Write(BinaryWriter writer)
		{
			this.array.Write(writer);
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00011AF8 File Offset: 0x0000FCF8
		public override void WriteFull(BinaryWriter writer)
		{
			this.array.WriteFull(writer);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00011B06 File Offset: 0x0000FD06
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(this.array);
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x00011B14 File Offset: 0x0000FD14
		public override string ToString()
		{
			return string.Join<T>(",", this);
		}

		// Token: 0x0400018A RID: 394
		private NetArray<T, NetRef<T>> array = new NetArray<T, NetRef<T>>();

		// Token: 0x020003EC RID: 1004
		public struct Enumerator : IEnumerator<!0>, IEnumerator, IDisposable
		{
			// Token: 0x060039FB RID: 14843 RVA: 0x002D7E2B File Offset: 0x002D602B
			public Enumerator(NetArray<T, NetRef<T>> array)
			{
				this._array = array;
				this._index = 0;
				this._current = default(T);
				this._done = false;
			}

			// Token: 0x060039FC RID: 14844 RVA: 0x002D7E50 File Offset: 0x002D6050
			public bool MoveNext()
			{
				while (this._index < this._array.Count)
				{
					T v = this._array[this._index];
					this._index++;
					if (v != null)
					{
						this._current = v;
						return true;
					}
				}
				this._done = true;
				this._current = default(T);
				return false;
			}

			// Token: 0x170004B5 RID: 1205
			// (get) Token: 0x060039FD RID: 14845 RVA: 0x002D7EB7 File Offset: 0x002D60B7
			public T Current
			{
				get
				{
					return this._current;
				}
			}

			// Token: 0x060039FE RID: 14846 RVA: 0x002D7EBF File Offset: 0x002D60BF
			public void Dispose()
			{
			}

			// Token: 0x170004B6 RID: 1206
			// (get) Token: 0x060039FF RID: 14847 RVA: 0x002D7EC1 File Offset: 0x002D60C1
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

			// Token: 0x06003A00 RID: 14848 RVA: 0x002D7EDC File Offset: 0x002D60DC
			void IEnumerator.Reset()
			{
				this._index = 0;
				this._current = default(T);
				this._done = false;
			}

			// Token: 0x040026C4 RID: 9924
			private readonly NetArray<T, NetRef<T>> _array;

			// Token: 0x040026C5 RID: 9925
			private int _index;

			// Token: 0x040026C6 RID: 9926
			private T _current;

			// Token: 0x040026C7 RID: 9927
			private bool _done;
		}
	}
}
