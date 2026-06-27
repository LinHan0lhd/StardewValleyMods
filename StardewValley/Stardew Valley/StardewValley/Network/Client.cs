using System;
using System.Collections.Generic;
using System.IO;
using Netcode;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewValley.Network
{
	// Token: 0x020001CA RID: 458
	public abstract class Client : IBandwidthMonitor
	{
		// Token: 0x06002037 RID: 8247
		protected abstract void connectImpl();

		// Token: 0x06002038 RID: 8248
		public abstract void disconnect(bool neatly = true);

		// Token: 0x06002039 RID: 8249
		protected abstract void receiveMessagesImpl();

		// Token: 0x0600203A RID: 8250
		public abstract void sendMessage(OutgoingMessage message);

		// Token: 0x0600203B RID: 8251
		public abstract string getUserID();

		// Token: 0x0600203C RID: 8252
		protected abstract string getHostUserName();

		// Token: 0x0600203D RID: 8253 RVA: 0x0016E911 File Offset: 0x0016CB11
		public virtual float GetPingToHost()
		{
			return 0f;
		}

		// Token: 0x0600203E RID: 8254 RVA: 0x0016E918 File Offset: 0x0016CB18
		public virtual string getUserName(long farmerId)
		{
			if (farmerId != Game1.serverHost.Value.UniqueMultiplayerID)
			{
				return this.userNames.GetValueOrDefault(farmerId, "?");
			}
			return this.getHostUserName();
		}

		// Token: 0x0600203F RID: 8255 RVA: 0x0016E944 File Offset: 0x0016CB44
		public virtual void connect()
		{
			Game1.log.Verbose("Starting client. Protocol version: " + Multiplayer.protocolVersion);
			this.connectionMessage = null;
			if (!this.connectionStarted)
			{
				this.connectionStarted = true;
				this.connectImpl();
				this.timeoutTime = new long?(DateTime.UtcNow.Ticks / 10000L + 45000L);
			}
		}

		// Token: 0x06002040 RID: 8256 RVA: 0x0016E9AC File Offset: 0x0016CBAC
		public virtual void receiveMessages()
		{
			this.receiveMessagesImpl();
			if (this.hasHandshaked)
			{
				this.timeoutTime = null;
			}
			if (this.timeoutTime != null && DateTime.UtcNow.Ticks / 10000L >= this.timeoutTime.Value)
			{
				this.pendingDisconnect = Multiplayer.DisconnectType.ClientTimeout;
				this.timedOut = true;
				this.disconnect(false);
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ClientTimeout);
			}
			BandwidthLogger bandwidthLogger = this.bandwidthLogger;
			if (bandwidthLogger == null)
			{
				return;
			}
			bandwidthLogger.Update();
		}

		// Token: 0x06002041 RID: 8257 RVA: 0x0016EA34 File Offset: 0x0016CC34
		protected virtual void processIncomingMessage(IncomingMessage message)
		{
			byte messageType = message.MessageType;
			if (messageType <= 9)
			{
				switch (messageType)
				{
				case 1:
					this.receiveServerIntroduction(message.Reader);
					return;
				case 2:
					this.userNames[message.FarmerID] = message.Reader.ReadString();
					Game1.multiplayer.processIncomingMessage(message);
					return;
				case 3:
					Game1.multiplayer.processIncomingMessage(message);
					return;
				default:
					if (messageType == 9)
					{
						this.receiveAvailableFarmhands(message.Reader);
						return;
					}
					break;
				}
			}
			else
			{
				if (messageType == 11)
				{
					this.connectionMessage = Game1.content.LoadString(message.Reader.ReadString());
					return;
				}
				if (messageType == 16)
				{
					if (message.FarmerID == Game1.serverHost.Value.UniqueMultiplayerID)
					{
						this.receiveUserNameUpdate(message.Reader);
						return;
					}
					return;
				}
			}
			Game1.multiplayer.processIncomingMessage(message);
		}

		// Token: 0x06002042 RID: 8258 RVA: 0x0016EB14 File Offset: 0x0016CD14
		protected virtual void receiveUserNameUpdate(BinaryReader msg)
		{
			long farmerId = msg.ReadInt64();
			string userName = msg.ReadString();
			this.userNames[farmerId] = userName;
		}

		// Token: 0x06002043 RID: 8259 RVA: 0x0016EB3C File Offset: 0x0016CD3C
		protected virtual void receiveAvailableFarmhands(BinaryReader msg)
		{
			int year = msg.ReadInt32();
			int season = msg.ReadInt32();
			int dayOfMonth = msg.ReadInt32();
			int count = (int)msg.ReadByte();
			this.availableFarmhands = new List<Farmer>();
			while (this.availableFarmhands.Count < count)
			{
				NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
				netFarmerRoot.ReadFull(msg, default(NetVersion));
				netFarmerRoot.MarkReassigned();
				netFarmerRoot.MarkClean();
				Farmer farmhand = netFarmerRoot.Value;
				this.availableFarmhands.Add(farmhand);
				farmhand.yearForSaveGame = new int?(year);
				farmhand.seasonForSaveGame = new int?(season);
				farmhand.dayOfMonthForSaveGame = new int?(dayOfMonth);
			}
			this.hasHandshaked = true;
			this.connectionMessage = null;
			if (!(Game1.activeClickableMenu is TitleMenu) && !(Game1.activeClickableMenu is FarmhandMenu))
			{
				using (List<Farmer>.Enumerator enumerator = this.availableFarmhands.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Game1.player = enumerator.Current;
						this.sendPlayerIntroduction();
						return;
					}
				}
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerFull);
			}
		}

		// Token: 0x06002044 RID: 8260 RVA: 0x0016EC5C File Offset: 0x0016CE5C
		public virtual bool PopulatePlatformData(Farmer farmer)
		{
			return false;
		}

		// Token: 0x06002045 RID: 8261 RVA: 0x0016EC60 File Offset: 0x0016CE60
		public virtual void sendPlayerIntroduction()
		{
			if (this.getUserID() != "")
			{
				string uid = this.getUserID();
				Game1.log.Verbose("sendPlayerIntroduction " + uid);
				Game1.player.userID.Value = uid;
			}
			this.PopulatePlatformData(Game1.player);
			(Game1.player.NetFields.Root as NetRoot<Farmer>).MarkClean();
			this.sendMessage(2, new object[]
			{
				Game1.multiplayer.writeObjectFullBytes<Farmer>(Game1.player.NetFields.Root as NetFarmerRoot, null)
			});
		}

		// Token: 0x06002046 RID: 8262 RVA: 0x0016ED08 File Offset: 0x0016CF08
		protected virtual void setUpGame()
		{
			Game1.flushLocationLookup();
			Game1.player.updateFriendshipGifts(Game1.Date);
			Game1.gameMode = 3;
			Game1.stats.checkForAchievements();
			Game1.multiplayerMode = 1;
			Game1.client = this;
			this.readyToPlay = true;
			BedFurniture.ApplyWakeUpPosition(Game1.player);
			Game1.fadeClear();
			Game1.currentLocation.updateSeasonalTileSheets(null);
			Game1.currentLocation.resetForPlayerEntry();
			Game1.player.sleptInTemporaryBed.Value = false;
			Game1.initializeVolumeLevels();
			if (Game1.MasterPlayer.eventsSeen.Contains("558291"))
			{
				Game1.player.songsHeard.Add("grandpas_theme");
			}
			Game1.AddNPCs();
			Game1.AddModNPCs();
			Utility.ForEachVillager(delegate(NPC villager)
			{
				villager.ChooseAppearance(null);
				return true;
			}, false);
			Game1.exitActiveMenu();
			if (!Game1.player.isCustomized.Value)
			{
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewFarmhand, false);
			}
			Game1.player.team.AddAnyBroadcastedMail();
			if (Game1.shouldPlayMorningSong(true))
			{
				Game1.playMorningSong(false);
			}
			for (int i = 1; i < Game1.netWorldState.Value.HighestPlayerLimit; i++)
			{
				if (Game1.getLocationFromName("Cellar" + (i + 1).ToString()) == null)
				{
					GameLocation cellar = Game1.CreateGameLocation("Cellar");
					if (cellar == null)
					{
						Game1.log.Error("Couldn't create 'Cellar' location. Was it removed from Data/Locations?", null);
					}
					else
					{
						NetString name = cellar.name;
						name.Value += (i + 1).ToString();
						Game1.locations.Add(cellar);
					}
				}
			}
			Game1.player.showToolUpgradeAvailability();
			Game1.dayTimeMoneyBox.questsDirty = true;
			Game1.player.ReequipEnchantments();
			foreach (Item item in Game1.player.Items)
			{
				Object o = item as Object;
				if (o != null)
				{
					o.reloadSprite();
				}
			}
			Game1.player.companions.Clear();
			Game1.player.resetAllTrinketEffects();
			Game1.player.isSitting.Value = false;
			Horse mount = Game1.player.mount;
			if (mount != null)
			{
				mount.dismount(false);
			}
			Game1.player.forceCanMove();
			Game1.player.viewingLocation.Value = null;
			Game1.player.timeWentToBed.Value = 0;
		}

		// Token: 0x06002047 RID: 8263 RVA: 0x0016EF78 File Offset: 0x0016D178
		protected virtual void receiveServerIntroduction(BinaryReader msg)
		{
			Game1.otherFarmers.Roots[Game1.player.UniqueMultiplayerID] = (Game1.player.NetFields.Root as NetFarmerRoot);
			NetFarmerRoot f = Game1.multiplayer.readFarmer(msg);
			long id = f.Value.UniqueMultiplayerID;
			Game1.serverHost = f;
			Game1.serverHost.Value.teamRoot = Game1.multiplayer.readObjectFull<FarmerTeam>(msg);
			Game1.otherFarmers.Roots.Add(id, f);
			Game1.player.teamRoot = Game1.serverHost.Value.teamRoot;
			Game1.netWorldState = Game1.multiplayer.readObjectFull<NetWorldState>(msg);
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			Game1.netWorldState.Value.WriteToGame1(true);
			this.setUpGame();
			if (Game1.chatBox != null)
			{
				Game1.chatBox.listPlayers(false, true);
			}
		}

		// Token: 0x06002048 RID: 8264 RVA: 0x0016F060 File Offset: 0x0016D260
		public virtual void sendMessages()
		{
			if (Game1.serverHost == null)
			{
				return;
			}
			foreach (OutgoingMessage message in Game1.serverHost.Value.messageQueue)
			{
				this.sendMessage(message);
			}
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				v.Value.messageQueue.Clear();
			}
		}

		// Token: 0x06002049 RID: 8265 RVA: 0x0016F110 File Offset: 0x0016D310
		public virtual void sendMessage(byte which, params object[] data)
		{
			this.sendMessage(new OutgoingMessage(which, Game1.player, data));
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x0600204A RID: 8266 RVA: 0x0016F124 File Offset: 0x0016D324
		public BandwidthLogger BandwidthLogger
		{
			get
			{
				return this.bandwidthLogger;
			}
		}

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x0600204B RID: 8267 RVA: 0x0016F12C File Offset: 0x0016D32C
		// (set) Token: 0x0600204C RID: 8268 RVA: 0x0016F137 File Offset: 0x0016D337
		public bool LogBandwidth
		{
			get
			{
				return this.bandwidthLogger != null;
			}
			set
			{
				this.bandwidthLogger = (value ? new BandwidthLogger() : null);
			}
		}

		// Token: 0x040013AF RID: 5039
		public const int connectionTimeout = 45000;

		// Token: 0x040013B0 RID: 5040
		public bool hasHandshaked;

		// Token: 0x040013B1 RID: 5041
		public bool readyToPlay;

		// Token: 0x040013B2 RID: 5042
		public bool timedOut;

		// Token: 0x040013B3 RID: 5043
		public bool connectionStarted;

		// Token: 0x040013B4 RID: 5044
		public string serverName = "???";

		// Token: 0x040013B5 RID: 5045
		public string connectionMessage;

		// Token: 0x040013B6 RID: 5046
		public Multiplayer.DisconnectType pendingDisconnect;

		// Token: 0x040013B7 RID: 5047
		protected BandwidthLogger bandwidthLogger;

		// Token: 0x040013B8 RID: 5048
		protected long? timeoutTime;

		// Token: 0x040013B9 RID: 5049
		public List<Farmer> availableFarmhands;

		// Token: 0x040013BA RID: 5050
		public Dictionary<long, string> userNames = new Dictionary<long, string>();
	}
}
