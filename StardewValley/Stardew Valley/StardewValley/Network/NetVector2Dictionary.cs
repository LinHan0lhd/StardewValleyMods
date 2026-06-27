using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001EC RID: 492
	public sealed class NetVector2Dictionary<T, TField> : NetFieldDictionary<Vector2, T, TField, SerializableDictionary<Vector2, T>, NetVector2Dictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x060021B4 RID: 8628 RVA: 0x00174199 File Offset: 0x00172399
		public NetVector2Dictionary()
		{
		}

		// Token: 0x060021B5 RID: 8629 RVA: 0x001741A1 File Offset: 0x001723A1
		public NetVector2Dictionary(IEnumerable<KeyValuePair<Vector2, T>> dict) : base(dict)
		{
		}

		// Token: 0x060021B6 RID: 8630 RVA: 0x001741AC File Offset: 0x001723AC
		protected override Vector2 ReadKey(BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new Vector2(x, y);
		}

		// Token: 0x060021B7 RID: 8631 RVA: 0x001741CC File Offset: 0x001723CC
		protected override void WriteKey(BinaryWriter writer, Vector2 key)
		{
			writer.Write(key.X);
			writer.Write(key.Y);
		}
	}
}
