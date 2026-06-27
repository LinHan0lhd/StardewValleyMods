using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x020002B8 RID: 696
	public class TutorialMenu : IClickableMenu
	{
		// Token: 0x06002D6B RID: 11627 RVA: 0x0023830C File Offset: 0x0023650C
		public TutorialMenu() : base(Game1.uiViewport.Width / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 192, 600 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 192, false)
		{
			int xPos = this.xPositionOnScreen + 64 + 42 - 2;
			int yPos = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11805"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 276, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11807"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 142, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11809"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 334, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11811"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 308, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11813"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 395, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11815"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11817"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 102, -1, -1), 1f, false));
			yPos += 68;
			this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, this.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11819"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f, false));
			this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 403, -1, -1), 1f, false));
			yPos += 68;
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
			this.backButton = new ClickableTextureComponent("Back", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 48, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
		}

		// Token: 0x06002D6C RID: 11628 RVA: 0x00238908 File Offset: 0x00236B08
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.currentTab == -1)
			{
				for (int i = 0; i < this.topics.Count; i++)
				{
					if (this.topics[i].containsPoint(x, y))
					{
						this.currentTab = i;
						Game1.playSound("smallSelect", null);
						break;
					}
				}
			}
			if (this.currentTab != -1 && this.backButton.containsPoint(x, y))
			{
				this.currentTab = -1;
				Game1.playSound("bigDeSelect", null);
				return;
			}
			if (this.currentTab == -1 && this.okButton.containsPoint(x, y))
			{
				Game1.playSound("bigDeSelect", null);
				Game1.exitActiveMenu();
				if (Game1.currentLocation.currentEvent != null)
				{
					Event currentEvent = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent.CurrentCommand;
					currentEvent.CurrentCommand = currentCommand + 1;
				}
			}
		}

		// Token: 0x06002D6D RID: 11629 RVA: 0x002389F0 File Offset: 0x00236BF0
		public override void performHoverAction(int x, int y)
		{
			foreach (ClickableComponent c in this.topics)
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
			if (this.okButton.containsPoint(x, y))
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}
			if (this.backButton.containsPoint(x, y))
			{
				this.backButton.scale = Math.Min(this.backButton.scale + 0.02f, this.backButton.baseScale + 0.1f);
				return;
			}
			this.backButton.scale = Math.Max(this.backButton.scale - 0.02f, this.backButton.baseScale);
		}

		// Token: 0x06002D6E RID: 11630 RVA: 0x00238B3C File Offset: 0x00236D3C
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			if (this.currentTab != -1)
			{
				this.backButton.draw(b);
				b.Draw(this.topics[this.currentTab].texture, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16)), new Rectangle?(this.topics[this.currentTab].texture.Bounds), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.89f);
			}
			else
			{
				foreach (ClickableTextureComponent c in this.topics)
				{
					Color color = (c.scale > 1f) ? Color.Blue : Game1.textColor;
					b.DrawString(Game1.smallFont, c.label, new Vector2((float)(c.bounds.X + 64 + 16), (float)(c.bounds.Y + 21)), color);
				}
				foreach (ClickableTextureComponent clickableTextureComponent in this.icons)
				{
					clickableTextureComponent.draw(b);
				}
				this.okButton.draw(b);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001F30 RID: 7984
		public const int constructionTab = 4;

		// Token: 0x04001F31 RID: 7985
		public const int friendshipTab = 5;

		// Token: 0x04001F32 RID: 7986
		public const int townTab = 6;

		// Token: 0x04001F33 RID: 7987
		public const int animalsTab = 7;

		// Token: 0x04001F34 RID: 7988
		private int currentTab = -1;

		// Token: 0x04001F35 RID: 7989
		private List<ClickableTextureComponent> topics = new List<ClickableTextureComponent>();

		// Token: 0x04001F36 RID: 7990
		private ClickableTextureComponent backButton;

		// Token: 0x04001F37 RID: 7991
		private ClickableTextureComponent okButton;

		// Token: 0x04001F38 RID: 7992
		private List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();
	}
}
