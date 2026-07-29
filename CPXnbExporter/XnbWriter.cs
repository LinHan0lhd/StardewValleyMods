using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPXnbExporter
{
    /// <summary>XNB 贴图写入器。支持 PC (w) 和移动端 (a/i) 格式。</summary>
    public static class XnbWriter
    {
        private const string Texture2DReaderFull =
            "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, " +
            "Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

        private const string Texture2DReaderShort =
            "Microsoft.Xna.Framework.Content.Texture2DReader";

        /// <summary>
        /// 导出贴图资产为 XNB（packed）和可选的 PNG + .config（unpacked）。
        /// 主线程调用版本：直接接受 Texture2D（用于单资产导出命令）。
        /// 参考游戏反编译行为：不做任何 Alpha 预乘/反预乘、边缘 padding、alpha bleeding。
        /// </summary>
        public static XnbMetadata ExportTextureSet(
            string packedBasePath,
            string unpackedBasePath,
            Texture2D texture,
            char platform = 'a',
            byte xnbVersion = 5,
            int tileSheetEdgePadding = 0)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(packedBasePath));
            if (!string.IsNullOrEmpty(unpackedBasePath))
                Directory.CreateDirectory(Path.GetDirectoryName(unpackedBasePath));

            int width = texture.Width;
            int height = texture.Height;
            var pixels = new Color[width * height];
            texture.GetData(pixels);

            // XNB：直接使用原始像素
            Color[] xnbPixels = pixels;
            // PNG：直接使用原始像素（不做反预乘处理）
            Color[] pngPixels = (unpackedBasePath != null) ? pixels : null;

            // XNB
            string xnbPath = packedBasePath + ".xnb";
            long xnbFileSize;
            using (var fs = new FileStream(xnbPath, FileMode.Create, FileAccess.Write))
            {
                xnbFileSize = WriteXnbBinaryFromPixels(fs, xnbPixels, width, height, platform, xnbVersion);
            }

            var metadata = new XnbMetadata
            {
                Width = width,
                Height = height,
                Format = 0,
                FormatName = "Color",
                MipCount = 1,
                Platform = platform.ToString(),
                Version = xnbVersion,
                Compressed = (platform == 'a'),
                SharedResources = 0,
                FileSize = xnbFileSize
            };

            // Unpacked
            if (!string.IsNullOrEmpty(unpackedBasePath))
            {
                string pngPath = unpackedBasePath + ".png";
                using var pngTex = new Texture2D(texture.GraphicsDevice, width, height);
                pngTex.SetData(pngPixels);
                using (var fs = new FileStream(pngPath, FileMode.Create, FileAccess.Write))
                    pngTex.SaveAsPng(fs, width, height);

                string configPath = unpackedBasePath + ".config";
                File.WriteAllText(configPath, metadata.ToConfig(), Encoding.UTF8);
            }

            return metadata;
        }

        /// <summary>
        /// 从像素数组写入 XNB 二进制（纯 CPU，可在后台线程执行）。
        /// v2.1 多线程版本核心方法。
        /// </summary>
        public static long WriteXnbBinaryFromPixels(
            Stream output,
            Color[] pixels,
            int width,
            int height,
            char platform,
            byte xnbVersion = 5)
        {
            bool isMobile = (platform == 'a' || platform == 'i');
            string readerName = isMobile ? Texture2DReaderShort : Texture2DReaderFull;

            // 注：预乘 Alpha 处理已在 EnqueueTexture 入队前完成（ModEntry.PremultiplyAlpha），
            // 此处直接写入即可。调用方（如单资产导出命令）需自行确保像素格式正确。

            // 注：曾尝试 iOS POT padding（高16位=used，低16位=actual），
            // 但导致 iOS 1.6 纹理完全解析错乱（彩虹漫色、背景图块）。
            // xnb.js 的该逻辑是针对 Stardew Valley 1.5 iOS，1.6 似乎已不需要。
            // 现直接按原始尺寸写入。
            int actualWidth = width;
            int actualHeight = height;

            bool compressed = platform == 'a';
            var writer = new XnbBufferWriter(4096);
            XnbFormat.WriteHeader(writer, platform, xnbVersion, compressed, out int fileSizePos, out int contentSizePos);

            // Type reader
            writer.Write7BitEncodedInt(1);
            writer.Write7BitEncodedString(readerName);
            writer.WriteUInt32(0);

            // Shared resources
            writer.Write7BitEncodedInt(0);

            // Object Type ID
            writer.Write7BitEncodedInt(1);

            // Surface format
            writer.WriteInt32(0);
            writer.WriteInt32(actualWidth);
            writer.WriteInt32(actualHeight);
            writer.WriteInt32(1);

            int dataSize = actualWidth * actualHeight * 4;
            writer.WriteInt32(dataSize);

            byte[] pixelData = new byte[dataSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixelData[i * 4 + 0] = pixels[i].R;
                pixelData[i * 4 + 1] = pixels[i].G;
                pixelData[i * 4 + 2] = pixels[i].B;
                pixelData[i * 4 + 3] = pixels[i].A;
            }
            writer.WriteBytes(pixelData);

            return XnbFormat.FinalizeAndWrite(writer, fileSizePos, contentSizePos, compressed, output);
        }

    }
}
