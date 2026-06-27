using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200038F RID: 911
	public class Birdie : Critter
	{
		// Token: 0x06003821 RID: 14369 RVA: 0x002C6104 File Offset: 0x002C4304
		public Birdie(int tileX, int tileY, int startingIndex = 25) : base(startingIndex, new Vector2((float)(tileX * 64), (float)(tileY * 64)))
		{
			this.flip = Game1.random.NextBool();
			this.position.X = this.position.X + 32f;
			this.position.Y = this.position.Y + 32f;
			this.startingPosition = this.position;
			this.flightOffset = (float)Game1.random.NextDouble() - 0.5f;
			this.state = 0;
		}

		// Token: 0x06003822 RID: 14370 RVA: 0x002C6194 File Offset: 0x002C4394
		public Birdie(Vector2 position, float yOffset, int startingIndex = 25, bool stationary = false) : base(startingIndex, position)
		{
			this.yOffset = yOffset;
			this.flip = Game1.random.NextBool();
			this.startingPosition = position;
			this.stationary = stationary;
			this.state = Game1.random.Next(2, 5);
			this.flightOffset = (float)Game1.random.NextDouble() - 0.5f;
		}

		// Token: 0x06003823 RID: 14371 RVA: 0x002C6203 File Offset: 0x002C4403
		public void hop(Farmer who)
		{
			this.gravityAffectedDY = -2f;
		}

		// Token: 0x06003824 RID: 14372 RVA: 0x002C6210 File Offset: 0x002C4410
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			if (this.state == 1)
			{
				base.draw(b);
			}
		}

		// Token: 0x06003825 RID: 14373 RVA: 0x002C6222 File Offset: 0x002C4422
		public override void draw(SpriteBatch b)
		{
			if (this.state != 1)
			{
				base.draw(b);
			}
		}

		// Token: 0x06003826 RID: 14374 RVA: 0x002C6234 File Offset: 0x002C4434
		private void donePecking(Farmer who)
		{
			this.state = Game1.random.Choose(0, 3);
		}

		// Token: 0x06003827 RID: 14375 RVA: 0x002C6248 File Offset: 0x002C4448
		private void playFlap(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("batFlap", null);
			}
		}

		// Token: 0x06003828 RID: 14376 RVA: 0x002C6278 File Offset: 0x002C4478
		private void playPeck(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06003829 RID: 14377 RVA: 0x002C62A8 File Offset: 0x002C44A8
		public override bool update(GameTime time, GameLocation environment)
		{
			if (this.yJumpOffset < 0f && this.state != 1 && !this.stationary)
			{
				if (!this.flip && !environment.isCollidingPosition(this.getBoundingBox(-2, 0), Game1.viewport, false, 0, false, null, false, false, true, false))
				{
					this.position.X = this.position.X - 2f;
				}
				else if (!environment.isCollidingPosition(this.getBoundingBox(2, 0), Game1.viewport, false, 0, false, null, false, false, true, false))
				{
					this.position.X = this.position.X + 2f;
				}
			}
			this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterCheckTimer < 0)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 4, environment);
				this.characterCheckTimer = 200;
				if (f != null && this.state != 1)
				{
					if (Game1.random.NextDouble() < 0.85)
					{
						Game1.playSound("SpringBirds", null);
					}
					this.state = 1;
					if (f.Position.X > this.position.X)
					{
						this.flip = false;
					}
					else
					{
						this.flip = true;
					}
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 6)), 70),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 60, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.playFlap), false),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 70),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 60)
					});
					this.sprite.loop = true;
				}
			}
			switch (this.state)
			{
			case 0:
				if (this.sprite.CurrentAnimation == null)
				{
					List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 2)), 480),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 170, false, this.flip, null, false),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 4)), 170, false, this.flip, null, false)
					};
					int pecks = Game1.random.Next(1, 5);
					for (int i = 0; i < pecks; i++)
					{
						peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 70));
						peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 4)), 100, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.playPeck), false));
					}
					peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 100));
					peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 2)), 70, false, this.flip, null, false));
					peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 1)), 70, false, this.flip, null, false));
					peckAnim.Add(new FarmerSprite.AnimationFrame((int)((short)this.baseFrame), 500, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.donePecking), false));
					this.sprite.loop = false;
					this.sprite.setCurrentAnimation(peckAnim);
				}
				break;
			case 1:
				if (!this.flip)
				{
					this.position.X = this.position.X - 6f;
				}
				else
				{
					this.position.X = this.position.X + 6f;
				}
				this.yOffset -= 2f + this.flightOffset;
				break;
			case 2:
				if (this.sprite.CurrentAnimation == null)
				{
					this.sprite.currentFrame = this.baseFrame + 5;
				}
				if (Game1.random.NextDouble() < 0.003 && this.sprite.CurrentAnimation == null)
				{
					this.state = 3;
				}
				break;
			case 3:
				if (Game1.random.NextDouble() < 0.008 && this.sprite.CurrentAnimation == null && this.yJumpOffset >= 0f)
				{
					switch (Game1.random.Next(6))
					{
					case 0:
						this.state = 2;
						break;
					case 1:
						this.state = 0;
						break;
					case 2:
						this.hop(null);
						break;
					case 3:
						this.flip = !this.flip;
						this.hop(null);
						break;
					case 4:
					case 5:
						this.state = 4;
						this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((int)((short)this.baseFrame), 100),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 1)), 100)
						});
						this.sprite.loop = true;
						if (this.position.X >= this.startingPosition.X)
						{
							this.flip = false;
						}
						else
						{
							this.flip = true;
						}
						this.walkTimer = Game1.random.Next(5, 15) * 100;
						break;
					}
				}
				else if (this.sprite.CurrentAnimation == null)
				{
					this.sprite.currentFrame = this.baseFrame;
				}
				break;
			case 4:
				if (!this.stationary)
				{
					int delta = this.flip ? 1 : -1;
					if (!environment.isCollidingPosition(this.getBoundingBox(delta, 0), Game1.viewport, false, 0, false, null, false, false, true, false))
					{
						this.position.X = this.position.X + (float)delta;
					}
				}
				else
				{
					float delta2 = this.flip ? 0.5f : -0.5f;
					if (Math.Abs(this.position.X + delta2 - this.startingPosition.X) < 8f)
					{
						this.position.X = this.position.X + delta2;
					}
					else
					{
						this.flip = !this.flip;
					}
				}
				this.walkTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.walkTimer < 0)
				{
					this.state = 3;
					this.sprite.loop = false;
					this.sprite.CurrentAnimation = null;
					this.sprite.currentFrame = this.baseFrame;
				}
				break;
			}
			return base.update(time, environment);
		}

		// Token: 0x0400247D RID: 9341
		public const int brownBird = 25;

		// Token: 0x0400247E RID: 9342
		public const int blueBird = 45;

		// Token: 0x0400247F RID: 9343
		public const int flyingSpeed = 6;

		// Token: 0x04002480 RID: 9344
		public const int walkingSpeed = 1;

		// Token: 0x04002481 RID: 9345
		public const int pecking = 0;

		// Token: 0x04002482 RID: 9346
		public const int flyingAway = 1;

		// Token: 0x04002483 RID: 9347
		public const int sleeping = 2;

		// Token: 0x04002484 RID: 9348
		public const int stopped = 3;

		// Token: 0x04002485 RID: 9349
		public const int walking = 4;

		// Token: 0x04002486 RID: 9350
		private int state;

		// Token: 0x04002487 RID: 9351
		private float flightOffset;

		// Token: 0x04002488 RID: 9352
		private bool stationary;

		// Token: 0x04002489 RID: 9353
		private int characterCheckTimer = 200;

		// Token: 0x0400248A RID: 9354
		private int walkTimer;
	}
}
