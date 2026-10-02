#nullable disable
using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace FarmhouseMoveIn;

/// <summary>
/// 迁主屋核心：
/// - 世界创建时首位农场助手默认住主屋
/// - 保护已住主屋的玩家不被重新分配回小屋
/// - 提供 movein 指令手动切换住客
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
            _harmony = new Harmony("LinHan.FarmhouseMoveIn");

            // Patch 1: 保留已住主屋玩家的分配
            var assignMethod = AccessTools.Method(typeof(NetWorldState), "TryAssignFarmhandHome");
            if (assignMethod != null)
            {
                var patchInfo = Harmony.GetPatchInfo(assignMethod);
                bool hasPrefix = patchInfo?.Prefixes?.Any(p => p.owner == "LinHan.FarmhouseMoveIn") ?? false;
                if (!hasPrefix)
                    _harmony.Patch(assignMethod, prefix: new HarmonyMethod(typeof(MoveInManager), nameof(TryAssignFarmhandHomePrefix)));
            }

            // Patch 2: 创建新存档时，第一个农场助手默认住主屋
            var createFarmhandMethod = AccessTools.Method(typeof(Cabin), "CreateFarmhand");
            if (createFarmhandMethod != null)
            {
                var patchInfo = Harmony.GetPatchInfo(createFarmhandMethod);
                bool hasPostfix = patchInfo?.Postfixes?.Any(p => p.owner == "LinHan.FarmhouseMoveIn") ?? false;
                if (!hasPostfix)
                    _harmony.Patch(createFarmhandMethod, postfix: new HarmonyMethod(typeof(MoveInManager), nameof(CreateFarmhandPostfix)));
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装迁主屋补丁失败: {ex.Message}", LogLevel.Error);
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

                GetMonitor()?.Log($"[迁主屋] 新存档首位农场助手 {newFarmhand.Name} (ID:{newFarmhand.UniqueMultiplayerID}) 默认入住主屋", LogLevel.Info);
            }
        }
        catch (Exception ex)
        {
            GetMonitor()?.Log($"[迁主屋] CreateFarmhandPostfix 异常: {ex.Message}", LogLevel.Error);
        }
    }

    private static IMonitor GetMonitor() => ModEntry.Instance?.Monitor;

    /// <summary>
    /// 让指定联机玩家搬进主屋。已有住客则先将其家具归位并迁回原小屋。
    /// 主屋与玩家小屋的家具互换，主屋等级取两者最大值。
    /// </summary>
    public bool MoveIn(Farmer who)
    {
        if (who == null)
        {
            _monitor.Log("[迁主屋] 失败: 玩家为空", LogLevel.Warn);
            return false;
        }
        if (who.IsMainPlayer)
        {
            _monitor.Log($"[迁主屋] {who.Name} 是主机，已住主屋", LogLevel.Info);
            return false;
        }

        FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
        if (farmhouse == null)
        {
            _monitor.Log("[迁主屋] 失败: 找不到主屋", LogLevel.Error);
            return false;
        }

        if (string.Equals(who.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
        {
            _monitor.Log($"[迁主屋] {who.Name} 已经住在主屋", LogLevel.Info);
            return false;
        }

        Cabin cabin = Game1.getLocationFromName(who.homeLocation.Value) as Cabin;
        if (cabin == null)
        {
            _monitor.Log($"[迁主屋] 失败: 找不到 {who.Name} 的小屋 ({who.homeLocation.Value})", LogLevel.Warn);
            return false;
        }

        // 已有住主屋的联机玩家：先把主屋与其小屋家具交换回来，再迁回原小屋
        Farmer existing = GetFarmhandLivingInFarmHouse(who);
        if (existing != null)
        {
            Cabin existingCabin = GetCabinOfFarmer(existing);
            if (existingCabin != null)
            {
                SwapFurniture(farmhouse, existingCabin);
                existing.homeLocation.Value = existingCabin.NameOrUniqueName;
                _monitor.Log($"[迁主屋] {existing.Name} 已迁回小屋 {existingCabin.NameOrUniqueName}，家具已归位", LogLevel.Info);
            }
            else
            {
                existing.homeLocation.Value = "FarmHouse";
            }
        }

        // 交换主屋与玩家 Cabin 的家具
        SwapFurniture(farmhouse, cabin);

        // 主屋等级取最大值（不降级）
        if (cabin.upgradeLevel > farmhouse.upgradeLevel)
        {
            farmhouse.upgradeLevel = cabin.upgradeLevel;
            _monitor.Log($"[迁主屋] 主屋等级提升至 {farmhouse.upgradeLevel}", LogLevel.Info);
        }

        // 同步玩家 houseUpgradeLevel，避免后续升级时降级主屋
        if (who.houseUpgradeLevel.Value < farmhouse.upgradeLevel)
        {
            who.houseUpgradeLevel.Value = farmhouse.upgradeLevel;
        }

        who.homeLocation.Value = "FarmHouse";
        return true;
    }

    private Farmer GetFarmhandLivingInFarmHouse(Farmer exclude)
    {
        foreach (Farmer f in Game1.getAllFarmers())
        {
            if (f == null || f == exclude || f.IsMainPlayer) continue;
            if (string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                return f;
        }
        return null;
    }

    private Cabin GetCabinOfFarmer(Farmer who)
    {
        Farm farm = Game1.getFarm();
        if (farm != null)
        {
            foreach (var b in farm.buildings)
            {
                if (b != null && b.isCabin)
                {
                    Cabin indoors = b.GetIndoors() as Cabin;
                    if (indoors != null && indoors.owner == who)
                        return indoors;
                }
            }
        }
        string locName = who.homeLocation.Value;
        if (!string.Equals(locName, "FarmHouse", StringComparison.OrdinalIgnoreCase))
            return Game1.getLocationFromName(locName) as Cabin;
        return null;
    }

    private void SwapFurniture(FarmHouse farmhouse, Cabin cabin)
    {
        var farmFurniture = farmhouse.furniture.ToList();
        var cabinFurniture = cabin.furniture.ToList();
        farmhouse.furniture.Clear();
        cabin.furniture.Clear();
        foreach (var f in cabinFurniture) farmhouse.furniture.Add(f);
        foreach (var f in farmFurniture) cabin.furniture.Add(f);
    }
}
