using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000C1 RID: 193
	public static class KeyboardInput
	{
		// Token: 0x14000012 RID: 18
		// (add) Token: 0x06000D69 RID: 3433 RVA: 0x0009234C File Offset: 0x0009054C
		// (remove) Token: 0x06000D6A RID: 3434 RVA: 0x00092380 File Offset: 0x00090580
		public static event CharEnteredHandler CharEntered;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x06000D6B RID: 3435 RVA: 0x000923B4 File Offset: 0x000905B4
		// (remove) Token: 0x06000D6C RID: 3436 RVA: 0x000923E8 File Offset: 0x000905E8
		public static event KeyEventHandler KeyDown;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x06000D6D RID: 3437 RVA: 0x0009241C File Offset: 0x0009061C
		// (remove) Token: 0x06000D6E RID: 3438 RVA: 0x00092450 File Offset: 0x00090650
		public static event KeyEventHandler KeyUp;

		// Token: 0x06000D6F RID: 3439
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr ImmGetContext(IntPtr hWnd);

		// Token: 0x06000D70 RID: 3440
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

		// Token: 0x06000D71 RID: 3441
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		// Token: 0x06000D72 RID: 3442
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		// Token: 0x06000D73 RID: 3443 RVA: 0x00092484 File Offset: 0x00090684
		public static void Initialize(GameWindow window)
		{
			if (KeyboardInput.initialized)
			{
				throw new InvalidOperationException("TextInput.Initialize can only be called once!");
			}
			KeyboardInput.hookProcDelegate = new KeyboardInput.WndProc(KeyboardInput.HookProc);
			KeyboardInput.prevWndProc = (IntPtr)KeyboardInput.SetWindowLong(window.Handle, -4, (int)Marshal.GetFunctionPointerForDelegate<KeyboardInput.WndProc>(KeyboardInput.hookProcDelegate));
			KeyboardInput.hIMC = KeyboardInput.ImmGetContext(window.Handle);
			KeyboardInput.initialized = true;
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x000924F0 File Offset: 0x000906F0
		private static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr returnCode = KeyboardInput.CallWindowProc(KeyboardInput.prevWndProc, hWnd, msg, wParam, lParam);
			if (msg <= 135U)
			{
				if (msg != 81U)
				{
					if (msg == 135U)
					{
						returnCode = (IntPtr)(returnCode.ToInt32() | 4);
					}
				}
				else
				{
					KeyboardInput.ImmAssociateContext(hWnd, KeyboardInput.hIMC);
					returnCode = (IntPtr)1;
				}
			}
			else
			{
				switch (msg)
				{
				case 256U:
				{
					KeyEventHandler keyDown = KeyboardInput.KeyDown;
					if (keyDown != null)
					{
						keyDown(null, new KeyEventArgs((Keys)((int)wParam)));
					}
					break;
				}
				case 257U:
				{
					KeyEventHandler keyUp = KeyboardInput.KeyUp;
					if (keyUp != null)
					{
						keyUp(null, new KeyEventArgs((Keys)((int)wParam)));
					}
					break;
				}
				case 258U:
				{
					CharEnteredHandler charEntered = KeyboardInput.CharEntered;
					if (charEntered != null)
					{
						charEntered(null, new CharacterEventArgs((char)((int)wParam), lParam.ToInt32()));
					}
					break;
				}
				default:
					if (msg == 641U)
					{
						if (wParam.ToInt32() == 1)
						{
							KeyboardInput.ImmAssociateContext(hWnd, KeyboardInput.hIMC);
						}
					}
					break;
				}
			}
			return returnCode;
		}

		// Token: 0x040008F8 RID: 2296
		private static bool initialized;

		// Token: 0x040008F9 RID: 2297
		private static IntPtr prevWndProc;

		// Token: 0x040008FA RID: 2298
		private static KeyboardInput.WndProc hookProcDelegate;

		// Token: 0x040008FB RID: 2299
		private static IntPtr hIMC;

		// Token: 0x040008FC RID: 2300
		private const int GWL_WNDPROC = -4;

		// Token: 0x040008FD RID: 2301
		private const int WM_KEYDOWN = 256;

		// Token: 0x040008FE RID: 2302
		private const int WM_KEYUP = 257;

		// Token: 0x040008FF RID: 2303
		private const int WM_CHAR = 258;

		// Token: 0x04000900 RID: 2304
		private const int WM_IME_SETCONTEXT = 641;

		// Token: 0x04000901 RID: 2305
		private const int WM_INPUTLANGCHANGE = 81;

		// Token: 0x04000902 RID: 2306
		private const int WM_GETDLGCODE = 135;

		// Token: 0x04000903 RID: 2307
		private const int DLGC_WANTALLKEYS = 4;

		// Token: 0x0200046C RID: 1132
		// (Invoke) Token: 0x06003E29 RID: 15913
		private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
}
