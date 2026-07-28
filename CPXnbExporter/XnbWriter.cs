using System;
using System.IO;
using System.Text;
using K4os.Compression.LZ4;
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
        /// </summary>
        public static XnbMetadata ExportTextureSet(
            string packedBasePath,
            string unpackedBasePath,
            Texture2D texture,
            char platform = 'a',
            byte xnbVersion = 5)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(packedBasePath));
            if (!string.IsNullOrEmpty(unpackedBasePath))
                Directory.CreateDirectory(Path.GetDirectoryName(unpackedBasePath));

            int width = texture.Width;
            int height = texture.Height;
            var pixels = new Color[width * height];
            texture.GetData(pixels);

            // 检测 Alpha 格式并转换为预乘（XNB SurfaceFormat.Color 要求预乘 Alpha）
            bool isStraightAlpha = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.R > p.A || p.G > p.A || p.B > p.A)
                {
                    isStraightAlpha = true;
                    break;
                }
            }

            // XNB 像素（预乘格式）
            Color[] xnbPixels = pixels;
            // PNG 像素（Straight Alpha 格式）
            Color[] pngPixels = null;

            if (isStraightAlpha)
            {
                // 原始是 Straight Alpha：XNB 需预乘，PNG 直接用原始
                xnbPixels = (Color[])pixels.Clone();
                for (int i = 0; i < xnbPixels.Length; i++)
                {
                    int a = xnbPixels[i].A;
                    if (a == 0)
                        xnbPixels[i] = new Color(0, 0, 0, 0);
                    else if (a < 255)
                        xnbPixels[i] = new Color(
                            (byte)(xnbPixels[i].R * a / 255),
                            (byte)(xnbPixels[i].G * a / 255),
                            (byte)(xnbPixels[i].B * a / 255), a);
                }
                if (unpackedBasePath != null)
                    pngPixels = pixels;
            }
            else if (unpackedBasePath != null)
            {
                // 原始是预乘 Alpha：PNG 需反预乘
                pngPixels = (Color[])pixels.Clone();
                for (int i = 0; i < pngPixels.Length; i++)
                {
                    var p = pngPixels[i];
                    if (p.A == 0)
                        pngPixels[i] = new Color(0, 0, 0, 0);
                    else if (p.A < 255)
                    {
                        float scale = 255f / p.A;
                        pngPixels[i] = new Color(
                            (byte)Math.Min(255, p.R * scale),
                            (byte)Math.Min(255, p.G * scale),
                            (byte)Math.Min(255, p.B * scale), p.A);
                    }
                }
            }

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
                Compressed = (platform == 'a' || platform == 'i'),
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
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Header
            writer.Write((byte)'X');
            writer.Write((byte)'N');
            writer.Write((byte)'B');
            writer.Write((byte)platform);
            writer.Write(xnbVersion);

            byte flags = isMobile ? (byte)0x41 : (byte)0x01;
            writer.Write(flags);

            int fileSizePosition = (int)ms.Position;
            writer.Write((uint)0);

            int contentSizePosition = -1;
            if (isMobile)
            {
                contentSizePosition = (int)ms.Position;
                writer.Write((uint)0);
            }

            // Type reader
            Write7BitEncodedInt(writer, 1);
            Write7BitEncodedString(writer, readerName);
            writer.Write((uint)0);

            // Shared resources
            Write7BitEncodedInt(writer, 0);

            // Object Type ID
            Write7BitEncodedInt(writer, 1);

            // Surface format
            writer.Write((int)0);
            writer.Write(width);
            writer.Write(height);
            writer.Write(1);

            int dataSize = width * height * 4;
            writer.Write(dataSize);

            byte[] pixelData = new byte[dataSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixelData[i * 4 + 0] = pixels[i].R;
                pixelData[i * 4 + 1] = pixels[i].G;
                pixelData[i * 4 + 2] = pixels[i].B;
                pixelData[i * 4 + 3] = pixels[i].A;
            }
            writer.Write(pixelData);
            writer.Flush();

            byte[] uncompressedData = ms.ToArray();
            int headerSize = isMobile ? 14 : 10;
            int bodySize = uncompressedData.Length - headerSize;

            if (isMobile)
            {
                byte[] bodyBytes = new byte[bodySize];
                Array.Copy(uncompressedData, headerSize, bodyBytes, 0, bodySize);

                int maxCompressedSize = LZ4Codec.MaximumOutputSize(bodySize);
                byte[] compressedBody = new byte[maxCompressedSize];
                int compressedSize = LZ4Codec.Encode(bodyBytes, 0, bodySize, compressedBody, 0, maxCompressedSize);

                byte[] finalData = new byte[headerSize + compressedSize];
                Array.Copy(uncompressedData, 0, finalData, 0, headerSize);
                Array.Copy(compressedBody, 0, finalData, headerSize, compressedSize);

                byte[] fileSizeBytes = BitConverter.GetBytes((uint)finalData.Length);
                Array.Copy(fileSizeBytes, 0, finalData, fileSizePosition, 4);

                byte[] contentSizeBytes = BitConverter.GetBytes((uint)bodySize);
                Array.Copy(contentSizeBytes, 0, finalData, contentSizePosition, 4);

                output.Write(finalData, 0, finalData.Length);
                output.Flush();
                return finalData.Length;
            }
            else
            {
                byte[] fileSizeBytes = BitConverter.GetBytes((uint)uncompressedData.Length);
                Array.Copy(fileSizeBytes, 0, uncompressedData, fileSizePosition, 4);

                output.Write(uncompressedData, 0, uncompressedData.Length);
                output.Flush();
                return uncompressedData.Length;
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
