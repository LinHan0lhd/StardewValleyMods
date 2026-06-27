using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000394 RID: 916
	public abstract class Critter
	{
		// Token: 0x06003842 RID: 14402 RVA: 0x002C8596 File Offset: 0x002C6796
		public Critter()
		{
		}

		// Token: 0x06003843 RID: 14403 RVA: 0x002C859E File Offset: 0x002C679E
		public Critter(int baseFrame, Vector2 position)
		{
			this.baseFrame = baseFrame;
			this.position = position;
			this.sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 32, 32);
			this.startingPosition = position;
		}

		// Token: 0x06003844 RID: 14404 RVA: 0x002C85D0 File Offset: 0x002C67D0
		public virtual Rectangle getBoundingBox(int xOffset, int yOffset)
		{
			return new Rectangle((int)this.position.X - 32 + xOffset, (int)this.position.Y - 16 + yOffset, 64, 32);
		}

		// Token: 0x06003845 RID: 14405 RVA: 0x002C8600 File Offset: 0x002C6800
		public virtual bool update(GameTime time, GameLocation environment)
		{
			this.sprite.animateOnce(time);
			if (this.gravityAffectedDY < 0f || this.yJumpOffset < 0f)
			{
				this.yJumpOffset += this.gravityAffectedDY;
				this.gravityAffectedDY += 0.25f;
			}
			return this.position.X < -128f || this.position.Y < -128f || this.position.X > (float)environment.map.DisplayWidth || this.position.Y > (float)environment.map.DisplayHeight;
		}

		// Token: 0x06003846 RID: 14406 RVA: 0x002C86B4 File Offset: 0x002C68B4
		public virtual void draw(SpriteBatch b)
		{
			if (this.sprite != null)
			{
				this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(-64f, -128f + this.yJumpOffset + this.yOffset)), this.position.Y / 10000f + this.position.X / 1000000f, 0, 0, Color.White, this.flip, 4f, 0f, false);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, -4f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * (1f - Math.Min(1f, Math.Abs((this.yJumpOffset + this.yOffset) / 64f))), 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 64f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
			}
		}

		// Token: 0x06003847 RID: 14407 RVA: 0x002C882A File Offset: 0x002C6A2A
		public virtual void drawAboveFrontLayer(SpriteBatch b)
		{
		}

		// Token: 0x06003848 RID: 14408 RVA: 0x002C882C File Offset: 0x002C6A2C
		protected virtual string GenerateLightSourceId(int identifier)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(identifier);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040024B8 RID: 9400
		public const int spriteWidth = 32;

		// Token: 0x040024B9 RID: 9401
		public const int spriteHeight = 32;

		// Token: 0x040024BA RID: 9402
		public const float gravity = 0.25f;

		// Token: 0x040024BB RID: 9403
		public static string critterTexture = "TileSheets\\critters";

		// Token: 0x040024BC RID: 9404
		public Vector2 position;

		// Token: 0x040024BD RID: 9405
		public Vector2 startingPosition;

		// Token: 0x040024BE RID: 9406
		public int baseFrame;

		// Token: 0x040024BF RID: 9407
		public AnimatedSprite sprite;

		// Token: 0x040024C0 RID: 9408
		public bool flip;

		// Token: 0x040024C1 RID: 9409
		public float gravityAffectedDY;

		// Token: 0x040024C2 RID: 9410
		public float yOffset;

		// Token: 0x040024C3 RID: 9411
		public float yJumpOffset;
	}
}
