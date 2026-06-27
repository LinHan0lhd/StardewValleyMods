using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x02000396 RID: 918
	public class EmilysParrot : TemporaryAnimatedSprite
	{
		// Token: 0x06003850 RID: 14416 RVA: 0x002C8F70 File Offset: 0x002C7170
		public EmilysParrot(Vector2 location)
		{
			this.texture = Game1.mouseCursors;
			this.sourceRect = new Rectangle(92, 148, 9, 16);
			this.sourceRectStartingPos = new Vector2(92f, 149f);
			this.position = location;
			this.initialPosition = this.position;
			this.scale = 4f;
			this.id = 5858585;
		}

		// Token: 0x06003851 RID: 14417 RVA: 0x002C8FE4 File Offset: 0x002C71E4
		public void doAction()
		{
			Game1.playSound("parrot", null);
			this.shakeTimer = 800;
		}

		// Token: 0x06003852 RID: 14418 RVA: 0x002C9010 File Offset: 0x002C7210
		public override bool update(GameTime time)
		{
			this.currentPhaseTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.currentPhaseTimer <= 0)
			{
				this.currentPhase = Game1.random.Next(5);
				this.currentPhaseTimer = Game1.random.Next(4000, 16000);
				if (this.currentPhase == 1)
				{
					this.currentPhaseTimer /= 2;
					this.updateFlappingPhase();
				}
				else
				{
					this.position = this.initialPosition;
				}
			}
			if (this.shakeTimer > 0)
			{
				this.shakeIntensity = 1f;
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.shakeIntensity = 0f;
			}
			this.currentFrameTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.currentFrameTimer <= 0)
			{
				switch (this.currentPhase)
				{
				case 0:
					if (this.currentFrame == 7)
					{
						this.currentFrame = 0;
						this.currentFrameTimer = 600;
					}
					else if (Game1.random.NextBool())
					{
						this.currentFrame = 7;
						this.currentFrameTimer = 300;
					}
					break;
				case 1:
					this.updateFlappingPhase();
					this.currentFrameTimer = 0;
					break;
				case 2:
					this.currentFrame = Game1.random.Next(3, 5);
					this.currentFrameTimer = 1000;
					break;
				case 3:
					if (this.currentFrame == 5)
					{
						this.currentFrame = 6;
					}
					else
					{
						this.currentFrame = 5;
					}
					this.currentFrameTimer = 1000;
					break;
				case 4:
					if (this.currentFrame == 1 && Game1.random.NextDouble() < 0.1)
					{
						this.currentFrame = 2;
					}
					else if (this.currentFrame == 2)
					{
						this.currentFrame = 1;
					}
					else
					{
						this.currentFrame = Game1.random.Next(2);
					}
					this.currentFrameTimer = 500;
					break;
				}
			}
			if (this.currentPhase == 1 && this.currentFrame != 0)
			{
				this.sourceRect.X = 38 + this.currentFrame * 13;
				this.sourceRect.Width = 13;
			}
			else
			{
				this.sourceRect.X = 92 + this.currentFrame * 9;
				this.sourceRect.Width = 9;
			}
			return false;
		}

		// Token: 0x06003853 RID: 14419 RVA: 0x002C926C File Offset: 0x002C746C
		private void updateFlappingPhase()
		{
			this.currentFrame = 6 - this.currentPhaseTimer % 1000 / 166;
			this.currentFrame = 3 - Math.Abs(this.currentFrame - 3);
			this.position.Y = this.initialPosition.Y - (float)(4 * (3 - this.currentFrame));
			if (this.currentFrame == 0)
			{
				this.position.X = this.initialPosition.X;
				return;
			}
			this.position.X = this.initialPosition.X - 8f;
		}

		// Token: 0x040024CA RID: 9418
		public const int flappingPhase = 1;

		// Token: 0x040024CB RID: 9419
		public const int hoppingPhase = 0;

		// Token: 0x040024CC RID: 9420
		public const int lookingSidewaysPhase = 2;

		// Token: 0x040024CD RID: 9421
		public const int nappingPhase = 3;

		// Token: 0x040024CE RID: 9422
		public const int headBobbingPhase = 4;

		// Token: 0x040024CF RID: 9423
		private int currentFrame;

		// Token: 0x040024D0 RID: 9424
		private int currentFrameTimer;

		// Token: 0x040024D1 RID: 9425
		private int currentPhaseTimer;

		// Token: 0x040024D2 RID: 9426
		private int currentPhase;

		// Token: 0x040024D3 RID: 9427
		private int shakeTimer;
	}
}
