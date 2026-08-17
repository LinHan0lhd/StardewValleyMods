#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using AutoServerPro.Models;
using AutoServerPro.Utils;

namespace AutoServerPro.Core
{
    public class SaveProcessCoordinator
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly ModConfig _config;
        private readonly SavePathManager _pathManager;
        private readonly FestivalManager _festivalManager;

        private IEnumerator<int> _saveCoroutine;
        private bool _isSaving, _waitingForFestivalEnd, _saveRequestedDuringFestival, _quitAfterSave;
        private GameStateSnapshot _pendingSnapshot;

        public bool IsSavingComplete { get; private set; }
        public bool IsSaving => _isSaving;
        public bool IsWaitingFestivalEnd => _waitingForFestivalEnd;

        private static readonly MethodInfo GetSaveEnumeratorMethod;
        private static readonly MethodInfo LoadForNewGameMethod;

        static SaveProcessCoordinator()
        {
            GetSaveEnumeratorMethod = AccessTools.Method(typeof(SaveGame), "getSaveEnumerator");
            LoadForNewGameMethod = AccessTools.Method(typeof(Game1), "loadForNewGame");
        }

        public SaveProcessCoordinator(IMonitor monitor, IModHelper helper, ModConfig config,
            SavePathManager pathManager, FestivalManager festivalManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _pathManager = pathManager;
            _festivalManager = festivalManager;
        }

        public void TickFestivalSaveFlow()
        {
            if (!_waitingForFestivalEnd) return;
            if (_festivalManager.IsFestivalActive)
            {
                if (Game1.multiplayerMode != 0) { Game1.multiplayerMode = 0; _monitor.Log("节日中切换单机模式...", LogLevel.Info); }
                return;
            }
            _waitingForFestivalEnd = false;
            if (_saveRequestedDuringFestival) { _saveRequestedDuringFestival = false; ForceSaveNow(); }
        }

        public void ForceSaveNow()
        {
            if (!Context.IsWorldReady) { _monitor.Log("世界未加载", LogLevel.Warn); return; }
            if (_isSaving) { _monitor.Log("保存中...", LogLevel.Warn); return; }
            if (_festivalManager.IsFestivalActive) { _waitingForFestivalEnd = true; _saveRequestedDuringFestival = true; _monitor.Log("等待节日结束...", LogLevel.Info); return; }
            if (SaveGame.IsProcessing) { _monitor.Log("游戏正在保存", LogLevel.Warn); return; }

            try
            {
                SyncOnlinePlayerPositions();
                _pathManager.RedirectSavesToTemp();
                _pendingSnapshot = CreateSnapshot();

                if (GetSaveEnumeratorMethod == null)
                {
                    _monitor.Log("找不到 getSaveEnumerator", LogLevel.Error);
                    SaveGame.IsProcessing = false;
                    _pendingSnapshot = null;
                    return;
                }

                SaveGame.IsProcessing = true;
                _saveCoroutine = (IEnumerator<int>)GetSaveEnumeratorMethod.Invoke(null, null);
                _isSaving = true;
                IsSavingComplete = false;
                _monitor.Log($"开始保存... (掉落物: {_pendingSnapshot.DebrisItems.Count})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"启动保存失败: {ex.Message}", LogLevel.Error);
                CleanupSaveState();
            }
        }

        public void ForceSaveAndQuit()
        {
            if (!Context.IsWorldReady) { Game1.quit = true; return; }
            if (_isSaving) { _quitAfterSave = true; return; }
            IsSavingComplete = false; _quitAfterSave = true; ForceSaveNow();
            if (!_isSaving) Game1.quit = true;
        }

        private void CleanupSaveState()
        {
            _isSaving = false;
            _saveCoroutine = null;
            SaveGame.IsProcessing = false;
            _pendingSnapshot = null;
        }

        public void UpdateSave()
        {
            if (!_isSaving || _saveCoroutine == null) return;
            try
            {
                bool moved = _saveCoroutine.MoveNext();
                int progress = moved ? _saveCoroutine.Current : -1;
                if ((moved && progress == 100) || !moved) FinishSave();
                else if (progress % 20 == 0) _monitor.Log($"保存进度: {progress}%", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"保存失败: {ex.Message}", LogLevel.Error);
                CleanupSaveState();
                if (_quitAfterSave) { Game1.quit = true; _quitAfterSave = false; }
            }
        }

        private void FinishSave()
        {
            _monitor.Log("原生保存完成", LogLevel.Info);
            IsSavingComplete = true;

            var snapshot = _pendingSnapshot;
            if (snapshot != null)
            {
                try { SaveExtraData(snapshot); }
                catch (Exception ex) { _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn); }
            }

            CleanupSaveState();
            _pathManager.RedirectSavesToOriginal();

            if (!string.IsNullOrEmpty(_pathManager.CurrentSavesPath))
            {
                try
                {
                    string saveName = Constants.SaveFolderName;
                    if (!string.IsNullOrEmpty(saveName))
                    {
                        string backupRoot = _pathManager.BackupRootPath;
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string dest = Path.Combine(backupRoot, saveName, timestamp);
                        CopyDirectory(Path.Combine(_pathManager.CurrentSavesPath, saveName), dest);
                        _monitor.Log($"存档已备份: {dest}", LogLevel.Info);
                    }
                }
                catch (Exception ex) { _monitor.Log($"备份失败: {ex.Message}", LogLevel.Warn); }
            }

            if (_quitAfterSave) { _quitAfterSave = false; Game1.quit = true; }
        }

        private static void SyncOnlinePlayerPositions()
        {
            if (Game1.netWorldState?.Value?.farmhandData == null) return;
            int daysPlayed = (int?)Game1.MasterPlayer?.stats?.DaysPlayed ?? 0;
            foreach (var farmer in Game1.otherFarmers.Values)
            {
                if (farmer == null) continue;
                try
                {
                    if (Game1.netWorldState.Value.farmhandData.TryGetValue(farmer.UniqueMultiplayerID, out var fd))
                    {
                        fd.disconnectPosition.Value = farmer.position.Value;
                        fd.disconnectLocation.Value = farmer.currentLocation?.NameOrUniqueName ?? "";
                        fd.disconnectDay.Value = daysPlayed;
                    }
                }
                catch { }
            }
        }

        private GameStateSnapshot CreateSnapshot()
        {
            var snapshot = new GameStateSnapshot { TimeOfDay = Game1.timeOfDay };
            int debrisCount = 0, chunkCount = 0, failed = 0, xmlFail = 0;

            foreach (var loc in Game1.locations)
            {
                if (loc?.debris == null) continue;
                foreach (var debris in loc.debris)
                {
                    try
                    {
                        Item item = debris.item;
                        string itemId = debris.itemId.Value;
                        if (item == null && !string.IsNullOrEmpty(itemId)) item = ItemRegistry.Create(itemId, 1, debris.itemQuality);
                        if (item == null) continue;

                        string itemXml = null; try { itemXml = SaveXmlSerializer.SerializeItem(item); } catch { }
                        var state = new DebrisState
                        {
                            LocationName = loc.NameOrUniqueName,
                            ItemId = itemId ?? item.QualifiedItemId,
                            ItemXml = itemXml,
                            Stack = item.Stack > 0 ? item.Stack : 1,
                            Quality = item.Quality,
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
                            IsSinking = debris.isSinking.Value,
                            ChunksMoveTowardPlayer = debris.chunksMoveTowardPlayer,
                            TimeSinceDoneBouncing = debris.timeSinceDoneBouncing,
                        };

                        if (debris.Chunks != null && debris.Chunks.Count > 0)
                        {
                            foreach (var chunk in debris.Chunks)
                            {
                                try
                                {
                                    Vector2 pos = chunk.position.Field.TargetValue;
                                    state.Chunks.Add(new ChunkState
                                    {
                                        X = pos.X, Y = pos.Y,
                                        RandomOffset = chunk.randomOffset,
                                        XSpriteSheet = chunk.xSpriteSheet.Value,
                                        YSpriteSheet = chunk.ySpriteSheet.Value,
                                        Scale = chunk.scale, Alpha = chunk.alpha,
                                        Rotation = chunk.rotation,
                                        RotationVelocity = chunk.rotationVelocity,
                                        HitWall = chunk.hitWall,
                                        Bob = chunk.bob, Bounces = chunk.bounces,
                                    });
                                    chunkCount++;
                                }
                                catch { }
                            }
                        }
                        else state.Chunks.Add(new ChunkState { X = 0, Y = debris.chunkFinalYLevel, Scale = 1f, Alpha = 1f });

                        snapshot.DebrisItems.Add(state);
                        if (itemXml == null) xmlFail++; debrisCount++;
                    }
                    catch { failed++; }
                }
            }

            var mine = Game1.currentLocation as MineShaft;
            if (mine != null)
            {
                int? forceLayout = null; try { MineShaft.IsGeneratedLevel(mine, out _, out forceLayout); } catch { }
                snapshot.Mine = new MineState { MineLevel = mine.mineLevel, ForceLayout = forceLayout };
                foreach (var farmer in Game1.getAllFarmers())
                    if (farmer.currentLocation == mine)
                        snapshot.Mine.FarmerPositions.Add(new FarmerPositionState { FarmerId = farmer.UniqueMultiplayerID, X = farmer.Position.X, Y = farmer.Position.Y });

                foreach (var kvp in mine.objects.Pairs.ToList())
                {
                    try
                    {
                        var obj = kvp.Value; if (obj?.ItemId == null) continue;
                        string itemXml = null; try { itemXml = SaveXmlSerializer.SerializeItem(obj); } catch { }
                        snapshot.Mine.Objects.Add(new ObjectState { TileX = kvp.Key.X, TileY = kvp.Key.Y, ItemId = obj.ItemId, ItemXml = itemXml });
                    }
                    catch { }
                }
            }

            // NPC 位置已通过 SavePatch 写入游戏原生存档 (DefaultMap / DefaultPosition)
            // 无需在额外数据中重复记录

            if (xmlFail > 0) _monitor.Log($"XML序列化失败: {xmlFail}个", LogLevel.Warn);
            if (failed > 0) _monitor.Log($"记录失败: {failed}个", LogLevel.Warn);
            return snapshot;
        }

        private void SaveExtraData(GameStateSnapshot snapshot)
        {
            string saveName = Constants.SaveFolderName;
            if (string.IsNullOrEmpty(saveName)) return;
            try
            {
                string dir = Path.Combine(_pathManager.CurrentSavesPath, saveName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string extraPath = _pathManager.ExtraDataPath(saveName);
                using (var fs = new FileStream(extraPath, FileMode.Create))
                    SaveXmlSerializer.SnapshotSerializer.Serialize(fs, snapshot);
                _monitor.Log($"额外数据已保存: {snapshot.DebrisItems.Count}掉落物", LogLevel.Info);
            }
            catch (Exception ex) { _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn); }
        }

        public void CreateNewWorld(string saveName, string hostName = null)
        {
            hostName ??= _config.DefaultHostName;
            if (Directory.Exists(Path.Combine(_pathManager.CurrentSavesPath, saveName))) { _monitor.Log($"存档已存在", LogLevel.Error); return; }
            if (LoadForNewGameMethod == null) { _monitor.Log("找不到 loadForNewGame", LogLevel.Error); return; }
            try
            {
                Game1.SetSaveName(saveName);
                LoadForNewGameMethod.Invoke(Game1.game1, new object[] { false });
                if (!string.IsNullOrEmpty(hostName) && Game1.player != null) Game1.player.Name = hostName;
                _monitor.Log($"已创建新存档：{saveName}", LogLevel.Info);
            }
            catch (Exception ex) { _monitor.Log($"创建失败: {ex.Message}", LogLevel.Error); }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }
}
