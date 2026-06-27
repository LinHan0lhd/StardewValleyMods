using System;

namespace StardewValley
{
	// Token: 0x020000DB RID: 219
	public class WaterTiles
	{
		// Token: 0x06001098 RID: 4248 RVA: 0x000C7368 File Offset: 0x000C5568
		public WaterTiles(bool[,] source)
		{
			int width = source.GetLength(0);
			int height = source.GetLength(1);
			this.waterTiles = new WaterTiles.WaterTileData[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					this.waterTiles[x, y] = new WaterTiles.WaterTileData(source[x, y], true);
				}
			}
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x000C73CC File Offset: 0x000C55CC
		public WaterTiles(int width, int height)
		{
			this.waterTiles = new WaterTiles.WaterTileData[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					this.waterTiles[x, y] = new WaterTiles.WaterTileData(false, true);
				}
			}
		}

		// Token: 0x170001E5 RID: 485
		public bool this[int x, int y]
		{
			get
			{
				return this.waterTiles[x, y].isWater;
			}
			set
			{
				this.waterTiles[x, y] = new WaterTiles.WaterTileData(value, true);
			}
		}

		// Token: 0x04000A0D RID: 2573
		public WaterTiles.WaterTileData[,] waterTiles;

		// Token: 0x020004A8 RID: 1192
		public struct WaterTileData
		{
			// Token: 0x06003EDE RID: 16094 RVA: 0x002FB597 File Offset: 0x002F9797
			public WaterTileData(bool is_water, bool is_visible)
			{
				this.isWater = is_water;
				this.isVisible = is_visible;
			}

			// Token: 0x040028EB RID: 10475
			public bool isWater;

			// Token: 0x040028EC RID: 10476
			public bool isVisible;
		}
	}
}
