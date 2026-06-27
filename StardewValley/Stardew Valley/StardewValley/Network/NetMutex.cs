using System;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E5 RID: 485
	public class NetMutex : INetObject<NetFields>
	{
		// Token: 0x17000380 RID: 896
		// (get) Token: 0x06002176 RID: 8566 RVA: 0x001736B8 File Offset: 0x001718B8
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("NetMutex");

		// Token: 0x06002177 RID: 8567 RVA: 0x001736C0 File Offset: 0x001718C0
		public NetMutex()
		{
			this.NetFields.SetOwner(this).AddField(this.owner, "owner").AddField(this.lockRequest, "lockRequest");
			this.lockRequest.onEvent += delegate(long playerId)
			{
				if (!Game1.IsMasterGame)
				{
					return;
				}
				if (this.owner.Value == -1L || this.owner.Value == playerId)
				{
					this.owner.Value = playerId;
					this.owner.MarkDirty();
				}
			};
		}

		// Token: 0x06002178 RID: 8568 RVA: 0x00173758 File Offset: 0x00171958
		public void RequestLock(Action acquired = null, Action failed = null)
		{
			if (this.owner.Value == Game1.player.UniqueMultiplayerID)
			{
				if (acquired != null)
				{
					acquired();
				}
				return;
			}
			if (this.owner.Value != -1L)
			{
				if (failed != null)
				{
					failed();
				}
				return;
			}
			this.lockRequest.Fire(Game1.player.UniqueMultiplayerID);
			this.onLockAcquired = acquired;
			this.onLockFailed = failed;
		}

		// Token: 0x06002179 RID: 8569 RVA: 0x001737C2 File Offset: 0x001719C2
		public void ReleaseLock()
		{
			this.owner.Value = -1L;
			this.onLockFailed = null;
			this.onLockAcquired = null;
		}

		// Token: 0x0600217A RID: 8570 RVA: 0x001737DF File Offset: 0x001719DF
		public bool IsLocked()
		{
			return this.owner.Value != -1L;
		}

		// Token: 0x0600217B RID: 8571 RVA: 0x001737F3 File Offset: 0x001719F3
		public bool IsLockHeld()
		{
			return this.owner.Value == Game1.player.UniqueMultiplayerID;
		}

		// Token: 0x0600217C RID: 8572 RVA: 0x0017380C File Offset: 0x00171A0C
		public void Update(GameLocation location)
		{
			this.Update(location.farmers);
		}

		// Token: 0x0600217D RID: 8573 RVA: 0x0017381C File Offset: 0x00171A1C
		public void Update(FarmerCollection farmers)
		{
			this.lockRequest.Poll();
			if (this.owner.Value != this.prevOwner)
			{
				if (this.owner.Value == Game1.player.UniqueMultiplayerID && this.onLockAcquired != null)
				{
					this.onLockAcquired();
				}
				if (this.owner.Value != Game1.player.UniqueMultiplayerID && this.onLockFailed != null)
				{
					this.onLockFailed();
				}
				this.onLockAcquired = null;
				this.onLockFailed = null;
				this.prevOwner = this.owner.Value;
			}
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (this.owner.Value != -1L && farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == this.owner.Value && f.locationBeforeForcedEvent.Value == null) == null)
			{
				this.ReleaseLock();
				return;
			}
		}

		// Token: 0x040013FE RID: 5118
		public const long NoOwner = -1L;

		// Token: 0x040013FF RID: 5119
		private long prevOwner = -1L;

		// Token: 0x04001400 RID: 5120
		private readonly NetLong owner = new NetLong(-1L)
		{
			InterpolationWait = false
		};

		// Token: 0x04001401 RID: 5121
		private readonly NetEvent1Field<long, NetLong> lockRequest = new NetEvent1Field<long, NetLong>
		{
			InterpolationWait = false
		};

		// Token: 0x04001402 RID: 5122
		private Action onLockAcquired;

		// Token: 0x04001403 RID: 5123
		private Action onLockFailed;
	}
}
