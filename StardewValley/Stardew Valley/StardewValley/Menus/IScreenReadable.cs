using System;

namespace StardewValley.Menus
{
	// Token: 0x0200027A RID: 634
	public interface IScreenReadable
	{
		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x060029FF RID: 10751
		string ScreenReaderText { get; }

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06002A00 RID: 10752
		string ScreenReaderDescription { get; }

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06002A01 RID: 10753
		bool ScreenReaderIgnore { get; }
	}
}
