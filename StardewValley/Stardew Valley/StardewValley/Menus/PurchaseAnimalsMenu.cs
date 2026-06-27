using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	// Token: 0x0200029F RID: 671
	public class PurchaseAnimalsMenu : IClickableMenu
	{
		// Token: 0x06002BCF RID: 11215 RVA: 0x00214ED8 File Offset: 0x002130D8
		public PurchaseAnimalsMenu(List<Object> stock, GameLocation targetLocation = null) : base(Game1.uiViewport.Width / 2 - PurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - PurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4, PurchaseAnimalsMenu.menuWidth + IClickableMenu.borderWidth * 2 + ((PurchaseAnimalsMenu.GetOffScreenRows(stock.Count) > 0) ? 44 : 0), PurchaseAnimalsMenu.menuHeight + IClickableMenu.borderWidth, false)
		{
			this.height += 64;
			this.TargetLocation = (targetLocation ?? Game1.getFarm());
			for (int i = 0; i < stock.Count; i++)
			{
				FarmAnimalData animalData;
				Texture2D texture;
				Microsoft.Xna.Framework.Rectangle sourceRect;
				if (Game1.farmAnimalData.TryGetValue(stock[i].Name, out animalData) && animalData.ShopTexture != null)
				{
					texture = Game1.content.Load<Texture2D>(animalData.ShopTexture);
					sourceRect = animalData.ShopSourceRect;
				}
				else if (i >= 9)
				{
					texture = Game1.mouseCursors2;
					sourceRect = new Microsoft.Xna.Framework.Rectangle(128 + i % 3 * 16 * 2, i / 3 * 16, 32, 16);
				}
				else
				{
					texture = Game1.mouseCursors;
					sourceRect = new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16);
				}
				ClickableTextureComponent animalButton = new ClickableTextureComponent(stock[i].salePrice(false).ToString() ?? "", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth + i % 3 * 64 * 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + i / 3 * 85, 128, 64), null, stock[i].Name, texture, sourceRect, 4f, stock[i].Type == null)
				{
					item = stock[i],
					myID = i,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998,
					upNeighborID = -99998
				};
				this.animalsToPurchase.Add(animalButton);
			}
			this.scrollRows = PurchaseAnimalsMenu.GetOffScreenRows(this.animalsToPurchase.Count);
			if (this.scrollRows < 0)
			{
				this.scrollRows = 0;
			}
			this.RepositionAnimalButtons();
			this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 101,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f, false)
			{
				myID = 103,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			PurchaseAnimalsMenu.menuHeight = 320;
			PurchaseAnimalsMenu.menuWidth = 384;
			this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			this.textBox.X = Game1.uiViewport.Width / 2 - 192;
			this.textBox.Y = Game1.uiViewport.Height / 2;
			this.textBox.Width = 256;
			this.textBox.Height = 192;
			this.textBoxEvent = new TextBoxEvent(this.textBoxEnter);
			this.textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f, false)
			{
				myID = 103,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 102,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			int arrowsX = this.xPositionOnScreen + this.width - 64 - 24;
			this.upArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 459, 11, 12), 4f, false)
			{
				myID = 105,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.downArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, this.yPositionOnScreen + this.height - 64 - 24, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 472, 11, 12), 4f, false)
			{
				myID = 106,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.doneNamingButton.visible = false;
			this.randomButton.visible = false;
			this.textBoxCC.visible = false;
			if (this.scrollRows <= 0)
			{
				this.upArrow.visible = false;
				this.downArrow.visible = false;
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002BD0 RID: 11216 RVA: 0x002155A5 File Offset: 0x002137A5
		public static int GetOffScreenRows(int animalsToPurchase)
		{
			return (animalsToPurchase - 1) / 3 + 1 - 3;
		}

		// Token: 0x06002BD1 RID: 11217 RVA: 0x002155B0 File Offset: 0x002137B0
		public override bool shouldClampGamePadCursor()
		{
			return this.onFarm;
		}

		// Token: 0x06002BD2 RID: 11218 RVA: 0x002155B8 File Offset: 0x002137B8
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002BD3 RID: 11219 RVA: 0x002155D0 File Offset: 0x002137D0
		public void textBoxEnter(TextBox sender)
		{
			if (!this.namingAnimal)
			{
				return;
			}
			if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
			{
				this.textBox.OnEnterPressed -= this.textBoxEvent;
				return;
			}
			if (sender.Text.Length >= 1)
			{
				if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11308"), true);
					return;
				}
				this.textBox.OnEnterPressed -= this.textBoxEvent;
				this.animalBeingPurchased.Name = sender.Text;
				this.animalBeingPurchased.displayName = sender.Text;
				((AnimalHouse)this.newAnimalHome.GetIndoors()).adoptAnimal(this.animalBeingPurchased);
				this.newAnimalHome = null;
				this.namingAnimal = false;
				Game1.player.Money -= this.priceOfAnimal;
				this.setUpForReturnAfterPurchasingAnimal();
			}
		}

		// Token: 0x06002BD4 RID: 11220 RVA: 0x002156B8 File Offset: 0x002138B8
		public void setUpForReturnAfterPurchasingAnimal()
		{
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop", false);
			locationRequest.OnWarp += delegate()
			{
				this.onFarm = false;
				Game1.player.viewingLocation.Value = null;
				this.okButton.bounds.X = this.xPositionOnScreen + this.width + 4;
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				this.freeze = false;
				this.textBox.OnEnterPressed -= this.textBoxEvent;
				this.textBox.Selected = false;
				Game1.viewportFreeze = false;
				this.marnieAnimalPurchaseMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
		}

		// Token: 0x06002BD5 RID: 11221 RVA: 0x00215710 File Offset: 0x00213910
		public void marnieAnimalPurchaseMessage()
		{
			base.exitThisMenu(true);
			Game1.player.forceCanMove();
			this.freeze = false;
			Game1.DrawDialogue(Game1.getCharacterFromName("Marnie", true, false), this.animalBeingPurchased.isMale() ? "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311" : "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", new object[]
			{
				this.animalBeingPurchased.displayName
			});
		}

		// Token: 0x06002BD6 RID: 11222 RVA: 0x00215774 File Offset: 0x00213974
		public void setUpForAnimalPlacement()
		{
			this.upArrow.visible = false;
			this.downArrow.visible = false;
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			Game1.currentLocation = this.TargetLocation;
			Game1.player.viewingLocation.Value = this.TargetLocation.NameOrUniqueName;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear(null, 0.02f);
			this.onFarm = true;
			this.freeze = false;
			this.okButton.bounds.X = Game1.uiViewport.Width - 128;
			this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Building suggestedBuilding = this.GetSuggestedBuilding(this.animalBeingPurchased);
			if (suggestedBuilding != null)
			{
				Game1.viewport.Location = this.GetTopLeftPixelToCenterBuilding(suggestedBuilding);
			}
			Game1.panScreen(0, 0);
		}

		// Token: 0x06002BD7 RID: 11223 RVA: 0x00215880 File Offset: 0x00213A80
		public void setUpForReturnToShopMenu()
		{
			this.freeze = false;
			if (this.scrollRows > 0)
			{
				this.upArrow.visible = true;
				this.downArrow.visible = true;
			}
			this.doneNamingButton.visible = false;
			this.randomButton.visible = false;
			Game1.displayFarmer = true;
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop", false);
			locationRequest.OnWarp += delegate()
			{
				this.onFarm = false;
				Game1.player.viewingLocation.Value = null;
				this.okButton.bounds.X = this.xPositionOnScreen + this.width + 4;
				this.okButton.bounds.Y = this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				this.namingAnimal = false;
				this.textBox.OnEnterPressed -= this.textBoxEvent;
				this.textBox.Selected = false;
				if (Game1.options.SnappyMenus)
				{
					this.setCurrentlySnappedComponentTo(this.clickedAnimalButton);
					this.snapCursorToCurrentSnappedComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
		}

		// Token: 0x06002BD8 RID: 11224 RVA: 0x0021591D File Offset: 0x00213B1D
		public virtual void Scroll(int offset)
		{
			this.currentScroll += offset;
			if (this.currentScroll < 0)
			{
				this.currentScroll = 0;
			}
			if (this.currentScroll > this.scrollRows)
			{
				this.currentScroll = this.scrollRows;
			}
			this.RepositionAnimalButtons();
		}

		// Token: 0x06002BD9 RID: 11225 RVA: 0x00215960 File Offset: 0x00213B60
		public virtual void RepositionAnimalButtons()
		{
			foreach (ClickableTextureComponent clickableTextureComponent in this.animalsToPurchase)
			{
				clickableTextureComponent.visible = false;
			}
			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < 3; x++)
				{
					int index = (y + this.currentScroll) * 3 + x;
					if (index >= this.animalsToPurchase.Count || index < 0)
					{
						break;
					}
					ClickableTextureComponent clickableTextureComponent2 = this.animalsToPurchase[index];
					clickableTextureComponent2.bounds.X = this.xPositionOnScreen + IClickableMenu.borderWidth + x * 64 * 2;
					clickableTextureComponent2.bounds.Y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + y * 85;
					clickableTextureComponent2.visible = true;
				}
			}
		}

		// Token: 0x06002BDA RID: 11226 RVA: 0x00215A44 File Offset: 0x00213C44
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.IsFading() || this.freeze)
			{
				return;
			}
			if (this.upArrow.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this.Scroll(-1);
			}
			else if (this.downArrow.containsPoint(x, y))
			{
				Game1.playSound("shwip", null);
				this.Scroll(1);
			}
			if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
			{
				if (this.onFarm)
				{
					this.setUpForReturnToShopMenu();
					Game1.playSound("smallSelect", null);
				}
				else
				{
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect", null);
				}
			}
			if (this.onFarm)
			{
				Vector2 clickTile = new Vector2((float)((int)((Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64f)), (float)((int)((Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64f)));
				Building selection = this.TargetLocation.getBuildingAt(clickTile);
				if (!this.namingAnimal)
				{
					AnimalHouse animalHouse = ((selection != null) ? selection.GetIndoors() : null) as AnimalHouse;
					if (animalHouse != null && !selection.isUnderConstruction(true))
					{
						if (this.animalBeingPurchased.CanLiveIn(selection))
						{
							if (animalHouse.isFull())
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"), true);
							}
							else
							{
								this.namingAnimal = true;
								this.doneNamingButton.visible = true;
								this.randomButton.visible = true;
								this.textBoxCC.visible = true;
								this.newAnimalHome = selection;
								FarmAnimalData data = this.animalBeingPurchased.GetAnimalData();
								if (data != null)
								{
									if (data.BabySound != null)
									{
										Game1.playSound(data.BabySound, new int?(1200 + Game1.random.Next(-200, 201)));
									}
									else if (data.Sound != null)
									{
										Game1.playSound(data.Sound, new int?(1200 + Game1.random.Next(-200, 201)));
									}
								}
								this.textBox.OnEnterPressed += this.textBoxEvent;
								this.textBox.Text = this.animalBeingPurchased.displayName;
								Game1.keyboardDispatcher.Subscriber = this.textBox;
								if (Game1.options.SnappyMenus)
								{
									this.currentlySnappedComponent = base.getComponentWithID(104);
									this.snapCursorToCurrentSnappedComponent();
								}
							}
						}
						else
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", this.animalBeingPurchased.displayType), true);
						}
					}
				}
				if (this.namingAnimal)
				{
					if (this.doneNamingButton.containsPoint(x, y))
					{
						this.textBoxEnter(this.textBox);
						Game1.playSound("smallSelect", null);
					}
					else if (this.namingAnimal && this.randomButton.containsPoint(x, y))
					{
						this.animalBeingPurchased.Name = Dialogue.randomName();
						this.animalBeingPurchased.displayName = this.animalBeingPurchased.Name;
						this.textBox.Text = this.animalBeingPurchased.displayName;
						this.randomButton.scale = this.randomButton.baseScale;
						Game1.playSound("drumkit6", null);
					}
					this.textBox.Update();
					return;
				}
			}
			else
			{
				foreach (ClickableTextureComponent c in this.animalsToPurchase)
				{
					if (!this.readOnly && c.containsPoint(x, y) && (c.item as Object).Type == null)
					{
						int price = c.item.salePrice(false);
						if (Game1.player.Money >= price)
						{
							this.clickedAnimalButton = c.myID;
							Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForAnimalPlacement), 0.02f);
							Game1.playSound("smallSelect", null);
							this.onFarm = true;
							string animalType = c.hoverText;
							FarmAnimalData animalData;
							if (Game1.farmAnimalData.TryGetValue(animalType, out animalData) && animalData.AlternatePurchaseTypes != null)
							{
								foreach (AlternatePurchaseAnimals alternateAnimal in animalData.AlternatePurchaseTypes)
								{
									if (GameStateQuery.CheckConditions(alternateAnimal.Condition, null, null, null, null, null, null))
									{
										animalType = Game1.random.ChooseFrom(alternateAnimal.AnimalIds);
										break;
									}
								}
							}
							this.animalBeingPurchased = new FarmAnimal(animalType, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
							this.priceOfAnimal = price;
						}
						else
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"), 3));
						}
					}
				}
			}
		}

		// Token: 0x06002BDB RID: 11227 RVA: 0x00215F74 File Offset: 0x00214174
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return this.onFarm && !this.namingAnimal;
		}

		// Token: 0x06002BDC RID: 11228 RVA: 0x00215F8C File Offset: 0x0021418C
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button == Buttons.B && !Game1.globalFade && this.onFarm && this.namingAnimal)
			{
				this.setUpForReturnToShopMenu();
				Game1.playSound("smallSelect", null);
			}
		}

		// Token: 0x06002BDD RID: 11229 RVA: 0x00215FDC File Offset: 0x002141DC
		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if ((b - Buttons.DPadUp <= 1 || b == Buttons.DPadLeft || b == Buttons.DPadRight) && this.onFarm && !this.namingAnimal)
			{
				GamePadState gamePadState = Game1.input.GetGamePadState();
				MouseState mouseState = Game1.input.GetMouseState();
				int speed = 12 + ((gamePadState.IsButtonDown(Buttons.RightTrigger) || gamePadState.IsButtonDown(Buttons.RightShoulder)) ? 8 : 0);
				int xOff = (b == Buttons.DPadRight) ? speed : ((b == Buttons.DPadLeft) ? (-speed) : 0);
				int yOff = (b == Buttons.DPadDown) ? speed : ((b == Buttons.DPadUp) ? (-speed) : 0);
				Game1.setMousePositionRaw(mouseState.X + xOff, mouseState.Y + yOff);
			}
		}

		// Token: 0x06002BDE RID: 11230 RVA: 0x00216088 File Offset: 0x00214288
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || this.freeze)
			{
				return;
			}
			if (!Game1.globalFade && this.onFarm)
			{
				if (!this.namingAnimal)
				{
					if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose() && !Game1.IsFading())
					{
						this.setUpForReturnToShopMenu();
						return;
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
				}
				else if (Game1.options.SnappyMenus)
				{
					if (!this.textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						this.setUpForReturnToShopMenu();
						Game1.playSound("smallSelect", null);
						return;
					}
					if (!this.textBox.Selected || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						base.receiveKeyPress(key);
						return;
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.IsFading())
			{
				if (this.readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect", null);
					return;
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002BDF RID: 11231 RVA: 0x0021625C File Offset: 0x0021445C
		public override void update(GameTime time)
		{
			base.update(time);
			if (!this.onFarm)
			{
				this.upArrow.visible = (this.currentScroll > 0);
				this.downArrow.visible = (this.currentScroll < this.scrollRows);
				return;
			}
			if (!this.namingAnimal)
			{
				int mouseX = Game1.getOldMouseX(false) + Game1.viewport.X;
				int mouseY = Game1.getOldMouseY(false) + Game1.viewport.Y;
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
				{
					Game1.panScreen(8, 0);
				}
				if (mouseY - Game1.viewport.Y < 64)
				{
					Game1.panScreen(0, -8);
				}
				else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
				{
					Game1.panScreen(0, 8);
				}
				foreach (Keys key in Game1.oldKBState.GetPressedKeys())
				{
					this.receiveKeyPress(key);
				}
			}
		}

		// Token: 0x06002BE0 RID: 11232 RVA: 0x00216374 File Offset: 0x00214574
		public override void performHoverAction(int x, int y)
		{
			this.hovered = null;
			if (Game1.IsFading() || this.freeze)
			{
				return;
			}
			this.upArrow.tryHover(x, y, 0.1f);
			this.downArrow.tryHover(x, y, 0.1f);
			if (this.okButton != null)
			{
				if (this.okButton.containsPoint(x, y))
				{
					this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
				}
				else
				{
					this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
				}
			}
			if (this.onFarm)
			{
				if (!this.namingAnimal)
				{
					Vector2 clickTile = new Vector2((float)((int)((Utility.ModifyCoordinateFromUIScale((float)x) + (float)Game1.viewport.X) / 64f)), (float)((int)((Utility.ModifyCoordinateFromUIScale((float)y) + (float)Game1.viewport.Y) / 64f)));
					GameLocation f = this.TargetLocation;
					foreach (Building building in f.buildings)
					{
						building.color = Color.White;
					}
					Building selection = f.getBuildingAt(clickTile);
					AnimalHouse animalHouse = ((selection != null) ? selection.GetIndoors() : null) as AnimalHouse;
					if (animalHouse != null)
					{
						if (this.animalBeingPurchased.CanLiveIn(selection) && !animalHouse.isFull())
						{
							selection.color = Color.LightGreen * 0.8f;
						}
						else
						{
							selection.color = Color.Red * 0.8f;
						}
					}
				}
				if (this.doneNamingButton != null)
				{
					if (this.doneNamingButton.containsPoint(x, y))
					{
						this.doneNamingButton.scale = Math.Min(1.1f, this.doneNamingButton.scale + 0.05f);
					}
					else
					{
						this.doneNamingButton.scale = Math.Max(1f, this.doneNamingButton.scale - 0.05f);
					}
				}
				this.randomButton.tryHover(x, y, 0.5f);
				return;
			}
			foreach (ClickableTextureComponent c in this.animalsToPurchase)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.05f, 4.1f);
					this.hovered = c;
				}
				else
				{
					c.scale = Math.Max(4f, c.scale - 0.025f);
				}
			}
		}

		// Token: 0x06002BE1 RID: 11233 RVA: 0x0021662C File Offset: 0x0021482C
		public override void draw(SpriteBatch b)
		{
			if (!this.onFarm && !Game1.dialogueUp && !Game1.IsFading())
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354"), this.xPositionOnScreen + 96, this.yPositionOnScreen, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
				Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
				this.upArrow.draw(b);
				this.downArrow.draw(b);
				using (List<ClickableTextureComponent>.Enumerator enumerator = this.animalsToPurchase.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ClickableTextureComponent c = enumerator.Current;
						c.draw(b, ((c.item as Object).Type != null) ? (Color.Black * 0.4f) : Color.White, 0.87f, 0, 0, 0);
					}
					goto IL_2CA;
				}
			}
			if (!Game1.IsFading() && this.onFarm)
			{
				string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", this.animalBeingPurchased.displayHouse, this.animalBeingPurchased.displayType);
				SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s, 999999) / 2, 16, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				if (this.namingAnimal)
				{
					if (!Game1.options.showClearBackgrounds)
					{
						b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					}
					Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 256, Game1.uiViewport.Height / 2 - 192 - 32, 512, 192, false, true, null, false, true, -1, -1, -1);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357"), Game1.dialogueFont, new Vector2((float)(Game1.uiViewport.Width / 2 - 256 + 32 + 8), (float)(Game1.uiViewport.Height / 2 - 128 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					this.textBox.Draw(b, true);
					this.doneNamingButton.draw(b);
					this.randomButton.draw(b);
				}
			}
			IL_2CA:
			if (!Game1.IsFading() && this.okButton != null)
			{
				this.okButton.draw(b);
			}
			if (this.hovered != null)
			{
				if ((this.hovered.item as Object).Type != null)
				{
					IClickableMenu.drawHoverText(b, Game1.parseText((this.hovered.item as Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
				else
				{
					string displayName = FarmAnimal.GetDisplayName(this.hovered.hoverText, true);
					SpriteText.drawStringWithScrollBackground(b, displayName, this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, this.yPositionOnScreen + this.height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig", 1f, null, SpriteText.ScrollTextAlignment.Left);
					SpriteText.drawStringWithScrollBackground(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.hovered.item.salePrice(false)), this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, this.yPositionOnScreen + this.height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", (Game1.player.Money >= this.hovered.item.salePrice(false)) ? 1f : 0.5f, null, SpriteText.ScrollTextAlignment.Left);
					string description = FarmAnimal.GetShopDescription(this.hovered.hoverText);
					IClickableMenu.drawHoverText(b, Game1.parseText(description, Game1.smallFont, 320), Game1.smallFont, 0, 0, this.hovered.item.salePrice(false), displayName, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
			Game1.mouseCursorTransparency = (Game1.IsFading() ? 0f : 1f);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x06002BE2 RID: 11234 RVA: 0x00216B44 File Offset: 0x00214D44
		public Building GetSuggestedBuilding(FarmAnimal animal)
		{
			Building bestBuilding = null;
			foreach (Building building in this.TargetLocation.buildings)
			{
				if (this.animalBeingPurchased.CanLiveIn(building))
				{
					bestBuilding = building;
					AnimalHouse animalHouse = building.GetIndoors() as AnimalHouse;
					if (animalHouse != null && !animalHouse.isFull())
					{
						return bestBuilding;
					}
				}
			}
			return bestBuilding;
		}

		// Token: 0x06002BE3 RID: 11235 RVA: 0x00216BC8 File Offset: 0x00214DC8
		public Location GetTopLeftPixelToCenterBuilding(Building building)
		{
			Vector2 screenPosition = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, building.tilesWide.Value * 64, building.tilesHigh.Value * 64, 0, 0);
			int x = building.tileX.Value * 64 - (int)screenPosition.X;
			int yOrigin = building.tileY.Value * 64 - (int)screenPosition.Y;
			return new Location(x, yOrigin);
		}

		// Token: 0x04001D85 RID: 7557
		public const int region_okButton = 101;

		// Token: 0x04001D86 RID: 7558
		public const int region_doneNamingButton = 102;

		// Token: 0x04001D87 RID: 7559
		public const int region_randomButton = 103;

		// Token: 0x04001D88 RID: 7560
		public const int region_namingBox = 104;

		// Token: 0x04001D89 RID: 7561
		public const int region_upArrow = 105;

		// Token: 0x04001D8A RID: 7562
		public const int region_downArrow = 106;

		// Token: 0x04001D8B RID: 7563
		public static int menuHeight = 320;

		// Token: 0x04001D8C RID: 7564
		public static int menuWidth = 384;

		// Token: 0x04001D8D RID: 7565
		public int clickedAnimalButton = -1;

		// Token: 0x04001D8E RID: 7566
		public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

		// Token: 0x04001D8F RID: 7567
		public ClickableTextureComponent okButton;

		// Token: 0x04001D90 RID: 7568
		public ClickableTextureComponent doneNamingButton;

		// Token: 0x04001D91 RID: 7569
		public ClickableTextureComponent randomButton;

		// Token: 0x04001D92 RID: 7570
		public ClickableTextureComponent upArrow;

		// Token: 0x04001D93 RID: 7571
		public ClickableTextureComponent downArrow;

		// Token: 0x04001D94 RID: 7572
		public ClickableTextureComponent hovered;

		// Token: 0x04001D95 RID: 7573
		public ClickableComponent textBoxCC;

		// Token: 0x04001D96 RID: 7574
		public bool onFarm;

		// Token: 0x04001D97 RID: 7575
		public bool namingAnimal;

		// Token: 0x04001D98 RID: 7576
		public bool freeze;

		// Token: 0x04001D99 RID: 7577
		public FarmAnimal animalBeingPurchased;

		// Token: 0x04001D9A RID: 7578
		public TextBox textBox;

		// Token: 0x04001D9B RID: 7579
		public TextBoxEvent textBoxEvent;

		// Token: 0x04001D9C RID: 7580
		public Building newAnimalHome;

		// Token: 0x04001D9D RID: 7581
		public int priceOfAnimal;

		// Token: 0x04001D9E RID: 7582
		public bool readOnly;

		// Token: 0x04001D9F RID: 7583
		public int currentScroll;

		// Token: 0x04001DA0 RID: 7584
		public int scrollRows;

		// Token: 0x04001DA1 RID: 7585
		public GameLocation TargetLocation;
	}
}
