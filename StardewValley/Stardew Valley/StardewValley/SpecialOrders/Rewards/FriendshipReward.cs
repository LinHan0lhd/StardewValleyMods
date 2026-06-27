using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x0200014E RID: 334
	public class FriendshipReward : OrderReward
	{
		// Token: 0x06001A95 RID: 6805 RVA: 0x0013A51C File Offset: 0x0013871C
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.targetName, "targetName").AddField(this.amount, "amount");
		}

		// Token: 0x06001A96 RID: 6806 RVA: 0x0013A54C File Offset: 0x0013874C
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string target_name;
			if (!data.TryGetValue("TargetName", out target_name))
			{
				target_name = order.requester.Value;
			}
			target_name = order.Parse(target_name);
			this.targetName.Value = target_name;
			string amountString = data.GetValueOrDefault("Amount", "250");
			amountString = order.Parse(amountString);
			this.amount.Value = int.Parse(amountString);
		}

		// Token: 0x06001A97 RID: 6807 RVA: 0x0013A5B4 File Offset: 0x001387B4
		public override void Grant()
		{
			NPC i = Game1.getCharacterFromName(this.targetName.Value, true, false);
			if (i != null)
			{
				Game1.player.changeFriendship(this.amount.Value, i);
			}
		}

		// Token: 0x04001065 RID: 4197
		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		// Token: 0x04001066 RID: 4198
		[XmlElement("amount")]
		public NetInt amount = new NetInt();
	}
}
