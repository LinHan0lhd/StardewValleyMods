using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200025F RID: 607
	public class ClickableTextureComponent : ClickableComponent
	{
		// Token: 0x0600283F RID: 10303 RVA: 0x001D4358 File Offset: 0x001D2558
		public ClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(bounds, name, label)
		{
			this.texture = texture;
			if (sourceRect.Equals(Rectangle.Empty) && texture != null)
			{
				this.sourceRect = texture.Bounds;
			}
			else
			{
				this.sourceRect = sourceRect;
			}
			this.scale = scale;
			this.baseScale = scale;
			this.hoverText = hoverText;
			this.drawShadow = drawShadow;
			this.label = label;
			this.startingSourceRect = sourceRect;
		}

		// Token: 0x06002840 RID: 10304 RVA: 0x001D43E0 File Offset: 0x001D25E0
		public ClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : this("", bounds, "", "", texture, sourceRect, scale, drawShadow)
		{
		}

		// Token: 0x06002841 RID: 10305 RVA: 0x001D4409 File Offset: 0x001D2609
		public Vector2 getVector2()
		{
			return new Vector2((float)this.bounds.X, (float)this.bounds.Y);
		}

		// Token: 0x06002842 RID: 10306 RVA: 0x001D4428 File Offset: 0x001D2628
		public void setPosition(Vector2 position)
		{
			this.setPosition((int)position.X, (int)position.Y);
		}

		// Token: 0x06002843 RID: 10307 RVA: 0x001D443E File Offset: 0x001D263E
		public void setPosition(int x, int y)
		{
			this.bounds.X = x;
			this.bounds.Y = y;
		}

		// Token: 0x06002844 RID: 10308 RVA: 0x001D4458 File Offset: 0x001D2658
		public virtual void tryHover(int x, int y, float maxScaleIncrease = 0.1f)
		{
			if (this.bounds.Contains(x, y))
			{
				this.scale = Math.Min(this.scale + 0.04f, this.baseScale + maxScaleIncrease);
				Game1.SetFreeCursorDrag();
				return;
			}
			this.scale = Math.Max(this.scale - 0.04f, this.baseScale);
		}

		// Token: 0x06002845 RID: 10309 RVA: 0x001D44B6 File Offset: 0x001D26B6
		public virtual void draw(SpriteBatch b)
		{
			if (this.visible)
			{
				this.draw(b, Color.White, 0.86f + (float)this.bounds.Y / 20000f, 0, 0, 0);
			}
		}

		// Token: 0x06002846 RID: 10310 RVA: 0x001D44E8 File Offset: 0x001D26E8
		public virtual void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0, int xOffset = 0, int yOffset = 0)
		{
			if (this.visible)
			{
				if (this.texture != null)
				{
					Rectangle r = this.sourceRect;
					if (frameOffset != 0)
					{
						r = new Rectangle(this.sourceRect.X + this.sourceRect.Width * frameOffset, this.sourceRect.Y, this.sourceRect.Width, this.sourceRect.Height);
					}
					if (this.drawShadow)
					{
						Utility.drawWithShadow(b, this.texture, new Vector2((float)(this.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)(this.bounds.Y + yOffset) + (float)(this.sourceRect.Height / 2) * this.baseScale), r, c, 0f, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.scale, false, layerDepth, -1, -1, 0.35f);
					}
					else
					{
						b.Draw(this.texture, new Vector2((float)(this.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)(this.bounds.Y + yOffset) + (float)(this.sourceRect.Height / 2) * this.baseScale), new Rectangle?(r), c, 0f, new Vector2((float)(this.sourceRect.Width / 2), (float)(this.sourceRect.Height / 2)), this.scale, SpriteEffects.None, layerDepth);
					}
				}
				if (this.drawLabel && !string.IsNullOrEmpty(this.label))
				{
					if (this.drawLabelWithShadow)
					{
						Utility.drawTextWithShadow(b, this.label, Game1.smallFont, new Vector2((float)(this.bounds.X + xOffset + this.bounds.Width), (float)(this.bounds.Y + yOffset) + ((float)(this.bounds.Height / 2) - Game1.smallFont.MeasureString(this.label).Y / 2f)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						return;
					}
					b.DrawString(Game1.smallFont, this.label, new Vector2((float)(this.bounds.X + xOffset + this.bounds.Width), (float)(this.bounds.Y + yOffset) + ((float)(this.bounds.Height / 2) - Game1.smallFont.MeasureString(this.label).Y / 2f)), Game1.textColor);
				}
			}
		}

		// Token: 0x06002847 RID: 10311 RVA: 0x001D47A4 File Offset: 0x001D29A4
		public virtual void drawItem(SpriteBatch b, int xOffset = 0, int yOffset = 0, float alpha = 1f)
		{
			if (this.item != null && this.visible)
			{
				this.item.drawInMenu(b, new Vector2((float)(this.bounds.X + xOffset), (float)(this.bounds.Y + yOffset)), this.scale / 4f, alpha, 0.9f);
			}
		}

		// Token: 0x040019F9 RID: 6649
		public Texture2D texture;

		// Token: 0x040019FA RID: 6650
		public Rectangle sourceRect;

		// Token: 0x040019FB RID: 6651
		public Rectangle startingSourceRect;

		// Token: 0x040019FC RID: 6652
		public float baseScale;

		// Token: 0x040019FD RID: 6653
		public string hoverText = "";

		// Token: 0x040019FE RID: 6654
		public bool drawLabel = true;

		// Token: 0x040019FF RID: 6655
		public bool drawShadow;

		// Token: 0x04001A00 RID: 6656
		public bool drawLabelWithShadow;
	}
}
