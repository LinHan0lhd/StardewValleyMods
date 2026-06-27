using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Minigames
{
	// Token: 0x02000245 RID: 581
	public class Test : IMinigame
	{
		// Token: 0x060026C6 RID: 9926 RVA: 0x001B760C File Offset: 0x001B580C
		public Test()
		{
			for (int i = 0; i < 40; i++)
			{
				this.wallpaper.Add(new Wallpaper(i, true));
			}
		}

		// Token: 0x060026C7 RID: 9927 RVA: 0x001B7649 File Offset: 0x001B5849
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060026C8 RID: 9928 RVA: 0x001B7655 File Offset: 0x001B5855
		public bool tick(GameTime time)
		{
			return false;
		}

		// Token: 0x060026C9 RID: 9929 RVA: 0x001B7658 File Offset: 0x001B5858
		public void afterFade()
		{
		}

		// Token: 0x060026CA RID: 9930 RVA: 0x001B765A File Offset: 0x001B585A
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Game1.currentMinigame = null;
		}

		// Token: 0x060026CB RID: 9931 RVA: 0x001B7662 File Offset: 0x001B5862
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x001B7664 File Offset: 0x001B5864
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x001B7666 File Offset: 0x001B5866
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x001B7668 File Offset: 0x001B5868
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x001B766A File Offset: 0x001B586A
		public void receiveKeyPress(Keys k)
		{
		}

		// Token: 0x060026D0 RID: 9936 RVA: 0x001B766C File Offset: 0x001B586C
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x001B7670 File Offset: 0x001B5870
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, 2000, 2000), Color.White);
			Vector2 itemData = new Vector2(16f, 16f);
			for (int i = 0; i < this.wallpaper.Count; i++)
			{
				this.wallpaper[i].drawInMenu(b, itemData, 1f);
				itemData.X += 128f;
				if (itemData.X >= (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 128))
				{
					itemData.X = 16f;
					itemData.Y += 128f;
				}
			}
			b.End();
		}

		// Token: 0x060026D2 RID: 9938 RVA: 0x001B7752 File Offset: 0x001B5952
		public void changeScreenSize()
		{
		}

		// Token: 0x060026D3 RID: 9939 RVA: 0x001B7754 File Offset: 0x001B5954
		public void unload()
		{
		}

		// Token: 0x060026D4 RID: 9940 RVA: 0x001B7756 File Offset: 0x001B5956
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x060026D5 RID: 9941 RVA: 0x001B7758 File Offset: 0x001B5958
		public string minigameId()
		{
			return null;
		}

		// Token: 0x060026D6 RID: 9942 RVA: 0x001B775B File Offset: 0x001B595B
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x060026D7 RID: 9943 RVA: 0x001B775E File Offset: 0x001B595E
		public bool forceQuit()
		{
			return true;
		}

		// Token: 0x0400180D RID: 6157
		public List<Wallpaper> wallpaper = new List<Wallpaper>();
	}
}
