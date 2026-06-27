using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000395 RID: 917
	public class Crow : Critter
	{
		// Token: 0x0600384A RID: 14410 RVA: 0x002C887C File Offset: 0x002C6A7C
		public Crow(int tileX, int tileY) : base(14, new Vector2((float)(tileX * 64), (float)(tileY * 64)))
		{
			this.flip = Game1.random.NextBool();
			this.position.X = this.position.X + 32f;
			this.position.Y = this.position.Y + 32f;
			this.startingPosition = this.position;
			this.state = 0;
		}

		// Token: 0x0600384B RID: 14411 RVA: 0x002C88EB File Offset: 0x002C6AEB
		public void hop(Farmer who)
		{
			this.gravityAffectedDY = -4f;
		}

		// Token: 0x0600384C RID: 14412 RVA: 0x002C88F8 File Offset: 0x002C6AF8
		private void donePecking(Farmer who)
		{
			this.state = Game1.random.Choose(0, 3);
		}

		// Token: 0x0600384D RID: 14413 RVA: 0x002C890C File Offset: 0x002C6B0C
		private void playFlap(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("batFlap", null);
			}
		}

		// Token: 0x0600384E RID: 14414 RVA: 0x002C893C File Offset: 0x002C6B3C
		private void playPeck(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x0600384F RID: 14415 RVA: 0x002C896C File Offset: 0x002C6B6C
		public override bool update(GameTime time, GameLocation environment)
		{
			Farmer f = Utility.isThereAFarmerWithinDistance(this.position / 64f, 4, environment);
			if (this.yJumpOffset < 0f && this.state != 1)
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
			if (f != null && this.state != 1)
			{
				if (Game1.random.NextDouble() < 0.85)
				{
					Game1.playSound("crow", null);
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
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 6)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 9)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 10)), 40, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.playFlap), false),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 9)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 40),
					new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 40)
				});
				this.sprite.loop = true;
			}
			switch (this.state)
			{
			case 0:
				if (this.sprite.CurrentAnimation == null)
				{
					List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((int)((short)this.baseFrame), 480),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 1)), 170, false, this.flip, null, false),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 2)), 170, false, this.flip, null, false)
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
				this.yOffset -= 2f;
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
					switch (Game1.random.Next(5))
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
						this.state = 1;
						this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 6)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 9)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 10)), 50, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.playFlap), false),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 9)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 8)), 50),
							new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 7)), 50)
						});
						this.sprite.loop = true;
						break;
					}
				}
				else if (this.sprite.CurrentAnimation == null)
				{
					this.sprite.currentFrame = this.baseFrame;
				}
				break;
			}
			return base.update(time, environment);
		}

		// Token: 0x040024C4 RID: 9412
		public const int flyingSpeed = 6;

		// Token: 0x040024C5 RID: 9413
		public const int pecking = 0;

		// Token: 0x040024C6 RID: 9414
		public const int flyingAway = 1;

		// Token: 0x040024C7 RID: 9415
		public const int sleeping = 2;

		// Token: 0x040024C8 RID: 9416
		public const int stopped = 3;

		// Token: 0x040024C9 RID: 9417
		private int state;
	}
}
