using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C6 RID: 710
	public class Caldera : IslandLocation
	{
		// Token: 0x06002E16 RID: 11798 RVA: 0x00240605 File Offset: 0x0023E805
		public Caldera()
		{
		}

		// Token: 0x06002E17 RID: 11799 RVA: 0x00240618 File Offset: 0x0023E818
		public Caldera(string filename, string locationName) : base(filename, locationName)
		{
		}

		// Token: 0x06002E18 RID: 11800 RVA: 0x0024062D File Offset: 0x0023E82D
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.visited, "visited");
		}

		// Token: 0x06002E19 RID: 11801 RVA: 0x0024064C File Offset: 0x0023E84C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!this.visited.Value)
			{
				this.visited.Value = true;
			}
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
			{
				Game1.addMailForTomorrow("reachedCaldera", true, true);
			}
			this.mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(this.map.RequireTileSheet(0, "dungeon").ImageSource);
			this.waterColor.Value = Color.White;
			if (Game1.player.mailReceived.Contains("CalderaTreasure"))
			{
				this.overlayObjects.Remove(new Vector2(25f, 28f));
			}
			else if (!this.objects.ContainsKey(new Vector2(25f, 28f)))
			{
				Chest chest = new Chest(false, "227");
				chest.addItem(ItemRegistry.Create("(O)74", 1, 0, false));
				chest.synchronized.Value = false;
				chest.type.Value = "interactive";
				chest.Fragility = 2;
				chest.SetBigCraftableSpriteIndex(227, -1, 3);
				this.overlayObjects.Add(new Vector2(25f, 28f), chest);
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask"))
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(15, 333, 13, 12), new Vector2(908.8f, 1792f), false, 0f, Color.White)
				{
					scale = 4f,
					interval = 99999f,
					totalNumberOfLoops = 99999,
					yPeriodic = true,
					yPeriodicRange = 2f,
					yPeriodicLoopTime = 2500f
				});
			}
		}

		// Token: 0x06002E1A RID: 11802 RVA: 0x00240834 File Offset: 0x0023EA34
		protected override void resetSharedState()
		{
			base.resetSharedState();
			this.critters = new List<Critter>();
			if (Game1.random.NextDouble() < 0.17)
			{
				base.addCritter(new CalderaMonkey(new Vector2(12f, 21.3f) * 64f));
			}
			if (Game1.random.NextDouble() < 0.17)
			{
				base.addCritter(new CalderaMonkey(new Vector2(33f, 21.3f) * 64f));
			}
			if (Game1.random.NextDouble() < 0.17)
			{
				base.addCritter(new CalderaMonkey(new Vector2(18f, 17.3f) * 64f));
			}
		}

		// Token: 0x06002E1B RID: 11803 RVA: 0x002408FD File Offset: 0x0023EAFD
		public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			return false;
		}

		// Token: 0x06002E1C RID: 11804 RVA: 0x00240900 File Offset: 0x0023EB00
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (this.visited.Value && !Game1.player.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
			{
				Game1.addMailForTomorrow("volcanoShortcutUnlocked", true, false);
			}
		}

		// Token: 0x06002E1D RID: 11805 RVA: 0x00240934 File Offset: 0x0023EB34
		public override void drawWaterTile(SpriteBatch b, int x, int y)
		{
			bool flag = y == this.map.Layers[0].LayerHeight - 1 || !this.waterTiles[x, y + 1];
			bool topY = y == 0 || !this.waterTiles[x, y - 1];
			int water_tile_upper_left_x = 0;
			int water_tile_upper_left_y = 320;
			b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - (int)((!topY) ? this.waterPosition : 0f)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + this.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + y) % 2 == 0) ? (this.waterTileFlip ? 32 : 0) : (this.waterTileFlip ? 0 : 32)) + (topY ? ((int)this.waterPosition / 4) : 0), 16, 16 + (topY ? ((int)(-(int)this.waterPosition) / 4) : 0))), this.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			if (flag)
			{
				b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y + 1) * 64 - (int)this.waterPosition))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + this.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + (y + 1)) % 2 == 0) ? (this.waterTileFlip ? 32 : 0) : (this.waterTileFlip ? 0 : 32)), 16, 16 - (int)(16f - this.waterPosition / 4f) - 1)), this.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			}
		}

		// Token: 0x06002E1E RID: 11806 RVA: 0x00240AF4 File Offset: 0x0023ECF4
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask") && tileLocation.X == 14 && tileLocation.Y == 28)
			{
				Game1.playSound("monkey1", null);
				who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(H)92", 1, 0, false), null, false);
				Game1.player.mailReceived.Add("gotCAMask");
			}
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
			if (tileIndexAt - 123 <= 1 || tileIndexAt - 133 <= 1 || tileIndexAt - 156 <= 1)
			{
				Game1.activeClickableMenu = new ForgeMenu();
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002E1F RID: 11807 RVA: 0x00240BC4 File Offset: 0x0023EDC4
		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			return (yTile == 21 && (xTile == 22 || xTile == 23)) || (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("gotCAMask") && xTile == 14 && yTile == 28) || base.isActionableTile(xTile, yTile, who);
		}

		// Token: 0x06002E20 RID: 11808 RVA: 0x00240C23 File Offset: 0x0023EE23
		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			this.DrawParallaxHorizon(b, false);
		}

		// Token: 0x06002E21 RID: 11809 RVA: 0x00240C34 File Offset: 0x0023EE34
		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is WateringCan && base.isTileOnMap(new Vector2((float)tileX, (float)tileY)) && this.waterTiles[tileX, tileY])
			{
				for (int i = 0; i < 10; i++)
				{
					TemporaryAnimatedSprite s = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), new Vector2((float)tileX + 0.5f, (float)tileY + 0.5f) * 64f + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), false, 0.02f, Color.White)
					{
						scale = 3f,
						animationLength = 7,
						totalNumberOfLoops = 10,
						interval = 90f,
						motion = new Vector2((float)Game1.random.Next(-10, 11) / 8f, -3f),
						acceleration = new Vector2(0f, 0.08f),
						delayBeforeAnimationStart = i * 50
					};
					this.temporarySprites.Add(s);
				}
				for (int j = 0; j < 5; j++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2((float)tileX, (float)tileY - 0.5f) * 64f + new Vector2((float)Game1.random.Next(64), (float)Game1.random.Next(64)), false, 0.007f, Color.White)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -1f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 4f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
						delayBeforeAnimationStart = j * 35
					});
				}
				DelayedAction.playSoundAfterDelay("fireball", 200, null, null, -1, false);
				Game1.playSound("steam", null);
			}
			return base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x04001F88 RID: 8072
		[XmlIgnore]
		public Texture2D mapBaseTilesheet;

		// Token: 0x04001F89 RID: 8073
		[XmlElement("visited")]
		public NetBool visited = new NetBool();
	}
}
