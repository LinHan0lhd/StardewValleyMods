using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000358 RID: 856
	public class WateringCanEnchantment : BaseEnchantment
	{
		// Token: 0x06003592 RID: 13714 RVA: 0x002A77AA File Offset: 0x002A59AA
		public override bool CanApplyTo(Item item)
		{
			return item is WateringCan;
		}
	}
}
