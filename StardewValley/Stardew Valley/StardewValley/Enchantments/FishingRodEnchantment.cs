using System;
using System.Xml.Serialization;
using StardewValley.Tools;

namespace StardewValley.Enchantments
{
	// Token: 0x02000341 RID: 833
	[XmlInclude(typeof(FishingRodEnchantment))]
	public class FishingRodEnchantment : BaseEnchantment
	{
		// Token: 0x06003541 RID: 13633 RVA: 0x002A6D92 File Offset: 0x002A4F92
		public override bool CanApplyTo(Item item)
		{
			return item is FishingRod;
		}
	}
}
