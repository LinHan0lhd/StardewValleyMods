using System.Text;

namespace CPXnbExporter
{
    public class XnbMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Format { get; set; }
        public string FormatName { get; set; }
        public int MipCount { get; set; }
        public string Platform { get; set; }
        public byte Version { get; set; }
        public bool Compressed { get; set; }
        public int SharedResources { get; set; }
        public long FileSize { get; set; }

        public string ToConfig()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# XNB Metadata");
            sb.AppendLine($"width: {Width}");
            sb.AppendLine($"height: {Height}");
            sb.AppendLine($"format: {FormatName} ({Format})");
            sb.AppendLine($"mipCount: {MipCount}");
            sb.AppendLine($"platform: {Platform}");
            sb.AppendLine($"version: {Version}");
            sb.AppendLine($"compressed: {Compressed}");
            sb.AppendLine($"fileSize: {FileSize}");
            return sb.ToString();
        }
    }
}
