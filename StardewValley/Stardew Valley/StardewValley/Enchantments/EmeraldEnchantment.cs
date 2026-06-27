using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033F RID: 831
	public class EmeraldEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003539 RID: 13625 RVA: 0x002A6CF0 File Offset: 0x002A4EF0
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.speed.Value += 5 * base.GetLevel();
			}
		}

		// Token: 0x0600353A RID: 13626 RVA: 0x002A6D28 File Offset: 0x002A4F28
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.speed.Value -= 5 * base.GetLevel();
			}
		}

		// Token: 0x0600353B RID: 13627 RVA: 0x002A6D60 File Offset: 0x002A4F60
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x0600353C RID: 13628 RVA: 0x002A6D63 File Offset: 0x002A4F63
		public override bool IsForge()
		{
			return true;
		}
	}
}
