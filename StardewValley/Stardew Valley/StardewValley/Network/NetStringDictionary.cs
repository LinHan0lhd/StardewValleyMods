using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001EB RID: 491
	public class NetStringDictionary<T, TField> : NetFieldDictionary<string, T, TField, SerializableDictionary<string, T>, NetStringDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x060021B0 RID: 8624 RVA: 0x00174177 File Offset: 0x00172377
		public NetStringDictionary()
		{
		}

		// Token: 0x060021B1 RID: 8625 RVA: 0x0017417F File Offset: 0x0017237F
		public NetStringDictionary(IEnumerable<KeyValuePair<string, T>> dict) : base(dict)
		{
		}

		// Token: 0x060021B2 RID: 8626 RVA: 0x00174188 File Offset: 0x00172388
		protected override string ReadKey(BinaryReader reader)
		{
			return reader.ReadString();
		}

		// Token: 0x060021B3 RID: 8627 RVA: 0x00174190 File Offset: 0x00172390
		protected override void WriteKey(BinaryWriter writer, string key)
		{
			writer.Write(key);
		}
	}
}
