using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x0200002F RID: 47
	public class NetArray<T, TField> : AbstractNetSerializable, IList<!0>, ICollection<!0>, IEnumerable<!0>, IEnumerable, IEquatable<NetArray<!0, !1>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000181 RID: 385 RVA: 0x0000B945 File Offset: 0x00009B45
		public List<TField> Fields
		{
			get
			{
				return this.elements;
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000182 RID: 386 RVA: 0x0000B950 File Offset: 0x00009B50
		// (remove) Token: 0x06000183 RID: 387 RVA: 0x0000B988 File Offset: 0x00009B88
		public event NetArray<T, TField>.FieldCreateEvent OnFieldCreate;

		// Token: 0x06000184 RID: 388 RVA: 0x0000B9BD File Offset: 0x00009BBD
		public NetArray()
		{
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000B9D0 File Offset: 0x00009BD0
		public NetArray(IEnumerable<T> values) : this()
		{
			int i = 0;
			foreach (T value in values)
			{
				TField field = this.createField(i++);
				field.Set(value);
				this.elements.Add(field);
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000BA40 File Offset: 0x00009C40
		public NetArray(int size) : this()
		{
			for (int i = 0; i < size; i++)
			{
				this.elements.Add(this.createField(i));
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000BA74 File Offset: 0x00009C74
		private TField createField(int index)
		{
			TField field = Activator.CreateInstance<TField>().Interpolated(false, false);
			NetArray<T, TField>.FieldCreateEvent onFieldCreate = this.OnFieldCreate;
			if (onFieldCreate != null)
			{
				onFieldCreate(index, field);
			}
			return field;
		}

		// Token: 0x17000047 RID: 71
		public T this[int index]
		{
			get
			{
				return this.elements[index].Get();
			}
			set
			{
				this.elements[index].Set(value);
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600018A RID: 394 RVA: 0x0000BAD8 File Offset: 0x00009CD8
		public int Count
		{
			get
			{
				return this.elements.Count;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600018B RID: 395 RVA: 0x0000BAE5 File Offset: 0x00009CE5
		public int Length
		{
			get
			{
				return this.elements.Count;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600018C RID: 396 RVA: 0x0000BAF2 File Offset: 0x00009CF2
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600018D RID: 397 RVA: 0x0000BAF5 File Offset: 0x00009CF5
		public bool IsFixedSize
		{
			get
			{
				return base.Parent != null;
			}
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000BB00 File Offset: 0x00009D00
		public void Add(T item)
		{
			if (this.IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			while (this.appendPosition >= this.elements.Count)
			{
				this.elements.Add(this.createField(this.elements.Count));
			}
			this.elements[this.appendPosition].Set(item);
			this.appendPosition++;
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000BB74 File Offset: 0x00009D74
		public void Clear()
		{
			if (this.IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			this.elements.Clear();
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000BB90 File Offset: 0x00009D90
		public bool Contains(T item)
		{
			using (List<TField>.Enumerator enumerator = this.elements.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (object.Equals(enumerator.Current.Get(), item))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000BC00 File Offset: 0x00009E00
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

		// Token: 0x06000192 RID: 402 RVA: 0x0000BC74 File Offset: 0x00009E74
		private void ensureCapacity(int size)
		{
			if (this.IsFixedSize && size != this.Count)
			{
				throw new InvalidOperationException();
			}
			while (this.Count < size)
			{
				this.elements.Add(this.createField(this.Count));
			}
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000BCAD File Offset: 0x00009EAD
		public void SetCount(int size)
		{
			this.ensureCapacity(size);
		}

		// Token: 0x06000194 RID: 404 RVA: 0x0000BCB8 File Offset: 0x00009EB8
		public void Set(IList<T> values)
		{
			this.ensureCapacity(values.Count);
			for (int i = 0; i < this.Count; i++)
			{
				this[i] = values[i];
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000BCF0 File Offset: 0x00009EF0
		public bool Equals(NetArray<T, TField> other)
		{
			return object.Equals(this.elements, other.elements);
		}

		// Token: 0x06000196 RID: 406 RVA: 0x0000BD04 File Offset: 0x00009F04
		public override bool Equals(object obj)
		{
			NetArray<T, TField> otherArray = obj as NetArray<T, TField>;
			return otherArray != null && this.Equals(otherArray);
		}

		// Token: 0x06000197 RID: 407 RVA: 0x0000BD24 File Offset: 0x00009F24
		public override int GetHashCode()
		{
			return this.elements.GetHashCode() ^ 805984909;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000BD37 File Offset: 0x00009F37
		public IEnumerator<T> GetEnumerator()
		{
			foreach (TField elementField in this.elements)
			{
				yield return elementField.Get();
			}
			List<TField>.Enumerator enumerator = default(List<TField>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000BD48 File Offset: 0x00009F48
		public int IndexOf(T item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (object.Equals(this.elements[i].Get(), item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000BD94 File Offset: 0x00009F94
		public void Insert(int index, T item)
		{
			if (this.IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			TField field = this.createField(index);
			field.Set(item);
			this.elements.Insert(index, field);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000BDD0 File Offset: 0x00009FD0
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

		// Token: 0x0600019C RID: 412 RVA: 0x0000BDF3 File Offset: 0x00009FF3
		public void RemoveAt(int index)
		{
			if (this.IsFixedSize)
			{
				throw new InvalidOperationException();
			}
			this.elements.RemoveAt(index);
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0000BE0F File Offset: 0x0000A00F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000BE18 File Offset: 0x0000A018
		public override void Read(BinaryReader reader, NetVersion version)
		{
			BitArray dirtyBits = reader.ReadBitArray();
			for (int i = 0; i < this.elements.Count; i++)
			{
				if (dirtyBits[i])
				{
					this.elements[i].Read(reader, version);
				}
			}
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000BE64 File Offset: 0x0000A064
		public override void Write(BinaryWriter writer)
		{
			BitArray dirtyBits = new BitArray(this.elements.Count);
			for (int i = 0; i < this.elements.Count; i++)
			{
				dirtyBits[i] = this.elements[i].Dirty;
			}
			writer.WriteBitArray(dirtyBits);
			for (int j = 0; j < this.elements.Count; j++)
			{
				if (dirtyBits[j])
				{
					this.elements[j].Write(writer);
				}
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x0000BEF4 File Offset: 0x0000A0F4
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			int size = reader.ReadInt32();
			this.elements.Clear();
			for (int i = 0; i < size; i++)
			{
				TField element = this.createField(this.elements.Count);
				element.ReadFull(reader, version);
				if (base.Parent != null)
				{
					element.Parent = this;
				}
				this.elements.Add(element);
			}
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x0000BF60 File Offset: 0x0000A160
		public override void WriteFull(BinaryWriter writer)
		{
			writer.Write(this.Count);
			foreach (TField tfield in this.elements)
			{
				tfield.WriteFull(writer);
			}
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x0000BFC4 File Offset: 0x0000A1C4
		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (TField elementField in this.elements)
			{
				childAction(elementField);
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000C01C File Offset: 0x0000A21C
		public override string ToString()
		{
			return string.Join<T>(",", this);
		}

		// Token: 0x0400014C RID: 332
		private int appendPosition;

		// Token: 0x0400014D RID: 333
		private readonly List<TField> elements = new List<TField>();

		// Token: 0x020003C6 RID: 966
		// (Invoke) Token: 0x0600397B RID: 14715
		public delegate void FieldCreateEvent(int index, TField field);
	}
}
