using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Extensions
{
	// Token: 0x0200031B RID: 795
	public static class FrameworkExtensions
	{
		// Token: 0x06003450 RID: 13392 RVA: 0x0029C744 File Offset: 0x0029A944
		public static Microsoft.Xna.Framework.Rectangle GetTitleSafeArea(this Viewport viewport)
		{
			return viewport.Bounds;
		}

		// Token: 0x06003451 RID: 13393 RVA: 0x0029C74D File Offset: 0x0029A94D
		public static IEnumerable<Point> GetPoints(this Microsoft.Xna.Framework.Rectangle rect)
		{
			int right = rect.Right;
			int bottom = rect.Bottom;
			int num;
			for (int y = rect.Y; y < bottom; y = num + 1)
			{
				for (int x = rect.X; x < right; x = num + 1)
				{
					yield return new Point(x, y);
					num = x;
				}
				num = y;
			}
			yield break;
		}

		// Token: 0x06003452 RID: 13394 RVA: 0x0029C75D File Offset: 0x0029A95D
		public static IEnumerable<Vector2> GetVectors(this Microsoft.Xna.Framework.Rectangle rect)
		{
			int right = rect.Right;
			int bottom = rect.Bottom;
			int num;
			for (int y = rect.Y; y < bottom; y = num + 1)
			{
				for (int x = rect.X; x < right; x = num + 1)
				{
					yield return new Vector2((float)x, (float)y);
					num = x;
				}
				num = y;
			}
			yield break;
		}

		// Token: 0x06003453 RID: 13395 RVA: 0x0029C76D File Offset: 0x0029A96D
		public static Microsoft.Xna.Framework.Rectangle Clone(this Microsoft.Xna.Framework.Rectangle rect)
		{
			return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		// Token: 0x06003454 RID: 13396 RVA: 0x0029C78C File Offset: 0x0029A98C
		public static Vector2 Size(this Viewport vp)
		{
			return new Vector2((float)vp.Width, (float)vp.Height);
		}

		// Token: 0x06003455 RID: 13397 RVA: 0x0029C7A3 File Offset: 0x0029A9A3
		public static int GetElementCount(this Texture2D texture)
		{
			return texture.ActualWidth * texture.ActualHeight;
		}

		// Token: 0x06003456 RID: 13398 RVA: 0x0029C7B2 File Offset: 0x0029A9B2
		public static int GetActualWidth(this Texture2D texture)
		{
			return texture.ActualWidth;
		}

		// Token: 0x06003457 RID: 13399 RVA: 0x0029C7BA File Offset: 0x0029A9BA
		public static int GetActualHeight(this Texture2D texture)
		{
			return texture.ActualHeight;
		}

		// Token: 0x06003458 RID: 13400 RVA: 0x0029C7C2 File Offset: 0x0029A9C2
		public static void SetContentSize(this Texture2D texture, int width, int height)
		{
			texture.SetImageSize(width, height);
		}

		// Token: 0x06003459 RID: 13401 RVA: 0x0029C7CC File Offset: 0x0029A9CC
		public static bool TryGetValue(this IPropertyCollection properties, string key, out string value)
		{
			PropertyValue propertyValue;
			if (!properties.TryGetValue(key, out propertyValue))
			{
				value = null;
				return false;
			}
			value = propertyValue;
			return true;
		}

		// Token: 0x0600345A RID: 13402 RVA: 0x0029C7F2 File Offset: 0x0029A9F2
		public static bool TryAdd(this IPropertyCollection properties, string key, string value)
		{
			if (properties.ContainsKey(key))
			{
				return false;
			}
			properties.Add(key, new PropertyValue(value));
			return true;
		}

		// Token: 0x0600345B RID: 13403 RVA: 0x0029C810 File Offset: 0x0029AA10
		public static Layer RequireLayer(this Map map, string layerId)
		{
			Layer layer = map.GetLayer(layerId);
			if (layer == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The '");
				defaultInterpolatedStringHandler.AppendFormatted(map.assetPath);
				defaultInterpolatedStringHandler.AppendLiteral("' map doesn't have required layer '");
				defaultInterpolatedStringHandler.AppendFormatted(layerId);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return layer;
		}

		// Token: 0x0600345C RID: 13404 RVA: 0x0029C878 File Offset: 0x0029AA78
		public static TileSheet RequireTileSheet(this Map map, string tilesheetId)
		{
			TileSheet tileSheet = map.GetTileSheet(tilesheetId);
			if (tileSheet == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The '");
				defaultInterpolatedStringHandler.AppendFormatted(map.assetPath);
				defaultInterpolatedStringHandler.AppendLiteral("' map doesn't have required tile sheet '");
				defaultInterpolatedStringHandler.AppendFormatted(tilesheetId);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return tileSheet;
		}

		// Token: 0x0600345D RID: 13405 RVA: 0x0029C8E0 File Offset: 0x0029AAE0
		public static TileSheet RequireTileSheet(this Map map, int expectedIndex, string tilesheetId)
		{
			if (map.TileSheets.Count > expectedIndex)
			{
				TileSheet tilesheet = map.TileSheets[expectedIndex];
				if (tilesheet.Id == tilesheetId)
				{
					return tilesheet;
				}
			}
			TileSheet tileSheet = map.GetTileSheet(tilesheetId);
			if (tileSheet == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The '");
				defaultInterpolatedStringHandler.AppendFormatted(map.assetPath);
				defaultInterpolatedStringHandler.AppendLiteral("' map doesn't have required tile sheet '");
				defaultInterpolatedStringHandler.AppendFormatted(tilesheetId);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return tileSheet;
		}

		// Token: 0x0600345E RID: 13406 RVA: 0x0029C974 File Offset: 0x0029AB74
		public static bool HasTileAt(this Map map, Location tile, string layerId, string tilesheetId = null)
		{
			bool? flag;
			if (map == null)
			{
				flag = null;
			}
			else
			{
				Layer layer = map.GetLayer(layerId);
				flag = ((layer != null) ? new bool?(layer.HasTileAt(tile.X, tile.Y, tilesheetId)) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		// Token: 0x0600345F RID: 13407 RVA: 0x0029C9C4 File Offset: 0x0029ABC4
		public static bool HasTileAt(this Map map, int x, int y, string layerId, string tilesheetId = null)
		{
			bool? flag;
			if (map == null)
			{
				flag = null;
			}
			else
			{
				Layer layer = map.GetLayer(layerId);
				flag = ((layer != null) ? new bool?(layer.HasTileAt(x, y, tilesheetId)) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		// Token: 0x06003460 RID: 13408 RVA: 0x0029CA0C File Offset: 0x0029AC0C
		public static int GetTileIndexAt(this Map map, int x, int y, string layerId, string tilesheetId = null)
		{
			int? num;
			if (map == null)
			{
				num = null;
			}
			else
			{
				Layer layer = map.GetLayer(layerId);
				num = ((layer != null) ? new int?(layer.GetTileIndexAt(x, y, tilesheetId)) : null);
			}
			int? num2 = num;
			return num2.GetValueOrDefault(-1);
		}

		// Token: 0x06003461 RID: 13409 RVA: 0x0029CA54 File Offset: 0x0029AC54
		public static int GetTileIndexAt(this Map map, Location tile, string layerId, string tilesheetId = null)
		{
			int? num;
			if (map == null)
			{
				num = null;
			}
			else
			{
				Layer layer = map.GetLayer(layerId);
				num = ((layer != null) ? new int?(layer.GetTileIndexAt(tile.X, tile.Y, tilesheetId)) : null);
			}
			int? num2 = num;
			return num2.GetValueOrDefault(-1);
		}

		// Token: 0x06003462 RID: 13410 RVA: 0x0029CAA5 File Offset: 0x0029ACA5
		public static bool HasTileAt(this Layer layer, Location tile, string tilesheetId = null)
		{
			return layer.HasTileAt(tile.X, tile.Y, tilesheetId);
		}

		// Token: 0x06003463 RID: 13411 RVA: 0x0029CABA File Offset: 0x0029ACBA
		public static bool HasTileAt(this Layer layer, int x, int y, string tilesheetId = null)
		{
			return layer.GetTileIndexAt(x, y, tilesheetId) != -1;
		}

		// Token: 0x06003464 RID: 13412 RVA: 0x0029CACB File Offset: 0x0029ACCB
		public static int GetTileIndexAt(this Layer layer, Location tile, string tilesheetId = null)
		{
			if (layer == null)
			{
				return -1;
			}
			return layer.GetTileIndexAt(tile.X, tile.Y, tilesheetId);
		}

		// Token: 0x06003465 RID: 13413 RVA: 0x0029CAE8 File Offset: 0x0029ACE8
		public static int GetTileIndexAt(this Layer layer, int x, int y, string tilesheetId = null)
		{
			Tile tile = (layer != null) ? layer.Tiles[x, y] : null;
			if (tile == null)
			{
				return -1;
			}
			if (tilesheetId != null)
			{
				TileSheet tileSheet = tile.TileSheet;
				if (!((tileSheet != null) ? tileSheet.Id : null).EqualsIgnoreCase(tilesheetId))
				{
					return -1;
				}
			}
			return tile.TileIndex;
		}

		// Token: 0x06003466 RID: 13414 RVA: 0x0029CB33 File Offset: 0x0029AD33
		public static Microsoft.Xna.Framework.Rectangle ToXna(this xTile.Dimensions.Rectangle xrect)
		{
			return new Microsoft.Xna.Framework.Rectangle(xrect.X, xrect.Y, xrect.Width, xrect.Height);
		}
	}
}
