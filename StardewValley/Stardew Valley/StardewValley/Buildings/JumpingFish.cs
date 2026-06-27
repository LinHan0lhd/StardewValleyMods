using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Buildings
{
	// Token: 0x02000382 RID: 898
	public class JumpingFish
	{
		// Token: 0x0600379F RID: 14239 RVA: 0x002C1A78 File Offset: 0x002BFC78
		public JumpingFish(FishPond pond, Vector2 start_position, Vector2 end_position)
		{
			this.angularVelocity = Utility.RandomFloat(20f, 40f, null) * 3.1415927f / 180f;
			this.startPosition = start_position;
			this.endPosition = end_position;
			this.position = this.startPosition;
			this._pond = pond;
			this._fishObject = pond.GetFishObject();
			if (this.startPosition.X > this.endPosition.X)
			{
				this._flipped = true;
			}
			this.jumpHeight = Utility.RandomFloat(75f, 100f, null);
			this.Splash();
		}

		// Token: 0x060037A0 RID: 14240 RVA: 0x002C1B20 File Offset: 0x002BFD20
		public void Splash()
		{
			if (this._pond != null && Game1.currentLocation.buildings.Contains(this._pond))
			{
				Game1.playSound("dropItemInWater", null);
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, this.position + new Vector2(-0.5f, -0.5f) * 64f, false, false)
				{
					delayBeforeAnimationStart = 0,
					layerDepth = this.startPosition.Y / 10000f
				});
			}
		}

		// Token: 0x060037A1 RID: 14241 RVA: 0x002C1BC4 File Offset: 0x002BFDC4
		public bool Update(float time)
		{
			this._age += time;
			this.angle += this.angularVelocity * time;
			if (this._age >= this.jumpTime)
			{
				this._age = time;
				this.Splash();
				return true;
			}
			this.position.X = Utility.Lerp(this.startPosition.X, this.endPosition.X, this._age / this.jumpTime);
			this.position.Y = Utility.Lerp(this.startPosition.Y, this.endPosition.Y, this._age / this.jumpTime);
			return false;
		}

		// Token: 0x060037A2 RID: 14242 RVA: 0x002C1C78 File Offset: 0x002BFE78
		public void Draw(SpriteBatch b)
		{
			float drawn_angle = this.angle;
			SpriteEffects effect = SpriteEffects.None;
			if (this._flipped)
			{
				effect = SpriteEffects.FlipHorizontally;
				drawn_angle *= -1f;
			}
			float draw_scale = 1f;
			Vector2 draw_position = this.position + new Vector2(0f, (float)Math.Sin((double)(this._age / this.jumpTime) * 3.141592653589793) * -this.jumpHeight);
			Vector2 origin = new Vector2(8f, 8f);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._fishObject.QualifiedItemId);
			b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position), new Rectangle?(itemData.GetSourceRect(0, null)), Color.White, drawn_angle, origin, 4f * draw_scale, effect, this.position.Y / 10000f + 1E-06f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0f, new Vector2((float)(Game1.shadowTexture.Bounds.Width / 2), (float)(Game1.shadowTexture.Bounds.Height / 2)), 2f, effect, this.position.Y / 10000f + 1E-06f);
		}

		// Token: 0x0400241B RID: 9243
		public Vector2 startPosition;

		// Token: 0x0400241C RID: 9244
		public Vector2 endPosition;

		// Token: 0x0400241D RID: 9245
		protected float _age;

		// Token: 0x0400241E RID: 9246
		public float jumpTime = 1f;

		// Token: 0x0400241F RID: 9247
		protected FishPond _pond;

		// Token: 0x04002420 RID: 9248
		protected Object _fishObject;

		// Token: 0x04002421 RID: 9249
		protected bool _flipped;

		// Token: 0x04002422 RID: 9250
		public Vector2 position;

		// Token: 0x04002423 RID: 9251
		public float jumpHeight;

		// Token: 0x04002424 RID: 9252
		public float angularVelocity;

		// Token: 0x04002425 RID: 9253
		public float angle;
	}
}
