using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000307 RID: 775
	public class WallpaperDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x060033A4 RID: 13220 RVA: 0x00299332 File Offset: 0x00297532
		public override string Identifier
		{
			get
			{
				return "(WP)";
			}
		}

		// Token: 0x060033A5 RID: 13221 RVA: 0x00299339 File Offset: 0x00297539
		public override IEnumerable<string> GetAllIds()
		{
			int num;
			for (int i = 0; i < 112; i = num + 1)
			{
				yield return i.ToString();
				num = i;
			}
			List<ModWallpaperOrFlooring> data = DataLoader.AdditionalWallpaperFlooring(Game1.content);
			foreach (ModWallpaperOrFlooring set in data)
			{
				if (!set.IsFlooring)
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

		// Token: 0x060033A6 RID: 13222 RVA: 0x00299344 File Offset: 0x00297544
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
			ModWallpaperOrFlooring wallpaperSet = this.GetWallpaperSet(id);
			int num = index;
			int? num2 = (wallpaperSet != null) ? new int?(wallpaperSet.Count) : null;
			return num < num2.GetValueOrDefault() & num2 != null;
		}

		// Token: 0x060033A7 RID: 13223 RVA: 0x002993A8 File Offset: 0x002975A8
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
				ModWallpaperOrFlooring data = this.GetWallpaperSet(id);
				if (data != null)
				{
					return this.GetData(itemId, index, data.Texture, data);
				}
			}
			return null;
		}

		// Token: 0x060033A8 RID: 13224 RVA: 0x002993F9 File Offset: 0x002975F9
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

		// Token: 0x060033A9 RID: 13225 RVA: 0x00299424 File Offset: 0x00297624
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			int legacyId;
			if (this.TryParseLegacyId(data.ItemId, out legacyId))
			{
				return new Wallpaper(legacyId, false);
			}
			string id;
			int index;
			this.ParseStandardId(data.ItemId, out id, out index);
			return new Wallpaper(id, index);
		}

		// Token: 0x060033AA RID: 13226 RVA: 0x0029946E File Offset: 0x0029766E
		protected bool TryParseLegacyId(string raw, out int legacyId)
		{
			return int.TryParse(raw, out legacyId) && legacyId >= 0 && legacyId < 112;
		}

		// Token: 0x060033AB RID: 13227 RVA: 0x00299488 File Offset: 0x00297688
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

		// Token: 0x060033AC RID: 13228 RVA: 0x002994C0 File Offset: 0x002976C0
		protected ModWallpaperOrFlooring GetWallpaperSet(string setId)
		{
			foreach (ModWallpaperOrFlooring set in DataLoader.AdditionalWallpaperFlooring(Game1.content))
			{
				if (set.Id == setId)
				{
					if (set.IsFlooring)
					{
						return null;
					}
					return set;
				}
			}
			return null;
		}

		// Token: 0x060033AD RID: 13229 RVA: 0x00299534 File Offset: 0x00297734
		protected ParsedItemData GetData(string itemId, int spriteIndex, string textureName, object rawData)
		{
			return new ParsedItemData(this, itemId, spriteIndex, textureName, "Wallpaper", Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206"), 0, null, rawData, false, false);
		}

		// Token: 0x04002213 RID: 8723
		protected const int LegacyWallpaperCount = 112;
	}
}
