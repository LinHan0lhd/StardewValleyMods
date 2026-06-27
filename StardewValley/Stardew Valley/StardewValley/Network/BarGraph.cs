using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Network
{
	// Token: 0x020001C7 RID: 455
	public class BarGraph
	{
		// Token: 0x0600201F RID: 8223 RVA: 0x0016E380 File Offset: 0x0016C580
		public BarGraph(Queue<double> elements, int x, int y, int width, int height, int elementWidth, double maxValue, Color barColor, Texture2D whiteTexture)
		{
			this.elements = elements;
			this.width = width;
			this.height = height;
			this.x = x;
			this.y = y;
			this.maxValue = maxValue;
			this.barColor = barColor;
			this.elementWidth = elementWidth;
			this.whiteTexture = whiteTexture;
		}

		// Token: 0x06002020 RID: 8224 RVA: 0x0016E3D8 File Offset: 0x0016C5D8
		public void Draw(SpriteBatch sb)
		{
			double scaleMaxValue = this.maxValue;
			if (scaleMaxValue == BarGraph.DYNAMIC_SCALE_MAX)
			{
				using (Queue<double>.Enumerator enumerator = this.elements.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						double val = enumerator.Current;
						scaleMaxValue = Math.Max(val, scaleMaxValue);
					}
					goto IL_A4;
				}
			}
			if (scaleMaxValue == BarGraph.DYNAMIC_SCALE_AVG)
			{
				double total = 0.0;
				foreach (double element in this.elements)
				{
					total += element;
				}
				scaleMaxValue = total / (double)Math.Max(1, this.elements.Count);
			}
			IL_A4:
			sb.Draw(this.whiteTexture, new Rectangle(this.x - 1, this.y, this.width, this.height), null, Color.Black * 0.5f);
			int leftX = this.x + this.width - this.elementWidth * this.elements.Count;
			int i = 0;
			using (Queue<double>.Enumerator enumerator = this.elements.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					float num = (float)enumerator.Current;
					int elementX = leftX + i * this.elementWidth;
					int elementY = this.y;
					int elementHeight = (int)((double)num / scaleMaxValue * (double)this.height);
					sb.Draw(this.whiteTexture, new Rectangle(elementX, elementY + this.height - elementHeight, this.elementWidth, elementHeight), null, this.barColor);
					i++;
				}
			}
		}

		// Token: 0x0400139C RID: 5020
		public static double DYNAMIC_SCALE_MAX = -1.0;

		// Token: 0x0400139D RID: 5021
		public static double DYNAMIC_SCALE_AVG = -2.0;

		// Token: 0x0400139E RID: 5022
		private Queue<double> elements;

		// Token: 0x0400139F RID: 5023
		private int height;

		// Token: 0x040013A0 RID: 5024
		private int width;

		// Token: 0x040013A1 RID: 5025
		private int x;

		// Token: 0x040013A2 RID: 5026
		private int y;

		// Token: 0x040013A3 RID: 5027
		private double maxValue;

		// Token: 0x040013A4 RID: 5028
		private Color barColor;

		// Token: 0x040013A5 RID: 5029
		private int elementWidth;

		// Token: 0x040013A6 RID: 5030
		private Texture2D whiteTexture;
	}
}
