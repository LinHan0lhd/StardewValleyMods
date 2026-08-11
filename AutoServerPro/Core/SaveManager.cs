#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using HarmonyLib;
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
    [XmlRoot("Items")]
    public class ItemsWrapper
    {
        [XmlElement("Item")]
        public Item[] Items { get; set; }
    }

    [XmlRoot("DebrisItem")]
    public class DebrisItemState
    {
        [XmlElement("LocationName")]
        public string LocationName { get; set; }

        [XmlElement("ItemId")]
        public string ItemId { get; set; }

        [XmlElement("ItemXml")]
        public string ItemXml { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }

        [XmlElement("Quality")]
        public int Quality { get; set; }

        [XmlElement("X")]
        public float X { get; set; }

        [XmlElement("Y")]
        public float Y { get; set; }

        [XmlElement("ChunkFinalYLevel")]
        public int ChunkFinalYLevel { get; set; }
    }

    [XmlRoot("ShippingBinItem")]
    public class ShippingBinItemState
    {
        [XmlElement("ItemId")]
        public string ItemId { get; set; }

        [XmlElement("ItemXml")]
        public string ItemXml { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }

        [XmlElement("Quality")]
        public int Quality { get; set; }

        [XmlElement("FarmerId")]
        public string FarmerId { get; set; }

        [XmlElement("IsLastItem")]
        public bool IsLastItem { get; set; }
    }

    [XmlRoot("GameStateSnapshot")]
    public class GameStateSnapshot
    {
        [XmlElement("TimeOfDay")]
        public int TimeOfDay { get; set; }

        [XmlElement("Season")]
        public string Season { get; set; }

        [XmlElement("DayOfMonth")]
        public int DayOfMonth { get; set; }

        [XmlElement("MineLowestLevelReached")]
        public int MineLowestLevelReached { get; set; }

        [XmlArray("DebrisItems")]
        [XmlArrayItem("DebrisItem")]
        public List<DebrisItemState> DebrisItems { get; set; } = new();

        [XmlArray("ShippingBinItems")]
        [XmlArrayItem("ShippingBinItem")]
        public List<ShippingBinItemState> ShippingBinItems { get; set; } = new();
    }

    public class SaveManager
    {
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private readonly IModHelper _helper;
        private readonly FestivalManager _festivalManager;
        private string _currentSaveName = "";
        private Harmony _harmony;
        private bool _savePathRedirected;

        private IEnumerator<int> _saveCoroutine;
        private bool _isSaving;
        private bool _saveNeedExtraData;
        private GameStateSnapshot _pendingSnapshot;

        private bool _waitingForFestivalEnd;
        private bool _saveRequestedDuringFestival;

        private GameStateSnapshot _pendingRestoreSnapshot;
        private int _restoreDelayTicks;

        // XML 序列化器
        private static readonly XmlSerializer ItemSerializer;
        private static readonly XmlSerializer SnapshotSerializer;
        private static readonly Type[] ItemDerivedTypes;

        static SaveManager()
        {
            ItemDerivedTypes = Assembly.GetAssembly(typeof(Item))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract)
                .ToArray();
            ItemSerializer = new XmlSerializer(typeof(ItemsWrapper), ItemDerivedTypes);
            SnapshotSerializer = new XmlSerializer(typeof(GameStateSnapshot));
        }

        public bool IsSaving => _isSaving;

        public string CurrentSavesPath { get; private set; }

        public SaveManager(IMonitor monitor, ModConfig config, IModHelper helper, FestivalManager festivalManager)
        {
            _monitor = monitor;
            _config = config;
            _helper = helper;
            _festivalManager = festivalManager;
            CurrentSavesPath = SavesRootPath;
        }

        public void UpdateConfig(ModConfig config) => _config = config;

        public string SavesRootPath => string.IsNullOrWhiteSpace(_config.CustomSavesPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves")
            : _config.CustomSavesPath;

        public string BackupRootPath => string.IsNullOrWhiteSpace(_config.BackupPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "AutoServerBackups")
            : _config.BackupPath;

        public string TempSavesRootPath => string.IsNullOrWhiteSpace(_config.CustomTempSavesPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "TempSaves")
            : _config.CustomTempSavesPath;

        private string ExtraDataPath(string saveName) => Path.Combine(TempSavesRootPath, saveName, ".extradata.xml");

        private string GetBackupRootPath()
        {
            string path = BackupRootPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public void RedirectSavesToTemp()
        {
            if (!_savePathRedirected)
            {
                try
                {
                    _harmony = new Harmony("LinHan.AutoServerPro.SavePathRedirect");
                    var getSavesFolder = AccessTools.Method(typeof(Program), "GetSavesFolder");
                    if (getSavesFolder != null)
                    {
                        var prefix = new HarmonyMethod(typeof(SaveManager), nameof(GetSavesFolderPrefix));
                        _harmony.Patch(getSavesFolder, prefix: prefix);
                        _savePathRedirected = true;
                        _monitor.Log("存档路径已重定向到TempSaves", LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"重定向存档路径失败: {ex.Message}", LogLevel.Error);
                }
            }
            CurrentSavesPath = TempSavesRootPath;
        }

        public void RedirectSavesToOriginal()
        {
            if (_savePathRedirected && _harmony != null)
            {
                _harmony.UnpatchAll("LinHan.AutoServerPro.SavePathRedirect");
                _savePathRedirected = false;
                _monitor.Log("存档路径已恢复到Saves", LogLevel.Debug);
            }
            CurrentSavesPath = SavesRootPath;
        }

        private static bool GetSavesFolderPrefix(ref string __result)
        {
            __result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "TempSaves");
            return false;
        }

        public bool AutoLoadSave()
        {
            if (!string.IsNullOrWhiteSpace(_config.NewSaveName))
            {
                string saveName = _config.NewSaveName;
                string loadSource = DetermineLoadSource(saveName);
                RedirectSavesTo(loadSource);
                _monitor.Log($"加载存档：{saveName}（来源：{loadSource}）", LogLevel.Info);
                LoadSave(saveName);
                Game1.multiplayerMode = 2;
                return true;
            }

            string latest = GetLatestSave();
            if (string.IsNullOrEmpty(latest))
            {
                _monitor.Log("未找到存档", LogLevel.Info);
                return false;
            }
            string loadSource2 = DetermineLoadSource(latest);
            RedirectSavesTo(loadSource2);
            _monitor.Log($"加载存档：{latest}（来源：{loadSource2}）", LogLevel.Info);
            LoadSave(latest);
            Game1.multiplayerMode = 2;
            return true;
        }

        private string DetermineLoadSource(string saveName)
        {
            string savesDir = Path.Combine(SavesRootPath, saveName);
            string tempDir = Path.Combine(TempSavesRootPath, saveName);

            bool hasSaves = Directory.Exists(savesDir);
            bool hasTemp = Directory.Exists(tempDir);

            if (!hasTemp) return "Saves";
            if (!hasSaves) return "TempSaves";

            DateTime? savesTime = GetDirectoryLatestTime(savesDir);
            DateTime? tempTime = GetDirectoryLatestTime(tempDir);

            if (!savesTime.HasValue && tempTime.HasValue) return "TempSaves";
            if (!tempTime.HasValue) return "Saves";

            if (savesTime.Value >= tempTime.Value)
            {
                _monitor.Log($"原生存档({savesTime.Value:HH:mm:ss})不早于临时存档({tempTime.Value:HH:mm:ss})", LogLevel.Debug);
                return "Saves";
            }

            _monitor.Log($"原生存档({savesTime.Value:HH:mm:ss})早于临时存档({tempTime.Value:HH:mm:ss})", LogLevel.Debug);
            return "TempSaves";
        }

        private void RedirectSavesTo(string source)
        {
            if (source == "TempSaves")
                RedirectSavesToTemp();
            else
                RedirectSavesToOriginal();
        }

        private string GetLatestSave()
        {
            if (Directory.Exists(SavesRootPath))
            {
                var dirs = Directory.GetDirectories(SavesRootPath);
                if (dirs.Length > 0)
                {
                    var latestSaves = dirs.OrderByDescending(d => Directory.GetLastWriteTime(d)).First();
                    string saveName = Path.GetFileName(latestSaves);

                    string tempDir = Path.Combine(TempSavesRootPath, saveName);
                    if (Directory.Exists(tempDir))
                    {
                        var savesTime = Directory.GetLastWriteTime(latestSaves);
                        var tempTime = Directory.GetLastWriteTime(tempDir);
                        if (tempTime > savesTime)
                            return saveName;
                    }
                    return saveName;
                }
            }

            if (Directory.Exists(TempSavesRootPath))
            {
                var dirs = Directory.GetDirectories(TempSavesRootPath);
                if (dirs.Length > 0)
                    return Path.GetFileName(dirs.OrderByDescending(d => Directory.GetLastWriteTime(d)).First());
            }

            return null;
        }

        private void LoadSave(string saveName)
        {
            string path = Path.Combine(CurrentSavesPath, saveName);
            if (!Directory.Exists(path))
            {
                _monitor.Log($"存档 {saveName} 不存在于 {CurrentSavesPath}", LogLevel.Warn);
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
                using (var fs = new FileStream(extraPath, FileMode.Open))
                {
                    var snapshot = (GameStateSnapshot)SnapshotSerializer.Deserialize(fs);
                    if (snapshot == null) return;

                    _monitor.Log($"加载存档数据 - 时间: {snapshot.Season} {snapshot.DayOfMonth}日 {snapshot.TimeOfDay}:00, 掉落物: {snapshot.DebrisItems.Count}", LogLevel.Info);

                    _pendingRestoreSnapshot = snapshot;
                    _restoreDelayTicks = 15;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复存档数据失败: {ex.Message}", LogLevel.Warn);
            }
        }

        public void UpdateRestoreDelay()
        {
            if (_pendingRestoreSnapshot == null) return;

            _restoreDelayTicks--;
            if (_restoreDelayTicks > 0) return;

            try
            {
                _monitor.Log($"延迟恢复存档数据 (延迟{_restoreDelayTicks + 1}tick已过)", LogLevel.Debug);
                RestoreSnapshot(_pendingRestoreSnapshot);
                ResetNpcSchedules();
                _monitor.Log("存档数据恢复完成", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复存档数据失败: {ex.Message}", LogLevel.Warn);
            }
            finally
            {
                _pendingRestoreSnapshot = null;
            }
        }

        private DateTime? GetDirectoryLatestTime(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return null;

            try
            {
                var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
                if (files.Length == 0) return null;

                DateTime latest = DateTime.MinValue;
                foreach (var file in files)
                {
                    var time = File.GetLastWriteTime(file);
                    if (time > latest) latest = time;
                }
                return latest;
            }
            catch
            {
                return null;
            }
        }

        private void RestoreSnapshot(GameStateSnapshot snapshot)
        {
            try
            {
                RestoreTimeByTick(snapshot.TimeOfDay);

                if (snapshot.MineLowestLevelReached > 0)
                {
                    MineShaft.lowestLevelReached = snapshot.MineLowestLevelReached;
                }

                _monitor.Log($"时间已恢复: {Game1.currentSeason} {Game1.dayOfMonth}日 {Game1.timeOfDay}:00", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复时间数据失败: {ex.Message}", LogLevel.Warn);
            }

            try
            {
                RestoreDebrisItems(snapshot);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复掉落物失败: {ex.Message}", LogLevel.Warn);
            }

            try
            {
                RestoreShippingBinItems(snapshot);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复出货箱失败: {ex.Message}", LogLevel.Warn);
            }

            _monitor.Log("玩家位置由原生存档处理（SyncOnlinePlayerPositions已同步在线位置）", LogLevel.Debug);
        }

        /// <summary>
        /// 通过推进游戏时间的方式到达目标时间，触发完整的游戏逻辑（与 MasterHand mh_time 一致）
        /// </summary>
        private void RestoreTimeByTick(int targetTime)
        {
            int intervals = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, targetTime) / 10;
            if (intervals > 0)
            {
                for (int i = 0; i < intervals; i++)
                    Game1.performTenMinuteClockUpdate();
            }
            else
            {
                for (int i = 0; i < -intervals; i++)
                {
                    Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, -20);
                    Game1.performTenMinuteClockUpdate();
                }
            }
            Game1.outdoorLight = Color.White;
            Game1.ambientLight = Color.White;
            Game1.gameTimeInterval = 0;
            Game1.UpdateGameClock(Game1.currentGameTime);

            _monitor.Log($"时间推进完成: {Game1.timeOfDay / 100:D2}:{Game1.timeOfDay % 100:D2}", LogLevel.Debug);
        }

        private void RestoreDebrisItems(GameStateSnapshot snapshot)
        {
            if (snapshot.DebrisItems == null || snapshot.DebrisItems.Count == 0) return;

            _monitor.Log($"恢复 {snapshot.DebrisItems.Count} 个掉落物", LogLevel.Debug);

            var snapshotLocations = snapshot.DebrisItems
                .Select(d => d.LocationName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            int cleared = 0;
            foreach (var locName in snapshotLocations)
            {
                var location = Game1.locations.FirstOrDefault(l => l.NameOrUniqueName == locName);
                if (location == null) continue;

                int beforeCount = location.debris.Count;
                location.debris.Clear();
                cleared += beforeCount;
            }
            if (cleared > 0)
                _monitor.Log($"已清除 {cleared} 个原生残留掉落物，准备恢复快照", LogLevel.Debug);

            int restored = 0;
            int locNotFound = 0;
            int itemFailed = 0;
            int oldFormatFallback = 0;  // 使用旧存档格式（没有ChunkFinalYLevel）的物品数量
            foreach (var debrisState in snapshot.DebrisItems)
            {
                try
                {
                    var location = Game1.locations.FirstOrDefault(l => l.NameOrUniqueName == debrisState.LocationName);
                    if (location == null)
                    {
                        locNotFound++;
                        continue;
                    }

                    Item item = null;

                    if (!string.IsNullOrEmpty(debrisState.ItemXml))
                    {
                        try
                        {
                            item = DeserializeItem(debrisState.ItemXml);
                        }
                        catch (Exception ex)
                        {
                            _monitor.Log($"  XML反序列化异常: {ex.Message}", LogLevel.Trace);
                        }
                    }

                    if (item == null && !string.IsNullOrEmpty(debrisState.ItemId))
                    {
                        // 回退创建时使用保存的数量和品质
                        int amount = debrisState.Amount > 0 ? debrisState.Amount : 1;
                        int quality = debrisState.Quality;
                        item = ItemRegistry.Create(debrisState.ItemId, amount, quality);
                    }

                    if (item == null)
                    {
                        itemFailed++;
                        continue;
                    }

                    var targetPos = new Vector2(debrisState.X, debrisState.Y);
                    var debris = new Debris();
                    
                    // 使用公共属性 item，其 setter 会自动设置 netItem.Value
                    debris.item = item;
                    debris.itemId.Value = item.QualifiedItemId;
                    debris.itemQuality = item.Quality; // 关键：itemQuality 决定物品大小和阴影位置
                    debris.debrisType.Value = Debris.DebrisType.OBJECT;

                    // 使用保存的 ChunkFinalYLevel（物品静止时的 Y 坐标）
                    // 这是物品真正"落地"的位置，用于阴影和物理模拟
                    int finalYLevel;
                    if (debrisState.ChunkFinalYLevel != 0)
                    {
                        finalYLevel = debrisState.ChunkFinalYLevel;
                    }
                    else
                    {
                        finalYLevel = (int)targetPos.Y;  // 回退：旧存档没有这个字段
                        oldFormatFallback++;
                    }
                    
                    // 关键：强制物品位置 Y = chunkFinalYLevel
                    // 保存时物品可能在 chunkFinalYLevel 上方（物理模拟未完全稳定）
                    // 恢复时需要强制对齐，确保物品真正"落地"
                    var restoredPos = new Vector2(targetPos.X, finalYLevel);
                    
                    var chunk = new Chunk(restoredPos, 0f, 0f, 0);
                    chunk.hasPassedRestingLineOnce.Value = true;
                    chunk.bounces = 100;
                    chunk.rotationVelocity = 0f;
                    chunk.sinkTimer.Value = int.MaxValue;
                    chunk.hitWall = false;
                    chunk.bob = 0f;
                    chunk.alpha = 1f;
                    chunk.position.Field.CancelInterpolation();
                    debris.Chunks.Add(chunk);
                    debris.chunkFinalYLevel = finalYLevel;
                    debris.chunksMoveTowardPlayer = false;
                    debris.isSinking.Value = false;

                    location.debris.Add(debris);
                    restored++;
                    
                    // 调试日志：显示每个恢复的物品信息
                    _monitor.Log($"  恢复: {item.QualifiedItemId} q{item.Quality} stack={item.Stack}", LogLevel.Trace);
                    _monitor.Log($"    saved: X={debrisState.X:F2} Y={debrisState.Y:F2} chunkFinalYLevel={debrisState.ChunkFinalYLevel}", LogLevel.Trace);
                    _monitor.Log($"    restored: X={chunk.position.Value.X:F2} Y={chunk.position.Value.Y:F2} chunkFinalYLevel={debris.chunkFinalYLevel}", LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    _monitor.Log($"  掉落物恢复异常: {ex.Message}", LogLevel.Trace);
                }
            }

            _monitor.Log($"掉落物恢复: 成功{restored}个物品实例, 位置未找到{locNotFound}, 物品创建失败{itemFailed}, 旧格式回退{oldFormatFallback}个", LogLevel.Debug);
            if (oldFormatFallback > 0)
                _monitor.Log($"检测到 {oldFormatFallback} 个物品使用旧存档格式，建议重新保存以修复位置问题", LogLevel.Warn);
        }

        private string SerializeItem(Item item)
        {
            try
            {
                var wrapper = new ItemsWrapper { Items = new[] { item } };
                using (var ms = new MemoryStream())
                {
                    ItemSerializer.Serialize(ms, wrapper);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                        return sr.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        private Item DeserializeItem(string xml)
        {
            try
            {
                if (string.IsNullOrEmpty(xml)) return null;

                // 只移除 xmlns:xsd 命名空间（它是多余的），保留 xmlns:xsi
                // xmlns:xsi 对多态反序列化至关重要，因为 xsi:type 属性依赖它
                xml = System.Text.RegularExpressions.Regex.Replace(xml, @"\s+xmlns:xsd\s*=\s*[""'][^""']*[""']", "");

                using (var sr = new StringReader(xml))
                {
                    var wrapper = (ItemsWrapper)ItemSerializer.Deserialize(sr);
                    if (wrapper?.Items != null && wrapper.Items.Length > 0)
                        return wrapper.Items[0];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void SyncOnlinePlayerPositions()
        {
            int synced = 0;

            // 仅同步 Farmhand（农场助手）的位置
            // 主机（Game1.player）的位置由原生保存流程直接处理，无需修改 disconnect* 字段
            // 关键：必须设置 disconnectDay = DaysPlayed，否则位置不会被恢复
            // 参考 BedFurniture.ApplyWakeUpPosition()：
            //   if (disconnectDay == DaysPlayed && !newDaySync) 
            //   { who.currentLocation = disconnectLocation; who.Position = disconnectPosition; }
            foreach (var farmer in Game1.otherFarmers.Values)
            {
                if (Game1.netWorldState.Value.farmhandData.TryGetValue(farmer.UniqueMultiplayerID, out var farmhandData))
                {
                    farmhandData.disconnectPosition.Value = farmer.position.Value;
                    farmhandData.disconnectLocation.Value = farmer.currentLocation?.NameOrUniqueName ?? "";
                    farmhandData.disconnectDay.Value = (int)Game1.MasterPlayer.stats.DaysPlayed;  // 关键：设置为当前天数
                    synced++;
                }
            }

            _monitor.Log($"已通过直接修改 farmhandData 同步 {synced} 个在线玩家位置", synced > 0 ? LogLevel.Debug : LogLevel.Trace);
        }

        private void ResetNpcSchedules()
        {
            _monitor.Log("重置 NPC 行程状态，防止卡死...", LogLevel.Debug);

            int resetCount = 0;
            int scheduleCount = 0;
            foreach (var npc in Utility.getAllCharacters())
            {
                try
                {
                    npc.ClearSchedule();  // 清除当前行程
                    npc.followSchedule = true;  // 重新启用行程跟随
                    npc.ignoreScheduleToday = false;
                    npc.lastAttemptedSchedule = -1;
                    npc.currentScheduleDelay = 0f;
                    npc.scheduleDelaySeconds = 0f;
                    npc.DirectionsToNewLocation = null;

                    if (npc.temporaryController != null)
                    {
                        npc.temporaryController = null;
                    }

                    npc.Halt();
                    resetCount++;

                    if (npc.IsVillager)
                    {
                        try
                        {
                            if (npc.TryLoadSchedule())
                                scheduleCount++;
                        }
                        catch { }
                    }
                }
                catch { }
            }

            if (resetCount > 0)
                _monitor.Log($"已重置 {resetCount} 个 NPC 行程状态，{scheduleCount} 个已重新加载日程", LogLevel.Debug);
        }

        public void AutoBackupCheck()
        {
            if (!Context.IsWorldReady || string.IsNullOrEmpty(_currentSaveName)) return;
            string saveDir = Path.Combine(CurrentSavesPath, _currentSaveName);
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
            string src = Path.Combine(CurrentSavesPath, saveName);
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

        private GameStateSnapshot CreateSnapshot()
        {
            var snapshot = new GameStateSnapshot
            {
                TimeOfDay = Game1.timeOfDay,
                Season = Game1.currentSeason,
                DayOfMonth = Game1.dayOfMonth,
                MineLowestLevelReached = MineShaft.lowestLevelReached
            };

            int debrisCount = 0;
            int chunkCount = 0;
            int failedCount = 0;
            int xmlFallbackCount = 0;
            foreach (var location in Game1.locations)
            {
                if (location == null || location.debris == null) continue;

                foreach (var debris in location.debris)
                {
                    try
                    {
                        Item item = debris.item;
                        string itemId = debris.itemId.Value;
                        if (item == null && !string.IsNullOrEmpty(itemId))
                        {
                            item = ItemRegistry.Create(itemId);
                        }
                        if (item == null) continue;

                        // 直接保存物品状态，不要修改游戏世界中的物品
                        // 使用 TargetValue 获取原始存储位置（而非显示用的 Value）
                        string itemXml = null;
                        try
                        {
                            itemXml = SerializeItem(item);
                        }
                        catch { }

                        int amount = item.Stack > 0 ? item.Stack : 1;
                        int quality = item.Quality;
                        string qualifiedItemId = itemId ?? item.QualifiedItemId;

                        // 为每个 Chunk 创建一个 DebrisItemState
                        // 关键：使用 chunkFinalYLevel 作为 Y 坐标
                        // 原因：物品静止时 chunk.position.Y 可能 > chunkFinalYLevel（物理弹跳结果）
                        //       但阴影绘制在 chunkFinalYLevel 处，所以物品应该对齐到阴影位置
                        if (debris.Chunks != null && debris.Chunks.Count > 0)
                        {
                            foreach (var chunk in debris.Chunks)
                            {
                                Vector2 originalPos = chunk.position.Field.TargetValue;
                                float alignedY = debris.chunkFinalYLevel;
                                
                                var state = new DebrisItemState
                                {
                                    LocationName = location.NameOrUniqueName,
                                    ItemId = qualifiedItemId,
                                    ItemXml = itemXml,
                                    Amount = amount,
                                    Quality = quality,
                                    X = originalPos.X,
                                    Y = alignedY,  // 使用 chunkFinalYLevel 而非原始位置
                                    ChunkFinalYLevel = debris.chunkFinalYLevel
                                };

                                snapshot.DebrisItems.Add(state);
                                chunkCount++;
                                
                                // 详细调试日志
                                _monitor.Log($"  保存: {qualifiedItemId} q{quality} stack={amount}", LogLevel.Trace);
                                _monitor.Log($"    original: pos.Y={originalPos.Y:F2} chunkFinalYLevel={debris.chunkFinalYLevel} diff={originalPos.Y - debris.chunkFinalYLevel:F2}", LogLevel.Trace);
                                _monitor.Log($"    saved: X={originalPos.X:F2} Y={alignedY} (aligned to chunkFinalYLevel)", LogLevel.Trace);
                                _monitor.Log($"    xVel={chunk.xVelocity.Value:F2} yVel={chunk.yVelocity.Value:F2} bounces={chunk.bounces} hasPassedResting={chunk.hasPassedRestingLineOnce.Value} sinkTimer={chunk.sinkTimer.Value}", LogLevel.Trace);
                            }
                        }
                        else
                        {
                            // 没有 Chunk 时仍保存一个
                            var state = new DebrisItemState
                            {
                                LocationName = location.NameOrUniqueName,
                                ItemId = qualifiedItemId,
                                ItemXml = itemXml,
                                Amount = amount,
                                Quality = quality,
                                X = 0,
                                Y = 0
                            };

                            snapshot.DebrisItems.Add(state);
                            chunkCount++;
                        }

                        if (itemXml == null) xmlFallbackCount++;
                        debrisCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }
            }

            _monitor.Log($"快照创建完成: {debrisCount}掉落物, {chunkCount}物品实例", LogLevel.Debug);
            if (xmlFallbackCount > 0)
                _monitor.Log($"掉落物XML序列化失败，将用ItemId恢复: {xmlFallbackCount} 个", LogLevel.Warn);
            if (failedCount > 0)
                _monitor.Log($"物品记录失败: {failedCount} 个", LogLevel.Warn);

            // 保存出货箱内容
            SaveShippingBinItems(snapshot);

            return snapshot;
        }

        private void SaveShippingBinItems(GameStateSnapshot snapshot)
        {
            try
            {
                Farm farm = Game1.getFarm();
                if (farm == null || farm.getShippingBin(Game1.player) == null) return;

                var shippingBin = farm.getShippingBin(Game1.player);
                int lastItemId = farm.lastItemShipped?.QualifiedItemId.GetHashCode() ?? 0;

                int count = 0;
                foreach (var item in shippingBin)
                {
                    try
                    {
                        string itemXml = null;
                        try
                        {
                            itemXml = SerializeItem(item);
                        }
                        catch { }

                        string farmerId = "";
                        if (item.modData.ContainsKey("farmerSelling"))
                            farmerId = item.modData["farmerSelling"];

                        snapshot.ShippingBinItems.Add(new ShippingBinItemState
                        {
                            ItemId = item.QualifiedItemId,
                            ItemXml = itemXml,
                            Amount = item.Stack,
                            Quality = item.Quality,
                            FarmerId = farmerId,
                            IsLastItem = (item.GetHashCode() == lastItemId)
                        });
                        count++;
                    }
                    catch { }
                }

                _monitor.Log($"出货箱保存: {count} 个物品", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"出货箱保存失败: {ex.Message}", LogLevel.Warn);
            }
        }

        private void RestoreShippingBinItems(GameStateSnapshot snapshot)
        {
            if (snapshot.ShippingBinItems == null || snapshot.ShippingBinItems.Count == 0) return;

            Farm farm = Game1.getFarm();
            if (farm == null) return;

            _monitor.Log($"恢复 {snapshot.ShippingBinItems.Count} 个出货箱物品", LogLevel.Debug);

            bool separateWallets = Game1.player.team.useSeparateWallets.Value;

            // 清空所有玩家的出货箱（共享模式下所有玩家共用一个）
            if (!separateWallets)
            {
                var sharedBin = farm.getShippingBin(Game1.player);
                if (sharedBin != null)
                    sharedBin.Clear();
            }
            else
            {
                // 单独钱包：清空主机 + 所有在线玩家的个人出货箱
                Game1.player.personalShippingBin.Value?.Clear();
                foreach (var farmer in Game1.otherFarmers.Values)
                    farmer.personalShippingBin.Value?.Clear();
            }

            // 收集所有可用玩家
            var allFarmers = new Dictionary<string, Farmer>(StringComparer.OrdinalIgnoreCase);
            allFarmers[Game1.player.UniqueMultiplayerID] = Game1.player;
            foreach (var kvp in Game1.otherFarmers)
                allFarmers[kvp.Key] = kvp.Value;

            Item lastRestoredItem = null;
            int restored = 0;
            int failed = 0;

            foreach (var state in snapshot.ShippingBinItems)
            {
                try
                {
                    Item item = null;

                    if (!string.IsNullOrEmpty(state.ItemXml))
                        item = DeserializeItem(state.ItemXml);

                    if (item == null && !string.IsNullOrEmpty(state.ItemId))
                        item = ItemRegistry.Create(state.ItemId, state.Amount > 0 ? state.Amount : 1, state.Quality);

                    if (item == null)
                    {
                        failed++;
                        continue;
                    }

                    if (state.Amount > 0)
                        item.Stack = state.Amount;
                    if (state.Quality > 0)
                        item.Quality = state.Quality;

                    // 根据 FarmerId 选择目标玩家的出货箱
                    Farmer targetFarmer = Game1.player;
                    if (!string.IsNullOrEmpty(state.FarmerId) &&
                        allFarmers.TryGetValue(state.FarmerId, out var farmer) &&
                        farmer != null)
                    {
                        targetFarmer = farmer;
                        item.modData["farmerSelling"] = state.FarmerId;
                    }

                    var bin = farm.getShippingBin(targetFarmer);
                    if (bin != null)
                    {
                        bin.Add(item);
                        if (state.IsLastItem)
                            lastRestoredItem = item;
                        restored++;
                    }
                    else
                    {
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"恢复出货箱物品失败: {ex.Message}", LogLevel.Trace);
                    failed++;
                }
            }

            if (lastRestoredItem != null)
                farm.lastItemShipped = lastRestoredItem;

            _monitor.Log($"出货箱恢复: 成功{restored} 个, 失败{failed} 个", LogLevel.Debug);
        }

        private void SaveExtraData(GameStateSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(_currentSaveName)) return;

            try
            {
                string tempSaveDir = Path.Combine(TempSavesRootPath, _currentSaveName);
                if (!Directory.Exists(tempSaveDir))
                    Directory.CreateDirectory(tempSaveDir);

                string path = ExtraDataPath(_currentSaveName);
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    SnapshotSerializer.Serialize(fs, snapshot);
                }
                _monitor.Log($"额外数据已保存(XML): {snapshot.DebrisItems.Count}掉落物", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn);
            }
        }

        public bool IsWaitingFestivalEnd => _waitingForFestivalEnd;

        public void TickFestivalSaveFlow()
        {
            if (!_waitingForFestivalEnd) return;

            if (_festivalManager.IsFestivalActive)
            {
                if (Game1.multiplayerMode != 0)
                {
                    Game1.multiplayerMode = 0;
                    _monitor.Log("存档时检测到节日活动，切换为单机模式以断开所有玩家...", LogLevel.Info);
                }
                return;
            }

            _waitingForFestivalEnd = false;
            if (_saveRequestedDuringFestival)
            {
                _saveRequestedDuringFestival = false;
                _monitor.Log("节日已结束，开始执行保存...", LogLevel.Info);
                ForceSaveNow();
            }
        }

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

            if (_festivalManager.IsFestivalActive)
            {
                _monitor.Log("存档时检测到节日活动，等待节日结束后再保存...", LogLevel.Info);
                _waitingForFestivalEnd = true;
                _saveRequestedDuringFestival = true;
                return;
            }

            // 修复：不再强制重置游戏保存状态，而是提示用户
            if (SaveGame.IsProcessing)
            {
                _monitor.Log("游戏正在处理其他保存请求，请稍后再试", LogLevel.Warn);
                return;
            }

            try
            {
                SyncOnlinePlayerPositions();

                _monitor.Log($"保存前位置状态: 主机=({Game1.player.position.Value.X}, {Game1.player.position.Value.Y}), " +
                    $"farmhands={Game1.netWorldState.Value.farmhandData.Count()}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"同步在线玩家位置失败: {ex.Message}", LogLevel.Warn);
            }

            try
            {
                RedirectSavesToTemp();

                _pendingSnapshot = CreateSnapshot();
                _saveNeedExtraData = true;

                SaveGame.IsProcessing = true;
                var getSaveEnumerator = AccessTools.Method(typeof(SaveGame), "getSaveEnumerator");
                if (getSaveEnumerator == null)
                {
                    // 尝试列出所有静态方法用于调试
                    var allMethods = typeof(SaveGame).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var methodNames = string.Join(", ", allMethods.Select(m => m.Name));
                    _monitor.Log($"找不到 getSaveEnumerator 方法。可用方法: {methodNames}", LogLevel.Error);
                    SaveGame.IsProcessing = false;
                    _pendingSnapshot = null;
                    return;
                }

                _saveCoroutine = (System.Collections.Generic.IEnumerator<int>)getSaveEnumerator.Invoke(null, null);
                _isSaving = true;
                _monitor.Log($"开始即时保存到TempSaves... (时间: {Game1.currentSeason} {Game1.dayOfMonth}日 {Game1.timeOfDay}:00, 掉落物: {_pendingSnapshot.DebrisItems.Count})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"启动保存失败: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                SaveGame.IsProcessing = false;
                _pendingSnapshot = null;
            }
        }

        public bool IsSavingComplete { get; private set; }

        public void ForceSaveAndQuit()
        {
            if (!Context.IsWorldReady)
            {
                _monitor.Log("世界未加载，直接退出", LogLevel.Info);
                Game1.quit = true;
                return;
            }

            if (_isSaving)
            {
                _monitor.Log("已有保存进行中，等待完成...", LogLevel.Info);
                _quitAfterSave = true;
                return;
            }

            IsSavingComplete = false;
            _quitAfterSave = true;
            ForceSaveNow();

            if (!_isSaving)
            {
                _monitor.Log("保存未启动，直接退出", LogLevel.Info);
                Game1.quit = true;
            }
        }

        private bool _quitAfterSave;

        public void UpdateSave()
        {
            if (!_isSaving || _saveCoroutine == null) return;

            try
            {
                bool moved = _saveCoroutine.MoveNext();
                int progress = moved ? _saveCoroutine.Current : -1;

                if (moved && progress == 100)
                {
                    FinishSave();
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
                    FinishSave();
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

                if (_quitAfterSave)
                {
                    _monitor.Log("保存失败，强制退出", LogLevel.Warn);
                    Game1.quit = true;
                    _quitAfterSave = false;
                }
            }
        }

        private void FinishSave()
        {
            _monitor.Log($"原生保存完成！", LogLevel.Info);
            _isSaving = false;
            _saveCoroutine = null;
            SaveGame.IsProcessing = false;

            IsSavingComplete = true;

            if (_saveNeedExtraData && _pendingSnapshot != null)
            {
                SaveExtraData(_pendingSnapshot);
                _pendingSnapshot = null;
                _saveNeedExtraData = false;
            }

            RedirectSavesToOriginal();

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

            if (_quitAfterSave)
            {
                _monitor.Log("保存完成，正在退出...", LogLevel.Info);
                _quitAfterSave = false;
                Game1.quit = true;
            }
        }

        public void SetCurrentSaveName(string saveName)
        {
            _currentSaveName = saveName;
        }

        public void CreateNewWorld(string saveName, string hostName = null)
        {
            hostName ??= _config.DefaultHostName;

            string targetPath = Path.Combine(CurrentSavesPath, saveName);
            if (Directory.Exists(targetPath))
            {
                _monitor.Log($"存档 {saveName} 已存在", LogLevel.Error);
                return;
            }

            try
            {
                // 使用反射调用游戏内部的新游戏创建流程
                var loadForNewGame = AccessTools.Method(typeof(Game1), "loadForNewGame");
                if (loadForNewGame != null)
                {
                    // 设置存档名
                    Game1.SetSaveName(saveName);

                    // 创建新游戏
                    loadForNewGame.Invoke(Game1.game1, new object[] { false });

                    // 设置主机名
                    if (!string.IsNullOrEmpty(hostName) && Game1.player != null)
                    {
                        Game1.player.Name = hostName;
                    }

                    _currentSaveName = saveName;
                    _monitor.Log($"已创建新存档：{saveName}，主机：{hostName}", LogLevel.Info);
                }
                else
                {
                    _monitor.Log("无法找到新游戏创建方法", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"创建存档失败：{ex.Message}", LogLevel.Error);
            }
        }
    }
}
