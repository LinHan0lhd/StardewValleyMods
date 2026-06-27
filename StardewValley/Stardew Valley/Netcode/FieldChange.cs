using System;

namespace Netcode
{
	// Token: 0x02000028 RID: 40
	// (Invoke) Token: 0x06000139 RID: 313
	public delegate void FieldChange<in TSelf, in TValue>(TSelf field, TValue oldValue, TValue newValue);
}
