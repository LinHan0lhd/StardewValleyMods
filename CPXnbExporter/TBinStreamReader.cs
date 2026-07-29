using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter
{
    /// <summary>
    /// 与 <see cref="TBinWriter"/> 对称的自定义 TBIN 流式读取器。
    /// 解析 <see cref="TBinWriter.SerializeTbin"/> 产生的字节数组，还原为 xTile Map。
    /// </summary>
    public static class TBinStreamReader
    {
        public static Map ReadMap(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms, Encoding.UTF8);

            // Header: "tBIN10" (6 bytes, 无 version byte)
            byte[] header = reader.ReadBytes(6);
            string headerStr = Encoding.UTF8.GetString(header);
            if (headerStr != "tBIN10")
                throw new InvalidDataException($"Invalid tBIN header: expected 'tBIN10', got '{headerStr}'");

            var map = new Map();

            // Map: Id, Description, Properties, TileSheets, Layers
            map.Id = ReadString(reader);
            map.Description = ReadString(reader);
            ReadProperties(reader, map.Properties);

            // TileSheets
            int tileSheetCount = reader.ReadInt32();
            for (int i = 0; i < tileSheetCount; i++)
            {
                ReadTileSheet(reader, map);
            }

            // Layers
            int layerCount = reader.ReadInt32();
            for (int i = 0; i < layerCount; i++)
            {
                ReadLayer(reader, map);
            }

            return map;
        }

        private static void ReadTileSheet(BinaryReader reader, Map map)
        {
            string id = ReadString(reader);
            string description = ReadString(reader);
            string imageSource = ReadString(reader);

            int sheetWidth = reader.ReadInt32();
            int sheetHeight = reader.ReadInt32();
            int tileWidth = reader.ReadInt32();
            int tileHeight = reader.ReadInt32();
            int marginWidth = reader.ReadInt32();
            int marginHeight = reader.ReadInt32();
            int spacingWidth = reader.ReadInt32();
            int spacingHeight = reader.ReadInt32();

            var sheetSize = new xTile.Dimensions.Size { Width = sheetWidth, Height = sheetHeight };
            var tileSize = new xTile.Dimensions.Size { Width = tileWidth, Height = tileHeight };
            var tileSheet = new TileSheet(id, map, imageSource, sheetSize, tileSize);

            tileSheet.Margin = new xTile.Dimensions.Size { Width = marginWidth, Height = marginHeight };
            tileSheet.Spacing = new xTile.Dimensions.Size { Width = spacingWidth, Height = spacingHeight };
            tileSheet.Description = description;

            ReadProperties(reader, tileSheet.Properties);

            map.AddTileSheet(tileSheet);
        }

        private static void ReadLayer(BinaryReader reader, Map map)
        {
            string id = ReadString(reader);
            bool visible = reader.ReadByte() != 0;
            string description = ReadString(reader);

            int layerWidth = reader.ReadInt32();
            int layerHeight = reader.ReadInt32();
            int tileWidth = reader.ReadInt32();
            int tileHeight = reader.ReadInt32();

            var layerSize = new xTile.Dimensions.Size { Width = layerWidth, Height = layerHeight };
            var tSize = new xTile.Dimensions.Size { Width = tileWidth, Height = tileHeight };
            var layer = new Layer(id, map, layerSize, tSize);
            layer.Visible = visible;
            layer.Description = description;

            ReadProperties(reader, layer.Properties);

            string currentTileSheetId = null;

            for (int y = 0; y < layerHeight; y++)
            {
                int x = 0;
                while (x < layerWidth)
                {
                    byte tag = reader.ReadByte();

                    if (tag == (byte)'N')
                    {
                        int nullCount = reader.ReadInt32();
                        for (int k = 0; k < nullCount && x < layerWidth; k++)
                        {
                            layer.Tiles[x, y] = null;
                            x++;
                        }
                        continue; // 跳过下面的 S/A 处理
                    }
                    else if (tag == (byte)'T')
                    {
                        currentTileSheetId = ReadString(reader);
                        // 'T' 后必须紧跟 'S' 或 'A'，继续读取下一个 tag
                        tag = reader.ReadByte();
                    }

                    if (tag == (byte)'S')
                    {
                        int tileIndex = reader.ReadInt32();
                        byte blendMode = reader.ReadByte();
                        var props = new Dictionary<string, PropertyValue>();
                        ReadProperties(reader, props);

                        TileSheet ts = FindTileSheet(map, currentTileSheetId);
                        var st = new StaticTile(layer, ts, (BlendMode)blendMode, tileIndex);
                        foreach (var p in props)
                            st.Properties[p.Key] = p.Value;

                        layer.Tiles[x, y] = st;
                        x++;
                    }
                    else if (tag == (byte)'A')
                    {
                        int frameInterval = reader.ReadInt32();
                        int frameCount = reader.ReadInt32();

                        var frames = new StaticTile[frameCount];
                        string frameCurrentTileSheetId = null;

                        for (int f = 0; f < frameCount; f++)
                        {
                            byte frameTag = reader.ReadByte();
                            if (frameTag == (byte)'T')
                            {
                                frameCurrentTileSheetId = ReadString(reader);
                                frameTag = reader.ReadByte();
                            }

                            if (frameTag == (byte)'S')
                            {
                                int frameTileIndex = reader.ReadInt32();
                                byte frameBlendMode = reader.ReadByte();
                                var frameProps = new Dictionary<string, PropertyValue>();
                                ReadProperties(reader, frameProps);

                                TileSheet frameTs = FindTileSheet(map, frameCurrentTileSheetId);
                                var frame = new StaticTile(layer, frameTs, (BlendMode)frameBlendMode, frameTileIndex);
                                foreach (var p in frameProps)
                                    frame.Properties[p.Key] = p.Value;

                                frames[f] = frame;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected animated frame tag: {(char)frameTag} (0x{frameTag:X2})");
                            }
                        }

                        var animTile = new AnimatedTile(layer, frames, frameInterval);

                        var animProps = new Dictionary<string, PropertyValue>();
                        ReadProperties(reader, animProps);
                        foreach (var p in animProps)
                            animTile.Properties[p.Key] = p.Value;

                        layer.Tiles[x, y] = animTile;
                        x++;
                    }
                    else
                    {
                        throw new InvalidDataException($"Unexpected tile tag: {(char)tag} (0x{tag:X2}) at layer ({x},{y})");
                    }
                }
            }

            map.AddLayer(layer);
        }

        private static TileSheet FindTileSheet(Map map, string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (var ts in map.TileSheets)
            {
                if (ts.Id == id)
                    return ts;
            }
            return null;
        }

        private static void ReadProperties(BinaryReader reader, IDictionary<string, PropertyValue> properties)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = ReadString(reader);
                PropertyValue value = ReadPropertyValue(reader);
                properties[key] = value;
            }
        }

        private static PropertyValue ReadPropertyValue(BinaryReader reader)
        {
            byte type = reader.ReadByte();
            return type switch
            {
                0 => reader.ReadByte() != 0,        // bool -> PropertyValue (implicit)
                1 => reader.ReadInt32(),             // int -> PropertyValue (implicit)
                2 => reader.ReadSingle(),            // float -> PropertyValue (implicit)
                3 => ReadString(reader),             // string -> PropertyValue (implicit)
                _ => throw new InvalidDataException($"Unknown property value type: {type}")
            };
        }

        private static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length <= 0) return "";
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
