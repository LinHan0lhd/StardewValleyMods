using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x0200018E RID: 398
	public class HaveBuildingQuest : Quest
	{
		// Token: 0x06001CA5 RID: 7333 RVA: 0x00147E60 File Offset: 0x00146060
		public HaveBuildingQuest()
		{
			this.questType.Value = 8;
		}

		// Token: 0x06001CA6 RID: 7334 RVA: 0x00147E7F File Offset: 0x0014607F
		public HaveBuildingQuest(string buildingType) : this()
		{
			this.buildingType.Value = buildingType;
		}

		// Token: 0x06001CA7 RID: 7335 RVA: 0x00147E93 File Offset: 0x00146093
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.buildingType, "buildingType");
		}

		// Token: 0x06001CA8 RID: 7336 RVA: 0x00147EB4 File Offset: 0x001460B4
		public override bool OnBuildingExists(string buildingType, bool probe = false)
		{
			bool baseChanged = base.OnBuildingExists(buildingType, probe);
			if (buildingType == this.buildingType.Value)
			{
				if (!probe)
				{
					this.questComplete();
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x0400117E RID: 4478
		[XmlElement("buildingType")]
		public readonly NetString buildingType = new NetString();
	}
}
