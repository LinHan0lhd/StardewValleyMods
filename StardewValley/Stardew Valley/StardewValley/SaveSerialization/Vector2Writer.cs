using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley.SaveSerialization
{
	// Token: 0x02000180 RID: 384
	public class Vector2Writer : XmlSerializationWriter
	{
		// Token: 0x06001C65 RID: 7269 RVA: 0x001411D4 File Offset: 0x0013F3D4
		public void WriteVector2(Vector2 vec)
		{
			XmlWriter writer = base.Writer;
			writer.WriteStartElement("Vector2");
			writer.WriteStartElement("X");
			writer.WriteValue(vec.X);
			writer.WriteEndElement();
			writer.WriteStartElement("Y");
			writer.WriteValue(vec.Y);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		// Token: 0x06001C66 RID: 7270 RVA: 0x00141231 File Offset: 0x0013F431
		protected override void InitCallbacks()
		{
		}
	}
}
