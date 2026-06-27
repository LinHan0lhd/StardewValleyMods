using System;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x0200015A RID: 346
	public class JKScoreObjective : OrderObjective
	{
		// Token: 0x06001AD7 RID: 6871 RVA: 0x0013B70F File Offset: 0x0013990F
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onJKScoreAchieved = (Action<Farmer, int>)Delegate.Combine(order.onJKScoreAchieved, new Action<Farmer, int>(this.OnNewValue));
		}

		// Token: 0x06001AD8 RID: 6872 RVA: 0x0013B73F File Offset: 0x0013993F
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onJKScoreAchieved = (Action<Farmer, int>)Delegate.Remove(order.onJKScoreAchieved, new Action<Farmer, int>(this.OnNewValue));
		}

		// Token: 0x06001AD9 RID: 6873 RVA: 0x0013B76F File Offset: 0x0013996F
		public virtual void OnNewValue(Farmer who, int new_value)
		{
			this.SetCount(Math.Min(Math.Max(new_value, this.currentCount.Value), base.GetMaxCount()));
		}
	}
}
