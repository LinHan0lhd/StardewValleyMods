using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E1 RID: 737
	public class IslandWest : IslandLocation
	{
		// Token: 0x060030AC RID: 12460 RVA: 0x0026717C File Offset: 0x0026537C
		public override void SetBuriedNutLocations()
		{
			this.buriedNutPoints.Add(new Point(21, 81));
			this.buriedNutPoints.Add(new Point(62, 76));
			this.buriedNutPoints.Add(new Point(39, 24));
			this.buriedNutPoints.Add(new Point(88, 14));
			this.buriedNutPoints.Add(new Point(43, 74));
			this.buriedNutPoints.Add(new Point(30, 75));
			base.SetBuriedNutLocations();
		}

		// Token: 0x060030AD RID: 12461 RVA: 0x00267207 File Offset: 0x00265407
		public override bool CanPlantSeedsHere(string itemId, int tileX, int tileY, bool isGardenPot, out string deniedMessage)
		{
			if (base.getTileSheetIDAt(tileX, tileY, "Back") != "untitled tile sheet2")
			{
				deniedMessage = null;
				return false;
			}
			return base.CanPlantSeedsHere(itemId, tileX, tileY, isGardenPot, out deniedMessage);
		}

		// Token: 0x060030AE RID: 12462 RVA: 0x00267238 File Offset: 0x00265438
		public override bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
		{
			if (base.getTileSheetIDAt(tileX, tileY, "Back") == "untitled tile sheet2" || Object.isWildTreeSeed(itemId))
			{
				string s = this.doesTileHavePropertyNoNull(tileX, tileY, "Type", "Back");
				if (s == "Dirt" || s == "Grass" || s == "")
				{
					return base.CheckItemPlantRules(itemId, false, true, out deniedMessage);
				}
			}
			return base.CanPlantTreesHere(itemId, tileX, tileY, out deniedMessage);
		}

		// Token: 0x060030AF RID: 12463 RVA: 0x002672B8 File Offset: 0x002654B8
		public IslandWest()
		{
		}

		// Token: 0x060030B0 RID: 12464 RVA: 0x00267326 File Offset: 0x00265526
		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			SandDuggy value = this.sandDuggy.Value;
			if (value != null)
			{
				value.PerformToolAction(t, tileX, tileY);
			}
			return base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x060030B1 RID: 12465 RVA: 0x0026734C File Offset: 0x0026554C
		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			return new List<Vector2>
			{
				new Vector2(54f, 18f),
				new Vector2(25f, 30f),
				new Vector2(15f, 3f)
			};
		}

		// Token: 0x060030B2 RID: 12466 RVA: 0x002673A0 File Offset: 0x002655A0
		public override void draw(SpriteBatch b)
		{
			SandDuggy value = this.sandDuggy.Value;
			if (value != null)
			{
				value.Draw(b);
			}
			if (this.farmhouseRestored.Value)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = this.shippingBinLid;
				if (temporaryAnimatedSprite != null)
				{
					temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
				}
			}
			if (this.farmhouseMailbox.Value && Game1.mailbox.Count > 0)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point mailbox_position = new Point(81, 40);
				float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
				float xOffset = -8f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64) + xOffset, (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64 + 32 + 4) + xOffset, (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13)), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
			}
			base.draw(b);
		}

		// Token: 0x060030B3 RID: 12467 RVA: 0x0026756C File Offset: 0x0026576C
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			SandDuggy value = this.sandDuggy.Value;
			if (value != null)
			{
				value.Update(time);
			}
			if (this.farmhouseRestored.Value && this.shippingBinLid != null)
			{
				bool opening = false;
				using (FarmerCollection.Enumerator enumerator = this.farmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.GetBoundingBox().Intersects(this.shippingBinLidOpenArea))
						{
							this.openShippingBinLid();
							opening = true;
						}
					}
				}
				if (!opening)
				{
					this.closeShippingBinLid();
				}
				this.updateShippingBinLid(time);
			}
			base.UpdateWhenCurrentLocation(time);
		}

		// Token: 0x060030B4 RID: 12468 RVA: 0x0026761C File Offset: 0x0026581C
		public IslandWest(string map, string name) : base(map, name)
		{
			this.sandDuggy.Value = new SandDuggy(this, new Point[]
			{
				new Point(37, 87),
				new Point(41, 86),
				new Point(45, 86),
				new Point(48, 87)
			});
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 37), new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 8), 20, delegate()
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)886", 1, 0, false), new Vector2(72f, 37f) * 64f + new Vector2(32f), 2, null, -1, false);
				Game1.addMailForTomorrow("Island_W_Obelisk", true, true);
				this.farmObelisk.Value = true;
			}, () => this.farmObelisk.Value, "Obelisk", "Island_UpgradeHouse_Mailbox"));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(80, 39, 3, 2), 5, delegate()
			{
				Game1.addMailForTomorrow("Island_UpgradeHouse_Mailbox", true, true);
				this.farmhouseMailbox.Value = true;
			}, () => this.farmhouseMailbox.Value, "House_Mailbox", "Island_UpgradeHouse"));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(74, 36, 7, 4), 20, delegate()
			{
				Game1.addMailForTomorrow("Island_UpgradeHouse", true, true);
				this.farmhouseRestored.Value = true;
			}, () => this.farmhouseRestored.Value, "House", ""));
			this.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 10), new Microsoft.Xna.Framework.Rectangle(73, 5, 3, 5), 10, delegate()
			{
				Game1.addMailForTomorrow("Island_UpgradeParrotPlatform", true, true);
				Game1.netWorldState.Value.ParrotPlatformsUnlocked = true;
			}, () => Game1.netWorldState.Value.ParrotPlatformsUnlocked, "ParrotPlatforms", ""));
		}

		// Token: 0x060030B5 RID: 12469 RVA: 0x0026782C File Offset: 0x00265A2C
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "FarmObelisk")
			{
				for (int i = 0; i < 12; i++)
				{
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), (float)Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), false, Game1.random.NextBool()));
				}
				who.currentLocation.playSound("wand", null, null, SoundContext.Default);
				Game1.displayFarmer = false;
				Game1.player.temporarilyInvincible = true;
				Game1.player.temporaryInvincibilityTimer = -2000;
				Game1.player.freezePause = 1000;
				Game1.flashAlpha = 1f;
				DelayedAction.fadeAfterDelay(delegate
				{
					Point warp_location;
					if (!Game1.getFarm().TryGetMapPropertyAs("WarpTotemEntry", out warp_location, false))
					{
						int whichFarm = Game1.whichFarm;
						if (whichFarm != 5)
						{
							if (whichFarm == 6)
							{
								warp_location = new Point(82, 29);
							}
							else
							{
								warp_location = new Point(48, 7);
							}
						}
						else
						{
							warp_location = new Point(48, 39);
						}
					}
					Game1.warpFarmer("Farm", warp_location.X, warp_location.Y, false);
					Game1.fadeToBlackAlpha = 0.99f;
					Game1.screenGlow = false;
					Game1.player.temporarilyInvincible = false;
					Game1.player.temporaryInvincibilityTimer = 0;
					Game1.displayFarmer = true;
				}, 1000);
				Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
				Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64);
				r.Inflate(192, 192);
				int j = 0;
				Point playerTile = who.TilePoint;
				for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
				{
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2((float)x, (float)playerTile.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = j * 25,
						motion = new Vector2(-0.25f, 0f)
					});
					j++;
				}
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x060030B6 RID: 12470 RVA: 0x00267A60 File Offset: 0x00265C60
		public override bool leftClick(int x, int y, Farmer who)
		{
			if (this.farmhouseRestored.Value)
			{
				Item item = who.ActiveItem;
				bool? flag = (item != null) ? new bool?(item.canBeShipped()) : null;
				if (flag != null && flag.GetValueOrDefault() && x / 64 >= this.shippingBinPosition.X && x / 64 <= this.shippingBinPosition.X + 1 && y / 64 >= this.shippingBinPosition.Y - 1 && y / 64 <= this.shippingBinPosition.Y && Vector2.Distance(who.Tile, new Vector2((float)this.shippingBinPosition.X + 0.5f, (float)this.shippingBinPosition.Y)) <= 2f)
				{
					Farm farm = Game1.getFarm();
					farm.getShippingBin(who).Add(item);
					farm.lastItemShipped = item;
					who.showNotCarrying();
					this.showShipment(item, true);
					who.ActiveItem = null;
					return true;
				}
			}
			return base.leftClick(x, y, who);
		}

		// Token: 0x060030B7 RID: 12471 RVA: 0x00267B74 File Offset: 0x00265D74
		public void showShipment(Item item, bool playThrowSound = true)
		{
			if (playThrowSound)
			{
				base.localSound("backpackIN", null, null, SoundContext.Default);
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0, null, null, -1, false);
			int id = Game1.random.Next();
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(90f, 38f) * 64f + new Vector2(0f, 5f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.25601003f,
				id = id,
				extraInfoForEndBehavior = id,
				endFunction = new TemporaryAnimatedSprite.endBehavior(base.removeTemporarySpritesWithID)
			});
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(90f, 38f) * 64f + new Vector2(0f, 17f) * 4f, false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.2563f,
				id = id,
				extraInfoForEndBehavior = id
			});
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
			ColoredObject coloredObj = item as ColoredObject;
			Vector2 initialPosition = new Vector2(90f, 38f) * 64f + new Vector2((float)(8 + Game1.random.Next(6)), 2f) * 4f;
			foreach (bool isColorOverlay in new bool[]
			{
				default(bool),
				true
			})
			{
				if (!isColorOverlay || (coloredObj != null && !coloredObj.ColorSameIndexAsParentSheetIndex))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect((isColorOverlay > false) ? 1 : 0, null), initialPosition, false, 0f, Color.White)
					{
						interval = 9999f,
						scale = 4f,
						alphaFade = 0.045f,
						layerDepth = 0.25622502f,
						motion = new Vector2(0f, 0.3f),
						acceleration = new Vector2(0f, 0.2f),
						scaleChange = -0.05f,
						color = ((coloredObj != null) ? coloredObj.color.Value : Color.White)
					});
				}
			}
		}

		// Token: 0x060030B8 RID: 12472 RVA: 0x00267E88 File Offset: 0x00266088
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (this.farmhouseRestored.Value && tileLocation.X >= this.shippingBinPosition.X && tileLocation.X <= this.shippingBinPosition.X + 1 && tileLocation.Y >= this.shippingBinPosition.Y - 1 && tileLocation.Y <= this.shippingBinPosition.Y)
			{
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightShippableObjects), new ItemGrabMenu.behaviorOnItemSelect(Game1.getFarm().shipItem), "", null, true, true, false, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
				itemGrabMenu.initializeUpperRightCloseButton();
				itemGrabMenu.setBackgroundTransparency(false);
				itemGrabMenu.setDestroyItemOnClick(true);
				itemGrabMenu.initializeShippingBin();
				Game1.activeClickableMenu = itemGrabMenu;
				base.playSound("shwip", null, null, SoundContext.Default);
				if (Game1.player.FacingDirection == 1)
				{
					Game1.player.Halt();
				}
				Game1.player.showCarrying();
				return true;
			}
			if (base.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "untitled tile sheet") == 1470)
			{
				int actualFoundWalnutsCount;
				if (!IslandWest.IsQiWalnutRoomDoorUnlocked(out actualFoundWalnutsCount))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:qiNutDoor", actualFoundWalnutsCount));
				}
				else
				{
					Game1.playSound("doorClose", null);
					Game1.warpFarmer("QiNutRoom", 7, 8, 0);
				}
				return true;
			}
			NPC birdie = base.getCharacterFromName("Birdie");
			if (birdie != null && !birdie.IsInvisible && (birdie.Tile == new Vector2((float)tileLocation.X, (float)tileLocation.Y) || birdie.Tile == new Vector2((float)(tileLocation.X - 1), (float)tileLocation.Y)))
			{
				if (who.mailReceived.Add("birdieQuestBegun"))
				{
					who.Halt();
					Game1.globalFadeToBlack(delegate
					{
						this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro"), null, "-888999", null));
					}, 0.02f);
					return true;
				}
				if (who.hasQuest("130"))
				{
					Object activeObject = who.ActiveObject;
					if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)870" && who.mailReceived.Add("birdieQuestFinished"))
					{
						who.Halt();
						Game1.globalFadeToBlack(delegate
						{
							who.reduceActiveItemByOne();
							this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished"), null, "-666777", null));
						}, 0.02f);
						return true;
					}
				}
				if (who.mailReceived.Contains("birdieQuestFinished"))
				{
					if (who.ActiveObject != null)
					{
						Game1.DrawDialogue(birdie, "Data\\ExtraDialogue:Birdie_NoGift");
					}
					else
					{
						Dialogue possible = Dialogue.TryGetDialogue(birdie, "Data\\ExtraDialogue:Birdie" + Game1.dayOfMonth.ToString());
						if (possible != null)
						{
							Game1.DrawDialogue(possible);
						}
						else
						{
							Game1.DrawDialogue(birdie, "Data\\ExtraDialogue:Birdie" + (Game1.dayOfMonth % 7).ToString());
						}
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x060030B9 RID: 12473 RVA: 0x002681A9 File Offset: 0x002663A9
		public static bool IsQiWalnutRoomDoorUnlocked(out int actualFoundWalnutsCount)
		{
			actualFoundWalnutsCount = Math.Max(0, Game1.netWorldState.Value.GoldenWalnutsFound - 1);
			return actualFoundWalnutsCount >= 100;
		}

		// Token: 0x060030BA RID: 12474 RVA: 0x002681D0 File Offset: 0x002663D0
		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			if (!Game1.eventUp)
			{
				NPC birdie = base.getCharacterFromName("Birdie");
				if (birdie != null && !birdie.IsInvisible && birdie.Tile == new Vector2((float)(xTile - 1), (float)yTile) && (!who.mailReceived.Contains("birdieQuestBegun") || who.mailReceived.Contains("birdieQuestFinished")))
				{
					Game1.isSpeechAtCurrentCursorTile = true;
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		// Token: 0x060030BB RID: 12475 RVA: 0x00268248 File Offset: 0x00266448
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.addedSlimesToday, "addedSlimesToday").AddField(this.farmhouseRestored, "farmhouseRestored").AddField(this.sandDuggy, "sandDuggy").AddField(this.farmhouseMailbox, "farmhouseMailbox").AddField(this.farmObelisk, "farmObelisk");
			this.farmhouseRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyFarmHouseRestore();
				}
			};
			this.farmhouseMailbox.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyFarmHouseRestore();
				}
			};
			this.farmObelisk.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && this.mapPath.Value != null)
				{
					this.ApplyFarmObeliskBuild();
				}
			};
		}

		// Token: 0x060030BC RID: 12476 RVA: 0x002682F8 File Offset: 0x002664F8
		public void ApplyFarmObeliskBuild()
		{
			if (this.map != null && !this._appliedMapOverrides.Contains("Island_W_Obelisk"))
			{
				base.ApplyMapOverride("Island_W_Obelisk", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 9)));
			}
		}

		// Token: 0x060030BD RID: 12477 RVA: 0x00268344 File Offset: 0x00266544
		public void ApplyFarmHouseRestore()
		{
			if (this.map != null)
			{
				if (!this._appliedMapOverrides.Contains("Island_House_Restored"))
				{
					base.ApplyMapOverride("Island_House_Restored", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(74, 33, 7, 9)));
					base.ApplyMapOverride("Island_House_Bin", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.shippingBinPosition.X, this.shippingBinPosition.Y - 1, 2, 2)));
					base.ApplyMapOverride("Island_House_Cave", null, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(95, 30, 3, 4)));
				}
				if (this.farmhouseMailbox.Value)
				{
					base.setMapTile(81, 40, 771, "Buildings", "untitled tile sheet", "Mailbox", true);
					base.setMapTile(81, 39, 739, "Front", "untitled tile sheet", null, true);
				}
			}
		}

		// Token: 0x060030BE RID: 12478 RVA: 0x0026843C File Offset: 0x0026663C
		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			base.monsterDrop(monster, x, y, who);
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("tigerSlimeNut"))
			{
				int numTigerSlimes = 0;
				foreach (NPC i in this.characters)
				{
					if (i is GreenSlime && i.name.Value == "Tiger Slime")
					{
						numTigerSlimes++;
					}
				}
				if (numTigerSlimes == 1)
				{
					Game1.addMailForTomorrow("tigerSlimeNut", true, true);
					Game1.player.team.RequestLimitedNutDrops("TigerSlimeNut", this, x, y, 1, 1);
				}
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				long farmerId = (who != null) ? who.UniqueMultiplayerID : 0L;
				Game1.createObjectDebris("(O)826", x, y, farmerId, this);
			}
		}

		// Token: 0x060030BF RID: 12479 RVA: 0x00268528 File Offset: 0x00266728
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			IslandWest location = l as IslandWest;
			if (location != null)
			{
				this.farmhouseRestored.Value = location.farmhouseRestored.Value;
				this.farmhouseMailbox.Value = location.farmhouseMailbox.Value;
				this.farmObelisk.Value = location.farmObelisk.Value;
				this.sandDuggy.Value.whacked.Value = location.sandDuggy.Value.whacked.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x060030C0 RID: 12480 RVA: 0x002685B4 File Offset: 0x002667B4
		public override void spawnObjects()
		{
			base.spawnObjects();
			Microsoft.Xna.Framework.Rectangle musselNodeSpawnArea = new Microsoft.Xna.Framework.Rectangle(57, 78, 43, 8);
			if (Utility.getNumObjectsOfIndexWithinRectangle(musselNodeSpawnArea, new string[]
			{
				"(O)25"
			}, this) < 10)
			{
				Vector2 spawn = Utility.getRandomPositionInThisRectangle(musselNodeSpawnArea, Game1.random);
				if (this.CanItemBePlacedHere(spawn, false, CollisionMask.All, CollisionMask.None, false, false))
				{
					this.objects.Add(spawn, new Object("25", 1, false, -1, 0)
					{
						MinutesUntilReady = 8,
						Flipped = Game1.random.NextBool()
					});
				}
			}
			Microsoft.Xna.Framework.Rectangle tidePoolsArea = new Microsoft.Xna.Framework.Rectangle(20, 71, 28, 16);
			if (Utility.getNumObjectsOfIndexWithinRectangle(tidePoolsArea, new string[]
			{
				"(O)393",
				"(O)397"
			}, this) < 5)
			{
				Vector2 spawn2 = Utility.getRandomPositionInThisRectangle(tidePoolsArea, Game1.random);
				if (this.CanItemBePlacedHere(spawn2, false, CollisionMask.All, CollisionMask.None, false, false))
				{
					Object obj = ItemRegistry.Create<Object>((Game1.random.NextDouble() < 0.1) ? "(O)397" : "(O)393", 1, 0, false);
					obj.IsSpawnedObject = true;
					obj.CanBeGrabbed = true;
					this.objects.Add(spawn2, obj);
				}
			}
		}

		// Token: 0x060030C1 RID: 12481 RVA: 0x002686D4 File Offset: 0x002668D4
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 18 && yLocation == 42 && who.secretNotesSeen.Contains(1004))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut", this, xLocation * 64, yLocation * 64, 1, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure"))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)877", 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, 1, null, -1, false);
					Game1.addMailForTomorrow("Island_W_BuriedTreasure", true, false);
				}
			}
			else if (xLocation == 104 && yLocation == 74 && who.secretNotesSeen.Contains(1006))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut2", this, xLocation * 64, yLocation * 64, 1, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure2"))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)797", 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, 1, null, -1, false);
					Game1.addMailForTomorrow("Island_W_BuriedTreasure2", true, false);
				}
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x060030C2 RID: 12482 RVA: 0x00268804 File Offset: 0x00266A04
		protected override bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
		{
			if (r.NextDouble() < ((stoneId == "25") ? 0.025 : 0.01))
			{
				long farmerId = (who != null) ? who.UniqueMultiplayerID : 0L;
				Game1.createObjectDebris("(O)826", x, y, farmerId, this);
			}
			return base.breakStone(stoneId, x, y, who, r);
		}

		// Token: 0x060030C3 RID: 12483 RVA: 0x00268868 File Offset: 0x00266A68
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.characters.RemoveWhere((NPC npc) => npc is Monster);
			this.addedSlimesToday.Value = false;
			this.terrainFeatures.RemoveWhere(delegate(KeyValuePair<Vector2, TerrainFeature> pair)
			{
				HoeDirt dirt = pair.Value as HoeDirt;
				return dirt != null && dirt.crop != null && dirt.crop.forageCrop.Value;
			});
			Microsoft.Xna.Framework.Rectangle[] gingerLocations = new Microsoft.Xna.Framework.Rectangle[]
			{
				new Microsoft.Xna.Framework.Rectangle(31, 43, 7, 6),
				new Microsoft.Xna.Framework.Rectangle(37, 62, 6, 5),
				new Microsoft.Xna.Framework.Rectangle(48, 42, 5, 4),
				new Microsoft.Xna.Framework.Rectangle(71, 12, 5, 4),
				new Microsoft.Xna.Framework.Rectangle(50, 59, 1, 1),
				new Microsoft.Xna.Framework.Rectangle(47, 64, 1, 1),
				new Microsoft.Xna.Framework.Rectangle(36, 58, 1, 1),
				new Microsoft.Xna.Framework.Rectangle(56, 48, 1, 1),
				new Microsoft.Xna.Framework.Rectangle(29, 46, 1, 1)
			};
			for (int i = 0; i < 5; i++)
			{
				Microsoft.Xna.Framework.Rectangle r = gingerLocations[Game1.random.Next(gingerLocations.Length)];
				Vector2 origin = new Vector2((float)Game1.random.Next(r.X, r.Right), (float)Game1.random.Next(r.Y, r.Bottom));
				foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16, 50))
				{
					string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back", false);
					if (!this.terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.35f))
					{
						HoeDirt d = new HoeDirt(0, new Crop(true, "2", (int)v.X, (int)v.Y, this));
						d.state.Value = 2;
						this.terrainFeatures.Add(v, d);
					}
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Island_Turtle"))
			{
				base.spawnWeedsAndStones(20, true, true);
				if (Game1.dayOfMonth % 7 == 1)
				{
					base.spawnWeedsAndStones(20, true, false);
				}
			}
		}

		// Token: 0x060030C4 RID: 12484 RVA: 0x00268AFC File Offset: 0x00266CFC
		public override double GetDirtDecayChance(Vector2 tile)
		{
			if (base.getTileSheetIDAt((int)tile.X, (int)tile.Y, "Back") != "untitled tile sheet2")
			{
				return 1.0;
			}
			return base.GetDirtDecayChance(tile);
		}

		// Token: 0x060030C5 RID: 12485 RVA: 0x00268B34 File Offset: 0x00266D34
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (this.farmhouseRestored.Value)
			{
				this.ApplyFarmHouseRestore();
			}
			if (this.farmObelisk.Value)
			{
				this.ApplyFarmObeliskBuild();
			}
		}

		// Token: 0x060030C6 RID: 12486 RVA: 0x00268B64 File Offset: 0x00266D64
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle((this.shippingBinPosition.X - 1) * 64, (this.shippingBinPosition.Y - 1) * 64, 256, 192);
			this.shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(134, 226, 30, 25), new Vector2((float)this.shippingBinPosition.X, (float)(this.shippingBinPosition.Y - 1)) * 64f + new Vector2(2f, -7f) * 4f, false, 0f, Color.White)
			{
				holdLastFrame = true,
				destroyable = false,
				interval = 20f,
				animationLength = 13,
				paused = true,
				scale = 4f,
				layerDepth = (float)((this.shippingBinPosition.Y + 1) * 64) / 10000f + 0.0001f,
				pingPong = true,
				pingPongMotion = 0
			};
			SandDuggy value = this.sandDuggy.Value;
			if (value != null)
			{
				value.ResetForPlayerEntry();
			}
			NPC i = base.getCharacterFromName("Birdie");
			if (i != null)
			{
				if (i.Sprite.SourceRect.Width < 32)
				{
					i.extendSourceRect(16, 0, true);
				}
				i.Sprite.SpriteWidth = 32;
				i.Sprite.ignoreSourceRectUpdates = false;
				i.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(8, 1000, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(9, 1000, 0, false, false, null, false, 0)
				});
				i.Sprite.loop = true;
				i.HideShadow = true;
				i.IsInvisible = base.IsRainingHere();
			}
			if (Game1.timeOfDay > 1700)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(23f, 58f) * 64f + new Vector2(-16f, -32f), false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = "IslandWest_Birdie",
					id = 987654,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.37824f
				});
				AmbientLocationSounds.addSound(new Vector2(23f, 58f), 1);
			}
			if (base.AreMoonlightJelliesOut())
			{
				base.addMoonlightJellies(100, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0, 0.0, 0.0), new Microsoft.Xna.Framework.Rectangle(35, 0, 60, 60));
			}
		}

		// Token: 0x060030C7 RID: 12487 RVA: 0x00268E6C File Offset: 0x0026706C
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (!this.addedSlimesToday.Value)
			{
				this.addedSlimesToday.Value = true;
				Random rand = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 12.0, 0.0, 0.0);
				Microsoft.Xna.Framework.Rectangle spawnArea = new Microsoft.Xna.Framework.Rectangle(28, 24, 19, 8);
				for (int tries = 5; tries > 0; tries--)
				{
					Vector2 tile = Utility.getRandomPositionInThisRectangle(spawnArea, rand);
					if (this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						GreenSlime i = new GreenSlime(tile * 64f, 0);
						i.makeTigerSlime(false);
						this.characters.Add(i);
					}
				}
			}
		}

		// Token: 0x060030C8 RID: 12488 RVA: 0x00268F30 File Offset: 0x00267130
		private void openShippingBinLid()
		{
			if (this.shippingBinLid != null)
			{
				if (this.shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
				{
					base.localSound("doorCreak", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = 1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x060030C9 RID: 12489 RVA: 0x00268F94 File Offset: 0x00267194
		private void closeShippingBinLid()
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.shippingBinLid;
			if (temporaryAnimatedSprite != null && temporaryAnimatedSprite.currentParentTileIndex > 0)
			{
				if (this.shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
				{
					base.localSound("doorCreakReverse", null, null, SoundContext.Default);
				}
				this.shippingBinLid.pingPongMotion = -1;
				this.shippingBinLid.paused = false;
			}
		}

		// Token: 0x060030CA RID: 12490 RVA: 0x00269004 File Offset: 0x00267204
		private void updateShippingBinLid(GameTime time)
		{
			if (this.isShippingBinLidOpen(true) && this.shippingBinLid.pingPongMotion == 1)
			{
				this.shippingBinLid.paused = true;
			}
			else if (this.shippingBinLid.currentParentTileIndex == 0 && this.shippingBinLid.pingPongMotion == -1)
			{
				if (!this.shippingBinLid.paused && Game1.currentLocation == this)
				{
					base.localSound("woodyStep", null, null, SoundContext.Default);
				}
				this.shippingBinLid.paused = true;
			}
			this.shippingBinLid.update(time);
		}

		// Token: 0x060030CB RID: 12491 RVA: 0x0026909D File Offset: 0x0026729D
		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			return this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex >= (requiredToBeFullyOpen ? (this.shippingBinLid.animationLength - 1) : 1);
		}

		// Token: 0x040020C8 RID: 8392
		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();

		// Token: 0x040020C9 RID: 8393
		[XmlElement("sandDuggy")]
		public NetRef<SandDuggy> sandDuggy = new NetRef<SandDuggy>();

		// Token: 0x040020CA RID: 8394
		[XmlElement("farmhouseRestored")]
		public readonly NetBool farmhouseRestored = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x040020CB RID: 8395
		[XmlElement("farmhouseMailbox")]
		public readonly NetBool farmhouseMailbox = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x040020CC RID: 8396
		[XmlElement("farmObelisk")]
		public readonly NetBool farmObelisk = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x040020CD RID: 8397
		public Point shippingBinPosition = new Point(90, 39);

		// Token: 0x040020CE RID: 8398
		private TemporaryAnimatedSprite shippingBinLid;

		// Token: 0x040020CF RID: 8399
		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea;
	}
}
