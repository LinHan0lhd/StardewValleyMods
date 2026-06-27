using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x020002B9 RID: 697
	public class WheelSpinGame : IClickableMenu
	{
		// Token: 0x06002D6F RID: 11631 RVA: 0x00238D28 File Offset: 0x00236F28
		public WheelSpinGame(int wager) : base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height / 2 - 224, 640, 448, false)
		{
			this.timerBeforeStart = 1000;
			this.arrowRotationVelocity = 0.19634954084936207;
			this.arrowRotationVelocity += (double)Game1.random.Next(0, 15) * 3.141592653589793 / 256.0;
			this.arrowRotationDeceleration = -0.0006283185307179586;
			if (Game1.random.NextBool())
			{
				this.arrowRotationVelocity += 0.04908738521234052;
			}
			this.wager = wager;
			Game1.player.Halt();
		}

		// Token: 0x06002D70 RID: 11632 RVA: 0x00238DF5 File Offset: 0x00236FF5
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002D71 RID: 11633 RVA: 0x00238DF8 File Offset: 0x00236FF8
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.timerBeforeStart <= 0)
			{
				double oldVelocity = this.arrowRotationVelocity;
				this.arrowRotationVelocity += this.arrowRotationDeceleration;
				if (this.arrowRotationVelocity <= 0.039269908169872414 && oldVelocity > 0.039269908169872414)
				{
					bool colorChoiceGreen = Game1.currentLocation.currentEvent.specialEventVariable2;
					if (this.arrowRotation > 1.5707963267948966 && this.arrowRotation <= 4.319689898685965 && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 15f))
					{
						if (colorChoiceGreen)
						{
							this.arrowRotationVelocity = 0.06544984694978735;
							Game1.playSound("dwop", null);
						}
					}
					else if ((this.arrowRotation + 3.141592653589793) % 6.283185307179586 <= 4.319689898685965 && !colorChoiceGreen && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 20f))
					{
						this.arrowRotationVelocity = 0.06544984694978735;
						Game1.playSound("dwop", null);
					}
				}
				if (this.arrowRotationVelocity <= 0.0 && !this.doneSpinning)
				{
					this.doneSpinning = true;
					this.arrowRotationDeceleration = 0.0;
					this.arrowRotationVelocity = 0.0;
					bool colorChoiceGreen2 = Game1.currentLocation.currentEvent.specialEventVariable2;
					bool won = false;
					if (this.arrowRotation > 1.5707963267948966 && this.arrowRotation <= 4.71238898038469)
					{
						if (!colorChoiceGreen2)
						{
							won = true;
						}
					}
					else if (colorChoiceGreen2)
					{
						won = true;
					}
					if (won)
					{
						Game1.playSound("reward", null);
						this.resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11829"), Color.Lime, Color.White, false, 0.1, 2500, -1, 500, 1f);
						Game1.player.festivalScore += this.wager;
					}
					else
					{
						this.resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11830"), Color.Red, Color.Transparent, false, 0.1, 2500, -1, 500, 1f);
						Game1.playSound("fishEscape", null);
						Game1.player.festivalScore -= this.wager;
					}
				}
				double num = this.arrowRotation;
				this.arrowRotation += this.arrowRotationVelocity;
				if (num % 1.5707963267948966 > this.arrowRotation % 1.5707963267948966)
				{
					Game1.playSound("Cowboy_gunshot", null);
				}
				this.arrowRotation %= 6.283185307179586;
			}
			else
			{
				this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
				if (this.timerBeforeStart <= 0)
				{
					Game1.playSound("cowboy_monsterhit", null);
				}
			}
			if (this.resultText != null && this.resultText.update(time))
			{
				this.resultText = null;
			}
			if (this.doneSpinning && this.resultText == null)
			{
				Game1.exitActiveMenu();
				Game1.player.canMove = true;
			}
		}

		// Token: 0x06002D72 RID: 11634 RVA: 0x00239179 File Offset: 0x00237379
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x06002D73 RID: 11635 RVA: 0x0023917B File Offset: 0x0023737B
		public override void receiveKeyPress(Keys key)
		{
		}

		// Token: 0x06002D74 RID: 11636 RVA: 0x00239180 File Offset: 0x00237380
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			b.Draw(Game1.mouseCursors, new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen), new Rectangle?(new Rectangle(128, 1184, 160, 112)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.95f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 320), (float)(this.yPositionOnScreen + 224 + 4)), new Rectangle?(new Rectangle(120, 1234, 8, 16)), Color.White, (float)this.arrowRotation, new Vector2(4f, 15f), 4f, SpriteEffects.None, 0.96f);
			SparklingText sparklingText = this.resultText;
			if (sparklingText == null)
			{
				return;
			}
			sparklingText.draw(b, new Vector2((float)(this.xPositionOnScreen + 320) - this.resultText.textWidth, (float)(this.yPositionOnScreen - 64)));
		}

		// Token: 0x04001F39 RID: 7993
		public new const int width = 640;

		// Token: 0x04001F3A RID: 7994
		public new const int height = 448;

		// Token: 0x04001F3B RID: 7995
		public double arrowRotation;

		// Token: 0x04001F3C RID: 7996
		public double arrowRotationVelocity;

		// Token: 0x04001F3D RID: 7997
		public double arrowRotationDeceleration;

		// Token: 0x04001F3E RID: 7998
		private int timerBeforeStart;

		// Token: 0x04001F3F RID: 7999
		private int wager;

		// Token: 0x04001F40 RID: 8000
		private SparklingText resultText;

		// Token: 0x04001F41 RID: 8001
		private bool doneSpinning;
	}
}
