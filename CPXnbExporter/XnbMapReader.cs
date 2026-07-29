using System;
using System.IO;
using System.Text;
using xTile;

namespace CPXnbExporter
{
    public static class XnbMapReader
    {
        /// <summary>
        /// 从 XNB 字节数组读取地图。支持 LZ4 压缩与未压缩格式。
        /// 参考 XnbConverter 的 XNB.Decode 流程实现。
        /// </summary>
        public static Map ReadMap(byte[] xnbData)
        {
            if (!XnbFormat.TryReadHeader(xnbData, out char platform, out byte version, out bool compressed, out int headerSize, out int fileSize, out int? originalContentSize))
                throw new InvalidDataException("Invalid XNB header");

            if (fileSize != xnbData.Length)
                throw new InvalidDataException($"XNB file size mismatch: header says {fileSize}, actual {xnbData.Length}");

            byte[] body;
            if (compressed)
            {
                int compressedSize = xnbData.Length - headerSize;
                body = XnbFormat.DecompressLz4(xnbData, headerSize, compressedSize, originalContentSize.Value);
            }
            else
            {
                body = new byte[xnbData.Length - headerSize];
                Buffer.BlockCopy(xnbData, headerSize, body, 0, body.Length);
            }

            using var ms = new MemoryStream(body);
            using var reader = new BinaryReader(ms, Encoding.UTF8);

            // Type reader count
            int readerCount = Read7BitEncodedInt(reader);
            for (int i = 0; i < readerCount; i++)
            {
                string readerName = Read7BitEncodedString(reader);
                uint readerVersion = reader.ReadUInt32();

                if (i == 0 && !readerName.Contains("TideReader"))
                    throw new InvalidDataException($"Unexpected XNB reader: {readerName}");
            }

            // Shared resources
            int sharedCount = Read7BitEncodedInt(reader);
            if (sharedCount != 0)
                throw new NotSupportedException($"Shared resources not supported: {sharedCount}");

            // 主对象 Type ID（通常为 1）
            int typeId = Read7BitEncodedInt(reader);

            // TBIN blob: int32 length + bytes
            int tbinLength = reader.ReadInt32();
            byte[] tbinData = reader.ReadBytes(tbinLength);

            return TBinStreamReader.ReadMap(tbinData);
        }

        private static int Read7BitEncodedInt(BinaryReader reader)
        {
            int num = 0;
            int shift = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                num |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return num;
        }

        private static string Read7BitEncodedString(BinaryReader reader)
        {
            int length = Read7BitEncodedInt(reader);
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
