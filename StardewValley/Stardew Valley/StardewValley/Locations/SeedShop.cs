using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	// Token: 0x020002ED RID: 749
	public class SeedShop : ShopLocation
	{
		// Token: 0x0600322B RID: 12843 RVA: 0x00280C28 File Offset: 0x0027EE28
		public SeedShop()
		{
		}

		// Token: 0x0600322C RID: 12844 RVA: 0x00280C30 File Offset: 0x0027EE30
		public SeedShop(string map, string name) : base(map, name)
		{
		}

		// Token: 0x0600322D RID: 12845 RVA: 0x00280C3C File Offset: 0x0027EE3C
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.player.maxItems.Value == 12)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle?(new Rectangle(255, 1436, 12, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
				return;
			}
			if (Game1.player.maxItems.Value < 36)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle?(new Rectangle(267, 1436, 12, 14)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
				return;
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(452, 1184, 112, 20)), new Rectangle?(new Rectangle(258, 1449, 1, 1)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1232f);
		}
	}
}
