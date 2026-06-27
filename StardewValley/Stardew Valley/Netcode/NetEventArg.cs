using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200003A RID: 58
	public interface NetEventArg
	{
		// Token: 0x06000253 RID: 595
		void Read(BinaryReader reader);

		// Token: 0x06000254 RID: 596
		void Write(BinaryWriter writer);
	}
}
