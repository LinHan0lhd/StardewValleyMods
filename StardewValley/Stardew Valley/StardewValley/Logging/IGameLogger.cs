using System;

namespace StardewValley.Logging
{
	// Token: 0x020002BC RID: 700
	public interface IGameLogger
	{
		// Token: 0x06002D87 RID: 11655
		void Verbose(string message);

		// Token: 0x06002D88 RID: 11656
		void Debug(string message);

		// Token: 0x06002D89 RID: 11657
		void Info(string message);

		// Token: 0x06002D8A RID: 11658
		void Warn(string message);

		// Token: 0x06002D8B RID: 11659
		void Error(string error, Exception exception = null);
	}
}
