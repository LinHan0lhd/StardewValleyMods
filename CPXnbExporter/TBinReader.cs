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
    public static class TBinReader
    {
        public static Map ReadTbin(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms, Encoding.UTF8);

            // Header
            var header = reader.ReadBytes(6);
            var version = reader.ReadByte();

            // String table
            var stringCount = reader.ReadInt32();
            var stringTable = new List<string>();
            for (var i = 0; i < stringCount; i++)
            {
                stringTable.Add(reader.ReadString());
            }

            // Map properties
            var map = new Map();
            var mapPropCount = reader.ReadInt32();
            for (var i = 0; i < mapPropCount; i++)
            {
                var key = stringTable[reader.ReadInt32()];
                var valueIndex = reader.ReadInt32();
                map.Properties[key] = valueIndex >= 0 ? stringTable[valueIndex] : "";
            }

            // Tile sheets
            var tileSheetCount = reader.ReadInt32();
            for (var i = 0; i < tileSheetCount; i++)
            {
                var id = stringTable[reader.ReadInt32()];
                var imageSource = stringTable[reader.ReadInt32()];
                var sheetWidth = reader.ReadInt32();
                var sheetHeight = reader.ReadInt32();
                var tileWidth = reader.ReadInt32();
                var tileHeight = reader.ReadInt32();
                var margin = reader.ReadInt32();
                var spacing = reader.ReadInt32();
                var descIndex = reader.ReadInt32();

                // FIX: Use Size object initializer
                var sheetSize = new xTile.Dimensions.Size { Width = sheetWidth, Height = sheetHeight };
                var tileSize = new xTile.Dimensions.Size { Width = tileWidth, Height = tileHeight };
                var tileSheet = new TileSheet(id, map, imageSource, sheetSize, tileSize);

                tileSheet.Margin = new xTile.Dimensions.Size { Width = margin, Height = margin };
                tileSheet.Spacing = new xTile.Dimensions.Size { Width = spacing, Height = spacing };
                if (descIndex >= 0)
                    tileSheet.Description = stringTable[descIndex];

                var tsPropCount = reader.ReadInt32();
                for (var j = 0; j < tsPropCount; j++)
                {
                    var key = stringTable[reader.ReadInt32()];
                    var valueIndex = reader.ReadInt32();
                    tileSheet.Properties[key] = valueIndex >= 0 ? stringTable[valueIndex] : "";
                }

                map.AddTileSheet(tileSheet);
            }

            // Layers
            var layerCount = reader.ReadInt32();
            for (var i = 0; i < layerCount; i++)
            {
                var id = stringTable[reader.ReadInt32()];
                var layerWidth = reader.ReadInt32();
                var layerHeight = reader.ReadInt32();
                var tileWidth = reader.ReadInt32();
                var tileHeight = reader.ReadInt32();
                var visible = reader.ReadBoolean();

                // FIX: Use Size object initializer
                var layerSize = new xTile.Dimensions.Size { Width = layerWidth, Height = layerHeight };
                var tileSize = new xTile.Dimensions.Size { Width = tileWidth, Height = tileHeight };
                var layer = new Layer(id, map, layerSize, tileSize);
                layer.Visible = visible;

                var layerPropCount = reader.ReadInt32();
                for (var j = 0; j < layerPropCount; j++)
                {
                    var key = stringTable[reader.ReadInt32()];
                    var valueIndex = reader.ReadInt32();
                    layer.Properties[key] = valueIndex >= 0 ? stringTable[valueIndex] : "";
                }

                for (var y = 0; y < layerHeight; y++)
                {
                    for (var x = 0; x < layerWidth; x++)
                    {
                        var tileType = reader.ReadByte();
                        switch ((char)tileType)
                        {
                            case 'N':
                                layer.Tiles[x, y] = null;
                                break;
                            case 'S':
                                var stTsIndex = reader.ReadInt32();
                                var stTileIndex = reader.ReadInt32();
                                var stBlendMode = reader.ReadByte();
                                var st = new StaticTile(layer, map.TileSheets[stTsIndex], (BlendMode)stBlendMode, stTileIndex);

                                var stPropCount = reader.ReadInt32();
                                for (var k = 0; k < stPropCount; k++)
                                {
                                    var key = stringTable[reader.ReadInt32()];
                                    var valueIndex = reader.ReadInt32();
                                    st.Properties[key] = valueIndex >= 0 ? stringTable[valueIndex] : "";
                                }
                                layer.Tiles[x, y] = st;
                                break;
                            case 'A':
                                var frameCount = reader.ReadUInt16();
                                var frameInterval = reader.ReadInt32();
                                var blendMode = reader.ReadByte();
                                var animTile = new AnimatedTile(layer, new StaticTile[frameCount], frameInterval);
                                animTile.BlendMode = (BlendMode)blendMode;
                                for (var j = 0; j < animTile.TileFrames.Length; j++)
                                {
                                    var frameTsIndex = reader.ReadInt32();
                                    var frameTileIndex = reader.ReadInt32();
                                    animTile.TileFrames[j] = new StaticTile(layer, map.TileSheets[frameTsIndex], BlendMode.Alpha, frameTileIndex);
                                }

                                var animPropCount = reader.ReadInt32();
                                for (var k = 0; k < animPropCount; k++)
                                {
                                    var key = stringTable[reader.ReadInt32()];
                                    var valueIndex = reader.ReadInt32();
                                    animTile.Properties[key] = valueIndex >= 0 ? stringTable[valueIndex] : "";
                                }
                                layer.Tiles[x, y] = animTile;
                                break;
                        }
                    }
                }

                map.AddLayer(layer);
            }

            return map;
        }
    }
}
