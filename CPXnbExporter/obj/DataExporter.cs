using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CPXnbExporter
{
    public static class DataExporter
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        public static bool ExportData(string filePath, object data, string assetName)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(filePath + ".json", json, System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
