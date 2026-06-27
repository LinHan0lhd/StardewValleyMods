using System;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000355 RID: 853
	public class SwiftToolEnchantment : BaseEnchantment
	{
		// Token: 0x06003585 RID: 13701 RVA: 0x002A759E File Offset: 0x002A579E
		public override string GetName()
		{
			return "Swift";
		}

		// Token: 0x06003586 RID: 13702 RVA: 0x002A75A8 File Offset: 0x002A57A8
		public override bool CanApplyTo(Item item)
		{
			return item is Tool && !(item is MilkPail) && !(item is MeleeWeapon) && !(item is Shears) && !(item is FishingRod) && !(item is Pan) && !(item is WateringCan) && !(item is Wand) && !(item is Slingshot);
		}

		// Token: 0x06003587 RID: 13703 RVA: 0x002A7604 File Offset: 0x002A5804
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.AnimationSpeedModifier = 0.66f;
			}
		}

		// Token: 0x06003588 RID: 13704 RVA: 0x002A7630 File Offset: 0x002A5830
		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.AnimationSpeedModifier = 1f;
			}
		}
	}
}
