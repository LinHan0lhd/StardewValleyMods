using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x02000102 RID: 258
	public interface ISoundBank : IDisposable
	{
		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06001482 RID: 5250
		bool IsInUse { get; }

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06001483 RID: 5251
		bool IsDisposed { get; }

		// Token: 0x06001484 RID: 5252
		ICue GetCue(string name);

		// Token: 0x06001485 RID: 5253
		void PlayCue(string name);

		// Token: 0x06001486 RID: 5254
		void PlayCue(string name, AudioListener listener, AudioEmitter emitter);

		// Token: 0x06001487 RID: 5255
		void AddCue(CueDefinition definition);

		// Token: 0x06001488 RID: 5256
		bool Exists(string name);

		// Token: 0x06001489 RID: 5257
		CueDefinition GetCueDefinition(string name);
	}
}
