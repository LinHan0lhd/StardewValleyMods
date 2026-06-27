using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000346 RID: 838
	public class JadeEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003550 RID: 13648 RVA: 0x002A6FBC File Offset: 0x002A51BC
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critMultiplier.Value += 0.1f * (float)base.GetLevel();
			}
		}

		// Token: 0x06003551 RID: 13649 RVA: 0x002A6FFC File Offset: 0x002A51FC
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critMultiplier.Value -= 0.1f * (float)base.GetLevel();
			}
		}

		// Token: 0x06003552 RID: 13650 RVA: 0x002A7039 File Offset: 0x002A5239
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x06003553 RID: 13651 RVA: 0x002A703C File Offset: 0x002A523C
		public override bool IsForge()
		{
			return true;
		}
	}
}
