using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects
{
	// Token: 0x020001A6 RID: 422
	public class Clothing : Item
	{
		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06001DF8 RID: 7672 RVA: 0x001582A7 File Offset: 0x001564A7
		public override string TypeDefinitionId
		{
			get
			{
				if (this.clothesType.Value != Clothing.ClothesType.PANTS)
				{
					return "(S)";
				}
				return "(P)";
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06001DFA RID: 7674 RVA: 0x001582D0 File Offset: 0x001564D0
		// (set) Token: 0x06001DF9 RID: 7673 RVA: 0x001582C2 File Offset: 0x001564C2
		public int Price
		{
			get
			{
				return this.price.Value;
			}
			set
			{
				this.price.Value = value;
			}
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x001582E0 File Offset: 0x001564E0
		public Clothing()
		{
			base.Category = -100;
		}

		// Token: 0x06001DFC RID: 7676 RVA: 0x00158354 File Offset: 0x00156554
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.price, "price").AddField(this.indexInTileSheet, "indexInTileSheet").AddField(this.clothesType, "clothesType").AddField(this.dyeable, "dyeable").AddField(this.clothesColor, "clothesColor").AddField(this.isPrismatic, "isPrismatic");
		}

		// Token: 0x06001DFD RID: 7677 RVA: 0x001583CE File Offset: 0x001565CE
		public Clothing(string itemId) : this()
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			this.Name = "Clothing";
			base.Category = -100;
			base.ItemId = itemId;
			this.LoadData(true, false);
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x00158404 File Offset: 0x00156604
		public virtual void LoadData(bool applyColor = false, bool forceReload = false)
		{
			if (this._loadedData && !forceReload)
			{
				return;
			}
			base.Category = -100;
			PantsData pantsData;
			ShirtData shirtData;
			if (Game1.pantsData.TryGetValue(base.ItemId, out pantsData))
			{
				this.Name = pantsData.Name;
				this.price.Value = pantsData.Price;
				this.indexInTileSheet.Value = pantsData.SpriteIndex;
				this.dyeable.Value = pantsData.CanBeDyed;
				if (applyColor)
				{
					this.clothesColor.Value = (Utility.StringToColor(pantsData.DefaultColor) ?? Color.White);
				}
				else if (forceReload)
				{
					this.clothesColor.Value = Color.White;
				}
				this.displayName = TokenParser.ParseText(pantsData.DisplayName, null, null, null);
				this.description = TokenParser.ParseText(pantsData.Description, null, null, null);
				this.clothesType.Value = Clothing.ClothesType.PANTS;
				this.isPrismatic.Value = pantsData.IsPrismatic;
			}
			else if (Game1.shirtData.TryGetValue(base.ItemId, out shirtData))
			{
				this.Name = shirtData.Name;
				this.price.Value = shirtData.Price;
				this.indexInTileSheet.Value = shirtData.SpriteIndex;
				this.dyeable.Value = shirtData.CanBeDyed;
				if (applyColor)
				{
					this.clothesColor.Value = (Utility.StringToColor(shirtData.DefaultColor) ?? Color.White);
				}
				else if (forceReload)
				{
					this.clothesColor.Value = Color.White;
				}
				this.displayName = TokenParser.ParseText(shirtData.DisplayName, null, null, null);
				this.description = TokenParser.ParseText(shirtData.Description, null, null, null);
				this.clothesType.Value = Clothing.ClothesType.SHIRT;
				this.isPrismatic.Value = shirtData.IsPrismatic;
			}
			else
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				this.displayName = itemData.DisplayName;
				this.description = itemData.Description;
			}
			if (this.dyeable.Value)
			{
				this.description = this.description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
			}
			this._loadedData = true;
		}

		// Token: 0x06001DFF RID: 7679 RVA: 0x00158650 File Offset: 0x00156850
		public override string getCategoryName()
		{
			return Object.GetCategoryDisplayName(-100);
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x00158659 File Offset: 0x00156859
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			return this.price.Value;
		}

		// Token: 0x06001E01 RID: 7681 RVA: 0x00158668 File Offset: 0x00156868
		public virtual void Dye(Color color, float strength = 0.5f)
		{
			if (!this.dyeable.Value)
			{
				return;
			}
			Color current_color = this.clothesColor.Value;
			this.clothesColor.Value = new Color(Utility.MoveTowards((float)current_color.R / 255f, (float)color.R / 255f, strength), Utility.MoveTowards((float)current_color.G / 255f, (float)color.G / 255f, strength), Utility.MoveTowards((float)current_color.B / 255f, (float)color.B / 255f, strength), Utility.MoveTowards((float)current_color.A / 255f, (float)color.A / 255f, strength));
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x00158728 File Offset: 0x00156928
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			Color clothes_color = this.clothesColor.Value;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			Rectangle spriteSourceRect = itemData.GetSourceRect(0, null);
			Rectangle dyeMaskSourceRect = Rectangle.Empty;
			if (!itemData.IsErrorItem)
			{
				if (this.clothesType.Value == Clothing.ClothesType.SHIRT)
				{
					dyeMaskSourceRect = new Rectangle(spriteSourceRect.X + texture.Width / 2, spriteSourceRect.Y, spriteSourceRect.Width, spriteSourceRect.Height);
				}
				if (this.isPrismatic.Value)
				{
					clothes_color = Utility.GetPrismaticColor(0, 1f);
				}
			}
			Clothing.ClothesType value = this.clothesType.Value;
			if (value != Clothing.ClothesType.SHIRT)
			{
				if (value == Clothing.ClothesType.PANTS)
				{
					spriteBatch.Draw(texture, location + new Vector2(32f, 32f), new Rectangle?(spriteSourceRect), Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
				}
			}
			else
			{
				float dye_portion_layer_offset = 1E-07f;
				if (layerDepth >= 1f - dye_portion_layer_offset)
				{
					layerDepth = 1f - dye_portion_layer_offset;
				}
				Vector2 origin = new Vector2(4f, 4f);
				if (itemData.IsErrorItem)
				{
					origin.X = (float)(spriteSourceRect.Width / 2);
					origin.Y = (float)(spriteSourceRect.Height / 2);
				}
				spriteBatch.Draw(texture, location + new Vector2(32f, 32f), new Rectangle?(spriteSourceRect), color * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(texture, location + new Vector2(32f, 32f), new Rectangle?(dyeMaskSourceRect), Utility.MultiplyColor(clothes_color, color) * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth + dye_portion_layer_offset);
			}
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x00158925 File Offset: 0x00156B25
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x00158928 File Offset: 0x00156B28
		public override string getDescription()
		{
			if (!this._loadedData)
			{
				this.LoadData(false, false);
			}
			return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x06001E05 RID: 7685 RVA: 0x00158950 File Offset: 0x00156B50
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06001E06 RID: 7686 RVA: 0x00158953 File Offset: 0x00156B53
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (!this._loadedData)
				{
					this.LoadData(false, false);
				}
				return this.displayName;
			}
		}

		// Token: 0x06001E07 RID: 7687 RVA: 0x0015896B File Offset: 0x00156B6B
		protected override Item GetOneNew()
		{
			return new Clothing(base.ItemId);
		}

		// Token: 0x06001E08 RID: 7688 RVA: 0x00158978 File Offset: 0x00156B78
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Clothing fromClothing = source as Clothing;
			if (fromClothing != null)
			{
				this.clothesColor.Value = fromClothing.clothesColor.Value;
			}
		}

		// Token: 0x04001278 RID: 4728
		public const int SHIRT_SHEET_WIDTH = 128;

		// Token: 0x04001279 RID: 4729
		public const string DefaultShirtSheetName = "Characters\\Farmer\\shirts";

		// Token: 0x0400127A RID: 4730
		public const string DefaultPantsSheetName = "Characters\\Farmer\\pants";

		// Token: 0x0400127B RID: 4731
		public const int MinShirtId = 1000;

		// Token: 0x0400127C RID: 4732
		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		// Token: 0x0400127D RID: 4733
		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheet = new NetInt();

		// Token: 0x0400127E RID: 4734
		[XmlElement("indexInTileSheetFemale")]
		public int? obsolete_indexInTileSheetFemale;

		// Token: 0x0400127F RID: 4735
		[XmlIgnore]
		public string description;

		// Token: 0x04001280 RID: 4736
		[XmlIgnore]
		public string displayName;

		// Token: 0x04001281 RID: 4737
		[XmlElement("clothesType")]
		public readonly NetEnum<Clothing.ClothesType> clothesType = new NetEnum<Clothing.ClothesType>();

		// Token: 0x04001282 RID: 4738
		[XmlElement("dyeable")]
		public readonly NetBool dyeable = new NetBool(false);

		// Token: 0x04001283 RID: 4739
		[XmlElement("clothesColor")]
		public readonly NetColor clothesColor = new NetColor(new Color(255, 255, 255));

		// Token: 0x04001284 RID: 4740
		[XmlElement("isPrismatic")]
		public readonly NetBool isPrismatic = new NetBool(false);

		// Token: 0x04001285 RID: 4741
		[XmlIgnore]
		protected bool _loadedData;

		// Token: 0x02000553 RID: 1363
		public enum ClothesType
		{
			// Token: 0x04002B3B RID: 11067
			SHIRT,
			// Token: 0x04002B3C RID: 11068
			PANTS
		}
	}
}
