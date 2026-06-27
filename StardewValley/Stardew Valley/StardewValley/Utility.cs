using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Weddings;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley
{
	// Token: 0x0200010D RID: 269
	public class Utility
	{
		// Token: 0x060015DB RID: 5595 RVA: 0x00101196 File Offset: 0x000FF396
		public static Microsoft.Xna.Framework.Rectangle controllerMapSourceRect(Microsoft.Xna.Framework.Rectangle xboxSourceRect)
		{
			return xboxSourceRect;
		}

		// Token: 0x060015DC RID: 5596 RVA: 0x0010119C File Offset: 0x000FF39C
		public static List<Vector2> removeDuplicates(List<Vector2> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = list.Count - 1; j >= 0; j--)
				{
					if (j != i && list[i].Equals(list[j]))
					{
						list.RemoveAt(j);
					}
				}
			}
			return list;
		}

		// Token: 0x060015DD RID: 5597 RVA: 0x001011F4 File Offset: 0x000FF3F4
		public static Utility.HorseWarpRestrictions GetHorseWarpRestrictionsForFarmer(Farmer who)
		{
			Utility.HorseWarpRestrictions restrictions = Utility.HorseWarpRestrictions.None;
			if (who.horseName.Value == null)
			{
				restrictions |= Utility.HorseWarpRestrictions.NoOwnedHorse;
			}
			GameLocation currentLocation = who.currentLocation;
			if (!currentLocation.IsOutdoors)
			{
				restrictions |= Utility.HorseWarpRestrictions.Indoors;
			}
			Point playerTile = who.TilePoint;
			Microsoft.Xna.Framework.Rectangle horse_check_rect = new Microsoft.Xna.Framework.Rectangle(playerTile.X * 64, playerTile.Y * 64, 128, 64);
			if (currentLocation.isCollidingPosition(horse_check_rect, Game1.viewport, true, 0, false, who))
			{
				restrictions |= Utility.HorseWarpRestrictions.NoRoom;
			}
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.mount != null && farmer.mount.getOwner() == who)
				{
					restrictions |= Utility.HorseWarpRestrictions.InUse;
					break;
				}
			}
			return restrictions;
		}

		// Token: 0x060015DE RID: 5598 RVA: 0x001012C0 File Offset: 0x000FF4C0
		public static string GetHorseWarpErrorMessage(Utility.HorseWarpRestrictions issue)
		{
			if (issue.HasFlag(Utility.HorseWarpRestrictions.NoOwnedHorse))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse");
			}
			if (issue.HasFlag(Utility.HorseWarpRestrictions.Indoors))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation");
			}
			if (issue.HasFlag(Utility.HorseWarpRestrictions.NoRoom))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance");
			}
			if (issue.HasFlag(Utility.HorseWarpRestrictions.InUse))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InUse");
			}
			return null;
		}

		// Token: 0x060015DF RID: 5599 RVA: 0x0010135C File Offset: 0x000FF55C
		public static Microsoft.Xna.Framework.Rectangle ConstrainScissorRectToScreen(Microsoft.Xna.Framework.Rectangle scissor_rect)
		{
			if (scissor_rect.Top < 0)
			{
				int amount_to_trim = -scissor_rect.Top;
				scissor_rect.Height -= amount_to_trim;
				scissor_rect.Y += amount_to_trim;
			}
			if (scissor_rect.Bottom > Game1.viewport.Height)
			{
				int amount_to_trim2 = scissor_rect.Bottom - Game1.viewport.Height;
				scissor_rect.Height -= amount_to_trim2;
			}
			if (scissor_rect.Left < 0)
			{
				int amount_to_trim3 = -scissor_rect.Left;
				scissor_rect.Width -= amount_to_trim3;
				scissor_rect.X += amount_to_trim3;
			}
			if (scissor_rect.Right > Game1.viewport.Width)
			{
				int amount_to_trim4 = scissor_rect.Right - Game1.viewport.Width;
				scissor_rect.Width -= amount_to_trim4;
			}
			return scissor_rect;
		}

		// Token: 0x060015E0 RID: 5600 RVA: 0x00101424 File Offset: 0x000FF624
		public static double getRandomDouble(double min, double max, Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			double range = max - min;
			return random.NextDouble() * range + min;
		}

		// Token: 0x060015E1 RID: 5601 RVA: 0x0010144C File Offset: 0x000FF64C
		public static Vector2 getRandom360degreeVector(float speed)
		{
			Vector2 motion = new Vector2(0f, -1f);
			motion = Vector2.Transform(motion, Matrix.CreateRotationZ((float)Utility.getRandomDouble(0.0, 6.283185307179586, null)));
			motion.Normalize();
			motion *= speed;
			return motion;
		}

		// Token: 0x060015E2 RID: 5602 RVA: 0x0010149F File Offset: 0x000FF69F
		public static Point Vector2ToPoint(Vector2 v)
		{
			return new Point((int)v.X, (int)v.Y);
		}

		// Token: 0x060015E3 RID: 5603 RVA: 0x001014B4 File Offset: 0x000FF6B4
		public static Item getRaccoonSeedForCurrentTimeOfYear(Farmer who, Random r, int stackOverride = -1)
		{
			int number = r.Next(2, 4);
			while (r.NextDouble() < 0.1 + who.team.AverageDailyLuck(null))
			{
				number++;
			}
			Item i = null;
			Season season = Game1.season;
			if (Game1.dayOfMonth > ((season == Season.Spring) ? 23 : 20))
			{
				season = (season + 1) % (Season)4;
			}
			switch (season)
			{
			case Season.Spring:
				i = ItemRegistry.Create("(O)CarrotSeeds", 1, 0, false);
				break;
			case Season.Summer:
				i = ItemRegistry.Create("(O)SummerSquashSeeds", 1, 0, false);
				break;
			case Season.Fall:
				i = ItemRegistry.Create("(O)BroccoliSeeds", 1, 0, false);
				break;
			case Season.Winter:
				i = ItemRegistry.Create("(O)PowdermelonSeeds", 1, 0, false);
				break;
			}
			i.Stack = ((stackOverride == -1) ? number : stackOverride);
			return i;
		}

		// Token: 0x060015E4 RID: 5604 RVA: 0x00101571 File Offset: 0x000FF771
		public static Vector2 PointToVector2(Point p)
		{
			return new Vector2((float)p.X, (float)p.Y);
		}

		// Token: 0x060015E5 RID: 5605 RVA: 0x00101588 File Offset: 0x000FF788
		public static int getStartTimeOfFestival()
		{
			if (Game1.weatherIcon == 1)
			{
				return Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth.ToString())["conditions"].Split('/', StringSplitOptions.None)[1], 0, null));
			}
			return -1;
		}

		// Token: 0x060015E6 RID: 5606 RVA: 0x001015E4 File Offset: 0x000FF7E4
		public static bool doesMasterPlayerHaveMailReceivedButNotMailForTomorrow(string mailID)
		{
			return (Game1.MasterPlayer.mailReceived.Contains(mailID) || Game1.MasterPlayer.mailReceived.Contains(mailID + "%&NL&%")) && !Game1.MasterPlayer.mailForTomorrow.Contains(mailID) && !Game1.MasterPlayer.mailForTomorrow.Contains(mailID + "%&NL&%");
		}

		// Token: 0x060015E7 RID: 5607 RVA: 0x00101652 File Offset: 0x000FF852
		public static bool isFestivalDay()
		{
			return Utility.isFestivalDay(Game1.dayOfMonth, Game1.season, null);
		}

		// Token: 0x060015E8 RID: 5608 RVA: 0x00101664 File Offset: 0x000FF864
		public static bool isFestivalDay(string locationContext)
		{
			return Utility.isFestivalDay(Game1.dayOfMonth, Game1.season, locationContext);
		}

		// Token: 0x060015E9 RID: 5609 RVA: 0x00101676 File Offset: 0x000FF876
		public static bool isFestivalDay(int day, Season season)
		{
			return Utility.isFestivalDay(day, season, null);
		}

		// Token: 0x060015EA RID: 5610 RVA: 0x00101680 File Offset: 0x000FF880
		public static bool isFestivalDay(int day, Season season, string locationContext)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted(Utility.getSeasonKey(season));
			defaultInterpolatedStringHandler.AppendFormatted<int>(day);
			string festivalId = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!DataLoader.Festivals_FestivalDates(Game1.temporaryContent).ContainsKey(festivalId))
			{
				return false;
			}
			if (locationContext != null)
			{
				string text;
				Dictionary<string, string> dictionary;
				string locationName;
				int num;
				int num2;
				if (!Event.tryToLoadFestivalData(festivalId, out text, out dictionary, out locationName, out num, out num2))
				{
					return false;
				}
				GameLocation location = Game1.getLocationFromName(locationName);
				if (location == null)
				{
					return false;
				}
				if (location.GetLocationContextId() != locationContext)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060015EB RID: 5611 RVA: 0x001016FC File Offset: 0x000FF8FC
		public static void ForEachLocation(Func<GameLocation, bool> action, bool includeInteriors = true, bool includeGenerated = false)
		{
			GameLocation currentLocation = Game1.currentLocation;
			string currentLocationName = (currentLocation != null) ? currentLocation.NameOrUniqueName : null;
			foreach (GameLocation rawLocation in Game1.locations)
			{
				GameLocation location = (rawLocation.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLocation;
				if (!action(location))
				{
					return;
				}
				if (includeInteriors)
				{
					bool shouldContinue = true;
					location.ForEachInstancedInterior(delegate(GameLocation interior)
					{
						if (action(interior))
						{
							return true;
						}
						shouldContinue = false;
						return false;
					});
					if (!shouldContinue)
					{
						return;
					}
				}
			}
			if (includeGenerated)
			{
				foreach (MineShaft rawLevel in MineShaft.activeMines)
				{
					GameLocation level = (rawLevel.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLevel;
					if (!action(level))
					{
						return;
					}
				}
				foreach (VolcanoDungeon rawLevel2 in VolcanoDungeon.activeLevels)
				{
					GameLocation level2 = (rawLevel2.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLevel2;
					if (!action(level2))
					{
						break;
					}
				}
			}
		}

		// Token: 0x060015EC RID: 5612 RVA: 0x001018A0 File Offset: 0x000FFAA0
		public static void ForEachBuilding(Func<Building, bool> action, bool ignoreUnderConstruction = true)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Building building in location.buildings)
				{
					if ((!ignoreUnderConstruction || !building.isUnderConstruction(true)) && !action(building))
					{
						return false;
					}
				}
				return true;
			}, false, false);
		}

		// Token: 0x060015ED RID: 5613 RVA: 0x001018C8 File Offset: 0x000FFAC8
		public static List<Pet> getAllPets()
		{
			List<Pet> pets = new List<Pet>();
			foreach (NPC npc in Game1.getFarm().characters)
			{
				Pet pet = npc as Pet;
				if (pet != null)
				{
					pets.Add(pet);
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				foreach (NPC npc2 in Utility.getHomeOfFarmer(who).characters)
				{
					Pet pet2 = npc2 as Pet;
					if (pet2 != null)
					{
						pets.Add(pet2);
					}
				}
			}
			return pets;
		}

		// Token: 0x060015EE RID: 5614 RVA: 0x001019B4 File Offset: 0x000FFBB4
		public static void ForEachCharacter(Func<NPC, bool> action, bool includeEventActors = false)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (NPC npc in location.characters)
				{
					if ((includeEventActors || !npc.EventActor) && !action(npc))
					{
						return false;
					}
				}
				return true;
			}, true, true);
		}

		// Token: 0x060015EF RID: 5615 RVA: 0x001019DB File Offset: 0x000FFBDB
		public static void ForEachVillager(Func<NPC, bool> action, bool includeEventActors = false)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (NPC npc in location.characters)
				{
					if ((includeEventActors || !npc.EventActor) && npc.IsVillager && !action(npc))
					{
						return false;
					}
				}
				return true;
			}, true, false);
		}

		// Token: 0x060015F0 RID: 5616 RVA: 0x00101A02 File Offset: 0x000FFC02
		public static void ForEachBuilding<TBuilding>(Func<TBuilding, bool> action, bool ignoreUnderConstruction = true) where TBuilding : Building
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Building building2 in location.buildings)
				{
					TBuilding building = building2 as TBuilding;
					if (building != null && (!ignoreUnderConstruction || !building.isUnderConstruction(true)) && !action(building))
					{
						return false;
					}
				}
				return true;
			}, false, false);
		}

		// Token: 0x060015F1 RID: 5617 RVA: 0x00101A29 File Offset: 0x000FFC29
		public static void ForEachCrop(Func<Crop, bool> action)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (TerrainFeature terrainFeature in location.terrainFeatures.Values)
				{
					HoeDirt hoeDirt = terrainFeature as HoeDirt;
					Crop crop = (hoeDirt != null) ? hoeDirt.crop : null;
					if (crop != null && !action(crop))
					{
						return false;
					}
				}
				foreach (Object @object in location.objects.Values)
				{
					IndoorPot indoorPot = @object as IndoorPot;
					Crop crop3;
					if (indoorPot == null)
					{
						crop3 = null;
					}
					else
					{
						HoeDirt value = indoorPot.hoeDirt.Value;
						crop3 = ((value != null) ? value.crop : null);
					}
					Crop crop2 = crop3;
					if (crop2 != null && !action(crop2))
					{
						return false;
					}
				}
				return true;
			}, true, false);
		}

		// Token: 0x060015F2 RID: 5618 RVA: 0x00101A49 File Offset: 0x000FFC49
		public static bool ForEachItem(Func<Item, bool> action)
		{
			Utility.<>c__DisplayClass24_0 CS$<>8__locals1 = new Utility.<>c__DisplayClass24_0();
			CS$<>8__locals1.action = action;
			return ForEachItemHelper.ForEachItemInWorld(new ForEachItemDelegate(CS$<>8__locals1.<ForEachItem>g__Handle|0));
		}

		// Token: 0x060015F3 RID: 5619 RVA: 0x00101A67 File Offset: 0x000FFC67
		public static bool ForEachItemContext(ForEachItemDelegate handler)
		{
			return ForEachItemHelper.ForEachItemInWorld(handler);
		}

		// Token: 0x060015F4 RID: 5620 RVA: 0x00101A70 File Offset: 0x000FFC70
		public static bool ForEachItemIn(GameLocation location, Func<Item, bool> action)
		{
			Utility.<>c__DisplayClass26_0 CS$<>8__locals1 = new Utility.<>c__DisplayClass26_0();
			CS$<>8__locals1.action = action;
			return ForEachItemHelper.ForEachItemInLocation(location, new ForEachItemDelegate(CS$<>8__locals1.<ForEachItemIn>g__Handle|0));
		}

		// Token: 0x060015F5 RID: 5621 RVA: 0x00101A9C File Offset: 0x000FFC9C
		public static bool ForEachItemContextIn(GameLocation location, ForEachItemDelegate handler)
		{
			return ForEachItemHelper.ForEachItemInLocation(location, handler);
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x00101AA8 File Offset: 0x000FFCA8
		public static int getNumObjectsOfIndexWithinRectangle(Microsoft.Xna.Framework.Rectangle r, string[] indexes, GameLocation location)
		{
			int count = 0;
			Vector2 v = Vector2.Zero;
			for (int y = r.Y; y < r.Bottom + 1; y++)
			{
				v.Y = (float)y;
				for (int x = r.X; x < r.Right + 1; x++)
				{
					v.X = (float)x;
					Object obj;
					if (location.objects.TryGetValue(v, out obj))
					{
						foreach (string itemId in indexes)
						{
							if (itemId == null || ItemRegistry.HasItemId(obj, itemId))
							{
								count++;
								break;
							}
						}
					}
				}
			}
			return count;
		}

		// Token: 0x060015F7 RID: 5623 RVA: 0x00101B40 File Offset: 0x000FFD40
		public static bool TryParseEnum<TEnum>(string value, out TEnum parsed) where TEnum : struct
		{
			if (Enum.TryParse<TEnum>(value, true, out parsed))
			{
				if (typeof(TEnum).IsEnumDefined(parsed))
				{
					return true;
				}
				long num;
				if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null && !long.TryParse(parsed.ToString(), out num))
				{
					return true;
				}
			}
			parsed = default(TEnum);
			return false;
		}

		// Token: 0x060015F8 RID: 5624 RVA: 0x00101BA5 File Offset: 0x000FFDA5
		public static TEnum GetEnumOrDefault<TEnum>(TEnum value, TEnum defaultValue) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnumDefined(value))
			{
				return defaultValue;
			}
			return value;
		}

		// Token: 0x060015F9 RID: 5625 RVA: 0x00101BC4 File Offset: 0x000FFDC4
		public static string TrimLines(string text)
		{
			text = ((text != null) ? text.Trim() : null);
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			string[] lines = LegacyShims.SplitAndTrim(text, '\n', StringSplitOptions.None);
			if (lines.Length <= 1)
			{
				return text;
			}
			return string.Join("\n", lines);
		}

		// Token: 0x060015FA RID: 5626 RVA: 0x00101C08 File Offset: 0x000FFE08
		public static bool IsLegacyIdAbove(string itemId, int lowerBound)
		{
			int legacyId;
			return int.TryParse(itemId, out legacyId) && legacyId > lowerBound;
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x00101C28 File Offset: 0x000FFE28
		public static bool IsLegacyIdBetween(string itemId, int lowerBound, int upperBound)
		{
			int legacyId;
			return int.TryParse(itemId, out legacyId) && legacyId >= lowerBound && legacyId <= upperBound;
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x00101C4C File Offset: 0x000FFE4C
		public static string fuzzySearch(string query, ICollection<string> terms)
		{
			int? bestPriority = null;
			string bestMatch = null;
			foreach (string term in terms)
			{
				int? priority = Utility.fuzzyCompare(query, term);
				if (priority != null)
				{
					if (bestPriority != null)
					{
						int? num = priority;
						int? num2 = bestPriority;
						if (!(num.GetValueOrDefault() < num2.GetValueOrDefault() & (num != null & num2 != null)))
						{
							continue;
						}
					}
					bestPriority = priority;
					bestMatch = term;
				}
			}
			return bestMatch;
		}

		// Token: 0x060015FD RID: 5629 RVA: 0x00101CE4 File Offset: 0x000FFEE4
		public static IEnumerable<string> fuzzySearchAll(string query, ICollection<string> terms, bool sortByScore = true)
		{
			if (!sortByScore)
			{
				return from term in terms
				where Utility.fuzzyCompare(query, term) != null
				orderby term.ToLowerInvariant()
				select term;
			}
			return from term in terms
			let score = Utility.fuzzyCompare(query, term)
			where score != null
			orderby score.Value, term.ToLowerInvariant()
			select term;
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x00101DDC File Offset: 0x000FFFDC
		public static int? fuzzyCompare(string query, string term)
		{
			if (query.Trim() == term.Trim())
			{
				return new int?(0);
			}
			string formattedQuery = Utility.<fuzzyCompare>g__FormatForFuzzySearch|36_0(query);
			string formattedTerm = Utility.<fuzzyCompare>g__FormatForFuzzySearch|36_0(term);
			if (formattedQuery == formattedTerm)
			{
				return new int?(1);
			}
			if (formattedTerm.StartsWith(formattedQuery))
			{
				return new int?(2);
			}
			if (formattedTerm.Contains(formattedQuery))
			{
				return new int?(3);
			}
			return null;
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x00101E4C File Offset: 0x0010004C
		public static Item fuzzyItemSearch(string query, int stack_count = 1, bool useLocalizedNames = false)
		{
			Dictionary<string, string> items = new Dictionary<string, string>();
			foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
			{
				foreach (string itemId in itemType.GetAllIds())
				{
					ParsedItemData itemData = itemType.GetData(itemId);
					string itemName = useLocalizedNames ? itemData.DisplayName : itemData.InternalName;
					if (!items.ContainsKey(itemName))
					{
						items[itemName] = itemType.Identifier + itemId;
					}
				}
			}
			ParsedItemData stoneData = ItemRegistry.GetData("(O)390");
			if (stoneData != null)
			{
				string stoneName = useLocalizedNames ? stoneData.DisplayName : stoneData.InternalName;
				items[stoneName] = "(O)390";
			}
			string result = Utility.fuzzySearch(query, items.Keys);
			if (result != null)
			{
				return ItemRegistry.Create(items[result], stack_count, 0, false);
			}
			return null;
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x00101F6C File Offset: 0x0010016C
		public static GameLocation fuzzyLocationSearch(string query)
		{
			Dictionary<string, GameLocation> name_bank = new Dictionary<string, GameLocation>();
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				name_bank[location.NameOrUniqueName] = location;
				return true;
			}, true, false);
			string location_name = Utility.fuzzySearch(query, name_bank.Keys);
			if (location_name == null)
			{
				return null;
			}
			return name_bank[location_name];
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x00101FC0 File Offset: 0x001001C0
		public static string AOrAn(string text)
		{
			if (text != null && text.Length > 0)
			{
				char letter = text.ToLowerInvariant()[0];
				if (letter == 'a' || letter == 'e' || letter == 'i' || letter == 'o' || letter == 'u')
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
					{
						return "az";
					}
					return "an";
				}
			}
			return "a";
		}

		// Token: 0x06001602 RID: 5634 RVA: 0x0010201C File Offset: 0x0010021C
		public static void getDefaultWarpLocation(string locationName, ref int x, ref int y)
		{
			GameLocation location = Game1.getLocationFromName(locationName);
			Point position;
			if (location != null && location.TryGetMapPropertyAs("DefaultWarpLocation", out position, false))
			{
				x = position.X;
				y = position.Y;
				return;
			}
			Farm farm = location as Farm;
			if (farm != null)
			{
				Point tile = farm.GetMainFarmHouseEntry();
				if (tile != Point.Zero)
				{
					x = tile.X;
					y = tile.Y;
				}
			}
			LocationData data = GameLocation.GetData(locationName);
			Point? arrivalTile = (data != null) ? data.DefaultArrivalTile : null;
			if (arrivalTile != null)
			{
				x = arrivalTile.Value.X;
				y = arrivalTile.Value.Y;
				return;
			}
			if (locationName != null)
			{
				int length = locationName.Length;
				if (length != 4)
				{
					if (length != 5)
					{
						if (length != 10)
						{
							goto IL_1A3;
						}
						if (!(locationName == "SlimeHutch"))
						{
							goto IL_1A3;
						}
						x = 8;
						y = 18;
						return;
					}
					else
					{
						char c = locationName[0];
						if (c != 'B')
						{
							if (c != 'C')
							{
								goto IL_1A3;
							}
							if (!(locationName == "Coop2") && !(locationName == "Coop3"))
							{
								goto IL_1A3;
							}
							goto IL_18B;
						}
						else if (!(locationName == "Barn2") && !(locationName == "Barn3"))
						{
							goto IL_1A3;
						}
					}
				}
				else
				{
					switch (locationName[0])
					{
					case 'B':
						if (!(locationName == "Barn"))
						{
							goto IL_1A3;
						}
						break;
					case 'C':
						if (!(locationName == "Coop"))
						{
							goto IL_1A3;
						}
						goto IL_18B;
					case 'D':
					case 'E':
						goto IL_1A3;
					case 'F':
						if (!(locationName == "Farm"))
						{
							goto IL_1A3;
						}
						x = 64;
						y = 15;
						return;
					default:
						goto IL_1A3;
					}
				}
				x = 11;
				y = 13;
				return;
				IL_18B:
				x = 2;
				y = 8;
				return;
			}
			IL_1A3:
			string warps;
			if (location != null && location.TryGetMapProperty("Warp", out warps))
			{
				string[] warpExtract = warps.Split(' ', StringSplitOptions.None);
				Vector2 vec = new Vector2((float)Convert.ToInt32(warpExtract[0]), (float)Convert.ToInt32(warpExtract[1]));
				Vector2 warpLoc = Utility.recursiveFindOpenTileForCharacter(Game1.player, Game1.getLocationFromName(locationName), vec, 10, false);
				x = (int)warpLoc.X;
				y = (int)warpLoc.Y;
				return;
			}
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x00102234 File Offset: 0x00100434
		public static FarmAnimal fuzzyAnimalSearch(string query)
		{
			List<FarmAnimal> animals = new List<FarmAnimal>();
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				animals.AddRange(location.Animals.Values);
				return true;
			}, true, false);
			Dictionary<string, FarmAnimal> name_bank = new Dictionary<string, FarmAnimal>();
			foreach (FarmAnimal animal in animals)
			{
				name_bank[animal.Name] = animal;
			}
			string character_name = Utility.fuzzySearch(query, name_bank.Keys);
			if (character_name == null)
			{
				return null;
			}
			return name_bank[character_name];
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x001022D0 File Offset: 0x001004D0
		public static NPC fuzzyCharacterSearch(string query, bool must_be_villager = true)
		{
			Dictionary<string, NPC> name_bank = new Dictionary<string, NPC>();
			Utility.ForEachCharacter(delegate(NPC character)
			{
				if (!must_be_villager || character.IsVillager)
				{
					name_bank[character.Name] = character;
				}
				return true;
			}, false);
			string character_name = Utility.fuzzySearch(query, name_bank.Keys);
			if (character_name == null)
			{
				return null;
			}
			return name_bank[character_name];
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x0010232C File Offset: 0x0010052C
		public static Color GetPrismaticColor(int offset = 0, float speedMultiplier = 1f)
		{
			float interval = 1500f;
			int current_index = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval) + offset) % Utility.PRISMATIC_COLORS.Length;
			int next_index = (current_index + 1) % Utility.PRISMATIC_COLORS.Length;
			float position = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval % 1f;
			return new Color
			{
				R = (byte)(Utility.Lerp((float)Utility.PRISMATIC_COLORS[current_index].R / 255f, (float)Utility.PRISMATIC_COLORS[next_index].R / 255f, position) * 255f),
				G = (byte)(Utility.Lerp((float)Utility.PRISMATIC_COLORS[current_index].G / 255f, (float)Utility.PRISMATIC_COLORS[next_index].G / 255f, position) * 255f),
				B = (byte)(Utility.Lerp((float)Utility.PRISMATIC_COLORS[current_index].B / 255f, (float)Utility.PRISMATIC_COLORS[next_index].B / 255f, position) * 255f),
				A = (byte)(Utility.Lerp((float)Utility.PRISMATIC_COLORS[current_index].A / 255f, (float)Utility.PRISMATIC_COLORS[next_index].A / 255f, position) * 255f)
			};
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x001024A0 File Offset: 0x001006A0
		public static Color Get2PhaseColor(Color color1, Color color2, int offset = 0, float speedMultiplier = 1f, float timeOffset = 0f)
		{
			float interval = 1500f;
			int num = ((int)((float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval) + offset) % 2;
			float position = (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval % 1f;
			Color prismatic_color = default(Color);
			Color a = (num == 0) ? color1 : color2;
			Color b = (num == 0) ? color2 : color1;
			prismatic_color.R = (byte)(Utility.Lerp((float)a.R / 255f, (float)b.R / 255f, position) * 255f);
			prismatic_color.G = (byte)(Utility.Lerp((float)a.G / 255f, (float)b.G / 255f, position) * 255f);
			prismatic_color.B = (byte)(Utility.Lerp((float)a.B / 255f, (float)b.B / 255f, position) * 255f);
			prismatic_color.A = (byte)(Utility.Lerp((float)a.A / 255f, (float)b.A / 255f, position) * 255f);
			return prismatic_color;
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x001025D0 File Offset: 0x001007D0
		public static bool IsNormalObjectAtParentSheetIndex(Item item, string itemId)
		{
			return item.HasTypeObject() && item.GetType() == typeof(Object) && item.ItemId == itemId;
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x00102600 File Offset: 0x00100800
		public static Microsoft.Xna.Framework.Rectangle getSafeArea()
		{
			Microsoft.Xna.Framework.Rectangle area = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
			if (Game1.game1.GraphicsDevice.GetRenderTargets().Length == 0)
			{
				float oneOverZoomLevel = 1f / Game1.options.zoomLevel;
				if (Game1.uiMode)
				{
					oneOverZoomLevel = 1f / Game1.options.uiScale;
				}
				area.X = (int)((float)area.X * oneOverZoomLevel);
				area.Y = (int)((float)area.Y * oneOverZoomLevel);
				area.Width = (int)((float)area.Width * oneOverZoomLevel);
				area.Height = (int)((float)area.Height * oneOverZoomLevel);
			}
			return area;
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x001026A4 File Offset: 0x001008A4
		public static Vector2 makeSafe(Vector2 renderPos, Vector2 renderSize)
		{
			int x = (int)renderPos.X;
			int y = (int)renderPos.Y;
			int w = (int)renderSize.X;
			int h = (int)renderSize.Y;
			Utility.makeSafe(ref x, ref y, w, h);
			return new Vector2((float)x, (float)y);
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x001026E8 File Offset: 0x001008E8
		public static void makeSafe(ref Vector2 position, int width, int height)
		{
			int x = (int)position.X;
			int y = (int)position.Y;
			Utility.makeSafe(ref x, ref y, width, height);
			position.X = (float)x;
			position.Y = (float)y;
		}

		// Token: 0x0600160B RID: 5643 RVA: 0x00102720 File Offset: 0x00100920
		public static void makeSafe(ref Microsoft.Xna.Framework.Rectangle bounds)
		{
			Utility.makeSafe(ref bounds.X, ref bounds.Y, bounds.Width, bounds.Height);
		}

		// Token: 0x0600160C RID: 5644 RVA: 0x00102740 File Offset: 0x00100940
		public static void makeSafe(ref int x, ref int y, int width, int height)
		{
			Microsoft.Xna.Framework.Rectangle area = Utility.getSafeArea();
			if (x < area.Left)
			{
				x = area.Left;
			}
			if (y < area.Top)
			{
				y = area.Top;
			}
			if (x + width > area.Right)
			{
				x = area.Right - width;
			}
			if (y + height > area.Bottom)
			{
				y = area.Bottom - height;
			}
		}

		// Token: 0x0600160D RID: 5645 RVA: 0x001027AC File Offset: 0x001009AC
		public static int makeSafeMarginY(int marginy)
		{
			Viewport vp = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle area = Utility.getSafeArea();
			int i = area.Top - vp.Bounds.Top;
			if (i > marginy)
			{
				marginy = i;
			}
			i = vp.Bounds.Bottom - area.Bottom;
			if (i > marginy)
			{
				marginy = i;
			}
			return marginy;
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x00102810 File Offset: 0x00100A10
		public static int CompareGameVersions(string version, string other_version, bool ignore_platform_specific = false)
		{
			string[] split = version.Split('.', StringSplitOptions.None);
			string[] other_split = other_version.Split('.', StringSplitOptions.None);
			for (int i = 0; i < Math.Max(split.Length, other_split.Length); i++)
			{
				float version_number = 0f;
				float other_version_number = 0f;
				if (i < split.Length)
				{
					float.TryParse(split[i], out version_number);
				}
				if (i < other_split.Length)
				{
					float.TryParse(other_split[i], out other_version_number);
				}
				if (version_number != other_version_number || (i == 2 && ignore_platform_specific))
				{
					return version_number.CompareTo(other_version_number);
				}
			}
			return 0;
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x00102890 File Offset: 0x00100A90
		public static float getFarmerItemsShippedPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Utility.recentlyDiscoveredMissingBasicShippedItem = null;
			int farmerShipped = 0;
			int total = 0;
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				int category = data.Category;
				if (category != -7 && category != -2 && Object.isPotentialBasicShipped(data.ItemId, data.Category, data.ObjectType))
				{
					total++;
					if (who.basicShipped.ContainsKey(data.ItemId))
					{
						farmerShipped++;
					}
					else if (Utility.recentlyDiscoveredMissingBasicShippedItem == null)
					{
						Utility.recentlyDiscoveredMissingBasicShippedItem = ItemRegistry.Create(data.QualifiedItemId, 1, 0, false);
					}
				}
			}
			return (float)farmerShipped / (float)total;
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x00102958 File Offset: 0x00100B58
		public static bool hasFarmerShippedAllItems()
		{
			return Utility.getFarmerItemsShippedPercent(null) >= 1f;
		}

		// Token: 0x06001611 RID: 5649 RVA: 0x0010296A File Offset: 0x00100B6A
		public static NPC getTodaysBirthdayNPC()
		{
			NPC match = null;
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (n.isBirthday())
				{
					match = n;
				}
				return match == null;
			}, false);
			return match;
		}

		// Token: 0x06001612 RID: 5650 RVA: 0x0010298F File Offset: 0x00100B8F
		public static Random CreateDaySaveRandom(double seedA = 0.0, double seedB = 0.0, double seedC = 0.0)
		{
			return Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2UL, seedA, seedB, seedC);
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x001029B0 File Offset: 0x00100BB0
		public static bool TryCreateIntervalRandom(string interval, string key, out Random random, out string error)
		{
			int seed = (key != null) ? Game1.hash.GetDeterministicHashCode(key) : 0;
			error = null;
			string a = interval.ToLower();
			double intervalSeed;
			if (!(a == "tick"))
			{
				if (!(a == "day"))
				{
					if (!(a == "season"))
					{
						if (!(a == "year"))
						{
							error = "invalid interval '" + interval + "'; expected one of 'tick', 'day', 'season', or 'year'";
							random = null;
							return false;
						}
						intervalSeed = (double)Game1.hash.GetDeterministicHashCode("year" + Game1.year.ToString());
					}
					else
					{
						intervalSeed = (double)Game1.hash.GetDeterministicHashCode(Game1.currentSeason + Game1.year.ToString());
					}
				}
				else
				{
					intervalSeed = Game1.stats.DaysPlayed;
				}
			}
			else
			{
				intervalSeed = (double)Game1.ticks;
			}
			random = Utility.CreateRandom((double)seed, Game1.uniqueIDForThisGame, intervalSeed, 0.0, 0.0);
			return true;
		}

		// Token: 0x06001614 RID: 5652 RVA: 0x00102AA6 File Offset: 0x00100CA6
		public static Random CreateRandom(double seedA, double seedB = 0.0, double seedC = 0.0, double seedD = 0.0, double seedE = 0.0)
		{
			return new Random(Utility.CreateRandomSeed(seedA, seedB, seedC, seedD, seedE));
		}

		// Token: 0x06001615 RID: 5653 RVA: 0x00102AB8 File Offset: 0x00100CB8
		public static int CreateRandomSeed(double seedA, double seedB, double seedC = 0.0, double seedD = 0.0, double seedE = 0.0)
		{
			if (Game1.UseLegacyRandom)
			{
				return (int)((seedA % 2147483647.0 + seedB % 2147483647.0 + seedC % 2147483647.0 + seedD % 2147483647.0 + seedE % 2147483647.0) % 2147483647.0);
			}
			return Game1.hash.GetDeterministicHashCode(new int[]
			{
				(int)(seedA % 2147483647.0),
				(int)(seedB % 2147483647.0),
				(int)(seedC % 2147483647.0),
				(int)(seedD % 2147483647.0),
				(int)(seedE % 2147483647.0)
			});
		}

		// Token: 0x06001616 RID: 5654 RVA: 0x00102B70 File Offset: 0x00100D70
		public static bool TryGetRandom<TKey, TValue>(IDictionary<TKey, TValue> dictionary, out TKey key, out TValue value, Random random = null)
		{
			if (dictionary == null || dictionary.Count == 0)
			{
				key = default(TKey);
				value = default(TValue);
				return false;
			}
			if (random == null)
			{
				random = Game1.random;
			}
			KeyValuePair<TKey, TValue> pair = dictionary.ElementAt(random.Next(dictionary.Count));
			key = pair.Key;
			value = pair.Value;
			return true;
		}

		// Token: 0x06001617 RID: 5655 RVA: 0x00102BD0 File Offset: 0x00100DD0
		public static bool TryGetRandom<TKey, TValue, TField, TSerialDict, TSelf>(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> dictionary, out TKey key, out TValue value, Random random = null) where TField : class, INetObject<INetSerializable>, new() where TSerialDict : IDictionary<!!0, !!1>, new() where TSelf : NetDictionary<!!0, !!1, !!2, !!3, !!4>
		{
			if (dictionary == null || dictionary.Length == 0)
			{
				key = default(TKey);
				value = default(TValue);
				return false;
			}
			if (random == null)
			{
				random = Game1.random;
			}
			KeyValuePair<TKey, TValue> pair = dictionary.Pairs.ElementAt(random.Next(dictionary.Length));
			key = pair.Key;
			value = pair.Value;
			return true;
		}

		// Token: 0x06001618 RID: 5656 RVA: 0x00102C38 File Offset: 0x00100E38
		public static bool TryGetRandom(OverlaidDictionary dictionary, out Vector2 key, out Object value, Random random = null)
		{
			if (dictionary == null || dictionary.Length == 0)
			{
				key = Vector2.Zero;
				value = null;
				return false;
			}
			if (random == null)
			{
				random = Game1.random;
			}
			KeyValuePair<Vector2, Object> pair = dictionary.Pairs.ElementAt(random.Next(dictionary.Length));
			key = pair.Key;
			value = pair.Value;
			return true;
		}

		// Token: 0x06001619 RID: 5657 RVA: 0x00102C9C File Offset: 0x00100E9C
		public static bool TryGetRandomExcept<T>(IList<T> list, ISet<T> except, Random random, out T selected)
		{
			if (list == null || list.Count == 0)
			{
				selected = default(T);
				return false;
			}
			if (except == null || except.Count == 0)
			{
				selected = random.ChooseFrom(list);
				return true;
			}
			T[] filtered = list.Except(except).ToArray<T>();
			selected = random.ChooseFrom(filtered);
			return true;
		}

		// Token: 0x0600161A RID: 5658 RVA: 0x00102CF4 File Offset: 0x00100EF4
		public static string getRandomSingleTileFurniture(Random r)
		{
			int num = r.Next(3);
			if (num == 0)
			{
				return "(F)" + (r.Next(10) * 3).ToString();
			}
			if (num != 1)
			{
				return "(F)" + (r.Next(6) * 2 + 1391).ToString();
			}
			return "(F)" + r.Next(1376, 1391).ToString();
		}

		// Token: 0x0600161B RID: 5659 RVA: 0x00102D73 File Offset: 0x00100F73
		public static void improveFriendshipWithEveryoneInRegion(Farmer who, int amount, string region)
		{
			Utility.ForEachLocation(delegate(GameLocation l)
			{
				foreach (NPC i in l.characters)
				{
					CharacterData data = i.GetData();
					if (((data != null) ? data.HomeRegion : null) == region && who.friendshipData.ContainsKey(i.Name))
					{
						who.changeFriendship(amount, i);
					}
				}
				return true;
			}, true, false);
		}

		// Token: 0x0600161C RID: 5660 RVA: 0x00102DA4 File Offset: 0x00100FA4
		public static Item getGiftFromNPC(NPC who)
		{
			Random giftRandom = Utility.CreateRandom(Game1.uniqueIDForThisGame / 2UL, (double)Game1.year, (double)Game1.dayOfMonth, (double)Game1.seasonIndex, (double)who.TilePoint.X);
			List<Item> gifts = new List<Item>();
			CharacterData data = who.GetData();
			List<GenericSpawnItemDataWithCondition> winterStarGifts = data.WinterStarGifts;
			if (winterStarGifts != null && winterStarGifts.Count > 0)
			{
				ItemQueryContext itemQueryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, giftRandom, "character '" + who.Name + "' > winter star gifts");
				using (List<GenericSpawnItemDataWithCondition>.Enumerator enumerator = data.WinterStarGifts.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GenericSpawnItemDataWithCondition entry = enumerator.Current;
						if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, giftRandom, null))
						{
							Item result = ItemQueryResolver.TryResolveRandomItem(entry, itemQueryContext, false, null, null, null, delegate(string query, string error)
							{
								IGameLogger log = Game1.log;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 4);
								defaultInterpolatedStringHandler.AppendFormatted(who.Name);
								defaultInterpolatedStringHandler.AppendLiteral(" failed parsing item query '");
								defaultInterpolatedStringHandler.AppendFormatted(query);
								defaultInterpolatedStringHandler.AppendLiteral("' for winter star gift entry '");
								defaultInterpolatedStringHandler.AppendFormatted(entry.Id);
								defaultInterpolatedStringHandler.AppendLiteral("': ");
								defaultInterpolatedStringHandler.AppendFormatted(error);
								log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							});
							if (result != null)
							{
								gifts.Add(result);
							}
						}
					}
				}
			}
			if (gifts.Count == 0)
			{
				if (who.Age == 2)
				{
					gifts.AddRange(new Item[]
					{
						ItemRegistry.Create("(O)330", 1, 0, false),
						ItemRegistry.Create("(O)103", 1, 0, false),
						ItemRegistry.Create("(O)394", 1, 0, false),
						ItemRegistry.Create("(O)" + giftRandom.Next(535, 538).ToString(), 1, 0, false)
					});
				}
				else
				{
					gifts.AddRange(new Item[]
					{
						ItemRegistry.Create("(O)608", 1, 0, false),
						ItemRegistry.Create("(O)651", 1, 0, false),
						ItemRegistry.Create("(O)611", 1, 0, false),
						ItemRegistry.Create("(O)517", 1, 0, false),
						ItemRegistry.Create("(O)466", 10, 0, false),
						ItemRegistry.Create("(O)422", 1, 0, false),
						ItemRegistry.Create("(O)392", 1, 0, false),
						ItemRegistry.Create("(O)348", 1, 0, false),
						ItemRegistry.Create("(O)346", 1, 0, false),
						ItemRegistry.Create("(O)341", 1, 0, false),
						ItemRegistry.Create("(O)221", 1, 0, false),
						ItemRegistry.Create("(O)64", 1, 0, false),
						ItemRegistry.Create("(O)60", 1, 0, false),
						ItemRegistry.Create("(O)70", 1, 0, false)
					});
				}
			}
			return giftRandom.ChooseFrom(gifts);
		}

		// Token: 0x0600161D RID: 5661 RVA: 0x00103068 File Offset: 0x00101268
		public static NPC getTopRomanticInterest(Farmer who)
		{
			NPC topSpot = null;
			int highestFriendPoints = -1;
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (who.friendshipData.ContainsKey(n.Name) && n.datable.Value && who.getFriendshipLevelForNPC(n.Name) > highestFriendPoints)
				{
					topSpot = n;
					highestFriendPoints = who.getFriendshipLevelForNPC(n.Name);
				}
				return true;
			}, false);
			return topSpot;
		}

		// Token: 0x0600161E RID: 5662 RVA: 0x0010309C File Offset: 0x0010129C
		public static Color getRandomRainbowColor(Random r = null)
		{
			switch ((r == null) ? Game1.random.Next(8) : r.Next(8))
			{
			case 0:
				return Color.Red;
			case 1:
				return Color.Orange;
			case 2:
				return Color.Yellow;
			case 3:
				return Color.Lime;
			case 4:
				return Color.Cyan;
			case 5:
				return new Color(0, 100, 255);
			case 6:
				return new Color(152, 96, 255);
			case 7:
				return new Color(255, 100, 255);
			default:
				return Color.White;
			}
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x0010313E File Offset: 0x0010133E
		public static NPC getTopNonRomanticInterest(Farmer who)
		{
			NPC topSpot = null;
			int highestFriendPoints = -1;
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (who.friendshipData.ContainsKey(n.Name) && !n.datable.Value && who.getFriendshipLevelForNPC(n.Name) > highestFriendPoints)
				{
					topSpot = n;
					highestFriendPoints = who.getFriendshipLevelForNPC(n.Name);
				}
				return true;
			}, false);
			return topSpot;
		}

		// Token: 0x06001620 RID: 5664 RVA: 0x00103174 File Offset: 0x00101374
		public static int getHighestSkill(Farmer who)
		{
			int topSkillExperience = 0;
			int topSkill = 0;
			for (int i = 0; i < who.experiencePoints.Length; i++)
			{
				int experiencePoints = who.experiencePoints[i];
				if (who.experiencePoints[i] > topSkillExperience)
				{
					topSkillExperience = experiencePoints;
					topSkill = i;
				}
			}
			return topSkill;
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x001031BC File Offset: 0x001013BC
		public static int getNumberOfFriendsWithinThisRange(Farmer who, int minFriendshipPoints, int maxFriendshipPoints, bool romanceOnly = false)
		{
			int number = 0;
			Utility.ForEachVillager(delegate(NPC n)
			{
				int? level = who.tryGetFriendshipLevelForNPC(n.Name);
				int? num = level;
				int num2 = minFriendshipPoints;
				if ((num.GetValueOrDefault() >= num2 & num != null) && level.Value <= maxFriendshipPoints && (!romanceOnly || n.datable.Value))
				{
					num2 = number;
					number = num2 + 1;
				}
				return true;
			}, false);
			return number;
		}

		// Token: 0x06001622 RID: 5666 RVA: 0x00103208 File Offset: 0x00101408
		public static bool highlightLuauSoupItems(Item i)
		{
			Object obj = i as Object;
			return obj != null && ((obj.edibility.Value != -300 && obj.Category != -7) || obj.QualifiedItemId == "(O)789" || obj.QualifiedItemId == "(O)71");
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x00103264 File Offset: 0x00101464
		public static bool highlightSmallObjects(Item i)
		{
			Object obj = i as Object;
			return obj != null && !obj.bigCraftable.Value;
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x0010328B File Offset: 0x0010148B
		public static bool highlightSantaObjects(Item i)
		{
			return i.canBeTrashed() && i.canBeGivenAsGift() && Utility.highlightSmallObjects(i);
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x001032A8 File Offset: 0x001014A8
		public static bool highlightShippableObjects(Item i)
		{
			return ((i != null) ? new bool?(i.canBeShipped()) : null) ?? false;
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x001032E4 File Offset: 0x001014E4
		public static int getFarmerNumberFromFarmer(Farmer who)
		{
			if (who != null)
			{
				if (who.IsMainPlayer)
				{
					return 1;
				}
				int farmerNumber = 2;
				using (IEnumerator<Farmer> enumerator = (from f in Game1.otherFarmers.Values
				orderby f.UniqueMultiplayerID
				where !f.IsMainPlayer
				select f).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.UniqueMultiplayerID == who.UniqueMultiplayerID)
						{
							return farmerNumber;
						}
						farmerNumber++;
					}
				}
				return -1;
			}
			return -1;
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x001033A0 File Offset: 0x001015A0
		public static Farmer getFarmerFromFarmerNumber(int number)
		{
			if (number <= 1)
			{
				return Game1.MasterPlayer;
			}
			int curNumber = 2;
			foreach (Farmer player in from f in Game1.otherFarmers.Values
			orderby f.UniqueMultiplayerID
			where !f.IsMainPlayer
			select f)
			{
				if (curNumber == number)
				{
					return player;
				}
				curNumber++;
			}
			return null;
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x00103450 File Offset: 0x00101650
		public static string getLoveInterest(string who)
		{
			if (who != null)
			{
				switch (who.Length)
				{
				case 3:
					if (who == "Sam")
					{
						return "Penny";
					}
					break;
				case 4:
				{
					char c = who[0];
					if (c != 'A')
					{
						if (c != 'L')
						{
							if (c == 'M')
							{
								if (who == "Maru")
								{
									return "Harvey";
								}
							}
						}
						else if (who == "Leah")
						{
							return "Elliott";
						}
					}
					else if (who == "Alex")
					{
						return "Haley";
					}
					break;
				}
				case 5:
				{
					char c = who[0];
					if (c <= 'H')
					{
						if (c != 'E')
						{
							if (c == 'H')
							{
								if (who == "Haley")
								{
									return "Alex";
								}
							}
						}
						else if (who == "Emily")
						{
							return "Shane";
						}
					}
					else if (c != 'P')
					{
						if (c == 'S')
						{
							if (who == "Shane")
							{
								return "Emily";
							}
						}
					}
					else if (who == "Penny")
					{
						return "Sam";
					}
					break;
				}
				case 6:
					if (who == "Harvey")
					{
						return "Maru";
					}
					break;
				case 7:
				{
					char c = who[0];
					if (c != 'A')
					{
						if (c == 'E')
						{
							if (who == "Elliott")
							{
								return "Leah";
							}
						}
					}
					else if (who == "Abigail")
					{
						return "Sebastian";
					}
					break;
				}
				case 9:
					if (who == "Sebastian")
					{
						return "Abigail";
					}
					break;
				}
			}
			return "";
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x0010362C File Offset: 0x0010182C
		public static string ParseGiftReveals(string str)
		{
			string original = str;
			try
			{
				for (;;)
				{
					int reveal_taste_location = str.IndexOf("%revealtaste");
					if (reveal_taste_location < 0)
					{
						break;
					}
					int tokenEnd = reveal_taste_location + "%revealtaste".Length;
					for (int i = tokenEnd; i < str.Length; i++)
					{
						char ch = str[i];
						if (char.IsWhiteSpace(ch) || ch == '#' || ch == '%' || ch == '$' || ch == '{' || ch == '^' || ch == '*')
						{
							break;
						}
						tokenEnd = i;
					}
					string match = str.Substring(reveal_taste_location, tokenEnd - reveal_taste_location + 1);
					string[] parts = match.Split(':', StringSplitOptions.None);
					if (parts.Length == 3 && parts[0] == "%revealtaste")
					{
						string npcName = parts[1].Trim();
						NPC npc = Game1.getCharacterFromName(npcName, true, false);
						ItemMetadata itemData = ItemRegistry.GetMetadata(parts[2].Trim());
						if (itemData == null)
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to parse gift taste reveal '");
							defaultInterpolatedStringHandler.AppendFormatted(match);
							defaultInterpolatedStringHandler.AppendLiteral("' in dialogue '");
							defaultInterpolatedStringHandler.AppendFormatted(str);
							defaultInterpolatedStringHandler.AppendLiteral("'. There is no item with that ID.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						else
						{
							Game1.player.revealGiftTaste(((npc != null) ? npc.Name : null) ?? npcName, itemData.LocalItemId);
						}
						str = str.Remove(reveal_taste_location, match.Length);
					}
					else
					{
						int token_start = reveal_taste_location + "%revealtaste".Length;
						int token_end = reveal_taste_location + 1;
						if (token_end >= str.Length)
						{
							token_end = str.Length - 1;
						}
						while (token_end < str.Length && (str[token_end] < '0' || str[token_end] > '9'))
						{
							token_end++;
						}
						string character_name = str.Substring(token_start, token_end - token_start);
						token_start = token_end;
						while (token_end < str.Length && str[token_end] >= '0' && str[token_end] <= '9')
						{
							token_end++;
						}
						string itemId = str.Substring(token_start, token_end - token_start);
						str = str.Remove(reveal_taste_location, token_end - reveal_taste_location);
						NPC target = Game1.getCharacterFromName(character_name, true, false);
						Game1.player.revealGiftTaste(((target != null) ? target.Name : null) ?? character_name, itemId);
					}
				}
			}
			catch (Exception e)
			{
				Game1.log.Error("Error parsing gift taste reveals in string '" + original + "'.", e);
			}
			return str;
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x001038A0 File Offset: 0x00101AA0
		public static void Shuffle<T>(Random rng, List<T> list)
		{
			int i = list.Count;
			while (i > 1)
			{
				int j = rng.Next(i--);
				T temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x001038E4 File Offset: 0x00101AE4
		public static void Shuffle<T>(Random rng, T[] array)
		{
			int i = array.Length;
			while (i > 1)
			{
				int j = rng.Next(i--);
				T temp = array[i];
				array[i] = array[j];
				array[j] = temp;
			}
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x00103928 File Offset: 0x00101B28
		public static string getSeasonKey(Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return "spring";
			case Season.Summer:
				return "summer";
			case Season.Fall:
				return "fall";
			case Season.Winter:
				return "winter";
			default:
				return season.ToString().ToLower();
			}
		}

		// Token: 0x0600162D RID: 5677 RVA: 0x00103978 File Offset: 0x00101B78
		public static int getSeasonNumber(string whichSeason)
		{
			Season season;
			if (Utility.TryParseEnum<Season>(whichSeason, out season))
			{
				return (int)season;
			}
			if (whichSeason.EqualsIgnoreCase("autumn"))
			{
				return 2;
			}
			return -1;
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x001039A4 File Offset: 0x00101BA4
		public static List<Vector2> getPositionsInClusterAroundThisTile(Vector2 startTile, int number)
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			List<Vector2> tiles = new List<Vector2>();
			openList.Enqueue(startTile);
			while (tiles.Count < number)
			{
				Vector2 currentTile = openList.Dequeue();
				tiles.Add(currentTile);
				if (!tiles.Contains(new Vector2(currentTile.X + 1f, currentTile.Y)))
				{
					openList.Enqueue(new Vector2(currentTile.X + 1f, currentTile.Y));
				}
				if (!tiles.Contains(new Vector2(currentTile.X - 1f, currentTile.Y)))
				{
					openList.Enqueue(new Vector2(currentTile.X - 1f, currentTile.Y));
				}
				if (!tiles.Contains(new Vector2(currentTile.X, currentTile.Y + 1f)))
				{
					openList.Enqueue(new Vector2(currentTile.X, currentTile.Y + 1f));
				}
				if (!tiles.Contains(new Vector2(currentTile.X, currentTile.Y - 1f)))
				{
					openList.Enqueue(new Vector2(currentTile.X, currentTile.Y - 1f));
				}
			}
			return tiles;
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x00103AD8 File Offset: 0x00101CD8
		public static bool doesPointHaveLineOfSightInMine(GameLocation mine, Vector2 start, Vector2 end, int visionDistance)
		{
			if (Vector2.Distance(start, end) > (float)visionDistance)
			{
				return false;
			}
			foreach (Point p in Utility.GetPointsOnLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y))
			{
				if (mine.hasTileAt(p, "Buildings", null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x00103B5C File Offset: 0x00101D5C
		public static void addSprinklesToLocation(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random r = Game1.random;
			int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
			for (int i = 0; i < numSprinkles; i++)
			{
				Vector2 currentSprinklePosition = Utility.getRandomPositionInThisRectangle(area, r) * 64f;
				l.temporarySprites.Add(new TemporaryAnimatedSprite(r.Next(10, 12), currentSprinklePosition, sprinkleColor, 8, false, 50f, 0, -1, -1f, -1, 0)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					interval = 100f,
					startSound = sound,
					motion = (motionTowardCenter ? Utility.getVelocityTowardPoint(currentSprinklePosition, new Vector2((float)sourceXTile, (float)sourceYTile) * 64f, Vector2.Distance(new Vector2((float)sourceXTile, (float)sourceYTile) * 64f, currentSprinklePosition) / 64f) : Vector2.Zero),
					xStopCoordinate = sourceXTile,
					yStopCoordinate = sourceYTile
				});
			}
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x00103C60 File Offset: 0x00101E60
		public static void addRainbowStarExplosion(GameLocation l, Vector2 origin, int numStars)
		{
			List<TemporaryAnimatedSprite> sprites = new List<TemporaryAnimatedSprite>();
			float radialStep = 6.2831855f / (float)Math.Max(1, numStars - 1);
			Vector2 radPosition = new Vector2(0f, -4f);
			double r = Game1.random.NextDouble() * 3.141592653589793 * 2.0;
			for (int i = 0; i < numStars; i++)
			{
				sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 640, 64, 64), origin + radPosition, false, 0.03f, Utility.GetPrismaticColor(Game1.random.Next(99999), 1f))
				{
					motion = Utility.getVectorDirection(origin, origin + radPosition, true) * 0.06f * 150f,
					acceleration = -Utility.getVectorDirection(origin, origin + radPosition, true) * 0.06f * 6f,
					totalNumberOfLoops = 1,
					animationLength = 8,
					interval = 50f,
					drawAboveAlwaysFront = true,
					rotation = -1.5707964f - radialStep * (float)i
				});
				radPosition.X = 4f * (float)Math.Sin((double)(radialStep * (float)(i + 1)) + r);
				radPosition.Y = 4f * (float)Math.Cos((double)(radialStep * (float)(i + 1)) + r);
			}
			sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), origin + radPosition, false, 0.03f, Color.White)
			{
				totalNumberOfLoops = 1,
				animationLength = 8,
				interval = 60f,
				drawAboveAlwaysFront = true
			});
			l.temporarySprites.AddRange(sprites);
		}

		// Token: 0x06001632 RID: 5682 RVA: 0x00103E2C File Offset: 0x0010202C
		public static Vector2 getVectorDirection(Vector2 start, Vector2 finish, bool normalize = false)
		{
			Vector2 v = new Vector2(finish.X - start.X, finish.Y - start.Y);
			if (normalize)
			{
				v.Normalize();
			}
			return v;
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x00103E68 File Offset: 0x00102068
		public static TemporaryAnimatedSpriteList getStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random r = Utility.CreateRandom((double)(sourceXTile * 7), (double)(sourceYTile * 77), Game1.currentGameTime.TotalGameTime.TotalSeconds, 0.0, 0.0);
			int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
			TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();
			for (int i = 0; i < numSprinkles; i++)
			{
				Vector2 currentSprinklePosition = Utility.getRandomPositionInThisRectangle(area, r) * 64f;
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", r.NextBool() ? new Microsoft.Xna.Framework.Rectangle(359, 1437, 14, 14) : new Microsoft.Xna.Framework.Rectangle(377, 1438, 9, 9), currentSprinklePosition, false, 0.01f, sprinkleColor)
				{
					xPeriodic = true,
					xPeriodicLoopTime = (float)r.Next(2000, 3000),
					xPeriodicRange = (float)r.Next(-64, 64),
					motion = new Vector2(0f, -2f),
					rotationChange = 3.1415927f / (float)r.Next(4, 64),
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					layerDepth = 1f,
					scaleChange = 0.04f,
					scaleChangeChange = -0.0008f,
					scale = 4f
				});
			}
			return tempSprites;
		}

		// Token: 0x06001634 RID: 5684 RVA: 0x00103FD0 File Offset: 0x001021D0
		public static void addStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			l.temporarySprites.AddRange(Utility.getStarsAndSpirals(l, sourceXTile, sourceYTile, tilesWide, tilesHigh, totalSprinkleDuration, millisecondsBetweenSprinkles, sprinkleColor, sound, motionTowardCenter));
		}

		// Token: 0x06001635 RID: 5685 RVA: 0x00103FFD File Offset: 0x001021FD
		public static Vector2 snapDrawPosition(Vector2 draw_position)
		{
			return new Vector2((float)((int)draw_position.X), (float)((int)draw_position.Y));
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x00104014 File Offset: 0x00102214
		public static Vector2 clampToTile(Vector2 nonTileLocation)
		{
			nonTileLocation.X -= nonTileLocation.X % 64f;
			nonTileLocation.Y -= nonTileLocation.Y % 64f;
			return nonTileLocation;
		}

		// Token: 0x06001637 RID: 5687 RVA: 0x00104045 File Offset: 0x00102245
		public static float distance(float x1, float x2, float y1, float y2)
		{
			return (float)Math.Sqrt((double)((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x00104060 File Offset: 0x00102260
		public static bool couldSeePlayerInPeripheralVision(Farmer player, Character c)
		{
			Point playerPixel = player.StandingPixel;
			Point targetPixel = c.StandingPixel;
			switch (c.FacingDirection)
			{
			case 0:
				if (playerPixel.Y < targetPixel.Y + 32)
				{
					return true;
				}
				break;
			case 1:
				if (playerPixel.X > targetPixel.X - 32)
				{
					return true;
				}
				break;
			case 2:
				if (playerPixel.Y > targetPixel.Y - 32)
				{
					return true;
				}
				break;
			case 3:
				if (playerPixel.X < targetPixel.X + 32)
				{
					return true;
				}
				break;
			}
			return false;
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x001040E7 File Offset: 0x001022E7
		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
		{
			return Utility.GetPointsOnLine(x0, y0, x1, y1, false);
		}

		// Token: 0x0600163A RID: 5690 RVA: 0x001040F4 File Offset: 0x001022F4
		public static List<Vector2> getBorderOfThisRectangle(Microsoft.Xna.Framework.Rectangle r)
		{
			List<Vector2> border = new List<Vector2>();
			for (int i = r.X; i < r.Right; i++)
			{
				border.Add(new Vector2((float)i, (float)r.Y));
			}
			for (int j = r.Y + 1; j < r.Bottom; j++)
			{
				border.Add(new Vector2((float)(r.Right - 1), (float)j));
			}
			for (int k = r.Right - 2; k >= r.X; k--)
			{
				border.Add(new Vector2((float)k, (float)(r.Bottom - 1)));
			}
			for (int l = r.Bottom - 2; l >= r.Y + 1; l--)
			{
				border.Add(new Vector2((float)r.X, (float)l));
			}
			return border;
		}

		// Token: 0x0600163B RID: 5691 RVA: 0x001041C8 File Offset: 0x001023C8
		public static Monster findClosestMonsterWithinRange(GameLocation location, Vector2 originPoint, int range, bool ignoreUntargetables = false, Func<Monster, bool> match = null)
		{
			Monster closestMonster = null;
			float closestDistance = (float)(range + 1);
			foreach (NPC npc in location.characters)
			{
				Monster monster = npc as Monster;
				if (monster != null && (!ignoreUntargetables || !(npc is Spiker)) && (match == null || match(monster)))
				{
					float distance = Vector2.Distance(originPoint, npc.getStandingPosition());
					if (distance <= (float)range && distance < closestDistance && !monster.IsInvisible)
					{
						closestMonster = monster;
						closestDistance = distance;
					}
				}
			}
			return closestMonster;
		}

		// Token: 0x0600163C RID: 5692 RVA: 0x0010426C File Offset: 0x0010246C
		public static Microsoft.Xna.Framework.Rectangle getTranslatedRectangle(Microsoft.Xna.Framework.Rectangle r, int xTranslate, int yTranslate = 0)
		{
			return Utility.translateRect(r, xTranslate, yTranslate);
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x00104276 File Offset: 0x00102476
		public static Microsoft.Xna.Framework.Rectangle translateRect(Microsoft.Xna.Framework.Rectangle r, int xTranslate, int yTranslate = 0)
		{
			r.X += xTranslate;
			r.Y += yTranslate;
			return r;
		}

		// Token: 0x0600163E RID: 5694 RVA: 0x00104294 File Offset: 0x00102494
		public static Point getTranslatedPoint(Point p, int direction, int movementAmount)
		{
			switch (direction)
			{
			case 0:
				return new Point(p.X, p.Y - movementAmount);
			case 1:
				return new Point(p.X + movementAmount, p.Y);
			case 2:
				return new Point(p.X, p.Y + movementAmount);
			case 3:
				return new Point(p.X - movementAmount, p.Y);
			default:
				return p;
			}
		}

		// Token: 0x0600163F RID: 5695 RVA: 0x0010430C File Offset: 0x0010250C
		public static Vector2 getTranslatedVector2(Vector2 p, int direction, float movementAmount)
		{
			switch (direction)
			{
			case 0:
				return new Vector2(p.X, p.Y - movementAmount);
			case 1:
				return new Vector2(p.X + movementAmount, p.Y);
			case 2:
				return new Vector2(p.X, p.Y + movementAmount);
			case 3:
				return new Vector2(p.X - movementAmount, p.Y);
			default:
				return p;
			}
		}

		// Token: 0x06001640 RID: 5696 RVA: 0x00104382 File Offset: 0x00102582
		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1, bool ignoreSwap)
		{
			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				int t = x0;
				x0 = y0;
				y0 = t;
				t = x1;
				x1 = y1;
				y1 = t;
			}
			if (!ignoreSwap && x0 > x1)
			{
				int t2 = x0;
				x0 = x1;
				x1 = t2;
				t2 = y0;
				y0 = y1;
				y1 = t2;
			}
			int dx = x1 - x0;
			int dy = Math.Abs(y1 - y0);
			int error = dx / 2;
			int ystep = (y0 < y1) ? 1 : -1;
			int y2 = y0;
			int num;
			for (int x2 = x0; x2 <= x1; x2 = num + 1)
			{
				yield return new Point(steep ? y2 : x2, steep ? x2 : y2);
				error -= dy;
				if (error < 0)
				{
					y2 += ystep;
					error += dx;
				}
				num = x2;
			}
			yield break;
		}

		// Token: 0x06001641 RID: 5697 RVA: 0x001043B0 File Offset: 0x001025B0
		public static Vector2 getRandomAdjacentOpenTile(Vector2 tile, GameLocation location)
		{
			List<Vector2> i = Utility.getAdjacentTileLocations(tile);
			int iter = 0;
			int which = Game1.random.Next(i.Count);
			Vector2 v = i[which];
			while (iter < 4 && location.IsTileBlockedBy(v, CollisionMask.All, CollisionMask.None, false))
			{
				which = (which + 1) % i.Count;
				v = i[which];
				iter++;
			}
			if (iter >= 4)
			{
				return Vector2.Zero;
			}
			return v;
		}

		// Token: 0x06001642 RID: 5698 RVA: 0x00104418 File Offset: 0x00102618
		public static void CollectSingleItemOrShowChestMenu(Chest chest, object context = null)
		{
			int item_count = 0;
			Item item_to_grab = null;
			IInventory items = chest.Items;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					item_count++;
					if (item_count == 1)
					{
						item_to_grab = items[i];
					}
					if (item_count == 2)
					{
						item_to_grab = null;
						break;
					}
				}
			}
			if (item_count == 0)
			{
				return;
			}
			if (item_to_grab != null)
			{
				int old_stack_amount = item_to_grab.Stack;
				if (Game1.player.addItemToInventory(item_to_grab) == null)
				{
					Game1.playSound("coin", null);
					items.Remove(item_to_grab);
					chest.clearNulls();
					return;
				}
				if (item_to_grab.Stack != old_stack_amount)
				{
					Game1.playSound("coin", null);
				}
			}
			Game1.activeClickableMenu = new ItemGrabMenu(items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromChest), false, true, true, true, true, 1, null, -1, context, ItemExitBehavior.ReturnToPlayer, false);
		}

		// Token: 0x06001643 RID: 5699 RVA: 0x00104500 File Offset: 0x00102700
		public static bool CollectOrDrop(Item item, int direction)
		{
			if (item == null)
			{
				return true;
			}
			item = Game1.player.addItemToInventory(item);
			if (item != null)
			{
				if (direction != -1)
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), direction, null, -1, false);
				}
				else
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06001644 RID: 5700 RVA: 0x0010455E File Offset: 0x0010275E
		public static bool CollectOrDrop(Item item)
		{
			return Utility.CollectOrDrop(item, -1);
		}

		// Token: 0x06001645 RID: 5701 RVA: 0x00104568 File Offset: 0x00102768
		public static List<string> getExes(Farmer farmer)
		{
			List<string> exes = new List<string>();
			foreach (string key in farmer.friendshipData.Keys)
			{
				if (farmer.friendshipData[key].IsDivorced())
				{
					exes.Add(key);
				}
			}
			return exes;
		}

		// Token: 0x06001646 RID: 5702 RVA: 0x001045E0 File Offset: 0x001027E0
		public static void fixAllAnimals()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			List<GameLocation> animalLocations = new List<GameLocation>();
			HashSet<long> uniqueAnimals = new HashSet<long>();
			List<long> animalsToRemove = new List<long>();
			Utility.ForEachLocation(delegate(GameLocation f)
			{
				if (f.animals.Length == 0 && f.buildings.Count == 0)
				{
					return true;
				}
				animalLocations.Clear();
				animalLocations.Add(f);
				foreach (Building building in f.buildings)
				{
					GameLocation interior = building.GetIndoors();
					if (interior != null && interior.animals.Length > 0)
					{
						animalLocations.Add(interior);
					}
				}
				bool hasHomelessAnimals = false;
				bool hasDuplicateAnimals = false;
				foreach (GameLocation animalLocation in animalLocations)
				{
					AnimalHouse animalHouse = animalLocation as AnimalHouse;
					animalsToRemove.Clear();
					foreach (KeyValuePair<long, NetRef<FarmAnimal>> animal in animalLocation.animals.FieldDict)
					{
						NetRef<FarmAnimal> value = animal.Value;
						if (((value != null) ? value.Value : null) == null)
						{
							animalsToRemove.Add(animal.Key);
						}
						else
						{
							if (animal.Value.Value.home == null)
							{
								hasHomelessAnimals = true;
							}
							if (!uniqueAnimals.Add(animal.Value.Value.myID.Value))
							{
								animalsToRemove.Add(animal.Key);
							}
						}
					}
					hasDuplicateAnimals = (hasDuplicateAnimals || animalsToRemove.Count > 0);
					foreach (long animalToRemove in animalsToRemove)
					{
						long animalId = animalLocation.animals[animalToRemove].myID.Value;
						animalLocation.animals.Remove(animalToRemove);
						if (animalHouse != null)
						{
							animalHouse.animalsThatLiveHere.RemoveWhere((long id) => id == animalId);
						}
					}
				}
				foreach (Building building2 in f.buildings)
				{
					AnimalHouse animalHouse2 = building2.GetIndoors() as AnimalHouse;
					if (animalHouse2 != null)
					{
						foreach (long id2 in animalHouse2.animalsThatLiveHere)
						{
							FarmAnimal animal2 = Utility.getAnimal(id2);
							if (animal2 != null)
							{
								if (animal2.home == null)
								{
									hasHomelessAnimals = true;
								}
								animal2.homeInterior = animalHouse2;
							}
						}
					}
				}
				if (!hasHomelessAnimals && !hasDuplicateAnimals)
				{
					return true;
				}
				List<FarmAnimal> buggedAnimals = f.getAllFarmAnimals();
				buggedAnimals.RemoveAll((FarmAnimal a) => a.home != null);
				using (List<FarmAnimal>.Enumerator enumerator6 = buggedAnimals.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						FarmAnimal a = enumerator6.Current;
						Func<KeyValuePair<long, FarmAnimal>, bool> <>9__4;
						foreach (Building building3 in f.buildings)
						{
							GameLocation indoors = building3.GetIndoors();
							if (indoors != null)
							{
								NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>> animals = indoors.animals;
								Func<KeyValuePair<long, FarmAnimal>, bool> match;
								if ((match = <>9__4) == null)
								{
									match = (<>9__4 = ((KeyValuePair<long, FarmAnimal> pair) => pair.Value.Equals(a)));
								}
								animals.RemoveWhere(match);
							}
						}
						f.animals.RemoveWhere((KeyValuePair<long, FarmAnimal> pair) => pair.Value.Equals(a));
					}
				}
				using (List<Building>.Enumerator enumerator = f.buildings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Building b = enumerator.Current;
						AnimalHouse animalHouse3 = b.GetIndoors() as AnimalHouse;
						if (animalHouse3 != null)
						{
							animalHouse3.animalsThatLiveHere.RemoveWhere(delegate(long id)
							{
								FarmAnimal animal3 = Utility.getAnimal(id);
								return ((animal3 != null) ? animal3.home : null) != b;
							});
						}
					}
				}
				foreach (FarmAnimal a3 in buggedAnimals)
				{
					foreach (Building b2 in f.buildings)
					{
						if (a3.CanLiveIn(b2))
						{
							AnimalHouse animalHouse4 = b2.GetIndoors() as AnimalHouse;
							if (animalHouse4 != null && !animalHouse4.isFull())
							{
								animalHouse4.adoptAnimal(a3);
								break;
							}
						}
					}
				}
				foreach (FarmAnimal a2 in buggedAnimals)
				{
					if (a2.home == null)
					{
						a2.Position = Utility.recursiveFindOpenTileForCharacter(a2, f, new Vector2(40f, 40f), 200, true) * 64f;
						f.animals.TryAdd(a2.myID.Value, a2);
					}
				}
				return true;
			}, false, false);
		}

		// Token: 0x06001647 RID: 5703 RVA: 0x00104630 File Offset: 0x00102830
		public static Event getWeddingEvent(Farmer farmer)
		{
			Utility.<>c__DisplayClass111_0 CS$<>8__locals1 = new Utility.<>c__DisplayClass111_0();
			Farmer spouseFarmer = null;
			long? spouseFarmerId = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
			if (spouseFarmerId != null)
			{
				spouseFarmer = Game1.GetPlayer(spouseFarmerId.Value, false);
			}
			CS$<>8__locals1.spouseActor = ((spouseFarmer != null) ? ("farmer" + Utility.getFarmerNumberFromFarmer(spouseFarmer).ToString()) : farmer.spouse);
			WeddingData data = DataLoader.Weddings(Game1.content);
			CS$<>8__locals1.contextualAttendees = new List<WeddingAttendeeData>();
			if (data.Attendees != null)
			{
				List<string> exes = Utility.getExes(farmer);
				foreach (WeddingAttendeeData attendee in data.Attendees.Values)
				{
					CharacterData characterData;
					if (!exes.Contains(attendee.Id) && !(attendee.Id == farmer.spouse) && GameStateQuery.CheckConditions(attendee.Condition, null, farmer, null, null, null, null) && (attendee.IgnoreUnlockConditions || !NPC.TryGetData(attendee.Id, out characterData) || GameStateQuery.CheckConditions(characterData.UnlockConditions, null, farmer, null, null, null, null)))
					{
						CS$<>8__locals1.contextualAttendees.Add(attendee);
					}
				}
			}
			string weddingEventString;
			if (!data.EventScript.TryGetValue(((spouseFarmerId != null) ? spouseFarmerId.GetValueOrDefault().ToString() : null) ?? farmer.spouse, out weddingEventString) && !data.EventScript.TryGetValue("default", out weddingEventString))
			{
				throw new InvalidOperationException("The Data/Weddings asset has no wedding script with the 'default' script key.");
			}
			weddingEventString = TokenParser.ParseText(weddingEventString, null, new TokenParserDelegate(CS$<>8__locals1.<getWeddingEvent>g__ParseWeddingToken|0), farmer);
			return new Event(weddingEventString, null, "-2", farmer);
		}

		// Token: 0x06001648 RID: 5704 RVA: 0x001047F8 File Offset: 0x001029F8
		public static void DrawSquare(SpriteBatch b, Microsoft.Xna.Framework.Rectangle pixelArea, int borderWidth, Color? borderColor = null, Color? backgroundColor = null)
		{
			if (backgroundColor != null)
			{
				b.Draw(Game1.staminaRect, pixelArea, backgroundColor.Value);
			}
			if (borderWidth > 0)
			{
				Color color = borderColor ?? Color.Black;
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y, pixelArea.Width, borderWidth), color);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y + pixelArea.Height - borderWidth, pixelArea.Width, borderWidth), color);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y, borderWidth, pixelArea.Height), color);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X + pixelArea.Width - borderWidth, pixelArea.Y, borderWidth, pixelArea.Height), color);
			}
		}

		// Token: 0x06001649 RID: 5705 RVA: 0x001048E4 File Offset: 0x00102AE4
		public static void DrawErrorTexture(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle screenArea, float layerDepth)
		{
			spriteBatch.Draw(Game1.mouseCursors, screenArea, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(320, 496, 16, 16)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
		}

		// Token: 0x0600164A RID: 5706 RVA: 0x00104928 File Offset: 0x00102B28
		public static void drawTinyDigits(int toDraw, SpriteBatch b, Vector2 position, float scale, float layerDepth, Color c)
		{
			int xPosition = 0;
			int currentValue = toDraw;
			int numDigits = 0;
			do
			{
				numDigits++;
			}
			while ((toDraw /= 10) >= 1);
			int digitStrip = (int)Math.Pow(10.0, (double)(numDigits - 1));
			bool significant = false;
			for (int i = 0; i < numDigits; i++)
			{
				int currentDigit = currentValue / digitStrip % 10;
				if (currentDigit > 0 || i == numDigits - 1)
				{
					significant = true;
				}
				if (significant)
				{
					b.Draw(Game1.mouseCursors, position + new Vector2((float)xPosition, 0f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(368 + currentDigit * 5, 56, 5, 7)), c, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				}
				xPosition += (int)(5f * scale) - 1;
				digitStrip /= 10;
			}
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x001049E4 File Offset: 0x00102BE4
		public static int getWidthOfTinyDigitString(int toDraw, float scale)
		{
			int numDigits = 0;
			do
			{
				numDigits++;
			}
			while ((toDraw /= 10) >= 1);
			return (int)((float)(numDigits * 5) * scale);
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x00104A08 File Offset: 0x00102C08
		public static bool isMale(string who)
		{
			CharacterData data;
			return !NPC.TryGetData(who, out data) || data.Gender == Gender.Male;
		}

		// Token: 0x0600164D RID: 5709 RVA: 0x00104A2C File Offset: 0x00102C2C
		public static int GetMaximumHeartsForCharacter(Character character)
		{
			if (character == null)
			{
				return 0;
			}
			int max_hearts = 10;
			NPC npc = character as NPC;
			if (npc != null && npc.datable.Value)
			{
				max_hearts = 8;
			}
			Friendship friendship;
			if (Game1.player.friendshipData.TryGetValue(character.Name, out friendship))
			{
				if (friendship.IsMarried())
				{
					max_hearts = 14;
				}
				else if (friendship.IsDating())
				{
					max_hearts = 10;
				}
			}
			return max_hearts;
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x00104A8C File Offset: 0x00102C8C
		public static bool doesItemExistAnywhere(string itemId)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == null)
			{
				return false;
			}
			bool itemFound = false;
			Utility.ForEachItem(delegate(Item item)
			{
				if (item.QualifiedItemId == itemId)
				{
					itemFound = true;
				}
				return !itemFound;
			});
			return itemFound;
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x00104AE0 File Offset: 0x00102CE0
		internal static void CollectGarbage(string filePath = "", int lineNumber = 0)
		{
			GC.Collect(0, GCCollectionMode.Forced);
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x00104AEC File Offset: 0x00102CEC
		public static List<string> possibleCropsAtThisTime(Season season, bool firstWeek)
		{
			List<string> firstWeekCrops = null;
			List<string> secondWeekCrops = null;
			switch (season)
			{
			case Season.Spring:
				firstWeekCrops = new List<string>
				{
					"24",
					"192"
				};
				if (Game1.year > 1)
				{
					firstWeekCrops.Add("250");
				}
				if (Utility.doesAnyFarmerHaveMail("ccVault"))
				{
					firstWeekCrops.Add("248");
				}
				secondWeekCrops = new List<string>
				{
					"190",
					"188"
				};
				if (Utility.doesAnyFarmerHaveMail("ccVault"))
				{
					secondWeekCrops.Add("252");
				}
				secondWeekCrops.AddRange(firstWeekCrops);
				break;
			case Season.Summer:
				firstWeekCrops = new List<string>
				{
					"264",
					"262",
					"260"
				};
				secondWeekCrops = new List<string>
				{
					"254",
					"256"
				};
				if (Game1.year > 1)
				{
					firstWeekCrops.Add("266");
				}
				if (Utility.doesAnyFarmerHaveMail("ccVault"))
				{
					secondWeekCrops.AddRange(new string[]
					{
						"258",
						"268"
					});
				}
				secondWeekCrops.AddRange(firstWeekCrops);
				break;
			case Season.Fall:
				firstWeekCrops = new List<string>
				{
					"272",
					"278"
				};
				secondWeekCrops = new List<string>
				{
					"270",
					"276",
					"280"
				};
				if (Game1.year > 1)
				{
					secondWeekCrops.Add("274");
				}
				if (Utility.doesAnyFarmerHaveMail("ccVault"))
				{
					firstWeekCrops.Add("284");
					secondWeekCrops.Add("282");
				}
				secondWeekCrops.AddRange(firstWeekCrops);
				break;
			}
			if (!firstWeek)
			{
				return secondWeekCrops;
			}
			return firstWeekCrops;
		}

		// Token: 0x06001651 RID: 5713 RVA: 0x00104CA5 File Offset: 0x00102EA5
		public static float RandomFloat(float min, float max, Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			return Utility.Lerp(min, max, (float)random.NextDouble());
		}

		// Token: 0x06001652 RID: 5714 RVA: 0x00104CBF File Offset: 0x00102EBF
		public static float Clamp(float value, float min, float max)
		{
			if (max < min)
			{
				float num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		// Token: 0x06001653 RID: 5715 RVA: 0x00104CDA File Offset: 0x00102EDA
		public static Color MakeCompletelyOpaque(Color color)
		{
			if (color.A >= 255)
			{
				return color;
			}
			color.A = byte.MaxValue;
			return color;
		}

		// Token: 0x06001654 RID: 5716 RVA: 0x00104CF9 File Offset: 0x00102EF9
		public static int Clamp(int value, int min, int max)
		{
			if (max < min)
			{
				int num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		// Token: 0x06001655 RID: 5717 RVA: 0x00104D14 File Offset: 0x00102F14
		public static float Lerp(float a, float b, float t)
		{
			return a + t * (b - a);
		}

		// Token: 0x06001656 RID: 5718 RVA: 0x00104D1D File Offset: 0x00102F1D
		public static float MoveTowards(float from, float to, float delta)
		{
			if (Math.Abs(to - from) <= delta)
			{
				return to;
			}
			return from + (float)Math.Sign(to - from) * delta;
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x00104D3C File Offset: 0x00102F3C
		public static Color MultiplyColor(Color a, Color b)
		{
			return new Color((float)a.R / 255f * ((float)b.R / 255f), (float)a.G / 255f * ((float)b.G / 255f), (float)a.B / 255f * ((float)b.B / 255f), (float)a.A / 255f * ((float)b.A / 255f));
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x00104DC2 File Offset: 0x00102FC2
		public static int CalculateMinutesUntilMorning(int currentTime)
		{
			return Utility.CalculateMinutesUntilMorning(currentTime, 1);
		}

		// Token: 0x06001659 RID: 5721 RVA: 0x00104DCB File Offset: 0x00102FCB
		public static int CalculateMinutesUntilMorning(int currentTime, int daysElapsed)
		{
			if (daysElapsed < 1)
			{
				return 0;
			}
			return Utility.ConvertTimeToMinutes(2600) - Utility.ConvertTimeToMinutes(currentTime) + 400 + (daysElapsed - 1) * 1600;
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x00104DF4 File Offset: 0x00102FF4
		public static int CalculateMinutesBetweenTimes(int startTime, int endTime)
		{
			return Utility.ConvertTimeToMinutes(endTime) - Utility.ConvertTimeToMinutes(startTime);
		}

		// Token: 0x0600165B RID: 5723 RVA: 0x00104E03 File Offset: 0x00103003
		public static int ModifyTime(int timestamp, int minutes_to_add)
		{
			timestamp = Utility.ConvertTimeToMinutes(timestamp);
			timestamp += minutes_to_add;
			return Utility.ConvertMinutesToTime(timestamp);
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x00104E18 File Offset: 0x00103018
		public static int ConvertMinutesToTime(int minutes)
		{
			return minutes / 60 * 100 + minutes % 60;
		}

		// Token: 0x0600165D RID: 5725 RVA: 0x00104E26 File Offset: 0x00103026
		public static int ConvertTimeToMinutes(int time_stamp)
		{
			return time_stamp / 100 * 60 + time_stamp % 100;
		}

		// Token: 0x0600165E RID: 5726 RVA: 0x00104E34 File Offset: 0x00103034
		public static int getSellToStorePriceOfItem(Item i, bool countStack = true)
		{
			if (i != null)
			{
				return i.sellToStorePrice(-1L) * (countStack ? i.Stack : 1);
			}
			return 0;
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x00104E50 File Offset: 0x00103050
		public static int[] GetUnseenSecretNotes(Farmer who, bool journal, out int totalNotes)
		{
			Func<int, bool> query;
			if (journal)
			{
				query = ((int id) => id >= GameLocation.JOURNAL_INDEX);
			}
			else
			{
				query = ((int id) => id < GameLocation.JOURNAL_INDEX);
			}
			int[] allNotes = DataLoader.SecretNotes(Game1.content).Keys.Where(query).ToArray<int>();
			totalNotes = allNotes.Length;
			return allNotes.Except(who.secretNotesSeen.Where(query)).ToArray<int>();
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x00104EDC File Offset: 0x001030DC
		public static bool HasAnyPlayerSeenSecretNote(int note_number)
		{
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.secretNotesSeen.Contains(note_number))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001661 RID: 5729 RVA: 0x00104F34 File Offset: 0x00103134
		public static bool HasAnyPlayerSeenEvent(string eventId)
		{
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.eventsSeen.Contains(eventId))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001662 RID: 5730 RVA: 0x00104F8C File Offset: 0x0010318C
		public static bool HaveAllPlayersSeenEvent(string eventId)
		{
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.eventsSeen.Contains(eventId))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x00104FE4 File Offset: 0x001031E4
		public static List<string> GetAllPlayerUnlockedCookingRecipes()
		{
			List<string> unlocked_recipes = new List<string>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				foreach (string recipe in farmer.cookingRecipes.Keys)
				{
					if (!unlocked_recipes.Contains(recipe))
					{
						unlocked_recipes.Add(recipe);
					}
				}
			}
			return unlocked_recipes;
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x00105084 File Offset: 0x00103284
		public static List<string> GetAllPlayerUnlockedCraftingRecipes()
		{
			List<string> unlocked_recipes = new List<string>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				foreach (string recipe in farmer.craftingRecipes.Keys)
				{
					if (!unlocked_recipes.Contains(recipe))
					{
						unlocked_recipes.Add(recipe);
					}
				}
			}
			return unlocked_recipes;
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x00105124 File Offset: 0x00103324
		public static int GetAllPlayerFriendshipLevel(NPC npc)
		{
			int highest_friendship_points = -1;
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Friendship friendship;
					if (enumerator.Current.friendshipData.TryGetValue(npc.Name, out friendship) && friendship.Points > highest_friendship_points)
					{
						highest_friendship_points = friendship.Points;
					}
				}
			}
			return highest_friendship_points;
		}

		// Token: 0x06001666 RID: 5734 RVA: 0x00105190 File Offset: 0x00103390
		public static int GetAllPlayerReachedBottomOfMines()
		{
			int highest_value = 0;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.timesReachedMineBottom > highest_value)
				{
					highest_value = farmer.timesReachedMineBottom;
				}
			}
			return highest_value;
		}

		// Token: 0x06001667 RID: 5735 RVA: 0x001051E8 File Offset: 0x001033E8
		public static int GetAllPlayerDeepestMineLevel()
		{
			int highest_value = 0;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.deepestMineLevel > highest_value)
				{
					highest_value = farmer.deepestMineLevel;
				}
			}
			return highest_value;
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x00105240 File Offset: 0x00103440
		public static string LegacyWeatherToWeather(int legacyWeather)
		{
			switch (legacyWeather)
			{
			case 1:
				return "Rain";
			case 2:
				return "Wind";
			case 3:
				return "Storm";
			case 4:
				return "Festival";
			case 5:
				return "Snow";
			case 6:
				return "Wedding";
			default:
				return "Sun";
			}
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x00105298 File Offset: 0x00103498
		public static string getRandomBasicSeasonalForageItem(Season season, int randomSeedAddition = -1)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)randomSeedAddition, 0.0, 0.0, 0.0);
			string[] possibleItems = LegacyShims.EmptyArray<string>();
			switch (season)
			{
			case Season.Spring:
				possibleItems = new string[]
				{
					"16",
					"18",
					"20",
					"22"
				};
				break;
			case Season.Summer:
				possibleItems = new string[]
				{
					"396",
					"398",
					"402"
				};
				break;
			case Season.Fall:
				possibleItems = new string[]
				{
					"404",
					"406",
					"408",
					"410"
				};
				break;
			case Season.Winter:
				possibleItems = new string[]
				{
					"412",
					"414",
					"416",
					"418"
				};
				break;
			}
			return r.ChooseFrom(possibleItems) ?? "0";
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x0010539C File Offset: 0x0010359C
		public static string getRandomPureSeasonalItem(Season season, int randomSeedAddition)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)randomSeedAddition, 0.0, 0.0, 0.0);
			string[] possibleItems = LegacyShims.EmptyArray<string>();
			switch (season)
			{
			case Season.Spring:
				possibleItems = new string[]
				{
					"16",
					"18",
					"20",
					"22",
					"129",
					"131",
					"132",
					"136",
					"137",
					"142",
					"143",
					"145",
					"147",
					"148",
					"152"
				};
				break;
			case Season.Summer:
				possibleItems = new string[]
				{
					"128",
					"130",
					"131",
					"132",
					"136",
					"138",
					"142",
					"144",
					"145",
					"146",
					"149",
					"150",
					"155",
					"396",
					"398",
					"402"
				};
				break;
			case Season.Fall:
				possibleItems = new string[]
				{
					"404",
					"406",
					"408",
					"410",
					"129",
					"131",
					"132",
					"136",
					"137",
					"139",
					"140",
					"142",
					"143",
					"148",
					"150",
					"154",
					"155"
				};
				break;
			case Season.Winter:
				possibleItems = new string[]
				{
					"412",
					"414",
					"416",
					"418",
					"130",
					"131",
					"132",
					"136",
					"140",
					"141",
					"143",
					"144",
					"146",
					"147",
					"150",
					"151",
					"154"
				};
				break;
			}
			return r.ChooseFrom(possibleItems) ?? "0";
		}

		// Token: 0x0600166B RID: 5739 RVA: 0x00105658 File Offset: 0x00103858
		public static Item CreateFlavoredItem(string baseID, string preservesID, int quality = 0, int stack = 1)
		{
			ItemQueryContext context = new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random, "FLAVORED_ITEM query");
			ItemQueryResult itemQueryResult = ItemQueryResolver.TryResolve("FLAVORED_ITEM " + baseID + " " + preservesID, context, ItemQuerySearchMode.All, null, null, false, null, null).FirstOrDefault<ItemQueryResult>();
			Item resultItem = ((itemQueryResult != null) ? itemQueryResult.Item : null) as Item;
			if (resultItem != null)
			{
				resultItem.Quality = quality;
				resultItem.Stack = stack;
				return resultItem;
			}
			return null;
		}

		// Token: 0x0600166C RID: 5740 RVA: 0x001056D0 File Offset: 0x001038D0
		public static string getRandomItemFromSeason(Season season, bool forQuest, Random random)
		{
			List<string> possibleItems = new List<string>
			{
				"68",
				"66",
				"78",
				"80",
				"86",
				"152",
				"167",
				"153",
				"420"
			};
			List<string> all_unlocked_crafting_recipes = new List<string>(Game1.player.craftingRecipes.Keys);
			List<string> all_unlocked_cooking_recipes = new List<string>(Game1.player.cookingRecipes.Keys);
			if (forQuest)
			{
				all_unlocked_crafting_recipes = Utility.GetAllPlayerUnlockedCraftingRecipes();
				all_unlocked_cooking_recipes = Utility.GetAllPlayerUnlockedCookingRecipes();
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 40 || Utility.GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 40 || Game1.player.timesReachedMineBottom >= 1)))
			{
				possibleItems.AddRange(new string[]
				{
					"62",
					"70",
					"72",
					"84",
					"422"
				});
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 80 || Utility.GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 80 || Game1.player.timesReachedMineBottom >= 1)))
			{
				possibleItems.AddRange(new string[]
				{
					"64",
					"60",
					"82"
				});
			}
			if (Utility.doesAnyFarmerHaveMail("ccVault"))
			{
				possibleItems.AddRange(new string[]
				{
					"88",
					"90",
					"164",
					"165"
				});
			}
			if (all_unlocked_crafting_recipes.Contains("Furnace"))
			{
				possibleItems.AddRange(new string[]
				{
					"334",
					"335",
					"336",
					"338"
				});
			}
			if (all_unlocked_crafting_recipes.Contains("Quartz Globe"))
			{
				possibleItems.Add("339");
			}
			switch (season)
			{
			case Season.Spring:
				possibleItems.AddRange(new string[]
				{
					"16",
					"18",
					"20",
					"22",
					"129",
					"131",
					"132",
					"136",
					"137",
					"142",
					"143",
					"145",
					"147",
					"148",
					"152",
					"167",
					"267"
				});
				break;
			case Season.Summer:
				possibleItems.AddRange(new string[]
				{
					"128",
					"130",
					"132",
					"136",
					"138",
					"142",
					"144",
					"145",
					"146",
					"149",
					"150",
					"155",
					"396",
					"398",
					"402",
					"267"
				});
				break;
			case Season.Fall:
				possibleItems.AddRange(new string[]
				{
					"404",
					"406",
					"408",
					"410",
					"129",
					"131",
					"132",
					"136",
					"137",
					"139",
					"140",
					"142",
					"143",
					"148",
					"150",
					"154",
					"155",
					"269"
				});
				break;
			case Season.Winter:
				possibleItems.AddRange(new string[]
				{
					"412",
					"414",
					"416",
					"418",
					"130",
					"131",
					"132",
					"136",
					"140",
					"141",
					"144",
					"146",
					"147",
					"150",
					"151",
					"154",
					"269"
				});
				break;
			}
			if (forQuest)
			{
				foreach (string s in all_unlocked_cooking_recipes)
				{
					if (random.NextDouble() >= 0.4)
					{
						List<string> cropsAvailableNow = Utility.possibleCropsAtThisTime(Game1.season, Game1.dayOfMonth <= 7);
						string rawCraftingData;
						if (CraftingRecipe.cookingRecipes.TryGetValue(s, out rawCraftingData))
						{
							string[] fields = rawCraftingData.Split('/', StringSplitOptions.None);
							string[] ingredientsSplit = ArgUtility.SplitBySpace(ArgUtility.Get(fields, 0, null, true));
							bool ingredientsAvailable = true;
							for (int i = 0; i < ingredientsSplit.Length; i++)
							{
								if (!possibleItems.Contains(ingredientsSplit[i]) && !Utility.isCategoryIngredientAvailable(ingredientsSplit[i]) && (cropsAvailableNow == null || !cropsAvailableNow.Contains(ingredientsSplit[i])))
								{
									ingredientsAvailable = false;
									break;
								}
							}
							if (ingredientsAvailable)
							{
								string itemId = ArgUtility.Get(fields, 2, null, true);
								if (itemId != null)
								{
									possibleItems.Add(itemId);
								}
							}
						}
					}
				}
			}
			return random.ChooseFrom(possibleItems);
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x00105C78 File Offset: 0x00103E78
		public static string getRandomItemFromSeason(Season season, int randomSeedAddition, bool forQuest, bool changeDaily = true)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, changeDaily ? Game1.stats.DaysPlayed : 0U, (double)randomSeedAddition, 0.0, 0.0);
			return Utility.getRandomItemFromSeason(season, forQuest, r);
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x00105CC0 File Offset: 0x00103EC0
		private static bool isCategoryIngredientAvailable(string category)
		{
			return category != null && category.StartsWith('-') && !(category == "-5") && !(category == "-6");
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x00105CF0 File Offset: 0x00103EF0
		public static void farmerHeardSong(string trackName)
		{
			if (string.IsNullOrWhiteSpace(trackName))
			{
				return;
			}
			HashSet<string> songs = Game1.player.songsHeard;
			if (trackName == "EarthMine")
			{
				songs.Add("Crystal Bells");
				songs.Add("Cavern");
				songs.Add("Secret Gnomes");
				return;
			}
			if (trackName == "FrostMine")
			{
				songs.Add("Cloth");
				songs.Add("Icicles");
				songs.Add("XOR");
				return;
			}
			if (trackName == "LavaMine")
			{
				songs.Add("Of Dwarves");
				songs.Add("Near The Planet Core");
				songs.Add("Overcast");
				songs.Add("tribal");
				return;
			}
			if (!(trackName == "VolcanoMines"))
			{
				if (trackName != "none" && trackName != "rain" && trackName != "silence")
				{
					songs.Add(trackName);
				}
				return;
			}
			songs.Add("VolcanoMines1");
			songs.Add("VolcanoMines2");
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x00105E10 File Offset: 0x00104010
		public static float getMaxedFriendshipPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			int maxedFriends = 0;
			int totalFriends = 0;
			foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
			{
				string npcName = pair.Key;
				CharacterData data = pair.Value;
				if (data.PerfectionScore && !GameStateQuery.IsImmutablyFalse(data.CanSocialize))
				{
					totalFriends++;
					Friendship friendship;
					if (who.friendshipData.TryGetValue(npcName, out friendship))
					{
						int maxPoints = (data.CanBeRomanced ? 8 : 10) * 250;
						if (friendship != null && friendship.Points >= maxPoints)
						{
							maxedFriends++;
						}
					}
				}
			}
			return (float)maxedFriends / ((float)totalFriends * 1f);
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x00105ED8 File Offset: 0x001040D8
		public static float getCookedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
			float numberOfRecipesCooked = 0f;
			foreach (KeyValuePair<string, string> v in recipes)
			{
				string recipeKey = v.Key;
				if (who.cookingRecipes.ContainsKey(recipeKey))
				{
					string recipe = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(v.Value.Split('/', StringSplitOptions.None), 2, null, true), 0, null);
					if (who.recipesCooked.ContainsKey(recipe))
					{
						numberOfRecipesCooked += 1f;
					}
				}
			}
			return numberOfRecipesCooked / (float)recipes.Count;
		}

		// Token: 0x06001672 RID: 5746 RVA: 0x00105F90 File Offset: 0x00104190
		public static float getCraftedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> recipes = CraftingRecipe.craftingRecipes;
			float numberOfRecipesMade = 0f;
			foreach (string s in recipes.Keys)
			{
				int timesCrafted;
				if (!(s == "Wedding Ring") && who.craftingRecipes.TryGetValue(s, out timesCrafted) && timesCrafted > 0)
				{
					numberOfRecipesMade += 1f;
				}
			}
			return numberOfRecipesMade / ((float)recipes.Count - 1f);
		}

		// Token: 0x06001673 RID: 5747 RVA: 0x0010602C File Offset: 0x0010422C
		public static float getFishCaughtPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			float fishCaught = 0f;
			float totalFish = 0f;
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (data.ObjectType == "Fish")
				{
					ObjectData objData = data.RawData as ObjectData;
					if (objData == null || !objData.ExcludeFromFishingCollection)
					{
						totalFish += 1f;
						if (who.fishCaught.ContainsKey(data.QualifiedItemId))
						{
							fishCaught += 1f;
						}
					}
				}
			}
			return fishCaught / totalFish;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x001060E0 File Offset: 0x001042E0
		public static KeyValuePair<Farmer, bool> GetFarmCompletion(Func<Farmer, bool> check)
		{
			if (check(Game1.player))
			{
				return new KeyValuePair<Farmer, bool>(Game1.player, true);
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value && check(farmer))
				{
					return new KeyValuePair<Farmer, bool>(farmer, true);
				}
			}
			return new KeyValuePair<Farmer, bool>(Game1.player, false);
		}

		// Token: 0x06001675 RID: 5749 RVA: 0x00106174 File Offset: 0x00104374
		public static KeyValuePair<Farmer, float> GetFarmCompletion(Func<Farmer, float> check)
		{
			Farmer highest_farmer = Game1.player;
			float highest_value = check(Game1.player);
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value)
				{
					float current_value = check(farmer);
					if (current_value > highest_value)
					{
						highest_farmer = farmer;
						highest_value = current_value;
					}
				}
			}
			return new KeyValuePair<Farmer, float>(highest_farmer, highest_value);
		}

		// Token: 0x06001676 RID: 5750 RVA: 0x001061FC File Offset: 0x001043FC
		public static float percentGameComplete()
		{
			float total = 0f;
			float num = 0f + Utility.GetFarmCompletion((Farmer farmer) => Utility.getFarmerItemsShippedPercent(farmer)).Value * 15f;
			total += 15f;
			float num2 = num + Math.Min((float)Utility.GetObeliskTypesBuilt(), 4f);
			total += 4f;
			float num3 = num2 + (float)(Game1.IsBuildingConstructed("Gold Clock") ? 10 : 0);
			total += 10f;
			float num4 = num3 + (float)(Utility.GetFarmCompletion((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value).Value ? 10 : 0);
			total += 10f;
			float NPCFriendPercent = Utility.GetFarmCompletion((Farmer farmer) => Utility.getMaxedFriendshipPercent(farmer)).Value;
			float num5 = num4 + NPCFriendPercent * 11f;
			total += 11f;
			float farmerLevelPercent = Utility.GetFarmCompletion((Farmer farmer) => Math.Min((float)farmer.Level, 25f) / 25f).Value;
			float num6 = num5 + farmerLevelPercent * 5f;
			total += 5f;
			float num7 = num6 + (float)(Utility.GetFarmCompletion((Farmer farmer) => Utility.foundAllStardrops(farmer)).Value ? 10 : 0);
			total += 10f;
			float num8 = num7 + Utility.GetFarmCompletion((Farmer farmer) => Utility.getCookedRecipesPercent(farmer)).Value * 10f;
			total += 10f;
			float num9 = num8 + Utility.GetFarmCompletion((Farmer farmer) => Utility.getCraftedRecipesPercent(farmer)).Value * 10f;
			total += 10f;
			float num10 = num9 + Utility.GetFarmCompletion((Farmer farmer) => Utility.getFishCaughtPercent(farmer)).Value * 10f;
			total += 10f;
			float totalNuts = 130f;
			float walnutsFound = Math.Min((float)Game1.netWorldState.Value.GoldenWalnutsFound, totalNuts);
			float num11 = num10 + walnutsFound / totalNuts * 5f;
			total += 5f;
			return num11 / total;
		}

		// Token: 0x06001677 RID: 5751 RVA: 0x00106463 File Offset: 0x00104663
		public static int GetObeliskTypesBuilt()
		{
			return ((Game1.IsBuildingConstructed("Water Obelisk") > false) + (Game1.IsBuildingConstructed("Earth Obelisk") > false) + (Game1.IsBuildingConstructed("Desert Obelisk") > false) + (Game1.IsBuildingConstructed("Island Obelisk") > false)) ? 1 : 0;
		}

		// Token: 0x06001678 RID: 5752 RVA: 0x0010649C File Offset: 0x0010469C
		private static int itemsShippedPercent()
		{
			return (int)((float)Game1.player.basicShipped.Length / 92f * 5f);
		}

		// Token: 0x06001679 RID: 5753 RVA: 0x001064BC File Offset: 0x001046BC
		public static int getTrashReclamationPrice(Item i, Farmer f)
		{
			float sellPercentage = 0.15f * (float)f.trashCanLevel;
			if (i.canBeTrashed())
			{
				if (i is Wallpaper || i is Furniture)
				{
					return -1;
				}
				Object obj = i as Object;
				if ((obj != null && !obj.bigCraftable.Value) || i is MeleeWeapon || i is Ring || i is Boots)
				{
					return (int)((float)i.Stack * ((float)i.sellToStorePrice(-1L) * sellPercentage));
				}
			}
			return -1;
		}

		// Token: 0x0600167A RID: 5754 RVA: 0x00106538 File Offset: 0x00104738
		public static Quest getQuestOfTheDay()
		{
			if (Game1.stats.DaysPlayed <= 1U)
			{
				return null;
			}
			double d = Utility.CreateDaySaveRandom(100.0, Game1.stats.DaysPlayed * 777U, 0.0).NextDouble();
			Quest quest;
			if (d < 0.08)
			{
				quest = new ResourceCollectionQuest();
			}
			else if (d < 0.2 && MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5U)
			{
				quest = new SlayMonsterQuest
				{
					ignoreFarmMonsters = 
					{
						true
					}
				};
			}
			else if (d < 0.5)
			{
				quest = null;
			}
			else if (d < 0.6)
			{
				quest = new FishingQuest();
			}
			else if (d < 0.66 && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Mon"))
			{
				bool foundOne = false;
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					using (NetList<Quest, NetRef<Quest>>.Enumerator enumerator2 = farmer.questLog.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (enumerator2.Current is SocializeQuest)
							{
								foundOne = true;
								break;
							}
						}
					}
					if (foundOne)
					{
						break;
					}
				}
				if (!foundOne)
				{
					quest = new SocializeQuest();
				}
				else
				{
					quest = new ItemDeliveryQuest();
				}
			}
			else
			{
				quest = new ItemDeliveryQuest();
			}
			return quest;
		}

		// Token: 0x0600167B RID: 5755 RVA: 0x001066C0 File Offset: 0x001048C0
		public static Color? StringToColor(string rawColor)
		{
			rawColor = ((rawColor != null) ? rawColor.Trim() : null);
			if (string.IsNullOrEmpty(rawColor))
			{
				return null;
			}
			if (rawColor.StartsWith('#'))
			{
				byte alpha = byte.MaxValue;
				byte red;
				byte green;
				byte blue;
				if ((rawColor.Length == 7 || rawColor.Length == 9) && byte.TryParse(rawColor.Substring(1, 2), NumberStyles.HexNumber, null, out red) && byte.TryParse(rawColor.Substring(3, 2), NumberStyles.HexNumber, null, out green) && byte.TryParse(rawColor.Substring(5, 2), NumberStyles.HexNumber, null, out blue) && (rawColor.Length == 7 || byte.TryParse(rawColor.Substring(7, 2), NumberStyles.HexNumber, null, out alpha)))
				{
					return new Color?(new Color(red, green, blue, alpha));
				}
			}
			else if (rawColor.Contains(' '))
			{
				string[] parts = ArgUtility.SplitBySpace(rawColor);
				int red2;
				string text;
				int green2;
				int blue2;
				int alpha2;
				if ((parts.Length == 3 || parts.Length == 4) && ArgUtility.TryGetInt(parts, 0, out red2, out text, "int red") && ArgUtility.TryGetInt(parts, 1, out green2, out text, "int green") && ArgUtility.TryGetInt(parts, 2, out blue2, out text, "int blue") && ArgUtility.TryGetOptionalInt(parts, 3, out alpha2, out text, 255, "int alpha"))
				{
					return new Color?(new Color(red2, green2, blue2, alpha2));
				}
			}
			else
			{
				PropertyInfo property = typeof(Color).GetProperty(rawColor, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null)
				{
					return new Color?((Color)property.GetValue(null, null));
				}
			}
			Game1.log.Warn("Can't parse '" + rawColor + "' as a color because it's not a hexadecimal code, RGB code, or color name.");
			return null;
		}

		// Token: 0x0600167C RID: 5756 RVA: 0x00106879 File Offset: 0x00104A79
		public static Color getOppositeColor(Color color)
		{
			return new Color((int)(byte.MaxValue - color.R), (int)(byte.MaxValue - color.G), (int)(byte.MaxValue - color.B));
		}

		// Token: 0x0600167D RID: 5757 RVA: 0x001068A8 File Offset: 0x00104AA8
		public static void drawLightningBolt(Vector2 strikePosition, GameLocation l)
		{
			Microsoft.Xna.Framework.Rectangle lightningSourceRect = new Microsoft.Xna.Framework.Rectangle(644, 1078, 37, 57);
			Vector2 drawPosition = strikePosition + new Vector2((float)(-(float)lightningSourceRect.Width * 4 / 2), (float)(-(float)lightningSourceRect.Height * 4));
			while (drawPosition.Y > (float)(-(float)lightningSourceRect.Height * 4))
			{
				TemporaryAnimatedSpriteList temporarySprites = l.temporarySprites;
				TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, false, Game1.random.NextBool(), (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 4);
				defaultInterpolatedStringHandler.AppendFormatted(l.NameOrUniqueName);
				defaultInterpolatedStringHandler.AppendLiteral("_LightningBolt_");
				defaultInterpolatedStringHandler.AppendFormatted<float>(strikePosition.X);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<float>(strikePosition.Y);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
				temporaryAnimatedSprite.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
				temporaryAnimatedSprite.lightRadius = 2f;
				temporaryAnimatedSprite.delayBeforeAnimationStart = 200;
				temporaryAnimatedSprite.lightcolor = Color.Black;
				temporarySprites.Add(temporaryAnimatedSprite);
				drawPosition.Y -= (float)(lightningSourceRect.Height * 4);
			}
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x00106A14 File Offset: 0x00104C14
		public static string getDateStringFor(int day, int season, int year)
		{
			if (day <= 0)
			{
				day += 28;
				season--;
				if (season < 0)
				{
					season = 3;
					year--;
				}
			}
			else if (day > 28)
			{
				day -= 28;
				season++;
				if (season > 3)
				{
					season = 0;
					year++;
				}
			}
			if (year == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5677");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5678", day, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? Utility.getSeasonNameFromNumber(season).ToLower() : Utility.getSeasonNameFromNumber(season), year);
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x00106AA4 File Offset: 0x00104CA4
		public static string getDateString(int offset = 0)
		{
			int dayOfMonth = Game1.dayOfMonth;
			int currentSeason = Game1.seasonIndex;
			int currentYear = Game1.year;
			return Utility.getDateStringFor(dayOfMonth + offset, currentSeason, currentYear);
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x00106ACB File Offset: 0x00104CCB
		public static string getYesterdaysDate()
		{
			return Utility.getDateString(-1);
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x00106AD4 File Offset: 0x00104CD4
		public static string getSeasonNameFromNumber(int number)
		{
			switch (number)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5680");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5681");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5682");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5683");
			default:
				return "";
			}
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x00106B40 File Offset: 0x00104D40
		public static string getNumberEnding(int number)
		{
			if (number % 100 > 10 && number % 100 < 20)
			{
				return "th";
			}
			switch (number % 10)
			{
			case 0:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
				return "th";
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "";
			}
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x00106BB8 File Offset: 0x00104DB8
		public static void killAllStaticLoopingSoundCues()
		{
			ICue roadNoise = Intro.roadNoise;
			if (roadNoise != null)
			{
				roadNoise.Stop(AudioStopOptions.Immediate);
			}
			ICue buzz = Fly.buzz;
			if (buzz != null)
			{
				buzz.Stop(AudioStopOptions.Immediate);
			}
			ICue trainLoop = Railroad.trainLoop;
			if (trainLoop != null)
			{
				trainLoop.Stop(AudioStopOptions.Immediate);
			}
			ICue reelSound = BobberBar.reelSound;
			if (reelSound != null)
			{
				reelSound.Stop(AudioStopOptions.Immediate);
			}
			ICue unReelSound = BobberBar.unReelSound;
			if (unReelSound != null)
			{
				unReelSound.Stop(AudioStopOptions.Immediate);
			}
			ICue reelSound2 = FishingRod.reelSound;
			if (reelSound2 != null)
			{
				reelSound2.Stop(AudioStopOptions.Immediate);
			}
			Game1.loopingLocationCues.StopAll();
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x00106C38 File Offset: 0x00104E38
		public static void consolidateStacks(IList<Item> objects)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				Object o = objects[i] as Object;
				if (o != null)
				{
					for (int j = i + 1; j < objects.Count; j++)
					{
						if (objects[j] != null && o.canStackWith(objects[j]))
						{
							int toRemove = o.Stack - objects[j].addToStack(o);
							if (o.ConsumeStack(toRemove) == null)
							{
								objects[i] = null;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x00106CBC File Offset: 0x00104EBC
		public static void performLightningUpdate(int time_of_day)
		{
			Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)time_of_day, 0.0, 0.0);
			if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck(null) + Game1.player.team.AverageLuckLevel(null) / 100.0)
			{
				Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
				lightningEvent.bigFlash = true;
				Farm farm = Game1.getFarm();
				List<Vector2> lightningRods = new List<Vector2>();
				foreach (KeyValuePair<Vector2, Object> v in farm.objects.Pairs)
				{
					if (v.Value.QualifiedItemId == "(BC)9")
					{
						lightningRods.Add(v.Key);
					}
				}
				if (lightningRods.Count > 0)
				{
					for (int i = 0; i < 2; i++)
					{
						Vector2 v2 = random.ChooseFrom(lightningRods);
						if (farm.objects[v2].heldObject.Value == null)
						{
							farm.objects[v2].heldObject.Value = ItemRegistry.Create<Object>("(O)787", 1, 0, false);
							farm.objects[v2].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
							farm.objects[v2].shakeTimer = 1000;
							lightningEvent.createBolt = true;
							lightningEvent.boltPosition = v2 * 64f + new Vector2(32f, 0f);
							farm.lightningStrikeEvent.Fire(lightningEvent);
							return;
						}
					}
				}
				if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck(null) - Game1.player.team.AverageLuckLevel(null) / 100.0)
				{
					try
					{
						Vector2 tile;
						TerrainFeature feature;
						if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(farm.terrainFeatures, out tile, out feature, null))
						{
							FruitTree fruitTree = feature as FruitTree;
							if (fruitTree != null)
							{
								fruitTree.struckByLightningCountdown.Value = 4;
								fruitTree.shake(tile, true);
								lightningEvent.createBolt = true;
								lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
							}
							else
							{
								HoeDirt hoeDirt = feature as HoeDirt;
								Crop crop = (hoeDirt != null) ? hoeDirt.crop : null;
								bool flag = crop != null && !crop.dead.Value;
								if (feature.performToolAction(null, 50, tile))
								{
									lightningEvent.destroyedTerrainFeature = true;
									lightningEvent.createBolt = true;
									farm.terrainFeatures.Remove(tile);
									lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
								}
								if (flag && crop.dead.Value)
								{
									lightningEvent.createBolt = true;
									lightningEvent.boltPosition = tile * 64f + new Vector2(32f, 0f);
								}
							}
						}
					}
					catch (Exception)
					{
					}
				}
				farm.lightningStrikeEvent.Fire(lightningEvent);
				return;
			}
			if (random.NextDouble() < 0.1)
			{
				Farm.LightningStrikeEvent lightningEvent2 = new Farm.LightningStrikeEvent();
				lightningEvent2.smallFlash = true;
				Farm farm = Game1.getFarm();
				farm.lightningStrikeEvent.Fire(lightningEvent2);
			}
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x00107064 File Offset: 0x00105264
		public static void overnightLightning(int timeWentToSleep)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			int numberOfLoops = (2300 - timeWentToSleep) / 100;
			for (int i = 1; i <= numberOfLoops; i++)
			{
				Utility.performLightningUpdate(timeWentToSleep + i * 100);
			}
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x0010709C File Offset: 0x0010529C
		public static List<Vector2> getAdjacentTileLocations(Vector2 tileLocation)
		{
			return new List<Vector2>
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation
			};
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x0010711C File Offset: 0x0010531C
		public static Vector2[] getAdjacentTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation
			};
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x001071A0 File Offset: 0x001053A0
		public static Vector2[] getSurroundingTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation,
				new Vector2(-1f, -1f) + tileLocation,
				new Vector2(1f, -1f) + tileLocation,
				new Vector2(1f, 1f) + tileLocation,
				new Vector2(-1f, 1f) + tileLocation
			};
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x00107294 File Offset: 0x00105494
		public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation, int range = -1, Func<Crop, bool> additional_check = null)
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			openList.Enqueue(startTileLocation);
			int attempts = 0;
			while ((range >= 0 || (range < 0 && attempts <= 150)) && openList.Count > 0)
			{
				Vector2 currentTile = openList.Dequeue();
				HoeDirt dirt = location.GetHoeDirtAtTile(currentTile);
				if (((dirt != null) ? dirt.crop : null) != null)
				{
					ParsedItemData data = ItemRegistry.GetData(dirt.crop.indexOfHarvest.Value);
					if (data != null && data.Category == -80 && dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && !dirt.crop.dead.Value && (additional_check == null || additional_check(dirt.crop)))
					{
						return dirt.crop;
					}
				}
				foreach (Vector2 v in Utility.getAdjacentTileLocations(currentTile))
				{
					if (!closedList.Contains(v) && (range < 0 || Math.Abs(v.X - startTileLocation.X) + Math.Abs(v.Y - startTileLocation.Y) <= (float)range))
					{
						openList.Enqueue(v);
					}
				}
				closedList.Add(currentTile);
				attempts++;
			}
			return null;
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x00107404 File Offset: 0x00105604
		public static void recursiveFenceBuild(Vector2 position, int direction, GameLocation location, Random r)
		{
			if (r.NextDouble() < 0.04)
			{
				return;
			}
			if (location.objects.ContainsKey(position) || !location.isTileLocationOpen(new Location((int)position.X, (int)position.Y)))
			{
				return;
			}
			location.objects.Add(position, new Fence(position, "322", false));
			int directionToBuild = direction;
			if (r.NextDouble() < 0.16)
			{
				directionToBuild = r.Next(4);
			}
			if (directionToBuild == (direction + 2) % 4)
			{
				directionToBuild = (directionToBuild + 1) % 4;
			}
			switch (direction)
			{
			case 0:
				Utility.recursiveFenceBuild(position + new Vector2(0f, -1f), directionToBuild, location, r);
				return;
			case 1:
				Utility.recursiveFenceBuild(position + new Vector2(1f, 0f), directionToBuild, location, r);
				return;
			case 2:
				Utility.recursiveFenceBuild(position + new Vector2(0f, 1f), directionToBuild, location, r);
				return;
			case 3:
				Utility.recursiveFenceBuild(position + new Vector2(-1f, 0f), directionToBuild, location, r);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x0010751C File Offset: 0x0010571C
		public static bool addAnimalToFarm(FarmAnimal animal)
		{
			if (((animal != null) ? animal.Sprite : null) == null)
			{
				return false;
			}
			foreach (Building b in Game1.currentLocation.buildings)
			{
				if (animal.CanLiveIn(b))
				{
					AnimalHouse animalHouse = b.GetIndoors() as AnimalHouse;
					if (animalHouse != null && !animalHouse.isFull())
					{
						animalHouse.adoptAnimal(animal);
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x001075AC File Offset: 0x001057AC
		[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
		public static Item getItemFromStandardTextDescription(string description, Farmer who, char delimiter = ' ')
		{
			string[] array = description.Split(delimiter, StringSplitOptions.None);
			string type = array[0];
			string id = array[1];
			int stock = Convert.ToInt32(array[2]);
			return Utility.getItemFromStandardTextDescription(type, id, stock, who);
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x001075DC File Offset: 0x001057DC
		[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
		public static Item getItemFromStandardTextDescription(string type, string itemId, int stock, Farmer who)
		{
			Item item = null;
			if (type != null)
			{
				switch (type.Length)
				{
				case 1:
				{
					char c = type[0];
					if (c <= 'O')
					{
						switch (c)
						{
						case 'B':
							goto IL_29D;
						case 'C':
						{
							int index;
							item = (int.TryParse(itemId, out index) ? ItemRegistry.Create(((index >= 1000) ? "(S)" : "(P)") + itemId, 1, 0, false) : ItemRegistry.Create(itemId, 1, 0, false));
							goto IL_357;
						}
						case 'D':
						case 'E':
						case 'G':
							goto IL_357;
						case 'F':
							break;
						case 'H':
							goto IL_2EC;
						default:
							if (c != 'O')
							{
								goto IL_357;
							}
							goto IL_26B;
						}
					}
					else
					{
						if (c == 'R')
						{
							goto IL_26B;
						}
						if (c != 'W')
						{
							goto IL_357;
						}
						goto IL_2B6;
					}
					break;
				}
				case 2:
				{
					char c = type[1];
					if (c != 'L')
					{
						if (c != 'O')
						{
							goto IL_357;
						}
						if (!(type == "BO"))
						{
							goto IL_357;
						}
						goto IL_284;
					}
					else
					{
						if (!(type == "BL"))
						{
							goto IL_357;
						}
						goto IL_2CF;
					}
					break;
				}
				case 3:
				{
					char c = type[2];
					if (c != 'L')
					{
						if (c != 'l')
						{
							if (c != 't')
							{
								goto IL_357;
							}
							if (!(type == "Hat"))
							{
								goto IL_357;
							}
							goto IL_2EC;
						}
						else
						{
							if (!(type == "BBl"))
							{
								goto IL_357;
							}
							goto IL_302;
						}
					}
					else
					{
						if (!(type == "BBL"))
						{
							goto IL_357;
						}
						goto IL_302;
					}
					break;
				}
				case 4:
				{
					char c = type[0];
					if (c != 'B')
					{
						if (c != 'R')
						{
							goto IL_357;
						}
						if (!(type == "Ring"))
						{
							goto IL_357;
						}
						goto IL_26B;
					}
					else
					{
						if (!(type == "Boot"))
						{
							goto IL_357;
						}
						goto IL_29D;
					}
					break;
				}
				case 5:
				case 7:
				case 8:
				case 10:
				case 11:
					goto IL_357;
				case 6:
				{
					char c = type[0];
					if (c != 'O')
					{
						if (c != 'W')
						{
							goto IL_357;
						}
						if (!(type == "Weapon"))
						{
							goto IL_357;
						}
						goto IL_2B6;
					}
					else
					{
						if (!(type == "Object"))
						{
							goto IL_357;
						}
						goto IL_26B;
					}
					break;
				}
				case 9:
				{
					char c = type[1];
					if (c != 'i')
					{
						if (c != 'l')
						{
							if (c != 'u')
							{
								goto IL_357;
							}
							if (!(type == "Furniture"))
							{
								goto IL_357;
							}
						}
						else
						{
							if (!(type == "Blueprint"))
							{
								goto IL_357;
							}
							goto IL_2CF;
						}
					}
					else
					{
						if (!(type == "BigObject"))
						{
							goto IL_357;
						}
						goto IL_284;
					}
					break;
				}
				case 12:
					if (!(type == "BigBlueprint"))
					{
						goto IL_357;
					}
					goto IL_302;
				default:
					goto IL_357;
				}
				item = ItemRegistry.Create("(F)" + itemId, 1, 0, false);
				goto IL_357;
				IL_26B:
				item = ItemRegistry.Create("(O)" + itemId, 1, 0, false);
				goto IL_357;
				IL_284:
				item = ItemRegistry.Create("(BC)" + itemId, 1, 0, false);
				goto IL_357;
				IL_29D:
				item = ItemRegistry.Create("(B)" + itemId, 1, 0, false);
				goto IL_357;
				IL_2B6:
				item = ItemRegistry.Create("(W)" + itemId, 1, 0, false);
				goto IL_357;
				IL_2CF:
				item = ItemRegistry.Create("(O)" + itemId, 1, 0, false);
				item.IsRecipe = true;
				goto IL_357;
				IL_2EC:
				item = ItemRegistry.Create("(H)" + itemId, 1, 0, false);
				goto IL_357;
				IL_302:
				item = ItemRegistry.Create("(BC)" + itemId, 1, 0, false);
				item.IsRecipe = true;
			}
			IL_357:
			item.Stack = stock;
			if (who != null && item.IsRecipe && who.knowsRecipe(item.Name))
			{
				return null;
			}
			return item;
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x00107963 File Offset: 0x00105B63
		[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
		public static string getStandardDescriptionFromItem(Item item, int stack, char delimiter = ' ')
		{
			return Utility.getStandardDescriptionFromItem(item.TypeDefinitionId, item.ItemId, item.isRecipe.Value, item is Ring, stack, delimiter);
		}

		// Token: 0x06001690 RID: 5776 RVA: 0x0010798C File Offset: 0x00105B8C
		[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
		public static string getStandardDescriptionFromItem(string typeDefinitionId, string itemId, bool isRecipe, bool isRing, int stack, char delimiter = ' ')
		{
			string identifier;
			if (typeDefinitionId != null)
			{
				int length = typeDefinitionId.Length;
				if (length != 3)
				{
					if (length == 4)
					{
						if (typeDefinitionId == "(BC)")
						{
							identifier = (isRecipe ? "BBL" : "BO");
							goto IL_155;
						}
					}
				}
				else
				{
					char c = typeDefinitionId[1];
					if (c <= 'F')
					{
						if (c != 'B')
						{
							if (c == 'F')
							{
								if (typeDefinitionId == "(F)")
								{
									identifier = "F";
									goto IL_155;
								}
							}
						}
						else if (typeDefinitionId == "(B)")
						{
							identifier = "B";
							goto IL_155;
						}
					}
					else
					{
						if (c != 'H')
						{
							switch (c)
							{
							case 'O':
								if (!(typeDefinitionId == "(O)"))
								{
									goto IL_14F;
								}
								if (isRing)
								{
									identifier = "R";
									goto IL_155;
								}
								identifier = (isRecipe ? "BL" : "O");
								goto IL_155;
							case 'P':
								if (!(typeDefinitionId == "(P)"))
								{
									goto IL_14F;
								}
								break;
							case 'Q':
							case 'R':
								goto IL_14F;
							case 'S':
								if (!(typeDefinitionId == "(S)"))
								{
									goto IL_14F;
								}
								break;
							default:
								if (c != 'W')
								{
									goto IL_14F;
								}
								if (!(typeDefinitionId == "(W)"))
								{
									goto IL_14F;
								}
								identifier = "W";
								goto IL_155;
							}
							identifier = "C";
							goto IL_155;
						}
						if (typeDefinitionId == "(H)")
						{
							identifier = "H";
							goto IL_155;
						}
					}
				}
			}
			IL_14F:
			identifier = "";
			IL_155:
			return string.Concat(new string[]
			{
				identifier,
				delimiter.ToString(),
				itemId,
				delimiter.ToString(),
				stack.ToString()
			});
		}

		// Token: 0x06001691 RID: 5777 RVA: 0x00107B1F File Offset: 0x00105D1F
		public static TemporaryAnimatedSpriteList sparkleWithinArea(Microsoft.Xna.Framework.Rectangle bounds, int numberOfSparkles, Color sparkleColor, int delayBetweenSparkles = 100, int delayBeforeStarting = 0, string sparkleSound = "")
		{
			return Utility.getTemporarySpritesWithinArea(new int[]
			{
				10,
				11
			}, bounds, numberOfSparkles, sparkleColor, delayBetweenSparkles, delayBeforeStarting, sparkleSound);
		}

		// Token: 0x06001692 RID: 5778 RVA: 0x00107B40 File Offset: 0x00105D40
		public static TemporaryAnimatedSpriteList getTemporarySpritesWithinArea(int[] temporarySpriteRowNumbers, Microsoft.Xna.Framework.Rectangle bounds, int numberOfsprites, Color color, int delayBetweenSprites = 100, int delayBeforeStarting = 0, string sound = "")
		{
			TemporaryAnimatedSpriteList sparkles = new TemporaryAnimatedSpriteList();
			for (int i = 0; i < numberOfsprites; i++)
			{
				sparkles.Add(new TemporaryAnimatedSprite(Game1.random.Choose(temporarySpriteRowNumbers), new Vector2((float)Game1.random.Next(bounds.X, bounds.Right), (float)Game1.random.Next(bounds.Y, bounds.Bottom)), color, 8, false, 100f, 0, -1, -1f, -1, 0)
				{
					delayBeforeAnimationStart = delayBeforeStarting + delayBetweenSprites * i,
					startSound = ((sound.Length > 0) ? sound : null)
				});
			}
			return sparkles;
		}

		// Token: 0x06001693 RID: 5779 RVA: 0x00107BE4 File Offset: 0x00105DE4
		public static Vector2 getAwayFromPlayerTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Farmer who)
		{
			Point monsterPixel = monsterBox.Center;
			Point playerPixel = who.StandingPixel;
			Vector2 offset = new Vector2((float)(-(float)(playerPixel.X - monsterPixel.X)), (float)(playerPixel.Y - monsterPixel.Y));
			if (offset.Length() <= 0f)
			{
				switch (who.FacingDirection)
				{
				case 0:
					offset = new Vector2(0f, 1f);
					break;
				case 1:
					offset = new Vector2(1f, 0f);
					break;
				case 2:
					offset = new Vector2(0f, -1f);
					break;
				case 3:
					offset = new Vector2(-1f, 0f);
					break;
				}
			}
			offset.Normalize();
			offset.X *= (float)(50 + Game1.random.Next(-20, 20));
			offset.Y *= (float)(50 + Game1.random.Next(-20, 20));
			return offset;
		}

		// Token: 0x06001694 RID: 5780 RVA: 0x00107CE0 File Offset: 0x00105EE0
		public static List<string> GetJukeboxTracks(Farmer player, GameLocation location)
		{
			Dictionary<string, string> cueNamesByAlternativeId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, JukeboxTrackData> entry in Game1.jukeboxTrackData)
			{
				List<string> alternativeTrackIds = entry.Value.AlternativeTrackIds;
				if (alternativeTrackIds != null && alternativeTrackIds.Count > 0)
				{
					foreach (string id in entry.Value.AlternativeTrackIds)
					{
						if (id != null)
						{
							cueNamesByAlternativeId[id] = entry.Key;
						}
					}
				}
			}
			List<string> tracks = new List<string>();
			HashSet<string> seen = new HashSet<string>();
			foreach (KeyValuePair<string, JukeboxTrackData> entry2 in Game1.jukeboxTrackData)
			{
				bool? available = entry2.Value.Available;
				if (available != null && available.GetValueOrDefault())
				{
					tracks.Add(entry2.Key);
					seen.Add(entry2.Key);
				}
			}
			foreach (string heardId in player.songsHeard)
			{
				string cueName = cueNamesByAlternativeId.GetValueOrDefault(heardId) ?? heardId;
				if (Utility.IsValidTrackName(cueName) && seen.Add(cueName))
				{
					JukeboxTrackData data;
					if (Game1.jukeboxTrackData.TryGetValue(cueName, out data))
					{
						bool? available = data.Available;
						if (available != null && !available.GetValueOrDefault())
						{
							continue;
						}
					}
					tracks.Add(cueName);
				}
			}
			return tracks;
		}

		// Token: 0x06001695 RID: 5781 RVA: 0x00107EB4 File Offset: 0x001060B4
		public static bool IsValidTrackName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			string lowerName = name.ToLower();
			return !lowerName.Contains("ambience") && !lowerName.Contains("ambient") && !lowerName.Contains("bigdrums") && !lowerName.Contains("clubloop") && Game1.soundBank.Exists(name);
		}

		// Token: 0x06001696 RID: 5782 RVA: 0x00107F18 File Offset: 0x00106118
		public static string getSongTitleFromCueName(string cueName)
		{
			if (!string.IsNullOrWhiteSpace(cueName))
			{
				string a = cueName.ToLowerInvariant();
				if (a == "turn_off")
				{
					return Game1.content.LoadString("Strings\\UI:Mini_JukeBox_Off");
				}
				if (a == "random")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:JukeboxRandomTrack");
				}
				JukeboxTrackData data;
				if (Game1.jukeboxTrackData.TryGetValue(cueName, out data))
				{
					return TokenParser.ParseText(data.Name, null, null, null) ?? cueName;
				}
				foreach (JukeboxTrackData entry in Game1.jukeboxTrackData.Values)
				{
					List<string> alternativeTrackIds = entry.AlternativeTrackIds;
					bool? flag = (alternativeTrackIds != null) ? new bool?(alternativeTrackIds.Contains(cueName, StringComparer.OrdinalIgnoreCase)) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						return TokenParser.ParseText(entry.Name, null, null, null) ?? cueName;
					}
				}
				return cueName;
			}
			return cueName;
		}

		// Token: 0x06001697 RID: 5783 RVA: 0x00108028 File Offset: 0x00106228
		public static bool isOffScreenEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			return !Utility.isOnScreen(new Vector2((float)(currentNode.x * 64), (float)(currentNode.y * 64)), 32);
		}

		// Token: 0x06001698 RID: 5784 RVA: 0x00108050 File Offset: 0x00106250
		public static Vector2 getAwayFromPositionTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Vector2 position)
		{
			float num = -(position.X - (float)monsterBox.Center.X);
			float ySlope = position.Y - (float)monsterBox.Center.Y;
			float total = Math.Abs(num) + Math.Abs(ySlope);
			if (total < 1f)
			{
				total = 5f;
			}
			float x = num / total * 20f;
			ySlope = ySlope / total * 20f;
			return new Vector2(x, ySlope);
		}

		// Token: 0x06001699 RID: 5785 RVA: 0x001080BC File Offset: 0x001062BC
		public static bool tileWithinRadiusOfPlayer(int xTile, int yTile, int tileRadius, Farmer f)
		{
			Point point = new Point(xTile, yTile);
			Vector2 playerTile = f.Tile;
			return Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius && Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
		}

		// Token: 0x0600169A RID: 5786 RVA: 0x0010810C File Offset: 0x0010630C
		public static bool withinRadiusOfPlayer(int x, int y, int tileRadius, Farmer f)
		{
			Point point = new Point(x / 64, y / 64);
			Vector2 playerTile = f.Tile;
			return Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius && Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
		}

		// Token: 0x0600169B RID: 5787 RVA: 0x00108164 File Offset: 0x00106364
		public static bool isThereAnObjectHereWhichAcceptsThisItem(GameLocation location, Item item, int x, int y)
		{
			if (item is Tool)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
			foreach (Building building in location.buildings)
			{
				if (building.occupiesTile(tileLocation, false) && building.performActiveObjectDropInAction(Game1.player, true))
				{
					return true;
				}
			}
			Object obj;
			return location.Objects.TryGetValue(tileLocation, out obj) && obj.heldObject.Value == null && obj.performObjectDropInAction((Object)item, true, Game1.player, false);
		}

		// Token: 0x0600169C RID: 5788 RVA: 0x00108224 File Offset: 0x00106424
		public static FarmAnimal getAnimal(long id)
		{
			FarmAnimal match = null;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				FarmAnimal animal;
				if (location.animals.TryGetValue(id, out animal))
				{
					match = animal;
					return false;
				}
				return true;
			}, true, false);
			return match;
		}

		// Token: 0x0600169D RID: 5789 RVA: 0x00108251 File Offset: 0x00106451
		public static bool isWallpaperOffLimitsForSale(string index)
		{
			return index.StartsWith("MoreWalls");
		}

		// Token: 0x0600169E RID: 5790 RVA: 0x00108263 File Offset: 0x00106463
		public static bool isFlooringOffLimitsForSale(string index)
		{
			return false;
		}

		// Token: 0x0600169F RID: 5791 RVA: 0x00108268 File Offset: 0x00106468
		public static bool TryOpenShopMenu(string shopId, string ownerName, bool playOpenSound = true)
		{
			ShopData shop;
			if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out shop))
			{
				return false;
			}
			ShopOwnerType ownerType;
			if (!Utility.TryParseEnum<ShopOwnerType>(ownerName, out ownerType))
			{
				ownerType = ShopOwnerType.NamedNpc;
			}
			ShopOwnerData[] owners = ShopBuilder.GetCurrentOwners(shop).ToArray<ShopOwnerData>();
			NPC owner;
			ShopOwnerData ownerData;
			switch (ownerType)
			{
			case ShopOwnerType.Any:
			{
				owner = null;
				ShopOwnerData shopOwnerData;
				if ((shopOwnerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType)) == null)
				{
					shopOwnerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type != ShopOwnerType.None);
				}
				ownerData = shopOwnerData;
				break;
			}
			case ShopOwnerType.AnyOrNone:
				owner = null;
				ownerData = (owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType) ?? owners.FirstOrDefault<ShopOwnerData>());
				break;
			case ShopOwnerType.None:
			{
				owner = null;
				ShopOwnerData shopOwnerData2;
				if ((shopOwnerData2 = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType)) == null)
				{
					shopOwnerData2 = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone);
				}
				ownerData = shopOwnerData2;
				break;
			}
			default:
				if (ownerName == null)
				{
					owner = null;
					ownerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone || p.Type == ShopOwnerType.None);
				}
				else
				{
					owner = Game1.getCharacterFromName(ownerName, true, false);
					ownerData = (from p in owners
					orderby p.Type == ShopOwnerType.NamedNpc descending, p.Type != ShopOwnerType.None descending
					select p).FirstOrDefault((ShopOwnerData p) => p.IsValid(ownerName));
				}
				break;
			}
			Game1.activeClickableMenu = new ShopMenu(shopId, shop, ownerData, owner, null, null, playOpenSound);
			return true;
		}

		// Token: 0x060016A0 RID: 5792 RVA: 0x00108440 File Offset: 0x00106640
		public static bool TryOpenShopMenu(string shopId, GameLocation location, Microsoft.Xna.Framework.Rectangle? ownerArea = null, int? maxOwnerY = null, bool forceOpen = false, bool playOpenSound = true, Action<string> showClosedMessage = null)
		{
			ShopData shop;
			if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out shop))
			{
				return false;
			}
			Event currentEvent = location.currentEvent;
			IList<NPC> characters = (currentEvent != null) ? currentEvent.actors : null;
			if (characters == null)
			{
				characters = location.characters;
			}
			NPC owner = null;
			ShopOwnerData ownerData = null;
			ShopOwnerData[] currentOwners = ShopBuilder.GetCurrentOwners(shop).ToArray<ShopOwnerData>();
			foreach (ShopOwnerData curOwner in currentOwners)
			{
				if (!forceOpen || curOwner.ClosedMessage == null)
				{
					foreach (NPC npc2 in characters)
					{
						if (curOwner.IsValid(npc2.Name))
						{
							Point tile = npc2.TilePoint;
							bool flag;
							if (ownerArea == null || ownerArea.Value.Contains(tile))
							{
								if (maxOwnerY != null)
								{
									int y = tile.Y;
									int? num = maxOwnerY;
									flag = (y <= num.GetValueOrDefault() & num != null);
								}
								else
								{
									flag = true;
								}
							}
							else
							{
								flag = false;
							}
							if (flag)
							{
								owner = npc2;
								ownerData = curOwner;
								break;
							}
						}
					}
					if (ownerData != null)
					{
						break;
					}
				}
			}
			if (ownerData == null)
			{
				ownerData = currentOwners.FirstOrDefault((ShopOwnerData p) => (p.Type == ShopOwnerType.AnyOrNone || p.Type == ShopOwnerType.None) && (!forceOpen || p.ClosedMessage == null));
			}
			if (forceOpen && ownerData == null)
			{
				Func<NPC, bool> <>9__2;
				foreach (ShopOwnerData entry in currentOwners)
				{
					if (entry.Type == ShopOwnerType.Any)
					{
						ownerData = entry;
						owner = characters.FirstOrDefault((NPC p) => p.IsVillager);
						if (owner == null)
						{
							Func<NPC, bool> action;
							if ((action = <>9__2) == null)
							{
								action = (<>9__2 = delegate(NPC npc)
								{
									owner = npc;
									return false;
								});
							}
							Utility.ForEachVillager(action, false);
						}
					}
					else
					{
						owner = Game1.getCharacterFromName(entry.Name, true, false);
						if (owner != null)
						{
							ownerData = entry;
						}
					}
					if (ownerData != null)
					{
						break;
					}
				}
			}
			if (ownerData != null && ownerData.ClosedMessage != null)
			{
				string closedMessage = TokenParser.ParseText(ownerData.ClosedMessage, null, null, null);
				if (showClosedMessage != null)
				{
					showClosedMessage(closedMessage);
				}
				else
				{
					Game1.drawObjectDialogue(closedMessage);
				}
				return false;
			}
			if (ownerData != null | forceOpen)
			{
				Game1.activeClickableMenu = new ShopMenu(shopId, shop, ownerData, owner, null, null, true);
				return true;
			}
			return false;
		}

		// Token: 0x060016A1 RID: 5793 RVA: 0x001086C0 File Offset: 0x001068C0
		public static float ApplyQuantityModifiers(float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode = QuantityModifier.QuantityModifierMode.Stack, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null, Random random = null)
		{
			if (modifiers == null || !modifiers.Any<QuantityModifier>())
			{
				return value;
			}
			if (random == null)
			{
				random = Game1.random;
			}
			float? newValue = null;
			foreach (QuantityModifier modifier in modifiers)
			{
				float amount = modifier.Amount;
				List<float> randomAmount = modifier.RandomAmount;
				if (randomAmount != null && randomAmount.Any<float>())
				{
					amount = random.ChooseFrom(modifier.RandomAmount);
				}
				if (GameStateQuery.CheckConditions(modifier.Condition, location, player, targetItem, inputItem, random, null))
				{
					if (mode != QuantityModifier.QuantityModifierMode.Minimum)
					{
						if (mode != QuantityModifier.QuantityModifierMode.Maximum)
						{
							newValue = new float?(QuantityModifier.Apply(newValue.GetValueOrDefault(value), modifier.Modification, amount));
						}
						else
						{
							float applied = QuantityModifier.Apply(value, modifier.Modification, amount);
							if (newValue != null)
							{
								float num = applied;
								float? num2 = newValue;
								if (!(num > num2.GetValueOrDefault() & num2 != null))
								{
									continue;
								}
							}
							newValue = new float?(applied);
						}
					}
					else
					{
						float applied2 = QuantityModifier.Apply(value, modifier.Modification, amount);
						if (newValue != null)
						{
							float num3 = applied2;
							float? num2 = newValue;
							if (!(num3 < num2.GetValueOrDefault() & num2 != null))
							{
								continue;
							}
						}
						newValue = new float?(applied2);
					}
				}
			}
			return newValue.GetValueOrDefault(value);
		}

		// Token: 0x060016A2 RID: 5794 RVA: 0x00108810 File Offset: 0x00106A10
		public static bool IsForbiddenDishOfTheDay(string id)
		{
			return id == "346" || id == "196" || id == "216" || id == "224" || id == "206" || id == "395" || !ItemRegistry.Exists(id);
		}

		// Token: 0x060016A3 RID: 5795 RVA: 0x00108876 File Offset: 0x00106A76
		public static bool removeLightSource([NotNullWhen(true)] string identifier)
		{
			return identifier != null && Game1.currentLightSources.Remove(identifier);
		}

		// Token: 0x060016A4 RID: 5796 RVA: 0x00108888 File Offset: 0x00106A88
		public static Horse findHorseForPlayer(long uid)
		{
			Horse match = null;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (NPC npc in location.characters)
				{
					Horse horse = npc as Horse;
					if (horse != null && horse.ownerId.Value == uid)
					{
						match = horse;
						return false;
					}
				}
				return true;
			}, true, true);
			return match;
		}

		// Token: 0x060016A5 RID: 5797 RVA: 0x001088B5 File Offset: 0x00106AB5
		public static Horse findHorse(Guid horseId)
		{
			Horse match = null;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (NPC npc in location.characters)
				{
					Horse horse = npc as Horse;
					if (horse != null && horse.HorseId == horseId)
					{
						match = horse;
						return false;
					}
				}
				return true;
			}, true, true);
			return match;
		}

		// Token: 0x060016A6 RID: 5798 RVA: 0x001088E4 File Offset: 0x00106AE4
		public static void addDirtPuffs(GameLocation location, int tileX, int tileY, int tilesWide, int tilesHigh, int number = 5)
		{
			for (int x = tileX; x < tileX + tilesWide; x++)
			{
				for (int y = tileY; y < tileY + tilesHigh; y++)
				{
					for (int i = 0; i < number; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(46, 12), new Vector2((float)x, (float)y) * 64f + new Vector2((float)Game1.random.Next(-16, 32), (float)Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool(), 100f, 0, -1, -1f, -1, 0)
						{
							delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
							motion = new Vector2(0f, -1f),
							interval = (float)Game1.random.Next(50, 80)
						});
					}
					location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2((float)x, (float)y) * 64f + new Vector2((float)Game1.random.Next(-16, 32), (float)Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool(), 100f, 0, -1, -1f, -1, 0));
				}
			}
		}

		// Token: 0x060016A7 RID: 5799 RVA: 0x00108A5C File Offset: 0x00106C5C
		public static void addSmokePuff(GameLocation l, Vector2 v, int delay = 0, float baseScale = 2f, float scaleChange = 0.02f, float alpha = 0.75f, float alphaFade = 0.002f)
		{
			TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, false, alphaFade, Color.Gray);
			sprite.alpha = alpha;
			sprite.motion = new Vector2(0f, -0.5f);
			sprite.acceleration = new Vector2(0.002f, 0f);
			sprite.interval = 99999f;
			sprite.layerDepth = 1f;
			sprite.scale = baseScale;
			sprite.scaleChange = scaleChange;
			sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
			sprite.delayBeforeAnimationStart = delay;
			l.temporarySprites.Add(sprite);
		}

		// Token: 0x060016A8 RID: 5800 RVA: 0x00108B1C File Offset: 0x00106D1C
		public static LightSource getLightSource([NotNullWhen(true)] string identifier)
		{
			LightSource light;
			if (identifier == null || !Game1.currentLightSources.TryGetValue(identifier, out light))
			{
				return null;
			}
			return light;
		}

		// Token: 0x060016A9 RID: 5801 RVA: 0x00108B40 File Offset: 0x00106D40
		public static int SortAllFurnitures(Furniture a, Furniture b)
		{
			string leftId = a.QualifiedItemId;
			string rightId = b.QualifiedItemId;
			if (leftId != rightId)
			{
				if (leftId == "(F)1226" || leftId == "(F)1308")
				{
					return -1;
				}
				if (rightId == "(F)1226" || rightId == "(F)1308")
				{
					return 1;
				}
			}
			if (a.furniture_type.Value != b.furniture_type.Value)
			{
				return a.furniture_type.Value.CompareTo(b.furniture_type.Value);
			}
			if (a.furniture_type.Value == 12 && b.furniture_type.Value == 12)
			{
				bool flag = a.Name.StartsWith("Floor Divider ");
				bool b_is_floor_divider = b.Name.StartsWith("Floor Divider ");
				if (flag != b_is_floor_divider)
				{
					if (b_is_floor_divider)
					{
						return -1;
					}
					return 1;
				}
			}
			return a.ItemId.CompareTo(b.ItemId);
		}

		// Token: 0x060016AA RID: 5802 RVA: 0x00108C30 File Offset: 0x00106E30
		public static bool doesAnyFarmerHaveOrWillReceiveMail(string id)
		{
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.hasOrWillReceiveMail(id))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060016AB RID: 5803 RVA: 0x00108C84 File Offset: 0x00106E84
		public static string loadStringShort(string fileWithinStringsFolder, string key)
		{
			return Game1.content.LoadString("Strings\\" + fileWithinStringsFolder + ":" + key);
		}

		// Token: 0x060016AC RID: 5804 RVA: 0x00108CA4 File Offset: 0x00106EA4
		public static bool doesAnyFarmerHaveMail(string id)
		{
			if (Game1.player.mailReceived.Contains(id))
			{
				return true;
			}
			using (IEnumerator<Farmer> enumerator = Game1.otherFarmers.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.mailReceived.Contains(id))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060016AD RID: 5805 RVA: 0x00108D18 File Offset: 0x00106F18
		public static FarmEvent pickFarmEvent()
		{
			return Game1.hooks.OnUtility_PickFarmEvent(delegate
			{
				Random r = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
				for (int i = 0; i < 10; i++)
				{
					r.NextDouble();
				}
				if (Game1.weddingToday)
				{
					return null;
				}
				foreach (Farmer farmer in Game1.getOnlineFarmers())
				{
					Friendship friendship = farmer.GetSpouseFriendship();
					if (friendship != null && friendship.IsMarried() && friendship.WeddingDate == Game1.Date)
					{
						return null;
					}
				}
				if (Game1.stats.DaysPlayed == 31U)
				{
					return new SoundInTheNightEvent(4);
				}
				if (Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved%&NL&%") || Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved"))
				{
					return new WorldChangeEvent(14);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
				{
					return new WorldChangeEvent(0);
				}
				if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
				{
					return new WorldChangeEvent(1);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
				{
					return new WorldChangeEvent(6);
				}
				if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
				{
					return new WorldChangeEvent(7);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
				{
					return new WorldChangeEvent(2);
				}
				if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
				{
					return new WorldChangeEvent(3);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
				{
					return new WorldChangeEvent(4);
				}
				if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
				{
					return new WorldChangeEvent(5);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
				{
					return new WorldChangeEvent(8);
				}
				if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
				{
					return new WorldChangeEvent(9);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
				{
					return new WorldChangeEvent(10);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(11);
				}
				if (Game1.MasterPlayer.eventsSeen.Contains("191393") && (Game1.isRaining || Game1.isLightning) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(12);
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor") && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
				{
					return new WorldChangeEvent(13);
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("activateGoldenParrotsTonight") && !Game1.netWorldState.Value.ActivatedGoldenParrot)
				{
					return new WorldChangeEvent(15);
				}
				if (Game1.player.mailReceived.Contains("ccPantry") && r.NextDouble() < 0.1 && !Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
				{
					return new SoundInTheNightEvent(5);
				}
				if (!Game1.player.mailReceived.Contains("sawQiPlane"))
				{
					using (FarmerCollection.Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.mailReceived.Contains("gotFirstBillboardPrizeTicket") || Game1.stats.DaysPlayed > 50U)
							{
								return new QiPlaneEvent();
							}
						}
					}
				}
				double extraFairyChance = Game1.getFarm().hasMatureFairyRoseTonight ? 0.007 : 0.0;
				Game1.getFarm().hasMatureFairyRoseTonight = false;
				if (r.NextDouble() < 0.01 + extraFairyChance && !Game1.IsWinter && Game1.dayOfMonth != 1)
				{
					return new FairyEvent();
				}
				if (r.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 20U)
				{
					return new WitchEvent();
				}
				if (r.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 5U)
				{
					return new SoundInTheNightEvent(1);
				}
				if (r.NextDouble() < 0.005)
				{
					return new SoundInTheNightEvent(3);
				}
				if (r.NextDouble() < 0.008 && Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
				{
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "Got_Capsule", MailType.Received, true, null);
					return new SoundInTheNightEvent(0);
				}
				return null;
			});
		}

		// Token: 0x060016AE RID: 5806 RVA: 0x00108D44 File Offset: 0x00106F44
		public static bool hasFinishedJojaRoute()
		{
			bool foundJoja = false;
			if (Game1.MasterPlayer.mailReceived.Contains("jojaVault"))
			{
				foundJoja = true;
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("ccVault"))
			{
				return false;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("jojaPantry"))
			{
				foundJoja = true;
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
			{
				return false;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("jojaBoilerRoom"))
			{
				foundJoja = true;
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
			{
				return false;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("jojaCraftsRoom"))
			{
				foundJoja = true;
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom"))
			{
				return false;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("jojaFishTank"))
			{
				foundJoja = true;
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
			{
				return false;
			}
			return foundJoja || Game1.MasterPlayer.mailReceived.Contains("JojaMember");
		}

		// Token: 0x060016AF RID: 5807 RVA: 0x00108E6C File Offset: 0x0010706C
		public static FarmEvent pickPersonalFarmEvent()
		{
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2UL, 470124797.0, (double)Game1.player.UniqueMultiplayerID, 0.0);
			if (Game1.weddingToday)
			{
				return null;
			}
			NPC npcSpouse = Game1.player.getSpouse();
			bool isMarriedOrRoommates = Game1.player.isMarriedOrRoommates();
			if (isMarriedOrRoommates && Game1.player.GetSpouseFriendship().DaysUntilBirthing <= 0 && Game1.player.GetSpouseFriendship().NextBirthingDate != null)
			{
				if (npcSpouse != null)
				{
					return new BirthingEvent();
				}
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				if (Game1.otherFarmers.ContainsKey(spouseID))
				{
					return new PlayerCoupleBirthingEvent();
				}
			}
			else
			{
				if (isMarriedOrRoommates)
				{
					bool? flag = (npcSpouse != null) ? new bool?(npcSpouse.canGetPregnant()) : null;
					if (flag != null && flag.GetValueOrDefault() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && r.NextDouble() < 0.05)
					{
						CharacterData data = npcSpouse.GetData();
						if (GameStateQuery.CheckConditions((data != null) ? data.SpouseWantsChildren : null, null, null, null, null, null, null))
						{
							return new QuestionEvent(1);
						}
					}
				}
				if (isMarriedOrRoommates && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID) != null && Game1.player.GetSpouseFriendship().NextBirthingDate == null && r.NextDouble() < 0.05)
				{
					long spouseID2 = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
					Farmer farmerSpouse;
					if (Game1.otherFarmers.TryGetValue(spouseID2, out farmerSpouse))
					{
						Farmer spouse = farmerSpouse;
						if (spouse.currentLocation == Game1.player.currentLocation && (spouse.currentLocation == Game1.getLocationFromName(spouse.homeLocation.Value) || spouse.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value)) && Utility.playersCanGetPregnantHere(spouse.currentLocation as FarmHouse))
						{
							return new QuestionEvent(3);
						}
					}
				}
			}
			if (r.NextBool())
			{
				return new QuestionEvent(2);
			}
			return new SoundInTheNightEvent(2);
		}

		// Token: 0x060016B0 RID: 5808 RVA: 0x001090D8 File Offset: 0x001072D8
		public static bool playersCanGetPregnantHere(FarmHouse farmHouse)
		{
			List<Child> kids = farmHouse.getChildren();
			return farmHouse.cribStyle.Value > 0 && (farmHouse.getChildrenCount() < 2 && farmHouse.upgradeLevel >= 2 && kids.Count < 2) && (kids.Count == 0 || kids[0].Age > 2);
		}

		// Token: 0x060016B1 RID: 5809 RVA: 0x00109134 File Offset: 0x00107334
		public static string capitalizeFirstLetter(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "";
			}
			return s[0].ToString().ToUpper() + ((s.Length > 1) ? s.Substring(1) : "");
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x00109180 File Offset: 0x00107380
		public static void repositionLightSource([NotNullWhen(true)] string identifier, Vector2 position)
		{
			LightSource light;
			if (identifier != null && Game1.currentLightSources.TryGetValue(identifier, out light))
			{
				light.position.Value = position;
			}
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x001091AC File Offset: 0x001073AC
		public static bool areThereAnyOtherAnimalsWithThisName(string name)
		{
			bool found = false;
			if (name != null)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					using (NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.ValuesCollection.Enumerator enumerator = location.animals.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.displayName == name)
							{
								found = true;
								return false;
							}
						}
					}
					return true;
				}, true, false);
			}
			return found;
		}

		// Token: 0x060016B4 RID: 5812 RVA: 0x001091F0 File Offset: 0x001073F0
		public static string getNumberWithCommas(int number)
		{
			StringBuilder s = new StringBuilder(number.ToString() ?? "");
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			string comma;
			if (currentLanguageCode <= LocalizedContentManager.LanguageCode.de)
			{
				if (currentLanguageCode == LocalizedContentManager.LanguageCode.ru)
				{
					comma = " ";
					goto IL_6B;
				}
				if (currentLanguageCode - LocalizedContentManager.LanguageCode.pt > 2)
				{
					goto IL_65;
				}
			}
			else if (currentLanguageCode != LocalizedContentManager.LanguageCode.hu)
			{
				if (currentLanguageCode != LocalizedContentManager.LanguageCode.mod)
				{
					goto IL_65;
				}
				ModLanguage currentModLanguage = LocalizedContentManager.CurrentModLanguage;
				comma = (((currentModLanguage != null) ? currentModLanguage.NumberComma : null) ?? ",");
				goto IL_6B;
			}
			comma = ".";
			goto IL_6B;
			IL_65:
			comma = ",";
			IL_6B:
			for (int i = s.Length - 4; i >= 0; i -= 3)
			{
				s.Insert(i + 1, comma);
			}
			return s.ToString();
		}

		// Token: 0x060016B5 RID: 5813 RVA: 0x0010928C File Offset: 0x0010748C
		protected static bool _HasBuildingOrUpgrade(GameLocation location, string buildingId)
		{
			if (location.getNumberBuildingsConstructed(buildingId, false) > 0)
			{
				return true;
			}
			foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
			{
				string curId = pair.Key;
				BuildingData building = pair.Value;
				if (!(curId == buildingId) && building.BuildingToUpgrade == buildingId && Utility._HasBuildingOrUpgrade(location, curId))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060016B6 RID: 5814 RVA: 0x00109318 File Offset: 0x00107518
		public static List<int> getDaysOfBooksellerThisSeason()
		{
			Random r = Utility.CreateRandom((double)(Game1.year * 11), Game1.uniqueIDForThisGame, (double)Game1.seasonIndex, 0.0, 0.0);
			int[] possible_days = null;
			List<int> days = new List<int>();
			switch (Game1.season)
			{
			case Season.Spring:
				possible_days = new int[]
				{
					11,
					12,
					21,
					22,
					25
				};
				break;
			case Season.Summer:
				possible_days = new int[]
				{
					9,
					12,
					18,
					25,
					27
				};
				break;
			case Season.Fall:
				possible_days = new int[]
				{
					4,
					7,
					8,
					9,
					12,
					19,
					22,
					25
				};
				break;
			case Season.Winter:
				possible_days = new int[]
				{
					5,
					11,
					12,
					19,
					22,
					24
				};
				break;
			}
			int index = r.Next(possible_days.Length);
			days.Add(possible_days[index]);
			days.Add(possible_days[(index + possible_days.Length / 2) % possible_days.Length]);
			return days;
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x001093F0 File Offset: 0x001075F0
		public static bool isGreenRainDay()
		{
			return Utility.isGreenRainDay(Game1.dayOfMonth, Game1.season);
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x00109404 File Offset: 0x00107604
		public static bool isGreenRainDay(int day, Season season)
		{
			if (season == Season.Summer)
			{
				Random r = Utility.CreateRandom((double)(Game1.year * 777), Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0);
				int[] possible_days = new int[]
				{
					5,
					6,
					7,
					14,
					15,
					16,
					18,
					23
				};
				return day == r.ChooseFrom(possible_days);
			}
			return false;
		}

		// Token: 0x060016B9 RID: 5817 RVA: 0x00109468 File Offset: 0x00107668
		public static List<Object> getPurchaseAnimalStock(GameLocation location)
		{
			List<Object> stock = new List<Object>();
			foreach (KeyValuePair<string, FarmAnimalData> pair in Game1.farmAnimalData)
			{
				FarmAnimalData data = pair.Value;
				if (data.PurchasePrice >= 0 && GameStateQuery.CheckConditions(data.UnlockCondition, null, null, null, null, null, null))
				{
					Object o = new Object("100", 1, false, data.PurchasePrice, 0)
					{
						Name = pair.Key,
						Type = null
					};
					if (data.RequiredBuilding != null && !Utility._HasBuildingOrUpgrade(location, data.RequiredBuilding))
					{
						o.Type = ((data.ShopMissingBuildingDescription == null) ? "" : TokenParser.ParseText(data.ShopMissingBuildingDescription, null, null, null));
					}
					o.displayNameFormat = data.ShopDisplayName;
					stock.Add(o);
				}
			}
			return stock;
		}

		// Token: 0x060016BA RID: 5818 RVA: 0x0010955C File Offset: 0x0010775C
		public static string SanitizeName(string name)
		{
			return Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty);
		}

		// Token: 0x060016BB RID: 5819 RVA: 0x00109570 File Offset: 0x00107770
		public static void FixChildNameCollisions()
		{
			List<NPC> all_characters = Utility.getAllCharacters();
			foreach (NPC character in all_characters)
			{
				if (character is Child)
				{
					string old_character_name = character.Name;
					string character_name = character.Name;
					bool collision_found;
					do
					{
						collision_found = false;
						if (Game1.characterData.ContainsKey(character_name))
						{
							character_name += " ";
							collision_found = true;
						}
						else
						{
							foreach (NPC i in all_characters)
							{
								if (i != character && i.name.Equals(character_name))
								{
									character_name += " ";
									collision_found = true;
								}
							}
						}
					}
					while (collision_found);
					if (character_name != character.Name)
					{
						character.Name = character_name;
						character.displayName = null;
						foreach (Farmer farmer in Game1.getAllFarmers())
						{
							Friendship oldFriendship;
							if (farmer.friendshipData != null && farmer.friendshipData.TryGetValue(old_character_name, out oldFriendship))
							{
								farmer.friendshipData[character_name] = oldFriendship;
								farmer.friendshipData.Remove(old_character_name);
							}
						}
					}
				}
			}
		}

		// Token: 0x060016BC RID: 5820 RVA: 0x00109718 File Offset: 0x00107918
		public static Vector2 getCornersOfThisRectangle(ref Microsoft.Xna.Framework.Rectangle r, int corner)
		{
			switch (corner)
			{
			case 1:
				return new Vector2((float)(r.Right - 1), (float)r.Y);
			case 2:
				return new Vector2((float)(r.Right - 1), (float)(r.Bottom - 1));
			case 3:
				return new Vector2((float)r.X, (float)(r.Bottom - 1));
			default:
				return new Vector2((float)r.X, (float)r.Y);
			}
		}

		// Token: 0x060016BD RID: 5821 RVA: 0x00109790 File Offset: 0x00107990
		public static Vector2 GetCurvePoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float cx = 3f * (p1.X - p0.X);
			float cy = 3f * (p1.Y - p0.Y);
			float bx = 3f * (p2.X - p1.X) - cx;
			float by = 3f * (p2.Y - p1.Y) - cy;
			float num = p3.X - p0.X - cx - bx;
			float ay = p3.Y - p0.Y - cy - by;
			float Cube = t * t * t;
			float Square = t * t;
			float x = num * Cube + bx * Square + cx * t + p0.X;
			float resY = ay * Cube + by * Square + cy * t + p0.Y;
			return new Vector2(x, resY);
		}

		// Token: 0x060016BE RID: 5822 RVA: 0x00109854 File Offset: 0x00107A54
		public static GameLocation getGameLocationOfCharacter(NPC n)
		{
			return n.currentLocation;
		}

		// Token: 0x060016BF RID: 5823 RVA: 0x0010985C File Offset: 0x00107A5C
		public static int[] parseStringToIntArray(string s, char delimiter = ' ')
		{
			string[] split = s.Split(delimiter, StringSplitOptions.None);
			int[] result = new int[split.Length];
			for (int i = 0; i < split.Length; i++)
			{
				result[i] = Convert.ToInt32(split[i]);
			}
			return result;
		}

		// Token: 0x060016C0 RID: 5824 RVA: 0x00109898 File Offset: 0x00107A98
		public static void drawLineWithScreenCoordinates(int x1, int y1, int x2, int y2, SpriteBatch b, Color color1, float layerDepth = 1f, int thickness = 1)
		{
			Vector2 value = new Vector2((float)x2, (float)y2);
			Vector2 start = new Vector2((float)x1, (float)y1);
			Vector2 edge = value - start;
			float angle = (float)Math.Atan2((double)edge.Y, (double)edge.X);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y + 1, (int)edge.Length(), thickness), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016C1 RID: 5825 RVA: 0x00109965 File Offset: 0x00107B65
		public static Farmer isThereAFarmerWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation location)
		{
			return Utility.GetPlayersWithinDistance(tileLocation, tilesAway, location).FirstOrDefault<Farmer>();
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x00109974 File Offset: 0x00107B74
		public static Character isThereAFarmerOrCharacterWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation environment)
		{
			Character result = Utility.GetNpcsWithinDistance(tileLocation, tilesAway, environment).FirstOrDefault<NPC>();
			if (result == null)
			{
				result = Utility.GetPlayersWithinDistance(tileLocation, tilesAway, environment).FirstOrDefault<Farmer>();
			}
			return result;
		}

		// Token: 0x060016C3 RID: 5827 RVA: 0x001099A1 File Offset: 0x00107BA1
		public static IEnumerable<NPC> GetNpcsWithinDistance(Vector2 centerTile, int tilesAway, GameLocation location)
		{
			foreach (NPC npc in location.characters)
			{
				if (Vector2.Distance(npc.Tile, centerTile) <= (float)tilesAway)
				{
					yield return npc;
				}
			}
			List<NPC>.Enumerator enumerator = default(List<NPC>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x060016C4 RID: 5828 RVA: 0x001099BF File Offset: 0x00107BBF
		public static IEnumerable<Farmer> GetPlayersWithinDistance(Vector2 centerTile, int tilesAway, GameLocation location)
		{
			foreach (Farmer player in location.farmers)
			{
				if (Vector2.Distance(player.Tile, centerTile) <= (float)tilesAway)
				{
					yield return player;
				}
			}
			FarmerCollection.Enumerator enumerator = default(FarmerCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x060016C5 RID: 5829 RVA: 0x001099E0 File Offset: 0x00107BE0
		public static Color getRedToGreenLerpColor(float power)
		{
			return new Color((int)((power <= 0.5f) ? 255f : ((1f - power) * 2f * 255f)), (int)Math.Min(255f, power * 2f * 255f), 0);
		}

		// Token: 0x060016C6 RID: 5830 RVA: 0x00109A2E File Offset: 0x00107C2E
		public static FarmHouse getHomeOfFarmer(Farmer who)
		{
			return Game1.RequireLocation<FarmHouse>(who.homeLocation.Value, false);
		}

		// Token: 0x060016C7 RID: 5831 RVA: 0x00109A41 File Offset: 0x00107C41
		public static Vector2 getRandomPositionOnScreen()
		{
			return new Vector2((float)Game1.random.Next(Game1.viewport.Width), (float)Game1.random.Next(Game1.viewport.Height));
		}

		// Token: 0x060016C8 RID: 5832 RVA: 0x00109A74 File Offset: 0x00107C74
		public static Vector2 getRandomPositionOnScreenNotOnMap()
		{
			Vector2 output = Vector2.Zero;
			int tries = 0;
			while (tries < 30 && (output.Equals(Vector2.Zero) || Game1.currentLocation.isTileOnMap((output + new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y)) / 64f)))
			{
				output = Utility.getRandomPositionOnScreen();
				tries++;
			}
			if (tries >= 30)
			{
				return new Vector2(-1000f, -1000f);
			}
			return output;
		}

		// Token: 0x060016C9 RID: 5833 RVA: 0x00109AF5 File Offset: 0x00107CF5
		public static Microsoft.Xna.Framework.Rectangle getRectangleCenteredAt(Vector2 v, int size)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)v.X - size / 2, (int)v.Y - size / 2, size, size);
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x00109B14 File Offset: 0x00107D14
		public static bool checkForCharacterInteractionAtTile(Vector2 tileLocation, Farmer who)
		{
			NPC character = Game1.currentLocation.isCharacterAtTile(tileLocation);
			if (character != null && !character.IsMonster && !character.IsInvisible)
			{
				if (character.SimpleNonVillagerNPC && character.nonVillagerNPCTimesTalked != -1)
				{
					Game1.mouseCursor = Game1.cursor_talk;
				}
				else if (Game1.currentLocation is MovieTheater)
				{
					Game1.mouseCursor = Game1.cursor_talk;
				}
				else
				{
					if (character.Name == "Pierre")
					{
						Object activeObject = who.ActiveObject;
						if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)897" && character.tryToReceiveActiveObject(who, true))
						{
							Game1.mouseCursor = Game1.cursor_gift;
							goto IL_20D;
						}
					}
					Item activeItem = who.ActiveItem;
					bool? flag = (activeItem != null) ? new bool?(activeItem.canBeGivenAsGift()) : null;
					if (flag != null && flag.GetValueOrDefault() && character.CanReceiveGifts() && !who.isRidingHorse() && who.friendshipData.ContainsKey(character.Name) && !Game1.eventUp)
					{
						Game1.mouseCursor = (character.tryToReceiveActiveObject(who, true) ? Game1.cursor_gift : Game1.cursor_default);
					}
					else if (character.canTalk())
					{
						if (character.CurrentDialogue == null || character.CurrentDialogue.Count <= 0)
						{
							if (Game1.player.spouse != null && character.Name != null && character.Name == Game1.player.spouse && character.shouldSayMarriageDialogue.Value)
							{
								NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = character.currentMarriageDialogue;
								if (currentMarriageDialogue != null && currentMarriageDialogue.Count > 0)
								{
									goto IL_1FB;
								}
							}
							if (!character.hasTemporaryMessageAvailable() && (!who.hasClubCard || !character.Name.Equals("Bouncer") || !who.IsLocalPlayer) && (!character.Name.Equals("Henchman") || !character.currentLocation.Name.Equals("WitchSwamp") || who.hasOrWillReceiveMail("henchmanGone")))
							{
								goto IL_20D;
							}
						}
						IL_1FB:
						if (!character.isOnSilentTemporaryMessage())
						{
							Game1.mouseCursor = Game1.cursor_talk;
						}
					}
				}
				IL_20D:
				if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence)
				{
					Game1.mouseCursor = Game1.cursor_default;
				}
				Game1.currentLocation.checkForSpecialCharacterIconAtThisTile(tileLocation);
				if (Game1.mouseCursor == Game1.cursor_gift || Game1.mouseCursor == Game1.cursor_talk)
				{
					if (Utility.tileWithinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, who))
					{
						Game1.mouseCursorTransparency = 1f;
					}
					else
					{
						Game1.mouseCursorTransparency = 0.5f;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x00109DA8 File Offset: 0x00107FA8
		public static bool canGrabSomethingFromHere(int x, int y, Farmer who)
		{
			if (Game1.currentLocation == null)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
			if (Game1.currentLocation.isObjectAt(x, y))
			{
				Game1.currentLocation.getObjectAt(x, y, false).hoverAction();
			}
			if (Utility.checkForCharacterInteractionAtTile(tileLocation, who))
			{
				return false;
			}
			if (Utility.checkForCharacterInteractionAtTile(tileLocation + new Vector2(0f, 1f), who))
			{
				return false;
			}
			if (who.IsLocalPlayer)
			{
				if (who.onBridge.Value)
				{
					return false;
				}
				if (Game1.currentLocation != null)
				{
					foreach (Furniture f in Game1.currentLocation.furniture)
					{
						if (f.GetBoundingBox().Contains(Utility.Vector2ToPoint(tileLocation * 64f)) && f.IsTable() && f.heldObject.Value != null)
						{
							return true;
						}
					}
				}
				Object obj;
				TerrainFeature terrainFeature;
				if (Game1.currentLocation.Objects.TryGetValue(tileLocation, out obj))
				{
					if (!obj.readyForHarvest.Value && !obj.isSpawnedObject.Value)
					{
						IndoorPot pot = obj as IndoorPot;
						if (pot == null || !pot.hoeDirt.Value.readyForHarvest())
						{
							return false;
						}
					}
					Game1.mouseCursor = Game1.cursor_harvest;
					if (!Utility.withinRadiusOfPlayer(x, y, 1, who))
					{
						Game1.mouseCursorTransparency = 0.5f;
						return false;
					}
					return true;
				}
				else if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation, out terrainFeature))
				{
					HoeDirt dirt = terrainFeature as HoeDirt;
					if (dirt != null && dirt.readyForHarvest())
					{
						Game1.mouseCursor = Game1.cursor_harvest;
						if (!Utility.withinRadiusOfPlayer(x, y, 1, who))
						{
							Game1.mouseCursorTransparency = 0.5f;
							return false;
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060016CC RID: 5836 RVA: 0x00109F80 File Offset: 0x00108180
		public static int getStringCountInList(List<string> strings, string whichStringToCheck)
		{
			int num = 0;
			if (strings != null)
			{
				using (List<string>.Enumerator enumerator = strings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == whichStringToCheck)
						{
							num++;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x060016CD RID: 5837 RVA: 0x00109FD8 File Offset: 0x001081D8
		public static Microsoft.Xna.Framework.Rectangle getSourceRectWithinRectangularRegion(int regionX, int regionY, int regionWidth, int sourceIndex, int sourceWidth, int sourceHeight)
		{
			int sourceRectWidthsOfRegion = regionWidth / sourceWidth;
			return new Microsoft.Xna.Framework.Rectangle(regionX + sourceIndex % sourceRectWidthsOfRegion * sourceWidth, regionY + sourceIndex / sourceRectWidthsOfRegion * sourceHeight, sourceWidth, sourceHeight);
		}

		// Token: 0x060016CE RID: 5838 RVA: 0x0010A004 File Offset: 0x00108204
		public static void drawWithShadow(SpriteBatch b, Texture2D texture, Vector2 position, Microsoft.Xna.Framework.Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale = -1f, bool flipped = false, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 0.35f)
		{
			if (scale == -1f)
			{
				scale = 4f;
			}
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = -4;
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = 4;
			}
			b.Draw(texture, position + new Vector2((float)horizontalShadowOffset, (float)verticalShadowOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRect), Color.Black * shadowIntensity * ((float)color.A / 255f), rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth - 0.0001f);
			b.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(sourceRect), color, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016CF RID: 5839 RVA: 0x0010A0BC File Offset: 0x001082BC
		public static void drawTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? -2 : -3);
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
			}
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, (float)verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows != 2)
			{
				if (numShadows == 3)
				{
					b.DrawString(font, text, position + new Vector2(0f, (float)verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
				}
			}
			else
			{
				b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016D0 RID: 5840 RVA: 0x0010A21C File Offset: 0x0010841C
		public static void drawTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? -2 : -3);
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, (float)verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows != 2)
			{
				if (numShadows == 3)
				{
					b.DrawString(font, text, position + new Vector2(0f, (float)verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
				}
			}
			else
			{
				b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016D1 RID: 5841 RVA: 0x0010A388 File Offset: 0x00108588
		public static void drawTextWithColoredShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, Color shadowColor, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? -2 : -3);
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, (float)verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows != 2)
			{
				if (numShadows == 3)
				{
					b.DrawString(font, text, position + new Vector2(0f, (float)verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
				}
			}
			else
			{
				b.DrawString(font, text, position + new Vector2((float)horizontalShadowOffset, 0f), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016D2 RID: 5842 RVA: 0x0010A4C8 File Offset: 0x001086C8
		public static void drawBoldText(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int boldnessOffset = 1)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2((float)boldnessOffset, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2((float)boldnessOffset, (float)boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(0f, (float)boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060016D3 RID: 5843 RVA: 0x0010A588 File Offset: 0x00108788
		protected static bool _HasNonMousePlacementLeeway(int x, int y, Item item, Farmer f)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return false;
			}
			Point start_point = f.TilePoint;
			if (!Utility.withinRadiusOfPlayer(x, y, 2, f))
			{
				return false;
			}
			if (item.Category == -74)
			{
				return true;
			}
			foreach (Point p in Utility.GetPointsOnLine(start_point.X, start_point.Y, x / 64, y / 64))
			{
				if (!(p == start_point) && !item.canBePlacedHere(f.currentLocation, new Vector2((float)p.X, (float)p.Y), ~(CollisionMask.Characters | CollisionMask.Farmers), false))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060016D4 RID: 5844 RVA: 0x0010A644 File Offset: 0x00108844
		public static bool isPlacementForbiddenHere(GameLocation location)
		{
			return location == null || Utility.isPlacementForbiddenHere(location.name.Value);
		}

		// Token: 0x060016D5 RID: 5845 RVA: 0x0010A65B File Offset: 0x0010885B
		public static bool TryGetPassiveFestivalData(string festivalId, out PassiveFestivalData data)
		{
			if (festivalId == null)
			{
				data = null;
				return false;
			}
			return DataLoader.PassiveFestivals(Game1.content).TryGetValue(festivalId, out data);
		}

		// Token: 0x060016D6 RID: 5846 RVA: 0x0010A678 File Offset: 0x00108878
		public static bool TryGetPassiveFestivalDataForDay(int dayOfMonth, Season season, string locationContextId, out string id, out PassiveFestivalData data, bool ignoreConditionsCheck = false)
		{
			bool checkDateAndConditions = true;
			ICollection<string> possibleIds;
			if (dayOfMonth == Game1.dayOfMonth && season == Game1.season)
			{
				possibleIds = Game1.netWorldState.Value.ActivePassiveFestivals;
				checkDateAndConditions = false;
			}
			else
			{
				possibleIds = DataLoader.PassiveFestivals(Game1.content).Keys;
			}
			foreach (string curId in possibleIds)
			{
				id = curId;
				if (Utility.TryGetPassiveFestivalData(id, out data) && (!checkDateAndConditions || (dayOfMonth >= data.StartDay && dayOfMonth <= data.EndDay && season == data.Season && (ignoreConditionsCheck || GameStateQuery.CheckConditions(data.Condition, null, null, null, null, null, null)))))
				{
					if (locationContextId != null)
					{
						if (data.MapReplacements == null)
						{
							continue;
						}
						using (Dictionary<string, string>.KeyCollection.Enumerator enumerator2 = data.MapReplacements.Keys.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								string name = enumerator2.Current;
								GameLocation locationFromName = Game1.getLocationFromName(name);
								if (((locationFromName != null) ? locationFromName.GetLocationContextId() : null) == locationContextId)
								{
									return true;
								}
							}
							continue;
						}
					}
					return true;
				}
			}
			id = null;
			data = null;
			return false;
		}

		// Token: 0x060016D7 RID: 5847 RVA: 0x0010A7C8 File Offset: 0x001089C8
		public static bool IsPassiveFestivalDay()
		{
			string text;
			PassiveFestivalData passiveFestivalData;
			return Utility.TryGetPassiveFestivalDataForDay(Game1.dayOfMonth, Game1.season, null, out text, out passiveFestivalData, false);
		}

		// Token: 0x060016D8 RID: 5848 RVA: 0x0010A7EC File Offset: 0x001089EC
		public static bool IsPassiveFestivalDay(int dayOfMonth, Season season, string locationContextId)
		{
			string text;
			PassiveFestivalData passiveFestivalData;
			return Utility.TryGetPassiveFestivalDataForDay(dayOfMonth, season, locationContextId, out text, out passiveFestivalData, false);
		}

		// Token: 0x060016D9 RID: 5849 RVA: 0x0010A806 File Offset: 0x00108A06
		public static bool IsPassiveFestivalDay(string festivalId)
		{
			return Game1.netWorldState.Value.ActivePassiveFestivals.Contains(festivalId);
		}

		// Token: 0x060016DA RID: 5850 RVA: 0x0010A820 File Offset: 0x00108A20
		public static bool IsPassiveFestivalOpen(string festivalId)
		{
			PassiveFestivalData festival;
			return Utility.IsPassiveFestivalDay(festivalId) && Utility.TryGetPassiveFestivalData(festivalId, out festival) && Game1.timeOfDay >= festival.StartTime;
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x0010A854 File Offset: 0x00108A54
		public static int GetDayOfPassiveFestival(string festivalId)
		{
			PassiveFestivalData festival;
			if (!Utility.IsPassiveFestivalDay(festivalId) || !Utility.TryGetPassiveFestivalData(festivalId, out festival))
			{
				return -1;
			}
			return Game1.dayOfMonth - festival.StartDay + 1;
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x0010A884 File Offset: 0x00108A84
		public static bool isPlacementForbiddenHere(string location_name)
		{
			if (location_name == "AbandonedJojaMart")
			{
				return true;
			}
			using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PassiveFestivalData festival;
					if (Utility.TryGetPassiveFestivalData(enumerator.Current, out festival) && festival.MapReplacements != null)
					{
						foreach (string festivalLocationName in festival.MapReplacements.Values)
						{
							if (location_name == festivalLocationName)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x0010A944 File Offset: 0x00108B44
		public static void transferPlacedObjectsFromOneLocationToAnother(GameLocation source, GameLocation destination, Vector2? overflow_chest_position = null, GameLocation overflow_chest_location = null)
		{
			if (source == null)
			{
				return;
			}
			List<Item> invalid_objects = new List<Item>();
			foreach (Vector2 position in new List<Vector2>(source.objects.Keys))
			{
				if (source.objects[position] != null)
				{
					Object source_object = source.objects[position];
					bool flag = destination != null && !destination.objects.ContainsKey(position) && destination.CanItemBePlacedHere(position, false, CollisionMask.All, ~CollisionMask.Objects, false, false);
					source.objects.Remove(position);
					if (flag && destination != null)
					{
						destination.objects[position] = source_object;
					}
					else
					{
						invalid_objects.Add(source_object);
						Chest source_chest = source_object as Chest;
						if (source_chest != null)
						{
							List<Item> chest_items = new List<Item>(source_chest.Items);
							source_chest.Items.Clear();
							foreach (Item chest_item in chest_items)
							{
								if (chest_item != null)
								{
									invalid_objects.Add(chest_item);
								}
							}
						}
					}
				}
			}
			if (overflow_chest_position != null)
			{
				if (overflow_chest_location != null)
				{
					Utility.createOverflowChest(overflow_chest_location, overflow_chest_position.Value, invalid_objects);
					return;
				}
				if (destination != null)
				{
					Utility.createOverflowChest(destination, overflow_chest_position.Value, invalid_objects);
				}
			}
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x0010AAB0 File Offset: 0x00108CB0
		public static void createOverflowChest(GameLocation destination, Vector2 overflow_chest_location, List<Item> overflow_items)
		{
			List<Chest> chests = new List<Chest>();
			foreach (Item overflow_object in overflow_items)
			{
				if (chests.Count == 0)
				{
					chests.Add(new Chest(true, "130"));
				}
				bool found_chest_to_stash_in = false;
				using (List<Chest>.Enumerator enumerator2 = chests.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.addItem(overflow_object) == null)
						{
							found_chest_to_stash_in = true;
							break;
						}
					}
				}
				if (!found_chest_to_stash_in)
				{
					Chest new_chest = new Chest(true, "130");
					new_chest.addItem(overflow_object);
					chests.Add(new_chest);
				}
			}
			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				Utility._placeOverflowChestInNearbySpace(destination, overflow_chest_location, chest);
			}
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x0010ABAC File Offset: 0x00108DAC
		protected static void _placeOverflowChestInNearbySpace(GameLocation location, Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int attempts = 0;
			Queue<Vector2> open_list = new Queue<Vector2>();
			HashSet<Vector2> closed_list = new HashSet<Vector2>();
			open_list.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			while (attempts < 100)
			{
				current = open_list.Dequeue();
				if (location.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					break;
				}
				closed_list.Add(current);
				foreach (Vector2 v in Utility.getAdjacentTileLocations(current))
				{
					if (!closed_list.Contains(v))
					{
						open_list.Enqueue(v);
					}
				}
				attempts++;
			}
			if (!current.Equals(Vector2.Zero) && location.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
			{
				o.TileLocation = current;
				location.objects.Add(current, o);
			}
		}

		// Token: 0x060016E0 RID: 5856 RVA: 0x0010ACA4 File Offset: 0x00108EA4
		public static bool isWithinTileWithLeeway(int x, int y, Item item, Farmer f)
		{
			return Utility.withinRadiusOfPlayer(x, y, 1, f) || Utility._HasNonMousePlacementLeeway(x, y, item, f);
		}

		// Token: 0x060016E1 RID: 5857 RVA: 0x0010ACBC File Offset: 0x00108EBC
		public static bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f, bool show_error = false)
		{
			if (Utility.isPlacementForbiddenHere(location))
			{
				return false;
			}
			if (item == null || item is Tool || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
			{
				return false;
			}
			if (!Utility.isWithinTileWithLeeway(x, y, item, f) && (!(item is Wallpaper) || !(location is DecoratableLocation)))
			{
				Furniture curFurniture = item as Furniture;
				if (curFurniture == null || !location.CanPlaceThisFurnitureHere(curFurniture))
				{
					return false;
				}
			}
			Furniture furniture = item as Furniture;
			if (furniture != null && !location.CanFreePlaceFurniture() && !furniture.IsCloseEnoughToFarmer(f, new int?(x / 64), new int?(y / 64)))
			{
				return false;
			}
			Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
			if (item.canBePlacedHere(location, tileLocation, CollisionMask.All, show_error))
			{
				return item.isPlaceable();
			}
			return false;
		}

		// Token: 0x060016E2 RID: 5858 RVA: 0x0010AD8C File Offset: 0x00108F8C
		public static string GetDoubleWideVersionOfBed(string bedId)
		{
			int bed_index;
			if (int.TryParse(bedId, out bed_index))
			{
				return (bed_index + 4).ToString();
			}
			if (bedId == "BluePinstripeBed")
			{
				return "BluePinstripeDoubleBed";
			}
			return BedFurniture.DOUBLE_BED_INDEX;
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x0010ADC8 File Offset: 0x00108FC8
		public static int getDirectionFromChange(Vector2 current, Vector2 previous)
		{
			if (current.X > previous.X)
			{
				return 1;
			}
			if (current.X < previous.X)
			{
				return 3;
			}
			if (current.Y > previous.Y)
			{
				return 2;
			}
			if (current.Y < previous.Y)
			{
				return 0;
			}
			return -1;
		}

		// Token: 0x060016E4 RID: 5860 RVA: 0x0010AE18 File Offset: 0x00109018
		public static bool doesRectangleIntersectTile(Microsoft.Xna.Framework.Rectangle r, int tileX, int tileY)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
			return r.Intersects(tileRect);
		}

		// Token: 0x060016E5 RID: 5861 RVA: 0x0010AE40 File Offset: 0x00109040
		public static bool IsHospitalVisitDay(string character_name)
		{
			try
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + character_name);
				string day_key = Game1.currentSeason + "_" + Game1.dayOfMonth.ToString();
				string scheduleScript;
				if (dictionary.TryGetValue(day_key, out scheduleScript) && scheduleScript.Contains("Hospital"))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		// Token: 0x060016E6 RID: 5862 RVA: 0x0010AEB0 File Offset: 0x001090B0
		public static List<NPC> getAllCharacters()
		{
			List<NPC> list = new List<NPC>();
			Utility.ForEachCharacter(delegate(NPC npc)
			{
				list.Add(npc);
				return true;
			}, false);
			return list;
		}

		// Token: 0x060016E7 RID: 5863 RVA: 0x0010AED9 File Offset: 0x001090D9
		public static List<NPC> getAllVillagers()
		{
			List<NPC> list = new List<NPC>();
			Utility.ForEachVillager(delegate(NPC npc)
			{
				list.Add(npc);
				return true;
			}, false);
			return list;
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x0010AF04 File Offset: 0x00109104
		public static Item PerformSpecialItemPlaceReplacement(Item placedItem)
		{
			string a = (placedItem != null) ? placedItem.QualifiedItemId : null;
			Item newItem;
			if (!(a == "(T)Pan"))
			{
				if (!(a == "(T)SteelPan"))
				{
					if (!(a == "(T)GoldPan"))
					{
						if (!(a == "(T)IridiumPan"))
						{
							if (!(a == "(O)71"))
							{
								return placedItem;
							}
							newItem = ItemRegistry.Create("(P)15", 1, 0, false);
						}
						else
						{
							newItem = ItemRegistry.Create("(H)IridiumPanHat", 1, 0, false);
						}
					}
					else
					{
						newItem = ItemRegistry.Create("(H)GoldPanHat", 1, 0, false);
					}
				}
				else
				{
					newItem = ItemRegistry.Create("(H)SteelPanHat", 1, 0, false);
				}
			}
			else
			{
				newItem = ItemRegistry.Create("(H)71", 1, 0, false);
			}
			newItem.modData.CopyFrom(placedItem.modData);
			Hat newHat = newItem as Hat;
			if (newHat != null)
			{
				Tool fromTool = placedItem as Tool;
				if (fromTool != null)
				{
					newHat.enchantments.AddRange(fromTool.enchantments);
					newHat.previousEnchantments.AddRange(fromTool.previousEnchantments);
				}
			}
			return newItem;
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x0010AFFC File Offset: 0x001091FC
		public static Item PerformSpecialItemGrabReplacement(Item heldItem)
		{
			string a = (heldItem != null) ? heldItem.QualifiedItemId : null;
			Item newItem;
			if (!(a == "(P)15"))
			{
				if (!(a == "(H)71"))
				{
					if (!(a == "(H)SteelPanHat"))
					{
						if (!(a == "(H)GoldPanHat"))
						{
							if (!(a == "(H)IridiumPanHat"))
							{
								return heldItem;
							}
							newItem = ItemRegistry.Create("(T)IridiumPan", 1, 0, false);
						}
						else
						{
							newItem = ItemRegistry.Create("(T)GoldPan", 1, 0, false);
						}
					}
					else
					{
						newItem = ItemRegistry.Create("(T)SteelPan", 1, 0, false);
					}
				}
				else
				{
					newItem = ItemRegistry.Create("(T)Pan", 1, 0, false);
				}
			}
			else
			{
				Object @object = ItemRegistry.Create<Object>("(O)71", 1, 0, false);
				@object.questItem.Value = true;
				@object.questId.Value = "102";
				newItem = @object;
			}
			newItem.modData.CopyFrom(heldItem.modData);
			Pan newPan = newItem as Pan;
			if (newPan != null)
			{
				Hat fromHat = heldItem as Hat;
				if (fromHat != null)
				{
					newPan.enchantments.AddRange(fromHat.enchantments);
					newPan.previousEnchantments.AddRange(fromHat.previousEnchantments);
				}
			}
			return newItem;
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x0010B110 File Offset: 0x00109310
		public static void iterateChestsAndStorage(Action<Item> action)
		{
			Utility.ForEachLocation(delegate(GameLocation l)
			{
				Chest fridge = l.GetFridge(false);
				if (fridge != null)
				{
					fridge.ForEachItem(new ForEachItemDelegate(base.<iterateChestsAndStorage>g__Handle|0), null);
				}
				foreach (Object o in l.objects.Values)
				{
					if (o != fridge)
					{
						if (o is Chest)
						{
							o.ForEachItem(new ForEachItemDelegate(base.<iterateChestsAndStorage>g__Handle|0), null);
						}
						else
						{
							Chest heldChest = o.heldObject.Value as Chest;
							if (heldChest != null)
							{
								heldChest.ForEachItem(new ForEachItemDelegate(base.<iterateChestsAndStorage>g__Handle|0), null);
							}
						}
					}
				}
				foreach (Furniture furniture in l.furniture)
				{
					furniture.ForEachItem(new ForEachItemDelegate(base.<iterateChestsAndStorage>g__Handle|0), null);
				}
				foreach (Building building in l.buildings)
				{
					foreach (Chest chest in building.buildingChests)
					{
						chest.ForEachItem(new ForEachItemDelegate(base.<iterateChestsAndStorage>g__Handle|0), null);
					}
				}
				return true;
			}, true, false);
			foreach (Item item in Game1.player.team.returnedDonations)
			{
				if (item != null)
				{
					action(item);
				}
			}
			foreach (Inventory inventory in Game1.player.team.globalInventories.Values)
			{
				foreach (Item item2 in ((IEnumerable<Item>)inventory))
				{
					if (item2 != null)
					{
						action(item2);
					}
				}
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (Item item3 in specialOrder.donatedItems)
				{
					if (item3 != null)
					{
						action(item3);
					}
				}
			}
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x0010B2B8 File Offset: 0x001094B8
		public static Item removeItemFromInventory(int whichItemIndex, IList<Item> items)
		{
			if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
			{
				Item tmp = items[whichItemIndex];
				if (whichItemIndex == Game1.player.CurrentToolIndex && items.Equals(Game1.player.Items) && tmp != null)
				{
					tmp.actionWhenStopBeingHeld(Game1.player);
				}
				items[whichItemIndex] = null;
				return tmp;
			}
			return null;
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x0010B31B File Offset: 0x0010951B
		public static NPC getRandomTownNPC(Random random = null)
		{
			return Utility.getRandomNpcFromHomeRegion("Town", random);
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x0010B328 File Offset: 0x00109528
		public static NPC getRandomNpcFromHomeRegion(string region, Random random = null)
		{
			return Utility.GetRandomNpc((string name, CharacterData data) => data.HomeRegion == region, random, true);
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x0010B348 File Offset: 0x00109548
		public static NPC GetRandomWinterStarParticipant(Func<string, bool> ignoreNpc = null)
		{
			return Utility.GetRandomNpc(delegate(string name, CharacterData data)
			{
				Func<string, bool> ignoreNpc2 = ignoreNpc;
				if (ignoreNpc2 != null && ignoreNpc2(name))
				{
					return false;
				}
				if (data.WinterStarParticipant == null)
				{
					return data.HomeRegion == "Town";
				}
				return GameStateQuery.CheckConditions(data.WinterStarParticipant, null, null, null, null, null, null);
			}, Utility.CreateRandom(Game1.uniqueIDForThisGame / 2UL, (double)Game1.year, (double)Game1.player.UniqueMultiplayerID, 0.0, 0.0), true);
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x0010B3A4 File Offset: 0x001095A4
		public static NPC GetRandomNpc(Func<string, CharacterData, bool> match = null, Random random = null, bool mustBeSocial = true)
		{
			List<string> npcNames = new List<string>();
			foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
			{
				if (match == null || match(entry.Key, entry.Value))
				{
					npcNames.Add(entry.Key);
				}
			}
			random = (random ?? Game1.random);
			while (npcNames.Count > 0)
			{
				int index = random.Next(npcNames.Count);
				NPC npc = Game1.getCharacterFromName(npcNames[index], true, false);
				if (npc != null && (!mustBeSocial || npc.CanSocialize))
				{
					return npc;
				}
				npcNames.RemoveAt(index);
			}
			return null;
		}

		// Token: 0x060016F0 RID: 5872 RVA: 0x0010B464 File Offset: 0x00109664
		public static bool foundAllStardrops(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			return who.mailReceived.Contains("gotMaxStamina") || (who.hasOrWillReceiveMail("CF_Fair") && who.hasOrWillReceiveMail("CF_Fish") && (who.hasOrWillReceiveMail("CF_Mines") || who.chestConsumedMineLevels.GetValueOrDefault(100, false)) && who.hasOrWillReceiveMail("CF_Sewer") && who.hasOrWillReceiveMail("museumComplete") && who.hasOrWillReceiveMail("CF_Spouse") && who.hasOrWillReceiveMail("CF_Statue"));
		}

		// Token: 0x060016F1 RID: 5873 RVA: 0x0010B4FC File Offset: 0x001096FC
		public static int numStardropsFound(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			int num = 0;
			if (who.hasOrWillReceiveMail("CF_Fair"))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("CF_Fish"))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("CF_Mines") || who.chestConsumedMineLevels.GetValueOrDefault(100, false))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("CF_Sewer"))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("museumComplete"))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("CF_Spouse"))
			{
				num++;
			}
			if (who.hasOrWillReceiveMail("CF_Statue"))
			{
				num++;
			}
			return num;
		}

		// Token: 0x060016F2 RID: 5874 RVA: 0x0010B5A0 File Offset: 0x001097A0
		public static int getGrandpaScore()
		{
			int points = 0;
			if (Game1.player.totalMoneyEarned >= 50000U)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 100000U)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 200000U)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 300000U)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 500000U)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 1000000U)
			{
				points += 2;
			}
			if (Game1.player.achievements.Contains(5))
			{
				points++;
			}
			if (Game1.player.hasSkullKey)
			{
				points++;
			}
			bool flag = Game1.isLocationAccessible("CommunityCenter");
			if (flag || Game1.player.hasCompletedCommunityCenter())
			{
				points++;
			}
			if (flag)
			{
				points += 2;
			}
			if (Game1.player.isMarriedOrRoommates() && Utility.getHomeOfFarmer(Game1.player).upgradeLevel >= 2)
			{
				points++;
			}
			if (Game1.player.hasRustyKey)
			{
				points++;
			}
			if (Game1.player.achievements.Contains(26))
			{
				points++;
			}
			if (Game1.player.achievements.Contains(34))
			{
				points++;
			}
			int numberOfFriendsWithinThisRange = Utility.getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999, false);
			if (numberOfFriendsWithinThisRange >= 5)
			{
				points++;
			}
			if (numberOfFriendsWithinThisRange >= 10)
			{
				points++;
			}
			int level = Game1.player.Level;
			if (level >= 15)
			{
				points++;
			}
			if (level >= 25)
			{
				points++;
			}
			if (Game1.player.mailReceived.Contains("petLoveMessage"))
			{
				points++;
			}
			return points;
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x0010B731 File Offset: 0x00109931
		public static int getGrandpaCandlesFromScore(int score)
		{
			if (score >= 12)
			{
				return 4;
			}
			if (score >= 8)
			{
				return 3;
			}
			if (score >= 4)
			{
				return 2;
			}
			return 1;
		}

		// Token: 0x060016F4 RID: 5876 RVA: 0x0010B748 File Offset: 0x00109948
		public static bool canItemBeAddedToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			if (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				return true;
			}
			int stack = i.Stack;
			foreach (Item slot in list)
			{
				if (slot == null)
				{
					return true;
				}
				if (slot.canStackWith(i) && slot.getRemainingStackSpace() > 0)
				{
					stack -= slot.getRemainingStackSpace();
					if (stack <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060016F5 RID: 5877 RVA: 0x0010B7CC File Offset: 0x001099CC
		public static bool TryParseDirection(string direction, out int parsed)
		{
			if (string.IsNullOrWhiteSpace(direction))
			{
				parsed = -1;
				return false;
			}
			if (direction.EqualsIgnoreCase("up"))
			{
				parsed = 0;
				return true;
			}
			if (direction.EqualsIgnoreCase("down"))
			{
				parsed = 2;
				return true;
			}
			if (direction.EqualsIgnoreCase("left"))
			{
				parsed = 3;
				return true;
			}
			if (direction.EqualsIgnoreCase("right"))
			{
				parsed = 1;
				return true;
			}
			if (int.TryParse(direction, out parsed))
			{
				int num = parsed;
				if (num <= 3)
				{
					return true;
				}
			}
			parsed = -1;
			return false;
		}

		// Token: 0x060016F6 RID: 5878 RVA: 0x0010B844 File Offset: 0x00109A44
		public static int GetNumberOfItemThatCanBeAddedToThisInventoryList(Item item, IList<Item> list, int listMaxItems)
		{
			int addableStacks = 0;
			foreach (Item existingStack in list)
			{
				if (existingStack == null)
				{
					addableStacks += item.maximumStackSize();
				}
				else if (existingStack != null && existingStack.canStackWith(item) && existingStack.getRemainingStackSpace() > 0)
				{
					addableStacks += existingStack.getRemainingStackSpace();
				}
			}
			for (int i = 0; i < listMaxItems - list.Count; i++)
			{
				addableStacks += item.maximumStackSize();
			}
			return addableStacks;
		}

		// Token: 0x060016F7 RID: 5879 RVA: 0x0010B8D0 File Offset: 0x00109AD0
		public static Item addItemToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			i.FixStackSize();
			foreach (Item slot in list)
			{
				if (slot != null && slot.canStackWith(i) && slot.getRemainingStackSpace() > 0)
				{
					int toRemove = i.Stack - slot.addToStack(i);
					if (i.ConsumeStack(toRemove) == null)
					{
						return null;
					}
				}
			}
			for (int j = list.Count - 1; j >= 0; j--)
			{
				if (list[j] == null)
				{
					if (i.Stack <= i.maximumStackSize())
					{
						list[j] = i;
						return null;
					}
					list[j] = i.getOne();
					list[j].Stack = i.maximumStackSize();
					Object obj = i as Object;
					if (obj != null)
					{
						obj.stack.Value -= i.maximumStackSize();
					}
					else
					{
						i.Stack -= i.maximumStackSize();
					}
				}
			}
			while (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				if (i.Stack <= i.maximumStackSize())
				{
					list.Add(i);
					return null;
				}
				Item tmp = i.getOne();
				tmp.Stack = i.maximumStackSize();
				Object obj2 = i as Object;
				if (obj2 != null)
				{
					obj2.stack.Value -= i.maximumStackSize();
				}
				else
				{
					i.Stack -= i.maximumStackSize();
				}
				list.Add(tmp);
			}
			return i;
		}

		// Token: 0x060016F8 RID: 5880 RVA: 0x0010BA6C File Offset: 0x00109C6C
		public static Item addItemToInventory(Item item, int position, IList<Item> items, ItemGrabMenu.behaviorOnItemSelect onAddFunction = null)
		{
			bool isCurrentPlayer = items.Equals(Game1.player.Items);
			if (isCurrentPlayer)
			{
				bool needsInventorySpace;
				bool flag;
				Game1.player.GetItemReceiveBehavior(item, out needsInventorySpace, out flag);
				if (!needsInventorySpace)
				{
					Game1.player.OnItemReceived(item, item.Stack, null, false);
					return null;
				}
			}
			if (position < 0 || position >= items.Count)
			{
				return item;
			}
			if (items[position] == null)
			{
				items[position] = item;
				if (isCurrentPlayer)
				{
					Game1.player.OnItemReceived(item, item.Stack, null, false);
				}
				if (onAddFunction != null)
				{
					onAddFunction(item, null);
				}
				return null;
			}
			if (!item.canStackWith(items[position]))
			{
				Item tmp = items[position];
				if (position == Game1.player.CurrentToolIndex && items.Equals(Game1.player.Items) && tmp != null)
				{
					tmp.actionWhenStopBeingHeld(Game1.player);
					item.actionWhenBeingHeld(Game1.player);
				}
				items[position] = item;
				if (isCurrentPlayer)
				{
					Game1.player.OnItemReceived(item, item.Stack, null, false);
				}
				if (onAddFunction != null)
				{
					onAddFunction(item, null);
				}
				return tmp;
			}
			int originalStack = item.Stack;
			int stackLeft = items[position].addToStack(item);
			if (isCurrentPlayer)
			{
				Game1.player.OnItemReceived(item, originalStack - stackLeft, items[position], false);
			}
			if (stackLeft <= 0)
			{
				return null;
			}
			item.Stack = stackLeft;
			if (onAddFunction != null)
			{
				onAddFunction(item, null);
			}
			return item;
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x0010BBC8 File Offset: 0x00109DC8
		public static bool trySpawnRareObject(Farmer who, Vector2 position, GameLocation location, double chanceModifier = 1.0, double dailyLuckWeight = 1.0, int groundLevel = -1, Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			double luckMod = 1.0;
			if (who != null)
			{
				luckMod = 1.0 + who.team.AverageDailyLuck(null) * dailyLuckWeight;
			}
			bool result = false;
			if (who != null && who.stats.Get(StatKeys.Mastery(0)) > 0U && random.NextDouble() < 0.001 * chanceModifier * luckMod)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)GoldenAnimalCracker", 1, 0, false), position, -1, location, groundLevel, false);
			}
			if (Game1.stats.DaysPlayed > 2U && random.NextDouble() < 0.002 * chanceModifier)
			{
				Game1.createItemDebris(Utility.getRandomCosmeticItem(random), position, -1, location, groundLevel, false);
			}
			if (Game1.stats.DaysPlayed > 2U && random.NextDouble() < 0.0006 * chanceModifier)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)SkillBook_" + random.Next(5).ToString(), 1, 0, false), position, -1, location, groundLevel, false);
			}
			return result;
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x0010BCD4 File Offset: 0x00109ED4
		public static bool spawnObjectAround(Vector2 tileLocation, Object o, GameLocation l, bool playSound = true, Action<Object> modifyObject = null)
		{
			if (o == null || l == null || tileLocation.Equals(Vector2.Zero))
			{
				return false;
			}
			int attempts = 0;
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			openList.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			while (attempts < 100)
			{
				current = openList.Dequeue();
				if (l.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					break;
				}
				closedList.Add(current);
				foreach (Vector2 v in (from a in Utility.getAdjacentTileLocations(current)
				orderby Guid.NewGuid()
				select a).ToArray<Vector2>())
				{
					if (!closedList.Contains(v))
					{
						openList.Enqueue(v);
					}
				}
				attempts++;
			}
			o.isSpawnedObject.Value = true;
			o.canBeGrabbed.Value = true;
			o.TileLocation = current;
			if (modifyObject != null)
			{
				modifyObject(o);
			}
			if (!current.Equals(Vector2.Zero) && l.CanItemBePlacedHere(current, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
			{
				l.objects.Add(current, o);
				if (playSound)
				{
					l.playSound("coin", null, null, SoundContext.Default);
				}
				if (l.Equals(Game1.currentLocation))
				{
					l.temporarySprites.Add(new TemporaryAnimatedSprite(5, current * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0));
				}
				return true;
			}
			return false;
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x0010BE6C File Offset: 0x0010A06C
		public static bool IsGeode(Item item, bool disallow_special_geodes = false)
		{
			if (!item.HasTypeObject() || (disallow_special_geodes && item.HasContextTag("geode_crusher_ignored")))
			{
				return false;
			}
			if (item.QualifiedItemId.Contains("MysteryBox"))
			{
				return true;
			}
			ObjectData data;
			if (!Game1.objectData.TryGetValue(item.ItemId, out data))
			{
				return false;
			}
			if (!data.GeodeDropsDefaultItems)
			{
				List<ObjectGeodeDropData> geodeDrops = data.GeodeDrops;
				return geodeDrops != null && geodeDrops.Count > 0;
			}
			return true;
		}

		// Token: 0x060016FC RID: 5884 RVA: 0x0010BEDC File Offset: 0x0010A0DC
		public static Item getRandomCosmeticItem(Random r)
		{
			if (r.NextDouble() < 0.2)
			{
				if (r.NextDouble() < 0.05)
				{
					return ItemRegistry.Create("(F)1369", 1, 0, false);
				}
				Item item = null;
				switch (r.Next(3))
				{
				case 0:
					item = ItemRegistry.Create(Utility.getRandomSingleTileFurniture(r), 1, 0, false);
					break;
				case 1:
					item = ItemRegistry.Create("(F)" + r.Next(1362, 1370).ToString(), 1, 0, false);
					break;
				case 2:
					item = ItemRegistry.Create("(F)" + r.Next(1376, 1391).ToString(), 1, 0, false);
					break;
				}
				if (item == null || item.Name.Contains("Error"))
				{
					item = ItemRegistry.Create("(F)1369", 1, 0, false);
				}
				return item;
			}
			else
			{
				if (r.NextDouble() < 0.25)
				{
					List<string> hats = new List<string>
					{
						"(H)45",
						"(H)46",
						"(H)47",
						"(H)49",
						"(H)52",
						"(H)53",
						"(H)54",
						"(H)55",
						"(H)57",
						"(H)58",
						"(H)59",
						"(H)62",
						"(H)63",
						"(H)68",
						"(H)69",
						"(H)70",
						"(H)84",
						"(H)85",
						"(H)87",
						"(H)88",
						"(H)89",
						"(H)90"
					};
					return ItemRegistry.Create(hats[r.Next(hats.Count)], 1, 0, false);
				}
				return ItemRegistry.Create("(S)" + Utility.getRandomIntWithExceptions(r, 1112, 1291, new List<int>
				{
					1038,
					1041,
					1129,
					1130,
					1132,
					1133,
					1136,
					1152,
					1176,
					1177,
					1201,
					1202,
					1127
				}).ToString(), 1, 0, false);
			}
		}

		// Token: 0x060016FD RID: 5885 RVA: 0x0010C1B0 File Offset: 0x0010A3B0
		public static int getRandomIntWithExceptions(Random r, int minValue, int maxValueExclusive, List<int> exceptions)
		{
			if (r == null)
			{
				r = Game1.random;
			}
			int value = r.Next(minValue, maxValueExclusive);
			while (exceptions != null && exceptions.Contains(value))
			{
				value = r.Next(minValue, maxValueExclusive);
			}
			return value;
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x0010C1E8 File Offset: 0x0010A3E8
		public static bool tryRollMysteryBox(double baseChance, Random r = null)
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("sawQiPlane"))
			{
				return false;
			}
			if (r == null)
			{
				r = Game1.random;
			}
			if (Game1.player.stats.Get("Book_Mystery") > 0U)
			{
				baseChance *= 0.88;
			}
			else
			{
				baseChance *= 0.66;
			}
			return r.NextDouble() < baseChance;
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x0010C254 File Offset: 0x0010A454
		public static Item getTreasureFromGeode(Item geode)
		{
			if (!Utility.IsGeode(geode, false))
			{
				return null;
			}
			try
			{
				string geodeId = geode.QualifiedItemId;
				Random r = Utility.CreateRandom(geodeId.Contains("MysteryBox") ? Game1.stats.Get("MysteryBoxesOpened") : Game1.stats.GeodesCracked, Game1.uniqueIDForThisGame / 2UL, (double)((int)Game1.player.uniqueMultiplayerID.Value / 2), 0.0, 0.0);
				int prewarm_amount = r.Next(1, 10);
				for (int i = 0; i < prewarm_amount; i++)
				{
					r.NextDouble();
				}
				prewarm_amount = r.Next(1, 10);
				for (int j = 0; j < prewarm_amount; j++)
				{
					r.NextDouble();
				}
				if (geodeId.Contains("MysteryBox"))
				{
					if (Game1.stats.Get("MysteryBoxesOpened") > 10U || geodeId == "(O)GoldenMysteryBox")
					{
						double rareMod = (double)((geodeId == "(O)GoldenMysteryBox") ? 2 : 1);
						if (geodeId == "(O)GoldenMysteryBox")
						{
							if (Game1.player.stats.Get(StatKeys.Mastery(0)) > 0U && r.NextBool(0.005))
							{
								return ItemRegistry.Create("(O)GoldenAnimalCracker", 1, 0, false);
							}
							if (r.NextBool(0.005))
							{
								return ItemRegistry.Create("(BC)272", 1, 0, false);
							}
						}
						if (r.NextBool(0.002 * rareMod))
						{
							return ItemRegistry.Create("(O)279", 1, 0, false);
						}
						if (r.NextBool(0.004 * rareMod))
						{
							return ItemRegistry.Create("(O)74", 1, 0, false);
						}
						if (r.NextBool(0.008 * rareMod))
						{
							return ItemRegistry.Create("(O)166", 1, 0, false);
						}
						if (r.NextBool(0.01 * rareMod + (Game1.player.mailReceived.Contains("GotMysteryBook") ? 0.0 : (0.0004 * Game1.stats.Get("MysteryBoxesOpened")))))
						{
							if (!Game1.player.mailReceived.Contains("GotMysteryBook"))
							{
								Game1.player.mailReceived.Add("GotMysteryBook");
								return ItemRegistry.Create("(O)Book_Mystery", 1, 0, false);
							}
							return ItemRegistry.Create(r.Choose("(O)PurpleBook", "(O)Book_Mystery"), 1, 0, false);
						}
						else
						{
							if (r.NextBool(0.01 * rareMod))
							{
								return ItemRegistry.Create(r.Choose("(O)797", "(O)373"), 1, 0, false);
							}
							if (r.NextBool(0.01 * rareMod))
							{
								return ItemRegistry.Create("(H)MysteryHat", 1, 0, false);
							}
							if (r.NextBool(0.01 * rareMod))
							{
								return ItemRegistry.Create("(S)MysteryShirt", 1, 0, false);
							}
							if (r.NextBool(0.01 * rareMod))
							{
								return ItemRegistry.Create("(WP)MoreWalls:11", 1, 0, false);
							}
							if (r.NextBool(0.1) || geodeId == "(O)GoldenMysteryBox")
							{
								switch (r.Next(15))
								{
								case 0:
									return ItemRegistry.Create("(O)288", 5, 0, false);
								case 1:
									return ItemRegistry.Create("(O)253", 3, 0, false);
								case 2:
									if (Game1.player.GetUnmodifiedSkillLevel(1) >= 6 && r.NextBool())
									{
										return ItemRegistry.Create(r.Choose("(O)687", "(O)695"), 1, 0, false);
									}
									return ItemRegistry.Create("(O)242", 2, 0, false);
								case 3:
									return ItemRegistry.Create("(O)204", 2, 0, false);
								case 4:
									return ItemRegistry.Create("(O)369", 20, 0, false);
								case 5:
									return ItemRegistry.Create("(O)466", 20, 0, false);
								case 6:
									return ItemRegistry.Create("(O)773", 2, 0, false);
								case 7:
									return ItemRegistry.Create("(O)688", 3, 0, false);
								case 8:
									return ItemRegistry.Create("(O)" + r.Next(628, 634).ToString(), 1, 0, false);
								case 9:
									return ItemRegistry.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 20, 0, false);
								case 10:
									if (r.NextBool())
									{
										return ItemRegistry.Create("(W)60", 1, 0, false);
									}
									return ItemRegistry.Create(r.Choose("(O)533", "(O)534"), 1, 0, false);
								case 11:
									return ItemRegistry.Create("(O)621", 1, 0, false);
								case 12:
									return ItemRegistry.Create("(O)MysteryBox", r.Next(3, 5), 0, false);
								case 13:
									return ItemRegistry.Create("(O)SkillBook_" + r.Next(5).ToString(), 1, 0, false);
								case 14:
									return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, r, 8);
								}
							}
						}
					}
					switch (r.Next(14))
					{
					case 0:
						return ItemRegistry.Create("(O)395", 3, 0, false);
					case 1:
						return ItemRegistry.Create("(O)287", 5, 0, false);
					case 2:
						return ItemRegistry.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 8, 0, false);
					case 3:
						return ItemRegistry.Create("(O)" + r.Next(727, 734).ToString(), 1, 0, false);
					case 4:
						return ItemRegistry.Create("(O)" + Utility.getRandomIntWithExceptions(r, 194, 240, new List<int>
						{
							217
						}).ToString(), 1, 0, false);
					case 5:
						return ItemRegistry.Create("(O)709", 10, 0, false);
					case 6:
						return ItemRegistry.Create("(O)369", 10, 0, false);
					case 7:
						return ItemRegistry.Create("(O)466", 10, 0, false);
					case 8:
						return ItemRegistry.Create("(O)688", 1, 0, false);
					case 9:
						return ItemRegistry.Create("(O)689", 1, 0, false);
					case 10:
						return ItemRegistry.Create("(O)770", 10, 0, false);
					case 11:
						return ItemRegistry.Create("(O)MixedFlowerSeeds", 10, 0, false);
					case 12:
						if (!r.NextBool(0.4))
						{
							return ItemRegistry.Create("(O)MysteryBox", 2, 0, false);
						}
						switch (r.Next(4))
						{
						case 0:
							return ItemRegistry.Create<Ring>("(O)525", 1, 0, false);
						case 1:
							return ItemRegistry.Create<Ring>("(O)529", 1, 0, false);
						case 2:
							return ItemRegistry.Create<Ring>("(O)888", 1, 0, false);
						default:
							return ItemRegistry.Create<Ring>("(O)" + r.Next(531, 533).ToString(), 1, 0, false);
						}
						break;
					case 13:
						return ItemRegistry.Create("(O)690", 1, 0, false);
					default:
						return ItemRegistry.Create("(O)382", 1, 0, false);
					}
				}
				else
				{
					if (r.NextBool(0.1) && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
					{
						return ItemRegistry.Create("(O)890", r.NextBool(0.25) ? 5 : 1, 0, false);
					}
					ObjectData data;
					if (Game1.objectData.TryGetValue(geode.ItemId, out data))
					{
						List<ObjectGeodeDropData> geodeDrops = data.GeodeDrops;
						if (geodeDrops != null && geodeDrops.Count > 0 && (!data.GeodeDropsDefaultItems || r.NextBool()))
						{
							using (IEnumerator<ObjectGeodeDropData> enumerator = (from p in data.GeodeDrops
							orderby p.Precedence
							select p).GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									ObjectGeodeDropData drop = enumerator.Current;
									if (r.NextBool(drop.Chance) && (drop.Condition == null || GameStateQuery.CheckConditions(drop.Condition, null, null, null, null, r, null)))
									{
										ISpawnItemData drop2 = drop;
										GameLocation location = null;
										Farmer player = null;
										Random random = r;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 2);
										defaultInterpolatedStringHandler.AppendLiteral("object '");
										defaultInterpolatedStringHandler.AppendFormatted(geode.ItemId);
										defaultInterpolatedStringHandler.AppendLiteral("' > geode drop '");
										defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
										defaultInterpolatedStringHandler.AppendLiteral("'");
										Item item = ItemQueryResolver.TryResolveRandomItem(drop2, new ItemQueryContext(location, player, random, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, delegate(string query, string error)
										{
											IGameLogger log2 = Game1.log;
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 5);
											defaultInterpolatedStringHandler2.AppendLiteral("Geode item '");
											defaultInterpolatedStringHandler2.AppendFormatted(geode.QualifiedItemId);
											defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
											defaultInterpolatedStringHandler2.AppendFormatted(query);
											defaultInterpolatedStringHandler2.AppendLiteral("' for ");
											defaultInterpolatedStringHandler2.AppendFormatted("GeodeDrops");
											defaultInterpolatedStringHandler2.AppendLiteral(" entry '");
											defaultInterpolatedStringHandler2.AppendFormatted(drop.Id);
											defaultInterpolatedStringHandler2.AppendLiteral("': ");
											defaultInterpolatedStringHandler2.AppendFormatted(error);
											log2.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
										});
										if (item != null)
										{
											if (drop.SetFlagOnPickup != null)
											{
												item.SetFlagOnPickup = drop.SetFlagOnPickup;
											}
											return item;
										}
									}
								}
							}
						}
					}
					int amount = r.Next(3) * 2 + 1;
					if (r.NextBool(0.1))
					{
						amount = 10;
					}
					if (r.NextBool(0.01))
					{
						amount = 20;
					}
					if (r.NextBool())
					{
						int num = r.Next(4);
						if (num <= 1)
						{
							return ItemRegistry.Create("(O)390", amount, 0, false);
						}
						if (num == 2)
						{
							return ItemRegistry.Create("(O)330", 1, 0, false);
						}
						if (geodeId == "(O)749")
						{
							return ItemRegistry.Create("(O)" + (82 + r.Next(3) * 2).ToString(), 1, 0, false);
						}
						if (geodeId == "(O)535")
						{
							return ItemRegistry.Create("(O)86", 1, 0, false);
						}
						if (!(geodeId == "(O)536"))
						{
							return ItemRegistry.Create("(O)82", 1, 0, false);
						}
						return ItemRegistry.Create("(O)84", 1, 0, false);
					}
					else if (!(geodeId == "(O)535"))
					{
						if (!(geodeId == "(O)536"))
						{
							switch (r.Next(5))
							{
							case 0:
								return ItemRegistry.Create("(O)378", amount, 0, false);
							case 1:
								return ItemRegistry.Create("(O)380", amount, 0, false);
							case 2:
								return ItemRegistry.Create("(O)382", amount, 0, false);
							case 3:
								return ItemRegistry.Create("(O)384", amount, 0, false);
							default:
								return ItemRegistry.Create("(O)386", amount / 2 + 1, 0, false);
							}
						}
						else
						{
							switch (r.Next(4))
							{
							case 0:
								return ItemRegistry.Create("(O)378", amount, 0, false);
							case 1:
								return ItemRegistry.Create("(O)380", amount, 0, false);
							case 2:
								return ItemRegistry.Create("(O)382", amount, 0, false);
							default:
								return ItemRegistry.Create((Game1.player.deepestMineLevel > 75) ? "(O)384" : "(O)380", amount, 0, false);
							}
						}
					}
					else
					{
						int num = r.Next(3);
						if (num == 0)
						{
							return ItemRegistry.Create("(O)378", amount, 0, false);
						}
						if (num != 1)
						{
							return ItemRegistry.Create("(O)382", amount, 0, false);
						}
						return ItemRegistry.Create((Game1.player.deepestMineLevel > 25) ? "(O)380" : "(O)378", amount, 0, false);
					}
				}
			}
			catch (Exception e)
			{
				IGameLogger log = Game1.log;
				string str = "Geode '";
				Item geode2 = geode;
				log.Error(str + ((geode2 != null) ? geode2.QualifiedItemId : null) + "' failed creating treasure.", e);
			}
			return ItemRegistry.Create("(O)390", 1, 0, false);
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x0010CF8C File Offset: 0x0010B18C
		public static Vector2 snapToInt(Vector2 v)
		{
			v.X = (float)((int)v.X);
			v.Y = (float)((int)v.Y);
			return v;
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x0010CFB0 File Offset: 0x0010B1B0
		public static Vector2 GetNearbyValidPlacementPosition(Farmer who, GameLocation location, Item item, int x, int y)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return new Vector2((float)x, (float)y);
			}
			int item_width = 1;
			int item_length = 1;
			Point direction = default(Point);
			Microsoft.Xna.Framework.Rectangle bounding_box = new Microsoft.Xna.Framework.Rectangle(0, 0, item_width * 64, item_length * 64);
			Furniture furniture = item as Furniture;
			if (furniture != null)
			{
				item_width = furniture.getTilesWide();
				item_length = furniture.getTilesHigh();
				bounding_box.Width = furniture.boundingBox.Value.Width;
				bounding_box.Height = furniture.boundingBox.Value.Height;
			}
			switch (who.FacingDirection)
			{
			case 0:
				direction.X = 0;
				direction.Y = -1;
				y -= (item_length - 1) * 64;
				break;
			case 1:
				direction.X = 1;
				direction.Y = 0;
				break;
			case 2:
				direction.X = 0;
				direction.Y = 1;
				break;
			case 3:
				direction.X = -1;
				direction.Y = 0;
				x -= (item_width - 1) * 64;
				break;
			}
			int scan_distance = 2;
			Object obj = item as Object;
			if (obj != null && obj.isPassable() && (obj.Category == -74 || obj.isSapling() || obj.Category == -19))
			{
				x = (int)who.GetToolLocation(false).X / 64 * 64;
				y = (int)who.GetToolLocation(false).Y / 64 * 64;
				direction.X = who.TilePoint.X - x / 64;
				direction.Y = who.TilePoint.Y - y / 64;
				int magnitude = (int)Math.Sqrt(Math.Pow((double)direction.X, 2.0) + Math.Pow((double)direction.Y, 2.0));
				if (magnitude > 0)
				{
					direction.X /= magnitude;
					direction.Y /= magnitude;
				}
				scan_distance = magnitude + 1;
			}
			Object @object = item as Object;
			bool is_passable = @object != null && @object.isPassable();
			x = x / 64 * 64;
			y = y / 64 * 64;
			Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
			for (int offset = 0; offset < scan_distance; offset++)
			{
				int checked_x = x + direction.X * offset * 64;
				int checked_y = y + direction.Y * offset * 64;
				bounding_box.X = checked_x;
				bounding_box.Y = checked_y;
				if ((!playerBounds.Intersects(bounding_box) && !is_passable) || Utility.playerCanPlaceItemHere(location, item, checked_x, checked_y, who, false))
				{
					return new Vector2((float)checked_x, (float)checked_y);
				}
			}
			return new Vector2((float)x, (float)y);
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x0010D24C File Offset: 0x0010B44C
		public static bool tryToPlaceItem(GameLocation location, Object item, int x, int y)
		{
			if (item == null)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
			if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player, false))
			{
				if (item is Furniture)
				{
					Game1.player.ActiveObject = null;
				}
				if (item.placementAction(location, x, y, Game1.player))
				{
					Game1.player.reduceActiveItemByOne();
				}
				else
				{
					Furniture furniture = item as Furniture;
					if (furniture != null)
					{
						Game1.player.ActiveObject = furniture;
					}
					else if (item is Wallpaper)
					{
						return false;
					}
				}
				return true;
			}
			if (Utility.isPlacementForbiddenHere(location) && item != null && item.isPlaceable())
			{
				if (Game1.didPlayerJustClickAtAll(true))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), true);
				}
			}
			else
			{
				Furniture furniture2 = item as Furniture;
				if (furniture2 != null && Game1.didPlayerJustLeftClick(true))
				{
					switch (furniture2.GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
					{
					case 1:
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12629"), true);
						break;
					case 2:
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"), true);
						break;
					case 3:
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12633"), true);
						break;
					case 4:
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"), true);
						break;
					}
				}
			}
			TerrainFeature terrainFeature;
			if (item.Category == -19 && location.terrainFeatures.TryGetValue(tileLocation, out terrainFeature))
			{
				HoeDirt dirt = terrainFeature as HoeDirt;
				if (dirt != null)
				{
					switch (dirt.CheckApplyFertilizerRules(item.QualifiedItemId))
					{
					case HoeDirtFertilizerApplyStatus.HasThisFertilizer:
						return false;
					case HoeDirtFertilizerApplyStatus.HasAnotherFertilizer:
						if (Game1.didPlayerJustClickAtAll(true))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"), true);
						}
						return false;
					case HoeDirtFertilizerApplyStatus.CropAlreadySprouted:
						if (Game1.didPlayerJustClickAtAll(true))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"), true);
						}
						return false;
					}
				}
			}
			Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player, true);
			return false;
		}

		// Token: 0x06001703 RID: 5891 RVA: 0x0010D444 File Offset: 0x0010B644
		public static bool pointInRectangles(List<Microsoft.Xna.Framework.Rectangle> rectangles, int x, int y)
		{
			foreach (Microsoft.Xna.Framework.Rectangle r in rectangles)
			{
				if (r.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001704 RID: 5892 RVA: 0x0010D4A0 File Offset: 0x0010B6A0
		public static Keys mapGamePadButtonToKey(Buttons b)
		{
			if (b <= Buttons.B)
			{
				if (b <= Buttons.Start)
				{
					switch (b)
					{
					case Buttons.DPadUp:
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton);
					case Buttons.DPadDown:
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton);
					case Buttons.DPadUp | Buttons.DPadDown:
						break;
					case Buttons.DPadLeft:
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton);
					default:
						if (b == Buttons.DPadRight)
						{
							return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton);
						}
						if (b == Buttons.Start)
						{
							return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
						}
						break;
					}
				}
				else
				{
					if (b == Buttons.Back)
					{
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton);
					}
					if (b == Buttons.A)
					{
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton);
					}
					if (b == Buttons.B)
					{
						return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
					}
				}
			}
			else if (b <= Buttons.LeftThumbstickLeft)
			{
				if (b == Buttons.X)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.useToolButton);
				}
				if (b == Buttons.Y)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
				}
				if (b == Buttons.LeftThumbstickLeft)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton);
				}
			}
			else
			{
				if (b == Buttons.LeftThumbstickUp)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton);
				}
				if (b == Buttons.LeftThumbstickDown)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton);
				}
				if (b == Buttons.LeftThumbstickRight)
				{
					return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton);
				}
			}
			return Keys.None;
		}

		// Token: 0x06001705 RID: 5893 RVA: 0x0010D67E File Offset: 0x0010B87E
		public static ButtonCollection getPressedButtons(GamePadState padState, GamePadState oldPadState)
		{
			return new ButtonCollection(ref padState, ref oldPadState);
		}

		// Token: 0x06001706 RID: 5894 RVA: 0x0010D68C File Offset: 0x0010B88C
		public static bool thumbstickIsInDirection(int direction, GamePadState padState)
		{
			if (Game1.currentMinigame != null)
			{
				return true;
			}
			switch (direction)
			{
			case 0:
				return Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y;
			case 1:
				return padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y);
			case 2:
				return Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y);
			case 3:
				return Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y);
			default:
				return false;
			}
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x0010D788 File Offset: 0x0010B988
		public static ButtonCollection getHeldButtons(GamePadState padState)
		{
			return new ButtonCollection(ref padState);
		}

		// Token: 0x06001708 RID: 5896 RVA: 0x0010D791 File Offset: 0x0010B991
		public static bool toggleMuteMusic()
		{
			if (Game1.options.musicVolumeLevel != 0f)
			{
				Utility.disableMusic();
				return true;
			}
			Utility.enableMusic();
			return false;
		}

		// Token: 0x06001709 RID: 5897 RVA: 0x0010D7B4 File Offset: 0x0010B9B4
		public static void enableMusic()
		{
			Game1.options.musicVolumeLevel = 0.75f;
			Game1.musicCategory.SetVolume(0.75f);
			Game1.musicPlayerVolume = 0.75f;
			Game1.options.ambientVolumeLevel = 0.75f;
			Game1.ambientCategory.SetVolume(0.75f);
			Game1.ambientPlayerVolume = 0.75f;
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x0010D814 File Offset: 0x0010BA14
		public static void disableMusic()
		{
			Game1.options.musicVolumeLevel = 0f;
			Game1.musicCategory.SetVolume(0f);
			Game1.options.ambientVolumeLevel = 0f;
			Game1.ambientCategory.SetVolume(0f);
			Game1.ambientPlayerVolume = 0f;
			Game1.musicPlayerVolume = 0f;
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x0010D874 File Offset: 0x0010BA74
		public static Vector2 getVelocityTowardPlayer(Point startingPoint, float speed, Farmer f)
		{
			Microsoft.Xna.Framework.Rectangle playerBounds = f.GetBoundingBox();
			return Utility.getVelocityTowardPoint(startingPoint, new Vector2((float)playerBounds.X, (float)playerBounds.Y), speed);
		}

		// Token: 0x0600170C RID: 5900 RVA: 0x0010D8A4 File Offset: 0x0010BAA4
		public static string getHoursMinutesStringFromMilliseconds(ulong milliseconds)
		{
			return (milliseconds / 3600000UL).ToString() + ":" + ((milliseconds % 3600000UL / 60000UL < 10UL) ? "0" : "") + (milliseconds % 3600000UL / 60000UL).ToString();
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x0010D904 File Offset: 0x0010BB04
		public static string getMinutesSecondsStringFromMilliseconds(int milliseconds)
		{
			return (milliseconds / 60000).ToString() + ":" + ((milliseconds % 60000 / 1000 < 10) ? "0" : "") + (milliseconds % 60000 / 1000).ToString();
		}

		// Token: 0x0600170E RID: 5902 RVA: 0x0010D95C File Offset: 0x0010BB5C
		public static Vector2 getVelocityTowardPoint(Vector2 startingPoint, Vector2 endingPoint, float speed)
		{
			double xDif = (double)(endingPoint.X - startingPoint.X);
			double yDif = (double)(endingPoint.Y - startingPoint.Y);
			if (Math.Abs(xDif) < 0.1 && Math.Abs(yDif) < 0.1)
			{
				return new Vector2(0f, 0f);
			}
			double total = Math.Sqrt(Math.Pow(xDif, 2.0) + Math.Pow(yDif, 2.0));
			xDif /= total;
			yDif /= total;
			return new Vector2((float)(xDif * (double)speed), (float)(yDif * (double)speed));
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x0010D9F5 File Offset: 0x0010BBF5
		public static Vector2 getVelocityTowardPoint(Point startingPoint, Vector2 endingPoint, float speed)
		{
			return Utility.getVelocityTowardPoint(new Vector2((float)startingPoint.X, (float)startingPoint.Y), endingPoint, speed);
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x0010DA11 File Offset: 0x0010BC11
		public static Vector2 getRandomPositionInThisRectangle(Microsoft.Xna.Framework.Rectangle r, Random random)
		{
			return new Vector2((float)random.Next(r.X, r.X + r.Width), (float)random.Next(r.Y, r.Y + r.Height));
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x0010DA4C File Offset: 0x0010BC4C
		public static Vector2 getTopLeftPositionForCenteringOnScreen(xTile.Dimensions.Rectangle viewport, int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return new Vector2((float)(viewport.Width / 2 - width / 2 + xOffset), (float)(viewport.Height / 2 - height / 2 + yOffset));
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x0010DA74 File Offset: 0x0010BC74
		public static Vector2 getTopLeftPositionForCenteringOnScreen(int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return Utility.getTopLeftPositionForCenteringOnScreen(Game1.uiViewport, width, height, xOffset, yOffset);
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x0010DA84 File Offset: 0x0010BC84
		public static void recursiveFindPositionForCharacter(NPC c, GameLocation l, Vector2 tileLocation, int maxIterations)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			Microsoft.Xna.Framework.Rectangle boundsSize = c.GetBoundingBox();
			while (iterations < maxIterations && positionsToCheck.Count > 0)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(boundsSize.Width / 2), currentPoint.Y * 64f - (float)boundsSize.Height);
				if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, false, 0, false, c, true, false, false, false))
				{
					if (!l.characters.Contains(c))
					{
						l.characters.Add(c);
						c.currentLocation = l;
					}
					return;
				}
				foreach (Vector2 v in Utility.DirectionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x0010DB98 File Offset: 0x0010BD98
		public static Pet findPet(Guid guid)
		{
			foreach (NPC npc in Game1.getFarm().characters)
			{
				Pet pet = npc as Pet;
				if (pet != null && pet.petId.Value.Equals(guid))
				{
					return pet;
				}
			}
			foreach (Farmer who in Game1.getAllFarmers())
			{
				foreach (NPC npc2 in Utility.getHomeOfFarmer(who).characters)
				{
					Pet pet2 = npc2 as Pet;
					if (pet2 != null && pet2.petId.Value.Equals(guid))
					{
						return pet2;
					}
				}
			}
			return null;
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x0010DCB0 File Offset: 0x0010BEB0
		public static Vector2 recursiveFindOpenTileForCharacter(Character c, GameLocation l, Vector2 tileLocation, int maxIterations, bool allowOffMap = true)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			Vector2 originalPosition = c.Position;
			int width = c.GetBoundingBox().Width;
			while (iterations < maxIterations && positionsToCheck.Count > 0)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(width / 2), currentPoint.Y * 64f + 4f);
				Microsoft.Xna.Framework.Rectangle boundingBox = c.GetBoundingBox();
				c.Position = originalPosition;
				if (!l.isCollidingPosition(boundingBox, Game1.viewport, c is Farmer, 0, false, c, false, false, false, true) && (allowOffMap || l.isTileOnMap(currentPoint)))
				{
					return currentPoint;
				}
				foreach (Vector2 v in Utility.DirectionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v) && l.isTilePlaceable(currentPoint + v, false) && (!(l is DecoratableLocation) || !(l as DecoratableLocation).isTileOnWall((int)(v.X + currentPoint.X), (int)(v.Y + currentPoint.Y))))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}
			return Vector2.Zero;
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x0010DE18 File Offset: 0x0010C018
		public static List<Vector2> recursiveFindOpenTiles(GameLocation l, Vector2 tileLocation, int maxOpenTilesToFind = 24, int maxIterations = 50)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			List<Vector2> successList = new List<Vector2>();
			while (iterations < maxIterations && positionsToCheck.Count > 0 && successList.Count < maxOpenTilesToFind)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				if (l.CanItemBePlacedHere(currentPoint, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					successList.Add(currentPoint);
				}
				foreach (Vector2 v in Utility.DirectionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}
			return successList;
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x0010DED4 File Offset: 0x0010C0D4
		public static void spreadAnimalsAround(Building b, GameLocation environment)
		{
			try
			{
				GameLocation indoors = b.GetIndoors();
				if (indoors != null)
				{
					Utility.spreadAnimalsAround(b, environment, indoors.animals.Values);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x0010DF18 File Offset: 0x0010C118
		public static void spreadAnimalsAround(Building b, GameLocation environment, IEnumerable<FarmAnimal> animalsList)
		{
			if (b.HasIndoors())
			{
				Queue<FarmAnimal> animals = new Queue<FarmAnimal>(animalsList);
				int iterations = 0;
				Queue<Vector2> positionsToCheck = new Queue<Vector2>();
				positionsToCheck.Enqueue(new Vector2((float)(b.tileX.Value + b.animalDoor.X), (float)(b.tileY.Value + b.animalDoor.Y + 1)));
				while (animals.Count > 0 && iterations < 40 && positionsToCheck.Count > 0)
				{
					Vector2 currentPoint = positionsToCheck.Dequeue();
					FarmAnimal animal = animals.Peek();
					Microsoft.Xna.Framework.Rectangle boundsSize = animal.GetBoundingBox();
					animal.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(boundsSize.Width / 2), currentPoint.Y * 64f - 32f - (float)(boundsSize.Height / 2));
					if (!environment.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, false, 0, false, animal, true, false, false, false))
					{
						environment.animals.Add(animal.myID.Value, animal);
						animals.Dequeue();
					}
					if (animals.Count > 0)
					{
						animal = animals.Peek();
						boundsSize = animal.GetBoundingBox();
						foreach (Vector2 v in Utility.DirectionsTileVectors)
						{
							animal.Position = new Vector2((currentPoint.X + v.X) * 64f + 32f - (float)(boundsSize.Width / 2), (currentPoint.Y + v.Y) * 64f - 32f - (float)(boundsSize.Height / 2));
							if (!environment.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, false, 0, false, animal, true, false, false, false))
							{
								positionsToCheck.Enqueue(currentPoint + v);
							}
						}
					}
					iterations++;
				}
			}
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x0010E104 File Offset: 0x0010C304
		public static Point findTile(GameLocation location, int tileIndex, string layerId, string tilesheet = null)
		{
			Layer layer = location.map.RequireLayer(layerId);
			for (int y = 0; y < layer.LayerHeight; y++)
			{
				for (int x = 0; x < layer.LayerWidth; x++)
				{
					if (location.getTileIndexAt(x, y, layerId, tilesheet) == tileIndex)
					{
						return new Point(x, y);
					}
				}
			}
			return new Point(-1, -1);
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x0010E15C File Offset: 0x0010C35C
		public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, Character c, bool projectile = false)
		{
			bool[] directions = new bool[2];
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
			rect.Width = 1;
			rect.X = boundingBox.Center.X;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, false, -1, projectile, c, false, projectile, false, false))
				{
					directions[1] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, false, -1, projectile, c, false, projectile, false, false))
			{
				directions[1] = true;
			}
			rect.Width = boundingBox.Width;
			rect.X = boundingBox.X;
			rect.Height = 1;
			rect.Y = boundingBox.Center.Y;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, false, -1, projectile, c, false, projectile, false, false))
				{
					directions[0] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, false, -1, projectile, c, false, projectile, false, false))
			{
				directions[0] = true;
			}
			return directions;
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x0010E264 File Offset: 0x0010C464
		public static Color getBlendedColor(Color c1, Color c2)
		{
			return new Color((int)(Game1.random.NextBool() ? Math.Max(c1.R, c2.R) : ((c1.R + c2.R) / 2)), (int)(Game1.random.NextBool() ? Math.Max(c1.G, c2.G) : ((c1.G + c2.G) / 2)), (int)(Game1.random.NextBool() ? Math.Max(c1.B, c2.B) : ((c1.B + c2.B) / 2)));
		}

		// Token: 0x0600171C RID: 5916 RVA: 0x0010E30C File Offset: 0x0010C50C
		public static Character checkForCharacterWithinArea(Type kindOfCharacter, Vector2 positionToAvoid, GameLocation location, Microsoft.Xna.Framework.Rectangle area)
		{
			foreach (NPC i in location.characters)
			{
				if (i.GetType().Equals(kindOfCharacter) && i.GetBoundingBox().Intersects(area) && !i.Position.Equals(positionToAvoid))
				{
					return i;
				}
			}
			return null;
		}

		// Token: 0x0600171D RID: 5917 RVA: 0x0010E394 File Offset: 0x0010C594
		public static int getNumberOfCharactersInRadius(GameLocation l, Point position, int tileRadius)
		{
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(position.X - tileRadius * 64, position.Y - tileRadius * 64, (tileRadius * 2 + 1) * 64, (tileRadius * 2 + 1) * 64);
			int count = 0;
			foreach (NPC i in l.characters)
			{
				if (rect.Contains(Utility.Vector2ToPoint(i.Position)))
				{
					count++;
				}
			}
			return count;
		}

		// Token: 0x0600171E RID: 5918 RVA: 0x0010E428 File Offset: 0x0010C628
		public static List<Vector2> getListOfTileLocationsForBordersOfNonTileRectangle(Microsoft.Xna.Framework.Rectangle rectangle)
		{
			return new List<Vector2>
			{
				new Vector2((float)(rectangle.Left / 64), (float)(rectangle.Top / 64)),
				new Vector2((float)(rectangle.Right / 64), (float)(rectangle.Top / 64)),
				new Vector2((float)(rectangle.Left / 64), (float)(rectangle.Bottom / 64)),
				new Vector2((float)(rectangle.Right / 64), (float)(rectangle.Bottom / 64)),
				new Vector2((float)(rectangle.Left / 64), (float)(rectangle.Center.Y / 64)),
				new Vector2((float)(rectangle.Right / 64), (float)(rectangle.Center.Y / 64)),
				new Vector2((float)(rectangle.Center.X / 64), (float)(rectangle.Bottom / 64)),
				new Vector2((float)(rectangle.Center.X / 64), (float)(rectangle.Top / 64)),
				new Vector2((float)(rectangle.Center.X / 64), (float)(rectangle.Center.Y / 64))
			};
		}

		// Token: 0x0600171F RID: 5919 RVA: 0x0010E584 File Offset: 0x0010C784
		public static void makeTemporarySpriteJuicier(TemporaryAnimatedSprite t, GameLocation l, int numAddOns = 4, int xRange = 64, int yRange = 64)
		{
			t.position.Y = t.position.Y - 8f;
			l.temporarySprites.Add(t);
			for (int i = 0; i < numAddOns; i++)
			{
				TemporaryAnimatedSprite clone = t.getClone();
				clone.delayBeforeAnimationStart = i * 100;
				clone.position += new Vector2((float)Game1.random.Next(-xRange / 2, xRange / 2 + 1), (float)Game1.random.Next(-yRange / 2, yRange / 2 + 1));
				clone.layerDepth += 1E-06f;
				l.temporarySprites.Add(clone);
			}
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x0010E62C File Offset: 0x0010C82C
		public static void recursiveObjectPlacement(Object o, int tileX, int tileY, double growthRate, double decay, GameLocation location, string terrainToExclude = "", int objectIndexAddRange = 0, double failChance = 0.0, int objectIndeAddRangeMultiplier = 1, List<string> itemIDVariations = null)
		{
			if (o == null)
			{
				return;
			}
			int parsedIndex;
			if (!int.TryParse(o.ItemId, out parsedIndex))
			{
				parsedIndex = -1;
			}
			if (location.isTileLocationOpen(new Location(tileX, tileY)) && !location.IsTileOccupiedBy(new Vector2((float)tileX, (float)tileY), CollisionMask.All, CollisionMask.None, false) && location.hasTileAt(tileX, tileY, "Back", null) && (terrainToExclude.Equals("") || (location.doesTileHaveProperty(tileX, tileY, "Type", "Back", false) != null && !location.doesTileHaveProperty(tileX, tileY, "Type", "Back", false).Equals(terrainToExclude))))
			{
				Vector2 objectPos = new Vector2((float)tileX, (float)tileY);
				if (!Game1.random.NextBool(failChance * 2.0))
				{
					string itemId = o.ItemId;
					if (parsedIndex >= 0)
					{
						itemId = (parsedIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier).ToString();
					}
					ColoredObject coloredObj = o as ColoredObject;
					if (coloredObj != null)
					{
						location.objects.Add(objectPos, new ColoredObject(itemId, 1, coloredObj.color.Value)
						{
							Fragility = o.fragility.Value,
							MinutesUntilReady = o.MinutesUntilReady,
							Name = o.name,
							CanBeSetDown = o.CanBeSetDown,
							CanBeGrabbed = o.CanBeGrabbed,
							IsSpawnedObject = o.IsSpawnedObject,
							TileLocation = objectPos,
							ColorSameIndexAsParentSheetIndex = coloredObj.ColorSameIndexAsParentSheetIndex
						});
					}
					else
					{
						location.objects.Add(objectPos, new Object(itemId, 1, false, -1, 0)
						{
							Fragility = o.fragility.Value,
							MinutesUntilReady = o.MinutesUntilReady,
							CanBeSetDown = o.canBeSetDown.Value,
							CanBeGrabbed = o.canBeGrabbed.Value,
							IsSpawnedObject = o.isSpawnedObject.Value
						});
					}
				}
				growthRate -= decay;
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveObjectPlacement(o, tileX + 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveObjectPlacement(o, tileX - 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveObjectPlacement(o, tileX, tileY + 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveObjectPlacement(o, tileX, tileY - 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
				}
			}
		}

		// Token: 0x06001721 RID: 5921 RVA: 0x0010E8BC File Offset: 0x0010CABC
		public static void recursiveFarmGrassPlacement(int tileX, int tileY, double growthRate, double decay, GameLocation farm)
		{
			if (farm.isTileLocationOpen(new Location(tileX, tileY)) && !farm.IsTileOccupiedBy(new Vector2((float)tileX, (float)tileY), CollisionMask.All, CollisionMask.None, false) && farm.doesTileHaveProperty(tileX, tileY, "Diggable", "Back", false) != null)
			{
				Vector2 objectPos = new Vector2((float)tileX, (float)tileY);
				if (Game1.random.NextDouble() < 0.05)
				{
					farm.objects.Add(new Vector2((float)tileX, (float)tileY), ItemRegistry.Create<Object>(Game1.random.Choose("(O)674", "(O)675"), 1, 0, false));
				}
				else
				{
					farm.terrainFeatures.Add(objectPos, new Grass(1, 4 - (int)((1.0 - growthRate) * 4.0)));
				}
				growthRate -= decay;
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveFarmGrassPlacement(tileX + 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveFarmGrassPlacement(tileX - 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveFarmGrassPlacement(tileX, tileY + 1, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveFarmGrassPlacement(tileX, tileY - 1, growthRate, decay, farm);
				}
			}
		}

		// Token: 0x06001722 RID: 5922 RVA: 0x0010E9FC File Offset: 0x0010CBFC
		public static void recursiveTreePlacement(int tileX, int tileY, double growthRate, int growthStage, double skipChance, GameLocation l, Microsoft.Xna.Framework.Rectangle clearPatch, bool sparse)
		{
			if (clearPatch.Contains(tileX, tileY))
			{
				return;
			}
			Vector2 location = new Vector2((float)tileX, (float)tileY);
			if (l.doesTileHaveProperty((int)location.X, (int)location.Y, "Diggable", "Back", false) != null && !l.IsNoSpawnTile(location, "All", false) && l.isTileLocationOpen(new Location((int)location.X, (int)location.Y)) && !l.IsTileOccupiedBy(location, CollisionMask.All, CollisionMask.None, false))
			{
				if (sparse)
				{
					if (l.IsTileOccupiedBy(new Vector2((float)tileX, (float)(tileY + -1)), CollisionMask.All, CollisionMask.None, false))
					{
						return;
					}
					if (l.IsTileOccupiedBy(new Vector2((float)tileX, (float)(tileY + 1)), CollisionMask.All, CollisionMask.None, false))
					{
						return;
					}
					if (l.IsTileOccupiedBy(new Vector2((float)(tileX + 1), (float)tileY), CollisionMask.All, CollisionMask.None, false))
					{
						return;
					}
					if (l.IsTileOccupiedBy(new Vector2((float)(tileX + -1), (float)tileY), CollisionMask.All, CollisionMask.None, false))
					{
						return;
					}
					if (l.IsTileOccupiedBy(new Vector2((float)(tileX + 1), (float)(tileY + 1)), CollisionMask.All, CollisionMask.None, false))
					{
						return;
					}
				}
				if (!Game1.random.NextBool(skipChance))
				{
					if (sparse && location.X < 70f && (location.X < 48f || location.Y > 26f) && Game1.random.NextDouble() < 0.07)
					{
						(l as Farm).resourceClumps.Add(new ResourceClump(Game1.random.Choose(672, 600, 602), 2, 2, location, null, null));
					}
					else
					{
						l.terrainFeatures.Add(location, new Tree(Game1.random.Next(1, 4).ToString(), (growthStage < 5) ? Game1.random.Next(5) : 5, false));
					}
					growthRate -= 0.05;
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveTreePlacement(tileX + Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveTreePlacement(tileX - Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveTreePlacement(tileX, tileY + Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					Utility.recursiveTreePlacement(tileX, tileY - Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
				}
			}
		}

		// Token: 0x06001723 RID: 5923 RVA: 0x0010ECA4 File Offset: 0x0010CEA4
		public static void recursiveRemoveTerrainFeatures(int tileX, int tileY, double growthRate, double decay, GameLocation l)
		{
			Vector2 location = new Vector2((float)tileX, (float)tileY);
			l.terrainFeatures.Remove(location);
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				Utility.recursiveRemoveTerrainFeatures(tileX + 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				Utility.recursiveRemoveTerrainFeatures(tileX - 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				Utility.recursiveRemoveTerrainFeatures(tileX, tileY + 1, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				Utility.recursiveRemoveTerrainFeatures(tileX, tileY - 1, growthRate, decay, l);
			}
		}

		// Token: 0x06001724 RID: 5924 RVA: 0x0010ED37 File Offset: 0x0010CF37
		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration)
		{
			return Utility.generateNewFarm(skipFarmGeneration, true);
		}

		// Token: 0x06001725 RID: 5925 RVA: 0x0010ED40 File Offset: 0x0010CF40
		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration, bool loadForNewGame)
		{
			Game1.fadeToBlack = false;
			Game1.fadeToBlackAlpha = 1f;
			Game1.debrisWeather.Clear();
			Game1.viewport.X = -9999;
			Game1.changeMusicTrack("none", false, MusicContext.Default);
			if (loadForNewGame)
			{
				Game1.game1.loadForNewGame(false);
			}
			Game1.currentLocation = Game1.RequireLocation("Farmhouse", false);
			Game1.currentLocation.currentEvent = new Event("none/-600 -600/farmer 4 8 2/warp farmer 4 8/end beginGame", null);
			Game1.gameMode = 2;
			yield return 100;
			yield break;
		}

		// Token: 0x06001726 RID: 5926 RVA: 0x0010ED50 File Offset: 0x0010CF50
		public static float distanceFromScreen(Vector2 pixelPosition)
		{
			float x = pixelPosition.X - (float)Game1.viewport.X;
			float y = pixelPosition.Y - (float)Game1.viewport.Y;
			float x2 = MathHelper.Clamp(x, 0f, (float)(Game1.viewport.Width - 1));
			float screenY = MathHelper.Clamp(y, 0f, (float)(Game1.viewport.Height - 1));
			return Utility.distance(x2, x, screenY, y);
		}

		// Token: 0x06001727 RID: 5927 RVA: 0x0010EDBC File Offset: 0x0010CFBC
		public static bool isOnScreen(Vector2 positionNonTile, int acceptableDistanceFromScreen)
		{
			positionNonTile.X -= (float)Game1.viewport.X;
			positionNonTile.Y -= (float)Game1.viewport.Y;
			return positionNonTile.X > (float)(-(float)acceptableDistanceFromScreen) && positionNonTile.X < (float)(Game1.viewport.Width + acceptableDistanceFromScreen) && positionNonTile.Y > (float)(-(float)acceptableDistanceFromScreen) && positionNonTile.Y < (float)(Game1.viewport.Height + acceptableDistanceFromScreen);
		}

		// Token: 0x06001728 RID: 5928 RVA: 0x0010EE38 File Offset: 0x0010D038
		public static bool isOnScreen(Point positionTile, int acceptableDistanceFromScreenNonTile, GameLocation location = null)
		{
			return (location == null || location.Equals(Game1.currentLocation)) && (positionTile.X * 64 > Game1.viewport.X - acceptableDistanceFromScreenNonTile && positionTile.X * 64 < Game1.viewport.X + Game1.viewport.Width + acceptableDistanceFromScreenNonTile && positionTile.Y * 64 > Game1.viewport.Y - acceptableDistanceFromScreenNonTile) && positionTile.Y * 64 < Game1.viewport.Y + Game1.viewport.Height + acceptableDistanceFromScreenNonTile;
		}

		// Token: 0x06001729 RID: 5929 RVA: 0x0010EECC File Offset: 0x0010D0CC
		public static void clearObjectsInArea(Microsoft.Xna.Framework.Rectangle r, GameLocation l)
		{
			for (int x = r.Left; x < r.Right; x += 64)
			{
				for (int y = r.Top; y < r.Bottom; y += 64)
				{
					l.removeEverythingFromThisTile(x / 64, y / 64);
				}
			}
		}

		// Token: 0x0600172A RID: 5930 RVA: 0x0010EF1C File Offset: 0x0010D11C
		public static void trashItem(Item item)
		{
			if (item is Object && Game1.player.specialItems.Contains(item.ItemId))
			{
				Game1.player.specialItems.Remove(item.ItemId);
			}
			if (Utility.getTrashReclamationPrice(item, Game1.player) > 0)
			{
				Game1.player.Money += Utility.getTrashReclamationPrice(item, Game1.player);
			}
			Game1.playSound("trashcan", null);
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x0010EF9C File Offset: 0x0010D19C
		public static FarmAnimal GetBestHarvestableFarmAnimal(IEnumerable<FarmAnimal> animals, Tool tool, Microsoft.Xna.Framework.Rectangle toolRect)
		{
			FarmAnimal fallbackAnimal = null;
			foreach (FarmAnimal animal in animals)
			{
				if (animal.GetHarvestBoundingBox().Intersects(toolRect))
				{
					if (animal.CanGetProduceWithTool(tool) && animal.currentProduce.Value != null && animal.isAdult())
					{
						return animal;
					}
					fallbackAnimal = animal;
				}
			}
			return fallbackAnimal;
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x0010F01C File Offset: 0x0010D21C
		public static long RandomLong(Random r = null)
		{
			if (r == null)
			{
				r = Game1.random;
			}
			byte[] bytes = new byte[8];
			r.NextBytes(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x0010F048 File Offset: 0x0010D248
		public static ulong NewUniqueIdForThisGame()
		{
			DateTime epoc = new DateTime(2012, 6, 22);
			return (ulong)((long)(DateTime.UtcNow - epoc).TotalSeconds);
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x0010F078 File Offset: 0x0010D278
		public static string FilterDirtyWords(string words)
		{
			return Program.sdk.FilterDirtyWords(words);
		}

		// Token: 0x0600172F RID: 5935 RVA: 0x0010F085 File Offset: 0x0010D285
		public static string FilterDirtyWordsIfStrictPlatform(string words)
		{
			return words;
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x0010F088 File Offset: 0x0010D288
		public static string FilterUserName(string name)
		{
			return name;
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x0010F08B File Offset: 0x0010D28B
		public static bool IsHorizontalDirection(int direction)
		{
			return direction == 3 || direction == 1;
		}

		// Token: 0x06001732 RID: 5938 RVA: 0x0010F097 File Offset: 0x0010D297
		public static bool IsVerticalDirection(int direction)
		{
			return direction == 0 || direction == 2;
		}

		// Token: 0x06001733 RID: 5939 RVA: 0x0010F0A4 File Offset: 0x0010D2A4
		public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int facingDirection, int pixels)
		{
			switch (facingDirection)
			{
			case 0:
				rect.Height += pixels;
				rect.Y -= pixels;
				break;
			case 1:
				rect.Width += pixels;
				break;
			case 2:
				rect.Height += pixels;
				break;
			case 3:
				rect.Width += pixels;
				rect.X -= pixels;
				break;
			}
			return rect;
		}

		// Token: 0x06001734 RID: 5940 RVA: 0x0010F118 File Offset: 0x0010D318
		public static int GetOppositeFacingDirection(int facingDirection)
		{
			switch (facingDirection)
			{
			case 0:
				return 2;
			case 1:
				return 3;
			case 2:
				return 0;
			case 3:
				return 1;
			default:
				return 0;
			}
		}

		// Token: 0x06001735 RID: 5941 RVA: 0x0010F13C File Offset: 0x0010D33C
		public static void RGBtoHSL(int r, int g, int b, out double h, out double s, out double l)
		{
			double double_r = (double)r / 255.0;
			double double_g = (double)g / 255.0;
			double double_b = (double)b / 255.0;
			double max = double_r;
			if (max < double_g)
			{
				max = double_g;
			}
			if (max < double_b)
			{
				max = double_b;
			}
			double min = double_r;
			if (min > double_g)
			{
				min = double_g;
			}
			if (min > double_b)
			{
				min = double_b;
			}
			double diff = max - min;
			l = (max + min) / 2.0;
			if (Math.Abs(diff) < 1E-05)
			{
				s = 0.0;
				h = 0.0;
				return;
			}
			if (l <= 0.5)
			{
				s = diff / (max + min);
			}
			else
			{
				s = diff / (2.0 - max - min);
			}
			double r_dist = (max - double_r) / diff;
			double g_dist = (max - double_g) / diff;
			double b_dist = (max - double_b) / diff;
			if (double_r == max)
			{
				h = b_dist - g_dist;
			}
			else if (double_g == max)
			{
				h = 2.0 + r_dist - b_dist;
			}
			else
			{
				h = 4.0 + g_dist - r_dist;
			}
			h *= 60.0;
			if (h < 0.0)
			{
				h += 360.0;
			}
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x0010F278 File Offset: 0x0010D478
		public static void HSLtoRGB(double h, double s, double l, out int r, out int g, out int b)
		{
			double p2;
			if (l <= 0.5)
			{
				p2 = l * (1.0 + s);
			}
			else
			{
				p2 = l + s - l * s;
			}
			double p3 = 2.0 * l - p2;
			double double_r;
			double double_g;
			double double_b;
			if (s == 0.0)
			{
				double_r = l;
				double_g = l;
				double_b = l;
			}
			else
			{
				double_r = Utility.QQHtoRGB(p3, p2, h + 120.0);
				double_g = Utility.QQHtoRGB(p3, p2, h);
				double_b = Utility.QQHtoRGB(p3, p2, h - 120.0);
			}
			r = (int)(double_r * 255.0);
			g = (int)(double_g * 255.0);
			b = (int)(double_b * 255.0);
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x0010F32C File Offset: 0x0010D52C
		private static double QQHtoRGB(double q1, double q2, double hue)
		{
			if (hue > 360.0)
			{
				hue -= 360.0;
			}
			else if (hue < 0.0)
			{
				hue += 360.0;
			}
			if (hue < 60.0)
			{
				return q1 + (q2 - q1) * hue / 60.0;
			}
			if (hue < 180.0)
			{
				return q2;
			}
			if (hue < 240.0)
			{
				return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
			}
			return q1;
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x0010F3C2 File Offset: 0x0010D5C2
		public static float ModifyCoordinateFromUIScale(float coordinate)
		{
			return coordinate * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		// Token: 0x06001739 RID: 5945 RVA: 0x0010F3DB File Offset: 0x0010D5DB
		public static Vector2 ModifyCoordinatesFromUIScale(Vector2 coordinates)
		{
			return coordinates * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		// Token: 0x0600173A RID: 5946 RVA: 0x0010F3FC File Offset: 0x0010D5FC
		public static float ModifyCoordinateForUIScale(float coordinate)
		{
			return coordinate / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		// Token: 0x0600173B RID: 5947 RVA: 0x0010F415 File Offset: 0x0010D615
		public static Vector2 ModifyCoordinatesForUIScale(Vector2 coordinates)
		{
			return coordinates / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		// Token: 0x0600173C RID: 5948 RVA: 0x0010F436 File Offset: 0x0010D636
		public static bool ShouldIgnoreValueChangeCallback()
		{
			return Game1.gameMode != 3 || (Game1.client != null && !Game1.client.readyToPlay) || (Game1.client != null && Game1.locationRequest != null);
		}

		// Token: 0x0600173D RID: 5949 RVA: 0x0010F468 File Offset: 0x0010D668
		public static int WrapIndex(int index, int count)
		{
			return (index + count) % count;
		}

		// Token: 0x06001740 RID: 5952 RVA: 0x0010F61C File Offset: 0x0010D81C
		[CompilerGenerated]
		internal static string <fuzzyCompare>g__FormatForFuzzySearch|36_0(string value)
		{
			string minimalFormatted = value.Trim().ToLowerInvariant().Replace(" ", "");
			string formatted = minimalFormatted.Replace("(", "").Replace(")", "").Replace("'", "").Replace(".", "").Replace("!", "").Replace("?", "").Replace("-", "");
			if (formatted.Length != 0)
			{
				return formatted;
			}
			return minimalFormatted;
		}

		// Token: 0x04000E01 RID: 3585
		public static Color[] PRISMATIC_COLORS = new Color[]
		{
			Color.Red,
			new Color(255, 120, 0),
			new Color(255, 217, 0),
			Color.Lime,
			Color.Cyan,
			Color.Violet
		};

		// Token: 0x04000E02 RID: 3586
		public static Item recentlyDiscoveredMissingBasicShippedItem;

		// Token: 0x04000E03 RID: 3587
		public static readonly Vector2[] DirectionsTileVectors = new Vector2[]
		{
			new Vector2(0f, -1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(-1f, 0f)
		};

		// Token: 0x04000E04 RID: 3588
		public static readonly Vector2[] DirectionsTileVectorsWithDiagonals = new Vector2[]
		{
			new Vector2(0f, -1f),
			new Vector2(1f, -1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),
			new Vector2(-1f, 1f),
			new Vector2(-1f, 0f),
			new Vector2(-1f, -1f)
		};

		// Token: 0x04000E05 RID: 3589
		public static readonly RasterizerState ScissorEnabled = new RasterizerState
		{
			ScissorTestEnable = true
		};

		// Token: 0x020004DF RID: 1247
		[Flags]
		public enum HorseWarpRestrictions
		{
			// Token: 0x040029C6 RID: 10694
			None = 0,
			// Token: 0x040029C7 RID: 10695
			NoOwnedHorse = 1,
			// Token: 0x040029C8 RID: 10696
			Indoors = 2,
			// Token: 0x040029C9 RID: 10697
			NoRoom = 4,
			// Token: 0x040029CA RID: 10698
			InUse = 8
		}
	}
}
