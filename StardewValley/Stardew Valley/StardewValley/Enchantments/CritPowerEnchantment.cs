using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033A RID: 826
	public class CritPowerEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003522 RID: 13602 RVA: 0x002A6B15 File Offset: 0x002A4D15
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x06003523 RID: 13603 RVA: 0x002A6B18 File Offset: 0x002A4D18
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x06003524 RID: 13604 RVA: 0x002A6B1B File Offset: 0x002A4D1B
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.CriticalPowerMultiplier.Value += (float)this.level.Value / 2f;
		}

		// Token: 0x06003525 RID: 13605 RVA: 0x002A6B48 File Offset: 0x002A4D48
		public override int GetMaximumLevel()
		{
			return 5;
		}

		// Token: 0x06003526 RID: 13606 RVA: 0x002A6B4B File Offset: 0x002A4D4B
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:CritPowerEnchantment", base.Level * 25);
		}
	}
}
