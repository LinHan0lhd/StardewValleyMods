using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000039 RID: 57
	public class NetEventBinary : AbstractNetEvent1<byte[]>
	{
		// Token: 0x0600024E RID: 590 RVA: 0x0000DF80 File Offset: 0x0000C180
		public void Fire(NetEventBinary.ArgWriter argWriter)
		{
			byte[] bytes;
			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(ms))
				{
					argWriter(writer);
					ms.Position = 0L;
					bytes = new byte[ms.Length];
					ms.Read(bytes, 0, (int)ms.Length);
				}
			}
			base.Fire(bytes);
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0000E000 File Offset: 0x0000C200
		public void AddReaderHandler(Action<BinaryReader> handler)
		{
			base.onEvent += delegate(byte[] bytes)
			{
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					using (BinaryReader reader = new BinaryReader(ms))
					{
						handler(reader);
					}
				}
			};
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000E02C File Offset: 0x0000C22C
		protected override byte[] readEventArg(BinaryReader reader, NetVersion version)
		{
			int count = reader.ReadInt32();
			return reader.ReadBytes(count);
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000E047 File Offset: 0x0000C247
		protected override void writeEventArg(BinaryWriter writer, byte[] arg)
		{
			writer.Write(arg.Length);
			writer.Write(arg);
		}

		// Token: 0x020003DE RID: 990
		// (Invoke) Token: 0x060039D5 RID: 14805
		public delegate void ArgWriter(BinaryWriter writer);
	}
}
