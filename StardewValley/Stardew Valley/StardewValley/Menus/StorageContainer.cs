using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x020002AD RID: 685
	public class StorageContainer : MenuWithInventory
	{
		// Token: 0x06002CBB RID: 11451 RVA: 0x00229E6C File Offset: 0x0022806C
		public StorageContainer(IList<Item> inventory, int capacity, int rows = 3, StorageContainer.behaviorOnItemChange itemChangeBehavior = null, InventoryMenu.highlightThisItem highlightMethod = null) : base(highlightMethod, true, true, 0, 0, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			this.itemChangeBehavior = itemChangeBehavior;
			int containerWidth = 64 * (capacity / rows);
			this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, this.yPositionOnScreen + 64, false, inventory, null, capacity, rows, 0, 0, true);
			for (int i = 0; i < this.ItemsToGrabMenu.actualInventory.Count; i++)
			{
				if (i >= this.ItemsToGrabMenu.actualInventory.Count - this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows)
				{
					this.ItemsToGrabMenu.inventory[i].downNeighborID = i + 53910;
				}
			}
			for (int j = 0; j < this.inventory.inventory.Count; j++)
			{
				this.inventory.inventory[j].myID = j + 53910;
				if (this.inventory.inventory[j].downNeighborID != -1)
				{
					this.inventory.inventory[j].downNeighborID += 53910;
				}
				if (this.inventory.inventory[j].rightNeighborID != -1)
				{
					this.inventory.inventory[j].rightNeighborID += 53910;
				}
				if (this.inventory.inventory[j].leftNeighborID != -1)
				{
					this.inventory.inventory[j].leftNeighborID += 53910;
				}
				if (this.inventory.inventory[j].upNeighborID != -1)
				{
					this.inventory.inventory[j].upNeighborID += 53910;
				}
				if (j < 12)
				{
					this.inventory.inventory[j].upNeighborID = this.ItemsToGrabMenu.actualInventory.Count - this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
				}
			}
			this.dropItemInvisibleButton.myID = -500;
			this.ItemsToGrabMenu.dropItemInvisibleButton.myID = -500;
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.setCurrentlySnappedComponentTo(53910);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002CBC RID: 11452 RVA: 0x0022A0DC File Offset: 0x002282DC
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int containerWidth = 64 * (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows);
			this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, this.yPositionOnScreen + 64, false, this.ItemsToGrabMenu.actualInventory, null, this.ItemsToGrabMenu.capacity, this.ItemsToGrabMenu.rows, 0, 0, true);
		}

		// Token: 0x06002CBD RID: 11453 RVA: 0x0022A158 File Offset: 0x00228358
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item old = base.heldItem;
			int oldStack = (old != null) ? old.Stack : -1;
			if (base.isWithinBounds(x, y))
			{
				base.receiveLeftClick(x, y, false);
				if (this.itemChangeBehavior == null && old == null && base.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					base.heldItem = this.ItemsToGrabMenu.tryToAddItem(base.heldItem, "Ship");
				}
			}
			bool sound = true;
			if (this.ItemsToGrabMenu.isWithinBounds(x, y))
			{
				base.heldItem = this.ItemsToGrabMenu.leftClick(x, y, base.heldItem, false);
				if ((base.heldItem != null && old == null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
				{
					if (this.itemChangeBehavior != null)
					{
						sound = this.itemChangeBehavior(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, true);
					}
					if (sound)
					{
						Game1.playSound("dwop", null);
					}
				}
				if ((base.heldItem == null && old != null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
				{
					Item tmp = base.heldItem;
					if (base.heldItem == null && this.ItemsToGrabMenu.getItemAt(x, y) != null && oldStack < this.ItemsToGrabMenu.getItemAt(x, y).Stack)
					{
						tmp = old.getOne();
						tmp.Stack = oldStack;
					}
					if (this.itemChangeBehavior != null)
					{
						sound = this.itemChangeBehavior(old, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), tmp, this, false);
					}
					if (sound)
					{
						Game1.playSound("Ship", null);
					}
				}
				Item heldItem = base.heldItem;
				if (heldItem != null && heldItem.IsRecipe)
				{
					base.heldItem.LearnRecipe(null);
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("newRecipe", null);
					base.heldItem = null;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(base.heldItem, false))
				{
					base.heldItem = null;
					if (this.itemChangeBehavior != null)
					{
						sound = this.itemChangeBehavior(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, true);
					}
					if (sound)
					{
						Game1.playSound("coin", null);
					}
				}
			}
			if (this.okButton.containsPoint(x, y) && this.readyToClose())
			{
				Game1.playSound("bigDeSelect", null);
				Game1.exitActiveMenu();
			}
			if (this.trashCan.containsPoint(x, y) && base.heldItem != null && base.heldItem.canBeTrashed())
			{
				Utility.trashItem(base.heldItem);
				base.heldItem = null;
			}
		}

		// Token: 0x06002CBE RID: 11454 RVA: 0x0022A460 File Offset: 0x00228660
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			int oldStack = (base.heldItem != null) ? base.heldItem.Stack : 0;
			Item old = base.heldItem;
			if (base.isWithinBounds(x, y))
			{
				base.receiveRightClick(x, y, true);
				if (this.itemChangeBehavior == null && old == null && base.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					base.heldItem = this.ItemsToGrabMenu.tryToAddItem(base.heldItem, "Ship");
				}
			}
			if (this.ItemsToGrabMenu.isWithinBounds(x, y))
			{
				base.heldItem = this.ItemsToGrabMenu.rightClick(x, y, base.heldItem, false, false);
				if ((base.heldItem != null && old == null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)) || (base.heldItem != null && old != null && base.heldItem.Equals(old) && base.heldItem.Stack != oldStack))
				{
					StorageContainer.behaviorOnItemChange behaviorOnItemChange = this.itemChangeBehavior;
					if (behaviorOnItemChange != null)
					{
						behaviorOnItemChange(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, true);
					}
					Game1.playSound("dwop", null);
				}
				if ((base.heldItem == null && old != null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
				{
					StorageContainer.behaviorOnItemChange behaviorOnItemChange2 = this.itemChangeBehavior;
					if (behaviorOnItemChange2 != null)
					{
						behaviorOnItemChange2(old, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), base.heldItem, this, false);
					}
					Game1.playSound("Ship", null);
				}
				Item heldItem = base.heldItem;
				if (heldItem != null && heldItem.IsRecipe)
				{
					base.heldItem.LearnRecipe(null);
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("newRecipe", null);
					base.heldItem = null;
					return;
				}
				if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(base.heldItem, false))
				{
					base.heldItem = null;
					Game1.playSound("coin", null);
					StorageContainer.behaviorOnItemChange behaviorOnItemChange3 = this.itemChangeBehavior;
					if (behaviorOnItemChange3 == null)
					{
						return;
					}
					behaviorOnItemChange3(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, true);
				}
			}
		}

		// Token: 0x06002CBF RID: 11455 RVA: 0x0022A6D6 File Offset: 0x002288D6
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.poof != null && this.poof.update(time))
			{
				this.poof = null;
			}
		}

		// Token: 0x06002CC0 RID: 11456 RVA: 0x0022A6FC File Offset: 0x002288FC
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.ItemsToGrabMenu.hover(x, y, base.heldItem);
		}

		// Token: 0x06002CC1 RID: 11457 RVA: 0x0022A71C File Offset: 0x0022891C
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			base.draw(b, false, false, -1, -1, -1);
			Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2, false, true, null, false, true, -1, -1, -1);
			this.ItemsToGrabMenu.draw(b);
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.poof;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			Item heldItem = base.heldItem;
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
			}
			base.drawMouse(b, false, -1);
			string descriptionTitle = this.ItemsToGrabMenu.descriptionTitle;
			if (descriptionTitle != null && descriptionTitle.Length > 1)
			{
				IClickableMenu.drawHoverText(b, this.ItemsToGrabMenu.descriptionTitle, Game1.smallFont, 32 + ((base.heldItem != null) ? 16 : -21), 32 + ((base.heldItem != null) ? 16 : -21), -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x04001E74 RID: 7796
		public InventoryMenu ItemsToGrabMenu;

		// Token: 0x04001E75 RID: 7797
		private TemporaryAnimatedSprite poof;

		// Token: 0x04001E76 RID: 7798
		private StorageContainer.behaviorOnItemChange itemChangeBehavior;

		// Token: 0x0200063D RID: 1597
		// (Invoke) Token: 0x060044C4 RID: 17604
		public delegate bool behaviorOnItemChange(Item i, int position, Item old, StorageContainer container, bool onRemoval = false);
	}
}
