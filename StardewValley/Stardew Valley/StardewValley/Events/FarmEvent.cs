using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events
{
	// Token: 0x02000326 RID: 806
	public interface FarmEvent : INetObject<NetFields>
	{
		// Token: 0x06003495 RID: 13461
		bool setUp();

		// Token: 0x06003496 RID: 13462
		bool tickUpdate(GameTime time);

		// Token: 0x06003497 RID: 13463
		void draw(SpriteBatch b);

		// Token: 0x06003498 RID: 13464
		void drawAboveEverything(SpriteBatch b);

		// Token: 0x06003499 RID: 13465
		void makeChangesToLocation();
	}
}
