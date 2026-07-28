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
                // 原始是 Straight Alpha：XNB 需预乘，并清零 A=0 像素 RGB。
                xnbPixels = (Color[])pixels.Clone();
                for (int i = 0; i < xnbPixels.Length; i++)
                {
                    int a = xnbPixels[i].A;
                    if (a == 0)
                        xnbPixels[i] = new Color((byte)0, (byte)0, (byte)0, (byte)0);
                    else if (a < 255)
                        xnbPixels[i] = new Color(
                            (byte)(xnbPixels[i].R * a / 255),
                            (byte)(xnbPixels[i].G * a / 255),
                            (byte)(xnbPixels[i].B * a / 255),
                            (byte)a);
                }
                if (unpackedBasePath != null)
                    pngPixels = pixels;
            }
            else
            {
                xnbPixels = (Color[])pixels.Clone();
                // SMAPI 的 PremultiplyTransparency 只处理半透明像素，跳过 A=0 像素，
                // 导致 A=0 像素仍保留原始 RGB。必须清零，否则 GPU 线性过滤在边缘采样时
                // 会把残留颜色混入可见像素，产生白线。
                ZeroTransparentPixels(xnbPixels);

                if (unpackedBasePath != null)
                {
                    // PNG 需反预乘还原为 Straight Alpha
                    pngPixels = (Color[])pixels.Clone();
                    for (int i = 0; i < pngPixels.Length; i++)
                    {
                        var p = pngPixels[i];
                        if (p.A == 0)
                            pngPixels[i] = new Color((byte)0, (byte)0, (byte)0, (byte)0);
                        else if (p.A < 255)
                        {
                            float scale = 255f / p.A;
                            pngPixels[i] = new Color(
                                (byte)Math.Min(255, p.R * scale),
                                (byte)Math.Min(255, p.G * scale),
                                (byte)Math.Min(255, p.B * scale),
                                p.A);
                        }
                    }
                }
            }

            // TileSheet 边缘像素复制（单资产导出路径）
            if (tileSheetEdgePadding > 0)
            {
                PadTileSheetEdges(xnbPixels, width, height, tileSheetEdgePadding, tileSheetEdgePadding);
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
            Color[] finalPixels = pixels;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Header
            writer.Write((byte)'X');
            writer.Write((byte)'N');
            writer.Write((byte)'B');
            writer.Write((byte)platform);
            writer.Write(xnbVersion);

            // 对照测试：iOS 暂时不压缩，排除 LZ4 压缩问题。
            // Android 保持 LZ4 压缩。
            byte flags = platform == 'a' ? (byte)0x41 : (byte)0x01;
            writer.Write(flags);

            int fileSizePosition = (int)ms.Position;
            writer.Write((uint)0);

            int contentSizePosition = -1;
            if (flags == 0x41)
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

            writer.Write(actualWidth);
            writer.Write(actualHeight);
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
            bool compressed = (flags & 0x40) != 0;
            int headerSize = compressed ? 14 : 10;
            int bodySize = uncompressedData.Length - headerSize;

            if (compressed)
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
        /// TileSheet 边缘像素复制：把每个 tile 最外一圈像素向内复制一份。
        /// 这样 GPU 线性过滤在 tile 边界采样时，采到的颜色和 tile 内部一致，
        /// 能有效减少相邻 tile 之间的可见缝隙。
        /// 仅对 OPAQUE 像素（A>0）做复制，避免把颜色灌进透明区域。
        /// </summary>
        public static void PadTileSheetEdges(Color[] pixels, int width, int height, int tileWidth, int tileHeight)
        {
            if (tileWidth < 3 || tileHeight < 3) return; // 太小没法做
            int cols = width / tileWidth;
            int rows = height / tileHeight;
            if (cols < 1 || rows < 1) return;

            for (int ty = 0; ty < rows; ty++)
            {
                for (int tx = 0; tx < cols; tx++)
                {
                    int sx = tx * tileWidth;
                    int sy = ty * tileHeight;

                    // 右边缘：倒数第二列 → 最后一列
                    for (int y = 0; y < tileHeight; y++)
                    {
                        int src = (sy + y) * width + (sx + tileWidth - 2);
                        int dst = (sy + y) * width + (sx + tileWidth - 1);
                        if (pixels[src].A > 0)
                            pixels[dst] = pixels[src];
                    }

                    // 左边缘：第二列 → 第一列
                    for (int y = 0; y < tileHeight; y++)
                    {
                        int src = (sy + y) * width + (sx + 1);
                        int dst = (sy + y) * width + (sx);
                        if (pixels[src].A > 0)
                            pixels[dst] = pixels[src];
                    }

                    // 下边缘：倒数第二行 → 最后一行
                    for (int x = 0; x < tileWidth; x++)
                    {
                        int src = (sy + tileHeight - 2) * width + (sx + x);
                        int dst = (sy + tileHeight - 1) * width + (sx + x);
                        if (pixels[src].A > 0)
                            pixels[dst] = pixels[src];
                    }

                    // 上边缘：第二行 → 第一行
                    for (int x = 0; x < tileWidth; x++)
                    {
                        int src = (sy + 1) * width + (sx + x);
                        int dst = (sy) * width + (sx + x);
                        if (pixels[src].A > 0)
                            pixels[dst] = pixels[src];
                    }
                }
            }
        }

        /// <summary>
        /// 清零所有完全透明（A=0）像素的 RGB。
        /// 预乘 Alpha 格式中 A=0 的像素 RGB 必须为 0，否则 GPU 线性过滤
        /// 在边缘采样时会把这些残留颜色混入可见像素，产生白线/色边。
        /// </summary>
        private static void ZeroTransparentPixels(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].A == 0)
                    pixels[i] = new Color((byte)0, (byte)0, (byte)0, (byte)0);
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
