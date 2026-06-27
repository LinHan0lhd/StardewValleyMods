using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x020002B6 RID: 694
	public class Toolbar : IClickableMenu
	{
		// Token: 0x06002D5F RID: 11615 RVA: 0x0023759C File Offset: 0x0023579C
		public Toolbar() : base(Game1.uiViewport.Width / 2 - 384 - 64, Game1.uiViewport.Height, 896, 208, false)
		{
			for (int i = 0; i < 12; i++)
			{
				this.buttons.Add(new ClickableComponent(new Rectangle(Game1.uiViewport.Width / 2 - 384 + i * 64, this.yPositionOnScreen - 96 + 8, 64, 64), i.ToString() ?? ""));
			}
		}

		// Token: 0x06002D60 RID: 11616 RVA: 0x002376D4 File Offset: 0x002358D4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!Game1.player.UsingTool && !Game1.IsChatting && Game1.farmEvent == null)
			{
				foreach (ClickableComponent c in this.buttons)
				{
					if (c.containsPoint(x, y))
					{
						Game1.player.CurrentToolIndex = Convert.ToInt32(c.name);
						if (Game1.player.ActiveObject != null)
						{
							Game1.player.showCarrying();
							Game1.playSound("pickUpItem", null);
							break;
						}
						Game1.player.showNotCarrying();
						Game1.playSound("stoneStep", null);
						break;
					}
				}
			}
		}

		// Token: 0x06002D61 RID: 11617 RVA: 0x002377B0 File Offset: 0x002359B0
		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				foreach (ClickableComponent c in this.buttons)
				{
					if (c.containsPoint(x, y))
					{
						int slotNumber = Convert.ToInt32(c.name);
						if (slotNumber < Game1.player.Items.Count && Game1.player.Items[slotNumber] != null)
						{
							this.hoverItem = Game1.player.Items[slotNumber];
							if (this.hoverItem.canBeDropped())
							{
								Game1.playSound("throwDownITem", null);
								Game1.player.Items[slotNumber] = null;
								Game1.createItemDebris(this.hoverItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null, -1, false).DroppedByPlayerID.Value = Game1.player.UniqueMultiplayerID;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06002D62 RID: 11618 RVA: 0x002378F4 File Offset: 0x00235AF4
		public override void performHoverAction(int x, int y)
		{
			if (this.hoverDirty)
			{
				this.gameWindowSizeChanged(new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height), new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
				this.hoverDirty = false;
			}
			this.hoverItem = null;
			foreach (ClickableComponent c in this.buttons)
			{
				if (c.containsPoint(x, y))
				{
					int slotNumber = Convert.ToInt32(c.name);
					if (slotNumber < Game1.player.Items.Count && Game1.player.Items[slotNumber] != null)
					{
						c.scale = Math.Min(c.scale + 0.05f, 1.1f);
						this.hoverItem = Game1.player.Items[slotNumber];
					}
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.025f, 1f);
				}
			}
		}

		// Token: 0x06002D63 RID: 11619 RVA: 0x00237A48 File Offset: 0x00235C48
		public void shifted(bool right)
		{
			if (right)
			{
				for (int i = 0; i < this.buttons.Count; i++)
				{
					this.buttons[i].scale = 1f + (float)i * 0.03f;
				}
				return;
			}
			for (int j = this.buttons.Count - 1; j >= 0; j--)
			{
				this.buttons[j].scale = 1f + (float)(11 - j) * 0.03f;
			}
		}

		// Token: 0x06002D64 RID: 11620 RVA: 0x00237AC8 File Offset: 0x00235CC8
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			for (int i = 0; i < 12; i++)
			{
				this.buttons[i].bounds = new Rectangle(Game1.uiViewport.Width / 2 - 384 + i * 64, this.yPositionOnScreen - 96 + 8, 64, 64);
			}
		}

		// Token: 0x06002D65 RID: 11621 RVA: 0x00237B20 File Offset: 0x00235D20
		public override bool isWithinBounds(int x, int y)
		{
			ClickableComponent firstButton = this.buttons[0];
			return new Rectangle(firstButton.bounds.X, firstButton.bounds.Y, this.buttons.Last<ClickableComponent>().bounds.X - firstButton.bounds.X + 64, 64).Contains(x, y);
		}

		// Token: 0x06002D66 RID: 11622 RVA: 0x00237B88 File Offset: 0x00235D88
		public override void draw(SpriteBatch b)
		{
			if (Game1.activeClickableMenu != null)
			{
				return;
			}
			Point playerGlobalPos = Game1.player.StandingPixel;
			Vector2 playerGlobalVec = new Vector2((float)playerGlobalPos.X, (float)playerGlobalPos.Y);
			Vector2 playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalVec);
			bool alignTop;
			if (Game1.options.pinToolbarToggle)
			{
				alignTop = false;
				this.transparency = Math.Min(1f, this.transparency + 0.075f);
				if (playerLocalVec.Y > (float)(Game1.viewport.Height - 192))
				{
					this.transparency = Math.Max(0.33f, this.transparency - 0.15f);
				}
			}
			else
			{
				alignTop = (playerLocalVec.Y > (float)(Game1.viewport.Height / 2 + 64));
				this.transparency = 1f;
			}
			int margin = Utility.makeSafeMarginY(8);
			int num = this.yPositionOnScreen;
			if (!alignTop)
			{
				this.yPositionOnScreen = Game1.uiViewport.Height;
				this.yPositionOnScreen += 8;
				this.yPositionOnScreen -= margin;
			}
			else
			{
				this.yPositionOnScreen = 112;
				this.yPositionOnScreen -= 8;
				this.yPositionOnScreen += margin;
			}
			if (num != this.yPositionOnScreen)
			{
				for (int i = 0; i < 12; i++)
				{
					this.buttons[i].bounds.Y = this.yPositionOnScreen - 96 + 8;
				}
			}
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, this.toolbarTextSource, Game1.uiViewport.Width / 2 - 384 - 16, this.yPositionOnScreen - 96 - 8, 800, 96, Color.White * this.transparency, 1f, false, -1f);
			for (int j = 0; j < 12; j++)
			{
				Vector2 toDraw = new Vector2((float)(Game1.uiViewport.Width / 2 - 384 + j * 64), (float)(this.yPositionOnScreen - 96 + 8));
				b.Draw(Game1.menuTexture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, (Game1.player.CurrentToolIndex == j) ? 56 : 10, -1, -1)), Color.White * this.transparency);
				if (!Game1.options.gamepadControls)
				{
					b.DrawString(Game1.tinyFont, this.slotText[j], toDraw + new Vector2(4f, -8f), Color.DimGray * this.transparency);
				}
			}
			for (int k = 0; k < 12; k++)
			{
				this.buttons[k].scale = Math.Max(1f, this.buttons[k].scale - 0.025f);
				Vector2 toDraw2 = new Vector2((float)(Game1.uiViewport.Width / 2 - 384 + k * 64), (float)(this.yPositionOnScreen - 96 + 8));
				if (Game1.player.Items.Count > k && Game1.player.Items[k] != null)
				{
					Game1.player.Items[k].drawInMenu(b, toDraw2, (Game1.player.CurrentToolIndex == k) ? 0.9f : (this.buttons[k].scale * 0.8f), this.transparency, 0.88f);
				}
			}
			if (this.hoverItem != null)
			{
				IClickableMenu.drawToolTip(b, this.hoverItem.getDescription(), this.hoverItem.DisplayName, this.hoverItem, false, -1, 0, null, -1, null, -1, null);
				this.hoverItem = null;
			}
		}

		// Token: 0x04001F27 RID: 7975
		public List<ClickableComponent> buttons = new List<ClickableComponent>();

		// Token: 0x04001F28 RID: 7976
		public new int yPositionOnScreen;

		// Token: 0x04001F29 RID: 7977
		public Item hoverItem;

		// Token: 0x04001F2A RID: 7978
		public float transparency = 1f;

		// Token: 0x04001F2B RID: 7979
		private bool hoverDirty = true;

		// Token: 0x04001F2C RID: 7980
		public string[] slotText = new string[]
		{
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"0",
			"-",
			"="
		};

		// Token: 0x04001F2D RID: 7981
		public Rectangle toolbarTextSource = new Rectangle(0, 256, 60, 60);
	}
}
