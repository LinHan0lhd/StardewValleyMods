using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x0200005F RID: 95
	public struct NetVersion : IEquatable<NetVersion>
	{
		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x000129E3 File Offset: 0x00010BE3
		private List<uint> vector
		{
			get
			{
				if (this._vector == null)
				{
					this._vector = new List<uint>();
				}
				return this._vector;
			}
		}

		// Token: 0x17000083 RID: 131
		public uint this[int peerId]
		{
			get
			{
				if (peerId >= this.vector.Count)
				{
					return 0U;
				}
				return this.vector[peerId];
			}
			set
			{
				while (this.vector.Count <= peerId)
				{
					this.vector.Add(0U);
				}
				this.vector[peerId] = value;
			}
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00012A47 File Offset: 0x00010C47
		public NetVersion(NetVersion other)
		{
			this._vector = new List<uint>();
			this.Set(other);
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00012A5B File Offset: 0x00010C5B
		public int Size()
		{
			return this.vector.Count;
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00012A68 File Offset: 0x00010C68
		public void Set(NetVersion other)
		{
			for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
			{
				this[i] = other[i];
			}
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00012AA4 File Offset: 0x00010CA4
		public void Merge(NetVersion other)
		{
			for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
			{
				this[i] = Math.Max(this[i], other[i]);
			}
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00012AEC File Offset: 0x00010CEC
		public bool IsPriorityOver(NetVersion other)
		{
			for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
			{
				if (this[i] > other[i])
				{
					return true;
				}
				if (this[i] < other[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00012B3D File Offset: 0x00010D3D
		public bool IsSimultaneousWith(NetVersion other)
		{
			return this.isOrdered(other, (uint a, uint b) => a == b);
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00012B65 File Offset: 0x00010D65
		public bool IsPrecededBy(NetVersion other)
		{
			return this.isOrdered(other, (uint a, uint b) => a >= b);
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00012B8D File Offset: 0x00010D8D
		public bool IsFollowedBy(NetVersion other)
		{
			return this.isOrdered(other, (uint a, uint b) => a < b);
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00012BB5 File Offset: 0x00010DB5
		public bool IsIndependent(NetVersion other)
		{
			return !this.IsSimultaneousWith(other) && !this.IsPrecededBy(other) && !this.IsFollowedBy(other);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x00012BD8 File Offset: 0x00010DD8
		private bool isOrdered(NetVersion other, Func<uint, uint, bool> comparison)
		{
			for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
			{
				if (!comparison(this[i], other[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00012C1C File Offset: 0x00010E1C
		public override string ToString()
		{
			if (this.Size() == 0)
			{
				return "v0";
			}
			return "v" + string.Join<uint>(",", this.vector);
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00012C48 File Offset: 0x00010E48
		public bool Equals(NetVersion other)
		{
			for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x00012C86 File Offset: 0x00010E86
		public override int GetHashCode()
		{
			return this.vector.GetHashCode() ^ -583558975;
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x00012C9C File Offset: 0x00010E9C
		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)this.Size());
			for (int i = 0; i < this.Size(); i++)
			{
				writer.Write(this[i]);
			}
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x00012CD4 File Offset: 0x00010ED4
		public void Read(BinaryReader reader)
		{
			int size = (int)reader.ReadByte();
			while (this.vector.Count > size)
			{
				this.vector.RemoveAt(size);
			}
			while (this.vector.Count < size)
			{
				this.vector.Add(0U);
			}
			for (int i = 0; i < size; i++)
			{
				this[i] = reader.ReadUInt32();
			}
			for (int j = size; j < this.Size(); j++)
			{
				this[j] = 0U;
			}
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x00012D54 File Offset: 0x00010F54
		public void Clear()
		{
			for (int i = 0; i < this.Size(); i++)
			{
				this[i] = 0U;
			}
		}

		// Token: 0x04000194 RID: 404
		private List<uint> _vector;
	}
}
