using System;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000AB RID: 171
	public class Proposal : INetObject<NetFields>
	{
		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060009EB RID: 2539 RVA: 0x0006C228 File Offset: 0x0006A428
		public NetFields NetFields { get; } = new NetFields("Proposal");

		// Token: 0x060009EC RID: 2540 RVA: 0x0006C230 File Offset: 0x0006A430
		public Proposal()
		{
			this.NetFields.SetOwner(this).AddField(this.sender.NetFields, "sender.NetFields").AddField(this.receiver.NetFields, "receiver.NetFields").AddField(this.proposalType, "proposalType").AddField(this.response, "response").AddField(this.responseMessageKey, "responseMessageKey").AddField(this.gift, "gift").AddField(this.canceled, "canceled").AddField(this.cancelConfirmed, "cancelConfirmed");
		}

		// Token: 0x0400062D RID: 1581
		public readonly NetFarmerRef sender = new NetFarmerRef();

		// Token: 0x0400062E RID: 1582
		public readonly NetFarmerRef receiver = new NetFarmerRef();

		// Token: 0x0400062F RID: 1583
		public readonly NetEnum<ProposalType> proposalType = new NetEnum<ProposalType>(ProposalType.Gift);

		// Token: 0x04000630 RID: 1584
		public readonly NetEnum<ProposalResponse> response = new NetEnum<ProposalResponse>(ProposalResponse.None);

		// Token: 0x04000631 RID: 1585
		public readonly NetString responseMessageKey = new NetString();

		// Token: 0x04000632 RID: 1586
		public readonly NetRef<Item> gift = new NetRef<Item>();

		// Token: 0x04000633 RID: 1587
		public readonly NetBool canceled = new NetBool();

		// Token: 0x04000634 RID: 1588
		public readonly NetBool cancelConfirmed = new NetBool();
	}
}
