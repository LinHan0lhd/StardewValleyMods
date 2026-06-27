using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;

namespace StardewValley.Menus
{
	// Token: 0x02000278 RID: 632
	public class InventoryPage : IClickableMenu
	{
		// Token: 0x060029EE RID: 10734 RVA: 0x001F0EFC File Offset: 0x001EF0FC
		public InventoryPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, true, null, null, -1, 3, 0, 0, true);
			bool flag = Game1.player.stats.Get("trinketSlots") > 0U;
			int trinkets_or_trash = flag ? 120 : 105;
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Left Ring")
			{
				myID = 102,
				downNeighborID = 103,
				upNeighborID = Game1.player.MaxItems - 12,
				rightNeighborID = 101,
				fullyImmutable = false
			});
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Right Ring")
			{
				myID = 103,
				upNeighborID = 102,
				downNeighborID = 104,
				rightNeighborID = 108,
				fullyImmutable = true
			});
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Boots")
			{
				myID = 104,
				upNeighborID = 103,
				rightNeighborID = 109,
				fullyImmutable = true
			});
			this.portrait = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 192 - 8 - 64 + 32, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8 + 64, 64, 96), "32");
			this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width / 3 + 576 + 32, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 + 64, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
			{
				myID = 105,
				upNeighborID = 106,
				leftNeighborID = 101
			};
			this.organizeButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + width, this.yPositionOnScreen + height / 3 - 64 + 8, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f, false)
			{
				myID = 106,
				downNeighborID = 105,
				leftNeighborID = 11,
				upNeighborID = 898
			};
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Hat")
			{
				myID = 101,
				leftNeighborID = 102,
				downNeighborID = 108,
				upNeighborID = Game1.player.MaxItems - 9,
				rightNeighborID = trinkets_or_trash,
				fullyImmutable = false
			});
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Shirt")
			{
				myID = 108,
				upNeighborID = 101,
				downNeighborID = 109,
				rightNeighborID = trinkets_or_trash,
				leftNeighborID = 103,
				fullyImmutable = true
			});
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Pants")
			{
				myID = 109,
				upNeighborID = 108,
				rightNeighborID = trinkets_or_trash,
				leftNeighborID = 104,
				fullyImmutable = true
			});
			if (flag)
			{
				Farmer.MaximumTrinkets = 1;
				for (int i = 0; i < Farmer.MaximumTrinkets; i++)
				{
					ClickableComponent trinket_slot = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 280, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + (4 + i) * 64 - 12, 64, 64), "Trinket")
					{
						myID = 120 + i,
						upNeighborID = Game1.player.MaxItems - 8,
						rightNeighborID = 105,
						leftNeighborID = -99998,
						fullyImmutable = true
					};
					if (i < Farmer.MaximumTrinkets - 1)
					{
						trinket_slot.downNeighborID = -99998;
					}
					this.equipmentIcons.Add(trinket_slot);
				}
			}
			if (InventoryPage.ShouldShowJunimoNoteIcon())
			{
				this.junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + width, this.yPositionOnScreen + 96, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f, false)
				{
					myID = 898,
					leftNeighborID = 11,
					downNeighborID = 106
				};
			}
			this._pet = Game1.GetCharacterOfType<Pet>(false);
			this._horse = Game1.getCharacterFromName<Horse>(Game1.player.horseName.Value, false, false);
			if (this._horse == null && Game1.player.isRidingHorse() && Game1.player.mount.Name.Equals(Game1.player.horseName.Value))
			{
				this._horse = Game1.player.mount;
			}
		}

		// Token: 0x060029EF RID: 10735 RVA: 0x001F1538 File Offset: 0x001EF738
		public static bool ShouldShowJunimoNoteIcon()
		{
			return Game1.player.hasOrWillReceiveMail("canReadJunimoText") && !Game1.player.hasOrWillReceiveMail("JojaMember") && (!Game1.MasterPlayer.hasCompletedCommunityCenter() || (Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater")));
		}

		// Token: 0x060029F0 RID: 10736 RVA: 0x001F159C File Offset: 0x001EF79C
		protected virtual bool checkHeldItem(Func<Item, bool> f = null)
		{
			if (f == null)
			{
				return Game1.player.CursorSlotItem != null;
			}
			return f(Game1.player.CursorSlotItem);
		}

		// Token: 0x060029F1 RID: 10737 RVA: 0x001F15BF File Offset: 0x001EF7BF
		protected virtual Item takeHeldItem()
		{
			Item cursorSlotItem = Game1.player.CursorSlotItem;
			Game1.player.CursorSlotItem = null;
			return cursorSlotItem;
		}

		// Token: 0x060029F2 RID: 10738 RVA: 0x001F15D6 File Offset: 0x001EF7D6
		protected virtual void setHeldItem(Item item)
		{
			if (item != null)
			{
				item.onDetachedFromParent();
			}
			Game1.player.CursorSlotItem = item;
		}

		// Token: 0x060029F3 RID: 10739 RVA: 0x001F15EC File Offset: 0x001EF7EC
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.checkHeldItem(null))
			{
				Game1.setMousePosition(this.trashCan.bounds.Center);
			}
			if (key == Keys.Delete)
			{
				if (this.checkHeldItem((Item i) => i != null && i.canBeTrashed()))
				{
					Utility.trashItem(this.takeHeldItem());
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot1, key))
			{
				Game1.player.CurrentToolIndex = 0;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot2, key))
			{
				Game1.player.CurrentToolIndex = 1;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot3, key))
			{
				Game1.player.CurrentToolIndex = 2;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot4, key))
			{
				Game1.player.CurrentToolIndex = 3;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot5, key))
			{
				Game1.player.CurrentToolIndex = 4;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot6, key))
			{
				Game1.player.CurrentToolIndex = 5;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot7, key))
			{
				Game1.player.CurrentToolIndex = 6;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot8, key))
			{
				Game1.player.CurrentToolIndex = 7;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot9, key))
			{
				Game1.player.CurrentToolIndex = 8;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot10, key))
			{
				Game1.player.CurrentToolIndex = 9;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot11, key))
			{
				Game1.player.CurrentToolIndex = 10;
				Game1.playSound("toolSwap", null);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot12, key))
			{
				Game1.player.CurrentToolIndex = 11;
				Game1.playSound("toolSwap", null);
			}
		}

		// Token: 0x060029F4 RID: 10740 RVA: 0x001F1909 File Offset: 0x001EFB09
		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			InventoryMenu inventoryMenu = this.inventory;
			if (inventoryMenu == null)
			{
				return;
			}
			inventoryMenu.setUpForGamePadMode();
		}

		// Token: 0x060029F5 RID: 10741 RVA: 0x001F1924 File Offset: 0x001EFB24
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					Item newItem = Utility.PerformSpecialItemPlaceReplacement(Game1.player.CursorSlotItem);
					bool heldItemWasNull = newItem == null;
					string name = c.name;
					if (name != null)
					{
						switch (name.Length)
						{
						case 3:
						{
							if (!(name == "Hat"))
							{
								goto IL_565;
							}
							if (newItem != null && !(newItem is Hat))
							{
								goto IL_565;
							}
							Item oldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip<Hat>((Hat)newItem, Game1.player.hat));
							this.setHeldItem(oldItem);
							if (Game1.player.hat.Value != null)
							{
								Game1.playSound("grassyStep", null);
								goto IL_565;
							}
							if (this.checkHeldItem(null))
							{
								Game1.playSound("dwop", null);
								goto IL_565;
							}
							goto IL_565;
						}
						case 4:
						case 6:
						case 8:
							goto IL_565;
						case 5:
						{
							char c2 = name[0];
							if (c2 != 'B')
							{
								if (c2 != 'P')
								{
									if (c2 != 'S')
									{
										goto IL_565;
									}
									if (!(name == "Shirt"))
									{
										goto IL_565;
									}
									if (newItem != null)
									{
										Clothing clothing = newItem as Clothing;
										if (clothing == null || clothing.clothesType.Value != Clothing.ClothesType.SHIRT)
										{
											goto IL_565;
										}
									}
									Item oldItem2 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip<Clothing>((Clothing)newItem, Game1.player.shirtItem));
									this.setHeldItem(oldItem2);
									if (Game1.player.shirtItem.Value != null)
									{
										Game1.playSound("sandyStep", null);
										goto IL_565;
									}
									if (this.checkHeldItem(null))
									{
										Game1.playSound("dwop", null);
										goto IL_565;
									}
									goto IL_565;
								}
								else
								{
									if (!(name == "Pants"))
									{
										goto IL_565;
									}
									if (newItem != null)
									{
										Clothing clothing2 = newItem as Clothing;
										if (clothing2 == null || clothing2.clothesType.Value != Clothing.ClothesType.PANTS)
										{
											goto IL_565;
										}
									}
									Item oldItem3 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip<Clothing>((Clothing)newItem, Game1.player.pantsItem));
									this.setHeldItem(oldItem3);
									if (Game1.player.pantsItem.Value != null)
									{
										Game1.playSound("sandyStep", null);
										goto IL_565;
									}
									if (this.checkHeldItem(null))
									{
										Game1.playSound("dwop", null);
										goto IL_565;
									}
									goto IL_565;
								}
							}
							else
							{
								if (!(name == "Boots"))
								{
									goto IL_565;
								}
								if (newItem != null && !(newItem is Boots))
								{
									goto IL_565;
								}
								Item oldItem4 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip<Boots>((Boots)newItem, Game1.player.boots));
								this.setHeldItem(oldItem4);
								if (Game1.player.boots.Value != null)
								{
									Game1.playSound("sandyStep", null);
									DelayedAction.playSoundAfterDelay("sandyStep", 150, null, null, -1, false);
									goto IL_565;
								}
								if (this.checkHeldItem(null))
								{
									Game1.playSound("dwop", null);
									goto IL_565;
								}
								goto IL_565;
							}
							break;
						}
						case 7:
						{
							if (!(name == "Trinket"))
							{
								goto IL_565;
							}
							if (Game1.player.stats.Get("trinketSlots") <= 0U)
							{
								goto IL_565;
							}
							if (!this.checkHeldItem((Item i) => i == null || i is Trinket))
							{
								goto IL_565;
							}
							int trinket_index = c.myID - 120;
							Trinket new_item = (Trinket)this.takeHeldItem();
							Trinket old_item = null;
							if (Game1.player.trinketItems.Count > trinket_index)
							{
								old_item = Game1.player.trinketItems[trinket_index];
							}
							old_item = (Trinket)Utility.PerformSpecialItemGrabReplacement(old_item);
							this.setHeldItem(old_item);
							while (Game1.player.trinketItems.Count <= trinket_index)
							{
								Game1.player.trinketItems.Add(null);
							}
							Game1.player.trinketItems[trinket_index] = new_item;
							if (Game1.player.trinketItems[trinket_index] != null)
							{
								Game1.playSound("clank", null);
								goto IL_565;
							}
							if (this.checkHeldItem(null))
							{
								Game1.playSound("dwop", null);
								goto IL_565;
							}
							goto IL_565;
						}
						case 9:
							if (!(name == "Left Ring"))
							{
								goto IL_565;
							}
							break;
						case 10:
							if (!(name == "Right Ring"))
							{
								goto IL_565;
							}
							break;
						default:
							goto IL_565;
						}
						if (newItem == null || newItem is Ring)
						{
							NetRef<Ring> ringField = (c.name == "Left Ring") ? Game1.player.leftRing : Game1.player.rightRing;
							Item oldItem5 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip<Ring>((Ring)newItem, ringField));
							this.setHeldItem(oldItem5);
							if (Game1.player.leftRing.Value != null)
							{
								Game1.playSound("crit", null);
							}
							else if (this.checkHeldItem(null))
							{
								Game1.playSound("dwop", null);
							}
						}
					}
					IL_565:
					if (heldItemWasNull && this.checkHeldItem(null) && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						int i2;
						int i;
						for (i = 0; i < Game1.player.Items.Count; i = i2 + 1)
						{
							if (Game1.player.Items[i] == null || this.checkHeldItem((Item item) => Game1.player.Items[i].canStackWith(item)))
							{
								if (Game1.player.CurrentToolIndex == i && this.checkHeldItem(null))
								{
									Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
								}
								this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory, null));
								if (Game1.player.CurrentToolIndex == i && this.checkHeldItem(null))
								{
									Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
								}
								Game1.playSound("stoneStep", null);
								return;
							}
							i2 = i;
						}
					}
				}
			}
			this.setHeldItem(this.inventory.leftClick(x, y, this.takeHeldItem(), !Game1.oldKBState.IsKeyDown(Keys.LeftShift)));
			if (this.checkHeldItem((Item i) => ((i != null) ? i.QualifiedItemId : null) == "(O)434"))
			{
				Game1.playSound("smallSelect", null);
				Game1.player.eatObject(this.takeHeldItem() as Object, true);
				Game1.exitActiveMenu();
			}
			else if (this.checkHeldItem(null) && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				if (this.checkHeldItem((Item i) => i is Ring))
				{
					if (Game1.player.leftRing.Value == null)
					{
						Game1.player.Equip<Ring>(this.takeHeldItem() as Ring, Game1.player.leftRing);
						Game1.playSound("crit", null);
						return;
					}
					if (Game1.player.rightRing.Value == null)
					{
						Game1.player.Equip<Ring>(this.takeHeldItem() as Ring, Game1.player.rightRing);
						Game1.playSound("crit", null);
						return;
					}
				}
				else if (this.checkHeldItem((Item i) => i is Hat))
				{
					if (Game1.player.hat.Value == null)
					{
						Game1.player.Equip<Hat>(this.takeHeldItem() as Hat, Game1.player.hat);
						Game1.playSound("grassyStep", null);
						return;
					}
				}
				else if (this.checkHeldItem((Item i) => i is Boots))
				{
					if (Game1.player.boots.Value == null)
					{
						Game1.player.Equip<Boots>(this.takeHeldItem() as Boots, Game1.player.boots);
						Game1.playSound("sandyStep", null);
						DelayedAction.playSoundAfterDelay("sandyStep", 150, null, null, -1, false);
						return;
					}
				}
				else if (this.checkHeldItem(delegate(Item i)
				{
					Clothing clothing3 = i as Clothing;
					return clothing3 != null && clothing3.clothesType.Value == Clothing.ClothesType.SHIRT;
				}))
				{
					if (Game1.player.shirtItem.Value == null)
					{
						Game1.player.Equip<Clothing>(this.takeHeldItem() as Clothing, Game1.player.shirtItem);
						Game1.playSound("sandyStep", null);
						DelayedAction.playSoundAfterDelay("sandyStep", 150, null, null, -1, false);
						return;
					}
				}
				else if (this.checkHeldItem(delegate(Item i)
				{
					Clothing clothing3 = i as Clothing;
					return clothing3 != null && clothing3.clothesType.Value == Clothing.ClothesType.PANTS;
				}))
				{
					if (Game1.player.pantsItem.Value == null)
					{
						Game1.player.Equip<Clothing>(this.takeHeldItem() as Clothing, Game1.player.pantsItem);
						Game1.playSound("sandyStep", null);
						DelayedAction.playSoundAfterDelay("sandyStep", 150, null, null, -1, false);
						return;
					}
				}
				else if (this.checkHeldItem((Item i) => i is Trinket) && Game1.player.stats.Get("trinketSlots") > 0U)
				{
					bool success = false;
					for (int j = 0; j < Game1.player.trinketItems.Count; j++)
					{
						if (Game1.player.trinketItems[j] == null)
						{
							Game1.player.trinketItems[j] = (this.takeHeldItem() as Trinket);
							success = true;
							break;
						}
					}
					if (Game1.player.trinketItems.Count < Farmer.MaximumTrinkets)
					{
						Game1.player.trinketItems.Add(this.takeHeldItem() as Trinket);
						success = true;
					}
					if (success)
					{
						Game1.playSound("clank", null);
						return;
					}
				}
				if (this.inventory.getInventoryPositionOfClick(x, y) >= 12)
				{
					int i2;
					int i;
					for (i = 0; i < 12; i = i2 + 1)
					{
						if (Game1.player.Items[i] == null || this.checkHeldItem((Item item) => Game1.player.Items[i].canStackWith(item)))
						{
							if (Game1.player.CurrentToolIndex == i && this.checkHeldItem(null))
							{
								Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
							}
							this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory, null));
							if (this.checkHeldItem(null))
							{
								Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
							}
							Game1.playSound("stoneStep", null);
							return;
						}
						i2 = i;
					}
				}
				else if (this.inventory.getInventoryPositionOfClick(x, y) < 12)
				{
					int i2;
					int i;
					for (i = 12; i < Game1.player.Items.Count; i = i2 + 1)
					{
						if (Game1.player.Items[i] == null || this.checkHeldItem((Item item) => Game1.player.Items[i].canStackWith(item)))
						{
							if (Game1.player.CurrentToolIndex == i && this.checkHeldItem(null))
							{
								Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
							}
							this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory, null));
							if (this.checkHeldItem(null))
							{
								Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
							}
							Game1.playSound("stoneStep", null);
							return;
						}
						i2 = i;
					}
				}
			}
			if (this.portrait.containsPoint(x, y))
			{
				this.portrait.name = (this.portrait.name.Equals("32") ? "8" : "32");
			}
			if (this.trashCan.containsPoint(x, y))
			{
				if (this.checkHeldItem((Item i) => i != null && i.canBeTrashed()))
				{
					Utility.trashItem(this.takeHeldItem());
					if (Game1.options.SnappyMenus)
					{
						this.snapCursorToCurrentSnappedComponent();
						goto IL_E3E;
					}
					goto IL_E3E;
				}
			}
			if (!this.isWithinBounds(x, y))
			{
				if (this.checkHeldItem((Item i) => i != null && i.canBeTrashed()))
				{
					Game1.playSound("throwDownITem", null);
					Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false).DroppedByPlayerID.Value = Game1.player.UniqueMultiplayerID;
				}
			}
			IL_E3E:
			if (this.organizeButton != null && this.organizeButton.containsPoint(x, y))
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.Items);
				Game1.playSound("Ship", null);
			}
			if (this.junimoNoteIcon != null && this.junimoNoteIcon.containsPoint(x, y) && this.readyToClose())
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(true, 1, false)
				{
					gameMenuTabToReturnTo = GameMenu.inventoryTab
				};
			}
		}

		// Token: 0x060029F6 RID: 10742 RVA: 0x001F2800 File Offset: 0x001F0A00
		public override void receiveGamePadButton(Buttons button)
		{
			if (button == Buttons.Back && this.organizeButton != null)
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.Items);
				Game1.playSound("Ship", null);
			}
		}

		// Token: 0x060029F7 RID: 10743 RVA: 0x001F283D File Offset: 0x001F0A3D
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			this.setHeldItem(this.inventory.rightClick(x, y, this.takeHeldItem(), true, false));
		}

		// Token: 0x060029F8 RID: 10744 RVA: 0x001F285C File Offset: 0x001F0A5C
		public override void performHoverAction(int x, int y)
		{
			this.hoverAmount = -1;
			this.hoveredItem = this.inventory.hover(x, y, Game1.player.CursorSlotItem);
			this.hoverText = this.inventory.hoverText;
			this.hoverTitle = this.inventory.hoverTitle;
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (name != null)
					{
						switch (name.Length)
						{
						case 3:
							if (name == "Hat")
							{
								if (Game1.player.hat.Value != null)
								{
									this.hoveredItem = Game1.player.hat.Value;
									this.hoverText = Game1.player.hat.Value.getDescription();
									this.hoverTitle = Game1.player.hat.Value.DisplayName;
								}
							}
							break;
						case 5:
						{
							char c2 = name[0];
							if (c2 != 'B')
							{
								if (c2 != 'P')
								{
									if (c2 == 'S')
									{
										if (name == "Shirt")
										{
											if (Game1.player.shirtItem.Value != null)
											{
												this.hoveredItem = Game1.player.shirtItem.Value;
												this.hoverText = Game1.player.shirtItem.Value.getDescription();
												this.hoverTitle = Game1.player.shirtItem.Value.DisplayName;
											}
										}
									}
								}
								else if (name == "Pants")
								{
									if (Game1.player.pantsItem.Value != null)
									{
										this.hoveredItem = Game1.player.pantsItem.Value;
										this.hoverText = Game1.player.pantsItem.Value.getDescription();
										this.hoverTitle = Game1.player.pantsItem.Value.DisplayName;
									}
								}
							}
							else if (name == "Boots")
							{
								if (Game1.player.boots.Value != null)
								{
									this.hoveredItem = Game1.player.boots.Value;
									this.hoverText = Game1.player.boots.Value.getDescription();
									this.hoverTitle = Game1.player.boots.Value.DisplayName;
								}
							}
							break;
						}
						case 7:
							if (name == "Trinket")
							{
								if (Game1.player.trinketItems.Count == 1 && Game1.player.trinketItems[0] != null)
								{
									this.hoveredItem = Game1.player.trinketItems[0];
									this.hoverText = Game1.player.trinketItems[0].getDescription();
									this.hoverTitle = Game1.player.trinketItems[0].DisplayName;
								}
							}
							break;
						case 9:
							if (name == "Left Ring")
							{
								if (Game1.player.leftRing.Value != null)
								{
									this.hoveredItem = Game1.player.leftRing.Value;
									this.hoverText = Game1.player.leftRing.Value.getDescription();
									this.hoverTitle = Game1.player.leftRing.Value.DisplayName;
								}
							}
							break;
						case 10:
							if (name == "Right Ring")
							{
								if (Game1.player.rightRing.Value != null)
								{
									this.hoveredItem = Game1.player.rightRing.Value;
									this.hoverText = Game1.player.rightRing.Value.getDescription();
									this.hoverTitle = Game1.player.rightRing.Value.DisplayName;
								}
							}
							break;
						}
					}
					c.scale = Math.Min(c.scale + 0.05f, 1.1f);
				}
				c.scale = Math.Max(1f, c.scale - 0.025f);
			}
			if (this.portrait.containsPoint(x, y))
			{
				this.portrait.scale += 0.2f;
				this.hoverText = Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level) + Environment.NewLine + Game1.player.getTitle();
			}
			else
			{
				this.portrait.scale = 0f;
			}
			if (this.trashCan.containsPoint(x, y))
			{
				if (this.trashCanLidRotation <= 0f)
				{
					Game1.playSound("trashcanlid", null);
				}
				this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + 0.06544985f, 1.5707964f);
				if (this.checkHeldItem(null) && Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player) > 0)
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
					this.hoverAmount = Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player);
				}
			}
			else if (this.trashCanLidRotation != 0f)
			{
				this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - 0.1308997f, 0f);
				if (this.trashCanLidRotation == 0f)
				{
					Game1.playSound("thudStep", null);
				}
			}
			if (this.organizeButton != null)
			{
				this.organizeButton.tryHover(x, y, 0.1f);
				if (this.organizeButton.containsPoint(x, y))
				{
					this.hoverText = this.organizeButton.hoverText;
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
					return;
				}
				this.junimoNotePulser = 0;
				this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale;
			}
		}

		// Token: 0x060029F9 RID: 10745 RVA: 0x001F2F40 File Offset: 0x001F1140
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060029FA RID: 10746 RVA: 0x001F2F55 File Offset: 0x001F1155
		public override bool readyToClose()
		{
			return !this.checkHeldItem(null);
		}

		// Token: 0x060029FB RID: 10747 RVA: 0x001F2F64 File Offset: 0x001F1164
		public override void draw(SpriteBatch b)
		{
			base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, false, -1, -1, -1);
			this.inventory.draw(b);
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				string name = c.name;
				if (name != null)
				{
					switch (name.Length)
					{
					case 3:
						if (name == "Hat")
						{
							if (Game1.player.hat.Value != null)
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
								Game1.player.hat.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, 1f, 0.866f, StackDrawType.Hide);
							}
							else
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42, -1, -1)), Color.White);
							}
						}
						break;
					case 5:
					{
						char c2 = name[0];
						if (c2 != 'B')
						{
							if (c2 != 'P')
							{
								if (c2 == 'S')
								{
									if (name == "Shirt")
									{
										if (Game1.player.shirtItem.Value != null)
										{
											b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
											Game1.player.shirtItem.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
										}
										else
										{
											b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69, -1, -1)), Color.White);
										}
									}
								}
							}
							else if (name == "Pants")
							{
								if (Game1.player.pantsItem.Value != null)
								{
									b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
									Game1.player.pantsItem.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
								}
								else
								{
									b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68, -1, -1)), Color.White);
								}
							}
						}
						else if (name == "Boots")
						{
							if (Game1.player.boots.Value != null)
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
								Game1.player.boots.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
							}
							else
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40, -1, -1)), Color.White);
							}
						}
						break;
					}
					case 7:
						if (name == "Trinket")
						{
							int trinket_index = c.myID - 120;
							if (Game1.player.trinketItems.Count > trinket_index && Game1.player.trinketItems[trinket_index] != null)
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
								Game1.player.trinketItems[trinket_index].drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
							}
							else
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 70, -1, -1)), Color.White);
							}
						}
						break;
					case 9:
						if (name == "Left Ring")
						{
							if (Game1.player.leftRing.Value != null)
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
								Game1.player.leftRing.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
							}
							else
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41, -1, -1)), Color.White);
							}
						}
						break;
					case 10:
						if (name == "Right Ring")
						{
							if (Game1.player.rightRing.Value != null)
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White);
								Game1.player.rightRing.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale);
							}
							else
							{
								b.Draw(Game1.menuTexture, c.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41, -1, -1)), Color.White);
							}
						}
						break;
					}
				}
			}
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2((float)(this.xPositionOnScreen + 192 - 64 - 8), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8)), Color.White);
			FarmerRenderer.isDrawingForUI = true;
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false, null, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.xPositionOnScreen + 192 - 8 - 32), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8)), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
			if (Game1.timeOfDay >= 1900)
			{
				Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false, null, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.xPositionOnScreen + 192 - 8 - 32), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8)), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
			}
			FarmerRenderer.isDrawingForUI = false;
			Utility.drawTextWithShadow(b, Game1.player.Name, Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + 192 - 8) - Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			float offset = 32f;
			string farmName = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", Game1.player.farmName);
			Utility.drawTextWithShadow(b, farmName, Game1.dialogueFont, new Vector2((float)this.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(farmName).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			string currentFunds = Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas(Game1.player.Money));
			Utility.drawTextWithShadow(b, currentFunds, Game1.dialogueFont, new Vector2((float)this.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(currentFunds).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			string totalEarnings = Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned));
			Utility.drawTextWithShadow(b, totalEarnings, Game1.dialogueFont, new Vector2((float)this.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(totalEarnings).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 384)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
			Utility.drawTextWithShadow(b, Utility.getDateString(0), Game1.dialogueFont, new Vector2((float)this.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(Utility.getDateString(0)).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448)), Game1.textColor * 0.8f, 1f, -1f, -1, -1, 1f, 3);
			ClickableTextureComponent clickableTextureComponent = this.organizeButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.draw(b);
			}
			this.trashCan.draw(b);
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
			if (this.checkHeldItem(null))
			{
				Game1.player.CursorSlotItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
			}
			if (!string.IsNullOrEmpty(this.hoverText))
			{
				if (this.hoverAmount > 0)
				{
					IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, null, true, -1, 0, null, -1, null, this.hoverAmount, null);
				}
				else
				{
					IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoveredItem, this.checkHeldItem(null), -1, 0, null, -1, null, -1, null);
				}
			}
			ClickableTextureComponent clickableTextureComponent2 = this.junimoNoteIcon;
			if (clickableTextureComponent2 == null)
			{
				return;
			}
			clickableTextureComponent2.draw(b);
		}

		// Token: 0x060029FC RID: 10748 RVA: 0x001F3BDC File Offset: 0x001F1DDC
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			this.setHeldItem(Game1.player.addItemToInventory(this.takeHeldItem()));
			if (this.checkHeldItem(null))
			{
				Game1.playSound("throwDownITem", null);
				Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
			}
		}

		// Token: 0x04001B72 RID: 7026
		public const int region_inventory = 100;

		// Token: 0x04001B73 RID: 7027
		public const int region_hat = 101;

		// Token: 0x04001B74 RID: 7028
		public const int region_ring1 = 102;

		// Token: 0x04001B75 RID: 7029
		public const int region_ring2 = 103;

		// Token: 0x04001B76 RID: 7030
		public const int region_boots = 104;

		// Token: 0x04001B77 RID: 7031
		public const int region_trashCan = 105;

		// Token: 0x04001B78 RID: 7032
		public const int region_organizeButton = 106;

		// Token: 0x04001B79 RID: 7033
		public const int region_accessory = 107;

		// Token: 0x04001B7A RID: 7034
		public const int region_shirt = 108;

		// Token: 0x04001B7B RID: 7035
		public const int region_pants = 109;

		// Token: 0x04001B7C RID: 7036
		public const int region_shoes = 110;

		// Token: 0x04001B7D RID: 7037
		public const int region_trinkets = 120;

		// Token: 0x04001B7E RID: 7038
		public InventoryMenu inventory;

		// Token: 0x04001B7F RID: 7039
		public string hoverText = "";

		// Token: 0x04001B80 RID: 7040
		public string hoverTitle = "";

		// Token: 0x04001B81 RID: 7041
		public int hoverAmount;

		// Token: 0x04001B82 RID: 7042
		public Item hoveredItem;

		// Token: 0x04001B83 RID: 7043
		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		// Token: 0x04001B84 RID: 7044
		public ClickableComponent portrait;

		// Token: 0x04001B85 RID: 7045
		public ClickableTextureComponent trashCan;

		// Token: 0x04001B86 RID: 7046
		public ClickableTextureComponent organizeButton;

		// Token: 0x04001B87 RID: 7047
		private float trashCanLidRotation;

		// Token: 0x04001B88 RID: 7048
		public ClickableTextureComponent junimoNoteIcon;

		// Token: 0x04001B89 RID: 7049
		private int junimoNotePulser;

		// Token: 0x04001B8A RID: 7050
		protected Pet _pet;

		// Token: 0x04001B8B RID: 7051
		protected Horse _horse;
	}
}
