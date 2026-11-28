#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace AutoServerPro.Core;

/// <summary>
/// 主机单端小屋同步
/// 公共增量不含小屋 全量按连接只带本人小屋
/// </summary>
public class CabinSyncManager
{
    private readonly IMonitor _monitor;
    private Harmony _harmony;
    private static int _buildingsFieldIndex = -1;

    [ThreadStatic] private static long? _fullSyncPeer;
    [ThreadStatic] private static bool _filterDelta;
    [ThreadStatic] private static long _teamMessageSource;
    [ThreadStatic] private static long _demolishedOwner;
    private static bool _snapshotQueued;
    private static readonly HashSet<long> _pendingFarmSnapshots = new();

    public CabinSyncManager(IModHelper helper, IMonitor monitor)
    {
        _monitor = monitor;
        InstallPatches();
        helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    /// <summary>读档时合并一次老存档小屋位置 之后不再轮询</summary>
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (!Game1.IsMasterGame) return;
        try
        {
            CabinGenerator.EnsureSharedCabinPosition(Game1.getFarm(), out _);
        }
        catch (Exception ex)
        {
            _monitor.Log($"[小屋同步] 读档合并小屋失败: {ex.Message}", LogLevel.Error);
        }
    }

    internal static bool FilterActive => _fullSyncPeer.HasValue || _filterDelta;

    internal static bool IncludeInSync(Building building)
    {
        if (building == null || building.buildingType.Value != "Cabin") return true;
        if (_filterDelta) return false;
        return _fullSyncPeer.HasValue && GetOwnerId(building) == _fullSyncPeer.Value;
    }

    private void InstallPatches()
    {
        try
        {
            _harmony = new Harmony("LinHan.AutoServerPro.CabinSync");

            Patch(
                AccessTools.Method(typeof(GameServer), "sendLocation"),
                nameof(SendLocationPrefix),
                nameof(SendLocationPostfix));
            Patch(
                AccessTools.Method(typeof(Multiplayer), "broadcastLocationDelta"),
                nameof(BroadcastLocationDeltaPrefix),
                nameof(BroadcastLocationDeltaPostfix));
            Patch(
                AccessTools.Method(typeof(Multiplayer), "broadcastLocationMessage"),
                nameof(BroadcastLocationMessagePrefix));
            Patch(
                AccessTools.Method(typeof(GameServer), "processIncomingMessage"),
                nameof(ProcessServerMessagePrefix));
            Patch(
                AccessTools.Method(typeof(Multiplayer), "processIncomingMessage"),
                nameof(ProcessTeamMessagePrefix),
                nameof(ProcessTeamMessagePostfix));
            Patch(
                AccessTools.Method(typeof(FarmerTeam), "OnBuildingDemolishedEvent"),
                nameof(BuildingDemolishedPrefix),
                nameof(BuildingDemolishedFinalizer));

            var collection = typeof(NetCollection<Building>);
            _harmony.Patch(
                AccessTools.Method(collection, "WriteFull"),
                prefix: new HarmonyMethod(typeof(CabinCollectionPatches), nameof(CabinCollectionPatches.WriteFullPrefix)));
            _harmony.Patch(
                AccessTools.Method(collection, "Write"),
                prefix: new HarmonyMethod(typeof(CabinCollectionPatches), nameof(CabinCollectionPatches.WritePrefix)));
        }
        catch (Exception ex)
        {
            _monitor.Log($"[小屋同步] 安装补丁失败: {ex}", LogLevel.Error);
        }
    }

    private void Patch(System.Reflection.MethodInfo method, string prefix, string postfix = null)
    {
        if (method == null) throw new MissingMethodException(prefix);
        _harmony.Patch(
            method,
            prefix: prefix == null ? null : new HarmonyMethod(typeof(CabinSyncManager), prefix),
            finalizer: postfix == null ? null : new HarmonyMethod(typeof(CabinSyncManager), postfix));
    }

    private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Game1.IsMasterGame || !Context.IsWorldReady) return;
        try
        {
            _pendingFarmSnapshots.RemoveWhere(peer => !Game1.otherFarmers.ContainsKey(peer));
            foreach (long peer in _pendingFarmSnapshots.ToList())
                SendFarmSnapshot(peer);
        }
        catch (Exception ex)
        {
            _monitor.Log($"[小屋同步] 公共槽位处理失败: {ex.Message}", LogLevel.Error);
        }
    }

    private static bool SendLocationPrefix(long peer, GameLocation location, ref FilterState __state)
    {
        if (!Game1.IsMasterGame || location == null) return true;

        if (location is Cabin cabin && cabin.OwnerId != peer)
        {
            Building own = FindOwnedCabin(peer);
            if (own?.GetIndoors() != null)
                SendNativeLocation(peer, own.GetIndoors(), false);
            return false;
        }

        __state = EnterFullSync(peer);
        return true;
    }

    private static void SendLocationPostfix(FilterState __state)
    {
        ExitFilter(__state);
    }

    private static bool BroadcastLocationDeltaPrefix(GameLocation loc, ref FilterState __state)
    {
        if (!Game1.IsMasterGame || loc == null) return true;
        if (loc is Farm farm)
        {
            foreach (Building building in farm.buildings)
            {
                if (building?.buildingType.Value != "Cabin" || !building.NetFields.Dirty) continue;
                long ownerId = GetOwnerId(building);
                if (Game1.otherFarmers.ContainsKey(ownerId))
                    _pendingFarmSnapshots.Add(ownerId);
            }
        }
        __state = EnterDeltaFilter();
        return true;
    }

    private static void BroadcastLocationDeltaPostfix(FilterState __state)
    {
        ExitFilter(__state);
    }

    private static bool BroadcastLocationMessagePrefix(GameLocation loc, OutgoingMessage message)
    {
        if (!Game1.IsMasterGame || loc is not Cabin cabin) return true;

        long ownerId = cabin.OwnerId;
        if (ownerId != 0 && Game1.otherFarmers.ContainsKey(ownerId))
            Game1.server?.sendMessage(ownerId, message);
        return false;
    }

    private static bool ProcessServerMessagePrefix(IncomingMessage message)
    {
        if (!Game1.IsMasterGame || message.MessageType != 6) return true;

        long position = message.Reader.BaseStream.Position;
        try
        {
            bool isStructure = message.Reader.ReadBoolean();
            string name = message.Reader.ReadString();

            if (isStructure)
            {
                Cabin cabin = Game1.getLocationFromName(name, true) as Cabin;
                if (cabin == null) return true;
                if (cabin.OwnerId == message.FarmerID)
                {
                    message.Reader.BaseStream.Position = position;
                    Game1.multiplayer.processIncomingMessage(message);
                }
                return false;
            }

            Farm farm = Game1.getFarm();
            if (farm == null || name != farm.NameOrUniqueName || !DeltaTouchesBuildings(message.Reader, farm))
                return true;

            message.Reader.BaseStream.Position = position;
            HandleFarmMessage(message, farm);
            return false;
        }
        catch (Exception ex)
        {
            message.Reader.BaseStream.Position = position;
            ModEntry.Instance?.Monitor.Log($"[小屋同步] 客户端地点包处理失败: {ex.Message}", LogLevel.Error);
            return false;
        }
        finally
        {
            if (message.Reader != null)
                message.Reader.BaseStream.Position = position;
        }
    }

    private static void HandleFarmMessage(IncomingMessage message, Farm farm)
    {
        var before = CaptureCabins(farm);
        Game1.multiplayer.processIncomingMessage(message);

        Vector2? requestedPosition = null;
        CabinState ownBefore = before.FirstOrDefault(state => state.OwnerId == message.FarmerID);
        if (ownBefore != null)
        {
            Building ownAfter = farm.buildings.FirstOrDefault(building =>
                building != null && building.id.Value == ownBefore.BuildingId && building.buildingType.Value == "Cabin");
            if (ownAfter != null && (ownAfter.tileX.Value != ownBefore.X || ownAfter.tileY.Value != ownBefore.Y))
                requestedPosition = new Vector2(ownAfter.tileX.Value, ownAfter.tileY.Value);
        }

        bool removedQueuedCabins = CabinCollectionPatches.RemoveIncomingCabins(farm.buildings,
            before.Select(state => state.CollectionId).ToHashSet());
        bool cabinsChanged = removedQueuedCabins || !CabinsMatch(farm, before);
        if (!cabinsChanged)
        {
            RebroadcastPublicMessage(message);
            return;
        }

        RestoreCabins(farm, before);
        if (requestedPosition.HasValue)
            CabinGenerator.MoveAllCabinsTo(farm, requestedPosition.Value, true);

        // 原包可能带其他人的小屋 等公共队列应用后再按玩家发送
        QueueFarmSnapshots();
    }

    private static List<CabinState> CaptureCabins(Farm farm)
    {
        var states = new List<CabinState>();
        foreach (Building building in farm.buildings)
        {
            if (building?.buildingType.Value != "Cabin") continue;
            states.Add(new CabinState
            {
                Building = building,
                BuildingId = building.id.Value,
                CollectionId = farm.buildings.GuidOf(building),
                OwnerId = GetOwnerId(building),
                X = building.tileX.Value,
                Y = building.tileY.Value
            });
        }
        return states;
    }

    private static bool CabinsMatch(Farm farm, List<CabinState> states)
    {
        var current = farm.buildings.Where(building => building?.buildingType.Value == "Cabin").ToList();
        if (current.Count != states.Count) return false;
        foreach (CabinState state in states)
        {
            if (!farm.buildings.Contains(state.Building)) return false;
            if (state.Building.tileX.Value != state.X || state.Building.tileY.Value != state.Y) return false;
        }
        return true;
    }

    private static void RebroadcastPublicMessage(IncomingMessage message)
    {
        var outgoing = new OutgoingMessage(message);
        foreach (long peer in Game1.otherFarmers.Keys)
        {
            if (peer != message.FarmerID)
                Game1.server.sendMessage(peer, outgoing);
        }
    }

    private static void RestoreCabins(Farm farm, List<CabinState> states)
    {
        var originals = new HashSet<Building>(states.Select(state => state.Building));
        foreach (Building building in farm.buildings.ToList())
        {
            if (building?.buildingType.Value == "Cabin" && !originals.Contains(building))
                farm.buildings.Remove(building);
        }

        foreach (CabinState state in states)
        {
            if (!farm.buildings.Contains(state.Building))
            {
                foreach (Building duplicate in farm.buildings.Where(building =>
                    building?.buildingType.Value == "Cabin" && building.id.Value == state.BuildingId).ToList())
                {
                    farm.buildings.Remove(duplicate);
                }
                farm.buildings.Add(state.Building);
            }

            // 只恢复位置 客户端不能改小屋坐标
            if (state.Building.tileX.Value != state.X || state.Building.tileY.Value != state.Y)
            {
                state.Building.tileX.Value = state.X;
                state.Building.tileY.Value = state.Y;
                CabinGenerator.UpdateCabinWarps(farm, state.Building);
            }
        }
    }

    /// <summary>reader 已越过地点头 判断增量是否包含建筑集合</summary>
    private static bool DeltaTouchesBuildings(BinaryReader reader, Farm farm)
    {
        long position = reader.BaseStream.Position;
        try
        {
            NetVersion version = default;
            version.Read(reader);
            byte deltaType = reader.ReadByte();
            reader.ReadUInt32();
            if (deltaType == 1) return true; // 整体替换 必含建筑

            var bits = reader.ReadBitArray();
            int index = GetBuildingsFieldIndex(farm);
            return index < 0 || (index < bits.Length && bits[index]);
        }
        catch
        {
            return true;
        }
        finally
        {
            reader.BaseStream.Position = position;
        }
    }

    private static int GetBuildingsFieldIndex(Farm farm)
    {
        if (_buildingsFieldIndex >= 0) return _buildingsFieldIndex;
        int index = 0;
        foreach (var field in farm.NetFields.GetFields())
        {
            if (ReferenceEquals(field, farm.buildings))
            {
                _buildingsFieldIndex = index;
                break;
            }
            index++;
        }
        return _buildingsFieldIndex;
    }

    private static void ProcessTeamMessagePrefix(IncomingMessage msg)
    {
        if (Game1.IsMasterGame && msg.MessageType == 13)
            _teamMessageSource = msg.FarmerID;
    }

    private static void ProcessTeamMessagePostfix(IncomingMessage msg)
    {
        if (msg.MessageType == 13)
            _teamMessageSource = 0;
    }

    private static bool BuildingDemolishedPrefix(BinaryReader reader)
    {
        _demolishedOwner = 0;
        if (!Game1.IsMasterGame || _teamMessageSource == 0) return true;

        long position = reader.BaseStream.Position;
        string locationName = reader.ReadString();
        string buildingType = reader.ReadString();
        Guid buildingId = reader.ReadGuid();
        reader.BaseStream.Position = position;
        if (buildingType != "Cabin") return true;

        Building building = Game1.getLocationFromName(locationName)?.getBuildingById(buildingId);
        if (building?.buildingType.Value != "Cabin" || GetOwnerId(building) != _teamMessageSource)
            return false; // 只能拆自己的小屋

        _demolishedOwner = _teamMessageSource;
        return true;
    }

    private static void BuildingDemolishedFinalizer()
    {
        if (_demolishedOwner == 0) return;
        SendFarmSnapshot(_demolishedOwner);
        _demolishedOwner = 0;
    }

    internal static void QueueFarmSnapshots()
    {
        if (_snapshotQueued) return;
        _snapshotQueued = true;
        DelayedAction.functionAfterDelay(() =>
        {
            _snapshotQueued = false;
            SendAllFarmSnapshots();
        }, 100);
    }

    internal static void SendAllFarmSnapshots()
    {
        if (!Game1.IsMasterGame) return;
        foreach (long peer in Game1.otherFarmers.Keys.ToList())
            SendFarmSnapshot(peer);
    }

    private static void SendFarmSnapshot(long peer)
    {
        if (!Game1.IsMasterGame || !Game1.otherFarmers.TryGetValue(peer, out Farmer farmer)) return;
        Farm farm = Game1.getFarm();
        if (farm == null) return;

        bool insideOwnCabin = farmer.currentLocation is Cabin cabin && cabin.OwnerId == peer;
        if (farmer.currentLocation != farm && !insideOwnCabin)
        {
            _pendingFarmSnapshots.Add(peer);
            return;
        }

        _pendingFarmSnapshots.Remove(peer);
        SendNativeLocation(peer, farm, false);
        if (insideOwnCabin)
        {
            GameLocation interior = FindOwnedCabin(peer)?.GetIndoors();
            if (interior != null)
                SendNativeLocation(peer, interior, true);
        }
    }

    private static void SendNativeLocation(long peer, GameLocation location, bool forceCurrent)
    {
        if (Game1.server == null || Game1.serverHost?.Value == null || location == null) return;
        FilterState state = EnterFullSync(peer);
        try
        {
            byte[] bytes = Game1.multiplayer.writeObjectFullBytes<GameLocation>(
                Game1.multiplayer.locationRoot(location), peer);
            Game1.server.sendMessage(peer, 3, Game1.serverHost.Value, forceCurrent, bytes);
        }
        finally
        {
            ExitFilter(state);
        }
    }

    private static Building FindOwnedCabin(long ownerId)
    {
        Farm farm = Game1.getFarm();
        if (farm == null) return null;
        return farm.buildings.FirstOrDefault(building =>
            building?.buildingType.Value == "Cabin" && GetOwnerId(building) == ownerId);
    }

    internal static long GetOwnerId(Building building)
    {
        return (building?.GetIndoors() as Cabin)?.OwnerId ?? 0;
    }

    private static FilterState EnterFullSync(long peer)
    {
        var state = new FilterState(_fullSyncPeer, _filterDelta, true);
        _fullSyncPeer = peer;
        _filterDelta = false;
        return state;
    }

    private static FilterState EnterDeltaFilter()
    {
        var state = new FilterState(_fullSyncPeer, _filterDelta, true);
        _fullSyncPeer = null;
        _filterDelta = true;
        return state;
    }

    private static void ExitFilter(FilterState state)
    {
        if (!state.Entered) return;
        _fullSyncPeer = state.PreviousPeer;
        _filterDelta = state.PreviousDelta;
    }

    private sealed class CabinState
    {
        public Building Building;
        public Guid BuildingId;
        public Guid CollectionId;
        public long OwnerId;
        public int X;
        public int Y;
    }

    private readonly struct FilterState
    {
        public readonly long? PreviousPeer;
        public readonly bool PreviousDelta;
        public readonly bool Entered;

        public FilterState(long? previousPeer, bool previousDelta, bool entered)
        {
            PreviousPeer = previousPeer;
            PreviousDelta = previousDelta;
            Entered = entered;
        }
    }
}
