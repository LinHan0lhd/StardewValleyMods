using System;
using System.IO;
using K4os.Compression.LZ4;

namespace CPXnbExporter
{
    /// <summary>
    /// XNB 格式公共工具，借鉴 XnbConverter 的 XNB.cs 中的头部与压缩逻辑。
    /// 统一处理 XNB 头部写入、LZ4 压缩/解压、以及头部解析。
    /// </summary>
    internal static class XnbFormat
    {
        public const int UncompressedHeaderSize = 10;
        public const int CompressedHeaderSize = 14;

        /// <summary>
        /// 写入 XNB 头部（XNB + platform + version + flags + fileSize占位 + [contentSize占位]）。
        /// </summary>
        public static void WriteHeader(
            XnbBufferWriter writer,
            char platform,
            byte version,
            bool compressed,
            out int fileSizePosition,
            out int contentSizePosition)
        {
            writer.WriteAsciiString("XNB");
            writer.WriteByte((byte)platform);
            writer.WriteByte(version);
            writer.WriteByte(compressed ? (byte)0x41 : (byte)0x01);

            fileSizePosition = writer.Position;
            writer.WriteUInt32(0);

            contentSizePosition = compressed ? writer.Position : -1;
            if (compressed)
                writer.WriteUInt32(0);
        }

        /// <summary>
        /// 完成 XNB 写入：回填 size 字段、执行 LZ4 压缩（如需），并写入输出流。
        /// </summary>
        public static long FinalizeAndWrite(
            XnbBufferWriter writer,
            int fileSizePosition,
            int contentSizePosition,
            bool compressed,
            Stream output)
        {
            byte[] data = writer.Buffer;
            int dataLength = writer.Position;
            int headerSize = compressed ? CompressedHeaderSize : UncompressedHeaderSize;
            int bodySize = dataLength - headerSize;

            if (compressed)
            {
                byte[] bodyBytes = new byte[bodySize];
                Buffer.BlockCopy(data, headerSize, bodyBytes, 0, bodySize);

                int maxCompressedSize = LZ4Codec.MaximumOutputSize(bodySize);
                byte[] compressedBody = new byte[maxCompressedSize];
                int compressedSize = LZ4Codec.Encode(bodyBytes, 0, bodySize, compressedBody, 0, maxCompressedSize);

                byte[] finalData = new byte[headerSize + compressedSize];
                Buffer.BlockCopy(data, 0, finalData, 0, headerSize);
                Buffer.BlockCopy(compressedBody, 0, finalData, headerSize, compressedSize);

                BitConverter.GetBytes((uint)finalData.Length).CopyTo(finalData, fileSizePosition);
                BitConverter.GetBytes((uint)bodySize).CopyTo(finalData, contentSizePosition);

                output.Write(finalData, 0, finalData.Length);
                output.Flush();
                return finalData.Length;
            }
            else
            {
                writer.WriteUInt32At(fileSizePosition, (uint)dataLength);
                output.Write(data, 0, dataLength);
                output.Flush();
                return dataLength;
            }
        }

        /// <summary>
        /// 解析 XNB 头部。返回是否成功，并输出平台、版本、压缩标志、头部长度、文件大小、原始内容大小。
        /// </summary>
        public static bool TryReadHeader(
            byte[] data,
            out char platform,
            out byte version,
            out bool compressed,
            out int headerSize,
            out int fileSize,
            out int? originalContentSize)
        {
            platform = default;
            version = default;
            compressed = false;
            headerSize = 0;
            fileSize = 0;
            originalContentSize = null;

            if (data.Length < UncompressedHeaderSize)
                return false;

            if (data[0] != (byte)'X' || data[1] != (byte)'N' || data[2] != (byte)'B')
                return false;

            platform = (char)data[3];
            version = data[4];
            byte flags = data[5];
            compressed = (flags & 0x40) != 0;

            fileSize = BitConverter.ToInt32(data, 6);
            headerSize = compressed ? CompressedHeaderSize : UncompressedHeaderSize;

            if (compressed)
            {
                if (data.Length < CompressedHeaderSize)
                    return false;
                originalContentSize = BitConverter.ToInt32(data, 10);
            }

            return true;
        }

        /// <summary>
        /// 对指定范围执行 LZ4 解压。
        /// </summary>
        public static byte[] DecompressLz4(byte[] data, int offset, int compressedSize, int decompressedSize)
        {
            byte[] result = new byte[decompressedSize];
            int decoded = LZ4Codec.Decode(data, offset, compressedSize, result, 0, decompressedSize);
            if (decoded != decompressedSize)
                throw new InvalidOperationException($"LZ4 decode size mismatch: expected {decompressedSize}, got {decoded}");
            return result;
        }
    }
}
