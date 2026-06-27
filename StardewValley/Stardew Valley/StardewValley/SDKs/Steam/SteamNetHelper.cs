using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Galaxy.Api;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam
{
	// Token: 0x02000166 RID: 358
	internal sealed class SteamNetHelper : SDKNetHelper
	{
		// Token: 0x06001B6C RID: 7020 RVA: 0x0013D054 File Offset: 0x0013B254
		public SteamNetHelper()
		{
			this.LobbyUpdateListeners = new List<LobbyUpdateListener>();
			this.GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.OnGameLobbyJoinRequested));
			this.LobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
			this.RequestedLobby.Clear();
			this.FindLaunchLobby();
		}

		// Token: 0x06001B6D RID: 7021 RVA: 0x0013D0B4 File Offset: 0x0013B2B4
		~SteamNetHelper()
		{
			this.GameLobbyJoinRequestedCallback.Unregister();
			this.LobbyDataUpdateCallback.Unregister();
		}

		// Token: 0x06001B6E RID: 7022 RVA: 0x0013D0F0 File Offset: 0x0013B2F0
		private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t evt)
		{
			this.RequestJoinLobby(evt.m_steamIDLobby);
		}

		// Token: 0x06001B6F RID: 7023 RVA: 0x0013D100 File Offset: 0x0013B300
		private void OnLobbyDataUpdate(LobbyDataUpdate_t evt)
		{
			CSteamID steamLobby = new CSteamID(evt.m_ulSteamIDLobby);
			if (SteamMatchmaking.GetLobbyOwner(steamLobby) == SteamUser.GetSteamID())
			{
				return;
			}
			HybridLobby lobby = new HybridLobby(steamLobby);
			foreach (LobbyUpdateListener lobbyUpdateListener in this.LobbyUpdateListeners)
			{
				lobbyUpdateListener.OnLobbyUpdate(lobby);
			}
		}

		// Token: 0x06001B70 RID: 7024 RVA: 0x0013D180 File Offset: 0x0013B380
		private void FindLaunchLobby()
		{
			CSteamID launchLobby = default(CSteamID);
			string[] args = Environment.GetCommandLineArgs();
			for (int argIdx = 0; argIdx < args.Length - 1; argIdx++)
			{
				if (!(args[argIdx] != "+connect_lobby"))
				{
					launchLobby.Clear();
					try
					{
						launchLobby = new CSteamID(Convert.ToUInt64(args[argIdx + 1]));
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Found startup Steam lobby ");
						defaultInterpolatedStringHandler.AppendFormatted<ulong>(launchLobby.m_SteamID);
						log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
						this.RequestJoinLobby(launchLobby);
						break;
					}
					catch (Exception)
					{
						Game1.log.Verbose("Could not parse argument for +connect_lobby: " + args[argIdx + 1]);
					}
				}
			}
		}

		// Token: 0x06001B71 RID: 7025 RVA: 0x0013D240 File Offset: 0x0013B440
		private void RequestJoinLobby(CSteamID requestedLobby)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (requestedLobby.IsValid() && requestedLobby.IsLobby())
			{
				IGameLogger log = Game1.log;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Requesting to join Steam lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(requestedLobby.m_SteamID);
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				this.RequestedLobby = new HybridLobby(requestedLobby);
				Game1.multiplayer.inviteAccepted();
				return;
			}
			IGameLogger log2 = Game1.log;
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Denied request to join invalid Steam lobby ");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(requestedLobby.m_SteamID);
			log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06001B72 RID: 7026 RVA: 0x0013D2E0 File Offset: 0x0013B4E0
		public string GetUserID()
		{
			string result;
			try
			{
				result = GalaxyInstance.User().GetGalaxyID().ToUint64().ToString();
			}
			catch (Exception)
			{
				result = "";
			}
			return result;
		}

		// Token: 0x06001B73 RID: 7027 RVA: 0x0013D324 File Offset: 0x0013B524
		private Client CreateClientFromHybrid(HybridLobby lobby)
		{
			switch (lobby.LobbyType)
			{
			case LobbyConnectionType.Steam:
				return new SteamNetClient(new CSteamID(lobby.SteamId));
			case LobbyConnectionType.Galaxy:
				return new GalaxyNetClient(new GalaxyID(lobby.GalaxyId));
			case LobbyConnectionType.Hybrid:
				return new SteamNetClient(new GalaxyID(lobby.GalaxyId));
			default:
				return null;
			}
		}

		// Token: 0x06001B74 RID: 7028 RVA: 0x0013D384 File Offset: 0x0013B584
		private Client CreateClientHelper(HybridLobby lobby)
		{
			Client client = this.CreateClientFromHybrid(lobby);
			if (client == null)
			{
				return null;
			}
			return Game1.multiplayer.InitClient(client);
		}

		// Token: 0x06001B75 RID: 7029 RVA: 0x0013D3AC File Offset: 0x0013B5AC
		public Client CreateClient(object lobby)
		{
			if (lobby is HybridLobby)
			{
				HybridLobby hybridLobby = (HybridLobby)lobby;
				return this.CreateClientHelper(hybridLobby);
			}
			return null;
		}

		// Token: 0x06001B76 RID: 7030 RVA: 0x0013D3D3 File Offset: 0x0013B5D3
		public Client GetRequestedClient()
		{
			Client result = this.CreateClientHelper(this.RequestedLobby);
			this.RequestedLobby.Clear();
			return result;
		}

		// Token: 0x06001B77 RID: 7031 RVA: 0x0013D3EC File Offset: 0x0013B5EC
		public Server CreateSteamServer(IGameServer gameServer)
		{
			return Game1.multiplayer.InitServer(new SteamNetServer(gameServer));
		}

		// Token: 0x06001B78 RID: 7032 RVA: 0x0013D400 File Offset: 0x0013B600
		public Server CreateServer(IGameServer gameServer)
		{
			SteamHelper steamHelper = Program.sdk as SteamHelper;
			if (steamHelper != null && !steamHelper.GalaxyConnected)
			{
				Game1.log.Error("Could not create a Galaxy server: not logged on", null);
				return null;
			}
			return Game1.multiplayer.InitServer(new GalaxyNetServer(gameServer));
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x0013D445 File Offset: 0x0013B645
		public void AddLobbyUpdateListener(LobbyUpdateListener listener)
		{
			this.LobbyUpdateListeners.Add(listener);
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x0013D453 File Offset: 0x0013B653
		public void RemoveLobbyUpdateListener(LobbyUpdateListener listener)
		{
			this.LobbyUpdateListeners.Remove(listener);
		}

		// Token: 0x06001B7B RID: 7035 RVA: 0x0013D464 File Offset: 0x0013B664
		public void RequestFriendLobbyData()
		{
			int count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			for (int i = 0; i < count; i++)
			{
				CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
				if (!(friendId == SteamUser.GetSteamID()))
				{
					FriendGameInfo_t gameInfo;
					SteamFriends.GetFriendGamePlayed(friendId, out gameInfo);
					if (!(gameInfo.m_gameID.AppID() != SteamUtils.GetAppID()))
					{
						SteamMatchmaking.RequestLobbyData(gameInfo.m_steamIDLobby);
					}
				}
			}
		}

		// Token: 0x06001B7C RID: 7036 RVA: 0x0013D4C8 File Offset: 0x0013B6C8
		public string GetLobbyData(object lobby, string key)
		{
			if (!(lobby is HybridLobby))
			{
				return "";
			}
			HybridLobby hybridLobby = (HybridLobby)lobby;
			LobbyConnectionType lobbyType = hybridLobby.LobbyType;
			if (lobbyType != LobbyConnectionType.Steam)
			{
				if (lobbyType - LobbyConnectionType.Galaxy <= 1)
				{
					try
					{
						return GalaxyInstance.Matchmaking().GetLobbyData(new GalaxyID(hybridLobby.GalaxyId), key);
					}
					catch (Exception)
					{
						return "";
					}
				}
				return "";
			}
			return SteamMatchmaking.GetLobbyData(new CSteamID(hybridLobby.SteamId), key);
		}

		// Token: 0x06001B7D RID: 7037 RVA: 0x0013D550 File Offset: 0x0013B750
		public string GetLobbyOwnerName(object lobby)
		{
			if (lobby is HybridLobby)
			{
				HybridLobby hybridLobby = (HybridLobby)lobby;
				switch (hybridLobby.LobbyType)
				{
				case LobbyConnectionType.Steam:
					return SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyOwner(new CSteamID(hybridLobby.SteamId)));
				case LobbyConnectionType.Galaxy:
					try
					{
						GalaxyID galaxyOwner = GalaxyInstance.Matchmaking().GetLobbyOwner(new GalaxyID(hybridLobby.GalaxyId));
						return GalaxyInstance.Friends().GetFriendPersonaName(galaxyOwner);
					}
					catch (Exception)
					{
						return "";
					}
					break;
				case LobbyConnectionType.Hybrid:
					return GalaxyNetHelper.TryGetHostSteamDisplayName(new GalaxyID(hybridLobby.GalaxyId)) ?? "";
				}
				return "";
			}
			return null;
		}

		// Token: 0x06001B7E RID: 7038 RVA: 0x0013D604 File Offset: 0x0013B804
		public bool SupportsInviteCodes()
		{
			return true;
		}

		// Token: 0x06001B7F RID: 7039 RVA: 0x0013D608 File Offset: 0x0013B808
		public object GetLobbyFromInviteCode(string inviteCode)
		{
			GalaxyID galaxyLobby = GalaxyNetHelper.GetLobbyFromGalaxyInvite(inviteCode);
			if (!(galaxyLobby != null))
			{
				return null;
			}
			return new HybridLobby(galaxyLobby, inviteCode[0] == 'S');
		}

		// Token: 0x06001B80 RID: 7040 RVA: 0x0013D640 File Offset: 0x0013B840
		public void ShowInviteDialog(object lobby)
		{
			if (lobby is CSteamID)
			{
				CSteamID steamLobby = (CSteamID)lobby;
				SteamFriends.ActivateGameOverlayInviteDialog(steamLobby);
			}
		}

		// Token: 0x06001B81 RID: 7041 RVA: 0x0013D662 File Offset: 0x0013B862
		public void MutePlayer(string userId, bool mute)
		{
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x0013D664 File Offset: 0x0013B864
		public bool IsPlayerMuted(string userId)
		{
			return false;
		}

		// Token: 0x06001B83 RID: 7043 RVA: 0x0013D667 File Offset: 0x0013B867
		public void ShowProfile(string userId)
		{
		}

		// Token: 0x040010A9 RID: 4265
		private List<LobbyUpdateListener> LobbyUpdateListeners;

		// Token: 0x040010AA RID: 4266
		private readonly Callback<LobbyDataUpdate_t> LobbyDataUpdateCallback;

		// Token: 0x040010AB RID: 4267
		private readonly Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;

		// Token: 0x040010AC RID: 4268
		private HybridLobby RequestedLobby;
	}
}
