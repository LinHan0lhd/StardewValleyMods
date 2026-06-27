using System;

namespace Ionic.Zlib
{
	// Token: 0x0200001D RID: 29
	public sealed class Adler
	{
		// Token: 0x0600009E RID: 158 RVA: 0x000090C4 File Offset: 0x000072C4
		public static uint Adler32(uint adler, byte[] buf, int index, int len)
		{
			if (buf == null)
			{
				return 1U;
			}
			uint s = adler & 65535U;
			uint s2 = adler >> 16 & 65535U;
			while (len > 0)
			{
				int i = (len < Adler.NMAX) ? len : Adler.NMAX;
				len -= i;
				while (i >= 16)
				{
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					s += (uint)buf[index++];
					s2 += s;
					i -= 16;
				}
				if (i != 0)
				{
					do
					{
						s += (uint)buf[index++];
						s2 += s;
					}
					while (--i != 0);
				}
				s %= Adler.BASE;
				s2 %= Adler.BASE;
			}
			return s2 << 16 | s;
		}

		// Token: 0x04000101 RID: 257
		private static readonly uint BASE = 65521U;

		// Token: 0x04000102 RID: 258
		private static readonly int NMAX = 5552;
	}
}
