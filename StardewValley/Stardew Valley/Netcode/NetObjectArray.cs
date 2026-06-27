using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000030 RID: 48
	public class NetObjectArray<T> : NetArray<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		// Token: 0x060001A4 RID: 420 RVA: 0x0000C029 File Offset: 0x0000A229
		public NetObjectArray()
		{
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x0000C031 File Offset: 0x0000A231
		public NetObjectArray(IEnumerable<T> values) : base(values)
		{
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000C03A File Offset: 0x0000A23A
		public NetObjectArray(int size) : base(size)
		{
		}
	}
}
