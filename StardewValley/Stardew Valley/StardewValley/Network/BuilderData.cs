using System;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001C9 RID: 457
	public class BuilderData : INetObject<NetFields>
	{
		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06002034 RID: 8244 RVA: 0x0016E7FF File Offset: 0x0016C9FF
		public NetFields NetFields { get; } = new NetFields("BuilderData");

		// Token: 0x06002035 RID: 8245 RVA: 0x0016E808 File Offset: 0x0016CA08
		public BuilderData()
		{
			this.NetFields.SetOwner(this).AddField(this.buildingType, "buildingType").AddField(this.daysUntilBuilt, "daysUntilBuilt").AddField(this.buildingLocation, "buildingLocation").AddField(this.buildingTile, "buildingTile").AddField(this.isUpgrade, "isUpgrade");
		}

		// Token: 0x06002036 RID: 8246 RVA: 0x0016E8C0 File Offset: 0x0016CAC0
		public BuilderData(string buildingType, int daysUntilBuilt, string location, Point tile, bool isUpgrade) : this()
		{
			this.buildingType.Value = buildingType;
			this.daysUntilBuilt.Value = daysUntilBuilt;
			this.buildingLocation.Value = location;
			this.buildingTile.Value = tile;
			this.isUpgrade.Value = isUpgrade;
		}

		// Token: 0x040013A9 RID: 5033
		public NetString buildingType = new NetString();

		// Token: 0x040013AA RID: 5034
		public NetInt daysUntilBuilt = new NetInt();

		// Token: 0x040013AB RID: 5035
		public NetString buildingLocation = new NetString();

		// Token: 0x040013AC RID: 5036
		public NetPoint buildingTile = new NetPoint();

		// Token: 0x040013AD RID: 5037
		public NetBool isUpgrade = new NetBool();
	}
}
