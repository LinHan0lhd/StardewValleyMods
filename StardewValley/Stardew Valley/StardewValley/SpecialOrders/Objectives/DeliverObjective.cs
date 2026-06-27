using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x02000156 RID: 342
	public class DeliverObjective : OrderObjective
	{
		// Token: 0x06001AB9 RID: 6841 RVA: 0x0013AD44 File Offset: 0x00138F44
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue));
			}
			if (data.TryGetValue("TargetName", out rawValue))
			{
				this.targetName.Value = order.Parse(rawValue);
			}
			else
			{
				this.targetName.Value = this._order.requester.Value;
			}
			if (data.TryGetValue("Message", out rawValue))
			{
				this.message.Value = order.Parse(rawValue);
				return;
			}
			this.message.Value = "";
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x0013ADE2 File Offset: 0x00138FE2
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets").AddField(this.targetName, "targetName").AddField(this.message, "message");
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x0013AE21 File Offset: 0x00139021
		public override bool ShouldShowProgress()
		{
			return false;
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x0013AE24 File Offset: 0x00139024
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Combine(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(this.OnItemDelivered));
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x0013AE54 File Offset: 0x00139054
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Remove(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(this.OnItemDelivered));
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x0013AE84 File Offset: 0x00139084
		public virtual int OnItemDelivered(Farmer farmer, NPC npc, Item item, bool probe)
		{
			if (this.IsComplete())
			{
				return 0;
			}
			if (npc.Name != this.targetName.Value)
			{
				return 0;
			}
			bool is_valid_delivery = true;
			foreach (string text in this.acceptableContextTagSets)
			{
				is_valid_delivery = false;
				bool fail = false;
				string[] array = text.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/', StringSplitOptions.None), item.GetContextTags()))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					is_valid_delivery = true;
					break;
				}
			}
			if (!is_valid_delivery)
			{
				return 0;
			}
			int required_amount = base.GetMaxCount() - base.GetCount();
			int donated_amount = Math.Min(item.Stack, required_amount);
			if (donated_amount < required_amount)
			{
				return 0;
			}
			if (!probe)
			{
				Item donated_item = item.getOne();
				donated_item.Stack = donated_amount;
				this._order.donatedItems.Add(donated_item);
				item.Stack -= donated_amount;
				this.IncrementCount(donated_amount);
				if (!string.IsNullOrEmpty(this.message.Value))
				{
					npc.CurrentDialogue.Push(new Dialogue(npc, null, this.message.Value));
					Game1.drawDialogue(npc);
				}
			}
			return donated_amount;
		}

		// Token: 0x04001073 RID: 4211
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		// Token: 0x04001074 RID: 4212
		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		// Token: 0x04001075 RID: 4213
		[XmlElement("message")]
		public NetString message = new NetString();
	}
}
