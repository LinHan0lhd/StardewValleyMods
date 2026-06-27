using System;
using System.Globalization;
using System.Text;

namespace StardewValley
{
	// Token: 0x02000109 RID: 265
	public static class StringBuilderFormatEx
	{
		// Token: 0x0600152C RID: 5420 RVA: 0x000F96B8 File Offset: 0x000F78B8
		public static bool StringsEqual(this StringBuilder sb, string value)
		{
			if (sb == null != (value == null))
			{
				return false;
			}
			if (value == null)
			{
				return true;
			}
			if (sb.Length != value.Length)
			{
				return false;
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] != sb[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600152D RID: 5421 RVA: 0x000F970A File Offset: 0x000F790A
		private static char[] _getBuffer(int len)
		{
			if (StringBuilderFormatEx._buffer == null || StringBuilderFormatEx._buffer.Length < len)
			{
				StringBuilderFormatEx._buffer = new char[len];
			}
			return StringBuilderFormatEx._buffer;
		}

		// Token: 0x0600152E RID: 5422 RVA: 0x000F9730 File Offset: 0x000F7930
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, StringBuilder value)
		{
			int len = value.Length;
			char[] buff = StringBuilderFormatEx._getBuffer(len);
			value.CopyTo(0, buff, 0, len);
			stringBuilder.Append(buff, 0, len);
			return stringBuilder;
		}

		// Token: 0x0600152F RID: 5423 RVA: 0x000F9760 File Offset: 0x000F7960
		static StringBuilderFormatEx()
		{
			StringBuilderFormatEx.Init();
		}

		// Token: 0x06001530 RID: 5424 RVA: 0x000F977E File Offset: 0x000F797E
		public static void Init()
		{
		}

		// Token: 0x06001531 RID: 5425 RVA: 0x000F9780 File Offset: 0x000F7980
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar, uint baseVal)
		{
			uint length = 0U;
			uint lengthCalc = uintVal;
			do
			{
				lengthCalc /= baseVal;
				length += 1U;
			}
			while (lengthCalc > 0U);
			stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));
			int strpos = stringBuilder.Length;
			while (length > 0U)
			{
				strpos--;
				stringBuilder[strpos] = StringBuilderFormatEx.MsDigits[(int)(uintVal % baseVal)];
				uintVal /= baseVal;
				length -= 1U;
			}
			return stringBuilder;
		}

		// Token: 0x06001532 RID: 5426 RVA: 0x000F97DA File Offset: 0x000F79DA
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal)
		{
			stringBuilder.AppendEx(uintVal, 0U, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x06001533 RID: 5427 RVA: 0x000F97EA File Offset: 0x000F79EA
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount)
		{
			stringBuilder.AppendEx(uintVal, padAmount, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x06001534 RID: 5428 RVA: 0x000F97FA File Offset: 0x000F79FA
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(uintVal, padAmount, padChar, 10U);
			return stringBuilder;
		}

		// Token: 0x06001535 RID: 5429 RVA: 0x000F980C File Offset: 0x000F7A0C
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar, uint baseVal)
		{
			if (intVal < 0)
			{
				stringBuilder.Append('-');
				uint uintVal = (uint)(-1 - intVal + 1);
				stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
			}
			else
			{
				stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
			}
			return stringBuilder;
		}

		// Token: 0x06001536 RID: 5430 RVA: 0x000F9847 File Offset: 0x000F7A47
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal)
		{
			stringBuilder.AppendEx(intVal, 0U, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x000F9857 File Offset: 0x000F7A57
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount)
		{
			stringBuilder.AppendEx(intVal, padAmount, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x000F9867 File Offset: 0x000F7A67
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10U);
			return stringBuilder;
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x000F9878 File Offset: 0x000F7A78
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar, uint baseVal)
		{
			uint length = 0U;
			ulong lengthCalc = uintVal;
			do
			{
				lengthCalc /= (ulong)baseVal;
				length += 1U;
			}
			while (lengthCalc > 0UL);
			stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));
			int strpos = stringBuilder.Length;
			while (length > 0U)
			{
				strpos--;
				stringBuilder[strpos] = StringBuilderFormatEx.MsDigits[(int)(checked((IntPtr)(uintVal % unchecked((ulong)baseVal))))];
				uintVal /= (ulong)baseVal;
				length -= 1U;
			}
			return stringBuilder;
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x000F98D7 File Offset: 0x000F7AD7
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal)
		{
			stringBuilder.AppendEx(uintVal, 0U, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x000F98E7 File Offset: 0x000F7AE7
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount)
		{
			stringBuilder.AppendEx(uintVal, padAmount, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x000F98F7 File Offset: 0x000F7AF7
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(uintVal, padAmount, padChar, 10U);
			return stringBuilder;
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x000F9908 File Offset: 0x000F7B08
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar, uint baseVal)
		{
			if (intVal < 0L)
			{
				stringBuilder.Append('-');
				uint uintVal = uint.MaxValue - (uint)intVal + 1U;
				stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
			}
			else
			{
				stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
			}
			return stringBuilder;
		}

		// Token: 0x0600153E RID: 5438 RVA: 0x000F9946 File Offset: 0x000F7B46
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal)
		{
			stringBuilder.AppendEx(intVal, 0U, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x0600153F RID: 5439 RVA: 0x000F9956 File Offset: 0x000F7B56
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount)
		{
			stringBuilder.AppendEx(intVal, padAmount, '0', 10U);
			return stringBuilder;
		}

		// Token: 0x06001540 RID: 5440 RVA: 0x000F9966 File Offset: 0x000F7B66
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10U);
			return stringBuilder;
		}

		// Token: 0x06001541 RID: 5441 RVA: 0x000F9978 File Offset: 0x000F7B78
		public static StringBuilder AppendEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount, char padChar)
		{
			if (decimalPlaces == 0U)
			{
				int intVal;
				if (floatVal >= 0f)
				{
					intVal = (int)(floatVal + 0.5f);
				}
				else
				{
					intVal = (int)(floatVal - 0.5f);
				}
				stringBuilder.AppendEx(intVal, padAmount, padChar, 10U);
			}
			else
			{
				int intPart = (int)floatVal;
				stringBuilder.AppendEx(intPart, padAmount, padChar, 10U);
				stringBuilder.Append('.');
				float remainder = Math.Abs(floatVal - (float)intPart);
				int i = 0;
				while ((long)i < (long)((ulong)decimalPlaces))
				{
					remainder *= 10f;
					i++;
				}
				stringBuilder.AppendEx((int)remainder, decimalPlaces, '0', 10U);
			}
			return stringBuilder;
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x000F99FB File Offset: 0x000F7BFB
		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal)
		{
			stringBuilder.AppendEx(floatVal, 5U, 0U, '0');
			return stringBuilder;
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x000F9A0A File Offset: 0x000F7C0A
		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, 0U, '0');
			return stringBuilder;
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x000F9A19 File Offset: 0x000F7C19
		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, padAmount, '0');
			return stringBuilder;
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x000F9A28 File Offset: 0x000F7C28
		public static StringBuilder AppendFormatEx<TA>(this StringBuilder stringBuilder, string formatString, TA arg1) where TA : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, 0, 0, 0, 0);
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x000F9A36 File Offset: 0x000F7C36
		public static StringBuilder AppendFormatEx<TA, TB>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2) where TA : IConvertible where TB : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, 0, 0, 0);
		}

		// Token: 0x06001547 RID: 5447 RVA: 0x000F9A44 File Offset: 0x000F7C44
		public static StringBuilder AppendFormatEx<TA, TB, TC>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3) where TA : IConvertible where TB : IConvertible where TC : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, 0, 0);
		}

		// Token: 0x06001548 RID: 5448 RVA: 0x000F9A53 File Offset: 0x000F7C53
		public static StringBuilder AppendFormatEx<TA, TB, TC, TD>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, arg4, 0);
		}

		// Token: 0x06001549 RID: 5449 RVA: 0x000F9A64 File Offset: 0x000F7C64
		public static StringBuilder AppendFormatEx<TA, TB, TC, TD, TE>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible where TE : IConvertible
		{
			int verbatimRangeStart = 0;
			for (int index = 0; index < formatString.Length; index++)
			{
				if (formatString[index] == '{')
				{
					if (verbatimRangeStart < index)
					{
						stringBuilder.Append(formatString, verbatimRangeStart, index - verbatimRangeStart);
					}
					uint baseValue = 10U;
					uint padding = 0U;
					uint decimalPlaces = 5U;
					index++;
					char formatChar = formatString[index];
					if (formatChar == '{')
					{
						stringBuilder.Append('{');
						index++;
					}
					else
					{
						index++;
						if (formatString[index] == ':')
						{
							index++;
							while (formatString[index] == '0')
							{
								index++;
								padding += 1U;
							}
							char c = formatString[index];
							if (c != '.')
							{
								if (c == 'X')
								{
									index++;
									baseValue = 16U;
									if (formatString[index] >= '0' && formatString[index] <= '9')
									{
										padding = (uint)(formatString[index] - '0');
										index++;
									}
								}
							}
							else
							{
								index++;
								decimalPlaces = 0U;
								while (formatString[index] == '0')
								{
									index++;
									decimalPlaces += 1U;
								}
							}
						}
						while (formatString[index] != '}')
						{
							index++;
						}
						switch (formatChar)
						{
						case '0':
							stringBuilder.AppendFormatValue(arg1, padding, baseValue, decimalPlaces);
							break;
						case '1':
							stringBuilder.AppendFormatValue(arg2, padding, baseValue, decimalPlaces);
							break;
						case '2':
							stringBuilder.AppendFormatValue(arg3, padding, baseValue, decimalPlaces);
							break;
						case '3':
							stringBuilder.AppendFormatValue(arg4, padding, baseValue, decimalPlaces);
							break;
						case '4':
							stringBuilder.AppendFormatValue(arg5, padding, baseValue, decimalPlaces);
							break;
						}
					}
					verbatimRangeStart = index + 1;
				}
			}
			if (verbatimRangeStart < formatString.Length)
			{
				stringBuilder.Append(formatString, verbatimRangeStart, formatString.Length - verbatimRangeStart);
			}
			return stringBuilder;
		}

		// Token: 0x0600154A RID: 5450 RVA: 0x000F9BF4 File Offset: 0x000F7DF4
		private static void AppendFormatValue<T>(this StringBuilder stringBuilder, T arg, uint padding, uint baseValue, uint decimalPlaces) where T : IConvertible
		{
			TypeCode typeCode;
			if (arg == null)
			{
				if (arg is string)
				{
					typeCode = TypeCode.String;
				}
				else
				{
					typeCode = TypeCode.Object;
				}
			}
			else
			{
				typeCode = arg.GetTypeCode();
			}
			switch (typeCode)
			{
			case TypeCode.Object:
			case TypeCode.Boolean:
				stringBuilder.Append(Convert.ToString(arg));
				return;
			case TypeCode.DBNull:
			case TypeCode.Char:
			case TypeCode.Decimal:
			case TypeCode.DateTime:
			case (TypeCode)17:
				break;
			case TypeCode.SByte:
			{
				ref T ptr = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr = ref t;
				}
				stringBuilder.AppendEx(ptr.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.Byte:
			{
				ref T ptr2 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr2 = ref t;
				}
				stringBuilder.AppendEx(ptr2.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.Int16:
			{
				ref T ptr3 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr3 = ref t;
				}
				stringBuilder.AppendEx(ptr3.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.UInt16:
			{
				ref T ptr4 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr4 = ref t;
				}
				stringBuilder.AppendEx(ptr4.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.Int32:
			{
				ref T ptr5 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr5 = ref t;
				}
				stringBuilder.AppendEx(ptr5.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.UInt32:
			{
				ref T ptr6 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr6 = ref t;
				}
				stringBuilder.AppendEx(ptr6.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.Int64:
			{
				ref T ptr7 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr7 = ref t;
				}
				stringBuilder.AppendEx(ptr7.ToInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.UInt64:
			{
				ref T ptr8 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr8 = ref t;
				}
				stringBuilder.AppendEx(ptr8.ToUInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				return;
			}
			case TypeCode.Single:
			case TypeCode.Double:
			{
				ref T ptr9 = ref arg;
				if (default(T) == null)
				{
					T t = arg;
					ptr9 = ref t;
				}
				stringBuilder.AppendEx(ptr9.ToSingle(NumberFormatInfo.CurrentInfo), decimalPlaces, padding, '0');
				return;
			}
			case TypeCode.String:
				stringBuilder.Append(arg);
				break;
			default:
				return;
			}
		}

		// Token: 0x04000D7C RID: 3452
		private static readonly char[] MsDigits = new char[]
		{
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F'
		};

		// Token: 0x04000D7D RID: 3453
		private const uint MsDefaultDecimalPlaces = 5U;

		// Token: 0x04000D7E RID: 3454
		private const char MsDefaultPadChar = '0';

		// Token: 0x04000D7F RID: 3455
		private static char[] _buffer;
	}
}
