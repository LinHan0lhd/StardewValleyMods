using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Locations
{
	// Token: 0x020002C8 RID: 712
	public class Club : GameLocation
	{
		// Token: 0x06002E27 RID: 11815 RVA: 0x00241053 File Offset: 0x0023F253
		public Club()
		{
		}

		// Token: 0x06002E28 RID: 11816 RVA: 0x0024105B File Offset: 0x0023F25B
		public Club(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002E29 RID: 11817 RVA: 0x00241065 File Offset: 0x0023F265
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.lightGlows.Clear();
			this.coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  ");
		}

		// Token: 0x06002E2A RID: 11818 RVA: 0x00241094 File Offset: 0x0023F294
		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.002)
			{
				base.localSound("boop", null, null, SoundContext.Default);
			}
		}

		// Token: 0x06002E2B RID: 11819 RVA: 0x002410D4 File Offset: 0x0023F2D4
		public override void drawOverlays(SpriteBatch b)
		{
			if (Game1.currentMinigame == null)
			{
				SpriteText.drawStringWithScrollBackground(b, this.coinBuffer + Game1.player.clubCoins.ToString(), 64, 16, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(68f, 20f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
			}
			base.drawOverlays(b);
		}

		// Token: 0x04001F8A RID: 8074
		public static int timesPlayedCalicoJack;

		// Token: 0x04001F8B RID: 8075
		public static int timesPlayedSlots;

		// Token: 0x04001F8C RID: 8076
		private string coinBuffer;
	}
}
