using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Bundles;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crafting;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Fences;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.GarbageCans;
using StardewValley.GameData.GiantCrops;
using StardewValley.GameData.HomeRenovations;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.GameData.Minecarts;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Museum;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Powers;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.GameData.Weddings;
using StardewValley.GameData.WildTrees;
using StardewValley.GameData.WorldMaps;

namespace StardewValley
{
	// Token: 0x02000091 RID: 145
	public static class DataLoader
	{
		// Token: 0x06000609 RID: 1545 RVA: 0x00021F06 File Offset: 0x00020106
		public static Dictionary<int, string> Achievements(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<int, string>>(content, "Data\\Achievements");
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x00021F13 File Offset: 0x00020113
		public static List<ModFarmType> AdditionalFarms(LocalizedContentManager content)
		{
			return DataLoader.Load<List<ModFarmType>>(content, "Data\\AdditionalFarms");
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00021F20 File Offset: 0x00020120
		public static List<ModLanguage> AdditionalLanguages(LocalizedContentManager content)
		{
			return DataLoader.Load<List<ModLanguage>>(content, "Data\\AdditionalLanguages");
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00021F2D File Offset: 0x0002012D
		public static List<ModWallpaperOrFlooring> AdditionalWallpaperFlooring(LocalizedContentManager content)
		{
			return DataLoader.Load<List<ModWallpaperOrFlooring>>(content, "Data\\AdditionalWallpaperFlooring");
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x00021F3A File Offset: 0x0002013A
		public static Dictionary<string, string> AnimationDescriptions(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\animationDescriptions");
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00021F47 File Offset: 0x00020147
		public static Dictionary<string, string> AquariumFish(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\AquariumFish");
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00021F54 File Offset: 0x00020154
		public static Dictionary<string, AudioCueData> AudioChanges(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, AudioCueData>>(content, "Data\\AudioChanges");
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x00021F61 File Offset: 0x00020161
		public static Dictionary<string, BigCraftableData> BigCraftables(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, BigCraftableData>>(content, "Data\\BigCraftables");
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x00021F6E File Offset: 0x0002016E
		public static Dictionary<string, string> Boots(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Boots");
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x00021F7B File Offset: 0x0002017B
		public static Dictionary<string, BuffData> Buffs(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, BuffData>>(content, "Data\\Buffs");
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x00021F88 File Offset: 0x00020188
		public static Dictionary<string, BuildingData> Buildings(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, BuildingData>>(content, "Data\\Buildings");
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x00021F95 File Offset: 0x00020195
		public static Dictionary<string, string> Bundles(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Bundles");
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x00021FA2 File Offset: 0x000201A2
		public static Dictionary<string, string> ChairTiles(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\ChairTiles");
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x00021FAF File Offset: 0x000201AF
		public static Dictionary<string, CharacterData> Characters(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, CharacterData>>(content, "Data\\Characters");
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00021FBC File Offset: 0x000201BC
		public static List<ConcessionItemData> Concessions(LocalizedContentManager content)
		{
			return DataLoader.Load<List<ConcessionItemData>>(content, "Data\\Concessions");
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x00021FC9 File Offset: 0x000201C9
		public static List<ConcessionTaste> ConcessionTastes(LocalizedContentManager content)
		{
			return DataLoader.Load<List<ConcessionTaste>>(content, "Data\\ConcessionTastes");
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x00021FD6 File Offset: 0x000201D6
		public static Dictionary<string, string> CookingRecipes(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\CookingRecipes");
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x00021FE3 File Offset: 0x000201E3
		public static Dictionary<string, string> CraftingRecipes(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\CraftingRecipes");
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00021FF0 File Offset: 0x000201F0
		public static Dictionary<string, CropData> Crops(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, CropData>>(content, "Data\\Crops");
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00021FFD File Offset: 0x000201FD
		public static List<LostItem> LostItemsShop(LocalizedContentManager content)
		{
			return DataLoader.Load<List<LostItem>>(content, "Data\\LostItemsShop");
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x0002200A File Offset: 0x0002020A
		public static Dictionary<string, string> EngagementDialogue(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\EngagementDialogue");
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00022017 File Offset: 0x00020217
		public static Dictionary<string, FarmAnimalData> FarmAnimals(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, FarmAnimalData>>(content, "Data\\FarmAnimals");
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00022024 File Offset: 0x00020224
		public static Dictionary<string, FenceData> Fences(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, FenceData>>(content, "Data\\Fences");
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00022031 File Offset: 0x00020231
		public static Dictionary<string, string> Festivals_FestivalDates(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Festivals\\FestivalDates");
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x0002203E File Offset: 0x0002023E
		public static Dictionary<string, string> Fish(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Fish");
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x0002204B File Offset: 0x0002024B
		public static List<FishPondData> FishPondData(LocalizedContentManager content)
		{
			return DataLoader.Load<List<FishPondData>>(content, "Data\\FishPondData");
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00022058 File Offset: 0x00020258
		public static Dictionary<string, FloorPathData> FloorsAndPaths(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, FloorPathData>>(content, "Data\\FloorsAndPaths");
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00022065 File Offset: 0x00020265
		public static Dictionary<string, FruitTreeData> FruitTrees(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, FruitTreeData>>(content, "Data\\FruitTrees");
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00022072 File Offset: 0x00020272
		public static Dictionary<string, string> Furniture(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Furniture");
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x0002207F File Offset: 0x0002027F
		public static GarbageCanData GarbageCans(LocalizedContentManager content)
		{
			return DataLoader.Load<GarbageCanData>(content, "Data\\GarbageCans");
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x0002208C File Offset: 0x0002028C
		public static Dictionary<string, GiantCropData> GiantCrops(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, GiantCropData>>(content, "Data\\GiantCrops");
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00022099 File Offset: 0x00020299
		public static Dictionary<int, string> HairData(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<int, string>>(content, "Data\\HairData");
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x000220A6 File Offset: 0x000202A6
		public static Dictionary<string, string> Hats(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\hats");
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x000220B3 File Offset: 0x000202B3
		public static Dictionary<string, HomeRenovation> HomeRenovations(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, HomeRenovation>>(content, "Data\\HomeRenovations");
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x000220C0 File Offset: 0x000202C0
		public static Dictionary<string, IncomingPhoneCallData> IncomingPhoneCalls(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, IncomingPhoneCallData>>(content, "Data\\IncomingPhoneCalls");
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x000220CD File Offset: 0x000202CD
		public static Dictionary<string, JukeboxTrackData> JukeboxTracks(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, JukeboxTrackData>>(content, "Data\\JukeboxTracks");
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x000220DA File Offset: 0x000202DA
		public static Dictionary<string, LocationContextData> LocationContexts(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, LocationContextData>>(content, "Data\\LocationContexts");
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x000220E7 File Offset: 0x000202E7
		public static Dictionary<string, LocationData> Locations(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, LocationData>>(content, "Data\\Locations");
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x000220F4 File Offset: 0x000202F4
		public static Dictionary<string, MachineData> Machines(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, MachineData>>(content, "Data\\Machines");
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x00022101 File Offset: 0x00020301
		public static Dictionary<string, string> Mail(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\mail");
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0002210E File Offset: 0x0002030E
		public static List<MakeoverOutfit> MakeoverOutfits(LocalizedContentManager content)
		{
			return content.Load<List<MakeoverOutfit>>("Data\\MakeoverOutfits");
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0002211B File Offset: 0x0002031B
		public static Dictionary<string, MannequinData> Mannequins(LocalizedContentManager content)
		{
			return content.Load<Dictionary<string, MannequinData>>("Data\\Mannequins");
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x00022128 File Offset: 0x00020328
		public static Dictionary<string, MinecartNetworkData> Minecarts(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, MinecartNetworkData>>(content, "Data\\Minecarts");
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x00022135 File Offset: 0x00020335
		public static Dictionary<string, string> Monsters(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Monsters");
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x00022142 File Offset: 0x00020342
		public static Dictionary<string, MonsterSlayerQuestData> MonsterSlayerQuests(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, MonsterSlayerQuestData>>(content, "Data\\MonsterSlayerQuests");
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x0002214F File Offset: 0x0002034F
		public static List<MovieData> Movies(LocalizedContentManager content)
		{
			return DataLoader.Load<List<MovieData>>(content, "Data\\Movies");
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0002215C File Offset: 0x0002035C
		public static List<MovieCharacterReaction> MoviesReactions(LocalizedContentManager content)
		{
			return DataLoader.Load<List<MovieCharacterReaction>>(content, "Data\\MoviesReactions");
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x00022169 File Offset: 0x00020369
		public static Dictionary<string, MuseumRewards> MuseumRewards(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, MuseumRewards>>(content, "Data\\MuseumRewards");
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x00022176 File Offset: 0x00020376
		public static Dictionary<string, string> NpcGiftTastes(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\NPCGiftTastes");
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x00022183 File Offset: 0x00020383
		public static Dictionary<string, ObjectData> Objects(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, ObjectData>>(content, "Data\\Objects");
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x00022190 File Offset: 0x00020390
		public static Dictionary<string, string> PaintData(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\PaintData");
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0002219D File Offset: 0x0002039D
		public static Dictionary<string, PantsData> Pants(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, PantsData>>(content, "Data\\Pants");
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x000221AA File Offset: 0x000203AA
		public static Dictionary<string, PassiveFestivalData> PassiveFestivals(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, PassiveFestivalData>>(content, "Data\\PassiveFestivals");
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x000221B7 File Offset: 0x000203B7
		public static Dictionary<string, PetData> Pets(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, PetData>>(content, "Data\\Pets");
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x000221C4 File Offset: 0x000203C4
		public static Dictionary<string, PowersData> Powers(LocalizedContentManager content)
		{
			return content.Load<Dictionary<string, PowersData>>("Data\\Powers");
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x000221D1 File Offset: 0x000203D1
		public static Dictionary<string, string> Quests(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\Quests");
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x000221DE File Offset: 0x000203DE
		public static List<RandomBundleData> RandomBundles(LocalizedContentManager content)
		{
			return DataLoader.Load<List<RandomBundleData>>(content, "Data\\RandomBundles");
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x000221EB File Offset: 0x000203EB
		public static Dictionary<int, string> SecretNotes(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<int, string>>(content, "Data\\SecretNotes");
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x000221F8 File Offset: 0x000203F8
		public static Dictionary<string, ShirtData> Shirts(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, ShirtData>>(content, "Data\\Shirts");
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x00022205 File Offset: 0x00020405
		public static Dictionary<string, ShopData> Shops(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, ShopData>>(content, "Data\\Shops");
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x00022212 File Offset: 0x00020412
		public static Dictionary<string, SpecialOrderData> SpecialOrders(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, SpecialOrderData>>(content, "Data\\SpecialOrders");
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0002221F File Offset: 0x0002041F
		public static List<TailorItemRecipe> TailoringRecipes(LocalizedContentManager content)
		{
			return DataLoader.Load<List<TailorItemRecipe>>(content, "Data\\TailoringRecipes");
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x0002222C File Offset: 0x0002042C
		public static Dictionary<string, ToolData> Tools(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, ToolData>>(content, "Data\\Tools");
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x00022239 File Offset: 0x00020439
		public static List<TriggerActionData> TriggerActions(LocalizedContentManager content)
		{
			return DataLoader.Load<List<TriggerActionData>>(content, "Data\\TriggerActions");
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x00022246 File Offset: 0x00020446
		public static Dictionary<string, TrinketData> Trinkets(LocalizedContentManager content)
		{
			return content.Load<Dictionary<string, TrinketData>>("Data\\Trinkets");
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x00022253 File Offset: 0x00020453
		public static Dictionary<string, WeaponData> Weapons(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, WeaponData>>(content, "Data\\Weapons");
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x00022260 File Offset: 0x00020460
		public static WeddingData Weddings(LocalizedContentManager content)
		{
			return DataLoader.Load<WeddingData>(content, "Data\\Weddings");
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0002226D File Offset: 0x0002046D
		public static Dictionary<string, WildTreeData> WildTrees(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, WildTreeData>>(content, "Data\\WildTrees");
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0002227A File Offset: 0x0002047A
		public static Dictionary<string, WorldMapRegionData> WorldMap(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, WorldMapRegionData>>(content, "Data\\WorldMap");
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x00022287 File Offset: 0x00020487
		public static Dictionary<string, string> Tv_CookingChannel(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\TV\\CookingChannel");
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x00022294 File Offset: 0x00020494
		public static Dictionary<string, string> Tv_TipChannel(LocalizedContentManager content)
		{
			return DataLoader.Load<Dictionary<string, string>>(content, "Data\\TV\\TipChannel");
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x000222A4 File Offset: 0x000204A4
		private static TAsset Load<TAsset>(LocalizedContentManager content, string assetName)
		{
			TAsset result;
			try
			{
				result = content.Load<TAsset>(assetName);
			}
			catch (Exception ex)
			{
				throw new ContentLoadException("Failed loading asset '" + assetName + "'.", ex);
			}
			return result;
		}
	}
}
