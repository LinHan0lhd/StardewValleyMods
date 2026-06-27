using System;

namespace StardewValley.SDKs
{
	// Token: 0x02000162 RID: 354
	public interface SDKHelper
	{
		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06001B23 RID: 6947
		bool IsEnterButtonAssignmentFlipped { get; }

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06001B24 RID: 6948
		bool IsJapaneseRegionRelease { get; }

		// Token: 0x06001B25 RID: 6949
		void EarlyInitialize();

		// Token: 0x06001B26 RID: 6950
		void Initialize();

		// Token: 0x06001B27 RID: 6951
		void Update();

		// Token: 0x06001B28 RID: 6952
		void Shutdown();

		// Token: 0x06001B29 RID: 6953
		void DebugInfo();

		// Token: 0x06001B2A RID: 6954
		bool RetroactiveAchievementsAllowed();

		// Token: 0x06001B2B RID: 6955
		void GetAchievement(string achieve);

		// Token: 0x06001B2C RID: 6956
		void ResetAchievements();

		// Token: 0x06001B2D RID: 6957
		string FilterDirtyWords(string words);

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06001B2E RID: 6958
		string Name { get; }

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06001B2F RID: 6959
		SDKNetHelper Networking { get; }

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06001B30 RID: 6960
		bool ConnectionFinished { get; }

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06001B31 RID: 6961
		int ConnectionProgress { get; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06001B32 RID: 6962
		bool HasOverlay { get; }
	}
}
