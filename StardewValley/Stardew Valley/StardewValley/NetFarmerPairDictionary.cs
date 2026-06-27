using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000AD RID: 173
	public class NetFarmerPairDictionary<T, TField> : NetFieldDictionary<FarmerPair, T, TField, SerializableDictionary<FarmerPair, T>, NetFarmerPairDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x060009F3 RID: 2547 RVA: 0x0006C405 File Offset: 0x0006A605
		public NetFarmerPairDictionary()
		{
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0006C40D File Offset: 0x0006A60D
		public NetFarmerPairDictionary(IEnumerable<KeyValuePair<FarmerPair, T>> dict) : base(dict)
		{
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0006C418 File Offset: 0x0006A618
		protected override FarmerPair ReadKey(BinaryReader reader)
		{
			long f = reader.ReadInt64();
			long farmer2 = reader.ReadInt64();
			return FarmerPair.MakePair(f, farmer2);
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x0006C438 File Offset: 0x0006A638
		protected override void WriteKey(BinaryWriter writer, FarmerPair key)
		{
			writer.Write(key.Farmer1);
			writer.Write(key.Farmer2);
		}
	}
}
