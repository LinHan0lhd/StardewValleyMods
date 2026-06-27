using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033C RID: 828
	public class DefenseEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600352B RID: 13611 RVA: 0x002A6BEF File Offset: 0x002A4DEF
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x0600352C RID: 13612 RVA: 0x002A6BF2 File Offset: 0x002A4DF2
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x0600352D RID: 13613 RVA: 0x002A6BF5 File Offset: 0x002A4DF5
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.Defense.Value += (float)this.level.Value;
		}

		// Token: 0x0600352E RID: 13614 RVA: 0x002A6C1C File Offset: 0x002A4E1C
		public override int GetMaximumLevel()
		{
			return 3;
		}

		// Token: 0x0600352F RID: 13615 RVA: 0x002A6C1F File Offset: 0x002A4E1F
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:DefenseEnchantment", base.Level);
		}
	}
}
