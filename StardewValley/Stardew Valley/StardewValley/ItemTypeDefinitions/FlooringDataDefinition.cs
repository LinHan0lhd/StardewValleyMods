using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002FA RID: 762
	public class FlooringDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x0600331D RID: 13085 RVA: 0x00297561 File Offset: 0x00295761
		public override string Identifier
		{
			get
			{
				return "(FL)";
			}
		}

		// Token: 0x0600331E RID: 13086 RVA: 0x00297568 File Offset: 0x00295768
		public override IEnumerable<string> GetAllIds()
		{
			int num;
			for (int i = 0; i < 88; i = num + 1)
			{
				yield return i.ToString();
				num = i;
			}
			List<ModWallpaperOrFlooring> data = DataLoader.AdditionalWallpaperFlooring(Game1.content);
			foreach (ModWallpaperOrFlooring set in data)
			{
				if (set.IsFlooring)
				{
					for (int i = 0; i < set.Count; i = num + 1)
					{
						yield return set.Id + ":" + i.ToString();
						num = i;
					}
				}
				set = null;
			}
			List<ModWallpaperOrFlooring>.Enumerator enumerator = default(List<ModWallpaperOrFlooring>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x0600331F RID: 13087 RVA: 0x00297574 File Offset: 0x00295774
		public override bool Exists(string itemId)
		{
			if (itemId == null)
			{
				return false;
			}
			int legacyId;
			if (this.TryParseLegacyId(itemId, out legacyId))
			{
				return true;
			}
			string id;
			int index;
			this.ParseStandardId(itemId, out id, out index);
			ModWallpaperOrFlooring flooringSet = this.GetFlooringSet(id);
			int num = index;
			int? num2 = (flooringSet != null) ? new int?(flooringSet.Count) : null;
			return num < num2.GetValueOrDefault() & num2 != null;
		}

		// Token: 0x06003320 RID: 13088 RVA: 0x002975D8 File Offset: 0x002957D8
		public override ParsedItemData GetData(string itemId)
		{
			if (itemId != null)
			{
				int legacyId;
				if (this.TryParseLegacyId(itemId, out legacyId))
				{
					return this.GetData(itemId, legacyId, "Maps\\walls_and_floors", null);
				}
				string id;
				int index;
				this.ParseStandardId(itemId, out id, out index);
				ModWallpaperOrFlooring data = this.GetFlooringSet(id);
				if (data != null)
				{
					return this.GetData(itemId, index, data.Texture, data);
				}
			}
			return null;
		}

		// Token: 0x06003321 RID: 13089 RVA: 0x0029762C File Offset: 0x0029582C
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			int legacyId;
			if (this.TryParseLegacyId(data.ItemId, out legacyId))
			{
				return new Wallpaper(legacyId, true);
			}
			string id;
			int index;
			this.ParseStandardId(data.ItemId, out id, out index);
			return new Wallpaper(id, index);
		}

		// Token: 0x06003322 RID: 13090 RVA: 0x00297676 File Offset: 0x00295876
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
			return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 48);
		}

		// Token: 0x06003323 RID: 13091 RVA: 0x0029769F File Offset: 0x0029589F
		protected bool TryParseLegacyId(string raw, out int legacyId)
		{
			return int.TryParse(raw, out legacyId) && legacyId >= 0 && legacyId < 88;
		}

		// Token: 0x06003324 RID: 13092 RVA: 0x002976B8 File Offset: 0x002958B8
		protected void ParseStandardId(string raw, out string id, out int index)
		{
			id = raw;
			index = 0;
			string[] parts = raw.Split(':', 2, StringSplitOptions.None);
			int parsedIndex;
			if (parts.Length == 2 && int.TryParse(parts[1], out parsedIndex))
			{
				id = parts[0];
				index = parsedIndex;
			}
		}

		// Token: 0x06003325 RID: 13093 RVA: 0x002976F0 File Offset: 0x002958F0
		protected ModWallpaperOrFlooring GetFlooringSet(string setId)
		{
			foreach (ModWallpaperOrFlooring set in DataLoader.AdditionalWallpaperFlooring(Game1.content))
			{
				if (set.Id == setId)
				{
					if (!set.IsFlooring)
					{
						return null;
					}
					return set;
				}
			}
			return null;
		}

		// Token: 0x06003326 RID: 13094 RVA: 0x00297764 File Offset: 0x00295964
		protected ParsedItemData GetData(string itemId, int spriteIndex, string textureName, object rawData)
		{
			return new ParsedItemData(this, itemId, spriteIndex, textureName, "Flooring", Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205"), 0, null, rawData, false, false);
		}

		// Token: 0x040021FA RID: 8698
		private const int LegacyFlooringCount = 88;
	}
}
