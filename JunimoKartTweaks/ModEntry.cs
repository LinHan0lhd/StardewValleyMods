using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Minigames;
using StardewValley.Menus;

namespace JunimoKartTweaks;

public class ModEntry : Mod
{
    public static ModConfig Config = null!;

    public override void Entry(IModHelper helper)
    {
        Config = helper.ReadConfig<ModConfig>();
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.PatchAll();
    }
}

/// <summary>
/// 跟踪每关是否已经补充过生命
/// </summary>
public static class RefillTracker
{
    private static readonly ConditionalWeakTable<MineCart, object> _refilled = new ConditionalWeakTable<MineCart, object>();

    public static bool HasRefilled(MineCart cart)
    {
        return _refilled.TryGetValue(cart, out _);
    }

    public static void MarkRefilled(MineCart cart)
    {
        _refilled.AddOrUpdate(cart, new object());
    }

    public static void Clear(MineCart cart)
    {
        _refilled.Remove(cart);
    }
}

/// <summary>
/// 初始生命数 + 每关结束补充生命
/// </summary>
[HarmonyPatch(typeof(MineCart), "restartLevel")]
public static class MineCart_restartLevel_Patch
{
    static void Postfix(MineCart __instance, bool new_game)
    {
        RefillTracker.Clear(__instance);
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (new_game)
        {
            tr.Field("livesLeft").SetValue(ModEntry.Config.InitialLives);
        }
        else if (gameMode == 3)
        {
            int livesLeft = tr.Field("livesLeft").GetValue<int>();
            livesLeft += ModEntry.Config.LivesRefillPerLevel;
            tr.Field("livesLeft").SetValue(livesLeft);
        }
    }
}

/// <summary>
/// 金币换命比例可配置
/// </summary>
[HarmonyPatch(typeof(MineCart), "CollectCoin")]
public static class MineCart_CollectCoin_Patch
{
    static bool Prefix(MineCart __instance, int amount)
    {
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (gameMode == 3)
        {
            __instance.coinCount += amount;
            int threshold = ModEntry.Config.CoinsPerLife;
            if (threshold > 0 && __instance.coinCount >= threshold)
            {
                Game1.playSound("yoba", null);
                int added = __instance.coinCount / threshold;
                __instance.coinCount %= threshold;
                int livesLeft = tr.Field("livesLeft").GetValue<int>();
                livesLeft += added;
                tr.Field("livesLeft").SetValue(livesLeft);
            }
            return false; // 跳过原版无尽模式逻辑
        }
        return true;
    }
}

/// <summary>
/// 水果总结界面完全重写
/// </summary>
[HarmonyPatch(typeof(MineCart), "UpdateFruitsSummary")]
public static class MineCart_UpdateFruitsSummary_Patch
{
    static bool Prefix(MineCart __instance, float time)
    {
        var config = ModEntry.Config;
        var tr = Traverse.Create(__instance);

        int currentTheme = tr.Field("currentTheme").GetValue<int>();
        if (currentTheme == 7)
        {
            tr.Field("currentFruitCheckIndex").SetValue(-1);
            tr.Method("ShowCutscene").GetValue();
            return false;
        }

        bool gamePaused = tr.Property("gamePaused").GetValue<bool>();
        if (gamePaused)
            return false;

        float stateTimer = tr.Field("stateTimer").GetValue<float>();
        if (stateTimer >= 0f)
        {
            stateTimer -= time;
            if (stateTimer < 0f)
                stateTimer = 0f;
            tr.Field("stateTimer").SetValue(stateTimer);
        }

        if (stateTimer == 0f)
        {
            int gameMode = tr.Field("gameMode").GetValue<int>();
            int livesLeft = tr.Field("livesLeft").GetValue<int>();

            // ---- 生命补充 ----
            if (!RefillTracker.HasRefilled(__instance) && gameMode == 3 && config.LivesRefillPerLevel > 0)
            {
                livesLeft += config.LivesRefillPerLevel;
                tr.Field("livesLeft").SetValue(livesLeft);
                tr.Field("stateTimer").SetValue(0.25f);
                Game1.playSound("coin", null);
                RefillTracker.MarkRefilled(__instance);
                return false;
            }

            // ---- 完美提示 ----
            bool lastLevelWasPerfect = tr.Field("lastLevelWasPerfect").GetValue<bool>();
            var perfectText = tr.Field("perfectText").GetValue();
            if (lastLevelWasPerfect && perfectText == null && gameMode == 3)
            {
                var sparkling = new SparklingText(
                    Game1.dialogueFont,
                    Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"),
                    Color.Lime, Color.White, true, 0.1, 2500, -1, 500, 0f);
                tr.Field("perfectText").SetValue(sparkling);
                Game1.playSound("yoba", null);
            }

            // ---- 初始化水果检查 ----
            int currentFruitCheckIndex = tr.Field("currentFruitCheckIndex").GetValue<int>();
            if (currentFruitCheckIndex == -1)
            {
                tr.Field("fruitEatCount").SetValue(0);
                tr.Field("currentFruitCheckIndex").SetValue(0);
                tr.Field("stateTimer").SetValue(0.5f);
                return false;
            }

            // ---- 结束水果展示 ----
            if (currentFruitCheckIndex >= 3)
            {
                tr.Field("perfectText").SetValue(null);
                tr.Field("currentFruitCheckIndex").SetValue(-1);
                tr.Method("ShowMap").GetValue();
                return false;
            }

            // ---- 检查当前水果 ----
            var collectedFruit = tr.Field("_collectedFruit").GetValue<HashSet<MineCart.CollectableFruits>>();
            int fruitEatCount = tr.Field("fruitEatCount").GetValue<int>();

            if (collectedFruit != null && collectedFruit.Contains((MineCart.CollectableFruits)currentFruitCheckIndex))
            {
                collectedFruit.Remove((MineCart.CollectableFruits)currentFruitCheckIndex);
                Game1.playSound("newArtifact", new int?(currentFruitCheckIndex * 100));
                fruitEatCount++;

                if (fruitEatCount >= 3)
                {
                    Game1.playSound("yoba", null);
                    if (gameMode == 3)
                    {
                        livesLeft += config.FruitBonusLives;
                        tr.Field("livesLeft").SetValue(livesLeft);

                        int coinCount = tr.Field("coinCount").GetValue<int>();
                        coinCount += config.FruitBonusCoins;
                        tr.Field("coinCount").SetValue(coinCount);
                    }
                    else
                    {
                        int score = tr.Field("score").GetValue<int>();
                        score += 5000;
                        tr.Field("score").SetValue(score);
                        __instance.UpdateScoreState();
                    }
                }
            }
            else
            {
                Game1.playSound("sell", new int?(currentFruitCheckIndex * 100));
            }

            tr.Field("stateTimer").SetValue(0.5f);
            tr.Field("currentFruitCheckMagnitude").SetValue(3f);
            tr.Field("currentFruitCheckIndex").SetValue(currentFruitCheckIndex + 1);
        }

        return false;
    }
}