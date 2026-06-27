using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Pants;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000302 RID: 770
	public class PantsDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06003378 RID: 13176 RVA: 0x00298946 File Offset: 0x00296B46
		public override string Identifier
		{
			get
			{
				return "(P)";
			}
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06003379 RID: 13177 RVA: 0x0029894D File Offset: 0x00296B4D
		public override string StandardDescriptor
		{
			get
			{
				return "C";
			}
		}

		// Token: 0x0600337A RID: 13178 RVA: 0x00298954 File Offset: 0x00296B54
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.pantsData.Keys;
		}

		// Token: 0x0600337B RID: 13179 RVA: 0x00298960 File Offset: 0x00296B60
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.pantsData.ContainsKey(itemId);
		}

		// Token: 0x0600337C RID: 13180 RVA: 0x00298974 File Offset: 0x00296B74
		public override ParsedItemData GetData(string itemId)
		{
			PantsData data;
			if (itemId == null || !Game1.pantsData.TryGetValue(itemId, out data))
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "Characters\\Farmer\\pants", data.Name, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -100, null, data, false, false);
		}

		// Token: 0x0600337D RID: 13181 RVA: 0x002989DC File Offset: 0x00296BDC
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
			return new Rectangle(192 * (spriteIndex % (texture.Width / 192)), 688 * (spriteIndex / (texture.Width / 192)) + 672, 16, 16);
		}

		// Token: 0x0600337E RID: 13182 RVA: 0x00298A3C File Offset: 0x00296C3C
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return new Clothing(data.ItemId);
		}
	}
}
