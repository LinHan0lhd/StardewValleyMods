using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000336 RID: 822
	public class BaseWeaponEnchantment : BaseEnchantment
	{
		// Token: 0x06003511 RID: 13585 RVA: 0x002A69B4 File Offset: 0x002A4BB4
		public override bool CanApplyTo(Item item)
		{
			MeleeWeapon weapon = item as MeleeWeapon;
			return weapon != null && !weapon.isScythe();
		}

		// Token: 0x06003512 RID: 13586 RVA: 0x002A69D6 File Offset: 0x002A4BD6
		public void OnSwing(MeleeWeapon weapon, Farmer farmer)
		{
			this._OnSwing(weapon, farmer);
		}

		// Token: 0x06003513 RID: 13587 RVA: 0x002A69E0 File Offset: 0x002A4BE0
		protected virtual void _OnSwing(MeleeWeapon weapon, Farmer farmer)
		{
		}
	}
}
