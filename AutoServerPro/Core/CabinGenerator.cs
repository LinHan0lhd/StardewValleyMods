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
/// 动态生成联机小屋。
/// 当玩家通过 IP 连接且没有可用空位时，自动在农场的预设小屋位置创建新小屋。
/// 如果没有可用位置则玩家无法加入（列表为空，相当于踢掉）。
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
            _monitor.Log("小屋动态生成补丁已安装", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"安装小屋生成补丁失败: {ex.Message}", LogLevel.Error);
        }
    }

    private static IMonitor GetMonitor() => ModEntry.Instance?.Monitor;

    /// <summary>
    /// 在 sendAvailableFarmhands 执行前检查是否有可用空位。
    /// 如果没有，尝试动态创建小屋。创建后原方法会自然地将新农场助手加入列表。
    /// </summary>
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
                monitor?.Log("[小屋生成] 无可用空位，尝试动态创建小屋...", LogLevel.Debug);
                Farmer newFarmhand = CreateCabinAndFarmhand();
                if (newFarmhand != null)
                {
                    monitor?.Log($"[小屋生成] 已创建小屋，新农场助手 ID: {newFarmhand.UniqueMultiplayerID}", LogLevel.Info);
                }
                else
                {
                    monitor?.Log("[小屋生成] 农场无可用位置，无法创建小屋", LogLevel.Warn);
                }
            }
        }
        catch (Exception ex)
        {
            monitor?.Log($"[小屋生成] 补丁异常: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// 在农场上创建一个新小屋并注册农场助手。
    /// </summary>
    private static Farmer CreateCabinAndFarmhand()
    {
        Farm farm = Game1.getFarm();
        if (farm == null) return null;

        // 检查玩家上限
        int maxFarmhands = Game1.Multiplayer.playerLimit - 1;
        int currentFarmhands = Game1.netWorldState.Value.farmhandData.FieldDict.Count;
        if (currentFarmhands >= maxFarmhands)
        {
            GetMonitor()?.Log($"[小屋生成] 已达玩家上限 ({currentFarmhands}/{maxFarmhands})", LogLevel.Warn);
            return null;
        }

        // 查找可用位置
        Vector2? position = FindUnusedCabinPosition(farm);
        if (position == null)
        {
            GetMonitor()?.Log("[小屋生成] 未找到可用的小屋位置", LogLevel.Warn);
            return null;
        }

        // 创建小屋建筑（与 BuildStartingCabins 相同的流程）
        Building cabinBuilding = new Building("Cabin", position.Value);
        cabinBuilding.magical.Value = true;
        cabinBuilding.daysOfConstructionLeft.Value = 0;

        int skinIndex = Game1.random.Next(CabinSkins.Length);
        cabinBuilding.skinId.Value = CabinSkins[skinIndex];

        cabinBuilding.load();

        if (!farm.buildStructure(cabinBuilding, position.Value, Game1.player, true))
        {
            GetMonitor()?.Log("[小屋生成] 建造小屋失败", LogLevel.Warn);
            return null;
        }

        Cabin cabin = cabinBuilding.GetIndoors() as Cabin;
        if (cabin == null)
        {
            GetMonitor()?.Log("[小屋生成] 小屋内部为空", LogLevel.Warn);
            return null;
        }

        // 为小屋创建农场助手（会自动加入 farmhandData）
        cabin.CreateFarmhand();

        Farmer newFarmhand = cabin.owner;
        if (newFarmhand == null)
        {
            GetMonitor()?.Log("[小屋生成] 农场助手创建失败", LogLevel.Warn);
            return null;
        }

        GetMonitor()?.Log($"[小屋生成] 在 ({position.Value.X}, {position.Value.Y}) 创建了小屋，皮肤: {CabinSkins[skinIndex]}", LogLevel.Info);
        return newFarmhand;
    }

    /// <summary>
    /// 在农场地图上扫描未使用的预设小屋位置。
    /// 预设位置是 "Paths" 图层中带有 "Order" 属性的 tile。
    /// 通过反射访问地图数据，避免 xTile 类型依赖。
    /// </summary>
    private static Vector2? FindUnusedCabinPosition(Farm farm)
    {
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
            GetMonitor()?.Log($"[小屋生成] 获取地图尺寸失败: {ex.Message}", LogLevel.Warn);
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

                // 检查该位置是否已有建筑
                bool occupied = false;
                foreach (var b in farm.buildings)
                {
                    if (b != null && b.tileX.Value == (int)pos.X && b.tileY.Value == (int)pos.Y)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    candidates.Add((pos, orderNum));
                }
            }
        }

        if (candidates.Count == 0) return null;

        // 按 Order 从小到大使用
        candidates.Sort((a, b) => a.order.CompareTo(b.order));
        return candidates[0].pos;
    }
}
