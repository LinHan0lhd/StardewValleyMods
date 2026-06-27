using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Shirts;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000304 RID: 772
	public class ShirtDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06003388 RID: 13192 RVA: 0x00298D94 File Offset: 0x00296F94
		public override string Identifier
		{
			get
			{
				return "(S)";
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06003389 RID: 13193 RVA: 0x00298D9B File Offset: 0x00296F9B
		public override string StandardDescriptor
		{
			get
			{
				return "C";
			}
		}

		// Token: 0x0600338A RID: 13194 RVA: 0x00298DA2 File Offset: 0x00296FA2
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.shirtData.Keys;
		}

		// Token: 0x0600338B RID: 13195 RVA: 0x00298DAE File Offset: 0x00296FAE
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.shirtData.ContainsKey(itemId);
		}

		// Token: 0x0600338C RID: 13196 RVA: 0x00298DC0 File Offset: 0x00296FC0
		public override ParsedItemData GetData(string itemId)
		{
			ShirtData data;
			if (itemId == null || !Game1.shirtData.TryGetValue(itemId, out data))
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "Characters\\Farmer\\shirts", data.Name, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -100, null, data, false, false);
		}

		// Token: 0x0600338D RID: 13197 RVA: 0x00298E28 File Offset: 0x00297028
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
			int spriteAreaWidth = texture.Width / 2;
			return new Rectangle(spriteIndex * 8 % spriteAreaWidth, spriteIndex * 8 / spriteAreaWidth * 32, 8, 8);
		}

		// Token: 0x0600338E RID: 13198 RVA: 0x00298E6E File Offset: 0x0029706E
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
