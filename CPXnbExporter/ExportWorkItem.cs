using Microsoft.Xna.Framework;

namespace CPXnbExporter;
public class ExportWorkItem
{
    public WorkItemType Type { get; init; }
    public string FileName { get; init; }
    public string PackedBasePath { get; init; }
    public string UnpackedBasePath { get; init; }
    public char Platform { get; init; }
    public Color[] PixelData { get; init; }
    public byte[] PngData { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public byte[] TbinData { get; init; }
    public object DataObject { get; init; }
    public string DataTypeName { get; init; }
}
public enum WorkItemType { Texture, Map, Data }
