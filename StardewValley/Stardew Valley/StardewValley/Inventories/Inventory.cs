using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Logging;
using StardewValley.SaveSerialization;

namespace StardewValley.Inventories
{
	// Token: 0x0200030A RID: 778
	[XmlRoot("items")]
	public class Inventory : INetObject<NetFields>, IXmlSerializable, IInventory, IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
	{
		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x060033CB RID: 13259 RVA: 0x002996EA File Offset: 0x002978EA
		public NetFields NetFields { get; } = new NetFields("Inventory");

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x060033CC RID: 13260 RVA: 0x002996F2 File Offset: 0x002978F2
		public int Count
		{
			get
			{
				return this.Items.Count;
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x060033CD RID: 13261 RVA: 0x002996FF File Offset: 0x002978FF
		public bool IsReadOnly
		{
			get
			{
				return this.Items.IsReadOnly;
			}
		}

		// Token: 0x17000450 RID: 1104
		public Item this[int index]
		{
			get
			{
				return this.Items[index];
			}
			set
			{
				this.Items[index] = value;
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x060033D0 RID: 13264 RVA: 0x00299729 File Offset: 0x00297929
		// (set) Token: 0x060033D1 RID: 13265 RVA: 0x00299731 File Offset: 0x00297931
		public bool IsLocalPlayerInventory { get; set; }

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x060033D2 RID: 13266 RVA: 0x0029973C File Offset: 0x0029793C
		// (remove) Token: 0x060033D3 RID: 13267 RVA: 0x00299774 File Offset: 0x00297974
		public event OnSlotChangedDelegate OnSlotChanged;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x060033D4 RID: 13268 RVA: 0x002997AC File Offset: 0x002979AC
		// (remove) Token: 0x060033D5 RID: 13269 RVA: 0x002997E4 File Offset: 0x002979E4
		public event OnInventoryReplacedDelegate OnInventoryReplaced;

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x060033D6 RID: 13270 RVA: 0x00299819 File Offset: 0x00297A19
		// (set) Token: 0x060033D7 RID: 13271 RVA: 0x00299821 File Offset: 0x00297A21
		public long LastTickSlotChanged { get; private set; }

		// Token: 0x060033D8 RID: 13272 RVA: 0x0029982C File Offset: 0x00297A2C
		public Inventory()
		{
			this.NetFields.SetOwner(this).AddField(this.Items, "this.Items");
			this.Items.OnElementChanged += this.HandleElementChanged;
			this.Items.OnArrayReplaced += this.HandleArrayReplaced;
		}

		// Token: 0x060033D9 RID: 13273 RVA: 0x002998A5 File Offset: 0x00297AA5
		public bool HasAny()
		{
			return this.GetItemsById().CountKeys() > 0;
		}

		// Token: 0x060033DA RID: 13274 RVA: 0x002998B5 File Offset: 0x00297AB5
		public bool HasEmptySlots()
		{
			return this.Count > this.CountItemStacks();
		}

		// Token: 0x060033DB RID: 13275 RVA: 0x002998C8 File Offset: 0x00297AC8
		public int CountItemStacks()
		{
			int? cachedItemStackCount = this.CachedItemStackCount;
			if (cachedItemStackCount == null)
			{
				int? num = this.CachedItemStackCount = new int?(this.GetItemsById().CountItems());
				return num.Value;
			}
			return cachedItemStackCount.GetValueOrDefault();
		}

		// Token: 0x060033DC RID: 13276 RVA: 0x0029990E File Offset: 0x00297B0E
		public void OverwriteWith(IList<Item> list)
		{
			if (this == list || this.Items == list)
			{
				return;
			}
			this.ClearIndex();
			this.Items.CopyFrom(list);
		}

		// Token: 0x060033DD RID: 13277 RVA: 0x00299930 File Offset: 0x00297B30
		public IList<Item> GetRange(int index, int count)
		{
			return this.Items.GetRange(index, count);
		}

		// Token: 0x060033DE RID: 13278 RVA: 0x0029993F File Offset: 0x00297B3F
		public void AddRange(ICollection<Item> collection)
		{
			this.Items.AddRange(collection);
		}

		// Token: 0x060033DF RID: 13279 RVA: 0x0029994D File Offset: 0x00297B4D
		public void RemoveRange(int index, int count)
		{
			this.Items.RemoveRange(index, count);
		}

		// Token: 0x060033E0 RID: 13280 RVA: 0x0029995C File Offset: 0x00297B5C
		public void RemoveEmptySlots()
		{
			if (!this.HasEmptySlots())
			{
				return;
			}
			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (this[i] == null)
				{
					this.RemoveAt(i);
				}
			}
		}

		// Token: 0x060033E1 RID: 13281 RVA: 0x00299995 File Offset: 0x00297B95
		public bool ContainsId(string itemId)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			return itemId != null && this.GetItemsById().Contains(itemId);
		}

		// Token: 0x060033E2 RID: 13282 RVA: 0x002999B0 File Offset: 0x00297BB0
		public bool ContainsId(string itemId, int minimum)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == null)
			{
				return false;
			}
			IReadOnlyList<Item> items;
			if (this.GetItemsById().TryGet(itemId, out items))
			{
				if (minimum <= 1)
				{
					return true;
				}
				int count = 0;
				foreach (Item item in items)
				{
					if (item.QualifiedItemId == itemId)
					{
						count += item.Stack;
					}
					if (count >= minimum)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060033E3 RID: 13283 RVA: 0x00299A3C File Offset: 0x00297C3C
		public int CountId(string itemId)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == null)
			{
				return 0;
			}
			IReadOnlyList<Item> items;
			if (this.GetItemsById().TryGet(itemId, out items))
			{
				int count = 0;
				foreach (Item item in items)
				{
					if (item.QualifiedItemId == itemId)
					{
						count += item.Stack;
					}
				}
				return count;
			}
			return 0;
		}

		// Token: 0x060033E4 RID: 13284 RVA: 0x00299AB8 File Offset: 0x00297CB8
		public IEnumerable<Item> GetById(string itemId)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			IReadOnlyList<Item> items;
			if (itemId == null || !this.GetItemsById().TryGet(itemId, out items))
			{
				return LegacyShims.EmptyArray<Item>();
			}
			return items;
		}

		// Token: 0x060033E5 RID: 13285 RVA: 0x00299AEC File Offset: 0x00297CEC
		public int Reduce(Item item, int count, bool reduceRemainderFromInventory = false)
		{
			int index = -1;
			if (this.IsLocalPlayerInventory)
			{
				index = Game1.player.CurrentToolIndex;
				if (index < 0 || index >= this.Count || this.Items[index] != item)
				{
					index = -1;
				}
			}
			if (index < 0)
			{
				index = this.IndexOf(item);
			}
			int remaining = count;
			if (index > -1)
			{
				remaining -= item.Stack;
				this.Items[index] = item.ConsumeStack(count);
			}
			else
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't deduct item with ID '");
				defaultInterpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
				defaultInterpolatedStringHandler.AppendLiteral("' from ");
				defaultInterpolatedStringHandler.AppendFormatted(this.IsLocalPlayerInventory ? "the player's" : "this");
				defaultInterpolatedStringHandler.AppendLiteral(" inventory because it's not in that inventory.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (reduceRemainderFromInventory && remaining > 0)
			{
				remaining -= this.ReduceId(item.QualifiedItemId, remaining);
			}
			if (remaining > 0)
			{
				return count - remaining;
			}
			return count;
		}

		// Token: 0x060033E6 RID: 13286 RVA: 0x00299BE4 File Offset: 0x00297DE4
		public int ReduceId(string itemId, int count)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == null)
			{
				return 0;
			}
			InventoryIndex itemsById = this.GetItemsById();
			IList<Item> items;
			if (itemsById.TryGetMutable(itemId, out items))
			{
				bool anyStacksRemoved = false;
				int remaining = count;
				int i = 0;
				while (i < items.Count && remaining > 0)
				{
					Item item = items[i];
					int toRemove = Math.Min(remaining, item.Stack);
					items[i] = item.ConsumeStack(toRemove);
					if (items[i] == null)
					{
						anyStacksRemoved = true;
						items.RemoveAt(i);
						item.SetTempData<string>("__Inventory_ReduceId_Remove", "");
						i--;
					}
					remaining -= toRemove;
					i++;
				}
				if (items.Count == 0)
				{
					itemsById.RemoveKey(itemId);
				}
				if (anyStacksRemoved)
				{
					for (int j = this.Items.Count - 1; j >= 0; j--)
					{
						Item item2 = this.Items[j];
						bool? flag;
						if (item2 == null)
						{
							flag = null;
						}
						else
						{
							Dictionary<string, object> tempData = item2.tempData;
							flag = ((tempData != null) ? new bool?(tempData.Remove("__Inventory_ReduceId_Remove")) : null);
						}
						bool? flag2 = flag;
						if (flag2 != null && flag2.GetValueOrDefault())
						{
							this.Items[j] = null;
						}
					}
				}
				return count - remaining;
			}
			return 0;
		}

		// Token: 0x060033E7 RID: 13287 RVA: 0x00299D20 File Offset: 0x00297F20
		public bool RemoveButKeepEmptySlot(Item item)
		{
			if (item == null)
			{
				return false;
			}
			int index = this.Items.IndexOf(item);
			if (index == -1)
			{
				return false;
			}
			this.Items[index] = null;
			return true;
		}

		// Token: 0x060033E8 RID: 13288 RVA: 0x00299D53 File Offset: 0x00297F53
		public IEnumerator<Item> GetEnumerator()
		{
			return this.Items.GetEnumerator();
		}

		// Token: 0x060033E9 RID: 13289 RVA: 0x00299D65 File Offset: 0x00297F65
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Items.GetEnumerator();
		}

		// Token: 0x060033EA RID: 13290 RVA: 0x00299D77 File Offset: 0x00297F77
		public void Add(Item item)
		{
			this.Items.Add(item);
		}

		// Token: 0x060033EB RID: 13291 RVA: 0x00299D85 File Offset: 0x00297F85
		public void Clear()
		{
			this.ClearIndex();
			this.Items.Clear();
		}

		// Token: 0x060033EC RID: 13292 RVA: 0x00299D98 File Offset: 0x00297F98
		public bool Contains(Item item)
		{
			IList<Item> list;
			return item != null && this.GetItemsById().TryGetMutable(item.QualifiedItemId, out list) && list.Contains(item);
		}

		// Token: 0x060033ED RID: 13293 RVA: 0x00299DC8 File Offset: 0x00297FC8
		public void CopyTo(Item[] array, int arrayIndex)
		{
			this.Items.CopyTo(array, arrayIndex);
		}

		// Token: 0x060033EE RID: 13294 RVA: 0x00299DD7 File Offset: 0x00297FD7
		public bool Remove(Item item)
		{
			return item != null && this.Items.Remove(item);
		}

		// Token: 0x060033EF RID: 13295 RVA: 0x00299DEA File Offset: 0x00297FEA
		public int IndexOf(Item item)
		{
			return this.Items.IndexOf(item);
		}

		// Token: 0x060033F0 RID: 13296 RVA: 0x00299DF8 File Offset: 0x00297FF8
		public void Insert(int index, Item item)
		{
			this.Items.Insert(index, item);
		}

		// Token: 0x060033F1 RID: 13297 RVA: 0x00299E07 File Offset: 0x00298007
		public void RemoveAt(int index)
		{
			this.Items.RemoveAt(index);
		}

		// Token: 0x060033F2 RID: 13298 RVA: 0x00299E15 File Offset: 0x00298015
		public XmlSchema GetSchema()
		{
			return null;
		}

		// Token: 0x060033F3 RID: 13299 RVA: 0x00299E18 File Offset: 0x00298018
		public void ReadXml(XmlReader reader)
		{
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (isEmptyElement)
			{
				return;
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				Item item = SaveSerializer.Deserialize<Item>(reader);
				this.Items.Add(item);
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		// Token: 0x060033F4 RID: 13300 RVA: 0x00299E60 File Offset: 0x00298060
		public void WriteXml(XmlWriter writer)
		{
			foreach (Item item in this.Items)
			{
				SaveSerializer.Serialize<Item>(writer, item);
			}
		}

		// Token: 0x060033F5 RID: 13301 RVA: 0x00299EB4 File Offset: 0x002980B4
		private InventoryIndex GetItemsById()
		{
			InventoryIndex result;
			if ((result = this.ItemsById) == null)
			{
				result = (this.ItemsById = InventoryIndex.ById(this.Items));
			}
			return result;
		}

		// Token: 0x060033F6 RID: 13302 RVA: 0x00299EE0 File Offset: 0x002980E0
		private void HandleArrayReplaced(NetList<Item, NetRef<Item>> list, IList<Item> before, IList<Item> after)
		{
			if (before.Count != 0 || after.Count != 0)
			{
				this.ClearIndex();
				this.CachedItemStackCount = null;
				this.LastTickSlotChanged = DateTime.UtcNow.Ticks;
				OnInventoryReplacedDelegate onInventoryReplaced = this.OnInventoryReplaced;
				if (onInventoryReplaced == null)
				{
					return;
				}
				onInventoryReplaced(this, before, after);
			}
		}

		// Token: 0x060033F7 RID: 13303 RVA: 0x00299F38 File Offset: 0x00298138
		private void HandleElementChanged(NetList<Item, NetRef<Item>> list, int index, Item before, Item after)
		{
			if (before != after)
			{
				InventoryIndex itemsById = this.ItemsById;
				if (itemsById != null)
				{
					itemsById.Remove(before);
				}
				InventoryIndex itemsById2 = this.ItemsById;
				if (itemsById2 != null)
				{
					itemsById2.Add(after);
				}
				this.CachedItemStackCount = null;
				this.LastTickSlotChanged = DateTime.UtcNow.Ticks;
				OnSlotChangedDelegate onSlotChanged = this.OnSlotChanged;
				if (onSlotChanged == null)
				{
					return;
				}
				onSlotChanged(this, index, before, after);
			}
		}

		// Token: 0x060033F8 RID: 13304 RVA: 0x00299FA3 File Offset: 0x002981A3
		private void ClearIndex()
		{
			this.ItemsById = null;
		}

		// Token: 0x04002214 RID: 8724
		private readonly NetObjectList<Item> Items = new NetObjectList<Item>();

		// Token: 0x04002215 RID: 8725
		private InventoryIndex ItemsById;

		// Token: 0x04002216 RID: 8726
		private int? CachedItemStackCount;
	}
}
