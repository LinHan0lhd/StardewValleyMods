using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments
{
	// Token: 0x02000354 RID: 852
	public class SlimeSlayerEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600357F RID: 13695 RVA: 0x002A754D File Offset: 0x002A574D
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x06003580 RID: 13696 RVA: 0x002A7550 File Offset: 0x002A5750
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x06003581 RID: 13697 RVA: 0x002A7553 File Offset: 0x002A5753
		public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
		{
			base.OnCalculateDamage(monster, location, who, fromBomb, ref amount);
			if (!fromBomb && monster is GreenSlime)
			{
				amount = (int)((float)amount * 1.33f + 1f);
			}
		}

		// Token: 0x06003582 RID: 13698 RVA: 0x002A7582 File Offset: 0x002A5782
		public override int GetMaximumLevel()
		{
			return 5;
		}

		// Token: 0x06003583 RID: 13699 RVA: 0x002A7585 File Offset: 0x002A5785
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:SlimeSlayerEnchantment");
		}
	}
}
