using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000BE RID: 190
	public class KeyEventArgs : EventArgs
	{
		// Token: 0x06000D5F RID: 3423 RVA: 0x00092332 File Offset: 0x00090532
		public KeyEventArgs(Keys keyCode)
		{
			this.keyCode = keyCode;
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000D60 RID: 3424 RVA: 0x00092341 File Offset: 0x00090541
		public Keys KeyCode
		{
			get
			{
				return this.keyCode;
			}
		}

		// Token: 0x040008F4 RID: 2292
		private Keys keyCode;
	}
}
