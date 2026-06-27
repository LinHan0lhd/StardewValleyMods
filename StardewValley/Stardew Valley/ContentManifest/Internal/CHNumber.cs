using System;
using System.Globalization;
using System.Text;

namespace ContentManifest.Internal
{
	// Token: 0x02000078 RID: 120
	internal class CHNumber : CHParsable
	{
		// Token: 0x0600047D RID: 1149 RVA: 0x00015257 File Offset: 0x00013457
		public static bool IsValidPrefix(char prefixChar)
		{
			return prefixChar == '-' || ('0' <= prefixChar && prefixChar <= '9');
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00015270 File Offset: 0x00013470
		public void Parse(CHJsonParserContext context)
		{
			CHNumber.EnsureStringBuilderInitialized();
			CHNumber.DoubleSb.Clear();
			if (context.JsonText[context.ReadHead] == '-')
			{
				context.ReadHead++;
				CHNumber.DoubleSb.Append('-');
			}
			context.AssertReadHeadIsValid();
			char firstDigit = context.JsonText[context.ReadHead];
			if (firstDigit == '0')
			{
				context.ReadHead++;
				if (context.ReadHead < context.JsonText.Length)
				{
					char oneNineChar = context.JsonText[context.ReadHead];
					if ('1' <= oneNineChar && oneNineChar <= '9')
					{
						throw new InvalidOperationException();
					}
				}
				CHNumber.DoubleSb.Append('0');
			}
			else
			{
				if ('1' > firstDigit || firstDigit > '9')
				{
					throw new InvalidOperationException();
				}
				context.ReadHead++;
				CHNumber.DoubleSb.Append(firstDigit);
			}
			this.ParseDigits(context);
			if (context.ReadHead < context.JsonText.Length && context.JsonText[context.ReadHead] == '.')
			{
				context.ReadHead++;
				context.AssertReadHeadIsValid();
				CHNumber.DoubleSb.Append('.');
				this.ParseDigits(context);
			}
			if (context.ReadHead < context.JsonText.Length)
			{
				char expChar = context.JsonText[context.ReadHead];
				if (expChar == 'e' || expChar == 'E')
				{
					context.ReadHead++;
					context.AssertReadHeadIsValid();
					CHNumber.DoubleSb.Append('E');
					char signChar = context.JsonText[context.ReadHead];
					if (signChar == '-' || signChar == '+')
					{
						context.ReadHead++;
						context.AssertReadHeadIsValid();
						CHNumber.DoubleSb.Append(signChar);
					}
					this.ParseDigits(context);
				}
			}
			this.RawDouble = double.Parse(CHNumber.DoubleSb.ToString(), CultureInfo.InvariantCulture);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x00015460 File Offset: 0x00013660
		private void ParseDigits(CHJsonParserContext context)
		{
			string jsonText = context.JsonText;
			int readHead;
			for (readHead = context.ReadHead; readHead < jsonText.Length; readHead++)
			{
				char c = jsonText[readHead];
				if (c < '0' || c > '9')
				{
					break;
				}
				CHNumber.DoubleSb.Append(c);
			}
			context.ReadHead = readHead;
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x000154B0 File Offset: 0x000136B0
		private static void EnsureStringBuilderInitialized()
		{
			string maxLongString = Convert.ToString(long.MaxValue);
			CHNumber.DoubleSb = new StringBuilder("-".Length + maxLongString.Length + ".".Length + maxLongString.Length + "E".Length + "+".Length + maxLongString.Length);
		}

		// Token: 0x040001AC RID: 428
		private static StringBuilder DoubleSb;

		// Token: 0x040001AD RID: 429
		public double RawDouble;
	}
}
