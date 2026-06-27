using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Content;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Movies;
using StardewValley.GameData.SpecialOrders;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.SpecialOrders;

namespace StardewValley.TokenizableStrings
{
	// Token: 0x02000136 RID: 310
	public class TokenParser
	{
		// Token: 0x060018DC RID: 6364 RVA: 0x0012470C File Offset: 0x0012290C
		static TokenParser()
		{
			foreach (MethodInfo method in typeof(TokenParser.DefaultResolvers).GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				TokenParserDelegate queryDelegate = (TokenParserDelegate)Delegate.CreateDelegate(typeof(TokenParserDelegate), method);
				TokenParser.Parsers[method.Name] = queryDelegate;
			}
		}

		// Token: 0x060018DD RID: 6365 RVA: 0x0012479C File Offset: 0x0012299C
		public static void RegisterParser(string tokenKey, TokenParserDelegate parser)
		{
			if (string.IsNullOrWhiteSpace(tokenKey))
			{
				throw new ArgumentException("The token key can't be empty.", "tokenKey");
			}
			if (parser == null)
			{
				throw new ArgumentException("The parser callback for token key '" + tokenKey + "' can't be null.", "parser");
			}
			tokenKey = tokenKey.Trim();
			if (!TokenParser.Parsers.TryAdd(tokenKey, parser))
			{
				throw new ArgumentException("Can't add token parser for key '" + tokenKey + "' because one is already registered for it.");
			}
		}

		// Token: 0x060018DE RID: 6366 RVA: 0x0012480B File Offset: 0x00122A0B
		public static string EscapeSpaces(string text)
		{
			if (text.Length <= 0)
			{
				return TokenParser.EscapedEmptyStr;
			}
			return text.Replace(' ', '\u00a0');
		}

		// Token: 0x060018DF RID: 6367 RVA: 0x0012482C File Offset: 0x00122A2C
		public static string ParseText(string text, Random random = null, TokenParserDelegate customParser = null, Farmer player = null)
		{
			if (text == null)
			{
				return null;
			}
			int startAt = text.IndexOf('[');
			if (startAt == -1)
			{
				return text;
			}
			for (int i = startAt; i < text.Length; i++)
			{
				if (text[i] == '[')
				{
					i = TokenParser.ParseTagStartingAt(ref text, i, random ?? Game1.random, customParser, player ?? Game1.player);
				}
			}
			return TokenParser.UnescapeText(text.Replace("\\n", "\n"));
		}

		// Token: 0x060018E0 RID: 6368 RVA: 0x0012489C File Offset: 0x00122A9C
		public static bool LogTokenError(string[] query, string error, out string replacement)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Failed parsing [");
			defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", query));
			defaultInterpolatedStringHandler.AppendLiteral("]: ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			replacement = null;
			return false;
		}

		// Token: 0x060018E1 RID: 6369 RVA: 0x00124907 File Offset: 0x00122B07
		public static bool LogTokenError(string[] query, Exception error, out string replacement)
		{
			Game1.log.Error("Failed parsing [" + string.Join(" ", query) + "].", error);
			replacement = null;
			return false;
		}

		// Token: 0x060018E2 RID: 6370 RVA: 0x00124934 File Offset: 0x00122B34
		private static int ParseTagStartingAt(ref string text, int startIndex, Random random, TokenParserDelegate customParser, Farmer player)
		{
			for (int i = startIndex + 1; i < text.Length; i++)
			{
				char c = text[i];
				if (c != '[')
				{
					if (c == ']')
					{
						string replacement;
						if (TokenParser.ParseTag(text.Substring(startIndex + 1, i - startIndex - 1), out replacement, random, customParser, player))
						{
							text = text.Remove(startIndex, i - startIndex + 1);
							text = text.Insert(startIndex, replacement);
							return startIndex + replacement.Length - 1;
						}
						return i;
					}
				}
				else
				{
					i = TokenParser.ParseTagStartingAt(ref text, i, random, customParser, player);
				}
			}
			return text.Length - 1;
		}

		// Token: 0x060018E3 RID: 6371 RVA: 0x001249C4 File Offset: 0x00122BC4
		private static bool ParseTag(string tag, out string replacement, Random random, TokenParserDelegate customParser, Farmer player)
		{
			string[] tagSplit = ArgUtility.SplitBySpace(tag);
			for (int i = 0; i < tagSplit.Length; i++)
			{
				tagSplit[i] = TokenParser.UnescapeText(tagSplit[i]);
			}
			if (customParser != null && customParser(tagSplit, out replacement, random, player))
			{
				return true;
			}
			TokenParserDelegate parser;
			if (TokenParser.Parsers.TryGetValue(tagSplit[0], out parser) && parser(tagSplit, out replacement, random, player))
			{
				return true;
			}
			replacement = null;
			return false;
		}

		// Token: 0x060018E4 RID: 6372 RVA: 0x00124A27 File Offset: 0x00122C27
		private static string UnescapeText(string text)
		{
			return text.Replace('\u00a0', ' ').Replace(TokenParser.EscapedEmptyStr, "");
		}

		// Token: 0x04000F00 RID: 3840
		private static readonly Dictionary<string, TokenParserDelegate> Parsers = new Dictionary<string, TokenParserDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000F01 RID: 3841
		private const char EscapedSpace = '\u00a0';

		// Token: 0x04000F02 RID: 3842
		private const char EscapedEmpty = '​';

		// Token: 0x04000F03 RID: 3843
		private static readonly string EscapedEmptyStr = '​'.ToString();

		// Token: 0x04000F04 RID: 3844
		internal const char StartTokenChar = '[';

		// Token: 0x04000F05 RID: 3845
		internal const char EndTokenChar = ']';

		// Token: 0x04000F06 RID: 3846
		internal static readonly char[] HeuristicCharactersForEscapableStrings = new char[]
		{
			' ',
			'['
		};

		// Token: 0x02000518 RID: 1304
		public static class DefaultResolvers
		{
			// Token: 0x06004071 RID: 16497 RVA: 0x00302B58 File Offset: 0x00300D58
			public static bool AchievementName(string[] query, out string replacement, Random random, Farmer player)
			{
				int achievementId;
				string error;
				if (!ArgUtility.TryGetInt(query, 1, out achievementId, out error, "int achievementId"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				string data;
				if (!Game1.achievements.TryGetValue(achievementId, out data))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
					defaultInterpolatedStringHandler.AppendLiteral("unknown achievement ID '");
					defaultInterpolatedStringHandler.AppendFormatted<int>(achievementId);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					return TokenParser.LogTokenError(query, defaultInterpolatedStringHandler.ToStringAndClear(), out replacement);
				}
				replacement = data.Split('^', 2, StringSplitOptions.None)[0];
				return true;
			}

			// Token: 0x06004072 RID: 16498 RVA: 0x00302BD8 File Offset: 0x00300DD8
			public static bool ArticleFor(string[] query, out string replacement, Random random, Farmer player)
			{
				string word;
				string error;
				if (!ArgUtility.TryGet(query, 1, out word, out error, true, "string word"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				replacement = Lexicon.getProperArticleForWord(word);
				return true;
			}

			// Token: 0x06004073 RID: 16499 RVA: 0x00302C0C File Offset: 0x00300E0C
			public static bool CapitalizeFirstLetter(string[] query, out string replacement, Random random, Farmer player)
			{
				string text;
				string error;
				if (!ArgUtility.TryGetRemainder(query, 1, out text, out error, ' ', "string text"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				replacement = Utility.capitalizeFirstLetter(text);
				return true;
			}

			// Token: 0x06004074 RID: 16500 RVA: 0x00302C3F File Offset: 0x00300E3F
			public static bool EscapedText(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = string.Join(" ", query.Skip(1));
				replacement = TokenParser.EscapeSpaces(replacement);
				return true;
			}

			// Token: 0x06004075 RID: 16501 RVA: 0x00302C60 File Offset: 0x00300E60
			public static bool GenderedText(string[] query, out string replacement, Random random, Farmer player)
			{
				string maleStr;
				string error;
				string femaleStr;
				string otherStr;
				if (!ArgUtility.TryGet(query, 1, out maleStr, out error, true, "string maleStr") || !ArgUtility.TryGet(query, 2, out femaleStr, out error, true, "string femaleStr") || !ArgUtility.TryGetOptional(query, 3, out otherStr, out error, null, true, "string otherStr"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				Gender gender = player.Gender;
				if (gender != Gender.Male)
				{
					if (gender != Gender.Female)
					{
						replacement = (otherStr ?? femaleStr);
					}
					else
					{
						replacement = femaleStr;
					}
				}
				else
				{
					replacement = maleStr;
				}
				return true;
			}

			// Token: 0x06004076 RID: 16502 RVA: 0x00302CD8 File Offset: 0x00300ED8
			public static bool ItemName(string[] query, out string replacement, Random random, Farmer player)
			{
				string itemId;
				string error;
				string fallbackItemName;
				if (!ArgUtility.TryGet(query, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptional(query, 2, out fallbackItemName, out error, null, true, "string fallbackItemName"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				ParsedItemData data = ItemRegistry.GetData(itemId);
				string text;
				if ((text = ((data != null) ? data.DisplayName : null)) == null)
				{
					text = (fallbackItemName ?? ItemRegistry.GetErrorItemName(itemId));
				}
				replacement = text;
				return true;
			}

			// Token: 0x06004077 RID: 16503 RVA: 0x00302D3C File Offset: 0x00300F3C
			public static bool ItemNameWithFlavor(string[] query, out string replacement, Random random, Farmer player)
			{
				Object.PreserveType preserveType;
				string error;
				string preservedId;
				string fallbackItemName;
				if (!ArgUtility.TryGetEnum<Object.PreserveType>(query, 1, out preserveType, out error, "Object.PreserveType preserveType") || !ArgUtility.TryGet(query, 2, out preservedId, out error, true, "string preservedId") || !ArgUtility.TryGetOptional(query, 3, out fallbackItemName, out error, null, true, "string fallbackItemName"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				string baseItemId = ItemRegistry.GetObjectTypeDefinition().GetBaseItemIdForFlavoredItem(preserveType, preservedId);
				replacement = Object.GetObjectDisplayName(baseItemId, new Object.PreserveType?(preserveType), preservedId, null, fallbackItemName);
				return true;
			}

			// Token: 0x06004078 RID: 16504 RVA: 0x00302DAC File Offset: 0x00300FAC
			public static bool LocalizedText(string[] query, out string replacement, Random random, Farmer player)
			{
				string key;
				string error;
				if (!ArgUtility.TryGet(query, 1, out key, out error, true, "string key"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				object[] replacements;
				if (query.Length > 2)
				{
					replacements = new object[query.Length - 2];
					for (int i = 2; i < query.Length; i++)
					{
						replacements[i - 2] = query[i];
					}
				}
				else
				{
					replacements = LegacyShims.EmptyArray<object>();
				}
				bool result;
				try
				{
					replacement = ((replacements.Length != 0) ? Game1.content.LoadString(key, replacements) : Game1.content.LoadString(key));
					result = true;
				}
				catch (ContentLoadException)
				{
					result = TokenParser.LogTokenError(query, "the key '" + key + "' doesn't match an existing asset", out replacement);
				}
				catch (InvalidCastException)
				{
					result = TokenParser.LogTokenError(query, "the key '" + key + "' matches an asset, but it isn't of the required type 'Dictionary<string, string>'", out replacement);
				}
				return result;
			}

			// Token: 0x06004079 RID: 16505 RVA: 0x00302E80 File Offset: 0x00301080
			public static bool MonsterName(string[] query, out string replacement, Random random, Farmer player)
			{
				string monsterId;
				string error;
				string fallbackText;
				if (!ArgUtility.TryGet(query, 1, out monsterId, out error, true, "string monsterId") || !ArgUtility.TryGetOptional(query, 2, out fallbackText, out error, null, true, "string fallbackText"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				string data;
				replacement = (DataLoader.Monsters(Game1.content).TryGetValue(monsterId, out data) ? ArgUtility.Get(data.Split('/', StringSplitOptions.None), 14, null, true) : null);
				string text;
				if ((text = replacement) == null)
				{
					text = (fallbackText ?? monsterId);
				}
				replacement = text;
				return true;
			}

			// Token: 0x0600407A RID: 16506 RVA: 0x00302EF8 File Offset: 0x003010F8
			public static bool MovieName(string[] query, out string replacement, Random random, Farmer player)
			{
				string movieId;
				string error;
				if (!ArgUtility.TryGet(query, 1, out movieId, out error, true, "string movieId"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				MovieData data;
				if (!MovieTheater.TryGetMovieData(movieId, out data))
				{
					return TokenParser.LogTokenError(query, "unknown movie ID '" + movieId + "'", out replacement);
				}
				replacement = TokenParser.ParseText(data.Title, null, null, null);
				return true;
			}

			// Token: 0x0600407B RID: 16507 RVA: 0x00302F54 File Offset: 0x00301154
			public static bool NumberWithSeparators(string[] query, out string replacement, Random random, Farmer player)
			{
				int number;
				string error;
				if (!ArgUtility.TryGetInt(query, 1, out number, out error, "int number"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				replacement = Utility.getNumberWithCommas(number);
				return true;
			}

			// Token: 0x0600407C RID: 16508 RVA: 0x00302F85 File Offset: 0x00301185
			public static bool PositiveAdjective(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(null);
				return true;
			}

			// Token: 0x0600407D RID: 16509 RVA: 0x00302F90 File Offset: 0x00301190
			public static bool SpecialOrderName(string[] query, out string replacement, Random random, Farmer player)
			{
				string orderId;
				string error;
				if (!ArgUtility.TryGet(query, 1, out orderId, out error, true, "string orderId"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.questKey.Value == orderId)
					{
						replacement = order.GetName();
						return true;
					}
				}
				SpecialOrderData data;
				if (SpecialOrder.TryGetData(orderId, out data))
				{
					replacement = SpecialOrder.MakeLocalizationReplacements(TokenParser.ParseText(data.Name, null, null, null));
					return true;
				}
				return TokenParser.LogTokenError(query, "unknown special order ID '" + orderId + "'", out replacement);
			}

			// Token: 0x0600407E RID: 16510 RVA: 0x00303060 File Offset: 0x00301260
			public static bool SpouseFarmerText(string[] query, out string replacement, Random random, Farmer player)
			{
				string playerSpouse;
				string error;
				string npcSpouse;
				if (!ArgUtility.TryGet(query, 1, out playerSpouse, out error, true, "string playerSpouse") || !ArgUtility.TryGet(query, 2, out npcSpouse, out error, true, "string npcSpouse"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				if (player.team.GetSpouse(player.UniqueMultiplayerID) != null)
				{
					replacement = playerSpouse;
					return true;
				}
				if (player.getSpouse() != null)
				{
					replacement = npcSpouse;
					return true;
				}
				return TokenParser.LogTokenError(query, "the target player '" + player.Name + "' isn't married", out replacement);
			}

			// Token: 0x0600407F RID: 16511 RVA: 0x003030E8 File Offset: 0x003012E8
			public static bool SpouseGenderedText(string[] query, out string replacement, Random random, Farmer player)
			{
				string maleStr;
				string error;
				string femaleStr;
				string otherStr;
				if (!ArgUtility.TryGet(query, 1, out maleStr, out error, true, "string maleStr") || !ArgUtility.TryGet(query, 2, out femaleStr, out error, true, "string femaleStr") || !ArgUtility.TryGetOptional(query, 3, out otherStr, out error, null, true, "string otherStr"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				Gender? gender = null;
				long? spousePlayerId = player.team.GetSpouse(player.UniqueMultiplayerID);
				if (spousePlayerId != null)
				{
					Farmer spouse = Game1.GetPlayer(spousePlayerId.Value, false);
					gender = new Gender?((spouse != null) ? spouse.Gender : Gender.Male);
				}
				else
				{
					NPC spouse2 = player.getSpouse();
					gender = ((spouse2 != null) ? new Gender?(spouse2.Gender) : null);
				}
				if (gender != null)
				{
					if (gender != null)
					{
						Gender valueOrDefault = gender.GetValueOrDefault();
						if (valueOrDefault == Gender.Male)
						{
							replacement = maleStr;
							return true;
						}
						if (valueOrDefault == Gender.Female)
						{
							replacement = femaleStr;
							return true;
						}
					}
					replacement = (otherStr ?? femaleStr);
					return true;
				}
				return TokenParser.LogTokenError(query, "the target player '" + player.Name + "' isn't married", out replacement);
			}

			// Token: 0x06004080 RID: 16512 RVA: 0x003031FC File Offset: 0x003013FC
			public static bool ToolName(string[] query, out string replacement, Random random, Farmer player)
			{
				string itemId;
				string error;
				int upgradeLevel;
				if (!ArgUtility.TryGet(query, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(query, 2, out upgradeLevel, out error, -1, "int upgradeLevel"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
				if (!data.HasTypeId("(T)"))
				{
					return TokenParser.LogTokenError(query, "the item ID '" + itemId + "' matches a non-tool item", out replacement);
				}
				replacement = data.DisplayName;
				return true;
			}

			// Token: 0x06004081 RID: 16513 RVA: 0x0030326D File Offset: 0x0030146D
			public static bool DayOfMonth(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = Game1.dayOfMonth.ToString();
				return true;
			}

			// Token: 0x06004082 RID: 16514 RVA: 0x0030327C File Offset: 0x0030147C
			public static bool Season(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = Game1.CurrentSeasonDisplayName;
				return true;
			}

			// Token: 0x06004083 RID: 16515 RVA: 0x00303288 File Offset: 0x00301488
			public static bool CharacterName(string[] query, out string replacement, Random random, Farmer player)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(query, 1, out npcName, out error, true, "string npcName"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				NPC character = Game1.getCharacterFromName(npcName, true, false);
				if (character == null)
				{
					return TokenParser.LogTokenError(query, "no character found with name '" + npcName + "'", out replacement);
				}
				replacement = character.displayName;
				return true;
			}

			// Token: 0x06004084 RID: 16516 RVA: 0x003032DE File Offset: 0x003014DE
			public static bool FarmName(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = player.farmName.Value;
				return true;
			}

			// Token: 0x06004085 RID: 16517 RVA: 0x003032F0 File Offset: 0x003014F0
			public static bool FarmerUniqueId(string[] query, out string replacement, Random random, Farmer player)
			{
				replacement = player.UniqueMultiplayerID.ToString();
				return true;
			}

			// Token: 0x06004086 RID: 16518 RVA: 0x00303310 File Offset: 0x00301510
			public static bool LocationName(string[] query, out string replacement, Random random, Farmer player)
			{
				string locationKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out locationKey, out error, true, "string locationKey"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				GameLocation location = Game1.getLocationFromName(locationKey);
				if (location == null)
				{
					return TokenParser.LogTokenError(query, "no location found with name '" + locationKey + "'", out replacement);
				}
				replacement = location.DisplayName;
				return true;
			}

			// Token: 0x06004087 RID: 16519 RVA: 0x00303364 File Offset: 0x00301564
			public static bool FarmerStat(string[] query, out string replacement, Random random, Farmer player)
			{
				string statName;
				string error;
				if (!ArgUtility.TryGet(query, 1, out statName, out error, true, "string statName"))
				{
					return TokenParser.LogTokenError(query, error, out replacement);
				}
				replacement = player.stats.Get(statName).ToString();
				return true;
			}
		}
	}
}
