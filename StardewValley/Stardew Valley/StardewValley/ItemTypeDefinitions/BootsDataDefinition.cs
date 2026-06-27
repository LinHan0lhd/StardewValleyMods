using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002F9 RID: 761
	public class BootsDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06003312 RID: 13074 RVA: 0x00297426 File Offset: 0x00295626
		public override string Identifier
		{
			get
			{
				return "(B)";
			}
		}

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06003313 RID: 13075 RVA: 0x0029742D File Offset: 0x0029562D
		public override string StandardDescriptor
		{
			get
			{
				return "B";
			}
		}

		// Token: 0x06003314 RID: 13076 RVA: 0x00297434 File Offset: 0x00295634
		public override IEnumerable<string> GetAllIds()
		{
			return this.GetDataSheet().Keys;
		}

		// Token: 0x06003315 RID: 13077 RVA: 0x00297441 File Offset: 0x00295641
		public override bool Exists(string itemId)
		{
			return itemId != null && this.GetDataSheet().ContainsKey(itemId);
		}

		// Token: 0x06003316 RID: 13078 RVA: 0x00297454 File Offset: 0x00295654
		public override ParsedItemData GetData(string itemId)
		{
			string[] fields = this.GetRawData(itemId);
			if (fields == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, this.GetSpriteIndex(itemId, fields), ArgUtility.Get(fields, 9, null, true) ?? "Maps\\springobjects", ArgUtility.Get(fields, 0, null, true), ArgUtility.Get(fields, 6, null, true), ArgUtility.Get(fields, 1, null, true), -97, null, fields, false, false);
		}

		// Token: 0x06003317 RID: 13079 RVA: 0x002974B1 File Offset: 0x002956B1
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

		// Token: 0x06003318 RID: 13080 RVA: 0x002974DA File Offset: 0x002956DA
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return new Boots(data.ItemId);
		}

		// Token: 0x06003319 RID: 13081 RVA: 0x002974F5 File Offset: 0x002956F5
		protected Dictionary<string, string> GetDataSheet()
		{
			return DataLoader.Boots(Game1.content);
		}

		// Token: 0x0600331A RID: 13082 RVA: 0x00297504 File Offset: 0x00295704
		protected string[] GetRawData(string itemId)
		{
			string raw;
			if (itemId == null || !this.GetDataSheet().TryGetValue(itemId, out raw))
			{
				return null;
			}
			return raw.Split('/', StringSplitOptions.None);
		}

		// Token: 0x0600331B RID: 13083 RVA: 0x00297530 File Offset: 0x00295730
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
