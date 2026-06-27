using System;
using System.Collections.Generic;

namespace Netcode
{
	// Token: 0x02000033 RID: 51
	public abstract class NetFieldDictionary<TKey, TValue, TField, TSerialDict, TSelf> : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> where TField : NetField<!1, !2>, new() where TSerialDict : IDictionary<!0, !1>, new() where TSelf : NetDictionary<!0, !1, !2, !3, !4>
	{
		// Token: 0x0600021C RID: 540 RVA: 0x0000D7D8 File Offset: 0x0000B9D8
		public NetFieldDictionary()
		{
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0000D7E0 File Offset: 0x0000B9E0
		public NetFieldDictionary(IEnumerable<KeyValuePair<TKey, TValue>> pairs) : base(pairs)
		{
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0000D7E9 File Offset: 0x0000B9E9
		protected override void setFieldValue(TField field, TKey key, TValue value)
		{
			field.Value = value;
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000D7F7 File Offset: 0x0000B9F7
		protected override TValue getFieldValue(TField field)
		{
			return field.Value;
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0000D804 File Offset: 0x0000BA04
		protected override TValue getFieldTargetValue(TField field)
		{
			return field.TargetValue;
		}
	}
}
