using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Minigames
{
	// Token: 0x02000241 RID: 577
	public class RobotBlastoff : IMinigame
	{
		// Token: 0x06002676 RID: 9846 RVA: 0x001B3E57 File Offset: 0x001B2057
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x001B3E64 File Offset: 0x001B2064
		public bool tick(GameTime time)
		{
			this.millisecondsSinceStart += time.ElapsedGameTime.Milliseconds;
			float f = 1.35f - 0.85f * (5f / Math.Max(5f, this.robotPosition.Y / 20f));
			this.backgroundPosition += (int)(0.25f * (float)time.ElapsedGameTime.Milliseconds * f) / 2;
			this.robotPosition.Y = this.robotPosition.Y - 0.3f * (float)time.ElapsedGameTime.Milliseconds / 4f;
			this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0)
			{
				this.smokeTimer = 350;
				this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(143, 1828, 15, 20), 1500f, 4, 0, this.robotPosition + new Vector2(0f, 72f), false, false)
				{
					motion = new Vector2(0f, -0.9f),
					acceleration = new Vector2(-0.001f, 0.006f),
					scale = 4f,
					scaleChange = 0.002f,
					alphaFade = 0.0025f
				});
			}
			this.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.robotPosition.Y < 0f && Game1.random.NextDouble() < 0.005)
			{
				this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(256, 1680, 16, 16), 80f, 5, 0, new Vector2((float)Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height / 2)), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(4f, 4f)
				});
			}
			if (this.robotPosition.Y < -512f && !Game1.globalFade)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterFade), 0.006f);
			}
			return false;
		}

		// Token: 0x06002678 RID: 9848 RVA: 0x001B4120 File Offset: 0x001B2320
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

		// Token: 0x06002679 RID: 9849 RVA: 0x001B4172 File Offset: 0x001B2372
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x0600267A RID: 9850 RVA: 0x001B4175 File Offset: 0x001B2375
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600267B RID: 9851 RVA: 0x001B4177 File Offset: 0x001B2377
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x0600267C RID: 9852 RVA: 0x001B4179 File Offset: 0x001B2379
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x001B417B File Offset: 0x001B237B
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600267E RID: 9854 RVA: 0x001B417D File Offset: 0x001B237D
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600267F RID: 9855 RVA: 0x001B417F File Offset: 0x001B237F
		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape)
			{
				this.robotPosition.Y = -1000f;
				this.tempSprites.Clear();
			}
		}

		// Token: 0x06002680 RID: 9856 RVA: 0x001B41A1 File Offset: 0x001B23A1
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x06002681 RID: 9857 RVA: 0x001B41A4 File Offset: 0x001B23A4
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(Game1.mouseCursors, new Rectangle(0, this.backgroundPosition, Game1.graphics.GraphicsDevice.Viewport.Width, 2560), new Rectangle?(new Rectangle(264, 1858, 1, 84)), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)this.backgroundPosition), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 752)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 1504)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(this.backgroundPosition - 2256)), new Rectangle?(new Rectangle(0, 1454, 639, 188)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, this.robotPosition + new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)), new Rectangle?(new Rectangle(206 + this.millisecondsSinceStart / 50 % 4 * 15, 1827, 15, 27)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			b.End();
		}

		// Token: 0x06002682 RID: 9858 RVA: 0x001B4448 File Offset: 0x001B2648
		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			this.backgroundPosition = 2560 - (int)((float)Game1.game1.localMultiplayerWindow.Height * pixel_zoom_adjustment);
			this.robotPosition = new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2), (float)Game1.game1.localMultiplayerWindow.Height) * pixel_zoom_adjustment;
		}

		// Token: 0x06002683 RID: 9859 RVA: 0x001B44B8 File Offset: 0x001B26B8
		public void unload()
		{
		}

		// Token: 0x06002684 RID: 9860 RVA: 0x001B44BA File Offset: 0x001B26BA
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x001B44C1 File Offset: 0x001B26C1
		public string minigameId()
		{
			return null;
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x001B44C4 File Offset: 0x001B26C4
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x040017E2 RID: 6114
		public const float backGroundSpeed = 0.25f;

		// Token: 0x040017E3 RID: 6115
		public const float robotSpeed = 0.3f;

		// Token: 0x040017E4 RID: 6116
		public const int skyLength = 2560;

		// Token: 0x040017E5 RID: 6117
		public int millisecondsSinceStart;

		// Token: 0x040017E6 RID: 6118
		public int backgroundPosition = -2560 + (int)((float)Game1.game1.localMultiplayerWindow.Height / Game1.options.zoomLevel);

		// Token: 0x040017E7 RID: 6119
		public int smokeTimer = 500;

		// Token: 0x040017E8 RID: 6120
		public Vector2 robotPosition = new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel, (float)Game1.game1.localMultiplayerWindow.Height / Game1.options.zoomLevel);

		// Token: 0x040017E9 RID: 6121
		public TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();
	}
}
