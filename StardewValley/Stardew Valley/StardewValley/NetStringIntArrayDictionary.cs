using System;
using System.IO;
using System.Linq;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000A4 RID: 164
	public class NetStringIntArrayDictionary : NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>
	{
		// Token: 0x0600098F RID: 2447 RVA: 0x00065791 File Offset: 0x00063991
		protected override string ReadKey(BinaryReader reader)
		{
			return reader.ReadString();
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x00065799 File Offset: 0x00063999
		protected override void WriteKey(BinaryWriter writer, string key)
		{
			writer.Write(key);
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x000657A2 File Offset: 0x000639A2
		protected override void setFieldValue(NetArray<int, NetInt> field, string key, int[] value)
		{
			field.Set(value);
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x000657AB File Offset: 0x000639AB
		protected override int[] getFieldValue(NetArray<int, NetInt> field)
		{
			return field.ToArray<int>();
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x000657B3 File Offset: 0x000639B3
		protected override int[] getFieldTargetValue(NetArray<int, NetInt> field)
		{
			return field.ToArray<int>();
		}
	}
}
