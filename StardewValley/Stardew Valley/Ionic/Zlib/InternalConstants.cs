using System;

namespace Ionic.Zlib
{
	// Token: 0x0200001B RID: 27
	internal static class InternalConstants
	{
		// Token: 0x040000ED RID: 237
		internal static readonly int MAX_BITS = 15;

		// Token: 0x040000EE RID: 238
		internal static readonly int BL_CODES = 19;

		// Token: 0x040000EF RID: 239
		internal static readonly int D_CODES = 30;

		// Token: 0x040000F0 RID: 240
		internal static readonly int LITERALS = 256;

		// Token: 0x040000F1 RID: 241
		internal static readonly int LENGTH_CODES = 29;

		// Token: 0x040000F2 RID: 242
		internal static readonly int L_CODES = InternalConstants.LITERALS + 1 + InternalConstants.LENGTH_CODES;

		// Token: 0x040000F3 RID: 243
		internal static readonly int MAX_BL_BITS = 7;

		// Token: 0x040000F4 RID: 244
		internal static readonly int REP_3_6 = 16;

		// Token: 0x040000F5 RID: 245
		internal static readonly int REPZ_3_10 = 17;

		// Token: 0x040000F6 RID: 246
		internal static readonly int REPZ_11_138 = 18;
	}
}
