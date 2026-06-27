using System;
using StardewValley.Delegates;

namespace StardewValley.Triggers
{
	// Token: 0x02000124 RID: 292
	public class CachedAction
	{
		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x060017D8 RID: 6104 RVA: 0x00112642 File Offset: 0x00110842
		public string[] Args { get; }

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x060017D9 RID: 6105 RVA: 0x0011264A File Offset: 0x0011084A
		public TriggerActionDelegate Handler { get; }

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x060017DA RID: 6106 RVA: 0x00112652 File Offset: 0x00110852
		public string Error { get; }

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x060017DB RID: 6107 RVA: 0x0011265A File Offset: 0x0011085A
		public bool IsNullHandler { get; }

		// Token: 0x060017DC RID: 6108 RVA: 0x00112662 File Offset: 0x00110862
		public CachedAction(string[] args, TriggerActionDelegate handler, string error, bool isNullHandler)
		{
			this.Args = args;
			this.Handler = handler;
			this.Error = error;
			this.IsNullHandler = isNullHandler;
		}
	}
}
