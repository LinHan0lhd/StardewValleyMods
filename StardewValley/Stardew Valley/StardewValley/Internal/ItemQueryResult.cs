using System;

namespace StardewValley.Internal
{
	// Token: 0x02000312 RID: 786
	public class ItemQueryResult
	{
		// Token: 0x06003430 RID: 13360 RVA: 0x0029B920 File Offset: 0x00299B20
		public ItemQueryResult(ISalable item)
		{
			this.Item = item;
		}

		// Token: 0x0400222B RID: 8747
		public ISalable Item;

		// Token: 0x0400222C RID: 8748
		public int? OverrideBasePrice;

		// Token: 0x0400222D RID: 8749
		public int? OverrideStackSize;

		// Token: 0x0400222E RID: 8750
		public int? OverrideShopAvailableStock;

		// Token: 0x0400222F RID: 8751
		public string OverrideTradeItemId;

		// Token: 0x04002230 RID: 8752
		public int? OverrideTradeItemAmount;

		// Token: 0x04002231 RID: 8753
		public Item SyncStacksWith;
	}
}
