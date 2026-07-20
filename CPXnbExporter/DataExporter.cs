using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CPXnbExporter
{
    /// <summary>数据导出工具。v2.0 统一使用 Newtonsoft.Json。</summary>
    public static class DataExporter
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

        public static bool ExportData(string filePath, object data, string assetName)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(data, JsonSettings);
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
