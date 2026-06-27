using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x0200008C RID: 140
	public class CraftingRecipe
	{
		// Token: 0x0600058F RID: 1423 RVA: 0x0001DD70 File Offset: 0x0001BF70
		public static void InitShared()
		{
			CraftingRecipe.craftingRecipes = DataLoader.CraftingRecipes(Game1.content);
			CraftingRecipe.cookingRecipes = DataLoader.CookingRecipes(Game1.content);
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0001DD90 File Offset: 0x0001BF90
		public CraftingRecipe(string name) : this(name, CraftingRecipe.cookingRecipes.ContainsKey(name))
		{
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0001DDA4 File Offset: 0x0001BFA4
		public CraftingRecipe(string name, bool isCookingRecipe)
		{
			this.isCookingRecipe = isCookingRecipe;
			this.name = name;
			string recipe;
			string info;
			if (isCookingRecipe && CraftingRecipe.cookingRecipes.TryGetValue(name, out recipe))
			{
				info = recipe;
			}
			else if (CraftingRecipe.craftingRecipes.TryGetValue(name, out recipe))
			{
				info = recipe;
			}
			else
			{
				name = (this.name = "Torch");
				info = CraftingRecipe.craftingRecipes[name];
			}
			string[] fields = info.Split('/', StringSplitOptions.None);
			string rawIngredients;
			string error;
			if (!ArgUtility.TryGet(fields, 0, out rawIngredients, out error, false, "string rawIngredients"))
			{
				rawIngredients = "";
				this.LogParseError(info, error);
			}
			string rawOutputItems;
			if (!ArgUtility.TryGet(fields, 2, out rawOutputItems, out error, false, "string rawOutputItems"))
			{
				rawOutputItems = "";
				this.LogParseError(info, error);
			}
			string tokenizableDisplayName;
			if (!ArgUtility.TryGetOptional(fields, isCookingRecipe ? 4 : 5, out tokenizableDisplayName, out error, null, true, "string tokenizableDisplayName"))
			{
				this.LogParseError(info, error);
			}
			this.bigCraftable = (!isCookingRecipe && ArgUtility.GetBool(fields, 3, false));
			string[] ingredients = ArgUtility.SplitBySpace(rawIngredients);
			for (int i = 0; i < ingredients.Length; i += 2)
			{
				this.recipeList.Add(ingredients[i], ArgUtility.GetInt(ingredients, i + 1, 1));
			}
			string[] outputItems = ArgUtility.SplitBySpace(rawOutputItems);
			for (int j = 0; j < outputItems.Length; j += 2)
			{
				this.itemToProduce.Add(outputItems[j]);
				this.numberProducedPerCraft = ArgUtility.GetInt(outputItems, j + 1, 1);
			}
			ParsedItemData itemData = this.GetItemData(true);
			this.DisplayName = ((!string.IsNullOrWhiteSpace(tokenizableDisplayName)) ? TokenParser.ParseText(tokenizableDisplayName, null, null, null) : (((itemData != null) ? itemData.DisplayName : null) ?? rawOutputItems));
			this.description = (((itemData != null) ? itemData.Description : null) ?? "");
			if (!Game1.player.craftingRecipes.TryGetValue(name, out this.timesCrafted))
			{
				this.timesCrafted = 0;
			}
			if (name.Equals("Crab Pot") && Game1.player.professions.Contains(7))
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				dictionary["388"] = 25;
				dictionary["334"] = 2;
				this.recipeList = dictionary;
			}
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0001DFD0 File Offset: 0x0001C1D0
		public virtual string getIndexOfMenuView()
		{
			if (this.itemToProduce.Count <= 0)
			{
				return "-1";
			}
			return this.itemToProduce[0];
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0001DFF4 File Offset: 0x0001C1F4
		public virtual bool doesFarmerHaveIngredientsInInventory(IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<string, int> kvp in this.recipeList)
			{
				int required_count = kvp.Value;
				required_count -= Game1.player.getItemCount(kvp.Key);
				if (required_count > 0)
				{
					if (extraToCheck != null)
					{
						required_count -= Game1.player.getItemCountInList(extraToCheck, kvp.Key);
						if (required_count <= 0)
						{
							continue;
						}
					}
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0001E084 File Offset: 0x0001C284
		public virtual void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88f, bool shadow = true)
		{
			ParsedItemData itemData = this.GetItemData(true);
			Texture2D texture = itemData.GetTexture();
			Rectangle sourceRect = itemData.GetSourceRect(0, null);
			Utility.drawWithShadow(b, texture, new Vector2((float)x, (float)y), sourceRect, Color.White, 0f, Vector2.Zero, 4f, false, layerDepth, -1, -1, 0.35f);
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0001E0E0 File Offset: 0x0001C2E0
		public virtual ParsedItemData GetItemData(bool useFirst = false)
		{
			string id = useFirst ? this.itemToProduce.FirstOrDefault<string>() : Game1.random.ChooseFrom(this.itemToProduce);
			if (this.bigCraftable)
			{
				id = ItemRegistry.ManuallyQualifyItemId(id, "(BC)", false);
			}
			return ItemRegistry.GetDataOrErrorItem(id);
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0001E12C File Offset: 0x0001C32C
		public virtual Item createItem()
		{
			Item item = ItemRegistry.Create(this.GetItemData(false).QualifiedItemId, this.numberProducedPerCraft, 0, false);
			if (this.isCookingRecipe)
			{
				Object obj = item as Object;
				if (obj != null && Game1.player.team.SpecialOrderRuleActive("QI_COOKING", null))
				{
					obj.orderData.Value = "QI_COOKING";
					obj.MarkContextTagsDirty();
				}
			}
			return item;
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0001E194 File Offset: 0x0001C394
		public static bool TryParseLevelRequirement(string id, string rawData, bool isCooking, out int skillNumber, out int minLevel, bool logErrors = true)
		{
			CraftingRecipe.<>c__DisplayClass27_0 CS$<>8__locals1;
			CS$<>8__locals1.isCooking = isCooking;
			CS$<>8__locals1.id = id;
			int conditionIndex = CS$<>8__locals1.isCooking ? 3 : 4;
			CS$<>8__locals1.conditions = ArgUtility.Get((rawData != null) ? rawData.Split('/', StringSplitOptions.None) : null, conditionIndex, null, true);
			string conditions = CS$<>8__locals1.conditions;
			string[] parts = (conditions != null) ? conditions.Split(' ', StringSplitOptions.None) : null;
			int argIndex = 1;
			string text = ArgUtility.Get(parts, 0, null, true);
			string text2 = (text != null) ? text.ToLower() : null;
			if (text2 != null)
			{
				switch (text2.Length)
				{
				case 1:
					if (!(text2 == "s"))
					{
						goto IL_1C9;
					}
					break;
				case 2:
				case 3:
				case 5:
					goto IL_1C9;
				case 4:
					if (!(text2 == "luck"))
					{
						goto IL_1C9;
					}
					goto IL_1C5;
				case 6:
				{
					char c = text2[0];
					if (c != 'c')
					{
						if (c != 'm')
						{
							goto IL_1C9;
						}
						if (!(text2 == "mining"))
						{
							goto IL_1C9;
						}
						goto IL_1C5;
					}
					else
					{
						if (!(text2 == "combat"))
						{
							goto IL_1C9;
						}
						goto IL_1C5;
					}
					break;
				}
				case 7:
				{
					char c = text2[1];
					if (c != 'a')
					{
						if (c != 'i')
						{
							goto IL_1C9;
						}
						if (!(text2 == "fishing"))
						{
							goto IL_1C9;
						}
						goto IL_1C5;
					}
					else
					{
						if (!(text2 == "farming"))
						{
							goto IL_1C9;
						}
						goto IL_1C5;
					}
					break;
				}
				case 8:
					if (!(text2 == "foraging"))
					{
						goto IL_1C9;
					}
					goto IL_1C5;
				default:
					goto IL_1C9;
				}
				IL_169:
				string skillId;
				string error;
				if (!ArgUtility.TryGet(parts, argIndex, out skillId, out error, true, "string skillId") || !ArgUtility.TryGetInt(parts, argIndex + 1, out minLevel, out error, "minLevel"))
				{
					CraftingRecipe.<TryParseLevelRequirement>g__LogFormatWarning|27_0(error, ref CS$<>8__locals1);
					goto IL_1C9;
				}
				skillNumber = Farmer.getSkillNumberFromName(skillId);
				if (skillNumber > -1)
				{
					return true;
				}
				CraftingRecipe.<TryParseLevelRequirement>g__LogFormatWarning|27_0("no skill found matching ID '" + skillId + "'.", ref CS$<>8__locals1);
				goto IL_1C9;
				IL_1C5:
				argIndex = 0;
				goto IL_169;
			}
			IL_1C9:
			skillNumber = -1;
			minLevel = -1;
			return false;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0001E374 File Offset: 0x0001C574
		public static bool isThereSpecialIngredientRule(Item potentialIngredient, string requiredIngredient)
		{
			return requiredIngredient == -777.ToString() && (potentialIngredient.QualifiedItemId == "(O)495" || potentialIngredient.QualifiedItemId == "(O)496" || potentialIngredient.QualifiedItemId == "(O)497" || potentialIngredient.QualifiedItemId == "(O)498");
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x0001E3E4 File Offset: 0x0001C5E4
		public virtual void consumeIngredients(List<IInventory> additionalMaterials)
		{
			foreach (KeyValuePair<string, int> pair in this.recipeList)
			{
				string itemId = pair.Key;
				int required_count = pair.Value;
				bool foundInBackpack = false;
				for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
				{
					if (CraftingRecipe.ItemMatchesForCrafting(Game1.player.Items[i], itemId))
					{
						int toRemove = required_count;
						required_count -= Game1.player.Items[i].Stack;
						Game1.player.Items[i] = Game1.player.Items[i].ConsumeStack(toRemove);
						if (required_count <= 0)
						{
							foundInBackpack = true;
							break;
						}
					}
				}
				if (additionalMaterials != null && !foundInBackpack)
				{
					for (int c = 0; c < additionalMaterials.Count; c++)
					{
						IInventory items = additionalMaterials[c];
						if (items != null)
						{
							bool removedItem = false;
							for (int j = items.Count - 1; j >= 0; j--)
							{
								if (CraftingRecipe.ItemMatchesForCrafting(items[j], itemId))
								{
									int removed_count = Math.Min(required_count, items[j].Stack);
									required_count -= removed_count;
									items[j] = items[j].ConsumeStack(removed_count);
									if (items[j] == null)
									{
										removedItem = true;
									}
									if (required_count <= 0)
									{
										break;
									}
								}
							}
							if (removedItem)
							{
								items.RemoveEmptySlots();
							}
							if (required_count <= 0)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x0001E598 File Offset: 0x0001C798
		public static bool DoesFarmerHaveAdditionalIngredientsInInventory(List<KeyValuePair<string, int>> additional_recipe_items, IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<string, int> kvp in additional_recipe_items)
			{
				int required_count = kvp.Value;
				required_count -= Game1.player.getItemCount(kvp.Key);
				if (required_count > 0)
				{
					if (extraToCheck != null)
					{
						required_count -= Game1.player.getItemCountInList(extraToCheck, kvp.Key);
						if (required_count <= 0)
						{
							continue;
						}
					}
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0001E624 File Offset: 0x0001C824
		public static bool ItemMatchesForCrafting(Item item, string item_id)
		{
			if (item == null)
			{
				return false;
			}
			if (item.Category.ToString() == item_id)
			{
				return true;
			}
			if (CraftingRecipe.isThereSpecialIngredientRule(item, item_id))
			{
				return true;
			}
			ParsedItemData item_data = ItemRegistry.GetDataOrErrorItem(item_id);
			return item.QualifiedItemId == item_data.QualifiedItemId;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x0001E678 File Offset: 0x0001C878
		public static void ConsumeAdditionalIngredients(List<KeyValuePair<string, int>> additionalRecipeItems, List<IInventory> additionalMaterials)
		{
			for (int i = additionalRecipeItems.Count - 1; i >= 0; i--)
			{
				string itemId = additionalRecipeItems[i].Key;
				int requiredCount = additionalRecipeItems[i].Value;
				bool foundInBackpack = false;
				for (int j = Game1.player.Items.Count - 1; j >= 0; j--)
				{
					Item item = Game1.player.Items[j];
					if (CraftingRecipe.ItemMatchesForCrafting(item, itemId))
					{
						int toRemove = Math.Min(requiredCount, item.Stack);
						requiredCount -= toRemove;
						Game1.player.Items[j] = item.ConsumeStack(toRemove);
						if (requiredCount <= 0)
						{
							foundInBackpack = true;
							break;
						}
					}
				}
				if (additionalMaterials != null && !foundInBackpack)
				{
					for (int c = 0; c < additionalMaterials.Count; c++)
					{
						IInventory items = additionalMaterials[c];
						if (items != null)
						{
							bool removedItem = false;
							for (int k = items.Count - 1; k >= 0; k--)
							{
								Item item2 = items[k];
								if (CraftingRecipe.ItemMatchesForCrafting(item2, itemId))
								{
									int toRemove2 = Math.Min(requiredCount, item2.Stack);
									requiredCount -= toRemove2;
									items[k] = item2.ConsumeStack(toRemove2);
									if (items[k] == null)
									{
										removedItem = true;
									}
									if (requiredCount <= 0)
									{
										break;
									}
								}
							}
							if (removedItem)
							{
								items.RemoveEmptySlots();
							}
							if (requiredCount <= 0)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x0001E7E4 File Offset: 0x0001C9E4
		public virtual int getCraftableCount(IList<Chest> additional_material_chests)
		{
			List<Item> additional_items = new List<Item>();
			if (additional_material_chests != null)
			{
				for (int c = 0; c < additional_material_chests.Count; c++)
				{
					additional_items.AddRange(additional_material_chests[c].Items);
				}
			}
			return this.getCraftableCount(additional_items);
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x0001E824 File Offset: 0x0001CA24
		public virtual int getCraftableCount(IList<Item> additional_materials)
		{
			int craftable_count = -1;
			foreach (KeyValuePair<string, int> pair in this.recipeList)
			{
				int ingredient_count = 0;
				string itemId = pair.Key;
				int required_count = pair.Value;
				if (!itemId.StartsWith("(") && !itemId.StartsWith("-"))
				{
					itemId = "(O)" + itemId;
				}
				for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
				{
					Object obj = Game1.player.Items[i] as Object;
					if (obj != null && (obj.QualifiedItemId == itemId || obj.Category.ToString() == itemId || CraftingRecipe.isThereSpecialIngredientRule(obj, itemId)))
					{
						ingredient_count += obj.Stack;
					}
				}
				if (additional_materials != null)
				{
					for (int c = 0; c < additional_materials.Count; c++)
					{
						Object obj2 = additional_materials[c] as Object;
						if (obj2 != null && (obj2.QualifiedItemId == itemId || obj2.Category.ToString() == itemId || CraftingRecipe.isThereSpecialIngredientRule(obj2, itemId)))
						{
							ingredient_count += obj2.Stack;
						}
					}
				}
				int current_craftable_count = ingredient_count / required_count;
				if (current_craftable_count < craftable_count || craftable_count == -1)
				{
					craftable_count = current_craftable_count;
				}
			}
			return craftable_count;
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0001E9B8 File Offset: 0x0001CBB8
		public virtual string getCraftCountText()
		{
			int timesCrafted;
			if (this.isCookingRecipe)
			{
				int timesCooked;
				if (Game1.player.recipesCooked.TryGetValue(this.getIndexOfMenuView(), out timesCooked) && timesCooked > 0)
				{
					return Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", timesCooked);
				}
			}
			else if (Game1.player.craftingRecipes.TryGetValue(this.name, out timesCrafted) && timesCrafted > 0)
			{
				return Game1.content.LoadString("Strings\\UI:Crafting_NumberCrafted", timesCrafted);
			}
			return null;
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x0001EA34 File Offset: 0x0001CC34
		public virtual int getDescriptionHeight(int width)
		{
			return (int)(Game1.smallFont.MeasureString(Game1.parseText(this.description, Game1.smallFont, width)).Y + (float)(this.getNumberOfIngredients() * 36) + (float)((int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567")).Y) + 21f);
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x0001EA94 File Offset: 0x0001CC94
		public virtual void drawRecipeDescription(SpriteBatch b, Vector2 position, int width, IList<Item> additional_crafting_items)
		{
			int lineExpansion = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0;
			b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f, 1f, -1f, -1, -1, 1f, 3);
			int i = -1;
			foreach (KeyValuePair<string, int> pair in this.recipeList)
			{
				i++;
				int required_count = pair.Value;
				string required_item = pair.Key;
				int bag_count = Game1.player.getItemCount(required_item);
				int containers_count = 0;
				int countLeft = required_count - bag_count;
				if (additional_crafting_items != null)
				{
					containers_count = Game1.player.getItemCountInList(additional_crafting_items, required_item);
					if (countLeft > 0)
					{
						countLeft -= containers_count;
					}
				}
				string ingredient_name_text = this.getNameFromIndex(required_item);
				Color drawColor = (countLeft <= 0) ? Game1.textColor : Color.Red;
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this.getSpriteIndexFromRawIndex(required_item));
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
				float scale = 2f;
				if (sourceRect.Width > 0 || sourceRect.Height > 0)
				{
					scale *= 16f / (float)Math.Max(sourceRect.Width, sourceRect.Height);
				}
				b.Draw(texture, new Vector2(position.X + 16f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 16f), new Rectangle?(sourceRect), Color.White, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), scale, SpriteEffects.None, 0.86f);
				Utility.drawTinyDigits(required_count, b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(required_count.ToString() ?? "").X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
				Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
				Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor, 1f, -1f, -1, -1, 1f, 3);
				if (Game1.options.showAdvancedCraftingInformation)
				{
					text_draw_position.X = position.X + (float)width - 40f;
					b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle?(new Rectangle(268, 1436, 11, 13)), Color.White);
					Utility.drawTextWithShadow(b, (bag_count + containers_count).ToString() ?? "", Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString((bag_count + containers_count).ToString() + " ").X, 0f), drawColor, 1f, -1f, -1, -1, 1f, 3);
				}
			}
			b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + this.recipeList.Count * 36, width - 32, 2), Game1.textColor * 0.35f);
			Utility.drawTextWithShadow(b, Game1.parseText(this.description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, (float)(76 + this.recipeList.Count * 36 + lineExpansion)), Game1.textColor * 0.75f, 1f, -1f, -1, -1, 1f, 3);
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x0001EF30 File Offset: 0x0001D130
		public virtual int getNumberOfIngredients()
		{
			return this.recipeList.Count;
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x0001EF40 File Offset: 0x0001D140
		public virtual string getSpriteIndexFromRawIndex(string item_id)
		{
			if (item_id == "-1")
			{
				return "(O)20";
			}
			if (item_id == "-2")
			{
				return "(O)80";
			}
			if (item_id == "-3")
			{
				return "(O)24";
			}
			if (item_id == "-4")
			{
				return "(O)145";
			}
			if (item_id == "-5")
			{
				return "(O)176";
			}
			if (item_id == "-6")
			{
				return "(O)184";
			}
			if (item_id == -777.ToString())
			{
				return "(O)495";
			}
			return item_id;
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0001EFE0 File Offset: 0x0001D1E0
		public virtual string getNameFromIndex(string item_id)
		{
			if (item_id != null && item_id.StartsWith('-'))
			{
				if (item_id == "-1")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568");
				}
				if (item_id == "-2")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
				}
				if (item_id == "-3")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
				}
				if (item_id == "-4")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
				}
				if (item_id == "-5")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
				}
				if (item_id == "-6")
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
				}
				if (item_id == -777.ToString())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574");
				}
				return "???";
			}
			else
			{
				ParsedItemData item_data = ItemRegistry.GetDataOrErrorItem(item_id);
				if (item_data != null)
				{
					return item_data.DisplayName;
				}
				return ItemRegistry.GetErrorItemName();
			}
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0001F0F4 File Offset: 0x0001D2F4
		private void LogParseError(string rawData, string message)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Failed parsing raw recipe data '");
			defaultInterpolatedStringHandler.AppendFormatted(rawData);
			defaultInterpolatedStringHandler.AppendLiteral("' for ");
			defaultInterpolatedStringHandler.AppendFormatted(this.isCookingRecipe ? "cooking" : "crafting");
			defaultInterpolatedStringHandler.AppendLiteral(" recipe '");
			defaultInterpolatedStringHandler.AppendFormatted(this.name);
			defaultInterpolatedStringHandler.AppendLiteral("': ");
			defaultInterpolatedStringHandler.AppendFormatted(message);
			log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x0001F188 File Offset: 0x0001D388
		[CompilerGenerated]
		internal static void <TryParseLevelRequirement>g__LogFormatWarning|27_0(string error, ref CraftingRecipe.<>c__DisplayClass27_0 A_1)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 4);
			defaultInterpolatedStringHandler.AppendFormatted(A_1.isCooking ? "Cooking" : "Crafting");
			defaultInterpolatedStringHandler.AppendLiteral(" recipe '");
			defaultInterpolatedStringHandler.AppendFormatted(A_1.id);
			defaultInterpolatedStringHandler.AppendLiteral("' has invalid skill level condition '");
			defaultInterpolatedStringHandler.AppendFormatted(A_1.conditions);
			defaultInterpolatedStringHandler.AppendLiteral("': ");
			defaultInterpolatedStringHandler.AppendFormatted(error);
			log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x040002B6 RID: 694
		public const int wild_seed_special_category = -777;

		// Token: 0x040002B7 RID: 695
		public const int index_ingredients = 0;

		// Token: 0x040002B8 RID: 696
		public const int index_output = 2;

		// Token: 0x040002B9 RID: 697
		public const int index_cookingUnlockConditions = 3;

		// Token: 0x040002BA RID: 698
		public const int index_cookingDisplayName = 4;

		// Token: 0x040002BB RID: 699
		public const int index_craftingBigCraftable = 3;

		// Token: 0x040002BC RID: 700
		public const int index_craftingUnlockConditions = 4;

		// Token: 0x040002BD RID: 701
		public const int index_craftingDisplayName = 5;

		// Token: 0x040002BE RID: 702
		public string name;

		// Token: 0x040002BF RID: 703
		public string DisplayName;

		// Token: 0x040002C0 RID: 704
		public string description;

		// Token: 0x040002C1 RID: 705
		public static Dictionary<string, string> craftingRecipes;

		// Token: 0x040002C2 RID: 706
		public static Dictionary<string, string> cookingRecipes;

		// Token: 0x040002C3 RID: 707
		public Dictionary<string, int> recipeList = new Dictionary<string, int>();

		// Token: 0x040002C4 RID: 708
		public List<string> itemToProduce = new List<string>();

		// Token: 0x040002C5 RID: 709
		public bool bigCraftable;

		// Token: 0x040002C6 RID: 710
		public bool isCookingRecipe;

		// Token: 0x040002C7 RID: 711
		public int timesCrafted;

		// Token: 0x040002C8 RID: 712
		public int numberProducedPerCraft;
	}
}
