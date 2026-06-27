using System;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A8 RID: 936
	public class SebsFrogs : TemporaryAnimatedSprite
	{
		// Token: 0x060038E8 RID: 14568 RVA: 0x002D16F0 File Offset: 0x002CF8F0
		public override bool update(GameTime time)
		{
			base.update(time);
			if (!this.pingPong && this.motion.Equals(Vector2.Zero) && Game1.random.NextDouble() < 0.003)
			{
				if (Game1.random.NextDouble() < 0.4)
				{
					this.animationLength = 3;
					this.pingPong = true;
				}
				else
				{
					this.flipJump = !this.flipJump;
					this.yOriginal = this.position.Y;
					this.motion = new Vector2((float)(this.flipJump ? -1 : 1), -3f);
					this.acceleration = new Vector2(0f, 0.2f);
					this.sourceRect.X = 0;
					this.interval = (float)Game1.random.Next(110, 150);
					this.animationLength = 5;
					this.flipped = this.flipJump;
					if (base.Parent != null && base.Parent == Game1.currentLocation && Game1.random.NextDouble() < 0.03)
					{
						Game1.playSound("croak", null);
					}
				}
			}
			else if (this.pingPong && Game1.random.NextDouble() < 0.02 && this.sourceRect.X == 64)
			{
				this.animationLength = 1;
				this.pingPong = false;
				this.sourceRect.X = (int)this.sourceRectStartingPos.X;
			}
			if (!this.motion.Equals(Vector2.Zero) && this.position.Y > this.yOriginal)
			{
				this.motion = Vector2.Zero;
				this.acceleration = Vector2.Zero;
				this.sourceRect.X = 64;
				this.animationLength = 1;
				this.position.Y = this.yOriginal;
			}
			return false;
		}

		// Token: 0x0400257B RID: 9595
		private float yOriginal;

		// Token: 0x0400257C RID: 9596
		private bool flipJump;
	}
}
