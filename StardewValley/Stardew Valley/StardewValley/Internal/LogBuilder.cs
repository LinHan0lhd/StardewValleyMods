using System;
using System.Text;

namespace StardewValley.Internal
{
	// Token: 0x02000314 RID: 788
	public class LogBuilder
	{
		// Token: 0x06003431 RID: 13361 RVA: 0x0029B92F File Offset: 0x00299B2F
		public LogBuilder(int indent = 0) : this(new StringBuilder(), indent)
		{
		}

		// Token: 0x06003432 RID: 13362 RVA: 0x0029B93D File Offset: 0x00299B3D
		public LogBuilder(StringBuilder log, int indent = 0)
		{
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			this.Log = log;
			this.Indent = indent;
		}

		// Token: 0x06003433 RID: 13363 RVA: 0x0029B962 File Offset: 0x00299B62
		public void AppendLine()
		{
			this.Log.AppendLine();
		}

		// Token: 0x06003434 RID: 13364 RVA: 0x0029B970 File Offset: 0x00299B70
		public void AppendLine(string message)
		{
			if (this.Indent > 0 && message.Length > 0)
			{
				message = message.PadLeft(message.Length + this.Indent, ' ');
			}
			this.Log.AppendLine(message);
		}

		// Token: 0x06003435 RID: 13365 RVA: 0x0029B9A8 File Offset: 0x00299BA8
		public LogBuilder GetIndentedLog(int indent = 3)
		{
			return new LogBuilder(this.Log, this.Indent + indent);
		}

		// Token: 0x04002237 RID: 8759
		public readonly StringBuilder Log;

		// Token: 0x04002238 RID: 8760
		public readonly int Indent;
	}
}
