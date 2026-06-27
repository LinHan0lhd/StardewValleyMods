using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames
{
	// Token: 0x02000239 RID: 569
	public class HaleyCowPictures : IMinigame
	{
		// Token: 0x060025C0 RID: 9664 RVA: 0x001A6DFC File Offset: 0x001A4FFC
		public HaleyCowPictures()
		{
			this.content = Game1.content.CreateTemporary();
			this.pictures = ((Game1.season == Season.Winter) ? this.content.Load<Texture2D>("LooseSprites\\cowPhotosWinter") : this.content.Load<Texture2D>("LooseSprites\\cowPhotos"));
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			this.centerOfScreen = new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2), (float)(Game1.game1.localMultiplayerWindow.Height / 2)) * pixel_zoom_adjustment;
		}

		// Token: 0x060025C1 RID: 9665 RVA: 0x001A6EA0 File Offset: 0x001A50A0
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060025C2 RID: 9666 RVA: 0x001A6EAC File Offset: 0x001A50AC
		public bool tick(GameTime time)
		{
			this.betweenPhotoTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.betweenPhotoTimer <= 0)
			{
				this.betweenPhotoTimer = 5000;
				this.numberOfPhotosSoFar++;
				if (this.numberOfPhotosSoFar < 5)
				{
					Game1.playSound("cameraNoise", null);
				}
				if (this.numberOfPhotosSoFar >= 6)
				{
					Event currentEvent = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent.CurrentCommand;
					currentEvent.CurrentCommand = currentCommand + 1;
					return true;
				}
			}
			if (this.numberOfPhotosSoFar >= 5)
			{
				this.fadeAlpha = Math.Min(1f, this.fadeAlpha += 0.007f);
			}
			if (this.numberOfPhotosSoFar > 0)
			{
				Game1.player.blinkTimer = 0;
				Game1.player.currentEyes = 0;
			}
			return false;
		}

		// Token: 0x060025C3 RID: 9667 RVA: 0x001A6F85 File Offset: 0x001A5185
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060025C4 RID: 9668 RVA: 0x001A6F87 File Offset: 0x001A5187
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x060025C5 RID: 9669 RVA: 0x001A6F89 File Offset: 0x001A5189
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060025C6 RID: 9670 RVA: 0x001A6F8B File Offset: 0x001A518B
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x060025C7 RID: 9671 RVA: 0x001A6F8D File Offset: 0x001A518D
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x060025C8 RID: 9672 RVA: 0x001A6F8F File Offset: 0x001A518F
		public void receiveKeyPress(Keys k)
		{
		}

		// Token: 0x060025C9 RID: 9673 RVA: 0x001A6F91 File Offset: 0x001A5191
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x060025CA RID: 9674 RVA: 0x001A6F94 File Offset: 0x001A5194
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, null);
			if (this.numberOfPhotosSoFar > 0)
			{
				b.Draw(this.pictures, this.centerOfScreen + new Vector2(-208f, -248f), new Rectangle?(new Rectangle(0, 0, 104, 124)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				Game1.player.faceDirection(2);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 0, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f, 0.01f, false);
				b.Draw(Game1.shadowTexture, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f + new Vector2(32f, 120f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.005f);
			}
			if (this.numberOfPhotosSoFar > 1)
			{
				Game1.player.faceDirection(3);
				b.Draw(this.pictures, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f), new Rectangle?(new Rectangle(104, 0, 104, 124)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 6, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f) + new Vector2(64f, 66f) * 4f, 0.11f, true);
				b.Draw(Game1.shadowTexture, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f) + new Vector2(64f, 66f) * 4f + new Vector2(32f, 120f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.105f);
			}
			if (this.numberOfPhotosSoFar > 2)
			{
				Game1.player.faceDirection(3);
				b.Draw(this.pictures, this.centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f), new Rectangle?(new Rectangle(0, 124, 104, 124)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 89, this.centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f) + new Vector2(55f, 66f) * 4f, 0.21f, true);
				b.Draw(Game1.shadowTexture, this.centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f) + new Vector2(55f, 66f) * 4f + new Vector2(32f, 120f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.205f);
			}
			if (this.numberOfPhotosSoFar > 3)
			{
				Game1.player.faceDirection(2);
				b.Draw(this.pictures, this.centerOfScreen + new Vector2(-208f, -248f), new Rectangle?(new Rectangle(104, 124, 104, 124)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.3f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 94, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f, 0.31f, false);
				b.Draw(Game1.shadowTexture, this.centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f + new Vector2(32f, 120f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.305f);
			}
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Rectangle?(Game1.staminaRect.Bounds), Color.Black * this.fadeAlpha, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			b.End();
		}

		// Token: 0x060025CB RID: 9675 RVA: 0x001A7660 File Offset: 0x001A5860
		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			this.centerOfScreen = new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2), (float)(Game1.game1.localMultiplayerWindow.Height / 2)) * pixel_zoom_adjustment;
		}

		// Token: 0x060025CC RID: 9676 RVA: 0x001A76B3 File Offset: 0x001A58B3
		public void unload()
		{
			this.content.Unload();
		}

		// Token: 0x060025CD RID: 9677 RVA: 0x001A76C0 File Offset: 0x001A58C0
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060025CE RID: 9678 RVA: 0x001A76C7 File Offset: 0x001A58C7
		public string minigameId()
		{
			return null;
		}

		// Token: 0x060025CF RID: 9679 RVA: 0x001A76CA File Offset: 0x001A58CA
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x060025D0 RID: 9680 RVA: 0x001A76CD File Offset: 0x001A58CD
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x04001711 RID: 5905
		private const int pictureWidth = 416;

		// Token: 0x04001712 RID: 5906
		private const int pictureHeight = 496;

		// Token: 0x04001713 RID: 5907
		private const int sourceWidth = 104;

		// Token: 0x04001714 RID: 5908
		private const int sourceHeight = 124;

		// Token: 0x04001715 RID: 5909
		private int numberOfPhotosSoFar;

		// Token: 0x04001716 RID: 5910
		private int betweenPhotoTimer = 1000;

		// Token: 0x04001717 RID: 5911
		private LocalizedContentManager content;

		// Token: 0x04001718 RID: 5912
		private Vector2 centerOfScreen;

		// Token: 0x04001719 RID: 5913
		private Texture2D pictures;

		// Token: 0x0400171A RID: 5914
		private float fadeAlpha;
	}
}
