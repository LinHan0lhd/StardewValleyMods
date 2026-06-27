using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000054 RID: 84
	public class NetObjectList<T> : NetList<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		// Token: 0x0600037A RID: 890 RVA: 0x000113CB File Offset: 0x0000F5CB
		public NetObjectList()
		{
		}

		// Token: 0x0600037B RID: 891 RVA: 0x000113D3 File Offset: 0x0000F5D3
		public NetObjectList(IEnumerable<T> values) : base(values)
		{
		}

		// Token: 0x0600037C RID: 892 RVA: 0x000113DC File Offset: 0x0000F5DC
		public NetObjectList(int capacity) : base(capacity)
		{
		}
	}
}
