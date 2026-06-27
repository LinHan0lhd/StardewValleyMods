using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000112 RID: 274
	public class MapAreaPosition
	{
		// Token: 0x1700029D RID: 669
		// (get) Token: 0x0600177E RID: 6014 RVA: 0x001109B6 File Offset: 0x0010EBB6
		public MapRegion Region { get; }

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x0600177F RID: 6015 RVA: 0x001109BE File Offset: 0x0010EBBE
		public MapArea Area { get; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06001780 RID: 6016 RVA: 0x001109C6 File Offset: 0x0010EBC6
		public WorldMapAreaPositionData Data { get; }

		// Token: 0x06001781 RID: 6017 RVA: 0x001109CE File Offset: 0x0010EBCE
		public MapAreaPosition(MapArea mapArea, WorldMapAreaPositionData data)
		{
			this.Region = mapArea.Region;
			this.Area = mapArea;
			this.Data = data;
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x001109F0 File Offset: 0x0010EBF0
		public bool Matches(string locationName, string contextName, Point tile)
		{
			return this.Matches(locationName, contextName, tile, null);
		}

		// Token: 0x06001783 RID: 6019 RVA: 0x001109FC File Offset: 0x0010EBFC
		internal bool Matches(string locationName, string contextName, Point tile, LogBuilder log)
		{
			WorldMapAreaPositionData data = this.Data;
			if (data.LocationContext != null && data.LocationContext != contextName)
			{
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Skipped: location context '");
					defaultInterpolatedStringHandler.AppendFormatted(contextName);
					defaultInterpolatedStringHandler.AppendLiteral("' doesn't match required context '");
					defaultInterpolatedStringHandler.AppendFormatted(data.LocationContext);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.AppendLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return false;
			}
			if (data.LocationName != null && data.LocationName != locationName)
			{
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Skipped: location '");
					defaultInterpolatedStringHandler.AppendFormatted(locationName);
					defaultInterpolatedStringHandler.AppendLiteral("' doesn't match required location '");
					defaultInterpolatedStringHandler.AppendFormatted(data.LocationName);
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.AppendLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return false;
			}
			List<string> locationNames = data.LocationNames;
			if (locationNames != null && locationNames.Count > 0 && !data.LocationNames.Contains(locationName))
			{
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Skipped: location '");
					defaultInterpolatedStringHandler.AppendFormatted(locationName);
					defaultInterpolatedStringHandler.AppendLiteral("' doesn't match one of the required locations '");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("', '", data.LocationNames));
					defaultInterpolatedStringHandler.AppendLiteral("'.");
					log.AppendLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return false;
			}
			if (!this.IsTileWithinZone(tile))
			{
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Skipped: tile position ");
					defaultInterpolatedStringHandler.AppendFormatted<Point>(tile);
					defaultInterpolatedStringHandler.AppendLiteral(" doesn't match required tile zone ");
					defaultInterpolatedStringHandler.AppendFormatted<Microsoft.Xna.Framework.Rectangle>(this.Data.ExtendedTileArea ?? this.Data.TileArea);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log.AppendLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return false;
			}
			if (log != null)
			{
				log.AppendLine("Matched successfully.");
			}
			return true;
		}

		// Token: 0x06001784 RID: 6020 RVA: 0x00110C04 File Offset: 0x0010EE04
		public Microsoft.Xna.Framework.Rectangle GetPixelArea()
		{
			Microsoft.Xna.Framework.Rectangle? cachedMapPixelArea = this.CachedMapPixelArea;
			if (cachedMapPixelArea == null)
			{
				Microsoft.Xna.Framework.Rectangle rawArea = this.Data.MapPixelArea;
				if (rawArea.IsEmpty)
				{
					rawArea = this.Area.Data.PixelArea;
				}
				Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(rawArea.X * 4, rawArea.Y * 4, rawArea.Width * 4, rawArea.Height * 4);
				this.CachedMapPixelArea = new Microsoft.Xna.Framework.Rectangle?(area);
				this.IsFixedMapPosition = (rawArea.Width <= 1 && rawArea.Height <= 1);
			}
			return this.CachedMapPixelArea.Value;
		}

		// Token: 0x06001785 RID: 6021 RVA: 0x00110CA4 File Offset: 0x0010EEA4
		public Vector2 GetMapPixelPosition(GameLocation location, Point tileLocation)
		{
			Microsoft.Xna.Framework.Rectangle mapPixelArea = this.GetPixelArea();
			if (this.IsFixedMapPosition)
			{
				return new Vector2((float)mapPixelArea.X, (float)mapPixelArea.Y);
			}
			Vector2? positionRatio = this.GetPositionRatioIfValid(location, tileLocation);
			if (positionRatio != null)
			{
				return new Vector2(Utility.Lerp((float)mapPixelArea.Left, (float)mapPixelArea.Right, positionRatio.Value.X), Utility.Lerp((float)mapPixelArea.Top, (float)mapPixelArea.Bottom, positionRatio.Value.Y));
			}
			Point center = mapPixelArea.Center;
			return new Vector2((float)center.X, (float)center.Y);
		}

		// Token: 0x06001786 RID: 6022 RVA: 0x00110D4C File Offset: 0x0010EF4C
		public string GetScrollText(Point playerTile)
		{
			if (this.CachedScrollText == null)
			{
				string scrollText = this.Data.ScrollText;
				List<WorldMapAreaPositionScrollTextZoneData> scrollTextZones = this.Data.ScrollTextZones;
				if (scrollTextZones != null && scrollTextZones.Count > 0)
				{
					foreach (WorldMapAreaPositionScrollTextZoneData zone in this.Data.ScrollTextZones)
					{
						if (zone.TileArea.Contains(playerTile))
						{
							scrollText = zone.ScrollText;
							break;
						}
					}
				}
				this.CachedScrollText = ((scrollText != null) ? TokenParser.ParseText(Utility.TrimLines(scrollText), null, null, null) : this.Area.GetScrollText());
			}
			return this.CachedScrollText;
		}

		// Token: 0x06001787 RID: 6023 RVA: 0x00110E14 File Offset: 0x0010F014
		public virtual Vector2? GetPositionRatioIfValid(GameLocation location, Point tile)
		{
			if (((location != null) ? location.map : null) == null || !this.IsTileWithinZone(tile))
			{
				return null;
			}
			Size layerSize = location.map.Layers[0].LayerSize;
			Microsoft.Xna.Framework.Rectangle tileArea = this.Data.TileArea;
			if (tileArea.IsEmpty || tileArea.Right > layerSize.Width || tileArea.Bottom > layerSize.Height)
			{
				tileArea = (tileArea.IsEmpty ? new Microsoft.Xna.Framework.Rectangle(0, 0, layerSize.Width, layerSize.Height) : new Microsoft.Xna.Framework.Rectangle(tileArea.X, tileArea.Y, Math.Min(tileArea.Width, layerSize.Width - tileArea.X), Math.Min(tileArea.Height, layerSize.Height - tileArea.Y)));
			}
			float num = (float)MathHelper.Clamp(tile.X, tileArea.X, tileArea.Right - 1);
			float y = (float)MathHelper.Clamp(tile.Y, tileArea.Y, tileArea.Bottom - 1);
			return new Vector2?(new Vector2((num - (float)tileArea.X) / (float)tileArea.Width, (y - (float)tileArea.Y) / (float)tileArea.Height));
		}

		// Token: 0x06001788 RID: 6024 RVA: 0x00110F50 File Offset: 0x0010F150
		public virtual bool IsTileWithinZone(Point tile)
		{
			Microsoft.Xna.Framework.Rectangle tileArea = this.Data.ExtendedTileArea ?? this.Data.TileArea;
			return tileArea.IsEmpty || tileArea.Contains(tile);
		}

		// Token: 0x04000E2D RID: 3629
		protected Microsoft.Xna.Framework.Rectangle? CachedMapPixelArea;

		// Token: 0x04000E2E RID: 3630
		protected string CachedScrollText;

		// Token: 0x04000E2F RID: 3631
		protected bool IsFixedMapPosition;
	}
}
