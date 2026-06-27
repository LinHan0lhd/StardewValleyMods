using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000114 RID: 276
	public class MapAreaTexture
	{
		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06001790 RID: 6032 RVA: 0x0011100E File Offset: 0x0010F20E
		public Texture2D Texture { get; }

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06001791 RID: 6033 RVA: 0x00111016 File Offset: 0x0010F216
		public Rectangle SourceRect { get; }

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06001792 RID: 6034 RVA: 0x0011101E File Offset: 0x0010F21E
		public Rectangle MapPixelArea { get; }

		// Token: 0x06001793 RID: 6035 RVA: 0x00111026 File Offset: 0x0010F226
		public MapAreaTexture(Texture2D texture, Rectangle sourceRect, Rectangle mapPixelArea)
		{
			this.Texture = texture;
			this.SourceRect = sourceRect;
			this.MapPixelArea = mapPixelArea;
		}

		// Token: 0x06001794 RID: 6036 RVA: 0x00111043 File Offset: 0x0010F243
		public Rectangle GetOffsetMapPixelArea(int x, int y)
		{
			return new Rectangle(this.MapPixelArea.X + x, this.MapPixelArea.Y + y, this.MapPixelArea.Width, this.MapPixelArea.Height);
		}
	}
}
