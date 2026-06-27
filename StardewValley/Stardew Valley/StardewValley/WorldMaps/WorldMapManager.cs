using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000117 RID: 279
	public static class WorldMapManager
	{
		// Token: 0x060017A5 RID: 6053 RVA: 0x001115BA File Offset: 0x0010F7BA
		static WorldMapManager()
		{
			WorldMapManager.ReloadData();
		}

		// Token: 0x060017A6 RID: 6054 RVA: 0x001115D8 File Offset: 0x0010F7D8
		public static void ReloadData()
		{
			WorldMapManager.Regions.Clear();
			foreach (KeyValuePair<string, WorldMapRegionData> pair in DataLoader.WorldMap(Game1.content))
			{
				WorldMapManager.Regions.Add(new MapRegion(pair.Key, pair.Value));
			}
			WorldMapManager.NextClearCacheTick = Game1.ticks + WorldMapManager.MaxCacheTicks;
		}

		// Token: 0x060017A7 RID: 6055 RVA: 0x00111660 File Offset: 0x0010F860
		public static IEnumerable<MapRegion> GetMapRegions()
		{
			WorldMapManager.ReloadDataIfStale();
			return WorldMapManager.Regions;
		}

		// Token: 0x060017A8 RID: 6056 RVA: 0x0011166C File Offset: 0x0010F86C
		public static MapAreaPositionWithContext? GetPositionData(GameLocation location, Point tile)
		{
			return WorldMapManager.GetPositionData(location, tile, null);
		}

		// Token: 0x060017A9 RID: 6057 RVA: 0x00111678 File Offset: 0x0010F878
		internal static MapAreaPositionWithContext? GetPositionData(GameLocation location, Point tile, LogBuilder log)
		{
			if (location == null)
			{
				if (log != null)
				{
					log.AppendLine("Skipped: location is null.");
				}
				return null;
			}
			LogBuilder subLog = (log != null) ? log.GetIndentedLog(3) : null;
			if (log != null)
			{
				log.AppendLine("Searching for the player position...");
			}
			MapAreaPosition position = WorldMapManager.GetPositionDataWithoutFallback(location, tile, subLog);
			if (position != null)
			{
				if (log != null)
				{
					log.AppendLine("Found match: position '" + position.Data.Id + "'.");
				}
				return new MapAreaPositionWithContext?(new MapAreaPositionWithContext(position, location, tile));
			}
			Building building = location.ParentBuilding;
			GameLocation buildingLocation = (building != null) ? building.GetParentLocation() : null;
			if (buildingLocation != null)
			{
				if (log != null)
				{
					log.AppendLine("");
				}
				if (log != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Searching for the exterior position of the '");
					defaultInterpolatedStringHandler.AppendFormatted(building.buildingType.Value);
					defaultInterpolatedStringHandler.AppendLiteral("' building in ");
					defaultInterpolatedStringHandler.AppendFormatted(buildingLocation.NameOrUniqueName);
					defaultInterpolatedStringHandler.AppendLiteral("...");
					log.AppendLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				Point buildingTile = new Point(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2);
				position = WorldMapManager.GetPositionDataWithoutFallback(buildingLocation, buildingTile, subLog);
				if (position != null)
				{
					if (log != null)
					{
						log.AppendLine("Found match: position '" + position.Data.Id + "'.");
					}
					return new MapAreaPositionWithContext?(new MapAreaPositionWithContext(position, buildingLocation, buildingTile));
				}
			}
			if (log != null)
			{
				log.AppendLine("");
			}
			if (log != null)
			{
				log.AppendLine("No match found.");
			}
			return null;
		}

		// Token: 0x060017AA RID: 6058 RVA: 0x0011181D File Offset: 0x0010FA1D
		public static MapAreaPosition GetPositionDataWithoutFallback(GameLocation location, Point tile)
		{
			return WorldMapManager.GetPositionDataWithoutFallback(location, tile, null);
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x00111828 File Offset: 0x0010FA28
		internal static MapAreaPosition GetPositionDataWithoutFallback(GameLocation location, Point tile, LogBuilder log)
		{
			if (location == null)
			{
				if (log != null)
				{
					log.AppendLine("Skipped: location is null.");
				}
				return null;
			}
			LogBuilder subLog = (log != null) ? log.GetIndentedLog(3) : null;
			foreach (MapRegion region in WorldMapManager.GetMapRegions())
			{
				if (log != null)
				{
					log.AppendLine("Checking region '" + region.Id + "'...");
				}
				MapAreaPosition position = region.GetPositionData(location, tile, subLog);
				if (position != null)
				{
					return position;
				}
			}
			return null;
		}

		// Token: 0x060017AC RID: 6060 RVA: 0x001118C4 File Offset: 0x0010FAC4
		private static void ReloadDataIfStale()
		{
			if (Game1.ticks >= WorldMapManager.NextClearCacheTick)
			{
				WorldMapManager.ReloadData();
			}
		}

		// Token: 0x04000E43 RID: 3651
		private static int NextClearCacheTick;

		// Token: 0x04000E44 RID: 3652
		private static int MaxCacheTicks = 3600;

		// Token: 0x04000E45 RID: 3653
		private static readonly List<MapRegion> Regions = new List<MapRegion>();
	}
}
