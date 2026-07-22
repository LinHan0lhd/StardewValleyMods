using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter
{
    public static class TBinWriter
    {
        /// <summary>
        /// 路径规范化函数。用于把 SMAPI 虚拟资产路径（如 SMAPI/模组ID/...）
        /// 映射为游戏 Content 可识别的路径（如 Mods/模组ID/...）。
        /// 在写入 TBIN 的 tilesheet ImageSource 时调用。
        /// 由 ModEntry 在导出开始时设置，结束时清理。
        /// </summary>
        public static Func<string, string> PathNormalizer { get; set; }

        /// <summary>
        /// 当前正在导出的地图 assetName（如 "Maps/ArchaeologyHouse"）。
        /// 用于把 tilesheet 的 ImageSource 从"相对于 Content 的完整路径"
        /// 转换为"相对于地图所在目录的相对路径"。
        ///
        /// 原因：游戏加载地图时，XnaDisplayDevice.LoadTileSheet 会以地图所在目录
        /// 为基准拼接 tilesheet 路径。如地图在 Maps/xxx，tilesheet 写 "Maps/paths"
        /// 会被拼成 "Maps/Maps/paths.xnb"（错误），应写 "paths" 拼成 "Maps/paths.xnb"。
        /// 跨目录引用（如 Mods/...）需要加 "../" 前缀跳出地图目录。
        ///
        /// 由 ModEntry 在每次调用 SerializeTbin 前设置（每张地图不同）。
        /// </summary>
        public static string MapAssetName { get; set; }

        /// <summary>
        /// PropertyValue 内部实例字段（反射缓存）。
        /// 用于在不依赖 TryGetValue&lt;T&gt; API 的情况下读取属性原始值
        /// （xTile 是闭源库，不同版本该方法签名/可用性不一致）。
        /// </summary>
        private static readonly FieldInfo[] _propertyValueFields =
            typeof(PropertyValue).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static byte[] SerializeTbin(Map map)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8);

            // Header: "tBIN10" (6 bytes, 无 version byte)
            writer.Write(Encoding.UTF8.GetBytes("tBIN10"));

            // Map: Id, Description, Properties, TileSheets, Layers
            WriteString(writer, map.Id ?? "");
            WriteString(writer, map.Description ?? "");
            WriteProperties(writer, map.Properties);

            // TileSheets
            writer.Write(map.TileSheets.Count);
            foreach (var tileSheet in map.TileSheets)
            {
                WriteTileSheet(writer, tileSheet);
            }

            // Layers
            writer.Write(map.Layers.Count);
            foreach (var layer in map.Layers)
            {
                WriteLayer(writer, layer);
            }

            return ms.ToArray();
        }

        private static void WriteTileSheet(BinaryWriter writer, TileSheet tileSheet)
        {
            // Id, Description, Image
            WriteString(writer, tileSheet.Id ?? "");
            WriteString(writer, tileSheet.Description ?? "");
            WriteString(writer, GetImageSourceWithoutExtension(tileSheet));

            // SheetSize (IntVector2: x=SheetWidth, y=SheetHeight)
            writer.Write(tileSheet.SheetWidth);
            writer.Write(tileSheet.SheetHeight);

            // TileSize (IntVector2: x=TileWidth, y=TileHeight)
            writer.Write(tileSheet.TileWidth);
            writer.Write(tileSheet.TileHeight);

            // Margin (IntVector2: x=Margin.Width, y=Margin.Height)
            writer.Write(tileSheet.Margin.Width);
            writer.Write(tileSheet.Margin.Height);

            // Spacing (IntVector2: x=Spacing.Width, y=Spacing.Height)
            writer.Write(tileSheet.Spacing.Width);
            writer.Write(tileSheet.Spacing.Height);

            // Properties
            WriteProperties(writer, tileSheet.Properties);
        }

        private static void WriteLayer(BinaryWriter writer, Layer layer)
        {
            // Id
            WriteString(writer, layer.Id ?? "");

            // Visible (byte: 0 or 1)
            writer.Write((byte)(layer.Visible ? 1 : 0));

            // Description
            WriteString(writer, layer.Description ?? "");

            // LayerSize (IntVector2: x=LayerWidth, y=LayerHeight)
            writer.Write(layer.LayerWidth);
            writer.Write(layer.LayerHeight);

            // TileSize (IntVector2: x=TileWidth, y=TileHeight)
            writer.Write(layer.TileWidth);
            writer.Write(layer.TileHeight);

            // Properties
            WriteProperties(writer, layer.Properties);

            // Tiles (逐行写入，支持压缩)
            string currentTileSheetId = "";
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                int x = 0;
                while (x < layer.LayerWidth)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile == null)
                    {
                        // 计算连续空tile数量，用 'N' + count 压缩
                        int nullCount = 0;
                        while (x < layer.LayerWidth && layer.Tiles[x, y] == null)
                        {
                            nullCount++;
                            x++;
                        }
                        writer.Write((byte)'N');
                        writer.Write(nullCount);
                    }
                    else if (tile is StaticTile st)
                    {
                        // 检查tilesheet是否改变，改变则写入 'T'
                        string tsId = st.TileSheet?.Id ?? "";
                        if (tsId != currentTileSheetId)
                        {
                            writer.Write((byte)'T');
                            WriteString(writer, tsId);
                            currentTileSheetId = tsId;
                        }
                        // 写入 'S' + StaticTile (TileIndex + BlendMode + Properties)
                        writer.Write((byte)'S');
                        writer.Write(st.TileIndex);
                        writer.Write((byte)st.BlendMode);
                        WriteProperties(writer, st.Properties);
                        x++;
                    }
                    else if (tile is AnimatedTile anim)
                    {
                        // 写入 'A' + AnimatedTile
                        writer.Write((byte)'A');
                        WriteAnimatedTile(writer, anim, ref currentTileSheetId);
                        x++;
                    }
                    else
                    {
                        // 未知tile类型，当作空tile处理
                        writer.Write((byte)'N');
                        writer.Write(1);
                        x++;
                    }
                }
            }
        }

        private static void WriteAnimatedTile(BinaryWriter writer, AnimatedTile anim, ref string layerCurrentTileSheetId)
        {
            // FrameInterval (int32)
            writer.Write(anim.FrameInterval);

            // FrameCount (int32)
            writer.Write(anim.TileFrames.Length);

            // Frames: 'T' + tilesheetId / 'S' + StaticTile
            string frameCurrentTileSheetId = "";
            foreach (var frame in anim.TileFrames)
            {
                string frameTsId = frame.TileSheet?.Id ?? "";
                if (frameTsId != frameCurrentTileSheetId)
                {
                    writer.Write((byte)'T');
                    WriteString(writer, frameTsId);
                    frameCurrentTileSheetId = frameTsId;
                }
                // 写入 'S' + StaticTile (TileIndex + BlendMode + Properties)
                writer.Write((byte)'S');
                writer.Write(frame.TileIndex);
                writer.Write((byte)anim.BlendMode);
                WriteProperties(writer, frame.Properties);
            }

            // AnimatedTile Properties
            WriteProperties(writer, anim.Properties);
        }

        private static void WriteProperties(BinaryWriter writer, IDictionary<string, PropertyValue> properties)
        {
            writer.Write(properties.Count);
            foreach (var prop in properties)
            {
                WriteString(writer, prop.Key ?? "");
                WritePropertyValue(writer, prop.Value);
            }
        }

        private static void WritePropertyValue(BinaryWriter writer, PropertyValue value)
        {
            if (value == null)
            {
                writer.Write((byte)3); // string
                WriteString(writer, "");
                return;
            }

            // 用反射获取 PropertyValue 内部存储的原始值。
            // 不依赖 TryGetValue<T>（xTile 闭源，不同版本该泛型方法可能不存在或签名不同）。
            //
            // 重要：PropertyValue 内部有一个类型标记字段（tag/type/kind 等，枚举或 int 类型），
            // 用于区分 bool/int/float/string。如果直接遍历字段取第一个匹配 bool/int/float/string 的，
            // 会错误地把"类型标记字段"当作"值字段"读取（例如 tag=3 被当成 int 值 3 写入）。
            // 因此必须跳过名字含 tag/type/kind 的字段，只取真正的值字段。
            object inner = null;
            foreach (var f in _propertyValueFields)
            {
                string fname = f.Name ?? "";
                string fnameLower = fname.ToLowerInvariant();
                // 跳过类型标记字段（tag/type/kind/discriminator/case）
                if (fnameLower.Contains("tag") ||
                    fnameLower.Contains("type") ||
                    fnameLower.Contains("kind") ||
                    fnameLower.Contains("discriminator") ||
                    fnameLower.Contains("case"))
                    continue;

                try
                {
                    var v = f.GetValue(value);
                    if (v is bool || v is int || v is float || v is string)
                    {
                        inner = v;
                        break;
                    }
                }
                catch { /* 忽略反射访问异常 */ }
            }

            switch (inner)
            {
                case bool b:
                    writer.Write((byte)0); // bool
                    writer.Write((byte)(b ? 1 : 0));
                    break;
                case int i:
                    writer.Write((byte)1); // int
                    writer.Write(i);
                    break;
                case float f:
                    writer.Write((byte)2); // float
                    writer.Write(f);
                    break;
                case string s:
                    writer.Write((byte)3); // string
                    WriteString(writer, s ?? "");
                    break;
                default:
                    // fallback: 反射取不到值字段时当作字符串处理。
                    // PropertyValue 重载了到 string 的隐式转换（op_Implicit），ToString/隐式转换会返回实际值字符串。
                    // 游戏读取属性大多用 string 形式（见 FrameworkExtensions.TryGetValue），仍可正常工作。
                    writer.Write((byte)3);
                    WriteString(writer, value.ToString() ?? "");
                    break;
            }
        }

        /// <summary>
        /// 写入字符串：int32 length + UTF8 bytes
        /// 这是 xTile tBIN 格式使用的字符串编码方式（不是 7-bit encoded）
        /// </summary>
        private static void WriteString(BinaryWriter writer, string str)
        {
            if (str == null) str = "";
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes.Length);  // int32 length
            writer.Write(bytes);          // bytes
        }

        private static string GetImageSourceWithoutExtension(TileSheet tileSheet)
        {
            string imageSource = tileSheet.ImageSource;
            if (string.IsNullOrEmpty(imageSource))
                return imageSource;

            // 应用路径规范化（如 SMAPI/模组ID/... → Mods/模组ID/...）
            if (PathNormalizer != null)
                imageSource = PathNormalizer(imageSource);

            string ext = Path.GetExtension(imageSource);
            if (!string.IsNullOrEmpty(ext) &&
                (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                 ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                 ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                 ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                 ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)))
            {
                imageSource = imageSource.Substring(0, imageSource.Length - ext.Length);
            }

            // 转换为相对于地图所在目录的路径。
            // 游戏加载地图时，XnaDisplayDevice 以地图目录为基准拼接 tilesheet 路径，
            // 所以 tbin 里必须存相对路径，不能存完整路径。
            // 例如地图 Maps/ArchaeologyHouse：
            //   "Maps/paths" → "paths"（去掉地图目录前缀）
            //   "Mods/xxx/yyy" → "../Mods/xxx/yyy"（跨目录加 ../）
            imageSource = MakeRelativeToMapDir(imageSource, MapAssetName);

            return imageSource;
        }

        /// <summary>
        /// 把 tilesheet ImageSource 从"相对于 Content 的完整路径"
        /// 转换为"相对于地图所在目录的相对路径"。
        ///
        /// ../ 路径方式（原版兼容）：
        ///   原版 ContentManager.Load("../Mods/x") → Content/../Mods/x → <游戏根>/Mods/x.xnb
        ///   文件放在游戏根目录的 Mods/ 下。
        ///
        /// 例如地图 assetName = "Maps/ArchaeologyHouse"（地图目录 = "Maps"）：
        ///   "Maps/paths"   → "paths"          （同级，去前缀）
        ///   "Mods/xxx/yyy" → "../Mods/xxx/yyy"（跨目录，加 ../ 跳出 Maps）
        /// </summary>
        private static string MakeRelativeToMapDir(string imagePath, string mapAssetName)
        {
            if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(mapAssetName))
                return imagePath;

            // 统一用 / 分隔
            string img = imagePath.Replace('\\', '/');
            string map = mapAssetName.Replace('\\', '/');

            // 获取地图所在目录（assetName 去掉最后一段）
            int lastSlash = map.LastIndexOf('/');
            if (lastSlash < 0)
                return img; // 地图在根目录，ImageSource 不用改
            string mapDir = map.Substring(0, lastSlash); // 如 "Maps" 或 "Maps/Mines"

            // 如果 ImageSource 以地图目录 + "/" 开头，直接去掉前缀
            if (img.StartsWith(mapDir + "/", StringComparison.OrdinalIgnoreCase))
                return img.Substring(mapDir.Length + 1);

            // 否则计算相对路径：找公共前缀，然后补 ../
            string[] imgParts = img.Split('/');
            string[] dirParts = mapDir.Split('/');

            int common = 0;
            while (common < imgParts.Length - 1 && common < dirParts.Length &&
                   imgParts[common].Equals(dirParts[common], StringComparison.OrdinalIgnoreCase))
                common++;

            var result = new StringBuilder();
            for (int i = common; i < dirParts.Length; i++)
                result.Append("../");
            for (int i = common; i < imgParts.Length; i++)
            {
                if (i > common)
                    result.Append('/');
                result.Append(imgParts[i]);
            }
            return result.ToString();
        }

        public static void WriteMapXnb(Stream fs, Map map, char platform)
        {
            XnbMapWriter.WriteMapXnb(fs, map, platform);
        }

        public static void WriteMapTbin(Stream fs, Map map)
        {
            byte[] data = SerializeTbin(map);
            fs.Write(data, 0, data.Length);
        }
    }
}
