using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;
using StardewValley.Locations;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000116 RID: 278
	public class MapRegion
	{
		// Token: 0x170002AA RID: 682
		// (get) Token: 0x0600179B RID: 6043 RVA: 0x00111152 File Offset: 0x0010F352
		public string Id { get; }

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x0600179C RID: 6044 RVA: 0x0011115A File Offset: 0x0010F35A
		public WorldMapRegionData Data { get; }

		// Token: 0x0600179D RID: 6045 RVA: 0x00111162 File Offset: 0x0010F362
		public MapRegion(string id, WorldMapRegionData data)
		{
			this.Id = id;
			this.Data = data;
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x00111178 File Offset: 0x0010F378
		public Rectangle GetMapPixelBounds()
		{
			Rectangle? cachedPixelBounds = this.CachedPixelBounds;
			if (cachedPixelBounds == null)
			{
				MapAreaTexture baseTexture = this.GetBaseTexture();
				MapArea[] mapAreas = this.GetAreas();
				int maxWidth = (baseTexture != null) ? baseTexture.MapPixelArea.Width : 0;
				int maxHeight = (baseTexture != null) ? baseTexture.MapPixelArea.Height : 0;
				MapArea[] array = mapAreas;
				for (int i = 0; i < array.Length; i++)
				{
					foreach (MapAreaTexture overlay in array[i].GetTextures())
					{
						maxWidth = Math.Max(maxWidth, overlay.MapPixelArea.Width);
						maxHeight = Math.Max(maxHeight, overlay.MapPixelArea.Height);
					}
				}
				Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(maxWidth, maxHeight, 0, 0);
				this.CachedPixelBounds = new Rectangle?(new Rectangle((int)topLeft.X, (int)topLeft.Y, maxWidth / 4, maxHeight / 4));
			}
			return this.CachedPixelBounds.Value;
		}

		// Token: 0x0600179F RID: 6047 RVA: 0x00111268 File Offset: 0x0010F468
		public MapAreaTexture GetBaseTexture()
		{
			if (this.CachedBaseTexture == null)
			{
				if (this.Data.BaseTexture.Count > 0)
				{
					foreach (WorldMapTextureData entry in this.Data.BaseTexture)
					{
						if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, null, null))
						{
							Texture2D texture = this.GetTexture(entry.Texture);
							Rectangle sourceRect = entry.SourceRect;
							if (sourceRect.IsEmpty)
							{
								sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
							}
							Rectangle mapPixelArea = entry.MapPixelArea;
							if (mapPixelArea.IsEmpty)
							{
								mapPixelArea = sourceRect;
							}
							mapPixelArea = new Rectangle(mapPixelArea.X * 4, mapPixelArea.Y * 4, mapPixelArea.Width * 4, mapPixelArea.Height * 4);
							this.CachedBaseTexture = new MapAreaTexture(texture, sourceRect, mapPixelArea);
							break;
						}
					}
				}
				if (this.CachedBaseTexture == null)
				{
					this.CachedBaseTexture = new MapAreaTexture(null, Rectangle.Empty, Rectangle.Empty);
				}
			}
			if (this.CachedBaseTexture.Texture == null)
			{
				return null;
			}
			return this.CachedBaseTexture;
		}

		// Token: 0x060017A0 RID: 6048 RVA: 0x001113AC File Offset: 0x0010F5AC
		public MapArea[] GetAreas()
		{
			if (this.CachedMapAreas == null)
			{
				List<MapArea> areas = new List<MapArea>();
				foreach (WorldMapAreaData area in this.Data.MapAreas)
				{
					if (GameStateQuery.CheckConditions(area.Condition, null, null, null, null, null, null))
					{
						areas.Add(new MapArea(this, area));
					}
				}
				this.CachedMapAreas = areas.ToArray();
			}
			return this.CachedMapAreas;
		}

		// Token: 0x060017A1 RID: 6049 RVA: 0x00111440 File Offset: 0x0010F640
		public MapAreaPosition GetPositionData(GameLocation location, Point tile)
		{
			return this.GetPositionData(location, tile, null);
		}

		// Token: 0x060017A2 RID: 6050 RVA: 0x0011144C File Offset: 0x0010F64C
		internal MapAreaPosition GetPositionData(GameLocation location, Point tile, LogBuilder log)
		{
			if (location == null)
			{
				if (log != null)
				{
					log.AppendLine("Skipped: location is null.");
				}
				return null;
			}
			string locationName = this.GetLocationName(location);
			string contextId = location.GetLocationContextId();
			LogBuilder subLog = (log != null) ? log.GetIndentedLog(3) : null;
			foreach (MapArea mapArea in this.GetAreas())
			{
				if (log != null)
				{
					log.AppendLine("Checking map area '" + mapArea.Id + "'...");
				}
				MapAreaPosition position = mapArea.GetWorldPosition(locationName, contextId, tile, subLog);
				if (position != null)
				{
					return position;
				}
			}
			return null;
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x001114DC File Offset: 0x0010F6DC
		protected string GetLocationName(GameLocation location)
		{
			string locationName = (location.IsTemporary && !string.IsNullOrEmpty(location.Map.Id)) ? location.Map.Id : location.Name;
			if (locationName == "Mine")
			{
				return "Mines";
			}
			MineShaft shaft = location as MineShaft;
			if (shaft != null)
			{
				if (shaft.mineLevel <= 120 || shaft.mineLevel == 77377)
				{
					return "Mines";
				}
				return "SkullCave";
			}
			else
			{
				if (VolcanoDungeon.IsGeneratedLevel(location.Name))
				{
					return "VolcanoDungeon";
				}
				return locationName;
			}
		}

		// Token: 0x060017A4 RID: 6052 RVA: 0x0011156C File Offset: 0x0010F76C
		private Texture2D GetTexture(string assetName)
		{
			if (Game1.season != Season.Spring)
			{
				string seasonalName = assetName + "_" + Game1.currentSeason.ToLower();
				if (Game1.content.DoesAssetExist<Texture2D>(seasonalName))
				{
					return Game1.content.Load<Texture2D>(seasonalName);
				}
			}
			return Game1.content.Load<Texture2D>(assetName);
		}

		// Token: 0x04000E3E RID: 3646
		protected Rectangle? CachedPixelBounds;

		// Token: 0x04000E3F RID: 3647
		protected MapArea[] CachedMapAreas;

		// Token: 0x04000E40 RID: 3648
		protected MapAreaTexture CachedBaseTexture;
	}
}
