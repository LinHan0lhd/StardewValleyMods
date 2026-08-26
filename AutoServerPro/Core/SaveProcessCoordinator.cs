#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using AutoServerPro.Models;
using AutoServerPro.Utils;

namespace AutoServerPro.Core;

public class SaveProcessCoordinator
{
    private readonly IMonitor _monitor;
    private ModConfig _config;
    private readonly SavePathManager _pathManager;
    private readonly FestivalManager _festivalManager;

    private IEnumerator<int> _saveCoroutine;
    private bool _isSaving, _waitingForFestivalEnd, _saveRequestedDuringFestival, _pendingSave, _pendingQuit;
    private int _stopWaitTicks;
    private int _stopTimeoutTicks;
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

    public SaveProcessCoordinator(IMonitor monitor, ModConfig config,
        SavePathManager pathManager, FestivalManager festivalManager)
    {
        _monitor = monitor;
        _config = config;
        _pathManager = pathManager;
        _festivalManager = festivalManager;
    }

    public void UpdateConfig(ModConfig config) => _config = config;

    public void TickFestivalSaveFlow()
    {
        if (_stopWaitTicks > 0)
        {
            if (HasFallingTree())
            {
                _stopTimeoutTicks--;
                if (_stopTimeoutTicks <= 0)
                    _stopWaitTicks = 0;
            }
            else
                _stopWaitTicks--;

            if (_stopWaitTicks == 0)
            {
                if (SaveGame.IsProcessing)
                {
                    _stopWaitTicks = 1;
                    _monitor.Log("原版保存仍在处理中 > 正在等待并重试中", LogLevel.Debug);
                }
                else ForceSaveNow();
            }
        }

        if (!_waitingForFestivalEnd) return;
        if (_festivalManager.IsFestivalActive)
        {
            if (Game1.multiplayerMode != 0) { Game1.multiplayerMode = 0; _monitor.Log("节日中切换单机模式...", LogLevel.Info); }
            return;
        }
        _waitingForFestivalEnd = false;
        if (_saveRequestedDuringFestival) { _saveRequestedDuringFestival = false; ForceSaveNow(); }
    }

    public void ForceSaveNow(bool allowFestivalQueue = true)
    {
        if (!Context.IsWorldReady)
        {
            _monitor.Log("世界未加载", LogLevel.Warn);
            _pendingSave = false;
            return;
        }
        if (_isSaving)
        {
            _monitor.Log("保存中 > 请求已排队...", LogLevel.Info);
            _pendingSave = true;
            return;
        }
        if (_festivalManager.IsFestivalActive)
        {
            if (!allowFestivalQueue)
            {
                _monitor.Log("节日期间无法保存", LogLevel.Warn);
                return;
            }
            _waitingForFestivalEnd = true;
            _saveRequestedDuringFestival = true;
            _monitor.Log("等待节日结束...", LogLevel.Info);
            return;
        }
        if (SaveGame.IsProcessing)
        {
            _monitor.Log("游戏正在保存", LogLevel.Warn);
            return;
        }

        _pendingSave = false;
        StartSaveInternal();
    }

    private void StartSaveInternal()
    {
        try
        {
            if (Game1.IsMasterGame)
            {
                foreach (var kvp in Game1.otherFarmers.Roots)
                {
                    long uid = kvp.Key;
                    NetFarmerRoot farmhandRoot = kvp.Value as NetFarmerRoot;
                    if (farmhandRoot == null) continue;

                    Farmer farmer = farmhandRoot.Value;
                    farmer.disconnectLocation.Value = farmer.currentLocation?.NameOrUniqueName ?? "";
                    farmer.disconnectPosition.Value = farmer.Position;

                    if (Game1.netWorldState.Value.farmhandData.FieldDict.TryGetValue(uid, out NetRef<Farmer> farmhandDataRef))
                    {
                        farmhandRoot.CloneInto(farmhandDataRef);
                    }
                }
            }

            _pathManager.RedirectSavesToTemp();
            _pendingSnapshot = CreateSnapshot();

            SaveGame.IsProcessing = true;
            _saveCoroutine = (IEnumerator<int>)GetSaveEnumeratorMethod.Invoke(null, null);
            _isSaving = true;
            IsSavingComplete = false;
        }
        catch (Exception ex)
        {
            _monitor.Log($"启动保存失败: {ex.Message}", LogLevel.Error);
            CleanupSaveState();
        }
    }

    public void ForceSaveAndQuit()
    {
        if (Game1.multiplayerMode != 0)
        {
            Game1.multiplayerMode = 0;
            _monitor.Log("停止服务器 > 切换为单人模式", LogLevel.Info);
        }
        if (!Context.IsWorldReady) { Game1.quit = true; return; }
        if (_isSaving) { _pendingQuit = true; return; }
        _pendingQuit = true;
        _stopWaitTicks = 3;
        _stopTimeoutTicks = 180;
        _monitor.Log("正在等待掉落物生成完成...", LogLevel.Info);
    }

    private static bool HasFallingTree()
    {
        foreach (var location in Game1.locations)
        {
            if (location?.terrainFeatures == null) continue;
            foreach (var pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is Tree tree && tree.falling.Value)
                    return true;
            }
        }
        return false;
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
        if (!_isSaving && _pendingSave)
        {
            _pendingSave = false;
            StartSaveInternal();
            return;
        }

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
            if (_pendingQuit) RequestQuit();
        }
    }

    private void FinishSave()
    {
        IsSavingComplete = true;

        var snapshot = _pendingSnapshot;
        if (snapshot != null)
        {
            try { SaveExtraData(snapshot); }
            catch (Exception ex) { _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn); }
        }

        CleanupSaveState();
        _pathManager.RedirectSavesToOriginal();

        if (_pendingQuit) RequestQuit();
    }

    private void RequestQuit()
    {
        _pendingQuit = false;
        Game1.paused = false;
        Game1.quit = true;
        _monitor.Log("保存完成 > 解除暂停并退出游戏", LogLevel.Info);
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
                                    X = pos.X,
                                    Y = pos.Y,
                                    RandomOffset = chunk.randomOffset,
                                    XSpriteSheet = chunk.xSpriteSheet.Value,
                                    YSpriteSheet = chunk.ySpriteSheet.Value,
                                    Scale = chunk.scale,
                                    Alpha = chunk.alpha,
                                    Rotation = chunk.rotation,
                                    RotationVelocity = chunk.rotationVelocity,
                                    HitWall = chunk.hitWall,
                                    Bob = chunk.bob,
                                    Bounces = chunk.bounces,
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

        foreach (var npc in Utility.getAllCharacters())
        {
            if (npc.currentLocation == null) continue;
            var tile = npc.Position / 64f;
            snapshot.NpcPositions.Add(new NpcPositionData
            {
                Name = npc.Name,
                MapName = npc.currentLocation.NameOrUniqueName,
                TileX = tile.X,
                TileY = tile.Y,
                Facing = npc.facingDirection.Value
            });
        }

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

            using (var ms = new MemoryStream())
            {
                SaveXmlSerializer.SnapshotSerializer.Serialize(ms, snapshot);
                ms.Position = 0;
                using (var fs = new FileStream(extraPath, FileMode.Create))
                using (var gz = new GZipStream(fs, CompressionLevel.Optimal))
                    ms.CopyTo(gz);
            }

            long size = new FileInfo(extraPath).Length;
            _monitor.Log($"已保存 {snapshot.DebrisItems.Count} 个掉落物 ({size} bytes)", LogLevel.Trace);
        }
        catch (Exception ex) { _monitor.Log($"保存额外数据失败: {ex.Message}", LogLevel.Warn); }
    }

    public void CreateNewWorld(string saveName, string hostName = null)
    {
        hostName ??= _config.DefaultHostName;
        saveName = string.IsNullOrEmpty(saveName) ? _config.DefaultFarmName : saveName;

        if (Directory.Exists(Path.Combine(_pathManager.CurrentSavesPath, saveName)))
        {
            _monitor.Log($"存档已存在", LogLevel.Error);
            return;
        }
        if (LoadForNewGameMethod == null)
        {
            _monitor.Log("找不到 loadForNewGame", LogLevel.Error);
            return;
        }
        try
        {
            Game1.player.team.useSeparateWallets.Value = _config.UseSeparateWallets;
            Game1.whichFarm = Math.Clamp(_config.FarmType, 0, 7);
            Game1.cabinsSeparate = _config.CabinLayoutNearby;
            Game1.bundleType = _config.BundlesRemix ? Game1.BundleType.Remixed : Game1.BundleType.Default;
            Game1.startingGameSeed = _config.RandomSeed;
            Game1.UseLegacyRandom = _config.UseLegacyRandom;
            Game1.startingCabins = Math.Max(0, _config.StartingCabins);

            Game1.game1.SetNewGameOption("YearOneCompletable", _config.CommunityCenterYear1);
            Game1.game1.SetNewGameOption("MineChests", _config.MinesRemix ? Game1.MineChestType.Remixed : Game1.MineChestType.Default);
            if (_config.SpawnMonstersAtNight.HasValue)
                Game1.game1.SetNewGameOption("SpawnMonstersAtNight", _config.SpawnMonstersAtNight.Value);

            Game1.multiplayerMode = 2;

            if (Game1.player != null)
            {
                Game1.player.Name = string.IsNullOrEmpty(hostName) ? _config.DefaultHostName : hostName;
                Game1.player.displayName = Game1.player.Name;
                Game1.player.favoriteThing.Value = "Farming";
                Game1.player.farmName.Value = saveName;
                Game1.player.whichPetBreed = Math.Max(0, _config.PetBreed).ToString();
                Game1.player.difficultyModifier = Math.Clamp(_config.ProfitMargin, 0.25f, 1f);
                Game1.player.isCustomized.Value = true;
                Game1.player.ConvertClothingOverrideToClothesItems();
                Game1.player.caveChoice.Value = _config.CreateMushroomCave ? 1 : 0;
            }

            Game1.SetSaveName(saveName);
            LoadForNewGameMethod.Invoke(Game1.game1, new object[] { false });

            Game1.saveOnNewDay = true;
            Game1.player.eventsSeen.Add("60367");
            Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
            Game1.player.Position = new Vector2(9f, 9f) * 64f;
            Game1.player.isInBed.Value = true;
            Game1.NewDay(0f);
            Game1.exitActiveMenu();
            Game1.setGameMode(3);

            _monitor.Log($"已创建新存档：{saveName}_{Game1.uniqueIDForThisGame}", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"创建失败: {ex.Message}", LogLevel.Error);
        }
    }
}
