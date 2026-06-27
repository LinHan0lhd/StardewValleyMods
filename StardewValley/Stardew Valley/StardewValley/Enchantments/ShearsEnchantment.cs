using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000352 RID: 850
	public class ShearsEnchantment : BaseEnchantment
	{
		// Token: 0x06003577 RID: 13687 RVA: 0x002A74A3 File Offset: 0x002A56A3
		public override bool CanApplyTo(Item item)
		{
			return item is Shears;
		}
	}
}
