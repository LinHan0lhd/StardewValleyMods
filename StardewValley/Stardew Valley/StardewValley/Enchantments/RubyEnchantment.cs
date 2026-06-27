using System;
using StardewValley.GameData.Weapons;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000350 RID: 848
	public class RubyEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003570 RID: 13680 RVA: 0x002A7378 File Offset: 0x002A5578
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				WeaponData data = weapon.GetData();
				if (data != null)
				{
					int baseMin = data.MinDamage;
					int baseMax = data.MaxDamage;
					weapon.minDamage.Value += Math.Max(1, (int)((float)baseMin * 0.1f)) * base.GetLevel();
					weapon.maxDamage.Value += Math.Max(1, (int)((float)baseMax * 0.1f)) * base.GetLevel();
				}
			}
		}

		// Token: 0x06003571 RID: 13681 RVA: 0x002A7400 File Offset: 0x002A5600
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				WeaponData data = weapon.GetData();
				if (data != null)
				{
					int baseMin = data.MinDamage;
					int baseMax = data.MaxDamage;
					weapon.minDamage.Value -= Math.Max(1, (int)((float)baseMin * 0.1f)) * base.GetLevel();
					weapon.maxDamage.Value -= Math.Max(1, (int)((float)baseMax * 0.1f)) * base.GetLevel();
				}
			}
		}

		// Token: 0x06003572 RID: 13682 RVA: 0x002A7486 File Offset: 0x002A5686
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x06003573 RID: 13683 RVA: 0x002A7489 File Offset: 0x002A5689
		public override bool IsForge()
		{
			return true;
		}
	}
}
