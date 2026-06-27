using System;
using System.IO;

namespace StardewValley.Network.Compress
{
	// Token: 0x02000205 RID: 517
	internal class NullNetCompression : INetCompression
	{
		// Token: 0x060022F0 RID: 8944 RVA: 0x0017864E File Offset: 0x0017684E
		public byte[] CompressAbove(byte[] data, int minSizeToCompress = 256)
		{
			return data;
		}

		// Token: 0x060022F1 RID: 8945 RVA: 0x00178651 File Offset: 0x00176851
		public byte[] DecompressBytes(byte[] data)
		{
			return data;
		}

		// Token: 0x060022F2 RID: 8946 RVA: 0x00178654 File Offset: 0x00176854
		public bool TryDecompressStream(Stream dataStream, out byte[] decompressed)
		{
			decompressed = null;
			return false;
		}
	}
}
