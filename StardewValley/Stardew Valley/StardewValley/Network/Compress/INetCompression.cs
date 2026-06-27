using System;
using System.IO;

namespace StardewValley.Network.Compress
{
	// Token: 0x02000203 RID: 515
	public interface INetCompression
	{
		// Token: 0x060022E8 RID: 8936
		byte[] CompressAbove(byte[] data, int minSizeToCompress = 256);

		// Token: 0x060022E9 RID: 8937
		byte[] DecompressBytes(byte[] data);

		// Token: 0x060022EA RID: 8938
		bool TryDecompressStream(Stream dataStream, out byte[] decompressed);
	}
}
