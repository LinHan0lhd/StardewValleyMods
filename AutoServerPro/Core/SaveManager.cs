#nullable disable
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using AutoServerPro.Models;

namespace AutoServerPro.Core
{
    public class SaveManager
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private ModConfig _config;
        private readonly FestivalManager _festivalManager;

        private readonly SavePathManager _pathManager;
        private readonly SaveAutoLoader _autoLoader;
        private readonly SaveStateRestorer _stateRestorer;
        private readonly SaveBackupManager _backupManager;
        private readonly SaveProcessCoordinator _processCoordinator;

        private int _freezeDelay = 0;
        private int _skipScheduleFrames = 0;

        public bool IsSavingComplete => _processCoordinator.IsSavingComplete;
        public bool IsSaving => _processCoordinator.IsSaving;
        public bool IsWaitingFestivalEnd => _processCoordinator.IsWaitingFestivalEnd;
        public string CurrentSavesPath => _pathManager.CurrentSavesPath;

        public SaveManager(IMonitor monitor, ModConfig config, IModHelper helper, FestivalManager festivalManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _festivalManager = festivalManager;

            _pathManager = new SavePathManager(monitor, config);
            _autoLoader = new SaveAutoLoader(monitor, config, _pathManager);
            _stateRestorer = new SaveStateRestorer(monitor, _pathManager);
            _backupManager = new SaveBackupManager(monitor, config, _pathManager);
            _processCoordinator = new SaveProcessCoordinator(monitor, helper, config, _pathManager, festivalManager);

            _helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            // 1. 激活 checkSchedule 拦截补丁，防止日程系统覆盖刚还原的 NPC 位置
            //    原生加载流程：initializeCharacter 读取 DefaultPosition 设置位置 → 
            //    但 NPC.update 中的 checkSchedule 会用预计算的过时路径起点覆盖它
            SavePatch.SkipSchedule = true;
            _skipScheduleFrames = 3;

            // 2. 恢复额外数据（时间、矿井、掉落物）
            //    NPC 位置已由原生存档通过 DefaultMap/DefaultPosition 恢复
            if (!string.IsNullOrEmpty(_autoLoader.CurrentSaveName))
                _stateRestorer.RestoreExtraDataAfterLoad(_autoLoader.CurrentSaveName);

            // 3. 延迟恢复 NPC 日程
            _freezeDelay = 2;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _processCoordinator.TickFestivalSaveFlow();

            // 递减 SkipSchedule 计数器，3帧后允许 checkSchedule 正常执行
            if (_skipScheduleFrames > 0)
            {
                _skipScheduleFrames--;
                if (_skipScheduleFrames == 0)
                {
                    SavePatch.SkipSchedule = false;
                    _monitor.Log("NPC checkSchedule 拦截已解除，日程系统恢复正常", LogLevel.Debug);
                }
            }

            // 延迟恢复 NPC 日程（在 SkipSchedule 解除后执行）
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

        public void ForceSaveNow() => _processCoordinator.ForceSaveNow();
        public void ForceSaveAndQuit() => _processCoordinator.ForceSaveAndQuit();
        public void UpdateSave() => _processCoordinator.UpdateSave();
        public void TickFestivalSaveFlow() => _processCoordinator.TickFestivalSaveFlow();

        public void AutoBackupCheck() => _backupManager.AutoBackupCheck(_autoLoader.CurrentSaveName);
        public void ManualBackupCommand(string arg, string[] args) => _backupManager.ManualBackupCommand(arg);

        public void CreateNewWorld(string saveName, string hostName = null) =>
            _processCoordinator.CreateNewWorld(saveName, hostName);
    }
}
