using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Logging;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000303 RID: 771
	public class ParsedItemData : IHaveItemTypeId
	{
		// Token: 0x06003380 RID: 13184 RVA: 0x00298A60 File Offset: 0x00296C60
		public ParsedItemData(IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false)
		{
			string qualifiedItemId = itemType.Identifier + itemId;
			if (string.IsNullOrWhiteSpace(internalName))
			{
				internalName = qualifiedItemId;
			}
			if (string.IsNullOrWhiteSpace(displayName))
			{
				displayName = ItemRegistry.GetUnnamedItemName(qualifiedItemId);
			}
			this.ItemType = itemType;
			this.ItemId = itemId;
			this.QualifiedItemId = qualifiedItemId;
			this.SpriteIndex = spriteIndex;
			this.TextureName = textureName;
			this.InternalName = internalName;
			this.DisplayName = displayName;
			this.Description = description;
			this.Category = category;
			this.ObjectType = objectType;
			this.RawData = rawData;
			this.IsErrorItem = isErrorItem;
			this.ExcludeFromRandomSale = excludeFromRandomSale;
			if (this.IsErrorItem)
			{
				this.LoadedTexture = true;
			}
		}

		// Token: 0x06003381 RID: 13185 RVA: 0x00298B10 File Offset: 0x00296D10
		public string GetItemTypeId()
		{
			return this.ItemType.Identifier;
		}

		// Token: 0x06003382 RID: 13186 RVA: 0x00298B20 File Offset: 0x00296D20
		public virtual Texture2D GetTexture()
		{
			if (!this.IsErrorItem)
			{
				this.LoadTextureIfNeeded();
				Texture2D texture = this.Texture;
				if (texture != null)
				{
					return texture;
				}
			}
			return this.ItemType.GetErrorTexture();
		}

		// Token: 0x06003383 RID: 13187 RVA: 0x00298B54 File Offset: 0x00296D54
		public virtual string GetTextureName()
		{
			if (!this.IsErrorItem)
			{
				this.LoadTextureIfNeeded();
				string textureName = this.TextureName;
				if (this.Texture != null && textureName != null)
				{
					return textureName;
				}
			}
			return this.ItemType.GetErrorTextureName();
		}

		// Token: 0x06003384 RID: 13188 RVA: 0x00298B90 File Offset: 0x00296D90
		public virtual Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
		{
			if (!this.IsErrorItem)
			{
				this.LoadTextureIfNeeded();
				if (this.Texture != null)
				{
					if (offset == 0)
					{
						if (spriteIndex != null)
						{
							int? num = spriteIndex;
							int spriteIndex2 = this.SpriteIndex;
							if (!(num.GetValueOrDefault() == spriteIndex2 & num != null))
							{
								goto IL_3F;
							}
						}
						return this.DefaultSourceRect;
					}
					IL_3F:
					return this.ItemType.GetSourceRect(this, this.Texture, (spriteIndex ?? this.SpriteIndex) + offset);
				}
			}
			return this.ItemType.GetErrorSourceRect();
		}

		// Token: 0x06003385 RID: 13189 RVA: 0x00298C1D File Offset: 0x00296E1D
		public virtual bool HasCategory()
		{
			return this.Category < -1;
		}

		// Token: 0x06003386 RID: 13190 RVA: 0x00298C28 File Offset: 0x00296E28
		protected virtual void LoadTextureIfNeeded()
		{
			if (this.LoadedTexture)
			{
				return;
			}
			if (this.IsErrorItem)
			{
				this.Texture = null;
				this.DefaultSourceRect = Rectangle.Empty;
				this.LoadedTexture = true;
				return;
			}
			this.Texture = this.TryLoadTexture();
			this.DefaultSourceRect = ((this.Texture == null) ? Rectangle.Empty : this.ItemType.GetSourceRect(this, this.Texture, this.SpriteIndex));
			this.LoadedTexture = true;
		}

		// Token: 0x06003387 RID: 13191 RVA: 0x00298CA0 File Offset: 0x00296EA0
		protected virtual Texture2D TryLoadTexture()
		{
			string textureName = this.TextureName;
			Texture2D result;
			try
			{
				if (!Game1.content.DoesAssetExist<Texture2D>(textureName))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed loading texture ");
					defaultInterpolatedStringHandler.AppendFormatted(textureName);
					defaultInterpolatedStringHandler.AppendLiteral(" for item ");
					defaultInterpolatedStringHandler.AppendFormatted(this.QualifiedItemId);
					defaultInterpolatedStringHandler.AppendLiteral(": asset doesn't exist.");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					result = null;
				}
				else
				{
					result = Game1.content.Load<Texture2D>(textureName);
				}
			}
			catch (Exception ex)
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed loading texture ");
				defaultInterpolatedStringHandler.AppendFormatted(textureName);
				defaultInterpolatedStringHandler.AppendLiteral(" for item ");
				defaultInterpolatedStringHandler.AppendFormatted(this.QualifiedItemId);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				result = null;
			}
			return result;
		}

		// Token: 0x04002203 RID: 8707
		private bool LoadedTexture;

		// Token: 0x04002204 RID: 8708
		private Texture2D Texture;

		// Token: 0x04002205 RID: 8709
		private Rectangle DefaultSourceRect;

		// Token: 0x04002206 RID: 8710
		public readonly IItemDataDefinition ItemType;

		// Token: 0x04002207 RID: 8711
		public readonly string ItemId;

		// Token: 0x04002208 RID: 8712
		public readonly string QualifiedItemId;

		// Token: 0x04002209 RID: 8713
		public readonly int SpriteIndex;

		// Token: 0x0400220A RID: 8714
		public readonly string TextureName;

		// Token: 0x0400220B RID: 8715
		public readonly string InternalName;

		// Token: 0x0400220C RID: 8716
		public readonly string DisplayName;

		// Token: 0x0400220D RID: 8717
		public readonly string Description;

		// Token: 0x0400220E RID: 8718
		public readonly int Category;

		// Token: 0x0400220F RID: 8719
		public readonly string ObjectType;

		// Token: 0x04002210 RID: 8720
		public readonly object RawData;

		// Token: 0x04002211 RID: 8721
		public readonly bool IsErrorItem;

		// Token: 0x04002212 RID: 8722
		public readonly bool ExcludeFromRandomSale;
	}
}
