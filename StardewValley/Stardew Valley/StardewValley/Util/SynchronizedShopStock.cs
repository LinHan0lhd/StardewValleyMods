using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Netcode;
using StardewValley.GameData.Shops;
using StardewValley.Network;

namespace StardewValley.Util
{
	// Token: 0x02000123 RID: 291
	public class SynchronizedShopStock : INetObject<NetFields>
	{
		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x060017D0 RID: 6096 RVA: 0x00112377 File Offset: 0x00110577
		public NetFields NetFields { get; } = new NetFields("SynchronizedShopStock");

		// Token: 0x060017D1 RID: 6097 RVA: 0x0011237F File Offset: 0x0011057F
		public SynchronizedShopStock()
		{
			this.initNetFields();
		}

		// Token: 0x060017D2 RID: 6098 RVA: 0x001123A8 File Offset: 0x001105A8
		private void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.stockDictionary, "stockDictionary");
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x001123C7 File Offset: 0x001105C7
		public virtual void Clear()
		{
			this.stockDictionary.Clear();
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x001123D4 File Offset: 0x001105D4
		public void OnItemPurchased(string shop_id, ISalable item, Dictionary<ISalable, ItemStockInformation> stock, int amount)
		{
			NetStringDictionary<int, NetInt> sharedStock = this.stockDictionary;
			ItemStockInformation stockData;
			if (!stock.TryGetValue(item, out stockData))
			{
				return;
			}
			if (stockData.Stock == 2147483647)
			{
				return;
			}
			string key = this.GetQualifiedSyncedKey(shop_id, stockData);
			stockData.Stock -= amount;
			sharedStock[key] = stockData.Stock;
		}

		// Token: 0x060017D5 RID: 6101 RVA: 0x00112428 File Offset: 0x00110628
		public string GetQualifiedSyncedKey(string shop_id, ItemStockInformation item)
		{
			if (item.LimitedStockMode == LimitedStockMode.Global)
			{
				return shop_id + "/Global/" + item.SyncedKey;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted(shop_id);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<long>(Game1.player.UniqueMultiplayerID);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted(item.SyncedKey);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060017D6 RID: 6102 RVA: 0x001124A0 File Offset: 0x001106A0
		public void UpdateLocalStockWithSyncedQuanitities(string shop_id, Dictionary<ISalable, ItemStockInformation> local_stock)
		{
			SynchronizedShopStock._usedKeys.Clear();
			SynchronizedShopStock._stockSalables.Clear();
			List<ISalable> items_to_remove = new List<ISalable>();
			SynchronizedShopStock._stockSalables.AddRange(local_stock.Keys);
			foreach (ISalable salable in SynchronizedShopStock._stockSalables)
			{
				ItemStockInformation stock_data = local_stock[salable];
				if (stock_data.Stock != 2147483647 && stock_data.LimitedStockMode != LimitedStockMode.None)
				{
					if (stock_data.SyncedKey == null)
					{
						string base_key = salable.Name;
						string key = base_key;
						int collision_count = 1;
						while (SynchronizedShopStock._usedKeys.Contains(key))
						{
							key = base_key + collision_count.ToString();
							collision_count++;
						}
						SynchronizedShopStock._usedKeys.Add(key);
						stock_data.SyncedKey = key;
						local_stock[salable] = stock_data;
					}
					string qualified_key = this.GetQualifiedSyncedKey(shop_id, stock_data);
					int stock;
					if (this.stockDictionary.TryGetValue(qualified_key, out stock))
					{
						stock_data.Stock = stock;
						local_stock[salable] = stock_data;
						if (stock <= 0)
						{
							items_to_remove.Add(salable);
						}
					}
				}
			}
			SynchronizedShopStock._usedKeys.Clear();
			SynchronizedShopStock._stockSalables.Clear();
			foreach (ISalable salable2 in items_to_remove)
			{
				Item item = (Item)salable2;
				local_stock.Remove(item);
			}
		}

		// Token: 0x04000E53 RID: 3667
		private readonly NetStringDictionary<int, NetInt> stockDictionary = new NetStringDictionary<int, NetInt>();

		// Token: 0x04000E55 RID: 3669
		protected static HashSet<string> _usedKeys = new HashSet<string>();

		// Token: 0x04000E56 RID: 3670
		protected static List<ISalable> _stockSalables = new List<ISalable>();
	}
}
