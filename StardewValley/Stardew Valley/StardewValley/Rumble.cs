using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000F9 RID: 249
	[InstanceStatics]
	public static class Rumble
	{
		// Token: 0x06001430 RID: 5168 RVA: 0x000F48D0 File Offset: 0x000F2AD0
		public static void update(float milliseconds)
		{
			float rumble_amount = 0f;
			if (Rumble.isRumbling)
			{
				rumble_amount = Rumble.rumbleStrength;
				Rumble.rumbleTimerCurrent += milliseconds;
				if (Rumble.rumbleTimerCurrent > Rumble.rumbleTimerMax)
				{
					rumble_amount = 0f;
				}
				else if (Rumble.fade)
				{
					if (Rumble.rumbleTimerCurrent > Rumble.rumbleTimerMax - 1000f)
					{
						Rumble.rumbleDuringFade = Utility.Lerp(Rumble.maxRumbleDuringFade, 0f, (Rumble.rumbleTimerCurrent - (Rumble.rumbleTimerMax - 1000f)) / 1000f);
					}
					rumble_amount = Rumble.rumbleDuringFade;
				}
			}
			if (rumble_amount <= 0f)
			{
				rumble_amount = 0f;
				Rumble.isRumbling = false;
			}
			if ((double)rumble_amount > 1.0)
			{
				rumble_amount = 1f;
			}
			if (!Game1.options.gamepadControls || !Game1.options.rumble)
			{
				rumble_amount = 0f;
			}
			if (Game1.playerOneIndex == (PlayerIndex)(-1))
			{
				return;
			}
			GamePad.SetVibration(Game1.playerOneIndex, rumble_amount, rumble_amount);
		}

		// Token: 0x06001431 RID: 5169 RVA: 0x000F49B5 File Offset: 0x000F2BB5
		public static void stopRumbling()
		{
			Rumble.rumbleStrength = 0f;
			Rumble.isRumbling = false;
		}

		// Token: 0x06001432 RID: 5170 RVA: 0x000F49C7 File Offset: 0x000F2BC7
		public static void rumble(float leftPower, float rightPower, float milliseconds)
		{
			Rumble.rumble(leftPower, milliseconds);
		}

		// Token: 0x06001433 RID: 5171 RVA: 0x000F49D0 File Offset: 0x000F2BD0
		public static void rumble(float power, float milliseconds)
		{
			if (!Rumble.isRumbling && Game1.options.gamepadControls && Game1.options.rumble)
			{
				Rumble.fade = false;
				Rumble.rumbleTimerCurrent = 0f;
				Rumble.rumbleTimerMax = milliseconds;
				Rumble.isRumbling = true;
				Rumble.rumbleStrength = power;
			}
		}

		// Token: 0x06001434 RID: 5172 RVA: 0x000F4A20 File Offset: 0x000F2C20
		public static void rumbleAndFade(float power, float milliseconds)
		{
			if (!Rumble.isRumbling && Game1.options.gamepadControls && Game1.options.rumble)
			{
				Rumble.rumbleTimerCurrent = 0f;
				Rumble.rumbleTimerMax = milliseconds;
				Rumble.isRumbling = true;
				Rumble.fade = true;
				Rumble.rumbleDuringFade = power;
				Rumble.maxRumbleDuringFade = power;
				Rumble.rumbleStrength = power;
			}
		}

		// Token: 0x04000CAB RID: 3243
		private static float rumbleStrength;

		// Token: 0x04000CAC RID: 3244
		private static float rumbleTimerMax;

		// Token: 0x04000CAD RID: 3245
		private static float rumbleTimerCurrent;

		// Token: 0x04000CAE RID: 3246
		private static float rumbleDuringFade;

		// Token: 0x04000CAF RID: 3247
		private static float maxRumbleDuringFade;

		// Token: 0x04000CB0 RID: 3248
		private static bool isRumbling;

		// Token: 0x04000CB1 RID: 3249
		private static bool fade;
	}
}
