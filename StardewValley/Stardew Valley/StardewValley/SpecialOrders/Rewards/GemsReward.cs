using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x0200014F RID: 335
	public class GemsReward : OrderReward
	{
		// Token: 0x06001A99 RID: 6809 RVA: 0x0013A60B File Offset: 0x0013880B
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.amount, "amount");
		}

		// Token: 0x06001A9A RID: 6810 RVA: 0x0013A62A File Offset: 0x0013882A
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			this.amount.Value = int.Parse(order.Parse(data["Amount"]));
		}

		// Token: 0x06001A9B RID: 6811 RVA: 0x0013A64D File Offset: 0x0013884D
		public override void Grant()
		{
			Game1.player.QiGems += this.amount.Value;
		}

		// Token: 0x04001067 RID: 4199
		public NetInt amount = new NetInt(0);
	}
}
