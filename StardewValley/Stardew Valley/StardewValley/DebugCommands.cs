using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.Compress;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using StardewValley.WorldMaps;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley
{
	// Token: 0x02000093 RID: 147
	public static class DebugCommands
	{
		// Token: 0x0600067A RID: 1658 RVA: 0x000244FC File Offset: 0x000226FC
		static DebugCommands()
		{
			MethodInfo[] methods = typeof(DebugCommands.DefaultHandlers).GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo method in methods)
			{
				try
				{
					DebugCommands.Handlers[method.Name] = (DebugCommandHandlerDelegate)Delegate.CreateDelegate(typeof(DebugCommandHandlerDelegate), method);
				}
				catch (Exception ex)
				{
					Game1.log.Error("Failed to initialize debug command " + method.Name + ".", ex);
				}
			}
			foreach (MethodInfo method2 in methods)
			{
				OtherNamesAttribute attribute = method2.GetCustomAttribute<OtherNamesAttribute>();
				if (attribute != null)
				{
					foreach (string alias in attribute.Aliases)
					{
						if (DebugCommands.Handlers.ContainsKey(alias))
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Can't register alias '");
							defaultInterpolatedStringHandler.AppendFormatted(alias);
							defaultInterpolatedStringHandler.AppendLiteral("' for debug command '");
							defaultInterpolatedStringHandler.AppendFormatted(method2.Name);
							defaultInterpolatedStringHandler.AppendLiteral("', because there's a command with that name.");
							log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						string conflictingName;
						if (DebugCommands.Aliases.TryGetValue(alias, out conflictingName))
						{
							IGameLogger log2 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Can't register alias '");
							defaultInterpolatedStringHandler.AppendFormatted(alias);
							defaultInterpolatedStringHandler.AppendLiteral("' for debug command '");
							defaultInterpolatedStringHandler.AppendFormatted(method2.Name);
							defaultInterpolatedStringHandler.AppendLiteral("', because that's already an alias for '");
							defaultInterpolatedStringHandler.AppendFormatted(conflictingName);
							defaultInterpolatedStringHandler.AppendLiteral("'.");
							log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						DebugCommands.Aliases[alias] = method2.Name;
					}
				}
			}
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x000246F4 File Offset: 0x000228F4
		public static bool TryHandle(string[] command, IGameLogger log = null)
		{
			if (log == null)
			{
				log = Game1.log;
			}
			string commandName = ArgUtility.Get(command, 0, null, true);
			if (string.IsNullOrWhiteSpace(commandName))
			{
				log.Error("Can't parse an empty command.", null);
				return false;
			}
			string aliasTarget;
			if (DebugCommands.Aliases.TryGetValue(commandName, out aliasTarget))
			{
				commandName = aliasTarget;
			}
			DebugCommandHandlerDelegate handler;
			if (!DebugCommands.Handlers.TryGetValue(commandName, out handler))
			{
				log.Error("Unknown debug command '" + commandName + "'.", null);
				string[] similar = DebugCommands.SearchCommandNames(commandName, true).Take(10).ToArray<string>();
				if (similar.Length != 0)
				{
					log.Info("Did you mean one of these?\n- " + string.Join("\n- ", similar));
				}
				return false;
			}
			bool result;
			try
			{
				handler(command, log);
				result = true;
			}
			catch (Exception ex)
			{
				log.Error("Error running debug command '" + string.Join(" ", command) + "'.", ex);
				result = false;
			}
			return result;
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x000247E0 File Offset: 0x000229E0
		public static List<string> SearchCommandNames(string search, bool displayAliases = true)
		{
			ILookup<string, string> aliasesByName = DebugCommands.Aliases.ToLookup((KeyValuePair<string, string> p) => p.Value, (KeyValuePair<string, string> p) => p.Key);
			List<string> commands = new List<string>();
			foreach (string name in DebugCommands.Handlers.Keys.OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase))
			{
				string[] aliases = aliasesByName[name].ToArray<string>();
				if (aliases.Length == 0)
				{
					commands.Add(name);
				}
				else if (displayAliases)
				{
					commands.Add(name + " (" + string.Join(", ", aliases.OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase)) + ")");
				}
				else
				{
					commands.Add("###" + name + "###" + string.Join(",", aliases));
				}
			}
			if (search != null)
			{
				commands.RemoveAll((string line) => Utility.fuzzyCompare(search, line) == null);
			}
			if (!displayAliases)
			{
				for (int i = 0; i < commands.Count; i++)
				{
					if (commands[i].StartsWith("###"))
					{
						commands[i] = commands[i].Split("###", 3, StringSplitOptions.None)[1];
					}
				}
			}
			return commands;
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x000249B0 File Offset: 0x00022BB0
		private static void LogArgError(IGameLogger log, string[] command, string error)
		{
			string rawCommandName = ArgUtility.Get(command, 0, null, true);
			string commandLabel = rawCommandName;
			if (!string.IsNullOrWhiteSpace(rawCommandName))
			{
				string actualCommandName;
				if (!DebugCommands.Aliases.TryGetValue(rawCommandName, out actualCommandName))
				{
					foreach (string handlerName in DebugCommands.Handlers.Keys)
					{
						if (rawCommandName.EqualsIgnoreCase(handlerName))
						{
							actualCommandName = handlerName;
							break;
						}
					}
				}
				commandLabel = (actualCommandName ?? rawCommandName);
				if (!commandLabel.EqualsIgnoreCase(rawCommandName))
				{
					commandLabel = rawCommandName + " (" + commandLabel + ")";
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Failed parsing ");
			defaultInterpolatedStringHandler.AppendFormatted(commandLabel);
			defaultInterpolatedStringHandler.AppendLiteral(" command: ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x04000326 RID: 806
		private static readonly Dictionary<string, DebugCommandHandlerDelegate> Handlers = new Dictionary<string, DebugCommandHandlerDelegate>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000327 RID: 807
		private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x02000409 RID: 1033
		public static class DefaultHandlers
		{
			// Token: 0x06003A7F RID: 14975 RVA: 0x002D9594 File Offset: 0x002D7794
			public static void GrowWildTrees(string[] command, IGameLogger log)
			{
				TerrainFeature[] array = Game1.currentLocation.terrainFeatures.Values.ToArray<TerrainFeature>();
				for (int i = 0; i < array.Length; i++)
				{
					Tree tree = array[i] as Tree;
					if (tree != null)
					{
						tree.growthStage.Value = 4;
						tree.fertilized.Value = true;
						tree.dayUpdate();
						tree.fertilized.Value = false;
					}
				}
			}

			// Token: 0x06003A80 RID: 14976 RVA: 0x002D9600 File Offset: 0x002D7800
			public static void Emote(string[] command, IGameLogger log)
			{
				for (int i = 1; i < command.Length; i += 2)
				{
					string npcName;
					string error;
					int emoteId;
					if (!ArgUtility.TryGet(command, i, out npcName, out error, false, "string npcName") || !ArgUtility.TryGetInt(command, i + 1, out emoteId, out error, "int emoteId"))
					{
						log.Warn(error);
					}
					else
					{
						NPC npc = Utility.fuzzyCharacterSearch(npcName, false);
						if (npc == null)
						{
							log.Error("Couldn't find character named " + npcName, null);
						}
						else
						{
							npc.doEmote(emoteId, true);
						}
					}
				}
			}

			// Token: 0x06003A81 RID: 14977 RVA: 0x002D9675 File Offset: 0x002D7875
			public static void EventTestSpecific(string[] command, IGameLogger log)
			{
				Game1.eventTest = new EventTest(command);
			}

			// Token: 0x06003A82 RID: 14978 RVA: 0x002D9684 File Offset: 0x002D7884
			public static void EventTest(string[] command, IGameLogger log)
			{
				string locationName;
				string error;
				int startingEventIndex;
				if (!ArgUtility.TryGetOptional(command, 1, out locationName, out error, null, true, "string locationName") || !ArgUtility.TryGetOptionalInt(command, 2, out startingEventIndex, out error, 0, "int startingEventIndex"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.eventTest = new EventTest(locationName ?? "", startingEventIndex);
			}

			// Token: 0x06003A83 RID: 14979 RVA: 0x002D96D8 File Offset: 0x002D78D8
			public static void GetAllQuests(string[] command, IGameLogger log)
			{
				foreach (KeyValuePair<string, string> v in DataLoader.Quests(Game1.content))
				{
					Game1.player.addQuest(v.Key);
				}
			}

			// Token: 0x06003A84 RID: 14980 RVA: 0x002D973C File Offset: 0x002D793C
			public static void Movie(string[] command, IGameLogger log)
			{
				string movieId;
				string error;
				string invitedNpcName;
				if (!ArgUtility.TryGetOptional(command, 1, out movieId, out error, null, false, "string movieId") || !ArgUtility.TryGetOptional(command, 2, out invitedNpcName, out error, null, false, "string invitedNpcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				MovieData movieData;
				if (movieId != null && !MovieTheater.TryGetMovieData(movieId, out movieData))
				{
					log.Error("No movie found with ID '" + movieId + "'.", null);
					return;
				}
				if (invitedNpcName != null)
				{
					NPC npc = Utility.fuzzyCharacterSearch(invitedNpcName, true);
					if (npc != null)
					{
						MovieTheater.Invite(Game1.player, npc);
					}
					else
					{
						log.Error("No NPC found matching '" + invitedNpcName + "'.", null);
					}
				}
				if (movieId != null)
				{
					MovieTheater.forceMovieId = movieId;
				}
				LocationRequest locationRequest = Game1.getLocationRequest("MovieTheater", false);
				locationRequest.OnWarp += delegate()
				{
					((MovieTheater)Game1.currentLocation).performAction("Theater_Doors", Game1.player, Location.Origin);
				};
				Game1.warpFarmer(locationRequest, 10, 10, 0);
			}

			// Token: 0x06003A85 RID: 14981 RVA: 0x002D9818 File Offset: 0x002D7A18
			public static void MovieSchedule(string[] command, IGameLogger log)
			{
				int year;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out year, out error, Game1.year, "int year"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("Movie schedule for ");
				string value;
				if (year != Game1.year)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 1);
					defaultInterpolatedStringHandler.AppendLiteral("year ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(year);
					value = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendLiteral("this year (year ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(year);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					value = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				appendInterpolatedStringHandler.AppendFormatted(value);
				appendInterpolatedStringHandler.AppendLiteral(":");
				StringBuilder schedule = stringBuilder2.AppendLine(ref appendInterpolatedStringHandler).AppendLine();
				Season[] array = new Season[4];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.BAED642339816AFFB3FE8719792D0E4CE82F12DB72B7373D244EAA65445800FE).FieldHandle);
				foreach (Season season in array)
				{
					List<Tuple<MovieData, int>> movies = new List<Tuple<MovieData, int>>();
					string lastMovieId = null;
					for (int day = 1; day <= 28; day++)
					{
						MovieData movie = MovieTheater.GetMovieForDate(new WorldDate(year, season, day));
						if (movie.Id != lastMovieId)
						{
							movies.Add(Tuple.Create<MovieData, int>(movie, day));
							lastMovieId = movie.Id;
						}
					}
					for (int i = 0; i < movies.Count; i++)
					{
						MovieData item = movies[i].Item1;
						int startDay = movies[i].Item2;
						int endDay = (movies.Count > i + 1) ? (movies[i + 1].Item2 - 1) : 28;
						string title = TokenParser.ParseText(item.Title, null, null, null);
						schedule.Append(season).Append(' ').Append(startDay);
						if (endDay != startDay)
						{
							schedule.Append("-").Append(endDay);
						}
						schedule.Append(": ").AppendLine(title);
					}
				}
				log.Info(schedule.ToString());
			}

			// Token: 0x06003A86 RID: 14982 RVA: 0x002D9A2C File Offset: 0x002D7C2C
			public static void Shop(string[] command, IGameLogger log)
			{
				string shopId;
				string error;
				string ownerName;
				if (!ArgUtility.TryGet(command, 1, out shopId, out error, false, "string shopId") || !ArgUtility.TryGetOptional(command, 2, out ownerName, out error, null, false, "string ownerName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				string foundShopId = Utility.fuzzySearch(shopId, DataLoader.Shops(Game1.content).Keys.ToArray<string>());
				if (foundShopId == null)
				{
					log.Error("Couldn't find any shop in Data/Shops matching ID '" + shopId + "'.", null);
					return;
				}
				shopId = foundShopId;
				if ((ownerName != null) ? Utility.TryOpenShopMenu(shopId, ownerName, true) : Utility.TryOpenShopMenu(shopId, Game1.player.currentLocation, null, null, true, true, null))
				{
					log.Info("Opened shop with ID '" + shopId + "'.");
					return;
				}
				log.Error("Failed to open shop with ID '" + shopId + "'. Is the data in Data/Shops valid?", null);
			}

			// Token: 0x06003A87 RID: 14983 RVA: 0x002D9B08 File Offset: 0x002D7D08
			public static void ExportShops(string[] command, IGameLogger log)
			{
				StringBuilder report = new StringBuilder();
				string[] array = new string[2];
				array[0] = "Shop";
				string[] openShopArgs = array;
				foreach (string shopId in DataLoader.Shops(Game1.content).Keys)
				{
					report.AppendLine(shopId);
					report.AppendLine("".PadRight(Math.Max(50, shopId.Length), '-'));
					StringBuilder stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler;
					try
					{
						openShopArgs[1] = shopId;
						DebugCommands.DefaultHandlers.Shop(openShopArgs, log);
					}
					catch (Exception ex)
					{
						stringBuilder = report.Append("    ");
						StringBuilder stringBuilder2 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder);
						appendInterpolatedStringHandler.AppendLiteral("Failed to open shop '");
						appendInterpolatedStringHandler.AppendFormatted(shopId);
						appendInterpolatedStringHandler.AppendLiteral("'.");
						stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
						report.AppendLine("    " + string.Join("\n    ", ex.ToString().Split('\n', StringSplitOptions.None)));
						continue;
					}
					ShopMenu shop = Game1.activeClickableMenu as ShopMenu;
					if (shop != null)
					{
						switch (shop.currency)
						{
						case 0:
							report.AppendLine("    Currency: gold");
							break;
						case 1:
							report.AppendLine("    Currency: star tokens");
							break;
						case 2:
							report.AppendLine("    Currency: Qi coins");
							break;
						case 3:
							goto IL_156;
						case 4:
							report.AppendLine("    Currency: Qi gems");
							break;
						default:
							goto IL_156;
						}
						IL_1A1:
						report.AppendLine();
						var summary = shop.itemPriceAndStock.Select(delegate(KeyValuePair<ISalable, ItemStockInformation> entry)
						{
							ISalable item = entry.Key;
							ItemStockInformation stock = entry.Value;
							string qualifiedItemId = item.QualifiedItemId;
							string displayName = item.DisplayName;
							int price = stock.Price;
							string trade = (stock.TradeItem != null) ? (stock.TradeItem + " x" + stock.TradeItemCount.GetValueOrDefault(1).ToString()) : null;
							string stockLimit;
							if (stock.Stock == 2147483647 || stock.LimitedStockMode == LimitedStockMode.None)
							{
								stockLimit = null;
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
								defaultInterpolatedStringHandler2.AppendFormatted<LimitedStockMode>(stock.LimitedStockMode);
								defaultInterpolatedStringHandler2.AppendLiteral(" ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(stock.Stock);
								stockLimit = defaultInterpolatedStringHandler2.ToStringAndClear();
							}
							return new
							{
								Id = qualifiedItemId,
								Name = displayName,
								Price = price,
								Trade = trade,
								StockLimit = stockLimit
							};
						}).ToArray();
						int idWidth = "id".Length;
						int nameWidth = "name".Length;
						int priceWidth = "price".Length;
						int tradeWidth = "trade".Length;
						int stockWidth = "stock limit".Length;
						var array2 = summary;
						for (int i = 0; i < array2.Length; i++)
						{
							var entry3 = array2[i];
							idWidth = Math.Max(idWidth, entry3.Id.Length);
							nameWidth = Math.Max(nameWidth, entry3.Name.Length);
							priceWidth = Math.Max(priceWidth, entry3.Price.ToString().Length);
							if (entry3.Trade != null)
							{
								tradeWidth = Math.Max(tradeWidth, entry3.Trade.Length);
							}
							if (entry3.StockLimit != null)
							{
								tradeWidth = Math.Max(tradeWidth, entry3.StockLimit.Length);
							}
						}
						report.Append("    ").Append("id".PadRight(idWidth)).Append(" | ").Append("name".PadRight(nameWidth)).Append(" | ").Append("price".PadRight(priceWidth)).Append(" | ").Append("trade".PadRight(tradeWidth)).AppendLine(" | stock limit");
						report.Append("    ").Append("".PadRight(idWidth, '-')).Append(" | ").Append("".PadRight(nameWidth, '-')).Append(" | ").Append("".PadRight(priceWidth, '-')).Append(" | ").Append("".PadRight(tradeWidth, '-')).Append(" | ").AppendLine("".PadRight(stockWidth, '-'));
						array2 = summary;
						for (int i = 0; i < array2.Length; i++)
						{
							var entry2 = array2[i];
							report.Append("    ").Append(entry2.Id.PadRight(idWidth)).Append(" | ").Append(entry2.Name.PadRight(nameWidth)).Append(" | ").Append(entry2.Price.ToString().PadRight(priceWidth)).Append(" | ").Append((entry2.Trade ?? "").PadRight(tradeWidth)).Append(" | ").AppendLine(entry2.StockLimit);
						}
						goto IL_4D1;
						IL_156:
						stringBuilder = report;
						StringBuilder stringBuilder3 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder);
						appendInterpolatedStringHandler.AppendFormatted("    ");
						appendInterpolatedStringHandler.AppendLiteral("Currency: unknown (");
						appendInterpolatedStringHandler.AppendFormatted<int>(shop.currency);
						appendInterpolatedStringHandler.AppendLiteral(")");
						stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
						goto IL_1A1;
					}
					stringBuilder = report.Append("    ");
					StringBuilder stringBuilder4 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(60, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("Failed to open shop '");
					appendInterpolatedStringHandler.AppendFormatted(shopId);
					appendInterpolatedStringHandler.AppendLiteral("': shop menu unexpected failed to open.");
					stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
					IL_4D1:
					report.AppendLine();
					report.AppendLine();
				}
				string localAppDataFolder = Program.GetLocalAppDataFolder("Exports", true);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "yyyy-MM-dd");
				defaultInterpolatedStringHandler.AppendLiteral(" shop export.txt");
				string exportFilePath = Path.Combine(localAppDataFolder, defaultInterpolatedStringHandler.ToStringAndClear());
				File.WriteAllText(exportFilePath, report.ToString());
				log.Info("Exported shop data to " + exportFilePath + ".");
			}

			// Token: 0x06003A88 RID: 14984 RVA: 0x002DA0A8 File Offset: 0x002D82A8
			public static void Dating(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.friendshipData[npcName].Status = FriendshipStatus.Dating;
			}

			// Token: 0x06003A89 RID: 14985 RVA: 0x002DA0E7 File Offset: 0x002D82E7
			public static void ClearActiveDialogueEvents(string[] command, IGameLogger log)
			{
				Game1.player.activeDialogueEvents.Clear();
			}

			// Token: 0x06003A8A RID: 14986 RVA: 0x002DA0F8 File Offset: 0x002D82F8
			public static void Buff(string[] command, IGameLogger log)
			{
				string buffId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out buffId, out error, false, "string buffId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.applyBuff(buffId);
			}

			// Token: 0x06003A8B RID: 14987 RVA: 0x002DA12C File Offset: 0x002D832C
			public static void ClearBuffs(string[] command, IGameLogger log)
			{
				Game1.player.ClearBuffs();
			}

			// Token: 0x06003A8C RID: 14988 RVA: 0x002DA138 File Offset: 0x002D8338
			public static void PauseTime(string[] command, IGameLogger log)
			{
				Game1.isTimePaused = !Game1.isTimePaused;
				Game1.playSound(Game1.isTimePaused ? "bigSelect" : "bigDeSelect", null);
			}

			// Token: 0x06003A8D RID: 14989 RVA: 0x002DA174 File Offset: 0x002D8374
			[OtherNames(new string[]
			{
				"fbf"
			})]
			public static void FrameByFrame(string[] command, IGameLogger log)
			{
				Game1.frameByFrame = !Game1.frameByFrame;
				Game1.playSound(Game1.frameByFrame ? "bigSelect" : "bigDeSelect", null);
			}

			// Token: 0x06003A8E RID: 14990 RVA: 0x002DA1B0 File Offset: 0x002D83B0
			[OtherNames(new string[]
			{
				"fbp",
				"fill",
				"fillbp"
			})]
			public static void FillBackpack(string[] command, IGameLogger log)
			{
				for (int i = 0; i < Game1.player.Items.Count; i++)
				{
					if (Game1.player.Items[i] == null)
					{
						ItemMetadata metadata = null;
						while (metadata == null)
						{
							metadata = ItemRegistry.ResolveMetadata(Game1.random.Next(1000).ToString());
							ParsedItemData data = (metadata != null) ? metadata.GetParsedData() : null;
							if (data == null || data.Category == -999 || data.ObjectType == "Crafting" || data.ObjectType == "Seeds")
							{
								metadata = null;
							}
						}
						Game1.player.Items[i] = metadata.CreateItem(1, 0);
					}
				}
			}

			// Token: 0x06003A8F RID: 14991 RVA: 0x002DA270 File Offset: 0x002D8470
			public static void Bobber(string[] command, IGameLogger log)
			{
				int bobberStyle;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out bobberStyle, out error, "int bobberStyle"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.bobberStyle.Value = bobberStyle;
			}

			// Token: 0x06003A90 RID: 14992 RVA: 0x002DA2A8 File Offset: 0x002D84A8
			[OtherNames(new string[]
			{
				"sl"
			})]
			public static void ShiftToolbarLeft(string[] command, IGameLogger log)
			{
				Game1.player.shiftToolbar(false);
			}

			// Token: 0x06003A91 RID: 14993 RVA: 0x002DA2B5 File Offset: 0x002D84B5
			[OtherNames(new string[]
			{
				"sr"
			})]
			public static void ShiftToolbarRight(string[] command, IGameLogger log)
			{
				Game1.player.shiftToolbar(true);
			}

			// Token: 0x06003A92 RID: 14994 RVA: 0x002DA2C4 File Offset: 0x002D84C4
			public static void CharacterInfo(string[] command, IGameLogger log)
			{
				Game1.showGlobalMessage(Game1.currentLocation.characters.Count.ToString() + " characters on this map");
			}

			// Token: 0x06003A93 RID: 14995 RVA: 0x002DA2F8 File Offset: 0x002D84F8
			public static void DoesItemExist(string[] command, IGameLogger log)
			{
				string itemId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out itemId, out error, false, "string itemId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.showGlobalMessage(Utility.doesItemExistAnywhere(itemId) ? "Yes" : "No");
			}

			// Token: 0x06003A94 RID: 14996 RVA: 0x002DA33C File Offset: 0x002D853C
			public static void SpecialItem(string[] command, IGameLogger log)
			{
				string itemId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out itemId, out error, false, "string itemId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.specialItems.Add(itemId);
			}

			// Token: 0x06003A95 RID: 14997 RVA: 0x002DA378 File Offset: 0x002D8578
			public static void AnimalInfo(string[] command, IGameLogger log)
			{
				int animalCount = 0;
				int locationCount = 0;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					int curCount = location.animals.Length;
					if (curCount > 0)
					{
						animalCount += curCount;
						int locationCount = locationCount;
						locationCount++;
					}
					return true;
				}, true, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
				defaultInterpolatedStringHandler.AppendFormatted<int>(animalCount);
				defaultInterpolatedStringHandler.AppendLiteral(" animals in ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(locationCount);
				defaultInterpolatedStringHandler.AppendLiteral(" locations");
				Game1.showGlobalMessage(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003A96 RID: 14998 RVA: 0x002DA3F4 File Offset: 0x002D85F4
			public static void ClearChildren(string[] command, IGameLogger log)
			{
				Game1.player.getRidOfChildren();
			}

			// Token: 0x06003A97 RID: 14999 RVA: 0x002DA400 File Offset: 0x002D8600
			public static void CreateSplash(string[] command, IGameLogger log)
			{
				Point offset = default(Point);
				switch (Game1.player.FacingDirection)
				{
				case 0:
					offset.Y = 4;
					break;
				case 1:
					offset.X = 4;
					break;
				case 2:
					offset.Y = -4;
					break;
				case 3:
					offset.X = -4;
					break;
				}
				Game1.player.currentLocation.fishSplashPoint.Set(new Point(Game1.player.TilePoint.X + offset.X, Game1.player.TilePoint.Y + offset.Y));
			}

			// Token: 0x06003A98 RID: 15000 RVA: 0x002DA4A8 File Offset: 0x002D86A8
			public static void Pregnant(string[] command, IGameLogger log)
			{
				WorldDate birthingDate = Game1.Date;
				birthingDate.TotalDays++;
				Game1.player.GetSpouseFriendship().NextBirthingDate = birthingDate;
			}

			// Token: 0x06003A99 RID: 15001 RVA: 0x002DA4DC File Offset: 0x002D86DC
			public static void SpreadSeeds(string[] command, IGameLogger log)
			{
				string cropId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out cropId, out error, false, "string cropId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation currentLocation = Game1.currentLocation;
				if (currentLocation == null)
				{
					return;
				}
				currentLocation.ForEachDirt(delegate(HoeDirt dirt)
				{
					dirt.crop = new Crop(cropId, (int)dirt.Tile.X, (int)dirt.Tile.Y, dirt.Location);
					return true;
				}, true);
			}

			// Token: 0x06003A9A RID: 15002 RVA: 0x002DA52C File Offset: 0x002D872C
			public static void SpreadDirt(string[] command, IGameLogger log)
			{
				GameLocation location = Game1.currentLocation;
				if (location == null)
				{
					return;
				}
				for (int x = 0; x < location.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < location.map.Layers[0].LayerHeight; y++)
					{
						if (location.doesTileHaveProperty(x, y, "Diggable", "Back", false) != null && location.CanItemBePlacedHere(new Vector2((float)x, (float)y), true, CollisionMask.All, CollisionMask.None, false, false))
						{
							location.terrainFeatures.Add(new Vector2((float)x, (float)y), new HoeDirt());
						}
					}
				}
			}

			// Token: 0x06003A9B RID: 15003 RVA: 0x002DA5D0 File Offset: 0x002D87D0
			public static void RemoveFurniture(string[] command, IGameLogger log)
			{
				Game1.currentLocation.furniture.Clear();
			}

			// Token: 0x06003A9C RID: 15004 RVA: 0x002DA5E4 File Offset: 0x002D87E4
			public static void MakeEx(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.friendshipData[npcName].RoommateMarriage = false;
				Game1.player.friendshipData[npcName].Status = FriendshipStatus.Divorced;
			}

			// Token: 0x06003A9D RID: 15005 RVA: 0x002DA63C File Offset: 0x002D883C
			public static void DarkTalisman(string[] command, IGameLogger log)
			{
				GameLocation gameLocation = Game1.RequireLocation("Railroad", false);
				GameLocation witchHut = Game1.RequireLocation("WitchHut", false);
				gameLocation.setMapTile(54, 35, 287, "Buildings", "untitled tile sheet", "", true);
				gameLocation.setMapTile(54, 34, 262, "Front", "untitled tile sheet", "", true);
				witchHut.setMapTile(4, 11, 114, "Buildings", "untitled tile sheet", "MagicInk", true);
				Game1.player.hasDarkTalisman = true;
				Game1.player.hasMagicInk = false;
				Game1.player.mailReceived.Clear();
			}

			// Token: 0x06003A9E RID: 15006 RVA: 0x002DA6E0 File Offset: 0x002D88E0
			public static void ConventionMode(string[] command, IGameLogger log)
			{
				Game1.conventionMode = !Game1.conventionMode;
			}

			// Token: 0x06003A9F RID: 15007 RVA: 0x002DA6F0 File Offset: 0x002D88F0
			public static void FarmMap(string[] command, IGameLogger log)
			{
				int farmType;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out farmType, out error, "int farmType"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.locations.RemoveWhere((GameLocation location) => location is Farm || location is FarmHouse);
				Game1.whichFarm = farmType;
				Game1.locations.Add(new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm"));
				Game1.locations.Add(new FarmHouse("Maps\\FarmHouse", "FarmHouse"));
			}

			// Token: 0x06003AA0 RID: 15008 RVA: 0x002DA789 File Offset: 0x002D8989
			public static void ClearMuseum(string[] command, IGameLogger log)
			{
				Game1.RequireLocation<LibraryMuseum>("ArchaeologyHouse", false).museumPieces.Clear();
			}

			// Token: 0x06003AA1 RID: 15009 RVA: 0x002DA7A0 File Offset: 0x002D89A0
			public static void Clone(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.characters.Add(Utility.fuzzyCharacterSearch(npcName, true));
			}

			// Token: 0x06003AA2 RID: 15010 RVA: 0x002DA7E0 File Offset: 0x002D89E0
			[OtherNames(new string[]
			{
				"zl"
			})]
			public static void ZoomLevel(string[] command, IGameLogger log)
			{
				int zoomLevel;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out zoomLevel, out error, "int zoomLevel"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.options.desiredBaseZoomLevel = (float)zoomLevel / 100f;
			}

			// Token: 0x06003AA3 RID: 15011 RVA: 0x002DA81C File Offset: 0x002D8A1C
			[OtherNames(new string[]
			{
				"us"
			})]
			public static void UiScale(string[] command, IGameLogger log)
			{
				int uiScale;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out uiScale, out error, "int uiScale"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.options.desiredUIScale = (float)uiScale / 100f;
			}

			// Token: 0x06003AA4 RID: 15012 RVA: 0x002DA856 File Offset: 0x002D8A56
			public static void DeleteArch(string[] command, IGameLogger log)
			{
				Game1.player.archaeologyFound.Clear();
				Game1.player.fishCaught.Clear();
				Game1.player.mineralsFound.Clear();
				Game1.player.mailReceived.Clear();
			}

			// Token: 0x06003AA5 RID: 15013 RVA: 0x002DA894 File Offset: 0x002D8A94
			public static void Save(string[] command, IGameLogger log)
			{
				Game1.saveOnNewDay = !Game1.saveOnNewDay;
				Game1.playSound(Game1.saveOnNewDay ? "bigSelect" : "bigDeSelect", null);
			}

			// Token: 0x06003AA6 RID: 15014 RVA: 0x002DA8D0 File Offset: 0x002D8AD0
			[OtherNames(new string[]
			{
				"removeLargeTf"
			})]
			public static void RemoveLargeTerrainFeature(string[] command, IGameLogger log)
			{
				Game1.currentLocation.largeTerrainFeatures.Clear();
			}

			// Token: 0x06003AA7 RID: 15015 RVA: 0x002DA8E1 File Offset: 0x002D8AE1
			public static void Test(string[] command, IGameLogger log)
			{
				Game1.currentMinigame = new Test();
			}

			// Token: 0x06003AA8 RID: 15016 RVA: 0x002DA8F0 File Offset: 0x002D8AF0
			public static void FenceDecay(string[] command, IGameLogger log)
			{
				int decayAmount;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out decayAmount, out error, "int decayAmount"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				foreach (Object @object in Game1.currentLocation.objects.Values)
				{
					Fence fence = @object as Fence;
					if (fence != null)
					{
						fence.health.Value -= (float)decayAmount;
					}
				}
			}

			// Token: 0x06003AA9 RID: 15017 RVA: 0x002DA97C File Offset: 0x002D8B7C
			[OtherNames(new string[]
			{
				"sb"
			})]
			public static void ShowTextAboveHead(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Utility.fuzzyCharacterSearch(npcName, true).showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3206"), null, 2, 3000, 0);
			}

			// Token: 0x06003AAA RID: 15018 RVA: 0x002DA9D0 File Offset: 0x002D8BD0
			public static void Gamepad(string[] command, IGameLogger log)
			{
				Game1.options.gamepadControls = !Game1.options.gamepadControls;
				Game1.options.mouseControls = !Game1.options.gamepadControls;
				Game1.showGlobalMessage(Game1.options.gamepadControls ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3209") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3210"));
			}

			// Token: 0x06003AAB RID: 15019 RVA: 0x002DAA3C File Offset: 0x002D8C3C
			public static void Slimecraft(string[] command, IGameLogger log)
			{
				Game1.player.craftingRecipes.Add("Slime Incubator", 0);
				Game1.player.craftingRecipes.Add("Slime Egg-Press", 0);
				Game1.playSound("crystal", new int?(0));
			}

			// Token: 0x06003AAC RID: 15020 RVA: 0x002DAA7C File Offset: 0x002D8C7C
			[OtherNames(new string[]
			{
				"kms"
			})]
			public static void KillMonsterStat(string[] command, IGameLogger log)
			{
				string monsterId;
				string error;
				int kills;
				if (!ArgUtility.TryGet(command, 1, out monsterId, out error, false, "string monsterId") || !ArgUtility.TryGetInt(command, 2, out kills, out error, "int kills"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.stats.specificMonstersKilled[monsterId] = kills;
				log.Info(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", monsterId, kills));
			}

			// Token: 0x06003AAD RID: 15021 RVA: 0x002DAAE4 File Offset: 0x002D8CE4
			public static void RemoveAnimals(string[] command, IGameLogger log)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.Animals.Clear();
					foreach (Building building in location.buildings)
					{
						AnimalHouse animalHouse = building.GetIndoors() as AnimalHouse;
						if (animalHouse != null)
						{
							animalHouse.Animals.Clear();
						}
					}
					return true;
				}, false, false);
			}

			// Token: 0x06003AAE RID: 15022 RVA: 0x002DAB0C File Offset: 0x002D8D0C
			public static void FixAnimals(string[] command, IGameLogger log)
			{
				bool fixedAny = false;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					int fixedCount = 0;
					foreach (Building building in location.buildings)
					{
						AnimalHouse animalHouse = building.GetIndoors() as AnimalHouse;
						if (animalHouse != null)
						{
							using (NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.ValuesCollection.Enumerator enumerator2 = animalHouse.animals.Values.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									FarmAnimal animal = enumerator2.Current;
									Func<long, bool> <>9__2;
									foreach (Building otherBuilding in location.buildings)
									{
										AnimalHouse otherHouse = otherBuilding.GetIndoors() as AnimalHouse;
										if (otherHouse != null && otherHouse.animalsThatLiveHere.Contains(animal.myID.Value) && !otherBuilding.Equals(animal.home))
										{
											int num = fixedCount;
											NetList<long, NetLong> animalsThatLiveHere = otherHouse.animalsThatLiveHere;
											Func<long, bool> match;
											if ((match = <>9__2) == null)
											{
												match = (<>9__2 = ((long id) => id == animal.myID.Value));
											}
											fixedCount = num + animalsThatLiveHere.RemoveWhere(match);
										}
									}
								}
							}
							fixedCount += animalHouse.animalsThatLiveHere.RemoveWhere((long id) => Utility.getAnimal(id) == null);
						}
					}
					if (fixedCount > 0)
					{
						Game1.playSound("crystal", new int?(0));
						IGameLogger log2 = log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Fixed ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(fixedCount);
						defaultInterpolatedStringHandler.AppendLiteral(" animals in the '");
						defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral("' location.");
						log2.Info(defaultInterpolatedStringHandler.ToStringAndClear());
						fixedAny = true;
					}
					return true;
				}, false, false);
				if (!fixedAny)
				{
					log.Info("No animal issues found.");
				}
				Utility.fixAllAnimals();
			}

			// Token: 0x06003AAF RID: 15023 RVA: 0x002DAB5D File Offset: 0x002D8D5D
			public static void DisplaceAnimals(string[] command, IGameLogger log)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location.animals.Length == 0 && location.buildings.Count == 0)
					{
						return true;
					}
					Utility.fixAllAnimals();
					foreach (Building building in location.buildings)
					{
						AnimalHouse animalHouse = building.GetIndoors() as AnimalHouse;
						if (animalHouse != null)
						{
							foreach (FarmAnimal animal in animalHouse.animals.Values)
							{
								animal.homeInterior = null;
								animal.Position = Utility.recursiveFindOpenTileForCharacter(animal, location, new Vector2(40f, 40f), 200, true) * 64f;
								location.animals.TryAdd(animal.myID.Value, animal);
							}
							animalHouse.animals.Clear();
							animalHouse.animalsThatLiveHere.Clear();
						}
					}
					return true;
				}, true, false);
			}

			// Token: 0x06003AB0 RID: 15024 RVA: 0x002DAB85 File Offset: 0x002D8D85
			[OtherNames(new string[]
			{
				"sdkInfo"
			})]
			public static void SteamInfo(string[] command, IGameLogger log)
			{
				Program.sdk.DebugInfo();
			}

			// Token: 0x06003AB1 RID: 15025 RVA: 0x002DAB94 File Offset: 0x002D8D94
			public static void Achieve(string[] command, IGameLogger log)
			{
				string achievementId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out achievementId, out error, false, "string achievementId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.getSteamAchievement(achievementId);
			}

			// Token: 0x06003AB2 RID: 15026 RVA: 0x002DABC3 File Offset: 0x002D8DC3
			public static void ResetAchievements(string[] command, IGameLogger log)
			{
				Program.sdk.ResetAchievements();
			}

			// Token: 0x06003AB3 RID: 15027 RVA: 0x002DABCF File Offset: 0x002D8DCF
			public static void Divorce(string[] command, IGameLogger log)
			{
				Game1.player.divorceTonight.Value = true;
			}

			// Token: 0x06003AB4 RID: 15028 RVA: 0x002DABE4 File Offset: 0x002D8DE4
			public static void BefriendAnimals(string[] command, IGameLogger log)
			{
				int friendship;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out friendship, out error, 1000, "int friendship"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				foreach (FarmAnimal farmAnimal in Game1.currentLocation.animals.Values)
				{
					farmAnimal.friendshipTowardFarmer.Value = friendship;
				}
			}

			// Token: 0x06003AB5 RID: 15029 RVA: 0x002DAC68 File Offset: 0x002D8E68
			public static void PetToFarm(string[] command, IGameLogger log)
			{
				Game1.RequireCharacter<Pet>(Game1.player.getPetName(), false).setAtFarmPosition();
			}

			// Token: 0x06003AB6 RID: 15030 RVA: 0x002DAC80 File Offset: 0x002D8E80
			public static void BefriendPets(string[] command, IGameLogger log)
			{
				foreach (NPC npc in Utility.getAllCharacters())
				{
					Pet pet = npc as Pet;
					if (pet != null)
					{
						pet.friendshipTowardFarmer.Value = 1000;
					}
				}
			}

			// Token: 0x06003AB7 RID: 15031 RVA: 0x002DACE4 File Offset: 0x002D8EE4
			public static void Version(string[] command, IGameLogger log)
			{
				Version version = typeof(Game1).Assembly.GetName().Version;
				log.Info(((version != null) ? version.ToString() : null) ?? "");
			}

			// Token: 0x06003AB8 RID: 15032 RVA: 0x002DAD1C File Offset: 0x002D8F1C
			[OtherNames(new string[]
			{
				"sdlv"
			})]
			public static void SdlVersion(string[] command, IGameLogger log)
			{
				Assembly assembly = Assembly.GetAssembly(GameRunner.instance.Window.GetType());
				Type sdlType = (assembly != null) ? assembly.GetType("Sdl") : null;
				if (sdlType == null)
				{
					log.Error("Could not find type 'Sdl'", null);
					return;
				}
				FieldInfo versionField = sdlType.GetField("version", BindingFlags.Static | BindingFlags.Public);
				if (versionField == null)
				{
					log.Error("SDL does not have field 'version'", null);
					return;
				}
				Type versionType = versionField.FieldType;
				object versionObject = versionField.GetValue(null);
				if (versionType == null)
				{
					log.Error("Could not find type 'Sdl::Type'", null);
					return;
				}
				if (versionObject == null)
				{
					log.Error("The obtained from from SDL was null", null);
					return;
				}
				byte[] versionBytes = new byte[3];
				string[] versionComponents = new string[]
				{
					"Major",
					"Minor",
					"Patch"
				};
				for (int c = 0; c < 3; c++)
				{
					string componentName = versionComponents[c];
					FieldInfo componentField = versionType.GetField(componentName, BindingFlags.Instance | BindingFlags.Public);
					if (componentField == null)
					{
						log.Error("SDL::Version does not have field '" + componentName + "'", null);
						return;
					}
					object componentObject = componentField.GetValue(versionObject);
					if (!(componentObject is byte))
					{
						log.Error("SDL::Version field '" + componentName + "' is not a byte", null);
						return;
					}
					byte b = (byte)componentObject;
					versionBytes[c] = (byte)componentObject;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
				defaultInterpolatedStringHandler.AppendLiteral("SDL Version: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)versionBytes[0]);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)versionBytes[1]);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)versionBytes[2]);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003AB9 RID: 15033 RVA: 0x002DAEB8 File Offset: 0x002D90B8
			[OtherNames(new string[]
			{
				"ns"
			})]
			public static void NoSave(string[] command, IGameLogger log)
			{
				Game1.saveOnNewDay = !Game1.saveOnNewDay;
				if (!Game1.saveOnNewDay)
				{
					Game1.playSound("bigDeSelect", null);
				}
				else
				{
					Game1.playSound("bigSelect", null);
				}
				log.Info("Saving is now " + (Game1.saveOnNewDay ? "enabled" : "disabled"));
			}

			// Token: 0x06003ABA RID: 15034 RVA: 0x002DAF28 File Offset: 0x002D9128
			[OtherNames(new string[]
			{
				"rfh"
			})]
			public static void ReadyForHarvest(string[] command, IGameLogger log)
			{
				Vector2 tile;
				string error;
				if (!ArgUtility.TryGetVector2(command, 1, out tile, out error, true, "Vector2 tile"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.objects[tile].minutesUntilReady.Value = 1;
			}

			// Token: 0x06003ABB RID: 15035 RVA: 0x002DAF6C File Offset: 0x002D916C
			public static void BeachBridge(string[] command, IGameLogger log)
			{
				Beach beach = Game1.RequireLocation<Beach>("Beach", false);
				beach.bridgeFixed.Value = !beach.bridgeFixed.Value;
				if (!beach.bridgeFixed.Value)
				{
					beach.setMapTile(58, 13, 284, "Buildings", "untitled tile sheet", null, true);
				}
			}

			// Token: 0x06003ABC RID: 15036 RVA: 0x002DAFC8 File Offset: 0x002D91C8
			public static void Dp(string[] command, IGameLogger log)
			{
				int daysPlayed;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out daysPlayed, out error, "int daysPlayed"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.stats.DaysPlayed = (uint)daysPlayed;
			}

			// Token: 0x06003ABD RID: 15037 RVA: 0x002DAFFC File Offset: 0x002D91FC
			[OtherNames(new string[]
			{
				"fo"
			})]
			public static void FrameOffset(string[] command, IGameLogger log)
			{
				int frame;
				string error;
				int offsetX;
				int offsetY;
				if (!ArgUtility.TryGetInt(command, 1, out frame, out error, "int frame") || !ArgUtility.TryGetInt(command, 2, out offsetX, out error, "int offsetX") || !ArgUtility.TryGetInt(command, 3, out offsetY, out error, "int offsetY"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				FarmerRenderer.featureXOffsetPerFrame[frame] = (int)((short)offsetX);
				FarmerRenderer.featureYOffsetPerFrame[frame] = (int)((short)offsetY);
			}

			// Token: 0x06003ABE RID: 15038 RVA: 0x002DB05C File Offset: 0x002D925C
			public static void Horse(string[] command, IGameLogger log)
			{
				int tileX;
				string error;
				int tileY;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out tileX, out error, Game1.player.TilePoint.X, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 1, out tileY, out error, Game1.player.TilePoint.Y, "int tileY"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.characters.Add(new Horse(GuidHelper.NewGuid(), tileX, tileY));
			}

			// Token: 0x06003ABF RID: 15039 RVA: 0x002DB0CF File Offset: 0x002D92CF
			public static void Owl(string[] command, IGameLogger log)
			{
				Game1.currentLocation.addOwl();
			}

			// Token: 0x06003AC0 RID: 15040 RVA: 0x002DB0DC File Offset: 0x002D92DC
			public static void Pole(string[] command, IGameLogger log)
			{
				int rodLevel;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out rodLevel, out error, 0, "int rodLevel"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Item fishingRod;
				switch (rodLevel)
				{
				case 1:
					fishingRod = ItemRegistry.Create("(T)TrainingRod", 1, 0, false);
					break;
				case 2:
					fishingRod = ItemRegistry.Create("(T)FiberglassRod", 1, 0, false);
					break;
				case 3:
					fishingRod = ItemRegistry.Create("(T)IridiumRod", 1, 0, false);
					break;
				default:
					fishingRod = ItemRegistry.Create("(T)BambooRod", 1, 0, false);
					break;
				}
				Game1.player.addItemToInventoryBool(fishingRod, false);
			}

			// Token: 0x06003AC1 RID: 15041 RVA: 0x002DB168 File Offset: 0x002D9368
			public static void RemoveQuest(string[] command, IGameLogger log)
			{
				string questId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out questId, out error, false, "string questId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.removeQuest(questId);
			}

			// Token: 0x06003AC2 RID: 15042 RVA: 0x002DB19C File Offset: 0x002D939C
			public static void CompleteQuest(string[] command, IGameLogger log)
			{
				string questId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out questId, out error, false, "string questId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.completeQuest(questId);
			}

			// Token: 0x06003AC3 RID: 15043 RVA: 0x002DB1D0 File Offset: 0x002D93D0
			public static void SetPreferredPet(string[] command, IGameLogger log)
			{
				DebugCommands.DefaultHandlers.<>c__DisplayClass68_0 CS$<>8__locals1 = new DebugCommands.DefaultHandlers.<>c__DisplayClass68_0();
				string typeId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out typeId, out error, false, "string typeId") || !ArgUtility.TryGetOptional(command, 2, out CS$<>8__locals1.breedId, out error, null, false, "string breedId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				PetData data;
				if (!Pet.TryGetData(typeId, out data))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Can't set the player's preferred pet type to '");
					defaultInterpolatedStringHandler.AppendFormatted(typeId);
					defaultInterpolatedStringHandler.AppendLiteral("': no such pet type found. Expected one of ['");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", Game1.petData.Keys));
					defaultInterpolatedStringHandler.AppendLiteral("'].");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					return;
				}
				if (CS$<>8__locals1.breedId != null && data.Breeds.All((PetBreed p) => p.Id != CS$<>8__locals1.breedId))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Can't set the player's preferred pet breed to '");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.breedId);
					defaultInterpolatedStringHandler.AppendLiteral("': no such breed found. Expected one of ['");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", from p in data.Breeds
					select p.Id));
					defaultInterpolatedStringHandler.AppendLiteral("'].");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					return;
				}
				bool changed = false;
				if (Game1.player.whichPetType != typeId)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Changed preferred pet type from '");
					defaultInterpolatedStringHandler.AppendFormatted(Game1.player.whichPetType);
					defaultInterpolatedStringHandler.AppendLiteral("' to '");
					defaultInterpolatedStringHandler.AppendFormatted(typeId);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					Game1.player.whichPetType = typeId;
					changed = true;
					if (CS$<>8__locals1.breedId == null)
					{
						DebugCommands.DefaultHandlers.<>c__DisplayClass68_0 CS$<>8__locals2 = CS$<>8__locals1;
						PetBreed petBreed = data.Breeds.FirstOrDefault<PetBreed>();
						CS$<>8__locals2.breedId = ((petBreed != null) ? petBreed.Id : null);
					}
				}
				if (CS$<>8__locals1.breedId != null && Game1.player.whichPetBreed != CS$<>8__locals1.breedId)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Changed preferred pet breed from '");
					defaultInterpolatedStringHandler.AppendFormatted(Game1.player.whichPetBreed);
					defaultInterpolatedStringHandler.AppendLiteral("' to '");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.breedId);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					Game1.player.whichPetBreed = CS$<>8__locals1.breedId;
					changed = true;
				}
				if (!changed)
				{
					log.Info("The player's pet type and breed already match those values.");
				}
			}

			// Token: 0x06003AC4 RID: 15044 RVA: 0x002DB46C File Offset: 0x002D966C
			public static void ChangePet(string[] command, IGameLogger log)
			{
				DebugCommands.DefaultHandlers.<>c__DisplayClass69_0 CS$<>8__locals1 = new DebugCommands.DefaultHandlers.<>c__DisplayClass69_0();
				string petName;
				string error;
				string typeId;
				if (!ArgUtility.TryGet(command, 1, out petName, out error, false, "string petName") || !ArgUtility.TryGet(command, 2, out typeId, out error, false, "string typeId") || !ArgUtility.TryGetOptional(command, 3, out CS$<>8__locals1.breedId, out error, null, false, "string breedId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				PetData data;
				if (!Pet.TryGetData(typeId, out data))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Can't set the pet type to '");
					defaultInterpolatedStringHandler.AppendFormatted(typeId);
					defaultInterpolatedStringHandler.AppendLiteral("': no such pet type found. Expected one of ['");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", Game1.petData.Keys));
					defaultInterpolatedStringHandler.AppendLiteral("'].");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					return;
				}
				if (CS$<>8__locals1.breedId != null && data.Breeds.All((PetBreed p) => p.Id != CS$<>8__locals1.breedId))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Can't set the pet breed to '");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.breedId);
					defaultInterpolatedStringHandler.AppendLiteral("': no such breed found. Expected one of ['");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", from p in data.Breeds
					select p.Id));
					defaultInterpolatedStringHandler.AppendLiteral("'].");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					return;
				}
				Pet pet = Game1.getCharacterFromName<Pet>(petName, false, false);
				if (pet == null)
				{
					log.Error("No pet found with name '" + petName + "'.", null);
					return;
				}
				bool changed = false;
				if (pet.petType.Value != typeId)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Changed ");
					defaultInterpolatedStringHandler.AppendFormatted(pet.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'s type from '");
					defaultInterpolatedStringHandler.AppendFormatted(pet.petType.Value);
					defaultInterpolatedStringHandler.AppendLiteral("' to '");
					defaultInterpolatedStringHandler.AppendFormatted(typeId);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					pet.petType.Value = typeId;
					changed = true;
					if (CS$<>8__locals1.breedId == null)
					{
						DebugCommands.DefaultHandlers.<>c__DisplayClass69_0 CS$<>8__locals2 = CS$<>8__locals1;
						PetBreed petBreed = data.Breeds.FirstOrDefault<PetBreed>();
						CS$<>8__locals2.breedId = ((petBreed != null) ? petBreed.Id : null);
					}
				}
				if (CS$<>8__locals1.breedId != null && pet.whichBreed.Value != CS$<>8__locals1.breedId)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Changed ");
					defaultInterpolatedStringHandler.AppendFormatted(pet.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'s breed from '");
					defaultInterpolatedStringHandler.AppendFormatted(pet.whichBreed.Value);
					defaultInterpolatedStringHandler.AppendLiteral("' to '");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.breedId);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					pet.whichBreed.Value = CS$<>8__locals1.breedId;
					changed = true;
				}
				if (!changed)
				{
					log.Info(pet.Name + "'s type and breed already match those values.");
				}
			}

			// Token: 0x06003AC5 RID: 15045 RVA: 0x002DB790 File Offset: 0x002D9990
			public static void ClearCharacters(string[] command, IGameLogger log)
			{
				Game1.currentLocation.characters.Clear();
			}

			// Token: 0x06003AC6 RID: 15046 RVA: 0x002DB7A4 File Offset: 0x002D99A4
			public static void Cat(string[] command, IGameLogger log)
			{
				Point tile;
				string error;
				string breedId;
				if (!ArgUtility.TryGetPoint(command, 1, out tile, out error, "Point tile") || !ArgUtility.TryGetOptional(command, 3, out breedId, out error, "0", false, "string breedId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.characters.Add(new Pet(tile.X, tile.Y, breedId, "Cat"));
			}

			// Token: 0x06003AC7 RID: 15047 RVA: 0x002DB80C File Offset: 0x002D9A0C
			public static void Dog(string[] command, IGameLogger log)
			{
				Point tile;
				string error;
				string breedId;
				if (!ArgUtility.TryGetPoint(command, 1, out tile, out error, "Point tile") || !ArgUtility.TryGetOptional(command, 3, out breedId, out error, "0", false, "string breedId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.characters.Add(new Pet(tile.X, tile.Y, breedId, "Dog"));
			}

			// Token: 0x06003AC8 RID: 15048 RVA: 0x002DB874 File Offset: 0x002D9A74
			public static void Quest(string[] command, IGameLogger log)
			{
				string questId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out questId, out error, false, "string questId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.addQuest(questId);
			}

			// Token: 0x06003AC9 RID: 15049 RVA: 0x002DB8A8 File Offset: 0x002D9AA8
			public static void DeliveryQuest(string[] command, IGameLogger log)
			{
				Game1.player.questLog.Add(new ItemDeliveryQuest());
			}

			// Token: 0x06003ACA RID: 15050 RVA: 0x002DB8BE File Offset: 0x002D9ABE
			public static void CollectQuest(string[] command, IGameLogger log)
			{
				Game1.player.questLog.Add(new ResourceCollectionQuest());
			}

			// Token: 0x06003ACB RID: 15051 RVA: 0x002DB8D4 File Offset: 0x002D9AD4
			public static void SlayQuest(string[] command, IGameLogger log)
			{
				bool ignoreFarmMonsters;
				string error;
				if (!ArgUtility.TryGetOptionalBool(command, 1, out ignoreFarmMonsters, out error, true, "bool ignoreFarmMonsters"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.questLog.Add(new SlayMonsterQuest
				{
					ignoreFarmMonsters = 
					{
						ignoreFarmMonsters
					}
				});
			}

			// Token: 0x06003ACC RID: 15052 RVA: 0x002DB920 File Offset: 0x002D9B20
			public static void Quests(string[] command, IGameLogger log)
			{
				foreach (string id in DataLoader.Quests(Game1.content).Keys)
				{
					if (!Game1.player.hasQuest(id))
					{
						Game1.player.addQuest(id);
					}
				}
				Game1.player.questLog.Add(new ItemDeliveryQuest());
				Game1.player.questLog.Add(new SlayMonsterQuest());
			}

			// Token: 0x06003ACD RID: 15053 RVA: 0x002DB9B8 File Offset: 0x002D9BB8
			public static void ClearQuests(string[] command, IGameLogger log)
			{
				Game1.player.questLog.Clear();
			}

			// Token: 0x06003ACE RID: 15054 RVA: 0x002DB9CC File Offset: 0x002D9BCC
			[OtherNames(new string[]
			{
				"fb"
			})]
			public static void FillBin(string[] command, IGameLogger log)
			{
				IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
				shippingBin.Add(ItemRegistry.Create("(O)24", 1, 0, false));
				shippingBin.Add(ItemRegistry.Create("(O)82", 1, 0, false));
				shippingBin.Add(ItemRegistry.Create("(O)136", 1, 0, false));
				shippingBin.Add(ItemRegistry.Create("(O)16", 1, 0, false));
				shippingBin.Add(ItemRegistry.Create("(O)388", 1, 0, false));
			}

			// Token: 0x06003ACF RID: 15055 RVA: 0x002DBA46 File Offset: 0x002D9C46
			public static void Gold(string[] command, IGameLogger log)
			{
				Game1.player.Money += 1000000;
			}

			// Token: 0x06003AD0 RID: 15056 RVA: 0x002DBA60 File Offset: 0x002D9C60
			public static void ClearFarm(string[] command, IGameLogger log)
			{
				Farm farm = Game1.getFarm();
				Layer layer = farm.map.Layers[0];
				farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, layer.LayerHeight);
			}

			// Token: 0x06003AD1 RID: 15057 RVA: 0x002DBA98 File Offset: 0x002D9C98
			public static void SetupFarm(string[] command, IGameLogger log)
			{
				bool clearMore;
				string error;
				if (!ArgUtility.TryGetOptionalBool(command, 1, out clearMore, out error, false, "bool clearMore"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Farm farm = Game1.getFarm();
				Layer layer = farm.map.Layers[0];
				farm.buildings.Clear();
				farm.AddDefaultBuildings(true);
				farm.removeObjectsAndSpawned(0, 0, layer.LayerWidth, 16 + (clearMore ? 32 : 0));
				farm.removeObjectsAndSpawned(56, 17, 16, 18);
				for (int x = 58; x < 70; x++)
				{
					for (int y = 19; y < 29; y++)
					{
						farm.terrainFeatures.Add(new Vector2((float)x, (float)y), new HoeDirt());
					}
				}
				Building coop;
				if (farm.buildStructure("Coop", new Vector2(52f, 11f), Game1.player, out coop, false, false))
				{
					coop.daysOfConstructionLeft.Value = 0;
				}
				Building silo;
				if (farm.buildStructure("Silo", new Vector2(36f, 9f), Game1.player, out silo, false, false))
				{
					silo.daysOfConstructionLeft.Value = 0;
				}
				Building barn;
				if (farm.buildStructure("Barn", new Vector2(42f, 10f), Game1.player, out barn, false, false))
				{
					barn.daysOfConstructionLeft.Value = 0;
				}
				for (int i = 0; i < Game1.player.Items.Count; i++)
				{
					Tool tool = Game1.player.Items[i] as Tool;
					if (tool != null)
					{
						string newId = null;
						string qualifiedItemId = tool.QualifiedItemId;
						if (qualifiedItemId != null)
						{
							switch (qualifiedItemId.Length)
							{
							case 6:
							{
								char c = qualifiedItemId[3];
								if (c != 'A')
								{
									if (c != 'H')
									{
										goto IL_3D9;
									}
									if (!(qualifiedItemId == "(T)Hoe"))
									{
										goto IL_3D9;
									}
									goto IL_3C0;
								}
								else if (!(qualifiedItemId == "(T)Axe"))
								{
									goto IL_3D9;
								}
								break;
							}
							case 7:
							case 8:
							case 9:
							case 13:
							case 17:
								goto IL_3D9;
							case 10:
							{
								char c = qualifiedItemId[7];
								if (c != 'A')
								{
									if (c != 'H')
									{
										if (c != 'a')
										{
											goto IL_3D9;
										}
										if (!(qualifiedItemId == "(T)Pickaxe"))
										{
											goto IL_3D9;
										}
										goto IL_3C9;
									}
									else
									{
										if (!(qualifiedItemId == "(T)GoldHoe"))
										{
											goto IL_3D9;
										}
										goto IL_3C0;
									}
								}
								else if (!(qualifiedItemId == "(T)GoldAxe"))
								{
									goto IL_3D9;
								}
								break;
							}
							case 11:
							{
								char c = qualifiedItemId[8];
								if (c != 'A')
								{
									if (c != 'H')
									{
										goto IL_3D9;
									}
									if (!(qualifiedItemId == "(T)SteelHoe"))
									{
										goto IL_3D9;
									}
									goto IL_3C0;
								}
								else if (!(qualifiedItemId == "(T)SteelAxe"))
								{
									goto IL_3D9;
								}
								break;
							}
							case 12:
							{
								char c = qualifiedItemId[9];
								if (c != 'A')
								{
									if (c != 'H')
									{
										goto IL_3D9;
									}
									if (!(qualifiedItemId == "(T)CopperHoe"))
									{
										goto IL_3D9;
									}
									goto IL_3C0;
								}
								else if (!(qualifiedItemId == "(T)CopperAxe"))
								{
									goto IL_3D9;
								}
								break;
							}
							case 14:
							{
								char c = qualifiedItemId[3];
								if (c != 'G')
								{
									if (c != 'W')
									{
										goto IL_3D9;
									}
									if (!(qualifiedItemId == "(T)WateringCan"))
									{
										goto IL_3D9;
									}
									goto IL_3D2;
								}
								else
								{
									if (!(qualifiedItemId == "(T)GoldPickaxe"))
									{
										goto IL_3D9;
									}
									goto IL_3C9;
								}
								break;
							}
							case 15:
								if (!(qualifiedItemId == "(T)SteelPickaxe"))
								{
									goto IL_3D9;
								}
								goto IL_3C9;
							case 16:
								if (!(qualifiedItemId == "(T)CopperPickaxe"))
								{
									goto IL_3D9;
								}
								goto IL_3C9;
							case 18:
								if (!(qualifiedItemId == "(T)GoldWateringCan"))
								{
									goto IL_3D9;
								}
								goto IL_3D2;
							case 19:
								if (!(qualifiedItemId == "(T)SteelWateringCan"))
								{
									goto IL_3D9;
								}
								goto IL_3D2;
							case 20:
								if (!(qualifiedItemId == "(T)CopperWateringCan"))
								{
									goto IL_3D9;
								}
								goto IL_3D2;
							default:
								goto IL_3D9;
							}
							newId = "(T)IridiumAxe";
							goto IL_3D9;
							IL_3C0:
							newId = "(T)IridiumHoe";
							goto IL_3D9;
							IL_3C9:
							newId = "(T)IridiumPickaxe";
							goto IL_3D9;
							IL_3D2:
							newId = "(T)IridiumWateringCan";
						}
						IL_3D9:
						if (newId != null)
						{
							Tool newTool = ItemRegistry.Create<Tool>(newId, 1, 0, false);
							newTool.UpgradeFrom(newTool);
							Game1.player.Items[i] = newTool;
						}
					}
				}
				Game1.player.Money += 20000;
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)Shears", 1, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(T)MilkPail", 1, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)472", 999, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)473", 999, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)322", 999, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)388", 999, 0, false), false);
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)390", 999, 0, false), false);
			}

			// Token: 0x06003AD2 RID: 15058 RVA: 0x002DBF9F File Offset: 0x002DA19F
			public static void RemoveBuildings(string[] command, IGameLogger log)
			{
				Game1.currentLocation.buildings.Clear();
			}

			// Token: 0x06003AD3 RID: 15059 RVA: 0x002DBFB0 File Offset: 0x002DA1B0
			public static void Build(string[] command, IGameLogger log)
			{
				string buildingType;
				string error;
				int x;
				int y;
				bool forceBuild;
				if (!ArgUtility.TryGet(command, 1, out buildingType, out error, false, "string buildingType") || !ArgUtility.TryGetOptionalInt(command, 2, out x, out error, Game1.player.TilePoint.X + 1, "int x") || !ArgUtility.TryGetOptionalInt(command, 3, out y, out error, Game1.player.TilePoint.Y, "int y") || !ArgUtility.TryGetOptionalBool(command, 4, out forceBuild, out error, ArgUtility.Get(command, 0, null, true) == "ForceBuild", "bool forceBuild"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (!Game1.buildingData.ContainsKey(buildingType))
				{
					buildingType = (Game1.buildingData.Keys.FirstOrDefault((string key) => buildingType.EqualsIgnoreCase(key)) ?? buildingType);
				}
				if (!Game1.buildingData.ContainsKey(buildingType))
				{
					string[] matches = Utility.fuzzySearchAll(buildingType, Game1.buildingData.Keys, false).ToArray<string>();
					log.Warn((matches.Length == 0) ? ("There's no building with type '" + buildingType + "'.") : ("There's no building with type '" + buildingType + "'. Did you mean one of these?\n- " + string.Join("\n- ", matches)));
					return;
				}
				Building constructed;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
				if (!Game1.currentLocation.buildStructure(buildingType, new Vector2((float)x, (float)y), Game1.player, out constructed, false, forceBuild))
				{
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't place a '");
					defaultInterpolatedStringHandler.AppendFormatted(buildingType);
					defaultInterpolatedStringHandler.AppendLiteral("' building at position (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(x);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(y);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				constructed.daysOfConstructionLeft.Value = 0;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Placed '");
				defaultInterpolatedStringHandler.AppendFormatted(buildingType);
				defaultInterpolatedStringHandler.AppendLiteral("' at position (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(x);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(y);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003AD4 RID: 15060 RVA: 0x002DC1F7 File Offset: 0x002DA3F7
			public static void ForceBuild(string[] command, IGameLogger log)
			{
				if (ArgUtility.HasIndex<string>(command, 0))
				{
					command[0] = "ForceBuild";
				}
				DebugCommands.DefaultHandlers.Build(command, log);
			}

			// Token: 0x06003AD5 RID: 15061 RVA: 0x002DC214 File Offset: 0x002DA414
			[OtherNames(new string[]
			{
				"fab"
			})]
			public static void FinishAllBuilds(string[] command, IGameLogger log)
			{
				if (!Game1.IsMasterGame)
				{
					log.Error("Only the host can use this command.", null);
					return;
				}
				int count = 0;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					foreach (Building building in location.buildings)
					{
						if (building.daysOfConstructionLeft.Value > 0 || building.daysUntilUpgrade.Value > 0)
						{
							building.FinishConstruction(false);
							int count;
							count++;
							count = count;
						}
					}
					return true;
				}, true, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Finished constructing ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(count);
				defaultInterpolatedStringHandler.AppendLiteral(" building(s).");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003AD6 RID: 15062 RVA: 0x002DC294 File Offset: 0x002DA494
			public static void LocalInfo(string[] command, IGameLogger log)
			{
				int grass = 0;
				int trees = 0;
				int other = 0;
				foreach (TerrainFeature t in Game1.currentLocation.terrainFeatures.Values)
				{
					if (!(t is Grass))
					{
						if (!(t is Tree))
						{
							other++;
						}
						else
						{
							trees++;
						}
					}
					else
					{
						grass++;
					}
				}
				string summary = string.Concat(new object[]
				{
					"Grass:",
					grass,
					",  ",
					"Trees:",
					trees,
					",  ",
					"Other Terrain Features:",
					other,
					",  ",
					"Objects: ",
					Game1.currentLocation.objects.Length,
					",  ",
					"temporarySprites: ",
					Game1.currentLocation.temporarySprites.Count,
					",  "
				});
				log.Info(summary);
				Game1.drawObjectDialogue(summary);
			}

			// Token: 0x06003AD7 RID: 15063 RVA: 0x002DC3D4 File Offset: 0x002DA5D4
			[OtherNames(new string[]
			{
				"al"
			})]
			public static void AmbientLight(string[] command, IGameLogger log)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(command, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(command, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out blue, out error, "int blue"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.ambientLight = new Color(red, green, blue);
			}

			// Token: 0x06003AD8 RID: 15064 RVA: 0x002DC430 File Offset: 0x002DA630
			public static void ResetMines(string[] command, IGameLogger log)
			{
				MineShaft.permanentMineChanges.Clear();
				Game1.playSound("jingle1", null);
			}

			// Token: 0x06003AD9 RID: 15065 RVA: 0x002DC45C File Offset: 0x002DA65C
			[OtherNames(new string[]
			{
				"db"
			})]
			public static void SpeakTo(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out npcName, out error, "Pierre", false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.activeClickableMenu = new DialogueBox(Utility.fuzzyCharacterSearch(npcName, true).CurrentDialogue.Peek());
			}

			// Token: 0x06003ADA RID: 15066 RVA: 0x002DC4A5 File Offset: 0x002DA6A5
			public static void SkullKey(string[] command, IGameLogger log)
			{
				Game1.player.hasSkullKey = true;
			}

			// Token: 0x06003ADB RID: 15067 RVA: 0x002DC4B2 File Offset: 0x002DA6B2
			public static void TownKey(string[] command, IGameLogger log)
			{
				Game1.player.HasTownKey = true;
			}

			// Token: 0x06003ADC RID: 15068 RVA: 0x002DC4C0 File Offset: 0x002DA6C0
			public static void Specials(string[] command, IGameLogger log)
			{
				Game1.player.hasRustyKey = true;
				Game1.player.hasSkullKey = true;
				Game1.player.hasSpecialCharm = true;
				Game1.player.hasDarkTalisman = true;
				Game1.player.hasMagicInk = true;
				Game1.player.hasClubCard = true;
				Game1.player.canUnderstandDwarves = true;
				Game1.player.hasMagnifyingGlass = true;
				Game1.player.eventsSeen.Add("2120303");
				Game1.player.eventsSeen.Add("3910979");
				Game1.player.HasTownKey = true;
				Game1.player.stats.Set("trinketSlots", 1);
			}

			// Token: 0x06003ADD RID: 15069 RVA: 0x002DC570 File Offset: 0x002DA770
			public static void SkullGear(string[] command, IGameLogger log)
			{
				int addSlots = 36 - Game1.player.MaxItems;
				if (addSlots > 0)
				{
					Game1.player.increaseBackpackSize(addSlots);
				}
				Game1.player.hasSkullKey = true;
				Game1.player.Equip<Ring>(ItemRegistry.Create<Ring>("(O)527", 1, 0, false), Game1.player.leftRing);
				Game1.player.Equip<Ring>(ItemRegistry.Create<Ring>("(O)523", 1, 0, false), Game1.player.rightRing);
				Game1.player.Equip<Boots>(ItemRegistry.Create<Boots>("(B)514", 1, 0, false), Game1.player.boots);
				Game1.player.clearBackpack();
				Game1.player.addItemToInventory(ItemRegistry.Create("(T)IridiumPickaxe", 1, 0, false));
				Game1.player.addItemToInventory(ItemRegistry.Create("(W)4", 1, 0, false));
				Game1.player.addItemToInventory(ItemRegistry.Create("(O)226", 20, 0, false));
				Game1.player.addItemToInventory(ItemRegistry.Create("(O)288", 20, 0, false));
				Game1.player.professions.Add(24);
				Game1.player.maxHealth = 75;
			}

			// Token: 0x06003ADE RID: 15070 RVA: 0x002DC698 File Offset: 0x002DA898
			public static void ClearSpecials(string[] command, IGameLogger log)
			{
				Game1.player.hasRustyKey = false;
				Game1.player.hasSkullKey = false;
				Game1.player.hasSpecialCharm = false;
				Game1.player.hasDarkTalisman = false;
				Game1.player.hasMagicInk = false;
				Game1.player.hasClubCard = false;
				Game1.player.canUnderstandDwarves = false;
				Game1.player.hasMagnifyingGlass = false;
			}

			// Token: 0x06003ADF RID: 15071 RVA: 0x002DC700 File Offset: 0x002DA900
			public static void Tv(string[] command, IGameLogger log)
			{
				string itemId = Game1.random.Choose("(F)1466", "(F)1468");
				Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId, 1, 0, false), false);
			}

			// Token: 0x06003AE0 RID: 15072 RVA: 0x002DC738 File Offset: 0x002DA938
			[OtherNames(new string[]
			{
				"sn"
			})]
			public static void SecretNote(string[] command, IGameLogger log)
			{
				int noteId;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out noteId, out error, -1, "int noteId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.hasMagnifyingGlass = true;
				if (noteId > -1)
				{
					int whichNote = noteId;
					Object note = ItemRegistry.Create<Object>("(O)79", 1, 0, false);
					Object @object = note;
					@object.name = @object.name + " #" + whichNote.ToString();
					Game1.player.addItemToInventory(note);
					return;
				}
				Game1.player.addItemToInventory(Game1.currentLocation.tryToCreateUnseenSecretNote(Game1.player));
			}

			// Token: 0x06003AE1 RID: 15073 RVA: 0x002DC7C4 File Offset: 0x002DA9C4
			public static void Child2(string[] command, IGameLogger log)
			{
				Farmer player = Game1.player;
				List<Child> children = player.getChildren();
				if (children.Count > 1)
				{
					Child child = children[1];
					int age = child.Age;
					child.Age = age + 1;
					children[1].reloadSprite(false);
					return;
				}
				Utility.getHomeOfFarmer(player).characters.Add(new Child("Baby2", Game1.random.NextBool(), Game1.random.NextBool(), player));
			}

			// Token: 0x06003AE2 RID: 15074 RVA: 0x002DC83C File Offset: 0x002DAA3C
			[OtherNames(new string[]
			{
				"kid"
			})]
			public static void Child(string[] command, IGameLogger log)
			{
				Farmer player = Game1.player;
				List<Child> children = player.getChildren();
				if (children.Count > 0)
				{
					Child child = children[0];
					int age = child.Age;
					child.Age = age + 1;
					children[0].reloadSprite(false);
					return;
				}
				Utility.getHomeOfFarmer(player).characters.Add(new Child("Baby", Game1.random.NextBool(), Game1.random.NextBool(), player));
			}

			// Token: 0x06003AE3 RID: 15075 RVA: 0x002DC8B4 File Offset: 0x002DAAB4
			public static void KillAll(string[] command, IGameLogger log)
			{
				string safeNpcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out safeNpcName, out error, false, "string safeNpcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Func<NPC, bool> <>9__1;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (!location.Equals(Game1.currentLocation))
					{
						location.characters.Clear();
					}
					else
					{
						NetCollection<NPC> characters = location.characters;
						Func<NPC, bool> match;
						if ((match = <>9__1) == null)
						{
							match = (<>9__1 = ((NPC npc) => npc.Name != safeNpcName));
						}
						characters.RemoveWhere(match);
					}
					return true;
				}, true, false);
			}

			// Token: 0x06003AE4 RID: 15076 RVA: 0x002DC8FC File Offset: 0x002DAAFC
			public static void ResetWorldState(string[] command, IGameLogger log)
			{
				Game1.worldStateIDs.Clear();
				Game1.netWorldState.Value = new NetWorldState();
				Game1.game1.parseDebugInput("DeleteArch", log);
				Game1.player.mailReceived.Clear();
				Game1.player.eventsSeen.Clear();
				Game1.eventsSeenSinceLastLocationChange.Clear();
			}

			// Token: 0x06003AE5 RID: 15077 RVA: 0x002DC95B File Offset: 0x002DAB5B
			public static void KillAllHorses(string[] command, IGameLogger log)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location.characters.RemoveWhere((NPC npc) => npc is Horse) > 0)
					{
						Game1.playSound("drumkit0", null);
					}
					return true;
				}, true, false);
			}

			// Token: 0x06003AE6 RID: 15078 RVA: 0x002DC984 File Offset: 0x002DAB84
			public static void DatePlayer(string[] command, IGameLogger log)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer != Game1.player && farmer.isCustomized.Value)
					{
						Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID).Status = FriendshipStatus.Dating;
						break;
					}
				}
			}

			// Token: 0x06003AE7 RID: 15079 RVA: 0x002DCA08 File Offset: 0x002DAC08
			public static void EngagePlayer(string[] command, IGameLogger log)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer != Game1.player && farmer.isCustomized.Value)
					{
						Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
						friendship.Status = FriendshipStatus.Engaged;
						friendship.WeddingDate = Game1.Date;
						friendship.WeddingDate.TotalDays++;
						break;
					}
				}
			}

			// Token: 0x06003AE8 RID: 15080 RVA: 0x002DCAA8 File Offset: 0x002DACA8
			public static void MarryPlayer(string[] command, IGameLogger log)
			{
				foreach (Farmer farmer in Game1.getOnlineFarmers())
				{
					if (farmer != Game1.player && farmer.isCustomized.Value)
					{
						Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, farmer.UniqueMultiplayerID);
						friendship.Status = FriendshipStatus.Married;
						friendship.WeddingDate = Game1.Date;
						break;
					}
				}
			}

			// Token: 0x06003AE9 RID: 15081 RVA: 0x002DCB3C File Offset: 0x002DAD3C
			public static void Marry(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC spouse = Utility.fuzzyCharacterSearch(npcName, true);
				if (spouse == null)
				{
					log.Error("No character found matching '" + npcName + "'.", null);
					return;
				}
				Friendship friendship;
				if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out friendship))
				{
					friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
				}
				Game1.player.changeFriendship(2500, spouse);
				Game1.player.spouse = spouse.Name;
				friendship.WeddingDate = new WorldDate(Game1.Date);
				friendship.Status = FriendshipStatus.Married;
				Game1.prepareSpouseForWedding(Game1.player);
			}

			// Token: 0x06003AEA RID: 15082 RVA: 0x002DCC00 File Offset: 0x002DAE00
			public static void Engaged(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC spouse = Utility.fuzzyCharacterSearch(npcName, true);
				if (spouse == null)
				{
					log.Error("No character found matching '" + npcName + "'.", null);
					return;
				}
				Friendship friendship;
				if (!Game1.player.friendshipData.TryGetValue(spouse.Name, out friendship))
				{
					friendship = (Game1.player.friendshipData[spouse.Name] = new Friendship());
				}
				Game1.player.changeFriendship(2500, spouse);
				Game1.player.spouse = spouse.Name;
				friendship.Status = FriendshipStatus.Engaged;
				WorldDate weddingDate = Game1.Date;
				weddingDate.TotalDays++;
				friendship.WeddingDate = weddingDate;
			}

			// Token: 0x06003AEB RID: 15083 RVA: 0x002DCCC6 File Offset: 0x002DAEC6
			public static void ClearLightGlows(string[] command, IGameLogger log)
			{
				Game1.currentLocation.lightGlows.Clear();
			}

			// Token: 0x06003AEC RID: 15084 RVA: 0x002DCCD8 File Offset: 0x002DAED8
			[OtherNames(new string[]
			{
				"wp"
			})]
			public static void Wallpaper(string[] command, IGameLogger log)
			{
				int wallpaperId;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out wallpaperId, out error, -1, "int wallpaperId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (wallpaperId > -1)
				{
					Game1.player.addItemToInventoryBool(new Wallpaper(wallpaperId, false), false);
					return;
				}
				bool floor = Game1.random.NextBool();
				Game1.player.addItemToInventoryBool(new Wallpaper(floor ? Game1.random.Next(40) : Game1.random.Next(112), floor), false);
			}

			// Token: 0x06003AED RID: 15085 RVA: 0x002DCD53 File Offset: 0x002DAF53
			public static void ClearFurniture(string[] command, IGameLogger log)
			{
				Game1.currentLocation.furniture.Clear();
			}

			// Token: 0x06003AEE RID: 15086 RVA: 0x002DCD64 File Offset: 0x002DAF64
			[OtherNames(new string[]
			{
				"ff"
			})]
			public static void Furniture(string[] command, IGameLogger log)
			{
				string furnitureId;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out furnitureId, out error, null, false, "string furnitureId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (furnitureId == null)
				{
					Item furniture = null;
					while (furniture == null)
					{
						try
						{
							furniture = ItemRegistry.Create("(F)" + Game1.random.Next(1613).ToString(), 1, 0, false);
						}
						catch
						{
						}
					}
					Game1.player.addItemToInventoryBool(furniture, false);
					return;
				}
				Game1.player.addItemToInventoryBool(ItemRegistry.Create("(F)" + furnitureId, 1, 0, false), false);
			}

			// Token: 0x06003AEF RID: 15087 RVA: 0x002DCE08 File Offset: 0x002DB008
			public static void SpawnCoopsAndBarns(string[] command, IGameLogger log)
			{
				int count;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out count, out error, "int count"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Farm farm = Game1.currentLocation as Farm;
				if (farm != null)
				{
					for (int i = 0; i < count; i++)
					{
						for (int j = 0; j < 20; j++)
						{
							bool coop = Game1.random.NextBool();
							Building building;
							if (farm.buildStructure(coop ? "Deluxe Coop" : "Deluxe Barn", farm.getRandomTile(null), Game1.player, out building, false, false))
							{
								building.daysOfConstructionLeft.Value = 0;
								building.doAction(Utility.PointToVector2(building.animalDoor.Value) + new Vector2((float)building.tileX.Value, (float)building.tileY.Value), Game1.player);
								for (int k = 0; k < 16; k++)
								{
									Utility.addAnimalToFarm(new FarmAnimal(coop ? "White Chicken" : "Cow", (long)Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID));
								}
								break;
							}
						}
					}
				}
			}

			// Token: 0x06003AF0 RID: 15088 RVA: 0x002DCF38 File Offset: 0x002DB138
			public static void SetupFishPondFarm(string[] command, IGameLogger log)
			{
				int population;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out population, out error, 10, "int population"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.game1.parseDebugInput("ClearFarm", log);
				for (int x = 4; x < 77; x += 6)
				{
					for (int y = 9; y < 60; y += 6)
					{
						Game1 game = Game1.game1;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 3);
						defaultInterpolatedStringHandler.AppendFormatted("Build");
						defaultInterpolatedStringHandler.AppendLiteral(" \"Fish Pond\" ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(x);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(y);
						game.parseDebugInput(defaultInterpolatedStringHandler.ToStringAndClear(), log);
					}
				}
				foreach (Building building in Game1.getFarm().buildings)
				{
					FishPond fishPond = building as FishPond;
					if (fishPond != null)
					{
						int fish = Game1.random.Next(128, 159);
						if (Game1.random.NextDouble() < 0.15)
						{
							fish = Game1.random.Next(698, 724);
						}
						if (Game1.random.NextDouble() < 0.05)
						{
							fish = Game1.random.Next(796, 801);
						}
						ParsedItemData data = ItemRegistry.GetData(fish.ToString());
						if (data != null && data.Category == -4)
						{
							fishPond.fishType.Value = fish.ToString();
						}
						else
						{
							fishPond.fishType.Value = Game1.random.Choose("393", "397");
						}
						fishPond.maxOccupants.Value = 10;
						fishPond.currentOccupants.Value = population;
						fishPond.GetFishObject();
					}
				}
				Game1.game1.parseDebugInput("DayUpdate 1", log);
			}

			// Token: 0x06003AF1 RID: 15089 RVA: 0x002DD138 File Offset: 0x002DB338
			public static void Grass(string[] command, IGameLogger log)
			{
				GameLocation location = Game1.currentLocation;
				if (location == null)
				{
					return;
				}
				for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
					{
						if (location.CanItemBePlacedHere(new Vector2((float)x, (float)y), true, CollisionMask.All, CollisionMask.None, false, false))
						{
							location.terrainFeatures.Add(new Vector2((float)x, (float)y), new Grass(1, 4));
						}
					}
				}
			}

			// Token: 0x06003AF2 RID: 15090 RVA: 0x002DD1C8 File Offset: 0x002DB3C8
			public static void SetupBigFarm(string[] command, IGameLogger log)
			{
				Farm farm = Game1.getFarm();
				Game1.game1.parseDebugInput("ClearFarm", log);
				Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 4 9", log);
				Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 10 9", log);
				Game1.game1.parseDebugInput("Build \"Deluxe Coop\" 36 11", log);
				Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 16 9", log);
				Game1.game1.parseDebugInput("Build \"Deluxe Barn\" 3 16", log);
				Game1.game1.parseDebugInput("Build Mill 30 20", log);
				Game1.game1.parseDebugInput("Build Stable 46 10", log);
				Game1.game1.parseDebugInput("Build Silo 54 14", log);
				Game1.game1.parseDebugInput("Build \"Junimo Hut\" 48 52", log);
				Game1.game1.parseDebugInput("Build \"Junimo Hut\" 55 52", log);
				Game1.game1.parseDebugInput("Build \"Junimo Hut\" 59 52", log);
				Game1.game1.parseDebugInput("Build \"Junimo Hut\" 65 52", log);
				foreach (Building building in farm.buildings)
				{
					AnimalHouse animalHouse = building.GetIndoors() as AnimalHouse;
					if (animalHouse != null)
					{
						BuildingData buildingData = building.GetData();
						string[] validAnimalKeys = (from p in Game1.farmAnimalData
						where p.Value.House != null && buildingData.ValidOccupantTypes.Contains(p.Value.House)
						select p.Key).ToArray<string>();
						int i = 0;
						while (i < animalHouse.animalLimit.Value && !animalHouse.isFull())
						{
							FarmAnimal animal = new FarmAnimal(Game1.random.ChooseFrom(validAnimalKeys), (long)Game1.random.Next(int.MaxValue), Game1.player.UniqueMultiplayerID);
							if (Game1.random.NextBool())
							{
								animal.growFully(null);
							}
							animalHouse.adoptAnimal(animal);
							i++;
						}
					}
				}
				foreach (Building building2 in farm.buildings)
				{
					building2.doAction(Utility.PointToVector2(building2.animalDoor.Value) + new Vector2((float)building2.tileX.Value, (float)building2.tileY.Value), Game1.player);
				}
				for (int x = 11; x < 23; x++)
				{
					for (int y = 14; y < 25; y++)
					{
						farm.terrainFeatures.Add(new Vector2((float)x, (float)y), new Grass(1, 4));
					}
				}
				for (int x2 = 3; x2 < 23; x2++)
				{
					for (int y2 = 57; y2 < 61; y2++)
					{
						farm.terrainFeatures.Add(new Vector2((float)x2, (float)y2), new Grass(1, 4));
					}
				}
				for (int y3 = 17; y3 < 25; y3++)
				{
					farm.terrainFeatures.Add(new Vector2(64f, (float)y3), new Flooring("6"));
				}
				for (int x3 = 35; x3 < 64; x3++)
				{
					farm.terrainFeatures.Add(new Vector2((float)x3, 24f), new Flooring("6"));
				}
				for (int x4 = 38; x4 < 76; x4++)
				{
					for (int y4 = 18; y4 < 52; y4++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x4, (float)y4), true, CollisionMask.All, CollisionMask.None, false, false))
						{
							HoeDirt dirt = new HoeDirt();
							farm.terrainFeatures.Add(new Vector2((float)x4, (float)y4), dirt);
							dirt.plant((472 + Game1.random.Next(5)).ToString(), Game1.player, false);
						}
					}
				}
				Game1.game1.parseDebugInput("GrowCrops 8", log);
				Vector2[] array = new Vector2[]
				{
					new Vector2(8f, 25f),
					new Vector2(11f, 25f),
					new Vector2(14f, 25f),
					new Vector2(17f, 25f),
					new Vector2(20f, 25f),
					new Vector2(23f, 25f),
					new Vector2(8f, 28f),
					new Vector2(11f, 28f),
					new Vector2(14f, 28f),
					new Vector2(17f, 28f),
					new Vector2(20f, 28f),
					new Vector2(23f, 28f),
					new Vector2(8f, 31f),
					new Vector2(11f, 31f),
					new Vector2(14f, 31f),
					new Vector2(17f, 31f),
					new Vector2(20f, 31f),
					new Vector2(23f, 31f)
				};
				NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = farm.terrainFeatures;
				foreach (Vector2 tile in array)
				{
					terrainFeatures.Add(tile, new FruitTree((628 + Game1.random.Next(2)).ToString(), 4));
				}
				for (int x5 = 3; x5 < 15; x5++)
				{
					for (int y5 = 36; y5 < 45; y5++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x5, (float)y5), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							Object keg = ItemRegistry.Create<Object>("(BC)12", 1, 0, false);
							farm.objects.Add(new Vector2((float)x5, (float)y5), keg);
							keg.performObjectDropInAction(ItemRegistry.Create<Object>("(O)454", 1, 0, false), false, Game1.player, false);
						}
					}
				}
				for (int x6 = 16; x6 < 26; x6++)
				{
					for (int y6 = 36; y6 < 45; y6++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x6, (float)y6), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							farm.objects.Add(new Vector2((float)x6, (float)y6), ItemRegistry.Create<Object>("(BC)13", 1, 0, false));
						}
					}
				}
				for (int x7 = 3; x7 < 15; x7++)
				{
					for (int y7 = 47; y7 < 57; y7++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x7, (float)y7), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							farm.objects.Add(new Vector2((float)x7, (float)y7), ItemRegistry.Create<Object>("(BC)16", 1, 0, false));
						}
					}
				}
				for (int x8 = 16; x8 < 26; x8++)
				{
					for (int y8 = 47; y8 < 57; y8++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x8, (float)y8), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							farm.objects.Add(new Vector2((float)x8, (float)y8), ItemRegistry.Create<Object>("(BC)15", 1, 0, false));
						}
					}
				}
				for (int x9 = 28; x9 < 38; x9++)
				{
					for (int y9 = 26; y9 < 46; y9++)
					{
						if (farm.CanItemBePlacedHere(new Vector2((float)x9, (float)y9), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							new Torch().placementAction(farm, x9 * 64, y9 * 64, null);
						}
					}
				}
			}

			// Token: 0x06003AF3 RID: 15091 RVA: 0x002DD9D8 File Offset: 0x002DBBD8
			[OtherNames(new string[]
			{
				"hu",
				"house"
			})]
			public static void HouseUpgrade(string[] command, IGameLogger log)
			{
				int upgradeLevel;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out upgradeLevel, out error, "int upgradeLevel"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Utility.getHomeOfFarmer(Game1.player).moveObjectsForHouseUpgrade(upgradeLevel);
				Utility.getHomeOfFarmer(Game1.player).setMapForUpgradeLevel(upgradeLevel);
				Game1.player.HouseUpgradeLevel = upgradeLevel;
				Game1.addNewFarmBuildingMaps();
				Utility.getHomeOfFarmer(Game1.player).ReadWallpaperAndFloorTileData();
				Utility.getHomeOfFarmer(Game1.player).RefreshFloorObjectNeighbors();
			}

			// Token: 0x06003AF4 RID: 15092 RVA: 0x002DDA50 File Offset: 0x002DBC50
			[OtherNames(new string[]
			{
				"thu",
				"thishouse"
			})]
			public static void ThisHouseUpgrade(string[] command, IGameLogger log)
			{
				int upgradeLevel;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out upgradeLevel, out error, "int upgradeLevel"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation currentLocation = Game1.currentLocation;
				object obj;
				if (currentLocation == null)
				{
					obj = null;
				}
				else
				{
					Building buildingAt = currentLocation.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f));
					obj = ((buildingAt != null) ? buildingAt.GetIndoors() : null);
				}
				FarmHouse house = (obj as FarmHouse) ?? (Game1.currentLocation as FarmHouse);
				if (house != null)
				{
					house.moveObjectsForHouseUpgrade(upgradeLevel);
					house.setMapForUpgradeLevel(upgradeLevel);
					house.upgradeLevel = upgradeLevel;
					Game1.addNewFarmBuildingMaps();
					house.ReadWallpaperAndFloorTileData();
					house.RefreshFloorObjectNeighbors();
				}
			}

			// Token: 0x06003AF5 RID: 15093 RVA: 0x002DDAF0 File Offset: 0x002DBCF0
			[OtherNames(new string[]
			{
				"ci"
			})]
			public static void Clear(string[] command, IGameLogger log)
			{
				Game1.player.clearBackpack();
			}

			// Token: 0x06003AF6 RID: 15094 RVA: 0x002DDAFC File Offset: 0x002DBCFC
			[OtherNames(new string[]
			{
				"w"
			})]
			public static void Wall(string[] command, IGameLogger log)
			{
				string wallpaperId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out wallpaperId, out error, false, "string wallpaperId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<FarmHouse>("FarmHouse", false).SetWallpaper(wallpaperId, null);
			}

			// Token: 0x06003AF7 RID: 15095 RVA: 0x002DDB38 File Offset: 0x002DBD38
			public static void Floor(string[] command, IGameLogger log)
			{
				string floorId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out floorId, out error, false, "string floorId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<FarmHouse>("FarmHouse", false).SetFloor(floorId, null);
			}

			// Token: 0x06003AF8 RID: 15096 RVA: 0x002DDB74 File Offset: 0x002DBD74
			public static void Sprinkle(string[] command, IGameLogger log)
			{
				Utility.addSprinklesToLocation(Game1.currentLocation, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 7, 7, 2000, 100, Color.White, null, false);
			}

			// Token: 0x06003AF9 RID: 15097 RVA: 0x002DDBB9 File Offset: 0x002DBDB9
			public static void ClearMail(string[] command, IGameLogger log)
			{
				Game1.player.mailReceived.Clear();
			}

			// Token: 0x06003AFA RID: 15098 RVA: 0x002DDBCC File Offset: 0x002DBDCC
			public static void BroadcastMailbox(string[] command, IGameLogger log)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out mailId, out error, false, "string mailId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.addMail(mailId, false, true);
			}

			// Token: 0x06003AFB RID: 15099 RVA: 0x002DDC00 File Offset: 0x002DBE00
			[OtherNames(new string[]
			{
				"mft"
			})]
			public static void MailForTomorrow(string[] command, IGameLogger log)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out mailId, out error, false, "string mailId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.addMailForTomorrow(mailId, command.Length > 2, false);
			}

			// Token: 0x06003AFC RID: 15100 RVA: 0x002DDC38 File Offset: 0x002DBE38
			public static void AllMail(string[] command, IGameLogger log)
			{
				foreach (string mailName in DataLoader.Mail(Game1.content).Keys)
				{
					Game1.addMailForTomorrow(mailName, false, false);
				}
			}

			// Token: 0x06003AFD RID: 15101 RVA: 0x002DDC94 File Offset: 0x002DBE94
			public static void AllMailRead(string[] command, IGameLogger log)
			{
				foreach (string key in DataLoader.Mail(Game1.content).Keys)
				{
					Game1.player.mailReceived.Add(key);
				}
			}

			// Token: 0x06003AFE RID: 15102 RVA: 0x002DDCFC File Offset: 0x002DBEFC
			public static void ShowMail(string[] command, IGameLogger log)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out mailId, out error, false, "string mailId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.activeClickableMenu = new LetterViewerMenu(DataLoader.Mail(Game1.content).GetValueOrDefault(mailId, ""), mailId, false);
			}

			// Token: 0x06003AFF RID: 15103 RVA: 0x002DDD48 File Offset: 0x002DBF48
			[OtherNames(new string[]
			{
				"where"
			})]
			public static void WhereIs(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				List<string> lines = new List<string>();
				if (Game1.CurrentEvent != null)
				{
					foreach (NPC npc in Game1.CurrentEvent.actors)
					{
						if (Utility.fuzzyCompare(npcName, npc.Name) != null)
						{
							List<string> lines3 = lines;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 3);
							defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" is in this event at (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(npc.TilePoint.X);
							defaultInterpolatedStringHandler.AppendLiteral(", ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(npc.TilePoint.Y);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							lines3.Add(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				Utility.ForEachCharacter(delegate(NPC character)
				{
					if (Utility.fuzzyCompare(npcName, character.Name) != null)
					{
						List<string> lines2 = lines;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 5);
						defaultInterpolatedStringHandler2.AppendLiteral("'");
						defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("'");
						defaultInterpolatedStringHandler2.AppendFormatted(character.EventActor ? " (event actor)" : "");
						defaultInterpolatedStringHandler2.AppendLiteral(" is at ");
						defaultInterpolatedStringHandler2.AppendFormatted(character.currentLocation.NameOrUniqueName);
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(character.TilePoint.X);
						defaultInterpolatedStringHandler2.AppendLiteral(", ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(character.TilePoint.Y);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						lines2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					return true;
				}, true);
				if (lines.Any<string>())
				{
					log.Info(string.Join("\n", lines));
					return;
				}
				log.Error("No NPC found matching '" + npcName + "'.", null);
			}

			// Token: 0x06003B00 RID: 15104 RVA: 0x002DDEB8 File Offset: 0x002DC0B8
			[OtherNames(new string[]
			{
				"whereItem"
			})]
			public static void WhereIsItem(string[] command, IGameLogger log)
			{
				DebugCommands.DefaultHandlers.<>c__DisplayClass129_0 CS$<>8__locals1 = new DebugCommands.DefaultHandlers.<>c__DisplayClass129_0();
				string error;
				if (!ArgUtility.TryGet(command, 1, out CS$<>8__locals1.itemNameOrId, out error, false, "string itemNameOrId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				DebugCommands.DefaultHandlers.<>c__DisplayClass129_0 CS$<>8__locals2 = CS$<>8__locals1;
				ParsedItemData data = ItemRegistry.GetData(CS$<>8__locals1.itemNameOrId);
				CS$<>8__locals2.itemId = ((data != null) ? data.QualifiedItemId : null);
				CS$<>8__locals1.lines = new List<string>();
				CS$<>8__locals1.count = 0L;
				Utility.ForEachItemContext(delegate(in ForEachItemContext context)
				{
					Item item = context.Item;
					if ((CS$<>8__locals1.itemId != null) ? (item.QualifiedItemId == CS$<>8__locals1.itemId) : (Utility.fuzzyCompare(CS$<>8__locals1.itemNameOrId, item.Name) != null || Utility.fuzzyCompare(CS$<>8__locals1.itemNameOrId, item.DisplayName) != null))
					{
						CS$<>8__locals1.count += (long)Math.Min(item.Stack, 1);
						List<string> lines = CS$<>8__locals1.lines;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(7, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("  - ");
						defaultInterpolatedStringHandler2.AppendFormatted(string.Join(" > ", context.GetDisplayPath(true)));
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted(item.QualifiedItemId);
						string value;
						if (item.Stack <= 1)
						{
							value = "";
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 1);
							defaultInterpolatedStringHandler3.AppendLiteral(" x ");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(item.Stack);
							value = defaultInterpolatedStringHandler3.ToStringAndClear();
						}
						defaultInterpolatedStringHandler2.AppendFormatted(value);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						lines.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					return true;
				});
				string label = (CS$<>8__locals1.itemId != null) ? ("ID '" + CS$<>8__locals1.itemId + "'") : ("name '" + CS$<>8__locals1.itemNameOrId + "'");
				if (CS$<>8__locals1.lines.Any<string>())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Found ");
					defaultInterpolatedStringHandler.AppendFormatted<long>(CS$<>8__locals1.count);
					defaultInterpolatedStringHandler.AppendLiteral(" item");
					defaultInterpolatedStringHandler.AppendFormatted((CS$<>8__locals1.count > 1L) ? "s" : "");
					defaultInterpolatedStringHandler.AppendLiteral(" matching ");
					defaultInterpolatedStringHandler.AppendFormatted(label);
					defaultInterpolatedStringHandler.AppendLiteral(":\n");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("\n", CS$<>8__locals1.lines));
					log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				log.Error("No item found matching " + label + ".", null);
			}

			// Token: 0x06003B01 RID: 15105 RVA: 0x002DE01C File Offset: 0x002DC21C
			[OtherNames(new string[]
			{
				"pm"
			})]
			public static void PanMode(string[] command, IGameLogger log)
			{
				string option;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out option, out error, null, false, "string option"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (option == null)
				{
					if (!Game1.panMode)
					{
						Game1.panMode = true;
						Game1.viewportFreeze = true;
						Game1.debugMode = true;
						Game1.game1.panFacingDirectionWait = false;
						Game1.game1.panModeString = "";
						log.Info("Screen pan mode enabled.");
						return;
					}
					Game1.panMode = false;
					Game1.viewportFreeze = false;
					Game1.game1.panModeString = "";
					Game1.debugMode = false;
					Game1.game1.panFacingDirectionWait = false;
					Game1.inputSimulator = null;
					log.Info("Screen pan mode disabled.");
					return;
				}
				else
				{
					if (!Game1.panMode)
					{
						log.Error("Screen pan mode isn't enabled. You can enable it by using this command without arguments.", null);
						return;
					}
					if (option == "clear")
					{
						Game1.game1.panModeString = "";
						Game1.game1.panFacingDirectionWait = false;
						return;
					}
					int time;
					string text;
					if (ArgUtility.TryGetInt(command, 1, out time, out text, "int time"))
					{
						if (!Game1.game1.panFacingDirectionWait)
						{
							Game1 game = Game1.game1;
							game.panModeString = game.panModeString + ((Game1.game1.panModeString.Length > 0) ? "/" : "") + time.ToString() + " ";
							log.Info(Game1.game1.panModeString + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3191"));
						}
						return;
					}
					DebugCommands.LogArgError(log, command, "the first argument must be omitted (to toggle pan mode), 'clear', or a numeric time");
					return;
				}
			}

			// Token: 0x06003B02 RID: 15106 RVA: 0x002DE194 File Offset: 0x002DC394
			[OtherNames(new string[]
			{
				"is"
			})]
			public static void InputSim(string[] command, IGameLogger log)
			{
				string option;
				string error;
				if (!ArgUtility.TryGet(command, 1, out option, out error, false, "string option"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.inputSimulator = null;
				string a = option.ToLower();
				if (a == "spamtool")
				{
					Game1.inputSimulator = new ToolSpamInputSimulator();
					return;
				}
				if (!(a == "spamlr"))
				{
					log.Error("No input simulator found for " + option, null);
					return;
				}
				Game1.inputSimulator = new LeftRightClickSpamInputSimulator();
			}

			// Token: 0x06003B03 RID: 15107 RVA: 0x002DE210 File Offset: 0x002DC410
			public static void Hurry(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Utility.fuzzyCharacterSearch(npcName, true).warpToPathControllerDestination();
			}

			// Token: 0x06003B04 RID: 15108 RVA: 0x002DE248 File Offset: 0x002DC448
			public static void MorePollen(string[] command, IGameLogger log)
			{
				int amount;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out amount, out error, "int amount"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				for (int i = 0; i < amount; i++)
				{
					Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
				}
			}

			// Token: 0x06003B05 RID: 15109 RVA: 0x002DE318 File Offset: 0x002DC518
			public static void FillWithObject(string[] command, IGameLogger log)
			{
				string id;
				string error;
				bool bigCraftable;
				if (!ArgUtility.TryGet(command, 1, out id, out error, false, "string id") || !ArgUtility.TryGetOptionalBool(command, 2, out bigCraftable, out error, false, "bool bigCraftable"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				for (int y = 0; y < Game1.currentLocation.map.Layers[0].LayerHeight; y++)
				{
					for (int x = 0; x < Game1.currentLocation.map.Layers[0].LayerWidth; x++)
					{
						Vector2 loc = new Vector2((float)x, (float)y);
						if (Game1.currentLocation.CanItemBePlacedHere(loc, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
						{
							string typeId = bigCraftable ? "(BC)" : "(O)";
							Game1.currentLocation.setObject(loc, ItemRegistry.Create<Object>(typeId + id, 1, 0, false));
						}
					}
				}
			}

			// Token: 0x06003B06 RID: 15110 RVA: 0x002DE3FC File Offset: 0x002DC5FC
			public static void SpawnWeeds(string[] command, IGameLogger log)
			{
				int spawnPasses;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out spawnPasses, out error, "int spawnPasses"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				for (int i = 0; i < spawnPasses; i++)
				{
					Game1.currentLocation.spawnWeedsAndStones(1, false, true);
				}
			}

			// Token: 0x06003B07 RID: 15111 RVA: 0x002DE43D File Offset: 0x002DC63D
			public static void BusDriveBack(string[] command, IGameLogger log)
			{
				Game1.RequireLocation<BusStop>("BusStop", false).busDriveBack();
			}

			// Token: 0x06003B08 RID: 15112 RVA: 0x002DE44F File Offset: 0x002DC64F
			public static void BusDriveOff(string[] command, IGameLogger log)
			{
				Game1.RequireLocation<BusStop>("BusStop", false).busDriveOff();
			}

			// Token: 0x06003B09 RID: 15113 RVA: 0x002DE464 File Offset: 0x002DC664
			public static void CompleteJoja(string[] command, IGameLogger log)
			{
				Game1.player.mailReceived.Add("ccCraftsRoom");
				Game1.player.mailReceived.Add("ccVault");
				Game1.player.mailReceived.Add("ccFishTank");
				Game1.player.mailReceived.Add("ccBoilerRoom");
				Game1.player.mailReceived.Add("ccPantry");
				Game1.player.mailReceived.Add("jojaCraftsRoom");
				Game1.player.mailReceived.Add("jojaVault");
				Game1.player.mailReceived.Add("jojaFishTank");
				Game1.player.mailReceived.Add("jojaBoilerRoom");
				Game1.player.mailReceived.Add("jojaPantry");
				Game1.player.mailReceived.Add("JojaMember");
			}

			// Token: 0x06003B0A RID: 15114 RVA: 0x002DE558 File Offset: 0x002DC758
			public static void CompleteCc(string[] command, IGameLogger log)
			{
				Game1.player.mailReceived.Add("ccCraftsRoom");
				Game1.player.mailReceived.Add("ccVault");
				Game1.player.mailReceived.Add("ccFishTank");
				Game1.player.mailReceived.Add("ccBoilerRoom");
				Game1.player.mailReceived.Add("ccPantry");
				Game1.player.mailReceived.Add("ccBulletin");
				Game1.player.mailReceived.Add("ccBoilerRoom");
				Game1.player.mailReceived.Add("ccPantry");
				Game1.player.mailReceived.Add("ccBulletin");
				CommunityCenter ccc = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
				for (int i = 0; i < ccc.areasComplete.Count; i++)
				{
					ccc.markAreaAsComplete(i);
					ccc.areasComplete[i] = true;
				}
			}

			// Token: 0x06003B0B RID: 15115 RVA: 0x002DE658 File Offset: 0x002DC858
			public static void Break(string[] command, IGameLogger log)
			{
			}

			// Token: 0x06003B0C RID: 15116 RVA: 0x002DE65A File Offset: 0x002DC85A
			public static void WhereOre(string[] command, IGameLogger log)
			{
				log.Info(Convert.ToString(Game1.currentLocation.orePanPoint.Value));
			}

			// Token: 0x06003B0D RID: 15117 RVA: 0x002DE67C File Offset: 0x002DC87C
			public static void AllBundles(string[] command, IGameLogger log)
			{
				foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundles.FieldDict)
				{
					for (int i = 0; i < b.Value.Count; i++)
					{
						b.Value[i] = true;
					}
				}
				Game1.playSound("crystal", new int?(0));
			}

			// Token: 0x06003B0E RID: 15118 RVA: 0x002DE710 File Offset: 0x002DC910
			public static void JunimoGoodbye(string[] command, IGameLogger log)
			{
				CommunityCenter communityCenter = Game1.currentLocation as CommunityCenter;
				if (communityCenter == null)
				{
					log.Error("The JunimoGoodbye command must be run while inside the community center.", null);
					return;
				}
				communityCenter.junimoGoodbyeDance();
			}

			// Token: 0x06003B0F RID: 15119 RVA: 0x002DE740 File Offset: 0x002DC940
			public static void Bundle(string[] command, IGameLogger log)
			{
				int bundleKey;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out bundleKey, out error, "int bundleKey"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				foreach (KeyValuePair<int, NetArray<bool, NetBool>> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundles.FieldDict)
				{
					if (b.Key == bundleKey)
					{
						for (int i = 0; i < b.Value.Count; i++)
						{
							b.Value[i] = true;
						}
					}
				}
				Game1.playSound("crystal", new int?(0));
			}

			// Token: 0x06003B10 RID: 15120 RVA: 0x002DE7FC File Offset: 0x002DC9FC
			[OtherNames(new string[]
			{
				"lu"
			})]
			public static void Lookup(string[] command, IGameLogger log)
			{
				string search;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out search, out error, ' ', "string search"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					if (item.InternalName.EqualsIgnoreCase(search))
					{
						log.Info(item.InternalName + " " + item.ItemId);
					}
				}
			}

			// Token: 0x06003B11 RID: 15121 RVA: 0x002DE890 File Offset: 0x002DCA90
			public static void CcLoadCutscene(string[] command, IGameLogger log)
			{
				int areaId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out areaId, out error, "int areaId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).restoreAreaCutscene(areaId);
			}

			// Token: 0x06003B12 RID: 15122 RVA: 0x002DE8CC File Offset: 0x002DCACC
			public static void CcLoad(string[] command, IGameLogger log)
			{
				int areaId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out areaId, out error, "int areaId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).loadArea(areaId, true);
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).markAreaAsComplete(areaId);
			}

			// Token: 0x06003B13 RID: 15123 RVA: 0x002DE917 File Offset: 0x002DCB17
			public static void Plaque(string[] command, IGameLogger log)
			{
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).addStarToPlaque();
			}

			// Token: 0x06003B14 RID: 15124 RVA: 0x002DE92C File Offset: 0x002DCB2C
			public static void JunimoStar(string[] command, IGameLogger log)
			{
				CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
				Junimo junimo = communityCenter.characters.OfType<Junimo>().FirstOrDefault<Junimo>();
				if (junimo == null)
				{
					log.Error("No Junimo found in the community center.", null);
					return;
				}
				junimo.returnToJunimoHutToFetchStar(communityCenter);
			}

			// Token: 0x06003B15 RID: 15125 RVA: 0x002DE970 File Offset: 0x002DCB70
			[OtherNames(new string[]
			{
				"j",
				"aj"
			})]
			public static void AddJunimo(string[] command, IGameLogger log)
			{
				Vector2 tile;
				string error;
				int areaId;
				if (!ArgUtility.TryGetVector2(command, 1, out tile, out error, true, "Vector2 tile") || !ArgUtility.TryGetInt(command, 3, out areaId, out error, "int areaId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).addCharacter(new Junimo(tile * 64f, areaId, false));
			}

			// Token: 0x06003B16 RID: 15126 RVA: 0x002DE9D0 File Offset: 0x002DCBD0
			public static void ResetJunimoNotes(string[] command, IGameLogger log)
			{
				foreach (NetArray<bool, NetBool> b in Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundles.FieldDict.Values)
				{
					for (int i = 0; i < b.Count; i++)
					{
						b[i] = false;
					}
				}
			}

			// Token: 0x06003B17 RID: 15127 RVA: 0x002DEA4C File Offset: 0x002DCC4C
			[OtherNames(new string[]
			{
				"jn"
			})]
			public static void JunimoNote(string[] command, IGameLogger log)
			{
				int areaId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out areaId, out error, "int areaId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).addJunimoNote(areaId);
			}

			// Token: 0x06003B18 RID: 15128 RVA: 0x002DEA88 File Offset: 0x002DCC88
			public static void WaterColor(string[] command, IGameLogger log)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(command, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(command, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out blue, out error, "int blue"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.waterColor.Value = new Color(red, green, blue) * 0.5f;
			}

			// Token: 0x06003B19 RID: 15129 RVA: 0x002DEAF8 File Offset: 0x002DCCF8
			public static void FestivalScore(string[] command, IGameLogger log)
			{
				int score;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out score, out error, "int score"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.festivalScore += score;
			}

			// Token: 0x06003B1A RID: 15130 RVA: 0x002DEB34 File Offset: 0x002DCD34
			public static void AddOtherFarmer(string[] command, IGameLogger log)
			{
				Farmer f = new Farmer(new FarmerSprite("Characters\\Farmer\\farmer_base"), new Vector2(Game1.player.Position.X - 64f, Game1.player.Position.Y), 2, Dialogue.randomName(), null, true);
				f.changeShirt(Game1.random.Next(1000, 1040).ToString());
				f.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
				f.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
				if (Game1.random.NextBool())
				{
					f.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
				}
				else
				{
					Game1.player.changeHat(-1);
				}
				f.changeHairColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
				f.changeSkinColor(Game1.random.Next(16), false);
				f.currentLocation = Game1.currentLocation;
				Game1.otherFarmers.Add((long)Game1.random.Next(), f);
			}

			// Token: 0x06003B1B RID: 15131 RVA: 0x002DECA4 File Offset: 0x002DCEA4
			public static void PlayMusic(string[] command, IGameLogger log)
			{
				string trackName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out trackName, out error, false, "string trackName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.changeMusicTrack(trackName, false, MusicContext.Default);
			}

			// Token: 0x06003B1C RID: 15132 RVA: 0x002DECD8 File Offset: 0x002DCED8
			public static void Jump(string[] command, IGameLogger log)
			{
				string target;
				string error;
				float jumpVelocity;
				if (!ArgUtility.TryGet(command, 1, out target, out error, false, "string target") || !ArgUtility.TryGetOptionalFloat(command, 2, out jumpVelocity, out error, 8f, "float jumpVelocity"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (target == "farmer")
				{
					Game1.player.jump(jumpVelocity);
					return;
				}
				Utility.fuzzyCharacterSearch(target, true).jump(jumpVelocity);
			}

			// Token: 0x06003B1D RID: 15133 RVA: 0x002DED40 File Offset: 0x002DCF40
			public static void Toss(string[] command, IGameLogger log)
			{
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 2700f, 1, 0, Game1.player.Tile * 64f, false, false)
				{
					rotationChange = 0.09817477f,
					motion = new Vector2(0f, -6f),
					acceleration = new Vector2(0f, 0.08f)
				});
			}

			// Token: 0x06003B1E RID: 15134 RVA: 0x002DEDB8 File Offset: 0x002DCFB8
			public static void Rain(string[] command, IGameLogger log)
			{
				string contextId = Game1.player.currentLocation.GetLocationContextId();
				LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
				weather.IsRaining = !weather.IsRaining;
				weather.IsDebrisWeather = false;
				if (contextId == "Default")
				{
					Game1.isRaining = weather.IsRaining;
					Game1.isDebrisWeather = false;
				}
			}

			// Token: 0x06003B1F RID: 15135 RVA: 0x002DEE1C File Offset: 0x002DD01C
			public static void GreenRain(string[] command, IGameLogger log)
			{
				string contextId = Game1.player.currentLocation.GetLocationContextId();
				LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
				weather.IsGreenRain = !weather.IsGreenRain;
				weather.IsDebrisWeather = false;
				if (contextId == "Default")
				{
					Game1.isRaining = weather.IsRaining;
					Game1.isGreenRain = weather.IsGreenRain;
					Game1.isDebrisWeather = false;
				}
			}

			// Token: 0x06003B20 RID: 15136 RVA: 0x002DEE8C File Offset: 0x002DD08C
			[OtherNames(new string[]
			{
				"sf"
			})]
			public static void SetFrame(string[] command, IGameLogger log)
			{
				int animationId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out animationId, out error, "int animationId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.FarmerSprite.PauseForSingleAnimation = true;
				Game1.player.FarmerSprite.setCurrentSingleAnimation(animationId);
			}

			// Token: 0x06003B21 RID: 15137 RVA: 0x002DEED4 File Offset: 0x002DD0D4
			[OtherNames(new string[]
			{
				"ee"
			})]
			public static void EndEvent(string[] command, IGameLogger log)
			{
				Event @event = Game1.CurrentEvent;
				if (@event == null)
				{
					log.Warn("Can't end an event because there's none playing.");
					return;
				}
				if (@event.id == "1590166")
				{
					Game1.player.mailReceived.Add("rejectedPet");
				}
				@event.skipped = true;
				@event.skipEvent();
			}

			// Token: 0x06003B22 RID: 15138 RVA: 0x002DEF2A File Offset: 0x002DD12A
			public static void Language(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new LanguageSelectionMenu();
			}

			// Token: 0x06003B23 RID: 15139 RVA: 0x002DEF36 File Offset: 0x002DD136
			[OtherNames(new string[]
			{
				"rte"
			})]
			public static void RunTestEvent(string[] command, IGameLogger log)
			{
				Game1.runTestEvent();
			}

			// Token: 0x06003B24 RID: 15140 RVA: 0x002DEF3D File Offset: 0x002DD13D
			[OtherNames(new string[]
			{
				"qb"
			})]
			public static void QiBoard(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new SpecialOrdersBoard("Qi");
			}

			// Token: 0x06003B25 RID: 15141 RVA: 0x002DEF4E File Offset: 0x002DD14E
			[OtherNames(new string[]
			{
				"ob"
			})]
			public static void OrdersBoard(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new SpecialOrdersBoard("");
			}

			// Token: 0x06003B26 RID: 15142 RVA: 0x002DEF5F File Offset: 0x002DD15F
			public static void ReturnedDonations(string[] command, IGameLogger log)
			{
				Game1.player.team.CheckReturnedDonations();
			}

			// Token: 0x06003B27 RID: 15143 RVA: 0x002DEF70 File Offset: 0x002DD170
			[OtherNames(new string[]
			{
				"cso"
			})]
			public static void CompleteSpecialOrders(string[] command, IGameLogger log)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					foreach (OrderObjective orderObjective in specialOrder.objectives)
					{
						orderObjective.SetCount(orderObjective.maxCount.Value);
					}
				}
			}

			// Token: 0x06003B28 RID: 15144 RVA: 0x002DF010 File Offset: 0x002DD210
			public static void SpecialOrder(string[] command, IGameLogger log)
			{
				string orderId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out orderId, out error, false, "string orderId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.team.AddSpecialOrder(orderId, null, false);
			}

			// Token: 0x06003B29 RID: 15145 RVA: 0x002DF053 File Offset: 0x002DD253
			public static void BoatJourney(string[] command, IGameLogger log)
			{
				Game1.currentMinigame = new BoatJourney();
			}

			// Token: 0x06003B2A RID: 15146 RVA: 0x002DF060 File Offset: 0x002DD260
			public static void Minigame(string[] command, IGameLogger log)
			{
				string minigame;
				string error;
				if (!ArgUtility.TryGet(command, 1, out minigame, out error, false, "string minigame"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (minigame != null)
				{
					switch (minigame.Length)
					{
					case 5:
					{
						char c = minigame[0];
						if (c != 'i')
						{
							if (c != 'p')
							{
								if (c != 's')
								{
									return;
								}
								if (!(minigame == "slots"))
								{
									return;
								}
								Game1.currentMinigame = new Slots(-1, false);
								return;
							}
							else
							{
								if (!(minigame == "plane"))
								{
									return;
								}
								Game1.currentMinigame = new PlaneFlyBy();
								return;
							}
						}
						else
						{
							if (!(minigame == "intro"))
							{
								return;
							}
							Game1.currentMinigame = new Intro();
						}
						break;
					}
					case 6:
					{
						char c = minigame[0];
						if (c != 'c')
						{
							if (c != 't')
							{
								return;
							}
							if (!(minigame == "target"))
							{
								return;
							}
							Game1.currentMinigame = new TargetGame();
							return;
						}
						else
						{
							if (!(minigame == "cowboy"))
							{
								return;
							}
							Game1.updateViewportForScreenSizeChange(false, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
							Game1.currentMinigame = new AbigailGame(null);
							return;
						}
						break;
					}
					case 7:
					{
						char c = minigame[0];
						if (c != 'f')
						{
							if (c != 'g')
							{
								return;
							}
							if (!(minigame == "grandpa"))
							{
								return;
							}
							Game1.currentMinigame = new GrandpaStory();
							return;
						}
						else
						{
							if (!(minigame == "fishing"))
							{
								return;
							}
							Game1.currentMinigame = new FishingGame();
							return;
						}
						break;
					}
					case 8:
					{
						char c = minigame[0];
						if (c != 'b')
						{
							if (c != 'm')
							{
								return;
							}
							if (!(minigame == "minecart"))
							{
								return;
							}
							Game1.currentMinigame = new MineCart(0, 3);
							return;
						}
						else
						{
							if (!(minigame == "blastoff"))
							{
								return;
							}
							Game1.currentMinigame = new RobotBlastoff();
							return;
						}
						break;
					}
					case 9:
					{
						char c = minigame[0];
						if (c != 'h')
						{
							if (c != 'm')
							{
								return;
							}
							if (!(minigame == "marucomet"))
							{
								return;
							}
							Game1.currentMinigame = new MaruComet();
							return;
						}
						else
						{
							if (!(minigame == "haleyCows"))
							{
								return;
							}
							Game1.currentMinigame = new HaleyCowPictures();
							return;
						}
						break;
					}
					default:
						return;
					}
				}
			}

			// Token: 0x06003B2B RID: 15147 RVA: 0x002DF278 File Offset: 0x002DD478
			public static void Event(string[] command, IGameLogger log)
			{
				string locationName;
				string error;
				int eventIndex;
				bool clearEventsSeen;
				if (!ArgUtility.TryGet(command, 1, out locationName, out error, false, "string locationName") || !ArgUtility.TryGetInt(command, 2, out eventIndex, out error, "int eventIndex") || !ArgUtility.TryGetOptionalBool(command, 3, out clearEventsSeen, out error, true, "bool clearEventsSeen"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation location = Utility.fuzzyLocationSearch(locationName);
				if (location == null)
				{
					log.Error("No location with name " + locationName, null);
					return;
				}
				locationName = location.Name;
				if (locationName == "Pool")
				{
					locationName = "BathHouse_Pool";
				}
				if (clearEventsSeen)
				{
					Game1.player.eventsSeen.Clear();
				}
				string assetName = "Data\\Events\\" + locationName;
				KeyValuePair<string, string> entry = Game1.content.Load<Dictionary<string, string>>(assetName).ElementAt(eventIndex);
				if (entry.Key.Contains('/'))
				{
					LocationRequest locationRequest = Game1.getLocationRequest(locationName, false);
					locationRequest.OnLoad += delegate()
					{
						Game1.currentLocation.currentEvent = new Event(entry.Value, assetName, StardewValley.Event.SplitPreconditions(entry.Key)[0], null);
					};
					Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
				}
			}

			// Token: 0x06003B2C RID: 15148 RVA: 0x002DF384 File Offset: 0x002DD584
			[OtherNames(new string[]
			{
				"ebi"
			})]
			public static void EventById(string[] command, IGameLogger log)
			{
				string eventId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out eventId, out error, false, "string eventId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.eventsSeen.Remove(eventId);
				Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
				if (Game1.PlayEvent(eventId, false, false))
				{
					log.Info("Starting event " + eventId);
					return;
				}
				log.Error("Event '" + eventId + "' not found.", null);
			}

			// Token: 0x06003B2D RID: 15149 RVA: 0x002DF400 File Offset: 0x002DD600
			public static void EventScript(string[] command, IGameLogger log)
			{
				string script;
				string location;
				string error;
				if (!ArgUtility.TryGet(command, 1, out location, out error, true, "string location") || !ArgUtility.TryGetRemainder(command, 2, out script, out error, ' ', "string script"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (location != Game1.currentLocation.Name)
				{
					LocationRequest locationRequest = Game1.getLocationRequest(location, false);
					locationRequest.OnLoad += delegate()
					{
						Game1.currentLocation.currentEvent = new Event(script, null);
					};
					int x = 8;
					int y = 8;
					Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
					Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
					return;
				}
				Game1.globalFadeToBlack(delegate
				{
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.currentLocation.startEvent(new Event(script, null));
					Game1.globalFadeToClear(null, 0.02f);
				}, 0.02f);
			}

			// Token: 0x06003B2E RID: 15150 RVA: 0x002DF4B0 File Offset: 0x002DD6B0
			[OtherNames(new string[]
			{
				"sfe"
			})]
			public static void SetFarmEvent(string[] command, IGameLogger log)
			{
				string eventName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out eventName, out error, false, "string eventName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Dictionary<string, Func<FarmEvent>> dictionary = new Dictionary<string, Func<FarmEvent>>(StringComparer.OrdinalIgnoreCase);
				dictionary["dogs"] = (() => new SoundInTheNightEvent(2));
				dictionary["earthquake"] = (() => new SoundInTheNightEvent(4));
				dictionary["fairy"] = (() => new FairyEvent());
				dictionary["meteorite"] = (() => new SoundInTheNightEvent(1));
				dictionary["owl"] = (() => new SoundInTheNightEvent(3));
				dictionary["racoon"] = (() => new SoundInTheNightEvent(5));
				dictionary["ufo"] = (() => new SoundInTheNightEvent(0));
				dictionary["witch"] = (() => new WitchEvent());
				Dictionary<string, Func<FarmEvent>> farmEvents = dictionary;
				Func<FarmEvent> getEvent;
				if (farmEvents.TryGetValue(eventName, out getEvent))
				{
					Game1.farmEventOverride = getEvent();
					log.Info("Set farm event to '" + eventName + "'! The event will play if no other nightly event plays normally.");
					return;
				}
				log.Error("Unknown event type; expected one of '" + string.Join("', '", farmEvents.Keys) + "'.", null);
			}

			// Token: 0x06003B2F RID: 15151 RVA: 0x002DF688 File Offset: 0x002DD888
			public static void TestWedding(string[] command, IGameLogger log)
			{
				Event weddingEvent = Utility.getWeddingEvent(Game1.player);
				LocationRequest locationRequest = Game1.getLocationRequest("Town", false);
				locationRequest.OnLoad += delegate()
				{
					Game1.currentLocation.currentEvent = weddingEvent;
				};
				int x = 8;
				int y = 8;
				Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
				Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
			}

			// Token: 0x06003B30 RID: 15152 RVA: 0x002DF6EC File Offset: 0x002DD8EC
			public static void Festival(string[] command, IGameLogger log)
			{
				string festivalId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out festivalId, out error, false, "string festivalId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Dictionary<string, string> festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + festivalId);
				if (festivalData != null)
				{
					string season = new string(festivalId.Where(new Func<char, bool>(char.IsLetter)).ToArray<char>());
					int day = Convert.ToInt32(new string(festivalId.Where(new Func<char, bool>(char.IsDigit)).ToArray<char>()));
					Game1.game1.parseDebugInput("Season " + season, log);
					Game1 game = Game1.game1;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted("Day");
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(day);
					game.parseDebugInput(defaultInterpolatedStringHandler.ToStringAndClear(), log);
					string[] array = festivalData["conditions"].Split('/', StringSplitOptions.None);
					int startTime = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(array[1], 0, null));
					Game1 game2 = Game1.game1;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted("Time");
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(startTime);
					game2.parseDebugInput(defaultInterpolatedStringHandler.ToStringAndClear(), log);
					string where = array[0];
					Game1.game1.parseDebugInput("Warp " + where + " 1 1", log);
				}
			}

			// Token: 0x06003B31 RID: 15153 RVA: 0x002DF848 File Offset: 0x002DDA48
			[OtherNames(new string[]
			{
				"ps"
			})]
			public static void PlaySound(string[] command, IGameLogger log)
			{
				string soundId;
				string error;
				int pitch;
				if (!ArgUtility.TryGet(command, 1, out soundId, out error, false, "string soundId") || !ArgUtility.TryGetOptionalInt(command, 2, out pitch, out error, -1, "int pitch"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (pitch > -1)
				{
					Game1.playSound(soundId, new int?(pitch));
					return;
				}
				Game1.playSound(soundId, null);
			}

			// Token: 0x06003B32 RID: 15154 RVA: 0x002DF8A8 File Offset: 0x002DDAA8
			public static void LogSounds(string[] command, IGameLogger log)
			{
				Game1.sounds.LogSounds = !Game1.sounds.LogSounds;
				log.Info((Game1.sounds.LogSounds ? "Enabled" : "Disabled") + " sound logging.");
			}

			// Token: 0x06003B33 RID: 15155 RVA: 0x002DF8F4 File Offset: 0x002DDAF4
			[OtherNames(new string[]
			{
				"poali"
			})]
			public static void PrintOpenAlInfo(string[] command, IGameLogger log)
			{
				DebugCommands.DefaultHandlers.<>c__DisplayClass180_0 CS$<>8__locals1;
				CS$<>8__locals1.log = log;
				Assembly assembly = Assembly.GetAssembly(Game1.staminaRect.GetType());
				CS$<>8__locals1.oalType = ((assembly != null) ? assembly.GetType("Microsoft.Xna.Framework.Audio.OpenALSoundController") : null);
				if (CS$<>8__locals1.oalType == null)
				{
					CS$<>8__locals1.log.Error("Could not find type 'OpenALSoundController'", null);
					return;
				}
				FieldInfo instanceField;
				FieldInfo availableField;
				FieldInfo inUseField;
				if (!DebugCommands.DefaultHandlers.<PrintOpenAlInfo>g__TryGetField|180_0("_instance", BindingFlags.Static | BindingFlags.NonPublic, out instanceField, ref CS$<>8__locals1) || !DebugCommands.DefaultHandlers.<PrintOpenAlInfo>g__TryGetField|180_0("availableSourcesCollection", BindingFlags.Instance | BindingFlags.NonPublic, out availableField, ref CS$<>8__locals1) || !DebugCommands.DefaultHandlers.<PrintOpenAlInfo>g__TryGetField|180_0("inUseSourcesCollection", BindingFlags.Instance | BindingFlags.NonPublic, out inUseField, ref CS$<>8__locals1))
				{
					return;
				}
				object instanceObject = instanceField.GetValue(null);
				if (instanceObject == null)
				{
					CS$<>8__locals1.log.Error("OpenALSoundController._instance is null", null);
					return;
				}
				if (instanceObject.GetType() != CS$<>8__locals1.oalType)
				{
					CS$<>8__locals1.log.Error("OpenALSoundController._instance is not an instance of " + CS$<>8__locals1.oalType.ToString(), null);
					return;
				}
				object value = availableField.GetValue(instanceObject);
				object inUseObject = inUseField.GetValue(instanceObject);
				List<int> availableSourcesCollection = value as List<int>;
				List<int> inUseSourcesCollection = inUseObject as List<int>;
				if (availableSourcesCollection == null)
				{
					CS$<>8__locals1.log.Error("OpenALSoundController._instance.availableSourcesCollection is not an instance of List<int>", null);
					return;
				}
				if (inUseSourcesCollection == null)
				{
					CS$<>8__locals1.log.Error("OpenALSoundController._instance.inUseSourcesCollection is not an instance of List<int>", null);
					return;
				}
				IGameLogger log2 = CS$<>8__locals1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Available: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(availableSourcesCollection.Count);
				defaultInterpolatedStringHandler.AppendLiteral("\nIn Use: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(inUseSourcesCollection.Count);
				log2.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B34 RID: 15156 RVA: 0x002DFA78 File Offset: 0x002DDC78
			public static void Crafting(string[] command, IGameLogger log)
			{
				foreach (string s in CraftingRecipe.craftingRecipes.Keys)
				{
					Game1.player.craftingRecipes.TryAdd(s, 0);
				}
			}

			// Token: 0x06003B35 RID: 15157 RVA: 0x002DFADC File Offset: 0x002DDCDC
			public static void Cooking(string[] command, IGameLogger log)
			{
				foreach (string s in CraftingRecipe.cookingRecipes.Keys)
				{
					Game1.player.cookingRecipes.TryAdd(s, 0);
				}
			}

			// Token: 0x06003B36 RID: 15158 RVA: 0x002DFB40 File Offset: 0x002DDD40
			public static void Experience(string[] command, IGameLogger log)
			{
				string skill;
				string error;
				int experiencePoints;
				if (!ArgUtility.TryGet(command, 1, out skill, out error, false, "string skill") | !ArgUtility.TryGetInt(command, 2, out experiencePoints, out error, "int experiencePoints"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				string a = skill.ToLower();
				if (a == "all")
				{
					Game1.player.gainExperience(0, experiencePoints);
					Game1.player.gainExperience(1, experiencePoints);
					Game1.player.gainExperience(3, experiencePoints);
					Game1.player.gainExperience(2, experiencePoints);
					Game1.player.gainExperience(4, experiencePoints);
					return;
				}
				if (a == "farming")
				{
					Game1.player.gainExperience(0, experiencePoints);
					return;
				}
				if (a == "fishing")
				{
					Game1.player.gainExperience(1, experiencePoints);
					return;
				}
				if (a == "mining")
				{
					Game1.player.gainExperience(3, experiencePoints);
					return;
				}
				if (a == "foraging")
				{
					Game1.player.gainExperience(2, experiencePoints);
					return;
				}
				if (a == "combat")
				{
					Game1.player.gainExperience(4, experiencePoints);
					return;
				}
				int which;
				if (int.TryParse(skill, out which))
				{
					Game1.player.gainExperience(which, experiencePoints);
					return;
				}
				DebugCommands.LogArgError(log, command, "unknown skill ID '" + skill + "'");
			}

			// Token: 0x06003B37 RID: 15159 RVA: 0x002DFC8C File Offset: 0x002DDE8C
			public static void ShowExperience(string[] command, IGameLogger log)
			{
				int skillId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out skillId, out error, "int skillId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				log.Info(Game1.player.experiencePoints[skillId].ToString());
			}

			// Token: 0x06003B38 RID: 15160 RVA: 0x002DFCD4 File Offset: 0x002DDED4
			public static void Profession(string[] command, IGameLogger log)
			{
				int professionId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out professionId, out error, "int professionId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.professions.Add(professionId);
			}

			// Token: 0x06003B39 RID: 15161 RVA: 0x002DFD0D File Offset: 0x002DDF0D
			public static void ClearFishCaught(string[] command, IGameLogger log)
			{
				Game1.player.fishCaught.Clear();
			}

			// Token: 0x06003B3A RID: 15162 RVA: 0x002DFD20 File Offset: 0x002DDF20
			[OtherNames(new string[]
			{
				"caughtFish"
			})]
			public static void FishCaught(string[] command, IGameLogger log)
			{
				int count;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out count, out error, "int count"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.stats.FishCaught = (uint)count;
			}

			// Token: 0x06003B3B RID: 15163 RVA: 0x002DFD53 File Offset: 0x002DDF53
			[OtherNames(new string[]
			{
				"r"
			})]
			public static void ResetForPlayerEntry(string[] command, IGameLogger log)
			{
				Game1.currentLocation.cleanupBeforePlayerExit();
				Game1.currentLocation.resetForPlayerEntry();
			}

			// Token: 0x06003B3C RID: 15164 RVA: 0x002DFD6C File Offset: 0x002DDF6C
			public static void Fish(string[] command, IGameLogger log)
			{
				string fishId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out fishId, out error, false, "string fishId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				FishingRod rod = Game1.player.CurrentTool as FishingRod;
				if (rod != null)
				{
					List<string> tackleIds = rod.GetTackleQualifiedItemIDs();
					Game1.activeClickableMenu = new BobberBar(fishId, 0.5f, true, tackleIds, null, false, "", false);
					return;
				}
				log.Error("The player must have a fishing rod equipped to use this command.", null);
			}

			// Token: 0x06003B3D RID: 15165 RVA: 0x002DFDD8 File Offset: 0x002DDFD8
			public static void GrowAnimals(string[] command, IGameLogger log)
			{
				foreach (FarmAnimal farmAnimal in Game1.currentLocation.animals.Values)
				{
					farmAnimal.growFully(null);
				}
			}

			// Token: 0x06003B3E RID: 15166 RVA: 0x002DFE38 File Offset: 0x002DE038
			public static void PauseAnimals(string[] command, IGameLogger log)
			{
				foreach (FarmAnimal farmAnimal in Game1.currentLocation.Animals.Values)
				{
					farmAnimal.pauseTimer = int.MaxValue;
				}
			}

			// Token: 0x06003B3F RID: 15167 RVA: 0x002DFE9C File Offset: 0x002DE09C
			public static void UnpauseAnimals(string[] command, IGameLogger log)
			{
				foreach (FarmAnimal farmAnimal in Game1.currentLocation.Animals.Values)
				{
					farmAnimal.pauseTimer = 0;
				}
			}

			// Token: 0x06003B40 RID: 15168 RVA: 0x002DFEFC File Offset: 0x002DE0FC
			[OtherNames(new string[]
			{
				"removetf"
			})]
			public static void RemoveTerrainFeatures(string[] command, IGameLogger log)
			{
				Game1.currentLocation.terrainFeatures.Clear();
			}

			// Token: 0x06003B41 RID: 15169 RVA: 0x002DFF10 File Offset: 0x002DE110
			public static void MushroomTrees(string[] command, IGameLogger log)
			{
				foreach (TerrainFeature terrainFeature in Game1.currentLocation.terrainFeatures.Values)
				{
					Tree tree = terrainFeature as Tree;
					if (tree != null)
					{
						tree.treeType.Value = "7";
					}
				}
			}

			// Token: 0x06003B42 RID: 15170 RVA: 0x002DFF80 File Offset: 0x002DE180
			public static void TrashCan(string[] command, IGameLogger log)
			{
				int trashCanLevel;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out trashCanLevel, out error, "int trashCanLevel"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.trashCanLevel = trashCanLevel;
			}

			// Token: 0x06003B43 RID: 15171 RVA: 0x002DFFB4 File Offset: 0x002DE1B4
			public static void FruitTrees(string[] command, IGameLogger log)
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> t in Game1.currentLocation.terrainFeatures.Pairs)
				{
					FruitTree tree = t.Value as FruitTree;
					if (tree != null)
					{
						tree.daysUntilMature.Value -= 27;
						tree.dayUpdate();
					}
				}
			}

			// Token: 0x06003B44 RID: 15172 RVA: 0x002E0038 File Offset: 0x002DE238
			public static void Train(string[] command, IGameLogger log)
			{
				Game1.RequireLocation<Railroad>("Railroad", false).setTrainComing(7500);
			}

			// Token: 0x06003B45 RID: 15173 RVA: 0x002E0050 File Offset: 0x002DE250
			public static void DebrisWeather(string[] command, IGameLogger log)
			{
				string contextId = Game1.player.currentLocation.GetLocationContextId();
				LocationWeather weather = Game1.netWorldState.Value.GetWeatherForLocation(contextId);
				weather.IsDebrisWeather = !weather.IsDebrisWeather;
				if (contextId == "Default")
				{
					Game1.isDebrisWeather = weather.isDebrisWeather.Value;
				}
				Game1.debrisWeather.Clear();
				if (weather.IsDebrisWeather)
				{
					Game1.populateDebrisWeatherArray();
				}
			}

			// Token: 0x06003B46 RID: 15174 RVA: 0x002E00C4 File Offset: 0x002DE2C4
			public static void Speed(string[] command, IGameLogger log)
			{
				int speed;
				string error;
				int minutes;
				if (!ArgUtility.TryGetInt(command, 1, out speed, out error, "int speed") || !ArgUtility.TryGetOptionalInt(command, 2, out minutes, out error, 30, "int minutes"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				BuffEffects effects = new BuffEffects();
				effects.Speed.Value = (float)speed;
				Game1.player.applyBuff(new Buff("debug_speed", "Debug Speed", "Debug Speed", minutes * Game1.realMilliSecondsPerGameMinute, null, 0, effects, null, null, null));
			}

			// Token: 0x06003B47 RID: 15175 RVA: 0x002E0148 File Offset: 0x002DE348
			public static void DayUpdate(string[] command, IGameLogger log)
			{
				int days;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out days, out error, "int days"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				for (int i = 0; i < days; i++)
				{
					Game1.currentLocation.DayUpdate(Game1.dayOfMonth);
				}
			}

			// Token: 0x06003B48 RID: 15176 RVA: 0x002E018C File Offset: 0x002DE38C
			public static void FarmerDayUpdate(string[] command, IGameLogger log)
			{
				int days;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out days, out error, "int days"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				for (int i = 0; i < days; i++)
				{
					Game1.player.dayupdate(Game1.timeOfDay);
				}
			}

			// Token: 0x06003B49 RID: 15177 RVA: 0x002E01D0 File Offset: 0x002DE3D0
			public static void MuseumLoot(string[] command, IGameLogger log)
			{
				foreach (ParsedItemData parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					string id = parsedItemData.ItemId;
					string type = parsedItemData.ObjectType;
					if ((type == "Arch" || type == "Minerals") && !Game1.player.mineralsFound.ContainsKey(id) && !Game1.player.archaeologyFound.ContainsKey(id))
					{
						if (type == "Arch")
						{
							Game1.player.foundArtifact(id, 1);
						}
						else
						{
							Game1.player.addItemToInventoryBool(new Object(id, 1, false, -1, 0), false);
						}
					}
					if (Game1.player.freeSpotsInInventory() == 0)
					{
						break;
					}
				}
			}

			// Token: 0x06003B4A RID: 15178 RVA: 0x002E02AC File Offset: 0x002DE4AC
			public static void NewMuseumLoot(string[] command, IGameLogger log)
			{
				foreach (ParsedItemData parsedItemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					string itemId = parsedItemData.QualifiedItemId;
					if (LibraryMuseum.IsItemSuitableForDonation(itemId, true) && !LibraryMuseum.HasDonatedArtifact(itemId))
					{
						Game1.player.addItemToInventoryBool(ItemRegistry.Create(itemId, 1, 0, false), false);
					}
					if (Game1.player.freeSpotsInInventory() == 0)
					{
						break;
					}
				}
			}

			// Token: 0x06003B4B RID: 15179 RVA: 0x002E0330 File Offset: 0x002DE530
			public static void CreateDebris(string[] command, IGameLogger log)
			{
				string itemId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out itemId, out error, false, "string itemId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.createObjectDebris(itemId, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, -1, 0, 1f, null);
			}

			// Token: 0x06003B4C RID: 15180 RVA: 0x002E0385 File Offset: 0x002DE585
			public static void RemoveDebris(string[] command, IGameLogger log)
			{
				Game1.currentLocation.debris.Clear();
			}

			// Token: 0x06003B4D RID: 15181 RVA: 0x002E0396 File Offset: 0x002DE596
			public static void RemoveDirt(string[] command, IGameLogger log)
			{
				Game1.currentLocation.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value is HoeDirt);
			}

			// Token: 0x06003B4E RID: 15182 RVA: 0x002E03C7 File Offset: 0x002DE5C7
			public static void DyeAll(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots, false);
			}

			// Token: 0x06003B4F RID: 15183 RVA: 0x002E03D5 File Offset: 0x002DE5D5
			public static void DyeShirt(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new CharacterCustomization(Game1.player.shirtItem.Value);
			}

			// Token: 0x06003B50 RID: 15184 RVA: 0x002E03F0 File Offset: 0x002DE5F0
			public static void DyePants(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new CharacterCustomization(Game1.player.pantsItem.Value);
			}

			// Token: 0x06003B51 RID: 15185 RVA: 0x002E040B File Offset: 0x002DE60B
			[OtherNames(new string[]
			{
				"cmenu",
				"customize"
			})]
			public static void CustomizeMenu(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame, false);
			}

			// Token: 0x06003B52 RID: 15186 RVA: 0x002E041C File Offset: 0x002DE61C
			public static void CopyOutfit(string[] command, IGameLogger log)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("<Item><OutfitParts>");
				if (Game1.player.hat.Value != null)
				{
					sb.Append("<Item><ItemId>" + Game1.player.hat.Value.QualifiedItemId + "</ItemId></Item>");
				}
				if (Game1.player.pantsItem.Value != null)
				{
					sb.Append(string.Concat(new string[]
					{
						"<Item><ItemId>",
						Game1.player.pantsItem.Value.QualifiedItemId,
						"</ItemId><Color>",
						Game1.player.pantsItem.Value.clothesColor.Value.R.ToString(),
						" ",
						Game1.player.pantsItem.Value.clothesColor.Value.G.ToString(),
						" ",
						Game1.player.pantsItem.Value.clothesColor.Value.B.ToString(),
						"</Color></Item>"
					}));
				}
				if (Game1.player.shirtItem.Value != null)
				{
					sb.Append(string.Concat(new string[]
					{
						"<Item><ItemId>",
						Game1.player.shirtItem.Value.QualifiedItemId,
						"</ItemId><Color>",
						Game1.player.shirtItem.Value.clothesColor.Value.R.ToString(),
						" ",
						Game1.player.shirtItem.Value.clothesColor.Value.G.ToString(),
						" ",
						Game1.player.shirtItem.Value.clothesColor.Value.B.ToString(),
						"</Color></Item>"
					}));
				}
				sb.Append("</OutfitParts></Item>");
				string text = sb.ToString();
				DesktopClipboard.SetText(text);
				Game1.debugOutput = text;
			}

			// Token: 0x06003B53 RID: 15187 RVA: 0x002E0668 File Offset: 0x002DE868
			public static void SkinColor(string[] command, IGameLogger log)
			{
				int skinColor;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out skinColor, out error, "int skinColor"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changeSkinColor(skinColor, false);
			}

			// Token: 0x06003B54 RID: 15188 RVA: 0x002E069C File Offset: 0x002DE89C
			public static void Hat(string[] command, IGameLogger log)
			{
				int hatId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out hatId, out error, "int hatId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changeHat(hatId);
				Game1.playSound("coin", null);
			}

			// Token: 0x06003B55 RID: 15189 RVA: 0x002E06E4 File Offset: 0x002DE8E4
			public static void Pants(string[] command, IGameLogger log)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(command, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(command, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out blue, out error, "int blue"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changePantsColor(new Color(red, green, blue));
			}

			// Token: 0x06003B56 RID: 15190 RVA: 0x002E0744 File Offset: 0x002DE944
			public static void HairStyle(string[] command, IGameLogger log)
			{
				int hairStyle;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out hairStyle, out error, "int hairStyle"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changeHairStyle(hairStyle);
			}

			// Token: 0x06003B57 RID: 15191 RVA: 0x002E0778 File Offset: 0x002DE978
			public static void HairColor(string[] command, IGameLogger log)
			{
				int red;
				string error;
				int green;
				int blue;
				if (!ArgUtility.TryGetInt(command, 1, out red, out error, "int red") || !ArgUtility.TryGetInt(command, 2, out green, out error, "int green") || !ArgUtility.TryGetInt(command, 3, out blue, out error, "int blue"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changeHairColor(new Color(red, green, blue));
			}

			// Token: 0x06003B58 RID: 15192 RVA: 0x002E07D8 File Offset: 0x002DE9D8
			public static void Shirt(string[] command, IGameLogger log)
			{
				string shirtId;
				string error;
				if (!ArgUtility.TryGet(command, 1, out shirtId, out error, false, "string shirtId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.changeShirt(shirtId);
			}

			// Token: 0x06003B59 RID: 15193 RVA: 0x002E080C File Offset: 0x002DEA0C
			[OtherNames(new string[]
			{
				"m",
				"mv"
			})]
			public static void MusicVolume(string[] command, IGameLogger log)
			{
				float volume;
				string error;
				if (!ArgUtility.TryGetFloat(command, 1, out volume, out error, "float volume"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.musicPlayerVolume = volume;
				Game1.options.musicVolumeLevel = volume;
				Game1.musicCategory.SetVolume(Game1.options.musicVolumeLevel);
			}

			// Token: 0x06003B5A RID: 15194 RVA: 0x002E0859 File Offset: 0x002DEA59
			public static void RemoveObjects(string[] command, IGameLogger log)
			{
				Game1.currentLocation.objects.Clear();
			}

			// Token: 0x06003B5B RID: 15195 RVA: 0x002E086C File Offset: 0x002DEA6C
			public static void ListLights(string[] command, IGameLogger log)
			{
				StringBuilder report = new StringBuilder();
				StringBuilder stringBuilder = report;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(69, 6, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("The viewport covers tiles (");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.viewport.X / 64);
				appendInterpolatedStringHandler.AppendLiteral(", ");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.viewport.Y / 64);
				appendInterpolatedStringHandler.AppendLiteral(") through (");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.viewport.MaxCorner.X / 64);
				appendInterpolatedStringHandler.AppendLiteral(", ");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.viewport.MaxCorner.Y / 64);
				appendInterpolatedStringHandler.AppendLiteral("), with the player at (");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.player.TilePoint.X);
				appendInterpolatedStringHandler.AppendLiteral(", ");
				appendInterpolatedStringHandler.AppendFormatted<int>(Game1.player.TilePoint.Y);
				appendInterpolatedStringHandler.AppendLiteral(").");
				stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
				report.AppendLine();
				if (Game1.currentLightSources.Count > 0)
				{
					using (IEnumerator<IGrouping<bool, KeyValuePair<string, LightSource>>> enumerator = (from p in Game1.currentLightSources.ToLookup((KeyValuePair<string, LightSource> light) => light.Value.IsOnScreen())
					orderby p.Key descending
					select p).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							IGrouping<bool, KeyValuePair<string, LightSource>> grouping = enumerator.Current;
							bool inView = grouping.Key;
							KeyValuePair<string, LightSource>[] lights = grouping.ToArray<KeyValuePair<string, LightSource>>();
							if (lights.Length != 0)
							{
								stringBuilder = report;
								StringBuilder stringBuilder3 = stringBuilder;
								appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder);
								appendInterpolatedStringHandler.AppendLiteral("Lights ");
								appendInterpolatedStringHandler.AppendFormatted(inView ? "in view" : "out of view");
								appendInterpolatedStringHandler.AppendLiteral(":");
								stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
								int i = 1;
								foreach (KeyValuePair<string, LightSource> pair in lights)
								{
									LightSource light2 = pair.Value;
									Vector2 tile = new Vector2(light2.position.X / 64f, light2.position.Y / 64f);
									stringBuilder = report;
									StringBuilder stringBuilder4 = stringBuilder;
									appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(32, 5, stringBuilder);
									appendInterpolatedStringHandler.AppendLiteral("  ");
									appendInterpolatedStringHandler.AppendFormatted<int>(i++);
									appendInterpolatedStringHandler.AppendLiteral(". '");
									appendInterpolatedStringHandler.AppendFormatted(light2.Id);
									appendInterpolatedStringHandler.AppendLiteral("' at tile (");
									appendInterpolatedStringHandler.AppendFormatted<float>(tile.X);
									appendInterpolatedStringHandler.AppendLiteral(", ");
									appendInterpolatedStringHandler.AppendFormatted<float>(tile.Y);
									appendInterpolatedStringHandler.AppendLiteral(") with radius ");
									appendInterpolatedStringHandler.AppendFormatted<float>(light2.radius.Value);
									stringBuilder4.Append(ref appendInterpolatedStringHandler);
									if (light2.onlyLocation.Value != null)
									{
										string value = light2.onlyLocation.Value;
										GameLocation currentLocation = Game1.currentLocation;
										if (value != ((currentLocation != null) ? currentLocation.NameOrUniqueName : null))
										{
											stringBuilder = report;
											StringBuilder stringBuilder5 = stringBuilder;
											appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder);
											appendInterpolatedStringHandler.AppendLiteral(" [only shown in location '");
											appendInterpolatedStringHandler.AppendFormatted(light2.onlyLocation.Value);
											appendInterpolatedStringHandler.AppendLiteral("']");
											stringBuilder5.Append(ref appendInterpolatedStringHandler);
										}
									}
									if (light2.Id != pair.Key)
									{
										stringBuilder = report;
										StringBuilder stringBuilder6 = stringBuilder;
										appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(74, 2, stringBuilder);
										appendInterpolatedStringHandler.AppendLiteral(" [WARNING: ID mismatch between dictionary lookup (");
										appendInterpolatedStringHandler.AppendFormatted(pair.Key);
										appendInterpolatedStringHandler.AppendLiteral(") and light instance (");
										appendInterpolatedStringHandler.AppendFormatted(light2.Id);
										appendInterpolatedStringHandler.AppendLiteral(")]");
										stringBuilder6.Append(ref appendInterpolatedStringHandler);
									}
									report.AppendLine(".");
								}
								report.AppendLine();
							}
						}
						goto IL_3D2;
					}
				}
				report.AppendLine("There are no current light sources.");
				IL_3D2:
				log.Info(report.ToString().TrimEnd());
			}

			// Token: 0x06003B5C RID: 15196 RVA: 0x002E0C78 File Offset: 0x002DEE78
			public static void RemoveLights(string[] command, IGameLogger log)
			{
				Game1.currentLightSources.Clear();
			}

			// Token: 0x06003B5D RID: 15197 RVA: 0x002E0C84 File Offset: 0x002DEE84
			[OtherNames(new string[]
			{
				"i"
			})]
			public static void Item(string[] command, IGameLogger log)
			{
				string itemId;
				string error;
				int count;
				int quality;
				if (!ArgUtility.TryGet(command, 1, out itemId, out error, false, "string itemId") || !ArgUtility.TryGetOptionalInt(command, 2, out count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out quality, out error, 0, "int quality"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Item item = ItemRegistry.Create(itemId, count, quality, false);
				Game1.playSound("coin", null);
				Game1.player.addItemToInventoryBool(item, false);
			}

			// Token: 0x06003B5E RID: 15198 RVA: 0x002E0D04 File Offset: 0x002DEF04
			[OtherNames(new string[]
			{
				"iq"
			})]
			public static void ItemQuery(string[] command, IGameLogger log)
			{
				string query;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out query, out error, ' ', "string query"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				ItemQueryResult[] result = ItemQueryResolver.TryResolve(query, null, ItemQuerySearchMode.All, null, null, false, null, delegate(string _, string queryError)
				{
					log.Error("Failed parsing that query: " + queryError, null);
				});
				if (result.Length == 0)
				{
					log.Info("That query did not match any items.");
					return;
				}
				ShopMenu shop = new ShopMenu("DebugItemQuery", new Dictionary<ISalable, ItemStockInformation>(), 0, null, null, null, true);
				foreach (ItemQueryResult entry in result)
				{
					shop.AddForSale(entry.Item, new ItemStockInformation(0, int.MaxValue, null, null, LimitedStockMode.Global, null, null, null, null));
				}
				Game1.activeClickableMenu = shop;
			}

			// Token: 0x06003B5F RID: 15199 RVA: 0x002E0DE4 File Offset: 0x002DEFE4
			[OtherNames(new string[]
			{
				"gq"
			})]
			public static void GameQuery(string[] command, IGameLogger log)
			{
				string query;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out query, out error, ' ', "string query"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				var rows = (from rawQuery in GameStateQuery.SplitRaw(query)
				select new
				{
					Query = rawQuery,
					Result = GameStateQuery.CheckConditions(rawQuery, null, null, null, null, null, null)
				}).ToArray();
				int queryLength = Math.Max("Query".Length, rows.Max(p => p.Query.Length));
				StringBuilder summary = new StringBuilder().AppendLine().Append("   ").Append("Query".PadRight(queryLength, ' ')).AppendLine(" | Result").Append("   ").Append("".PadRight(queryLength, '-')).AppendLine(" | ------");
				bool result = true;
				var array = rows;
				for (int i = 0; i < array.Length; i++)
				{
					var row = array[i];
					result = (result && row.Result);
					summary.Append("   ").Append(row.Query.PadRight(queryLength, ' ')).Append(" | ").AppendLine(row.Result.ToString().ToLower());
				}
				summary.AppendLine().Append("Overall result: ").Append(result.ToString().ToLower()).AppendLine(".");
				log.Info(summary.ToString());
			}

			// Token: 0x06003B60 RID: 15200 RVA: 0x002E0F7C File Offset: 0x002DF17C
			public static void Tokens(string[] command, IGameLogger log)
			{
				string input;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out input, out error, ' ', "string input"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				string result = TokenParser.ParseText(input, null, null, null);
				log.Info("Result: \"" + result + "\".");
			}

			// Token: 0x06003B61 RID: 15201 RVA: 0x002E0FC6 File Offset: 0x002DF1C6
			public static void DyeMenu(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new DyeMenu();
			}

			// Token: 0x06003B62 RID: 15202 RVA: 0x002E0FD2 File Offset: 0x002DF1D2
			public static void Tailor(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new TailoringMenu();
			}

			// Token: 0x06003B63 RID: 15203 RVA: 0x002E0FDE File Offset: 0x002DF1DE
			public static void Forge(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new ForgeMenu();
			}

			// Token: 0x06003B64 RID: 15204 RVA: 0x002E0FEC File Offset: 0x002DF1EC
			public static void ListTags(string[] command, IGameLogger log)
			{
				if (Game1.player.CurrentItem != null)
				{
					string out_string = "Tags on " + Game1.player.CurrentItem.DisplayName + ": ";
					foreach (string tag in Game1.player.CurrentItem.GetContextTags())
					{
						out_string = out_string + tag + " ";
					}
					log.Info(out_string.Trim());
				}
			}

			// Token: 0x06003B65 RID: 15205 RVA: 0x002E1088 File Offset: 0x002DF288
			public static void QualifiedId(string[] command, IGameLogger log)
			{
				if (Game1.player.CurrentItem != null)
				{
					string result = "Qualified ID of " + Game1.player.CurrentItem.DisplayName + ": " + Game1.player.CurrentItem.QualifiedItemId;
					log.Info(result.Trim());
				}
			}

			// Token: 0x06003B66 RID: 15206 RVA: 0x002E10DC File Offset: 0x002DF2DC
			public static void Dye(string[] command, IGameLogger log)
			{
				string slot;
				string error;
				string color;
				float dyeStrength;
				if (!ArgUtility.TryGet(command, 1, out slot, out error, false, "string slot") || !ArgUtility.TryGet(command, 2, out color, out error, false, "string color") || !ArgUtility.TryGetOptionalFloat(command, 3, out dyeStrength, out error, 1f, "float dyeStrength"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Color target = Color.White;
				string a = color.ToLower().Trim();
				if (!(a == "black"))
				{
					if (!(a == "red"))
					{
						if (!(a == "blue"))
						{
							if (!(a == "yellow"))
							{
								if (!(a == "white"))
								{
									if (a == "green")
									{
										target = new Color(10, 143, 0);
									}
								}
								else
								{
									target = Color.White;
								}
							}
							else
							{
								target = new Color(255, 230, 0);
							}
						}
						else
						{
							target = new Color(0, 100, 220);
						}
					}
					else
					{
						target = new Color(220, 0, 0);
					}
				}
				else
				{
					target = Color.Black;
				}
				a = slot.ToLower().Trim();
				if (!(a == "shirt"))
				{
					if (!(a == "pants"))
					{
						return;
					}
					Clothing value = Game1.player.pantsItem.Value;
					if (value == null)
					{
						return;
					}
					value.Dye(target, dyeStrength);
					return;
				}
				else
				{
					Clothing value2 = Game1.player.shirtItem.Value;
					if (value2 == null)
					{
						return;
					}
					value2.Dye(target, dyeStrength);
					return;
				}
			}

			// Token: 0x06003B67 RID: 15207 RVA: 0x002E1254 File Offset: 0x002DF454
			public static void GetIndex(string[] command, IGameLogger log)
			{
				string itemName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out itemName, out error, false, "string itemName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Item item = Utility.fuzzyItemSearch(itemName, 1, false);
				if (item != null)
				{
					log.Info(item.DisplayName + "'s qualified ID is " + item.QualifiedItemId);
					return;
				}
				log.Error("No item found with name " + itemName, null);
			}

			// Token: 0x06003B68 RID: 15208 RVA: 0x002E12B8 File Offset: 0x002DF4B8
			[OtherNames(new string[]
			{
				"f",
				"fin"
			})]
			public static void FuzzyItemNamed(string[] command, IGameLogger log)
			{
				string itemId;
				string error;
				int count;
				int quality;
				if (!ArgUtility.TryGet(command, 1, out itemId, out error, true, "string itemId") || !ArgUtility.TryGetOptionalInt(command, 2, out count, out error, 0, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out quality, out error, 0, "int quality"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Item item = Utility.fuzzyItemSearch(itemId, count, false);
				if (item == null)
				{
					log.Error("No item found with name '" + itemId + "'", null);
					return;
				}
				item.quality.Value = quality;
				MeleeWeapon.attemptAddRandomInnateEnchantment(item, null, false, null);
				Game1.player.addItemToInventory(item);
				Game1.playSound("coin", null);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Added ");
				defaultInterpolatedStringHandler.AppendFormatted(item.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B69 RID: 15209 RVA: 0x002E13C0 File Offset: 0x002DF5C0
			[OtherNames(new string[]
			{
				"in"
			})]
			public static void ItemNamed(string[] command, IGameLogger log)
			{
				string itemName;
				string error;
				int count;
				int quality;
				if (!ArgUtility.TryGet(command, 1, out itemName, out error, false, "string itemName") || !ArgUtility.TryGetOptionalInt(command, 2, out count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(command, 3, out quality, out error, 0, "int quality"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					if (item.InternalName.EqualsIgnoreCase(itemName))
					{
						Game1.player.addItemToInventory(ItemRegistry.Create("(O)" + item.ItemId, count, quality, false));
						Game1.playSound("coin", null);
					}
				}
			}

			// Token: 0x06003B6A RID: 15210 RVA: 0x002E149C File Offset: 0x002DF69C
			public static void Achievement(string[] command, IGameLogger log)
			{
				int achievementId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out achievementId, out error, "int achievementId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.getAchievement(achievementId, true);
			}

			// Token: 0x06003B6B RID: 15211 RVA: 0x002E14CB File Offset: 0x002DF6CB
			public static void Heal(string[] command, IGameLogger log)
			{
				Game1.player.health = Game1.player.maxHealth;
			}

			// Token: 0x06003B6C RID: 15212 RVA: 0x002E14E1 File Offset: 0x002DF6E1
			public static void Die(string[] command, IGameLogger log)
			{
				Game1.player.health = 0;
			}

			// Token: 0x06003B6D RID: 15213 RVA: 0x002E14F0 File Offset: 0x002DF6F0
			public static void Energize(string[] command, IGameLogger log)
			{
				int stamina;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out stamina, out error, Game1.player.MaxStamina, "int stamina"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.Stamina = (float)stamina;
			}

			// Token: 0x06003B6E RID: 15214 RVA: 0x002E152E File Offset: 0x002DF72E
			public static void Exhaust(string[] command, IGameLogger log)
			{
				Game1.player.Stamina = -15f;
			}

			// Token: 0x06003B6F RID: 15215 RVA: 0x002E1540 File Offset: 0x002DF740
			public static void Warp(string[] command, IGameLogger log)
			{
				string locationName;
				string error;
				int tileX;
				int tileY;
				if (!ArgUtility.TryGet(command, 1, out locationName, out error, false, "string locationName") || !ArgUtility.TryGetOptionalInt(command, 2, out tileX, out error, -1, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 3, out tileY, out error, -1, "int tileY"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (tileX > -1 && tileY <= -1)
				{
					DebugCommands.LogArgError(log, command, "must specify both X and Y positions, or neither");
					return;
				}
				GameLocation location = Utility.fuzzyLocationSearch(locationName);
				if (location == null)
				{
					log.Error("No location with name " + locationName, null);
					return;
				}
				if (tileX < 0)
				{
					tileX = 0;
					tileY = 0;
					Utility.getDefaultWarpLocation(location.Name, ref tileX, ref tileY);
				}
				Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), tileX, tileY, 2);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Warping Game1.player to ");
				defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral(" at ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tileX);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(tileY);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B70 RID: 15216 RVA: 0x002E1659 File Offset: 0x002DF859
			[OtherNames(new string[]
			{
				"wh"
			})]
			public static void WarpHome(string[] command, IGameLogger log)
			{
				Game1.warpHome();
			}

			// Token: 0x06003B71 RID: 15217 RVA: 0x002E1660 File Offset: 0x002DF860
			public static void Money(string[] command, IGameLogger log)
			{
				int amount;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out amount, out error, "int amount"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.Money = amount;
			}

			// Token: 0x06003B72 RID: 15218 RVA: 0x002E1694 File Offset: 0x002DF894
			public static void CatchAllFish(string[] command, IGameLogger log)
			{
				foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					if (itemData.ObjectType == "Fish")
					{
						Game1.player.caughtFish(itemData.ItemId, 9, false, 1);
					}
				}
			}

			// Token: 0x06003B73 RID: 15219 RVA: 0x002E1708 File Offset: 0x002DF908
			public static void ActivateCalicoStatue(string[] command, IGameLogger log)
			{
				Game1.mine.calicoStatueSpot.Value = new Point(8, 8);
				Game1.mine.calicoStatueActivated(new NetPoint(new Point(8, 8)), Point.Zero, new Point(8, 8));
			}

			// Token: 0x06003B74 RID: 15220 RVA: 0x002E1744 File Offset: 0x002DF944
			public static void Perfection(string[] command, IGameLogger log)
			{
				Game1.game1.parseDebugInput("CompleteCc", log);
				Game1.game1.parseDebugInput("Specials", log);
				Game1.game1.parseDebugInput("FriendAll", log);
				Game1.game1.parseDebugInput("Cooking", log);
				Game1.game1.parseDebugInput("Crafting", log);
				foreach (string key in Game1.player.craftingRecipes.Keys)
				{
					Game1.player.craftingRecipes[key] = 1;
				}
				foreach (ParsedItemData item in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					string id = item.ItemId;
					if (item.ObjectType == "Fish")
					{
						Game1.player.fishCaught.Add(item.QualifiedItemId, new int[3]);
					}
					if (Object.isPotentialBasicShipped(id, item.Category, item.ObjectType))
					{
						Game1.player.basicShipped.Add(id, 1);
					}
					Game1.player.recipesCooked.Add(id, 1);
				}
				Game1.game1.parseDebugInput("Walnut 130", log);
				Game1.player.mailReceived.Add("CF_Fair");
				Game1.player.mailReceived.Add("CF_Fish");
				Game1.player.mailReceived.Add("CF_Sewer");
				Game1.player.mailReceived.Add("CF_Mines");
				Game1.player.mailReceived.Add("CF_Spouse");
				Game1.player.mailReceived.Add("CF_Statue");
				Game1.player.mailReceived.Add("museumComplete");
				Game1.player.miningLevel.Value = 10;
				Game1.player.fishingLevel.Value = 10;
				Game1.player.foragingLevel.Value = 10;
				Game1.player.combatLevel.Value = 10;
				Game1.player.farmingLevel.Value = 10;
				Farm farm = Game1.getFarm();
				Building building;
				farm.buildStructure("Water Obelisk", new Vector2(0f, 0f), Game1.player, out building, true, true);
				farm.buildStructure("Earth Obelisk", new Vector2(4f, 0f), Game1.player, out building, true, true);
				farm.buildStructure("Desert Obelisk", new Vector2(8f, 0f), Game1.player, out building, true, true);
				farm.buildStructure("Island Obelisk", new Vector2(12f, 0f), Game1.player, out building, true, true);
				farm.buildStructure("Gold Clock", new Vector2(16f, 0f), Game1.player, out building, true, true);
				foreach (KeyValuePair<string, string> v in DataLoader.Monsters(Game1.content))
				{
					for (int i = 0; i < 500; i++)
					{
						Game1.stats.monsterKilled(v.Key);
					}
				}
			}

			// Token: 0x06003B75 RID: 15221 RVA: 0x002E1ACC File Offset: 0x002DFCCC
			public static void Walnut(string[] command, IGameLogger log)
			{
				int count;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out count, out error, "int count"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.netWorldState.Value.GoldenWalnuts += count;
				Game1.netWorldState.Value.GoldenWalnutsFound += count;
			}

			// Token: 0x06003B76 RID: 15222 RVA: 0x002E1B24 File Offset: 0x002DFD24
			public static void Gem(string[] command, IGameLogger log)
			{
				int count;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out count, out error, "int count"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.QiGems += count;
			}

			// Token: 0x06003B77 RID: 15223 RVA: 0x002E1B60 File Offset: 0x002DFD60
			[OtherNames(new string[]
			{
				"removeNpc"
			})]
			public static void KillNpc(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				bool anyFound = false;
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.characters.RemoveWhere(delegate(NPC npc)
					{
						if (npc.Name == npcName)
						{
							log.Info("Removed " + npc.Name + " from " + location.NameOrUniqueName);
							anyFound = true;
							return true;
						}
						return false;
					});
					return true;
				}, true, false);
				if (!anyFound)
				{
					log.Error("Couldn't find " + npcName + " in any locations.", null);
				}
			}

			// Token: 0x06003B78 RID: 15224 RVA: 0x002E1BE2 File Offset: 0x002DFDE2
			[OtherNames(new string[]
			{
				"dap"
			})]
			public static void DaysPlayed(string[] command, IGameLogger log)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3332", (int)Game1.stats.DaysPlayed));
			}

			// Token: 0x06003B79 RID: 15225 RVA: 0x002E1C08 File Offset: 0x002DFE08
			public static void FriendAll(string[] command, IGameLogger log)
			{
				int friendship;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out friendship, out error, 2500, "int friendship"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (Game1.year == 1)
				{
					Game1.AddCharacterIfNecessary("Kent", true);
					Game1.AddCharacterIfNecessary("Leo", true);
				}
				Utility.ForEachVillager(delegate(NPC n)
				{
					if (!n.CanSocialize && n.Name != "Sandy" && n.Name == "Krobus")
					{
						return true;
					}
					if (n.Name == "Marlon")
					{
						return true;
					}
					if (!Game1.player.friendshipData.ContainsKey(n.Name))
					{
						Game1.player.friendshipData.Add(n.Name, new Friendship());
					}
					Game1.player.changeFriendship(friendship, n);
					return true;
				}, false);
			}

			// Token: 0x06003B7A RID: 15226 RVA: 0x002E1C74 File Offset: 0x002DFE74
			[OtherNames(new string[]
			{
				"friend"
			})]
			public static void Friendship(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				int friendshipPoints;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName") || !ArgUtility.TryGetInt(command, 2, out friendshipPoints, out error, "int friendshipPoints"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				if (npc == null)
				{
					log.Error("No character found matching '" + npcName + "'.", null);
					return;
				}
				Friendship friendship;
				if (!Game1.player.friendshipData.TryGetValue(npc.Name, out friendship))
				{
					friendship = (Game1.player.friendshipData[npc.Name] = new Friendship());
				}
				friendship.Points = friendshipPoints;
			}

			// Token: 0x06003B7B RID: 15227 RVA: 0x002E1D10 File Offset: 0x002DFF10
			public static void GetStat(string[] command, IGameLogger log)
			{
				string statName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out statName, out error, false, "string statName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				uint value = Game1.stats.Get(statName);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The '");
				defaultInterpolatedStringHandler.AppendFormatted(statName);
				defaultInterpolatedStringHandler.AppendLiteral("' stat is set to ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(value);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B7C RID: 15228 RVA: 0x002E1D90 File Offset: 0x002DFF90
			public static void SetStat(string[] command, IGameLogger log)
			{
				string statName;
				string error;
				int newValue;
				if (!ArgUtility.TryGet(command, 1, out statName, out error, false, "string statName") || !ArgUtility.TryGetInt(command, 2, out newValue, out error, "int newValue"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.stats.Set(statName, newValue);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Set '");
				defaultInterpolatedStringHandler.AppendFormatted(statName);
				defaultInterpolatedStringHandler.AppendLiteral("' stat to ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(Game1.stats.Get(statName));
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B7D RID: 15229 RVA: 0x002E1E2C File Offset: 0x002E002C
			[OtherNames(new string[]
			{
				"eventSeen"
			})]
			public static void SeenEvent(string[] command, IGameLogger log)
			{
				string eventId;
				string error;
				bool seen;
				if (!ArgUtility.TryGet(command, 1, out eventId, out error, false, "string eventId") || !ArgUtility.TryGetOptionalBool(command, 2, out seen, out error, true, "bool seen"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.eventsSeen.Toggle(eventId, seen);
				if (!seen)
				{
					Game1.eventsSeenSinceLastLocationChange.Remove(eventId);
				}
			}

			// Token: 0x06003B7E RID: 15230 RVA: 0x002E1E88 File Offset: 0x002E0088
			public static void SeenMail(string[] command, IGameLogger log)
			{
				string mailId;
				string error;
				bool seen;
				if (!ArgUtility.TryGet(command, 1, out mailId, out error, false, "string mailId") || !ArgUtility.TryGetOptionalBool(command, 2, out seen, out error, true, "bool seen"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.mailReceived.Toggle(mailId, seen);
			}

			// Token: 0x06003B7F RID: 15231 RVA: 0x002E1ED8 File Offset: 0x002E00D8
			public static void CookingRecipe(string[] command, IGameLogger log)
			{
				string recipeName;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out recipeName, out error, ' ', "string recipeName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.cookingRecipes.Add(recipeName.Trim(), 0);
			}

			// Token: 0x06003B80 RID: 15232 RVA: 0x002E1F18 File Offset: 0x002E0118
			[OtherNames(new string[]
			{
				"craftingRecipe"
			})]
			public static void AddCraftingRecipe(string[] command, IGameLogger log)
			{
				string recipeName;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out recipeName, out error, ' ', "string recipeName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.craftingRecipes.Add(recipeName.Trim(), 0);
			}

			// Token: 0x06003B81 RID: 15233 RVA: 0x002E1F58 File Offset: 0x002E0158
			public static void UpgradeHouse(string[] command, IGameLogger log)
			{
				Game1.player.HouseUpgradeLevel = Math.Min(3, Game1.player.HouseUpgradeLevel + 1);
				Game1.addNewFarmBuildingMaps();
			}

			// Token: 0x06003B82 RID: 15234 RVA: 0x002E1F7B File Offset: 0x002E017B
			public static void StopRafting(string[] command, IGameLogger log)
			{
				Game1.player.isRafting = false;
			}

			// Token: 0x06003B83 RID: 15235 RVA: 0x002E1F88 File Offset: 0x002E0188
			public static void Time(string[] command, IGameLogger log)
			{
				int time;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out time, out error, "int time"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.timeOfDay = time;
				Game1.outdoorLight = Color.White;
			}

			// Token: 0x06003B84 RID: 15236 RVA: 0x002E1FC0 File Offset: 0x002E01C0
			public static void AddMinute(string[] command, IGameLogger log)
			{
				Game1.addMinute();
			}

			// Token: 0x06003B85 RID: 15237 RVA: 0x002E1FC7 File Offset: 0x002E01C7
			public static void AddHour(string[] command, IGameLogger log)
			{
				Game1.addHour();
			}

			// Token: 0x06003B86 RID: 15238 RVA: 0x002E1FCE File Offset: 0x002E01CE
			public static void Water(string[] command, IGameLogger log)
			{
				GameLocation currentLocation = Game1.currentLocation;
				if (currentLocation == null)
				{
					return;
				}
				currentLocation.ForEachDirt(delegate(HoeDirt dirt)
				{
					if (dirt.Pot != null)
					{
						dirt.Pot.Water();
					}
					else
					{
						dirt.state.Value = 1;
					}
					return true;
				}, true);
			}

			// Token: 0x06003B87 RID: 15239 RVA: 0x002E2000 File Offset: 0x002E0200
			public static void GrowCrops(string[] command, IGameLogger log)
			{
				int days;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out days, out error, "int days"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation currentLocation = Game1.currentLocation;
				if (currentLocation == null)
				{
					return;
				}
				currentLocation.ForEachDirt(delegate(HoeDirt dirt)
				{
					if (((dirt != null) ? dirt.crop : null) != null)
					{
						for (int i = 0; i < days; i++)
						{
							dirt.crop.newDay(1);
							if (dirt.crop == null)
							{
								break;
							}
						}
					}
					return true;
				}, true);
			}

			// Token: 0x06003B88 RID: 15240 RVA: 0x002E2050 File Offset: 0x002E0250
			[OtherNames(new string[]
			{
				"c",
				"cm"
			})]
			public static void CanMove(string[] command, IGameLogger log)
			{
				Game1.player.isEating = false;
				Game1.player.CanMove = true;
				Game1.player.UsingTool = false;
				Game1.player.usingSlingshot = false;
				Game1.player.FarmerSprite.PauseForSingleAnimation = false;
				FishingRod fishingRod = Game1.player.CurrentTool as FishingRod;
				if (fishingRod != null)
				{
					fishingRod.isFishing = false;
				}
				Horse mount = Game1.player.mount;
				if (mount == null)
				{
					return;
				}
				mount.dismount(false);
			}

			// Token: 0x06003B89 RID: 15241 RVA: 0x002E20C8 File Offset: 0x002E02C8
			public static void Backpack(string[] command, IGameLogger log)
			{
				int increaseBy;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out increaseBy, out error, "int increaseBy"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.increaseBackpackSize(Math.Min(36 - Game1.player.Items.Count, increaseBy));
			}

			// Token: 0x06003B8A RID: 15242 RVA: 0x002E2114 File Offset: 0x002E0314
			public static void Question(string[] command, IGameLogger log)
			{
				string questionId;
				string error;
				bool seen;
				if (!ArgUtility.TryGet(command, 1, out questionId, out error, false, "string questionId") || !ArgUtility.TryGetOptionalBool(command, 2, out seen, out error, true, "bool seen"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.dialogueQuestionsAnswered.Toggle(questionId, seen);
			}

			// Token: 0x06003B8B RID: 15243 RVA: 0x002E2164 File Offset: 0x002E0364
			public static void Year(string[] command, IGameLogger log)
			{
				int year;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out year, out error, "int year"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.year = year;
			}

			// Token: 0x06003B8C RID: 15244 RVA: 0x002E2194 File Offset: 0x002E0394
			public static void Day(string[] command, IGameLogger log)
			{
				int day;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out day, out error, "int day"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.stats.DaysPlayed = (uint)(Game1.seasonIndex * 28 + day + (Game1.year - 1) * 4 * 28);
				Game1.dayOfMonth = day;
			}

			// Token: 0x06003B8D RID: 15245 RVA: 0x002E21E4 File Offset: 0x002E03E4
			public static void Season(string[] command, IGameLogger log)
			{
				Season season;
				string error;
				if (!ArgUtility.TryGetEnum<Season>(command, 1, out season, out error, "Season season"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.season = season;
				Game1.setGraphicsForSeason(false);
			}

			// Token: 0x06003B8E RID: 15246 RVA: 0x002E2218 File Offset: 0x002E0418
			[OtherNames(new string[]
			{
				"dialogue"
			})]
			public static void AddDialogue(string[] command, IGameLogger log)
			{
				string search;
				string error;
				string dialogueText;
				if (!ArgUtility.TryGet(command, 1, out search, out error, false, "string search") || !ArgUtility.TryGetRemainder(command, 2, out dialogueText, out error, ' ', "string dialogueText"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(search, true);
				if (npc == null)
				{
					log.Error("No NPC found matching search '" + search + "'.", null);
					return;
				}
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}

			// Token: 0x06003B8F RID: 15247 RVA: 0x002E2288 File Offset: 0x002E0488
			public static void Speech(string[] command, IGameLogger log)
			{
				string search;
				string error;
				string dialogueText;
				if (!ArgUtility.TryGet(command, 1, out search, out error, false, "string search") || !ArgUtility.TryGetRemainder(command, 2, out dialogueText, out error, ' ', "string dialogueText"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(search, true);
				if (npc == null)
				{
					log.Error("No NPC found matching search '" + search + "'.", null);
					return;
				}
				Game1.DrawDialogue(new Dialogue(npc, null, dialogueText));
			}

			// Token: 0x06003B90 RID: 15248 RVA: 0x002E22F8 File Offset: 0x002E04F8
			public static void LoadDialogue(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				string translationKey;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName") || !ArgUtility.TryGet(command, 2, out translationKey, out error, false, "string translationKey"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				string text = Game1.content.LoadString(translationKey).Replace("{", "<").Replace("}", ">");
				npc.CurrentDialogue.Push(new Dialogue(npc, translationKey, text));
				Game1.drawDialogue(npc);
			}

			// Token: 0x06003B91 RID: 15249 RVA: 0x002E2384 File Offset: 0x002E0584
			public static void Wedding(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.spouse = npcName;
				Game1.weddingsToday.Add(Game1.player.UniqueMultiplayerID);
			}

			// Token: 0x06003B92 RID: 15250 RVA: 0x002E23CC File Offset: 0x002E05CC
			public static void GameMode(string[] command, IGameLogger log)
			{
				int gameMode;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out gameMode, out error, "int gameMode"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.setGameMode((byte)gameMode);
			}

			// Token: 0x06003B93 RID: 15251 RVA: 0x002E23FC File Offset: 0x002E05FC
			public static void Volcano(string[] command, IGameLogger log)
			{
				int level;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out level, out error, "int level"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.warpFarmer(VolcanoDungeon.GetLevelName(level), 0, 1, 2);
			}

			// Token: 0x06003B94 RID: 15252 RVA: 0x002E2434 File Offset: 0x002E0634
			public static void MineLevel(string[] command, IGameLogger log)
			{
				int level;
				string error;
				int layout;
				if (!ArgUtility.TryGetInt(command, 1, out level, out error, "int level") || !ArgUtility.TryGetOptionalInt(command, 2, out layout, out error, -1, "int layout"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				int? forceLayout = new int?(layout);
				int? num = forceLayout;
				int num2 = 0;
				if (num.GetValueOrDefault() < num2 & num != null)
				{
					forceLayout = null;
				}
				Game1.enterMine(level, forceLayout);
			}

			// Token: 0x06003B95 RID: 15253 RVA: 0x002E24A4 File Offset: 0x002E06A4
			public static void MineInfo(string[] command, IGameLogger log)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
				defaultInterpolatedStringHandler.AppendLiteral("MineShaft.lowestLevelReached = ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(MineShaft.lowestLevelReached);
				defaultInterpolatedStringHandler.AppendLiteral("\nplayer.deepestMineLevel = ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.player.deepestMineLevel);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003B96 RID: 15254 RVA: 0x002E2500 File Offset: 0x002E0700
			public static void Viewport(string[] command, IGameLogger log)
			{
				Point tilePosition;
				string error;
				if (!ArgUtility.TryGetPoint(command, 1, out tilePosition, out error, "Point tilePosition"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.viewport.X = tilePosition.X * 64;
				Game1.viewport.Y = tilePosition.Y * 64;
			}

			// Token: 0x06003B97 RID: 15255 RVA: 0x002E254E File Offset: 0x002E074E
			public static void MakeInedible(string[] command, IGameLogger log)
			{
				if (Game1.player.ActiveObject != null)
				{
					Game1.player.ActiveObject.edibility.Value = -300;
				}
			}

			// Token: 0x06003B98 RID: 15256 RVA: 0x002E2578 File Offset: 0x002E0778
			[OtherNames(new string[]
			{
				"watm"
			})]
			public static void WarpAnimalToMe(string[] command, IGameLogger log)
			{
				string animalName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out animalName, out error, false, "string animalName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				FarmAnimal animal = Utility.fuzzyAnimalSearch(animalName);
				if (animal == null)
				{
					log.Info("Couldn't find character named " + animalName);
					return;
				}
				log.Info("Warping " + animal.displayName);
				animal.currentLocation.Animals.Remove(animal.myID.Value);
				Game1.currentLocation.Animals.Add(animal.myID.Value, animal);
				animal.Position = Game1.player.Position;
				animal.controller = null;
			}

			// Token: 0x06003B99 RID: 15257 RVA: 0x002E2624 File Offset: 0x002E0824
			[OtherNames(new string[]
			{
				"wctm"
			})]
			public static void WarpCharacterToMe(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, false);
				if (npc == null)
				{
					log.Error("Couldn't find character named " + npcName, null);
					return;
				}
				log.Info("Warping " + npc.displayName);
				Game1.warpCharacter(npc, Game1.currentLocation.Name, new Vector2((float)Game1.player.TilePoint.X, (float)Game1.player.TilePoint.Y));
				npc.controller = null;
				npc.Halt();
			}

			// Token: 0x06003B9A RID: 15258 RVA: 0x002E26C4 File Offset: 0x002E08C4
			[OtherNames(new string[]
			{
				"wc"
			})]
			public static void WarpCharacter(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				Point tile;
				int facingDirection;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName") || !ArgUtility.TryGetPoint(command, 2, out tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(command, 4, out facingDirection, out error, 2, "int facingDirection"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, false);
				if (npc == null)
				{
					log.Error("Couldn't find character named " + npcName, null);
					return;
				}
				Game1.warpCharacter(npc, Game1.currentLocation.Name, tile);
				npc.faceDirection(facingDirection);
				npc.controller = null;
				npc.Halt();
			}

			// Token: 0x06003B9B RID: 15259 RVA: 0x002E275C File Offset: 0x002E095C
			[OtherNames(new string[]
			{
				"wtp"
			})]
			public static void WarpToPlayer(string[] command, IGameLogger log)
			{
				string playerName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out playerName, out error, false, "string playerName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Farmer otherFarmer = Game1.getOnlineFarmers().FirstOrDefault((Farmer other) => other.displayName.EqualsIgnoreCase(playerName));
				if (otherFarmer == null)
				{
					log.Error("Could not find other farmer " + playerName, null);
					return;
				}
				Game1 game = Game1.game1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
				defaultInterpolatedStringHandler.AppendFormatted("Warp");
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(otherFarmer.currentLocation.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(otherFarmer.TilePoint.X);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(otherFarmer.TilePoint.Y);
				game.parseDebugInput(defaultInterpolatedStringHandler.ToStringAndClear(), log);
			}

			// Token: 0x06003B9C RID: 15260 RVA: 0x002E2844 File Offset: 0x002E0A44
			[OtherNames(new string[]
			{
				"wtc"
			})]
			public static void WarpToCharacter(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				if (npc == null)
				{
					log.Error("Could not find valid character " + npcName, null);
					return;
				}
				Game1 game = Game1.game1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
				defaultInterpolatedStringHandler.AppendFormatted("Warp");
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(Utility.getGameLocationOfCharacter(npc).Name);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(npc.TilePoint.X);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(npc.TilePoint.Y);
				game.parseDebugInput(defaultInterpolatedStringHandler.ToStringAndClear(), log);
			}

			// Token: 0x06003B9D RID: 15261 RVA: 0x002E2910 File Offset: 0x002E0B10
			[OtherNames(new string[]
			{
				"wct"
			})]
			public static void WarpCharacterTo(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				string locationName;
				Point tile;
				int facingDirection;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName") || !ArgUtility.TryGet(command, 2, out locationName, out error, false, "string locationName") || !ArgUtility.TryGetPoint(command, 3, out tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(command, 5, out facingDirection, out error, 2, "int facingDirection"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				if (npc == null)
				{
					log.Error("Could not find valid character " + npcName, null);
					return;
				}
				Game1.warpCharacter(npc, locationName, tile);
				npc.faceDirection(facingDirection);
				npc.controller = null;
				npc.Halt();
			}

			// Token: 0x06003B9E RID: 15262 RVA: 0x002E29B4 File Offset: 0x002E0BB4
			[OtherNames(new string[]
			{
				"ws"
			})]
			public static void WarpShop(string[] command, IGameLogger log)
			{
				string shopKey;
				string error;
				if (!ArgUtility.TryGet(command, 1, out shopKey, out error, false, "string shopKey"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				string text = shopKey.ToLower();
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
					{
						char c = text[0];
						if (c != 'g')
						{
							if (c == 'p')
							{
								if (text == "pam")
								{
									Game1.game1.parseDebugInput("Warp BusStop 7 12", log);
									Game1.game1.parseDebugInput("WarpCharacterTo Pam BusStop 11 10", log);
									return;
								}
							}
						}
						else if (text == "gus")
						{
							Game1.game1.parseDebugInput("Warp Saloon 10 20", log);
							Game1.game1.parseDebugInput("WarpCharacterTo Gus Saloon 10 18", log);
							return;
						}
						break;
					}
					case 5:
					{
						char c = text[0];
						if (c <= 'd')
						{
							if (c != 'c')
							{
								if (c == 'd')
								{
									if (text == "dwarf")
									{
										Game1.game1.parseDebugInput("Warp Mine 43 7", log);
										return;
									}
								}
							}
							else if (text == "clint")
							{
								Game1.game1.parseDebugInput("Warp Blacksmith 3 15", log);
								Game1.game1.parseDebugInput("WarpCharacterTo Clint Blacksmith 3 13", log);
								return;
							}
						}
						else if (c != 'r')
						{
							if (c != 's')
							{
								if (c == 'w')
								{
									if (text == "willy")
									{
										Game1.game1.parseDebugInput("Warp FishShop 6 6", log);
										Game1.game1.parseDebugInput("WarpCharacterTo Willy FishShop 6 4", log);
										return;
									}
								}
							}
							else if (text == "sandy")
							{
								Game1.game1.parseDebugInput("Warp SandyHouse 2 7", log);
								Game1.game1.parseDebugInput("WarpCharacterTo Sandy SandyHouse 2 5", log);
								return;
							}
						}
						else if (text == "robin")
						{
							Game1.game1.parseDebugInput("Warp ScienceHouse 8 20", log);
							Game1.game1.parseDebugInput("WarpCharacterTo Robin ScienceHouse 8 18", log);
							return;
						}
						break;
					}
					case 6:
					{
						char c = text[0];
						if (c <= 'm')
						{
							if (c != 'k')
							{
								if (c == 'm')
								{
									if (text == "marnie")
									{
										Game1.game1.parseDebugInput("Warp AnimalShop 12 16", log);
										Game1.game1.parseDebugInput("WarpCharacterTo Marnie AnimalShop 12 14", log);
										return;
									}
								}
							}
							else if (text == "krobus")
							{
								Game1.game1.parseDebugInput("Warp Sewer 31 19", log);
								return;
							}
						}
						else if (c != 'p')
						{
							if (c == 'w')
							{
								if (text == "wizard")
								{
									Game1.player.eventsSeen.Add("418172");
									Game1.player.hasMagicInk = true;
									Game1.game1.parseDebugInput("Warp WizardHouse 2 14", log);
									return;
								}
							}
						}
						else if (text == "pierre")
						{
							Game1.game1.parseDebugInput("Warp SeedShop 4 19", log);
							Game1.game1.parseDebugInput("WarpCharacterTo Pierre SeedShop 4 17", log);
							return;
						}
						break;
					}
					}
				}
				log.Error("That npc doesn't have a shop or it isn't handled by this command", null);
			}

			// Token: 0x06003B9F RID: 15263 RVA: 0x002E2D10 File Offset: 0x002E0F10
			public static void FacePlayer(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				if (npc == null)
				{
					log.Error("Can't find NPC '" + npcName + "'.", null);
					return;
				}
				npc.faceTowardFarmer = true;
			}

			// Token: 0x06003BA0 RID: 15264 RVA: 0x002E2D64 File Offset: 0x002E0F64
			public static void Refuel(string[] command, IGameLogger log)
			{
				Lantern lantern = Game1.player.getToolFromName("Lantern") as Lantern;
				if (lantern != null)
				{
					lantern.fuelLeft = 100;
				}
			}

			// Token: 0x06003BA1 RID: 15265 RVA: 0x002E2D91 File Offset: 0x002E0F91
			public static void Lantern(string[] command, IGameLogger log)
			{
				Game1.player.Items.Add(ItemRegistry.Create("(T)Lantern", 1, 0, false));
			}

			// Token: 0x06003BA2 RID: 15266 RVA: 0x002E2DB0 File Offset: 0x002E0FB0
			public static void GrowGrass(string[] command, IGameLogger log)
			{
				int iterations;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out iterations, out error, "int iterations"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.currentLocation.spawnWeeds(false);
				Game1.currentLocation.growWeedGrass(iterations);
			}

			// Token: 0x06003BA3 RID: 15267 RVA: 0x002E2DF0 File Offset: 0x002E0FF0
			public static void AddAllCrafting(string[] command, IGameLogger log)
			{
				foreach (string s in CraftingRecipe.craftingRecipes.Keys)
				{
					Game1.player.craftingRecipes.Add(s, 0);
				}
			}

			// Token: 0x06003BA4 RID: 15268 RVA: 0x002E2E54 File Offset: 0x002E1054
			public static void Animal(string[] command, IGameLogger log)
			{
				string animalName;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out animalName, out error, ' ', "string animalName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Utility.addAnimalToFarm(new FarmAnimal(animalName.Trim(), Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID));
			}

			// Token: 0x06003BA5 RID: 15269 RVA: 0x002E2EA4 File Offset: 0x002E10A4
			public static void MoveBuilding(string[] command, IGameLogger log)
			{
				Vector2 fromTile;
				string error;
				Point toTile;
				if (!ArgUtility.TryGetVector2(command, 1, out fromTile, out error, true, "Vector2 fromTile") || !ArgUtility.TryGetPoint(command, 3, out toTile, out error, "Point toTile"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation location = Game1.currentLocation;
				if (location == null)
				{
					return;
				}
				Building building = location.getBuildingAt(fromTile);
				if (building == null)
				{
					return;
				}
				building.tileX.Value = toTile.X;
				building.tileY.Value = toTile.Y;
			}

			// Token: 0x06003BA6 RID: 15270 RVA: 0x002E2F1C File Offset: 0x002E111C
			public static void Fishing(string[] command, IGameLogger log)
			{
				int level;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out level, out error, "int level"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.fishingLevel.Value = level;
			}

			// Token: 0x06003BA7 RID: 15271 RVA: 0x002E2F54 File Offset: 0x002E1154
			[OtherNames(new string[]
			{
				"fd",
				"face"
			})]
			public static void FaceDirection(string[] command, IGameLogger log)
			{
				string targetName;
				string error;
				int facingDirection;
				if (!ArgUtility.TryGet(command, 1, out targetName, out error, false, "string targetName") || !ArgUtility.TryGetInt(command, 2, out facingDirection, out error, "int facingDirection"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (targetName == "farmer")
				{
					Game1.player.Halt();
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.player.faceDirection(facingDirection);
					return;
				}
				Utility.fuzzyCharacterSearch(targetName, true).faceDirection(facingDirection);
			}

			// Token: 0x06003BA8 RID: 15272 RVA: 0x002E2FCC File Offset: 0x002E11CC
			public static void Note(string[] command, IGameLogger log)
			{
				int noteId;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out noteId, out error, "int noteId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				int[] data;
				if (!Game1.player.archaeologyFound.TryGetValue("102", out data))
				{
					data = (Game1.player.archaeologyFound["102"] = new int[2]);
				}
				data[0] = 18;
				Game1.netWorldState.Value.LostBooksFound = 18;
				Game1.currentLocation.readNote(noteId);
			}

			// Token: 0x06003BA9 RID: 15273 RVA: 0x002E3049 File Offset: 0x002E1249
			public static void NetHost(string[] command, IGameLogger log)
			{
				Game1.multiplayer.StartServer();
			}

			// Token: 0x06003BAA RID: 15274 RVA: 0x002E3058 File Offset: 0x002E1258
			public static void NetJoin(string[] command, IGameLogger log)
			{
				string address;
				string error;
				if (!ArgUtility.TryGet(command, 1, out address, out error, false, "string address"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				FarmhandMenu farmhandMenu = new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address)));
				if (Game1.activeClickableMenu is TitleMenu)
				{
					TitleMenu.subMenu = farmhandMenu;
					return;
				}
				Game1.ExitToTitle(delegate
				{
					(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
					TitleMenu.subMenu = farmhandMenu;
				});
			}

			// Token: 0x06003BAB RID: 15275 RVA: 0x002E30CC File Offset: 0x002E12CC
			public static void ToggleNetCompression(string[] command, IGameLogger log)
			{
				DebugCommands.DefaultHandlers.<>c__DisplayClass300_0 CS$<>8__locals1 = new DebugCommands.DefaultHandlers.<>c__DisplayClass300_0();
				CS$<>8__locals1.log = log;
				if (Program.defaultCompression.GetType() == typeof(NullNetCompression))
				{
					CS$<>8__locals1.log.Error("This command can only be used on platforms that support compression.", null);
					return;
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					CS$<>8__locals1.<ToggleNetCompression>g__ToggleCompression|0();
					return;
				}
				Game1.ExitToTitle(delegate
				{
					(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
					base.<ToggleNetCompression>g__ToggleCompression|0();
				});
			}

			// Token: 0x06003BAC RID: 15276 RVA: 0x002E3138 File Offset: 0x002E1338
			public static void LevelUp(string[] command, IGameLogger log)
			{
				int skill;
				string error;
				int level;
				if (!ArgUtility.TryGetInt(command, 1, out skill, out error, "int skill") || !ArgUtility.TryGetInt(command, 2, out level, out error, "int level"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.activeClickableMenu = new LevelUpMenu(skill, level);
			}

			// Token: 0x06003BAD RID: 15277 RVA: 0x002E317E File Offset: 0x002E137E
			public static void Darts(string[] command, IGameLogger log)
			{
				Game1.currentMinigame = new Darts(20);
			}

			// Token: 0x06003BAE RID: 15278 RVA: 0x002E318C File Offset: 0x002E138C
			public static void MineGame(string[] command, IGameLogger log)
			{
				string mode;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out mode, out error, null, false, "string mode"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				int gameMode = (mode == "infinite") ? 2 : 3;
				Game1.currentMinigame = new MineCart(0, gameMode);
			}

			// Token: 0x06003BAF RID: 15279 RVA: 0x002E31D4 File Offset: 0x002E13D4
			public static void Crane(string[] command, IGameLogger log)
			{
				Game1.currentMinigame = new CraneGame();
			}

			// Token: 0x06003BB0 RID: 15280 RVA: 0x002E31E0 File Offset: 0x002E13E0
			[OtherNames(new string[]
			{
				"trlt"
			})]
			public static void TailorRecipeListTool(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new TailorRecipeListTool();
			}

			// Token: 0x06003BB1 RID: 15281 RVA: 0x002E31EC File Offset: 0x002E13EC
			[OtherNames(new string[]
			{
				"apt"
			})]
			public static void AnimationPreviewTool(string[] command, IGameLogger log)
			{
				Game1.activeClickableMenu = new AnimationPreviewTool();
			}

			// Token: 0x06003BB2 RID: 15282 RVA: 0x002E31F8 File Offset: 0x002E13F8
			public static void CreateDino(string[] command, IGameLogger log)
			{
				Game1.currentLocation.characters.Add(new DinoMonster(Game1.player.position.Value + new Vector2(100f, 0f)));
			}

			// Token: 0x06003BB3 RID: 15283 RVA: 0x002E3234 File Offset: 0x002E1434
			[OtherNames(new string[]
			{
				"pta"
			})]
			public static void PerformTitleAction(string[] command, IGameLogger log)
			{
				string titleAction;
				string error;
				if (!ArgUtility.TryGet(command, 1, out titleAction, out error, false, "string titleAction"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
				if (titleMenu != null)
				{
					titleMenu.performButtonAction(titleAction);
					return;
				}
				Game1.ExitToTitle(delegate
				{
					TitleMenu menu = Game1.activeClickableMenu as TitleMenu;
					if (menu != null)
					{
						menu.skipToTitleButtons();
						menu.performButtonAction(titleAction);
					}
				});
			}

			// Token: 0x06003BB4 RID: 15284 RVA: 0x002E3294 File Offset: 0x002E1494
			public static void Action(string[] command, IGameLogger log)
			{
				string action;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out action, out error, ' ', "string action"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Exception ex;
				if (TriggerActionManager.TryRunAction(action, out error, out ex))
				{
					log.Info("Applied action '" + action + "'.");
					return;
				}
				log.Error("Couldn't apply action '" + action + "': " + error, ex);
			}

			// Token: 0x06003BB5 RID: 15285 RVA: 0x002E32FC File Offset: 0x002E14FC
			public static void BroadcastMail(string[] command, IGameLogger log)
			{
				string mailId;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out mailId, out error, ' ', "string mailId"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.addMailForTomorrow(mailId, false, true);
			}

			// Token: 0x06003BB6 RID: 15286 RVA: 0x002E332E File Offset: 0x002E152E
			public static void Phone(string[] command, IGameLogger log)
			{
				Game1.game1.ShowTelephoneMenu();
			}

			// Token: 0x06003BB7 RID: 15287 RVA: 0x002E333A File Offset: 0x002E153A
			public static void Renovate(string[] command, IGameLogger log)
			{
				HouseRenovation.ShowRenovationMenu();
			}

			// Token: 0x06003BB8 RID: 15288 RVA: 0x002E3344 File Offset: 0x002E1544
			public static void Crib(string[] command, IGameLogger log)
			{
				int style;
				string error;
				if (!ArgUtility.TryGetInt(command, 1, out style, out error, "int style"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				FarmHouse house = Game1.getLocationFromName(Game1.player.homeLocation.Value) as FarmHouse;
				if (house != null)
				{
					house.cribStyle.Value = style;
				}
			}

			// Token: 0x06003BB9 RID: 15289 RVA: 0x002E3395 File Offset: 0x002E1595
			public static void TestNut(string[] command, IGameLogger log)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), Vector2.Zero, 2, null, -1, false);
			}

			// Token: 0x06003BBA RID: 15290 RVA: 0x002E33B3 File Offset: 0x002E15B3
			public static void ShuffleBundles(string[] command, IGameLogger log)
			{
				Game1.GenerateBundles(Game1.BundleType.Remixed, false);
			}

			// Token: 0x06003BBB RID: 15291 RVA: 0x002E33BC File Offset: 0x002E15BC
			public static void Split(string[] command, IGameLogger log)
			{
				int playerIndex;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out playerIndex, out error, -1, "int playerIndex"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (playerIndex > -1)
				{
					GameRunner.instance.AddGameInstance((PlayerIndex)playerIndex);
					return;
				}
				Game1.game1.ShowLocalCoopJoinMenu();
			}

			// Token: 0x06003BBC RID: 15292 RVA: 0x002E3400 File Offset: 0x002E1600
			[OtherNames(new string[]
			{
				"bsm"
			})]
			public static void SkinBuilding(string[] command, IGameLogger log)
			{
				GameLocation currentLocation = Game1.currentLocation;
				Building building = (currentLocation != null) ? currentLocation.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f)) : null;
				if (building == null)
				{
					log.Error("No building found in front of player.", null);
					return;
				}
				if (building.CanBeReskinned(false))
				{
					Game1.activeClickableMenu = new BuildingSkinMenu(building, false);
					return;
				}
				log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be skinned.", null);
			}

			// Token: 0x06003BBD RID: 15293 RVA: 0x002E3484 File Offset: 0x002E1684
			[OtherNames(new string[]
			{
				"bpm"
			})]
			public static void PaintBuilding(string[] command, IGameLogger log)
			{
				GameLocation currentLocation = Game1.currentLocation;
				Building building = (currentLocation != null) ? currentLocation.getBuildingAt(Game1.player.Tile + new Vector2(0f, -1f)) : null;
				if (building != null)
				{
					if (building.CanBePainted())
					{
						Game1.activeClickableMenu = new BuildingPaintMenu(building);
						return;
					}
					log.Error("The '" + building.buildingType.Value + "' building in front of the player can't be painted. Defaulting to main farmhouse.", null);
				}
				Building farmhouse = Game1.getFarm().GetMainFarmHouse();
				if (farmhouse == null)
				{
					log.Error("The main farmhouse wasn't found.", null);
					return;
				}
				if (!farmhouse.CanBePainted())
				{
					log.Error("The main farmhouse can't be painted.", null);
					return;
				}
				Game1.activeClickableMenu = new BuildingPaintMenu(farmhouse);
			}

			// Token: 0x06003BBE RID: 15294 RVA: 0x002E3534 File Offset: 0x002E1734
			[OtherNames(new string[]
			{
				"md"
			})]
			public static void MineDifficulty(string[] command, IGameLogger log)
			{
				int difficulty;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out difficulty, out error, -1, "int difficulty"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (difficulty > -1)
				{
					Game1.netWorldState.Value.MinesDifficulty = difficulty;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Mine difficulty: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.netWorldState.Value.MinesDifficulty);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BBF RID: 15295 RVA: 0x002E35AC File Offset: 0x002E17AC
			[OtherNames(new string[]
			{
				"scd"
			})]
			public static void SkullCaveDifficulty(string[] command, IGameLogger log)
			{
				int difficulty;
				string error;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out difficulty, out error, -1, "int difficulty"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (difficulty > -1)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty = difficulty;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Skull Cave difficulty: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.netWorldState.Value.SkullCavesDifficulty);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BC0 RID: 15296 RVA: 0x002E3624 File Offset: 0x002E1824
			[OtherNames(new string[]
			{
				"tls"
			})]
			public static void ToggleLightingScale(string[] command, IGameLogger log)
			{
				Game1.game1.useUnscaledLighting = !Game1.game1.useUnscaledLighting;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Toggled Lighting Scale: useUnscaledLighting: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(Game1.game1.useUnscaledLighting);
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BC1 RID: 15297 RVA: 0x002E367C File Offset: 0x002E187C
			public static void FixWeapons(string[] command, IGameLogger log)
			{
				SaveMigrator_1_5.ResetForges();
				log.Info("Reset forged weapon attributes.");
			}

			// Token: 0x06003BC2 RID: 15298 RVA: 0x002E3690 File Offset: 0x002E1890
			[OtherNames(new string[]
			{
				"plsf"
			})]
			public static void PrintLatestSaveFix(string[] command, IGameLogger log)
			{
				SaveFixes latestFix = SaveFixes.FixDuplicateMissedMail;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The latest save fix is '");
				defaultInterpolatedStringHandler.AppendFormatted(latestFix.ToString());
				defaultInterpolatedStringHandler.AppendLiteral("' (ID: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)latestFix);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BC3 RID: 15299 RVA: 0x002E36F8 File Offset: 0x002E18F8
			[OtherNames(new string[]
			{
				"pdb"
			})]
			public static void PrintGemBirds(string[] command, IGameLogger log)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Gem birds: North ");
				defaultInterpolatedStringHandler.AppendFormatted<IslandGemBird.GemBirdType>(IslandGemBird.GetBirdTypeForLocation("IslandNorth"));
				defaultInterpolatedStringHandler.AppendLiteral(" South ");
				defaultInterpolatedStringHandler.AppendFormatted<IslandGemBird.GemBirdType>(IslandGemBird.GetBirdTypeForLocation("IslandSouth"));
				defaultInterpolatedStringHandler.AppendLiteral(" East ");
				defaultInterpolatedStringHandler.AppendFormatted<IslandGemBird.GemBirdType>(IslandGemBird.GetBirdTypeForLocation("IslandEast"));
				defaultInterpolatedStringHandler.AppendLiteral(" West ");
				defaultInterpolatedStringHandler.AppendFormatted<IslandGemBird.GemBirdType>(IslandGemBird.GetBirdTypeForLocation("IslandWest"));
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BC4 RID: 15300 RVA: 0x002E3790 File Offset: 0x002E1990
			[OtherNames(new string[]
			{
				"ppp"
			})]
			public static void PrintPlayerPos(string[] command, IGameLogger log)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Player tile position is ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(Game1.player.Tile);
				defaultInterpolatedStringHandler.AppendLiteral(" (World position: ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(Game1.player.Position);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BC5 RID: 15301 RVA: 0x002E37FC File Offset: 0x002E19FC
			public static void ShowPlurals(string[] command, IGameLogger log)
			{
				List<string> item_names = new List<string>();
				foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					item_names.Add(data.InternalName);
				}
				foreach (ParsedItemData data2 in ItemRegistry.RequireTypeDefinition("(BC)").GetAllData())
				{
					item_names.Add(data2.InternalName);
				}
				item_names.Sort();
				foreach (string item_name in item_names)
				{
					log.Info(Lexicon.makePlural(item_name, false));
				}
			}

			// Token: 0x06003BC6 RID: 15302 RVA: 0x002E38F0 File Offset: 0x002E1AF0
			public static void HoldItem(string[] command, IGameLogger log)
			{
				bool showMessage;
				string error;
				if (!ArgUtility.TryGetOptionalBool(command, 1, out showMessage, out error, false, "bool showMessage"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				Game1.player.holdUpItemThenMessage(Game1.player.CurrentItem, showMessage);
			}

			// Token: 0x06003BC7 RID: 15303 RVA: 0x002E3930 File Offset: 0x002E1B30
			[OtherNames(new string[]
			{
				"rm"
			})]
			public static void RunMacro(string[] command, IGameLogger log)
			{
				string fileName;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out fileName, out error, "macro.txt", false, "string fileName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (Game1.isRunningMacro)
				{
					log.Error("You cannot run a macro from within a macro.", null);
					return;
				}
				Game1.isRunningMacro = true;
				try
				{
					StreamReader file = new StreamReader(fileName);
					string line;
					while ((line = file.ReadLine()) != null)
					{
						Game1.chatBox.textBoxEnter(line);
					}
					log.Info("Executed macro file " + fileName);
					file.Close();
				}
				catch (Exception e)
				{
					log.Error("Error running macro file " + fileName + ".", e);
				}
				Game1.isRunningMacro = false;
			}

			// Token: 0x06003BC8 RID: 15304 RVA: 0x002E39E4 File Offset: 0x002E1BE4
			public static void InviteMovie(string[] command, IGameLogger log)
			{
				string npcName;
				string error;
				if (!ArgUtility.TryGet(command, 1, out npcName, out error, false, "string npcName"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				NPC npc = Utility.fuzzyCharacterSearch(npcName, true);
				if (npc == null)
				{
					log.Error("Invalid NPC", null);
					return;
				}
				MovieTheater.Invite(Game1.player, npc);
			}

			// Token: 0x06003BC9 RID: 15305 RVA: 0x002E3A34 File Offset: 0x002E1C34
			public static void Monster(string[] command, IGameLogger log)
			{
				string typeName;
				string error;
				Point tile;
				string monsterNameOrNumber;
				if (!ArgUtility.TryGet(command, 1, out typeName, out error, false, "string typeName") || !ArgUtility.TryGetPoint(command, 2, out tile, out error, "Point tile") || !ArgUtility.TryGetOptionalRemainder(command, 4, out monsterNameOrNumber, null, ' '))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				string fullTypeName = "StardewValley.Monsters." + typeName;
				Type monsterType = Type.GetType(fullTypeName);
				if (monsterType == null)
				{
					log.Error("There's no monster with type '" + fullTypeName + "'.", null);
					return;
				}
				Vector2 pos = new Vector2((float)(tile.X * 64), (float)(tile.Y * 64));
				object[] args;
				int numberArg;
				if (string.IsNullOrWhiteSpace(monsterNameOrNumber))
				{
					args = new object[]
					{
						pos
					};
				}
				else if (int.TryParse(monsterNameOrNumber, out numberArg))
				{
					args = new object[]
					{
						pos,
						numberArg
					};
				}
				else
				{
					args = new object[]
					{
						pos,
						monsterNameOrNumber
					};
				}
				Monster mon = Activator.CreateInstance(monsterType, args) as Monster;
				Game1.currentLocation.characters.Add(mon);
			}

			// Token: 0x06003BCA RID: 15306 RVA: 0x002E3B48 File Offset: 0x002E1D48
			[OtherNames(new string[]
			{
				"shaft"
			})]
			public static void Ladder(string[] command, IGameLogger log)
			{
				int tileX;
				string error;
				int tileY;
				if (!ArgUtility.TryGetOptionalInt(command, 1, out tileX, out error, Game1.player.TilePoint.X, "int tileX") || !ArgUtility.TryGetOptionalInt(command, 2, out tileY, out error, Game1.player.TilePoint.Y + 1, "int tileY"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				bool forceShaft = command[0].EqualsIgnoreCase("shaft");
				Game1.mine.createLadderDown(tileX, tileY, forceShaft);
			}

			// Token: 0x06003BCB RID: 15307 RVA: 0x002E3BC0 File Offset: 0x002E1DC0
			public static void NetLog(string[] command, IGameLogger log)
			{
				Game1.multiplayer.logging.IsLogging = !Game1.multiplayer.logging.IsLogging;
				log.Info("Turned " + (Game1.multiplayer.logging.IsLogging ? "on" : "off") + " network write logging");
			}

			// Token: 0x06003BCC RID: 15308 RVA: 0x002E3C20 File Offset: 0x002E1E20
			public static void NetClear(string[] command, IGameLogger log)
			{
				Game1.multiplayer.logging.Clear();
			}

			// Token: 0x06003BCD RID: 15309 RVA: 0x002E3C31 File Offset: 0x002E1E31
			public static void NetDump(string[] command, IGameLogger log)
			{
				log.Info("Wrote log to " + Game1.multiplayer.logging.Dump());
			}

			// Token: 0x06003BCE RID: 15310 RVA: 0x002E3C54 File Offset: 0x002E1E54
			[OtherNames(new string[]
			{
				"tto"
			})]
			public static void ToggleTimingOverlay(string[] command, IGameLogger log)
			{
				Game1 game = Game1.game1;
				if (!(((game != null) ? new bool?(game.IsMainInstance) : null) ?? false))
				{
					log.Error("Cannot toggle timing overlay as a splitscreen instance.", null);
					return;
				}
				log.Info((Game1.debugTimings.Toggle() ? "Enabled" : "Disabled") + " in-game timing overlay.");
			}

			// Token: 0x06003BCF RID: 15311 RVA: 0x002E3CCC File Offset: 0x002E1ECC
			public static void LogBandwidth(string[] command, IGameLogger log)
			{
				if (Game1.IsServer)
				{
					Game1.server.LogBandwidth = !Game1.server.LogBandwidth;
					log.Info("Turned " + (Game1.server.LogBandwidth ? "on" : "off") + " server bandwidth logging");
					return;
				}
				if (Game1.IsClient)
				{
					Game1.client.LogBandwidth = !Game1.client.LogBandwidth;
					log.Info("Turned " + (Game1.client.LogBandwidth ? "on" : "off") + " client bandwidth logging");
					return;
				}
				log.Error("Cannot toggle bandwidth logging in non-multiplayer games", null);
			}

			// Token: 0x06003BD0 RID: 15312 RVA: 0x002E3D7D File Offset: 0x002E1F7D
			public static void LogWallAndFloorWarnings(string[] command, IGameLogger log)
			{
				DecoratableLocation.LogTroubleshootingInfo = !DecoratableLocation.LogTroubleshootingInfo;
				log.Info((DecoratableLocation.LogTroubleshootingInfo ? "Enabled" : "Disabled") + " wall and floor warning logs.");
			}

			// Token: 0x06003BD1 RID: 15313 RVA: 0x002E3DAF File Offset: 0x002E1FAF
			public static void ChangeWallet(string[] command, IGameLogger log)
			{
				if (Game1.IsMasterGame)
				{
					Game1.player.changeWalletTypeTonight.Value = true;
				}
			}

			// Token: 0x06003BD2 RID: 15314 RVA: 0x002E3DC8 File Offset: 0x002E1FC8
			public static void SeparateWallets(string[] command, IGameLogger log)
			{
				if (Game1.IsMasterGame)
				{
					ManorHouse.SeparateWallets();
				}
			}

			// Token: 0x06003BD3 RID: 15315 RVA: 0x002E3DD6 File Offset: 0x002E1FD6
			public static void MergeWallets(string[] command, IGameLogger log)
			{
				if (Game1.IsMasterGame)
				{
					ManorHouse.MergeWallets();
				}
			}

			// Token: 0x06003BD4 RID: 15316 RVA: 0x002E3DE4 File Offset: 0x002E1FE4
			[OtherNames(new string[]
			{
				"nd",
				"newDay",
				"s"
			})]
			public static void Sleep(string[] command, IGameLogger log)
			{
				Game1.player.isInBed.Value = true;
				Game1.player.sleptInTemporaryBed.Value = true;
				Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);
			}

			// Token: 0x06003BD5 RID: 15317 RVA: 0x002E3E18 File Offset: 0x002E2018
			[OtherNames(new string[]
			{
				"gm",
				"inv"
			})]
			public static void Invincible(string[] command, IGameLogger log)
			{
				if (Game1.player.temporarilyInvincible)
				{
					Game1.player.temporaryInvincibilityTimer = 0;
					Game1.playSound("bigDeSelect", null);
					return;
				}
				Game1.player.temporarilyInvincible = true;
				Game1.player.temporaryInvincibilityTimer = -1000000000;
				Game1.playSound("bigSelect", null);
			}

			// Token: 0x06003BD6 RID: 15318 RVA: 0x002E3E7F File Offset: 0x002E207F
			public static void ValidateNetFields(string[] command, IGameLogger log)
			{
				NetFields.ShouldValidateNetFields = !NetFields.ShouldValidateNetFields;
				log.Info(NetFields.ShouldValidateNetFields ? "Enabled net field validation, which may impact performance. This only affects new net fields created after it's enabled." : "Disabled net field validation.");
			}

			// Token: 0x06003BD7 RID: 15319 RVA: 0x002E3EA8 File Offset: 0x002E20A8
			[OtherNames(new string[]
			{
				"flm"
			})]
			public static void FilterLoadMenu(string[] command, IGameLogger log)
			{
				string filter;
				string error;
				if (!ArgUtility.TryGetRemainder(command, 1, out filter, out error, ' ', "string filter"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					IClickableMenu subMenu = TitleMenu.subMenu;
					CoopMenu coopMenu = subMenu as CoopMenu;
					if (coopMenu != null)
					{
						TitleMenu.subMenu = new CoopMenu(coopMenu.tooManyFarms, false, coopMenu.currentTab, filter);
						return;
					}
					if (!(subMenu is FarmhandMenu))
					{
						if (subMenu is LoadGameMenu)
						{
							TitleMenu.subMenu = new LoadGameMenu(filter);
							return;
						}
					}
				}
				log.Error("The FilterLoadMenu debug command must be run while the list of saved games is open.", null);
			}

			// Token: 0x06003BD8 RID: 15320 RVA: 0x002E3F34 File Offset: 0x002E2134
			public static void WorldMapLines(string[] command, IGameLogger log)
			{
				MapPage.WorldMapDebugLineType types;
				if (command.Length > 1)
				{
					if (!Utility.TryParseEnum<MapPage.WorldMapDebugLineType>(string.Join(", ", command.Skip(1)), out types))
					{
						DebugCommands.LogArgError(log, command, "unknown type '" + string.Join(" ", command.Skip(1)) + "', expected space-delimited list of " + string.Join(", ", Enum.GetNames(typeof(MapPage.WorldMapDebugLineType))));
						return;
					}
				}
				else
				{
					types = ((MapPage.EnableDebugLines == MapPage.WorldMapDebugLineType.None) ? MapPage.WorldMapDebugLineType.All : MapPage.WorldMapDebugLineType.None);
				}
				MapPage.EnableDebugLines = types;
				string message;
				if (types != MapPage.WorldMapDebugLineType.None)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
					defaultInterpolatedStringHandler.AppendLiteral("World map debug lines enabled for types ");
					defaultInterpolatedStringHandler.AppendFormatted<MapPage.WorldMapDebugLineType>(types);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					message = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					message = "World map debug lines disabled.";
				}
				log.Info(message);
			}

			// Token: 0x06003BD9 RID: 15321 RVA: 0x002E3FF8 File Offset: 0x002E21F8
			public static void WorldMapPosition(string[] command, IGameLogger log)
			{
				bool includeLog;
				string error;
				if (!ArgUtility.TryGetOptionalBool(command, 1, out includeLog, out error, false, "bool includeLog"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				GameLocation location = Game1.currentLocation;
				Point tile = Game1.player.TilePoint;
				LogBuilder logBuilder = includeLog ? new LogBuilder(3) : null;
				MapAreaPositionWithContext? rawPosition = WorldMapManager.GetPositionData(location, tile, logBuilder);
				StringBuilder result = new StringBuilder();
				if (rawPosition == null)
				{
					result.AppendLine("The player's current position didn't match any entry in Data/WorldMaps.");
				}
				else
				{
					MapAreaPositionWithContext position = rawPosition.Value;
					MapAreaPosition data = rawPosition.Value.Data;
					StringBuilder stringBuilder = result;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(33, 3, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("The player is currently at ");
					appendInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
					appendInterpolatedStringHandler.AppendLiteral(" (");
					appendInterpolatedStringHandler.AppendFormatted<int>(tile.X);
					appendInterpolatedStringHandler.AppendLiteral(", ");
					appendInterpolatedStringHandler.AppendFormatted<int>(tile.Y);
					appendInterpolatedStringHandler.AppendLiteral(").");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					if (location.NameOrUniqueName != position.Location.NameOrUniqueName || tile != position.Tile)
					{
						stringBuilder = result;
						StringBuilder stringBuilder3 = stringBuilder;
						appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(31, 3, stringBuilder);
						appendInterpolatedStringHandler.AppendLiteral("That was translated to '");
						appendInterpolatedStringHandler.AppendFormatted(position.Location.NameOrUniqueName);
						appendInterpolatedStringHandler.AppendLiteral("' (");
						appendInterpolatedStringHandler.AppendFormatted<int>(position.Tile.X);
						appendInterpolatedStringHandler.AppendLiteral(", ");
						appendInterpolatedStringHandler.AppendFormatted<int>(position.Tile.Y);
						appendInterpolatedStringHandler.AppendLiteral(").");
						stringBuilder3.AppendLine(ref appendInterpolatedStringHandler);
					}
					stringBuilder = result;
					StringBuilder stringBuilder4 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(53, 3, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("This matches region '");
					appendInterpolatedStringHandler.AppendFormatted(data.Region.Id);
					appendInterpolatedStringHandler.AppendLiteral("', area '");
					appendInterpolatedStringHandler.AppendFormatted(data.Area.Id);
					appendInterpolatedStringHandler.AppendLiteral("', and map position '");
					appendInterpolatedStringHandler.AppendFormatted(data.Data.Id);
					appendInterpolatedStringHandler.AppendLiteral("'.");
					stringBuilder4.AppendLine(ref appendInterpolatedStringHandler);
					stringBuilder = result;
					StringBuilder stringBuilder5 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(79, 3, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("The position's pixel area is ");
					appendInterpolatedStringHandler.AppendFormatted<Microsoft.Xna.Framework.Rectangle>(data.GetPixelArea());
					appendInterpolatedStringHandler.AppendLiteral(", with the player at position ");
					appendInterpolatedStringHandler.AppendFormatted<Vector2>(position.GetMapPixelPosition());
					appendInterpolatedStringHandler.AppendLiteral(" (position ratio: ");
					appendInterpolatedStringHandler.AppendFormatted<Vector2?>(position.GetPositionRatioIfValid());
					appendInterpolatedStringHandler.AppendLiteral(").");
					stringBuilder5.AppendLine(ref appendInterpolatedStringHandler);
					stringBuilder = result;
					StringBuilder stringBuilder6 = stringBuilder;
					appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendLiteral("Scroll text: ");
					appendInterpolatedStringHandler.AppendFormatted(position.GetScrollText() ?? "none");
					appendInterpolatedStringHandler.AppendLiteral(".");
					stringBuilder6.AppendLine(ref appendInterpolatedStringHandler);
				}
				result.AppendLine();
				result.AppendLine("Log:");
				if (logBuilder != null)
				{
					result.Append(logBuilder.Log);
				}
				else
				{
					result.AppendLine("   Run `debug WorldMapPosition true` to show the detailed log.");
				}
				log.Info(result.ToString());
			}

			// Token: 0x06003BDA RID: 15322 RVA: 0x002E432C File Offset: 0x002E252C
			public static void Search(string[] command, IGameLogger log)
			{
				string search;
				string error;
				if (!ArgUtility.TryGetOptional(command, 1, out search, out error, null, false, "string search"))
				{
					DebugCommands.LogArgError(log, command, error);
					return;
				}
				List<string> commands = DebugCommands.SearchCommandNames(search, true);
				if (commands.Count == 0)
				{
					log.Info("No debug commands found matching '" + search + "'.");
					return;
				}
				string str;
				if (search == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendFormatted<int>(commands.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" debug commands registered:\n");
					str = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Found ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(commands.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" debug commands matching search term '");
					defaultInterpolatedStringHandler.AppendFormatted(search);
					defaultInterpolatedStringHandler.AppendLiteral("':\n");
					str = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				log.Info(str + "  - " + string.Join("\n  - ", commands) + ((search == null) ? "\n\nTip: you can search debug commands like 'debug Search searchTermHere'." : ""));
			}

			// Token: 0x06003BDB RID: 15323 RVA: 0x002E4420 File Offset: 0x002E2620
			public static void ArtifactSpots(string[] command, IGameLogger log)
			{
				GameLocation location = Game1.player.currentLocation;
				Vector2 playerTile = Game1.player.Tile;
				if (location == null)
				{
					log.Info("You must be in a location to use this command.");
					return;
				}
				int spawned = 0;
				foreach (Vector2 tile in Utility.getSurroundingTileLocationsArray(playerTile))
				{
					TerrainFeature feature;
					if (location.terrainFeatures.TryGetValue(tile, out feature))
					{
						HoeDirt dirt = feature as HoeDirt;
						if (dirt != null && dirt.crop == null)
						{
							location.terrainFeatures.Remove(tile);
						}
					}
					if (location.isTilePassable(tile) && !location.IsTileOccupiedBy(tile, ~(CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.TerrainFeatures), CollisionMask.None, false))
					{
						location.objects.Add(tile, ItemRegistry.Create<Object>("(O)590", 1, 0, false));
						spawned++;
					}
				}
				if (spawned == 0)
				{
					log.Info("No unoccupied tiles found around the player.");
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Spawned ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(spawned);
				defaultInterpolatedStringHandler.AppendLiteral(" artifact spots around the player.");
				log.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}

			// Token: 0x06003BDC RID: 15324 RVA: 0x002E4530 File Offset: 0x002E2730
			public static void LogFile(string[] command, IGameLogger log)
			{
				DefaultLogger logger = Game1.log as DefaultLogger;
				if (logger != null)
				{
					Game1.log = new DefaultLogger(logger.ShouldWriteToConsole, !logger.ShouldWriteToLogFile);
					log.Info((logger.ShouldWriteToLogFile ? "Disabled" : "Enabled") + " the game log file at " + Program.GetDebugLogPath() + ".");
					return;
				}
				IGameLogger log2 = Game1.log;
				bool? flag;
				if (log2 == null)
				{
					flag = null;
				}
				else
				{
					string fullName = log2.GetType().FullName;
					flag = ((fullName != null) ? new bool?(fullName.StartsWith("StardewModdingAPI.")) : null);
				}
				bool? flag2 = flag;
				if (flag2 != null && flag2.GetValueOrDefault())
				{
					log.Error("The debug log can't be enabled when SMAPI is installed. SMAPI already includes log messages in its own log file.", null);
					return;
				}
				string str = "The debug log can't be enabled: the game logger has been replaced with unknown implementation '";
				IGameLogger log3 = Game1.log;
				string str2;
				if (log3 == null)
				{
					str2 = null;
				}
				else
				{
					Type type = log3.GetType();
					str2 = ((type != null) ? type.FullName : null);
				}
				log.Error(str + str2 + "'.", null);
			}

			// Token: 0x06003BDD RID: 15325 RVA: 0x002E4622 File Offset: 0x002E2822
			public static void ToggleCheats(string[] command, IGameLogger log)
			{
				Program.enableCheats = !Program.enableCheats;
				log.Info((Program.enableCheats ? "Enabled" : "Disabled") + " in-game cheats.");
			}

			// Token: 0x06003BDE RID: 15326 RVA: 0x002E4654 File Offset: 0x002E2854
			[CompilerGenerated]
			internal static bool <PrintOpenAlInfo>g__TryGetField|180_0(string fieldName, BindingFlags fieldFlags, out FieldInfo destField, ref DebugCommands.DefaultHandlers.<>c__DisplayClass180_0 A_3)
			{
				destField = A_3.oalType.GetField(fieldName, fieldFlags);
				if (destField == null)
				{
					A_3.log.Error("OpenALSoundController does not have field '" + fieldName + "'", null);
					return false;
				}
				return true;
			}
		}
	}
}
