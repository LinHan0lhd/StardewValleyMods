using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Objects;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Locations
{
	// Token: 0x020002D5 RID: 725
	public class IslandFarmHouse : DecoratableLocation
	{
		// Token: 0x06002FAB RID: 12203 RVA: 0x0025A448 File Offset: 0x00258648
		public IslandFarmHouse()
		{
			this.fridge.Value.Location = this;
		}

		// Token: 0x06002FAC RID: 12204 RVA: 0x0025A4BC File Offset: 0x002586BC
		public IslandFarmHouse(string map, string name) : base(map, name)
		{
			this.fridge.Value.Location = this;
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1798", 1, 0, false).SetPlacement(12, 8, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(3, 1, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(8, 1, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(20, 1, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614", 1, 0, false).SetPlacement(25, 1, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(1, 4, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(10, 4, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(18, 4, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294", 1, 0, false).SetPlacement(28, 4, 0));
			this.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742", 1, 0, false).SetPlacement(20, 4, 0));
			Furniture f = ItemRegistry.Create<Furniture>("(F)1755", 1, 0, false).SetPlacement(14, 9, 0);
			this.furniture.Add(f);
			this.ReadWallpaperAndFloorTileData();
			base.SetWallpaper("88", "UpperLeft");
			base.SetFloor("23", "UpperLeft");
			base.SetWallpaper("88", "UpperRight");
			base.SetFloor("48", "Kitchen");
			base.SetWallpaper("87", "Kitchen");
			base.SetFloor("52", "UpperRight");
			base.SetWallpaper("87", "BottomRight_Left");
			base.SetFloor("23", "BottomRight");
			base.SetWallpaper("87", "BottomRight_Right");
			this.fridgePosition = default(Point);
		}

		// Token: 0x06002FAD RID: 12205 RVA: 0x0025A740 File Offset: 0x00258940
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			IslandFarmHouse fromLocation = (IslandFarmHouse)l;
			this.fridge.Value = fromLocation.fridge.Value;
			this.visited.Value = fromLocation.visited.Value;
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06002FAE RID: 12206 RVA: 0x0025A787 File Offset: 0x00258987
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			this.fridge.Value.updateWhenCurrentLocation(time);
		}

		// Token: 0x06002FAF RID: 12207 RVA: 0x0025A7A4 File Offset: 0x002589A4
		public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>
			{
				new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3),
				new Microsoft.Xna.Framework.Rectangle(18, 1, 11, 3),
				new Microsoft.Xna.Framework.Rectangle(12, 5, 5, 2),
				new Microsoft.Xna.Framework.Rectangle(17, 9, 2, 2),
				new Microsoft.Xna.Framework.Rectangle(21, 9, 8, 2)
			};
		}

		// Token: 0x06002FB0 RID: 12208 RVA: 0x0025A80C File Offset: 0x00258A0C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!this.visited.Value)
			{
				this.visited.Value = true;
			}
			this.fridgePosition = (this.GetFridgePositionFromMap() ?? Point.Zero);
		}

		// Token: 0x06002FB1 RID: 12209 RVA: 0x0025A85C File Offset: 0x00258A5C
		public Point? GetFridgePositionFromMap()
		{
			Layer layer = this.map.RequireLayer("Buildings");
			for (int y = 0; y < layer.LayerHeight; y++)
			{
				for (int x = 0; x < layer.LayerWidth; x++)
				{
					if (layer.GetTileIndexAt(x, y, "untitled tile sheet") == 258)
					{
						return new Point?(new Point(x, y));
					}
				}
			}
			return null;
		}

		// Token: 0x06002FB2 RID: 12210 RVA: 0x0025A8C8 File Offset: 0x00258AC8
		public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>
			{
				new Microsoft.Xna.Framework.Rectangle(1, 3, 11, 12),
				new Microsoft.Xna.Framework.Rectangle(11, 7, 6, 9),
				new Microsoft.Xna.Framework.Rectangle(18, 3, 11, 6),
				new Microsoft.Xna.Framework.Rectangle(17, 11, 12, 6)
			};
		}

		// Token: 0x06002FB3 RID: 12211 RVA: 0x0025A920 File Offset: 0x00258B20
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.fridge, "fridge").AddField(this.visited, "visited");
			this.visited.fieldChangeVisibleEvent += delegate(NetBool a, bool b, bool c)
			{
				this.InitializeBeds();
			};
			this.fridge.fieldChangeEvent += delegate(NetRef<Chest> field, Chest oldValue, Chest newValue)
			{
				newValue.Location = this;
			};
		}

		// Token: 0x06002FB4 RID: 12212 RVA: 0x0025A988 File Offset: 0x00258B88
		public virtual void InitializeBeds()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (Game1.gameMode == 6)
			{
				return;
			}
			if (!this.visited.Value)
			{
				return;
			}
			int player_count = 0;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				player_count++;
			}
			string bedId = "2176";
			this.furniture.Add(new BedFurniture(bedId, new Vector2(22f, 3f)));
			player_count--;
			if (player_count > 0)
			{
				this.furniture.Add(new BedFurniture(bedId, new Vector2(26f, 3f)));
				player_count--;
			}
			for (int i = 0; i < Math.Min(6, player_count); i++)
			{
				int x = 3;
				int y = 3;
				if (i % 2 == 0)
				{
					x += 4;
				}
				y += i / 2 * 4;
				this.furniture.Add(new BedFurniture(bedId, new Vector2((float)x, (float)y)));
			}
		}

		// Token: 0x06002FB5 RID: 12213 RVA: 0x0025AA90 File Offset: 0x00258C90
		protected override void _updateAmbientLighting()
		{
			if (Game1.isStartingToGetDarkOut(this) || this.lightLevel.Value > 0f)
			{
				int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + base.ExtraMillisecondsPerInGameMinute);
				float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
				Game1.ambientLight = new Color((int)((byte)Utility.Lerp((float)(Game1.isRaining ? this.rainLightingColor.R : 0), (float)this.nightLightingColor.R, lerp)), (int)((byte)Utility.Lerp((float)(Game1.isRaining ? this.rainLightingColor.G : 0), (float)this.nightLightingColor.G, lerp)), (int)((byte)Utility.Lerp(0f, (float)this.nightLightingColor.B, lerp)));
				return;
			}
			Game1.ambientLight = (Game1.isRaining ? this.rainLightingColor : Color.White);
		}

		// Token: 0x06002FB6 RID: 12214 RVA: 0x0025AB8C File Offset: 0x00258D8C
		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			base.drawAboveFrontLayer(b);
			if (this.fridge.Value.mutex.IsLocked())
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)this.fridgePosition.X, (float)(this.fridgePosition.Y - 1)) * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.fridgePosition.Y + 1) * 64 + 1) / 10000f);
			}
		}

		// Token: 0x06002FB7 RID: 12215 RVA: 0x0025AC3C File Offset: 0x00258E3C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 258)
			{
				this.fridge.Value.fridge.Value = true;
				this.fridge.Value.checkForAction(who, false);
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x04002049 RID: 8265
		[XmlElement("fridge")]
		public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(true, "130"));

		// Token: 0x0400204A RID: 8266
		public Point fridgePosition;

		// Token: 0x0400204B RID: 8267
		public NetBool visited = new NetBool(false)
		{
			InterpolationEnabled = false
		};

		// Token: 0x0400204C RID: 8268
		private Color nightLightingColor = new Color(180, 180, 0);

		// Token: 0x0400204D RID: 8269
		private Color rainLightingColor = new Color(90, 90, 0);
	}
}
