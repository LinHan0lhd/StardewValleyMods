using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio
{
	// Token: 0x020003B4 RID: 948
	internal class DummyAudioEngine : IAudioEngine, IDisposable
	{
		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x06003945 RID: 14661 RVA: 0x002D6A25 File Offset: 0x002D4C25
		public AudioEngine Engine { get; }

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06003946 RID: 14662 RVA: 0x002D6A2D File Offset: 0x002D4C2D
		public bool IsDisposed { get; } = 1;

		// Token: 0x06003947 RID: 14663 RVA: 0x002D6A35 File Offset: 0x002D4C35
		public void Update()
		{
		}

		// Token: 0x06003948 RID: 14664 RVA: 0x002D6A37 File Offset: 0x002D4C37
		public IAudioCategory GetCategory(string name)
		{
			return this.category;
		}

		// Token: 0x06003949 RID: 14665 RVA: 0x002D6A3F File Offset: 0x002D4C3F
		public int GetCategoryIndex(string name)
		{
			return -1;
		}

		// Token: 0x0600394A RID: 14666 RVA: 0x002D6A42 File Offset: 0x002D4C42
		public void Dispose()
		{
		}

		// Token: 0x040025F6 RID: 9718
		private IAudioCategory category = new DummyAudioCategory();
	}
}
