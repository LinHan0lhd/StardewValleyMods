using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments
{
	// Token: 0x02000353 RID: 851
	public class SlimeGathererEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003579 RID: 13689 RVA: 0x002A74B8 File Offset: 0x002A56B8
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x0600357A RID: 13690 RVA: 0x002A74BB File Offset: 0x002A56BB
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x0600357B RID: 13691 RVA: 0x002A74C0 File Offset: 0x002A56C0
		public override void OnMonsterSlay(Monster monster, GameLocation location, Farmer who, bool slainByBomb)
		{
			base.OnMonsterSlay(monster, location, who, slainByBomb);
			if (!slainByBomb && (monster is GreenSlime || monster is BigSlime))
			{
				int toDrop = 1 + Game1.random.Next((int)Math.Ceiling(Math.Sqrt((double)monster.MaxHealth) / 3.0));
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)766", toDrop, 0, false), monster.getStandingPosition(), -1, null, -1, false);
			}
		}

		// Token: 0x0600357C RID: 13692 RVA: 0x002A7531 File Offset: 0x002A5731
		public override int GetMaximumLevel()
		{
			return 5;
		}

		// Token: 0x0600357D RID: 13693 RVA: 0x002A7534 File Offset: 0x002A5734
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:SlimeGathererEnchantment");
		}
	}
}
