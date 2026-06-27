using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000050 RID: 80
	public class NetIntHashSet : NetHashSet<int>
	{
		// Token: 0x06000345 RID: 837 RVA: 0x00010A1C File Offset: 0x0000EC1C
		public override int ReadValue(BinaryReader reader)
		{
			return reader.ReadInt32();
		}

		// Token: 0x06000346 RID: 838 RVA: 0x00010A24 File Offset: 0x0000EC24
		public override void WriteValue(BinaryWriter writer, int value)
		{
			writer.Write(value);
		}
	}
}
