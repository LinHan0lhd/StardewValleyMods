using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CPXnbExporter;

public class XnbMetadata
{
    public int Width, Height, Format, MipCount;
    public string FormatName, Platform;
    public byte Version;
    public bool Compressed;
    public long FileSize;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new StringEnumConverter() }
    };

    /// <summary>Texture2D .config (XnbConverter pack compatible)</summary>
    public string ToConfig()
    {
        bool isMobile = Platform?.ToLowerInvariant() == "a" || Platform?.ToLowerInvariant() == "i";
        string target = Platform?.ToLowerInvariant() switch
        {
            "a" => "Android",
            "i" => "Ios",
            _ => "Windows"
        };

        var obj = new
        {
            Header = new
            {
                Target = target,
                FormatVersion = (int)Version,
                CompressedFlag = isMobile ? (object)"Lz4" : (object)0
            },
            Readers = new[]
            {
                new
                {
                    Type = isMobile
                        ? "Microsoft.Xna.Framework.Content.Texture2DReader"
                        : "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553",
                    Version = 0
                }
            },
            Content = new
            {
                Extension = ".png",
                Format = Format
            }
        };

        return JsonConvert.SerializeObject(obj, JsonSettings);
    }

    /// <summary>Map .config (XnbConverter pack compatible)</summary>
    public static string MapConfig(char platform, long fileSize)
    {
        bool isMobile = platform == 'a' || platform == 'i';
        string target = platform switch
        {
            'a' => "Android",
            'i' => "Ios",
            _ => "Windows"
        };

        var obj = new
        {
            Header = new
            {
                Target = target,
                FormatVersion = 5,
                CompressedFlag = (object)0
            },
            Readers = new[]
            {
                new
                {
                    // XnbConverter TypeReadHelper has a hardcoded entry for this exact key
                    Type = "xTile.Pipeline.TideReader, xTile",
                    Version = 0
                }
            },
            Content = new
            {
                Extension = ".tbin",
                Format = 0
            }
        };

        return JsonConvert.SerializeObject(obj, JsonSettings);
    }

    /// <summary>Data .config (reference only, XnbConverter Data pack not fully supported)</summary>
    public static string DataConfig(string dataTypeName, string fileName, long fileSize)
    {
        var obj = new
        {
            type = "Data",
            dataTypeName = dataTypeName,
            fileName = fileName,
            fileSize = fileSize
        };

        return JsonConvert.SerializeObject(obj, JsonSettings);
    }
}
