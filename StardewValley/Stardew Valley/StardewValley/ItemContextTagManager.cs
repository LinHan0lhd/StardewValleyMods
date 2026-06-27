using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley
{
	// Token: 0x020000BA RID: 186
	public static class ItemContextTagManager
	{
		// Token: 0x06000D34 RID: 3380 RVA: 0x00090880 File Offset: 0x0008EA80
		public static HashSet<string> GetBaseContextTags(string itemId)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(itemId);
			HashSet<string> tags;
			if (!ItemContextTagManager.BaseTagsCache.TryGetValue(itemData.QualifiedItemId, out tags))
			{
				IItemDataDefinition itemType = itemData.ItemType;
				tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string idTag = ItemContextTagManager.SanitizeContextTag("id_" + itemData.QualifiedItemId);
				tags.Add(idTag);
				if (itemType.StandardDescriptor != null)
				{
					string legacyIdTag = ItemContextTagManager.SanitizeContextTag("id_" + itemData.ItemType.StandardDescriptor + "_" + itemData.ItemId);
					tags.Add(legacyIdTag);
				}
				string identifier = itemType.Identifier;
				if (!(identifier == "(BC)"))
				{
					if (!(identifier == "(F)"))
					{
						if (!(identifier == "(O)"))
						{
							if (!(identifier == "(H)"))
							{
								goto IL_249;
							}
							string[] hatData = itemData.RawData as string[];
							if (hatData != null)
							{
								string rawTags = ArgUtility.Get(hatData, 4, null, true);
								tags.AddRange(ArgUtility.SplitBySpace(rawTags));
								goto IL_249;
							}
							goto IL_249;
						}
						else
						{
							ObjectData objectData = itemData.RawData as ObjectData;
							if (objectData == null)
							{
								goto IL_249;
							}
							List<string> contextTags = objectData.ContextTags;
							if (contextTags != null && contextTags.Count > 0)
							{
								foreach (string tag in objectData.ContextTags)
								{
									tags.Add(tag);
								}
							}
							if (!objectData.GeodeDropsDefaultItems)
							{
								List<ObjectGeodeDropData> geodeDrops = objectData.GeodeDrops;
								if (geodeDrops == null || geodeDrops.Count <= 0)
								{
									goto IL_207;
								}
							}
							tags.Add("geode");
							IL_207:
							if (!objectData.CanBeGivenAsGift)
							{
								tags.Add("not_giftable");
								goto IL_249;
							}
							goto IL_249;
						}
					}
				}
				else
				{
					BigCraftableData bigCraftableData = itemData.RawData as BigCraftableData;
					if (bigCraftableData == null)
					{
						goto IL_249;
					}
					List<string> contextTags2 = bigCraftableData.ContextTags;
					if (contextTags2 == null || contextTags2.Count <= 0)
					{
						goto IL_249;
					}
					using (List<string>.Enumerator enumerator = bigCraftableData.ContextTags.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							string tag2 = enumerator.Current;
							tags.Add(tag2);
						}
						goto IL_249;
					}
				}
				string[] furnitureData = itemData.RawData as string[];
				if (furnitureData != null)
				{
					string rawTags2 = ArgUtility.Get(furnitureData, 11, null, true);
					tags.AddRange(ArgUtility.SplitBySpace(rawTags2));
				}
				IL_249:
				if (itemData.InternalName != null)
				{
					tags.Add("item_" + ItemContextTagManager.SanitizeContextTag(itemData.InternalName));
				}
				if (itemData.ObjectType != null)
				{
					tags.Add("item_type_" + ItemContextTagManager.SanitizeContextTag(itemData.ObjectType));
				}
				MachineData machineData;
				if (DataLoader.Machines(Game1.content).TryGetValue(itemData.QualifiedItemId, out machineData))
				{
					tags.Add("is_machine");
					bool flag;
					if (!machineData.HasOutput)
					{
						List<MachineOutputRule> outputRules = machineData.OutputRules;
						flag = (outputRules != null && outputRules.Count > 0);
					}
					else
					{
						flag = true;
					}
					bool machineOutputs = flag;
					bool machineInputs = machineData.HasInput;
					if (!machineInputs)
					{
						List<MachineOutputRule> outputRules2 = machineData.OutputRules;
						if (outputRules2 != null && outputRules2.Count > 0)
						{
							foreach (MachineOutputRule rule in machineData.OutputRules)
							{
								if (rule.Triggers != null)
								{
									using (List<MachineOutputTriggerRule>.Enumerator enumerator3 = rule.Triggers.GetEnumerator())
									{
										while (enumerator3.MoveNext())
										{
											if (enumerator3.Current.Trigger.HasFlag(MachineOutputTrigger.ItemPlacedInMachine))
											{
												machineInputs = true;
												break;
											}
										}
									}
									if (machineInputs)
									{
										break;
									}
								}
							}
						}
					}
					if (machineOutputs)
					{
						tags.Add("machine_output");
					}
					if (machineInputs)
					{
						tags.Add("machine_input");
					}
				}
				string rawFishData;
				if (itemData.Category == -4 && DataLoader.Fish(Game1.content).TryGetValue(itemData.ItemId, out rawFishData))
				{
					string[] fields = rawFishData.Split('/', StringSplitOptions.None);
					if (fields[1] == "trap")
					{
						tags.Add("fish_trap_location_" + fields[4]);
					}
					else
					{
						tags.Add("fish_motion_" + fields[2]);
						int difficulty = Convert.ToInt32(fields[1]);
						if (difficulty <= 33)
						{
							tags.Add("fish_difficulty_easy");
						}
						else if (difficulty <= 66)
						{
							tags.Add("fish_difficulty_medium");
						}
						else if (difficulty <= 100)
						{
							tags.Add("fish_difficulty_hard");
						}
						else
						{
							tags.Add("fish_difficulty_extremely_hard");
						}
						tags.Add("fish_favor_weather_" + fields[7]);
					}
				}
				int category = itemData.Category;
				if (category <= -95)
				{
					if (category != -999)
					{
						switch (category)
						{
						case -101:
							tags.Add("category_trinket");
							break;
						case -100:
							tags.Add("category_clothing");
							break;
						case -99:
							tags.Add("category_tool");
							break;
						case -98:
							tags.Add("category_weapon");
							break;
						case -97:
							tags.Add("category_boots");
							break;
						case -96:
							tags.Add("category_ring");
							break;
						case -95:
							tags.Add("category_hat");
							break;
						}
					}
					else
					{
						tags.Add("category_litter");
					}
				}
				else
				{
					switch (category)
					{
					case -81:
						tags.Add("category_greens");
						break;
					case -80:
						tags.Add("category_flowers");
						break;
					case -79:
						tags.Add("category_fruits");
						break;
					case -78:
					case -77:
					case -76:
						break;
					case -75:
						tags.Add("category_vegetable");
						break;
					case -74:
						tags.Add("category_seeds");
						break;
					default:
						switch (category)
						{
						case -29:
							tags.Add("category_equipment");
							break;
						case -28:
							tags.Add("category_monster_loot");
							break;
						case -27:
							tags.Add("category_syrup");
							break;
						case -26:
							tags.Add("category_artisan_goods");
							break;
						case -25:
							tags.Add("category_ingredients");
							break;
						case -24:
							tags.Add("category_furniture");
							break;
						case -23:
							tags.Add("category_sell_at_fish_shop");
							break;
						case -22:
							tags.Add("category_tackle");
							break;
						case -21:
							tags.Add("category_bait");
							break;
						case -20:
							tags.Add("category_junk");
							break;
						case -19:
							tags.Add("category_fertilizer");
							break;
						case -18:
							tags.Add("category_sell_at_pierres_and_marnies");
							break;
						case -17:
							tags.Add("category_sell_at_pierres");
							break;
						case -16:
							tags.Add("category_building_resources");
							break;
						case -15:
							tags.Add("category_metal_resources");
							break;
						case -14:
							tags.Add("category_meat");
							break;
						case -12:
							tags.Add("category_minerals");
							break;
						case -9:
							tags.Add("category_big_craftable");
							break;
						case -8:
							tags.Add("category_crafting");
							break;
						case -7:
							tags.Add("category_cooking");
							break;
						case -6:
							tags.Add("category_milk");
							break;
						case -5:
							tags.Add("category_egg");
							break;
						case -4:
							tags.Add("category_fish");
							break;
						case -2:
							tags.Add("category_gem");
							break;
						}
						break;
					}
				}
				ItemContextTagManager.BaseTagsCache[itemData.QualifiedItemId] = tags;
			}
			return tags;
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x000910A4 File Offset: 0x0008F2A4
		public static bool HasBaseTag(string itemId, string tag)
		{
			return ItemContextTagManager.GetBaseContextTags(itemId).Contains(tag);
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x000910B2 File Offset: 0x0008F2B2
		public static bool DoesTagQueryMatch(string tagQueryString, HashSet<string> tags)
		{
			return ItemContextTagManager.DoAllTagsMatch((tagQueryString != null) ? tagQueryString.Split(',', StringSplitOptions.None) : null, tags);
		}

		// Token: 0x06000D37 RID: 3383 RVA: 0x000910CC File Offset: 0x0008F2CC
		public static bool DoAllTagsMatch(IList<string> requiredTags, HashSet<string> actualTags)
		{
			if (requiredTags == null || requiredTags.Count == 0)
			{
				return false;
			}
			using (IEnumerator<string> enumerator = requiredTags.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!ItemContextTagManager.DoesTagMatch(enumerator.Current, actualTags))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000D38 RID: 3384 RVA: 0x00091128 File Offset: 0x0008F328
		public static bool DoAnyTagsMatch(IList<string> requiredTags, HashSet<string> actualTags)
		{
			if (requiredTags != null && requiredTags.Count > 0)
			{
				foreach (string requiredTag in requiredTags)
				{
					if (requiredTag != null && requiredTag.Length > 0 && ItemContextTagManager.DoesTagMatch(requiredTag, actualTags))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x00091194 File Offset: 0x0008F394
		public static bool DoesTagMatch(string tag, HashSet<string> tags)
		{
			if (tag == null)
			{
				return false;
			}
			tag = tag.Trim();
			bool match = true;
			if (tag.StartsWith('!'))
			{
				tag = tag.Substring(1).TrimStart();
				match = false;
			}
			return tag.Length > 0 && tags.Contains(tag) == match;
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x000911DF File Offset: 0x0008F3DF
		public static string SanitizeContextTag(string tag)
		{
			return tag.Trim().ToLower().Replace(' ', '_').Replace("'", "");
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x00091204 File Offset: 0x0008F404
		public static Color? GetColorFromTags(Item item)
		{
			foreach (string tag in item.GetContextTags())
			{
				if (tag.StartsWithIgnoreCase("color_"))
				{
					string text = tag.ToLowerInvariant();
					if (text != null)
					{
						switch (text.Length)
						{
						case 9:
							if (text == "color_red")
							{
								return new Color?(new Color(220, 0, 0));
							}
							break;
						case 10:
						{
							char c = text[6];
							switch (c)
							{
							case 'b':
								if (text == "color_blue")
								{
									return new Color?(new Color(46, 85, 183));
								}
								break;
							case 'c':
								if (text == "color_cyan")
								{
									return new Color?(Color.Cyan);
								}
								break;
							case 'd':
							case 'e':
							case 'f':
							case 'h':
							case 'k':
								break;
							case 'g':
								if (text == "color_gray")
								{
									return new Color?(Color.Gray);
								}
								if (text == "color_gold")
								{
									return new Color?(Color.Gold);
								}
								break;
							case 'i':
								if (text == "color_iron")
								{
									return new Color?(new Color(197, 213, 224));
								}
								break;
							case 'j':
								if (text == "color_jade")
								{
									return new Color?(new Color(130, 158, 93));
								}
								break;
							case 'l':
								if (text == "color_lime")
								{
									return new Color?(Color.Lime);
								}
								break;
							default:
								if (c != 'p')
								{
									if (c == 's')
									{
										if (text == "color_sand")
										{
											return new Color?(Color.NavajoWhite);
										}
									}
								}
								else if (text == "color_pink")
								{
									return new Color?(new Color(255, 163, 186));
								}
								break;
							}
							break;
						}
						case 11:
						{
							char c = text[8];
							if (c <= 'e')
							{
								if (c != 'a')
								{
									if (c == 'e')
									{
										if (text == "color_green")
										{
											return new Color?(new Color(10, 143, 0));
										}
									}
								}
								else if (text == "color_black")
								{
									return new Color?(new Color(45, 45, 45));
								}
							}
							else if (c != 'i')
							{
								if (c == 'o')
								{
									if (text == "color_brown")
									{
										return new Color?(new Color(130, 73, 37));
									}
								}
							}
							else if (text == "color_white")
							{
								return new Color?(Color.White);
							}
							break;
						}
						case 12:
						{
							char c = text[6];
							if (c != 'c')
							{
								switch (c)
								{
								case 'o':
									if (text == "color_orange")
									{
										return new Color?(new Color(255, 128, 0));
									}
									break;
								case 'p':
									if (text == "color_purple")
									{
										return new Color?(new Color(115, 41, 181));
									}
									break;
								case 'q':
								case 'r':
									break;
								case 's':
									if (text == "color_salmon")
									{
										return new Color?(new Color(255, 85, 95));
									}
									break;
								default:
									if (c == 'y')
									{
										if (text == "color_yellow")
										{
											return new Color?(new Color(255, 230, 0));
										}
									}
									break;
								}
							}
							else if (text == "color_copper")
							{
								return new Color?(new Color(179, 85, 0));
							}
							break;
						}
						case 13:
							if (text == "color_iridium")
							{
								return new Color?(new Color(105, 15, 255));
							}
							break;
						case 14:
							if (text == "color_dark_red")
							{
								return new Color?(Color.DarkRed);
							}
							break;
						case 15:
						{
							char c = text[11];
							if (c <= 'c')
							{
								if (c != 'b')
								{
									if (c == 'c')
									{
										if (text == "color_dark_cyan")
										{
											return new Color?(Color.DarkCyan);
										}
									}
								}
								else if (text == "color_dark_blue")
								{
									return new Color?(Color.DarkBlue);
								}
							}
							else if (c != 'g')
							{
								switch (c)
								{
								case 'p':
									if (text == "color_dark_pink")
									{
										return new Color?(Color.DeepPink);
									}
									break;
								case 'r':
									if (text == "color_sea_green")
									{
										return new Color?(Color.SeaGreen);
									}
									break;
								case 's':
									if (text == "color_poppyseed")
									{
										return new Color?(new Color(82, 47, 153));
									}
									break;
								}
							}
							else if (text == "color_dark_gray")
							{
								return new Color?(Color.DarkGray);
							}
							break;
						}
						case 16:
						{
							char c = text[11];
							switch (c)
							{
							case '_':
								if (text == "color_light_cyan")
								{
									return new Color?(new Color(180, 255, 255));
								}
								break;
							case '`':
								break;
							case 'a':
								if (text == "color_aquamarine")
								{
									return new Color?(Color.Aquamarine);
								}
								break;
							case 'b':
								if (text == "color_dark_brown")
								{
									return new Color?(Color.SaddleBrown);
								}
								break;
							default:
								if (c == 'g')
								{
									if (text == "color_dark_green")
									{
										return new Color?(Color.DarkGreen);
									}
								}
								break;
							}
							break;
						}
						case 17:
						{
							char c = text[11];
							if (c != 'o')
							{
								if (c != 'p')
								{
									if (c == 'y')
									{
										if (text == "color_dark_yellow")
										{
											return new Color?(Color.DarkGoldenrod);
										}
									}
								}
								else if (text == "color_dark_purple")
								{
									return new Color?(Color.DarkViolet);
								}
							}
							else if (text == "color_dark_orange")
							{
								return new Color?(Color.DarkOrange);
							}
							break;
						}
						case 18:
							if (text == "color_yellow_green")
							{
								return new Color?(Color.GreenYellow);
							}
							break;
						case 21:
							if (text == "color_pale_violet_red")
							{
								return new Color?(Color.PaleVioletRed);
							}
							break;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000D3C RID: 3388 RVA: 0x00091A38 File Offset: 0x0008FC38
		internal static void ResetCache()
		{
			ItemContextTagManager.BaseTagsCache.Clear();
		}

		// Token: 0x040008D8 RID: 2264
		private static readonly Dictionary<string, HashSet<string>> BaseTagsCache = new Dictionary<string, HashSet<string>>();
	}
}
