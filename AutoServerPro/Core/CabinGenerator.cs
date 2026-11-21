#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

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

            // 连接介绍包出异常时记录现场 方便定位
            var introMethod = AccessTools.Method(typeof(GameServer), "sendServerIntroduction");
            if (introMethod != null)
            {
                _harmony.Patch(introMethod,
                    prefix: new HarmonyMethod(typeof(CabinGenerator), nameof(SendServerIntroductionPrefix)),
                    finalizer: new HarmonyMethod(typeof(CabinGenerator), nameof(SendServerIntroductionFinalizer)));
            }
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

    /// <summary>发送介绍包前记录一次现场</summary>
    private static void SendServerIntroductionPrefix(long peer)
    {
        LogNetStats("发送前", peer, null);
    }

    /// <summary>介绍包抛异常时记录现场并原样返回异常 不改变原版行为</summary>
    private static Exception SendServerIntroductionFinalizer(Exception __exception, long peer)
    {
        if (__exception != null) LogNetStats("发送失败", peer, __exception);
        return __exception;
    }

    private static void LogNetStats(string tag, long peer, Exception ex)
    {
        try
        {
            int farmhands = Game1.netWorldState?.Value?.farmhandData?.FieldDict.Count ?? -1;
            int online = Game1.otherFarmers?.Count ?? -1;
            long gcMb = GC.GetTotalMemory(false) / (1024 * 1024);
            long processMb = 0;
            using (Process p = Process.GetCurrentProcess())
            {
                processMb = p.WorkingSet64 / (1024 * 1024);
            }

            string extra = ex == null
                ? ""
                : $" 异常:{ex.GetType().Name} {ex.Message}";
            GetMonitor()?.Log(
                $"[连接包] {tag} 目标:{peer} 农场助手:{farmhands} 在线:{online} GC:{gcMb}MB 进程:{processMb}MB{extra}",
                ex == null ? LogLevel.Trace : LogLevel.Error);
        }
        catch { }
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

        // 先确认没有玩家物品的空地 再建在地图外 等玩家上线再搬
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

        // 小屋实际先建在 999 999 等玩家上线再搬 这里只是预留位置
        GetMonitor()?.Log($"联机小屋（{CabinSkins[skinIndex]}）已预留空地 ({position.Value.X}, {position.Value.Y})，等待玩家上线后搬迁", LogLevel.Info);
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

        // 清掉选址处的系统杂物 玩家物品不动
        ClearSystemDebrisInFootprint(farm, land.Value, building.tilesWide.Value, building.tilesHigh.Value, building.humanDoor.X, building.humanDoor.Y);

        // 把选址处的玩家踢出并改离线位置 动物 NPC 挪到墙边
        EvictCharactersInFootprint(farm, land.Value, building.tilesWide.Value, building.tilesHigh.Value, cabin.owner.UniqueMultiplayerID);

        // 用原版搬迁逻辑同步坐标和室内出口
        farm.buildStructure(building, land.Value, Game1.player, true);
        building.removeOverlappingBushes(farm);

        string ownerName = string.IsNullOrEmpty(cabin.owner.Name)
            ? $"ID:{cabin.owner.UniqueMultiplayerID}"
            : cabin.owner.Name;
        GetMonitor()?.Log($"[小屋生成] 新玩家 {ownerName} 上线，小屋搬至 ({land.Value.X}, {land.Value.Y})", LogLevel.Info);
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
    /// 在农场找没有玩家物品的联机小屋位置
    /// 预设位是 Paths 图层带 Order 属性的格
    /// 建筑范围和门口格允许系统杂物 搬迁时清除
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
                if (!IsFootprintFreeOfPlayerItems(farm, pos, width, height, doorX, doorY)) continue;

                candidates.Add((pos, orderNum));
            }
        }

        if (candidates.Count > 0)
        {
            // 按 Order 从小到大使用
            candidates.Sort((a, b) => a.order.CompareTo(b.order));
            return candidates[0].pos;
        }

        // 预设位不可用 退回全农场找可建造的空地
        return ScanFarmForEmptySpace(farm, width, height, doorX, doorY, mapWidth, mapHeight);
    }

    /// <summary>全农场逐行扫描没有玩家物品的可建造区域</summary>
    private static Vector2? ScanFarmForEmptySpace(Farm farm, int width, int height, int doorX, int doorY, int mapWidth, int mapHeight)
    {
        for (int y = 0; y <= mapHeight - height; y++)
        {
            for (int x = 0; x <= mapWidth - width; x++)
            {
                Vector2 anchor = new Vector2(x, y);

                // 先看锚点 快速排除大部分格子
                if (!IsTileFreeOfPlayerItems(farm, anchor, true)) continue;

                if (!IsFootprintFreeOfPlayerItems(farm, anchor, width, height, doorX, doorY, true)) continue;
                if (CoversWarpOrDoor(farm, anchor, width, height)) continue;
                return anchor;
            }
        }
        return null;
    }

    /// <summary>建筑范围加门口格都不能有玩家放置的物品 系统杂物可清</summary>
    private static bool IsFootprintFreeOfPlayerItems(Farm farm, Vector2 anchor, int width, int height, int doorX, int doorY, bool requireBuildable = false)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 tile = new Vector2(anchor.X + x, anchor.Y + y);
                if (!IsTileFreeOfPlayerItems(farm, tile, requireBuildable)) return false;
            }
        }

        // 门口格要能站人 只要求可通行且无玩家物品
        if (doorX < 0 || doorY < 0) return true;

        Vector2 doorTile = new Vector2(anchor.X + doorX, anchor.Y + doorY + 1);
        if (!farm.isTileOnMap(doorTile)) return false;
        if (!farm.isTilePassable(doorTile)) return false;
        if (farm.getBuildingAt(doorTile) != null) return false;

        return !HasPlayerItemAtTile(farm, doorTile);
    }

    /// <summary>
    /// 单格是否没有玩家物品
    /// 玩家物品包括 家具 耕地 地板 果树 巨型作物 制作物品
    /// 系统杂物 杂草 石头 树枝 草 树 灌木 树桩 不算 会在搬迁时清掉
    /// </summary>
    private static bool IsTileFreeOfPlayerItems(Farm farm, Vector2 tile, bool requireBuildable)
    {
        if (!farm.isTileOnMap(tile)) return false;

        // 地图层必须可通行 排除水面和碰撞格
        if (!farm.isTilePassable(tile)) return false;

        // 禁止建造的图块
        if (!farm.isTilePlaceable(tile, false)) return false;

        // 不能有已有建筑
        foreach (var b in farm.buildings)
        {
            if (b != null && b.occupiesTile(tile)) return false;
        }

        // 玩家放的物体
        if (farm.objects.TryGetValue(tile, out var obj) && IsPlayerPlacedObject(obj)) return false;

        // 玩家地形 耕地 地板 果树等
        if (farm.terrainFeatures.TryGetValue(tile, out var tf) && IsPlayerTerrainFeature(tf)) return false;

        Rectangle tileBounds = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

        // 玩家大型地形 按实际占地判断
        foreach (var ltf in farm.largeTerrainFeatures)
        {
            if (ltf != null && IsPlayerLargeTerrainFeature(ltf) && ltf.getBoundingBox().Intersects(tileBounds)) return false;
        }

        // 巨型作物也是玩家设施 不能当普通树桩清掉
        foreach (var rc in farm.resourceClumps)
        {
            if (rc is GiantCrop && rc.occupiesTile((int)tile.X, (int)tile.Y)) return false;
        }

        // 家具按实际占地判断
        foreach (var f in farm.furniture)
        {
            if (f != null && f.GetBoundingBox().Intersects(tileBounds)) return false;
        }

        // 全农场扫描时只挑可建造的农田格
        if (requireBuildable && !IsBuildableTile(farm, tile)) return false;

        return true;
    }

    /// <summary>门口格有没有玩家物品</summary>
    private static bool HasPlayerItemAtTile(Farm farm, Vector2 tile)
    {
        if (farm.objects.TryGetValue(tile, out var obj) && IsPlayerPlacedObject(obj)) return true;
        if (farm.terrainFeatures.TryGetValue(tile, out var tf) && IsPlayerTerrainFeature(tf)) return true;

        Rectangle tileBounds = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
        foreach (var ltf in farm.largeTerrainFeatures)
            if (ltf != null && IsPlayerLargeTerrainFeature(ltf) && ltf.getBoundingBox().Intersects(tileBounds)) return true;
        foreach (var rc in farm.resourceClumps)
            if (rc is GiantCrop && rc.occupiesTile((int)tile.X, (int)tile.Y)) return true;
        foreach (var f in farm.furniture)
            if (f != null && f.GetBoundingBox().Intersects(tileBounds)) return true;
        return false;
    }

    /// <summary>是不是玩家放置的物体 系统生成的杂草石头树枝觅食物不算</summary>
    private static bool IsPlayerPlacedObject(StardewValley.Object o)
    {
        if (o == null) return false;
        if (o is Furniture) return true;
        if (o.bigCraftable.Value) return true;
        if (o.IsWeeds() || o.IsBreakableStone() || o.IsTwig()) return false;
        if (o.IsSpawnedObject) return false; // 系统刷新的觅食物
        return true;
    }

    /// <summary>草和树可清 其他地形都保留 避免误删模组内容</summary>
    private static bool IsPlayerTerrainFeature(TerrainFeature tf)
    {
        return !(tf is Grass || tf is Tree);
    }

    /// <summary>灌木是系统的 其他大型地形保留</summary>
    private static bool IsPlayerLargeTerrainFeature(LargeTerrainFeature ltf)
    {
        return !(ltf is Bush);
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

    /// <summary>清除 5x3 范围和门口格里的系统杂物 玩家物品不动</summary>
    private static void ClearSystemDebrisInFootprint(Farm farm, Vector2 anchor, int width, int height, int doorX, int doorY)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ClearSystemDebrisAtTile(farm, new Vector2(anchor.X + x, anchor.Y + y));
            }
        }

        if (doorX >= 0 && doorY >= 0)
            ClearSystemDebrisAtTile(farm, new Vector2(anchor.X + doorX, anchor.Y + doorY + 1));
    }

    /// <summary>单格清除系统杂物</summary>
    private static void ClearSystemDebrisAtTile(Farm farm, Vector2 tile)
    {
        // 系统物体 杂草 石头 树枝 觅食物
        if (farm.objects.TryGetValue(tile, out var obj))
        {
            if (!IsPlayerPlacedObject(obj))
                farm.objects.Remove(tile);
        }

        // 系统地形 草 树
        if (farm.terrainFeatures.TryGetValue(tile, out var tf))
        {
            if (tf is Grass || tf is Tree)
                farm.terrainFeatures.Remove(tile);
        }

        Rectangle tileBounds = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

        // 灌木按实际占地清除
        for (int i = farm.largeTerrainFeatures.Count - 1; i >= 0; i--)
        {
            var ltf = farm.largeTerrainFeatures[i];
            if (ltf is Bush && ltf.getBoundingBox().Intersects(tileBounds))
                farm.largeTerrainFeatures.RemoveAt(i);
        }

        // 树桩 圆木 巨石可清 巨型作物保留
        for (int i = farm.resourceClumps.Count - 1; i >= 0; i--)
        {
            var rc = farm.resourceClumps[i];
            if (rc != null && !(rc is GiantCrop) && rc.occupiesTile((int)tile.X, (int)tile.Y))
                farm.resourceClumps.RemoveAt(i);
        }
    }

    /// <summary>
    /// 把目标范围里的角色处理掉
    /// 玩家 踢出并把离线位置设到小屋墙边 避免上线卡在屋里
    /// 动物 NPC 直接挪到墙边
    /// </summary>
    private static void EvictCharactersInFootprint(Farm farm, Vector2 anchor, int width, int height, long relocatingOwnerId)
    {
        Vector2 fallbackTile = FindAdjacentOpenTile(farm, anchor, width, height);
        Vector2 fallbackPos = fallbackTile * 64f;

        bool Inside(Vector2 tile) =>
            tile.X >= anchor.X && tile.X < anchor.X + width && tile.Y >= anchor.Y && tile.Y < anchor.Y + height;

        // NPC 挪到墙边
        foreach (var c in farm.characters)
        {
            if (c != null && Inside(c.Tile))
            {
                try { c.Position = fallbackPos; } catch { }
            }
        }

        // 农场动物单独存放 也要挪到墙边
        foreach (var animal in farm.animals.Values)
        {
            if (animal != null && Inside(animal.Tile))
            {
                try { animal.Position = fallbackPos; } catch { }
            }
        }

        // 联机玩家 踢出并改离线位置 避免下次上线卡在屋里
        if (Game1.otherFarmers != null)
        {
            // 先快照 部分联机通道踢人时会立即改在线集合
            foreach (var f in Game1.otherFarmers.Values.ToArray())
            {
                if (f == null || f.UniqueMultiplayerID == relocatingOwnerId) continue;

                // 只处理农场地面上的 屋内的不管 他们能自己走出来
                if (f.currentLocation != farm) continue;

                if (Inside(f.Tile))
                {
                    long id = f.UniqueMultiplayerID;
                    try { f.Position = fallbackPos; } catch { }
                    try { Game1.server?.kick(id); } catch { }
                    GetMonitor()?.Log($"[小屋生成] 玩家 {f.Name} 挡在选址处 已踢至 ({fallbackTile.X},{fallbackTile.Y})", LogLevel.Info);
                }
            }
        }
    }

    /// <summary>找小屋旁边一块可通行的空地 当墙边落脚点</summary>
    private static Vector2 FindAdjacentOpenTile(Farm farm, Vector2 anchor, int width, int height)
    {
        Vector2[] candidates =
        {
            new Vector2(anchor.X - 1, anchor.Y + 1),
            new Vector2(anchor.X + width, anchor.Y + 1),
            new Vector2(anchor.X + 1, anchor.Y - 1),
            new Vector2(anchor.X + 1, anchor.Y + height),
        };
        foreach (var c in candidates)
        {
            if (farm.CanSpawnCharacterHere(c))
                return c;
        }
        return new Vector2(anchor.X + width / 2, anchor.Y + height);
    }
}
