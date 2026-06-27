using System;

namespace StardewValley.SDKs.GogGalaxy.Internal
{
	// Token: 0x0200017C RID: 380
	public class Base36
	{
		// Token: 0x06001C4F RID: 7247 RVA: 0x00140F24 File Offset: 0x0013F124
		public static string Encode(ulong value)
		{
			string result = "";
			if (value == 0UL)
			{
				return "0";
			}
			while (value != 0UL)
			{
				int digit = (int)(value % 36UL);
				value /= 36UL;
				result = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[digit].ToString() + result;
			}
			return result;
		}

		// Token: 0x06001C50 RID: 7248 RVA: 0x00140F6C File Offset: 0x0013F16C
		public static ulong Decode(string value)
		{
			value = value.ToUpper();
			ulong result = 0UL;
			foreach (char ch in value)
			{
				result *= 36UL;
				int digit = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(ch);
				if (digit == -1)
				{
					throw new FormatException(value);
				}
				result += (ulong)((long)digit);
			}
			return result;
		}

		// Token: 0x04001116 RID: 4374
		private const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		// Token: 0x04001117 RID: 4375
		private const ulong Base = 36UL;
	}
}
