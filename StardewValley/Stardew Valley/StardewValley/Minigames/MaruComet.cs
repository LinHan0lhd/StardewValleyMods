using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Minigames
{
	// Token: 0x0200023B RID: 571
	public class MaruComet : IMinigame
	{
		// Token: 0x060025E8 RID: 9704 RVA: 0x001A9578 File Offset: 0x001A7778
		public MaruComet()
		{
			this.zoom = 4;
			this.content = Game1.content.CreateTemporary();
			this.cometTexture = this.content.Load<Texture2D>("Minigames\\MaruComet");
			this.changeScreenSize();
		}

		// Token: 0x060025E9 RID: 9705 RVA: 0x001A95F8 File Offset: 0x001A77F8
		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			this.centerOfScreen = pixel_zoom_adjustment * new Vector2((float)(Game1.game1.localMultiplayerWindow.Width / 2), (float)(Game1.game1.localMultiplayerWindow.Height / 2));
			this.centerOfScreen.X = (float)((int)this.centerOfScreen.X);
			this.centerOfScreen.Y = (float)((int)this.centerOfScreen.Y);
			this.cometColorOrigin = this.centerOfScreen + pixel_zoom_adjustment * new Vector2((float)(-71 * this.zoom), (float)(71 * this.zoom));
		}

		// Token: 0x060025EA RID: 9706 RVA: 0x001A96AB File Offset: 0x001A78AB
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x060025EB RID: 9707 RVA: 0x001A96B0 File Offset: 0x001A78B0
		public bool tick(GameTime time)
		{
			this.flybyTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.fade > 0f)
			{
				this.fade -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
			}
			if (this.flybyTimer <= 0)
			{
				this.flybyTimer = 200;
				bool bottom = Game1.random.NextBool();
				this.flybys.Add(new Vector2((float)(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom)), (float)(bottom ? (8 * this.zoom) : (-(float)Game1.random.Next(143 * this.zoom)))));
				this.flybysClose.Add(new Vector2((float)(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom)), (float)(bottom ? (8 * this.zoom) : (-(float)Game1.random.Next(143 * this.zoom)))));
				this.flybysFar.Add(new Vector2((float)(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom)), (float)(bottom ? (8 * this.zoom) : (-(float)Game1.random.Next(143 * this.zoom)))));
			}
			for (int i = this.flybys.Count - 1; i >= 0; i--)
			{
				this.flybys[i] = new Vector2(this.flybys[i].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds, this.flybys[i].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds);
				if (this.cometColorOrigin.Y + this.flybys[i].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
				{
					this.flybys.RemoveAt(i);
				}
			}
			for (int j = this.flybysClose.Count - 1; j >= 0; j--)
			{
				this.flybysClose[j] = new Vector2(this.flybysClose[j].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f, this.flybysClose[j].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f);
				if (this.cometColorOrigin.Y + this.flybysClose[j].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
				{
					this.flybysClose.RemoveAt(j);
				}
			}
			for (int k = this.flybysFar.Count - 1; k >= 0; k--)
			{
				this.flybysFar[k] = new Vector2(this.flybysFar[k].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f, this.flybysFar[k].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f);
				if (this.cometColorOrigin.Y + this.flybysFar[k].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
				{
					this.flybysFar.RemoveAt(k);
				}
			}
			this.totalTimer += time.ElapsedGameTime.Milliseconds;
			if (this.totalTimer >= 28000)
			{
				if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet5")))
				{
					this.currentStringCharacter = 0;
					this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet5");
				}
			}
			else if (this.totalTimer >= 25000)
			{
				if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet4")))
				{
					this.currentStringCharacter = 0;
					this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet4");
				}
			}
			else if (this.totalTimer >= 20000)
			{
				if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet3")))
				{
					this.currentStringCharacter = 0;
					this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet3");
				}
			}
			else if (this.totalTimer >= 16000)
			{
				if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet2")))
				{
					this.currentStringCharacter = 0;
					this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet2");
				}
			}
			else if (this.totalTimer >= 10000 && !this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet1")))
			{
				this.currentStringCharacter = 0;
				this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet1");
			}
			this.characterAdvanceTimer += time.ElapsedGameTime.Milliseconds;
			if (this.characterAdvanceTimer > 30)
			{
				this.currentStringCharacter++;
				this.characterAdvanceTimer = 0;
			}
			if (this.totalTimer >= 35000)
			{
				this.fade += (float)time.ElapsedGameTime.Milliseconds * 0.002f;
				if (this.fade >= 1f)
				{
					if (Game1.currentLocation.currentEvent != null)
					{
						Event currentEvent = Game1.currentLocation.currentEvent;
						int currentCommand = currentEvent.CurrentCommand;
						currentEvent.CurrentCommand = currentCommand + 1;
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x060025EC RID: 9708 RVA: 0x001A9CD0 File Offset: 0x001A7ED0
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, null);
			b.Draw(this.cometTexture, this.cometColorOrigin + new Vector2((float)((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0)), (float)(-(float)((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0)))), new Rectangle?(new Rectangle(247, 0, 265, 240)), Color.White, 0f, new Vector2(265f, 0f), (float)this.zoom, SpriteEffects.None, 0.1f);
			b.Draw(this.cometTexture, this.cometColorOrigin + new Vector2((float)((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0) + 808), (float)(-(float)((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0)) - 808)), new Rectangle?(new Rectangle(247, 0, 265, 240)), Color.White, 0f, new Vector2(265f, 0f), (float)this.zoom, SpriteEffects.None, 0.1f);
			b.Draw(this.cometTexture, this.centerOfScreen + new Vector2(-71f, -71f) * (float)this.zoom, new Rectangle?(new Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 300.0 / 100.0) * 143, 240, 143, 143)), Color.White, 0f, Vector2.Zero, (float)this.zoom, SpriteEffects.None, 0.2f);
			foreach (Vector2 v in this.flybys)
			{
				b.Draw(this.cometTexture, this.cometColorOrigin + v, new Rectangle?(new Rectangle(0, 0, 8, 8)), Color.White * 0.4f, 0f, Vector2.Zero, (float)this.zoom, SpriteEffects.None, 0.24f);
			}
			foreach (Vector2 v2 in this.flybysClose)
			{
				b.Draw(this.cometTexture, this.cometColorOrigin + v2, new Rectangle?(new Rectangle(0, 0, 8, 8)), Color.White * 0.4f, 0f, Vector2.Zero, (float)(this.zoom + 1), SpriteEffects.None, 0.24f);
			}
			foreach (Vector2 v3 in this.flybysFar)
			{
				b.Draw(this.cometTexture, this.cometColorOrigin + v3, new Rectangle?(new Rectangle(0, 0, 8, 8)), Color.White * 0.4f, 0f, Vector2.Zero, (float)(this.zoom - 1), SpriteEffects.None, 0.24f);
			}
			b.Draw(this.cometTexture, this.centerOfScreen + new Vector2(-71f, -71f) * (float)this.zoom, new Rectangle?(new Rectangle(0, 97, 143, 143)), Color.White, 0f, Vector2.Zero, (float)this.zoom, SpriteEffects.None, 0.3f);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, (int)this.centerOfScreen.X - 71 * this.zoom, Game1.graphics.GraphicsDevice.Viewport.Height), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, (int)this.centerOfScreen.Y - 71 * this.zoom), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle((int)this.centerOfScreen.X + 71 * this.zoom, 0, Game1.graphics.GraphicsDevice.Viewport.Width - ((int)this.centerOfScreen.X + 71 * this.zoom), Game1.graphics.GraphicsDevice.Viewport.Height), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle((int)this.centerOfScreen.X - 71 * this.zoom, (int)this.centerOfScreen.Y + 71 * this.zoom, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - ((int)this.centerOfScreen.Y + 71 * this.zoom)), new Rectangle?(Game1.staminaRect.Bounds), Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			float height = (float)SpriteText.getHeightOfString(this.currentString, Game1.game1.localMultiplayerWindow.Width);
			float text_draw_y = (float)((int)this.centerOfScreen.Y + 79 * this.zoom);
			if (text_draw_y + height > (float)Game1.viewport.Height)
			{
				text_draw_y += (float)Game1.viewport.Height - (text_draw_y + height);
			}
			SpriteText.drawStringHorizontallyCenteredAt(b, this.currentString, (int)this.centerOfScreen.X, (int)text_draw_y, this.currentStringCharacter, -1, 99999, 1f, 0.99f, false, new Color?(SpriteText.color_Purple), Game1.game1.localMultiplayerWindow.Width);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Rectangle?(Game1.staminaRect.Bounds), Color.Black * this.fade, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			b.End();
		}

		// Token: 0x060025ED RID: 9709 RVA: 0x001AA3FC File Offset: 0x001A85FC
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x060025EE RID: 9710 RVA: 0x001AA3FE File Offset: 0x001A85FE
		public string minigameId()
		{
			return null;
		}

		// Token: 0x060025EF RID: 9711 RVA: 0x001AA401 File Offset: 0x001A8601
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060025F0 RID: 9712 RVA: 0x001AA40D File Offset: 0x001A860D
		public void receiveEventPoke(int data)
		{
		}

		// Token: 0x060025F1 RID: 9713 RVA: 0x001AA40F File Offset: 0x001A860F
		public void receiveKeyPress(Keys k)
		{
		}

		// Token: 0x060025F2 RID: 9714 RVA: 0x001AA411 File Offset: 0x001A8611
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x060025F3 RID: 9715 RVA: 0x001AA413 File Offset: 0x001A8613
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060025F4 RID: 9716 RVA: 0x001AA415 File Offset: 0x001A8615
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060025F5 RID: 9717 RVA: 0x001AA417 File Offset: 0x001A8617
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x060025F6 RID: 9718 RVA: 0x001AA419 File Offset: 0x001A8619
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x060025F7 RID: 9719 RVA: 0x001AA41B File Offset: 0x001A861B
		public void unload()
		{
			this.content.Unload();
		}

		// Token: 0x060025F8 RID: 9720 RVA: 0x001AA428 File Offset: 0x001A8628
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x04001752 RID: 5970
		private const int telescopeCircleWidth = 143;

		// Token: 0x04001753 RID: 5971
		private const int flybyRepeater = 200;

		// Token: 0x04001754 RID: 5972
		private const float flybySpeed = 0.8f;

		// Token: 0x04001755 RID: 5973
		private LocalizedContentManager content;

		// Token: 0x04001756 RID: 5974
		private Vector2 centerOfScreen;

		// Token: 0x04001757 RID: 5975
		private Vector2 cometColorOrigin;

		// Token: 0x04001758 RID: 5976
		private Texture2D cometTexture;

		// Token: 0x04001759 RID: 5977
		private List<Vector2> flybys = new List<Vector2>();

		// Token: 0x0400175A RID: 5978
		private List<Vector2> flybysClose = new List<Vector2>();

		// Token: 0x0400175B RID: 5979
		private List<Vector2> flybysFar = new List<Vector2>();

		// Token: 0x0400175C RID: 5980
		private string currentString = "";

		// Token: 0x0400175D RID: 5981
		private int zoom;

		// Token: 0x0400175E RID: 5982
		private int flybyTimer;

		// Token: 0x0400175F RID: 5983
		private int totalTimer;

		// Token: 0x04001760 RID: 5984
		private int currentStringCharacter;

		// Token: 0x04001761 RID: 5985
		private int characterAdvanceTimer;

		// Token: 0x04001762 RID: 5986
		private float fade = 1f;
	}
}
