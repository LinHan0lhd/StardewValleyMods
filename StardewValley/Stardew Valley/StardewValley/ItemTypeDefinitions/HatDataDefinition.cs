using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002FC RID: 764
	public class HatDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06003333 RID: 13107 RVA: 0x002978F5 File Offset: 0x00295AF5
		public override string Identifier
		{
			get
			{
				return "(H)";
			}
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06003334 RID: 13108 RVA: 0x002978FC File Offset: 0x00295AFC
		public override string StandardDescriptor
		{
			get
			{
				return "H";
			}
		}

		// Token: 0x06003335 RID: 13109 RVA: 0x00297903 File Offset: 0x00295B03
		public override IEnumerable<string> GetAllIds()
		{
			return this.GetDataSheet().Keys;
		}

		// Token: 0x06003336 RID: 13110 RVA: 0x00297910 File Offset: 0x00295B10
		public override bool Exists(string itemId)
		{
			return itemId != null && this.GetDataSheet().ContainsKey(itemId);
		}

		// Token: 0x06003337 RID: 13111 RVA: 0x00297924 File Offset: 0x00295B24
		public override ParsedItemData GetData(string itemId)
		{
			string[] fields = this.GetRawData(itemId);
			if (fields == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, this.GetSpriteIndex(itemId, fields), ArgUtility.Get(fields, 7, null, true) ?? "Characters\\Farmer\\hats", ArgUtility.Get(fields, 0, null, true), ArgUtility.Get(fields, 5, null, true), ArgUtility.Get(fields, 1, null, true), -95, null, fields, false, false);
		}

		// Token: 0x06003338 RID: 13112 RVA: 0x00297980 File Offset: 0x00295B80
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
			return new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4, 20, 20);
		}

		// Token: 0x06003339 RID: 13113 RVA: 0x002979CD File Offset: 0x00295BCD
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return new Hat(data.ItemId);
		}

		// Token: 0x0600333A RID: 13114 RVA: 0x002979E8 File Offset: 0x00295BE8
		protected Dictionary<string, string> GetDataSheet()
		{
			return DataLoader.Hats(Game1.content);
		}

		// Token: 0x0600333B RID: 13115 RVA: 0x002979F4 File Offset: 0x00295BF4
		protected string[] GetRawData(string itemId)
		{
			string raw;
			if (itemId == null || !this.GetDataSheet().TryGetValue(itemId, out raw))
			{
				return null;
			}
			return raw.Split('/', StringSplitOptions.None);
		}

		// Token: 0x0600333C RID: 13116 RVA: 0x00297A20 File Offset: 0x00295C20
		protected int GetSpriteIndex(string itemId, string[] fields)
		{
			int overrideIndex = ArgUtility.GetInt(fields, 6, -1);
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
