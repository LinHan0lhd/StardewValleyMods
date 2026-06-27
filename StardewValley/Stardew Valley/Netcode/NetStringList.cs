using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000057 RID: 87
	public sealed class NetStringList : NetList<string, NetString>
	{
		// Token: 0x06000387 RID: 903 RVA: 0x00011534 File Offset: 0x0000F734
		public NetStringList()
		{
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0001153C File Offset: 0x0000F73C
		public NetStringList(IEnumerable<string> values) : base(values)
		{
		}

		// Token: 0x06000389 RID: 905 RVA: 0x00011545 File Offset: 0x0000F745
		public NetStringList(int capacity) : base(capacity)
		{
		}
	}
}
