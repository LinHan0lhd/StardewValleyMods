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
            // 遍历所有实例字段，只取值类型为 bool/int/float/string 的字段（跳过 enum 等类型字段）。
            object inner = null;
            foreach (var f in _propertyValueFields)
            {
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
                    // fallback: 反射取不到值时当作字符串处理。
                    // 游戏读取属性大多用 string（xTile 隐式转换支持），仍可正常工作。
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
                return imageSource.Substring(0, imageSource.Length - ext.Length);
            }
            return imageSource;
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
