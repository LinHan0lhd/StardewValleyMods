using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	// Token: 0x0200018D RID: 397
	public class GoSomewhereQuest : Quest
	{
		// Token: 0x06001CA1 RID: 7329 RVA: 0x00147DCD File Offset: 0x00145FCD
		public GoSomewhereQuest()
		{
		}

		// Token: 0x06001CA2 RID: 7330 RVA: 0x00147DE0 File Offset: 0x00145FE0
		public GoSomewhereQuest(string where)
		{
			this.whereToGo.Value = where;
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x00147DFF File Offset: 0x00145FFF
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.whereToGo, "whereToGo");
		}

		// Token: 0x06001CA4 RID: 7332 RVA: 0x00147E20 File Offset: 0x00146020
		public override bool OnWarped(GameLocation location, bool probe = false)
		{
			bool baseChanged = base.OnWarped(location, probe);
			if (((location != null) ? location.NameOrUniqueName : null) == this.whereToGo.Value)
			{
				if (!probe)
				{
					this.questComplete();
				}
				return true;
			}
			return baseChanged;
		}

		// Token: 0x0400117D RID: 4477
		[XmlElement("whereToGo")]
		public readonly NetString whereToGo = new NetString();
	}
}
