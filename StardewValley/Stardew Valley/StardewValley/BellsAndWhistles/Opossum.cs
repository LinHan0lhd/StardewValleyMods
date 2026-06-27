using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200039B RID: 923
	public class Opossum : Critter
	{
		// Token: 0x06003871 RID: 14449 RVA: 0x002CAC6C File Offset: 0x002C8E6C
		public Opossum(GameLocation location, Vector2 position, bool flip)
		{
			this.characterCheckTimer = Game1.random.Next(500, 3000);
			this.position = position * 64f;
			position.Y += 48f;
			this.flip = flip;
			this.baseFrame = 150;
			this.sprite = new AnimatedSprite(Critter.critterTexture, 150, 32, 32);
			this.sprite.loop = true;
			this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(this.baseFrame, 500),
				new FarmerSprite.AnimationFrame(this.baseFrame + 1, 50),
				new FarmerSprite.AnimationFrame(this.baseFrame + 2, 500),
				new FarmerSprite.AnimationFrame(this.baseFrame + 1, 50),
				new FarmerSprite.AnimationFrame(this.baseFrame, 1000),
				new FarmerSprite.AnimationFrame(this.baseFrame + 1, 50),
				new FarmerSprite.AnimationFrame(this.baseFrame + 2, 700),
				new FarmerSprite.AnimationFrame(this.baseFrame + 1, 50)
			});
			this.startingPosition = position;
		}

		// Token: 0x06003872 RID: 14450 RVA: 0x002CADCC File Offset: 0x002C8FCC
		public override bool update(GameTime time, GameLocation environment)
		{
			this.characterCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 8, environment) != null)
			{
				this.characterCheckTimer = 0;
			}
			if (this.jumpTimer > -1)
			{
				this.jumpTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				this.yJumpOffset = -(float)Math.Sin((double)((600f - (float)this.jumpTimer) / 600f) * 3.141592653589793) * 4f * 16f;
				if (this.jumpTimer <= -1)
				{
					this.running = true;
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame + 5, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 6, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 7, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 8, 40)
					});
					this.sprite.loop = true;
				}
			}
			else if (this.characterCheckTimer <= 0 && !this.running)
			{
				if (Utility.isOnScreen(this.position, -32) && this.jumpTimer == -1)
				{
					this.jumpTimer = 600;
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame + 4, 20)
					});
				}
				this.characterCheckTimer = 200;
			}
			if (this.running)
			{
				this.position.X = this.position.X + (float)(this.flip ? -6 : 6);
			}
			if (this.running && this.characterCheckTimer <= 0)
			{
				this.characterCheckTimer = 200;
				if (environment.largeTerrainFeatures != null)
				{
					Rectangle tileRect = new Rectangle((int)this.position.X + 32, (int)this.position.Y - 32, 4, 192);
					foreach (LargeTerrainFeature f in environment.largeTerrainFeatures)
					{
						Bush bush = f as Bush;
						if (bush != null && f.getBoundingBox().Intersects(tileRect))
						{
							bush.performUseAction(f.Tile);
							return true;
						}
					}
				}
			}
			return base.update(time, environment);
		}

		// Token: 0x040024EA RID: 9450
		private int characterCheckTimer = 1500;

		// Token: 0x040024EB RID: 9451
		private bool running;

		// Token: 0x040024EC RID: 9452
		private int jumpTimer = -1;
	}
}
