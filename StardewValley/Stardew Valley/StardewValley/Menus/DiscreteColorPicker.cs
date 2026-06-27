using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	// Token: 0x02000268 RID: 616
	public class DiscreteColorPicker : IClickableMenu
	{
		// Token: 0x060028E3 RID: 10467 RVA: 0x001DFF38 File Offset: 0x001DE138
		public DiscreteColorPicker(int xPosition, int yPosition, int startingColor = 0, Item itemToDrawColored = null)
		{
			this.xPositionOnScreen = xPosition;
			this.yPositionOnScreen = yPosition;
			this.width = DiscreteColorPicker.totalColors * 9 * 4 + IClickableMenu.borderWidth;
			this.height = 28 + IClickableMenu.borderWidth;
			this.colorSelection = ((startingColor != 0 && DiscreteColorPicker.getColorFromSelection(startingColor) != Color.Black) ? startingColor : 0);
			this.itemToDrawColored = itemToDrawColored;
			Chest chest = this.itemToDrawColored as Chest;
			if (chest != null)
			{
				chest.resetLidFrame();
			}
			this.visible = Game1.player.showChestColorPicker;
		}

		// Token: 0x060028E4 RID: 10468 RVA: 0x001DFFD0 File Offset: 0x001DE1D0
		public DiscreteColorPicker(int xPosition, int yPosition, Color startingColor, Item itemToDrawColored = null) : this(xPosition, yPosition, DiscreteColorPicker.getSelectionFromColor(startingColor), itemToDrawColored)
		{
		}

		// Token: 0x060028E5 RID: 10469 RVA: 0x001DFFE4 File Offset: 0x001DE1E4
		public static int getSelectionFromColor(Color c)
		{
			for (int i = 0; i < DiscreteColorPicker.totalColors; i++)
			{
				if (DiscreteColorPicker.getColorFromSelection(i).Equals(c))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060028E6 RID: 10470 RVA: 0x001E0015 File Offset: 0x001DE215
		public Color getCurrentColor()
		{
			return DiscreteColorPicker.getColorFromSelection(this.colorSelection);
		}

		// Token: 0x060028E7 RID: 10471 RVA: 0x001E0024 File Offset: 0x001DE224
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.visible)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			Rectangle area = new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth / 2, this.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36 * DiscreteColorPicker.totalColors, 28);
			if (area.Contains(x, y))
			{
				this.colorSelection = (x - area.X) / 36;
				try
				{
					Game1.playSound("coin", null);
				}
				catch
				{
				}
				Chest chest = this.itemToDrawColored as Chest;
				if (chest != null)
				{
					chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.colorSelection);
					chest.resetLidFrame();
				}
			}
		}

		// Token: 0x060028E8 RID: 10472 RVA: 0x001E00E0 File Offset: 0x001DE2E0
		public static Color getColorFromSelection(int selection)
		{
			switch (selection)
			{
			case 1:
				return new Color(85, 85, 255);
			case 2:
				return new Color(119, 191, 255);
			case 3:
				return new Color(0, 170, 170);
			case 4:
				return new Color(0, 234, 175);
			case 5:
				return new Color(0, 170, 0);
			case 6:
				return new Color(159, 236, 0);
			case 7:
				return new Color(255, 234, 18);
			case 8:
				return new Color(255, 167, 18);
			case 9:
				return new Color(255, 105, 18);
			case 10:
				return new Color(255, 0, 0);
			case 11:
				return new Color(135, 0, 35);
			case 12:
				return new Color(255, 173, 199);
			case 13:
				return new Color(255, 117, 195);
			case 14:
				return new Color(172, 0, 198);
			case 15:
				return new Color(143, 0, 255);
			case 16:
				return new Color(89, 11, 142);
			case 17:
				return new Color(64, 64, 64);
			case 18:
				return new Color(100, 100, 100);
			case 19:
				return new Color(200, 200, 200);
			case 20:
				return new Color(254, 254, 254);
			default:
				return Color.Black;
			}
		}

		// Token: 0x060028E9 RID: 10473 RVA: 0x001E0298 File Offset: 0x001DE498
		public override void draw(SpriteBatch b)
		{
			if (this.visible)
			{
				IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.LightGray);
				for (int i = 0; i < DiscreteColorPicker.totalColors; i++)
				{
					if (i == 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth / 2), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth / 2)), new Rectangle?(new Rectangle(295, 503, 7, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
					else
					{
						b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, this.yPositionOnScreen + IClickableMenu.borderWidth / 2, 28, 28), DiscreteColorPicker.getColorFromSelection(i));
					}
					if (i == this.colorSelection)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4 + i * 9 * 4, this.yPositionOnScreen + IClickableMenu.borderWidth / 2 - 4, 36, 36, Color.Black, 4f, false, -1f);
					}
				}
				Chest chest = this.itemToDrawColored as Chest;
				if (chest != null)
				{
					chest.draw(b, this.xPositionOnScreen + this.width + IClickableMenu.borderWidth / 2, this.yPositionOnScreen + 16, 1f, true);
				}
			}
		}

		// Token: 0x04001AAE RID: 6830
		public const int sizeOfEachSwatch = 7;

		// Token: 0x04001AAF RID: 6831
		public Item itemToDrawColored;

		// Token: 0x04001AB0 RID: 6832
		public bool visible = true;

		// Token: 0x04001AB1 RID: 6833
		public static int totalColors = 21;

		// Token: 0x04001AB2 RID: 6834
		public int colorSelection;
	}
}
