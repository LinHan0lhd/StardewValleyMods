using System;

namespace StardewValley.Util
{
	// Token: 0x02000120 RID: 288
	public interface IInputSimulator
	{
		// Token: 0x060017C5 RID: 6085
		void SimulateInput(ref bool actionButtonPressed, ref bool switchToolButtonPressed, ref bool useToolButtonPressed, ref bool useToolButtonReleased, ref bool addItemToInventoryButtonPressed, ref bool cancelButtonPressed, ref bool moveUpPressed, ref bool moveRightPressed, ref bool moveLeftPressed, ref bool moveDownPressed, ref bool moveUpReleased, ref bool moveRightReleased, ref bool moveLeftReleased, ref bool moveDownReleased, ref bool moveUpHeld, ref bool moveRightHeld, ref bool moveLeftHeld, ref bool moveDownHeld);
	}
}
