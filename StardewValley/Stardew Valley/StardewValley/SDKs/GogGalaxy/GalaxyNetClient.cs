using System;
using System.IO;
using System.Linq;
using Galaxy.Api;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy
{
	// Token: 0x0200016D RID: 365
	public class GalaxyNetClient : HookableClient
	{
		// Token: 0x06001BCA RID: 7114 RVA: 0x0013EBCF File Offset: 0x0013CDCF
		public GalaxyNetClient(GalaxyID lobbyId)
		{
			this.lobbyId = lobbyId;
			this.hostDisplayName = null;
		}

		// Token: 0x06001BCB RID: 7115 RVA: 0x0013EBE8 File Offset: 0x0013CDE8
		protected override void Finalize()
		{
			try
			{
				GalaxySpecificUserDataListener galaxySpecificUserDataListener = this.galaxySpecificUserDataListener;
				if (galaxySpecificUserDataListener != null)
				{
					galaxySpecificUserDataListener.Dispose();
				}
				this.galaxySpecificUserDataListener = null;
			}
			finally
			{
				base.Finalize();
			}
		}

		// Token: 0x06001BCC RID: 7116 RVA: 0x0013EC28 File Offset: 0x0013CE28
		private void onProfileDataReady(GalaxyID userID)
		{
			if (userID != this.serverId)
			{
				return;
			}
			this.hostDisplayName = null;
			try
			{
				this.hostDisplayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			GalaxySpecificUserDataListener galaxySpecificUserDataListener = this.galaxySpecificUserDataListener;
			if (galaxySpecificUserDataListener != null)
			{
				galaxySpecificUserDataListener.Dispose();
			}
			this.galaxySpecificUserDataListener = null;
		}

		// Token: 0x06001BCD RID: 7117 RVA: 0x0013EC90 File Offset: 0x0013CE90
		public override string getUserID()
		{
			return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
		}

		// Token: 0x06001BCE RID: 7118 RVA: 0x0013ECA6 File Offset: 0x0013CEA6
		protected override string getHostUserName()
		{
			if (!string.IsNullOrEmpty(this.hostDisplayName))
			{
				return this.hostDisplayName;
			}
			return GalaxyInstance.Friends().GetFriendPersonaName(this.serverId);
		}

		// Token: 0x06001BCF RID: 7119 RVA: 0x0013ECCC File Offset: 0x0013CECC
		public override float GetPingToHost()
		{
			return this.lastPingMs;
		}

		// Token: 0x06001BD0 RID: 7120 RVA: 0x0013ECD4 File Offset: 0x0013CED4
		protected override void connectImpl()
		{
			this.client = new GalaxySocket(Multiplayer.protocolVersion);
			GalaxyInstance.User().GetGalaxyID();
			this.client.JoinLobby(this.lobbyId, new Action<string>(this.onReceiveError));
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x0013ED10 File Offset: 0x0013CF10
		public override void disconnect(bool neatly = true)
		{
			if (this.client == null)
			{
				return;
			}
			IGameLogger log = Game1.log;
			string str = "Disconnecting from server ";
			GalaxyID galaxyID = this.lobbyId;
			log.Verbose(str + ((galaxyID != null) ? galaxyID.ToString() : null));
			this.client.Close();
			this.client = null;
			this.connectionMessage = null;
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x0013ED68 File Offset: 0x0013CF68
		protected override void receiveMessagesImpl()
		{
			if (this.client == null || !this.client.Connected)
			{
				return;
			}
			if (this.client.Connected && this.serverId == null)
			{
				IGameLogger log = Game1.log;
				string str = "Connected to server ";
				GalaxyID galaxyID = this.lobbyId;
				log.Verbose(str + ((galaxyID != null) ? galaxyID.ToString() : null));
				this.serverId = this.client.LobbyOwner;
				if (GalaxyInstance.User().IsUserDataAvailable(this.serverId))
				{
					this.onProfileDataReady(this.serverId);
				}
				else
				{
					this.hostDisplayName = GalaxyNetHelper.TryGetHostSteamDisplayName(this.lobbyId);
					this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(new Action<GalaxyID>(this.onProfileDataReady));
					GalaxyInstance.User().RequestUserData(this.serverId);
				}
			}
			this.client.Receive(new Action<GalaxyID>(this.onReceiveConnection), new Action<GalaxyID, Stream>(this.onReceiveMessage), new Action<GalaxyID>(this.onReceiveDisconnect), new Action<string>(this.onReceiveError));
			if (this.client != null)
			{
				this.client.Heartbeat(Enumerable.Repeat<GalaxyID>(this.serverId, 1));
				this.lastPingMs = (float)this.client.GetPingWith(this.serverId);
				if (this.lastPingMs > 30000f)
				{
					this.timedOut = true;
					this.pendingDisconnect = Multiplayer.DisconnectType.GalaxyTimeout;
					this.disconnect(true);
				}
			}
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x0013EED4 File Offset: 0x0013D0D4
		protected virtual void onReceiveConnection(GalaxyID peer)
		{
		}

		// Token: 0x06001BD4 RID: 7124 RVA: 0x0013EED8 File Offset: 0x0013D0D8
		protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
		{
			if (peer != this.serverId)
			{
				return;
			}
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesDown(messageStream.Length);
			}
			using (IncomingMessage message = new IncomingMessage())
			{
				using (BinaryReader reader = new BinaryReader(messageStream))
				{
					message.Read(reader);
					base.OnProcessingMessage(message, new Action<OutgoingMessage>(this.sendMessageImpl), delegate
					{
						this.processIncomingMessage(message);
					});
				}
			}
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x0013EF9C File Offset: 0x0013D19C
		protected virtual void onReceiveDisconnect(GalaxyID peer)
		{
			if (peer != this.serverId)
			{
				Game1.multiplayer.playerDisconnected((long)peer.ToUint64());
				return;
			}
			this.timedOut = true;
			this.pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
		}

		// Token: 0x06001BD6 RID: 7126 RVA: 0x0013EFCB File Offset: 0x0013D1CB
		protected virtual void onReceiveError(string message)
		{
			this.connectionMessage = message;
		}

		// Token: 0x06001BD7 RID: 7127 RVA: 0x0013EFD4 File Offset: 0x0013D1D4
		protected virtual void sendMessageImpl(OutgoingMessage message)
		{
			if (this.client == null || !this.client.Connected || this.serverId == null)
			{
				return;
			}
			if (this.bandwidthLogger != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						message.Write(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						byte[] bytes = stream.ToArray();
						this.client.Send(this.serverId, bytes);
						this.bandwidthLogger.RecordBytesUp((long)bytes.Length);
						return;
					}
				}
			}
			this.client.Send(this.serverId, message);
		}

		// Token: 0x06001BD8 RID: 7128 RVA: 0x0013F098 File Offset: 0x0013D298
		public override void sendMessage(OutgoingMessage message)
		{
			base.OnSendingMessage(message, new Action<OutgoingMessage>(this.sendMessageImpl), delegate
			{
				this.sendMessageImpl(message);
			});
		}

		// Token: 0x040010D5 RID: 4309
		public GalaxyID lobbyId;

		// Token: 0x040010D6 RID: 4310
		protected GalaxySocket client;

		// Token: 0x040010D7 RID: 4311
		private GalaxyID serverId;

		// Token: 0x040010D8 RID: 4312
		private string hostDisplayName;

		// Token: 0x040010D9 RID: 4313
		private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

		// Token: 0x040010DA RID: 4314
		private float lastPingMs;
	}
}
