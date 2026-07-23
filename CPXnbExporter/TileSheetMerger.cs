using System;
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
    /// <summary>
    /// 将地图中的虚拟 tilesheet（通常是 SMAPI 注册的模组贴图）合并到宿主 tilesheet。
    /// 用于绕过原版游戏的 ContentHashes.json 白名单限制：新增路径无法加载，
    /// 但已有白名单路径（如 Maps/paths）可以被替换/扩展。
    /// </summary>
    public static class TileSheetMerger
    {
        /// <summary>
        /// 默认宿主 tilesheet 的 Content 路径。
        /// 使用 Maps/busPeople（废弃的公交车场景 tilesheet），原因：
        /// 1. 游戏废案，原版不会加载，随便改不会破坏任何正常场景；
        /// 2. 宽度极大（1024px+），能容纳最宽的模组贴图，减少切片次数；
        /// 3. 在白名单内，原版 ContentHashes.json 允许加载。
        /// </summary>
        public const string DefaultHostAssetName = "Maps/busPeople";

        /// <summary>
        /// 判断一个 tilesheet 是否是虚拟 tilesheet（需要合并）。
        /// 虚拟 tilesheet 指 SMAPI 注册的模组贴图，或已规范化到 Maps/Mods/... 的路径。
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
        /// 把地图中所有虚拟 tilesheet 合并到指定宿主 tilesheet。
        /// 合并后，地图中引用虚拟 tilesheet 的 tile 会被重写到宿主 tilesheet 的扩展区域。
        /// 使用 Shelf Next-Fit 算法在宿主下方紧凑排列；过宽的贴图会自动垂直切片，
        /// 切成不超过宿主宽度的条带分别放入不同货架行。
        /// </summary>
        public static Texture2D MergeVirtualTileSheets(Map map, string hostAssetName, IModHelper helper, IMonitor monitor)
        {
            if (map == null || helper == null) return null;

            var virtualSheets = map.TileSheets.Where(IsVirtualTileSheet).ToList();
            if (virtualSheets.Count == 0) return null;

            monitor?.Log($"  ↳ 发现 {virtualSheets.Count} 个虚拟 tilesheet，准备紧凑合并到 {hostAssetName}", LogLevel.Trace);

            // 解析宿主 ID
            string hostId = hostAssetName.Replace('\\', '/');
            int lastSlash = hostId.LastIndexOf('/');
            if (lastSlash >= 0) hostId = hostId.Substring(lastSlash + 1);

            TileSheet hostSheet = map.TileSheets.FirstOrDefault(ts =>
                ts.ImageSource?.Replace('\\', '/').Equals(hostAssetName, StringComparison.OrdinalIgnoreCase) == true
                || ts.Id.Equals(hostId, StringComparison.OrdinalIgnoreCase));

            // 加载宿主贴图
            Texture2D hostTexture;
            try { hostTexture = helper.GameContent.Load<Texture2D>(hostAssetName); }
            catch (Exception ex)
            {
                monitor?.Log($"  ↳ 合并失败：无法加载宿主贴图 {hostAssetName}: {ex.Message}", LogLevel.Trace);
                return null;
            }

            int tileWidth = hostSheet?.TileWidth ?? virtualSheets[0].TileWidth;
            int tileHeight = hostSheet?.TileHeight ?? virtualSheets[0].TileHeight;
            if (tileWidth <= 0 || tileHeight <= 0)
            {
                monitor?.Log($"  ↳ 合并失败：tile 尺寸无效 {tileWidth}x{tileHeight}", LogLevel.Trace);
                return null;
            }

            int hostPixelW = hostTexture.Width;
            int hostPixelH = hostTexture.Height;
            int hostTilesWide = hostPixelW / tileWidth;
            if (hostTilesWide <= 0) hostTilesWide = 1;

            // ---- 加载所有虚拟贴图，过宽则自动垂直切片 ----
            var candidates = new List<VirtualSheetInfo>();
            foreach (var vSheet in virtualSheets)
            {
                Texture2D vTex;
                try { vTex = helper.GameContent.Load<Texture2D>(vSheet.ImageSource); }
                catch (Exception ex)
                {
                    monitor?.Log($"  ↳ 合并失败：无法加载虚拟贴图 {vSheet.ImageSource}: {ex.Message}", LogLevel.Trace);
                    return null;
                }

                if (vSheet.TileWidth != tileWidth || vSheet.TileHeight != tileHeight)
                {
                    monitor?.Log($"  ↳ 合并失败：虚拟 tilesheet {vSheet.Id} 的 tile 尺寸 {vSheet.TileWidth}x{vSheet.TileHeight} 与宿主 {tileWidth}x{tileHeight} 不一致", LogLevel.Trace);
                    return null;
                }

                int alignedW = ((vTex.Width + tileWidth - 1) / tileWidth) * tileWidth;
                int alignedH = ((vTex.Height + tileHeight - 1) / tileHeight) * tileHeight;
                int vTilesWide = alignedW / tileWidth;

                if (vTilesWide > hostTilesWide)
                {
                    // 过宽，垂直切成多条，每条宽度不超过宿主宽度
                    monitor?.Log($"  ↳ 虚拟贴图 {vSheet.Id} 宽度 {alignedW}px ({vTilesWide} tiles) 超过宿主 {hostPixelW}px，自动切片", LogLevel.Trace);

                    for (int col = 0; col < vTilesWide; col += hostTilesWide)
                    {
                        int colsInSlice = Math.Min(hostTilesWide, vTilesWide - col);
                        int slicePixelW = colsInSlice * tileWidth;

                        candidates.Add(new VirtualSheetInfo
                        {
                            TileSheet = vSheet,
                            Texture = vTex,
                            AlignedWidth = slicePixelW,
                            AlignedHeight = alignedH,
                            SourceX = col * tileWidth,
                            SourceY = 0,
                            StartTileCol = col,
                            EndTileCol = col + colsInSlice
                        });
                    }
                }
                else
                {
                    candidates.Add(new VirtualSheetInfo
                    {
                        TileSheet = vSheet,
                        Texture = vTex,
                        AlignedWidth = alignedW,
                        AlignedHeight = alignedH,
                        SourceX = 0,
                        SourceY = 0,
                        StartTileCol = 0,
                        EndTileCol = vTilesWide
                    });
                }
            }

            // ---- Shelf Next-Fit Decreasing Height 紧凑排列 ----
            candidates.Sort((a, b) => b.AlignedHeight.CompareTo(a.AlignedHeight));

            var shelves = new List<Shelf>();
            foreach (var cand in candidates)
            {
                bool placed = false;
                foreach (var shelf in shelves.OrderByDescending(s => s.RemainingWidth))
                {
                    if (shelf.TryPlace(cand, hostPixelW))
                    {
                        placed = true;
                        break;
                    }
                }
                if (!placed)
                {
                    var newShelf = new Shelf(hostPixelH);
                    newShelf.TryPlace(cand, hostPixelW);
                    shelves.Add(newShelf);
                }
            }

            // 计算最终贴图尺寸
            int newPixelW = hostPixelW;
            int newPixelH = hostPixelH;
            foreach (var shelf in shelves)
                newPixelH = Math.Max(newPixelH, shelf.Y + shelf.Height);

            // ---- 创建合并贴图 ----
            var device = hostTexture.GraphicsDevice;
            var mergedTexture = new Texture2D(device, newPixelW, newPixelH);
            var mergedPixels = new Color[newPixelW * newPixelH];
            Array.Clear(mergedPixels, 0, mergedPixels.Length);

            // 复制宿主
            var hostPixels = new Color[hostPixelW * hostPixelH];
            hostTexture.GetData(hostPixels);
            CopyPixels(hostPixels, hostPixelW, hostPixelH, 0, 0, hostPixelW, hostPixelH, mergedPixels, newPixelW, newPixelH, 0, 0);

            // 复制虚拟贴图（按条带）
            var layoutMap = new Dictionary<TileSheet, List<VirtualSheetLayout>>();
            foreach (var cand in candidates)
            {
                var vPixels = new Color[cand.Texture.Width * cand.Texture.Height];
                cand.Texture.GetData(vPixels);

                // 防御：切片后的实际复制区域不能超出原 texture 边界
                int copyW = Math.Min(cand.AlignedWidth, cand.Texture.Width - cand.SourceX);
                int copyH = Math.Min(cand.AlignedHeight, cand.Texture.Height - cand.SourceY);

                CopyPixels(vPixels, cand.Texture.Width, cand.Texture.Height,
                           cand.SourceX, cand.SourceY, copyW, copyH,
                           mergedPixels, newPixelW, newPixelH, cand.OffsetX, cand.OffsetY);

                if (!layoutMap.ContainsKey(cand.TileSheet))
                    layoutMap[cand.TileSheet] = new List<VirtualSheetLayout>();

                layoutMap[cand.TileSheet].Add(new VirtualSheetLayout
                {
                    TileSheet = cand.TileSheet,
                    OffsetX = cand.OffsetX,
                    OffsetY = cand.OffsetY,
                    AlignedWidth = cand.AlignedWidth,
                    AlignedHeight = cand.AlignedHeight,
                    StartTileCol = cand.StartTileCol,
                    EndTileCol = cand.EndTileCol
                });
            }

            mergedTexture.SetData(mergedPixels);

            // ---- 更新宿主 tilesheet ----
            int newSheetWidth = newPixelW / tileWidth;
            int newSheetHeight = newPixelH / tileHeight;

            if (hostSheet == null)
            {
                hostSheet = new TileSheet(hostId, map, hostAssetName,
                    new Size { Width = newSheetWidth, Height = newSheetHeight },
                    new Size { Width = tileWidth, Height = tileHeight });
                map.AddTileSheet(hostSheet);
            }
            else
            {
                hostSheet.ImageSource = hostAssetName;
                hostSheet.SheetWidth = newSheetWidth;
                hostSheet.SheetHeight = newSheetHeight;
            }

            // ---- 重写地图 tile 引用 ----
            foreach (Layer layer in map.Layers)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                    for (int x = 0; x < layer.LayerWidth; x++)
                        RewriteTile(layer, x, y, hostSheet, layoutMap, newSheetWidth);
            }

            // 移除虚拟 tilesheet
            foreach (var vSheet in virtualSheets)
                map.RemoveTileSheet(vSheet);

            int sliceCount = candidates.Count - virtualSheets.Count;
            monitor?.Log($"  ↳ 紧凑合并完成：宿主扩展为 {newSheetWidth}x{newSheetHeight} tiles，共 {shelves.Count} 行货架，{virtualSheets.Count} 个虚拟 tilesheet" +
                         (sliceCount > 0 ? $"（含 {sliceCount} 次自动切片）" : ""), LogLevel.Trace);

            return mergedTexture;
        }

        private static void RewriteTile(Layer layer, int x, int y, TileSheet hostSheet, Dictionary<TileSheet, List<VirtualSheetLayout>> layoutMap, int newSheetWidth)
        {
            var tile = layer.Tiles[x, y];
            if (tile == null) return;

            if (tile is StaticTile st)
            {
                if (layoutMap.TryGetValue(st.TileSheet, out var layouts))
                {
                    int oldX = st.TileIndex % st.TileSheet.SheetWidth;
                    int oldY = st.TileIndex / st.TileSheet.SheetWidth;

                    var layout = layouts.FirstOrDefault(l => oldX >= l.StartTileCol && oldX < l.EndTileCol);
                    if (layout != null)
                    {
                        // OffsetX/OffsetY 是合并贴图中的绝对像素偏移，直接除以 tile 尺寸即得目标 tile 坐标
                        int newX = (layout.OffsetX / st.TileSheet.TileWidth) + (oldX - layout.StartTileCol);
                        int newY = (layout.OffsetY / st.TileSheet.TileHeight) + oldY;
                        int newIndex = newY * newSheetWidth + newX;

                        var newTile = new StaticTile(layer, hostSheet, st.BlendMode, newIndex);
                        CopyProperties(st.Properties, newTile.Properties);
                        layer.Tiles[x, y] = newTile;
                    }
                }
            }
            else if (tile is AnimatedTile anim)
            {
                var newFrames = new StaticTile[anim.TileFrames.Length];
                bool anyRewritten = false;
                for (int i = 0; i < anim.TileFrames.Length; i++)
                {
                    var frame = anim.TileFrames[i];
                    if (layoutMap.TryGetValue(frame.TileSheet, out var layouts))
                    {
                        int oldX = frame.TileIndex % frame.TileSheet.SheetWidth;
                        int oldY = frame.TileIndex / frame.TileSheet.SheetWidth;

                        var layout = layouts.FirstOrDefault(l => oldX >= l.StartTileCol && oldX < l.EndTileCol);
                        if (layout != null)
                        {
                            int newX = (layout.OffsetX / frame.TileSheet.TileWidth) + (oldX - layout.StartTileCol);
                            int newY = (layout.OffsetY / frame.TileSheet.TileHeight) + oldY;
                            int newIndex = newY * newSheetWidth + newX;

                            newFrames[i] = new StaticTile(layer, hostSheet, frame.BlendMode, newIndex);
                            CopyProperties(frame.Properties, newFrames[i].Properties);
                            anyRewritten = true;
                        }
                        else
                        {
                            newFrames[i] = frame;
                        }
                    }
                    else
                    {
                        newFrames[i] = frame;
                    }
                }

                if (anyRewritten)
                {
                    var newAnim = new AnimatedTile(layer, newFrames, anim.FrameInterval);
                    newAnim.BlendMode = anim.BlendMode;
                    CopyProperties(anim.Properties, newAnim.Properties);
                    layer.Tiles[x, y] = newAnim;
                }
            }
        }

        private static void CopyProperties(IPropertyCollection source, IPropertyCollection dest)
        {
            foreach (var kv in source)
                dest[kv.Key] = kv.Value;
        }

        private static void CopyPixels(Color[] src, int srcW, int srcH, int srcX, int srcY, int copyW, int copyH,
                                       Color[] dst, int dstW, int dstH, int dstX, int dstY)
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

        // ---- 数据结构 ----

        private class VirtualSheetInfo
        {
            public TileSheet TileSheet { get; set; }
            public Texture2D Texture { get; set; }
            public int AlignedWidth { get; set; }
            public int AlignedHeight { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }

            // 切片信息（当贴图宽度超过宿主时）
            public int SourceX { get; set; }
            public int SourceY { get; set; }
            public int StartTileCol { get; set; }
            public int EndTileCol { get; set; }
        }

        private class VirtualSheetLayout
        {
            public TileSheet TileSheet { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
            public int AlignedWidth { get; set; }
            public int AlignedHeight { get; set; }
            public int StartTileCol { get; set; }
            public int EndTileCol { get; set; }
        }

        private class Shelf
        {
            public int Y { get; }
            public int Height { get; private set; }
            public int CurrentX { get; private set; }
            public int RemainingWidth => _maxWidth - CurrentX;
            private int _maxWidth;

            public Shelf(int startY)
            {
                Y = startY;
                Height = 0;
                CurrentX = 0;
            }

            public bool TryPlace(VirtualSheetInfo info, int maxWidth)
            {
                _maxWidth = maxWidth;
                if (info.AlignedWidth > RemainingWidth) return false;

                info.OffsetX = CurrentX;
                info.OffsetY = Y;
                CurrentX += info.AlignedWidth;
                Height = Math.Max(Height, info.AlignedHeight);
                return true;
            }
        }
    }
}
