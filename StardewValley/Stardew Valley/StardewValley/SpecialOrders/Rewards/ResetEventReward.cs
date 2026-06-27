using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x02000154 RID: 340
	public class ResetEventReward : OrderReward
	{
		// Token: 0x06001AAF RID: 6831 RVA: 0x0013AB26 File Offset: 0x00138D26
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.resetEvents, "resetEvents");
		}

		// Token: 0x06001AB0 RID: 6832 RVA: 0x0013AB48 File Offset: 0x00138D48
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string raw = order.Parse(data["ResetEvents"]);
			this.resetEvents.AddRange(ArgUtility.SplitBySpace(raw));
		}

		// Token: 0x06001AB1 RID: 6833 RVA: 0x0013AB78 File Offset: 0x00138D78
		public override void Grant()
		{
			foreach (string event_index in this.resetEvents)
			{
				Game1.player.eventsSeen.Remove(event_index);
			}
		}

		// Token: 0x04001071 RID: 4209
		[XmlArrayItem("int")]
		public NetStringList resetEvents = new NetStringList();
	}
}
