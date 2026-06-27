using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x020000A7 RID: 167
	[InstanceStatics]
	public class FarmerRenderer : INetObject<NetFields>
	{
		// Token: 0x17000133 RID: 307
		// (get) Token: 0x0600099C RID: 2460 RVA: 0x00065804 File Offset: 0x00063A04
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("FarmerRenderer");

		// Token: 0x0600099D RID: 2461 RVA: 0x0006580C File Offset: 0x00063A0C
		public FarmerRenderer()
		{
			this.NetFields.SetOwner(this).AddField(this.textureName, "textureName").AddField(this.heightOffset, "heightOffset").AddField(this.eyes, "eyes").AddField(this.skin, "skin").AddField(this.shoes, "shoes").AddField(this.shirt, "shirt").AddField(this.pants, "pants");
			this.farmerTextureManager = Game1.content.CreateTemporary();
			this.textureName.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._spriteDirty = true;
				this._baseTextureDirty = true;
			};
			this.eyes.fieldChangeVisibleEvent += delegate(NetColor <p0>, Color <p1>, Color <p2>)
			{
				this._spriteDirty = true;
				this._eyesDirty = true;
			};
			this.skin.fieldChangeVisibleEvent += delegate(NetInt <p0>, int <p1>, int <p2>)
			{
				this._spriteDirty = true;
				this._skinDirty = true;
				this._shirtDirty = true;
			};
			this.shoes.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._spriteDirty = true;
				this._shoesDirty = true;
			};
			this.shirt.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._spriteDirty = true;
				this._shirtDirty = true;
			};
			this.pants.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this._spriteDirty = true;
				this._pantsDirty = true;
			};
			this._spriteDirty = true;
			this._baseTextureDirty = true;
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x000659A2 File Offset: 0x00063BA2
		public FarmerRenderer(string textureName, Farmer farmer) : this()
		{
			this.eyes.Set(farmer.newEyeColor.Value);
			this.textureName.Set(textureName);
			this._spriteDirty = true;
			this._baseTextureDirty = true;
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x000659DA File Offset: 0x00063BDA
		public bool isAccessoryFacialHair(int which)
		{
			return which < 6 || (which >= 19 && which <= 22);
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x000659F1 File Offset: 0x00063BF1
		public bool drawAccessoryBelowHair(int which)
		{
			return which < 8 || this.isAccessoryFacialHair(which);
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x00065A00 File Offset: 0x00063C00
		private void executeRecolorActions(Farmer farmer)
		{
			if (this._spriteDirty)
			{
				this._spriteDirty = false;
				if (this._baseTextureDirty)
				{
					this._baseTextureDirty = false;
					this.textureChanged();
					this._eyesDirty = true;
					this._shoesDirty = true;
					this._pantsDirty = true;
					this._skinDirty = true;
					this._shirtDirty = true;
				}
				if (FarmerRenderer.recolorOffsets == null)
				{
					FarmerRenderer.recolorOffsets = new Dictionary<string, Dictionary<int, List<int>>>();
				}
				if (!FarmerRenderer.recolorOffsets.ContainsKey(this.textureName.Value))
				{
					FarmerRenderer.recolorOffsets[this.textureName.Value] = new Dictionary<int, List<int>>();
					Texture2D sourceTexture = this.farmerTextureManager.Load<Texture2D>(this.textureName.Value);
					Color[] sourcePixelData = new Color[sourceTexture.Width * sourceTexture.Height];
					sourceTexture.GetData<Color>(sourcePixelData);
					this._GeneratePixelIndices(256, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(257, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(258, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(268, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(269, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(270, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(271, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(260, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(261, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(262, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(276, this.textureName.Value, sourcePixelData);
					this._GeneratePixelIndices(277, this.textureName.Value, sourcePixelData);
				}
				Color[] pixelData = new Color[this.baseTexture.Width * this.baseTexture.Height];
				this.baseTexture.GetData<Color>(pixelData);
				if (this._eyesDirty)
				{
					this._eyesDirty = false;
					this.ApplyEyeColor(this.textureName.Value, pixelData);
				}
				if (this._skinDirty)
				{
					this._skinDirty = false;
					this.ApplySkinColor(this.textureName.Value, pixelData);
				}
				if (this._shoesDirty)
				{
					this._shoesDirty = false;
					this.ApplyShoeColor(this.textureName.Value, pixelData);
				}
				if (this._shirtDirty)
				{
					this._shirtDirty = false;
					this.ApplySleeveColor(this.textureName.Value, pixelData, farmer);
				}
				if (this._pantsDirty)
				{
					this._pantsDirty = false;
				}
				this.baseTexture.SetData<Color>(pixelData);
			}
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00065CAC File Offset: 0x00063EAC
		protected void _GeneratePixelIndices(int source_color_index, string texture_name, Color[] pixels)
		{
			Color sourceColor = pixels[source_color_index];
			List<int> pixelIndices = new List<int>();
			for (int i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].PackedValue == sourceColor.PackedValue)
				{
					pixelIndices.Add(i);
				}
			}
			FarmerRenderer.recolorOffsets[texture_name][source_color_index] = pixelIndices;
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00065D03 File Offset: 0x00063F03
		public void unload()
		{
			this.farmerTextureManager.Unload();
			this.farmerTextureManager.Dispose();
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00065D1C File Offset: 0x00063F1C
		public void textureChanged()
		{
			if (this.baseTexture != null)
			{
				this.baseTexture.Dispose();
				this.baseTexture = null;
			}
			Texture2D sourceTexture = this.farmerTextureManager.Load<Texture2D>(this.textureName.Value);
			this.baseTexture = new Texture2D(Game1.graphics.GraphicsDevice, sourceTexture.GetActualWidth(), sourceTexture.GetActualHeight())
			{
				Name = "@FarmerRenderer.baseTexture"
			};
			Color[] data = new Color[sourceTexture.GetElementCount()];
			sourceTexture.GetData<Color>(data, 0, data.Length);
			this.baseTexture.SetData<Color>(data);
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x00065DA9 File Offset: 0x00063FA9
		public void recolorEyes(Color lightestColor)
		{
			this.eyes.Set(lightestColor);
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x00065DB8 File Offset: 0x00063FB8
		public void ApplyEyeColor(string texture_name, Color[] pixels)
		{
			Color lightestColor = this.eyes.Value;
			Color darkerColor = FarmerRenderer.changeBrightness(lightestColor, -75);
			if (lightestColor.Equals(darkerColor))
			{
				lightestColor.B += 10;
			}
			this._SwapColor(texture_name, pixels, 276, lightestColor);
			this._SwapColor(texture_name, pixels, 277, darkerColor);
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x00065E14 File Offset: 0x00064014
		private void _SwapColor(string texture_name, Color[] pixels, int color_index, Color color)
		{
			foreach (int pixelOffset in FarmerRenderer.recolorOffsets[texture_name][color_index])
			{
				pixels[pixelOffset] = color;
			}
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x00065E74 File Offset: 0x00064074
		public void recolorShoes(string which)
		{
			this.shoes.Set(which);
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00065E84 File Offset: 0x00064084
		private void ApplyShoeColor(string texture_name, Color[] pixels)
		{
			int which = 12;
			Texture2D texture = null;
			int splitIndex = this.shoes.Value.LastIndexOf(':');
			if (splitIndex > -1)
			{
				string texturePath = this.shoes.Value.Substring(0, splitIndex);
				string index = this.shoes.Value.Substring(splitIndex + 1);
				try
				{
					texture = this.farmerTextureManager.Load<Texture2D>(texturePath);
					if (!int.TryParse(index, out which))
					{
						which = 12;
					}
					goto IL_8E;
				}
				catch (Exception)
				{
					texture = this.farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\shoeColors");
					goto IL_8E;
				}
			}
			if (!int.TryParse(this.shoes.Value, out which))
			{
				which = 12;
			}
			IL_8E:
			if (texture == null)
			{
				texture = this.farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\shoeColors");
			}
			Texture2D shoeColors = texture;
			if (which >= shoeColors.Height)
			{
				which = shoeColors.Height - 1;
			}
			if (shoeColors.Width < 4)
			{
				return;
			}
			Color[] shoeColorsData = new Color[shoeColors.Width * shoeColors.Height];
			shoeColors.GetData<Color>(shoeColorsData);
			Color darkest = shoeColorsData[which * 4 % (shoeColors.Height * 4)];
			Color medium = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 1];
			Color lightest = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 2];
			Color lightest2 = shoeColorsData[which * 4 % (shoeColors.Height * 4) + 3];
			this._SwapColor(texture_name, pixels, 268, darkest);
			this._SwapColor(texture_name, pixels, 269, medium);
			this._SwapColor(texture_name, pixels, 270, lightest);
			this._SwapColor(texture_name, pixels, 271, lightest2);
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x00066014 File Offset: 0x00064214
		public int recolorSkin(int which, bool force = false)
		{
			if (force)
			{
				this.skin.Value = -1;
			}
			this.skin.Set(which);
			return which;
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00066034 File Offset: 0x00064234
		private void ApplySkinColor(string texture_name, Color[] pixels)
		{
			int which = this.skin.Value;
			Texture2D skinColors = this.farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
			Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
			if (which < 0)
			{
				which = skinColors.Height - 1;
			}
			if (which > skinColors.Height - 1)
			{
				which = 0;
			}
			skinColors.GetData<Color>(skinColorsData);
			Color darkest = skinColorsData[which * 3 % (skinColors.Height * 3)];
			Color medium = skinColorsData[which * 3 % (skinColors.Height * 3) + 1];
			Color lightest = skinColorsData[which * 3 % (skinColors.Height * 3) + 2];
			if (this.skin.Value == -12345)
			{
				medium = (darkest = (lightest = Color.Transparent));
			}
			this._SwapColor(texture_name, pixels, 260, darkest);
			this._SwapColor(texture_name, pixels, 261, medium);
			this._SwapColor(texture_name, pixels, 262, lightest);
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x0006611B File Offset: 0x0006431B
		public void changeShirt(string whichShirt)
		{
			this.shirt.Set(whichShirt);
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x00066129 File Offset: 0x00064329
		public void changePants(string whichPants)
		{
			this.pants.Set(whichPants);
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x00066137 File Offset: 0x00064337
		public void MarkSpriteDirty()
		{
			this._spriteDirty = true;
			this._shirtDirty = true;
			this._pantsDirty = true;
			this._eyesDirty = true;
			this._shoesDirty = true;
			this._baseTextureDirty = true;
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x00066164 File Offset: 0x00064364
		public void ApplySleeveColor(string texture_name, Color[] pixels, Farmer who)
		{
			Texture2D shirtTexture;
			int shirtIndex;
			who.GetDisplayShirt(out shirtTexture, out shirtIndex);
			Color[] shirtData = new Color[shirtTexture.Bounds.Width * shirtTexture.Bounds.Height];
			shirtTexture.GetData<Color>(shirtData);
			int index = shirtIndex * 8 / 128 * 32 * shirtTexture.Bounds.Width + shirtIndex * 8 % 128 + shirtTexture.Width * 4;
			int dyeIndex = index + 128;
			if (!who.ShirtHasSleeves() || index >= shirtData.Length || (this.skin.Value == -12345 && who.shirtItem.Value == null))
			{
				Texture2D skinColors = this.farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
				Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
				int skinIndex = this.skin.Value;
				if (skinIndex < 0)
				{
					skinIndex = skinColors.Height - 1;
				}
				if (skinIndex > skinColors.Height - 1)
				{
					skinIndex = 0;
				}
				skinColors.GetData<Color>(skinColorsData);
				Color darkest = skinColorsData[skinIndex * 3 % (skinColors.Height * 3)];
				Color medium = skinColorsData[skinIndex * 3 % (skinColors.Height * 3) + 1];
				Color lightest = skinColorsData[skinIndex * 3 % (skinColors.Height * 3) + 2];
				if (this.skin.Value == -12345)
				{
					darkest = pixels[260 + this.baseTexture.Width * 2];
					medium = pixels[261 + this.baseTexture.Width * 2];
					lightest = pixels[262 + this.baseTexture.Width * 2];
				}
				if (this._sickFrame)
				{
					darkest = pixels[260 + this.baseTexture.Width];
					medium = pixels[261 + this.baseTexture.Width];
					lightest = pixels[262 + this.baseTexture.Width];
				}
				this._SwapColor(texture_name, pixels, 256, darkest);
				this._SwapColor(texture_name, pixels, 257, medium);
				this._SwapColor(texture_name, pixels, 258, lightest);
				return;
			}
			Color color = Utility.MakeCompletelyOpaque(who.GetShirtColor());
			Color shirtSleeveColor = shirtData[dyeIndex];
			Color clothesColor = color;
			if (shirtSleeveColor.A < 255)
			{
				shirtSleeveColor = shirtData[index];
				clothesColor = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothesColor);
			this._SwapColor(texture_name, pixels, 256, shirtSleeveColor);
			shirtSleeveColor = shirtData[dyeIndex - shirtTexture.Width];
			if (shirtSleeveColor.A < 255)
			{
				shirtSleeveColor = shirtData[index - shirtTexture.Width];
				clothesColor = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothesColor);
			this._SwapColor(texture_name, pixels, 257, shirtSleeveColor);
			shirtSleeveColor = shirtData[dyeIndex - shirtTexture.Width * 2];
			if (shirtSleeveColor.A < 255)
			{
				shirtSleeveColor = shirtData[index - shirtTexture.Width * 2];
				clothesColor = Color.White;
			}
			shirtSleeveColor = Utility.MultiplyColor(shirtSleeveColor, clothesColor);
			this._SwapColor(texture_name, pixels, 258, shirtSleeveColor);
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00066488 File Offset: 0x00064688
		public static Color changeBrightness(Color c, int brightness)
		{
			c.R = (byte)Math.Min(255, Math.Max(0, (int)c.R + brightness));
			c.G = (byte)Math.Min(255, Math.Max(0, (int)c.G + brightness));
			c.B = (byte)Math.Min(255, Math.Max(0, (int)c.B + ((brightness > 0) ? (brightness * 5 / 6) : (brightness * 8 / 7))));
			return c;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00066508 File Offset: 0x00064708
		public void draw(SpriteBatch b, Farmer who, int whichFrame, Vector2 position, float layerDepth = 1f, bool flip = false)
		{
			who.FarmerSprite.setCurrentSingleFrame(whichFrame, 32000, false, flip);
			this.draw(b, who.FarmerSprite, who.FarmerSprite.SourceRect, position, Vector2.Zero, layerDepth, Color.White, 0f, who);
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x00066558 File Offset: 0x00064758
		public void draw(SpriteBatch b, FarmerSprite farmerSprite, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, Color overrideColor, float rotation, Farmer who)
		{
			this.draw(b, farmerSprite.CurrentAnimationFrame, farmerSprite.CurrentFrame, sourceRect, position, origin, layerDepth, overrideColor, rotation, 1f, who);
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x0006658C File Offset: 0x0006478C
		public void drawMiniPortrat(SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who, float alpha = 1f)
		{
			int hairStyle = who.getHair(true);
			this.executeRecolorActions(who);
			facingDirection = 2;
			bool flip = false;
			int yOffset = 0;
			int featureYOffset = 0;
			HairStyleMetadata hairMetadata = Farmer.GetHairStyleMetadata(who.hair.Value);
			Texture2D hairTexture = ((hairMetadata != null) ? hairMetadata.texture : null) ?? FarmerRenderer.hairStylesTexture;
			this.hairstyleSourceRect = ((hairMetadata != null) ? new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 15) : new Rectangle(hairStyle * 16 % FarmerRenderer.hairStylesTexture.Width, hairStyle * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 15));
			if (facingDirection == 2)
			{
				yOffset = 0;
				this.hairstyleSourceRect.Offset(0, 0);
				featureYOffset = FarmerRenderer.featureYOffsetPerFrame[0];
			}
			b.Draw(this.baseTexture, position, new Rectangle?(new Rectangle(0, yOffset, 16, who.IsMale ? 15 : 16)), Color.White * alpha, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base, false));
			Color hairColor = who.prismaticHair.Value ? Utility.GetPrismaticColor(0, 1f) : who.hairstyleColor.Value;
			b.Draw(hairTexture, position + new Vector2(0f, (float)(featureYOffset * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0)))) * scale / 4f, new Rectangle?(this.hairstyleSourceRect), hairColor * alpha, 0f, Vector2.Zero, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair, false));
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x00066760 File Offset: 0x00064960
		public void draw(SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, Color overrideColor, float rotation, float scale, Farmer who)
		{
			this.draw(b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, who.FacingDirection, overrideColor, rotation, scale, who);
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00066790 File Offset: 0x00064990
		public void drawHairAndAccesories(SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
		{
			int hairStyle = who.getHair(false);
			float scaledPixelZoom = 4f * scale;
			int frameXOffset = FarmerRenderer.featureXOffsetPerFrame[currentFrame];
			int frameYOffset = FarmerRenderer.featureYOffsetPerFrame[currentFrame];
			HairStyleMetadata hairMetadata = Farmer.GetHairStyleMetadata(hairStyle);
			Hat value = who.hat.Value;
			if (value != null && value.hairDrawType.Value == 1 && hairMetadata != null && hairMetadata.coveredIndex != -1)
			{
				hairStyle = hairMetadata.coveredIndex;
				hairMetadata = Farmer.GetHairStyleMetadata(hairStyle);
			}
			this.executeRecolorActions(who);
			Texture2D shirtTexture;
			int shirtIndex;
			who.GetDisplayShirt(out shirtTexture, out shirtIndex);
			Color hairColor = who.prismaticHair.Value ? Utility.GetPrismaticColor(0, 1f) : who.hairstyleColor.Value;
			this.shirtSourceRect = new Rectangle(shirtIndex * 8 % 128, shirtIndex * 8 / 128 * 32, 8, 8);
			Texture2D hairTexture = ((hairMetadata != null) ? hairMetadata.texture : null) ?? FarmerRenderer.hairStylesTexture;
			this.hairstyleSourceRect = ((hairMetadata != null) ? new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 32) : new Rectangle(hairStyle * 16 % FarmerRenderer.hairStylesTexture.Width, hairStyle * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32));
			if (who.accessory.Value >= 0)
			{
				this.accessorySourceRect = new Rectangle(who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
			}
			Texture2D hatTexture = FarmerRenderer.hatsTexture;
			bool isErrorHat = false;
			if (who.hat.Value != null)
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(who.hat.Value.QualifiedItemId);
				int spriteIndex = itemData.SpriteIndex;
				hatTexture = itemData.GetTexture();
				this.hatSourceRect = new Rectangle(20 * spriteIndex % hatTexture.Width, 20 * spriteIndex / hatTexture.Width * 20 * 4, 20, 20);
				if (itemData.IsErrorItem)
				{
					this.hatSourceRect = itemData.GetSourceRect(0, null);
					isErrorHat = true;
				}
			}
			FarmerRenderer.FarmerSpriteLayers accessoryLayer = FarmerRenderer.FarmerSpriteLayers.Accessory;
			if (who.accessory.Value >= 0 && this.drawAccessoryBelowHair(who.accessory.Value))
			{
				accessoryLayer = FarmerRenderer.FarmerSpriteLayers.AccessoryUnderHair;
			}
			switch (facingDirection)
			{
			case 0:
			{
				this.shirtSourceRect.Offset(0, 24);
				this.hairstyleSourceRect.Offset(0, 64);
				Rectangle dyedShirtSourceRect = this.shirtSourceRect;
				dyedShirtSourceRect.Offset(128, 0);
				if (!isErrorHat && who.hat.Value != null)
				{
					this.hatSourceRect.Offset(0, 60);
				}
				if (!who.bathingClothes.Value && (this.skin.Value != -12345 || who.shirtItem.Value != null))
				{
					Vector2 shirtPosition = position + origin + this.positionOffset + new Vector2(16f * scale + (float)(frameXOffset * 4), (float)(56 + frameYOffset * 4) + (float)this.heightOffset.Value * scale);
					b.Draw(shirtTexture, shirtPosition, new Rectangle?(this.shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, false));
					b.Draw(shirtTexture, shirtPosition, new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, true));
				}
				b.Draw(hairTexture, position + origin + this.positionOffset + new Vector2((float)(frameXOffset * 4), (float)(frameYOffset * 4 + 4 + ((who.IsMale && hairStyle >= 16) ? -4 : ((!who.IsMale && hairStyle < 16) ? 4 : 0)))), new Rectangle?(this.hairstyleSourceRect), overrideColor.Equals(Color.White) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair, false));
				break;
			}
			case 1:
			{
				this.shirtSourceRect.Offset(0, 8);
				this.hairstyleSourceRect.Offset(0, 32);
				Rectangle dyedShirtSourceRect = this.shirtSourceRect;
				dyedShirtSourceRect.Offset(128, 0);
				if (!isErrorHat && who.hat.Value != null)
				{
					this.hatSourceRect.Offset(0, 20);
				}
				if (rotation != -0.09817477f)
				{
					if (rotation == 0.09817477f)
					{
						this.rotationAdjustment.X = -6f;
						this.rotationAdjustment.Y = 1f;
					}
				}
				else
				{
					this.rotationAdjustment.X = 6f;
					this.rotationAdjustment.Y = -2f;
				}
				if (!who.bathingClothes.Value && (this.skin.Value != -12345 || who.shirtItem.Value != null))
				{
					Vector2 shirtPosition2 = position + origin + this.positionOffset + this.rotationAdjustment + new Vector2(16f * scale + (float)(frameXOffset * 4), 56f * scale + (float)(frameYOffset * 4) + (float)this.heightOffset.Value * scale);
					b.Draw(shirtTexture, shirtPosition2, new Rectangle?(this.shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, false));
					b.Draw(shirtTexture, shirtPosition2, new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, true));
				}
				if (who.accessory.Value >= 0)
				{
					this.accessorySourceRect.Offset(0, 16);
					b.Draw(FarmerRenderer.accessoriesTexture, position + origin + this.positionOffset + this.rotationAdjustment + new Vector2((float)(frameXOffset * 4), (float)(4 + frameYOffset * 4 + this.heightOffset.Value)), new Rectangle?(this.accessorySourceRect), (overrideColor.Equals(Color.White) && this.isAccessoryFacialHair(who.accessory.Value)) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, accessoryLayer, false));
				}
				b.Draw(hairTexture, position + origin + this.positionOffset + new Vector2((float)(frameXOffset * 4), (float)(frameYOffset * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0)))), new Rectangle?(this.hairstyleSourceRect), overrideColor.Equals(Color.White) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair, false));
				break;
			}
			case 2:
			{
				Rectangle dyedShirtSourceRect = this.shirtSourceRect;
				dyedShirtSourceRect.Offset(128, 0);
				if (!who.bathingClothes.Value && (this.skin.Value != -12345 || who.shirtItem.Value != null))
				{
					Vector2 shirtPosition3 = position + origin + this.positionOffset + new Vector2((float)(16 + frameXOffset * 4), (float)(56 + frameYOffset * 4) + (float)this.heightOffset.Value * scale);
					b.Draw(shirtTexture, shirtPosition3, new Rectangle?(this.shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, false));
					b.Draw(shirtTexture, shirtPosition3, new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, true));
				}
				if (who.accessory.Value >= 0)
				{
					if (who.accessory.Value == 26 && (currentFrame == 70 || (currentFrame > 23 && currentFrame < 27)))
					{
						this.positionOffset.Y = this.positionOffset.Y + 4f;
					}
					b.Draw(FarmerRenderer.accessoriesTexture, position + origin + this.positionOffset + this.rotationAdjustment + new Vector2((float)(frameXOffset * 4), (float)(8 + frameYOffset * 4 + this.heightOffset.Value - 4)), new Rectangle?(this.accessorySourceRect), (overrideColor.Equals(Color.White) && this.isAccessoryFacialHair(who.accessory.Value)) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, accessoryLayer, false));
				}
				b.Draw(hairTexture, position + origin + this.positionOffset + new Vector2((float)(frameXOffset * 4), (float)(frameYOffset * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0)))), new Rectangle?(this.hairstyleSourceRect), overrideColor.Equals(Color.White) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair, false));
				break;
			}
			case 3:
			{
				bool flip = true;
				this.shirtSourceRect.Offset(0, 16);
				Rectangle dyedShirtSourceRect = this.shirtSourceRect;
				dyedShirtSourceRect.Offset(128, 0);
				if (hairMetadata != null && hairMetadata.usesUniqueLeftSprite)
				{
					flip = false;
					this.hairstyleSourceRect.Offset(0, 96);
				}
				else
				{
					this.hairstyleSourceRect.Offset(0, 32);
				}
				if (!isErrorHat && who.hat.Value != null)
				{
					this.hatSourceRect.Offset(0, 40);
				}
				if (rotation != -0.09817477f)
				{
					if (rotation == 0.09817477f)
					{
						this.rotationAdjustment.X = -5f;
						this.rotationAdjustment.Y = 1f;
					}
				}
				else
				{
					this.rotationAdjustment.X = 6f;
					this.rotationAdjustment.Y = -2f;
				}
				if (!who.bathingClothes.Value && (this.skin.Value != -12345 || who.shirtItem.Value != null))
				{
					Vector2 shirtPosition4 = position + origin + this.positionOffset + this.rotationAdjustment + new Vector2(16f * scale - (float)(frameXOffset * 4), 56f * scale + (float)(frameYOffset * 4) + (float)this.heightOffset.Value * scale);
					b.Draw(shirtTexture, shirtPosition4, new Rectangle?(this.shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, false));
					b.Draw(shirtTexture, shirtPosition4, new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt, true));
				}
				if (who.accessory.Value >= 0)
				{
					this.accessorySourceRect.Offset(0, 16);
					b.Draw(FarmerRenderer.accessoriesTexture, position + origin + this.positionOffset + this.rotationAdjustment + new Vector2((float)(-(float)frameXOffset * 4), (float)(4 + frameYOffset * 4 + this.heightOffset.Value)), new Rectangle?(this.accessorySourceRect), (overrideColor.Equals(Color.White) && this.isAccessoryFacialHair(who.accessory.Value)) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.FlipHorizontally, FarmerRenderer.GetLayerDepth(layerDepth, accessoryLayer, false));
				}
				b.Draw(hairTexture, position + origin + this.positionOffset + new Vector2((float)(-(float)frameXOffset * 4), (float)(frameYOffset * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0)))), new Rectangle?(this.hairstyleSourceRect), overrideColor.Equals(Color.White) ? hairColor : overrideColor, rotation, origin, scaledPixelZoom, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair, false));
				break;
			}
			}
			if (who.hat.Value != null && !who.bathingClothes.Value)
			{
				bool flip2 = who.FarmerSprite.CurrentAnimationFrame.flip;
				int hatOffset = who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16];
				Vector2 hatPosition = position + origin + this.positionOffset + new Vector2(-scaledPixelZoom * 2f + (float)((flip2 ? -1 : 1) * frameXOffset) * scaledPixelZoom, -scaledPixelZoom * 4f + (float)(frameYOffset * 4) + (float)hatOffset + 4f + (float)this.heightOffset.Value);
				Color hatColor = who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor(0, 1f) : overrideColor;
				if (!isErrorHat && who.hat.Value.isMask && facingDirection == 0)
				{
					Rectangle maskDrawRect = this.hatSourceRect;
					maskDrawRect.Height -= 11;
					maskDrawRect.Y += 11;
					b.Draw(hatTexture, position + origin + this.positionOffset + new Vector2(0f, 11f * scaledPixelZoom) + new Vector2(-scaledPixelZoom * 2f + (float)((flip2 ? -1 : 1) * frameXOffset * 4), (float)(-16 + frameYOffset * 4 + hatOffset + 4 + this.heightOffset.Value)), new Rectangle?(maskDrawRect), overrideColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hat, false));
					maskDrawRect = this.hatSourceRect;
					maskDrawRect.Height = 11;
					b.Draw(hatTexture, hatPosition, new Rectangle?(maskDrawRect), hatColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.HatMaskUp, false));
					return;
				}
				b.Draw(hatTexture, hatPosition, new Rectangle?(this.hatSourceRect), hatColor, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hat, false));
			}
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x00067624 File Offset: 0x00065824
		public static float GetLayerDepth(float baseLayerDepth, FarmerRenderer.FarmerSpriteLayers layer, bool dyeLayer = false)
		{
			if (layer == FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE)
			{
				return baseLayerDepth + 0.0032f;
			}
			int sortDirection = Game1.isUsingBackToFrontSorting ? -1 : 1;
			if (dyeLayer)
			{
				baseLayerDepth += 1E-07f * (float)sortDirection;
			}
			return baseLayerDepth + (float)layer * 1E-06f * (float)sortDirection;
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x00067668 File Offset: 0x00065868
		public void draw(SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
		{
			float scaledPixelZoom = 4f * scale;
			int frameXOffset = FarmerRenderer.featureXOffsetPerFrame[currentFrame];
			int frameYOffset = FarmerRenderer.featureYOffsetPerFrame[currentFrame];
			bool sickFrame = currentFrame == 104 || currentFrame == 105;
			if (this._sickFrame != sickFrame)
			{
				this._sickFrame = sickFrame;
				this._shirtDirty = true;
				this._spriteDirty = true;
			}
			this.executeRecolorActions(who);
			position = new Vector2((float)Math.Floor((double)position.X), (float)Math.Floor((double)position.Y));
			this.rotationAdjustment = Vector2.Zero;
			this.positionOffset.Y = (float)(animationFrame.positionOffset * 4);
			this.positionOffset.X = (float)(animationFrame.xOffset * 4);
			if (!FarmerRenderer.isDrawingForUI && who.swimming.Value)
			{
				sourceRect.Height /= 2;
				sourceRect.Height -= (int)who.yOffset / 4;
				position.Y += 64f;
			}
			if (facingDirection == 3 || facingDirection == 1)
			{
				facingDirection = (animationFrame.flip ? 3 : 1);
			}
			b.Draw(this.baseTexture, position + origin + this.positionOffset, new Rectangle?(sourceRect), overrideColor, rotation, origin, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base, false));
			if (!FarmerRenderer.isDrawingForUI && who.swimming.Value)
			{
				if (who.currentEyes != 0 && who.FacingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
				{
					Vector2 eyePosition = position + origin + this.positionOffset + new Vector2((float)(frameXOffset * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0))), (float)(frameYOffset * 4 + 40));
					b.Draw(this.baseTexture, eyePosition, new Rectangle?(new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.FaceSkin, false));
					b.Draw(this.baseTexture, eyePosition, new Rectangle?(new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Eyes, false));
				}
				this.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
				b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), new Rectangle?(Game1.staminaRect.Bounds), Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.SwimWaterRing, false));
				return;
			}
			Texture2D texture;
			int pantsIndex;
			who.GetDisplayPants(out texture, out pantsIndex);
			Rectangle pantsRect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
			pantsRect.X += pantsIndex % 10 * 192;
			pantsRect.Y += pantsIndex / 10 * 688;
			if (!who.IsMale)
			{
				pantsRect.X += 96;
			}
			if (this.skin.Value != -12345 || who.pantsItem.Value != null)
			{
				b.Draw(texture, position + origin + this.positionOffset, new Rectangle?(pantsRect), (overrideColor == Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, rotation, origin, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, (who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? FarmerRenderer.FarmerSpriteLayers.PantsPassedOut : FarmerRenderer.FarmerSpriteLayers.Pants, false));
			}
			sourceRect.Offset(288, 0);
			if (who.currentEyes != 0 && facingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
			{
				if (who.UsingTool)
				{
					FishingRod fishingRod = who.CurrentTool as FishingRod;
					if (fishingRod != null && !fishingRod.isFishing)
					{
						goto IL_65A;
					}
				}
				int xAdjustment = 5;
				if (!animationFrame.flip)
				{
					xAdjustment += frameXOffset;
				}
				else
				{
					xAdjustment -= frameXOffset;
				}
				if (facingDirection != 1)
				{
					if (facingDirection == 3)
					{
						xAdjustment++;
					}
				}
				else
				{
					xAdjustment += 3;
				}
				xAdjustment *= 4;
				b.Draw(this.baseTexture, position + origin + this.positionOffset + new Vector2((float)xAdjustment, (float)(frameYOffset * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40))), new Rectangle?(new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.FaceSkin, false));
				b.Draw(this.baseTexture, position + origin + this.positionOffset + new Vector2((float)xAdjustment, (float)(frameYOffset * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44))), new Rectangle?(new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2)), overrideColor, 0f, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Eyes, false));
			}
			IL_65A:
			this.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
			FarmerRenderer.FarmerSpriteLayers armLayer = FarmerRenderer.FarmerSpriteLayers.Arms;
			if (facingDirection == 0)
			{
				armLayer = FarmerRenderer.FarmerSpriteLayers.ArmsUp;
			}
			if (animationFrame.armOffset > 0)
			{
				sourceRect.Offset(-288 + animationFrame.armOffset * 16, 0);
				b.Draw(this.baseTexture, position + origin + this.positionOffset + who.armOffset, new Rectangle?(sourceRect), overrideColor, rotation, origin, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, armLayer, false));
			}
			if (who.usingSlingshot)
			{
				Slingshot slingshot = who.CurrentTool as Slingshot;
				if (slingshot != null)
				{
					Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value), true));
					int mouseX = point.X;
					float y = (float)point.Y;
					int backArmDistance = slingshot.GetBackArmDistance(who);
					Vector2 shootOrigin = slingshot.GetShootOrigin(who);
					float frontArmRotation = (float)Math.Atan2((double)(y - shootOrigin.Y), (double)((float)mouseX - shootOrigin.X)) + 3.1415927f;
					if (!Game1.options.useLegacySlingshotFiring)
					{
						frontArmRotation -= 3.1415927f;
						if (frontArmRotation < 0f)
						{
							frontArmRotation += 6.2831855f;
						}
					}
					switch (facingDirection)
					{
					case 0:
						b.Draw(this.baseTexture, position + new Vector2(4f + frontArmRotation * 8f, -44f), new Rectangle?(new Rectangle(173, 238, 9, 14)), Color.White, 0f, new Vector2(4f, 11f), scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.SlingshotUp, false));
						return;
					case 1:
					{
						b.Draw(this.baseTexture, position + new Vector2((float)(52 - backArmDistance), -32f), new Rectangle?(new Rectangle(147, 237, 10, 4)), Color.White, 0f, new Vector2(8f, 3f), scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Slingshot, false));
						b.Draw(this.baseTexture, position + new Vector2(36f, -44f), new Rectangle?(new Rectangle(156, 244, 9, 10)), Color.White, frontArmRotation, new Vector2(0f, 3f), scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.SlingshotUp, false));
						int slingshotAttachX = (int)(Math.Cos((double)(frontArmRotation + 1.5707964f)) * (double)(20 - backArmDistance - 8) - Math.Sin((double)(frontArmRotation + 1.5707964f)) * -68.0);
						int slingshotAttachY = (int)(Math.Sin((double)(frontArmRotation + 1.5707964f)) * (double)(20 - backArmDistance - 8) + Math.Cos((double)(frontArmRotation + 1.5707964f)) * -68.0);
						Utility.drawLineWithScreenCoordinates((int)(position.X + 52f - (float)backArmDistance), (int)(position.Y - 32f - 4f), (int)(position.X + 32f + (float)(slingshotAttachX / 2)), (int)(position.Y - 32f - 12f + (float)(slingshotAttachY / 2)), b, Color.White, 1f, 1);
						return;
					}
					case 2:
						b.Draw(this.baseTexture, position + new Vector2(4f, (float)(-32 - backArmDistance / 2)), new Rectangle?(new Rectangle(148, 244, 4, 4)), Color.White, 0f, Vector2.Zero, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Arms, false));
						Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 44f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White, 1f, 1);
						Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 56f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White, 1f, 1);
						b.Draw(this.baseTexture, position + new Vector2(44f - frontArmRotation * 10f, -16f), new Rectangle?(new Rectangle(167, 235, 7, 9)), Color.White, 0f, new Vector2(3f, 5f), scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Slingshot, true));
						break;
					case 3:
					{
						b.Draw(this.baseTexture, position + new Vector2((float)(40 + backArmDistance), -32f), new Rectangle?(new Rectangle(147, 237, 10, 4)), Color.White, 0f, new Vector2(9f, 4f), scaledPixelZoom, SpriteEffects.FlipHorizontally, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Slingshot, false));
						b.Draw(this.baseTexture, position + new Vector2(24f, -40f), new Rectangle?(new Rectangle(156, 244, 9, 10)), Color.White, frontArmRotation + 3.1415927f, new Vector2(8f, 3f), scaledPixelZoom, SpriteEffects.FlipHorizontally, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.SlingshotUp, false));
						int slingshotAttachX = (int)(Math.Cos((double)(frontArmRotation + 1.2566371f)) * (double)(20 + backArmDistance - 8) - Math.Sin((double)(frontArmRotation + 1.2566371f)) * -68.0);
						int slingshotAttachY = (int)(Math.Sin((double)(frontArmRotation + 1.2566371f)) * (double)(20 + backArmDistance - 8) + Math.Cos((double)(frontArmRotation + 1.2566371f)) * -68.0);
						Utility.drawLineWithScreenCoordinates((int)(position.X + 4f + (float)backArmDistance), (int)(position.Y - 32f - 8f), (int)(position.X + 26f + (float)slingshotAttachX * 4f / 10f), (int)(position.Y - 32f - 8f + (float)slingshotAttachY * 4f / 10f), b, Color.White, 1f, 1);
						return;
					}
					default:
						return;
					}
				}
			}
		}

		// Token: 0x040005A5 RID: 1445
		public const int sleeveDarkestColorIndex = 256;

		// Token: 0x040005A6 RID: 1446
		public const int skinDarkestColorIndex = 260;

		// Token: 0x040005A7 RID: 1447
		public const int shoeDarkestColorIndex = 268;

		// Token: 0x040005A8 RID: 1448
		public const int eyeLightestColorIndex = 276;

		// Token: 0x040005A9 RID: 1449
		public const int accessoryDrawBelowHairThreshold = 8;

		// Token: 0x040005AA RID: 1450
		public const int accessoryFacialHairThreshold = 6;

		// Token: 0x040005AB RID: 1451
		protected bool _sickFrame;

		// Token: 0x040005AC RID: 1452
		public static bool isDrawingForUI = false;

		// Token: 0x040005AD RID: 1453
		public const int TransparentSkin = -12345;

		// Token: 0x040005AE RID: 1454
		public const int pantsOffset = 288;

		// Token: 0x040005AF RID: 1455
		public const int armOffset = 96;

		// Token: 0x040005B0 RID: 1456
		public const int shirtXOffset = 16;

		// Token: 0x040005B1 RID: 1457
		public const int shirtYOffset = 56;

		// Token: 0x040005B2 RID: 1458
		public static int[] featureYOffsetPerFrame = new int[]
		{
			1,
			2,
			2,
			0,
			5,
			6,
			1,
			2,
			2,
			1,
			0,
			2,
			0,
			1,
			1,
			0,
			2,
			2,
			3,
			3,
			2,
			2,
			1,
			1,
			0,
			0,
			2,
			2,
			4,
			4,
			0,
			0,
			1,
			2,
			1,
			1,
			1,
			1,
			0,
			0,
			1,
			1,
			1,
			0,
			0,
			-2,
			-1,
			1,
			1,
			0,
			-1,
			-2,
			-1,
			-1,
			5,
			4,
			0,
			0,
			3,
			2,
			-1,
			0,
			4,
			2,
			0,
			0,
			2,
			1,
			0,
			-1,
			1,
			-2,
			0,
			0,
			1,
			1,
			1,
			1,
			1,
			1,
			0,
			0,
			0,
			0,
			1,
			-1,
			-1,
			-1,
			-1,
			1,
			1,
			0,
			0,
			0,
			0,
			4,
			1,
			0,
			1,
			2,
			1,
			0,
			1,
			0,
			1,
			2,
			-3,
			-4,
			-1,
			0,
			0,
			2,
			1,
			-4,
			-1,
			0,
			0,
			-3,
			0,
			0,
			-1,
			0,
			0,
			2,
			1,
			1
		};

		// Token: 0x040005B3 RID: 1459
		public static int[] featureXOffsetPerFrame = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			-1,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			4,
			0,
			0,
			0,
			0,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};

		// Token: 0x040005B4 RID: 1460
		public static int[] hairstyleHatOffset = new int[]
		{
			0,
			0,
			0,
			4,
			0,
			0,
			3,
			0,
			4,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};

		// Token: 0x040005B5 RID: 1461
		public static Texture2D hairStylesTexture;

		// Token: 0x040005B6 RID: 1462
		public static Texture2D shirtsTexture;

		// Token: 0x040005B7 RID: 1463
		public static Texture2D hatsTexture;

		// Token: 0x040005B8 RID: 1464
		public static Texture2D accessoriesTexture;

		// Token: 0x040005B9 RID: 1465
		public static Texture2D pantsTexture;

		// Token: 0x040005BA RID: 1466
		public static Dictionary<string, Dictionary<int, List<int>>> recolorOffsets;

		// Token: 0x040005BB RID: 1467
		[XmlElement("textureName")]
		public readonly NetString textureName = new NetString();

		// Token: 0x040005BC RID: 1468
		[XmlIgnore]
		private LocalizedContentManager farmerTextureManager;

		// Token: 0x040005BD RID: 1469
		[XmlIgnore]
		internal Texture2D baseTexture;

		// Token: 0x040005BE RID: 1470
		[XmlElement("heightOffset")]
		public readonly NetInt heightOffset = new NetInt(0);

		// Token: 0x040005BF RID: 1471
		[XmlIgnore]
		public readonly NetColor eyes = new NetColor();

		// Token: 0x040005C0 RID: 1472
		[XmlIgnore]
		public readonly NetInt skin = new NetInt();

		// Token: 0x040005C1 RID: 1473
		[XmlIgnore]
		public readonly NetString shoes = new NetString();

		// Token: 0x040005C2 RID: 1474
		[XmlIgnore]
		public readonly NetString shirt = new NetString();

		// Token: 0x040005C3 RID: 1475
		[XmlIgnore]
		public readonly NetString pants = new NetString();

		// Token: 0x040005C5 RID: 1477
		protected bool _spriteDirty;

		// Token: 0x040005C6 RID: 1478
		protected bool _baseTextureDirty;

		// Token: 0x040005C7 RID: 1479
		protected bool _eyesDirty;

		// Token: 0x040005C8 RID: 1480
		protected bool _skinDirty;

		// Token: 0x040005C9 RID: 1481
		protected bool _shoesDirty;

		// Token: 0x040005CA RID: 1482
		protected bool _shirtDirty;

		// Token: 0x040005CB RID: 1483
		protected bool _pantsDirty;

		// Token: 0x040005CC RID: 1484
		public Rectangle shirtSourceRect;

		// Token: 0x040005CD RID: 1485
		public Rectangle hairstyleSourceRect;

		// Token: 0x040005CE RID: 1486
		public Rectangle hatSourceRect;

		// Token: 0x040005CF RID: 1487
		public Rectangle accessorySourceRect;

		// Token: 0x040005D0 RID: 1488
		public Vector2 rotationAdjustment;

		// Token: 0x040005D1 RID: 1489
		public Vector2 positionOffset;

		// Token: 0x0200042B RID: 1067
		public enum FarmerSpriteLayers
		{
			// Token: 0x0400275E RID: 10078
			SlingshotUp,
			// Token: 0x0400275F RID: 10079
			ToolUp,
			// Token: 0x04002760 RID: 10080
			Base,
			// Token: 0x04002761 RID: 10081
			Pants,
			// Token: 0x04002762 RID: 10082
			FaceSkin,
			// Token: 0x04002763 RID: 10083
			Eyes,
			// Token: 0x04002764 RID: 10084
			Shirt,
			// Token: 0x04002765 RID: 10085
			AccessoryUnderHair,
			// Token: 0x04002766 RID: 10086
			ArmsUp,
			// Token: 0x04002767 RID: 10087
			HatMaskUp,
			// Token: 0x04002768 RID: 10088
			Hair,
			// Token: 0x04002769 RID: 10089
			Accessory,
			// Token: 0x0400276A RID: 10090
			Hat,
			// Token: 0x0400276B RID: 10091
			Tool,
			// Token: 0x0400276C RID: 10092
			Arms,
			// Token: 0x0400276D RID: 10093
			ToolDown,
			// Token: 0x0400276E RID: 10094
			Slingshot,
			// Token: 0x0400276F RID: 10095
			PantsPassedOut,
			// Token: 0x04002770 RID: 10096
			SwimWaterRing,
			// Token: 0x04002771 RID: 10097
			MAX,
			// Token: 0x04002772 RID: 10098
			TOOL_IN_USE_SIDE
		}
	}
}
