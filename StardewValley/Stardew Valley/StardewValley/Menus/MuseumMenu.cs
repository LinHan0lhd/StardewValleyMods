using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	// Token: 0x0200028B RID: 651
	public class MuseumMenu : MenuWithInventory
	{
		// Token: 0x06002B12 RID: 11026 RVA: 0x002090D8 File Offset: 0x002072D8
		public MuseumMenu(InventoryMenu.highlightThisItem highlighterMethod) : base(highlighterMethod, true, false, 0, 0, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			this.fadeTimer = 800;
			this.fadeIntoBlack = true;
			base.movePosition(0, Game1.uiViewport.Height - this.yPositionOnScreen - this.height);
			Game1.player.forceCanMove();
			LibraryMuseum libraryMuseum = Game1.currentLocation as LibraryMuseum;
			if (libraryMuseum == null)
			{
				throw new InvalidOperationException("The museum donation menu must be used from within the museum.");
			}
			this.Museum = libraryMuseum;
			if (Game1.options.SnappyMenus)
			{
				if (this.okButton != null)
				{
					this.okButton.myID = 106;
				}
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
			Game1.displayHUD = false;
		}

		// Token: 0x06002B13 RID: 11027 RVA: 0x0020918D File Offset: 0x0020738D
		public override bool shouldClampGamePadCursor()
		{
			return true;
		}

		// Token: 0x06002B14 RID: 11028 RVA: 0x00209190 File Offset: 0x00207390
		public override void receiveKeyPress(Keys key)
		{
			if (this.fadeTimer <= 0)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && this.readyToClose())
				{
					this.state = 2;
					this.fadeTimer = 500;
					this.fadeIntoBlack = true;
				}
				else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && !this.holdingMuseumPiece && this.menuMovingDown)
				{
					if (base.heldItem != null)
					{
						Game1.playSound("bigDeSelect", null);
						Utility.CollectOrDrop(base.heldItem);
						base.heldItem = null;
					}
					this.ReturnToDonatableItems();
				}
				else if (Game1.options.SnappyMenus && base.heldItem == null && !this.reOrganizing)
				{
					base.receiveKeyPress(key);
				}
				if (!Game1.options.SnappyMenus)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
						return;
					}
				}
				else if (base.heldItem != null || this.reOrganizing)
				{
					LibraryMuseum museum = this.Museum;
					Vector2 newCursorPositionTile = new Vector2((float)((int)((Utility.ModifyCoordinateFromUIScale((float)Game1.getMouseX()) + (float)Game1.viewport.X) / 64f)), (float)((int)((Utility.ModifyCoordinateFromUIScale((float)Game1.getMouseY()) + (float)Game1.viewport.Y) / 64f)));
					if (!museum.isTileSuitableForMuseumPiece((int)newCursorPositionTile.X, (int)newCursorPositionTile.Y) && (!this.reOrganizing || !LibraryMuseum.HasDonatedArtifactAt(newCursorPositionTile)))
					{
						newCursorPositionTile = museum.getFreeDonationSpot();
						Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale(newCursorPositionTile.X * 64f - (float)Game1.viewport.X + 32f), (int)Utility.ModifyCoordinateForUIScale(newCursorPositionTile.Y * 64f - (float)Game1.viewport.Y + 32f));
						return;
					}
					if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton))
					{
						newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 0, 21, !this.reOrganizing);
					}
					else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton))
					{
						newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 1, 21, !this.reOrganizing);
					}
					else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton))
					{
						newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 2, 21, !this.reOrganizing);
					}
					else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton))
					{
						newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 3, 21, !this.reOrganizing);
					}
					if (!Game1.viewport.Contains(new Location((int)(newCursorPositionTile.X * 64f + 32f), Game1.viewport.Y + 1)))
					{
						Game1.panScreen((int)(newCursorPositionTile.X * 64f - (float)Game1.viewport.X), 0);
					}
					else if (!Game1.viewport.Contains(new Location(Game1.viewport.X + 1, (int)(newCursorPositionTile.Y * 64f + 32f))))
					{
						Game1.panScreen(0, (int)(newCursorPositionTile.Y * 64f - (float)Game1.viewport.Y));
					}
					Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale((float)((int)newCursorPositionTile.X * 64 - Game1.viewport.X + 32)), (int)Utility.ModifyCoordinateForUIScale((float)((int)newCursorPositionTile.Y * 64 - Game1.viewport.Y + 32)));
				}
			}
		}

		// Token: 0x06002B15 RID: 11029 RVA: 0x00209598 File Offset: 0x00207798
		public override void receiveGamePadButton(Buttons button)
		{
			if ((button == Buttons.DPadUp || button == Buttons.LeftThumbstickUp) && !this.menuMovingDown && Game1.options.SnappyMenus && this.currentlySnappedComponent != null && this.currentlySnappedComponent.myID < 12)
			{
				this.reOrganizing = true;
				this.menuMovingDown = true;
				this.receiveKeyPress(Game1.options.moveUpButton[0].key);
			}
		}

		// Token: 0x06002B16 RID: 11030 RVA: 0x00209608 File Offset: 0x00207808
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.fadeTimer <= 0)
			{
				if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
				{
					if (this.fadeTimer <= 0)
					{
						Game1.playSound("bigDeSelect", null);
					}
					this.state = 2;
					this.fadeTimer = 800;
					this.fadeIntoBlack = true;
					return;
				}
				Item oldItem = base.heldItem;
				if (!this.holdingMuseumPiece)
				{
					if (base.heldItem == null)
					{
						int inventoryIndex = this.inventory.getInventoryPositionOfClick(x, y);
						Item inventoryItem = (inventoryIndex >= 0 && inventoryIndex < this.inventory.actualInventory.Count) ? this.inventory.actualInventory[inventoryIndex] : null;
						if (inventoryItem != null && this.inventory.highlightMethod(inventoryItem))
						{
							base.heldItem = inventoryItem.getOne();
							this.inventory.actualInventory[inventoryIndex] = inventoryItem.ConsumeStack(1);
						}
					}
					else
					{
						base.heldItem = this.inventory.leftClick(x, y, base.heldItem, true);
					}
				}
				if (oldItem == null && base.heldItem != null && Game1.isAnyGamePadButtonBeingPressed())
				{
					this.receiveGamePadButton(Buttons.DPadUp);
				}
				if (oldItem != null && base.heldItem != null && (y < Game1.viewport.Height - (this.height - (IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192)) || this.menuMovingDown))
				{
					Item item = base.heldItem;
					LibraryMuseum museum = this.Museum;
					int mapXTile = (int)(Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64;
					int mapYTile = (int)(Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64;
					if (museum.isTileSuitableForMuseumPiece(mapXTile, mapYTile) && museum.isItemSuitableForDonation(item))
					{
						int rewardsCount = museum.getRewardsForPlayer(Game1.player).Count;
						museum.museumPieces.Add(new Vector2((float)mapXTile, (float)mapYTile), item.ItemId);
						Game1.playSound("stoneStep", null);
						if (museum.getRewardsForPlayer(Game1.player).Count > rewardsCount && !this.holdingMuseumPiece)
						{
							this.sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:NewReward"), Color.MediumSpringGreen, Color.White, false, 0.1, 2500, -1, 500, 1f);
							Game1.playSound("reward", null);
							this.globalLocationOfSparklingArtifact = new Vector2((float)(mapXTile * 64 + 32) - this.sparkleText.textWidth / 2f, (float)(mapYTile * 64 - 48));
						}
						else
						{
							Game1.playSound("newArtifact", null);
						}
						Game1.player.completeQuest("24");
						base.heldItem = item.ConsumeStack(1);
						int pieces = museum.museumPieces.Length;
						if (!this.holdingMuseumPiece)
						{
							Game1.stats.checkForArchaeologyAchievements();
							if (pieces == LibraryMuseum.totalArtifacts)
							{
								Game1.multiplayer.globalChatInfoMessage("MuseumComplete", new string[]
								{
									Game1.player.farmName.Value
								});
							}
							else if (pieces == 40)
							{
								Game1.multiplayer.globalChatInfoMessage("Museum40", new string[]
								{
									Game1.player.farmName.Value
								});
							}
							else
							{
								Game1.multiplayer.globalChatInfoMessage("donation", new string[]
								{
									Game1.player.name.Value,
									TokenStringBuilder.ItemNameFor(item, null)
								});
							}
						}
						this.ReturnToDonatableItems();
					}
				}
				else if (base.heldItem == null && !this.inventory.isWithinBounds(x, y))
				{
					int mapXTile2 = (int)(Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64;
					int mapYTile2 = (int)(Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64;
					Vector2 v = new Vector2((float)mapXTile2, (float)mapYTile2);
					LibraryMuseum location = this.Museum;
					string itemId;
					if (location.museumPieces.TryGetValue(v, out itemId))
					{
						base.heldItem = ItemRegistry.Create(itemId, 1, 0, true);
						location.museumPieces.Remove(v);
						if (base.heldItem != null)
						{
							this.holdingMuseumPiece = !LibraryMuseum.HasDonatedArtifact(base.heldItem.QualifiedItemId);
						}
					}
				}
				if (base.heldItem != null && oldItem == null)
				{
					this.menuMovingDown = true;
					this.reOrganizing = false;
				}
			}
		}

		// Token: 0x06002B17 RID: 11031 RVA: 0x00209A8D File Offset: 0x00207C8D
		public virtual void ReturnToDonatableItems()
		{
			this.menuMovingDown = false;
			this.holdingMuseumPiece = false;
			this.reOrganizing = false;
			if (Game1.options.SnappyMenus)
			{
				base.movePosition(0, -this.menuPositionOffset);
				this.menuPositionOffset = 0;
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002B18 RID: 11032 RVA: 0x00209ACC File Offset: 0x00207CCC
		public override void emergencyShutDown()
		{
			if (base.heldItem != null && this.holdingMuseumPiece)
			{
				Vector2 tile = this.Museum.getFreeDonationSpot();
				if (this.Museum.museumPieces.TryAdd(tile, base.heldItem.ItemId))
				{
					base.heldItem = null;
					this.holdingMuseumPiece = false;
				}
			}
			base.emergencyShutDown();
		}

		// Token: 0x06002B19 RID: 11033 RVA: 0x00209B27 File Offset: 0x00207D27
		public override bool readyToClose()
		{
			return !this.holdingMuseumPiece && base.heldItem == null && !this.menuMovingDown;
		}

		// Token: 0x06002B1A RID: 11034 RVA: 0x00209B44 File Offset: 0x00207D44
		protected override void cleanupBeforeExit()
		{
			if (base.heldItem != null)
			{
				base.heldItem = Game1.player.addItemToInventory(base.heldItem);
				if (base.heldItem != null)
				{
					Game1.createItemDebris(base.heldItem, Game1.player.Position, -1, null, -1, false);
					base.heldItem = null;
				}
			}
			Game1.displayHUD = true;
		}

		// Token: 0x06002B1B RID: 11035 RVA: 0x00209BA0 File Offset: 0x00207DA0
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Item oldItem = base.heldItem;
			if (this.fadeTimer <= 0)
			{
				base.receiveRightClick(x, y, true);
			}
			if (base.heldItem != null && oldItem == null)
			{
				this.menuMovingDown = true;
			}
		}

		// Token: 0x06002B1C RID: 11036 RVA: 0x00209BD8 File Offset: 0x00207DD8
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.sparkleText != null && this.sparkleText.update(time))
			{
				this.sparkleText = null;
			}
			if (this.fadeTimer > 0)
			{
				this.fadeTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.fadeIntoBlack)
				{
					this.blackFadeAlpha = 0f + (1500f - (float)this.fadeTimer) / 1500f;
				}
				else
				{
					this.blackFadeAlpha = 1f - (1500f - (float)this.fadeTimer) / 1500f;
				}
				if (this.fadeTimer <= 0)
				{
					switch (this.state)
					{
					case 0:
						this.state = 1;
						Game1.viewportFreeze = true;
						Game1.viewport.Location = new Location(1152, 128);
						Game1.clampViewportToGameMap();
						this.fadeTimer = 800;
						this.fadeIntoBlack = false;
						break;
					case 2:
						Game1.viewportFreeze = false;
						this.fadeIntoBlack = false;
						this.fadeTimer = 800;
						this.state = 3;
						break;
					case 3:
						base.exitThisMenuNoSound();
						break;
					}
				}
			}
			if (this.menuMovingDown && this.menuPositionOffset < this.height / 3)
			{
				this.menuPositionOffset += 8;
				base.movePosition(0, 8);
			}
			else if (!this.menuMovingDown && this.menuPositionOffset > 0)
			{
				this.menuPositionOffset -= 8;
				base.movePosition(0, -8);
			}
			int mouseX = Game1.getOldMouseX(false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(false) + Game1.viewport.Y;
			if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseX - Game1.viewport.X < 64) || Game1.input.GetGamePadState().ThumbSticks.Right.X < 0f)
			{
				Game1.panScreen(-4, 0);
				if (Game1.input.GetGamePadState().ThumbSticks.Right.X < 0f)
				{
					this.snapCursorToCurrentMuseumSpot();
				}
			}
			else if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64) || Game1.input.GetGamePadState().ThumbSticks.Right.X > 0f)
			{
				Game1.panScreen(4, 0);
				if (Game1.input.GetGamePadState().ThumbSticks.Right.X > 0f)
				{
					this.snapCursorToCurrentMuseumSpot();
				}
			}
			if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseY - Game1.viewport.Y < 64) || Game1.input.GetGamePadState().ThumbSticks.Right.Y > 0f)
			{
				Game1.panScreen(0, -4);
				if (Game1.input.GetGamePadState().ThumbSticks.Right.Y > 0f)
				{
					this.snapCursorToCurrentMuseumSpot();
				}
			}
			else if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64) || Game1.input.GetGamePadState().ThumbSticks.Right.Y < 0f)
			{
				Game1.panScreen(0, 4);
				if (Game1.input.GetGamePadState().ThumbSticks.Right.Y < 0f)
				{
					this.snapCursorToCurrentMuseumSpot();
				}
			}
			foreach (Keys key in Game1.oldKBState.GetPressedKeys())
			{
				this.receiveKeyPress(key);
			}
		}

		// Token: 0x06002B1D RID: 11037 RVA: 0x00209FD8 File Offset: 0x002081D8
		private void snapCursorToCurrentMuseumSpot()
		{
			if (!this.menuMovingDown)
			{
				return;
			}
			Vector2 newCursorPositionTile = new Vector2((float)((Game1.getMouseX(false) + Game1.viewport.X) / 64), (float)((Game1.getMouseY(false) + Game1.viewport.Y) / 64));
			Game1.setMousePosition((int)newCursorPositionTile.X * 64 - Game1.viewport.X + 32, (int)newCursorPositionTile.Y * 64 - Game1.viewport.Y + 32, false);
		}

		// Token: 0x06002B1E RID: 11038 RVA: 0x0020A055 File Offset: 0x00208255
		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			base.movePosition(0, Game1.viewport.Height - this.yPositionOnScreen - this.height);
			Game1.player.forceCanMove();
		}

		// Token: 0x06002B1F RID: 11039 RVA: 0x0020A088 File Offset: 0x00208288
		public override void draw(SpriteBatch b)
		{
			if ((this.fadeTimer <= 0 || !this.fadeIntoBlack) && this.state != 3)
			{
				if (base.heldItem != null)
				{
					Game1.StartWorldDrawInUI(b);
					for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2; y++)
					{
						for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
						{
							if (this.Museum.isTileSuitableForMuseumPiece(x, y))
							{
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)x, (float)y) * 64f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29, -1, -1)), Color.LightGreen);
							}
						}
					}
					Game1.EndWorldDrawInUI(b);
				}
				if (!this.holdingMuseumPiece)
				{
					base.draw(b, false, false, -1, -1, -1);
				}
				if (!this.hoverText.Equals(""))
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
				Item heldItem = base.heldItem;
				if (heldItem != null)
				{
					heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
				}
				base.drawMouse(b, false, -1);
				SparklingText sparklingText = this.sparkleText;
				if (sparklingText != null)
				{
					sparklingText.draw(b, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.viewport, this.globalLocationOfSparklingArtifact)));
				}
			}
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * this.blackFadeAlpha);
		}

		// Token: 0x04001CAC RID: 7340
		public const int startingState = 0;

		// Token: 0x04001CAD RID: 7341
		public const int placingInMuseumState = 1;

		// Token: 0x04001CAE RID: 7342
		public const int exitingState = 2;

		// Token: 0x04001CAF RID: 7343
		public int fadeTimer;

		// Token: 0x04001CB0 RID: 7344
		public int state;

		// Token: 0x04001CB1 RID: 7345
		public int menuPositionOffset;

		// Token: 0x04001CB2 RID: 7346
		public bool fadeIntoBlack;

		// Token: 0x04001CB3 RID: 7347
		public bool menuMovingDown;

		// Token: 0x04001CB4 RID: 7348
		public float blackFadeAlpha;

		// Token: 0x04001CB5 RID: 7349
		public SparklingText sparkleText;

		// Token: 0x04001CB6 RID: 7350
		public Vector2 globalLocationOfSparklingArtifact;

		// Token: 0x04001CB7 RID: 7351
		public LibraryMuseum Museum;

		// Token: 0x04001CB8 RID: 7352
		private bool holdingMuseumPiece;

		// Token: 0x04001CB9 RID: 7353
		public bool reOrganizing;
	}
}
