using System;
using System.Runtime.CompilerServices;

namespace StardewValley.Constants
{
	// Token: 0x0200036B RID: 875
	public class StatKeys
	{
		// Token: 0x060035D1 RID: 13777 RVA: 0x002A78B8 File Offset: 0x002A5AB8
		public static string Mastery(int skill)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("mastery_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(skill);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060035D2 RID: 13778 RVA: 0x002A78EC File Offset: 0x002A5AEC
		public static string SquidFestScore(int dayOfMonth, int year)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 2);
			defaultInterpolatedStringHandler.AppendLiteral("SquidFestScore_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(dayOfMonth);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(year);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040022EB RID: 8939
		public const string AverageBedtime = "averageBedtime";

		// Token: 0x040022EC RID: 8940
		public const string BeachFarmSpawns = "beachFarmSpawns";

		// Token: 0x040022ED RID: 8941
		public const string BeveragesMade = "beveragesMade";

		// Token: 0x040022EE RID: 8942
		public const string BillboardQuestsDone = "BillboardQuestsDone";

		// Token: 0x040022EF RID: 8943
		public const string BlessingOfWaters = "blessingOfWaters";

		// Token: 0x040022F0 RID: 8944
		public const string BoatRidesToIsland = "boatRidesToIsland";

		// Token: 0x040022F1 RID: 8945
		public const string Book_Bomb = "Book_Bombs";

		// Token: 0x040022F2 RID: 8946
		public const string Book_Crabbing = "Book_Crabbing";

		// Token: 0x040022F3 RID: 8947
		public const string Book_Defense = "Book_Defense";

		// Token: 0x040022F4 RID: 8948
		public const string Book_Friendship = "Book_Friendship";

		// Token: 0x040022F5 RID: 8949
		public const string Book_Marlon = "Book_Marlon";

		// Token: 0x040022F6 RID: 8950
		public const string Book_PriceCatalogue = "Book_PriceCatalogue";

		// Token: 0x040022F7 RID: 8951
		public const string Book_Roe = "Book_Roe";

		// Token: 0x040022F8 RID: 8952
		public const string Book_Speed = "Book_Speed";

		// Token: 0x040022F9 RID: 8953
		public const string Book_Speed2 = "Book_Speed2";

		// Token: 0x040022FA RID: 8954
		public const string Book_Trash = "Book_Trash";

		// Token: 0x040022FB RID: 8955
		public const string Book_Void = "Book_Void";

		// Token: 0x040022FC RID: 8956
		public const string Book_WildSeeds = "Book_WildSeeds";

		// Token: 0x040022FD RID: 8957
		public const string Book_Woodcutting = "Book_Woodcutting";

		// Token: 0x040022FE RID: 8958
		public const string Book_Diamonds = "Book_Diamonds";

		// Token: 0x040022FF RID: 8959
		public const string Book_Mystery = "Book_Mystery";

		// Token: 0x04002300 RID: 8960
		public const string Book_AnimalCatalogue = "Book_AnimalCatalogue";

		// Token: 0x04002301 RID: 8961
		public const string Book_Horse = "Book_Horse";

		// Token: 0x04002302 RID: 8962
		public const string Book_Artifact = "Book_Artifact";

		// Token: 0x04002303 RID: 8963
		public const string Book_Grass = "Book_Grass";

		// Token: 0x04002304 RID: 8964
		public const string CaveCarrotsFound = "caveCarrotsFound";

		// Token: 0x04002305 RID: 8965
		public const string CheeseMade = "cheeseMade";

		// Token: 0x04002306 RID: 8966
		public const string ChickenEggsLayed = "chickenEggsLayed";

		// Token: 0x04002307 RID: 8967
		public const string ChildrenTurnedToDoves = "childrenTurnedToDoves";

		// Token: 0x04002308 RID: 8968
		public const string CompletedJunimoKart = "completedJunimoKart";

		// Token: 0x04002309 RID: 8969
		public const string CompletedPrairieKing = "completedPrairieKing";

		// Token: 0x0400230A RID: 8970
		public const string CompletedPrairieKingWithoutDying = "completedPrairieKingWithoutDying";

		// Token: 0x0400230B RID: 8971
		public const string CopperFound = "copperFound";

		// Token: 0x0400230C RID: 8972
		public const string CowMilkProduced = "cowMilkProduced";

		// Token: 0x0400230D RID: 8973
		public const string CropsShipped = "cropsShipped";

		// Token: 0x0400230E RID: 8974
		public const string DaysPlayed = "daysPlayed";

		// Token: 0x0400230F RID: 8975
		public const string DiamondsFound = "diamondsFound";

		// Token: 0x04002310 RID: 8976
		public const string DirtHoed = "dirtHoed";

		// Token: 0x04002311 RID: 8977
		public const string DuckEggsLayed = "duckEggsLayed";

		// Token: 0x04002312 RID: 8978
		public const string ExMemoriesWiped = "exMemoriesWiped";

		// Token: 0x04002313 RID: 8979
		public const string FishCaught = "fishCaught";

		// Token: 0x04002314 RID: 8980
		public const string GeodesCracked = "geodesCracked";

		// Token: 0x04002315 RID: 8981
		public const string GiftsGiven = "giftsGiven";

		// Token: 0x04002316 RID: 8982
		public const string GoatCheeseMade = "goatCheeseMade";

		// Token: 0x04002317 RID: 8983
		public const string GoatMilkProduced = "goatMilkProduced";

		// Token: 0x04002318 RID: 8984
		public const string GoldenTagsTurnedIn = "GoldenTagsTurnedIn";

		// Token: 0x04002319 RID: 8985
		public const string GoldFound = "goldFound";

		// Token: 0x0400231A RID: 8986
		public const string GoodFriends = "goodFriends";

		// Token: 0x0400231B RID: 8987
		public const string HardModeMonstersKilled = "hardModeMonstersKilled";

		// Token: 0x0400231C RID: 8988
		public const string IndividualMoneyEarned = "individualMoneyEarned";

		// Token: 0x0400231D RID: 8989
		public const string IridiumFound = "iridiumFound";

		// Token: 0x0400231E RID: 8990
		public const string IronFound = "ironFound";

		// Token: 0x0400231F RID: 8991
		public const string ItemsCooked = "itemsCooked";

		// Token: 0x04002320 RID: 8992
		public const string ItemsCrafted = "itemsCrafted";

		// Token: 0x04002321 RID: 8993
		public const string ItemsForaged = "itemsForaged";

		// Token: 0x04002322 RID: 8994
		public const string ItemsShipped = "itemsShipped";

		// Token: 0x04002323 RID: 8995
		public const string MasteryExp = "MasteryExp";

		// Token: 0x04002324 RID: 8996
		public const string MasteryLevelsSpent = "masteryLevelsSpent";

		// Token: 0x04002325 RID: 8997
		public const string MonstersKilled = "monstersKilled";

		// Token: 0x04002326 RID: 8998
		public const string MossHarvested = "mossHarvested";

		// Token: 0x04002327 RID: 8999
		public const string MysteryBoxesOpened = "MysteryBoxesOpened";

		// Token: 0x04002328 RID: 9000
		public const string MysticStonesCrushed = "mysticStonesCrushed";

		// Token: 0x04002329 RID: 9001
		public const string NotesFound = "notesFound";

		// Token: 0x0400232A RID: 9002
		public const string OtherPreciousGemsFound = "otherPreciousGemsFound";

		// Token: 0x0400232B RID: 9003
		public const string PiecesOfTrashRecycled = "piecesOfTrashRecycled";

		// Token: 0x0400232C RID: 9004
		public const string PreservesMade = "preservesMade";

		// Token: 0x0400232D RID: 9005
		public const string PrismaticShardsFound = "prismaticShardsFound";

		// Token: 0x0400232E RID: 9006
		public const string QuestsCompleted = "questsCompleted";

		// Token: 0x0400232F RID: 9007
		public const string RabbitWoolProduced = "rabbitWoolProduced";

		// Token: 0x04002330 RID: 9008
		public const string RocksCrushed = "rocksCrushed";

		// Token: 0x04002331 RID: 9009
		public const string SheepWoolProduced = "sheepWoolProduced";

		// Token: 0x04002332 RID: 9010
		public const string SlimesKilled = "slimesKilled";

		// Token: 0x04002333 RID: 9011
		public const string SpecialOrderPrizeTickets = "specialOrderPrizeTickets";

		// Token: 0x04002334 RID: 9012
		public const string StepsTaken = "stepsTaken";

		// Token: 0x04002335 RID: 9013
		public const string StoneGathered = "stoneGathered";

		// Token: 0x04002336 RID: 9014
		public const string StumpsChopped = "stumpsChopped";

		// Token: 0x04002337 RID: 9015
		public const string TicketPrizesClaimed = "ticketPrizesClaimed";

		// Token: 0x04002338 RID: 9016
		public const string TimesEnchanted = "timesEnchanted";

		// Token: 0x04002339 RID: 9017
		public const string TimesFished = "timesFished";

		// Token: 0x0400233A RID: 9018
		public const string TimesUnconscious = "timesUnconscious";

		// Token: 0x0400233B RID: 9019
		public const string TotalMoneyGifted = "totalMoneyGifted";

		// Token: 0x0400233C RID: 9020
		public const string TrashCansChecked = "trashCansChecked";

		// Token: 0x0400233D RID: 9021
		public const string TrinketSlots = "trinketSlots";

		// Token: 0x0400233E RID: 9022
		public const string TrufflesFound = "trufflesFound";

		// Token: 0x0400233F RID: 9023
		public const string WeedsEliminated = "weedsEliminated";

		// Token: 0x04002340 RID: 9024
		public const string WildTreesPlanted = "wildtreesplanted";

		// Token: 0x04002341 RID: 9025
		public const string SeedsSown = "seedsSown";
	}
}
