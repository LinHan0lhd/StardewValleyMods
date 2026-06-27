using System;
using System.Collections.Generic;
using StardewValley.Network.NetReady.Internal;

namespace StardewValley.Network.NetReady
{
	// Token: 0x020001F3 RID: 499
	public class ReadySynchronizer
	{
		// Token: 0x0600226E RID: 8814 RVA: 0x00176498 File Offset: 0x00174698
		public void SetLocalRequiredFarmers(string id, List<Farmer> requiredFarmers)
		{
			List<long> farmerIds = new List<long>();
			foreach (Farmer player in requiredFarmers)
			{
				farmerIds.Add(player.UniqueMultiplayerID);
			}
			this.GetOrCreate(id).SetRequiredFarmers(farmerIds);
		}

		// Token: 0x0600226F RID: 8815 RVA: 0x00176500 File Offset: 0x00174700
		public void SetLocalReady(string id, bool ready)
		{
			this.GetOrCreate(id).SetLocalReady(ready);
		}

		// Token: 0x06002270 RID: 8816 RVA: 0x00176510 File Offset: 0x00174710
		public bool IsReady(string id)
		{
			BaseReadyCheck ifExists = this.GetIfExists(id);
			return ifExists != null && ifExists.IsReady;
		}

		// Token: 0x06002271 RID: 8817 RVA: 0x00176524 File Offset: 0x00174724
		public bool IsReadyCheckCancelable(string id)
		{
			BaseReadyCheck ifExists = this.GetIfExists(id);
			return ifExists != null && ifExists.IsCancelable;
		}

		// Token: 0x06002272 RID: 8818 RVA: 0x00176538 File Offset: 0x00174738
		public int GetNumberReady(string id)
		{
			BaseReadyCheck ifExists = this.GetIfExists(id);
			if (ifExists == null)
			{
				return 0;
			}
			return ifExists.NumberReady;
		}

		// Token: 0x06002273 RID: 8819 RVA: 0x0017654C File Offset: 0x0017474C
		public int GetNumberRequired(string id)
		{
			BaseReadyCheck ifExists = this.GetIfExists(id);
			if (ifExists == null)
			{
				return 0;
			}
			return ifExists.NumberRequired;
		}

		// Token: 0x06002274 RID: 8820 RVA: 0x00176560 File Offset: 0x00174760
		public void Update()
		{
			foreach (BaseReadyCheck baseReadyCheck in this.ReadyChecks.Values)
			{
				baseReadyCheck.Update();
			}
		}

		// Token: 0x06002275 RID: 8821 RVA: 0x001765B8 File Offset: 0x001747B8
		public void Reset()
		{
			this.ReadyChecks.Clear();
		}

		// Token: 0x06002276 RID: 8822 RVA: 0x001765C8 File Offset: 0x001747C8
		public void ProcessMessage(IncomingMessage message)
		{
			string id = message.Reader.ReadString();
			ReadyCheckMessageType messageType = (ReadyCheckMessageType)message.Reader.ReadByte();
			this.GetOrCreate(id).ProcessMessage(messageType, message);
		}

		// Token: 0x06002277 RID: 8823 RVA: 0x001765FC File Offset: 0x001747FC
		private BaseReadyCheck GetIfExists(string id)
		{
			BaseReadyCheck check;
			if (id == null || !this.ReadyChecks.TryGetValue(id, out check))
			{
				return null;
			}
			return check;
		}

		// Token: 0x06002278 RID: 8824 RVA: 0x00176620 File Offset: 0x00174820
		private BaseReadyCheck GetOrCreate(string id)
		{
			BaseReadyCheck check;
			if (this.ReadyChecks.TryGetValue(id, out check))
			{
				return check;
			}
			check = (Game1.IsMasterGame ? new ServerReadyCheck(id) : new ClientReadyCheck(id));
			this.ReadyChecks.Add(id, check);
			return check;
		}

		// Token: 0x04001465 RID: 5221
		private readonly Dictionary<string, BaseReadyCheck> ReadyChecks = new Dictionary<string, BaseReadyCheck>();
	}
}
