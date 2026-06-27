using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x0200015B RID: 347
	[XmlInclude(typeof(CollectObjective))]
	[XmlInclude(typeof(DeliverObjective))]
	[XmlInclude(typeof(DonateObjective))]
	[XmlInclude(typeof(FishObjective))]
	[XmlInclude(typeof(GiftObjective))]
	[XmlInclude(typeof(JKScoreObjective))]
	[XmlInclude(typeof(ReachMineFloorObjective))]
	[XmlInclude(typeof(ShipObjective))]
	[XmlInclude(typeof(SlayObjective))]
	public class OrderObjective : INetObject<NetFields>
	{
		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06001ADB RID: 6875 RVA: 0x0013B79B File Offset: 0x0013999B
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("OrderObjective");

		// Token: 0x06001ADC RID: 6876 RVA: 0x0013B7A4 File Offset: 0x001399A4
		public OrderObjective()
		{
			this.InitializeNetFields();
		}

		// Token: 0x06001ADD RID: 6877 RVA: 0x0013B7FB File Offset: 0x001399FB
		public virtual void OnFail()
		{
		}

		// Token: 0x06001ADE RID: 6878 RVA: 0x0013B800 File Offset: 0x00139A00
		public virtual void InitializeNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.currentCount, "currentCount").AddField(this.maxCount, "maxCount").AddField(this.failOnCompletion, "failOnCompletion").AddField(this.description, "description");
			this.currentCount.fieldChangeVisibleEvent += this.OnCurrentCountChanged;
		}

		// Token: 0x06001ADF RID: 6879 RVA: 0x0013B871 File Offset: 0x00139A71
		protected void OnCurrentCountChanged(NetIntDelta field, int oldValue, int newValue)
		{
			if (Utility.ShouldIgnoreValueChangeCallback())
			{
				return;
			}
			this.CheckCompletion(true);
		}

		// Token: 0x06001AE0 RID: 6880 RVA: 0x0013B882 File Offset: 0x00139A82
		public void Register(SpecialOrder new_order)
		{
			this._registered = true;
			this._order = new_order;
			this._Register();
			this.CheckCompletion(false);
		}

		// Token: 0x06001AE1 RID: 6881 RVA: 0x0013B89F File Offset: 0x00139A9F
		protected virtual void _Register()
		{
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x0013B8A1 File Offset: 0x00139AA1
		public virtual void Unregister()
		{
			this._registered = false;
			this._Unregister();
			this._order = null;
		}

		// Token: 0x06001AE3 RID: 6883 RVA: 0x0013B8B7 File Offset: 0x00139AB7
		protected virtual void _Unregister()
		{
		}

		// Token: 0x06001AE4 RID: 6884 RVA: 0x0013B8B9 File Offset: 0x00139AB9
		public virtual bool ShouldShowProgress()
		{
			return true;
		}

		// Token: 0x06001AE5 RID: 6885 RVA: 0x0013B8BC File Offset: 0x00139ABC
		public int GetCount()
		{
			return this.currentCount.Value;
		}

		// Token: 0x06001AE6 RID: 6886 RVA: 0x0013B8CC File Offset: 0x00139ACC
		public virtual void IncrementCount(int amount)
		{
			int new_value = this.GetCount() + amount;
			if (new_value < 0)
			{
				new_value = 0;
			}
			if (new_value > this.GetMaxCount())
			{
				new_value = this.GetMaxCount();
			}
			this.SetCount(new_value);
		}

		// Token: 0x06001AE7 RID: 6887 RVA: 0x0013B8FF File Offset: 0x00139AFF
		public virtual void SetCount(int new_count)
		{
			if (new_count > this.GetMaxCount())
			{
				new_count = this.GetMaxCount();
			}
			if (new_count != this.GetCount())
			{
				this.currentCount.Value = new_count;
			}
		}

		// Token: 0x06001AE8 RID: 6888 RVA: 0x0013B927 File Offset: 0x00139B27
		public int GetMaxCount()
		{
			return this.maxCount.Value;
		}

		// Token: 0x06001AE9 RID: 6889 RVA: 0x0013B934 File Offset: 0x00139B34
		public virtual void OnCompletion()
		{
		}

		// Token: 0x06001AEA RID: 6890 RVA: 0x0013B938 File Offset: 0x00139B38
		public virtual void CheckCompletion(bool play_sound = true)
		{
			if (!this._registered)
			{
				return;
			}
			bool was_just_completed = false;
			if (this.GetCount() >= this.GetMaxCount() && this.CanComplete())
			{
				if (!this._complete)
				{
					was_just_completed = true;
					this.OnCompletion();
				}
				this._complete = true;
			}
			else if (this.CanUncomplete() && this._complete)
			{
				this._complete = false;
			}
			if (this._order != null)
			{
				this._order.CheckCompletion();
				if (was_just_completed && this._order.questState.Value != SpecialOrderStatus.Complete && play_sound)
				{
					Game1.playSound("jingle1", null);
				}
			}
		}

		// Token: 0x06001AEB RID: 6891 RVA: 0x0013B9D6 File Offset: 0x00139BD6
		public virtual bool IsComplete()
		{
			return this._complete;
		}

		// Token: 0x06001AEC RID: 6892 RVA: 0x0013B9DE File Offset: 0x00139BDE
		public virtual bool CanUncomplete()
		{
			return false;
		}

		// Token: 0x06001AED RID: 6893 RVA: 0x0013B9E1 File Offset: 0x00139BE1
		public virtual bool CanComplete()
		{
			return true;
		}

		// Token: 0x06001AEE RID: 6894 RVA: 0x0013B9E4 File Offset: 0x00139BE4
		public virtual string GetDescription()
		{
			return this.description.Value;
		}

		// Token: 0x06001AEF RID: 6895 RVA: 0x0013B9F1 File Offset: 0x00139BF1
		public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
		{
		}

		// Token: 0x0400107F RID: 4223
		[XmlIgnore]
		protected SpecialOrder _order;

		// Token: 0x04001080 RID: 4224
		[XmlElement("currentCount")]
		public NetIntDelta currentCount = new NetIntDelta();

		// Token: 0x04001081 RID: 4225
		[XmlElement("maxCount")]
		public NetInt maxCount = new NetInt(0);

		// Token: 0x04001082 RID: 4226
		[XmlElement("description")]
		public NetString description = new NetString();

		// Token: 0x04001083 RID: 4227
		[XmlIgnore]
		protected bool _complete;

		// Token: 0x04001084 RID: 4228
		[XmlIgnore]
		protected bool _registered;

		// Token: 0x04001085 RID: 4229
		[XmlElement("failOnCompletion")]
		public NetBool failOnCompletion = new NetBool(false);
	}
}
