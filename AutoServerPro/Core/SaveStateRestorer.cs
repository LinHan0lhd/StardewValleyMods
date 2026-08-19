#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using AutoServerPro.Models;
using AutoServerPro.Utils;

namespace AutoServerPro.Core;

    public class SaveStateRestorer
{
    private readonly IMonitor _monitor;
    private readonly SavePathManager _pathManager;
    private readonly MethodInfo _getRouteEndBehaviorMethod;

    public SaveStateRestorer(IMonitor monitor, SavePathManager pathManager)
    {
        _monitor = monitor;
        _pathManager = pathManager;
        _getRouteEndBehaviorMethod = typeof(NPC).GetMethod(
            "getRouteEndBehaviorFunction",
            BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public void RestoreExtraDataAfterLoad(string currentSaveName)
    {
        if (string.IsNullOrEmpty(currentSaveName)) return;

        string extraPath = _pathManager.ExtraDataPath(currentSaveName);
        if (!File.Exists(extraPath)) return;

        try
        {
            using var fs = new FileStream(extraPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var gz = new GZipStream(fs, CompressionMode.Decompress);
            var snapshot = (GameStateSnapshot)SaveXmlSerializer.SnapshotSerializer.Deserialize(gz);
            if (snapshot == null) return;
            RestoreSnapshot(snapshot);
        }
        catch (Exception ex)
        {
            _monitor.Log($"恢复存档数据失败: {ex.Message}", LogLevel.Warn);
        }
    }

    public void RestoreSnapshot(GameStateSnapshot snapshot)
    {
        // 用 warpCharacter 强制恢复 NPC 到保存时的精确位置
        RestoreNpcPositions(snapshot.NpcPositions);
        SafelySetTime(snapshot.TimeOfDay);
        RestoreDebrisItems(snapshot);
        _monitor.Log("快照恢复完成", LogLevel.Info);
    }



    public void ResumeAllNpcSchedules()
    {
        SavePatch.SkipSchedule = false;

        foreach (var npc in Utility.getAllCharacters())
        {
            if (!npc.IsVillager) continue;
            if (npc.Schedule == null || npc.Schedule.Count == 0) continue;

            try
            {
                npc.controller = null;
                npc.temporaryController = null;

                var entry = npc.Schedule
                    .Where(kv => kv.Key <= Game1.timeOfDay)
                    .OrderByDescending(kv => kv.Key)
                    .FirstOrDefault();

                if (entry.Key == 0) continue;

                var pathDesc = entry.Value;
                string targetLocationName = pathDesc.targetLocationName;
                Point targetTile = pathDesc.targetTile;
                GameLocation targetLocation = Game1.getLocationFromName(targetLocationName);
                if (targetLocation == null) continue;

                var newPathDesc = npc.pathfindToNextScheduleLocation(
                    "resume",
                    npc.currentLocation.NameOrUniqueName,
                    npc.TilePoint.X,
                    npc.TilePoint.Y,
                    targetLocationName,
                    targetTile.X,
                    targetTile.Y,
                    pathDesc.facingDirection,
                    pathDesc.endOfRouteBehavior,
                    pathDesc.endOfRouteMessage
                );

                if (newPathDesc?.route == null || newPathDesc.route.Count == 0)
                    continue;

                var controller = new PathFindController(newPathDesc.route, npc, npc.currentLocation)
                {
                    finalFacingDirection = newPathDesc.facingDirection
                };

                if (_getRouteEndBehaviorMethod != null)
                {
                    var endBehavior = _getRouteEndBehaviorMethod.Invoke(npc, new object[] { newPathDesc.endOfRouteBehavior, newPathDesc.endOfRouteMessage });
                    if (endBehavior is PathFindController.endBehavior behavior)
                        controller.endBehaviorFunction = behavior;
                }

                npc.controller = controller;
                npc.lastAttemptedSchedule = entry.Key;
                npc.ignoreScheduleToday = false;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"NPC {npc.Name} 恢复日程失败: {ex.Message}", LogLevel.Trace);
            }
        }
    }

    private void SafelySetTime(int targetTime)
    {
        int intervals = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, targetTime) / 10;
        if (intervals > 0)
        {
            for (int i = 0; i < intervals; i++)
                Game1.performTenMinuteClockUpdate();
        }
        else if (intervals < 0)
        {
            for (int i = 0; i > intervals; i--)
            {
                Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, -20);
                Game1.performTenMinuteClockUpdate();
            }
        }

        Game1.outdoorLight = Color.White;
        Game1.ambientLight = Color.White;
        Game1.gameTimeInterval = 0;
        Game1.UpdateGameClock(Game1.currentGameTime);
    }



    private void RestoreDebrisItems(GameStateSnapshot snapshot)
    {
        if (snapshot.DebrisItems == null || snapshot.DebrisItems.Count == 0) return;
        _monitor.Log($"恢复 {snapshot.DebrisItems.Count} 个掉落物", LogLevel.Debug);

        var locs = snapshot.DebrisItems.Select(d => d.LocationName).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        foreach (var ln in locs)
        {
            var loc = Game1.getLocationFromName(ln);
            if (loc != null) loc.debris.Clear();
        }

        int restored = 0, failed = 0;
        foreach (var ds in snapshot.DebrisItems)
        {
            try
            {
                var loc = Game1.getLocationFromName(ds.LocationName);
                if (loc == null) continue;

                Item item = null;
                if (!string.IsNullOrEmpty(ds.ItemXml)) try { item = SaveXmlSerializer.DeserializeItem(ds.ItemXml); } catch { }
                if (item == null && !string.IsNullOrEmpty(ds.ItemId)) item = ItemRegistry.Create(ds.ItemId, ds.Stack > 0 ? ds.Stack : 1, ds.Quality);
                if (item == null) { failed++; continue; }

                var debris = new Debris();
                debris.itemId.Value = item.QualifiedItemId;
                debris.itemQuality = item.Quality;
                if (ds.Stack > 0) item.Stack = ds.Stack;

                debris.InitializeItem(item.QualifiedItemId);
                debris.debrisType.Value = (Debris.DebrisType)ds.DebrisType;

                var data = ItemRegistry.GetData(item.QualifiedItemId);
                if (data.HasTypeObject())
                {
                    debris.floppingFish.Value = data.Category == -4 && data.InternalName != "Mussel";
                    debris.isFishable = data.ObjectType == "Fish";
                    if (data.ObjectType == "Arch") debris.debrisType.Value = Debris.DebrisType.ARCHAEOLOGY;
                }
                else debris.item = item;

                debris.chunkType.Value = ds.ChunkType;
                debris.chunkFinalYLevel = ds.ChunkFinalYLevel;
                debris.scale.Value = ds.Scale;
                debris.chunksMoveTowardPlayer = ds.ChunksMoveTowardPlayer;
                debris.timeSinceDoneBouncing = ds.TimeSinceDoneBouncing;
                debris.isSinking.Value = ds.IsSinking;
                debris.chunksColor.Value = new Color(ds.ChunksColorR, ds.ChunksColorG, ds.ChunksColorB, ds.ChunksColorA);
                debris.nonSpriteChunkColor.Value = new Color(ds.NonSpriteColorR, ds.NonSpriteColorG, ds.NonSpriteColorB, ds.NonSpriteColorA);
                debris.spriteChunkSheetName.Value = ds.SpriteChunkSheetName ?? "";
                debris.sizeOfSourceRectSquares.Value = ds.SizeOfSourceRectSquares;
                debris.debrisMessage.Value = ds.DebrisMessage ?? "";

                debris.Chunks.Clear();
                if (ds.Chunks != null && ds.Chunks.Count > 0)
                {
                    foreach (var cs in ds.Chunks)
                    {
                        var chunk = new Chunk(new Vector2(cs.X, cs.Y), 0f, 0f, cs.RandomOffset);
                        chunk.xSpriteSheet.Value = cs.XSpriteSheet; chunk.ySpriteSheet.Value = cs.YSpriteSheet;
                        chunk.scale = cs.Scale; chunk.alpha = cs.Alpha; chunk.rotation = cs.Rotation;
                        chunk.rotationVelocity = cs.RotationVelocity; chunk.hitWall = cs.HitWall; chunk.bob = cs.Bob; chunk.bounces = cs.Bounces;
                        chunk.hasPassedRestingLineOnce.Value = true; chunk.sinkTimer.Value = int.MaxValue;
                        chunk.position.Field.CancelInterpolation();
                        debris.Chunks.Add(chunk);
                    }
                }
                else
                {
                    debris.Chunks.Add(new Chunk(new Vector2(0, ds.ChunkFinalYLevel), 0f, 0f, 0) { scale = 1f, alpha = 1f, hasPassedRestingLineOnce = { Value = true }, sinkTimer = { Value = int.MaxValue } });
                }

                loc.debris.Add(debris); restored++;
            }
            catch { failed++; }
        }
        _monitor.Log($"掉落物恢复: 成功 {restored} 个, 失败 {failed} 个", LogLevel.Debug);
    }

    /// <summary>
    /// 用 warpCharacter 强制恢复 NPC 到保存时的精确位置
    /// </summary>
    private void RestoreNpcPositions(List<NpcPositionData> positions)
    {
        if (positions == null || positions.Count == 0) return;

        int restored = 0, skipped = 0;

        foreach (var pos in positions)
        {
            try
            {
                var npc = Game1.getCharacterFromName(pos.Name, mustBeVillager: false);
                if (npc == null)
                {
                    _monitor?.Log($"恢复 NPC {pos.Name} 失败：找不到 NPC", LogLevel.Trace);
                    continue;
                }

                var targetLoc = Game1.getLocationFromName(pos.MapName);
                if (targetLoc == null)
                {
                    _monitor?.Log($"恢复 NPC {pos.Name} 失败：地图 {pos.MapName} 不存在", LogLevel.Trace);
                    continue;
                }

                if (npc.currentLocation == targetLoc)
                {
                    var currentTile = npc.Position / 64f;
                    float dx = Math.Abs(currentTile.X - pos.TileX);
                    float dy = Math.Abs(currentTile.Y - pos.TileY);
                    if (dx < 0.02f && dy < 0.02f) // 约 1px 以内
                    {
                        skipped++;
                        continue;
                    }
                }

                Game1.warpCharacter(npc, pos.MapName, new Vector2(pos.TileX, pos.TileY));
                npc.faceDirection(pos.Facing);

                npc.ignoreScheduleToday = true;
                npc.controller = null;
                npc.temporaryController = null;
                restored++;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"恢复 NPC {pos.Name} 失败: {ex.Message}", LogLevel.Warn);
            }
        }

        if (restored > 0 || skipped > 0)
            _monitor?.Log($"NPC 位置恢复: 修正 {restored} 个 跳过 {skipped} 个", LogLevel.Trace);
    }
}