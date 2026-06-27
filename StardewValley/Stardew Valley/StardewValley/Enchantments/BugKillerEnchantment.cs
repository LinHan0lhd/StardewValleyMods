using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments
{
	// Token: 0x02000338 RID: 824
	public class BugKillerEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003519 RID: 13593 RVA: 0x002A6A58 File Offset: 0x002A4C58
		public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
		{
			base.OnCalculateDamage(monster, location, who, fromBomb, ref amount);
			if (!fromBomb && (monster is Grub || monster is Fly || monster is Bug || monster is Leaper || monster is RockCrab))
			{
				amount = (int)((float)amount * 2f);
			}
		}

		// Token: 0x0600351A RID: 13594 RVA: 0x002A6AAC File Offset: 0x002A4CAC
		public override string GetName()
		{
			return "Bug Killer";
		}
	}
}
