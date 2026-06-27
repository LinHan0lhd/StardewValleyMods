using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000334 RID: 820
	public class AxeEnchantment : BaseEnchantment
	{
		// Token: 0x060034EC RID: 13548 RVA: 0x002A637F File Offset: 0x002A457F
		public override bool CanApplyTo(Item item)
		{
			return item is Axe;
		}
	}
}
