using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using StardewValley.SaveSerialization;

namespace StardewValley
{
	// Token: 0x020000FD RID: 253
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		// Token: 0x06001451 RID: 5201 RVA: 0x000F6607 File Offset: 0x000F4807
		public SerializableDictionary()
		{
		}

		// Token: 0x06001452 RID: 5202 RVA: 0x000F660F File Offset: 0x000F480F
		public SerializableDictionary(IDictionary<TKey, TValue> data) : base(data)
		{
		}

		// Token: 0x06001453 RID: 5203 RVA: 0x000F6618 File Offset: 0x000F4818
		public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceValue>(IDictionary<TKey, TSourceValue> data, Func<TSourceValue, TValue> getValue)
		{
			SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
			foreach (KeyValuePair<TKey, TSourceValue> entry in data)
			{
				result[entry.Key] = getValue(entry.Value);
			}
			return result;
		}

		// Token: 0x06001454 RID: 5204 RVA: 0x000F667C File Offset: 0x000F487C
		public static SerializableDictionary<TKey, TValue> BuildFrom<TSourceKey, TSourceValue>(IDictionary<TSourceKey, TSourceValue> data, Func<TSourceKey, TKey> getKey, Func<TSourceValue, TValue> getValue)
		{
			SerializableDictionary<TKey, TValue> result = new SerializableDictionary<TKey, TValue>();
			foreach (KeyValuePair<TSourceKey, TSourceValue> entry in data)
			{
				result[getKey(entry.Key)] = getValue(entry.Value);
			}
			return result;
		}

		// Token: 0x06001455 RID: 5205 RVA: 0x000F66E4 File Offset: 0x000F48E4
		protected SerializableDictionary(IEqualityComparer<TKey> comparer = null) : base(comparer)
		{
		}

		// Token: 0x06001456 RID: 5206 RVA: 0x000F66ED File Offset: 0x000F48ED
		protected SerializableDictionary(IDictionary<TKey, TValue> data, IEqualityComparer<TKey> comparer = null) : base(data, comparer)
		{
		}

		// Token: 0x06001457 RID: 5207 RVA: 0x000F66F7 File Offset: 0x000F48F7
		public new void Add(TKey key, TValue value)
		{
			base.Add(key, value);
			this.OnCollectionChanged(this, new SerializableDictionary<TKey, TValue>.ChangeArgs(ChangeType.Add, key, value));
		}

		// Token: 0x06001458 RID: 5208 RVA: 0x000F6710 File Offset: 0x000F4910
		public new bool Remove(TKey key)
		{
			TValue val;
			if (base.TryGetValue(key, out val))
			{
				base.Remove(key);
				this.OnCollectionChanged(this, new SerializableDictionary<TKey, TValue>.ChangeArgs(ChangeType.Remove, key, val));
				return true;
			}
			return false;
		}

		// Token: 0x06001459 RID: 5209 RVA: 0x000F6744 File Offset: 0x000F4944
		public new void Clear()
		{
			base.Clear();
			this.OnCollectionChanged(this, new SerializableDictionary<TKey, TValue>.ChangeArgs(ChangeType.Clear, default(TKey), default(TValue)));
		}

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x0600145A RID: 5210 RVA: 0x000F6778 File Offset: 0x000F4978
		// (remove) Token: 0x0600145B RID: 5211 RVA: 0x000F67B0 File Offset: 0x000F49B0
		public event SerializableDictionary<TKey, TValue>.ChangeCallback CollectionChanged;

		// Token: 0x0600145C RID: 5212 RVA: 0x000F67E5 File Offset: 0x000F49E5
		private void OnCollectionChanged(object sender, SerializableDictionary<TKey, TValue>.ChangeArgs args)
		{
			SerializableDictionary<TKey, TValue>.ChangeCallback collectionChanged = this.CollectionChanged;
			if (collectionChanged == null)
			{
				return;
			}
			collectionChanged(sender ?? this, args);
		}

		// Token: 0x0600145D RID: 5213 RVA: 0x000F67FE File Offset: 0x000F49FE
		public XmlSchema GetSchema()
		{
			return null;
		}

		// Token: 0x0600145E RID: 5214 RVA: 0x000F6804 File Offset: 0x000F4A04
		public void ReadXml(XmlReader reader)
		{
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (isEmptyElement)
			{
				return;
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");
				reader.ReadStartElement("key");
				bool read = false;
				TKey key = default(TKey);
				if (typeof(TKey) == typeof(string))
				{
					string name = reader.Name;
					if (!(name == "int"))
					{
						if (name == "LocationContext")
						{
							reader.ReadStartElement();
							key = (TKey)((object)Convert.ChangeType(reader.ReadContentAsString(), typeof(TKey)));
							reader.ReadEndElement();
							read = true;
						}
					}
					else
					{
						key = (TKey)((object)Convert.ChangeType(SaveSerializer.Deserialize<int>(reader), typeof(TKey)));
						read = true;
					}
				}
				if (!read)
				{
					key = (TKey)((object)SerializableDictionary<TKey, TValue>._keySerializer.DeserializeFast(reader));
				}
				reader.ReadEndElement();
				reader.ReadStartElement("value");
				TValue value = default(TValue);
				read = false;
				if (typeof(TValue) == typeof(string) && reader.Name == "int")
				{
					value = (TValue)((object)Convert.ChangeType(SaveSerializer.Deserialize<int>(reader), typeof(TValue)));
					read = true;
				}
				if (!read)
				{
					value = (TValue)((object)SerializableDictionary<TKey, TValue>._valueSerializer.DeserializeFast(reader));
				}
				reader.ReadEndElement();
				this.AddDuringDeserialization(key, value);
				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		// Token: 0x0600145F RID: 5215 RVA: 0x000F6990 File Offset: 0x000F4B90
		public void WriteXml(XmlWriter writer)
		{
			foreach (TKey key in base.Keys)
			{
				writer.WriteStartElement("item");
				writer.WriteStartElement("key");
				SerializableDictionary<TKey, TValue>._keySerializer.SerializeFast(writer, key);
				writer.WriteEndElement();
				writer.WriteStartElement("value");
				TValue value = base[key];
				SerializableDictionary<TKey, TValue>._valueSerializer.SerializeFast(writer, value);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}

		// Token: 0x06001460 RID: 5216 RVA: 0x000F6A3C File Offset: 0x000F4C3C
		protected virtual void AddDuringDeserialization(TKey key, TValue value)
		{
			base.Add(key, value);
		}

		// Token: 0x04000D17 RID: 3351
		private static XmlSerializer _keySerializer = SaveSerializer.GetSerializer(typeof(TKey));

		// Token: 0x04000D18 RID: 3352
		private static XmlSerializer _valueSerializer = SaveSerializer.GetSerializer(typeof(TValue));

		// Token: 0x020004DA RID: 1242
		public struct ChangeArgs
		{
			// Token: 0x06003F9B RID: 16283 RVA: 0x002FFD84 File Offset: 0x002FDF84
			public ChangeArgs(ChangeType type, TKey k, TValue v)
			{
				this.Type = type;
				this.Key = k;
				this.Value = v;
			}

			// Token: 0x040029BF RID: 10687
			public readonly ChangeType Type;

			// Token: 0x040029C0 RID: 10688
			public readonly TKey Key;

			// Token: 0x040029C1 RID: 10689
			public readonly TValue Value;
		}

		// Token: 0x020004DB RID: 1243
		// (Invoke) Token: 0x06003F9D RID: 16285
		public delegate void ChangeCallback(object sender, SerializableDictionary<TKey, TValue>.ChangeArgs args);
	}
}
