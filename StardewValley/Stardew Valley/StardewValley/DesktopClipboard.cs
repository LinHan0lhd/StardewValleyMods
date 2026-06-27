using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using StardewValley.NativeClipboard;
using TextCopy;

namespace StardewValley
{
	// Token: 0x02000099 RID: 153
	public class DesktopClipboard
	{
		// Token: 0x060006B8 RID: 1720 RVA: 0x00025C88 File Offset: 0x00023E88
		public static bool GetText(ref string output)
		{
			output = SdlClipboard.GetText();
			if (output != null)
			{
				return true;
			}
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				output = "";
				output = ClipboardService.GetText();
				return true;
			}
			return DesktopClipboard.externalGetText("xclip", "-o", ref output) || DesktopClipboard.externalGetText("pbpaste", "", ref output);
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x00025CE8 File Offset: 0x00023EE8
		public static bool SetText(string text)
		{
			if (text == null)
			{
				text = "";
			}
			if (SdlClipboard.SetText(text))
			{
				return true;
			}
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				ClipboardService.SetText(text);
				return true;
			}
			return DesktopClipboard.externalSetText("xclip", "-selection clipboard", text) || DesktopClipboard.externalSetText("pbcopy", "", text);
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x00025D44 File Offset: 0x00023F44
		private static bool externalSetText(string executable, string arguments, string text)
		{
			ProcessStartInfo psi = new ProcessStartInfo(executable, arguments)
			{
				RedirectStandardInput = true,
				UseShellExecute = false
			};
			bool result;
			try
			{
				using (Process process = Process.Start(psi))
				{
					process.StandardInput.Write(text);
					process.StandardInput.Close();
					process.WaitForExit();
					result = (process.ExitCode == 0);
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x00025DC4 File Offset: 0x00023FC4
		private static bool externalGetText(string executable, string arguments, ref string output)
		{
			ProcessStartInfo psi = new ProcessStartInfo(executable, arguments)
			{
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			bool result;
			try
			{
				using (Process process = Process.Start(psi))
				{
					string temp = process.StandardOutput.ReadToEnd();
					process.StandardOutput.Close();
					process.WaitForExit();
					if (process.ExitCode == 0)
					{
						output = temp;
					}
					else
					{
						output = "";
					}
					result = true;
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0400034F RID: 847
		public const bool IsAvailable = true;
	}
}
