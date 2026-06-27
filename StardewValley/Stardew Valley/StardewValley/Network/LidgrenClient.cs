using System;
using System.Net;
using System.Runtime.CompilerServices;
using Lidgren.Network;
using StardewValley.Logging;

namespace StardewValley.Network
{
	// Token: 0x020001D4 RID: 468
	public class LidgrenClient : HookableClient
	{
		// Token: 0x060020BF RID: 8383 RVA: 0x00170FEB File Offset: 0x0016F1EB
		public LidgrenClient(string address)
		{
			this.address = address;
		}

		// Token: 0x060020C0 RID: 8384 RVA: 0x00171005 File Offset: 0x0016F205
		public override string getUserID()
		{
			return "";
		}

		// Token: 0x060020C1 RID: 8385 RVA: 0x0017100C File Offset: 0x0016F20C
		public override float GetPingToHost()
		{
			return this.lastLatencyMs / 2f;
		}

		// Token: 0x060020C2 RID: 8386 RVA: 0x0017101C File Offset: 0x0016F21C
		protected override string getHostUserName()
		{
			NetClient netClient = this.client;
			string text;
			if (netClient == null)
			{
				text = null;
			}
			else
			{
				NetConnection serverConnection = netClient.ServerConnection;
				if (serverConnection == null)
				{
					text = null;
				}
				else
				{
					IPEndPoint remoteEndPoint = serverConnection.RemoteEndPoint;
					if (remoteEndPoint == null)
					{
						text = null;
					}
					else
					{
						IPAddress ipaddress = remoteEndPoint.Address;
						text = ((ipaddress != null) ? ipaddress.ToString() : null);
					}
				}
			}
			return text ?? "";
		}

		// Token: 0x060020C3 RID: 8387 RVA: 0x00171068 File Offset: 0x0016F268
		protected override void connectImpl()
		{
			NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
			config.ConnectionTimeout = 30f;
			config.PingInterval = 5f;
			config.MaximumTransmissionUnit = 1200;
			this.client = new NetClient(config);
			this.client.Start();
			this.attemptConnection();
		}

		// Token: 0x060020C4 RID: 8388 RVA: 0x001710D4 File Offset: 0x0016F2D4
		private void attemptConnection()
		{
			int port = 24642;
			if (this.address.Contains(':'))
			{
				string[] split = this.address.Split(':', StringSplitOptions.None);
				this.address = split[0];
				try
				{
					port = Convert.ToInt32(split[1]);
				}
				catch (Exception)
				{
					port = 24642;
				}
			}
			this.client.DiscoverKnownPeer(this.address, port);
			this.lastAttemptMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
		}

		// Token: 0x060020C5 RID: 8389 RVA: 0x00171160 File Offset: 0x0016F360
		public override void disconnect(bool neatly = true)
		{
			if (this.client == null)
			{
				return;
			}
			if (this.client.ConnectionStatus != NetConnectionStatus.Disconnected && this.client.ConnectionStatus != NetConnectionStatus.Disconnecting)
			{
				if (neatly)
				{
					this.sendMessage(new OutgoingMessage(19, Game1.player, Array.Empty<object>()));
				}
				this.client.FlushSendQueue();
				this.client.Disconnect("");
				this.client.FlushSendQueue();
			}
			this.connectionMessage = null;
		}

		// Token: 0x060020C6 RID: 8390 RVA: 0x001711D9 File Offset: 0x0016F3D9
		protected virtual bool validateProtocol(string version)
		{
			return version == Multiplayer.protocolVersion;
		}

		// Token: 0x060020C7 RID: 8391 RVA: 0x001711E8 File Offset: 0x0016F3E8
		protected override void receiveMessagesImpl()
		{
			if (this.client != null && !this.serverDiscovered && DateTime.UtcNow.TimeOfDay.TotalMilliseconds >= this.lastAttemptMs + (double)this.retryMs && this.retryAttempts < this.maxRetryAttempts)
			{
				this.attemptConnection();
				this.retryAttempts++;
			}
			NetIncomingMessage inc;
			while ((inc = this.client.ReadMessage()) != null)
			{
				NetIncomingMessageType messageType = inc.MessageType;
				if (messageType <= NetIncomingMessageType.DiscoveryResponse)
				{
					if (messageType != NetIncomingMessageType.StatusChanged)
					{
						if (messageType != NetIncomingMessageType.Data)
						{
							if (messageType == NetIncomingMessageType.DiscoveryResponse)
							{
								if (!this.serverDiscovered)
								{
									IGameLogger log = Game1.log;
									string str = "Found server at ";
									IPEndPoint senderEndPoint = inc.SenderEndPoint;
									log.Verbose(str + ((senderEndPoint != null) ? senderEndPoint.ToString() : null));
									string protocolVersion = inc.ReadString();
									if (this.validateProtocol(protocolVersion))
									{
										this.serverName = inc.ReadString();
										this.receiveHandshake(inc);
										this.serverDiscovered = true;
									}
									else
									{
										IGameLogger log2 = Game1.log;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
										defaultInterpolatedStringHandler.AppendLiteral("Failed to connect. The server's protocol (");
										defaultInterpolatedStringHandler.AppendFormatted(protocolVersion);
										defaultInterpolatedStringHandler.AppendLiteral(") does not match our own (");
										defaultInterpolatedStringHandler.AppendFormatted(Multiplayer.protocolVersion);
										defaultInterpolatedStringHandler.AppendLiteral(").");
										log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
										this.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_FailedProtocolVersion");
										this.client.Disconnect("");
									}
								}
							}
						}
						else
						{
							this.parseDataMessageFromServer(inc);
						}
					}
					else
					{
						this.statusChanged(inc);
					}
				}
				else
				{
					if (messageType <= NetIncomingMessageType.WarningMessage)
					{
						if (messageType != NetIncomingMessageType.DebugMessage && messageType != NetIncomingMessageType.WarningMessage)
						{
							continue;
						}
					}
					else if (messageType != NetIncomingMessageType.ErrorMessage)
					{
						if (messageType == NetIncomingMessageType.ConnectionLatencyUpdated)
						{
							this.readLatency(inc);
							continue;
						}
						continue;
					}
					string message = inc.ReadString();
					Game1.log.Verbose(inc.MessageType.ToString() + ": " + message);
					Game1.debugOutput = message;
				}
			}
		}

		// Token: 0x060020C8 RID: 8392 RVA: 0x00171401 File Offset: 0x0016F601
		private void readLatency(NetIncomingMessage msg)
		{
			this.lastLatencyMs = msg.ReadFloat() * 1000f;
		}

		// Token: 0x060020C9 RID: 8393 RVA: 0x00171415 File Offset: 0x0016F615
		private void receiveHandshake(NetIncomingMessage msg)
		{
			this.client.Connect(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
		}

		// Token: 0x060020CA RID: 8394 RVA: 0x00171440 File Offset: 0x0016F640
		private void statusChanged(NetIncomingMessage message)
		{
			NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
			if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
			{
				string byeMessage = message.ReadString();
				this.clientRemotelyDisconnected(status, byeMessage);
			}
		}

		// Token: 0x060020CB RID: 8395 RVA: 0x0017146B File Offset: 0x0016F66B
		private void clientRemotelyDisconnected(NetConnectionStatus status, string message)
		{
			this.timedOut = true;
			if (status != NetConnectionStatus.Disconnected)
			{
				this.pendingDisconnect = Multiplayer.DisconnectType.LidgrenDisconnect_Unknown;
				return;
			}
			if (message == Multiplayer.kicked)
			{
				this.pendingDisconnect = Multiplayer.DisconnectType.Kicked;
				return;
			}
			this.pendingDisconnect = Multiplayer.DisconnectType.LidgrenTimeout;
		}

		// Token: 0x060020CC RID: 8396 RVA: 0x001714A0 File Offset: 0x0016F6A0
		protected virtual void sendMessageImpl(OutgoingMessage message)
		{
			NetOutgoingMessage sendMsg = this.client.CreateMessage();
			LidgrenMessageUtils.WriteMessage(message, sendMsg);
			this.client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger == null)
			{
				return;
			}
			bandwidthLogger.RecordBytesUp((long)sendMsg.LengthBytes);
		}

		// Token: 0x060020CD RID: 8397 RVA: 0x001714E8 File Offset: 0x0016F6E8
		public override void sendMessage(OutgoingMessage message)
		{
			base.OnSendingMessage(message, new Action<OutgoingMessage>(this.sendMessageImpl), delegate
			{
				this.sendMessageImpl(message);
			});
		}

		// Token: 0x060020CE RID: 8398 RVA: 0x00171534 File Offset: 0x0016F734
		private void parseDataMessageFromServer(NetIncomingMessage dataMsg)
		{
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesDown((long)dataMsg.LengthBytes);
			}
			using (IncomingMessage message = new IncomingMessage())
			{
				using (NetBufferReadStream stream = new NetBufferReadStream(dataMsg))
				{
					Action <>9__0;
					while ((long)dataMsg.LengthBits - dataMsg.Position >= 8L)
					{
						LidgrenMessageUtils.ReadStreamToMessage(stream, message);
						Action<IncomingMessage, Action<OutgoingMessage>, Action> onProcessingMessage = base.OnProcessingMessage;
						IncomingMessage message2 = message;
						Action<OutgoingMessage> arg = new Action<OutgoingMessage>(this.sendMessageImpl);
						Action arg2;
						if ((arg2 = <>9__0) == null)
						{
							arg2 = (<>9__0 = delegate()
							{
								this.processIncomingMessage(message);
							});
						}
						onProcessingMessage(message2, arg, arg2);
					}
				}
			}
		}

		// Token: 0x040013CB RID: 5067
		public string address;

		// Token: 0x040013CC RID: 5068
		public NetClient client;

		// Token: 0x040013CD RID: 5069
		private bool serverDiscovered;

		// Token: 0x040013CE RID: 5070
		private int maxRetryAttempts;

		// Token: 0x040013CF RID: 5071
		private int retryMs = 10000;

		// Token: 0x040013D0 RID: 5072
		private double lastAttemptMs;

		// Token: 0x040013D1 RID: 5073
		private int retryAttempts;

		// Token: 0x040013D2 RID: 5074
		private float lastLatencyMs;
	}
}
