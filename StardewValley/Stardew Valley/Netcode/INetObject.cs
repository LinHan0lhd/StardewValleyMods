using System;

namespace Netcode
{
	// Token: 0x0200002C RID: 44
	public interface INetObject<out T> where T : INetSerializable
	{
		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000159 RID: 345
		T NetFields { get; }
	}
}
