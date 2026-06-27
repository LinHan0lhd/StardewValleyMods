using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001E2 RID: 482
	public class NetIntDictionary<T, TField> : NetFieldDictionary<int, T, TField, SerializableDictionary<int, T>, NetIntDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x0600215F RID: 8543 RVA: 0x001733C4 File Offset: 0x001715C4
		public NetIntDictionary()
		{
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x001733CC File Offset: 0x001715CC
		public NetIntDictionary(IEnumerable<KeyValuePair<int, T>> dict) : base(dict)
		{
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x001733D5 File Offset: 0x001715D5
		protected override int ReadKey(BinaryReader reader)
		{
			return reader.ReadInt32();
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x001733DD File Offset: 0x001715DD
		protected override void WriteKey(BinaryWriter writer, int key)
		{
			writer.Write(key);
		}
	}
}
