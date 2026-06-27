using System;
using System.Xml.Serialization;

namespace StardewValley.Util
{
	// Token: 0x02000119 RID: 281
	public struct SaveablePair<TKey, TValue>
	{
		// Token: 0x170002AC RID: 684
		// (get) Token: 0x060017B5 RID: 6069 RVA: 0x00111A9B File Offset: 0x0010FC9B
		[XmlIgnore]
		public TKey Key
		{
			get
			{
				return this.key[0];
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x060017B6 RID: 6070 RVA: 0x00111AA9 File Offset: 0x0010FCA9
		[XmlIgnore]
		public TValue Value
		{
			get
			{
				return this.value[0];
			}
		}

		// Token: 0x060017B7 RID: 6071 RVA: 0x00111AB7 File Offset: 0x0010FCB7
		public SaveablePair(TKey key, TValue value)
		{
			this.key = new TKey[]
			{
				key
			};
			this.value = new TValue[]
			{
				value
			};
		}

		// Token: 0x04000E47 RID: 3655
		public TKey[] key;

		// Token: 0x04000E48 RID: 3656
		public TValue[] value;
	}
}
