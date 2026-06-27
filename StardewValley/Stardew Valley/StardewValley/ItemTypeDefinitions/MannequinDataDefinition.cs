using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000300 RID: 768
	public class MannequinDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06003356 RID: 13142 RVA: 0x00297C6A File Offset: 0x00295E6A
		public override string Identifier
		{
			get
			{
				return "(M)";
			}
		}

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06003357 RID: 13143 RVA: 0x00297C71 File Offset: 0x00295E71
		public override string StandardDescriptor
		{
			get
			{
				return "M";
			}
		}

		// Token: 0x06003358 RID: 13144 RVA: 0x00297C78 File Offset: 0x00295E78
		public override IEnumerable<string> GetAllIds()
		{
			return this.GetDataSheet().Keys;
		}

		// Token: 0x06003359 RID: 13145 RVA: 0x00297C85 File Offset: 0x00295E85
		public override bool Exists(string itemId)
		{
			return this.GetDataSheet().ContainsKey(itemId);
		}

		// Token: 0x0600335A RID: 13146 RVA: 0x00297C94 File Offset: 0x00295E94
		public override ParsedItemData GetData(string itemId)
		{
			MannequinData data;
			if (!this.GetDataSheet().TryGetValue(itemId, out data))
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SheetIndex, data.Texture ?? "TileSheets/Mannequins", itemId, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -24, null, null, false, false);
		}

		// Token: 0x0600335B RID: 13147 RVA: 0x00297CF2 File Offset: 0x00295EF2
		public override Item CreateItem(ParsedItemData data)
		{
			return new Mannequin(data.ItemId);
		}

		// Token: 0x0600335C RID: 13148 RVA: 0x00297CFF File Offset: 0x00295EFF
		public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
		{
			return Object.getSourceRectForBigCraftable(texture, spriteIndex);
		}

		// Token: 0x0600335D RID: 13149 RVA: 0x00297D08 File Offset: 0x00295F08
		protected Dictionary<string, MannequinData> GetDataSheet()
		{
			return DataLoader.Mannequins(Game1.content);
		}
	}
}
