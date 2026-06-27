using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x0200009D RID: 157
	public class Prop
	{
		// Token: 0x0600077F RID: 1919 RVA: 0x00049B94 File Offset: 0x00047D94
		public Prop(Texture2D texture, int index, int tilesWideSolid, int tilesHighSolid, int tilesHighDraw, int tileX, int tileY, bool solid = true)
		{
			this.texture = texture;
			this.sourceRect = Game1.getSourceRectForStandardTileSheet(texture, index, 16, 16);
			this.sourceRect.Width = tilesWideSolid * 16;
			this.sourceRect.Height = tilesHighDraw * 16;
			this.drawRect = new Rectangle(tileX * 64, tileY * 64 + (tilesHighSolid - tilesHighDraw) * 64, tilesWideSolid * 64, tilesHighDraw * 64);
			this.boundingRect = new Rectangle(tileX * 64, tileY * 64, tilesWideSolid * 64, tilesHighSolid * 64);
			this.solid = solid;
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00049C2B File Offset: 0x00047E2B
		public bool isColliding(Rectangle r)
		{
			return this.solid && r.Intersects(this.boundingRect);
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00049C44 File Offset: 0x00047E44
		public void draw(SpriteBatch b)
		{
			this.drawRect.X = this.boundingRect.X - Game1.viewport.X;
			this.drawRect.Y = this.boundingRect.Y + (this.boundingRect.Height - this.drawRect.Height) - Game1.viewport.Y;
			b.Draw(this.texture, this.drawRect, new Rectangle?(this.sourceRect), Color.White, 0f, Vector2.Zero, SpriteEffects.None, this.solid ? ((float)this.boundingRect.Y / 10000f) : 0f);
		}

		// Token: 0x040003F3 RID: 1011
		private Texture2D texture;

		// Token: 0x040003F4 RID: 1012
		private Rectangle sourceRect;

		// Token: 0x040003F5 RID: 1013
		private Rectangle drawRect;

		// Token: 0x040003F6 RID: 1014
		private Rectangle boundingRect;

		// Token: 0x040003F7 RID: 1015
		private bool solid;
	}
}
