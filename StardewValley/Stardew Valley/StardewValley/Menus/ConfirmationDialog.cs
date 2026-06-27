using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x02000262 RID: 610
	public class ConfirmationDialog : IClickableMenu
	{
		// Token: 0x06002869 RID: 10345 RVA: 0x001D7AA0 File Offset: 0x001D5CA0
		public ConfirmationDialog(string message, ConfirmationDialog.behavior onConfirm, ConfirmationDialog.behavior onCancel = null) : base(Game1.uiViewport.Width / 2 - (int)Game1.dialogueFont.MeasureString(message).X / 2 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - (int)Game1.dialogueFont.MeasureString(message).Y / 2, (int)Game1.dialogueFont.MeasureString(message).X + IClickableMenu.borderWidth * 2, (int)Game1.dialogueFont.MeasureString(message).Y + IClickableMenu.borderWidth * 2 + 160, false)
		{
			if (onCancel == null)
			{
				onCancel = new ConfirmationDialog.behavior(this.closeDialog);
			}
			else
			{
				this.onCancel = onCancel;
			}
			this.onConfirm = onConfirm;
			Rectangle titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
			message = Game1.parseText(message, Game1.dialogueFont, Math.Min(titleSafeArea.Width - 64, this.width));
			this.message = message;
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 4, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 101,
				rightNeighborID = 102
			};
			this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
				this.delayBeforeCancellable = 300;
			}
		}

		// Token: 0x0600286A RID: 10346 RVA: 0x001D7CB8 File Offset: 0x001D5EB8
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.okButton.setPosition(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 4, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21);
			this.cancelButton.setPosition(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21);
		}

		// Token: 0x0600286B RID: 10347 RVA: 0x001D7D58 File Offset: 0x001D5F58
		public virtual void closeDialog(Farmer who)
		{
			TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
			if (titleMenu != null)
			{
				titleMenu.backButtonPressed();
				return;
			}
			Game1.exitActiveMenu();
		}

		// Token: 0x0600286C RID: 10348 RVA: 0x001D7D7F File Offset: 0x001D5F7F
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(102);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x0600286D RID: 10349 RVA: 0x001D7D98 File Offset: 0x001D5F98
		public void confirm()
		{
			if (!this.active)
			{
				return;
			}
			this.active = false;
			ConfirmationDialog.behavior behavior = this.onConfirm;
			if (behavior != null)
			{
				behavior(Game1.player);
			}
			Game1.playSound("smallSelect", null);
		}

		// Token: 0x0600286E RID: 10350 RVA: 0x001D7DE0 File Offset: 0x001D5FE0
		public void cancel()
		{
			if (this.onCancel != null)
			{
				this.onCancel(Game1.player);
			}
			else
			{
				this.closeDialog(Game1.player);
			}
			Game1.playSound("bigDeSelect", null);
		}

		// Token: 0x0600286F RID: 10351 RVA: 0x001D7E26 File Offset: 0x001D6026
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.active)
			{
				if (this.okButton.containsPoint(x, y))
				{
					this.confirm();
				}
				if (this.cancelButton.containsPoint(x, y) && this.delayBeforeCancellable <= 0)
				{
					this.cancel();
				}
			}
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x001D7E64 File Offset: 0x001D6064
		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (this.active && Game1.activeClickableMenu == null && this.onCancel != null)
			{
				this.onCancel(Game1.player);
			}
			if (this.active)
			{
				if (key != Keys.N)
				{
					if (key == Keys.Y)
					{
						this.confirm();
						return;
					}
				}
				else
				{
					this.cancel();
				}
			}
		}

		// Token: 0x06002871 RID: 10353 RVA: 0x001D7EC0 File Offset: 0x001D60C0
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.delayBeforeCancellable > 0)
			{
				this.delayBeforeCancellable -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x06002872 RID: 10354 RVA: 0x001D7EFC File Offset: 0x001D60FC
		public override void performHoverAction(int x, int y)
		{
			if (this.okButton.containsPoint(x, y))
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.2f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}
			if (this.cancelButton.containsPoint(x, y))
			{
				this.cancelButton.scale = ((this.cancelButton.baseScale == 1f) ? Math.Min(this.cancelButton.scale + 0.02f, this.cancelButton.baseScale + 0.2f) : Math.Min(this.cancelButton.scale + 0.1f, this.cancelButton.baseScale + 0.75f));
				return;
			}
			this.cancelButton.scale = ((this.cancelButton.baseScale == 1f) ? Math.Max(this.cancelButton.scale - 0.02f, this.cancelButton.baseScale) : Math.Max(this.cancelButton.scale - 0.1f, this.cancelButton.baseScale));
		}

		// Token: 0x06002873 RID: 10355 RVA: 0x001D8058 File Offset: 0x001D6258
		public override void draw(SpriteBatch b)
		{
			if (this.active)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
				b.DrawString(Game1.dialogueFont, this.message, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2)), Game1.textColor);
				this.okButton.draw(b);
				this.cancelButton.draw(b);
				base.drawMouse(b, false, -1);
			}
		}

		// Token: 0x04001A2C RID: 6700
		public const int region_okButton = 101;

		// Token: 0x04001A2D RID: 6701
		public const int region_cancelButton = 102;

		// Token: 0x04001A2E RID: 6702
		protected string message;

		// Token: 0x04001A2F RID: 6703
		public ClickableTextureComponent okButton;

		// Token: 0x04001A30 RID: 6704
		public ClickableTextureComponent cancelButton;

		// Token: 0x04001A31 RID: 6705
		protected ConfirmationDialog.behavior onConfirm;

		// Token: 0x04001A32 RID: 6706
		protected ConfirmationDialog.behavior onCancel;

		// Token: 0x04001A33 RID: 6707
		private bool active = true;

		// Token: 0x04001A34 RID: 6708
		private int delayBeforeCancellable;

		// Token: 0x020005F8 RID: 1528
		// (Invoke) Token: 0x060043CC RID: 17356
		public delegate void behavior(Farmer who);
	}
}
