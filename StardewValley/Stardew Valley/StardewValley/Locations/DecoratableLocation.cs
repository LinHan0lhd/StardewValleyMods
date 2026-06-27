using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	// Token: 0x020002CB RID: 715
	public class DecoratableLocation : GameLocation
	{
		// Token: 0x06002E7B RID: 11899 RVA: 0x00244500 File Offset: 0x00242700
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.appliedWallpaper, "appliedWallpaper").AddField(this.appliedFloor, "appliedFloor").AddField(this.floorIDs, "floorIDs").AddField(this.wallpaperIDs, "wallpaperIDs");
			this.appliedWallpaper.OnValueAdded += delegate(string key, string value)
			{
				this.UpdateWallpaper(key);
			};
			this.appliedWallpaper.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
			{
				this.UpdateWallpaper(key);
			};
			this.appliedWallpaper.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
			{
				NetString value;
				if (this.appliedWallpaper.FieldDict.TryGetValue(key, out value))
				{
					value.CancelInterpolation();
				}
				this.UpdateWallpaper(key);
			};
			this.appliedFloor.OnValueAdded += delegate(string key, string value)
			{
				this.UpdateFloor(key);
			};
			this.appliedFloor.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
			{
				this.UpdateFloor(key);
			};
			this.appliedFloor.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
			{
				NetString value;
				if (this.appliedFloor.FieldDict.TryGetValue(key, out value))
				{
					value.CancelInterpolation();
				}
				this.UpdateFloor(key);
			};
		}

		// Token: 0x06002E7C RID: 11900 RVA: 0x002445E4 File Offset: 0x002427E4
		public DecoratableLocation()
		{
		}

		// Token: 0x06002E7D RID: 11901 RVA: 0x00244668 File Offset: 0x00242868
		public DecoratableLocation(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002E7E RID: 11902 RVA: 0x002446EE File Offset: 0x002428EE
		public override void updateLayout()
		{
			base.updateLayout();
			if (Game1.IsMasterGame)
			{
				this.setWallpapers();
				this.setFloors();
			}
		}

		// Token: 0x06002E7F RID: 11903 RVA: 0x0024470C File Offset: 0x0024290C
		public virtual void ReadWallpaperAndFloorTileData()
		{
			this.updateMap();
			this.wallpaperTiles.Clear();
			this.floorTiles.Clear();
			this.wallpaperIDs.Clear();
			this.floorIDs.Clear();
			string defaultWallpaper = "0";
			string defaultFlooring = "0";
			FarmHouse farmHouse = this as FarmHouse;
			if (farmHouse != null && farmHouse.upgradeLevel < 3)
			{
				Farm farm = Game1.getLocationFromName("Farm", false) as Farm;
				defaultWallpaper = (FarmHouse.GetStarterWallpaper(farm) ?? "0");
				defaultFlooring = (FarmHouse.GetStarterFlooring(farm) ?? "0");
			}
			Dictionary<string, string> initial_values = new Dictionary<string, string>();
			string wallProperty;
			if (base.TryGetMapProperty("WallIDs", out wallProperty))
			{
				string[] array = wallProperty.Split(',', StringSplitOptions.None);
				for (int k = 0; k < array.Length; k++)
				{
					string[] data_split = ArgUtility.SplitBySpace(array[k]);
					if (data_split.Length >= 1)
					{
						this.wallpaperIDs.Add(data_split[0]);
					}
					if (data_split.Length >= 2)
					{
						initial_values[data_split[0]] = data_split[1];
					}
				}
			}
			if (this.wallpaperIDs.Count == 0)
			{
				List<Microsoft.Xna.Framework.Rectangle> walls = this.getWalls();
				for (int i = 0; i < walls.Count; i++)
				{
					string id = "Wall_" + i.ToString();
					this.wallpaperIDs.Add(id);
					Microsoft.Xna.Framework.Rectangle rect = walls[i];
					if (!this.wallpaperTiles.ContainsKey(i.ToString()))
					{
						this.wallpaperTiles[id] = new List<Vector3>();
					}
					foreach (Point tile in rect.GetPoints())
					{
						this.wallpaperTiles[id].Add(new Vector3((float)tile.X, (float)tile.Y, (float)(tile.Y - rect.Top)));
					}
				}
			}
			else
			{
				for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
					{
						string tile_property = this.doesTileHaveProperty(x, y, "WallID", "Back", false);
						if (tile_property != null)
						{
							if (!this.wallpaperIDs.Contains(tile_property))
							{
								this.wallpaperIDs.Add(tile_property);
							}
							string initial_value;
							if (this.appliedWallpaper.TryAdd(tile_property, defaultWallpaper) && initial_values.TryGetValue(tile_property, out initial_value))
							{
								string newValue;
								if (this.appliedWallpaper.TryGetValue(initial_value, out newValue))
								{
									this.appliedWallpaper[tile_property] = newValue;
								}
								else if (this.GetWallpaperSource(initial_value).Value >= 0)
								{
									this.appliedWallpaper[tile_property] = initial_value;
								}
							}
							List<Vector3> areas;
							if (!this.wallpaperTiles.TryGetValue(tile_property, out areas))
							{
								areas = (this.wallpaperTiles[tile_property] = new List<Vector3>());
							}
							areas.Add(new Vector3((float)x, (float)y, 0f));
							if (this.IsFloorableOrWallpaperableTile(x, y + 1, "Back"))
							{
								areas.Add(new Vector3((float)x, (float)(y + 1), 1f));
							}
							if (this.IsFloorableOrWallpaperableTile(x, y + 2, "Buildings"))
							{
								areas.Add(new Vector3((float)x, (float)(y + 2), 2f));
							}
							else if (this.IsFloorableOrWallpaperableTile(x, y + 2, "Back") && !this.IsFloorableTile(x, y + 2, "Back"))
							{
								areas.Add(new Vector3((float)x, (float)(y + 2), 2f));
							}
						}
					}
				}
			}
			initial_values.Clear();
			string floorProperty;
			if (base.TryGetMapProperty("FloorIDs", out floorProperty))
			{
				string[] array = floorProperty.Split(',', StringSplitOptions.None);
				for (int k = 0; k < array.Length; k++)
				{
					string[] data_split2 = ArgUtility.SplitBySpace(array[k]);
					if (data_split2.Length >= 1)
					{
						this.floorIDs.Add(data_split2[0]);
					}
					if (data_split2.Length >= 2)
					{
						initial_values[data_split2[0]] = data_split2[1];
					}
				}
			}
			if (this.floorIDs.Count == 0)
			{
				List<Microsoft.Xna.Framework.Rectangle> floors = this.getFloors();
				for (int j = 0; j < floors.Count; j++)
				{
					string id2 = "Floor_" + j.ToString();
					this.floorIDs.Add(id2);
					Microsoft.Xna.Framework.Rectangle rect2 = floors[j];
					if (!this.floorTiles.ContainsKey(j.ToString()))
					{
						this.floorTiles[id2] = new List<Vector3>();
					}
					foreach (Point tile2 in rect2.GetPoints())
					{
						this.floorTiles[id2].Add(new Vector3((float)tile2.X, (float)tile2.Y, 0f));
					}
				}
			}
			else
			{
				for (int x2 = 0; x2 < this.map.Layers[0].LayerWidth; x2++)
				{
					for (int y2 = 0; y2 < this.map.Layers[0].LayerHeight; y2++)
					{
						string tile_property2 = this.doesTileHaveProperty(x2, y2, "FloorID", "Back", false);
						if (tile_property2 != null)
						{
							if (!this.floorIDs.Contains(tile_property2))
							{
								this.floorIDs.Add(tile_property2);
							}
							string initial_value2;
							if (this.appliedFloor.TryAdd(tile_property2, defaultFlooring) && initial_values.TryGetValue(tile_property2, out initial_value2))
							{
								string newValue2;
								if (this.appliedFloor.TryGetValue(initial_value2, out newValue2))
								{
									this.appliedFloor[tile_property2] = newValue2;
								}
								else if (this.GetFloorSource(initial_value2).Value >= 0)
								{
									this.appliedFloor[tile_property2] = initial_value2;
								}
							}
							List<Vector3> areas2;
							if (!this.floorTiles.TryGetValue(tile_property2, out areas2))
							{
								areas2 = (this.floorTiles[tile_property2] = new List<Vector3>());
							}
							areas2.Add(new Vector3((float)x2, (float)y2, 0f));
						}
					}
				}
			}
			this.setFloors();
			this.setWallpapers();
		}

		// Token: 0x06002E80 RID: 11904 RVA: 0x00244D60 File Offset: 0x00242F60
		public virtual TileSheet GetWallAndFloorTilesheet(string id)
		{
			if (this.map != this._wallAndFloorTileSheetMap)
			{
				this._wallAndFloorTileSheets.Clear();
				this._wallAndFloorTileSheetMap = this.map;
			}
			TileSheet wallAndFloorTilesheet;
			if (this._wallAndFloorTileSheets.TryGetValue(id, out wallAndFloorTilesheet))
			{
				return wallAndFloorTilesheet;
			}
			TileSheet result;
			try
			{
				foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
				{
					if (!(entry.Id != id))
					{
						Texture2D texture = Game1.content.Load<Texture2D>(entry.Texture);
						if (texture.Width != 256)
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 3);
							defaultInterpolatedStringHandler.AppendLiteral("The tilesheet for wallpaper/floor '");
							defaultInterpolatedStringHandler.AppendFormatted(entry.Id);
							defaultInterpolatedStringHandler.AppendLiteral("' is ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(texture.Width);
							defaultInterpolatedStringHandler.AppendLiteral(" pixels wide, but it must be exactly ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(256);
							defaultInterpolatedStringHandler.AppendLiteral(" pixels wide.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						TileSheet tilesheet = new TileSheet("x_WallsAndFloors_" + id, this.map, entry.Texture, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
						this.map.AddTileSheet(tilesheet);
						this.map.LoadTileSheets(Game1.mapDisplayDevice);
						this._wallAndFloorTileSheets[id] = tilesheet;
						return tilesheet;
					}
				}
				Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded: no such ID found in Data/AdditionalWallpaperFlooring.", null);
				this._wallAndFloorTileSheets[id] = null;
				result = null;
			}
			catch (Exception ex)
			{
				Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded.", ex);
				this._wallAndFloorTileSheets[id] = null;
				result = null;
			}
			return result;
		}

		// Token: 0x06002E81 RID: 11905 RVA: 0x00244F80 File Offset: 0x00243180
		public virtual KeyValuePair<string, int> GetFloorSource(string pattern_id)
		{
			int pattern_index;
			if (pattern_id.Contains(':'))
			{
				string[] pattern_split = pattern_id.Split(':', StringSplitOptions.None);
				TileSheet tilesheet = this.GetWallAndFloorTilesheet(pattern_split[0]);
				if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
				{
					return new KeyValuePair<string, int>(tilesheet.Id, pattern_index);
				}
			}
			if (int.TryParse(pattern_id, out pattern_index))
			{
				return new KeyValuePair<string, int>("walls_and_floors", pattern_index);
			}
			return new KeyValuePair<string, int>(null, -1);
		}

		// Token: 0x06002E82 RID: 11906 RVA: 0x00244FE4 File Offset: 0x002431E4
		public virtual KeyValuePair<string, int> GetWallpaperSource(string pattern_id)
		{
			int pattern_index;
			if (pattern_id.Contains(':'))
			{
				string[] pattern_split = pattern_id.Split(':', StringSplitOptions.None);
				TileSheet tilesheet = this.GetWallAndFloorTilesheet(pattern_split[0]);
				if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
				{
					return new KeyValuePair<string, int>(tilesheet.Id, pattern_index);
				}
			}
			if (int.TryParse(pattern_id, out pattern_index))
			{
				return new KeyValuePair<string, int>("walls_and_floors", pattern_index);
			}
			return new KeyValuePair<string, int>(null, -1);
		}

		// Token: 0x06002E83 RID: 11907 RVA: 0x00245048 File Offset: 0x00243248
		public virtual void UpdateFloor(string floorId)
		{
			this.updateMap();
			string patternId;
			List<Vector3> tiles;
			if (this.appliedFloor.TryGetValue(floorId, out patternId) && this.floorTiles.TryGetValue(floorId, out tiles))
			{
				bool appliedAny = false;
				HashSet<string> errors = null;
				foreach (Vector3 vector in tiles)
				{
					int x = (int)vector.X;
					int y = (int)vector.Y;
					KeyValuePair<string, int> source = this.GetFloorSource(patternId);
					if (source.Value < 0)
					{
						if (DecoratableLocation.LogTroubleshootingInfo)
						{
							errors = (errors ?? new HashSet<string>());
							errors.Add("floor pattern '" + patternId + "' doesn't match any known floor set");
						}
					}
					else
					{
						string tilesheetId = source.Key;
						int spriteIndex = source.Value;
						int tilesWide = this.map.RequireTileSheet(tilesheetId).SheetWidth;
						spriteIndex = spriteIndex * 2 + spriteIndex / (tilesWide / 2) * tilesWide;
						if (tilesheetId == "walls_and_floors")
						{
							spriteIndex += this.GetFirstFlooringTile();
						}
						string reason;
						if (!this.IsFloorableOrWallpaperableTile(x, y, "Back", out reason))
						{
							if (DecoratableLocation.LogTroubleshootingInfo)
							{
								errors = (errors ?? new HashSet<string>());
								errors.Add(reason);
							}
						}
						else
						{
							base.setMapTile(x, y, this.GetFlooringIndex(spriteIndex, x, y), "Back", tilesheetId, null, true);
							appliedAny = true;
						}
					}
				}
				if (!appliedAny && errors != null && errors.Count > 0)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't apply floors for area ID '");
					defaultInterpolatedStringHandler.AppendFormatted(floorId);
					defaultInterpolatedStringHandler.AppendLiteral("' (");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("; ", errors));
					defaultInterpolatedStringHandler.AppendLiteral(")");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}

		// Token: 0x06002E84 RID: 11908 RVA: 0x00245230 File Offset: 0x00243430
		public virtual void UpdateWallpaper(string wallpaperId)
		{
			this.updateMap();
			string patternId;
			List<Vector3> tiles;
			if (this.appliedWallpaper.TryGetValue(wallpaperId, out patternId) && this.wallpaperTiles.TryGetValue(wallpaperId, out tiles))
			{
				bool appliedAny = false;
				HashSet<string> errors = null;
				foreach (Vector3 vector in tiles)
				{
					int x = (int)vector.X;
					int y = (int)vector.Y;
					int type = (int)vector.Z;
					KeyValuePair<string, int> source = this.GetWallpaperSource(patternId);
					if (source.Value < 0)
					{
						if (DecoratableLocation.LogTroubleshootingInfo)
						{
							errors = (errors ?? new HashSet<string>());
							errors.Add("wallpaper pattern '" + patternId + "' doesn't match any known wallpaper set");
						}
					}
					else
					{
						string tileSheetId = source.Key;
						int spriteIndex = source.Value;
						TileSheet tilesheet = this.map.RequireTileSheet(tileSheetId);
						int tilesWide = tilesheet.SheetWidth;
						string text;
						string layer = (type == 2 && this.IsFloorableOrWallpaperableTile(x, y, "Buildings", out text)) ? "Buildings" : "Back";
						string reason;
						if (!this.IsFloorableOrWallpaperableTile(x, y, layer, out reason))
						{
							if (DecoratableLocation.LogTroubleshootingInfo)
							{
								errors = (errors ?? new HashSet<string>());
								errors.Add(reason);
							}
						}
						else
						{
							base.setMapTile(x, y, spriteIndex / tilesWide * tilesWide * 3 + spriteIndex % tilesWide + type * tilesWide, layer, tilesheet.Id, null, true);
							appliedAny = true;
						}
					}
				}
				if (!appliedAny && errors != null && errors.Count > 0)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't apply wallpaper for area ID '");
					defaultInterpolatedStringHandler.AppendFormatted(wallpaperId);
					defaultInterpolatedStringHandler.AppendLiteral("' (");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join("; ", errors));
					defaultInterpolatedStringHandler.AppendLiteral(")");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}

		// Token: 0x06002E85 RID: 11909 RVA: 0x0024542C File Offset: 0x0024362C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (this.wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x06002E86 RID: 11910 RVA: 0x00245440 File Offset: 0x00243640
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (!(this is FarmHouse))
			{
				this.ReadWallpaperAndFloorTileData();
				this.setWallpapers();
				this.setFloors();
			}
			if (base.hasTileAt(Game1.player.TilePoint, "Buildings", null))
			{
				Game1.player.position.Y += 64f;
			}
		}

		// Token: 0x06002E87 RID: 11911 RVA: 0x002454A1 File Offset: 0x002436A1
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.player.mailReceived.Add("button_tut_1"))
			{
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(0));
			}
		}

		// Token: 0x06002E88 RID: 11912 RVA: 0x002454CF File Offset: 0x002436CF
		public override bool CanFreePlaceFurniture()
		{
			return true;
		}

		// Token: 0x06002E89 RID: 11913 RVA: 0x002454D4 File Offset: 0x002436D4
		public virtual bool isTileOnWall(int x, int y)
		{
			foreach (string id in this.wallpaperTiles.Keys)
			{
				foreach (Vector3 tile_data in this.wallpaperTiles[id])
				{
					if ((int)tile_data.X == x && (int)tile_data.Y == y)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002E8A RID: 11914 RVA: 0x00245584 File Offset: 0x00243784
		public int GetWallTopY(int x, int y)
		{
			foreach (string id in this.wallpaperTiles.Keys)
			{
				foreach (Vector3 tile_data in this.wallpaperTiles[id])
				{
					if ((int)tile_data.X == x && (int)tile_data.Y == y)
					{
						return y - (int)tile_data.Z;
					}
				}
			}
			return -1;
		}

		// Token: 0x06002E8B RID: 11915 RVA: 0x0024563C File Offset: 0x0024383C
		public virtual void setFloors()
		{
			foreach (KeyValuePair<string, string> kvp in this.appliedFloor.Pairs)
			{
				this.UpdateFloor(kvp.Key);
			}
		}

		// Token: 0x06002E8C RID: 11916 RVA: 0x002456A0 File Offset: 0x002438A0
		public virtual void setWallpapers()
		{
			foreach (KeyValuePair<string, string> kvp in this.appliedWallpaper.Pairs)
			{
				this.UpdateWallpaper(kvp.Key);
			}
		}

		// Token: 0x06002E8D RID: 11917 RVA: 0x00245704 File Offset: 0x00243904
		public void SetFloor(string which, string which_room)
		{
			if (which_room == null)
			{
				foreach (string key in this.floorIDs)
				{
					this.appliedFloor[key] = which;
				}
				return;
			}
			this.appliedFloor[which_room] = which;
		}

		// Token: 0x06002E8E RID: 11918 RVA: 0x00245770 File Offset: 0x00243970
		public void SetWallpaper(string which, string which_room)
		{
			if (which_room == null)
			{
				foreach (string key in this.wallpaperIDs)
				{
					this.appliedWallpaper[key] = which;
				}
				return;
			}
			this.appliedWallpaper[which_room] = which;
		}

		// Token: 0x06002E8F RID: 11919 RVA: 0x002457DC File Offset: 0x002439DC
		public void OverrideSpecificWallpaper(string which, string which_room, string wallpaperStyleToOverride)
		{
			if (which_room == null)
			{
				foreach (string key in this.wallpaperIDs)
				{
					string prevStyle;
					if (this.appliedWallpaper.TryGetValue(key, out prevStyle) && prevStyle == wallpaperStyleToOverride)
					{
						this.appliedWallpaper[key] = which;
					}
				}
				return;
			}
			if (this.appliedWallpaper[which_room] == wallpaperStyleToOverride)
			{
				this.appliedWallpaper[which_room] = which;
			}
		}

		// Token: 0x06002E90 RID: 11920 RVA: 0x00245874 File Offset: 0x00243A74
		public void OverrideSpecificFlooring(string which, string which_room, string flooringStyleToOverride)
		{
			if (which_room == null)
			{
				foreach (string key in this.floorIDs)
				{
					string prevStyle;
					if (this.appliedFloor.TryGetValue(key, out prevStyle) && prevStyle == flooringStyleToOverride)
					{
						this.appliedFloor[key] = which;
					}
				}
				return;
			}
			if (this.appliedFloor[which_room] == flooringStyleToOverride)
			{
				this.appliedFloor[which_room] = which;
			}
		}

		// Token: 0x06002E91 RID: 11921 RVA: 0x0024590C File Offset: 0x00243B0C
		public string GetFloorID(int x, int y)
		{
			foreach (string id in this.floorTiles.Keys)
			{
				foreach (Vector3 tile_data in this.floorTiles[id])
				{
					if ((int)tile_data.X == x && (int)tile_data.Y == y)
					{
						return id;
					}
				}
			}
			return null;
		}

		// Token: 0x06002E92 RID: 11922 RVA: 0x002459BC File Offset: 0x00243BBC
		public string GetWallpaperID(int x, int y)
		{
			foreach (string id in this.wallpaperTiles.Keys)
			{
				foreach (Vector3 tile_data in this.wallpaperTiles[id])
				{
					if ((int)tile_data.X == x && (int)tile_data.Y == y)
					{
						return id;
					}
				}
			}
			return null;
		}

		// Token: 0x06002E93 RID: 11923 RVA: 0x00245A6C File Offset: 0x00243C6C
		protected bool IsFloorableTile(int x, int y, string layer_name)
		{
			int tileIndex = base.getTileIndexAt(x, y, "Buildings", "untitled tile sheet");
			return (tileIndex < 197 || tileIndex > 199) && this.IsFloorableOrWallpaperableTile(x, y, layer_name);
		}

		// Token: 0x06002E94 RID: 11924 RVA: 0x00245AA7 File Offset: 0x00243CA7
		public bool IsWallAndFloorTilesheet(string tilesheet_id)
		{
			return tilesheet_id == "walls_and_floors" || tilesheet_id.Contains("walls_and_floors") || tilesheet_id.StartsWith("x_WallsAndFloors_");
		}

		// Token: 0x06002E95 RID: 11925 RVA: 0x00245AD0 File Offset: 0x00243CD0
		protected bool IsFloorableOrWallpaperableTile(int x, int y, string layerName)
		{
			string text;
			return this.IsFloorableOrWallpaperableTile(x, y, layerName, out text);
		}

		// Token: 0x06002E96 RID: 11926 RVA: 0x00245AE8 File Offset: 0x00243CE8
		protected bool IsFloorableOrWallpaperableTile(int x, int y, string layerName, out string reasonInvalid)
		{
			Layer layer = this.map.GetLayer(layerName);
			if (layer == null)
			{
				reasonInvalid = "layer '" + layerName + "' not found";
				return false;
			}
			if (x < 0 || x >= layer.LayerWidth || y < 0 || y >= layer.LayerHeight)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
				defaultInterpolatedStringHandler.AppendLiteral("tile (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(x);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(y);
				defaultInterpolatedStringHandler.AppendLiteral(") is out of bounds for the layer");
				reasonInvalid = defaultInterpolatedStringHandler.ToStringAndClear();
				return false;
			}
			Tile tile = layer.Tiles[x, y];
			if (tile == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("tile (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(x);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(y);
				defaultInterpolatedStringHandler.AppendLiteral(") not found");
				reasonInvalid = defaultInterpolatedStringHandler.ToStringAndClear();
				return false;
			}
			TileSheet tilesheet = tile.TileSheet;
			if (tilesheet == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("tile (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(x);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(y);
				defaultInterpolatedStringHandler.AppendLiteral(") has unknown tilesheet");
				reasonInvalid = defaultInterpolatedStringHandler.ToStringAndClear();
				return false;
			}
			if (!this.IsWallAndFloorTilesheet(tilesheet.Id))
			{
				reasonInvalid = "tilesheet '" + tilesheet.Id + "' isn't a wall and floor tilesheet, expected tilesheet ID containing 'walls_and_floors' or starting with 'x_WallsAndFloors_'";
				return false;
			}
			reasonInvalid = null;
			return true;
		}

		// Token: 0x06002E97 RID: 11927 RVA: 0x00245C5C File Offset: 0x00243E5C
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			DecoratableLocation decoratable_location = l as DecoratableLocation;
			if (decoratable_location != null)
			{
				if (!decoratable_location.appliedWallpaper.Keys.Any() && !decoratable_location.appliedFloor.Keys.Any())
				{
					this.ReadWallpaperAndFloorTileData();
					for (int i = 0; i < decoratable_location.wallPaper.Count; i++)
					{
						try
						{
							string key = this.wallpaperIDs[i];
							string value = decoratable_location.wallPaper[i].ToString();
							this.appliedWallpaper[key] = value;
						}
						catch (Exception)
						{
						}
					}
					for (int j = 0; j < decoratable_location.floor.Count; j++)
					{
						try
						{
							string key2 = this.floorIDs[j];
							string value2 = decoratable_location.floor[j].ToString();
							this.appliedFloor[key2] = value2;
						}
						catch (Exception)
						{
						}
					}
				}
				else
				{
					foreach (string key3 in decoratable_location.appliedWallpaper.Keys)
					{
						this.appliedWallpaper[key3] = decoratable_location.appliedWallpaper[key3];
					}
					foreach (string key4 in decoratable_location.appliedFloor.Keys)
					{
						this.appliedFloor[key4] = decoratable_location.appliedFloor[key4];
					}
				}
			}
			this.setWallpapers();
			this.setFloors();
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06002E98 RID: 11928 RVA: 0x00245E44 File Offset: 0x00244044
		public Furniture getRandomFurniture(Random r)
		{
			return r.ChooseFrom(this.furniture);
		}

		// Token: 0x06002E99 RID: 11929 RVA: 0x00245E54 File Offset: 0x00244054
		public virtual string getFloorRoomIdAt(Point p)
		{
			foreach (string key in this.floorTiles.Keys)
			{
				foreach (Vector3 tile_data in this.floorTiles[key])
				{
					if ((int)tile_data.X == p.X && (int)tile_data.Y == p.Y)
					{
						return key;
					}
				}
			}
			return null;
		}

		// Token: 0x06002E9A RID: 11930 RVA: 0x00245F10 File Offset: 0x00244110
		public virtual int GetFirstFlooringTile()
		{
			return 336;
		}

		// Token: 0x06002E9B RID: 11931 RVA: 0x00245F18 File Offset: 0x00244118
		public virtual int GetFlooringIndex(int base_tile_sheet, int tile_x, int tile_y)
		{
			if (!base.hasTileAt(tile_x, tile_y, "Back", null))
			{
				return 0;
			}
			string tilesheet_name = base.getTileSheetIDAt(tile_x, tile_y, "Back");
			TileSheet tilesheet = this.map.GetTileSheet(tilesheet_name);
			int tiles_wide = 16;
			if (tilesheet != null)
			{
				tiles_wide = tilesheet.SheetWidth;
			}
			int x_offset = tile_x % 2;
			int y_offset = tile_y % 2;
			return base_tile_sheet + x_offset + tiles_wide * y_offset;
		}

		// Token: 0x06002E9C RID: 11932 RVA: 0x00245F70 File Offset: 0x00244170
		public virtual List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>();
		}

		// Token: 0x04001FB4 RID: 8116
		public readonly DecorationFacade wallPaper = new DecorationFacade();

		// Token: 0x04001FB5 RID: 8117
		[XmlIgnore]
		public readonly NetStringList wallpaperIDs = new NetStringList();

		// Token: 0x04001FB6 RID: 8118
		public readonly NetStringDictionary<string, NetString> appliedWallpaper = new NetStringDictionary<string, NetString>
		{
			InterpolationWait = false
		};

		// Token: 0x04001FB7 RID: 8119
		[XmlIgnore]
		public readonly Dictionary<string, List<Vector3>> wallpaperTiles = new Dictionary<string, List<Vector3>>();

		// Token: 0x04001FB8 RID: 8120
		public readonly DecorationFacade floor = new DecorationFacade();

		// Token: 0x04001FB9 RID: 8121
		[XmlIgnore]
		public readonly NetStringList floorIDs = new NetStringList();

		// Token: 0x04001FBA RID: 8122
		public readonly NetStringDictionary<string, NetString> appliedFloor = new NetStringDictionary<string, NetString>
		{
			InterpolationWait = false
		};

		// Token: 0x04001FBB RID: 8123
		[XmlIgnore]
		public readonly Dictionary<string, List<Vector3>> floorTiles = new Dictionary<string, List<Vector3>>();

		// Token: 0x04001FBC RID: 8124
		protected Dictionary<string, TileSheet> _wallAndFloorTileSheets = new Dictionary<string, TileSheet>();

		// Token: 0x04001FBD RID: 8125
		protected Map _wallAndFloorTileSheetMap;

		// Token: 0x04001FBE RID: 8126
		public static bool LogTroubleshootingInfo;
	}
}
