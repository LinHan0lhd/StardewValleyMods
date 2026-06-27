using System;
using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E0 RID: 480
	public class NetFarmerRef : INetObject<NetFields>, IEnumerable<long?>, IEnumerable
	{
		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06002150 RID: 8528 RVA: 0x00173146 File Offset: 0x00171346
		public NetFields NetFields { get; } = new NetFields("NetFarmerRef");

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06002151 RID: 8529 RVA: 0x0017314E File Offset: 0x0017134E
		// (set) Token: 0x06002152 RID: 8530 RVA: 0x0017316B File Offset: 0x0017136B
		public long UID
		{
			get
			{
				if (!this.defined.Value)
				{
					return 0L;
				}
				return this.uid.Value;
			}
			set
			{
				this.uid.Value = value;
				this.defined.Value = true;
			}
		}

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06002153 RID: 8531 RVA: 0x00173185 File Offset: 0x00171385
		// (set) Token: 0x06002154 RID: 8532 RVA: 0x001731A7 File Offset: 0x001713A7
		public Farmer Value
		{
			get
			{
				if (!this.defined.Value)
				{
					return null;
				}
				return this.getFarmer(this.uid.Value);
			}
			set
			{
				this.defined.Value = (value != null);
				this.uid.Value = ((value != null) ? value.UniqueMultiplayerID : 0L);
			}
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x001731D0 File Offset: 0x001713D0
		public NetFarmerRef()
		{
			this.NetFields.SetOwner(this).AddField(this.defined, "defined").AddField(this.uid, "uid");
		}

		// Token: 0x06002156 RID: 8534 RVA: 0x00173238 File Offset: 0x00171438
		private Farmer getFarmer(long uid)
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.UniqueMultiplayerID == uid)
				{
					return farmer;
				}
			}
			return null;
		}

		// Token: 0x06002157 RID: 8535 RVA: 0x00173290 File Offset: 0x00171490
		public NetFarmerRef Delayed(bool interpolationWait)
		{
			this.defined.Interpolated(false, interpolationWait);
			this.uid.Interpolated(false, interpolationWait);
			return this;
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x001732AF File Offset: 0x001714AF
		public void Set(NetFarmerRef other)
		{
			this.uid.Value = other.uid.Value;
			this.defined.Value = other.defined.Value;
		}

		// Token: 0x06002159 RID: 8537 RVA: 0x001732DD File Offset: 0x001714DD
		public IEnumerator<long?> GetEnumerator()
		{
			yield return this.defined.Value ? new long?(this.uid.Value) : null;
			yield break;
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x001732EC File Offset: 0x001714EC
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x001732F4 File Offset: 0x001714F4
		public void Add(long? value)
		{
			if (value == null)
			{
				this.defined.Value = false;
				this.uid.Value = 0L;
				return;
			}
			this.defined.Value = true;
			this.uid.Value = value.Value;
		}

		// Token: 0x040013F4 RID: 5108
		public readonly NetBool defined = new NetBool();

		// Token: 0x040013F5 RID: 5109
		public readonly NetLong uid = new NetLong();
	}
}
