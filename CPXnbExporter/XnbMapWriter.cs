using System;
using System.IO;
using xTile;

namespace CPXnbExporter
{
    public static class XnbMapWriter
    {
        private const string MapReaderFull = "xTile.Pipeline.TideReader, xTile";

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
            // 地图 reader 在所有平台均使用全名，与 XnbConverter 参考实现一致
            string readerName = MapReaderFull;
            // Android 保持 LZ4 压缩；iOS/PC 不压缩（与贴图逻辑一致）
            bool compressed = platform == 'a';

            var writer = new XnbBufferWriter(4096);
            XnbFormat.WriteHeader(writer, platform, 5, compressed, out int fileSizePos, out int contentSizePos);

            // Type reader count
            writer.Write7BitEncodedInt(1);

            // Reader name (7-bit encoded string)
            writer.Write7BitEncodedString(readerName);

            // Reader version (uint32)
            writer.WriteUInt32(0);

            // Shared resources
            writer.Write7BitEncodedInt(0);

            // 主对象 Type ID
            writer.Write7BitEncodedInt(1);

            // TBIN blob
            writer.WriteInt32(tbinData.Length);
            writer.WriteBytes(tbinData);

            XnbFormat.FinalizeAndWrite(writer, fileSizePos, contentSizePos, compressed, output);
        }
    }
}
