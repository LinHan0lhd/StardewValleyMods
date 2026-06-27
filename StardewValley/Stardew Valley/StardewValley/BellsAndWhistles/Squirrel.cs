using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AB RID: 939
	public class Squirrel : Critter
	{
		// Token: 0x06003916 RID: 14614 RVA: 0x002D4568 File Offset: 0x002D2768
		public Squirrel(Vector2 position, bool flip)
		{
			this.position = position * 64f;
			this.flip = flip;
			this.baseFrame = 60;
			this.sprite = new AnimatedSprite(Critter.critterTexture, this.baseFrame, 32, 32);
			this.sprite.loop = false;
			this.startingPosition = position;
		}

		// Token: 0x06003917 RID: 14615 RVA: 0x002D45DE File Offset: 0x002D27DE
		private void doneNibbling(Farmer who)
		{
			this.nextNibbleTimer = Game1.random.Next(2000);
		}

		// Token: 0x06003918 RID: 14616 RVA: 0x002D45F8 File Offset: 0x002D27F8
		public override void draw(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2((float)(-64 + ((this.treeRunTimer > 0) ? (this.flip ? 224 : -16) : 0)), -64f + this.yJumpOffset + this.yOffset + (float)((this.treeRunTimer > 0) ? (this.flip ? 0 : 128) : 0))), (this.position.Y + 64f + (float)((this.treeRunTimer > 0) ? 128 : 0)) / 10000f + this.position.X / 1000000f, 0, 0, Color.White, this.flip, 4f, (this.treeRunTimer > 0) ? ((float)((double)(this.flip ? 1 : -1) * 3.141592653589793 / 2.0)) : 0f, false);
			if (this.treeRunTimer <= 0)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, 60f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 16f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
			}
		}

		// Token: 0x06003919 RID: 14617 RVA: 0x002D47C4 File Offset: 0x002D29C4
		public override bool update(GameTime time, GameLocation environment)
		{
			this.nextNibbleTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.sprite.CurrentAnimation == null && this.nextNibbleTimer <= 0)
			{
				int nibbles = Game1.random.Next(2, 8);
				List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
				for (int i = 0; i < nibbles; i++)
				{
					anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame, 200));
					anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame + 1, 200));
				}
				anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneNibbling), false));
				this.sprite.setCurrentAnimation(anim);
			}
			this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterCheckTimer <= 0 && !this.running)
			{
				if (Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 12, environment) != null)
				{
					this.running = true;
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(this.baseFrame + 2, 50),
						new FarmerSprite.AnimationFrame(this.baseFrame + 3, 50),
						new FarmerSprite.AnimationFrame(this.baseFrame + 4, 50),
						new FarmerSprite.AnimationFrame(this.baseFrame + 5, 120),
						new FarmerSprite.AnimationFrame(this.baseFrame + 6, 80),
						new FarmerSprite.AnimationFrame(this.baseFrame + 7, 50)
					});
					this.sprite.loop = true;
				}
				this.characterCheckTimer = 200;
			}
			if (this.running)
			{
				if (this.treeRunTimer > 0)
				{
					this.position.Y = this.position.Y - 4f;
				}
				else
				{
					this.position.X = this.position.X + (float)(this.flip ? -4 : 4);
				}
			}
			if (this.running && this.characterCheckTimer <= 0 && this.treeRunTimer <= 0)
			{
				this.characterCheckTimer = 100;
				Vector2 v = new Vector2((float)((int)(this.position.X / 64f)), (float)((int)this.position.Y / 64));
				TerrainFeature terrainFeature;
				if (environment.terrainFeatures.TryGetValue(v, out terrainFeature))
				{
					Tree tree = terrainFeature as Tree;
					if (tree != null)
					{
						this.treeRunTimer = 700;
						this.climbed = tree;
						this.treeTile = v;
						this.position = v * 64f;
						return false;
					}
				}
			}
			if (this.treeRunTimer > 0)
			{
				this.treeRunTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.treeRunTimer <= 0)
				{
					this.climbed.performUseAction(this.treeTile);
					return true;
				}
			}
			return base.update(time, environment);
		}

		// Token: 0x040025B8 RID: 9656
		private int nextNibbleTimer = 1000;

		// Token: 0x040025B9 RID: 9657
		private int treeRunTimer;

		// Token: 0x040025BA RID: 9658
		private int characterCheckTimer = 200;

		// Token: 0x040025BB RID: 9659
		private bool running;

		// Token: 0x040025BC RID: 9660
		private Tree climbed;

		// Token: 0x040025BD RID: 9661
		private Vector2 treeTile;
	}
}
