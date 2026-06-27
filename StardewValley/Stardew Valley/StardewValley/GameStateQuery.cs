using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.Objects.Trinkets;

namespace StardewValley
{
	// Token: 0x020000B2 RID: 178
	public class GameStateQuery
	{
		// Token: 0x06000C60 RID: 3168 RVA: 0x0008CD58 File Offset: 0x0008AF58
		static GameStateQuery()
		{
			MethodInfo[] methods = typeof(GameStateQuery.DefaultResolvers).GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo method in methods)
			{
				GameStateQueryDelegate queryDelegate = (GameStateQueryDelegate)Delegate.CreateDelegate(typeof(GameStateQueryDelegate), method);
				GameStateQuery.Register(method.Name, queryDelegate);
			}
			foreach (MethodInfo method2 in methods)
			{
				OtherNamesAttribute attribute = method2.GetCustomAttribute<OtherNamesAttribute>();
				if (attribute != null)
				{
					string[] aliases = attribute.Aliases;
					for (int j = 0; j < aliases.Length; j++)
					{
						GameStateQuery.RegisterAlias(aliases[j], method2.Name);
					}
				}
			}
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x0008CEBC File Offset: 0x0008B0BC
		internal static void Update()
		{
			if (Game1.ticks >= GameStateQuery.NextClearCacheTick)
			{
				if (GameStateQuery.ParseCache.Count > 50)
				{
					GameStateQuery.ParseCache.Clear();
				}
				GameStateQuery.NextClearCacheTick = Game1.ticks + 3600;
			}
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x0008CEF2 File Offset: 0x0008B0F2
		public static bool Exists(string queryKey)
		{
			return queryKey != null && (GameStateQuery.QueryTypeLookup.ContainsKey(queryKey) || GameStateQuery.Aliases.ContainsKey(queryKey));
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x0008CF14 File Offset: 0x0008B114
		public static void Register(string queryKey, GameStateQueryDelegate queryDelegate)
		{
			queryKey = ((queryKey != null) ? queryKey.Trim() : null);
			if (string.IsNullOrWhiteSpace(queryKey))
			{
				throw new ArgumentException("The query key can't be null or empty.", "queryKey");
			}
			if (GameStateQuery.QueryTypeLookup.ContainsKey(queryKey))
			{
				throw new InvalidOperationException("The query key '" + queryKey + "' is already registered.");
			}
			string aliasFor;
			if (GameStateQuery.Aliases.TryGetValue(queryKey, out aliasFor))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The query key '");
				defaultInterpolatedStringHandler.AppendFormatted(queryKey);
				defaultInterpolatedStringHandler.AppendLiteral("' is already registered as an alias of '");
				defaultInterpolatedStringHandler.AppendFormatted(aliasFor);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			Dictionary<string, GameStateQueryDelegate> queryTypeLookup = GameStateQuery.QueryTypeLookup;
			string key = queryKey;
			if (queryDelegate == null)
			{
				throw new ArgumentNullException("queryDelegate");
			}
			queryTypeLookup[key] = queryDelegate;
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x0008CFE0 File Offset: 0x0008B1E0
		public static void RegisterAlias(string alias, string queryKey)
		{
			alias = ((alias != null) ? alias.Trim() : null);
			if (string.IsNullOrWhiteSpace(alias))
			{
				throw new ArgumentException("The alias can't be null or empty.", "alias");
			}
			if (GameStateQuery.QueryTypeLookup.ContainsKey(alias))
			{
				throw new InvalidOperationException("The alias '" + alias + "' is already registered as a game state query.");
			}
			string otherQuery;
			if (GameStateQuery.Aliases.TryGetValue(alias, out otherQuery))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' is already registered for '");
				defaultInterpolatedStringHandler.AppendFormatted(otherQuery);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (string.IsNullOrWhiteSpace(queryKey))
			{
				throw new ArgumentException("The query key can't be null or empty.", "alias");
			}
			if (!GameStateQuery.QueryTypeLookup.ContainsKey(queryKey))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The alias '");
				defaultInterpolatedStringHandler.AppendFormatted(alias);
				defaultInterpolatedStringHandler.AppendLiteral("' can't be registered for '");
				defaultInterpolatedStringHandler.AppendFormatted(queryKey);
				defaultInterpolatedStringHandler.AppendLiteral("' because there's no game state query with that name.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			GameStateQuery.Aliases[alias] = queryKey;
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x0008D10C File Offset: 0x0008B30C
		public static bool CheckConditions(string queryString, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null, Random random = null, HashSet<string> ignoreQueryKeys = null)
		{
			if (queryString == null || (queryString != null && queryString.Length == 0) || queryString == "TRUE")
			{
				return true;
			}
			if (!(queryString == "FALSE"))
			{
				GameStateQueryContext context = new GameStateQueryContext(location, player, targetItem, inputItem, random, ignoreQueryKeys, null);
				return GameStateQuery.CheckConditionsImpl(queryString, context);
			}
			return false;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x0008D15F File Offset: 0x0008B35F
		public static bool CheckConditions(string queryString, GameStateQueryContext context)
		{
			return queryString == null || (queryString != null && queryString.Length == 0) || queryString == "TRUE" || (!(queryString == "FALSE") && GameStateQuery.CheckConditionsImpl(queryString, context));
		}

		// Token: 0x06000C67 RID: 3175 RVA: 0x0008D198 File Offset: 0x0008B398
		public static bool IsImmutablyFalse(string queryString)
		{
			if (queryString == null || (queryString != null && queryString.Length == 0) || queryString == "TRUE")
			{
				return false;
			}
			if (!(queryString == "FALSE"))
			{
				foreach (GameStateQuery.ParsedGameStateQuery query in GameStateQuery.Parse(queryString))
				{
					if (query.Query.Length != 0)
					{
						string immutableFalseName = query.Negated ? "TRUE" : "FALSE";
						if (query.Query[0].EqualsIgnoreCase(immutableFalseName))
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x0008D224 File Offset: 0x0008B424
		public static bool IsImmutablyTrue(string queryString)
		{
			if (queryString == null || (queryString != null && queryString.Length == 0) || queryString == "TRUE")
			{
				return true;
			}
			if (!(queryString == "FALSE"))
			{
				foreach (GameStateQuery.ParsedGameStateQuery query in GameStateQuery.Parse(queryString))
				{
					if (query.Query.Length != 0)
					{
						string immutableTrueName = query.Negated ? "FALSE" : "TRUE";
						if (!query.Query[0].EqualsIgnoreCase(immutableTrueName))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x0008D2B0 File Offset: 0x0008B4B0
		public static GameStateQuery.ParsedGameStateQuery[] Parse(string queryString)
		{
			GameStateQuery.ParsedGameStateQuery[] parsed;
			if (!GameStateQuery.ParseCache.TryGetValue(queryString, out parsed))
			{
				string[] rawQueries = GameStateQuery.SplitRaw(queryString);
				parsed = new GameStateQuery.ParsedGameStateQuery[rawQueries.Length];
				for (int i = 0; i < rawQueries.Length; i++)
				{
					string[] query = ArgUtility.SplitBySpaceQuoteAware(rawQueries[i]);
					string key = query[0];
					bool negated = key.StartsWith('!');
					if (negated)
					{
						key = (query[0] = key.Substring(1));
					}
					string aliasFor;
					if (GameStateQuery.Aliases.TryGetValue(key, out aliasFor))
					{
						key = aliasFor;
						query[0] = aliasFor;
					}
					GameStateQueryDelegate resolver;
					if (!GameStateQuery.QueryTypeLookup.TryGetValue(key, out resolver))
					{
						if (parsed.Length > 1)
						{
							parsed = new GameStateQuery.ParsedGameStateQuery[1];
						}
						parsed[0] = new GameStateQuery.ParsedGameStateQuery(false, query, null, "'" + key + "' isn't a known query or alias");
						break;
					}
					parsed[i] = new GameStateQuery.ParsedGameStateQuery(negated, query, resolver, null);
				}
				GameStateQuery.ParseCache[queryString] = parsed;
			}
			return parsed;
		}

		// Token: 0x06000C6A RID: 3178 RVA: 0x0008D395 File Offset: 0x0008B595
		public static string[] SplitRaw(string queryString)
		{
			return ArgUtility.SplitQuoteAware(queryString, ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, true);
		}

		// Token: 0x06000C6B RID: 3179 RVA: 0x0008D3A4 File Offset: 0x0008B5A4
		private static bool CheckConditionsImpl(string queryString, GameStateQueryContext context)
		{
			if (queryString == null)
			{
				return true;
			}
			GameStateQuery.ParsedGameStateQuery[] parsed = GameStateQuery.Parse(queryString);
			if (parsed.Length == 0)
			{
				return true;
			}
			if (parsed[0].Error != null)
			{
				return GameStateQuery.Helpers.ErrorResult(parsed[0].Query, parsed[0].Error, null);
			}
			foreach (GameStateQuery.ParsedGameStateQuery query in parsed)
			{
				HashSet<string> ignoreQueryKeys = context.IgnoreQueryKeys;
				if (ignoreQueryKeys == null || !ignoreQueryKeys.Contains(query.Query[0]))
				{
					try
					{
						if (query.Resolver(query.Query, context) == query.Negated)
						{
							return false;
						}
					}
					catch (Exception e)
					{
						return GameStateQuery.Helpers.ErrorResult(query.Query, "unhandled exception", e);
					}
				}
			}
			return true;
		}

		// Token: 0x0400088A RID: 2186
		private static readonly Dictionary<string, GameStateQueryDelegate> QueryTypeLookup = new Dictionary<string, GameStateQueryDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x0400088B RID: 2187
		private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x0400088C RID: 2188
		private static int NextClearCacheTick;

		// Token: 0x0400088D RID: 2189
		private static readonly Dictionary<string, GameStateQuery.ParsedGameStateQuery[]> ParseCache = new Dictionary<string, GameStateQuery.ParsedGameStateQuery[]>();

		// Token: 0x0400088E RID: 2190
		public static HashSet<string> SeasonQueryKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"LOCATION_SEASON",
			"SEASON"
		};

		// Token: 0x0400088F RID: 2191
		public static HashSet<string> MagicBaitIgnoreQueryKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"DAY_OF_MONTH",
			"DAY_OF_WEEK",
			"DAYS_PLAYED",
			"LOCATION_SEASON",
			"SEASON",
			"SEASON_DAY",
			"WEATHER",
			"TIME"
		};

		// Token: 0x02000461 RID: 1121
		public static class Helpers
		{
			// Token: 0x06003D9F RID: 15775 RVA: 0x002F62CB File Offset: 0x002F44CB
			public static GameLocation GetLocation(string locationName, GameLocation contextualLocation)
			{
				if (locationName.EqualsIgnoreCase("Here"))
				{
					return Game1.currentLocation;
				}
				if (locationName.EqualsIgnoreCase("Target"))
				{
					return contextualLocation ?? Game1.currentLocation;
				}
				return Game1.getLocationFromName(locationName);
			}

			// Token: 0x06003DA0 RID: 15776 RVA: 0x002F62FE File Offset: 0x002F44FE
			public static GameLocation RequireLocation(string locationName, GameLocation contextualLocation)
			{
				GameLocation location = GameStateQuery.Helpers.GetLocation(locationName, contextualLocation);
				if (location == null)
				{
					throw new KeyNotFoundException("Required location '" + locationName + "' not found.");
				}
				return location;
			}

			// Token: 0x06003DA1 RID: 15777 RVA: 0x002F6324 File Offset: 0x002F4524
			public static bool TryGetLocationArg(string[] query, int index, ref GameLocation location, out string error)
			{
				string locationTarget;
				if (!ArgUtility.TryGet(query, index, out locationTarget, out error, true, "string locationTarget"))
				{
					location = null;
					return false;
				}
				GameLocation loaded = GameStateQuery.Helpers.GetLocation(locationTarget, location);
				if (loaded == null)
				{
					error = "no location found matching '" + locationTarget + "'";
					return false;
				}
				location = loaded;
				return true;
			}

			// Token: 0x06003DA2 RID: 15778 RVA: 0x002F636C File Offset: 0x002F456C
			public static bool TryGetItemArg(string[] query, int index, Item targetItem, Item inputItem, out Item item, out string error)
			{
				string itemType;
				if (!ArgUtility.TryGet(query, index, out itemType, out error, true, "string itemType"))
				{
					item = null;
					return false;
				}
				if (itemType.EqualsIgnoreCase("Target"))
				{
					item = targetItem;
					return true;
				}
				if (itemType.EqualsIgnoreCase("Input"))
				{
					item = inputItem;
					return true;
				}
				item = null;
				error = "invalid item type '" + itemType + "' (should be 'Input' or 'Target')";
				return false;
			}

			// Token: 0x06003DA3 RID: 15779 RVA: 0x002F63D0 File Offset: 0x002F45D0
			public static bool WithPlayer(Farmer contextualPlayer, string playerKey, Func<Farmer, bool> check)
			{
				if (playerKey.EqualsIgnoreCase("Any"))
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (check(farmer))
						{
							return true;
						}
					}
					return false;
				}
				if (playerKey.EqualsIgnoreCase("All"))
				{
					foreach (Farmer farmer2 in Game1.getAllFarmers())
					{
						if (!check(farmer2))
						{
							return false;
						}
					}
					return true;
				}
				if (playerKey.EqualsIgnoreCase("Current"))
				{
					return check(Game1.player);
				}
				if (playerKey.EqualsIgnoreCase("Target"))
				{
					return check(contextualPlayer);
				}
				if (playerKey.EqualsIgnoreCase("Host"))
				{
					return check(Game1.MasterPlayer);
				}
				long parsedId;
				return long.TryParse(playerKey, out parsedId) && check(Game1.GetPlayer(parsedId, false));
			}

			// Token: 0x06003DA4 RID: 15780 RVA: 0x002F64E8 File Offset: 0x002F46E8
			public static bool AnyArgMatches(string[] query, int startAt, Func<string, bool?> check)
			{
				for (int i = startAt; i < query.Length; i++)
				{
					bool? flag = check(query[i]);
					if (flag == null)
					{
						return false;
					}
					if (flag.GetValueOrDefault())
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003DA5 RID: 15781 RVA: 0x002F6524 File Offset: 0x002F4724
			public static bool ErrorResult(string[] query, string reason, Exception exception = null)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed parsing condition '");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", query));
				defaultInterpolatedStringHandler.AppendLiteral("': ");
				defaultInterpolatedStringHandler.AppendFormatted(reason);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), exception);
				return false;
			}

			// Token: 0x06003DA6 RID: 15782 RVA: 0x002F658C File Offset: 0x002F478C
			public static bool PlayerSkillLevelImpl(string[] query, Farmer player, Func<Farmer, int> getLevel)
			{
				int minLevel;
				int maxLevel;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out minLevel, out error, "int minLevel") || !ArgUtility.TryGetOptionalInt(query, 3, out maxLevel, out error, 2147483647, "int maxLevel"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(player, playerKey, delegate(Farmer target)
				{
					int level = getLevel(target);
					return level >= minLevel && level <= maxLevel;
				});
			}

			// Token: 0x06003DA7 RID: 15783 RVA: 0x002F6608 File Offset: 0x002F4808
			public static bool RandomImpl(Random random, string[] query, int skipArguments)
			{
				float chance;
				string error;
				if (!ArgUtility.TryGetFloat(query, skipArguments, out chance, out error, "float chance"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool addDailyLuck = false;
				for (int i = skipArguments + 1; i < query.Length; i++)
				{
					if (query[i].EqualsIgnoreCase("@addDailyLuck"))
					{
						addDailyLuck = true;
					}
				}
				if (addDailyLuck)
				{
					chance += (float)Game1.player.DailyLuck;
				}
				return random.NextDouble() < (double)chance;
			}
		}

		// Token: 0x02000462 RID: 1122
		public readonly struct ParsedGameStateQuery
		{
			// Token: 0x06003DA8 RID: 15784 RVA: 0x002F666E File Offset: 0x002F486E
			public ParsedGameStateQuery(bool negated, string[] query, GameStateQueryDelegate resolver, string error)
			{
				this.Negated = negated;
				this.Query = query;
				this.Resolver = resolver;
				this.Error = error;
			}

			// Token: 0x0400281B RID: 10267
			public readonly bool Negated;

			// Token: 0x0400281C RID: 10268
			public readonly string[] Query;

			// Token: 0x0400281D RID: 10269
			public readonly GameStateQueryDelegate Resolver;

			// Token: 0x0400281E RID: 10270
			public readonly string Error;
		}

		// Token: 0x02000463 RID: 1123
		public static class DefaultResolvers
		{
			// Token: 0x06003DA9 RID: 15785 RVA: 0x002F6690 File Offset: 0x002F4890
			public static bool ANY(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, (string value) => new bool?(GameStateQuery.CheckConditions(value, context)));
			}

			// Token: 0x06003DAA RID: 15786 RVA: 0x002F66C0 File Offset: 0x002F48C0
			public static bool DATE_RANGE(string[] query, GameStateQueryContext context)
			{
				Season minSeason;
				string error;
				int minDayOfMonth;
				int minYear;
				Season maxSeason;
				int maxDayOfMonth;
				int maxYear;
				if (!ArgUtility.TryGetEnum<Season>(query, 1, out minSeason, out error, "Season minSeason") || !ArgUtility.TryGetInt(query, 2, out minDayOfMonth, out error, "int minDayOfMonth") || !ArgUtility.TryGetInt(query, 3, out minYear, out error, "int minYear") || !ArgUtility.TryGetOptionalEnum<Season>(query, 4, out maxSeason, out error, Season.Winter, "Season maxSeason") || !ArgUtility.TryGetOptionalInt(query, 5, out maxDayOfMonth, out error, 28, "int maxDayOfMonth") || !ArgUtility.TryGetOptionalInt(query, 6, out maxYear, out error, 2147483647, "int maxYear"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int minDaysPlayed = WorldDate.GetDaysPlayed(minYear, minSeason, minDayOfMonth);
				int maxDaysPlayed = (maxYear != int.MaxValue) ? WorldDate.GetDaysPlayed(maxYear, maxSeason, maxDayOfMonth) : int.MaxValue;
				int daysPlayed = Game1.Date.TotalDays;
				return daysPlayed >= minDaysPlayed && daysPlayed <= maxDaysPlayed;
			}

			// Token: 0x06003DAB RID: 15787 RVA: 0x002F6790 File Offset: 0x002F4990
			public static bool SEASON_DAY(string[] query, GameStateQueryContext context)
			{
				for (int i = 1; i < query.Length; i += 2)
				{
					Season season;
					string error;
					int day;
					if (!ArgUtility.TryGetEnum<Season>(query, i, out season, out error, "Season season") || !ArgUtility.TryGetInt(query, i + 1, out day, out error, "int day"))
					{
						return GameStateQuery.Helpers.ErrorResult(query, error, null);
					}
					if (Game1.season == season && Game1.dayOfMonth == day)
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003DAC RID: 15788 RVA: 0x002F67F0 File Offset: 0x002F49F0
			public static bool DAY_OF_MONTH(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, delegate(string rawDay)
				{
					int dayNumber;
					if (int.TryParse(rawDay, out dayNumber))
					{
						return new bool?(Game1.dayOfMonth == dayNumber);
					}
					if (rawDay.EqualsIgnoreCase("even"))
					{
						return new bool?(Game1.dayOfMonth % 2 == 0);
					}
					if (rawDay.EqualsIgnoreCase("odd"))
					{
						return new bool?(Game1.dayOfMonth % 2 == 1);
					}
					GameStateQuery.Helpers.ErrorResult(query, "'" + rawDay + "' isn't a valid day of month", null);
					return null;
				});
			}

			// Token: 0x06003DAD RID: 15789 RVA: 0x002F6824 File Offset: 0x002F4A24
			public static bool DAY_OF_WEEK(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, delegate(string rawDay)
				{
					DayOfWeek dayOfWeek;
					if (!WorldDate.TryGetDayOfWeekFor(rawDay, out dayOfWeek))
					{
						GameStateQuery.Helpers.ErrorResult(query, "'" + rawDay + "' isn't a valid day of week", null);
						return null;
					}
					if (Game1.Date.DayOfWeek == dayOfWeek)
					{
						return new bool?(true);
					}
					return new bool?(false);
				});
			}

			// Token: 0x06003DAE RID: 15790 RVA: 0x002F6858 File Offset: 0x002F4A58
			public static bool DAYS_PLAYED(string[] query, GameStateQueryContext context)
			{
				int minDaysPlayed;
				string error;
				int maxDaysPlayed;
				if (!ArgUtility.TryGetInt(query, 1, out minDaysPlayed, out error, "int minDaysPlayed") || !ArgUtility.TryGetOptionalInt(query, 2, out maxDaysPlayed, out error, 2147483647, "int maxDaysPlayed"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				uint daysPlayed = Game1.stats.DaysPlayed;
				return (ulong)daysPlayed >= (ulong)((long)minDaysPlayed) && (ulong)daysPlayed <= (ulong)((long)maxDaysPlayed);
			}

			// Token: 0x06003DAF RID: 15791 RVA: 0x002F68B4 File Offset: 0x002F4AB4
			public static bool IS_GREEN_RAIN_DAY(string[] query, GameStateQueryContext context)
			{
				WorldDate tomorrow = new WorldDate(Game1.Date);
				WorldDate worldDate = tomorrow;
				int totalDays = worldDate.TotalDays;
				worldDate.TotalDays = totalDays + 1;
				return Utility.isGreenRainDay(tomorrow.DayOfMonth, tomorrow.Season);
			}

			// Token: 0x06003DB0 RID: 15792 RVA: 0x002F68F0 File Offset: 0x002F4AF0
			public static bool IS_FESTIVAL_DAY(string[] query, GameStateQueryContext context)
			{
				string locationContextId;
				string error;
				int dayOffset;
				if (!ArgUtility.TryGetOptional(query, 1, out locationContextId, out error, "any", false, "string locationContextId") || !ArgUtility.TryGetOptionalInt(query, 2, out dayOffset, out error, 0, "int dayOffset"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string text = (locationContextId != null) ? locationContextId.ToLower() : null;
				if (text != null && !(text == "any"))
				{
					if (text == "here" || text == "target")
					{
						locationContextId = GameStateQuery.Helpers.RequireLocation(locationContextId, context.Location).GetLocationContextId();
					}
				}
				else
				{
					locationContextId = null;
				}
				int num = (Game1.Date.TotalDays + dayOffset) % 112;
				Season season = (Season)(num / 28);
				return Utility.isFestivalDay(num % 28 + 1, season, locationContextId);
			}

			// Token: 0x06003DB1 RID: 15793 RVA: 0x002F69A8 File Offset: 0x002F4BA8
			public static bool IS_PASSIVE_FESTIVAL_OPEN(string[] query, GameStateQueryContext context)
			{
				string festivalId;
				string error;
				if (!ArgUtility.TryGet(query, 1, out festivalId, out error, true, "string festivalId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return Utility.IsPassiveFestivalOpen(festivalId);
			}

			// Token: 0x06003DB2 RID: 15794 RVA: 0x002F69D8 File Offset: 0x002F4BD8
			public static bool IS_PASSIVE_FESTIVAL_TODAY(string[] query, GameStateQueryContext context)
			{
				string festivalId;
				string error;
				if (!ArgUtility.TryGet(query, 1, out festivalId, out error, true, "string festivalId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return Utility.IsPassiveFestivalDay(festivalId);
			}

			// Token: 0x06003DB3 RID: 15795 RVA: 0x002F6A08 File Offset: 0x002F4C08
			public static bool SEASON(string[] query, GameStateQueryContext context)
			{
				for (int i = 1; i < query.Length; i++)
				{
					Season season;
					string error;
					if (!ArgUtility.TryGetEnum<Season>(query, i, out season, out error, "Season season"))
					{
						return GameStateQuery.Helpers.ErrorResult(query, error, null);
					}
					if (Game1.season == season)
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003DB4 RID: 15796 RVA: 0x002F6A4C File Offset: 0x002F4C4C
			public static bool YEAR(string[] query, GameStateQueryContext context)
			{
				int minYear;
				string error;
				int maxYear;
				if (!ArgUtility.TryGetInt(query, 1, out minYear, out error, "int minYear") || !ArgUtility.TryGetOptionalInt(query, 2, out maxYear, out error, 2147483647, "int maxYear"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int year = Game1.year;
				return year >= minYear && year <= maxYear;
			}

			// Token: 0x06003DB5 RID: 15797 RVA: 0x002F6AA0 File Offset: 0x002F4CA0
			public static bool TIME(string[] query, GameStateQueryContext context)
			{
				int minTime;
				string error;
				int maxTime;
				if (!ArgUtility.TryGetInt(query, 1, out minTime, out error, "int minTime") || !ArgUtility.TryGetOptionalInt(query, 2, out maxTime, out error, 2147483647, "int maxTime"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int time = Game1.timeOfDay;
				return time >= minTime && time <= maxTime;
			}

			// Token: 0x06003DB6 RID: 15798 RVA: 0x002F6AF4 File Offset: 0x002F4CF4
			[OtherNames(new string[]
			{
				"EVENT_ID"
			})]
			public static bool IS_EVENT(string[] query, GameStateQueryContext context)
			{
				Event @event = Game1.CurrentEvent;
				return @event != null && (query.Length == 1 || GameStateQuery.Helpers.AnyArgMatches(query, 1, (string eventId) => new bool?(eventId == @event.id)));
			}

			// Token: 0x06003DB7 RID: 15799 RVA: 0x002F6B38 File Offset: 0x002F4D38
			public static bool CAN_BUILD_CABIN(string[] query, GameStateQueryContext context)
			{
				int totalCabins = Game1.GetNumberBuildingsConstructed("Cabin", false);
				return Game1.IsMasterGame && totalCabins < Game1.CurrentPlayerLimit - 1;
			}

			// Token: 0x06003DB8 RID: 15800 RVA: 0x002F6B64 File Offset: 0x002F4D64
			public static bool CAN_BUILD_FOR_CABINS(string[] query, GameStateQueryContext context)
			{
				string buildingType;
				string error;
				if (!ArgUtility.TryGet(query, 1, out buildingType, out error, true, "string buildingType"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int totalCabins = Game1.GetNumberBuildingsConstructed("Cabin", false);
				return Game1.GetNumberBuildingsConstructed(buildingType, false) < totalCabins + 1;
			}

			// Token: 0x06003DB9 RID: 15801 RVA: 0x002F6BA8 File Offset: 0x002F4DA8
			public static bool BUILDINGS_CONSTRUCTED(string[] query, GameStateQueryContext context)
			{
				string locationFilter;
				string error;
				string buildingType;
				int minCount;
				int maxCount;
				bool includeUnderConstruction;
				if (!ArgUtility.TryGet(query, 1, out locationFilter, out error, true, "string locationFilter") || !ArgUtility.TryGetOptional(query, 2, out buildingType, out error, "All", true, "string buildingType") || !ArgUtility.TryGetOptionalInt(query, 3, out minCount, out error, 1, "int minCount") || !ArgUtility.TryGetOptionalInt(query, 4, out maxCount, out error, 2147483647, "int maxCount") || !ArgUtility.TryGetOptionalBool(query, 5, out includeUnderConstruction, out error, false, "bool includeUnderConstruction"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool allLocations = locationFilter.EqualsIgnoreCase("All");
				bool allBuildings = buildingType.EqualsIgnoreCase("All");
				GameLocation location = context.Location;
				if (!allLocations)
				{
					location = GameStateQuery.Helpers.GetLocation(locationFilter, location);
					if (location == null)
					{
						return GameStateQuery.Helpers.ErrorResult(query, "required index 2 has value '" + locationFilter + "', which doesn't match an existing location name or one of the special keys (All, Here, or Target)", null);
					}
				}
				int count;
				if (allLocations)
				{
					count = (allBuildings ? Game1.GetNumberBuildingsConstructed(includeUnderConstruction) : Game1.GetNumberBuildingsConstructed(buildingType, includeUnderConstruction));
				}
				else
				{
					count = (allBuildings ? location.getNumberBuildingsConstructed(includeUnderConstruction) : location.getNumberBuildingsConstructed(buildingType, includeUnderConstruction));
				}
				return count >= minCount && count <= maxCount;
			}

			// Token: 0x06003DBA RID: 15802 RVA: 0x002F6CBC File Offset: 0x002F4EBC
			public static bool FARM_CAVE(string[] query, GameStateQueryContext context)
			{
				string text;
				string error;
				if (!ArgUtility.TryGet(query, 1, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int value = Game1.MasterPlayer.caveChoice.Value;
				string caveType;
				if (value != 1)
				{
					if (value != 2)
					{
						caveType = "None";
					}
					else
					{
						caveType = "Mushrooms";
					}
				}
				else
				{
					caveType = "Bats";
				}
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, (string rawCaveType) => new bool?(rawCaveType.EqualsIgnoreCase(caveType)));
			}

			// Token: 0x06003DBB RID: 15803 RVA: 0x002F6D40 File Offset: 0x002F4F40
			public static bool FARM_NAME(string[] query, GameStateQueryContext context)
			{
				string farmName;
				string error;
				if (!ArgUtility.TryGetRemainder(query, 1, out farmName, out error, ' ', "string farmName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return context.Player.farmName.Value.EqualsIgnoreCase(farmName);
			}

			// Token: 0x06003DBC RID: 15804 RVA: 0x002F6D80 File Offset: 0x002F4F80
			public static bool FARM_TYPE(string[] query, GameStateQueryContext context)
			{
				string text;
				string error;
				if (!ArgUtility.TryGet(query, 1, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string farmTypeId = Game1.GetFarmTypeID();
				string farmTypeKey = Game1.GetFarmTypeKey();
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, (string rawFarmType) => new bool?(rawFarmType.EqualsIgnoreCase(farmTypeId) || rawFarmType.EqualsIgnoreCase(farmTypeKey)));
			}

			// Token: 0x06003DBD RID: 15805 RVA: 0x002F6DD8 File Offset: 0x002F4FD8
			public static bool FOUND_ALL_LOST_BOOKS(string[] query, GameStateQueryContext context)
			{
				return Game1.netWorldState.Value.LostBooksFound >= 21;
			}

			// Token: 0x06003DBE RID: 15806 RVA: 0x002F6DF0 File Offset: 0x002F4FF0
			public static bool HAS_TARGET_LOCATION(string[] query, GameStateQueryContext context)
			{
				return context.ExplicitTargetLocation != null;
			}

			// Token: 0x06003DBF RID: 15807 RVA: 0x002F6DFB File Offset: 0x002F4FFB
			public static bool IS_COMMUNITY_CENTER_COMPLETE(string[] query, GameStateQueryContext context)
			{
				return Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember");
			}

			// Token: 0x06003DC0 RID: 15808 RVA: 0x002F6E22 File Offset: 0x002F5022
			public static bool IS_CUSTOM_FARM_TYPE(string[] query, GameStateQueryContext context)
			{
				return Game1.whichFarm == 7;
			}

			// Token: 0x06003DC1 RID: 15809 RVA: 0x002F6E2C File Offset: 0x002F502C
			public static bool IS_HOST(string[] query, GameStateQueryContext context)
			{
				return Game1.IsMasterGame;
			}

			// Token: 0x06003DC2 RID: 15810 RVA: 0x002F6E33 File Offset: 0x002F5033
			public static bool IS_ISLAND_NORTH_BRIDGE_FIXED(string[] query, GameStateQueryContext context)
			{
				IslandNorth islandNorth = (IslandNorth)Game1.getLocationFromName("IslandNorth");
				return islandNorth != null && islandNorth.bridgeFixed.Value;
			}

			// Token: 0x06003DC3 RID: 15811 RVA: 0x002F6E54 File Offset: 0x002F5054
			public static bool IS_JOJA_MART_COMPLETE(string[] query, GameStateQueryContext context)
			{
				return Utility.hasFinishedJojaRoute();
			}

			// Token: 0x06003DC4 RID: 15812 RVA: 0x002F6E5B File Offset: 0x002F505B
			public static bool IS_MULTIPLAYER(string[] query, GameStateQueryContext context)
			{
				return Game1.IsMultiplayer;
			}

			// Token: 0x06003DC5 RID: 15813 RVA: 0x002F6E64 File Offset: 0x002F5064
			public static bool IS_VISITING_ISLAND(string[] query, GameStateQueryContext context)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(query, 1, out npcName, out error, true, "string npcName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return Game1.IsVisitingIslandToday(npcName);
			}

			// Token: 0x06003DC6 RID: 15814 RVA: 0x002F6E94 File Offset: 0x002F5094
			public static bool LOCATION_ACCESSIBLE(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return Game1.isLocationAccessible(location.NameOrUniqueName);
			}

			// Token: 0x06003DC7 RID: 15815 RVA: 0x002F6ECC File Offset: 0x002F50CC
			public static bool LOCATION_CONTEXT(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				string text;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error) || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string contextId = location.GetLocationContextId();
				return GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawContextId) => new bool?(rawContextId.EqualsIgnoreCase(contextId)));
			}

			// Token: 0x06003DC8 RID: 15816 RVA: 0x002F6F30 File Offset: 0x002F5130
			public static bool LOCATION_HAS_CUSTOM_FIELD(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				string fieldKey;
				string value;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error) || !ArgUtility.TryGet(query, 2, out fieldKey, out error, false, "string fieldKey") || !ArgUtility.TryGetOptional(query, 3, out value, out error, null, true, "string value"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool checkValue = ArgUtility.HasIndex<string>(query, 3);
				LocationData data = (location != null) ? location.GetData() : null;
				string actualValue;
				return ((data != null) ? data.CustomFields : null) != null && data.CustomFields.TryGetValue(fieldKey, out actualValue) && (!checkValue || actualValue == value);
			}

			// Token: 0x06003DC9 RID: 15817 RVA: 0x002F6FCC File Offset: 0x002F51CC
			public static bool LOCATION_IS_INDOORS(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool? flag = (location != null) ? new bool?(location.IsOutdoors) : null;
				return flag != null && !flag.GetValueOrDefault();
			}

			// Token: 0x06003DCA RID: 15818 RVA: 0x002F7028 File Offset: 0x002F5228
			public static bool LOCATION_IS_OUTDOORS(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return ((location != null) ? new bool?(location.IsOutdoors) : null) ?? false;
			}

			// Token: 0x06003DCB RID: 15819 RVA: 0x002F7080 File Offset: 0x002F5280
			public static bool LOCATION_IS_MINES(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return location is MineShaft;
			}

			// Token: 0x06003DCC RID: 15820 RVA: 0x002F70B4 File Offset: 0x002F52B4
			public static bool LOCATION_IS_SKULL_CAVE(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				MineShaft shaft = location as MineShaft;
				return shaft != null && shaft.mineLevel >= 121 && shaft.mineLevel != 77377;
			}

			// Token: 0x06003DCD RID: 15821 RVA: 0x002F7104 File Offset: 0x002F5304
			public static bool LOCATION_NAME(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				string text;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error) || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return location != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawName) => new bool?(rawName.EqualsIgnoreCase(location.Name)));
			}

			// Token: 0x06003DCE RID: 15822 RVA: 0x002F7170 File Offset: 0x002F5370
			public static bool LOCATION_UNIQUE_NAME(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				string text;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error) || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return location != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawName) => new bool?(rawName.EqualsIgnoreCase(location.NameOrUniqueName)));
			}

			// Token: 0x06003DCF RID: 15823 RVA: 0x002F71DC File Offset: 0x002F53DC
			public static bool LOCATION_SEASON(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string season = Game1.GetSeasonKeyForLocation(location);
				return GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawSeason) => new bool?(rawSeason.EqualsIgnoreCase(season)));
			}

			// Token: 0x06003DD0 RID: 15824 RVA: 0x002F722C File Offset: 0x002F542C
			public static bool MUSEUM_DONATIONS(string[] query, GameStateQueryContext context)
			{
				int filterIndex = 3;
				int minCount;
				string error;
				if (!ArgUtility.TryGetInt(query, 1, out minCount, out error, "int minCount"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int maxCount;
				string text;
				if (!ArgUtility.TryGetInt(query, 2, out maxCount, out text, "int maxCount"))
				{
					filterIndex = 2;
					maxCount = int.MaxValue;
				}
				bool filtered = query.Length > filterIndex;
				int count = 0;
				foreach (string itemId in Game1.netWorldState.Value.MuseumPieces.Values)
				{
					if (filtered)
					{
						ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
						if (data.ObjectType != null)
						{
							for (int i = filterIndex; i < query.Length; i++)
							{
								if (data.ObjectType == query[i])
								{
									count++;
									break;
								}
							}
						}
					}
					else
					{
						count++;
					}
				}
				return count >= minCount && count <= maxCount;
			}

			// Token: 0x06003DD1 RID: 15825 RVA: 0x002F7328 File Offset: 0x002F5528
			public static bool WEATHER(string[] query, GameStateQueryContext context)
			{
				GameLocation location = context.Location;
				string error;
				string text;
				if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out error) || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				if (location != null)
				{
					string weather = location.GetWeather().Weather;
					return GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawWeather) => new bool?(rawWeather.EqualsIgnoreCase(weather)));
				}
				return false;
			}

			// Token: 0x06003DD2 RID: 15826 RVA: 0x002F7394 File Offset: 0x002F5594
			public static bool WORLD_STATE_FIELD(string[] query, GameStateQueryContext context)
			{
				string name;
				string error;
				string expectedValue;
				int maxValue;
				if (!ArgUtility.TryGet(query, 1, out name, out error, true, "string name") || !ArgUtility.TryGet(query, 2, out expectedValue, out error, true, "string expectedValue") || !ArgUtility.TryGetOptionalInt(query, 3, out maxValue, out error, 2147483647, "int maxValue"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				PropertyInfo property = typeof(NetWorldState).GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
				if (property == null)
				{
					return false;
				}
				object actualValue = property.GetValue(Game1.netWorldState.Value, null);
				if (actualValue == null)
				{
					return expectedValue.EqualsIgnoreCase("null");
				}
				if (actualValue is bool)
				{
					bool actualBool = (bool)actualValue;
					bool expectedBool;
					return bool.TryParse(expectedValue, out expectedBool) && actualBool == expectedBool;
				}
				if (actualValue is int)
				{
					int actualInt = (int)actualValue;
					int minValue;
					return int.TryParse(expectedValue, out minValue) && actualInt >= minValue && actualInt <= maxValue;
				}
				string actualStr = actualValue as string;
				if (actualStr == null)
				{
					return actualValue.ToString().EqualsIgnoreCase(expectedValue);
				}
				return actualStr.EqualsIgnoreCase(expectedValue);
			}

			// Token: 0x06003DD3 RID: 15827 RVA: 0x002F74A0 File Offset: 0x002F56A0
			public static bool WORLD_STATE_ID(string[] query, GameStateQueryContext context)
			{
				string text;
				string error;
				if (!ArgUtility.TryGet(query, 1, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.AnyArgMatches(query, 1, (string worldStateId) => new bool?(NetWorldState.checkAnywhereForWorldStateID(worldStateId)));
			}

			// Token: 0x06003DD4 RID: 15828 RVA: 0x002F74F0 File Offset: 0x002F56F0
			public static bool MINE_LOWEST_LEVEL_REACHED(string[] query, GameStateQueryContext context)
			{
				int minLevel;
				string error;
				int maxLevel;
				if (!ArgUtility.TryGetInt(query, 1, out minLevel, out error, "int minLevel") || !ArgUtility.TryGetOptionalInt(query, 2, out maxLevel, out error, 2147483647, "int maxLevel"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int level = MineShaft.lowestLevelReached;
				return level >= minLevel && level <= maxLevel;
			}

			// Token: 0x06003DD5 RID: 15829 RVA: 0x002F7542 File Offset: 0x002F5742
			public static bool PLAYER_BASE_COMBAT_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.combatLevel.Value);
			}

			// Token: 0x06003DD6 RID: 15830 RVA: 0x002F756F File Offset: 0x002F576F
			public static bool PLAYER_BASE_FARMING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.farmingLevel.Value);
			}

			// Token: 0x06003DD7 RID: 15831 RVA: 0x002F759C File Offset: 0x002F579C
			public static bool PLAYER_BASE_FISHING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.fishingLevel.Value);
			}

			// Token: 0x06003DD8 RID: 15832 RVA: 0x002F75C9 File Offset: 0x002F57C9
			public static bool PLAYER_BASE_FORAGING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.foragingLevel.Value);
			}

			// Token: 0x06003DD9 RID: 15833 RVA: 0x002F75F6 File Offset: 0x002F57F6
			public static bool PLAYER_BASE_LUCK_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.luckLevel.Value);
			}

			// Token: 0x06003DDA RID: 15834 RVA: 0x002F7623 File Offset: 0x002F5823
			public static bool PLAYER_BASE_MINING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.miningLevel.Value);
			}

			// Token: 0x06003DDB RID: 15835 RVA: 0x002F7650 File Offset: 0x002F5850
			public static bool PLAYER_COMBAT_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.CombatLevel);
			}

			// Token: 0x06003DDC RID: 15836 RVA: 0x002F767D File Offset: 0x002F587D
			public static bool PLAYER_FARMING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.FarmingLevel);
			}

			// Token: 0x06003DDD RID: 15837 RVA: 0x002F76AA File Offset: 0x002F58AA
			public static bool PLAYER_FISHING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.FishingLevel);
			}

			// Token: 0x06003DDE RID: 15838 RVA: 0x002F76D7 File Offset: 0x002F58D7
			public static bool PLAYER_FORAGING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.ForagingLevel);
			}

			// Token: 0x06003DDF RID: 15839 RVA: 0x002F7704 File Offset: 0x002F5904
			public static bool PLAYER_LUCK_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.LuckLevel);
			}

			// Token: 0x06003DE0 RID: 15840 RVA: 0x002F7731 File Offset: 0x002F5931
			public static bool PLAYER_MINING_LEVEL(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.PlayerSkillLevelImpl(query, context.Player, (Farmer target) => target.MiningLevel);
			}

			// Token: 0x06003DE1 RID: 15841 RVA: 0x002F7760 File Offset: 0x002F5960
			public static bool PLAYER_CURRENT_MONEY(string[] query, GameStateQueryContext context)
			{
				int minAmount;
				int maxAmount;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out minAmount, out error, "int minAmount") || !ArgUtility.TryGetOptionalInt(query, 3, out maxAmount, out error, 2147483647, "int maxAmount"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int amount = target.Money;
					return amount >= minAmount && amount <= maxAmount;
				});
			}

			// Token: 0x06003DE2 RID: 15842 RVA: 0x002F77D8 File Offset: 0x002F59D8
			public static bool PLAYER_FARMHOUSE_UPGRADE(string[] query, GameStateQueryContext context)
			{
				int minUpgradeLevel;
				int maxUpgradeLevel;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out minUpgradeLevel, out error, "int minUpgradeLevel") || !ArgUtility.TryGetOptionalInt(query, 3, out maxUpgradeLevel, out error, 2147483647, "int maxUpgradeLevel"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int level = target.HouseUpgradeLevel;
					return level >= minUpgradeLevel && level <= maxUpgradeLevel;
				});
			}

			// Token: 0x06003DE3 RID: 15843 RVA: 0x002F7850 File Offset: 0x002F5A50
			public static bool PLAYER_GENDER(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string genderName;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out genderName, out error, true, "string genderName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool isMale = genderName.EqualsIgnoreCase("Male");
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.IsMale == isMale);
			}

			// Token: 0x06003DE4 RID: 15844 RVA: 0x002F78BC File Offset: 0x002F5ABC
			public static bool PLAYER_HAS_ACHIEVEMENT(string[] query, GameStateQueryContext context)
			{
				int achievementId;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out achievementId, out error, "int achievementId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.achievements.Contains(achievementId));
			}

			// Token: 0x06003DE5 RID: 15845 RVA: 0x002F791C File Offset: 0x002F5B1C
			public static bool PLAYER_HAS_ALL_ACHIEVEMENTS(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					foreach (int key in Game1.achievements.Keys)
					{
						if (!target.achievements.Contains(key))
						{
							return false;
						}
					}
					return true;
				});
			}

			// Token: 0x06003DE6 RID: 15846 RVA: 0x002F7970 File Offset: 0x002F5B70
			public static bool PLAYER_HAS_BUFF(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string buffId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out buffId, out error, true, "string buffId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.buffs.IsApplied(id))));
			}

			// Token: 0x06003DE7 RID: 15847 RVA: 0x002F79E0 File Offset: 0x002F5BE0
			public static bool PLAYER_HAS_CAUGHT_FISH(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string fishId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out fishId, out error, true, "string fishId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				fishId = ItemRegistry.QualifyItemId(fishId);
				return fishId != null && GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.fishCaught.ContainsKey(id))));
			}

			// Token: 0x06003DE8 RID: 15848 RVA: 0x002F7A5C File Offset: 0x002F5C5C
			public static bool PLAYER_HAS_CONVERSATION_TOPIC(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string topic;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out topic, out error, true, "string topic"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.activeDialogueEvents.ContainsKey(id))));
			}

			// Token: 0x06003DE9 RID: 15849 RVA: 0x002F7ACC File Offset: 0x002F5CCC
			public static bool PLAYER_HAS_CRAFTING_RECIPE(string[] query, GameStateQueryContext context)
			{
				string recipeName;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetRemainder(query, 2, out recipeName, out error, ' ', "string recipeName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.craftingRecipes.ContainsKey(recipeName));
			}

			// Token: 0x06003DEA RID: 15850 RVA: 0x002F7B2C File Offset: 0x002F5D2C
			public static bool PLAYER_HAS_COOKING_RECIPE(string[] query, GameStateQueryContext context)
			{
				string recipeName;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetRemainder(query, 2, out recipeName, out error, ' ', "string recipeName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.cookingRecipes.ContainsKey(recipeName));
			}

			// Token: 0x06003DEB RID: 15851 RVA: 0x002F7B8C File Offset: 0x002F5D8C
			public static bool PLAYER_HAS_DIALOGUE_ANSWER(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string responseId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out responseId, out error, true, "string responseId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.DialogueQuestionsAnswered.Contains(id))));
			}

			// Token: 0x06003DEC RID: 15852 RVA: 0x002F7BFC File Offset: 0x002F5DFC
			public static bool PLAYER_HAS_HEARD_SONG(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string songId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out songId, out error, true, "string songId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.songsHeard.Contains(id))));
			}

			// Token: 0x06003DED RID: 15853 RVA: 0x002F7C6C File Offset: 0x002F5E6C
			public static bool PLAYER_HAS_ITEM(string[] query, GameStateQueryContext context)
			{
				string itemId;
				int minCount;
				int maxCount;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(query, 3, out minCount, out error, 1, "int minCount") || !ArgUtility.TryGetOptionalInt(query, 4, out maxCount, out error, 2147483647, "int maxCount"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					string itemId = itemId;
					if (itemId == "73" || itemId == "(O)73")
					{
						int count = Game1.netWorldState.Value.GoldenWalnuts;
						return count >= minCount && count <= maxCount;
					}
					if (itemId == "858" || itemId == "(O)858")
					{
						int count2 = target.QiGems;
						return count2 >= minCount && count2 <= maxCount;
					}
					if (maxCount != 2147483647)
					{
						int count3 = target.Items.CountId(itemId);
						return count3 >= minCount && count3 <= maxCount;
					}
					return target.Items.ContainsId(itemId, minCount);
				});
			}

			// Token: 0x06003DEE RID: 15854 RVA: 0x002F7CFC File Offset: 0x002F5EFC
			public static bool PLAYER_HAS_MAIL(string[] query, GameStateQueryContext context)
			{
				string mailId;
				string playerKey;
				string error;
				string rawType;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out mailId, out error, true, "string mailId") || !ArgUtility.TryGetOptional(query, 3, out rawType, out error, "any", true, "string rawType"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string type = (rawType != null) ? rawType.ToLower() : null;
				string type3 = type;
				if (!(type3 == "mailbox") && !(type3 == "tomorrow") && !(type3 == "received") && !(type3 == "any"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, "unknown mail type '" + type + "'; expected 'Mailbox', 'Tomorrow', 'Received', or 'Any'", null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					string type2 = type;
					if (type2 == "mailbox")
					{
						return target.mailbox.Contains(mailId);
					}
					if (type2 == "tomorrow")
					{
						return target.mailForTomorrow.Contains(mailId);
					}
					if (!(type2 == "received"))
					{
						return target.hasOrWillReceiveMail(mailId);
					}
					return target.mailReceived.Contains(mailId);
				});
			}

			// Token: 0x06003DEF RID: 15855 RVA: 0x002F7DE4 File Offset: 0x002F5FE4
			public static bool PLAYER_HAS_PROFESSION(string[] query, GameStateQueryContext context)
			{
				int professionId;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out professionId, out error, "int professionId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.professions.Contains(professionId));
			}

			// Token: 0x06003DF0 RID: 15856 RVA: 0x002F7E44 File Offset: 0x002F6044
			public static bool PLAYER_HAS_RUN_TRIGGER_ACTION(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string actionId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out actionId, out error, true, "string actionId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.triggerActionsRun.Contains(id))));
			}

			// Token: 0x06003DF1 RID: 15857 RVA: 0x002F7EB4 File Offset: 0x002F60B4
			public static bool PLAYER_HAS_SECRET_NOTE(string[] query, GameStateQueryContext context)
			{
				int noteId;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out noteId, out error, "int noteId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.secretNotesSeen.Contains(noteId));
			}

			// Token: 0x06003DF2 RID: 15858 RVA: 0x002F7F14 File Offset: 0x002F6114
			public static bool PLAYER_HAS_SEEN_EVENT(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string eventId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out eventId, out error, true, "string eventId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.eventsSeen.Contains(id))));
			}

			// Token: 0x06003DF3 RID: 15859 RVA: 0x002F7F84 File Offset: 0x002F6184
			public static bool PLAYER_HAS_TOWN_KEY(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.HasTownKey);
			}

			// Token: 0x06003DF4 RID: 15860 RVA: 0x002F7FD8 File Offset: 0x002F61D8
			public static bool PLAYER_HAS_TRASH_CAN_LEVEL(string[] query, GameStateQueryContext context)
			{
				int minLevel;
				int maxLevel;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out minLevel, out error, "int minLevel") || !ArgUtility.TryGetOptionalInt(query, 3, out maxLevel, out error, 2147483647, "int maxLevel"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int level = target.trashCanLevel;
					return level >= minLevel && level <= maxLevel;
				});
			}

			// Token: 0x06003DF5 RID: 15861 RVA: 0x002F8050 File Offset: 0x002F6250
			public static bool PLAYER_HAS_TRINKET(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					foreach (Trinket trinket in target.trinketItems)
					{
						if (trinket != null)
						{
							for (int i = 2; i < query.Length; i++)
							{
								if (trinket.QualifiedItemId == query[i] || trinket.ItemId == query[i])
								{
									return true;
								}
							}
						}
					}
					return false;
				});
			}

			// Token: 0x06003DF6 RID: 15862 RVA: 0x002F80A8 File Offset: 0x002F62A8
			public static bool PLAYER_LOCATION_CONTEXT(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string text;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					GameStateQuery.DefaultResolvers.<>c__DisplayClass77_1 CS$<>8__locals2 = new GameStateQuery.DefaultResolvers.<>c__DisplayClass77_1();
					GameStateQuery.DefaultResolvers.<>c__DisplayClass77_1 CS$<>8__locals3 = CS$<>8__locals2;
					GameLocation currentLocation = target.currentLocation;
					CS$<>8__locals3.contextId = ((currentLocation != null) ? currentLocation.GetLocationContextId() : null);
					return GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawContextId) => new bool?(rawContextId.EqualsIgnoreCase(CS$<>8__locals2.contextId)));
				});
			}

			// Token: 0x06003DF7 RID: 15863 RVA: 0x002F8118 File Offset: 0x002F6318
			public static bool PLAYER_LOCATION_NAME(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string text;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, delegate(string rawName)
				{
					GameLocation currentLocation = target.currentLocation;
					return new bool?(rawName.EqualsIgnoreCase((currentLocation != null) ? currentLocation.Name : null));
				}));
			}

			// Token: 0x06003DF8 RID: 15864 RVA: 0x002F8188 File Offset: 0x002F6388
			public static bool PLAYER_LOCATION_UNIQUE_NAME(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string text;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, delegate(string rawName)
				{
					GameLocation currentLocation = target.currentLocation;
					return new bool?(rawName.EqualsIgnoreCase((currentLocation != null) ? currentLocation.NameOrUniqueName : null));
				}));
			}

			// Token: 0x06003DF9 RID: 15865 RVA: 0x002F81F8 File Offset: 0x002F63F8
			public static bool PLAYER_MOD_DATA(string[] query, GameStateQueryContext context)
			{
				string key;
				string value;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out key, out error, true, "string key") || !ArgUtility.TryGet(query, 3, out value, out error, true, "string value"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					string curValue;
					return target.modData.TryGetValue(key, out curValue) && curValue.EqualsIgnoreCase(value);
				});
			}

			// Token: 0x06003DFA RID: 15866 RVA: 0x002F8270 File Offset: 0x002F6470
			public static bool PLAYER_MONEY_EARNED(string[] query, GameStateQueryContext context)
			{
				int minAmount;
				int maxAmount;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetInt(query, 2, out minAmount, out error, "int minAmount") || !ArgUtility.TryGetOptionalInt(query, 3, out maxAmount, out error, 2147483647, "int maxAmount"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					uint money = target.totalMoneyEarned;
					return (ulong)money >= (ulong)((long)minAmount) && (ulong)money <= (ulong)((long)maxAmount);
				});
			}

			// Token: 0x06003DFB RID: 15867 RVA: 0x002F82E8 File Offset: 0x002F64E8
			public static bool PLAYER_SHIPPED_BASIC_ITEM(string[] query, GameStateQueryContext context)
			{
				string itemId;
				int minShipped;
				int maxShipped;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(query, 3, out minShipped, out error, 1, "int minShipped") || !ArgUtility.TryGetOptionalInt(query, 4, out maxShipped, out error, 2147483647, "int maxShipped"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				if (ItemRegistry.IsQualifiedItemId(itemId))
				{
					ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
					if (((metadata != null) ? metadata.TypeIdentifier : null) != "(O)")
					{
						return false;
					}
					itemId = metadata.LocalItemId;
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int shipped;
					return target.basicShipped.TryGetValue(itemId, out shipped) && shipped >= minShipped && shipped <= maxShipped;
				});
			}

			// Token: 0x06003DFC RID: 15868 RVA: 0x002F83B8 File Offset: 0x002F65B8
			public static bool PLAYER_SPECIAL_ORDER_ACTIVE(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string orderId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out orderId, out error, true, "string orderId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.team.SpecialOrderActive(id))));
			}

			// Token: 0x06003DFD RID: 15869 RVA: 0x002F8428 File Offset: 0x002F6628
			public static bool PLAYER_SPECIAL_ORDER_RULE_ACTIVE(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string ruleId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out ruleId, out error, true, "string ruleId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.team.SpecialOrderRuleActive(id, null))));
			}

			// Token: 0x06003DFE RID: 15870 RVA: 0x002F8498 File Offset: 0x002F6698
			public static bool PLAYER_SPECIAL_ORDER_COMPLETE(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string orderId;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out orderId, out error, true, "string orderId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string id) => new bool?(target.team.completedSpecialOrders.Contains(id))));
			}

			// Token: 0x06003DFF RID: 15871 RVA: 0x002F8508 File Offset: 0x002F6708
			public static bool PLAYER_KILLED_MONSTERS(string[] query, GameStateQueryContext context)
			{
				List<string> monsterNames = new List<string>();
				int min = 1;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "playerKey"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int argIndex = 2;
				while (argIndex < query.Length)
				{
					string name = query[argIndex];
					argIndex++;
					int rawMin;
					if (int.TryParse(name, out rawMin))
					{
						min = rawMin;
						break;
					}
					monsterNames.Add(name);
				}
				int max;
				if (!ArgUtility.TryGetOptionalInt(query, argIndex, out max, out error, 2147483647, "max"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				if (monsterNames.Count == 0)
				{
					return GameStateQuery.Helpers.ErrorResult(query, "must specify at least one monster name to count", null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int count = 0;
					foreach (string name2 in monsterNames)
					{
						count += target.stats.getMonstersKilled(name2);
					}
					return count >= min && count <= max;
				});
			}

			// Token: 0x06003E00 RID: 15872 RVA: 0x002F85D4 File Offset: 0x002F67D4
			public static bool PLAYER_STAT(string[] query, GameStateQueryContext context)
			{
				string statName;
				int minValue;
				int maxValue;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out statName, out error, true, "string statName") || !ArgUtility.TryGetInt(query, 3, out minValue, out error, "int minValue") || !ArgUtility.TryGetOptionalInt(query, 4, out maxValue, out error, 2147483647, "int maxValue"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					uint value = target.stats.Get(statName);
					return (ulong)value >= (ulong)((long)minValue) && (ulong)value <= (ulong)((long)maxValue);
				});
			}

			// Token: 0x06003E01 RID: 15873 RVA: 0x002F8664 File Offset: 0x002F6864
			public static bool PLAYER_VISITED_LOCATION(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string text;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string locationName) => new bool?(target.locationsVisited.Contains(locationName))));
			}

			// Token: 0x06003E02 RID: 15874 RVA: 0x002F86D4 File Offset: 0x002F68D4
			public static bool PLAYER_FRIENDSHIP_POINTS(string[] query, GameStateQueryContext context)
			{
				int minPoints;
				int maxPoints;
				string npcName;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out npcName, out error, true, "string npcName") || !ArgUtility.TryGetInt(query, 3, out minPoints, out error, "int minPoints") || !ArgUtility.TryGetOptionalInt(query, 4, out maxPoints, out error, 2147483647, "int maxPoints"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool isAny = npcName.EqualsIgnoreCase("Any");
				bool isAnyDateable = !isAny && npcName.EqualsIgnoreCase("AnyDateable");
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					if (isAny)
					{
						return target.hasAFriendWithFriendshipPoints(minPoints, false, maxPoints);
					}
					if (isAnyDateable)
					{
						return target.hasAFriendWithFriendshipPoints(minPoints, true, maxPoints);
					}
					int points = target.getFriendshipLevelForNPC(npcName);
					return points >= minPoints && points <= maxPoints;
				});
			}

			// Token: 0x06003E03 RID: 15875 RVA: 0x002F879C File Offset: 0x002F699C
			public static bool PLAYER_HAS_CHILDREN(string[] query, GameStateQueryContext context)
			{
				int minCount;
				int maxCount;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGetOptionalInt(query, 2, out minCount, out error, 1, "int minCount") || !ArgUtility.TryGetOptionalInt(query, 3, out maxCount, out error, 2147483647, "int maxCount"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					int count = target.getChildrenCount();
					return count >= minCount && count <= maxCount;
				});
			}

			// Token: 0x06003E04 RID: 15876 RVA: 0x002F8818 File Offset: 0x002F6A18
			public static bool PLAYER_HAS_PET(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => target.hasPet());
			}

			// Token: 0x06003E05 RID: 15877 RVA: 0x002F886C File Offset: 0x002F6A6C
			public static bool PLAYER_HEARTS(string[] query, GameStateQueryContext context)
			{
				int minHearts;
				int maxHearts;
				string npcName;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out npcName, out error, true, "string npcName") || !ArgUtility.TryGetInt(query, 3, out minHearts, out error, "int minHearts") || !ArgUtility.TryGetOptionalInt(query, 4, out maxHearts, out error, 2147483647, "int maxHearts"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				bool isAny = npcName.EqualsIgnoreCase("Any");
				bool isAnyDateable = !isAny && npcName.EqualsIgnoreCase("AnyDateable");
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					if (isAny)
					{
						return target.hasAFriendWithHeartLevel(minHearts, false, maxHearts);
					}
					if (isAnyDateable)
					{
						return target.hasAFriendWithHeartLevel(minHearts, true, maxHearts);
					}
					int hearts = target.getFriendshipHeartLevelForNPC(npcName);
					return hearts >= minHearts && hearts <= maxHearts;
				});
			}

			// Token: 0x06003E06 RID: 15878 RVA: 0x002F8934 File Offset: 0x002F6B34
			public static bool PLAYER_HAS_MET(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string npcName;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out npcName, out error, true, "string npcName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string name) => new bool?(target.friendshipData.ContainsKey(name))));
			}

			// Token: 0x06003E07 RID: 15879 RVA: 0x002F89A4 File Offset: 0x002F6BA4
			public static bool PLAYER_NPC_RELATIONSHIP(string[] query, GameStateQueryContext context)
			{
				string npcName;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, false, "string playerKey") || !ArgUtility.TryGet(query, 2, out npcName, out error, false, "string npcName"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string[] relationships = new string[query.Length - 3];
				int i = 3;
				string type;
				while (i < query.Length && ArgUtility.TryGet(query, i, out type, out error, false, "string type"))
				{
					type = type.ToLower();
					relationships[i - 3] = type;
					if (!(type == "friendly") && !(type == "roommate") && !(type == "dating") && !(type == "engaged") && !(type == "married") && !(type == "divorced"))
					{
						return GameStateQuery.Helpers.ErrorResult(query, "unknown relationship type '" + type + "'; expected one of Friendly, Roommate, Dating, Engaged, Married, or Divorced", null);
					}
					i++;
				}
				if (relationships.Length == 0)
				{
					return GameStateQuery.Helpers.ErrorResult(query, ArgUtility.GetMissingRequiredIndexError(query, 3, "type"), null);
				}
				bool anyNpc = npcName.EqualsIgnoreCase("Any");
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, delegate(Farmer target)
				{
					if (anyNpc)
					{
						using (NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>.ValuesCollection.Enumerator enumerator = target.friendshipData.Values.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Friendship friendship = enumerator.Current;
								if (base.<PLAYER_NPC_RELATIONSHIP>g__IsMatch|0(friendship, relationships))
								{
									return true;
								}
							}
							return false;
						}
					}
					Friendship friendship2;
					if (target.friendshipData.TryGetValue(npcName, out friendship2) && base.<PLAYER_NPC_RELATIONSHIP>g__IsMatch|0(friendship2, relationships))
					{
						return true;
					}
					return false;
				});
			}

			// Token: 0x06003E08 RID: 15880 RVA: 0x002F8B28 File Offset: 0x002F6D28
			public static bool PLAYER_PLAYER_RELATIONSHIP(string[] query, GameStateQueryContext context)
			{
				string targetPlayerKey;
				string type;
				string playerKey;
				string error;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, false, "string playerKey") || !ArgUtility.TryGet(query, 2, out targetPlayerKey, out error, false, "string targetPlayerKey") || !ArgUtility.TryGet(query, 3, out type, out error, false, "string type"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				type = type.ToLower();
				string type3 = type;
				if (!(type3 == "friendly") && !(type3 == "engaged") && !(type3 == "married"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, "unknown relationship type '" + type + "'; expected one of Friendly, Engaged, or Married", null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer fromPlayer) => GameStateQuery.Helpers.WithPlayer(context.Player, targetPlayerKey, delegate(Farmer toPlayer)
				{
					FriendshipStatus status = fromPlayer.team.GetFriendship(fromPlayer.UniqueMultiplayerID, toPlayer.UniqueMultiplayerID).Status;
					string type2 = type;
					if (type2 == "friendly")
					{
						return status != FriendshipStatus.Engaged && status != FriendshipStatus.Married;
					}
					if (type2 == "engaged")
					{
						return status == FriendshipStatus.Engaged;
					}
					if (!(type2 == "married"))
					{
						return GameStateQuery.Helpers.ErrorResult(query, "unhandled relationship type '" + type + "'", null);
					}
					return status == FriendshipStatus.Married;
				}));
			}

			// Token: 0x06003E09 RID: 15881 RVA: 0x002F8C28 File Offset: 0x002F6E28
			public static bool PLAYER_PREFERRED_PET(string[] query, GameStateQueryContext context)
			{
				string playerKey;
				string error;
				string text;
				if (!ArgUtility.TryGet(query, 1, out playerKey, out error, true, "string playerKey") || !ArgUtility.TryGet(query, 2, out text, out error, true, "_"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (Farmer target) => GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawPetId) => new bool?(rawPetId.EqualsIgnoreCase(target.whichPetType))));
			}

			// Token: 0x06003E0A RID: 15882 RVA: 0x002F8C98 File Offset: 0x002F6E98
			public static bool RANDOM(string[] query, GameStateQueryContext context)
			{
				return GameStateQuery.Helpers.RandomImpl(context.Random, query, 1);
			}

			// Token: 0x06003E0B RID: 15883 RVA: 0x002F8CA8 File Offset: 0x002F6EA8
			public static bool SYNCED_CHOICE(string[] query, GameStateQueryContext context)
			{
				string interval;
				string error;
				string key;
				int min;
				int max;
				Random syncedRandom;
				if (!ArgUtility.TryGet(query, 1, out interval, out error, true, "string interval") || !ArgUtility.TryGet(query, 2, out key, out error, true, "string key") || !ArgUtility.TryGetInt(query, 3, out min, out error, "int min") || !ArgUtility.TryGetInt(query, 4, out max, out error, "int max") || !Utility.TryCreateIntervalRandom(interval, key, out syncedRandom, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				string selected = syncedRandom.Next(min, max + 1).ToString();
				for (int i = 5; i < query.Length; i++)
				{
					if (query[i] == selected)
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003E0C RID: 15884 RVA: 0x002F8D50 File Offset: 0x002F6F50
			public static bool SYNCED_RANDOM(string[] query, GameStateQueryContext context)
			{
				string interval;
				string error;
				string key;
				Random syncedRandom;
				if (!ArgUtility.TryGet(query, 1, out interval, out error, true, "string interval") || !ArgUtility.TryGet(query, 2, out key, out error, true, "string key") || !Utility.TryCreateIntervalRandom(interval, key, out syncedRandom, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return GameStateQuery.Helpers.RandomImpl(syncedRandom, query, 3);
			}

			// Token: 0x06003E0D RID: 15885 RVA: 0x002F8DA4 File Offset: 0x002F6FA4
			public static bool SYNCED_SUMMER_RAIN_RANDOM(string[] query, GameStateQueryContext context)
			{
				Random random = Utility.CreateDaySaveRandom((double)Game1.hash.GetDeterministicHashCode("summer_rain_chance"), 0.0, 0.0);
				float chanceToRain = 0.12f + (float)Game1.dayOfMonth * 0.003f;
				return random.NextBool(chanceToRain);
			}

			// Token: 0x06003E0E RID: 15886 RVA: 0x002F8DF4 File Offset: 0x002F6FF4
			public static bool ITEM_CONTEXT_TAG(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				if (item == null)
				{
					return false;
				}
				for (int i = 2; i < query.Length; i++)
				{
					if (!item.HasContextTag(query[i]))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x06003E0F RID: 15887 RVA: 0x002F8E44 File Offset: 0x002F7044
			public static bool ITEM_CATEGORY(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				if (item != null)
				{
					if (query.Length == 2)
					{
						return item.Category < -1;
					}
					for (int i = 2; i < query.Length; i++)
					{
						int category;
						if (!ArgUtility.TryGetInt(query, i, out category, out error, "int category"))
						{
							return GameStateQuery.Helpers.ErrorResult(query, error, null);
						}
						if (item.Category == category)
						{
							return true;
						}
					}
				}
				return false;
			}

			// Token: 0x06003E10 RID: 15888 RVA: 0x002F8EBC File Offset: 0x002F70BC
			public static bool ITEM_HAS_EXPLICIT_OBJECT_CATEGORY(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return ObjectDataDefinition.HasExplicitCategory(ItemRegistry.GetData((item != null) ? item.QualifiedItemId : null));
			}

			// Token: 0x06003E11 RID: 15889 RVA: 0x002F8F04 File Offset: 0x002F7104
			public static bool ITEM_ID(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return item != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawItemId) => new bool?(rawItemId.EqualsIgnoreCase(item.QualifiedItemId) || rawItemId.EqualsIgnoreCase(item.ItemId)));
			}

			// Token: 0x06003E12 RID: 15890 RVA: 0x002F8F5C File Offset: 0x002F715C
			public static bool ITEM_ID_PREFIX(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return item != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string prefix) => new bool?(item.ItemId.StartsWithIgnoreCase(prefix) || item.QualifiedItemId.StartsWithIgnoreCase(prefix)));
			}

			// Token: 0x06003E13 RID: 15891 RVA: 0x002F8FB4 File Offset: 0x002F71B4
			public static bool ITEM_NUMERIC_ID(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				int minId;
				int maxId;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error) || !ArgUtility.TryGetInt(query, 2, out minId, out error, "int minId") || !ArgUtility.TryGetOptionalInt(query, 3, out maxId, out error, 2147483647, "int maxId"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int id;
				return int.TryParse((item != null) ? item.ItemId : null, out id) && id >= minId && id <= maxId;
			}

			// Token: 0x06003E14 RID: 15892 RVA: 0x002F9030 File Offset: 0x002F7230
			public static bool ITEM_OBJECT_TYPE(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				Object obj = item as Object;
				return obj != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawObjType) => new bool?(rawObjType.EqualsIgnoreCase(obj.Type)));
			}

			// Token: 0x06003E15 RID: 15893 RVA: 0x002F9090 File Offset: 0x002F7290
			public static bool ITEM_PRICE(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				int minPrice;
				int maxPrice;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error) || !ArgUtility.TryGetInt(query, 2, out minPrice, out error, "int minPrice") || !ArgUtility.TryGetOptionalInt(query, 3, out maxPrice, out error, 2147483647, "int maxPrice"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int? price = (item != null) ? new int?(item.salePrice(false)) : null;
				int? num = price;
				int num2 = minPrice;
				if (num.GetValueOrDefault() >= num2 & num != null)
				{
					num = price;
					num2 = maxPrice;
					return num.GetValueOrDefault() <= num2 & num != null;
				}
				return false;
			}

			// Token: 0x06003E16 RID: 15894 RVA: 0x002F9144 File Offset: 0x002F7344
			public static bool ITEM_QUALITY(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				int minQuality;
				int maxQuality;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error) || !ArgUtility.TryGetInt(query, 2, out minQuality, out error, "int minQuality") || !ArgUtility.TryGetOptionalInt(query, 3, out maxQuality, out error, 2147483647, "int maxQuality"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int? quality = (item != null) ? new int?(item.Quality) : null;
				int? num = quality;
				int num2 = minQuality;
				if (num.GetValueOrDefault() >= num2 & num != null)
				{
					num = quality;
					num2 = maxQuality;
					return num.GetValueOrDefault() <= num2 & num != null;
				}
				return false;
			}

			// Token: 0x06003E17 RID: 15895 RVA: 0x002F91F8 File Offset: 0x002F73F8
			public static bool ITEM_STACK(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				int minStack;
				int maxStack;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error) || !ArgUtility.TryGetInt(query, 2, out minStack, out error, "int minStack") || !ArgUtility.TryGetOptionalInt(query, 3, out maxStack, out error, 2147483647, "int maxStack"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				int? stack = (item != null) ? new int?(item.Stack) : null;
				int? num = stack;
				int num2 = minStack;
				if (num.GetValueOrDefault() >= num2 & num != null)
				{
					num = stack;
					num2 = maxStack;
					return num.GetValueOrDefault() <= num2 & num != null;
				}
				return false;
			}

			// Token: 0x06003E18 RID: 15896 RVA: 0x002F92AC File Offset: 0x002F74AC
			public static bool ITEM_TYPE(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				return item != null && GameStateQuery.Helpers.AnyArgMatches(query, 2, (string rawItemType) => new bool?(rawItemType.EqualsIgnoreCase(item.TypeDefinitionId)));
			}

			// Token: 0x06003E19 RID: 15897 RVA: 0x002F9304 File Offset: 0x002F7504
			public static bool ITEM_EDIBILITY(string[] query, GameStateQueryContext context)
			{
				Item item;
				string error;
				int minEdibility;
				int maxEdibility;
				if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, context.TargetItem, context.InputItem, out item, out error) || !ArgUtility.TryGetOptionalInt(query, 2, out minEdibility, out error, -299, "int minEdibility") || !ArgUtility.TryGetOptionalInt(query, 3, out maxEdibility, out error, 2147483647, "int maxEdibility"))
				{
					return GameStateQuery.Helpers.ErrorResult(query, error, null);
				}
				Object obj = item as Object;
				return obj != null && obj.Edibility >= minEdibility && obj.Edibility <= maxEdibility;
			}

			// Token: 0x06003E1A RID: 15898 RVA: 0x002F9386 File Offset: 0x002F7586
			public static bool TRUE(string[] query, GameStateQueryContext context)
			{
				return true;
			}

			// Token: 0x06003E1B RID: 15899 RVA: 0x002F9389 File Offset: 0x002F7589
			public static bool FALSE(string[] query, GameStateQueryContext context)
			{
				return false;
			}
		}
	}
}
