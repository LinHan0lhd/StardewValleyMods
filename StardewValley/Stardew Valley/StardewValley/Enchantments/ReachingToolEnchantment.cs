using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200034F RID: 847
	public class ReachingToolEnchantment : BaseEnchantment
	{
		// Token: 0x0600356D RID: 13677 RVA: 0x002A732B File Offset: 0x002A552B
		public override string GetName()
		{
			return "Expansive";
		}

		// Token: 0x0600356E RID: 13678 RVA: 0x002A7334 File Offset: 0x002A5534
		public override bool CanApplyTo(Item item)
		{
			Tool tool = item as Tool;
			return tool != null && (tool is WateringCan || tool is Hoe || tool is Pan) && tool.UpgradeLevel == 4;
		}
	}
}
