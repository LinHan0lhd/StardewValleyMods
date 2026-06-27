using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Logging;
using StardewValley.WorldMaps;

namespace StardewValley.Menus
{
	// Token: 0x02000287 RID: 647
	public class MapPage : IClickableMenu
	{
		// Token: 0x06002AE0 RID: 10976 RVA: 0x00205430 File Offset: 0x00203630
		public MapPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			WorldMapManager.ReloadData();
			Point playerTile = this.GetNormalizedPlayerTile(Game1.player);
			MapAreaPositionWithContext? positionData = WorldMapManager.GetPositionData(Game1.player.currentLocation, playerTile);
			this.mapPosition = ((positionData != null) ? positionData : WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero));
			this.mapRegion = (((this.mapPosition != null) ? this.mapPosition.GetValueOrDefault().Data.Region : null) ?? WorldMapManager.GetMapRegions().First<MapRegion>());
			this.mapAreas = this.mapRegion.GetAreas();
			this.scrollText = ((this.mapPosition != null) ? this.mapPosition.GetValueOrDefault().Data.GetScrollText(playerTile) : null);
			this.mapBounds = this.mapRegion.GetMapPixelBounds();
			int id = this.defaultComponentID = 1000;
			MapArea[] array = this.mapAreas;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MapAreaTooltip tooltip in array[i].GetTooltips())
				{
					Rectangle pixelArea = tooltip.GetPixelArea();
					pixelArea = new Rectangle(this.mapBounds.X + pixelArea.X, this.mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
					id++;
					ClickableComponent component = new ClickableComponent(pixelArea, tooltip.NamespacedId)
					{
						myID = id,
						label = tooltip.Text
					};
					this.points[tooltip.NamespacedId] = component;
					if (tooltip.NamespacedId == "Farm/Default")
					{
						this.defaultComponentID = id;
					}
				}
			}
			array = this.mapAreas;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MapAreaTooltip tooltip2 in array[i].GetTooltips())
				{
					ClickableComponent component2;
					if (this.points.TryGetValue(tooltip2.NamespacedId, out component2))
					{
						this.SetNeighborId(component2, "left", tooltip2.Data.LeftNeighbor);
						this.SetNeighborId(component2, "right", tooltip2.Data.RightNeighbor);
						this.SetNeighborId(component2, "up", tooltip2.Data.UpNeighbor);
						this.SetNeighborId(component2, "down", tooltip2.Data.DownNeighbor);
					}
				}
			}
		}

		// Token: 0x06002AE1 RID: 10977 RVA: 0x002056EB File Offset: 0x002038EB
		public override void populateClickableComponentList()
		{
			base.populateClickableComponentList();
			this.allClickableComponents.AddRange(this.points.Values);
		}

		// Token: 0x06002AE2 RID: 10978 RVA: 0x0020570C File Offset: 0x0020390C
		public void SetNeighborId(ClickableComponent component, string direction, string neighborKeys)
		{
			if (string.IsNullOrWhiteSpace(neighborKeys))
			{
				return;
			}
			int neighborId;
			bool foundIgnore;
			if (!this.TryGetNeighborId(neighborKeys, out neighborId, out foundIgnore, false))
			{
				if (!foundIgnore)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 3);
					defaultInterpolatedStringHandler.AppendLiteral("World map tooltip '");
					defaultInterpolatedStringHandler.AppendFormatted(component.name);
					defaultInterpolatedStringHandler.AppendLiteral("' has ");
					defaultInterpolatedStringHandler.AppendFormatted(direction);
					defaultInterpolatedStringHandler.AppendLiteral(" neighbor keys '");
					defaultInterpolatedStringHandler.AppendFormatted(neighborKeys);
					defaultInterpolatedStringHandler.AppendLiteral("' which don't match a tooltip namespaced ID or alias.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return;
			}
			if (direction == "left")
			{
				component.leftNeighborID = neighborId;
				return;
			}
			if (direction == "right")
			{
				component.rightNeighborID = neighborId;
				return;
			}
			if (direction == "up")
			{
				component.upNeighborID = neighborId;
				return;
			}
			if (!(direction == "down"))
			{
				Game1.log.Warn("Can't set neighbor ID for unknown direction '" + direction + "'.");
				return;
			}
			component.downNeighborID = neighborId;
		}

		// Token: 0x06002AE3 RID: 10979 RVA: 0x0020580C File Offset: 0x00203A0C
		public bool TryGetNeighborId(string keys, out int id, out bool foundIgnore, bool isAlias = false)
		{
			foundIgnore = false;
			if (!string.IsNullOrWhiteSpace(keys))
			{
				string[] array = keys.Split(',', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string key = array[i].Trim();
					if (key.EqualsIgnoreCase("ignore"))
					{
						foundIgnore = true;
					}
					else
					{
						ClickableComponent neighbor;
						if (this.points.TryGetValue(key, out neighbor))
						{
							id = neighbor.myID;
							return true;
						}
						string alias;
						if (!isAlias && this.mapRegion.Data.MapNeighborIdAliases.TryGetValue(key, out alias))
						{
							bool localFoundIgnore;
							if (this.TryGetNeighborId(alias, out id, out localFoundIgnore, true))
							{
								foundIgnore = (foundIgnore || localFoundIgnore);
								return true;
							}
							foundIgnore = (foundIgnore || localFoundIgnore);
						}
					}
				}
			}
			id = -1;
			return false;
		}

		// Token: 0x06002AE4 RID: 10980 RVA: 0x002058B4 File Offset: 0x00203AB4
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(this.defaultComponentID);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x002058D0 File Offset: 0x00203AD0
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in this.points.Values)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (name == "Beach/LonelyStone")
					{
						Game1.playSound("stoneCrack", null);
						return;
					}
					if (!(name == "Forest/SewerPipe"))
					{
						return;
					}
					Game1.playSound("shadowpeep", null);
					return;
				}
			}
			GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
			if (gameMenu != null)
			{
				gameMenu.changeTab(gameMenu.lastOpenedNonMapTab, true);
			}
		}

		// Token: 0x06002AE6 RID: 10982 RVA: 0x00205998 File Offset: 0x00203B98
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			foreach (ClickableComponent c in this.points.Values)
			{
				if (c.containsPoint(x, y))
				{
					this.hoverText = c.label;
					break;
				}
			}
		}

		// Token: 0x06002AE7 RID: 10983 RVA: 0x00205A0C File Offset: 0x00203C0C
		public override void draw(SpriteBatch b)
		{
			this.drawMap(b, true, 1f);
			this.drawMiniPortraits(b, 1f);
			this.drawScroll(b);
			this.drawTooltip(b);
		}

		// Token: 0x06002AE8 RID: 10984 RVA: 0x00205A35 File Offset: 0x00203C35
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.mapButton, key) && this.readyToClose())
			{
				base.exitThisMenu(true);
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002AE9 RID: 10985 RVA: 0x00205A64 File Offset: 0x00203C64
		public virtual void drawMiniPortraits(SpriteBatch b, float alpha = 1f)
		{
			Dictionary<Vector2, int> usedPositions = new Dictionary<Vector2, int>();
			foreach (Farmer player in Game1.getOnlineFarmers())
			{
				Point tile = this.GetNormalizedPlayerTile(player);
				MapAreaPositionWithContext? positionData = player.IsLocalPlayer ? this.mapPosition : WorldMapManager.GetPositionData(player.currentLocation, tile);
				if (positionData != null && !(positionData.Value.Data.Region.Id != this.mapRegion.Id))
				{
					Vector2 pos = positionData.Value.GetMapPixelPosition();
					pos = new Vector2(pos.X + (float)this.mapBounds.X - 32f, pos.Y + (float)this.mapBounds.Y - 32f);
					int count;
					usedPositions.TryGetValue(pos, out count);
					usedPositions[pos] = count + 1;
					if (count > 0)
					{
						pos += new Vector2((float)(48 * (count % 2)), (float)(48 * (count / 2)));
					}
					player.FarmerRenderer.drawMiniPortrat(b, pos, 0.00011f, 4f, 2, player, alpha);
				}
			}
		}

		// Token: 0x06002AEA RID: 10986 RVA: 0x00205BCC File Offset: 0x00203DCC
		public virtual void drawScroll(SpriteBatch b)
		{
			if (this.scrollText != null)
			{
				float scrollDrawY = (float)(this.yPositionOnScreen + this.height + 32 + 4);
				float scrollDrawBottom = scrollDrawY + 80f;
				if (scrollDrawBottom > (float)Game1.uiViewport.Height)
				{
					scrollDrawY -= scrollDrawBottom - (float)Game1.uiViewport.Height;
				}
				SpriteText.drawStringWithScrollCenteredAt(b, this.scrollText, this.xPositionOnScreen + this.width / 2, (int)scrollDrawY, "", 1f, null, 0, 0.88f, false);
			}
		}

		// Token: 0x06002AEB RID: 10987 RVA: 0x00205C54 File Offset: 0x00203E54
		public virtual void drawMap(SpriteBatch b, bool drawBorders = true, float alpha = 1f)
		{
			if (drawBorders)
			{
				int boxY = this.mapBounds.Y - 96;
				Game1.drawDialogueBox(this.mapBounds.X - 32, boxY, (this.mapBounds.Width + 16) * 4, (this.mapBounds.Height + 32) * 4, false, true, null, false, true, -1, -1, -1);
			}
			float sortLayer = 0.86f;
			MapAreaTexture baseTexture = this.mapRegion.GetBaseTexture();
			if (baseTexture != null)
			{
				Rectangle destRect = baseTexture.GetOffsetMapPixelArea(this.mapBounds.X, this.mapBounds.Y);
				b.Draw(baseTexture.Texture, destRect, new Rectangle?(baseTexture.SourceRect), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
				sortLayer += 0.001f;
			}
			MapArea[] array = this.mapAreas;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (MapAreaTexture overlay in array[i].GetTextures())
				{
					Rectangle destRect2 = overlay.GetOffsetMapPixelArea(this.mapBounds.X, this.mapBounds.Y);
					b.Draw(overlay.Texture, destRect2, new Rectangle?(overlay.SourceRect), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
					sortLayer += 0.001f;
				}
			}
			if (MapPage.EnableDebugLines != MapPage.WorldMapDebugLineType.None)
			{
				foreach (MapArea area in this.mapAreas)
				{
					if (MapPage.EnableDebugLines.HasFlag(MapPage.WorldMapDebugLineType.Tooltips))
					{
						MapAreaTooltip[] tooltips = area.GetTooltips();
						for (int j = 0; j < tooltips.Length; j++)
						{
							Rectangle pixelArea = tooltips[j].GetPixelArea();
							pixelArea = new Rectangle(this.mapBounds.X + pixelArea.X, this.mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
							Utility.DrawSquare(b, pixelArea, 2, new Color?(Color.Blue * alpha), null);
						}
					}
					if (MapPage.EnableDebugLines.HasFlag(MapPage.WorldMapDebugLineType.Areas))
					{
						Rectangle pixelArea2 = area.Data.PixelArea;
						if (pixelArea2.Width > 0 || pixelArea2.Height > 0)
						{
							pixelArea2 = new Rectangle(this.mapBounds.X + pixelArea2.X * 4, this.mapBounds.Y + pixelArea2.Y * 4, pixelArea2.Width * 4, pixelArea2.Height * 4);
							Utility.DrawSquare(b, pixelArea2, 4, new Color?(Color.Black * alpha), null);
						}
					}
					if (MapPage.EnableDebugLines.HasFlag(MapPage.WorldMapDebugLineType.Positions))
					{
						foreach (MapAreaPosition mapAreaPosition in area.GetWorldPositions())
						{
							Rectangle pixelArea3 = mapAreaPosition.GetPixelArea();
							pixelArea3 = new Rectangle(this.mapBounds.X + pixelArea3.X, this.mapBounds.Y + pixelArea3.Y, pixelArea3.Width, pixelArea3.Height);
							Utility.DrawSquare(b, pixelArea3, 2, new Color?(Color.Red * alpha), null);
						}
					}
				}
			}
		}

		// Token: 0x06002AEC RID: 10988 RVA: 0x00205FEC File Offset: 0x002041EC
		public virtual void drawTooltip(SpriteBatch b)
		{
			if (!string.IsNullOrEmpty(this.hoverText))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x06002AED RID: 10989 RVA: 0x00206050 File Offset: 0x00204250
		public Point GetNormalizedPlayerTile(Farmer player)
		{
			Point tile = player.TilePoint;
			if (tile.X < 0 || tile.Y < 0)
			{
				tile = new Point(Math.Max(0, tile.X), Math.Max(0, tile.Y));
			}
			return tile;
		}

		// Token: 0x04001C89 RID: 7305
		public static MapPage.WorldMapDebugLineType EnableDebugLines;

		// Token: 0x04001C8A RID: 7306
		public readonly MapAreaPositionWithContext? mapPosition;

		// Token: 0x04001C8B RID: 7307
		public readonly MapRegion mapRegion;

		// Token: 0x04001C8C RID: 7308
		public readonly MapArea[] mapAreas;

		// Token: 0x04001C8D RID: 7309
		public readonly string scrollText;

		// Token: 0x04001C8E RID: 7310
		public readonly int defaultComponentID;

		// Token: 0x04001C8F RID: 7311
		public Rectangle mapBounds;

		// Token: 0x04001C90 RID: 7312
		public readonly Dictionary<string, ClickableComponent> points = new Dictionary<string, ClickableComponent>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04001C91 RID: 7313
		public string hoverText = "";

		// Token: 0x02000623 RID: 1571
		[Flags]
		public enum WorldMapDebugLineType
		{
			// Token: 0x04002E93 RID: 11923
			None = 0,
			// Token: 0x04002E94 RID: 11924
			Areas = 1,
			// Token: 0x04002E95 RID: 11925
			Positions = 2,
			// Token: 0x04002E96 RID: 11926
			Tooltips = 4,
			// Token: 0x04002E97 RID: 11927
			All = -1
		}
	}
}
