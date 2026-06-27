using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000056 RID: 86
	public sealed class NetIntList : NetList<int, NetInt>
	{
		// Token: 0x06000382 RID: 898 RVA: 0x0001148C File Offset: 0x0000F68C
		public NetIntList()
		{
		}

		// Token: 0x06000383 RID: 899 RVA: 0x00011494 File Offset: 0x0000F694
		public NetIntList(IEnumerable<int> values) : base(values)
		{
		}

		// Token: 0x06000384 RID: 900 RVA: 0x0001149D File Offset: 0x0000F69D
		public NetIntList(int capacity) : base(capacity)
		{
		}

		// Token: 0x06000385 RID: 901 RVA: 0x000114A8 File Offset: 0x0000F6A8
		public override bool Contains(int item)
		{
			using (NetList<int, NetInt>.Enumerator enumerator = base.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000386 RID: 902 RVA: 0x000114F8 File Offset: 0x0000F6F8
		public override int IndexOf(int item)
		{
			NetInt count = this.count;
			for (int i = 0; i < count.Value; i++)
			{
				if (this.array.Value[i] == item)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
