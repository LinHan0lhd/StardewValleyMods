using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x020000FB RID: 251
	public class SeaMonsterTemporarySprite : TemporaryAnimatedSprite
	{
		// Token: 0x0600144D RID: 5197 RVA: 0x000F6468 File Offset: 0x000F4668
		public SeaMonsterTemporarySprite(float animationInterval, int animationLength, int numberOfLoops, Vector2 position) : base(-666, animationInterval, animationLength, numberOfLoops, position, false, false)
		{
			this.texture = Game1.content.Load<Texture2D>("LooseSprites\\SeaMonster");
			Game1.playSound("pullItemFromWater", null);
			this.currentParentTileIndex = 0;
		}

		// Token: 0x0600144E RID: 5198 RVA: 0x000F64B8 File Offset: 0x000F46B8
		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle?(new Rectangle(this.currentParentTileIndex * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
		}

		// Token: 0x0600144F RID: 5199 RVA: 0x000F6528 File Offset: 0x000F4728
		public override bool update(GameTime time)
		{
			this.timer += (float)time.ElapsedGameTime.Milliseconds;
			if (this.timer > this.interval)
			{
				this.currentParentTileIndex++;
				this.timer = 0f;
				if (this.currentParentTileIndex >= this.animationLength)
				{
					this.currentNumberOfLoops++;
					this.currentParentTileIndex = 2;
				}
			}
			if (this.currentNumberOfLoops >= this.totalNumberOfLoops)
			{
				this.position.Y = this.position.Y + 2f;
				if (this.position.Y >= (float)Game1.currentLocation.Map.DisplayHeight)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000D12 RID: 3346
		public new Texture2D texture;
	}
}
