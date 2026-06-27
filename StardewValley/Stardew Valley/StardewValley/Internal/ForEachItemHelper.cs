using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.SpecialOrders;

namespace StardewValley.Internal
{
	// Token: 0x0200030F RID: 783
	public static class ForEachItemHelper
	{
		// Token: 0x06003410 RID: 13328 RVA: 0x0029A498 File Offset: 0x00298698
		public static bool ForEachItemInWorld(ForEachItemDelegate handler)
		{
			bool canContinue = true;
			Utility.ForEachLocation((GameLocation location) => canContinue = ForEachItemHelper.ForEachItemInLocation(location, handler), true, false);
			if (!canContinue)
			{
				return false;
			}
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ForEachItemHelper.<>c__DisplayClass0_1 CS$<>8__locals2 = new ForEachItemHelper.<>c__DisplayClass0_1();
					CS$<>8__locals2.farmer = enumerator.Current;
					ForEachItemHelper.<>c__DisplayClass0_2 CS$<>8__locals3 = new ForEachItemHelper.<>c__DisplayClass0_2();
					CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals2;
					CS$<>8__locals3.toolIndex = CS$<>8__locals3.CS$<>8__locals1.farmer.CurrentToolIndex;
					if (!ForEachItemHelper.ApplyToList<Item>(CS$<>8__locals3.CS$<>8__locals1.farmer.Items, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), true, new Action<Item, Item, int>(CS$<>8__locals3.<ForEachItemInWorld>g__OnChangedItemSlot|3)) || !ForEachItemHelper.ApplyToField<Clothing>(CS$<>8__locals3.CS$<>8__locals1.farmer.shirtItem, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToField<Clothing>(CS$<>8__locals3.CS$<>8__locals1.farmer.pantsItem, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToField<Boots>(CS$<>8__locals3.CS$<>8__locals1.farmer.boots, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToField<Hat>(CS$<>8__locals3.CS$<>8__locals1.farmer.hat, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToField<Ring>(CS$<>8__locals3.CS$<>8__locals1.farmer.leftRing, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToField<Ring>(CS$<>8__locals3.CS$<>8__locals1.farmer.rightRing, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), new Action<Item, Item>(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__OnChangedEquipment|2)) || !ForEachItemHelper.ApplyToItem<Item>(CS$<>8__locals3.CS$<>8__locals1.farmer.recoveredItem, handler, delegate
					{
						CS$<>8__locals3.CS$<>8__locals1.farmer.recoveredItem = null;
					}, delegate(Item newItem)
					{
						CS$<>8__locals3.CS$<>8__locals1.farmer.recoveredItem = ForEachItemHelper.PrepareForReplaceWith<Item>(CS$<>8__locals3.CS$<>8__locals1.farmer.recoveredItem, newItem);
					}, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4)) || !ForEachItemHelper.ApplyToField<Tool>(CS$<>8__locals3.CS$<>8__locals1.farmer.toolBeingUpgraded, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), null) || !ForEachItemHelper.ApplyToList<Item>(CS$<>8__locals3.CS$<>8__locals1.farmer.itemsLostLastDeath, handler, new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals1.<ForEachItemInWorld>g__GetParentPath|4), false, null))
					{
						return false;
					}
				}
			}
			if (!ForEachItemHelper.ApplyToList<Item>(Game1.player.team.returnedDonations, handler, new GetForEachItemPathDelegate(ForEachItemHelper.<ForEachItemInWorld>g__GetParentPathForTeam|0_0), false, null))
			{
				return false;
			}
			using (NetDictionary<string, Inventory, NetRef<Inventory>, SerializableDictionary<string, Inventory>, NetStringDictionary<Inventory, NetRef<Inventory>>>.ValuesCollection.Enumerator enumerator2 = Game1.player.team.globalInventories.Values.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (!ForEachItemHelper.ApplyToList<Item>(enumerator2.Current, handler, new GetForEachItemPathDelegate(ForEachItemHelper.<ForEachItemInWorld>g__GetParentPathForTeam|0_0), false, null))
					{
						return false;
					}
				}
			}
			using (NetList<SpecialOrder, NetRef<SpecialOrder>>.Enumerator enumerator3 = Game1.player.team.specialOrders.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					SpecialOrder order = enumerator3.Current;
					if (!ForEachItemHelper.ApplyToList<Item>(order.donatedItems, handler, () => ForEachItemHelper.CombinePath(new GetForEachItemPathDelegate(ForEachItemHelper.<ForEachItemInWorld>g__GetParentPathForTeam|0_0), new object[]
					{
						Game1.player.team.specialOrders,
						order
					}), false, null))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06003411 RID: 13329 RVA: 0x0029A90C File Offset: 0x00298B0C
		public static bool ForEachItemInLocation(GameLocation location, ForEachItemDelegate handler)
		{
			ForEachItemHelper.<>c__DisplayClass1_0 CS$<>8__locals1 = new ForEachItemHelper.<>c__DisplayClass1_0();
			CS$<>8__locals1.location = location;
			if (CS$<>8__locals1.location == null)
			{
				return true;
			}
			if (!ForEachItemHelper.ApplyToList<Furniture>(CS$<>8__locals1.location.furniture, handler, new GetForEachItemPathDelegate(CS$<>8__locals1.<ForEachItemInLocation>g__GetLocationPath|0), false, null))
			{
				return false;
			}
			using (List<NPC>.Enumerator enumerator = CS$<>8__locals1.location.characters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ForEachItemHelper.<>c__DisplayClass1_1 CS$<>8__locals2 = new ForEachItemHelper.<>c__DisplayClass1_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.character = enumerator.Current;
					Child child = CS$<>8__locals2.character as Child;
					if (child == null)
					{
						Horse horse = CS$<>8__locals2.character as Horse;
						if (horse == null)
						{
							Pet pet = CS$<>8__locals2.character as Pet;
							if (pet != null)
							{
								if (!ForEachItemHelper.ApplyToField<Hat>(pet.hat, handler, new GetForEachItemPathDelegate(CS$<>8__locals2.<ForEachItemInLocation>g__GetNpcPath|3), null))
								{
									return false;
								}
							}
						}
						else if (!ForEachItemHelper.ApplyToField<Hat>(horse.hat, handler, new GetForEachItemPathDelegate(CS$<>8__locals2.<ForEachItemInLocation>g__GetNpcPath|3), null))
						{
							return false;
						}
					}
					else if (!ForEachItemHelper.ApplyToField<Hat>(child.hat, handler, new GetForEachItemPathDelegate(CS$<>8__locals2.<ForEachItemInLocation>g__GetNpcPath|3), null))
					{
						return false;
					}
				}
			}
			using (List<Building>.Enumerator enumerator2 = CS$<>8__locals1.location.buildings.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (!enumerator2.Current.ForEachItemContextExcludingInterior(handler, new GetForEachItemPathDelegate(CS$<>8__locals1.<ForEachItemInLocation>g__GetLocationPath|0)))
					{
						return false;
					}
				}
			}
			Chest fridge = CS$<>8__locals1.location.GetFridge(false);
			if (!(((fridge != null) ? new bool?(fridge.ForEachItem(handler, new GetForEachItemPathDelegate(CS$<>8__locals1.<ForEachItemInLocation>g__GetLocationPath|0))) : null) ?? true))
			{
				return false;
			}
			if (CS$<>8__locals1.location.objects.Length > 0)
			{
				using (Dictionary<Vector2, Object>.KeyCollection.Enumerator enumerator3 = CS$<>8__locals1.location.objects.Keys.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						ForEachItemHelper.<>c__DisplayClass1_2 CS$<>8__locals3 = new ForEachItemHelper.<>c__DisplayClass1_2();
						CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals1;
						CS$<>8__locals3.tile = enumerator3.Current;
						Object obj = CS$<>8__locals3.CS$<>8__locals2.location.objects[CS$<>8__locals3.tile];
						Object obj2 = obj;
						Action remove = delegate()
						{
							CS$<>8__locals3.CS$<>8__locals2.location.objects.Remove(CS$<>8__locals3.tile);
						};
						Action<Item> replaceWith = delegate(Item newItem)
						{
							CS$<>8__locals3.CS$<>8__locals2.location.objects[CS$<>8__locals3.tile] = ForEachItemHelper.PrepareForReplaceWith<Object>(obj, (Object)newItem);
						};
						GetForEachItemPathDelegate getParentPath;
						if ((getParentPath = CS$<>8__locals3.CS$<>8__locals2.<>9__6) == null)
						{
							getParentPath = (CS$<>8__locals3.CS$<>8__locals2.<>9__6 = (() => ForEachItemHelper.CombinePath(new GetForEachItemPathDelegate(base.<ForEachItemInLocation>g__GetLocationPath|0), new object[]
							{
								CS$<>8__locals3.CS$<>8__locals2.location.objects
							})));
						}
						if (!ForEachItemHelper.ApplyToItem<Object>(obj2, handler, remove, replaceWith, getParentPath))
						{
							return false;
						}
					}
				}
			}
			ForEachItemHelper.<>c__DisplayClass1_4 CS$<>8__locals5 = new ForEachItemHelper.<>c__DisplayClass1_4();
			CS$<>8__locals5.CS$<>8__locals4 = CS$<>8__locals1;
			CS$<>8__locals5.i = CS$<>8__locals5.CS$<>8__locals4.location.debris.Count - 1;
			while (CS$<>8__locals5.i >= 0)
			{
				ForEachItemHelper.<>c__DisplayClass1_5 CS$<>8__locals6 = new ForEachItemHelper.<>c__DisplayClass1_5();
				CS$<>8__locals6.CS$<>8__locals5 = CS$<>8__locals5;
				CS$<>8__locals6.d = CS$<>8__locals6.CS$<>8__locals5.CS$<>8__locals4.location.debris[CS$<>8__locals6.CS$<>8__locals5.i];
				if (CS$<>8__locals6.d.item != null)
				{
					Item item = CS$<>8__locals6.d.item;
					Action remove2 = new Action(CS$<>8__locals6.<ForEachItemInLocation>g__Remove|7);
					Action<Item> replaceWith2 = new Action<Item>(CS$<>8__locals6.<ForEachItemInLocation>g__ReplaceWith|8);
					GetForEachItemPathDelegate getParentPath2;
					if ((getParentPath2 = CS$<>8__locals6.CS$<>8__locals5.CS$<>8__locals4.<>9__9) == null)
					{
						getParentPath2 = (CS$<>8__locals6.CS$<>8__locals5.CS$<>8__locals4.<>9__9 = (() => ForEachItemHelper.CombinePath(new GetForEachItemPathDelegate(base.<ForEachItemInLocation>g__GetLocationPath|0), new object[]
						{
							CS$<>8__locals6.CS$<>8__locals5.CS$<>8__locals4.location.debris
						})));
					}
					if (!ForEachItemHelper.ApplyToItem<Item>(item, handler, remove2, replaceWith2, getParentPath2))
					{
						return false;
					}
				}
				int i = CS$<>8__locals5.i;
				CS$<>8__locals5.i = i - 1;
			}
			CS$<>8__locals1.shopLocation = (CS$<>8__locals1.location as ShopLocation);
			if (CS$<>8__locals1.shopLocation != null)
			{
				if (!ForEachItemHelper.ApplyToList<Item>(CS$<>8__locals1.shopLocation.itemsFromPlayerToSell, handler, () => ForEachItemHelper.CombinePath(new GetForEachItemPathDelegate(base.<ForEachItemInLocation>g__GetLocationPath|0), new object[]
				{
					CS$<>8__locals1.shopLocation.itemsFromPlayerToSell
				}), false, null))
				{
					return false;
				}
				if (!ForEachItemHelper.ApplyToList<Item>(CS$<>8__locals1.shopLocation.itemsToStartSellingTomorrow, handler, () => ForEachItemHelper.CombinePath(new GetForEachItemPathDelegate(base.<ForEachItemInLocation>g__GetLocationPath|0), new object[]
				{
					CS$<>8__locals1.shopLocation.itemsToStartSellingTomorrow
				}), false, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06003412 RID: 13330 RVA: 0x0029AD94 File Offset: 0x00298F94
		public static bool ApplyToItem<TItem>(TItem item, ForEachItemDelegate handler, Action remove, Action<Item> replaceWith, GetForEachItemPathDelegate getParentPath) where TItem : Item
		{
			ForEachItemHelper.<>c__DisplayClass2_0<TItem> CS$<>8__locals1 = new ForEachItemHelper.<>c__DisplayClass2_0<TItem>();
			CS$<>8__locals1.remove = remove;
			CS$<>8__locals1.item = item;
			CS$<>8__locals1.replaceWith = replaceWith;
			CS$<>8__locals1.getParentPath = getParentPath;
			if (CS$<>8__locals1.item == null)
			{
				return true;
			}
			ForEachItemContext forEachItemContext = new ForEachItemContext(CS$<>8__locals1.item, new Action(CS$<>8__locals1.<ApplyToItem>g__Remove|0), new Action<Item>(CS$<>8__locals1.<ApplyToItem>g__ReplaceWith|1), CS$<>8__locals1.getParentPath);
			if (handler(forEachItemContext))
			{
				TItem titem = CS$<>8__locals1.item;
				return titem == null || titem.ForEachItem(handler, () => ForEachItemHelper.CombinePath(CS$<>8__locals1.getParentPath, new object[]
				{
					CS$<>8__locals1.item
				}));
			}
			return false;
		}

		// Token: 0x06003413 RID: 13331 RVA: 0x0029AE34 File Offset: 0x00299034
		public static bool ApplyToField<TItem>(NetRef<TItem> field, ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath, Action<Item, Item> onChanged = null) where TItem : Item
		{
			ForEachItemHelper.<>c__DisplayClass3_0<TItem> CS$<>8__locals1 = new ForEachItemHelper.<>c__DisplayClass3_0<TItem>();
			CS$<>8__locals1.field = field;
			CS$<>8__locals1.onChanged = onChanged;
			CS$<>8__locals1.getParentPath = getParentPath;
			CS$<>8__locals1.oldValue = CS$<>8__locals1.field.Value;
			return ForEachItemHelper.ApplyToItem<TItem>(CS$<>8__locals1.field.Value, handler, new Action(CS$<>8__locals1.<ApplyToField>g__Remove|0), new Action<Item>(CS$<>8__locals1.<ApplyToField>g__ReplaceWith|1), new GetForEachItemPathDelegate(CS$<>8__locals1.<ApplyToField>g__GetPath|2));
		}

		// Token: 0x06003414 RID: 13332 RVA: 0x0029AEA8 File Offset: 0x002990A8
		public static bool ApplyToList<TItem>(IList<TItem> list, ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath, bool leaveNullSlotsOnRemoval = false, Action<Item, Item, int> onChanged = null) where TItem : Item
		{
			ForEachItemHelper.<>c__DisplayClass4_0<TItem> CS$<>8__locals1 = new ForEachItemHelper.<>c__DisplayClass4_0<TItem>();
			CS$<>8__locals1.getParentPath = getParentPath;
			CS$<>8__locals1.list = list;
			CS$<>8__locals1.leaveNullSlotsOnRemoval = leaveNullSlotsOnRemoval;
			CS$<>8__locals1.onChanged = onChanged;
			ForEachItemHelper.<>c__DisplayClass4_1<TItem> CS$<>8__locals2 = new ForEachItemHelper.<>c__DisplayClass4_1<TItem>();
			CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
			CS$<>8__locals2.i = CS$<>8__locals2.CS$<>8__locals1.list.Count - 1;
			while (CS$<>8__locals2.i >= 0)
			{
				ForEachItemHelper.<>c__DisplayClass4_2<TItem> CS$<>8__locals3 = new ForEachItemHelper.<>c__DisplayClass4_2<TItem>();
				CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals2;
				CS$<>8__locals3.oldValue = CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.list[CS$<>8__locals3.CS$<>8__locals2.i];
				if (!ForEachItemHelper.ApplyToItem<TItem>(CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.list[CS$<>8__locals3.CS$<>8__locals2.i], handler, new Action(CS$<>8__locals3.<ApplyToList>g__Remove|1), new Action<Item>(CS$<>8__locals3.<ApplyToList>g__ReplaceWith|2), new GetForEachItemPathDelegate(CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<ApplyToList>g__GetPath|0)))
				{
					return false;
				}
				int i = CS$<>8__locals2.i;
				CS$<>8__locals2.i = i - 1;
			}
			return true;
		}

		// Token: 0x06003415 RID: 13333 RVA: 0x0029AFB0 File Offset: 0x002991B0
		public static IList<object> CombinePath(GetForEachItemPathDelegate parentPath, params object[] pathValues)
		{
			IList<object> combined = ((parentPath != null) ? parentPath() : null) ?? new List<object>();
			foreach (object pathValue in pathValues)
			{
				combined.Add(pathValue);
			}
			return combined;
		}

		// Token: 0x06003416 RID: 13334 RVA: 0x0029AFF0 File Offset: 0x002991F0
		private static TItem PrepareForReplaceWith<TItem>(TItem previousItem, TItem newItem) where TItem : Item
		{
			Object previousObj = previousItem as Object;
			Object newObj = newItem as Object;
			if (previousObj != null && newObj != null)
			{
				newObj.TileLocation = previousObj.TileLocation;
			}
			return newItem;
		}

		// Token: 0x06003417 RID: 13335 RVA: 0x0029B028 File Offset: 0x00299228
		[CompilerGenerated]
		internal static IList<object> <ForEachItemInWorld>g__GetParentPathForTeam|0_0()
		{
			return new List<object>
			{
				Game1.player.team
			};
		}
	}
}
