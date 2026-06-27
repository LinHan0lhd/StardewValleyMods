using System;
using Microsoft.Xna.Framework;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000113 RID: 275
	public readonly struct MapAreaPositionWithContext
	{
		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06001789 RID: 6025 RVA: 0x00110F9A File Offset: 0x0010F19A
		public MapAreaPosition Data { get; }

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x0600178A RID: 6026 RVA: 0x00110FA2 File Offset: 0x0010F1A2
		public GameLocation Location { get; }

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x0600178B RID: 6027 RVA: 0x00110FAA File Offset: 0x0010F1AA
		public Point Tile { get; }

		// Token: 0x0600178C RID: 6028 RVA: 0x00110FB2 File Offset: 0x0010F1B2
		public MapAreaPositionWithContext(MapAreaPosition data, GameLocation location, Point tile)
		{
			this.Data = data;
			this.Location = location;
			this.Tile = tile;
		}

		// Token: 0x0600178D RID: 6029 RVA: 0x00110FC9 File Offset: 0x0010F1C9
		public Vector2 GetMapPixelPosition()
		{
			return this.Data.GetMapPixelPosition(this.Location, this.Tile);
		}

		// Token: 0x0600178E RID: 6030 RVA: 0x00110FE2 File Offset: 0x0010F1E2
		public Vector2? GetPositionRatioIfValid()
		{
			return this.Data.GetPositionRatioIfValid(this.Location, this.Tile);
		}

		// Token: 0x0600178F RID: 6031 RVA: 0x00110FFB File Offset: 0x0010F1FB
		public string GetScrollText()
		{
			return this.Data.GetScrollText(this.Tile);
		}
	}
}
