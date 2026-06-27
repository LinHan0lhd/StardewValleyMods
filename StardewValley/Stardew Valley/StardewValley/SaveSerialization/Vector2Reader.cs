using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley.SaveSerialization
{
	// Token: 0x0200017E RID: 382
	public class Vector2Reader : XmlSerializationReader
	{
		// Token: 0x06001C5B RID: 7259 RVA: 0x00141104 File Offset: 0x0013F304
		public Vector2 ReadVector2()
		{
			XmlReader reader = base.Reader;
			reader.ReadStartElement("Vector2");
			reader.ReadStartElement("X");
			float x = reader.ReadContentAsFloat();
			reader.ReadEndElement();
			reader.ReadStartElement("Y");
			float y = reader.ReadContentAsFloat();
			reader.ReadEndElement();
			reader.ReadEndElement();
			return new Vector2(x, y);
		}

		// Token: 0x06001C5C RID: 7260 RVA: 0x0014115E File Offset: 0x0013F35E
		protected override void InitCallbacks()
		{
		}

		// Token: 0x06001C5D RID: 7261 RVA: 0x00141160 File Offset: 0x0013F360
		protected override void InitIDs()
		{
		}
	}
}
