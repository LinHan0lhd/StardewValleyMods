using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200026C RID: 620
	public class EmoteSelector : IClickableMenu
	{
		// Token: 0x06002916 RID: 10518 RVA: 0x001E2CBC File Offset: 0x001E0EBC
		public EmoteSelector(int emote_index, string selected_emote = "") : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64, false)
		{
			this.emoteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
			Game1.playSound("shwip", null);
			this.emoteIndex = emote_index;
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			this.emoteButtons = new List<ClickableTextureComponent>();
			this.currentlySnappedComponent = null;
			for (int i = 0; i < Farmer.EMOTES.Length; i++)
			{
				Farmer.EmoteType emote_type = Farmer.EMOTES[i];
				if (!emote_type.hidden || Game1.player.performedEmotes.ContainsKey(emote_type.emoteString))
				{
					ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 80, 68), this.emoteTexture, EmoteMenu.GetEmoteNonBubbleSpriteRect(i), 4f, true)
					{
						leftNeighborID = -99998,
						rightNeighborID = -99998,
						upNeighborID = -99998,
						downNeighborID = -99998,
						myID = i
					};
					component.label = emote_type.displayName;
					component.name = emote_type.emoteString;
					component.drawLabelWithShadow = true;
					component.hoverText = ((emote_type.animationFrames != null) ? "animated" : "");
					this.emoteButtons.Add(component);
					if (this.currentlySnappedComponent == null)
					{
						this.currentlySnappedComponent = component;
					}
					if (selected_emote != "" && selected_emote == component.name)
					{
						this.currentlySnappedComponent = component;
						this._selectedEmote = component;
					}
				}
			}
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998,
				myID = 1000,
				drawShadow = true
			};
			this.RepositionElements();
			this.populateClickableComponentList();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002917 RID: 10519 RVA: 0x001E2F70 File Offset: 0x001E1170
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			this.RepositionElements();
		}

		// Token: 0x06002918 RID: 10520 RVA: 0x001E2FD4 File Offset: 0x001E11D4
		public override void performHoverAction(int x, int y)
		{
			ClickableTextureComponent oldHovered = this._hoveredEmote;
			this._hoveredEmote = null;
			this.okButton.tryHover(x, y, 0.1f);
			foreach (ClickableTextureComponent component in this.emoteButtons)
			{
				int component_width = component.bounds.Width;
				component.bounds.Width = this.scrollView.Width / 3;
				component.tryHover(x, y, 0.1f);
				if (component != this._selectedEmote && component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
				{
					this._hoveredEmote = component;
				}
				component.bounds.Width = component_width;
			}
			if (this._hoveredEmote != null && this._hoveredEmote != oldHovered)
			{
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002919 RID: 10521 RVA: 0x001E30D4 File Offset: 0x001E12D4
		private void RepositionElements()
		{
			this.scrollView = new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 4, this.width - 128, this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder - 64 + 8);
			this.RepositionScrollElements();
		}

		// Token: 0x0600291A RID: 10522 RVA: 0x001E3134 File Offset: 0x001E1334
		public void RepositionScrollElements()
		{
			int y_offset = (int)this.scrollY + 4;
			if (this.scrollY > 0f)
			{
				this.scrollY = 0f;
			}
			int x_offset = 8;
			foreach (ClickableTextureComponent component in this.emoteButtons)
			{
				component.bounds.X = this.scrollView.X + x_offset;
				component.bounds.Y = this.scrollView.Y + y_offset;
				if (component.bounds.Bottom > this.scrollView.Bottom)
				{
					y_offset = 4;
					x_offset += this.scrollView.Width / 3;
					component.bounds.X = this.scrollView.X + x_offset;
					component.bounds.Y = this.scrollView.Y + y_offset;
				}
				y_offset += component.bounds.Height;
				if (this.scrollView.Intersects(component.bounds))
				{
					component.visible = true;
				}
				else
				{
					component.visible = false;
				}
			}
		}

		// Token: 0x0600291B RID: 10523 RVA: 0x001E3268 File Offset: 0x001E1468
		public override void snapToDefaultClickableComponent()
		{
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x0600291C RID: 10524 RVA: 0x001E3270 File Offset: 0x001E1470
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in this.emoteButtons)
			{
				int component_width = component.bounds.Width;
				component.bounds.Width = this.scrollView.Width / 3;
				if (component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
				{
					component.bounds.Width = component_width;
					if (this.emoteIndex < Game1.player.GetEmoteFavorites().Count)
					{
						Game1.player.GetEmoteFavorites()[this.emoteIndex] = component.name;
					}
					base.exitThisMenu(false);
					Game1.playSound("drumkit6", null);
					if (!Game1.options.gamepadControls)
					{
						Game1.emoteMenu = new EmoteMenu();
					}
					return;
				}
				component.bounds.Width = component_width;
			}
			if (this.okButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x0600291D RID: 10525 RVA: 0x001E339C File Offset: 0x001E159C
		public bool canLeaveMenu()
		{
			return true;
		}

		// Token: 0x0600291E RID: 10526 RVA: 0x001E33A0 File Offset: 0x001E15A0
		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.xPositionOnScreen - 128 - 8, this.yPositionOnScreen + 128 - 8, 192, 164, Color.White, 1f, false, -1f);
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			foreach (ClickableTextureComponent component in this.emoteButtons)
			{
				if (component == this.currentlySnappedComponent && Game1.options.gamepadControls && component != this._selectedEmote && component == this._hoveredEmote)
				{
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(64, 320, 60, 60), component.bounds.X + 64 + 8, component.bounds.Y + 8, this.scrollView.Width / 3 - 64 - 16, component.bounds.Height - 16, Color.White, 1f, false, -1f);
					Utility.drawWithShadow(b, this.emoteTexture, component.getVector2() - new Vector2(4f, 4f), new Rectangle(83, 0, 18, 18), Color.White, 0f, Vector2.Zero, 4f, false, -1f, -1, -1, 0.35f);
				}
				component.draw(b, Color.White * ((component == this._selectedEmote) ? 0.4f : 1f), 0.87f, 0, 0, 0);
				if (component != this._selectedEmote && component.hoverText != "" && Game1.currentGameTime.TotalGameTime.Milliseconds % 500 < 250)
				{
					b.Draw(component.texture, component.getVector2(), new Rectangle?(new Rectangle(component.sourceRect.X + 80, component.sourceRect.Y, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
			}
			if (this._selectedEmote != null)
			{
				for (int i = 0; i < 8; i++)
				{
					float radians = Utility.Lerp(0f, 6.2831855f, (float)i / 8f);
					Vector2 pos = Vector2.Zero;
					pos.X = (float)((int)((float)(this.xPositionOnScreen - 64 + (int)(Math.Cos((double)radians) * 12.0) * 4) - 3.5f));
					pos.Y = (float)((int)((float)(this.yPositionOnScreen + 192 + (int)(-Math.Sin((double)radians) * 12.0) * 4) - 3.5f));
					Utility.drawWithShadow(b, this.emoteTexture, pos, new Rectangle(64 + ((i == this.emoteIndex) ? 8 : 0), 48, 8, 8), Color.White, 0f, Vector2.Zero, -1f, false, -1f, -1, -1, 0.35f);
				}
			}
			this.okButton.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x0600291F RID: 10527 RVA: 0x001E371C File Offset: 0x001E191C
		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			Game1.player.noMovementPause = Math.Max(Game1.player.noMovementPause, 200);
		}

		// Token: 0x04001ADD RID: 6877
		public Rectangle scrollView;

		// Token: 0x04001ADE RID: 6878
		public List<ClickableTextureComponent> emoteButtons;

		// Token: 0x04001ADF RID: 6879
		public ClickableTextureComponent okButton;

		// Token: 0x04001AE0 RID: 6880
		public float scrollY;

		// Token: 0x04001AE1 RID: 6881
		public int emoteIndex;

		// Token: 0x04001AE2 RID: 6882
		protected ClickableTextureComponent _selectedEmote;

		// Token: 0x04001AE3 RID: 6883
		protected ClickableTextureComponent _hoveredEmote;

		// Token: 0x04001AE4 RID: 6884
		protected Texture2D emoteTexture;
	}
}
