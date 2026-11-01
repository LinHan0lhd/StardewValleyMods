#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace AutoServerPro.Core;

/// <summary>
/// 动态生成联机小屋
/// 玩家连接时没有空位就自动建房
/// </summary>
public class CabinGenerator
{
    private readonly IMonitor _monitor;
    private Harmony _harmony;

    private static readonly string[] CabinSkins =
    {
        "Log Cabin", "Plank Cabin", "Stone Cabin",
        "Rustic Cabin", "Trailer Cabin", "Neighbor Cabin", "Beach Cabin"
    };

    // 判定空地时算占位的类别 不含玩家和动物 避免路过被误判
    private const CollisionMask BlockingMask =
        CollisionMask.Buildings | CollisionMask.Objects | CollisionMask.Furniture |
        CollisionMask.TerrainFeatures | CollisionMask.Flooring | CollisionMask.LocationSpecific;

    public CabinGenerator(IMonitor monitor)
    {
        _monitor = monitor;
        InstallPatches();
    }

    private void InstallPatches()
    {
        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.CabinGenerator");
            var method = AccessTools.Method(typeof(GameServer), "sendAvailableFarmhands");
            if (method == null)
            {
                _monitor.Log("无法找到 GameServer.sendAvailableFarmhands 方法", LogLevel.Warn);
                return;
            }
            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(CabinGenerator), nameof(SendAvailableFarmhandsPrefix)));
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装小屋生成补丁失败: {ex.Message}", LogLevel.Error);
        }
    }

    private static IMonitor GetMonitor() => ModEntry.Instance?.Monitor;

    private static void SendAvailableFarmhandsPrefix(GameServer __instance)
    {
        var monitor = GetMonitor();
        try
        {
            if (Game1.netWorldState?.Value == null) return;

            // 检查是否已有可用空位
            bool hasAvailableSlot = false;
            foreach (var farmhand in Game1.netWorldState.Value.farmhandData.Values)
            {
                if (farmhand == null) continue;
                if ((!farmhand.isActive() || Game1.Multiplayer.isDisconnecting(farmhand.UniqueMultiplayerID)) &&
                    __instance.IsFarmhandAvailable(farmhand))
                {
                    hasAvailableSlot = true;
                    break;
                }
            }

            if (!hasAvailableSlot)
            {
                monitor?.Log("尝试动态创建联机小屋...", LogLevel.Debug);
                Farmer newFarmhand = CreateCabinAndFarmhand();
                if (newFarmhand == null)
                {
                    monitor?.Log("农场无可用位置 > 无法创建联机小屋", LogLevel.Warn);
                }
            }
        }
        catch (Exception ex)
        {
            monitor?.Log($"联机小屋生成异常: {ex.Message}", LogLevel.Error);
        }
    }

    private static Farmer CreateCabinAndFarmhand()
    {
        Farm farm = Game1.getFarm();
        if (farm == null) return null;

        // 检查玩家上限
        int maxFarmhands = Game1.Multiplayer.playerLimit - 1;
        int currentFarmhands = Game1.netWorldState.Value.farmhandData.FieldDict.Count;
        if (currentFarmhands >= maxFarmhands)
        {
            return null;
        }

        // 取小屋尺寸和门位置 用来判断占位
        if (!Building.TryGetData("Cabin", out var cabinData) || cabinData == null)
        {
            GetMonitor()?.Log("[小屋生成] 读取小屋数据失败", LogLevel.Warn);
            return null;
        }

        // 必须完全空地 有地才建 但先建在地图外 等玩家上线再搬
        Vector2? position = FindUnusedCabinPosition(
            farm, cabinData.Size.X, cabinData.Size.Y, cabinData.HumanDoor.X, cabinData.HumanDoor.Y);
        if (position == null)
        {
            GetMonitor()?.Log("[小屋生成] 没有空余位置，无法创建联机小屋", LogLevel.Warn);
            return null;
        }

        // 建在 999 999 不占可见地块
        Vector2 hiddenPos = new Vector2(999, 999);
        Building cabinBuilding = new Building("Cabin", hiddenPos);
        cabinBuilding.magical.Value = true;
        cabinBuilding.daysOfConstructionLeft.Value = 0;

        int skinIndex = Game1.random.Next(CabinSkins.Length);
        cabinBuilding.skinId.Value = CabinSkins[skinIndex];

        cabinBuilding.load();

        if (!farm.buildStructure(cabinBuilding, hiddenPos, Game1.player, true))
        {
            GetMonitor()?.Log("建造小屋失败", LogLevel.Warn);
            return null;
        }

        Cabin cabin = cabinBuilding.GetIndoors() as Cabin;
        if (cabin == null)
        {
            GetMonitor()?.Log("小屋内部为空", LogLevel.Warn);
            return null;
        }

        cabin.CreateFarmhand();

        Farmer newFarmhand = cabin.owner;
        if (newFarmhand == null)
        {
            GetMonitor()?.Log("联机玩家创建失败", LogLevel.Warn);
            return null;
        }

        GetMonitor()?.Log($"联机小屋在 ({position.Value.X}, {position.Value.Y}) 生成，皮肤: {CabinSkins[skinIndex]}", LogLevel.Info);
        return newFarmhand;
    }

    /// <summary>
    /// 玩家上线时调用
    /// 先判断是否新玩家 是的话把 999 隐藏小屋搬到空地
    /// </summary>
    public static void OnPlayerOnline(Farmer farmer)
    {
        Farm farm = Game1.getFarm();
        if (farm == null || farmer == null || farmer.IsMainPlayer) return;

        Building hidden = FindHiddenCabinOwnedBy(farm, farmer);
        if (hidden == null) return; // 不是新玩家 没有预留给他的隐藏小屋

        RelocateCabin(farm, hidden);
    }

    /// <summary>查找停在 999 且 owner 是该玩家的隐藏小屋</summary>
    private static Building FindHiddenCabinOwnedBy(Farm farm, Farmer farmer)
    {
        foreach (var building in farm.buildings)
        {
            if (building == null) continue;
            if (building.buildingType.Value != "Cabin") continue;
            if (building.tileX.Value != 999 || building.tileY.Value != 999) continue;

            Cabin cabin = building.GetIndoors() as Cabin;
            if (cabin?.owner == null) continue;
            if (cabin.owner.UniqueMultiplayerID != farmer.UniqueMultiplayerID) continue;

            return building;
        }
        return null;
    }

    /// <summary>把隐藏小屋搬到空地 没空地则踢人并销毁</summary>
    private static void RelocateCabin(Farm farm, Building building)
    {
        Cabin cabin = building.GetIndoors() as Cabin;
        if (cabin?.owner == null) return;

        Vector2? land = FindUnusedCabinPosition(
            farm, building.tilesWide.Value, building.tilesHigh.Value, building.humanDoor.X, building.humanDoor.Y);
        if (land == null)
        {
            long ownerId = cabin.owner.UniqueMultiplayerID;
            GetMonitor()?.Log($"[小屋生成] 没有空余位置，踢出玩家 {cabin.owner.Name} [ID:{ownerId}] 并销毁小屋", LogLevel.Warn);
            try
            {
                Game1.Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode,
                    "农场没有空余位置，暂时无法入住", Multiplayer.AllPlayers);
            }
            catch { }
            try { Game1.server?.kick(ownerId); } catch { }
            farm.destroyStructure(building);
            return;
        }

        building.tileX.Value = (int)land.Value.X;
        building.tileY.Value = (int)land.Value.Y;

        // 更新室内出口 warp
        GameLocation interior = building.GetIndoors();
        if (interior != null)
        {
            foreach (Warp warp in interior.warps)
            {
                if (warp.TargetName == farm.NameOrUniqueName)
                {
                    warp.TargetX = building.humanDoor.X + building.tileX.Value;
                    warp.TargetY = building.humanDoor.Y + building.tileY.Value + 1;
                }
            }
        }

        GetMonitor()?.Log($"[小屋生成] 新玩家 {cabin.owner.Name} 上线，小屋搬至 ({land.Value.X}, {land.Value.Y})", LogLevel.Info);
    }

    /// <summary>
    /// 每秒调用 清理一直没人认领的隐藏小屋
    /// 只在没有任何连接时执行 避免误删正在选角色的
    /// </summary>
    public static void CleanupAbandonedCabins()
    {
        Farm farm = Game1.getFarm();
        if (farm == null) return;

        // 还有连接时不动 包括正在选角色的玩家
        if (Game1.server != null && Game1.server.connectionsCount > 0) return;

        var abandoned = new List<Building>();
        foreach (var building in farm.buildings)
        {
            if (building == null) continue;
            if (building.buildingType.Value != "Cabin") continue;
            if (building.tileX.Value != 999 || building.tileY.Value != 999) continue;

            Cabin cabin = building.GetIndoors() as Cabin;
            Farmer owner = cabin?.owner;
            if (owner != null && owner.isActive()) continue; // 正在使用
            if (owner != null && !string.IsNullOrEmpty(owner.userID.Value)) continue; // 已绑定玩家 留给本人

            abandoned.Add(building);
        }

        foreach (var building in abandoned)
        {
            Cabin cabin = building.GetIndoors() as Cabin;
            string ownerName = cabin?.owner?.Name ?? "无";
            farm.destroyStructure(building);
            GetMonitor()?.Log($"[小屋生成] 已清理未认领的隐藏小屋（owner: {ownerName}）", LogLevel.Info);
        }
    }

    /// <summary>
    /// 在农场找完全空地的联机小屋位置
    /// 预设位是 Paths 图层带 Order 属性的格
    /// 5x3 范围和门口格都必须没有任何物体
    /// 通过反射访问地图数据 避免 xTile 类型依赖
    /// </summary>
    private static Vector2? FindUnusedCabinPosition(Farm farm, int width, int height, int doorX, int doorY)
    {
        if (farm == null || width <= 0 || height <= 0) return null;

        // 通过反射获取 Paths 图层尺寸
        int mapWidth = 0, mapHeight = 0;
        try
        {
            var mapField = typeof(GameLocation).GetField("map", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (mapField != null)
            {
                object map = mapField.GetValue(farm);
                if (map != null)
                {
                    var layersProp = map.GetType().GetProperty("Layers");
                    object layers = layersProp?.GetValue(map);
                    if (layers is IEnumerable layerEnum)
                    {
                        foreach (object layer in layerEnum)
                        {
                            var idProp = layer.GetType().GetProperty("Id");
                            string id = idProp?.GetValue(layer) as string;
                            if (id == "Paths")
                            {
                                mapWidth = (int)layer.GetType().GetProperty("LayerWidth").GetValue(layer);
                                mapHeight = (int)layer.GetType().GetProperty("LayerHeight").GetValue(layer);
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GetMonitor()?.Log($"联机小屋生成 > 获取地图尺寸失败: {ex.Message}", LogLevel.Warn);
        }

        if (mapWidth == 0 || mapHeight == 0) return null;

        var candidates = new List<(Vector2 pos, int order)>();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                string order = farm.doesTileHavePropertyNoNull(x, y, "Order", "Paths");
                if (string.IsNullOrEmpty(order)) continue;
                if (!int.TryParse(order, out int orderNum)) continue;

                Vector2 pos = new Vector2(x, y);
                if (!IsFootprintClear(farm, pos, width, height, doorX, doorY)) continue;

                candidates.Add((pos, orderNum));
            }
        }

        if (candidates.Count > 0)
        {
            // 按 Order 从小到大使用
            candidates.Sort((a, b) => a.order.CompareTo(b.order));
            return candidates[0].pos;
        }

        // 预设位都被杂物占住 退回全农场找可建造的空地
        return ScanFarmForEmptySpace(farm, width, height, doorX, doorY, mapWidth, mapHeight);
    }

    /// <summary>全农场逐行扫描完全空地的可建造区域</summary>
    private static Vector2? ScanFarmForEmptySpace(Farm farm, int width, int height, int doorX, int doorY, int mapWidth, int mapHeight)
    {
        for (int y = 0; y <= mapHeight - height; y++)
        {
            for (int x = 0; x <= mapWidth - width; x++)
            {
                Vector2 anchor = new Vector2(x, y);

                // 先看锚点 快速排除大部分格子
                if (!IsTileCompletelyEmpty(farm, anchor, true)) continue;

                if (!IsFootprintClear(farm, anchor, width, height, doorX, doorY, true)) continue;
                if (CoversWarpOrDoor(farm, anchor, width, height)) continue;
                return anchor;
            }
        }
        return null;
    }

    /// <summary>建筑范围加门口格必须完全空地</summary>
    private static bool IsFootprintClear(Farm farm, Vector2 anchor, int width, int height, int doorX, int doorY, bool requireBuildable = false)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 tile = new Vector2(anchor.X + x, anchor.Y + y);
                if (!IsTileCompletelyEmpty(farm, tile, requireBuildable)) return false;
            }
        }

        // 门口格要能站人 只要求可通行且无遮挡
        if (doorX < 0 || doorY < 0) return true;

        Vector2 doorTile = new Vector2(anchor.X + doorX, anchor.Y + doorY + 1);
        if (!farm.isTileOnMap(doorTile)) return false;
        if (!farm.isTilePassable(doorTile)) return false;

        return !farm.IsTileOccupiedBy(doorTile, BlockingMask, CollisionMask.None, false);
    }

    /// <summary>
    /// 单格是否完全空地
    /// 排除杂草 石头 树枝 树木 草 耕种土 地板 家具 树桩 灌木 建筑 水面
    /// </summary>
    private static bool IsTileCompletelyEmpty(Farm farm, Vector2 tile, bool requireBuildable)
    {
        if (!farm.isTileOnMap(tile)) return false;

        // 地图层必须可通行 排除水面和碰撞格
        if (!farm.isTilePassable(tile)) return false;

        // 禁止建造的图块
        if (!farm.isTilePlaceable(tile, false)) return false;

        // 杂草 石头 树 草 耕种土 树桩 灌木 建筑 等都不能有
        if (farm.IsTileOccupiedBy(tile, BlockingMask, CollisionMask.None, false)) return false;

        // 全农场扫描时只挑可建造的农田格
        if (requireBuildable && !IsBuildableTile(farm, tile)) return false;

        return true;
    }

    /// <summary>图块是否可建造 对齐原版 isBuildable 判定</summary>
    private static bool IsBuildableTile(Farm farm, Vector2 tile)
    {
        Rectangle rect = farm.GetBuildableRectangle();
        if (rect != Rectangle.Empty && !rect.Contains((int)tile.X, (int)tile.Y)) return false;

        string buildable = farm.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "Buildable", "Back");
        if (string.Equals(buildable, "f", StringComparison.OrdinalIgnoreCase)) return false;
        if (string.Equals(buildable, "t", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(buildable, "true", StringComparison.OrdinalIgnoreCase)) return true;

        return farm.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back", false) != null;
    }

    /// <summary>不能压住地图出口和已有建筑门口</summary>
    private static bool CoversWarpOrDoor(Farm farm, Vector2 anchor, int width, int height)
    {
        foreach (Warp warp in farm.warps)
        {
            if (IsInsideFootprint(warp.X, warp.Y, anchor, width, height)) return true;
            if (IsInsideFootprint(warp.X, warp.Y + 1, anchor, width, height)) return true;
        }

        foreach (var building in farm.buildings)
        {
            if (building == null) continue;
            Point door = building.getPointForHumanDoor();
            if (IsInsideFootprint(door.X, door.Y, anchor, width, height)) return true;
            if (IsInsideFootprint(door.X, door.Y + 1, anchor, width, height)) return true;
        }
        return false;
    }

    private static bool IsInsideFootprint(int x, int y, Vector2 anchor, int width, int height)
    {
        return x >= anchor.X && x < anchor.X + width && y >= anchor.Y && y < anchor.Y + height;
    }
}
