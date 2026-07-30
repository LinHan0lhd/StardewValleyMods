using System;
using System.IO;

namespace CPXnbExporter;
public readonly struct ExportOptions
{
    public char Platform { get; init; }
    public bool Unpacked { get; init; }
    public string BaseDir { get; init; }
    public string PackedDir => Path.Combine(BaseDir, "packed");
    public string UnpackedDir => Path.Combine(BaseDir, "unpacked");
    public string TroubleshootDir => Path.Combine(BaseDir, "troubleshoot");
    public static ExportOptions Parse(string[] a, string b)
    {
        char p = 'a'; bool u = false;
        foreach (var x in a)
        {
            switch (x.ToLowerInvariant())
            {
                case "pc": case "w": case "windows": p = 'w'; break;
                case "mobile": case "a": case "android": p = 'a'; break;
                case "ios": case "i": p = 'i'; break;
                case "unpacked": case "u": u = true; break;
            }
        }
        return new ExportOptions { Platform = p, Unpacked = u, BaseDir = b };
    }
}
