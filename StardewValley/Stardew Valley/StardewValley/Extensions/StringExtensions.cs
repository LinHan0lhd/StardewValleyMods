using System;

namespace StardewValley.Extensions
{
	// Token: 0x02000321 RID: 801
	public static class StringExtensions
	{
		// Token: 0x06003476 RID: 13430 RVA: 0x0029CED8 File Offset: 0x0029B0D8
		public static bool ContainsIgnoreCase(this string str, string value)
		{
			return str != null && str.Contains(value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06003477 RID: 13431 RVA: 0x0029CEE7 File Offset: 0x0029B0E7
		public static bool EqualsIgnoreCase(this string str, string value)
		{
			return string.Equals(str, value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06003478 RID: 13432 RVA: 0x0029CEF1 File Offset: 0x0029B0F1
		public static int IndexOfIgnoreCase(this string str, string value)
		{
			if (str == null)
			{
				return -1;
			}
			return str.IndexOf(value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06003479 RID: 13433 RVA: 0x0029CF00 File Offset: 0x0029B100
		public static bool StartsWithIgnoreCase(this string str, string value)
		{
			return str != null && str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600347A RID: 13434 RVA: 0x0029CF0F File Offset: 0x0029B10F
		public static bool EndsWithIgnoreCase(this string str, string value)
		{
			return str != null && str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
		}
	}
}
