using System;
using System.IO;

namespace StardewValley.Network.NetEvents
{
	// Token: 0x020001FA RID: 506
	public abstract class BaseSetFlagRequest : BasePlayerActionRequest
	{
		// Token: 0x170003CE RID: 974
		// (get) Token: 0x060022B0 RID: 8880 RVA: 0x00177237 File Offset: 0x00175437
		// (set) Token: 0x060022B1 RID: 8881 RVA: 0x0017723F File Offset: 0x0017543F
		public string FlagId { get; private set; }

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x060022B2 RID: 8882 RVA: 0x00177248 File Offset: 0x00175448
		// (set) Token: 0x060022B3 RID: 8883 RVA: 0x00177250 File Offset: 0x00175450
		public bool FlagState { get; private set; }

		// Token: 0x060022B4 RID: 8884 RVA: 0x00177259 File Offset: 0x00175459
		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			this.FlagId = reader.ReadString();
			this.FlagState = reader.ReadBoolean();
		}

		// Token: 0x060022B5 RID: 8885 RVA: 0x0017727A File Offset: 0x0017547A
		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(this.FlagId);
			writer.Write(this.FlagState);
		}

		// Token: 0x060022B6 RID: 8886 RVA: 0x0017729B File Offset: 0x0017549B
		protected BaseSetFlagRequest()
		{
		}

		// Token: 0x060022B7 RID: 8887 RVA: 0x001772A3 File Offset: 0x001754A3
		protected BaseSetFlagRequest(PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId) : base(target, onlyPlayerId)
		{
			this.FlagId = flagId;
			this.FlagState = flagState;
		}
	}
}
