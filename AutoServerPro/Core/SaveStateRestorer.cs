#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AutoServerPro.Core
{
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
            if (!File.Exists(extraPath))
            {
                _monitor.Log("无额外存档数据，跳过恢复", LogLevel.Debug);
                return;
            }

            try
            {
                using var fs = new FileStream(extraPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var snapshot = (GameStateSnapshot)SaveXmlSerializer.SnapshotSerializer.Deserialize(fs);
                if (snapshot == null) return;

                _monitor.Log($"加载额外数据 - 时间: {snapshot.TimeOfDay}:00, 掉落物: {snapshot.DebrisItems.Count}", LogLevel.Info);
                RestoreSnapshot(snapshot);
            }
            catch (Exception ex)
            {
                _monitor.Log($"恢复存档数据失败: {ex.Message}", LogLevel.Warn);
            }
        }

        public void RestoreSnapshot(GameStateSnapshot snapshot)
        {
            // NPC 位置已由游戏原生存档通过 DefaultMap/DefaultPosition 恢复
            // 只需冻结防止加载后立即乱跑，延迟后恢复日程
            FreezeAllNpcs();
            SafelySetTime(snapshot.TimeOfDay);
            RestoreMineState(snapshot);
            RestoreDebrisItems(snapshot);
            _monitor.Log("快照恢复完成", LogLevel.Info);
        }



        public void FreezeAllNpcs()
        {
            foreach (var npc in Utility.getAllCharacters())
            {
                if (npc.IsVillager)
                {
                    npc.ignoreScheduleToday = true;
                    npc.controller = null;
                    npc.temporaryController = null;
                }
            }
        }

        public void ResumeAllNpcSchedules()
        {
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

                    var controller = new PathFindController(newPathDesc.route, npc, npc.currentLocation);
                    controller.finalFacingDirection = newPathDesc.facingDirection;

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

        private void RestoreMineState(GameStateSnapshot snapshot)
        {
            if (snapshot.Mine == null) return;
            var mineState = snapshot.Mine;
            var current = Game1.currentLocation as MineShaft;
            if (current == null) { _monitor.Log("跳过矿井恢复：当前非矿井", LogLevel.Debug); return; }
            _monitor.Log($"恢复矿井: Level={mineState.MineLevel}", LogLevel.Debug);

            if (current.mineLevel == mineState.MineLevel)
            {
                current.objects.Clear(); current.terrainFeatures.Clear();
                foreach (var os in mineState.Objects)
                {
                    Item item = null;
                    if (!string.IsNullOrEmpty(os.ItemXml)) try { item = SaveXmlSerializer.DeserializeItem(os.ItemXml); } catch { }
                    if (item == null && !string.IsNullOrEmpty(os.ItemId)) item = ItemRegistry.Create(os.ItemId);
                    if (item is StardewValley.Object obj) { obj.TileLocation = new Vector2(os.TileX, os.TileY); current.objects[obj.TileLocation] = obj; }
                }
            }

            int daysPlayed = (int)Game1.MasterPlayer.stats.DaysPlayed;
            foreach (var fp in mineState.FarmerPositions)
            {
                if (Game1.netWorldState.Value.farmhandData.TryGetValue(fp.FarmerId, out var fh))
                {
                    fh.disconnectDay.Value = daysPlayed;
                    fh.disconnectLocation.Value = current.NameOrUniqueName;
                    fh.disconnectPosition.Value = new Vector2(fp.X, fp.Y);
                }
            }
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
                        debris.floppingFish.Value = (data.Category == -4 && data.InternalName != "Mussel");
                        debris.isFishable = (data.ObjectType == "Fish");
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
            _monitor.Log($"掉落物恢复: 成功{restored}个, 失败{failed}个", LogLevel.Debug);
        }
    }
}
