using System;

namespace StardewValley
{
	// Token: 0x020000BD RID: 189
	public class CharacterEventArgs : EventArgs
	{
		// Token: 0x06000D59 RID: 3417 RVA: 0x000922DC File Offset: 0x000904DC
		public CharacterEventArgs(char character, int lParam)
		{
			this.character = character;
			this.lParam = lParam;
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000D5A RID: 3418 RVA: 0x000922F2 File Offset: 0x000904F2
		public char Character
		{
			get
			{
				return this.character;
			}
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000D5B RID: 3419 RVA: 0x000922FA File Offset: 0x000904FA
		public int Param
		{
			get
			{
				return this.lParam;
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000D5C RID: 3420 RVA: 0x00092302 File Offset: 0x00090502
		public int RepeatCount
		{
			get
			{
				return this.lParam & 65535;
			}
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000D5D RID: 3421 RVA: 0x00092310 File Offset: 0x00090510
		public bool PreviousState
		{
			get
			{
				return (this.lParam & 1073741824) > 0;
			}
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000D5E RID: 3422 RVA: 0x00092321 File Offset: 0x00090521
		public bool TransitionState
		{
			get
			{
				return (this.lParam & int.MinValue) > 0;
			}
		}

		// Token: 0x040008F2 RID: 2290
		private readonly char character;

		// Token: 0x040008F3 RID: 2291
		private readonly int lParam;
	}
}
