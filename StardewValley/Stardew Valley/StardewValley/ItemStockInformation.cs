using System;
using System.Collections.Generic;
using StardewValley.GameData.Shops;

namespace StardewValley
{
	// Token: 0x020000BC RID: 188
	public class ItemStockInformation
	{
		// Token: 0x06000D58 RID: 3416 RVA: 0x00092284 File Offset: 0x00090484
		public ItemStockInformation(int price, int stock, string tradeItem = null, int? tradeItemCount = null, LimitedStockMode stockMode = LimitedStockMode.Global, string syncedKey = null, ISalable itemToSyncStack = null, StackDrawType? stackDrawType = null, List<string> actionsOnPurchase = null)
		{
			this.Price = price;
			this.Stock = stock;
			this.TradeItem = tradeItem;
			this.TradeItemCount = tradeItemCount;
			this.LimitedStockMode = stockMode;
			this.SyncedKey = syncedKey;
			this.ItemToSyncStack = itemToSyncStack;
			this.StackDrawType = stackDrawType;
			this.ActionsOnPurchase = actionsOnPurchase;
		}

		// Token: 0x040008E9 RID: 2281
		public int Price;

		// Token: 0x040008EA RID: 2282
		public int Stock;

		// Token: 0x040008EB RID: 2283
		public string TradeItem;

		// Token: 0x040008EC RID: 2284
		public int? TradeItemCount;

		// Token: 0x040008ED RID: 2285
		public LimitedStockMode LimitedStockMode;

		// Token: 0x040008EE RID: 2286
		public string SyncedKey;

		// Token: 0x040008EF RID: 2287
		public ISalable ItemToSyncStack;

		// Token: 0x040008F0 RID: 2288
		public StackDrawType? StackDrawType;

		// Token: 0x040008F1 RID: 2289
		public List<string> ActionsOnPurchase;
	}
}
