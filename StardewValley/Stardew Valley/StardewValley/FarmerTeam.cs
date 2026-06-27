using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.Util;

namespace StardewValley
{
	// Token: 0x020000AE RID: 174
	public class FarmerTeam : INetObject<NetFields>
	{
		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060009F7 RID: 2551 RVA: 0x0006C452 File Offset: 0x0006A652
		public NetFields NetFields { get; } = new NetFields("FarmerTeam");

		// Token: 0x060009F8 RID: 2552 RVA: 0x0006C45C File Offset: 0x0006A65C
		public FarmerTeam()
		{
			this.NetFields.SetOwner(this).AddField(this.money, "money").AddField(this.totalMoneyEarned, "totalMoneyEarned").AddField(this.proposals, "proposals").AddField(this.luauIngredients, "luauIngredients").AddField(this.grangeDisplay, "grangeDisplay").AddField(this.grangeMutex.NetFields, "grangeMutex.NetFields").AddField(this.festivalPropRemovalEvent, "festivalPropRemovalEvent").AddField(this.friendshipData, "friendshipData").AddField(this.demolishLock.NetFields, "demolishLock.NetFields").AddField(this.buildLock.NetFields, "buildLock.NetFields").AddField(this.movieInvitations, "movieInvitations").AddField(this.movieMutex.NetFields, "movieMutex.NetFields").AddField(this.requestMovieEndEvent, "requestMovieEndEvent").AddField(this.endMovieEvent, "endMovieEvent").AddField(this.requestSpouseSleepEvent, "requestSpouseSleepEvent").AddField(this.requestNPCGoHome, "requestNPCGoHome").AddField(this.useSeparateWallets, "useSeparateWallets").AddField(this.individualMoney, "individualMoney").AddField(this.announcedSleepingFarmers.NetFields, "announcedSleepingFarmers.NetFields").AddField(this.sleepAnnounceMode, "sleepAnnounceMode").AddField(this.theaterBuildDate, "theaterBuildDate").AddField(this.buildingConstructedEvent, "buildingConstructedEvent").AddField(this.buildingMovedEvent, "buildingMovedEvent").AddField(this.buildingDemolishedEvent, "buildingDemolishedEvent").AddField(this.queenOfSauceRerunWeek, "queenOfSauceRerunWeek").AddField(this.lastDayQueenOfSauceRerunUpdated, "lastDayQueenOfSauceRerunUpdated").AddField(this.broadcastedMail, "broadcastedMail").AddField(this.constructedBuildings, "constructedBuildings").AddField(this.sharedDailyLuck, "sharedDailyLuck").AddField(this.spawnMonstersAtNight, "spawnMonstersAtNight").AddField(this.useLegacyRandom, "useLegacyRandom").AddField(this.allowChatCheats, "allowChatCheats").AddField(this.hasDedicatedHost, "hasDedicatedHost").AddField(this.junimoKartScores.NetFields, "junimoKartScores.NetFields").AddField(this.cellarAssignments, "cellarAssignments").AddField(this.synchronizedShopStock.NetFields, "synchronizedShopStock.NetFields").AddField(this.junimoKartStatus.NetFields, "junimoKartStatus.NetFields").AddField(this.endOfNightStatus.NetFields, "endOfNightStatus.NetFields").AddField(this.festivalScoreStatus.NetFields, "festivalScoreStatus.NetFields").AddField(this.sleepStatus.NetFields, "sleepStatus.NetFields").AddField(this.farmhandsCanMoveBuildings, "farmhandsCanMoveBuildings").AddField(this.requestPetWarpHomeEvent, "requestPetWarpHomeEvent").AddField(this.ringPhoneEvent, "ringPhoneEvent").AddField(this.specialOrders, "specialOrders").AddField(this.returnedDonations, "returnedDonations").AddField(this.returnedDonationsMutex.NetFields, "returnedDonationsMutex.NetFields").AddField(this.goldenCoconutMutex.NetFields, "goldenCoconutMutex.NetFields").AddField(this.requestNutDrop, "requestNutDrop").AddField(this.requestSetSimpleFlag, "requestSetSimpleFlag").AddField(this.requestSetMail, "requestSetMail").AddField(this.limitedNutDrops, "limitedNutDrops").AddField(this.availableSpecialOrders, "availableSpecialOrders").AddField(this.acceptedSpecialOrderTypes, "acceptedSpecialOrderTypes").AddField(this.ordersBoardMutex.NetFields, "ordersBoardMutex.NetFields").AddField(this.qiChallengeBoardMutex.NetFields, "qiChallengeBoardMutex.NetFields").AddField(this.completedSpecialOrders, "completedSpecialOrders").AddField(this.addCharacterEvent, "addCharacterEvent").AddField(this.requestAddCharacterEvent, "requestAddCharacterEvent").AddField(this.requestLeoMove, "requestLeoMove").AddField(this.collectedNutTracker, "collectedNutTracker").AddField(this.itemsToRemoveOvernight, "itemsToRemoveOvernight").AddField(this.mailToRemoveOvernight, "mailToRemoveOvernight").AddField(this.newLostAndFoundItems, "newLostAndFoundItems").AddField(this.globalInventories, "globalInventories").AddField(this.globalInventoryMutexes, "globalInventoryMutexes").AddField(this.requestHorseWarpEvent, "requestHorseWarpEvent").AddField(this.kickOutOfMinesEvent, "kickOutOfMinesEvent").AddField(this.toggleMineShrineOvernight, "toggleMineShrineOvernight").AddField(this.mineShrineActivated, "mineShrineActivated").AddField(this.toggleSkullShrineOvernight, "toggleSkullShrineOvernight").AddField(this.skullShrineActivated, "skullShrineActivated").AddField(this.specialRulesRemovedToday, "specialRulesRemovedToday").AddField(this.addQiGemsToTeam, "addQiGemsToTeam").AddField(this.farmPerfect, "farmPerfect").AddField(this.calicoEggSkullCavernRating, "calicoEggSkullCavernRating").AddField(this.highestCalicoEggRatingToday, "highestCalicoEggRatingToday").AddField(this.calicoStatueEffects, "calicoStatueEffects");
			this.newLostAndFoundItems.Interpolated(false, false);
			this.junimoKartStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			this.festivalScoreStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			this.endOfNightStatus.displayMode = PlayerStatusList.DisplayMode.Icons;
			this.endOfNightStatus.AddSpriteDefinition("sleep", "LooseSprites\\PlayerStatusList", 0, 0, 16, 16);
			this.endOfNightStatus.AddSpriteDefinition("level", "LooseSprites\\PlayerStatusList", 16, 0, 16, 16);
			this.endOfNightStatus.AddSpriteDefinition("shipment", "LooseSprites\\PlayerStatusList", 32, 0, 16, 16);
			this.endOfNightStatus.AddSpriteDefinition("ready", "LooseSprites\\PlayerStatusList", 48, 0, 16, 16);
			this.endOfNightStatus.iconAnimationFrames = 4;
			this.festivalPropRemovalEvent.onEvent += delegate(Rectangle rect)
			{
				if (Game1.CurrentEvent == null)
				{
					return;
				}
				Game1.CurrentEvent.removeFestivalProps(rect);
			};
			this.toggleSkullShrineOvernight.fieldChangeEvent += delegate(NetBool field, bool oldVal, bool newVal)
			{
				if ((newVal || Game1.player.team.skullShrineActivated.Value) && Game1.currentLocation.NameOrUniqueName == "SkullCave")
				{
					Game1.currentLocation.MakeMapModifications(true);
				}
			};
			this.requestSpouseSleepEvent.onEvent += this.OnRequestSpouseSleepEvent;
			this.requestNPCGoHome.onEvent += this.OnRequestNPCGoHome;
			this.requestPetWarpHomeEvent.onEvent += this.OnRequestPetWarpHomeEvent;
			this.requestMovieEndEvent.onEvent += this.OnRequestMovieEndEvent;
			this.endMovieEvent.onEvent += this.OnEndMovieEvent;
			this.buildingConstructedEvent.AddReaderHandler(new Action<BinaryReader>(this.OnBuildingConstructedEvent));
			this.buildingMovedEvent.AddReaderHandler(new Action<BinaryReader>(this.OnBuildingMovedEvent));
			this.buildingDemolishedEvent.AddReaderHandler(new Action<BinaryReader>(this.OnBuildingDemolishedEvent));
			this.ringPhoneEvent.onEvent += this.OnRingPhoneEvent;
			this.requestNutDrop.onEvent += this.OnRequestNutDrop;
			this.requestSetSimpleFlag.onEvent += new AbstractNetEvent1<SetSimpleFlagRequest>.Event(this.OnRequestPlayerAction);
			this.requestSetMail.onEvent += new AbstractNetEvent1<SetMailRequest>.Event(this.OnRequestPlayerAction);
			this.requestAddCharacterEvent.onEvent += this.OnRequestAddCharacterEvent;
			this.addCharacterEvent.onEvent += this.OnAddCharacterEvent;
			this.requestLeoMove.onEvent += this.OnRequestLeoMoveEvent;
			this.requestHorseWarpEvent.onEvent += this.OnRequestHorseWarp;
			this.calicoEggSkullCavernRating.fieldChangeEvent += this.OnCalicoEggRatingChanged;
			this.calicoStatueEffects.OnValueAdded += delegate(int key, int _)
			{
				this.OnCalicoStatueEffectAdded(key);
			};
			this.calicoStatueEffects.OnValueTargetUpdated += delegate(int key, int oldValue, int newValue)
			{
				this.OnCalicoStatueEffectAdded(key);
			};
			this.kickOutOfMinesEvent.onEvent += this.OnKickOutOfMinesEvent;
			this.addQiGemsToTeam.onEvent += this._AddQiGemsToTeam;
			this.constructedBuildings.OnValueAdded += delegate(string buildingType)
			{
				if (Game1.hasStartedDay)
				{
					Game1.player.NotifyQuests((Quest quest) => quest.OnBuildingExists(buildingType, false), false);
				}
			};
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0006D05C File Offset: 0x0006B25C
		public void AddCalicoStatueEffect(int effectId)
		{
			if (!this.calicoStatueEffects.TryAdd(effectId, 1))
			{
				NetIntDictionary<int, NetInt> netIntDictionary = this.calicoStatueEffects;
				netIntDictionary[effectId]++;
			}
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x0006D094 File Offset: 0x0006B294
		private void OnCalicoStatueEffectAdded(int key)
		{
			switch (key)
			{
			case 10:
				if (Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 121)
				{
					DesertFestival.addCalicoStatueSpeedBuff();
				}
				break;
			case 11:
				Game1.player.health = Game1.player.maxHealth;
				Game1.player.stamina = (float)Game1.player.maxStamina.Value;
				break;
			case 12:
				if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 50, 0, false), false))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 50, 0, false), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation, -1, false);
				}
				break;
			case 15:
				if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 25, 0, false), false))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 25, 0, false), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation, -1, false);
				}
				break;
			case 16:
				if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 10, 0, false), false))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 10, 0, false), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation, -1, false);
				}
				break;
			case 17:
				if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 100, 0, false), false))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 100, 0, false), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation, -1, false);
				}
				break;
			}
			if (Game1.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 121)
			{
				string description = Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Description_" + key.ToString());
				Point newVector = Game1.mine.calicoStatueSpot.Value;
				foreach (Vector2 v in Utility.getAdjacentTileLocations(Vector2.Zero))
				{
					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(null, Rectangle.Empty, new Vector2((float)(newVector.X * 64 + 32) - (float)SpriteText.getWidthOfString(description, 999999) / 2f, (float)((newVector.Y - 3) * 64)) + v * 4f, false, 0f, Color.Black)
					{
						text = description,
						extraInfoForEndBehavior = -777,
						layerDepth = 0.99f,
						motion = new Vector2(0f, -1f),
						yStopCoordinate = (newVector.Y - 4) * 64,
						animationLength = 1,
						delayBeforeAnimationStart = 500,
						totalNumberOfLoops = 10,
						interval = 300f,
						drawAboveAlwaysFront = true
					});
				}
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(null, Rectangle.Empty, new Vector2((float)(newVector.X * 64 + 32) - (float)SpriteText.getWidthOfString(description, 999999) / 2f, (float)((newVector.Y - 3) * 64)), false, 0f, Color.White)
				{
					text = description,
					extraInfoForEndBehavior = -777,
					layerDepth = 1f,
					motion = new Vector2(0f, -1f),
					yStopCoordinate = (newVector.Y - 4) * 64,
					animationLength = 1,
					delayBeforeAnimationStart = 500,
					totalNumberOfLoops = 10,
					interval = 300f,
					drawAboveAlwaysFront = true
				});
			}
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x0006D48C File Offset: 0x0006B68C
		private void OnCalicoEggRatingChanged(NetInt field, int oldValue, int newValue)
		{
			if (newValue > oldValue && Game1.currentLocation is MineShaft)
			{
				if (Game1.mine != null)
				{
					Game1.mine.calicoEggIconTimerShake = 1500f;
				}
				DelayedAction.playSoundAfterDelay("yoba", 800, null, null, -1, false);
			}
			if (Game1.IsMasterGame && Game1.hasStartedDay && newValue > Game1.player.team.highestCalicoEggRatingToday.Value)
			{
				Game1.player.team.highestCalicoEggRatingToday.Value = newValue;
			}
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x0006D514 File Offset: 0x0006B714
		protected virtual void _AddQiGemsToTeam(int amount)
		{
			Game1.player.QiGems += amount;
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x0006D528 File Offset: 0x0006B728
		public virtual void OnKickOutOfMinesEvent(int mineshaftType)
		{
			MineShaft mineshaft = Game1.currentLocation as MineShaft;
			if (mineshaft == null || !((mineshaftType == 120) ? (mineshaft.mineLevel <= mineshaftType) : (mineshaft.getMineArea(-1) == mineshaftType)))
			{
				return;
			}
			if (mineshaftType == 121)
			{
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.warpFarmer(Game1.getLocationRequest("SkullCave", false), 3, 4, 2);
				return;
			}
			if (mineshaftType == 77377)
			{
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.warpFarmer(Game1.getLocationRequest("Mine", false), 67, 10, 2);
				return;
			}
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.warpFarmer(Game1.getLocationRequest("Mine", false), 18, 4, 2);
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x0006D5D0 File Offset: 0x0006B7D0
		public virtual void OnRequestHorseWarp(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmer = Game1.GetPlayer(uid, false);
			if (farmer == null)
			{
				return;
			}
			Horse horse = null;
			Utility.ForEachBuilding<Stable>(delegate(Stable stable)
			{
				Horse curHorse = stable.getStableHorse();
				if (curHorse != null && curHorse.getOwner() == farmer)
				{
					horse = curHorse;
					return false;
				}
				return true;
			}, true);
			if (horse != null && Utility.GetHorseWarpRestrictionsForFarmer(farmer) == Utility.HorseWarpRestrictions.None)
			{
				horse.mutex.RequestLock(delegate
				{
					horse.mutex.ReleaseLock();
					GameLocation location = horse.currentLocation;
					Vector2 tile_location = horse.Tile;
					for (int i = 0; i < 8; i++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(10, new Vector2(tile_location.X + Utility.RandomFloat(-1f, 1f, null), tile_location.Y + Utility.RandomFloat(-1f, 0f, null)) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
							{
								layerDepth = 1f,
								motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f, null), Utility.RandomFloat(-0.5f, 0.5f, null))
							}
						});
					}
					location.playSound("wand", new Vector2?(horse.Tile), null, SoundContext.Default);
					location = farmer.currentLocation;
					tile_location = farmer.Tile;
					location.playSound("wand", new Vector2?(tile_location), null, SoundContext.Default);
					for (int j = 0; j < 8; j++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(10, new Vector2(tile_location.X + Utility.RandomFloat(-1f, 1f, null), tile_location.Y + Utility.RandomFloat(-1f, 0f, null)) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
							{
								layerDepth = 1f,
								motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f, null), Utility.RandomFloat(-0.5f, 0.5f, null))
							}
						});
					}
					Game1.warpCharacter(horse, farmer.currentLocation, tile_location);
					int k = 0;
					for (int x = (int)tile_location.X + 3; x >= (int)tile_location.X - 3; x--)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(6, new Vector2((float)x, tile_location.Y) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
							{
								layerDepth = 1f,
								delayBeforeAnimationStart = k * 25,
								motion = new Vector2(-0.25f, 0f)
							}
						});
						k++;
					}
				}, null);
			}
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x0006D64C File Offset: 0x0006B84C
		public virtual void OnRequestLeoMoveEvent()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			NPC leo = Game1.getCharacterFromName("Leo", true, false);
			if (leo == null)
			{
				return;
			}
			leo.reloadDefaultLocation();
			leo.faceDirection(2);
			leo.InvalidateMasterSchedule();
			leo.ClearSchedule();
			leo.controller = null;
			leo.temporaryController = null;
			Game1.warpCharacter(leo, Game1.RequireLocation("Mountain", false), new Vector2(16f, 8f));
			leo.Halt();
			leo.ignoreScheduleToday = false;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x0006D6DF File Offset: 0x0006B8DF
		public virtual void MarkCollectedNut(string key)
		{
			this.collectedNutTracker.Add(key);
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x0006D6EE File Offset: 0x0006B8EE
		public int GetIndividualMoney(Farmer who)
		{
			return this.GetMoney(who).Value;
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x0006D6FC File Offset: 0x0006B8FC
		public void AddIndividualMoney(Farmer who, int value)
		{
			this.GetMoney(who).Value += value;
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x0006D712 File Offset: 0x0006B912
		public void SetIndividualMoney(Farmer who, int value)
		{
			this.GetMoney(who).Value = value;
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x0006D724 File Offset: 0x0006B924
		public NetIntDelta GetMoney(Farmer who)
		{
			if (this.useSeparateWallets.Value)
			{
				NetIntDelta value;
				if (!this.individualMoney.TryGetValue(who.UniqueMultiplayerID, out value))
				{
					NetDictionary<long, NetIntDelta, NetRef<NetIntDelta>, SerializableDictionary<long, NetIntDelta>, NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>>> netDictionary = this.individualMoney;
					long uniqueMultiplayerID = who.UniqueMultiplayerID;
					NetIntDelta netIntDelta = new NetIntDelta(500);
					netIntDelta.Minimum = new int?(0);
					value = netIntDelta;
					netDictionary[uniqueMultiplayerID] = netIntDelta;
				}
				return value;
			}
			return this.money;
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x0006D784 File Offset: 0x0006B984
		public bool SpecialOrderActive(string special_order_key)
		{
			foreach (SpecialOrder order in this.specialOrders)
			{
				if (order.questKey.Value == special_order_key && order.questState.Value == SpecialOrderStatus.InProgress)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x0006D7F8 File Offset: 0x0006B9F8
		public bool SpecialOrderRuleActive(string special_rule, SpecialOrder order_to_ignore = null)
		{
			foreach (SpecialOrder order in this.specialOrders)
			{
				if (order != order_to_ignore && order.questState.Value == SpecialOrderStatus.InProgress && order.specialRule.Value != null)
				{
					string[] array = order.specialRule.Value.Split(',', StringSplitOptions.None);
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].Trim() == special_rule)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x0006D8A0 File Offset: 0x0006BAA0
		public void AddSpecialOrder(string id, int? generationSeed = null, bool forceRepeatable = false)
		{
			if (this.specialOrders.Any((SpecialOrder p) => p.questKey.Value == id))
			{
				return;
			}
			SpecialOrder order = SpecialOrder.GetSpecialOrder(id, generationSeed);
			if (order == null)
			{
				Game1.log.Warn("Can't add special order with ID '" + id + "' because no such ID was found.");
				return;
			}
			if (this.completedSpecialOrders.Contains(order.questKey.Value) && !forceRepeatable)
			{
				SpecialOrderData data = order.GetData();
				if (data == null || !data.Repeatable)
				{
					return;
				}
			}
			this.specialOrders.Add(order);
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x0006D944 File Offset: 0x0006BB44
		public SpecialOrder GetAvailableSpecialOrder(int index = 0, string type = "")
		{
			foreach (SpecialOrder order in this.availableSpecialOrders)
			{
				if (order.orderType.Value == type)
				{
					if (index <= 0)
					{
						return order;
					}
					index--;
				}
			}
			return null;
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x0006D9B4 File Offset: 0x0006BBB4
		public void CheckReturnedDonations()
		{
			this.returnedDonationsMutex.RequestLock(delegate
			{
				this.returnedDonations.RemoveWhere((Item item) => item == null);
				Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
				foreach (Item item2 in this.returnedDonations)
				{
					contents[item2] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None, null, null, null, null);
				}
				Game1.activeClickableMenu = new ShopMenu("ReturnedDonations", contents, 0, null, new ShopMenu.OnPurchaseDelegate(this.OnDonatedItemWithdrawn), new Func<ISalable, bool>(this.OnReturnedDonationDeposited), true)
				{
					source = this,
					behaviorBeforeCleanup = delegate(IClickableMenu menu)
					{
						this.returnedDonationsMutex.ReleaseLock();
					}
				};
			}, null);
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x0006D9D0 File Offset: 0x0006BBD0
		public bool OnDonatedItemWithdrawn(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
		{
			Item item = salable as Item;
			if (item != null && stock.Stock < 1)
			{
				this.returnedDonations.Remove(item);
			}
			return false;
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x0006D9FF File Offset: 0x0006BBFF
		public bool OnReturnedDonationDeposited(ISalable deposited_salable)
		{
			return false;
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x0006DA02 File Offset: 0x0006BC02
		public void OnRequestMovieEndEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Game1.RequireLocation<MovieTheater>("MovieTheater", false).RequestEndMovie(uid);
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x0006DA20 File Offset: 0x0006BC20
		public void OnRequestPetWarpHomeEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmer = Game1.GetPlayer(uid, false);
			if (farmer == null)
			{
				farmer = Game1.MasterPlayer;
			}
			Pet pet = Game1.getCharacterFromName<Pet>(farmer.getPetName(), false, false);
			if (((pet != null) ? pet.currentLocation : null) is FarmHouse)
			{
				return;
			}
			if (pet != null)
			{
				pet.warpToFarmHouse(farmer);
			}
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x0006DA74 File Offset: 0x0006BC74
		public void OnRequestNPCGoHome(string npc_name)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			NPC npc = Game1.getCharacterFromName(npc_name, true, false);
			if (string.IsNullOrEmpty(npc.defaultMap.Value))
			{
				npc.doingEndOfRouteAnimation.Value = false;
				npc.nextEndOfRouteMessage = null;
				npc.endOfRouteMessage.Value = null;
				npc.controller = null;
				npc.temporaryController = null;
				npc.Halt();
				Game1.warpCharacter(npc, npc.defaultMap.Value, npc.DefaultPosition / 64f);
				npc.ignoreScheduleToday = true;
			}
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x0006DB00 File Offset: 0x0006BD00
		public void OnRequestSpouseSleepEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmer = Game1.GetPlayer(uid, false);
			if (farmer != null)
			{
				NPC spouse = Game1.getCharacterFromName(farmer.spouse, true, false);
				if (spouse != null && !spouse.isSleeping.Value)
				{
					FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
					Game1.warpCharacter(spouse, farm_house, new Vector2((float)farm_house.getSpouseBedSpot(farmer.spouse).X, (float)farm_house.getSpouseBedSpot(farmer.spouse).Y));
					spouse.NetFields.CancelInterpolation();
					spouse.Halt();
					spouse.faceDirection(0);
					spouse.controller = null;
					spouse.temporaryController = null;
					spouse.ignoreScheduleToday = true;
					if (farm_house.GetSpouseBed() != null)
					{
						FarmHouse.spouseSleepEndFunction(spouse, farm_house);
					}
				}
			}
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x0006DBB7 File Offset: 0x0006BDB7
		public virtual void OnRequestAddCharacterEvent(string character_name)
		{
			if (Game1.IsMasterGame && Game1.AddCharacterIfNecessary(character_name, false))
			{
				this.addCharacterEvent.Fire(character_name);
			}
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x0006DBD5 File Offset: 0x0006BDD5
		public virtual void OnAddCharacterEvent(string character_name)
		{
			if (!Game1.IsMasterGame)
			{
				Game1.AddCharacterIfNecessary(character_name, true);
			}
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x0006DBE8 File Offset: 0x0006BDE8
		public void RequestLimitedNutDrops(string key, GameLocation location, int x, int y, int limit, int rewardAmount = 1)
		{
			int count;
			if (!this.limitedNutDrops.TryGetValue(key, out count) || count < limit)
			{
				this.requestNutDrop.Fire(new NutDropRequest(key, (location != null) ? location.NameOrUniqueName : null, new Point(x, y), limit, rewardAmount));
			}
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x0006DC34 File Offset: 0x0006BE34
		public int GetDroppedLimitedNutCount(string key)
		{
			int count;
			if (!this.limitedNutDrops.TryGetValue(key, out count))
			{
				return 0;
			}
			return count;
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x0006DC54 File Offset: 0x0006BE54
		protected void OnRequestNutDrop(NutDropRequest request)
		{
			if (Game1.IsMasterGame)
			{
				int count = this.GetDroppedLimitedNutCount(request.Key);
				if (count < request.Limit)
				{
					int award_amount = request.RewardAmount;
					award_amount = Math.Min(request.Limit - count, award_amount);
					this.limitedNutDrops[request.Key] = count + award_amount;
					GameLocation location = null;
					if (request.LocationName != "null")
					{
						location = Game1.getLocationFromName(request.LocationName);
					}
					if (location != null)
					{
						for (int i = 0; i < award_amount; i++)
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2((float)request.Tile.X, (float)request.Tile.Y), -1, location, -1, false);
						}
						return;
					}
					Game1.netWorldState.Value.GoldenWalnutsFound += award_amount;
					Game1.netWorldState.Value.GoldenWalnuts += award_amount;
				}
			}
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x0006DD40 File Offset: 0x0006BF40
		public void RequestSetSimpleFlag(SimpleFlagType flag, PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId = null)
		{
			this.RequestPlayerAction<SetSimpleFlagRequest>(new SetSimpleFlagRequest(flag, target, flagId, flagState, onlyPlayerId), this.requestSetSimpleFlag);
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x0006DD5A File Offset: 0x0006BF5A
		public void RequestSetMail(PlayerActionTarget playerTarget, string mailId, MailType mailType, bool add, long? onlyPlayerId = null)
		{
			this.RequestPlayerAction<SetMailRequest>(new SetMailRequest(playerTarget, mailId, mailType, add, onlyPlayerId), this.requestSetMail);
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x0006DD74 File Offset: 0x0006BF74
		public void OnRingPhoneEvent(string callId)
		{
			Phone.Ring(callId);
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x0006DD7C File Offset: 0x0006BF7C
		public void OnEndMovieEvent(long uid)
		{
			if (Game1.player.UniqueMultiplayerID != uid)
			{
				return;
			}
			Game1.player.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
			if (Game1.CurrentEvent != null)
			{
				Event currentEvent = Game1.CurrentEvent;
				currentEvent.onEventFinished = (Action)Delegate.Combine(currentEvent.onEventFinished, new Action(delegate()
				{
					Game1.warpFarmer(Game1.getLocationRequest("MovieTheater", false), 13, 4, 2);
					Game1.fadeToBlackAlpha = 1f;
				}));
				Game1.CurrentEvent.endBehaviors(null);
			}
		}

		// Token: 0x06000A19 RID: 2585 RVA: 0x0006DDFC File Offset: 0x0006BFFC
		public void SendBuildingConstructedEvent(GameLocation location, Building building, Farmer who)
		{
			this.buildingConstructedEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(location.NameOrUniqueName);
				writer.WriteGuid(building.id.Value);
				writer.Write(who.UniqueMultiplayerID);
			});
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x0006DE3C File Offset: 0x0006C03C
		public void OnBuildingConstructedEvent(BinaryReader reader)
		{
			string name = reader.ReadString();
			Guid buildingId = reader.ReadGuid();
			long farmerId = reader.ReadInt64();
			GameLocation location = Game1.getLocationFromName(name);
			Building building = (location != null) ? location.getBuildingById(buildingId) : null;
			Farmer who = Game1.GetPlayer(farmerId, false);
			if (building != null)
			{
				location.OnBuildingConstructed(building, who);
			}
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x0006DE88 File Offset: 0x0006C088
		public void SendBuildingMovedEvent(GameLocation location, Building building)
		{
			this.buildingMovedEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(location.NameOrUniqueName);
				writer.WriteGuid(building.id.Value);
			});
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x0006DEC0 File Offset: 0x0006C0C0
		public void OnBuildingMovedEvent(BinaryReader reader)
		{
			string name = reader.ReadString();
			Guid buildingId = reader.ReadGuid();
			GameLocation location = Game1.getLocationFromName(name);
			Building building = (location != null) ? location.getBuildingById(buildingId) : null;
			if (building != null)
			{
				location.OnBuildingMoved(building);
			}
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x0006DEF8 File Offset: 0x0006C0F8
		public void SendBuildingDemolishedEvent(GameLocation location, Building building)
		{
			this.buildingDemolishedEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(location.NameOrUniqueName);
				writer.Write(building.buildingType.Value);
				writer.WriteGuid(building.id.Value);
			});
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x0006DF30 File Offset: 0x0006C130
		public void OnBuildingDemolishedEvent(BinaryReader reader)
		{
			string name = reader.ReadString();
			string buildingType = reader.ReadString();
			Guid buildingId = reader.ReadGuid();
			Game1.getLocationFromName(name).OnBuildingDemolished(buildingType, buildingId);
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x0006DF60 File Offset: 0x0006C160
		public void DeleteFarmhand(Farmer farmhand)
		{
			this.friendshipData.RemoveWhere((KeyValuePair<FarmerPair, Friendship> pair) => pair.Key.Contains(farmhand.UniqueMultiplayerID));
			Game1.netWorldState.Value.farmhandData.Remove(farmhand.UniqueMultiplayerID);
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x0006DFB4 File Offset: 0x0006C1B4
		public Friendship GetFriendship(long farmer1, long farmer2)
		{
			FarmerPair pair = FarmerPair.MakePair(farmer1, farmer2);
			if (!this.friendshipData.ContainsKey(pair))
			{
				this.friendshipData.Add(pair, new Friendship());
			}
			return this.friendshipData[pair];
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x0006DFF4 File Offset: 0x0006C1F4
		public void AddAnyBroadcastedMail()
		{
			foreach (string text in this.broadcastedMail)
			{
				Multiplayer.PartyWideMessageQueue mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
				string mail_key = text;
				if (mail_key.StartsWith("%&SM&%"))
				{
					mail_key = mail_key.Substring("%&SM&%".Length);
					mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
				}
				else if (mail_key.StartsWith("%&MFT&%"))
				{
					mail_key = mail_key.Substring("%&MFT&%".Length);
					mail_queue = Multiplayer.PartyWideMessageQueue.MailForTomorrow;
				}
				if (mail_queue == Multiplayer.PartyWideMessageQueue.SeenMail)
				{
					if (mail_key.Contains("%&NL&%") || mail_key.StartsWith("NightMarketYear"))
					{
						mail_key = mail_key.Replace("%&NL&%", "");
						Game1.player.mailReceived.Add(mail_key);
					}
					else if (!Game1.player.hasOrWillReceiveMail(mail_key))
					{
						Game1.player.mailbox.Add(mail_key);
					}
				}
				else if (!Game1.MasterPlayer.mailForTomorrow.Contains(mail_key))
				{
					if (!Game1.player.hasOrWillReceiveMail(mail_key))
					{
						if (mail_key.Contains("%&NL&%"))
						{
							string stripped = mail_key.Replace("%&NL&%", "");
							Game1.player.mailReceived.Add(stripped);
						}
						else if (!Game1.player.mailbox.Contains(mail_key))
						{
							Game1.player.mailbox.Add(mail_key);
						}
					}
				}
				else if (!Game1.player.hasOrWillReceiveMail(mail_key))
				{
					Game1.player.mailForTomorrow.Add(mail_key);
				}
			}
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x0006E18C File Offset: 0x0006C38C
		public bool IsMarried(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in this.friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && kvpair.Value.IsMarried())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x0006E20C File Offset: 0x0006C40C
		public bool IsEngaged(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in this.friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && kvpair.Value.IsEngaged())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x0006E28C File Offset: 0x0006C48C
		public long? GetSpouse(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in this.friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && (kvpair.Value.IsEngaged() || kvpair.Value.IsMarried()))
				{
					return new long?(kvpair.Key.GetOther(farmer));
				}
			}
			return null;
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x0006E334 File Offset: 0x0006C534
		public void FestivalPropsRemoved(Rectangle rect)
		{
			this.festivalPropRemovalEvent.Fire(rect);
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x0006E344 File Offset: 0x0006C544
		public void SendProposal(Farmer receiver, ProposalType proposalType, Item gift = null)
		{
			Proposal proposal = new Proposal();
			proposal.sender.Value = Game1.player;
			proposal.receiver.Value = receiver;
			proposal.proposalType.Value = proposalType;
			proposal.gift.Value = gift;
			this.proposals[Game1.player.UniqueMultiplayerID] = proposal;
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x0006E3A4 File Offset: 0x0006C5A4
		public Proposal GetOutgoingProposal()
		{
			Proposal proposal;
			if (this.proposals.TryGetValue(Game1.player.UniqueMultiplayerID, out proposal))
			{
				return proposal;
			}
			return null;
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x0006E3CD File Offset: 0x0006C5CD
		public void RemoveOutgoingProposal()
		{
			this.proposals.Remove(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x0006E3E8 File Offset: 0x0006C5E8
		public Proposal GetIncomingProposal()
		{
			foreach (Proposal proposal in this.proposals.Values)
			{
				if (proposal.receiver.Value == Game1.player && proposal.response.Value == ProposalResponse.None)
				{
					return proposal;
				}
			}
			return null;
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x0006E464 File Offset: 0x0006C664
		private bool locationsMatch(GameLocation location1, GameLocation location2)
		{
			int mineLevel;
			return location1 != null && location2 != null && (location1.Name == location2.Name || ((location1 is Mine || (MineShaft.IsGeneratedLevel(location1, out mineLevel) && mineLevel < 121)) && (location2 is Mine || (MineShaft.IsGeneratedLevel(location2, out mineLevel) && mineLevel < 121))) || ((location1.Name.Equals("SkullCave") || (MineShaft.IsGeneratedLevel(location1, out mineLevel) && mineLevel >= 121)) && (location2.Name.Equals("SkullCave") || (MineShaft.IsGeneratedLevel(location2, out mineLevel) && mineLevel >= 121))));
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x0006E504 File Offset: 0x0006C704
		public double AverageDailyLuck(GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || this.locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += farmer.DailyLuck;
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x0006E584 File Offset: 0x0006C784
		public double AverageLuckLevel(GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || this.locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += (double)farmer.LuckLevel;
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x0006E608 File Offset: 0x0006C808
		public double AverageSkillLevel(int skillIndex, GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || this.locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += (double)farmer.GetSkillLevel(skillIndex);
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x0006E68C File Offset: 0x0006C88C
		public void Update()
		{
			this.requestLeoMove.Poll();
			this.requestMovieEndEvent.Poll();
			this.endMovieEvent.Poll();
			this.ringPhoneEvent.Poll();
			this.festivalPropRemovalEvent.Poll();
			this.buildingConstructedEvent.Poll();
			this.buildingMovedEvent.Poll();
			this.buildingDemolishedEvent.Poll();
			this.requestSpouseSleepEvent.Poll();
			this.requestNPCGoHome.Poll();
			this.requestHorseWarpEvent.Poll();
			this.kickOutOfMinesEvent.Poll();
			this.requestPetWarpHomeEvent.Poll();
			this.requestNutDrop.Poll();
			this.requestSetSimpleFlag.Poll();
			this.requestSetMail.Poll();
			this.requestAddCharacterEvent.Poll();
			this.addCharacterEvent.Poll();
			this.addQiGemsToTeam.Poll();
			this.grangeMutex.Update(Game1.getOnlineFarmers());
			this.returnedDonationsMutex.Update(Game1.getOnlineFarmers());
			this.ordersBoardMutex.Update(Game1.getOnlineFarmers());
			this.qiChallengeBoardMutex.Update(Game1.getOnlineFarmers());
			this.chestHit.Update();
			foreach (NetMutex netMutex in this.globalInventoryMutexes.Values)
			{
				netMutex.Update(Game1.getOnlineFarmers());
			}
			this.demolishLock.Update();
			this.buildLock.Update(Game1.getOnlineFarmers());
			this.movieMutex.Update(Game1.getOnlineFarmers());
			this.goldenCoconutMutex.Update(Game1.getOnlineFarmers());
			if (this.grangeMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				this.grangeMutex.ReleaseLock();
			}
			foreach (SpecialOrder specialOrder in this.specialOrders)
			{
				specialOrder.Update();
			}
			Game1.netReady.Update();
			if (Game1.IsMasterGame && this.proposals.Length > 0)
			{
				this.proposals.RemoveWhere((KeyValuePair<long, Proposal> pair) => !this.playerIsOnline(pair.Key) || !this.playerIsOnline(pair.Value.receiver.UID));
			}
			Proposal proposal = this.GetIncomingProposal();
			if (proposal != null && proposal.canceled.Value)
			{
				proposal.cancelConfirmed.Value = true;
			}
			if (!Game1.dialogueUp)
			{
				if (proposal != null)
				{
					if (!this.handleIncomingProposal(proposal))
					{
						proposal.responseMessageKey.Value = this.genderedKey("Strings\\UI:Proposal_PlayerBusy", Game1.player);
						proposal.response.Value = ProposalResponse.Rejected;
						return;
					}
				}
				else if (Game1.activeClickableMenu == null && this.GetOutgoingProposal() != null)
				{
					Game1.activeClickableMenu = new PendingProposalDialog();
				}
			}
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x0006E94C File Offset: 0x0006CB4C
		private string genderedKey(string baseKey, Farmer farmer)
		{
			return baseKey + (farmer.IsMale ? "_Male" : "_Female");
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0006E968 File Offset: 0x0006CB68
		private bool handleIncomingProposal(Proposal proposal)
		{
			if (Game1.gameMode != 3 || Game1.activeClickableMenu != null || Game1.currentMinigame != null)
			{
				return proposal.proposalType.Value == ProposalType.Baby;
			}
			if (Game1.currentLocation == null)
			{
				return false;
			}
			if (proposal.proposalType.Value != ProposalType.Dance && Game1.CurrentEvent != null)
			{
				return false;
			}
			string additionalVar = "";
			string responseYes = null;
			string responseNo = null;
			string questionKey;
			switch (proposal.proposalType.Value)
			{
			case ProposalType.Gift:
				if (proposal.gift.Value == null)
				{
					return false;
				}
				if (!Game1.player.couldInventoryAcceptThisItem(proposal.gift.Value))
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = this.genderedKey("Strings\\UI:GiftPlayerItem_NoInventorySpace", Game1.player);
					return true;
				}
				questionKey = "Strings\\UI:GivenGift";
				additionalVar = proposal.gift.Value.DisplayName;
				break;
			case ProposalType.Marriage:
				if (Game1.player.isMarriedOrRoommates() || Game1.player.isEngaged())
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = this.genderedKey("Strings\\UI:AskedToMarry_NotSingle", Game1.player);
					return true;
				}
				questionKey = "Strings\\UI:AskedToMarry";
				responseYes = "Strings\\UI:AskedToMarry_Accepted";
				responseNo = "Strings\\UI:AskedToMarry_Rejected";
				break;
			case ProposalType.Dance:
				if (Game1.CurrentEvent == null || !Game1.CurrentEvent.isSpecificFestival("spring24"))
				{
					return false;
				}
				questionKey = "Strings\\UI:AskedToDance";
				responseYes = "Strings\\UI:AskedToDance_Accepted";
				responseNo = "Strings\\UI:AskedToDance_Rejected";
				if (Game1.player.dancePartner.Value != null)
				{
					return false;
				}
				break;
			case ProposalType.Baby:
				if (proposal.sender.Value.IsMale != Game1.player.IsMale)
				{
					questionKey = "Strings\\UI:AskedToHaveBaby";
					responseYes = "Strings\\UI:AskedToHaveBaby_Accepted";
					responseNo = "Strings\\UI:AskedToHaveBaby_Rejected";
				}
				else
				{
					questionKey = "Strings\\UI:AskedToAdoptBaby";
					responseYes = "Strings\\UI:AskedToAdoptBaby_Accepted";
					responseNo = "Strings\\UI:AskedToAdoptBaby_Rejected";
				}
				break;
			default:
				return false;
			}
			questionKey = this.genderedKey(questionKey, proposal.sender.Value);
			if (responseYes != null)
			{
				responseYes = this.genderedKey(responseYes, Game1.player);
			}
			if (responseNo != null)
			{
				responseNo = this.genderedKey(responseNo, Game1.player);
			}
			string question = Game1.content.LoadString(questionKey, proposal.sender.Value.Name, additionalVar);
			Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), delegate(Farmer _, string answer)
			{
				if (proposal.canceled.Value)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:ProposalWithdrawn", proposal.sender.Value.Name));
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = responseNo;
					return;
				}
				if (answer == "Yes")
				{
					proposal.response.Value = ProposalResponse.Accepted;
					proposal.responseMessageKey.Value = responseYes;
					if (proposal.proposalType.Value == ProposalType.Gift || proposal.proposalType.Value == ProposalType.Marriage)
					{
						Item item = proposal.gift.Value;
						proposal.gift.Value = null;
						item = Game1.player.addItemToInventory(item);
						if (item != null)
						{
							Game1.currentLocation.debris.Add(new Debris(item, Game1.player.Position));
						}
					}
					switch (proposal.proposalType.Value)
					{
					case ProposalType.Marriage:
					{
						Friendship friendship = this.GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						friendship.Status = FriendshipStatus.Engaged;
						friendship.Proposer = proposal.sender.Value.UniqueMultiplayerID;
						WorldDate weddingDate = new WorldDate(Game1.Date);
						weddingDate.TotalDays += 3;
						while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
						{
							weddingDate.TotalDays++;
						}
						friendship.WeddingDate = weddingDate;
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:PlayerWeddingArranged"));
						Game1.multiplayer.globalChatInfoMessage("Engaged", new string[]
						{
							Game1.player.Name,
							proposal.sender.Value.Name
						});
						break;
					}
					case ProposalType.Dance:
						Game1.player.dancePartner.Value = proposal.sender.Value;
						break;
					case ProposalType.Baby:
					{
						Friendship friendship2 = this.GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						WorldDate birthingDate = new WorldDate(Game1.Date);
						birthingDate.TotalDays += 14;
						friendship2.NextBirthingDate = birthingDate;
						break;
					}
					}
					Game1.player.doEmote(20);
					return;
				}
				proposal.response.Value = ProposalResponse.Rejected;
				proposal.responseMessageKey.Value = responseNo;
			}, null);
			return true;
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x0006EC50 File Offset: 0x0006CE50
		public bool playerIsOnline(long uid)
		{
			return Game1.MasterPlayer.UniqueMultiplayerID == uid || (Game1.serverHost != null && Game1.serverHost.Value.UniqueMultiplayerID == uid) || (Game1.otherFarmers.ContainsKey(uid) && !Game1.multiplayer.isDisconnecting(uid));
		}

		// Token: 0x06000A32 RID: 2610 RVA: 0x0006ECAC File Offset: 0x0006CEAC
		public Inventory GetOrCreateGlobalInventory(string id)
		{
			Inventory inventory;
			if (!this.globalInventories.TryGetValue(id, out inventory))
			{
				inventory = (this.globalInventories[id] = new Inventory());
			}
			return inventory;
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x0006ECE0 File Offset: 0x0006CEE0
		public NetMutex GetOrCreateGlobalInventoryMutex(string id)
		{
			NetMutex mutex;
			if (!this.globalInventoryMutexes.TryGetValue(id, out mutex))
			{
				mutex = (this.globalInventoryMutexes[id] = new NetMutex());
			}
			return mutex;
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x0006ED14 File Offset: 0x0006CF14
		public void NewDay()
		{
			Game1.dedicatedServer.ResetForNewDay();
			Game1.netReady.Reset();
			this.chestHit.Reset();
			if (Game1.IsClient)
			{
				return;
			}
			this.luauIngredients.Clear();
			if (this.grangeDisplay.Count > 0)
			{
				for (int i = 0; i < this.grangeDisplay.Count; i++)
				{
					Item item = this.grangeDisplay[i];
					this.grangeDisplay[i] = null;
					if (item != null)
					{
						this.returnedDonations.Add(item);
						this.newLostAndFoundItems.Value = true;
					}
				}
			}
			this.grangeDisplay.Clear();
			this.movieInvitations.Clear();
			this.synchronizedShopStock.Clear();
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x0006EDCD File Offset: 0x0006CFCD
		private void RequestPlayerAction<T>(T request, NetEvent1<T> @event) where T : BasePlayerActionRequest, new()
		{
			if (request.OnlyForLocalPlayer())
			{
				request.PerformAction(Game1.player);
				return;
			}
			@event.Fire(request);
		}

		// Token: 0x06000A36 RID: 2614 RVA: 0x0006EDF4 File Offset: 0x0006CFF4
		private void OnRequestPlayerAction(BasePlayerActionRequest request)
		{
			if (request.MatchesPlayer(Game1.player))
			{
				request.PerformAction(Game1.player);
			}
			if (request.Target == PlayerActionTarget.All && Game1.IsMasterGame)
			{
				foreach (Farmer farmhand in Game1.getOfflineFarmhands())
				{
					if (request.MatchesPlayer(farmhand))
					{
						request.PerformAction(farmhand);
					}
				}
			}
		}

		// Token: 0x04000638 RID: 1592
		public const string GlobalInventoryId_LostItemsShop = "LostItemsShop";

		// Token: 0x04000639 RID: 1593
		public const string GlobalInventoryId_JunimoChest = "JunimoChests";

		// Token: 0x0400063A RID: 1594
		public readonly NetIntDelta money = new NetIntDelta(500)
		{
			Minimum = new int?(0)
		};

		// Token: 0x0400063B RID: 1595
		public readonly NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>> individualMoney = new NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>>();

		// Token: 0x0400063C RID: 1596
		public readonly NetIntDelta totalMoneyEarned = new NetIntDelta(0);

		// Token: 0x0400063D RID: 1597
		public readonly NetBool useSeparateWallets = new NetBool();

		// Token: 0x0400063E RID: 1598
		public readonly NetBool newLostAndFoundItems = new NetBool();

		// Token: 0x0400063F RID: 1599
		public readonly NetBool toggleMineShrineOvernight = new NetBool();

		// Token: 0x04000640 RID: 1600
		public readonly NetBool mineShrineActivated = new NetBool();

		// Token: 0x04000641 RID: 1601
		public readonly NetBool toggleSkullShrineOvernight = new NetBool();

		// Token: 0x04000642 RID: 1602
		public readonly NetBool skullShrineActivated = new NetBool();

		// Token: 0x04000643 RID: 1603
		public readonly NetBool farmPerfect = new NetBool();

		// Token: 0x04000644 RID: 1604
		public readonly NetList<string, NetString> specialRulesRemovedToday = new NetList<string, NetString>();

		// Token: 0x04000645 RID: 1605
		public readonly NetList<string, NetString> itemsToRemoveOvernight = new NetList<string, NetString>();

		// Token: 0x04000646 RID: 1606
		public readonly NetList<string, NetString> mailToRemoveOvernight = new NetList<string, NetString>();

		// Token: 0x04000647 RID: 1607
		public NetIntDictionary<long, NetLong> cellarAssignments = new NetIntDictionary<long, NetLong>();

		// Token: 0x04000648 RID: 1608
		public NetStringHashSet broadcastedMail = new NetStringHashSet();

		// Token: 0x04000649 RID: 1609
		public readonly NetStringHashSet constructedBuildings = new NetStringHashSet();

		// Token: 0x0400064A RID: 1610
		public NetStringHashSet collectedNutTracker = new NetStringHashSet();

		// Token: 0x0400064B RID: 1611
		public NetStringHashSet completedSpecialOrders = new NetStringHashSet();

		// Token: 0x0400064C RID: 1612
		public NetList<SpecialOrder, NetRef<SpecialOrder>> specialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

		// Token: 0x0400064D RID: 1613
		public NetList<SpecialOrder, NetRef<SpecialOrder>> availableSpecialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

		// Token: 0x0400064E RID: 1614
		public NetStringHashSet acceptedSpecialOrderTypes = new NetStringHashSet();

		// Token: 0x0400064F RID: 1615
		public readonly NetCollection<Item> returnedDonations = new NetCollection<Item>();

		// Token: 0x04000650 RID: 1616
		internal readonly ChestHitSynchronizer chestHit = new ChestHitSynchronizer();

		// Token: 0x04000651 RID: 1617
		public readonly NetStringDictionary<Inventory, NetRef<Inventory>> globalInventories = new NetStringDictionary<Inventory, NetRef<Inventory>>();

		// Token: 0x04000652 RID: 1618
		public readonly NetStringDictionary<NetMutex, NetRef<NetMutex>> globalInventoryMutexes = new NetStringDictionary<NetMutex, NetRef<NetMutex>>();

		// Token: 0x04000653 RID: 1619
		public readonly NetFarmerCollection announcedSleepingFarmers = new NetFarmerCollection();

		// Token: 0x04000654 RID: 1620
		public readonly NetEnum<FarmerTeam.SleepAnnounceModes> sleepAnnounceMode = new NetEnum<FarmerTeam.SleepAnnounceModes>(FarmerTeam.SleepAnnounceModes.All);

		// Token: 0x04000655 RID: 1621
		public readonly NetEnum<FarmerTeam.RemoteBuildingPermissions> farmhandsCanMoveBuildings = new NetEnum<FarmerTeam.RemoteBuildingPermissions>(FarmerTeam.RemoteBuildingPermissions.Off);

		// Token: 0x04000656 RID: 1622
		private readonly NetLongDictionary<Proposal, NetRef<Proposal>> proposals = new NetLongDictionary<Proposal, NetRef<Proposal>>();

		// Token: 0x04000657 RID: 1623
		public readonly NetList<MovieInvitation, NetRef<MovieInvitation>> movieInvitations = new NetList<MovieInvitation, NetRef<MovieInvitation>>();

		// Token: 0x04000658 RID: 1624
		public readonly NetCollection<Item> luauIngredients = new NetCollection<Item>();

		// Token: 0x04000659 RID: 1625
		public readonly NetCollection<Item> grangeDisplay = new NetCollection<Item>();

		// Token: 0x0400065A RID: 1626
		public readonly NetMutex grangeMutex = new NetMutex();

		// Token: 0x0400065B RID: 1627
		public readonly NetMutex returnedDonationsMutex = new NetMutex();

		// Token: 0x0400065C RID: 1628
		public readonly NetMutex ordersBoardMutex = new NetMutex();

		// Token: 0x0400065D RID: 1629
		public readonly NetMutex qiChallengeBoardMutex = new NetMutex();

		// Token: 0x0400065E RID: 1630
		private readonly NetEvent1Field<Rectangle, NetRectangle> festivalPropRemovalEvent = new NetEvent1Field<Rectangle, NetRectangle>();

		// Token: 0x0400065F RID: 1631
		public readonly NetEvent1Field<int, NetInt> addQiGemsToTeam = new NetEvent1Field<int, NetInt>();

		// Token: 0x04000660 RID: 1632
		public readonly NetEvent1Field<string, NetString> addCharacterEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x04000661 RID: 1633
		public readonly NetEvent1Field<string, NetString> requestAddCharacterEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x04000662 RID: 1634
		public readonly NetEvent0 requestLeoMove = new NetEvent0(false);

		// Token: 0x04000663 RID: 1635
		public readonly NetEvent1Field<int, NetInt> kickOutOfMinesEvent = new NetEvent1Field<int, NetInt>();

		// Token: 0x04000664 RID: 1636
		public readonly NetEvent1Field<string, NetString> requestNPCGoHome = new NetEvent1Field<string, NetString>
		{
			InterpolationWait = false
		};

		// Token: 0x04000665 RID: 1637
		public readonly NetEvent1Field<long, NetLong> requestSpouseSleepEvent = new NetEvent1Field<long, NetLong>
		{
			InterpolationWait = false
		};

		// Token: 0x04000666 RID: 1638
		public readonly NetEvent1Field<string, NetString> ringPhoneEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x04000667 RID: 1639
		public readonly NetEvent1Field<long, NetLong> requestHorseWarpEvent = new NetEvent1Field<long, NetLong>
		{
			InterpolationWait = false
		};

		// Token: 0x04000668 RID: 1640
		public readonly NetEvent1Field<long, NetLong> requestPetWarpHomeEvent = new NetEvent1Field<long, NetLong>
		{
			InterpolationWait = false
		};

		// Token: 0x04000669 RID: 1641
		public readonly NetEvent1Field<long, NetLong> requestMovieEndEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x0400066A RID: 1642
		public readonly NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x0400066B RID: 1643
		public readonly NetEventBinary buildingConstructedEvent = new NetEventBinary();

		// Token: 0x0400066C RID: 1644
		public readonly NetEventBinary buildingMovedEvent = new NetEventBinary();

		// Token: 0x0400066D RID: 1645
		public readonly NetEventBinary buildingDemolishedEvent = new NetEventBinary();

		// Token: 0x0400066E RID: 1646
		public readonly NetStringDictionary<int, NetInt> limitedNutDrops = new NetStringDictionary<int, NetInt>();

		// Token: 0x0400066F RID: 1647
		private readonly NetEvent1<NutDropRequest> requestNutDrop = new NetEvent1<NutDropRequest>();

		// Token: 0x04000670 RID: 1648
		private readonly NetEvent1<SetSimpleFlagRequest> requestSetSimpleFlag = new NetEvent1<SetSimpleFlagRequest>();

		// Token: 0x04000671 RID: 1649
		private readonly NetEvent1<SetMailRequest> requestSetMail = new NetEvent1<SetMailRequest>();

		// Token: 0x04000672 RID: 1650
		public readonly NetFarmerPairDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetFarmerPairDictionary<Friendship, NetRef<Friendship>>();

		// Token: 0x04000673 RID: 1651
		public readonly NetWitnessedLock demolishLock = new NetWitnessedLock();

		// Token: 0x04000674 RID: 1652
		public readonly NetMutex buildLock = new NetMutex();

		// Token: 0x04000675 RID: 1653
		public readonly NetMutex movieMutex = new NetMutex();

		// Token: 0x04000676 RID: 1654
		public readonly NetMutex goldenCoconutMutex = new NetMutex();

		// Token: 0x04000677 RID: 1655
		public readonly SynchronizedShopStock synchronizedShopStock = new SynchronizedShopStock();

		// Token: 0x04000678 RID: 1656
		public readonly NetLong theaterBuildDate = new NetLong(-1L);

		// Token: 0x04000679 RID: 1657
		public readonly NetInt lastDayQueenOfSauceRerunUpdated = new NetInt(0);

		// Token: 0x0400067A RID: 1658
		public readonly NetInt queenOfSauceRerunWeek = new NetInt(1);

		// Token: 0x0400067B RID: 1659
		public readonly NetDouble sharedDailyLuck = new NetDouble(0.0010000000474974513);

		// Token: 0x0400067C RID: 1660
		public readonly NetBool spawnMonstersAtNight = new NetBool(false);

		// Token: 0x0400067D RID: 1661
		public readonly NetBool useLegacyRandom = new NetBool(false);

		// Token: 0x0400067E RID: 1662
		internal readonly NetBool allowChatCheats = new NetBool(false);

		// Token: 0x0400067F RID: 1663
		internal readonly NetBool hasDedicatedHost = new NetBool(false);

		// Token: 0x04000680 RID: 1664
		public readonly NetInt calicoEggSkullCavernRating = new NetInt(0);

		// Token: 0x04000681 RID: 1665
		public readonly NetInt highestCalicoEggRatingToday = new NetInt(0);

		// Token: 0x04000682 RID: 1666
		public readonly NetIntDictionary<int, NetInt> calicoStatueEffects = new NetIntDictionary<int, NetInt>();

		// Token: 0x04000683 RID: 1667
		public readonly NetLeaderboards junimoKartScores = new NetLeaderboards();

		// Token: 0x04000685 RID: 1669
		public PlayerStatusList junimoKartStatus = new PlayerStatusList();

		// Token: 0x04000686 RID: 1670
		public PlayerStatusList endOfNightStatus = new PlayerStatusList();

		// Token: 0x04000687 RID: 1671
		public PlayerStatusList festivalScoreStatus = new PlayerStatusList();

		// Token: 0x04000688 RID: 1672
		public PlayerStatusList sleepStatus = new PlayerStatusList();

		// Token: 0x0200042D RID: 1069
		public enum RemoteBuildingPermissions
		{
			// Token: 0x0400277C RID: 10108
			Off,
			// Token: 0x0400277D RID: 10109
			OwnedBuildings,
			// Token: 0x0400277E RID: 10110
			On
		}

		// Token: 0x0200042E RID: 1070
		public enum SleepAnnounceModes
		{
			// Token: 0x04002780 RID: 10112
			All,
			// Token: 0x04002781 RID: 10113
			First,
			// Token: 0x04002782 RID: 10114
			Off
		}
	}
}
