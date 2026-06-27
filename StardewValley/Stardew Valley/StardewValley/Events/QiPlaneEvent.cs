using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;

namespace StardewValley.Events
{
	// Token: 0x02000329 RID: 809
	public class QiPlaneEvent : BaseFarmEvent
	{
		// Token: 0x060034AC RID: 13484 RVA: 0x002A0AE8 File Offset: 0x0029ECE8
		public QiPlaneEvent()
		{
			this.qiPlanePos = new Vector2(-400f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 4));
			this.boxDropTimer = 2000f;
			this.str = Game1.content.LoadString("Strings\\1_6_Strings:MysteryBoxAnnounce");
			Game1.changeMusicTrack("nightTime", false, MusicContext.Default);
			DelayedAction.playSoundAfterDelay("planeflyby", 1000, null, null, -1, false);
			Game1.player.mailReceived.Add("sawQiPlane");
		}

		// Token: 0x060034AD RID: 13485 RVA: 0x002A0B8C File Offset: 0x0029ED8C
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), new Color(24, 34, 84));
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, (int)((float)Game1.graphics.GraphicsDevice.Viewport.Height * 0.7f)), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.LightBlue);
			b.Draw(Game1.mouseCursors, new Vector2(1f, 1f), new Rectangle?(new Rectangle(0, 1453, 639, 191)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			b.Draw(Game1.mouseCursors, new Vector2(2564f, 1f), new Rectangle?(new Rectangle(0, 1453, 639, 191)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			b.Draw(Game1.mouseCursors, new Vector2(-50f, -10f) * 4f + new Vector2(0f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 596)), new Rectangle?(new Rectangle(0, 885, 639, 149)), Color.DarkCyan, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			b.Draw(Game1.mouseCursors, new Vector2(-50f, -10f) * 4f + new Vector2(2556f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 596)), new Rectangle?(new Rectangle(0, 885, 639, 149)), Color.DarkCyan, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 596)), new Rectangle?(new Rectangle(0, 885, 639, 149)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.graphics.GraphicsDevice.Viewport.Height - 596)), new Rectangle?(new Rectangle(0, 885, 639, 149)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.tempSprites)
			{
				temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
			}
			b.Draw(Game1.mouseCursors_1_6, this.qiPlanePos, new Rectangle?(new Rectangle(113, 204, 79, 43)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
			b.Draw(Game1.mouseCursors_1_6, this.qiPlanePos + new Vector2(79f, 0f) * 4f, new Rectangle?(new Rectangle(192 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 90.0 / 30.0) * 4, 204, 4, 44)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
			if (this.qiPlanePos.X > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 480))
			{
				float oldTime = this.textTimer;
				this.textTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.textTimer % 100f < oldTime % 100f && (int)(this.textTimer / 100f) < this.str.Length)
				{
					Game1.playSound("dialogueCharacter", null);
				}
				if ((int)(this.textTimer / 100f) > this.str.Length + 27)
				{
					this.finalFadeTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
				b.Draw(Game1.staminaRect, new Rectangle(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - SpriteText.getWidthOfString(this.str, 999999) / 2 - 18, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 56, SpriteText.getWidthOfString(this.str, 999999) + 20, 60), Color.Black * 0.4f);
				SpriteText.drawStringHorizontallyCenteredAt(b, this.str, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 50, (int)(this.textTimer / 100f), -1, 999999, 1f, 0.9f, false, new Color?(Color.White), 99999);
			}
			b.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * (this.finalFadeTimer / 3000f));
			base.draw(b);
		}

		// Token: 0x060034AE RID: 13486 RVA: 0x002A11E8 File Offset: 0x0029F3E8
		public override void drawAboveEverything(SpriteBatch b)
		{
			base.drawAboveEverything(b);
		}

		// Token: 0x060034AF RID: 13487 RVA: 0x002A11F1 File Offset: 0x0029F3F1
		public override bool setUp()
		{
			return base.setUp();
		}

		// Token: 0x060034B0 RID: 13488 RVA: 0x002A11FC File Offset: 0x0029F3FC
		public override bool tickUpdate(GameTime time)
		{
			if (Game1.GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				this.qiPlanePos.X = (float)(Game1.graphics.GraphicsDevice.Viewport.Width + 1000);
				this.textTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds * 2f;
				if ((int)(this.textTimer / 100f) > this.str.Length + 27)
				{
					this.finalFadeTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds * 2f;
				}
			}
			this.boxDropTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.boxDropTimer <= 0f && this.qiPlanePos.X < (float)Game1.graphics.GraphicsDevice.Viewport.Width)
			{
				this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(112, 166, 14, 35), 50f, 10, 1, this.qiPlanePos + new Vector2(52f, -4f) * 4f, false, false)
				{
					holdLastFrame = true,
					motion = new Vector2(-1f, (float)Game1.random.Next(3, 5)),
					accelerationChange = new Vector2(0f, -0.001f + (float)(Game1.random.NextDouble() - 0.5) / 1000f),
					acceleration = new Vector2(0f, 0.05f),
					scale = 4f
				});
				this.boxDropTimer = (float)Game1.random.Next(150, 500);
				DelayedAction.playSoundAfterDelay("parachute", 300, null, null, -1, false);
			}
			for (int i = this.tempSprites.Count - 1; i >= 0; i--)
			{
				this.tempSprites[i].update(time);
				if (this.tempSprites[i].motion.Y < 1f)
				{
					this.tempSprites[i].motion.Y = 1f;
				}
				if (this.tempSprites[i].position.Y > (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 500))
				{
					this.tempSprites[i].alphaFade = 0.01f;
				}
				if (this.tempSprites[i].alpha <= 0f)
				{
					this.tempSprites.RemoveAt(i);
				}
			}
			this.qiPlanePos.X = this.qiPlanePos.X + (float)(time.ElapsedGameTime.TotalMilliseconds * 0.25);
			return this.finalFadeTimer > 4000f;
		}

		// Token: 0x04002265 RID: 8805
		private Vector2 qiPlanePos;

		// Token: 0x04002266 RID: 8806
		private List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		// Token: 0x04002267 RID: 8807
		private float boxDropTimer;

		// Token: 0x04002268 RID: 8808
		private float textTimer;

		// Token: 0x04002269 RID: 8809
		private float finalFadeTimer;

		// Token: 0x0400226A RID: 8810
		private string str;
	}
}
