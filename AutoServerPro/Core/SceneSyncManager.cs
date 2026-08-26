using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SceneSyncManager
{
    private ModConfig _config;
    private long _syncSourceId = 0;
    private Queue<long> _joinOrder = new();
    private IModHelper _helper;

    public SceneSyncManager(ModConfig config, IModHelper helper)
    {
        _config = config;
        _helper = helper;
    }
    public void UpdateConfig(ModConfig config) => _config = config;

    public void AddPlayer(long id) => _joinOrder.Enqueue(id);
    public void RemovePlayer(long id) => _joinOrder = new Queue<long>(_joinOrder.Where(p => p != id));

    public void TeleportToFarm()
    {
        Game1.warpFarmer("Farm", 999, 999, false);
    }

    public void UpdateSyncSource()
    {
        while (_joinOrder.Count > 0 && !Game1.otherFarmers?.ContainsKey(_joinOrder.Peek()) == true)
            _joinOrder.Dequeue();

        if (_config.SyncPlayerId != 0 && Game1.otherFarmers?.TryGetValue(_config.SyncPlayerId, out var named) == true && named?.isActive() == true)
        {
            _syncSourceId = _config.SyncPlayerId;
            return;
        }
        _syncSourceId = _joinOrder.Count > 0 ? _joinOrder.Peek() : 0;
    }

    public void SyncLocation()
    {
        if (_syncSourceId == 0 || Game1.otherFarmers == null) return;
        if (!Game1.otherFarmers.TryGetValue(_syncSourceId, out var leader)) return;
        if (leader == null || !leader.isActive()) return;

        var loc = leader.currentLocation;
        if (loc == null || Game1.currentLocation == loc) return;

        string name = loc.NameOrUniqueName;
        if (name == "Temp" || loc is FarmHouse || loc is Cabin)
            return;

        Game1.warpFarmer(name, 999, 999, false);
        EventHandler<UpdateTickedEventArgs>? fix = null;
        fix = (s, e) =>
        {
            if (!Game1.isWarping)
            {
                Game1.player.setTileLocation(new Vector2(999, 999));
                _helper.Events.GameLoop.UpdateTicked -= fix;
            }
        };
        _helper.Events.GameLoop.UpdateTicked += fix;
    }

    public bool AnyPlayerInTemp() => Game1.getOnlineFarmers()?.Any(f => f?.currentLocation?.Name == "Temp") == true;
}