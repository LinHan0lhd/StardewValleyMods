using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000330 RID: 816
	public class ArchaeologistEnchantment : HoeEnchantment
	{
		// Token: 0x060034DF RID: 13535 RVA: 0x002A62DF File Offset: 0x002A44DF
		public override string GetName()
		{
			return "Archaeologist";
		}

		// Token: 0x060034E0 RID: 13536 RVA: 0x002A62E6 File Offset: 0x002A44E6
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && (item is Hoe || item is Pan);
		}
	}
}
