using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPXnbExporter
{
    /// <summary>
    /// XNB 贴图写入器。支持 PC (w) 和移动端 (a/i) 格式。
    ///
    /// Alpha 处理策略（参考 SMAPI 加载机制和原版内容管道）：
    ///
    /// SMAPI 加载机制 (ModContentManager.LoadRawImageData)：
    ///   - PNG 纹理通过 SKPMColor.PreMultiply 预乘 Alpha
    ///   - A=0 像素强制为 Color.Transparent (RGB=0,0,0)
    ///   - 半透明像素 (0&lt;A&lt;255) 的 RGB 已预乘 (R*A/255)
    ///   - 不透明像素 (A=255) 保持不变
    ///   - 无边缘 padding / extrusion / alpha bleeding
    ///
    /// 原版 XNB 加载机制：
    ///   - XNB 纹理在内容管道 (MGCB) 构建时已预乘 Alpha
    ///   - Texture2DReader 直接读取预乘像素，不做额外处理
    ///   - SurfaceFormat.Color (0)，1 mip level
    ///
    /// 导出策略：
    ///   - XNB：直接使用 SMAPI 加载的预乘像素（与原版 XNB 一致）
    ///   - PNG：反预乘为非预乘像素（供 XnbConverter/MGCB 重新打包时正确预乘）
    ///   - A=0 像素统一规范化为 Color.Transparent
    /// </summary>
    public static class XnbWriter
    {
        private const string Texture2DReaderFull =
            "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, " +
            "Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

        private const string Texture2DReaderShort =
            "Microsoft.Xna.Framework.Content.Texture2DReader";

        /// <summary>
        /// 规范化 Alpha：A=0 像素强制为 Color.Transparent。
        /// 匹配 SMAPI LoadRawImageData 行为，防止 RGB 残留导致 GPU 线性过滤渗色。
        /// 就地修改，不分配新数组。
        /// </summary>
        public static void NormalizeAlpha(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].A == 0)
                    pixels[i] = Color.Transparent;
            }
        }

        /// <summary>
        /// 将预乘 Alpha 像素转换为非预乘像素（用于 PNG 输出）。
        /// 这样 XnbConverter/MGCB 重新打包 PNG 为 XNB 时，会正确执行预乘，
        /// 避免二次预乘导致的暗化/缝隙。
        /// </summary>
        public static Color[] UnpremultiplyAlpha(Color[] premultiplied)
        {
            var result = new Color[premultiplied.Length];
            for (int i = 0; i < premultiplied.Length; i++)
            {
                byte a = premultiplied[i].A;
                if (a == 0)
                    result[i] = Color.Transparent;
                else if (a == 255)
                    result[i] = premultiplied[i];
                else
                {
                    result[i] = new Color(
                        (byte)Math.Min(255, premultiplied[i].R * 255 / a),
                        (byte)Math.Min(255, premultiplied[i].G * 255 / a),
                        (byte)Math.Min(255, premultiplied[i].B * 255 / a),
                        a);
                }
            }
            return result;
        }

        /// <summary>
        /// 导出贴图资产为 XNB（packed）和可选的 PNG + .config（unpacked）。
        /// 主线程调用版本：直接接受 Texture2D（用于单资产导出命令）。
        /// XNB 使用预乘像素（与原版一致），PNG 使用非预乘像素（供重新打包）。
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

            // Alpha 规范化：A=0 → Color.Transparent（匹配 SMAPI LoadRawImageData）
            NormalizeAlpha(pixels);

            // XNB：直接使用预乘像素（与原版 XNB 一致）
            Color[] xnbPixels = pixels;
            // PNG：反预乘为非预乘像素（供 XnbConverter/MGCB 重新打包时正确预乘）
            Color[] pngPixels = (unpackedBasePath != null) ? UnpremultiplyAlpha(pixels) : null;

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

            // 像素数据应为预乘 Alpha 格式（与原版 XNB 一致）。
            // 调用方负责在调用前完成 NormalizeAlpha 规范化。
            // 参考：SMAPI LoadRawImageData 对 PNG 预乘，原版 MGCB 构建时预乘。

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
