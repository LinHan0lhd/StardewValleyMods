using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x02000274 RID: 628
	public class GeodeMenu : MenuWithInventory
	{
		// Token: 0x06002987 RID: 10631 RVA: 0x001EA0B8 File Offset: 0x001E82B8
		public GeodeMenu() : base(null, true, true, 12, 132, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			if (this.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			this.inventory.highlightMethod = new InventoryMenu.highlightThisItem(this.highlightGeodes);
			this.geodeSpot = new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "")
			{
				myID = 998,
				downNeighborID = 0
			};
			this.clint = new AnimatedSprite("Characters\\Clint", 8, 32, 48);
			List<ClickableComponent> inventory = this.inventory.inventory;
			if (inventory != null && inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (this.inventory.inventory[i] != null)
					{
						this.inventory.inventory[i].upNeighborID = 998;
					}
				}
			}
			if (this.trashCan != null)
			{
				this.trashCan.myID = 106;
			}
			if (this.okButton != null)
			{
				this.okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002988 RID: 10632 RVA: 0x001EA21F File Offset: 0x001E841F
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002989 RID: 10633 RVA: 0x001EA234 File Offset: 0x001E8434
		public override bool readyToClose()
		{
			return base.readyToClose() && this.geodeAnimationTimer <= 0 && base.heldItem == null && !this.waitingForServerResponse;
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x001EA25A File Offset: 0x001E845A
		public bool highlightGeodes(Item i)
		{
			return base.heldItem != null || Utility.IsGeode(i, false);
		}

		// Token: 0x0600298B RID: 10635 RVA: 0x001EA270 File Offset: 0x001E8470
		public virtual void startGeodeCrack()
		{
			this.geodeSpot.item = base.heldItem.getOne();
			base.heldItem = base.heldItem.ConsumeStack(1);
			this.geodeAnimationTimer = 2700;
			Game1.player.Money -= 25;
			Game1.playSound("stoneStep", null);
			this.clint.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(8, 300),
				new FarmerSprite.AnimationFrame(9, 200),
				new FarmerSprite.AnimationFrame(10, 80),
				new FarmerSprite.AnimationFrame(11, 200),
				new FarmerSprite.AnimationFrame(12, 100),
				new FarmerSprite.AnimationFrame(8, 300)
			});
			this.clint.loop = false;
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x001EA358 File Offset: 0x001E8558
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.waitingForServerResponse)
			{
				return;
			}
			base.receiveLeftClick(x, y, true);
			if (this.geodeSpot.containsPoint(x, y))
			{
				if (base.heldItem != null && Utility.IsGeode(base.heldItem, false) && Game1.player.Money >= 25 && this.geodeAnimationTimer <= 0)
				{
					int freeSpotsInInventory = Game1.player.freeSpotsInInventory();
					if (freeSpotsInInventory <= 1 && (freeSpotsInInventory != 1 || base.heldItem.Stack != 1))
					{
						this.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_InventoryFull");
						this.wiggleWordsTimer = 500;
						this.alertTimer = 1500;
						return;
					}
					if (base.heldItem.QualifiedItemId == "(O)791" && !Game1.netWorldState.Value.GoldenCoconutCracked)
					{
						this.waitingForServerResponse = true;
						Game1.player.team.goldenCoconutMutex.RequestLock(delegate
						{
							this.waitingForServerResponse = false;
							this.geodeTreasureOverride = ItemRegistry.Create("(O)73", 1, 0, false);
							this.startGeodeCrack();
						}, delegate
						{
							this.waitingForServerResponse = false;
							this.startGeodeCrack();
						});
						return;
					}
					this.startGeodeCrack();
					return;
				}
				else if (Game1.player.Money < 25)
				{
					this.wiggleWordsTimer = 500;
					Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
				}
			}
		}

		// Token: 0x0600298D RID: 10637 RVA: 0x001EA49A File Offset: 0x001E869A
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			base.receiveRightClick(x, y, true);
		}

		// Token: 0x0600298E RID: 10638 RVA: 0x001EA4A8 File Offset: 0x001E86A8
		public override void performHoverAction(int x, int y)
		{
			if (this.alertTimer <= 0)
			{
				base.performHoverAction(x, y);
				if (this.descriptionText.Equals(""))
				{
					if (Game1.player.Money < 25)
					{
						this.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description_NotEnoughMoney");
						return;
					}
					this.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description");
				}
			}
		}

		// Token: 0x0600298F RID: 10639 RVA: 0x001EA511 File Offset: 0x001E8711
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (base.heldItem != null)
			{
				Game1.player.addItemToInventoryBool(base.heldItem, false);
			}
		}

		// Token: 0x06002990 RID: 10640 RVA: 0x001EA534 File Offset: 0x001E8734
		public override void update(GameTime time)
		{
			base.update(time);
			this.fluffSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.alertTimer > 0)
			{
				this.alertTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.geodeAnimationTimer > 0)
			{
				Game1.MusicDuckTimer = 1500f;
				this.geodeAnimationTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.geodeAnimationTimer <= 0)
				{
					this.geodeDestructionAnimation = null;
					this.geodeSpot.item = null;
					Item item = this.geodeTreasure;
					if (((item != null) ? item.QualifiedItemId : null) == "(O)73")
					{
						Game1.netWorldState.Value.GoldenCoconutCracked = true;
					}
					Game1.player.addItemToInventoryBool(this.geodeTreasure, false);
					this.geodeTreasure = null;
					this.yPositionOfGem = 0;
					this.fluffSprites.Clear();
					this.delayBeforeShowArtifactTimer = 0f;
					return;
				}
				int frame = this.clint.currentFrame;
				this.clint.animateOnce(time);
				if (this.clint.currentFrame == 11 && frame != 11)
				{
					Item item2 = this.geodeSpot.item;
					if (!(((item2 != null) ? item2.QualifiedItemId : null) == "(O)275"))
					{
						Item item3 = this.geodeSpot.item;
						if (!(((item3 != null) ? item3.QualifiedItemId : null) == "(O)MysteryBox"))
						{
							Item item4 = this.geodeSpot.item;
							if (!(((item4 != null) ? item4.QualifiedItemId : null) == "(O)GoldenMysteryBox"))
							{
								Game1.playSound("hammer", null);
								Game1.playSound("stoneCrack", null);
								goto IL_208;
							}
						}
					}
					Game1.playSound("hammer", null);
					Game1.playSound("woodWhack", null);
					IL_208:
					Stats stats = Game1.stats;
					uint geodesCracked = stats.GeodesCracked;
					stats.GeodesCracked = geodesCracked + 1U;
					Item item5 = this.geodeSpot.item;
					if (!(((item5 != null) ? item5.QualifiedItemId : null) == "(O)MysteryBox"))
					{
						Item item6 = this.geodeSpot.item;
						if (!(((item6 != null) ? item6.QualifiedItemId : null) == "(O)GoldenMysteryBox"))
						{
							goto IL_275;
						}
					}
					Game1.stats.Increment("MysteryBoxesOpened", 1U);
					IL_275:
					int geodeDestructionYOffset = 448;
					if (this.geodeSpot.item != null)
					{
						string a = this.geodeSpot.item.QualifiedItemId;
						if (!(a == "(O)536"))
						{
							if (a == "(O)537")
							{
								geodeDestructionYOffset += 128;
							}
						}
						else
						{
							geodeDestructionYOffset += 64;
						}
						this.geodeDestructionAnimation = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, geodeDestructionYOffset, 64, 64), 100f, 8, 0, new Vector2((float)(this.geodeSpot.bounds.X + 392 - 32), (float)(this.geodeSpot.bounds.Y + 192 - 32)), false, false);
						Item item7 = this.geodeSpot.item;
						a = ((item7 != null) ? item7.QualifiedItemId : null);
						if (!(a == "(O)275"))
						{
							if (a == "(O)MysteryBox" || a == "(O)GoldenMysteryBox")
							{
								TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite();
								temporaryAnimatedSprite.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors_1_6");
								Item item8 = this.geodeSpot.item;
								temporaryAnimatedSprite.sourceRect = new Rectangle((((item8 != null) ? item8.QualifiedItemId : null) == "(O)GoldenMysteryBox") ? 256 : 0, 27, 24, 24);
								Item item9 = this.geodeSpot.item;
								temporaryAnimatedSprite.sourceRectStartingPos = new Vector2((float)((((item9 != null) ? item9.QualifiedItemId : null) == "(O)GoldenMysteryBox") ? 256 : 0), 27f);
								temporaryAnimatedSprite.animationLength = 8;
								temporaryAnimatedSprite.position = new Vector2((float)(this.geodeSpot.bounds.X + 380 - 48), (float)(this.geodeSpot.bounds.Y + 192 - 48));
								temporaryAnimatedSprite.holdLastFrame = true;
								temporaryAnimatedSprite.interval = 100f;
								temporaryAnimatedSprite.id = 777;
								temporaryAnimatedSprite.scale = 4f;
								this.geodeDestructionAnimation = temporaryAnimatedSprite;
								for (int i = 0; i < 6; i++)
								{
									this.fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2((float)(this.geodeSpot.bounds.X + 392 - 48 + Game1.random.Next(32)), (float)(this.geodeSpot.bounds.Y + 192 - 24)), false, 0.002f, new Color(255, 222, 198))
									{
										alphaFade = 0.02f,
										motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
										interval = 99999f,
										layerDepth = 0.9f,
										scale = 3f,
										scaleChange = 0.01f,
										rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
										delayBeforeAnimationStart = i * 20
									});
									int which = Game1.random.Next(3);
									TemporaryAnimatedSpriteList temporaryAnimatedSpriteList = this.fluffSprites;
									TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite();
									temporaryAnimatedSprite2.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors_1_6");
									Item item10 = this.geodeSpot.item;
									temporaryAnimatedSprite2.sourceRect = new Rectangle(((((item10 != null) ? item10.QualifiedItemId : null) == "(O)GoldenMysteryBox") ? 15 : 0) + which * 5, 52, 5, 5);
									temporaryAnimatedSprite2.sourceRectStartingPos = new Vector2((float)(which * 5), 75f);
									temporaryAnimatedSprite2.motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-7, -4));
									temporaryAnimatedSprite2.acceleration = new Vector2(0f, 0.25f);
									temporaryAnimatedSprite2.totalNumberOfLoops = 1;
									temporaryAnimatedSprite2.interval = 1000f;
									temporaryAnimatedSprite2.alphaFade = 0.015f;
									temporaryAnimatedSprite2.animationLength = 1;
									temporaryAnimatedSprite2.layerDepth = 1f;
									temporaryAnimatedSprite2.scale = 4f;
									temporaryAnimatedSprite2.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
									temporaryAnimatedSprite2.delayBeforeAnimationStart = i * 10;
									temporaryAnimatedSprite2.position = new Vector2((float)(this.geodeSpot.bounds.X + 392 - 48 + Game1.random.Next(32)), (float)(this.geodeSpot.bounds.Y + 192 - 24));
									temporaryAnimatedSpriteList.Add(temporaryAnimatedSprite2);
									this.delayBeforeShowArtifactTimer = 500f;
								}
							}
						}
						else
						{
							this.geodeDestructionAnimation = new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
								sourceRect = new Rectangle(388, 123, 18, 21),
								sourceRectStartingPos = new Vector2(388f, 123f),
								animationLength = 6,
								position = new Vector2((float)(this.geodeSpot.bounds.X + 380 - 32), (float)(this.geodeSpot.bounds.Y + 192 - 32)),
								holdLastFrame = true,
								interval = 100f,
								id = 777,
								scale = 4f
							};
							for (int j = 0; j < 6; j++)
							{
								this.fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2((float)(this.geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21)), (float)(this.geodeSpot.bounds.Y + 192 - 16)), false, 0.002f, new Color(255, 222, 198))
								{
									alphaFade = 0.02f,
									motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
									interval = 99999f,
									layerDepth = 0.9f,
									scale = 3f,
									scaleChange = 0.01f,
									rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
									delayBeforeAnimationStart = j * 20
								});
								this.fluffSprites.Add(new TemporaryAnimatedSprite
								{
									texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
									sourceRect = new Rectangle(499, 132, 5, 5),
									sourceRectStartingPos = new Vector2(499f, 132f),
									motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-7, -4)),
									acceleration = new Vector2(0f, 0.25f),
									totalNumberOfLoops = 1,
									interval = 1000f,
									alphaFade = 0.015f,
									animationLength = 1,
									layerDepth = 1f,
									scale = 4f,
									rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f,
									delayBeforeAnimationStart = j * 10,
									position = new Vector2((float)(this.geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21)), (float)(this.geodeSpot.bounds.Y + 192 - 16))
								});
								this.delayBeforeShowArtifactTimer = 500f;
							}
						}
						if (this.geodeTreasureOverride != null)
						{
							this.geodeTreasure = this.geodeTreasureOverride;
							this.geodeTreasureOverride = null;
						}
						else
						{
							this.geodeTreasure = Utility.getTreasureFromGeode(this.geodeSpot.item);
						}
						if (!(this.geodeSpot.item.QualifiedItemId == "(O)275"))
						{
							Object mineral = this.geodeTreasure as Object;
							if (mineral == null || !(mineral.Type == "Minerals"))
							{
								Object artifact = this.geodeTreasure as Object;
								if (artifact != null && artifact.Type == "Arch" && !Game1.player.hasOrWillReceiveMail("artifactFound"))
								{
									this.geodeTreasure = ItemRegistry.Create("(O)390", 5, 0, false);
								}
							}
						}
					}
				}
				if (this.geodeDestructionAnimation != null && ((this.geodeDestructionAnimation.id != 777 && this.geodeDestructionAnimation.currentParentTileIndex < 7) || (this.geodeDestructionAnimation.id == 777 && this.geodeDestructionAnimation.currentParentTileIndex < 5)))
				{
					this.geodeDestructionAnimation.update(time);
					if (this.delayBeforeShowArtifactTimer > 0f)
					{
						this.delayBeforeShowArtifactTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
						if (this.delayBeforeShowArtifactTimer <= 0f)
						{
							this.fluffSprites.Add(this.geodeDestructionAnimation);
							this.fluffSprites.Reverse<TemporaryAnimatedSprite>();
							this.geodeDestructionAnimation = new TemporaryAnimatedSprite
							{
								interval = 100f,
								animationLength = 6,
								alpha = 0.001f,
								id = 777
							};
						}
					}
					else
					{
						if (this.geodeDestructionAnimation.currentParentTileIndex < 3)
						{
							this.yPositionOfGem--;
						}
						this.yPositionOfGem--;
						if (this.geodeDestructionAnimation.currentParentTileIndex == 7 || (this.geodeDestructionAnimation.id == 777 && this.geodeDestructionAnimation.currentParentTileIndex == 5))
						{
							Object treasure = this.geodeTreasure as Object;
							if (treasure != null && treasure.price.Value <= 75)
							{
								Item item11 = this.geodeSpot.item;
								if (!(((item11 != null) ? item11.QualifiedItemId : null) == "(O)MysteryBox"))
								{
									Item item12 = this.geodeSpot.item;
									if (!(((item12 != null) ? item12.QualifiedItemId : null) == "(O)GoldenMysteryBox"))
									{
										Game1.playSound("newArtifact", null);
										goto IL_DB9;
									}
								}
							}
							if (this.geodeSpot.item != null)
							{
								this.sparkle = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 640, 64, 64), 100f, 8, 0, new Vector2((float)(this.geodeSpot.bounds.X + ((this.geodeSpot.item.itemId.Value == "MysteryBox") ? 94 : 98) * 4 - 32), (float)(this.geodeSpot.bounds.Y + 192 + this.yPositionOfGem - 32)), false, false);
							}
							Game1.playSound("discoverMineral", null);
						}
					}
				}
				IL_DB9:
				if (this.sparkle != null && this.sparkle.update(time))
				{
					this.sparkle = null;
				}
			}
		}

		// Token: 0x06002991 RID: 10641 RVA: 0x001EB31C File Offset: 0x001E951C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Vector2 v = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
			this.xPositionOnScreen = (int)v.X;
			this.yPositionOnScreen = (int)v.Y;
			Item tmpItem = this.geodeSpot.item;
			this.geodeSpot = new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "Anvil");
			this.geodeSpot.item = tmpItem;
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			if (this.okButton != null)
			{
				this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
				{
					myID = 4857,
					upNeighborID = 5948,
					leftNeighborID = 12
				};
			}
			if (this.trashCan != null)
			{
				this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
				{
					myID = 5948,
					downNeighborID = 4857,
					leftNeighborID = 12,
					upNeighborID = 106
				};
			}
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, false, null, this.inventory.highlightMethod, -1, 3, 0, 0, true);
		}

		// Token: 0x06002992 RID: 10642 RVA: 0x001EB528 File Offset: 0x001E9728
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			base.draw(b, true, true, -1, -1, -1);
			Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
			b.Draw(Game1.mouseCursors, new Vector2((float)this.geodeSpot.bounds.X, (float)this.geodeSpot.bounds.Y), new Rectangle?(new Rectangle(0, 512, 140, 78)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			if (this.geodeSpot.item != null)
			{
				if (this.geodeDestructionAnimation == null)
				{
					Vector2 offset = Vector2.Zero;
					if (this.geodeSpot.item.QualifiedItemId == "(O)275")
					{
						offset = new Vector2(-2f, 2f);
					}
					else
					{
						if (!(this.geodeSpot.item.QualifiedItemId == "(O)MysteryBox"))
						{
							Item item = this.geodeSpot.item;
							if (!(((item != null) ? item.QualifiedItemId : null) == "(O)GoldenMysteryBox"))
							{
								goto IL_159;
							}
						}
						offset = new Vector2(-7f, 4f);
					}
					IL_159:
					this.geodeSpot.item.QualifiedItemId == "(O)275";
					this.geodeSpot.item.drawInMenu(b, new Vector2((float)(this.geodeSpot.bounds.X + 360), (float)(this.geodeSpot.bounds.Y + 160)) + offset, 1f);
				}
				else
				{
					this.geodeDestructionAnimation.draw(b, true, 0, 0, 1f);
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.fluffSprites)
				{
					temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				}
				if (this.geodeTreasure != null && this.delayBeforeShowArtifactTimer <= 0f)
				{
					this.geodeTreasure.drawInMenu(b, new Vector2((float)(this.geodeSpot.bounds.X + (this.geodeSpot.item.QualifiedItemId.Contains("MysteryBox") ? 86 : 90) * 4), (float)(this.geodeSpot.bounds.Y + 160 + this.yPositionOfGem)), 1f);
				}
				TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.sparkle;
				if (temporaryAnimatedSprite2 != null)
				{
					temporaryAnimatedSprite2.draw(b, true, 0, 0, 1f);
				}
			}
			this.clint.draw(b, new Vector2((float)(this.geodeSpot.bounds.X + 384), (float)(this.geodeSpot.bounds.Y + 64)), 0.877f);
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			Item heldItem = base.heldItem;
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
			}
			if (!Game1.options.hardwareCursor)
			{
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x04001B2D RID: 6957
		public const int region_geodeSpot = 998;

		// Token: 0x04001B2E RID: 6958
		public ClickableComponent geodeSpot;

		// Token: 0x04001B2F RID: 6959
		public AnimatedSprite clint;

		// Token: 0x04001B30 RID: 6960
		public TemporaryAnimatedSprite geodeDestructionAnimation;

		// Token: 0x04001B31 RID: 6961
		public TemporaryAnimatedSprite sparkle;

		// Token: 0x04001B32 RID: 6962
		public int geodeAnimationTimer;

		// Token: 0x04001B33 RID: 6963
		public int yPositionOfGem;

		// Token: 0x04001B34 RID: 6964
		public int alertTimer;

		// Token: 0x04001B35 RID: 6965
		public float delayBeforeShowArtifactTimer;

		// Token: 0x04001B36 RID: 6966
		public Item geodeTreasure;

		// Token: 0x04001B37 RID: 6967
		public Item geodeTreasureOverride;

		// Token: 0x04001B38 RID: 6968
		public bool waitingForServerResponse;

		// Token: 0x04001B39 RID: 6969
		private TemporaryAnimatedSpriteList fluffSprites = new TemporaryAnimatedSpriteList();
	}
}
