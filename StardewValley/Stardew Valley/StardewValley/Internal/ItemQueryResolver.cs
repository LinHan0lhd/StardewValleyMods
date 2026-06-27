using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Internal
{
	// Token: 0x02000311 RID: 785
	public static class ItemQueryResolver
	{
		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06003425 RID: 13349 RVA: 0x0029B149 File Offset: 0x00299349
		public static Dictionary<string, ResolveItemQueryDelegate> ItemResolvers { get; } = new Dictionary<string, ResolveItemQueryDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x06003426 RID: 13350 RVA: 0x0029B150 File Offset: 0x00299350
		static ItemQueryResolver()
		{
			foreach (MethodInfo method in typeof(ItemQueryResolver.DefaultResolvers).GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				ResolveItemQueryDelegate queryDelegate = (ResolveItemQueryDelegate)Delegate.CreateDelegate(typeof(ResolveItemQueryDelegate), method);
				ItemQueryResolver.Register(method.Name, queryDelegate);
			}
		}

		// Token: 0x06003427 RID: 13351 RVA: 0x0029B1B4 File Offset: 0x002993B4
		public static void Register(string queryKey, ResolveItemQueryDelegate queryDelegate)
		{
			if (string.IsNullOrWhiteSpace(queryKey))
			{
				throw new ArgumentException("The query key can't be null or empty.", "queryKey");
			}
			if (ItemQueryResolver.ItemResolvers.ContainsKey(queryKey))
			{
				throw new InvalidOperationException("The query key '" + queryKey + "' is already registered.");
			}
			Dictionary<string, ResolveItemQueryDelegate> itemResolvers = ItemQueryResolver.ItemResolvers;
			string key = queryKey.Trim();
			if (queryDelegate == null)
			{
				throw new ArgumentNullException("queryDelegate");
			}
			itemResolvers[key] = queryDelegate;
		}

		// Token: 0x06003428 RID: 13352 RVA: 0x0029B21C File Offset: 0x0029941C
		public static ItemQueryResult[] TryResolve(string query, ItemQueryContext context, ItemQuerySearchMode filter = ItemQuerySearchMode.All, string perItemCondition = null, int? maxItems = null, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Action<string, string> logError = null)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return ItemQueryResolver.Helpers.ErrorResult(query, "", logError, "must specify an item ID or query");
			}
			string queryKey = query;
			string arguments = null;
			int splitIndex = query.IndexOf(' ');
			if (splitIndex > -1)
			{
				queryKey = query.Substring(0, splitIndex);
				arguments = query.Substring(splitIndex + 1);
			}
			if (context == null)
			{
				context = new ItemQueryContext();
			}
			context.QueryString = query;
			if (context.ParentContext != null)
			{
				List<string> path = new List<string>();
				for (ItemQueryContext cur = context; cur != null; cur = cur.ParentContext)
				{
					bool flag = path.Contains(cur.QueryString);
					path.Add(cur.QueryString);
					if (flag)
					{
						if (logError != null)
						{
							logError(query, "detected circular reference in item queries: " + string.Join(" -> ", path));
						}
						return LegacyShims.EmptyArray<ItemQueryResult>();
					}
				}
			}
			ResolveItemQueryDelegate resolver;
			if (!ItemQueryResolver.ItemResolvers.TryGetValue(queryKey, out resolver))
			{
				Item instance = ItemRegistry.Create(query, 1, 0, false);
				if (instance != null)
				{
					HashSet<string> avoidItemIds2 = avoidItemIds;
					if (avoidItemIds2 == null || !avoidItemIds2.Contains(instance.QualifiedItemId))
					{
						return new ItemQueryResult[]
						{
							new ItemQueryResult(instance)
						};
					}
				}
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			IEnumerable<ItemQueryResult> results = resolver(queryKey, arguments ?? string.Empty, context, avoidRepeat, avoidItemIds, logError ?? new Action<string, string>(ItemQueryResolver.LogNothing));
			ItemQueryResult[] rawArray = results as ItemQueryResult[];
			if (rawArray != null && rawArray.Length == 0)
			{
				return rawArray;
			}
			HashSet<string> duplicates = avoidRepeat ? new HashSet<string>() : null;
			if (!avoidRepeat)
			{
				HashSet<string> avoidItemIds3 = avoidItemIds;
				if ((avoidItemIds3 == null || avoidItemIds3.Count <= 0) && GameStateQuery.IsImmutablyFalse(perItemCondition))
				{
					goto IL_174;
				}
			}
			results = results.Where(delegate(ItemQueryResult result)
			{
				HashSet<string> avoidItemIds4 = avoidItemIds;
				if (avoidItemIds4 == null || !avoidItemIds4.Contains(result.Item.QualifiedItemId))
				{
					HashSet<string> duplicates = duplicates;
					if (duplicates == null || duplicates.Add(result.Item.QualifiedItemId))
					{
						return GameStateQuery.CheckConditions(perItemCondition, null, null, result.Item as Item, null, null, null);
					}
				}
				return false;
			});
			IL_174:
			switch (filter)
			{
			case ItemQuerySearchMode.AllOfTypeItem:
				results = from result in results
				where result.Item is Item
				select result;
				break;
			case ItemQuerySearchMode.FirstOfTypeItem:
			{
				ItemQueryResult result3 = results.FirstOrDefault((ItemQueryResult p) => p.Item is Item);
				ItemQueryResult[] array;
				if (result3 == null)
				{
					array = LegacyShims.EmptyArray<ItemQueryResult>();
				}
				else
				{
					(array = new ItemQueryResult[1])[0] = result3;
				}
				results = array;
				break;
			}
			case ItemQuerySearchMode.RandomOfTypeItem:
			{
				ItemQueryResult result2 = (context.Random ?? Game1.random).ChooseFrom((from p in results
				where p.Item is Item
				select p).ToArray<ItemQueryResult>());
				ItemQueryResult[] array2;
				if (result2 == null)
				{
					array2 = LegacyShims.EmptyArray<ItemQueryResult>();
				}
				else
				{
					(array2 = new ItemQueryResult[1])[0] = result2;
				}
				results = array2;
				break;
			}
			}
			if (maxItems != null)
			{
				results = results.Take(maxItems.Value);
			}
			return (results as ItemQueryResult[]) ?? results.ToArray<ItemQueryResult>();
		}

		// Token: 0x06003429 RID: 13353 RVA: 0x0029B4F0 File Offset: 0x002996F0
		public static IList<ItemQueryResult> TryResolve(ISpawnItemData data, ItemQueryContext context, ItemQuerySearchMode filter = ItemQuerySearchMode.All, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Func<string, string> formatItemId = null, Action<string, string> logError = null, Item inputItem = null)
		{
			Random random = ((context != null) ? context.Random : null) ?? Game1.random;
			string itemId = data.ItemId;
			List<string> randomItemId = data.RandomItemId;
			if (randomItemId != null && randomItemId.Any<string>())
			{
				if (avoidItemIds != null)
				{
					if (!Utility.TryGetRandomExcept<string>(data.RandomItemId, avoidItemIds, random, out itemId))
					{
						return LegacyShims.EmptyArray<ItemQueryResult>();
					}
				}
				else
				{
					itemId = random.ChooseFrom(data.RandomItemId);
				}
			}
			if (string.IsNullOrWhiteSpace(itemId))
			{
				Game1.log.Warn(ItemQueryResolver.FormatLogMessage("Item spawn fields for {0} produced a null or empty item ID.", data, context));
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}
			if (formatItemId != null)
			{
				itemId = formatItemId(itemId);
			}
			ItemQueryResult[] results = ItemQueryResolver.TryResolve(itemId, context, filter, data.PerItemCondition, data.MaxItems, avoidRepeat, avoidItemIds, logError);
			foreach (ItemQueryResult itemQueryResult in results)
			{
				itemQueryResult.Item = ItemQueryResolver.ApplyItemFields(itemQueryResult.Item, data, context, inputItem);
			}
			return results;
		}

		// Token: 0x0600342A RID: 13354 RVA: 0x0029B5D0 File Offset: 0x002997D0
		public static Item TryResolveRandomItem(string query, ItemQueryContext context, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Action<string, string> logError = null)
		{
			ItemQueryResult itemQueryResult = ItemQueryResolver.TryResolve(query, context, ItemQuerySearchMode.RandomOfTypeItem, null, null, avoidRepeat, avoidItemIds, logError).FirstOrDefault<ItemQueryResult>();
			return ((itemQueryResult != null) ? itemQueryResult.Item : null) as Item;
		}

		// Token: 0x0600342B RID: 13355 RVA: 0x0029B609 File Offset: 0x00299809
		public static Item TryResolveRandomItem(ISpawnItemData data, ItemQueryContext context, bool avoidRepeat = false, HashSet<string> avoidItemIds = null, Func<string, string> formatItemId = null, Item inputItem = null, Action<string, string> logError = null)
		{
			ItemQueryResult itemQueryResult = ItemQueryResolver.TryResolve(data, context, ItemQuerySearchMode.RandomOfTypeItem, avoidRepeat, avoidItemIds, formatItemId, logError, inputItem).FirstOrDefault<ItemQueryResult>();
			return ((itemQueryResult != null) ? itemQueryResult.Item : null) as Item;
		}

		// Token: 0x0600342C RID: 13356 RVA: 0x0029B634 File Offset: 0x00299834
		public static ISalable ApplyItemFields(ISalable item, ISpawnItemData data, ItemQueryContext context, Item inputItem = null)
		{
			return ItemQueryResolver.ApplyItemFields(item, data.MinStack, data.MaxStack, data.ToolUpgradeLevel, data.ObjectInternalName, data.ObjectDisplayName, data.ObjectColor, data.Quality, data.IsRecipe, data.StackModifiers, data.StackModifierMode, data.QualityModifiers, data.QualityModifierMode, data.ModData, context, inputItem);
		}

		// Token: 0x0600342D RID: 13357 RVA: 0x0029B698 File Offset: 0x00299898
		public static ISalable ApplyItemFields(ISalable item, int minStackSize, int maxStackSize, int toolUpgradeLevel, string objectInternalName, string objectDisplayName, string objectColor, int quality, bool isRecipe, List<QuantityModifier> stackSizeModifiers, QuantityModifier.QuantityModifierMode stackSizeModifierMode, List<QuantityModifier> qualityModifiers, QuantityModifier.QuantityModifierMode qualityModifierMode, Dictionary<string, string> modData, ItemQueryContext context, Item inputItem = null)
		{
			if (item == null)
			{
				return null;
			}
			Ring ring = item as Ring;
			if (ring != null && isRecipe)
			{
				item = new Object(ring.ItemId, ring.Stack, true, -1, 0);
			}
			int stackSize = 1;
			if (!isRecipe)
			{
				if (minStackSize == -1 && maxStackSize == -1)
				{
					stackSize = item.Stack;
				}
				else if (maxStackSize > 1)
				{
					minStackSize = Math.Max(minStackSize, 1);
					maxStackSize = Math.Max(maxStackSize, minStackSize);
					stackSize = (((context != null) ? context.Random : null) ?? Game1.random).Next(minStackSize, maxStackSize + 1);
				}
				else if (minStackSize > 1)
				{
					stackSize = minStackSize;
				}
				stackSize = (int)Utility.ApplyQuantityModifiers((float)stackSize, stackSizeModifiers, stackSizeModifierMode, (context != null) ? context.Location : null, (context != null) ? context.Player : null, item as Item, inputItem, (context != null) ? context.Random : null);
			}
			quality = ((quality >= 0) ? quality : item.Quality);
			quality = (int)Utility.ApplyQuantityModifiers((float)quality, qualityModifiers, qualityModifierMode, (context != null) ? context.Location : null, (context != null) ? context.Player : null, item as Item, inputItem, (context != null) ? context.Random : null);
			if (isRecipe)
			{
				item.IsRecipe = true;
			}
			if (stackSize > -1 && stackSize != item.Stack)
			{
				item.Stack = stackSize;
				item.FixStackSize();
			}
			if (quality >= 0 && quality != item.Quality)
			{
				item.Quality = quality;
				item.FixQuality();
			}
			if (modData != null && modData.Count > 0)
			{
				Item item2 = item as Item;
				if (item2 != null)
				{
					item2.modData.CopyFrom(modData);
				}
			}
			Object obj = item as Object;
			if (obj == null)
			{
				Tool tool = item as Tool;
				if (tool != null)
				{
					if (toolUpgradeLevel > -1 && toolUpgradeLevel != tool.UpgradeLevel)
					{
						tool.UpgradeLevel = toolUpgradeLevel;
					}
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(objectInternalName))
				{
					obj.Name = objectInternalName;
				}
				if (!string.IsNullOrWhiteSpace(objectDisplayName))
				{
					obj.displayNameFormat = objectDisplayName;
				}
				if (!string.IsNullOrWhiteSpace(objectColor) && item.HasTypeObject())
				{
					Color? color = Utility.StringToColor(objectColor);
					ColoredObject coloredObj;
					if (color != null && ColoredObject.TrySetColor(obj, color.Value, out coloredObj))
					{
						item = coloredObj;
					}
				}
			}
			return item;
		}

		// Token: 0x0600342E RID: 13358 RVA: 0x0029B8AC File Offset: 0x00299AAC
		public static string FormatLogMessage(string template, ISpawnItemData data, ItemQueryContext context)
		{
			GenericSpawnItemData genericSpawnItemData = data as GenericSpawnItemData;
			string entryId = (genericSpawnItemData != null) ? genericSpawnItemData.Id : null;
			string sourceLabel;
			if (context != null && context.SourcePhrase != null)
			{
				sourceLabel = ((entryId != null) ? (context.SourcePhrase + " > entry '" + entryId + "'") : context.SourcePhrase);
			}
			else if (entryId != null)
			{
				sourceLabel = "entry '" + entryId + "'";
			}
			else
			{
				sourceLabel = "unknown context";
			}
			return string.Format(template, sourceLabel);
		}

		// Token: 0x0600342F RID: 13359 RVA: 0x0029B91E File Offset: 0x00299B1E
		private static void LogNothing(string query, string error)
		{
		}

		// Token: 0x02000687 RID: 1671
		public static class Helpers
		{
			// Token: 0x060045B0 RID: 17840 RVA: 0x00320164 File Offset: 0x0031E364
			public static string[] SplitArguments(string arguments)
			{
				if (arguments.Length <= 0)
				{
					return LegacyShims.EmptyArray<string>();
				}
				return ArgUtility.SplitBySpace(arguments);
			}

			// Token: 0x060045B1 RID: 17841 RVA: 0x0032017B File Offset: 0x0031E37B
			public static ItemQueryResult[] ErrorResult(string key, string arguments, Action<string, string> logError, string message)
			{
				if (logError != null)
				{
					logError((key + " " + arguments).Trim(), message);
				}
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}

			// Token: 0x060045B2 RID: 17842 RVA: 0x003201A0 File Offset: 0x0031E3A0
			public static bool ExcludeFromRandomSale(ParsedItemData data)
			{
				if (data.ExcludeFromRandomSale)
				{
					return true;
				}
				string itemTypeId = data.GetItemTypeId();
				if (!(itemTypeId == "(WP)"))
				{
					if (itemTypeId == "(FL)")
					{
						if (Utility.isFlooringOffLimitsForSale(data.ItemId))
						{
							return true;
						}
					}
				}
				else if (Utility.isWallpaperOffLimitsForSale(data.ItemId))
				{
					return true;
				}
				return false;
			}
		}

		// Token: 0x02000688 RID: 1672
		public static class DefaultResolvers
		{
			// Token: 0x060045B3 RID: 17843 RVA: 0x003201F9 File Offset: 0x0031E3F9
			public static IEnumerable<ItemQueryResult> ALL_ITEMS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				string onlyTypeId = null;
				bool isRandomSale = false;
				bool requirePrice = false;
				string[] args = ItemQueryResolver.Helpers.SplitArguments(arguments);
				int flagsIndex = 0;
				if (ArgUtility.HasIndex<string>(args, 0) && !args[0].StartsWith('@'))
				{
					onlyTypeId = args[0];
					flagsIndex++;
				}
				for (int i = flagsIndex; i < args.Length; i++)
				{
					string arg = args[i];
					if (arg.EqualsIgnoreCase("@isRandomSale"))
					{
						isRandomSale = true;
					}
					else if (arg.EqualsIgnoreCase("@requirePrice"))
					{
						requirePrice = true;
					}
					else
					{
						if (arg.StartsWith('@'))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler.AppendLiteral(" has unknown option flag '");
							defaultInterpolatedStringHandler.AppendFormatted(arg);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
							yield break;
						}
						if (onlyTypeId != null && onlyTypeId != arg)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler.AppendLiteral(" must be an option flag starting with '@'");
							ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
							yield break;
						}
						onlyTypeId = arg;
					}
				}
				foreach (IItemDataDefinition itemDataDefinition in ItemRegistry.ItemTypes)
				{
					string typeId = itemDataDefinition.Identifier;
					if (onlyTypeId == null || !(typeId != onlyTypeId))
					{
						if (typeId == "(F)")
						{
							List<Furniture> furniture = new List<Furniture>();
							foreach (ParsedItemData data in itemDataDefinition.GetAllData())
							{
								if (!isRandomSale || !ItemQueryResolver.Helpers.ExcludeFromRandomSale(data))
								{
									Furniture item = ItemRegistry.Create<Furniture>(data.QualifiedItemId, 1, 0, false);
									if (!requirePrice || item.salePrice(true) > 0)
									{
										furniture.Add(item);
									}
								}
							}
							furniture.Sort(new Comparison<Furniture>(Utility.SortAllFurnitures));
							foreach (Furniture item2 in furniture)
							{
								yield return new ItemQueryResult(item2);
							}
							List<Furniture>.Enumerator enumerator3 = default(List<Furniture>.Enumerator);
						}
						else
						{
							foreach (ParsedItemData data2 in itemDataDefinition.GetAllData())
							{
								if (!isRandomSale || !ItemQueryResolver.Helpers.ExcludeFromRandomSale(data2))
								{
									Item item3 = ItemRegistry.Create(data2.QualifiedItemId, 1, 0, false);
									if (!requirePrice || item3.salePrice(true) > 0)
									{
										yield return new ItemQueryResult(item3);
									}
								}
							}
							IEnumerator<ParsedItemData> enumerator4 = null;
						}
					}
				}
				List<IItemDataDefinition>.Enumerator enumerator = default(List<IItemDataDefinition>.Enumerator);
				yield break;
				yield break;
			}

			// Token: 0x060045B4 RID: 17844 RVA: 0x00320218 File Offset: 0x0031E418
			public static IEnumerable<ItemQueryResult> DISH_OF_THE_DAY(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				if (Game1.dishOfTheDay == null)
				{
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
				return new ItemQueryResult[]
				{
					new ItemQueryResult(Game1.dishOfTheDay.getOne())
					{
						OverrideShopAvailableStock = new int?(Game1.dishOfTheDay.Stack),
						SyncStacksWith = Game1.dishOfTheDay
					}
				};
			}

			// Token: 0x060045B5 RID: 17845 RVA: 0x0032026C File Offset: 0x0031E46C
			public static IEnumerable<ItemQueryResult> FLAVORED_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				int quality = 0;
				bool isWildHoney = false;
				string[] splitArgs = ItemQueryResolver.Helpers.SplitArguments(arguments);
				Object.PreserveType type;
				if (!Utility.TryParseEnum<Object.PreserveType>(splitArgs[0], out type))
				{
					return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "invalid flavored item type (must be one of " + string.Join(", ", Enum.GetNames(typeof(Object.PreserveType))) + ")");
				}
				string ingredientId = ArgUtility.Get(splitArgs, 1, null, true);
				if (type == Object.PreserveType.Honey && ingredientId == "-1")
				{
					isWildHoney = true;
					ingredientId = null;
				}
				else
				{
					ingredientId = ItemRegistry.QualifyItemId(ingredientId);
					if (ingredientId == null)
					{
						return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "must specify a valid flavor ingredient ID");
					}
				}
				string ingredientPreservedId = ArgUtility.Get(splitArgs, 2, null, true);
				if (ingredientPreservedId == "0")
				{
					ingredientPreservedId = null;
				}
				string text;
				ArgUtility.TryGetOptionalInt(splitArgs, 2, out quality, out text, 0, "quality");
				ObjectDataDefinition objectData = ItemRegistry.GetObjectTypeDefinition();
				Object ingredient = null;
				if (!isWildHoney)
				{
					try
					{
						ingredient = ((type == Object.PreserveType.AgedRoe && ingredientId == "(O)812" && ingredientPreservedId != null) ? objectData.CreateFlavoredItem(Object.PreserveType.Roe, ItemRegistry.Create<Object>(ingredientPreservedId, 1, 0, false)) : (ItemRegistry.Create(ingredientId, 1, 0, false) as Object));
					}
					catch (Exception ex)
					{
						return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, ex.Message);
					}
					if (ingredient != null)
					{
						ingredient.Quality = quality;
					}
				}
				Object flavoredItem = objectData.CreateFlavoredItem(type, ingredient);
				if (flavoredItem == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler.AppendLiteral("unsupported flavor type '");
					defaultInterpolatedStringHandler.AppendFormatted<Object.PreserveType>(type);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return new ItemQueryResult[]
				{
					new ItemQueryResult(flavoredItem)
				};
			}

			// Token: 0x060045B6 RID: 17846 RVA: 0x00320408 File Offset: 0x0031E608
			public static IEnumerable<ItemQueryResult> ITEMS_LOST_ON_DEATH(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				List<ItemQueryResult> items = new List<ItemQueryResult>();
				foreach (Item item in Game1.player.itemsLostLastDeath)
				{
					if (item != null)
					{
						item.isLostItem = true;
						items.Add(new ItemQueryResult(item)
						{
							OverrideStackSize = new int?(item.Stack),
							OverrideBasePrice = new int?((Game1.player.stats.Get("Book_Marlon") > 0U) ? ((int)((float)Utility.getSellToStorePriceOfItem(item, true) * 0.5f)) : Utility.getSellToStorePriceOfItem(item, true))
						});
					}
				}
				return items;
			}

			// Token: 0x060045B7 RID: 17847 RVA: 0x003204C0 File Offset: 0x0031E6C0
			public static IEnumerable<ItemQueryResult> ITEMS_SOLD_BY_PLAYER(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				if (string.IsNullOrWhiteSpace(arguments))
				{
					ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "must specify a location ID");
					yield break;
				}
				GameLocation rawShop = Game1.getLocationFromName(arguments);
				if (rawShop == null)
				{
					ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "the specified location ID didn't match any location");
					yield break;
				}
				ShopLocation shopLocation = rawShop as ShopLocation;
				if (shopLocation == null)
				{
					ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "the specified location ID matched a location which isn't a ShopLocation instance");
					yield break;
				}
				foreach (Item i in shopLocation.itemsFromPlayerToSell)
				{
					if (i.Stack > 0)
					{
						Object obj = i as Object;
						int price = (obj != null) ? obj.sellToStorePrice(-1L) : i.salePrice(false);
						yield return new ItemQueryResult(i.getOne())
						{
							OverrideBasePrice = new int?(price),
							OverrideShopAvailableStock = new int?(i.Stack),
							SyncStacksWith = i
						};
					}
				}
				NetList<Item, NetRef<Item>>.Enumerator enumerator = default(NetList<Item, NetRef<Item>>.Enumerator);
				yield break;
				yield break;
			}

			// Token: 0x060045B8 RID: 17848 RVA: 0x003204E0 File Offset: 0x0031E6E0
			public static IEnumerable<ItemQueryResult> LOCATION_FISH(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				string[] splitArgs = ItemQueryResolver.Helpers.SplitArguments(arguments);
				if (splitArgs.Length != 4)
				{
					return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "expected four arguments in the form <location name> <bobber x> <bobber y> <depth>");
				}
				string locationName = splitArgs[0];
				string rawX = splitArgs[1];
				string rawY = splitArgs[2];
				string rawDepth = splitArgs[3];
				int x;
				int y;
				if (!int.TryParse(rawX, out x) || !int.TryParse(rawY, out y))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
					defaultInterpolatedStringHandler.AppendLiteral("can't parse '");
					defaultInterpolatedStringHandler.AppendFormatted(rawX);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(rawY);
					defaultInterpolatedStringHandler.AppendLiteral("' as numeric 'x y' values");
					return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
				}
				int depth;
				if (!int.TryParse(rawDepth, out depth))
				{
					return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "can't parse '" + rawDepth + "' as a numeric depth value");
				}
				Item fish = GameLocation.GetFishFromLocationData(locationName, new Vector2((float)x, (float)y), depth, (context != null) ? context.Player : null, false, true, null);
				if (fish == null)
				{
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
				return new ItemQueryResult[]
				{
					new ItemQueryResult(fish)
				};
			}

			// Token: 0x060045B9 RID: 17849 RVA: 0x003205E4 File Offset: 0x0031E7E4
			public static IEnumerable<ItemQueryResult> LOST_BOOK_OR_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				if (Game1.netWorldState.Value.LostBooksFound < 21)
				{
					return new ItemQueryResult[]
					{
						new ItemQueryResult(ItemRegistry.Create("(O)102", 1, 0, false))
					};
				}
				if (string.IsNullOrWhiteSpace(arguments))
				{
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
				return ItemQueryResolver.TryResolve(arguments, new ItemQueryContext(context, "query 'LOST_BOOK_OR_ITEM'"), ItemQuerySearchMode.All, null, null, false, null, null);
			}

			// Token: 0x060045BA RID: 17850 RVA: 0x00320650 File Offset: 0x0031E850
			public static IEnumerable<ItemQueryResult> LOST_UNIQUE_ITEMS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				List<ItemQueryResult> items = new List<ItemQueryResult>();
				foreach (Item item in Woods.GetLostItemsShopInventory())
				{
					if (item != null && item.Stack > 0)
					{
						items.Add(new ItemQueryResult(item)
						{
							OverrideStackSize = new int?(item.Stack),
							SyncStacksWith = item
						});
					}
				}
				return items;
			}

			// Token: 0x060045BB RID: 17851 RVA: 0x003206CC File Offset: 0x0031E8CC
			public static IEnumerable<ItemQueryResult> MONSTER_SLAYER_REWARDS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				KeyValuePair<string, MonsterSlayerQuestData>[] monsterSlayerQuestData = (from p in DataLoader.MonsterSlayerQuests(Game1.content)
				where AdventureGuild.HasCollectedReward(context.Player, p.Key)
				select p).ToArray<KeyValuePair<string, MonsterSlayerQuestData>>();
				HashSet<string> questIds = new HashSet<string>();
				foreach (KeyValuePair<string, MonsterSlayerQuestData> pair in monsterSlayerQuestData)
				{
					string id = pair.Key;
					MonsterSlayerQuestData questData = pair.Value;
					if (!questIds.Contains(id))
					{
						if (questData.RewardItemId != null && questData.RewardItemPrice != -1)
						{
							if (!ItemContextTagManager.HasBaseTag(questData.RewardItemId, "item_type_ring"))
							{
								goto IL_14C;
							}
							Item i = ItemRegistry.Create(questData.RewardItemId, 1, 0, false);
							yield return new ItemQueryResult(i)
							{
								OverrideBasePrice = new int?(questData.RewardItemPrice),
								OverrideShopAvailableStock = new int?(int.MaxValue)
							};
							questIds.Add(id);
						}
						id = null;
					}
					IL_14C:;
				}
				KeyValuePair<string, MonsterSlayerQuestData>[] array = null;
				foreach (KeyValuePair<string, MonsterSlayerQuestData> pair2 in monsterSlayerQuestData)
				{
					string id = pair2.Key;
					MonsterSlayerQuestData questData2 = pair2.Value;
					if (!questIds.Contains(id))
					{
						if (questData2.RewardItemId != null && questData2.RewardItemPrice != -1)
						{
							ItemMetadata itemMetadata = ItemRegistry.ResolveMetadata(questData2.RewardItemId);
							string a;
							if (itemMetadata == null)
							{
								a = null;
							}
							else
							{
								IItemDataDefinition typeDefinition = itemMetadata.GetTypeDefinition();
								a = ((typeDefinition != null) ? typeDefinition.Identifier : null);
							}
							if (a != "(H)")
							{
								goto IL_27D;
							}
							Item j = ItemRegistry.Create(questData2.RewardItemId, 1, 0, false);
							yield return new ItemQueryResult(j)
							{
								OverrideBasePrice = new int?(questData2.RewardItemPrice),
								OverrideShopAvailableStock = new int?(int.MaxValue)
							};
							questIds.Add(id);
						}
						id = null;
					}
					IL_27D:;
				}
				array = null;
				foreach (KeyValuePair<string, MonsterSlayerQuestData> pair3 in monsterSlayerQuestData)
				{
					string id = pair3.Key;
					MonsterSlayerQuestData questData3 = pair3.Value;
					if (!questIds.Contains(id))
					{
						if (questData3.RewardItemId != null && questData3.RewardItemPrice != -1)
						{
							ItemMetadata itemMetadata2 = ItemRegistry.ResolveMetadata(questData3.RewardItemId);
							string a2;
							if (itemMetadata2 == null)
							{
								a2 = null;
							}
							else
							{
								IItemDataDefinition typeDefinition2 = itemMetadata2.GetTypeDefinition();
								a2 = ((typeDefinition2 != null) ? typeDefinition2.Identifier : null);
							}
							if (a2 != "(W)")
							{
								goto IL_3AE;
							}
							Item k = ItemRegistry.Create(questData3.RewardItemId, 1, 0, false);
							yield return new ItemQueryResult(k)
							{
								OverrideBasePrice = new int?(questData3.RewardItemPrice),
								OverrideShopAvailableStock = new int?(int.MaxValue)
							};
							questIds.Add(id);
						}
						id = null;
					}
					IL_3AE:;
				}
				array = null;
				foreach (KeyValuePair<string, MonsterSlayerQuestData> pair4 in monsterSlayerQuestData)
				{
					string id = pair4.Key;
					MonsterSlayerQuestData questData4 = pair4.Value;
					if (!questIds.Contains(id))
					{
						if (questData4.RewardItemId != null && questData4.RewardItemPrice != -1)
						{
							Item l = ItemRegistry.Create(questData4.RewardItemId, 1, 0, false);
							yield return new ItemQueryResult(l)
							{
								OverrideBasePrice = new int?(questData4.RewardItemPrice),
								OverrideShopAvailableStock = new int?(int.MaxValue)
							};
							questIds.Add(id);
						}
						id = null;
					}
				}
				array = null;
				yield break;
			}

			// Token: 0x060045BC RID: 17852 RVA: 0x003206DC File Offset: 0x0031E8DC
			public static IEnumerable<ItemQueryResult> MOVIE_CONCESSIONS_FOR_GUEST(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				string npcName = ArgUtility.SplitBySpaceAndGet(arguments, 0, null);
				List<MovieConcession> concessions = (npcName != null) ? MovieTheater.GetConcessionsForGuest(npcName) : MovieTheater.GetConcessionsForGuest();
				foreach (MovieConcession concession in concessions)
				{
					yield return new ItemQueryResult(concession);
				}
				List<MovieConcession>.Enumerator enumerator = default(List<MovieConcession>.Enumerator);
				yield break;
				yield break;
			}

			// Token: 0x060045BD RID: 17853 RVA: 0x003206EC File Offset: 0x0031E8EC
			public static IEnumerable<ItemQueryResult> RANDOM_ARTIFACT_FOR_DIG_SPOT(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				Random random = context.Random ?? Game1.random;
				Farmer player = context.Player;
				string locationName = context.Location.Name;
				Hoe hoe = player.CurrentTool as Hoe;
				int chanceMultiplier = (hoe != null && hoe.hasEnchantmentOfType<ArchaeologistEnchantment>()) ? 2 : 1;
				foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					if (!(data.ObjectType != "Arch"))
					{
						ObjectData objectData = data.RawData as ObjectData;
						Dictionary<string, float> dropChances = (objectData != null) ? objectData.ArtifactSpotChances : null;
						float chance;
						if (dropChances != null && dropChances.TryGetValue(locationName, out chance) && random.NextBool((float)chanceMultiplier * chance))
						{
							return new ItemQueryResult[]
							{
								new ItemQueryResult(ItemRegistry.Create(data.QualifiedItemId, 1, 0, false))
							};
						}
					}
				}
				return LegacyShims.EmptyArray<ItemQueryResult>();
			}

			// Token: 0x060045BE RID: 17854 RVA: 0x003207EC File Offset: 0x0031E9EC
			public static IEnumerable<ItemQueryResult> RANDOM_BASE_SEASON_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				GameLocation location = context.Location;
				Random random = context.Random ?? Utility.CreateDaySaveRandom((double)Game1.hash.GetDeterministicHashCode(key + arguments), 0.0, 0.0);
				Item item = ItemRegistry.Create(Utility.getRandomItemFromSeason(location.GetSeason(), false, random), 1, 0, false);
				return new ItemQueryResult[]
				{
					new ItemQueryResult(item)
				};
			}

			// Token: 0x060045BF RID: 17855 RVA: 0x00320857 File Offset: 0x0031EA57
			public static IEnumerable<ItemQueryResult> RANDOM_ITEMS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				int minId = int.MinValue;
				int maxId = int.MaxValue;
				bool isRandomSale = false;
				bool requirePrice = false;
				string[] args = ItemQueryResolver.Helpers.SplitArguments(arguments);
				string typeId;
				string error;
				if (!ArgUtility.TryGet(args, 0, out typeId, out error, false, "typeId"))
				{
					ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
					yield break;
				}
				int flagsIndex = 1;
				int parsedId;
				if (ArgUtility.HasIndex<string>(args, 1) && int.TryParse(args[1], out parsedId))
				{
					minId = parsedId;
					flagsIndex++;
					if (ArgUtility.HasIndex<string>(args, 2) && int.TryParse(args[2], out parsedId))
					{
						maxId = parsedId;
						flagsIndex++;
					}
				}
				for (int i = flagsIndex; i < args.Length; i++)
				{
					string arg = args[i];
					if (arg.EqualsIgnoreCase("@isRandomSale"))
					{
						isRandomSale = true;
					}
					else if (arg.EqualsIgnoreCase("@requirePrice"))
					{
						requirePrice = true;
					}
					else
					{
						if (arg.StartsWith('@'))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 2);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler.AppendLiteral(" has unknown flag argument '");
							defaultInterpolatedStringHandler.AppendFormatted(arg);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
							yield break;
						}
						if (i == 1 || i == 2)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 2);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler.AppendLiteral(" must a numeric ");
							defaultInterpolatedStringHandler.AppendFormatted((i == 1) ? "min" : "max");
							defaultInterpolatedStringHandler.AppendLiteral(" ID, or an option flag starting with '@'.");
							ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 1);
							defaultInterpolatedStringHandler.AppendLiteral("index ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler.AppendLiteral(" must be an option flag starting with '@'.");
							ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, defaultInterpolatedStringHandler.ToStringAndClear());
						}
						yield break;
					}
				}
				IItemDataDefinition typeDef = ItemRegistry.GetTypeDefinition(typeId);
				if (typeDef == null)
				{
					ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "there's no item data definition with ID '" + typeId + "'");
					yield break;
				}
				bool hasRange = minId != int.MinValue || maxId != int.MaxValue;
				Random random = context.Random ?? Game1.random;
				IEnumerable<ParsedItemData> allData = typeDef.GetAllData();
				Func<ParsedItemData, int> <>9__0;
				Func<ParsedItemData, int> keySelector;
				if ((keySelector = <>9__0) == null)
				{
					keySelector = (<>9__0 = ((ParsedItemData p) => random.Next()));
				}
				foreach (ParsedItemData data in allData.OrderBy(keySelector))
				{
					int index;
					if ((!isRandomSale || !ItemQueryResolver.Helpers.ExcludeFromRandomSale(data)) && (!hasRange || (int.TryParse(data.ItemId, out index) && index >= minId && index <= maxId)))
					{
						Item item = ItemRegistry.Create(data.QualifiedItemId, 1, 0, false);
						if (!requirePrice || item.salePrice(true) > 0)
						{
							yield return new ItemQueryResult(item);
						}
					}
				}
				IEnumerator<ParsedItemData> enumerator = null;
				yield break;
				yield break;
			}

			// Token: 0x060045C0 RID: 17856 RVA: 0x00320880 File Offset: 0x0031EA80
			public static IEnumerable<ItemQueryResult> SECRET_NOTE_OR_ITEM(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				GameLocation location = context.Location;
				Farmer player = context.Player;
				if (location != null && location.HasUnlockedAreaSecretNotes(player))
				{
					Object secretNote = location.tryToCreateUnseenSecretNote(player);
					if (secretNote != null)
					{
						return new ItemQueryResult[]
						{
							new ItemQueryResult(secretNote)
						};
					}
				}
				if (string.IsNullOrWhiteSpace(arguments))
				{
					return LegacyShims.EmptyArray<ItemQueryResult>();
				}
				return ItemQueryResolver.TryResolve(arguments, new ItemQueryContext(context, "query 'SECRET_NOTE_OR_ITEM'"), ItemQuerySearchMode.All, null, null, false, null, null);
			}

			// Token: 0x060045C1 RID: 17857 RVA: 0x003208F0 File Offset: 0x0031EAF0
			public static IEnumerable<ItemQueryResult> SHOP_TOWN_KEY(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				ISalable townKey = new PurchaseableKeyItem(Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"), Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown_desc"), 912, delegate(Farmer farmer)
				{
					farmer.HasTownKey = true;
				});
				return new ItemQueryResult[]
				{
					new ItemQueryResult(townKey)
					{
						OverrideShopAvailableStock = new int?(1)
					}
				};
			}

			// Token: 0x060045C2 RID: 17858 RVA: 0x00320960 File Offset: 0x0031EB60
			public static IEnumerable<ItemQueryResult> TOOL_UPGRADES(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				string onlyItemId = null;
				if (!string.IsNullOrWhiteSpace(arguments))
				{
					ParsedItemData data = ItemRegistry.GetDataOrErrorItem(arguments);
					if (data.HasTypeId("(T)"))
					{
						return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "can't filter for ID '" + arguments + "' because that isn't a tool item ID");
					}
					onlyItemId = data.ItemId;
				}
				List<ItemQueryResult> stock = new List<ItemQueryResult>();
				foreach (KeyValuePair<string, ToolData> pair in Game1.toolData)
				{
					string itemId = pair.Key;
					ToolData entry = pair.Value;
					if (onlyItemId == null || !(itemId != onlyItemId))
					{
						ToolUpgradeData upgrade = ShopBuilder.GetToolUpgradeData(entry, Game1.player);
						if (upgrade != null)
						{
							Item tool = ItemRegistry.Create("(T)" + itemId, 1, 0, false);
							int price = (upgrade.Price > -1) ? upgrade.Price : Math.Max(0, tool.salePrice(false));
							stock.Add(new ItemQueryResult(tool)
							{
								OverrideBasePrice = new int?(price),
								OverrideShopAvailableStock = new int?(1),
								OverrideTradeItemId = upgrade.TradeItemId,
								OverrideTradeItemAmount = new int?(upgrade.TradeItemAmount)
							});
						}
					}
				}
				return stock;
			}

			// Token: 0x060045C3 RID: 17859 RVA: 0x00320AA4 File Offset: 0x0031ECA4
			public static IEnumerable<ItemQueryResult> PET_ADOPTION(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
			{
				List<ItemQueryResult> stock = new List<ItemQueryResult>();
				foreach (KeyValuePair<string, PetData> pair in Game1.petData)
				{
					foreach (PetBreed breed in pair.Value.Breeds)
					{
						if (breed.CanBeAdoptedFromMarnie)
						{
							stock.Add(new ItemQueryResult(new PetLicense
							{
								Name = pair.Key + "|" + breed.Id
							})
							{
								OverrideBasePrice = new int?(breed.AdoptionPrice)
							});
						}
					}
				}
				return stock;
			}
		}
	}
}
