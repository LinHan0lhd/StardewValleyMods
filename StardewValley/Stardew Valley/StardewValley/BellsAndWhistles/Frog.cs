using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000398 RID: 920
	public class Frog : Critter
	{
		// Token: 0x06003858 RID: 14424 RVA: 0x002C95D8 File Offset: 0x002C77D8
		public Frog(Vector2 position, bool waterLeaper = false, bool forceFlip = false)
		{
			this.waterLeaper = waterLeaper;
			this.position = position * 64f;
			this.sprite = new AnimatedSprite(Critter.critterTexture, waterLeaper ? 300 : 280, 16, 16);
			this.sprite.loop = true;
			if (!this.flip && forceFlip)
			{
				this.flip = true;
			}
			if (waterLeaper)
			{
				this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(300, 600),
					new FarmerSprite.AnimationFrame(304, 100),
					new FarmerSprite.AnimationFrame(305, 100),
					new FarmerSprite.AnimationFrame(306, 300),
					new FarmerSprite.AnimationFrame(305, 100),
					new FarmerSprite.AnimationFrame(304, 100)
				});
			}
			else
			{
				this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(280, 60),
					new FarmerSprite.AnimationFrame(281, 70),
					new FarmerSprite.AnimationFrame(282, 140),
					new FarmerSprite.AnimationFrame(283, 90)
				});
				this.beforeFadeTimer = 1000;
				this.flip = (this.position.X + 4f < Game1.player.Position.X);
			}
			this.startingPosition = position;
		}

		// Token: 0x06003859 RID: 14425 RVA: 0x002C977E File Offset: 0x002C797E
		public void startSplash(Farmer who)
		{
			this.splash = true;
		}

		// Token: 0x0600385A RID: 14426 RVA: 0x002C9788 File Offset: 0x002C7988
		public override bool update(GameTime time, GameLocation environment)
		{
			if (this.waterLeaper)
			{
				if (!this.leapingIntoWater)
				{
					this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.characterCheckTimer <= 0)
					{
						if (Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 6, environment) != null)
						{
							this.leapingIntoWater = true;
							this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(300, 100),
								new FarmerSprite.AnimationFrame(301, 100),
								new FarmerSprite.AnimationFrame(302, 100),
								new FarmerSprite.AnimationFrame(303, 1500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.startSplash), true)
							});
							this.sprite.loop = false;
							this.sprite.oldFrame = 303;
							this.gravityAffectedDY = -6f;
						}
						else if (Game1.random.NextDouble() < 0.01)
						{
							Game1.playSound("croak", null);
						}
						this.characterCheckTimer = 200;
					}
				}
				else
				{
					this.position.X = this.position.X + (float)(this.flip ? -4 : 4);
					if (this.gravityAffectedDY >= 0f && this.yJumpOffset >= 0f)
					{
						this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(300, 100),
							new FarmerSprite.AnimationFrame(301, 100),
							new FarmerSprite.AnimationFrame(302, 100),
							new FarmerSprite.AnimationFrame(303, 1500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.startSplash), true)
						});
						this.sprite.loop = false;
						this.sprite.oldFrame = 303;
						this.gravityAffectedDY = -6f;
						this.yJumpOffset = 0f;
						if (environment.isWaterTile((int)this.position.X / 64, (int)this.position.Y / 64))
						{
							this.splash = true;
						}
					}
				}
			}
			else
			{
				this.position.X = this.position.X + (float)(this.flip ? -3 : 3);
				this.beforeFadeTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.beforeFadeTimer <= 0)
				{
					this.alpha -= 0.001f * (float)time.ElapsedGameTime.Milliseconds;
					if (this.alpha <= 0f)
					{
						return true;
					}
				}
				if (environment.isWaterTile((int)this.position.X / 64, (int)this.position.Y / 64))
				{
					this.splash = true;
				}
			}
			if (this.splash)
			{
				environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, this.position, false, false));
				Game1.playSound("dropItemInWater", null);
				return true;
			}
			return base.update(time, environment);
		}

		// Token: 0x0600385B RID: 14427 RVA: 0x002C9AB8 File Offset: 0x002C7CB8
		public override void draw(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(this.position + new Vector2(0f, -20f + this.yJumpOffset + this.yOffset))), (this.position.Y + 64f) / 10000f, 0, 0, Color.White * this.alpha, this.flip, 4f, 0f, false);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(32f, 40f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * this.alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 16f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
		}

		// Token: 0x0600385C RID: 14428 RVA: 0x002C9C05 File Offset: 0x002C7E05
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}

		// Token: 0x040024D8 RID: 9432
		private bool waterLeaper;

		// Token: 0x040024D9 RID: 9433
		private bool leapingIntoWater;

		// Token: 0x040024DA RID: 9434
		private bool splash;

		// Token: 0x040024DB RID: 9435
		private int characterCheckTimer = 200;

		// Token: 0x040024DC RID: 9436
		private int beforeFadeTimer;

		// Token: 0x040024DD RID: 9437
		private float alpha = 1f;
	}
}
