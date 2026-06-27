using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x02000151 RID: 337
	public class MoneyReward : OrderReward
	{
		// Token: 0x06001AA1 RID: 6817 RVA: 0x0013A8CB File Offset: 0x00138ACB
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.amount, "amount").AddField(this.multiplier, "multiplier");
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x0013A8FA File Offset: 0x00138AFA
		public virtual int GetRewardMoneyAmount()
		{
			return (int)((float)this.amount.Value * this.multiplier.Value);
		}

		// Token: 0x06001AA3 RID: 6819 RVA: 0x0013A918 File Offset: 0x00138B18
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			this.amount.Value = int.Parse(order.Parse(data["Amount"]));
			string rawValue;
			if (data.TryGetValue("Multiplier", out rawValue))
			{
				this.multiplier.Value = float.Parse(order.Parse(rawValue));
			}
		}

		// Token: 0x0400106B RID: 4203
		public NetInt amount = new NetInt(0);

		// Token: 0x0400106C RID: 4204
		public NetFloat multiplier = new NetFloat(1f);
	}
}
