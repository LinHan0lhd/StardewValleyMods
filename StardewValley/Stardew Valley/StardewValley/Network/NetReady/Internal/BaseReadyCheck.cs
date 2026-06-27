using System;
using System.Collections.Generic;

namespace StardewValley.Network.NetReady.Internal
{
	// Token: 0x020001F4 RID: 500
	internal abstract class BaseReadyCheck
	{
		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x0600227A RID: 8826 RVA: 0x00176676 File Offset: 0x00174876
		public string Id { get; }

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x0600227B RID: 8827 RVA: 0x0017667E File Offset: 0x0017487E
		// (set) Token: 0x0600227C RID: 8828 RVA: 0x00176686 File Offset: 0x00174886
		public int ActiveLockId { get; protected set; }

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x0600227D RID: 8829 RVA: 0x0017668F File Offset: 0x0017488F
		// (set) Token: 0x0600227E RID: 8830 RVA: 0x00176697 File Offset: 0x00174897
		public ReadyState State { get; protected set; }

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x0600227F RID: 8831 RVA: 0x001766A0 File Offset: 0x001748A0
		// (set) Token: 0x06002280 RID: 8832 RVA: 0x001766A8 File Offset: 0x001748A8
		public int NumberReady { get; protected set; }

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x06002281 RID: 8833 RVA: 0x001766B1 File Offset: 0x001748B1
		// (set) Token: 0x06002282 RID: 8834 RVA: 0x001766B9 File Offset: 0x001748B9
		public int NumberRequired { get; protected set; }

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x06002283 RID: 8835 RVA: 0x001766C2 File Offset: 0x001748C2
		// (set) Token: 0x06002284 RID: 8836 RVA: 0x001766CA File Offset: 0x001748CA
		public bool IsReady { get; protected set; }

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x06002285 RID: 8837 RVA: 0x001766D3 File Offset: 0x001748D3
		public bool IsCancelable
		{
			get
			{
				return this.State != ReadyState.Locked;
			}
		}

		// Token: 0x06002286 RID: 8838 RVA: 0x001766E1 File Offset: 0x001748E1
		protected BaseReadyCheck(string id)
		{
			this.Id = id;
			this.State = ReadyState.NotReady;
			this.NumberReady = 0;
			this.NumberRequired = Game1.getOnlineFarmers().Count;
			this.IsReady = false;
		}

		// Token: 0x06002287 RID: 8839
		public abstract void SetRequiredFarmers(List<long> farmerIds);

		// Token: 0x06002288 RID: 8840 RVA: 0x00176715 File Offset: 0x00174915
		public virtual bool SetLocalReady(bool ready)
		{
			if (!this.IsCancelable)
			{
				return false;
			}
			ReadyState state = this.State;
			this.State = (ready ? ReadyState.Ready : ReadyState.NotReady);
			return state != this.State;
		}

		// Token: 0x06002289 RID: 8841
		public abstract void Update();

		// Token: 0x0600228A RID: 8842
		public abstract void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message);

		// Token: 0x0600228B RID: 8843
		protected abstract void SendMessage(ReadyCheckMessageType messageType, params object[] data);

		// Token: 0x0600228C RID: 8844 RVA: 0x00176740 File Offset: 0x00174940
		protected OutgoingMessage CreateSyncMessage(ReadyCheckMessageType messageType, params object[] data)
		{
			object[] messageData = new object[data.Length + 2];
			messageData[0] = this.Id;
			messageData[1] = (byte)messageType;
			Array.Copy(data, 0, messageData, 2, data.Length);
			return new OutgoingMessage(31, Game1.player, messageData);
		}
	}
}
