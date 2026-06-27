using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200034B RID: 843
	public class PanEnchantment : BaseEnchantment
	{
		// Token: 0x06003562 RID: 13666 RVA: 0x002A720B File Offset: 0x002A540B
		public override bool CanApplyTo(Item item)
		{
			return item is Pan;
		}
	}
}
