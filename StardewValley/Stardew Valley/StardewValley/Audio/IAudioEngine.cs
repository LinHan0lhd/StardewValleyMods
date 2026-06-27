using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio
{
	// Token: 0x020003B6 RID: 950
	public interface IAudioEngine : IDisposable
	{
		// Token: 0x0600394D RID: 14669
		void Update();

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x0600394E RID: 14670
		bool IsDisposed { get; }

		// Token: 0x0600394F RID: 14671
		IAudioCategory GetCategory(string name);

		// Token: 0x06003950 RID: 14672
		int GetCategoryIndex(string name);

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06003951 RID: 14673
		AudioEngine Engine { get; }
	}
}
