using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio
{
	// Token: 0x020003B2 RID: 946
	internal class AudioEngineWrapper : IAudioEngine, IDisposable
	{
		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x0600393C RID: 14652 RVA: 0x002D69BC File Offset: 0x002D4BBC
		public AudioEngine Engine
		{
			get
			{
				return this.audioEngine;
			}
		}

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x0600393D RID: 14653 RVA: 0x002D69C4 File Offset: 0x002D4BC4
		public bool IsDisposed
		{
			get
			{
				return this.audioEngine.IsDisposed;
			}
		}

		// Token: 0x0600393E RID: 14654 RVA: 0x002D69D1 File Offset: 0x002D4BD1
		public AudioEngineWrapper(AudioEngine engine)
		{
			this.audioEngine = engine;
		}

		// Token: 0x0600393F RID: 14655 RVA: 0x002D69E0 File Offset: 0x002D4BE0
		public void Dispose()
		{
			this.audioEngine.Dispose();
		}

		// Token: 0x06003940 RID: 14656 RVA: 0x002D69ED File Offset: 0x002D4BED
		public IAudioCategory GetCategory(string name)
		{
			return new AudioCategoryWrapper(this.audioEngine.GetCategory(name));
		}

		// Token: 0x06003941 RID: 14657 RVA: 0x002D6A00 File Offset: 0x002D4C00
		public int GetCategoryIndex(string name)
		{
			return this.audioEngine.GetCategoryIndex(name);
		}

		// Token: 0x06003942 RID: 14658 RVA: 0x002D6A0E File Offset: 0x002D4C0E
		public void Update()
		{
			this.audioEngine.Update();
		}

		// Token: 0x040025F5 RID: 9717
		private AudioEngine audioEngine;
	}
}
