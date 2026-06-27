using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000301 RID: 769
	public class ObjectDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x0600335F RID: 13151 RVA: 0x00297D1C File Offset: 0x00295F1C
		public override string Identifier
		{
			get
			{
				return "(O)";
			}
		}

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06003360 RID: 13152 RVA: 0x00297D23 File Offset: 0x00295F23
		public override string StandardDescriptor
		{
			get
			{
				return "O";
			}
		}

		// Token: 0x06003361 RID: 13153 RVA: 0x00297D2A File Offset: 0x00295F2A
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.objectData.Keys;
		}

		// Token: 0x06003362 RID: 13154 RVA: 0x00297D36 File Offset: 0x00295F36
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.objectData.ContainsKey(itemId);
		}

		// Token: 0x06003363 RID: 13155 RVA: 0x00297D48 File Offset: 0x00295F48
		public override ParsedItemData GetData(string itemId)
		{
			ObjectData data = this.GetRawData(itemId);
			if (data == null)
			{
				return null;
			}
			int category = data.Category;
			if (category == 0 && data.Type == "Ring")
			{
				category = -96;
			}
			return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "Maps\\springobjects", data.Name, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), category, data.Type, data, false, data.ExcludeFromRandomSale);
		}

		// Token: 0x06003364 RID: 13156 RVA: 0x00297DCE File Offset: 0x00295FCE
		public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}
			return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16);
		}

		// Token: 0x06003365 RID: 13157 RVA: 0x00297DF8 File Offset: 0x00295FF8
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			string itemId = data.ItemId;
			HashSet<string> contextTags = ItemContextTagManager.GetBaseContextTags(itemId);
			if (contextTags.Contains("torch_item"))
			{
				return new Torch(1, itemId);
			}
			if (itemId == "812")
			{
				return new ColoredObject(itemId, 1, Color.Orange);
			}
			if (!contextTags.Contains("item_type_ring") && !(itemId == "801"))
			{
				return new Object(itemId, 1, false, -1, 0);
			}
			if (!(itemId == "880"))
			{
				return new Ring(itemId);
			}
			return new CombinedRing();
		}

		// Token: 0x06003366 RID: 13158 RVA: 0x00297E90 File Offset: 0x00296090
		public static bool HasExplicitCategory(ParsedItemData data)
		{
			if (data.HasTypeObject())
			{
				ObjectData objectData = data.RawData as ObjectData;
				if (objectData != null)
				{
					return objectData.Category < 0;
				}
			}
			return false;
		}

		// Token: 0x06003367 RID: 13159 RVA: 0x00297EC0 File Offset: 0x002960C0
		public static int GetRawPrice(ParsedItemData data)
		{
			if (data.HasTypeObject())
			{
				ObjectData objectData = data.RawData as ObjectData;
				if (objectData != null)
				{
					return objectData.Price;
				}
			}
			return 0;
		}

		// Token: 0x06003368 RID: 13160 RVA: 0x00297EEC File Offset: 0x002960EC
		public bool CanHaveRoe(Item fish)
		{
			Object fishObj = fish as Object;
			return fishObj != null && ItemContextTagManager.HasBaseTag(fishObj.QualifiedItemId, "fish_has_roe");
		}

		// Token: 0x06003369 RID: 13161 RVA: 0x00297F18 File Offset: 0x00296118
		public virtual ColoredObject CreateFlavoredAgedRoe(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			if (ingredient.QualifiedItemId != "(O)812")
			{
				ingredient = this.CreateFlavoredRoe(ingredient);
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
			return new ColoredObject("447", 1, color)
			{
				Name = "Aged " + ingredient.Name,
				preserve = 
				{
					Value = new Object.PreserveType?(Object.PreserveType.AgedRoe)
				},
				preservedParentSheetIndex = 
				{
					Value = ingredient.preservedParentSheetIndex.Value
				},
				Price = ingredient.Price * 2
			};
		}

		// Token: 0x0600336A RID: 13162 RVA: 0x00297FC4 File Offset: 0x002961C4
		public virtual Object CreateFlavoredHoney(Object ingredient)
		{
			Object honey = new Object("340", 1, false, -1, 0);
			if (ingredient == null || ingredient.Name == null || ingredient.Name == "Error Item" || ingredient.ItemId == "-1")
			{
				ingredient = null;
			}
			if (ingredient == null)
			{
				honey.Name = "Wild Honey";
			}
			else
			{
				honey.Name = ingredient.Name + " Honey";
				honey.Price += ingredient.Price * 2;
			}
			honey.preserve.Value = new Object.PreserveType?(Object.PreserveType.Honey);
			honey.preservedParentSheetIndex.Value = (((ingredient != null) ? ingredient.ItemId : null) ?? "-1");
			return honey;
		}

		// Token: 0x0600336B RID: 13163 RVA: 0x00298080 File Offset: 0x00296280
		public virtual Object CreateFlavoredJelly(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Red;
			Object jelly = new ColoredObject("344", 1, color);
			jelly.Name = ingredient.Name + " Jelly";
			jelly.preserve.Value = new Object.PreserveType?(Object.PreserveType.Jelly);
			jelly.preservedParentSheetIndex.Value = ingredient.ItemId;
			jelly.Price = ingredient.Price * 2 + 50;
			if (ingredient.Edibility > 0)
			{
				jelly.Edibility = (int)((float)ingredient.Edibility * 2f);
			}
			else if (ingredient.Edibility == -300)
			{
				jelly.Edibility = (int)((float)ingredient.Price * 0.2f);
			}
			else
			{
				jelly.Edibility = ingredient.Edibility;
			}
			return jelly;
		}

		// Token: 0x0600336C RID: 13164 RVA: 0x00298160 File Offset: 0x00296360
		public virtual Object CreateFlavoredJuice(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Green;
			Object juice = new ColoredObject("350", 1, color);
			juice.Name = ingredient.Name + " Juice";
			juice.preserve.Value = new Object.PreserveType?(Object.PreserveType.Juice);
			juice.preservedParentSheetIndex.Value = ingredient.ItemId;
			juice.Price = (int)((double)ingredient.Price * 2.25);
			if (ingredient.Edibility > 0)
			{
				juice.Edibility = (int)((float)ingredient.Edibility * 2f);
			}
			else if (ingredient.Edibility == -300)
			{
				juice.Edibility = (int)((float)ingredient.Price * 0.4f);
			}
			else
			{
				juice.Edibility = ingredient.Edibility;
			}
			return juice;
		}

		// Token: 0x0600336D RID: 13165 RVA: 0x00298248 File Offset: 0x00296448
		public virtual Object CreateFlavoredPickle(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Green;
			Object pickled = new ColoredObject("342", 1, color);
			pickled.Name = "Pickled " + ingredient.Name;
			pickled.preserve.Value = new Object.PreserveType?(Object.PreserveType.Pickle);
			pickled.preservedParentSheetIndex.Value = ingredient.ItemId;
			pickled.Price = ingredient.Price * 2 + 50;
			if (ingredient.Edibility > 0)
			{
				pickled.Edibility = (int)((float)ingredient.Edibility * 1.75f);
			}
			else if (ingredient.Edibility == -300)
			{
				pickled.Edibility = (int)((float)ingredient.Price * 0.25f);
			}
			else
			{
				pickled.Edibility = ingredient.Edibility;
			}
			return pickled;
		}

		// Token: 0x0600336E RID: 13166 RVA: 0x00298328 File Offset: 0x00296528
		public virtual ColoredObject CreateFlavoredRoe(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = (ingredient.QualifiedItemId == "(O)698") ? new Color(61, 55, 42) : (TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange);
			ColoredObject coloredObject = new ColoredObject("812", 1, color);
			coloredObject.Name = ingredient.Name + " Roe";
			coloredObject.preserve.Value = new Object.PreserveType?(Object.PreserveType.Roe);
			coloredObject.preservedParentSheetIndex.Value = ingredient.ItemId;
			coloredObject.Price += ingredient.Price / 2;
			return coloredObject;
		}

		// Token: 0x0600336F RID: 13167 RVA: 0x002983DC File Offset: 0x002965DC
		public virtual Object CreateFlavoredWine(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Purple;
			ColoredObject wine = new ColoredObject("348", 1, color);
			wine.Name = ingredient.Name + " Wine";
			wine.Price = ingredient.Price * 3;
			wine.preserve.Value = new Object.PreserveType?(Object.PreserveType.Wine);
			wine.preservedParentSheetIndex.Value = ingredient.ItemId;
			if (ingredient.Edibility > 0)
			{
				wine.Edibility = (int)((float)ingredient.Edibility * 1.75f);
			}
			else if (ingredient.Edibility == -300)
			{
				wine.Edibility = (int)((float)ingredient.Price * 0.1f);
			}
			else
			{
				wine.Edibility = ingredient.Edibility;
			}
			return wine;
		}

		// Token: 0x06003370 RID: 13168 RVA: 0x002984B8 File Offset: 0x002966B8
		public virtual Object CreateFlavoredBait(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
			return new ColoredObject("SpecificBait", 1, color)
			{
				Name = ingredient.Name + " Bait",
				Price = Math.Max(1, (int)((float)ingredient.Price * 0.1f)),
				preserve = 
				{
					Value = new Object.PreserveType?(Object.PreserveType.Bait)
				},
				preservedParentSheetIndex = 
				{
					Value = ingredient.ItemId
				}
			};
		}

		// Token: 0x06003371 RID: 13169 RVA: 0x00298550 File Offset: 0x00296750
		public virtual Object CreateFlavoredDriedFruit(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
			Object driedFruit = new ColoredObject("DriedFruit", 1, color);
			driedFruit.Name = Lexicon.makePlural("Dried " + ingredient.Name, false);
			driedFruit.Price = (int)((float)(ingredient.Price * 5) * 1.5f) + 25;
			driedFruit.Quality = ingredient.Quality;
			driedFruit.preserve.Value = new Object.PreserveType?(Object.PreserveType.DriedFruit);
			driedFruit.preservedParentSheetIndex.Value = ingredient.ItemId;
			driedFruit.Edibility = ingredient.Edibility * 3;
			if (ingredient.Edibility > 0)
			{
				driedFruit.Edibility = (int)((float)ingredient.Edibility * 3f);
			}
			else if (ingredient.Edibility == -300)
			{
				driedFruit.Edibility = (int)((float)ingredient.Price * 0.5f);
			}
			else
			{
				driedFruit.Edibility = ingredient.Edibility;
			}
			return driedFruit;
		}

		// Token: 0x06003372 RID: 13170 RVA: 0x00298658 File Offset: 0x00296858
		public virtual Object CreateFlavoredDriedMushroom(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
			return new ColoredObject("DriedMushrooms", 1, color)
			{
				Name = Lexicon.makePlural("Dried " + ingredient.Name, false),
				Price = (int)((float)(ingredient.Price * 5) * 1.5f) + 25,
				Quality = ingredient.Quality,
				preserve = 
				{
					Value = new Object.PreserveType?(Object.PreserveType.DriedMushroom)
				},
				preservedParentSheetIndex = 
				{
					Value = ingredient.ItemId
				},
				Edibility = ingredient.Edibility * 3
			};
		}

		// Token: 0x06003373 RID: 13171 RVA: 0x00298710 File Offset: 0x00296910
		public virtual Object CreateFlavoredSmokedFish(Object ingredient)
		{
			if (ingredient == null)
			{
				throw new ArgumentNullException("ingredient");
			}
			Color color = TailoringMenu.GetDyeColor(ingredient) ?? Color.Orange;
			Object driedFruit = new ColoredObject("SmokedFish", 1, color);
			driedFruit.Name = "Smoked " + ingredient.Name;
			driedFruit.Price = ingredient.Price * 2;
			driedFruit.Quality = ingredient.Quality;
			driedFruit.preserve.Value = new Object.PreserveType?(Object.PreserveType.SmokedFish);
			driedFruit.preservedParentSheetIndex.Value = ingredient.ItemId;
			if (ingredient.Edibility > 0)
			{
				driedFruit.Edibility = (int)((float)ingredient.Edibility * 1.5f);
			}
			else if (ingredient.Edibility == -300)
			{
				driedFruit.Edibility = (int)((float)ingredient.Price * 0.3f);
			}
			else
			{
				driedFruit.Edibility = ingredient.Edibility;
			}
			return driedFruit;
		}

		// Token: 0x06003374 RID: 13172 RVA: 0x002987FC File Offset: 0x002969FC
		public virtual Object CreateFlavoredItem(Object.PreserveType preserveType, Object ingredient)
		{
			switch (preserveType)
			{
			case Object.PreserveType.Wine:
				return this.CreateFlavoredWine(ingredient);
			case Object.PreserveType.Jelly:
				return this.CreateFlavoredJelly(ingredient);
			case Object.PreserveType.Pickle:
				return this.CreateFlavoredPickle(ingredient);
			case Object.PreserveType.Juice:
				return this.CreateFlavoredJuice(ingredient);
			case Object.PreserveType.Roe:
				return this.CreateFlavoredRoe(ingredient);
			case Object.PreserveType.AgedRoe:
				return this.CreateFlavoredAgedRoe(ingredient);
			case Object.PreserveType.Honey:
				return this.CreateFlavoredHoney(ingredient);
			case Object.PreserveType.Bait:
				return this.CreateFlavoredBait(ingredient);
			case Object.PreserveType.DriedFruit:
				return this.CreateFlavoredDriedFruit(ingredient);
			case Object.PreserveType.DriedMushroom:
				return this.CreateFlavoredDriedMushroom(ingredient);
			case Object.PreserveType.SmokedFish:
				return this.CreateFlavoredSmokedFish(ingredient);
			default:
				return null;
			}
		}

		// Token: 0x06003375 RID: 13173 RVA: 0x00298898 File Offset: 0x00296A98
		public string GetBaseItemIdForFlavoredItem(Object.PreserveType preserveType, string ingredientItemId)
		{
			switch (preserveType)
			{
			case Object.PreserveType.Wine:
				return "(O)348";
			case Object.PreserveType.Jelly:
				return "(O)344";
			case Object.PreserveType.Pickle:
				return "(O)342";
			case Object.PreserveType.Juice:
				return "(O)350";
			case Object.PreserveType.Roe:
				return "(O)812";
			case Object.PreserveType.AgedRoe:
				return "(O)447";
			case Object.PreserveType.Honey:
				return "(O)340";
			case Object.PreserveType.Bait:
				return "(O)SpecificBait";
			case Object.PreserveType.DriedFruit:
				return "(O)DriedFruit";
			case Object.PreserveType.DriedMushroom:
				return "(O)DriedMushrooms";
			case Object.PreserveType.SmokedFish:
				return "(O)SmokedFish";
			default:
				return null;
			}
		}

		// Token: 0x06003376 RID: 13174 RVA: 0x0029891C File Offset: 0x00296B1C
		protected ObjectData GetRawData(string itemId)
		{
			ObjectData data;
			if (itemId == null || !Game1.objectData.TryGetValue(itemId, out data))
			{
				return null;
			}
			return data;
		}
	}
}
