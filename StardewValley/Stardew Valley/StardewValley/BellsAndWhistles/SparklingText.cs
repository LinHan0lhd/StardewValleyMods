using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A9 RID: 937
	public class SparklingText
	{
		// Token: 0x060038EA RID: 14570 RVA: 0x002D18EC File Offset: 0x002CFAEC
		public SparklingText(SpriteFont font, string text, Color color, Color sparkleColor, bool rainbow = false, double sparkleFrequency = 0.1, int millisecondsDuration = 2500, int amplitude = -1, int speed = 500, float depth = 1f)
		{
			if (amplitude == -1)
			{
				amplitude = 64;
			}
			SparklingText.maxDistanceForSparkle = 32;
			this.font = font;
			this.color = color;
			this.sparkleColor = sparkleColor;
			this.text = text;
			this.rainbow = rainbow;
			if (rainbow)
			{
				color = Color.Yellow;
			}
			this.sparkleFrequency = sparkleFrequency;
			this.millisecondsDuration = millisecondsDuration;
			this.individualCharacterOffsets = new float[text.Length];
			this.amplitude = amplitude;
			this.period = speed;
			this.sparkles = new TemporaryAnimatedSpriteList();
			this.boundingBox = new Rectangle(-SparklingText.maxDistanceForSparkle, -SparklingText.maxDistanceForSparkle, (int)font.MeasureString(text).X + SparklingText.maxDistanceForSparkle * 2, (int)font.MeasureString(text).Y + SparklingText.maxDistanceForSparkle * 2);
			this.textWidth = font.MeasureString(text).X;
			this.layerDepth = depth;
			int xOffset = 0;
			for (int i = 0; i < text.Length; i++)
			{
				xOffset += (int)font.MeasureString(text[i].ToString() ?? "").X;
			}
			this.drawnTextWidth = (float)xOffset;
		}

		// Token: 0x060038EB RID: 14571 RVA: 0x002D1A3C File Offset: 0x002CFC3C
		public bool update(GameTime time)
		{
			this.millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
			this.offsetDecay -= 0.001f;
			this.amplitude = (int)((float)this.amplitude * this.offsetDecay);
			if (this.millisecondsDuration <= 500)
			{
				this.alpha = (float)this.millisecondsDuration / 500f;
			}
			for (int i = 0; i < this.individualCharacterOffsets.Length; i++)
			{
				this.individualCharacterOffsets[i] = (float)((double)(this.amplitude / 2) * Math.Sin(6.283185307179586 / (double)this.period * (double)(this.millisecondsDuration - i * 100)));
			}
			if (this.millisecondsDuration > 500 && Game1.random.NextDouble() < this.sparkleFrequency)
			{
				int speed = Game1.random.Next(100, 600);
				this.sparkles.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 704, 64, 64), (float)(speed / 6), 6, 0, new Vector2((float)Game1.random.Next(this.boundingBox.X, this.boundingBox.Right), (float)Game1.random.Next(this.boundingBox.Y, this.boundingBox.Bottom)), false, false, this.layerDepth, 0f, this.rainbow ? this.color : this.sparkleColor, 1f, 0f, 0f, 0f, false));
			}
			this.sparkles.RemoveWhere((TemporaryAnimatedSprite sparkle) => sparkle.update(time));
			if (this.rainbow)
			{
				this.incrementRainbowColors();
			}
			return this.millisecondsDuration <= 0;
		}

		// Token: 0x060038EC RID: 14572 RVA: 0x002D1C18 File Offset: 0x002CFE18
		private void incrementRainbowColors()
		{
			if (this.colorCycle == 0)
			{
				if ((this.color.G = this.color.G + 4) >= 255)
				{
					this.colorCycle = 1;
					return;
				}
				if (this.colorCycle == 1)
				{
					if ((this.color.R = this.color.R - 4) <= 0)
					{
						this.colorCycle = 2;
						return;
					}
					if (this.colorCycle == 2)
					{
						if ((this.color.B = this.color.B + 4) >= 255)
						{
							this.colorCycle = 3;
							return;
						}
						if (this.colorCycle == 3)
						{
							if ((this.color.G = this.color.G - 4) <= 0)
							{
								this.colorCycle = 4;
								return;
							}
							if (this.colorCycle == 4)
							{
								if ((this.color.R = this.color.R + 1) >= 255)
								{
									this.colorCycle = 5;
									return;
								}
								if (this.colorCycle == 5 && (this.color.B = this.color.B - 4) <= 0)
								{
									this.colorCycle = 0;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060038ED RID: 14573 RVA: 0x002D1D3C File Offset: 0x002CFF3C
		private static Color getRainbowColorFromIndex(int index)
		{
			switch (index % 8)
			{
			case 0:
				return Color.Red;
			case 1:
				return Color.Orange;
			case 2:
				return Color.Yellow;
			case 3:
				return Color.Chartreuse;
			case 4:
				return Color.Green;
			case 5:
				return Color.Cyan;
			case 6:
				return Color.Blue;
			case 7:
				return Color.Violet;
			default:
				return Color.White;
			}
		}

		// Token: 0x060038EE RID: 14574 RVA: 0x002D1DAC File Offset: 0x002CFFAC
		public void draw(SpriteBatch b, Vector2 onScreenPosition)
		{
			int xOffset = 0;
			for (int i = 0; i < this.text.Length; i++)
			{
				b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2((float)(xOffset - 2), this.individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2((float)(xOffset + 2), this.individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.991f);
				b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2((float)xOffset, this.individualCharacterOffsets[i] - 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.992f);
				b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2((float)xOffset, this.individualCharacterOffsets[i] + 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.993f);
				b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2((float)xOffset, this.individualCharacterOffsets[i]), this.rainbow ? SparklingText.getRainbowColorFromIndex(i) : (this.color * this.alpha), 0f, Vector2.Zero, 1f, SpriteEffects.None, this.layerDepth);
				xOffset += (int)this.font.MeasureString(this.text[i].ToString() ?? "").X;
			}
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.sparkles)
			{
				temporaryAnimatedSprite.Position += onScreenPosition;
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				temporaryAnimatedSprite.Position -= onScreenPosition;
			}
		}

		// Token: 0x0400257D RID: 9597
		public static int maxDistanceForSparkle = 32;

		// Token: 0x0400257E RID: 9598
		private SpriteFont font;

		// Token: 0x0400257F RID: 9599
		private Color color;

		// Token: 0x04002580 RID: 9600
		private Color sparkleColor;

		// Token: 0x04002581 RID: 9601
		private bool rainbow;

		// Token: 0x04002582 RID: 9602
		private int millisecondsDuration;

		// Token: 0x04002583 RID: 9603
		private int amplitude;

		// Token: 0x04002584 RID: 9604
		private int period;

		// Token: 0x04002585 RID: 9605
		private int colorCycle;

		// Token: 0x04002586 RID: 9606
		public string text;

		// Token: 0x04002587 RID: 9607
		private float[] individualCharacterOffsets;

		// Token: 0x04002588 RID: 9608
		public float offsetDecay = 1f;

		// Token: 0x04002589 RID: 9609
		public float alpha = 1f;

		// Token: 0x0400258A RID: 9610
		public float textWidth;

		// Token: 0x0400258B RID: 9611
		public float drawnTextWidth;

		// Token: 0x0400258C RID: 9612
		public float layerDepth = 1f;

		// Token: 0x0400258D RID: 9613
		private double sparkleFrequency;

		// Token: 0x0400258E RID: 9614
		private TemporaryAnimatedSpriteList sparkles;

		// Token: 0x0400258F RID: 9615
		private Rectangle boundingBox;
	}
}
