using System;
using System.Runtime.InteropServices;

namespace StardewValley.NativeClipboard
{
	// Token: 0x0200020E RID: 526
	internal sealed class WindowsSdlClipboard : SdlClipboard
	{
		// Token: 0x0600230C RID: 8972
		[DllImport("SDL2.dll", CallingConvention = 2)]
		private static extern IntPtr SDL_GetClipboardText();

		// Token: 0x0600230D RID: 8973
		[DllImport("SDL2.dll", CallingConvention = 2)]
		private static extern int SDL_SetClipboardText(IntPtr text);

		// Token: 0x0600230E RID: 8974 RVA: 0x00178F61 File Offset: 0x00177161
		public WindowsSdlClipboard()
		{
			this.PlatformName = "Windows";
		}

		// Token: 0x0600230F RID: 8975 RVA: 0x00178F74 File Offset: 0x00177174
		protected override IntPtr GetTextImpl()
		{
			return WindowsSdlClipboard.SDL_GetClipboardText();
		}

		// Token: 0x06002310 RID: 8976 RVA: 0x00178F7B File Offset: 0x0017717B
		protected override int SetTextImpl(IntPtr text)
		{
			return WindowsSdlClipboard.SDL_SetClipboardText(text);
		}
	}
}
