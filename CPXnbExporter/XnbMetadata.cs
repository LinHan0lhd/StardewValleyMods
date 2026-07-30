using System.Text;

namespace CPXnbExporter;
public class XnbMetadata
{
    public int Width, Height, Format, MipCount; public string FormatName, Platform; public byte Version; public bool Compressed; public long FileSize;
    public string ToConfig()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# XNB Metadata"); sb.AppendLine($"width: {Width}"); sb.AppendLine($"height: {Height}"); sb.AppendLine($"format: {FormatName} ({Format})");
        sb.AppendLine($"mipCount: {MipCount}"); sb.AppendLine($"platform: {Platform}"); sb.AppendLine($"version: {Version}"); sb.AppendLine($"compressed: {Compressed}"); sb.AppendLine($"fileSize: {FileSize}");
        return sb.ToString();
    }
}
