using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x0200005D RID: 93
	public class NetRoot<T> : NetRef<T>, INetRoot where T : class, INetObject<INetSerializable>
	{
		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060003D2 RID: 978 RVA: 0x00012451 File Offset: 0x00010651
		public NetClock Clock { get; } = new NetClock();

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x00012459 File Offset: 0x00010659
		public override bool Dirty
		{
			get
			{
				return base.DirtyTick <= this.Clock.GetLocalTick();
			}
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00012471 File Offset: 0x00010671
		public NetRoot()
		{
			base.Root = this;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00012496 File Offset: 0x00010696
		public NetRoot(T value) : this()
		{
			base.cleanSet(value);
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x000124A5 File Offset: 0x000106A5
		public void TickTree()
		{
			this.Clock.Tick();
			base.Tick();
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x000124BC File Offset: 0x000106BC
		public override void Read(BinaryReader reader, NetVersion _)
		{
			NetVersion remoteVersion = default(NetVersion);
			remoteVersion.Read(reader);
			base.Read(reader, remoteVersion);
			this.Clock.netVersion.Merge(remoteVersion);
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x000124F4 File Offset: 0x000106F4
		public void Read(BinaryReader reader)
		{
			NetVersion remoteVersion = default(NetVersion);
			remoteVersion.Read(reader);
			base.Read(reader, remoteVersion);
			this.Clock.netVersion.Merge(remoteVersion);
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0001252A File Offset: 0x0001072A
		public override void Write(BinaryWriter writer)
		{
			this.Clock.netVersion.Write(writer);
			base.Write(writer);
			base.MarkClean();
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0001254A File Offset: 0x0001074A
		public override void ReadFull(BinaryReader reader, NetVersion _)
		{
			base.ReadFull(reader, this.Clock.netVersion);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0001255E File Offset: 0x0001075E
		public static NetRoot<T> Connect(BinaryReader reader)
		{
			NetRoot<T> netRoot = new NetRoot<T>();
			netRoot.ReadConnectionPacket(reader);
			return netRoot;
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0001256C File Offset: 0x0001076C
		public void ReadConnectionPacket(BinaryReader reader)
		{
			this.Clock.LocalId = (int)reader.ReadByte();
			this.Clock.netVersion.Read(reader);
			base.ReadFull(reader, this.Clock.netVersion);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x000125A4 File Offset: 0x000107A4
		public void CreateConnectionPacket(BinaryWriter writer, long? connection)
		{
			int peerId;
			if (connection == null || !this.connections.TryGetValue(connection.Value, out peerId))
			{
				peerId = this.Clock.AddNewPeer();
				if (connection != null)
				{
					this.connections[connection.Value] = peerId;
				}
			}
			writer.Write((byte)peerId);
			this.Clock.netVersion.Write(writer);
			this.WriteFull(writer);
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00012618 File Offset: 0x00010818
		public void Disconnect(long connection)
		{
			int peerId;
			if (this.connections.TryGetValue(connection, out peerId))
			{
				this.Clock.RemovePeer(peerId);
			}
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00012644 File Offset: 0x00010844
		public virtual NetRoot<T> Clone()
		{
			NetRoot<T> result;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						this.WriteFull(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						NetRoot<T> netRoot = new NetRoot<T>();
						netRoot.Serializer = this.Serializer;
						netRoot.ReadFull(reader, this.Clock.netVersion);
						netRoot.reassigned.Set(default(NetVersion));
						netRoot.MarkClean();
						result = netRoot;
					}
				}
			}
			return result;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x00012700 File Offset: 0x00010900
		public void CloneInto(NetRef<T> netref)
		{
			NetRoot<T> netRoot = this.Clone();
			T copy = netRoot.Value;
			netRoot.Value = default(T);
			netref.Value = copy;
		}

		// Token: 0x04000190 RID: 400
		private Dictionary<long, int> connections = new Dictionary<long, int>();
	}
}
