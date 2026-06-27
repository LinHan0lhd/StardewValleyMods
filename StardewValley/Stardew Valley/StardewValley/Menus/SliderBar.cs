using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x020002A9 RID: 681
	public class SliderBar
	{
		// Token: 0x06002C7F RID: 11391 RVA: 0x00225C48 File Offset: 0x00223E48
		public SliderBar(int x, int y, int initialValue)
		{
			this.bounds = new Rectangle(x, y, SliderBar.defaultWidth, 20);
			this.value = initialValue;
		}

		// Token: 0x06002C80 RID: 11392 RVA: 0x00225C6C File Offset: 0x00223E6C
		public int click(int x, int y)
		{
			if (this.bounds.Contains(x, y))
			{
				x -= this.bounds.X;
				this.value = (int)((float)x / (float)this.bounds.Width * 100f);
			}
			return this.value;
		}

		// Token: 0x06002C81 RID: 11393 RVA: 0x00225CB9 File Offset: 0x00223EB9
		public void changeValueBy(int amount)
		{
			this.value += amount;
			this.value = Math.Max(0, Math.Min(100, this.value));
		}

		// Token: 0x06002C82 RID: 11394 RVA: 0x00225CE2 File Offset: 0x00223EE2
		public void release(int x, int y)
		{
		}

		// Token: 0x06002C83 RID: 11395 RVA: 0x00225CE4 File Offset: 0x00223EE4
		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X, this.bounds.Center.Y - 2, this.bounds.Width, 4), Color.DarkGray);
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.bounds.X + (int)((float)this.value / 100f * (float)this.bounds.Width) + 4), (float)this.bounds.Center.Y), new Rectangle?(new Rectangle(64, 256, 32, 32)), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
		}

		// Token: 0x04001E56 RID: 7766
		public static int defaultWidth = 128;

		// Token: 0x04001E57 RID: 7767
		public const int defaultHeight = 20;

		// Token: 0x04001E58 RID: 7768
		public int value;

		// Token: 0x04001E59 RID: 7769
		public Rectangle bounds;
	}
}
