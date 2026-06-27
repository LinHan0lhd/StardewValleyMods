using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x0200009F RID: 159
	public interface ICustomEventScript
	{
		// Token: 0x06000789 RID: 1929
		bool update(GameTime time, Event e);

		// Token: 0x0600078A RID: 1930
		void draw(SpriteBatch b);

		// Token: 0x0600078B RID: 1931
		void drawAboveAlwaysFront(SpriteBatch b);
	}
}
