using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	// Token: 0x0200010E RID: 270
	public class Warp : INetObject<NetFields>
	{
		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06001741 RID: 5953 RVA: 0x0010F6BA File Offset: 0x0010D8BA
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("Warp");

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06001742 RID: 5954 RVA: 0x0010F6C2 File Offset: 0x0010D8C2
		public int X
		{
			get
			{
				return this.x.Value;
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06001743 RID: 5955 RVA: 0x0010F6CF File Offset: 0x0010D8CF
		public int Y
		{
			get
			{
				return this.y.Value;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06001744 RID: 5956 RVA: 0x0010F6DC File Offset: 0x0010D8DC
		// (set) Token: 0x06001745 RID: 5957 RVA: 0x0010F6E9 File Offset: 0x0010D8E9
		public int TargetX
		{
			get
			{
				return this.targetX.Value;
			}
			set
			{
				this.targetX.Value = value;
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06001746 RID: 5958 RVA: 0x0010F6F7 File Offset: 0x0010D8F7
		// (set) Token: 0x06001747 RID: 5959 RVA: 0x0010F704 File Offset: 0x0010D904
		public int TargetY
		{
			get
			{
				return this.targetY.Value;
			}
			set
			{
				this.targetY.Value = value;
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06001748 RID: 5960 RVA: 0x0010F712 File Offset: 0x0010D912
		// (set) Token: 0x06001749 RID: 5961 RVA: 0x0010F71F File Offset: 0x0010D91F
		public string TargetName
		{
			get
			{
				return this.targetName.Value;
			}
			set
			{
				this.targetName.Value = value;
			}
		}

		// Token: 0x0600174A RID: 5962 RVA: 0x0010F730 File Offset: 0x0010D930
		public Warp()
		{
			this.NetFields.SetOwner(this).AddField(this.x, "this.x").AddField(this.y, "this.y").AddField(this.targetX, "this.targetX").AddField(this.targetY, "this.targetY").AddField(this.targetName, "this.targetName").AddField(this.flipFarmer, "this.flipFarmer").AddField(this.npcOnly, "this.npcOnly");
		}

		// Token: 0x0600174B RID: 5963 RVA: 0x0010F820 File Offset: 0x0010DA20
		public Warp(int x, int y, string targetName, int targetX, int targetY, bool flipFarmer, bool npcOnly = false) : this()
		{
			this.x.Value = x;
			this.y.Value = y;
			this.targetX.Value = targetX;
			this.targetY.Value = targetY;
			this.targetName.Value = targetName;
			this.flipFarmer.Value = flipFarmer;
			this.npcOnly.Value = npcOnly;
		}

		// Token: 0x04000E06 RID: 3590
		[XmlElement("x")]
		private readonly NetInt x = new NetInt();

		// Token: 0x04000E07 RID: 3591
		[XmlElement("y")]
		private readonly NetInt y = new NetInt();

		// Token: 0x04000E08 RID: 3592
		[XmlElement("targetX")]
		private readonly NetInt targetX = new NetInt();

		// Token: 0x04000E09 RID: 3593
		[XmlElement("targetY")]
		private readonly NetInt targetY = new NetInt();

		// Token: 0x04000E0A RID: 3594
		[XmlElement("flipFarmer")]
		public readonly NetBool flipFarmer = new NetBool();

		// Token: 0x04000E0B RID: 3595
		[XmlElement("targetName")]
		private readonly NetString targetName = new NetString();

		// Token: 0x04000E0C RID: 3596
		[XmlElement("npcOnly")]
		public readonly NetBool npcOnly = new NetBool();
	}
}
