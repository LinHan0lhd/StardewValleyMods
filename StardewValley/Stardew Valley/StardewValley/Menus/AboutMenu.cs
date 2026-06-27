using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x02000246 RID: 582
	public class AboutMenu : IClickableMenu
	{
		// Token: 0x060026D8 RID: 9944 RVA: 0x001B7764 File Offset: 0x001B5964
		public AboutMenu()
		{
			this.width = 1280;
			this.height = 700;
			this.SetUpCredits();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x060026D9 RID: 9945 RVA: 0x001B77C4 File Offset: 0x001B59C4
		public void SetUpCredits()
		{
			foreach (string line in Game1.temporaryContent.Load<List<string>>("Strings\\credits"))
			{
				if (line != null && line.Length >= 6 && line.StartsWith("[image"))
				{
					string[] split = ArgUtility.SplitBySpace(line);
					string path = split[1];
					int sourceX = Convert.ToInt32(split[2]);
					int sourceY = Convert.ToInt32(split[3]);
					int sourceWidth = Convert.ToInt32(split[4]);
					int sourceHeight = Convert.ToInt32(split[5]);
					int zoom = Convert.ToInt32(split[6]);
					int animationFrames = (split.Length > 7) ? Convert.ToInt32(split[7]) : 1;
					Texture2D tex = null;
					try
					{
						tex = Game1.temporaryContent.Load<Texture2D>(path);
					}
					catch (Exception)
					{
					}
					if (tex != null)
					{
						if (sourceWidth == -1)
						{
							sourceWidth = tex.Width;
							sourceHeight = tex.Height;
						}
						this.credits.Add(new ImageCreditsBlock(tex, new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), zoom, animationFrames));
					}
				}
				else if (line != null && line.Length >= 6 && line.StartsWith("[link"))
				{
					string[] array = ArgUtility.SplitBySpace(line, 3);
					string url = array[1];
					string text = array[2];
					this.credits.Add(new LinkCreditsBlock(text, url));
				}
				else
				{
					this.credits.Add(new TextCreditsBlock(line));
				}
			}
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0);
			this.xPositionOnScreen = (int)topLeft.X;
			this.yPositionOnScreen = (int)topLeft.Y;
			this.upButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + this.width - 80, (int)topLeft.Y + 64 + 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false)
			{
				myID = 94444,
				downNeighborID = 95555,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			};
			this.downButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + this.width - 80, (int)topLeft.Y + this.height - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false)
			{
				myID = 95555,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			};
			this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
			{
				myID = 81114,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = 95555
			};
		}

		// Token: 0x060026DA RID: 9946 RVA: 0x001B7B08 File Offset: 0x001B5D08
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(81114);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x060026DB RID: 9947 RVA: 0x001B7B24 File Offset: 0x001B5D24
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (this.upButton.containsPoint(x, y))
			{
				if (this.currentCreditsIndex > 0)
				{
					this.currentCreditsIndex--;
					Game1.playSound("shiny4", null);
					this.upButton.scale = this.upButton.baseScale;
					return;
				}
			}
			else if (this.downButton.containsPoint(x, y))
			{
				if (this.currentCreditsIndex < this.credits.Count - 1)
				{
					this.currentCreditsIndex++;
					Game1.playSound("shiny4", null);
					this.downButton.scale = this.downButton.baseScale;
					return;
				}
			}
			else if (this.isWithinBounds(x, y))
			{
				int yPos = this.yPositionOnScreen + 96;
				int oldYpos = yPos;
				int i = 0;
				while (yPos < this.yPositionOnScreen + this.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
				{
					yPos += this.credits[this.currentCreditsIndex + i].getHeight(this.width - 64) + ((this.credits.Count > this.currentCreditsIndex + i + 1 && this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock) ? 0 : 8);
					if (y >= oldYpos && y < yPos)
					{
						this.credits[this.currentCreditsIndex + i].clicked();
						return;
					}
					i++;
					oldYpos = yPos;
				}
			}
		}

		// Token: 0x060026DC RID: 9948 RVA: 0x001B7CBC File Offset: 0x001B5EBC
		public override void update(GameTime time)
		{
			base.update(time);
			this.upButton.visible = (this.currentCreditsIndex > 0);
			this.downButton.visible = (this.currentCreditsIndex < this.credits.Count - 1);
		}

		// Token: 0x060026DD RID: 9949 RVA: 0x001B7CFC File Offset: 0x001B5EFC
		public override void receiveScrollWheelAction(int direction)
		{
			if (direction > 0 && this.currentCreditsIndex > 0)
			{
				this.currentCreditsIndex--;
				Game1.playSound("shiny4", null);
				return;
			}
			if (direction < 0 && this.currentCreditsIndex < this.credits.Count - 1)
			{
				this.currentCreditsIndex++;
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x060026DE RID: 9950 RVA: 0x001B7D74 File Offset: 0x001B5F74
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			this.upButton.tryHover(x, y, 0.1f);
			this.downButton.tryHover(x, y, 0.1f);
			if (this.isWithinBounds(x, y))
			{
				int yPos = this.yPositionOnScreen + 96;
				int oldYpos = yPos;
				int i = 0;
				while (yPos < this.yPositionOnScreen + this.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
				{
					yPos += this.credits[this.currentCreditsIndex + i].getHeight(this.width - 64) + ((this.credits.Count > this.currentCreditsIndex + i + 1 && this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock) ? 0 : 8);
					if (y >= oldYpos && y < yPos)
					{
						this.credits[this.currentCreditsIndex + i].hovered();
						return;
					}
					i++;
					oldYpos = yPos;
				}
			}
		}

		// Token: 0x060026DF RID: 9951 RVA: 0x001B7E7C File Offset: 0x001B607C
		public override void draw(SpriteBatch b)
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height - 100, 0, 0);
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X, (int)topLeft.Y, this.width, this.height, Color.White, 4f, false, -1f);
			int yPos = this.yPositionOnScreen + 96;
			int i = 0;
			while (yPos < this.yPositionOnScreen + this.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
			{
				this.credits[this.currentCreditsIndex + i].draw(this.xPositionOnScreen + 32, yPos, this.width - 64, b);
				yPos += this.credits[this.currentCreditsIndex + i].getHeight(this.width - 64) + ((this.credits.Count > this.currentCreditsIndex + i + 1 && this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock) ? 0 : 8);
				i++;
			}
			if (this.currentCreditsIndex > 0)
			{
				this.upButton.draw(b);
			}
			if (this.currentCreditsIndex < this.credits.Count - 1)
			{
				this.downButton.draw(b);
			}
			string versionText = "v" + Game1.GetVersionString();
			float versionTextHeight = Game1.smallFont.MeasureString(versionText).Y;
			b.DrawString(Game1.smallFont, versionText, new Vector2(16f, (float)Game1.uiViewport.Height - versionTextHeight - 8f), Color.White);
			TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
			if (titleMenu != null && !string.IsNullOrWhiteSpace(titleMenu.startupMessage))
			{
				string tipText = Game1.parseText(titleMenu.startupMessage, Game1.smallFont, 640);
				float tipHeight = Game1.smallFont.MeasureString(tipText).Y;
				b.DrawString(Game1.smallFont, tipText, new Vector2(8f, (float)Game1.uiViewport.Height - versionTextHeight - tipHeight - 4f), Color.White);
			}
			base.draw(b);
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x001B80F8 File Offset: 0x001B62F8
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.SetUpCredits();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				int id = (this.currentlySnappedComponent != null) ? this.currentlySnappedComponent.myID : 81114;
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(id);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x0400180E RID: 6158
		public const int region_upArrow = 94444;

		// Token: 0x0400180F RID: 6159
		public const int region_downArrow = 95555;

		// Token: 0x04001810 RID: 6160
		public new const int height = 700;

		// Token: 0x04001811 RID: 6161
		public ClickableComponent backButton;

		// Token: 0x04001812 RID: 6162
		public ClickableTextureComponent upButton;

		// Token: 0x04001813 RID: 6163
		public ClickableTextureComponent downButton;

		// Token: 0x04001814 RID: 6164
		public List<ICreditsBlock> credits = new List<ICreditsBlock>();

		// Token: 0x04001815 RID: 6165
		private int currentCreditsIndex;
	}
}
