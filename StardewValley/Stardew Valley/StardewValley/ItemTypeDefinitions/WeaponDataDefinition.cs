using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Weapons;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000308 RID: 776
	public class WeaponDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x060033AF RID: 13231 RVA: 0x0029957B File Offset: 0x0029777B
		public override string Identifier
		{
			get
			{
				return "(W)";
			}
		}

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x060033B0 RID: 13232 RVA: 0x00299582 File Offset: 0x00297782
		public override string StandardDescriptor
		{
			get
			{
				return "W";
			}
		}

		// Token: 0x060033B1 RID: 13233 RVA: 0x00299589 File Offset: 0x00297789
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.weaponData.Keys;
		}

		// Token: 0x060033B2 RID: 13234 RVA: 0x00299595 File Offset: 0x00297795
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.weaponData.ContainsKey(itemId);
		}

		// Token: 0x060033B3 RID: 13235 RVA: 0x002995A8 File Offset: 0x002977A8
		public override ParsedItemData GetData(string itemId)
		{
			WeaponData data = this.GetRawData(itemId);
			if (data == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture, data.Name, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), MeleeWeapon.IsScythe("(W)" + itemId) ? -99 : -98, null, data, false, false);
		}

		// Token: 0x060033B4 RID: 13236 RVA: 0x00299613 File Offset: 0x00297813
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

		// Token: 0x060033B5 RID: 13237 RVA: 0x0029963C File Offset: 0x0029783C
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			string itemId = data.ItemId;
			if (!(itemId == "32") && !(itemId == "33") && !(itemId == "34"))
			{
				return new MeleeWeapon(itemId);
			}
			return new Slingshot(itemId);
		}

		// Token: 0x060033B6 RID: 13238 RVA: 0x00299694 File Offset: 0x00297894
		protected WeaponData GetRawData(string itemId)
		{
			WeaponData raw;
			if (itemId == null || !Game1.weaponData.TryGetValue(itemId, out raw))
			{
				return null;
			}
			return raw;
		}

		// Token: 0x060033B7 RID: 13239 RVA: 0x002996B8 File Offset: 0x002978B8
		protected int GetSpriteIndex(string itemId, string[] fields)
		{
			int overrideIndex = ArgUtility.GetInt(fields, 15, -1);
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
