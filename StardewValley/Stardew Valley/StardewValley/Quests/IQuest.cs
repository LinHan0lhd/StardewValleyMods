using System;
using System.Collections.Generic;

namespace StardewValley.Quests
{
	// Token: 0x02000193 RID: 403
	public interface IQuest
	{
		// Token: 0x06001CEF RID: 7407
		string GetName();

		// Token: 0x06001CF0 RID: 7408
		string GetDescription();

		// Token: 0x06001CF1 RID: 7409
		List<string> GetObjectiveDescriptions();

		// Token: 0x06001CF2 RID: 7410
		bool CanBeCancelled();

		// Token: 0x06001CF3 RID: 7411
		void MarkAsViewed();

		// Token: 0x06001CF4 RID: 7412
		bool ShouldDisplayAsNew();

		// Token: 0x06001CF5 RID: 7413
		bool ShouldDisplayAsComplete();

		// Token: 0x06001CF6 RID: 7414
		bool IsTimedQuest();

		// Token: 0x06001CF7 RID: 7415
		int GetDaysLeft();

		// Token: 0x06001CF8 RID: 7416
		bool IsHidden();

		// Token: 0x06001CF9 RID: 7417
		bool HasReward();

		// Token: 0x06001CFA RID: 7418
		bool HasMoneyReward();

		// Token: 0x06001CFB RID: 7419
		int GetMoneyReward();

		// Token: 0x06001CFC RID: 7420
		void OnMoneyRewardClaimed();

		// Token: 0x06001CFD RID: 7421
		bool OnLeaveQuestPage();
	}
}
