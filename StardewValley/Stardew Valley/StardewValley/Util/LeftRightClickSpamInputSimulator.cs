using System;

namespace StardewValley.Util
{
	// Token: 0x0200011F RID: 287
	public class LeftRightClickSpamInputSimulator : IInputSimulator
	{
		// Token: 0x060017C3 RID: 6083 RVA: 0x00112150 File Offset: 0x00110350
		public void SimulateInput(ref bool actionButtonPressed, ref bool switchToolButtonPressed, ref bool useToolButtonPressed, ref bool useToolButtonReleased, ref bool addItemToInventoryButtonPressed, ref bool cancelButtonPressed, ref bool moveUpPressed, ref bool moveRightPressed, ref bool moveLeftPressed, ref bool moveDownPressed, ref bool moveUpReleased, ref bool moveRightReleased, ref bool moveLeftReleased, ref bool moveDownReleased, ref bool moveUpHeld, ref bool moveRightHeld, ref bool moveLeftHeld, ref bool moveDownHeld)
		{
			useToolButtonPressed = this.leftClickThisFrame;
			useToolButtonReleased = !this.leftClickThisFrame;
			actionButtonPressed = !this.leftClickThisFrame;
			this.leftClickThisFrame = !this.leftClickThisFrame;
		}

		// Token: 0x04000E4F RID: 3663
		private bool leftClickThisFrame;
	}
}
