#nullable disable
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SaveManager
{
    private readonly IMonitor _monitor;
    private readonly IModHelper _helper;
    private ModConfig _config;

    private readonly SavePathManager _pathManager;
    private readonly SaveAutoLoader _autoLoader;
    private readonly SaveStateRestorer _stateRestorer;
    private readonly SaveBackupManager _backupManager;
    private readonly SaveProcessCoordinator _processCoordinator;

    private int _freezeDelay = 0;

    public bool IsSavingComplete => _processCoordinator.IsSavingComplete;
    public bool IsSaving => _processCoordinator.IsSaving;
    public bool IsWaitingFestivalEnd => _processCoordinator.IsWaitingFestivalEnd;
    public string CurrentSavesPath => _pathManager.CurrentSavesPath;

    public SaveManager(IMonitor monitor, ModConfig config, IModHelper helper, FestivalManager festivalManager)
    {
        _monitor = monitor;
        _helper = helper;
        _config = config;

        _pathManager = new SavePathManager(monitor, config);
        _autoLoader = new SaveAutoLoader(monitor, config, _pathManager);
        _stateRestorer = new SaveStateRestorer(monitor, _pathManager);
        _backupManager = new SaveBackupManager(monitor, config, _pathManager);
        _processCoordinator = new SaveProcessCoordinator(monitor, config, _pathManager, festivalManager);

        _helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (!Context.IsMainPlayer) return;

        SavePatch.SkipSchedule = true;

        if (!string.IsNullOrEmpty(_autoLoader.CurrentSaveName))
            _stateRestorer.RestoreExtraDataAfterLoad(_autoLoader.CurrentSaveName);

        // 验证联机玩家的 homeLocation 是否正确保留（搬入主屋的玩家应仍为 FarmHouse）。
        // 真正的保留由 MoveInManager 对 NetWorldState.TryAssignFarmhandHome 的补丁保证。
        foreach (var f in Game1.getAllFarmers())
        {
            if (f != null && !f.IsMainPlayer &&
                string.Equals(f.homeLocation.Value, "FarmHouse", StringComparison.OrdinalIgnoreCase))
            {
                _monitor.Log($"[搬家] 存档加载后，{f.Name} 仍住在主屋", LogLevel.Info);
            }
        }

        // 处理隐藏床：主屋默认创建，联机小屋清除
        EnsureHiddenBeds();

        _freezeDelay = 2;
    }

    /// <summary>
    /// 确保主屋有隐藏床（供机器人房主睡觉），并清除所有联机小屋中的隐藏床。
    /// </summary>
    private void EnsureHiddenBeds()
    {
        try
        {
            var farmhouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
            if (farmhouse != null)
            {
                bool hasHidden = farmhouse.furniture.OfType<BedFurniture>()
                    .Any(b => b.TileLocation.X == 999 && b.TileLocation.Y == 999);
                if (!hasHidden)
                {
                    var hidden = new BedFurniture("2048", new Vector2(999, 999));
                    farmhouse.furniture.Add(hidden);
                    _monitor.Log("[隐藏床] 主屋已创建隐藏床", LogLevel.Info);
                }
            }

            // 清除所有联机小屋中的隐藏床
            foreach (var loc in Game1.locations)
            {
                if (loc is Cabin cabin)
                {
                    var hiddenBeds = cabin.furniture.OfType<BedFurniture>()
                        .Where(b => b.TileLocation.X == 999 && b.TileLocation.Y == 999)
                        .ToList();
                    foreach (var hb in hiddenBeds)
                    {
                        cabin.furniture.Remove(hb);
                    }
                    if (hiddenBeds.Count > 0)
                        _monitor.Log($"[隐藏床] 已清除小屋 {cabin.NameOrUniqueName} 的 {hiddenBeds.Count} 个隐藏床", LogLevel.Info);
                }
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"[隐藏床] 处理异常: {ex.Message}", LogLevel.Warn);
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        _processCoordinator.TickFestivalSaveFlow();

        if (_freezeDelay > 0)
        {
            _freezeDelay--;
            if (_freezeDelay == 0) _stateRestorer.ResumeAllNpcSchedules();
        }
    }

    public void UpdateConfig(ModConfig config)
    {
        _config = config;
        _pathManager.UpdateConfig(config);
        _autoLoader.UpdateConfig(config);
        _backupManager.UpdateConfig(config);
        _processCoordinator.UpdateConfig(config);
    }

    public void RedirectSavesToTemp() => _pathManager.RedirectSavesToTemp();
    public void RedirectSavesToOriginal() => _pathManager.RedirectSavesToOriginal();

    public bool AutoLoadSave() => _autoLoader.AutoLoadSave();
    public void SetCurrentSaveName(string saveName) => typeof(SaveAutoLoader)
        .GetProperty("CurrentSaveName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        ?.SetValue(_autoLoader, saveName);

    public void ForceSaveNow(bool allowFestivalQueue = true) => _processCoordinator.ForceSaveNow(allowFestivalQueue);
    public void ForceSaveAndQuit() => _processCoordinator.ForceSaveAndQuit();
    public void UpdateSave() => _processCoordinator.UpdateSave();
    public void TickFestivalSaveFlow() => _processCoordinator.TickFestivalSaveFlow();

    public void AutoBackupCheck() => _backupManager.AutoBackupCheck(_autoLoader.CurrentSaveName);
    public void ManualBackupCommand(string arg, string[] _) => _backupManager.ManualBackupCommand(arg);

    public void CreateNewWorld(string saveName, string hostName = null) =>
        _processCoordinator.CreateNewWorld(saveName, hostName);
}
