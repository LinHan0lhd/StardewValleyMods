using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000337 RID: 823
	public class BottomlessEnchantment : WateringCanEnchantment
	{
		// Token: 0x06003515 RID: 13589 RVA: 0x002A69EA File Offset: 0x002A4BEA
		public override string GetName()
		{
			return "Bottomless";
		}

		// Token: 0x06003516 RID: 13590 RVA: 0x002A69F4 File Offset: 0x002A4BF4
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			WateringCan tool = item as WateringCan;
			if (tool != null)
			{
				tool.IsBottomless = true;
				tool.WaterLeft = tool.waterCanMax;
			}
		}

		// Token: 0x06003517 RID: 13591 RVA: 0x002A6A28 File Offset: 0x002A4C28
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			WateringCan tool = item as WateringCan;
			if (tool != null)
			{
				tool.IsBottomless = false;
			}
		}
	}
}
