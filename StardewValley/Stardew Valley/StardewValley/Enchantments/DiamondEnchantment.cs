using System;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033D RID: 829
	public class DiamondEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003531 RID: 13617 RVA: 0x002A6C43 File Offset: 0x002A4E43
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x06003532 RID: 13618 RVA: 0x002A6C46 File Offset: 0x002A4E46
		public override bool IsForge()
		{
			return true;
		}
	}
}
