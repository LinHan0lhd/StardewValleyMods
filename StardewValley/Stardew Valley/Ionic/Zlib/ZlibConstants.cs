using System;

namespace Ionic.Zlib
{
	// Token: 0x02000021 RID: 33
	public static class ZlibConstants
	{
		// Token: 0x04000128 RID: 296
		public const int WindowBitsMax = 15;

		// Token: 0x04000129 RID: 297
		public const int WindowBitsDefault = 15;

		// Token: 0x0400012A RID: 298
		public const int Z_OK = 0;

		// Token: 0x0400012B RID: 299
		public const int Z_STREAM_END = 1;

		// Token: 0x0400012C RID: 300
		public const int Z_NEED_DICT = 2;

		// Token: 0x0400012D RID: 301
		public const int Z_STREAM_ERROR = -2;

		// Token: 0x0400012E RID: 302
		public const int Z_DATA_ERROR = -3;

		// Token: 0x0400012F RID: 303
		public const int Z_BUF_ERROR = -5;

		// Token: 0x04000130 RID: 304
		public const int WorkingBufferSizeDefault = 16384;

		// Token: 0x04000131 RID: 305
		public const int WorkingBufferSizeMin = 1024;
	}
}
