using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x02000053 RID: 83
	public class NetList<T, TField> : AbstractNetSerializable, IList<!0>, ICollection<!0>, IEnumerable<!0>, IEnumerable, IEquatable<NetList<!0, !1>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x17000074 RID: 116
		public virtual T this[int index]
		{
			get
			{
				if (index >= this.Count || index < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				return this.array.Value[index];
			}
			set
			{
				if (index >= this.Count || index < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.array.Value[index] = value;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600034F RID: 847 RVA: 0x00010AE1 File Offset: 0x0000ECE1
		public int Count
		{
			get
			{
				return this.count.Value;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000350 RID: 848 RVA: 0x00010AEE File Offset: 0x0000ECEE
		public int Capacity
		{
			get
			{
				return this.array.Value.Count;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000351 RID: 849 RVA: 0x00010B00 File Offset: 0x0000ED00
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000352 RID: 850 RVA: 0x00010B04 File Offset: 0x0000ED04
		// (remove) Token: 0x06000353 RID: 851 RVA: 0x00010B3C File Offset: 0x0000ED3C
		public event NetList<T, TField>.ElementChangedEvent OnElementChanged;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000354 RID: 852 RVA: 0x00010B74 File Offset: 0x0000ED74
		// (remove) Token: 0x06000355 RID: 853 RVA: 0x00010BAC File Offset: 0x0000EDAC
		public event NetList<T, TField>.ArrayReplacedEvent OnArrayReplaced;

		// Token: 0x06000356 RID: 854 RVA: 0x00010BE4 File Offset: 0x0000EDE4
		public NetList()
		{
			this.hookArray(this.array.Value);
			this.array.fieldChangeVisibleEvent += delegate(NetRef<NetArray<T, TField>> arrayRef, NetArray<T, TField> oldArray, NetArray<T, TField> newArray)
			{
				if (newArray != null)
				{
					this.hookArray(newArray);
				}
				NetList<T, TField>.ArrayReplacedEvent onArrayReplaced = this.OnArrayReplaced;
				if (onArrayReplaced == null)
				{
					return;
				}
				onArrayReplaced(this, oldArray, newArray);
			};
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00010C4C File Offset: 0x0000EE4C
		public NetList(IEnumerable<T> values) : this()
		{
			foreach (T value in values)
			{
				this.Add(value);
			}
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00010C9C File Offset: 0x0000EE9C
		public NetList(int capacity) : this()
		{
			this.Resize(capacity);
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00010CAC File Offset: 0x0000EEAC
		private void hookField(int index, TField field)
		{
			if (field == default(TField))
			{
				return;
			}
			field.fieldChangeVisibleEvent += delegate(TField f, T oldValue, T newValue)
			{
				NetList<T, TField>.ElementChangedEvent onElementChanged = this.OnElementChanged;
				if (onElementChanged == null)
				{
					return;
				}
				onElementChanged(this, index, oldValue, newValue);
			};
		}

		// Token: 0x0600035A RID: 858 RVA: 0x00010CFC File Offset: 0x0000EEFC
		private void hookArray(NetArray<T, TField> array)
		{
			for (int i = 0; i < array.Count; i++)
			{
				this.hookField(i, array.Fields[i]);
			}
			array.OnFieldCreate += this.hookField;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00010D40 File Offset: 0x0000EF40
		private void Resize(int capacity)
		{
			this.count.Set(Math.Min(capacity, this.count.Value));
			NetArray<T, TField> oldArray = this.array.Value;
			NetArray<T, TField> newArray = new NetArray<T, TField>(capacity);
			this.array.Value = newArray;
			int i = 0;
			while (i < capacity && i < this.Count)
			{
				T tmp = oldArray[i];
				oldArray[i] = default(T);
				this.array.Value[i] = tmp;
				i++;
			}
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00010DC8 File Offset: 0x0000EFC8
		private void EnsureCapacity(int neededCapacity)
		{
			if (neededCapacity > this.Capacity)
			{
				int newCapacity = (int)((double)this.Capacity * 1.5);
				while (neededCapacity > newCapacity)
				{
					newCapacity = (int)((double)newCapacity * 1.5);
				}
				this.Resize(newCapacity);
			}
		}

		// Token: 0x0600035D RID: 861 RVA: 0x00010E0C File Offset: 0x0000F00C
		public virtual void Add(T item)
		{
			this.EnsureCapacity(this.Count + 1);
			this.array.Value[this.Count] = item;
			this.count.Set(this.count.Value + 1);
		}

		// Token: 0x0600035E RID: 862 RVA: 0x00010E4B File Offset: 0x0000F04B
		public virtual void Clear()
		{
			this.count.Set(0);
			this.Resize(10);
			this.fillNull();
		}

		// Token: 0x0600035F RID: 863 RVA: 0x00010E68 File Offset: 0x0000F068
		private void fillNull()
		{
			for (int i = 0; i < this.Capacity; i++)
			{
				this.array.Value[i] = default(T);
			}
		}

		// Token: 0x06000360 RID: 864 RVA: 0x00010EA0 File Offset: 0x0000F0A0
		public virtual void CopyFrom(IList<T> list)
		{
			if (list == this)
			{
				return;
			}
			this.EnsureCapacity(list.Count);
			this.fillNull();
			this.count.Set(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				this.array.Value[i] = list[i];
			}
		}

		// Token: 0x06000361 RID: 865 RVA: 0x00010EFE File Offset: 0x0000F0FE
		public void Set(IList<T> list)
		{
			this.CopyFrom(list);
		}

		// Token: 0x06000362 RID: 866 RVA: 0x00010F08 File Offset: 0x0000F108
		public void MoveFrom(NetList<T, TField> list)
		{
			List<T> values = new List<T>(list);
			list.Clear();
			this.Set(values);
		}

		// Token: 0x06000363 RID: 867 RVA: 0x00010F29 File Offset: 0x0000F129
		public bool Any()
		{
			return this.count.Value > 0;
		}

		// Token: 0x06000364 RID: 868 RVA: 0x00010F3C File Offset: 0x0000F13C
		public virtual bool Contains(T item)
		{
			using (NetList<T, TField>.Enumerator enumerator = this.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (object.Equals(enumerator.Current, item))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000365 RID: 869 RVA: 0x00010F9C File Offset: 0x0000F19C
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

		// Token: 0x06000366 RID: 870 RVA: 0x00011018 File Offset: 0x0000F218
		public List<T> GetRange(int index, int count)
		{
			List<T> result = new List<T>();
			for (int i = index; i < index + count; i++)
			{
				result.Add(this[i]);
			}
			return result;
		}

		// Token: 0x06000367 RID: 871 RVA: 0x00011048 File Offset: 0x0000F248
		public void AddRange(IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				this.Add(value);
			}
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00011090 File Offset: 0x0000F290
		public void RemoveRange(int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				this.RemoveAt(index);
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x000110B0 File Offset: 0x0000F2B0
		public bool Equals(NetList<T, TField> other)
		{
			return object.Equals(this.array, other.array);
		}

		// Token: 0x0600036A RID: 874 RVA: 0x000110C3 File Offset: 0x0000F2C3
		public NetList<T, TField>.Enumerator GetEnumerator()
		{
			return new NetList<T, TField>.Enumerator(this);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x000110CB File Offset: 0x0000F2CB
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NetList<T, TField>.Enumerator(this);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x000110D8 File Offset: 0x0000F2D8
		IEnumerator<T> IEnumerable<!0>.GetEnumerator()
		{
			return new NetList<T, TField>.Enumerator(this);
		}

		// Token: 0x0600036D RID: 877 RVA: 0x000110E8 File Offset: 0x0000F2E8
		public virtual int IndexOf(T item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (object.Equals(this.array.Value[i], item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0001112C File Offset: 0x0000F32C
		public virtual void Insert(int index, T item)
		{
			if (index > this.Count || index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.EnsureCapacity(this.Count + 1);
			this.count.Set(this.count.Value + 1);
			for (int i = this.Count - 1; i > index; i--)
			{
				T tmp = this.array.Value[i - 1];
				this.array.Value[i - 1] = default(T);
				this.array.Value[i] = tmp;
			}
			this.array.Value[index] = item;
		}

		// Token: 0x0600036F RID: 879 RVA: 0x000111D9 File Offset: 0x0000F3D9
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.count.Read(reader, version);
			this.array.Read(reader, version);
		}

		// Token: 0x06000370 RID: 880 RVA: 0x000111F5 File Offset: 0x0000F3F5
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.count.ReadFull(reader, version);
			this.array.ReadFull(reader, version);
		}

		// Token: 0x06000371 RID: 881 RVA: 0x00011214 File Offset: 0x0000F414
		public bool Remove(T item)
		{
			int index = this.IndexOf(item);
			if (index != -1)
			{
				this.RemoveAt(index);
				return true;
			}
			return false;
		}

		// Token: 0x06000372 RID: 882 RVA: 0x00011238 File Offset: 0x0000F438
		public virtual void RemoveAt(int index)
		{
			if (index < 0 || index >= this.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.count.Set(this.count.Value - 1);
			for (int i = index; i < this.Count; i++)
			{
				T tmp = this.array.Value[i + 1];
				this.array.Value[i + 1] = default(T);
				this.array.Value[i] = tmp;
			}
			this.array.Value[this.Count] = default(T);
		}

		// Token: 0x06000373 RID: 883 RVA: 0x000112E4 File Offset: 0x0000F4E4
		public int RemoveWhere(Func<T, bool> match)
		{
			int count = 0;
			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (match(this[i]))
				{
					this.RemoveAt(i);
					count++;
				}
			}
			return count;
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00011324 File Offset: 0x0000F524
		[Obsolete("Use RemoveWhere instead.")]
		public void Filter(Func<T, bool> f)
		{
			this.RemoveWhere((T pair) => !f(pair));
		}

		// Token: 0x06000375 RID: 885 RVA: 0x00011351 File Offset: 0x0000F551
		public override void Write(BinaryWriter writer)
		{
			this.count.Write(writer);
			this.array.Write(writer);
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0001136B File Offset: 0x0000F56B
		public override void WriteFull(BinaryWriter writer)
		{
			this.count.WriteFull(writer);
			this.array.WriteFull(writer);
		}

		// Token: 0x06000377 RID: 887 RVA: 0x00011385 File Offset: 0x0000F585
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(this.count);
			childAction(this.array);
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0001139F File Offset: 0x0000F59F
		public override string ToString()
		{
			return string.Join<T>(",", this);
		}

		// Token: 0x04000184 RID: 388
		private const int initialSize = 10;

		// Token: 0x04000185 RID: 389
		private const double resizeFactor = 1.5;

		// Token: 0x04000186 RID: 390
		protected readonly NetInt count = new NetInt(0).Interpolated(false, false);

		// Token: 0x04000187 RID: 391
		protected readonly NetRef<NetArray<T, TField>> array = new NetRef<NetArray<T, TField>>(new NetArray<T, TField>(10)).Interpolated(false, false);

		// Token: 0x020003E7 RID: 999
		// (Invoke) Token: 0x060039EA RID: 14826
		public delegate void ElementChangedEvent(NetList<T, TField> list, int index, T oldValue, T newValue);

		// Token: 0x020003E8 RID: 1000
		// (Invoke) Token: 0x060039EE RID: 14830
		public delegate void ArrayReplacedEvent(NetList<T, TField> list, IList<T> before, IList<T> after);

		// Token: 0x020003E9 RID: 1001
		public struct Enumerator : IEnumerator<!0>, IEnumerator, IDisposable
		{
			// Token: 0x060039F1 RID: 14833 RVA: 0x002D7D14 File Offset: 0x002D5F14
			public Enumerator(NetList<T, TField> list)
			{
				this._list = list;
				this._index = 0;
				this._current = default(T);
				this._done = false;
			}

			// Token: 0x060039F2 RID: 14834 RVA: 0x002D7D38 File Offset: 0x002D5F38
			public bool MoveNext()
			{
				int count = this._list.count.Value;
				if (this._index < count)
				{
					this._current = this._list.array.Value[this._index];
					this._index++;
					return true;
				}
				this._done = true;
				this._current = default(T);
				return false;
			}

			// Token: 0x170004B3 RID: 1203
			// (get) Token: 0x060039F3 RID: 14835 RVA: 0x002D7DA4 File Offset: 0x002D5FA4
			public T Current
			{
				get
				{
					return this._current;
				}
			}

			// Token: 0x060039F4 RID: 14836 RVA: 0x002D7DAC File Offset: 0x002D5FAC
			public void Dispose()
			{
			}

			// Token: 0x170004B4 RID: 1204
			// (get) Token: 0x060039F5 RID: 14837 RVA: 0x002D7DAE File Offset: 0x002D5FAE
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

			// Token: 0x060039F6 RID: 14838 RVA: 0x002D7DC9 File Offset: 0x002D5FC9
			void IEnumerator.Reset()
			{
				this._index = 0;
				this._current = default(T);
				this._done = false;
			}

			// Token: 0x040026BD RID: 9917
			private readonly NetList<T, TField> _list;

			// Token: 0x040026BE RID: 9918
			private int _index;

			// Token: 0x040026BF RID: 9919
			private T _current;

			// Token: 0x040026C0 RID: 9920
			private bool _done;
		}
	}
}
