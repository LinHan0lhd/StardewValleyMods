using System;
using System.IO;

namespace Netcode
{
	// Token: 0x0200003C RID: 60
	public class NetEvent1Field<T, TField> : AbstractNetEvent1<T> where TField : NetField<!0, !1>, new()
	{
		// Token: 0x06000258 RID: 600 RVA: 0x0000E09E File Offset: 0x0000C29E
		protected override T readEventArg(BinaryReader reader, NetVersion version)
		{
			TField tfield = Activator.CreateInstance<TField>();
			tfield.ReadFull(reader, version);
			return tfield.Value;
		}

		// Token: 0x06000259 RID: 601 RVA: 0x0000E0BC File Offset: 0x0000C2BC
		protected override void writeEventArg(BinaryWriter writer, T eventArg)
		{
			TField tfield = Activator.CreateInstance<TField>();
			tfield.Value = eventArg;
			tfield.WriteFull(writer);
		}
	}
}
