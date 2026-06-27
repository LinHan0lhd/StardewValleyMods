using System;
using System.Runtime.InteropServices;
using System.Text;

namespace StardewValley.NativeClipboard
{
	// Token: 0x0200020D RID: 525
	internal abstract class SdlClipboard
	{
		// Token: 0x06002305 RID: 8965 RVA: 0x00178D84 File Offset: 0x00176F84
		static SdlClipboard()
		{
			switch (SdlClipboard.Platform)
			{
			case ClipboardPlatformType.Linux:
				SdlClipboard.PlatformClipboard = new LinuxSdlClipboard();
				return;
			case ClipboardPlatformType.OSX:
				SdlClipboard.PlatformClipboard = new OsxSdlClipboard();
				return;
			case ClipboardPlatformType.Windows:
				SdlClipboard.PlatformClipboard = new WindowsSdlClipboard();
				return;
			default:
				SdlClipboard.PlatformClipboard = null;
				return;
			}
		}

		// Token: 0x06002306 RID: 8966 RVA: 0x00178DDC File Offset: 0x00176FDC
		public static string GetText()
		{
			if (SdlClipboard.PlatformClipboard == null)
			{
				return null;
			}
			IntPtr clipboardPtr;
			try
			{
				clipboardPtr = SdlClipboard.PlatformClipboard.GetTextImpl();
			}
			catch (Exception)
			{
				return null;
			}
			if (clipboardPtr == IntPtr.Zero)
			{
				return null;
			}
			int length = 0;
			while (Marshal.ReadByte(clipboardPtr, length) != 0)
			{
				length++;
			}
			if (length == 0)
			{
				return null;
			}
			byte[] stringBytes = new byte[length];
			Marshal.Copy(clipboardPtr, stringBytes, 0, length);
			string result;
			try
			{
				result = Encoding.UTF8.GetString(stringBytes, 0, length);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06002307 RID: 8967 RVA: 0x00178E6C File Offset: 0x0017706C
		public static bool SetText(string text)
		{
			if (SdlClipboard.PlatformClipboard == null)
			{
				return false;
			}
			if (text == null)
			{
				return false;
			}
			byte[] stringBytes = Encoding.UTF8.GetBytes(text);
			IntPtr stringPtr = Marshal.AllocHGlobal(stringBytes.Length + 1);
			bool result2;
			try
			{
				Marshal.Copy(stringBytes, 0, stringPtr, stringBytes.Length);
				Marshal.WriteByte(stringPtr, stringBytes.Length, 0);
				int result;
				try
				{
					result = SdlClipboard.PlatformClipboard.SetTextImpl(stringPtr);
				}
				catch (Exception)
				{
					return false;
				}
				result2 = (result == 0);
			}
			finally
			{
				Marshal.FreeHGlobal(stringPtr);
			}
			return result2;
		}

		// Token: 0x06002308 RID: 8968 RVA: 0x00178EF4 File Offset: 0x001770F4
		private static ClipboardPlatformType GetPlatformType()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return ClipboardPlatformType.Linux;
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return ClipboardPlatformType.OSX;
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return ClipboardPlatformType.Windows;
			}
			return ClipboardPlatformType.Unknown;
		}

		// Token: 0x06002309 RID: 8969 RVA: 0x00178F21 File Offset: 0x00177121
		protected virtual IntPtr GetTextImpl()
		{
			throw new NotImplementedException("GetClipboardText() for " + this.PlatformName + " is not provided on this platform!");
		}

		// Token: 0x0600230A RID: 8970 RVA: 0x00178F3D File Offset: 0x0017713D
		protected virtual int SetTextImpl(IntPtr text)
		{
			throw new NotImplementedException("SetClipboardText(...) for " + this.PlatformName + " is not provided on this platform!");
		}

		// Token: 0x040014C1 RID: 5313
		private static SdlClipboard PlatformClipboard;

		// Token: 0x040014C2 RID: 5314
		protected string PlatformName;

		// Token: 0x040014C3 RID: 5315
		internal static readonly ClipboardPlatformType Platform = SdlClipboard.GetPlatformType();
	}
}
