using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x02000289 RID: 649
	public class MenuWithInventory : IClickableMenu
	{
		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06002AFC RID: 11004 RVA: 0x00207EA8 File Offset: 0x002060A8
		// (set) Token: 0x06002AFD RID: 11005 RVA: 0x00207EB0 File Offset: 0x002060B0
		public Item heldItem
		{
			get
			{
				return this._heldItem;
			}
			set
			{
				if (value != null)
				{
					value.onDetachedFromParent();
				}
				this._heldItem = value;
			}
		}

		// Token: 0x06002AFE RID: 11006 RVA: 0x00207EC4 File Offset: 0x002060C4
		public MenuWithInventory(InventoryMenu.highlightThisItem highlighterMethod = null, bool okButton = false, bool trashCan = false, int inventoryXOffset = 0, int inventoryYOffset = 0, int menuOffsetHack = 0, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false) : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 + menuOffsetHack, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false)
		{
			if (this.yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				this.yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			}
			if (this.xPositionOnScreen < 0)
			{
				this.xPositionOnScreen = 0;
			}
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + inventoryYOffset;
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + inventoryXOffset, yPositionForInventory, false, null, highlighterMethod, -1, 3, 0, 0, true);
			this.HeldItemExitBehavior = heldItemExitBehavior;
			this.AllowExitWithHeldItem = allowExitWithHeldItem;
			if (okButton)
			{
				this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
				{
					myID = 4857,
					upNeighborID = 5948,
					leftNeighborID = 12
				};
			}
			if (trashCan)
			{
				this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
				{
					myID = 5948,
					downNeighborID = 4857,
					leftNeighborID = 12,
					upNeighborID = 106
				};
			}
			this.dropItemInvisibleButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionForInventory - 12, 64, 64), "")
			{
				myID = 107,
				rightNeighborID = 0
			};
		}

		// Token: 0x06002AFF RID: 11007 RVA: 0x00208130 File Offset: 0x00206330
		public void movePosition(int dx, int dy)
		{
			this.xPositionOnScreen += dx;
			this.yPositionOnScreen += dy;
			this.inventory.movePosition(dx, dy);
			if (this.okButton != null)
			{
				ClickableTextureComponent clickableTextureComponent = this.okButton;
				clickableTextureComponent.bounds.X = clickableTextureComponent.bounds.X + dx;
				ClickableTextureComponent clickableTextureComponent2 = this.okButton;
				clickableTextureComponent2.bounds.Y = clickableTextureComponent2.bounds.Y + dy;
			}
			if (this.trashCan != null)
			{
				ClickableTextureComponent clickableTextureComponent3 = this.trashCan;
				clickableTextureComponent3.bounds.X = clickableTextureComponent3.bounds.X + dx;
				ClickableTextureComponent clickableTextureComponent4 = this.trashCan;
				clickableTextureComponent4.bounds.Y = clickableTextureComponent4.bounds.Y + dy;
			}
			if (this.dropItemInvisibleButton != null)
			{
				ClickableComponent clickableComponent = this.dropItemInvisibleButton;
				clickableComponent.bounds.X = clickableComponent.bounds.X + dx;
				ClickableComponent clickableComponent2 = this.dropItemInvisibleButton;
				clickableComponent2.bounds.Y = clickableComponent2.bounds.Y + dy;
			}
		}

		// Token: 0x06002B00 RID: 11008 RVA: 0x002081FC File Offset: 0x002063FC
		public override bool readyToClose()
		{
			return this.AllowExitWithHeldItem || this.heldItem == null;
		}

		// Token: 0x06002B01 RID: 11009 RVA: 0x00208211 File Offset: 0x00206411
		protected override void cleanupBeforeExit()
		{
			this.RescueHeldItemOnExit();
			base.cleanupBeforeExit();
		}

		// Token: 0x06002B02 RID: 11010 RVA: 0x0020821F File Offset: 0x0020641F
		public override void emergencyShutDown()
		{
			this.RescueHeldItemOnExit();
			base.emergencyShutDown();
		}

		// Token: 0x06002B03 RID: 11011 RVA: 0x00208230 File Offset: 0x00206430
		protected void RescueHeldItemOnExit()
		{
			if (this.heldItem != null)
			{
				switch (this.HeldItemExitBehavior)
				{
				case ItemExitBehavior.ReturnToPlayer:
					this.heldItem = Game1.player.addItemToInventory(this.heldItem);
					break;
				case ItemExitBehavior.ReturnToMenu:
					this.heldItem = this.inventory.tryToAddItem(this.heldItem, "coin");
					break;
				case ItemExitBehavior.Discard:
					this.heldItem = null;
					break;
				}
				this.DropHeldItem();
			}
		}

		// Token: 0x06002B04 RID: 11012 RVA: 0x002082A8 File Offset: 0x002064A8
		public virtual void DropHeldItem()
		{
			if (this.heldItem == null)
			{
				return;
			}
			Game1.playSound("throwDownITem", null);
			int drop_direction = Game1.player.FacingDirection;
			ItemGrabMenu grabMenu = this as ItemGrabMenu;
			if (grabMenu != null && grabMenu.context is LibraryMuseum)
			{
				drop_direction = 2;
			}
			Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), drop_direction, null, -1, false);
			ItemGrabMenu.behaviorOnItemSelect onAddItem = this.inventory.onAddItem;
			if (onAddItem != null)
			{
				onAddItem(this.heldItem, Game1.player);
			}
			this.heldItem = null;
		}

		// Token: 0x06002B05 RID: 11013 RVA: 0x00208338 File Offset: 0x00206538
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			this.heldItem = this.inventory.leftClick(x, y, this.heldItem, playSound);
			if (!this.isWithinBounds(x, y) && this.readyToClose() && this.trashCan != null)
			{
				this.trashCan.containsPoint(x, y);
			}
			if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
			{
				base.exitThisMenu(true);
				Event currentEvent = Game1.currentLocation.currentEvent;
				if (currentEvent != null && currentEvent.CurrentCommand > 0)
				{
					Event currentEvent2 = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent2.CurrentCommand;
					currentEvent2.CurrentCommand = currentCommand + 1;
				}
				Game1.playSound("bigDeSelect", null);
			}
			if (this.trashCan != null && this.trashCan.containsPoint(x, y) && this.heldItem != null && this.heldItem.canBeTrashed())
			{
				Utility.trashItem(this.heldItem);
				this.heldItem = null;
			}
		}

		// Token: 0x06002B06 RID: 11014 RVA: 0x00208433 File Offset: 0x00206633
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.heldItem = this.inventory.rightClick(x, y, this.heldItem, playSound, false);
		}

		// Token: 0x06002B07 RID: 11015 RVA: 0x00208450 File Offset: 0x00206650
		public void receiveRightClickOnlyToolAttachments(int x, int y)
		{
			this.heldItem = this.inventory.rightClick(x, y, this.heldItem, true, true);
		}

		// Token: 0x06002B08 RID: 11016 RVA: 0x00208470 File Offset: 0x00206670
		public override void performHoverAction(int x, int y)
		{
			this.descriptionText = "";
			this.descriptionTitle = "";
			this.hoveredItem = this.inventory.hover(x, y, this.heldItem);
			this.hoverText = this.inventory.hoverText;
			this.hoverAmount = 0;
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
			if (this.trashCan != null)
			{
				if (this.trashCan.containsPoint(x, y))
				{
					if (this.trashCanLidRotation <= 0f)
					{
						Game1.playSound("trashcanlid", null);
					}
					this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + 0.06544985f, 1.5707964f);
					if (this.heldItem != null && Utility.getTrashReclamationPrice(this.heldItem, Game1.player) > 0)
					{
						this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
						this.hoverAmount = Utility.getTrashReclamationPrice(this.heldItem, Game1.player);
						return;
					}
				}
				else
				{
					this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - 0.06544985f, 0f);
				}
			}
		}

		// Token: 0x06002B09 RID: 11017 RVA: 0x002085E8 File Offset: 0x002067E8
		public override void update(GameTime time)
		{
			if (this.wiggleWordsTimer > 0)
			{
				this.wiggleWordsTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		// Token: 0x06002B0A RID: 11018 RVA: 0x0020861C File Offset: 0x0020681C
		public virtual void draw(SpriteBatch b, bool drawUpperPortion = true, bool drawDescriptionArea = true, int red = -1, int green = -1, int blue = -1)
		{
			if (this.trashCan != null)
			{
				this.trashCan.draw(b);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
			}
			if (drawUpperPortion)
			{
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, false, red, green, blue);
				base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256, false, red, green, blue);
				if (drawDescriptionArea)
				{
					base.drawVerticalUpperIntersectingPartition(b, this.xPositionOnScreen + 576, 328, red, green, blue);
					if (!this.descriptionText.Equals(""))
					{
						int xPosition = this.xPositionOnScreen + 576 + 42 + ((this.wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
						int yPosition = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32 + ((this.wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
						int max_height = 320;
						float scale = 0f;
						string parsed_text;
						do
						{
							if (scale == 0f)
							{
								scale = 1f;
							}
							else
							{
								scale -= 0.1f;
							}
							parsed_text = Game1.parseText(this.descriptionText, Game1.smallFont, (int)(224f / scale));
						}
						while (Game1.smallFont.MeasureString(parsed_text).Y > (float)max_height / scale && scale > 0.5f);
						if (red == -1)
						{
							Utility.drawTextWithShadow(b, parsed_text, Game1.smallFont, new Vector2((float)xPosition, (float)yPosition), Game1.textColor * 0.75f, scale, -1f, -1, -1, 1f, 3);
						}
						else
						{
							Utility.drawTextWithColoredShadow(b, parsed_text, Game1.smallFont, new Vector2((float)xPosition, (float)yPosition), Game1.textColor * 0.75f, Color.Black * 0.2f, scale, -1f, -1, -1, 3);
						}
					}
				}
			}
			else
			{
				Game1.drawDialogueBox(this.xPositionOnScreen - IClickableMenu.borderWidth / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64, this.width, this.height - (IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192), false, true, null, false, true, -1, -1, -1);
			}
			ClickableTextureComponent clickableTextureComponent = this.okButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.draw(b);
			}
			this.inventory.draw(b, red, green, blue);
		}

		// Token: 0x06002B0B RID: 11019 RVA: 0x002088F8 File Offset: 0x00206AF8
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			if (this.yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				this.yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			}
			if (this.xPositionOnScreen < 0)
			{
				this.xPositionOnScreen = 0;
			}
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16;
			string move_item_sound = this.inventory.moveItemSound;
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, yPositionForInventory, false, null, this.inventory.highlightMethod, -1, 3, 0, 0, true);
			this.inventory.moveItemSound = move_item_sound;
			if (this.okButton != null)
			{
				this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
			}
			if (this.trashCan != null)
			{
				this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), 4f, false);
			}
		}

		// Token: 0x06002B0C RID: 11020 RVA: 0x00208A6C File Offset: 0x00206C6C
		public override void draw(SpriteBatch b)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04001C9B RID: 7323
		public const int region_okButton = 4857;

		// Token: 0x04001C9C RID: 7324
		public const int region_trashCan = 5948;

		// Token: 0x04001C9D RID: 7325
		private Item _heldItem;

		// Token: 0x04001C9E RID: 7326
		public string descriptionText = "";

		// Token: 0x04001C9F RID: 7327
		public string hoverText = "";

		// Token: 0x04001CA0 RID: 7328
		public string descriptionTitle = "";

		// Token: 0x04001CA1 RID: 7329
		public InventoryMenu inventory;

		// Token: 0x04001CA2 RID: 7330
		public Item hoveredItem;

		// Token: 0x04001CA3 RID: 7331
		public int wiggleWordsTimer;

		// Token: 0x04001CA4 RID: 7332
		public int hoverAmount;

		// Token: 0x04001CA5 RID: 7333
		public ClickableTextureComponent okButton;

		// Token: 0x04001CA6 RID: 7334
		public ClickableTextureComponent trashCan;

		// Token: 0x04001CA7 RID: 7335
		public float trashCanLidRotation;

		// Token: 0x04001CA8 RID: 7336
		public ClickableComponent dropItemInvisibleButton;

		// Token: 0x04001CA9 RID: 7337
		public ItemExitBehavior HeldItemExitBehavior;

		// Token: 0x04001CAA RID: 7338
		public bool AllowExitWithHeldItem;
	}
}
