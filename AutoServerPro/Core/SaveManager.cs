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

    [XmlRoot("DebrisState")]
    public class DebrisState
    {
        [XmlElement("LocationName")]
        public string LocationName { get; set; }

        [XmlElement("ItemId")]
        public string ItemId { get; set; }

        [XmlElement("ItemXml")]
        public string ItemXml { get; set; }

        [XmlElement("Stack")]
        public int Stack { get; set; }

        [XmlElement("Quality")]
        public int Quality { get; set; }

        [XmlElement("DebrisType")]
        public int DebrisType { get; set; }

        [XmlElement("ChunkType")]
        public int ChunkType { get; set; }

        [XmlElement("ChunkFinalYLevel")]
        public int ChunkFinalYLevel { get; set; }

        [XmlElement("FloppingFish")]
        public bool FloppingFish { get; set; }

        [XmlElement("Scale")]
        public float Scale { get; set; }

        [XmlElement("ItemQuality")]
        public int ItemQuality { get; set; }

        // chunksColor 默认是 (0,0,0,0) 透明，必须保存
        [XmlElement("ChunksColorR")]
        public int ChunksColorR { get; set; }

        [XmlElement("ChunksColorG")]
        public int ChunksColorG { get; set; }

        [XmlElement("ChunksColorB")]
        public int ChunksColorB { get; set; }

        [XmlElement("ChunksColorA")]
        public int ChunksColorA { get; set; }

        // nonSpriteChunkColor 默认 White，影响 NUMBERS/SPRITECHUNKS/LETTERS 渲染
        [XmlElement("NonSpriteColorR")]
        public int NonSpriteColorR { get; set; }

        [XmlElement("NonSpriteColorG")]
        public int NonSpriteColorG { get; set; }

        [XmlElement("NonSpriteColorB")]
        public int NonSpriteColorB { get; set; }

        [XmlElement("NonSpriteColorA")]
        public int NonSpriteColorA { get; set; }

        [XmlElement("SpriteChunkSheetName")]
        public string SpriteChunkSheetName { get; set; }

        [XmlElement("SizeOfSourceRectSquares")]
        public int SizeOfSourceRectSquares { get; set; }

        [XmlElement("DebrisMessage")]
        public string DebrisMessage { get; set; }

        [XmlArray("Chunks")]
        [XmlArrayItem("Chunk")]
        public List<ChunkState> Chunks { get; set; } = new();
    }

    public class ChunkState
    {
        [XmlElement("X")]
        public float X { get; set; }

        [XmlElement("Y")]
        public float Y { get; set; }

        [XmlElement("RandomOffset")]
        public int RandomOffset { get; set; }

        [XmlElement("XSpriteSheet")]
        public int XSpriteSheet { get; set; }

        [XmlElement("YSpriteSheet")]
        public int YSpriteSheet { get; set; }

        [XmlElement("Scale")]
        public float Scale { get; set; }

        [XmlElement("Alpha")]
        public float Alpha { get; set; }

        [XmlElement("Rotation")]
        public float Rotation { get; set; }
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
        public List<DebrisState> DebrisItems { get; set; } = new();
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
            int chunkFailed = 0;

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

                    // 1. 反序列化物品
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
                        int stack = debrisState.Stack > 0 ? debrisState.Stack : 1;
                        int quality = debrisState.Quality;
                        item = ItemRegistry.Create(debrisState.ItemId, stack, quality);
                    }

                    if (item == null)
                    {
                        itemFailed++;
                        continue;
                    }

                    // 2. 创建 Debris — 复刻游戏 InitializeItem 逻辑
                    var debris = new Debris();
                    debris.itemId.Value = item.QualifiedItemId;
                    debris.itemQuality = item.Quality;
                    if (debrisState.Stack > 0)
                        item.Stack = debrisState.Stack;

                    // 恢复保存的 Debris 类型
                    debris.debrisType.Value = (Debris.DebrisType)debrisState.DebrisType;

                    // 根据物品类型决定渲染方式（与游戏 InitializeItem 一致）
                    ParsedItemData itemData = ItemRegistry.GetData(item.QualifiedItemId);
                    if (itemData.HasTypeObject())
                    {
                        // Object-type: 不设置 item → 多 chunk 渲染
                        debris.floppingFish.Value = (itemData.Category == -4 && itemData.InternalName != "Mussel");
                        debris.isFishable = (itemData.ObjectType == "Fish");
                        // Arch 类物品覆盖为 ARCHAEOLOGY 类型
                        if (itemData.ObjectType == "Arch")
                            debris.debrisType.Value = Debris.DebrisType.ARCHAEOLOGY;
                    }
                    else
                    {
                        // 非 Object-type: 设置 item → 单 sprite 渲染
                        debris.item = item;
                    }

                    // 3. 恢复 Debris 渲染属性（物理/状态字段使用强制默认值）
                    // 注意：debrisType 已在上方设置（可能被 Arch 覆盖为 ARCHAEOLOGY）
                    // 仅在非 ARCHAEOLOGY 时用保存值覆盖
                    if (debris.debrisType.Value != Debris.DebrisType.ARCHAEOLOGY)
                        debris.debrisType.Value = (Debris.DebrisType)debrisState.DebrisType;
                    debris.chunkType.Value = debrisState.ChunkType;
                    debris.chunkFinalYLevel = debrisState.ChunkFinalYLevel;
                    // floppingFish 已在上方根据物品类型正确设置
                    // （Object-type 鱼类自动 true，其余默认 false）
                    debris.scale.Value = debrisState.Scale;
                    debris.itemQuality = debrisState.ItemQuality;
                    debris.chunksColor.Value = new Color(
                        debrisState.ChunksColorR,
                        debrisState.ChunksColorG,
                        debrisState.ChunksColorB,
                        debrisState.ChunksColorA);
                    debris.nonSpriteChunkColor.Value = new Color(
                        debrisState.NonSpriteColorR,
                        debrisState.NonSpriteColorG,
                        debrisState.NonSpriteColorB,
                        debrisState.NonSpriteColorA);
                    debris.spriteChunkSheetName.Value = debrisState.SpriteChunkSheetName ?? "";
                    debris.sizeOfSourceRectSquares.Value = debrisState.SizeOfSourceRectSquares;
                    debris.debrisMessage.Value = debrisState.DebrisMessage ?? "";
                    // 强制物理/状态为"可拾取"
                    debris.chunksMoveTowardPlayer = true;
                    debris.timeSinceDoneBouncing = 600f;

                    // 4. 重建 Chunk — 恢复渲染属性，强制物理状态
                    if (debrisState.Chunks != null && debrisState.Chunks.Count > 0)
                    {
                        foreach (var chunkState in debrisState.Chunks)
                        {
                            try
                            {
                                var pos = new Vector2(chunkState.X, chunkState.Y);
                                var chunk = new Chunk(pos, 0f, 0f, chunkState.RandomOffset);

                                // 渲染属性
                                chunk.xSpriteSheet.Value = chunkState.XSpriteSheet;
                                chunk.ySpriteSheet.Value = chunkState.YSpriteSheet;
                                chunk.scale = chunkState.Scale;
                                chunk.alpha = chunkState.Alpha;
                                chunk.rotation = chunkState.Rotation;
                                // 强制物理状态为"静止可拾取"
                                chunk.hasPassedRestingLineOnce.Value = true;
                                chunk.bounces = 100;
                                chunk.sinkTimer.Value = int.MaxValue;
                                chunk.position.Field.CancelInterpolation();

                                debris.Chunks.Add(chunk);
                            }
                            catch (Exception ex)
                            {
                                _monitor.Log($"    Chunk恢复失败: {ex.Message}", LogLevel.Trace);
                                chunkFailed++;
                            }
                        }
                    }

                    location.debris.Add(debris);
                    restored++;

                    _monitor.Log($"  恢复 Debris: {item.QualifiedItemId} q{item.Quality} stack={item.Stack} type={debrisState.DebrisType} chunks={debris.Chunks.Count}", LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    _monitor.Log($"  掉落物恢复异常: {ex.Message}", LogLevel.Trace);
                }
            }

            _monitor.Log($"掉落物恢复: 成功{restored}个, 位置未找到{locNotFound}, 物品创建失败{itemFailed}, Chunk恢复失败{chunkFailed}", LogLevel.Debug);
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
                            // Object-type 物品：用 Debris 级别的 itemQuality 修正品质
                            item = ItemRegistry.Create(itemId, 1, debris.itemQuality);
                        }
                        if (item == null) continue;

                        string itemXml = null;
                        try
                        {
                            itemXml = SerializeItem(item);
                        }
                        catch { }

                        int stack = item.Stack > 0 ? item.Stack : 1;
                        int quality = item.Quality;
                        string qualifiedItemId = itemId ?? item.QualifiedItemId;

                        var state = new DebrisState
                        {
                            LocationName = location.NameOrUniqueName,
                            ItemId = qualifiedItemId,
                            ItemXml = itemXml,
                            Stack = stack,
                            Quality = quality,
                            DebrisType = (int)debris.debrisType.Value,
                            ChunkType = debris.chunkType.Value,
                            ChunkFinalYLevel = debris.chunkFinalYLevel,
                            FloppingFish = debris.floppingFish.Value,
                            Scale = debris.scale.Value,
                            ItemQuality = debris.itemQuality,
                            ChunksColorR = debris.chunksColor.Value.R,
                            ChunksColorG = debris.chunksColor.Value.G,
                            ChunksColorB = debris.chunksColor.Value.B,
                            ChunksColorA = debris.chunksColor.Value.A,
                            NonSpriteColorR = debris.nonSpriteChunkColor.Value.R,
                            NonSpriteColorG = debris.nonSpriteChunkColor.Value.G,
                            NonSpriteColorB = debris.nonSpriteChunkColor.Value.B,
                            NonSpriteColorA = debris.nonSpriteChunkColor.Value.A,
                            SpriteChunkSheetName = debris.spriteChunkSheetName.Value ?? "",
                            SizeOfSourceRectSquares = debris.sizeOfSourceRectSquares.Value,
                            DebrisMessage = debris.debrisMessage.Value ?? "",
                        };

                        if (debris.Chunks != null && debris.Chunks.Count > 0)
                        {
                            foreach (var chunk in debris.Chunks)
                            {
                                Vector2 pos = chunk.position.Field.TargetValue;

                                state.Chunks.Add(new ChunkState
                                {
                                    X = pos.X,
                                    Y = pos.Y,
                                    RandomOffset = chunk.randomOffset,
                                    XSpriteSheet = chunk.xSpriteSheet.Value,
                                    YSpriteSheet = chunk.ySpriteSheet.Value,
                                    Scale = chunk.scale,
                                    Alpha = chunk.alpha,
                                    Rotation = chunk.rotation,
                                });
                                chunkCount++;

                                _monitor.Log($"  保存 Chunk: pos=({pos.X:F2},{pos.Y:F2}) random={chunk.randomOffset} scale={chunk.scale:F2} alpha={chunk.alpha:F2} rot={chunk.rotation:F2}", LogLevel.Trace);
                            }
                        }
                        else
                        {
                            state.Chunks.Add(new ChunkState
                            {
                                X = 0,
                                Y = debris.chunkFinalYLevel,
                                RandomOffset = 0,
                                Scale = 1f,
                                Alpha = 1f,
                            });
                            chunkCount++;
                        }

                        snapshot.DebrisItems.Add(state);
                        if (itemXml == null) xmlFallbackCount++;
                        debrisCount++;

                        _monitor.Log($"保存 Debris: {qualifiedItemId} q{quality} stack={stack} type={state.DebrisType} chunks={state.Chunks.Count}", LogLevel.Trace);
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

            return snapshot;
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
