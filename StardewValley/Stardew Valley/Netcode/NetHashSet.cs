using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	// Token: 0x0200004E RID: 78
	public abstract class NetHashSet<TValue> : AbstractNetSerializable, IEquatable<NetHashSet<!0>>, ISet<!0>, ICollection<!0>, IEnumerable<!0>, IEnumerable
	{
		// Token: 0x1400000D RID: 13
		// (add) Token: 0x0600031A RID: 794 RVA: 0x0001031C File Offset: 0x0000E51C
		// (remove) Token: 0x0600031B RID: 795 RVA: 0x00010354 File Offset: 0x0000E554
		public event NetHashSet<TValue>.ContentsChangeEvent OnValueAdded;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x0600031C RID: 796 RVA: 0x0001038C File Offset: 0x0000E58C
		// (remove) Token: 0x0600031D RID: 797 RVA: 0x000103C4 File Offset: 0x0000E5C4
		public event NetHashSet<TValue>.ContentsChangeEvent OnValueRemoved;

		// Token: 0x0600031E RID: 798 RVA: 0x000103F9 File Offset: 0x0000E5F9
		public NetHashSet()
		{
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0001042C File Offset: 0x0000E62C
		public NetHashSet(IEnumerable<TValue> values) : this()
		{
			foreach (TValue value in values)
			{
				this.Add(value);
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000320 RID: 800 RVA: 0x0001047C File Offset: 0x0000E67C
		public int Count
		{
			get
			{
				return this.Set.Count;
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000321 RID: 801 RVA: 0x00010489 File Offset: 0x0000E689
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000322 RID: 802 RVA: 0x0001048C File Offset: 0x0000E68C
		public bool Add(TValue item)
		{
			if (!this.Set.Add(item))
			{
				return false;
			}
			this.OutgoingChanges.Add(new NetHashSet<TValue>.OutgoingChange(false, item));
			base.MarkDirty();
			this.addedEvent(item);
			return true;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x000104C0 File Offset: 0x0000E6C0
		public void Clear()
		{
			foreach (TValue entry in this.Set.ToArray<TValue>())
			{
				this.Remove(entry);
			}
			this.OutgoingChanges.RemoveAll((NetHashSet<TValue>.OutgoingChange ch) => !ch.Removal);
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00010522 File Offset: 0x0000E722
		public bool Contains(TValue item)
		{
			return this.Set.Contains(item);
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00010530 File Offset: 0x0000E730
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			this.Set.CopyTo(array, arrayIndex);
		}

		// Token: 0x06000326 RID: 806 RVA: 0x0001053F File Offset: 0x0000E73F
		public bool Equals(NetHashSet<TValue> other)
		{
			return this.Set.Equals((other != null) ? other.Set : null);
		}

		// Token: 0x06000327 RID: 807 RVA: 0x00010558 File Offset: 0x0000E758
		public void ExceptWith(IEnumerable<TValue> other)
		{
			this.Set.ExceptWith(other);
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00010566 File Offset: 0x0000E766
		public IEnumerator<TValue> GetEnumerator()
		{
			return this.Set.GetEnumerator();
		}

		// Token: 0x06000329 RID: 809 RVA: 0x00010578 File Offset: 0x0000E778
		public void IntersectWith(IEnumerable<TValue> other)
		{
			this.Set.IntersectWith(other);
		}

		// Token: 0x0600032A RID: 810 RVA: 0x00010586 File Offset: 0x0000E786
		public bool IsProperSubsetOf(IEnumerable<TValue> other)
		{
			return this.Set.IsProperSubsetOf(other);
		}

		// Token: 0x0600032B RID: 811 RVA: 0x00010594 File Offset: 0x0000E794
		public bool IsProperSupersetOf(IEnumerable<TValue> other)
		{
			return this.Set.IsProperSupersetOf(other);
		}

		// Token: 0x0600032C RID: 812 RVA: 0x000105A2 File Offset: 0x0000E7A2
		public bool IsSubsetOf(IEnumerable<TValue> other)
		{
			return this.Set.IsSubsetOf(other);
		}

		// Token: 0x0600032D RID: 813 RVA: 0x000105B0 File Offset: 0x0000E7B0
		public bool IsSupersetOf(IEnumerable<TValue> other)
		{
			return this.Set.IsSupersetOf(other);
		}

		// Token: 0x0600032E RID: 814 RVA: 0x000105BE File Offset: 0x0000E7BE
		public bool Overlaps(IEnumerable<TValue> other)
		{
			return this.Set.Overlaps(other);
		}

		// Token: 0x0600032F RID: 815 RVA: 0x000105CC File Offset: 0x0000E7CC
		public bool Remove(TValue item)
		{
			if (!this.Set.Remove(item))
			{
				return false;
			}
			this.OutgoingChanges.Add(new NetHashSet<TValue>.OutgoingChange(true, item));
			base.MarkDirty();
			this.removedEvent(item);
			return true;
		}

		// Token: 0x06000330 RID: 816 RVA: 0x00010600 File Offset: 0x0000E800
		public int RemoveWhere(Predicate<TValue> match)
		{
			int num = this.Set.RemoveWhere(delegate(TValue value)
			{
				if (match(value))
				{
					this.OutgoingChanges.Add(new NetHashSet<TValue>.OutgoingChange(true, value));
					this.removedEvent(value);
					return true;
				}
				return false;
			});
			if (num > 0)
			{
				base.MarkDirty();
			}
			return num;
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00010642 File Offset: 0x0000E842
		public bool SetEquals(IEnumerable<TValue> other)
		{
			return this.Set.SetEquals(other);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00010650 File Offset: 0x0000E850
		public void SymmetricExceptWith(IEnumerable<TValue> other)
		{
			this.Set.SymmetricExceptWith(other);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0001065E File Offset: 0x0000E85E
		public void UnionWith(IEnumerable<TValue> other)
		{
			this.Set.UnionWith(other);
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0001066C File Offset: 0x0000E86C
		void ICollection<!0>.Add(TValue item)
		{
			this.Add(item);
		}

		// Token: 0x06000335 RID: 821 RVA: 0x00010676 File Offset: 0x0000E876
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Set.GetEnumerator();
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00010688 File Offset: 0x0000E888
		protected override bool tickImpl()
		{
			List<NetHashSet<TValue>.IncomingChange> triggeredChanges = null;
			foreach (NetHashSet<TValue>.IncomingChange ch in this.IncomingChanges)
			{
				if (base.Root != null && base.GetLocalTick() < ch.Tick)
				{
					break;
				}
				if (triggeredChanges == null)
				{
					triggeredChanges = new List<NetHashSet<TValue>.IncomingChange>();
				}
				triggeredChanges.Add(ch);
			}
			if (triggeredChanges != null)
			{
				foreach (NetHashSet<TValue>.IncomingChange ch2 in triggeredChanges)
				{
					this.IncomingChanges.Remove(ch2);
				}
				foreach (NetHashSet<TValue>.IncomingChange ch3 in triggeredChanges)
				{
					if (ch3.Removal)
					{
						if (this.Set.Remove(ch3.Value))
						{
							this.removedEvent(ch3.Value);
						}
					}
					else if (this.Set.Add(ch3.Value))
					{
						this.addedEvent(ch3.Value);
					}
				}
			}
			return this.IncomingChanges.Count > 0;
		}

		// Token: 0x06000337 RID: 823 RVA: 0x000107D8 File Offset: 0x0000E9D8
		private void removedEvent(TValue value)
		{
			NetHashSet<TValue>.ContentsChangeEvent onValueRemoved = this.OnValueRemoved;
			if (onValueRemoved == null)
			{
				return;
			}
			onValueRemoved(value);
		}

		// Token: 0x06000338 RID: 824 RVA: 0x000107EB File Offset: 0x0000E9EB
		private void addedEvent(TValue value)
		{
			NetHashSet<TValue>.ContentsChangeEvent onValueAdded = this.OnValueAdded;
			if (onValueAdded == null)
			{
				return;
			}
			onValueAdded(value);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x00010800 File Offset: 0x0000EA00
		public override bool Equals(object obj)
		{
			NetHashSet<TValue> other = obj as NetHashSet<TValue>;
			return other != null && this.Equals(other);
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00010820 File Offset: 0x0000EA20
		public override void Read(BinaryReader reader, NetVersion version)
		{
			uint tick = base.GetLocalTick() + (uint)((this.InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0);
			uint count = reader.Read7BitEncoded();
			for (uint i = 0U; i < count; i += 1U)
			{
				bool removal = reader.ReadBoolean();
				TValue value = this.ReadValue(reader);
				this.IncomingChanges.Add(new NetHashSet<TValue>.IncomingChange(tick, removal, value));
				base.NeedsTick = true;
			}
		}

		// Token: 0x0600033B RID: 827 RVA: 0x00010898 File Offset: 0x0000EA98
		public override void Write(BinaryWriter writer)
		{
			writer.Write7BitEncoded((uint)this.OutgoingChanges.Count);
			foreach (NetHashSet<TValue>.OutgoingChange ch in this.OutgoingChanges)
			{
				writer.Write(ch.Removal);
				this.WriteValue(writer, ch.Value);
			}
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00010910 File Offset: 0x0000EB10
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.Set.Clear();
			int count = reader.ReadInt32();
			this.Set.EnsureCapacity(count);
			for (int i = 0; i < count; i++)
			{
				TValue value = this.ReadValue(reader);
				this.Set.Add(value);
				this.addedEvent(value);
			}
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00010964 File Offset: 0x0000EB64
		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(this.Set.Count);
			foreach (TValue value in this.Set)
			{
				this.WriteValue(writer, value);
			}
		}

		// Token: 0x0600033E RID: 830 RVA: 0x000109CC File Offset: 0x0000EBCC
		public override int GetHashCode()
		{
			return this.Set.GetHashCode();
		}

		// Token: 0x0600033F RID: 831
		public abstract TValue ReadValue(BinaryReader reader);

		// Token: 0x06000340 RID: 832
		public abstract void WriteValue(BinaryWriter writer, TValue value);

		// Token: 0x06000341 RID: 833 RVA: 0x000109D9 File Offset: 0x0000EBD9
		protected override void CleanImpl()
		{
			base.CleanImpl();
			this.OutgoingChanges.Clear();
		}

		// Token: 0x0400017E RID: 382
		public bool InterpolationWait = true;

		// Token: 0x0400017F RID: 383
		private readonly HashSet<TValue> Set = new HashSet<TValue>();

		// Token: 0x04000180 RID: 384
		private readonly List<NetHashSet<TValue>.IncomingChange> IncomingChanges = new List<NetHashSet<TValue>.IncomingChange>();

		// Token: 0x04000181 RID: 385
		private readonly List<NetHashSet<TValue>.OutgoingChange> OutgoingChanges = new List<NetHashSet<TValue>.OutgoingChange>();

		// Token: 0x020003E2 RID: 994
		public class IncomingChange
		{
			// Token: 0x060039DE RID: 14814 RVA: 0x002D7C84 File Offset: 0x002D5E84
			public IncomingChange(uint tick, bool removal, TValue value)
			{
				this.Tick = tick;
				this.Removal = removal;
				this.Value = value;
			}

			// Token: 0x040026B4 RID: 9908
			public uint Tick;

			// Token: 0x040026B5 RID: 9909
			public bool Removal;

			// Token: 0x040026B6 RID: 9910
			public TValue Value;
		}

		// Token: 0x020003E3 RID: 995
		public class OutgoingChange
		{
			// Token: 0x060039DF RID: 14815 RVA: 0x002D7CA1 File Offset: 0x002D5EA1
			public OutgoingChange(bool removal, TValue value)
			{
				this.Removal = removal;
				this.Value = value;
			}

			// Token: 0x040026B7 RID: 9911
			public bool Removal;

			// Token: 0x040026B8 RID: 9912
			public TValue Value;
		}

		// Token: 0x020003E4 RID: 996
		// (Invoke) Token: 0x060039E1 RID: 14817
		public delegate void ContentsChangeEvent(TValue value);
	}
}
