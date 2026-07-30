using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace CPXnbExporter;
public static class TBinStreamReader
{
    public static Map ReadMap(byte[] d)
    {
        using var ms = new MemoryStream(d);
        using var r = new BinaryReader(ms, Encoding.UTF8);
        if (Encoding.UTF8.GetString(r.ReadBytes(6)) != "tBIN10") throw new InvalidDataException("Bad header");
        var m = new Map { Id = ReadStr(r), Description = ReadStr(r) };
        ReadProps(r, m.Properties);
        int n = r.ReadInt32();
        for (int i = 0; i < n; i++) ReadTS(r, m);
        n = r.ReadInt32();
        for (int i = 0; i < n; i++) ReadLayer(r, m);
        return m;
    }

    static void ReadTS(BinaryReader r, Map m)
    {
        string id = ReadStr(r), desc = ReadStr(r), img = ReadStr(r);
        int sw = r.ReadInt32(), sh = r.ReadInt32(), tw = r.ReadInt32(), th = r.ReadInt32(), mw = r.ReadInt32(), mh = r.ReadInt32(), spw = r.ReadInt32(), sph = r.ReadInt32();
        var ts = new TileSheet(id, m, img, new xTile.Dimensions.Size { Width = sw, Height = sh }, new xTile.Dimensions.Size { Width = tw, Height = th });
        ts.Margin = new xTile.Dimensions.Size { Width = mw, Height = mh };
        ts.Spacing = new xTile.Dimensions.Size { Width = spw, Height = sph };
        ts.Description = desc; ReadProps(r, ts.Properties); m.AddTileSheet(ts);
    }

    static void ReadLayer(BinaryReader r, Map m)
    {
        string id = ReadStr(r); bool vis = r.ReadByte() != 0; string desc = ReadStr(r);
        int lw = r.ReadInt32(), lh = r.ReadInt32(), tw = r.ReadInt32(), th = r.ReadInt32();
        var layer = new Layer(id, m, new xTile.Dimensions.Size { Width = lw, Height = lh }, new xTile.Dimensions.Size { Width = tw, Height = th });
        layer.Visible = vis; layer.Description = desc; ReadProps(r, layer.Properties);
        string cur = null;
        for (int y = 0; y < lh; y++)
        {
            int x = 0;
            while (x < lw)
            {
                byte tag = r.ReadByte();
                if (tag == (byte)'N') { int c = r.ReadInt32(); for (int k = 0; k < c && x < lw; k++) { layer.Tiles[x, y] = null; x++; } continue; }
                else if (tag == (byte)'T') { cur = ReadStr(r); tag = r.ReadByte(); }
                if (tag == (byte)'S')
                {
                    int ti = r.ReadInt32(), bm = r.ReadByte(); var p = new Dictionary<string, PropertyValue>(); ReadProps(r, p);
                    var ts = FindTS(m, cur); var st = new StaticTile(layer, ts, (BlendMode)bm, ti); foreach (var kv in p) st.Properties[kv.Key] = kv.Value;
                    layer.Tiles[x, y] = st; x++;
                }
                else if (tag == (byte)'A')
                {
                    int fi = r.ReadInt32(), fc = r.ReadInt32(); var frames = new StaticTile[fc]; string fcur = null;
                    for (int f = 0; f < fc; f++)
                    {
                        byte ft = r.ReadByte();
                        if (ft == (byte)'T') { fcur = ReadStr(r); ft = r.ReadByte(); }
                        if (ft == (byte)'S') { int fti = r.ReadInt32(), fbm = r.ReadByte(); var fp = new Dictionary<string, PropertyValue>(); ReadProps(r, fp); var fts = FindTS(m, fcur); frames[f] = new StaticTile(layer, fts, (BlendMode)fbm, fti); foreach (var kv in fp) frames[f].Properties[kv.Key] = kv.Value; }
                        else throw new InvalidDataException($"Bad frame tag {(char)ft}");
                    }
                    var anim = new AnimatedTile(layer, frames, fi); var ap = new Dictionary<string, PropertyValue>(); ReadProps(r, ap); foreach (var kv in ap) anim.Properties[kv.Key] = kv.Value;
                    layer.Tiles[x, y] = anim; x++;
                }
                else throw new InvalidDataException($"Bad tile tag {(char)tag}");
            }
        }
        m.AddLayer(layer);
    }

    static TileSheet FindTS(Map m, string id) { if (string.IsNullOrEmpty(id)) return null; foreach (var ts in m.TileSheets) if (ts.Id == id) return ts; return null; }
    static void ReadProps(BinaryReader r, IDictionary<string, PropertyValue> p) { int n = r.ReadInt32(); for (int i = 0; i < n; i++) p[ReadStr(r)] = ReadPV(r); }
    static PropertyValue ReadPV(BinaryReader r) => r.ReadByte() switch { 0 => r.ReadByte() != 0, 1 => r.ReadInt32(), 2 => r.ReadSingle(), 3 => ReadStr(r), _ => throw new InvalidDataException("Bad PV type") };
    static string ReadStr(BinaryReader r) { int n = r.ReadInt32(); if (n <= 0) return ""; return Encoding.UTF8.GetString(r.ReadBytes(n)); }
}
