#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using AutoServerPro.Utils;
using AutoServerPro.Models;

namespace AutoServerPro.Core
{
    public class GameStateSnapshot
    {
        public int TimeOfDay { get; set; }
        public int DayOfMonth { get; set; }
        public string Season { get; set; }
        public int Year { get; set; }
        public int DayOfWeek { get; set; }
        public string Weather { get; set; }
        public bool IsRaining { get; set; }
        public bool IsSnowing { get; set; }
        public bool IsLightning { get; set; }
        public int MineLowestLevelReached { get; set; }
        public List<PlayerPosition> PlayerPositions { get; set; } = new();
        public List<MapObjectState> MapObjects { get; set; } = new();
        public List<NpcState> NpcStates { get; set; } = new();
    }

    public class PlayerPosition
    {
        public string Name { get; set; }
        public string UniqueId { get; set; }
        public string LocationName { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int FacingDirection { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }
        public bool IsFrozen { get; set; }
    }

    public class MapObjectState
    {
        public string LocationName { get; set; }
        public string ObjectType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int StackSize { get; set; }
        public int Quality { get; set; }
        public int MinutesUntilReady { get; set; }
    }

    public class NpcState
    {
        public string Name { get; set; }
        public string CurrentLocation { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int FacingDirection { get; set; }
        public bool IsInBuilding { get; set; }
        public bool IsWalking { get; set; }
        public bool IsEmoting { get; set; }
    }

    public class SaveManager
    {
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private readonly IModHelper _helper;
        private string _currentSaveName = "";

        private IEnumerator<int> _saveCoroutine;
        private bool _isSaving;
        private bool _saveNeedExtraData;
        private GameStateSnapshot _pendingSnapshot;

        public bool IsSaving => _isSaving;

        public SaveManager(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            _monitor = monitor;
            _config = config;
            _helper = helper;
        }

        public void UpdateConfig(ModConfig config) => _config = config;

        public string SavesRootPath => string.IsNullOrWhiteSpace(_config.CustomSavesPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves")
            : _config.CustomSavesPath;

        public string BackupRootPath => string.IsNullOrWhiteSpace(_config.BackupPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "AutoServerBackups")
            : _config.BackupPath;

        private string ExtraDataPath(string saveName) => Path.Combine(SavesRootPath, saveName, ".extradata.json");

        private string GetBackupRootPath()
        {
            string path = BackupRootPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        // ========== 自动加载 ==========
        public bool AutoLoadSave()
        {
            if (!string.IsNullOrWhiteSpace(_config.NewSaveName))
            {
                string dir = Path.Combine(SavesRootPath, _config.NewSaveName);
                if (Directory.Exists(dir))
                {
                    _monitor.Log($"加载指定存档：{_config.NewSaveName}", LogLevel.Info);
                    LoadSave(_config.NewSaveName);
                    Game1.multiplayerMode = 2;
                    return true;
                }
            }

            string latest = GetLatestSave();
            if (string.IsNullOrEmpty(latest))
            {
                _monitor.Log("未找到存档", LogLevel.Info);
                return false;
            }
            _monitor.Log($"加载最新存档：{latest}", LogLevel.Info);
            LoadSave(latest);
            Game1.multiplayerMode = 2;
            return true;
        }

        private string GetLatestSave()
        {
            if (!Directory.Exists(SavesRootPath)) return null;
            var dirs = Directory.GetDirectories(SavesRootPath);
            if (dirs.Length == 0) return null;
            return Path.GetFileName(dirs.OrderByDescending(d => Directory.GetLastWriteTime(d)).First());
        }

        private void LoadSave(string saveName)
        {
            string path = Path.Combine(SavesRootPath, saveName);
            if (!Directory.Exists(path))
            {
                _monitor.Log($"存档 {saveName} 不存在", LogLevel.Warn);
                return;
            }
            try
            {
                SaveGame.Load(saveName);
                _currentSaveName = saveName;
                if (Game1.activeClickableMenu is TitleMenu menu) menu.exitThisMenu(false);
                _monitor.Log($"存档 {saveName} 加载成功", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"加载存档失败：{ex.Message}", LogLevel.Error);
            }
        }

        // ========== 额外数据恢复 ==========
        public void RestoreExtraDataAfterLoad()
        {
            if (string.IsNullOrEmpty(_currentSaveName)) return;

            string extraPath = ExtraDataPath(_currentSaveName);
            if (!File.Exists(extraPath))
            {
                _monitor.Log("无额外存档数据，跳过恢复", LogLevel.Debug);
                return;
            }

            try
            {
                string json = File.ReadAllText(extraPath);
                var snapshot = JsonSerializer.Deserialize<GameStateSnapshot>(json);
                if (snapshot == null) return;

                _monitor.Log($"恢复额外数据 - 时间: {snapshot.Season} {snapshot.DayOfMonth}日 {snapshot.TimeOfDay}:00", LogLevel.Info);

                RestoreSnapshot(snapshot);
                _monitor.Log("额外数据恢复完成", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复额外数据失败: {ex.Message}", LogLevel.Warn);
            }
        }

        private void RestoreSnapshot(GameStateSnapshot snapshot)
        {
            try
            {
                Game1.timeOfDay = snapshot.TimeOfDay;
                Game1.dayOfMonth = snapshot.DayOfMonth;
                Game1.currentSeason = snapshot.Season;
                Game1.year = snapshot.Year;
                Game1.dayOfWeek = snapshot.DayOfWeek;
                Game1.isRaining = snapshot.IsRaining;
                Game1.isSnowing = snapshot.IsSnowing;
                Game1.isLightning = snapshot.IsLightning;

                if (snapshot.MineLowestLevelReached > 0)
                {
                    MineShaft.lowestLevelReached = snapshot.MineLowestLevelReached;
                }

                _monitor.Log($"时间已恢复: {snapshot.Season} {snapshot.DayOfMonth}日 {snapshot.TimeOfDay}:00", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复时间数据失败: {ex.Message}", LogLevel.Warn);
            }

            try
            {
                RestorePlayerPositions(snapshot);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复玩家位置失败: {ex.Message}", LogLevel.Warn);
            }
        }

        private void RestorePlayerPositions(GameStateSnapshot snapshot)
        {
            if (snapshot.PlayerPositions == null || snapshot.PlayerPositions.Count == 0) return;

            _monitor.Log($"恢复 {snapshot.PlayerPositions.Count} 个玩家位置", LogLevel.Debug);

            foreach (var pos in snapshot.PlayerPositions)
            {
                if (pos.IsFrozen) continue;

                if (pos.Name == Game1.player.Name && pos.LocationName == Game1.player.currentLocation.NameOrUniqueName)
                {
                    Game1.player.position.Set(pos.X, pos.Y);
                    Game1.player.facingDirection = pos.FacingDirection;
                    _monitor.Log($"  主机 {pos.Name} 位置: ({pos.X}, {pos.Y})", LogLevel.Debug);
                }
            }

            foreach (var farmer in Game1.otherFarmers)
            {
                var saved = snapshot.PlayerPositions.FirstOrDefault(p => p.UniqueId == farmer.UniqueMultiplayerID);
                if (saved != null && saved.LocationName == farmer.currentLocation?.NameOrUniqueName)
                {
                    farmer.position.Set(saved.X, saved.Y);
                    farmer.facingDirection = saved.FacingDirection;
                    _monitor.Log($"  玩家 {farmer.Name} 位置: ({saved.X}, {saved.Y})", LogLevel.Debug);
                }
            }
        }

        // ========== 备份系统 ==========
        public void AutoBackupCheck()
        {
            if (!Context.IsWorldReady || string.IsNullOrEmpty(_currentSaveName)) return;
            string saveDir = Path.Combine(SavesRootPath, _currentSaveName);
            if (!Directory.Exists(saveDir)) return;

            string backupRoot = GetBackupRootPath();

            string marker = Path.Combine(saveDir, ".last_backup");
            int today = Game1.Date.TotalDays;

            if (File.Exists(marker) && int.TryParse(File.ReadAllText(marker).Trim(), out int last) && (today - last) < _config.AutoBackupDayInterval)
                return;

            DoSaveBackup(_currentSaveName);
            File.WriteAllText(marker, today.ToString());

            if (_config.AutoCleanOldBackup) CleanOldBackups(backupRoot);
        }

        private void DoSaveBackup(string saveName)
        {
            string src = Path.Combine(SavesRootPath, saveName);
            string time = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dst = Path.Combine(BackupRootPath, $"{saveName}_{time}");
            DirectoryHelper.CopyDirectory(src, dst, true);
            _monitor.Log($"备份完成：{Path.GetFileName(dst)}", LogLevel.Info);
        }

        private void CleanOldBackups(string root)
        {
            if (!Directory.Exists(root)) return;
            var dirs = Directory.GetDirectories(root)
                .Select(d => new DirectoryInfo(d))
                .OrderByDescending(d => d.CreationTime)
                .ToList();
            if (dirs.Count <= _config.MaxBackupCount) return;
            foreach (var d in dirs.Skip(_config.MaxBackupCount))
            {
                Directory.Delete(d.FullName, true);
                _monitor.Log($"清理过期备份：{d.Name}", LogLevel.Debug);
            }
        }

        public void ManualBackupCommand(string _, string[] __)
        {
            if (!Context.IsWorldReady || string.IsNullOrEmpty(_currentSaveName))
            {
                _monitor.Log("请先加载存档", LogLevel.Warn);
                return;
            }
            DoSaveBackup(_currentSaveName);
        }

        // ========== 游戏状态快照 ==========
        private GameStateSnapshot CreateSnapshot()
        {
            var snapshot = new GameStateSnapshot
            {
                TimeOfDay = Game1.timeOfDay,
                DayOfMonth = Game1.dayOfMonth,
                Season = Game1.currentSeason,
                Year = Game1.year,
                DayOfWeek = Game1.dayOfWeek,
                IsRaining = Game1.isRaining,
                IsSnowing = Game1.isSnowing,
                IsLightning = Game1.isLightning,
                MineLowestLevelReached = Game1.mineLowestLevelReached
            };

            snapshot.PlayerPositions.Add(new PlayerPosition
            {
                Name = Game1.player.Name,
                UniqueId = Game1.player.UniqueMultiplayerID,
                LocationName = Game1.player.currentLocation?.NameOrUniqueName ?? "",
                X = Game1.player.position.X,
                Y = Game1.player.position.Y,
                FacingDirection = Game1.player.facingDirection,
                Health = Game1.player.health,
                MaxHealth = Game1.player.maxHealth,
                Stamina = Game1.player.Stamina,
                MaxStamina = Game1.player.MaxStamina,
                IsFrozen = Game1.player.freezePause > 0
            });

            foreach (var farmer in Game1.otherFarmers)
            {
                snapshot.PlayerPositions.Add(new PlayerPosition
                {
                    Name = farmer.Name,
                    UniqueId = farmer.UniqueMultiplayerID,
                    LocationName = farmer.currentLocation?.NameOrUniqueName ?? "",
                    X = farmer.position.X,
                    Y = farmer.position.Y,
                    FacingDirection = farmer.facingDirection,
                    Health = farmer.health,
                    MaxHealth = farmer.maxHealth,
                    Stamina = farmer.Stamina,
                    MaxStamina = farmer.MaxStamina,
                    IsFrozen = farmer.freezePause > 0
                });
            }

            foreach (var location in Game1.locations)
            {
                if (location == null) continue;

                if (location.Objects != null)
                {
                    foreach (var kvp in location.Objects.Pairs)
                    {
                        var obj = kvp.Value;
                        if (obj == null) continue;
                        if (obj is CrabPot || obj is IndoorPot || obj is ItemPedestal) continue;

                        try
                        {
                            snapshot.MapObjects.Add(new MapObjectState
                            {
                                LocationName = location.NameOrUniqueName,
                                ObjectType = obj.GetType().Name,
                                X = (int)kvp.Key.X,
                                Y = (int)kvp.Key.Y,
                                StackSize = obj.StackSize,
                                Quality = obj.Quality,
                                MinutesUntilReady = obj.MinutesUntilReady,
                                HoldsObjCount = obj.heldObject != null ? 1 : 0
                            });
                        }
                        catch { }
                    }
                }

                if (location is Farm farm && farm.debris != null)
                {
                    foreach (var debris in farm.debris)
                    {
                        if (debris.item != null)
                        {
                            try
                            {
                                snapshot.MapObjects.Add(new MapObjectState
                                {
                                    LocationName = location.NameOrUniqueName,
                                    ObjectType = "Debris",
                                    X = (int)debris.position.X,
                                    Y = (int)debris.position.Y,
                                    StackSize = debris.item.StackSize,
                                    Quality = debris.item.Quality
                                });
                            }
                            catch { }
                        }
                    }
                }
            }

            foreach (var npc in Utility.getAllCharacters())
            {
                try
                {
                    snapshot.NpcStates.Add(new NpcState
                    {
                        Name = npc.Name,
                        CurrentLocation = npc.currentLocation?.NameOrUniqueName ?? "",
                        X = npc.position.X,
                        Y = npc.position.Y,
                        FacingDirection = npc.facingDirection,
                        IsInBuilding = npc.currentLocation is Building,
                        IsWalking = npc.Schedule != null,
                        IsEmoting = npc.IsEmoting
                    });
                }
                catch { }
            }

            return snapshot;
        }

        private void SaveExtraData(GameStateSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(_currentSaveName)) return;

            try
            {
                string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                string path = ExtraDataPath(_currentSaveName);
                File.WriteAllText(path, json);
                _monitor.Log($"额外数据已保存: {snapshot.PlayerPositions.Count}玩家, {snapshot.MapObjects.Count}物品, {snapshot.NpcStates.Count}NPC", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn);
            }
        }

        // ========== 即时保存 ==========
        public void ForceSaveNow()
        {
            if (!Context.IsWorldReady)
            {
                _monitor.Log("世界未加载，无法保存", LogLevel.Warn);
                return;
            }
            if (_isSaving)
            {
                _monitor.Log("正在保存中，请稍候...", LogLevel.Warn);
                return;
            }
            if (SaveGame.IsProcessing)
            {
                _monitor.Log("游戏正在处理保存，强制重置状态...", LogLevel.Warn);
                SaveGame.IsProcessing = false;
            }

            try
            {
                _monitor.Log($"保存状态检查 - SaveFolder: {Constants.SaveFolderName}, IsProcessing: {SaveGame.IsProcessing}", LogLevel.Debug);

                _pendingSnapshot = CreateSnapshot();
                _saveNeedExtraData = true;

                SaveGame.IsProcessing = true;
                var getSaveEnumerator = typeof(SaveGame).GetMethod("getSaveEnumerator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (getSaveEnumerator == null)
                {
                    _monitor.Log("找不到 getSaveEnumerator 方法", LogLevel.Error);
                    SaveGame.IsProcessing = false;
                    _pendingSnapshot = null;
                    return;
                }

                _saveCoroutine = (System.Collections.Generic.IEnumerator<int>)getSaveEnumerator.Invoke(null, null);
                _isSaving = true;
                _monitor.Log($"开始即时保存... (时间: {Game1.currentSeason} {Game1.dayOfMonth}日 {Game1.timeOfDay}:00)", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"启动保存失败: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                SaveGame.IsProcessing = false;
                _pendingSnapshot = null;
            }
        }

        public void UpdateSave()
        {
            if (!_isSaving || _saveCoroutine == null) return;

            try
            {
                bool moved = _saveCoroutine.MoveNext();
                int progress = moved ? _saveCoroutine.Current : -1;

                if (moved && progress == 100)
                {
                    _monitor.Log($"原生保存完成！进度: {progress}", LogLevel.Info);
                    _isSaving = false;
                    _saveCoroutine = null;
                    SaveGame.IsProcessing = false;

                    if (_saveNeedExtraData && _pendingSnapshot != null)
                    {
                        SaveExtraData(_pendingSnapshot);
                        _pendingSnapshot = null;
                        _saveNeedExtraData = false;
                    }

                    if (!string.IsNullOrEmpty(_currentSaveName))
                    {
                        try
                        {
                            DoSaveBackup(_currentSaveName);
                        }
                        catch (Exception ex)
                        {
                            _monitor.Log($"自动备份失败: {ex.Message}", LogLevel.Warn);
                        }
                    }
                }
                else if (moved)
                {
                    if (progress % 20 == 0)
                    {
                        _monitor.Log($"保存进度: {progress}%", LogLevel.Debug);
                    }
                }
                else if (!moved)
                {
                    _monitor.Log($"原生保存完成！(MoveNext返回false)", LogLevel.Info);
                    _isSaving = false;
                    _saveCoroutine = null;
                    SaveGame.IsProcessing = false;

                    if (_saveNeedExtraData && _pendingSnapshot != null)
                    {
                        SaveExtraData(_pendingSnapshot);
                        _pendingSnapshot = null;
                        _saveNeedExtraData = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"保存失败: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                _isSaving = false;
                _saveCoroutine = null;
                SaveGame.IsProcessing = false;
                _pendingSnapshot = null;
                _saveNeedExtraData = false;
            }
        }

        // ========== 创建存档 ==========
        public void CreateNewWorld(string saveName, string hostName = null)
        {
            hostName ??= _config.DefaultHostName;

            string targetPath = Path.Combine(SavesRootPath, saveName);
            if (Directory.Exists(targetPath))
            {
                _monitor.Log($"存档 {saveName} 已存在", LogLevel.Error);
                return;
            }

            try
            {
                Game1.CreateNewGame(saveName);
                Game1.activeClickableMenu = new SaveGameMenu(saveName, true);
                _currentSaveName = saveName;
                _monitor.Log($"已创建新存档：{saveName}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"创建存档失败：{ex.Message}", LogLevel.Error);
            }
        }

        public void SetCurrentSaveName(string saveName)
        {
            _currentSaveName = saveName;
        }
    }
}
