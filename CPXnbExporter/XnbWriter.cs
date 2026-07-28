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

            // 检测 Alpha 格式：只检查半透明像素（0 < A < 255）
            // A=0 的 RGB 残留不代表格式（SMAPI 跳过 A=0），A=255 时 R≤A 恒成立无法判断
            bool isStraightAlpha = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.A > 0 && p.A < 255)
                {
                    if (p.R > p.A || p.G > p.A || p.B > p.A)
                    {
                        isStraightAlpha = true;
                        break;
                    }
                }
            }

            // XNB 像素（预乘格式）
            Color[] xnbPixels = pixels;
            // PNG 像素（Straight Alpha 格式）
            Color[] pngPixels = null;

            if (isStraightAlpha)
            {
                // 原始是 Straight Alpha：XNB 需预乘，再做 Alpha Bleeding，PNG 直接用原始
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
                AlphaBleed(xnbPixels, width, height);
                if (unpackedBasePath != null)
                    pngPixels = pixels;
            }
            else
            {
                // 原始已是预乘 Alpha：做 Alpha Bleeding 填充 A=0 像素 RGB，
                // 防止移动端 GPU block compression 污染半透明边缘形成白线/黑缝。
                xnbPixels = (Color[])pixels.Clone();
                AlphaBleed(xnbPixels, width, height);

                if (unpackedBasePath != null)
                {
                    // PNG 需反预乘还原为 Straight Alpha
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

            // iOS 版 Stardew Valley 要求 Texture2D 尺寸为 2 的幂（POT）。
            // 非 POT 纹理需要 padding 到 POT，并把 width/height 编码为：
            //   高 16 位 = 原始使用尺寸（usedWidth/usedHeight）
            //   低 16 位 = POT 后的实际尺寸（actualWidth/actualHeight）
            // Android 目前未发现此要求，仅对 iOS 处理。
            int usedWidth = width;
            int usedHeight = height;
            int actualWidth = width;
            int actualHeight = height;
            Color[] finalPixels = pixels;

            if (platform == 'i')
            {
                int potWidth = NextPowerOfTwo(width);
                int potHeight = NextPowerOfTwo(height);
                if (potWidth != width || potHeight != height)
                {
                    actualWidth = potWidth;
                    actualHeight = potHeight;
                    finalPixels = PadPixelsToSize(pixels, width, height, actualWidth, actualHeight);
                }
            }

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

            // iOS POT 编码：高16位=used，低16位=actual
            int encodedWidth = platform == 'i' ? ((usedWidth << 16) | (actualWidth & 0xFFFF)) : actualWidth;
            int encodedHeight = platform == 'i' ? ((usedHeight << 16) | (actualHeight & 0xFFFF)) : actualHeight;
            writer.Write(encodedWidth);
            writer.Write(encodedHeight);
            writer.Write(1);

            int dataSize = actualWidth * actualHeight * 4;
            writer.Write(dataSize);

            byte[] pixelData = new byte[dataSize];
            for (int i = 0; i < finalPixels.Length; i++)
            {
                pixelData[i * 4 + 0] = finalPixels[i].R;
                pixelData[i * 4 + 1] = finalPixels[i].G;
                pixelData[i * 4 + 2] = finalPixels[i].B;
                pixelData[i * 4 + 3] = finalPixels[i].A;
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

        /// <summary>
        /// 计算不小于 value 的最小 2 的幂（Power of Two）。
        /// 若 value 本身已是 POT，则返回 value；最大支持到 65535（16 位）。
        /// </summary>
        private static int NextPowerOfTwo(int value)
        {
            if (value <= 1) return 1;
            if (value > 0x8000) return 0x10000; // 65536

            int pot = 1;
            while (pot < value)
                pot <<= 1;
            return pot;
        }

        /// <summary>
        /// 把像素数组 padding 到更大的尺寸。
        /// 新区域用原始内容的边缘像素"extrude"填充，而不是透明黑色，
        /// 避免 iOS GPU block compression 把黑色/白色污染到有效内容边缘。
        /// 用于 iOS 版 Stardew Valley 的 POT 纹理要求。
        /// </summary>
        private static Color[] PadPixelsToSize(Color[] pixels, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            var result = new Color[dstWidth * dstHeight];
            // 1. 复制原始内容到左上角
            for (int y = 0; y < srcHeight; y++)
            {
                int srcOffset = y * srcWidth;
                int dstOffset = y * dstWidth;
                Array.Copy(pixels, srcOffset, result, dstOffset, srcWidth);
            }
            // 2. 把最右列复制到右侧 padding
            for (int y = 0; y < srcHeight; y++)
            {
                int dstOffset = y * dstWidth + srcWidth - 1;
                Color rightEdge = result[dstOffset];
                for (int x = srcWidth; x < dstWidth; x++)
                    result[y * dstWidth + x] = rightEdge;
            }
            // 3. 把最底行复制到底部 padding（包括右下角区域）
            int lastSrcRow = (srcHeight - 1) * dstWidth;
            for (int y = srcHeight; y < dstHeight; y++)
            {
                int dstRowOffset = y * dstWidth;
                for (int x = 0; x < dstWidth; x++)
                    result[dstRowOffset + x] = result[lastSrcRow + x];
            }
            return result;
        }

        /// <summary>
        /// 对完全透明（A=0）像素做 Alpha Bleeding：用最近的不透明像素颜色填充其 RGB。
        /// 防止移动端 GPU 的 PVRTC/ETC 等 block compression 把透明像素 RGB 污染到半透明边缘。
        /// </summary>
        private static void AlphaBleed(Color[] pixels, int width, int height, int maxIterations = 32)
        {
            int count = width * height;
            var filled = new bool[count];
            for (int i = 0; i < count; i++)
                filled[i] = pixels[i].A > 0;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                bool changed = false;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = y * width + x;
                        if (filled[i]) continue;

                        int r = 0, g = 0, b = 0, n = 0;
                        if (x > 0 && filled[i - 1])
                        {
                            r += pixels[i - 1].R; g += pixels[i - 1].G; b += pixels[i - 1].B; n++;
                        }
                        if (x < width - 1 && filled[i + 1])
                        {
                            r += pixels[i + 1].R; g += pixels[i + 1].G; b += pixels[i + 1].B; n++;
                        }
                        if (y > 0 && filled[i - width])
                        {
                            r += pixels[i - width].R; g += pixels[i - width].G; b += pixels[i - width].B; n++;
                        }
                        if (y < height - 1 && filled[i + width])
                        {
                            r += pixels[i + width].R; g += pixels[i + width].G; b += pixels[i + width].B; n++;
                        }

                        if (n > 0)
                        {
                            pixels[i] = new Color((byte)(r / n), (byte)(g / n), (byte)(b / n), 0);
                            filled[i] = true;
                            changed = true;
                        }
                    }
                }
                if (!changed) break;
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
