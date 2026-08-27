using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;

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

            switch (__instance.QualifiedItemId)
            {
                case "(BC)163": return; // 木桶
                case "(BC)25":  return; // 种子制造机
            }

            var held = __instance.heldObject.Value;
            var input = __instance.lastInputItem.Value;
            if (input == null) return;

            if (input.Quality > 0)
                held.Quality = input.Quality;

            if (IsLargeQualityRule(__instance) && held.Stack == 1)
                held.Stack = 2;

            if (ModEntry.Config.DisableDoubleOutputOnLoom
                && __instance.QualifiedItemId == "(BC)17"
                && held.Stack > 1)
                held.Stack = 1;
        }

        private static bool IsLargeQualityRule(StardewValley.Object machine)
        {
            string ruleId = machine.lastOutputRuleId.Value;
            if (string.IsNullOrEmpty(ruleId)) return false;

            MachineData machineData = machine.GetMachineData();
            if (machineData?.OutputRules == null) return false;

            foreach (MachineOutputRule rule in machineData.OutputRules)
            {
                if (rule.Id == ruleId && rule.OutputItem != null)
                {
                    foreach (MachineItemOutput output in rule.OutputItem)
                    {
                        if (output.Quality == 2) return true;
                    }
                }
            }
            return false;
        }
    }
}