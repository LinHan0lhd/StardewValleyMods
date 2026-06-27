using System;

namespace StardewValley.Enchantments
{
	// Token: 0x02000342 RID: 834
	public class GalaxySoulEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003543 RID: 13635 RVA: 0x002A6DA7 File Offset: 0x002A4FA7
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x06003544 RID: 13636 RVA: 0x002A6DAA File Offset: 0x002A4FAA
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x06003545 RID: 13637 RVA: 0x002A6DAD File Offset: 0x002A4FAD
		public override int GetMaximumLevel()
		{
			return 3;
		}

		// Token: 0x06003546 RID: 13638 RVA: 0x002A6DB0 File Offset: 0x002A4FB0
		public override bool ShouldBeDisplayed()
		{
			return false;
		}
	}
}
