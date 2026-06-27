using System;
using System.Collections.Generic;
using Netcode;
using Netcode.Validation;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x02000152 RID: 338
	public class ObjectReward : OrderReward
	{
		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06001AA5 RID: 6821 RVA: 0x0013A990 File Offset: 0x00138B90
		public Object objectInstance
		{
			get
			{
				if (this._objectInstance == null && !string.IsNullOrEmpty(this.itemKey.Value) && this.amount.Value > 0)
				{
					this._objectInstance = new Object(this.itemKey.Value, this.amount.Value, false, -1, 0);
				}
				return this._objectInstance;
			}
		}

		// Token: 0x06001AA6 RID: 6822 RVA: 0x0013A9EF File Offset: 0x00138BEF
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.itemKey, "itemKey").AddField(this.amount, "amount");
		}

		// Token: 0x06001AA7 RID: 6823 RVA: 0x0013AA20 File Offset: 0x00138C20
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			this.itemKey.Value = order.Parse(data["Item"]);
			this.amount.Value = int.Parse(order.Parse(data["Amount"]));
			this._objectInstance = new Object(this.itemKey.Value, this.amount.Value, false, -1, 0);
		}

		// Token: 0x06001AA8 RID: 6824 RVA: 0x0013AA90 File Offset: 0x00138C90
		public override void Grant()
		{
			Object i = new Object(this.itemKey.Value, this.amount.Value, false, -1, 0);
			Game1.player.addItemByMenuIfNecessary(i, null, false);
		}

		// Token: 0x0400106D RID: 4205
		public readonly NetString itemKey = new NetString("");

		// Token: 0x0400106E RID: 4206
		public readonly NetInt amount = new NetInt(0);

		// Token: 0x0400106F RID: 4207
		[NotNetField]
		private Object _objectInstance;
	}
}
