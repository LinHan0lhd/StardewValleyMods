using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lidgren.Network;

namespace StardewValley.Network
{
	// Token: 0x020001D6 RID: 470
	public class LidgrenServer : HookableServer
	{
		// Token: 0x1700035E RID: 862
		// (get) Token: 0x060020D1 RID: 8401 RVA: 0x00171718 File Offset: 0x0016F918
		public override int connectionsCount
		{
			get
			{
				if (this.server == null)
				{
					return 0;
				}
				return this.server.ConnectionsCount;
			}
		}

		// Token: 0x060020D2 RID: 8402 RVA: 0x0017172F File Offset: 0x0016F92F
		public LidgrenServer(IGameServer gameServer) : base(gameServer)
		{
		}

		// Token: 0x060020D3 RID: 8403 RVA: 0x00171750 File Offset: 0x0016F950
		public override bool isConnectionActive(string connectionID)
		{
			foreach (NetConnection connection in this.server.Connections)
			{
				if (this.getConnectionId(connection) == connectionID && connection.Status == NetConnectionStatus.Connected)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060020D4 RID: 8404 RVA: 0x001717C0 File Offset: 0x0016F9C0
		public override string getUserId(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return this.peers[farmerId].RemoteEndPoint.Address.ToString();
		}

		// Token: 0x060020D5 RID: 8405 RVA: 0x001717F0 File Offset: 0x0016F9F0
		public override bool hasUserId(string userId)
		{
			using (IEnumerator<NetConnection> enumerator = this.peers.RightValues.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.RemoteEndPoint.Address.ToString().Equals(userId))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060020D6 RID: 8406 RVA: 0x00171858 File Offset: 0x0016FA58
		public override string getUserName(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return this.peers[farmerId].RemoteEndPoint.Address.ToString();
		}

		// Token: 0x060020D7 RID: 8407 RVA: 0x00171885 File Offset: 0x0016FA85
		public override float getPingToClient(long farmerId)
		{
			if (!this.peers.ContainsLeft(farmerId))
			{
				return -1f;
			}
			return this.peers[farmerId].AverageRoundtripTime / 2f * 1000f;
		}

		// Token: 0x060020D8 RID: 8408 RVA: 0x001718B8 File Offset: 0x0016FAB8
		public override void setPrivacy(ServerPrivacy privacy)
		{
		}

		// Token: 0x060020D9 RID: 8409 RVA: 0x001718BA File Offset: 0x0016FABA
		public override bool canAcceptIPConnections()
		{
			return true;
		}

		// Token: 0x060020DA RID: 8410 RVA: 0x001718BD File Offset: 0x0016FABD
		public override bool connected()
		{
			return this.server != null;
		}

		// Token: 0x060020DB RID: 8411 RVA: 0x001718C8 File Offset: 0x0016FAC8
		public override void initialize()
		{
			Game1.log.Verbose("Starting LAN server");
			NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
			config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			config.Port = 24642;
			config.ConnectionTimeout = 30f;
			config.PingInterval = 5f;
			config.MaximumConnections = Game1.multiplayer.playerLimit * 2;
			config.MaximumTransmissionUnit = 1200;
			this.server = new NetServer(config);
			this.server.Start();
		}

		// Token: 0x060020DC RID: 8412 RVA: 0x00171954 File Offset: 0x0016FB54
		public override void stopServer()
		{
			Game1.log.Verbose("Stopping LAN server");
			this.server.Shutdown("Server shutting down...");
			this.server.FlushSendQueue();
			this.introductionsSent.Clear();
			this.peers.Clear();
		}

		// Token: 0x060020DD RID: 8413 RVA: 0x001719A4 File Offset: 0x0016FBA4
		public static bool IsLocal(string host_name_or_address)
		{
			if (string.IsNullOrEmpty(host_name_or_address))
			{
				return false;
			}
			bool result;
			try
			{
				IEnumerable<IPAddress> hostAddresses = Dns.GetHostAddresses(host_name_or_address);
				IPAddress[] local_ips = Dns.GetHostAddresses(Dns.GetHostName());
				result = hostAddresses.Any((IPAddress host_ip) => IPAddress.IsLoopback(host_ip) || local_ips.Contains(host_ip));
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060020DE RID: 8414 RVA: 0x00171A04 File Offset: 0x0016FC04
		public override void receiveMessages()
		{
			NetIncomingMessage inc;
			while ((inc = this.server.ReadMessage()) != null)
			{
				BandwidthLogger bandwidthLogger = this.bandwidthLogger;
				if (bandwidthLogger != null)
				{
					bandwidthLogger.RecordBytesDown((long)inc.LengthBytes);
				}
				NetIncomingMessageType messageType = inc.MessageType;
				if (messageType <= NetIncomingMessageType.Data)
				{
					if (messageType != NetIncomingMessageType.StatusChanged)
					{
						if (messageType != NetIncomingMessageType.ConnectionApproval)
						{
							if (messageType != NetIncomingMessageType.Data)
							{
								goto IL_166;
							}
							this.parseDataMessageFromClient(inc);
						}
						else if (Game1.options.ipConnectionsEnabled || this.gameServer.IsLocalMultiplayerInitiatedServer())
						{
							inc.SenderConnection.Approve();
						}
						else
						{
							inc.SenderConnection.Deny();
						}
					}
					else
					{
						this.statusChanged(inc);
					}
				}
				else
				{
					if (messageType <= NetIncomingMessageType.DebugMessage)
					{
						if (messageType != NetIncomingMessageType.DiscoveryRequest)
						{
							if (messageType != NetIncomingMessageType.DebugMessage)
							{
								goto IL_166;
							}
						}
						else
						{
							if ((Game1.options.ipConnectionsEnabled || this.gameServer.IsLocalMultiplayerInitiatedServer()) && (!this.gameServer.IsLocalMultiplayerInitiatedServer() || LidgrenServer.IsLocal(inc.SenderEndPoint.Address.ToString())) && !this.gameServer.isUserBanned(inc.SenderEndPoint.Address.ToString()))
							{
								this.sendVersionInfo(inc);
								goto IL_171;
							}
							goto IL_171;
						}
					}
					else if (messageType != NetIncomingMessageType.WarningMessage && messageType != NetIncomingMessageType.ErrorMessage)
					{
						goto IL_166;
					}
					string message = inc.ReadString();
					Game1.log.Verbose(inc.MessageType.ToString() + ": " + message);
					Game1.debugOutput = message;
				}
				IL_171:
				this.server.Recycle(inc);
				continue;
				IL_166:
				Game1.debugOutput = inc.ToString();
				goto IL_171;
			}
			using (List<NetConnection>.Enumerator enumerator = this.server.Connections.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NetConnection conn = enumerator.Current;
					if (conn.Status == NetConnectionStatus.Connected && !this.introductionsSent.Contains(conn))
					{
						Action<OutgoingMessage> <>9__2;
						if (!this.gameServer.whenGameAvailable(delegate
						{
							IGameServer gameServer = this.gameServer;
							string userId = "";
							string connectionId = this.getConnectionId(conn);
							Action<OutgoingMessage> sendMessage;
							if ((sendMessage = <>9__2) == null)
							{
								sendMessage = (<>9__2 = delegate(OutgoingMessage msg)
								{
									this.sendMessage(conn, msg);
								});
							}
							gameServer.sendAvailableFarmhands(userId, connectionId, sendMessage);
						}, () => Game1.gameMode != 6))
						{
							Game1.log.Verbose("Postponing introduction message");
							this.sendMessage(conn, new OutgoingMessage(11, Game1.player, new object[]
							{
								"Strings\\UI:Client_WaitForHostLoad"
							}));
						}
						this.introductionsSent.Add(conn);
					}
				}
			}
			BandwidthLogger bandwidthLogger2 = this.bandwidthLogger;
			if (bandwidthLogger2 == null)
			{
				return;
			}
			bandwidthLogger2.Update();
		}

		// Token: 0x060020DF RID: 8415 RVA: 0x00171CBC File Offset: 0x0016FEBC
		private void sendVersionInfo(NetIncomingMessage message)
		{
			NetOutgoingMessage response = this.server.CreateMessage();
			response.Write(Multiplayer.protocolVersion);
			response.Write("StardewValley");
			this.server.SendDiscoveryResponse(response, message.SenderEndPoint);
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger == null)
			{
				return;
			}
			bandwidthLogger.RecordBytesUp((long)response.LengthBytes);
		}

		// Token: 0x060020E0 RID: 8416 RVA: 0x00171D14 File Offset: 0x0016FF14
		private void statusChanged(NetIncomingMessage message)
		{
			NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
			if (status == NetConnectionStatus.Connected)
			{
				this.onConnect(this.getConnectionId(message.SenderConnection));
				return;
			}
			if (status - NetConnectionStatus.Disconnecting > 1)
			{
				return;
			}
			this.onDisconnect(this.getConnectionId(message.SenderConnection));
			if (this.peers.ContainsRight(message.SenderConnection))
			{
				this.playerDisconnected(this.peers[message.SenderConnection]);
			}
		}

		// Token: 0x060020E1 RID: 8417 RVA: 0x00171D82 File Offset: 0x0016FF82
		public override void kick(long disconnectee)
		{
			base.kick(disconnectee);
			if (this.peers.ContainsLeft(disconnectee))
			{
				this.peers[disconnectee].Disconnect(Multiplayer.kicked);
				this.server.FlushSendQueue();
				this.playerDisconnected(disconnectee);
			}
		}

		// Token: 0x060020E2 RID: 8418 RVA: 0x00171DC1 File Offset: 0x0016FFC1
		public override void playerDisconnected(long disconnectee)
		{
			base.playerDisconnected(disconnectee);
			this.introductionsSent.Remove(this.peers[disconnectee]);
			this.peers.RemoveLeft(disconnectee);
		}

		// Token: 0x060020E3 RID: 8419 RVA: 0x00171DF0 File Offset: 0x0016FFF0
		protected virtual void parseDataMessageFromClient(NetIncomingMessage dataMsg)
		{
			LidgrenServer.<>c__DisplayClass23_0 CS$<>8__locals1 = new LidgrenServer.<>c__DisplayClass23_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.dataMsg = dataMsg;
			CS$<>8__locals1.peer = CS$<>8__locals1.dataMsg.SenderConnection;
			using (IncomingMessage message = new IncomingMessage())
			{
				using (NetBufferReadStream stream = new NetBufferReadStream(CS$<>8__locals1.dataMsg))
				{
					Action <>9__1;
					while ((long)CS$<>8__locals1.dataMsg.LengthBits - CS$<>8__locals1.dataMsg.Position >= 8L)
					{
						LidgrenMessageUtils.ReadStreamToMessage(stream, message);
						Action<IncomingMessage, Action<OutgoingMessage>, Action> onProcessingMessage = base.OnProcessingMessage;
						IncomingMessage message2 = message;
						Action<OutgoingMessage> arg;
						if ((arg = CS$<>8__locals1.<>9__0) == null)
						{
							arg = (CS$<>8__locals1.<>9__0 = delegate(OutgoingMessage outgoing)
							{
								CS$<>8__locals1.<>4__this.sendMessage(CS$<>8__locals1.peer, outgoing);
							});
						}
						Action arg2;
						if ((arg2 = <>9__1) == null)
						{
							arg2 = (<>9__1 = delegate()
							{
								if (CS$<>8__locals1.<>4__this.peers.ContainsLeft(message.FarmerID) && CS$<>8__locals1.<>4__this.peers[message.FarmerID] == CS$<>8__locals1.peer)
								{
									CS$<>8__locals1.<>4__this.gameServer.processIncomingMessage(message);
									return;
								}
								if (message.MessageType == 2)
								{
									NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
									IGameServer gameServer = CS$<>8__locals1.<>4__this.gameServer;
									string userId = "";
									string connectionId = CS$<>8__locals1.<>4__this.getConnectionId(CS$<>8__locals1.dataMsg.SenderConnection);
									NetFarmerRoot farmer2 = farmer;
									Action<OutgoingMessage> sendMessage;
									if ((sendMessage = CS$<>8__locals1.<>9__2) == null)
									{
										sendMessage = (CS$<>8__locals1.<>9__2 = delegate(OutgoingMessage msg)
										{
											CS$<>8__locals1.<>4__this.sendMessage(CS$<>8__locals1.peer, msg);
										});
									}
									gameServer.checkFarmhandRequest(userId, connectionId, farmer2, sendMessage, delegate
									{
										CS$<>8__locals1.<>4__this.peers[farmer.Value.UniqueMultiplayerID] = CS$<>8__locals1.peer;
									});
								}
							});
						}
						onProcessingMessage(message2, arg, arg2);
					}
				}
			}
		}

		// Token: 0x060020E4 RID: 8420 RVA: 0x00171F20 File Offset: 0x00170120
		public string getConnectionId(NetConnection connection)
		{
			return "L_" + connection.RemoteUniqueIdentifier.ToString();
		}

		// Token: 0x060020E5 RID: 8421 RVA: 0x00171F45 File Offset: 0x00170145
		public override void sendMessage(long peerId, OutgoingMessage message)
		{
			if (this.peers.ContainsLeft(peerId))
			{
				this.sendMessage(this.peers[peerId], message);
			}
		}

		// Token: 0x060020E6 RID: 8422 RVA: 0x00171F68 File Offset: 0x00170168
		protected virtual void sendMessage(NetConnection connection, OutgoingMessage message)
		{
			NetOutgoingMessage msg = this.server.CreateMessage();
			LidgrenMessageUtils.WriteMessage(message, msg);
			this.server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger == null)
			{
				return;
			}
			bandwidthLogger.RecordBytesUp((long)msg.LengthBytes);
		}

		// Token: 0x060020E7 RID: 8423 RVA: 0x00171FAF File Offset: 0x001701AF
		public override void setLobbyData(string key, string value)
		{
		}

		// Token: 0x040013D3 RID: 5075
		public const int defaultPort = 24642;

		// Token: 0x040013D4 RID: 5076
		public NetServer server;

		// Token: 0x040013D5 RID: 5077
		private HashSet<NetConnection> introductionsSent = new HashSet<NetConnection>();

		// Token: 0x040013D6 RID: 5078
		protected Bimap<long, NetConnection> peers = new Bimap<long, NetConnection>();
	}
}
