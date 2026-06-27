using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	// Token: 0x02000279 RID: 633
	public class InviteCodeDialog : ConfirmationDialog
	{
		// Token: 0x060029FD RID: 10749 RVA: 0x001F3C48 File Offset: 0x001F1E48
		public InviteCodeDialog(string code, ConfirmationDialog.behavior onClose) : base(Game1.content.LoadString("Strings\\UI:Server_InviteCode", code), onClose, onClose)
		{
			this.code = code;
			this.onCancel = new ConfirmationDialog.behavior(this.copyCode);
			this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.currentlySnappedComponent = base.getComponentWithID(101);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x060029FE RID: 10750 RVA: 0x001F3D2D File Offset: 0x001F1F2D
		protected void copyCode(Farmer who)
		{
			if (DesktopClipboard.SetText(this.code))
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Server_InviteCode_Copied")));
				return;
			}
			Game1.showRedMessageUsingLoadString("Strings\\UI:Server_InviteCode_CopyFailed", true);
		}

		// Token: 0x04001B8C RID: 7052
		private string code;
	}
}
