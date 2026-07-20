using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter
{
    public static class TBinWriter
    {
        public static byte[] SerializeTbin(Map map)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8);

            // Header
            writer.Write(Encoding.UTF8.GetBytes("tBIN10"));
            writer.Write((byte)0);

            // Build string table
            var stringTable = new List<string>();
            var stringIndex = new Dictionary<string, int>();

            void AddString(string str)
            {
                if (string.IsNullOrEmpty(str)) return;
                if (!stringIndex.ContainsKey(str))
                {
                    stringIndex[str] = stringTable.Count;
                    stringTable.Add(str);
                }
            }

            // Collect all strings
            AddString(map.Id);
            foreach (var prop in map.Properties)
            {
                AddString(prop.Key);
                AddString(prop.Value?.ToString() ?? "");
            }
            foreach (var tileSheet in map.TileSheets)
            {
                AddString(tileSheet.Id);
                AddString(tileSheet.ImageSource);
                AddString(tileSheet.Description);
                foreach (var prop in tileSheet.Properties)
                {
                    AddString(prop.Key);
                    AddString(prop.Value?.ToString() ?? "");
                }
            }
            foreach (var layer in map.Layers)
            {
                AddString(layer.Id);
                foreach (var prop in layer.Properties)
                {
                    AddString(prop.Key);
                    AddString(prop.Value?.ToString() ?? "");
                }
                for (var y = 0; y < layer.LayerHeight; y++)
                {
                    for (var x = 0; x < layer.LayerWidth; x++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile != null)
                        {
                            foreach (var prop in tile.Properties)
                            {
                                AddString(prop.Key);
                                AddString(prop.Value?.ToString() ?? "");
                            }
                            if (tile is AnimatedTile anim)
                            {
                                foreach (var frame in anim.TileFrames)
                                {
                                    foreach (var prop in frame.Properties)
                                    {
                                        AddString(prop.Key);
                                        AddString(prop.Value?.ToString() ?? "");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Write string table
            writer.Write(stringTable.Count);
            foreach (var str in stringTable)
            {
                writer.Write(str);
            }

            // Write map properties
            writer.Write(map.Properties.Count);
            foreach (var prop in map.Properties)
            {
                writer.Write(stringIndex[prop.Key]);
                writer.Write(stringIndex.ContainsKey(prop.Value?.ToString() ?? "")
                    ? stringIndex[prop.Value?.ToString() ?? ""]
                    : -1);
            }

            // Build TileSheet index map
            var tileSheetIndexMap = new Dictionary<TileSheet, int>();
            for (int i = 0; i < map.TileSheets.Count; i++)
                tileSheetIndexMap[map.TileSheets[i]] = i;

            // Write tile sheets
            writer.Write(map.TileSheets.Count);
            foreach (var tileSheet in map.TileSheets)
            {
                writer.Write(stringIndex[tileSheet.Id]);
                writer.Write(stringIndex[tileSheet.ImageSource]);
                writer.Write(tileSheet.SheetWidth);
                writer.Write(tileSheet.SheetHeight);
                writer.Write(tileSheet.TileWidth);
                writer.Write(tileSheet.TileHeight);
                writer.Write(tileSheet.Margin.Width);
                writer.Write(tileSheet.Spacing.Width);
                writer.Write(string.IsNullOrEmpty(tileSheet.Description) ? -1 : stringIndex[tileSheet.Description]);

                writer.Write(tileSheet.Properties.Count);
                foreach (var prop in tileSheet.Properties)
                {
                    writer.Write(stringIndex[prop.Key]);
                    writer.Write(stringIndex.ContainsKey(prop.Value?.ToString() ?? "")
                        ? stringIndex[prop.Value?.ToString() ?? ""]
                        : -1);
                }
            }

            // Write layers
            writer.Write(map.Layers.Count);
            foreach (var layer in map.Layers)
            {
                writer.Write(stringIndex[layer.Id]);
                writer.Write(layer.LayerWidth);
                writer.Write(layer.LayerHeight);
                writer.Write(layer.TileWidth);
                writer.Write(layer.TileHeight);
                writer.Write(layer.Visible);

                writer.Write(layer.Properties.Count);
                foreach (var prop in layer.Properties)
                {
                    writer.Write(stringIndex[prop.Key]);
                    writer.Write(stringIndex.ContainsKey(prop.Value?.ToString() ?? "")
                        ? stringIndex[prop.Value?.ToString() ?? ""]
                        : -1);
                }

                for (var y = 0; y < layer.LayerHeight; y++)
                {
                    for (var x = 0; x < layer.LayerWidth; x++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile == null)
                        {
                            writer.Write((byte)'N');
                        }
                        else if (tile is StaticTile st)
                        {
                            writer.Write((byte)'S');
                            writer.Write(tileSheetIndexMap.ContainsKey(st.TileSheet) ? tileSheetIndexMap[st.TileSheet] : 0);
                            writer.Write(st.TileIndex);
                            writer.Write((byte)st.BlendMode);

                            writer.Write(st.Properties.Count);
                            foreach (var prop in st.Properties)
                            {
                                writer.Write(stringIndex[prop.Key]);
                                writer.Write(stringIndex.ContainsKey(prop.Value?.ToString() ?? "")
                                    ? stringIndex[prop.Value?.ToString() ?? ""]
                                    : -1);
                            }
                        }
                        else if (tile is AnimatedTile anim)
                        {
                            writer.Write((byte)'A');
                            writer.Write((ushort)anim.TileFrames.Length);
                            writer.Write(anim.FrameInterval);
                            writer.Write((byte)anim.BlendMode);
                            foreach (var frame in anim.TileFrames)
                            {
                                writer.Write(tileSheetIndexMap.ContainsKey(frame.TileSheet) ? tileSheetIndexMap[frame.TileSheet] : 0);
                                writer.Write(frame.TileIndex);
                            }

                            writer.Write(anim.Properties.Count);
                            foreach (var prop in anim.Properties)
                            {
                                writer.Write(stringIndex[prop.Key]);
                                writer.Write(stringIndex.ContainsKey(prop.Value?.ToString() ?? "")
                                    ? stringIndex[prop.Value?.ToString() ?? ""]
                                    : -1);
                            }
                        }
                    }
                }
            }

            return ms.ToArray();
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
