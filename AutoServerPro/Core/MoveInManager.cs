#nullable disable
using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace AutoServerPro.Core;

/// <summary>
/// 世界创建/玩家加入时的住房分配补丁。
/// - 第一个创建的农场助手默认住主屋
/// - 保护已住主屋的玩家不被重新分配回小屋
/// 搬家指令（手动切换住客）由 MasterHand 模组提供。
/// </summary>
public class MoveInManager
{
    private readonly IMonitor _monitor;
    private Harmony _harmony;

    public MoveInManager(IMonitor monitor)
    {
        _monitor = monitor;
        InstallPatches();
    }

    private void InstallPatches()
    {
        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.MoveIn");

            // Patch 1: 保留已住主屋玩家的分配
            var assignMethod = AccessTools.Method(typeof(NetWorldState), "TryAssignFarmhandHome");
            if (assignMethod != null)
            {
                var patchInfo = Harmony.GetPatchInfo(assignMethod);
                bool hasPrefix = patchInfo?.Prefixes?.Any(p => p.owner == "LinHan.AutoServerPro.MoveIn") ?? false;
                if (!hasPrefix)
                    _harmony.Patch(assignMethod, prefix: new HarmonyMethod(typeof(MoveInManager), nameof(TryAssignFarmhandHomePrefix)));
            }

            // Patch 2: 创建新存档时，第一个农场助手默认住主屋
            var createFarmhandMethod = AccessTools.Method(typeof(Cabin), "CreateFarmhand");
            if (createFarmhandMethod != null)
            {
                var patchInfo = Harmony.GetPatchInfo(createFarmhandMethod);
                bool hasPostfix = patchInfo?.Postfixes?.Any(p => p.owner == "LinHan.AutoServerPro.MoveIn") ?? false;
                if (!hasPostfix)
                    _harmony.Patch(createFarmhandMethod, postfix: new HarmonyMethod(typeof(MoveInManager), nameof(CreateFarmhandPostfix)));
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装搬家补丁失败: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Patch: 当联机玩家的 homeLocation 是 FarmHouse 时，跳过原版的 Cabin 分配逻辑，
    /// 防止搬家后的玩家被重新分配回 Cabin。
    /// </summary>
    private static bool TryAssignFarmhandHomePrefix(Farmer farmhand, ref bool __result)
    {
        if (farmhand == null || farmhand.IsMainPlayer)
            return true;

        if (string.Equals(farmhand.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
        {
            __result = true;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Patch: 创建新存档时，第一个被创建的农场助手默认住主屋。
    /// 判断依据：当前没有任何联机玩家的 homeLocation 是 FarmHouse。
    /// </summary>
    private static void CreateFarmhandPostfix(Cabin __instance)
    {
        try
        {
            if (!Game1.IsMasterGame || Game1.netWorldState?.Value == null)
                return;

            Farmer newFarmhand = __instance.owner;
            if (newFarmhand == null || newFarmhand.IsMainPlayer)
                return;

            // 检查是否已有联机玩家住主屋
            bool hasFarmhouseResident = false;
            foreach (var f in Game1.getAllFarmers())
            {
                if (f == null || f.IsMainPlayer) continue;
                if (string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                {
                    hasFarmhouseResident = true;
                    break;
                }
            }

            if (!hasFarmhouseResident)
            {
                newFarmhand.homeLocation.Value = "FarmHouse";

                FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
                if (farmhouse != null)
                {
                    newFarmhand.houseUpgradeLevel.Value = Math.Max(
                        newFarmhand.houseUpgradeLevel.Value,
                        farmhouse.upgradeLevel);
                }

                GetMonitor()?.Log($"[搬家] 新存档首位农场助手 {newFarmhand.Name} (ID:{newFarmhand.UniqueMultiplayerID}) 默认入住主屋", LogLevel.Info);
            }
        }
        catch (Exception ex)
        {
            GetMonitor()?.Log($"[搬家] CreateFarmhandPostfix 异常: {ex.Message}", LogLevel.Error);
        }
    }

    private static IMonitor GetMonitor() => ModEntry.Instance?.Monitor;
}
