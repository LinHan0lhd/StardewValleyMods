using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x02000034 RID: 52
	public class NetGuidDictionary<T, TField> : NetFieldDictionary<Guid, T, TField, Dictionary<Guid, T>, NetGuidDictionary<T, TField>> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x06000221 RID: 545 RVA: 0x0000D811 File Offset: 0x0000BA11
		public NetGuidDictionary()
		{
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000D819 File Offset: 0x0000BA19
		public NetGuidDictionary(IEnumerable<KeyValuePair<Guid, T>> pairs) : base(pairs)
		{
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000D822 File Offset: 0x0000BA22
		protected override Guid ReadKey(BinaryReader reader)
		{
			return reader.ReadGuid();
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000D82A File Offset: 0x0000BA2A
		protected override void WriteKey(BinaryWriter writer, Guid key)
		{
			writer.WriteGuid(key);
		}
	}
}
