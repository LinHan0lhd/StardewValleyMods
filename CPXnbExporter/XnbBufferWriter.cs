using System;
using System.Text;

namespace CPXnbExporter;
internal class XnbBufferWriter
{
    byte[] _b; int _p;
    public XnbBufferWriter(int c = 4096) { _b = new byte[c]; }
    public int Position => _p; public byte[] Buffer => _b;
    void Grow(int n) { if (_b.Length < n) { int s = _b.Length * 2; while (s < n) s *= 2; Array.Resize(ref _b, s); } }
    public void WriteByte(byte v) { Grow(_p + 1); _b[_p++] = v; }
    public void WriteInt32(int v) { Grow(_p + 4); _b[_p++] = (byte)v; _b[_p++] = (byte)(v >> 8); _b[_p++] = (byte)(v >> 16); _b[_p++] = (byte)(v >> 24); }
    public void WriteUInt32(uint v) { Grow(_p + 4); _b[_p++] = (byte)v; _b[_p++] = (byte)(v >> 8); _b[_p++] = (byte)(v >> 16); _b[_p++] = (byte)(v >> 24); }
    public void WriteUInt32At(int o, uint v) { _b[o] = (byte)v; _b[o + 1] = (byte)(v >> 8); _b[o + 2] = (byte)(v >> 16); _b[o + 3] = (byte)(v >> 24); }
    public void Write7BitEncodedInt(int v) { uint x = (uint)v; while (x >= 0x80) { WriteByte((byte)(x | 0x80)); x >>= 7; } WriteByte((byte)x); }
    public void Write7BitEncodedString(string v) { byte[] b = Encoding.UTF8.GetBytes(v ?? ""); Write7BitEncodedInt(b.Length); WriteBytes(b); }
    public void WriteAsciiString(string v) { if (string.IsNullOrEmpty(v)) return; WriteBytes(Encoding.ASCII.GetBytes(v)); }
    public void WriteBytes(byte[] b) { if (b == null || b.Length == 0) return; Grow(_p + b.Length); Buffer.BlockCopy(b, 0, _b, _p, b.Length); _p += b.Length; }
    public void WriteBytes(byte[] b, int o, int c) { if (c <= 0) return; Grow(_p + c); Buffer.BlockCopy(b, o, _b, _p, c); _p += c; }
}
