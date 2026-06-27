using System;
using System.Text;

namespace ContentManifest.Internal
{
	// Token: 0x0200007B RID: 123
	internal class CHString : CHParsable
	{
		// Token: 0x06000485 RID: 1157 RVA: 0x0001565C File Offset: 0x0001385C
		public void Parse(CHJsonParserContext context)
		{
			if (context.JsonText[context.ReadHead] != '"')
			{
				throw new InvalidOperationException();
			}
			context.ReadHead++;
			int readHead = context.ReadHead;
			string jsonText = context.JsonText;
			StringBuilder sb = new StringBuilder();
			while (readHead < jsonText.Length)
			{
				char readChar = jsonText[readHead];
				if (readChar == '"')
				{
					this.RawString = sb.ToString();
					context.ReadHead = readHead + 1;
					return;
				}
				if (readChar != '\\')
				{
					sb.Append(readChar);
				}
				else
				{
					readHead++;
					if (readHead >= jsonText.Length)
					{
						throw new InvalidOperationException();
					}
					char escapeChar = jsonText[readHead];
					if (escapeChar <= '\\')
					{
						if (escapeChar == '"' || escapeChar == '/' || escapeChar == '\\')
						{
							sb.Append(escapeChar);
						}
					}
					else if (escapeChar <= 'f')
					{
						if (escapeChar != 'b')
						{
							if (escapeChar == 'f')
							{
								sb.Append('\f');
							}
						}
						else
						{
							sb.Append('\b');
						}
					}
					else if (escapeChar != 'n')
					{
						switch (escapeChar)
						{
						case 'r':
							sb.Append('\r');
							break;
						case 't':
							sb.Append('\t');
							break;
						case 'u':
						{
							if (readHead + 4 >= jsonText.Length)
							{
								throw new InvalidOperationException();
							}
							string decodedString = char.ConvertFromUtf32(0 | (this.ParseHexChar(jsonText[readHead + 1]) & 15) << 12 | (this.ParseHexChar(jsonText[readHead + 2]) & 15) << 8 | (this.ParseHexChar(jsonText[readHead + 3]) & 15) << 4 | (this.ParseHexChar(jsonText[readHead + 4]) & 15));
							if (decodedString.Length != 1)
							{
								throw new InvalidOperationException();
							}
							sb.Append(decodedString[0]);
							readHead += 4;
							break;
						}
						}
					}
					else
					{
						sb.Append('\n');
					}
				}
				readHead++;
			}
			throw new InvalidOperationException();
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x0001584E File Offset: 0x00013A4E
		private int ParseHexChar(char hexChar)
		{
			if ('0' <= hexChar && hexChar < '9')
			{
				return (int)(hexChar - '0');
			}
			if ('a' <= hexChar && hexChar <= 'z')
			{
				return (int)(hexChar - 'a' + '\n');
			}
			if ('A' <= hexChar && hexChar <= 'Z')
			{
				return (int)(hexChar - 'A' + '\n');
			}
			throw new InvalidOperationException();
		}

		// Token: 0x040001AF RID: 431
		public string RawString = "";
	}
}
