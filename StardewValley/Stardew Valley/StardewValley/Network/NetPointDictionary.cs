using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E9 RID: 489
	public class NetPointDictionary<T, TField> : NetFieldDictionary<Point, T, TField, SerializableDictionary<Point, T>, NetPointDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x06002198 RID: 8600 RVA: 0x00173DE8 File Offset: 0x00171FE8
		public NetPointDictionary()
		{
		}

		// Token: 0x06002199 RID: 8601 RVA: 0x00173DF0 File Offset: 0x00171FF0
		public NetPointDictionary(IEnumerable<KeyValuePair<Point, T>> dict) : base(dict)
		{
		}

		// Token: 0x0600219A RID: 8602 RVA: 0x00173DFC File Offset: 0x00171FFC
		protected override Point ReadKey(BinaryReader reader)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			return new Point(x, y);
		}

		// Token: 0x0600219B RID: 8603 RVA: 0x00173E1C File Offset: 0x0017201C
		protected override void WriteKey(BinaryWriter writer, Point key)
		{
			writer.Write(key.X);
			writer.Write(key.Y);
		}
	}
}
