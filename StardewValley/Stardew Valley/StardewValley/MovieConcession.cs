using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Movies;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x020000DE RID: 222
	public class MovieConcession : ISalable, IHaveItemTypeId
	{
		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x060010AA RID: 4266 RVA: 0x000C8049 File Offset: 0x000C6249
		public string TypeDefinitionId
		{
			get
			{
				return "(Salable)";
			}
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x060010AB RID: 4267 RVA: 0x000C8050 File Offset: 0x000C6250
		public string QualifiedItemId
		{
			get
			{
				return this.TypeDefinitionId + "MovieConcession." + this.Id;
			}
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x060010AC RID: 4268 RVA: 0x000C8068 File Offset: 0x000C6268
		public string Id
		{
			get
			{
				return this.Data.Id;
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x060010AD RID: 4269 RVA: 0x000C8075 File Offset: 0x000C6275
		public string Name
		{
			get
			{
				return this.Data.Name;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x060010AE RID: 4270 RVA: 0x000C8082 File Offset: 0x000C6282
		public string DisplayName
		{
			get
			{
				return TokenParser.ParseText(this.Data.DisplayName, null, null, null);
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x060010AF RID: 4271 RVA: 0x000C8097 File Offset: 0x000C6297
		// (set) Token: 0x060010B0 RID: 4272 RVA: 0x000C809A File Offset: 0x000C629A
		public bool IsRecipe
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x060010B1 RID: 4273 RVA: 0x000C809C File Offset: 0x000C629C
		// (set) Token: 0x060010B2 RID: 4274 RVA: 0x000C809F File Offset: 0x000C629F
		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x060010B3 RID: 4275 RVA: 0x000C80A1 File Offset: 0x000C62A1
		// (set) Token: 0x060010B4 RID: 4276 RVA: 0x000C80A4 File Offset: 0x000C62A4
		public int Quality
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x060010B5 RID: 4277 RVA: 0x000C80A6 File Offset: 0x000C62A6
		public List<string> Tags { get; }

		// Token: 0x060010B6 RID: 4278 RVA: 0x000C80AE File Offset: 0x000C62AE
		public MovieConcession(ConcessionItemData data)
		{
			this.Data = data;
			List<string> itemTags = data.ItemTags;
			this.Tags = ((itemTags != null) ? itemTags.ToList<string>() : null);
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x000C80D8 File Offset: 0x000C62D8
		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (drawShadow)
			{
				spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
			}
			spriteBatch.Draw(this.GetTexture(), location + new Vector2((float)((int)(32f * scaleSize)), (float)((int)(32f * scaleSize))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.GetTexture(), this.GetSpriteIndex(), 16, 16)), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x000C81DF File Offset: 0x000C63DF
		public Texture2D GetTexture()
		{
			if (!(this.Data.Texture == "LooseSprites\\Concessions"))
			{
				return Game1.content.Load<Texture2D>(this.Data.Texture);
			}
			return Game1.concessionsSpriteSheet;
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x000C8213 File Offset: 0x000C6413
		public int GetSpriteIndex()
		{
			return this.Data.SpriteIndex;
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x000C8220 File Offset: 0x000C6420
		public bool ShouldDrawIcon()
		{
			return true;
		}

		// Token: 0x060010BB RID: 4283 RVA: 0x000C8223 File Offset: 0x000C6423
		public string getDescription()
		{
			return Game1.parseText(TokenParser.ParseText(this.Data.Description, null, null, null), Game1.smallFont, 320);
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x000C8247 File Offset: 0x000C6447
		public int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x000C824A File Offset: 0x000C644A
		public int addToStack(Item stack)
		{
			return 1;
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x000C824D File Offset: 0x000C644D
		public bool canStackWith(ISalable other)
		{
			return false;
		}

		// Token: 0x060010BF RID: 4287 RVA: 0x000C8250 File Offset: 0x000C6450
		public int sellToStorePrice(long specificPlayerID = -1L)
		{
			return -1;
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x000C8253 File Offset: 0x000C6453
		public int salePrice(bool ignoreProfitMargins = false)
		{
			return this.Data.Price;
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x000C8260 File Offset: 0x000C6460
		public bool appliesProfitMargins()
		{
			return false;
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x000C8263 File Offset: 0x000C6463
		public bool actionWhenPurchased(string shopId)
		{
			return true;
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x000C8266 File Offset: 0x000C6466
		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		// Token: 0x060010C4 RID: 4292 RVA: 0x000C8269 File Offset: 0x000C6469
		public bool IsInfiniteStock()
		{
			return true;
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x000C826C File Offset: 0x000C646C
		public ISalable GetSalableInstance()
		{
			return this;
		}

		// Token: 0x060010C6 RID: 4294 RVA: 0x000C826F File Offset: 0x000C646F
		public void FixStackSize()
		{
		}

		// Token: 0x060010C7 RID: 4295 RVA: 0x000C8271 File Offset: 0x000C6471
		public void FixQuality()
		{
		}

		// Token: 0x060010C8 RID: 4296 RVA: 0x000C8273 File Offset: 0x000C6473
		public string GetItemTypeId()
		{
			return this.TypeDefinitionId;
		}

		// Token: 0x04000A10 RID: 2576
		private readonly ConcessionItemData Data;
	}
}
