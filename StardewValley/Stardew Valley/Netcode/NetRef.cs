using System;

namespace Netcode
{
	// Token: 0x0200005B RID: 91
	public class NetRef<T> : NetExtendableRef<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		// Token: 0x060003C9 RID: 969 RVA: 0x000122AA File Offset: 0x000104AA
		public NetRef()
		{
		}

		// Token: 0x060003CA RID: 970 RVA: 0x000122B2 File Offset: 0x000104B2
		public NetRef(T value) : base(value)
		{
		}
	}
}
