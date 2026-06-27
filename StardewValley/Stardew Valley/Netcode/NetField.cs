using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode
{
	// Token: 0x0200003E RID: 62
	public abstract class NetField<T, TSelf> : NetFieldBase<T, TSelf>, IEnumerable<!0>, IEnumerable where TSelf : NetField<!0, !1>
	{
		// Token: 0x06000287 RID: 647 RVA: 0x0000E6CD File Offset: 0x0000C8CD
		public NetField()
		{
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0000E6D5 File Offset: 0x0000C8D5
		public NetField(T value) : base(value)
		{
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0000E6DE File Offset: 0x0000C8DE
		public IEnumerator<T> GetEnumerator()
		{
			return Enumerable.Repeat<T>(base.Get(), 1).GetEnumerator();
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0000E6F1 File Offset: 0x0000C8F1
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000E6FC File Offset: 0x0000C8FC
		public void Add(T value)
		{
			if (this.xmlInitialized || base.Parent != null)
			{
				throw new InvalidOperationException(base.GetType().Name + " already has value " + this.ToString());
			}
			base.cleanSet(value);
			this.xmlInitialized = true;
		}

		// Token: 0x0400016D RID: 365
		private bool xmlInitialized;
	}
}
