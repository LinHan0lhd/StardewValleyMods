using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Inventories
{
	// Token: 0x02000309 RID: 777
	public interface IInventory : IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
	{
		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x060033B9 RID: 13241
		// (set) Token: 0x060033BA RID: 13242
		bool IsLocalPlayerInventory { get; set; }

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x060033BB RID: 13243
		long LastTickSlotChanged { get; }

		// Token: 0x060033BC RID: 13244
		bool HasAny();

		// Token: 0x060033BD RID: 13245
		bool HasEmptySlots();

		// Token: 0x060033BE RID: 13246
		int CountItemStacks();

		// Token: 0x060033BF RID: 13247
		void OverwriteWith(IList<Item> list);

		// Token: 0x060033C0 RID: 13248
		IList<Item> GetRange(int index, int count);

		// Token: 0x060033C1 RID: 13249
		void AddRange(ICollection<Item> collection);

		// Token: 0x060033C2 RID: 13250
		void RemoveRange(int index, int count);

		// Token: 0x060033C3 RID: 13251
		void RemoveEmptySlots();

		// Token: 0x060033C4 RID: 13252
		bool ContainsId(string itemId);

		// Token: 0x060033C5 RID: 13253
		bool ContainsId(string itemId, int minimum);

		// Token: 0x060033C6 RID: 13254
		int CountId(string itemId);

		// Token: 0x060033C7 RID: 13255
		IEnumerable<Item> GetById(string itemId);

		// Token: 0x060033C8 RID: 13256
		int Reduce(Item item, int count, bool reduceRemainderFromInventory = false);

		// Token: 0x060033C9 RID: 13257
		int ReduceId(string itemId, int count);

		// Token: 0x060033CA RID: 13258
		bool RemoveButKeepEmptySlot(Item item);
	}
}
