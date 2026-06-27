using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley
{
	// Token: 0x020000BB RID: 187
	public static class ItemRegistry
	{
		// Token: 0x06000D3E RID: 3390 RVA: 0x00091A50 File Offset: 0x0008FC50
		internal static void RegisterItemTypes()
		{
			IItemDataDefinition[] array = new IItemDataDefinition[]
			{
				new ObjectDataDefinition(),
				new BigCraftableDataDefinition(),
				new FurnitureDataDefinition(),
				new WeaponDataDefinition(),
				new BootsDataDefinition(),
				new HatDataDefinition(),
				new MannequinDataDefinition(),
				new PantsDataDefinition(),
				new ShirtDataDefinition(),
				new ToolDataDefinition(),
				new TrinketDataDefinition(),
				new WallpaperDataDefinition(),
				new FlooringDataDefinition()
			};
			for (int i = 0; i < array.Length; i++)
			{
				ItemRegistry.AddTypeDefinition(array[i]);
			}
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x00091AE8 File Offset: 0x0008FCE8
		public static void AddTypeDefinition(IItemDataDefinition definition)
		{
			ItemRegistry.<>c__DisplayClass17_0 CS$<>8__locals1;
			CS$<>8__locals1.definition = definition;
			if (CS$<>8__locals1.definition == null)
			{
				throw new ArgumentNullException("definition");
			}
			string identifier = CS$<>8__locals1.definition.Identifier;
			if (string.IsNullOrWhiteSpace(identifier))
			{
				throw ItemRegistry.<AddTypeDefinition>g__GetException|17_0("it has no identifier", ref CS$<>8__locals1);
			}
			if (identifier.Length < 2 || identifier[0] != '(' || identifier[identifier.Length - 1] != ')')
			{
				throw ItemRegistry.<AddTypeDefinition>g__GetException|17_0("its identifier must start with '(' and end with ')'", ref CS$<>8__locals1);
			}
			if (identifier.IndexOf('(', 1) != -1 || identifier.IndexOf(')') != identifier.Length - 1)
			{
				throw ItemRegistry.<AddTypeDefinition>g__GetException|17_0("its identifier can't contain '(' or ')' except as the first and last character respectively", ref CS$<>8__locals1);
			}
			if (ItemRegistry.IdentifierLookup.ContainsKey(identifier))
			{
				throw ItemRegistry.<AddTypeDefinition>g__GetException|17_0("its identifier is already registered", ref CS$<>8__locals1);
			}
			ItemRegistry.ItemTypes.Add(CS$<>8__locals1.definition);
			ItemRegistry.IdentifierLookup[identifier] = CS$<>8__locals1.definition;
			ItemRegistry.ResetCache();
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x00091BCF File Offset: 0x0008FDCF
		public static IItemDataDefinition GetTypeDefinition(string identifier)
		{
			if (identifier == null)
			{
				return null;
			}
			return ItemRegistry.IdentifierLookup.GetValueOrDefault(identifier);
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x00091BE1 File Offset: 0x0008FDE1
		public static IItemDataDefinition RequireTypeDefinition(string identifier)
		{
			IItemDataDefinition typeDefinition = ItemRegistry.GetTypeDefinition(identifier);
			if (typeDefinition == null)
			{
				throw new KeyNotFoundException("No item type definition found with ID '" + identifier + "'.");
			}
			return typeDefinition;
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x00091C04 File Offset: 0x0008FE04
		public static TItemDataDefinition RequireTypeDefinition<TItemDataDefinition>(string identifier) where TItemDataDefinition : class, IItemDataDefinition
		{
			IItemDataDefinition typeDefinition = ItemRegistry.GetTypeDefinition(identifier);
			if (typeDefinition == null)
			{
				throw new KeyNotFoundException("No item type definition found with ID '" + identifier + "'.");
			}
			IItemDataDefinition definition = typeDefinition;
			TItemDataDefinition titemDataDefinition = definition as TItemDataDefinition;
			if (titemDataDefinition == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 3);
				defaultInterpolatedStringHandler.AppendLiteral("The item type definition for ID '");
				defaultInterpolatedStringHandler.AppendFormatted(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("' implements ");
				defaultInterpolatedStringHandler.AppendFormatted(definition.GetType().FullName);
				defaultInterpolatedStringHandler.AppendLiteral(", but expected ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(TItemDataDefinition).FullName);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				throw new InvalidCastException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return titemDataDefinition;
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x00091CBD File Offset: 0x0008FEBD
		public static ObjectDataDefinition GetObjectTypeDefinition()
		{
			return ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>("(O)");
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x00091CC9 File Offset: 0x0008FEC9
		public static void ResetCache()
		{
			ItemRegistry.CachedItems.Clear();
			ItemContextTagManager.ResetCache();
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00091CDA File Offset: 0x0008FEDA
		public static bool HasItemId(Item item, string itemId)
		{
			if (item == null)
			{
				return string.IsNullOrEmpty(itemId);
			}
			return item.QualifiedItemId == ItemRegistry.QualifyItemId(itemId);
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x00091CF7 File Offset: 0x0008FEF7
		public static bool IsQualifiedItemId(string itemId)
		{
			return itemId != null && itemId.StartsWith('(') && itemId.Contains(')');
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x00091D10 File Offset: 0x0008FF10
		public static string QualifyItemId(string itemId)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			if (metadata == null)
			{
				return null;
			}
			if (metadata.QualifiedItemId != null)
			{
				return metadata.QualifiedItemId;
			}
			metadata.GetTypeDefinition();
			if (metadata.QualifiedItemId != null)
			{
				return metadata.QualifiedItemId;
			}
			if (!itemId.StartsWith('(') || !itemId.Contains(')'))
			{
				return null;
			}
			return itemId;
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x00091D68 File Offset: 0x0008FF68
		public static string ManuallyQualifyItemId(string itemId, string typeDefinitionId, bool overrideIfQualified = false)
		{
			if (string.IsNullOrWhiteSpace(itemId))
			{
				return itemId;
			}
			if (itemId.StartsWith('('))
			{
				if (!overrideIfQualified)
				{
					return itemId;
				}
				int splitIndex = itemId.IndexOf(')') + 1;
				if (splitIndex > 0)
				{
					return typeDefinitionId + itemId.Substring(splitIndex).Trim();
				}
			}
			return typeDefinitionId + itemId;
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x00091DB8 File Offset: 0x0008FFB8
		public static ItemMetadata GetMetadata(string itemId)
		{
			if (string.IsNullOrWhiteSpace(itemId))
			{
				return null;
			}
			if (ItemRegistry.CachedItems.Count == 0)
			{
				ItemRegistry.RebuildCache();
			}
			ItemMetadata metadata;
			if (!ItemRegistry.CachedItems.TryGetValue(itemId, out metadata))
			{
				if (itemId[0] == '(')
				{
					int splitIndex = itemId.IndexOf(')') + 1;
					if (splitIndex >= 0)
					{
						metadata = new ItemMetadata(itemId, itemId.Substring(splitIndex), itemId.Substring(0, splitIndex));
					}
				}
				else
				{
					metadata = new ItemMetadata(null, itemId, null);
				}
				ItemRegistry.CachedItems[itemId] = metadata;
			}
			return metadata;
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x00091E37 File Offset: 0x00090037
		public static bool Exists(string itemId)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			return metadata != null && metadata.Exists();
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x00091E4C File Offset: 0x0009004C
		public static ItemMetadata ResolveMetadata(string itemId)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			if (metadata == null || !metadata.Exists())
			{
				return null;
			}
			return metadata;
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x00091E70 File Offset: 0x00090070
		internal static IItemDataDefinition GetTypeDefinitionFor(ItemMetadata metadata)
		{
			if (metadata.TypeIdentifier != null)
			{
				return ItemRegistry.GetTypeDefinition(metadata.TypeIdentifier);
			}
			foreach (IItemDataDefinition type in ItemRegistry.ItemTypes)
			{
				if (type.Exists(metadata.LocalItemId))
				{
					return type;
				}
			}
			return null;
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x00091EE4 File Offset: 0x000900E4
		public static ParsedItemData GetData(string itemId)
		{
			ItemMetadata itemMetadata = ItemRegistry.ResolveMetadata(itemId);
			if (itemMetadata == null)
			{
				return null;
			}
			return itemMetadata.GetParsedData();
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x00091EF8 File Offset: 0x000900F8
		public static ParsedItemData GetDataOrErrorItem(string itemId)
		{
			ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
			IItemDataDefinition itemType = (metadata != null) ? metadata.GetTypeDefinition() : null;
			if (itemType != null)
			{
				ParsedItemData data = metadata.GetParsedData();
				if (data != null)
				{
					return data;
				}
			}
			ParsedItemData result;
			if ((result = ((itemType != null) ? itemType.GetErrorData(((metadata != null) ? metadata.LocalItemId : null) ?? itemId) : null)) == null)
			{
				result = ItemRegistry.RequireTypeDefinition("(O)").GetErrorData(((metadata != null) ? metadata.LocalItemId : null) ?? itemId);
			}
			return result;
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x00091F6C File Offset: 0x0009016C
		public static Item Create(string itemId, int amount = 1, int quality = 0, bool allowNull = false)
		{
			ParsedItemData data = allowNull ? ItemRegistry.GetData(itemId) : ItemRegistry.GetDataOrErrorItem(itemId);
			if (data == null || data.IsErrorItem)
			{
				if (allowNull)
				{
					return null;
				}
				if (data == null)
				{
					data = ItemRegistry.RequireTypeDefinition("(O)").GetErrorData(itemId);
				}
			}
			Item item = data.ItemType.CreateItem(data);
			if (amount != 1)
			{
				item.Stack = amount;
				item.FixStackSize();
			}
			if (quality != 0)
			{
				item.Quality = quality;
				item.FixQuality();
			}
			return item;
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x00091FE0 File Offset: 0x000901E0
		public static TItem Create<TItem>(string itemId, int amount = 1, int quality = 0, bool allowNull = false) where TItem : Item
		{
			Item item = ItemRegistry.Create(itemId, amount, quality, allowNull);
			if (item == null)
			{
				return default(TItem);
			}
			TItem castItem = item as TItem;
			if (castItem == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't create item ID '");
				defaultInterpolatedStringHandler.AppendFormatted(itemId);
				defaultInterpolatedStringHandler.AppendLiteral("' as a ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(TItem).Name);
				defaultInterpolatedStringHandler.AppendLiteral(" type because it's a ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(item.GetType());
				defaultInterpolatedStringHandler.AppendLiteral(" instance.");
				throw new InvalidCastException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return castItem;
		}

		// Token: 0x06000D51 RID: 3409 RVA: 0x0009208D File Offset: 0x0009028D
		public static string GetErrorItemName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x0009209E File Offset: 0x0009029E
		public static string GetErrorItemName(string itemId)
		{
			return ItemRegistry.GetErrorItemName() + " (" + itemId + ")";
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x000920B5 File Offset: 0x000902B5
		public static string GetUnnamedItemName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:UnnamedItem");
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x000920C6 File Offset: 0x000902C6
		public static string GetUnnamedItemName(string itemId)
		{
			return ItemRegistry.GetUnnamedItemName() + " (" + itemId + ")";
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x000920E0 File Offset: 0x000902E0
		private static void RebuildCache()
		{
			ItemRegistry.CachedItems.Clear();
			foreach (IItemDataDefinition type in ItemRegistry.ItemTypes)
			{
				string qualifier = type.Identifier;
				foreach (string id in type.GetAllIds())
				{
					string qualifiedId = qualifier + id;
					ItemMetadata parsed = new ItemMetadata(qualifiedId, id, qualifier);
					parsed.SetTypeDefinition(qualifier, type, new bool?(true));
					ItemRegistry.CachedItems[qualifiedId] = parsed;
					ItemRegistry.CachedItems.TryAdd(id, parsed);
				}
			}
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x000921D8 File Offset: 0x000903D8
		[CompilerGenerated]
		internal static InvalidOperationException <AddTypeDefinition>g__GetException|17_0(string reason, ref ItemRegistry.<>c__DisplayClass17_0 A_1)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Can't add item data definition of type '");
			defaultInterpolatedStringHandler.AppendFormatted(A_1.definition.GetType().FullName);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			defaultInterpolatedStringHandler.AppendFormatted((!string.IsNullOrWhiteSpace(A_1.definition.Identifier)) ? (" with identifier '" + A_1.definition.Identifier + "'") : "");
			defaultInterpolatedStringHandler.AppendLiteral(" because ");
			defaultInterpolatedStringHandler.AppendFormatted(reason);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			return new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x040008D9 RID: 2265
		private static readonly Dictionary<string, IItemDataDefinition> IdentifierLookup = new Dictionary<string, IItemDataDefinition>();

		// Token: 0x040008DA RID: 2266
		private static readonly Dictionary<string, ItemMetadata> CachedItems = new Dictionary<string, ItemMetadata>();

		// Token: 0x040008DB RID: 2267
		[NonInstancedStatic]
		public static readonly List<IItemDataDefinition> ItemTypes = new List<IItemDataDefinition>();

		// Token: 0x040008DC RID: 2268
		public const string type_object = "(O)";

		// Token: 0x040008DD RID: 2269
		public const string type_bigCraftable = "(BC)";

		// Token: 0x040008DE RID: 2270
		public const string type_boots = "(B)";

		// Token: 0x040008DF RID: 2271
		public const string type_floorpaper = "(FL)";

		// Token: 0x040008E0 RID: 2272
		public const string type_furniture = "(F)";

		// Token: 0x040008E1 RID: 2273
		public const string type_hat = "(H)";

		// Token: 0x040008E2 RID: 2274
		public const string type_mannequin = "(M)";

		// Token: 0x040008E3 RID: 2275
		public const string type_pants = "(P)";

		// Token: 0x040008E4 RID: 2276
		public const string type_shirt = "(S)";

		// Token: 0x040008E5 RID: 2277
		public const string type_tool = "(T)";

		// Token: 0x040008E6 RID: 2278
		public const string type_trinket = "(TR)";

		// Token: 0x040008E7 RID: 2279
		public const string type_wallpaper = "(WP)";

		// Token: 0x040008E8 RID: 2280
		public const string type_weapon = "(W)";
	}
}
