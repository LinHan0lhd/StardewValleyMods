using System;
using Microsoft.Xna.Framework;
using StardewValley.GameData.WorldMaps;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000115 RID: 277
	public class MapAreaTooltip
	{
		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06001795 RID: 6037 RVA: 0x0011107A File Offset: 0x0010F27A
		public MapArea Area { get; }

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06001796 RID: 6038 RVA: 0x00111082 File Offset: 0x0010F282
		public WorldMapTooltipData Data { get; }

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06001797 RID: 6039 RVA: 0x0011108A File Offset: 0x0010F28A
		public string Text { get; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06001798 RID: 6040 RVA: 0x00111092 File Offset: 0x0010F292
		public string NamespacedId { get; }

		// Token: 0x06001799 RID: 6041 RVA: 0x0011109A File Offset: 0x0010F29A
		public MapAreaTooltip(MapArea mapArea, WorldMapTooltipData data, string text)
		{
			this.Area = mapArea;
			this.Data = data;
			this.Text = text;
			this.NamespacedId = mapArea.Id + "/" + data.Id;
		}

		// Token: 0x0600179A RID: 6042 RVA: 0x001110D4 File Offset: 0x0010F2D4
		public Rectangle GetPixelArea()
		{
			Rectangle? cachedPixelArea = this.CachedPixelArea;
			if (cachedPixelArea == null)
			{
				Rectangle area = this.Data.PixelArea;
				if (area.IsEmpty)
				{
					area = this.Area.Data.PixelArea;
				}
				this.CachedPixelArea = new Rectangle?(new Rectangle(area.X * 4, area.Y * 4, area.Width * 4, area.Height * 4));
			}
			return this.CachedPixelArea.Value;
		}

		// Token: 0x04000E39 RID: 3641
		protected Rectangle? CachedPixelArea;
	}
}
