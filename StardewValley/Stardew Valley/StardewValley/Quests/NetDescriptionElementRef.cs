using System;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x02000189 RID: 393
	public class NetDescriptionElementRef : NetExtendableRef<DescriptionElement, NetDescriptionElementRef>
	{
		// Token: 0x06001C8D RID: 7309 RVA: 0x00146CDB File Offset: 0x00144EDB
		public NetDescriptionElementRef()
		{
			this.Serializer = DescriptionElement.serializer;
		}

		// Token: 0x06001C8E RID: 7310 RVA: 0x00146CEE File Offset: 0x00144EEE
		public NetDescriptionElementRef(DescriptionElement value) : base(value)
		{
			this.Serializer = DescriptionElement.serializer;
		}
	}
}
