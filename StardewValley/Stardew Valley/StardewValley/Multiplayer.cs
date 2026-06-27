using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Netcode;
using StardewValley.Buildings;
using StardewValley.GameData.LocationContexts;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley
{
	// Token: 0x020000E1 RID: 225
	public class Multiplayer
	{
		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x060010EA RID: 4330 RVA: 0x000C86C8 File Offset: 0x000C68C8
		public static string protocolVersion
		{
			get
			{
				if (Multiplayer.protocolVersionOverride != null)
				{
					return Multiplayer.protocolVersionOverride;
				}
				return Game1.version + ((Game1.versionLabel != null) ? ("+" + new string(Game1.versionLabel.Where(new Func<char, bool>(char.IsLetterOrDigit)).ToArray<char>())) : "");
			}
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x000C8724 File Offset: 0x000C6924
		public Multiplayer()
		{
			this.playerLimit = 8;
		}

		// Token: 0x060010EC RID: 4332 RVA: 0x000C8790 File Offset: 0x000C6990
		public virtual long getNewID()
		{
			ulong seqNum = (this.latestID & 255UL) + 1UL & 255UL;
			ulong nodeID = (ulong)Game1.player.UniqueMultiplayerID;
			nodeID = (nodeID >> 32 ^ (nodeID & (ulong)-1));
			nodeID = ((nodeID >> 16 ^ (nodeID & 65535UL)) & 65535UL);
			ulong timestamp = (ulong)(DateTime.UtcNow.Ticks / 10000L);
			this.latestID = (timestamp << 24 | nodeID << 8 | seqNum);
			return (long)this.latestID;
		}

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x060010ED RID: 4333 RVA: 0x000C880A File Offset: 0x000C6A0A
		public virtual int MaxPlayers
		{
			get
			{
				if (Game1.server == null)
				{
					return 1;
				}
				return this.playerLimit;
			}
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x000C881B File Offset: 0x000C6A1B
		public virtual bool isDisconnecting(Farmer farmer)
		{
			return this.isDisconnecting(farmer.UniqueMultiplayerID);
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x000C8829 File Offset: 0x000C6A29
		public virtual bool isDisconnecting(long uid)
		{
			return this.disconnectingFarmers.Contains(uid);
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x000C8838 File Offset: 0x000C6A38
		public virtual bool isClientBroadcastType(byte messageType)
		{
			switch (messageType)
			{
			case 0:
			case 2:
			case 4:
			case 6:
			case 7:
			case 12:
			case 13:
			case 14:
			case 15:
			case 19:
			case 20:
			case 21:
			case 22:
			case 24:
			case 26:
				return true;
			}
			return false;
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x000C88BC File Offset: 0x000C6ABC
		public virtual bool allowSyncDelay()
		{
			return !Game1.newDaySync.hasInstance();
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x000C88CB File Offset: 0x000C6ACB
		public virtual int interpolationTicks()
		{
			if (!this.allowSyncDelay())
			{
				return 0;
			}
			if (LocalMultiplayer.IsLocalMultiplayer(true))
			{
				return 4;
			}
			return this.defaultInterpolationTicks;
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x000C88E7 File Offset: 0x000C6AE7
		public virtual IEnumerable<NetFarmerRoot> farmerRoots()
		{
			if (Game1.serverHost != null)
			{
				yield return Game1.serverHost;
			}
			foreach (NetRoot<Farmer> farmerRoot in Game1.otherFarmers.Roots.Values)
			{
				if (Game1.serverHost == null || farmerRoot != Game1.serverHost)
				{
					yield return farmerRoot as NetFarmerRoot;
				}
			}
			Dictionary<long, NetRoot<Farmer>>.ValueCollection.Enumerator enumerator = default(Dictionary<long, NetRoot<Farmer>>.ValueCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x000C88F0 File Offset: 0x000C6AF0
		public virtual NetFarmerRoot farmerRoot(long id)
		{
			if (Game1.serverHost != null && id == Game1.serverHost.Value.UniqueMultiplayerID)
			{
				return Game1.serverHost;
			}
			NetRoot<Farmer> otherFarmer;
			if (Game1.otherFarmers.Roots.TryGetValue(id, out otherFarmer))
			{
				return otherFarmer as NetFarmerRoot;
			}
			return null;
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x000C8940 File Offset: 0x000C6B40
		public virtual void broadcastFarmerDeltas()
		{
			foreach (NetFarmerRoot farmerRoot in this.farmerRoots())
			{
				if (farmerRoot.Dirty && Game1.player.UniqueMultiplayerID == farmerRoot.Value.UniqueMultiplayerID)
				{
					this.broadcastFarmerDelta(farmerRoot.Value, this.writeObjectDeltaBytes<Farmer>(farmerRoot));
				}
			}
			if (Game1.player.teamRoot.Dirty)
			{
				this.broadcastTeamDelta(this.writeObjectDeltaBytes<FarmerTeam>(Game1.player.teamRoot));
			}
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x000C89E0 File Offset: 0x000C6BE0
		protected virtual void broadcastTeamDelta(byte[] delta)
		{
			if (Game1.IsServer)
			{
				using (IEnumerator<Farmer> enumerator = Game1.otherFarmers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Farmer farmer = enumerator.Current;
						if (farmer != Game1.player)
						{
							Game1.server.sendMessage(farmer.UniqueMultiplayerID, 13, Game1.player, new object[]
							{
								delta
							});
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(13, new object[]
				{
					delta
				});
			}
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x000C8A78 File Offset: 0x000C6C78
		protected virtual void broadcastFarmerDelta(Farmer farmer, byte[] delta)
		{
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					v.Value.queueMessage(0, farmer, new object[]
					{
						farmer.UniqueMultiplayerID,
						delta
					});
				}
			}
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x000C8B04 File Offset: 0x000C6D04
		public void updateRoot<T>(T root) where T : INetRoot
		{
			foreach (long id in this.disconnectingFarmers)
			{
				root.Disconnect(id);
			}
			root.TickTree();
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x000C8B6C File Offset: 0x000C6D6C
		public virtual void updateRoots()
		{
			this.updateRoot<NetRoot<NetWorldState>>(Game1.netWorldState);
			foreach (NetFarmerRoot farmerRoot in this.farmerRoots())
			{
				farmerRoot.Clock.InterpolationTicks = this.interpolationTicks();
				this.updateRoot<NetFarmerRoot>(farmerRoot);
			}
			Game1.player.teamRoot.Clock.InterpolationTicks = this.interpolationTicks();
			this.updateRoot<NetRoot<FarmerTeam>>(Game1.player.teamRoot);
			if (Game1.IsClient)
			{
				using (IEnumerator<GameLocation> enumerator2 = this.activeLocations().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GameLocation location2 = enumerator2.Current;
						if (location2.Root != null && this._updatedRoots.Add(location2.Root.Value))
						{
							location2.Root.Clock.InterpolationTicks = this.interpolationTicks();
							this.updateRoot<NetRoot<GameLocation>>(location2.Root);
						}
					}
					goto IL_101;
				}
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.Root != null)
				{
					location.Root.Clock.InterpolationTicks = this.interpolationTicks();
					this.updateRoot<NetRoot<GameLocation>>(location.Root);
				}
				return true;
			}, false, true);
			IL_101:
			this._updatedRoots.Clear();
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x000C8CA4 File Offset: 0x000C6EA4
		public virtual void broadcastLocationDeltas()
		{
			if (Game1.IsClient)
			{
				using (IEnumerator<GameLocation> enumerator = this.activeLocations().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GameLocation location2 = enumerator.Current;
						if (!(location2.Root == null) && location2.Root.Dirty)
						{
							this.broadcastLocationDelta(location2);
						}
					}
					return;
				}
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.Root != null && location.Root.Dirty)
				{
					this.broadcastLocationDelta(location);
				}
				return true;
			}, false, true);
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x000C8D28 File Offset: 0x000C6F28
		public virtual void broadcastLocationDelta(GameLocation loc)
		{
			if (loc.Root == null || !loc.Root.Dirty)
			{
				return;
			}
			byte[] delta = this.writeObjectDeltaBytes<GameLocation>(loc.Root);
			this.broadcastLocationBytes(loc, 6, delta);
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x000C8D68 File Offset: 0x000C6F68
		protected virtual void broadcastLocationBytes(GameLocation loc, byte messageType, byte[] bytes)
		{
			OutgoingMessage message = new OutgoingMessage(messageType, Game1.player, new object[]
			{
				loc.isStructure.Value,
				loc.NameOrUniqueName,
				bytes
			});
			this.broadcastLocationMessage(loc, message);
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x000C8DB0 File Offset: 0x000C6FB0
		protected virtual void broadcastLocationMessage(GameLocation loc, OutgoingMessage message)
		{
			Multiplayer.<>c__DisplayClass78_0 CS$<>8__locals1;
			CS$<>8__locals1.message = message;
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(CS$<>8__locals1.message);
				return;
			}
			if (this.isAlwaysActiveLocation(loc))
			{
				using (IEnumerator<Farmer> enumerator = Game1.otherFarmers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Farmer f = enumerator.Current;
						Multiplayer.<broadcastLocationMessage>g__TellFarmer|78_0(f, ref CS$<>8__locals1);
					}
					return;
				}
			}
			foreach (Farmer f2 in loc.farmers)
			{
				Multiplayer.<broadcastLocationMessage>g__TellFarmer|78_0(f2, ref CS$<>8__locals1);
			}
			foreach (Building building in loc.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null)
				{
					foreach (Farmer f3 in indoors.farmers)
					{
						Multiplayer.<broadcastLocationMessage>g__TellFarmer|78_0(f3, ref CS$<>8__locals1);
					}
				}
			}
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x000C8EF4 File Offset: 0x000C70F4
		public virtual void broadcastSprites(GameLocation location, TemporaryAnimatedSpriteList sprites)
		{
			this.broadcastSprites(location, sprites.ToArray<TemporaryAnimatedSprite>());
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x000C8F04 File Offset: 0x000C7104
		public virtual void broadcastSprites(GameLocation location, params TemporaryAnimatedSprite[] sprites)
		{
			location.temporarySprites.AddRange(sprites);
			if (sprites.Length == 0 || !Game1.IsMultiplayer)
			{
				return;
			}
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = this.createWriter(stream))
				{
					writer.Push("TemporaryAnimatedSprites");
					writer.Write(sprites.Length);
					for (int i = 0; i < sprites.Length; i++)
					{
						sprites[i].Write(writer, location);
					}
					writer.Pop();
				}
				this.broadcastLocationBytes(location, 7, stream.ToArray());
			}
		}

		// Token: 0x06001100 RID: 4352 RVA: 0x000C8FB0 File Offset: 0x000C71B0
		public virtual void broadcastWorldStateDeltas()
		{
			if (!Game1.netWorldState.Dirty)
			{
				return;
			}
			byte[] delta = this.writeObjectDeltaBytes<NetWorldState>(Game1.netWorldState);
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				if (v.Value != Game1.player)
				{
					v.Value.queueMessage(12, Game1.player, new object[]
					{
						delta
					});
				}
			}
		}

		// Token: 0x06001101 RID: 4353 RVA: 0x000C9040 File Offset: 0x000C7240
		public virtual void receiveWorldState(BinaryReader msg)
		{
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			this.readObjectDelta<NetWorldState>(msg, Game1.netWorldState);
			Game1.netWorldState.TickTree();
			int origTime = Game1.timeOfDay;
			Game1.netWorldState.Value.WriteToGame1(false);
			if (!Game1.IsServer && origTime != Game1.timeOfDay && Game1.currentLocation != null && !Game1.newDaySync.hasInstance())
			{
				Game1.performTenMinuteClockUpdate();
			}
		}

		// Token: 0x06001102 RID: 4354 RVA: 0x000C90B0 File Offset: 0x000C72B0
		public virtual void requestCharacterWarp(NPC character, GameLocation targetLocation, Vector2 position)
		{
			if (!Game1.IsClient)
			{
				return;
			}
			GameLocation loc = character.currentLocation;
			if (loc == null)
			{
				throw new ArgumentException("In warpCharacter, the character's currentLocation must not be null");
			}
			Guid characterGuid = loc.characters.GuidOf(character);
			if (characterGuid == Guid.Empty)
			{
				throw new ArgumentException("In warpCharacter, the character must be in its currentLocation");
			}
			OutgoingMessage message = new OutgoingMessage(8, Game1.player, new object[]
			{
				loc.isStructure.Value,
				loc.NameOrUniqueName,
				characterGuid,
				targetLocation.isStructure.Value,
				targetLocation.NameOrUniqueName,
				position
			});
			Game1.serverHost.Value.queueMessage(message);
		}

		// Token: 0x06001103 RID: 4355 RVA: 0x000C916C File Offset: 0x000C736C
		public virtual NetRoot<GameLocation> locationRoot(GameLocation location)
		{
			if (location.Root == null && Game1.IsMasterGame)
			{
				new NetRoot<GameLocation>().Set(location);
				location.Root.Clock.InterpolationTicks = this.interpolationTicks();
				location.Root.MarkClean();
			}
			return location.Root;
		}

		// Token: 0x06001104 RID: 4356 RVA: 0x000C91C0 File Offset: 0x000C73C0
		public virtual void sendPassoutRequest()
		{
			object[] message = new object[]
			{
				Game1.player.UniqueMultiplayerID
			};
			if (Game1.IsMasterGame)
			{
				this._receivePassoutRequest(Game1.player);
				return;
			}
			Game1.client.sendMessage(28, message);
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x000C9208 File Offset: 0x000C7408
		public virtual void receivePassoutRequest(IncomingMessage msg)
		{
			if (Game1.IsServer)
			{
				Farmer farmer = Game1.GetPlayer(msg.Reader.ReadInt64(), false);
				if (farmer != null)
				{
					this._receivePassoutRequest(farmer);
				}
			}
		}

		// Token: 0x06001106 RID: 4358 RVA: 0x000C9238 File Offset: 0x000C7438
		protected virtual void _receivePassoutRequest(Farmer farmer)
		{
			if (Game1.IsMasterGame)
			{
				GameLocation lastSleepLocation = (farmer.lastSleepLocation.Value != null && Game1.isLocationAccessible(farmer.lastSleepLocation.Value)) ? Game1.getLocationFromName(farmer.lastSleepLocation.Value) : null;
				bool? flag = (lastSleepLocation != null) ? new bool?(lastSleepLocation.CanWakeUpHere(farmer, null)) : null;
				if (flag != null && flag.GetValueOrDefault() && lastSleepLocation.GetLocationContextId() == farmer.currentLocation.GetLocationContextId())
				{
					if (Game1.IsServer && farmer != Game1.player)
					{
						object[] message = new object[]
						{
							farmer.lastSleepLocation.Value,
							farmer.lastSleepPoint.X,
							farmer.lastSleepPoint.Y,
							true
						};
						Game1.server.sendMessage(farmer.UniqueMultiplayerID, 29, Game1.player, message.ToArray<object>());
						return;
					}
					Farmer.performPassoutWarp(farmer, farmer.lastSleepLocation.Value, farmer.lastSleepPoint.Value, true);
					return;
				}
				else
				{
					FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(farmer);
					string wakeup_location = homeOfFarmer.NameOrUniqueName;
					Point wakeup_point = homeOfFarmer.GetPlayerBedSpot();
					bool has_bed = homeOfFarmer.GetPlayerBed() != null;
					GameLocation currentLocation = farmer.currentLocation;
					List<ReviveLocation> wakeUpLocations = ((currentLocation != null) ? currentLocation.GetLocationContext().PassOutLocations : null) ?? LocationContexts.Default.PassOutLocations;
					if (wakeUpLocations != null)
					{
						foreach (ReviveLocation wakeUpLocation in wakeUpLocations)
						{
							if (GameStateQuery.CheckConditions(wakeUpLocation.Condition, farmer.currentLocation, farmer, null, null, null, null))
							{
								GameLocation location = Game1.getLocationFromName(wakeUpLocation.Location);
								if (location == null)
								{
									break;
								}
								wakeup_location = wakeUpLocation.Location;
								wakeup_point = wakeUpLocation.Position;
								has_bed = false;
								using (List<Furniture>.Enumerator enumerator2 = location.furniture.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										Furniture furniture = enumerator2.Current;
										BedFurniture bed = furniture as BedFurniture;
										if (bed != null && bed.bedType != BedFurniture.BedType.Child)
										{
											wakeup_point = bed.GetBedSpot();
											has_bed = true;
											break;
										}
									}
									break;
								}
							}
						}
					}
					if (Game1.IsServer && farmer != Game1.player)
					{
						object[] message2 = new object[]
						{
							wakeup_location,
							wakeup_point.X,
							wakeup_point.Y,
							has_bed
						};
						Game1.server.sendMessage(farmer.UniqueMultiplayerID, 29, Game1.player, message2.ToArray<object>());
						return;
					}
					Farmer.performPassoutWarp(farmer, wakeup_location, wakeup_point, has_bed);
				}
			}
		}

		// Token: 0x06001107 RID: 4359 RVA: 0x000C950C File Offset: 0x000C770C
		public virtual void receivePassout(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				string wakeup_location = msg.Reader.ReadString();
				Point wakeup_point = new Point(msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
				bool has_bed = msg.Reader.ReadBoolean();
				Farmer.performPassoutWarp(Game1.player, wakeup_location, wakeup_point, has_bed);
			}
		}

		// Token: 0x06001108 RID: 4360 RVA: 0x000C9570 File Offset: 0x000C7770
		public virtual object[] generateForceEventMessage(string eventId, GameLocation location, int tileX, int tileY, bool use_local_farmer, bool notify_when_done)
		{
			return new object[]
			{
				eventId,
				use_local_farmer,
				notify_when_done,
				tileX,
				tileY,
				(location.isStructure.Value > false) ? 1 : 0,
				location.NameOrUniqueName
			};
		}

		// Token: 0x06001109 RID: 4361 RVA: 0x000C95D0 File Offset: 0x000C77D0
		public virtual void broadcastEvent(Event evt, GameLocation location, Vector2 positionBeforeEvent, bool use_local_farmer = true, bool notify_when_done = false)
		{
			if (string.IsNullOrEmpty(evt.id) || evt.id == "-1")
			{
				return;
			}
			object[] message = this.generateForceEventMessage(evt.id, location, (int)positionBeforeEvent.X, (int)positionBeforeEvent.Y, use_local_farmer, notify_when_done);
			if (Game1.IsServer)
			{
				using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<long, Farmer> v = enumerator.Current;
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 4, Game1.dedicatedServer.FakeFarmer, message);
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(4, message);
			}
		}

		// Token: 0x0600110A RID: 4362 RVA: 0x000C96B0 File Offset: 0x000C78B0
		protected virtual void receiveRequestGrandpaReevaluation(IncomingMessage msg)
		{
			Farm farm = Game1.getFarm();
			if (farm == null)
			{
				return;
			}
			farm.requestGrandpaReevaluation();
		}

		// Token: 0x0600110B RID: 4363 RVA: 0x000C96C4 File Offset: 0x000C78C4
		protected virtual void receiveFarmerKilledMonster(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				string which = msg.Reader.ReadString();
				if (which != null)
				{
					Game1.stats.monsterKilled(which);
				}
			}
		}

		// Token: 0x0600110C RID: 4364 RVA: 0x000C9700 File Offset: 0x000C7900
		public virtual void broadcastRemoveLocationFromLookup(GameLocation location)
		{
			List<object> message = new List<object>();
			message.Add(location.NameOrUniqueName);
			if (Game1.IsServer)
			{
				using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<long, Farmer> v = enumerator.Current;
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 24, Game1.player, message.ToArray());
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(24, message.ToArray());
			}
		}

		// Token: 0x0600110D RID: 4365 RVA: 0x000C97B8 File Offset: 0x000C79B8
		public virtual void broadcastNutDig(GameLocation location, Point point)
		{
			if (Game1.IsMasterGame)
			{
				this._performNutDig(location, point);
				return;
			}
			List<object> message = new List<object>();
			message.Add(location.NameOrUniqueName);
			message.Add(point.X);
			message.Add(point.Y);
			Game1.client.sendMessage(27, message.ToArray());
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x000C981C File Offset: 0x000C7A1C
		protected virtual void receiveNutDig(IncomingMessage msg)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			string name = msg.Reader.ReadString();
			Point point = new Point(msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
			GameLocation location = Game1.getLocationFromName(name);
			this._performNutDig(location, point);
		}

		// Token: 0x0600110F RID: 4367 RVA: 0x000C9868 File Offset: 0x000C7A68
		protected virtual void _performNutDig(GameLocation location, Point point)
		{
			IslandLocation island_location = location as IslandLocation;
			if (island_location != null && island_location.IsBuriedNutLocation(point))
			{
				string key = string.Concat(new string[]
				{
					location.NameOrUniqueName,
					"_",
					point.X.ToString(),
					"_",
					point.Y.ToString()
				});
				if (Game1.netWorldState.Value.FoundBuriedNuts.Add(key))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2((float)point.X, (float)point.Y) * 64f, -1, island_location, -1, false);
				}
			}
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x000C991C File Offset: 0x000C7B1C
		public virtual void broadcastPartyWideMail(string mail_key, Multiplayer.PartyWideMessageQueue message_queue = Multiplayer.PartyWideMessageQueue.MailForTomorrow, bool no_letter = false)
		{
			mail_key = mail_key.Trim();
			mail_key = mail_key.Replace(Environment.NewLine, "");
			List<object> message = new List<object>();
			message.Add(mail_key);
			message.Add((int)message_queue);
			message.Add(no_letter);
			this._performPartyWideMail(mail_key, message_queue, no_letter);
			if (Game1.IsServer)
			{
				using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<long, Farmer> v = enumerator.Current;
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 22, Game1.player, message.ToArray());
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(22, message.ToArray());
			}
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x000C9A0C File Offset: 0x000C7C0C
		public virtual void broadcastGrandpaReevaluation()
		{
			Game1.getFarm().requestGrandpaReevaluation();
			if (Game1.IsServer)
			{
				using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<long, Farmer> v = enumerator.Current;
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 26, Game1.player, Array.Empty<object>());
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(26, Array.Empty<object>());
			}
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x000C9ABC File Offset: 0x000C7CBC
		public virtual void broadcastGlobalMessage(string translationKey, bool onlyShowIfEmpty = false, GameLocation location = null, params string[] substitutions)
		{
			if (!onlyShowIfEmpty || Game1.hudMessages.Count == 0)
			{
				if (location != null)
				{
					string nameOrUniqueName = location.NameOrUniqueName;
					GameLocation currentLocation = Game1.player.currentLocation;
					if (!(nameOrUniqueName == ((currentLocation != null) ? currentLocation.NameOrUniqueName : null)))
					{
						goto IL_70;
					}
				}
				string[] parsedTokens = new string[substitutions.Length];
				for (int i = 0; i < substitutions.Length; i++)
				{
					parsedTokens[i] = TokenParser.ParseText(substitutions[i], null, null, null);
				}
				LocalizedContentManager content = Game1.content;
				object[] substitutions2 = parsedTokens;
				Game1.showGlobalMessage(content.LoadString(translationKey, substitutions2));
			}
			IL_70:
			List<object> message = new List<object>
			{
				translationKey,
				onlyShowIfEmpty,
				((location != null) ? location.NameOrUniqueName : null) ?? "",
				substitutions.Length
			};
			message.AddRange(substitutions);
			if (Game1.IsServer)
			{
				using (NetRootDictionary<long, Farmer>.Enumerator enumerator = Game1.otherFarmers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<long, Farmer> v = enumerator.Current;
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 21, Game1.player, message.ToArray());
						}
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(21, message.ToArray());
			}
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x000C9C20 File Offset: 0x000C7E20
		public virtual NetRoot<T> readObjectFull<T>(BinaryReader reader) where T : class, INetObject<INetSerializable>
		{
			NetRoot<T> netRoot = NetRoot<T>.Connect(reader);
			netRoot.Clock.InterpolationTicks = this.defaultInterpolationTicks;
			return netRoot;
		}

		// Token: 0x06001114 RID: 4372 RVA: 0x000C9C3C File Offset: 0x000C7E3C
		protected virtual BinaryWriter createWriter(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (this.logging.IsLogging)
			{
				writer = new LoggingBinaryWriter(writer);
			}
			return writer;
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x000C9C65 File Offset: 0x000C7E65
		public virtual void writeObjectFull<T>(BinaryWriter writer, NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			root.CreateConnectionPacket(writer, peer);
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x000C9C70 File Offset: 0x000C7E70
		public virtual byte[] writeObjectFullBytes<T>(NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			byte[] result;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = this.createWriter(stream))
				{
					root.CreateConnectionPacket(writer, peer);
					result = stream.ToArray();
				}
			}
			return result;
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x000C9CD0 File Offset: 0x000C7ED0
		public virtual void readObjectDelta<T>(BinaryReader reader, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Read(reader);
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x000C9CD9 File Offset: 0x000C7ED9
		public virtual void writeObjectDelta<T>(BinaryWriter writer, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Write(writer);
		}

		// Token: 0x06001119 RID: 4377 RVA: 0x000C9CE4 File Offset: 0x000C7EE4
		public virtual byte[] writeObjectDeltaBytes<T>(NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			byte[] result;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = this.createWriter(stream))
				{
					root.Write(writer);
					result = stream.ToArray();
				}
			}
			return result;
		}

		// Token: 0x0600111A RID: 4378 RVA: 0x000C9D40 File Offset: 0x000C7F40
		public virtual NetFarmerRoot readFarmer(BinaryReader reader)
		{
			NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
			netFarmerRoot.ReadConnectionPacket(reader);
			netFarmerRoot.Clock.InterpolationTicks = this.defaultInterpolationTicks;
			return netFarmerRoot;
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x000C9D60 File Offset: 0x000C7F60
		public virtual void addPlayer(NetFarmerRoot f)
		{
			long id = f.Value.UniqueMultiplayerID;
			f.Value.teamRoot = Game1.player.teamRoot;
			Game1.otherFarmers.Roots[id] = f;
			this.disconnectingFarmers.Remove(id);
			if (Game1.chatBox != null)
			{
				string farmerName = ChatBox.formattedUserNameLong(f.Value);
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerJoined", farmerName));
			}
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x000C9DD9 File Offset: 0x000C7FD9
		public virtual void receivePlayerIntroduction(BinaryReader reader)
		{
			this.addPlayer(this.readFarmer(reader));
		}

		// Token: 0x0600111D RID: 4381 RVA: 0x000C9DE8 File Offset: 0x000C7FE8
		public virtual void broadcastPlayerIntroduction(NetFarmerRoot farmerRoot)
		{
			if (Game1.server == null)
			{
				return;
			}
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				if (farmerRoot.Value.UniqueMultiplayerID != v.Value.UniqueMultiplayerID)
				{
					Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 2, farmerRoot.Value, new object[]
					{
						Game1.server.getUserName(farmerRoot.Value.UniqueMultiplayerID),
						this.writeObjectFullBytes<Farmer>(farmerRoot, new long?(v.Value.UniqueMultiplayerID))
					});
				}
			}
		}

		// Token: 0x0600111E RID: 4382 RVA: 0x000C9EAC File Offset: 0x000C80AC
		public virtual void broadcastUserName(long farmerId, string userName)
		{
			if (Game1.server != null)
			{
				return;
			}
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				Farmer farmer = v.Value;
				if (farmer.UniqueMultiplayerID != farmerId)
				{
					Game1.server.sendMessage(farmer.UniqueMultiplayerID, 16, Game1.serverHost.Value, new object[]
					{
						farmerId,
						userName
					});
				}
			}
		}

		// Token: 0x0600111F RID: 4383 RVA: 0x000C9F40 File Offset: 0x000C8140
		public virtual string getUserName(long id)
		{
			if (id == Game1.player.UniqueMultiplayerID)
			{
				return Game1.content.LoadString("Strings\\UI:Chat_SelfPlayerID");
			}
			if (Game1.server != null)
			{
				return Game1.server.getUserName(id);
			}
			if (Game1.client != null)
			{
				return Game1.client.getUserName(id);
			}
			return "?";
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x000C9F98 File Offset: 0x000C8198
		public virtual void playerDisconnected(long id)
		{
			NetRoot<Farmer> otherFarmer;
			if (Game1.otherFarmers.Roots.TryGetValue(id, out otherFarmer) && !this.disconnectingFarmers.Contains(id))
			{
				NetFarmerRoot farmhand = otherFarmer as NetFarmerRoot;
				if (farmhand.Value.mount != null && Game1.IsMasterGame)
				{
					farmhand.Value.mount.dismount(false);
				}
				if (Game1.IsMasterGame)
				{
					farmhand.TargetValue.handleDisconnect();
					farmhand.TargetValue.companions.Clear();
					this.saveFarmhand(farmhand);
					farmhand.Value.handleDisconnect();
				}
				if (Game1.player.dancePartner.Value is Farmer && ((Farmer)Game1.player.dancePartner.Value).UniqueMultiplayerID == farmhand.Value.UniqueMultiplayerID)
				{
					Game1.player.dancePartner.Value = null;
				}
				if (Game1.chatBox != null)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerLeft", ChatBox.formattedUserNameLong(Game1.otherFarmers[id])));
				}
				this.disconnectingFarmers.Add(id);
			}
		}

		// Token: 0x06001121 RID: 4385 RVA: 0x000CA0B8 File Offset: 0x000C82B8
		protected virtual void removeDisconnectedFarmers()
		{
			foreach (long id in this.disconnectingFarmers)
			{
				Game1.otherFarmers.Remove(id);
			}
			this.disconnectingFarmers.Clear();
		}

		// Token: 0x06001122 RID: 4386 RVA: 0x000CA11C File Offset: 0x000C831C
		public virtual void sendFarmhand()
		{
			(Game1.player.NetFields.Root as NetFarmerRoot).MarkReassigned();
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x000CA137 File Offset: 0x000C8337
		protected virtual void saveFarmhand(NetFarmerRoot farmhand)
		{
			Game1.netWorldState.Value.SaveFarmhand(farmhand);
		}

		// Token: 0x06001124 RID: 4388 RVA: 0x000CA14C File Offset: 0x000C834C
		public virtual void saveFarmhands()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			foreach (NetRoot<Farmer> farmer in Game1.otherFarmers.Roots.Values)
			{
				this.saveFarmhand(farmer as NetFarmerRoot);
			}
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x000CA1B8 File Offset: 0x000C83B8
		public virtual void clientRemotelyDisconnected(Multiplayer.DisconnectType disconnectType)
		{
			Multiplayer.LogDisconnect(disconnectType);
			this.returnToMainMenu();
		}

		// Token: 0x06001126 RID: 4390 RVA: 0x000CA1C8 File Offset: 0x000C83C8
		private void returnToMainMenu()
		{
			if (!Game1.game1.IsMainInstance)
			{
				GameRunner.instance.RemoveGameInstance(Game1.game1);
				return;
			}
			Game1.ExitToTitle(delegate
			{
				(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
				TitleMenu.subMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\UI:Client_RemotelyDisconnected"), null, null)
				{
					okButton = 
					{
						visible = false
					}
				};
			});
		}

		// Token: 0x06001127 RID: 4391 RVA: 0x000CA218 File Offset: 0x000C8418
		public static bool ShouldLogDisconnect(Multiplayer.DisconnectType disconnectType)
		{
			switch (disconnectType)
			{
			case Multiplayer.DisconnectType.ClosedGame:
			case Multiplayer.DisconnectType.ExitedToMainMenu:
			case Multiplayer.DisconnectType.ExitedToMainMenu_FromFarmhandSelect:
			case Multiplayer.DisconnectType.ServerOfflineMode:
			case Multiplayer.DisconnectType.ServerFull:
			case Multiplayer.DisconnectType.AcceptedOtherInvite:
				return false;
			}
			return true;
		}

		// Token: 0x06001128 RID: 4392 RVA: 0x000CA266 File Offset: 0x000C8466
		public static bool IsTimeout(Multiplayer.DisconnectType disconnectType)
		{
			return disconnectType - Multiplayer.DisconnectType.ClientTimeout <= 2;
		}

		// Token: 0x06001129 RID: 4393 RVA: 0x000CA274 File Offset: 0x000C8474
		public static void LogDisconnect(Multiplayer.DisconnectType disconnectType)
		{
			if (Multiplayer.ShouldLogDisconnect(disconnectType))
			{
				string message = "Disconnected at : " + DateTime.Now.ToLongTimeString() + " - " + disconnectType.ToString();
				if (Game1.client != null)
				{
					message = message + " Ping: " + Game1.client.GetPingToHost().ToString("0.#");
					message += ((Game1.client is LidgrenClient) ? " ip" : " friend/invite");
				}
				Program.WriteLog(Program.LogType.Disconnect, message, true);
			}
			Game1.log.Verbose("Disconnected: " + disconnectType.ToString());
		}

		// Token: 0x0600112A RID: 4394 RVA: 0x000CA328 File Offset: 0x000C8528
		public virtual void sendSharedAchievementMessage(int achievement)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(20, new object[]
				{
					achievement
				});
				return;
			}
			if (Game1.IsServer)
			{
				foreach (long id in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(id, 20, Game1.player, new object[]
					{
						achievement
					});
				}
			}
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x000CA3C0 File Offset: 0x000C85C0
		public virtual void sendServerToClientsMessage(string message)
		{
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					v.Value.queueMessage(18, Game1.player, new object[]
					{
						message
					});
				}
			}
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x000CA430 File Offset: 0x000C8630
		public virtual void sendChatMessage(LocalizedContentManager.LanguageCode language, string message, long recipientID)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(10, new object[]
				{
					recipientID,
					language,
					message
				});
				return;
			}
			if (Game1.IsServer)
			{
				if (recipientID == Multiplayer.AllPlayers)
				{
					using (IEnumerator<long> enumerator = Game1.otherFarmers.Keys.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							long id = enumerator.Current;
							Game1.server.sendMessage(id, 10, Game1.player, new object[]
							{
								recipientID,
								language,
								message
							});
						}
						return;
					}
				}
				Game1.server.sendMessage(recipientID, 10, Game1.player, new object[]
				{
					recipientID,
					language,
					message
				});
			}
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x000CA518 File Offset: 0x000C8718
		public virtual void receiveChatMessage(Farmer sourceFarmer, long recipientID, LocalizedContentManager.LanguageCode language, string message)
		{
			if (Game1.chatBox != null)
			{
				int messageType = 0;
				message = Program.sdk.FilterDirtyWords(message);
				if (recipientID != Multiplayer.AllPlayers)
				{
					messageType = 3;
				}
				Game1.chatBox.receiveChatMessage(sourceFarmer.UniqueMultiplayerID, messageType, language, message);
			}
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x000CA55A File Offset: 0x000C875A
		public virtual void globalChatInfoMessage(string messageKey, params string[] args)
		{
			if (!Game1.IsMultiplayer && Game1.multiplayerMode == 0)
			{
				return;
			}
			this.receiveChatInfoMessage(Game1.player, messageKey, args);
			this.sendChatInfoMessage(messageKey, args);
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x000CA580 File Offset: 0x000C8780
		public void globalChatInfoMessageEvenInSinglePlayer(string messageKey, params string[] args)
		{
			this.receiveChatInfoMessage(Game1.player, messageKey, args);
			this.sendChatInfoMessage(messageKey, args);
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x000CA598 File Offset: 0x000C8798
		protected virtual void sendChatInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(15, new object[]
				{
					messageKey,
					args
				});
				return;
			}
			if (Game1.IsServer)
			{
				foreach (long id in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(id, 15, Game1.player, new object[]
					{
						messageKey,
						args
					});
				}
			}
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x000CA62C File Offset: 0x000C882C
		protected virtual void receiveChatInfoMessage(Farmer sourceFarmer, string messageKey, string[] args)
		{
			if (Game1.chatBox != null)
			{
				try
				{
					string[] processedArgs = args.Select(delegate(string arg)
					{
						if (arg.StartsWith("aOrAn:"))
						{
							return Utility.AOrAn(TokenParser.ParseText(arg.Substring("aOrAn:".Length), null, null, null));
						}
						return TokenParser.ParseText(arg, null, null, null);
					}).ToArray<string>();
					ChatBox chatBox = Game1.chatBox;
					LocalizedContentManager content = Game1.content;
					string path = "Strings\\UI:Chat_" + messageKey;
					object[] substitutions = processedArgs;
					chatBox.addInfoMessage(content.LoadString(path, substitutions));
				}
				catch (ContentLoadException)
				{
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
				catch (KeyNotFoundException)
				{
				}
			}
		}

		// Token: 0x06001132 RID: 4402 RVA: 0x000CA6D0 File Offset: 0x000C88D0
		public virtual void parseServerToClientsMessage(string message)
		{
			if (Game1.IsClient)
			{
				if (!(message == "festivalEvent"))
				{
					if (!(message == "endFest"))
					{
						if (!(message == "trainApproach"))
						{
							return;
						}
						Railroad railroad = Game1.getLocationFromName("Railroad") as Railroad;
						if (railroad != null)
						{
							railroad.PlayTrainApproach();
						}
					}
					else if (Game1.CurrentEvent != null)
					{
						Game1.CurrentEvent.forceEndFestival(Game1.player);
						return;
					}
				}
				else if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.forceFestivalContinue();
					return;
				}
			}
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x000CA758 File Offset: 0x000C8958
		public virtual IEnumerable<GameLocation> activeLocations()
		{
			if (Game1.currentLocation != null)
			{
				yield return Game1.currentLocation;
			}
			foreach (GameLocation location in Game1.locations)
			{
				if (this.isAlwaysActiveLocation(location))
				{
					foreach (GameLocation activeLocation in this._GetActiveLocationsHere(location))
					{
						yield return activeLocation;
					}
					IEnumerator<GameLocation> enumerator2 = null;
				}
			}
			IEnumerator<GameLocation> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x000CA768 File Offset: 0x000C8968
		protected virtual IEnumerable<GameLocation> _GetActiveLocationsHere(GameLocation location)
		{
			if (location != Game1.currentLocation)
			{
				yield return location;
			}
			foreach (Building building in location.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null && (!indoors.isAlwaysActive.Value || building.GetIndoorsType() != IndoorsType.Global))
				{
					foreach (GameLocation childLocation in this._GetActiveLocationsHere(indoors))
					{
						yield return childLocation;
					}
					IEnumerator<GameLocation> enumerator2 = null;
				}
			}
			List<Building>.Enumerator enumerator = default(List<Building>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x000CA77F File Offset: 0x000C897F
		public virtual bool isAlwaysActiveLocation(GameLocation location)
		{
			return (location.Root != null && location.Root.Value != location && this.isAlwaysActiveLocation(location.Root.Value)) || location.isAlwaysActive.Value;
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x000CA7C0 File Offset: 0x000C89C0
		protected virtual void readActiveLocation(IncomingMessage msg)
		{
			bool force_current_location = msg.Reader.ReadBoolean();
			NetRoot<GameLocation> root = this.readObjectFull<GameLocation>(msg.Reader);
			if (this.isAlwaysActiveLocation(root.Value))
			{
				int i = 0;
				while (i < Game1.locations.Count)
				{
					GameLocation local = Game1.locations[i];
					if (local.Equals(root.Value))
					{
						if (local != root.Value)
						{
							if (local != null)
							{
								if (Game1.currentLocation == local)
								{
									Game1.currentLocation = root.Value;
								}
								if (Game1.player.currentLocation == local)
								{
									Game1.player.currentLocation = root.Value;
								}
								Game1.removeLocationFromLocationLookup(local);
								local.OnRemoved();
							}
							Game1.locations[i] = root.Value;
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
			if (Game1.locationRequest != null || force_current_location)
			{
				if (Game1.locationRequest != null)
				{
					Game1.currentLocation = (Game1.findStructure(root.Value, Game1.locationRequest.Name) ?? root.Value);
				}
				else if (force_current_location)
				{
					Game1.currentLocation = root.Value;
				}
				if (Game1.locationRequest != null)
				{
					Game1.locationRequest.Location = Game1.currentLocation;
					Game1.locationRequest.Loaded(Game1.currentLocation);
				}
				if (Game1.client == null && Game1.activeClickableMenu is TitleMenu)
				{
					FarmhandMenu farmhandMenu = TitleMenu.subMenu as FarmhandMenu;
					if (((farmhandMenu != null) ? farmhandMenu.client : null) != null)
					{
						goto IL_15B;
					}
				}
				Game1.currentLocation.resetForPlayerEntry();
				IL_15B:
				Game1.player.currentLocation = Game1.currentLocation;
				LocationRequest locationRequest = Game1.locationRequest;
				if (locationRequest != null)
				{
					locationRequest.Warped(Game1.currentLocation);
				}
				Game1.currentLocation.updateSeasonalTileSheets(null);
				Game1.locationRequest = null;
			}
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x000CA960 File Offset: 0x000C8B60
		public virtual bool isActiveLocation(GameLocation location)
		{
			return Game1.IsMasterGame || (((location != null) ? location.Root : null) != null && ((Game1.currentLocation != null && Game1.currentLocation.Root != null && Game1.currentLocation.Root.Value == location.Root.Value) || this.isAlwaysActiveLocation(location)));
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x000CA9C4 File Offset: 0x000C8BC4
		protected virtual GameLocation readLocation(BinaryReader reader)
		{
			bool structure = reader.ReadByte() > 0;
			GameLocation location = Game1.getLocationFromName(reader.ReadString(), structure);
			if (location == null || this.locationRoot(location) == null)
			{
				return null;
			}
			if (!this.isActiveLocation(location))
			{
				return null;
			}
			return location;
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x000CAA08 File Offset: 0x000C8C08
		protected virtual LocationRequest readLocationRequest(BinaryReader reader)
		{
			bool structure = reader.ReadByte() > 0;
			return Game1.getLocationRequest(reader.ReadString(), structure);
		}

		// Token: 0x0600113A RID: 4410 RVA: 0x000CAA2C File Offset: 0x000C8C2C
		protected virtual NPC readNPC(BinaryReader reader)
		{
			GameLocation gameLocation = this.readLocation(reader);
			Guid guid = reader.ReadGuid();
			NPC npc;
			if (!gameLocation.characters.TryGetValue(guid, out npc))
			{
				return null;
			}
			return npc;
		}

		// Token: 0x0600113B RID: 4411 RVA: 0x000CAA5C File Offset: 0x000C8C5C
		public virtual void readSprites(BinaryReader reader, GameLocation location, Action<TemporaryAnimatedSprite> assignSprite)
		{
			int count = reader.ReadInt32();
			TemporaryAnimatedSprite[] result = new TemporaryAnimatedSprite[count];
			for (int i = 0; i < count; i++)
			{
				TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite();
				sprite.Read(reader, location);
				sprite.ticksBeforeAnimationStart += this.interpolationTicks();
				result[i] = sprite;
				assignSprite(sprite);
			}
		}

		// Token: 0x0600113C RID: 4412 RVA: 0x000CAAAF File Offset: 0x000C8CAF
		protected virtual void receiveTeamDelta(BinaryReader msg)
		{
			this.readObjectDelta<FarmerTeam>(msg, Game1.player.teamRoot);
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x000CAAC4 File Offset: 0x000C8CC4
		protected virtual void receiveNewDaySync(IncomingMessage msg)
		{
			if (!Game1.newDaySync.hasInstance() && msg.SourceFarmer == Game1.serverHost.Value)
			{
				Game1.NewDay(0f);
			}
			if (Game1.newDaySync.hasInstance())
			{
				Game1.newDaySync.receiveMessage(msg);
			}
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x000CAB10 File Offset: 0x000C8D10
		protected virtual void receiveFarmerGainExperience(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				int which = msg.Reader.ReadInt32();
				int howMuch = msg.Reader.ReadInt32();
				Game1.player.gainExperience(which, howMuch);
			}
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x000CAB53 File Offset: 0x000C8D53
		protected virtual void receiveSharedAchievement(IncomingMessage msg)
		{
			Game1.getAchievement(msg.Reader.ReadInt32(), false);
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x000CAB66 File Offset: 0x000C8D66
		protected virtual void receiveRemoveLocationFromLookup(IncomingMessage msg)
		{
			Game1.removeLocationFromLocationLookup(msg.Reader.ReadString());
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x000CAB78 File Offset: 0x000C8D78
		protected virtual void receivePartyWideMail(IncomingMessage msg)
		{
			string mail_key = msg.Reader.ReadString();
			Multiplayer.PartyWideMessageQueue message_queue = (Multiplayer.PartyWideMessageQueue)msg.Reader.ReadInt32();
			bool no_letter = msg.Reader.ReadBoolean();
			this._performPartyWideMail(mail_key, message_queue, no_letter);
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x000CABB4 File Offset: 0x000C8DB4
		protected void _performPartyWideMail(string mail_key, Multiplayer.PartyWideMessageQueue message_queue, bool no_letter)
		{
			if (message_queue != Multiplayer.PartyWideMessageQueue.MailForTomorrow)
			{
				if (message_queue == Multiplayer.PartyWideMessageQueue.SeenMail)
				{
					Game1.addMail(mail_key, no_letter, false);
				}
			}
			else
			{
				Game1.addMailForTomorrow(mail_key, no_letter, false);
			}
			if (no_letter)
			{
				mail_key += "%&NL&%";
			}
			if (message_queue != Multiplayer.PartyWideMessageQueue.MailForTomorrow)
			{
				if (message_queue == Multiplayer.PartyWideMessageQueue.SeenMail)
				{
					mail_key = "%&SM&%" + mail_key;
				}
			}
			else
			{
				mail_key = "%&MFT&%" + mail_key;
			}
			if (Game1.IsMasterGame && !Game1.player.team.broadcastedMail.Contains(mail_key))
			{
				Game1.player.team.broadcastedMail.Add(mail_key);
			}
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x000CAC45 File Offset: 0x000C8E45
		protected void receiveForceKick()
		{
			if (Game1.IsServer)
			{
				return;
			}
			this.Disconnect(Multiplayer.DisconnectType.Kicked);
			this.returnToMainMenu();
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x000CAC5C File Offset: 0x000C8E5C
		protected virtual void receiveGlobalMessage(IncomingMessage msg)
		{
			string translationKey = msg.Reader.ReadString();
			bool flag = msg.Reader.ReadBoolean();
			string locationName = msg.Reader.ReadString();
			if (flag && Game1.hudMessages.Count > 0)
			{
				return;
			}
			if (!string.IsNullOrEmpty(locationName))
			{
				string a = locationName;
				GameLocation currentLocation = Game1.player.currentLocation;
				if (a != ((currentLocation != null) ? currentLocation.NameOrUniqueName : null))
				{
					return;
				}
			}
			int count = msg.Reader.ReadInt32();
			object[] substitutions = new object[count];
			for (int i = 0; i < count; i++)
			{
				substitutions[i] = TokenParser.ParseText(msg.Reader.ReadString(), null, null, null);
			}
			Game1.showGlobalMessage(Game1.content.LoadString(translationKey, substitutions));
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x000CAD0E File Offset: 0x000C8F0E
		protected void receiveStartNewDaySync()
		{
			Game1.newDaySync.flagServerReady();
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x000CAD1A File Offset: 0x000C8F1A
		protected void receiveReadySync(IncomingMessage msg)
		{
			Game1.netReady.ProcessMessage(msg);
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x000CAD27 File Offset: 0x000C8F27
		protected void receiveChestHitSync(IncomingMessage msg)
		{
			Game1.player.team.chestHit.ProcessMessage(msg);
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x000CAD3E File Offset: 0x000C8F3E
		protected void receiveDedicatedServerSync(IncomingMessage msg)
		{
			Game1.dedicatedServer.ProcessMessage(msg);
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x000CAD4C File Offset: 0x000C8F4C
		public virtual void processIncomingMessage(IncomingMessage msg)
		{
			Multiplayer.<>c__DisplayClass157_1 CS$<>8__locals2 = new Multiplayer.<>c__DisplayClass157_1();
			byte messageType = msg.MessageType;
			switch (messageType)
			{
			case 0:
			{
				long f = msg.Reader.ReadInt64();
				NetFarmerRoot farmer = this.farmerRoot(f);
				if (farmer != null)
				{
					this.readObjectDelta<Farmer>(msg.Reader, farmer);
					return;
				}
				break;
			}
			case 1:
			case 5:
			case 9:
			case 11:
			case 16:
				break;
			case 2:
				this.receivePlayerIntroduction(msg.Reader);
				return;
			case 3:
				this.readActiveLocation(msg);
				return;
			case 4:
			{
				CS$<>8__locals2.eventId = msg.Reader.ReadString();
				bool use_local_farmer = msg.Reader.ReadBoolean();
				CS$<>8__locals2.notify_when_done = msg.Reader.ReadBoolean();
				CS$<>8__locals2.tileX = msg.Reader.ReadInt32();
				CS$<>8__locals2.tileY = msg.Reader.ReadInt32();
				CS$<>8__locals2.request = this.readLocationRequest(msg.Reader);
				CS$<>8__locals2.location_for_event_check = Game1.getLocationFromName(CS$<>8__locals2.request.Name);
				GameLocation location_for_event_check = CS$<>8__locals2.location_for_event_check;
				if (((location_for_event_check != null) ? location_for_event_check.findEventById(CS$<>8__locals2.eventId, null) : null) == null)
				{
					Game1.log.Warn("Couldn't find event " + CS$<>8__locals2.eventId + " for broadcast event!");
					return;
				}
				CS$<>8__locals2.farmerActor = (use_local_farmer ? (Game1.player.NetFields.Root as NetRoot<Farmer>).Clone().Value : (msg.SourceFarmer.NetFields.Root as NetRoot<Farmer>).Clone().Value);
				CS$<>8__locals2.oldTile = Game1.player.TilePoint;
				CS$<>8__locals2.oldLocation = Game1.player.currentLocation.NameOrUniqueName;
				CS$<>8__locals2.direction = Game1.player.facingDirection.Value;
				Game1.player.locationBeforeForcedEvent.Value = CS$<>8__locals2.oldLocation;
				CS$<>8__locals2.request.OnWarp += delegate()
				{
					CS$<>8__locals2.farmerActor.currentLocation = Game1.currentLocation;
					CS$<>8__locals2.farmerActor.completelyStopAnimatingOrDoingAction();
					CS$<>8__locals2.farmerActor.UsingTool = false;
					CS$<>8__locals2.farmerActor.Items.Clear();
					CS$<>8__locals2.farmerActor.hidden.Value = false;
					Event evt = Game1.currentLocation.findEventById(CS$<>8__locals2.eventId, CS$<>8__locals2.farmerActor);
					evt.notifyWhenDone = CS$<>8__locals2.notify_when_done;
					evt.notifyLocationName = CS$<>8__locals2.location_for_event_check.NameOrUniqueName;
					evt.notifyLocationIsStructure = ((CS$<>8__locals2.request.IsStructure > false) ? 1 : 0);
					Game1.currentLocation.startEvent(evt);
					CS$<>8__locals2.farmerActor.Position = Game1.player.Position;
					Game1.warpingForForcedRemoteEvent = false;
					string old_location_before_event = Game1.player.locationBeforeForcedEvent.Value;
					Game1.player.locationBeforeForcedEvent.Value = null;
					evt.setExitLocation(CS$<>8__locals2.oldLocation, CS$<>8__locals2.oldTile.X, CS$<>8__locals2.oldTile.Y);
					Game1.player.locationBeforeForcedEvent.Value = old_location_before_event;
					Game1.player.orientationBeforeEvent = CS$<>8__locals2.direction;
				};
				Game1.remoteEventQueue.Add(new Action(CS$<>8__locals2.<processIncomingMessage>g__PerformForcedEvent|2));
				return;
			}
			case 6:
			{
				GameLocation location = this.readLocation(msg.Reader);
				if (location != null)
				{
					this.readObjectDelta<GameLocation>(msg.Reader, location.Root);
					return;
				}
				break;
			}
			case 7:
			{
				GameLocation location = this.readLocation(msg.Reader);
				if (location != null)
				{
					this.readSprites(msg.Reader, location, delegate(TemporaryAnimatedSprite sprite)
					{
						location.temporarySprites.Add(sprite);
					});
					return;
				}
				break;
			}
			case 8:
			{
				NPC character = this.readNPC(msg.Reader);
				GameLocation location = this.readLocation(msg.Reader);
				if (character != null && location != null)
				{
					Game1.warpCharacter(character, location, msg.Reader.ReadVector2());
					return;
				}
				break;
			}
			case 10:
			{
				long recipientId = msg.Reader.ReadInt64();
				LocalizedContentManager.LanguageCode langCode = msg.Reader.ReadEnum<LocalizedContentManager.LanguageCode>();
				string message = msg.Reader.ReadString();
				this.receiveChatMessage(msg.SourceFarmer, recipientId, langCode, message);
				return;
			}
			case 12:
				this.receiveWorldState(msg.Reader);
				return;
			case 13:
				this.receiveTeamDelta(msg.Reader);
				return;
			case 14:
				this.receiveNewDaySync(msg);
				return;
			case 15:
			{
				string messageKey = msg.Reader.ReadString();
				string[] args = new string[(int)msg.Reader.ReadByte()];
				for (int i = 0; i < args.Length; i++)
				{
					args[i] = msg.Reader.ReadString();
				}
				this.receiveChatInfoMessage(msg.SourceFarmer, messageKey, args);
				return;
			}
			case 17:
				this.receiveFarmerGainExperience(msg);
				return;
			case 18:
				this.parseServerToClientsMessage(msg.Reader.ReadString());
				return;
			case 19:
				this.playerDisconnected(msg.SourceFarmer.UniqueMultiplayerID);
				return;
			case 20:
				this.receiveSharedAchievement(msg);
				return;
			case 21:
				this.receiveGlobalMessage(msg);
				return;
			case 22:
				this.receivePartyWideMail(msg);
				return;
			case 23:
				this.receiveForceKick();
				return;
			case 24:
				this.receiveRemoveLocationFromLookup(msg);
				return;
			case 25:
				this.receiveFarmerKilledMonster(msg);
				return;
			case 26:
				this.receiveRequestGrandpaReevaluation(msg);
				return;
			case 27:
				this.receiveNutDig(msg);
				return;
			case 28:
				this.receivePassoutRequest(msg);
				return;
			case 29:
				this.receivePassout(msg);
				return;
			case 30:
				this.receiveStartNewDaySync();
				return;
			case 31:
				this.receiveReadySync(msg);
				return;
			case 32:
				this.receiveChestHitSync(msg);
				return;
			case 33:
				this.receiveDedicatedServerSync(msg);
				return;
			default:
				if (messageType != 127)
				{
					return;
				}
				Game1.log.Warn("Unexpectedly received a compressed multiplayer message that wasn't decompressed by the net client.");
				break;
			}
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x000CB1D7 File Offset: 0x000C93D7
		public virtual void StartLocalMultiplayerServer()
		{
			Game1.server = new GameServer(true);
			Game1.server.startServer();
		}

		// Token: 0x0600114B RID: 4427 RVA: 0x000CB1EE File Offset: 0x000C93EE
		public virtual void StartServer()
		{
			Game1.server = new GameServer(false);
			Game1.server.startServer();
		}

		// Token: 0x0600114C RID: 4428 RVA: 0x000CB208 File Offset: 0x000C9408
		public virtual void Disconnect(Multiplayer.DisconnectType disconnectType)
		{
			if (Game1.server != null)
			{
				Game1.server.stopServer();
				Game1.server = null;
				foreach (long id in Game1.otherFarmers.Keys)
				{
					this.playerDisconnected(id);
				}
			}
			if (Game1.client != null)
			{
				this.sendFarmhand();
				this.UpdateLate(true);
				Game1.client.disconnect(true);
				Game1.client = null;
			}
			Game1.otherFarmers.Clear();
			Multiplayer.LogDisconnect(disconnectType);
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x000CB2A8 File Offset: 0x000C94A8
		protected virtual void updatePendingConnections()
		{
			byte multiplayerMode = Game1.multiplayerMode;
			if (multiplayerMode != 1)
			{
				if (multiplayerMode == 2 && Game1.server == null && Game1.options.enableServer)
				{
					this.StartServer();
					return;
				}
			}
			else if (Game1.client != null && !Game1.client.readyToPlay)
			{
				Game1.client.receiveMessages();
			}
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x000CB2FA File Offset: 0x000C94FA
		public void UpdateLoading()
		{
			this.updatePendingConnections();
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x000CB314 File Offset: 0x000C9514
		public virtual void UpdateEarly()
		{
			this.updatePendingConnections();
			if (Game1.multiplayerMode == 2 && Game1.serverHost == null && Game1.options.enableServer)
			{
				Game1.server.initializeHost();
			}
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
			else if (Game1.client != null)
			{
				Game1.client.receiveMessages();
			}
			this.updateRoots();
			if (Game1.CurrentEvent == null)
			{
				this.removeDisconnectedFarmers();
			}
		}

		// Token: 0x06001150 RID: 4432 RVA: 0x000CB38C File Offset: 0x000C958C
		public virtual void UpdateLate(bool forceSync = false)
		{
			if (Game1.multiplayerMode != 0)
			{
				if (!this.allowSyncDelay() || forceSync || Game1.ticks % this.farmerDeltaBroadcastPeriod == 0)
				{
					this.broadcastFarmerDeltas();
				}
				if (!this.allowSyncDelay() || forceSync || Game1.ticks % this.locationDeltaBroadcastPeriod == 0)
				{
					this.broadcastLocationDeltas();
				}
				if (!this.allowSyncDelay() || forceSync || Game1.ticks % this.worldStateDeltaBroadcastPeriod == 0)
				{
					this.broadcastWorldStateDeltas();
				}
			}
			if (Game1.server != null)
			{
				Game1.server.sendMessages();
			}
			if (Game1.client != null)
			{
				Game1.client.sendMessages();
			}
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x000CB428 File Offset: 0x000C9628
		public virtual void inviteAccepted()
		{
			TitleMenu title = Game1.activeClickableMenu as TitleMenu;
			if (title != null)
			{
				IClickableMenu subMenu = TitleMenu.subMenu;
				if (subMenu == null)
				{
					title.performButtonAction("Invite");
					return;
				}
				if (!(subMenu is FarmhandMenu) && !(subMenu is CoopMenu))
				{
					return;
				}
				TitleMenu.subMenu = new FarmhandMenu();
			}
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x000CB473 File Offset: 0x000C9673
		public virtual Client InitClient(Client client)
		{
			return client;
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x000CB476 File Offset: 0x000C9676
		public virtual Server InitServer(Server server)
		{
			return server;
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x000CB4E4 File Offset: 0x000C96E4
		[CompilerGenerated]
		internal static void <broadcastLocationMessage>g__TellFarmer|78_0(Farmer f, ref Multiplayer.<>c__DisplayClass78_0 A_1)
		{
			if (f != Game1.player)
			{
				Game1.server.sendMessage(f.UniqueMultiplayerID, A_1.message);
			}
		}

		// Token: 0x04000A19 RID: 2585
		public static readonly long AllPlayers = 0L;

		// Token: 0x04000A1A RID: 2586
		public const byte farmerDelta = 0;

		// Token: 0x04000A1B RID: 2587
		public const byte serverIntroduction = 1;

		// Token: 0x04000A1C RID: 2588
		public const byte playerIntroduction = 2;

		// Token: 0x04000A1D RID: 2589
		public const byte locationIntroduction = 3;

		// Token: 0x04000A1E RID: 2590
		public const byte forceEvent = 4;

		// Token: 0x04000A1F RID: 2591
		public const byte warpFarmer = 5;

		// Token: 0x04000A20 RID: 2592
		public const byte locationDelta = 6;

		// Token: 0x04000A21 RID: 2593
		public const byte locationSprites = 7;

		// Token: 0x04000A22 RID: 2594
		public const byte characterWarp = 8;

		// Token: 0x04000A23 RID: 2595
		public const byte availableFarmhands = 9;

		// Token: 0x04000A24 RID: 2596
		public const byte chatMessage = 10;

		// Token: 0x04000A25 RID: 2597
		public const byte connectionMessage = 11;

		// Token: 0x04000A26 RID: 2598
		public const byte worldDelta = 12;

		// Token: 0x04000A27 RID: 2599
		public const byte teamDelta = 13;

		// Token: 0x04000A28 RID: 2600
		public const byte newDaySync = 14;

		// Token: 0x04000A29 RID: 2601
		public const byte chatInfoMessage = 15;

		// Token: 0x04000A2A RID: 2602
		public const byte userNameUpdate = 16;

		// Token: 0x04000A2B RID: 2603
		public const byte farmerGainExperience = 17;

		// Token: 0x04000A2C RID: 2604
		public const byte serverToClientsMessage = 18;

		// Token: 0x04000A2D RID: 2605
		public const byte disconnecting = 19;

		// Token: 0x04000A2E RID: 2606
		public const byte sharedAchievement = 20;

		// Token: 0x04000A2F RID: 2607
		public const byte globalMessage = 21;

		// Token: 0x04000A30 RID: 2608
		public const byte partyWideMail = 22;

		// Token: 0x04000A31 RID: 2609
		public const byte forceKick = 23;

		// Token: 0x04000A32 RID: 2610
		public const byte removeLocationFromLookup = 24;

		// Token: 0x04000A33 RID: 2611
		public const byte farmerKilledMonster = 25;

		// Token: 0x04000A34 RID: 2612
		public const byte requestGrandpaReevaluation = 26;

		// Token: 0x04000A35 RID: 2613
		public const byte digBuriedNut = 27;

		// Token: 0x04000A36 RID: 2614
		public const byte requestPassout = 28;

		// Token: 0x04000A37 RID: 2615
		public const byte passout = 29;

		// Token: 0x04000A38 RID: 2616
		public const byte startNewDaySync = 30;

		// Token: 0x04000A39 RID: 2617
		public const byte readySync = 31;

		// Token: 0x04000A3A RID: 2618
		public const byte chestHitSync = 32;

		// Token: 0x04000A3B RID: 2619
		public const byte dedicatedServerSync = 33;

		// Token: 0x04000A3C RID: 2620
		public const byte compressed = 127;

		// Token: 0x04000A3D RID: 2621
		public const byte WARP_FLAG_STRUCTURE = 1;

		// Token: 0x04000A3E RID: 2622
		public const byte WARP_FLAG_FORCED = 2;

		// Token: 0x04000A3F RID: 2623
		public const byte WARP_FLAG_NEEDS_INFO = 4;

		// Token: 0x04000A40 RID: 2624
		public const byte WARP_FLAG_FACE_UP = 8;

		// Token: 0x04000A41 RID: 2625
		public const byte WARP_FLAG_FACE_RIGHT = 16;

		// Token: 0x04000A42 RID: 2626
		public const byte WARP_FLAG_FACE_DOWN = 32;

		// Token: 0x04000A43 RID: 2627
		public const byte WARP_FLAG_FACE_LEFT = 64;

		// Token: 0x04000A44 RID: 2628
		public const string chat_token_aOrAn = "aOrAn:";

		// Token: 0x04000A45 RID: 2629
		public int defaultInterpolationTicks = 15;

		// Token: 0x04000A46 RID: 2630
		public int farmerDeltaBroadcastPeriod = 3;

		// Token: 0x04000A47 RID: 2631
		public int locationDeltaBroadcastPeriod = 3;

		// Token: 0x04000A48 RID: 2632
		public int worldStateDeltaBroadcastPeriod = 3;

		// Token: 0x04000A49 RID: 2633
		public int playerLimit = 4;

		// Token: 0x04000A4A RID: 2634
		public static string kicked = "KICKED";

		// Token: 0x04000A4B RID: 2635
		internal static string protocolVersionOverride;

		// Token: 0x04000A4C RID: 2636
		public readonly NetLogger logging = new NetLogger();

		// Token: 0x04000A4D RID: 2637
		protected List<long> disconnectingFarmers = new List<long>();

		// Token: 0x04000A4E RID: 2638
		public ulong latestID;

		// Token: 0x04000A4F RID: 2639
		public Dictionary<string, CachedMultiplayerMap> cachedMultiplayerMaps = new Dictionary<string, CachedMultiplayerMap>();

		// Token: 0x04000A50 RID: 2640
		protected HashSet<GameLocation> _updatedRoots = new HashSet<GameLocation>();

		// Token: 0x04000A51 RID: 2641
		public const string MSG_START_FESTIVAL_EVENT = "festivalEvent";

		// Token: 0x04000A52 RID: 2642
		public const string MSG_END_FESTIVAL = "endFest";

		// Token: 0x04000A53 RID: 2643
		public const string MSG_TRAIN_APPROACH = "trainApproach";

		// Token: 0x020004AE RID: 1198
		public enum PartyWideMessageQueue
		{
			// Token: 0x040028F8 RID: 10488
			MailForTomorrow,
			// Token: 0x040028F9 RID: 10489
			SeenMail
		}

		// Token: 0x020004AF RID: 1199
		public enum DisconnectType
		{
			// Token: 0x040028FB RID: 10491
			None,
			// Token: 0x040028FC RID: 10492
			ClosedGame,
			// Token: 0x040028FD RID: 10493
			ExitedToMainMenu,
			// Token: 0x040028FE RID: 10494
			ExitedToMainMenu_FromFarmhandSelect,
			// Token: 0x040028FF RID: 10495
			HostLeft,
			// Token: 0x04002900 RID: 10496
			ServerOfflineMode,
			// Token: 0x04002901 RID: 10497
			ServerFull,
			// Token: 0x04002902 RID: 10498
			Kicked,
			// Token: 0x04002903 RID: 10499
			AcceptedOtherInvite,
			// Token: 0x04002904 RID: 10500
			ClientTimeout,
			// Token: 0x04002905 RID: 10501
			LidgrenTimeout,
			// Token: 0x04002906 RID: 10502
			GalaxyTimeout,
			// Token: 0x04002907 RID: 10503
			Timeout_FarmhandSelection,
			// Token: 0x04002908 RID: 10504
			LidgrenDisconnect_Unknown
		}
	}
}
