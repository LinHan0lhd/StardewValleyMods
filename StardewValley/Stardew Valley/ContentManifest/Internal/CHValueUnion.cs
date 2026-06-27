using System;
using System.Runtime.InteropServices;

namespace ContentManifest.Internal
{
	// Token: 0x0200007E RID: 126
	[StructLayout(LayoutKind.Explicit)]
	internal struct CHValueUnion
	{
		// Token: 0x040001BA RID: 442
		[FieldOffset(0)]
		public CHObject ValueObject;

		// Token: 0x040001BB RID: 443
		[FieldOffset(0)]
		public CHArray ValueArray;

		// Token: 0x040001BC RID: 444
		[FieldOffset(0)]
		public CHString ValueString;

		// Token: 0x040001BD RID: 445
		[FieldOffset(0)]
		public CHNumber ValueNumber;

		// Token: 0x040001BE RID: 446
		[FieldOffset(0)]
		public CHBoolean ValueBoolean;

		// Token: 0x040001BF RID: 447
		[FieldOffset(0)]
		public object ValueNull;
	}
}
