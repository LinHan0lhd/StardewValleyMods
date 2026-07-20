using System;
using xTile;

namespace CPXnbExporter
{
    public static class MapConverter
    {
        public static byte[] ConvertToTBin(Map map)
        {
            return TBinWriter.SerializeTbin(map);
        }

        public static Map ConvertFromTBin(byte[] data)
        {
            return TBinReader.ReadTbin(data);
        }

        public static byte[] ConvertXnbMapToTBin(byte[] xnbData)
        {
            var map = XnbMapReader.ReadMap(xnbData);
            return ConvertToTBin(map);
        }

        public static Map CloneMap(Map source)
        {
            var tbinData = ConvertToTBin(source);
            return ConvertFromTBin(tbinData);
        }
    }
}
