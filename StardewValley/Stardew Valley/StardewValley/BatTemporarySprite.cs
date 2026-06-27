using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x02000082 RID: 130
	public class BatTemporarySprite : TemporaryAnimatedSprite
	{
		// Token: 0x060004E3 RID: 1251 RVA: 0x00018890 File Offset: 0x00016A90
		public BatTemporarySprite(Vector2 position) : base(-666, 100f, 4, 99999, position, false, false)
		{
			this.texture = Game1.content.Load<Texture2D>("LooseSprites\\Bat");
			this.currentParentTileIndex = 0;
			if (position.X > (float)(Game1.currentLocation.Map.DisplayWidth / 2))
			{
				this.moveLeft = true;
			}
			this.horizontalSpeed = Game1.random.Next(1, 8);
			this.verticalSpeed = (float)Game1.random.Next(3, 7);
			this.interval = 160f - ((float)this.horizontalSpeed + this.verticalSpeed) * 10f;
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x00018938 File Offset: 0x00016B38
		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle?(new Rectangle(this.currentParentTileIndex * 64, 0, 64, 64)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x000189A8 File Offset: 0x00016BA8
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
					this.currentParentTileIndex = 0;
				}
			}
			if (this.moveLeft)
			{
				this.position.X = this.position.X - (float)this.horizontalSpeed;
			}
			else
			{
				this.position.X = this.position.X + (float)this.horizontalSpeed;
			}
			this.position.Y = this.position.Y + this.verticalSpeed;
			this.verticalSpeed -= 0.1f;
			return this.position.Y >= (float)Game1.currentLocation.Map.DisplayHeight || this.position.Y < 0f || this.position.X < 0f || this.position.X >= (float)Game1.currentLocation.Map.DisplayWidth;
		}

		// Token: 0x040001EB RID: 491
		public new Texture2D texture;

		// Token: 0x040001EC RID: 492
		private bool moveLeft;

		// Token: 0x040001ED RID: 493
		private int horizontalSpeed;

		// Token: 0x040001EE RID: 494
		private float verticalSpeed;
	}
}
