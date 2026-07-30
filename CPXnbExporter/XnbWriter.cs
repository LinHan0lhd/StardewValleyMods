using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPXnbExporter;
public static class XnbWriter
{
    const string ReaderFull = "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
    const string ReaderShort = "Microsoft.Xna.Framework.Content.Texture2DReader";

    public static void NormalizeAlpha(Color[] p) { for (int i = 0; i < p.Length; i++) if (p[i].A == 0) p[i] = Color.Transparent; }
    public static Color[] UnpremultiplyAlpha(Color[] p)
    {
        var r = new Color[p.Length];
        for (int i = 0; i < p.Length; i++)
        {
            byte a = p[i].A;
            if (a == 0) r[i] = Color.Transparent;
            else if (a == 255) r[i] = p[i];
            else r[i] = new Color((byte)Math.Min(255, p[i].R * 255 / a), (byte)Math.Min(255, p[i].G * 255 / a), (byte)Math.Min(255, p[i].B * 255 / a), a);
        }
        return r;
    }

    public static XnbMetadata ExportTextureSet(string pb, string ub, Texture2D t, char p = 'a', byte v = 5)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(pb));
        if (ub != null) Directory.CreateDirectory(Path.GetDirectoryName(ub));
        int w = t.Width, h = t.Height; var px = new Color[w * h]; t.GetData(px); NormalizeAlpha(px);
        Color[] xnbPx = px, pngPx = ub != null ? UnpremultiplyAlpha(px) : null;
        long fs;
        using (var s = new FileStream(pb + ".xnb", FileMode.Create, FileAccess.Write)) fs = WriteXnb(s, xnbPx, w, h, p, v);
        var meta = new XnbMetadata { Width = w, Height = h, Format = 0, FormatName = "Color", MipCount = 1, Platform = p.ToString(), Version = v, Compressed = p == 'a', FileSize = fs };
        if (ub != null)
        {
            using var pt = new Texture2D(t.GraphicsDevice, w, h); pt.SetData(pngPx);
            using var ms = new MemoryStream(); pt.SaveAsPng(ms, w, h);
            File.WriteAllBytes(ub + ".png", ms.ToArray());
            File.WriteAllText(ub + ".config", meta.ToConfig(), Encoding.UTF8);
        }
        return meta;
    }

    public static void WriteTextureXnb(string path, Color[] px, int w, int h, char p, byte v = 5)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteXnb(fs, px, w, h, p, v);
    }

    static long WriteXnb(Stream s, Color[] px, int w, int h, char p, byte v)
    {
        bool comp = p == 'a';
        var bw = new XnbBufferWriter(4096);
        XnbFormat.WriteHeader(bw, p, v, comp, out int fsp, out int csp);
        bw.Write7BitEncodedInt(1);
        bw.Write7BitEncodedString(p == 'a' ? ReaderShort : ReaderFull);
        bw.WriteUInt32(0);
        bw.Write7BitEncodedInt(0);
        bw.Write7BitEncodedInt(1);
        bw.WriteInt32(0);
        bw.WriteInt32(w);
        bw.WriteInt32(h);
        bw.WriteInt32(1);
        bw.WriteInt32(w * h * 4);
        for (int i = 0; i < px.Length; i++) { bw.WriteByte(px[i].R); bw.WriteByte(px[i].G); bw.WriteByte(px[i].B); bw.WriteByte(px[i].A); }
        return XnbFormat.FinalizeAndWrite(bw, fsp, csp, comp, s);
    }
}
