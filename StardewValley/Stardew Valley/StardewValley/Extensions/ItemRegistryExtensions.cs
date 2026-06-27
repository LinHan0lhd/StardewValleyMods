using System;
using System.Collections.Generic;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Extensions
{
	// Token: 0x0200031E RID: 798
	public static class ItemRegistryExtensions
	{
		// Token: 0x0600346C RID: 13420 RVA: 0x0029CC83 File Offset: 0x0029AE83
		public static IEnumerable<ParsedItemData> GetAllData(this IItemDataDefinition definition)
		{
			foreach (string id in definition.GetAllIds())
			{
				yield return ItemRegistry.GetDataOrErrorItem(definition.Identifier + id);
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}
	}
}
