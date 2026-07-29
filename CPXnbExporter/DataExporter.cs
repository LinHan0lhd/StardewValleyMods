using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CPXnbExporter
{
    /// <summary>数据导出工具。参考 ContentPatcher ExportCommand 的 JSON 设置模式。</summary>
    public static class DataExporter
    {
        /// <summary>The settings to use when writing data to a JSON file.</summary>
        private static readonly Lazy<JsonSerializerSettings> JsonSettings = new(() => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = { new StringEnumConverter() }
        });

        public static bool ExportData(string filePath, object data, string assetName)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(data, JsonSettings.Value);
                File.WriteAllText(filePath + ".json", json, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                // 静默失败，由调用方记录日志
                return false;
            }
        }
    }
}
