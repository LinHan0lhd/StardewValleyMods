using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Galaxy.Api;
using StardewValley.Logging;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Internal;
using StardewValley.SDKs.GogGalaxy.Listeners;
using Steamworks;

namespace StardewValley.SDKs.GogGalaxy
{
	// Token: 0x02000170 RID: 368
	public class GalaxySocket
	{
		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06001C0B RID: 7179 RVA: 0x0013FCAF File Offset: 0x0013DEAF
		public int ConnectionCount
		{
			get
			{
				return this.connections.Count;
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06001C0C RID: 7180 RVA: 0x0013FCBC File Offset: 0x0013DEBC
		public IEnumerable<GalaxyID> Connections
		{
			get
			{
				return this.connections.Values;
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06001C0D RID: 7181 RVA: 0x0013FCC9 File Offset: 0x0013DEC9
		public bool Connected
		{
			get
			{
				return this.lobby != null;
			}
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06001C0E RID: 7182 RVA: 0x0013FCD7 File Offset: 0x0013DED7
		public GalaxyID LobbyOwner
		{
			get
			{
				return this.lobbyOwner;
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06001C0F RID: 7183 RVA: 0x0013FCDF File Offset: 0x0013DEDF
		public GalaxyID Lobby
		{
			get
			{
				return this.lobby;
			}
		}

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06001C10 RID: 7184 RVA: 0x0013FCE8 File Offset: 0x0013DEE8
		public ulong? InviteDialogLobby
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x0013FD00 File Offset: 0x0013DF00
		public GalaxySocket(string protocolVersion)
		{
			this.protocolVersion = protocolVersion;
			this.checkedProcotolVersion = false;
			this.lobbyData["protocolVersion"] = protocolVersion;
			this.selfId = GalaxyInstance.User().GetGalaxyID();
			this.galaxyLobbyEnterCallback = new GalaxyLobbyEnteredListener(new Action<GalaxyID, LobbyEnterResult>(this.onGalaxyLobbyEnter));
			this.galaxyLobbyCreatedCallback = new GalaxyLobbyCreatedListener(new Action<GalaxyID, LobbyCreateResult>(this.onGalaxyLobbyCreated));
			this.galaxyLobbyMemberStateCallback = new GalaxyLobbyMemberStateListener(new Action<GalaxyID, GalaxyID, LobbyMemberStateChange>(this.onGalaxyMemberState));
			this.lobbyData["SteamHostId"] = SteamUser.GetSteamID().m_SteamID.ToString();
			this.lobbyData["HostDisplayName"] = SteamFriends.GetPersonaName();
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0013FDEA File Offset: 0x0013DFEA
		public string GetInviteCode()
		{
			if (this.lobby == null)
			{
				return null;
			}
			return "S" + Base36.Encode(this.lobby.GetRealID());
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x0013FE18 File Offset: 0x0013E018
		private string getConnectionString()
		{
			if (this.lobby == null)
			{
				return "";
			}
			return "-connect-lobby-" + this.lobby.ToUint64().ToString();
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x0013FE58 File Offset: 0x0013E058
		private long getTimeNow()
		{
			return DateTime.UtcNow.Ticks / 10000L;
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x0013FE79 File Offset: 0x0013E079
		public long GetPingWith(GalaxyID peer)
		{
			return (long)GalaxyInstance.Networking().GetPingWith(peer);
		}

		// Token: 0x06001C16 RID: 7190 RVA: 0x0013FE88 File Offset: 0x0013E088
		private LobbyType privacyToLobbyType(ServerPrivacy privacy)
		{
			switch (privacy)
			{
			case ServerPrivacy.InviteOnly:
				return LobbyType.LOBBY_TYPE_PRIVATE;
			case ServerPrivacy.FriendsOnly:
				return LobbyType.LOBBY_TYPE_FRIENDS_ONLY;
			case ServerPrivacy.Public:
				return LobbyType.LOBBY_TYPE_PUBLIC;
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Unknown server privacy type '");
				defaultInterpolatedStringHandler.AppendFormatted<ServerPrivacy>(privacy);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			}
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x0013FEE5 File Offset: 0x0013E0E5
		public void SetPrivacy(ServerPrivacy privacy)
		{
			this.privacy = privacy;
			this.updateLobbyPrivacy();
		}

		// Token: 0x06001C18 RID: 7192 RVA: 0x0013FEF4 File Offset: 0x0013E0F4
		public void CreateLobby(ServerPrivacy privacy, uint memberLimit)
		{
			this.privacy = privacy;
			this.memberLimit = memberLimit;
			this.lobbyOwner = this.selfId;
			this.isRecreatedLobby = false;
			this.tryCreateLobby();
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x0013FF20 File Offset: 0x0013E120
		private void tryCreateLobby()
		{
			Game1.log.Verbose("Creating lobby...");
			if (this.galaxyLobbyLeftCallback != null)
			{
				this.galaxyLobbyLeftCallback.Dispose();
				this.galaxyLobbyLeftCallback = null;
			}
			this.galaxyLobbyLeftCallback = new GalaxyLobbyLeftListener(new Action<GalaxyID, ILobbyLeftListener.LobbyLeaveReason>(this.onGalaxyLobbyLeft));
			try
			{
				GalaxyInstance.Matchmaking().CreateLobby(this.privacyToLobbyType(this.privacy), this.memberLimit, true, LobbyTopologyType.LOBBY_TOPOLOGY_TYPE_STAR);
			}
			catch (Exception e)
			{
				Game1.log.Error("Galaxy CreateLobby failed with an exception:", e);
				this.OnLobbyCreateFailed();
			}
			this.recreateTimer = 0L;
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x0013FFC0 File Offset: 0x0013E1C0
		public void JoinLobby(GalaxyID lobbyId, Action<string> onError)
		{
			try
			{
				this.connectingLobbyID = lobbyId;
				GalaxyInstance.Matchmaking().JoinLobby(this.connectingLobbyID);
			}
			catch (Exception e)
			{
				Game1.log.Error("Error joining Galaxy lobby.", e);
				string error_message = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
				if (e.Message.EndsWith("already joined this lobby"))
				{
					error_message += " (already connected)";
				}
				else
				{
					error_message = error_message + " (" + e.Message + ")";
				}
				onError(error_message);
				this.Close();
			}
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x00140060 File Offset: 0x0013E260
		public void SetLobbyData(string key, string value)
		{
			this.lobbyData[key] = value;
			if (this.lobby != null)
			{
				GalaxyInstance.Matchmaking().SetLobbyData(this.lobby, key, value);
			}
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x0014008F File Offset: 0x0013E28F
		private void updateLobbyPrivacy()
		{
			if (this.lobbyOwner != this.selfId)
			{
				return;
			}
			if (this.lobby != null)
			{
				GalaxyInstance.Matchmaking().SetLobbyType(this.lobby, this.privacyToLobbyType(this.privacy));
			}
		}

		// Token: 0x06001C1D RID: 7197 RVA: 0x001400D0 File Offset: 0x0013E2D0
		private void OnLobbyCreateFailed()
		{
			if (Game1.chatBox != null && this.isFirstRecreateAttempt)
			{
				if (this.isRecreatedLobby)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyCreateFail"));
				}
				else
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyCreateFail"));
				}
			}
			this.recreateTimer = this.getTimeNow() + 20000L;
			this.isRecreatedLobby = true;
			this.isFirstRecreateAttempt = false;
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x00140149 File Offset: 0x0013E349
		private void onGalaxyLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
		{
			if (result == LobbyCreateResult.LOBBY_CREATE_RESULT_ERROR)
			{
				Game1.log.Error("Failed to create Galaxy lobby.", null);
				this.OnLobbyCreateFailed();
			}
		}

		// Token: 0x06001C1F RID: 7199 RVA: 0x00140168 File Offset: 0x0013E368
		private void onGalaxyMemberState(GalaxyID lobbyID, GalaxyID memberID, LobbyMemberStateChange memberStateChange)
		{
			switch (memberStateChange)
			{
			case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_ENTERED:
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(memberID);
				defaultInterpolatedStringHandler.AppendLiteral(" connected to lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyID);
				log.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_LEFT:
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 2);
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(memberID);
				defaultInterpolatedStringHandler.AppendLiteral(" left lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyID);
				log2.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			case (LobbyMemberStateChange)3:
				break;
			case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_DISCONNECTED:
			{
				IGameLogger log3 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(memberID);
				defaultInterpolatedStringHandler.AppendLiteral(" disconnected from lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyID);
				defaultInterpolatedStringHandler.AppendLiteral(" without leaving");
				log3.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
				if (memberStateChange == LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_KICKED)
				{
					IGameLogger log4 = Game1.log;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 2);
					defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(memberID);
					defaultInterpolatedStringHandler.AppendLiteral(" was kicked from lobby ");
					defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyID);
					log4.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				if (memberStateChange != LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_BANNED)
				{
					return;
				}
				IGameLogger log5 = Game1.log;
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 2);
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(memberID);
				defaultInterpolatedStringHandler.AppendLiteral(" was banned from lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<GalaxyID>(lobbyID);
				log5.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				break;
			}
			}
		}

		// Token: 0x06001C20 RID: 7200 RVA: 0x001402C0 File Offset: 0x0013E4C0
		private void onGalaxyLobbyLeft(GalaxyID lobbyID, ILobbyLeftListener.LobbyLeaveReason leaveReason)
		{
			if (leaveReason != ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_USER_LEFT)
			{
				Program.WriteLog(Program.LogType.Disconnect, "Forcibly left Galaxy lobby at " + DateTime.Now.ToLongTimeString() + " - " + leaveReason.ToString(), true);
			}
			if (Game1.chatBox != null)
			{
				string lobby_lost_reason;
				switch (leaveReason)
				{
				case ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_USER_LEFT:
					lobby_lost_reason = Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_UserLeft");
					break;
				case ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_LOBBY_CLOSED:
					lobby_lost_reason = Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_LobbyClosed");
					break;
				case ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_CONNECTION_LOST:
					lobby_lost_reason = Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_ConnectionLost");
					break;
				default:
					lobby_lost_reason = "";
					break;
				}
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyLost", lobby_lost_reason).Trim());
			}
			Game1.log.Verbose("Left lobby " + lobbyID.ToUint64().ToString() + " - leaveReason: " + leaveReason.ToString());
			this.lobby = null;
			this.recreateTimer = this.getTimeNow() + 20000L;
			this.isRecreatedLobby = true;
			this.isFirstRecreateAttempt = true;
		}

		// Token: 0x06001C21 RID: 7201 RVA: 0x001403D8 File Offset: 0x0013E5D8
		private void onGalaxyLobbyEnter(GalaxyID lobbyID, LobbyEnterResult result)
		{
			this.connectingLobbyID = null;
			if (result != LobbyEnterResult.LOBBY_ENTER_RESULT_SUCCESS)
			{
				return;
			}
			Game1.log.Verbose("Lobby entered: " + lobbyID.ToUint64().ToString());
			this.lobby = lobbyID;
			this.lobbyOwner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobbyID);
			if (Game1.chatBox != null)
			{
				string invite_code_string = "";
				if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
				{
					invite_code_string = Game1.content.LoadString("Strings\\UI:Chat_LobbyJoined_InviteCode", this.GetInviteCode());
				}
				if (this.isRecreatedLobby)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyRecreated", invite_code_string).Trim());
				}
				else
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyJoined", invite_code_string).Trim());
				}
			}
			if (this.lobbyOwner == this.selfId)
			{
				foreach (KeyValuePair<string, string> pair in this.lobbyData)
				{
					GalaxyInstance.Matchmaking().SetLobbyData(this.lobby, pair.Key, pair.Value);
				}
				this.updateLobbyPrivacy();
			}
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x0014052C File Offset: 0x0013E72C
		public IEnumerable<GalaxyID> LobbyMembers()
		{
			if (this.lobby == null)
			{
				yield break;
			}
			uint lobby_members_count;
			try
			{
				lobby_members_count = GalaxyInstance.Matchmaking().GetNumLobbyMembers(this.lobby);
			}
			catch
			{
				yield break;
			}
			uint num;
			for (uint i = 0U; i < lobby_members_count; i = num)
			{
				GalaxyID lobbyMember = GalaxyInstance.Matchmaking().GetLobbyMemberByIndex(this.lobby, i);
				if (!(lobbyMember == this.selfId) && !this.ghosts.Contains(lobbyMember.ToUint64()))
				{
					yield return lobbyMember;
				}
				num = i + 1U;
			}
			yield break;
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x0014053C File Offset: 0x0013E73C
		private void close(GalaxyID peer)
		{
			this.connections.Remove(peer.ToUint64());
			this.incompletePackets.Remove(peer.ToUint64());
		}

		// Token: 0x06001C24 RID: 7204 RVA: 0x00140562 File Offset: 0x0013E762
		public void Kick(GalaxyID user)
		{
			this.ghosts.Add(user.ToUint64());
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x00140578 File Offset: 0x0013E778
		public void Close()
		{
			if (this.connectingLobbyID != null)
			{
				GalaxyInstance.Matchmaking().LeaveLobby(this.connectingLobbyID);
				this.connectingLobbyID = null;
			}
			if (this.lobby != null)
			{
				while (this.ConnectionCount > 0)
				{
					this.close(this.Connections.First<GalaxyID>());
				}
				GalaxyInstance.Matchmaking().LeaveLobby(this.lobby);
				this.lobby = null;
			}
			this.updateLobbyPrivacy();
			try
			{
				this.galaxyLobbyEnterCallback.Dispose();
			}
			catch (Exception)
			{
			}
			try
			{
				this.galaxyLobbyCreatedCallback.Dispose();
			}
			catch (Exception)
			{
			}
			try
			{
				this.galaxyLobbyMemberStateCallback.Dispose();
			}
			catch (Exception)
			{
			}
			GalaxyLobbyLeftListener galaxyLobbyLeftListener = this.galaxyLobbyLeftCallback;
			if (galaxyLobbyLeftListener == null)
			{
				return;
			}
			galaxyLobbyLeftListener.Dispose();
		}

		// Token: 0x06001C26 RID: 7206 RVA: 0x0014065C File Offset: 0x0013E85C
		private void PreprocessMessage(GalaxyID peer, MemoryStream stream, Action<GalaxyID, Stream> onMessage)
		{
			byte[] decompressed;
			if (Program.netCompression.TryDecompressStream(stream, out decompressed))
			{
				stream = new MemoryStream(decompressed);
			}
			onMessage(peer, stream);
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x00140688 File Offset: 0x0013E888
		public void Receive(Action<GalaxyID> onConnection, Action<GalaxyID, Stream> onMessage, Action<GalaxyID> onDisconnect, Action<string> onError)
		{
			long timeNow = this.getTimeNow();
			if (this.lobby == null)
			{
				if (this.lobbyOwner == this.selfId && this.recreateTimer > 0L && this.recreateTimer <= timeNow)
				{
					this.recreateTimer = 0L;
					this.tryCreateLobby();
				}
				this.DisconnectPeers(onDisconnect);
				return;
			}
			if (!this.checkedProcotolVersion)
			{
				try
				{
					string lobbyVersion = GalaxyInstance.Matchmaking().GetLobbyData(this.lobby, "protocolVersion");
					if (lobbyVersion != "")
					{
						this.checkedProcotolVersion = true;
						if (lobbyVersion != this.protocolVersion)
						{
							onError(Game1.content.LoadString("Strings\\UI:CoopMenu_FailedProtocolVersion"));
							this.Close();
							return;
						}
					}
				}
				catch (Exception)
				{
				}
			}
			IEnumerable<GalaxyID> cachedLobbyMembers = this.LobbyMembers();
			foreach (GalaxyID lobbyMember in cachedLobbyMembers)
			{
				if (!this.connections.ContainsKey(lobbyMember.ToUint64()) && !this.ghosts.Contains(lobbyMember.ToUint64()))
				{
					this.connections.Add(lobbyMember.ToUint64(), lobbyMember);
					onConnection(lobbyMember);
				}
			}
			this.ghosts.IntersectWith(from peer in cachedLobbyMembers
			select peer.ToUint64());
			byte[] buffer = new byte[1300];
			uint packetSize = 1300U;
			GalaxyID sender = new GalaxyID();
			while (GalaxyInstance.Networking().ReadP2PPacket(buffer, (uint)buffer.Length, ref packetSize, ref sender))
			{
				if (this.connections.ContainsKey(sender.ToUint64()) && buffer[0] != 255)
				{
					bool incomplete = buffer[0] == 1;
					MemoryStream messageData = new MemoryStream();
					messageData.Write(buffer, 4, (int)(packetSize - 4U));
					MemoryStream packet;
					if (this.incompletePackets.TryGetValue(sender.ToUint64(), out packet))
					{
						messageData.Position = 0L;
						messageData.CopyTo(packet);
						if (!incomplete)
						{
							messageData = packet;
							this.incompletePackets.Remove(sender.ToUint64());
							messageData.Position = 0L;
							this.PreprocessMessage(sender, messageData, onMessage);
						}
					}
					else if (incomplete)
					{
						messageData.Position = messageData.Length;
						this.incompletePackets[sender.ToUint64()] = messageData;
					}
					else
					{
						messageData.Position = 0L;
						this.PreprocessMessage(sender, messageData, onMessage);
					}
				}
			}
			this.DisconnectPeers(onDisconnect);
		}

		// Token: 0x06001C28 RID: 7208 RVA: 0x00140920 File Offset: 0x0013EB20
		public virtual void DisconnectPeers(Action<GalaxyID> onDisconnect)
		{
			List<GalaxyID> disconnectedPeers = new List<GalaxyID>();
			HashSet<GalaxyID> onlinePeers = new HashSet<GalaxyID>();
			foreach (GalaxyID lobbyMember in this.LobbyMembers())
			{
				onlinePeers.Add(lobbyMember);
			}
			foreach (GalaxyID peer in this.connections.Values)
			{
				if (this.lobby == null || !onlinePeers.Contains(peer))
				{
					disconnectedPeers.Add(peer);
				}
			}
			foreach (GalaxyID peer2 in disconnectedPeers)
			{
				onDisconnect(peer2);
				this.close(peer2);
			}
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x00140A24 File Offset: 0x0013EC24
		public void Heartbeat(IEnumerable<GalaxyID> peers)
		{
			long timeNow = this.getTimeNow();
			if (this.heartbeatTimer <= timeNow)
			{
				this.heartbeatTimer = timeNow + 8L;
				byte[] heartbeatPacket = new byte[]
				{
					byte.MaxValue
				};
				foreach (GalaxyID peer in peers)
				{
					GalaxyInstance.Networking().SendP2PPacket(peer, heartbeatPacket, (uint)heartbeatPacket.Length, P2PSendType.P2P_SEND_RELIABLE_IMMEDIATE);
				}
			}
		}

		// Token: 0x06001C2A RID: 7210 RVA: 0x00140AA0 File Offset: 0x0013ECA0
		public void Send(GalaxyID peer, byte[] data)
		{
			if (!this.connections.ContainsKey(peer.ToUint64()))
			{
				return;
			}
			data = Program.netCompression.CompressAbove(data, 256);
			if (data.Length <= 1100)
			{
				byte[] packet = new byte[data.Length + 4];
				data.CopyTo(packet, 4);
				GalaxyInstance.Networking().SendP2PPacket(peer, packet, (uint)packet.Length, P2PSendType.P2P_SEND_RELIABLE);
				return;
			}
			int chunkSize = 1096;
			int messageOffset = 0;
			byte[] packet2 = new byte[1100];
			packet2[0] = 1;
			while (messageOffset < data.Length)
			{
				int thisChunkSize = chunkSize;
				if (messageOffset + chunkSize >= data.Length)
				{
					packet2[0] = 0;
					thisChunkSize = data.Length - messageOffset;
				}
				Buffer.BlockCopy(data, messageOffset, packet2, 4, thisChunkSize);
				messageOffset += thisChunkSize;
				GalaxyInstance.Networking().SendP2PPacket(peer, packet2, (uint)(thisChunkSize + 4), P2PSendType.P2P_SEND_RELIABLE);
			}
		}

		// Token: 0x06001C2B RID: 7211 RVA: 0x00140B5C File Offset: 0x0013ED5C
		public void Send(GalaxyID peer, OutgoingMessage message)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					message.Write(writer);
					stream.Seek(0L, SeekOrigin.Begin);
					this.Send(peer, stream.ToArray());
				}
			}
		}

		// Token: 0x040010EA RID: 4330
		public const long Timeout = 30000L;

		// Token: 0x040010EB RID: 4331
		public const string ProtocolVersionKey = "protocolVersion";

		// Token: 0x040010EC RID: 4332
		public const string HostNameDataKey = "HostDisplayName";

		// Token: 0x040010ED RID: 4333
		public const string SteamHostIdDataKey = "SteamHostId";

		// Token: 0x040010EE RID: 4334
		public const string SteamLobbyIdDataKey = "SteamLobbyId";

		// Token: 0x040010EF RID: 4335
		private const int SendMaxPacketSize = 1100;

		// Token: 0x040010F0 RID: 4336
		private const int ReceiveMaxPacketSize = 1300;

		// Token: 0x040010F1 RID: 4337
		private const long RecreateLobbyDelay = 20000L;

		// Token: 0x040010F2 RID: 4338
		private const long HeartbeatDelay = 8L;

		// Token: 0x040010F3 RID: 4339
		private const byte HeartbeatMessage = 255;

		// Token: 0x040010F4 RID: 4340
		public bool isRecreatedLobby;

		// Token: 0x040010F5 RID: 4341
		public bool isFirstRecreateAttempt;

		// Token: 0x040010F6 RID: 4342
		private GalaxyID selfId;

		// Token: 0x040010F7 RID: 4343
		private GalaxyID connectingLobbyID;

		// Token: 0x040010F8 RID: 4344
		private GalaxyID lobby;

		// Token: 0x040010F9 RID: 4345
		private GalaxyID lobbyOwner;

		// Token: 0x040010FA RID: 4346
		private GalaxyLobbyEnteredListener galaxyLobbyEnterCallback;

		// Token: 0x040010FB RID: 4347
		private GalaxyLobbyCreatedListener galaxyLobbyCreatedCallback;

		// Token: 0x040010FC RID: 4348
		private GalaxyLobbyLeftListener galaxyLobbyLeftCallback;

		// Token: 0x040010FD RID: 4349
		private GalaxyLobbyMemberStateListener galaxyLobbyMemberStateCallback;

		// Token: 0x040010FE RID: 4350
		private string protocolVersion;

		// Token: 0x040010FF RID: 4351
		private bool checkedProcotolVersion;

		// Token: 0x04001100 RID: 4352
		private Dictionary<string, string> lobbyData = new Dictionary<string, string>();

		// Token: 0x04001101 RID: 4353
		private ServerPrivacy privacy;

		// Token: 0x04001102 RID: 4354
		private uint memberLimit;

		// Token: 0x04001103 RID: 4355
		private long recreateTimer;

		// Token: 0x04001104 RID: 4356
		private long heartbeatTimer;

		// Token: 0x04001105 RID: 4357
		private Dictionary<ulong, GalaxyID> connections = new Dictionary<ulong, GalaxyID>();

		// Token: 0x04001106 RID: 4358
		private HashSet<ulong> ghosts = new HashSet<ulong>();

		// Token: 0x04001107 RID: 4359
		private Dictionary<ulong, MemoryStream> incompletePackets = new Dictionary<ulong, MemoryStream>();
	}
}
