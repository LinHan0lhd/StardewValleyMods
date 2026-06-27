using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001ED RID: 493
	public class NetWitnessedLock : INetObject<NetFields>
	{
		// Token: 0x17000389 RID: 905
		// (get) Token: 0x060021B8 RID: 8632 RVA: 0x001741E6 File Offset: 0x001723E6
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("NetWitnessedLock");

		// Token: 0x060021B9 RID: 8633 RVA: 0x001741F0 File Offset: 0x001723F0
		public NetWitnessedLock()
		{
			this.NetFields.SetOwner(this).AddField(this.requested, "requested").AddField(this.witnesses.NetFields, "witnesses.NetFields");
		}

		// Token: 0x060021BA RID: 8634 RVA: 0x00174262 File Offset: 0x00172462
		public void RequestLock(Action acquired, Action failed)
		{
			if (!Game1.IsMasterGame)
			{
				throw new InvalidOperationException();
			}
			if (acquired == null)
			{
				throw new ArgumentException();
			}
			if (this.requested.Value)
			{
				failed();
				return;
			}
			this.requested.Value = true;
			this.acquired = acquired;
		}

		// Token: 0x060021BB RID: 8635 RVA: 0x001742A1 File Offset: 0x001724A1
		public bool IsLocked()
		{
			return this.requested.Value;
		}

		// Token: 0x060021BC RID: 8636 RVA: 0x001742B0 File Offset: 0x001724B0
		public void Update()
		{
			this.witnesses.RetainOnlinePlayers();
			if (this.requested.Value)
			{
				if (!this.witnesses.Contains(Game1.player))
				{
					this.witnesses.Add(Game1.player);
				}
				if (Game1.IsMasterGame)
				{
					foreach (Farmer f in Game1.otherFarmers.Values)
					{
						if (!this.witnesses.Contains(f))
						{
							return;
						}
					}
					this.acquired();
					this.acquired = null;
					this.requested.Value = false;
					this.witnesses.Clear();
				}
			}
		}

		// Token: 0x04001416 RID: 5142
		private readonly NetBool requested = new NetBool().Interpolated(false, false);

		// Token: 0x04001417 RID: 5143
		private readonly NetFarmerCollection witnesses = new NetFarmerCollection();

		// Token: 0x04001418 RID: 5144
		private Action acquired;
	}
}
