using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Objects
{
	// Token: 0x020001A9 RID: 425
	public class CrabPot : Object
	{
		// Token: 0x06001E28 RID: 7720 RVA: 0x0015A33D File Offset: 0x0015853D
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.directionOffset, "directionOffset").AddField(this.bait, "bait");
		}

		// Token: 0x06001E29 RID: 7721 RVA: 0x0015A36C File Offset: 0x0015856C
		public CrabPot() : base("710", 1, false, -1, 0)
		{
			base.CanBeGrabbed = false;
			this.type.Value = "interactive";
			this.tileIndexToShow = base.ParentSheetIndex;
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x0015A3C1 File Offset: 0x001585C1
		public bool NeedsBait(Farmer player)
		{
			return this.bait.Value == null && !(Game1.GetPlayer(this.owner.Value, false) ?? (player ?? Game1.player)).professions.Contains(11);
		}

		// Token: 0x06001E2B RID: 7723 RVA: 0x0015A400 File Offset: 0x00158600
		public List<Vector2> getOverlayTiles()
		{
			List<Vector2> tiles = new List<Vector2>();
			if (this.Location != null)
			{
				if (this.directionOffset.Y < 0f)
				{
					this.addOverlayTilesIfNecessary((int)this.TileLocation.X, (int)this.tileLocation.Y, tiles);
				}
				this.addOverlayTilesIfNecessary((int)this.TileLocation.X, (int)this.tileLocation.Y + 1, tiles);
				if (this.directionOffset.X < 0f)
				{
					this.addOverlayTilesIfNecessary((int)this.TileLocation.X - 1, (int)this.tileLocation.Y + 1, tiles);
				}
				if (this.directionOffset.X > 0f)
				{
					this.addOverlayTilesIfNecessary((int)this.TileLocation.X + 1, (int)this.tileLocation.Y + 1, tiles);
				}
			}
			return tiles;
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x0015A4DC File Offset: 0x001586DC
		protected void addOverlayTilesIfNecessary(int tile_x, int tile_y, List<Vector2> tiles)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (location != Game1.currentLocation)
			{
				return;
			}
			if (location.hasTileAt(tile_x, tile_y, "Buildings", null) && !location.isWaterTile(tile_x, tile_y + 1))
			{
				tiles.Add(new Vector2((float)tile_x, (float)tile_y));
			}
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x0015A528 File Offset: 0x00158728
		public void addOverlayTiles()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (location != Game1.currentLocation)
			{
				return;
			}
			foreach (Vector2 tile in this.getOverlayTiles())
			{
				int count;
				if (!Game1.crabPotOverlayTiles.TryGetValue(tile, out count))
				{
					count = (Game1.crabPotOverlayTiles[tile] = 0);
				}
				Game1.crabPotOverlayTiles[tile] = count + 1;
			}
		}

		// Token: 0x06001E2E RID: 7726 RVA: 0x0015A5B4 File Offset: 0x001587B4
		public void removeOverlayTiles()
		{
			if (this.Location == null)
			{
				return;
			}
			if (this.Location != Game1.currentLocation)
			{
				return;
			}
			foreach (Vector2 tile in this.getOverlayTiles())
			{
				int count;
				if (Game1.crabPotOverlayTiles.TryGetValue(tile, out count))
				{
					count--;
					if (count <= 0)
					{
						Game1.crabPotOverlayTiles.Remove(tile);
					}
					else
					{
						Game1.crabPotOverlayTiles[tile] = count;
					}
				}
			}
		}

		// Token: 0x06001E2F RID: 7727 RVA: 0x0015A648 File Offset: 0x00158848
		public static bool IsValidCrabPotLocationTile(GameLocation location, int x, int y)
		{
			if (location is Caldera || location is VolcanoDungeon || location is MineShaft)
			{
				return false;
			}
			Vector2 placement_tile = new Vector2((float)x, (float)y);
			bool neighbor_check = (location.isWaterTile(x + 1, y) && location.isWaterTile(x - 1, y)) || (location.isWaterTile(x, y + 1) && location.isWaterTile(x, y - 1));
			return !location.objects.ContainsKey(placement_tile) && neighbor_check && location.isWaterTile((int)placement_tile.X, (int)placement_tile.Y) && location.doesTileHaveProperty((int)placement_tile.X, (int)placement_tile.Y, "Passable", "Buildings", false) == null;
		}

		// Token: 0x06001E30 RID: 7728 RVA: 0x0015A6FA File Offset: 0x001588FA
		public override void actionOnPlayerEntry()
		{
			this.updateOffset();
			this.addOverlayTiles();
			base.actionOnPlayerEntry();
		}

		// Token: 0x06001E31 RID: 7729 RVA: 0x0015A710 File Offset: 0x00158910
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
			if (who != null)
			{
				this.owner.Value = who.UniqueMultiplayerID;
			}
			if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
			{
				return false;
			}
			this.TileLocation = placementTile;
			location.objects.Add(this.tileLocation.Value, this);
			location.playSound("waterSlosh", null, null, SoundContext.Default);
			DelayedAction.playSoundAfterDelay("slosh", 150, null, null, -1, false);
			this.updateOffset();
			this.addOverlayTiles();
			return true;
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x0015A7C0 File Offset: 0x001589C0
		public void updateOffset()
		{
			Vector2 offset = Vector2.Zero;
			if (this.checkLocation(this.tileLocation.X - 1f, this.tileLocation.Y))
			{
				offset += new Vector2(32f, 0f);
			}
			if (this.checkLocation(this.tileLocation.X + 1f, this.tileLocation.Y))
			{
				offset += new Vector2(-32f, 0f);
			}
			if (offset.X != 0f && this.checkLocation(this.tileLocation.X + (float)Math.Sign(offset.X), this.tileLocation.Y + 1f))
			{
				offset += new Vector2(0f, -42f);
			}
			if (this.checkLocation(this.tileLocation.X, this.tileLocation.Y - 1f))
			{
				offset += new Vector2(0f, 32f);
			}
			if (this.checkLocation(this.tileLocation.X, this.tileLocation.Y + 1f))
			{
				offset += new Vector2(0f, -42f);
			}
			this.directionOffset.Value = offset;
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x0015A91C File Offset: 0x00158B1C
		protected bool checkLocation(float tile_x, float tile_y)
		{
			GameLocation location = this.Location;
			return !location.isWaterTile((int)tile_x, (int)tile_y) || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings", false) != null;
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x0015A956 File Offset: 0x00158B56
		protected override Item GetOneNew()
		{
			return new Object(base.ItemId, 1, false, -1, 0);
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x0015A968 File Offset: 0x00158B68
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			Object dropIn = dropInItem as Object;
			if (dropIn != null && dropIn.Category == -21 && this.NeedsBait(who))
			{
				if (!probe)
				{
					if (who != null)
					{
						this.owner.Value = who.UniqueMultiplayerID;
					}
					this.bait.Value = (dropIn.getOne() as Object);
					location.playSound("Ship", null, null, SoundContext.Default);
					this.lidFlapping = true;
					this.lidFlapTimer = 60f;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x0015AA00 File Offset: 0x00158C00
		public override bool AttemptAutoLoad(IInventory inventory, Farmer who)
		{
			Object prevBait = this.bait.Value;
			if (base.AttemptAutoLoad(inventory, who) && prevBait != this.bait.Value)
			{
				inventory.ReduceId(this.bait.Value.QualifiedItemId, this.bait.Value.Stack);
				return true;
			}
			return false;
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x0015AA5C File Offset: 0x00158C5C
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (this.tileIndexToShow != 714)
			{
				if (this.bait.Value == null && this.ignoreRemovalTimer <= 0)
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					if (Game1.didPlayerJustClickAtAll(true))
					{
						if (Game1.player.addItemToInventoryBool(base.getOne(), false))
						{
							if (who.isMoving())
							{
								Game1.haltAfterCheck = false;
							}
							Game1.playSound("coin", null);
							location.objects.Remove(this.tileLocation.Value);
							return true;
						}
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					}
				}
				return false;
			}
			if (justCheckingForActivity)
			{
				return true;
			}
			Object item = this.heldObject.Value;
			if (item != null)
			{
				int numToCatch = item.Stack;
				if (Utility.CreateDaySaveRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 77U, (double)(this.tileLocation.X * 777f + this.tileLocation.Y)).NextDouble() < 0.25 && Game1.player.stats.Get("Book_Crabbing") > 0U && who.couldInventoryAcceptThisItem(item.QualifiedItemId, numToCatch * 2, item.Quality))
				{
					numToCatch *= 2;
				}
				item.Stack = numToCatch;
				this.heldObject.Value = null;
				if (who.IsLocalPlayer && !who.addItemToInventoryBool(item, false))
				{
					this.heldObject.Value = item;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					return false;
				}
				string rawDataStr;
				if (DataLoader.Fish(Game1.content).TryGetValue(item.ItemId, out rawDataStr))
				{
					string[] rawData = rawDataStr.Split('/', StringSplitOptions.None);
					int minFishSize = (rawData.Length > 5) ? Convert.ToInt32(rawData[5]) : 1;
					int maxFishSize = (rawData.Length > 5) ? Convert.ToInt32(rawData[6]) : 10;
					who.caughtFish(item.QualifiedItemId, Game1.random.Next(minFishSize, maxFishSize + 1), false, numToCatch);
				}
				who.gainExperience(1, 5);
			}
			this.readyForHarvest.Value = false;
			this.tileIndexToShow = 710;
			this.lidFlapping = true;
			this.lidFlapTimer = 60f;
			this.bait.Value = null;
			who.animateOnce(279 + who.FacingDirection);
			location.playSound("fishingRodBend", null, null, SoundContext.Default);
			DelayedAction.playSoundAfterDelay("coin", 500, null, null, -1, false);
			this.shake = Vector2.Zero;
			this.shakeTimer = 0f;
			this.ignoreRemovalTimer = 750;
			return true;
		}

		// Token: 0x06001E38 RID: 7736 RVA: 0x0015AD0A File Offset: 0x00158F0A
		public override void performRemoveAction()
		{
			this.removeOverlayTiles();
			base.performRemoveAction();
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x0015AD18 File Offset: 0x00158F18
		public override void DayUpdate()
		{
			GameLocation location = this.Location;
			Farmer ownedByPlayer = Game1.GetPlayer(this.owner.Value, false) ?? Game1.MasterPlayer;
			bool isMariner = ownedByPlayer.professions.Contains(10);
			if (!this.NeedsBait(ownedByPlayer) && this.heldObject.Value == null)
			{
				this.tileIndexToShow = 714;
				this.readyForHarvest.Value = true;
				Random r = Utility.CreateDaySaveRandom((double)(this.tileLocation.X * 1000f), (double)(this.tileLocation.Y * 255f), (double)(this.directionOffset.X * 1000f + this.directionOffset.Y));
				List<string> marinerList = new List<string>();
				string a;
				FishAreaData fishArea;
				if (!location.TryGetFishAreaForTile(this.tileLocation.Value, out a, out fishArea))
				{
					fishArea = null;
				}
				double num2;
				if (!isMariner)
				{
					float? num = (fishArea != null) ? new float?(fishArea.CrabPotJunkChance) : null;
					num2 = ((num != null) ? ((double)num.GetValueOrDefault()) : 0.2);
				}
				else
				{
					num2 = 0.0;
				}
				double chanceForJunk = num2;
				int quantity = 1;
				int quality = 0;
				string baitTargetFish = null;
				Object value = this.bait.Value;
				a = ((value != null) ? value.QualifiedItemId : null);
				if (!(a == "(O)DeluxeBait"))
				{
					if (!(a == "(O)774"))
					{
						if (a == "(O)SpecificBait")
						{
							if (this.bait.Value.preservedParentSheetIndex.Value != null && this.bait.Value.preserve.Value != null)
							{
								baitTargetFish = this.bait.Value.preservedParentSheetIndex.Value;
								chanceForJunk /= 2.0;
							}
						}
					}
					else
					{
						chanceForJunk /= 2.0;
						if (r.NextBool(0.25))
						{
							quantity = 2;
						}
					}
				}
				else
				{
					quality = 1;
					chanceForJunk /= 2.0;
				}
				if (!r.NextBool(chanceForJunk))
				{
					IList<string> targetAreas = location.GetCrabPotFishForTile(this.tileLocation.Value);
					foreach (KeyValuePair<string, string> v in DataLoader.Fish(Game1.content))
					{
						if (v.Value.Contains("trap"))
						{
							string[] rawSplit = v.Value.Split('/', StringSplitOptions.None);
							string[] array = ArgUtility.SplitBySpace(rawSplit[4]);
							bool found = false;
							foreach (string crabPotArea in array)
							{
								foreach (string targetArea in targetAreas)
								{
									if (crabPotArea == targetArea)
									{
										found = true;
										break;
									}
								}
							}
							if (found)
							{
								if (isMariner)
								{
									marinerList.Add(v.Key);
								}
								else
								{
									double chanceForCatch = Convert.ToDouble(rawSplit[2]);
									if (baitTargetFish != null && baitTargetFish == v.Key)
									{
										chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 : ((chanceForCatch < 0.2) ? 3 : 2));
									}
									if (r.NextDouble() < chanceForCatch)
									{
										this.heldObject.Value = ItemRegistry.Create<Object>("(O)" + v.Key, quantity, quality, false);
										break;
									}
								}
							}
						}
					}
				}
				if (this.heldObject.Value == null)
				{
					if (isMariner && marinerList.Count > 0)
					{
						this.heldObject.Value = ItemRegistry.Create<Object>("(O)" + r.ChooseFrom(marinerList), quantity, quality, false);
						return;
					}
					NetFieldBase<Object, NetRef<Object>> heldObject = this.heldObject;
					string str = "(O)";
					int i = r.Next(168, 173);
					heldObject.Value = ItemRegistry.Create<Object>(str + i.ToString(), 1, 0, false);
				}
			}
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x0015B148 File Offset: 0x00159348
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.lidFlapping)
			{
				this.lidFlapTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.lidFlapTimer <= 0f)
				{
					this.tileIndexToShow += (this.lidClosing ? -1 : 1);
					if (this.tileIndexToShow >= 713 && !this.lidClosing)
					{
						this.lidClosing = true;
						this.tileIndexToShow--;
					}
					else if (this.tileIndexToShow <= 709 && this.lidClosing)
					{
						this.lidClosing = false;
						this.tileIndexToShow++;
						this.lidFlapping = false;
						if (this.bait.Value != null)
						{
							this.tileIndexToShow = 713;
						}
					}
					this.lidFlapTimer = 60f;
				}
			}
			if (this.readyForHarvest.Value && this.heldObject.Value != null)
			{
				this.shakeTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.shakeTimer < 0f)
				{
					this.shakeTimer = (float)Game1.random.Next(2800, 3200);
				}
			}
			if (this.shakeTimer > 2000f)
			{
				this.shake.X = (float)Game1.random.Next(-1, 2);
			}
			else
			{
				this.shake.X = 0f;
			}
			if (this.ignoreRemovalTimer > 0)
			{
				this.ignoreRemovalTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x0015B2E0 File Offset: 0x001594E0
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			if (this.heldObject.Value != null)
			{
				this.tileIndexToShow = 714;
			}
			else if (this.tileIndexToShow == 0)
			{
				this.tileIndexToShow = base.ParentSheetIndex;
			}
			this.yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(x * 64)) * 8.0 + 8.0);
			if (this.yBob <= 0.001f)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, this.directionOffset.Value + new Vector2((float)(x * 64 + 4), (float)(y * 64 + 32)), false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2((float)(x * 64), (float)(y * 64 + (int)this.yBob))) + this.shake, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.tileIndexToShow, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 10000f);
			if (location.waterTiles != null && x < location.waterTiles.waterTiles.GetLength(0) && y < location.waterTiles.waterTiles.GetLength(1) && location.waterTiles.waterTiles[x, y].isWater)
			{
				if (location.waterTiles.waterTiles[x, y].isVisible)
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2((float)(x * 64 + 4), (float)(y * 64 + 48))) + this.shake, new Rectangle?(new Rectangle(location.waterAnimationIndex * 64, 2112 + (((x + y) % 2 == 0) ? (location.waterTileFlip ? 128 : 0) : (location.waterTileFlip ? 0 : 128)), 56, 16 + (int)this.yBob)), location.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 9999f);
				}
				else
				{
					Color water_color = new Color(135, 135, 135, 215);
					water_color = Utility.MultiplyColor(water_color, location.waterColor.Value);
					spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2((float)(x * 64 + 4), (float)(y * 64 + 48))) + this.shake, null, water_color, 0f, Vector2.Zero, new Vector2(56f, (float)(16 + (int)this.yBob)), SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 9999f);
				}
			}
			if (this.readyForHarvest.Value && this.heldObject.Value != null)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2((float)(x * 64 - 8), (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + this.tileLocation.X / 10000f);
				ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(this.heldObject.Value.QualifiedItemId);
				spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + yOffset)), new Rectangle?(heldItemData.GetSourceRect(0, null)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + this.tileLocation.X / 10000f);
			}
		}

		// Token: 0x04001289 RID: 4745
		public const int lidFlapTimerInterval = 60;

		// Token: 0x0400128A RID: 4746
		[XmlIgnore]
		public float yBob;

		// Token: 0x0400128B RID: 4747
		[XmlElement("directionOffset")]
		public readonly NetVector2 directionOffset = new NetVector2();

		// Token: 0x0400128C RID: 4748
		[XmlElement("bait")]
		public readonly NetRef<Object> bait = new NetRef<Object>();

		// Token: 0x0400128D RID: 4749
		public int tileIndexToShow;

		// Token: 0x0400128E RID: 4750
		[XmlIgnore]
		public bool lidFlapping;

		// Token: 0x0400128F RID: 4751
		[XmlIgnore]
		public bool lidClosing;

		// Token: 0x04001290 RID: 4752
		[XmlIgnore]
		public float lidFlapTimer;

		// Token: 0x04001291 RID: 4753
		[XmlIgnore]
		public new float shakeTimer;

		// Token: 0x04001292 RID: 4754
		[XmlIgnore]
		public Vector2 shake;

		// Token: 0x04001293 RID: 4755
		[XmlIgnore]
		private int ignoreRemovalTimer;
	}
}
