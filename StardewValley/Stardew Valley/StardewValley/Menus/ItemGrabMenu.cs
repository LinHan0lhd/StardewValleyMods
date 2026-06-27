using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x0200027C RID: 636
	public class ItemGrabMenu : MenuWithInventory
	{
		// Token: 0x06002A02 RID: 10754 RVA: 0x001F3D64 File Offset: 0x001F1F64
		public ItemGrabMenu(IList<Item> inventory, object context = null) : base(null, true, true, 0, 0, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			this.context = context;
			this.ItemsToGrabMenu = new InventoryMenu(this.xPositionOnScreen + 32, this.yPositionOnScreen, false, inventory, null, -1, 3, 0, 0, true);
			this.trashCan.myID = 106;
			this.ItemsToGrabMenu.populateClickableComponentList();
			for (int i = 0; i < this.ItemsToGrabMenu.inventory.Count; i++)
			{
				if (this.ItemsToGrabMenu.inventory[i] != null)
				{
					this.ItemsToGrabMenu.inventory[i].myID += 53910;
					this.ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
					this.ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].fullyImmutable = true;
					if (i % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows) == 0)
					{
						this.ItemsToGrabMenu.inventory[i].leftNeighborID = this.dropItemInvisibleButton.myID;
					}
					if (i % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows) == this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows - 1)
					{
						this.ItemsToGrabMenu.inventory[i].rightNeighborID = this.trashCan.myID;
					}
				}
			}
			for (int j = 0; j < this.GetColumnCount(); j++)
			{
				InventoryMenu inventory2 = this.inventory;
				int? num;
				if (inventory2 == null)
				{
					num = null;
				}
				else
				{
					List<ClickableComponent> inventory3 = inventory2.inventory;
					num = ((inventory3 != null) ? new int?(inventory3.Count) : null);
				}
				int? num2 = num;
				int num3 = this.GetColumnCount();
				if (num2.GetValueOrDefault() >= num3 & num2 != null)
				{
					this.inventory.inventory[j].upNeighborID = (this.shippingBin ? 12598 : -7777);
				}
			}
			if (!this.shippingBin)
			{
				for (int k = 0; k < this.GetColumnCount() * 3; k++)
				{
					InventoryMenu inventory4 = this.inventory;
					bool flag;
					if (inventory4 == null)
					{
						flag = false;
					}
					else
					{
						List<ClickableComponent> inventory5 = inventory4.inventory;
						int? num2 = (inventory5 != null) ? new int?(inventory5.Count) : null;
						int num3 = k;
						flag = (num2.GetValueOrDefault() > num3 & num2 != null);
					}
					if (flag)
					{
						this.inventory.inventory[k].upNeighborID = -7777;
						this.inventory.inventory[k].upNeighborImmutable = true;
					}
				}
			}
			if (this.trashCan != null)
			{
				this.trashCan.leftNeighborID = 11;
			}
			if (this.okButton != null)
			{
				this.okButton.leftNeighborID = 11;
			}
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
			this.inventory.showGrayedOutSlots = true;
			this.SetupBorderNeighbors();
		}

		// Token: 0x06002A03 RID: 10755 RVA: 0x001F40E4 File Offset: 0x001F22E4
		public virtual void DropRemainingItems()
		{
			InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
			if (((itemsToGrabMenu != null) ? itemsToGrabMenu.actualInventory : null) == null)
			{
				return;
			}
			foreach (Item item in this.ItemsToGrabMenu.actualInventory)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
				}
			}
			this.ItemsToGrabMenu.actualInventory.Clear();
		}

		// Token: 0x06002A04 RID: 10756 RVA: 0x001F4178 File Offset: 0x001F2378
		public ItemGrabMenu(ItemGrabMenu menu) : this(menu.ItemsToGrabMenu.actualInventory, menu.reverseGrab, menu.showReceivingMenu, menu.inventory.highlightMethod, menu.behaviorFunction, menu.message, menu.behaviorOnItemGrab, false, menu.canExitOnKey, menu.playRightClickSound, menu.allowRightClick, menu.organizeButton != null, menu.source, menu.sourceItem, menu.whichSpecialButton, menu.context, menu.HeldItemExitBehavior, menu.AllowExitWithHeldItem)
		{
			this.setEssential(menu.essential, false);
			if (menu.currentlySnappedComponent != null)
			{
				this.setCurrentlySnappedComponentTo(menu.currentlySnappedComponent.myID);
				if (Game1.options.SnappyMenus)
				{
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			base.heldItem = menu.heldItem;
		}

		// Token: 0x06002A05 RID: 10757 RVA: 0x001F4244 File Offset: 0x001F2444
		public ItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, ItemGrabMenu.behaviorOnItemSelect behaviorOnItemSelectFunction, string message, ItemGrabMenu.behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false) : base(highlightFunction, true, true, 0, 0, 64, heldItemExitBehavior, allowExitWithHeldItem)
		{
			this.source = source;
			this.message = message;
			this.reverseGrab = reverseGrab;
			this.showReceivingMenu = showReceivingMenu;
			this.playRightClickSound = playRightClickSound;
			this.allowRightClick = allowRightClick;
			this.inventory.showGrayedOutSlots = true;
			this.sourceItem = sourceItem;
			this.whichSpecialButton = whichSpecialButton;
			this.context = context;
			if (sourceItem != null && Game1.currentLocation.objects.Values.Contains(sourceItem))
			{
				this._sourceItemInCurrentLocation = true;
			}
			else
			{
				this._sourceItemInCurrentLocation = false;
			}
			Chest sourceChest = sourceItem as Chest;
			if (sourceChest != null)
			{
				if (this.CanHaveColorPicker())
				{
					Chest itemToDrawColored = new Chest(true, sourceItem.ItemId);
					this.chestColorPicker = new DiscreteColorPicker(this.xPositionOnScreen, this.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, sourceChest.playerChoiceColor.Value, itemToDrawColored);
					itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
					this.colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f, false)
					{
						hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
						myID = 27346,
						downNeighborID = -99998,
						leftNeighborID = 53921,
						region = 15923
					};
				}
				if (source == 1 && (sourceChest.SpecialChestType == Chest.SpecialChestTypes.None || sourceChest.SpecialChestType == Chest.SpecialChestTypes.BigChest) && InventoryPage.ShouldShowJunimoNoteIcon())
				{
					this.junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 + -216, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f, false)
					{
						myID = 898,
						leftNeighborID = 11,
						downNeighborID = 106
					};
				}
			}
			if (whichSpecialButton == 1)
			{
				this.specialButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), 4f, false)
				{
					myID = 12485,
					downNeighborID = (showOrganizeButton ? 12952 : 5948),
					region = 15923,
					leftNeighborID = 53921
				};
				JunimoHut hut = context as JunimoHut;
				if (hut != null)
				{
					this.specialButton.sourceRect.X = (hut.noHarvest.Value ? 124 : 108);
				}
			}
			if (snapToBottom)
			{
				base.movePosition(0, Game1.uiViewport.Height - (this.yPositionOnScreen + this.height - IClickableMenu.spaceToClearTopBorder));
				this.snappedtoBottom = true;
			}
			if (source == 1)
			{
				Chest chest = sourceItem as Chest;
				if (chest != null && chest.GetActualCapacity() != 36)
				{
					int capacity = chest.GetActualCapacity();
					int rows = (capacity >= 70) ? 5 : 3;
					if (capacity < 9)
					{
						rows = 1;
					}
					int containerWidth = 64 * (capacity / rows);
					this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, this.yPositionOnScreen + ((capacity < 70) ? 64 : -21), false, inventory, highlightFunction, capacity, rows, 0, 0, true);
					if (chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
					{
						this.inventory.moveItemSound = "Ship";
					}
					if (rows > 3)
					{
						this.yPositionOnScreen += 42;
						this.inventory.SetPosition(this.inventory.xPositionOnScreen, this.inventory.yPositionOnScreen + 38 + 4);
						this.ItemsToGrabMenu.SetPosition(this.ItemsToGrabMenu.xPositionOnScreen - 32 + 8, this.ItemsToGrabMenu.yPositionOnScreen);
						this.storageSpaceTopBorderOffset = 20;
						this.trashCan.bounds.X = this.ItemsToGrabMenu.width + this.ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
						this.okButton.bounds.X = this.ItemsToGrabMenu.width + this.ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
						goto IL_4DE;
					}
					goto IL_4DE;
				}
			}
			this.ItemsToGrabMenu = new InventoryMenu(this.xPositionOnScreen + 32, this.yPositionOnScreen, false, inventory, highlightFunction, -1, 3, 0, 0, true);
			IL_4DE:
			this.ItemsToGrabMenu.populateClickableComponentList();
			for (int i = 0; i < this.ItemsToGrabMenu.inventory.Count; i++)
			{
				if (this.ItemsToGrabMenu.inventory[i] != null)
				{
					this.ItemsToGrabMenu.inventory[i].myID += 53910;
					this.ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
					this.ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
					this.ItemsToGrabMenu.inventory[i].fullyImmutable = true;
				}
			}
			this.behaviorFunction = behaviorOnItemSelectFunction;
			this.behaviorOnItemGrab = behaviorOnItemGrab;
			this.canExitOnKey = canBeExitedWithKey;
			if (showOrganizeButton)
			{
				this.fillStacksButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 - 64 - 16, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f, false)
				{
					myID = 12952,
					upNeighborID = ((this.colorPickerToggleButton != null) ? 27346 : ((this.specialButton != null) ? 12485 : -500)),
					downNeighborID = 106,
					leftNeighborID = 53921,
					region = 15923
				};
				this.organizeButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f, false)
				{
					myID = 106,
					upNeighborID = 12952,
					downNeighborID = 5948,
					leftNeighborID = 53921,
					region = 15923
				};
			}
			this.RepositionSideButtons();
			if (this.chestColorPicker != null)
			{
				this.discreteColorPickerCC = new List<ClickableComponent>();
				for (int j = 0; j < DiscreteColorPicker.totalColors; j++)
				{
					List<ClickableComponent> list = this.discreteColorPickerCC;
					ClickableComponent clickableComponent = new ClickableComponent(new Rectangle(this.chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + j * 9 * 4, this.chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36, 28), "");
					clickableComponent.myID = j + 4343;
					clickableComponent.rightNeighborID = ((j < DiscreteColorPicker.totalColors - 1) ? (j + 4343 + 1) : -1);
					clickableComponent.leftNeighborID = ((j > 0) ? (j + 4343 - 1) : -1);
					InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
					clickableComponent.downNeighborID = ((itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count > 0) ? 53910 : 0);
					list.Add(clickableComponent);
				}
			}
			if (this.organizeButton != null)
			{
				foreach (ClickableComponent clickableComponent2 in this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
				{
					clickableComponent2.rightNeighborID = this.organizeButton.myID;
				}
			}
			if (this.trashCan != null && this.inventory.inventory.Count >= 12 && this.inventory.inventory[11] != null)
			{
				this.inventory.inventory[11].rightNeighborID = 5948;
			}
			if (this.trashCan != null)
			{
				this.trashCan.leftNeighborID = 11;
			}
			if (this.okButton != null)
			{
				this.okButton.leftNeighborID = 11;
			}
			ClickableComponent top_right = this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault<ClickableComponent>();
			if (top_right != null)
			{
				if (this.organizeButton != null)
				{
					this.organizeButton.leftNeighborID = top_right.myID;
				}
				if (this.specialButton != null)
				{
					this.specialButton.leftNeighborID = top_right.myID;
				}
				if (this.fillStacksButton != null)
				{
					this.fillStacksButton.leftNeighborID = top_right.myID;
				}
				if (this.junimoNoteIcon != null)
				{
					this.junimoNoteIcon.leftNeighborID = top_right.myID;
				}
			}
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
			this.SetupBorderNeighbors();
		}

		// Token: 0x06002A06 RID: 10758 RVA: 0x001F4C0C File Offset: 0x001F2E0C
		public static ItemGrabMenu CreateOverflowMenu(IList<Item> items, ItemGrabMenu.behaviorOnItemSelect onCollectItem = null)
		{
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(items, null).setEssential(true, false);
			itemGrabMenu.inventory.showGrayedOutSlots = true;
			itemGrabMenu.inventory.onAddItem = onCollectItem;
			itemGrabMenu.source = 4;
			return itemGrabMenu;
		}

		// Token: 0x06002A07 RID: 10759 RVA: 0x001F4C3C File Offset: 0x001F2E3C
		public virtual void RepositionSideButtons()
		{
			List<ClickableComponent> side_buttons = new List<ClickableComponent>();
			int slotsPerRow = this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
			if (this.organizeButton != null)
			{
				this.organizeButton.leftNeighborID = slotsPerRow - 1 + 53910;
				side_buttons.Add(this.organizeButton);
			}
			if (this.fillStacksButton != null)
			{
				this.fillStacksButton.leftNeighborID = slotsPerRow - 1 + 53910;
				side_buttons.Add(this.fillStacksButton);
			}
			if (this.colorPickerToggleButton != null)
			{
				this.colorPickerToggleButton.leftNeighborID = slotsPerRow - 1 + 53910;
				side_buttons.Add(this.colorPickerToggleButton);
			}
			if (this.specialButton != null)
			{
				side_buttons.Add(this.specialButton);
			}
			if (this.junimoNoteIcon != null)
			{
				this.junimoNoteIcon.leftNeighborID = slotsPerRow - 1;
				side_buttons.Add(this.junimoNoteIcon);
			}
			int step_size = 80;
			if (side_buttons.Count >= 4)
			{
				step_size = 72;
			}
			for (int i = 0; i < side_buttons.Count; i++)
			{
				ClickableComponent button = side_buttons[i];
				if (i > 0 && side_buttons.Count > 1)
				{
					button.downNeighborID = side_buttons[i - 1].myID;
				}
				if (i < side_buttons.Count - 1 && side_buttons.Count > 1)
				{
					button.upNeighborID = side_buttons[i + 1].myID;
				}
				button.bounds.X = this.ItemsToGrabMenu.xPositionOnScreen + this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2;
				button.bounds.Y = this.ItemsToGrabMenu.yPositionOnScreen + this.height / 3 - 64 - step_size * i;
			}
		}

		// Token: 0x06002A08 RID: 10760 RVA: 0x001F4DE4 File Offset: 0x001F2FE4
		public void SetupBorderNeighbors()
		{
			List<ClickableComponent> border = this.inventory.GetBorder(InventoryMenu.BorderSide.Right);
			foreach (ClickableComponent clickableComponent in border)
			{
				clickableComponent.rightNeighborID = -99998;
				clickableComponent.rightNeighborImmutable = true;
			}
			border = this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right);
			bool has_organizational_buttons = false;
			using (List<ClickableComponent>.Enumerator enumerator = this.allClickableComponents.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.region == 15923)
					{
						has_organizational_buttons = true;
						break;
					}
				}
			}
			foreach (ClickableComponent slot in border)
			{
				if (has_organizational_buttons)
				{
					slot.rightNeighborID = -99998;
					slot.rightNeighborImmutable = true;
				}
				else
				{
					slot.rightNeighborID = -1;
				}
			}
			int i = 0;
			while (i < this.GetColumnCount())
			{
				InventoryMenu inventory = this.inventory;
				bool flag;
				if (inventory == null)
				{
					flag = false;
				}
				else
				{
					List<ClickableComponent> inventory2 = inventory.inventory;
					int? num = (inventory2 != null) ? new int?(inventory2.Count) : null;
					int num2 = 12;
					flag = (num.GetValueOrDefault() >= num2 & num != null);
				}
				if (flag)
				{
					ClickableComponent clickableComponent2 = this.inventory.inventory[i];
					int upNeighborID;
					if (!this.shippingBin)
					{
						if (this.discreteColorPickerCC != null)
						{
							InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
							if (itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count <= i && Game1.player.showChestColorPicker)
							{
								upNeighborID = 4343;
								goto IL_1B0;
							}
						}
						upNeighborID = ((this.ItemsToGrabMenu.inventory.Count > i) ? (53910 + i) : 53910);
					}
					else
					{
						upNeighborID = 12598;
					}
					IL_1B0:
					clickableComponent2.upNeighborID = upNeighborID;
				}
				if (this.discreteColorPickerCC == null)
				{
					goto IL_204;
				}
				InventoryMenu itemsToGrabMenu2 = this.ItemsToGrabMenu;
				if (itemsToGrabMenu2 == null || itemsToGrabMenu2.inventory.Count <= i || !Game1.player.showChestColorPicker)
				{
					goto IL_204;
				}
				this.ItemsToGrabMenu.inventory[i].upNeighborID = 4343;
				IL_21C:
				i++;
				continue;
				IL_204:
				this.ItemsToGrabMenu.inventory[i].upNeighborID = -1;
				goto IL_21C;
			}
			if (!this.shippingBin)
			{
				for (int j = 0; j < 36; j++)
				{
					InventoryMenu inventory3 = this.inventory;
					bool flag2;
					if (inventory3 == null)
					{
						flag2 = false;
					}
					else
					{
						List<ClickableComponent> inventory4 = inventory3.inventory;
						int? num = (inventory4 != null) ? new int?(inventory4.Count) : null;
						int num2 = j;
						flag2 = (num.GetValueOrDefault() > num2 & num != null);
					}
					if (flag2)
					{
						this.inventory.inventory[j].upNeighborID = -7777;
						this.inventory.inventory[j].upNeighborImmutable = true;
					}
				}
			}
		}

		// Token: 0x06002A09 RID: 10761 RVA: 0x001F50E8 File Offset: 0x001F32E8
		public virtual bool CanHaveColorPicker()
		{
			if (this.source == 1)
			{
				Chest chest = this.sourceItem as Chest;
				if (chest != null && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
				{
					return !chest.fridge.Value;
				}
			}
			return false;
		}

		// Token: 0x06002A0A RID: 10762 RVA: 0x001F512E File Offset: 0x001F332E
		public virtual int GetColumnCount()
		{
			return this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
		}

		// Token: 0x06002A0B RID: 10763 RVA: 0x001F5147 File Offset: 0x001F3347
		public ItemGrabMenu setEssential(bool essential, bool superEssential = false)
		{
			this.essential = (essential || superEssential);
			this.superEssential = superEssential;
			return this;
		}

		// Token: 0x06002A0C RID: 10764 RVA: 0x001F515C File Offset: 0x001F335C
		public void initializeShippingBin()
		{
			this.shippingBin = true;
			this.lastShippedHolder = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width / 2 - 48, this.yPositionOnScreen + this.height / 2 - 80 - 64, 96, 96), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem"), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f, false)
			{
				myID = 12598,
				region = 12598
			};
			for (int i = 0; i < this.GetColumnCount(); i++)
			{
				InventoryMenu inventory = this.inventory;
				int? num;
				if (inventory == null)
				{
					num = null;
				}
				else
				{
					List<ClickableComponent> inventory2 = inventory.inventory;
					num = ((inventory2 != null) ? new int?(inventory2.Count) : null);
				}
				int? num2 = num;
				int columnCount = this.GetColumnCount();
				if (num2.GetValueOrDefault() >= columnCount & num2 != null)
				{
					this.inventory.inventory[i].upNeighborID = -7777;
					if (i == 11)
					{
						this.inventory.inventory[i].rightNeighborID = 5948;
					}
				}
			}
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A0D RID: 10765 RVA: 0x001F52B4 File Offset: 0x001F34B4
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (direction != 0)
			{
				if (direction == 2)
				{
					for (int i = 0; i < 12; i++)
					{
						InventoryMenu inventory = this.inventory;
						int? num;
						if (inventory == null)
						{
							num = null;
						}
						else
						{
							List<ClickableComponent> inventory2 = inventory.inventory;
							num = ((inventory2 != null) ? new int?(inventory2.Count) : null);
						}
						int? num2 = num;
						int columnCount = this.GetColumnCount();
						if ((num2.GetValueOrDefault() >= columnCount & num2 != null) && this.shippingBin)
						{
							this.inventory.inventory[i].upNeighborID = (this.shippingBin ? 12598 : (Math.Min(i, this.ItemsToGrabMenu.inventory.Count - 1) + 53910));
						}
					}
					if (!this.shippingBin && oldID >= 53910)
					{
						int index = oldID - 53910;
						if (index + this.GetColumnCount() <= this.ItemsToGrabMenu.inventory.Count - 1)
						{
							this.currentlySnappedComponent = base.getComponentWithID(index + this.GetColumnCount() + 53910);
							this.snapCursorToCurrentSnappedComponent();
							return;
						}
					}
					if (this.inventory != null)
					{
						int inventoryRowLength = this.inventory.capacity / this.inventory.rows;
						int diff = this.GetColumnCount() - inventoryRowLength;
						this.currentlySnappedComponent = base.getComponentWithID((oldRegion == 12598) ? 0 : Math.Max(0, Math.Min((oldID - 53910) % this.GetColumnCount() - diff / 2, this.inventory.capacity / this.inventory.rows - diff / 2)));
					}
					else
					{
						this.currentlySnappedComponent = base.getComponentWithID((oldRegion == 12598) ? 0 : ((oldID - 53910) % this.GetColumnCount()));
					}
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
			}
			else
			{
				if (this.shippingBin && Game1.getFarm().lastItemShipped != null && oldID < 12)
				{
					this.currentlySnappedComponent = base.getComponentWithID(12598);
					this.currentlySnappedComponent.downNeighborID = oldID;
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
				if (oldID < 53910 && oldID >= 12)
				{
					this.currentlySnappedComponent = base.getComponentWithID(oldID - 12);
					return;
				}
				int id = oldID + this.GetColumnCount() * (this.ItemsToGrabMenu.rows - 1);
				int j = 0;
				while (j < 3 && this.ItemsToGrabMenu.inventory.Count <= id)
				{
					id -= this.GetColumnCount();
					j++;
				}
				if (this.showReceivingMenu)
				{
					if (id < 0)
					{
						if (this.ItemsToGrabMenu.inventory.Count > 0)
						{
							this.currentlySnappedComponent = base.getComponentWithID(53910 + this.ItemsToGrabMenu.inventory.Count - 1);
						}
						else if (this.discreteColorPickerCC != null)
						{
							this.currentlySnappedComponent = base.getComponentWithID(4343);
						}
					}
					else
					{
						int inventoryRowLength2 = this.inventory.capacity / this.inventory.rows;
						int diff2 = this.GetColumnCount() - inventoryRowLength2;
						this.currentlySnappedComponent = base.getComponentWithID(id + 53910 + diff2 / 2);
						if (this.currentlySnappedComponent == null)
						{
							this.currentlySnappedComponent = base.getComponentWithID(53910);
						}
					}
				}
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002A0E RID: 10766 RVA: 0x001F55E4 File Offset: 0x001F37E4
		public override void snapToDefaultClickableComponent()
		{
			if (this.shippingBin)
			{
				this.currentlySnappedComponent = base.getComponentWithID(0);
			}
			else
			{
				if (this.source == 1)
				{
					Chest chest = this.sourceItem as Chest;
					if (chest != null && chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
					{
						this.currentlySnappedComponent = base.getComponentWithID(0);
						goto IL_76;
					}
				}
				this.currentlySnappedComponent = base.getComponentWithID((this.ItemsToGrabMenu.inventory.Count > 0 && this.showReceivingMenu) ? 53910 : 0);
			}
			IL_76:
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A0F RID: 10767 RVA: 0x001F5670 File Offset: 0x001F3870
		public void setSourceItem(Item item)
		{
			this.sourceItem = item;
			this.chestColorPicker = null;
			this.colorPickerToggleButton = null;
			if (this.CanHaveColorPicker())
			{
				Chest chest = this.sourceItem as Chest;
				if (chest != null)
				{
					Chest itemToDrawColored = new Chest(true, this.sourceItem.ItemId);
					this.chestColorPicker = new DiscreteColorPicker(this.xPositionOnScreen, this.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, itemToDrawColored);
					if (chest.SpecialChestType == Chest.SpecialChestTypes.BigChest)
					{
						this.chestColorPicker.yPositionOnScreen -= 42;
					}
					itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
					this.colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f, false)
					{
						hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker")
					};
				}
			}
			this.RepositionSideButtons();
		}

		// Token: 0x06002A10 RID: 10768 RVA: 0x001F5796 File Offset: 0x001F3996
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (direction != 1 || !this.ItemsToGrabMenu.inventory.Contains(a) || !this.inventory.inventory.Contains(b)) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x06002A11 RID: 10769 RVA: 0x001F57CD File Offset: 0x001F39CD
		public void setBackgroundTransparency(bool b)
		{
			this.drawBG = b;
		}

		// Token: 0x06002A12 RID: 10770 RVA: 0x001F57D6 File Offset: 0x001F39D6
		public void setDestroyItemOnClick(bool b)
		{
			this.destroyItemOnClick = b;
		}

		// Token: 0x06002A13 RID: 10771 RVA: 0x001F57E0 File Offset: 0x001F39E0
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!this.allowRightClick)
			{
				base.receiveRightClickOnlyToolAttachments(x, y);
				return;
			}
			base.receiveRightClick(x, y, playSound && this.playRightClickSound);
			if (base.heldItem == null && this.showReceivingMenu)
			{
				base.heldItem = this.ItemsToGrabMenu.rightClick(x, y, base.heldItem, false, false);
				if (base.heldItem != null && this.behaviorOnItemGrab != null)
				{
					this.behaviorOnItemGrab(base.heldItem, Game1.player);
					ItemGrabMenu itemGrabMenu = Game1.activeClickableMenu as ItemGrabMenu;
					if (itemGrabMenu != null)
					{
						itemGrabMenu.setSourceItem(this.sourceItem);
						if (Game1.options.SnappyMenus)
						{
							itemGrabMenu.currentlySnappedComponent = this.currentlySnappedComponent;
							itemGrabMenu.snapCursorToCurrentSnappedComponent();
						}
					}
				}
				Item heldItem = base.heldItem;
				if (((heldItem != null) ? heldItem.QualifiedItemId : null) == "(O)326")
				{
					base.heldItem = null;
					Game1.player.canUnderstandDwarves = true;
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("fireball", null);
					return;
				}
				Object obj = base.heldItem as Object;
				if (obj != null && ((obj != null) ? obj.QualifiedItemId : null) == "(O)434")
				{
					base.heldItem = null;
					base.exitThisMenu(false);
					Game1.player.eatObject(obj, true);
					return;
				}
				if (base.heldItem != null && base.heldItem.IsRecipe)
				{
					base.heldItem.LearnRecipe(null);
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("newRecipe", null);
					base.heldItem = null;
					return;
				}
				if (Game1.player.addItemToInventoryBool(base.heldItem, false))
				{
					base.heldItem = null;
					Game1.playSound("coin", null);
					return;
				}
			}
			else if (this.reverseGrab || this.behaviorFunction != null)
			{
				this.behaviorFunction(base.heldItem, Game1.player);
				ItemGrabMenu itemGrabMenu2 = Game1.activeClickableMenu as ItemGrabMenu;
				if (itemGrabMenu2 != null)
				{
					itemGrabMenu2.setSourceItem(this.sourceItem);
				}
				if (this.destroyItemOnClick)
				{
					base.heldItem = null;
					return;
				}
			}
		}

		// Token: 0x06002A14 RID: 10772 RVA: 0x001F5A68 File Offset: 0x001F3C68
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			if (this.snappedtoBottom)
			{
				base.movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.uiViewport.Height - (this.yPositionOnScreen + this.height - IClickableMenu.spaceToClearTopBorder));
			}
			else
			{
				base.movePosition((newBounds.Width - oldBounds.Width) / 2, (newBounds.Height - oldBounds.Height) / 2);
			}
			InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
			if (itemsToGrabMenu != null)
			{
				itemsToGrabMenu.gameWindowSizeChanged(oldBounds, newBounds);
			}
			this.RepositionSideButtons();
			if (this.CanHaveColorPicker())
			{
				Chest chest = this.sourceItem as Chest;
				if (chest != null)
				{
					this.chestColorPicker = new DiscreteColorPicker(this.xPositionOnScreen, this.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, new Chest(true, this.sourceItem.ItemId));
				}
			}
		}

		// Token: 0x06002A15 RID: 10773 RVA: 0x001F5B44 File Offset: 0x001F3D44
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, !this.destroyItemOnClick);
			if (this.shippingBin && this.lastShippedHolder.containsPoint(x, y))
			{
				if (Game1.getFarm().lastItemShipped != null)
				{
					Game1.getFarm().getShippingBin(Game1.player).Remove(Game1.getFarm().lastItemShipped);
					if (Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped, false))
					{
						Game1.playSound("coin", null);
						Game1.getFarm().lastItemShipped = null;
						if (Game1.player.ActiveObject != null)
						{
							Game1.player.showCarrying();
							Game1.player.Halt();
							return;
						}
					}
					else
					{
						Game1.getFarm().getShippingBin(Game1.player).Add(Game1.getFarm().lastItemShipped);
					}
				}
				return;
			}
			if (this.chestColorPicker != null)
			{
				this.chestColorPicker.receiveLeftClick(x, y, true);
				Chest chest = this.sourceItem as Chest;
				if (chest != null)
				{
					chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
				}
			}
			if (this.colorPickerToggleButton != null && this.colorPickerToggleButton.containsPoint(x, y))
			{
				Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
				this.chestColorPicker.visible = Game1.player.showChestColorPicker;
				try
				{
					Game1.playSound("drumkit6", null);
				}
				catch (Exception)
				{
				}
				this.SetupBorderNeighbors();
				return;
			}
			if (this.whichSpecialButton != -1 && this.specialButton != null && this.specialButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6", null);
				if (this.whichSpecialButton == 1)
				{
					JunimoHut hut = this.context as JunimoHut;
					if (hut != null)
					{
						hut.noHarvest.Value = !hut.noHarvest.Value;
						this.specialButton.sourceRect.X = (hut.noHarvest.Value ? 124 : 108);
					}
				}
				return;
			}
			if (base.heldItem == null && this.showReceivingMenu)
			{
				base.heldItem = this.ItemsToGrabMenu.leftClick(x, y, base.heldItem, false);
				if (base.heldItem != null && this.behaviorOnItemGrab != null)
				{
					this.behaviorOnItemGrab(base.heldItem, Game1.player);
					ItemGrabMenu itemGrabMenu = Game1.activeClickableMenu as ItemGrabMenu;
					if (itemGrabMenu != null)
					{
						itemGrabMenu.setSourceItem(this.sourceItem);
						if (Game1.options.SnappyMenus)
						{
							itemGrabMenu.currentlySnappedComponent = this.currentlySnappedComponent;
							itemGrabMenu.snapCursorToCurrentSnappedComponent();
						}
					}
				}
				Item heldItem = base.heldItem;
				string a = (heldItem != null) ? heldItem.QualifiedItemId : null;
				if (!(a == "(O)326"))
				{
					if (a == "(O)102")
					{
						base.heldItem = null;
						Game1.player.foundArtifact("102", 1);
						this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
						Game1.playSound("fireball", null);
					}
				}
				else
				{
					base.heldItem = null;
					Game1.player.canUnderstandDwarves = true;
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("fireball", null);
				}
				Object stardrop = base.heldItem as Object;
				if (stardrop != null && ((stardrop != null) ? stardrop.QualifiedItemId : null) == "(O)434")
				{
					base.heldItem = null;
					base.exitThisMenu(false);
					Game1.player.eatObject(stardrop, true);
				}
				else if (base.heldItem != null && base.heldItem.IsRecipe)
				{
					base.heldItem.LearnRecipe(null);
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % 64 + 16), (float)(y - y % 64 + 16)), false, false);
					Game1.playSound("newRecipe", null);
					base.heldItem = null;
				}
				else if (Game1.player.addItemToInventoryBool(base.heldItem, false))
				{
					base.heldItem = null;
					Game1.playSound("coin", null);
				}
			}
			else if ((this.reverseGrab || this.behaviorFunction != null) && this.isWithinBounds(x, y))
			{
				this.behaviorFunction(base.heldItem, Game1.player);
				ItemGrabMenu itemGrabMenu2 = Game1.activeClickableMenu as ItemGrabMenu;
				if (itemGrabMenu2 != null)
				{
					itemGrabMenu2.setSourceItem(this.sourceItem);
					if (Game1.options.SnappyMenus)
					{
						itemGrabMenu2.currentlySnappedComponent = this.currentlySnappedComponent;
						itemGrabMenu2.snapCursorToCurrentSnappedComponent();
					}
				}
				if (this.destroyItemOnClick)
				{
					base.heldItem = null;
					return;
				}
			}
			if (this.organizeButton != null && this.organizeButton.containsPoint(x, y))
			{
				ItemGrabMenu.organizeItemsInList(this.ItemsToGrabMenu.actualInventory);
				Game1.activeClickableMenu = new ItemGrabMenu(this);
				Game1.playSound("Ship", null);
				return;
			}
			if (this.fillStacksButton != null && this.fillStacksButton.containsPoint(x, y))
			{
				this.FillOutStacks();
				Game1.playSound("Ship", null);
				return;
			}
			if (this.junimoNoteIcon != null && this.junimoNoteIcon.containsPoint(x, y))
			{
				if (this.readyToClose())
				{
					Game1.activeClickableMenu = new JunimoNoteMenu(true, 1, false)
					{
						menuToReturnTo = this
					};
				}
				return;
			}
			if (base.heldItem != null && !this.isWithinBounds(x, y) && base.heldItem.canBeTrashed())
			{
				this.DropHeldItem();
			}
		}

		// Token: 0x06002A16 RID: 10774 RVA: 0x001F614C File Offset: 0x001F434C
		public void FillOutStacks()
		{
			IList<Item> playerInventory = this.inventory.actualInventory;
			IList<Item> chestInventory = this.ItemsToGrabMenu.actualInventory;
			HashSet<int> affectedChestSlots = new HashSet<int>();
			ILookup<string, Item> prevChestItemsById = (from item in chestInventory
			where item != null
			select item).ToLookup((Item item) => item.QualifiedItemId);
			if (prevChestItemsById.Count == 0)
			{
				return;
			}
			for (int playerIndex = 0; playerIndex < playerInventory.Count; playerIndex++)
			{
				Item playerItem = playerInventory[playerIndex];
				if (playerItem != null)
				{
					bool canStack = false;
					foreach (Item item2 in prevChestItemsById[playerItem.QualifiedItemId])
					{
						canStack = item2.canStackWith(playerItem);
						if (canStack)
						{
							break;
						}
					}
					if (canStack)
					{
						Item originalPlayerItem = playerItem;
						bool mergedAny = false;
						int firstEmptyChestIndex = -1;
						for (int chestIndex = 0; chestIndex < chestInventory.Count; chestIndex++)
						{
							Item chestSlot = chestInventory[chestIndex];
							if (chestSlot == null)
							{
								if (firstEmptyChestIndex == -1)
								{
									firstEmptyChestIndex = chestIndex;
								}
							}
							else if (chestSlot.canStackWith(playerItem))
							{
								int toRemove = playerItem.Stack - chestSlot.addToStack(playerItem);
								if (toRemove > 0)
								{
									mergedAny = true;
									affectedChestSlots.Add(chestIndex);
									playerItem = playerItem.ConsumeStack(toRemove);
									if (playerItem == null)
									{
										playerInventory[playerIndex] = null;
										break;
									}
								}
							}
						}
						if (playerItem != null)
						{
							if (firstEmptyChestIndex == -1 && chestInventory.Count < this.ItemsToGrabMenu.capacity)
							{
								firstEmptyChestIndex = chestInventory.Count;
								chestInventory.Add(null);
							}
							if (firstEmptyChestIndex > -1)
							{
								mergedAny = true;
								affectedChestSlots.Add(firstEmptyChestIndex);
								playerItem.onDetachedFromParent();
								chestInventory[firstEmptyChestIndex] = playerItem;
								playerInventory[playerIndex] = null;
							}
						}
						if (mergedAny)
						{
							ItemGrabMenu.TransferredItemSprite itemSprite = new ItemGrabMenu.TransferredItemSprite(originalPlayerItem.getOne(), this.inventory.inventory[playerIndex].bounds.X, this.inventory.inventory[playerIndex].bounds.Y);
							this._transferredItemSprites.Add(itemSprite);
						}
					}
				}
			}
			foreach (int slotIndex in affectedChestSlots)
			{
				this.ItemsToGrabMenu.ShakeItem(slotIndex);
			}
		}

		// Token: 0x06002A17 RID: 10775 RVA: 0x001F63C8 File Offset: 0x001F45C8
		public static void organizeItemsInList(IList<Item> items)
		{
			List<Item> copy = new List<Item>(items);
			List<Item> tools = new List<Item>();
			copy.RemoveAll(delegate(Item item)
			{
				if (item == null)
				{
					return true;
				}
				if (!(item is Tool))
				{
					return false;
				}
				tools.Add(item);
				return true;
			});
			for (int i = 0; i < copy.Count; i++)
			{
				Item current_item = copy[i];
				if (current_item.getRemainingStackSpace() > 0)
				{
					for (int j = i + 1; j < copy.Count; j++)
					{
						Item other_item = copy[j];
						if (current_item.canStackWith(other_item))
						{
							other_item.Stack = current_item.addToStack(other_item);
							if (other_item.Stack == 0)
							{
								copy.RemoveAt(j);
								j--;
							}
						}
					}
				}
			}
			copy.Sort();
			copy.InsertRange(0, tools);
			for (int k = 0; k < items.Count; k++)
			{
				items[k] = null;
			}
			for (int l = 0; l < copy.Count; l++)
			{
				items[l] = copy[l];
			}
		}

		// Token: 0x06002A18 RID: 10776 RVA: 0x001F64CC File Offset: 0x001F46CC
		public bool areAllItemsTaken()
		{
			for (int i = 0; i < this.ItemsToGrabMenu.actualInventory.Count; i++)
			{
				if (this.ItemsToGrabMenu.actualInventory[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002A19 RID: 10777 RVA: 0x001F650C File Offset: 0x001F470C
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button != Buttons.Back)
			{
				if (button != Buttons.LeftShoulder)
				{
					if (button != Buttons.RightShoulder)
					{
						return;
					}
					ClickableComponent fill_stacks_component = base.getComponentWithID(12952);
					if (fill_stacks_component != null)
					{
						this.setCurrentlySnappedComponentTo(fill_stacks_component.myID);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
					int highest_y = -1;
					ClickableComponent highest_component = null;
					foreach (ClickableComponent component in this.allClickableComponents)
					{
						if (component.region == 15923 && (highest_y == -1 || component.bounds.Y < highest_y))
						{
							highest_y = component.bounds.Y;
							highest_component = component;
						}
					}
					if (highest_component != null)
					{
						this.setCurrentlySnappedComponentTo(highest_component.myID);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
				else if (!this.shippingBin)
				{
					ClickableComponent component2 = base.getComponentWithID(53910);
					if (component2 != null)
					{
						this.setCurrentlySnappedComponentTo(component2.myID);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
					component2 = base.getComponentWithID(0);
					if (component2 != null)
					{
						this.setCurrentlySnappedComponentTo(0);
						this.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			else if (this.organizeButton != null)
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.Items);
				Game1.playSound("Ship", null);
				return;
			}
		}

		// Token: 0x06002A1A RID: 10778 RVA: 0x001F6660 File Offset: 0x001F4860
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				base.applyMovementKey(key);
			}
			if ((this.canExitOnKey || this.areAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				base.exitThisMenu(true);
				Event currentEvent = Game1.currentLocation.currentEvent;
				if (currentEvent != null && currentEvent.CurrentCommand > 0)
				{
					Event currentEvent2 = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent2.CurrentCommand;
					currentEvent2.CurrentCommand = currentCommand + 1;
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && base.heldItem != null)
			{
				Game1.setMousePosition(this.trashCan.bounds.Center);
			}
			if (key == Keys.Delete && base.heldItem != null && base.heldItem.canBeTrashed())
			{
				Utility.trashItem(base.heldItem);
				base.heldItem = null;
			}
		}

		// Token: 0x06002A1B RID: 10779 RVA: 0x001F6758 File Offset: 0x001F4958
		public override void update(GameTime time)
		{
			base.update(time);
			if (!this.HasUpdateTicked)
			{
				this.HasUpdateTicked = true;
				if (this.source == 4)
				{
					IList<Item> items = this.ItemsToGrabMenu.actualInventory;
					for (int i = 0; i < items.Count; i++)
					{
						Item item = items[i];
						if (((item != null) ? item.QualifiedItemId : null) == "(O)434")
						{
							List<Item> remainingItems = new List<Item>(items);
							remainingItems.RemoveAt(i);
							remainingItems.RemoveAll((Item p) => p == null);
							if (remainingItems.Count > 0)
							{
								Game1.nextClickableMenu.Insert(0, ItemGrabMenu.CreateOverflowMenu(remainingItems, this.inventory.onAddItem));
							}
							this.essential = false;
							this.superEssential = false;
							base.exitThisMenu(false);
							Game1.player.eatObject(items[i] as Object, true);
							return;
						}
					}
				}
			}
			if (this.poof != null && this.poof.update(time))
			{
				this.poof = null;
			}
			DiscreteColorPicker discreteColorPicker = this.chestColorPicker;
			if (discreteColorPicker != null)
			{
				discreteColorPicker.update(time);
			}
			Chest chest = this.sourceItem as Chest;
			if (chest != null && this._sourceItemInCurrentLocation)
			{
				Vector2 tileLocation = chest.tileLocation.Value;
				if (tileLocation != Vector2.Zero && !Game1.currentLocation.objects.ContainsKey(tileLocation))
				{
					if (Game1.activeClickableMenu != null)
					{
						Game1.activeClickableMenu.emergencyShutDown();
					}
					Game1.exitActiveMenu();
				}
			}
			this._transferredItemSprites.RemoveAll((ItemGrabMenu.TransferredItemSprite sprite) => sprite.Update(time));
		}

		// Token: 0x06002A1C RID: 10780 RVA: 0x001F6918 File Offset: 0x001F4B18
		public override void performHoverAction(int x, int y)
		{
			this.hoveredItem = null;
			this.hoverText = "";
			base.performHoverAction(x, y);
			if (this.colorPickerToggleButton != null)
			{
				this.colorPickerToggleButton.tryHover(x, y, 0.25f);
				if (this.colorPickerToggleButton.containsPoint(x, y))
				{
					this.hoverText = this.colorPickerToggleButton.hoverText;
				}
			}
			if (this.organizeButton != null)
			{
				this.organizeButton.tryHover(x, y, 0.25f);
				if (this.organizeButton.containsPoint(x, y))
				{
					this.hoverText = this.organizeButton.hoverText;
				}
			}
			if (this.fillStacksButton != null)
			{
				this.fillStacksButton.tryHover(x, y, 0.25f);
				if (this.fillStacksButton.containsPoint(x, y))
				{
					this.hoverText = this.fillStacksButton.hoverText;
				}
			}
			ClickableTextureComponent clickableTextureComponent = this.specialButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.tryHover(x, y, 0.25f);
			}
			if (this.showReceivingMenu)
			{
				Item item_grab_hovered_item = this.ItemsToGrabMenu.hover(x, y, base.heldItem);
				if (item_grab_hovered_item != null)
				{
					this.hoveredItem = item_grab_hovered_item;
				}
			}
			if (this.junimoNoteIcon != null)
			{
				this.junimoNoteIcon.tryHover(x, y, 0.1f);
				if (this.junimoNoteIcon.containsPoint(x, y))
				{
					this.hoverText = this.junimoNoteIcon.hoverText;
				}
				if (GameMenu.bundleItemHovered)
				{
					this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale + (float)Math.Sin((double)((float)this.junimoNotePulser / 100f)) / 4f;
					this.junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
				else
				{
					this.junimoNotePulser = 0;
					this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale;
				}
			}
			if (this.hoverText != null)
			{
				return;
			}
			if (this.organizeButton != null)
			{
				this.hoverText = null;
				this.organizeButton.tryHover(x, y, 0.1f);
				if (this.organizeButton.containsPoint(x, y))
				{
					this.hoverText = this.organizeButton.hoverText;
				}
			}
			if (this.shippingBin)
			{
				this.hoverText = null;
				if (this.lastShippedHolder.containsPoint(x, y) && Game1.getFarm().lastItemShipped != null)
				{
					this.hoverText = this.lastShippedHolder.hoverText;
				}
			}
			DiscreteColorPicker discreteColorPicker = this.chestColorPicker;
			if (discreteColorPicker == null)
			{
				return;
			}
			discreteColorPicker.performHoverAction(x, y);
		}

		// Token: 0x06002A1D RID: 10781 RVA: 0x001F6B78 File Offset: 0x001F4D78
		public override void draw(SpriteBatch b)
		{
			if (this.drawBG && !Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			}
			base.draw(b, false, false, -1, -1, -1);
			if (this.showReceivingMenu)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen - 64), (float)(this.yPositionOnScreen + this.height / 2 + 64 + 16)), new Rectangle?(new Rectangle(16, 368, 12, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen - 64), (float)(this.yPositionOnScreen + this.height / 2 + 64 - 16)), new Rectangle?(new Rectangle(21, 368, 11, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen - 40), (float)(this.yPositionOnScreen + this.height / 2 + 64 - 44)), new Rectangle?(new Rectangle(4, 372, 8, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + this.storageSpaceTopBorderOffset, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2 - this.storageSpaceTopBorderOffset, false, true, null, false, true, -1, -1, -1);
				if (this.source == 1)
				{
					Chest chest = this.sourceItem as Chest;
					if (chest != null && (chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin || chest.SpecialChestType == Chest.SpecialChestTypes.JunimoChest || chest.SpecialChestType == Chest.SpecialChestTypes.Enricher))
					{
						goto IL_380;
					}
				}
				if (this.source != 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.ItemsToGrabMenu.xPositionOnScreen - 100), (float)(this.yPositionOnScreen + 64 + 16)), new Rectangle?(new Rectangle(16, 368, 12, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.ItemsToGrabMenu.xPositionOnScreen - 100), (float)(this.yPositionOnScreen + 64 - 16)), new Rectangle?(new Rectangle(21, 368, 11, 16)), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Rectangle sourceRect = new Rectangle(127, 412, 10, 11);
					int num = this.source;
					if (num != 3)
					{
						if (num == 4)
						{
							sourceRect.X += 20;
						}
					}
					else
					{
						sourceRect.X += 10;
					}
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.ItemsToGrabMenu.xPositionOnScreen - 80), (float)(this.yPositionOnScreen + 64 - 44)), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				IL_380:
				this.ItemsToGrabMenu.draw(b);
			}
			else if (this.message != null)
			{
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2, this.ItemsToGrabMenu.yPositionOnScreen + this.ItemsToGrabMenu.height / 2, false, false, this.message);
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.poof;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			foreach (ItemGrabMenu.TransferredItemSprite transferredItemSprite in this._transferredItemSprites)
			{
				transferredItemSprite.Draw(b);
			}
			if (this.shippingBin && Game1.getFarm().lastItemShipped != null)
			{
				this.lastShippedHolder.draw(b);
				Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2((float)(this.lastShippedHolder.bounds.X + 16), (float)(this.lastShippedHolder.bounds.Y + 16)), 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.lastShippedHolder.bounds.X + -8), (float)(this.lastShippedHolder.bounds.Bottom - 100)), new Rectangle?(new Rectangle(325, 448, 5, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.lastShippedHolder.bounds.X + 84), (float)(this.lastShippedHolder.bounds.Bottom - 100)), new Rectangle?(new Rectangle(325, 448, 5, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.lastShippedHolder.bounds.X + -8), (float)(this.lastShippedHolder.bounds.Bottom - 44)), new Rectangle?(new Rectangle(325, 452, 5, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.lastShippedHolder.bounds.X + 84), (float)(this.lastShippedHolder.bounds.Bottom - 44)), new Rectangle?(new Rectangle(325, 452, 5, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (this.colorPickerToggleButton != null)
			{
				this.colorPickerToggleButton.draw(b);
			}
			else
			{
				ClickableTextureComponent clickableTextureComponent = this.specialButton;
				if (clickableTextureComponent != null)
				{
					clickableTextureComponent.draw(b);
				}
			}
			DiscreteColorPicker discreteColorPicker = this.chestColorPicker;
			if (discreteColorPicker != null)
			{
				discreteColorPicker.draw(b);
			}
			ClickableTextureComponent clickableTextureComponent2 = this.organizeButton;
			if (clickableTextureComponent2 != null)
			{
				clickableTextureComponent2.draw(b);
			}
			ClickableTextureComponent clickableTextureComponent3 = this.fillStacksButton;
			if (clickableTextureComponent3 != null)
			{
				clickableTextureComponent3.draw(b);
			}
			ClickableTextureComponent clickableTextureComponent4 = this.junimoNoteIcon;
			if (clickableTextureComponent4 != null)
			{
				clickableTextureComponent4.draw(b);
			}
			if (this.hoverText != null && (this.hoveredItem == null || this.ItemsToGrabMenu == null))
			{
				if (this.hoverAmount > 0)
				{
					IClickableMenu.drawToolTip(b, this.hoverText, "", null, true, -1, 0, null, -1, null, this.hoverAmount, null);
				}
				else
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
			if (this.hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, base.heldItem != null, -1, 0, null, -1, null, -1, null);
			}
			else if (this.hoveredItem != null && this.ItemsToGrabMenu != null)
			{
				IClickableMenu.drawToolTip(b, this.ItemsToGrabMenu.descriptionText, this.ItemsToGrabMenu.descriptionTitle, this.hoveredItem, base.heldItem != null, -1, 0, null, -1, null, -1, null);
			}
			Item heldItem = base.heldItem;
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
			}
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b, false, -1);
		}

		// Token: 0x06002A1E RID: 10782 RVA: 0x001F7374 File Offset: 0x001F5574
		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			if (this.superEssential)
			{
				this.DropRemainingItems();
			}
		}

		// Token: 0x06002A1F RID: 10783 RVA: 0x001F738C File Offset: 0x001F558C
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (this.essential)
			{
				foreach (Item item in this.ItemsToGrabMenu.actualInventory)
				{
					if (item != null)
					{
						Item leftOver = Game1.player.addItemToInventory(item);
						if (leftOver != null)
						{
							Game1.createItemDebris(leftOver, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
						}
					}
				}
			}
		}

		// Token: 0x04001B92 RID: 7058
		public const int region_organizationButtons = 15923;

		// Token: 0x04001B93 RID: 7059
		public const int region_itemsToGrabMenuModifier = 53910;

		// Token: 0x04001B94 RID: 7060
		public const int region_fillStacksButton = 12952;

		// Token: 0x04001B95 RID: 7061
		public const int region_organizeButton = 106;

		// Token: 0x04001B96 RID: 7062
		public const int region_colorPickToggle = 27346;

		// Token: 0x04001B97 RID: 7063
		public const int region_specialButton = 12485;

		// Token: 0x04001B98 RID: 7064
		public const int region_lastShippedHolder = 12598;

		// Token: 0x04001B99 RID: 7065
		public const int source_none = 0;

		// Token: 0x04001B9A RID: 7066
		public const int source_chest = 1;

		// Token: 0x04001B9B RID: 7067
		public const int source_gift = 2;

		// Token: 0x04001B9C RID: 7068
		public const int source_fishingChest = 3;

		// Token: 0x04001B9D RID: 7069
		public const int source_overflow = 4;

		// Token: 0x04001B9E RID: 7070
		public const int specialButton_junimotoggle = 1;

		// Token: 0x04001B9F RID: 7071
		public InventoryMenu ItemsToGrabMenu;

		// Token: 0x04001BA0 RID: 7072
		public TemporaryAnimatedSprite poof;

		// Token: 0x04001BA1 RID: 7073
		public bool reverseGrab;

		// Token: 0x04001BA2 RID: 7074
		public bool showReceivingMenu = true;

		// Token: 0x04001BA3 RID: 7075
		public bool drawBG = true;

		// Token: 0x04001BA4 RID: 7076
		public bool destroyItemOnClick;

		// Token: 0x04001BA5 RID: 7077
		public bool canExitOnKey;

		// Token: 0x04001BA6 RID: 7078
		public bool playRightClickSound;

		// Token: 0x04001BA7 RID: 7079
		public bool allowRightClick;

		// Token: 0x04001BA8 RID: 7080
		public bool shippingBin;

		// Token: 0x04001BA9 RID: 7081
		public string message;

		// Token: 0x04001BAA RID: 7082
		public ItemGrabMenu.behaviorOnItemSelect behaviorFunction;

		// Token: 0x04001BAB RID: 7083
		public ItemGrabMenu.behaviorOnItemSelect behaviorOnItemGrab;

		// Token: 0x04001BAC RID: 7084
		public Item sourceItem;

		// Token: 0x04001BAD RID: 7085
		public ClickableTextureComponent fillStacksButton;

		// Token: 0x04001BAE RID: 7086
		public ClickableTextureComponent organizeButton;

		// Token: 0x04001BAF RID: 7087
		public ClickableTextureComponent colorPickerToggleButton;

		// Token: 0x04001BB0 RID: 7088
		public ClickableTextureComponent specialButton;

		// Token: 0x04001BB1 RID: 7089
		public ClickableTextureComponent lastShippedHolder;

		// Token: 0x04001BB2 RID: 7090
		public List<ClickableComponent> discreteColorPickerCC;

		// Token: 0x04001BB3 RID: 7091
		public int source;

		// Token: 0x04001BB4 RID: 7092
		public int whichSpecialButton;

		// Token: 0x04001BB5 RID: 7093
		public object context;

		// Token: 0x04001BB6 RID: 7094
		public bool snappedtoBottom;

		// Token: 0x04001BB7 RID: 7095
		public DiscreteColorPicker chestColorPicker;

		// Token: 0x04001BB8 RID: 7096
		public bool essential;

		// Token: 0x04001BB9 RID: 7097
		public bool superEssential;

		// Token: 0x04001BBA RID: 7098
		public int storageSpaceTopBorderOffset;

		// Token: 0x04001BBB RID: 7099
		private bool HasUpdateTicked;

		// Token: 0x04001BBC RID: 7100
		public List<ItemGrabMenu.TransferredItemSprite> _transferredItemSprites = new List<ItemGrabMenu.TransferredItemSprite>();

		// Token: 0x04001BBD RID: 7101
		public bool _sourceItemInCurrentLocation;

		// Token: 0x04001BBE RID: 7102
		public ClickableTextureComponent junimoNoteIcon;

		// Token: 0x04001BBF RID: 7103
		public int junimoNotePulser;

		// Token: 0x02000614 RID: 1556
		// (Invoke) Token: 0x06004424 RID: 17444
		public delegate void behaviorOnItemSelect(Item item, Farmer who);

		// Token: 0x02000615 RID: 1557
		public class TransferredItemSprite
		{
			// Token: 0x06004427 RID: 17447 RVA: 0x0031BA37 File Offset: 0x00319C37
			public TransferredItemSprite(Item transferred_item, int start_x, int start_y)
			{
				this.item = transferred_item;
				this.position.X = (float)start_x;
				this.position.Y = (float)start_y;
			}

			// Token: 0x06004428 RID: 17448 RVA: 0x0031BA6C File Offset: 0x00319C6C
			public bool Update(GameTime time)
			{
				float life_time = 0.15f;
				this.position.Y = this.position.Y - (float)time.ElapsedGameTime.TotalSeconds * 128f;
				this.age += (float)time.ElapsedGameTime.TotalSeconds;
				this.alpha = 1f - this.age / life_time;
				return this.age >= life_time;
			}

			// Token: 0x06004429 RID: 17449 RVA: 0x0031BAE0 File Offset: 0x00319CE0
			public void Draw(SpriteBatch b)
			{
				this.item.drawInMenu(b, this.position, 1f, this.alpha, 0.9f, StackDrawType.Hide, Color.White, false);
			}

			// Token: 0x04002E73 RID: 11891
			public Item item;

			// Token: 0x04002E74 RID: 11892
			public Vector2 position;

			// Token: 0x04002E75 RID: 11893
			public float age;

			// Token: 0x04002E76 RID: 11894
			public float alpha = 1f;
		}
	}
}
