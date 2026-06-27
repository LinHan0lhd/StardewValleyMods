using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	// Token: 0x020001B4 RID: 436
	public class RandomizedPlantFurniture : Furniture
	{
		// Token: 0x06001F25 RID: 7973 RVA: 0x0016640C File Offset: 0x0016460C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.topIndex, "topIndex").AddField(this.middleIndex, "middleIndex").AddField(this.bottomIndex, "bottomIndex");
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x0016644B File Offset: 0x0016464B
		protected override Item GetOneNew()
		{
			return new RandomizedPlantFurniture(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x00166464 File Offset: 0x00164664
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			RandomizedPlantFurniture plant = source as RandomizedPlantFurniture;
			if (plant != null)
			{
				this.topIndex.Value = plant.topIndex.Value;
				this.middleIndex.Value = plant.middleIndex.Value;
				this.bottomIndex.Value = plant.bottomIndex.Value;
			}
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x001664C4 File Offset: 0x001646C4
		public RandomizedPlantFurniture(string which, Vector2 tile) : this(which, tile, Game1.random.Next())
		{
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x001664D8 File Offset: 0x001646D8
		public RandomizedPlantFurniture(string which, Vector2 tile, int random_seed)
		{
			this.topIndex = new NetInt();
			this.middleIndex = new NetInt();
			this.bottomIndex = new NetInt();
			base..ctor(which, tile);
			Random r = Utility.CreateRandom((double)random_seed, 0.0, 0.0, 0.0, 0.0);
			this.topIndex.Value = r.Next(24);
			this.middleIndex.Value = r.Next(24);
			this.bottomIndex.Value = r.Next(16);
		}

		// Token: 0x06001F2A RID: 7978 RVA: 0x00166573 File Offset: 0x00164773
		public RandomizedPlantFurniture()
		{
			this.topIndex = new NetInt();
			this.middleIndex = new NetInt();
			this.bottomIndex = new NetInt();
			base..ctor();
		}

		// Token: 0x06001F2B RID: 7979 RVA: 0x0016659C File Offset: 0x0016479C
		protected override float getScaleSize()
		{
			return 1.5f;
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x001665A4 File Offset: 0x001647A4
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			location += new Vector2(32f, 32f);
			this.DrawFurniture(spriteBatch, location, transparency, new Vector2(8f, 0f), this.getScaleSize() * scaleSize, layerDepth);
			if (((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != 2147483647)
			{
				Utility.drawTinyDigits(this.stack.Value, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack.Value, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
			}
		}

		// Token: 0x06001F2D RID: 7981 RVA: 0x00166689 File Offset: 0x00164889
		public override bool IsHeldOverHead()
		{
			return true;
		}

		// Token: 0x06001F2E RID: 7982 RVA: 0x0016668C File Offset: 0x0016488C
		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			this.DrawFurniture(spriteBatch, objectPosition, 4f, Vector2.Zero, 4f, (float)(f.StandingPixel.Y + 3) / 10000f);
		}

		// Token: 0x06001F2F RID: 7983 RVA: 0x001666BC File Offset: 0x001648BC
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			if (Furniture.isDrawingLocationFurniture)
			{
				x = (int)this.drawPosition.X;
				y = (int)this.drawPosition.Y;
			}
			else
			{
				x *= 64;
				y *= 64;
			}
			if (this.shakeTimer > 0)
			{
				x += Game1.random.Next(-1, 2);
				y += Game1.random.Next(-1, 2);
			}
			this.DrawFurniture(spriteBatch, Game1.GlobalToLocal(new Vector2((float)x, (float)y)), alpha, Vector2.Zero, 4f, (float)(this.boundingBox.Value.Bottom - 8) / 10000f);
		}

		// Token: 0x06001F30 RID: 7984 RVA: 0x00166769 File Offset: 0x00164969
		public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
		{
			this.DrawFurniture(spriteBatch, location, 1f, Vector2.Zero, 4f, layerDepth);
		}

		// Token: 0x06001F31 RID: 7985 RVA: 0x00166784 File Offset: 0x00164984
		public virtual void DrawFurniture(SpriteBatch sb, Vector2 location, float alpha, Vector2 origin, float scale, float base_sort_y)
		{
			Texture2D texture = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetTexture();
			Rectangle drawn_source_rect = new Rectangle(0, 96, 16, 16);
			drawn_source_rect.X += this.bottomIndex.Value % 8 * 16;
			drawn_source_rect.Y += this.bottomIndex.Value / 8 * 16;
			sb.Draw(texture, location, new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, origin, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base_sort_y);
			float offset_x = -1f * scale;
			drawn_source_rect = new Rectangle(0, 48, 16, 16);
			drawn_source_rect.X += this.middleIndex.Value % 8 * 16;
			drawn_source_rect.Y += this.middleIndex.Value / 8 * 16;
			sb.Draw(texture, location + new Vector2(offset_x, -8f * scale), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, origin, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base_sort_y + 1E-05f);
			drawn_source_rect = new Rectangle(0, 0, 16, 16);
			drawn_source_rect.X += this.topIndex.Value % 8 * 16;
			drawn_source_rect.Y += this.topIndex.Value / 8 * 16;
			sb.Draw(texture, location + new Vector2(offset_x, -24f * scale), new Rectangle?(drawn_source_rect), Color.White * alpha, 0f, origin, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base_sort_y + 1E-05f);
		}

		// Token: 0x04001320 RID: 4896
		public NetInt topIndex;

		// Token: 0x04001321 RID: 4897
		public NetInt middleIndex;

		// Token: 0x04001322 RID: 4898
		public NetInt bottomIndex;
	}
}
