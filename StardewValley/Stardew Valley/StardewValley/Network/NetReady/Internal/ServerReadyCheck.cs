using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewValley.Logging;

namespace StardewValley.Network.NetReady.Internal
{
	// Token: 0x020001F8 RID: 504
	internal sealed class ServerReadyCheck : BaseReadyCheck
	{
		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06002297 RID: 8855 RVA: 0x00176A3B File Offset: 0x00174C3B
		private bool IncludesAll
		{
			get
			{
				return this.RequiredFarmers.Count == 0;
			}
		}

		// Token: 0x06002298 RID: 8856 RVA: 0x00176A4B File Offset: 0x00174C4B
		public ServerReadyCheck(string id) : base(id)
		{
		}

		// Token: 0x06002299 RID: 8857 RVA: 0x00176A6A File Offset: 0x00174C6A
		public override void SetRequiredFarmers(List<long> farmerIds)
		{
			this.RequireFarmers(farmerIds);
		}

		// Token: 0x0600229A RID: 8858 RVA: 0x00176A74 File Offset: 0x00174C74
		public override bool SetLocalReady(bool ready)
		{
			if (!base.SetLocalReady(ready))
			{
				return false;
			}
			if (!this.IsFarmerRequired(Game1.player.UniqueMultiplayerID))
			{
				base.State = ReadyState.NotReady;
				return false;
			}
			this.ReadyStates[Game1.player.UniqueMultiplayerID] = base.State;
			return true;
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x00176AC4 File Offset: 0x00174CC4
		public override void Update()
		{
			if (base.IsReady)
			{
				return;
			}
			int ready = 0;
			int required = 0;
			int locked = 0;
			bool includeHost = this.IsFarmerRequired(Game1.player.UniqueMultiplayerID);
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (this.IsFarmerRequired(farmer.UniqueMultiplayerID) && !Game1.multiplayer.isDisconnecting(farmer))
				{
					ReadyState remoteState;
					if (!this.ReadyStates.TryGetValue(farmer.UniqueMultiplayerID, out remoteState))
					{
						remoteState = ReadyState.NotReady;
						this.ReadyStates[farmer.UniqueMultiplayerID] = remoteState;
					}
					required++;
					if (remoteState != ReadyState.Ready)
					{
						if (remoteState == ReadyState.Locked)
						{
							ready++;
							locked++;
						}
					}
					else
					{
						ready++;
					}
				}
			}
			if (ready != base.NumberReady || required != base.NumberRequired)
			{
				if (includeHost && Game1.IsDedicatedHost)
				{
					this.SendMessage(ReadyCheckMessageType.UpdateAmounts, new object[]
					{
						ready - ((base.State == ReadyState.Ready) ? 1 : 0),
						required - 1
					});
				}
				else
				{
					this.SendMessage(ReadyCheckMessageType.UpdateAmounts, new object[]
					{
						ready,
						required
					});
				}
				if (ready == required)
				{
					if (!this.Locking)
					{
						int activeLockId = base.ActiveLockId;
						base.ActiveLockId = activeLockId + 1;
						this.Locking = true;
						if (includeHost && base.State == ReadyState.Ready)
						{
							this.ReadyStates[Game1.player.UniqueMultiplayerID] = (base.State = ReadyState.Locked);
							locked = 1;
						}
						this.SendMessage(ReadyCheckMessageType.Lock, new object[]
						{
							base.ActiveLockId
						});
					}
				}
				else if (this.Locking)
				{
					this.Locking = false;
					if (base.State == ReadyState.Locked)
					{
						base.State = ReadyState.Ready;
					}
					foreach (long farmerId in this.ReadyStates.Keys)
					{
						if (this.ReadyStates[farmerId] == ReadyState.Locked && this.IsFarmerRequired(farmerId))
						{
							this.ReadyStates[farmerId] = ReadyState.Ready;
						}
					}
					locked = 0;
					this.SendMessage(ReadyCheckMessageType.Release, new object[]
					{
						base.ActiveLockId
					});
				}
			}
			if (this.Locking && locked == required)
			{
				base.IsReady = true;
				this.SendMessage(ReadyCheckMessageType.Finish, Array.Empty<object>());
			}
			base.NumberReady = ready;
			base.NumberRequired = required;
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x00176D54 File Offset: 0x00174F54
		public override void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message)
		{
			switch (messageType)
			{
			case ReadyCheckMessageType.Ready:
				this.ProcessReady(message);
				return;
			case ReadyCheckMessageType.Cancel:
				this.ProcessCancel(message);
				return;
			case ReadyCheckMessageType.AcceptLock:
				this.ProcessAcceptLock(message);
				return;
			case ReadyCheckMessageType.RejectLock:
				this.ProcessRejectLock(message);
				return;
			case ReadyCheckMessageType.RequireFarmers:
				this.ProcessRequireFarmers(message);
				return;
			}
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 3);
			defaultInterpolatedStringHandler.AppendFormatted("ServerReadyCheck");
			defaultInterpolatedStringHandler.AppendLiteral(" '");
			defaultInterpolatedStringHandler.AppendFormatted(base.Id);
			defaultInterpolatedStringHandler.AppendLiteral("' received invalid message type '");
			defaultInterpolatedStringHandler.AppendFormatted<ReadyCheckMessageType>(messageType);
			defaultInterpolatedStringHandler.AppendLiteral("'.");
			log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x00176E14 File Offset: 0x00175014
		protected override void SendMessage(ReadyCheckMessageType messageType, params object[] data)
		{
			if (Game1.server == null)
			{
				return;
			}
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				Game1.server.sendMessage(farmer.UniqueMultiplayerID, base.CreateSyncMessage(messageType, data));
			}
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x00176E80 File Offset: 0x00175080
		private void ProcessReady(IncomingMessage message)
		{
			if (this.Locking)
			{
				return;
			}
			this.ReadyStates[message.FarmerID] = ReadyState.Ready;
		}

		// Token: 0x0600229F RID: 8863 RVA: 0x00176E9D File Offset: 0x0017509D
		private void ProcessCancel(IncomingMessage message)
		{
			if (this.Locking)
			{
				return;
			}
			this.ReadyStates[message.FarmerID] = ReadyState.NotReady;
		}

		// Token: 0x060022A0 RID: 8864 RVA: 0x00176EBA File Offset: 0x001750BA
		private void ProcessAcceptLock(IncomingMessage message)
		{
			if (message.Reader.ReadInt32() == base.ActiveLockId)
			{
				this.ReadyStates[message.FarmerID] = ReadyState.Locked;
			}
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x00176EE1 File Offset: 0x001750E1
		private void ProcessRejectLock(IncomingMessage message)
		{
			if (message.Reader.ReadInt32() == base.ActiveLockId)
			{
				this.ReadyStates[message.FarmerID] = ReadyState.NotReady;
			}
		}

		// Token: 0x060022A2 RID: 8866 RVA: 0x00176F08 File Offset: 0x00175108
		private void ProcessRequireFarmers(IncomingMessage message)
		{
			int count = message.Reader.ReadInt32();
			HashSet<long> farmerIds = new HashSet<long>();
			for (int i = 0; i < count; i++)
			{
				farmerIds.Add(message.Reader.ReadInt64());
			}
			this.RequireFarmers(farmerIds);
		}

		// Token: 0x060022A3 RID: 8867 RVA: 0x00176F4C File Offset: 0x0017514C
		private void RequireFarmers(ICollection<long> farmerIds)
		{
			this.RequiredFarmers.Clear();
			if (farmerIds == null)
			{
				return;
			}
			foreach (long farmerId in farmerIds)
			{
				this.RequiredFarmers.Add(farmerId);
			}
		}

		// Token: 0x060022A4 RID: 8868 RVA: 0x00176FAC File Offset: 0x001751AC
		private bool IsFarmerRequired(long uid)
		{
			return this.IncludesAll || this.RequiredFarmers.Contains(uid);
		}

		// Token: 0x0400147A RID: 5242
		private readonly Dictionary<long, ReadyState> ReadyStates = new Dictionary<long, ReadyState>();

		// Token: 0x0400147B RID: 5243
		private bool Locking;

		// Token: 0x0400147C RID: 5244
		private readonly HashSet<long> RequiredFarmers = new HashSet<long>();
	}
}
