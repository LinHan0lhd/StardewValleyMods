using System;

namespace Netcode
{
	// Token: 0x0200002A RID: 42
	public interface INetRoot
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000143 RID: 323
		NetClock Clock { get; }

		// Token: 0x06000144 RID: 324
		void TickTree();

		// Token: 0x06000145 RID: 325
		void Disconnect(long connection);
	}
}
