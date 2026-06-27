using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using StardewValley.Network.Compress;
using StardewValley.SDKs;
using StardewValley.SDKs.Steam;

namespace StardewValley
{
	// Token: 0x020000F4 RID: 244
	public static class Program
	{
		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06001404 RID: 5124 RVA: 0x000F4024 File Offset: 0x000F2224
		internal static SDKHelper sdk
		{
			get
			{
				if (Program._sdk == null)
				{
					Program._sdk = new SteamHelper();
					if (Program._sdk == null)
					{
						Program._sdk = new NullSDKHelper();
					}
				}
				return Program._sdk;
			}
		}

		// Token: 0x06001405 RID: 5125 RVA: 0x000F4050 File Offset: 0x000F2250
		public static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Program.GameTesterMode = true;
			AppDomain.CurrentDomain.UnhandledException += Program.handleException;
			using (GameRunner game = new GameRunner())
			{
				GameRunner.instance = game;
				game.Run();
			}
		}

		// Token: 0x06001406 RID: 5126 RVA: 0x000F40B8 File Offset: 0x000F22B8
		public static string GetLocalAppDataFolder(string subfolder = null, bool createIfMissing = true)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (!string.IsNullOrWhiteSpace(appDataPath))
				{
					string fullPath = (subfolder != null) ? Path.Combine(appDataPath, "StardewValley", subfolder) : Path.Combine(appDataPath, "StardewValley");
					if (createIfMissing)
					{
						Directory.CreateDirectory(fullPath);
					}
					return fullPath;
				}
			}
			return Program.GetAppDataFolder(subfolder, createIfMissing);
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x000F4114 File Offset: 0x000F2314
		public static string GetAppDataFolder(string subfolder = null, bool createIfMissing = true)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string fullPath = (subfolder != null) ? Path.Combine(appDataPath, "StardewValley", subfolder) : Path.Combine(appDataPath, "StardewValley");
			if (createIfMissing)
			{
				Directory.CreateDirectory(fullPath);
			}
			return fullPath;
		}

		// Token: 0x06001408 RID: 5128 RVA: 0x000F4151 File Offset: 0x000F2351
		public static string GetDebugLogPath()
		{
			return Path.Combine(Program.GetLocalAppDataFolder("ErrorLogs", true), "game-latest.txt");
		}

		// Token: 0x06001409 RID: 5129 RVA: 0x000F4168 File Offset: 0x000F2368
		public static string GetSavesFolder()
		{
			return Program.GetAppDataFolder("Saves", true);
		}

		// Token: 0x0600140A RID: 5130 RVA: 0x000F4178 File Offset: 0x000F2378
		public static string WriteLog(Program.LogType logType, string message, bool append = false)
		{
			Farmer player = Game1.player;
			string filename = ((player != null) ? player.Name : null) ?? "NullPlayer";
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			filename = string.Join("-", filename.Split(invalidFileNameChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
			filename = string.Join("-", filename.Split(new char[0], StringSplitOptions.RemoveEmptyEntries));
			string logFolderName;
			if (logType == Program.LogType.Disconnect)
			{
				logFolderName = "DisconnectLogs";
				string str = filename;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(DateTime.Now.Month);
				defaultInterpolatedStringHandler.AppendLiteral("-");
				defaultInterpolatedStringHandler.AppendFormatted<int>(DateTime.Now.Day);
				defaultInterpolatedStringHandler.AppendLiteral(".txt");
				filename = str + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				logFolderName = "ErrorLogs";
				string str2 = filename;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(Game1.uniqueIDForThisGame);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>((ulong)((Game1.player == null) ? ((long)Game1.random.Next(999999)) : ((long)Game1.player.millisecondsPlayed)));
				defaultInterpolatedStringHandler.AppendLiteral(".txt");
				filename = str2 + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string logFolderPath = Program.GetLocalAppDataFolder(logFolderName, true);
			if (logFolderPath == null)
			{
				Game1.log.Error("WriteLog failed on GetLocalAppDataFolder(\"" + logFolderName + "\")", null);
				return null;
			}
			string fullFilePath = Path.Combine(logFolderPath, filename);
			try
			{
				if (append)
				{
					File.AppendAllText(fullFilePath, message + Environment.NewLine);
				}
				else
				{
					File.WriteAllText(fullFilePath, message);
				}
			}
			catch (Exception e)
			{
				Game1.log.Error("WriteLog failed with exception:", e);
				return null;
			}
			return fullFilePath;
		}

		// Token: 0x0600140B RID: 5131 RVA: 0x000F4340 File Offset: 0x000F2540
		public static void AppendDiagnostics(StringBuilder sb)
		{
			sb.AppendLine("Game Version: " + Game1.GetVersionString());
			try
			{
				if (Program.sdk != null)
				{
					sb.AppendLine("SDK Helper: " + Program.sdk.GetType().Name);
				}
				sb.AppendLine("Game Language: " + LocalizedContentManager.CurrentLanguageCode.ToString());
				try
				{
					sb.AppendLine("GPU: " + Game1.graphics.GraphicsDevice.Adapter.Description);
				}
				catch (Exception)
				{
					sb.AppendLine("GPU: Could not detect.");
				}
				sb.AppendLine("OS: " + Environment.OSVersion.Platform.ToString() + " " + Environment.OSVersion.VersionString);
				if (GameRunner.instance != null && GameRunner.instance.GetType().FullName.StartsWith("StardewModdingAPI."))
				{
					sb.AppendLine("Running SMAPI");
				}
				if (Game1.IsMultiplayer)
				{
					if (LocalMultiplayer.IsLocalMultiplayer(false))
					{
						sb.AppendLine("Multiplayer (Split Screen)");
					}
					else if (Game1.IsMasterGame)
					{
						sb.AppendLine("Multiplayer (Host)");
					}
					else
					{
						sb.AppendLine("Multiplayer (Client)");
					}
				}
				if (Game1.options.gamepadControls)
				{
					sb.AppendLine("Playing on Controller");
				}
				sb.AppendLine(string.Concat(new string[]
				{
					"In-game Date: ",
					Game1.season.ToString(),
					" ",
					Game1.dayOfMonth.ToString(),
					" Y",
					Game1.year.ToString(),
					" Time of Day: ",
					Game1.timeOfDay.ToString()
				}));
				sb.AppendLine("Game Location: " + ((Game1.currentLocation == null) ? "null" : Game1.currentLocation.NameOrUniqueName));
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600140C RID: 5132 RVA: 0x000F4570 File Offset: 0x000F2770
		public static void handleException(object sender, UnhandledExceptionEventArgs args)
		{
			if (Program.handlingException || !Program.GameTesterMode)
			{
				return;
			}
			Game1.gameMode = 11;
			Program.handlingException = true;
			StringBuilder sb = new StringBuilder();
			if (args != null)
			{
				Exception e = (Exception)args.ExceptionObject;
				sb.AppendLine("Message: " + e.Message);
				StringBuilder stringBuilder = sb;
				string str = "InnerException: ";
				Exception innerException = e.InnerException;
				stringBuilder.AppendLine(str + ((innerException != null) ? innerException.ToString() : null));
				sb.AppendLine("Stack Trace: " + e.StackTrace);
				sb.AppendLine("");
			}
			Program.AppendDiagnostics(sb);
			Game1.errorMessage = sb.ToString();
			if (!Program.hasTriedToPrintLog)
			{
				Program.hasTriedToPrintLog = true;
				string successfulErrorPath = Program.WriteLog(Program.LogType.Error, Game1.errorMessage, false);
				if (successfulErrorPath != null)
				{
					Program.successfullyPrintedLog = true;
					Game1.errorMessage = string.Concat(new string[]
					{
						"(Error Report created at ",
						successfulErrorPath,
						")",
						Environment.NewLine,
						Game1.errorMessage
					});
				}
			}
			if (args == null)
			{
				return;
			}
			Game1.gameMode = 3;
		}

		// Token: 0x04000C8C RID: 3212
		public const int build_steam = 0;

		// Token: 0x04000C8D RID: 3213
		public const int build_gog = 1;

		// Token: 0x04000C8E RID: 3214
		public const int build_rail = 2;

		// Token: 0x04000C8F RID: 3215
		public const int build_gdk = 3;

		// Token: 0x04000C90 RID: 3216
		public static bool GameTesterMode = false;

		// Token: 0x04000C91 RID: 3217
		public static bool releaseBuild = true;

		// Token: 0x04000C92 RID: 3218
		public static bool enableCheats = !Program.releaseBuild;

		// Token: 0x04000C93 RID: 3219
		public const int buildType = 0;

		// Token: 0x04000C94 RID: 3220
		private static SDKHelper _sdk;

		// Token: 0x04000C95 RID: 3221
		internal static readonly INetCompression defaultCompression = new LZ4NetCompression();

		// Token: 0x04000C96 RID: 3222
		internal static INetCompression netCompression = Program.defaultCompression;

		// Token: 0x04000C97 RID: 3223
		public static Game1 gamePtr;

		// Token: 0x04000C98 RID: 3224
		public static bool handlingException;

		// Token: 0x04000C99 RID: 3225
		public static bool hasTriedToPrintLog;

		// Token: 0x04000C9A RID: 3226
		public static bool successfullyPrintedLog;

		// Token: 0x020004D2 RID: 1234
		public enum LogType
		{
			// Token: 0x04002991 RID: 10641
			Error,
			// Token: 0x04002992 RID: 10642
			Disconnect
		}
	}
}
