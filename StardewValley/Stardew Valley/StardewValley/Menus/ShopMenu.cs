using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace StardewValley.Menus
{
	// Token: 0x020002A7 RID: 679
	public class ShopMenu : IClickableMenu
	{
		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06002C3F RID: 11327 RVA: 0x0021E12C File Offset: 0x0021C32C
		// (set) Token: 0x06002C40 RID: 11328 RVA: 0x0021E134 File Offset: 0x0021C334
		public ShopMenu.ShopCachedTheme VisualTheme { get; private set; }

		// Token: 0x06002C41 RID: 11329 RVA: 0x0021E140 File Offset: 0x0021C340
		public ShopMenu(string shopId, ShopData shopData, ShopOwnerData ownerData, NPC owner = null, ShopMenu.OnPurchaseDelegate onPurchase = null, Func<ISalable, bool> onSell = null, bool playOpenSound = true)
		{
			if (shopId == null)
			{
				throw new ArgumentNullException("shopId");
			}
			this.ShopId = shopId;
			foreach (KeyValuePair<ISalable, ItemStockInformation> pair in ShopBuilder.GetShopStock(shopId, shopData))
			{
				this.AddForSale(pair.Key, pair.Value);
			}
			this.ShopData = shopData;
			if (shopData.SalableItemTags != null)
			{
				foreach (string text in shopData.SalableItemTags)
				{
					List<string> list = new List<string>();
					foreach (string tag in text.Split(',', StringSplitOptions.None))
					{
						list.Add(tag.Trim());
					}
					this.tagsToSellHere.Add(list);
				}
			}
			this.openMenuSound = (shopData.OpenSound ?? this.openMenuSound);
			this.purchaseSound = (shopData.PurchaseSound ?? this.purchaseSound);
			this.purchaseRepeatSound = (shopData.PurchaseRepeatSound ?? this.purchaseRepeatSound);
			List<ShopThemeData> visualTheme = shopData.VisualTheme;
			ShopThemeData visualTheme2;
			if (visualTheme == null)
			{
				visualTheme2 = null;
			}
			else
			{
				visualTheme2 = visualTheme.FirstOrDefault((ShopThemeData theme) => GameStateQuery.CheckConditions(theme.Condition, null, null, null, null, null, null));
			}
			this.SetVisualTheme(visualTheme2);
			this.SetUpShopOwner(ownerData, owner);
			this.Initialize(shopData.Currency, onPurchase, onSell, playOpenSound);
		}

		// Token: 0x06002C42 RID: 11330 RVA: 0x0021E394 File Offset: 0x0021C594
		public ShopMenu(string shopId, Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, int currency = 0, string who = null, ShopMenu.OnPurchaseDelegate on_purchase = null, Func<ISalable, bool> on_sell = null, bool playOpenSound = true)
		{
			if (shopId == null)
			{
				throw new ArgumentNullException("shopId");
			}
			this.ShopId = shopId;
			foreach (KeyValuePair<ISalable, ItemStockInformation> pair in itemPriceAndStock)
			{
				this.AddForSale(pair.Key, pair.Value);
			}
			this.SetVisualTheme(null);
			this.setUpShopOwner(who, shopId);
			this.Initialize(currency, on_purchase, on_sell, playOpenSound);
		}

		// Token: 0x06002C43 RID: 11331 RVA: 0x0021E4E0 File Offset: 0x0021C6E0
		public ShopMenu(string shopId, List<ISalable> itemsForSale, int currency = 0, string who = null, ShopMenu.OnPurchaseDelegate on_purchase = null, Func<ISalable, bool> on_sell = null, bool playOpenSound = true) : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true)
		{
			if (shopId == null)
			{
				throw new ArgumentNullException("shopId");
			}
			this.ShopId = shopId;
			foreach (ISalable item in itemsForSale)
			{
				this.AddForSale(item, null);
			}
			this.SetVisualTheme(null);
			this.setUpShopOwner(who, shopId);
			this.Initialize(currency, on_purchase, on_sell, playOpenSound);
		}

		// Token: 0x06002C44 RID: 11332 RVA: 0x0021E674 File Offset: 0x0021C874
		public void SetVisualTheme(ShopThemeData theme)
		{
			this.VisualTheme = new ShopMenu.ShopCachedTheme(theme);
			if (this.upArrow != null)
			{
				Rectangle bounds = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
				this.gameWindowSizeChanged(bounds, bounds);
			}
		}

		// Token: 0x06002C45 RID: 11333 RVA: 0x0021E6CC File Offset: 0x0021C8CC
		private void Initialize(int currency, ShopMenu.OnPurchaseDelegate onPurchase, Func<ISalable, bool> onSell, bool playOpenSound)
		{
			ShopMenu.ShopCachedTheme theme = this.VisualTheme;
			this.updatePosition();
			this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 36, this.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
			this.currency = currency;
			this.onPurchase = onPurchase;
			this.onSell = onSell;
			Game1.player.forceCanMove();
			if (playOpenSound)
			{
				this.PlayOpenSound();
			}
			this.inventory = new InventoryMenu(this.xPositionOnScreen + this.width, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, false, null, new InventoryMenu.highlightThisItem(this.highlightItemToSell), -1, 3, 0, 0, true)
			{
				showGrayedOutSlots = true
			};
			this.inventory.movePosition(-this.inventory.width - 32, 0);
			this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 16, 44, 48), theme.ScrollUpTexture, theme.ScrollUpSourceRect, 4f, false)
			{
				myID = 97865,
				downNeighborID = 106,
				leftNeighborID = 3546
			};
			this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), theme.ScrollDownTexture, theme.ScrollDownSourceRect, 4f, false)
			{
				myID = 106,
				upNeighborID = 97865,
				leftNeighborID = 3546
			};
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), theme.ScrollBarFrontTexture, theme.ScrollBarFrontSourceRect, 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
			for (int i = 0; i < 4; i++)
			{
				this.forSaleButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * ((this.height - 256) / 4), this.width - 32, (this.height - 256) / 4 + 4), i.ToString() ?? "")
				{
					myID = i + 3546,
					rightNeighborID = 97865,
					fullyImmutable = true
				});
			}
			this.updateSaleButtonNeighbors();
			this.setUpStoreForContext();
			if (this.tabButtons.Count > 0)
			{
				foreach (ClickableComponent clickableComponent in this.forSaleButtons)
				{
					clickableComponent.leftNeighborID = -99998;
				}
			}
			this.applyTab();
			foreach (ClickableComponent clickableComponent2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				clickableComponent2.upNeighborID = -99998;
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			if (currency == 4)
			{
				int tickOpened = Game1.ticks;
				Game1.specialCurrencyDisplay.ShowCurrency("qiGems", () => Game1.ticks == tickOpened || (Game1.activeClickableMenu == this && this.currency == 4), 0f);
			}
		}

		// Token: 0x06002C46 RID: 11334 RVA: 0x0021EAF0 File Offset: 0x0021CCF0
		public void AddForSale(ISalable item, ItemStockInformation stock = null)
		{
			if (item.IsRecipe)
			{
				if (Game1.player.knowsRecipe(item.Name))
				{
					return;
				}
				item.Stack = 1;
			}
			this.forSale.Add(item);
			this.itemPriceAndStock.Add(item, stock ?? new ItemStockInformation(item.salePrice(false), item.Stack, null, null, LimitedStockMode.Global, null, null, null, null));
		}

		// Token: 0x06002C47 RID: 11335 RVA: 0x0021EB68 File Offset: 0x0021CD68
		public void updateSaleButtonNeighbors()
		{
			ClickableComponent lastValidButton = this.forSaleButtons[0];
			for (int i = 0; i < this.forSaleButtons.Count; i++)
			{
				ClickableComponent button = this.forSaleButtons[i];
				button.upNeighborImmutable = true;
				button.downNeighborImmutable = true;
				button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : -7777);
				button.downNeighborID = ((i < 3 && i < this.forSale.Count - 1) ? (i + 3546 + 1) : -7777);
				if (i >= this.forSale.Count)
				{
					if (button == this.currentlySnappedComponent)
					{
						this.currentlySnappedComponent = lastValidButton;
						if (Game1.options.SnappyMenus)
						{
							this.snapCursorToCurrentSnappedComponent();
						}
					}
				}
				else
				{
					lastValidButton = button;
				}
			}
		}

		// Token: 0x06002C48 RID: 11336 RVA: 0x0021EC34 File Offset: 0x0021CE34
		public virtual void setUpStoreForContext()
		{
			this.tabButtons = null;
			string shopId = this.ShopId;
			if (shopId != null)
			{
				int length = shopId.Length;
				switch (length)
				{
				case 7:
					if (shopId == "Dresser")
					{
						this.categoriesToSellHere.AddRange(new int[]
						{
							-95,
							-100,
							-97,
							-96
						});
						this.UseDresserTabs();
						this._isStorageShop = true;
						goto IL_19B;
					}
					break;
				case 8:
					if (shopId == "FishTank")
					{
						this.UseNoTabs();
						this._isStorageShop = true;
						goto IL_19B;
					}
					break;
				case 9:
					if (shopId == "Catalogue")
					{
						this.UseCatalogueTabs();
						goto IL_19B;
					}
					break;
				default:
					switch (length)
					{
					case 17:
						if (!(shopId == "ReturnedDonations"))
						{
							goto IL_195;
						}
						this.UseNoTabs();
						this._isStorageShop = true;
						goto IL_19B;
					case 18:
					case 20:
					case 21:
						goto IL_195;
					case 19:
						if (!(shopId == "Furniture Catalogue"))
						{
							goto IL_195;
						}
						break;
					case 22:
						if (!(shopId == "JojaFurnitureCatalogue"))
						{
							goto IL_195;
						}
						break;
					case 23:
					{
						char c = shopId[0];
						if (c != 'R')
						{
							if (c != 'T')
							{
								goto IL_195;
							}
							if (!(shopId == "TrashFurnitureCatalogue"))
							{
								goto IL_195;
							}
						}
						else if (!(shopId == "RetroFurnitureCatalogue"))
						{
							goto IL_195;
						}
						break;
					}
					case 24:
					{
						char c = shopId[0];
						if (c != 'J')
						{
							if (c != 'W')
							{
								goto IL_195;
							}
							if (!(shopId == "WizardFurnitureCatalogue"))
							{
								goto IL_195;
							}
						}
						else if (!(shopId == "JunimoFurnitureCatalogue"))
						{
							goto IL_195;
						}
						break;
					}
					default:
						goto IL_195;
					}
					this.UseFurnitureCatalogueTabs();
					goto IL_19B;
				}
			}
			IL_195:
			this.UseNoTabs();
			IL_19B:
			if (this._isStorageShop)
			{
				this.purchaseSound = null;
				this.purchaseRepeatSound = null;
			}
		}

		// Token: 0x06002C49 RID: 11337 RVA: 0x0021EDF2 File Offset: 0x0021CFF2
		public void UseNoTabs()
		{
			this.tabButtons = new List<ShopMenu.ShopTabClickableTextureComponent>();
			this.repositionTabs();
		}

		// Token: 0x06002C4A RID: 11338 RVA: 0x0021EE08 File Offset: 0x0021D008
		public void UseFurnitureCatalogueTabs()
		{
			List<ShopMenu.ShopTabClickableTextureComponent> list = new List<ShopMenu.ShopTabClickableTextureComponent>();
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent.myID = 99999;
			shopTabClickableTextureComponent.upNeighborID = -99998;
			shopTabClickableTextureComponent.downNeighborID = -99998;
			shopTabClickableTextureComponent.rightNeighborID = 3546;
			shopTabClickableTextureComponent.Filter = ((ISalable _) => true);
			list.Add(shopTabClickableTextureComponent);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent2 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent2.myID = 100000;
			shopTabClickableTextureComponent2.upNeighborID = -99998;
			shopTabClickableTextureComponent2.downNeighborID = -99998;
			shopTabClickableTextureComponent2.rightNeighborID = 3546;
			shopTabClickableTextureComponent2.Filter = delegate(ISalable item)
			{
				Furniture furniture = item as Furniture;
				return furniture != null && (furniture.IsTable() || furniture.furniture_type.Value == 4);
			};
			list.Add(shopTabClickableTextureComponent2);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent3 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent3.myID = 100001;
			shopTabClickableTextureComponent3.upNeighborID = -99998;
			shopTabClickableTextureComponent3.downNeighborID = -99998;
			shopTabClickableTextureComponent3.rightNeighborID = 3546;
			shopTabClickableTextureComponent3.Filter = delegate(ISalable item)
			{
				Furniture furniture = item as Furniture;
				return furniture != null && (furniture.furniture_type.Value == 0 || furniture.furniture_type.Value == 1 || furniture.furniture_type.Value == 2 || furniture.furniture_type.Value == 3);
			};
			list.Add(shopTabClickableTextureComponent3);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent4 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent4.myID = 100002;
			shopTabClickableTextureComponent4.upNeighborID = -99998;
			shopTabClickableTextureComponent4.downNeighborID = -99998;
			shopTabClickableTextureComponent4.rightNeighborID = 3546;
			shopTabClickableTextureComponent4.Filter = delegate(ISalable item)
			{
				Furniture furniture = item as Furniture;
				return furniture != null && (furniture.furniture_type.Value == 6 || furniture.furniture_type.Value == 13);
			};
			list.Add(shopTabClickableTextureComponent4);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent5 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent5.myID = 100003;
			shopTabClickableTextureComponent5.upNeighborID = -99998;
			shopTabClickableTextureComponent5.downNeighborID = -99998;
			shopTabClickableTextureComponent5.rightNeighborID = 3546;
			shopTabClickableTextureComponent5.Filter = delegate(ISalable item)
			{
				Furniture furniture = item as Furniture;
				return furniture != null && furniture.furniture_type.Value == 12;
			};
			list.Add(shopTabClickableTextureComponent5);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent6 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent6.myID = 100004;
			shopTabClickableTextureComponent6.upNeighborID = -99998;
			shopTabClickableTextureComponent6.downNeighborID = -99998;
			shopTabClickableTextureComponent6.rightNeighborID = 3546;
			shopTabClickableTextureComponent6.Filter = delegate(ISalable item)
			{
				Furniture furniture = item as Furniture;
				return furniture != null && (furniture.furniture_type.Value == 7 || furniture.furniture_type.Value == 17 || furniture.furniture_type.Value == 10 || furniture.furniture_type.Value == 8 || furniture.furniture_type.Value == 9 || furniture.furniture_type.Value == 14);
			};
			list.Add(shopTabClickableTextureComponent6);
			this.tabButtons = list;
			this.repositionTabs();
		}

		// Token: 0x06002C4B RID: 11339 RVA: 0x0021F120 File Offset: 0x0021D320
		public void UseCatalogueTabs()
		{
			List<ShopMenu.ShopTabClickableTextureComponent> list = new List<ShopMenu.ShopTabClickableTextureComponent>();
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent.myID = 99999;
			shopTabClickableTextureComponent.upNeighborID = -99998;
			shopTabClickableTextureComponent.downNeighborID = -99998;
			shopTabClickableTextureComponent.rightNeighborID = 3546;
			shopTabClickableTextureComponent.Filter = ((ISalable item) => true);
			list.Add(shopTabClickableTextureComponent);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent2 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent2.myID = 100000;
			shopTabClickableTextureComponent2.upNeighborID = -99998;
			shopTabClickableTextureComponent2.downNeighborID = -99998;
			shopTabClickableTextureComponent2.rightNeighborID = 3546;
			shopTabClickableTextureComponent2.Filter = delegate(ISalable item)
			{
				Wallpaper flooring = item as Wallpaper;
				return flooring != null && flooring.isFloor.Value;
			};
			list.Add(shopTabClickableTextureComponent2);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent3 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent3.myID = 100001;
			shopTabClickableTextureComponent3.upNeighborID = -99998;
			shopTabClickableTextureComponent3.downNeighborID = -99998;
			shopTabClickableTextureComponent3.rightNeighborID = 3546;
			shopTabClickableTextureComponent3.Filter = delegate(ISalable item)
			{
				Wallpaper wallpaper = item as Wallpaper;
				return wallpaper != null && !wallpaper.isFloor.Value;
			};
			list.Add(shopTabClickableTextureComponent3);
			this.tabButtons = list;
			this.repositionTabs();
		}

		// Token: 0x06002C4C RID: 11340 RVA: 0x0021F2BC File Offset: 0x0021D4BC
		public void UseDresserTabs()
		{
			List<ShopMenu.ShopTabClickableTextureComponent> list = new List<ShopMenu.ShopTabClickableTextureComponent>();
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent.myID = 99999;
			shopTabClickableTextureComponent.upNeighborID = -99998;
			shopTabClickableTextureComponent.downNeighborID = -99998;
			shopTabClickableTextureComponent.rightNeighborID = 3546;
			shopTabClickableTextureComponent.Filter = ((ISalable item) => true);
			list.Add(shopTabClickableTextureComponent);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent2 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent2.myID = 100000;
			shopTabClickableTextureComponent2.upNeighborID = -99998;
			shopTabClickableTextureComponent2.downNeighborID = -99998;
			shopTabClickableTextureComponent2.rightNeighborID = 3546;
			shopTabClickableTextureComponent2.Filter = delegate(ISalable salable)
			{
				Item item = salable as Item;
				return item != null && item.Category == -95;
			};
			list.Add(shopTabClickableTextureComponent2);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent3 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent3.myID = 100001;
			shopTabClickableTextureComponent3.upNeighborID = -99998;
			shopTabClickableTextureComponent3.downNeighborID = -99998;
			shopTabClickableTextureComponent3.rightNeighborID = 3546;
			shopTabClickableTextureComponent3.Filter = delegate(ISalable salable)
			{
				Clothing clothes = salable as Clothing;
				return clothes != null && clothes.clothesType.Value == Clothing.ClothesType.SHIRT;
			};
			list.Add(shopTabClickableTextureComponent3);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent4 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f, false);
			shopTabClickableTextureComponent4.myID = 100002;
			shopTabClickableTextureComponent4.upNeighborID = -99998;
			shopTabClickableTextureComponent4.downNeighborID = -99998;
			shopTabClickableTextureComponent4.rightNeighborID = 3546;
			shopTabClickableTextureComponent4.Filter = delegate(ISalable salable)
			{
				Clothing clothes = salable as Clothing;
				return clothes != null && clothes.clothesType.Value == Clothing.ClothesType.PANTS;
			};
			list.Add(shopTabClickableTextureComponent4);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent5 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent5.myID = 100003;
			shopTabClickableTextureComponent5.upNeighborID = -99998;
			shopTabClickableTextureComponent5.downNeighborID = -99998;
			shopTabClickableTextureComponent5.rightNeighborID = 3546;
			shopTabClickableTextureComponent5.Filter = delegate(ISalable salable)
			{
				Item item = salable as Item;
				return item != null && item.Category == -97;
			};
			list.Add(shopTabClickableTextureComponent5);
			ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent6 = new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f, false);
			shopTabClickableTextureComponent6.myID = 100004;
			shopTabClickableTextureComponent6.upNeighborID = -99998;
			shopTabClickableTextureComponent6.downNeighborID = -99998;
			shopTabClickableTextureComponent6.rightNeighborID = 3546;
			shopTabClickableTextureComponent6.Filter = delegate(ISalable salable)
			{
				Item item = salable as Item;
				return item != null && item.Category == -96;
			};
			list.Add(shopTabClickableTextureComponent6);
			this.tabButtons = list;
			this.repositionTabs();
		}

		// Token: 0x06002C4D RID: 11341 RVA: 0x0021F5D4 File Offset: 0x0021D7D4
		public void repositionTabs()
		{
			for (int i = 0; i < this.tabButtons.Count; i++)
			{
				if (i == this.currentTab)
				{
					this.tabButtons[i].bounds.X = this.xPositionOnScreen - 56;
				}
				else
				{
					this.tabButtons[i].bounds.X = this.xPositionOnScreen - 64;
				}
				this.tabButtons[i].bounds.Y = this.yPositionOnScreen + i * 16 * 4 + 16;
			}
		}

		// Token: 0x06002C4E RID: 11342 RVA: 0x0021F66C File Offset: 0x0021D86C
		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (direction != 0)
			{
				if (direction == 2)
				{
					if (this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
					{
						this.downArrowPressed();
						return;
					}
					int emptySlot = -1;
					for (int i = 0; i < 12; i++)
					{
						this.inventory.inventory[i].upNeighborID = oldID;
						if (emptySlot == -1 && this.heldItem != null)
						{
							IList<Item> actualInventory = this.inventory.actualInventory;
							if (actualInventory != null && actualInventory.Count > i && this.inventory.actualInventory[i] == null)
							{
								emptySlot = i;
							}
						}
					}
					this.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
					this.snapCursorToCurrentSnappedComponent();
					return;
				}
			}
			else if (this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				this.currentlySnappedComponent = base.getComponentWithID(3546);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002C4F RID: 11343 RVA: 0x0021F74C File Offset: 0x0021D94C
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(3546);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C50 RID: 11344 RVA: 0x0021F768 File Offset: 0x0021D968
		public void setUpShopOwner(string who, string shopId)
		{
			ShopData shopData;
			if (DataLoader.Shops(Game1.content).TryGetValue(shopId, out shopData))
			{
				foreach (ShopOwnerData owner in ShopBuilder.GetCurrentOwners(shopData))
				{
					if (owner.IsValid(who))
					{
						this.SetUpShopOwner(owner, null);
						break;
					}
				}
			}
		}

		// Token: 0x06002C51 RID: 11345 RVA: 0x0021F7D8 File Offset: 0x0021D9D8
		public void SetUpShopOwner(ShopOwnerData ownerData, NPC owner = null)
		{
			if (ownerData == null)
			{
				this.portraitTexture = null;
				this.potraitPersonDialogue = null;
				return;
			}
			string dialogueText = null;
			bool disableDialogue = false;
			if (ownerData.Dialogues != null)
			{
				Random random = ownerData.RandomizeDialogueOnOpen ? Game1.random : Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
				foreach (ShopDialogueData dialogue in ownerData.Dialogues)
				{
					if (GameStateQuery.CheckConditions(dialogue.Condition, null, null, null, null, null, null))
					{
						string rawText = dialogue.Dialogue;
						List<string> randomDialogue = dialogue.RandomDialogue;
						if (randomDialogue != null && randomDialogue.Any<string>())
						{
							rawText = random.ChooseFrom(dialogue.RandomDialogue);
						}
						dialogueText = TokenParser.ParseText(rawText, random, new TokenParserDelegate(this.ParseDialogueSubstitution), null);
						break;
					}
				}
				if (string.IsNullOrWhiteSpace(dialogueText))
				{
					disableDialogue = true;
				}
			}
			this.portraitTexture = this.TryLoadPortrait(ownerData, owner);
			if (!disableDialogue)
			{
				this.potraitPersonDialogue = Game1.parseText(dialogueText ?? Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11457"), Game1.dialogueFont, 304);
			}
		}

		// Token: 0x06002C52 RID: 11346 RVA: 0x0021F928 File Offset: 0x0021DB28
		public Texture2D TryLoadPortrait(ShopOwnerData ownerData, NPC owner)
		{
			if (ownerData.Type == ShopOwnerType.None)
			{
				return null;
			}
			if (ownerData.Portrait != null)
			{
				if (!string.IsNullOrWhiteSpace(ownerData.Portrait))
				{
					if (Game1.content.DoesAssetExist<Texture2D>(ownerData.Portrait))
					{
						return Game1.content.Load<Texture2D>(ownerData.Portrait);
					}
					NPC npc = Game1.getCharacterFromName(ownerData.Portrait, true, false);
					if (((npc != null) ? npc.Portrait : null) != null)
					{
						return npc.Portrait;
					}
				}
				return null;
			}
			if (((owner != null) ? owner.Portrait : null) != null)
			{
				return owner.Portrait;
			}
			if (ownerData.Type == ShopOwnerType.NamedNpc && !string.IsNullOrWhiteSpace(ownerData.Name))
			{
				NPC npc2 = Game1.getCharacterFromName(ownerData.Name, true, false);
				if (((npc2 != null) ? npc2.Portrait : null) != null)
				{
					return npc2.Portrait;
				}
			}
			return null;
		}

		// Token: 0x06002C53 RID: 11347 RVA: 0x0021F9EC File Offset: 0x0021DBEC
		public bool ParseDialogueSubstitution(string[] query, out string replacement, Random random, Farmer player)
		{
			if (query[0] == "SuggestedItem")
			{
				string interval = ArgUtility.Get(query, 1, "day", true);
				string syncKey = ArgUtility.Get(query, 2, this.ShopId, true);
				string error;
				if (!Utility.TryCreateIntervalRandom(interval, syncKey, out random, out error))
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Failed parsing [SuggestedItem ");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join(" ", query));
					defaultInterpolatedStringHandler.AppendLiteral("] in dialogue shop '");
					defaultInterpolatedStringHandler.AppendFormatted(this.ShopId);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(error);
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					random = Utility.CreateRandom((double)Game1.ticks, 0.0, 0.0, 0.0, 0.0);
				}
				ISalable suggestedItem;
				ItemStockInformation itemStockInformation;
				if (Utility.TryGetRandom<ISalable, ItemStockInformation>(this.itemPriceAndStock, out suggestedItem, out itemStockInformation, random))
				{
					replacement = suggestedItem.DisplayName;
					return true;
				}
			}
			replacement = null;
			return false;
		}

		// Token: 0x06002C54 RID: 11348 RVA: 0x0021FAF0 File Offset: 0x0021DCF0
		public bool highlightItemToSell(Item i)
		{
			if (this.heldItem != null)
			{
				return this.heldItem.canStackWith(i);
			}
			if (this.categoriesToSellHere.Contains(i.Category))
			{
				return true;
			}
			foreach (List<string> list in this.tagsToSellHere)
			{
				bool fail = false;
				foreach (string tag in list)
				{
					if (!i.HasContextTag(tag))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002C55 RID: 11349 RVA: 0x0021FBB8 File Offset: 0x0021DDB8
		public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
		{
			switch (currencyType)
			{
			case 0:
				return who.Money;
			case 1:
				return who.festivalScore;
			case 2:
				return who.clubCoins;
			case 4:
				return who.QiGems;
			}
			return 0;
		}

		// Token: 0x06002C56 RID: 11350 RVA: 0x0021FBF4 File Offset: 0x0021DDF4
		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				int y2 = this.scrollBar.bounds.Y;
				this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + 20));
				float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
				this.currentItemIndex = Math.Min(Math.Max(0, this.forSale.Count - 4), Math.Max(0, (int)((float)this.forSale.Count * percentage)));
				this.setScrollBarToCurrentIndex();
				this.updateSaleButtonNeighbors();
				if (y2 != this.scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4", null);
				}
			}
		}

		// Token: 0x06002C57 RID: 11351 RVA: 0x0021FCFE File Offset: 0x0021DEFE
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002C58 RID: 11352 RVA: 0x0021FD10 File Offset: 0x0021DF10
		private void setScrollBarToCurrentIndex()
		{
			if (this.forSale.Count > 0)
			{
				float percentage = (float)this.scrollBarRunner.Height / (float)Math.Max(1, this.forSale.Count - 4 + 1);
				this.scrollBar.bounds.Y = (int)(percentage * (float)this.currentItemIndex + (float)this.upArrow.bounds.Bottom + 4f);
				if (this.currentItemIndex == this.forSale.Count - 4)
				{
					this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
				}
			}
		}

		// Token: 0x06002C59 RID: 11353 RVA: 0x0021FDCC File Offset: 0x0021DFCC
		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4", null);
				return;
			}
			if (direction < 0 && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002C5A RID: 11354 RVA: 0x0021FE41 File Offset: 0x0021E041
		private void downArrowPressed()
		{
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			this.setScrollBarToCurrentIndex();
			this.updateSaleButtonNeighbors();
		}

		// Token: 0x06002C5B RID: 11355 RVA: 0x0021FE73 File Offset: 0x0021E073
		private void upArrowPressed()
		{
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			this.setScrollBarToCurrentIndex();
			this.updateSaleButtonNeighbors();
		}

		// Token: 0x06002C5C RID: 11356 RVA: 0x0021FEA8 File Offset: 0x0021E0A8
		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				Item item = this.heldItem as Item;
				if (item != null)
				{
					this.heldItem = null;
					if (Utility.CollectOrDrop(item))
					{
						Game1.playSound("stoneStep", null);
						return;
					}
					Game1.playSound("throwDownITem", null);
					return;
				}
			}
			base.receiveKeyPress(key);
		}

		// Token: 0x06002C5D RID: 11357 RVA: 0x0021FF1C File Offset: 0x0021E11C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, true);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
			if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
			{
				this.downArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shwip", null);
			}
			else if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
			for (int i = 0; i < this.tabButtons.Count; i++)
			{
				if (this.tabButtons[i].containsPoint(x, y))
				{
					this.switchTab(i);
				}
			}
			this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
			if (this.safetyTimer <= 0)
			{
				if (this.heldItem == null && !this.readOnly)
				{
					Item toSell = this.inventory.leftClick(x, y, null, false);
					if (toSell != null)
					{
						if (this.onSell != null)
						{
							this.onSell(toSell);
						}
						else
						{
							int sell_unit_price = (int)((float)toSell.sellToStorePrice(-1L) * this.sellPercentage);
							ShopMenu.chargePlayer(Game1.player, this.currency, -sell_unit_price * toSell.Stack);
							int coins = toSell.Stack / 8 + 2;
							for (int j = 0; j < coins; j++)
							{
								this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), false, false)
								{
									alphaFade = 0.025f,
									motion = new Vector2((float)Game1.random.Next(-3, 4), -4f),
									acceleration = new Vector2(0f, 0.5f),
									delayBeforeAnimationStart = j * 25,
									scale = 2f
								});
								this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), false, false)
								{
									scale = 4f,
									alphaFade = 0.025f,
									delayBeforeAnimationStart = j * 50,
									motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 8f),
									acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 0.5f)
								});
							}
							ISalable buyback_item = null;
							if (this.CanBuyback())
							{
								buyback_item = this.AddBuybackItem(toSell, sell_unit_price, toSell.Stack);
							}
							Object sellObj = toSell as Object;
							if (sellObj != null && sellObj.edibility.Value != -300)
							{
								Item stackClone = sellObj.getOne();
								stackClone.Stack = sellObj.Stack;
								ISalable soldTomorrowItem;
								if (buyback_item != null && this.buyBackItemsToResellTomorrow.TryGetValue(buyback_item, out soldTomorrowItem))
								{
									soldTomorrowItem.Stack += sellObj.Stack;
								}
								else
								{
									ShopLocation shopLocation = Game1.currentLocation as ShopLocation;
									if (shopLocation != null)
									{
										if (buyback_item != null)
										{
											this.buyBackItemsToResellTomorrow[buyback_item] = stackClone;
										}
										shopLocation.itemsToStartSellingTomorrow.Add(stackClone);
									}
								}
							}
							Game1.playSound("sell", null);
							Game1.playSound("purchase", null);
							if (this.inventory.getItemAt(x, y) == null)
							{
								this.animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
								{
									motion = new Vector2(0f, -0.5f)
								});
							}
						}
						this.updateSaleButtonNeighbors();
					}
				}
				else
				{
					this.heldItem = this.inventory.leftClick(x, y, this.heldItem as Item, true);
				}
				for (int k = 0; k < this.forSaleButtons.Count; k++)
				{
					if (this.currentItemIndex + k < this.forSale.Count && this.forSaleButtons[k].containsPoint(x, y))
					{
						int index = this.currentItemIndex + k;
						if (this.forSale[index] != null)
						{
							int toBuy = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? (Game1.oldKBState.IsKeyDown(Keys.D1) ? 999 : 25) : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / Math.Max(1, this.itemPriceAndStock[this.forSale[index]].Price)), Math.Max(1, this.itemPriceAndStock[this.forSale[index]].Stock)) : 1;
							if (this.ShopId == "ReturnedDonations")
							{
								toBuy = this.itemPriceAndStock[this.forSale[index]].Stock;
							}
							toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());
							if (toBuy == -1)
							{
								toBuy = 1;
							}
							if (this.canPurchaseCheck != null && !this.canPurchaseCheck(index))
							{
								return;
							}
							if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y))
							{
								this.itemPriceAndStock.Remove(this.forSale[index]);
								this.forSale.RemoveAt(index);
							}
							else if (toBuy <= 0)
							{
								if (this.itemPriceAndStock[this.forSale[index]].Price > 0)
								{
									Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
								}
								Game1.playSound("cancel", null);
							}
							if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus || (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && (this.heldItem.maximumStackSize() == 1 || this.heldItem.Stack == 999))) && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item, false))
							{
								this.heldItem = null;
								DelayedAction.playSoundAfterDelay("coin", 100, null, null, -1, false);
							}
						}
						this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
						this.updateSaleButtonNeighbors();
						this.setScrollBarToCurrentIndex();
						return;
					}
				}
			}
			if (this.readyToClose() && (x < this.xPositionOnScreen - 64 || y < this.yPositionOnScreen - 64 || x > this.xPositionOnScreen + this.width + 128 || y > this.yPositionOnScreen + this.height + 64))
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x06002C5E RID: 11358 RVA: 0x00220782 File Offset: 0x0021E982
		public virtual bool CanBuyback()
		{
			return true;
		}

		// Token: 0x06002C5F RID: 11359 RVA: 0x00220788 File Offset: 0x0021E988
		public virtual void BuyBuybackItem(ISalable bought_item, int price, int stack)
		{
			Game1.player.totalMoneyEarned -= (uint)price;
			if (Game1.player.useSeparateWallets)
			{
				Game1.player.stats.IndividualMoneyEarned -= (uint)price;
			}
			ISalable sold_tomorrow_item;
			if (this.buyBackItemsToResellTomorrow.TryGetValue(bought_item, out sold_tomorrow_item))
			{
				sold_tomorrow_item.Stack -= stack;
				if (sold_tomorrow_item.Stack <= 0)
				{
					this.buyBackItemsToResellTomorrow.Remove(bought_item);
					(Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Remove(sold_tomorrow_item as Item);
				}
			}
		}

		// Token: 0x06002C60 RID: 11360 RVA: 0x0022081C File Offset: 0x0021EA1C
		public virtual ISalable AddBuybackItem(ISalable sold_item, int sell_unit_price, int stack)
		{
			ISalable target = null;
			while (stack > 0)
			{
				target = null;
				foreach (ISalable buyback_item in this.buyBackItems)
				{
					if (buyback_item.canStackWith(sold_item) && buyback_item.Stack < buyback_item.maximumStackSize())
					{
						target = buyback_item;
						break;
					}
				}
				if (target == null)
				{
					target = sold_item.GetSalableInstance();
					int amount_to_deposit = Math.Min(stack, target.maximumStackSize());
					this.buyBackItems.Add(target);
					this.itemPriceAndStock.Add(target, new ItemStockInformation(sell_unit_price, amount_to_deposit, null, null, LimitedStockMode.Global, null, null, null, null));
					target.Stack = 1;
					stack -= amount_to_deposit;
				}
				else
				{
					int amount_to_deposit2 = Math.Min(stack, target.maximumStackSize() - target.Stack);
					ItemStockInformation stock_data = this.itemPriceAndStock[target];
					stock_data.Stock += amount_to_deposit2;
					this.itemPriceAndStock[target] = stock_data;
					target.Stack = 1;
					stack -= amount_to_deposit2;
				}
			}
			this.forSale = this.itemPriceAndStock.Keys.ToList<ISalable>();
			return target;
		}

		// Token: 0x06002C61 RID: 11361 RVA: 0x00220958 File Offset: 0x0021EB58
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (direction != 1 || !this.tabButtons.Contains(a) || !this.tabButtons.Contains(b)) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x06002C62 RID: 11362 RVA: 0x00220988 File Offset: 0x0021EB88
		public virtual void switchTab(int new_tab)
		{
			this.currentTab = new_tab;
			Game1.playSound("shwip", null);
			this.applyTab();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002C63 RID: 11363 RVA: 0x002209D4 File Offset: 0x0021EBD4
		public virtual void applyTab()
		{
			if (this.currentTab < 0 || this.currentTab >= this.tabButtons.Count)
			{
				this.forSale = this.itemPriceAndStock.Keys.ToList<ISalable>();
				return;
			}
			ShopMenu.ShopTabClickableTextureComponent tab = this.tabButtons[this.currentTab];
			if (tab.Filter == null)
			{
				tab.Filter = ((ISalable _) => true);
			}
			this.forSale.Clear();
			foreach (ISalable item in this.itemPriceAndStock.Keys)
			{
				if (tab.Filter(item))
				{
					this.forSale.Add(item);
				}
			}
			this.currentItemIndex = 0;
			this.setScrollBarToCurrentIndex();
			this.updateSaleButtonNeighbors();
		}

		// Token: 0x06002C64 RID: 11364 RVA: 0x00220AD0 File Offset: 0x0021ECD0
		public override bool readyToClose()
		{
			return this.heldItem == null && this.animations.Count == 0;
		}

		// Token: 0x06002C65 RID: 11365 RVA: 0x00220AEC File Offset: 0x0021ECEC
		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (this.heldItem != null)
			{
				Game1.player.addItemToInventoryBool(this.heldItem as Item, false);
				Game1.playSound("coin", null);
			}
		}

		// Token: 0x06002C66 RID: 11366 RVA: 0x00220B34 File Offset: 0x0021ED34
		public void PlayOpenSound()
		{
			Game1.playSound(this.openMenuSound, null);
		}

		// Token: 0x06002C67 RID: 11367 RVA: 0x00220B56 File Offset: 0x0021ED56
		public bool IsOutOfStock()
		{
			return !this._isStorageShop && this.forSale.Count == 0;
		}

		// Token: 0x06002C68 RID: 11368 RVA: 0x00220B70 File Offset: 0x0021ED70
		public static void chargePlayer(Farmer who, int currencyType, int amount)
		{
			switch (currencyType)
			{
			case 0:
				who.Money -= amount;
				return;
			case 1:
				who.festivalScore -= amount;
				return;
			case 2:
				who.clubCoins -= amount;
				return;
			case 3:
				break;
			case 4:
				who.QiGems -= amount;
				break;
			default:
				return;
			}
		}

		// Token: 0x06002C69 RID: 11369 RVA: 0x00220BD3 File Offset: 0x0021EDD3
		public virtual void HandleSynchedItemPurchase(ISalable item, Farmer who, int number_purchased)
		{
			if (this.itemPriceAndStock.ContainsKey(item))
			{
				who.team.synchronizedShopStock.OnItemPurchased(this.ShopId, item, this.itemPriceAndStock, number_purchased);
			}
		}

		// Token: 0x06002C6A RID: 11370 RVA: 0x00220C04 File Offset: 0x0021EE04
		private bool tryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y)
		{
			if (this.readOnly)
			{
				return false;
			}
			ItemStockInformation stock = this.itemPriceAndStock[item];
			if (held_item == null)
			{
				if (stock.Stock == 0)
				{
					this.hoveredItem = null;
					return true;
				}
				if (stockToBuy > item.GetSalableInstance().maximumStackSize())
				{
					stockToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
				}
				int price = stock.Price * stockToBuy;
				string extraTradeItem = null;
				int extraTradeItemCount = 5;
				int stacksToBuy = stockToBuy * item.Stack;
				if (stock.TradeItem != null)
				{
					extraTradeItem = stock.TradeItem;
					if (stock.TradeItemCount != null)
					{
						extraTradeItemCount = stock.TradeItemCount.Value;
					}
					extraTradeItemCount *= stockToBuy;
				}
				if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price && (extraTradeItem == null || this.HasTradeItem(extraTradeItem, extraTradeItemCount)))
				{
					this.heldItem = item.GetSalableInstance();
					this.heldItem.Stack = stacksToBuy;
					if (!this.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && !item.IsRecipe)
					{
						Game1.playSound("smallSelect", null);
						this.heldItem = null;
						return false;
					}
					if (this.CanBuyback() && this.buyBackItems.Contains(item))
					{
						this.BuyBuybackItem(item, price, stacksToBuy);
					}
					ShopMenu.chargePlayer(Game1.player, this.currency, price);
					if (!string.IsNullOrEmpty(extraTradeItem))
					{
						this.ConsumeTradeItem(extraTradeItem, extraTradeItemCount);
					}
					if (!this._isStorageShop && item.actionWhenPurchased(this.ShopId))
					{
						if (item.IsRecipe)
						{
							Item item2 = item as Item;
							if (item2 != null)
							{
								item2.LearnRecipe(null);
							}
							Game1.playSound("newRecipe", null);
						}
						held_item = null;
						this.heldItem = null;
					}
					else
					{
						Item item3 = this.heldItem as Item;
						if (((item3 != null) ? item3.QualifiedItemId : null) == "(O)858")
						{
							Game1.player.team.addQiGemsToTeam.Fire(this.heldItem.Stack);
							this.heldItem = null;
						}
						if (Game1.mouseClickPolling > 300)
						{
							if (this.purchaseRepeatSound != null)
							{
								Game1.playSound(this.purchaseRepeatSound, null);
							}
						}
						else if (this.purchaseSound != null)
						{
							Game1.playSound(this.purchaseSound, null);
						}
					}
					if (stock.Stock != 2147483647 && !item.IsInfiniteStock())
					{
						this.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
						if (stock.ItemToSyncStack != null)
						{
							stock.ItemToSyncStack.Stack = stock.Stock;
						}
					}
					List<string> actionsOnPurchase = stock.ActionsOnPurchase;
					if (actionsOnPurchase != null && actionsOnPurchase.Count > 0)
					{
						foreach (string action in stock.ActionsOnPurchase)
						{
							string error;
							Exception ex;
							if (!TriggerActionManager.TryRunAction(action, out error, out ex))
							{
								IGameLogger log = Game1.log;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 4);
								defaultInterpolatedStringHandler.AppendLiteral("Shop ");
								defaultInterpolatedStringHandler.AppendFormatted(this.ShopId);
								defaultInterpolatedStringHandler.AppendLiteral(" ignored invalid action '");
								defaultInterpolatedStringHandler.AppendFormatted(action);
								defaultInterpolatedStringHandler.AppendLiteral("' on purchase of item '");
								defaultInterpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
								defaultInterpolatedStringHandler.AppendLiteral("': ");
								defaultInterpolatedStringHandler.AppendFormatted(error);
								log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
							}
						}
					}
					if (this.onPurchase != null && this.onPurchase(item, Game1.player, stockToBuy, stock))
					{
						base.exitThisMenu(true);
					}
				}
				else
				{
					if (price > 0)
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					}
					Game1.playSound("cancel", null);
				}
			}
			else if (held_item.canStackWith(item))
			{
				stockToBuy = Math.Min(stockToBuy, (held_item.maximumStackSize() - held_item.Stack) / item.Stack);
				int stacksToBuy2 = stockToBuy * item.Stack;
				if (stockToBuy > 0)
				{
					int price2 = stock.Price * stockToBuy;
					string extraTradeItem2 = null;
					int extraTradeItemCount2 = 5;
					if (stock.TradeItem != null)
					{
						extraTradeItem2 = stock.TradeItem;
						if (stock.TradeItemCount != null)
						{
							extraTradeItemCount2 = stock.TradeItemCount.Value;
						}
						extraTradeItemCount2 *= stockToBuy;
					}
					ISalable salableInstance = item.GetSalableInstance();
					salableInstance.Stack = stacksToBuy2;
					if (!salableInstance.CanBuyItem(Game1.player))
					{
						Game1.playSound("cancel", null);
						return false;
					}
					if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price2 && (extraTradeItem2 == null || this.HasTradeItem(extraTradeItem2, extraTradeItemCount2)))
					{
						this.heldItem.Stack += stacksToBuy2;
						if (this.CanBuyback() && this.buyBackItems.Contains(item))
						{
							this.BuyBuybackItem(item, price2, stacksToBuy2);
						}
						ShopMenu.chargePlayer(Game1.player, this.currency, price2);
						if (Game1.mouseClickPolling > 300)
						{
							if (this.purchaseRepeatSound != null)
							{
								Game1.playSound(this.purchaseRepeatSound, null);
							}
						}
						else if (this.purchaseSound != null)
						{
							Game1.playSound(this.purchaseSound, null);
						}
						if (extraTradeItem2 != null)
						{
							this.ConsumeTradeItem(extraTradeItem2, extraTradeItemCount2);
						}
						if (!this._isStorageShop && item.actionWhenPurchased(this.ShopId))
						{
							this.heldItem = null;
						}
						if (stock.Stock != 2147483647 && !item.IsInfiniteStock())
						{
							this.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
							if (stock.ItemToSyncStack != null)
							{
								stock.ItemToSyncStack.Stack = stock.Stock;
							}
						}
						if (this.onPurchase != null && this.onPurchase(item, Game1.player, stockToBuy, stock))
						{
							base.exitThisMenu(true);
						}
					}
					else
					{
						if (price2 > 0)
						{
							Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						}
						Game1.playSound("cancel", null);
					}
				}
			}
			if (stock.Stock <= 0)
			{
				this.buyBackItems.Remove(item);
				this.hoveredItem = null;
				return true;
			}
			return false;
		}

		// Token: 0x06002C6B RID: 11371 RVA: 0x00221204 File Offset: 0x0021F404
		public bool HasTradeItem(string itemId, int count)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == "(O)858")
			{
				return Game1.player.QiGems >= count;
			}
			if (!(itemId == "(O)73"))
			{
				return Game1.player.Items.ContainsId(itemId, count);
			}
			return Game1.netWorldState.Value.GoldenWalnuts >= count;
		}

		// Token: 0x06002C6C RID: 11372 RVA: 0x00221270 File Offset: 0x0021F470
		public void ConsumeTradeItem(string itemId, int count)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == "(O)858")
			{
				Game1.player.QiGems = Math.Max(0, Game1.player.QiGems - count);
				return;
			}
			if (!(itemId == "(O)73"))
			{
				Game1.player.Items.ReduceId(itemId, count);
				return;
			}
			Game1.netWorldState.Value.GoldenWalnuts = Math.Max(0, Game1.netWorldState.Value.GoldenWalnuts - count);
		}

		// Token: 0x06002C6D RID: 11373 RVA: 0x002212F8 File Offset: 0x0021F4F8
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
			if (this.safetyTimer <= 0)
			{
				if (this.heldItem == null && !this.readOnly)
				{
					ISalable toSell = this.inventory.rightClick(x, y, null, false, false);
					if (toSell != null)
					{
						if (this.onSell != null)
						{
							this.onSell(toSell);
						}
						else
						{
							int sell_unit_price = (int)((float)toSell.sellToStorePrice(-1L) * this.sellPercentage);
							int sell_stack = toSell.Stack;
							ISalable sold_item = toSell;
							ShopMenu.chargePlayer(Game1.player, this.currency, -sell_unit_price * sell_stack);
							ISalable buyback_item = null;
							if (this.CanBuyback())
							{
								buyback_item = this.AddBuybackItem(toSell, sell_unit_price, sell_stack);
							}
							if (Game1.mouseClickPolling > 300)
							{
								if (this.purchaseRepeatSound != null)
								{
									Game1.playSound(this.purchaseRepeatSound, null);
								}
							}
							else if (this.purchaseSound != null)
							{
								Game1.playSound(this.purchaseSound, null);
							}
							int coins = 2;
							for (int i = 0; i < coins; i++)
							{
								this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), false, false)
								{
									alphaFade = 0.025f,
									motion = new Vector2((float)Game1.random.Next(-3, 4), -4f),
									acceleration = new Vector2(0f, 0.5f),
									delayBeforeAnimationStart = i * 25,
									scale = 2f
								});
								this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), false, false)
								{
									scale = 4f,
									alphaFade = 0.025f,
									delayBeforeAnimationStart = i * 50,
									motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 8f),
									acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 0.5f)
								});
							}
							ISalable soldTomorrowItem;
							if (buyback_item != null && this.buyBackItemsToResellTomorrow.TryGetValue(buyback_item, out soldTomorrowItem))
							{
								soldTomorrowItem.Stack += sell_stack;
							}
							else
							{
								Object obj = sold_item as Object;
								if (obj != null && obj.edibility.Value != -300 && Game1.random.NextDouble() < 0.03999999910593033)
								{
									ShopLocation shopLocation = Game1.currentLocation as ShopLocation;
									if (shopLocation != null)
									{
										ISalable sell_back_instance = sold_item.GetSalableInstance();
										if (buyback_item != null)
										{
											this.buyBackItemsToResellTomorrow[buyback_item] = sell_back_instance;
										}
										shopLocation.itemsToStartSellingTomorrow.Add(sell_back_instance as Item);
									}
								}
							}
							if (this.inventory.getItemAt(x, y) == null)
							{
								Game1.playSound("sell", null);
								this.animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
								{
									motion = new Vector2(0f, -0.5f)
								});
							}
						}
					}
				}
				else
				{
					this.heldItem = this.inventory.rightClick(x, y, this.heldItem as Item, true, false);
				}
				for (int j = 0; j < this.forSaleButtons.Count; j++)
				{
					if (this.currentItemIndex + j < this.forSale.Count && this.forSaleButtons[j].containsPoint(x, y))
					{
						int index = this.currentItemIndex + j;
						if (this.forSale[index] != null)
						{
							int toBuy = 1;
							if (this.itemPriceAndStock[this.forSale[index]].Price > 0)
							{
								toBuy = (Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? (Game1.oldKBState.IsKeyDown(Keys.OemTilde) ? 999 : 25) : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / this.itemPriceAndStock[this.forSale[index]].Price), this.itemPriceAndStock[this.forSale[index]].Stock) : 1);
							}
							if (this.canPurchaseCheck != null && !this.canPurchaseCheck(index))
							{
								return;
							}
							if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y))
							{
								this.itemPriceAndStock.Remove(this.forSale[index]);
								this.forSale.RemoveAt(index);
							}
							if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus) && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item, false))
							{
								this.heldItem = null;
								DelayedAction.playSoundAfterDelay("coin", 100, null, null, -1, false);
							}
							this.setScrollBarToCurrentIndex();
						}
						return;
					}
				}
			}
		}

		// Token: 0x06002C6E RID: 11374 RVA: 0x00221900 File Offset: 0x0021FB00
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.hoverText = "";
			this.hoveredItem = null;
			this.hoverPrice = -1;
			this.boldTitleText = "";
			this.upArrow.tryHover(x, y, 0.1f);
			this.downArrow.tryHover(x, y, 0.1f);
			this.scrollBar.tryHover(x, y, 0.1f);
			if (this.scrolling)
			{
				return;
			}
			for (int i = 0; i < this.forSaleButtons.Count; i++)
			{
				if (this.currentItemIndex + i < this.forSale.Count && this.forSaleButtons[i].containsPoint(x, y))
				{
					ISalable item = this.forSale[this.currentItemIndex + i];
					if (this.canPurchaseCheck == null || this.canPurchaseCheck(this.currentItemIndex + i))
					{
						this.hoverText = item.getDescription();
						this.boldTitleText = item.DisplayName;
						if (!this._isStorageShop)
						{
							ItemStockInformation stock;
							this.hoverPrice = ((this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(item, out stock)) ? stock.Price : item.salePrice(false));
						}
						this.hoveredItem = item;
						this.forSaleButtons[i].scale = Math.Min(this.forSaleButtons[i].scale + 0.03f, 1.1f);
					}
				}
				else
				{
					this.forSaleButtons[i].scale = Math.Max(1f, this.forSaleButtons[i].scale - 0.03f);
				}
			}
			if (this.heldItem == null)
			{
				foreach (ClickableComponent c in this.inventory.inventory)
				{
					if (c.containsPoint(x, y))
					{
						Item j = this.inventory.getItemFromClickableComponent(c);
						if (j != null && (this.inventory.highlightMethod == null || this.inventory.highlightMethod(j)))
						{
							if (this._isStorageShop)
							{
								this.hoverText = j.getDescription();
								this.boldTitleText = j.DisplayName;
								this.hoveredItem = j;
							}
							else
							{
								this.hoverText = j.DisplayName + " x" + j.Stack.ToString();
								Object hovered_object = j as Object;
								if (hovered_object != null && hovered_object.needsToBeDonated())
								{
									this.hoverText = this.hoverText + "\n\n" + j.getDescription() + "\n";
								}
								this.hoverPrice = (int)((float)j.sellToStorePrice(-1L) * this.sellPercentage) * j.Stack;
							}
						}
					}
				}
			}
		}

		// Token: 0x06002C6F RID: 11375 RVA: 0x00221C08 File Offset: 0x0021FE08
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.safetyTimer > 0)
			{
				this.safetyTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.poof != null && this.poof.update(time))
			{
				this.poof = null;
			}
			this.repositionTabs();
		}

		// Token: 0x06002C70 RID: 11376 RVA: 0x00221C64 File Offset: 0x0021FE64
		public void drawCurrency(SpriteBatch b)
		{
			if (this._isStorageShop)
			{
				return;
			}
			if (this.currency == 0)
			{
				Game1.dayTimeMoneyBox.drawMoneyBox(b, this.xPositionOnScreen - 36, this.yPositionOnScreen + this.height - this.inventory.height - 12);
			}
		}

		// Token: 0x06002C71 RID: 11377 RVA: 0x00221CB4 File Offset: 0x0021FEB4
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (button == Buttons.RightTrigger || button == Buttons.LeftTrigger)
			{
				ClickableComponent currentlySnappedComponent = this.currentlySnappedComponent;
				if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= 3546)
				{
					int emptySlot = -1;
					for (int i = 0; i < 12; i++)
					{
						this.inventory.inventory[i].upNeighborID = 3546 + this.forSaleButtons.Count - 1;
						if (emptySlot == -1 && this.heldItem != null)
						{
							IList<Item> actualInventory = this.inventory.actualInventory;
							if (actualInventory != null && actualInventory.Count > i && this.inventory.actualInventory[i] == null)
							{
								emptySlot = i;
							}
						}
					}
					this.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
					this.snapCursorToCurrentSnappedComponent();
				}
				else
				{
					this.snapToDefaultClickableComponent();
				}
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002C72 RID: 11378 RVA: 0x00221DA8 File Offset: 0x0021FFA8
		private string getHoveredItemExtraItemIndex()
		{
			ItemStockInformation stock;
			if (this.hoveredItem != null && this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(this.hoveredItem, out stock) && stock.TradeItem != null)
			{
				return stock.TradeItem;
			}
			return null;
		}

		// Token: 0x06002C73 RID: 11379 RVA: 0x00221DEC File Offset: 0x0021FFEC
		private int getHoveredItemExtraItemAmount()
		{
			ItemStockInformation stock;
			if (this.hoveredItem != null && this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(this.hoveredItem, out stock) && stock.TradeItem != null && stock.TradeItemCount != null)
			{
				return stock.TradeItemCount.Value;
			}
			return 5;
		}

		// Token: 0x06002C74 RID: 11380 RVA: 0x00221E40 File Offset: 0x00220040
		public void updatePosition()
		{
			this.width = 1000 + IClickableMenu.borderWidth * 2;
			this.height = 600 + IClickableMenu.borderWidth * 2;
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
			int portraitDrawPosition = this.xPositionOnScreen - 320;
			if ((this.portraitTexture == null && string.IsNullOrEmpty(this.potraitPersonDialogue)) || portraitDrawPosition <= 0 || !Game1.options.showMerchantPortraits)
			{
				this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (1000 + IClickableMenu.borderWidth * 2) / 2;
				this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
			}
		}

		// Token: 0x06002C75 RID: 11381 RVA: 0x00221F34 File Offset: 0x00220134
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			ShopMenu.ShopCachedTheme theme = this.VisualTheme;
			this.updatePosition();
			base.initializeUpperRightCloseButton();
			Game1.player.forceCanMove();
			this.inventory = new InventoryMenu(this.xPositionOnScreen + this.width, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, false, null, new InventoryMenu.highlightThisItem(this.highlightItemToSell), -1, 3, 0, 0, true)
			{
				showGrayedOutSlots = true
			};
			this.inventory.movePosition(-this.inventory.width - 32, 0);
			this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 16, 44, 48), theme.ScrollUpTexture, theme.ScrollUpSourceRect, 4f, false);
			this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), theme.ScrollDownTexture, theme.ScrollDownSourceRect, 4f, false);
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), theme.ScrollBarFrontTexture, theme.ScrollBarFrontSourceRect, 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
			this.forSaleButtons.Clear();
			for (int i = 0; i < 4; i++)
			{
				this.forSaleButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * ((this.height - 256) / 4), this.width - 32, (this.height - 256) / 4 + 4), i.ToString() ?? ""));
			}
			if (this.tabButtons.Count > 0)
			{
				foreach (ClickableComponent clickableComponent in this.forSaleButtons)
				{
					clickableComponent.leftNeighborID = -99998;
				}
			}
			this.repositionTabs();
			foreach (ClickableComponent clickableComponent2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				clickableComponent2.upNeighborID = -99998;
			}
		}

		// Token: 0x06002C76 RID: 11382 RVA: 0x00222230 File Offset: 0x00220430
		public void setItemPriceAndStock(Dictionary<ISalable, ItemStockInformation> new_stock)
		{
			this.itemPriceAndStock = new_stock;
			this.forSale = this.itemPriceAndStock.Keys.ToList<ISalable>();
			this.applyTab();
		}

		// Token: 0x06002C77 RID: 11383 RVA: 0x00222258 File Offset: 0x00220458
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			ShopMenu.ShopCachedTheme theme = this.VisualTheme;
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen + this.width - this.inventory.width - 32 - 24, this.yPositionOnScreen + this.height - 256 + 40, this.inventory.width + 56, this.height - 448 + 20, Color.White, 4f, true, -1f);
			IClickableMenu.drawTextureBox(b, theme.WindowBorderTexture, theme.WindowBorderSourceRect, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height - 256 + 32 + 4, Color.White, 4f, true, -1f);
			this.drawCurrency(b);
			for (int i = 0; i < this.forSaleButtons.Count; i++)
			{
				ClickableComponent forSaleButton = this.forSaleButtons[i];
				if (this.currentItemIndex + i < this.forSale.Count)
				{
					bool failedCanPurchaseCheck = this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + i);
					ISalable item = this.forSale[this.currentItemIndex + i];
					ItemStockInformation stock = this.itemPriceAndStock[item];
					StackDrawType stackDrawType = this.GetStackDrawType(stock, item);
					string displayName = item.DisplayName;
					IClickableMenu.drawTextureBox(b, theme.ItemRowBackgroundTexture, theme.ItemRowBackgroundSourceRect, forSaleButton.bounds.X, forSaleButton.bounds.Y, forSaleButton.bounds.Width, forSaleButton.bounds.Height, (forSaleButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.scrolling) ? theme.ItemRowBackgroundHoverColor : Color.White, 4f, false, -1f);
					if (item.Stack > 1)
					{
						displayName = displayName + " x" + item.Stack.ToString();
					}
					if (item.ShouldDrawIcon())
					{
						b.Draw(theme.ItemIconBackgroundTexture, new Vector2((float)(forSaleButton.bounds.X + 32 - 12), (float)(forSaleButton.bounds.Y + 24 - 4)), new Rectangle?(theme.ItemIconBackgroundSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						Vector2 drawPos = new Vector2((float)(forSaleButton.bounds.X + 32 - 8), (float)(forSaleButton.bounds.Y + 24));
						Color color = Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f);
						int drawnStack = stock.Stock;
						item.drawInMenu(b, drawPos, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, true);
						if (drawnStack != 2147483647 && this.ShopId != "ClintUpgrade" && ((stackDrawType == StackDrawType.Draw && drawnStack > 1) || stackDrawType == StackDrawType.Draw_OneInclusive))
						{
							Utility.drawTinyDigits(drawnStack, b, drawPos + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(drawnStack, 3f) + 3), 47f), 3f, 1f, color);
						}
						if (this.buyBackItems.Contains(item))
						{
							b.Draw(Game1.mouseCursors2, new Vector2((float)(forSaleButton.bounds.X + 32 - 8), (float)(forSaleButton.bounds.Y + 24)), new Rectangle?(new Rectangle(64, 240, 16, 16)), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
						}
						string formattedDisplayName = displayName;
						bool hasPrice = stock.Price > 0;
						if (SpriteText.getWidthOfString(formattedDisplayName, 999999) > this.width - (hasPrice ? (150 + SpriteText.getWidthOfString(stock.Price.ToString() + " ", 999999)) : 100) && formattedDisplayName.Length > (hasPrice ? 27 : 37))
						{
							formattedDisplayName = formattedDisplayName.Substring(0, hasPrice ? 27 : 37);
							formattedDisplayName += "...";
						}
						SpriteText.drawString(b, formattedDisplayName, forSaleButton.bounds.X + 96 + 8, forSaleButton.bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, false, -1, "", theme.ItemRowTextColor, SpriteText.ScrollTextAlignment.Left);
					}
					else
					{
						SpriteText.drawString(b, displayName, forSaleButton.bounds.X + 32 + 8, forSaleButton.bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, false, -1, "", theme.ItemRowTextColor, SpriteText.ScrollTextAlignment.Left);
					}
					int right = forSaleButton.bounds.Right;
					int tradeIconDrawY = forSaleButton.bounds.Y + 28 - 4;
					int tradeTextDrawY = forSaleButton.bounds.Y + 44;
					if (stock.Price > 0)
					{
						SpriteText.drawString(b, stock.Price.ToString() + " ", right - SpriteText.getWidthOfString(stock.Price.ToString() + " ", 999999) - 60, forSaleButton.bounds.Y + 28, 999999, -1, 999999, (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= stock.Price && !failedCanPurchaseCheck) ? 1f : 0.5f, 0.88f, false, -1, "", theme.ItemRowTextColor, SpriteText.ScrollTextAlignment.Left);
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(forSaleButton.bounds.Right - 52), (float)(forSaleButton.bounds.Y + 40 - 4)), new Rectangle(193 + this.currency * 9, 373, 9, 10), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, Vector2.Zero, 4f, false, -1f, -1, -1, (!failedCanPurchaseCheck) ? 0.35f : 0f);
						right -= SpriteText.getWidthOfString(stock.Price.ToString() + " ", 999999) + 96;
						tradeIconDrawY = forSaleButton.bounds.Y + 20;
						tradeTextDrawY = forSaleButton.bounds.Y + 28;
					}
					if (stock.TradeItem != null)
					{
						int requiredItemCount = 5;
						string requiredItem = stock.TradeItem;
						if (requiredItem != null && stock.TradeItemCount != null)
						{
							requiredItemCount = stock.TradeItemCount.Value;
						}
						bool hasEnoughToTrade = this.HasTradeItem(requiredItem, requiredItemCount);
						if (this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + i))
						{
							hasEnoughToTrade = false;
						}
						float textWidth = (float)SpriteText.getWidthOfString("x" + requiredItemCount.ToString(), 999999);
						ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(requiredItem);
						Texture2D texture = dataOrErrorItem.GetTexture();
						Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
						Utility.drawWithShadow(b, texture, new Vector2((float)(right - 88) - textWidth, (float)tradeIconDrawY), sourceRect, Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, -1f, false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
						SpriteText.drawString(b, "x" + requiredItemCount.ToString(), right - (int)textWidth - 16, tradeTextDrawY, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, false, -1, "", theme.ItemRowTextColor, SpriteText.ScrollTextAlignment.Left);
					}
				}
			}
			if (this.IsOutOfStock())
			{
				string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583");
				SpriteText.drawString(b, text, this.xPositionOnScreen + this.width / 2 - SpriteText.getWidthOfString(text, 999999) / 2, this.yPositionOnScreen + this.height / 2 - 128, 999999, -1, 999999, 1f, 0.88f, false, -1, "", (theme != null) ? theme.ItemRowTextColor : null, SpriteText.ScrollTextAlignment.Left);
			}
			this.inventory.draw(b);
			for (int j = this.animations.Count - 1; j >= 0; j--)
			{
				if (this.animations[j].update(Game1.currentGameTime))
				{
					this.animations.RemoveAt(j);
				}
				else
				{
					this.animations[j].draw(b, true, 0, 0, 1f);
				}
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.poof;
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			foreach (ShopMenu.ShopTabClickableTextureComponent shopTabClickableTextureComponent in this.tabButtons)
			{
				shopTabClickableTextureComponent.draw(b);
			}
			if (this.forSale.Count > 4)
			{
				IClickableMenu.drawTextureBox(b, theme.ScrollBarBackTexture, theme.ScrollBarBackSourceRect, this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true, -1f);
				this.scrollBar.draw(b);
			}
			if (this.hoverText != "")
			{
				Item actualItem = this.hoveredItem as Item;
				ISalable salable = this.hoveredItem;
				if (salable != null && salable.IsRecipe)
				{
					IClickableMenu.drawToolTip(b, " ", this.boldTitleText, actualItem, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), new CraftingRecipe(((actualItem != null) ? actualItem.BaseName : null) ?? this.hoveredItem.Name), (this.hoverPrice > 0) ? this.hoverPrice : -1, null);
				}
				else
				{
					IClickableMenu.drawToolTip(b, this.hoverText, this.boldTitleText, actualItem, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), null, (this.hoverPrice > 0) ? this.hoverPrice : -1, null);
				}
			}
			ISalable salable2 = this.heldItem;
			if (salable2 != null)
			{
				salable2.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
			}
			base.draw(b);
			int portrait_draw_position = this.xPositionOnScreen - 320;
			if (portrait_draw_position > 0 && Game1.options.showMerchantPortraits)
			{
				if (this.portraitTexture != null)
				{
					Utility.drawWithShadow(b, theme.PortraitBackgroundTexture, new Vector2((float)portrait_draw_position, (float)this.yPositionOnScreen), theme.PortraitBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, false, 0.91f, -1, -1, 0.35f);
					if (this.portraitTexture != null)
					{
						b.Draw(this.portraitTexture, new Vector2((float)(portrait_draw_position + 20), (float)(this.yPositionOnScreen + 20)), new Rectangle?(new Rectangle(0, 0, 64, 64)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
					}
				}
				if (this.potraitPersonDialogue != null)
				{
					portrait_draw_position = this.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(this.potraitPersonDialogue).X - 64;
					if (portrait_draw_position > 0)
					{
						IClickableMenu.drawHoverText(b, this.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, portrait_draw_position, this.yPositionOnScreen + ((this.portraitTexture != null) ? 312 : 0), 1f, null, null, theme.DialogueBackgroundTexture, new Rectangle?(theme.DialogueBackgroundSourceRect), theme.DialogueColor, theme.DialogueShadowColor, 1f, -1, -1);
					}
				}
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x06002C78 RID: 11384 RVA: 0x00222F04 File Offset: 0x00221104
		public StackDrawType GetStackDrawType(ItemStockInformation stockInfo, ISalable item)
		{
			if (item.IsRecipe)
			{
				return StackDrawType.Hide;
			}
			if (stockInfo.StackDrawType != null)
			{
				return stockInfo.StackDrawType.Value;
			}
			if (stockInfo.Stock == 2147483647)
			{
				return StackDrawType.HideButShowQuality;
			}
			if (this.DefaultStackDrawType != null)
			{
				return this.DefaultStackDrawType.Value;
			}
			ShopData shopData = this.ShopData;
			if (shopData != null && shopData.StackSizeVisibility != null)
			{
				StackSizeVisibility? stackSizeVisibility = this.ShopData.StackSizeVisibility;
				if (stackSizeVisibility != null)
				{
					StackSizeVisibility valueOrDefault = stackSizeVisibility.GetValueOrDefault();
					if (valueOrDefault == StackSizeVisibility.Hide)
					{
						return StackDrawType.HideButShowQuality;
					}
					if (valueOrDefault == StackSizeVisibility.ShowIfMultiple)
					{
						return StackDrawType.Draw;
					}
				}
				return StackDrawType.Draw_OneInclusive;
			}
			if (!this._isStorageShop)
			{
				return StackDrawType.Draw_OneInclusive;
			}
			return StackDrawType.Draw;
		}

		// Token: 0x04001E09 RID: 7689
		public const int region_shopButtonModifier = 3546;

		// Token: 0x04001E0A RID: 7690
		public const int region_upArrow = 97865;

		// Token: 0x04001E0B RID: 7691
		public const int region_downArrow = 97866;

		// Token: 0x04001E0C RID: 7692
		public const int region_tabStartIndex = 99999;

		// Token: 0x04001E0D RID: 7693
		public const int infiniteStock = 2147483647;

		// Token: 0x04001E0E RID: 7694
		public const int itemsPerPage = 4;

		// Token: 0x04001E0F RID: 7695
		public const int numberRequiredForExtraItemTrade = 5;

		// Token: 0x04001E10 RID: 7696
		public string hoverText = "";

		// Token: 0x04001E11 RID: 7697
		public string boldTitleText = "";

		// Token: 0x04001E12 RID: 7698
		public string openMenuSound = "dwop";

		// Token: 0x04001E13 RID: 7699
		public string purchaseSound = "purchaseClick";

		// Token: 0x04001E14 RID: 7700
		public string purchaseRepeatSound = "purchaseRepeat";

		// Token: 0x04001E15 RID: 7701
		public string ShopId;

		// Token: 0x04001E16 RID: 7702
		public ShopData ShopData;

		// Token: 0x04001E18 RID: 7704
		public InventoryMenu inventory;

		// Token: 0x04001E19 RID: 7705
		public ISalable heldItem;

		// Token: 0x04001E1A RID: 7706
		public ISalable hoveredItem;

		// Token: 0x04001E1B RID: 7707
		public StackDrawType? DefaultStackDrawType;

		// Token: 0x04001E1C RID: 7708
		private TemporaryAnimatedSprite poof;

		// Token: 0x04001E1D RID: 7709
		private Rectangle scrollBarRunner;

		// Token: 0x04001E1E RID: 7710
		public List<ISalable> forSale = new List<ISalable>();

		// Token: 0x04001E1F RID: 7711
		public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

		// Token: 0x04001E20 RID: 7712
		public List<int> categoriesToSellHere = new List<int>();

		// Token: 0x04001E21 RID: 7713
		public List<List<string>> tagsToSellHere = new List<List<string>>();

		// Token: 0x04001E22 RID: 7714
		public Dictionary<ISalable, ItemStockInformation> itemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();

		// Token: 0x04001E23 RID: 7715
		private float sellPercentage = 1f;

		// Token: 0x04001E24 RID: 7716
		private TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

		// Token: 0x04001E25 RID: 7717
		public int hoverPrice = -1;

		// Token: 0x04001E26 RID: 7718
		public int currentItemIndex;

		// Token: 0x04001E27 RID: 7719
		public int currency;

		// Token: 0x04001E28 RID: 7720
		public ClickableTextureComponent upArrow;

		// Token: 0x04001E29 RID: 7721
		public ClickableTextureComponent downArrow;

		// Token: 0x04001E2A RID: 7722
		public ClickableTextureComponent scrollBar;

		// Token: 0x04001E2B RID: 7723
		public Texture2D portraitTexture;

		// Token: 0x04001E2C RID: 7724
		public string potraitPersonDialogue;

		// Token: 0x04001E2D RID: 7725
		public object source;

		// Token: 0x04001E2E RID: 7726
		private bool scrolling;

		// Token: 0x04001E2F RID: 7727
		public ShopMenu.OnPurchaseDelegate onPurchase;

		// Token: 0x04001E30 RID: 7728
		public Func<ISalable, bool> onSell;

		// Token: 0x04001E31 RID: 7729
		public Func<int, bool> canPurchaseCheck;

		// Token: 0x04001E32 RID: 7730
		public List<ShopMenu.ShopTabClickableTextureComponent> tabButtons = new List<ShopMenu.ShopTabClickableTextureComponent>();

		// Token: 0x04001E33 RID: 7731
		protected int currentTab;

		// Token: 0x04001E34 RID: 7732
		protected bool _isStorageShop;

		// Token: 0x04001E35 RID: 7733
		public bool readOnly;

		// Token: 0x04001E36 RID: 7734
		public HashSet<ISalable> buyBackItems = new HashSet<ISalable>();

		// Token: 0x04001E37 RID: 7735
		public Dictionary<ISalable, ISalable> buyBackItemsToResellTomorrow = new Dictionary<ISalable, ISalable>();

		// Token: 0x04001E38 RID: 7736
		public int safetyTimer = 250;

		// Token: 0x02000630 RID: 1584
		// (Invoke) Token: 0x06004476 RID: 17526
		public delegate bool OnPurchaseDelegate(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock);

		// Token: 0x02000631 RID: 1585
		public class ShopCachedTheme
		{
			// Token: 0x1700050B RID: 1291
			// (get) Token: 0x06004479 RID: 17529 RVA: 0x0031C959 File Offset: 0x0031AB59
			public ShopThemeData ThemeData { get; }

			// Token: 0x1700050C RID: 1292
			// (get) Token: 0x0600447A RID: 17530 RVA: 0x0031C961 File Offset: 0x0031AB61
			public Texture2D WindowBorderTexture { get; }

			// Token: 0x1700050D RID: 1293
			// (get) Token: 0x0600447B RID: 17531 RVA: 0x0031C969 File Offset: 0x0031AB69
			public Rectangle WindowBorderSourceRect { get; }

			// Token: 0x1700050E RID: 1294
			// (get) Token: 0x0600447C RID: 17532 RVA: 0x0031C971 File Offset: 0x0031AB71
			public Texture2D PortraitBackgroundTexture { get; }

			// Token: 0x1700050F RID: 1295
			// (get) Token: 0x0600447D RID: 17533 RVA: 0x0031C979 File Offset: 0x0031AB79
			public Rectangle PortraitBackgroundSourceRect { get; }

			// Token: 0x17000510 RID: 1296
			// (get) Token: 0x0600447E RID: 17534 RVA: 0x0031C981 File Offset: 0x0031AB81
			public Texture2D DialogueBackgroundTexture { get; }

			// Token: 0x17000511 RID: 1297
			// (get) Token: 0x0600447F RID: 17535 RVA: 0x0031C989 File Offset: 0x0031AB89
			public Rectangle DialogueBackgroundSourceRect { get; }

			// Token: 0x17000512 RID: 1298
			// (get) Token: 0x06004480 RID: 17536 RVA: 0x0031C991 File Offset: 0x0031AB91
			public Color? DialogueColor { get; }

			// Token: 0x17000513 RID: 1299
			// (get) Token: 0x06004481 RID: 17537 RVA: 0x0031C999 File Offset: 0x0031AB99
			public Color? DialogueShadowColor { get; }

			// Token: 0x17000514 RID: 1300
			// (get) Token: 0x06004482 RID: 17538 RVA: 0x0031C9A1 File Offset: 0x0031ABA1
			public Texture2D ItemRowBackgroundTexture { get; }

			// Token: 0x17000515 RID: 1301
			// (get) Token: 0x06004483 RID: 17539 RVA: 0x0031C9A9 File Offset: 0x0031ABA9
			public Rectangle ItemRowBackgroundSourceRect { get; }

			// Token: 0x17000516 RID: 1302
			// (get) Token: 0x06004484 RID: 17540 RVA: 0x0031C9B1 File Offset: 0x0031ABB1
			public Color ItemRowBackgroundHoverColor { get; }

			// Token: 0x17000517 RID: 1303
			// (get) Token: 0x06004485 RID: 17541 RVA: 0x0031C9B9 File Offset: 0x0031ABB9
			public Color? ItemRowTextColor { get; }

			// Token: 0x17000518 RID: 1304
			// (get) Token: 0x06004486 RID: 17542 RVA: 0x0031C9C1 File Offset: 0x0031ABC1
			public Texture2D ItemIconBackgroundTexture { get; }

			// Token: 0x17000519 RID: 1305
			// (get) Token: 0x06004487 RID: 17543 RVA: 0x0031C9C9 File Offset: 0x0031ABC9
			public Rectangle ItemIconBackgroundSourceRect { get; }

			// Token: 0x1700051A RID: 1306
			// (get) Token: 0x06004488 RID: 17544 RVA: 0x0031C9D1 File Offset: 0x0031ABD1
			public Texture2D ScrollUpTexture { get; }

			// Token: 0x1700051B RID: 1307
			// (get) Token: 0x06004489 RID: 17545 RVA: 0x0031C9D9 File Offset: 0x0031ABD9
			public Rectangle ScrollUpSourceRect { get; }

			// Token: 0x1700051C RID: 1308
			// (get) Token: 0x0600448A RID: 17546 RVA: 0x0031C9E1 File Offset: 0x0031ABE1
			public Texture2D ScrollDownTexture { get; }

			// Token: 0x1700051D RID: 1309
			// (get) Token: 0x0600448B RID: 17547 RVA: 0x0031C9E9 File Offset: 0x0031ABE9
			public Rectangle ScrollDownSourceRect { get; }

			// Token: 0x1700051E RID: 1310
			// (get) Token: 0x0600448C RID: 17548 RVA: 0x0031C9F1 File Offset: 0x0031ABF1
			public Texture2D ScrollBarFrontTexture { get; }

			// Token: 0x1700051F RID: 1311
			// (get) Token: 0x0600448D RID: 17549 RVA: 0x0031C9F9 File Offset: 0x0031ABF9
			public Rectangle ScrollBarFrontSourceRect { get; }

			// Token: 0x17000520 RID: 1312
			// (get) Token: 0x0600448E RID: 17550 RVA: 0x0031CA01 File Offset: 0x0031AC01
			public Texture2D ScrollBarBackTexture { get; }

			// Token: 0x17000521 RID: 1313
			// (get) Token: 0x0600448F RID: 17551 RVA: 0x0031CA09 File Offset: 0x0031AC09
			public Rectangle ScrollBarBackSourceRect { get; }

			// Token: 0x06004490 RID: 17552 RVA: 0x0031CA14 File Offset: 0x0031AC14
			public ShopCachedTheme(ShopThemeData theme)
			{
				this.ThemeData = theme;
				this.WindowBorderTexture = this.LoadThemeTexture((theme != null) ? theme.WindowBorderTexture : null, Game1.mouseCursors);
				this.WindowBorderSourceRect = (((theme != null) ? theme.WindowBorderSourceRect : null) ?? new Rectangle(384, 373, 18, 18));
				this.PortraitBackgroundTexture = this.LoadThemeTexture((theme != null) ? theme.PortraitBackgroundTexture : null, Game1.mouseCursors);
				this.PortraitBackgroundSourceRect = (((theme != null) ? theme.PortraitBackgroundSourceRect : null) ?? new Rectangle(603, 414, 74, 74));
				this.DialogueBackgroundTexture = this.LoadThemeTexture((theme != null) ? theme.DialogueBackgroundTexture : null, Game1.menuTexture);
				this.DialogueBackgroundSourceRect = (((theme != null) ? theme.DialogueBackgroundSourceRect : null) ?? new Rectangle(0, 256, 60, 60));
				this.DialogueColor = Utility.StringToColor((theme != null) ? theme.DialogueColor : null);
				this.DialogueShadowColor = Utility.StringToColor((theme != null) ? theme.DialogueShadowColor : null);
				this.ItemRowBackgroundTexture = this.LoadThemeTexture((theme != null) ? theme.ItemRowBackgroundTexture : null, Game1.mouseCursors);
				this.ItemRowBackgroundSourceRect = (((theme != null) ? theme.ItemRowBackgroundSourceRect : null) ?? new Rectangle(384, 396, 15, 15));
				this.ItemRowBackgroundHoverColor = (Utility.StringToColor((theme != null) ? theme.ItemRowBackgroundHoverColor : null) ?? Color.Wheat);
				this.ItemRowTextColor = Utility.StringToColor((theme != null) ? theme.ItemRowTextColor : null);
				this.ItemIconBackgroundTexture = this.LoadThemeTexture((theme != null) ? theme.ItemIconBackgroundTexture : null, Game1.mouseCursors);
				this.ItemIconBackgroundSourceRect = (((theme != null) ? theme.ItemIconBackgroundSourceRect : null) ?? new Rectangle(296, 363, 18, 18));
				this.ScrollUpTexture = this.LoadThemeTexture((theme != null) ? theme.ScrollUpTexture : null, Game1.mouseCursors);
				this.ScrollUpSourceRect = (((theme != null) ? theme.ScrollUpSourceRect : null) ?? new Rectangle(421, 459, 11, 12));
				this.ScrollDownTexture = this.LoadThemeTexture((theme != null) ? theme.ScrollDownTexture : null, Game1.mouseCursors);
				this.ScrollDownSourceRect = (((theme != null) ? theme.ScrollDownSourceRect : null) ?? new Rectangle(421, 472, 11, 12));
				this.ScrollBarFrontTexture = this.LoadThemeTexture((theme != null) ? theme.ScrollBarFrontTexture : null, Game1.mouseCursors);
				this.ScrollBarFrontSourceRect = (((theme != null) ? theme.ScrollBarFrontSourceRect : null) ?? new Rectangle(435, 463, 6, 10));
				this.ScrollBarBackTexture = this.LoadThemeTexture((theme != null) ? theme.ScrollBarBackTexture : null, Game1.mouseCursors);
				this.ScrollBarBackSourceRect = (((theme != null) ? theme.ScrollBarBackSourceRect : null) ?? new Rectangle(403, 383, 6, 6));
			}

			// Token: 0x06004491 RID: 17553 RVA: 0x0031CDE0 File Offset: 0x0031AFE0
			private Texture2D LoadThemeTexture(string customTextureName, Texture2D defaultTexture)
			{
				if (customTextureName == null || !Game1.content.DoesAssetExist<Texture2D>(customTextureName))
				{
					return defaultTexture;
				}
				return Game1.content.Load<Texture2D>(customTextureName);
			}
		}

		// Token: 0x02000632 RID: 1586
		public class ShopTabClickableTextureComponent : ClickableTextureComponent
		{
			// Token: 0x06004492 RID: 17554 RVA: 0x0031CE00 File Offset: 0x0031B000
			public ShopTabClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(name, bounds, label, hoverText, texture, sourceRect, scale, drawShadow)
			{
			}

			// Token: 0x06004493 RID: 17555 RVA: 0x0031CE20 File Offset: 0x0031B020
			public ShopTabClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(bounds, texture, sourceRect, scale, drawShadow)
			{
			}

			// Token: 0x04002EC5 RID: 11973
			public Func<ISalable, bool> Filter;
		}
	}
}
