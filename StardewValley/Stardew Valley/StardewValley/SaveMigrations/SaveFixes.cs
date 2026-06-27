using System;

namespace StardewValley.SaveMigrations
{
	// Token: 0x02000182 RID: 386
	public enum SaveFixes
	{
		// Token: 0x0400111C RID: 4380
		NONE,
		// Token: 0x0400111D RID: 4381
		StoredBigCraftablesStackFix,
		// Token: 0x0400111E RID: 4382
		PorchedCabinBushesFix,
		// Token: 0x0400111F RID: 4383
		ChangeObeliskFootprintHeight,
		// Token: 0x04001120 RID: 4384
		CreateStorageDressers,
		// Token: 0x04001121 RID: 4385
		InferPreserves,
		// Token: 0x04001122 RID: 4386
		TransferHatSkipHairFlag,
		// Token: 0x04001123 RID: 4387
		RevealSecretNoteItemTastes,
		// Token: 0x04001124 RID: 4388
		TransferHoneyTypeToPreserves,
		// Token: 0x04001125 RID: 4389
		TransferNoteBlockScale,
		// Token: 0x04001126 RID: 4390
		FixCropHarvestAmountsAndInferSeedIndex,
		// Token: 0x04001127 RID: 4391
		quarryMineBushes = 13,
		// Token: 0x04001128 RID: 4392
		MissingQisChallenge,
		// Token: 0x04001129 RID: 4393
		BedsToFurniture,
		// Token: 0x0400112A RID: 4394
		ChildBedsToFurniture,
		// Token: 0x0400112B RID: 4395
		ModularizeFarmStructures,
		// Token: 0x0400112C RID: 4396
		FixFlooringFlags,
		// Token: 0x0400112D RID: 4397
		FixStableOwnership,
		// Token: 0x0400112E RID: 4398
		AddTownBush = 21,
		// Token: 0x0400112F RID: 4399
		ResetForges = 23,
		// Token: 0x04001130 RID: 4400
		MakeDarkSwordVampiric = 25,
		// Token: 0x04001131 RID: 4401
		FixBeachFarmBushes = 27,
		// Token: 0x04001132 RID: 4402
		OstrichIncubatorFragility = 30,
		// Token: 0x04001133 RID: 4403
		LeoChildrenFix = 32,
		// Token: 0x04001134 RID: 4404
		Leo6HeartGermanFix,
		// Token: 0x04001135 RID: 4405
		BirdieQuestRemovedFix,
		// Token: 0x04001136 RID: 4406
		SkippedSummit,
		// Token: 0x04001137 RID: 4407
		MigrateBuildingsToData = 37,
		// Token: 0x04001138 RID: 4408
		ModularizeFarmhouse,
		// Token: 0x04001139 RID: 4409
		ModularizePets,
		// Token: 0x0400113A RID: 4410
		AddNpcRemovalFlags = 42,
		// Token: 0x0400113B RID: 4411
		MigrateFarmhands = 44,
		// Token: 0x0400113C RID: 4412
		MigrateLitterItemData,
		// Token: 0x0400113D RID: 4413
		MigrateHoneyItems = 47,
		// Token: 0x0400113E RID: 4414
		MigrateMachineLastOutputRule,
		// Token: 0x0400113F RID: 4415
		StandardizeBundleFields,
		// Token: 0x04001140 RID: 4416
		MigrateAdventurerGoalFlags = 51,
		// Token: 0x04001141 RID: 4417
		SetCropSeedId = 53,
		// Token: 0x04001142 RID: 4418
		FixMineBoulderCollisions,
		// Token: 0x04001143 RID: 4419
		MigratePetAndPetBowlIds = 56,
		// Token: 0x04001144 RID: 4420
		MigrateHousePaint = 58,
		// Token: 0x04001145 RID: 4421
		MigrateShedFloorWallIds = 61,
		// Token: 0x04001146 RID: 4422
		MigrateItemIds,
		// Token: 0x04001147 RID: 4423
		RemoveMeatFromAnimalBundle,
		// Token: 0x04001148 RID: 4424
		RemoveMasteryRoomFoliage = 65,
		// Token: 0x04001149 RID: 4425
		AddTownTrees,
		// Token: 0x0400114A RID: 4426
		MapAdjustments_1_6,
		// Token: 0x0400114B RID: 4427
		MigrateWalletItems,
		// Token: 0x0400114C RID: 4428
		MigrateResourceClumps,
		// Token: 0x0400114D RID: 4429
		MigrateFishingRodAttachmentSlots,
		// Token: 0x0400114E RID: 4430
		MoveSlimeHutches = 72,
		// Token: 0x0400114F RID: 4431
		AddLocationsVisited = 74,
		// Token: 0x04001150 RID: 4432
		MarkStarterGiftBoxes,
		// Token: 0x04001151 RID: 4433
		MigrateMailEventsToTriggerActions,
		// Token: 0x04001152 RID: 4434
		ShiftFarmHouseFurnitureForExpansion,
		// Token: 0x04001153 RID: 4435
		MigratePreservesTo16,
		// Token: 0x04001154 RID: 4436
		MigrateQuestDataTo16,
		// Token: 0x04001155 RID: 4437
		SetBushesInPots,
		// Token: 0x04001156 RID: 4438
		FixItemsNotMarkedAsInInventory,
		// Token: 0x04001157 RID: 4439
		BetaFixesFor16,
		// Token: 0x04001158 RID: 4440
		FixBasicWines,
		// Token: 0x04001159 RID: 4441
		ResetForges_1_6,
		// Token: 0x0400115A RID: 4442
		RestoreAncientSeedRecipe_1_6,
		// Token: 0x0400115B RID: 4443
		FixInstancedInterior,
		// Token: 0x0400115C RID: 4444
		FixNonInstancedInterior,
		// Token: 0x0400115D RID: 4445
		PopulateConstructedBuildings,
		// Token: 0x0400115E RID: 4446
		FixRacoonQuestCompletion,
		// Token: 0x0400115F RID: 4447
		RestoreDwarvish,
		// Token: 0x04001160 RID: 4448
		FixTubOFlowers,
		// Token: 0x04001161 RID: 4449
		MigrateStatFields,
		// Token: 0x04001162 RID: 4450
		MakeWildSeedsDeterministic,
		// Token: 0x04001163 RID: 4451
		FixTranslatedInternalNames,
		// Token: 0x04001164 RID: 4452
		ConvertBuildingQuests,
		// Token: 0x04001165 RID: 4453
		AddJunimoKartAndPrairieKingStats,
		// Token: 0x04001166 RID: 4454
		FixEmptyLostAndFoundItemStacks,
		// Token: 0x04001167 RID: 4455
		FixDuplicateMissedMail,
		// Token: 0x04001168 RID: 4456
		MAX
	}
}
