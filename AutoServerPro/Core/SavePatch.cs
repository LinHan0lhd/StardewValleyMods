using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Linq;

namespace AutoServerPro.Core
{
    /// <summary>
    /// Harmony 补丁，用于在保存时更新所有 NPC 的 DefaultMap 和 DefaultPosition，
    /// 确保加载后 NPC 出现在保存时的精确位置。
    /// </summary>
    public static class SavePatch
    {
        public static IMonitor? Monitor { get; set; }

        /// <summary>
        /// 加载后前 3 帧阻止 checkSchedule，防止日程系统用预计算的过时路径覆盖刚还原的 NPC 位置。
        /// 这是必需的，因为原生加载流程中：initializeCharacter 设置 DefaultPosition → 
        /// 但 NPC.update 中的 checkSchedule 立即用 TryLoadSchedule 时预计算的路径起点（旧位置）覆盖它。
        /// </summary>
        public static bool SkipSchedule { get; set; } = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.getSaveEnumerator))]
        public static void Postfix()
        {
            Monitor?.Log("SavePatch.Postfix is running!", LogLevel.Info);

            foreach (var npc in Utility.getAllCharacters())
            {
                if (npc.currentLocation == null)
                    continue;

                Monitor?.Log($"Save NPC: {npc.Name}, Map: {npc.currentLocation.NameOrUniqueName}, Pos: {npc.Position}, DefaultPos: {npc.DefaultPosition}");
                npc.DefaultMap = npc.currentLocation.NameOrUniqueName;
                npc.DefaultPosition = npc.Position;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkSchedule))]
        public static bool CheckSchedulePrefix(NPC __instance)
        {
            if (SkipSchedule && __instance.IsVillager)
            {
                // 清除控制器，确保 NPC 保持静止
                __instance.controller = null;
                __instance.temporaryController = null;
                return false; // 跳过原方法
            }
            return true;
        }
    }
}
