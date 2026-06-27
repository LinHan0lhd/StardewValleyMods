using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002FE RID: 766
	public interface IItemDataDefinition
	{
		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x0600333F RID: 13119
		string Identifier { get; }

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06003340 RID: 13120
		string StandardDescriptor { get; }

		// Token: 0x06003341 RID: 13121
		IEnumerable<string> GetAllIds();

		// Token: 0x06003342 RID: 13122
		bool Exists(string itemId);

		// Token: 0x06003343 RID: 13123
		ParsedItemData GetData(string itemId);

		// Token: 0x06003344 RID: 13124
		ParsedItemData GetErrorData(string itemId);

		// Token: 0x06003345 RID: 13125
		Item CreateItem(ParsedItemData data);

		// Token: 0x06003346 RID: 13126
		Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex);

		// Token: 0x06003347 RID: 13127
		Texture2D GetErrorTexture();

		// Token: 0x06003348 RID: 13128
		string GetErrorTextureName();

		// Token: 0x06003349 RID: 13129
		Rectangle GetErrorSourceRect();
	}
}
