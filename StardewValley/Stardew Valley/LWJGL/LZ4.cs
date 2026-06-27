using System;
using System.Runtime.InteropServices;

namespace LWJGL
{
	// Token: 0x02000066 RID: 102
	public class LZ4
	{
		// Token: 0x06000420 RID: 1056
		[DllImport("liblwjgl_lz4", CallingConvention = 2, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_LZ4_1compressBound")]
		private static extern int lwjgl_compressBound(IntPtr env, IntPtr clazz, int inputSize);

		// Token: 0x06000421 RID: 1057
		[DllImport("liblwjgl_lz4", CallingConvention = 2, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_nLZ4_1compress_1default")]
		private static extern int lwjgl_compress_default(IntPtr env, IntPtr clazz, byte[] src, IntPtr dest, int srcSize, int dstCapacity);

		// Token: 0x06000422 RID: 1058
		[DllImport("liblwjgl_lz4", CallingConvention = 2, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_nLZ4_1decompress_1safe")]
		private static extern int lwjgl_decompress_safe(IntPtr env, IntPtr clazz, IntPtr src, byte[] dest, int compressedSize, int dstCapacity);

		// Token: 0x06000423 RID: 1059 RVA: 0x000130ED File Offset: 0x000112ED
		public static int CompressBound(int inputSize)
		{
			return LZ4.lwjgl_compressBound(IntPtr.Zero, IntPtr.Zero, inputSize);
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x000130FF File Offset: 0x000112FF
		public static int CompressDefault(byte[] src, IntPtr dest, int srcSize, int dstCapacity)
		{
			return LZ4.lwjgl_compress_default(IntPtr.Zero, IntPtr.Zero, src, dest, srcSize, dstCapacity);
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00013114 File Offset: 0x00011314
		public static int DecompressSafe(IntPtr src, byte[] dest, int compressedSize, int dstCapacity)
		{
			return LZ4.lwjgl_decompress_safe(IntPtr.Zero, IntPtr.Zero, src, dest, compressedSize, dstCapacity);
		}
	}
}
