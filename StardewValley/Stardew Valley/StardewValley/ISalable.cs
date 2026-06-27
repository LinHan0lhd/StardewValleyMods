using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley
{
	// Token: 0x020000B7 RID: 183
	public interface ISalable : IHaveItemTypeId
	{
		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000CAC RID: 3244
		string TypeDefinitionId { get; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000CAD RID: 3245
		string QualifiedItemId { get; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000CAE RID: 3246
		string DisplayName { get; }

		// Token: 0x06000CAF RID: 3247
		bool ShouldDrawIcon();

		// Token: 0x06000CB0 RID: 3248
		void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow);

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000CB1 RID: 3249
		string Name { get; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000CB2 RID: 3250
		// (set) Token: 0x06000CB3 RID: 3251
		bool IsRecipe { get; set; }

		// Token: 0x06000CB4 RID: 3252
		string getDescription();

		// Token: 0x06000CB5 RID: 3253
		int maximumStackSize();

		// Token: 0x06000CB6 RID: 3254
		int addToStack(Item stack);

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000CB7 RID: 3255
		// (set) Token: 0x06000CB8 RID: 3256
		int Stack { get; set; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000CB9 RID: 3257
		// (set) Token: 0x06000CBA RID: 3258
		int Quality { get; set; }

		// Token: 0x06000CBB RID: 3259
		int sellToStorePrice(long specificPlayerID = -1L);

		// Token: 0x06000CBC RID: 3260
		int salePrice(bool ignoreProfitMargins = false);

		// Token: 0x06000CBD RID: 3261
		bool appliesProfitMargins();

		// Token: 0x06000CBE RID: 3262
		bool actionWhenPurchased(string shopId);

		// Token: 0x06000CBF RID: 3263
		bool canStackWith(ISalable other);

		// Token: 0x06000CC0 RID: 3264
		bool CanBuyItem(Farmer farmer);

		// Token: 0x06000CC1 RID: 3265
		bool IsInfiniteStock();

		// Token: 0x06000CC2 RID: 3266
		ISalable GetSalableInstance();

		// Token: 0x06000CC3 RID: 3267
		void FixStackSize();

		// Token: 0x06000CC4 RID: 3268
		void FixQuality();
	}
}
