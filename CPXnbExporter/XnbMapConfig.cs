using Newtonsoft.Json;

namespace CPXnbExporter
{
    public class XnbMapConfig
    {
        public string AssetName { get; set; }
        public string Platform { get; set; }
        public int TileSheetCount { get; set; }
        public int LayerCount { get; set; }

        public string ToConfig()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
