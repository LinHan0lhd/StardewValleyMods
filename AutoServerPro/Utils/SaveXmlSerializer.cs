#nullable disable
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using StardewValley;
using AutoServerPro.Models;

namespace AutoServerPro.Utils;

public static class SaveXmlSerializer
{
    private static readonly Type[] ItemDerivedTypes;
    public static readonly XmlSerializer ItemSerializer;
    public static readonly XmlSerializer SnapshotSerializer;

    static SaveXmlSerializer()
    {
        ItemDerivedTypes = Assembly.GetAssembly(typeof(Item))
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract)
            .ToArray();

        ItemSerializer = new XmlSerializer(typeof(ItemsWrapper), ItemDerivedTypes);
        SnapshotSerializer = new XmlSerializer(typeof(GameStateSnapshot));
    }

    public static string SerializeItem(Item item)
    {
        if (item == null) return null;
        try
        {
            var wrapper = new ItemsWrapper { Items = new[] { item } };
            using var ms = new MemoryStream();
            ItemSerializer.Serialize(ms, wrapper);
            ms.Position = 0;
            return new StreamReader(ms).ReadToEnd();
        }
        catch { return null; }
    }

    public static Item DeserializeItem(string xml)
    {
        try
        {
            if (string.IsNullOrEmpty(xml)) return null;
            using var sr = new StringReader(xml);
            var wrapper = (ItemsWrapper)ItemSerializer.Deserialize(sr);
            return wrapper?.Items?.FirstOrDefault();
        }
        catch { return null; }
    }
}
