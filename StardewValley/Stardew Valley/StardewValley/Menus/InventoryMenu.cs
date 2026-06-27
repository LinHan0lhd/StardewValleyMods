using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x02000277 RID: 631
	public class InventoryMenu : IClickableMenu
	{
		// Token: 0x060029D8 RID: 10712 RVA: 0x001EF754 File Offset: 0x001ED954
		public InventoryMenu(int xPosition, int yPosition, bool playerInventory, IList<Item> actualInventory = null, InventoryMenu.highlightThisItem highlightMethod = null, int capacity = -1, int rows = 3, int horizontalGap = 0, int verticalGap = 0, bool drawSlots = true) : base(xPosition, yPosition, 64 * (((capacity == -1) ? 36 : capacity) / rows), 64 * rows + 16, false)
		{
			this.drawSlots = drawSlots;
			this.horizontalGap = horizontalGap;
			this.verticalGap = verticalGap;
			this.rows = rows;
			this.capacity = ((capacity == -1) ? 36 : capacity);
			this.playerInventory = playerInventory;
			this.actualInventory = actualInventory;
			if (actualInventory == null)
			{
				this.actualInventory = Game1.player.Items;
			}
			for (int i = 0; i < Game1.player.maxItems.Value; i++)
			{
				if (Game1.player.Items.Count <= i)
				{
					Game1.player.Items.Add(null);
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				int downNeighbor;
				if (playerInventory)
				{
					downNeighbor = ((j >= this.actualInventory.Count - this.capacity / rows) ? ((j >= this.actualInventory.Count - 3 || this.actualInventory.Count < 36) ? ((j % 12 < 2) ? 102 : 101) : -99998) : (j + this.capacity / rows));
				}
				else
				{
					downNeighbor = ((j >= this.capacity - this.capacity / rows) ? -99998 : (j + this.capacity / rows));
				}
				this.inventory.Add(new ClickableComponent(new Rectangle(xPosition + j % (this.capacity / rows) * 64 + horizontalGap * (j % (this.capacity / rows)), this.yPositionOnScreen + j / (this.capacity / rows) * (64 + verticalGap) + (j / (this.capacity / rows) - 1) * 4 - ((j > this.capacity / rows || !playerInventory || verticalGap != 0) ? 0 : 12), 64, 64), j.ToString() ?? "")
				{
					myID = j,
					leftNeighborID = ((j % (this.capacity / rows) != 0) ? (j - 1) : 107),
					rightNeighborID = (((j + 1) % (this.capacity / rows) != 0) ? (j + 1) : 106),
					downNeighborID = downNeighbor,
					upNeighborID = ((j < this.capacity / rows) ? (12340 + j) : (j - this.capacity / rows)),
					region = 9000,
					upNeighborImmutable = true,
					downNeighborImmutable = true,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
			}
			this.highlightMethod = highlightMethod;
			if (highlightMethod == null)
			{
				this.highlightMethod = new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems);
			}
			this.dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPosition - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, this.yPositionOnScreen - 12, 64, 64), "")
			{
				myID = (playerInventory ? 107 : -500),
				rightNeighborID = 0
			};
			foreach (ClickableComponent clickableComponent in this.GetBorder(InventoryMenu.BorderSide.Top))
			{
				clickableComponent.upNeighborImmutable = false;
			}
			foreach (ClickableComponent clickableComponent2 in this.GetBorder(InventoryMenu.BorderSide.Bottom))
			{
				clickableComponent2.downNeighborImmutable = false;
			}
			foreach (ClickableComponent clickableComponent3 in this.GetBorder(InventoryMenu.BorderSide.Left))
			{
				clickableComponent3.leftNeighborImmutable = false;
			}
			foreach (ClickableComponent clickableComponent4 in this.GetBorder(InventoryMenu.BorderSide.Right))
			{
				clickableComponent4.rightNeighborImmutable = false;
			}
		}

		// Token: 0x060029D9 RID: 10713 RVA: 0x001EFB8C File Offset: 0x001EDD8C
		public List<ClickableComponent> GetBorder(InventoryMenu.BorderSide side)
		{
			List<ClickableComponent> inventory_slots = new List<ClickableComponent>();
			int row_size = this.capacity / this.rows;
			switch (side)
			{
			case InventoryMenu.BorderSide.Top:
				for (int i = 0; i < this.inventory.Count; i++)
				{
					if (i < row_size)
					{
						inventory_slots.Add(this.inventory[i]);
					}
				}
				break;
			case InventoryMenu.BorderSide.Left:
				for (int j = 0; j < this.inventory.Count; j++)
				{
					if (j % row_size == 0)
					{
						inventory_slots.Add(this.inventory[j]);
					}
				}
				break;
			case InventoryMenu.BorderSide.Right:
				for (int k = 0; k < this.inventory.Count; k++)
				{
					if (k % row_size == row_size - 1)
					{
						inventory_slots.Add(this.inventory[k]);
					}
				}
				break;
			case InventoryMenu.BorderSide.Bottom:
				for (int l = 0; l < this.inventory.Count; l++)
				{
					if (l >= this.actualInventory.Count - row_size)
					{
						inventory_slots.Add(this.inventory[l]);
					}
				}
				break;
			}
			return inventory_slots;
		}

		// Token: 0x060029DA RID: 10714 RVA: 0x001EFC9F File Offset: 0x001EDE9F
		public static bool highlightAllItems(Item i)
		{
			return true;
		}

		// Token: 0x060029DB RID: 10715 RVA: 0x001EFCA2 File Offset: 0x001EDEA2
		public static bool highlightNoItems(Item i)
		{
			return false;
		}

		// Token: 0x060029DC RID: 10716 RVA: 0x001EFCA5 File Offset: 0x001EDEA5
		public void SetPosition(int x, int y)
		{
			this.movePosition(-this.xPositionOnScreen, -this.yPositionOnScreen);
			this.movePosition(x, y);
		}

		// Token: 0x060029DD RID: 10717 RVA: 0x001EFCC4 File Offset: 0x001EDEC4
		public void movePosition(int x, int y)
		{
			this.xPositionOnScreen += x;
			this.yPositionOnScreen += y;
			foreach (ClickableComponent clickableComponent in this.inventory)
			{
				clickableComponent.bounds.X = clickableComponent.bounds.X + x;
				clickableComponent.bounds.Y = clickableComponent.bounds.Y + y;
			}
			ClickableComponent clickableComponent2 = this.dropItemInvisibleButton;
			clickableComponent2.bounds.X = clickableComponent2.bounds.X + x;
			ClickableComponent clickableComponent3 = this.dropItemInvisibleButton;
			clickableComponent3.bounds.Y = clickableComponent3.bounds.Y + y;
		}

		// Token: 0x060029DE RID: 10718 RVA: 0x001EFD74 File Offset: 0x001EDF74
		public void ShakeItem(Item item)
		{
			this.ShakeItem(this.actualInventory.IndexOf(item));
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x001EFD88 File Offset: 0x001EDF88
		public void ShakeItem(int index)
		{
			if (index < 0 || index >= this.inventory.Count)
			{
				return;
			}
			this._iconShakeTimer[index] = Game1.currentGameTime.TotalGameTime.TotalSeconds + 0.5;
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x001EFDD0 File Offset: 0x001EDFD0
		public Item tryToAddItem(Item toPlace, string sound = "coin")
		{
			if (toPlace == null)
			{
				return null;
			}
			int originalStack = toPlace.Stack;
			foreach (ClickableComponent clickableComponent in this.inventory)
			{
				int slotNumber = Convert.ToInt32(clickableComponent.name);
				Item slot = (slotNumber < this.actualInventory.Count) ? this.actualInventory[slotNumber] : null;
				if (slot != null && this.highlightMethod(slot) && slot.canStackWith(toPlace))
				{
					int toRemove = toPlace.Stack - slot.addToStack(toPlace);
					if (toPlace.ConsumeStack(toRemove) == null)
					{
						try
						{
							Game1.playSound(sound, null);
							ItemGrabMenu.behaviorOnItemSelect behaviorOnItemSelect = this.onAddItem;
							if (behaviorOnItemSelect != null)
							{
								behaviorOnItemSelect(toPlace, this.playerInventory ? Game1.player : null);
							}
						}
						catch (Exception)
						{
						}
						return null;
					}
				}
			}
			foreach (ClickableComponent clickableComponent2 in this.inventory)
			{
				int slotNumber2 = Convert.ToInt32(clickableComponent2.name);
				Item slot2 = (slotNumber2 < this.actualInventory.Count) ? this.actualInventory[slotNumber2] : null;
				if (slotNumber2 < this.actualInventory.Count && slot2 == null)
				{
					if (!string.IsNullOrEmpty(sound))
					{
						try
						{
							Game1.playSound(sound, null);
						}
						catch (Exception)
						{
						}
					}
					return Utility.addItemToInventory(toPlace, slotNumber2, this.actualInventory, this.onAddItem);
				}
			}
			if (toPlace.Stack < originalStack)
			{
				Game1.playSound(sound, null);
			}
			return toPlace;
		}

		// Token: 0x060029E1 RID: 10721 RVA: 0x001EFFB4 File Offset: 0x001EE1B4
		public int getInventoryPositionOfClick(int x, int y)
		{
			for (int i = 0; i < this.inventory.Count; i++)
			{
				if (this.inventory[i] != null && this.inventory[i].bounds.Contains(x, y))
				{
					return Convert.ToInt32(this.inventory[i].name);
				}
			}
			return -1;
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x001F0018 File Offset: 0x001EE218
		public Item leftClick(int x, int y, Item toPlace, bool playSound = true)
		{
			foreach (ClickableComponent c in this.inventory)
			{
				if (c.containsPoint(x, y))
				{
					int slotNumber = Convert.ToInt32(c.name);
					if (slotNumber < this.actualInventory.Count && (this.actualInventory[slotNumber] == null || this.highlightMethod(this.actualInventory[slotNumber]) || this.actualInventory[slotNumber].canStackWith(toPlace)))
					{
						if (this.actualInventory[slotNumber] != null)
						{
							if (toPlace != null)
							{
								if (playSound)
								{
									Game1.playSound("stoneStep", null);
								}
								return Utility.addItemToInventory(toPlace, slotNumber, this.actualInventory, this.onAddItem);
							}
							if (playSound)
							{
								Game1.playSound(this.moveItemSound, null);
							}
							return Utility.removeItemFromInventory(slotNumber, this.actualInventory);
						}
						else if (toPlace != null)
						{
							if (playSound)
							{
								Game1.playSound("stoneStep", null);
							}
							return Utility.addItemToInventory(toPlace, slotNumber, this.actualInventory, this.onAddItem);
						}
					}
				}
			}
			return toPlace;
		}

		// Token: 0x060029E3 RID: 10723 RVA: 0x001F0180 File Offset: 0x001EE380
		public Vector2 snapToClickableComponent(int x, int y)
		{
			foreach (ClickableComponent c in this.inventory)
			{
				if (c.containsPoint(x, y))
				{
					return new Vector2((float)c.bounds.X, (float)c.bounds.Y);
				}
			}
			return new Vector2((float)x, (float)y);
		}

		// Token: 0x060029E4 RID: 10724 RVA: 0x001F0204 File Offset: 0x001EE404
		public Item getItemAt(int x, int y)
		{
			foreach (ClickableComponent c in this.inventory)
			{
				if (c.containsPoint(x, y))
				{
					return this.getItemFromClickableComponent(c);
				}
			}
			return null;
		}

		// Token: 0x060029E5 RID: 10725 RVA: 0x001F0268 File Offset: 0x001EE468
		public Item getItemFromClickableComponent(ClickableComponent c)
		{
			if (c != null)
			{
				int slotNumber = Convert.ToInt32(c.name);
				if (slotNumber < this.actualInventory.Count)
				{
					return this.actualInventory[slotNumber];
				}
			}
			return null;
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x001F02A0 File Offset: 0x001EE4A0
		public Item rightClick(int x, int y, Item toAddTo, bool playSound = true, bool onlyCheckToolAttachments = false)
		{
			foreach (ClickableComponent clickableComponent in this.inventory)
			{
				int slotNumber = Convert.ToInt32(clickableComponent.name);
				Item slot = (slotNumber < this.actualInventory.Count) ? this.actualInventory[slotNumber] : null;
				if (clickableComponent.containsPoint(x, y) && slotNumber < this.actualInventory.Count && (slot == null || this.highlightMethod(slot)) && slot != null)
				{
					Tool tool = slot as Tool;
					if (tool != null && (toAddTo == null || toAddTo is Object) && tool.canThisBeAttached((Object)toAddTo))
					{
						return tool.attach((Object)toAddTo);
					}
					if (onlyCheckToolAttachments)
					{
						return toAddTo;
					}
					if (toAddTo == null)
					{
						if (slot.maximumStackSize() != -1)
						{
							if (slotNumber == Game1.player.CurrentToolIndex && slot.Stack == 1)
							{
								slot.actionWhenStopBeingHeld(Game1.player);
							}
							Item newItem = slot.getOne();
							newItem.Stack = ((slot.Stack > 1 && Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[]
							{
								new InputButton(Keys.LeftShift)
							})) ? ((int)Math.Ceiling((double)slot.Stack / 2.0)) : 1);
							this.actualInventory[slotNumber] = slot.ConsumeStack(newItem.Stack);
							if (playSound)
							{
								Game1.playSound(this.moveItemSound, null);
							}
							return newItem;
						}
					}
					else if (slot.canStackWith(toAddTo) && toAddTo.Stack < toAddTo.maximumStackSize())
					{
						if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[]
						{
							new InputButton(Keys.LeftShift)
						}))
						{
							int amountToAdd = (int)Math.Ceiling((double)slot.Stack / 2.0);
							amountToAdd = Math.Min(toAddTo.maximumStackSize() - toAddTo.Stack, amountToAdd);
							toAddTo.Stack += amountToAdd;
							this.actualInventory[slotNumber] = slot.ConsumeStack(amountToAdd);
						}
						else
						{
							int stack = toAddTo.Stack;
							toAddTo.Stack = stack + 1;
							this.actualInventory[slotNumber] = slot.ConsumeStack(1);
						}
						if (playSound)
						{
							Game1.playSound(this.moveItemSound, null);
						}
						if (this.actualInventory[slotNumber] == null && slotNumber == Game1.player.CurrentToolIndex)
						{
							slot.actionWhenStopBeingHeld(Game1.player);
						}
						return toAddTo;
					}
				}
			}
			return toAddTo;
		}

		// Token: 0x060029E7 RID: 10727 RVA: 0x001F0568 File Offset: 0x001EE768
		public Item hover(int x, int y, Item heldItem)
		{
			this.descriptionText = "";
			this.descriptionTitle = "";
			this.hoverText = "";
			this.hoverTitle = "";
			Item toReturn = null;
			foreach (ClickableComponent c in this.inventory)
			{
				int slotNumber = Convert.ToInt32(c.name);
				c.scale = Math.Max(1f, c.scale - 0.025f);
				if (c.containsPoint(x, y) && slotNumber < this.actualInventory.Count && (this.actualInventory[slotNumber] == null || this.highlightMethod(this.actualInventory[slotNumber])) && slotNumber < this.actualInventory.Count && this.actualInventory[slotNumber] != null)
				{
					this.descriptionTitle = this.actualInventory[slotNumber].DisplayName;
					this.descriptionText = Environment.NewLine + this.actualInventory[slotNumber].getDescription();
					c.scale = Math.Min(c.scale + 0.05f, 1.1f);
					string s = this.actualInventory[slotNumber].getHoverBoxText(heldItem);
					if (s != null)
					{
						this.hoverText = s;
						this.hoverTitle = this.actualInventory[slotNumber].DisplayName;
					}
					else
					{
						this.hoverText = this.actualInventory[slotNumber].getDescription();
						this.hoverTitle = this.actualInventory[slotNumber].DisplayName;
					}
					if (toReturn == null)
					{
						toReturn = this.actualInventory[slotNumber];
					}
				}
			}
			Object returnObj = toReturn as Object;
			if (returnObj != null && Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).couldThisIngredienteBeUsedInABundle(returnObj))
			{
				GameMenu.bundleItemHovered = true;
			}
			return toReturn;
		}

		// Token: 0x060029E8 RID: 10728 RVA: 0x001F0784 File Offset: 0x001EE984
		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			List<ClickableComponent> list = this.inventory;
			if (list != null && list.Count > 0)
			{
				Game1.setMousePosition(this.inventory[0].bounds.Right - this.inventory[0].bounds.Width / 8, this.inventory[0].bounds.Bottom - this.inventory[0].bounds.Height / 8);
			}
		}

		// Token: 0x060029E9 RID: 10729 RVA: 0x001F0811 File Offset: 0x001EEA11
		public override void draw(SpriteBatch b)
		{
			this.draw(b, -1, -1, -1);
		}

		// Token: 0x060029EA RID: 10730 RVA: 0x001F0820 File Offset: 0x001EEA20
		public override void draw(SpriteBatch b, int red, int green, int blue)
		{
			for (int i = 0; i < this.inventory.Count; i++)
			{
				double endTime;
				if (this._iconShakeTimer.TryGetValue(i, out endTime) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= endTime)
				{
					this._iconShakeTimer.Remove(i);
				}
			}
			Color tint = (red == -1) ? Color.White : new Color((int)Utility.Lerp((float)red, (float)Math.Min(255, red + 150), 0.65f), (int)Utility.Lerp((float)green, (float)Math.Min(255, green + 150), 0.65f), (int)Utility.Lerp((float)blue, (float)Math.Min(255, blue + 150), 0.65f));
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			if (this.drawSlots)
			{
				for (int j = 0; j < this.capacity; j++)
				{
					Vector2 toDraw = new Vector2((float)(this.xPositionOnScreen + j % (this.capacity / this.rows) * 64 + this.horizontalGap * (j % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + j / (this.capacity / this.rows) * (64 + this.verticalGap) + (j / (this.capacity / this.rows) - 1) * 4 - ((j >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0) ? 0 : 12)));
					b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
					if ((this.playerInventory || this.showGrayedOutSlots) && j >= Game1.player.maxItems.Value)
					{
						b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57, -1, -1)), tint * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
					}
					if (!Game1.options.gamepadControls && j < 12 && this.playerInventory)
					{
						string strToDraw = (j == 9) ? "0" : ((j == 10) ? "-" : ((j == 11) ? "=" : ((j + 1).ToString() ?? "")));
						Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
						b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(32f - strSize.X / 2f, -strSize.Y), (j == Game1.player.CurrentToolIndex) ? Color.Red : Color.DimGray);
					}
				}
				for (int k = 0; k < this.capacity; k++)
				{
					Vector2 toDraw2 = new Vector2((float)(this.xPositionOnScreen + k % (this.capacity / this.rows) * 64 + this.horizontalGap * (k % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + k / (this.capacity / this.rows) * (64 + this.verticalGap) + (k / (this.capacity / this.rows) - 1) * 4 - ((k >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0) ? 0 : 12)));
					if (this.actualInventory.Count > k && this.actualInventory[k] != null)
					{
						bool highlight = this.highlightMethod(this.actualInventory[k]);
						if (this._iconShakeTimer.ContainsKey(k))
						{
							toDraw2 += 1f * new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
						}
						this.actualInventory[k].drawInMenu(b, toDraw2, (this.inventory.Count > k) ? this.inventory[k].scale : 1f, (!this.highlightMethod(this.actualInventory[k])) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, highlight);
					}
				}
				return;
			}
			for (int l = 0; l < this.capacity; l++)
			{
				Vector2 toDraw3 = new Vector2((float)(this.xPositionOnScreen + l % (this.capacity / this.rows) * 64 + this.horizontalGap * (l % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + l / (this.capacity / this.rows) * (64 + this.verticalGap) + (l / (this.capacity / this.rows) - 1) * 4 - ((l >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0) ? 0 : 12)));
				if (this.actualInventory.Count > l && this.actualInventory[l] != null)
				{
					bool highlight2 = this.highlightMethod(this.actualInventory[l]);
					if (this._iconShakeTimer.ContainsKey(l))
					{
						toDraw3 += 1f * new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
					}
					this.actualInventory[l].drawInMenu(b, toDraw3, (this.inventory.Count > l) ? this.inventory[l].scale : 1f, (!highlight2) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, highlight2);
				}
			}
		}

		// Token: 0x060029EB RID: 10731 RVA: 0x001F0E34 File Offset: 0x001EF034
		public List<Vector2> GetSlotDrawPositions()
		{
			List<Vector2> slot_draw_positions = new List<Vector2>();
			for (int i = 0; i < this.capacity; i++)
			{
				slot_draw_positions.Add(new Vector2((float)(this.xPositionOnScreen + i % (this.capacity / this.rows) * 64 + this.horizontalGap * (i % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + i / (this.capacity / this.rows) * (64 + this.verticalGap) + (i / (this.capacity / this.rows) - 1) * 4 - ((i >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0) ? 0 : 12))));
			}
			return slot_draw_positions;
		}

		// Token: 0x060029EC RID: 10732 RVA: 0x001F0EF6 File Offset: 0x001EF0F6
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060029ED RID: 10733 RVA: 0x001F0EF8 File Offset: 0x001EF0F8
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x04001B56 RID: 6998
		public const int region_inventorySlot0 = 0;

		// Token: 0x04001B57 RID: 6999
		public const int region_inventorySlot1 = 1;

		// Token: 0x04001B58 RID: 7000
		public const int region_inventorySlot2 = 2;

		// Token: 0x04001B59 RID: 7001
		public const int region_inventorySlot3 = 3;

		// Token: 0x04001B5A RID: 7002
		public const int region_inventorySlot4 = 4;

		// Token: 0x04001B5B RID: 7003
		public const int region_inventorySlot5 = 5;

		// Token: 0x04001B5C RID: 7004
		public const int region_inventorySlot6 = 6;

		// Token: 0x04001B5D RID: 7005
		public const int region_inventorySlot7 = 7;

		// Token: 0x04001B5E RID: 7006
		public const int region_dropButton = 107;

		// Token: 0x04001B5F RID: 7007
		public const int region_inventoryArea = 9000;

		// Token: 0x04001B60 RID: 7008
		public string hoverText = "";

		// Token: 0x04001B61 RID: 7009
		public string hoverTitle = "";

		// Token: 0x04001B62 RID: 7010
		public string descriptionTitle = "";

		// Token: 0x04001B63 RID: 7011
		public string descriptionText = "";

		// Token: 0x04001B64 RID: 7012
		public List<ClickableComponent> inventory = new List<ClickableComponent>();

		// Token: 0x04001B65 RID: 7013
		protected Dictionary<int, double> _iconShakeTimer = new Dictionary<int, double>();

		// Token: 0x04001B66 RID: 7014
		public IList<Item> actualInventory;

		// Token: 0x04001B67 RID: 7015
		public InventoryMenu.highlightThisItem highlightMethod;

		// Token: 0x04001B68 RID: 7016
		public ItemGrabMenu.behaviorOnItemSelect onAddItem;

		// Token: 0x04001B69 RID: 7017
		public bool playerInventory;

		// Token: 0x04001B6A RID: 7018
		public bool drawSlots;

		// Token: 0x04001B6B RID: 7019
		public bool showGrayedOutSlots;

		// Token: 0x04001B6C RID: 7020
		public int capacity;

		// Token: 0x04001B6D RID: 7021
		public int rows;

		// Token: 0x04001B6E RID: 7022
		public int horizontalGap;

		// Token: 0x04001B6F RID: 7023
		public int verticalGap;

		// Token: 0x04001B70 RID: 7024
		public ClickableComponent dropItemInvisibleButton;

		// Token: 0x04001B71 RID: 7025
		public string moveItemSound = "dwop";

		// Token: 0x0200060E RID: 1550
		// (Invoke) Token: 0x0600440D RID: 17421
		public delegate bool highlightThisItem(Item i);

		// Token: 0x0200060F RID: 1551
		public enum BorderSide
		{
			// Token: 0x04002E60 RID: 11872
			Top,
			// Token: 0x04002E61 RID: 11873
			Left,
			// Token: 0x04002E62 RID: 11874
			Right,
			// Token: 0x04002E63 RID: 11875
			Bottom
		}
	}
}
