using Microsoft.Xna.Framework;

namespace CPXnbExporter
{
    /// <summary>后台写入线程的工作单元</summary>
    public class ExportWorkItem
    {
        public WorkItemType Type { get; init; }
        public string FileName { get; init; }
        public string PackedBasePath { get; init; }
        public string UnpackedBasePath { get; init; }
        public char Platform { get; init; }

        // Texture data (主线程提取后传递，无需 GPU)
        public Color[] PixelData { get; init; }
        public byte[] PngData { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        // Map data
        public byte[] TbinData { get; init; }

        // Data asset (JSON)
        public object DataObject { get; init; }
        public string DataTypeName { get; init; }
    }

    public enum WorkItemType
    {
        Texture,
        Map,
        Data
    }
}
