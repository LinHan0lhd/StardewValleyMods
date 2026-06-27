using System;
using StardewValley.GameData;

namespace StardewValley.Triggers
{
	// Token: 0x02000125 RID: 293
	public class CachedTriggerAction
	{
		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x060017DD RID: 6109 RVA: 0x00112687 File Offset: 0x00110887
		public TriggerActionData Data { get; }

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x060017DE RID: 6110 RVA: 0x0011268F File Offset: 0x0011088F
		public CachedAction[] Actions { get; }

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x060017DF RID: 6111 RVA: 0x00112697 File Offset: 0x00110897
		public string[] ActionStrings { get; }

		// Token: 0x060017E0 RID: 6112 RVA: 0x001126A0 File Offset: 0x001108A0
		public CachedTriggerAction(TriggerActionData data, CachedAction[] actions)
		{
			this.Data = data;
			this.Actions = actions;
			if (actions.Length == 0)
			{
				this.ActionStrings = LegacyShims.EmptyArray<string>();
				return;
			}
			this.ActionStrings = new string[actions.Length];
			for (int i = 0; i < actions.Length; i++)
			{
				this.ActionStrings[i] = string.Join(" ", actions[i].Args);
			}
		}
	}
}
