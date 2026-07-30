using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter
{
    public static class TileSheetMerger
    {
        public const string DefaultHostAssetName = "Maps/busPeople";

        /// <summary>
        /// 判断一个 TileSheet 是否为虚拟贴图（由 SMAPI 动态生成，无实体文件）。
        /// </summary>
        public static bool IsVirtualTileSheet(TileSheet tileSheet)
        {
            if (tileSheet == null) return false;
            string src = tileSheet.ImageSource?.Replace('\\', '/');
            if (string.IsNullOrEmpty(src)) return false;

            return src.StartsWith("SMAPI/", StringComparison.OrdinalIgnoreCase)
                || src.IndexOf("/Mods/", StringComparison.OrdinalIgnoreCase) >= 0
                || src.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 核心方法：合并地图中所有虚拟瓦片表到宿主贴图，并更新地图瓦片索引。
        /// </summary>
        public static Texture2D MergeVirtualTileSheets(
            Map map,
            string hostAssetName,
            IModHelper helper,
            IMonitor monitor,
            string troubleshootDir = null)
        {
            if (map == null || helper == null) return null;

            var virtualSheets = map.TileSheets.Where(IsVirtualTileSheet).ToList();
            if (virtualSheets.Count == 0) return null;

            monitor?.Log($"  ↳ 发现 {virtualSheets.Count} 个虚拟贴图", LogLevel.Trace);

            // ---- 宿主 TileSheet 查找 ----
            string hostId = hostAssetName.Replace('\\', '/');
            int lastSlash = hostId.LastIndexOf('/');
            if (lastSlash >= 0) hostId = hostId.Substring(lastSlash + 1);

            TileSheet hostSheet = map.TileSheets.FirstOrDefault(ts =>
                ts.ImageSource?.Replace('\\', '/').Equals(hostAssetName, StringComparison.OrdinalIgnoreCase) == true
                || ts.Id.Equals(hostId, StringComparison.OrdinalIgnoreCase));

            // ---- 加载宿主纹理 ----
            Texture2D hostTexture;
            try { hostTexture = helper.GameContent.Load<Texture2D>(hostAssetName); }
            catch (Exception ex)
            {
                monitor?.Log($"  ↳ 无法加载宿主贴图 {hostAssetName}: {ex.Message}", LogLevel.Trace);
                return null;
            }

            int hostPixelW = hostTexture.Width;
            int hostPixelH = hostTexture.Height;
            // 强制 16×16，与 TBinWriter 写入的 layer tile size 一致
            // （若用 hostSheet?.TileWidth ?? 64，当地图无 busPeople tilesheet 时会得到 64，与 layer 16×16 冲突导致错位）
            int hostTileW = 16;
            int hostTileH = 16;

            monitor?.Log($"  ↳ 宿主: {hostPixelW}x{hostPixelH}px, Tile={hostTileW}x{hostTileH}", LogLevel.Trace);

            // ---- 加载每个虚拟贴图并修正尺寸 ----
            var virtualDataList = new List<VirtualTileData>();
            foreach (var vSheet in virtualSheets)
            {
                Texture2D vTex;
                try { vTex = helper.GameContent.Load<Texture2D>(vSheet.ImageSource); }
                catch (Exception ex)
                {
                    monitor?.Log($"  ↳ 无法加载虚拟贴图 {vSheet.ImageSource}: {ex.Message}", LogLevel.Trace);
                    return null;
                }

                // 修正瓦片尺寸（xTile 反序列化时常错误设为 64）
                if (vSheet.SheetWidth > 0 && vSheet.SheetHeight > 0)
                {
                    int calcW = vTex.Width / vSheet.SheetWidth;
                    int calcH = vTex.Height / vSheet.SheetHeight;
                    if (calcW > 0 && calcH > 0 &&
                        (vSheet.TileWidth != calcW || vSheet.TileHeight != calcH))
                    {
                        monitor?.Log(
                            $"  ⚠ 修正虚拟贴图 [{vSheet.Id}] 的瓦片尺寸: " +
                            $"{vSheet.TileWidth}x{vSheet.TileHeight} → {calcW}x{calcH}",
                            LogLevel.Warn);
                        vSheet.TileWidth = calcW;
                        vSheet.TileHeight = calcH;
                    }
                }

                var pixels = new Color[vTex.Width * vTex.Height];
                vTex.GetData(pixels);

                var data = new VirtualTileData
                {
                    Sheet = vSheet,
                    Texture = vTex,
                    Pixels = pixels,
                    PixelW = vTex.Width,
                    PixelH = vTex.Height,
                    TileW = vSheet.TileWidth,
                    TileH = vSheet.TileHeight,
                    OldSheetW = vSheet.SheetWidth,
                    OldSheetH = vSheet.SheetHeight,
                    Id = vSheet.Id
                };

                data.SameSizeAsHost = (data.TileW == hostTileW && data.TileH == hostTileH);
                monitor?.Log($"  ↳ 虚拟 {data.Id}: {data.PixelW}x{data.PixelH}px, Tile={data.TileW}x{data.TileH}, Sheet={data.OldSheetW}x{data.OldSheetH}, SameSize={data.SameSizeAsHost}", LogLevel.Trace);

                virtualDataList.Add(data);
            }

            // ---- 收集被使用的瓦片坐标 ----
            var usedCoordsMap = new Dictionary<VirtualTileData, HashSet<(int x, int y)>>();
            foreach (var vd in virtualDataList)
                usedCoordsMap[vd] = new HashSet<(int, int)>();

            foreach (Layer layer in map.Layers)
                for (int y = 0; y < layer.LayerHeight; y++)
                    for (int x = 0; x < layer.LayerWidth; x++)
                        CollectUsedCoords(layer.Tiles[x, y], usedCoordsMap);

            // ---- 筛选、裁剪、去重、去透明 ----
            var activeVirtualData = new List<VirtualTileData>();
            foreach (var vd in virtualDataList)
            {
                var coords = usedCoordsMap[vd];
                if (coords.Count == 0)
                {
                    monitor?.Log($"  ↳ 虚拟 {vd.Id} 未被任何 tile 引用，跳过", LogLevel.Trace);
                    continue;
                }

                var clampedCoords = new HashSet<(int x, int y)>();
                int maxX = vd.PixelW / vd.TileW;
                int maxY = vd.PixelH / vd.TileH;

                foreach (var (cx, cy) in coords)
                {
                    int ccx = Math.Max(0, Math.Min(cx, maxX - 1));
                    int ccy = Math.Max(0, Math.Min(cy, maxY - 1));
                    clampedCoords.Add((ccx, ccy));
                }

                if (clampedCoords.Count == 0)
                {
                    monitor?.Log($"  ↳ 虚拟 {vd.Id} Clamp 后无有效坐标，跳过", LogLevel.Trace);
                    continue;
                }

                // 去重 + 去透明
                var uniqueNonTransparent = new Dictionary<string, (int x, int y)>();
                var dedupMap = new Dictionary<(int, int), (int x, int y)>();
                int transparentCount = 0;

                foreach (var coord in clampedCoords)
                {
                    if (IsTileFullyTransparent(vd, coord.x, coord.y))
                    {
                        dedupMap[coord] = (-1, -1); // 透明瓦片特殊标记
                        transparentCount++;
                        continue;
                    }

                    byte[] hash = GetTilePixelHash(vd, coord.x, coord.y);
                    string hashKey = Convert.ToBase64String(hash);

                    if (uniqueNonTransparent.TryGetValue(hashKey, out var rep))
                        dedupMap[coord] = rep;
                    else
                    {
                        uniqueNonTransparent[hashKey] = coord;
                        dedupMap[coord] = coord;
                    }
                }

                vd.ClampedCoords = new HashSet<(int x, int y)>(uniqueNonTransparent.Values);
                vd.DedupMap = dedupMap;

                if (vd.ClampedCoords.Count == 0)
                {
                    monitor?.Log($"  ↳ 虚拟 {vd.Id} 所有瓦片均为透明或重复，跳过", LogLevel.Trace);
                    continue;
                }

                // 旧包围盒（仅用于日志）
                vd.MinX = vd.ClampedCoords.Min(c => c.x);
                vd.MinY = vd.ClampedCoords.Min(c => c.y);
                vd.MaxX = vd.ClampedCoords.Max(c => c.x);
                vd.MaxY = vd.ClampedCoords.Max(c => c.y);
                vd.BoundsW = (vd.MaxX - vd.MinX + 1) * vd.TileW;
                vd.BoundsH = (vd.MaxY - vd.MinY + 1) * vd.TileH;

                int dupCount = clampedCoords.Count - uniqueNonTransparent.Count - transparentCount;
                monitor?.Log($"  ↳ 虚拟 {vd.Id}: 原始 {clampedCoords.Count} 个, 透明 {transparentCount} 个, 重复 {dupCount} 个, 最终唯一瓦片 {vd.ClampedCoords.Count} 个", LogLevel.Trace);

                activeVirtualData.Add(vd);
            }

            if (activeVirtualData.Count == 0)
            {
                monitor?.Log($"  ↳ 没有需要合并的虚拟贴图", LogLevel.Trace);
                return null;
            }

            // ---- 排序（面积大者优先） ----
            activeVirtualData.Sort((a, b) =>
                (b.BoundsW * b.BoundsH).CompareTo(a.BoundsW * a.BoundsH));

            // ---- 初步计算实际排列高度（用于后续偏移） ----
            foreach (var vd in activeVirtualData)
            {
                int tilesPerRow = Math.Max(1, hostPixelW / vd.TileW);
                int rows = (vd.ClampedCoords.Count + tilesPerRow - 1) / tilesPerRow;
                vd.ActualRows = rows;
                vd.ActualBoundsH = rows * vd.TileH;
            }

            // ---- 计算合并贴图宽度并确定最终高度 ----
            int mergedW = hostPixelW;
            int mergedH = hostPixelH;

            foreach (var vd in activeVirtualData)
            {
                vd.OffsetX = 0;
                vd.OffsetY = mergedH;
                int rem = vd.OffsetY % vd.TileH;
                if (rem != 0) vd.OffsetY += vd.TileH - rem;
                mergedH = vd.OffsetY + vd.ActualBoundsH;
                if (vd.BoundsW > mergedW) mergedW = vd.BoundsW;
            }

            int lcm = hostTileW;
            foreach (var vd in activeVirtualData)
                lcm = Lcm(lcm, vd.TileW);
            mergedW = ((mergedW + lcm - 1) / lcm) * lcm;

            // 宽度扩大后重新计算每个虚拟贴图的行数和实际高度
            mergedH = hostPixelH;
            foreach (var vd in activeVirtualData)
            {
                int tilesPerRow = Math.Max(1, mergedW / vd.TileW);
                int rows = (vd.ClampedCoords.Count + tilesPerRow - 1) / tilesPerRow;
                vd.ActualRows = rows;
                vd.ActualBoundsH = rows * vd.TileH;

                vd.OffsetY = mergedH;
                int rem = vd.OffsetY % vd.TileH;
                if (rem != 0) vd.OffsetY += vd.TileH - rem;
                mergedH = vd.OffsetY + vd.ActualBoundsH;
            }

            monitor?.Log($"  ↳ 合并贴图: {mergedW}x{mergedH}px", LogLevel.Trace);

            // ---- 生成 TileMap（新坐标映射） ----
            foreach (var vd in activeVirtualData)
            {
                vd.TileMap = new Dictionary<(int oldX, int oldY), (int newX, int newY)>();

                int tilesPerRow = Math.Max(1, mergedW / vd.TileW);
                int col = 0, row = 0;

                var uniqueToNew = new Dictionary<(int, int), (int newX, int newY)>();
                foreach (var uniqueCoord in vd.ClampedCoords)
                {
                    var newCoord = (col, row);
                    uniqueToNew[uniqueCoord] = newCoord;
                    col++;
                    if (col >= tilesPerRow) { col = 0; row++; }
                }

                foreach (var kv in vd.DedupMap)
                {
                    if (kv.Value == (-1, -1)) continue; // 透明瓦片不映射
                    vd.TileMap[kv.Key] = uniqueToNew[kv.Value];
                }

                vd.NewSheetW = mergedW / vd.TileW;
                vd.NewSheetH = mergedH / vd.TileH;

                monitor?.Log($"  ↳ 虚拟 {vd.Id} 映射: {vd.TileMap.Count} 个 tile (有效), Sheet={vd.NewSheetW}x{vd.NewSheetH}, Offset=({vd.OffsetX},{vd.OffsetY})", LogLevel.Trace);
            }

            // ---- 创建合并纹理并拷贝像素 ----
            var device = hostTexture.GraphicsDevice;
            var mergedTex = new Texture2D(device, mergedW, mergedH);
            var mergedPixels = new Color[mergedW * mergedH];
            Array.Clear(mergedPixels, 0, mergedPixels.Length);

            var hostPixels = new Color[hostPixelW * hostPixelH];
            hostTexture.GetData(hostPixels);
            CopyPixels(hostPixels, hostPixelW, hostPixelH, 0, 0, hostPixelW, hostPixelH,
                       mergedPixels, mergedW, mergedH, 0, 0);

            foreach (var vd in activeVirtualData)
            {
                var copiedReps = new HashSet<(int x, int y)>();
                foreach (var kv in vd.TileMap)
                {
                    var oldCoord = kv.Key;
                    var newCoord = kv.Value;

                    (int repX, int repY) rep = vd.DedupMap[oldCoord];
                    if (rep == (-1, -1)) continue;

                    if (copiedReps.Contains(rep))
                        continue;
                    copiedReps.Add(rep);

                    int srcX = rep.repX * vd.TileW;
                    int srcY = rep.repY * vd.TileH;
                    int dstX = vd.OffsetX + newCoord.newX * vd.TileW;
                    int dstY = vd.OffsetY + newCoord.newY * vd.TileH;

                    CopyPixels(vd.Pixels, vd.PixelW, vd.PixelH,
                               srcX, srcY, vd.TileW, vd.TileH,
                               mergedPixels, mergedW, mergedH,
                               dstX, dstY);
                }
            }

            mergedTex.SetData(mergedPixels);

            // ---- 更新宿主 TileSheet ----
            int newHostSheetW = mergedW / hostTileW;
            int newHostSheetH = mergedH / hostTileH;
            int oldHostSheetW = hostSheet?.SheetWidth ?? newHostSheetW;

            if (hostSheet == null)
            {
                hostSheet = new TileSheet(hostId, map, hostAssetName,
                    new Size { Width = newHostSheetW, Height = newHostSheetH },
                    new Size { Width = hostTileW, Height = hostTileH });
                map.AddTileSheet(hostSheet);
            }
            else
            {
                hostSheet.ImageSource = hostAssetName;
                hostSheet.SheetWidth = newHostSheetW;
                hostSheet.SheetHeight = newHostSheetH;
                hostSheet.TileWidth = hostTileW;
                hostSheet.TileHeight = hostTileH;
            }

            monitor?.Log($"  ↳ 宿主 TileSheet: {newHostSheetW}x{newHostSheetH} tiles, Tile={hostTileW}x{hostTileH}", LogLevel.Trace);

            // ---- 更新所有虚拟 TileSheet 的属性 ----
            foreach (var vd in activeVirtualData)
            {
                vd.Sheet.ImageSource = hostAssetName;
                vd.Sheet.SheetWidth = vd.NewSheetW;
                vd.Sheet.SheetHeight = vd.NewSheetH;
                monitor?.Log($"  ↳ 虚拟 {vd.Id} 已更新: ImageSource={hostAssetName}, Tile={vd.Sheet.TileWidth}x{vd.Sheet.TileHeight}, Sheet={vd.NewSheetW}x{vd.NewSheetH}", LogLevel.Trace);
            }

            // ---- 重写地图瓦片索引 ----
            var lookupDict = new Dictionary<string, VirtualTileData>(StringComparer.OrdinalIgnoreCase);
            foreach (var vd in activeVirtualData)
                if (!string.IsNullOrEmpty(vd.Id))
                    lookupDict[vd.Id] = vd;

            foreach (Layer layer in map.Layers)
                for (int y = 0; y < layer.LayerHeight; y++)
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile is StaticTile st)
                            RewriteStaticTile(layer, x, y, st, hostSheet, lookupDict, oldHostSheetW, newHostSheetW, hostTileW, hostTileH, monitor);
                        else if (tile is AnimatedTile anim)
                            RewriteAnimatedTile(layer, x, y, anim, hostSheet, lookupDict, oldHostSheetW, newHostSheetW, hostTileW, hostTileH, monitor);
                    }

            // ---- 移除完全重定向到宿主的虚拟 TileSheet ----
            foreach (var vd in activeVirtualData)
            {
                if (vd.SameSizeAsHost)
                {
                    try
                    {
                        bool stillReferenced = false;
                        foreach (Layer layer in map.Layers)
                            for (int y = 0; y < layer.LayerHeight && !stillReferenced; y++)
                                for (int x = 0; x < layer.LayerWidth && !stillReferenced; x++)
                                {
                                    var tile = layer.Tiles[x, y];
                                    if (tile != null && tile.TileSheet == vd.Sheet)
                                        stillReferenced = true;
                                }

                        if (!stillReferenced)
                        {
                            map.RemoveTileSheet(vd.Sheet);
                            monitor?.Log($"  ↳ 移除冗余虚拟 TileSheet: {vd.Id}", LogLevel.Trace);
                        }
                    }
                    catch (Exception ex)
                    {
                        monitor?.Log($"  ↳ 无法移除虚拟 TileSheet {vd.Id}: {ex.Message}", LogLevel.Trace);
                    }
                }
            }

            // ---- 生成综合排查图 ----
            if (!string.IsNullOrEmpty(troubleshootDir))
                GenerateCombinedTroubleshootImage(mergedTex, activeVirtualData, hostPixelH, hostTileW, hostTileH, troubleshootDir, monitor);

            monitor?.Log($"  ↳ 合并完成: {mergedW}x{mergedH}px", LogLevel.Trace);
            return mergedTex;
        }

        private static bool IsTileFullyTransparent(VirtualTileData vd, int tileX, int tileY)
        {
            for (int y = 0; y < vd.TileH; y++)
                for (int x = 0; x < vd.TileW; x++)
                {
                    int idx = (tileY * vd.TileH + y) * vd.PixelW + (tileX * vd.TileW + x);
                    if (vd.Pixels[idx].A != 0) return false;
                }
            return true;
        }

        private static byte[] GetTilePixelHash(VirtualTileData vd, int tileX, int tileY)
        {
            int tileW = vd.TileW;
            int tileH = vd.TileH;
            int stride = tileW * 4;
            byte[] raw = new byte[tileH * stride];

            int srcStartX = tileX * tileW;
            int srcStartY = tileY * tileH;

            for (int y = 0; y < tileH; y++)
                for (int x = 0; x < tileW; x++)
                {
                    int srcIdx = (srcStartY + y) * vd.PixelW + (srcStartX + x);
                    Color c = vd.Pixels[srcIdx];
                    int baseIdx = y * stride + x * 4;
                    raw[baseIdx] = c.R;
                    raw[baseIdx + 1] = c.G;
                    raw[baseIdx + 2] = c.B;
                    raw[baseIdx + 3] = c.A;
                }

            using var md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(raw);
        }

        private static void CollectUsedCoords(Tile tile, Dictionary<VirtualTileData, HashSet<(int x, int y)>> usedCoordsMap)
        {
            if (tile == null) return;

            if (tile is StaticTile st)
            {
                foreach (var kv in usedCoordsMap)
                    if (st.TileSheet == kv.Key.Sheet)
                    {
                        int ox = st.TileIndex % kv.Key.OldSheetW;
                        int oy = st.TileIndex / kv.Key.OldSheetW;
                        kv.Value.Add((ox, oy));
                        break;
                    }
            }
            else if (tile is AnimatedTile anim)
            {
                foreach (var frame in anim.TileFrames)
                {
                    if (frame == null) continue;
                    foreach (var kv in usedCoordsMap)
                        if (frame.TileSheet == kv.Key.Sheet)
                        {
                            int ox = frame.TileIndex % kv.Key.OldSheetW;
                            int oy = frame.TileIndex / kv.Key.OldSheetW;
                            kv.Value.Add((ox, oy));
                            break;
                        }
                }
            }
        }

        private static void RewriteStaticTile(
            Layer layer, int x, int y,
            StaticTile st,
            TileSheet hostSheet,
            Dictionary<string, VirtualTileData> lookupDict,
            int oldHostSheetW, int newHostSheetW,
            int hostTileW, int hostTileH,
            IMonitor monitor)
        {
            if (st.TileSheet == hostSheet)
            {
                if (oldHostSheetW != newHostSheetW)
                {
                    int hostOldX = st.TileIndex % oldHostSheetW;
                    int hostOldY = st.TileIndex / oldHostSheetW;
                    int hostNewIdx = hostOldY * newHostSheetW + hostOldX;
                    var newTile = new StaticTile(layer, hostSheet, st.BlendMode, hostNewIdx);
                    CopyProperties(st.Properties, newTile.Properties);
                    layer.Tiles[x, y] = newTile;
                }
                return;
            }

            string sheetId = st.TileSheet.Id;
            if (string.IsNullOrEmpty(sheetId)) return;

            if (!lookupDict.TryGetValue(sheetId, out var vd)) return;

            int vOldX = st.TileIndex % vd.OldSheetW;
            int vOldY = st.TileIndex / vd.OldSheetW;

            if (!vd.TileMap.TryGetValue((vOldX, vOldY), out var newPos))
            {
                layer.Tiles[x, y] = null;
                monitor?.Log($"  ↳ Tile ({x},{y}) 旧坐标 ({vOldX},{vOldY}) 无内容，已置空", LogLevel.Trace);
                return;
            }

            if (vd.SameSizeAsHost)
            {
                int pixelX = newPos.newX * vd.TileW;
                int pixelY = vd.OffsetY + newPos.newY * vd.TileH;
                int newX = pixelX / hostTileW;
                int newY = pixelY / hostTileH;
                int newIdx = newY * newHostSheetW + newX;

                var newTile = new StaticTile(layer, hostSheet, st.BlendMode, newIdx);
                CopyProperties(st.Properties, newTile.Properties);
                layer.Tiles[x, y] = newTile;
            }
            else
            {
                int newX = newPos.newX;
                int newY = vd.OffsetY / vd.TileH + newPos.newY;
                int newIdx = newY * vd.NewSheetW + newX;

                var newTile = new StaticTile(layer, vd.Sheet, st.BlendMode, newIdx);
                CopyProperties(st.Properties, newTile.Properties);
                layer.Tiles[x, y] = newTile;
            }
        }

        private static void RewriteAnimatedTile(
            Layer layer, int x, int y,
            AnimatedTile anim,
            TileSheet hostSheet,
            Dictionary<string, VirtualTileData> lookupDict,
            int oldHostSheetW, int newHostSheetW,
            int hostTileW, int hostTileH,
            IMonitor monitor)
        {
            var newFrames = new StaticTile[anim.TileFrames.Length];
            bool anyRewritten = false;

            for (int i = 0; i < anim.TileFrames.Length; i++)
            {
                var frame = anim.TileFrames[i];
                if (frame.TileSheet == hostSheet)
                {
                    if (oldHostSheetW != newHostSheetW)
                    {
                        int hostOldX = frame.TileIndex % oldHostSheetW;
                        int hostOldY = frame.TileIndex / oldHostSheetW;
                        int hostNewIdx = hostOldY * newHostSheetW + hostOldX;
                        newFrames[i] = new StaticTile(layer, hostSheet, frame.BlendMode, hostNewIdx);
                        CopyProperties(frame.Properties, newFrames[i].Properties);
                        anyRewritten = true;
                    }
                    else newFrames[i] = frame;
                    continue;
                }

                string sheetId = frame.TileSheet.Id;
                if (string.IsNullOrEmpty(sheetId))
                {
                    newFrames[i] = frame;
                    continue;
                }

                if (!lookupDict.TryGetValue(sheetId, out var vd))
                {
                    newFrames[i] = frame;
                    continue;
                }

                int vOldX = frame.TileIndex % vd.OldSheetW;
                int vOldY = frame.TileIndex / vd.OldSheetW;

                if (!vd.TileMap.TryGetValue((vOldX, vOldY), out var newPos))
                {
                    newFrames[i] = null;
                    anyRewritten = true;
                    continue;
                }

                if (vd.SameSizeAsHost)
                {
                    int pixelX = newPos.newX * vd.TileW;
                    int pixelY = vd.OffsetY + newPos.newY * vd.TileH;
                    int newX = pixelX / hostTileW;
                    int newY = pixelY / hostTileH;
                    int hostNewIdx = newY * newHostSheetW + newX;
                    newFrames[i] = new StaticTile(layer, hostSheet, frame.BlendMode, hostNewIdx);
                    CopyProperties(frame.Properties, newFrames[i].Properties);
                    anyRewritten = true;
                }
                else
                {
                    int newX = newPos.newX;
                    int newY = vd.OffsetY / vd.TileH + newPos.newY;
                    int vNewIdx = newY * vd.NewSheetW + newX;
                    newFrames[i] = new StaticTile(layer, vd.Sheet, frame.BlendMode, vNewIdx);
                    CopyProperties(frame.Properties, newFrames[i].Properties);
                    anyRewritten = true;
                }
            }

            if (anyRewritten)
            {
                var validFrames = newFrames.Where(f => f != null).ToArray();
                if (validFrames.Length == 0)
                    layer.Tiles[x, y] = null;
                else if (validFrames.Length == 1)
                    layer.Tiles[x, y] = validFrames[0];
                else
                {
                    var newAnim = new AnimatedTile(layer, validFrames, anim.FrameInterval);
                    newAnim.BlendMode = anim.BlendMode;
                    CopyProperties(anim.Properties, newAnim.Properties);
                    layer.Tiles[x, y] = newAnim;
                }
            }
        }

        private static void GenerateCombinedTroubleshootImage(
            Texture2D mergedTex,
            List<VirtualTileData> activeVirtualData,
            int hostPixelH,
            int hostTileW,
            int hostTileH,
            string troubleshootDir,
            IMonitor monitor)
        {
            int w = mergedTex.Width;
            int h = mergedTex.Height;
            var pixels = new Color[w * h];
            mergedTex.GetData(pixels);

            // 宿主网格（灰色）
            Color hostGridColor = new Color(128, 128, 128, 100);
            for (int gy = 0; gy < hostPixelH; gy += hostTileH)
                for (int px = 0; px < w; px++)
                    if (gy * w + px < pixels.Length) pixels[gy * w + px] = BlendOver(pixels[gy * w + px], hostGridColor);
            for (int gx = 0; gx < w; gx += hostTileW)
                for (int py = 0; py < hostPixelH; py++)
                    if (py * w + gx < pixels.Length) pixels[py * w + gx] = BlendOver(pixels[py * w + gx], hostGridColor);

            // 分隔线（绿色）
            Color sepColor = new Color(0, 255, 0, 200);
            for (int px = 0; px < w; px++)
            {
                int idx = hostPixelH * w + px;
                if (idx < pixels.Length) pixels[idx] = sepColor;
            }

            // 各虚拟贴图
            foreach (var vd in activeVirtualData)
            {
                var occupiedCells = new HashSet<(int newX, int newY)>();
                foreach (var kv in vd.TileMap)
                    occupiedCells.Add(kv.Value);

                int boxX = vd.OffsetX;
                int boxY = vd.OffsetY;
                int boxW = w - boxX;
                int boxH = vd.ActualBoundsH;

                Color boxColor = new Color(255, 0, 0, 220);
                for (int px = boxX; px < boxX + boxW && px < w; px++)
                {
                    if (boxY >= 0 && boxY < h) pixels[boxY * w + px] = boxColor;
                    if (boxY + boxH - 1 >= 0 && boxY + boxH - 1 < h) pixels[(boxY + boxH - 1) * w + px] = boxColor;
                }
                for (int py = boxY; py < boxY + boxH && py < h; py++)
                {
                    if (boxX >= 0 && boxX < w) pixels[py * w + boxX] = boxColor;
                    if (boxX + boxW - 1 >= 0 && boxX + boxW - 1 < w) pixels[py * w + (boxX + boxW - 1)] = boxColor;
                }

                Color gridColor = new Color(0, 100, 255, 80);
                foreach (var cell in occupiedCells)
                {
                    int cellLeft = vd.OffsetX + cell.newX * vd.TileW;
                    int cellTop = vd.OffsetY + cell.newY * vd.TileH;

                    int lineY = cellTop + vd.TileH - 1;
                    if (lineY < h)
                        for (int px = cellLeft; px < cellLeft + vd.TileW && px < w; px++)
                            pixels[lineY * w + px] = BlendOver(pixels[lineY * w + px], gridColor);

                    int lineX = cellLeft + vd.TileW - 1;
                    if (lineX < w)
                        for (int py = cellTop; py < cellTop + vd.TileH && py < h; py++)
                            pixels[py * w + lineX] = BlendOver(pixels[py * w + lineX], gridColor);
                }

                Color dotColor = new Color(255, 255, 0, 180);
                foreach (var cell in occupiedCells)
                {
                    int dotX = vd.OffsetX + cell.newX * vd.TileW + vd.TileW / 2;
                    int dotY = vd.OffsetY + cell.newY * vd.TileH + vd.TileH / 2;
                    if (dotX < w && dotY < h)
                        pixels[dotY * w + dotX] = dotColor;
                }
            }

            string debugPath = Path.Combine(troubleshootDir, "merged_debug.png");
            using var debugTex = new Texture2D(mergedTex.GraphicsDevice, w, h);
            debugTex.SetData(pixels);
            using (var fs = new FileStream(debugPath, FileMode.Create, FileAccess.Write))
                debugTex.SaveAsPng(fs, w, h);

            monitor?.Log($"  ↳ 综合排查图: {debugPath}", LogLevel.Trace);
        }

        private static Color BlendOver(Color dst, Color src)
        {
            float sa = src.A / 255f;
            float da = dst.A / 255f;
            float outA = sa + da * (1 - sa);
            if (outA < 0.001f) return new Color(0, 0, 0, 0);
            float r = (src.R * sa + dst.R * da * (1 - sa)) / outA;
            float g = (src.G * sa + dst.G * da * (1 - sa)) / outA;
            float b = (src.B * sa + dst.B * da * (1 - sa)) / outA;
            return new Color((byte)r, (byte)g, (byte)b, (byte)(outA * 255));
        }

        private static void CopyProperties(IPropertyCollection source, IPropertyCollection dest)
        {
            foreach (var kv in source)
                dest[kv.Key] = kv.Value;
        }

        private static void CopyPixels(Color[] src, int srcW, int srcH,
                                       int srcX, int srcY, int copyW, int copyH,
                                       Color[] dst, int dstW, int dstH,
                                       int dstX, int dstY)
        {
            for (int y = 0; y < copyH; y++)
                for (int x = 0; x < copyW; x++)
                {
                    int si = (srcY + y) * srcW + (srcX + x);
                    int di = (dstY + y) * dstW + (dstX + x);
                    if (di >= 0 && di < dst.Length && si >= 0 && si < src.Length)
                        dst[di] = src[si];
                }
        }

        private static int Gcd(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
        private static int Lcm(int a, int b) => a / Gcd(a, b) * b;

        private class VirtualTileData
        {
            public TileSheet Sheet;
            public Texture2D Texture;
            public Color[] Pixels;
            public int PixelW, PixelH;
            public int TileW, TileH;
            public int OldSheetW, OldSheetH;
            public int NewSheetW, NewSheetH;
            public string Id;
            public bool SameSizeAsHost;

            public HashSet<(int x, int y)> ClampedCoords;
            public int MinX, MinY, MaxX, MaxY;
            public int BoundsW, BoundsH;
            public int OffsetX, OffsetY;

            public int ActualRows;
            public int ActualBoundsH;

            public Dictionary<(int oldX, int oldY), (int newX, int newY)> TileMap;
            public Dictionary<(int, int), (int, int)> DedupMap;
        }
    }
}
