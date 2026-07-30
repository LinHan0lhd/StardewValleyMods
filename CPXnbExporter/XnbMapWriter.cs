using System;
using System.IO;
using xTile;

namespace CPXnbExporter;
public static class XnbMapWriter
{
    const string Reader = "xTile.Pipeline.TideReader, xTile";
    public static void WriteMapXnb(Stream s, Map m, char p) => WriteMapXnbFromTbin(s, TBinWriter.SerializeTbin(m), p);
    public static void WriteMapXnbFromTbin(Stream s, byte[] d, char p)
    {
        bool c = p == 'a';
        var w = new XnbBufferWriter(4096);
        XnbFormat.WriteHeader(w, p, 5, c, out int fsp, out int csp);
        w.Write7BitEncodedInt(1); w.Write7BitEncodedString(Reader); w.WriteUInt32(0); w.Write7BitEncodedInt(0); w.Write7BitEncodedInt(1);
        w.WriteInt32(d.Length); w.WriteBytes(d);
        XnbFormat.FinalizeAndWrite(w, fsp, csp, c, s);
    }
}
