using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Minigames;

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

public static class LevelSkipState
{
    public static bool IsSkipping = false;
    public static float AnimationTimer = 0f;
    public static SparklingText? CompletionText = null;
    public static int DeathCount = 0;
    public static int LastLivesLeft = -1;
}

[HarmonyPatch(typeof(MineCart), "restartLevel")]
public static class MineCart_restartLevel_Patch
{
    static void Postfix(MineCart __instance, bool new_game)
    {
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (gameMode != 3) return;

        LevelSkipState.DeathCount = 0;
        LevelSkipState.LastLivesLeft = -1;
        LevelSkipState.IsSkipping = false;

        if (new_game)
        {
            tr.Field("livesLeft").SetValue(ModEntry.Config.InitialLives);
        }
    }
}

[HarmonyPatch(typeof(MineCart), "CollectCoin")]
public static class MineCart_CollectCoin_Patch
{
    static bool Prefix(MineCart __instance, int amount)
    {
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (gameMode != 3) return true;

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
        return false;
    }
}

[HarmonyPatch(typeof(MineCart), "tick")]
public static class MineCart_tick_Patch
{
    static bool Prefix(MineCart __instance, GameTime time)
    {
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (gameMode != 3) return true;

        if (LevelSkipState.IsSkipping)
        {
            LevelSkipState.CompletionText?.update(time);

            float deltaTime = (float)time.ElapsedGameTime.TotalSeconds;
            LevelSkipState.AnimationTimer -= deltaTime;

            if (LevelSkipState.AnimationTimer <= 0f)
            {
                LevelSkipState.IsSkipping = false;
                LevelSkipState.CompletionText = null;
                tr.Field("perfectText").SetValue(null);
                tr.Method("ShowMap").GetValue();
            }
            return false;
        }

        return true;
    }

    static void Postfix(MineCart __instance, GameTime time)
    {
        var tr = Traverse.Create(__instance);
        int gameMode = tr.Field("gameMode").GetValue<int>();

        if (gameMode != 3) return;

        if (LevelSkipState.IsSkipping)
            return;

        if (!IsIngame(tr))
        {
            LevelSkipState.LastLivesLeft = -1;
            return;
        }

        int currentLives = tr.Field("livesLeft").GetValue<int>();

        if (LevelSkipState.LastLivesLeft < 0)
        {
            LevelSkipState.LastLivesLeft = currentLives;
            return;
        }

        if (currentLives < LevelSkipState.LastLivesLeft)
        {
            LevelSkipState.DeathCount++;
            LevelSkipState.LastLivesLeft = currentLives;

            if (LevelSkipState.DeathCount >= 3)
            {
                DoLevelSkip(tr);
            }
        }
    }

    static void DoLevelSkip(Traverse tr)
    {
        tr.Field("livesLeft").SetValue(0);
        tr.Field("gameOver").SetValue(false);
        tr.Field("fadeDelta").SetValue(0f);
        tr.Field("gameState").SetValue(MineCart.GameStates.Ingame);

        LevelSkipState.IsSkipping = true;
        LevelSkipState.AnimationTimer = 2.5f;

        var completionText = new SparklingText(
            Game1.dialogueFont,
            "LEVEL COMPLETE!",
            Color.Gold, Color.White, true, 0.1, 2500, -1, 500, 0f);
        LevelSkipState.CompletionText = completionText;
        tr.Field("perfectText").SetValue(completionText);

        Game1.playSound("yoba", null);
    }

    static bool IsIngame(Traverse tr)
    {
        var gameState = tr.Field("gameState").GetValue<MineCart.GameStates>();
        return gameState == MineCart.GameStates.Ingame;
    }
}
