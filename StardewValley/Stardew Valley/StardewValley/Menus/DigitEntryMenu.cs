using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x02000267 RID: 615
	internal class DigitEntryMenu : NumberSelectionMenu
	{
		// Token: 0x060028DB RID: 10459 RVA: 0x001DF810 File Offset: 0x001DDA10
		public DigitEntryMenu(string message, NumberSelectionMenu.behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0) : base(message, behaviorOnSelection, price, minValue, maxValue, defaultNumber)
		{
			int buttonsPerRow = 3;
			int buttonWidth = 44;
			int buttonHeight = buttonWidth;
			int bufferX = 8;
			int bufferY = bufferX;
			int rowWidth = buttonsPerRow * buttonWidth + (buttonsPerRow - 1) * bufferX;
			this.calculatorWidth = buttonWidth * buttonsPerRow + bufferX * (buttonsPerRow - 1) + IClickableMenu.spaceToClearSideBorder * 2 + 128;
			this.calculatorHeight = buttonHeight * 4 + bufferY * 3 + IClickableMenu.spaceToClearTopBorder * 2;
			this.calculatorX = Game1.uiViewport.Width / 2 - this.calculatorWidth / 2;
			this.calculatorY = Game1.uiViewport.Height / 2 - this.calculatorHeight;
			int buttonX = Game1.uiViewport.Width / 2;
			int buttonY = Game1.uiViewport.Height / 2 - 384 + 24 + IClickableMenu.spaceToClearTopBorder;
			for (int i = 0; i < 11; i++)
			{
				string digit;
				if (i != 9)
				{
					if (i != 10)
					{
						digit = (i + 1).ToString();
					}
					else
					{
						digit = "0";
					}
				}
				else
				{
					digit = DigitEntryMenu.clear;
				}
				this.digits.Add(new ClickableComponent(new Rectangle(buttonX - rowWidth / 2 + i % buttonsPerRow * (bufferX + buttonWidth), buttonY + i / buttonsPerRow * (bufferY + buttonHeight), buttonWidth, buttonHeight), digit)
				{
					myID = i,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998,
					upNeighborID = -99998
				});
			}
			this.populateClickableComponentList();
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x060028DC RID: 10460 RVA: 0x001DF995 File Offset: 0x001DDB95
		protected override Vector2 centerPosition
		{
			get
			{
				return new Vector2((float)(Game1.uiViewport.Width / 2), (float)(Game1.uiViewport.Height / 2 + 128));
			}
		}

		// Token: 0x060028DD RID: 10461 RVA: 0x001DF9BC File Offset: 0x001DDBBC
		private void onDigitPressed(string digit)
		{
			if (digit == DigitEntryMenu.clear)
			{
				this.currentValue = 0;
				this.numberSelectedBox.Text = this.currentValue.ToString();
				return;
			}
			string currentStr = this.currentValue.ToString();
			if (currentStr == "0")
			{
				currentStr = digit;
			}
			else
			{
				currentStr += digit;
			}
			this.currentValue = Math.Min(this.maxValue, Convert.ToInt32(currentStr));
			this.numberSelectedBox.Text = this.currentValue.ToString();
		}

		// Token: 0x060028DE RID: 10462 RVA: 0x001DFA48 File Offset: 0x001DDC48
		public override bool isWithinBounds(int x, int y)
		{
			return base.isWithinBounds(x, y) || (x - this.calculatorX < this.calculatorWidth && x - this.calculatorX >= 0 && y - this.calculatorY < this.calculatorHeight && y - this.calculatorY >= 0);
		}

		// Token: 0x060028DF RID: 10463 RVA: 0x001DFA9C File Offset: 0x001DDC9C
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in this.digits)
			{
				if (c.containsPoint(x, y))
				{
					Game1.playSound("smallSelect", null);
					this.onDigitPressed(c.name);
				}
			}
			base.receiveLeftClick(x, y, true);
		}

		// Token: 0x060028E0 RID: 10464 RVA: 0x001DFB1C File Offset: 0x001DDD1C
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent c in this.digits)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = 2f;
				}
				else
				{
					c.scale = 1f;
				}
			}
		}

		// Token: 0x060028E1 RID: 10465 RVA: 0x001DFB94 File Offset: 0x001DDD94
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Game1.drawDialogueBox(this.calculatorX, this.calculatorY, this.calculatorWidth, this.calculatorHeight, false, true, null, false, true, -1, -1, -1);
			foreach (ClickableComponent c in this.digits)
			{
				if (c.name == DigitEntryMenu.clear)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X - 4), (float)(c.bounds.Y + 4)), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2((float)c.bounds.X, (float)c.bounds.Y), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					Vector2 textPosition = new Vector2((float)(c.bounds.X + c.bounds.Width / 2 - SpriteText.getWidthOfString(c.name, 999999) / 2), (float)(c.bounds.Y + c.bounds.Height / 2 - SpriteText.getHeightOfString(c.name, 999999) / 2 - 4));
					SpriteText.drawString(b, c.name, (int)textPosition.X, (int)textPosition.Y, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X - 4), (float)(c.bounds.Y + 4)), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2((float)c.bounds.X, (float)c.bounds.Y), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					Vector2 textPosition2 = new Vector2((float)(c.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(c.name)) * 6), (float)(c.bounds.Y + 24 - NumberSprite.getHeight() / 4));
					NumberSprite.draw(Convert.ToInt32(c.name), b, textPosition2, Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
				}
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001AA8 RID: 6824
		public List<ClickableComponent> digits = new List<ClickableComponent>();

		// Token: 0x04001AA9 RID: 6825
		private int calculatorX;

		// Token: 0x04001AAA RID: 6826
		private int calculatorY;

		// Token: 0x04001AAB RID: 6827
		private int calculatorWidth;

		// Token: 0x04001AAC RID: 6828
		private int calculatorHeight;

		// Token: 0x04001AAD RID: 6829
		private static string clear = "c";
	}
}
