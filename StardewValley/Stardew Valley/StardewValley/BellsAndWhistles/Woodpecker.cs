using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AF RID: 943
	public class Woodpecker : Critter
	{
		// Token: 0x0600392E RID: 14638 RVA: 0x002D61D8 File Offset: 0x002D43D8
		public Woodpecker(Tree tree, Vector2 position)
		{
			this.tree = tree;
			position *= 64f;
			this.position = position;
			this.position.X = this.position.X + 32f;
			this.position.Y = this.position.Y + 0f;
			this.startingPosition = position;
			this.baseFrame = 320;
			this.sprite = new AnimatedSprite(Critter.critterTexture, 320, 16, 16);
		}

		// Token: 0x0600392F RID: 14639 RVA: 0x002D6264 File Offset: 0x002D4464
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(-80f, -64f + this.yJumpOffset + this.yOffset)), 1f, 0, 0, Color.White, this.flip, 4f, 0f, false);
		}

		// Token: 0x06003930 RID: 14640 RVA: 0x002D62CC File Offset: 0x002D44CC
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, -4f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 16f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
		}

		// Token: 0x06003931 RID: 14641 RVA: 0x002D6391 File Offset: 0x002D4591
		private void donePecking(Farmer who)
		{
			this.peckTimer = Game1.random.Next(1000, 3000);
		}

		// Token: 0x06003932 RID: 14642 RVA: 0x002D63B0 File Offset: 0x002D45B0
		private void playFlap(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("batFlap", null);
			}
		}

		// Token: 0x06003933 RID: 14643 RVA: 0x002D63E0 File Offset: 0x002D45E0
		private void playPeck(Farmer who)
		{
			if (Utility.isOnScreen(this.position, 64))
			{
				Game1.playSound("Cowboy_gunshot", null);
			}
		}

		// Token: 0x06003934 RID: 14644 RVA: 0x002D6410 File Offset: 0x002D4610
		public override bool update(GameTime time, GameLocation environment)
		{
			if (environment == null || this.sprite == null || this.tree == null)
			{
				return true;
			}
			if (this.yJumpOffset < 0f && !this.flyingAway)
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
			this.peckTimer -= time.ElapsedGameTime.Milliseconds;
			if (!this.flyingAway && this.peckTimer <= 0 && this.sprite.CurrentAnimation == null)
			{
				int nibbles = Game1.random.Next(2, 8);
				List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
				for (int i = 0; i < nibbles; i++)
				{
					anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame, 100));
					anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame + 1, 100, false, false, new AnimatedSprite.endOfAnimationBehavior(this.playPeck), false));
				}
				anim.Add(new FarmerSprite.AnimationFrame(this.baseFrame, 200, false, false, new AnimatedSprite.endOfAnimationBehavior(this.donePecking), false));
				this.sprite.setCurrentAnimation(anim);
				this.sprite.loop = false;
			}
			this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterCheckTimer < 0)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(this.position / 64f, 6, environment);
				this.characterCheckTimer = 200;
				if ((f != null || this.tree.stump.Value) && !this.flyingAway)
				{
					this.flyingAway = true;
					if (f != null && f.Position.X > this.position.X)
					{
						this.flip = false;
					}
					else
					{
						this.flip = true;
					}
					this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 2)), 70),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 60, false, this.flip, new AnimatedSprite.endOfAnimationBehavior(this.playFlap), false),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 4)), 70),
						new FarmerSprite.AnimationFrame((int)((short)(this.baseFrame + 3)), 60)
					});
					this.sprite.loop = true;
				}
			}
			if (this.flyingAway)
			{
				if (!this.flip)
				{
					this.position.X = this.position.X - 6f;
				}
				else
				{
					this.position.X = this.position.X + 6f;
				}
				this.yOffset -= 1f;
			}
			return base.update(time, environment);
		}

		// Token: 0x040025EE RID: 9710
		public const int flyingSpeed = 6;

		// Token: 0x040025EF RID: 9711
		private bool flyingAway;

		// Token: 0x040025F0 RID: 9712
		private Tree tree;

		// Token: 0x040025F1 RID: 9713
		private int peckTimer;

		// Token: 0x040025F2 RID: 9714
		private int characterCheckTimer = 200;
	}
}
