using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects.Trinkets;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000306 RID: 774
	public class TrinketDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x0600339B RID: 13211 RVA: 0x00299276 File Offset: 0x00297476
		public override string Identifier
		{
			get
			{
				return "(TR)";
			}
		}

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x0600339C RID: 13212 RVA: 0x0029927D File Offset: 0x0029747D
		public override string StandardDescriptor
		{
			get
			{
				return "TR";
			}
		}

		// Token: 0x0600339D RID: 13213 RVA: 0x00299284 File Offset: 0x00297484
		public override IEnumerable<string> GetAllIds()
		{
			return this.GetDataSheet().Keys;
		}

		// Token: 0x0600339E RID: 13214 RVA: 0x00299291 File Offset: 0x00297491
		public override bool Exists(string itemId)
		{
			return this.GetDataSheet().ContainsKey(itemId);
		}

		// Token: 0x0600339F RID: 13215 RVA: 0x002992A0 File Offset: 0x002974A0
		public override ParsedItemData GetData(string itemId)
		{
			TrinketData data;
			if (!this.GetDataSheet().TryGetValue(itemId, out data))
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SheetIndex, data.Texture, itemId, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -101, null, null, false, false);
		}

		// Token: 0x060033A0 RID: 13216 RVA: 0x002992F5 File Offset: 0x002974F5
		public override Item CreateItem(ParsedItemData data)
		{
			return new Trinket(data.ItemId, Game1.random.Next(9999999));
		}

		// Token: 0x060033A1 RID: 13217 RVA: 0x00299311 File Offset: 0x00297511
		public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
		{
			return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16);
		}

		// Token: 0x060033A2 RID: 13218 RVA: 0x0029931E File Offset: 0x0029751E
		protected Dictionary<string, TrinketData> GetDataSheet()
		{
			return DataLoader.Trinkets(Game1.content);
		}
	}
}
