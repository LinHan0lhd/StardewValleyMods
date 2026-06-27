using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam
{
	// Token: 0x02000167 RID: 359
	internal sealed class SteamNetServer : HookableServer
	{
		// Token: 0x06001B84 RID: 7044 RVA: 0x0013D669 File Offset: 0x0013B869
		public SteamNetServer(IGameServer gameServer) : base(gameServer)
		{
		}

		// Token: 0x06001B85 RID: 7045 RVA: 0x0013D6A4 File Offset: 0x0013B8A4
		private void UpdateLobbyPrivacy()
		{
			if (!this.Lobby.IsValid())
			{
				return;
			}
			ServerPrivacy privacy = this.Privacy;
			ELobbyType lobbyType;
			if (privacy != ServerPrivacy.FriendsOnly)
			{
				if (privacy != ServerPrivacy.Public)
				{
					lobbyType = ELobbyType.k_ELobbyTypePrivate;
				}
				else
				{
					lobbyType = ELobbyType.k_ELobbyTypePublic;
				}
			}
			else
			{
				lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
			}
			SteamMatchmaking.SetLobbyType(this.Lobby, lobbyType);
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x0013D6E8 File Offset: 0x0013B8E8
		private string ConnectionDataToId(ConnectionData connection)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);
			defaultInterpolatedStringHandler.AppendLiteral("SN_");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(connection.SteamId.m_SteamID);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<uint>(connection.Connection.m_HSteamNetConnection);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x0013D744 File Offset: 0x0013B944
		private ConnectionData IdToConnectionData(string connectionId)
		{
			if (connectionId.Length <= 3 || !connectionId.StartsWith("SN_"))
			{
				return null;
			}
			string steamConnectionString = connectionId.Substring(3);
			int underscoreIdx = steamConnectionString.IndexOf('_');
			if (underscoreIdx < 0)
			{
				return null;
			}
			CSteamID steamId = default(CSteamID);
			ulong rawSteamId = steamId.m_SteamID;
			uint connectionRaw = HSteamNetConnection.Invalid.m_HSteamNetConnection;
			try
			{
				rawSteamId = Convert.ToUInt64(steamConnectionString.Substring(0, underscoreIdx));
				connectionRaw = Convert.ToUInt32(steamConnectionString.Substring(underscoreIdx + 1));
			}
			catch (Exception)
			{
			}
			steamId = new CSteamID(rawSteamId);
			if (!steamId.IsValid())
			{
				return null;
			}
			HSteamNetConnection connection = HSteamNetConnection.Invalid;
			connection.m_HSteamNetConnection = connectionRaw;
			ConnectionData connectionData;
			if (!this.ConnectionDataMap.TryGetValue(connection, out connectionData))
			{
				return null;
			}
			if (connectionData.SteamId.m_SteamID != rawSteamId)
			{
				return null;
			}
			return connectionData;
		}

		// Token: 0x06001B88 RID: 7048 RVA: 0x0013D818 File Offset: 0x0013BA18
		public override bool isConnectionActive(string connectionId)
		{
			return this.IdToConnectionData(connectionId) != null;
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06001B89 RID: 7049 RVA: 0x0013D827 File Offset: 0x0013BA27
		public override int connectionsCount
		{
			get
			{
				Dictionary<HSteamNetConnection, ConnectionData> connectionDataMap = this.ConnectionDataMap;
				if (connectionDataMap == null)
				{
					return 0;
				}
				return connectionDataMap.Count;
			}
		}

		// Token: 0x06001B8A RID: 7050 RVA: 0x0013D83C File Offset: 0x0013BA3C
		public override string getUserId(long farmerId)
		{
			ConnectionData connectionData;
			if (!this.FarmerConnectionMap.TryGetValue(farmerId, out connectionData))
			{
				return null;
			}
			return connectionData.SteamId.m_SteamID.ToString();
		}

		// Token: 0x06001B8B RID: 7051 RVA: 0x0013D86C File Offset: 0x0013BA6C
		public override bool hasUserId(string userId)
		{
			CSteamID steamId = default(CSteamID);
			try
			{
				steamId = new CSteamID(Convert.ToUInt64(userId));
			}
			catch (Exception)
			{
			}
			if (!steamId.IsValid())
			{
				return false;
			}
			foreach (KeyValuePair<HSteamNetConnection, ConnectionData> connection in this.ConnectionDataMap)
			{
				if (connection.Value.SteamId.m_SteamID == steamId.m_SteamID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001B8C RID: 7052 RVA: 0x0013D908 File Offset: 0x0013BB08
		public override string getUserName(long farmerId)
		{
			ConnectionData connectionData;
			if (!this.FarmerConnectionMap.TryGetValue(farmerId, out connectionData))
			{
				return "";
			}
			string userName = SteamFriends.GetFriendPersonaName(connectionData.SteamId);
			if (string.IsNullOrWhiteSpace(userName) || userName == "[unknown]")
			{
				userName = connectionData.DisplayName;
			}
			connectionData.DisplayName = userName;
			return userName;
		}

		// Token: 0x06001B8D RID: 7053 RVA: 0x0013D95B File Offset: 0x0013BB5B
		public override void setPrivacy(ServerPrivacy privacy)
		{
			this.Privacy = privacy;
			this.UpdateLobbyPrivacy();
		}

		// Token: 0x06001B8E RID: 7054 RVA: 0x0013D96C File Offset: 0x0013BB6C
		public override bool connected()
		{
			return this.Lobby.IsValid() && this.Lobby.IsLobby() && this.ListenSocket != HSteamListenSocket.Invalid && this.JoiningGroup != HSteamNetPollGroup.Invalid && this.FarmhandGroup != HSteamNetPollGroup.Invalid;
		}

		// Token: 0x06001B8F RID: 7055 RVA: 0x0013D9CC File Offset: 0x0013BBCC
		private void OnConnecting(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamId.m_SteamID);
			defaultInterpolatedStringHandler.AppendLiteral(" connecting to server");
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			if (this.gameServer.isUserBanned(steamId.m_SteamID.ToString()))
			{
				IGameLogger log2 = Game1.log;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamId.m_SteamID);
				defaultInterpolatedStringHandler.AppendLiteral(" is banned");
				log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				this.ShutdownConnection(evt.m_hConn);
				return;
			}
			SteamFriends.RequestUserInformation(steamId, true);
			SteamNetworkingSockets.AcceptConnection(evt.m_hConn);
		}

		// Token: 0x06001B90 RID: 7056 RVA: 0x0013DA7C File Offset: 0x0013BC7C
		private void OnConnected(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamId.m_SteamID);
			defaultInterpolatedStringHandler.AppendLiteral(" connected to server");
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			string displayName = this.CachedDisplayNames.GetValueOrDefault(steamId);
			ConnectionData connectionData = new ConnectionData(evt.m_hConn, steamId, displayName);
			this.ConnectionDataMap[evt.m_hConn] = connectionData;
			SteamNetworkingSockets.SetConnectionPollGroup(evt.m_hConn, this.JoiningGroup);
			string connectionId = this.ConnectionDataToId(connectionData);
			this.onConnect(connectionId);
			this.gameServer.sendAvailableFarmhands("", connectionId, delegate(OutgoingMessage outgoing)
			{
				this.SendMessageToConnection(evt.m_hConn, outgoing);
			});
		}

		// Token: 0x06001B91 RID: 7057 RVA: 0x0013DB4C File Offset: 0x0013BD4C
		private void OnDisconnected(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
		{
			if (!steamId.IsValid())
			{
				return;
			}
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamId.m_SteamID);
			defaultInterpolatedStringHandler.AppendLiteral(" disconnected from server");
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			ConnectionData connectionData;
			if (!this.ConnectionDataMap.TryGetValue(evt.m_hConn, out connectionData))
			{
				SteamSocketUtils.CloseConnection(evt.m_hConn, null);
				return;
			}
			this.onDisconnect(this.ConnectionDataToId(connectionData));
			if (connectionData.Online)
			{
				this.playerDisconnected(connectionData.FarmerId);
			}
			this.ConnectionDataMap.Remove(evt.m_hConn);
			SteamSocketUtils.CloseConnection(evt.m_hConn, null);
		}

		// Token: 0x06001B92 RID: 7058 RVA: 0x0013DBF8 File Offset: 0x0013BDF8
		private void OnDisconnected(HSteamNetConnection connection)
		{
			SteamNetConnectionStatusChangedCallback_t fakeStatusChange = new SteamNetConnectionStatusChangedCallback_t
			{
				m_hConn = connection,
				m_eOldState = ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected
			};
			SteamNetworkingSockets.GetConnectionInfo(connection, out fakeStatusChange.m_info);
			this.OnDisconnected(fakeStatusChange, fakeStatusChange.m_info.m_identityRemote.GetSteamID());
		}

		// Token: 0x06001B93 RID: 7059 RVA: 0x0013DC48 File Offset: 0x0013BE48
		private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t evt)
		{
			switch (evt.m_info.m_eState)
			{
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
				this.OnConnecting(evt, evt.m_info.m_identityRemote.GetSteamID());
				return;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
				return;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
				this.OnConnected(evt, evt.m_info.m_identityRemote.GetSteamID());
				return;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
				this.OnDisconnected(evt, evt.m_info.m_identityRemote.GetSteamID());
				return;
			default:
				return;
			}
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x0013DCCC File Offset: 0x0013BECC
		private void OnLobbyChatUpdate(LobbyChatUpdate_t evt)
		{
			if (evt.m_ulSteamIDLobby != this.Lobby.m_SteamID)
			{
				return;
			}
			CSteamID memberId = new CSteamID(evt.m_ulSteamIDUserChanged);
			if ((evt.m_rgfChatMemberStateChange & 1U) != 0U)
			{
				this.CachedDisplayNames[memberId] = SteamFriends.GetFriendPersonaName(memberId);
				return;
			}
			if ((evt.m_rgfChatMemberStateChange & 30U) != 0U)
			{
				this.CachedDisplayNames.Remove(memberId);
			}
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x0013DD30 File Offset: 0x0013BF30
		private string OnLobbyCreatedHelper(LobbyCreated_t evt, bool ioFailure)
		{
			if (ioFailure)
			{
				return "IO Failure";
			}
			EResult eResult = evt.m_eResult;
			if (eResult <= EResult.k_EResultNoConnection)
			{
				if (eResult == EResult.k_EResultOK)
				{
					this.Lobby = new CSteamID(evt.m_ulSteamIDLobby);
					return null;
				}
				if (eResult == EResult.k_EResultNoConnection)
				{
					return "No connection to Steam";
				}
			}
			else
			{
				if (eResult == EResult.k_EResultAccessDenied)
				{
					return "Steam denied access";
				}
				if (eResult == EResult.k_EResultTimeout)
				{
					return "Steam timed out";
				}
				if (eResult == EResult.k_EResultLimitExceeded)
				{
					return "Too many Steam lobbies created";
				}
			}
			return "Unknown Steam failure";
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x0013DD9C File Offset: 0x0013BF9C
		private void OnLobbyCreated(LobbyCreated_t evt, bool ioFailure)
		{
			string lobbyError = this.OnLobbyCreatedHelper(evt, ioFailure);
			if (lobbyError == null)
			{
				SteamNetworkingConfigValue_t[] options = SteamSocketUtils.GetNetworkingOptions();
				this.ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
				this.JoiningGroup = SteamNetworkingSockets.CreatePollGroup();
				this.FarmhandGroup = SteamNetworkingSockets.CreatePollGroup();
				SteamMatchmaking.SetLobbyGameServer(this.Lobby, 0U, 0, SteamUser.GetSteamID());
				foreach (KeyValuePair<string, string> data in this.LobbyData)
				{
					SteamMatchmaking.SetLobbyData(this.Lobby, data.Key, data.Value);
				}
				SteamMatchmaking.SetLobbyJoinable(this.Lobby, true);
				this.UpdateLobbyPrivacy();
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Steam server successfully created lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.Lobby.m_SteamID);
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				StardewValley.Network.GameServer gameServerImpl = this.gameServer as StardewValley.Network.GameServer;
				if (gameServerImpl != null)
				{
					foreach (Server server in gameServerImpl.servers)
					{
						GalaxyNetServer galaxyServer = server as GalaxyNetServer;
						if (galaxyServer != null)
						{
							galaxyServer.setLobbyData("SteamLobbyId", this.Lobby.m_SteamID.ToString());
							Game1.log.Verbose("Updated Galaxy server with Steam lobby info");
							break;
						}
					}
				}
				return;
			}
			Game1.log.Verbose("Server failed to create lobby (" + lobbyError + ")");
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x0013DF3C File Offset: 0x0013C13C
		public override void initialize()
		{
			Game1.log.Verbose("Starting Steam server");
			this.LobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnLobbyCreated));
			this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(new Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate(this.OnSteamNetConnectionStatusChanged));
			this.LobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(new Callback<LobbyChatUpdate_t>.DispatchDelegate(this.OnLobbyChatUpdate));
			this.LobbyData = new Dictionary<string, string>();
			this.ConnectionDataMap = new Dictionary<HSteamNetConnection, ConnectionData>();
			this.FarmerConnectionMap = new Dictionary<long, ConnectionData>();
			this.CachedDisplayNames = new Dictionary<CSteamID, string>();
			this.RecentlyJoined = new HashSet<HSteamNetConnection>();
			this.LobbyData["protocolVersion"] = Multiplayer.protocolVersion;
			this.Lobby.Clear();
			this.ListenSocket = HSteamListenSocket.Invalid;
			this.JoiningGroup = HSteamNetPollGroup.Invalid;
			this.FarmhandGroup = HSteamNetPollGroup.Invalid;
			this.Privacy = Game1.options.serverPrivacy;
			SteamAPICall_t steamApiCall = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, Game1.multiplayer.playerLimit * 2);
			this.LobbyCreatedCallResult.Set(steamApiCall, null);
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x0013E048 File Offset: 0x0013C248
		public override void stopServer()
		{
			Game1.log.Verbose("Stopping Steam server");
			foreach (KeyValuePair<HSteamNetConnection, ConnectionData> connection in this.ConnectionDataMap)
			{
				this.ShutdownConnection(connection.Value.Connection);
			}
			if (this.Lobby.IsValid())
			{
				SteamMatchmaking.LeaveLobby(this.Lobby);
			}
			if (this.ListenSocket != HSteamListenSocket.Invalid)
			{
				SteamNetworkingSockets.CloseListenSocket(this.ListenSocket);
				this.ListenSocket = HSteamListenSocket.Invalid;
			}
			if (this.JoiningGroup != HSteamNetPollGroup.Invalid)
			{
				SteamNetworkingSockets.DestroyPollGroup(this.JoiningGroup);
				this.JoiningGroup = HSteamNetPollGroup.Invalid;
			}
			if (this.FarmhandGroup != HSteamNetPollGroup.Invalid)
			{
				SteamNetworkingSockets.DestroyPollGroup(this.FarmhandGroup);
				this.FarmhandGroup = HSteamNetPollGroup.Invalid;
			}
			Callback<SteamNetConnectionStatusChangedCallback_t> steamNetConnectionStatusChangedCallback = this.SteamNetConnectionStatusChangedCallback;
			if (steamNetConnectionStatusChangedCallback != null)
			{
				steamNetConnectionStatusChangedCallback.Unregister();
			}
			Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback = this.LobbyChatUpdateCallback;
			if (lobbyChatUpdateCallback == null)
			{
				return;
			}
			lobbyChatUpdateCallback.Unregister();
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x0013E16C File Offset: 0x0013C36C
		private void HandleFarmhandRequest(IncomingMessage message, ConnectionData connectionData)
		{
			NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
			long farmerId = farmer.Value.UniqueMultiplayerID;
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Server received farmhand request from ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(connectionData.SteamId.m_SteamID);
			defaultInterpolatedStringHandler.AppendLiteral(" for ");
			defaultInterpolatedStringHandler.AppendFormatted<long>(farmerId);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.gameServer.checkFarmhandRequest("", this.ConnectionDataToId(connectionData), farmer, delegate(OutgoingMessage outgoing)
			{
				this.SendMessageToConnection(connectionData.Connection, outgoing);
			}, delegate
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(29, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Server accepted ");
				defaultInterpolatedStringHandler2.AppendFormatted<ulong>(connectionData.SteamId.m_SteamID);
				defaultInterpolatedStringHandler2.AppendLiteral(" as farmhand ");
				defaultInterpolatedStringHandler2.AppendFormatted<long>(farmerId);
				log2.Verbose(defaultInterpolatedStringHandler2.ToStringAndClear());
				SteamNetworkingSockets.SetConnectionUserData(connectionData.Connection, farmerId);
				SteamNetworkingSockets.SetConnectionPollGroup(connectionData.Connection, this.FarmhandGroup);
				this.RecentlyJoined.Add(connectionData.Connection);
				connectionData.FarmerId = farmerId;
				connectionData.Online = true;
				this.FarmerConnectionMap[farmerId] = connectionData;
			});
		}

		// Token: 0x06001B9A RID: 7066 RVA: 0x0013E23C File Offset: 0x0013C43C
		private void PollJoiningMessages()
		{
			this.RecentlyJoined.Clear();
			int messageCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(this.JoiningGroup, this.Messages, 256);
			for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
			{
				IncomingMessage message = new IncomingMessage();
				HSteamNetConnection messageConnection;
				SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out messageConnection, this.bandwidthLogger);
				ConnectionData connectionData;
				if (!this.ConnectionDataMap.TryGetValue(messageConnection, out connectionData))
				{
					Game1.log.Warn("Tried to process multiplayer message from an invalid connection.");
					this.ShutdownConnection(messageConnection);
				}
				else
				{
					bool isRecentlyJoined = this.RecentlyJoined.Contains(messageConnection);
					if (connectionData.Online && !isRecentlyJoined)
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Online farmhand ");
						defaultInterpolatedStringHandler.AppendFormatted<long>(connectionData.FarmerId);
						defaultInterpolatedStringHandler.AppendLiteral(" is in the wrong poll group. Closing their connection.");
						log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						this.ShutdownConnection(messageConnection);
					}
					else
					{
						base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
						{
							this.SendMessageToConnection(messageConnection, outgoing);
						}, delegate
						{
							if (isRecentlyJoined)
							{
								this.gameServer.processIncomingMessage(message);
								return;
							}
							if (message.MessageType == 2)
							{
								this.HandleFarmhandRequest(message, connectionData);
							}
						});
					}
				}
			}
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x0013E398 File Offset: 0x0013C598
		private void PollFarmhandMessages()
		{
			int messageCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(this.FarmhandGroup, this.Messages, 256);
			for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
			{
				IncomingMessage message = new IncomingMessage();
				HSteamNetConnection messageConnection;
				SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out messageConnection, this.bandwidthLogger);
				ConnectionData connectionData;
				if (message.MessageType == 2)
				{
					Game1.log.Warn("Received farmhand request in the wrong poll group. Closing their connection.");
					this.ShutdownConnection(messageConnection);
				}
				else if (!this.ConnectionDataMap.TryGetValue(messageConnection, out connectionData))
				{
					Game1.log.Warn("Tried to process multiplayer message from an invalid connection.");
					this.ShutdownConnection(messageConnection);
				}
				else if (!connectionData.Online)
				{
					Game1.log.Warn("A non-farmhand connection is in the wrong poll group. Closing their connection.");
					this.ShutdownConnection(messageConnection);
				}
				else
				{
					base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
					{
						this.SendMessageToConnection(messageConnection, outgoing);
					}, delegate
					{
						this.gameServer.processIncomingMessage(message);
					});
				}
			}
		}

		// Token: 0x06001B9C RID: 7068 RVA: 0x0013E4B4 File Offset: 0x0013C6B4
		public override void receiveMessages()
		{
			if (!this.connected())
			{
				return;
			}
			this.PollJoiningMessages();
			this.PollFarmhandMessages();
			foreach (KeyValuePair<HSteamNetConnection, ConnectionData> connection in this.ConnectionDataMap)
			{
				SteamNetworkingSockets.FlushMessagesOnConnection(connection.Value.Connection);
			}
		}

		// Token: 0x06001B9D RID: 7069 RVA: 0x0013E528 File Offset: 0x0013C728
		private void SendMessageToConnection(HSteamNetConnection connection, OutgoingMessage message)
		{
			SteamSocketUtils.SendMessage(connection, message, this.bandwidthLogger, new Action<HSteamNetConnection>(this.OnDisconnected));
		}

		// Token: 0x06001B9E RID: 7070 RVA: 0x0013E544 File Offset: 0x0013C744
		public override void sendMessage(long peerId, OutgoingMessage message)
		{
			if (!this.connected())
			{
				return;
			}
			ConnectionData connectionData;
			if (!this.FarmerConnectionMap.TryGetValue(peerId, out connectionData))
			{
				return;
			}
			if (connectionData.Connection == HSteamNetConnection.Invalid)
			{
				return;
			}
			this.SendMessageToConnection(connectionData.Connection, message);
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x0013E58B File Offset: 0x0013C78B
		public override void setLobbyData(string key, string value)
		{
			if (this.LobbyData == null)
			{
				return;
			}
			this.LobbyData[key] = value;
			if (this.Lobby.IsValid())
			{
				SteamMatchmaking.SetLobbyData(this.Lobby, key, value);
			}
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x0013E5C0 File Offset: 0x0013C7C0
		public override void kick(long disconnectee)
		{
			base.kick(disconnectee);
			this.sendMessage(disconnectee, new OutgoingMessage(23, Game1.player, Array.Empty<object>()));
			ConnectionData connectionData;
			if (this.FarmerConnectionMap.TryGetValue(disconnectee, out connectionData))
			{
				this.ShutdownConnection(connectionData.Connection);
			}
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x0013E608 File Offset: 0x0013C808
		public override void playerDisconnected(long disconnectee)
		{
			ConnectionData connectionData;
			if (!this.FarmerConnectionMap.TryGetValue(disconnectee, out connectionData))
			{
				return;
			}
			base.playerDisconnected(disconnectee);
			this.FarmerConnectionMap.Remove(disconnectee);
		}

		// Token: 0x06001BA2 RID: 7074 RVA: 0x0013E63C File Offset: 0x0013C83C
		public override float getPingToClient(long farmerId)
		{
			ConnectionData connectionData;
			if (!this.FarmerConnectionMap.TryGetValue(farmerId, out connectionData))
			{
				return -1f;
			}
			SteamNetworkingQuickConnectionStatus status;
			SteamNetworkingSockets.GetQuickConnectionStatus(connectionData.Connection, out status);
			return (float)status.m_nPing;
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x0013E674 File Offset: 0x0013C874
		public override bool canOfferInvite()
		{
			return this.connected();
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x0013E67C File Offset: 0x0013C87C
		public override void offerInvite()
		{
			if (!this.connected())
			{
				return;
			}
			Program.sdk.Networking.ShowInviteDialog(this.Lobby);
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x0013E6A1 File Offset: 0x0013C8A1
		private void ShutdownConnection(HSteamNetConnection connection)
		{
			SteamSocketUtils.CloseConnection(connection, new Action<HSteamNetConnection>(this.OnDisconnected));
		}

		// Token: 0x040010AD RID: 4269
		private const int ServerBufferSize = 256;

		// Token: 0x040010AE RID: 4270
		private const int FlagsLobbyEntered = 1;

		// Token: 0x040010AF RID: 4271
		private const int FlagsLobbyLeft = 30;

		// Token: 0x040010B0 RID: 4272
		private CallResult<LobbyCreated_t> LobbyCreatedCallResult;

		// Token: 0x040010B1 RID: 4273
		private Callback<SteamNetConnectionStatusChangedCallback_t> SteamNetConnectionStatusChangedCallback;

		// Token: 0x040010B2 RID: 4274
		private Callback<LobbyChatUpdate_t> LobbyChatUpdateCallback;

		// Token: 0x040010B3 RID: 4275
		private Dictionary<string, string> LobbyData;

		// Token: 0x040010B4 RID: 4276
		private Dictionary<HSteamNetConnection, ConnectionData> ConnectionDataMap;

		// Token: 0x040010B5 RID: 4277
		private Dictionary<long, ConnectionData> FarmerConnectionMap;

		// Token: 0x040010B6 RID: 4278
		private Dictionary<CSteamID, string> CachedDisplayNames;

		// Token: 0x040010B7 RID: 4279
		private HashSet<HSteamNetConnection> RecentlyJoined;

		// Token: 0x040010B8 RID: 4280
		private readonly IntPtr[] Messages = new IntPtr[256];

		// Token: 0x040010B9 RID: 4281
		private CSteamID Lobby;

		// Token: 0x040010BA RID: 4282
		private HSteamListenSocket ListenSocket = HSteamListenSocket.Invalid;

		// Token: 0x040010BB RID: 4283
		private HSteamNetPollGroup JoiningGroup = HSteamNetPollGroup.Invalid;

		// Token: 0x040010BC RID: 4284
		private HSteamNetPollGroup FarmhandGroup = HSteamNetPollGroup.Invalid;

		// Token: 0x040010BD RID: 4285
		private ServerPrivacy Privacy;
	}
}
