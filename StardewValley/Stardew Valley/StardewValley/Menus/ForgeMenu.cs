using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	// Token: 0x02000272 RID: 626
	public class ForgeMenu : MenuWithInventory
	{
		// Token: 0x0600294D RID: 10573 RVA: 0x001E5A90 File Offset: 0x001E3C90
		public ForgeMenu() : base(null, true, true, 12, 132, 0, ItemExitBehavior.ReturnToPlayer, false)
		{
			Game1.playSound("bigSelect", null);
			if (this.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			this.inventory.highlightMethod = new InventoryMenu.highlightThisItem(this.HighlightItems);
			this.forgeTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\ForgeMenu");
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

		// Token: 0x0600294E RID: 10574 RVA: 0x001E5B80 File Offset: 0x001E3D80
		protected void _CreateButtons()
		{
			ClickableTextureComponent clickableTextureComponent = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 204, this.yPositionOnScreen + 212, 64, 64), this.forgeTextures, new Rectangle(142, 0, 16, 16), 4f, false);
			clickableTextureComponent.myID = 998;
			clickableTextureComponent.downNeighborID = -99998;
			clickableTextureComponent.leftNeighborID = 110;
			clickableTextureComponent.rightNeighborID = 997;
			ClickableTextureComponent clickableTextureComponent2 = this.leftIngredientSpot;
			clickableTextureComponent.item = ((clickableTextureComponent2 != null) ? clickableTextureComponent2.item : null);
			clickableTextureComponent.fullyImmutable = true;
			this.leftIngredientSpot = clickableTextureComponent;
			ClickableTextureComponent clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 348, this.yPositionOnScreen + 212, 64, 64), this.forgeTextures, new Rectangle(142, 0, 16, 16), 4f, false);
			clickableTextureComponent3.myID = 997;
			clickableTextureComponent3.downNeighborID = 996;
			clickableTextureComponent3.leftNeighborID = 998;
			clickableTextureComponent3.rightNeighborID = 994;
			ClickableTextureComponent clickableTextureComponent4 = this.rightIngredientSpot;
			clickableTextureComponent3.item = ((clickableTextureComponent4 != null) ? clickableTextureComponent4.item : null);
			clickableTextureComponent3.fullyImmutable = true;
			this.rightIngredientSpot = clickableTextureComponent3;
			ClickableTextureComponent clickableTextureComponent5 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 204, this.yPositionOnScreen + 308, 52, 56), this.forgeTextures, new Rectangle(0, 80, 13, 14), 4f, false);
			clickableTextureComponent5.myID = 996;
			clickableTextureComponent5.downNeighborID = -99998;
			clickableTextureComponent5.leftNeighborID = 111;
			clickableTextureComponent5.rightNeighborID = 994;
			clickableTextureComponent5.upNeighborID = 998;
			ClickableTextureComponent clickableTextureComponent6 = this.startTailoringButton;
			clickableTextureComponent5.item = ((clickableTextureComponent6 != null) ? clickableTextureComponent6.item : null);
			clickableTextureComponent5.fullyImmutable = true;
			this.startTailoringButton = clickableTextureComponent5;
			this.unforgeButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 484, this.yPositionOnScreen + 312, 40, 44), "Unforge")
			{
				myID = 994,
				downNeighborID = -99998,
				leftNeighborID = 996,
				rightNeighborID = 995,
				upNeighborID = 997,
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
			ClickableTextureComponent clickableTextureComponent7 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), this.forgeTextures, new Rectangle(0, 208, 16, 16), 4f, false);
			clickableTextureComponent7.myID = 995;
			clickableTextureComponent7.downNeighborID = -99998;
			clickableTextureComponent7.leftNeighborID = 996;
			clickableTextureComponent7.upNeighborID = 997;
			ClickableTextureComponent clickableTextureComponent8 = this.craftResultDisplay;
			clickableTextureComponent7.item = ((clickableTextureComponent8 != null) ? clickableTextureComponent8.item : null);
			this.craftResultDisplay = clickableTextureComponent7;
			this.equipmentIcons = new List<ClickableComponent>();
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring1")
			{
				myID = 110,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborID = -99998
			});
			this.equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring2")
			{
				myID = 111,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			});
			for (int j = 0; j < this.equipmentIcons.Count; j++)
			{
				this.equipmentIcons[j].bounds.X = this.xPositionOnScreen - 64 + 9;
				this.equipmentIcons[j].bounds.Y = this.yPositionOnScreen + 192 + j * 64;
			}
		}

		// Token: 0x0600294F RID: 10575 RVA: 0x001E5FC4 File Offset: 0x001E41C4
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002950 RID: 10576 RVA: 0x001E5FD9 File Offset: 0x001E41D9
		public bool IsBusy()
		{
			return this._timeUntilCraft > 0 || this._sparklingTimer > 0;
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x001E5FEF File Offset: 0x001E41EF
		public override bool readyToClose()
		{
			return base.readyToClose() && base.heldItem == null && !this.IsBusy();
		}

		// Token: 0x06002952 RID: 10578 RVA: 0x001E600C File Offset: 0x001E420C
		public bool HighlightItems(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (i != null && !this.IsValidCraftIngredient(i))
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
			return this._highlightDictionary[i];
		}

		// Token: 0x06002953 RID: 10579 RVA: 0x001E6064 File Offset: 0x001E4264
		public virtual void GenerateHighlightDictionary()
		{
			this._highlightDictionary = new Dictionary<Item, bool>();
			List<Item> item_list = new List<Item>(this.inventory.actualInventory);
			if (Game1.player.leftRing.Value != null)
			{
				item_list.Add(Game1.player.leftRing.Value);
			}
			if (Game1.player.rightRing.Value != null)
			{
				item_list.Add(Game1.player.rightRing.Value);
			}
			foreach (Item item in item_list)
			{
				if (item != null)
				{
					if (item.QualifiedItemId == "(O)848")
					{
						this._highlightDictionary[item] = true;
					}
					else
					{
						if (this.leftIngredientSpot.item == null && this.rightIngredientSpot.item == null)
						{
							if (item is Ring)
							{
								goto IL_EF;
							}
							Tool tool = item as Tool;
							if (tool != null && BaseEnchantment.GetAvailableEnchantmentsForItem(tool).Count > 0)
							{
								goto IL_EF;
							}
							bool flag = BaseEnchantment.GetEnchantmentFromItem(null, item) != null;
							IL_F0:
							bool valid = flag;
							this._highlightDictionary[item] = valid;
							continue;
							IL_EF:
							flag = true;
							goto IL_F0;
						}
						if (this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null)
						{
							this._highlightDictionary[item] = false;
						}
						else if (this.leftIngredientSpot.item != null)
						{
							this._highlightDictionary[item] = this.IsValidCraft(this.leftIngredientSpot.item, item);
						}
						else
						{
							this._highlightDictionary[item] = this.IsValidCraft(item, this.rightIngredientSpot.item);
						}
					}
				}
			}
		}

		// Token: 0x06002954 RID: 10580 RVA: 0x001E6220 File Offset: 0x001E4420
		private void _leftIngredientSpotClicked()
		{
			Item old_item = this.leftIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				if (base.heldItem != null && !(base.heldItem is Tool) && !(base.heldItem is Ring))
				{
					return;
				}
				Game1.playSound("stoneStep", null);
				this.leftIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this._highlightDictionary = null;
				this._ValidateCraft();
			}
		}

		// Token: 0x06002955 RID: 10581 RVA: 0x001E62AC File Offset: 0x001E44AC
		public virtual bool IsValidCraftIngredient(Item item)
		{
			if (!item.canBeTrashed())
			{
				Tool tool = item as Tool;
				if (tool == null || BaseEnchantment.GetAvailableEnchantmentsForItem(tool).Count <= 0)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002956 RID: 10582 RVA: 0x001E62DC File Offset: 0x001E44DC
		private void _rightIngredientSpotClicked()
		{
			Item old_item = this.rightIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				Item heldItem = base.heldItem;
				if (((heldItem != null) ? heldItem.QualifiedItemId : null) == "(O)848")
				{
					return;
				}
				Game1.playSound("stoneStep", null);
				this.rightIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this._highlightDictionary = null;
				this._ValidateCraft();
			}
		}

		// Token: 0x06002957 RID: 10583 RVA: 0x001E6363 File Offset: 0x001E4563
		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				if (base.heldItem != null && this.IsValidCraftIngredient(base.heldItem))
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

		// Token: 0x06002958 RID: 10584 RVA: 0x001E639C File Offset: 0x001E459C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item old_held_item = base.heldItem;
			base.receiveLeftClick(x, y, true);
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (!(name == "Ring1"))
					{
						if (name == "Ring2")
						{
							if (this.HighlightItems(Game1.player.rightRing.Value) || Game1.player.rightRing.Value == null)
							{
								Item item_to_place = base.heldItem;
								if (item_to_place != Game1.player.rightRing.Value)
								{
									if (item_to_place == null || item_to_place is Ring)
									{
										base.heldItem = Game1.player.Equip<Ring>(item_to_place as Ring, Game1.player.rightRing);
										if (Game1.player.rightRing.Value != null)
										{
											Game1.playSound("crit", null);
										}
										else if (base.heldItem != null)
										{
											Game1.playSound("dwop", null);
										}
										this._highlightDictionary = null;
										this._ValidateCraft();
									}
								}
							}
						}
					}
					else if (this.HighlightItems(Game1.player.leftRing.Value) || Game1.player.leftRing.Value == null)
					{
						Item item_to_place2 = base.heldItem;
						if (item_to_place2 != Game1.player.leftRing.Value)
						{
							if (item_to_place2 == null || item_to_place2 is Ring)
							{
								base.heldItem = Game1.player.Equip<Ring>(item_to_place2 as Ring, Game1.player.leftRing);
								if (Game1.player.leftRing.Value != null)
								{
									Game1.playSound("crit", null);
								}
								else if (base.heldItem != null)
								{
									Game1.playSound("dwop", null);
								}
								this._highlightDictionary = null;
								this._ValidateCraft();
							}
						}
					}
					return;
				}
			}
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && old_held_item != base.heldItem && base.heldItem != null)
			{
				if (base.heldItem is Tool || (base.heldItem is Ring && this.leftIngredientSpot.item == null))
				{
					this._leftIngredientSpotClicked();
				}
				else
				{
					this._rightIngredientSpotClicked();
				}
			}
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
						if (!fail && this.IsValidCraft(this.leftIngredientSpot.item, this.rightIngredientSpot.item) && Game1.player.Items.ContainsId("(O)848", this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item)))
						{
							Game1.playSound("bigSelect", null);
							this.startTailoringButton.scale = this.startTailoringButton.baseScale;
							this._timeUntilCraft = 1600;
							this._clankEffectTimer = 300;
							this._UpdateDescriptionText();
							int crystals = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
							for (int i = 0; i < crystals; i++)
							{
								this.tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2((float)(this.xPositionOnScreen + 276), (float)(this.yPositionOnScreen + 300)), false, 0.1f, Color.White)
								{
									texture = this.forgeTextures,
									motion = new Vector2(-4f, -4f),
									scale = 4f,
									layerDepth = 1f,
									startSound = "boulderCrack",
									delayBeforeAnimationStart = 1400 / crystals * i
								});
							}
							Item item = this.rightIngredientSpot.item;
							if (((item != null) ? item.QualifiedItemId : null) == "(O)74")
							{
								this._sparklingTimer = 900;
								Rectangle r = this.leftIngredientSpot.bounds;
								r.Offset(-32, -32);
								TemporaryAnimatedSpriteList sparkles = Utility.sparkleWithinArea(r, 6, Color.White, 80, 1600, "");
								sparkles[0].startSound = "discoverMineral";
								this.tempSprites.AddRange(sparkles);
								r = this.rightIngredientSpot.bounds;
								r.Inflate(-16, -16);
								int num = 30;
								for (int j = 0; j < num; j++)
								{
									Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
									this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position, false, 0f, Color.White)
									{
										motion = new Vector2(-4f, 0f),
										yPeriodic = true,
										yPeriodicRange = 16f,
										yPeriodicLoopTime = 1200f,
										scale = 4f,
										layerDepth = 1f,
										animationLength = 12,
										interval = (float)Game1.random.Next(20, 40),
										totalNumberOfLoops = 1,
										delayBeforeAnimationStart = this._clankEffectTimer / num * j
									});
								}
							}
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
				else if (this.unforgeButton.containsPoint(x, y))
				{
					if (this.rightIngredientSpot.item == null)
					{
						if (this.IsValidUnforge(false))
						{
							MeleeWeapon leftWeapon = this.leftIngredientSpot.item as MeleeWeapon;
							if (leftWeapon != null && !Game1.player.couldInventoryAcceptThisItem("(O)848", leftWeapon.GetTotalForgeLevels(false) * 5 + (leftWeapon.GetTotalForgeLevels(false) - 1) * 2, 0))
							{
								this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
								Game1.playSound("cancel", null);
							}
							else if (this.leftIngredientSpot.item is CombinedRing && Game1.player.freeSpotsInInventory() < 2)
							{
								this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
								Game1.playSound("cancel", null);
							}
							else
							{
								this.unforging = true;
								this._timeUntilCraft = 1600;
								int crystals2 = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item) / 2;
								for (int k = 0; k < crystals2; k++)
								{
									Vector2 motion = new Vector2((float)Game1.random.Next(-4, 5), (float)Game1.random.Next(-4, 5));
									if (motion.X == 0f && motion.Y == 0f)
									{
										motion = new Vector2(-4f, -4f);
									}
									this.tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2((float)this.leftIngredientSpot.bounds.X, (float)this.leftIngredientSpot.bounds.Y), false, 0.1f, Color.White)
									{
										alpha = 0.01f,
										alphaFade = -0.1f,
										alphaFadeFade = -0.005f,
										texture = this.forgeTextures,
										motion = motion,
										scale = 4f,
										layerDepth = 1f,
										startSound = "boulderCrack",
										delayBeforeAnimationStart = 1100 / crystals2 * k
									});
								}
								Game1.playSound("debuffHit", null);
							}
						}
						else
						{
							this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
							Game1.playSound("cancel", null);
						}
					}
					else
					{
						if (this.IsValidUnforge(true))
						{
							this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_right_slot");
						}
						else
						{
							this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
						}
						Game1.playSound("cancel", null);
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

		// Token: 0x06002959 RID: 10585 RVA: 0x001E6E94 File Offset: 0x001E5094
		public virtual int GetForgeCostAtLevel(int level)
		{
			return 10 + level * 5;
		}

		// Token: 0x0600295A RID: 10586 RVA: 0x001E6E9C File Offset: 0x001E509C
		public virtual int GetForgeCost(Item left_item, Item right_item)
		{
			string a = (right_item != null) ? right_item.QualifiedItemId : null;
			if (a == "(O)896" || a == "(O)74")
			{
				return 20;
			}
			if (a == "(O)72")
			{
				return 10;
			}
			if (a == "(O)852")
			{
				return 10;
			}
			Tool leftTool = left_item as Tool;
			if (leftTool == null)
			{
				if (!(left_item is Ring))
				{
					return 1;
				}
				if (!(right_item is Ring))
				{
					return 1;
				}
				return 20;
			}
			else
			{
				if (!(leftTool is MeleeWeapon) || !(right_item is MeleeWeapon))
				{
					return this.GetForgeCostAtLevel(leftTool.GetTotalForgeLevels(false));
				}
				return 10;
			}
		}

		// Token: 0x0600295B RID: 10587 RVA: 0x001E6F38 File Offset: 0x001E5138
		protected void _ValidateCraft()
		{
			Item left_item = this.leftIngredientSpot.item;
			Item right_item = this.rightIngredientSpot.item;
			if (left_item == null || right_item == null)
			{
				this._craftState = ForgeMenu.CraftState.MissingIngredients;
			}
			else if (this.IsValidCraft(left_item, right_item))
			{
				this._craftState = ForgeMenu.CraftState.Valid;
				Item left_item_clone = left_item.getOne();
				if (((right_item != null) ? right_item.QualifiedItemId : null) == "(O)72")
				{
					(left_item_clone as Tool).AddEnchantment(new DiamondEnchantment());
					this.craftResultDisplay.item = left_item_clone;
				}
				else
				{
					this.craftResultDisplay.item = this.CraftItem(left_item_clone, right_item.getOne(), false);
				}
			}
			else
			{
				this._craftState = ForgeMenu.CraftState.InvalidRecipe;
			}
			this._UpdateDescriptionText();
		}

		// Token: 0x0600295C RID: 10588 RVA: 0x001E6FE4 File Offset: 0x001E51E4
		protected void _UpdateDescriptionText()
		{
			if (this.IsBusy())
			{
				Item item = this.rightIngredientSpot.item;
				this.displayedDescription = ((((item != null) ? item.QualifiedItemId : null) == "(O)74") ? Game1.content.LoadString("Strings\\UI:Forge_enchanting") : Game1.content.LoadString("Strings\\UI:Forge_forging"));
				return;
			}
			switch (this._craftState)
			{
			case ForgeMenu.CraftState.MissingIngredients:
				this.displayedDescription = (this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_description1") + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Forge_description2"));
				return;
			case ForgeMenu.CraftState.MissingShards:
			{
				Item heldItem = base.heldItem;
				this.displayedDescription = ((((heldItem != null) ? heldItem.QualifiedItemId : null) == "(O)848") ? Game1.content.LoadString("Strings\\UI:Forge_shards") : Game1.content.LoadString("Strings\\UI:Forge_notenoughshards"));
				return;
			}
			case ForgeMenu.CraftState.Valid:
				this.displayedDescription = ((!this.CanFitCraftedItem()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588") : Game1.content.LoadString("Strings\\UI:Forge_valid"));
				return;
			case ForgeMenu.CraftState.InvalidRecipe:
				this.displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_wrongorder");
				return;
			default:
				this.displayedDescription = "";
				return;
			}
		}

		// Token: 0x0600295D RID: 10589 RVA: 0x001E7138 File Offset: 0x001E5338
		public virtual bool IsValidCraft(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return false;
			}
			Tool leftTool = left_item as Tool;
			if (leftTool != null && leftTool.CanForge(right_item))
			{
				return true;
			}
			Ring leftRing = left_item as Ring;
			if (leftRing != null)
			{
				Ring rightRing = right_item as Ring;
				if (rightRing != null && leftRing.CanCombine(rightRing))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600295E RID: 10590 RVA: 0x001E7184 File Offset: 0x001E5384
		public virtual Item CraftItem(Item left_item, Item right_item, bool forReal = false)
		{
			if (left_item == null || right_item == null)
			{
				return null;
			}
			Tool leftTool = left_item as Tool;
			if (leftTool != null && !leftTool.Forge(right_item, forReal))
			{
				return null;
			}
			Ring leftRing = left_item as Ring;
			if (leftRing != null)
			{
				Ring rightRing = right_item as Ring;
				if (rightRing != null)
				{
					left_item = leftRing.Combine(rightRing);
				}
			}
			return left_item;
		}

		// Token: 0x0600295F RID: 10591 RVA: 0x001E71CD File Offset: 0x001E53CD
		public void SpendRightItem()
		{
			ClickableComponent clickableComponent = this.rightIngredientSpot;
			Item item = this.rightIngredientSpot.item;
			clickableComponent.item = ((item != null) ? item.ConsumeStack(1) : null);
		}

		// Token: 0x06002960 RID: 10592 RVA: 0x001E71F2 File Offset: 0x001E53F2
		public void SpendLeftItem()
		{
			ClickableComponent clickableComponent = this.leftIngredientSpot;
			Item item = this.leftIngredientSpot.item;
			clickableComponent.item = ((item != null) ? item.ConsumeStack(1) : null);
		}

		// Token: 0x06002961 RID: 10593 RVA: 0x001E7217 File Offset: 0x001E5417
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.IsBusy())
			{
				return;
			}
			base.receiveRightClick(x, y, true);
		}

		// Token: 0x06002962 RID: 10594 RVA: 0x001E722C File Offset: 0x001E542C
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
					if (!(name == "Ring1"))
					{
						if (name == "Ring2")
						{
							this.hoveredItem = Game1.player.rightRing.Value;
						}
					}
					else
					{
						this.hoveredItem = Game1.player.leftRing.Value;
					}
				}
			}
			if (this.craftResultDisplay.visible && this.craftResultDisplay.containsPoint(x, y) && this.craftResultDisplay.item != null)
			{
				this.hoveredItem = this.craftResultDisplay.item;
			}
			if (this.leftIngredientSpot.containsPoint(x, y) && this.leftIngredientSpot.item != null)
			{
				this.hoveredItem = this.leftIngredientSpot.item;
			}
			if (this.rightIngredientSpot.containsPoint(x, y) && this.rightIngredientSpot.item != null)
			{
				this.hoveredItem = this.rightIngredientSpot.item;
			}
			if (this.unforgeButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Forge_Unforge");
			}
			if (this._craftState == ForgeMenu.CraftState.Valid && this.CanFitCraftedItem())
			{
				this.startTailoringButton.tryHover(x, y, 0.33f);
				return;
			}
			this.startTailoringButton.tryHover(-999, -999, 0.1f);
		}

		// Token: 0x06002963 RID: 10595 RVA: 0x001E73DA File Offset: 0x001E55DA
		public bool CanFitCraftedItem()
		{
			return this.craftResultDisplay.item == null || Utility.canItemBeAddedToThisInventoryList(this.craftResultDisplay.item, this.inventory.actualInventory, -1);
		}

		// Token: 0x06002964 RID: 10596 RVA: 0x001E740C File Offset: 0x001E560C
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int yPositionForInventory = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, false, null, this.inventory.highlightMethod, -1, 3, 0, 0, true);
			this._CreateButtons();
		}

		// Token: 0x06002965 RID: 10597 RVA: 0x001E7480 File Offset: 0x001E5680
		public override void emergencyShutDown()
		{
			this._OnCloseMenu();
			base.emergencyShutDown();
		}

		// Token: 0x06002966 RID: 10598 RVA: 0x001E7490 File Offset: 0x001E5690
		public override void update(GameTime time)
		{
			base.update(time);
			this.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null && !Game1.player.Items.ContainsId("(O)848", this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item)))
			{
				if (this._craftState != ForgeMenu.CraftState.MissingShards)
				{
					this._craftState = ForgeMenu.CraftState.MissingShards;
					this.craftResultDisplay.item = null;
					this._UpdateDescriptionText();
				}
			}
			else if (this._craftState == ForgeMenu.CraftState.MissingShards)
			{
				this._ValidateCraft();
			}
			this.descriptionText = this.displayedDescription;
			this.questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
			this.questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
			bool can_fit_crafted_item = this.CanFitCraftedItem();
			if (this._craftState == ForgeMenu.CraftState.Valid && !this.IsBusy() && can_fit_crafted_item)
			{
				this.craftResultDisplay.visible = true;
			}
			else
			{
				this.craftResultDisplay.visible = false;
			}
			if (this._timeUntilCraft <= 0 && this._sparklingTimer <= 0)
			{
				return;
			}
			this.startTailoringButton.tryHover(this.startTailoringButton.bounds.Center.X, this.startTailoringButton.bounds.Center.Y, 0.33f);
			this._timeUntilCraft -= (int)time.ElapsedGameTime.TotalMilliseconds;
			this._clankEffectTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this._timeUntilCraft <= 0 && this._sparklingTimer > 0)
			{
				this._sparklingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			else if (this._clankEffectTimer <= 0 && !this.unforging)
			{
				this._clankEffectTimer = 450;
				Item item = this.rightIngredientSpot.item;
				if (((item != null) ? item.QualifiedItemId : null) == "(O)74")
				{
					Rectangle r = this.rightIngredientSpot.bounds;
					r.Inflate(-16, -16);
					int num = 30;
					for (int i = 0; i < num; i++)
					{
						Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
						this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position, false, 0f, Color.White)
						{
							motion = new Vector2(-4f, 0f),
							yPeriodic = true,
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 1200f,
							scale = 4f,
							layerDepth = 1f,
							animationLength = 12,
							interval = (float)Game1.random.Next(20, 40),
							totalNumberOfLoops = 1,
							delayBeforeAnimationStart = this._clankEffectTimer / num * i
						});
					}
				}
				else
				{
					Game1.playSound("crafting", null);
					Game1.playSound("clank", null);
					Rectangle r2 = this.leftIngredientSpot.bounds;
					r2.Inflate(-21, -21);
					Vector2 position2 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
					this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position2, false, 0.015f, Color.White)
					{
						motion = new Vector2(-1f, -10f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position2, false, 0.015f, Color.White)
					{
						motion = new Vector2(0f, -8f),
						acceleration = new Vector2(0f, 0.48f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position2, false, 0.015f, Color.White)
					{
						motion = new Vector2(1f, -10f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position2, false, 0.015f, Color.White)
					{
						motion = new Vector2(-2f, -8f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 2f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position2, false, 0.015f, Color.White)
					{
						motion = new Vector2(2f, -8f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 2f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
				}
			}
			if (this._timeUntilCraft > 0 || this._sparklingTimer > 0)
			{
				return;
			}
			if (this.unforging)
			{
				Item item2 = this.leftIngredientSpot.item;
				MeleeWeapon leftWeapon = item2 as MeleeWeapon;
				if (leftWeapon == null)
				{
					CombinedRing leftRing = item2 as CombinedRing;
					if (leftRing != null)
					{
						List<Ring> rings = new List<Ring>(leftRing.combinedRings);
						leftRing.combinedRings.Clear();
						foreach (Ring item3 in rings)
						{
							Utility.CollectOrDrop(item3);
						}
						this.leftIngredientSpot.item = null;
						Game1.playSound("coin", null);
						Utility.CollectOrDrop(ItemRegistry.Create("(O)848", 10, 0, false));
					}
				}
				else
				{
					int cost = 0;
					int weapon_forge_levels = leftWeapon.GetTotalForgeLevels(true);
					for (int j = 0; j < weapon_forge_levels; j++)
					{
						cost += this.GetForgeCostAtLevel(j);
					}
					if (leftWeapon.hasEnchantmentOfType<DiamondEnchantment>())
					{
						cost += this.GetForgeCost(this.leftIngredientSpot.item, ItemRegistry.Create("(O)72", 1, 0, false));
					}
					for (int k = leftWeapon.enchantments.Count - 1; k >= 0; k--)
					{
						if (leftWeapon.enchantments[k].IsForge())
						{
							leftWeapon.RemoveEnchantment(leftWeapon.enchantments[k]);
						}
					}
					if (leftWeapon.appearance.Value != null)
					{
						Utility.CollectOrDrop(ItemRegistry.Create(leftWeapon.appearance.Value, 1, 0, false));
						leftWeapon.appearance.Value = null;
						leftWeapon.ResetIndexOfMenuItemView();
						cost += 10;
					}
					this.leftIngredientSpot.item = null;
					Game1.playSound("coin", null);
					Utility.CollectOrDrop(base.heldItem);
					base.heldItem = leftWeapon;
					Utility.CollectOrDrop(ItemRegistry.Create("(O)848", cost / 2, 0, false));
				}
				this.unforging = false;
				this._timeUntilCraft = 0;
				this._ValidateCraft();
				return;
			}
			Game1.player.Items.ReduceId("(O)848", this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item));
			Item crafted_item = this.CraftItem(this.leftIngredientSpot.item, this.rightIngredientSpot.item, true);
			if (crafted_item != null && !Utility.canItemBeAddedToThisInventoryList(crafted_item, this.inventory.actualInventory, -1))
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
			this.SpendRightItem();
			Game1.playSound("coin", null);
			Utility.CollectOrDrop(base.heldItem);
			base.heldItem = crafted_item;
			this._timeUntilCraft = 0;
			this._ValidateCraft();
		}

		// Token: 0x06002967 RID: 10599 RVA: 0x001E7E14 File Offset: 0x001E6014
		public virtual bool IsValidUnforge(bool ignore_right_slot_occupancy = false)
		{
			if (!ignore_right_slot_occupancy && this.rightIngredientSpot.item != null)
			{
				return false;
			}
			MeleeWeapon leftWeapon = this.leftIngredientSpot.item as MeleeWeapon;
			return (leftWeapon != null && (leftWeapon.GetTotalForgeLevels(false) > 0 || leftWeapon.appearance.Value != null)) || this.leftIngredientSpot.item is CombinedRing;
		}

		// Token: 0x06002968 RID: 10600 RVA: 0x001E7E78 File Offset: 0x001E6078
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			Game1.DrawBox(this.xPositionOnScreen - 64, this.yPositionOnScreen + 128, 128, 201, new Color?(new Color(116, 11, 3)));
			Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(this.xPositionOnScreen - 64) + 9.6f, (float)(this.yPositionOnScreen + 128)), 0.87f, 4f, 2, Game1.player, 1f);
			base.draw(b, true, true, 116, 11, 3);
			b.Draw(this.forgeTextures, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder)), new Rectangle?(new Rectangle(0, 0, 142, 80)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Color draw_color = Color.White;
			if (this._craftState == ForgeMenu.CraftState.MissingShards)
			{
				draw_color = Color.Gray * 0.75f;
			}
			b.Draw(this.forgeTextures, new Vector2((float)(this.xPositionOnScreen + 276), (float)(this.yPositionOnScreen + 300)), new Rectangle?(new Rectangle(142, 16, 17, 17)), draw_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			if (this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null && this.IsValidCraft(this.leftIngredientSpot.item, this.rightIngredientSpot.item))
			{
				int source_offset = (this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item) - 10) / 5;
				if (source_offset >= 0 && source_offset <= 2)
				{
					b.Draw(this.forgeTextures, new Vector2((float)(this.xPositionOnScreen + 344), (float)(this.yPositionOnScreen + 320)), new Rectangle?(new Rectangle(142, 38 + source_offset * 10, 17, 10)), Color.White * ((this._craftState == ForgeMenu.CraftState.MissingShards) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
			}
			if (this.IsValidUnforge(false))
			{
				b.Draw(this.forgeTextures, new Vector2((float)this.unforgeButton.bounds.X, (float)this.unforgeButton.bounds.Y), new Rectangle?(new Rectangle(143, 69, 11, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			if (this._craftState == ForgeMenu.CraftState.Valid)
			{
				this.startTailoringButton.draw(b, Color.White, 0.96f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 200 % 12, 0, 0);
				this.startTailoringButton.drawItem(b, 16, 16, 1f);
			}
			Point random_shaking = new Point(0, 0);
			bool left_slot_accepts_this_item = false;
			bool right_slot_accepts_this_item = false;
			Item highlight_item = this.hoveredItem;
			if (base.heldItem != null)
			{
				highlight_item = base.heldItem;
			}
			if (highlight_item != null && highlight_item != this.leftIngredientSpot.item && highlight_item != this.rightIngredientSpot.item && highlight_item != this.craftResultDisplay.item)
			{
				if (highlight_item is Tool)
				{
					if (this.leftIngredientSpot.item is Tool)
					{
						right_slot_accepts_this_item = true;
					}
					else
					{
						left_slot_accepts_this_item = true;
					}
				}
				if (BaseEnchantment.GetEnchantmentFromItem(this.leftIngredientSpot.item, highlight_item) != null)
				{
					right_slot_accepts_this_item = true;
				}
				if (highlight_item is Ring && !(highlight_item is CombinedRing) && (this.leftIngredientSpot.item == null || this.leftIngredientSpot.item is Ring) && (this.rightIngredientSpot.item == null || this.rightIngredientSpot.item is Ring))
				{
					left_slot_accepts_this_item = true;
					right_slot_accepts_this_item = true;
				}
			}
			foreach (ClickableComponent c in this.equipmentIcons)
			{
				string name = c.name;
				if (!(name == "Ring1"))
				{
					if (name == "Ring2")
					{
						if (Game1.player.rightRing.Value != null)
						{
							b.Draw(this.forgeTextures, c.bounds, new Rectangle?(new Rectangle(0, 96, 16, 16)), Color.White);
							float transparency = 1f;
							if (!this.HighlightItems(Game1.player.rightRing.Value))
							{
								transparency = 0.5f;
							}
							if (Game1.player.rightRing.Value == base.heldItem)
							{
								transparency = 0.5f;
							}
							Game1.player.rightRing.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, transparency, 0.866f, StackDrawType.Hide);
						}
						else
						{
							b.Draw(this.forgeTextures, c.bounds, new Rectangle?(new Rectangle(16, 96, 16, 16)), Color.White);
						}
					}
				}
				else if (Game1.player.leftRing.Value != null)
				{
					b.Draw(this.forgeTextures, c.bounds, new Rectangle?(new Rectangle(0, 96, 16, 16)), Color.White);
					float transparency2 = 1f;
					if (!this.HighlightItems(Game1.player.leftRing.Value))
					{
						transparency2 = 0.5f;
					}
					if (Game1.player.leftRing.Value == base.heldItem)
					{
						transparency2 = 0.5f;
					}
					Game1.player.leftRing.Value.drawInMenu(b, new Vector2((float)c.bounds.X, (float)c.bounds.Y), c.scale, transparency2, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(this.forgeTextures, c.bounds, new Rectangle?(new Rectangle(16, 96, 16, 16)), Color.White);
				}
			}
			if (!this.IsBusy())
			{
				if (left_slot_accepts_this_item)
				{
					this.leftIngredientSpot.draw(b, Color.White, 0.87f, 0, 0, 0);
				}
			}
			else if (this._clankEffectTimer > 300 || (this._timeUntilCraft > 0 && this.unforging))
			{
				random_shaking.X = Game1.random.Next(-1, 2);
				random_shaking.Y = Game1.random.Next(-1, 2);
			}
			this.leftIngredientSpot.drawItem(b, random_shaking.X * 4, random_shaking.Y * 4, 1f);
			if (this.craftResultDisplay.visible)
			{
				string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
				Vector2 text_position = new Vector2((float)this.craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)this.craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y);
				Utility.drawTextWithColoredShadow(b, make_result_text, Game1.smallFont, text_position, Game1.textColor * 0.75f, Color.Black * 0.2f, 1f, -1f, -1, -1, 3);
				if (this.craftResultDisplay.item != null)
				{
					this.craftResultDisplay.drawItem(b, 0, 0, 1f);
				}
			}
			if (!this.IsBusy() && right_slot_accepts_this_item)
			{
				this.rightIngredientSpot.draw(b, Color.White, 0.87f, 0, 0, 0);
			}
			this.rightIngredientSpot.drawItem(b, 0, 0, 1f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, (base.heldItem != null) ? 32 : 0, (base.heldItem != null) ? 32 : 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			else if (this.hoveredItem != null)
			{
				if (this.hoveredItem == this.craftResultDisplay.item)
				{
					Item item = this.rightIngredientSpot.item;
					if (((item != null) ? item.QualifiedItemId : null) == "(O)74")
					{
						BaseEnchantment.hideEnchantmentName = true;
						goto IL_941;
					}
				}
				if (this.hoveredItem == this.craftResultDisplay.item)
				{
					Item item2 = this.rightIngredientSpot.item;
					if (((item2 != null) ? item2.QualifiedItemId : null) == "(O)852")
					{
						BaseEnchantment.hideSecondaryEnchantName = true;
					}
				}
				IL_941:
				IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, base.heldItem != null, -1, 0, null, -1, null, -1, null);
				BaseEnchantment.hideEnchantmentName = false;
				BaseEnchantment.hideSecondaryEnchantName = false;
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

		// Token: 0x06002969 RID: 10601 RVA: 0x001E887C File Offset: 0x001E6A7C
		protected override void cleanupBeforeExit()
		{
			this._OnCloseMenu();
		}

		// Token: 0x0600296A RID: 10602 RVA: 0x001E8884 File Offset: 0x001E6A84
		protected void _OnCloseMenu()
		{
			if (!Game1.player.IsEquippedItem(base.heldItem))
			{
				Utility.CollectOrDrop(base.heldItem, 2);
			}
			if (!Game1.player.IsEquippedItem(this.leftIngredientSpot.item))
			{
				Utility.CollectOrDrop(this.leftIngredientSpot.item, 2);
			}
			if (!Game1.player.IsEquippedItem(this.rightIngredientSpot.item))
			{
				Utility.CollectOrDrop(this.rightIngredientSpot.item, 2);
			}
			if (!Game1.player.IsEquippedItem(this.startTailoringButton.item))
			{
				Utility.CollectOrDrop(this.startTailoringButton.item, 2);
			}
			base.heldItem = null;
			this.leftIngredientSpot.item = null;
			this.rightIngredientSpot.item = null;
			this.startTailoringButton.item = null;
		}

		// Token: 0x04001AF6 RID: 6902
		protected int _timeUntilCraft;

		// Token: 0x04001AF7 RID: 6903
		protected int _clankEffectTimer;

		// Token: 0x04001AF8 RID: 6904
		protected int _sparklingTimer;

		// Token: 0x04001AF9 RID: 6905
		public const int region_leftIngredient = 998;

		// Token: 0x04001AFA RID: 6906
		public const int region_rightIngredient = 997;

		// Token: 0x04001AFB RID: 6907
		public const int region_startButton = 996;

		// Token: 0x04001AFC RID: 6908
		public const int region_resultItem = 995;

		// Token: 0x04001AFD RID: 6909
		public const int region_unforgeButton = 994;

		// Token: 0x04001AFE RID: 6910
		public ClickableTextureComponent craftResultDisplay;

		// Token: 0x04001AFF RID: 6911
		public ClickableTextureComponent leftIngredientSpot;

		// Token: 0x04001B00 RID: 6912
		public ClickableTextureComponent rightIngredientSpot;

		// Token: 0x04001B01 RID: 6913
		public ClickableTextureComponent startTailoringButton;

		// Token: 0x04001B02 RID: 6914
		public ClickableComponent unforgeButton;

		// Token: 0x04001B03 RID: 6915
		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		// Token: 0x04001B04 RID: 6916
		public const int region_ring_1 = 110;

		// Token: 0x04001B05 RID: 6917
		public const int region_ring_2 = 111;

		// Token: 0x04001B06 RID: 6918
		public const int CRAFT_TIME = 1600;

		// Token: 0x04001B07 RID: 6919
		public Texture2D forgeTextures;

		// Token: 0x04001B08 RID: 6920
		protected Dictionary<Item, bool> _highlightDictionary;

		// Token: 0x04001B09 RID: 6921
		protected TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04001B0A RID: 6922
		private bool unforging;

		// Token: 0x04001B0B RID: 6923
		protected string displayedDescription = "";

		// Token: 0x04001B0C RID: 6924
		protected ForgeMenu.CraftState _craftState;

		// Token: 0x04001B0D RID: 6925
		public Vector2 questionMarkOffset;

		// Token: 0x0200060A RID: 1546
		public enum CraftState
		{
			// Token: 0x04002E59 RID: 11865
			MissingIngredients,
			// Token: 0x04002E5A RID: 11866
			MissingShards,
			// Token: 0x04002E5B RID: 11867
			Valid,
			// Token: 0x04002E5C RID: 11868
			InvalidRecipe
		}
	}
}
