using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x020000EC RID: 236
	public class NumberSprite
	{
		// Token: 0x0600128A RID: 4746 RVA: 0x000DBC98 File Offset: 0x000D9E98
		public static void draw(int number, SpriteBatch b, Vector2 position, Color c, float scale, float layerDepth, float alpha, int secondDigitOffset, int spaceBetweenDigits = 0)
		{
			int digit = 1;
			secondDigitOffset = Math.Min(0, secondDigitOffset);
			do
			{
				int currentDigit = number % 10;
				number /= 10;
				int textX = 512 + currentDigit * 8 % 48;
				int textY = 128 + currentDigit * 8 / 48 * 8;
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, (float)((digit == 2) ? secondDigitOffset : 0)), new Rectangle?(new Rectangle(textX, textY, 8, 8)), c * alpha, 0f, new Vector2(4f, 4f), 4f * scale, SpriteEffects.None, layerDepth);
				position.X -= 8f * scale * 4f - 4f - (float)spaceBetweenDigits;
				digit++;
			}
			while (number > 0);
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x000DBD60 File Offset: 0x000D9F60
		public static int getHeight()
		{
			return 8;
		}

		// Token: 0x0600128C RID: 4748 RVA: 0x000DBD63 File Offset: 0x000D9F63
		public static int getWidth(string number)
		{
			return NumberSprite.getWidth(Convert.ToInt32(number));
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x000DBD70 File Offset: 0x000D9F70
		public static int getWidth(int number)
		{
			int width = 8;
			number /= 10;
			while (number != 0)
			{
				number /= 10;
				width += 8;
			}
			return width;
		}

		// Token: 0x0600128E RID: 4750 RVA: 0x000DBD98 File Offset: 0x000D9F98
		public static int numberOfDigits(int number)
		{
			int num = 1;
			number /= 10;
			while (number != 0)
			{
				number /= 10;
				num++;
			}
			return num;
		}

		// Token: 0x04000B09 RID: 2825
		public const int textureX = 512;

		// Token: 0x04000B0A RID: 2826
		public const int textureY = 128;

		// Token: 0x04000B0B RID: 2827
		public const int digitWidth = 8;

		// Token: 0x04000B0C RID: 2828
		public const int digitHeight = 8;

		// Token: 0x04000B0D RID: 2829
		public const int groupWidth = 48;
	}
}
