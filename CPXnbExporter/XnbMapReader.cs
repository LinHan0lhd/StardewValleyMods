using System;
using System.IO;
using System.Text;
using xTile;

namespace CPXnbExporter;
public static class XnbMapReader
{
    public static Map ReadMap(byte[] d)
    {
        if (!XnbFormat.TryReadHeader(d, out _, out _, out bool c, out int hs, out int fs, out int? ocs)) throw new InvalidDataException("Bad XNB header");
        if (fs != d.Length) throw new InvalidDataException("Size mismatch");
        byte[] body = c ? XnbFormat.DecompressLz4(d, hs, d.Length - hs, ocs.Value) : new byte[d.Length - hs];
        if (!c) Buffer.BlockCopy(d, hs, body, 0, body.Length);
        using var ms = new MemoryStream(body);
        using var r = new BinaryReader(ms, Encoding.UTF8);
        int rc = Read7(r);
        for (int i = 0; i < rc; i++) { string rn = Read7s(r); r.ReadUInt32(); if (i == 0 && !rn.Contains("TideReader")) throw new InvalidDataException($"Bad reader {rn}"); }
        if (Read7(r) != 0) throw new NotSupportedException("Shared resources");
        Read7(r); int tl = r.ReadInt32(); return TBinStreamReader.ReadMap(r.ReadBytes(tl));
    }
    static int Read7(BinaryReader r) { int n = 0, s = 0; byte b; do { b = r.ReadByte(); n |= (b & 0x7F) << s; s += 7; } while ((b & 0x80) != 0); return n; }
    static string Read7s(BinaryReader r) { int n = Read7(r); return Encoding.UTF8.GetString(r.ReadBytes(n)); }
}
