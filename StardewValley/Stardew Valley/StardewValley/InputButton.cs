using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000F0 RID: 240
	public struct InputButton
	{
		// Token: 0x0600139C RID: 5020 RVA: 0x000F03AC File Offset: 0x000EE5AC
		public InputButton(Keys key)
		{
			this.key = key;
			this.mouseLeft = false;
			this.mouseRight = false;
		}

		// Token: 0x0600139D RID: 5021 RVA: 0x000F03C3 File Offset: 0x000EE5C3
		public InputButton(bool mouseLeft)
		{
			this.key = Keys.None;
			this.mouseLeft = mouseLeft;
			this.mouseRight = !mouseLeft;
		}

		// Token: 0x0600139E RID: 5022 RVA: 0x000F03E0 File Offset: 0x000EE5E0
		public override string ToString()
		{
			if (this.mouseLeft)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Left-Click");
			}
			if (this.mouseRight)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Right-Click");
			}
			switch (this.key)
			{
			case Keys.D0:
				return "0";
			case Keys.D1:
				return "1";
			case Keys.D2:
				return "2";
			case Keys.D3:
				return "3";
			case Keys.D4:
				return "4";
			case Keys.D5:
				return "5";
			case Keys.D6:
				return "6";
			case Keys.D7:
				return "7";
			case Keys.D8:
				return "8";
			case Keys.D9:
				return "9";
			default:
			{
				string retVal = this.key.ToString().Replace("Oem", "");
				if (Game1.content.LoadString("Strings\\StringsFromCSFiles:" + this.key.ToString().Replace("Oem", "")) != "Strings\\StringsFromCSFiles:" + this.key.ToString().Replace("Oem", ""))
				{
					retVal = Game1.content.LoadString("Strings\\StringsFromCSFiles:" + this.key.ToString().Replace("Oem", ""));
				}
				return retVal;
			}
			}
		}

		// Token: 0x04000BE8 RID: 3048
		public Keys key;

		// Token: 0x04000BE9 RID: 3049
		public bool mouseLeft;

		// Token: 0x04000BEA RID: 3050
		public bool mouseRight;
	}
}
