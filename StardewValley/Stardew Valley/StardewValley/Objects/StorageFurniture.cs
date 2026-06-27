using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects
{
	// Token: 0x020001B8 RID: 440
	[XmlInclude(typeof(FishTankFurniture))]
	public class StorageFurniture : Furniture
	{
		// Token: 0x06001F66 RID: 8038 RVA: 0x00168591 File Offset: 0x00166791
		public StorageFurniture()
		{
		}

		// Token: 0x06001F67 RID: 8039 RVA: 0x001685AF File Offset: 0x001667AF
		public StorageFurniture(string itemId, Vector2 tile, int initialRotations) : base(itemId, tile, initialRotations)
		{
		}

		// Token: 0x06001F68 RID: 8040 RVA: 0x001685D0 File Offset: 0x001667D0
		public StorageFurniture(string itemId, Vector2 tile) : base(itemId, tile)
		{
		}

		// Token: 0x06001F69 RID: 8041 RVA: 0x001685F0 File Offset: 0x001667F0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.heldItems, "heldItems").AddField(this.mutex.NetFields, "mutex.NetFields");
		}

		// Token: 0x06001F6A RID: 8042 RVA: 0x00168624 File Offset: 0x00166824
		public override bool canBeRemoved(Farmer who)
		{
			return !this.mutex.IsLocked() && base.canBeRemoved(who);
		}

		// Token: 0x06001F6B RID: 8043 RVA: 0x0016863C File Offset: 0x0016683C
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			this.mutex.RequestLock(new Action(this.ShowMenu), null);
			return true;
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x0016865D File Offset: 0x0016685D
		public virtual void ShowMenu()
		{
			this.ShowShopMenu();
		}

		// Token: 0x06001F6D RID: 8045 RVA: 0x00168668 File Offset: 0x00166868
		public virtual void ShowChestMenu()
		{
			ItemGrabMenu activeClickableMenu;
			(activeClickableMenu = new ItemGrabMenu(this.heldItems, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.GrabItemFromInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.GrabItemFromChest), false, true, true, true, true, 1, this, -1, this, ItemExitBehavior.ReturnToPlayer, false)).behaviorBeforeCleanup = delegate(IClickableMenu menu)
			{
				this.mutex.ReleaseLock();
				this.OnMenuClose();
			};
			Game1.activeClickableMenu = activeClickableMenu;
			Game1.playSound("dwop", null);
		}

		// Token: 0x06001F6E RID: 8046 RVA: 0x001686E0 File Offset: 0x001668E0
		public virtual void GrabItemFromInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item tmp = this.AddItem(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				tmp = who.addItemToInventory(tmp);
			}
			this.ClearNulls();
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
			this.ShowChestMenu();
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x00168776 File Offset: 0x00166976
		public virtual bool HighlightItems(Item item)
		{
			return InventoryMenu.highlightAllItems(item);
		}

		// Token: 0x06001F70 RID: 8048 RVA: 0x0016877E File Offset: 0x0016697E
		public virtual void GrabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				this.heldItems.Remove(item);
				this.ClearNulls();
				this.ShowChestMenu();
			}
		}

		// Token: 0x06001F71 RID: 8049 RVA: 0x001687A2 File Offset: 0x001669A2
		public virtual void ClearNulls()
		{
			this.heldItems.RemoveWhere((Item slot) => slot == null);
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x001687D0 File Offset: 0x001669D0
		public virtual Item AddItem(Item item)
		{
			item.resetState();
			this.ClearNulls();
			for (int i = 0; i < this.heldItems.Count; i++)
			{
				if (this.heldItems[i] != null && this.heldItems[i].canStackWith(item))
				{
					int toRemove = item.Stack - this.heldItems[i].addToStack(item);
					if (item.ConsumeStack(toRemove) == null)
					{
						return null;
					}
				}
			}
			if (this.heldItems.Count < 36)
			{
				this.heldItems.Add(item);
				return null;
			}
			return item;
		}

		// Token: 0x06001F73 RID: 8051 RVA: 0x00168864 File Offset: 0x00166A64
		public virtual void ShowShopMenu()
		{
			List<Item> list = this.heldItems.ToList<Item>();
			list.Sort(new Comparison<Item>(this.SortItems));
			Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
			foreach (Item item in list)
			{
				contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None, null, null, null, null);
			}
			ShopMenu shopMenu;
			(shopMenu = new ShopMenu(this.GetShopMenuContext(), contents, 0, null, new ShopMenu.OnPurchaseDelegate(this.onDresserItemWithdrawn), new Func<ISalable, bool>(this.onDresserItemDeposited), true)).source = this;
			ShopMenu activeClickableMenu;
			(activeClickableMenu = shopMenu).behaviorBeforeCleanup = delegate(IClickableMenu menu)
			{
				this.mutex.ReleaseLock();
				this.OnMenuClose();
			};
			Game1.activeClickableMenu = activeClickableMenu;
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x0016893C File Offset: 0x00166B3C
		public virtual void OnMenuClose()
		{
		}

		// Token: 0x06001F75 RID: 8053 RVA: 0x0016893E File Offset: 0x00166B3E
		public virtual string GetShopMenuContext()
		{
			return "Dresser";
		}

		// Token: 0x06001F76 RID: 8054 RVA: 0x00168945 File Offset: 0x00166B45
		public override bool canBeTrashed()
		{
			return this.heldItems.Count <= 0 && base.canBeTrashed();
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x0016895D File Offset: 0x00166B5D
		public override void DayUpdate()
		{
			base.DayUpdate();
			this.mutex.ReleaseLock();
		}

		// Token: 0x06001F78 RID: 8056 RVA: 0x00168970 File Offset: 0x00166B70
		protected override Item GetOneNew()
		{
			return new StorageFurniture(base.ItemId, this.tileLocation.Value);
		}

		// Token: 0x06001F79 RID: 8057 RVA: 0x00168988 File Offset: 0x00166B88
		public virtual int SortItems(Item a, Item b)
		{
			if (a.Category != b.Category)
			{
				return a.Category.CompareTo(b.Category);
			}
			Clothing clothingA = a as Clothing;
			if (clothingA != null)
			{
				Clothing clothingB = b as Clothing;
				if (clothingB != null && clothingA.clothesType.Value != clothingB.clothesType.Value)
				{
					return clothingA.clothesType.Value.CompareTo(clothingB.clothesType.Value);
				}
			}
			return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x00168A24 File Offset: 0x00166C24
		public virtual bool onDresserItemWithdrawn(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
		{
			Item item = salable as Item;
			if (item != null)
			{
				this.heldItems.Remove(item);
			}
			return false;
		}

		// Token: 0x06001F7B RID: 8059 RVA: 0x00168A4C File Offset: 0x00166C4C
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (environment != null)
			{
				this.mutex.Update(environment);
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x06001F7C RID: 8060 RVA: 0x00168A78 File Offset: 0x00166C78
		public virtual bool onDresserItemDeposited(ISalable deposited_salable)
		{
			Item depositedItem = deposited_salable as Item;
			if (depositedItem != null)
			{
				this.heldItems.Add(depositedItem);
				if (Game1.activeClickableMenu is ShopMenu)
				{
					Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
					List<Item> list = this.heldItems.ToList<Item>();
					list.Sort(new Comparison<Item>(this.SortItems));
					foreach (Item item in list)
					{
						contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None, null, null, null, null);
					}
					(Game1.activeClickableMenu as ShopMenu).setItemPriceAndStock(contents);
					Game1.playSound("dwop", null);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001F7D RID: 8061 RVA: 0x00168B5C File Offset: 0x00166D5C
		public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
		{
			return base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToList<Item>(this.heldItems, handler, getPath, false, null);
		}

		// Token: 0x04001358 RID: 4952
		[XmlElement("heldItems")]
		public readonly NetObjectList<Item> heldItems = new NetObjectList<Item>();

		// Token: 0x04001359 RID: 4953
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();
	}
}
