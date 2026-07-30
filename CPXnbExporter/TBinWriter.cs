using System.Reflection;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter;
public static class TBinWriter
{
    public static string MapAssetName { get; set; }
    static readonly FieldInfo[] _pf = typeof(PropertyValue).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

    public static byte[] SerializeTbin(Map map)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms, Encoding.UTF8);
        w.Write(Encoding.UTF8.GetBytes("tBIN10"));
        WriteStr(w, map.Id ?? ""); WriteStr(w, map.Description ?? ""); WriteProps(w, map.Properties);
        w.Write(map.TileSheets.Count);
        foreach (var ts in map.TileSheets) WriteTS(w, ts);
        w.Write(map.Layers.Count);
        foreach (var layer in map.Layers) WriteLayer(w, layer, map);
        return ms.ToArray();
    }

    public static void WriteMapXnb(Stream s, Map m, char p) => XnbMapWriter.WriteMapXnbFromTbin(s, SerializeTbin(m), p);
    public static void WriteMapTbin(Stream s, Map m) => s.Write(SerializeTbin(m), 0, SerializeTbin(m).Length);

    static void WriteTS(BinaryWriter w, TileSheet ts)
    {
        WriteStr(w, ts.Id ?? ""); WriteStr(w, ts.Description ?? ""); WriteStr(w, GetImg(ts));
        w.Write(ts.SheetWidth); w.Write(ts.SheetHeight);
        w.Write(ts.TileWidth); w.Write(ts.TileHeight);
        w.Write(ts.Margin.Width); w.Write(ts.Margin.Height);
        w.Write(ts.Spacing.Width); w.Write(ts.Spacing.Height);
        WriteProps(w, ts.Properties);
    }

    static bool IsNull(Tile t) => t == null || (t is StaticTile st && st.TileIndex == -1);

    static void WriteLayer(BinaryWriter w, Layer layer, Map map)
    {
        WriteStr(w, layer.Id ?? ""); w.Write((byte)(layer.Visible ? 1 : 0)); WriteStr(w, layer.Description ?? "");
        w.Write(layer.LayerWidth); w.Write(layer.LayerHeight);
        w.Write(16); w.Write(16);
        WriteProps(w, layer.Properties);
        string cur = null;
        for (int y = 0; y < layer.LayerHeight; y++)
        {
            int x = 0;
            while (x < layer.LayerWidth)
            {
                var t = layer.Tiles[x, y];
                if (IsNull(t))
                {
                    int n = 0; while (x < layer.LayerWidth && IsNull(layer.Tiles[x, y])) { n++; x++; }
                    w.Write((byte)'N'); w.Write(n);
                }
                else if (t is StaticTile st)
                {
                    string id = st.TileSheet?.Id ?? "";
                    if (id != cur) { w.Write((byte)'T'); WriteStr(w, id); cur = id; }
                    w.Write((byte)'S'); w.Write(st.TileIndex); w.Write((byte)st.BlendMode); WriteProps(w, st.Properties); x++;
                }
                else if (t is AnimatedTile a)
                {
                    if (a.TileFrames.Length == 0) { w.Write((byte)'N'); w.Write(1); }
                    else { w.Write((byte)'A'); WriteAnim(w, a); }
                    x++;
                }
                else { w.Write((byte)'N'); w.Write(1); x++; }
            }
        }
    }

    static void WriteAnim(BinaryWriter w, AnimatedTile a)
    {
        w.Write((int)a.FrameInterval); w.Write(a.TileFrames.Length);
        string cur = null;
        foreach (var f in a.TileFrames)
        {
            string id = f.TileSheet?.Id ?? "";
            if (id != cur) { w.Write((byte)'T'); WriteStr(w, id); cur = id; }
            w.Write((byte)'S'); w.Write(f.TileIndex); w.Write((byte)f.BlendMode); WriteProps(w, f.Properties);
        }
        WriteProps(w, a.Properties);
    }

    static void WriteProps(BinaryWriter w, IDictionary<string, PropertyValue> p)
    {
        w.Write(p.Count);
        foreach (var kv in p) { WriteStr(w, kv.Key ?? ""); WritePV(w, kv.Value); }
    }

    static void WritePV(BinaryWriter w, PropertyValue v)
    {
        if (v == null) { w.Write((byte)3); WriteStr(w, ""); return; }
        object inner = null;
        foreach (var f in _pf)
        {
            string n = f.Name?.ToLowerInvariant() ?? "";
            if (n.Contains("tag") || n.Contains("type") || n.Contains("kind") || n.Contains("discriminator") || n.Contains("case")) continue;
            try { var x = f.GetValue(v); if (x is bool || x is int || x is float || x is string) { inner = x; break; } } catch { }
        }
        switch (inner)
        {
            case bool b: w.Write((byte)0); w.Write(b ? (byte)1 : (byte)0); break;
            case int i: w.Write((byte)1); w.Write(i); break;
            case float f: w.Write((byte)2); w.Write(f); break;
            case string s: w.Write((byte)3); WriteStr(w, s); break;
            default: w.Write((byte)3); WriteStr(w, ""); break;
        }
    }

    static void WriteStr(BinaryWriter w, string s)
    {
        if (string.IsNullOrEmpty(s)) { w.Write(0); return; }
        var b = Encoding.UTF8.GetBytes(s); w.Write(b.Length); w.Write(b);
    }

    static string GetImg(TileSheet ts)
    {
        string src = ts.ImageSource?.Replace('\\', '/');
        if (string.IsNullOrEmpty(src)) return "";
        int dot = src.LastIndexOf('.');
        if (dot >= 0) src = src[..dot];
        if (!string.IsNullOrEmpty(MapAssetName) && !src.StartsWith("SMAPI/", StringComparison.OrdinalIgnoreCase) && !src.Contains("/Mods/", StringComparison.OrdinalIgnoreCase))
        {
            string mapDir = MapAssetName.Replace('\\', '/');
            int ls = mapDir.LastIndexOf('/');
            if (ls >= 0) mapDir = mapDir[..ls];
            if (src.StartsWith(mapDir + "/", StringComparison.OrdinalIgnoreCase)) src = src[(mapDir.Length + 1)..];
        }
        return src;
    }
}
