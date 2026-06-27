using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200034D RID: 845
	public class PowerfulEnchantment : BaseEnchantment
	{
		// Token: 0x06003566 RID: 13670 RVA: 0x002A7235 File Offset: 0x002A5435
		public override string GetName()
		{
			return "Powerful";
		}

		// Token: 0x06003567 RID: 13671 RVA: 0x002A723C File Offset: 0x002A543C
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && (item is Pickaxe || item is Axe);
		}

		// Token: 0x06003568 RID: 13672 RVA: 0x002A725C File Offset: 0x002A545C
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			Pickaxe pickaxe = item as Pickaxe;
			if (pickaxe != null)
			{
				pickaxe.additionalPower.Value += base.GetLevel();
				return;
			}
			Axe axe = item as Axe;
			if (axe == null)
			{
				return;
			}
			axe.additionalPower.Value += 2 * base.GetLevel();
		}

		// Token: 0x06003569 RID: 13673 RVA: 0x002A72B8 File Offset: 0x002A54B8
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			Pickaxe pickaxe = item as Pickaxe;
			if (pickaxe != null)
			{
				pickaxe.additionalPower.Value -= base.GetLevel();
				return;
			}
			Axe axe = item as Axe;
			if (axe == null)
			{
				return;
			}
			axe.additionalPower.Value -= 2 * base.GetLevel();
		}
	}
}
