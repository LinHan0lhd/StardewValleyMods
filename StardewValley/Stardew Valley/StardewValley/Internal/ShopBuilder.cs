using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;
using StardewValley.Logging;

namespace StardewValley.Internal
{
	// Token: 0x02000316 RID: 790
	public static class ShopBuilder
	{
		// Token: 0x06003438 RID: 13368 RVA: 0x0029B9D4 File Offset: 0x00299BD4
		public static Dictionary<ISalable, ItemStockInformation> GetShopStock(string shopId)
		{
			ShopData shop;
			if (DataLoader.Shops(Game1.content).TryGetValue(shopId, out shop))
			{
				return ShopBuilder.GetShopStock(shopId, shop);
			}
			return new Dictionary<ISalable, ItemStockInformation>();
		}

		// Token: 0x06003439 RID: 13369 RVA: 0x0029BA04 File Offset: 0x00299C04
		public static Dictionary<ISalable, ItemStockInformation> GetShopStock(string shopId, ShopData shop)
		{
			Dictionary<ISalable, ItemStockInformation> stock = new Dictionary<ISalable, ItemStockInformation>();
			List<ShopItemData> items = shop.Items;
			if (items != null && items.Count > 0)
			{
				Random shopRandom = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
				HashSet<string> stockedItemIds = new HashSet<string>();
				ItemQueryContext itemQueryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, shopRandom, "shop '" + shopId + "'");
				bool applyPierreStockList = shopId == "SeedShop" && Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist");
				HashSet<string> syncKeys = new HashSet<string>();
				Action<string, string> <>9__0;
				foreach (ShopItemData itemData in shop.Items)
				{
					if (!syncKeys.Add(itemData.Id))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Shop ");
						defaultInterpolatedStringHandler.AppendFormatted(shopId);
						defaultInterpolatedStringHandler.AppendLiteral(" has multiple items with entry ID '");
						defaultInterpolatedStringHandler.AppendFormatted(itemData.Id);
						defaultInterpolatedStringHandler.AppendLiteral("'. This may cause unintended behavior.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					bool isItemOutOfSeason;
					if (ShopBuilder.CheckItemCondition(itemData.Condition, applyPierreStockList, out isItemOutOfSeason))
					{
						ISpawnItemData data = itemData;
						ItemQueryContext context = itemQueryContext;
						ItemQuerySearchMode filter = ItemQuerySearchMode.All;
						bool avoidRepeat = itemData.AvoidRepeat;
						HashSet<string> avoidItemIds = itemData.AvoidRepeat ? stockedItemIds : null;
						Func<string, string> formatItemId = null;
						Action<string, string> logError;
						if ((logError = <>9__0) == null)
						{
							logError = (<>9__0 = delegate(string query, string message)
							{
								IGameLogger log2 = Game1.log;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 3);
								defaultInterpolatedStringHandler2.AppendLiteral("Failed parsing shop item query '");
								defaultInterpolatedStringHandler2.AppendFormatted(query);
								defaultInterpolatedStringHandler2.AppendLiteral("' for the '");
								defaultInterpolatedStringHandler2.AppendFormatted(shopId);
								defaultInterpolatedStringHandler2.AppendLiteral("' shop: ");
								defaultInterpolatedStringHandler2.AppendFormatted(message);
								defaultInterpolatedStringHandler2.AppendLiteral(".");
								log2.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
							});
						}
						IEnumerable<ItemQueryResult> enumerable = ItemQueryResolver.TryResolve(data, context, filter, avoidRepeat, avoidItemIds, formatItemId, logError, null);
						int i = 0;
						foreach (ItemQueryResult shopItem in enumerable)
						{
							ISalable item = shopItem.Item;
							item.Stack = (shopItem.OverrideStackSize ?? item.Stack);
							float price = (float)ShopBuilder.GetBasePrice(shopItem, shop, itemData, item, isItemOutOfSeason, itemData.UseObjectDataPrice);
							int availableStock = shopItem.OverrideShopAvailableStock ?? itemData.AvailableStock;
							LimitedStockMode availableStockLimit = itemData.AvailableStockLimit;
							string tradeItemId = shopItem.OverrideTradeItemId ?? itemData.TradeItemId;
							int? num = shopItem.OverrideTradeItemAmount;
							int num2 = 0;
							int? tradeItemAmount = (num.GetValueOrDefault() > num2 & num != null) ? shopItem.OverrideTradeItemAmount : new int?(itemData.TradeItemAmount);
							if (tradeItemId == null)
							{
								goto IL_27E;
							}
							num = tradeItemAmount;
							num2 = 0;
							if (num.GetValueOrDefault() < num2 & num != null)
							{
								goto IL_27E;
							}
							IL_289:
							if (itemData.IsRecipe)
							{
								item.Stack = 1;
								availableStockLimit = LimitedStockMode.None;
								availableStock = 1;
							}
							if (!itemData.IgnoreShopPriceModifiers)
							{
								price = Utility.ApplyQuantityModifiers(price, shop.PriceModifiers, shop.PriceModifierMode, null, null, item as Item, null, shopRandom);
							}
							price = Utility.ApplyQuantityModifiers(price, itemData.PriceModifiers, itemData.PriceModifierMode, null, null, item as Item, null, shopRandom);
							if (!itemData.IsRecipe)
							{
								availableStock = (int)Utility.ApplyQuantityModifiers((float)availableStock, itemData.AvailableStockModifiers, itemData.AvailableStockModifierMode, null, null, item as Item, null, shopRandom);
							}
							if (!ShopBuilder.TrackSeenItems(stockedItemIds, item) || !itemData.AvoidRepeat)
							{
								if (availableStock < 0)
								{
									availableStock = int.MaxValue;
								}
								string syncKey = itemData.Id;
								if (++i > 1)
								{
									syncKey += i.ToString();
								}
								Dictionary<ISalable, ItemStockInformation> dictionary = stock;
								ISalable key = item;
								int price2 = (int)price;
								int stock2 = availableStock;
								string tradeItem = tradeItemId;
								int? tradeItemCount = tradeItemAmount;
								LimitedStockMode stockMode = availableStockLimit;
								string syncedKey = syncKey;
								ISalable syncStacksWith = shopItem.SyncStacksWith;
								List<string> actionsOnPurchase = itemData.ActionsOnPurchase;
								dictionary.Add(key, new ItemStockInformation(price2, stock2, tradeItem, tradeItemCount, stockMode, syncedKey, syncStacksWith, null, actionsOnPurchase));
								continue;
							}
							continue;
							IL_27E:
							tradeItemId = null;
							tradeItemAmount = null;
							goto IL_289;
						}
					}
				}
			}
			Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(shopId, stock);
			return stock;
		}

		// Token: 0x0600343A RID: 13370 RVA: 0x0029BE28 File Offset: 0x0029A028
		public static bool CheckItemCondition(string conditions, bool applyPierreMissingStockList, out bool isOutOfSeason)
		{
			if (conditions == null || GameStateQuery.CheckConditions(conditions, null, null, null, null, null, null))
			{
				isOutOfSeason = false;
				return true;
			}
			if (applyPierreMissingStockList && GameStateQuery.CheckConditions(conditions, null, null, null, null, null, GameStateQuery.SeasonQueryKeys))
			{
				isOutOfSeason = true;
				return true;
			}
			isOutOfSeason = false;
			return false;
		}

		// Token: 0x0600343B RID: 13371 RVA: 0x0029BE60 File Offset: 0x0029A060
		public static ToolUpgradeData GetToolUpgradeData(ToolData tool, Farmer player)
		{
			if (tool == null)
			{
				return null;
			}
			IList<ToolUpgradeData> upgradeFrom = tool.UpgradeFrom;
			if (tool.ConventionalUpgradeFrom != null)
			{
				IList<ToolUpgradeData> conventional = new ToolUpgradeData[]
				{
					new ToolUpgradeData
					{
						RequireToolId = tool.ConventionalUpgradeFrom,
						Price = ShopBuilder.GetToolUpgradeConventionalPrice(tool.UpgradeLevel),
						TradeItemId = ShopBuilder.GetToolUpgradeConventionalTradeItem(tool.UpgradeLevel),
						TradeItemAmount = 5
					}
				};
				IList<ToolUpgradeData> list;
				if (upgradeFrom == null || upgradeFrom.Count <= 0)
				{
					list = conventional;
				}
				else
				{
					IList<ToolUpgradeData> list2 = conventional.Concat(upgradeFrom).ToList<ToolUpgradeData>();
					list = list2;
				}
				upgradeFrom = list;
			}
			if (upgradeFrom == null)
			{
				return null;
			}
			foreach (ToolUpgradeData upgrade in upgradeFrom)
			{
				if ((upgrade.Condition == null || GameStateQuery.CheckConditions(upgrade.Condition, player.currentLocation, player, null, null, null, null)) && (upgrade.RequireToolId == null || player.Items.ContainsId(upgrade.RequireToolId)))
				{
					return upgrade;
				}
			}
			return null;
		}

		// Token: 0x0600343C RID: 13372 RVA: 0x0029BF68 File Offset: 0x0029A168
		public static int GetToolUpgradeConventionalPrice(int level)
		{
			switch (level)
			{
			case 1:
				return 2000;
			case 2:
				return 5000;
			case 3:
				return 10000;
			case 4:
				return 25000;
			default:
				return 2000;
			}
		}

		// Token: 0x0600343D RID: 13373 RVA: 0x0029BFA1 File Offset: 0x0029A1A1
		private static string GetToolUpgradeConventionalTradeItem(int level)
		{
			switch (level)
			{
			case 1:
				return "334";
			case 2:
				return "335";
			case 3:
				return "336";
			case 4:
				return "337";
			default:
				return "334";
			}
		}

		// Token: 0x0600343E RID: 13374 RVA: 0x0029BFDC File Offset: 0x0029A1DC
		public static IEnumerable<ShopOwnerData> GetCurrentOwners(ShopData shop)
		{
			IEnumerable<ShopOwnerData> enumerable;
			if (shop == null)
			{
				enumerable = null;
			}
			else
			{
				List<ShopOwnerData> owners = shop.Owners;
				if (owners == null)
				{
					enumerable = null;
				}
				else
				{
					enumerable = from owner in owners
					where GameStateQuery.CheckConditions(owner.Condition, null, null, null, null, null, null)
					select owner;
				}
			}
			return enumerable ?? LegacyShims.EmptyArray<ShopOwnerData>();
		}

		// Token: 0x0600343F RID: 13375 RVA: 0x0029C02C File Offset: 0x0029A22C
		public static int GetBasePrice(ItemQueryResult output, ShopData shopData, ShopItemData itemData, ISalable item, bool outOfSeasonPrice, bool useObjectDataPrice = false)
		{
			float price = (float)(output.OverrideBasePrice ?? itemData.Price);
			if (price < 0f)
			{
				if (itemData.TradeItemId != null)
				{
					price = 0f;
				}
				else
				{
					if (useObjectDataPrice && item.HasTypeObject())
					{
						Object obj = item as Object;
						if (obj != null)
						{
							price = (float)obj.Price;
							goto IL_62;
						}
					}
					price = (float)item.salePrice(true);
				}
			}
			IL_62:
			if (itemData.ApplyProfitMargins ?? (shopData.ApplyProfitMargins ?? item.appliesProfitMargins()))
			{
				price *= Game1.MasterPlayer.difficultyModifier;
			}
			if (outOfSeasonPrice)
			{
				price *= 1.5f;
			}
			return (int)price;
		}

		// Token: 0x06003440 RID: 13376 RVA: 0x0029C0F4 File Offset: 0x0029A2F4
		public static bool TrackSeenItems(HashSet<string> stockedItems, ISalable item)
		{
			string fullyQualifiedId = item.QualifiedItemId;
			Tool tool = item as Tool;
			if (tool != null && tool.UpgradeLevel > 0)
			{
				fullyQualifiedId = fullyQualifiedId + "#" + tool.UpgradeLevel.ToString();
			}
			if (item.IsRecipe)
			{
				fullyQualifiedId += "#Recipe";
			}
			return !stockedItems.Add(fullyQualifiedId);
		}
	}
}
