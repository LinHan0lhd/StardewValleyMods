using System;
using System.IO;
using System.Runtime.CompilerServices;
using StardewValley.Extensions;
using StardewValley.Logging;

namespace StardewValley.Network.NetEvents
{
	// Token: 0x020001FE RID: 510
	public sealed class SetMailRequest : BaseSetFlagRequest
	{
		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x060022C6 RID: 8902 RVA: 0x0017742B File Offset: 0x0017562B
		// (set) Token: 0x060022C7 RID: 8903 RVA: 0x00177433 File Offset: 0x00175633
		public MailType MailType { get; private set; } = MailType.Tomorrow;

		// Token: 0x060022C8 RID: 8904 RVA: 0x0017743C File Offset: 0x0017563C
		public SetMailRequest()
		{
		}

		// Token: 0x060022C9 RID: 8905 RVA: 0x0017744B File Offset: 0x0017564B
		public SetMailRequest(PlayerActionTarget target, string mailId, MailType mailType, bool state, long? onlyPlayerId = null) : base(target, mailId, state, onlyPlayerId)
		{
			this.MailType = mailType;
		}

		// Token: 0x060022CA RID: 8906 RVA: 0x00177468 File Offset: 0x00175668
		public override void PerformAction(Farmer farmer)
		{
			switch (this.MailType)
			{
			case MailType.Now:
				this.ToggleMailbox(farmer, base.FlagId, base.FlagState);
				return;
			case MailType.Tomorrow:
				farmer.mailForTomorrow.Toggle(base.FlagId, base.FlagState);
				return;
			case MailType.Received:
				farmer.mailReceived.Toggle(base.FlagId, base.FlagState);
				return;
			case MailType.All:
				this.ToggleMailbox(farmer, base.FlagId, base.FlagState);
				farmer.mailForTomorrow.Toggle(base.FlagId, base.FlagState);
				farmer.mailReceived.Toggle(base.FlagId, base.FlagState);
				return;
			default:
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Received request to add mail ID '");
				defaultInterpolatedStringHandler.AppendFormatted(base.FlagId);
				defaultInterpolatedStringHandler.AppendLiteral("' with unknown mail type '");
				defaultInterpolatedStringHandler.AppendFormatted<MailType>(this.MailType);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			}
		}

		// Token: 0x060022CB RID: 8907 RVA: 0x00177576 File Offset: 0x00175776
		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			this.MailType = (MailType)Enum.ToObject(typeof(MailType), reader.ReadByte());
		}

		// Token: 0x060022CC RID: 8908 RVA: 0x0017759F File Offset: 0x0017579F
		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write((byte)this.MailType);
		}

		// Token: 0x060022CD RID: 8909 RVA: 0x001775B4 File Offset: 0x001757B4
		private void ToggleMailbox(Farmer farmer, string mailId, bool add)
		{
			if (add)
			{
				farmer.mailbox.Add(mailId);
				return;
			}
			farmer.mailbox.RemoveWhere((string p) => p == mailId);
		}
	}
}
