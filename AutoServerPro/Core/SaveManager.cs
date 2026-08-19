#nullable disable
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using AutoServerPro.Models;

namespace AutoServerPro.Core;

public class SaveManager
{
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

        _freezeDelay = 2;

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

    public void UpdateConfig(ModConfig config) => _config = config;

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
    public void ManualBackupCommand(string arg, string[] args) => _backupManager.ManualBackupCommand(arg);

    public void CreateNewWorld(string saveName, string hostName = null) =>
        _processCoordinator.CreateNewWorld(saveName, hostName);
}