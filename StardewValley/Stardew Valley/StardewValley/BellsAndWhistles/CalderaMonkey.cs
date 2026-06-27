using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000391 RID: 913
	public class CalderaMonkey : Critter
	{
		// Token: 0x06003830 RID: 14384 RVA: 0x002C756C File Offset: 0x002C576C
		public CalderaMonkey()
		{
			this.sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			this.sprite.SourceRect = this._baseSourceRectangle;
			this.sprite.ignoreSourceRectUpdates = true;
			this.texture = Game1.temporaryContent.Load<Texture2D>(Critter.critterTexture);
			this.swimShadow = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\swimShadow");
		}

		// Token: 0x06003831 RID: 14385 RVA: 0x002C7608 File Offset: 0x002C5808
		public CalderaMonkey(Vector2 start_position) : this()
		{
			this.position = start_position;
			this.sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			this.sprite.SourceRect = this._baseSourceRectangle;
			this.sprite.ignoreSourceRectUpdates = true;
			if (Game1.random.NextBool())
			{
				this.buddies.Add(new Vector2(-96f, 76.8f) + this.position);
			}
			if (Game1.random.NextBool())
			{
				this.buddies.Add(new Vector2(32f, 134.4f) + this.position);
			}
			if (Game1.random.NextBool())
			{
				this.buddies.Add(new Vector2(128f, 44.8f) + this.position);
			}
			this.texture = Game1.temporaryContent.Load<Texture2D>(Critter.critterTexture);
		}

		// Token: 0x06003832 RID: 14386 RVA: 0x002C76FC File Offset: 0x002C58FC
		public override bool update(GameTime time, GameLocation environment)
		{
			this.nextFrameTimer -= (float)((int)time.ElapsedGameTime.TotalMilliseconds);
			if (this.nextPhaseTimer >= 0f)
			{
				this.nextPhaseTimer -= (float)((int)time.ElapsedGameTime.TotalMilliseconds);
				if (this.nextPhaseTimer <= 0f)
				{
					if (this.currentPhase != 3 || Game1.random.NextDouble() >= 0.2)
					{
						this.currentPhase = Game1.random.Next(4);
					}
					this.nextFrameTimer = 0f;
					switch (this.currentPhase)
					{
					case 0:
						this.currentFrameDelay = (float)Game1.random.Next(400, 500);
						this.nextPhaseTimer = (float)Game1.random.Next(3000, 8000);
						break;
					case 1:
						this.currentFrameDelay = (float)Game1.random.Next(300, 1200);
						this.nextPhaseTimer = (float)Game1.random.Next(3000, 6000);
						break;
					case 2:
						this.nextPhaseTimer = (float)Game1.random.Next(3000, 8000);
						break;
					case 3:
						this.nextPhaseTimer = (float)Game1.random.Next(700, 3000);
						this.nextFrameTimer = 400f;
						if (Game1.activeClickableMenu == null)
						{
							environment.playSound("monkey1", null, null, SoundContext.Default);
						}
						this.setFrame(5);
						break;
					}
				}
			}
			switch (this.currentPhase)
			{
			case 0:
				if (this.nextFrameTimer <= 0f)
				{
					if (this.currentFrame == 0)
					{
						this.setFrame(1);
					}
					else
					{
						this.setFrame(0);
					}
					if (Game1.random.NextDouble() < 0.2)
					{
						this.setFrame(6);
						this.nextFrameTimer = 200f;
					}
					else
					{
						this.nextFrameTimer = this.currentFrameDelay;
					}
				}
				break;
			case 1:
				if (this.nextFrameTimer <= 0f)
				{
					if (this.currentFrame == 2)
					{
						this.setFrame(3);
					}
					else
					{
						this.setFrame(2);
					}
					this.nextFrameTimer = this.currentFrameDelay;
					if (Game1.activeClickableMenu == null)
					{
						environment.playSound("slosh", null, null, SoundContext.Default);
					}
					environment.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 3, 0, this.position + new Vector2((float)((this.currentFrame == 2) ? 32 : -8), 48f), false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
				}
				break;
			case 2:
				this.setFrame(4);
				break;
			case 3:
				if (this.nextFrameTimer <= 0f)
				{
					this.setFrame(0);
				}
				break;
			}
			return base.update(time, environment);
		}

		// Token: 0x06003833 RID: 14387 RVA: 0x002C7A24 File Offset: 0x002C5C24
		private void setFrame(int frame)
		{
			this.sprite.sourceRect.X = frame * 20;
			this.currentFrame = frame;
		}

		// Token: 0x06003834 RID: 14388 RVA: 0x002C7A44 File Offset: 0x002C5C44
		public override void draw(SpriteBatch b)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(this.position + new Vector2(0f, -20f + this.yJumpOffset + this.yOffset))), (this.position.Y + 64f - 32f) / 10000f, 0, 0, Color.White, this.flip, 4f, 0f, false);
			for (int i = 0; i < this.buddies.Count; i++)
			{
				float yOffset = (float)Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds) / 500.0 + (double)(i * 100)) * 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, this.buddies[i]);
				b.Draw(this.texture, position + new Vector2(0f, yOffset), new Rectangle?(new Rectangle(14 * i, 333, 14, 12 - (int)yOffset / 2)), Color.White, 0f, Vector2.Zero, 4f, (position.X > 1408f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.buddies[i].Y / 10000f);
				b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)yOffset + 8, (int)position.Y + 44 + 2, 56 - (int)yOffset * 2 - 16, 4), new Rectangle?(Game1.staminaRect.Bounds), new Color(255, 255, 150) * 0.55f, 0f, Vector2.Zero, SpriteEffects.None, this.buddies[i].Y / 10000f + 0.001f);
				b.Draw(this.swimShadow, position + new Vector2(-4f, 48f), new Rectangle?(new Rectangle((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 700 / 70 * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, this.buddies[i].Y / 10000f - 0.001f);
			}
		}

		// Token: 0x06003835 RID: 14389 RVA: 0x002C7CAB File Offset: 0x002C5EAB
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}

		// Token: 0x04002497 RID: 9367
		private const int phase_tailBOB = 0;

		// Token: 0x04002498 RID: 9368
		private const int phase_footPaddle = 1;

		// Token: 0x04002499 RID: 9369
		private const int phase_relaxing = 2;

		// Token: 0x0400249A RID: 9370
		private const int phase_scream = 3;

		// Token: 0x0400249B RID: 9371
		public Rectangle movementRectangle;

		// Token: 0x0400249C RID: 9372
		private int currentPhase;

		// Token: 0x0400249D RID: 9373
		private int currentFrame;

		// Token: 0x0400249E RID: 9374
		private float nextFrameTimer;

		// Token: 0x0400249F RID: 9375
		private float nextPhaseTimer;

		// Token: 0x040024A0 RID: 9376
		private float currentFrameDelay;

		// Token: 0x040024A1 RID: 9377
		protected Rectangle _baseSourceRectangle = new Rectangle(0, 309, 20, 24);

		// Token: 0x040024A2 RID: 9378
		protected Vector2 movementDirection = Vector2.Zero;

		// Token: 0x040024A3 RID: 9379
		private List<Vector2> buddies = new List<Vector2>();

		// Token: 0x040024A4 RID: 9380
		private Texture2D texture;

		// Token: 0x040024A5 RID: 9381
		private Texture2D swimShadow;
	}
}
