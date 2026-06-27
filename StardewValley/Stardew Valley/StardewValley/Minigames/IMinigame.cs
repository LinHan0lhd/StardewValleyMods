using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames
{
	// Token: 0x0200023D RID: 573
	public interface IMinigame
	{
		// Token: 0x0600264A RID: 9802
		bool tick(GameTime time);

		// Token: 0x0600264B RID: 9803
		bool overrideFreeMouseMovement();

		// Token: 0x0600264C RID: 9804
		bool doMainGameUpdates();

		// Token: 0x0600264D RID: 9805
		void receiveLeftClick(int x, int y, bool playSound = true);

		// Token: 0x0600264E RID: 9806
		void leftClickHeld(int x, int y);

		// Token: 0x0600264F RID: 9807
		void receiveRightClick(int x, int y, bool playSound = true);

		// Token: 0x06002650 RID: 9808
		void releaseLeftClick(int x, int y);

		// Token: 0x06002651 RID: 9809
		void releaseRightClick(int x, int y);

		// Token: 0x06002652 RID: 9810
		void receiveKeyPress(Keys k);

		// Token: 0x06002653 RID: 9811
		void receiveKeyRelease(Keys k);

		// Token: 0x06002654 RID: 9812
		void draw(SpriteBatch b);

		// Token: 0x06002655 RID: 9813
		void changeScreenSize();

		// Token: 0x06002656 RID: 9814
		void unload();

		// Token: 0x06002657 RID: 9815
		void receiveEventPoke(int data);

		// Token: 0x06002658 RID: 9816
		string minigameId();

		// Token: 0x06002659 RID: 9817
		bool forceQuit();
	}
}
