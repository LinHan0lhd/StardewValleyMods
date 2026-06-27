using System;
using System.Diagnostics.CodeAnalysis;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Extensions
{
	// Token: 0x0200031D RID: 797
	public static class ItemExtensions
	{
		// Token: 0x06003469 RID: 13417 RVA: 0x0029CC55 File Offset: 0x0029AE55
		public static bool HasTypeId([NotNullWhen(true)] this IHaveItemTypeId item, string typeId)
		{
			return ((item != null) ? item.GetItemTypeId() : null) == typeId;
		}

		// Token: 0x0600346A RID: 13418 RVA: 0x0029CC69 File Offset: 0x0029AE69
		public static bool HasTypeObject([NotNullWhen(true)] this IHaveItemTypeId item)
		{
			return item.HasTypeId("(O)");
		}

		// Token: 0x0600346B RID: 13419 RVA: 0x0029CC76 File Offset: 0x0029AE76
		public static bool HasTypeBigCraftable([NotNullWhen(true)] this IHaveItemTypeId item)
		{
			return item.HasTypeId("(BC)");
		}
	}
}
