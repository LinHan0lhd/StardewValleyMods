using System;
using System.Collections.Generic;

namespace StardewValley.Inventories
{
	// Token: 0x0200030B RID: 779
	public class InventoryIndex
	{
		// Token: 0x060033F9 RID: 13305 RVA: 0x00299FAC File Offset: 0x002981AC
		public InventoryIndex(Action<InventoryIndex, Item> addImpl, Action<InventoryIndex, Item> removeImpl)
		{
			this.AddImpl = addImpl;
			this.RemoveImpl = removeImpl;
		}

		// Token: 0x060033FA RID: 13306 RVA: 0x00299FD0 File Offset: 0x002981D0
		public static InventoryIndex ById(IList<Item> items)
		{
			InventoryIndex instance = new InventoryIndex(delegate(InventoryIndex index, Item item)
			{
				index.AddWithKey(item.QualifiedItemId, item);
			}, delegate(InventoryIndex index, Item item)
			{
				index.RemoveItem(item.QualifiedItemId, item);
			});
			foreach (Item item2 in items)
			{
				instance.Add(item2);
			}
			return instance;
		}

		// Token: 0x060033FB RID: 13307 RVA: 0x0029A060 File Offset: 0x00298260
		public int CountKeys()
		{
			return this.Index.Count;
		}

		// Token: 0x060033FC RID: 13308 RVA: 0x0029A070 File Offset: 0x00298270
		public int CountItems()
		{
			int count = 0;
			foreach (List<Item> list in this.Index.Values)
			{
				count += list.Count;
			}
			return count;
		}

		// Token: 0x060033FD RID: 13309 RVA: 0x0029A0D0 File Offset: 0x002982D0
		public bool Contains(string key)
		{
			return key != null && this.Index.ContainsKey(key);
		}

		// Token: 0x060033FE RID: 13310 RVA: 0x0029A0E4 File Offset: 0x002982E4
		public bool TryGet(string key, out IReadOnlyList<Item> items)
		{
			List<Item> indexed;
			if (key != null && this.Index.TryGetValue(key, out indexed))
			{
				items = indexed;
				return true;
			}
			items = null;
			return false;
		}

		// Token: 0x060033FF RID: 13311 RVA: 0x0029A110 File Offset: 0x00298310
		public bool TryGetMutable(string key, out IList<Item> items)
		{
			List<Item> indexed;
			if (key != null && this.Index.TryGetValue(key, out indexed))
			{
				items = indexed;
				return true;
			}
			items = null;
			return false;
		}

		// Token: 0x06003400 RID: 13312 RVA: 0x0029A139 File Offset: 0x00298339
		public void Add(Item item)
		{
			if (item != null)
			{
				this.AddImpl(this, item);
			}
		}

		// Token: 0x06003401 RID: 13313 RVA: 0x0029A14C File Offset: 0x0029834C
		public void AddWithKey(string key, Item item)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (item != null)
			{
				List<Item> list;
				if (!this.Index.TryGetValue(key, out list))
				{
					list = (this.Index[key] = new List<Item>());
				}
				list.Add(item);
			}
		}

		// Token: 0x06003402 RID: 13314 RVA: 0x0029A194 File Offset: 0x00298394
		public void Remove(Item item)
		{
			if (item != null)
			{
				this.RemoveImpl(this, item);
			}
		}

		// Token: 0x06003403 RID: 13315 RVA: 0x0029A1A6 File Offset: 0x002983A6
		public void RemoveKey(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			this.Index.Remove(key);
		}

		// Token: 0x06003404 RID: 13316 RVA: 0x0029A1C4 File Offset: 0x002983C4
		public void RemoveItem(string key, Item item)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			List<Item> list;
			if (item != null && this.Index.TryGetValue(key, out list))
			{
				list.Remove(item);
				if (list.Count == 0)
				{
					this.Index.Remove(key);
				}
			}
		}

		// Token: 0x0400221C RID: 8732
		private readonly Dictionary<string, List<Item>> Index = new Dictionary<string, List<Item>>();

		// Token: 0x0400221D RID: 8733
		private readonly Action<InventoryIndex, Item> AddImpl;

		// Token: 0x0400221E RID: 8734
		private readonly Action<InventoryIndex, Item> RemoveImpl;
	}
}
