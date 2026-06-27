using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000356 RID: 854
	public class TopazEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600358A RID: 13706 RVA: 0x002A7664 File Offset: 0x002A5864
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.addedDefense.Value += base.GetLevel();
			}
		}

		// Token: 0x0600358B RID: 13707 RVA: 0x002A769C File Offset: 0x002A589C
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.addedDefense.Value -= base.GetLevel();
			}
		}

		// Token: 0x0600358C RID: 13708 RVA: 0x002A76D2 File Offset: 0x002A58D2
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		// Token: 0x0600358D RID: 13709 RVA: 0x002A76D5 File Offset: 0x002A58D5
		public override bool IsForge()
		{
			return true;
		}
	}
}
