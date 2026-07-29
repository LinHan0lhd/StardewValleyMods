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

        public static bool IsVirtualTileSheet(TileSheet tileSheet)
        {
            if (tileSheet == null) return false;
            string src = tileSheet.ImageSource?.Replace('\\', '/');
            if (string.IsNullOrEmpty(src)) return false;

            return src.StartsWith("SMAPI/", StringComparison.OrdinalIgnoreCase)
                || src.IndexOf("/Mods/", StringComparison.OrdinalIgnoreCase) >= 0
                || src.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase);
        }

        private static int Gcd(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
        private static int Lcm(int a, int b) => a / Gcd(a, b) * b;

        /// <summary>
        /// 合并虚拟贴图：提取有内容的 tile，去重，紧凑排列到宿主下方。
        /// 宿主的 Tile 尺寸（通常 64×64）与虚拟贴图的 Tile 尺寸（通常 16×16）各自独立。
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

            monitor?.Log($"  ↳ 发现 {virtualSheets.Count} 个虚拟贴图，提取有内容的 tile 合并到 {hostAssetName}", LogLevel.Trace);

            // ---- 宿主贴图 ----
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
                monitor?.Log($"  ↳ 合并失败：无法加载宿主贴图 {hostAssetName}: {ex.Message}", LogLevel.Trace);
                return null;
            }

            int hostPixelW = hostTexture.Width;
            int hostPixelH = hostTexture.Height;
            int hostTileW = hostSheet?.TileWidth ?? 64;
            int hostTileH = hostSheet?.TileHeight ?? 64;

            monitor?.Log($"  ↳ 宿主: {hostPixelW}x{hostPixelH}px, Tile={hostTileW}x{hostTileH}", LogLevel.Trace);

            // ---- 加载虚拟贴图，提取有内容的 tile（原始尺寸，不放大） ----
            var vDataList = new List<VirtualData>();
            var allTiles = new List<(VirtualData vd, int oldX, int oldY, Color[] pixels)>();

            foreach (var vSheet in virtualSheets)
            {
                Texture2D vTex;
                try { vTex = helper.GameContent.Load<Texture2D>(vSheet.ImageSource); }
                catch (Exception ex)
                {
                    monitor?.Log($"  ↳ 合并失败：无法加载虚拟贴图 {vSheet.ImageSource}: {ex.Message}", LogLevel.Trace);
                    return null;
                }

                int pixelW = vTex.Width;
                int pixelH = vTex.Height;

                // 修正瓦片尺寸：TBIN 中的 TileWidth/TileHeight 有时与实际贴图
                // 不符（例如 CP 的 PatchMode.Replace 换了贴图但没改 TBIN）。
                // 根据贴图像素尺寸 / Sheet tile 数量反推实际 tile 尺寸。
                if (vSheet.SheetWidth > 0 && vSheet.SheetHeight > 0)
                {
                    int calcW = pixelW / vSheet.SheetWidth;
                    int calcH = pixelH / vSheet.SheetHeight;
                    if (calcW > 0 && calcH > 0 &&
                        (vSheet.TileWidth != calcW || vSheet.TileHeight != calcH))
                    {
                        monitor?.Log(
                            $"  ↳ 修正虚拟贴图 [{vSheet.Id}] 的瓦片尺寸: " +
                            $"{vSheet.TileWidth}x{vSheet.TileHeight} → {calcW}x{calcH}",
                            LogLevel.Warn);
                        vSheet.TileWidth = calcW;
                        vSheet.TileHeight = calcH;
                    }
                }

                int tileW = vSheet.TileWidth;
                int tileH = vSheet.TileHeight;
                int oldSheetW = vSheet.SheetWidth;
                int oldSheetH = vSheet.SheetHeight;

                var vPixels = new Color[pixelW * pixelH];
                vTex.GetData(vPixels);

                var vd = new VirtualData
                {
                    Sheet = vSheet,
                    Texture = vTex,
                    PixelW = pixelW,
                    PixelH = pixelH,
                    TileW = tileW,
                    TileH = tileH,
                    OldSheetW = oldSheetW,
                    OldSheetH = oldSheetH,
                    SheetId = vSheet.Id,
                    TileMap = new Dictionary<(int oldX, int oldY), (int newX, int newY)>()
                };

                monitor?.Log($"  ↳ 虚拟 {vSheet.Id}: {pixelW}x{pixelH}px, Tile={tileW}x{tileH}, Sheet={oldSheetW}x{oldSheetH}", LogLevel.Trace);

                // 遍历所有 tile，提取有内容的
                int contentCount = 0;
                for (int ty = 0; ty < oldSheetH; ty++)
                {
                    for (int tx = 0; tx < oldSheetW; tx++)
                    {
                        bool hasContent = false;
                        int startY = ty * tileH;
                        int endY = Math.Min(startY + tileH, pixelH);
                        int startX = tx * tileW;
                        int endX = Math.Min(startX + tileW, pixelW);

                        for (int py = startY; py < endY && !hasContent; py++)
                        {
                            for (int px = startX; px < endX && !hasContent; px++)
                            {
                                int idx = py * pixelW + px;
                                if (idx < vPixels.Length && vPixels[idx].A > 0)
                                    hasContent = true;
                            }
                        }

                        if (hasContent)
                        {
                            var tilePixels = new Color[tileW * tileH];
                            for (int py = 0; py < tileH && startY + py < pixelH; py++)
                            {
                                for (int px = 0; px < tileW && startX + px < pixelW; px++)
                                {
                                    int srcIdx = (startY + py) * pixelW + (startX + px);
                                    int dstIdx = py * tileW + px;
                                    if (srcIdx < vPixels.Length)
                                        tilePixels[dstIdx] = vPixels[srcIdx];
                                }
                            }
                            allTiles.Add((vd, tx, ty, tilePixels));
                            contentCount++;
                        }
                    }
                }

                monitor?.Log($"  ↳ 虚拟 {vSheet.Id} 有 {contentCount} 个有内容的 tile", LogLevel.Trace);
                vDataList.Add(vd);
            }

            if (allTiles.Count == 0)
            {
                monitor?.Log($"  ↳ 没有有内容的虚拟 tile，跳过合并", LogLevel.Trace);
                return null;
            }

            // ---- 去重：相同像素的 tile 只保留一份 ----
            var uniqueTiles = new List<(VirtualData vd, int oldX, int oldY, Color[] pixels)>();
            var hashSet = new HashSet<int>();

            foreach (var (vd, oldX, oldY, pixels) in allTiles)
            {
                int hash = 17;
                for (int i = 0; i < pixels.Length; i++)
                {
                    var c = pixels[i];
                    hash = hash * 31 + c.R;
                    hash = hash * 31 + c.G;
                    hash = hash * 31 + c.B;
                    hash = hash * 31 + c.A;
                }

                if (!hashSet.Contains(hash))
                {
                    hashSet.Add(hash);
                    uniqueTiles.Add((vd, oldX, oldY, pixels));
                }
            }

            int uniqueCount = uniqueTiles.Count;
            monitor?.Log($"  ↳ 去重后：{uniqueCount} 个唯一 tile", LogLevel.Trace);
            allTiles = uniqueTiles;

            // ---- 计算合并布局 ----
            int lcmW = hostTileW;
            foreach (var vd in vDataList)
                lcmW = Lcm(lcmW, vd.TileW);
            int mergedW = ((hostPixelW + lcmW - 1) / lcmW) * lcmW;
            if (mergedW < hostPixelW) mergedW += lcmW;

            int mergedH = hostPixelH;
            foreach (var vd in vDataList)
            {
                var tiles = allTiles.Where(t => t.vd == vd).ToList();
                int tilesPerRow = mergedW / vd.TileW;
                int rows = (tiles.Count + tilesPerRow - 1) / tilesPerRow;
                int usedHeight = rows * vd.TileH;

                // 确保 OffsetY 是 vd.TileH 的整数倍
                int offsetY = mergedH;
                int rem = offsetY % vd.TileH;
                if (rem != 0) offsetY += vd.TileH - rem;

                vd.OffsetY = offsetY;
                vd.UsedHeight = usedHeight;
                vd.UsedTilesCount = tiles.Count;

                mergedH = offsetY + usedHeight;

                int col = 0, row = 0;
                foreach (var (_, oldX, oldY, _) in tiles)
                {
                    vd.TileMap[(oldX, oldY)] = (col, row);
                    col++;
                    if (col >= tilesPerRow)
                    {
                        col = 0;
                        row++;
                    }
                }

                monitor?.Log($"  ↳ 虚拟 {vd.SheetId}: {tiles.Count} 个唯一 tile, {rows} 行, Tile={vd.TileW}x{vd.TileH}, OffsetY={vd.OffsetY}px", LogLevel.Trace);
            }

            monitor?.Log($"  ↳ 合并贴图: {mergedW}x{mergedH}px", LogLevel.Trace);

            // ---- 创建合并贴图 ----
            var device = hostTexture.GraphicsDevice;
            var mergedTex = new Texture2D(device, mergedW, mergedH);
            var mergedPixels = new Color[mergedW * mergedH];
            Array.Clear(mergedPixels, 0, mergedPixels.Length);

            // 复制宿主像素
            var hostPixels = new Color[hostPixelW * hostPixelH];
            hostTexture.GetData(hostPixels);
            CopyPixels(hostPixels, hostPixelW, hostPixelH, 0, 0, hostPixelW, hostPixelH,
                       mergedPixels, mergedW, mergedH, 0, 0);

            // 复制虚拟 tile 像素
            foreach (var (vd, oldX, oldY, tilePixels) in allTiles)
            {
                var (newX, newY) = vd.TileMap[(oldX, oldY)];
                int dstX = newX * vd.TileW;
                int dstY = vd.OffsetY + newY * vd.TileH;

                CopyPixels(tilePixels, vd.TileW, vd.TileH, 0, 0, vd.TileW, vd.TileH,
                           mergedPixels, mergedW, mergedH, dstX, dstY);
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
            }

            monitor?.Log($"  ↳ 宿主 TileSheet: {newHostSheetW}x{newHostSheetH} tiles", LogLevel.Trace);

            // ---- 更新虚拟 TileSheet（保留，指向同一张合并贴图） ----
            foreach (var vd in vDataList)
            {
                vd.NewSheetW = mergedW / vd.TileW;
                vd.NewSheetH = mergedH / vd.TileH;

                vd.Sheet.ImageSource = hostAssetName;
                vd.Sheet.SheetWidth = vd.NewSheetW;
                vd.Sheet.SheetHeight = vd.NewSheetH;

                monitor?.Log($"  ↳ 虚拟 {vd.Sheet.Id} TileSheet: {vd.NewSheetW}x{vd.NewSheetH} tiles, Tile={vd.TileW}x{vd.TileH}, OffsetY={vd.OffsetY}px", LogLevel.Trace);
            }

            // ---- 构建 Sheet.Id 查找字典 ----
            var vDataById = new Dictionary<string, VirtualData>(StringComparer.OrdinalIgnoreCase);
            foreach (var vd in vDataList)
                if (!string.IsNullOrEmpty(vd.SheetId))
                    vDataById[vd.SheetId] = vd;

            // ---- 重写地图中的 Tile 引用 ----
            foreach (Layer layer in map.Layers)
            {
                for (int ly = 0; ly < layer.LayerHeight; ly++)
                    for (int lx = 0; lx < layer.LayerWidth; lx++)
                        RewriteTile(layer, lx, ly, hostSheet, vDataById, oldHostSheetW, newHostSheetW, monitor);
            }

            // ---- 生成排查图 ----
            if (!string.IsNullOrEmpty(troubleshootDir))
            {
                foreach (var vd in vDataList)
                {
                    GenerateTroubleshootImage(mergedTex, vd, hostPixelH, hostTileW, hostTileH, troubleshootDir, monitor);
                }
            }

            monitor?.Log($"  ↳ 合并完成: {mergedW}x{mergedH}px, 共提取 {uniqueCount} 个唯一 tile", LogLevel.Trace);
            return mergedTex;
        }

        private static void RewriteTile(
            Layer layer, int x, int y,
            TileSheet hostSheet,
            Dictionary<string, VirtualData> vDataById,
            int oldHostSheetW, int newHostSheetW,
            IMonitor monitor)
        {
            var tile = layer.Tiles[x, y];
            if (tile == null) return;

            if (tile is StaticTile st)
            {
                // 宿主 tile
                if (st.TileSheet == hostSheet)
                {
                    if (oldHostSheetW != newHostSheetW)
                    {
                        int oldX = st.TileIndex % oldHostSheetW;
                        int oldY = st.TileIndex / oldHostSheetW;
                        int hostNewIdx = oldY * newHostSheetW + oldX;
                        var hostNewTile = new StaticTile(layer, hostSheet, st.BlendMode, hostNewIdx);
                        CopyProperties(st.Properties, hostNewTile.Properties);
                        layer.Tiles[x, y] = hostNewTile;
                    }
                    return;
                }

                // 虚拟 tile
                string sheetId = st.TileSheet.Id;
                if (string.IsNullOrEmpty(sheetId)) return;

                if (!vDataById.TryGetValue(sheetId, out var vd))
                {
                    monitor?.Log($"  ↳ 警告：Tile ({x},{y}) 引用的 SheetId='{sheetId}' 未找到，跳过", LogLevel.Trace);
                    return;
                }

                int vOldX = st.TileIndex % vd.OldSheetW;
                int vOldY = st.TileIndex / vd.OldSheetW;

                if (!vd.TileMap.TryGetValue((vOldX, vOldY), out var newPos))
                {
                    layer.Tiles[x, y] = null;
                    monitor?.Log($"  ↳ Tile ({x},{y}) 旧坐标 ({vOldX},{vOldY}) 没有内容，已置空", LogLevel.Trace);
                    return;
                }

                int newX = newPos.newX;
                int newY = vd.OffsetY / vd.TileH + newPos.newY;
                int vNewIdx = newY * vd.NewSheetW + newX;

                if (vNewIdx < 0 || vNewIdx >= vd.NewSheetW * vd.NewSheetH)
                {
                    layer.Tiles[x, y] = null;
                    monitor?.Log($"  ↳ Tile ({x},{y}) 新索引 {vNewIdx} 超出范围，已置空", LogLevel.Trace);
                    return;
                }

                var vNewTile = new StaticTile(layer, vd.Sheet, st.BlendMode, vNewIdx);
                CopyProperties(st.Properties, vNewTile.Properties);
                layer.Tiles[x, y] = vNewTile;
            }
            else if (tile is AnimatedTile anim)
            {
                var newFrames = new StaticTile[anim.TileFrames.Length];
                bool anyRewritten = false;
                bool anyFailed = false;

                for (int i = 0; i < anim.TileFrames.Length; i++)
                {
                    var frame = anim.TileFrames[i];

                    if (frame.TileSheet == hostSheet)
                    {
                        if (oldHostSheetW != newHostSheetW)
                        {
                            int oldX = frame.TileIndex % oldHostSheetW;
                            int oldY = frame.TileIndex / oldHostSheetW;
                            int hostFrmIdx = oldY * newHostSheetW + oldX;
                            newFrames[i] = new StaticTile(layer, hostSheet, frame.BlendMode, hostFrmIdx);
                            CopyProperties(frame.Properties, newFrames[i].Properties);
                            anyRewritten = true;
                        }
                        else
                        {
                            newFrames[i] = frame;
                        }
                        continue;
                    }

                    string frameSheetId = frame.TileSheet.Id;
                    if (string.IsNullOrEmpty(frameSheetId))
                    {
                        newFrames[i] = frame;
                        continue;
                    }

                    if (!vDataById.TryGetValue(frameSheetId, out var vd))
                    {
                        monitor?.Log($"  ↳ 警告：AnimatedTile ({x},{y}) frame {i} SheetId='{frameSheetId}' 未找到", LogLevel.Trace);
                        newFrames[i] = frame;
                        continue;
                    }

                    int vOldX = frame.TileIndex % vd.OldSheetW;
                    int vOldY = frame.TileIndex / vd.OldSheetW;

                    if (!vd.TileMap.TryGetValue((vOldX, vOldY), out var newPos))
                    {
                        newFrames[i] = null;
                        anyFailed = true;
                        continue;
                    }

                    int newX = newPos.newX;
                    int newY = vd.OffsetY / vd.TileH + newPos.newY;
                    int vFrmIdx = newY * vd.NewSheetW + newX;

                    if (vFrmIdx < 0 || vFrmIdx >= vd.NewSheetW * vd.NewSheetH)
                    {
                        newFrames[i] = null;
                        anyFailed = true;
                        continue;
                    }

                    newFrames[i] = new StaticTile(layer, vd.Sheet, frame.BlendMode, vFrmIdx);
                    CopyProperties(frame.Properties, newFrames[i].Properties);
                    anyRewritten = true;
                }

                if (anyRewritten || anyFailed)
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
        }

        /// <summary>
        /// 生成排查图：宿主网格（灰色）+ 虚拟网格（蓝色）
        /// </summary>
        private static void GenerateTroubleshootImage(
            Texture2D mergedTex,
            VirtualData vd,
            int hostPixelH,
            int hostTileW,
            int hostTileH,
            string troubleshootDir,
            IMonitor monitor)
        {
            if (mergedTex == null || string.IsNullOrEmpty(troubleshootDir)) return;

            int w = mergedTex.Width;
            int h = mergedTex.Height;
            var pixels = new Color[w * h];
            mergedTex.GetData(pixels);

            // 宿主网格（灰色）
            Color hostGridColor = new Color(128, 128, 128, 100);
            for (int gy = 0; gy < hostPixelH; gy += hostTileH)
                for (int px = 0; px < w; px++)
                {
                    int idx = gy * w + px;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = BlendOver(pixels[idx], hostGridColor);
                }
            for (int gx = 0; gx < w; gx += hostTileW)
                for (int py = 0; py < hostPixelH; py++)
                {
                    int idx = py * w + gx;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = BlendOver(pixels[idx], hostGridColor);
                }

            // 分隔线（绿色）
            Color sepColor = new Color(0, 255, 0, 200);
            for (int px = 0; px < w; px++)
            {
                int idx = hostPixelH * w + px;
                if (idx >= 0 && idx < pixels.Length)
                    pixels[idx] = sepColor;
            }

            // 虚拟区域边框（红色）
            Color boxColor = new Color(255, 0, 0, 220);
            int boxX = 0;
            int boxY = vd.OffsetY;
            int boxW = Math.Min(vd.UsedTilesCount * vd.TileW, w);
            int boxH = vd.UsedHeight;

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

            // 虚拟网格（蓝色，直接用 TileSheet 的 Tile 尺寸）
            int vTileW = vd.Sheet.TileWidth;
            int vTileH = vd.Sheet.TileHeight;
            Color gridColor = new Color(0, 100, 255, 80);

            for (int gy = boxY; gy < boxY + boxH && gy < h; gy += vTileH)
                for (int px = boxX; px < boxX + boxW && px < w; px++)
                {
                    int idx = gy * w + px;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = BlendOver(pixels[idx], gridColor);
                }
            for (int gx = boxX; gx < boxX + boxW && gx < w; gx += vTileW)
                for (int py = boxY; py < boxY + boxH && py < h; py++)
                {
                    int idx = py * w + gx;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = BlendOver(pixels[idx], gridColor);
                }

            // 黄色标记点
            Color dotColor = new Color(255, 255, 0, 180);
            foreach (var kv in vd.TileMap)
            {
                var newPos = kv.Value;
                int dotX = newPos.newX * vTileW + vTileW / 2;
                int dotY = vd.OffsetY + newPos.newY * vTileH + vTileH / 2;
                if (dotX >= 0 && dotX < w && dotY >= 0 && dotY < h)
                {
                    int idx = dotY * w + dotX;
                    if (idx >= 0 && idx < pixels.Length)
                        pixels[idx] = dotColor;
                }
            }

            // 保存
            using var debugTex = new Texture2D(mergedTex.GraphicsDevice, w, h);
            debugTex.SetData(pixels);

            string safeId = vd.Sheet.Id?.Replace('/', '_').Replace('\\', '_') ?? "unknown";
            string tsDir = Path.Combine(troubleshootDir, safeId);
            Directory.CreateDirectory(tsDir);

            string debugPath = Path.Combine(tsDir, "merged_debug.png");
            using (var fs = new FileStream(debugPath, FileMode.Create, FileAccess.Write))
                debugTex.SaveAsPng(fs, w, h);

            monitor?.Log($"  ↳ 排查图: {debugPath} (宿主网格{hostTileW}x{hostTileH}, 虚拟网格{vTileW}x{vTileH})", LogLevel.Trace);
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

        private class VirtualData
        {
            public TileSheet Sheet;
            public Texture2D Texture;
            public int PixelW, PixelH;
            public int TileW, TileH;
            public int OldSheetW, OldSheetH;
            public int NewSheetW, NewSheetH;
            public int OffsetY;
            public int UsedHeight;
            public int UsedTilesCount;
            public string SheetId;
            public Dictionary<(int oldX, int oldY), (int newX, int newY)> TileMap;
        }
    }
}
