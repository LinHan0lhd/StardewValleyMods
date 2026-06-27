using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.LocationContexts;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.Util;

namespace StardewValley.Network
{
	// Token: 0x020001EE RID: 494
	public class NetWorldState : INetObject<NetFields>
	{
		// Token: 0x1700038A RID: 906
		// (get) Token: 0x060021BD RID: 8637 RVA: 0x00174378 File Offset: 0x00172578
		public NetFields NetFields { get; } = new NetFields("NetWorldState");

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x060021BE RID: 8638 RVA: 0x00174380 File Offset: 0x00172580
		// (set) Token: 0x060021BF RID: 8639 RVA: 0x0017438D File Offset: 0x0017258D
		public ServerPrivacy ServerPrivacy
		{
			get
			{
				return this.serverPrivacy.Value;
			}
			set
			{
				this.serverPrivacy.Value = value;
			}
		}

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x060021C0 RID: 8640 RVA: 0x0017439B File Offset: 0x0017259B
		// (set) Token: 0x060021C1 RID: 8641 RVA: 0x001743A8 File Offset: 0x001725A8
		public Game1.MineChestType ShuffleMineChests
		{
			get
			{
				return this.shuffleMineChests.Value;
			}
			set
			{
				this.shuffleMineChests.Value = value;
			}
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x060021C2 RID: 8642 RVA: 0x001743B6 File Offset: 0x001725B6
		// (set) Token: 0x060021C3 RID: 8643 RVA: 0x001743C3 File Offset: 0x001725C3
		public int MinesDifficulty
		{
			get
			{
				return this.minesDifficulty.Value;
			}
			set
			{
				this.minesDifficulty.Value = value;
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x060021C4 RID: 8644 RVA: 0x001743D1 File Offset: 0x001725D1
		// (set) Token: 0x060021C5 RID: 8645 RVA: 0x001743DE File Offset: 0x001725DE
		public int SkullCavesDifficulty
		{
			get
			{
				return this.skullCavesDifficulty.Value;
			}
			set
			{
				this.skullCavesDifficulty.Value = value;
			}
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x060021C6 RID: 8646 RVA: 0x001743EC File Offset: 0x001725EC
		// (set) Token: 0x060021C7 RID: 8647 RVA: 0x001743F9 File Offset: 0x001725F9
		public int HighestPlayerLimit
		{
			get
			{
				return this.highestPlayerLimit.Value;
			}
			set
			{
				this.highestPlayerLimit.Value = value;
			}
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x060021C8 RID: 8648 RVA: 0x00174407 File Offset: 0x00172607
		// (set) Token: 0x060021C9 RID: 8649 RVA: 0x00174414 File Offset: 0x00172614
		public int CurrentPlayerLimit
		{
			get
			{
				return this.currentPlayerLimit.Value;
			}
			set
			{
				this.currentPlayerLimit.Value = value;
			}
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x060021CA RID: 8650 RVA: 0x00174422 File Offset: 0x00172622
		public WorldDate Date
		{
			get
			{
				return WorldDate.Now();
			}
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x060021CB RID: 8651 RVA: 0x00174429 File Offset: 0x00172629
		// (set) Token: 0x060021CC RID: 8652 RVA: 0x00174436 File Offset: 0x00172636
		public int VisitsUntilY1Guarantee
		{
			get
			{
				return this.visitsUntilY1Guarantee.Value;
			}
			set
			{
				this.visitsUntilY1Guarantee.Value = value;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x060021CD RID: 8653 RVA: 0x00174444 File Offset: 0x00172644
		// (set) Token: 0x060021CE RID: 8654 RVA: 0x00174451 File Offset: 0x00172651
		public bool IsPaused
		{
			get
			{
				return this.isPaused.Value;
			}
			set
			{
				this.isPaused.Value = value;
			}
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x060021CF RID: 8655 RVA: 0x0017445F File Offset: 0x0017265F
		// (set) Token: 0x060021D0 RID: 8656 RVA: 0x0017446C File Offset: 0x0017266C
		public bool IsTimePaused
		{
			get
			{
				return this.isTimePaused.Value;
			}
			set
			{
				this.isTimePaused.Value = value;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x060021D1 RID: 8657 RVA: 0x0017447A File Offset: 0x0017267A
		public NetStringDictionary<LocationWeather, NetRef<LocationWeather>> LocationWeather
		{
			get
			{
				return this.locationWeather;
			}
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x060021D2 RID: 8658 RVA: 0x00174482 File Offset: 0x00172682
		// (set) Token: 0x060021D3 RID: 8659 RVA: 0x0017448F File Offset: 0x0017268F
		public string WeatherForTomorrow
		{
			get
			{
				return this.weatherForTomorrow.Value;
			}
			set
			{
				this.weatherForTomorrow.Value = value;
			}
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x060021D4 RID: 8660 RVA: 0x0017449D File Offset: 0x0017269D
		public NetBundles Bundles
		{
			get
			{
				return this.bundles;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x060021D5 RID: 8661 RVA: 0x001744A5 File Offset: 0x001726A5
		public NetIntDictionary<bool, NetBool> BundleRewards
		{
			get
			{
				return this.bundleRewards;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x060021D6 RID: 8662 RVA: 0x001744B0 File Offset: 0x001726B0
		public Dictionary<string, string> BundleData
		{
			get
			{
				if (this.netBundleData.Length == 0)
				{
					this.SetBundleData(DataLoader.Bundles(Game1.content));
				}
				if (this._bundleDataDirty)
				{
					this._bundleDataDirty = false;
					this._bundleData = new Dictionary<string, string>();
					foreach (string key in this.netBundleData.Keys)
					{
						this._bundleData[key] = this.netBundleData[key];
					}
					this.UpdateBundleDisplayNames();
				}
				return this._bundleData;
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x060021D7 RID: 8663 RVA: 0x00174560 File Offset: 0x00172760
		// (set) Token: 0x060021D8 RID: 8664 RVA: 0x0017456D File Offset: 0x0017276D
		public bool ParrotPlatformsUnlocked
		{
			get
			{
				return this.parrotPlatformsUnlocked.Value;
			}
			set
			{
				this.parrotPlatformsUnlocked.Value = value;
			}
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x060021D9 RID: 8665 RVA: 0x0017457B File Offset: 0x0017277B
		// (set) Token: 0x060021DA RID: 8666 RVA: 0x00174588 File Offset: 0x00172788
		public bool IsGoblinRemoved
		{
			get
			{
				return this.goblinRemoved.Value;
			}
			set
			{
				this.goblinRemoved.Value = value;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x060021DB RID: 8667 RVA: 0x00174596 File Offset: 0x00172796
		// (set) Token: 0x060021DC RID: 8668 RVA: 0x001745A3 File Offset: 0x001727A3
		public bool IsSubmarineLocked
		{
			get
			{
				return this.submarineLocked.Value;
			}
			set
			{
				this.submarineLocked.Value = value;
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x060021DD RID: 8669 RVA: 0x001745B1 File Offset: 0x001727B1
		// (set) Token: 0x060021DE RID: 8670 RVA: 0x001745BE File Offset: 0x001727BE
		public int LowestMineLevel
		{
			get
			{
				return this.lowestMineLevel.Value;
			}
			set
			{
				this.lowestMineLevel.Value = value;
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x060021DF RID: 8671 RVA: 0x001745CC File Offset: 0x001727CC
		// (set) Token: 0x060021E0 RID: 8672 RVA: 0x001745D9 File Offset: 0x001727D9
		public int LowestMineLevelForOrder
		{
			get
			{
				return this.lowestMineLevelForOrder.Value;
			}
			set
			{
				this.lowestMineLevelForOrder.Value = value;
			}
		}

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x060021E1 RID: 8673 RVA: 0x001745E7 File Offset: 0x001727E7
		public NetVector2Dictionary<string, NetString> MuseumPieces
		{
			get
			{
				return this.museumPieces;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x060021E2 RID: 8674 RVA: 0x001745EF File Offset: 0x001727EF
		// (set) Token: 0x060021E3 RID: 8675 RVA: 0x001745FC File Offset: 0x001727FC
		public int LostBooksFound
		{
			get
			{
				return this.lostBooksFound.Value;
			}
			set
			{
				this.lostBooksFound.Value = value;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x060021E4 RID: 8676 RVA: 0x0017460A File Offset: 0x0017280A
		// (set) Token: 0x060021E5 RID: 8677 RVA: 0x00174617 File Offset: 0x00172817
		public int GoldenWalnuts
		{
			get
			{
				return this.goldenWalnuts.Value;
			}
			set
			{
				this.goldenWalnuts.Value = value;
			}
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x060021E6 RID: 8678 RVA: 0x00174625 File Offset: 0x00172825
		// (set) Token: 0x060021E7 RID: 8679 RVA: 0x00174632 File Offset: 0x00172832
		public int GoldenWalnutsFound
		{
			get
			{
				return this.goldenWalnutsFound.Value;
			}
			set
			{
				this.goldenWalnutsFound.Value = value;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x060021E8 RID: 8680 RVA: 0x00174640 File Offset: 0x00172840
		// (set) Token: 0x060021E9 RID: 8681 RVA: 0x0017464D File Offset: 0x0017284D
		public bool GoldenCoconutCracked
		{
			get
			{
				return this.goldenCoconutCracked.Value;
			}
			set
			{
				this.goldenCoconutCracked.Value = value;
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x060021EA RID: 8682 RVA: 0x0017465B File Offset: 0x0017285B
		// (set) Token: 0x060021EB RID: 8683 RVA: 0x00174668 File Offset: 0x00172868
		public bool ActivatedGoldenParrot
		{
			get
			{
				return this.activatedGoldenParrot.Value;
			}
			set
			{
				this.activatedGoldenParrot.Value = value;
			}
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x060021EC RID: 8684 RVA: 0x00174676 File Offset: 0x00172876
		public ISet<string> FoundBuriedNuts
		{
			get
			{
				return this.foundBuriedNuts;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x060021ED RID: 8685 RVA: 0x0017467E File Offset: 0x0017287E
		// (set) Token: 0x060021EE RID: 8686 RVA: 0x0017468B File Offset: 0x0017288B
		public int MiniShippingBinsObtained
		{
			get
			{
				return this.miniShippingBinsObtained.Value;
			}
			set
			{
				this.miniShippingBinsObtained.Value = value;
			}
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x060021EF RID: 8687 RVA: 0x00174699 File Offset: 0x00172899
		// (set) Token: 0x060021F0 RID: 8688 RVA: 0x001746A6 File Offset: 0x001728A6
		public int PerfectionWaivers
		{
			get
			{
				return this.perfectionWaivers.Value;
			}
			set
			{
				this.perfectionWaivers.Value = value;
			}
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x060021F1 RID: 8689 RVA: 0x001746B4 File Offset: 0x001728B4
		// (set) Token: 0x060021F2 RID: 8690 RVA: 0x001746C1 File Offset: 0x001728C1
		public int TimesFedRaccoons
		{
			get
			{
				return this.timesFedRaccoons.Value;
			}
			set
			{
				this.timesFedRaccoons.Value = value;
			}
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x060021F3 RID: 8691 RVA: 0x001746CF File Offset: 0x001728CF
		// (set) Token: 0x060021F4 RID: 8692 RVA: 0x001746DC File Offset: 0x001728DC
		public int TreasureTotemsUsed
		{
			get
			{
				return this.treasureTotemsUsed.Value;
			}
			set
			{
				this.treasureTotemsUsed.Value = value;
			}
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x060021F5 RID: 8693 RVA: 0x001746EA File Offset: 0x001728EA
		// (set) Token: 0x060021F6 RID: 8694 RVA: 0x001746F7 File Offset: 0x001728F7
		public int SeasonOfCurrentRacconBundle
		{
			get
			{
				return this.seasonOfCurrentRacconBundle.Value;
			}
			set
			{
				this.seasonOfCurrentRacconBundle.Value = value;
			}
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x060021F7 RID: 8695 RVA: 0x00174705 File Offset: 0x00172905
		// (set) Token: 0x060021F8 RID: 8696 RVA: 0x00174712 File Offset: 0x00172912
		public int DaysPlayedWhenLastRaccoonBundleWasFinished
		{
			get
			{
				return this.daysPlayedWhenLastRaccoonBundleWasFinished.Value;
			}
			set
			{
				this.daysPlayedWhenLastRaccoonBundleWasFinished.Value = value;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x060021F9 RID: 8697 RVA: 0x00174720 File Offset: 0x00172920
		public ISet<string> LocationsWithBuildings
		{
			get
			{
				return this.locationsWithBuildings;
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x060021FA RID: 8698 RVA: 0x00174728 File Offset: 0x00172928
		public NetStringDictionary<BuilderData, NetRef<BuilderData>> Builders
		{
			get
			{
				return this.builders;
			}
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x060021FB RID: 8699 RVA: 0x00174730 File Offset: 0x00172930
		public ISet<string> ActivePassiveFestivals
		{
			get
			{
				return this.activePassiveFestivals;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x060021FC RID: 8700 RVA: 0x00174738 File Offset: 0x00172938
		public ISet<string> IslandVisitors
		{
			get
			{
				return this.islandVisitors;
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x060021FD RID: 8701 RVA: 0x00174740 File Offset: 0x00172940
		public ISet<string> CheckedGarbage
		{
			get
			{
				return this.checkedGarbage;
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x060021FE RID: 8702 RVA: 0x00174748 File Offset: 0x00172948
		// (set) Token: 0x060021FF RID: 8703 RVA: 0x00174755 File Offset: 0x00172955
		public Object DishOfTheDay
		{
			get
			{
				return this.dishOfTheDay.Value;
			}
			set
			{
				this.dishOfTheDay.Value = value;
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06002200 RID: 8704 RVA: 0x00174763 File Offset: 0x00172963
		// (set) Token: 0x06002201 RID: 8705 RVA: 0x0017476B File Offset: 0x0017296B
		public Quest QuestOfTheDay { get; private set; }

		// Token: 0x06002202 RID: 8706 RVA: 0x00174774 File Offset: 0x00172974
		public NetWorldState()
		{
			this.RegisterSpecialCurrencies();
			this.NetFields.SetOwner(this).AddField(this.uniqueIDForThisGame, "uniqueIDForThisGame").AddField(this.serverPrivacy, "serverPrivacy").AddField(this.whichFarm, "whichFarm").AddField(this.whichModFarm, "whichModFarm").AddField(this.shuffleMineChests, "shuffleMineChests").AddField(this.minesDifficulty, "minesDifficulty").AddField(this.skullCavesDifficulty, "skullCavesDifficulty").AddField(this.highestPlayerLimit, "highestPlayerLimit").AddField(this.currentPlayerLimit, "currentPlayerLimit").AddField(this.year, "year").AddField(this.season, "season").AddField(this.dayOfMonth, "dayOfMonth").AddField(this.timeOfDay, "timeOfDay").AddField(this.daysPlayed, "daysPlayed").AddField(this.visitsUntilY1Guarantee, "visitsUntilY1Guarantee").AddField(this.isPaused, "isPaused").AddField(this.isTimePaused, "isTimePaused").AddField(this.locationWeather, "locationWeather").AddField(this.isRaining, "isRaining").AddField(this.isSnowing, "isSnowing").AddField(this.isLightning, "isLightning").AddField(this.isDebrisWeather, "isDebrisWeather").AddField(this.weatherForTomorrow, "weatherForTomorrow").AddField(this.bundles, "bundles").AddField(this.bundleRewards, "bundleRewards").AddField(this.netBundleData, "netBundleData").AddField(this.raccoonBundles, "raccoonBundles").AddField(this.seasonOfCurrentRacconBundle, "seasonOfCurrentRacconBundle").AddField(this.parrotPlatformsUnlocked, "parrotPlatformsUnlocked").AddField(this.goblinRemoved, "goblinRemoved").AddField(this.submarineLocked, "submarineLocked").AddField(this.lowestMineLevel, "lowestMineLevel").AddField(this.lowestMineLevelForOrder, "lowestMineLevelForOrder").AddField(this.museumPieces, "museumPieces").AddField(this.lostBooksFound, "lostBooksFound").AddField(this.goldenWalnuts, "goldenWalnuts").AddField(this.goldenWalnutsFound, "goldenWalnutsFound").AddField(this.goldenCoconutCracked, "goldenCoconutCracked").AddField(this.foundBuriedNuts, "foundBuriedNuts").AddField(this.miniShippingBinsObtained, "miniShippingBinsObtained").AddField(this.perfectionWaivers, "perfectionWaivers").AddField(this.timesFedRaccoons, "timesFedRaccoons").AddField(this.treasureTotemsUsed, "treasureTotemsUsed").AddField(this.farmhandData, "farmhandData").AddField(this.locationsWithBuildings, "locationsWithBuildings").AddField(this.builders, "builders").AddField(this.activePassiveFestivals, "activePassiveFestivals").AddField(this.worldStateIDs, "worldStateIDs").AddField(this.islandVisitors, "islandVisitors").AddField(this.checkedGarbage, "checkedGarbage").AddField(this.dishOfTheDay, "dishOfTheDay").AddField(this.netQuestOfTheDay, "netQuestOfTheDay").AddField(this.activatedGoldenParrot, "activatedGoldenParrot").AddField(this.daysPlayedWhenLastRaccoonBundleWasFinished, "daysPlayedWhenLastRaccoonBundleWasFinished").AddField(this.canDriveYourselfToday, "canDriveYourselfToday").AddField(this.goldenClocksTurnedOff, "goldenClocksTurnedOff");
			this.netBundleData.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
			{
				this._bundleDataDirty = true;
			};
			this.netBundleData.OnValueAdded += delegate(string key, string value)
			{
				this._bundleDataDirty = true;
			};
			this.netBundleData.OnValueRemoved += delegate(string key, string value)
			{
				this._bundleDataDirty = true;
			};
			this.netQuestOfTheDay.fieldChangeVisibleEvent += delegate(NetRef<Quest> field, Quest oldQuest, Quest newQuest)
			{
				if (newQuest == null)
				{
					this.QuestOfTheDay = null;
					return;
				}
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						new NetRef<Quest>
						{
							Value = newQuest
						}.WriteFull(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						using (BinaryReader reader = new BinaryReader(stream))
						{
							NetRef<Quest> destQuest = new NetRef<Quest>();
							destQuest.ReadFull(reader, default(NetVersion));
							this.QuestOfTheDay = destQuest.Value;
						}
					}
				}
			};
		}

		// Token: 0x06002203 RID: 8707 RVA: 0x00174E67 File Offset: 0x00173067
		public virtual void RegisterSpecialCurrencies()
		{
			if (Game1.specialCurrencyDisplay != null)
			{
				Game1.specialCurrencyDisplay.Register("walnuts", this.goldenWalnuts, null, null);
				Game1.specialCurrencyDisplay.Register("qiGems", Game1.player.netQiGems, null, null);
			}
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x00174EA2 File Offset: 0x001730A2
		public void SetQuestOfTheDay(Quest quest)
		{
			if (!Game1.IsMasterGame)
			{
				Game1.log.Warn("Can't set the daily quest from a farmhand instance.");
				Game1.log.Verbose(new StackTraceHelper().ToString());
				return;
			}
			this.netQuestOfTheDay.Value = quest;
		}

		// Token: 0x06002205 RID: 8709 RVA: 0x00174EDC File Offset: 0x001730DC
		public void SetBundleData(Dictionary<string, string> data)
		{
			this._bundleDataDirty = true;
			this.netBundleData.CopyFrom(data);
			foreach (KeyValuePair<string, string> pair in this.netBundleData.Pairs)
			{
				string key = pair.Key;
				string value = pair.Value;
				int index = Convert.ToInt32(key.Split('/', StringSplitOptions.None)[1]);
				int count = ArgUtility.SplitBySpace(value.Split('/', StringSplitOptions.None)[2]).Length;
				if (!this.bundles.ContainsKey(index))
				{
					this.bundles.Add(index, new NetArray<bool, NetBool>(count));
				}
				else if (this.bundles[index].Length < count)
				{
					NetArray<bool, NetBool> new_array = new NetArray<bool, NetBool>(count);
					for (int i = 0; i < Math.Min(this.bundles[index].Length, count); i++)
					{
						new_array[i] = this.bundles[index][i];
					}
					this.bundles.Remove(index);
					this.bundles.Add(index, new_array);
				}
				if (!this.bundleRewards.ContainsKey(index))
				{
					this.bundleRewards.Add(index, new NetBool(false));
				}
			}
		}

		// Token: 0x06002206 RID: 8710 RVA: 0x00175048 File Offset: 0x00173248
		public static bool checkAnywhereForWorldStateID(string id)
		{
			return Game1.worldStateIDs.Contains(id) || Game1.netWorldState.Value.hasWorldStateID(id);
		}

		// Token: 0x06002207 RID: 8711 RVA: 0x00175069 File Offset: 0x00173269
		public static void addWorldStateIDEverywhere(string id)
		{
			Game1.netWorldState.Value.addWorldStateID(id);
			if (!Game1.worldStateIDs.Contains(id))
			{
				Game1.worldStateIDs.Add(id);
			}
		}

		// Token: 0x06002208 RID: 8712 RVA: 0x00175094 File Offset: 0x00173294
		public virtual void UpdateBundleDisplayNames()
		{
			List<string> list = new List<string>(this._bundleData.Keys);
			Dictionary<string, string> localizedBundleData = DataLoader.Bundles(Game1.content);
			foreach (string key in list)
			{
				string[] fields = this._bundleData[key].Split('/', StringSplitOptions.None);
				string bundleName = fields[0];
				if (!ArgUtility.HasIndex<string>(fields, 6))
				{
					Array.Resize<string>(ref fields, 7);
				}
				string displayName = null;
				foreach (string text in localizedBundleData.Values)
				{
					string[] localizedFields = text.Split('/', StringSplitOptions.None);
					if (ArgUtility.Get(localizedFields, 0, null, true) == bundleName)
					{
						displayName = ArgUtility.Get(localizedFields, 6, null, true);
						break;
					}
				}
				if (displayName == null)
				{
					displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\BundleNames:" + bundleName, true);
				}
				fields[6] = (displayName ?? bundleName);
				this._bundleData[key] = string.Join("/", fields);
			}
		}

		// Token: 0x06002209 RID: 8713 RVA: 0x001751D0 File Offset: 0x001733D0
		public bool hasWorldStateID(string id)
		{
			return this.worldStateIDs.Contains(id);
		}

		// Token: 0x0600220A RID: 8714 RVA: 0x001751DE File Offset: 0x001733DE
		public void addWorldStateID(string id)
		{
			this.worldStateIDs.Add(id);
		}

		// Token: 0x0600220B RID: 8715 RVA: 0x001751ED File Offset: 0x001733ED
		public void removeWorldStateID(string id)
		{
			this.worldStateIDs.Remove(id);
		}

		// Token: 0x0600220C RID: 8716 RVA: 0x001751FC File Offset: 0x001733FC
		public void SaveFarmhand(NetFarmerRoot farmhand)
		{
			NetRef<Farmer> farmhandData;
			if (Game1.netWorldState.Value.farmhandData.FieldDict.TryGetValue(farmhand.Value.UniqueMultiplayerID, out farmhandData))
			{
				farmhand.CloneInto(farmhandData);
			}
			this.ResetFarmhandState(farmhand.Value);
		}

		// Token: 0x0600220D RID: 8717 RVA: 0x00175244 File Offset: 0x00173444
		public void ResetFarmhandState(Farmer farmhand)
		{
			farmhand.farmName.Value = Game1.MasterPlayer.farmName.Value;
			if (this.TryAssignFarmhandHome(farmhand))
			{
				FarmHouse farmhandHome = Utility.getHomeOfFarmer(farmhand);
				if (farmhand.lastSleepLocation.Value == null || farmhand.lastSleepLocation.Value == farmhandHome.NameOrUniqueName)
				{
					farmhand.currentLocation = farmhandHome;
					farmhand.Position = Utility.PointToVector2(farmhandHome.GetPlayerBedSpot()) * 64f;
				}
			}
			else
			{
				farmhand.userID.Value = "";
				farmhand.homeLocation.Value = null;
				Game1.otherFarmers.Remove(farmhand.UniqueMultiplayerID);
			}
			farmhand.resetState();
		}

		// Token: 0x0600220E RID: 8718 RVA: 0x001752F8 File Offset: 0x001734F8
		public bool TryAssignFarmhandHome(Farmer farmhand)
		{
			if (farmhand.IsMainPlayer || Game1.getLocationFromName(farmhand.homeLocation.Value) is Cabin)
			{
				return true;
			}
			Cabin curLocation = farmhand.currentLocation as Cabin;
			if (curLocation != null && curLocation.CanAssignTo(farmhand))
			{
				curLocation.AssignFarmhand(farmhand);
				return true;
			}
			Cabin lastSleptCabin = Game1.getLocationFromName(farmhand.lastSleepLocation.Value) as Cabin;
			if (lastSleptCabin != null && lastSleptCabin.CanAssignTo(farmhand))
			{
				lastSleptCabin.AssignFarmhand(farmhand);
				return true;
			}
			bool found = false;
			Utility.ForEachBuilding(delegate(Building building)
			{
				Cabin cabin = building.GetIndoors() as Cabin;
				if (cabin != null && cabin.CanAssignTo(farmhand))
				{
					cabin.AssignFarmhand(farmhand);
					found = true;
					return false;
				}
				return true;
			}, true);
			return found;
		}

		// Token: 0x0600220F RID: 8719 RVA: 0x001753C8 File Offset: 0x001735C8
		public void UpdateFromGame1()
		{
			this.year.Value = Game1.year;
			this.season.Value = Game1.season;
			this.dayOfMonth.Value = Game1.dayOfMonth;
			this.timeOfDay.Value = Game1.timeOfDay;
			LocationWeather weatherForLocation = this.GetWeatherForLocation("Default");
			weatherForLocation.WeatherForTomorrow = Game1.weatherForTomorrow;
			weatherForLocation.IsRaining = Game1.isRaining;
			weatherForLocation.IsSnowing = Game1.isSnowing;
			weatherForLocation.IsDebrisWeather = Game1.isDebrisWeather;
			weatherForLocation.IsGreenRain = Game1.isGreenRain;
			this.isDebrisWeather.Value = Game1.isDebrisWeather;
			this.whichFarm.Value = Game1.whichFarm;
			this.weatherForTomorrow.Value = Game1.weatherForTomorrow;
			this.daysPlayed.Value = (int)Game1.stats.DaysPlayed;
			this.uniqueIDForThisGame.Value = (long)Game1.uniqueIDForThisGame;
			if (Game1.whichFarm != 7 || Game1.whichModFarm == null)
			{
				this.whichModFarm.Value = null;
			}
			else
			{
				this.whichModFarm.Value = Game1.whichModFarm.Id;
			}
			this.currentPlayerLimit.Value = Game1.multiplayer.playerLimit;
			this.highestPlayerLimit.Value = Math.Max(this.highestPlayerLimit.Value, Game1.multiplayer.playerLimit);
			this.worldStateIDs.Clear();
			this.worldStateIDs.AddRange(Game1.worldStateIDs);
		}

		// Token: 0x06002210 RID: 8720 RVA: 0x00175534 File Offset: 0x00173734
		public LocationWeather GetWeatherForLocation(string locationContextId)
		{
			LocationWeather weather;
			if (!this.locationWeather.TryGetValue(locationContextId, out weather))
			{
				weather = (this.locationWeather[locationContextId] = new LocationWeather());
				LocationContextData contextData;
				if (Game1.locationContextData.TryGetValue(locationContextId, out contextData))
				{
					weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
					weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
				}
			}
			return weather;
		}

		// Token: 0x06002211 RID: 8721 RVA: 0x00175590 File Offset: 0x00173790
		public void WriteToGame1(bool onLoad = false)
		{
			if (Game1.farmEvent != null)
			{
				return;
			}
			LocationWeather weatherForLocation = this.GetWeatherForLocation("Default");
			Game1.weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
			Game1.isRaining = weatherForLocation.IsRaining;
			Game1.isSnowing = weatherForLocation.IsSnowing;
			Game1.isLightning = weatherForLocation.IsLightning;
			Game1.isDebrisWeather = weatherForLocation.IsDebrisWeather;
			Game1.isGreenRain = weatherForLocation.IsGreenRain;
			Game1.weatherForTomorrow = this.weatherForTomorrow.Value;
			Game1.worldStateIDs = new HashSet<string>(this.worldStateIDs);
			if (!Game1.IsServer)
			{
				bool newSeason = Game1.season != this.season.Value;
				Game1.year = this.year.Value;
				Game1.season = this.season.Value;
				Game1.dayOfMonth = this.dayOfMonth.Value;
				Game1.timeOfDay = this.timeOfDay.Value;
				Game1.whichFarm = this.whichFarm.Value;
				if (Game1.whichFarm != 7)
				{
					Game1.whichModFarm = null;
				}
				else if (this._oldModFarmType != this.whichModFarm.Value)
				{
					this._oldModFarmType = this.whichModFarm.Value;
					Game1.whichModFarm = null;
					List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
					if (farm_types != null)
					{
						foreach (ModFarmType farm_type in farm_types)
						{
							if (farm_type.Id == this.whichModFarm.Value)
							{
								Game1.whichModFarm = farm_type;
								break;
							}
						}
					}
					if (Game1.whichModFarm == null)
					{
						throw new Exception(this.whichModFarm.Value + " is not a valid farm type.");
					}
				}
				Game1.stats.DaysPlayed = (uint)this.daysPlayed.Value;
				Game1.uniqueIDForThisGame = (ulong)this.uniqueIDForThisGame.Value;
				if (newSeason)
				{
					Game1.setGraphicsForSeason(onLoad);
				}
			}
			Game1.updateWeatherIcon();
			if (this.IsGoblinRemoved)
			{
				Game1.player.removeQuest("27");
			}
		}

		// Token: 0x06002212 RID: 8722 RVA: 0x0017579C File Offset: 0x0017399C
		public BuilderData GetBuilderData(string builderName)
		{
			BuilderData data;
			if (!this.builders.TryGetValue(builderName, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06002213 RID: 8723 RVA: 0x001757BC File Offset: 0x001739BC
		public void MarkUnderConstruction(string builderName, Building building)
		{
			int buildDays = building.daysOfConstructionLeft.Value;
			int upgradeDays = building.daysUntilUpgrade.Value;
			int daysUntilFinished = Math.Max(buildDays, upgradeDays);
			if (daysUntilFinished == 0)
			{
				return;
			}
			this.builders[builderName] = new BuilderData(building.buildingType.Value, daysUntilFinished, building.parentLocationName.Value, new Point(building.tileX.Value, building.tileY.Value), upgradeDays > 0 && buildDays <= 0);
		}

		// Token: 0x06002214 RID: 8724 RVA: 0x00175840 File Offset: 0x00173A40
		public void UpdateUnderConstruction()
		{
			foreach (KeyValuePair<string, BuilderData> pair in this.builders.Pairs.ToArray<KeyValuePair<string, BuilderData>>())
			{
				string builderName = pair.Key;
				BuilderData data = pair.Value;
				GameLocation location = Game1.getLocationFromName(data.buildingLocation.Value);
				if (location == null)
				{
					this.builders.Remove(builderName);
				}
				else
				{
					Building building = location.getBuildingAt(Utility.PointToVector2(data.buildingTile.Value));
					if (building == null || !building.isUnderConstruction(false))
					{
						this.builders.Remove(builderName);
					}
				}
			}
		}

		// Token: 0x06002215 RID: 8725 RVA: 0x001758E8 File Offset: 0x00173AE8
		public void UpdateBuildingCache(GameLocation location)
		{
			string name = location.NameOrUniqueName;
			if (location.buildings.Count > 0)
			{
				this.locationsWithBuildings.Add(name);
				return;
			}
			this.locationsWithBuildings.Remove(name);
		}

		// Token: 0x0400141A RID: 5146
		protected readonly NetLong uniqueIDForThisGame = new NetLong();

		// Token: 0x0400141B RID: 5147
		protected readonly NetEnum<ServerPrivacy> serverPrivacy = new NetEnum<ServerPrivacy>();

		// Token: 0x0400141C RID: 5148
		protected readonly NetInt whichFarm = new NetInt();

		// Token: 0x0400141D RID: 5149
		protected readonly NetString whichModFarm = new NetString();

		// Token: 0x0400141E RID: 5150
		protected string _oldModFarmType;

		// Token: 0x0400141F RID: 5151
		public readonly NetEnum<Game1.MineChestType> shuffleMineChests = new NetEnum<Game1.MineChestType>(Game1.MineChestType.Default);

		// Token: 0x04001420 RID: 5152
		public readonly NetInt minesDifficulty = new NetInt();

		// Token: 0x04001421 RID: 5153
		public readonly NetInt skullCavesDifficulty = new NetInt();

		// Token: 0x04001422 RID: 5154
		public readonly NetInt highestPlayerLimit = new NetInt(-1);

		// Token: 0x04001423 RID: 5155
		public readonly NetInt currentPlayerLimit = new NetInt(-1);

		// Token: 0x04001424 RID: 5156
		protected readonly NetInt year = new NetInt(1);

		// Token: 0x04001425 RID: 5157
		protected readonly NetEnum<Season> season = new NetEnum<Season>(Season.Spring);

		// Token: 0x04001426 RID: 5158
		protected readonly NetInt dayOfMonth = new NetInt(0);

		// Token: 0x04001427 RID: 5159
		protected readonly NetInt timeOfDay = new NetInt();

		// Token: 0x04001428 RID: 5160
		protected readonly NetInt daysPlayed = new NetInt();

		// Token: 0x04001429 RID: 5161
		public readonly NetInt visitsUntilY1Guarantee = new NetInt(-1);

		// Token: 0x0400142A RID: 5162
		protected readonly NetBool isPaused = new NetBool();

		// Token: 0x0400142B RID: 5163
		protected readonly NetBool isTimePaused = new NetBool
		{
			InterpolationWait = false
		};

		// Token: 0x0400142C RID: 5164
		protected readonly NetStringDictionary<LocationWeather, NetRef<LocationWeather>> locationWeather = new NetStringDictionary<LocationWeather, NetRef<LocationWeather>>();

		// Token: 0x0400142D RID: 5165
		protected readonly NetBool isRaining = new NetBool();

		// Token: 0x0400142E RID: 5166
		protected readonly NetBool isSnowing = new NetBool();

		// Token: 0x0400142F RID: 5167
		protected readonly NetBool isLightning = new NetBool();

		// Token: 0x04001430 RID: 5168
		protected readonly NetBool isDebrisWeather = new NetBool();

		// Token: 0x04001431 RID: 5169
		public readonly NetString weatherForTomorrow = new NetString();

		// Token: 0x04001432 RID: 5170
		protected readonly NetBundles bundles = new NetBundles();

		// Token: 0x04001433 RID: 5171
		protected readonly NetIntDictionary<bool, NetBool> bundleRewards = new NetIntDictionary<bool, NetBool>();

		// Token: 0x04001434 RID: 5172
		protected readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();

		// Token: 0x04001435 RID: 5173
		protected Dictionary<string, string> _bundleData;

		// Token: 0x04001436 RID: 5174
		protected bool _bundleDataDirty = true;

		// Token: 0x04001437 RID: 5175
		public readonly NetArray<bool, NetBool> raccoonBundles = new NetArray<bool, NetBool>(2);

		// Token: 0x04001438 RID: 5176
		public readonly NetInt seasonOfCurrentRacconBundle = new NetInt(-1);

		// Token: 0x04001439 RID: 5177
		public readonly NetBool parrotPlatformsUnlocked = new NetBool();

		// Token: 0x0400143A RID: 5178
		public readonly NetBool goblinRemoved = new NetBool();

		// Token: 0x0400143B RID: 5179
		public readonly NetBool submarineLocked = new NetBool();

		// Token: 0x0400143C RID: 5180
		public readonly NetInt lowestMineLevel = new NetInt();

		// Token: 0x0400143D RID: 5181
		public readonly NetInt lowestMineLevelForOrder = new NetInt(-1);

		// Token: 0x0400143E RID: 5182
		protected readonly NetVector2Dictionary<string, NetString> museumPieces = new NetVector2Dictionary<string, NetString>();

		// Token: 0x0400143F RID: 5183
		protected readonly NetIntDelta lostBooksFound = new NetIntDelta
		{
			Minimum = new int?(0),
			Maximum = new int?(21)
		};

		// Token: 0x04001440 RID: 5184
		protected readonly NetIntDelta goldenWalnuts = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001441 RID: 5185
		protected readonly NetIntDelta goldenWalnutsFound = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001442 RID: 5186
		protected readonly NetBool goldenCoconutCracked = new NetBool();

		// Token: 0x04001443 RID: 5187
		protected readonly NetStringHashSet foundBuriedNuts = new NetStringHashSet();

		// Token: 0x04001444 RID: 5188
		protected readonly NetIntDelta miniShippingBinsObtained = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001445 RID: 5189
		protected readonly NetIntDelta perfectionWaivers = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001446 RID: 5190
		protected readonly NetIntDelta timesFedRaccoons = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001447 RID: 5191
		protected readonly NetIntDelta treasureTotemsUsed = new NetIntDelta
		{
			Minimum = new int?(0)
		};

		// Token: 0x04001448 RID: 5192
		public NetLongDictionary<Farmer, NetRef<Farmer>> farmhandData = new NetLongDictionary<Farmer, NetRef<Farmer>>();

		// Token: 0x04001449 RID: 5193
		public readonly NetStringHashSet locationsWithBuildings = new NetStringHashSet();

		// Token: 0x0400144A RID: 5194
		public NetStringDictionary<BuilderData, NetRef<BuilderData>> builders = new NetStringDictionary<BuilderData, NetRef<BuilderData>>();

		// Token: 0x0400144B RID: 5195
		public NetStringHashSet activePassiveFestivals = new NetStringHashSet();

		// Token: 0x0400144C RID: 5196
		protected readonly NetStringHashSet worldStateIDs = new NetStringHashSet();

		// Token: 0x0400144D RID: 5197
		protected readonly NetStringHashSet islandVisitors = new NetStringHashSet();

		// Token: 0x0400144E RID: 5198
		protected readonly NetStringHashSet checkedGarbage = new NetStringHashSet();

		// Token: 0x0400144F RID: 5199
		public readonly NetRef<Object> dishOfTheDay = new NetRef<Object>();

		// Token: 0x04001450 RID: 5200
		private readonly NetBool activatedGoldenParrot = new NetBool();

		// Token: 0x04001451 RID: 5201
		private readonly NetInt daysPlayedWhenLastRaccoonBundleWasFinished = new NetInt();

		// Token: 0x04001452 RID: 5202
		public readonly NetBool canDriveYourselfToday = new NetBool();

		// Token: 0x04001453 RID: 5203
		public readonly NetBool goldenClocksTurnedOff = new NetBool();

		// Token: 0x04001454 RID: 5204
		protected readonly NetRef<Quest> netQuestOfTheDay = new NetRef<Quest>();
	}
}
