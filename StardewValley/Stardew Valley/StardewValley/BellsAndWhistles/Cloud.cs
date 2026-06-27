using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000392 RID: 914
	public class Cloud : Critter
	{
		// Token: 0x06003836 RID: 14390 RVA: 0x002C7CAD File Offset: 0x002C5EAD
		public Cloud()
		{
		}

		// Token: 0x06003837 RID: 14391 RVA: 0x002C7CBC File Offset: 0x002C5EBC
		public Cloud(Vector2 position)
		{
			this.position = position * 64f;
			this.startingPosition = position;
			this.verticalFlip = Game1.random.NextBool();
			this.horizontalFlip = Game1.random.NextBool();
			this.zoom = Game1.random.Next(4, 7);
		}

		// Token: 0x06003838 RID: 14392 RVA: 0x002C7D20 File Offset: 0x002C5F20
		public override bool update(GameTime time, GameLocation environment)
		{
			this.position.Y = this.position.Y - (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
			this.position.X = this.position.X - (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
			return this.position.X < (float)(-147 * this.zoom) || this.position.Y < (float)(-100 * this.zoom);
		}

		// Token: 0x06003839 RID: 14393 RVA: 0x002C7DA9 File Offset: 0x002C5FA9
		public override Rectangle getBoundingBox(int xOffset, int yOffset)
		{
			return new Rectangle((int)this.position.X, (int)this.position.Y, 147 * this.zoom, 100 * this.zoom);
		}

		// Token: 0x0600383A RID: 14394 RVA: 0x002C7DDD File Offset: 0x002C5FDD
		public override void draw(SpriteBatch b)
		{
		}

		// Token: 0x0600383B RID: 14395 RVA: 0x002C7DE0 File Offset: 0x002C5FE0
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(this.position), new Rectangle?(new Rectangle(128, 0, 146, 99)), Color.White, (this.verticalFlip && this.horizontalFlip) ? 3.1415927f : 0f, Vector2.Zero, (float)this.zoom, (this.verticalFlip && !this.horizontalFlip) ? SpriteEffects.FlipVertically : ((this.horizontalFlip && !this.verticalFlip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 1f);
		}

		// Token: 0x040024A6 RID: 9382
		public const int width = 147;

		// Token: 0x040024A7 RID: 9383
		public const int height = 100;

		// Token: 0x040024A8 RID: 9384
		public int zoom = 5;

		// Token: 0x040024A9 RID: 9385
		private bool verticalFlip;

		// Token: 0x040024AA RID: 9386
		private bool horizontalFlip;
	}
}
