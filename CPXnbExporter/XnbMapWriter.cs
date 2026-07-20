using System;
using System.IO;
using System.Text;
using K4os.Compression.LZ4;
using xTile;

namespace CPXnbExporter
{
    public static class XnbMapWriter
    {
        private const string MapReaderName = "xTile.Pipeline.TideReader, xTile";

        /// <summary>主线程调用版本：直接接受 Map 对象</summary>
        public static void WriteMapXnb(Stream output, Map map, char platform)
        {
            byte[] tbinData = TBinWriter.SerializeTbin(map);
            WriteMapXnbFromTbin(output, tbinData, platform);
        }

        /// <summary>
        /// 从 TBIN 字节数组写入 XNB（纯 CPU，可在后台线程执行）。
        /// </summary>
        public static void WriteMapXnbFromTbin(Stream output, byte[] tbinData, char platform)
        {
            bool isMobile = (platform == 'a' || platform == 'i');
            string readerName = MapReaderName;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8);

            // Header
            writer.Write((byte)'X');
            writer.Write((byte)'N');
            writer.Write((byte)'B');
            writer.Write((byte)platform);
            writer.Write((byte)5);

            byte flags = isMobile ? (byte)0x41 : (byte)0x01;
            writer.Write(flags);

            int fileSizePos = (int)ms.Position;
            writer.Write((uint)0);

            int contentSizePos = -1;
            if (isMobile)
            {
                contentSizePos = (int)ms.Position;
                writer.Write((uint)0);
            }

            // Type reader count
            Write7BitEncodedInt(writer, 1);

            // Reader name (7-bit encoded string)
            Write7BitEncodedString(writer, readerName);

            // Reader version (uint32)
            writer.Write((uint)0);

            // Shared resources
            Write7BitEncodedInt(writer, 0);

            // 主对象 Type ID
            Write7BitEncodedInt(writer, 1);

            int objectStart = (int)ms.Position;

            // TBIN blob
            writer.Write(tbinData.Length);
            writer.Write(tbinData);
            writer.Flush();

            byte[] rawData = ms.ToArray();
            int headerSize = isMobile ? 15 : 10;
            int bodySize = rawData.Length - headerSize;

            if (isMobile)
            {
                byte[] bodyBytes = new byte[bodySize];
                Array.Copy(rawData, headerSize, bodyBytes, 0, bodySize);

                int maxCompressedSize = LZ4Codec.MaximumOutputSize(bodySize);
                byte[] compressedBody = new byte[maxCompressedSize];
                int compressedSize = LZ4Codec.Encode(bodyBytes, 0, bodySize, compressedBody, 0, maxCompressedSize);

                byte[] finalData = new byte[headerSize + compressedSize];
                Array.Copy(rawData, 0, finalData, 0, headerSize);
                Array.Copy(compressedBody, 0, finalData, headerSize, compressedSize);

                byte[] fileSizeBytes = BitConverter.GetBytes((uint)finalData.Length);
                Array.Copy(fileSizeBytes, 0, finalData, fileSizePos, 4);

                byte[] contentSizeBytes = BitConverter.GetBytes((uint)compressedSize);
                Array.Copy(contentSizeBytes, 0, finalData, contentSizePos, 4);

                output.Write(finalData, 0, finalData.Length);
                output.Flush();
            }
            else
            {
                byte[] fileSizeBytes = BitConverter.GetBytes((uint)rawData.Length);
                Array.Copy(fileSizeBytes, 0, rawData, fileSizePos, 4);

                output.Write(rawData, 0, rawData.Length);
                output.Flush();
            }
        }

        private static void Write7BitEncodedInt(BinaryWriter writer, int value)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                writer.Write((byte)(v | 0x80));
                v >>= 7;
            }
            writer.Write((byte)v);
        }

        private static void Write7BitEncodedString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Write7BitEncodedInt(writer, bytes.Length);
            writer.Write(bytes);
        }
    }
}
