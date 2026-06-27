using System;
using System.Collections.Generic;
using StardewValley.GameData;

namespace StardewValley.Delegates
{
	// Token: 0x02000367 RID: 871
	public readonly struct TriggerActionContext
	{
		// Token: 0x060035CB RID: 13771 RVA: 0x002A7890 File Offset: 0x002A5A90
		public TriggerActionContext(string trigger, object[] triggerArgs, TriggerActionData data, Dictionary<string, object> customFields = null)
		{
			this.Trigger = trigger;
			this.TriggerArgs = triggerArgs;
			this.Data = data;
			this.CustomFields = customFields;
		}

		// Token: 0x040022B7 RID: 8887
		public readonly string Trigger;

		// Token: 0x040022B8 RID: 8888
		public readonly object[] TriggerArgs;

		// Token: 0x040022B9 RID: 8889
		public readonly TriggerActionData Data;

		// Token: 0x040022BA RID: 8890
		public readonly Dictionary<string, object> CustomFields;
	}
}
