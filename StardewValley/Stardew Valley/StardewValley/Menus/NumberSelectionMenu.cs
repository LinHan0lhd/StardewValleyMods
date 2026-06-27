using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x0200028D RID: 653
	public class NumberSelectionMenu : IClickableMenu
	{
		// Token: 0x06002B28 RID: 11048 RVA: 0x0020A820 File Offset: 0x00208A20
		public NumberSelectionMenu(string message, NumberSelectionMenu.behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
		{
			Vector2 vector = Game1.dialogueFont.MeasureString(message);
			int menuWidth = Math.Max((int)vector.X, 600) + IClickableMenu.borderWidth * 2;
			int menuHeight = (int)vector.Y + IClickableMenu.borderWidth * 2 + 160;
			int menuX = (int)this.centerPosition.X - menuWidth / 2;
			int menuY = (int)this.centerPosition.Y - menuHeight / 2;
			base.initialize(menuX, menuY, menuWidth, menuHeight, false);
			this.message = message;
			this.price = price;
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.currentValue = defaultNumber;
			this.behaviorFunction = behaviorOnSelection;
			this.numberSelectedBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = this.xPositionOnScreen + IClickableMenu.borderWidth + 56,
				Y = this.yPositionOnScreen + IClickableMenu.borderWidth + this.height / 2,
				Text = (this.currentValue.ToString() ?? ""),
				numbersOnly = true,
				textLimit = (maxValue.ToString() ?? "").Length
			};
			this.numberSelectedBox.SelectMe();
			this.leftButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + this.height / 2, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102,
				upNeighborID = -99998
			};
			this.rightButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth + 64 + this.numberSelectedBox.Width, this.yPositionOnScreen + IClickableMenu.borderWidth + this.height / 2, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = 103,
				upNeighborID = -99998
			};
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 103,
				leftNeighborID = 102,
				rightNeighborID = 104,
				upNeighborID = -99998
			};
			this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 104,
				leftNeighborID = 103,
				upNeighborID = -99998
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06002B29 RID: 11049 RVA: 0x0020AB87 File Offset: 0x00208D87
		protected virtual Vector2 centerPosition
		{
			get
			{
				return new Vector2((float)(Game1.uiViewport.Width / 2), (float)(Game1.uiViewport.Height / 2));
			}
		}

		// Token: 0x06002B2A RID: 11050 RVA: 0x0020ABA8 File Offset: 0x00208DA8
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(102);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B2B RID: 11051 RVA: 0x0020ABC0 File Offset: 0x00208DC0
		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (b == Buttons.A && this.currentlySnappedComponent != null)
			{
				this.heldTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (this.heldTimer > 300)
				{
					int step_size = (int)Math.Pow(10.0, (double)((this.heldTimer - 300) / 3000));
					int myID = this.currentlySnappedComponent.myID;
					if (myID != 101)
					{
						if (myID == 102)
						{
							int tempNumber = this.currentValue + step_size;
							int max_affordable = int.MaxValue;
							if (this.price != -1 && this.price != 0)
							{
								max_affordable = Game1.player.Money / this.price;
							}
							tempNumber = Math.Min(tempNumber, Math.Min(this.maxValue, max_affordable));
							if (tempNumber != this.currentValue)
							{
								this.rightButton.scale = this.rightButton.baseScale;
								this.currentValue = tempNumber;
								this.numberSelectedBox.Text = (this.currentValue.ToString() ?? "");
								return;
							}
						}
					}
					else
					{
						int tempNumber2 = this.currentValue - step_size;
						tempNumber2 = Math.Max(tempNumber2, this.minValue);
						if (tempNumber2 != this.currentValue)
						{
							this.leftButton.scale = this.leftButton.baseScale;
							this.currentValue = tempNumber2;
							this.numberSelectedBox.Text = (this.currentValue.ToString() ?? "");
						}
					}
				}
			}
		}

		// Token: 0x06002B2C RID: 11052 RVA: 0x0020AD4C File Offset: 0x00208F4C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.leftButton.containsPoint(x, y))
			{
				int tempNumber = this.currentValue - 1;
				if (tempNumber >= this.minValue)
				{
					this.leftButton.scale = this.leftButton.baseScale;
					this.currentValue = tempNumber;
					this.numberSelectedBox.Text = (this.currentValue.ToString() ?? "");
					Game1.playSound("smallSelect", null);
				}
			}
			if (this.rightButton.containsPoint(x, y))
			{
				int tempNumber2 = this.currentValue + 1;
				if (tempNumber2 <= this.maxValue && (this.price == -1 || tempNumber2 * this.price <= Game1.player.Money))
				{
					this.rightButton.scale = this.rightButton.baseScale;
					this.currentValue = tempNumber2;
					this.numberSelectedBox.Text = (this.currentValue.ToString() ?? "");
					Game1.playSound("smallSelect", null);
				}
			}
			if (this.okButton.containsPoint(x, y))
			{
				if (this.currentValue > this.maxValue || this.currentValue < this.minValue)
				{
					this.currentValue = Math.Max(this.minValue, Math.Min(this.maxValue, this.currentValue));
					this.numberSelectedBox.Text = (this.currentValue.ToString() ?? "");
				}
				else
				{
					this.behaviorFunction(this.currentValue, this.price, Game1.player);
				}
				Game1.playSound("smallSelect", null);
			}
			if (this.cancelButton.containsPoint(x, y))
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect", null);
				Game1.player.canMove = true;
			}
			this.numberSelectedBox.Update();
		}

		// Token: 0x06002B2D RID: 11053 RVA: 0x0020AF37 File Offset: 0x00209137
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (key == Keys.Enter)
			{
				this.receiveLeftClick(this.okButton.bounds.Center.X, this.okButton.bounds.Center.Y, true);
			}
		}

		// Token: 0x06002B2E RID: 11054 RVA: 0x0020AF78 File Offset: 0x00209178
		public override void update(GameTime time)
		{
			base.update(time);
			this.currentValue = 0;
			if (this.numberSelectedBox.Text != null)
			{
				int.TryParse(this.numberSelectedBox.Text, out this.currentValue);
			}
			if (this.priceShake > 0)
			{
				this.priceShake -= time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.options.SnappyMenus && !Game1.oldPadState.IsButtonDown(Buttons.A))
			{
				this.heldTimer = 0;
			}
		}

		// Token: 0x06002B2F RID: 11055 RVA: 0x0020B000 File Offset: 0x00209200
		public override void performHoverAction(int x, int y)
		{
			if (this.okButton.containsPoint(x, y) && (this.price == -1 || this.currentValue > this.minValue))
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.2f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}
			if (this.cancelButton.containsPoint(x, y))
			{
				this.cancelButton.scale = Math.Min(this.cancelButton.scale + 0.02f, this.cancelButton.baseScale + 0.2f);
			}
			else
			{
				this.cancelButton.scale = Math.Max(this.cancelButton.scale - 0.02f, this.cancelButton.baseScale);
			}
			if (this.leftButton.containsPoint(x, y))
			{
				this.leftButton.scale = Math.Min(this.leftButton.scale + 0.02f, this.leftButton.baseScale + 0.2f);
			}
			else
			{
				this.leftButton.scale = Math.Max(this.leftButton.scale - 0.02f, this.leftButton.baseScale);
			}
			if (this.rightButton.containsPoint(x, y))
			{
				this.rightButton.scale = Math.Min(this.rightButton.scale + 0.02f, this.rightButton.baseScale + 0.2f);
				return;
			}
			this.rightButton.scale = Math.Max(this.rightButton.scale - 0.02f, this.rightButton.baseScale);
		}

		// Token: 0x06002B30 RID: 11056 RVA: 0x0020B1E0 File Offset: 0x002093E0
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			b.DrawString(Game1.dialogueFont, this.message, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2)), Game1.textColor);
			this.okButton.draw(b);
			this.cancelButton.draw(b);
			this.leftButton.draw(b);
			this.rightButton.draw(b);
			if (this.price != -1)
			{
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.price * this.currentValue), new Vector2((float)(this.rightButton.bounds.Right + 32 + ((this.priceShake > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(this.rightButton.bounds.Y + ((this.priceShake > 0) ? Game1.random.Next(-1, 2) : 0))), (this.currentValue * this.price > Game1.player.Money) ? Color.Red : Game1.textColor);
			}
			this.numberSelectedBox.Draw(b, true);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x06002B31 RID: 11057 RVA: 0x0020B383 File Offset: 0x00209583
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x04001CC6 RID: 7366
		public const int region_leftButton = 101;

		// Token: 0x04001CC7 RID: 7367
		public const int region_rightButton = 102;

		// Token: 0x04001CC8 RID: 7368
		public const int region_okButton = 103;

		// Token: 0x04001CC9 RID: 7369
		public const int region_cancelButton = 104;

		// Token: 0x04001CCA RID: 7370
		private string message;

		// Token: 0x04001CCB RID: 7371
		protected int price;

		// Token: 0x04001CCC RID: 7372
		protected int minValue;

		// Token: 0x04001CCD RID: 7373
		protected int maxValue;

		// Token: 0x04001CCE RID: 7374
		protected int currentValue;

		// Token: 0x04001CCF RID: 7375
		protected int priceShake;

		// Token: 0x04001CD0 RID: 7376
		protected int heldTimer;

		// Token: 0x04001CD1 RID: 7377
		private NumberSelectionMenu.behaviorOnNumberSelect behaviorFunction;

		// Token: 0x04001CD2 RID: 7378
		protected TextBox numberSelectedBox;

		// Token: 0x04001CD3 RID: 7379
		public ClickableTextureComponent leftButton;

		// Token: 0x04001CD4 RID: 7380
		public ClickableTextureComponent rightButton;

		// Token: 0x04001CD5 RID: 7381
		public ClickableTextureComponent okButton;

		// Token: 0x04001CD6 RID: 7382
		public ClickableTextureComponent cancelButton;

		// Token: 0x02000626 RID: 1574
		// (Invoke) Token: 0x06004462 RID: 17506
		public delegate void behaviorOnNumberSelect(int number, int price, Farmer who);
	}
}
