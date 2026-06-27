using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x02000269 RID: 617
	public class DyeMenu : MenuWithInventory
	{
		// Token: 0x060028EB RID: 10475 RVA: 0x001E042C File Offset: 0x001DE62C
		public DyeMenu() : base(null, true, true, 12, 132, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			if (this.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			Game1.playSound("bigSelect", null);
			this.inventory.highlightMethod = new InventoryMenu.highlightThisItem(this.HighlightItems);
			this.dyeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\dye_bench");
			this.dyedClothesDisplays = new List<ClickableTextureComponent>();
			this._CreateButtons();
			if (this.trashCan != null)
			{
				this.trashCan.myID = 106;
			}
			if (this.okButton != null)
			{
				this.okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			this.GenerateHighlightDictionary();
			this._UpdateDescriptionText();
		}

		// Token: 0x060028EC RID: 10476 RVA: 0x001E0650 File Offset: 0x001DE850
		protected void _CreateButtons()
		{
			this._slotDrawPositions = this.inventory.GetSlotDrawPositions();
			Dictionary<int, Item> old_items = new Dictionary<int, Item>();
			if (this.dyePots != null)
			{
				for (int i = 0; i < this.dyePots.Count; i++)
				{
					old_items[i] = this.dyePots[i].item;
				}
			}
			this.dyePots = new List<ClickableTextureComponent>();
			for (int j = 0; j < this.validPotColors.Length; j++)
			{
				ClickableTextureComponent dye_pot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4 + 68 + 18 * j * 4, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 132, 64, 64), this.dyeTexture, new Rectangle(32 + 16 * j, 80, 16, 16), 4f, false)
				{
					myID = j + 5000,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					upNeighborID = -99998,
					item = old_items.GetValueOrDefault(j)
				};
				this.dyePots.Add(dye_pot);
			}
			this._dyeDropAnimationFrames = new int[this.dyePots.Count];
			for (int k = 0; k < this._dyeDropAnimationFrames.Length; k++)
			{
				this._dyeDropAnimationFrames[k] = -1;
			}
			this.dyeButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 200, 96, 96), this.dyeTexture, new Rectangle(0, 80, 24, 24), 4f, false)
			{
				myID = 1000,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((this.dyeButton != null) ? this.dyeButton.item : null)
			};
			List<ClickableComponent> inventory = this.inventory.inventory;
			if (inventory != null && inventory.Count >= 12)
			{
				for (int l = 0; l < 12; l++)
				{
					if (this.inventory.inventory[l] != null)
					{
						this.inventory.inventory[l].upNeighborID = -99998;
					}
				}
			}
			this.dyedClothesDisplays.Clear();
			this._dyedClothesDisplayPosition = new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 692), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232));
			Vector2 dyed_items_position = this._dyedClothesDisplayPosition;
			int drawn_items_count = 0;
			if (Game1.player.CanDyeShirt())
			{
				drawn_items_count++;
			}
			if (Game1.player.CanDyePants())
			{
				drawn_items_count++;
			}
			dyed_items_position.X -= (float)(drawn_items_count * 64 / 2);
			if (Game1.player.CanDyeShirt())
			{
				ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f, false);
				component.item = Game1.player.shirtItem.Value;
				dyed_items_position.X += 64f;
				this.dyedClothesDisplays.Add(component);
			}
			if (Game1.player.CanDyePants())
			{
				ClickableTextureComponent component2 = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f, false);
				component2.item = Game1.player.pantsItem.Value;
				dyed_items_position.X += 64f;
				this.dyedClothesDisplays.Add(component2);
			}
		}

		// Token: 0x060028ED RID: 10477 RVA: 0x001E0A31 File Offset: 0x001DEC31
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060028EE RID: 10478 RVA: 0x001E0A46 File Offset: 0x001DEC46
		public bool IsBusy()
		{
			return this._timeUntilCraft > 0;
		}

		// Token: 0x060028EF RID: 10479 RVA: 0x001E0A51 File Offset: 0x001DEC51
		public override bool readyToClose()
		{
			return base.readyToClose() && base.heldItem == null && !this.IsBusy();
		}

		// Token: 0x060028F0 RID: 10480 RVA: 0x001E0A70 File Offset: 0x001DEC70
		public bool HighlightItems(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (i != null && !i.canBeTrashed())
			{
				return false;
			}
			if (this._highlightDictionary == null)
			{
				this.GenerateHighlightDictionary();
			}
			if (!this._highlightDictionary.ContainsKey(i))
			{
				this._highlightDictionary = null;
				this.GenerateHighlightDictionary();
			}
			if (this._hoveredPotIndex >= 0)
			{
				return this._hoveredPotIndex == this._highlightDictionary[i];
			}
			return this._highlightDictionary[i] >= 0 && this.dyePots[this._highlightDictionary[i]].item == null;
		}

		// Token: 0x060028F1 RID: 10481 RVA: 0x001E0B08 File Offset: 0x001DED08
		public void GenerateHighlightDictionary()
		{
			this._highlightDictionary = new Dictionary<Item, int>();
			foreach (Item item in new List<Item>(this.inventory.actualInventory))
			{
				if (item != null)
				{
					this._highlightDictionary[item] = this.GetPotIndex(item);
				}
			}
		}

		// Token: 0x060028F2 RID: 10482 RVA: 0x001E0B80 File Offset: 0x001DED80
		private void _DyePotClicked(ClickableTextureComponent dyePot)
		{
			Item oldItem = dyePot.item;
			int index = this.dyePots.IndexOf(dyePot);
			if (index < 0)
			{
				return;
			}
			if (base.heldItem == null || (base.heldItem.canBeTrashed() && this.GetPotIndex(base.heldItem) == index))
			{
				bool removeHeldItem = false;
				if (dyePot.item != null && base.heldItem != null && dyePot.item.canStackWith(base.heldItem))
				{
					Item heldItem = base.heldItem;
					int stack = heldItem.Stack;
					heldItem.Stack = stack + 1;
					dyePot.item = null;
					Game1.playSound("quickSlosh", null);
					return;
				}
				Item heldItem2 = base.heldItem;
				dyePot.item = ((heldItem2 != null) ? heldItem2.getOne() : null);
				if (base.heldItem != null && base.heldItem.ConsumeStack(1) == null)
				{
					removeHeldItem = true;
				}
				if (base.heldItem != null && removeHeldItem)
				{
					base.heldItem = oldItem;
				}
				else if (base.heldItem != null && oldItem != null)
				{
					Item i = Game1.player.addItemToInventory(base.heldItem);
					if (i != null)
					{
						Game1.createItemDebris(i, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
					}
					base.heldItem = oldItem;
				}
				else if (oldItem != null)
				{
					base.heldItem = oldItem;
				}
				else if (base.heldItem != null && oldItem == null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
				{
					Game1.player.addItemToInventory(base.heldItem);
					base.heldItem = null;
				}
				if (oldItem != dyePot.item)
				{
					this._dyeDropAnimationFrames[index] = 0;
					Game1.playSound("quickSlosh", null);
					int count = 0;
					for (int j = 0; j < this.dyePots.Count; j++)
					{
						if (this.dyePots[j].item != null)
						{
							count++;
						}
					}
					if (count >= this.dyePots.Count)
					{
						DelayedAction.playSoundAfterDelay("newArtifact", 200, null, null, -1, false);
					}
				}
				this._highlightDictionary = null;
				this.GenerateHighlightDictionary();
			}
			this._UpdateDescriptionText();
		}

		// Token: 0x060028F3 RID: 10483 RVA: 0x001E0D9C File Offset: 0x001DEF9C
		public Color GetColorForPot(int index)
		{
			switch (index)
			{
			case 0:
				return new Color(220, 0, 0);
			case 1:
				return new Color(255, 128, 0);
			case 2:
				return new Color(255, 230, 0);
			case 3:
				return new Color(10, 143, 0);
			case 4:
				return new Color(46, 105, 203);
			case 5:
				return new Color(115, 41, 181);
			default:
				return Color.Black;
			}
		}

		// Token: 0x060028F4 RID: 10484 RVA: 0x001E0E2C File Offset: 0x001DF02C
		public int GetPotIndex(Item item)
		{
			for (int i = 0; i < this.validPotColors.Length; i++)
			{
				for (int j = 0; j < this.validPotColors[i].Length; j++)
				{
					ColoredObject colorObject = item as ColoredObject;
					if (colorObject != null && colorObject.preservedParentSheetIndex.Value != null && ItemContextTagManager.DoAnyTagsMatch(new List<string>
					{
						this.validPotColors[i][j]
					}, ItemContextTagManager.GetBaseContextTags(colorObject.preservedParentSheetIndex.Value)))
					{
						return i;
					}
					if (item.HasContextTag(this.validPotColors[i][j]))
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x060028F5 RID: 10485 RVA: 0x001E0EBB File Offset: 0x001DF0BB
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				if (base.heldItem != null && base.heldItem.canBeTrashed())
				{
					Utility.trashItem(base.heldItem);
					base.heldItem = null;
					return;
				}
			}
			else
			{
				base.receiveKeyPress(key);
			}
		}

		// Token: 0x060028F6 RID: 10486 RVA: 0x001E0EF4 File Offset: 0x001DF0F4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item oldHeldItem = base.heldItem;
			base.receiveLeftClick(x, y, base.heldItem != null || !Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift));
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && oldHeldItem != base.heldItem && base.heldItem != null)
			{
				foreach (ClickableTextureComponent pot in this.dyePots)
				{
					if (pot.item == null)
					{
						this._DyePotClicked(pot);
					}
					if (base.heldItem == null)
					{
						return;
					}
				}
			}
			if (!this.IsBusy())
			{
				bool wasHeldItem = base.heldItem != null;
				foreach (ClickableTextureComponent pot2 in this.dyePots)
				{
					if (pot2.containsPoint(x, y))
					{
						this._DyePotClicked(pot2);
						if (!wasHeldItem && base.heldItem != null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
						{
							base.heldItem = Game1.player.addItemToInventory(base.heldItem);
						}
						return;
					}
				}
				if (this.dyeButton.containsPoint(x, y))
				{
					if (base.heldItem == null && this.CanDye())
					{
						Game1.playSound("glug", null);
						foreach (ClickableTextureComponent dyePot in this.dyePots)
						{
							if (dyePot.item != null)
							{
								dyePot.item = dyePot.item.ConsumeStack(1);
							}
						}
						Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots, false);
						this._UpdateDescriptionText();
					}
					else
					{
						Game1.playSound("sell", null);
					}
				}
				if (base.heldItem != null && !this.isWithinBounds(x, y) && base.heldItem.canBeTrashed())
				{
					Game1.playSound("throwDownITem", null);
					Game1.createItemDebris(base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
					base.heldItem = null;
				}
			}
		}

		// Token: 0x060028F7 RID: 10487 RVA: 0x001E1168 File Offset: 0x001DF368
		public bool CanDye()
		{
			for (int i = 0; i < this.dyePots.Count; i++)
			{
				if (this.dyePots[i].item == null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x001E11A1 File Offset: 0x001DF3A1
		public static bool IsWearingDyeable()
		{
			return Game1.player.CanDyeShirt() || Game1.player.CanDyePants();
		}

		// Token: 0x060028F9 RID: 10489 RVA: 0x001E11BC File Offset: 0x001DF3BC
		protected void _UpdateDescriptionText()
		{
			if (!DyeMenu.IsWearingDyeable())
			{
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable");
				return;
			}
			if (this.CanDye())
			{
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_CanDye");
				return;
			}
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_Help");
		}

		// Token: 0x060028FA RID: 10490 RVA: 0x001E1219 File Offset: 0x001DF419
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.IsBusy())
			{
				return;
			}
			base.receiveRightClick(x, y, true);
		}

		// Token: 0x060028FB RID: 10491 RVA: 0x001E1230 File Offset: 0x001DF430
		public override void performHoverAction(int x, int y)
		{
			if (x <= this.dyePots[0].bounds.X || x >= this.dyePots.Last<ClickableTextureComponent>().bounds.Right || y <= this.dyePots[0].bounds.Y || y >= this.dyePots[0].bounds.Bottom)
			{
				this._hoveredPotIndex = -1;
			}
			if (this.IsBusy())
			{
				return;
			}
			this.hoveredItem = null;
			base.performHoverAction(x, y);
			this.hoverText = "";
			foreach (ClickableTextureComponent component in this.dyedClothesDisplays)
			{
				if (component.containsPoint(x, y))
				{
					this.hoveredItem = component.item;
				}
			}
			for (int i = 0; i < this.dyePots.Count; i++)
			{
				if (this.dyePots[i].containsPoint(x, y))
				{
					this.dyePots[i].tryHover(x, y, 0f);
					this._hoveredPotIndex = i;
				}
			}
			if (this.CanDye())
			{
				this.dyeButton.tryHover(x, y, 0.2f);
			}
		}

		// Token: 0x060028FC RID: 10492 RVA: 0x001E1384 File Offset: 0x001DF584
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, false, null, this.inventory.highlightMethod, -1, 3, 0, 0, true);
			this._CreateButtons();
		}

		// Token: 0x060028FD RID: 10493 RVA: 0x001E13F8 File Offset: 0x001DF5F8
		public override void emergencyShutDown()
		{
			this._OnCloseMenu();
			base.emergencyShutDown();
		}

		// Token: 0x060028FE RID: 10494 RVA: 0x001E1408 File Offset: 0x001DF608
		public override void update(GameTime time)
		{
			base.update(time);
			this.descriptionText = this.displayedDescription;
			if (this.CanDye())
			{
				this.dyeButton.sourceRect.Y = 180;
				this.dyeButton.sourceRect.X = (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 24;
			}
			else
			{
				this.dyeButton.sourceRect.Y = 80;
				this.dyeButton.sourceRect.X = 0;
			}
			for (int i = 0; i < this.dyePots.Count; i++)
			{
				if (this._dyeDropAnimationFrames[i] >= 0)
				{
					this._dyeDropAnimationFrames[i] += time.ElapsedGameTime.Milliseconds;
					if (this._dyeDropAnimationFrames[i] >= 500)
					{
						this._dyeDropAnimationFrames[i] = -1;
					}
				}
			}
		}

		// Token: 0x060028FF RID: 10495 RVA: 0x001E14F8 File Offset: 0x001DF6F8
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			base.draw(b, true, true, 50, 160, 255);
			b.Draw(this.dyeTexture, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder)), new Rectangle?(new Rectangle(0, 0, 142, 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			for (int i = 0; i < this._slotDrawPositions.Count; i++)
			{
				int index;
				if (i < this.inventory.actualInventory.Count && this.inventory.actualInventory[i] != null && this._highlightDictionary.TryGetValue(this.inventory.actualInventory[i], out index) && index >= 0)
				{
					Color color = this.GetColorForPot(index);
					if (this._hoveredPotIndex == -1 && this.HighlightItems(this.inventory.actualInventory[i]))
					{
						b.Draw(this.dyeTexture, this._slotDrawPositions[i], new Rectangle?(new Rectangle(32, 96, 32, 32)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					}
				}
			}
			this.dyeButton.draw(b, Color.White * (this.CanDye() ? 1f : 0.55f), 0.96f, 0, 0, 0);
			this.dyeButton.drawItem(b, 16, 16, 1f);
			string make_result_text = Game1.content.LoadString("Strings\\UI:DyePot_WillDye");
			Vector2 dyed_items_position = this._dyedClothesDisplayPosition;
			Vector2 text_position = new Vector2(dyed_items_position.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)((int)dyed_items_position.Y) - Game1.smallFont.MeasureString(make_result_text).Y);
			Utility.drawTextWithColoredShadow(b, make_result_text, Game1.smallFont, text_position, Game1.textColor * 0.75f, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
			foreach (ClickableTextureComponent clickableTextureComponent in this.dyedClothesDisplays)
			{
				clickableTextureComponent.drawItem(b, 0, 0, 1f);
			}
			for (int j = 0; j < this.dyePots.Count; j++)
			{
				this.dyePots[j].drawItem(b, 0, -16, 1f);
				if (this._dyeDropAnimationFrames[j] >= 0)
				{
					Color color2 = this.GetColorForPot(j);
					b.Draw(this.dyeTexture, new Vector2((float)this.dyePots[j].bounds.X, (float)(this.dyePots[j].bounds.Y - 12)), new Rectangle?(new Rectangle(this._dyeDropAnimationFrames[j] / 50 * 16, 128, 16, 16)), color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				}
				this.dyePots[j].draw(b);
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, (base.heldItem != null) ? 32 : 0, (base.heldItem != null) ? 32 : 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			else if (this.hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, base.heldItem != null, -1, 0, null, -1, null, -1, null);
			}
			Item heldItem = base.heldItem;
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
			}
			if (!Game1.options.hardwareCursor)
			{
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x06002900 RID: 10496 RVA: 0x001E1990 File Offset: 0x001DFB90
		protected override void cleanupBeforeExit()
		{
			this._OnCloseMenu();
		}

		// Token: 0x06002901 RID: 10497 RVA: 0x001E1998 File Offset: 0x001DFB98
		protected void _OnCloseMenu()
		{
			Utility.CollectOrDrop(base.heldItem);
			for (int i = 0; i < this.dyePots.Count; i++)
			{
				if (this.dyePots[i].item != null)
				{
					Utility.CollectOrDrop(this.dyePots[i].item);
				}
			}
			base.heldItem = null;
			this.dyeButton.item = null;
		}

		// Token: 0x04001AB3 RID: 6835
		protected int _timeUntilCraft;

		// Token: 0x04001AB4 RID: 6836
		public List<ClickableTextureComponent> dyePots;

		// Token: 0x04001AB5 RID: 6837
		public ClickableTextureComponent dyeButton;

		// Token: 0x04001AB6 RID: 6838
		public const int DYE_POT_ID_OFFSET = 5000;

		// Token: 0x04001AB7 RID: 6839
		public Texture2D dyeTexture;

		// Token: 0x04001AB8 RID: 6840
		protected Dictionary<Item, int> _highlightDictionary;

		// Token: 0x04001AB9 RID: 6841
		protected List<Vector2> _slotDrawPositions;

		// Token: 0x04001ABA RID: 6842
		protected int _hoveredPotIndex = -1;

		// Token: 0x04001ABB RID: 6843
		protected int[] _dyeDropAnimationFrames;

		// Token: 0x04001ABC RID: 6844
		public const int MILLISECONDS_PER_DROP_FRAME = 50;

		// Token: 0x04001ABD RID: 6845
		public const int TOTAL_DROP_FRAMES = 10;

		// Token: 0x04001ABE RID: 6846
		public string[][] validPotColors = new string[][]
		{
			new string[]
			{
				"color_red",
				"color_salmon",
				"color_dark_red",
				"color_pink"
			},
			new string[]
			{
				"color_orange",
				"color_dark_orange",
				"color_dark_brown",
				"color_brown",
				"color_copper"
			},
			new string[]
			{
				"color_yellow",
				"color_dark_yellow",
				"color_gold",
				"color_sand"
			},
			new string[]
			{
				"color_green",
				"color_dark_green",
				"color_lime",
				"color_yellow_green",
				"color_jade"
			},
			new string[]
			{
				"color_blue",
				"color_dark_blue",
				"color_dark_cyan",
				"color_light_cyan",
				"color_cyan",
				"color_aquamarine"
			},
			new string[]
			{
				"color_purple",
				"color_dark_purple",
				"color_dark_pink",
				"color_pale_violet_red",
				"color_poppyseed",
				"color_iridium"
			}
		};

		// Token: 0x04001ABF RID: 6847
		protected string displayedDescription = "";

		// Token: 0x04001AC0 RID: 6848
		public List<ClickableTextureComponent> dyedClothesDisplays;

		// Token: 0x04001AC1 RID: 6849
		protected Vector2 _dyedClothesDisplayPosition;
	}
}
