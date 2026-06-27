using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Galaxy.Api;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy
{
	// Token: 0x0200016F RID: 367
	public class GalaxyNetServer : HookableServer
	{
		// Token: 0x06001BF2 RID: 7154 RVA: 0x0013F4EA File Offset: 0x0013D6EA
		public GalaxyNetServer(IGameServer gameServer) : base(gameServer)
		{
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06001BF3 RID: 7155 RVA: 0x0013F509 File Offset: 0x0013D709
		public override int connectionsCount
		{
			get
			{
				if (this.server == null)
				{
					return 0;
				}
				return this.server.ConnectionCount;
			}
		}

		// Token: 0x06001BF4 RID: 7156 RVA: 0x0013F520 File Offset: 0x0013D720
		public override string getUserId(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return this.peers[farmerId].ToString();
		}

		// Token: 0x06001BF5 RID: 7157 RVA: 0x0013F554 File Offset: 0x0013D754
		public override bool hasUserId(string userId)
		{
			foreach (ulong id in this.peers.RightValues)
			{
				if (id.ToString().Equals(userId))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001BF6 RID: 7158 RVA: 0x0013F5B8 File Offset: 0x0013D7B8
		public override bool isConnectionActive(string connection_id)
		{
			foreach (GalaxyID connection in this.server.Connections)
			{
				if (this.getConnectionId(connection) == connection_id && connection.IsValid())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x0013F624 File Offset: 0x0013D824
		public override string getUserName(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return null;
			}
			ulong peerId = this.peers[farmerId];
			string displayName;
			if (this.displayNames.TryGetValue(peerId, out displayName))
			{
				return displayName;
			}
			GalaxyID user = new GalaxyID(peerId);
			return GalaxyInstance.Friends().GetFriendPersonaName(user);
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x0013F674 File Offset: 0x0013D874
		public override float getPingToClient(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return -1f;
			}
			GalaxyID user = new GalaxyID(this.peers[farmerId]);
			return (float)this.server.GetPingWith(user);
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x0013F6B4 File Offset: 0x0013D8B4
		public override void setPrivacy(ServerPrivacy privacy)
		{
			this.server.SetPrivacy(privacy);
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x0013F6C2 File Offset: 0x0013D8C2
		public override bool connected()
		{
			return this.server.Connected;
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x0013F6CF File Offset: 0x0013D8CF
		public override string getInviteCode()
		{
			return this.server.GetInviteCode();
		}

		// Token: 0x06001BFC RID: 7164 RVA: 0x0013F6DC File Offset: 0x0013D8DC
		public override void initialize()
		{
			Game1.log.Verbose("Starting Galaxy server");
			this.host = GalaxyInstance.User().GetGalaxyID();
			this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(new Action<GalaxyID>(this.onProfileDataReady));
			this.server = new GalaxySocket(Multiplayer.protocolVersion);
			this.server.CreateLobby(Game1.options.serverPrivacy, (uint)(Game1.multiplayer.playerLimit * 2));
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x0013F750 File Offset: 0x0013D950
		public override void stopServer()
		{
			Game1.log.Verbose("Stopping Galaxy server");
			this.server.Close();
			GalaxySpecificUserDataListener galaxySpecificUserDataListener = this.galaxySpecificUserDataListener;
			if (galaxySpecificUserDataListener != null)
			{
				galaxySpecificUserDataListener.Dispose();
			}
			this.galaxySpecificUserDataListener = null;
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x0013F784 File Offset: 0x0013D984
		private void onProfileDataReady(GalaxyID userID)
		{
			if (userID == this.host)
			{
				return;
			}
			if (this.displayNames.ContainsKey(userID.ToUint64()))
			{
				return;
			}
			string displayName = null;
			try
			{
				displayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			if (!string.IsNullOrEmpty(displayName))
			{
				this.displayNames[userID.ToUint64()] = displayName;
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(userID);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(displayName);
				defaultInterpolatedStringHandler.AppendLiteral(") connected");
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			else
			{
				IGameLogger log2 = Game1.log;
				GalaxyID userID2 = userID;
				log2.Verbose(((userID2 != null) ? userID2.ToString() : null) + " connected");
			}
			this.onConnect(this.getConnectionId(userID));
			this.gameServer.sendAvailableFarmhands(this.createUserID(userID), this.getConnectionId(userID), delegate(OutgoingMessage msg)
			{
				this.sendMessage(userID, msg);
			});
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x0013F8D0 File Offset: 0x0013DAD0
		public override void receiveMessages()
		{
			if (this.server == null)
			{
				return;
			}
			this.server.Receive(new Action<GalaxyID>(this.onReceiveConnection), new Action<GalaxyID, Stream>(this.onReceiveMessage), new Action<GalaxyID>(this.onReceiveDisconnect), new Action<string>(this.onReceiveError));
			this.server.Heartbeat(this.server.LobbyMembers());
			foreach (GalaxyID client in this.server.Connections)
			{
				if (this.server.GetPingWith(client) > 30000L)
				{
					this.server.Kick(client);
				}
			}
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger == null)
			{
				return;
			}
			bandwidthLogger.Update();
		}

		// Token: 0x06001C00 RID: 7168 RVA: 0x0013F9AC File Offset: 0x0013DBAC
		public override void kick(long disconnectee)
		{
			base.kick(disconnectee);
			if (!this.peers.ContainsLeft(disconnectee))
			{
				return;
			}
			GalaxyID user = new GalaxyID(this.peers[disconnectee]);
			this.server.Kick(user);
			this.sendMessage(user, new OutgoingMessage(23, Game1.player, Array.Empty<object>()));
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x0013FA05 File Offset: 0x0013DC05
		public string getConnectionId(GalaxyID peer)
		{
			return "GN_" + Convert.ToString(peer.ToUint64());
		}

		// Token: 0x06001C02 RID: 7170 RVA: 0x0013FA1C File Offset: 0x0013DC1C
		private string createUserID(GalaxyID peer)
		{
			return Convert.ToString(peer.ToUint64());
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x0013FA29 File Offset: 0x0013DC29
		protected virtual void onReceiveConnection(GalaxyID peer)
		{
			if (this.gameServer.isUserBanned(peer.ToString()))
			{
				return;
			}
			if (GalaxyInstance.User().IsUserDataAvailable(peer))
			{
				this.onProfileDataReady(peer);
				return;
			}
			GalaxyInstance.User().RequestUserData(peer);
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x0013FA60 File Offset: 0x0013DC60
		protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
		{
			GalaxyNetServer.<>c__DisplayClass24_0 CS$<>8__locals1 = new GalaxyNetServer.<>c__DisplayClass24_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.peer = peer;
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
					base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
					{
						CS$<>8__locals1.<>4__this.sendMessage(CS$<>8__locals1.peer, outgoing);
					}, delegate
					{
						if (CS$<>8__locals1.<>4__this.peers.ContainsLeft(message.FarmerID) && CS$<>8__locals1.<>4__this.peers[message.FarmerID] == CS$<>8__locals1.peer.ToUint64())
						{
							CS$<>8__locals1.<>4__this.gameServer.processIncomingMessage(message);
							return;
						}
						if (message.MessageType == 2)
						{
							NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
							GalaxyID capturedPeer = new GalaxyID(CS$<>8__locals1.peer.ToUint64());
							CS$<>8__locals1.<>4__this.gameServer.checkFarmhandRequest(CS$<>8__locals1.<>4__this.createUserID(CS$<>8__locals1.peer), CS$<>8__locals1.<>4__this.getConnectionId(CS$<>8__locals1.peer), farmer, delegate(OutgoingMessage msg)
							{
								CS$<>8__locals1.<>4__this.sendMessage(capturedPeer, msg);
							}, delegate
							{
								CS$<>8__locals1.<>4__this.peers[farmer.Value.UniqueMultiplayerID] = capturedPeer.ToUint64();
							});
						}
					});
				}
			}
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x0013FB30 File Offset: 0x0013DD30
		public virtual void onReceiveDisconnect(GalaxyID peer)
		{
			Game1.log.Verbose(((peer != null) ? peer.ToString() : null) + " disconnected");
			this.onDisconnect(this.getConnectionId(peer));
			if (this.peers.ContainsRight(peer.ToUint64()))
			{
				this.playerDisconnected(this.peers[peer.ToUint64()]);
			}
			this.displayNames.Remove(peer.ToUint64());
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x0013FBA7 File Offset: 0x0013DDA7
		protected virtual void onReceiveError(string messageKey)
		{
			Game1.log.Error("Server error: " + Game1.content.LoadString(messageKey), null);
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x0013FBC9 File Offset: 0x0013DDC9
		public override void playerDisconnected(long disconnectee)
		{
			base.playerDisconnected(disconnectee);
			this.peers.RemoveLeft(disconnectee);
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x0013FBDE File Offset: 0x0013DDDE
		public override void sendMessage(long peerId, OutgoingMessage message)
		{
			if (this.peers.ContainsLeft(peerId))
			{
				this.sendMessage(new GalaxyID(this.peers[peerId]), message);
			}
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x0013FC08 File Offset: 0x0013DE08
		protected virtual void sendMessage(GalaxyID peer, OutgoingMessage message)
		{
			if (this.bandwidthLogger != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						message.Write(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						byte[] bytes = stream.ToArray();
						this.server.Send(peer, bytes);
						this.bandwidthLogger.RecordBytesUp((long)bytes.Length);
						return;
					}
				}
			}
			this.server.Send(peer, message);
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x0013FCA0 File Offset: 0x0013DEA0
		public override void setLobbyData(string key, string value)
		{
			this.server.SetLobbyData(key, value);
		}

		// Token: 0x040010E5 RID: 4325
		private GalaxyID host;

		// Token: 0x040010E6 RID: 4326
		protected GalaxySocket server;

		// Token: 0x040010E7 RID: 4327
		private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

		// Token: 0x040010E8 RID: 4328
		protected Bimap<long, ulong> peers = new Bimap<long, ulong>();

		// Token: 0x040010E9 RID: 4329
		protected Dictionary<ulong, string> displayNames = new Dictionary<ulong, string>();
	}
}
