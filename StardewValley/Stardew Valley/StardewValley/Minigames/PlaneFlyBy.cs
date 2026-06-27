using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Minigames
{
	// Token: 0x02000240 RID: 576
	public class PlaneFlyBy : IMinigame
	{
		// Token: 0x06002664 RID: 9828 RVA: 0x001B38B9 File Offset: 0x001B1AB9
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002665 RID: 9829 RVA: 0x001B38C8 File Offset: 0x001B1AC8
		public bool tick(GameTime time)
		{
			this.millisecondsSinceStart += time.ElapsedGameTime.Milliseconds;
			this.robotPosition.X = this.robotPosition.X - 1f * (float)time.ElapsedGameTime.Milliseconds / 4f;
			this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0)
			{
				this.smokeTimer = 100;
				this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(173, 1828, 15, 20), 1500f, 2, 0, this.robotPosition + new Vector2(68f, -24f), false, false)
				{
					motion = new Vector2(0f, 0.1f),
					scale = 4f,
					scaleChange = 0.002f,
					alphaFade = 0.0025f,
					rotation = -1.5707964f
				});
			}
			this.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.robotPosition.X < -128f && !Game1.globalFade)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterFade), 0.006f);
			}
			return false;
		}

		// Token: 0x06002666 RID: 9830 RVA: 0x001B3A3C File Offset: 0x001B1C3C
		public void afterFade()
		{
			Game1.currentMinigame = null;
			Game1.globalFadeToClear(null, 0.02f);
			if (Game1.currentLocation.currentEvent != null)
			{
				Event currentEvent = Game1.currentLocation.currentEvent;
				int currentCommand = currentEvent.CurrentCommand;
				currentEvent.CurrentCommand = currentCommand + 1;
				Game1.currentLocation.temporarySprites.Clear();
			}
		}

		// Token: 0x06002667 RID: 9831 RVA: 0x001B3A8E File Offset: 0x001B1C8E
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002668 RID: 9832 RVA: 0x001B3A90 File Offset: 0x001B1C90
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x001B3A92 File Offset: 0x001B1C92
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x001B3A94 File Offset: 0x001B1C94
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600266B RID: 9835 RVA: 0x001B3A96 File Offset: 0x001B1C96
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600266C RID: 9836 RVA: 0x001B3A98 File Offset: 0x001B1C98
		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape)
			{
				this.robotPosition.X = -1000f;
				this.tempSprites.Clear();
			}
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x001B3ABA File Offset: 0x001B1CBA
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x001B3ABC File Offset: 0x001B1CBC
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.mouseCursors, new Rectangle(0, this.backgroundPosition, Game1.graphics.GraphicsDevice.Viewport.Width, 2560), new Rectangle?(new Rectangle(264, 1858, 1, 84)), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)this.backgroundPosition), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 752)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 1504)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 2256)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, this.robotPosition, new Rectangle?(new Rectangle(222 + this.millisecondsSinceStart / 50 % 2 * 20, 1890, 20, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			b.End();
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x001B3D3C File Offset: 0x001B1F3C
		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			this.backgroundPosition = 2560 - (int)((float)Game1.game1.localMultiplayerWindow.Height * pixel_zoom_adjustment);
			this.robotPosition = new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2), (float)Game1.game1.localMultiplayerWindow.Height) * pixel_zoom_adjustment;
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x001B3DAC File Offset: 0x001B1FAC
		public void unload()
		{
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x001B3DAE File Offset: 0x001B1FAE
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x001B3DB5 File Offset: 0x001B1FB5
		public string minigameId()
		{
			return null;
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x001B3DB8 File Offset: 0x001B1FB8
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x001B3DBB File Offset: 0x001B1FBB
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x040017DB RID: 6107
		public const float robotSpeed = 1f;

		// Token: 0x040017DC RID: 6108
		public const int skyLength = 2560;

		// Token: 0x040017DD RID: 6109
		public int millisecondsSinceStart;

		// Token: 0x040017DE RID: 6110
		public int backgroundPosition = -2560 + (int)((float)Game1.game1.localMultiplayerWindow.Height / Game1.options.zoomLevel);

		// Token: 0x040017DF RID: 6111
		public int smokeTimer = 500;

		// Token: 0x040017E0 RID: 6112
		public Vector2 robotPosition = new Vector2((float)Game1.game1.localMultiplayerWindow.Width, (float)(Game1.game1.localMultiplayerWindow.Height / 2)) * 1f / Game1.options.zoomLevel;

		// Token: 0x040017E1 RID: 6113
		public TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();
	}
}
