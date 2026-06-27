using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200039C RID: 924
	public class OverheadParrot : Critter
	{
		// Token: 0x06003873 RID: 14451 RVA: 0x002CB058 File Offset: 0x002C9258
		public OverheadParrot(Vector2 start_position)
		{
			this.position = start_position;
			this.velocity = new Vector2(Utility.RandomFloat(-4f, -2f, null), Utility.RandomFloat(5f, 6f, null));
			this._texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
			this.sourceRect = new Rectangle(0, 0, 24, 24);
			this.sourceRect.Y = 24 * Game1.random.Next(4);
			this.currentFlapIndex = Game1.random.Next(this.spriteFlapFrames.Length);
			this.flyOffset = (float)(Game1.random.NextDouble() * 100.0);
			this.swayAmount.X = Utility.RandomFloat(16f, 32f, null);
			this.swayAmount.Y = Utility.RandomFloat(10f, 24f, null);
		}

		// Token: 0x06003874 RID: 14452 RVA: 0x002CB168 File Offset: 0x002C9368
		public override bool update(GameTime time, GameLocation environment)
		{
			this.flapFrameAccumulator++;
			if (this.flapFrameAccumulator >= 2)
			{
				this.currentFlapIndex++;
				if (this.currentFlapIndex >= this.spriteFlapFrames.Length)
				{
					this.currentFlapIndex = 0;
				}
				this.flapFrameAccumulator = 0;
			}
			this.age += (float)time.ElapsedGameTime.TotalSeconds;
			this.position += this.velocity;
			float x_offset_rad = (this.age + this.flyOffset) * 1f;
			float y_offset_rad = (this.age + this.flyOffset) * 2f;
			this.drawOffset.X = (float)Math.Sin((double)x_offset_rad) * this.swayAmount.X;
			this.drawOffset.Y = (float)Math.Cos((double)y_offset_rad) * this.swayAmount.Y;
			Vector2 draw_position = this.GetDrawPosition();
			if (this.currentFlapIndex == 4 && this.flapFrameAccumulator == 0 && Utility.isOnScreen(draw_position, 64))
			{
				Game1.playSound("parrot_flap", null);
			}
			Vector2 draw_position_offset = draw_position - this.lastDrawPosition;
			this.lastDrawPosition = draw_position;
			int base_sprite = 2;
			if (Math.Abs(draw_position_offset.X) < Math.Abs(draw_position_offset.Y))
			{
				base_sprite = 5;
			}
			this.sourceRect.X = (this.spriteFlapFrames[this.currentFlapIndex] + base_sprite) * 24;
			this._shouldDrawShadow = true;
			Vector2 shadow_position = this.GetShadowPosition();
			if (!Game1.currentLocation.hasTileAt((int)shadow_position.X / 64, (int)shadow_position.Y / 64, "Back", null))
			{
				this._shouldDrawShadow = false;
			}
			return this.position.X < -64f - this.swayAmount.X * 4f || this.position.Y > (float)(environment.map.Layers[0].DisplayHeight + 64) + (this.height + this.swayAmount.Y) * 4f;
		}

		// Token: 0x06003875 RID: 14453 RVA: 0x002CB37E File Offset: 0x002C957E
		public Vector2 GetDrawPosition()
		{
			return this.position + new Vector2(this.drawOffset.X, -this.height + this.drawOffset.Y) * 4f;
		}

		// Token: 0x06003876 RID: 14454 RVA: 0x002CB3B8 File Offset: 0x002C95B8
		public Vector2 GetShadowPosition()
		{
			return this.position + new Vector2(this.drawOffset.X * 4f, -4f);
		}

		// Token: 0x06003877 RID: 14455 RVA: 0x002CB3E0 File Offset: 0x002C95E0
		public override void draw(SpriteBatch b)
		{
			if (this._shouldDrawShadow)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.GetShadowPosition()), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (this.position.Y - 1f) / 10000f);
			}
		}

		// Token: 0x06003878 RID: 14456 RVA: 0x002CB480 File Offset: 0x002C9680
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			b.Draw(this._texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), new Rectangle?(this.sourceRect), Color.White, 0f, new Vector2(12f, 20f), 4f, SpriteEffects.None, this.position.Y / 10000f);
		}

		// Token: 0x040024ED RID: 9453
		protected Texture2D _texture;

		// Token: 0x040024EE RID: 9454
		public Vector2 velocity;

		// Token: 0x040024EF RID: 9455
		public float age;

		// Token: 0x040024F0 RID: 9456
		public float flyOffset;

		// Token: 0x040024F1 RID: 9457
		public float height = 64f;

		// Token: 0x040024F2 RID: 9458
		public Rectangle sourceRect;

		// Token: 0x040024F3 RID: 9459
		public Vector2 drawOffset;

		// Token: 0x040024F4 RID: 9460
		public int[] spriteFlapFrames = new int[]
		{
			0,
			0,
			0,
			0,
			1,
			2,
			2,
			1
		};

		// Token: 0x040024F5 RID: 9461
		public int currentFlapIndex;

		// Token: 0x040024F6 RID: 9462
		public int flapFrameAccumulator;

		// Token: 0x040024F7 RID: 9463
		public Vector2 swayAmount;

		// Token: 0x040024F8 RID: 9464
		public Vector2 lastDrawPosition;

		// Token: 0x040024F9 RID: 9465
		protected bool _shouldDrawShadow;
	}
}
