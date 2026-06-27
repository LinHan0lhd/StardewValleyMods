using System;
using System.Collections.Generic;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Internal;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy
{
	// Token: 0x0200016E RID: 366
	public class GalaxyNetHelper : SDKNetHelper
	{
		// Token: 0x06001BD9 RID: 7129 RVA: 0x0013F0E4 File Offset: 0x0013D2E4
		public GalaxyNetHelper()
		{
			this.lobbyRequested = this.getStartupLobby();
			this.lobbyJoinRequested = new GalaxyGameJoinRequestedListener(new Action<GalaxyID, string>(this.onLobbyJoinRequested));
			this.lobbyEntered = new GalaxyLobbyEnteredListener(new Action<GalaxyID, LobbyEnterResult>(this.onLobbyEntered));
			this.lobbyDataListener = new GalaxyLobbyDataListener(new Action<GalaxyID, GalaxyID>(this.onLobbyDataUpdated));
			this.richPresenceListener = new GalaxyRichPresenceListener(new Action<GalaxyID>(this.onRichPresenceUpdated));
			if (this.lobbyRequested != null)
			{
				Game1.multiplayer.inviteAccepted();
			}
		}

		// Token: 0x06001BDA RID: 7130 RVA: 0x0013F184 File Offset: 0x0013D384
		public static string TryGetHostSteamDisplayName(GalaxyID lobbyId)
		{
			string result;
			try
			{
				result = GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "HostDisplayName");
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06001BDB RID: 7131 RVA: 0x0013F1BC File Offset: 0x0013D3BC
		public virtual string GetUserID()
		{
			return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x0013F1D2 File Offset: 0x0013D3D2
		protected virtual Client createClient(GalaxyID lobby)
		{
			return Game1.multiplayer.InitClient(new GalaxyNetClient(lobby));
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x0013F1E4 File Offset: 0x0013D3E4
		public Client CreateClient(object lobby)
		{
			return this.createClient(new GalaxyID((ulong)lobby));
		}

		// Token: 0x06001BDE RID: 7134 RVA: 0x0013F1F7 File Offset: 0x0013D3F7
		public virtual Server CreateServer(IGameServer gameServer)
		{
			return Game1.multiplayer.InitServer(new GalaxyNetServer(gameServer));
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x0013F20C File Offset: 0x0013D40C
		protected GalaxyID parseConnectionString(string connectionString)
		{
			if (connectionString == null)
			{
				return null;
			}
			if (connectionString.StartsWith("-connect-lobby-"))
			{
				return new GalaxyID(Convert.ToUInt64(connectionString.Substring("-connect-lobby-".Length)));
			}
			if (connectionString.StartsWith("+connect_lobby "))
			{
				return new GalaxyID(Convert.ToUInt64(connectionString.Substring("+connect_lobby".Length + 1)));
			}
			return null;
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x0013F274 File Offset: 0x0013D474
		protected virtual GalaxyID getStartupLobby()
		{
			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-connect-lobby-"))
				{
					return this.parseConnectionString(args[i]);
				}
			}
			return null;
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x0013F2AF File Offset: 0x0013D4AF
		public Client GetRequestedClient()
		{
			if (this.lobbyRequested != null)
			{
				return this.createClient(this.lobbyRequested);
			}
			return null;
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x0013F2CD File Offset: 0x0013D4CD
		public void AddLobbyUpdateListener(LobbyUpdateListener listener)
		{
			this.lobbyUpdateListeners.Add(listener);
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x0013F2DB File Offset: 0x0013D4DB
		public void RemoveLobbyUpdateListener(LobbyUpdateListener listener)
		{
			this.lobbyUpdateListeners.Remove(listener);
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x0013F2EC File Offset: 0x0013D4EC
		public virtual void RequestFriendLobbyData()
		{
			uint count = GalaxyInstance.Friends().GetFriendCount();
			for (uint i = 0U; i < count; i += 1U)
			{
				GalaxyID friend = GalaxyInstance.Friends().GetFriendByIndex(i);
				GalaxyInstance.Friends().RequestRichPresence(friend);
			}
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x0013F328 File Offset: 0x0013D528
		private void onRichPresenceUpdated(GalaxyID userID)
		{
			GalaxyID lobby = this.parseConnectionString(GalaxyInstance.Friends().GetRichPresence("connect", userID));
			if (lobby != null)
			{
				GalaxyInstance.Matchmaking().RequestLobbyData(lobby);
			}
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x0013F360 File Offset: 0x0013D560
		private void onLobbyDataUpdated(GalaxyID lobbyID, GalaxyID memberID)
		{
			foreach (LobbyUpdateListener lobbyUpdateListener in this.lobbyUpdateListeners)
			{
				lobbyUpdateListener.OnLobbyUpdate(lobbyID.ToUint64());
			}
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x0013F3BC File Offset: 0x0013D5BC
		public virtual string GetLobbyData(object lobby, string key)
		{
			return GalaxyInstance.Matchmaking().GetLobbyData(new GalaxyID((ulong)lobby), key);
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x0013F3D4 File Offset: 0x0013D5D4
		public virtual string GetLobbyOwnerName(object lobbyId)
		{
			GalaxyID lobby = new GalaxyID((ulong)lobbyId);
			GalaxyID owner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobby);
			return GalaxyInstance.Friends().GetFriendPersonaName(owner);
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x0013F404 File Offset: 0x0013D604
		protected virtual void onLobbyEntered(GalaxyID lobby_id, LobbyEnterResult result)
		{
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x0013F406 File Offset: 0x0013D606
		private void onLobbyJoinRequested(GalaxyID userID, string connectionString)
		{
			this.lobbyRequested = this.parseConnectionString(connectionString);
			if (this.lobbyRequested != null)
			{
				Game1.multiplayer.inviteAccepted();
			}
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x0013F42D File Offset: 0x0013D62D
		public bool SupportsInviteCodes()
		{
			return true;
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x0013F430 File Offset: 0x0013D630
		public static GalaxyID GetLobbyFromGalaxyInvite(string inviteCode)
		{
			if (inviteCode.Length <= 1)
			{
				return null;
			}
			char c = inviteCode[0];
			if (c != 'G' && c != 'S')
			{
				return null;
			}
			ulong decoded;
			try
			{
				decoded = Base36.Decode(inviteCode.Substring(1));
			}
			catch (FormatException)
			{
				return null;
			}
			if (decoded == 0UL || decoded >> 56 != 0UL)
			{
				return null;
			}
			return GalaxyID.FromRealID(GalaxyID.IDType.ID_TYPE_LOBBY, decoded);
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x0013F498 File Offset: 0x0013D698
		public object GetLobbyFromInviteCode(string inviteCode)
		{
			GalaxyID lobbyID = GalaxyNetHelper.GetLobbyFromGalaxyInvite(inviteCode);
			if (lobbyID == null)
			{
				return null;
			}
			return lobbyID.ToUint64();
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x0013F4C2 File Offset: 0x0013D6C2
		public virtual void ShowInviteDialog(object lobby)
		{
			GalaxyInstance.Friends().ShowOverlayInviteDialog("-connect-lobby-" + Convert.ToString((ulong)lobby));
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x0013F4E3 File Offset: 0x0013D6E3
		public void MutePlayer(string userId, bool mute)
		{
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x0013F4E5 File Offset: 0x0013D6E5
		public bool IsPlayerMuted(string userId)
		{
			return false;
		}

		// Token: 0x06001BF1 RID: 7153 RVA: 0x0013F4E8 File Offset: 0x0013D6E8
		public void ShowProfile(string userId)
		{
		}

		// Token: 0x040010DB RID: 4315
		public const string GalaxyConnectionStringPrefix = "-connect-lobby-";

		// Token: 0x040010DC RID: 4316
		public const string SteamConnectionStringPrefix = "+connect_lobby";

		// Token: 0x040010DD RID: 4317
		public const char GalaxyInvitePrefix = 'G';

		// Token: 0x040010DE RID: 4318
		public const char SteamInvitePrefix = 'S';

		// Token: 0x040010DF RID: 4319
		protected GalaxyID lobbyRequested;

		// Token: 0x040010E0 RID: 4320
		private GalaxyLobbyEnteredListener lobbyEntered;

		// Token: 0x040010E1 RID: 4321
		private GalaxyGameJoinRequestedListener lobbyJoinRequested;

		// Token: 0x040010E2 RID: 4322
		private GalaxyLobbyDataListener lobbyDataListener;

		// Token: 0x040010E3 RID: 4323
		private GalaxyRichPresenceListener richPresenceListener;

		// Token: 0x040010E4 RID: 4324
		private List<LobbyUpdateListener> lobbyUpdateListeners = new List<LobbyUpdateListener>();
	}
}
