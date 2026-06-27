using System;

namespace StardewValley.Util
{
	// Token: 0x0200011E RID: 286
	public class ToolSpamInputSimulator : IInputSimulator
	{
		// Token: 0x060017C1 RID: 6081 RVA: 0x00112123 File Offset: 0x00110323
		public void SimulateInput(ref bool actionButtonPressed, ref bool switchToolButtonPressed, ref bool useToolButtonPressed, ref bool useToolButtonReleased, ref bool addItemToInventoryButtonPressed, ref bool cancelButtonPressed, ref bool moveUpPressed, ref bool moveRightPressed, ref bool moveLeftPressed, ref bool moveDownPressed, ref bool moveUpReleased, ref bool moveRightReleased, ref bool moveLeftReleased, ref bool moveDownReleased, ref bool moveUpHeld, ref bool moveRightHeld, ref bool moveLeftHeld, ref bool moveDownHeld)
		{
			useToolButtonPressed = this.pressedLastFrame;
			useToolButtonReleased = !this.pressedLastFrame;
			this.pressedLastFrame = !this.pressedLastFrame;
		}

		// Token: 0x04000E4E RID: 3662
		private bool pressedLastFrame;
	}
}
