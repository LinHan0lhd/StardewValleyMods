using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	// Token: 0x02000298 RID: 664
	public class PendingProposalDialog : ConfirmationDialog
	{
		// Token: 0x06002B7D RID: 11133 RVA: 0x0020F0CE File Offset: 0x0020D2CE
		public PendingProposalDialog() : base(Game1.content.LoadString("Strings\\UI:PendingProposal"), null, null)
		{
			this.okButton.visible = false;
			this.onCancel = new ConfirmationDialog.behavior(this.cancelProposal);
			this.setCancelable(true);
		}

		// Token: 0x06002B7E RID: 11134 RVA: 0x0020F10C File Offset: 0x0020D30C
		public void cancelProposal(Farmer who)
		{
			Proposal proposal = Game1.player.team.GetOutgoingProposal();
			if (((proposal != null) ? proposal.receiver.Value : null) == null || !proposal.receiver.Value.isActive())
			{
				return;
			}
			proposal.canceled.Value = true;
			this.message = Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling");
			this.setCancelable(false);
		}

		// Token: 0x06002B7F RID: 11135 RVA: 0x0020F177 File Offset: 0x0020D377
		public void setCancelable(bool cancelable)
		{
			this.cancelButton.visible = cancelable;
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002B80 RID: 11136 RVA: 0x0020F19D File Offset: 0x0020D39D
		public override bool readyToClose()
		{
			return false;
		}

		// Token: 0x06002B81 RID: 11137 RVA: 0x0020F1A0 File Offset: 0x0020D3A0
		private bool consumesItem(ProposalType pt)
		{
			return pt == ProposalType.Gift || pt == ProposalType.Marriage;
		}

		// Token: 0x06002B82 RID: 11138 RVA: 0x0020F1AC File Offset: 0x0020D3AC
		public override void update(GameTime time)
		{
			base.update(time);
			Proposal proposal = Game1.player.team.GetOutgoingProposal();
			if (((proposal != null) ? proposal.receiver.Value : null) == null || !proposal.receiver.Value.isActive())
			{
				Game1.player.team.RemoveOutgoingProposal();
				this.closeDialog(Game1.player);
				return;
			}
			if (proposal.cancelConfirmed.Value && proposal.response.Value != ProposalResponse.Accepted)
			{
				Game1.player.team.RemoveOutgoingProposal();
				this.closeDialog(Game1.player);
				return;
			}
			if (proposal.response.Value != ProposalResponse.None)
			{
				if (proposal.response.Value == ProposalResponse.Accepted)
				{
					if (this.consumesItem(proposal.proposalType.Value))
					{
						Game1.player.reduceActiveItemByOne();
					}
					if (proposal.proposalType.Value == ProposalType.Dance)
					{
						Game1.player.dancePartner.Value = proposal.receiver.Value;
					}
					proposal.receiver.Value.doEmote(20);
				}
				Game1.player.team.RemoveOutgoingProposal();
				this.closeDialog(Game1.player);
				if (proposal.responseMessageKey.Value != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString(proposal.responseMessageKey.Value, proposal.receiver.Value.Name));
				}
			}
		}
	}
}
