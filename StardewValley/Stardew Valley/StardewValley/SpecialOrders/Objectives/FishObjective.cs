using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x02000158 RID: 344
	public class FishObjective : OrderObjective
	{
		// Token: 0x06001ACB RID: 6859 RVA: 0x0013B39F File Offset: 0x0013959F
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets");
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x0013B3C0 File Offset: 0x001395C0
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue));
			}
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x0013B3EE File Offset: 0x001395EE
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Combine(order.onFishCaught, new Action<Farmer, Item>(this.OnFishCaught));
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x0013B41E File Offset: 0x0013961E
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Remove(order.onFishCaught, new Action<Farmer, Item>(this.OnFishCaught));
		}

		// Token: 0x06001ACF RID: 6863 RVA: 0x0013B450 File Offset: 0x00139650
		public virtual void OnFishCaught(Farmer farmer, Item fish_item)
		{
			foreach (string text in this.acceptableContextTagSets)
			{
				bool fail = false;
				string[] array = text.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/', StringSplitOptions.None), fish_item.GetContextTags()))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					this.IncrementCount(fish_item.Stack);
					break;
				}
			}
		}

		// Token: 0x0400107C RID: 4220
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();
	}
}
