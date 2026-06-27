using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x0200033E RID: 830
	public class EfficientToolEnchantment : BaseEnchantment
	{
		// Token: 0x06003534 RID: 13620 RVA: 0x002A6C51 File Offset: 0x002A4E51
		public override string GetName()
		{
			return "Efficient";
		}

		// Token: 0x06003535 RID: 13621 RVA: 0x002A6C58 File Offset: 0x002A4E58
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && !(item is MilkPail) && !(item is MeleeWeapon) && !(item is Shears) && !(item is Pan) && !(item is Wand) && !(item is Slingshot);
		}

		// Token: 0x06003536 RID: 13622 RVA: 0x002A6C98 File Offset: 0x002A4E98
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.IsEfficient = true;
			}
		}

		// Token: 0x06003537 RID: 13623 RVA: 0x002A6CC0 File Offset: 0x002A4EC0
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.IsEfficient = false;
			}
		}
	}
}
