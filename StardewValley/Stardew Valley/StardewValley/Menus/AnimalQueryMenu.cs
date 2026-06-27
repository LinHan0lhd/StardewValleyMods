using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	// Token: 0x0200024D RID: 589
	public class AnimalQueryMenu : IClickableMenu
	{
		// Token: 0x0600272E RID: 10030 RVA: 0x001BB580 File Offset: 0x001B9780
		public AnimalQueryMenu(FarmAnimal animal) : base(Game1.uiViewport.Width / 2 - AnimalQueryMenu.width / 2, Game1.uiViewport.Height / 2 - AnimalQueryMenu.height / 2, AnimalQueryMenu.width, AnimalQueryMenu.height, false)
		{
			Game1.player.Halt();
			Game1.player.faceGeneralDirection(animal.Position, 0, false, false);
			AnimalQueryMenu.width = 384;
			if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
			{
				AnimalQueryMenu.width += 32;
			}
			AnimalQueryMenu.height = 512;
			this.animal = animal;
			this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			this.textBox.X = Game1.uiViewport.Width / 2 - 128 - 12;
			this.textBox.Y = this.yPositionOnScreen - 4 + 128;
			this.textBox.Width = 256;
			this.textBox.Height = 192;
			this.textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X, this.textBox.Y, this.textBox.Width, 64), "")
			{
				myID = 110,
				downNeighborID = 104
			};
			this.textBox.Text = animal.displayName;
			Game1.keyboardDispatcher.Subscriber = this.textBox;
			this.textBox.Selected = false;
			if (animal.parentId.Value != -1L)
			{
				FarmAnimal parent = Utility.getAnimal(animal.parentId.Value);
				if (parent != null)
				{
					this.parentName = parent.displayName;
				}
			}
			animal.makeSound();
			this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + AnimalQueryMenu.width + 4, this.yPositionOnScreen + AnimalQueryMenu.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101,
				upNeighborID = -99998
			};
			this.sellButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + AnimalQueryMenu.width + 4, this.yPositionOnScreen + AnimalQueryMenu.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 384, 16, 16), 4f, false)
			{
				myID = 103,
				downNeighborID = -99998,
				upNeighborID = 104
			};
			this.moveHomeButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + AnimalQueryMenu.width + 4, this.yPositionOnScreen + AnimalQueryMenu.height - 256 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(16, 384, 16, 16), 4f, false)
			{
				myID = 104,
				downNeighborID = 103,
				upNeighborID = 110
			};
			if (!animal.isBaby() && animal.CanHavePregnancy())
			{
				this.allowReproductionButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + AnimalQueryMenu.width + 16, this.yPositionOnScreen + AnimalQueryMenu.height - 128 - IClickableMenu.borderWidth + 8, 36, 36), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(animal.allowReproduction.Value ? 128 : 137, 393, 9, 9), 4f, false)
				{
					myID = 106,
					downNeighborID = 101,
					upNeighborID = 103
				};
			}
			this.love = new ClickableTextureComponent((Math.Round((double)animal.friendshipTowardFarmer.Value, 0) / 10.0).ToString() + "<", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 + 16, this.yPositionOnScreen - 32 + IClickableMenu.spaceToClearTopBorder + 256 - 32, AnimalQueryMenu.width - 128, 64), null, "Friendship", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(172, 512, 16, 16), 4f, false)
			{
				myID = 102
			};
			this.loveHover = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 192 - 32, AnimalQueryMenu.width, 64), "Friendship")
			{
				myID = 109
			};
			if (animal.homeInterior == null)
			{
				Utility.fixAllAnimals();
			}
			this.loveLevel = (double)((float)animal.friendshipTowardFarmer.Value / 1000f);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x0600272F RID: 10031 RVA: 0x001BBA3D File Offset: 0x001B9C3D
		public override bool shouldClampGamePadCursor()
		{
			return this.movingAnimal;
		}

		// Token: 0x06002730 RID: 10032 RVA: 0x001BBA45 File Offset: 0x001B9C45
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002731 RID: 10033 RVA: 0x001BBA5C File Offset: 0x001B9C5C
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (Game1.options.menuButton.Contains(new InputButton(key)) && (this.textBox == null || !this.textBox.Selected))
			{
				Game1.playSound("smallSelect", null);
				if (this.readyToClose())
				{
					Game1.exitActiveMenu();
					if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
					{
						this.animal.displayName = this.textBox.Text;
						this.animal.Name = this.textBox.Text;
						return;
					}
				}
				else if (this.movingAnimal)
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.prepareForReturnFromPlacement), 0.02f);
					return;
				}
			}
			else if (Game1.options.SnappyMenus && (!Game1.options.menuButton.Contains(new InputButton(key)) || this.textBox == null || !this.textBox.Selected))
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x06002732 RID: 10034 RVA: 0x001BBB7C File Offset: 0x001B9D7C
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.movingAnimal)
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

		// Token: 0x06002733 RID: 10035 RVA: 0x001BBC60 File Offset: 0x001B9E60
		public void finishedPlacingAnimal()
		{
			Game1.exitActiveMenu();
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear(null, 0.02f);
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_HomeChanged")));
			Game1.player.viewingLocation.Value = null;
		}

		// Token: 0x06002734 RID: 10036 RVA: 0x001BBCDC File Offset: 0x001B9EDC
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (this.movingAnimal)
			{
				if (this.okButton != null && this.okButton.containsPoint(x, y))
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.prepareForReturnFromPlacement), 0.02f);
					Game1.playSound("smallSelect", null);
				}
				Vector2 clickTile = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
				Farm f = Game1.getFarm();
				Building selection = f.getBuildingAt(clickTile);
				if (selection != null)
				{
					if (!this.animal.CanLiveIn(selection))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_CantLiveThere", this.animal.shortDisplayType()), true);
						return;
					}
					AnimalHouse selectedHome = (AnimalHouse)selection.GetIndoors();
					if (selectedHome.isFull())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_BuildingFull"), true);
						return;
					}
					if (selection.Equals(this.animal.home))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_AlreadyHome"), true);
						return;
					}
					AnimalHouse oldHome = (AnimalHouse)this.animal.homeInterior;
					if (oldHome.animals.Remove(this.animal.myID.Value) || f.animals.Remove(this.animal.myID.Value))
					{
						oldHome.animalsThatLiveHere.Remove(this.animal.myID.Value);
						selectedHome.adoptAnimal(this.animal);
					}
					this.animal.makeSound();
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.finishedPlacingAnimal), 0.02f);
					return;
				}
			}
			else if (this.confirmingSell)
			{
				if (this.yesButton.containsPoint(x, y))
				{
					Game1.player.Money += this.animal.getSellPrice();
					((AnimalHouse)this.animal.homeInterior).animalsThatLiveHere.Remove(this.animal.myID.Value);
					this.animal.health.Value = -1;
					if (this.animal.foundGrass != null && FarmAnimal.reservedGrass.Contains(this.animal.foundGrass))
					{
						FarmAnimal.reservedGrass.Remove(this.animal.foundGrass);
					}
					int numClouds = this.animal.Sprite.getWidth() / 2;
					for (int i = 0; i < numClouds; i++)
					{
						int nonRedness = Game1.random.Next(25, 200);
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, this.animal.Position + new Vector2((float)Game1.random.Next(-32, this.animal.Sprite.getWidth() * 3), (float)Game1.random.Next(-32, this.animal.GetBoundingBox().Height * 3)), new Color(255 - nonRedness, 255, 255 - nonRedness), 8, false, (float)(Game1.random.NextBool() ? 50 : Game1.random.Next(30, 200)), 0, 64, -1f, 64, Game1.random.NextBool() ? 0 : Game1.random.Next(0, 600))
							{
								scale = (float)Game1.random.Next(2, 5) * 0.25f,
								alpha = (float)Game1.random.Next(2, 5) * 0.25f,
								motion = new Vector2(0f, (float)(-(float)Game1.random.NextDouble()))
							}
						});
					}
					Game1.playSound("newRecipe", null);
					Game1.playSound("money", null);
					Game1.exitActiveMenu();
					return;
				}
				if (this.noButton.containsPoint(x, y))
				{
					this.confirmingSell = false;
					Game1.playSound("smallSelect", null);
					if (Game1.options.SnappyMenus)
					{
						this.currentlySnappedComponent = base.getComponentWithID(103);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
			}
			else
			{
				if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
				{
					Game1.exitActiveMenu();
					if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
					{
						this.animal.displayName = this.textBox.Text;
						this.animal.Name = this.textBox.Text;
					}
					Game1.playSound("smallSelect", null);
				}
				if (this.sellButton.containsPoint(x, y))
				{
					this.confirmingSell = true;
					this.yesButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.Width / 2 - 64 - 4, Game1.uiViewport.Height / 2 - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
					{
						myID = 111,
						rightNeighborID = 105
					};
					this.noButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.Width / 2 + 4, Game1.uiViewport.Height / 2 - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
					{
						myID = 105,
						leftNeighborID = 111
					};
					Game1.playSound("smallSelect", null);
					if (Game1.options.SnappyMenus)
					{
						this.populateClickableComponentList();
						this.currentlySnappedComponent = this.noButton;
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
				if (this.moveHomeButton.containsPoint(x, y))
				{
					Game1.playSound("smallSelect", null);
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.prepareForAnimalPlacement), 0.02f);
				}
				if (this.allowReproductionButton != null && this.allowReproductionButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6", null);
					this.animal.allowReproduction.Value = !this.animal.allowReproduction.Value;
					if (this.animal.allowReproduction.Value)
					{
						this.allowReproductionButton.sourceRect.X = 128;
					}
					else
					{
						this.allowReproductionButton.sourceRect.X = 137;
					}
				}
				this.textBox.Update();
			}
		}

		// Token: 0x06002735 RID: 10037 RVA: 0x001BC39C File Offset: 0x001BA59C
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return this.movingAnimal;
		}

		// Token: 0x06002736 RID: 10038 RVA: 0x001BC3A4 File Offset: 0x001BA5A4
		public void prepareForAnimalPlacement()
		{
			this.movingAnimal = true;
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation = Game1.getFarm();
			Game1.player.viewingLocation.Value = Game1.currentLocation.NameOrUniqueName;
			Game1.globalFadeToClear(null, 0.02f);
			this.okButton.bounds.X = Game1.uiViewport.Width - 128;
			this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
			Game1.currentLocation.resetForPlayerEntry();
			Game1.displayFarmer = false;
		}

		// Token: 0x06002737 RID: 10039 RVA: 0x001BC46C File Offset: 0x001BA66C
		public void prepareForReturnFromPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear(null, 0.02f);
			this.okButton.bounds.X = this.xPositionOnScreen + AnimalQueryMenu.width + 4;
			this.okButton.bounds.Y = this.yPositionOnScreen + AnimalQueryMenu.height - 64 - IClickableMenu.borderWidth;
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			this.movingAnimal = false;
			Game1.player.viewingLocation.Value = null;
		}

		// Token: 0x06002738 RID: 10040 RVA: 0x001BC513 File Offset: 0x001BA713
		public override bool readyToClose()
		{
			this.textBox.Selected = false;
			return base.readyToClose() && !this.movingAnimal && !Game1.globalFade;
		}

		// Token: 0x06002739 RID: 10041 RVA: 0x001BC53C File Offset: 0x001BA73C
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (this.readyToClose())
			{
				Game1.exitActiveMenu();
				if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
				{
					this.animal.displayName = this.textBox.Text;
					this.animal.Name = this.textBox.Text;
				}
				Game1.playSound("smallSelect", null);
				return;
			}
			if (this.movingAnimal)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.prepareForReturnFromPlacement), 0.02f);
			}
		}

		// Token: 0x0600273A RID: 10042 RVA: 0x001BC5E4 File Offset: 0x001BA7E4
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			if (this.movingAnimal)
			{
				Vector2 clickTile = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
				Farm f = Game1.getFarm();
				foreach (Building building in f.buildings)
				{
					building.color = Color.White;
				}
				Building selection = f.getBuildingAt(clickTile);
				if (selection != null)
				{
					if (this.animal.CanLiveIn(selection) && !((AnimalHouse)selection.GetIndoors()).isFull() && !selection.Equals(this.animal.home))
					{
						selection.color = Color.LightGreen * 0.8f;
					}
					else
					{
						selection.color = Color.Red * 0.8f;
					}
				}
			}
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
			if (this.sellButton != null)
			{
				if (this.sellButton.containsPoint(x, y))
				{
					this.sellButton.scale = Math.Min(4.1f, this.sellButton.scale + 0.05f);
					this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_Sell", this.animal.getSellPrice());
				}
				else
				{
					this.sellButton.scale = Math.Max(4f, this.sellButton.scale - 0.05f);
				}
			}
			if (this.moveHomeButton != null)
			{
				if (this.moveHomeButton.containsPoint(x, y))
				{
					this.moveHomeButton.scale = Math.Min(4.1f, this.moveHomeButton.scale + 0.05f);
					this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_Move");
				}
				else
				{
					this.moveHomeButton.scale = Math.Max(4f, this.moveHomeButton.scale - 0.05f);
				}
			}
			if (this.allowReproductionButton != null)
			{
				if (this.allowReproductionButton.containsPoint(x, y))
				{
					this.allowReproductionButton.scale = Math.Min(4.1f, this.allowReproductionButton.scale + 0.05f);
					this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_AllowReproduction");
				}
				else
				{
					this.allowReproductionButton.scale = Math.Max(4f, this.allowReproductionButton.scale - 0.05f);
				}
			}
			if (this.yesButton != null)
			{
				if (this.yesButton.containsPoint(x, y))
				{
					this.yesButton.scale = Math.Min(1.1f, this.yesButton.scale + 0.05f);
				}
				else
				{
					this.yesButton.scale = Math.Max(1f, this.yesButton.scale - 0.05f);
				}
			}
			if (this.noButton != null)
			{
				if (this.noButton.containsPoint(x, y))
				{
					this.noButton.scale = Math.Min(1.1f, this.noButton.scale + 0.05f);
					return;
				}
				this.noButton.scale = Math.Max(1f, this.noButton.scale - 0.05f);
			}
		}

		// Token: 0x0600273B RID: 10043 RVA: 0x001BC9A0 File Offset: 0x001BABA0
		public override void draw(SpriteBatch b)
		{
			if (!this.movingAnimal && !Game1.globalFade)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen + 128, AnimalQueryMenu.width, AnimalQueryMenu.height - 128, false, true, null, false, true, -1, -1, -1);
				this.textBox.Draw(b, true);
				int age = (this.animal.GetDaysOwned() + 1) / 28 + 1;
				string ageText;
				if (age > 1)
				{
					ageText = Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeN", age);
				}
				else
				{
					ageText = Game1.content.LoadString("Strings\\UI:AnimalQuery_Age1");
				}
				if (this.animal.isBaby())
				{
					ageText += Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeBaby");
				}
				Utility.drawTextWithShadow(b, ageText, Game1.smallFont, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				int yOffset = 0;
				if (this.parentName != null)
				{
					yOffset = 21;
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AnimalQuery_Parent", this.parentName), Game1.smallFont, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32), (float)(32 + this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
				int halfHeart = (int)((this.loveLevel * 1000.0 % 200.0 >= 100.0) ? (this.loveLevel * 1000.0 / 200.0) : -100.0);
				for (int i = 0; i < 5; i++)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 96 + 32 * i), (float)(yOffset + this.yPositionOnScreen - 32 + 320)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(211 + ((this.loveLevel * 1000.0 <= (double)((i + 1) * 195)) ? 7 : 0), 428, 7, 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					if (halfHeart == i)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 96 + 32 * i), (float)(yOffset + this.yPositionOnScreen - 32 + 320)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(211, 428, 4, 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
					}
				}
				Utility.drawTextWithShadow(b, Game1.parseText(this.animal.getMoodMessage(), Game1.smallFont, AnimalQueryMenu.width - IClickableMenu.spaceToClearSideBorder * 2 - 64), Game1.smallFont, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32), (float)(yOffset + this.yPositionOnScreen + 384 - 64 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				this.okButton.draw(b);
				this.sellButton.draw(b);
				this.moveHomeButton.draw(b);
				ClickableTextureComponent clickableTextureComponent = this.allowReproductionButton;
				if (clickableTextureComponent != null)
				{
					clickableTextureComponent.draw(b);
				}
				if (this.animal != null && this.animal.hasEatenAnimalCracker.Value && Game1.objectSpriteSheet_2 != null)
				{
					Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(this.xPositionOnScreen + AnimalQueryMenu.width) - 105.6f, (float)this.yPositionOnScreen + 224f), new Microsoft.Xna.Framework.Rectangle(16, 240, 16, 16), Color.White, 0f, Vector2.Zero, 4f, false, 0.89f, -1, -1, 0.35f);
				}
				if (this.confirmingSell)
				{
					if (!Game1.options.showClearBackgrounds)
					{
						b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					}
					Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 160, Game1.uiViewport.Height / 2 - 192, 320, 256, false, true, null, false, true, -1, -1, -1);
					string confirmText = Game1.content.LoadString("Strings\\UI:AnimalQuery_ConfirmSell");
					b.DrawString(Game1.dialogueFont, confirmText, new Vector2((float)(Game1.uiViewport.Width / 2) - Game1.dialogueFont.MeasureString(confirmText).X / 2f, (float)(Game1.uiViewport.Height / 2 - 96 + 8)), Game1.textColor);
					this.yesButton.draw(b);
					this.noButton.draw(b);
				}
				else
				{
					string text = this.hoverText;
					if (text != null && text.Length > 0)
					{
						IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
					}
				}
			}
			else if (!Game1.globalFade)
			{
				string s = Game1.content.LoadString("Strings\\UI:AnimalQuery_ChooseBuilding", this.animal.displayHouse, this.animal.displayType);
				Game1.drawDialogueBox(32, -64, (int)Game1.dialogueFont.MeasureString(s).X + IClickableMenu.borderWidth * 2 + 16, 128 + IClickableMenu.borderWidth * 2, false, true, null, false, true, -1, -1, -1);
				b.DrawString(Game1.dialogueFont, s, new Vector2((float)(32 + IClickableMenu.spaceToClearSideBorder * 2 + 8), 44f), Game1.textColor);
				this.okButton.draw(b);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x0400183F RID: 6207
		public const int region_okButton = 101;

		// Token: 0x04001840 RID: 6208
		public const int region_love = 102;

		// Token: 0x04001841 RID: 6209
		public const int region_sellButton = 103;

		// Token: 0x04001842 RID: 6210
		public const int region_moveHomeButton = 104;

		// Token: 0x04001843 RID: 6211
		public const int region_noButton = 105;

		// Token: 0x04001844 RID: 6212
		public const int region_allowReproductionButton = 106;

		// Token: 0x04001845 RID: 6213
		public const int region_loveHover = 109;

		// Token: 0x04001846 RID: 6214
		public const int region_textBoxCC = 110;

		// Token: 0x04001847 RID: 6215
		public new static int width = 384;

		// Token: 0x04001848 RID: 6216
		public new static int height = 512;

		// Token: 0x04001849 RID: 6217
		public FarmAnimal animal;

		// Token: 0x0400184A RID: 6218
		public TextBox textBox;

		// Token: 0x0400184B RID: 6219
		public ClickableTextureComponent okButton;

		// Token: 0x0400184C RID: 6220
		public ClickableTextureComponent love;

		// Token: 0x0400184D RID: 6221
		public ClickableTextureComponent sellButton;

		// Token: 0x0400184E RID: 6222
		public ClickableTextureComponent moveHomeButton;

		// Token: 0x0400184F RID: 6223
		public ClickableTextureComponent yesButton;

		// Token: 0x04001850 RID: 6224
		public ClickableTextureComponent noButton;

		// Token: 0x04001851 RID: 6225
		public ClickableTextureComponent allowReproductionButton;

		// Token: 0x04001852 RID: 6226
		public ClickableComponent loveHover;

		// Token: 0x04001853 RID: 6227
		public ClickableComponent textBoxCC;

		// Token: 0x04001854 RID: 6228
		public double loveLevel;

		// Token: 0x04001855 RID: 6229
		public bool confirmingSell;

		// Token: 0x04001856 RID: 6230
		public bool movingAnimal;

		// Token: 0x04001857 RID: 6231
		public string hoverText = "";

		// Token: 0x04001858 RID: 6232
		public string parentName;
	}
}
