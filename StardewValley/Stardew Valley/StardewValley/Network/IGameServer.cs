using System;

namespace StardewValley.Network
{
	// Token: 0x020001CC RID: 460
	public interface IGameServer : IBandwidthMonitor
	{
		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06002055 RID: 8277
		int connectionsCount { get; }

		// Token: 0x06002056 RID: 8278
		string getInviteCode();

		// Token: 0x06002057 RID: 8279
		string getUserName(long farmerId);

		// Token: 0x06002058 RID: 8280
		void setPrivacy(ServerPrivacy privacy);

		// Token: 0x06002059 RID: 8281
		void stopServer();

		// Token: 0x0600205A RID: 8282
		void receiveMessages();

		// Token: 0x0600205B RID: 8283
		void sendMessage(long peerId, OutgoingMessage message);

		// Token: 0x0600205C RID: 8284
		bool canAcceptIPConnections();

		// Token: 0x0600205D RID: 8285
		bool canOfferInvite();

		// Token: 0x0600205E RID: 8286
		void offerInvite();

		// Token: 0x0600205F RID: 8287
		bool connected();

		// Token: 0x06002060 RID: 8288
		void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data);

		// Token: 0x06002061 RID: 8289
		void sendMessages();

		// Token: 0x06002062 RID: 8290
		void startServer();

		// Token: 0x06002063 RID: 8291
		void initializeHost();

		// Token: 0x06002064 RID: 8292
		void sendServerIntroduction(long peer);

		// Token: 0x06002065 RID: 8293
		void kick(long disconnectee);

		// Token: 0x06002066 RID: 8294
		string ban(long farmerId);

		// Token: 0x06002067 RID: 8295
		void playerDisconnected(long disconnectee);

		// Token: 0x06002068 RID: 8296
		bool isGameAvailable();

		// Token: 0x06002069 RID: 8297
		bool whenGameAvailable(Action action, Func<bool> customAvailabilityCheck = null);

		// Token: 0x0600206A RID: 8298
		void checkFarmhandRequest(string userId, string connectionId, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve);

		// Token: 0x0600206B RID: 8299
		void sendAvailableFarmhands(string userId, string connectionId, Action<OutgoingMessage> sendMessage);

		// Token: 0x0600206C RID: 8300
		void processIncomingMessage(IncomingMessage message);

		// Token: 0x0600206D RID: 8301
		void updateLobbyData();

		// Token: 0x0600206E RID: 8302
		float getPingToClient(long peer);

		// Token: 0x0600206F RID: 8303
		bool isUserBanned(string userID);

		// Token: 0x06002070 RID: 8304
		void onConnect(string connectionID);

		// Token: 0x06002071 RID: 8305
		void onDisconnect(string connectionID);

		// Token: 0x06002072 RID: 8306
		bool IsLocalMultiplayerInitiatedServer();
	}
}
