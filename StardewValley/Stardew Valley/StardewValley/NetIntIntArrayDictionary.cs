using System;
using System.IO;
using System.Linq;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000A5 RID: 165
	public class NetIntIntArrayDictionary : NetDictionary<int, int[], NetArray<int, NetInt>, SerializableDictionary<int, int[]>, NetIntIntArrayDictionary>
	{
		// Token: 0x06000995 RID: 2453 RVA: 0x000657C3 File Offset: 0x000639C3
		protected override int ReadKey(BinaryReader reader)
		{
			return reader.ReadInt32();
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x000657CB File Offset: 0x000639CB
		protected override void WriteKey(BinaryWriter writer, int key)
		{
			writer.Write(key);
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x000657D4 File Offset: 0x000639D4
		protected override void setFieldValue(NetArray<int, NetInt> field, int key, int[] value)
		{
			field.Set(value);
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x000657DD File Offset: 0x000639DD
		protected override int[] getFieldValue(NetArray<int, NetInt> field)
		{
			return field.ToArray<int>();
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x000657E5 File Offset: 0x000639E5
		protected override int[] getFieldTargetValue(NetArray<int, NetInt> field)
		{
			return field.ToArray<int>();
		}
	}
}
