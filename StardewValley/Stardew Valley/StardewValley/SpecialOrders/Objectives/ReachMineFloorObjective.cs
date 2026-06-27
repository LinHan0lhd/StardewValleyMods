using System;
using System.Collections.Generic;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x0200015C RID: 348
	public class ReachMineFloorObjective : OrderObjective
	{
		// Token: 0x06001AF0 RID: 6896 RVA: 0x0013B9F3 File Offset: 0x00139BF3
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.skullCave, "skullCave");
		}

		// Token: 0x06001AF1 RID: 6897 RVA: 0x0013BA14 File Offset: 0x00139C14
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			base.Load(order, data);
			string rawValue;
			if (data.TryGetValue("SkullCave", out rawValue) && rawValue.EqualsIgnoreCase("true"))
			{
				this.skullCave.Value = true;
			}
		}

		// Token: 0x06001AF2 RID: 6898 RVA: 0x0013BA51 File Offset: 0x00139C51
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onMineFloorReached = (Action<Farmer, int>)Delegate.Combine(order.onMineFloorReached, new Action<Farmer, int>(this.OnNewValue));
		}

		// Token: 0x06001AF3 RID: 6899 RVA: 0x0013BA81 File Offset: 0x00139C81
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onMineFloorReached = (Action<Farmer, int>)Delegate.Remove(order.onMineFloorReached, new Action<Farmer, int>(this.OnNewValue));
		}

		// Token: 0x06001AF4 RID: 6900 RVA: 0x0013BAB4 File Offset: 0x00139CB4
		public virtual void OnNewValue(Farmer who, int new_value)
		{
			if (this.skullCave.Value)
			{
				new_value -= 120;
			}
			else if (new_value > 120)
			{
				return;
			}
			if (new_value <= 0)
			{
				return;
			}
			this.SetCount(Math.Min(Math.Max(new_value, this.currentCount.Value), base.GetMaxCount()));
		}

		// Token: 0x04001087 RID: 4231
		public NetBool skullCave = new NetBool(false);
	}
}
