using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;
using StardewValley.TokenizableStrings;

namespace StardewValley.WorldMaps
{
	// Token: 0x02000111 RID: 273
	public class MapArea
	{
		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06001773 RID: 6003 RVA: 0x00110535 File Offset: 0x0010E735
		public string Id { get; }

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06001774 RID: 6004 RVA: 0x0011053D File Offset: 0x0010E73D
		public MapRegion Region { get; }

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06001775 RID: 6005 RVA: 0x00110545 File Offset: 0x0010E745
		public WorldMapAreaData Data { get; }

		// Token: 0x06001776 RID: 6006 RVA: 0x0011054D File Offset: 0x0010E74D
		public MapArea(MapRegion region, WorldMapAreaData data)
		{
			this.Data = data;
			this.Id = data.Id;
			this.Region = region;
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x00110570 File Offset: 0x0010E770
		public MapAreaTexture[] GetTextures()
		{
			if (this.CachedTextures == null)
			{
				if (this.Data.Textures.Count > 0)
				{
					List<MapAreaTexture> textures = new List<MapAreaTexture>();
					foreach (WorldMapTextureData entry in this.Data.Textures)
					{
						if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, null, null))
						{
							Texture2D texture;
							if (entry.Condition == "IS_CUSTOM_FARM_TYPE")
							{
								ModFarmType whichModFarm = Game1.whichModFarm;
								string textureName = (whichModFarm != null) ? whichModFarm.WorldMapTexture : null;
								if (textureName == null)
								{
									continue;
								}
								texture = this.GetTexture(textureName);
								if (texture.Width <= 200)
								{
									entry.SourceRect = texture.Bounds;
								}
							}
							else
							{
								texture = this.GetTexture(entry.Texture);
							}
							Rectangle sourceRect = entry.SourceRect;
							if (sourceRect.IsEmpty)
							{
								sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
							}
							Rectangle mapPixelArea = entry.MapPixelArea;
							if (mapPixelArea.IsEmpty)
							{
								mapPixelArea = this.Data.PixelArea;
							}
							mapPixelArea = new Rectangle(mapPixelArea.X * 4, mapPixelArea.Y * 4, mapPixelArea.Width * 4, mapPixelArea.Height * 4);
							textures.Add(new MapAreaTexture(texture, sourceRect, mapPixelArea));
						}
					}
					this.CachedTextures = textures.ToArray();
				}
				else
				{
					this.CachedTextures = LegacyShims.EmptyArray<MapAreaTexture>();
				}
			}
			return this.CachedTextures;
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x00110710 File Offset: 0x0010E910
		public MapAreaTooltip[] GetTooltips()
		{
			if (this.CachedTooltips == null)
			{
				List<WorldMapTooltipData> tooltips2 = this.Data.Tooltips;
				if (tooltips2 != null && tooltips2.Count > 0)
				{
					List<MapAreaTooltip> tooltips = new List<MapAreaTooltip>();
					foreach (WorldMapTooltipData entry in this.Data.Tooltips)
					{
						if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, null, null))
						{
							string text = GameStateQuery.CheckConditions(entry.KnownCondition, null, null, null, null, null, null) ? TokenParser.ParseText(Utility.TrimLines(entry.Text), null, null, null) : "???";
							if (!string.IsNullOrWhiteSpace(text))
							{
								tooltips.Add(new MapAreaTooltip(this, entry, text));
							}
						}
					}
					this.CachedTooltips = tooltips.ToArray();
				}
				else
				{
					this.CachedTooltips = LegacyShims.EmptyArray<MapAreaTooltip>();
				}
			}
			return this.CachedTooltips;
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x00110808 File Offset: 0x0010EA08
		public IEnumerable<MapAreaPosition> GetWorldPositions()
		{
			if (this.CachedWorldPositions == null)
			{
				List<MapAreaPosition> positions = new List<MapAreaPosition>();
				foreach (WorldMapAreaPositionData entry in this.Data.WorldPositions)
				{
					if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, null, null))
					{
						positions.Add(new MapAreaPosition(this, entry));
					}
				}
				this.CachedWorldPositions = positions.ToArray();
			}
			return this.CachedWorldPositions;
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x0011089C File Offset: 0x0010EA9C
		public MapAreaPosition GetWorldPosition(string locationName, string contextName, Point tile)
		{
			return this.GetWorldPosition(locationName, contextName, tile, null);
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x001108A8 File Offset: 0x0010EAA8
		internal MapAreaPosition GetWorldPosition(string locationName, string contextName, Point tile, LogBuilder log)
		{
			LogBuilder subLog = (log != null) ? log.GetIndentedLog(3) : null;
			foreach (MapAreaPosition position in this.GetWorldPositions())
			{
				if (log != null)
				{
					log.AppendLine("Checking position '" + position.Data.Id + "'...");
				}
				if (position.Matches(locationName, contextName, tile, subLog))
				{
					return position;
				}
			}
			return null;
		}

		// Token: 0x0600177C RID: 6012 RVA: 0x00110938 File Offset: 0x0010EB38
		public virtual string GetScrollText()
		{
			if (this.CachedScrollText == null)
			{
				this.CachedScrollText = TokenParser.ParseText(Utility.TrimLines(this.Data.ScrollText), null, null, null);
			}
			return this.CachedScrollText;
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x00110968 File Offset: 0x0010EB68
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

		// Token: 0x04000E26 RID: 3622
		protected MapAreaTexture[] CachedTextures;

		// Token: 0x04000E27 RID: 3623
		protected MapAreaTooltip[] CachedTooltips;

		// Token: 0x04000E28 RID: 3624
		protected MapAreaPosition[] CachedWorldPositions;

		// Token: 0x04000E29 RID: 3625
		protected string CachedScrollText;
	}
}
