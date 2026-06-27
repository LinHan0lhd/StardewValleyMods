using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200004F RID: 79
	public class NetStringHashSet : NetHashSet<string>
	{
		// Token: 0x06000342 RID: 834 RVA: 0x000109EC File Offset: 0x0000EBEC
		public override string ReadValue(BinaryReader reader)
		{
			if (!reader.ReadBoolean())
			{
				return null;
			}
			return reader.ReadString();
		}

		// Token: 0x06000343 RID: 835 RVA: 0x000109FE File Offset: 0x0000EBFE
		public override void WriteValue(BinaryWriter writer, string value)
		{
			writer.Write(value != null);
			if (value != null)
			{
				writer.Write(value);
			}
		}
	}
}
