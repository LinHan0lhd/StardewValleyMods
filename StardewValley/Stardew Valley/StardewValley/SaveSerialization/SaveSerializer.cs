using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.Quests;
using StardewValley.SaveMigrations;

namespace StardewValley.SaveSerialization
{
	// Token: 0x0200017D RID: 381
	public static class SaveSerializer
	{
		// Token: 0x06001C52 RID: 7250 RVA: 0x00140FD0 File Offset: 0x0013F1D0
		public static XmlSerializer GetSerializer(Type type)
		{
			XmlSerializer obj;
			if (!SaveSerializer._serializerLookup.TryGetValue(type, out obj))
			{
				if (type == typeof(SaveGame))
				{
					return SaveGame.serializer;
				}
				if (type == typeof(Farmer))
				{
					return SaveGame.farmerSerializer;
				}
				if (type == typeof(GameLocation))
				{
					return SaveGame.locationSerializer;
				}
				if (type == typeof(DescriptionElement))
				{
					return SaveGame.descriptionElementSerializer;
				}
				if (type == typeof(SaveMigrator_1_6.LegacyDescriptionElement))
				{
					return SaveGame.legacyDescriptionElementSerializer;
				}
				obj = new XmlSerializer(type);
				SaveSerializer._serializerLookup.Add(type, obj);
			}
			return obj;
		}

		// Token: 0x06001C53 RID: 7251 RVA: 0x0014107B File Offset: 0x0013F27B
		public static void SerializeFast(this XmlSerializer serializer, Stream stream, object obj)
		{
			serializer.Serialize(stream, obj);
		}

		// Token: 0x06001C54 RID: 7252 RVA: 0x00141085 File Offset: 0x0013F285
		public static void Serialize<T>(XmlWriter xmlWriter, T obj)
		{
			SaveSerializer.GetSerializer(typeof(T)).SerializeFast(xmlWriter, obj);
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x001410A2 File Offset: 0x0013F2A2
		public static void SerializeFast(this XmlSerializer serializer, XmlWriter xmlWriter, object obj)
		{
			serializer.Serialize(xmlWriter, obj);
		}

		// Token: 0x06001C56 RID: 7254 RVA: 0x001410AC File Offset: 0x0013F2AC
		public static T Deserialize<T>(Stream stream)
		{
			return (T)((object)SaveSerializer.GetSerializer(typeof(T)).DeserializeFast(stream));
		}

		// Token: 0x06001C57 RID: 7255 RVA: 0x001410C8 File Offset: 0x0013F2C8
		public static T Deserialize<T>(XmlReader reader)
		{
			return (T)((object)SaveSerializer.GetSerializer(typeof(T)).DeserializeFast(reader));
		}

		// Token: 0x06001C58 RID: 7256 RVA: 0x001410E4 File Offset: 0x0013F2E4
		public static object DeserializeFast(this XmlSerializer serializer, Stream stream)
		{
			return serializer.Deserialize(stream);
		}

		// Token: 0x06001C59 RID: 7257 RVA: 0x001410ED File Offset: 0x0013F2ED
		public static object DeserializeFast(this XmlSerializer serializer, XmlReader reader)
		{
			return serializer.Deserialize(reader);
		}

		// Token: 0x04001118 RID: 4376
		private static readonly Dictionary<Type, XmlSerializer> _serializerLookup = new Dictionary<Type, XmlSerializer>();
	}
}
