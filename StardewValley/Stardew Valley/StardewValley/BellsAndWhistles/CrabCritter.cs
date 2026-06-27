using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000393 RID: 915
	public class CrabCritter : Critter
	{
		// Token: 0x0600383C RID: 14396 RVA: 0x002C7E74 File Offset: 0x002C6074
		public CrabCritter()
		{
			this.sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			this.sprite.SourceRect = this._baseSourceRectangle;
			this.sprite.ignoreSourceRectUpdates = true;
			this._crabVariant = 1;
			this.UpdateSpriteRectangle();
		}

		// Token: 0x0600383D RID: 14397 RVA: 0x002C7EFC File Offset: 0x002C60FC
		public CrabCritter(Vector2 start_position) : this()
		{
			this.position = start_position;
			float movement_rectangle_width = 256f;
			this.movementBounds = new Microsoft.Xna.Framework.Rectangle((int)(start_position.X - movement_rectangle_width / 2f), (int)start_position.Y, (int)movement_rectangle_width, 0);
		}

		// Token: 0x0600383E RID: 14398 RVA: 0x002C7F40 File Offset: 0x002C6140
		public override bool update(GameTime time, GameLocation environment)
		{
			this.nextFrameChange -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.skittering)
			{
				this.skitterTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.nextFrameChange <= 0f && (this.moving || this.skittering))
			{
				this._currentFrame++;
				if (this._currentFrame >= 4)
				{
					this._currentFrame = 0;
				}
				if (this.skittering)
				{
					this.nextFrameChange = Utility.RandomFloat(0.025f, 0.05f, null);
				}
				else
				{
					this.nextFrameChange = Utility.RandomFloat(0.05f, 0.15f, null);
				}
			}
			if (this.skittering)
			{
				if (this.yJumpOffset >= 0f)
				{
					if (!this.diving)
					{
						if (Game1.random.Next(0, 4) == 0)
						{
							this.gravityAffectedDY = -4f;
						}
						else
						{
							this.gravityAffectedDY = -2f;
						}
					}
					else
					{
						if (environment.isWaterTile((int)this.position.X / 64, (int)this.position.Y / 64))
						{
							environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, this.position, false, false));
							Game1.playSound("dropItemInWater", null);
							return true;
						}
						this.gravityAffectedDY = -4f;
					}
				}
			}
			else
			{
				this.nextCharacterCheck -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.nextCharacterCheck <= 0f)
				{
					Character f = Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 7, environment);
					if (f != null)
					{
						this._crabVariant = 0;
						this.skittering = true;
						if (f.position.X > this.position.X)
						{
							this.movementDirection.X = -3f;
						}
						else
						{
							this.movementDirection.X = 3f;
						}
					}
					this.nextCharacterCheck = 0.25f;
				}
				if (!this.skittering)
				{
					if (this.moving && this.yJumpOffset >= 0f)
					{
						this.gravityAffectedDY = -1f;
					}
					this.nextMovementChange -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this.nextMovementChange <= 0f)
					{
						this.moving = !this.moving;
						if (this.moving)
						{
							if (!Game1.random.NextBool())
							{
								this.movementDirection.X = 1f;
							}
							else
							{
								this.movementDirection.X = -1f;
							}
						}
						else
						{
							this.movementDirection = Vector2.Zero;
						}
						if (this.moving)
						{
							this.nextMovementChange = Utility.RandomFloat(0.15f, 0.5f, null);
						}
						else
						{
							this.nextMovementChange = Utility.RandomFloat(0.2f, 1f, null);
						}
					}
				}
			}
			this.position += this.movementDirection;
			if (!this.diving && !environment.isTilePassable(new Location((int)(this.position.X / 64f), (int)(this.position.Y / 64f)), Game1.viewport))
			{
				this.position -= this.movementDirection;
				this.movementDirection *= -1f;
			}
			if (!this.skittering)
			{
				if (this.position.X < (float)this.movementBounds.Left)
				{
					this.position.X = (float)this.movementBounds.Left;
					this.movementDirection *= -1f;
				}
				if (this.position.X > (float)this.movementBounds.Right)
				{
					this.position.X = (float)this.movementBounds.Right;
					this.movementDirection *= -1f;
				}
			}
			else if (!this.diving && environment.isWaterTile((int)(this.position.X / 64f + (float)Math.Sign(this.movementDirection.X) * 1f), (int)this.position.Y / 64))
			{
				if (this.yJumpOffset >= 0f)
				{
					this.gravityAffectedDY = -7f;
				}
				this.diving = true;
			}
			this.UpdateSpriteRectangle();
			return this.skitterTime <= 0f || base.update(time, environment);
		}

		// Token: 0x0600383F RID: 14399 RVA: 0x002C83D8 File Offset: 0x002C65D8
		public virtual void UpdateSpriteRectangle()
		{
			Microsoft.Xna.Framework.Rectangle source_rectangle = this._baseSourceRectangle;
			source_rectangle.Y += this._crabVariant * 18;
			int drawn_frame = this._currentFrame;
			if (drawn_frame == 3)
			{
				drawn_frame = 1;
			}
			source_rectangle.X += drawn_frame * 18;
			this.sprite.SourceRect = source_rectangle;
		}

		// Token: 0x06003840 RID: 14400 RVA: 0x002C8428 File Offset: 0x002C6628
		public override void draw(SpriteBatch b)
		{
			float alpha = this.skitterTime;
			if (alpha > 1f)
			{
				alpha = 1f;
			}
			if (alpha < 0f)
			{
				alpha = 0f;
			}
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(this.position + new Vector2(0f, -20f + this.yJumpOffset + this.yOffset))), (this.position.Y + 64f - 32f) / 10000f, 0, 0, Color.White * alpha, this.flip, 4f, 0f, false);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(32f, 40f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 16f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
		}

		// Token: 0x06003841 RID: 14401 RVA: 0x002C8594 File Offset: 0x002C6794
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}

		// Token: 0x040024AB RID: 9387
		public Microsoft.Xna.Framework.Rectangle movementRectangle;

		// Token: 0x040024AC RID: 9388
		public float nextCharacterCheck = 2f;

		// Token: 0x040024AD RID: 9389
		public float nextFrameChange;

		// Token: 0x040024AE RID: 9390
		public float nextMovementChange;

		// Token: 0x040024AF RID: 9391
		public bool moving;

		// Token: 0x040024B0 RID: 9392
		public bool diving;

		// Token: 0x040024B1 RID: 9393
		public bool skittering;

		// Token: 0x040024B2 RID: 9394
		protected float skitterTime = 5f;

		// Token: 0x040024B3 RID: 9395
		protected Microsoft.Xna.Framework.Rectangle _baseSourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 272, 18, 18);

		// Token: 0x040024B4 RID: 9396
		protected int _currentFrame;

		// Token: 0x040024B5 RID: 9397
		protected int _crabVariant;

		// Token: 0x040024B6 RID: 9398
		protected Vector2 movementDirection = Vector2.Zero;

		// Token: 0x040024B7 RID: 9399
		public Microsoft.Xna.Framework.Rectangle movementBounds;
	}
}
