using System;
using System.IO;

namespace Netcode
{
	// Token: 0x02000041 RID: 65
	public sealed class NetLong : NetField<long, NetLong>
	{
		// Token: 0x0600029D RID: 669 RVA: 0x0000E93C File Offset: 0x0000CB3C
		public NetLong()
		{
		}

		// Token: 0x0600029E RID: 670 RVA: 0x0000E944 File Offset: 0x0000CB44
		public NetLong(long value) : base(value)
		{
		}

		// Token: 0x0600029F RID: 671 RVA: 0x0000E94D File Offset: 0x0000CB4D
		public override void Set(long newValue)
		{
			if (base.canShortcutSet())
			{
				this.value = newValue;
				return;
			}
			if (newValue != this.value)
			{
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0000E975 File Offset: 0x0000CB75
		protected override long interpolate(long startValue, long endValue, float factor)
		{
			return startValue + (long)((float)(endValue - startValue) * factor);
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000E980 File Offset: 0x0000CB80
		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			long newValue = reader.ReadInt64();
			if (version.IsPriorityOver(this.ChangeVersion))
			{
				base.setInterpolationTarget(newValue);
			}
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000E9AA File Offset: 0x0000CBAA
		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(this.value);
		}
	}
}
