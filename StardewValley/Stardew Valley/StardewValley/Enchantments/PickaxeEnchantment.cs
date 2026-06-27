using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200034C RID: 844
	public class PickaxeEnchantment : BaseEnchantment
	{
		// Token: 0x06003564 RID: 13668 RVA: 0x002A7220 File Offset: 0x002A5420
		public override bool CanApplyTo(Item item)
		{
			return item is Pickaxe;
		}
	}
}
