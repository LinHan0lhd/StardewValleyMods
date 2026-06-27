using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200039D RID: 925
	public class Owl : Critter
	{
		// Token: 0x06003879 RID: 14457 RVA: 0x002CB4E4 File Offset: 0x002C96E4
		public Owl()
		{
		}

		// Token: 0x0600387A RID: 14458 RVA: 0x002CB4EC File Offset: 0x002C96EC
		public Owl(Vector2 position)
		{
			this.baseFrame = 83;
			this.position = position;
			this.sprite = new AnimatedSprite(Critter.critterTexture, this.baseFrame, 32, 32);
			this.startingPosition = position;
			this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(83, 100),
				new FarmerSprite.AnimationFrame(84, 100),
				new FarmerSprite.AnimationFrame(85, 100),
				new FarmerSprite.AnimationFrame(86, 100)
			});
		}

		// Token: 0x0600387B RID: 14459 RVA: 0x002CB57C File Offset: 0x002C977C
		public override bool update(GameTime time, GameLocation environment)
		{
			Vector2 parallax = new Vector2((float)Game1.viewport.X - Game1.previousViewportPosition.X, (float)Game1.viewport.Y - Game1.previousViewportPosition.Y) * 0.15f;
			this.position.Y = this.position.Y + (float)time.ElapsedGameTime.TotalMilliseconds * 0.2f;
			this.position.X = this.position.X + (float)time.ElapsedGameTime.TotalMilliseconds * 0.05f;
			this.position -= parallax;
			return base.update(time, environment);
		}

		// Token: 0x0600387C RID: 14460 RVA: 0x002CB627 File Offset: 0x002C9827
		public override void draw(SpriteBatch b)
		{
		}

		// Token: 0x0600387D RID: 14461 RVA: 0x002CB62C File Offset: 0x002C982C
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(-64f, -128f + this.yJumpOffset + this.yOffset)), this.position.Y / 10000f + this.position.X / 100000f, 0, 0, Color.MediumBlue, this.flip, 4f, 0f, false);
		}
	}
}
