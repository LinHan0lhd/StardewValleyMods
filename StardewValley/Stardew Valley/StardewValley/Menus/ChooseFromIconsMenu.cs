using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	// Token: 0x0200025B RID: 603
	public class ChooseFromIconsMenu : IClickableMenu
	{
		// Token: 0x0600281B RID: 10267 RVA: 0x001D2610 File Offset: 0x001D0810
		public ChooseFromIconsMenu(string which)
		{
			this.setUpIcons(which);
		}

		// Token: 0x0600281C RID: 10268 RVA: 0x001D266B File Offset: 0x001D086B
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.setUpIcons(this.which);
		}

		// Token: 0x0600281D RID: 10269 RVA: 0x001D2684 File Offset: 0x001D0884
		public void setUpIcons(string which)
		{
			int iconSpacing = 32;
			int iconOffsetXMargin = 12;
			int iconOffsetYMargin = 4;
			this.which = which;
			this.title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseOne");
			this.hoverSound = "boulderCrack";
			this.icons.Clear();
			this.iconFronts.Clear();
			if (!(which == "dwarfStatue"))
			{
				if (which == "bobbers")
				{
					if (Game1.player.usingRandomizedBobber)
					{
						Game1.player.bobberStyle.Value = -2;
					}
					int available = Game1.player.fishCaught.Count() / 2;
					iconSpacing = 4;
					this.iconBackRectangle = new Rectangle(222, 317, 16, 16);
					this.iconBackHighlightPosition = new Point(256, 317);
					this.texture = Game1.mouseCursors_1_6;
					for (int i = 0; i < FishingRod.NUM_BOBBER_STYLES; i++)
					{
						bool flag = i > available;
						Rectangle src = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, i, 16, 32);
						src.Height = 16;
						this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), this.texture, this.iconBackRectangle, 4f, true)
						{
							name = (i.ToString() ?? "")
						});
						if (flag)
						{
							this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.mouseCursors_1_6, new Rectangle(272, 317, 16, 16), 4f, false)
							{
								name = "ghosted"
							});
						}
						else
						{
							this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.bobbersTexture, src, 4f, true));
						}
					}
					this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, new Rectangle(0, 0, 0, 0), 4f, true)
					{
						name = "-2"
					});
					this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 10, 10), Game1.mouseCursors_1_6, new Rectangle(496, 28, 16, 16), 4f, true));
					this.selected = Game1.player.bobberStyle.Value;
					iconOffsetXMargin = 0;
					iconOffsetYMargin = 0;
					this.hasTooltips = false;
					this.title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseBobber");
					this.titleStyle = 0;
					this.hoverSound = null;
				}
			}
			else
			{
				Game1.playSound("stone_button", null);
				this.iconBackRectangle = new Rectangle(127, 123, 21, 21);
				this.iconBackHighlightPosition = new Point(127, 144);
				this.iconFrontHighlightPositionOffset = new Point(0, 17);
				this.texture = Game1.mouseCursors_1_6;
				Random dwarf_random = Utility.CreateRandom(Game1.stats.DaysPlayed * 77U, Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0);
				int icon = dwarf_random.Next(5);
				int icon2;
				do
				{
					icon2 = dwarf_random.Next(5);
				}
				while (icon2 == icon);
				this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), this.texture, this.iconBackRectangle, 4f, true)
				{
					name = (icon.ToString() ?? ""),
					hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon.ToString())
				});
				this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), this.texture, this.iconBackRectangle, 4f, true)
				{
					name = (icon2.ToString() ?? ""),
					hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon2.ToString())
				});
				this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), this.texture, new Rectangle(148 + icon * 17, 123, 17, 17), 4f, false));
				this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), this.texture, new Rectangle(148 + icon2 * 17, 123, 17, 17), 4f, false));
			}
			int toolTipWidth = this.hasTooltips ? 240 : 0;
			int iconWidth = Math.Max(this.iconBackRectangle.Width * 4, toolTipWidth) + iconSpacing;
			this.iconXOffset = iconWidth / 2 - this.iconBackRectangle.Width * 4 / 2 - 4;
			this.width = Math.Max(800, Game1.uiViewport.Width / 3);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
			this.height = 100;
			this.maxTooltipHeight = 0;
			this.maxTooltipWidth = 0;
			if (this.hasTooltips)
			{
				foreach (ClickableTextureComponent j in this.icons)
				{
					j.hoverText = Game1.parseText(j.hoverText, Game1.smallFont, toolTipWidth - 32);
					this.maxTooltipHeight = Math.Max(this.maxTooltipHeight, (int)Game1.smallFont.MeasureString(j.hoverText).Y);
					this.maxTooltipWidth = Math.Max(this.maxTooltipWidth, (int)Game1.smallFont.MeasureString(j.hoverText).X);
				}
				this.maxTooltipHeight += 48;
				this.maxTooltipWidth += 48;
			}
			this.height += (this.icons.Count * iconWidth / this.width + 1) * (this.maxTooltipHeight + this.icons[0].bounds.Height + iconSpacing);
			int maxIconsPerRow = this.width / iconWidth;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
			int y = this.yPositionOnScreen + 100;
			for (int k = 0; k < this.icons.Count; k += maxIconsPerRow)
			{
				int rowCount = Math.Min(this.icons.Count - k, maxIconsPerRow);
				int x = this.xPositionOnScreen + this.width / 2 - rowCount * iconWidth / 2;
				for (int l = 0; l < rowCount; l++)
				{
					int index = l + k;
					this.icons[index].bounds.X = x + l * iconWidth;
					this.icons[index].bounds.Y = y;
					this.icons[index].bounds.Width = iconWidth;
					ClickableTextureComponent clickableTextureComponent = this.icons[index];
					clickableTextureComponent.bounds.Height = clickableTextureComponent.bounds.Height + this.maxTooltipHeight;
					this.iconFronts[index].bounds.X = this.icons[index].bounds.X + iconOffsetXMargin;
					this.iconFronts[index].bounds.Y = this.icons[index].bounds.Y + iconOffsetYMargin;
					this.icons[index].myID = index;
					this.icons[index].leftNeighborID = index - 1;
					this.icons[index].rightNeighborID = index + 1;
					this.icons[index].downNeighborID = index + rowCount;
					this.icons[index].upNeighborID = index - rowCount;
				}
				y += this.maxTooltipHeight + this.icons[0].bounds.Height + iconSpacing;
			}
			base.initialize(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, true);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x0600281E RID: 10270 RVA: 0x001D2EE4 File Offset: 0x001D10E4
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.destroyTimer > 0f)
			{
				this.destroyTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.destroyTimer <= 0f)
				{
					this.flairOnDestroy();
					Game1.activeClickableMenu = null;
				}
			}
			this.temporarySprites.RemoveAll((TemporaryAnimatedSprite sprite) => sprite.update(time));
		}

		// Token: 0x0600281F RID: 10271 RVA: 0x001D2F6C File Offset: 0x001D116C
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			for (int i = 0; i < this.icons.Count; i++)
			{
				ClickableTextureComponent c = this.icons[i];
				this.iconFronts[i].sourceRect = this.iconFronts[i].startingSourceRect;
				if (c.containsPoint(x, y) && this.destroyTimer == -1f)
				{
					if (c.sourceRect == c.startingSourceRect && this.hoverSound != null)
					{
						Game1.playSound(this.hoverSound, null);
					}
					c.sourceRect.Location = this.iconBackHighlightPosition;
					this.iconFronts[i].sourceRect.Location = new Point(this.iconFronts[i].sourceRect.Location.X + this.iconFrontHighlightPositionOffset.X, this.iconFronts[i].sourceRect.Location.Y + this.iconFrontHighlightPositionOffset.Y);
				}
				else
				{
					c.sourceRect = this.iconBackRectangle;
				}
			}
		}

		// Token: 0x06002820 RID: 10272 RVA: 0x001D30A4 File Offset: 0x001D12A4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.destroyTimer >= 0f)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			for (int i = 0; i < this.icons.Count; i++)
			{
				ClickableTextureComponent c = this.icons[i];
				if (c.containsPoint(x, y))
				{
					bool ghosted = this.iconFronts[i].name.Contains("ghosted");
					string a = this.which;
					if (!(a == "dwarfStatue"))
					{
						if (a == "bobbers")
						{
							if (ghosted)
							{
								Game1.playSound("smallSelect", null);
								return;
							}
							int selection = Convert.ToInt32(c.name);
							if (Game1.player.bobberStyle.Value != selection)
							{
								Game1.playSound("button1", null);
								this.hoverSound = null;
								Game1.player.bobberStyle.Value = Convert.ToInt32(c.name);
								this.selected = Game1.player.bobberStyle.Value;
								if (this.selected == -2)
								{
									Game1.player.usingRandomizedBobber = true;
								}
								else
								{
									Game1.player.usingRandomizedBobber = false;
								}
							}
						}
					}
					else
					{
						Game1.playSound("button_tap", null);
						DelayedAction.playSoundAfterDelay("button_tap", 70, null, null, -1, false);
						DelayedAction.playSoundAfterDelay("discoverMineral", 750, null, null, -1, false);
						for (int j = 0; j < 16; j++)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(98 + Game1.random.Next(3) * 4, 161, 4, 4), Utility.getRandomPositionInThisRectangle(c.bounds, Game1.random), false, 0f, Color.White)
							{
								local = true,
								scale = 4f,
								interval = 9999f,
								motion = new Vector2((float)Game1.random.Next(-15, 16) / 10f, -7f + (float)Game1.random.Next(-10, 11) / 10f),
								acceleration = new Vector2(0f, 0.5f)
							});
						}
						this.destroyTimer = 800f;
					}
					this.doIconAction(c.name);
				}
			}
		}

		// Token: 0x06002821 RID: 10273 RVA: 0x001D331F File Offset: 0x001D151F
		private void doIconAction(string iconName)
		{
			if (this.which == "dwarfStatue" && !Game1.player.hasBuffWithNameContainingString("dwarfStatue"))
			{
				Game1.player.applyBuff(this.which + "_" + iconName);
			}
		}

		// Token: 0x06002822 RID: 10274 RVA: 0x001D3360 File Offset: 0x001D1560
		private void flairOnDestroy()
		{
			if (this.which == "dwarfStatue")
			{
				this.sourceObject.shakeTimer = 500;
				if (this.sourceObject.Location != null)
				{
					Utility.addSprinklesToLocation(this.sourceObject.Location, (int)this.sourceObject.TileLocation.X, (int)this.sourceObject.TileLocation.Y, 3, 4, 800, 40, Color.White, null, false);
				}
			}
		}

		// Token: 0x06002823 RID: 10275 RVA: 0x001D33E0 File Offset: 0x001D15E0
		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.7f);
			base.draw(b);
			SpriteText.drawStringWithScrollCenteredAt(b, this.title, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + 20, "", 1f, new Color?((this.titleStyle == 3) ? Color.LightGray : Game1.textColor), this.titleStyle, 0.88f, false);
			for (int i = 0; i < this.icons.Count; i++)
			{
				if (this.selected == i || (this.selected == -2 && i == this.icons.Count - 1))
				{
					if (this.selected == i)
					{
						Rectangle rect = this.icons[i].bounds;
						rect.Inflate(2, 4);
						rect.X += this.iconXOffset - 2;
						b.Draw(Game1.staminaRect, rect, Color.Red);
						if (this.icons[i].sourceRect.Width > 0)
						{
							this.icons[i].sourceRect.X = this.iconBackHighlightPosition.X;
							this.icons[i].sourceRect.Y = this.iconBackHighlightPosition.Y;
						}
					}
					else
					{
						b.Draw(Game1.mouseCursors_1_6, this.icons[i].getVector2(), new Rectangle?(new Rectangle(480, 28, 16, 16)), Color.Red, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
				this.icons[i].draw(b, Color.White, 0f, 0, this.iconXOffset, 0);
				this.iconFronts[i].draw(b, this.iconFronts[i].name.Equals("ghosted_fade") ? (Color.Black * 0.4f) : Color.White, 0.87f, 0, this.iconXOffset, 0);
				IClickableMenu.drawHoverText(b, this.icons[i].hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, this.icons[i].bounds.X + 4, this.icons[i].bounds.Y + this.icons[i].bounds.Height - this.maxTooltipHeight + 4, 1f, null, null, Game1.mouseCursors_1_6, new Rectangle?((this.icons[i].sourceRect != this.icons[i].startingSourceRect) ? new Rectangle(111, 145, 15, 15) : new Rectangle(96, 145, 15, 15)), new Color?(Color.White), new Color?(new Color(26, 26, 43)), 4f, this.maxTooltipWidth, this.maxTooltipHeight);
			}
			foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in this.temporarySprites)
			{
				temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
			}
			base.drawMouse(b, false, -1);
		}

		// Token: 0x040019B9 RID: 6585
		private Rectangle iconBackRectangle;

		// Token: 0x040019BA RID: 6586
		private Texture2D texture;

		// Token: 0x040019BB RID: 6587
		private Point iconBackHighlightPosition;

		// Token: 0x040019BC RID: 6588
		private Point iconFrontHighlightPositionOffset;

		// Token: 0x040019BD RID: 6589
		private string which;

		// Token: 0x040019BE RID: 6590
		public List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();

		// Token: 0x040019BF RID: 6591
		public List<ClickableTextureComponent> iconFronts = new List<ClickableTextureComponent>();

		// Token: 0x040019C0 RID: 6592
		private int iconXOffset;

		// Token: 0x040019C1 RID: 6593
		private int maxTooltipHeight;

		// Token: 0x040019C2 RID: 6594
		private int maxTooltipWidth;

		// Token: 0x040019C3 RID: 6595
		private float destroyTimer = -1f;

		// Token: 0x040019C4 RID: 6596
		private List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

		// Token: 0x040019C5 RID: 6597
		public Object sourceObject;

		// Token: 0x040019C6 RID: 6598
		private bool hasTooltips = true;

		// Token: 0x040019C7 RID: 6599
		private string title;

		// Token: 0x040019C8 RID: 6600
		private string hoverSound;

		// Token: 0x040019C9 RID: 6601
		private int titleStyle = 3;

		// Token: 0x040019CA RID: 6602
		private int selected = -1;
	}
}
