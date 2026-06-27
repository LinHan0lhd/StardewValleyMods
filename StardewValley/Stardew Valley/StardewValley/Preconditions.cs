using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Network.Dedicated;

namespace StardewValley
{
	// Token: 0x020000F3 RID: 243
	public class Preconditions
	{
		// Token: 0x060013CD RID: 5069 RVA: 0x000F33E4 File Offset: 0x000F15E4
		[OtherNames(new string[]
		{
			"e"
		})]
		public static bool SawEvent(GameLocation location, string eventId, string[] args)
		{
			for (int i = 1; i < args.Length; i++)
			{
				string id;
				string error;
				if (!ArgUtility.TryGet(args, i, out id, out error, false, "string id"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				if (Game1.player.eventsSeen.Contains(id))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013CE RID: 5070 RVA: 0x000F3434 File Offset: 0x000F1634
		[OtherNames(new string[]
		{
			"h"
		})]
		public static bool MissingPet(GameLocation location, string eventId, string[] args)
		{
			string petType;
			string error;
			if (!ArgUtility.TryGetOptional(args, 1, out petType, out error, null, false, "string petType"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return !Game1.player.hasPet() && (petType == null || petType.EqualsIgnoreCase(Game1.player.whichPetType));
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x000F3482 File Offset: 0x000F1682
		[OtherNames(new string[]
		{
			"H"
		})]
		public static bool IsHost(GameLocation location, string eventId, string[] args)
		{
			if (Game1.dedicatedServer != null)
			{
				Game1.dedicatedServer.CheckedHostPrecondition = true;
			}
			return Game1.IsMasterGame;
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x000F349C File Offset: 0x000F169C
		[OtherNames(new string[]
		{
			"Hn"
		})]
		public static bool HostMail(GameLocation location, string eventId, string[] args)
		{
			string mailId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out mailId, out error, false, "string mailId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.MasterPlayer.mailReceived.Contains(mailId);
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x000F34D8 File Offset: 0x000F16D8
		[Obsolete("New events should use !HostMail instead.")]
		[OtherNames(new string[]
		{
			"Hl"
		})]
		public static bool NotHostMail(GameLocation location, string eventId, string[] args)
		{
			string mailId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out mailId, out error, false, "string mailId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return !Game1.MasterPlayer.mailReceived.Contains(mailId);
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x000F3518 File Offset: 0x000F1718
		[OtherNames(new string[]
		{
			"*"
		})]
		public static bool WorldState(GameLocation location, string eventId, string[] args)
		{
			string worldStateId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out worldStateId, out error, false, "string worldStateId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return NetWorldState.checkAnywhereForWorldStateID(worldStateId);
		}

		// Token: 0x060013D3 RID: 5075 RVA: 0x000F3548 File Offset: 0x000F1748
		[OtherNames(new string[]
		{
			"*n"
		})]
		public static bool HostOrLocalMail(GameLocation location, string eventId, string[] args)
		{
			string mailId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out mailId, out error, false, "string mailId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.MasterPlayer.mailReceived.Contains(mailId) || Game1.player.mailReceived.Contains(mailId);
		}

		// Token: 0x060013D4 RID: 5076 RVA: 0x000F3596 File Offset: 0x000F1796
		[Obsolete("New events should use !HostOrLocalMail instead.")]
		[OtherNames(new string[]
		{
			"*l"
		})]
		public static bool NotHostOrLocalMail(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.HostOrLocalMail(location, eventId, args);
		}

		// Token: 0x060013D5 RID: 5077 RVA: 0x000F35A4 File Offset: 0x000F17A4
		[OtherNames(new string[]
		{
			"m"
		})]
		public static bool EarnedMoney(GameLocation location, string eventId, string[] args)
		{
			int minMoney;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out minMoney, out error, "int minMoney"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return (ulong)Game1.player.totalMoneyEarned >= (ulong)((long)minMoney);
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x000F35E0 File Offset: 0x000F17E0
		[OtherNames(new string[]
		{
			"M"
		})]
		public static bool HasMoney(GameLocation location, string eventId, string[] args)
		{
			int minMoney;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out minMoney, out error, "int minMoney"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.Money >= minMoney;
		}

		// Token: 0x060013D7 RID: 5079 RVA: 0x000F361C File Offset: 0x000F181C
		[OtherNames(new string[]
		{
			"c"
		})]
		public static bool FreeInventorySlots(GameLocation location, string eventId, string[] args)
		{
			int minFreeSpots;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out minFreeSpots, out error, "int minFreeSpots"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.freeSpotsInInventory() >= minFreeSpots;
		}

		// Token: 0x060013D8 RID: 5080 RVA: 0x000F3655 File Offset: 0x000F1855
		[OtherNames(new string[]
		{
			"C"
		})]
		public static bool CommunityCenterOrWarehouseDone(GameLocation location, string eventId, string[] args)
		{
			return Game1.MasterPlayer.eventsSeen.Contains("191393") || Game1.MasterPlayer.eventsSeen.Contains("502261") || Game1.MasterPlayer.hasCompletedCommunityCenter();
		}

		// Token: 0x060013D9 RID: 5081 RVA: 0x000F368F File Offset: 0x000F188F
		[Obsolete("New events should use !CommunityCenterOrWarehouseDone instead.")]
		[OtherNames(new string[]
		{
			"X"
		})]
		public static bool NotCommunityCenterOrWarehouseDone(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.CommunityCenterOrWarehouseDone(location, eventId, args);
		}

		// Token: 0x060013DA RID: 5082 RVA: 0x000F369C File Offset: 0x000F189C
		[OtherNames(new string[]
		{
			"D"
		})]
		public static bool Dating(GameLocation location, string eventId, string[] args)
		{
			string npcName;
			string error;
			if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			Friendship friendship;
			return Game1.player.friendshipData.TryGetValue(npcName, out friendship) && friendship.IsDating();
		}

		// Token: 0x060013DB RID: 5083 RVA: 0x000F36E4 File Offset: 0x000F18E4
		[OtherNames(new string[]
		{
			"j"
		})]
		public static bool DaysPlayed(GameLocation location, string eventId, string[] args)
		{
			int minDaysPlayed;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out minDaysPlayed, out error, "int minDaysPlayed"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return (ulong)Game1.stats.DaysPlayed > (ulong)((long)minDaysPlayed);
		}

		// Token: 0x060013DC RID: 5084 RVA: 0x000F371C File Offset: 0x000F191C
		[OtherNames(new string[]
		{
			"J"
		})]
		public static bool JojaBundlesDone(GameLocation location, string eventId, string[] args)
		{
			return Utility.hasFinishedJojaRoute();
		}

		// Token: 0x060013DD RID: 5085 RVA: 0x000F3724 File Offset: 0x000F1924
		[OtherNames(new string[]
		{
			"f"
		})]
		public static bool Friendship(GameLocation location, string eventId, string[] args)
		{
			for (int i = 1; i < args.Length; i += 2)
			{
				string npcName;
				string error;
				int minPoints;
				if (!ArgUtility.TryGet(args, i, out npcName, out error, false, "string npcName") || !ArgUtility.TryGetInt(args, i + 1, out minPoints, out error, "int minPoints"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				Friendship friendship;
				if (!Game1.player.friendshipData.TryGetValue(npcName, out friendship) || friendship.Points < minPoints)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060013DE RID: 5086 RVA: 0x000F3791 File Offset: 0x000F1991
		public static bool FestivalDay(GameLocation location, string eventId, string[] args)
		{
			return Utility.isFestivalDay();
		}

		// Token: 0x060013DF RID: 5087 RVA: 0x000F3798 File Offset: 0x000F1998
		[Obsolete("New events should use !FestivalDay instead.")]
		[OtherNames(new string[]
		{
			"F"
		})]
		public static bool NotFestivalDay(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.FestivalDay(location, eventId, args);
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x000F37A8 File Offset: 0x000F19A8
		[OtherNames(new string[]
		{
			"r"
		})]
		public static bool Random(GameLocation location, string eventId, string[] args)
		{
			float probability;
			string error;
			if (!ArgUtility.TryGetFloat(args, 1, out probability, out error, "float probability"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.random.NextDouble() <= (double)probability;
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x000F37E4 File Offset: 0x000F19E4
		[OtherNames(new string[]
		{
			"s"
		})]
		public static bool Shipped(GameLocation location, string eventId, string[] args)
		{
			for (int i = 1; i < args.Length; i += 2)
			{
				string itemId;
				string error;
				int minShipped;
				if (!ArgUtility.TryGet(args, i, out itemId, out error, false, "string itemId") || !ArgUtility.TryGetInt(args, i + 1, out minShipped, out error, "int minShipped"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				int countShipped;
				if (!Game1.player.basicShipped.TryGetValue(itemId, out countShipped) || countShipped < minShipped)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x000F384C File Offset: 0x000F1A4C
		[OtherNames(new string[]
		{
			"S"
		})]
		public static bool SawSecretNote(GameLocation location, string eventId, string[] args)
		{
			int secretNoteId;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out secretNoteId, out error, "int secretNoteId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.secretNotesSeen.Contains(secretNoteId);
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x000F3888 File Offset: 0x000F1A88
		[OtherNames(new string[]
		{
			"q"
		})]
		public static bool ChoseDialogueAnswers(GameLocation location, string eventId, string[] args)
		{
			for (int i = 1; i < args.Length; i++)
			{
				string answerId;
				string error;
				if (!ArgUtility.TryGet(args, i, out answerId, out error, false, "string answerId"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				if (!Game1.player.DialogueQuestionsAnswered.Contains(answerId))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x000F38D8 File Offset: 0x000F1AD8
		[OtherNames(new string[]
		{
			"n"
		})]
		public static bool LocalMail(GameLocation location, string eventId, string[] args)
		{
			string mailId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out mailId, out error, false, "string mailId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.mailReceived.Contains(mailId);
		}

		// Token: 0x060013E5 RID: 5093 RVA: 0x000F3914 File Offset: 0x000F1B14
		[OtherNames(new string[]
		{
			"N"
		})]
		public static bool GoldenWalnuts(GameLocation location, string eventId, string[] args)
		{
			int minWalnuts;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out minWalnuts, out error, "int minWalnuts"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.netWorldState.Value.GoldenWalnutsFound >= minWalnuts;
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x000F3952 File Offset: 0x000F1B52
		[Obsolete("New events should use !LocalMail instead.")]
		[OtherNames(new string[]
		{
			"l"
		})]
		public static bool NotLocalMail(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.LocalMail(location, eventId, args);
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x000F3960 File Offset: 0x000F1B60
		[OtherNames(new string[]
		{
			"L"
		})]
		public static bool InUpgradedHouse(GameLocation location, string eventId, string[] args)
		{
			int minUpgradeLevel;
			string error;
			if (!ArgUtility.TryGetOptionalInt(args, 1, out minUpgradeLevel, out error, 2, "int minUpgradeLevel"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			FarmHouse farmHouse = location as FarmHouse;
			return farmHouse != null && farmHouse.upgradeLevel >= minUpgradeLevel;
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x000F39A4 File Offset: 0x000F1BA4
		[OtherNames(new string[]
		{
			"t"
		})]
		public static bool Time(GameLocation location, string eventId, string[] args)
		{
			int minTime;
			string error;
			int maxTime;
			if (!ArgUtility.TryGetInt(args, 1, out minTime, out error, "int minTime") || !ArgUtility.TryGetInt(args, 2, out maxTime, out error, "int maxTime"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.timeOfDay >= minTime && Game1.timeOfDay <= maxTime;
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x000F39F4 File Offset: 0x000F1BF4
		[OtherNames(new string[]
		{
			"w"
		})]
		public static bool Weather(GameLocation location, string eventId, string[] args)
		{
			string weather;
			string error;
			if (!ArgUtility.TryGet(args, 1, out weather, out error, false, "string weather"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (weather == "rainy")
			{
				return location.IsRainingHere();
			}
			if (!(weather == "sunny"))
			{
				return weather == location.GetWeather().Weather;
			}
			return !location.IsRainingHere();
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x000F3A5C File Offset: 0x000F1C5C
		public static bool DayOfWeek(GameLocation location, string eventId, string[] args)
		{
			DayOfWeek actualDay = Game1.Date.DayOfWeek;
			for (int i = 1; i < args.Length; i++)
			{
				string rawDayName;
				string error;
				if (!ArgUtility.TryGet(args, i, out rawDayName, out error, false, "string rawDayName"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				DayOfWeek expectedDay;
				if (!WorldDate.TryGetDayOfWeekFor(rawDayName, out expectedDay))
				{
					return Event.LogPreconditionError(location, eventId, args, "can't parse '" + rawDayName + "' as a day of week");
				}
				if (actualDay == expectedDay)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x000F3ACA File Offset: 0x000F1CCA
		[Obsolete("New events should use !DayOfWeek instead.")]
		[OtherNames(new string[]
		{
			"d"
		})]
		public static bool NotDayOfWeek(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.DayOfWeek(location, eventId, args);
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x000F3AD8 File Offset: 0x000F1CD8
		[OtherNames(new string[]
		{
			"O"
		})]
		public static bool Spouse(GameLocation location, string eventId, string[] args)
		{
			string npcName;
			string error;
			if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.spouse == npcName;
		}

		// Token: 0x060013ED RID: 5101 RVA: 0x000F3B12 File Offset: 0x000F1D12
		[Obsolete("New events should use !Spouse instead.")]
		[OtherNames(new string[]
		{
			"o"
		})]
		public static bool NotSpouse(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.Spouse(location, eventId, args);
		}

		// Token: 0x060013EE RID: 5102 RVA: 0x000F3B1F File Offset: 0x000F1D1F
		[OtherNames(new string[]
		{
			"R"
		})]
		public static bool Roommate(GameLocation location, string eventId, string[] args)
		{
			return Game1.player.hasCurrentOrPendingRoommate();
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x000F3B2B File Offset: 0x000F1D2B
		[Obsolete("New events should use !Roommate instead.")]
		[OtherNames(new string[]
		{
			"Rf"
		})]
		public static bool NotRoommate(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.Roommate(location, eventId, args);
		}

		// Token: 0x060013F0 RID: 5104 RVA: 0x000F3B38 File Offset: 0x000F1D38
		[OtherNames(new string[]
		{
			"v"
		})]
		public static bool NpcVisible(GameLocation location, string eventId, string[] args)
		{
			string npcName;
			string error;
			if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			NPC characterFromName = Game1.getCharacterFromName(npcName, true, false);
			return characterFromName != null && !characterFromName.IsInvisible;
		}

		// Token: 0x060013F1 RID: 5105 RVA: 0x000F3B78 File Offset: 0x000F1D78
		[OtherNames(new string[]
		{
			"p"
		})]
		public static bool NpcVisibleHere(GameLocation location, string eventId, string[] args)
		{
			string npcName;
			string error;
			if (!ArgUtility.TryGet(args, 1, out npcName, out error, false, "string npcName"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			foreach (NPC i in location.characters)
			{
				if (i.Name == npcName && !i.IsInvisible)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x000F3C00 File Offset: 0x000F1E00
		public static bool Season(GameLocation location, string eventId, string[] args)
		{
			for (int i = 1; i < args.Length; i++)
			{
				Season season;
				string error;
				if (!ArgUtility.TryGetEnum<Season>(args, 1, out season, out error, "Season season"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				if (Game1.season == season)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x000F3C42 File Offset: 0x000F1E42
		[Obsolete("New events should use !Season instead.")]
		[OtherNames(new string[]
		{
			"z"
		})]
		public static bool NotSeason(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.Season(location, eventId, args);
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x000F3C4F File Offset: 0x000F1E4F
		[OtherNames(new string[]
		{
			"B"
		})]
		public static bool SpouseBed(GameLocation location, string eventId, string[] args)
		{
			return Utility.getHomeOfFarmer(Game1.player).GetSpouseBed() != null;
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x000F3C64 File Offset: 0x000F1E64
		[OtherNames(new string[]
		{
			"b"
		})]
		public static bool ReachedMineBottom(GameLocation location, string eventId, string[] args)
		{
			int minTimes;
			string error;
			if (!ArgUtility.TryGetOptionalInt(args, 1, out minTimes, out error, 1, "int minTimes"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.timesReachedMineBottom >= minTimes;
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x000F3CA0 File Offset: 0x000F1EA0
		[OtherNames(new string[]
		{
			"y"
		})]
		public static bool Year(GameLocation location, string eventId, string[] args)
		{
			int desiredYear;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out desiredYear, out error, "int desiredYear"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (desiredYear != 1)
			{
				return Game1.year >= desiredYear;
			}
			return Game1.year == 1;
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x000F3CE4 File Offset: 0x000F1EE4
		[OtherNames(new string[]
		{
			"g"
		})]
		public static bool Gender(GameLocation location, string eventId, string[] args)
		{
			string gender;
			string error;
			if (!ArgUtility.TryGet(args, 1, out gender, out error, false, "string gender"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			bool male = gender.EqualsIgnoreCase("male");
			return Game1.player.IsMale == male;
		}

		// Token: 0x060013F8 RID: 5112 RVA: 0x000F3D28 File Offset: 0x000F1F28
		[OtherNames(new string[]
		{
			"i"
		})]
		public static bool HasItem(GameLocation location, string eventId, string[] args)
		{
			string itemId;
			string error;
			if (!ArgUtility.TryGet(args, 1, out itemId, out error, false, "string itemId"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.Items.ContainsId(itemId) || (Game1.player.ActiveObject != null && ItemRegistry.HasItemId(Game1.player.ActiveObject, itemId));
		}

		// Token: 0x060013F9 RID: 5113 RVA: 0x000F3D84 File Offset: 0x000F1F84
		[Obsolete("New events should use !SawEvent instead.")]
		[OtherNames(new string[]
		{
			"k"
		})]
		public static bool NotSawEvent(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.SawEvent(location, eventId, args);
		}

		// Token: 0x060013FA RID: 5114 RVA: 0x000F3D94 File Offset: 0x000F1F94
		[OtherNames(new string[]
		{
			"a"
		})]
		public static bool Tile(GameLocation location, string eventId, string[] args)
		{
			Point point;
			if (!Game1.isWarping)
			{
				DedicatedServer dedicatedServer = Game1.dedicatedServer;
				if (dedicatedServer == null || !dedicatedServer.FakeWarp)
				{
					point = Game1.player.TilePoint;
					goto IL_35;
				}
			}
			point = new Point(Game1.xLocationAfterWarp, Game1.yLocationAfterWarp);
			IL_35:
			Point actualTile = point;
			for (int i = 1; i < args.Length - 1; i += 2)
			{
				Point tile;
				string error;
				if (!ArgUtility.TryGetPoint(args, i, out tile, out error, "Point tile"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				if (tile == actualTile)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013FB RID: 5115 RVA: 0x000F3E10 File Offset: 0x000F2010
		public static bool ActiveDialogueEvent(GameLocation location, string eventId, string[] args)
		{
			string id;
			string error;
			if (!ArgUtility.TryGet(args, 1, out id, out error, false, "string id"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			return Game1.player.activeDialogueEvents.ContainsKey(id);
		}

		// Token: 0x060013FC RID: 5116 RVA: 0x000F3E4A File Offset: 0x000F204A
		[Obsolete("New events should use !ActiveDialogueEvent instead.")]
		[OtherNames(new string[]
		{
			"A"
		})]
		public static bool NotActiveDialogueEvent(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.ActiveDialogueEvent(location, eventId, args);
		}

		// Token: 0x060013FD RID: 5117 RVA: 0x000F3E58 File Offset: 0x000F2058
		[Obsolete("This is a deprecated way to send mail using a hidden pseudo-event. Newer code should use Data/TriggerActions instead.")]
		[OtherNames(new string[]
		{
			"x"
		})]
		public static bool SendMail(GameLocation location, string eventId, string[] args)
		{
			string mailId;
			string error;
			bool inMailboxToday;
			if (!ArgUtility.TryGet(args, 1, out mailId, out error, false, "string mailId") || !ArgUtility.TryGetOptionalBool(args, 2, out inMailboxToday, out error, false, "bool inMailboxToday"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (inMailboxToday)
			{
				Game1.player.mailbox.Add(mailId);
			}
			else
			{
				Game1.addMailForTomorrow(mailId, false, false);
			}
			Game1.player.eventsSeen.Add(eventId);
			return false;
		}

		// Token: 0x060013FE RID: 5118 RVA: 0x000F3EC4 File Offset: 0x000F20C4
		[OtherNames(new string[]
		{
			"u"
		})]
		public static bool DayOfMonth(GameLocation location, string eventId, string[] args)
		{
			bool foundDay = false;
			for (int i = 1; i < args.Length; i++)
			{
				int day;
				string error;
				if (!ArgUtility.TryGetInt(args, i, out day, out error, "int day"))
				{
					return Event.LogPreconditionError(location, eventId, args, error);
				}
				if (Game1.dayOfMonth == day)
				{
					foundDay = true;
					break;
				}
			}
			return foundDay;
		}

		// Token: 0x060013FF RID: 5119 RVA: 0x000F3F0C File Offset: 0x000F210C
		public static bool UpcomingFestival(GameLocation location, string eventId, string[] args)
		{
			int numberOfDays;
			string error;
			if (!ArgUtility.TryGetInt(args, 1, out numberOfDays, out error, "int numberOfDays"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			Season season = Game1.season;
			int seasonIndex = Game1.seasonIndex;
			int day = Game1.dayOfMonth;
			for (int i = 0; i < numberOfDays; i++)
			{
				if (Utility.isFestivalDay(day, season))
				{
					return true;
				}
				day++;
				if (day > 28)
				{
					day = 1;
					season = (seasonIndex + StardewValley.Season.Summer) % (Season)4;
				}
			}
			return false;
		}

		// Token: 0x06001400 RID: 5120 RVA: 0x000F3F7A File Offset: 0x000F217A
		[Obsolete("New events should use !UpcomingFestival instead.")]
		[OtherNames(new string[]
		{
			"U"
		})]
		public static bool NotUpcomingFestival(GameLocation location, string eventId, string[] args)
		{
			return !Preconditions.UpcomingFestival(location, eventId, args);
		}

		// Token: 0x06001401 RID: 5121 RVA: 0x000F3F88 File Offset: 0x000F2188
		[OtherNames(new string[]
		{
			"G"
		})]
		public static bool GameStateQuery(GameLocation location, string eventId, string[] args)
		{
			string query = ArgUtility.UnsplitQuoteAware(args, ' ', 1, int.MaxValue);
			if (string.IsNullOrWhiteSpace(query))
			{
				return Event.LogPreconditionError(location, eventId, args, "must specify a game state query");
			}
			return StardewValley.GameStateQuery.CheckConditions(query, location, null, null, null, null, null);
		}

		// Token: 0x06001402 RID: 5122 RVA: 0x000F3FC8 File Offset: 0x000F21C8
		public static bool Skill(GameLocation location, string eventId, string[] args)
		{
			string name;
			string error;
			int minSkillLevel;
			if (!ArgUtility.TryGet(args, 1, out name, out error, false, "string name") || !ArgUtility.TryGetInt(args, 2, out minSkillLevel, out error, "int minSkillLevel"))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			int whichSkill = Farmer.getSkillNumberFromName(name);
			return Game1.player.GetUnmodifiedSkillLevel(whichSkill) >= minSkillLevel;
		}
	}
}
