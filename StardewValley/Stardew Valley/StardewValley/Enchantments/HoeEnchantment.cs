using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000345 RID: 837
	public class HoeEnchantment : BaseEnchantment
	{
		// Token: 0x0600354E RID: 13646 RVA: 0x002A6FA7 File Offset: 0x002A51A7
		public override bool CanApplyTo(Item item)
		{
			return item is Hoe;
		}
	}
}
