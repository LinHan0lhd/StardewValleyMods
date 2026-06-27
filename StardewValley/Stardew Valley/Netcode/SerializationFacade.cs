using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode
{
	// Token: 0x02000060 RID: 96
	public abstract class SerializationFacade<SerialT> : IEnumerable<!0>, IEnumerable
	{
		// Token: 0x06000408 RID: 1032
		protected abstract SerialT Serialize();

		// Token: 0x06000409 RID: 1033
		protected abstract void Deserialize(SerialT serialValue);

		// Token: 0x0600040A RID: 1034 RVA: 0x00012D7A File Offset: 0x00010F7A
		public IEnumerator<SerialT> GetEnumerator()
		{
			return Enumerable.Repeat<SerialT>(this.Serialize(), 1).GetEnumerator();
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00012D8D File Offset: 0x00010F8D
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00012D95 File Offset: 0x00010F95
		public void Add(SerialT value)
		{
			this.Deserialize(value);
		}
	}
}
