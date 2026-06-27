using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000261 RID: 609
	public class ColorPicker
	{
		// Token: 0x0600285A RID: 10330 RVA: 0x001D6F30 File Offset: 0x001D5130
		public ColorPicker(string name, int x, int y)
		{
			this.Name = name;
			this.hueBar = new SliderBar(0, 0, 50);
			this.saturationBar = new SliderBar(0, 20, 50);
			this.valueBar = new SliderBar(0, 40, 50);
			this.bounds = new Rectangle(x, y, SliderBar.defaultWidth, 60);
		}

		// Token: 0x0600285B RID: 10331 RVA: 0x001D6F90 File Offset: 0x001D5190
		public Color getSelectedColor()
		{
			return ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)this.saturationBar.value / 100.0, (double)this.valueBar.value / 100.0);
		}

		// Token: 0x0600285C RID: 10332 RVA: 0x001D6FF0 File Offset: 0x001D51F0
		public Color click(int x, int y)
		{
			if (this.bounds.Contains(x, y))
			{
				x -= this.bounds.X;
				y -= this.bounds.Y;
				if (this.hueBar.bounds.Contains(x, y))
				{
					this.hueBar.click(x, y);
					this.recentSliderBar = this.hueBar;
				}
				if (this.saturationBar.bounds.Contains(x, y))
				{
					this.recentSliderBar = this.saturationBar;
					this.saturationBar.click(x, y);
				}
				if (this.valueBar.bounds.Contains(x, y))
				{
					this.recentSliderBar = this.valueBar;
					this.valueBar.click(x, y);
				}
			}
			return this.getSelectedColor();
		}

		// Token: 0x0600285D RID: 10333 RVA: 0x001D70BD File Offset: 0x001D52BD
		public void changeHue(int amount)
		{
			this.hueBar.changeValueBy(amount);
			this.recentSliderBar = this.hueBar;
		}

		// Token: 0x0600285E RID: 10334 RVA: 0x001D70D7 File Offset: 0x001D52D7
		public void changeSaturation(int amount)
		{
			this.saturationBar.changeValueBy(amount);
			this.recentSliderBar = this.saturationBar;
		}

		// Token: 0x0600285F RID: 10335 RVA: 0x001D70F1 File Offset: 0x001D52F1
		public void changeValue(int amount)
		{
			this.valueBar.changeValueBy(amount);
			this.recentSliderBar = this.valueBar;
		}

		// Token: 0x06002860 RID: 10336 RVA: 0x001D710C File Offset: 0x001D530C
		public Color clickHeld(int x, int y)
		{
			if (this.recentSliderBar != null)
			{
				x = Math.Max(x, this.bounds.X);
				x = Math.Min(x, this.bounds.Right - 1);
				y = this.recentSliderBar.bounds.Center.Y;
				x -= this.bounds.X;
				if (this.recentSliderBar.Equals(this.hueBar))
				{
					this.hueBar.click(x, y);
				}
				if (this.recentSliderBar.Equals(this.saturationBar))
				{
					this.saturationBar.click(x, y);
				}
				if (this.recentSliderBar.Equals(this.valueBar))
				{
					this.valueBar.click(x, y);
				}
			}
			return this.getSelectedColor();
		}

		// Token: 0x06002861 RID: 10337 RVA: 0x001D71DB File Offset: 0x001D53DB
		public void releaseClick()
		{
			this.hueBar.release(0, 0);
			this.saturationBar.release(0, 0);
			this.valueBar.release(0, 0);
			this.recentSliderBar = null;
		}

		// Token: 0x06002862 RID: 10338 RVA: 0x001D720C File Offset: 0x001D540C
		public void draw(SpriteBatch b)
		{
			for (int i = 0; i < 24; i++)
			{
				Color c = ColorPicker.HsvToRgb((double)i / 24.0 * 360.0, 0.9, 0.9);
				b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * i, this.bounds.Y + this.hueBar.bounds.Center.Y - 2, this.hueBar.bounds.Width / 24, 4), c);
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.bounds.X + (int)((float)this.hueBar.value / 100f * (float)this.hueBar.bounds.Width)), (float)(this.bounds.Y + this.hueBar.bounds.Center.Y)), new Rectangle?(new Rectangle(64, 256, 32, 32)), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, this.hueBar.value.ToString() ?? "", Game1.smallFont, new Vector2((float)(this.bounds.X + this.bounds.Width + 8), (float)(this.bounds.Y + this.hueBar.bounds.Y)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			for (int j = 0; j < 24; j++)
			{
				Color c2 = ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)j / 24.0, (double)this.valueBar.value / 100.0);
				b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * j, this.bounds.Y + this.saturationBar.bounds.Center.Y - 2, this.saturationBar.bounds.Width / 24, 4), c2);
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.bounds.X + (int)((float)this.saturationBar.value / 100f * (float)this.saturationBar.bounds.Width)), (float)(this.bounds.Y + this.saturationBar.bounds.Center.Y)), new Rectangle?(new Rectangle(64, 256, 32, 32)), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, this.saturationBar.value.ToString() ?? "", Game1.smallFont, new Vector2((float)(this.bounds.X + this.bounds.Width + 8), (float)(this.bounds.Y + this.saturationBar.bounds.Y)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			for (int k = 0; k < 24; k++)
			{
				Color c3 = ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)this.saturationBar.value / 100.0, (double)k / 24.0);
				b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * k, this.bounds.Y + this.valueBar.bounds.Center.Y - 2, this.valueBar.bounds.Width / 24, 4), c3);
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.bounds.X + (int)((float)this.valueBar.value / 100f * (float)this.valueBar.bounds.Width)), (float)(this.bounds.Y + this.valueBar.bounds.Center.Y)), new Rectangle?(new Rectangle(64, 256, 32, 32)), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, this.valueBar.value.ToString() ?? "", Game1.smallFont, new Vector2((float)(this.bounds.X + this.bounds.Width + 8), (float)(this.bounds.Y + this.valueBar.bounds.Y)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
		}

		// Token: 0x06002863 RID: 10339 RVA: 0x001D7780 File Offset: 0x001D5980
		public bool containsPoint(int x, int y)
		{
			return this.bounds.Contains(x, y);
		}

		// Token: 0x06002864 RID: 10340 RVA: 0x001D7790 File Offset: 0x001D5990
		public void setColor(Color color)
		{
			float hue;
			float sat;
			float value;
			ColorPicker.RGBtoHSV((float)color.R, (float)color.G, (float)color.B, out hue, out sat, out value);
			this.setHsvColor(hue, sat, value);
		}

		// Token: 0x06002865 RID: 10341 RVA: 0x001D77CC File Offset: 0x001D59CC
		public void setHsvColor(float hue, float sat, float value)
		{
			if (float.IsNaN(hue))
			{
				hue = 0f;
			}
			if (float.IsNaN(sat))
			{
				sat = 0f;
			}
			if (float.IsNaN(hue))
			{
				hue = 0f;
			}
			this.hueBar.value = (int)(hue / 360f * 100f);
			this.saturationBar.value = (int)(sat * 100f);
			this.valueBar.value = (int)(value / 255f * 100f);
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x001D784C File Offset: 0x001D5A4C
		public static void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
		{
			float min = Math.Min(Math.Min(r, g), b);
			float max = Math.Max(Math.Max(r, g), b);
			v = max;
			float delta = max - min;
			if (max != 0f)
			{
				s = delta / max;
				if (r == max)
				{
					h = (g - b) / delta;
				}
				else if (g == max)
				{
					h = 2f + (b - r) / delta;
				}
				else
				{
					h = 4f + (r - g) / delta;
				}
				h *= 60f;
				if (h < 0f)
				{
					h += 360f;
				}
				return;
			}
			s = 0f;
			h = -1f;
		}

		// Token: 0x06002867 RID: 10343 RVA: 0x001D78E8 File Offset: 0x001D5AE8
		public static Color HsvToRgb(double hue, double saturation, double value)
		{
			double H = hue;
			while (H < 0.0)
			{
				H += 1.0;
				if (H < -1000000.0)
				{
					H = 0.0;
				}
			}
			while (H >= 360.0)
			{
				H -= 1.0;
			}
			double B;
			double R;
			double G;
			if (value <= 0.0)
			{
				G = (R = (B = 0.0));
			}
			else if (saturation <= 0.0)
			{
				B = value;
				G = value;
				R = value;
			}
			else
			{
				double num = H / 60.0;
				int i = (int)Math.Floor(num);
				double f = num - (double)i;
				double pv = value * (1.0 - saturation);
				double qv = value * (1.0 - saturation * f);
				double tv = value * (1.0 - saturation * (1.0 - f));
				switch (i)
				{
				case -1:
					R = value;
					G = pv;
					B = qv;
					break;
				case 0:
					R = value;
					G = tv;
					B = pv;
					break;
				case 1:
					R = qv;
					G = value;
					B = pv;
					break;
				case 2:
					R = pv;
					G = value;
					B = tv;
					break;
				case 3:
					R = pv;
					G = qv;
					B = value;
					break;
				case 4:
					R = tv;
					G = pv;
					B = value;
					break;
				case 5:
					R = value;
					G = pv;
					B = qv;
					break;
				case 6:
					R = value;
					G = tv;
					B = pv;
					break;
				default:
					B = value;
					G = value;
					R = value;
					break;
				}
			}
			return new Color(ColorPicker.Clamp((int)(R * 255.0)), ColorPicker.Clamp((int)(G * 255.0)), ColorPicker.Clamp((int)(B * 255.0)));
		}

		// Token: 0x06002868 RID: 10344 RVA: 0x001D7A87 File Offset: 0x001D5C87
		public static int Clamp(int value)
		{
			if (value < 0)
			{
				return 0;
			}
			if (value > 255)
			{
				return 255;
			}
			return value;
		}

		// Token: 0x04001A23 RID: 6691
		public const int sliderChunks = 24;

		// Token: 0x04001A24 RID: 6692
		private Rectangle bounds;

		// Token: 0x04001A25 RID: 6693
		public SliderBar hueBar;

		// Token: 0x04001A26 RID: 6694
		public SliderBar valueBar;

		// Token: 0x04001A27 RID: 6695
		public SliderBar saturationBar;

		// Token: 0x04001A28 RID: 6696
		public SliderBar recentSliderBar;

		// Token: 0x04001A29 RID: 6697
		public string Name;

		// Token: 0x04001A2A RID: 6698
		public Color LastColor;

		// Token: 0x04001A2B RID: 6699
		public bool Dirty;
	}
}
