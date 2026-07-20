using System.Collections.Generic;

namespace CPXnbExporter.TBin
{
    public struct IntVector2
    {
        public int X;
        public int Y;
    }

    public class TBin10
    {
        public byte[] Data { get; set; }
        public string Format { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public List<Propertie> Properties { get; set; }
        public List<TileSheet> TileSheets { get; set; }
        public List<Layer> Layers { get; set; }
    }

    public class TileSheet
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public IntVector2? SheetSize { get; set; }
        public IntVector2 TileSize { get; set; }
        public IntVector2 Margin { get; set; }
        public IntVector2 Spacing { get; set; }
        public List<Propertie> Properties { get; set; }
    }

    public class Layer
    {
        public string Id;
        public byte Visible;
        public string Description;
        public IntVector2 LayerSize;
        public IntVector2 TileSize;
        public List<Propertie> Properties;
        public List<BaseTile> Tiles;
        public List<char> Index;
        public List<int> _sizeArr;
        public List<string> _currTileSheet;
    }

    public class BaseTile
    {
        public List<Propertie> Properties { get; set; }
    }

    public class StaticTile : BaseTile
    {
        public string TileSheet { get; set; }
        public int TileIndex { get; set; }
        public byte BlendMode { get; set; }
    }

    public class AnimatedTile : BaseTile
    {
        public int FrameInterval { get; set; }
        public List<StaticTile> Frames { get; set; }
        public int _frameCount { get; set; }
        public List<char> Index { get; set; }
        public List<string> _currTileSheet { get; set; }
    }

    public class Propertie
    {
        public string Key { get; set; }
        public byte Type { get; set; }
        public object Value { get; set; }
    }
}
