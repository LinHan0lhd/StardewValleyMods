using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x02000155 RID: 341
	public class CollectObjective : OrderObjective
	{
		// Token: 0x06001AB3 RID: 6835 RVA: 0x0013ABEC File Offset: 0x00138DEC
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue));
			}
		}

		// Token: 0x06001AB4 RID: 6836 RVA: 0x0013AC1A File Offset: 0x00138E1A
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets");
		}

		// Token: 0x06001AB5 RID: 6837 RVA: 0x0013AC39 File Offset: 0x00138E39
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onItemCollected = (Action<Farmer, Item>)Delegate.Combine(order.onItemCollected, new Action<Farmer, Item>(this.OnItemShipped));
		}

		// Token: 0x06001AB6 RID: 6838 RVA: 0x0013AC69 File Offset: 0x00138E69
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onItemCollected = (Action<Farmer, Item>)Delegate.Remove(order.onItemCollected, new Action<Farmer, Item>(this.OnItemShipped));
		}

		// Token: 0x06001AB7 RID: 6839 RVA: 0x0013AC9C File Offset: 0x00138E9C
		public virtual void OnItemShipped(Farmer farmer, Item item)
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
						break;
					}
				}
				if (!fail)
				{
					this.IncrementCount(item.Stack);
					break;
				}
			}
		}

		// Token: 0x04001072 RID: 4210
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();
	}
}
