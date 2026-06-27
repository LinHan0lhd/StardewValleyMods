using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000343 RID: 835
	public class GenerousEnchantment : HoeEnchantment
	{
		// Token: 0x06003548 RID: 13640 RVA: 0x002A6DBB File Offset: 0x002A4FBB
		public override string GetName()
		{
			return "Generous";
		}

		// Token: 0x06003549 RID: 13641 RVA: 0x002A6DC2 File Offset: 0x002A4FC2
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && (item is Hoe || item is Pan);
		}
	}
}
