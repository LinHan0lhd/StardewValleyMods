using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio
{
	// Token: 0x020003B0 RID: 944
	public class AudioCategoryWrapper : IAudioCategory
	{
		// Token: 0x06003935 RID: 14645 RVA: 0x002D6708 File Offset: 0x002D4908
		public AudioCategoryWrapper(AudioCategory category)
		{
			this.audioCategory = category;
		}

		// Token: 0x06003936 RID: 14646 RVA: 0x002D6717 File Offset: 0x002D4917
		public void SetVolume(float volume)
		{
			this.audioCategory.SetVolume(volume);
		}

		// Token: 0x040025F3 RID: 9715
		private AudioCategory audioCategory;
	}
}
