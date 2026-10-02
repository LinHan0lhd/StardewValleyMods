#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace AutoServerPro.Core;

/// <summary>
/// 管理联机玩家搬进主屋的功能。
/// 同一时间只允许一个联机玩家住主屋，新搬入者替换原住者。
/// 搬家时交换主屋与玩家原 Cabin 的家具，主屋等级取两者最大值。
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
                // 第一个农场助手住进主屋
                newFarmhand.homeLocation.Value = "FarmHouse";

                // 同步升级等级，避免后续升级时降级主屋
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

    /// <summary>
    /// 让指定联机玩家搬进主屋。
    /// 如果已有其他联机玩家住在主屋，先将其家具归位并迁回原小屋，再执行搬入。
    /// </summary>
    public bool MoveIn(Farmer who)
    {
        if (who == null)
        {
            _monitor.Log("搬家失败: 玩家为空", LogLevel.Warn);
            return false;
        }

        if (who.IsMainPlayer)
        {
            _monitor.Log($"玩家 {who.Name} 是主机，已住主屋", LogLevel.Info);
            return false;
        }

        FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
        if (farmhouse == null)
        {
            _monitor.Log("搬家失败: 找不到主屋", LogLevel.Error);
            return false;
        }

        // 已经住主屋则无需操作
        if (string.Equals(who.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
        {
            _monitor.Log($"玩家 {who.Name} 已经住在主屋", LogLevel.Info);
            return false;
        }

        Cabin cabin = Game1.getLocationFromName(who.homeLocation.Value) as Cabin;
        if (cabin == null)
        {
            _monitor.Log($"搬家失败: 找不到玩家 {who.Name} 的小屋 ({who.homeLocation.Value})", LogLevel.Warn);
            return false;
        }

        // 处理已有住主屋的联机玩家：先把主屋与其小屋的家具交换回来，让原住客的家具归位，
        // 同时主屋恢复为搬家前的家具状态；再把原住客迁回自己的小屋。
        Farmer existing = GetFarmhandLivingInFarmHouse(who);
        if (existing != null)
        {
            Cabin existingCabin = GetCabinOfFarmer(existing);
            if (existingCabin != null)
            {
                SwapFurniture(farmhouse, existingCabin);
                existing.homeLocation.Value = existingCabin.NameOrUniqueName;
                _monitor.Log($"玩家 {existing.Name} 已迁回小屋 {existingCabin.NameOrUniqueName}，家具已归位", LogLevel.Info);
            }
            else
            {
                existing.homeLocation.Value = "FarmHouse";
            }
        }

        // 交换主屋与玩家 Cabin 的家具（此时主屋内是上一位住客搬入前的家具）
        SwapFurniture(farmhouse, cabin);

        // 主屋等级取两者最大值（不降级）
        if (cabin.upgradeLevel > farmhouse.upgradeLevel)
        {
            farmhouse.upgradeLevel = cabin.upgradeLevel;
            _monitor.Log($"主屋等级已提升至 {farmhouse.upgradeLevel}", LogLevel.Info);
        }

        // 同步玩家的 houseUpgradeLevel 与主屋等级，避免玩家后续升级主屋时把主屋降级。
        // 原版升级逻辑：setMapForUpgradeLevel(player.houseUpgradeLevel)，
        // 若玩家等级低于主屋当前等级会导致主屋被降级。
        if (who.houseUpgradeLevel.Value < farmhouse.upgradeLevel)
        {
            who.houseUpgradeLevel.Value = farmhouse.upgradeLevel;
            _monitor.Log($"玩家 {who.Name} 的房屋升级等级已同步至 {who.houseUpgradeLevel.Value}", LogLevel.Info);
        }

        // 切换玩家的家到主屋
        who.homeLocation.Value = "FarmHouse";

        _monitor.Log($"玩家 {who.Name} 已搬进主屋（原小屋: {cabin.NameOrUniqueName}）", LogLevel.Info);
        return true;
    }

    /// <summary>
    /// 获取当前住在主屋的联机玩家（排除指定玩家）。
    /// </summary>
    private Farmer GetFarmhandLivingInFarmHouse(Farmer exclude)
    {
        foreach (Farmer f in Game1.getAllFarmers())
        {
            if (f == null || f == exclude) continue;
            if (f.IsMainPlayer) continue;
            if (string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                return f;
        }
        return null;
    }

    /// <summary>
    /// 获取指定玩家的 Cabin。
    /// </summary>
    private Cabin GetCabinOfFarmer(Farmer who)
    {
        // 优先从建筑物中查找属于该玩家的 Cabin
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

        // 回退：按 homeLocation 查找
        string locName = who.homeLocation.Value;
        if (!string.Equals(locName, "FarmHouse", StringComparison.OrdinalIgnoreCase))
        {
            return Game1.getLocationFromName(locName) as Cabin;
        }

        return null;
    }

    /// <summary>
    /// 交换两个位置的家具列表。
    /// </summary>
    private void SwapFurniture(FarmHouse farmhouse, Cabin cabin)
    {
        try
        {
            // 保存双方家具快照
            var farmFurniture = farmhouse.furniture.ToList();
            var cabinFurniture = cabin.furniture.ToList();

            farmhouse.furniture.Clear();
            cabin.furniture.Clear();

            // Cabin 家具搬到主屋
            foreach (var f in cabinFurniture)
            {
                farmhouse.furniture.Add(f);
            }

            // 主屋家具搬到 Cabin
            foreach (var f in farmFurniture)
            {
                cabin.furniture.Add(f);
            }

            _monitor.Log($"已交换主屋与小屋家具（主屋 {farmhouse.furniture.Count} 件，小屋 {cabin.furniture.Count} 件）", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _monitor.Log($"交换家具失败: {ex.Message}", LogLevel.Error);
        }
    }
}
