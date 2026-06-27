using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x020002A2 RID: 674
	public class ReadyCheckDialog : ConfirmationDialog
	{
		// Token: 0x06002C09 RID: 11273 RVA: 0x002199C8 File Offset: 0x00217BC8
		public ReadyCheckDialog(string checkName, bool allowCancel, ConfirmationDialog.behavior onConfirm = null, ConfirmationDialog.behavior onCancel = null) : base(Game1.content.LoadString("Strings\\UI:ReadyCheck", "N", "M"), onConfirm, onCancel)
		{
			this.checkName = checkName;
			this.allowCancel = allowCancel;
			this.okButton.visible = false;
			this.cancelButton.visible = this.isCancelable();
			this.updateMessage();
			this.exitFunction = delegate()
			{
				this.closeDialog(Game1.player);
			};
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002C0A RID: 11274 RVA: 0x00219A52 File Offset: 0x00217C52
		public bool isCancelable()
		{
			return this.allowCancel && Game1.netReady.IsReadyCheckCancelable(this.checkName);
		}

		// Token: 0x06002C0B RID: 11275 RVA: 0x00219A6E File Offset: 0x00217C6E
		public override bool readyToClose()
		{
			return this.isCancelable();
		}

		// Token: 0x06002C0C RID: 11276 RVA: 0x00219A76 File Offset: 0x00217C76
		public override void closeDialog(Farmer who)
		{
			base.closeDialog(who);
			Game1.displayFarmer = true;
			if (this.isCancelable())
			{
				Game1.netReady.SetLocalReady(this.checkName, false);
			}
		}

		// Token: 0x06002C0D RID: 11277 RVA: 0x00219A9E File Offset: 0x00217C9E
		public override void receiveKeyPress(Keys key)
		{
		}

		// Token: 0x06002C0E RID: 11278 RVA: 0x00219AA0 File Offset: 0x00217CA0
		private void updateMessage()
		{
			int readyNum = Game1.netReady.GetNumberReady(this.checkName);
			int requiredNum = Game1.netReady.GetNumberRequired(this.checkName);
			this.message = Game1.content.LoadString("Strings\\UI:ReadyCheck", readyNum, requiredNum);
		}

		// Token: 0x06002C0F RID: 11279 RVA: 0x00219AF0 File Offset: 0x00217CF0
		public override void update(GameTime time)
		{
			base.update(time);
			this.cancelButton.visible = this.isCancelable();
			this.updateMessage();
			Game1.netReady.SetLocalReady(this.checkName, true);
			if (Game1.netReady.IsReady(this.checkName))
			{
				base.confirm();
			}
		}

		// Token: 0x04001DBE RID: 7614
		public string checkName;

		// Token: 0x04001DBF RID: 7615
		private bool allowCancel;
	}
}
