using System;
using StardewValley;

namespace Netcode
{
	// Token: 0x02000052 RID: 82
	internal static class NetHelper
	{
		// Token: 0x0600034B RID: 843 RVA: 0x00010A7A File Offset: 0x0000EC7A
		public static void LogWarning(string message)
		{
			Game1.log.Warn(message);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00010A87 File Offset: 0x0000EC87
		public static void LogVerbose(string message)
		{
			Game1.log.Verbose(message);
		}
	}
}
