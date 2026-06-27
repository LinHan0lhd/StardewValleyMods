using System;
using System.Collections;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000061 RID: 97
	public abstract class SerializationCollectionFacade<SerialT> : IEnumerable<!0>, IEnumerable
	{
		// Token: 0x0600040E RID: 1038
		protected abstract List<SerialT> Serialize();

		// Token: 0x0600040F RID: 1039
		protected abstract void DeserializeAdd(SerialT serialElem);

		// Token: 0x06000410 RID: 1040 RVA: 0x00012DA6 File Offset: 0x00010FA6
		public IEnumerator<SerialT> GetEnumerator()
		{
			return this.Serialize().GetEnumerator();
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00012DB8 File Offset: 0x00010FB8
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00012DC0 File Offset: 0x00010FC0
		public void Add(SerialT value)
		{
			this.DeserializeAdd(value);
		}
	}
}
