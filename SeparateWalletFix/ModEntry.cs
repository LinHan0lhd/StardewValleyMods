using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace SeparateWalletFix
{
    public class ModEntry : Mod
    {
        public static IMonitor? StaticMonitor;

        public override void Entry(IModHelper helper)
        {
            StaticMonitor = Monitor;
            var harmony = new Harmony(ModManifest.UniqueID);

            // Core Defense: NetFieldBase.setInterpolationTarget
            var setInterp = AccessTools.Method(typeof(NetFieldBase<bool, NetBool>), "setInterpolationTarget", new[] { typeof(bool) });
            if (setInterp != null)
                harmony.Patch(setInterp, prefix: new HarmonyMethod(typeof(Patcher), nameof(Patcher.SetInterpolationTargetPrefix)));
        }
    }

    public static class Patcher
    {
        public static bool SetInterpolationTargetPrefix(NetFieldBase<bool, NetBool> __instance, bool newValue)
        {
            if (!Game1.IsMasterGame) return true;
            if (!ReferenceEquals(__instance, Game1.player?.team?.useSeparateWallets)) return true;
            if (__instance.Value && !newValue)
            {
                ModEntry.StaticMonitor?.Log(
                    "[SWF] Defense triggered: useSeparateWallets attempted to change from true to false. Sync blocked.",
                    LogLevel.Trace);
                return false;
            }
            return true;
        }
    }
}