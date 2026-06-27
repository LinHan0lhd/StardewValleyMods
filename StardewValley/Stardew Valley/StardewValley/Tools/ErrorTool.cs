using System;

namespace StardewValley.Tools
{
	// Token: 0x02000128 RID: 296
	public class ErrorTool : Tool
	{
		// Token: 0x060017F7 RID: 6135 RVA: 0x001134E9 File Offset: 0x001116E9
		public ErrorTool() : base("Error Item", 0, 0, 0, false, 0)
		{
		}

		// Token: 0x060017F8 RID: 6136 RVA: 0x001134FB File Offset: 0x001116FB
		public ErrorTool(string itemId, int upgradeLevel = 0, int numAttachmentSlots = 0) : base("Error Item", upgradeLevel, 0, 0, false, numAttachmentSlots)
		{
			base.ItemId = itemId;
			this.Name = "Error Item";
		}

		// Token: 0x060017F9 RID: 6137 RVA: 0x0011351F File Offset: 0x0011171F
		protected override Item GetOneNew()
		{
			return new ErrorTool(base.ItemId, base.UpgradeLevel, this.numAttachmentSlots.Value);
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x0011353D File Offset: 0x0011173D
		protected override string loadDescription()
		{
			return ItemRegistry.RequireTypeDefinition("(T)").GetErrorData(base.ItemId).Description;
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x00113559 File Offset: 0x00111759
		protected override string loadDisplayName()
		{
			return ItemRegistry.RequireTypeDefinition("(T)").GetErrorData(base.ItemId).DisplayName;
		}
	}
}
