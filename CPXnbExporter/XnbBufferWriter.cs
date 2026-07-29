using System;
using System.Text;

namespace CPXnbExporter
{
    /// <summary>
    /// 轻量字节数组写入器，借鉴 XnbConverter 的 BufferWriter 设计。
    /// 直接操作字节数组，避免 BinaryWriter + MemoryStream 的额外分配与拷贝。
    /// </summary>
    internal class XnbBufferWriter
    {
        private byte[] _buffer;
        private int _position;

        public XnbBufferWriter(int initialCapacity = 4096)
        {
            _buffer = new byte[initialCapacity];
            _position = 0;
        }

        public int Position => _position;
        public byte[] Buffer => _buffer;

        private void EnsureCapacity(int needed)
        {
            if (_buffer.Length < needed)
            {
                int newSize = _buffer.Length * 2;
                while (newSize < needed) newSize *= 2;
                Array.Resize(ref _buffer, newSize);
            }
        }

        public void WriteByte(byte value)
        {
            EnsureCapacity(_position + 1);
            _buffer[_position++] = value;
        }

        public void WriteInt32(int value)
        {
            EnsureCapacity(_position + 4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        public void WriteUInt32(uint value)
        {
            EnsureCapacity(_position + 4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        /// <summary>在指定偏移处覆写 UInt32（用于回填 fileSize / contentSize）。</summary>
        public void WriteUInt32At(int offset, uint value)
        {
            _buffer[offset] = (byte)value;
            _buffer[offset + 1] = (byte)(value >> 8);
            _buffer[offset + 2] = (byte)(value >> 16);
            _buffer[offset + 3] = (byte)(value >> 24);
        }

        public void Write7BitEncodedInt(int value)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                WriteByte((byte)(v | 0x80));
                v >>= 7;
            }
            WriteByte((byte)v);
        }

        public void Write7BitEncodedString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");
            Write7BitEncodedInt(bytes.Length);
            WriteBytes(bytes);
        }

        public void WriteAsciiString(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;
            EnsureCapacity(_position + bytes.Length);
            Buffer.BlockCopy(bytes, 0, _buffer, _position, bytes.Length);
            _position += bytes.Length;
        }

        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            if (count <= 0) return;
            EnsureCapacity(_position + count);
            Buffer.BlockCopy(bytes, offset, _buffer, _position, count);
            _position += count;
        }
    }
}
