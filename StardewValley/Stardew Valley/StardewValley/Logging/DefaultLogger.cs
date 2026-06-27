using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace StardewValley.Logging
{
	// Token: 0x020002BB RID: 699
	internal class DefaultLogger : IGameLogger
	{
		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06002D7B RID: 11643 RVA: 0x00239372 File Offset: 0x00237572
		private string LogPath
		{
			get
			{
				if (this._LogPath == null)
				{
					this._LogPath = Program.GetDebugLogPath();
				}
				return this._LogPath;
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06002D7C RID: 11644 RVA: 0x0023938D File Offset: 0x0023758D
		public bool ShouldWriteToConsole { get; }

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06002D7D RID: 11645 RVA: 0x00239395 File Offset: 0x00237595
		public bool ShouldWriteToLogFile { get; }

		// Token: 0x06002D7E RID: 11646 RVA: 0x0023939D File Offset: 0x0023759D
		public DefaultLogger(bool shouldWriteToConsole, bool shouldWriteToLogFile)
		{
			this.ShouldWriteToConsole = shouldWriteToConsole;
			this.ShouldWriteToLogFile = shouldWriteToLogFile;
			if (shouldWriteToLogFile)
			{
				this.WriteMessageToFile("");
			}
		}

		// Token: 0x06002D7F RID: 11647 RVA: 0x002393CC File Offset: 0x002375CC
		public void Verbose(string message)
		{
			this.LogImpl("Verbose", message, null);
		}

		// Token: 0x06002D80 RID: 11648 RVA: 0x002393DB File Offset: 0x002375DB
		public void Debug(string message)
		{
			this.LogImpl("Debug", message, null);
		}

		// Token: 0x06002D81 RID: 11649 RVA: 0x002393EA File Offset: 0x002375EA
		public void Info(string message)
		{
			this.LogImpl("Info", message, null);
		}

		// Token: 0x06002D82 RID: 11650 RVA: 0x002393F9 File Offset: 0x002375F9
		public void Warn(string message)
		{
			this.LogImpl("Warn", message, null);
		}

		// Token: 0x06002D83 RID: 11651 RVA: 0x00239408 File Offset: 0x00237608
		public void Error(string error, Exception exception)
		{
			this.LogImpl("Error", error, exception);
		}

		// Token: 0x06002D84 RID: 11652 RVA: 0x00239418 File Offset: 0x00237618
		private void WriteMessageToFile(string message)
		{
			if (this.LogPath == null)
			{
				return;
			}
			if (!this.StartedLogFile)
			{
				File.WriteAllText(this.LogPath, message);
				this.StartedLogFile = true;
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Starting log file at ");
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "yyyy-MM-dd HH:mm:ii");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			try
			{
				File.AppendAllText(this.LogPath, message);
			}
			catch (Exception ex)
			{
				if (this.ShouldWriteToConsole)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed writing to log file:\n");
					defaultInterpolatedStringHandler.AppendFormatted<Exception>(ex);
					Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
		}

		// Token: 0x06002D85 RID: 11653 RVA: 0x002394E4 File Offset: 0x002376E4
		private void LogImpl(string level, string message, Exception exception = null)
		{
			bool logToConsole = this.ShouldWriteToConsole;
			bool logToFile = this.ShouldWriteToLogFile;
			if (logToConsole || logToFile)
			{
				message = this.FormatLog(level, message, exception);
				if (logToConsole)
				{
					Console.WriteLine(message);
				}
				if (logToFile)
				{
					this.WriteMessageToFile(message);
				}
			}
		}

		// Token: 0x06002D86 RID: 11654 RVA: 0x00239524 File Offset: 0x00237724
		private string FormatLog(string level, string text, Exception exception = null)
		{
			StringBuilder message = this.MessageBuilder;
			string result;
			try
			{
				Game1 game = Game1.game1;
				int screenId = (game != null) ? game.instanceId : 0;
				StringBuilder stringBuilder = message.Append('[');
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "HH:mm:ss");
				appendInterpolatedStringHandler.AppendLiteral(" ");
				StringBuilder stringBuilder3 = stringBuilder2.Append(ref appendInterpolatedStringHandler).Append(level).Append(' ');
				string value;
				if (screenId != 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
					defaultInterpolatedStringHandler.AppendLiteral("screen");
					defaultInterpolatedStringHandler.AppendFormatted<int>(screenId);
					value = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					value = "game";
				}
				stringBuilder3.Append(value).Append("] ").Append(text).AppendLine();
				if (exception != null)
				{
					message.Append(exception).AppendLine();
				}
				result = message.ToString();
			}
			finally
			{
				message.Clear();
			}
			return result;
		}

		// Token: 0x04001F43 RID: 8003
		private readonly StringBuilder MessageBuilder = new StringBuilder();

		// Token: 0x04001F44 RID: 8004
		private string _LogPath;

		// Token: 0x04001F45 RID: 8005
		private bool StartedLogFile;
	}
}
