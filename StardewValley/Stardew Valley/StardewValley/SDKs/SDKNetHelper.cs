using System;
using StardewValley.Network;

namespace StardewValley.SDKs
{
	// Token: 0x02000161 RID: 353
	public interface SDKNetHelper
	{
		// Token: 0x06001B14 RID: 6932
		string GetUserID();

		// Token: 0x06001B15 RID: 6933
		Client CreateClient(object lobby);

		// Token: 0x06001B16 RID: 6934
		Client GetRequestedClient();

		// Token: 0x06001B17 RID: 6935
		Server CreateServer(IGameServer gameServer);

		// Token: 0x06001B18 RID: 6936
		void AddLobbyUpdateListener(LobbyUpdateListener listener);

		// Token: 0x06001B19 RID: 6937
		void RemoveLobbyUpdateListener(LobbyUpdateListener listener);

		// Token: 0x06001B1A RID: 6938
		void RequestFriendLobbyData();

		// Token: 0x06001B1B RID: 6939
		string GetLobbyData(object lobby, string key);

		// Token: 0x06001B1C RID: 6940
		string GetLobbyOwnerName(object lobby);

		// Token: 0x06001B1D RID: 6941
		bool SupportsInviteCodes();

		// Token: 0x06001B1E RID: 6942
		object GetLobbyFromInviteCode(string inviteCode);

		// Token: 0x06001B1F RID: 6943
		void ShowInviteDialog(object lobby);

		// Token: 0x06001B20 RID: 6944
		void MutePlayer(string userId, bool mute);

		// Token: 0x06001B21 RID: 6945
		bool IsPlayerMuted(string userId);

		// Token: 0x06001B22 RID: 6946
		void ShowProfile(string userId);
	}
}
