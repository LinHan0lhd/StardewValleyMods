namespace CPXnbExporter
{
    /// <summary>移动端兼容性配置</summary>
    public static class MobileConfig
    {
        public static class Platform
        {
            public const char Windows = 'w';
            public const char Android = 'a';
            public const char IOS = 'i';
        }

        public static class Paths
        {
            public const string AndroidContent =
                "/storage/emulated/0/Android/data/com.chucklefish.stardewvalley/files/Content/";

            public const string AndroidContentAlt =
                "/sdcard/Android/data/com.chucklefish.stardewvalley/files/Content/";
        }

        public static class Features
        {
            public const int DefaultSurfaceFormat = 0;
            public const int DefaultMipCount = 1;
            public const bool DefaultCompressed = false;
        }
    }
}
