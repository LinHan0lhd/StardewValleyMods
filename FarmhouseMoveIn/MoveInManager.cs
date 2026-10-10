#nullable disable
using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

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

            // Patch 3: 让住主屋的 farmhand 被判定为房屋归属者，
            // 否则打开新手礼包盒/邮箱时显示"别人的"
            var ownedProp = AccessTools.Property(typeof(FarmHouse), "IsOwnedByCurrentPlayer");
            var ownedGetter = ownedProp?.GetGetMethod(nonPublic: true);
            if (ownedGetter != null && (Harmony.GetPatchInfo(ownedGetter)?.Postfixes?.Any(p => p.owner == "LinHan.FarmhouseMoveIn") ?? false) == false)
                _harmony.Patch(ownedGetter, postfix: new HarmonyMethod(typeof(MoveInManager), nameof(IsOwnedByCurrentPlayerPostfix)));
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
    /// Patch: 当前玩家住主屋时，把 IsOwnedByCurrentPlayer 判为 true。
    /// 解决联机玩家在主屋打开新手礼包盒/邮箱时显示"别人的"。
    /// </summary>
    private static void IsOwnedByCurrentPlayerPostfix(FarmHouse __instance, ref bool __result)
    {
        if (__result) return;
        try
        {
            // 实例必须就是当前玩家住的那间主屋
            if (!__instance.NameOrUniqueName.Equals("FarmHouse", StringComparison.OrdinalIgnoreCase))
                return;
            if (string.Equals(Game1.player?.homeLocation?.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                __result = true;
        }
        catch { }
    }

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

        // 把小屋里的新手礼包盒移到主屋
        MoveStarterGiftBox(cabin, farmhouse);

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

        // 交换冰箱内容：Cabin.fridge 与 FarmHouse.fridge 各自独立，
        // 不交换的话搬入者打开主屋冰箱是空的，自己的东西还留在小屋冰箱里
        var farmFridgeItems = farmhouse.fridge.Value.Items.ToList();
        var cabinFridgeItems = cabin.fridge.Value.Items.ToList();
        farmhouse.fridge.Value.Items.Clear();
        cabin.fridge.Value.Items.Clear();
        foreach (var item in cabinFridgeItems) farmhouse.fridge.Value.Items.Add(item);
        foreach (var item in farmFridgeItems) cabin.fridge.Value.Items.Add(item);
    }

    /// <summary>
    /// 把小屋里的新手礼包盒（15个防风草种子）移到主屋。
    /// 主屋已有未开的礼包盒则不动（主人还没开自己的）。
    /// </summary>
    private void MoveStarterGiftBox(Cabin cabin, FarmHouse farmhouse)
    {
        try
        {
            // 找 Cabin 里的新手礼包盒及其位置
            Vector2 cabinGiftTile = Vector2.Zero;
            Chest cabinGiftBox = null;
            foreach (var kvp in cabin.objects.Pairs)
            {
                if (kvp.Value is Chest c && c.giftbox.Value && c.giftboxIsStarterGift.Value)
                {
                    cabinGiftTile = kvp.Key;
                    cabinGiftBox = c;
                    break;
                }
            }
            if (cabinGiftBox == null) return;

            // 主屋已有新手礼包盒则保留主屋的
            foreach (var kvp in farmhouse.objects.Pairs)
            {
                if (kvp.Value is Chest c && c.giftbox.Value && c.giftboxIsStarterGift.Value)
                    return;
            }

            cabin.objects.Remove(cabinGiftTile);
            farmhouse.objects[cabinGiftTile] = cabinGiftBox;
            _monitor.Log($"[迁主屋] 已把新手礼包盒从小屋移到主屋 (位置 {cabinGiftTile})", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _monitor.Log($"[迁主屋] 移动新手礼包盒失败: {ex.Message}", LogLevel.Error);
        }
    }
}
