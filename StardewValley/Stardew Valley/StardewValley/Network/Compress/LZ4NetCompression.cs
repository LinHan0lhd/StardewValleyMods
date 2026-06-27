using System;
using System.IO;
using System.Runtime.InteropServices;
using LWJGL;

namespace StardewValley.Network.Compress
{
	// Token: 0x02000204 RID: 516
	internal class LZ4NetCompression : INetCompression
	{
		// Token: 0x060022EB RID: 8939 RVA: 0x001784EC File Offset: 0x001766EC
		public byte[] CompressAbove(byte[] data, int minSizeToCompress = 256)
		{
			if (data.Length < minSizeToCompress)
			{
				return data;
			}
			int destSize = LZ4.CompressBound(data.Length);
			IntPtr intPtr = Marshal.AllocHGlobal(destSize + 9);
			IntPtr dest = IntPtr.Add(intPtr, 9);
			int compressedSize = LZ4.CompressDefault(data, dest, data.Length, destSize);
			Marshal.WriteByte(intPtr, 0, 127);
			Marshal.WriteInt32(intPtr, 1, compressedSize);
			Marshal.WriteInt32(intPtr, 5, data.Length);
			byte[] compressed = new byte[compressedSize + 9];
			Marshal.Copy(intPtr, compressed, 0, compressed.Length);
			Marshal.FreeHGlobal(intPtr);
			return compressed;
		}

		// Token: 0x060022EC RID: 8940 RVA: 0x0017855E File Offset: 0x0017675E
		public byte[] DecompressBytes(byte[] data)
		{
			if (data[0] != 127)
			{
				return data;
			}
			return this.DecompressImpl(data);
		}

		// Token: 0x060022ED RID: 8941 RVA: 0x00178570 File Offset: 0x00176770
		public bool TryDecompressStream(Stream dataStream, out byte[] decompressed)
		{
			decompressed = null;
			if (!dataStream.CanSeek || !dataStream.CanRead)
			{
				throw new ArgumentException("dataStream must support both reading and seeking");
			}
			long startPosition = dataStream.Position;
			if ((byte)dataStream.ReadByte() != 127)
			{
				dataStream.Seek(startPosition, SeekOrigin.Begin);
				return false;
			}
			byte[] compressedSizeHeader = new byte[4];
			dataStream.Read(compressedSizeHeader, 0, 4);
			int compressedSize = BitConverter.ToInt32(compressedSizeHeader, 0);
			byte[] data = new byte[compressedSize + 9];
			dataStream.Read(data, 5, 4 + compressedSize);
			decompressed = this.DecompressImpl(data);
			return true;
		}

		// Token: 0x060022EE RID: 8942 RVA: 0x001785F4 File Offset: 0x001767F4
		private unsafe byte[] DecompressImpl(byte[] data)
		{
			int decompressedSize = BitConverter.ToInt32(data, 5);
			byte[] decompressed = new byte[decompressedSize];
			fixed (byte[] array = data)
			{
				byte* ptr;
				if (data == null || array.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &array[0];
				}
				LZ4.DecompressSafe(IntPtr.Add((IntPtr)((void*)ptr), 9), decompressed, data.Length - 9, decompressedSize);
			}
			return decompressed;
		}

		// Token: 0x040014AB RID: 5291
		private const int HeaderSize = 9;
	}
}
