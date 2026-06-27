using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002F7 RID: 759
	public abstract class BaseItemDataDefinition : IItemDataDefinition
	{
		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x060032FD RID: 13053
		public abstract string Identifier { get; }

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x060032FE RID: 13054 RVA: 0x00297283 File Offset: 0x00295483
		public virtual string StandardDescriptor
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060032FF RID: 13055
		public abstract IEnumerable<string> GetAllIds();

		// Token: 0x06003300 RID: 13056
		public abstract bool Exists(string itemId);

		// Token: 0x06003301 RID: 13057
		public abstract ParsedItemData GetData(string itemId);

		// Token: 0x06003302 RID: 13058 RVA: 0x00297288 File Offset: 0x00295488
		public ParsedItemData GetErrorData(string itemId)
		{
			return new ParsedItemData(this, itemId, 0, this.GetErrorTextureName(), "ErrorItem", ItemRegistry.GetErrorItemName(itemId), "???", -1, null, null, true, false);
		}

		// Token: 0x06003303 RID: 13059
		public abstract Item CreateItem(ParsedItemData data);

		// Token: 0x06003304 RID: 13060
		public abstract Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex);

		// Token: 0x06003305 RID: 13061 RVA: 0x002972B8 File Offset: 0x002954B8
		public virtual Texture2D GetErrorTexture()
		{
			return Game1.mouseCursors;
		}

		// Token: 0x06003306 RID: 13062 RVA: 0x002972BF File Offset: 0x002954BF
		public virtual string GetErrorTextureName()
		{
			return "LooseSprites\\Cursors";
		}

		// Token: 0x06003307 RID: 13063 RVA: 0x002972C6 File Offset: 0x002954C6
		public virtual Rectangle GetErrorSourceRect()
		{
			return new Rectangle(320, 496, 16, 16);
		}

		// Token: 0x040021F9 RID: 8697
		public Dictionary<string, ParsedItemData> ParsedItemCache = new Dictionary<string, ParsedItemData>();
	}
}
