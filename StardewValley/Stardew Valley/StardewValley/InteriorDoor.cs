using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x020000D2 RID: 210
	public class InteriorDoor : NetField<bool, InteriorDoor>
	{
		// Token: 0x06001057 RID: 4183 RVA: 0x000C5FE5 File Offset: 0x000C41E5
		public InteriorDoor()
		{
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x000C5FED File Offset: 0x000C41ED
		public InteriorDoor(GameLocation location, Point position) : this()
		{
			this.Location = location;
			this.Position = position;
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x000C6003 File Offset: 0x000C4203
		public override void Set(bool newValue)
		{
			if (newValue != this.value)
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x000C601C File Offset: 0x000C421C
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			bool newValue = reader.ReadBoolean();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x000C6046 File Offset: 0x000C4246
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.targetValue);
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x000C6054 File Offset: 0x000C4254
		public void ResetLocalState()
		{
			int x = this.Position.X;
			int y = this.Position.Y;
			Location doorLocation = new Location(x, y);
			Layer buildingsLayer = this.Location.Map.RequireLayer("Buildings");
			Layer backLayer = this.Location.Map.RequireLayer("Back");
			if (this.Tile == null)
			{
				this.Tile = buildingsLayer.Tiles[doorLocation];
			}
			if (this.Tile == null)
			{
				return;
			}
			string doorAction;
			if (this.Tile.Properties.TryGetValue("Action", out doorAction) && doorAction.Contains("Door"))
			{
				string[] actionParts = ArgUtility.SplitBySpace(doorAction, 2);
				if (actionParts.Length > 1)
				{
					Tile tile = backLayer.Tiles[doorLocation];
					if (tile != null && !tile.Properties.ContainsKey("TouchAction"))
					{
						tile.Properties.Add("TouchAction", "Door " + actionParts[1]);
					}
				}
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = default(Microsoft.Xna.Framework.Rectangle);
			bool flip = false;
			int tileIndex = this.Tile.TileIndex;
			if (tileIndex <= 824)
			{
				if (tileIndex != 120)
				{
					if (tileIndex == 824)
					{
						sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
					}
				}
				else
				{
					sourceRect = new Microsoft.Xna.Framework.Rectangle(512, 144, 16, 48);
				}
			}
			else if (tileIndex != 825)
			{
				if (tileIndex == 838)
				{
					sourceRect = new Microsoft.Xna.Framework.Rectangle(576, 144, 16, 48);
					if (x == 10 && y == 5)
					{
						flip = true;
					}
				}
			}
			else
			{
				sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
				flip = true;
			}
			this.Sprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 100f, 4, 1, new Vector2((float)x, (float)(y - 2)) * 64f, false, flip, (float)((y + 1) * 64 - 12) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
			{
				holdLastFrame = true,
				paused = true
			};
			if (base.Value)
			{
				this.Sprite.paused = false;
				this.Sprite.resetEnd();
			}
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x000C6290 File Offset: 0x000C4490
		public virtual void ApplyMapModifications()
		{
			if (base.Value)
			{
				this.openDoorTiles();
				return;
			}
			this.closeDoorTiles();
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x000C62A7 File Offset: 0x000C44A7
		public void CleanUpLocalState()
		{
			this.closeDoorTiles();
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x000C62AF File Offset: 0x000C44AF
		private void closeDoorSprite()
		{
			this.Sprite.reset();
			this.Sprite.paused = true;
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x000C62C8 File Offset: 0x000C44C8
		private void openDoorSprite()
		{
			this.Sprite.paused = false;
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x000C62D8 File Offset: 0x000C44D8
		private void openDoorTiles()
		{
			this.Location.setTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier", "T");
			this.Location.removeTile(this.Position.X, this.Position.Y, "Buildings");
			DelayedAction.functionAfterDelay(delegate
			{
				this.Location.removeTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier");
			}, 400);
			this.Location.removeTile(this.Position.X, this.Position.Y - 1, "Front");
			this.Location.removeTile(this.Position.X, this.Position.Y - 2, "Front");
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x000C63A4 File Offset: 0x000C45A4
		private void closeDoorTiles()
		{
			Location doorLocation = new Location(this.Position.X, this.Position.Y);
			Map map = this.Location.Map;
			if (map == null)
			{
				return;
			}
			if (this.Tile == null)
			{
				return;
			}
			map.RequireLayer("Buildings").Tiles[doorLocation] = this.Tile;
			this.Location.removeTileProperty(this.Position.X, this.Position.Y, "Back", "TemporaryBarrier");
			doorLocation.Y--;
			map.RequireLayer("Front").Tiles[doorLocation] = new StaticTile(map.RequireLayer("Front"), this.Tile.TileSheet, BlendMode.Alpha, this.Tile.TileIndex - this.Tile.TileSheet.SheetWidth);
			doorLocation.Y--;
			map.RequireLayer("Front").Tiles[doorLocation] = new StaticTile(map.RequireLayer("Front"), this.Tile.TileSheet, BlendMode.Alpha, this.Tile.TileIndex - this.Tile.TileSheet.SheetWidth * 2);
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x000C64E4 File Offset: 0x000C46E4
		public void Update(GameTime time)
		{
			if (this.Sprite == null)
			{
				return;
			}
			if (base.Value && this.Sprite.paused)
			{
				this.openDoorSprite();
				this.openDoorTiles();
			}
			else if (!base.Value && !this.Sprite.paused)
			{
				this.closeDoorSprite();
				this.closeDoorTiles();
			}
			this.Sprite.update(time);
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x000C654B File Offset: 0x000C474B
		public void Draw(SpriteBatch b)
		{
			TemporaryAnimatedSprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			sprite.draw(b, false, 0, 0, 1f);
		}

		// Token: 0x040009F8 RID: 2552
		public GameLocation Location;

		// Token: 0x040009F9 RID: 2553
		public Point Position;

		// Token: 0x040009FA RID: 2554
		public TemporaryAnimatedSprite Sprite;

		// Token: 0x040009FB RID: 2555
		public Tile Tile;
	}
}
