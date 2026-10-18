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
/// - 首位上线联机玩家迁入主屋，并成为主屋“真正的归属者”
/// - 已住主屋的玩家不被重新分配回小屋
/// - movein 指令手动切换住客
/// - 主屋与小屋交换家具/冰箱/objects，主屋等级取二者较高值
/// - 迁出的小屋补一个未认领农场助手，作为加入界面的可选空位
/// </summary>
public class FarmhouseMoveInManager
{
    private const string HarmonyOwner = "LinHan.AutoServerPro.FarmhouseMoveIn";

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
            _harmony = new Harmony(HarmonyOwner);

            // 保留已住主屋玩家的分配
            var assignMethod = AccessTools.Method(typeof(NetWorldState), "TryAssignFarmhandHome");
            if (assignMethod != null && !Harmony.GetPatchInfo(assignMethod)?.Prefixes?.Any(p => p.owner == HarmonyOwner) == true)
                _harmony.Patch(assignMethod, prefix: new HarmonyMethod(typeof(FarmhouseMoveInManager), nameof(TryAssignFarmhandHomePrefix)));

            // 核心：让住主屋的联机玩家成为主屋真正的 owner。
            // 原版 FarmHouse.owner 永远返回 Game1.MasterPlayer(主机)，所以住主屋的联机玩家
            // 在礼包盒 / 邮箱 / 房屋等级 / OwnerId 等处都不被认作归属者。改写 owner 后一次性解决。
            var ownerGetter = AccessTools.Property(typeof(FarmHouse), "owner")?.GetGetMethod();
            if (ownerGetter != null && !Harmony.GetPatchInfo(ownerGetter)?.Postfixes?.Any(p => p.owner == HarmonyOwner) == true)
                _harmony.Patch(ownerGetter, postfix: new HarmonyMethod(typeof(FarmhouseMoveInManager), nameof(FarmHouseOwnerPostfix)));
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

    /// <summary>
    /// 主屋 owner 改写：住主屋的联机玩家(在线)存在时，owner 返回该玩家，
    /// 否则维持原版(Game1.MasterPlayer)。
    /// 只处理主屋("FarmHouse")，Cabin 的 owner 由 Cabin 重写，不受影响。
    /// </summary>
    private static void FarmHouseOwnerPostfix(FarmHouse __instance, ref Farmer __result)
    {
        try
        {
            if (__instance == null) return;
            if (!__instance.NameOrUniqueName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase)) return;

            Farmer resident = GetFarmHouseResident();
            if (resident != null) __result = resident;
        }
        catch { }
    }

    /// <summary>取住在主屋的联机玩家：优先本地玩家(客户端侧)，其次任一在线的非主机玩家</summary>
    private static Farmer GetFarmHouseResident()
    {
        Farmer local = Game1.player;
        if (local != null && !local.IsMainPlayer &&
            string.Equals(local.homeLocation?.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
            return local;

        var all = Game1.getAllFarmers();
        if (all != null)
        {
            foreach (Farmer f in all)
            {
                if (f == null || f.IsMainPlayer) continue;
                if (!f.isActive()) continue;
                if (string.Equals(f.homeLocation?.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                    return f;
            }
        }
        return null;
    }

    /// <summary>有联机玩家在线且主屋无人时，把首位在线联机玩家迁入主屋</summary>
    public void TryMoveFirstOnlinePlayerIntoFarmhouse()
    {
        try
        {
            if (!Game1.IsMasterGame) return;

            bool hasResident = Game1.getAllFarmers().Any(f =>
                f != null && !f.IsMainPlayer &&
                string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase));
            if (hasResident) return;

            Farmer first = Game1.otherFarmers?.Values?
                .FirstOrDefault(f => f != null && !f.IsMainPlayer && f.isActive());
            if (first == null) return;

            if (MoveIn(first))
                _monitor.Log($"[迁主屋] 首位联机玩家 {first.Name} 已迁入主屋", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"[迁主屋] 上线迁入异常: {ex.Message}", LogLevel.Error);
        }
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

        // 已有住主屋的联机玩家：让 existing 接手本次腾出的小屋，who 直接搬入主屋。
        Farmer existing = GetFarmhandLivingInFarmHouse(who);
        if (existing != null && existing != cabin.owner)
        {
            SwapFurniture(farmhouse, cabin);
            SetCabinOwner(cabin, existing);   // 腾出的小屋改归 existing，不再补助手
            SyncHouseLevel(farmhouse, who);
            who.homeLocation.Value = "FarmHouse";
            who.isCustomized.Value = true;
            _monitor.Log($"[迁主屋] {existing.Name} 已迁回小屋 {cabin.NameOrUniqueName}，{who.Name} 迁入主屋", LogLevel.Info);
            return true;
        }

        // 常规：主屋与玩家小屋交换家具/冰箱/objects，主屋取二者较高的房屋等级
        SyncHouseLevel(farmhouse, who);
        SwapFurniture(farmhouse, cabin);

        who.homeLocation.Value = "FarmHouse";
        // 主屋住客不再被任何小屋 farmhandReference 引用，存档加载时会被当作
        // “孤儿农场助手”删除，标记为已定制以保留
        who.isCustomized.Value = true;

        // 腾出的小屋补一个未认领农场助手：加入界面靠 farmhandData 里的场助手
        // 才显示可选空位，无主人的小屋不会出现在列表里
        try
        {
            cabin.farmhandReference.Value = null;
            cabin.CreateFarmhand();
            _monitor.Log($"[迁主屋] 原小屋 {cabin.NameOrUniqueName} 已补一名未认领农场助手，可作为新玩家空位", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"[迁主屋] 原小屋补助手失败: {ex.Message}", LogLevel.Error);
        }

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

    /// <summary>把小屋归属改给指定玩家（不复用 AssignFarmhand，避免误删该小屋原有农场助手数据）</summary>
    private static void SetCabinOwner(Cabin cabin, Farmer farmer)
    {
        cabin.farmhandReference.Value = farmer;
        farmer.homeLocation.Value = cabin.NameOrUniqueName;
        farmer.isCustomized.Value = true;
    }

    /// <summary>
    /// 主屋等级取主屋当前等级与玩家等级的较高值（不降级），并同步到玩家，
    /// 使 owner 改写后 farmhouse.upgradeLevel(= owner.HouseUpgradeLevel) 与地图一致。
    /// </summary>
    private static void SyncHouseLevel(FarmHouse farmhouse, Farmer farmer)
    {
        int houseLevel = farmhouse.upgradeLevel;
        int target = Math.Max(houseLevel, farmer.HouseUpgradeLevel);
        if (target > houseLevel)
        {
            farmer.HouseUpgradeLevel = target;
            // owner 此时仍是主机，setMapForUpgradeLevel 会写入主机等级并刷新主屋地图
            farmhouse.setMapForUpgradeLevel(target);
            GetMonitor()?.Log($"[迁主屋] 主屋等级提升 {houseLevel}->{target}", LogLevel.Debug);
        }
        else
        {
            farmer.HouseUpgradeLevel = houseLevel;
        }
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
