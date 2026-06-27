using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x020002AF RID: 687
	public class TailoringMenu : MenuWithInventory
	{
		// Token: 0x06002CC9 RID: 11465 RVA: 0x0022B3B8 File Offset: 0x002295B8
		public TailoringMenu() : base(null, true, true, 12, 132, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			Game1.playSound("bigSelect", null);
			if (this.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			this.inventory.highlightMethod = new InventoryMenu.highlightThisItem(this.HighlightItems);
			this.tailoringTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\tailoring");
			this._tailoringRecipes = DataLoader.TailoringRecipes(Game1.temporaryContent);
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
			this._ValidateCraft();
		}

		// Token: 0x06002CCA RID: 11466 RVA: 0x0022B4B8 File Offset: 0x002296B8
		protected void _CreateButtons()
		{
			this.leftIngredientSpot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), this.tailoringTextures, new Rectangle(0, 156, 24, 24), 4f, false)
			{
				myID = 998,
				downNeighborID = -99998,
				leftNeighborID = 109,
				rightNeighborID = 996,
				upNeighborID = 997,
				item = ((this.leftIngredientSpot != null) ? this.leftIngredientSpot.item : null)
			};
			this.leftIngredientStartSpot = new Vector2((float)this.leftIngredientSpot.bounds.X, (float)this.leftIngredientSpot.bounds.Y);
			this.leftIngredientEndSpot = this.leftIngredientStartSpot + new Vector2(256f, 0f);
			this.needleSprite = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(64, 80, 16, 32), 4f, false);
			this.presserSprite = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(48, 80, 16, 32), 4f, false);
			this.needlePosition = new Vector2((float)this.needleSprite.bounds.X, (float)this.needleSprite.bounds.Y);
			this.presserPosition = new Vector2((float)this.presserSprite.bounds.X, (float)this.presserSprite.bounds.Y);
			this.rightIngredientSpot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), this.tailoringTextures, new Rectangle(0, 180, 24, 24), 4f, false)
			{
				myID = 997,
				downNeighborID = 996,
				leftNeighborID = 998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((this.rightIngredientSpot != null) ? this.rightIngredientSpot.item : null),
				fullyImmutable = true
			};
			this.blankRightIngredientSpot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), this.tailoringTextures, new Rectangle(0, 128, 24, 24), 4f, false);
			this.blankLeftIngredientSpot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), this.tailoringTextures, new Rectangle(0, 128, 24, 24), 4f, false);
			this.startTailoringButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(24, 80, 24, 24), 4f, false)
			{
				myID = 996,
				downNeighborID = -99998,
				leftNeighborID = 998,
				rightNeighborID = 995,
				upNeighborID = 997,
				item = ((this.startTailoringButton != null) ? this.startTailoringButton.item : null),
				fullyImmutable = true
			};
			List<ClickableComponent> inventory = this.inventory.inventory;
			if (inventory != null && inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (this.inventory.inventory[i] != null)
					{
						this.inventory.inventory[i].upNeighborID = -99998;
					}
				}
			}
			this.equipmentIcons = new List<ClickableComponent>
			{
				new ClickableComponent(new Rectangle(0, 0, 64, 64), "Hat")
				{
					myID = 101,
					leftNeighborID = -99998,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborID = -99998
				},
				new ClickableComponent(new Rectangle(0, 0, 64, 64), "Shirt")
				{
					myID = 108,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998
				},
				new ClickableComponent(new Rectangle(0, 0, 64, 64), "Pants")
				{
					myID = 109,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				}
			};
			for (int j = 0; j < this.equipmentIcons.Count; j++)
			{
				this.equipmentIcons[j].bounds.X = this.xPositionOnScreen - 64 + 9;
				this.equipmentIcons[j].bounds.Y = this.yPositionOnScreen + 192 + j * 64;
			}
			ClickableTextureComponent clickableTextureComponent = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), this.tailoringTextures, new Rectangle(0, 208, 16, 16), 4f, false);
			clickableTextureComponent.myID = 995;
			clickableTextureComponent.downNeighborID = -99998;
			clickableTextureComponent.leftNeighborID = 996;
			clickableTextureComponent.upNeighborID = 997;
			ClickableTextureComponent clickableTextureComponent2 = this.craftResultDisplay;
			clickableTextureComponent.item = ((clickableTextureComponent2 != null) ? clickableTextureComponent2.item : null);
			this.craftResultDisplay = clickableTextureComponent;
		}

		// Token: 0x06002CCB RID: 11467 RVA: 0x0022BB3A File Offset: 0x00229D3A
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002CCC RID: 11468 RVA: 0x0022BB4F File Offset: 0x00229D4F
		public bool IsBusy()
		{
			return this._timeUntilCraft > 0;
		}

		// Token: 0x06002CCD RID: 11469 RVA: 0x0022BB5A File Offset: 0x00229D5A
		public override bool readyToClose()
		{
			return base.readyToClose() && base.heldItem == null && !this.IsBusy();
		}

		// Token: 0x06002CCE RID: 11470 RVA: 0x0022BB77 File Offset: 0x00229D77
		public bool HighlightItems(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (!this.ItemHighlightCache.ContainsKey(i))
			{
				this.BuildHighlightCache();
			}
			return this.ItemHighlightCache[i].AnySlot;
		}

		// Token: 0x06002CCF RID: 11471 RVA: 0x0022BBA4 File Offset: 0x00229DA4
		public void BuildHighlightCache()
		{
			this.ItemHighlightCache.Clear();
			List<Item> list = new List<Item>(this.inventory.actualInventory);
			list.Add(Game1.player.pantsItem.Value);
			list.Add(Game1.player.shirtItem.Value);
			list.Add(Game1.player.hat.Value);
			Item leftItem = this.leftIngredientSpot.item;
			Item rightItem = this.rightIngredientSpot.item;
			bool leftFree = leftItem == null;
			bool rightFree = rightItem == null;
			foreach (Item item in list)
			{
				if (item != null)
				{
					if ((!leftFree && !rightFree) || !this.IsValidCraftIngredient(item))
					{
						this.ItemHighlightCache[item] = new TailoringMenu.TailorHighlight();
					}
					else if (!this.IsValidCraftIngredient(item))
					{
						this.ItemHighlightCache[item] = new TailoringMenu.TailorHighlight(false, false, item is Hat || item is Clothing);
					}
					else if (leftFree != rightFree)
					{
						this.ItemHighlightCache[item] = new TailoringMenu.TailorHighlight(leftFree && this.IsValidCraft(item, rightItem), rightFree && this.IsValidCraft(leftItem, item), false);
					}
					else
					{
						bool validForLeft = false;
						bool validForRight = false;
						if (item is Boots)
						{
							validForLeft = true;
							validForRight = true;
						}
						else
						{
							Clothing clothing = item as Clothing;
							if (clothing != null && clothing.dyeable.Value)
							{
								validForLeft = true;
							}
							else if (item.HasContextTag("color_prismatic") || TailoringMenu.GetDyeColor(item) != null)
							{
								validForRight = true;
							}
						}
						foreach (TailorItemRecipe recipe in this._tailoringRecipes)
						{
							if (validForLeft && validForRight)
							{
								break;
							}
							validForLeft = (validForLeft || this.HasRequiredTags(item, recipe.FirstItemTags));
							validForRight = (validForRight || this.HasRequiredTags(item, recipe.SecondItemTags));
						}
						this.ItemHighlightCache[item] = new TailoringMenu.TailorHighlight(validForLeft, validForRight, item is Hat || item is Clothing);
					}
				}
			}
		}

		// Token: 0x06002CD0 RID: 11472 RVA: 0x0022BE24 File Offset: 0x0022A024
		private void _leftIngredientSpotClicked()
		{
			if (base.heldItem != null)
			{
				TailoringMenu.TailorHighlight valueOrDefault = this.ItemHighlightCache.GetValueOrDefault(base.heldItem);
				if (!(((valueOrDefault != null) ? new bool?(valueOrDefault.LeftSlot) : null) ?? true))
				{
					return;
				}
			}
			Item old_item = this.leftIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				Game1.playSound("stoneStep", null);
				this.leftIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this.ItemHighlightCache.Clear();
				this._ValidateCraft();
			}
		}

		// Token: 0x06002CD1 RID: 11473 RVA: 0x0022BED6 File Offset: 0x0022A0D6
		public bool IsValidCraftIngredient(Item item)
		{
			return item.HasContextTag("item_lucky_purple_shorts") || item.canBeTrashed();
		}

		// Token: 0x06002CD2 RID: 11474 RVA: 0x0022BEF0 File Offset: 0x0022A0F0
		private void _rightIngredientSpotClicked()
		{
			if (base.heldItem != null)
			{
				TailoringMenu.TailorHighlight valueOrDefault = this.ItemHighlightCache.GetValueOrDefault(base.heldItem);
				if (!(((valueOrDefault != null) ? new bool?(valueOrDefault.RightSlot) : null) ?? true))
				{
					return;
				}
			}
			Item old_item = this.rightIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				Game1.playSound("stoneStep", null);
				this.rightIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this.ItemHighlightCache.Clear();
				this._ValidateCraft();
			}
		}

		// Token: 0x06002CD3 RID: 11475 RVA: 0x0022BFA4 File Offset: 0x0022A1A4
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				Item heldItem = base.heldItem;
				bool? flag = (heldItem != null) ? new bool?(heldItem.canBeTrashed()) : null;
				if (flag != null && flag.GetValueOrDefault())
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

		// Token: 0x06002CD4 RID: 11476 RVA: 0x0022C004 File Offset: 0x0022A204
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item oldHeldItem = base.heldItem;
			bool flag = Game1.player.IsEquippedItem(oldHeldItem);
			base.receiveLeftClick(x, y, true);
			if (flag && base.heldItem != oldHeldItem)
			{
				if (oldHeldItem == Game1.player.hat.Value)
				{
					Game1.player.Equip<Hat>(null, Game1.player.hat);
					this.ItemHighlightCache.Clear();
				}
				else if (oldHeldItem == Game1.player.shirtItem.Value)
				{
					Game1.player.Equip<Clothing>(null, Game1.player.shirtItem);
					this.ItemHighlightCache.Clear();
				}
				else if (oldHeldItem == Game1.player.pantsItem.Value)
				{
					Game1.player.Equip<Clothing>(null, Game1.player.pantsItem);
					this.ItemHighlightCache.Clear();
				}
			}
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (!(name == "Hat"))
					{
						if (!(name == "Shirt"))
						{
							if (name == "Pants")
							{
								Item item_to_place = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
								if (base.heldItem == null)
								{
									if (this.HighlightItems(Game1.player.pantsItem.Value))
									{
										base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.pantsItem.Value);
										if (!(base.heldItem is Clothing))
										{
											Game1.player.Equip<Clothing>(null, Game1.player.pantsItem);
										}
										Game1.playSound("dwop", null);
										this.ItemHighlightCache.Clear();
										this._ValidateCraft();
									}
								}
								else
								{
									Clothing pants = item_to_place as Clothing;
									if (pants != null && pants.clothesType.Value == Clothing.ClothesType.PANTS)
									{
										Item old_item = Game1.player.pantsItem.Value;
										old_item = Utility.PerformSpecialItemGrabReplacement(old_item);
										if (old_item == base.heldItem)
										{
											old_item = null;
										}
										Game1.player.Equip<Clothing>(pants, Game1.player.pantsItem);
										base.heldItem = old_item;
										Game1.playSound("sandyStep", null);
										this.ItemHighlightCache.Clear();
										this._ValidateCraft();
									}
								}
							}
						}
						else
						{
							Item item_to_place2 = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
							if (base.heldItem == null)
							{
								if (this.HighlightItems(Game1.player.shirtItem.Value))
								{
									base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.shirtItem.Value);
									Game1.playSound("dwop", null);
									if (!(base.heldItem is Clothing))
									{
										Game1.player.Equip<Clothing>(null, Game1.player.shirtItem);
									}
									this.ItemHighlightCache.Clear();
									this._ValidateCraft();
								}
							}
							else
							{
								Clothing shirt = item_to_place2 as Clothing;
								if (shirt != null && shirt.clothesType.Value == Clothing.ClothesType.SHIRT)
								{
									Item old_item2 = Game1.player.shirtItem.Value;
									old_item2 = Utility.PerformSpecialItemGrabReplacement(old_item2);
									if (old_item2 == base.heldItem)
									{
										old_item2 = null;
									}
									Game1.player.Equip<Clothing>(shirt, Game1.player.shirtItem);
									base.heldItem = old_item2;
									Game1.playSound("sandyStep", null);
									this.ItemHighlightCache.Clear();
									this._ValidateCraft();
								}
							}
						}
					}
					else
					{
						Item item_to_place3 = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
						if (base.heldItem == null)
						{
							if (this.HighlightItems(Game1.player.hat.Value))
							{
								base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.hat.Value);
								Game1.playSound("dwop", null);
								if (!(base.heldItem is Hat))
								{
									Game1.player.Equip<Hat>(null, Game1.player.hat);
								}
								this.ItemHighlightCache.Clear();
								this._ValidateCraft();
							}
						}
						else
						{
							Hat hat = item_to_place3 as Hat;
							if (hat != null)
							{
								Item old_item3 = Game1.player.hat.Value;
								old_item3 = Utility.PerformSpecialItemGrabReplacement(old_item3);
								if (old_item3 == base.heldItem)
								{
									old_item3 = null;
								}
								Game1.player.Equip<Hat>(hat, Game1.player.hat);
								base.heldItem = old_item3;
								Game1.playSound("grassyStep", null);
								this.ItemHighlightCache.Clear();
								this._ValidateCraft();
							}
						}
					}
					return;
				}
			}
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && oldHeldItem != base.heldItem && base.heldItem != null)
			{
				if (!(base.heldItem.QualifiedItemId == "(O)428"))
				{
					Clothing clothing = base.heldItem as Clothing;
					if (clothing == null || !clothing.dyeable.Value)
					{
						this._rightIngredientSpotClicked();
						goto IL_51C;
					}
				}
				this._leftIngredientSpotClicked();
			}
			IL_51C:
			if (!this.IsBusy())
			{
				if (this.leftIngredientSpot.containsPoint(x, y))
				{
					this._leftIngredientSpotClicked();
					if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
					{
						if (Game1.player.IsEquippedItem(base.heldItem))
						{
							base.heldItem = null;
						}
						else
						{
							base.heldItem = this.inventory.tryToAddItem(base.heldItem, "");
						}
					}
				}
				else if (this.rightIngredientSpot.containsPoint(x, y))
				{
					this._rightIngredientSpotClicked();
					if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
					{
						if (Game1.player.IsEquippedItem(base.heldItem))
						{
							base.heldItem = null;
						}
						else
						{
							base.heldItem = this.inventory.tryToAddItem(base.heldItem, "");
						}
					}
				}
				else if (this.startTailoringButton.containsPoint(x, y))
				{
					if (base.heldItem == null)
					{
						bool fail = false;
						if (!this.CanFitCraftedItem())
						{
							Game1.playSound("cancel", null);
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
							this._timeUntilCraft = 0;
							fail = true;
						}
						if (!fail && this.IsValidCraft(this.leftIngredientSpot.item, this.rightIngredientSpot.item))
						{
							Game1.playSound("bigSelect", null);
							Game1.playSound("sewing_loop", out this._sewingSound);
							this.startTailoringButton.scale = this.startTailoringButton.baseScale;
							this._timeUntilCraft = 1500;
							this._UpdateDescriptionText();
						}
						else
						{
							Game1.playSound("sell", null);
						}
					}
					else
					{
						Game1.playSound("sell", null);
					}
				}
				if (base.heldItem != null && !this.isWithinBounds(x, y) && base.heldItem.canBeTrashed())
				{
					if (Game1.player.IsEquippedItem(base.heldItem))
					{
						if (base.heldItem == Game1.player.hat.Value)
						{
							Game1.player.Equip<Hat>(null, Game1.player.hat);
						}
						else if (base.heldItem == Game1.player.shirtItem.Value)
						{
							Game1.player.Equip<Clothing>(null, Game1.player.shirtItem);
						}
						else if (base.heldItem == Game1.player.pantsItem.Value)
						{
							Game1.player.Equip<Clothing>(null, Game1.player.pantsItem);
						}
					}
					Game1.playSound("throwDownITem", null);
					Game1.createItemDebris(base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
					base.heldItem = null;
				}
			}
		}

		// Token: 0x06002CD5 RID: 11477 RVA: 0x0022C844 File Offset: 0x0022AA44
		protected void _ValidateCraft()
		{
			Item left_item = this.leftIngredientSpot.item;
			Item right_item = this.rightIngredientSpot.item;
			if (left_item == null || right_item == null)
			{
				this._craftState = TailoringMenu.CraftState.MissingIngredients;
			}
			else
			{
				Clothing clothing = left_item as Clothing;
				if (clothing != null && !clothing.dyeable.Value)
				{
					this._craftState = TailoringMenu.CraftState.NotDyeable;
				}
				else if (this.IsValidCraft(left_item, right_item))
				{
					this._craftState = TailoringMenu.CraftState.Valid;
					bool should_prismatic_dye = this._shouldPrismaticDye;
					Item left_item_clone = left_item.getOne();
					if (this.IsMultipleResultCraft(left_item, right_item))
					{
						this._isMultipleResultCraft = true;
					}
					else
					{
						this._isMultipleResultCraft = false;
					}
					this.craftResultDisplay.item = this.CraftItem(left_item_clone, right_item.getOne());
					if (this.craftResultDisplay.item == left_item_clone)
					{
						this._isDyeCraft = true;
					}
					else
					{
						this._isDyeCraft = false;
					}
					this._shouldPrismaticDye = should_prismatic_dye;
				}
				else
				{
					this._craftState = TailoringMenu.CraftState.InvalidRecipe;
				}
			}
			this._UpdateDescriptionText();
		}

		// Token: 0x06002CD6 RID: 11478 RVA: 0x0022C928 File Offset: 0x0022AB28
		protected void _UpdateDescriptionText()
		{
			if (this.IsBusy())
			{
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Busy");
				return;
			}
			switch (this._craftState)
			{
			case TailoringMenu.CraftState.MissingIngredients:
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_MissingIngredients");
				return;
			case TailoringMenu.CraftState.Valid:
				this.displayedDescription = ((!this.CanFitCraftedItem()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588") : Game1.content.LoadString("Strings\\UI:Tailor_Valid"));
				return;
			case TailoringMenu.CraftState.InvalidRecipe:
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_InvalidRecipe");
				return;
			case TailoringMenu.CraftState.NotDyeable:
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_NotDyeable");
				return;
			default:
				this.displayedDescription = "";
				return;
			}
		}

		// Token: 0x06002CD7 RID: 11479 RVA: 0x0022C9F0 File Offset: 0x0022ABF0
		public static Color? GetDyeColor(Item dye_object)
		{
			if (dye_object == null)
			{
				return null;
			}
			if (dye_object.QualifiedItemId == "(O)74")
			{
				return new Color?(Color.White);
			}
			ColoredObject coloredObject = dye_object as ColoredObject;
			if (coloredObject != null)
			{
				return new Color?(coloredObject.color.Value);
			}
			return ItemContextTagManager.GetColorFromTags(dye_object);
		}

		// Token: 0x06002CD8 RID: 11480 RVA: 0x0022CA48 File Offset: 0x0022AC48
		public bool DyeItems(Clothing clothing, Item dye_object, float dye_strength_override = -1f)
		{
			if (dye_object.QualifiedItemId == "(O)74")
			{
				clothing.Dye(Color.White, 1f);
				clothing.isPrismatic.Set(true);
				return true;
			}
			Color? dye_color = TailoringMenu.GetDyeColor(dye_object);
			if (dye_color != null)
			{
				float dye_strength = 0.25f;
				if (dye_object.HasContextTag("dye_medium"))
				{
					dye_strength = 0.5f;
				}
				if (dye_object.HasContextTag("dye_strong"))
				{
					dye_strength = 1f;
				}
				if (dye_strength_override >= 0f)
				{
					dye_strength = dye_strength_override;
				}
				clothing.Dye(dye_color.Value, dye_strength);
				if (clothing == Game1.player.shirtItem.Value || clothing == Game1.player.pantsItem.Value)
				{
					Game1.player.FarmerRenderer.MarkSpriteDirty();
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002CD9 RID: 11481 RVA: 0x0022CB10 File Offset: 0x0022AD10
		public TailorItemRecipe GetRecipeForItems(Item leftItem, Item rightItem)
		{
			if (leftItem != null && rightItem != null)
			{
				foreach (TailorItemRecipe recipe in this._tailoringRecipes)
				{
					if (this.HasRequiredTags(leftItem, recipe.FirstItemTags) && this.HasRequiredTags(rightItem, recipe.SecondItemTags))
					{
						return recipe;
					}
				}
			}
			return null;
		}

		// Token: 0x06002CDA RID: 11482 RVA: 0x0022CB88 File Offset: 0x0022AD88
		private bool HasRequiredTags(Item item, List<string> requiredTags)
		{
			if (item != null && requiredTags != null && requiredTags.Count > 0)
			{
				foreach (string tag in requiredTags)
				{
					if (!item.HasContextTag(tag))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002CDB RID: 11483 RVA: 0x0022CBF0 File Offset: 0x0022ADF0
		public bool IsValidCraft(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return false;
			}
			if (left_item is Boots && right_item is Boots)
			{
				return true;
			}
			Clothing clothing = left_item as Clothing;
			if (clothing != null && clothing.dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					return true;
				}
				if (TailoringMenu.GetDyeColor(right_item) != null)
				{
					return true;
				}
			}
			return this.GetRecipeForItems(left_item, right_item) != null;
		}

		// Token: 0x06002CDC RID: 11484 RVA: 0x0022CC5C File Offset: 0x0022AE5C
		public bool IsMultipleResultCraft(Item left_item, Item right_item)
		{
			TailorItemRecipe recipeForItems = this.GetRecipeForItems(left_item, right_item);
			if (recipeForItems == null)
			{
				return false;
			}
			List<string> craftedItemIds = recipeForItems.CraftedItemIds;
			int? num = (craftedItemIds != null) ? new int?(craftedItemIds.Count) : null;
			int num2 = 0;
			return num.GetValueOrDefault() > num2 & num != null;
		}

		// Token: 0x06002CDD RID: 11485 RVA: 0x0022CCAC File Offset: 0x0022AEAC
		public Item CraftItem(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return null;
			}
			Boots leftBoots = left_item as Boots;
			if (leftBoots != null)
			{
				Boots rightBoots = right_item as Boots;
				if (rightBoots != null)
				{
					leftBoots.applyStats(rightBoots);
					return leftBoots;
				}
			}
			Clothing leftClothing = left_item as Clothing;
			if (leftClothing != null && leftClothing.dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					this._shouldPrismaticDye = true;
					return leftClothing;
				}
				if (this.DyeItems(leftClothing, right_item, -1f))
				{
					return leftClothing;
				}
			}
			TailorItemRecipe recipe = this.GetRecipeForItems(left_item, right_item);
			if (recipe != null)
			{
				string crafted_item_id;
				if (recipe.CraftedItemIdFeminine != null && !Game1.player.IsMale)
				{
					crafted_item_id = recipe.CraftedItemIdFeminine;
				}
				else
				{
					List<string> craftedItemIds = recipe.CraftedItemIds;
					if (craftedItemIds != null && craftedItemIds.Count > 0)
					{
						crafted_item_id = Game1.random.ChooseFrom(recipe.CraftedItemIds);
					}
					else
					{
						crafted_item_id = recipe.CraftedItemId;
					}
				}
				crafted_item_id = TailoringMenu.ConvertLegacyItemId(crafted_item_id);
				Item item = ItemRegistry.Create(crafted_item_id, 1, 0, false);
				Clothing craftedClothing = item as Clothing;
				if (craftedClothing != null)
				{
					this.DyeItems(craftedClothing, right_item, 1f);
				}
				Object craftedObj = item as Object;
				if (craftedObj != null)
				{
					Object leftObj = left_item as Object;
					if (leftObj == null || !leftObj.questItem.Value)
					{
						Object rightObj = right_item as Object;
						if (rightObj == null || !rightObj.questItem.Value)
						{
							return item;
						}
					}
					craftedObj.questItem.Value = true;
				}
				return item;
			}
			return null;
		}

		// Token: 0x06002CDE RID: 11486 RVA: 0x0022CDFC File Offset: 0x0022AFFC
		public static string ConvertLegacyItemId(string id)
		{
			int legacyId;
			if (!int.TryParse(id, out legacyId))
			{
				return id;
			}
			if (legacyId < 0)
			{
				return "(O)" + (-legacyId).ToString();
			}
			if (legacyId >= 2000 && legacyId < 3000)
			{
				return "(H)" + (legacyId - 2000).ToString();
			}
			if (legacyId >= 1000)
			{
				return "(S)" + legacyId.ToString();
			}
			return "(P)" + legacyId.ToString();
		}

		// Token: 0x06002CDF RID: 11487 RVA: 0x0022CE83 File Offset: 0x0022B083
		public void SpendRightItem()
		{
			ClickableComponent clickableComponent = this.rightIngredientSpot;
			Item item = this.rightIngredientSpot.item;
			clickableComponent.item = ((item != null) ? item.ConsumeStack(1) : null);
		}

		// Token: 0x06002CE0 RID: 11488 RVA: 0x0022CEA8 File Offset: 0x0022B0A8
		public void SpendLeftItem()
		{
			ClickableComponent clickableComponent = this.leftIngredientSpot;
			Item item = this.leftIngredientSpot.item;
			clickableComponent.item = ((item != null) ? item.ConsumeStack(1) : null);
		}

		// Token: 0x06002CE1 RID: 11489 RVA: 0x0022CECD File Offset: 0x0022B0CD
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.IsBusy())
			{
				return;
			}
			base.receiveRightClick(x, y, true);
		}

		// Token: 0x06002CE2 RID: 11490 RVA: 0x0022CEE4 File Offset: 0x0022B0E4
		public override void performHoverAction(int x, int y)
		{
			if (this.IsBusy())
			{
				return;
			}
			this.hoveredItem = null;
			base.performHoverAction(x, y);
			this.hoverText = "";
			for (int i = 0; i < this.equipmentIcons.Count; i++)
			{
				if (this.equipmentIcons[i].containsPoint(x, y))
				{
					string name = this.equipmentIcons[i].name;
					if (!(name == "Shirt"))
					{
						if (!(name == "Hat"))
						{
							if (name == "Pants")
							{
								this.hoveredItem = Game1.player.pantsItem.Value;
							}
						}
						else
						{
							this.hoveredItem = Game1.player.hat.Value;
						}
					}
					else
					{
						this.hoveredItem = Game1.player.shirtItem.Value;
					}
				}
			}
			if (this.craftResultDisplay.visible && this.craftResultDisplay.containsPoint(x, y) && this.craftResultDisplay.item != null)
			{
				if (this._isDyeCraft || Game1.player.HasTailoredThisItem(this.craftResultDisplay.item))
				{
					this.hoveredItem = this.craftResultDisplay.item;
				}
				else
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_MakeResultUnknown");
				}
			}
			if (this.leftIngredientSpot.containsPoint(x, y))
			{
				if (this.leftIngredientSpot.item != null)
				{
					this.hoveredItem = this.leftIngredientSpot.item;
				}
				else
				{
					this.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Feed");
				}
			}
			if (this.rightIngredientSpot.containsPoint(x, y) && this.rightIngredientSpot.item == null)
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Spool");
			}
			this.rightIngredientSpot.tryHover(x, y, 0.1f);
			this.leftIngredientSpot.tryHover(x, y, 0.1f);
			if (this._craftState == TailoringMenu.CraftState.Valid && this.CanFitCraftedItem())
			{
				this.startTailoringButton.tryHover(x, y, 0.33f);
				return;
			}
			this.startTailoringButton.tryHover(-999, -999, 0.1f);
		}

		// Token: 0x06002CE3 RID: 11491 RVA: 0x0022D10A File Offset: 0x0022B30A
		public bool CanFitCraftedItem()
		{
			return this.craftResultDisplay.item == null || Utility.canItemBeAddedToThisInventoryList(this.craftResultDisplay.item, this.inventory.actualInventory, -1);
		}

		// Token: 0x06002CE4 RID: 11492 RVA: 0x0022D13C File Offset: 0x0022B33C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, false, null, this.inventory.highlightMethod, -1, 3, 0, 0, true);
			this._CreateButtons();
		}

		// Token: 0x06002CE5 RID: 11493 RVA: 0x0022D1B0 File Offset: 0x0022B3B0
		public override void emergencyShutDown()
		{
			this._OnCloseMenu();
			base.emergencyShutDown();
		}

		// Token: 0x06002CE6 RID: 11494 RVA: 0x0022D1C0 File Offset: 0x0022B3C0
		public override void update(GameTime time)
		{
			base.update(time);
			this.descriptionText = this.displayedDescription;
			this.questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
			this.questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
			bool can_fit_crafted_item = this.CanFitCraftedItem();
			this.startTailoringButton.sourceRect.Y = ((this._craftState == TailoringMenu.CraftState.Valid && can_fit_crafted_item) ? 104 : 80);
			this.craftResultDisplay.visible = (this._craftState == TailoringMenu.CraftState.Valid && !this.IsBusy() && can_fit_crafted_item);
			if (this._timeUntilCraft > 0)
			{
				this.startTailoringButton.tryHover(this.startTailoringButton.bounds.Center.X, this.startTailoringButton.bounds.Center.Y, 0.33f);
				this.leftIngredientSpot.bounds.X = (int)Utility.Lerp(this.leftIngredientEndSpot.X, this.leftIngredientStartSpot.X, (float)this._timeUntilCraft / 1500f);
				this.leftIngredientSpot.bounds.Y = (int)Utility.Lerp(this.leftIngredientEndSpot.Y, this.leftIngredientStartSpot.Y, (float)this._timeUntilCraft / 1500f);
				this._timeUntilCraft -= time.ElapsedGameTime.Milliseconds;
				this.needleSprite.bounds.Location = new Point((int)this.needlePosition.X, (int)(this.needlePosition.Y - 2f * ((float)this._timeUntilCraft % 25f) / 25f * 4f));
				this.presserSprite.bounds.Location = new Point((int)this.presserPosition.X, (int)(this.presserPosition.Y - 1f * ((float)this._timeUntilCraft % 50f) / 50f * 4f));
				this._rightItemOffset = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 2.0 * 3.141592653589793 / 180.0) * 2f;
				if (this._timeUntilCraft > 0)
				{
					return;
				}
				TailorItemRecipe recipe = this.GetRecipeForItems(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
				this._shouldPrismaticDye = false;
				Item crafted_item = this.CraftItem(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
				if (this._sewingSound != null && this._sewingSound.IsPlaying)
				{
					this._sewingSound.Stop(AudioStopOptions.Immediate);
				}
				if (!Utility.canItemBeAddedToThisInventoryList(crafted_item, this.inventory.actualInventory, -1))
				{
					Game1.playSound("cancel", null);
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
					this._timeUntilCraft = 0;
					return;
				}
				if (this.leftIngredientSpot.item == crafted_item)
				{
					this.leftIngredientSpot.item = null;
				}
				else
				{
					this.SpendLeftItem();
				}
				if ((recipe == null || recipe.SpendRightItem) && (this.readyToClose() || !this._shouldPrismaticDye))
				{
					this.SpendRightItem();
				}
				if (recipe != null)
				{
					Game1.player.MarkItemAsTailored(crafted_item);
				}
				Game1.playSound("coin", null);
				base.heldItem = crafted_item;
				this._timeUntilCraft = 0;
				this._ValidateCraft();
				if (this._shouldPrismaticDye)
				{
					Item old_held_item = base.heldItem;
					base.heldItem = null;
					if (this.readyToClose())
					{
						base.exitThisMenuNoSound();
						Game1.activeClickableMenu = new CharacterCustomization(crafted_item as Clothing);
						return;
					}
					base.heldItem = old_held_item;
				}
			}
			this._rightItemOffset = 0f;
			this.leftIngredientSpot.bounds.X = (int)this.leftIngredientStartSpot.X;
			this.leftIngredientSpot.bounds.Y = (int)this.leftIngredientStartSpot.Y;
			this.needleSprite.bounds.Location = new Point((int)this.needlePosition.X, (int)this.needlePosition.Y);
			this.presserSprite.bounds.Location = new Point((int)this.presserPosition.X, (int)this.presserPosition.Y);
		}

		// Token: 0x06002CE7 RID: 11495 RVA: 0x0022D644 File Offset: 0x0022B844
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 96f, (float)(this.yPositionOnScreen - 64)), new Rectangle?(new Rectangle(101, 80, 41, 36)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 352f, (float)(this.yPositionOnScreen - 64)), new Rectangle?(new Rectangle(101, 80, 41, 36)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 608f, (float)(this.yPositionOnScreen - 64)), new Rectangle?(new Rectangle(101, 80, 41, 36)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 256f, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(79, 97, 22, 20)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 512f, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(79, 97, 22, 20)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 32f, (float)(this.yPositionOnScreen + 44)), new Rectangle?(new Rectangle(81, 81, 16, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(this.tailoringTextures, new Vector2((float)this.xPositionOnScreen + 768f, (float)(this.yPositionOnScreen + 44)), new Rectangle?(new Rectangle(81, 81, 16, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Game1.DrawBox(this.xPositionOnScreen - 64, this.yPositionOnScreen + 128, 128, 265, new Color?(new Color(50, 160, 255)));
			Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(this.xPositionOnScreen - 64) + 9.6f, (float)(this.yPositionOnScreen + 128)), 0.87f, 4f, 2, Game1.player, 1f);
			base.draw(b, true, true, 50, 160, 255);
			b.Draw(this.tailoringTextures, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder)), new Rectangle?(new Rectangle(0, 0, 142, 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			this.startTailoringButton.draw(b, Color.White, 0.96f, 0, 0, 0);
			this.startTailoringButton.drawItem(b, 16, 16, 1f);
			this.presserSprite.draw(b, Color.White, 0.99f, 0, 0, 0);
			this.needleSprite.draw(b, Color.White, 0.97f, 0, 0, 0);
			Point random_shaking = new Point(0, 0);
			if (!this.IsBusy())
			{
				Color color3;
				if (base.heldItem != null)
				{
					TailoringMenu.TailorHighlight valueOrDefault = this.ItemHighlightCache.GetValueOrDefault(base.heldItem);
					if (!(((valueOrDefault != null) ? new bool?(valueOrDefault.LeftSlot) : null) ?? false))
					{
						color3 = Color.White * 0.5f;
						goto IL_44B;
					}
				}
				color3 = Color.White;
				IL_44B:
				Color color = color3;
				if (this.leftIngredientSpot.item != null)
				{
					this.blankLeftIngredientSpot.draw(b, color, 0.87f, 0, 0, 0);
				}
				else
				{
					this.leftIngredientSpot.draw(b, color, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200, 0, 0);
				}
			}
			else
			{
				random_shaking.X = Game1.random.Next(-1, 2);
				random_shaking.Y = Game1.random.Next(-1, 2);
			}
			this.leftIngredientSpot.drawItem(b, (4 + random_shaking.X) * 4, (4 + random_shaking.Y) * 4, 1f);
			if (this.craftResultDisplay.visible)
			{
				string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
				Vector2 text_position = new Vector2((float)this.craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)this.craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y);
				Utility.drawTextWithColoredShadow(b, make_result_text, Game1.smallFont, text_position, Game1.textColor * 0.75f, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
				this.craftResultDisplay.draw(b);
				if (this.craftResultDisplay.item != null)
				{
					if (this._isMultipleResultCraft)
					{
						Rectangle question_mark_bounds = this.craftResultDisplay.bounds;
						question_mark_bounds.X += 6;
						question_mark_bounds.Y -= 8 + (int)this.questionMarkOffset.Y;
						b.Draw(this.tailoringTextures, question_mark_bounds, new Rectangle?(new Rectangle(112, 208, 16, 16)), Color.White);
					}
					else if (this._isDyeCraft || Game1.player.HasTailoredThisItem(this.craftResultDisplay.item))
					{
						this.craftResultDisplay.drawItem(b, 0, 0, 1f);
					}
					else
					{
						Item item = this.craftResultDisplay.item;
						if (!(item is Hat))
						{
							Clothing clothing = item as Clothing;
							if (clothing == null)
							{
								Object crafted_object = item as Object;
								if (crafted_object != null)
								{
									if (crafted_object.QualifiedItemId == "(O)71")
									{
										b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle?(new Rectangle(64, 208, 16, 16)), Color.White);
									}
								}
							}
							else
							{
								Clothing.ClothesType value = clothing.clothesType.Value;
								if (value != Clothing.ClothesType.SHIRT)
								{
									if (value == Clothing.ClothesType.PANTS)
									{
										b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle?(new Rectangle(64, 208, 16, 16)), Color.White);
									}
								}
								else
								{
									b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle?(new Rectangle(80, 208, 16, 16)), Color.White);
								}
							}
						}
						else
						{
							b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle?(new Rectangle(96, 208, 16, 16)), Color.White);
						}
						Rectangle question_mark_bounds2 = this.craftResultDisplay.bounds;
						question_mark_bounds2.X += 24;
						question_mark_bounds2.Y += 12 + (int)this.questionMarkOffset.Y;
						b.Draw(this.tailoringTextures, question_mark_bounds2, new Rectangle?(new Rectangle(112, 208, 16, 16)), Color.White);
					}
				}
			}
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				string name = c.name;
				if (!(name == "Hat"))
				{
					if (!(name == "Shirt"))
					{
						if (name == "Pants")
						{
							if (Game1.player.pantsItem.Value != null)
							{
								b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(0, 208, 16, 16)), Color.White);
								if (!this.HighlightItems(Game1.player.pantsItem.Value) || Game1.player.pantsItem.Value == base.heldItem)
								{
									goto IL_B0F;
								}
								if (base.heldItem != null)
								{
									Clothing clothing2 = base.heldItem as Clothing;
									if (clothing2 == null || clothing2.clothesType.Value != Clothing.ClothesType.PANTS)
									{
										goto IL_B0F;
									}
								}
								float num = 1f;
								IL_B14:
								float transparency = num;
								Game1.player.pantsItem.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, transparency, 0.866f);
								continue;
								IL_B0F:
								num = 0.5f;
								goto IL_B14;
							}
							b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(16, 208, 16, 16)), Color.White);
						}
					}
					else
					{
						if (Game1.player.shirtItem.Value != null)
						{
							b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(0, 208, 16, 16)), Color.White);
							if (!this.HighlightItems(Game1.player.shirtItem.Value) || Game1.player.shirtItem.Value == base.heldItem)
							{
								goto IL_9EE;
							}
							if (base.heldItem != null)
							{
								Clothing clothing3 = base.heldItem as Clothing;
								if (clothing3 == null || clothing3.clothesType.Value > Clothing.ClothesType.SHIRT)
								{
									goto IL_9EE;
								}
							}
							float num2 = 1f;
							IL_9F3:
							float transparency2 = num2;
							Game1.player.shirtItem.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, transparency2, 0.866f);
							continue;
							IL_9EE:
							num2 = 0.5f;
							goto IL_9F3;
						}
						b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(32, 208, 16, 16)), Color.White);
					}
				}
				else if (Game1.player.hat.Value != null)
				{
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(0, 208, 16, 16)), Color.White);
					float transparency3 = (!this.HighlightItems(Game1.player.hat.Value) || Game1.player.hat.Value == base.heldItem || (base.heldItem != null && !(base.heldItem is Hat))) ? 0.5f : 1f;
					Game1.player.hat.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, transparency3, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle?(new Rectangle(48, 208, 16, 16)), Color.White);
				}
			}
			if (!this.IsBusy())
			{
				Color color4;
				if (base.heldItem != null)
				{
					TailoringMenu.TailorHighlight valueOrDefault2 = this.ItemHighlightCache.GetValueOrDefault(base.heldItem);
					if (!(((valueOrDefault2 != null) ? new bool?(valueOrDefault2.RightSlot) : null) ?? false))
					{
						color4 = Color.White * 0.5f;
						goto IL_C0A;
					}
				}
				color4 = Color.White;
				IL_C0A:
				Color color2 = color4;
				if (this.rightIngredientSpot.item != null)
				{
					this.blankRightIngredientSpot.draw(b, color2, 0.87f, 0, 0, 0);
				}
				else
				{
					this.rightIngredientSpot.draw(b, color2, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200, 0, 0);
				}
			}
			this.rightIngredientSpot.drawItem(b, 16, (4 + (int)this._rightItemOffset) * 4, 1f);
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

		// Token: 0x06002CE8 RID: 11496 RVA: 0x0022E3E4 File Offset: 0x0022C5E4
		protected override void cleanupBeforeExit()
		{
			this._OnCloseMenu();
		}

		// Token: 0x06002CE9 RID: 11497 RVA: 0x0022E3EC File Offset: 0x0022C5EC
		protected void _OnCloseMenu()
		{
			if (!Game1.player.IsEquippedItem(base.heldItem))
			{
				Utility.CollectOrDrop(base.heldItem);
			}
			if (!Game1.player.IsEquippedItem(this.leftIngredientSpot.item))
			{
				Utility.CollectOrDrop(this.leftIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(this.rightIngredientSpot.item))
			{
				Utility.CollectOrDrop(this.rightIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(this.startTailoringButton.item))
			{
				Utility.CollectOrDrop(this.startTailoringButton.item);
			}
			base.heldItem = null;
			this.leftIngredientSpot.item = null;
			this.rightIngredientSpot.item = null;
			this.startTailoringButton.item = null;
		}

		// Token: 0x04001E7F RID: 7807
		protected int _timeUntilCraft;

		// Token: 0x04001E80 RID: 7808
		public const int region_leftIngredient = 998;

		// Token: 0x04001E81 RID: 7809
		public const int region_rightIngredient = 997;

		// Token: 0x04001E82 RID: 7810
		public const int region_startButton = 996;

		// Token: 0x04001E83 RID: 7811
		public const int region_resultItem = 995;

		// Token: 0x04001E84 RID: 7812
		public ClickableTextureComponent needleSprite;

		// Token: 0x04001E85 RID: 7813
		public ClickableTextureComponent presserSprite;

		// Token: 0x04001E86 RID: 7814
		public ClickableTextureComponent craftResultDisplay;

		// Token: 0x04001E87 RID: 7815
		public Vector2 needlePosition;

		// Token: 0x04001E88 RID: 7816
		public Vector2 presserPosition;

		// Token: 0x04001E89 RID: 7817
		public Vector2 leftIngredientStartSpot;

		// Token: 0x04001E8A RID: 7818
		public Vector2 leftIngredientEndSpot;

		// Token: 0x04001E8B RID: 7819
		protected float _rightItemOffset;

		// Token: 0x04001E8C RID: 7820
		public ClickableTextureComponent leftIngredientSpot;

		// Token: 0x04001E8D RID: 7821
		public ClickableTextureComponent rightIngredientSpot;

		// Token: 0x04001E8E RID: 7822
		public ClickableTextureComponent blankLeftIngredientSpot;

		// Token: 0x04001E8F RID: 7823
		public ClickableTextureComponent blankRightIngredientSpot;

		// Token: 0x04001E90 RID: 7824
		public ClickableTextureComponent startTailoringButton;

		// Token: 0x04001E91 RID: 7825
		public const int region_shirt = 108;

		// Token: 0x04001E92 RID: 7826
		public const int region_pants = 109;

		// Token: 0x04001E93 RID: 7827
		public const int region_hat = 101;

		// Token: 0x04001E94 RID: 7828
		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		// Token: 0x04001E95 RID: 7829
		public const int CRAFT_TIME = 1500;

		// Token: 0x04001E96 RID: 7830
		public Texture2D tailoringTextures;

		// Token: 0x04001E97 RID: 7831
		public List<TailorItemRecipe> _tailoringRecipes;

		// Token: 0x04001E98 RID: 7832
		private ICue _sewingSound;

		// Token: 0x04001E99 RID: 7833
		private readonly Dictionary<Item, TailoringMenu.TailorHighlight> ItemHighlightCache = new Dictionary<Item, TailoringMenu.TailorHighlight>();

		// Token: 0x04001E9A RID: 7834
		protected bool _shouldPrismaticDye;

		// Token: 0x04001E9B RID: 7835
		protected bool _isDyeCraft;

		// Token: 0x04001E9C RID: 7836
		protected bool _isMultipleResultCraft;

		// Token: 0x04001E9D RID: 7837
		protected string displayedDescription = "";

		// Token: 0x04001E9E RID: 7838
		protected TailoringMenu.CraftState _craftState;

		// Token: 0x04001E9F RID: 7839
		public Vector2 questionMarkOffset;

		// Token: 0x0200063E RID: 1598
		protected enum CraftState
		{
			// Token: 0x04002EFF RID: 12031
			MissingIngredients,
			// Token: 0x04002F00 RID: 12032
			Valid,
			// Token: 0x04002F01 RID: 12033
			InvalidRecipe,
			// Token: 0x04002F02 RID: 12034
			NotDyeable
		}

		// Token: 0x0200063F RID: 1599
		public class TailorHighlight
		{
			// Token: 0x060044C7 RID: 17607 RVA: 0x0031D66E File Offset: 0x0031B86E
			public TailorHighlight()
			{
			}

			// Token: 0x060044C8 RID: 17608 RVA: 0x0031D676 File Offset: 0x0031B876
			public TailorHighlight(bool leftSlot, bool rightSlot, bool equipmentSlot)
			{
				this.LeftSlot = leftSlot;
				this.RightSlot = rightSlot;
				this.EquipmentSlot = equipmentSlot;
				this.AnySlot = (leftSlot || rightSlot || equipmentSlot);
			}

			// Token: 0x04002F03 RID: 12035
			public readonly bool LeftSlot;

			// Token: 0x04002F04 RID: 12036
			public readonly bool RightSlot;

			// Token: 0x04002F05 RID: 12037
			public readonly bool EquipmentSlot;

			// Token: 0x04002F06 RID: 12038
			public readonly bool AnySlot;
		}
	}
}
