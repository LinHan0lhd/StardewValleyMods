using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x020002A0 RID: 672
	public class QuestContainerMenu : MenuWithInventory
	{
		// Token: 0x06002BE7 RID: 11239 RVA: 0x00216D80 File Offset: 0x00214F80
		public QuestContainerMenu(IList<Item> inventory, int rows = 3, InventoryMenu.highlightThisItem highlight_method = null, Func<Item, int> stack_capacity_check = null, Action on_item_changed = null, Action on_confirm = null) : base(highlight_method, true, false, 0, 0, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			this.onItemChanged = (Action)Delegate.Combine(this.onItemChanged, on_item_changed);
			this.onConfirm = (Action)Delegate.Combine(this.onConfirm, on_confirm);
			int capacity = inventory.Count;
			int containerWidth = 64 * (capacity / rows);
			this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, this.yPositionOnScreen + 64, false, inventory, null, capacity, rows, 0, 0, true);
			this.stackCapacityCheck = stack_capacity_check;
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
				foreach (ClickableComponent clickableComponent in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
				{
					clickableComponent.rightNeighborID = this.okButton.myID;
				}
			}
			this.dropItemInvisibleButton.myID = -500;
			this.ItemsToGrabMenu.dropItemInvisibleButton.myID = -500;
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.setCurrentlySnappedComponentTo(53910);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002BE8 RID: 11240 RVA: 0x0021707C File Offset: 0x0021527C
		public virtual int GetDonatableAmount(Item item)
		{
			if (item == null)
			{
				return 0;
			}
			int stack_capacity = item.Stack;
			if (this.stackCapacityCheck != null)
			{
				stack_capacity = Math.Min(stack_capacity, this.stackCapacityCheck(item));
			}
			return stack_capacity;
		}

		// Token: 0x06002BE9 RID: 11241 RVA: 0x002170B4 File Offset: 0x002152B4
		public virtual Item TryToGrab(Item item, int amount)
		{
			int grabbed_amount = Math.Min(amount, item.Stack);
			if (grabbed_amount == 0)
			{
				return item;
			}
			Item taken_stack = item.getOne();
			taken_stack.Stack = grabbed_amount;
			item.Stack -= grabbed_amount;
			InventoryMenu.highlightThisItem highlight_method = this.inventory.highlightMethod;
			this.inventory.highlightMethod = new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems);
			Item leftover_items = this.inventory.tryToAddItem(taken_stack, "coin");
			this.inventory.highlightMethod = highlight_method;
			if (leftover_items != null)
			{
				item.Stack += leftover_items.Stack;
			}
			Action action = this.onItemChanged;
			if (action != null)
			{
				action();
			}
			if (item.Stack <= 0)
			{
				return null;
			}
			return item;
		}

		// Token: 0x06002BEA RID: 11242 RVA: 0x00217164 File Offset: 0x00215364
		public virtual Item TryToPlace(Item item, int amount)
		{
			int stack_capacity = Math.Min(amount, this.GetDonatableAmount(item));
			if (stack_capacity == 0)
			{
				return item;
			}
			Item donation_stack = item.getOne();
			donation_stack.Stack = stack_capacity;
			item.Stack -= stack_capacity;
			Item leftover_items = this.ItemsToGrabMenu.tryToAddItem(donation_stack, "Ship");
			if (leftover_items != null)
			{
				item.Stack += leftover_items.Stack;
			}
			Action action = this.onItemChanged;
			if (action != null)
			{
				action();
			}
			if (item.Stack <= 0)
			{
				return null;
			}
			return item;
		}

		// Token: 0x06002BEB RID: 11243 RVA: 0x002171E8 File Offset: 0x002153E8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (base.isWithinBounds(x, y))
			{
				Item clicked_item = this.inventory.getItemAt(x, y);
				if (clicked_item != null)
				{
					int clicked_index = this.inventory.getInventoryPositionOfClick(x, y);
					this.inventory.actualInventory[clicked_index] = this.TryToPlace(clicked_item, clicked_item.Stack);
				}
			}
			if (this.ItemsToGrabMenu.isWithinBounds(x, y))
			{
				Item clicked_item2 = this.ItemsToGrabMenu.getItemAt(x, y);
				if (clicked_item2 != null)
				{
					int clicked_index2 = this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y);
					this.ItemsToGrabMenu.actualInventory[clicked_index2] = this.TryToGrab(clicked_item2, clicked_item2.Stack);
				}
			}
			if (this.okButton.containsPoint(x, y) && this.readyToClose())
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x06002BEC RID: 11244 RVA: 0x002172A8 File Offset: 0x002154A8
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (base.isWithinBounds(x, y))
			{
				Item clicked_item = this.inventory.getItemAt(x, y);
				if (clicked_item != null)
				{
					int clicked_index = this.inventory.getInventoryPositionOfClick(x, y);
					this.inventory.actualInventory[clicked_index] = this.TryToPlace(clicked_item, 1);
				}
			}
			if (this.ItemsToGrabMenu.isWithinBounds(x, y))
			{
				Item clicked_item2 = this.ItemsToGrabMenu.getItemAt(x, y);
				if (clicked_item2 != null)
				{
					int clicked_index2 = this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y);
					this.ItemsToGrabMenu.actualInventory[clicked_index2] = this.TryToGrab(clicked_item2, 1);
				}
			}
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x0021733E File Offset: 0x0021553E
		protected override void cleanupBeforeExit()
		{
			Action action = this.onConfirm;
			if (action != null)
			{
				action();
			}
			base.cleanupBeforeExit();
		}

		// Token: 0x06002BEE RID: 11246 RVA: 0x00217357 File Offset: 0x00215557
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.ItemsToGrabMenu.hover(x, y, base.heldItem);
		}

		// Token: 0x06002BEF RID: 11247 RVA: 0x00217378 File Offset: 0x00215578
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			base.draw(b, false, false, -1, -1, -1);
			Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2, false, true, null, false, true, -1, -1, -1);
			this.ItemsToGrabMenu.draw(b);
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

		// Token: 0x04001DA2 RID: 7586
		public InventoryMenu ItemsToGrabMenu;

		// Token: 0x04001DA3 RID: 7587
		public Func<Item, int> stackCapacityCheck;

		// Token: 0x04001DA4 RID: 7588
		public Action onItemChanged;

		// Token: 0x04001DA5 RID: 7589
		public Action onConfirm;

		// Token: 0x0200062E RID: 1582
		public enum ChangeType
		{
			// Token: 0x04002EAA RID: 11946
			None,
			// Token: 0x04002EAB RID: 11947
			Place,
			// Token: 0x04002EAC RID: 11948
			Grab
		}
	}
}
