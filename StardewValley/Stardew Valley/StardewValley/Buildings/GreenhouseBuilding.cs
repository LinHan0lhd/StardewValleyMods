using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Buildings
{
	// Token: 0x02000384 RID: 900
	public class GreenhouseBuilding : Building
	{
		// Token: 0x060037A8 RID: 14248 RVA: 0x002C2616 File Offset: 0x002C0816
		public GreenhouseBuilding(Vector2 tileLocation) : base("Greenhouse", tileLocation)
		{
		}

		// Token: 0x060037A9 RID: 14249 RVA: 0x002C2624 File Offset: 0x002C0824
		public GreenhouseBuilding() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037AA RID: 14250 RVA: 0x002C2631 File Offset: 0x002C0831
		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			if (base.isMoving)
			{
				return;
			}
			this.DrawEntranceTiles(b);
			this.drawShadow(b, -1, -1);
		}

		// Token: 0x060037AB RID: 14251 RVA: 0x002C2653 File Offset: 0x002C0853
		public Farm GetFarm()
		{
			if (this._farm == null)
			{
				this._farm = Game1.getFarm();
			}
			return this._farm;
		}

		// Token: 0x060037AC RID: 14252 RVA: 0x002C266E File Offset: 0x002C086E
		public override bool OnUseHumanDoor(Farmer who)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
			{
				return true;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GreenhouseRuins"));
			return false;
		}

		// Token: 0x060037AD RID: 14253 RVA: 0x002C269D File Offset: 0x002C089D
		public override string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_position)
		{
			return null;
		}

		// Token: 0x060037AE RID: 14254 RVA: 0x002C26A0 File Offset: 0x002C08A0
		public override bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (base.isMoving)
			{
				return false;
			}
			if (layer_name == "Back" && ((tile_x >= this.tileX.Value - 1 && tile_x <= this.tileX.Value + this.tilesWide.Value - 1 && tile_y <= this.tileY.Value + this.tilesHigh.Value && tile_y >= this.tileY.Value) || (this.CanDrawEntranceTiles() && tile_x >= this.tileX.Value + 1 && tile_x <= this.tileX.Value + this.tilesWide.Value - 2 && tile_y == this.tileY.Value + this.tilesHigh.Value + 1)))
			{
				if (this.CanDrawEntranceTiles() && tile_x >= this.tileX.Value + this.humanDoor.X - 1 && tile_x <= this.tileX.Value + this.humanDoor.X + 1 && tile_y <= this.tileY.Value + this.tilesHigh.Value + 1 && tile_y >= this.tileY.Value + this.humanDoor.Y + 1)
				{
					if (property_name == "Type")
					{
						property_value = "Stone";
						return true;
					}
					if (property_name == "NoSpawn")
					{
						property_value = "All";
						return true;
					}
					if (property_name == "Buildable")
					{
						property_value = null;
						return true;
					}
				}
				if (property_name == "Buildable")
				{
					property_value = "T";
					return true;
				}
				if (property_name == "NoSpawn")
				{
					property_value = "Tree";
					return true;
				}
				if (property_name == "Diggable")
				{
					property_value = null;
					return true;
				}
			}
			return base.doesTileHaveProperty(tile_x, tile_y, property_name, layer_name, ref property_value);
		}

		// Token: 0x060037AF RID: 14255 RVA: 0x002C2889 File Offset: 0x002C0A89
		public virtual bool CanDrawEntranceTiles()
		{
			return true;
		}

		// Token: 0x060037B0 RID: 14256 RVA: 0x002C288C File Offset: 0x002C0A8C
		public virtual void DrawEntranceTiles(SpriteBatch b)
		{
			Map map = this.GetFarm().Map;
			Layer back_layer = map.RequireLayer("Back");
			TileSheet tilesheet = map.GetTileSheet("untitled tile sheet");
			if (tilesheet == null)
			{
				tilesheet = map.TileSheets[Math.Min(1, map.TileSheets.Count - 1)];
			}
			if (tilesheet == null)
			{
				return;
			}
			StaticTile tile = new StaticTile(back_layer, tilesheet, BlendMode.Alpha, 812);
			if (this.CanDrawEntranceTiles())
			{
				float draw_layer = 0f;
				Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value + this.humanDoor.Value.X - 1), (float)(this.tileY.Value + this.humanDoor.Value.Y + 1)) * 64f);
				Location draw_location = new Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
				tile = new StaticTile(back_layer, tilesheet, BlendMode.Alpha, 838);
				vector_draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX.Value + this.humanDoor.Value.X - 1), (float)(this.tileY.Value + this.humanDoor.Value.Y + 2)) * 64f);
				draw_location.X = (int)vector_draw_position.X;
				draw_location.Y = (int)vector_draw_position.Y;
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile, draw_location, draw_layer);
			}
		}

		// Token: 0x060037B1 RID: 14257 RVA: 0x002C2A88 File Offset: 0x002C0C88
		public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Microsoft.Xna.Framework.Rectangle shadow_rectangle = new Microsoft.Xna.Framework.Rectangle(112, 0, 128, 144);
			if (this.CanDrawEntranceTiles())
			{
				shadow_rectangle.Y = 144;
			}
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((this.tileX.Value - 1) * 64), (float)(this.tileY.Value * 64))), new Microsoft.Xna.Framework.Rectangle?(shadow_rectangle), Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}

		// Token: 0x04002431 RID: 9265
		protected Farm _farm;
	}
}
