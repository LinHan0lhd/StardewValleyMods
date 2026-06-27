using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StardewValley.Extensions;
using StardewValley.GameData.Bundles;

namespace StardewValley
{
	// Token: 0x02000086 RID: 134
	public class BundleGenerator
	{
		// Token: 0x060004F8 RID: 1272 RVA: 0x0001978C File Offset: 0x0001798C
		public Dictionary<string, string> Generate(List<RandomBundleData> bundle_data, Random rng)
		{
			this.random = rng;
			this.randomBundleData = bundle_data;
			this.bundleData = new Dictionary<string, string>(DataLoader.Bundles(Game1.content));
			foreach (RandomBundleData area_data in this.randomBundleData)
			{
				List<int> index_lookups = new List<int>();
				string[] array = ArgUtility.SplitBySpace(area_data.Keys);
				Dictionary<int, BundleData> selected_bundles = new Dictionary<int, BundleData>();
				foreach (string index_string in array)
				{
					index_lookups.Add(int.Parse(index_string));
				}
				BundleSetData bundle_set = this.random.ChooseFrom(area_data.BundleSets);
				if (bundle_set != null)
				{
					foreach (BundleData bundle in bundle_set.Bundles)
					{
						selected_bundles[bundle.Index] = bundle;
					}
				}
				List<BundleData> random_bundle_pool = new List<BundleData>();
				foreach (BundleData bundle2 in area_data.Bundles)
				{
					random_bundle_pool.Add(bundle2);
				}
				for (int i = 0; i < index_lookups.Count; i++)
				{
					if (!selected_bundles.ContainsKey(i))
					{
						List<BundleData> index_bundles = new List<BundleData>();
						foreach (BundleData bundle3 in random_bundle_pool)
						{
							if (bundle3.Index == i)
							{
								index_bundles.Add(bundle3);
							}
						}
						if (index_bundles.Count > 0)
						{
							BundleData selected_bundle = this.random.ChooseFrom(index_bundles);
							random_bundle_pool.Remove(selected_bundle);
							selected_bundles[i] = selected_bundle;
						}
						else
						{
							foreach (BundleData bundle4 in random_bundle_pool)
							{
								if (bundle4.Index == -1)
								{
									index_bundles.Add(bundle4);
								}
							}
							if (index_bundles.Count > 0)
							{
								BundleData selected_bundle2 = this.random.ChooseFrom(index_bundles);
								random_bundle_pool.Remove(selected_bundle2);
								selected_bundles[i] = selected_bundle2;
							}
						}
					}
				}
				foreach (int key in selected_bundles.Keys)
				{
					BundleData data = selected_bundles[key];
					StringBuilder string_data = new StringBuilder();
					string_data.Append(data.Name);
					string_data.Append("/");
					string reward_string = data.Reward;
					if (reward_string.Length > 0)
					{
						try
						{
							if (char.IsDigit(reward_string[0]))
							{
								string[] reward_split = ArgUtility.SplitBySpace(reward_string);
								int count = int.Parse(reward_split[0]);
								Item reward = Utility.fuzzyItemSearch(string.Join(" ", reward_split, 1, reward_split.Length - 1), count, false);
								if (reward != null)
								{
									reward_string = Utility.getStandardDescriptionFromItem(reward, reward.Stack, ' ');
								}
							}
						}
						catch (Exception e)
						{
							Game1.log.Error("ERROR: Malformed reward string in bundle: " + reward_string, e);
							reward_string = data.Reward;
						}
					}
					string_data.Append(reward_string);
					string_data.Append("/");
					int color = 0;
					string color2 = data.Color;
					if (color2 != null)
					{
						switch (color2.Length)
						{
						case 3:
							if (color2 == "Red")
							{
								color = 4;
							}
							break;
						case 4:
						{
							char c = color2[0];
							if (c != 'B')
							{
								if (c == 'T')
								{
									if (color2 == "Teal")
									{
										color = 6;
									}
								}
							}
							else if (color2 == "Blue")
							{
								color = 5;
							}
							break;
						}
						case 5:
							if (color2 == "Green")
							{
								color = 0;
							}
							break;
						case 6:
						{
							char c = color2[0];
							if (c != 'O')
							{
								if (c != 'P')
								{
									if (c == 'Y')
									{
										if (color2 == "Yellow")
										{
											color = 3;
										}
									}
								}
								else if (color2 == "Purple")
								{
									color = 1;
								}
							}
							else if (color2 == "Orange")
							{
								color = 2;
							}
							break;
						}
						}
					}
					this.ParseItemList(string_data, data.Items, data.Pick, data.RequiredItems, color);
					string_data.Append("/");
					string_data.Append(data.Sprite);
					string_data.Append('/');
					string_data.Append(data.Name);
					this.bundleData[area_data.AreaName + "/" + index_lookups[key].ToString()] = string_data.ToString();
				}
			}
			return this.bundleData;
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x00019D24 File Offset: 0x00017F24
		public string ParseRandomTags(string data)
		{
			for (;;)
			{
				int open_index = data.LastIndexOf('[');
				if (open_index >= 0)
				{
					int close_index = data.IndexOf(']', open_index);
					if (close_index == -1)
					{
						break;
					}
					string inner = data.Substring(open_index + 1, close_index - open_index - 1);
					string value = this.random.ChooseFrom(inner.Split('|', StringSplitOptions.None));
					data = data.Remove(open_index, close_index - open_index + 1);
					data = data.Insert(open_index, value);
				}
				if (open_index < 0)
				{
					return data;
				}
			}
			return data;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x00019D90 File Offset: 0x00017F90
		public Item ParseItemString(string item_string)
		{
			string[] parts = ArgUtility.SplitBySpace(item_string);
			int index = 0;
			int count = int.Parse(parts[index]);
			index++;
			int quality = 0;
			string a = parts[index];
			if (!(a == "NQ"))
			{
				if (!(a == "SQ"))
				{
					if (!(a == "GQ"))
					{
						if (a == "IQ")
						{
							quality = 3;
							index++;
						}
					}
					else
					{
						quality = 2;
						index++;
					}
				}
				else
				{
					quality = 1;
					index++;
				}
			}
			else
			{
				quality = 0;
				index++;
			}
			string item_name = string.Join(" ", parts, index, parts.Length - index);
			if (char.IsDigit(item_name[0]))
			{
				Item item = ItemRegistry.Create("(O)" + item_name, count, 0, false);
				item.Quality = quality;
				return item;
			}
			Item found_item = null;
			if (item_name.EndsWithIgnoreCase("category"))
			{
				try
				{
					FieldInfo field = typeof(Object).GetField(item_name);
					if (field != null)
					{
						found_item = new Object(((int)field.GetValue(null)).ToString(), 1, false, -1, 0);
					}
				}
				catch (Exception)
				{
				}
			}
			if (found_item == null)
			{
				found_item = Utility.fuzzyItemSearch(item_name, 1, false);
				found_item.Quality = quality;
			}
			if (found_item == null)
			{
				throw new Exception("Invalid item name '" + item_name + "' encountered while generating a bundle.");
			}
			found_item.Stack = count;
			return found_item;
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00019EF0 File Offset: 0x000180F0
		public void ParseItemList(StringBuilder builder, string item_list, int pick_count, int required_items, int color)
		{
			item_list = this.ParseRandomTags(item_list);
			string[] items = item_list.Split(',', StringSplitOptions.None);
			List<string> item_strings = new List<string>();
			for (int i = 0; i < items.Length; i++)
			{
				Item item = this.ParseItemString(items[i]);
				item_strings.Add(string.Concat(new string[]
				{
					item.ItemId,
					" ",
					item.Stack.ToString(),
					" ",
					item.Quality.ToString()
				}));
			}
			if (pick_count < 0)
			{
				pick_count = item_strings.Count;
			}
			if (required_items < 0)
			{
				required_items = pick_count;
			}
			while (item_strings.Count > pick_count)
			{
				int index_to_remove = this.random.Next(item_strings.Count);
				item_strings.RemoveAt(index_to_remove);
			}
			for (int j = 0; j < item_strings.Count; j++)
			{
				builder.Append(item_strings[j]);
				if (j < item_strings.Count - 1)
				{
					builder.Append(" ");
				}
			}
			builder.Append("/");
			builder.Append(color);
			builder.Append("/");
			builder.Append(required_items);
		}

		// Token: 0x0400022E RID: 558
		public List<RandomBundleData> randomBundleData;

		// Token: 0x0400022F RID: 559
		public Dictionary<string, string> bundleData;

		// Token: 0x04000230 RID: 560
		public Random random;
	}
}
