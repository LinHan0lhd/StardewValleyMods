using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace AutoServerPro.Core;

public static class SavePatch
{
    public static IMonitor? Monitor { get; set; }
    public static bool SkipSchedule { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NPC), nameof(NPC.checkSchedule))]
    public static bool CheckSchedulePrefix(NPC __instance)
    {
        if (SkipSchedule && __instance.IsVillager)
        {
            __instance.controller = null;
            __instance.temporaryController = null;
            return false;
        }
        return true;
    }
}
