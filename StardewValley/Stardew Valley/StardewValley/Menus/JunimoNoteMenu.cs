using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x0200027F RID: 639
	public class JunimoNoteMenu : IClickableMenu
	{
		// Token: 0x06002A34 RID: 10804 RVA: 0x001F8320 File Offset: 0x001F6520
		public JunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
		{
			CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
			if (fromGameMenu && !fromThisMenu)
			{
				for (int i = 0; i < cc.areasComplete.Count; i++)
				{
					if (cc.shouldNoteAppearInArea(i) && !cc.areasComplete[i])
					{
						area = i;
						this.whichArea = area;
						break;
					}
				}
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					area = 6;
				}
			}
			this.setUpMenu(area, cc.bundlesDict());
			Game1.player.forceCanMove();
			this.areaNextButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 128, this.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				visible = false,
				myID = 101,
				leftNeighborID = 102,
				leftNeighborImmutable = true,
				downNeighborID = -99998
			};
			this.areaBackButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				visible = false,
				myID = 102,
				rightNeighborID = 101,
				rightNeighborImmutable = true,
				downNeighborID = -99998
			};
			int area_count = 6;
			for (int j = 0; j < area_count; j++)
			{
				if (j != area && cc.shouldNoteAppearInArea(j))
				{
					this.areaNextButton.visible = true;
					this.areaBackButton.visible = true;
					break;
				}
			}
			this.fromGameMenu = fromGameMenu;
			this.fromThisMenu = fromThisMenu;
			foreach (Bundle bundle in this.bundles)
			{
				bundle.depositsAllowed = false;
			}
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A35 RID: 10805 RVA: 0x001F859C File Offset: 0x001F679C
		public JunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
		{
			this.setUpMenu(whichArea, bundlesComplete);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A36 RID: 10806 RVA: 0x001F8638 File Offset: 0x001F6838
		public JunimoNoteMenu(Bundle b, string noteTexturePath) : base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, true)
		{
			this.singleBundleMenu = true;
			this.whichArea = -1;
			this.noteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
			JunimoNoteMenu.tempSprites.Clear();
			InventoryMenu inventoryMenu;
			(inventoryMenu = new InventoryMenu(this.xPositionOnScreen + 128, this.yPositionOnScreen + 140, true, null, new InventoryMenu.highlightThisItem(this.HighlightObjects), 36, 6, 8, 8, false)).capacity = 36;
			this.inventory = inventoryMenu;
			for (int i = 0; i < this.inventory.inventory.Count; i++)
			{
				if (i >= this.inventory.actualInventory.Count)
				{
					this.inventory.inventory[i].visible = false;
				}
			}
			foreach (ClickableComponent clickableComponent in this.inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
			{
				clickableComponent.downNeighborID = -99998;
			}
			foreach (ClickableComponent clickableComponent2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				clickableComponent2.rightNeighborID = -99998;
			}
			this.inventory.dropItemInvisibleButton.visible = false;
			JunimoNoteMenu.canClick = true;
			this.setUpBundleSpecificPage(b);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x001F8830 File Offset: 0x001F6A30
		public override void snapToDefaultClickableComponent()
		{
			if (this.specificBundlePage)
			{
				this.currentlySnappedComponent = base.getComponentWithID(0);
			}
			else
			{
				this.currentlySnappedComponent = base.getComponentWithID(5000);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002A38 RID: 10808 RVA: 0x001F8860 File Offset: 0x001F6A60
		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return !this.specificBundlePage;
		}

		// Token: 0x06002A39 RID: 10809 RVA: 0x001F886C File Offset: 0x001F6A6C
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
			{
				return;
			}
			if (oldID - 5000 >= 0 && oldID - 5000 < 10 && this.currentlySnappedComponent != null)
			{
				int lowestScoreBundle = -1;
				int lowestScore = 999999;
				Point startingPosition = this.currentlySnappedComponent.bounds.Center;
				for (int i = 0; i < this.bundles.Count; i++)
				{
					if (this.bundles[i].myID != oldID)
					{
						int score = 999999;
						Point bundlePosition = this.bundles[i].bounds.Center;
						switch (direction)
						{
						case 0:
							if (bundlePosition.Y < startingPosition.Y)
							{
								score = startingPosition.Y - bundlePosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
							}
							break;
						case 1:
							if (bundlePosition.X > startingPosition.X)
							{
								score = bundlePosition.X - startingPosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
							}
							break;
						case 2:
							if (bundlePosition.Y > startingPosition.Y)
							{
								score = bundlePosition.Y - startingPosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
							}
							break;
						case 3:
							if (bundlePosition.X < startingPosition.X)
							{
								score = startingPosition.X - bundlePosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
							}
							break;
						}
						if (score < 10000 && score < lowestScore)
						{
							lowestScore = score;
							lowestScoreBundle = i;
						}
					}
				}
				if (lowestScoreBundle != -1)
				{
					this.currentlySnappedComponent = base.getComponentWithID(lowestScoreBundle + 5000);
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
				switch (direction)
				{
				case 1:
					if (this.areaNextButton != null && this.areaNextButton.visible)
					{
						this.currentlySnappedComponent = this.areaNextButton;
						this.snapCursorToCurrentSnappedComponent();
						this.areaNextButton.leftNeighborID = oldID;
					}
					break;
				case 2:
					if (this.presentButton != null)
					{
						this.currentlySnappedComponent = this.presentButton;
						this.snapCursorToCurrentSnappedComponent();
						this.presentButton.upNeighborID = oldID;
						return;
					}
					break;
				case 3:
					if (this.areaBackButton != null && this.areaBackButton.visible)
					{
						this.currentlySnappedComponent = this.areaBackButton;
						this.snapCursorToCurrentSnappedComponent();
						this.areaBackButton.rightNeighborID = oldID;
						return;
					}
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06002A3A RID: 10810 RVA: 0x001F8AF4 File Offset: 0x001F6CF4
		public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		{
			this.noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
			if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
			{
				Game1.player.removeQuest("26");
				Game1.player.mailReceived.Add("seenJunimoNote");
			}
			if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
			{
				Game1.addMailForTomorrow("wizardJunimoNote", false, false);
			}
			if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
			{
				Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
			}
			this.scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
			JunimoNoteMenu.tempSprites.Clear();
			this.whichArea = whichArea;
			InventoryMenu inventoryMenu;
			(inventoryMenu = new InventoryMenu(this.xPositionOnScreen + 128, this.yPositionOnScreen + 140, true, null, new InventoryMenu.highlightThisItem(this.HighlightObjects), 36, 6, 8, 8, false)).capacity = 36;
			this.inventory = inventoryMenu;
			for (int i = 0; i < this.inventory.inventory.Count; i++)
			{
				if (i >= this.inventory.actualInventory.Count)
				{
					this.inventory.inventory[i].visible = false;
				}
			}
			foreach (ClickableComponent clickableComponent in this.inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
			{
				clickableComponent.downNeighborID = -99998;
			}
			foreach (ClickableComponent clickableComponent2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				clickableComponent2.rightNeighborID = -99998;
			}
			this.inventory.dropItemInvisibleButton.visible = false;
			Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
			string areaName = CommunityCenter.getAreaNameFromNumber(whichArea);
			int bundlesAdded = 0;
			foreach (string j in bundlesInfo.Keys)
			{
				if (j.Contains(areaName))
				{
					int bundleIndex = Convert.ToInt32(j.Split('/', StringSplitOptions.None)[1]);
					this.bundles.Add(new Bundle(bundleIndex, bundlesInfo[j], bundlesComplete[bundleIndex], this.getBundleLocationFromNumber(bundlesAdded), "LooseSprites\\JunimoNote", this)
					{
						myID = bundlesAdded + 5000,
						rightNeighborID = -7777,
						leftNeighborID = -7777,
						upNeighborID = -7777,
						downNeighborID = -7777,
						fullyImmutable = true
					});
					bundlesAdded++;
				}
			}
			this.backButton = new ClickableTextureComponent("Back", new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
			{
				myID = 103
			};
			this.checkForRewards();
			JunimoNoteMenu.canClick = true;
			Game1.playSound("shwip", null);
			bool isOneIncomplete = false;
			foreach (Bundle b in this.bundles)
			{
				if (!b.complete && !b.Equals(this.currentPageBundle))
				{
					isOneIncomplete = true;
					break;
				}
			}
			if (!isOneIncomplete)
			{
				CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
				communityCenter.markAreaAsComplete(whichArea);
				this.exitFunction = new IClickableMenu.onExit(this.restoreAreaOnExit);
				communityCenter.areaCompleteReward(whichArea);
			}
		}

		// Token: 0x06002A3B RID: 10811 RVA: 0x001F8EE8 File Offset: 0x001F70E8
		public virtual bool HighlightObjects(Item item)
		{
			if (this.currentPageBundle != null)
			{
				if (this.partialDonationItem != null && this.currentPartialIngredientDescriptionIndex >= 0)
				{
					return this.currentPageBundle.IsValidItemForThisIngredientDescription(item, this.currentPageBundle.ingredients[this.currentPartialIngredientDescriptionIndex]);
				}
				foreach (BundleIngredientDescription ingredient in this.currentPageBundle.ingredients)
				{
					if (this.currentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06002A3C RID: 10812 RVA: 0x001F8F8C File Offset: 0x001F718C
		public override bool readyToClose()
		{
			return (!this.specificBundlePage || this.singleBundleMenu) && this.isReadyToCloseMenuOrBundle();
		}

		// Token: 0x06002A3D RID: 10813 RVA: 0x001F8FA8 File Offset: 0x001F71A8
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!JunimoNoteMenu.canClick)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (this.scrambledText)
			{
				return;
			}
			if (this.specificBundlePage)
			{
				if (!this.currentPageBundle.complete && this.currentPageBundle.completionTimer <= 0)
				{
					this.heldItem = this.inventory.leftClick(x, y, this.heldItem, true);
				}
				if (this.backButton != null && this.backButton.containsPoint(x, y) && this.heldItem == null)
				{
					this.closeBundlePage();
				}
				if (this.partialDonationItem != null)
				{
					if (this.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						for (int i = 0; i < this.ingredientSlots.Count; i++)
						{
							if (this.ingredientSlots[i].item == this.partialDonationItem)
							{
								this.HandlePartialDonation(this.heldItem, this.ingredientSlots[i]);
							}
						}
					}
					else
					{
						int j = 0;
						while (j < this.ingredientSlots.Count)
						{
							if (this.ingredientSlots[j].containsPoint(x, y) && this.ingredientSlots[j].item == this.partialDonationItem)
							{
								if (this.heldItem != null)
								{
									this.HandlePartialDonation(this.heldItem, this.ingredientSlots[j]);
									return;
								}
								bool return_to_inventory = Game1.oldKBState.IsKeyDown(Keys.LeftShift);
								this.ReturnPartialDonations(!return_to_inventory);
								return;
							}
							else
							{
								j++;
							}
						}
					}
				}
				else if (this.heldItem != null)
				{
					if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						for (int k = 0; k < this.ingredientSlots.Count; k++)
						{
							if (this.currentPageBundle.canAcceptThisItem(this.heldItem, this.ingredientSlots[k]))
							{
								if (this.ingredientSlots[k].item == null)
								{
									this.heldItem = this.currentPageBundle.tryToDepositThisItem(this.heldItem, this.ingredientSlots[k], "LooseSprites\\JunimoNote", this);
									this.checkIfBundleIsComplete();
									return;
								}
							}
							else if (this.ingredientSlots[k].item == null)
							{
								this.HandlePartialDonation(this.heldItem, this.ingredientSlots[k]);
							}
						}
					}
					for (int l = 0; l < this.ingredientSlots.Count; l++)
					{
						if (this.ingredientSlots[l].containsPoint(x, y))
						{
							if (this.currentPageBundle.canAcceptThisItem(this.heldItem, this.ingredientSlots[l]))
							{
								this.heldItem = this.currentPageBundle.tryToDepositThisItem(this.heldItem, this.ingredientSlots[l], "LooseSprites\\JunimoNote", this);
								this.checkIfBundleIsComplete();
							}
							else if (this.ingredientSlots[l].item == null)
							{
								this.HandlePartialDonation(this.heldItem, this.ingredientSlots[l]);
							}
						}
					}
				}
				if (this.purchaseButton != null && this.purchaseButton.containsPoint(x, y))
				{
					int moneyRequired = this.currentPageBundle.ingredients.Last<BundleIngredientDescription>().stack;
					if (Game1.player.Money >= moneyRequired)
					{
						Game1.player.Money -= moneyRequired;
						Game1.playSound("select", null);
						this.currentPageBundle.completionAnimation(this, true, 0);
						if (this.purchaseButton != null)
						{
							this.purchaseButton.scale = this.purchaseButton.baseScale * 0.75f;
						}
						CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
						communityCenter.bundleRewards[this.currentPageBundle.bundleIndex] = true;
						communityCenter.bundles.FieldDict[this.currentPageBundle.bundleIndex][0] = true;
						this.checkForRewards();
						bool isOneIncomplete = false;
						foreach (Bundle b in this.bundles)
						{
							if (!b.complete && !b.Equals(this.currentPageBundle))
							{
								isOneIncomplete = true;
								break;
							}
						}
						if (!isOneIncomplete)
						{
							communityCenter.markAreaAsComplete(this.whichArea);
							this.exitFunction = new IClickableMenu.onExit(this.restoreAreaOnExit);
							communityCenter.areaCompleteReward(this.whichArea);
						}
						else
						{
							Junimo junimoForArea = communityCenter.getJunimoForArea(this.whichArea);
							if (junimoForArea != null)
							{
								junimoForArea.bringBundleBackToHut(Bundle.getColorFromColorIndex(this.currentPageBundle.bundleColor), Game1.RequireLocation("CommunityCenter", false));
							}
						}
						Game1.multiplayer.globalChatInfoMessage("Bundle", Array.Empty<string>());
					}
					else
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
					}
				}
				if (this.upperRightCloseButton != null && this.isReadyToCloseMenuOrBundle() && this.upperRightCloseButton.containsPoint(x, y))
				{
					this.closeBundlePage();
					return;
				}
			}
			else
			{
				foreach (Bundle b2 in this.bundles)
				{
					if (b2.canBeClicked() && b2.containsPoint(x, y))
					{
						this.setUpBundleSpecificPage(b2);
						Game1.playSound("shwip", null);
						return;
					}
				}
				if (this.presentButton != null && this.presentButton.containsPoint(x, y) && !this.fromGameMenu && !this.fromThisMenu)
				{
					this.openRewardsMenu();
				}
				if (this.fromGameMenu)
				{
					if (this.areaNextButton.containsPoint(x, y))
					{
						this.SwapPage(1);
					}
					else if (this.areaBackButton.containsPoint(x, y))
					{
						this.SwapPage(-1);
					}
				}
			}
			if (this.heldItem != null && !this.isWithinBounds(x, y) && this.heldItem.canBeTrashed())
			{
				Game1.playSound("throwDownITem", null);
				Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false);
				this.heldItem = null;
			}
		}

		// Token: 0x06002A3E RID: 10814 RVA: 0x001F95E4 File Offset: 0x001F77E4
		public virtual void ReturnPartialDonation(Item item, bool play_sound = true)
		{
			List<Item> affected_items = new List<Item>();
			Item remainder = Game1.player.addItemToInventory(item, affected_items);
			foreach (Item affected_item in affected_items)
			{
				this.inventory.ShakeItem(affected_item);
			}
			if (remainder != null)
			{
				Utility.CollectOrDrop(remainder);
				this.inventory.ShakeItem(remainder);
			}
			if (play_sound)
			{
				Game1.playSound("coin", null);
			}
		}

		// Token: 0x06002A3F RID: 10815 RVA: 0x001F9678 File Offset: 0x001F7878
		public virtual void ReturnPartialDonations(bool to_hand = true)
		{
			if (this.partialDonationComponents.Count > 0)
			{
				bool play_sound = true;
				foreach (Item item in this.partialDonationComponents)
				{
					if (this.heldItem == null && to_hand)
					{
						Game1.playSound("dwop", null);
						this.heldItem = item;
					}
					else
					{
						this.ReturnPartialDonation(item, play_sound);
						play_sound = false;
					}
				}
			}
			this.ResetPartialDonation();
		}

		// Token: 0x06002A40 RID: 10816 RVA: 0x001F9710 File Offset: 0x001F7910
		public virtual void ResetPartialDonation()
		{
			this.partialDonationComponents.Clear();
			this.currentPartialIngredientDescription = null;
			this.currentPartialIngredientDescriptionIndex = -1;
			foreach (ClickableTextureComponent slot in this.ingredientSlots)
			{
				if (slot.item == this.partialDonationItem)
				{
					slot.item = null;
				}
			}
			this.partialDonationItem = null;
		}

		// Token: 0x06002A41 RID: 10817 RVA: 0x001F9798 File Offset: 0x001F7998
		public virtual bool CanBePartiallyOrFullyDonated(Item item)
		{
			if (this.currentPageBundle == null)
			{
				return false;
			}
			int index = this.currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
			if (index < 0)
			{
				return false;
			}
			BundleIngredientDescription description = this.currentPageBundle.ingredients[index];
			int count = 0;
			if (this.currentPageBundle.IsValidItemForThisIngredientDescription(item, description))
			{
				count += item.Stack;
			}
			foreach (Item inventory_item in Game1.player.Items)
			{
				if (this.currentPageBundle.IsValidItemForThisIngredientDescription(inventory_item, description))
				{
					count += inventory_item.Stack;
				}
			}
			if (index == this.currentPartialIngredientDescriptionIndex && this.partialDonationItem != null)
			{
				count += this.partialDonationItem.Stack;
			}
			return count >= description.stack;
		}

		// Token: 0x06002A42 RID: 10818 RVA: 0x001F9874 File Offset: 0x001F7A74
		public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
		{
			if (this.currentPageBundle != null && !this.currentPageBundle.depositsAllowed)
			{
				return;
			}
			if (this.partialDonationItem != null && slot.item != this.partialDonationItem)
			{
				return;
			}
			if (!this.CanBePartiallyOrFullyDonated(item))
			{
				return;
			}
			if (this.currentPartialIngredientDescription == null)
			{
				this.currentPartialIngredientDescriptionIndex = this.currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
				if (this.currentPartialIngredientDescriptionIndex != -1)
				{
					this.currentPartialIngredientDescription = new BundleIngredientDescription?(this.currentPageBundle.ingredients[this.currentPartialIngredientDescriptionIndex]);
				}
			}
			if (this.currentPartialIngredientDescription != null && this.currentPageBundle.IsValidItemForThisIngredientDescription(item, this.currentPartialIngredientDescription.Value))
			{
				bool playSound = true;
				bool isHeldItem = item == this.heldItem;
				int amountToDonate;
				if (slot.item == null)
				{
					Game1.playSound("sell", null);
					playSound = false;
					this.partialDonationItem = item.getOne();
					amountToDonate = Math.Min(this.currentPartialIngredientDescription.Value.stack, item.Stack);
					this.partialDonationItem.Stack = amountToDonate;
					item = item.ConsumeStack(amountToDonate);
					this.partialDonationItem.Quality = this.currentPartialIngredientDescription.Value.quality;
					slot.item = this.partialDonationItem;
					slot.sourceRect.X = 512;
					slot.sourceRect.Y = 244;
				}
				else
				{
					amountToDonate = Math.Min(this.currentPartialIngredientDescription.Value.stack - this.partialDonationItem.Stack, item.Stack);
					this.partialDonationItem.Stack += amountToDonate;
					item = item.ConsumeStack(amountToDonate);
				}
				if (amountToDonate > 0)
				{
					Item donatedItem = this.heldItem.getOne();
					donatedItem.Stack = amountToDonate;
					foreach (Item contributed_item in this.partialDonationComponents)
					{
						if (contributed_item.canStackWith(this.heldItem))
						{
							donatedItem.Stack = contributed_item.addToStack(donatedItem);
						}
					}
					if (donatedItem.Stack > 0)
					{
						this.partialDonationComponents.Add(donatedItem);
					}
					this.partialDonationComponents.Sort((Item a, Item b) => b.Stack.CompareTo(a.Stack));
				}
				if (isHeldItem && item == null)
				{
					this.heldItem = null;
				}
				if (this.partialDonationItem.Stack >= this.currentPartialIngredientDescription.Value.stack)
				{
					slot.item = null;
					this.partialDonationItem = this.currentPageBundle.tryToDepositThisItem(this.partialDonationItem, slot, "LooseSprites\\JunimoNote", this);
					Item item2 = this.partialDonationItem;
					if (item2 != null && item2.Stack > 0)
					{
						this.ReturnPartialDonation(this.partialDonationItem, true);
					}
					this.partialDonationItem = null;
					this.ResetPartialDonation();
					this.checkIfBundleIsComplete();
					return;
				}
				if (amountToDonate > 0 && playSound)
				{
					Game1.playSound("sell", null);
				}
			}
		}

		// Token: 0x06002A43 RID: 10819 RVA: 0x001F9B84 File Offset: 0x001F7D84
		public bool isReadyToCloseMenuOrBundle()
		{
			if (this.specificBundlePage)
			{
				Bundle bundle = this.currentPageBundle;
				if (bundle != null && bundle.completionTimer > 0)
				{
					return false;
				}
			}
			return this.heldItem == null;
		}

		// Token: 0x06002A44 RID: 10820 RVA: 0x001F9BB4 File Offset: 0x001F7DB4
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (this.specificBundlePage)
			{
				if (button != Buttons.RightTrigger)
				{
					if (button != Buttons.LeftTrigger)
					{
						return;
					}
					ClickableComponent currentlySnappedComponent = this.currentlySnappedComponent;
					if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= 250)
					{
						this.setCurrentlySnappedComponentTo(this.oldTriggerSpot);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
				else
				{
					ClickableComponent currentlySnappedComponent2 = this.currentlySnappedComponent;
					if (currentlySnappedComponent2 != null && currentlySnappedComponent2.myID < 50)
					{
						this.oldTriggerSpot = this.currentlySnappedComponent.myID;
						int id = 250;
						foreach (ClickableTextureComponent c in this.ingredientSlots)
						{
							if (c.item == null)
							{
								id = c.myID;
								break;
							}
						}
						this.setCurrentlySnappedComponentTo(id);
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
			}
			else if (this.fromGameMenu)
			{
				if (button == Buttons.RightTrigger)
				{
					this.SwapPage(1);
					return;
				}
				if (button != Buttons.LeftTrigger)
				{
					return;
				}
				this.SwapPage(-1);
			}
		}

		// Token: 0x06002A45 RID: 10821 RVA: 0x001F9CD0 File Offset: 0x001F7ED0
		public void SwapPage(int direction)
		{
			if (direction > 0 && !this.areaNextButton.visible)
			{
				return;
			}
			if (direction < 0 && !this.areaBackButton.visible)
			{
				return;
			}
			CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
			int area = this.whichArea;
			int area_count = 6;
			for (int i = 0; i < area_count; i++)
			{
				area += direction;
				if (area < 0)
				{
					area += area_count;
				}
				if (area >= area_count)
				{
					area -= area_count;
				}
				if (cc.shouldNoteAppearInArea(area))
				{
					int selected_id = -1;
					if (this.currentlySnappedComponent != null && (this.currentlySnappedComponent.myID >= 5000 || this.currentlySnappedComponent.myID == 101 || this.currentlySnappedComponent.myID == 102))
					{
						selected_id = this.currentlySnappedComponent.myID;
					}
					JunimoNoteMenu new_menu = new JunimoNoteMenu(true, area, true)
					{
						gameMenuTabToReturnTo = this.gameMenuTabToReturnTo
					};
					Game1.activeClickableMenu = new_menu;
					if (selected_id >= 0)
					{
						new_menu.currentlySnappedComponent = new_menu.getComponentWithID(this.currentlySnappedComponent.myID);
						new_menu.snapCursorToCurrentSnappedComponent();
					}
					if (new_menu.getComponentWithID(this.areaNextButton.leftNeighborID) != null)
					{
						new_menu.areaNextButton.leftNeighborID = this.areaNextButton.leftNeighborID;
					}
					else
					{
						new_menu.areaNextButton.leftNeighborID = new_menu.areaBackButton.myID;
					}
					new_menu.areaNextButton.rightNeighborID = this.areaNextButton.rightNeighborID;
					new_menu.areaNextButton.upNeighborID = this.areaNextButton.upNeighborID;
					new_menu.areaNextButton.downNeighborID = this.areaNextButton.downNeighborID;
					if (new_menu.getComponentWithID(this.areaBackButton.rightNeighborID) != null)
					{
						new_menu.areaBackButton.leftNeighborID = this.areaBackButton.leftNeighborID;
					}
					else
					{
						new_menu.areaBackButton.leftNeighborID = new_menu.areaNextButton.myID;
					}
					new_menu.areaBackButton.rightNeighborID = this.areaBackButton.rightNeighborID;
					new_menu.areaBackButton.upNeighborID = this.areaBackButton.upNeighborID;
					new_menu.areaBackButton.downNeighborID = this.areaBackButton.downNeighborID;
					return;
				}
			}
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x001F9EE8 File Offset: 0x001F80E8
		public override void receiveKeyPress(Keys key)
		{
			if (this.gameMenuTabToReturnTo != -1)
			{
				this.closeSound = "shwip";
			}
			base.receiveKeyPress(key);
			if (key == Keys.Delete && this.heldItem != null && this.heldItem.canBeTrashed())
			{
				Utility.trashItem(this.heldItem);
				this.heldItem = null;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.isReadyToCloseMenuOrBundle())
			{
				if (this.singleBundleMenu)
				{
					base.exitThisMenu(this.gameMenuTabToReturnTo == -1);
				}
				this.closeBundlePage();
			}
		}

		// Token: 0x06002A47 RID: 10823 RVA: 0x001F9F78 File Offset: 0x001F8178
		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			if (this.gameMenuTabToReturnTo != -1)
			{
				Game1.activeClickableMenu = new GameMenu(this.gameMenuTabToReturnTo, -1, false);
				return;
			}
			if (this.menuToReturnTo != null)
			{
				Game1.activeClickableMenu = this.menuToReturnTo;
			}
		}

		// Token: 0x06002A48 RID: 10824 RVA: 0x001F9FB0 File Offset: 0x001F81B0
		private void closeBundlePage()
		{
			if (this.partialDonationItem != null)
			{
				this.ReturnPartialDonations(false);
				return;
			}
			if (this.specificBundlePage)
			{
				this.hoveredItem = null;
				this.inventory.descriptionText = "";
				if (this.heldItem == null)
				{
					this.takeDownBundleSpecificPage();
					Game1.playSound("shwip", null);
					return;
				}
				this.heldItem = this.inventory.tryToAddItem(this.heldItem, "coin");
			}
		}

		// Token: 0x06002A49 RID: 10825 RVA: 0x001FA02C File Offset: 0x001F822C
		private void reOpenThisMenu()
		{
			bool flag = this.specificBundlePage;
			JunimoNoteMenu newMenu;
			if (this.fromGameMenu || this.fromThisMenu)
			{
				newMenu = new JunimoNoteMenu(this.fromGameMenu, this.whichArea, this.fromThisMenu)
				{
					gameMenuTabToReturnTo = this.gameMenuTabToReturnTo,
					menuToReturnTo = this.menuToReturnTo
				};
			}
			else
			{
				newMenu = new JunimoNoteMenu(this.whichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundlesDict())
				{
					gameMenuTabToReturnTo = this.gameMenuTabToReturnTo,
					menuToReturnTo = this.menuToReturnTo
				};
			}
			if (flag)
			{
				foreach (Bundle bundle in newMenu.bundles)
				{
					if (bundle.bundleIndex == this.currentPageBundle.bundleIndex)
					{
						newMenu.setUpBundleSpecificPage(bundle);
						break;
					}
				}
			}
			Game1.activeClickableMenu = newMenu;
		}

		// Token: 0x06002A4A RID: 10826 RVA: 0x001FA118 File Offset: 0x001F8318
		private void updateIngredientSlots()
		{
			int slotNumber = 0;
			foreach (BundleIngredientDescription ingredient in this.currentPageBundle.ingredients)
			{
				if (ingredient.completed && slotNumber < this.ingredientSlots.Count)
				{
					string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
					if (ingredient.preservesId != null)
					{
						this.ingredientSlots[slotNumber].item = Utility.CreateFlavoredItem(id, ingredient.preservesId, ingredient.quality, ingredient.stack);
					}
					else
					{
						this.ingredientSlots[slotNumber].item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality, false);
					}
					this.currentPageBundle.ingredientDepositAnimation(this.ingredientSlots[slotNumber], "LooseSprites\\JunimoNote", true);
					slotNumber++;
				}
			}
		}

		// Token: 0x06002A4B RID: 10827 RVA: 0x001FA20C File Offset: 0x001F840C
		public static string GetRepresentativeItemId(BundleIngredientDescription ingredient)
		{
			if (ingredient.category != null)
			{
				foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
				{
					int category = data.Category;
					int? category2 = ingredient.category;
					if (category == category2.GetValueOrDefault() & category2 != null)
					{
						return data.QualifiedItemId;
					}
				}
				return "0";
			}
			return ingredient.id;
		}

		// Token: 0x06002A4C RID: 10828 RVA: 0x001FA29C File Offset: 0x001F849C
		public static void GetBundleRewards(int area, List<Item> rewards)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
			Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
			foreach (string i in bundlesInfo.Keys)
			{
				if (i.Contains(CommunityCenter.getAreaNameFromNumber(area)))
				{
					int bundleIndex = Convert.ToInt32(i.Split('/', StringSplitOptions.None)[1]);
					if (communityCenter.bundleRewards[bundleIndex])
					{
						Item j = Utility.getItemFromStandardTextDescription(bundlesInfo[i].Split('/', StringSplitOptions.None)[1], Game1.player, ' ');
						j.SpecialVariable = bundleIndex;
						rewards.Add(j);
					}
				}
			}
		}

		// Token: 0x06002A4D RID: 10829 RVA: 0x001FA364 File Offset: 0x001F8564
		private void openRewardsMenu()
		{
			Game1.playSound("smallSelect", null);
			List<Item> rewards = new List<Item>();
			JunimoNoteMenu.GetBundleRewards(this.whichArea, rewards);
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, null, new ItemGrabMenu.behaviorOnItemSelect(this.rewardGrabbed), false, true, true, true, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
			Game1.activeClickableMenu.exitFunction = ((this.exitFunction != null) ? this.exitFunction : new IClickableMenu.onExit(this.reOpenThisMenu));
		}

		// Token: 0x06002A4E RID: 10830 RVA: 0x001FA3E4 File Offset: 0x001F85E4
		private void rewardGrabbed(Item item, Farmer who)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundleRewards[item.SpecialVariable] = false;
		}

		// Token: 0x06002A4F RID: 10831 RVA: 0x001FA404 File Offset: 0x001F8604
		private void checkIfBundleIsComplete()
		{
			this.ReturnPartialDonations(true);
			if (!this.specificBundlePage || this.currentPageBundle == null)
			{
				return;
			}
			int numberOfFilledSlots = 0;
			foreach (ClickableTextureComponent c in this.ingredientSlots)
			{
				if (c.item != null && c.item != this.partialDonationItem)
				{
					numberOfFilledSlots++;
				}
			}
			if (numberOfFilledSlots >= this.currentPageBundle.numberOfIngredientSlots)
			{
				if (this.heldItem != null)
				{
					Game1.player.addItemToInventory(this.heldItem);
					this.heldItem = null;
				}
				if (!this.singleBundleMenu)
				{
					CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false);
					for (int i = 0; i < communityCenter.bundles[this.currentPageBundle.bundleIndex].Length; i++)
					{
						communityCenter.bundles.FieldDict[this.currentPageBundle.bundleIndex][i] = true;
					}
					communityCenter.checkForNewJunimoNotes();
					JunimoNoteMenu.screenSwipe = new ScreenSwipe(0, -1f, -1, this.width, this.height);
					this.currentPageBundle.completionAnimation(this, true, 400);
					JunimoNoteMenu.canClick = false;
					communityCenter.bundleRewards[this.currentPageBundle.bundleIndex] = true;
					Game1.multiplayer.globalChatInfoMessage("Bundle", Array.Empty<string>());
					bool isOneIncomplete = false;
					foreach (Bundle b in this.bundles)
					{
						if (!b.complete && !b.Equals(this.currentPageBundle))
						{
							isOneIncomplete = true;
							break;
						}
					}
					if (!isOneIncomplete)
					{
						if (this.whichArea == 6)
						{
							this.exitFunction = new IClickableMenu.onExit(this.restoreaAreaOnExit_AbandonedJojaMart);
						}
						else
						{
							communityCenter.markAreaAsComplete(this.whichArea);
							this.exitFunction = new IClickableMenu.onExit(this.restoreAreaOnExit);
							communityCenter.areaCompleteReward(this.whichArea);
						}
					}
					else
					{
						Junimo junimoForArea = communityCenter.getJunimoForArea(this.whichArea);
						if (junimoForArea != null)
						{
							junimoForArea.bringBundleBackToHut(Bundle.getColorFromColorIndex(this.currentPageBundle.bundleColor), communityCenter);
						}
					}
					this.checkForRewards();
					return;
				}
				if (this.onBundleComplete != null)
				{
					this.onBundleComplete(this);
				}
			}
		}

		// Token: 0x06002A50 RID: 10832 RVA: 0x001FA66C File Offset: 0x001F886C
		private void restoreaAreaOnExit_AbandonedJojaMart()
		{
			Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart", false).restoreAreaCutscene();
		}

		// Token: 0x06002A51 RID: 10833 RVA: 0x001FA67E File Offset: 0x001F887E
		private void restoreAreaOnExit()
		{
			if (!this.fromGameMenu)
			{
				Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).restoreAreaCutscene(this.whichArea);
			}
		}

		// Token: 0x06002A52 RID: 10834 RVA: 0x001FA6A0 File Offset: 0x001F88A0
		public void checkForRewards()
		{
			Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
			foreach (string i in bundlesInfo.Keys)
			{
				if (i.Contains(CommunityCenter.getAreaNameFromNumber(this.whichArea)) && bundlesInfo[i].Split('/', StringSplitOptions.None)[1].Length > 1)
				{
					int bundleIndex = Convert.ToInt32(i.Split('/', StringSplitOptions.None)[1]);
					if (Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundleRewards[bundleIndex])
					{
						this.presentButton = new ClickableAnimatedComponent(new Rectangle(this.xPositionOnScreen + 592, this.yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), false, false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, true));
						break;
					}
				}
			}
		}

		// Token: 0x06002A53 RID: 10835 RVA: 0x001FA80C File Offset: 0x001F8A0C
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!JunimoNoteMenu.canClick)
			{
				return;
			}
			if (this.specificBundlePage)
			{
				this.heldItem = this.inventory.rightClick(x, y, this.heldItem, true, false);
				if (this.partialDonationItem != null)
				{
					int i = 0;
					while (i < this.ingredientSlots.Count)
					{
						if (this.ingredientSlots[i].containsPoint(x, y) && this.ingredientSlots[i].item == this.partialDonationItem)
						{
							if (this.partialDonationComponents.Count <= 0)
							{
								break;
							}
							Item item = this.partialDonationComponents[0].getOne();
							bool valid = false;
							if (this.heldItem == null)
							{
								this.heldItem = item;
								Game1.playSound("dwop", null);
								valid = true;
							}
							else if (this.heldItem.canStackWith(item))
							{
								this.heldItem.addToStack(item);
								Game1.playSound("dwop", null);
								valid = true;
							}
							if (!valid)
							{
								break;
							}
							if (this.partialDonationComponents[0].ConsumeStack(1) == null)
							{
								this.partialDonationComponents.RemoveAt(0);
							}
							if (this.partialDonationItem != null)
							{
								int count = 0;
								foreach (Item contributedItem in this.partialDonationComponents)
								{
									count += contributedItem.Stack;
								}
								this.partialDonationItem.Stack = count;
							}
							if (this.partialDonationComponents.Count == 0)
							{
								this.ResetPartialDonation();
								break;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
			}
			if (!this.specificBundlePage && this.isReadyToCloseMenuOrBundle())
			{
				base.exitThisMenu(this.gameMenuTabToReturnTo == -1);
			}
		}

		// Token: 0x06002A54 RID: 10836 RVA: 0x001FA9E0 File Offset: 0x001F8BE0
		public override void update(GameTime time)
		{
			if (this.specificBundlePage && this.currentPageBundle != null && this.currentPageBundle.completionTimer <= 0 && this.isReadyToCloseMenuOrBundle() && this.currentPageBundle.complete)
			{
				this.takeDownBundleSpecificPage();
			}
			foreach (Bundle bundle in this.bundles)
			{
				bundle.update(time);
			}
			JunimoNoteMenu.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			ClickableAnimatedComponent clickableAnimatedComponent = this.presentButton;
			if (clickableAnimatedComponent != null)
			{
				clickableAnimatedComponent.update(time);
			}
			if (JunimoNoteMenu.screenSwipe != null)
			{
				JunimoNoteMenu.canClick = false;
				if (JunimoNoteMenu.screenSwipe.update(time))
				{
					JunimoNoteMenu.screenSwipe = null;
					JunimoNoteMenu.canClick = true;
					Action<JunimoNoteMenu> action = this.onScreenSwipeFinished;
					if (action != null)
					{
						action(this);
					}
				}
			}
			if (this.bundlesChanged && this.fromGameMenu)
			{
				this.reOpenThisMenu();
			}
		}

		// Token: 0x06002A55 RID: 10837 RVA: 0x001FAB00 File Offset: 0x001F8D00
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (this.scrambledText)
			{
				return;
			}
			JunimoNoteMenu.hoverText = "";
			if (!this.specificBundlePage)
			{
				if (this.presentButton != null)
				{
					JunimoNoteMenu.hoverText = this.presentButton.tryHover(x, y);
				}
				foreach (Bundle bundle in this.bundles)
				{
					bundle.tryHoverAction(x, y);
				}
				if (this.fromGameMenu)
				{
					this.areaNextButton.tryHover(x, y, 0.1f);
					this.areaBackButton.tryHover(x, y, 0.1f);
				}
				return;
			}
			ClickableTextureComponent clickableTextureComponent = this.backButton;
			if (clickableTextureComponent != null)
			{
				clickableTextureComponent.tryHover(x, y, 0.1f);
			}
			if (!this.currentPageBundle.complete && this.currentPageBundle.completionTimer <= 0)
			{
				this.hoveredItem = this.inventory.hover(x, y, this.heldItem);
			}
			else
			{
				this.hoveredItem = null;
			}
			foreach (ClickableTextureComponent c in this.ingredientList)
			{
				if (c.bounds.Contains(x, y))
				{
					JunimoNoteMenu.hoverText = c.hoverText;
					break;
				}
			}
			if (this.heldItem != null)
			{
				foreach (ClickableTextureComponent c2 in this.ingredientSlots)
				{
					if (c2.bounds.Contains(x, y) && this.CanBePartiallyOrFullyDonated(this.heldItem) && (this.partialDonationItem == null || c2.item == this.partialDonationItem))
					{
						c2.sourceRect.X = 530;
						c2.sourceRect.Y = 262;
					}
					else
					{
						c2.sourceRect.X = 512;
						c2.sourceRect.Y = 244;
					}
				}
			}
			ClickableTextureComponent clickableTextureComponent2 = this.purchaseButton;
			if (clickableTextureComponent2 == null)
			{
				return;
			}
			clickableTextureComponent2.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002A56 RID: 10838 RVA: 0x001FAD40 File Offset: 0x001F8F40
		public override void draw(SpriteBatch b)
		{
			if (Game1.options.showMenuBackground)
			{
				base.drawBackground(b);
			}
			else if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			}
			if (!this.specificBundlePage)
			{
				b.Draw(this.noteTexture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(0, 0, 320, 180)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				SpriteText.drawStringHorizontallyCenteredAt(b, this.scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(this.whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(this.whichArea), this.xPositionOnScreen + this.width / 2 + 16, this.yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, this.scrambledText, null, 99999);
				if (this.scrambledText)
				{
					SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), this.xPositionOnScreen + 96, this.yPositionOnScreen + 96, 999999, this.width - 192, 99999, 0.88f, 0.88f, true, -1, "", null, SpriteText.ScrollTextAlignment.Left);
					base.draw(b);
					if (!Game1.options.SnappyMenus && JunimoNoteMenu.canClick)
					{
						base.drawMouse(b, false, -1);
					}
					return;
				}
				foreach (Bundle bundle in this.bundles)
				{
					bundle.draw(b);
				}
				ClickableAnimatedComponent clickableAnimatedComponent = this.presentButton;
				if (clickableAnimatedComponent != null)
				{
					clickableAnimatedComponent.draw(b);
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in JunimoNoteMenu.tempSprites)
				{
					temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				}
				if (this.fromGameMenu)
				{
					if (this.areaNextButton.visible)
					{
						this.areaNextButton.draw(b);
					}
					if (this.areaBackButton.visible)
					{
						this.areaBackButton.draw(b);
					}
				}
			}
			else
			{
				b.Draw(this.noteTexture, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(320, 0, 320, 180)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				if (this.currentPageBundle != null)
				{
					int bundle_index = this.currentPageBundle.bundleIndex;
					Texture2D bundle_texture = this.noteTexture;
					int y_offset = 180;
					if (this.currentPageBundle.bundleTextureIndexOverride >= 0)
					{
						bundle_index = this.currentPageBundle.bundleTextureIndexOverride;
					}
					if (this.currentPageBundle.bundleTextureOverride != null)
					{
						bundle_texture = this.currentPageBundle.bundleTextureOverride;
						y_offset = 0;
					}
					b.Draw(bundle_texture, new Vector2((float)(this.xPositionOnScreen + 872), (float)(this.yPositionOnScreen + 88)), new Rectangle?(new Rectangle(bundle_index * 16 * 2 % bundle_texture.Width, y_offset + 32 * (bundle_index * 16 * 2 / bundle_texture.Width), 32, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
					if (this.currentPageBundle.label != null)
					{
						float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label)).X;
						b.Draw(this.noteTexture, new Vector2((float)(this.xPositionOnScreen + 936 - (int)textX / 2 - 16), (float)(this.yPositionOnScreen + 228)), new Rectangle?(new Rectangle(517, 266, 4, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
						b.Draw(this.noteTexture, new Rectangle(this.xPositionOnScreen + 936 - (int)textX / 2, this.yPositionOnScreen + 228, (int)textX, 68), new Rectangle?(new Rectangle(520, 266, 1, 17)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
						b.Draw(this.noteTexture, new Vector2((float)(this.xPositionOnScreen + 936 + (int)textX / 2), (float)(this.yPositionOnScreen + 228)), new Rectangle?(new Rectangle(524, 266, 4, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
						b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(this.xPositionOnScreen + 936) - textX / 2f, (float)(this.yPositionOnScreen + 236)) + new Vector2(2f, 2f), Game1.textShadowColor);
						b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(this.xPositionOnScreen + 936) - textX / 2f, (float)(this.yPositionOnScreen + 236)) + new Vector2(0f, 2f), Game1.textShadowColor);
						b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(this.xPositionOnScreen + 936) - textX / 2f, (float)(this.yPositionOnScreen + 236)) + new Vector2(2f, 0f), Game1.textShadowColor);
						b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(this.xPositionOnScreen + 936) - textX / 2f, (float)(this.yPositionOnScreen + 236)), Game1.textColor * 0.9f);
					}
				}
				if (this.backButton != null)
				{
					this.backButton.draw(b);
				}
				if (this.purchaseButton != null)
				{
					this.purchaseButton.draw(b);
					Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
				}
				float completed_slot_alpha = 1f;
				if (this.partialDonationItem != null)
				{
					completed_slot_alpha = 0.25f;
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in JunimoNoteMenu.tempSprites)
				{
					temporaryAnimatedSprite2.draw(b, true, 0, 0, completed_slot_alpha);
				}
				foreach (ClickableTextureComponent c in this.ingredientSlots)
				{
					float alpha_mult = 1f;
					if (this.partialDonationItem != null && c.item != this.partialDonationItem)
					{
						alpha_mult = 0.25f;
					}
					if (c.item == null || (this.partialDonationItem != null && c.item == this.partialDonationItem))
					{
						c.draw(b, (this.fromGameMenu ? (Color.LightGray * 0.5f) : Color.White) * alpha_mult, 0.89f, 0, 0, 0);
					}
					c.drawItem(b, 4, 4, alpha_mult);
				}
				for (int i = 0; i < this.ingredientList.Count; i++)
				{
					float alpha_mult2 = 1f;
					if (this.currentPartialIngredientDescriptionIndex >= 0 && this.currentPartialIngredientDescriptionIndex != i)
					{
						alpha_mult2 = 0.25f;
					}
					ClickableTextureComponent c2 = this.ingredientList[i];
					bool completed = false;
					int num = i;
					Bundle bundle2 = this.currentPageBundle;
					int? num2;
					if (bundle2 == null)
					{
						num2 = null;
					}
					else
					{
						List<BundleIngredientDescription> ingredients = bundle2.ingredients;
						num2 = ((ingredients != null) ? new int?(ingredients.Count) : null);
					}
					int? num3 = num2;
					if ((num < num3.GetValueOrDefault() & num3 != null) && this.currentPageBundle.ingredients[i].completed)
					{
						completed = true;
					}
					if (!completed)
					{
						b.Draw(Game1.shadowTexture, new Vector2((float)(c2.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4), (float)(c2.bounds.Center.Y + 4)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha_mult2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					}
					if (c2.item != null && c2.visible)
					{
						c2.item.drawInMenu(b, new Vector2((float)c2.bounds.X, (float)c2.bounds.Y), c2.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (completed ? 0.25f : alpha_mult2), false);
					}
				}
				this.inventory.draw(b);
			}
			if (this.getRewardNameForArea(this.whichArea) != "")
			{
				SpriteText.drawStringWithScrollCenteredAt(b, this.getRewardNameForArea(this.whichArea), this.xPositionOnScreen + this.width / 2, Math.Min(this.yPositionOnScreen + this.height + 20, Game1.uiViewport.Height - 64 - 8), "", 1f, null, 0, 0.88f, false);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (JunimoNoteMenu.canClick)
			{
				base.drawMouse(b, false, -1);
			}
			Item item = this.heldItem;
			if (item != null)
			{
				item.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
			}
			if (this.inventory.descriptionText.Length > 0)
			{
				if (this.hoveredItem != null)
				{
					IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, false, -1, 0, null, -1, null, -1, null);
				}
			}
			else
			{
				IClickableMenu.drawHoverText(b, (!this.singleBundleMenu && !Game1.player.hasOrWillReceiveMail("canReadJunimoText") && JunimoNoteMenu.hoverText.Length > 0) ? "???" : JunimoNoteMenu.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			ScreenSwipe screenSwipe = JunimoNoteMenu.screenSwipe;
			if (screenSwipe == null)
			{
				return;
			}
			screenSwipe.draw(b);
		}

		// Token: 0x06002A57 RID: 10839 RVA: 0x001FB904 File Offset: 0x001F9B04
		public string getRewardNameForArea(int whichArea)
		{
			switch (whichArea)
			{
			case -1:
				return "";
			case 0:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry");
			case 1:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts");
			case 2:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank");
			case 3:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler");
			case 4:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault");
			case 5:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin");
			default:
				return "???";
			}
		}

		// Token: 0x06002A58 RID: 10840 RVA: 0x001FB9A4 File Offset: 0x001F9BA4
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			JunimoNoteMenu.tempSprites.Clear();
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
			this.backButton = new ClickableTextureComponent("Back", new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
			if (this.fromGameMenu)
			{
				this.areaNextButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 128, this.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
				{
					visible = false
				};
				this.areaBackButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
				{
					visible = false
				};
			}
			this.inventory = new InventoryMenu(this.xPositionOnScreen + 128, this.yPositionOnScreen + 140, true, null, new InventoryMenu.highlightThisItem(this.HighlightObjects), Game1.player.maxItems.Value, 6, 8, 8, false);
			for (int i = 0; i < this.inventory.inventory.Count; i++)
			{
				if (i >= this.inventory.actualInventory.Count)
				{
					this.inventory.inventory[i].visible = false;
				}
			}
			for (int j = 0; j < this.bundles.Count; j++)
			{
				Point p = this.getBundleLocationFromNumber(j);
				this.bundles[j].bounds.X = p.X;
				this.bundles[j].bounds.Y = p.Y;
				this.bundles[j].sprite.position = new Vector2((float)p.X, (float)p.Y);
			}
			if (this.specificBundlePage)
			{
				int numberOfIngredientSlots = this.currentPageBundle.numberOfIngredientSlots;
				List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
				this.addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
				this.ingredientSlots.Clear();
				for (int k = 0; k < ingredientSlotRectangles.Count; k++)
				{
					this.ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[k], this.noteTexture, new Rectangle(512, 244, 18, 18), 4f, false));
				}
				List<Rectangle> ingredientListRectangles = new List<Rectangle>();
				this.ingredientList.Clear();
				this.addRectangleRowsToList(ingredientListRectangles, this.currentPageBundle.ingredients.Count, 932, 364);
				for (int l = 0; l < ingredientListRectangles.Count; l++)
				{
					BundleIngredientDescription ingredient = this.currentPageBundle.ingredients[l];
					ItemMetadata metadata = ItemRegistry.GetMetadata(ingredient.id);
					if (((metadata != null) ? metadata.TypeIdentifier : null) == "(O)")
					{
						ParsedItemData parsedOrErrorData = metadata.GetParsedOrErrorData();
						Texture2D texture = parsedOrErrorData.GetTexture();
						Rectangle sourceRect = parsedOrErrorData.GetSourceRect(0, null);
						Item item = (ingredient.preservesId != null) ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality, false);
						this.ingredientList.Add(new ClickableTextureComponent("", ingredientListRectangles[l], "", item.DisplayName, texture, sourceRect, 4f, false)
						{
							myID = l + 1000,
							item = item,
							upNeighborID = -99998,
							rightNeighborID = -99998,
							leftNeighborID = -99998,
							downNeighborID = -99998
						});
					}
				}
				this.updateIngredientSlots();
			}
		}

		// Token: 0x06002A59 RID: 10841 RVA: 0x001FBDF4 File Offset: 0x001F9FF4
		private void setUpBundleSpecificPage(Bundle b)
		{
			JunimoNoteMenu.tempSprites.Clear();
			this.currentPageBundle = b;
			this.specificBundlePage = true;
			if (this.whichArea == 4)
			{
				if (!this.fromGameMenu)
				{
					this.purchaseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 800, this.yPositionOnScreen + 504, 260, 72), this.noteTexture, new Rectangle(517, 286, 65, 20), 4f, false)
					{
						myID = 797,
						leftNeighborID = 103
					};
					if (Game1.options.SnappyMenus)
					{
						this.currentlySnappedComponent = this.purchaseButton;
						this.snapCursorToCurrentSnappedComponent();
						return;
					}
				}
			}
			else
			{
				int numberOfIngredientSlots = b.numberOfIngredientSlots;
				List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
				this.addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
				for (int i = 0; i < ingredientSlotRectangles.Count; i++)
				{
					this.ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[i], this.noteTexture, new Rectangle(512, 244, 18, 18), 4f, false)
					{
						myID = i + 250,
						upNeighborID = -99998,
						rightNeighborID = -99998,
						leftNeighborID = -99998,
						downNeighborID = -99998
					});
				}
				List<Rectangle> ingredientListRectangles = new List<Rectangle>();
				this.addRectangleRowsToList(ingredientListRectangles, b.ingredients.Count, 932, 364);
				for (int j = 0; j < ingredientListRectangles.Count; j++)
				{
					BundleIngredientDescription ingredient = b.ingredients[j];
					string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
					if (itemData.HasTypeObject())
					{
						int? category = ingredient.category;
						if (category == null)
						{
							goto IL_24B;
						}
						int valueOrDefault = category.GetValueOrDefault();
						string displayName;
						if (valueOrDefault != -75)
						{
							switch (valueOrDefault)
							{
							case -6:
								displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
								break;
							case -5:
								displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
								break;
							case -4:
								displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
								break;
							case -3:
								goto IL_24B;
							case -2:
								displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
								break;
							default:
								goto IL_24B;
							}
						}
						else
						{
							displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
						}
						IL_254:
						Item item;
						if (ingredient.preservesId != null)
						{
							item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
							displayName = item.DisplayName;
						}
						else
						{
							item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality, false);
						}
						Texture2D texture = itemData.GetTexture();
						Rectangle sourceRect = itemData.GetSourceRect(0, null);
						this.ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", ingredientListRectangles[j], "", displayName, texture, sourceRect, 4f, false)
						{
							myID = j + 1000,
							item = item,
							upNeighborID = -99998,
							rightNeighborID = -99998,
							leftNeighborID = -99998,
							downNeighborID = -99998
						});
						goto IL_330;
						IL_24B:
						displayName = itemData.DisplayName;
						goto IL_254;
					}
					IL_330:;
				}
				this.updateIngredientSlots();
				if (Game1.options.SnappyMenus)
				{
					this.populateClickableComponentList();
					InventoryMenu inventoryMenu = this.inventory;
					if (((inventoryMenu != null) ? inventoryMenu.inventory : null) != null)
					{
						for (int k = 0; k < this.inventory.inventory.Count; k++)
						{
							if (this.inventory.inventory[k] != null)
							{
								if (this.inventory.inventory[k].downNeighborID == 101)
								{
									this.inventory.inventory[k].downNeighborID = -1;
								}
								if (this.inventory.inventory[k].leftNeighborID == -1)
								{
									this.inventory.inventory[k].leftNeighborID = 103;
								}
								if (this.inventory.inventory[k].upNeighborID >= 1000)
								{
									this.inventory.inventory[k].upNeighborID = 103;
								}
							}
						}
					}
					this.currentlySnappedComponent = base.getComponentWithID(0);
					this.snapCursorToCurrentSnappedComponent();
				}
			}
		}

		// Token: 0x06002A5A RID: 10842 RVA: 0x001FC264 File Offset: 0x001FA464
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (this.currentPartialIngredientDescriptionIndex >= 0)
			{
				if (this.ingredientSlots.Contains(b) && b.item != this.partialDonationItem)
				{
					return false;
				}
				if (this.ingredientList.Contains(b) && this.ingredientList.IndexOf(b as ClickableTextureComponent) != this.currentPartialIngredientDescriptionIndex)
				{
					return false;
				}
			}
			return (a.myID >= 5000 || a.myID == 101 || a.myID == 102) == (b.myID >= 5000 || b.myID == 101 || b.myID == 102);
		}

		// Token: 0x06002A5B RID: 10843 RVA: 0x001FC30C File Offset: 0x001FA50C
		private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
		{
			switch (numberOfItems)
			{
			case 1:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY, 1, 72, 72, 12));
				return;
			case 2:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY, 2, 72, 72, 12));
				return;
			case 3:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY, 3, 72, 72, 12));
				return;
			case 4:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY, 4, 72, 72, 12));
				return;
			case 5:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
				return;
			case 6:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
				return;
			case 7:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
				return;
			case 8:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
				return;
			case 9:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
				return;
			case 10:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
				return;
			case 11:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
				return;
			case 12:
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
				toAddTo.AddRange(this.createRowOfBoxesCenteredAt(this.xPositionOnScreen + centerX, this.yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
				return;
			default:
				return;
			}
		}

		// Token: 0x06002A5C RID: 10844 RVA: 0x001FC660 File Offset: 0x001FA860
		private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
		{
			List<Rectangle> rectangles = new List<Rectangle>();
			int actualXStart = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
			int actualYStart = yStart - boxHeight / 2;
			for (int i = 0; i < numBoxes; i++)
			{
				rectangles.Add(new Rectangle(actualXStart + i * (boxWidth + horizontalGap), actualYStart, boxWidth, boxHeight));
			}
			return rectangles;
		}

		// Token: 0x06002A5D RID: 10845 RVA: 0x001FC6AC File Offset: 0x001FA8AC
		public void takeDownBundleSpecificPage()
		{
			if (!this.isReadyToCloseMenuOrBundle())
			{
				return;
			}
			this.ReturnPartialDonations(false);
			this.hoveredItem = null;
			if (!this.specificBundlePage)
			{
				return;
			}
			this.specificBundlePage = false;
			this.ingredientSlots.Clear();
			this.ingredientList.Clear();
			JunimoNoteMenu.tempSprites.Clear();
			this.purchaseButton = null;
			if (Game1.options.SnappyMenus)
			{
				if (this.currentPageBundle != null)
				{
					this.currentlySnappedComponent = this.currentPageBundle;
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002A5E RID: 10846 RVA: 0x001FC734 File Offset: 0x001FA934
		private Point getBundleLocationFromNumber(int whichBundle)
		{
			Point location = new Point(this.xPositionOnScreen, this.yPositionOnScreen);
			switch (whichBundle)
			{
			case 0:
				location.X += 592;
				location.Y += 136;
				break;
			case 1:
				location.X += 392;
				location.Y += 384;
				break;
			case 2:
				location.X += 784;
				location.Y += 388;
				break;
			case 3:
				location.X += 304;
				location.Y += 252;
				break;
			case 4:
				location.X += 892;
				location.Y += 252;
				break;
			case 5:
				location.X += 588;
				location.Y += 276;
				break;
			case 6:
				location.X += 588;
				location.Y += 380;
				break;
			case 7:
				location.X += 440;
				location.Y += 164;
				break;
			case 8:
				location.X += 776;
				location.Y += 164;
				break;
			}
			return location;
		}

		// Token: 0x04001BD4 RID: 7124
		public const int region_ingredientSlotModifier = 250;

		// Token: 0x04001BD5 RID: 7125
		public const int region_ingredientListModifier = 1000;

		// Token: 0x04001BD6 RID: 7126
		public const int region_bundleModifier = 5000;

		// Token: 0x04001BD7 RID: 7127
		public const int region_areaNextButton = 101;

		// Token: 0x04001BD8 RID: 7128
		public const int region_areaBackButton = 102;

		// Token: 0x04001BD9 RID: 7129
		public const int region_backButton = 103;

		// Token: 0x04001BDA RID: 7130
		public const int region_purchaseButton = 104;

		// Token: 0x04001BDB RID: 7131
		public const int region_presentButton = 105;

		// Token: 0x04001BDC RID: 7132
		public const string noteTextureName = "LooseSprites\\JunimoNote";

		// Token: 0x04001BDD RID: 7133
		public Texture2D noteTexture;

		// Token: 0x04001BDE RID: 7134
		public bool specificBundlePage;

		// Token: 0x04001BDF RID: 7135
		public const int baseWidth = 320;

		// Token: 0x04001BE0 RID: 7136
		public const int baseHeight = 180;

		// Token: 0x04001BE1 RID: 7137
		public InventoryMenu inventory;

		// Token: 0x04001BE2 RID: 7138
		public Item partialDonationItem;

		// Token: 0x04001BE3 RID: 7139
		public List<Item> partialDonationComponents = new List<Item>();

		// Token: 0x04001BE4 RID: 7140
		public BundleIngredientDescription? currentPartialIngredientDescription;

		// Token: 0x04001BE5 RID: 7141
		public int currentPartialIngredientDescriptionIndex = -1;

		// Token: 0x04001BE6 RID: 7142
		public Item heldItem;

		// Token: 0x04001BE7 RID: 7143
		public Item hoveredItem;

		// Token: 0x04001BE8 RID: 7144
		public static bool canClick = true;

		// Token: 0x04001BE9 RID: 7145
		public int whichArea;

		// Token: 0x04001BEA RID: 7146
		public int gameMenuTabToReturnTo = -1;

		// Token: 0x04001BEB RID: 7147
		public IClickableMenu menuToReturnTo;

		// Token: 0x04001BEC RID: 7148
		public bool bundlesChanged;

		// Token: 0x04001BED RID: 7149
		public static ScreenSwipe screenSwipe;

		// Token: 0x04001BEE RID: 7150
		public static string hoverText = "";

		// Token: 0x04001BEF RID: 7151
		public List<Bundle> bundles = new List<Bundle>();

		// Token: 0x04001BF0 RID: 7152
		public static TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04001BF1 RID: 7153
		public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();

		// Token: 0x04001BF2 RID: 7154
		public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();

		// Token: 0x04001BF3 RID: 7155
		public bool fromGameMenu;

		// Token: 0x04001BF4 RID: 7156
		public bool fromThisMenu;

		// Token: 0x04001BF5 RID: 7157
		public bool scrambledText;

		// Token: 0x04001BF6 RID: 7158
		private bool singleBundleMenu;

		// Token: 0x04001BF7 RID: 7159
		public ClickableTextureComponent backButton;

		// Token: 0x04001BF8 RID: 7160
		public ClickableTextureComponent purchaseButton;

		// Token: 0x04001BF9 RID: 7161
		public ClickableTextureComponent areaNextButton;

		// Token: 0x04001BFA RID: 7162
		public ClickableTextureComponent areaBackButton;

		// Token: 0x04001BFB RID: 7163
		public ClickableAnimatedComponent presentButton;

		// Token: 0x04001BFC RID: 7164
		public Action<int> onIngredientDeposit;

		// Token: 0x04001BFD RID: 7165
		public Action<JunimoNoteMenu> onBundleComplete;

		// Token: 0x04001BFE RID: 7166
		public Action<JunimoNoteMenu> onScreenSwipeFinished;

		// Token: 0x04001BFF RID: 7167
		public Bundle currentPageBundle;

		// Token: 0x04001C00 RID: 7168
		private int oldTriggerSpot;
	}
}
