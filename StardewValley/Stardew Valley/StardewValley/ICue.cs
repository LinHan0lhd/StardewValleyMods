using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	// Token: 0x0200008E RID: 142
	public interface ICue : IDisposable
	{
		// Token: 0x060005D2 RID: 1490
		void Play();

		// Token: 0x060005D3 RID: 1491
		void Pause();

		// Token: 0x060005D4 RID: 1492
		void Resume();

		// Token: 0x060005D5 RID: 1493
		void Stop(AudioStopOptions options);

		// Token: 0x060005D6 RID: 1494
		void SetVariable(string var, int val);

		// Token: 0x060005D7 RID: 1495
		void SetVariable(string var, float val);

		// Token: 0x060005D8 RID: 1496
		float GetVariable(string var);

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x060005D9 RID: 1497
		bool IsStopped { get; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x060005DA RID: 1498
		bool IsStopping { get; }

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060005DB RID: 1499
		bool IsPlaying { get; }

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060005DC RID: 1500
		bool IsPaused { get; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x060005DD RID: 1501
		string Name { get; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x060005DE RID: 1502
		// (set) Token: 0x060005DF RID: 1503
		float Pitch { get; set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060005E0 RID: 1504
		// (set) Token: 0x060005E1 RID: 1505
		float Volume { get; set; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x060005E2 RID: 1506
		bool IsPitchBeingControlledByRPC { get; }
	}
}
