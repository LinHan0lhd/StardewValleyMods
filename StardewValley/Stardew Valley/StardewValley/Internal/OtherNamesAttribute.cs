using System;

namespace StardewValley.Internal
{
	// Token: 0x02000315 RID: 789
	[AttributeUsage(AttributeTargets.Method)]
	public class OtherNamesAttribute : Attribute
	{
		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06003436 RID: 13366 RVA: 0x0029B9BD File Offset: 0x00299BBD
		public string[] Aliases { get; }

		// Token: 0x06003437 RID: 13367 RVA: 0x0029B9C5 File Offset: 0x00299BC5
		public OtherNamesAttribute(params string[] aliases)
		{
			this.Aliases = aliases;
		}
	}
}
