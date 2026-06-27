using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x0200015D RID: 349
	public class ShipObjective : OrderObjective
	{
		// Token: 0x06001AF6 RID: 6902 RVA: 0x0013BB18 File Offset: 0x00139D18
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue));
			}
			if (data.TryGetValue("UseShipmentValue", out rawValue) && rawValue.Trim().EqualsIgnoreCase("true"))
			{
				this.useShipmentValue.Value = true;
			}
		}

		// Token: 0x06001AF7 RID: 6903 RVA: 0x0013BB73 File Offset: 0x00139D73
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets").AddField(this.useShipmentValue, "useShipmentValue");
		}

		// Token: 0x06001AF8 RID: 6904 RVA: 0x0013BBA2 File Offset: 0x00139DA2
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Combine(order.onItemShipped, new Action<Farmer, Item, int>(this.OnItemShipped));
		}

		// Token: 0x06001AF9 RID: 6905 RVA: 0x0013BBD2 File Offset: 0x00139DD2
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Remove(order.onItemShipped, new Action<Farmer, Item, int>(this.OnItemShipped));
		}

		// Token: 0x06001AFA RID: 6906 RVA: 0x0013BC04 File Offset: 0x00139E04
		public virtual void OnItemShipped(Farmer farmer, Item item, int shipped_price)
		{
			foreach (string text in this.acceptableContextTagSets)
			{
				bool fail = false;
				string[] array = text.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/', StringSplitOptions.None), item.GetContextTags()))
					{
						fail = true;
					}
				}
				if (!fail)
				{
					if (this.useShipmentValue.Value)
					{
						this.IncrementCount(shipped_price);
						break;
					}
					this.IncrementCount(item.Stack);
					break;
				}
			}
		}

		// Token: 0x04001088 RID: 4232
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		// Token: 0x04001089 RID: 4233
		[XmlElement("useShipmentValue")]
		public NetBool useShipmentValue = new NetBool();
	}
}
