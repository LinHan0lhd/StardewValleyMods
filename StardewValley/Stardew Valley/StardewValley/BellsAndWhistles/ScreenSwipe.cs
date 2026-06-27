using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A6 RID: 934
	public class ScreenSwipe
	{
		// Token: 0x060038DF RID: 14559 RVA: 0x002D08CC File Offset: 0x002CEACC
		public ScreenSwipe(int which, float swipeVelocity = -1f, int durationAfterSwipe = -1, int w = -1, int h = -1)
		{
			Game1.playSound("throw", null);
			if (swipeVelocity == -1f)
			{
				swipeVelocity = 5f;
			}
			if (durationAfterSwipe == -1)
			{
				durationAfterSwipe = 2700;
			}
			this.swipeVelocity = swipeVelocity;
			this.durationAfterSwipe = durationAfterSwipe;
			Vector2 screenCenter = new Vector2((float)(this.ViewportWidth / 2), (float)(this.ViewportHeight / 2));
			if (which == 0)
			{
				this.messageSource = new Rectangle(128, 1367, 150, 14);
			}
			if (which != 0)
			{
				if (which == 1)
				{
					this.texture = Game1.mouseCursors_1_6;
					this.bgSource = new Rectangle(0, 361, 1, 71);
					this.flairSource = new Rectangle(1, 361, 159, 71);
					this.movingFlairSource = new Rectangle(161, 412, 17, 16);
					this.originalBGSourceXLimit = this.bgSource.X + this.bgSource.Width;
					this.yPosition = (int)screenCenter.Y - this.bgSource.Height * 4 / 2;
					this.messagePosition = new Vector2(screenCenter.X - (float)(this.messageSource.Width * 4 / 2), screenCenter.Y - (float)(this.messageSource.Height * 4 / 2));
					this.flairPositions.Add(new Vector2(this.messagePosition.X - (float)(this.flairSource.Width * 4 / 2), (float)this.yPosition));
					this.movingFlairPosition = new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
					this.movingFlairMotion = new Vector2(0f, -0.5f);
				}
			}
			else
			{
				this.texture = Game1.mouseCursors;
				this.bgSource = new Rectangle(128, 1296, 1, 71);
				this.flairSource = new Rectangle(144, 1303, 144, 58);
				this.movingFlairSource = new Rectangle(643, 768, 8, 13);
				this.originalBGSourceXLimit = this.bgSource.X + this.bgSource.Width;
				this.yPosition = (int)screenCenter.Y - this.bgSource.Height * 4 / 2;
				this.messagePosition = new Vector2(screenCenter.X - (float)(this.messageSource.Width * 4 / 2), screenCenter.Y - (float)(this.messageSource.Height * 4 / 2));
				this.flairPositions.Add(new Vector2(this.messagePosition.X - (float)(this.flairSource.Width * 4) - 64f, (float)(this.yPosition + 28)));
				this.flairPositions.Add(new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 64f, (float)(this.yPosition + 28)));
				this.movingFlairPosition = new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
				this.movingFlairMotion = new Vector2(0f, -0.5f);
			}
			this.bgDest = new Rectangle(0, this.yPosition, this.bgSource.Width * 4, this.bgSource.Height * 4);
		}

		// Token: 0x060038E0 RID: 14560 RVA: 0x002D0C6C File Offset: 0x002CEE6C
		public bool update(GameTime time)
		{
			if (this.durationAfterSwipe > 0 && this.bgDest.Width <= this.ViewportWidth)
			{
				this.bgDest.Width = this.bgDest.Width + (int)((double)this.swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
				if (this.bgDest.Width > this.ViewportWidth)
				{
					Game1.playSound("newRecord", null);
				}
			}
			else if (this.durationAfterSwipe <= 0)
			{
				this.bgDest.X = this.bgDest.X + (int)((double)this.swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
				for (int i = 0; i < this.flairPositions.Count; i++)
				{
					if ((float)this.bgDest.X > this.flairPositions[i].X)
					{
						this.flairPositions[i] = new Vector2((float)this.bgDest.X, this.flairPositions[i].Y);
					}
				}
				if ((float)this.bgDest.X > this.messagePosition.X)
				{
					this.messagePosition = new Vector2((float)this.bgDest.X, this.messagePosition.Y);
				}
				if ((float)this.bgDest.X > this.movingFlairPosition.X)
				{
					this.movingFlairPosition = new Vector2((float)this.bgDest.X, this.movingFlairPosition.Y);
				}
			}
			if (this.bgDest.Width > this.ViewportWidth && this.durationAfterSwipe > 0)
			{
				if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
				{
					this.durationAfterSwipe = 0;
				}
				this.durationAfterSwipe -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this.durationAfterSwipe <= 0)
				{
					Game1.playSound("tinyWhip", null);
				}
			}
			this.movingFlairPosition += this.movingFlairMotion;
			return this.bgDest.X > this.ViewportWidth;
		}

		// Token: 0x060038E1 RID: 14561 RVA: 0x002D0E88 File Offset: 0x002CF088
		public Rectangle getAdjustedSourceRect(Rectangle sourceRect, float xStartPosition)
		{
			if (xStartPosition > (float)this.bgDest.Width || xStartPosition + (float)(sourceRect.Width * 4) < (float)this.bgDest.X)
			{
				return Rectangle.Empty;
			}
			Math.Min((float)(sourceRect.X + sourceRect.Width), Math.Max((float)sourceRect.X, (float)sourceRect.X + ((float)this.bgDest.Width - xStartPosition) / 4f));
			return new Rectangle(sourceRect.X, sourceRect.Y, (int)Math.Min((float)sourceRect.Width, ((float)this.bgDest.Width - xStartPosition) / 4f), sourceRect.Height);
		}

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x060038E2 RID: 14562 RVA: 0x002D0F37 File Offset: 0x002CF137
		private int ViewportWidth
		{
			get
			{
				return Game1.uiViewport.Width;
			}
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x060038E3 RID: 14563 RVA: 0x002D0F43 File Offset: 0x002CF143
		private int ViewportHeight
		{
			get
			{
				return Game1.uiViewport.Height;
			}
		}

		// Token: 0x060038E4 RID: 14564 RVA: 0x002D0F50 File Offset: 0x002CF150
		public void draw(SpriteBatch b)
		{
			b.Draw(this.texture, this.bgDest, new Rectangle?(this.bgSource), Color.White);
			foreach (Vector2 v in this.flairPositions)
			{
				Rectangle r = this.getAdjustedSourceRect(this.flairSource, v.X);
				int right = r.Right;
				int num = this.originalBGSourceXLimit;
				b.Draw(this.texture, v, new Rectangle?(r), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			b.Draw(this.texture, this.movingFlairPosition, new Rectangle?(this.getAdjustedSourceRect(this.movingFlairSource, this.movingFlairPosition.X)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(this.texture, this.messagePosition, new Rectangle?(this.getAdjustedSourceRect(this.messageSource, this.messagePosition.X)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}

		// Token: 0x0400255E RID: 9566
		public const int swipe_bundleComplete = 0;

		// Token: 0x0400255F RID: 9567
		public const int swipe_raccoon = 1;

		// Token: 0x04002560 RID: 9568
		public const int borderPixelWidth = 7;

		// Token: 0x04002561 RID: 9569
		private Rectangle bgSource;

		// Token: 0x04002562 RID: 9570
		private Rectangle flairSource;

		// Token: 0x04002563 RID: 9571
		private Rectangle messageSource;

		// Token: 0x04002564 RID: 9572
		private Rectangle movingFlairSource;

		// Token: 0x04002565 RID: 9573
		private Rectangle bgDest;

		// Token: 0x04002566 RID: 9574
		private int yPosition;

		// Token: 0x04002567 RID: 9575
		private int durationAfterSwipe;

		// Token: 0x04002568 RID: 9576
		private int originalBGSourceXLimit;

		// Token: 0x04002569 RID: 9577
		private List<Vector2> flairPositions = new List<Vector2>();

		// Token: 0x0400256A RID: 9578
		private Vector2 messagePosition;

		// Token: 0x0400256B RID: 9579
		private Vector2 movingFlairPosition;

		// Token: 0x0400256C RID: 9580
		private Vector2 movingFlairMotion;

		// Token: 0x0400256D RID: 9581
		private float swipeVelocity;

		// Token: 0x0400256E RID: 9582
		private Texture2D texture;

		// Token: 0x0400256F RID: 9583
		private int width;

		// Token: 0x04002570 RID: 9584
		private int height;
	}
}
