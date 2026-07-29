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

namespace CPXnbExporter;

public static class TileSheetMerger
{
    public const string DefaultHostAssetName = "Maps/busPeople";

    public static bool IsVirtualTileSheet(TileSheet tileSheet)
    {
        if (tileSheet == null) return false;
        string src = tileSheet.ImageSource?.Replace('\\', '/');
        if (string.IsNullOrEmpty(src)) return false;

        return src.StartsWith("SMAPI/", StringComparison.OrdinalIgnoreCase)
            || src.IndexOf("/Mods/", StringComparison.OrdinalIgnoreCase) >= 0
            || src.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase);
    }

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

        monitor?.Log($"发现 {virtualSheets.Count} 个虚拟贴图", LogLevel.Trace);

        string hostId = hostAssetName.Replace('\\', '/');
        int lastSlash = hostId.LastIndexOf('/');
        if (lastSlash >= 0) hostId = hostId.Substring(lastSlash + 1);

        TileSheet hostSheet = map.TileSheets.FirstOrDefault(ts =>
            ts.ImageSource?.Replace('\\', '/').Equals(hostAssetName, StringComparison.OrdinalIgnoreCase) == true
            || ts.Id.Equals(hostId, StringComparison.OrdinalIgnoreCase));

        Texture2D hostTexture;
        try { hostTexture = helper.GameContent.Load<Texture2D>(hostAssetName); }
        catch (Exception ex)
        {
            monitor?.Log($"无法加载宿主贴图 {hostAssetName}: {ex.Message}", LogLevel.Trace);
            return null;
        }

        int hostPixelW = hostTexture.Width;
        int hostPixelH = hostTexture.Height;
        int hostTileW = hostSheet?.TileWidth ?? 64;
        int hostTileH = hostSheet?.TileHeight ?? 64;

        monitor?.Log($"宿主: {hostPixelW}x{hostPixelH}px, Tile={hostTileW}x{hostTileH}", LogLevel.Trace);

        var virtualDataList = new List<VirtualTileData>();

        foreach (var vSheet in virtualSheets)
        {
            Texture2D vTex;
            try { vTex = helper.GameContent.Load<Texture2D>(vSheet.ImageSource); }
            catch (Exception ex)
            {
                monitor?.Log($"无法加载虚拟贴图 {vSheet.ImageSource}: {ex.Message}", LogLevel.Trace);
                return null;
            }

            // ⚠ TileWidth/TileHeight 严格使用 TBIN 中定义的值（tile 尺寸在 TBIN，不在贴图）。
            //   绝不能根据贴图像素反推：例如虚拟 16×16 tile、宿主 64×64 tile，
            //   若按 vTex.Width/vSheet.SheetWidth 计算会把 16 误改成 64，导致 tile 放大。
            int tileW = vSheet.TileWidth;
            int tileH = vSheet.TileHeight;

            var pixels = new Color[vTex.Width * vTex.Height];
            vTex.GetData(pixels);

            var data = new VirtualTileData
            {
                Sheet = vSheet,
                Texture = vTex,
                Pixels = pixels,
                PixelW = vTex.Width,
                PixelH = vTex.Height,
                TileW = tileW,
                TileH = tileH,
                OldSheetW = vSheet.SheetWidth,
                OldSheetH = vSheet.SheetHeight,
                Id = vSheet.Id
            };

            data.SameSizeAsHost = (data.TileW == hostTileW && data.TileH == hostTileH);

            monitor?.Log($"虚拟 {data.Id}: {data.PixelW}x{data.PixelH}px, Tile={data.TileW}x{data.TileH}, Sheet={data.OldSheetW}x{data.OldSheetH}, SameSize={data.SameSizeAsHost}", LogLevel.Trace);

            virtualDataList.Add(data);
        }

        var usedCoordsMap = new Dictionary<VirtualTileData, HashSet<(int x, int y)>>();
        foreach (var vd in virtualDataList)
            usedCoordsMap[vd] = new HashSet<(int, int)>();

        foreach (Layer layer in map.Layers)
        {
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    CollectUsedCoords(layer.Tiles[x, y], usedCoordsMap);
                }
            }
        }

        var activeVirtualData = new List<VirtualTileData>();

        foreach (var vd in virtualDataList)
        {
            var coords = usedCoordsMap[vd];
            if (coords.Count == 0)
            {
                monitor?.Log($"虚拟 {vd.Id} 未被任何 tile 引用，跳过", LogLevel.Trace);
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
                monitor?.Log($"虚拟 {vd.Id} Clamp 后无有效坐标，跳过", LogLevel.Trace);
                continue;
            }

            // 去重 + 删除透明瓦片
            var uniqueNonTransparent = new Dictionary<string, (int x, int y)>();
            var dedupMap = new Dictionary<(int, int), (int x, int y)>();
            int transparentCount = 0;

            foreach (var coord in clampedCoords)
            {
                if (IsTileFullyTransparent(vd, coord.x, coord.y))
                {
                    dedupMap[coord] = (-1, -1);
                    transparentCount++;
                    continue;
                }

                byte[] hash = GetTilePixelHash(vd, coord.x, coord.y);
                string hashKey = Convert.ToBase64String(hash);

                if (uniqueNonTransparent.TryGetValue(hashKey, out var rep))
                {
                    dedupMap[coord] = rep;
                }
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
                monitor?.Log($"虚拟 {vd.Id} 所有瓦片均为透明或重复，跳过", LogLevel.Trace);
                continue;
            }

            // 计算旧包围盒（用于日志，但后续实际高度用新行数）
            vd.MinX = vd.ClampedCoords.Min(c => c.x);
            vd.MinY = vd.ClampedCoords.Min(c => c.y);
            vd.MaxX = vd.ClampedCoords.Max(c => c.x);
            vd.MaxY = vd.ClampedCoords.Max(c => c.y);
            vd.BoundsW = (vd.MaxX - vd.MinX + 1) * vd.TileW;
            vd.BoundsH = (vd.MaxY - vd.MinY + 1) * vd.TileH;

            int dupCount = clampedCoords.Count - uniqueNonTransparent.Count - transparentCount;
            monitor?.Log($"虚拟 {vd.Id}: 原始 {clampedCoords.Count} 个, 透明 {transparentCount} 个, 重复 {dupCount} 个, 最终唯一瓦片 {vd.ClampedCoords.Count} 个", LogLevel.Trace);

            activeVirtualData.Add(vd);
        }

        if (activeVirtualData.Count == 0)
        {
            monitor?.Log($"没有需要合并的虚拟贴图", LogLevel.Trace);
            return null;
        }

        activeVirtualData.Sort((a, b) =>
            (b.BoundsW * b.BoundsH).CompareTo(a.BoundsW * a.BoundsH));

        // 预先计算每个虚拟贴图的排列行数及实际像素高度
        foreach (var vd in activeVirtualData)
        {
            int tilesPerRow = hostPixelW / vd.TileW; // 暂时用宿主宽度，后面会扩展
            if (tilesPerRow < 1) tilesPerRow = 1;
            int rows = (vd.ClampedCoords.Count + tilesPerRow - 1) / tilesPerRow;
            vd.ActualRows = rows;
            vd.ActualBoundsH = rows * vd.TileH;
        }

        // 计算合并贴图总尺寸
        int mergedW = hostPixelW;
        int mergedH = hostPixelH;

        foreach (var vd in activeVirtualData)
        {
            // 先确定宽度，再确定每行能放多少瓦片（宽度可能还没最终确定，先暂用宿主宽度）
            int tilesPerRow = mergedW / vd.TileW;
            if (tilesPerRow < 1) tilesPerRow = 1;
            int rows = (vd.ClampedCoords.Count + tilesPerRow - 1) / tilesPerRow;
            vd.ActualRows = rows;
            vd.ActualBoundsH = rows * vd.TileH;

            vd.OffsetX = 0;
            vd.OffsetY = mergedH;
            int rem = vd.OffsetY % vd.TileH;
            if (rem != 0) vd.OffsetY += vd.TileH - rem;
            mergedH = vd.OffsetY + vd.ActualBoundsH; // 使用实际高度
            if (vd.BoundsW > mergedW) mergedW = vd.BoundsW;
        }

        int lcm = hostTileW;
        foreach (var vd in activeVirtualData)
            lcm = Lcm(lcm, vd.TileW);
        mergedW = ((mergedW + lcm - 1) / lcm) * lcm;

        // 宽度变化后，重新计算每个虚拟贴图的每行瓦片数和实际行数/高度，并更新偏移（因为偏移已经计算好，但排列可能因为宽度变化导致每行放更多瓦片，实际行数可能减少）
        foreach (var vd in activeVirtualData)
        {
            int tilesPerRow = mergedW / vd.TileW;
            if (tilesPerRow < 1) tilesPerRow = 1;
            int rows = (vd.ClampedCoords.Count + tilesPerRow - 1) / tilesPerRow;
            vd.ActualRows = rows;
            vd.ActualBoundsH = rows * vd.TileH;
            // 注意：偏移量 OffsetY 不变（已经在正确位置），但 mergedH 需要根据新的实际高度重新累计
        }

        // 重新按顺序累加 mergedH
        int tempH = hostPixelH;
        foreach (var vd in activeVirtualData)
        {
            vd.OffsetY = tempH;
            int rem = vd.OffsetY % vd.TileH;
            if (rem != 0) vd.OffsetY += vd.TileH - rem;
            tempH = vd.OffsetY + vd.ActualBoundsH;
        }
        mergedH = tempH;

        // 保证 mergedH 是所有 TileH（含宿主）的公倍数，防止 NewSheetH 截断
        int lcmH = hostTileH;
        foreach (var vd in activeVirtualData)
            lcmH = Lcm(lcmH, vd.TileH);
        mergedH = ((mergedH + lcmH - 1) / lcmH) * lcmH;

        monitor?.Log($"合并贴图: {mergedW}x{mergedH}px", LogLevel.Trace);

        // 生成 TileMap（跳过透明瓦片）
        foreach (var vd in activeVirtualData)
        {
            vd.TileMap = new Dictionary<(int oldX, int oldY), (int newX, int newY)>();

            int tilesPerRow = mergedW / vd.TileW;
            int col = 0, row = 0;

            var uniqueToNew = new Dictionary<(int, int), (int newX, int newY)>();
            foreach (var uniqueCoord in vd.ClampedCoords)
            {
                var newCoord = (col, row);
                uniqueToNew[uniqueCoord] = newCoord;
                col++;
                if (col >= tilesPerRow)
                {
                    col = 0;
                    row++;
                }
            }

            foreach (var kv in vd.DedupMap)
            {
                var oldCoord = kv.Key;
                var repCoord = kv.Value;

                if (repCoord == (-1, -1))
                    continue;

                vd.TileMap[oldCoord] = uniqueToNew[repCoord];
            }

            vd.NewSheetW = mergedW / vd.TileW;
            vd.NewSheetH = mergedH / vd.TileH;

            monitor?.Log($"虚拟 {vd.Id} 映射: {vd.TileMap.Count} 个 tile (有效), Sheet={vd.NewSheetW}x{vd.NewSheetH}, Offset=({vd.OffsetX},{vd.OffsetY})", LogLevel.Trace);
        }

        var device = hostTexture.GraphicsDevice;
        var mergedTex = new Texture2D(device, mergedW, mergedH);
        var mergedPixels = new Color[mergedW * mergedH];
        Array.Clear(mergedPixels, 0, mergedPixels.Length);

        var hostPixels = new Color[hostPixelW * hostPixelH];
        hostTexture.GetData(hostPixels);
        CopyPixels(hostPixels, hostPixelW, hostPixelH, 0, 0, hostPixelW, hostPixelH,
                   mergedPixels, mergedW, mergedH, 0, 0);

        // 填充宿主贴图底部到第一个虚拟贴图 OffsetY 之间的空隙
        tempH = hostPixelH;
        foreach (var vd in activeVirtualData)
        {
            int expectedY = tempH;
            int rem = expectedY % vd.TileH;
            if (rem != 0) expectedY += vd.TileH - rem;

            // 用宿主/前一贴图最后一行像素填充对齐产生的透明缝
            if (expectedY > tempH && tempH > 0)
            {
                for (int y = tempH; y < expectedY; y++)
                    for (int x = 0; x < mergedW; x++)
                    {
                        int srcIdx = (tempH - 1) * mergedW + x;
                        int dstIdx = y * mergedW + x;
                        if (srcIdx >= 0 && dstIdx < mergedPixels.Length)
                            mergedPixels[dstIdx] = mergedPixels[srcIdx];
                    }
            }

            // 复制该虚拟贴图的所有有效 tile
            var copiedReps = new HashSet<(int x, int y)>();
            foreach (var kv in vd.TileMap)
            {
                var oldCoord = kv.Key;
                var newCoord = kv.Value;

                (int repX, int repY) rep = vd.DedupMap[oldCoord];
                if (rep == (-1, -1)) continue;
                if (copiedReps.Contains(rep)) continue;
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

            tempH = expectedY + vd.ActualBoundsH;
        }

        // 若 mergedH 底部还有剩余空白，用最后一行像素填充
        if (tempH > 0 && tempH < mergedH)
        {
            for (int y = tempH; y < mergedH; y++)
                for (int x = 0; x < mergedW; x++)
                {
                    int srcIdx = (tempH - 1) * mergedW + x;
                    int dstIdx = y * mergedW + x;
                    if (srcIdx >= 0 && dstIdx < mergedPixels.Length)
                        mergedPixels[dstIdx] = mergedPixels[srcIdx];
                }
        }

        mergedTex.SetData(mergedPixels);

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
        }

        monitor?.Log($"宿主 TileSheet: {newHostSheetW}x{newHostSheetH} tiles, Tile={hostTileW}x{hostTileH}", LogLevel.Trace);

        foreach (var vd in activeVirtualData)
        {
            vd.Sheet.ImageSource = hostAssetName;
            vd.Sheet.SheetWidth = vd.NewSheetW;
            vd.Sheet.SheetHeight = vd.NewSheetH;
            monitor?.Log($"虚拟 {vd.Id} 已更新: ImageSource={hostAssetName}, Tile={vd.Sheet.TileWidth}x{vd.Sheet.TileHeight}, Sheet={vd.NewSheetW}x{vd.NewSheetH}", LogLevel.Trace);
        }

        var lookupDict = new Dictionary<string, VirtualTileData>(StringComparer.OrdinalIgnoreCase);
        foreach (var vd in activeVirtualData)
            if (!string.IsNullOrEmpty(vd.Id))
                lookupDict[vd.Id] = vd;

        foreach (Layer layer in map.Layers)
        {
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile is StaticTile st)
                        RewriteStaticTile(layer, x, y, st, hostSheet, lookupDict, oldHostSheetW, newHostSheetW, hostTileW, hostTileH, monitor);
                    else if (tile is AnimatedTile anim)
                        RewriteAnimatedTile(layer, x, y, anim, hostSheet, lookupDict, oldHostSheetW, newHostSheetW, hostTileW, hostTileH, monitor);
                }
            }
        }

        foreach (var vd in activeVirtualData)
        {
            if (vd.SameSizeAsHost)
            {
                try
                {
                    bool stillReferenced = false;
                    foreach (Layer layer in map.Layers)
                    {
                        for (int y = 0; y < layer.LayerHeight && !stillReferenced; y++)
                        {
                            for (int x = 0; x < layer.LayerWidth && !stillReferenced; x++)
                            {
                                var tile = layer.Tiles[x, y];
                                if (tile != null && tile.TileSheet == vd.Sheet)
                                    stillReferenced = true;
                            }
                        }
                    }

                    if (!stillReferenced)
                    {
                        map.RemoveTileSheet(vd.Sheet);
                        monitor?.Log($"移除冗余虚拟 TileSheet: {vd.Id} (尺寸与宿主相同，已重定向到宿主)", LogLevel.Trace);
                    }
                }
                catch (Exception ex)
                {
                    monitor?.Log($"无法移除虚拟 TileSheet {vd.Id}: {ex.Message}", LogLevel.Trace);
                }
            }
        }

        // 生成综合排查图
        if (!string.IsNullOrEmpty(troubleshootDir))
        {
            GenerateCombinedTroubleshootImage(mergedTex, activeVirtualData, hostPixelH, hostTileW, hostTileH, troubleshootDir, monitor);
        }

        monitor?.Log($"合并完成: {mergedW}x{mergedH}px", LogLevel.Trace);
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
            {
                if (st.TileSheet == kv.Key.Sheet)
                {
                    int ox = st.TileIndex % kv.Key.OldSheetW;
                    int oy = st.TileIndex / kv.Key.OldSheetW;
                    kv.Value.Add((ox, oy));
                    break;
                }
            }
        }
        else if (tile is AnimatedTile anim)
        {
            foreach (var frame in anim.TileFrames)
            {
                if (frame == null) continue;
                foreach (var kv in usedCoordsMap)
                {
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
            monitor?.Log($"Tile ({x},{y}) 旧坐标 ({vOldX},{vOldY}) 无内容，已置空", LogLevel.Trace);
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
                else
                {
                    newFrames[i] = frame;
                }
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

        // 每个虚拟贴图
        foreach (var vd in activeVirtualData)
        {
            var occupiedCells = new HashSet<(int newX, int newY)>();
            foreach (var kv in vd.TileMap)
            {
                occupiedCells.Add(kv.Value);
            }

            int boxX = vd.OffsetX;
            int boxY = vd.OffsetY;
            int boxW = w - boxX; // 左对齐占满行宽
            int boxH = vd.ActualBoundsH;

            Color boxColor = new Color(255, 0, 0, 220);
            // 上边框
            for (int px = boxX; px < boxX + boxW && px < w; px++)
                if (boxY >= 0 && boxY < h) pixels[boxY * w + px] = boxColor;
            // 下边框
            for (int px = boxX; px < boxX + boxW && px < w; px++)
                if (boxY + boxH - 1 >= 0 && boxY + boxH - 1 < h) pixels[(boxY + boxH - 1) * w + px] = boxColor;
            // 左边框
            for (int py = boxY; py < boxY + boxH && py < h; py++)
                if (boxX >= 0 && boxX < w) pixels[py * w + boxX] = boxColor;
            // 右边框
            for (int py = boxY; py < boxY + boxH && py < h; py++)
                if (boxX + boxW - 1 >= 0 && boxX + boxW - 1 < w) pixels[py * w + (boxX + boxW - 1)] = boxColor;

            // 蓝网格
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

            // 黄点
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

        monitor?.Log($"综合排查图: {debugPath}", LogLevel.Trace);
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
        // 主体复制
        for (int y = 0; y < copyH; y++)
            for (int x = 0; x < copyW; x++)
            {
                int si = (srcY + y) * srcW + (srcX + x);
                int di = (dstY + y) * dstW + (dstX + x);
                if (di >= 0 && di < dst.Length && si >= 0 && si < src.Length)
                    dst[di] = src[si];
            }

        // ---- 1px Extrude（防边缘白线）----
        // 上
        if (dstY > 0)
            for (int x = 0; x < copyW; x++)
            {
                int si = srcY * srcW + (srcX + x);
                int di = (dstY - 1) * dstW + (dstX + x);
                if (di >= 0 && di < dst.Length && si >= 0 && si < src.Length)
                    dst[di] = src[si];
            }
        // 下
        if (dstY + copyH < dstH)
            for (int x = 0; x < copyW; x++)
            {
                int si = (srcY + copyH - 1) * srcW + (srcX + x);
                int di = (dstY + copyH) * dstW + (dstX + x);
                if (di >= 0 && di < dst.Length && si >= 0 && si < src.Length)
                    dst[di] = src[si];
            }
        // 左
        if (dstX > 0)
            for (int y = 0; y < copyH; y++)
            {
                int si = (srcY + y) * srcW + srcX;
                int di = (dstY + y) * dstW + (dstX - 1);
                if (di >= 0 && di < dst.Length && si >= 0 && si < src.Length)
                    dst[di] = src[si];
            }
        // 右
        if (dstX + copyW < dstW)
            for (int y = 0; y < copyH; y++)
            {
                int si = (srcY + y) * srcW + (srcX + copyW - 1);
                int di = (dstY + y) * dstW + (dstX + copyW);
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

        // 实际排列后的行数和像素高度
        public int ActualRows;
        public int ActualBoundsH;

        public Dictionary<(int oldX, int oldY), (int newX, int newY)> TileMap;
        public Dictionary<(int, int), (int, int)> DedupMap;
    }
}