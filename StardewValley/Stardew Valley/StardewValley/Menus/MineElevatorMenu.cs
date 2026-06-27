using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	// Token: 0x0200028A RID: 650
	public class MineElevatorMenu : IClickableMenu
	{
		// Token: 0x06002B0D RID: 11021 RVA: 0x00208A74 File Offset: 0x00206C74
		public MineElevatorMenu() : base(0, 0, 0, 0, true)
		{
			int numElevators = Math.Min(MineShaft.lowestLevelReached, 120) / 5;
			this.width = ((numElevators > 50) ? (484 + IClickableMenu.borderWidth * 2) : Math.Min(220 + IClickableMenu.borderWidth * 2, numElevators * 44 + IClickableMenu.borderWidth * 2));
			this.height = Math.Max(64 + IClickableMenu.borderWidth * 3, numElevators * 44 / (this.width - IClickableMenu.borderWidth) * 44 + 64 + IClickableMenu.borderWidth * 3);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
			Game1.playSound("crystal", new int?(0));
			int buttonsPerRow = this.width / 44 - 1;
			int x = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
			int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
			this.elevators.Add(new ClickableComponent(new Rectangle(x, y, 44, 44), 0.ToString() ?? "")
			{
				myID = 0,
				rightNeighborID = 1,
				downNeighborID = buttonsPerRow
			});
			x = x + 64 - 20;
			if (x > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
			{
				x = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
				y += 44;
			}
			for (int i = 1; i <= numElevators; i++)
			{
				this.elevators.Add(new ClickableComponent(new Rectangle(x, y, 44, 44), (i * 5).ToString() ?? "")
				{
					myID = i,
					rightNeighborID = ((i % buttonsPerRow == buttonsPerRow - 1) ? -1 : (i + 1)),
					leftNeighborID = ((i % buttonsPerRow == 0) ? -1 : (i - 1)),
					downNeighborID = i + buttonsPerRow,
					upNeighborID = i - buttonsPerRow
				});
				x = x + 64 - 20;
				if (x > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
				{
					x = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
					y += 44;
				}
			}
			base.initializeUpperRightCloseButton();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002B0E RID: 11022 RVA: 0x00208CFE File Offset: 0x00206EFE
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002B0F RID: 11023 RVA: 0x00208D14 File Offset: 0x00206F14
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.isWithinBounds(x, y))
			{
				foreach (ClickableComponent c in this.elevators)
				{
					if (c.containsPoint(x, y))
					{
						Game1.playSound("smallSelect", null);
						if (Convert.ToInt32(c.name) == 0)
						{
							if (!(Game1.currentLocation is MineShaft))
							{
								return;
							}
							Game1.warpFarmer("Mine", 17, 4, true);
							Game1.exitActiveMenu();
						}
						else
						{
							if (Convert.ToInt32(c.name) == Game1.CurrentMineLevel)
							{
								return;
							}
							Game1.player.ridingMineElevator = true;
							Game1.enterMine(Convert.ToInt32(c.name), null);
							Game1.exitActiveMenu();
						}
					}
				}
				base.receiveLeftClick(x, y, true);
				return;
			}
			Game1.exitActiveMenu();
		}

		// Token: 0x06002B10 RID: 11024 RVA: 0x00208E10 File Offset: 0x00207010
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent c in this.elevators)
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

		// Token: 0x06002B11 RID: 11025 RVA: 0x00208E88 File Offset: 0x00207088
		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen - 64 + 8, this.width + 21, this.height + 64, false, true, null, false, true, -1, -1, -1);
			foreach (ClickableComponent c in this.elevators)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X - 4), (float)(c.bounds.Y + 4)), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(Game1.mouseCursors, new Vector2((float)c.bounds.X, (float)c.bounds.Y), new Rectangle?(new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
				Vector2 textPosition = new Vector2((float)(c.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(c.name)) * 6), (float)(c.bounds.Y + 24 - NumberSprite.getHeight() / 4));
				NumberSprite.draw(Convert.ToInt32(c.name), b, textPosition, (Game1.CurrentMineLevel == Convert.ToInt32(c.name)) ? (Color.Gray * 0.75f) : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
			}
			base.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001CAB RID: 7339
		public List<ClickableComponent> elevators = new List<ClickableComponent>();
	}
}
