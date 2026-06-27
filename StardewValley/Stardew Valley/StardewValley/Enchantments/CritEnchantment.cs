using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x02000339 RID: 825
	public class CritEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x0600351C RID: 13596 RVA: 0x002A6ABB File Offset: 0x002A4CBB
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x0600351D RID: 13597 RVA: 0x002A6ABE File Offset: 0x002A4CBE
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x0600351E RID: 13598 RVA: 0x002A6AC1 File Offset: 0x002A4CC1
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.CriticalChanceMultiplier.Value += 0.02f * (float)this.level.Value;
		}

		// Token: 0x0600351F RID: 13599 RVA: 0x002A6AEE File Offset: 0x002A4CEE
		public override int GetMaximumLevel()
		{
			return 3;
		}

		// Token: 0x06003520 RID: 13600 RVA: 0x002A6AF1 File Offset: 0x002A4CF1
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:CritEnchantment", base.Level);
		}
	}
}
