using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000055 RID: 85
	public sealed class NetLongList : NetList<long, NetLong>
	{
		// Token: 0x0600037D RID: 893 RVA: 0x000113E5 File Offset: 0x0000F5E5
		public NetLongList()
		{
		}

		// Token: 0x0600037E RID: 894 RVA: 0x000113ED File Offset: 0x0000F5ED
		public NetLongList(IEnumerable<long> values) : base(values)
		{
		}

		// Token: 0x0600037F RID: 895 RVA: 0x000113F6 File Offset: 0x0000F5F6
		public NetLongList(int capacity) : base(capacity)
		{
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00011400 File Offset: 0x0000F600
		public override bool Contains(long item)
		{
			using (NetList<long, NetLong>.Enumerator enumerator = base.GetEnumerator())
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

		// Token: 0x06000381 RID: 897 RVA: 0x00011450 File Offset: 0x0000F650
		public override int IndexOf(long item)
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
