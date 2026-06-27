using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000340 RID: 832
	public class FisherEnchantment : BaseEnchantment
	{
		// Token: 0x0600353E RID: 13630 RVA: 0x002A6D6E File Offset: 0x002A4F6E
		public override string GetName()
		{
			return "Fisher";
		}

		// Token: 0x0600353F RID: 13631 RVA: 0x002A6D75 File Offset: 0x002A4F75
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && item is Pan;
		}
	}
}
