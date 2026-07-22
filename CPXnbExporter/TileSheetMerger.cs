using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
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
        /// </summary>
        public const string DefaultHostAssetName = "Maps/paths";

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
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="hostAssetName">宿主 tilesheet 的 Content 路径，如 "Maps/paths"</param>
        /// <param name="helper">SMAPI helper</param>
        /// <param name="monitor">日志</param>
        /// <returns>合并后的宿主贴图；如果没有虚拟 tilesheet 需要合并，返回 null</returns>
        public static Texture2D MergeVirtualTileSheets(Map map, string hostAssetName, IModHelper helper, IMonitor monitor)
        {
            if (map == null || helper == null) return null;

            // 收集虚拟 tilesheet
            var virtualSheets = map.TileSheets.Where(IsVirtualTileSheet).ToList();
            if (virtualSheets.Count == 0) return null;

            monitor?.Log($"  ↳ 发现 {virtualSheets.Count} 个虚拟 tilesheet，准备合并到 {hostAssetName}", LogLevel.Trace);

            // 找到或创建宿主 tilesheet
            string hostId = hostAssetName.Replace('\\', '/');
            int lastSlash = hostId.LastIndexOf('/');
            if (lastSlash >= 0) hostId = hostId.Substring(lastSlash + 1);

            TileSheet hostSheet = map.TileSheets.FirstOrDefault(ts =>
                ts.ImageSource?.Replace('\\', '/').Equals(hostAssetName, StringComparison.OrdinalIgnoreCase) == true
                || ts.Id.Equals(hostId, StringComparison.OrdinalIgnoreCase));

            // 加载宿主贴图
            Texture2D hostTexture;
            try
            {
                hostTexture = helper.GameContent.Load<Texture2D>(hostAssetName);
            }
            catch (Exception ex)
            {
                monitor?.Log($"  ↳ 合并失败：无法加载宿主贴图 {hostAssetName}: {ex.Message}", LogLevel.Trace);
                return null;
            }

            int tileWidth = hostSheet != null ? hostSheet.TileWidth : virtualSheets[0].TileWidth;
            int tileHeight = hostSheet != null ? hostSheet.TileHeight : virtualSheets[0].TileHeight;

            if (tileWidth <= 0 || tileHeight <= 0)
            {
                monitor?.Log($"  ↳ 合并失败：tile 尺寸无效 {tileWidth}x{tileHeight}", LogLevel.Trace);
                return null;
            }

            int hostPixelW = hostTexture.Width;
            int hostPixelH = hostTexture.Height;

            // 保持宿主宽度不变，否则宿主原有 tile 的 index 会变化。
            // 只在高度方向扩展，在宿主下方堆叠虚拟贴图。
            int newPixelW = hostPixelW;
            int newPixelH = hostPixelH;
            var layout = new List<VirtualSheetLayout>();

            foreach (var vSheet in virtualSheets)
            {
                Texture2D vTex;
                try
                {
                    vTex = helper.GameContent.Load<Texture2D>(vSheet.ImageSource);
                }
                catch (Exception ex)
                {
                    monitor?.Log($"  ↳ 合并失败：无法加载虚拟贴图 {vSheet.ImageSource}: {ex.Message}", LogLevel.Trace);
                    return null;
                }

                // 检查 tile 尺寸一致性
                if (vSheet.TileWidth != tileWidth || vSheet.TileHeight != tileHeight)
                {
                    monitor?.Log($"  ↳ 合并失败：虚拟 tilesheet {vSheet.Id} 的 tile 尺寸 {vSheet.TileWidth}x{vSheet.TileHeight} 与宿主 {tileWidth}x{tileHeight} 不一致", LogLevel.Trace);
                    return null;
                }

                // 计算对齐后的尺寸（tile 的整数倍）
                int alignedW = ((vTex.Width + tileWidth - 1) / tileWidth) * tileWidth;
                int alignedH = ((vTex.Height + tileHeight - 1) / tileHeight) * tileHeight;

                if (alignedW > hostPixelW)
                {
                    monitor?.Log($"  ↳ 合并失败：虚拟贴图 {vSheet.Id} 宽度 {alignedW} 超过宿主宽度 {hostPixelW}。请改用更大的宿主 tilesheet（如 Maps/spring_townInterior）。", LogLevel.Trace);
                    return null;
                }

                layout.Add(new VirtualSheetLayout
                {
                    TileSheet = vSheet,
                    Texture = vTex,
                    AlignedWidth = alignedW,
                    AlignedHeight = alignedH,
                    OffsetY = newPixelH
                });

                newPixelH += alignedH;
            }

            // 创建新的合并贴图
            var device = hostTexture.GraphicsDevice;
            var mergedTexture = new Texture2D(device, newPixelW, newPixelH);
            var mergedPixels = new Color[newPixelW * newPixelH];

            // 初始透明
            Array.Clear(mergedPixels, 0, mergedPixels.Length);

            // 复制宿主像素
            var hostPixels = new Color[hostPixelW * hostPixelH];
            hostTexture.GetData(hostPixels);
            CopyPixels(hostPixels, hostPixelW, hostPixelH, mergedPixels, newPixelW, newPixelH, 0, 0);

            // 复制虚拟贴图到对应位置
            foreach (var l in layout)
            {
                var vPixels = new Color[l.Texture.Width * l.Texture.Height];
                l.Texture.GetData(vPixels);
                CopyPixels(vPixels, l.Texture.Width, l.Texture.Height, mergedPixels, newPixelW, newPixelH, 0, l.OffsetY);
            }

            mergedTexture.SetData(mergedPixels);

            // 更新宿主 tilesheet
            int newSheetWidth = newPixelW / tileWidth;
            int newSheetHeight = newPixelH / tileHeight;
            int hostSheetHeightTiles = hostPixelH / tileHeight;

            if (hostSheet == null)
            {
                hostSheet = new TileSheet(hostId, map, hostAssetName, new Size { Width = newSheetWidth, Height = newSheetHeight }, new Size { Width = tileWidth, Height = tileHeight });
                map.AddTileSheet(hostSheet);
            }
            else
            {
                hostSheet.ImageSource = hostAssetName;
                hostSheet.SheetWidth = newSheetWidth;
                hostSheet.SheetHeight = newSheetHeight;
            }

            // 重写地图 tile 引用
            var indexMap = new Dictionary<TileSheet, VirtualSheetLayout>();
            for (int i = 0; i < virtualSheets.Count; i++)
            {
                indexMap[virtualSheets[i]] = layout[i];
            }

            foreach (Layer layer in map.Layers)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        RewriteTile(layer, x, y, hostSheet, indexMap, hostSheetHeightTiles, newSheetWidth);
                    }
                }
            }

            // 移除虚拟 tilesheet
            foreach (var vSheet in virtualSheets)
            {
                map.RemoveTileSheet(vSheet);
            }

            monitor?.Log($"  ↳ 合并完成：宿主扩展为 {newSheetWidth}x{newSheetHeight} tiles，合并了 {virtualSheets.Count} 个虚拟 tilesheet", LogLevel.Trace);

            return mergedTexture;
        }

        private static void RewriteTile(Layer layer, int x, int y, TileSheet hostSheet, Dictionary<TileSheet, VirtualSheetLayout> layoutMap, int hostSheetHeightTiles, int newSheetWidth)
        {
            var tile = layer.Tiles[x, y];
            if (tile == null) return;

            if (tile is StaticTile st)
            {
                if (layoutMap.TryGetValue(st.TileSheet, out var layout))
                {
                    int oldX = st.TileIndex % layout.TileSheet.SheetWidth;
                    int oldY = st.TileIndex / layout.TileSheet.SheetWidth;
                    int newY = hostSheetHeightTiles + (layout.OffsetY / layout.TileSheet.TileHeight) + oldY;
                    int newIndex = newY * newSheetWidth + oldX;

                    var newTile = new StaticTile(layer, hostSheet, st.BlendMode, newIndex);
                    CopyProperties(st.Properties, newTile.Properties);
                    layer.Tiles[x, y] = newTile;
                }
            }
            else if (tile is AnimatedTile anim)
            {
                var newFrames = new StaticTile[anim.TileFrames.Length];
                bool anyRewritten = false;
                for (int i = 0; i < anim.TileFrames.Length; i++)
                {
                    var frame = anim.TileFrames[i];
                    if (layoutMap.TryGetValue(frame.TileSheet, out var layout))
                    {
                        int oldX = frame.TileIndex % layout.TileSheet.SheetWidth;
                        int oldY = frame.TileIndex / layout.TileSheet.SheetWidth;
                        int newY = hostSheetHeightTiles + (layout.OffsetY / layout.TileSheet.TileHeight) + oldY;
                        int newIndex = newY * newSheetWidth + oldX;

                        newFrames[i] = new StaticTile(layer, hostSheet, frame.BlendMode, newIndex);
                        CopyProperties(frame.Properties, newFrames[i].Properties);
                        anyRewritten = true;
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

        private static void CopyProperties(IDictionary<string, PropertyValue> source, IDictionary<string, PropertyValue> dest)
        {
            foreach (var kv in source)
            {
                dest[kv.Key] = kv.Value;
            }
        }

        private static void CopyPixels(Color[] src, int srcW, int srcH, Color[] dst, int dstW, int dstH, int dstX, int dstY)
        {
            for (int y = 0; y < srcH; y++)
            {
                for (int x = 0; x < srcW; x++)
                {
                    int dstIndex = (dstY + y) * dstW + (dstX + x);
                    int srcIndex = y * srcW + x;
                    if (dstIndex >= 0 && dstIndex < dst.Length)
                    {
                        dst[dstIndex] = src[srcIndex];
                    }
                }
            }
        }

        private class VirtualSheetLayout
        {
            public TileSheet TileSheet { get; set; }
            public Texture2D Texture { get; set; }
            public int AlignedWidth { get; set; }
            public int AlignedHeight { get; set; }
            public int OffsetY { get; set; }
        }
    }
}
