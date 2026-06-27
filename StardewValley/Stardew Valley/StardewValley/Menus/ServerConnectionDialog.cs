using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	// Token: 0x020002A5 RID: 677
	public class ServerConnectionDialog : ConfirmationDialog
	{
		// Token: 0x06002C2A RID: 11306 RVA: 0x0021B11B File Offset: 0x0021931B
		public ServerConnectionDialog(ConfirmationDialog.behavior onConfirm = null, ConfirmationDialog.behavior onCancel = null) : base(Game1.content.LoadString("Strings\\UI:CoopMenu_Connecting"), onConfirm, onCancel)
		{
			this.okButton.visible = false;
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002C2B RID: 11307 RVA: 0x0021B158 File Offset: 0x00219358
		public override void update(GameTime time)
		{
			base.update(time);
			if (Game1.server != null && Game1.server.connected())
			{
				base.confirm();
			}
		}
	}
}
