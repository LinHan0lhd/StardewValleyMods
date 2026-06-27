using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033B RID: 827
	public class CrusaderEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003528 RID: 13608 RVA: 0x002A6B74 File Offset: 0x002A4D74
		public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
		{
			base.OnCalculateDamage(monster, location, who, fromBomb, ref amount);
			if (!fromBomb && (monster is Ghost || monster is Skeleton || monster is Mummy || monster is ShadowBrute || monster is ShadowShaman || monster is ShadowGirl || monster is ShadowGuy || monster is Shooter))
			{
				amount = (int)((float)amount * 1.5f);
			}
		}

		// Token: 0x06003529 RID: 13609 RVA: 0x002A6BE0 File Offset: 0x002A4DE0
		public override string GetName()
		{
			return "Crusader";
		}
	}
}
