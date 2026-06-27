using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A7 RID: 935
	public class Seagull : Critter
	{
		// Token: 0x060038E5 RID: 14565 RVA: 0x002D10A0 File Offset: 0x002CF2A0
		public Seagull(Vector2 position, int startingState) : base(0, position)
		{
			this.moveLeft = Game1.random.NextBool();
			this.startingPosition = position;
			this.state = startingState;
		}

		// Token: 0x060038E6 RID: 14566 RVA: 0x002D10D3 File Offset: 0x002CF2D3
		public void hop(Farmer who)
		{
			this.gravityAffectedDY = -4f;
		}

		// Token: 0x060038E7 RID: 14567 RVA: 0x002D10E0 File Offset: 0x002CF2E0
		public override bool update(GameTime time, GameLocation environment)
		{
			this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterCheckTimer < 0)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 4, environment);
				this.characterCheckTimer = 200;
				if (f != null && this.state != 1)
				{
					if (Game1.random.NextDouble() < 0.25)
					{
						Game1.playSound("seagulls", null);
					}
					this.state = 1;
					if (f.Position.X > this.position.X)
					{
						this.moveLeft = true;
					}
					else
					{
						this.moveLeft = false;
					}
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 10)), 80),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 11)), 80),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 12)), 80),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 13)), 100)
					});
					this.sprite.loop = true;
				}
			}
			switch (this.state)
			{
			case 0:
			{
				int delta = this.moveLeft ? -2 : 2;
				if (!environment.isCollidingPosition(this.getBoundingBox(delta, 0), Game1.viewport, false, 0, false, null, false, false, true, false))
				{
					this.position.X = this.position.X + (float)delta;
				}
				if (Game1.random.NextDouble() < 0.005)
				{
					this.state = 3;
					this.sprite.loop = false;
					this.sprite.CurrentAnimation = null;
					this.sprite.currentFrame = 0;
				}
				break;
			}
			case 1:
				if (this.moveLeft)
				{
					this.position.X = this.position.X - 4f;
				}
				else
				{
					this.position.X = this.position.X + 4f;
				}
				this.yOffset -= 2f;
				break;
			case 2:
			{
				this.sprite.currentFrame = this.baseFrame + 9;
				float tmpY = this.yOffset;
				if ((time.TotalGameTime.TotalMilliseconds + (double)((int)this.position.X * 4)) % 2000.0 < 1000.0)
				{
					this.yOffset = 2f;
				}
				else
				{
					this.yOffset = 0f;
				}
				if (this.yOffset > tmpY)
				{
					environment.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(this.position.X - 32f, this.position.Y - 32f), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false));
				}
				break;
			}
			case 3:
				if (Game1.random.NextDouble() < 0.003 && this.sprite.CurrentAnimation == null)
				{
					this.sprite.loop = false;
					switch (Game1.random.Next(4))
					{
					case 0:
					{
						List<FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 2)), 100),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 100),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 4)), 200),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 5)), 200)
						};
						int extra = Game1.random.Next(5);
						for (int i = 0; i < extra; i++)
						{
							frames.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 4)), 200));
							frames.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 5)), 200));
						}
						this.sprite.setCurrentAnimation(frames);
						break;
					}
					case 1:
						this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(6, (int)((short)Game1.random.Next(500, 4000)))
						});
						break;
					case 2:
					{
						List<FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 6)), 500),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 100, false, false, new AnimatedSprite.endOfAnimationBehavior(this.hop), false),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 100)
						};
						int extra = Game1.random.Next(3);
						for (int j = 0; j < extra; j++)
						{
							frames.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 100));
							frames.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 100));
						}
						this.sprite.setCurrentAnimation(frames);
						break;
					}
					case 3:
						this.state = 0;
						this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((int)((short)this.baseFrame), 200),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 1)), 200)
						});
						this.sprite.loop = true;
						this.moveLeft = Game1.random.NextBool();
						if (Game1.random.NextDouble() < 0.33)
						{
							if (this.position.X > this.startingPosition.X)
							{
								this.moveLeft = true;
							}
							else
							{
								this.moveLeft = false;
							}
						}
						break;
					}
				}
				else if (this.sprite.CurrentAnimation == null)
				{
					this.sprite.currentFrame = this.baseFrame;
				}
				break;
			}
			this.flip = !this.moveLeft;
			return base.update(time, environment);
		}

		// Token: 0x04002571 RID: 9585
		public const int walkingSpeed = 2;

		// Token: 0x04002572 RID: 9586
		public const int flyingSpeed = 4;

		// Token: 0x04002573 RID: 9587
		public const int walking = 0;

		// Token: 0x04002574 RID: 9588
		public const int flyingAway = 1;

		// Token: 0x04002575 RID: 9589
		public const int flyingToLand = 4;

		// Token: 0x04002576 RID: 9590
		public const int swimming = 2;

		// Token: 0x04002577 RID: 9591
		public const int stopped = 3;

		// Token: 0x04002578 RID: 9592
		private int state;

		// Token: 0x04002579 RID: 9593
		private int characterCheckTimer = 200;

		// Token: 0x0400257A RID: 9594
		private bool moveLeft;
	}
}
