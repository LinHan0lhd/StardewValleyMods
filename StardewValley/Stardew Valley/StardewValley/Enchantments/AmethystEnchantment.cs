using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200032E RID: 814
	public class AmethystEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x060034D5 RID: 13525 RVA: 0x002A61D4 File Offset: 0x002A43D4
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.knockback.Value += (float)base.GetLevel();
			}
		}

		// Token: 0x060034D6 RID: 13526 RVA: 0x002A620C File Offset: 0x002A440C
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.knockback.Value -= (float)base.GetLevel();
			}
		}

		// Token: 0x060034D7 RID: 13527 RVA: 0x002A6243 File Offset: 0x002A4443
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x060034D8 RID: 13528 RVA: 0x002A6246 File Offset: 0x002A4446
		public override bool IsForge()
		{
			return true;
		}
	}
}
