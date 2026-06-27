using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x02000332 RID: 818
	public class AttackEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x060034E4 RID: 13540 RVA: 0x002A631C File Offset: 0x002A451C
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x060034E5 RID: 13541 RVA: 0x002A631F File Offset: 0x002A451F
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x060034E6 RID: 13542 RVA: 0x002A6322 File Offset: 0x002A4522
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.Attack.Value += (float)this.level.Value;
		}

		// Token: 0x060034E7 RID: 13543 RVA: 0x002A6349 File Offset: 0x002A4549
		public override int GetMaximumLevel()
		{
			return 5;
		}

		// Token: 0x060034E8 RID: 13544 RVA: 0x002A634C File Offset: 0x002A454C
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:AttackEnchantment", base.Level);
		}
	}
}
