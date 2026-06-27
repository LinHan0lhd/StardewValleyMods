using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x02000153 RID: 339
	[XmlInclude(typeof(FriendshipReward))]
	[XmlInclude(typeof(GemsReward))]
	[XmlInclude(typeof(MailReward))]
	[XmlInclude(typeof(MoneyReward))]
	[XmlInclude(typeof(ObjectReward))]
	[XmlInclude(typeof(ResetEventReward))]
	public class OrderReward : INetObject<NetFields>
	{
		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06001AAA RID: 6826 RVA: 0x0013AAED File Offset: 0x00138CED
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("OrderReward");

		// Token: 0x06001AAB RID: 6827 RVA: 0x0013AAF5 File Offset: 0x00138CF5
		public OrderReward()
		{
			this.InitializeNetFields();
		}

		// Token: 0x06001AAC RID: 6828 RVA: 0x0013AB13 File Offset: 0x00138D13
		public virtual void InitializeNetFields()
		{
			this.NetFields.SetOwner(this);
		}

		// Token: 0x06001AAD RID: 6829 RVA: 0x0013AB22 File Offset: 0x00138D22
		public virtual void Grant()
		{
		}

		// Token: 0x06001AAE RID: 6830 RVA: 0x0013AB24 File Offset: 0x00138D24
		public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
		{
		}
	}
}
