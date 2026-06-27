using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000029 RID: 41
	public class NetClock
	{
		// Token: 0x0600013C RID: 316 RVA: 0x0000B385 File Offset: 0x00009585
		public NetClock()
		{
			this.netVersion = default(NetVersion);
			this.LocalId = this.AddNewPeer();
		}

		// Token: 0x0600013D RID: 317 RVA: 0x0000B3B0 File Offset: 0x000095B0
		public int AddNewPeer()
		{
			int id = this.blanks.IndexOf(true);
			if (id != -1)
			{
				this.blanks[id] = false;
			}
			else
			{
				id = this.netVersion.Size();
				while (this.blanks.Count < this.netVersion.Size())
				{
					this.blanks.Add(false);
				}
				this.netVersion[id] = 0U;
			}
			return id;
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000B41D File Offset: 0x0000961D
		public void RemovePeer(int id)
		{
			while (this.blanks.Count <= id)
			{
				this.blanks.Add(false);
			}
			this.blanks[id] = true;
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0000B448 File Offset: 0x00009648
		public uint GetLocalTick()
		{
			return this.netVersion[this.LocalId];
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0000B45C File Offset: 0x0000965C
		public void Tick()
		{
			int localId = this.LocalId;
			uint value = this.netVersion[localId] + 1U;
			this.netVersion[localId] = value;
		}

		// Token: 0x06000141 RID: 321 RVA: 0x0000B487 File Offset: 0x00009687
		public void Clear()
		{
			this.netVersion.Clear();
			this.LocalId = 0;
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000B49B File Offset: 0x0000969B
		public override string ToString()
		{
			return base.ToString() + ";LocalId=" + this.LocalId.ToString();
		}

		// Token: 0x0400013F RID: 319
		public NetVersion netVersion;

		// Token: 0x04000140 RID: 320
		public int LocalId;

		// Token: 0x04000141 RID: 321
		public int InterpolationTicks;

		// Token: 0x04000142 RID: 322
		public List<bool> blanks = new List<bool>();
	}
}
