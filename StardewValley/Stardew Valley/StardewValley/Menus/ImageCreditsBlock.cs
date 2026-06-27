using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000248 RID: 584
	internal class ImageCreditsBlock : ICreditsBlock
	{
		// Token: 0x060026E6 RID: 9958 RVA: 0x001B8170 File Offset: 0x001B6370
		public ImageCreditsBlock(Texture2D texture, Rectangle sourceRect, int pixelZoom, int animationFrames)
		{
			this.animationFrames = animationFrames;
			this.clickableComponent = new ClickableTextureComponent(new Rectangle(0, 0, sourceRect.Width * pixelZoom, sourceRect.Height * pixelZoom), texture, sourceRect, (float)pixelZoom, false);
		}

		// Token: 0x060026E7 RID: 9959 RVA: 0x001B81A8 File Offset: 0x001B63A8
		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			b.Draw(this.clickableComponent.texture, new Rectangle(topLeftX + widthToOccupy / 2 - this.clickableComponent.bounds.Width / 2, topLeftY, this.clickableComponent.bounds.Width, this.clickableComponent.bounds.Height), new Rectangle?(new Rectangle(this.clickableComponent.sourceRect.X + this.clickableComponent.sourceRect.Width * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / (double)(600 / this.animationFrames)), this.clickableComponent.sourceRect.Y, this.clickableComponent.sourceRect.Width, this.clickableComponent.sourceRect.Height)), Color.White);
		}

		// Token: 0x060026E8 RID: 9960 RVA: 0x001B8291 File Offset: 0x001B6491
		public override int getHeight(int maxWidth)
		{
			return this.clickableComponent.bounds.Height;
		}

		// Token: 0x04001816 RID: 6166
		private ClickableTextureComponent clickableComponent;

		// Token: 0x04001817 RID: 6167
		private int animationFrames;
	}
}
