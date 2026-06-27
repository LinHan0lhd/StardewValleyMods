using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames
{
	// Token: 0x02000236 RID: 566
	public class FantasyBoardGame : IMinigame
	{
		// Token: 0x06002583 RID: 9603 RVA: 0x001A3498 File Offset: 0x001A1698
		public FantasyBoardGame()
		{
			this.content = Game1.content.CreateTemporary();
			this.slides = this.content.Load<Texture2D>("LooseSprites\\boardGame");
			this.border = this.content.Load<Texture2D>("LooseSprites\\boardGameBorder");
			Game1.globalFadeToClear(null, 0.02f);
		}

		// Token: 0x06002584 RID: 9604 RVA: 0x001A3523 File Offset: 0x001A1723
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x06002585 RID: 9605 RVA: 0x001A3530 File Offset: 0x001A1730
		public bool tick(GameTime time)
		{
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			Game1.currentLocation.currentEvent.Update(Game1.currentLocation, time);
			if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				Game1.activeClickableMenu.update(time);
				Game1.PopUIMode();
			}
			if (this.endTimer > 0)
			{
				this.endTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.endTimer <= 0 && this.whichSlide == -1)
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.end), 0.02f);
				}
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				Game1.activeClickableMenu.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
				Game1.PopUIMode();
			}
			return false;
		}

		// Token: 0x06002586 RID: 9606 RVA: 0x001A3604 File Offset: 0x001A1804
		public void end()
		{
			this.unload();
			Event currentEvent = Game1.currentLocation.currentEvent;
			int currentCommand = currentEvent.CurrentCommand;
			currentEvent.CurrentCommand = currentCommand + 1;
			Game1.currentMinigame = null;
		}

		// Token: 0x06002587 RID: 9607 RVA: 0x001A3636 File Offset: 0x001A1836
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				Game1.activeClickableMenu.receiveLeftClick(x, y, true);
				Game1.PopUIMode();
			}
		}

		// Token: 0x06002588 RID: 9608 RVA: 0x001A3656 File Offset: 0x001A1856
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x06002589 RID: 9609 RVA: 0x001A3658 File Offset: 0x001A1858
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.pressActionButton(Game1.GetKeyboardState(), Game1.input.GetMouseState(), Game1.input.GetGamePadState());
			if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				Game1.activeClickableMenu.receiveRightClick(x, y, true);
				Game1.PopUIMode();
			}
		}

		// Token: 0x0600258A RID: 9610 RVA: 0x001A3697 File Offset: 0x001A1897
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x0600258B RID: 9611 RVA: 0x001A3699 File Offset: 0x001A1899
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x0600258C RID: 9612 RVA: 0x001A369C File Offset: 0x001A189C
		public void receiveKeyPress(Keys k)
		{
			if (Game1.isQuestion)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
					Game1.playSound("toolSwap", null);
					return;
				}
				if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
					Game1.playSound("toolSwap", null);
					return;
				}
			}
			else if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				Game1.activeClickableMenu.receiveKeyPress(k);
				Game1.PopUIMode();
			}
		}

		// Token: 0x0600258D RID: 9613 RVA: 0x001A3756 File Offset: 0x001A1956
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x0600258E RID: 9614 RVA: 0x001A3758 File Offset: 0x001A1958
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (this.whichSlide >= 0)
			{
				Vector2 offset = default(Vector2);
				if (this.shakeTimer > 0)
				{
					offset = new Vector2((float)Game1.random.Next(-2, 2), (float)Game1.random.Next(-2, 2));
				}
				b.Draw(this.border, offset + new Vector2((float)(Game1.viewport.Width / 2 - this.borderSourceWidth * 4 / 2), (float)(Game1.viewport.Height / 2 - this.borderSourceHeight * 4 / 2 - 128)), new Rectangle?(new Rectangle(0, 0, this.borderSourceWidth, this.borderSourceHeight)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				b.Draw(this.slides, offset + new Vector2((float)(Game1.viewport.Width / 2 - this.slideSourceWidth * 4 / 2), (float)(Game1.viewport.Height / 2 - this.slideSourceHeight * 4 / 2 - 128)), new Rectangle?(new Rectangle(this.whichSlide % 2 * this.slideSourceWidth, this.whichSlide / 2 * this.slideSourceHeight, this.slideSourceWidth, this.slideSourceHeight)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			}
			else
			{
				string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:FantasyBoardGame.cs.11980", this.grade);
				float yOffset = (float)Math.Sin((double)(this.endTimer / 1000)) * 8f;
				Game1.drawWithBorder(s, Game1.textColor, Color.Purple, new Vector2((float)(Game1.viewport.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, yOffset + (float)(Game1.viewport.Height / 2)));
			}
			b.End();
			if (Game1.activeClickableMenu != null)
			{
				Game1.PushUIMode();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				Game1.activeClickableMenu.draw(b);
				b.End();
				Game1.PopUIMode();
			}
		}

		// Token: 0x0600258F RID: 9615 RVA: 0x001A399B File Offset: 0x001A1B9B
		public void changeScreenSize()
		{
		}

		// Token: 0x06002590 RID: 9616 RVA: 0x001A399D File Offset: 0x001A1B9D
		public void unload()
		{
			this.content.Unload();
		}

		// Token: 0x06002591 RID: 9617 RVA: 0x001A39AC File Offset: 0x001A1BAC
		public void afterFade()
		{
			this.whichSlide = -1;
			int score = 0;
			if (Game1.player.mailReceived.Contains("savedFriends"))
			{
				score++;
			}
			if (Game1.player.mailReceived.Contains("destroyedPods"))
			{
				score++;
			}
			if (Game1.player.mailReceived.Contains("killedSkeleton"))
			{
				score++;
			}
			switch (score)
			{
			case 0:
				this.grade = "D";
				break;
			case 1:
				this.grade = "C";
				break;
			case 2:
				this.grade = "B";
				break;
			case 3:
				this.grade = "A";
				break;
			}
			Game1.playSound("newArtifact", null);
			this.endTimer = 5500;
		}

		// Token: 0x06002592 RID: 9618 RVA: 0x001A3A79 File Offset: 0x001A1C79
		public void receiveEventPoke(int data)
		{
			if (data == -2)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.afterFade), 0.02f);
				return;
			}
			if (data == -1)
			{
				this.shakeTimer = 1000;
				return;
			}
			this.whichSlide = data;
		}

		// Token: 0x06002593 RID: 9619 RVA: 0x001A3AAE File Offset: 0x001A1CAE
		public string minigameId()
		{
			return "FantasyBoardGame";
		}

		// Token: 0x06002594 RID: 9620 RVA: 0x001A3AB5 File Offset: 0x001A1CB5
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x06002595 RID: 9621 RVA: 0x001A3AB8 File Offset: 0x001A1CB8
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x040016D9 RID: 5849
		public int borderSourceWidth = 138;

		// Token: 0x040016DA RID: 5850
		public int borderSourceHeight = 74;

		// Token: 0x040016DB RID: 5851
		public int slideSourceWidth = 128;

		// Token: 0x040016DC RID: 5852
		public int slideSourceHeight = 64;

		// Token: 0x040016DD RID: 5853
		private LocalizedContentManager content;

		// Token: 0x040016DE RID: 5854
		private Texture2D slides;

		// Token: 0x040016DF RID: 5855
		private Texture2D border;

		// Token: 0x040016E0 RID: 5856
		public int whichSlide;

		// Token: 0x040016E1 RID: 5857
		public int shakeTimer;

		// Token: 0x040016E2 RID: 5858
		public int endTimer;

		// Token: 0x040016E3 RID: 5859
		private string grade = "";
	}
}
