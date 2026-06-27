using System;
using Netcode;
using StardewValley.SaveSerialization;

namespace StardewValley.Network
{
	// Token: 0x020001E1 RID: 481
	public class NetFarmerRoot : NetRoot<Farmer>
	{
		// Token: 0x0600215C RID: 8540 RVA: 0x00173342 File Offset: 0x00171542
		public NetFarmerRoot()
		{
			this.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
		}

		// Token: 0x0600215D RID: 8541 RVA: 0x0017335F File Offset: 0x0017155F
		public NetFarmerRoot(Farmer value) : base(value)
		{
			this.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x00173380 File Offset: 0x00171580
		public override NetRoot<Farmer> Clone()
		{
			NetRoot<Farmer> result = base.Clone();
			if (Game1.serverHost != null && result.Value != null)
			{
				result.Value.teamRoot = Game1.serverHost.Value.teamRoot;
			}
			return result;
		}
	}
}
