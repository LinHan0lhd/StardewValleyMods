using System;
using System.Collections.Generic;

namespace StardewValley.Extensions
{
	// Token: 0x0200031F RID: 799
	public static class RandomExtensions
	{
		// Token: 0x0600346D RID: 13421 RVA: 0x0029CC93 File Offset: 0x0029AE93
		public static T Choose<T>(this Random random, T optionA, T optionB)
		{
			if (random.NextDouble() >= 0.5)
			{
				return optionB;
			}
			return optionA;
		}

		// Token: 0x0600346E RID: 13422 RVA: 0x0029CCAC File Offset: 0x0029AEAC
		public static T Choose<T>(this Random random, T optionA, T optionB, T optionC)
		{
			int num = random.Next(3);
			if (num == 0)
			{
				return optionA;
			}
			if (num != 1)
			{
				return optionC;
			}
			return optionB;
		}

		// Token: 0x0600346F RID: 13423 RVA: 0x0029CCD0 File Offset: 0x0029AED0
		public static T Choose<T>(this Random random, T optionA, T optionB, T optionC, T optionD)
		{
			switch (random.Next(4))
			{
			case 0:
				return optionA;
			case 1:
				return optionB;
			case 2:
				return optionC;
			default:
				return optionD;
			}
		}

		// Token: 0x06003470 RID: 13424 RVA: 0x0029CD04 File Offset: 0x0029AF04
		public static T Choose<T>(this Random random, params T[] options)
		{
			if (options == null || options.Length == 0)
			{
				return default(T);
			}
			return options[random.Next(options.Length)];
		}

		// Token: 0x06003471 RID: 13425 RVA: 0x0029CD34 File Offset: 0x0029AF34
		public static T ChooseFrom<T>(this Random random, IList<T> options)
		{
			if (options == null || options.Count <= 0)
			{
				return default(T);
			}
			return options[random.Next(options.Count)];
		}

		// Token: 0x06003472 RID: 13426 RVA: 0x0029CD69 File Offset: 0x0029AF69
		public static bool NextBool(this Random random)
		{
			return random.NextDouble() < 0.5;
		}

		// Token: 0x06003473 RID: 13427 RVA: 0x0029CD7C File Offset: 0x0029AF7C
		public static bool NextBool(this Random random, double chance)
		{
			return chance >= 1.0 || random.NextDouble() < chance;
		}

		// Token: 0x06003474 RID: 13428 RVA: 0x0029CD95 File Offset: 0x0029AF95
		public static bool NextBool(this Random random, float chance)
		{
			return chance >= 1f || random.NextDouble() < (double)chance;
		}
	}
}
