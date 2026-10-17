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
/// 联机玩家迁入主屋管理。
/// - 新存档首位 farmhand 默认住主屋
/// - 已住主屋的玩家不被重新分配回小屋
/// - movein 指令手动切换住客
/// - 主屋与小屋交换家具/冰箱/objects，交换房屋等级
/// </summary>
public class FarmhouseMoveInManager
{
    private readonly IMonitor _monitor;
    private Harmony _harmony;

    public FarmhouseMoveInManager(IMonitor monitor)
    {
        _monitor = monitor;
        InstallPatches();
    }

    private void InstallPatches()
    {
        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.FarmhouseMoveIn");

            // 保留已住主屋玩家的分配
            var assignMethod = AccessTools.Method(typeof(NetWorldState), "TryAssignFarmhandHome");
            if (assignMethod != null && !Harmony.GetPatchInfo(assignMethod)?.Prefixes?.Any(p => p.owner == "LinHan.AutoServerPro.FarmhouseMoveIn") == true)
                _harmony.Patch(assignMethod, prefix: new HarmonyMethod(typeof(FarmhouseMoveInManager), nameof(TryAssignFarmhandHomePrefix)));

            // 新存档首位 farmhand 默认住主屋
            var createFarmhandMethod = AccessTools.Method(typeof(Cabin), "CreateFarmhand");
            if (createFarmhandMethod != null && !Harmony.GetPatchInfo(createFarmhandMethod)?.Postfixes?.Any(p => p.owner == "LinHan.AutoServerPro.FarmhouseMoveIn") == true)
                _harmony.Patch(createFarmhandMethod, postfix: new HarmonyMethod(typeof(FarmhouseMoveInManager), nameof(CreateFarmhandPostfix)));

            // 住主屋的 farmhand 在主屋开礼包盒/邮箱时判为归属者
            var ownedGetter = AccessTools.Property(typeof(FarmHouse), "IsOwnedByCurrentPlayer")?.GetGetMethod(nonPublic: true);
            if (ownedGetter != null && !Harmony.GetPatchInfo(ownedGetter)?.Postfixes?.Any(p => p.owner == "LinHan.AutoServerPro.FarmhouseMoveIn") == true)
                _harmony.Patch(ownedGetter, postfix: new HarmonyMethod(typeof(FarmhouseMoveInManager), nameof(IsOwnedByCurrentPlayerPostfix)));
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装迁主屋补丁失败: {ex.Message}", LogLevel.Error);
        }
    }

    private static IMonitor GetMonitor() => ModEntry.Instance?.Monitor;

    /// <summary>已住主屋的 farmhand 跳过原版 Cabin 分配</summary>
    private static bool TryAssignFarmhandHomePrefix(Farmer farmhand, ref bool __result)
    {
        if (farmhand == null || farmhand.IsMainPlayer) return true;
        if (string.Equals(farmhand.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
        {
            __result = true;
            return false;
        }
        return true;
    }

    /// <summary>新存档首位 farmhand 默认住主屋</summary>
    private static void CreateFarmhandPostfix(Cabin __instance)
    {
        try
        {
            if (!Game1.IsMasterGame || Game1.netWorldState?.Value == null) return;

            Farmer newFarmhand = __instance.owner;
            if (newFarmhand == null || newFarmhand.IsMainPlayer) return;

            bool hasFarmhouseResident = Game1.getAllFarmers().Any(f =>
                f != null && !f.IsMainPlayer &&
                string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase));

            if (!hasFarmhouseResident)
            {
                newFarmhand.homeLocation.Value = "FarmHouse";
                FarmHouse farmhouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
                if (farmhouse != null)
                {
                    MoveContents(farmhouse, __instance);
                    SwapHouseUpgradeLevel(farmhouse, newFarmhand);
                }
                GetMonitor()?.Log($"[迁主屋] 新存档首位农场助手 {newFarmhand.Name} 默认入住主屋", LogLevel.Info);
            }
        }
        catch (Exception ex)
        {
            GetMonitor()?.Log($"[迁主屋] CreateFarmhandPostfix 异常: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>当前玩家住主屋时，IsOwnedByCurrentPlayer 判 true</summary>
    private static void IsOwnedByCurrentPlayerPostfix(FarmHouse __instance, ref bool __result)
    {
        if (__result) return;
        try
        {
            if (!__instance.NameOrUniqueName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(Game1.player?.homeLocation?.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                __result = true;
        }
        catch { }
    }

    /// <summary>让指定联机玩家搬进主屋</summary>
    public bool MoveIn(Farmer who)
    {
        if (who == null)
        {
            _monitor.Log("[迁主屋] 失败: 玩家为空", LogLevel.Warn);
            return false;
        }
        if (who.IsMainPlayer)
        {
            _monitor.Log($"[迁主屋] {who.Name} 是主机", LogLevel.Info);
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
            _monitor.Log($"[迁主屋] {who.Name} 已住主屋", LogLevel.Info);
            return false;
        }

        Cabin cabin = Game1.getLocationFromName(who.homeLocation.Value) as Cabin;
        if (cabin == null)
        {
            _monitor.Log($"[迁主屋] 失败: 找不到 {who.Name} 的小屋", LogLevel.Warn);
            return false;
        }

        // 已有住主屋的联机玩家：交换主屋与其小屋内容，existing 搬回原小屋
        Farmer existing = GetFarmhandLivingInFarmHouse(who);
        bool hadExisting = existing != null;
        if (existing != null)
        {
            Cabin existingCabin = GetCabinOfFarmer(existing);
            if (existingCabin != null)
            {
                SwapFurniture(farmhouse, existingCabin);
                SwapHouseUpgradeLevel(farmhouse, existing);
                existing.homeLocation.Value = existingCabin.NameOrUniqueName;
                _monitor.Log($"[迁主屋] {existing.Name} 已迁回小屋", LogLevel.Info);
            }
            else
            {
                existing.homeLocation.Value = "FarmHouse";
            }
        }

        // 新玩家搬入：已有住客则交换，否则清空主屋
        if (hadExisting)
            SwapFurniture(farmhouse, cabin);
        else
            MoveContents(farmhouse, cabin);

        SwapHouseUpgradeLevel(farmhouse, who);

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

    /// <summary>交换主屋与玩家的房屋等级</summary>
    private static void SwapHouseUpgradeLevel(FarmHouse farmhouse, Farmer farmer)
    {
        int farmLevel = farmhouse.upgradeLevel;
        int farmerLevel = farmer.HouseUpgradeLevel;
        if (farmLevel == farmerLevel) return;

        // setMapForUpgradeLevel 是原版公开接口：内部会同步写入 upgradeLevel(归属者)、
        // 私有 currentlyDisplayedUpgradeLevel 与 synchronizedDisplayedLevel，并刷新地图
        farmhouse.setMapForUpgradeLevel(farmerLevel);
        farmer.HouseUpgradeLevel = farmLevel;

        GetMonitor()?.Log($"[迁主屋] 等级交换：主屋 {farmLevel}->{farmerLevel}，{farmer.Name} {farmerLevel}->{farmLevel}", LogLevel.Debug);
    }

    /// <summary>把 from 的家具/冰箱/objects 搬到 to，清空 to，from 留空</summary>
    private static void MoveContents(FarmHouse to, FarmHouse from)
    {
        var fromFurniture = from.furniture.ToList();
        to.furniture.Clear();
        from.furniture.Clear();
        foreach (var f in fromFurniture) to.furniture.Add(f);

        var fromFridgeItems = from.fridge.Value.Items.ToList();
        to.fridge.Value.Items.Clear();
        from.fridge.Value.Items.Clear();
        foreach (var item in fromFridgeItems) to.fridge.Value.Items.Add(item);

        var fromObjects = from.objects.Pairs.ToList();
        to.objects.Clear();
        from.objects.Clear();
        foreach (var kvp in fromObjects) to.objects[kvp.Key] = kvp.Value;
    }

    /// <summary>交换两边的家具/冰箱/objects</summary>
    private static void SwapFurniture(FarmHouse a, FarmHouse b)
    {
        var aFurniture = a.furniture.ToList();
        var bFurniture = b.furniture.ToList();
        a.furniture.Clear();
        b.furniture.Clear();
        foreach (var f in bFurniture) a.furniture.Add(f);
        foreach (var f in aFurniture) b.furniture.Add(f);

        var aFridgeItems = a.fridge.Value.Items.ToList();
        var bFridgeItems = b.fridge.Value.Items.ToList();
        a.fridge.Value.Items.Clear();
        b.fridge.Value.Items.Clear();
        foreach (var item in bFridgeItems) a.fridge.Value.Items.Add(item);
        foreach (var item in aFridgeItems) b.fridge.Value.Items.Add(item);

        var aObjects = a.objects.Pairs.ToList();
        var bObjects = b.objects.Pairs.ToList();
        a.objects.Clear();
        b.objects.Clear();
        foreach (var kvp in bObjects) a.objects[kvp.Key] = kvp.Value;
        foreach (var kvp in aObjects) b.objects[kvp.Key] = kvp.Value;
    }
}
