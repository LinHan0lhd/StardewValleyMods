using System;
using System.Runtime.InteropServices;

namespace Ionic.Zlib
{
	// Token: 0x02000019 RID: 25
	[Guid("ebc25cf6-9120-4283-b972-0e5520d0000E")]
	public class ZlibException : Exception
	{
		// Token: 0x06000094 RID: 148 RVA: 0x00008F15 File Offset: 0x00007115
		public ZlibException()
		{
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00008F1D File Offset: 0x0000711D
		public ZlibException(string s) : base(s)
		{
		}
	}
}
