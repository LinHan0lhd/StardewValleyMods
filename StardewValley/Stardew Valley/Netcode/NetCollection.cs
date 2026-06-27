using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x02000031 RID: 49
	public sealed class NetCollection<T> : AbstractNetSerializable, IList<!0>, ICollection<!0>, IEnumerable<!0>, IEnumerable, IEquatable<NetCollection<!0>> where T : class, INetObject<INetSerializable>
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x0000C043 File Offset: 0x0000A243
		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x0000C050 File Offset: 0x0000A250
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x0000C053 File Offset: 0x0000A253
		// (set) Token: 0x060001AA RID: 426 RVA: 0x0000C060 File Offset: 0x0000A260
		public bool InterpolationWait
		{
			get
			{
				return this.elements.InterpolationWait;
			}
			set
			{
				this.elements.InterpolationWait = value;
			}
		}

		// Token: 0x1700004F RID: 79
		public T this[int index]
		{
			get
			{
				return this.list[index];
			}
			set
			{
				this.elements[this.guids[index]] = value;
			}
		}

		// Token: 0x17000050 RID: 80
		public T this[Guid guid]
		{
			get
			{
				return this.elements[guid];
			}
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x060001AE RID: 430 RVA: 0x0000C0A4 File Offset: 0x0000A2A4
		// (remove) Token: 0x060001AF RID: 431 RVA: 0x0000C0DC File Offset: 0x0000A2DC
		public event NetCollection<T>.ContentsChangeEvent OnValueAdded;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060001B0 RID: 432 RVA: 0x0000C114 File Offset: 0x0000A314
		// (remove) Token: 0x060001B1 RID: 433 RVA: 0x0000C14C File Offset: 0x0000A34C
		public event NetCollection<T>.ContentsChangeEvent OnValueRemoved;

		// Token: 0x060001B2 RID: 434 RVA: 0x0000C184 File Offset: 0x0000A384
		public NetCollection()
		{
			this.elements.OnValueTargetUpdated += delegate(Guid guid, T old_target_value, T new_target_value)
			{
				if (old_target_value == new_target_value)
				{
					return;
				}
				int index = this.guids.IndexOf(guid);
				if (index == -1)
				{
					this.guids.Add(guid);
					this.list.Add(new_target_value);
					return;
				}
				this.list[index] = new_target_value;
			};
			this.elements.OnValueAdded += delegate(Guid guid, T value)
			{
				int index = this.guids.IndexOf(guid);
				if (index == -1)
				{
					this.guids.Add(guid);
					this.list.Add(value);
				}
				else
				{
					this.list[index] = value;
				}
				NetCollection<T>.ContentsChangeEvent onValueAdded = this.OnValueAdded;
				if (onValueAdded == null)
				{
					return;
				}
				onValueAdded(value);
			};
			this.elements.OnValueRemoved += delegate(Guid guid, T value)
			{
				int index = this.guids.IndexOf(guid);
				if (index != -1)
				{
					this.guids.RemoveAt(index);
					this.list.RemoveAt(index);
				}
				NetCollection<T>.ContentsChangeEvent onValueRemoved = this.OnValueRemoved;
				if (onValueRemoved == null)
				{
					return;
				}
				onValueRemoved(value);
			};
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000C200 File Offset: 0x0000A400
		public NetCollection(IEnumerable<T> values) : this()
		{
			foreach (T value in values)
			{
				this.Add(value);
			}
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000C250 File Offset: 0x0000A450
		public bool TryGetValue(Guid id, out T value)
		{
			return this.elements.TryGetValue(id, out value);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000C260 File Offset: 0x0000A460
		public void Add(T item)
		{
			Guid key = Guid.NewGuid();
			this.elements.Add(key, item);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000C280 File Offset: 0x0000A480
		public bool Equals(NetCollection<T> other)
		{
			return this.elements.Equals(other.elements);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000C293 File Offset: 0x0000A493
		public List<T>.Enumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000C2A0 File Offset: 0x0000A4A0
		IEnumerator<T> IEnumerable<!0>.GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000C2B2 File Offset: 0x0000A4B2
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000C2BF File Offset: 0x0000A4BF
		public void Clear()
		{
			this.elements.Clear();
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000C2CC File Offset: 0x0000A4CC
		public void Set(ICollection<T> other)
		{
			this.Clear();
			foreach (T elem in other)
			{
				this.Add(elem);
			}
		}

		// Token: 0x060001BC RID: 444 RVA: 0x0000C31C File Offset: 0x0000A51C
		public bool Contains(T item)
		{
			return this.list.Contains(item);
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000C32A File Offset: 0x0000A52A
		public bool ContainsGuid(Guid guid)
		{
			return this.elements.ContainsKey(guid);
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000C338 File Offset: 0x0000A538
		public Guid GuidOf(T item)
		{
			for (int i = 0; i < this.list.Count; i++)
			{
				if (this.list[i] == item)
				{
					return this.guids[i];
				}
			}
			return Guid.Empty;
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000C386 File Offset: 0x0000A586
		public int IndexOf(T item)
		{
			return this.list.IndexOf(item);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000C394 File Offset: 0x0000A594
		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000C39C File Offset: 0x0000A59C
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

		// Token: 0x060001C2 RID: 450 RVA: 0x0000C418 File Offset: 0x0000A618
		public bool Remove(T item)
		{
			foreach (Guid key in this.guids)
			{
				if (this.elements[key] == item)
				{
					this.elements.Remove(key);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000C494 File Offset: 0x0000A694
		public void RemoveAt(int index)
		{
			this.elements.Remove(this.guids[index]);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000C4AE File Offset: 0x0000A6AE
		public void Remove(Guid guid)
		{
			this.elements.Remove(guid);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000C4C0 File Offset: 0x0000A6C0
		public int RemoveWhere(Func<T, bool> match)
		{
			int count = 0;
			for (int i = this.list.Count - 1; i >= 0; i--)
			{
				if (match(this.list[i]))
				{
					this.elements.Remove(this.guids[i]);
					count++;
				}
			}
			return count;
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000C518 File Offset: 0x0000A718
		[Obsolete("Use RemoveWhere instead.")]
		public void Filter(Func<T, bool> f)
		{
			this.RemoveWhere((T pair) => !f(pair));
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000C545 File Offset: 0x0000A745
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			childAction(this.elements);
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000C553 File Offset: 0x0000A753
		public override void Read(BinaryReader reader, NetVersion version)
		{
			this.elements.Read(reader, version);
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000C562 File Offset: 0x0000A762
		public override void Write(BinaryWriter writer)
		{
			this.elements.Write(writer);
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000C570 File Offset: 0x0000A770
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.elements.ReadFull(reader, version);
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000C57F File Offset: 0x0000A77F
		public override void WriteFull(BinaryWriter writer)
		{
			this.elements.WriteFull(writer);
		}

		// Token: 0x0400014F RID: 335
		private List<Guid> guids = new List<Guid>();

		// Token: 0x04000150 RID: 336
		private List<T> list = new List<T>();

		// Token: 0x04000151 RID: 337
		private NetGuidDictionary<T, NetRef<T>> elements = new NetGuidDictionary<T, NetRef<T>>();

		// Token: 0x020003C8 RID: 968
		// (Invoke) Token: 0x06003986 RID: 14726
		public delegate void ContentsChangeEvent(T value);
	}
}
