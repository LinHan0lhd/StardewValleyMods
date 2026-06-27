using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A3 RID: 931
	public class Rabbit : Critter
	{
		// Token: 0x060038C8 RID: 14536 RVA: 0x002CFA30 File Offset: 0x002CDC30
		public Rabbit(GameLocation location, Vector2 position, bool flip)
		{
			bool isWinter = location.IsWinterHere();
			this.position = position * 64f;
			position.Y += 48f;
			this.flip = flip;
			this.baseFrame = (isWinter ? 74 : 54);
			this.sprite = new AnimatedSprite(Critter.critterTexture, isWinter ? 69 : 68, 32, 32);
			this.sprite.loop = true;
			this.startingPosition = position;
		}

		// Token: 0x060038C9 RID: 14537 RVA: 0x002CFABC File Offset: 0x002CDCBC
		public override bool update(GameTime time, GameLocation environment)
		{
			this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterCheckTimer <= 0 && !this.running)
			{
				if (Utility.isOnScreen(this.position, -32))
				{
					this.running = true;
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 1, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 2, 40),
						new FarmerSprite.AnimationFrame(this.baseFrame + 3, 100),
						new FarmerSprite.AnimationFrame(this.baseFrame + 5, 70),
						new FarmerSprite.AnimationFrame(this.baseFrame + 5, 40)
					});
					this.sprite.loop = true;
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

		// Token: 0x04002548 RID: 9544
		private int characterCheckTimer = 200;

		// Token: 0x04002549 RID: 9545
		private bool running;
	}
}
