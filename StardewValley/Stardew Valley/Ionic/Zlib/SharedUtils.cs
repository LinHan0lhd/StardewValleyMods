using System;
using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	// Token: 0x0200001A RID: 26
	internal class SharedUtils
	{
		// Token: 0x06000096 RID: 150 RVA: 0x00008F26 File Offset: 0x00007126
		public static int URShift(int number, int bits)
		{
			return (int)((uint)number >> bits);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00008F30 File Offset: 0x00007130
		public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
		{
			if (target.Length == 0)
			{
				return 0;
			}
			char[] charArray = new char[target.Length];
			int bytesRead = sourceTextReader.Read(charArray, start, count);
			if (bytesRead == 0)
			{
				return -1;
			}
			for (int index = start; index < start + bytesRead; index++)
			{
				target[index] = (byte)charArray[index];
			}
			return bytesRead;
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00008F71 File Offset: 0x00007171
		internal static byte[] ToByteArray(string sourceString)
		{
			return Encoding.UTF8.GetBytes(sourceString);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00008F7E File Offset: 0x0000717E
		internal static char[] ToCharArray(byte[] byteArray)
		{
			return Encoding.UTF8.GetChars(byteArray);
		}
	}
}
