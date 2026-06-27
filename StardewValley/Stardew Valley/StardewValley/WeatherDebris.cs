using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x0200010F RID: 271
	[InstanceStatics]
	public class WeatherDebris
	{
		// Token: 0x0600174C RID: 5964 RVA: 0x0010F88C File Offset: 0x0010DA8C
		public WeatherDebris(Vector2 position, int which, float rotationVelocity, float dx, float dy)
		{
			this.position = position;
			this.which = which;
			this.dx = dx;
			this.dy = dy;
			switch (which)
			{
			case 0:
				this.sourceRect = new Rectangle(352, 1184, 16, 16);
				this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				return;
			case 1:
				this.sourceRect = new Rectangle(352, 1200, 16, 16);
				this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				return;
			case 2:
				this.sourceRect = new Rectangle(352, 1216, 16, 16);
				this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				return;
			case 3:
				this.sourceRect = new Rectangle(391 + 4 * Game1.random.Next(5), 1236, 4, 4);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600174D RID: 5965 RVA: 0x0010F99B File Offset: 0x0010DB9B
		public void update()
		{
			this.update(false);
		}

		// Token: 0x0600174E RID: 5966 RVA: 0x0010F9A4 File Offset: 0x0010DBA4
		public void update(bool slow)
		{
			this.position.X = this.position.X + (this.dx + (slow ? 0f : WeatherDebris.globalWind));
			this.position.Y = this.position.Y + (this.dy - (slow ? 0f : -0.5f));
			if (this.dy < 0f && !this.blowing)
			{
				this.dy += 0.01f;
			}
			if (!Game1.fadeToBlack && Game1.fadeToBlackAlpha <= 0f)
			{
				if (this.position.X < -80f)
				{
					this.position.X = (float)Game1.viewport.Width;
					this.position.Y = (float)Game1.random.Next(0, Game1.viewport.Height - 64);
				}
				if (this.position.Y > (float)(Game1.viewport.Height + 16))
				{
					this.position.X = (float)Game1.random.Next(0, Game1.viewport.Width);
					this.position.Y = -64f;
					this.dy = (float)Game1.random.Next(-15, 10) / (slow ? ((Game1.random.NextDouble() < 0.1) ? 5f : 200f) : 50f);
					this.dx = (float)Game1.random.Next(-10, 0) / (slow ? 200f : 50f);
				}
				else if (this.position.Y < -64f)
				{
					this.position.Y = (float)Game1.viewport.Height;
					this.position.X = (float)Game1.random.Next(0, Game1.viewport.Width);
				}
			}
			if (this.blowing)
			{
				this.dy -= 0.01f;
				if (Game1.random.NextDouble() < 0.006 || this.dy < -2f)
				{
					this.blowing = false;
				}
			}
			else if (!slow && Game1.random.NextDouble() < 0.001 && (Game1.IsSpring || Game1.IsSummer))
			{
				this.blowing = true;
			}
			int num = this.which;
			if (num <= 3)
			{
				this.animationTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (this.animationTimer <= 0)
				{
					this.animationTimer = 100 + this.animationIntervalOffset;
					this.animationIndex += this.animationDirection;
					if (this.animationDirection == 0)
					{
						if (this.animationIndex >= 9)
						{
							this.animationDirection = -1;
						}
						else
						{
							this.animationDirection = 1;
						}
					}
					if (this.animationIndex > 10)
					{
						if (Game1.random.NextDouble() < 0.82)
						{
							this.animationIndex--;
							this.animationDirection = 0;
							this.dx += 0.1f;
							this.dy -= 0.2f;
						}
						else
						{
							this.animationIndex = 0;
						}
					}
					else if (this.animationIndex == 4 && this.animationDirection == -1)
					{
						this.animationIndex++;
						this.animationDirection = 0;
						this.dx -= 0.1f;
						this.dy -= 0.1f;
					}
					if (this.animationIndex == 7 && this.animationDirection == -1)
					{
						this.dy -= 0.2f;
					}
					if (this.which != 3)
					{
						this.sourceRect.X = 352 + this.animationIndex * 16;
					}
				}
			}
		}

		// Token: 0x0600174F RID: 5967 RVA: 0x0010FD6C File Offset: 0x0010DF6C
		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, this.position, new Rectangle?(this.sourceRect), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
		}

		// Token: 0x04000E0E RID: 3598
		public const int pinkPetals = 0;

		// Token: 0x04000E0F RID: 3599
		public const int greenLeaves = 1;

		// Token: 0x04000E10 RID: 3600
		public const int fallLeaves = 2;

		// Token: 0x04000E11 RID: 3601
		public const int snow = 3;

		// Token: 0x04000E12 RID: 3602
		public const int animationInterval = 100;

		// Token: 0x04000E13 RID: 3603
		public const float gravity = -0.5f;

		// Token: 0x04000E14 RID: 3604
		public Vector2 position;

		// Token: 0x04000E15 RID: 3605
		public Rectangle sourceRect;

		// Token: 0x04000E16 RID: 3606
		public int which;

		// Token: 0x04000E17 RID: 3607
		public int animationIndex;

		// Token: 0x04000E18 RID: 3608
		public int animationTimer = 100;

		// Token: 0x04000E19 RID: 3609
		public int animationDirection = 1;

		// Token: 0x04000E1A RID: 3610
		public int animationIntervalOffset;

		// Token: 0x04000E1B RID: 3611
		public float dx;

		// Token: 0x04000E1C RID: 3612
		public float dy;

		// Token: 0x04000E1D RID: 3613
		public static float globalWind = -0.25f;

		// Token: 0x04000E1E RID: 3614
		private bool blowing;
	}
}
