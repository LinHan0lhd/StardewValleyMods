using System;
using System.Runtime.CompilerServices;
using Galaxy.Api;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam
{
	// Token: 0x02000165 RID: 357
	internal sealed class SteamNetClient : HookableClient
	{
		// Token: 0x06001B55 RID: 6997 RVA: 0x0013C584 File Offset: 0x0013A784
		public SteamNetClient(GalaxyID galaxyLobby)
		{
			this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(new Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate(this.OnSteamNetConnectionStatusChanged));
			this.GalaxyLobby = galaxyLobby;
		}

		// Token: 0x06001B56 RID: 6998 RVA: 0x0013C5D0 File Offset: 0x0013A7D0
		public SteamNetClient(CSteamID steamLobby)
		{
			this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(new Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate(this.OnSteamNetConnectionStatusChanged));
			this.GalaxyLobby = null;
			this.SteamLobby = steamLobby;
		}

		// Token: 0x06001B57 RID: 6999 RVA: 0x0013C624 File Offset: 0x0013A824
		~SteamNetClient()
		{
			this.CleanupLobbyDataRetrieve();
			this.SteamNetConnectionStatusChangedCallback.Unregister();
		}

		// Token: 0x06001B58 RID: 7000 RVA: 0x0013C65C File Offset: 0x0013A85C
		private void OnDisconnected(HSteamNetConnection connection)
		{
			if (connection == HSteamNetConnection.Invalid || connection != this.Connection)
			{
				return;
			}
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Client disconnected from server ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.HostId.m_SteamID);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.timedOut = true;
			this.pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
			SteamSocketUtils.CloseConnection(this.Connection, null);
			this.Connection = HSteamNetConnection.Invalid;
		}

		// Token: 0x06001B59 RID: 7001 RVA: 0x0013C6E4 File Offset: 0x0013A8E4
		private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t evt)
		{
			if (evt.m_hConn != this.Connection)
			{
				return;
			}
			switch (evt.m_info.m_eState)
			{
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Client connecting to server ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.HostId.m_SteamID);
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
				return;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Client connected to server ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.HostId.m_SteamID);
				log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
				this.OnDisconnected(evt.m_hConn);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001B5A RID: 7002 RVA: 0x0013C7AF File Offset: 0x0013A9AF
		public override string getUserID()
		{
			return Program.sdk.Networking.GetUserID();
		}

		// Token: 0x06001B5B RID: 7003 RVA: 0x0013C7C0 File Offset: 0x0013A9C0
		protected override string getHostUserName()
		{
			if (!this.HostId.IsValid())
			{
				return "???";
			}
			string userName = SteamFriends.GetFriendPersonaName(this.HostId);
			if (string.IsNullOrWhiteSpace(userName) || userName == "[unknown]")
			{
				userName = this.CachedHostName;
			}
			this.CachedHostName = userName;
			return userName;
		}

		// Token: 0x06001B5C RID: 7004 RVA: 0x0013C810 File Offset: 0x0013AA10
		private void ConnectToHost()
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Found Steam host ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.HostId.m_SteamID);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			SteamNetworkingIdentity identity = default(SteamNetworkingIdentity);
			identity.SetSteamID(this.HostId);
			SteamNetworkingConfigValue_t[] options = SteamSocketUtils.GetNetworkingOptions();
			this.Connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, options.Length, options);
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x0013C884 File Offset: 0x0013AA84
		private string TryConnectSteam(LobbyEnter_t evt, bool ioFailure, out string errorTranslationKey)
		{
			this.SteamLobby.Clear();
			if (ioFailure)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
				return "IO Failure";
			}
			if (evt.m_EChatRoomEnterResponse != 1U)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to join: ");
				defaultInterpolatedStringHandler.AppendFormatted<EChatRoomEnterResponse>((EChatRoomEnterResponse)evt.m_EChatRoomEnterResponse);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			this.SteamLobby = new CSteamID(evt.m_ulSteamIDLobby);
			string protocolVersion = SteamMatchmaking.GetLobbyData(this.SteamLobby, "protocolVersion");
			if (protocolVersion != Multiplayer.protocolVersion)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
				if (!(protocolVersion == ""))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Protocol (");
					defaultInterpolatedStringHandler.AppendFormatted(protocolVersion);
					defaultInterpolatedStringHandler.AppendLiteral(") does not match our own (");
					defaultInterpolatedStringHandler.AppendFormatted(Multiplayer.protocolVersion);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
				return "Missing protocol version data";
			}
			else
			{
				uint num;
				ushort num2;
				CSteamID hostId;
				if (!SteamMatchmaking.GetLobbyGameServer(this.SteamLobby, out num, out num2, out hostId))
				{
					errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
					return "Missing game server data";
				}
				if (!hostId.IsValid())
				{
					errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
					return "Invalid host ID";
				}
				this.CachedHostName = SteamFriends.GetFriendPersonaName(this.HostId);
				SteamFriends.RequestUserInformation(hostId, true);
				this.HostId = hostId;
				this.ConnectToHost();
				errorTranslationKey = null;
				return null;
			}
		}

		// Token: 0x06001B5E RID: 7006 RVA: 0x0013C9DC File Offset: 0x0013ABDC
		private void OnLobbyEnter(LobbyEnter_t evt, bool ioFailure)
		{
			if (evt.m_ulSteamIDLobby != this.SteamLobby.m_SteamID)
			{
				return;
			}
			string errorTranslationKey;
			string errorMsg = this.TryConnectSteam(evt, ioFailure, out errorTranslationKey);
			if (errorMsg != null)
			{
				this.connectionMessage = Game1.content.LoadString(errorTranslationKey);
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error joining via Steam lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(evt.m_ulSteamIDLobby);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(errorMsg);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.SteamLobbyEnterCallResult = null;
		}

		// Token: 0x06001B5F RID: 7007 RVA: 0x0013CA78 File Offset: 0x0013AC78
		private void ConnectImplSteam()
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Resolving Steam host via Steam lobby ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.SteamLobby.m_SteamID);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.SteamLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(new CallResult<LobbyEnter_t>.APIDispatchDelegate(this.OnLobbyEnter));
			SteamAPICall_t steamApiCall = SteamMatchmaking.JoinLobby(this.SteamLobby);
			this.SteamLobbyEnterCallResult.Set(steamApiCall, null);
		}

		// Token: 0x06001B60 RID: 7008 RVA: 0x0013CAEE File Offset: 0x0013ACEE
		private void CleanupLobbyDataRetrieve()
		{
			GalaxyLobbyDataRetrieveListener galaxyLobbyDataRetrieveCallback = this.GalaxyLobbyDataRetrieveCallback;
			if (galaxyLobbyDataRetrieveCallback != null)
			{
				galaxyLobbyDataRetrieveCallback.Dispose();
			}
			this.GalaxyLobbyDataRetrieveCallback = null;
		}

		// Token: 0x06001B61 RID: 7009 RVA: 0x0013CB08 File Offset: 0x0013AD08
		private string TryConnectGalaxy(GalaxyID lobbyId, out string errorTranslationKey)
		{
			string steamLobbyIdString;
			try
			{
				steamLobbyIdString = GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "SteamLobbyId");
			}
			catch (Exception)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
				return "Failed to get Steam lobby ID";
			}
			if (string.IsNullOrEmpty(steamLobbyIdString))
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
				return "Missing Steam lobby ID";
			}
			string protocolVersion;
			try
			{
				protocolVersion = GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "protocolVersion");
			}
			catch (Exception)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
				return "Failed to get protocol version";
			}
			if (protocolVersion != Multiplayer.protocolVersion)
			{
				errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
				if (!string.IsNullOrEmpty(protocolVersion))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Protocol (");
					defaultInterpolatedStringHandler.AppendFormatted(protocolVersion);
					defaultInterpolatedStringHandler.AppendLiteral(") does not match our own (");
					defaultInterpolatedStringHandler.AppendFormatted(Multiplayer.protocolVersion);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
				return "Missing protocol version data";
			}
			else
			{
				CSteamID steamLobbyId = default(CSteamID);
				try
				{
					steamLobbyId = new CSteamID(Convert.ToUInt64(steamLobbyIdString));
				}
				catch (Exception)
				{
				}
				if (!steamLobbyId.IsValid())
				{
					errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
					return "Invalid lobby ID";
				}
				this.SteamLobby = steamLobbyId;
				this.GalaxyLobby = null;
				errorTranslationKey = null;
				this.ConnectImplSteam();
				return null;
			}
			string result;
			return result;
		}

		// Token: 0x06001B62 RID: 7010 RVA: 0x0013CC54 File Offset: 0x0013AE54
		private void OnLobbyDataRetrieveSuccess(GalaxyID lobbyId)
		{
			if (lobbyId != null && lobbyId != this.GalaxyLobby)
			{
				return;
			}
			string errorTranslationKey;
			string errorMsg = this.TryConnectGalaxy(lobbyId, out errorTranslationKey);
			if (errorMsg != null)
			{
				this.connectionMessage = Game1.content.LoadString(errorTranslationKey);
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error joining via Galaxy lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyId);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted(errorMsg);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			else
			{
				try
				{
					GalaxyInstance.Matchmaking().LeaveLobby(lobbyId);
				}
				catch (Exception)
				{
				}
			}
			this.CleanupLobbyDataRetrieve();
		}

		// Token: 0x06001B63 RID: 7011 RVA: 0x0013CD10 File Offset: 0x0013AF10
		private void OnLobbyDataRetrieveFailure(GalaxyID lobbyId, ILobbyDataRetrieveListener.FailureReason failureReason)
		{
			if (lobbyId != null && lobbyId != this.GalaxyLobby)
			{
				return;
			}
			this.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Steam client failed to get data from Galaxy lobby ");
			defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyId);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.CleanupLobbyDataRetrieve();
		}

		// Token: 0x06001B64 RID: 7012 RVA: 0x0013CD80 File Offset: 0x0013AF80
		private void ConnectImplGalaxy()
		{
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Resolving Steam lobby via Galaxy lobby ");
			defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(this.GalaxyLobby);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.GalaxyLobbyDataRetrieveCallback = new GalaxyLobbyDataRetrieveListener(new Action<GalaxyID>(this.OnLobbyDataRetrieveSuccess), new Action<GalaxyID, ILobbyDataRetrieveListener.FailureReason>(this.OnLobbyDataRetrieveFailure));
			try
			{
				GalaxyInstance.Matchmaking().RequestLobbyData(this.GalaxyLobby, this.GalaxyLobbyDataRetrieveCallback);
			}
			catch (Exception e)
			{
				this.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
				Game1.log.Error("Steam client Galaxy RequestLobbyData failed with an exception:", e);
				this.CleanupLobbyDataRetrieve();
			}
		}

		// Token: 0x06001B65 RID: 7013 RVA: 0x0013CE3C File Offset: 0x0013B03C
		protected override void connectImpl()
		{
			if (this.GalaxyLobby == null)
			{
				this.ConnectImplSteam();
				return;
			}
			this.ConnectImplGalaxy();
		}

		// Token: 0x06001B66 RID: 7014 RVA: 0x0013CE5C File Offset: 0x0013B05C
		public override void disconnect(bool neatly = true)
		{
			if (this.SteamLobby.IsValid())
			{
				SteamMatchmaking.LeaveLobby(this.SteamLobby);
				this.SteamLobby.Clear();
			}
			IGameLogger log = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Client disconnecting from server ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(this.HostId.m_SteamID);
			log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			this.connectionMessage = null;
			this.ShutdownConnection();
		}

		// Token: 0x06001B67 RID: 7015 RVA: 0x0013CED4 File Offset: 0x0013B0D4
		protected override void receiveMessagesImpl()
		{
			if (this.Connection == HSteamNetConnection.Invalid)
			{
				return;
			}
			int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(this.Connection, this.Messages, 256);
			for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
			{
				IncomingMessage message = new IncomingMessage();
				HSteamNetConnection hsteamNetConnection;
				SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out hsteamNetConnection, this.bandwidthLogger);
				base.OnProcessingMessage(message, new Action<OutgoingMessage>(this.SendMessageImpl), delegate
				{
					this.processIncomingMessage(message);
				});
			}
			SteamNetworkingSockets.FlushMessagesOnConnection(this.Connection);
		}

		// Token: 0x06001B68 RID: 7016 RVA: 0x0013CF80 File Offset: 0x0013B180
		public override void sendMessage(OutgoingMessage message)
		{
			base.OnSendingMessage(message, new Action<OutgoingMessage>(this.SendMessageImpl), delegate
			{
				this.SendMessageImpl(message);
			});
		}

		// Token: 0x06001B69 RID: 7017 RVA: 0x0013CFCC File Offset: 0x0013B1CC
		public override float GetPingToHost()
		{
			if (this.Connection == HSteamNetConnection.Invalid)
			{
				return -1f;
			}
			SteamNetworkingQuickConnectionStatus status;
			SteamNetworkingSockets.GetQuickConnectionStatus(this.Connection, out status);
			return (float)status.m_nPing;
		}

		// Token: 0x06001B6A RID: 7018 RVA: 0x0013D006 File Offset: 0x0013B206
		private void SendMessageImpl(OutgoingMessage message)
		{
			if (this.Connection == HSteamNetConnection.Invalid)
			{
				return;
			}
			SteamSocketUtils.SendMessage(this.Connection, message, this.bandwidthLogger, new Action<HSteamNetConnection>(this.OnDisconnected));
		}

		// Token: 0x06001B6B RID: 7019 RVA: 0x0013D039 File Offset: 0x0013B239
		private void ShutdownConnection()
		{
			SteamSocketUtils.CloseConnection(this.Connection, new Action<HSteamNetConnection>(this.OnDisconnected));
		}

		// Token: 0x0400109F RID: 4255
		private const int ClientBufferSize = 256;

		// Token: 0x040010A0 RID: 4256
		private CallResult<LobbyEnter_t> SteamLobbyEnterCallResult;

		// Token: 0x040010A1 RID: 4257
		private readonly Callback<SteamNetConnectionStatusChangedCallback_t> SteamNetConnectionStatusChangedCallback;

		// Token: 0x040010A2 RID: 4258
		private GalaxyLobbyDataRetrieveListener GalaxyLobbyDataRetrieveCallback;

		// Token: 0x040010A3 RID: 4259
		private readonly IntPtr[] Messages = new IntPtr[256];

		// Token: 0x040010A4 RID: 4260
		private GalaxyID GalaxyLobby;

		// Token: 0x040010A5 RID: 4261
		private CSteamID SteamLobby;

		// Token: 0x040010A6 RID: 4262
		private CSteamID HostId;

		// Token: 0x040010A7 RID: 4263
		private string CachedHostName;

		// Token: 0x040010A8 RID: 4264
		private HSteamNetConnection Connection = HSteamNetConnection.Invalid;
	}
}
