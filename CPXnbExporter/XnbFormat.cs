using System;
using System.IO;
using K4os.Compression.LZ4;

namespace CPXnbExporter;
internal static class XnbFormat
{
    public const int HeaderSize = 10, CompHeaderSize = 14;

    public static void WriteHeader(XnbBufferWriter w, char p, byte v, bool c, out int fsp, out int csp)
    {
        w.WriteAsciiString("XNB"); w.WriteByte((byte)p); w.WriteByte(v); w.WriteByte(c ? (byte)0x41 : (byte)0x01);
        fsp = w.Position; w.WriteUInt32(0); csp = c ? w.Position : -1; if (c) w.WriteUInt32(0);
    }

    public static long FinalizeAndWrite(XnbBufferWriter w, int fsp, int csp, bool c, Stream s)
    {
        byte[] d = w.Buffer; int len = w.Position, hs = c ? CompHeaderSize : HeaderSize, bs = len - hs;
        if (c)
        {
            byte[] body = new byte[bs]; Buffer.BlockCopy(d, hs, body, 0, bs);
            int max = LZ4Codec.MaximumOutputSize(bs); byte[] comp = new byte[max]; int n = LZ4Codec.Encode(body, 0, bs, comp, 0, max);
            byte[] final = new byte[hs + n]; Buffer.BlockCopy(d, 0, final, 0, hs); Buffer.BlockCopy(comp, 0, final, hs, n);
            BitConverter.GetBytes((uint)final.Length).CopyTo(final, fsp); BitConverter.GetBytes((uint)bs).CopyTo(final, csp);
            s.Write(final, 0, final.Length); s.Flush(); return final.Length;
        }
        else { w.WriteUInt32At(fsp, (uint)len); s.Write(d, 0, len); s.Flush(); return len; }
    }

    public static bool TryReadHeader(byte[] d, out char p, out byte v, out bool c, out int hs, out int fs, out int? ocs)
    {
        p = default; v = default; c = false; hs = 0; fs = 0; ocs = null;
        if (d.Length < HeaderSize || d[0] != (byte)'X' || d[1] != (byte)'N' || d[2] != (byte)'B') return false;
        p = (char)d[3]; v = d[4]; c = (d[5] & 0x40) != 0; fs = BitConverter.ToInt32(d, 6); hs = c ? CompHeaderSize : HeaderSize;
        if (c) { if (d.Length < CompHeaderSize) return false; ocs = BitConverter.ToInt32(d, 10); }
        return true;
    }

    public static byte[] DecompressLz4(byte[] d, int off, int cs, int ds)
    {
        byte[] r = new byte[ds]; int n = LZ4Codec.Decode(d, off, cs, r, 0, ds); if (n != ds) throw new InvalidOperationException("LZ4 size mismatch"); return r;
    }
}
