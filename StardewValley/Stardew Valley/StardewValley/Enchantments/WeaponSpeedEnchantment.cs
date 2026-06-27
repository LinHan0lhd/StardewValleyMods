using System;
using StardewValley.Buffs;

namespace StardewValley.Enchantments
{
	// Token: 0x02000359 RID: 857
	public class WeaponSpeedEnchantment : BaseWeaponEnchantment
	{
		// Token: 0x06003594 RID: 13716 RVA: 0x002A77BF File Offset: 0x002A59BF
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		// Token: 0x06003595 RID: 13717 RVA: 0x002A77C2 File Offset: 0x002A59C2
		public override bool IsForge()
		{
			return false;
		}

		// Token: 0x06003596 RID: 13718 RVA: 0x002A77C5 File Offset: 0x002A59C5
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			effects.WeaponSpeedMultiplier.Value += (float)this.level.Value * 0.1f;
		}

		// Token: 0x06003597 RID: 13719 RVA: 0x002A77F2 File Offset: 0x002A59F2
		public override int GetMaximumLevel()
		{
			return 3;
		}

		// Token: 0x06003598 RID: 13720 RVA: 0x002A77F5 File Offset: 0x002A59F5
		public override string GetName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:SpeedEnchantment", base.Level);
		}
	}
}
