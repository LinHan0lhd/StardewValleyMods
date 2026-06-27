using System;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000181 RID: 385
	public interface ISaveMigrator
	{
		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06001C68 RID: 7272
		Version GameVersion { get; }

		// Token: 0x06001C69 RID: 7273
		bool ApplySaveFix(SaveFixes saveFix);
	}
}
