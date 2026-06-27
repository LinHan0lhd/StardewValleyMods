using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley.SaveSerialization
{
	// Token: 0x0200017F RID: 383
	public class Vector2Serializer : XmlSerializer
	{
		// Token: 0x06001C5F RID: 7263 RVA: 0x0014116A File Offset: 0x0013F36A
		public Vector2Serializer() : base(typeof(Vector2))
		{
		}

		// Token: 0x06001C60 RID: 7264 RVA: 0x00141192 File Offset: 0x0013F392
		protected override XmlSerializationReader CreateReader()
		{
			return this._reader;
		}

		// Token: 0x06001C61 RID: 7265 RVA: 0x0014119A File Offset: 0x0013F39A
		protected override XmlSerializationWriter CreateWriter()
		{
			return this._writer;
		}

		// Token: 0x06001C62 RID: 7266 RVA: 0x001411A2 File Offset: 0x0013F3A2
		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("Vector2");
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x001411AF File Offset: 0x0013F3AF
		protected override void Serialize(object o, XmlSerializationWriter writer)
		{
			this._writer.WriteVector2((Vector2)o);
		}

		// Token: 0x06001C64 RID: 7268 RVA: 0x001411C2 File Offset: 0x0013F3C2
		protected override object Deserialize(XmlSerializationReader reader)
		{
			return this._reader.ReadVector2();
		}

		// Token: 0x04001119 RID: 4377
		private Vector2Reader _reader = new Vector2Reader();

		// Token: 0x0400111A RID: 4378
		private Vector2Writer _writer = new Vector2Writer();
	}
}
