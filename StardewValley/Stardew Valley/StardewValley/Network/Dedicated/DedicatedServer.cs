using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Network.Dedicated
{
	// Token: 0x02000202 RID: 514
	public class DedicatedServer
	{
		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x060022D5 RID: 8917 RVA: 0x0017776A File Offset: 0x0017596A
		public bool FakeWarp
		{
			get
			{
				return Game1.IsDedicatedHost && this.fakeWarp;
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x060022D6 RID: 8918 RVA: 0x0017777C File Offset: 0x0017597C
		public Farmer FakeFarmer
		{
			get
			{
				if (!Game1.IsDedicatedHost)
				{
					return Game1.player;
				}
				Farmer farmer = Game1.getFarmer(this.fakeFarmerId);
				if (!Game1.multiplayer.isDisconnecting(farmer))
				{
					return farmer;
				}
				return Game1.player;
			}
		}

		// Token: 0x060022D7 RID: 8919 RVA: 0x001777B8 File Offset: 0x001759B8
		public DedicatedServer()
		{
			this.Reset();
		}

		// Token: 0x060022D8 RID: 8920 RVA: 0x00177808 File Offset: 0x00175A08
		public void Reset()
		{
			this.fakeWarp = false;
			this.warpingSleep = false;
			this.warpingFestival = false;
			this.startedFestivalMainEvent = false;
			this.startedFestivalEnd = false;
			this.shouldJudgeGrange = false;
			this.warpingHostBroadcastEvent = false;
			this.broadcastEvents.Clear();
			this.eventLocks.Clear();
		}

		// Token: 0x060022D9 RID: 8921 RVA: 0x0017785C File Offset: 0x00175A5C
		public void ResetForNewDay()
		{
			if (!Game1.IsDedicatedHost)
			{
				return;
			}
			this.fakeWarp = false;
			this.warpingSleep = false;
			this.warpingFestival = false;
			this.startedFestivalMainEvent = false;
			this.startedFestivalEnd = false;
			this.shouldJudgeGrange = false;
			this.warpingHostBroadcastEvent = false;
			this.eventLocks.Clear();
		}

		// Token: 0x060022DA RID: 8922 RVA: 0x001778B0 File Offset: 0x00175AB0
		private bool TryForceClientHostEvent(DedicatedServer.FarmerWarp warp, GameLocation location, string eventId)
		{
			if (Game1.server == null)
			{
				return false;
			}
			string key = (warp.isStructure ? "1" : "0") + location.NameOrUniqueName;
			Dictionary<string, long> locationEvents;
			if (!this.eventLocks.TryGetValue(key, out locationEvents))
			{
				this.eventLocks[key] = new Dictionary<string, long>();
			}
			else if (locationEvents.ContainsKey(eventId))
			{
				return false;
			}
			this.eventLocks[key][eventId] = warp.who.UniqueMultiplayerID;
			object[] message = Game1.multiplayer.generateForceEventMessage(eventId, location, (int)warp.x, (int)warp.y, true, true);
			Game1.server.sendMessage(warp.who.UniqueMultiplayerID, 4, Game1.player, message);
			return true;
		}

		// Token: 0x060022DB RID: 8923 RVA: 0x0017796C File Offset: 0x00175B6C
		private void CheckForWarpEvents(DedicatedServer.FarmerWarp warp)
		{
			if (warp.warpingForForcedRemoteEvent)
			{
				return;
			}
			if (Game1.eventUp || Game1.farmEvent != null || this.IsWarping())
			{
				return;
			}
			GameLocation location = Game1.getLocationFromName(warp.name, warp.isStructure);
			Dictionary<string, string> events;
			try
			{
				string eventAssetName;
				if (!location.TryGetLocationEvents(out eventAssetName, out events) || events == null)
				{
					return;
				}
			}
			catch
			{
				return;
			}
			int xLocationAfterWarp = Game1.xLocationAfterWarp;
			int yLocationAfterWarp = Game1.yLocationAfterWarp;
			Game1.xLocationAfterWarp = (int)warp.x;
			Game1.yLocationAfterWarp = (int)warp.y;
			this.fakeWarp = true;
			EventCommandDelegate broadcastEventCommand = null;
			foreach (string eventKey in events.Keys)
			{
				this.CheckedHostPrecondition = false;
				string eventId = location.checkEventPrecondition(eventKey);
				if (this.CheckedHostPrecondition && !(eventId == "-1") && !string.IsNullOrEmpty(eventId) && GameLocation.IsValidLocationEvent(eventKey, events[eventKey]) && (broadcastEventCommand != null || Event.TryGetEventCommandHandler("BroadcastEvent", out broadcastEventCommand)))
				{
					if (this.notBroadcastEvents.Contains(eventId))
					{
						if (this.TryForceClientHostEvent(warp, location, eventId))
						{
							break;
						}
					}
					else
					{
						if (this.broadcastEvents.Contains(eventId))
						{
							this.fakeFarmerId = warp.who.UniqueMultiplayerID;
							this.warpingHostBroadcastEvent = true;
							break;
						}
						string[] array = Event.ParseCommands(events[eventKey], null);
						for (int i = 0; i < array.Length; i++)
						{
							string commandName = ArgUtility.Get(ArgUtility.SplitBySpaceQuoteAware(array[i]), 0, null, true);
							EventCommandDelegate eventCommandHandler;
							if (!(((commandName != null) ? new bool?(commandName.StartsWith("--")) : null) ?? true) && Event.TryGetEventCommandHandler(commandName, out eventCommandHandler) && eventCommandHandler == broadcastEventCommand)
							{
								this.fakeFarmerId = warp.who.UniqueMultiplayerID;
								this.warpingHostBroadcastEvent = true;
								this.broadcastEvents.Add(eventId);
								break;
							}
						}
						if (!this.warpingHostBroadcastEvent)
						{
							this.notBroadcastEvents.Add(eventId);
							if (this.TryForceClientHostEvent(warp, location, eventId))
							{
								break;
							}
						}
					}
				}
			}
			this.fakeWarp = false;
			Game1.xLocationAfterWarp = xLocationAfterWarp;
			Game1.yLocationAfterWarp = yLocationAfterWarp;
			if (this.warpingHostBroadcastEvent)
			{
				LocationRequest locationRequest = Game1.getLocationRequest(warp.name, warp.isStructure);
				locationRequest.OnWarp += delegate()
				{
					this.warpingHostBroadcastEvent = false;
				};
				Game1.warpFarmer(locationRequest, (int)warp.x, (int)warp.y, warp.facingDirection);
			}
		}

		// Token: 0x060022DC RID: 8924 RVA: 0x00177C38 File Offset: 0x00175E38
		private bool IsWarping()
		{
			return Game1.isWarping || this.warpingHostBroadcastEvent || this.warpingSleep || this.warpingFestival;
		}

		// Token: 0x060022DD RID: 8925 RVA: 0x00177C5C File Offset: 0x00175E5C
		public void DoHostAction(string action, params object[] data)
		{
			object[] messageData = new object[data.Length + 2];
			messageData[0] = 1;
			messageData[1] = action;
			Array.Copy(data, 0, messageData, 2, data.Length);
			OutgoingMessage message = new OutgoingMessage(33, Game1.player, messageData);
			if (Game1.IsMasterGame)
			{
				IncomingMessage fakeMessage = new IncomingMessage();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(memoryStream))
					{
						message.Write(writer);
						memoryStream.Seek(0L, SeekOrigin.Begin);
						using (BinaryReader reader = new BinaryReader(memoryStream))
						{
							fakeMessage.Read(reader);
						}
					}
				}
				Game1.multiplayer.processIncomingMessage(fakeMessage);
				return;
			}
			if (!Game1.HasDedicatedHost)
			{
				Game1.log.Error("Tried to execute a host-only action '" + action + "' as a client on a non-dedicated server.", null);
				return;
			}
			if (Game1.client == null)
			{
				return;
			}
			Game1.client.sendMessage(message);
		}

		// Token: 0x060022DE RID: 8926 RVA: 0x00177D68 File Offset: 0x00175F68
		public void Tick()
		{
			if (!Game1.IsDedicatedHost)
			{
				return;
			}
			this.onlineIds.Clear();
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (!Game1.multiplayer.isDisconnecting(farmer) && farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					this.onlineIds.Add(farmer.UniqueMultiplayerID);
				}
			}
			if (this.onlineIds.Count == 0)
			{
				this.farmerWarps.Clear();
				this.eventLocks.Clear();
				Event currentEvent = Game1.CurrentEvent;
				bool? flag = (currentEvent != null) ? new bool?(currentEvent.isFestival) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					if (Game1.netWorldState.Value.IsPaused)
					{
						Game1.netWorldState.Value.IsPaused = false;
					}
					if (!this.startedFestivalEnd)
					{
						Game1.CurrentEvent.TryStartEndFestivalDialogue(Game1.player);
						this.startedFestivalEnd = true;
					}
					return;
				}
				if (!Game1.netWorldState.Value.IsPaused)
				{
					Game1.netWorldState.Value.IsPaused = true;
				}
				return;
			}
			else
			{
				if (Game1.netWorldState.Value.IsPaused)
				{
					Game1.netWorldState.Value.IsPaused = false;
				}
				if (Game1.player.Stamina < (float)Game1.player.MaxStamina)
				{
					Game1.player.Stamina = (float)Game1.player.MaxStamina;
				}
				if (Game1.player.health < Game1.player.maxHealth)
				{
					Game1.player.health = Game1.player.maxHealth;
				}
				if (this.eventLocks.Count > 0)
				{
					List<string> removeLocations = new List<string>();
					List<string> removeEvents = new List<string>();
					foreach (KeyValuePair<string, Dictionary<string, long>> locationEntry in this.eventLocks)
					{
						removeEvents.Clear();
						foreach (KeyValuePair<string, long> eventEntry in locationEntry.Value)
						{
							if (!this.onlineIds.Contains(eventEntry.Value))
							{
								removeEvents.Add(eventEntry.Key);
							}
						}
						if (locationEntry.Value.Count - removeEvents.Count <= 0)
						{
							removeLocations.Add(locationEntry.Key);
						}
						else
						{
							foreach (string eventToRemove in removeEvents)
							{
								locationEntry.Value.Remove(eventToRemove);
							}
						}
					}
					foreach (string locationToRemove in removeLocations)
					{
						this.eventLocks.Remove(locationToRemove);
					}
				}
				DedicatedServer.FarmerWarp warp;
				while (this.farmerWarps.TryDequeue(out warp))
				{
					if (warp.who != null && this.onlineIds.Contains(warp.who.UniqueMultiplayerID))
					{
						this.CheckForWarpEvents(warp);
					}
				}
				if (this.IsWarping())
				{
					return;
				}
				DialogueBox dialogueBox = Game1.activeClickableMenu as DialogueBox;
				if (dialogueBox != null)
				{
					if (dialogueBox.isQuestion)
					{
						dialogueBox.selectedResponse = 0;
					}
					dialogueBox.receiveLeftClick(0, 0, true);
				}
				if (Game1.CurrentEvent != null)
				{
					if (!Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
					{
						Game1.CurrentEvent.skipped = true;
						Game1.CurrentEvent.skipEvent();
						Game1.freezeControls = false;
					}
					if (Game1.CurrentEvent.isFestival)
					{
						NPC festivalHost = Game1.CurrentEvent.festivalHost;
						if (festivalHost != null && !this.startedFestivalMainEvent && this.CheckOthersReady("MainEvent_" + Game1.CurrentEvent.id))
						{
							Game1.CurrentEvent.answerDialogueQuestion(festivalHost, "yes");
							this.startedFestivalMainEvent = true;
						}
					}
					if (!this.startedFestivalEnd && Game1.CurrentEvent.isFestival && this.CheckOthersReady("festivalEnd"))
					{
						Game1.CurrentEvent.TryStartEndFestivalDialogue(Game1.player);
						this.startedFestivalEnd = true;
					}
					return;
				}
				if (!this.warpingSleep && this.CheckOthersReady("sleep"))
				{
					if (Game1.currentLocation.NameOrUniqueName.EqualsIgnoreCase(Game1.player.homeLocation.Value))
					{
						this.HostSleepInBed();
					}
					else
					{
						this.warpingSleep = true;
						LocationRequest locationRequest = Game1.getLocationRequest(Game1.player.homeLocation.Value, false);
						locationRequest.OnWarp += delegate()
						{
							this.HostSleepInBed();
						};
						Game1.warpFarmer(locationRequest, 5, 9, Game1.player.FacingDirection);
					}
				}
				if (!this.warpingFestival && Game1.whereIsTodaysFest != null && this.CheckOthersReady("festivalStart"))
				{
					this.warpingFestival = true;
					LocationRequest locationRequest2 = Game1.getLocationRequest(Game1.whereIsTodaysFest, false);
					locationRequest2.OnWarp += delegate()
					{
						this.warpingFestival = false;
					};
					int tileX = -1;
					int tileY = -1;
					Utility.getDefaultWarpLocation(Game1.whereIsTodaysFest, ref tileX, ref tileY);
					Game1.warpFarmer(locationRequest2, tileX, tileY, 2);
				}
				return;
			}
		}

		// Token: 0x060022DF RID: 8927 RVA: 0x001782BC File Offset: 0x001764BC
		internal void HandleFarmerWarp(DedicatedServer.FarmerWarp warp)
		{
			if (!Game1.IsDedicatedHost)
			{
				return;
			}
			if (warp.who == null)
			{
				return;
			}
			this.farmerWarps.Enqueue(warp);
		}

		// Token: 0x060022E0 RID: 8928 RVA: 0x001782DC File Offset: 0x001764DC
		private bool CheckOthersReady(string readyCheck)
		{
			if (readyCheck == "MainEvent_festival_fall16")
			{
				return this.shouldJudgeGrange;
			}
			int ready = Game1.netReady.GetNumberReady(readyCheck);
			return ready > 0 && !Game1.netReady.IsReady(readyCheck) && ready >= Game1.netReady.GetNumberRequired(readyCheck) - 1;
		}

		// Token: 0x060022E1 RID: 8929 RVA: 0x00178334 File Offset: 0x00176534
		private void HostSleepInBed()
		{
			FarmHouse farmHouse = Game1.currentLocation as FarmHouse;
			if (farmHouse != null)
			{
				Game1.player.position.Set(Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f);
				farmHouse.answerDialogueAction("Sleep_Yes", null);
			}
			this.warpingSleep = false;
		}

		// Token: 0x060022E2 RID: 8930 RVA: 0x00178388 File Offset: 0x00176588
		private void ProcessEventDone(IncomingMessage message)
		{
			if (message.SourceFarmer == null)
			{
				return;
			}
			string name = message.Reader.ReadString();
			bool locationIsStructure = message.Reader.ReadByte() > 0;
			string eventId = message.Reader.ReadString();
			GameLocation location = Game1.getLocationFromName(name, locationIsStructure);
			if (location == null)
			{
				return;
			}
			string key = (locationIsStructure ? "1" : "0") + location.NameOrUniqueName;
			Dictionary<string, long> locationEvents;
			if (!this.eventLocks.TryGetValue(key, out locationEvents))
			{
				return;
			}
			long lockOwner;
			if (!locationEvents.TryGetValue(eventId, out lockOwner))
			{
				return;
			}
			if (lockOwner != message.SourceFarmer.UniqueMultiplayerID)
			{
				return;
			}
			Game1.player.eventsSeen.Add(eventId);
			locationEvents.Remove(eventId);
		}

		// Token: 0x060022E3 RID: 8931 RVA: 0x00178434 File Offset: 0x00176634
		private void ProcessHostAction(IncomingMessage message)
		{
			string actionName = message.Reader.ReadString();
			if (actionName == "ChooseCave")
			{
				Event.hostActionChooseCave(message.SourceFarmer, message.Reader);
				return;
			}
			if (actionName == "NamePet")
			{
				Event.hostActionNamePet(message.SourceFarmer, message.Reader);
				return;
			}
			if (!(actionName == "JudgeGrange"))
			{
				return;
			}
			this.shouldJudgeGrange = true;
		}

		// Token: 0x060022E4 RID: 8932 RVA: 0x001784A0 File Offset: 0x001766A0
		public void ProcessMessage(IncomingMessage message)
		{
			DedicatedServerMessageType dedicatedServerMessageType = (DedicatedServerMessageType)message.Reader.ReadByte();
			if (dedicatedServerMessageType == DedicatedServerMessageType.EventDone)
			{
				this.ProcessEventDone(message);
				return;
			}
			if (dedicatedServerMessageType != DedicatedServerMessageType.HostAction)
			{
				return;
			}
			this.ProcessHostAction(message);
		}

		// Token: 0x0400149C RID: 5276
		private const string BROADCAST_EVENT_KEY = "BroadcastEvent";

		// Token: 0x0400149D RID: 5277
		private readonly ConcurrentQueue<DedicatedServer.FarmerWarp> farmerWarps = new ConcurrentQueue<DedicatedServer.FarmerWarp>();

		// Token: 0x0400149E RID: 5278
		private readonly Dictionary<string, Dictionary<string, long>> eventLocks = new Dictionary<string, Dictionary<string, long>>();

		// Token: 0x0400149F RID: 5279
		private readonly HashSet<long> onlineIds = new HashSet<long>();

		// Token: 0x040014A0 RID: 5280
		private readonly HashSet<string> broadcastEvents = new HashSet<string>();

		// Token: 0x040014A1 RID: 5281
		private readonly HashSet<string> notBroadcastEvents = new HashSet<string>();

		// Token: 0x040014A2 RID: 5282
		private bool fakeWarp;

		// Token: 0x040014A3 RID: 5283
		private bool warpingSleep;

		// Token: 0x040014A4 RID: 5284
		private bool warpingFestival;

		// Token: 0x040014A5 RID: 5285
		private bool warpingHostBroadcastEvent;

		// Token: 0x040014A6 RID: 5286
		private bool startedFestivalMainEvent;

		// Token: 0x040014A7 RID: 5287
		private bool startedFestivalEnd;

		// Token: 0x040014A8 RID: 5288
		private bool shouldJudgeGrange;

		// Token: 0x040014A9 RID: 5289
		public bool CheckedHostPrecondition;

		// Token: 0x040014AA RID: 5290
		private long fakeFarmerId;

		// Token: 0x02000585 RID: 1413
		public class FarmerWarp
		{
			// Token: 0x060041B8 RID: 16824 RVA: 0x00308A83 File Offset: 0x00306C83
			public FarmerWarp(Farmer who, short x, short y, string name, bool isStructure, int facingDirection, bool warpingForForcedRemoteEvent)
			{
				this.who = who;
				this.name = name;
				this.facingDirection = facingDirection;
				this.x = x;
				this.y = y;
				this.isStructure = isStructure;
				this.warpingForForcedRemoteEvent = warpingForForcedRemoteEvent;
			}

			// Token: 0x04002BC0 RID: 11200
			public Farmer who;

			// Token: 0x04002BC1 RID: 11201
			public string name;

			// Token: 0x04002BC2 RID: 11202
			public int facingDirection;

			// Token: 0x04002BC3 RID: 11203
			public short x;

			// Token: 0x04002BC4 RID: 11204
			public short y;

			// Token: 0x04002BC5 RID: 11205
			public bool isStructure;

			// Token: 0x04002BC6 RID: 11206
			public bool warpingForForcedRemoteEvent;
		}
	}
}
