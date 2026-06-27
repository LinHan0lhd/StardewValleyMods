using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network.Dedicated;
using StardewValley.SaveSerialization;
using StardewValley.SDKs.Steam;

namespace StardewValley.Network
{
	// Token: 0x020001CD RID: 461
	public class GameServer : IGameServer, IBandwidthMonitor
	{
		// Token: 0x06002073 RID: 8307 RVA: 0x0016F29C File Offset: 0x0016D49C
		public GameServer(bool local_multiplayer = false)
		{
			if (Game1.options != null)
			{
				Game1.options.enableServer = true;
			}
			this.servers.Add(Game1.multiplayer.InitServer(new LidgrenServer(this)));
			this._isLocalMultiplayerInitiatedServer = local_multiplayer;
			if (!this._isLocalMultiplayerInitiatedServer && Program.sdk.Networking != null)
			{
				SteamNetHelper steamNetworking = Program.sdk.Networking as SteamNetHelper;
				if (steamNetworking != null)
				{
					this.servers.Add(steamNetworking.CreateSteamServer(this));
				}
				Server sdkServer = Program.sdk.Networking.CreateServer(this);
				if (sdkServer != null)
				{
					this.servers.Add(sdkServer);
				}
			}
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06002074 RID: 8308 RVA: 0x0016F373 File Offset: 0x0016D573
		public int connectionsCount
		{
			get
			{
				return this.servers.Sum((Server s) => s.connectionsCount);
			}
		}

		// Token: 0x06002075 RID: 8309 RVA: 0x0016F3A0 File Offset: 0x0016D5A0
		public bool isConnectionActive(string connectionId)
		{
			using (List<Server>.Enumerator enumerator = this.servers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.isConnectionActive(connectionId))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002076 RID: 8310 RVA: 0x0016F3FC File Offset: 0x0016D5FC
		public virtual void onConnect(string connectionID)
		{
			this.UpdateLocalOnlyFlag();
		}

		// Token: 0x06002077 RID: 8311 RVA: 0x0016F404 File Offset: 0x0016D604
		public virtual void onDisconnect(string connectionID)
		{
			this.UpdateLocalOnlyFlag();
		}

		// Token: 0x06002078 RID: 8312 RVA: 0x0016F40C File Offset: 0x0016D60C
		public bool IsLocalMultiplayerInitiatedServer()
		{
			return this._isLocalMultiplayerInitiatedServer;
		}

		// Token: 0x06002079 RID: 8313 RVA: 0x0016F414 File Offset: 0x0016D614
		public virtual void UpdateLocalOnlyFlag()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			bool local_only = true;
			HashSet<long> local_clients = new HashSet<long>();
			GameRunner.instance.ExecuteForInstances(delegate(Game1 instance)
			{
				Client client = Game1.client;
				if (client == null)
				{
					FarmhandMenu farmhandMenu = Game1.activeClickableMenu as FarmhandMenu;
					if (farmhandMenu != null)
					{
						client = farmhandMenu.client;
					}
				}
				LidgrenClient lidgrenClient = client as LidgrenClient;
				if (lidgrenClient != null)
				{
					local_clients.Add(lidgrenClient.client.UniqueIdentifier);
				}
			});
			foreach (Server server in this.servers)
			{
				LidgrenServer lidgren_server = server as LidgrenServer;
				if (lidgren_server != null)
				{
					using (List<NetConnection>.Enumerator enumerator2 = lidgren_server.server.Connections.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							NetConnection connection = enumerator2.Current;
							if (!local_clients.Contains(connection.RemoteUniqueIdentifier))
							{
								local_only = false;
								break;
							}
						}
						goto IL_B4;
					}
					goto IL_A7;
				}
				goto IL_A7;
				IL_B4:
				if (!local_only)
				{
					break;
				}
				continue;
				IL_A7:
				if (server.connectionsCount > 0)
				{
					local_only = false;
					break;
				}
				goto IL_B4;
			}
			if (Game1.hasLocalClientsOnly != local_only)
			{
				Game1.hasLocalClientsOnly = local_only;
				if (Game1.hasLocalClientsOnly)
				{
					Game1.log.Verbose("Game has only local clients.");
					return;
				}
				Game1.log.Verbose("Game has remote clients.");
			}
		}

		// Token: 0x0600207A RID: 8314 RVA: 0x0016F544 File Offset: 0x0016D744
		public string getInviteCode()
		{
			foreach (Server server in this.servers)
			{
				string code = server.getInviteCode();
				if (code != null)
				{
					return code;
				}
			}
			return null;
		}

		// Token: 0x0600207B RID: 8315 RVA: 0x0016F5A0 File Offset: 0x0016D7A0
		public string getUserName(long farmerId)
		{
			foreach (Server server in this.servers)
			{
				string name = server.getUserName(farmerId);
				if (name != null)
				{
					return name;
				}
			}
			return null;
		}

		// Token: 0x0600207C RID: 8316 RVA: 0x0016F5FC File Offset: 0x0016D7FC
		public float getPingToClient(long farmerId)
		{
			foreach (Server server in this.servers)
			{
				if (server.getPingToClient(farmerId) != -1f)
				{
					return server.getPingToClient(farmerId);
				}
			}
			return -1f;
		}

		// Token: 0x0600207D RID: 8317 RVA: 0x0016F668 File Offset: 0x0016D868
		protected void initialize()
		{
			foreach (Server server in this.servers)
			{
				server.initialize();
			}
			this.whenGameAvailable(new Action(this.updateLobbyData), null);
		}

		// Token: 0x0600207E RID: 8318 RVA: 0x0016F6D0 File Offset: 0x0016D8D0
		public void setPrivacy(ServerPrivacy privacy)
		{
			foreach (Server server in this.servers)
			{
				server.setPrivacy(privacy);
			}
			if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
			{
				Game1.netWorldState.Value.ServerPrivacy = privacy;
			}
		}

		// Token: 0x0600207F RID: 8319 RVA: 0x0016F74C File Offset: 0x0016D94C
		public void stopServer()
		{
			if (Game1.chatBox != null)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_DisablingServer"));
			}
			foreach (Server server in this.servers)
			{
				server.stopServer();
			}
		}

		// Token: 0x06002080 RID: 8320 RVA: 0x0016F7BC File Offset: 0x0016D9BC
		public void receiveMessages()
		{
			foreach (Server server in this.servers)
			{
				server.receiveMessages();
			}
			this.completedPendingActions.Clear();
			foreach (Action action in this.pendingGameAvailableActions.Keys)
			{
				if (this.pendingGameAvailableActions[action]())
				{
					action();
					this.completedPendingActions.Add(action);
				}
			}
			foreach (Action action2 in this.completedPendingActions)
			{
				this.pendingGameAvailableActions.Remove(action2);
			}
			this.completedPendingActions.Clear();
			if (Game1.chatBox != null)
			{
				bool any_server_connected = this.anyServerConnected();
				if (this._wasConnected != any_server_connected)
				{
					this._wasConnected = any_server_connected;
					if (this._wasConnected)
					{
						Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_StartingServer"));
					}
				}
			}
		}

		// Token: 0x06002081 RID: 8321 RVA: 0x0016F914 File Offset: 0x0016DB14
		public void sendMessage(long peerId, OutgoingMessage message)
		{
			foreach (Server server in this.servers)
			{
				server.sendMessage(peerId, message);
			}
		}

		// Token: 0x06002082 RID: 8322 RVA: 0x0016F968 File Offset: 0x0016DB68
		public bool canAcceptIPConnections()
		{
			return (from s in this.servers
			select s.canAcceptIPConnections()).Aggregate(false, (bool a, bool b) => a || b);
		}

		// Token: 0x06002083 RID: 8323 RVA: 0x0016F9C4 File Offset: 0x0016DBC4
		public bool canOfferInvite()
		{
			return (from s in this.servers
			select s.canOfferInvite()).Aggregate(false, (bool a, bool b) => a || b);
		}

		// Token: 0x06002084 RID: 8324 RVA: 0x0016FA20 File Offset: 0x0016DC20
		public void offerInvite()
		{
			foreach (Server s in this.servers)
			{
				if (s.canOfferInvite())
				{
					s.offerInvite();
				}
			}
		}

		// Token: 0x06002085 RID: 8325 RVA: 0x0016FA7C File Offset: 0x0016DC7C
		public bool anyServerConnected()
		{
			using (List<Server>.Enumerator enumerator = this.servers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.connected())
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002086 RID: 8326 RVA: 0x0016FAD8 File Offset: 0x0016DCD8
		public bool connected()
		{
			using (List<Server>.Enumerator enumerator = this.servers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.connected())
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002087 RID: 8327 RVA: 0x0016FB34 File Offset: 0x0016DD34
		public void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data)
		{
			this.sendMessage(peerId, new OutgoingMessage(messageType, sourceFarmer, data));
		}

		// Token: 0x06002088 RID: 8328 RVA: 0x0016FB48 File Offset: 0x0016DD48
		public void sendMessages()
		{
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				foreach (OutgoingMessage message in farmer.messageQueue)
				{
					this.sendMessage(farmer.UniqueMultiplayerID, message);
				}
				farmer.messageQueue.Clear();
			}
		}

		// Token: 0x06002089 RID: 8329 RVA: 0x0016FBE0 File Offset: 0x0016DDE0
		public void startServer()
		{
			this._wasConnected = false;
			Game1.log.Verbose("Starting server. Protocol version: " + Multiplayer.protocolVersion);
			this.initialize();
			if (Game1.netWorldState == null)
			{
				Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
			}
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			Game1.netWorldState.Value.UpdateFromGame1();
		}

		// Token: 0x0600208A RID: 8330 RVA: 0x0016FC50 File Offset: 0x0016DE50
		public void initializeHost()
		{
			if (Game1.serverHost == null)
			{
				Game1.serverHost = new NetFarmerRoot();
			}
			Game1.serverHost.Value = Game1.player;
			using (List<Server>.Enumerator enumerator = this.servers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.PopulatePlatformData(Game1.player))
					{
						break;
					}
				}
			}
			Game1.serverHost.MarkClean();
			Game1.serverHost.Clock.InterpolationTicks = Game1.multiplayer.defaultInterpolationTicks;
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x0016FCF4 File Offset: 0x0016DEF4
		public void sendServerIntroduction(long peer)
		{
			this.sendMessage(peer, new OutgoingMessage(1, Game1.serverHost.Value, new object[]
			{
				Game1.multiplayer.writeObjectFullBytes<Farmer>(Game1.serverHost, new long?(peer)),
				Game1.multiplayer.writeObjectFullBytes<FarmerTeam>(Game1.player.teamRoot, new long?(peer)),
				Game1.multiplayer.writeObjectFullBytes<NetWorldState>(Game1.netWorldState, new long?(peer))
			}));
			foreach (KeyValuePair<long, NetRoot<Farmer>> r in Game1.otherFarmers.Roots)
			{
				if (r.Key != Game1.player.UniqueMultiplayerID && r.Key != peer)
				{
					this.sendMessage(peer, new OutgoingMessage(2, r.Value.Value, new object[]
					{
						this.getUserName(r.Value.Value.UniqueMultiplayerID),
						Game1.multiplayer.writeObjectFullBytes<Farmer>(r.Value, new long?(peer))
					}));
				}
			}
		}

		// Token: 0x0600208C RID: 8332 RVA: 0x0016FE24 File Offset: 0x0016E024
		public void kick(long disconnectee)
		{
			foreach (Server server in this.servers)
			{
				server.kick(disconnectee);
			}
		}

		// Token: 0x0600208D RID: 8333 RVA: 0x0016FE78 File Offset: 0x0016E078
		public string ban(long farmerId)
		{
			string userId = null;
			foreach (Server server in this.servers)
			{
				userId = server.getUserId(farmerId);
				if (userId != null)
				{
					break;
				}
			}
			if (userId != null && !Game1.bannedUsers.ContainsKey(userId))
			{
				string userName = Game1.multiplayer.getUserName(farmerId);
				if (userName == "" || userName == userId)
				{
					userName = null;
				}
				Game1.bannedUsers.Add(userId, userName);
				this.kick(farmerId);
				return userId;
			}
			return null;
		}

		// Token: 0x0600208E RID: 8334 RVA: 0x0016FF1C File Offset: 0x0016E11C
		public void playerDisconnected(long disconnectee)
		{
			Farmer disconnectedFarmer;
			Game1.otherFarmers.TryGetValue(disconnectee, out disconnectedFarmer);
			Game1.multiplayer.playerDisconnected(disconnectee);
			if (disconnectedFarmer != null)
			{
				OutgoingMessage message = new OutgoingMessage(19, disconnectedFarmer, Array.Empty<object>());
				foreach (long peer in Game1.otherFarmers.Keys)
				{
					if (peer != disconnectee)
					{
						this.sendMessage(peer, message);
					}
				}
			}
		}

		// Token: 0x0600208F RID: 8335 RVA: 0x0016FFA0 File Offset: 0x0016E1A0
		public bool isGameAvailable()
		{
			bool inIntro = Game1.currentMinigame is Intro || Game1.Date.DayOfMonth == 0;
			bool isWedding = Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding;
			bool isSleeping = Game1.newDaySync.hasInstance() && !Game1.newDaySync.hasFinished();
			bool isDemolishing = Game1.player.team.demolishLock.IsLocked();
			return !Game1.isFestival() && !isWedding && !inIntro && !isSleeping && !isDemolishing && Game1.weddingsToday.Count == 0 && Game1.gameMode != 6;
		}

		// Token: 0x06002090 RID: 8336 RVA: 0x00170040 File Offset: 0x0016E240
		public bool whenGameAvailable(Action action, Func<bool> customAvailabilityCheck = null)
		{
			Func<bool> availabilityCheck = (customAvailabilityCheck != null) ? customAvailabilityCheck : new Func<bool>(this.isGameAvailable);
			if (availabilityCheck())
			{
				action();
				return true;
			}
			this.pendingGameAvailableActions.Add(action, availabilityCheck);
			return false;
		}

		// Token: 0x06002091 RID: 8337 RVA: 0x00170080 File Offset: 0x0016E280
		private void rejectFarmhandRequest(string userId, string connectionId, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage)
		{
			this.sendAvailableFarmhands(userId, connectionId, sendMessage);
			Game1.log.Verbose("Rejected request for farmhand " + ((farmer.Value != null) ? farmer.Value.UniqueMultiplayerID.ToString() : "???"));
		}

		// Token: 0x06002092 RID: 8338 RVA: 0x001700CD File Offset: 0x0016E2CD
		public bool isUserBanned(string userID)
		{
			return Game1.bannedUsers.ContainsKey(userID);
		}

		// Token: 0x06002093 RID: 8339 RVA: 0x001700DC File Offset: 0x0016E2DC
		private bool authCheck(string userID, Farmer farmhand)
		{
			return (Game1.options.enableFarmhandCreation || this.IsLocalMultiplayerInitiatedServer() || farmhand.isCustomized.Value) && (userID == "" || farmhand.userID.Value == "" || farmhand.userID.Value == userID);
		}

		// Token: 0x06002094 RID: 8340 RVA: 0x00170143 File Offset: 0x0016E343
		public bool IsFarmhandAvailable(Farmer farmhand)
		{
			if (!Game1.netWorldState.Value.TryAssignFarmhandHome(farmhand))
			{
				return false;
			}
			Cabin cabin = Utility.getHomeOfFarmer(farmhand) as Cabin;
			return cabin == null || !cabin.isInventoryOpen();
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x00170178 File Offset: 0x0016E378
		public void checkFarmhandRequest(string userId, string connectionId, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve)
		{
			GameServer.<>c__DisplayClass42_0 CS$<>8__locals1;
			CS$<>8__locals1.farmer = farmer;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.connectionId = connectionId;
			CS$<>8__locals1.userId = userId;
			CS$<>8__locals1.sendMessage = sendMessage;
			CS$<>8__locals1.approve = approve;
			if (CS$<>8__locals1.farmer.Value == null)
			{
				this.rejectFarmhandRequest(CS$<>8__locals1.userId, CS$<>8__locals1.connectionId, CS$<>8__locals1.farmer, CS$<>8__locals1.sendMessage);
				return;
			}
			CS$<>8__locals1.id = CS$<>8__locals1.farmer.Value.UniqueMultiplayerID;
			if (this.isGameAvailable())
			{
				this.<checkFarmhandRequest>g__Check|42_0(ref CS$<>8__locals1);
				return;
			}
			this.sendAvailableFarmhands(CS$<>8__locals1.userId, CS$<>8__locals1.connectionId, CS$<>8__locals1.sendMessage);
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x00170224 File Offset: 0x0016E424
		public void sendAvailableFarmhands(string userId, string connectionId, Action<OutgoingMessage> sendMessage)
		{
			if (this.isGameAvailable())
			{
				Game1.log.Verbose("Sending available farmhands to connection ID " + connectionId);
				List<NetRef<Farmer>> availableFarmhands = new List<NetRef<Farmer>>();
				foreach (NetRef<Farmer> farmhand in Game1.netWorldState.Value.farmhandData.FieldDict.Values)
				{
					if ((!farmhand.Value.isActive() || Game1.multiplayer.isDisconnecting(farmhand.Value.UniqueMultiplayerID)) && this.IsFarmhandAvailable(farmhand.Value))
					{
						availableFarmhands.Add(farmhand);
					}
				}
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(Game1.year);
						writer.Write(Game1.seasonIndex);
						writer.Write(Game1.dayOfMonth);
						writer.Write((byte)availableFarmhands.Count);
						foreach (NetRef<Farmer> farmhand2 in availableFarmhands)
						{
							try
							{
								farmhand2.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
								farmhand2.WriteFull(writer);
							}
							finally
							{
								farmhand2.Serializer = null;
							}
						}
						stream.Seek(0L, SeekOrigin.Begin);
						sendMessage(new OutgoingMessage(9, Game1.player, new object[]
						{
							stream.ToArray()
						}));
					}
				}
				return;
			}
			sendMessage(new OutgoingMessage(11, Game1.player, new object[]
			{
				"Strings\\UI:Client_WaitForHostAvailability"
			}));
			if (this.pendingAvailableFarmhands.Contains(connectionId))
			{
				Game1.log.Verbose("Connection " + connectionId + " is already waiting to receive available farmhands");
				return;
			}
			Game1.log.Verbose("Postponing sending available farmhands to connection ID " + connectionId);
			this.pendingAvailableFarmhands.Add(connectionId);
			this.whenGameAvailable(delegate
			{
				this.pendingAvailableFarmhands.Remove(connectionId);
				if (this.isConnectionActive(connectionId))
				{
					this.sendAvailableFarmhands(userId, connectionId, sendMessage);
					return;
				}
				Game1.log.Verbose("Failed to send available farmhands to connection ID " + connectionId + ": Connection not active.");
			}, null);
		}

		// Token: 0x06002097 RID: 8343 RVA: 0x001704B8 File Offset: 0x0016E6B8
		public T GetServer<T>() where T : Server
		{
			foreach (Server server in this.servers)
			{
				T match = server as T;
				if (match != null)
				{
					return match;
				}
			}
			return default(T);
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x00170528 File Offset: 0x0016E728
		private void sendLocation(long peer, GameLocation location, bool force_current = false)
		{
			this.sendMessage(peer, 3, Game1.serverHost.Value, new object[]
			{
				force_current,
				Game1.multiplayer.writeObjectFullBytes<GameLocation>(Game1.multiplayer.locationRoot(location), new long?(peer))
			});
		}

		// Token: 0x06002099 RID: 8345 RVA: 0x00170574 File Offset: 0x0016E774
		private void warpFarmer(Farmer farmer, short x, short y, string name, bool isStructure)
		{
			GameLocation location = Game1.RequireLocation(name, isStructure);
			if (Game1.IsMasterGame)
			{
				location.hostSetup();
			}
			farmer.currentLocation = location;
			farmer.Position = new Vector2((float)(x * 64), (float)((int)(y * 64) - (farmer.Sprite.getHeight() - 32) + 16));
			this.sendLocation(farmer.UniqueMultiplayerID, location, false);
		}

		// Token: 0x0600209A RID: 8346 RVA: 0x001705D4 File Offset: 0x0016E7D4
		public void processIncomingMessage(IncomingMessage message)
		{
			byte messageType = message.MessageType;
			if (messageType != 2)
			{
				if (messageType != 5)
				{
					if (messageType != 10)
					{
						Game1.multiplayer.processIncomingMessage(message);
					}
					else
					{
						long recipient = message.Reader.ReadInt64();
						message.Reader.BaseStream.Position -= 8L;
						if (recipient == Multiplayer.AllPlayers || recipient == Game1.player.UniqueMultiplayerID)
						{
							Game1.multiplayer.processIncomingMessage(message);
						}
						this.rebroadcastClientMessage(message, recipient);
					}
				}
				else
				{
					short x = message.Reader.ReadInt16();
					short y = message.Reader.ReadInt16();
					string name = message.Reader.ReadString();
					byte flags = message.Reader.ReadByte();
					bool isStructure = (flags & 1) > 0;
					bool warpingForForcedRemoteEvent = (flags & 2) > 0;
					bool needsLocationInfo = (flags & 4) > 0;
					int facingDirection = 0;
					if ((flags & 16) != 0)
					{
						facingDirection = 1;
					}
					else if ((flags & 32) != 0)
					{
						facingDirection = 2;
					}
					else if ((flags & 64) != 0)
					{
						facingDirection = 3;
					}
					if (needsLocationInfo)
					{
						this.warpFarmer(message.SourceFarmer, x, y, name, isStructure);
					}
					Game1.dedicatedServer.HandleFarmerWarp(new DedicatedServer.FarmerWarp(message.SourceFarmer, x, y, name, isStructure, facingDirection, warpingForForcedRemoteEvent));
				}
			}
			else
			{
				message.Reader.ReadString();
				Game1.multiplayer.processIncomingMessage(message);
			}
			if (Game1.multiplayer.isClientBroadcastType(message.MessageType))
			{
				this.rebroadcastClientMessage(message, Multiplayer.AllPlayers);
			}
		}

		// Token: 0x0600209B RID: 8347 RVA: 0x0017073C File Offset: 0x0016E93C
		private void rebroadcastClientMessage(IncomingMessage message, long peerID)
		{
			OutgoingMessage outMessage = new OutgoingMessage(message);
			foreach (long peer in Game1.otherFarmers.Keys)
			{
				if (peer != message.FarmerID && (peerID == Multiplayer.AllPlayers || peer == peerID))
				{
					this.sendMessage(peer, outMessage);
				}
			}
		}

		// Token: 0x0600209C RID: 8348 RVA: 0x001707AC File Offset: 0x0016E9AC
		private void setLobbyData(string key, string value)
		{
			foreach (Server server in this.servers)
			{
				server.setLobbyData(key, value);
			}
		}

		// Token: 0x0600209D RID: 8349 RVA: 0x00170800 File Offset: 0x0016EA00
		private bool unclaimedFarmhandsExist()
		{
			using (NetDictionary<long, Farmer, NetRef<Farmer>, SerializableDictionary<long, Farmer>, NetLongDictionary<Farmer, NetRef<Farmer>>>.ValuesCollection.Enumerator enumerator = Game1.netWorldState.Value.farmhandData.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.userID.Value == "")
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600209E RID: 8350 RVA: 0x0017087C File Offset: 0x0016EA7C
		public void updateLobbyData()
		{
			this.setLobbyData("farmName", Game1.player.farmName.Value);
			this.setLobbyData("farmType", Convert.ToString(Game1.whichFarm));
			if (Game1.whichFarm == 7)
			{
				this.setLobbyData("modFarmType", Game1.GetFarmTypeID());
			}
			else
			{
				this.setLobbyData("modFarmType", "");
			}
			WorldDate date = WorldDate.Now();
			this.setLobbyData("date", Convert.ToString(date.TotalDays));
			IEnumerable<string> farmhandUserIds = from farmhand in Game1.getAllFarmhands()
			select farmhand.userID.Value;
			this.setLobbyData("farmhands", string.Join(",", from user in farmhandUserIds
			where user != ""
			select user));
			this.setLobbyData("newFarmhands", Convert.ToString(Game1.options.enableFarmhandCreation && this.unclaimedFarmhandsExist()));
		}

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x0600209F RID: 8351 RVA: 0x00170988 File Offset: 0x0016EB88
		public BandwidthLogger BandwidthLogger
		{
			get
			{
				foreach (Server server in this.servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.BandwidthLogger;
					}
				}
				return null;
			}
		}

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x060020A0 RID: 8352 RVA: 0x001709EC File Offset: 0x0016EBEC
		// (set) Token: 0x060020A1 RID: 8353 RVA: 0x00170A50 File Offset: 0x0016EC50
		public bool LogBandwidth
		{
			get
			{
				foreach (Server server in this.servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.LogBandwidth;
					}
				}
				return false;
			}
			set
			{
				foreach (Server server in this.servers)
				{
					if (server.connectionsCount > 0)
					{
						server.LogBandwidth = value;
						break;
					}
				}
			}
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x00170AB0 File Offset: 0x0016ECB0
		[CompilerGenerated]
		private void <checkFarmhandRequest>g__Check|42_0(ref GameServer.<>c__DisplayClass42_0 A_1)
		{
			Farmer originalFarmhand = Game1.netWorldState.Value.farmhandData[A_1.farmer.Value.UniqueMultiplayerID];
			if (!this.isConnectionActive(A_1.connectionId))
			{
				Game1.log.Verbose("Rejected request for connection ID " + A_1.connectionId + ": Connection not active.");
				return;
			}
			if (originalFarmhand == null)
			{
				Game1.log.Verbose("Rejected request for farmhand " + A_1.id.ToString() + ": doesn't exist");
				this.rejectFarmhandRequest(A_1.userId, A_1.connectionId, A_1.farmer, A_1.sendMessage);
				return;
			}
			if (!this.authCheck(A_1.userId, originalFarmhand))
			{
				Game1.log.Verbose(string.Concat(new string[]
				{
					"Rejected request for farmhand ",
					A_1.id.ToString(),
					": authorization failure ",
					A_1.userId,
					" ",
					originalFarmhand.userID.Value
				}));
				this.rejectFarmhandRequest(A_1.userId, A_1.connectionId, A_1.farmer, A_1.sendMessage);
				return;
			}
			if ((Game1.otherFarmers.ContainsKey(A_1.id) && !Game1.multiplayer.isDisconnecting(A_1.id)) || Game1.serverHost.Value.UniqueMultiplayerID == A_1.id)
			{
				Game1.log.Verbose("Rejected request for farmhand " + A_1.id.ToString() + ": already in use");
				this.rejectFarmhandRequest(A_1.userId, A_1.connectionId, A_1.farmer, A_1.sendMessage);
				return;
			}
			if (!this.IsFarmhandAvailable(A_1.farmer.Value))
			{
				Game1.log.Verbose("Rejected request for farmhand " + A_1.id.ToString() + ": farmhand availability failed");
				this.rejectFarmhandRequest(A_1.userId, A_1.connectionId, A_1.farmer, A_1.sendMessage);
				return;
			}
			if (!Game1.netWorldState.Value.TryAssignFarmhandHome(A_1.farmer.Value))
			{
				Game1.log.Verbose("Rejected request for farmhand " + A_1.id.ToString() + ": farmhand has no assigned cabin, and none is available to assign.");
				this.rejectFarmhandRequest(A_1.userId, A_1.connectionId, A_1.farmer, A_1.sendMessage);
				return;
			}
			Game1.log.Verbose("Approved request for farmhand " + A_1.id.ToString());
			A_1.approve();
			Game1.updateCellarAssignments();
			Game1.multiplayer.addPlayer(A_1.farmer);
			Game1.multiplayer.broadcastPlayerIntroduction(A_1.farmer);
			foreach (GameLocation location in Game1.locations)
			{
				if (Game1.multiplayer.isAlwaysActiveLocation(location))
				{
					this.sendLocation(A_1.id, location, false);
				}
			}
			if ((long)A_1.farmer.Value.disconnectDay.Value == (long)((ulong)Game1.MasterPlayer.stats.DaysPlayed))
			{
				GameLocation disconnectLoc = Game1.getLocationFromName(A_1.farmer.Value.disconnectLocation.Value);
				if (disconnectLoc != null && !Game1.multiplayer.isAlwaysActiveLocation(disconnectLoc))
				{
					this.sendLocation(A_1.id, disconnectLoc, true);
				}
			}
			else if (!string.IsNullOrEmpty(A_1.farmer.Value.lastSleepLocation.Value))
			{
				GameLocation last_sleep_location = Game1.getLocationFromName(A_1.farmer.Value.lastSleepLocation.Value);
				if (last_sleep_location != null && Game1.isLocationAccessible(last_sleep_location.Name) && !Game1.multiplayer.isAlwaysActiveLocation(last_sleep_location))
				{
					this.sendLocation(A_1.id, last_sleep_location, true);
				}
			}
			this.sendServerIntroduction(A_1.id);
			this.updateLobbyData();
		}

		// Token: 0x040013BC RID: 5052
		internal List<Server> servers = new List<Server>();

		// Token: 0x040013BD RID: 5053
		private Dictionary<Action, Func<bool>> pendingGameAvailableActions = new Dictionary<Action, Func<bool>>();

		// Token: 0x040013BE RID: 5054
		private readonly HashSet<string> pendingAvailableFarmhands = new HashSet<string>();

		// Token: 0x040013BF RID: 5055
		private List<Action> completedPendingActions = new List<Action>();

		// Token: 0x040013C0 RID: 5056
		private List<string> bannedUsers = new List<string>();

		// Token: 0x040013C1 RID: 5057
		protected bool _wasConnected;

		// Token: 0x040013C2 RID: 5058
		protected bool _isLocalMultiplayerInitiatedServer;
	}
}
