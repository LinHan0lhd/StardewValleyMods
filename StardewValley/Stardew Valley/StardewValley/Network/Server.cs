using System;

namespace StardewValley.Network
{
	// Token: 0x020001F2 RID: 498
	public abstract class Server : IBandwidthMonitor
	{
		// Token: 0x06002254 RID: 8788 RVA: 0x0017640E File Offset: 0x0017460E
		public Server(IGameServer gameServer)
		{
			this.gameServer = gameServer;
		}

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06002255 RID: 8789
		public abstract int connectionsCount { get; }

		// Token: 0x06002256 RID: 8790
		public abstract void initialize();

		// Token: 0x06002257 RID: 8791
		public abstract void setPrivacy(ServerPrivacy privacy);

		// Token: 0x06002258 RID: 8792
		public abstract void stopServer();

		// Token: 0x06002259 RID: 8793
		public abstract void receiveMessages();

		// Token: 0x0600225A RID: 8794
		public abstract void sendMessage(long peerId, OutgoingMessage message);

		// Token: 0x0600225B RID: 8795
		public abstract bool connected();

		// Token: 0x0600225C RID: 8796 RVA: 0x0017641D File Offset: 0x0017461D
		public virtual bool canAcceptIPConnections()
		{
			return false;
		}

		// Token: 0x0600225D RID: 8797 RVA: 0x00176420 File Offset: 0x00174620
		public virtual bool canOfferInvite()
		{
			return false;
		}

		// Token: 0x0600225E RID: 8798 RVA: 0x00176423 File Offset: 0x00174623
		public virtual void offerInvite()
		{
		}

		// Token: 0x0600225F RID: 8799 RVA: 0x00176425 File Offset: 0x00174625
		public virtual string getInviteCode()
		{
			return null;
		}

		// Token: 0x06002260 RID: 8800 RVA: 0x00176428 File Offset: 0x00174628
		public virtual bool PopulatePlatformData(Farmer farmer)
		{
			return false;
		}

		// Token: 0x06002261 RID: 8801 RVA: 0x0017642B File Offset: 0x0017462B
		public virtual string getUserId(long farmerId)
		{
			return null;
		}

		// Token: 0x06002262 RID: 8802 RVA: 0x0017642E File Offset: 0x0017462E
		public virtual bool hasUserId(string userId)
		{
			return false;
		}

		// Token: 0x06002263 RID: 8803 RVA: 0x00176431 File Offset: 0x00174631
		public virtual float getPingToClient(long farmerId)
		{
			return 0f;
		}

		// Token: 0x06002264 RID: 8804 RVA: 0x00176438 File Offset: 0x00174638
		public virtual bool isConnectionActive(string connectionId)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002265 RID: 8805 RVA: 0x0017643F File Offset: 0x0017463F
		public virtual void onConnect(string connectionId)
		{
			this.gameServer.onConnect(connectionId);
		}

		// Token: 0x06002266 RID: 8806 RVA: 0x0017644D File Offset: 0x0017464D
		public virtual void onDisconnect(string connectionId)
		{
			this.gameServer.onDisconnect(connectionId);
		}

		// Token: 0x06002267 RID: 8807
		public abstract string getUserName(long farmerId);

		// Token: 0x06002268 RID: 8808
		public abstract void setLobbyData(string key, string value);

		// Token: 0x06002269 RID: 8809 RVA: 0x0017645B File Offset: 0x0017465B
		public virtual void kick(long disconnectee)
		{
		}

		// Token: 0x0600226A RID: 8810 RVA: 0x0017645D File Offset: 0x0017465D
		public virtual void playerDisconnected(long disconnectee)
		{
			this.gameServer.playerDisconnected(disconnectee);
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x0600226B RID: 8811 RVA: 0x0017646B File Offset: 0x0017466B
		// (set) Token: 0x0600226C RID: 8812 RVA: 0x00176476 File Offset: 0x00174676
		public bool LogBandwidth
		{
			get
			{
				return this.bandwidthLogger != null;
			}
			set
			{
				if (value)
				{
					this.bandwidthLogger = new BandwidthLogger();
					return;
				}
				this.bandwidthLogger = null;
			}
		}

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x0600226D RID: 8813 RVA: 0x0017648E File Offset: 0x0017468E
		public BandwidthLogger BandwidthLogger
		{
			get
			{
				return this.bandwidthLogger;
			}
		}

		// Token: 0x04001463 RID: 5219
		internal IGameServer gameServer;

		// Token: 0x04001464 RID: 5220
		protected BandwidthLogger bandwidthLogger;
	}
}
