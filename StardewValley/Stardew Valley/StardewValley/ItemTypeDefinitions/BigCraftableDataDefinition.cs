using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.BigCraftables;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002F8 RID: 760
	public class BigCraftableDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06003309 RID: 13065 RVA: 0x002972EE File Offset: 0x002954EE
		public override string Identifier
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x0600330A RID: 13066 RVA: 0x002972F5 File Offset: 0x002954F5
		public override string StandardDescriptor
		{
			get
			{
				return "BO";
			}
		}

		// Token: 0x0600330B RID: 13067 RVA: 0x002972FC File Offset: 0x002954FC
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.bigCraftableData.Keys;
		}

		// Token: 0x0600330C RID: 13068 RVA: 0x00297308 File Offset: 0x00295508
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.bigCraftableData.ContainsKey(itemId);
		}

		// Token: 0x0600330D RID: 13069 RVA: 0x0029731C File Offset: 0x0029551C
		public override ParsedItemData GetData(string itemId)
		{
			BigCraftableData data = this.GetRawData(itemId);
			if (data == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "TileSheets\\Craftables", data.Name, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -9, "Crafting", data, false, false);
		}

		// Token: 0x0600330E RID: 13070 RVA: 0x00297380 File Offset: 0x00295580
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.QualifiedItemId == "(BC)221")
			{
				return new ItemPedestal(Vector2.Zero, null, false, Color.White, "221");
			}
			return new Object(Vector2.Zero, data.ItemId, false);
		}

		// Token: 0x0600330F RID: 13071 RVA: 0x002973D5 File Offset: 0x002955D5
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
			return Object.getSourceRectForBigCraftable(texture, spriteIndex);
		}

		// Token: 0x06003310 RID: 13072 RVA: 0x002973FC File Offset: 0x002955FC
		protected BigCraftableData GetRawData(string itemId)
		{
			BigCraftableData data;
			if (itemId == null || !Game1.bigCraftableData.TryGetValue(itemId, out data))
			{
				return null;
			}
			return data;
		}
	}
}
