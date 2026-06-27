using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewValley.Logging;

namespace StardewValley.Network.NetReady.Internal
{
	// Token: 0x020001F5 RID: 501
	internal sealed class ClientReadyCheck : BaseReadyCheck
	{
		// Token: 0x0600228D RID: 8845 RVA: 0x00176783 File Offset: 0x00174983
		public ClientReadyCheck(string id) : base(id)
		{
		}

		// Token: 0x0600228E RID: 8846 RVA: 0x0017678C File Offset: 0x0017498C
		public override void SetRequiredFarmers(List<long> farmerIds)
		{
			if (farmerIds == null)
			{
				int required = 0;
				foreach (Farmer farmer in Game1.getOnlineFarmers())
				{
					if (!Game1.multiplayer.isDisconnecting(farmer) && !farmer.IsDedicatedPlayer)
					{
						required++;
					}
				}
				base.NumberRequired = required;
				this.SendMessage(ReadyCheckMessageType.RequireFarmers, new object[]
				{
					-1
				});
				return;
			}
			base.NumberRequired = farmerIds.Count;
			object[] data = new object[farmerIds.Count + 1];
			data[0] = farmerIds.Count;
			for (int i = 0; i < farmerIds.Count; i++)
			{
				data[i + 1] = farmerIds[i];
			}
			this.SendMessage(ReadyCheckMessageType.RequireFarmers, data);
		}

		// Token: 0x0600228F RID: 8847 RVA: 0x0017686C File Offset: 0x00174A6C
		public override bool SetLocalReady(bool ready)
		{
			if (!base.SetLocalReady(ready))
			{
				return false;
			}
			int numberReady = base.NumberReady;
			base.NumberReady = numberReady + 1;
			this.SendMessage(ready ? ReadyCheckMessageType.Ready : ReadyCheckMessageType.Cancel, Array.Empty<object>());
			return true;
		}

		// Token: 0x06002290 RID: 8848 RVA: 0x001768A7 File Offset: 0x00174AA7
		public override void Update()
		{
		}

		// Token: 0x06002291 RID: 8849 RVA: 0x001768AC File Offset: 0x00174AAC
		public override void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message)
		{
			switch (messageType)
			{
			case ReadyCheckMessageType.Lock:
				this.ProcessLock(message);
				return;
			case ReadyCheckMessageType.Release:
				this.ProcessRelease(message);
				return;
			case ReadyCheckMessageType.UpdateAmounts:
				this.ProcessUpdateAmounts(message);
				return;
			case ReadyCheckMessageType.Finish:
				this.ProcessFinish(message);
				return;
			}
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 3);
			defaultInterpolatedStringHandler.AppendFormatted("ClientReadyCheck");
			defaultInterpolatedStringHandler.AppendLiteral(" '");
			defaultInterpolatedStringHandler.AppendFormatted(base.Id);
			defaultInterpolatedStringHandler.AppendLiteral("' received invalid message type '");
			defaultInterpolatedStringHandler.AppendFormatted<ReadyCheckMessageType>(messageType);
			defaultInterpolatedStringHandler.AppendLiteral("'.");
			log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06002292 RID: 8850 RVA: 0x0017695F File Offset: 0x00174B5F
		protected override void SendMessage(ReadyCheckMessageType messageType, params object[] data)
		{
			Client client = Game1.client;
			if (client == null)
			{
				return;
			}
			client.sendMessage(base.CreateSyncMessage(messageType, data));
		}

		// Token: 0x06002293 RID: 8851 RVA: 0x00176978 File Offset: 0x00174B78
		private void ProcessLock(IncomingMessage message)
		{
			base.ActiveLockId = message.Reader.ReadInt32();
			if (base.State == ReadyState.NotReady)
			{
				this.SendMessage(ReadyCheckMessageType.RejectLock, new object[]
				{
					base.ActiveLockId
				});
				return;
			}
			base.State = ReadyState.Locked;
			this.SendMessage(ReadyCheckMessageType.AcceptLock, new object[]
			{
				base.ActiveLockId
			});
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x001769DC File Offset: 0x00174BDC
		private void ProcessRelease(IncomingMessage message)
		{
			int lockId = message.Reader.ReadInt32();
			if (base.State == ReadyState.Locked && lockId == base.ActiveLockId)
			{
				base.State = ReadyState.Ready;
			}
		}

		// Token: 0x06002295 RID: 8853 RVA: 0x00176A0E File Offset: 0x00174C0E
		private void ProcessUpdateAmounts(IncomingMessage message)
		{
			base.NumberReady = message.Reader.ReadInt32();
			base.NumberRequired = message.Reader.ReadInt32();
		}

		// Token: 0x06002296 RID: 8854 RVA: 0x00176A32 File Offset: 0x00174C32
		private void ProcessFinish(IncomingMessage message)
		{
			base.IsReady = true;
		}
	}
}
