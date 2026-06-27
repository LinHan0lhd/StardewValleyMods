using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200034A RID: 842
	public class MilkPailEnchantment : BaseEnchantment
	{
		// Token: 0x06003560 RID: 13664 RVA: 0x002A71F6 File Offset: 0x002A53F6
		public override bool CanApplyTo(Item item)
		{
			return item is MilkPail;
		}
	}
}
