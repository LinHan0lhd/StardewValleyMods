using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley
{
	// Token: 0x02000084 RID: 132
	[XmlInclude(typeof(BuildingPaintColor))]
	public class BuildingPainter
	{
		// Token: 0x060004EC RID: 1260 RVA: 0x00018F40 File Offset: 0x00017140
		public static Texture2D Apply(Texture2D base_texture, string mask_path, BuildingPaintColor color)
		{
			List<List<int>> paint_indices;
			if (!BuildingPainter.paintMaskLookup.TryGetValue(mask_path, out paint_indices))
			{
				try
				{
					Texture2D texture2D = Game1.content.Load<Texture2D>(mask_path);
					Color[] mask_pixels = new Color[texture2D.GetElementCount()];
					texture2D.GetData<Color>(mask_pixels);
					paint_indices = new List<List<int>>();
					for (int i = 0; i < 3; i++)
					{
						paint_indices.Add(new List<int>());
					}
					for (int j = 0; j < mask_pixels.Length; j++)
					{
						if (mask_pixels[j] == Color.Red)
						{
							paint_indices[0].Add(j);
						}
						else if (mask_pixels[j] == Color.Lime)
						{
							paint_indices[1].Add(j);
						}
						else if (mask_pixels[j] == Color.Blue)
						{
							paint_indices[2].Add(j);
						}
					}
					BuildingPainter.paintMaskLookup[mask_path] = paint_indices;
				}
				catch (Exception)
				{
					BuildingPainter.paintMaskLookup[mask_path] = null;
				}
			}
			if (paint_indices == null)
			{
				return null;
			}
			if (!color.RequiresRecolor())
			{
				return null;
			}
			Color[] painted_pixels = new Color[base_texture.GetElementCount()];
			base_texture.GetData<Color>(painted_pixels);
			Texture2D paintedTexture = new Texture2D(Game1.graphics.GraphicsDevice, base_texture.GetActualWidth(), base_texture.GetActualHeight())
			{
				Name = "@BuildingPainter.paintedTexture"
			};
			if (base_texture.Width != base_texture.GetActualWidth() || base_texture.Height != base_texture.GetActualHeight())
			{
				paintedTexture.SetContentSize(base_texture.Width, base_texture.Height);
			}
			if (!color.Color1Default.Value)
			{
				BuildingPainter._ApplyPaint(0, -100, 0, painted_pixels, paint_indices[0]);
				BuildingPainter._ApplyPaint(color.Color1Hue.Value, color.Color1Saturation.Value, color.Color1Lightness.Value, painted_pixels, paint_indices[0]);
			}
			if (!color.Color2Default.Value)
			{
				BuildingPainter._ApplyPaint(0, -100, 0, painted_pixels, paint_indices[1]);
				BuildingPainter._ApplyPaint(color.Color2Hue.Value, color.Color2Saturation.Value, color.Color2Lightness.Value, painted_pixels, paint_indices[1]);
			}
			if (!color.Color3Default.Value)
			{
				BuildingPainter._ApplyPaint(0, -100, 0, painted_pixels, paint_indices[2]);
				BuildingPainter._ApplyPaint(color.Color3Hue.Value, color.Color3Saturation.Value, color.Color3Lightness.Value, painted_pixels, paint_indices[2]);
			}
			paintedTexture.SetData<Color>(painted_pixels);
			return paintedTexture;
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x000191AC File Offset: 0x000173AC
		protected static void _ApplyPaint(int h_shift, int s_shift, int l_shift, Color[] pixels, List<int> indices)
		{
			foreach (int index in indices)
			{
				if (index < pixels.Length)
				{
					Color color = pixels[index];
					double h;
					double s;
					double i;
					Utility.RGBtoHSL((int)color.R, (int)color.G, (int)color.B, out h, out s, out i);
					h += (double)h_shift;
					s += (double)s_shift / 100.0;
					i += (double)l_shift / 100.0;
					while (h > 360.0)
					{
						h -= 360.0;
					}
					while (h < 0.0)
					{
						h += 360.0;
					}
					if (s < 0.0)
					{
						s = 0.0;
					}
					if (s > 1.0)
					{
						s = 1.0;
					}
					if (i < 0.0)
					{
						i = 0.0;
					}
					if (i > 1.0)
					{
						i = 1.0;
					}
					int r;
					int g;
					int b;
					Utility.HSLtoRGB(h, s, i, out r, out g, out b);
					color.R = (byte)r;
					color.G = (byte)g;
					color.B = (byte)b;
					pixels[index] = color;
				}
			}
		}

		// Token: 0x0400021E RID: 542
		[XmlIgnore]
		public static Dictionary<string, List<List<int>>> paintMaskLookup = new Dictionary<string, List<List<int>>>(StringComparer.OrdinalIgnoreCase);
	}
}
