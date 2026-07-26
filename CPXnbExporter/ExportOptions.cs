namespace CPXnbExporter
{
    /// <summary>导出选项</summary>
    public readonly struct ExportOptions
    {
        /// <summary>平台标识: 'w'=PC, 'a'=移动端</summary>
        public char Platform { get; init; }

        /// <summary>是否同时输出 unpacked (PNG/TBIN + .config)</summary>
        public bool OutputUnpacked { get; init; }

        /// <summary>导出基础目录</summary>
        public string BaseDir { get; init; }

        /// <summary>Packed 输出目录</summary>
        public string PackedDir => System.IO.Path.Combine(BaseDir, "packed");

        /// <summary>Unpacked 输出目录</summary>
        public string UnpackedDir => System.IO.Path.Combine(BaseDir, "unpacked");

        /// <summary>Troubleshoot 排查输出目录</summary>
        public string TroubleshootDir => System.IO.Path.Combine(BaseDir, "troubleshootDir");

        /// <summary>从命令参数解析</summary>
        public static ExportOptions Parse(string[] args, string baseDir)
        {
            char platform = 'a';
            bool unpacked = false;

            foreach (var arg in args)
            {
                string lower = arg.ToLowerInvariant();
                switch (lower)
                {
                    case "pc":
                    case "w":
                    case "windows":
                        platform = 'w';
                        break;
                    case "mobile":
                    case "a":
                    case "android":
                    case "i":
                    case "ios":
                        platform = 'a';
                        break;
                    case "unpacked":
                    case "u":
                        unpacked = true;
                        break;
                }
            }

            return new ExportOptions
            {
                Platform = platform,
                OutputUnpacked = unpacked,
                BaseDir = baseDir
            };
        }
    }
}
