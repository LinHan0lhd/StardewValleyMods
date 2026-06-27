using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x02000347 RID: 839
	public class LightweightEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003555 RID: 13653 RVA: 0x002A7047 File Offset: 0x002A5247
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x06003556 RID: 13654 RVA: 0x002A704A File Offset: 0x002A524A
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x06003557 RID: 13655 RVA: 0x002A704D File Offset: 0x002A524D
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.KnockbackMultiplier.Value -= (float)this.level.Value * 0.1f;
		}

		// Token: 0x06003558 RID: 13656 RVA: 0x002A707A File Offset: 0x002A527A
		public override int GetMaximumLevel()
		{
			return 5;
		}

		// Token: 0x06003559 RID: 13657 RVA: 0x002A707D File Offset: 0x002A527D
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:LightweightEnchantment", base.Level);
		}
	}
}
