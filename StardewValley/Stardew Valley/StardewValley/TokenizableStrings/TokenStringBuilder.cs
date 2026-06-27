using System;

namespace StardewValley.TokenizableStrings
{
	// Token: 0x02000138 RID: 312
	public static class TokenStringBuilder
	{
		// Token: 0x060018EA RID: 6378 RVA: 0x00124A4D File Offset: 0x00122C4D
		public static string EscapedText(string value, bool skipIfNotNeeded = true)
		{
			if (!skipIfNotNeeded || (value.IndexOfAny(TokenParser.HeuristicCharactersForEscapableStrings) != -1 && !value.StartsWith("[EscapedText")))
			{
				value = "[EscapedText " + value + "]";
			}
			return value;
		}

		// Token: 0x060018EB RID: 6379 RVA: 0x00124A80 File Offset: 0x00122C80
		public static string AchievementName(int achievementId)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("AchievementName", achievementId.ToString());
		}

		// Token: 0x060018EC RID: 6380 RVA: 0x00124A93 File Offset: 0x00122C93
		public static string ArticleFor(string word)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("ArticleFor", word);
		}

		// Token: 0x060018ED RID: 6381 RVA: 0x00124AA0 File Offset: 0x00122CA0
		public static string CapitalizeFirstLetter(string text)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("CapitalizeFirstLetter", text);
		}

		// Token: 0x060018EE RID: 6382 RVA: 0x00124AAD File Offset: 0x00122CAD
		public static string ItemName(string itemId, string fallbackItemName = null)
		{
			if (fallbackItemName == null)
			{
				return TokenStringBuilder.BuildTokenWithArgumentString("ItemName", itemId);
			}
			return TokenStringBuilder.BuildTokenWithArgumentString("ItemName", itemId, fallbackItemName);
		}

		// Token: 0x060018EF RID: 6383 RVA: 0x00124ACA File Offset: 0x00122CCA
		public static string ItemNameWithFlavor(Object.PreserveType preserveType, string preservedId, string fallbackItemName = null)
		{
			if (fallbackItemName == null)
			{
				return TokenStringBuilder.BuildTokenWithArgumentString("ItemNameWithFlavor", preserveType.ToString(), preservedId);
			}
			return TokenStringBuilder.BuildTokenWithArgumentString("ItemNameWithFlavor", preserveType.ToString(), preservedId, fallbackItemName);
		}

		// Token: 0x060018F0 RID: 6384 RVA: 0x00124B04 File Offset: 0x00122D04
		public static string ItemNameFor(Item item, string fallbackItemName = null)
		{
			Object obj = item as Object;
			if (obj != null)
			{
				if (!string.IsNullOrWhiteSpace(obj.displayNameFormat))
				{
					return obj.displayNameFormat;
				}
				if (obj.preserve.Value != null)
				{
					return TokenStringBuilder.ItemNameWithFlavor(obj.preserve.Value.Value, obj.preservedParentSheetIndex.Value, fallbackItemName);
				}
			}
			return TokenStringBuilder.ItemName((item != null) ? item.QualifiedItemId : null, fallbackItemName);
		}

		// Token: 0x060018F1 RID: 6385 RVA: 0x00124B7B File Offset: 0x00122D7B
		public static string LocalizedText(string translationKey)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("LocalizedText", translationKey);
		}

		// Token: 0x060018F2 RID: 6386 RVA: 0x00124B88 File Offset: 0x00122D88
		public static string MonsterName(string monsterId, string fallbackText = null)
		{
			if (fallbackText == null)
			{
				return TokenStringBuilder.BuildTokenWithArgumentString("MonsterName", monsterId);
			}
			return TokenStringBuilder.BuildTokenWithArgumentString("MonsterName", monsterId, fallbackText);
		}

		// Token: 0x060018F3 RID: 6387 RVA: 0x00124BA5 File Offset: 0x00122DA5
		public static string MovieName(string movieId)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("MovieName", movieId);
		}

		// Token: 0x060018F4 RID: 6388 RVA: 0x00124BB2 File Offset: 0x00122DB2
		public static string NumberWithSeparators(int number)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("NumberWithSeparators", number.ToString());
		}

		// Token: 0x060018F5 RID: 6389 RVA: 0x00124BC5 File Offset: 0x00122DC5
		public static string SpecialOrderName(string orderId)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("SpecialOrderName", orderId);
		}

		// Token: 0x060018F6 RID: 6390 RVA: 0x00124BD2 File Offset: 0x00122DD2
		public static string ToolName(string itemId, int upgradeLevel)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("ToolName", itemId, upgradeLevel.ToString());
		}

		// Token: 0x060018F7 RID: 6391 RVA: 0x00124BE6 File Offset: 0x00122DE6
		public static string BuildTokenWithArgumentString(string tokenName, string argument)
		{
			return string.Concat(new string[]
			{
				"[",
				tokenName,
				" ",
				TokenStringBuilder.EscapedText(argument, true),
				"]"
			});
		}

		// Token: 0x060018F8 RID: 6392 RVA: 0x00124C1C File Offset: 0x00122E1C
		public static string BuildTokenWithArgumentString(string tokenName, string arg1, string arg2)
		{
			return string.Concat(new string[]
			{
				"[",
				tokenName,
				" ",
				TokenStringBuilder.EscapedText(arg1, true),
				" ",
				TokenStringBuilder.EscapedText(arg2, true),
				"]"
			});
		}

		// Token: 0x060018F9 RID: 6393 RVA: 0x00124C6C File Offset: 0x00122E6C
		public static string BuildTokenWithArgumentString(string tokenName, string arg1, string arg2, string arg3)
		{
			return string.Concat(new string[]
			{
				"[",
				tokenName,
				" ",
				TokenStringBuilder.EscapedText(arg1, true),
				" ",
				TokenStringBuilder.EscapedText(arg2, true),
				" ",
				TokenStringBuilder.EscapedText(arg3, true),
				"]"
			});
		}
	}
}
