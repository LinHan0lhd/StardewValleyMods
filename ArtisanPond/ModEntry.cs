using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPonds;

namespace ArtisanPond;

public class ModEntry : Mod
{
    public static ModConfig Config = null!;
    public static IMonitor MonitorRef = null!;

    private const string MoodKey = "AP.Mood";
    private const string FriendshipKey = "AP.Friendship";
    private const string PopKey = "AP.LastPop";
    private const string HadOutputKey = "AP.HadOutput";
    private const string RewardedKey = "AP.Rewarded";
    private const string InsuranceKey = "AP.Insurance";
    private const string LastNeededKey = "AP.LastNeeded";
    private const string QuestStartDayKey = "AP.QuestStartDay";
    private const string QuestOriginalCountKey = "AP.QuestOriginalCount";
    private const string LegendaryNextQuestDayKey = "AP.LegendaryNextQuestDay";

    private static List<(string itemId, int category)>? _questItemsCache;

    public override void Entry(IModHelper helper)
    {
        Config = helper.ReadConfig<ModConfig>();
        MonitorRef = Monitor;

        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(FishPond), nameof(FishPond.HasUnresolvedNeeds)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(HasUnresolvedNeeds_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(FishPond), nameof(FishPond.dayUpdate)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(DayUpdate_Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(FishPond), nameof(FishPond.SpawnFish)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(SpawnFish_Prefix))
        );
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!e.NameWithoutLocale.IsEquivalentTo("Data/FishPondData"))
            return;

        e.Edit(asset =>
        {
            var list = asset.GetData<List<FishPondData>>();
            int maxCap = Math.Clamp(Config.LegendaryPondMaxCapacity, 1, 10);

            foreach (var entry in list)
            {
                if (entry.Id == "LegendaryFish")
                    continue;

                entry.PopulationGates ??= new Dictionary<int, List<string>>();
                if (!entry.PopulationGates.ContainsKey(11))
                    entry.PopulationGates[11] = new List<string> { "(O)685 1" };
            }

            var legendary = list.FirstOrDefault(e => e.Id == "LegendaryFish");
            if (legendary != null)
            {
                legendary.MaxPopulation = -1;
                legendary.PopulationGates ??= new Dictionary<int, List<string>>();

                for (int i = 2; i <= maxCap; i++)
                {
                    if (!legendary.PopulationGates.ContainsKey(i))
                        legendary.PopulationGates[i] = new List<string> { "(O)560 1" };
                }
            }
        });
    }

    private static void HasUnresolvedNeeds_Postfix(FishPond __instance, ref bool __result)
    {
        if (!__result
            && __instance.neededItem.Value != null
            && !__instance.hasCompletedRequest.Value)
        {
            __result = true;
        }
    }

    private static bool SpawnFish_Prefix(FishPond __instance)
    {
        if (!Config.EnableLegendaryPondExpansion) return true;
        if (!IsLegendaryFish(__instance)) return true;
        return false;
    }

    private static void DayUpdate_Postfix(FishPond __instance)
    {
        if (!Game1.IsMasterGame) return;

        if (Config.EnableLegendaryPondExpansion && IsLegendaryFish(__instance))
        {
            TryGenerateLegendaryInfiniteQuest(__instance);
        }

        if (Config.EnableInfinitePondQuests && !IsLegendaryFish(__instance))
        {
            TryGenerateInfiniteQuest(__instance);
        }

        if (__instance.output.Value is not StardewValley.Object outputItem)
            return;
        if (outputItem.ItemId != "812")
            return;
        if (outputItem.modData.ContainsKey("AP.QualityFixed"))
            return;

        int mood = 0;
        if (__instance.modData.ContainsKey(MoodKey))
            int.TryParse(__instance.modData[MoodKey], out mood);

        int q = outputItem.Quality == 0 ? CalculateQuality(__instance, mood) : outputItem.Quality;
        int final = TryUpgradeWithInsurance(__instance, q);

        if (final != outputItem.Quality)
            outputItem.Quality = final;

        outputItem.modData["AP.QualityFixed"] = "true";
    }

    private static void TryGenerateLegendaryInfiniteQuest(FishPond pond)
    {
        if (pond.currentOccupants.Value < pond.maxOccupants.Value) return;
        if (pond.neededItem.Value != null) return;
        if (pond.hasCompletedRequest.Value) return;

        var data = pond.GetFishPondData();
        if (data?.PopulationGates == null) return;

        int maxCap = Math.Clamp(Config.LegendaryPondMaxCapacity, 1, 10);
        int nextGate = pond.maxOccupants.Value + 1;

        if (nextGate <= maxCap && data.PopulationGates.ContainsKey(nextGate))
            return;

        int friendship = 0;
        if (pond.modData.ContainsKey(FriendshipKey))
            int.TryParse(pond.modData[FriendshipKey], out friendship);

        int interval = 1 + (friendship * 6 / 100);
        interval = Math.Clamp(interval, 1, 7);

        int today = (int)Game1.stats.DaysPlayed;
        int nextQuestDay = 0;
        if (pond.modData.ContainsKey(LegendaryNextQuestDayKey))
            int.TryParse(pond.modData[LegendaryNextQuestDayKey], out nextQuestDay);

        if (nextQuestDay == 0)
        {
            pond.modData[LegendaryNextQuestDayKey] = (today + interval).ToString();
            return;
        }

        if (today < nextQuestDay) return;

        pond.neededItem.Value = ItemRegistry.Create("(O)74", 1, 0, false);
        pond.neededItemCount.Value = 1;
        pond.modData[LegendaryNextQuestDayKey] = (today + interval).ToString();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!Game1.IsMasterGame) return;

        foreach (GameLocation loc in Game1.locations)
        {
            foreach (Building b in loc.buildings)
            {
                if (b is not FishPond pond) continue;

                if (Config.EnableMoodSystem)
                    UpdatePond(pond, loc);
            }
        }
    }

    private static void TryGenerateInfiniteQuest(FishPond pond)
    {
        if (pond.currentOccupants.Value < pond.maxOccupants.Value) return;
        if (pond.maxOccupants.Value < 10) return;

        pond.lastUnlockedPopulationGate.Value = 10;

        if (pond.neededItem.Value != null && !pond.hasCompletedRequest.Value)
        {
            int originalCount = 0;
            if (pond.modData.ContainsKey(QuestOriginalCountKey))
                int.TryParse(pond.modData[QuestOriginalCountKey], out originalCount);

            if (pond.neededItemCount.Value < originalCount)
                return;

            int startDay = 0;
            if (pond.modData.ContainsKey(QuestStartDayKey))
                int.TryParse(pond.modData[QuestStartDayKey], out startDay);

            int daysPassed = (int)Game1.stats.DaysPlayed - startDay;
            if (daysPassed < Config.InfiniteQuestRefreshInterval)
                return;

            var (newItemId, count) = PickRandomQuestItem();
            if (newItemId != null)
            {
                pond.neededItem.Value = ItemRegistry.Create(newItemId, 1, 0, false);
                pond.neededItemCount.Value = count;
                pond.modData[QuestStartDayKey] = Game1.stats.DaysPlayed.ToString();
                pond.modData[QuestOriginalCountKey] = count.ToString();
            }
            return;
        }

        if (pond.neededItem.Value == null)
        {
            var (itemId, count) = PickRandomQuestItem();
            if (itemId == null) return;

            pond.neededItem.Value = ItemRegistry.Create(itemId, 1, 0, false);
            pond.neededItemCount.Value = count;
            pond.modData[QuestStartDayKey] = Game1.stats.DaysPlayed.ToString();
            pond.modData[QuestOriginalCountKey] = count.ToString();
        }
    }

    private static (string? itemId, int count) PickRandomQuestItem()
    {
        if (_questItemsCache == null)
        {
            _questItemsCache = new List<(string, int)>();
            var objects = DataLoader.Objects(Game1.content);
            foreach (var pair in objects)
            {
                int cat = pair.Value.Category;
                if (cat != -5 && cat != -6 && cat != -7 && cat != -75 && cat != -79)
                    continue;
                if (cat == -7)
                {
                    var tags = ItemContextTagManager.GetBaseContextTags(pair.Key);
                    if (tags.Contains("food_seafood")) continue;
                }
                _questItemsCache.Add((pair.Key, cat));
            }
        }
        if (_questItemsCache.Count == 0) return (null, 0);

        var chosen = _questItemsCache[Game1.random.Next(_questItemsCache.Count)];
        int count = chosen.category == -7 ? 1 : Game1.random.Next(1, 6);
        return (chosen.itemId, count);
    }

    private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Game1.IsMasterGame || !Config.EnableLegendaryPondExpansion) return;

        int maxCap = Math.Clamp(Config.LegendaryPondMaxCapacity, 1, 10);

        foreach (GameLocation loc in Game1.locations)
            foreach (Building b in loc.buildings)
                if (b is FishPond pond && IsLegendaryFish(pond))
                {
                    if (pond.lastUnlockedPopulationGate.Value > maxCap)
                    {
                        pond.lastUnlockedPopulationGate.Value = maxCap;
                        pond.UpdateMaximumOccupancy();
                    }
                }
    }

    private void UpdatePond(FishPond pond, GameLocation loc)
    {
        if (!pond.modData.ContainsKey(MoodKey)) pond.modData[MoodKey] = "0";
        if (!pond.modData.ContainsKey(FriendshipKey)) pond.modData[FriendshipKey] = "0";
        if (!pond.modData.ContainsKey(PopKey)) pond.modData[PopKey] = pond.currentOccupants.Value.ToString();
        if (!pond.modData.ContainsKey(LegendaryNextQuestDayKey)) pond.modData[LegendaryNextQuestDayKey] = "0";

        int mood = int.Parse(pond.modData[MoodKey]);
        int friendship = int.Parse(pond.modData[FriendshipKey]);
        int lastPop = int.Parse(pond.modData[PopKey]);
        int currentPop = pond.currentOccupants.Value;
        bool hasOutput = pond.output.Value != null;
        bool hadYesterday = pond.modData.ContainsKey(HadOutputKey) && pond.modData[HadOutputKey] == "true";

        bool isLegendary = Config.EnableLegendaryPondExpansion && IsLegendaryFish(pond);
        int maxCap = isLegendary ? Math.Clamp(Config.LegendaryPondMaxCapacity, 1, 10) : 10;
        bool isFull = currentPop >= pond.maxOccupants.Value;

        if (isLegendary && pond.lastUnlockedPopulationGate.Value == 0 && currentPop > 0)
        {
            pond.lastUnlockedPopulationGate.Value = 1;
            pond.UpdateMaximumOccupancy();
        }

        if (!pond.modData.ContainsKey(LastNeededKey))
            pond.modData[LastNeededKey] = pond.neededItem.Value != null ? "true" : "false";

        bool hadNeededYesterday = pond.modData[LastNeededKey] == "true";
        bool hasNeededToday = pond.neededItem.Value != null;

        if (hadNeededYesterday && !hasNeededToday)
        {
            if (isLegendary)
            {
                int currentGate = pond.lastUnlockedPopulationGate.Value;
                int nextGate = Math.Min(currentGate + 1, maxCap);

                if (nextGate > currentGate)
                {
                    pond.lastUnlockedPopulationGate.Value = nextGate;
                    pond.UpdateMaximumOccupancy();
                    if (pond.maxOccupants.Value < nextGate)
                        pond.maxOccupants.Value = nextGate;
                }

                if (!(pond.modData.ContainsKey(RewardedKey) && pond.modData[RewardedKey] == "true"))
                {
                    mood += isFull ? 4 : 5;
                    friendship += 4;
                    pond.modData[RewardedKey] = "true";
                }
            }
            else
            {
                if (!(pond.modData.ContainsKey(RewardedKey) && pond.modData[RewardedKey] == "true"))
                {
                    mood += isFull ? 7 : 10;
                    friendship += 7;
                    pond.modData[RewardedKey] = "true";
                }
            }
        }

        pond.modData[LastNeededKey] = hasNeededToday ? "true" : "false";

        if (loc.IsRainingHere()) mood += 5;
        if (loc.IsLightningHere()) mood -= 1;
        if (IsGreenRain(loc)) mood -= 3;
        if (currentPop > lastPop) mood += 3 * (currentPop - lastPop);
        if (isFull) mood += 2;
        mood += Game1.random.Next(-2, 3);
        if (hadYesterday && hasOutput) { mood -= 1; friendship -= 1; }

        if (hadYesterday && !hasOutput) friendship += 1;
        pond.modData[HadOutputKey] = hasOutput ? "true" : "false";

        if (currentPop == 0 && lastPop > 0)
        {
            mood = 0;
            friendship = 0;
            pond.modData[RewardedKey] = "false";
            pond.modData[InsuranceKey] = "0";
            pond.modData[HadOutputKey] = "false";
            pond.modData[LastNeededKey] = "false";
            pond.modData[LegendaryNextQuestDayKey] = "0";
        }

        if (friendship > 100)
        {
            int overflow = friendship - 100;
            friendship = 100;
            int insurance = 0;
            if (pond.modData.ContainsKey(InsuranceKey))
                int.TryParse(pond.modData[InsuranceKey], out insurance);
            pond.modData[InsuranceKey] = Math.Min(insurance + overflow, 10).ToString();
        }

        mood = Math.Clamp(mood, 0, 100);
        friendship = Math.Clamp(friendship, 0, 100);

        pond.modData[MoodKey] = mood.ToString();
        pond.modData[FriendshipKey] = friendship.ToString();
        pond.modData[PopKey] = currentPop.ToString();
    }

    private static bool IsLegendaryFish(FishPond pond)
    {
        if (string.IsNullOrEmpty(pond.fishType.Value)) return false;
        try
        {
            var fish = ItemRegistry.Create(pond.fishType.Value);
            return fish?.HasContextTag("fish_legendary") == true;
        }
        catch { return false; }
    }

    private static bool IsGreenRain(GameLocation loc)
    {
        try
        {
            var m = typeof(GameLocation).GetMethod("IsGreenRainingHere");
            return m != null && (bool)(m.Invoke(loc, null) ?? false);
        }
        catch { return false; }
    }

    private static int TryUpgradeWithInsurance(FishPond pond, int q)
    {
        if (q >= 4) return q;
        if (pond.modData.ContainsKey(InsuranceKey)
            && int.TryParse(pond.modData[InsuranceKey], out int ins) && ins >= 2)
        {
            int up = q switch { 0 => 1, 1 => 2, 2 => 4, _ => q };
            if (up != q)
            {
                pond.modData[InsuranceKey] = (ins - 2).ToString();
                return up;
            }
        }
        return q;
    }

    private static int CalculateQuality(FishPond pond, int mood)
    {
        int pop = pond.currentOccupants.Value;
        if (pop <= 0) return 0;

        double wNormal = Math.Max(15.0, 55.0 - pop * 1.5 - mood * 0.35);
        double wSilver = 28.0 + pop * 1.2 + mood * 0.25;
        double wGold = 14.0 + pop * 0.6 + mood * 0.25;
        double wIrid = 4.0 + pop * 0.15 + mood * 0.35;

        double total = wNormal + wSilver + wGold + wIrid;

        int off = (int)((Game1.stats.DaysPlayed * 7 + pond.tileX.Value * 13 + pond.tileY.Value * 17) % 2000) - 999;
        Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, pond.tileX.Value, pond.tileY.Value, off);
        double roll = r.NextDouble() * total;

        if (roll < wIrid) return 4;
        roll -= wIrid;
        if (roll < wGold) return 2;
        roll -= wGold;
        if (roll < wSilver) return 1;
        return 0;
    }
}