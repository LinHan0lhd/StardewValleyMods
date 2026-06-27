using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200032F RID: 815
	public class AquamarineEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x060034DA RID: 13530 RVA: 0x002A6254 File Offset: 0x002A4454
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critChance.Value += 0.046f * (float)base.GetLevel();
			}
		}

		// Token: 0x060034DB RID: 13531 RVA: 0x002A6294 File Offset: 0x002A4494
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critChance.Value -= 0.046f * (float)base.GetLevel();
			}
		}

		// Token: 0x060034DC RID: 13532 RVA: 0x002A62D1 File Offset: 0x002A44D1
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x060034DD RID: 13533 RVA: 0x002A62D4 File Offset: 0x002A44D4
		public override bool IsForge()
		{
			return true;
		}
	}
}
