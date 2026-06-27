using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002FB RID: 763
	public class FurnitureDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06003328 RID: 13096 RVA: 0x002977AB File Offset: 0x002959AB
		public override string Identifier
		{
			get
			{
				return "(F)";
			}
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06003329 RID: 13097 RVA: 0x002977B2 File Offset: 0x002959B2
		public override string StandardDescriptor
		{
			get
			{
				return "F";
			}
		}

		// Token: 0x0600332A RID: 13098 RVA: 0x002977B9 File Offset: 0x002959B9
		public override IEnumerable<string> GetAllIds()
		{
			return this.GetDataSheet().Keys;
		}

		// Token: 0x0600332B RID: 13099 RVA: 0x002977C6 File Offset: 0x002959C6
		public override bool Exists(string itemId)
		{
			return itemId != null && this.GetDataSheet().ContainsKey(itemId);
		}

		// Token: 0x0600332C RID: 13100 RVA: 0x002977DC File Offset: 0x002959DC
		public override ParsedItemData GetData(string itemId)
		{
			string[] fields = this.GetRawData(itemId);
			if (fields == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, this.GetSpriteIndex(itemId, fields), ArgUtility.Get(fields, 9, "TileSheets\\furniture", false), ArgUtility.Get(fields, 0, null, true), TokenParser.ParseText(ArgUtility.Get(fields, 7, null, true), null, null, null), null, -24, null, fields, false, ArgUtility.GetBool(fields, 10, false));
		}

		// Token: 0x0600332D RID: 13101 RVA: 0x0029783C File Offset: 0x00295A3C
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
			return Furniture.GetDefaultSourceRect(data.ItemId, texture);
		}

		// Token: 0x0600332E RID: 13102 RVA: 0x00297866 File Offset: 0x00295A66
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return Furniture.GetFurnitureInstance(data.ItemId, new Vector2?(Vector2.Zero));
		}

		// Token: 0x0600332F RID: 13103 RVA: 0x0029788B File Offset: 0x00295A8B
		protected Dictionary<string, string> GetDataSheet()
		{
			return DataLoader.Furniture(Game1.content);
		}

		// Token: 0x06003330 RID: 13104 RVA: 0x00297898 File Offset: 0x00295A98
		private string[] GetRawData(string itemId)
		{
			string raw;
			if (itemId == null || !this.GetDataSheet().TryGetValue(itemId, out raw))
			{
				return null;
			}
			return raw.Split('/', StringSplitOptions.None);
		}

		// Token: 0x06003331 RID: 13105 RVA: 0x002978C4 File Offset: 0x00295AC4
		protected int GetSpriteIndex(string itemId, string[] fields)
		{
			int overrideIndex = ArgUtility.GetInt(fields, 8, -1);
			if (overrideIndex > -1)
			{
				return overrideIndex;
			}
			int value;
			if (int.TryParse(itemId, out value))
			{
				return value;
			}
			return -1;
		}
	}
}
