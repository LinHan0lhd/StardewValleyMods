using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200003B RID: 59
	public class NetEvent1<T> : AbstractNetEvent1<T> where T : NetEventArg, new()
	{
		// Token: 0x06000255 RID: 597 RVA: 0x0000E064 File Offset: 0x0000C264
		protected override T readEventArg(BinaryReader reader, NetVersion version)
		{
			T arg = Activator.CreateInstance<T>();
			arg.Read(reader);
			return arg;
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000E086 File Offset: 0x0000C286
		protected override void writeEventArg(BinaryWriter writer, T eventArg)
		{
			eventArg.Write(writer);
		}
	}
}
