using System;
using System.IO;

namespace StardewValley
{
	// Token: 0x020000FF RID: 255
	internal static class LegacyShims
	{
		// Token: 0x06001463 RID: 5219 RVA: 0x000F6A61 File Offset: 0x000F4C61
		public static T[] EmptyArray<T>()
		{
			return Array.Empty<T>();
		}

		// Token: 0x06001464 RID: 5220 RVA: 0x000F6A68 File Offset: 0x000F4C68
		public static string[] SplitAndTrim(string str, char separator, StringSplitOptions options = StringSplitOptions.None)
		{
			return str.Split(separator, options | StringSplitOptions.TrimEntries);
		}

		// Token: 0x06001465 RID: 5221 RVA: 0x000F6A74 File Offset: 0x000F4C74
		public static string[] SplitAndTrim(string str, string separator, StringSplitOptions options = StringSplitOptions.None)
		{
			return str.Split(separator, options | StringSplitOptions.TrimEntries);
		}

		// Token: 0x06001466 RID: 5222 RVA: 0x000F6A80 File Offset: 0x000F4C80
		public static void MoveFileWithOverwrite(string sourceFilePath, string destFilePath)
		{
			File.Move(sourceFilePath, destFilePath, true);
		}
	}
}
