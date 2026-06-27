using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Audio
{
	// Token: 0x020003B7 RID: 951
	public interface ISoundsHelper
	{
		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06003952 RID: 14674
		// (set) Token: 0x06003953 RID: 14675
		bool LogSounds { get; set; }

		// Token: 0x06003954 RID: 14676
		bool ShouldPlayLocal(SoundContext context);

		// Token: 0x06003955 RID: 14677
		float GetVolumeForDistance(GameLocation location, Vector2? position);

		// Token: 0x06003956 RID: 14678
		bool PlayLocal(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context, out ICue cue);

		// Token: 0x06003957 RID: 14679
		void PlayAll(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context);

		// Token: 0x06003958 RID: 14680
		void SetPitch(ICue cue, float pitch, bool forcePitch = true);
	}
}
