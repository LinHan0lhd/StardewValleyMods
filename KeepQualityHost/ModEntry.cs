using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace ArtisanGoodsKeepQuality
{
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

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.minutesElapsed))]
    public static class MinutesElapsedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(StardewValley.Object __instance)
        {
            if (!Game1.IsMasterGame) return;
            if (!__instance.readyForHarvest.Value) return;
            if (__instance.heldObject.Value == null) return;

            // 跳过不应该继承品质的机器
            switch (__instance.QualifiedItemId)
            {
                case "(BC)163": return; // 木桶
                case "(BC)25":  return; // 种子制造机
            }

            var held = __instance.heldObject.Value;
            var input = __instance.lastInputItem.Value;
            if (input == null) return;

            bool isLargeInput = held.Quality == 2;

            if (input.Quality > 0)
                held.Quality = input.Quality;

            if (isLargeInput && held.Stack == 1)
                held.Stack = 2;

            if (ModEntry.Config.DisableDoubleOutputOnLoom
                && __instance.QualifiedItemId == "(BC)17"
                && held.Stack > 1)
                held.Stack = 1;
        }
    }
}