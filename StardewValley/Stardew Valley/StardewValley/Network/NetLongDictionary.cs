using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E4 RID: 484
	public class NetLongDictionary<T, TField> : NetFieldDictionary<long, T, TField, SerializableDictionary<long, T>, NetLongDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x06002172 RID: 8562 RVA: 0x00173696 File Offset: 0x00171896
		public NetLongDictionary()
		{
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x0017369E File Offset: 0x0017189E
		public NetLongDictionary(IEnumerable<KeyValuePair<long, T>> dict) : base(dict)
		{
		}

		// Token: 0x06002174 RID: 8564 RVA: 0x001736A7 File Offset: 0x001718A7
		protected override long ReadKey(BinaryReader reader)
		{
			return reader.ReadInt64();
		}

		// Token: 0x06002175 RID: 8565 RVA: 0x001736AF File Offset: 0x001718AF
		protected override void WriteKey(BinaryWriter writer, long key)
		{
			writer.Write(key);
		}
	}
}
